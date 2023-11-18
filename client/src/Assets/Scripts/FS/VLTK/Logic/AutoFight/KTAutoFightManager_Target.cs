using FS.GameEngine.Logic;
using FS.GameEngine.Sprite;
using FS.VLTK.Control.Component;
using FS.VLTK.Factory;
using FS.VLTK.Logic.Settings;
using Server.Data;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

namespace FS.VLTK.Logic
{
    /// <summary>
    /// Quản lý tìm mục tiêu
    /// </summary>
    public partial class KTAutoFightManager
    {
        /// <summary>
        /// ID mục tiêu
        /// </summary>
        public int TrigerAttackID { get; set; } = -1;

        /// <summary>
        /// Đang phản kháng lại hay không
        /// </summary>
        public bool IsTriger { get; set; } = false;

        /// <summary>
        /// Tập trung vào đánh mục tiêu có ID tương ứng
        /// </summary>
        /// <param name="roleID"></param>
        public void ForceTaget(int roleID)
        {
            /// Nếu đang bật tự động đánh
            if (this.IsAutoFighting)
            {
                /// Nếu có config tự động đánh trả
                if (KTAutoAttackSetting.Config.PK.AutoReflect)
                {
                    /// Tìm thử xem có thấy thằng kia xung quanh mình không
                    GSprite players = KTObjectsManager.Instance.FindObjects<GSprite>(x => x.SpriteType == GSpriteTypes.Other && x.RoleData.RoleID == roleID).FirstOrDefault();
                    if (players != null)
                    {
                        if (!players.IsDeath && players.HP > 0)
                        {
                            /// Thằng nào đánh
                            this.TrigerAttackID = roleID;
                            /// reset lại thời gian bị triger
                            this.TrigerTime = Stopwatch.StartNew();
                            /// set cho nó đang bị triger
                            this.IsTriger = true;
                            /// Đổi mục tiêu sẽ đấm là thằng này
                            this.ChangeAutoFightTarget(players);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Tìm mục tiêu phù hợp nhất
        /// </summary>
        /// <param name="ignoredTarget"></param>
        /// <param name="ignoreLastTarget"></param>
        /// <param name="skillRange"></param>
        /// <returns></returns>
        private GSprite FindBestTarget(List<GSprite> ignoredTarget = null, bool ignoreLastTarget = false, int skillRange = -1)
        {
            GSprite newTarget = null;

            /// Nếu không phải chế độ đánh quái==> tức là chỉ đánh người
            if (KTAutoAttackSetting.Config.EnableAutoPK || this.IsTriger)
            {
                /// Nếu đang bị tấn công
                if (this.IsTriger && TrigerAttackID != -1)
                {
                    /// Tìm thử xem có thấy thằng kia xung quanh mình không
                    GSprite players = KTObjectsManager.Instance.FindObjects<GSprite>(x => x.SpriteType == GSpriteTypes.Other && x.RoleData.RoleID == TrigerAttackID).FirstOrDefault();

                    if (players != null)
                    {
                        /// Nếu thằng này chưa chết và có HP > 0

                        if (!players.IsDeath && players.HP > 0)
                        {
                            float distance = Vector2.Distance(players.PositionInVector2, Global.Data.Leader.PositionInVector2);

                            /// Nếu khoảng cách còn trong 1 màn hình thì vã nó
                            if (distance < 600f)
                            {
                                return players;
                            }
                        }
                        /// Nếu thấy thằng này đã chết thì xóa triger cho nhân vật
                        else
                        {
                            this.TrigerTime.Stop();
                            this.TrigerAttackID = -1;
                            this.IsTriger = false;
                        }
                    }
                }

                newTarget = SkillManager.FindNearestEnemy(Global.Data.Leader, (sprite) =>
                {
                    if (Global.Data.OtherRoles.TryGetValue(sprite.RoleID, out RoleData rd))
                    {
                        if (rd.CurrentHP <= 0)
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }

                    return (!ignoreLastTarget || sprite != this.AutoFightLastTarget) && (ignoredTarget == null || (ignoredTarget != null && !ignoredTarget.Contains(sprite)));
                }, skillRange);

                /// Nếu chỉ đánh người mà không tìm thấy người thì thôi
                if (newTarget == null && KTAutoAttackSetting.Config.EnableAutoPK)
                {
                    long Now = KTGlobal.GetCurrentTimeMilis();

                    if (Now - this.LastArletNoTarget > 10000)
                    {
                        

                        this.LastArletNoTarget = Now;
                        KTGlobal.AddNotification("Không tìm thấy người chơi có thể tấn công chuyển sang tìm quái!");
                    }
                    // Nếu ko tìm thây người thì đánh quái
                    goto GOFINDMONSTER;
                }

                /// Nếu tìm thấy mục tiêu thì trả về kết quả luôn
                if (newTarget != null)
                {
                    return newTarget;
                }
            }

            GOFINDMONSTER:
            if (KTAutoAttackSetting.Config.Farm.FarmAround)
            {
                newTarget = SkillManager.FindNearestEnemy(Global.Data.Leader, this.StartPos, (sprite) =>
                {
                    /// Dữ liệu đối tượng
                    if (Global.Data.SystemMonsters.TryGetValue(sprite.RoleID, out MonsterData md))
                    {
                        /// Nếu có thiết lập bỏ qua Boss
                        if (KTAutoAttackSetting.Config.Farm.IgnoreBoss && md.MonsterType != (int)MonsterTypes.Normal && md.MonsterType != (int)MonsterTypes.Hater && md.MonsterType != (int)MonsterTypes.Special_Normal && md.MonsterType != (int)MonsterTypes.Static && md.MonsterType != (int)MonsterTypes.Static_ImmuneAll)
                        {
                            return false;
                        }
                        else if (md.HP <= 0)
                        {
                            return false;
                        }

                        // nếu như đang có config đánh quanh điểm
                        if (KTAutoAttackSetting.Config.Farm.FarmAround)
                        {
                            // Lấy ra vị trí
                            float DistanceCheckWithRadius = Vector2.Distance(this.StartPos, sprite.PositionInVector2);
                            // Nếu con quái này nằm ngoài phạm vi thì thôi
                            if (DistanceCheckWithRadius > KTAutoAttackSetting.Config.Farm.ScanRange)
                            {
                                return false;
                            }
                        }
                    }
                    else if (Global.Data.OtherRoles.TryGetValue(sprite.RoleID, out RoleData rd))
                    {
                        if (rd.CurrentHP <= 0)
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }

                    return (!ignoreLastTarget || sprite != this.AutoFightLastTarget) && (ignoredTarget == null || (ignoredTarget != null && !ignoredTarget.Contains(sprite)));
                }, skillRange);
            }
            else
            {
                /// Tìm mục tiêu gần nhất khác với mục tiêu hiện tại
                newTarget = SkillManager.FindNearestEnemy(Global.Data.Leader, (sprite) =>
                {
                    /// Dữ liệu đối tượng
                    if (Global.Data.SystemMonsters.TryGetValue(sprite.RoleID, out MonsterData md))
                    {
                        /// Nếu có thiết lập bỏ qua Boss
                        if (KTAutoAttackSetting.Config.Farm.IgnoreBoss && md.MonsterType != (int)MonsterTypes.Normal && md.MonsterType != (int)MonsterTypes.Hater && md.MonsterType != (int)MonsterTypes.Special_Normal && md.MonsterType != (int)MonsterTypes.Static && md.MonsterType != (int)MonsterTypes.Static_ImmuneAll)
                        {
                            return false;
                        }
                        else if (md.HP <= 0)
                        {
                            return false;
                        }

                        // nếu như đang có config đánh quanh điểm
                        if (KTAutoAttackSetting.Config.Farm.FarmAround)
                        {
                            // Lấy ra vị trí
                            float DistanceCheckWithRadius = Vector2.Distance(this.StartPos, sprite.PositionInVector2);
                            // Nếu con quái này nằm ngoài phạm vi thì thôi
                            if (DistanceCheckWithRadius > KTAutoAttackSetting.Config.Farm.ScanRange)
                            {
                                return false;
                            }
                        }
                    }
                    else if (Global.Data.OtherRoles.TryGetValue(sprite.RoleID, out RoleData rd))
                    {
                        if (rd.CurrentHP <= 0)
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }

                    return (!ignoreLastTarget || sprite != this.AutoFightLastTarget) && (ignoredTarget == null || (ignoredTarget != null && !ignoredTarget.Contains(sprite)));
                }, skillRange);
            }

            if (KTAutoAttackSetting.Config.Farm.FarmAround && newTarget == null)
            {
                long Now = KTGlobal.GetCurrentTimeMilis();

                if (Now - this.LastArletNoTarget > 5000)
                {
                    if (ignoredTarget != null)
                    {
                        ignoredTarget = null;
                    }
                    // Nếu mà bug mục tiêu quá lâu thì thực hiện clear dánh sách bandlist để bú lại

                    this.LastArletNoTarget = Now;
                    KTGlobal.AddNotification("Không tìm thấy mục tiêu trong phạm vi đánh <color=red>" + KTAutoAttackSetting.Config.Farm.ScanRange + "</color>");
                }
            }
            /// Trả về kết quả
            return newTarget;
        }
    }
}