using GameServer.KiemThe.Utilities;
using GameServer.Logic;

namespace GameServer.KiemThe.Entities
{
    /// <summary>
    /// Định nghĩa kỹ năng tự kích hoạt của đối tượng
    /// </summary>
    public class KNpcAutoSkill
    {
        /// <summary>
        /// Thông tin kỹ năng tự kích hoạt
        /// </summary>
        public AutoSkill Info { get; set; }

        /// <summary>
        /// Chủ nhân kỹ năng
        /// </summary>
        public GameObject Owner { get; set; }

        /// <summary>
        /// Cấp độ kỹ năng tự kích hoạt
        /// </summary>
        public int Level { get; set; }

        /// <summary>
        /// ID kỹ năng chủ
        /// </summary>
        public int OwnerSkillID { get; set; }

        /// <summary>
        /// Tỷ lệ kích hoạt
        /// </summary>
        public int CastSkillActivatePercent
        {
            get
            {
                return this.Info.CastPercentByLevel[this.Level].Value;
            }
        }

        private SkillLevelRef _CastSkill = null;
        /// <summary>
        /// Kỹ năng tương ứng khi kích hoạt
        /// </summary>
        public SkillLevelRef CastSkill
        {
            get
            {
                if (this._CastSkill != null)
                {
                    return this._CastSkill;
                }
                SkillDataEx skillData = KSkill.GetSkillData(this.Info.CastSkillID);
                if (skillData == null)
                {
                    return null;
                }

                int skillLevel = this.Info.CastSkillLevelByLevel[this.Level].Value;
                if (skillLevel == -1)
                {
                    skillLevel = this.Level;
                }
                SkillLevelRef skill = new SkillLevelRef()
                {
                    AddedLevel = skillLevel,
                    BonusLevel = 0,
                    CanStudy = false,
                    Data = skillData,
                };
                this._CastSkill = skill;
                return skill;
            }
        }

        /// <summary>
        /// Thời gian kích hoạt lần trước
        /// </summary>
        public long LastActivateTick { get; set; }

        /// <summary>
        /// Có đang trong thời gian chờ phục hồi không
        /// </summary>
        public bool IsCoolDown
        {
            get
            {
                /// Nếu cấp độ không nằm trong danh sách tức có lỗi
                if (!this.Info.DelayPerCastByLevel.TryGetValue(this.Level, out _))
                {
                    return true;
                }

                long cooldownTick = this.Info.DelayPerCastByLevel[this.Level].Value * 1000 / 18;

                /// Kiểm tra nếu có kỹ năng hỗ trợ giảm thời gian giãn cách xuất hiện
                if (this.Owner is KPlayer)
                {
                    KPlayer player = this.Owner as KPlayer;
                    PropertyDictionary enchantPd = player.Skills.GetEnchantProperties(this.OwnerSkillID);
                    if (enchantPd != null)
                    {
                        if (enchantPd.ContainsKey((int) MAGIC_ATTRIB.magic_skilladdition_decautoskillcdtime))
                        {
                            KMagicAttrib magicAttrib = enchantPd.Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_skilladdition_decautoskillcdtime);
                            cooldownTick -= magicAttrib.nValue[2] * 1000 / 18;
                        }
                    }
                }

                return KTGlobal.GetCurrentTimeMilis() - this.LastActivateTick < cooldownTick;
            }
        }
    }
}
