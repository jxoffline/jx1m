using GameServer.KiemThe.Utilities;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace GameServer.KiemThe.Entities
{
    /// <summary>
    /// Dữ liệu kỹ năng
    /// </summary>
    public class SkillDataEx
    {
        /// <summary>
        /// Cấp độ tối đa của kỹ năng
        /// </summary>
        public const int SystemMaxLevel = 50;

        /// <summary>
        /// Thời gian mặc định xuất hiện đạn (với loại đạn xuất nhiều chiêu một lần trong cùng một đường thẳng)
        /// </summary>
        public const float BulletSpawnPeriod = 0.2f;

        /// <summary>
        /// Vận tốc ban đầu của đạn bay
        /// </summary>
        public const int InitBulletVelocity = 500;

        /// <summary>
        /// ID kỹ năng
        /// </summary>
        public int ID { get; private set; }

        /// <summary>
        /// Tên kỹ năng
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Mô tả ngắn gọn
        /// </summary>
        public string ShortDesc { get; private set; }

        /// <summary>
        /// Mô tả chi tiết
        /// </summary>
        public string FullDesc { get; private set; }

        /// <summary>
        /// Vị trí bắt đầu tia đạn
        /// </summary>
        public int StartPoint { get; set; }

        /// <summary>
        /// Loại biểu diễn kỹ năng
        /// <para>0: Bay theo đường thẳng</para>
        /// </summary>
        public int Form { get; set; }

        /// <summary>
        /// Loại kỹ năng (1, 2, 4, 5, 6: Chủ động, 3: Bị động)
        /// <para>2: Buff</para>
        /// <para>5: Kỹ năng tấn công</para>
        /// </summary>
        public int Type { get; private set; }

        /// <summary>
        /// Loại kỹ năng
        /// <para>~physicalattack: Xuất chiêu bay theo đường từ vị trí xuất chiêu</para>
        /// <para>~magicattack: Xuất chiêu tại một điểm</para>
        /// </summary>
        public string SkillStyle { get; private set; }

        /// <summary>
        /// Cộng thêm sát thương vật lý (cái này chỉ ở quái mới có)
        /// </summary>
        public int AppendDamageP { get; private set; }

        /// <summary>
        /// Số chiêu xuất ra mỗi lần
        /// </summary>
        public int ShootCount { get; private set; }

        /// <summary>
        /// ID phái
        /// </summary>
        public int FactionID { get; private set; }

        /// <summary>
        /// ID nhánh
        /// </summary>
        public int SubID { get; private set; }

        /// <summary>
        /// Đường dẫn file Bundle chứa Icon
        /// </summary>
        public string IconBundleDir { get; private set; }

        /// <summary>
        /// Tên Atlas chứa ảnh
        /// </summary>
        public string IconAtlasName { get; private set; }

        /// <summary>
        /// Icon kỹ năng
        /// </summary>
        public string Icon { get; private set; }

        /// <summary>
        /// ID hiệu ứng xuất chiêu
        /// </summary>
        public int CastEffectID { get; private set; }

        /// <summary>
        /// Tên âm thanh động tác xuất chiêu với nhân vật Nam
        /// </summary>
        public string MaleCastSound { get; private set; }

        /// <summary>
        /// Tên âm thanh động tác xuất chiêu với nhân vật Nữ
        /// </summary>
        public string FemaleCastSound { get; private set; }

        /// <summary>
        /// ID hiệu ứng BUFF nếu có (xem ở StateEffect.xml)
        /// </summary>
        public int StateEffectID { get; private set; }

        /// <summary>
        /// Nếu là BUFF thì có bị hủy khi nhân vật chết không
        /// </summary>
        public bool IsStateNoClearOnDead { get; private set; }

        /// <summary>
        /// Có phải vòng sáng hỗ trợ không
        /// </summary>
        public bool IsArua { get; private set; }

        /// <summary>
        /// Phạm vi tấn công
        /// </summary>
        public int AttackRadius { get; private set; }

        /// <summary>
        /// Bỏ qua khoảng cách xuất chiêu
        /// </summary>
        public bool IgnoreDistance { get; private set; }

        /// <summary>
        /// Có phải BulletID là ID của đạn trong file cấu hình đạn không
        /// </summary>
        public bool IsBullet { get; private set; }

        /// <summary>
        /// ID đạn bay
        /// <para>Nếu IsBullet = false thì sẽ là ID kỹ năng biểu diễn đạn bay</para>
        /// </summary>
        public int BulletID { get; private set; }

        /// <summary>
        /// Tổng số đạn bay
        /// </summary>
        public int BulletCount { get; private set; }

        /// <summary>
        /// ID động tác xuất chiêu
        /// </summary>
        public int CastActionID { get; private set; }

        /// <summary>
        /// Tay ngắn
        /// </summary>
        public bool IsMelee { get; private set; }

        /// <summary>
        /// CÓ phải sát thương ngoại công không
        /// </summary>
        public bool IsPhysical { get; private set; }

        /// <summary>
        /// Có phải kỹ năng gây sát thương không
        /// </summary>
        public bool IsDamageSkill { get; private set; }

        /// <summary>
        /// Sử dụng điểm chính xác
        /// </summary>
        public bool IsUseAR { get; private set; }

        /// <summary>
        /// Mục tiêu
        /// </summary>
        public string TargetType { get; private set; }

        /// <summary>
        /// Thời gian chờ đến lượt thi triển
        /// </summary>
        public int WaitTime { get; set; }

        /// <summary>
        /// ID kỹ năng đi kèm lúc bắt đầu thi triển
        /// </summary>
        public int StartSkillID { get; private set; }

        /// <summary>
        /// ID kỹ năng đạn bay đi kèm (sau khi bẫy nổ)
        /// </summary>
        public int BulletSkillID { get; private set; }

        /// <summary>
        /// ID kỹ năng đi kèm khi chạm mục tiêu
        /// </summary>
        public int CollideSkillID { get; private set; }

        /// <summary>
        /// ID kỹ năng thi triển khi biến mất
        /// </summary>
        public int VanishSkillID { get; private set; }

        /// <summary>
        /// Kỹ năng yêu cầu cấp độ
        /// </summary>
        public int RequireLevel { get; private set; }

        /// <summary>
        /// Cấp độ tối đa có thể học của kỹ năng
        /// </summary>
        public int MaxSkillLevel { get; private set; }

        /// <summary>
        /// ID vũ khí giới hạn
        /// </summary>
        public List<int> WeaponLimit { get; private set; }

        /// <summary>
        /// Chỉ được sử dụng trên ngựa
        /// </summary>
        public bool HorseLimit { get; private set; }

        /// <summary>
        /// Kỹ năng sử dụng nộ khí
        /// </summary>
        public bool IsUseRage { get; private set; }

        /// <summary>
        /// Kỹ năng dùng vũ khí cơ bản của người chơi mới
        /// </summary>
        public bool IsBasicWeaponSkill { get; private set; }

        /// <summary>
        /// Có thể sử dụng ở mọi nơi, nếu False thì chỉ được sử dụng ở bản đồ dã ngoại
        /// </summary>
        public bool IsCanUseEverywhere { get; private set; }

        /// <summary>
        /// Kỹ năng cần sử dụng mật tịch để thăng cấp
        /// </summary>
        public bool IsExpSkill { get; private set; }

        /// <summary>
        /// Danh sách Param (5 cái)
        /// </summary>
        public int[] Params { get; private set; }

        /// <summary>
        /// Danh sách thuộc tính theo cấp độ được thiết lập dạng nguyên thủy
        /// </summary>
        public string RawPropertiesConfig { get; private set; }

        /// <summary>
        /// Danh sách thuộc tính theo cấp độ
        /// </summary>
        public Dictionary<int, PropertyDictionary> Properties { get; private set; }

        /// <summary>
        /// Giá trị thuộc tính ngũ hành
        /// </summary>
        /// <returns></returns>
        public int ElementalSeries { get; private set; }

        /// <summary>
        /// Có thể cộng điểm vào không
        /// </summary>
        public bool CanAddPoint { get; set; }

        /// <summary>
        /// ID vòng sáng cha (nếu là vòng sáng, hoặc -1 nếu không tồn tại)
        /// </summary>
        public int ParentAura { get; set; }

        /// <summary>
        /// Thời gian xuất hiện tia đạn giữa các lần liên tiếp
        /// </summary>
        public int BulletRoundTime { get; set; }

        /// <summary>
        /// ID nhiệm vụ cần hoàn thành để có thể học
        /// </summary>
        public int NeedCompleteQuestID { get; set; }

        /// <summary>
        /// Số lần tấn công cố định (không ảnh hưởng bởi tốc đánh)
        /// </summary>
        public int FixedAttackActionCount { get; set; }

        /// <summary>
        /// Kỹ năng không cần mục tiêu
        /// </summary>
        public bool IsSkillNoTarget { get; set; }

        /// <summary>
        /// Loại năng lượng kỹ năng sử dụng
        /// <para>0: Sinh lực, 1: Nội lực, 2: Thể lực</para>
        /// </summary>
        public int SkillCostType { get; set; }

        /// <summary>
        /// Có phải kỹ năng không ảnh hưởng tốc đánh không
        /// </summary>
        public bool IsSkillNoAddAttackSpeedCooldown { get; set; }

        /// <summary>
        /// Đánh luôn trúng
        /// </summary>
        public bool AlwaysHit { get; set; }

        /// <summary>
        /// Chuyển đối tượng từ XML Node
        /// </summary>
        /// <param name="xmlNode"></param>
        /// <returns></returns>
        public static SkillDataEx Parse(XElement xmlNode)
        {
            SkillDataEx skillData = new SkillDataEx()
            {
                ID = int.Parse(xmlNode.Attribute("ID").Value),
                Name = xmlNode.Attribute("Name").Value,
                ShortDesc = xmlNode.Attribute("ShortDesc").Value,
                FullDesc = xmlNode.Attribute("FullDesc").Value,
                StartPoint = int.Parse(xmlNode.Attribute("StartPoint").Value),
                Form = int.Parse(xmlNode.Attribute("Form").Value),
                Type = int.Parse(xmlNode.Attribute("Type").Value),
                SkillStyle = xmlNode.Attribute("SkillStyle").Value,
                AppendDamageP = int.Parse(xmlNode.Attribute("AppendDamageP").Value),
                ShootCount = int.Parse(xmlNode.Attribute("ShootCount").Value),
                FactionID = int.Parse(xmlNode.Attribute("FactionID").Value),
                SubID = int.Parse(xmlNode.Attribute("SubID").Value),
                IconBundleDir = xmlNode.Attribute("IconBundleDir").Value,
                IconAtlasName = xmlNode.Attribute("IconAtlasName").Value,
                Icon = xmlNode.Attribute("Icon").Value,
                CastEffectID = int.Parse(xmlNode.Attribute("CastEffectID").Value),
                MaleCastSound = xmlNode.Attribute("MaleCastSound").Value,
                FemaleCastSound = xmlNode.Attribute("FemaleCastSound").Value,
                StateEffectID = int.Parse(xmlNode.Attribute("StateEffectID").Value),
                IsStateNoClearOnDead = bool.Parse(xmlNode.Attribute("IsStateNoClearOnDead").Value),
                IsArua = bool.Parse(xmlNode.Attribute("IsArua").Value),
                AttackRadius = int.Parse(xmlNode.Attribute("AttackRadius").Value),
                IgnoreDistance = bool.Parse(xmlNode.Attribute("IgnoreDistance").Value),
                IsBullet = int.Parse(xmlNode.Attribute("IsBullet").Value) == 1,
                BulletID = int.Parse(xmlNode.Attribute("BulletID").Value),
                BulletCount = int.Parse(xmlNode.Attribute("BulletCount").Value),
                CastActionID = int.Parse(xmlNode.Attribute("CastActionID").Value),
                IsMelee = bool.Parse(xmlNode.Attribute("IsMelee").Value),
                IsPhysical = bool.Parse(xmlNode.Attribute("IsPhysical").Value),
                IsDamageSkill = bool.Parse(xmlNode.Attribute("IsDamageSkill").Value),
                IsUseAR = bool.Parse(xmlNode.Attribute("IsUseAR").Value),
                TargetType = xmlNode.Attribute("TargetType").Value,
                WaitTime = int.Parse(xmlNode.Attribute("WaitTime").Value),
                StartSkillID = int.Parse(xmlNode.Attribute("StartSkillID").Value),
                BulletSkillID = int.Parse(xmlNode.Attribute("BulletSkillID").Value),
                CollideSkillID = int.Parse(xmlNode.Attribute("FinishSkillID").Value),
                VanishSkillID = int.Parse(xmlNode.Attribute("VanishSkillID").Value),
                RequireLevel = int.Parse(xmlNode.Attribute("RequireLevel").Value),
                MaxSkillLevel = int.Parse(xmlNode.Attribute("MaxSkillLevel").Value),
                WeaponLimit = new List<int>(),
                HorseLimit = bool.Parse(xmlNode.Attribute("HorseLimit").Value),
                IsUseRage = bool.Parse(xmlNode.Attribute("IsUseRage").Value),
                CanAddPoint = bool.Parse(xmlNode.Attribute("IsCanAddPoint").Value),
                IsBasicWeaponSkill = bool.Parse(xmlNode.Attribute("IsBasicWeaponSkill").Value),
                IsCanUseEverywhere = bool.Parse(xmlNode.Attribute("IsCanUseEverywhere").Value),
                IsExpSkill = bool.Parse(xmlNode.Attribute("IsExpSkill").Value),
                ElementalSeries = int.Parse(xmlNode.Attribute("ElementalSeries").Value),
                Params = new int[] {
                    int.Parse(xmlNode.Attribute("Param1").Value),
                    int.Parse(xmlNode.Attribute("Param2").Value),
                    int.Parse(xmlNode.Attribute("Param3").Value),
                    int.Parse(xmlNode.Attribute("Param4").Value),
                    int.Parse(xmlNode.Attribute("Param5").Value),
                },
                RawPropertiesConfig = xmlNode.Attribute("Properties").Value,
                Properties = new Dictionary<int, PropertyDictionary>(),
                ParentAura = xmlNode.Attribute("ParentAura") == null ? -1 : int.Parse(xmlNode.Attribute("ParentAura").Value),
                BulletRoundTime = int.Parse(xmlNode.Attribute("BulletRoundTime").Value),
                NeedCompleteQuestID = int.Parse(xmlNode.Attribute("IsNeedCompleteQuestID").Value),
                FixedAttackActionCount = int.Parse(xmlNode.Attribute("FixedAttackActionCount").Value),
                IsSkillNoTarget = bool.Parse(xmlNode.Attribute("IsSkillNoTarget").Value),
                SkillCostType = int.Parse(xmlNode.Attribute("SkillCostType").Value),
                IsSkillNoAddAttackSpeedCooldown = bool.Parse(xmlNode.Attribute("IsSkillNoAddAttackSpeedCooldown").Value),
                AlwaysHit = bool.Parse(xmlNode.Attribute("AlwaysHit").Value),
            };

            string weaponLimitString = xmlNode.Attribute("WeaponLimit").Value;
            foreach (string field in weaponLimitString.Split(';'))
            {
                skillData.WeaponLimit.Add(int.Parse(field));
            }

            for (int i = 1; i <= SkillDataEx.SystemMaxLevel; i++)
            {
                skillData.Properties[i] = new PropertyDictionary();
            }

            SkillConfigAttribute skillConfigAttribute = KSkill.GetSkillAttributes(skillData.RawPropertiesConfig);
            if (skillConfigAttribute != null)
            {
                foreach (KeyValuePair<string, SkillConfigAttribute.Symbol> pair in skillConfigAttribute.Symbols)
                {
                    string propertySymbol = pair.Key;
                    SkillConfigAttribute.Symbol propertyValue = pair.Value;

                    if (PropertyDefine.PropertiesBySymbolName.TryGetValue(propertySymbol, out PropertyDefine.Property property))
                    {
                        for (int i = 1; i <= SkillDataEx.SystemMaxLevel; i++)
                        {
                            KMagicAttrib kMagicAttrib = new KMagicAttrib();
                            kMagicAttrib.nAttribType = (MAGIC_ATTRIB)property.ID;
                            kMagicAttrib.nValue = new int[] { 0, 0, 0 };

                            if (propertyValue.Values.TryGetValue(1, out Dictionary<int, SkillConfigAttribute.Symbol.ValueByLevel> values1))
                            {
                                if (values1.TryGetValue(i, out SkillConfigAttribute.Symbol.ValueByLevel value))
                                {
                                    kMagicAttrib.nValue[0] = (int) value.Value;
                                }
                            }

                            if (propertyValue.Values.TryGetValue(2, out Dictionary<int, SkillConfigAttribute.Symbol.ValueByLevel> values2))
                            {
                                if (values2.TryGetValue(i, out SkillConfigAttribute.Symbol.ValueByLevel value))
                                {
                                    kMagicAttrib.nValue[1] = (int) value.Value;
                                }
                            }

                            if (propertyValue.Values.TryGetValue(3, out Dictionary<int, SkillConfigAttribute.Symbol.ValueByLevel> values3))
                            {
                                if (values3.TryGetValue(i, out SkillConfigAttribute.Symbol.ValueByLevel value))
                                {
                                    kMagicAttrib.nValue[2] = (int) value.Value;
                                }
                            }

                            skillData.Properties[i].Set<KMagicAttrib>(property.ID, kMagicAttrib);
                        }
                    }
                }
            }

            return skillData;
        }

        /// <summary>
        /// Tạo một bản sao của đối tượng
        /// </summary>
        /// <param name="skill"></param>
        /// <returns></returns>
        public SkillDataEx Clone()
        {
            return new SkillDataEx()
            {
                ID = this.ID,
                Name = this.Name,
                ShortDesc = this.ShortDesc,
                FullDesc = this.FullDesc,
                StartPoint = this.StartPoint,
                Form = this.Form,
                Type = this.Type,
                SkillStyle = this.SkillStyle,
                AppendDamageP = this.AppendDamageP,
                ShootCount = this.ShootCount,
                FactionID = this.FactionID,
                SubID = this.SubID,
                IconBundleDir = this.IconBundleDir,
                IconAtlasName = this.IconAtlasName,
                Icon = this.Icon,
                CastEffectID = this.CastEffectID,
                MaleCastSound = this.MaleCastSound,
                FemaleCastSound = this.FemaleCastSound,
                StateEffectID = this.StateEffectID,
                IsStateNoClearOnDead = this.IsStateNoClearOnDead,
                IsArua = this.IsArua,
                AttackRadius = this.AttackRadius,
                IgnoreDistance = this.IgnoreDistance,
                IsBullet = this.IsBullet,
                BulletID = this.BulletID,
                BulletCount = this.BulletCount,
                CastActionID = this.CastActionID,
                IsMelee = this.IsMelee,
                IsPhysical = this.IsPhysical,
                IsDamageSkill = this.IsDamageSkill,
                IsUseAR = this.IsUseAR,
                TargetType = this.TargetType,
                WaitTime = this.WaitTime,
                StartSkillID = this.StartSkillID,
                BulletSkillID = this.BulletSkillID,
                CollideSkillID = this.CollideSkillID,
                VanishSkillID = this.VanishSkillID,
                RequireLevel = this.RequireLevel,
                MaxSkillLevel = this.MaxSkillLevel,
                WeaponLimit = this.WeaponLimit,
                HorseLimit = this.HorseLimit,
                IsUseRage = this.IsUseRage,
                IsBasicWeaponSkill = this.IsBasicWeaponSkill,
                IsCanUseEverywhere = this.IsCanUseEverywhere,
                IsExpSkill = this.IsExpSkill,
                ElementalSeries = this.ElementalSeries,
                Params = new int[] { this.Params[0], this.Params[1], this.Params[2], this.Params[3], this.Params[4] },
                RawPropertiesConfig = this.RawPropertiesConfig,
                Properties = this.Properties.ToDictionary(entry => entry.Key, entry => entry.Value),
            };
        }
    }
}