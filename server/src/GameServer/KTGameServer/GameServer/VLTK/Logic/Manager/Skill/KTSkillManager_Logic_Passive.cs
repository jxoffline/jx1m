using GameServer.KiemThe.Core.Item;
using GameServer.KiemThe.Entities;
using GameServer.Logic;
using Server.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.KiemThe.Logic
{
    /// <summary>
    /// Thực hiện Logic kỹ năng bị động
    /// </summary>
    public static partial class KTSkillManager
    {
        /// <summary>
        /// Thực hiện kỹ năng bị động
        /// </summary>
        /// <param name="caster">Đối tượng xuất chiêu</param>
        /// <param name="skill">Kỹ năng</param>
        private static void DoSkillPassive(GameObject caster, SkillLevelRef skill)
        {
            /// Nếu kỹ năng yêu cầu vũ khí
            if (caster is KPlayer player && !skill.Data.WeaponLimit.Contains((int) KE_EQUIP_WEAPON_CATEGORY.emKEQUIP_WEAPON_CATEGORY_ALL))
            {
                /// Vũ khí hiện tại
                GoodsData currentWeaponGD = player.GetPlayEquipBody().GetItemByPostion((int) KE_EQUIP_POSITION.emEQUIPPOS_WEAPON)?._GoodDatas;
                /// Nếu không tồn tại
                if (currentWeaponGD == null)
                {
                    /// Coi như là tay không
                    if (!skill.Data.WeaponLimit.Contains((int) KE_EQUIP_WEAPON_CATEGORY.emKEQUIP_WEAPON_CATEGORY_HAND))
                    {
                        /// Không thỏa mãn vũ khí yêu cầu
                        return;
                    }
                }
                else
                {
                    /// Loại vũ khí
                    if (ItemManager._TotalGameItem.TryGetValue(currentWeaponGD.GoodsID, out ItemData itemData))
                    {
                        /// Nếu vũ khí yêu cầu không phù hợp
                        if (!skill.Data.WeaponLimit.Contains(itemData.Category))
                        {
                            /// Không thỏa mãn vũ khí yêu cầu
                            return;
                        }
                    }
                }
            }

            BuffDataEx buff = new BuffDataEx()
            {
                Duration = -1,
                LoseWhenUsingSkill = false,
                Skill = skill,
                SaveToDB = false,
                StartTick = KTGlobal.GetCurrentTimeMilis(),
            };
            caster.Buffs.AddBuff(buff);
        }
    }
}
