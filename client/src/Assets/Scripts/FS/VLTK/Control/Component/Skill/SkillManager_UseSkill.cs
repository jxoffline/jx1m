//#define Use_Parabol_On_Jump

using GameServer.VLTK.Utilities;
using FS.GameEngine.Logic;
using FS.GameEngine.Network;
using FS.GameEngine.Sprite;
using FS.VLTK.Entities;
using FS.VLTK.Entities.Config;
using FS.VLTK.Factory;
using FS.VLTK.Loader;
using FS.VLTK.Logic;
using FS.VLTK.Logic.Settings;
using FS.VLTK.Network.Skill;
using Server.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static FS.VLTK.Entities.Enum;

namespace FS.VLTK.Control.Component
{
    /// <summary>
    /// Quản lý kỹ năng
    /// </summary>
    public static partial class SkillManager
    {
        /// <summary>
        /// Đánh dấu có đang đợi dùng kỹ năng không
        /// </summary>
        public static bool IsWaitingToUseSkill { get; set; } = false;

        /// <summary>
		/// Đối tượng đang tấn công hiện tại
		/// </summary>
		public static GSprite SelectedTarget { get; set; }

        /// <summary>
        /// Kỹ năng hiện tại
        /// </summary>
        public static int CurrentWaitingSkillID { get; private set; } = -1;

        /// <summary>
        /// Mục tiêu trước đó
        /// </summary>
        public static GSprite LastWaitingTarget { get; set; } = null;

        /// <summary>
        /// Có phải kỹ năng không cần mục tiêu
        /// </summary>
        /// <param name="skillData"></param>
        /// <returns></returns>
        public static bool IsSkillNoNeedTarget(SkillDataEx skillData)
        {
            /// Nếu là kỹ năng không cần mục tiêu
            if (skillData.IsSkillNoTarget)
            {
                return true;
            }
            else if (skillData.Type == 4)
            {
                return true;
            }
            /// Nếu là kỹ năng tác động lên bản thân mà không phải kỹ năng tấn công
            else if (skillData.Type != 5 && skillData.TargetType != "enemy" && skillData.TargetType != "ally" && skillData.TargetType != "revivable")
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Trả về thời gian phục hồi của kỹ năng tương ứng
        /// </summary>
        /// <param name="skillData"></param>
        /// <param name="level"></param>
        /// <returns></returns>
        public static int GetSkillCooldown(SkillDataEx skillData, int level)
        {
            /// Thời gian phục hồi kỹ năng
            int skillCooldownTime = 0;
            /// ProDict kỹ năng
            PropertyDictionary skillPD = skillData.Properties[(byte) level];
            /// Nếu có Symbol thời gian phục hồi
            if (skillPD.ContainsKey((int) MAGIC_ATTRIB.magic_skill_mintimepercast_v))
            {
                skillCooldownTime = skillPD.Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_skill_mintimepercast_v).nValue[0] * 1000 / 18;
            }
            /// Trả về kết quả
            return skillCooldownTime;
        }

        /// <summary>
        /// Kiểm tra điều kiện (môn phái, hệ phái, loại vũ khí) thỏa mãn trước khi dùng kỹ năng
        /// </summary>
        /// <param name="skillData"></param>
        /// <param name="notification"></param>
        /// <returns></returns>
        public static bool ValidateUseSkill(SkillDataEx skillData, bool notification = true)
        {
            /// Nếu là kỹ năng bị động thì không được dùng
            if (skillData.Type == 3)
            {
                if (notification)
                {
                    KTGlobal.AddNotification("Kỹ năng này không thể sử dụng!");
                }
                return false;
            }

            /// Kiểm tra loại vũ khí
            if (!skillData.WeaponLimit.Contains((int) KE_EQUIP_WEAPON_CATEGORY.emKEQUIP_WEAPON_CATEGORY_ALL))
            {
                /// Vũ khí 
                GoodsData weapon = KTGlobal.GetEquipData(Global.Data.RoleData, KE_EQUIP_POSITION.emEQUIPPOS_WEAPON);
                /// Nếu không có vũ khí thì mặc định là triền thủ
                if (weapon == null)
                {
                    /// Nếu vũ khí không phù hợp
                    if (!skillData.WeaponLimit.Contains((int) KE_EQUIP_WEAPON_CATEGORY.emKEQUIP_WEAPON_CATEGORY_HAND))
                    {
                        if (notification)
                        {
                            List<string> weapons = new List<string>();
                            foreach (int weaponCategory in skillData.WeaponLimit)
                            {
                                weapons.Add(KTGlobal.GetWeaponKind(weaponCategory));
                            }
                            string weaponRequireText = string.Join(", ", weapons);
                            KTGlobal.AddNotification("Kỹ năng này yêu cầu vũ khí " + weaponRequireText + "!");
                        }
                        return false;
                    }
                }
                else
                {
                    /// Thông tin trang bị tương ứng
                    if (Loader.Loader.Items.TryGetValue(weapon.GoodsID, out ItemData itemData))
                    {
                        /// Nếu vũ khí không phù hợp
                        if (!skillData.WeaponLimit.Contains(itemData.Category))
                        {
                            if (notification)
                            {
                                List<string> weapons = new List<string>();
                                foreach (int weaponCategory in skillData.WeaponLimit)
                                {
                                    weapons.Add(KTGlobal.GetWeaponKind(weaponCategory));
                                }
                                string weaponRequireText = string.Join(", ", weapons);
                                KTGlobal.AddNotification("Kỹ năng này yêu cầu vũ khí " + weaponRequireText + "!");
                            }
                            return false;
                        }
                    }
                } 
            }

            /// Kiểm tra môn phái có phù hợp không
            if (skillData.FactionID != 0)
            {
                /// Nếu môn phái không phù hợp
                if (Global.Data.RoleData.FactionID != skillData.FactionID)
                {
                    if (notification)
                    {
                        KTGlobal.AddNotification("Môn phái không thích hợp!");
                    }
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Đủ điều kiện dùng kỹ năng không
        /// </summary>
        /// <param name="skillData"></param>
        /// <param name="level"></param>
        /// <param name="notification"></param>
        /// <returns></returns>
        public static bool IsAbleToUseSkill(SkillDataEx skillData, int level, bool notification = true)
        {
            if (skillData.Properties.TryGetValue((byte) level, out PropertyDictionary levelPd))
            {
                /// Kiểm tra có đủ nội lực không
                if (levelPd.ContainsKey((int) MAGIC_ATTRIB.magic_skill_cost_v))
                {
                    /// Số điểm cần
                    int pointNeed = levelPd.Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_skill_cost_v).nValue[0];

                    /// Nếu là mất sinh lực
                    if (skillData.SkillCostType == 0)
                    {
                        if (Global.Data.RoleData.CurrentHP < pointNeed)
                        {
                            if (notification)
                            {
                                KTGlobal.AddNotification("Sinh lực không đủ!");
                            }
                            return false;
                        }
                    }
                    /// Nếu là mất nội lực
                    else if (skillData.SkillCostType == 1)
                    {
                        if (Global.Data.RoleData.CurrentMP < pointNeed)
                        {
                            if (notification)
                            {
                                KTGlobal.AddNotification("Nội lực không đủ!");
                            }
                            return false;
                        }
                    }
                    /// Nếu là mất thể lực
                    else if (skillData.SkillCostType == 2)
                    {
                        if (Global.Data.RoleData.CurrentStamina < pointNeed)
                        {
                            if (notification)
                            {
                                KTGlobal.AddNotification("Thể lực không đủ!");
                            }
                            return false;
                        }
                    }
                }
            }
            else
            {
                if (notification)
                {
                    KTGlobal.AddNotification("Dữ liệu kỹ năng bị lỗi!");
                }
                return false;
            }

            /// Có thể dùng kỹ năng
            return SkillManager.ValidateUseSkill(skillData, notification);
        }

        /// <summary>
        /// Leader sử dụng kỹ năng
        /// </summary>
        /// <param name="skillID">ID kỹ năng</param>
        /// <param name="findNearestTarget">Tự tìm mục tiêu gần nhất</param>
        /// <param name="false">Chế độ im lặng, không thông báo</param>
        public static bool LeaderUseSkill(int skillID, bool findNearestTarget = false, bool ignoreTarget = false, bool silentMode = false, bool isSubPhrase = false)
        {
            /// Nếu không có Leader thì bỏ qua
            if (Global.Data.Leader == null)
            {
                /// Đánh dấu đang đợi dùng kỹ năng
                SkillManager.IsWaitingToUseSkill = false;
                return false;
            }
            /// Nếu Leader đã chết thì bỏ qua
            else if (Global.Data.Leader.IsDeath)
            {
                /// Đánh dấu đang đợi dùng kỹ năng
                SkillManager.IsWaitingToUseSkill = false;

                if (!silentMode)
                {
                    KTGlobal.AddNotification("Các hạ đã tử nạn, không thể sử dụng kỹ năng!");
                }
                return false;
            }
            /// Nếu đang trong trạng thái bán hàng thì không cho dùng kỹ năng
            else if (Global.Data.StallDataItem != null && Global.Data.StallDataItem.Start == 1 && !Global.Data.StallDataItem.IsBot)
            {
                /// Đánh dấu đang đợi dùng kỹ năng
                SkillManager.IsWaitingToUseSkill = false;

                if (!silentMode)
                {
                    KTGlobal.AddNotification("Trong trạng thái bán hàng không thể sử dụng kỹ năng!");
                }
                return false;
            }
            /// Nếu chưa thực hiện xong động tác trước đó
            else if (!Global.Data.Leader.IsReadyToMove)
            {
                return false;
            }
            /// Nếu chưa đến thời gian dùng kỹ năng
            else if (!KTGlobal.FinishedUseSkillAction)
            {
                /// Đánh dấu đang đợi dùng kỹ năng
                SkillManager.IsWaitingToUseSkill = false;

                return false;
            }
            /// Nếu đang đợi dùng kỹ năng
            else if (SkillManager.IsWaitingToUseSkill)
            {
                return false;
            }
            /// Nếu trùng SkillID và trùng mục tiêu và thêm điều kiện là nhân vật đang chờ di chuyển tới mục tiêu để vã thì mới retrrun ở đây nếu ko thì thực hiện tiếp vã luôn
            else if (SkillManager.CurrentWaitingSkillID == skillID && SkillManager.LastWaitingTarget == SkillManager.SelectedTarget && SkillManager.LastWaitingTarget != null && Global.Data.Leader.IsMoving)
            {
                return false;
            }

            /// Kiểm tra nếu Leader đang bị khóa, không thể sử dụng kỹ năng
            if (!Global.Data.Leader.CanDoLogic)
            {
                /// Đánh dấu đang đợi dùng kỹ năng
                SkillManager.IsWaitingToUseSkill = false;

                if (!silentMode)
                {
                    KTGlobal.AddNotification("Bạn đang trong trạng thái bị khống chế, không thể sử dụng kỹ năng!");
                }
                return false;
            }

            /// Hủy trạng thái tự tìm đường
            KTLeaderMovingManager.StopMoveImmediately();
            /// Ngừng AUto tự tìm đường
            AutoPathManager.Instance.StopAutoPath();
            /// Ngừng tự đốt lửa trại
            KTAutoFightManager.Instance.StopAutoFireCamp();
            /// Ngừng việc về thành bán đồ
            KTAutoFightManager.Instance.StopAutoSell();

            KTAutoFightManager.Instance.StopAutoBuyItem();

            /// Kiểm tra kỹ năng có tồn tại không
            if (!Loader.Loader.Skills.TryGetValue(skillID, out SkillDataEx skillData))
            {
                /// Đánh dấu đang đợi dùng kỹ năng
                SkillManager.IsWaitingToUseSkill = false;

                if (!silentMode)
                {
                    KTGlobal.AddNotification("Kỹ năng không tồn tại!");
                }
                return false;
            }

            /// Nếu kỹ năng đang trong trạng thái phục hồi thì không sử dụng
            if (SkillManager.IsSkillCooldown(skillID))
            {
                /// Đánh dấu đang đợi dùng kỹ năng
                SkillManager.IsWaitingToUseSkill = false;

                if (!silentMode)
                {
                    KTGlobal.AddNotification("Kỹ năng đang trong trạng thái phục hồi!");
                }

                return false;
            }

            /// Cấp độ hiện tại của kỹ năng
            SkillData skillDataGS = Global.Data.RoleData.SkillDataList.Where(x => x.SkillID == skillData.ID).FirstOrDefault();
            if (skillDataGS == null || !skillData.Properties.TryGetValue(skillDataGS.Level, out _))
            {
                /// Đánh dấu đang đợi dùng kỹ năng
                SkillManager.IsWaitingToUseSkill = false;

                if (!silentMode)
                {
                    KTGlobal.AddNotification("Dữ liệu kỹ năng không tồn tại!");
                }
                return false;
            }
            int skillLevel = skillDataGS.Level;

            /// Kiểm tra có đủ điều kiện để dùng kỹ năng không
            if (!SkillManager.IsAbleToUseSkill(skillData, skillLevel, !silentMode))
            {
                /// Đánh dấu đang đợi dùng kỹ năng
                SkillManager.IsWaitingToUseSkill = false;
                return false;
            }

            /// Nếu là kỹ năng không cần mục tiêu
            if (SkillManager.IsSkillNoNeedTarget(skillData))
            {
                /// Đánh dấu đang đợi dùng kỹ năng
                SkillManager.IsWaitingToUseSkill = true;
                /// Gửi yêu cầu dùng kỹ năng
                KTTCPSkillManager.SendUseSkill(skillID, true);

                return true;
            }

            /// Nếu bỏ qua mục tiêu
            if (ignoreTarget)
            {
                /// Đánh dấu đang đợi dùng kỹ năng
                SkillManager.IsWaitingToUseSkill = true;
                /// Gửi yêu cầu dùng kỹ năng
                KTTCPSkillManager.SendUseSkill(skillID, true);

                return true;
            }

            /// ProDict của kỹ năng
            PropertyDictionary skillPd = skillData.Properties[skillDataGS.Level];

            /// Phạm vi tấn công của kỹ năng
            int skillCastRange = skillData.AttackRadius;
            if (skillPd.ContainsKey((int) MAGIC_ATTRIB.magic_skill_attackradius))
            {
                skillCastRange = skillPd.Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_skill_attackradius).nValue[0];
            }

            /// Thiết lập kỹ năng hiện tại
            SkillManager.CurrentWaitingSkillID = skillID;

            /// Mục tiêu
            GSprite target = SkillManager.SelectedTarget;

            /// Nếu trước đó đã có mục tiêu, thì kiểm tra xem mục tiêu đó còn tồn tại không
            bool targetMustBeAlive = skillData.TargetType != "revivable";
            bool targetMustBeEnemy = skillData.TargetType != "revivable";
            /// Nếu mục tiêu cũ vẫn tồn tại
            if (SkillManager.IsValidTarget(target, targetMustBeEnemy, targetMustBeAlive))
            {
                /// Đánh dấu Target NULL
                target = null;
                /// Nếu không phải chế độ đánh quái mà là chế độ giết người
                if (KTAutoAttackSetting.Config.EnableAutoPK)
                {
                    /// Tìm mục tiêu là người chơi gần nhất
                    target = SkillManager.FindNearestEnemy(Global.Data.Leader, (sprite) => {
                        /// Nếu không phải người chơi thì thôi
                        if (!Global.Data.OtherRoles.TryGetValue(sprite.RoleID, out _))
                        {
                            return false;
                        }
                        return !sprite.IsDeath && SkillManager.GetTargetTrueHP(sprite) > 0;
                    });
                }

                /// Nếu không có mục tiêu thì giữ mục tiêu cũ
                if (target == null)
                {
                    target = SkillManager.SelectedTarget;
                }

                if (SkillManager.IsValidTarget(target, targetMustBeEnemy, targetMustBeAlive))
                {
                    SkillManager.SelectedTarget = target;
                }
                else
                {
                    target = null;
                }
            }
            /// Nếu mục tiêu cũ không tồn tại nhưng hiện tại đang để chế độ tự tìm mục tiêu gần nhất
            else if (findNearestTarget && skillData.Type != 2)
            {
                /// Đánh dấu Target NULL
                target = null;
                /// Nếu có ưu tiên đánh người
                if (KTAutoAttackSetting.Config.EnableAutoPK)
                {
                    /// Tìm mục tiêu là người chơi gần nhất
                    target = SkillManager.FindNearestEnemy(Global.Data.Leader, (sprite) => {
                        /// Nếu không phải người chơi thì thôi
                        if (!Global.Data.OtherRoles.TryGetValue(sprite.RoleID, out _))
                        {
                            return false;
                        }
                        return !sprite.IsDeath && SkillManager.GetTargetTrueHP(sprite) > 0;
                    });
                }

                /// Nếu không có mục tiêu thì tìm
                if (target == null)
                {
                    target = SkillManager.FindNearestEnemy(Global.Data.Leader, (sprite) => {
                        return !sprite.IsDeath && SkillManager.GetTargetTrueHP(sprite) > 0;
                    });
                }

                if (SkillManager.IsValidTarget(target, targetMustBeEnemy, targetMustBeAlive))
                {
                    SkillManager.SelectedTarget = target;
                }
                else
                {
                    target = null;
                }
            }
            /// Nếu không có mục tiêu thỏa mãn
            else if (targetMustBeEnemy)
            {
                target = null;
            }

            /// Nếu có mục tiêu
            if (target != null)
            {
                /// Thiết lập chọn mục tiêu cho pet
                if (targetMustBeEnemy)
                {
                    KTAutoPetManager.Instance.Target = target;
                }

                /// Hiển thị ký hiệu chọn mục tiêu
                Global.Data.GameScene.SetSelectTarget(target);
                /// Thông báo hiển thị khung mặt đối tượng
                PlayZone.Instance.NotifyRoleFace(target.RoleID, -1, true);

                /// Vị trí hiện tại của Leader
                Vector2 casterPos = Global.Data.Leader.PositionInVector2;

                /// Khoảng cách đến chỗ mục tiêu
                float distanceToTarget = Vector2.Distance(casterPos, target.PositionInVector2);

                /// Nếu mục tiêu nằm ngoài phạm vi đánh của kỹ năng thì tiến hành di chuyển đến chỗ mục tiêu
                if (distanceToTarget > skillCastRange + (isSubPhrase ? 100 : 0))
                {
                    /// Kiểm tra nếu Leader đang bị khóa, không thể di chuyển
                    if (!Global.Data.Leader.CanPositiveMove)
                    {
                        /// Đánh dấu đang đợi dùng kỹ năng
                        SkillManager.IsWaitingToUseSkill = false;

                        //KTGlobal.AddNotification("Bạn đang trong trạng thái bị khống chế, không thể di chuyển đến vị trí sử dụng kỹ năng!");
                        return false;
                    }

                    /// Đánh dấu mục tiêu trước đó đang đợi để thực thi kỹ năng
                    SkillManager.LastWaitingTarget = target;

                    /// Thực hiện đuổi mục tiêu
                    KTLeaderMovingManager.ChaseTarget(target, skillCastRange - 20, () => {
                        /// Đánh dấu mục tiêu trước đó đang đợi để thực thi kỹ năng
                        SkillManager.LastWaitingTarget = null;

                        /// Gọi lại hàm sử dụng kỹ năng
                        SkillManager.LeaderUseSkill(SkillManager.CurrentWaitingSkillID, findNearestTarget, false, false, true);
                    });
                }
                /// Nếu mục tiêu đã nằm trong phạm vi thì tiến hành sử dụng kỹ năng
                else
                {
                    /// Ngừng đuổi mục tiêu
                    KTLeaderMovingManager.StopChasingTarget();

                    /// Nếu vượt quá phạm vi thiết lập của kỹ năng thì xuất chiêu ở khoảng không
                    if (distanceToTarget > skillCastRange)
                    {
                        /// Bỏ đánh dấu mục tiêu
                        SkillManager.SelectedTarget = null;
                    }

                    /// Đánh dấu đang đợi dùng kỹ năng
                    SkillManager.IsWaitingToUseSkill = true;

                    /// Sử dụng kỹ năng
                    KTTCPSkillManager.SendUseSkill(skillID);
                }
            }
            /// Nếu không tìm thấy mục tiêu thì gửi yêu cầu dùng kỹ năng theo hướng
            else
            {

                //  KTGlobal.AddNotification("CLEAR TAGET");
                ///// Đánh dấu đang đợi dùng kỹ năng
                //SkillManager.IsWaitingToUseSkill = true;

                /// Ngừng đuổi mục tiêu
                KTLeaderMovingManager.StopChasingTarget();

                /// Xóa mục tiêu được chọn
                SkillManager.SelectedTarget = null;

                ///// Sử dụng kỹ năng
                //KTTCPSkillManager.SendUseSkill(skillID);

                KTGlobal.AddNotification("Không tìm thấy mục tiêu xung quanh!");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Đối tượng sử dụng kỹ năng
        /// </summary>
        /// <param name="sprite"></param>
        /// <param name="direction"></param>
        /// <param name="skillID"></param>
        /// <param name="isSpecialAttack"></param>
        public static void SpriteUseSkill(GSprite sprite, Direction direction, int skillID, bool isSpecialAttack)
        {
            /// Nếu không có thành phần biểu diễn thì bỏ qua
            if (sprite.Role2D == null)
            {
                return;
            }

            /// Nếu đối tượng đã chết thì bỏ qua
            if (sprite.IsDeath)
            {
                return;
            }

            /// Nếu tìm thấy dữ liệu kỹ năng thì tiến hành quay hướng đối tượng và play động tác xuất chiêu
            if (Loader.Loader.Skills.TryGetValue(skillID, out SkillDataEx skill))
            {
                Monster monster = sprite.Role2D.GetComponent<Monster>();
                Character character = sprite.Role2D.GetComponent<Character>();

                /// ID hiệu ứng ra chiêu
                int skillCastID = skill.CastEffectID;

                /// Cập nhật hướng của đối tượng
                sprite.Direction = direction;

                /// Nếu đối tượng là quái hoặc pet
                if (monster != null)
                {
                    if (!KTSystemSetting.HideSkillCastEffect)
                    {
                        monster.AddEffect(skillCastID, EffectType.CastEffect);
                    }

                    /// Nếu đây là kỹ năng đánh liên tiếp bỏ qua tốc đánh
                    if (skill.FixedAttackActionCount != -1)
                    {
                        if (isSpecialAttack)
                        {
                            sprite.DoSpecialAttackMultipleTimes(skill.FixedAttackActionCount);
                        }
                        else
                        {
                            sprite.DoAttackMultipleTimes(skill.FixedAttackActionCount);
                        }
                    }
                    else
                    {
                        if (isSpecialAttack)
                        {
                            sprite.DoSpecialAttack(skill.IsPhysical, skill.IsSkillNoAddAttackSpeedCooldown);
                        }
                        else
                        {
                            sprite.DoNormalAttack(skill.IsPhysical, skill.IsSkillNoAddAttackSpeedCooldown);
                        }
                    }

                    /// Nếu là Pet
                    if (sprite.PetData != null && Global.Data.RoleData.CurrentPetID + (int) ObjectBaseID.Pet == sprite.PetData.ID)
                    {
                        /// Nếu là kỹ năng ảnh hưởng đến tốc đánh
                        if (!skill.IsSkillNoAddAttackSpeedCooldown)
                        {
                            /// Cập nhật thời điểm dùng Skill lần cuối
                            KTAutoPetManager.Instance.LastUseSkillTick = KTGlobal.GetCurrentTimeMilis();
                            /// Cập nhật ID kỹ năng dùng lần cuối
                            KTAutoPetManager.Instance.LastUseSkillID = skillID;
                        }
                        /// Nếu là kỹ năng không ảnh hưởng đến tốc đánh
						else
                        {
                            /// Cập nhật thời điểm dùng
                            KTAutoPetManager.Instance.LastUseSkillNoAffectAtkSpeedTick = KTGlobal.GetCurrentTimeMilis();
                        }

                        /// Hủy đánh dấu đang đợi dùng kỹ năng
                        KTAutoPetManager.Instance.IsWaitingToUseSkill = false;
                    }
                }
                /// Nếu đối tượng là người chơi
                else if (character != null)
                {
                    if (!KTSystemSetting.HideSkillCastEffect)
                    {
                        character.AddEffect(skillCastID, EffectType.CastEffect);
                    }

                    switch (skill.CastActionID)
                    {
                        case 9:
                            if (sprite.CurrentAction != KE_NPC_DOING.do_jump && sprite.CurrentAction != KE_NPC_DOING.do_runattack)
                            {
                                /// Nếu đây là kỹ năng đánh liên tiếp bỏ qua tốc đánh
                                if (skill.FixedAttackActionCount != -1)
                                {
                                    if (isSpecialAttack)
                                    {
                                        sprite.DoSpecialAttackMultipleTimes(skill.FixedAttackActionCount);
                                    }
                                    else
                                    {
                                        sprite.DoAttackMultipleTimes(skill.FixedAttackActionCount);
                                    }
                                }
                                else
                                {
                                    if (isSpecialAttack)
                                    {
                                        sprite.DoSpecialAttack(skill.IsPhysical, skill.IsSkillNoAddAttackSpeedCooldown);
                                    }
                                    else
                                    {
                                        sprite.DoNormalAttack(skill.IsPhysical, skill.IsSkillNoAddAttackSpeedCooldown);
                                    }
                                }
                            }
                            break;
                        case 11:
                            if (sprite.CurrentAction != KE_NPC_DOING.do_jump && sprite.CurrentAction != KE_NPC_DOING.do_runattack)
                            {
                                sprite.DoMagic(skill.IsPhysical, skill.IsSkillNoAddAttackSpeedCooldown);
                            }
                            break;
                    }

                    string soundName = ((Sex) character.Data.RoleSex == Sex.MALE ? skill.MaleCastSound : skill.FemaleCastSound);
                    if (!string.IsNullOrEmpty(soundName))
                    {
                        PlayZone.Instance.StartCoroutine(KTResourceManager.Instance.LoadAssetAsync<AudioClip>(Loader.Loader.SkillCastSoundBundleDir, soundName, false, (isNewLoad) => {
                            AudioClip sound = KTResourceManager.Instance.GetAsset<AudioClip>(Loader.Loader.SkillCastSoundBundleDir, soundName);
                            SkillManager.CreateStandaloneAudioPlayer(soundName, sound);
                        }));
                    }

                    /// Nếu là Leader
                    if (Global.Data.Leader == sprite)
                    {
                        /// Nếu là kỹ năng ảnh hưởng đến tốc đánh
                        if (!skill.IsSkillNoAddAttackSpeedCooldown)
                        {
                            /// Cập nhật thời điểm dùng Skill lần cuối
                            KTGlobal.LastUseSkillTick = KTGlobal.GetCurrentTimeMilis();
                            /// Cập nhật ID kỹ năng dùng lần cuối
                            KTGlobal.LastUseSkillID = skillID;
                        }
                        /// Nếu là kỹ năng không ảnh hưởng đến tốc đánh
						else
                        {
                            /// Cập nhật thời điểm dùng
                            KTGlobal.LastUseSkillNoAffectAtkSpeedTick = KTGlobal.GetCurrentTimeMilis();
                        }

                        /// Hủy đánh dấu đang đợi dùng kỹ năng
                        SkillManager.IsWaitingToUseSkill = false;
                    }
                }
            }
        }

        /// <summary>
        /// Thực hiện hiệu ứng tốc biến của đối tượng
        /// </summary>
        /// <param name="sprite"></param>
        /// <param name="position"></param>
        /// <param name="duration"></param>
        public static void SpriteBlinkToPosition(GSprite sprite, Vector2 position, float duration, bool isDragged = false)
        {
            /// Nếu không tìm thấy đối tượng hoặc vị trí đích trùng với vị trí hiện tại của đối tượng thì bỏ qua
            if (sprite == null || position == sprite.PositionInVector2)
            {
                return;
            }

            /// Nếu đang có luồng bay thì ngừng lại
            if (sprite.FlyingCoroutine != null)
            {
                PlayZone.Instance.StopCoroutine(sprite.FlyingCoroutine);
            }

            /// Nếu đối tượng đã chết thì bỏ qua
            if (sprite.IsDeath)
            {
                return;
            }

            Vector2 startPos = sprite.PositionInVector2;
            Vector2 dirVector = position - startPos;
            /// Hướng quay của đối tượng
            float rotationAngle = KTMath.GetAngle360WithXAxis(dirVector);
            sprite.Direction = KTMath.GetDirectionByAngle360(rotationAngle);

            /// Nếu bị đẩy lui hoặc kéo lại
            if (!isDragged)
            {
                /// Thực hiện động tác bay
                sprite.DoRunAttack(duration);
            }

            /// Thực hiện di chuyển đối tượng nhanh đến vị trí chỉ định
            IEnumerator DoPlayAnimation()
            {
                /// Thực hiện đổ bóng
                if (sprite.ComponentCharacter != null)
                {
                    sprite.ComponentCharacter.ActivateTrailEffect(true);
                }
                else if (sprite.ComponentMonster != null)
                {
                    sprite.ComponentMonster.ActivateTrailEffect(true);
                }
                /// Bỏ qua 1 Frame
                yield return null;

                float lifeTime = 0;
                while (true)
                {
                    lifeTime += Time.deltaTime;
                    if (lifeTime >= duration)
                    {
                        break;
                    }

                    float percent = lifeTime / duration;
                    Vector2 nextPos = startPos + dirVector * percent;
                    sprite.Coordinate = new Drawing.Point((int) nextPos.x, (int) nextPos.y);

                    yield return null;
                }
                /// Cập nhật vị trí của đối tượng
                sprite.Coordinate = new Drawing.Point((int) position.x, (int) position.y);

                /// Hủy đổ bóng
                if (sprite.ComponentCharacter != null)
                {
                    sprite.ComponentCharacter.ActivateTrailEffect(false);
                }
                else if (sprite.ComponentMonster != null)
                {
                    sprite.ComponentMonster.ActivateTrailEffect(false);
                }

                /// Thực hiện động tác đứng
                sprite.DoStand();

                /// Hủy luồng thực thi khinh công
                sprite.FlyingCoroutine = null;
            }

            sprite.FlyingCoroutine = PlayZone.Instance.StartCoroutine(DoPlayAnimation());
        }

        /// <summary>
        /// Thực hiện động tác khinh công
        /// </summary>
        /// <param name="sprite"></param>
        /// <param name="position"></param>
        /// <param name="duration"></param>
        public static void SpriteFlyToPosition(GSprite sprite, Vector2 position, float duration)
        {
            /// Nếu không tìm thấy đối tượng hoặc vị trí đích trùng với vị trí hiện tại của đối tượng thì bỏ qua
            if (sprite == null || position == sprite.PositionInVector2)
            {
                return;
            }

            /// Nếu đang có luồng bay thì ngừng lại
            if (sprite.FlyingCoroutine != null)
            {
                PlayZone.Instance.StopCoroutine(sprite.FlyingCoroutine);
            }

            /// Nếu đối tượng đã chết thì bỏ qua
            if (sprite.IsDeath)
            {
                return;
            }

            /// Nếu không phải người chơi thì bỏ qua
            if (sprite.ComponentCharacter == null)
            {
                return;
            }

            Vector2 startPos = sprite.PositionInVector2;
            Vector2 dirVector = position - startPos;
            /// Hướng quay của đối tượng
            float rotationAngle = KTMath.GetAngle360WithXAxis(dirVector);
            sprite.Direction = KTMath.GetDirectionByAngle360(rotationAngle);

            /// Thời gian bay
            sprite.ComponentCharacter.JumpDuration = duration;

            /// Thiết lập vị trí của Model
            sprite.ComponentCharacter.Model.transform.localPosition = sprite.ComponentCharacter.OriginModelPos;

            /// Thực hiện di chuyển đối tượng nhanh đến vị trí chỉ định
            IEnumerator DoPlayAnimation()
            {
                /// Thực hiện đổ bóng
                sprite.ComponentCharacter.ActivateTrailEffect(true);

#if Use_Parabol_On_Jump
                float high = Mathf.Min(150, Vector2.Distance(startPos, position) / 2);
                float vertexY = Mathf.Max(startPos.y, position.y) + high;
                KTMath.Parabol parabol = KTMath.GetParabolFromTwoPointsAndVertexY(startPos, position, vertexY, true);
#else
                float high = Mathf.Min(150, Vector2.Distance(startPos, position) / 2);
#endif

                /// Tạm thời để đánh dấu
                int frameSkipped = 0;

                float lifeTime = 0;
                while (true)
                {
                    frameSkipped++;
                    if (frameSkipped == 3)
                    {
                        /// Thực hiện động tác bay
                        sprite.DoFly(duration);
                    }

                    /// Nếu không phải người chơi thì bỏ qua
                    if (sprite == null || sprite.ComponentCharacter == null)
                    {
                        yield break;
                    }

                    lifeTime += Time.deltaTime;
                    if (lifeTime >= duration)
                    {
                        break;
                    }

                    if (sprite.Direction != sprite.ComponentCharacter.Direction)
                    {
                        sprite.ComponentCharacter.Direction = sprite.Direction;
                    }

                    float percent = lifeTime / duration;
                    Vector2 nextPos = startPos + dirVector * percent;

#if Use_Parabol_On_Jump
                    /// Nếu có Parabol
                    if (parabol != null)
                    {
                        float newY = parabol.A * nextPos.x * nextPos.x + parabol.B * nextPos.x + parabol.C;
                        nextPos.y = newY;
                    }
                    /// Nếu không có Parabol
                    else
                    {
                        float newY;
                        /// Nếu là pha nhảy lên
                        if (percent <= 0.5f)
                        {
                            newY = startPos.y + (vertexY - startPos.y) * percent * 2;
                        }
                        /// Nếu là pha đáp xuống
                        else
                        {
                            newY = position.y + (vertexY - position.y) * (1 - (percent - 0.5f) * 2);
                        }
                        nextPos.y = newY;
                    }
#else
                    float newY;
                    /// Nếu là pha nhảy lên
                    if (percent <= 0.5f)
                    {
                        newY = high * percent * 2;
                    }
                    /// Nếu là pha đáp xuống
                    else
                    {
                        newY = high * (1 - (percent - 0.5f) * 2);
                    }

                    /// Cập nhật vị trí Model
                    sprite.ComponentCharacter.Model.transform.localPosition = new Vector3(sprite.ComponentCharacter.OriginModelPos.x, sprite.ComponentCharacter.OriginModelPos.y + newY, sprite.ComponentCharacter.OriginModelPos.z);
#endif

                    sprite.Coordinate = new Drawing.Point((int) nextPos.x, (int) nextPos.y);

                    yield return null;
                }
                /// Cập nhật vị trí của đối tượng
                sprite.Coordinate = new Drawing.Point((int) position.x, (int) position.y);

                /// Cập nhật lại vị trí Model
                sprite.ComponentCharacter.Model.transform.localPosition = sprite.ComponentCharacter.OriginModelPos;

                /// Thực hiện động tác đứng
                sprite.DoStand();

                /// Hủy luồng thực thi khinh công
                sprite.FlyingCoroutine = null;

                /// Hủy đổ bóng
                sprite.ComponentCharacter.ActivateTrailEffect(false);
            }

            sprite.FlyingCoroutine = PlayZone.Instance.StartCoroutine(DoPlayAnimation());
        }
    }
}
