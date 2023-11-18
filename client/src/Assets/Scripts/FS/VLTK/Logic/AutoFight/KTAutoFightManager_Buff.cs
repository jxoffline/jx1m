
using FS.GameEngine.Logic;
using FS.GameEngine.Sprite;
using FS.VLTK.Control.Component;
using FS.VLTK.Entities.Config;
using FS.VLTK.Factory.UIManager;
using FS.VLTK.Logic.Settings;
using Server.Data;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FS.VLTK.Logic
{
    /// <summary>
    /// Tự dùng Buff
    /// </summary>
    public partial class KTAutoFightManager
    {
        #region Nga My
        /// <summary>
        /// ID kỹ năng phục hồi máu của Nga My
        /// </summary>
        public const int EM_HealHPSkillID = 10175;

        /// <summary>
        /// Tự động Buff máu cho đồng đội, áp dụng với Nga My
        /// </summary>
        private void AutoEMBuff()
        {
            /// Nếu bản thân không phải Nga My
            if (Global.Data.RoleData.FactionID != 5)
            {
                return;
            }

            /// Nếu chưa đến thời gian kiểm tra thì bỏ qua
            if (KTGlobal.GetCurrentTimeMilis() - this.AutoFightLastCheckAutoEMBuff < this.AutoFight_AutoEMBuffEveryTick)
            {
                return;
            }

            /// Đánh dấu thời gian kiểm tra tự động kích hoạt các Buff chủ động
            this.AutoFightLastCheckAutoEMBuff = KTGlobal.GetCurrentTimeMilis();

            /// Duyệt danh sách kỹ năng, tìm ra các Buff chủ động tương ứng
            foreach (SkillData skill in Global.Data.RoleData.SkillDataList)
            {
                /// Nếu chưa học kỹ năng này
                if (skill.Level <= 0)
                {
                    continue;
                }

                if (Loader.Loader.Skills.TryGetValue(skill.SkillID, out SkillDataEx skillData))
                {
                    /// Nếu không thỏa mãn điều kiện có thể dùng kỹ năng này
                    if (!SkillManager.IsAbleToUseSkill(skillData, skill.Level, false))
                    {
                        continue;
                    }

                    /// Nếu có kỹ năng Từ Hàng Phổ Độ
                    if (skill.SkillID == KTAutoFightManager.EM_HealHPSkillID)
                    {
                        /// Nếu có thiết lập buff nga my
                        if (KTAutoAttackSetting.Config.Support.EM_AutoHeal)
                        {
                            /// Nếu có nhóm
                            if (Global.Data.RoleData.TeamID != -1)
                            {
                                /// Duyệt danh sách thành viên
                                foreach (RoleDataMini teammateRD in Global.Data.Teammates)
                                {
                                    /// % sinh lực đồng đội
                                    int hpPercent = teammateRD.HP * 100 / teammateRD.MaxHP;

                                    /// Nếu dưới ngưỡng thiết lập
                                    if (hpPercent < KTAutoAttackSetting.Config.Support.EM_AutoHealHPPercent)
                                    {
                                        /// Tìm xem thằng này có ở xung quanh không
                                        GSprite players = KTGlobal.FindSpriteByID(teammateRD.RoleID);

                                        /// Nếu còn sống
                                        if (players != null && !players.IsDeath)
                                        {
                                            /// Khoảng cách đến vị trí thằng ka
                                            float distance = Vector2.Distance(players.PositionInVector2, Global.Data.Leader.PositionInVector2);
                                            /// Nếu khoảng cách hợp lý thì buff cho nó ngay
                                            if (distance < 600f)
                                            {
                                                /// Mục tiêu
                                                GSprite tmp = SkillManager.SelectedTarget;
                                                /// Thiết lập không có mục tiêu
                                                SkillManager.SelectedTarget = players;
                                                /// Dùng kỹ năng Buff
                                                SkillManager.LeaderUseSkill(skill.SkillID, false);
                                                /// Trả lại mục tiêu cũ
                                                SkillManager.SelectedTarget = tmp;
                                            }
                                        }
                                    }
                                }
                            }
                            /// Nếu không có nhóm
                            else
                            {
                                /// % sinh lực bản thân
                                int hpPercent = Global.Data.RoleData.CurrentHP * 100 / Global.Data.RoleData.MaxHP;

                                /// Nếu dưới ngưỡng thiết lập
                                if (hpPercent < KTAutoAttackSetting.Config.Support.EM_AutoHealHPPercent)
                                {
                                    /// Mục tiêu
                                    GSprite tmp = SkillManager.SelectedTarget;
                                    /// Thiết lập không có mục tiêu
                                    SkillManager.SelectedTarget = Global.Data.Leader;
                                    /// Dùng kỹ năng Buff
                                    SkillManager.LeaderUseSkill(skill.SkillID, false);
                                    /// Trả lại mục tiêu cũ
                                    SkillManager.SelectedTarget = tmp;
                                    /// Thoát luôn
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }
        #endregion

        #region Buff chủ động
        /// <summary>
        /// Danh sách kỹ năng bỏ qua không sử dụng Buff
        /// </summary>
        private readonly HashSet<int> IgnoreBuffSkillID = new HashSet<int>()
        {

        };

        /// <summary>
        /// Trả về ID kỹ năng Buff chính của kỹ năng tương ứng
        /// </summary>
        /// <param name="skillID"></param>
        /// <param name="totalTries"></param>
        /// <returns></returns>
        private int GetBuffID(int skillID, int totalTries = 0)
        {
            /// Thông tin kỹ năng tương ứng
            if (!Loader.Loader.Skills.TryGetValue(skillID, out SkillDataEx skillData))
            {
                /// Không tìm thấy thì bỏ qua
                return -1;
            }

            /// Nếu đã quá số lần thử
            if (totalTries >= 10)
            {
                /// Không tìm thấy
                return -1;
            }

            /// Có kỹ năng con thì duyệt
            if (!skillData.IsBullet)
            {
                /// ID buff tương ứng
                int buffID = this.GetBuffID(skillData.BulletID, totalTries++);
                /// Vào kỹ năng con
                if (buffID != -1)
                {
                    /// Trả về kết quả nếu tìm thấy
                    return buffID;
                }
            }

            /// Vào kỹ năng con Start
            if (this.GetBuffID(skillData.StartSkillID, totalTries++) != -1)
            {
                /// ID buff tương ứng
                int buffID = this.GetBuffID(skillData.StartSkillID, totalTries++);
                /// Trả về kết quả nếu tìm thấy
                return buffID;
            }

            /// Vào kỹ năng con Collide
            if (this.GetBuffID(skillData.FinishSkillID, totalTries++) != -1)
            {
                /// ID buff tương ứng
                int buffID = this.GetBuffID(skillData.FinishSkillID, totalTries++);
                /// Trả về kết quả nếu tìm thấy
                return buffID;
            }

            /// Vào kỹ năng con Vanish
            if (this.GetBuffID(skillData.VanishSkillID, totalTries++) != -1)
            {
                /// ID buff tương ứng
                int buffID = this.GetBuffID(skillData.VanishSkillID, totalTries++);
                /// Trả về kết quả nếu tìm thấy
                return buffID;
            }

            /// Nếu đây là kỹ năng Buff
            if (skillData.Type == 2)
            {
                /// Trả về kết quả
                return skillID;
            }

            /// Không tìm thấy
            return -1;
        }

        /// <summary>
        /// Tự động kích hoạt các Buff chủ động
        /// </summary>
        private IEnumerator AutoActivatePositiveBuffs()
        {
            /// Nếu chưa đến thời gian kiểm tra thì bỏ qua
            if (KTGlobal.GetCurrentTimeMilis() - this.AutoFightLastCheckAutoActivatePositiveBuffs < this.AutoFight_CheckAndAutoActivatePositiveBuffs)
            {
                yield break; 
            }

            /// Đánh dấu thời gian kiểm tra tự động kích hoạt các Buff chủ động
            this.AutoFightLastCheckAutoActivatePositiveBuffs = KTGlobal.GetCurrentTimeMilis();

            /// Duyệt danh sách kỹ năng, tìm ra các Buff chủ động tương ứng
            foreach (SkillData skill in Global.Data.RoleData.SkillDataList)
            {
                /// Nếu chưa học kỹ năng này
                if (skill.Level <= 0)
                {
                    /// Bỏ qua
                    continue;
                }

                /// Thông tin kỹ năng
                if (Loader.Loader.Skills.TryGetValue(skill.SkillID, out SkillDataEx skillData))
                {
                    /// Nếu không thỏa mãn điều kiện có thể dùng kỹ năng này
                    if (!SkillManager.IsAbleToUseSkill(skillData, skill.Level, false))
                    {
                        /// Bỏ qua
                        continue;
                    }

                    /// Nếu là Từ Hàng Phổ Độ
                    if (skill.SkillID == KTAutoFightManager.EM_HealHPSkillID)
                    {
                        /// Bỏ qua
                        continue;
                    }
                    /// Nếu đang trong trạng thái phục hồi
                    if (skill.IsCooldown)
                    {
                        /// Bỏ qua
                        continue;
                    }

                    /// Nếu là kỹ năng không dùng
                    if (this.IgnoreBuffSkillID.Contains(skill.SkillID))
                    {
                        /// Bỏ qua
                        continue;
                    }

                    /// ID buff tương ứng
                    int buffID = this.GetBuffID(skill.SkillID);
                    /// Không phải Buff
                    if (buffID == -1)
                    {
                        /// Bỏ qua
                        continue;
                    }

                    /// Buff tương ứng
                    BufferData existBuff = Global.Data.RoleData.BufferDataList.Where(x => x.BufferID == buffID).FirstOrDefault();
                   
                    /// Nếu vẫn tồn tại
                    if (existBuff != null)
                    {
                        /// Bỏ qua
                        continue;
                    }

                    /// Thông tin kỹ năng Buff
                    SkillDataEx buffSkillData = Loader.Loader.Skills[buffID];

                    KTDebug.LogError(buffSkillData.Name);

                    /// Nếu là kỹ năng Buff, và chưa có hiệu ứng trên người thêm điều kiện cho Nga my khỏi sử dụng skill bữa bãi khi đầy máu vẫn tự buff
                    if (buffSkillData.TargetType == "self" || buffSkillData.TargetType == "team")
                    {
                       // this.StopAutoFight();
                        /// Mục tiêu
                        /// 
                        yield return new WaitForSeconds(1f);
                        GSprite tmp = SkillManager.SelectedTarget;
                        /// Thiết lập không có mục tiêu
                        SkillManager.SelectedTarget = null;
                        /// Dùng kỹ năng Buff
                        KTGlobal.AddNotification("Tự sử dụng buff: " + skillData.Name);

                        /// Sử dụng kỹ năng này
                        SkillManager.LeaderUseSkill(skill.SkillID, false);
                        /// Trả lại mục tiêu cũ
                        SkillManager.SelectedTarget = tmp;

                       // this.StartAuto();
                        /// Thoát luôn vì mỗi lần client chỉ cho dùng 1 kỹ năng duy nhất
                        break;
                    }
                }
            }
        }
        #endregion
    }
}