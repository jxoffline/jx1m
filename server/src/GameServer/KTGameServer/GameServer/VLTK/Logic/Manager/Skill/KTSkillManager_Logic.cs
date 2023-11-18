using GameServer.Logic;

namespace GameServer.KiemThe.Logic
{
	/// <summary>
	/// Quản lý Logic của kỹ năng
	/// </summary>
	public static partial class KTSkillManager
    {
        /// <summary>
        /// Thêm kết quả kỹ năng vào danh sách Packet gửi đi
        /// </summary>
        /// <param name="caster"></param>
        /// <param name="target"></param>
        /// <param name="type"></param>
        /// <param name="damage"></param>
        public static void AppendSkillResult(GameObject caster, GameObject target, SkillResult type, int damage)
        {
            /// Nếu toác
            if (caster == null || target == null)
			{
                return;
			}

            /// Nếu đối tượng ra chiêu là người chơi
            if (caster is KPlayer casterPlayer)
			{
                //KT_TCPHandler.SendSkillResultToMySelf(casterPlayer, caster, target, type, damage);
                /// Thêm sát thương vào
                casterPlayer.AddDamageDealt(target, type, damage, null);
            }
            /// Nếu đối tượng ra chiêu là pet của người chơi
            if (caster is Pet casterPet)
            {
                //KT_TCPHandler.SendSkillResultToMySelf(casterPet.Owner, caster, target, type, damage);
                /// Thêm sát thương vào
                casterPet.Owner.AddDamageDealt(target, type, damage, casterPet);
            }

            /// Nếu đối tượng bị sát thương là người chơi
            if (target is KPlayer targetPlayer)
            {
                //KT_TCPHandler.SendSkillResultToMySelf(targetPlayer, caster, target, type, damage);
                /// Thêm sát thương vào
                targetPlayer.AddReceiveDamage(caster, type, damage, null, null);
            }
            /// Nếu đối tượng bị sát thương là pet của người chơi
            if (target is Pet targetPet)
            {
                //KT_TCPHandler.SendSkillResultToMySelf(targetPet.Owner, caster, target, type, damage);
                /// Thêm sát thương vào
                targetPet.Owner.AddReceiveDamage(caster, type, damage, targetPet, null);
            }
            /// Nếu đối tượng bị sát thương là xe tiêu của người chơi
            if (target is TraderCarriage targetCarriage)
            {
                //KT_TCPHandler.SendSkillResultToMySelf(carriage.Owner, caster, target, type, damage);
                /// Thêm sát thương vào
                targetCarriage.Owner.AddReceiveDamage(caster, type, damage, null, targetCarriage);
            }
        }
    }
}
