using GameServer.KiemThe.Entities;
using GameServer.KiemThe.Logic;
using Server.Data;
using System;
using System.Collections.Generic;
using System.Windows;
using static GameServer.Logic.KTMapManager;

namespace GameServer.Logic
{
    /// <summary>
    /// Thuộc tính
    /// </summary>
    public partial class Monster
    {
        /// <summary>
        /// Tỷ lệ % để thực hiện di chuyển
        /// </summary>
        private const int AIRandomMoveBelowRate = 50;

        /// <summary>
        /// ID khu vực quái quản lý (nếu là quái ở map thường được sinh ra bởi hệ thống khi tạo bản đồ)
        /// </summary>
        public int MonsterZoneID { get; set; } = -1;

        /// <summary>
        /// Template quái
        /// </summary>
        public KTMonsterManager.MonsterTemplateData MonsterInfo
        {
            get;
            set;
        }

        private string _RoleName = null;
        /// <summary>
        /// Tên quái
        /// </summary>
        public override string RoleName
        {
            get
            {
                return this._RoleName;
            }
            set
            {
                string oldValue = this._RoleName;
                this._RoleName = value;

                if (oldValue != null)
                {
                    /// Thông báo tên thay đổi
                    KT_TCPHandler.NotifyOthersMyNameChanged(this);
                }
            }
        }

        private string _Title = null;
        /// <summary>
        /// Danh hiệu của đối tượng
        /// </summary>
        public override string Title
        {
            get
            {
                return this._Title;
            }
            set
            {
                string oldValue = this._Title;
                this._Title = value;

                if (oldValue != null)
                {
                    /// Thông báo về Client
                    KT_TCPHandler.NotifyOthersMyTitleChanged(this);
                }
            }
        }

        /// <summary>
        /// Số lượng người chơi xung quanh
        /// </summary>
        public int VisibleClientsNum { get; set; }

        /// <summary>
        /// Sự kiện khi quái chết
        /// </summary>
        public Action<GameObject> OnDieCallback { get; set; }

        /// <summary>
        /// ID Script AI điều khiển
        /// </summary>
        public int AIID { get; set; } = -1;

        /// <summary>
        /// Tag của đối tượng
        /// </summary>
        public string Tag { get; set; } = "";

        /// <summary>
        /// Phạm vi đuổi mục tiêu
        /// </summary>
        public int SeekRange { get; set; }

        /// <summary>
        /// Loại quái
        /// </summary>
        public MonsterAIType MonsterType { get; set; }

        /// <summary>
        /// Quái tĩnh, không di chuyển
        /// </summary>
        public bool IsStatic { get; set; } = false;

        /// <summary>
        /// Tìm đường sửa dụng thuật toán A*
        /// </summary>
        public bool UseAStarPathFinder { get; set; } = true;

        /// <summary>
        /// Sự kiện Tick
        /// </summary>
        public Action OnTick { get; set; } = null;

        /// <summary>
        /// Sự kiện khi đối tượng bị tấn công
        /// </summary>
        public Action<GameObject, int> OnTakeDamage { get; set; } = null;

        /// <summary>
        /// Thời gian tái sinh (nếu là quái di động)
        /// </summary>
        public long DynamicRespawnTicks { get; set; }

        /// <summary>
        /// Điều kiện tái sinh nếu là quái di động
        /// </summary>
        public Func<bool> DynamicRespawnPredicate { get; set; }

        /// <summary>
        /// Thời gian chết lần trước
        /// </summary>
        public long LastDeadTicks { get; private set; }

        /// <summary>
        /// Đối tượng có đang di chuyển không
        /// </summary>
        public bool IsMoving
        {
            get
            {
                return this.m_eDoing == KE_NPC_DOING.do_walk || this.m_eDoing == KE_NPC_DOING.do_run;
            }
        }

        /// <summary>
        /// Danh sách kỹ năng tự chọn
        /// <para>Nếu AIScript là mặc định thì không có tác dụng</para>
        /// </summary>
        public List<SkillLevelRef> CustomAISkills { get; private set; } = new List<SkillLevelRef>();

        /// <summary>
        /// Vị trí ban đầu
        /// </summary>
        public Point StartPos { get; set; }

        /// <summary>
        /// Vị trí khởi tạo ban đầu
        /// </summary>
        public Point InitializePos { get; set; }

        /// <summary>
        /// Thời điểm bị tấn công lần trước
        /// </summary>
        public long LastBeHitTicks { get; private set; } = 0;

        /// <summary>
        /// Có đang đuổi mục tiêu không
        /// </summary>
        public bool IsChasingTarget
        {
            get
            {
                return this.chaseTarget != null;
            }
        }

        #region Kế thừa - IObject
        /// <summary>
        /// Loại đối tượng
        /// </summary>
        public override ObjectTypes ObjectType
        {
            get { return ObjectTypes.OT_MONSTER; }
        }

        /// <summary>
        /// Vị trí hiện tại theo tọa độ lưới
        /// </summary>
        public override Point CurrentGrid
        {
            get
            {
                GameMap gameMap = KTMapManager.Find(this.MapCode);
                return new Point((int) (this.CurrentPos.X / gameMap.MapGridWidth), (int) (this.CurrentPos.Y / gameMap.MapGridHeight));
            }

            set
            {
                GameMap gameMap = KTMapManager.Find(this.MapCode);
                this.CurrentPos = new Point(value.X * gameMap.MapGridWidth + gameMap.MapGridWidth / 2, value.Y * gameMap.MapGridHeight + gameMap.MapGridHeight / 2);
            }
        }

        /// <summary>
        /// ID bản đồ hiện tại
        /// </summary>
        public int MapCode { get; set; }
        /// <summary>
        /// ID bản đồ hiện tại
        /// </summary>
        public override int CurrentMapCode
        {
            get
            {
                return this.MapCode;
            }
        }

        /// <summary>
        /// ID phụ bản hiện tại
        /// </summary>
        public override int CurrentCopyMapID { get; set; } = -1;
        #endregion
    }
}
