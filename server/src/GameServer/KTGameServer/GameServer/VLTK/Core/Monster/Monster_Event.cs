using GameServer.KiemThe;
using GameServer.KiemThe.Entities;
using GameServer.KiemThe.Logic;
using Server.Tools;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using static GameServer.Logic.KTMapManager;

namespace GameServer.Logic
{
    /// <summary>
    /// Sự kiện
    /// </summary>
    public partial class Monster
    {
        #region Base
        /// <summary>
        /// Hàm này gọi liên tục 0.5s một lần
        /// </summary>
        public override void Tick()
        {
            base.Tick();

            try
            {
                /// Thực thi sự kiện Tick
                this.OnTick?.Invoke();
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Exception, ex.ToString());
            }
        }

        /// <summary>
        /// Sự kiện khi quái chết
        /// </summary>
        /// <param name="killer"></param>
        public override void OnDie(GameObject killer)
        {
            /// Gọi phương thức cha
            base.OnDie(killer);

            /// Ngừng StoryBoard
            KTMonsterStoryBoardEx.Instance.Remove(this);
            /// Cập nhật thời điểm chết
            this.LastDeadTicks = KTGlobal.GetCurrentTimeMilis();

            /// Xóa toàn bộ Buff và vòng sáng tương ứng
            this.Buffs.RemoveAllBuffs();
            this.Buffs.RemoveAllAruas();
            this.Buffs.RemoveAllAvoidBuffs();

            /// Xóa toàn bộ kỹ năng tự kích hoạt tương ứng
            this.RemoveAllAutoSkills();

            /// Làm mới AI
            this.ResetAI();

            /// Làm rỗng danh sách biến cục bộ
            this.RemoveAllLocalParams();

            /// Xóa luồng thực thi
            KTMonsterTimerManager.Instance.Remove(this);

            try
            {
                /// Thực thi hàm Callback
                this.OnDieCallback?.Invoke(killer);
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Exception, ex.ToString());
            }

            /// Bản đồ
            GameMap gameMap = KTMapManager.Find(this.CurrentMapCode);
            /// Xóa khỏi lưới
            gameMap.Grid.RemoveObject(this);
        }

        /// <summary>
        /// Sự kiện khi quái giết đối tượng khác
        /// </summary>
        /// <param name="deadObj"></param>
        public override void OnKillObject(GameObject deadObj)
        {
            base.OnKillObject(deadObj);
        }

        /// <summary>
        /// Sự kiện khi quái đánh trúng
        /// </summary>
        /// <param name="target"></param>
        /// <param name="nDamage"></param>
        public override void OnHitTarget(GameObject target, int nDamage)
        {
            base.OnHitTarget(target, nDamage);
        }

        /// <summary>
        /// Sự kiện khi quái bị đánh trúng
        /// </summary>
        /// <param name="attacker"></param>
        /// <param name="nDamage"></param>
        public override void OnBeHit(GameObject attacker, int nDamage)
        {
            base.OnBeHit(attacker, nDamage);
        }

        /// <summary>
        /// Nhận sát thương
        /// </summary>
        /// <param name="attacker"></param>
        /// <param name="damageTaken"></param>
        public override void TakeDamage(GameObject attacker, int damageTaken)
        {
            try
            {
                /// Nếu không có attacker
                if (attacker == null)
                {
                    return;
                }

                /// Đánh dấu thời điểm bị tấn công lần trước
                this.LastBeHitTicks = KTGlobal.GetCurrentTimeMilis();

                /// Nếu là quái tĩnh thì thôi
                if (this.IsStatic)
                {
                    return;
                }

                /// Nếu là NPC thì thôi
                if (this.MonsterType == MonsterAIType.DynamicNPC)
                {
                    return;
                }

                try
                {
                    /// Thực thi sự kiện khi chịu sát thương
                    this.OnTakeDamage?.Invoke(attacker, damageTaken);
                }
                catch (Exception ex)
                {
                    LogManager.WriteLog(LogTypes.Exception, ex.ToString());
                }

                /// Nếu đang di chuyển
                if (this.IsMoving || this.IsBackingToOriginPos)
                {
                    return;
                }

                /// Nếu có mục tiêu đang đuổi theo
                if (this.chaseTarget != null && !this.chaseTarget.IsDead() && this.chaseTarget.CurrentMapCode == this.CurrentMapCode && this.chaseTarget.CurrentCopyMapID == this.CurrentCopyMapID)
                {
                    return;
                }

                /// Nếu là kẻ địch
                if (attacker != null && KTGlobal.IsOpposite(this, attacker))
                {
                    /// Đuổi theo mục tiêu vừa tấn công
                    this.chaseTarget = attacker;
                }
            }
            catch (Exception) { }
        }
        #endregion

        #region Tick move
        /// <summary>
        /// Vị trí tiếp theo cần tới
        /// </summary>
        public Point NextMoveTo { get; set; }

        /// <summary>
        /// Có phải AI tự chạy không
        /// </summary>
        public bool IsAIRandomMove { get; set; }

        /// <summary>
        /// Có đang quay trở lại vị trí ban đầu không
        /// </summary>
        public bool IsBackingToOriginPos { get; set; } = false;

        /// <summary>
        /// Tick di chuyển quái
        /// </summary>
        public void MonsterAI_TickMove()
        {
            try
            {
                /// Nếu quái đã chết
                if (this.IsDead())
                {
                    return;
                }
                /// Nếu đang di chuyển và đang quay trở lại vị trí ban đầu
                if (KTMonsterStoryBoardEx.Instance.HasStoryBoard(this) && this.IsBackingToOriginPos)
                {
                    return;
                }
                /// Nếu không đuổi mục tiêu và không có lệnh AI Random chạy và cũng không có lệnh về vị trí ban đầu
                else if (this.chaseTarget == null && !this.IsAIRandomMove && !this.IsBackingToOriginPos)
                {
                    return;
                }
                /// Nếu không có vị trí đích
                else if (this.NextMoveTo == new Point(-1, -1) || this.NextMoveTo == new Point(0, 0))
                {
                    return;
                }

                /// Vị trí hiện tại
                Point curPos = this.CurrentPos;

                /// Bỏ vị trí đích đến
                this.ToPos = this.NextMoveTo;

                /// Nếu không nằm ở vị trí có vật cản thì tiến hành tìm đường
                if (KTGlobal.HasPath(this.CurrentMapCode, curPos, this.ToPos) && !KTGlobal.InObs(this.MapCode, (int)curPos.X, (int)curPos.Y, this.CurrentCopyMapID) || !KTGlobal.InObs(this.MapCode, (int)this.NextMoveTo.X, (int)this.NextMoveTo.Y, this.CurrentCopyMapID))
                {
                    /// Nếu tồn tại StoryBoard cũ thì xóa
                    KTMonsterStoryBoardEx.Instance.Remove(this);

                    /// Nếu là AI tự chạy
                    if (this.IsAIRandomMove)
                    {
                        /// Nếu là AI tự chạy
                        this.StartPos = this.NextMoveTo;
                    }

                    /// Thực hiện chạy đến vị trí đích
                    KTMonsterStoryBoardEx.Instance.Add(this, this.CurrentPos, this.NextMoveTo, KiemThe.Entities.KE_NPC_DOING.do_run, false, this.UseAStarPathFinder);
                }

                /// Hủy vị trí tiếp theo cần đến
                this.NextMoveTo = new Point(-1, -1);
                /// Hủy đánh dấu AI tự di chuyển
                this.IsAIRandomMove = false;
            }
            catch (Exception ex)
            {

            }
        }
        #endregion

        #region Relive
        /// <summary>
        /// Sự kiện sinh quái vật
        /// </summary>
        public void OnRelive()
        {
            /// Ngừng di chuyển
            if (KTMonsterStoryBoardEx.Instance != null)
            {
                KTMonsterStoryBoardEx.Instance.Remove(this);
            }

            /// Cập nhật thời gian chết
            this.LastDeadTicks = 0;
            /// Reset đối tượng
            this.Reset();

            /// Gọi hàm Awake
            this.Awake();
        }
        #endregion

        #region Record damage
        /// <summary>
        /// Nhóm sát thương gây ra cho quái
        /// </summary>
        public class DamgeGroup
        {
            /// <summary>
            /// Có phải do 1 nhóm không
            /// </summary>
            public bool IsTeam { get; set; }

            /// <summary>
            /// ID thằng gây sát thương
            /// </summary>
            public int ID { get; set; }

            /// <summary>
            /// Tổng số sát thương
            /// </summary>
            public long TotalDamage { get; set; }

            /// <summary>
            /// Thời gian cập nhật lần cuối
            /// </summary>
            public long LastUpdateTime { get; set; }

        }

        /// <summary>
        /// Ghi lại lượng sát thương nhận được gây ra bởi đối tượng có ID tương ứng
        /// </summary>
        private readonly ConcurrentDictionary<int, DamgeGroup> DamageTakeRecord = new ConcurrentDictionary<int, DamgeGroup>();

        /// <summary>
        /// Có ghi lại tổng sát thương nhận được không
        /// </summary>
        public bool AllowRecordDamage { get; set; } = false;

        /// <summary>
        /// Xử lý lịch sử sát thương
        /// </summary>
        private void Tick_RecordDamage()
        {
            /// Nếu không ghi lại lịch sử
            if (!this.AllowRecordDamage)
            {
                /// Bỏ qua
                return;
            }

            try
            {
                /// Thời điểm hiện tại
                long currentTicks = KTGlobal.GetCurrentTimeMilis();

                /// Danh sách khóa
                List<int> keys = this.DamageTakeRecord.Keys.ToList();
                /// Duyệt danh sách khóa
                foreach (int key in keys)
                {
                    /// Không tồn tại
                    if (!this.DamageTakeRecord.TryGetValue(key, out DamgeGroup damageGroup))
                    {
                        /// Bỏ qua
                        continue;
                    }
                    /// Nếu thằng này 30s mà chưa cập nhật
                    if (currentTicks - damageGroup.LastUpdateTime > 30000)
                    {
                        /// Xóa khỏi danh sách
                        this.DamageTakeRecord.TryRemove(key, out _);
                        /// Bỏ qua
                        continue;
                    }
                }

                /// Thằng có sát thương cao nhất
                Monster.DamgeGroup topDamage = this.GetTopDamage();

                /// Nếu không có thằng nào có sát thương cao nhất
                if (topDamage == null || topDamage.TotalDamage <= 0)
                {
                    /// Lưu lại danh hiệu cho nó
                    this.Title = "(Không thuộc về ai)";
                    /// Bỏ qua
                    return;
                }

                /// Thời gian Reset
                long resetTime = (30000 - currentTicks + topDamage.LastUpdateTime) / 1000;
                /// Nếu là nhóm
                if (topDamage.IsTeam)
                {
                    /// Đội trưởng
                    KPlayer teamLeader = KTTeamManager.GetTeamLeader(topDamage.ID);
                    /// Nếu tồn tại
                    if (teamLeader != null)
                    {
                        this.Title = "<color=#00ff2a>(Thuộc đội: " + teamLeader.RoleName + ", còn: " + resetTime + "s)</color>";
                    }
                }
                else
                {
                    /// Người chơi tương ứng
                    KPlayer player = KTPlayerManager.Find(topDamage.ID);
                    /// Nếu tồn tại
                    if (player != null)
                    {
                        this.Title = "<color=#00ff2a>(Thuộc về: " + player.RoleName + ", còn: " + resetTime + "s)</color>";
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Exception, ex.ToString());
            }
        }

        /// <summary>
        /// Trả về thằng gây sát thương nhiều nhất lên quái này
        /// </summary>
        /// <returns></returns>
        public DamgeGroup GetTopDamage()
        {
            /// Thằng có sát thương cao nhất
            Monster.DamgeGroup topDamage = null;

            /// Danh sách khóa
            List<int> keys = this.DamageTakeRecord.Keys.ToList();
            /// Duyệt danh sách khóa
            foreach (int key in keys)
            {
                /// Không tồn tại
                if (!this.DamageTakeRecord.TryGetValue(key, out DamgeGroup damageGroup))
                {
                    /// Bỏ qua
                    continue;
                }
                
                /// Nếu chưa có thằng nào có sát thương cao nhất
                if (topDamage == null)
                {
                    /// Đánh dấu là thằng này
                    topDamage = damageGroup;
                }
                /// Nếu sát thương lớn hơn thằng cũ tìm được
                else if (damageGroup.TotalDamage > topDamage.TotalDamage)
                {
                    /// Đánh dấu lại là thằng này
                    topDamage = damageGroup;
                }
            }

            /// Trả về kết quả
            return topDamage;
        }

        /// <summary>
        /// Thêm sát thương nhận được bởi đối tượng tương ứng vào danh sách
        /// </summary>
        /// <param name="roleID"></param>
        /// <param name="nDamage"></param>
        /// <param name="isTeam"></param>
        public void RecordDamage(int roleID, int nDamage, bool isTeam)
        {
            /// Nếu không ghi lại lịch sử
            if (!this.AllowRecordDamage)
            {
                /// Bỏ qua
                return;
            }

            /// Nếu đã tồn tại
            if (this.DamageTakeRecord.TryGetValue(roleID, out DamgeGroup totalDamageDealt))
            {
                /// Cập nhật lại
                totalDamageDealt.TotalDamage += nDamage;
                totalDamageDealt.LastUpdateTime = KTGlobal.GetCurrentTimeMilis();
            }
            /// Nếu chưa tồn tại
            else
            {
                /// Tạo mới
                DamgeGroup damageGroup = new DamgeGroup()
                {
                    IsTeam = isTeam,
                    TotalDamage = nDamage,
                    LastUpdateTime = KTGlobal.GetCurrentTimeMilis(),
                    ID = roleID,
                };

                /// Thêm vào danh sách
                this.DamageTakeRecord[roleID] = damageGroup;
            }
        }
        #endregion
    }
}
