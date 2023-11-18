using GameServer.Core.Executor;
using GameServer.KiemThe.Entities;
using GameServer.Logic;
using Server.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GameServer.KiemThe.Logic
{
    /// <summary>
    /// Quản lý đạn bay
    /// </summary>
    public partial class KTBulletManager
    {
        #region Define
        /// <summary>
        /// Đối tượng kết quả tạo đạn
        /// </summary>
        public class CreateBulletFlyResult
        {
            /// <summary>
            /// ID đạn
            /// </summary>
            public int BulletID { get; set; }

            /// <summary>
            /// Vị trí đích bay tới
            /// <para>Nếu không phải đạn bay thì không có thuộc tính này</para>
            /// </summary>
            public UnityEngine.Vector2 ToPos { get; set; }
        }

        /// <summary>
        /// Đối tượng đạn bay
        /// </summary>
        private class Bullet
        {
            /// <summary>
            /// ID tự động tăng của đối tượng
            /// </summary>
            public static int AutoID = 0;

            /// <summary>
            /// ID đối tượng
            /// </summary>
            public int ID { get; private set; }

            /// <summary>
            /// Đối tượng xuất chiêu
            /// </summary>
            public GameObject Caster { get; set; }

            /// <summary>
            /// Kỹ năng
            /// </summary>
            public SkillLevelRef Skill { get; set; }

            /// <summary>
            /// Vector chỉ hướng xuất chiêu
            /// </summary>
            public UnityEngine.Vector2? DirVectorProperty { get; set; }

            /// <summary>
            /// Vị trí bắt đầu
            /// </summary>
            public UnityEngine.Vector2 FromPos { get; set; }

            /// <summary>
            /// Vị trí kết thúc
            /// <para>Nếu IsTrap = true thì không có tác dụng</para>
            /// <para>Nếu ChaseTarget != null thì không có tác dụng</para>
            /// </summary>
            public UnityEngine.Vector2 ToPos { get; set; }

            /// <summary>
            /// Mục tiêu đuổi theo
            /// <para>Nếu IsTrap = true thì không có tác dụng</para>
            /// </summary>
            public GameObject ChaseTarget { get; set; }

            /// <summary>
            /// Danh sách mục tiêu đã chạm vào
            /// </summary>
            public HashSet<GameObject> TouchTargets { get; set; }

            /// <summary>
            /// Di chuyển theo đường tròn
            /// </summary>
            public bool CircleMove { get; set; } = false;

            /// <summary>
            /// Thiết lập có phải bẫy không
            /// <para>Nếu là bẫy thì đứng yên một chỗ, là ở vị trí FromPos</para>
            /// <para>Các giá trị ToPos, ChaseTarget không có tác dụng</para>
            /// </summary>
            public bool IsTrap { get; set; }

            /// <summary>
            /// Thời gian tồn tại tối đa
            /// </summary>
            public float MaxLifeTime { get; set; }

            /// <summary>
            /// Vận tốc
            /// </summary>
            public float Velocity { get; set; }

            /// <summary>
            /// Số mục tiêu tối đa có thể va phải
            /// </summary>
            public int MaxTargetTouch { get; set; }

            /// <summary>
            /// Loại mục tiêu
            /// </summary>
            public string TargetType { get; set; }

            /// <summary>
            /// Số lần xuyên suốt mục tiêu
            /// <para>0 nghĩa là không xuyên</para>
            /// </summary>
            public bool PirceThroughTargets { get; set; }

            /// <summary>
            /// Số lần đã xuyên suốt mục tiêu
            /// </summary>
            public int PieceThroughTargetsCount { get; set; }

            /// <summary>
            /// Có thể nổ nhiều lần lên cùng 1 mục tiêu (áp dụng cho loại đạn xuyên suốt mục tiêu)
            /// </summary>
            public bool MultipleTouchTarget { get; set; } = false;

            /// <summary>
            /// Tạo mới đối tượng đạn bay
            /// </summary>
            public Bullet()
            {
                Bullet.AutoID = (Bullet.AutoID + 1) % 1000000007;
                this.ID = Bullet.AutoID;
                this.TouchTargets = new HashSet<GameObject>();
            }
        }

        /// <summary>
        /// Lớp mô tả thông tin đạn nổ
        /// </summary>
        private class BulletExplode
        {
            /// <summary>
            /// Mục tiêu
            /// </summary>
            public GameObject Target { get; set; }

            /// <summary>
            /// Vị trí nổ
            /// </summary>
            public UnityEngine.Vector2 ExplodePos { get; set; }

            /// <summary>
            /// Giời gian đếm lui để nổ
            /// </summary>
            public float Time { get; set; }
        }

        /// <summary>
        /// Timer của đạn
        /// </summary>
        private class BulletTimer
		{
            /// <summary>
            /// Ngừng Timer khi gặp ngoại lệ
            /// </summary>
            public bool StopWhenExceptionOccurs { get; set; } = true;

            /// <summary>
			/// Thời gian Tick kiểm tra
			/// </summary>
			public int PeriodTicks { get; set; }

            /// <summary>
            /// Thời gian bắt đầu thực thi
            /// </summary>
            public long InitTicks { get; private set; } = 0;

            /// <summary>
            /// Thời gian tồn tại
            /// </summary>
            public long LifeTime { get; set; }

            /// <summary>
            /// Thời gian tồn tại tối đa
            /// </summary>
            public int MaxLifeTime { get; set; }

            /// <summary>
            /// Có còn sống không
            /// </summary>
            public bool Alive { get; set; } = true;

            /// <summary>
            /// Đã bắt đầu chưa
            /// </summary>
            public bool IsStarted { get; set; }

            /// <summary>
            /// Sự kiện bắt đầu
            /// </summary>
            public Action Start { get; set; }

            /// <summary>
            /// Sự kiện Tick
            /// </summary>
            public Action Tick { get; set; }

            /// <summary>
            /// Sự kiện hoàn tất
            /// </summary>
            public Action Finish { get; set; }

            /// <summary>
            /// Sự kiện hủy bỏ
            /// </summary>
            public Action Destroy { get; set; }

            /// <summary>
            /// Sự kiện lỗi
            /// </summary>
            public Action<Exception> Error { get; set; }

            /// <summary>
            /// Thời điểm Tick lần trước
            /// </summary>
            public long LastTick { get; set; }

            /// <summary>
            /// Đã đến thời điểm Tick chưa
            /// </summary>
            public bool IsTickTime
            {
                get
                {
                    return KTGlobal.GetCurrentTimeMilis() - this.LastTick >= this.PeriodTicks;
                }
            }

            /// <summary>
            /// Đã quá thời gian chưa
            /// </summary>
            public bool IsOver
            {
                get
                {
                    /// Nếu chưa bắt đầu
                    if (this.InitTicks == 0)
                    {
                        return false;
                    }

                    /// Nếu tồn tại quá 30s thì tự hủy luôn
                    if (KTGlobal.GetCurrentTimeMilis() - this.InitTicks >= 30000)
					{
                        return true;
					}

                    /// Xem đã hết thời gian chưa
                    return this.LifeTime >= this.MaxLifeTime;
                }
            }

            /// <summary>
            /// Đối tượng đạn bay
            /// </summary>
            public Bullet Bullet { get; set; }

            /// <summary>
            /// Vị trí hiện tại của đạn
            /// </summary>
            public UnityEngine.Vector2 CurrentPos { get; set; }

            /// <summary>
            /// Số mục tiêu đã chạm
            /// </summary>
            public int TotalTouchTarget { get; set; }

            /// <summary>
            /// Hướng bay lần trước
            /// </summary>
            public UnityEngine.Vector2? LastDirVector { get; set; } = null;

            /// <summary>
            /// Mục tiêu chạm khi tan biến
            /// </summary>
            public GameObject VanishTarget { get; set; } = null;

            /// <summary>
            /// Vị trí thực hiện hàm Tick lần trước
            /// </summary>
            public UnityEngine.Vector2 LastTickPos { get; set; }

            /// <summary>
            /// Thực thi sự kiện
            /// </summary>
            public void Run()
            {
                /// Thiết lập thời điểm bắt đầu
                this.InitTicks = KTGlobal.GetCurrentTimeMilis();
                /// Thêm vào Timer
                KTBulletManager.Instance.AddBulletTimer(this, this.Bullet.Caster);
            }

            /// <summary>
            /// Ngừng Timer
            /// </summary>
            public void Stop()
            {
                this.Alive = false;
            }
        }
        #endregion

        #region Constants
        /// <summary>
        /// Thời gian tồn tại tối đa của đạn
        /// </summary>
        private const float DefaultBulletLifeTime = 5f;

        /// <summary>
        /// Thời gian tick kiểm tra vị trí đạn
        /// </summary>
        private const float DefaultBulletTick = 0.2f;

        /// <summary>
        /// Thời gian Tick kiểm tra bẫy
        /// </summary>
        public const float DefaultTrapTick = 0.2f;

        /// <summary>
        /// Thời gian tick kiểm tra vị trí đạn đuổi mục tiêu
        /// </summary>
        private const float DefaultBulletChaseTargetTick = 0.2f;
        #endregion

        #region Core
        /// <summary>
        /// Thực thi sự kiện sau khoảng thời gian tương ứng
        /// </summary>
        /// <param name="delaySec"></param>
        /// <param name="work"></param>
        /// <param name="caster"></param>
        private void SetTimeout(float delaySec, Action work, GameObject caster)
        {
            this.AddDelayTask(new DelayTask()
            {
                InitTicks = KTGlobal.GetCurrentTimeMilis(),
                DelayTicks = (int) (delaySec * 1000),
                Work = work,
            }, caster);
        }

        /// <summary>
        /// Thêm đạn nổ vào danh sách
        /// </summary>
        /// <param name="bulletID"></param>
        /// <param name="resID"></param>
        /// <param name="pos"></param>
        /// <param name="delay"></param>
        /// <param name="target"></param>
        private void AppendBulletExplode(Bullet bullet, int resID, UnityEngine.Vector2 pos, float delay, GameObject target)
        {
            if (bullet == null)
            {
                return;
            }

            /// Nếu quá gần thời điểm nổ lần trước
            if (target != null && KTGlobal.GetCurrentTimeMilis() - target.LastPlayBulletExplodeEffectTicks < 1000)
            {
                return;
            }

            /// Gửi gói tin nổ về Client
            KT_TCPHandler.SendBulletExplode(bullet.Caster, bullet.ID, resID, pos, delay, target);
        }
        #endregion
    }
}
