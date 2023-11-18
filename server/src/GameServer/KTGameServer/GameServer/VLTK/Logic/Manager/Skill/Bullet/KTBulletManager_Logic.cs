using GameServer.KiemThe.Entities;
using GameServer.KiemThe.Utilities;
using GameServer.Logic;
using Server.Tools;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GameServer.KiemThe.Logic
{
    /// <summary>
    /// Quản lý đạn bay
    /// </summary>
    public partial class KTBulletManager
    {
        /// <summary>
        /// Thực hiện biểu diễn đạn bay
        /// </summary>
        /// <param name="bulletTimer">Đạn</param>
        /// <param name="fromPos">Vị trí bắt đầu</param>
        /// <param name="toPos">Vị trí kết thúc</param>
        /// <param name="explodeRadius">Phạm vi nổ</param>
        /// <param name="maxTargetTouch">Số mục tiêu chạm phải tối đa</param>
        /// <param name="stopPos">Vị trí đích dừng lại (áp dụng với bước đầu tiên chạy tương đương hàm Start của Timer)</param>
        private void DoBulletFlyLogic(BulletTimer bulletTimer, UnityEngine.Vector2 fromPos, UnityEngine.Vector2 toPos, int explodeRadius, int maxTargetTouch, out UnityEngine.Vector2? stopPos)
        {
            stopPos = null;
            if (bulletTimer == null || bulletTimer.Bullet == null || bulletTimer.Bullet.Skill == null || bulletTimer.Bullet.Skill.Properties == null)
            {
                return;
            }

            try
			{
                /// Đối tượng người chơi
                KPlayer player = bulletTimer.Bullet.Caster as KPlayer;
                /// ProDict kỹ năng hỗ trợ kỹ năng này
                PropertyDictionary enchantPd = player == null ? null : player.Skills.GetEnchantProperties(bulletTimer.Bullet.Skill.SkillID);

                /// Phạm vi nổ cộng thêm mỗi lần xuyên suốt mục tiêu
                int pieceTargetExplodeRadius = 0;
                int pieceTargetMaxExplodeRadius = 0;
                /// Nếu có thiết lập phạm vi nổ cộng thêm mỗi lần xuyên suốt mục tiêu
                if (bulletTimer.Bullet.Skill.Properties.ContainsKey((int) MAGIC_ATTRIB.magic_skilladdition_addrangewhencol))
                {
                    pieceTargetExplodeRadius = bulletTimer.Bullet.Skill.Properties.Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_skilladdition_addrangewhencol).nValue[1] * 10;
                    pieceTargetMaxExplodeRadius = bulletTimer.Bullet.Skill.Properties.Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_skilladdition_addrangewhencol).nValue[2] * 10;
                }
                /// Nếu có kỹ năng hỗ trợ tăng phạm vi nổ cộng thêm mỗi lần xuyên suốt mục tiêu
                if (enchantPd != null && enchantPd.ContainsKey((int) MAGIC_ATTRIB.magic_skilladdition_addrangewhencol))
                {
                    pieceTargetExplodeRadius += enchantPd.Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_skilladdition_addrangewhencol).nValue[1] * 10;
                    pieceTargetMaxExplodeRadius += enchantPd.Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_skilladdition_addrangewhencol).nValue[2] * 10;
                }

                /// Tính phạm vi nổ của đạn
                explodeRadius += Math.Min(pieceTargetExplodeRadius * bulletTimer.Bullet.TouchTargets.Count, pieceTargetMaxExplodeRadius);

                /// Thiết lập chưa có vị trí đích
                stopPos = null;
                /// Tìm danh sách đối tượng nằm trong vùng ảnh hưởng của đạn
                List<BulletExplode> list = this.GetExplosionTargets(bulletTimer.Bullet, fromPos, toPos, explodeRadius, bulletTimer.Bullet.MultipleTouchTarget);
                if (list.Count <= 0)
                {
                    return;
                }
                //Console.WriteLine("List targets = " + list.Count);

                /// Vị trí nổ lần trước
                UnityEngine.Vector2 lastExplodePos = UnityEngine.Vector2.zero;

                /// Có phải mục tiêu đầu tiên không
                bool isFirstTarget = true;

                int index = 0;
                /// Duyệt danh sách đối tượng tìm được và tiến hành sự kiện đạn phát nổ
                foreach (BulletExplode pair in list)
                {
                    /// Tăng ID đối tượng lên
                    index++;

                    /// Tăng số mục tiêu đã va phải
                    bulletTimer.TotalTouchTarget++;

                    /// Vị trí đối tượng hiện tại
                    UnityEngine.Vector2 targetPos = new UnityEngine.Vector2((float) pair.Target.CurrentPos.X, (float) pair.Target.CurrentPos.Y);

                    /// Thực hiện sự kiện chạm mục tiêu lần đầu
                    if (isFirstTarget)
                    {
                        KTSkillManager.BulletFirstTouchTarget(bulletTimer.Bullet.Skill, bulletTimer.Bullet.Caster, pair.ExplodePos, pair.Target, toPos - fromPos);
                    }

                    /// Thực hiện gọi hàm OnExplode
                    this.SetTimeout(pair.Time, () => {
                        KTSkillManager.BulletExplode(bulletTimer.Bullet.Skill, bulletTimer.Bullet.Caster, null, pair.Target, bulletTimer.Bullet.DirVectorProperty, bulletTimer.Bullet.PirceThroughTargets ? bulletTimer.Bullet.TouchTargets.Count : 0, bulletTimer.Bullet.TouchTargets.Count);
                    }, bulletTimer.Bullet.Caster);

                    /// Thêm đạn nổ vào gói tin
                    this.AppendBulletExplode(bulletTimer.Bullet, bulletTimer.Bullet.Skill.Data.BulletID, pair.ExplodePos, pair.Time, pair.Target);

                    /// Thêm mục tiêu đã chạm phải vào danh sách
                    bulletTimer.Bullet.TouchTargets.Add(pair.Target);

                    /// Nếu là mục tiêu đầu tiên thì đánh dấu không cho bọn đằng sau lặp lại nữa
                    if (isFirstTarget)
                    {
                        isFirstTarget = false;
                    }

                    /// Nếu không xuyên mục tiêu
                    if (!bulletTimer.Bullet.PirceThroughTargets)
                    {
                        stopPos = pair.ExplodePos;
                        bulletTimer.VanishTarget = pair.Target;
                        bulletTimer.Stop();
                        break;
                    }

                    /// Tăng số lần đã xuyên suốt mục tiêu
                    bulletTimer.Bullet.PieceThroughTargetsCount++;

                    /// Nếu số mục tiêu chạm phải vượt quá tổng số mục tiêu có thể chạm
                    if (bulletTimer.TotalTouchTarget >= maxTargetTouch)
                    {
                        stopPos = pair.ExplodePos;
                        bulletTimer.VanishTarget = pair.Target;
                        bulletTimer.Stop();
                        break;
                    }

                    /// Nếu có mục tiêu đuổi theo và đã tiếp cận mục tiêu
                    if (bulletTimer.Bullet.ChaseTarget == pair.Target)
                    {
                        stopPos = pair.ExplodePos;
                        bulletTimer.VanishTarget = pair.Target;
                        bulletTimer.Stop();
                        break;
                    }
                }

                /// Tính phạm vi nổ của đạn
                explodeRadius += Math.Min(pieceTargetExplodeRadius * bulletTimer.Bullet.TouchTargets.Count, pieceTargetMaxExplodeRadius);

                /// Mục tiêu cuối cùng dính đạn nổ
                BulletExplode lastExplodeTargetInfo = list[index - 1];
                /// Tọa độ mục tiêu cuối cùng dính đạn
                lastExplodePos = new UnityEngine.Vector2((float) lastExplodeTargetInfo.Target.CurrentPos.X, (float) lastExplodeTargetInfo.Target.CurrentPos.Y);

                /// Lúc này xem còn thằng nào đứng xung quanh mà chưa nằm trong List mà vẫn nằm trong phạm vi nổ của đạn thì cho nổ viên đạn cuối
                for (int i = index; i < list.Count; i++)
                {
                    BulletExplode pair = list[i];
                    /// Vị trí đối tượng hiện tại
                    UnityEngine.Vector2 targetPos = new UnityEngine.Vector2((float) pair.Target.CurrentPos.X, (float) pair.Target.CurrentPos.Y);

                    /// Nếu vẫn nằm trong phạm vi nổ
                    if (UnityEngine.Vector2.Distance(lastExplodePos, targetPos) <= explodeRadius)
                    {
                        /// Thiết lập thời gian nổ trùng với lần gần nhất đạn nổ (tạo hiệu ứng lan truyền)
                        this.SetTimeout(lastExplodeTargetInfo.Time, () => {
                            KTSkillManager.BulletExplode(bulletTimer.Bullet.Skill, bulletTimer.Bullet.Caster, null, pair.Target, bulletTimer.Bullet.DirVectorProperty, bulletTimer.Bullet.PirceThroughTargets ? bulletTimer.Bullet.TouchTargets.Count : 0, bulletTimer.Bullet.TouchTargets.Count);
                        }, bulletTimer.Bullet.Caster);
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Skill, ex.ToString());
                /// Xóa luồng thực thi
                bulletTimer.Stop();
			}
        }

        /// <summary>
        /// Thực hiện biểu diễn đạn nổ tại vị trí chỉ định
        /// </summary>
        /// <param name="bullet">Đạn</param>
        /// <param name="pos">Vị trí</param>
        /// <param name="maxTargetTouch">Số mục tiêu ảnh hưởng tối đa</param>
        /// <param name="explodeRadius">Phạm vi nổ</param>
        /// <param name="destroyOnMaxTargetTouch">Xóa đạn nếu chạm quá mục tiêu</param>
        /// <param name="vanishTarget">Mục tiêu chạm lần cuối trước khi tan biến</param>
        private void DoStaticBulletLogic(Bullet bullet, UnityEngine.Vector2 pos, int maxTargetTouch, int explodeRadius, bool destroyOnMaxTargetTouch, out GameObject vanishTarget)
        {
            /// Đánh dấu chưa có mục tiêu chạm lần cuối
            vanishTarget = null;

            /// Toác gì đó
            if (bullet == null)
            {
                return;
            }

            try
            {
                List<GameObject> list = this.GetExplosionTargets(bullet, pos, explodeRadius, bullet.MultipleTouchTarget);
                if (list.Count <= 0)
                {
                    return;
                }

                /// Khởi tạo số mục tiêu đã nổ trúng
                int totalTargetTouch = 0;

                /// Đánh dấu là mục tiêu đầu tiên
                bool isFirstTarget = true;

                /// Duyệt toàn bộ danh sách
                foreach (GameObject go in list)
                {
                    /// Tăng số mục tiêu đã chạm phải
                    totalTargetTouch++;

                    /// Vị trí đối tượng hiện tại
                    UnityEngine.Vector2 targetPos = new UnityEngine.Vector2((float) go.CurrentPos.X, (float) go.CurrentPos.Y);

                    /// Thực hiện sự kiện chạm mục tiêu lần đầu
                    if (isFirstTarget)
                    {
                        KTSkillManager.BulletFirstTouchTarget(bullet.Skill, bullet.Caster, pos, go, null);
                    }

                    /// Thực hiện sự kiện đạn nổ
                    KTSkillManager.BulletExplode(bullet.Skill, bullet.Caster, pos, go, bullet.DirVectorProperty, bullet.TouchTargets.Count);

                    /// Nếu là mục tiêu đầu tiên thì đánh dấu không cho bọn đằng sau lặp lại nữa
                    if (isFirstTarget)
                    {
                        isFirstTarget = false;

                        /// Thêm đạn nổ vào gói tin
                        this.AppendBulletExplode(bullet, bullet.Skill.Data.BulletID, targetPos, 0, null);
                    }

                    /// Thêm mục tiêu vào danh sách đã chạm
                    bullet.TouchTargets.Add(go);

                    /// Đánh dấu mục tiêu là mục tiêu cuối chạm
                    vanishTarget = go;

                    /// Nếu số mục tiêu đã chạm phải tối đa thì thoát
                    if (totalTargetTouch >= maxTargetTouch)
                    {
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Skill, ex.ToString());
            }
        }

        /// <summary>
        /// Thực hiện Logic bẫy
        /// </summary>
        /// <param name="bullet">Bẫy</param>
        /// <param name="pos">Vị trí</param>
        /// <param name="maxTargetTouch">Số mục tiêu tối đa</param>
        /// <param name="explodeRadius">Phạm vi nổ</param>
        /// <param name="vanishTarget">Mục tiêu cuối chạm phải</param>
        private void DoTrapLogic(BulletTimer bulletTimer, UnityEngine.Vector2 pos, int maxTargetTouch, int explodeRadius)
        {
            if (bulletTimer == null || bulletTimer.Bullet == null)
            {
                return;
            }

            try
            {
                List<GameObject> list = this.GetExplosionTargets(bulletTimer.Bullet, pos, explodeRadius, bulletTimer.Bullet.MultipleTouchTarget);
                if (list.Count <= 0)
                {
                    return;
                }

                /// Nếu chưa thằng nào chạm bẫy
                if (list.Count <= 0)
                {
                    return;
                }

                /// Khởi tạo số mục tiêu đã nổ trúng
                int totalTargetTouch = 0;
                /// Khởi tạo chưa gửi hiệu ứng bẫy nổ về Client
                bool sentExplodeToClient = false;
                /// Đánh dấu có phải mục tiêu đầu tiên không
                bool isFirstTarget = true;

                /// Duyệt toàn bộ danh sách
                foreach (GameObject go in list)
                {
                    /// Tăng số mục tiêu đã chạm phải
                    totalTargetTouch++;

                    /// Thực hiện sự kiện chạm mục tiêu lần đầu
                    if (isFirstTarget)
                    {
                        KTSkillManager.BulletFirstTouchTarget(bulletTimer.Bullet.Skill, bulletTimer.Bullet.Caster, pos, go, null);
                    }

                    /// Thực hiện sự kiện bẫy nổ
                    KTSkillManager.BulletExplode(bulletTimer.Bullet.Skill, bulletTimer.Bullet.Caster, bulletTimer.Bullet.FromPos, go, bulletTimer.Bullet.DirVectorProperty, bulletTimer.Bullet.TouchTargets.Count);

                    if (!sentExplodeToClient)
                    {
                        /// Thêm đạn nổ vào gói tin
                        this.AppendBulletExplode(bulletTimer.Bullet, bulletTimer.Bullet.Skill.Data.BulletID, bulletTimer.Bullet.FromPos, 0, null);

                        /// Nếu đánh dấu bẫy không xóa khi chạm mục tiêu
                        if (bulletTimer.Bullet.Skill.Data.SkillStyle != "trapnodestroyontouch")
                        {
                            /// Xóa bẫy khỏi bản đồ
                            KTTrapManager.RemoveTrap(bulletTimer.Bullet.ID);
                        }
                    }

                    /// Nếu là mục tiêu đầu tiên thì đánh dấu không cho bọn đằng sau lặp lại nữa
                    if (isFirstTarget)
                    {
                        isFirstTarget = false;
                    }

                    /// Nếu đánh dấu bẫy không xóa khi chạm mục tiêu
                    if (bulletTimer.Bullet.Skill.Data.SkillStyle != "trapnodestroyontouch")
                    {
                        /// Thêm mục tiêu vào danh sách đã chạm
                        bulletTimer.Bullet.TouchTargets.Add(go);
                    }

                    /// Đánh dấu mục tiêu cuối chạm phải
                    bulletTimer.VanishTarget = go;

                    /// Đánh dấu đã gửi hiệu ứng bẫy nổ về Client
                    sentExplodeToClient = true;

                    /// Nếu số mục tiêu đã chạm phải tối đa thì thoát
                    if (totalTargetTouch >= maxTargetTouch)
                    {
                        break;
                    }
                }

                /// Nếu đánh dấu bẫy không xóa khi chạm mục tiêu
                if (bulletTimer.Bullet.Skill.Data.SkillStyle != "trapnodestroyontouch")
                {
                    /// Xóa luồng thực thi kiểm tra bẫy
                    bulletTimer.Stop();
                }
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Skill, ex.ToString());
                /// Xóa luồng thực thi kiểm tra bẫy
                bulletTimer.Stop();
            }
        }
    }
}
