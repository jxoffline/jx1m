using GameServer.KiemThe;
using GameServer.KiemThe.Core.MonsterAIScript;
using GameServer.KiemThe.Entities;
using GameServer.KiemThe.Logic;
using GameServer.KiemThe.LuaSystem;
using Server.Tools;
using System;
using System.Collections.Generic;
using System.Windows;
using static GameServer.Logic.KTMapManager;

namespace GameServer.Logic
{
    /// <summary>
    /// AI của quái
    /// </summary>
    public partial class Monster
    {
		#region Private fields
		/// <summary>
		/// Mục tiêu đuổi theo
		/// </summary>
		private GameObject chaseTarget = null;
        #endregion

        #region Public methods
        /// <summary>
        /// Hàm này gọi liên tục khi nào quái còn tồn tại
        /// </summary>
        public void AI_Tick()
        {
            try
            {
                /// Xử lý lịch sử sát thương
                this.Tick_RecordDamage();

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

                /// Bản đồ tương ứng
                GameMap gameMap = KTMapManager.Find(this.CurrentMapCode);

                /// Nếu vị trí ban đầu lỗi thì thiết lập lại vị trí hiện tại
                if (!gameMap.CanMove(this.StartPos, this.CurrentCopyMapID))
                {
                    /// Gắn lại vị trí hiện tại
                    this.StartPos = this.CurrentPos;
                }

                /// Nếu quái đã chết
                if (this.IsDead())
                {
                    return;
                }

                /// Nếu phạm vi đuổi quá nhỏ
                if (this.SeekRange < 500)
                {
                    this.SeekRange = 500;
                }

                /// Nếu vị trí hiện tại trùng với vị trí ban đầu
                if (KTGlobal.GetDistanceBetweenPoints(this.CurrentPos, this.StartPos) <= 10)
                {
                    /// Đánh dấu không phải đang quay về vị trí ban đầu
                    this.IsBackingToOriginPos = false;
                    /// Đánh dấu đang đứng
                    this.m_eDoing = KE_NPC_DOING.do_stand;
                }

                /// Nếu đang di chuyển về vị trí ban đầu thì thôi
                if (this.IsBackingToOriginPos)
                {
                    return;
                }

                /// Nếu tiến vào khu an toàn
                if (gameMap.MyNodeGrid.InSafeArea((int) this.CurrentGrid.X, (int) this.CurrentGrid.Y))
                {
                    /// Hủy mục tiêu đang đuổi
                    this.chaseTarget = null;
                    /// Đánh dấu đang quay trở lại vị trí ban đầu
                    this.IsBackingToOriginPos = true;
                    /// Quay trở lại vị trí ban đầu
                    this.NextMoveTo = this.StartPos;
                    /// Bỏ qua
                    return;
                }

                /// Nếu vị trí hiện tại quá xa so với vị trí ban đầu
                if (KTGlobal.GetDistanceBetweenPoints(this.CurrentPos, this.StartPos) >= this.SeekRange)
                {
                    /// Hủy mục tiêu đang đuổi
                    this.chaseTarget = null;
                    /// Đánh dấu đang quay trở lại vị trí ban đầu
                    this.IsBackingToOriginPos = true;
                    /// Quay trở lại vị trí ban đầu
                    this.NextMoveTo = this.StartPos;
                    /// Bỏ qua
                    return;
                }

                /// Nếu có mục tiêu đuổi nhưng đã chết, hoặc khác bản đồ
                if (this.chaseTarget != null && (this.chaseTarget.IsDead() || this.chaseTarget.CurrentMapCode != this.CurrentMapCode || this.chaseTarget.CurrentCopyMapID != this.CurrentCopyMapID))
                {
                    /// Hủy mục tiêu đang đuổi
                    this.chaseTarget = null;
                    /// Đánh dấu đang quay trở lại vị trí ban đầu
                    this.IsBackingToOriginPos = true;
                    /// Quay trở lại vị trí ban đầu
                    this.NextMoveTo = this.StartPos;
                    /// Bỏ qua
                    return;
                }

                /// Nếu có mục tiêu tàng hình và bản thân không nhìn được
                if (this.chaseTarget != null && this.chaseTarget.IsInvisible() && !this.chaseTarget.VisibleTo(this))
                {
                    /// Hủy mục tiêu đang đuổi
                    this.chaseTarget = null;
                    /// Đánh dấu đang quay trở lại vị trí ban đầu
                    this.IsBackingToOriginPos = true;
                    /// Quay trở lại vị trí ban đầu
                    this.NextMoveTo = this.StartPos;
                    /// Bỏ qua
                    return;
                }

                if(this.chaseTarget!=null)
                {
                    if (this.chaseTarget != null && !this.chaseTarget.IsDead() && this.chaseTarget.CurrentMapCode == this.CurrentMapCode && this.chaseTarget.CurrentCopyMapID == this.CurrentCopyMapID)
                    {
                        /// Nếu AIScript có tồn tại trong hệ thống
                        if (KTMonsterAIScriptManager.HasAIScript(this.AIID) && !this.IsDead() && this.chaseTarget != null)
                        {
                            /// Thực hiện ScriptAI tương ứng
                            UnityEngine.Vector2 destPos = KTMonsterAIScriptManager.GetAIScript(this.AIID).Process(this, this.chaseTarget);
                            /// Nếu có vị trí cần dịch đến
                            if (destPos != UnityEngine.Vector2.zero)
                            {
                                /// Di chuyển đến chỗ mục tiêu
                                this.MoveTo(new Point(destPos.x, destPos.y));
                            }
                        }
                    }

                }    
                /// Nếu có mục tiêu đuổi            
                else
                {
                    /// Nếu loại quái là quái tinh anh hoặc Boss hoặc quái chữ đỏ thì sẽ tìm kiếm mục tiêu xung quanh
                    if (this.MonsterType == MonsterAIType.Elite || this.MonsterType == MonsterAIType.Leader || this.MonsterType == MonsterAIType.Boss || this.MonsterType == MonsterAIType.Pirate || this.MonsterType == MonsterAIType.Hater || this.MonsterType == MonsterAIType.Special_Normal || this.MonsterType == MonsterAIType.Special_Boss)
                    {
                        /// Tìm kẻ địch xung quanh
                        List<GameObject> enemies = KTGlobal.GetNearByVisibleEnemies(this, this.SeekRange, 1);

                        /// Nếu tìm thấy mục tiêu
                        if (enemies != null && enemies.Count == 1)
                        {
                            /// Kẻ địch tương ứng
                            GameObject enemy = enemies[0];
                            /// Đánh dấu mục tiêu đuổi
                            this.chaseTarget = enemy;
                        }
                    }
                }
            }
            catch (Exception) { }
        }
        #endregion
    }
}
