using GameServer.KiemThe.Entities;
using GameServer.VLTK.Entities.Pet;
using Server.Data;

namespace GameServer.Logic
{
    /// <summary>
    /// Đối tượng Pet
    /// </summary>
    public partial class Pet
    {
        /// <summary>
        /// Trả về dữ liệu Pet Mini gửi cho Client
        /// </summary>
        /// <returns></returns>
        public PetDataMini GetMiniData()
        {
            return new PetDataMini()
            {
                ID = this.RoleID,
                RoleID = this.Owner.RoleID,
                ResID = this.ResID,
                Name = this.RoleName,
                Title = this.Title,
                PosX = (int) this.CurrentPos.X,
                PosY = (int) this.CurrentPos.Y,
                Direction = (int) this.CurrentDir,
                HP = this.m_CurrentLife,
                MaxHP = this.m_CurrentLifeMax,
                MoveSpeed = this.Owner.GetCurrentRunSpeed(),
                AtkSpeed = this.GetCurrentAttackSpeed(),
                CastSpeed = this.GetCurrentCastSpeed(),
                Skills = this.Skills,
                Level = this.m_Level,
            };
        }

        /// <summary>
        /// Trả về dữ liệu Pet trong DB
        /// </summary>
        /// <returns></returns>
        public PetData GetDBData()
        {
            return new PetData()
            {
                ID = this.RoleID - (int) ObjectBaseID.Pet,
                RoleID = this.Owner.RoleID,
                Name = this.RoleName,
                Level = this.m_Level,
                Exp = (int)this.m_Experience,
                ResID = this.ResID,
                Enlightenment = this.Enlightenment,
                Skills = this.Skills,
                Equips = this.Equips,
                HP = this.m_CurrentLife,
                MaxHP = this.m_CurrentLifeMax,
                Joyful = this.Joyful,
                Life = this.Life,
                AtkSpeed = this.GetCurrentAttackSpeed(),
                CastSpeed = this.GetCurrentCastSpeed(),
                Hit = this.CurrentAttackRating(),
                Dodge = this.GetCurrentDefend(),
                Crit = this.m_CurrentDeadlyStrike,
                Str = this.BaseStr,
                Dex = this.BaseDex,
                Sta = this.BaseSta,
                Int = this.BaseInt,
                MoveSpeed = this.Owner.GetCurrentRunSpeed(),
                RemainPoints = this.BaseRemainPoints,
                PAtk = this.m_PhysicsDamage.nValue[0],
                MAtk = this.m_MagicDamage.nValue[0],
                PDef = this.GetCurResist(KiemThe.Entities.DAMAGE_TYPE.damage_physics),
                PoisonRes = this.GetCurResist(KiemThe.Entities.DAMAGE_TYPE.damage_poison),
                IceRes = this.GetCurResist(KiemThe.Entities.DAMAGE_TYPE.damage_cold),
                FireRes = this.GetCurResist(KiemThe.Entities.DAMAGE_TYPE.damage_fire),
                LightningRes = this.GetCurResist(KiemThe.Entities.DAMAGE_TYPE.damage_light),
            };
        }
    }
}
