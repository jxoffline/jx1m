using GameServer.KiemThe.Logic;
using GameServer.Logic;
using Server.Data;
using System;
using System.Collections.Generic;

namespace GameServer.KiemThe.Network.Entities
{
    /// <summary>
    /// Đối tượng xây gói tin kết quả kỹ năng gửi về Client
    /// </summary>
    public class SkillResultPacketBuilder : IDisposable
    {
        /// <summary>
        /// Danh sách các gói tin tương ứng được gửi đi
        /// </summary>
        private readonly List<SkillResult> packets = new List<SkillResult>();

        /// <summary>
        /// Tổng số phần tử
        /// </summary>
        public int Count
        {
            get
            {
                return this.packets.Count;
            }
        }

        /// <summary>
        /// Thêm gói tin kết quả kỹ năng vào danh sách
        /// </summary>
        /// <param name="caster"></param>
        /// <param name="target"></param>
        /// <param name="type"></param>
        /// <param name="damage"></param>
        /// <param name="pet"></param>
        /// <param name="traderCarriage"></param>
        public void AppendDealDamage(GameObject caster, GameObject target, KTSkillManager.SkillResult type, int damage, GameObject pet, GameObject traderCarriage)
        {
            if (caster == null || target == null)
            {
                return;
            }
            SkillResult skillResult = new SkillResult()
            {
                CasterID = caster.RoleID,
                TargetID = target.RoleID,
                Type = (int) type,
                Damage = damage,
                TargetCurrentHP = target.m_CurrentLife,
                PetID = pet == null ? -1 : pet.RoleID,
                TraderCarriageID = traderCarriage == null ? -1 : traderCarriage.RoleID,
            };
            this.packets.Add(skillResult);
        }

        /// <summary>
        /// Thêm gói tin kết quả kỹ năng vào danh sách
        /// </summary>
        /// <param name="caster"></param>
        /// <param name="target"></param>
        /// <param name="type"></param>
        /// <param name="damage"></param>
        /// <param name="pet"></param>
        /// <param name="traderCarriage"></param>
        public void AppendDamageTaken(GameObject caster, GameObject target, KTSkillManager.SkillResult type, int damage, GameObject pet, GameObject traderCarriage)
        {
            if (caster == null || target == null)
            {
                return;
            }
            SkillResult skillResult = new SkillResult()
            {
                CasterID = caster.RoleID,
                TargetID = target.RoleID,
                Type = (int) type,
                Damage = damage,
                TargetCurrentHP = pet != null ? pet.m_CurrentLife : traderCarriage != null ? traderCarriage.m_CurrentLife : target.m_CurrentLife,
                PetID = pet == null ? -1 : pet.RoleID,
                TraderCarriageID = traderCarriage == null ? -1 : traderCarriage.RoleID,
            };
            this.packets.Add(skillResult);
        }

        /// <summary>
        /// Xây danh sách gói tin
        /// </summary>
        /// <returns></returns>
        public List<SkillResult> Build()
        {
            return this.packets;
        }

        /// <summary>
        /// Hủy đối tượng
        /// </summary>
        public void Dispose()
		{
            this.packets.Clear();
		}
    }
}
