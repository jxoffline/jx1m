using GameServer.Logic;
using System.Collections.Generic;
using System.Xml.Linq;

namespace GameServer.KiemThe.Entities
{
    /// <summary>
    /// Đối tượng quản lý kỹ năng hệ thống
    /// </summary>
    public class KSkill
    {
        /// <summary>
        /// Danh sách kỹ năng hệ thống
        /// </summary>
        private static readonly Dictionary<int, SkillDataEx> skillLists = new Dictionary<int, SkillDataEx>();

        /// <summary>
        /// Danh sách thuộc tính thông số kỹ năng
        /// </summary>
        private static readonly Dictionary<string, SkillConfigAttribute> attributeLists = new Dictionary<string, SkillConfigAttribute>();

        /// <summary>
        /// Danh sách các kỹ năng tự động thi triển theo điều kiện
        /// </summary>
        private static readonly Dictionary<int, AutoSkill> autoSkillLists = new Dictionary<int, AutoSkill>();

        /// <summary>
        /// Danh sách cấu hình đạn bay
        /// </summary>
        private static readonly Dictionary<int, BulletConfig> bulletLists = new Dictionary<int, BulletConfig>();

        /// <summary>
        /// Danh sách nhóm kỹ năng bổ trợ
        /// </summary>
        private static readonly Dictionary<int, EnchantSkill> enchantSkillLists = new Dictionary<int, EnchantSkill>();

        /// <summary>
        /// Đọc dữ liệu danh sách kỹ năng
        /// </summary>
        public static void LoadSkillData()
        {
            KSkill.skillLists.Clear();
            KSkill.attributeLists.Clear();
            KSkill.autoSkillLists.Clear();
            KSkill.bulletLists.Clear();
            KSkill.enchantSkillLists.Clear();

            XElement attribNode = KTGlobal.ReadXMLData("Config/KT_Skill/SkillPropertiesLua.xml");
            foreach (XElement node in attribNode.Elements("Skill"))
            {
                SkillConfigAttribute attrib = SkillConfigAttribute.Parse(node);
                KSkill.attributeLists[attrib.PropertyName] = attrib;
            }

            XElement xmlNode = KTGlobal.ReadXMLData("Config/KT_Skill/SkillData.xml");
            foreach (XElement node in xmlNode.Elements("Skill"))
            {
                SkillDataEx skill = SkillDataEx.Parse(node);
                KSkill.skillLists[skill.ID] = skill;
            }

            XElement autoSkillNode = KTGlobal.ReadXMLData("Config/KT_Skill/AutoSkill.xml");
            foreach (XElement node in autoSkillNode.Elements("AutoSkill"))
            {
                AutoSkill autoSkill = AutoSkill.Parse(node);
                KSkill.autoSkillLists[autoSkill.ID] = autoSkill;
            }

            XElement bulletConfigNode = KTGlobal.ReadXMLData("Config/KT_Skill/BulletConfig.xml");
            foreach (XElement node in bulletConfigNode.Elements("Bullet"))
            {
                BulletConfig bulletConfig = BulletConfig.Parse(node);
                KSkill.bulletLists[bulletConfig.ID] = bulletConfig;
            }

            XElement enchantSkillNode = KTGlobal.ReadXMLData("Config/KT_Skill/EnchantSkill.xml");
            foreach (XElement node in enchantSkillNode.Elements("Skill"))
            {
                EnchantSkill enchantSkill = EnchantSkill.Parse(node);
                KSkill.enchantSkillLists[enchantSkill.ID] = enchantSkill;
            }
        }

        /// <summary>
        /// Trả về kỹ năng tự thi triển theo điều kiện
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static AutoSkill GetAutoSkill(int id)
        {
            if (KSkill.autoSkillLists.TryGetValue(id, out AutoSkill autoSkill))
            {
                return autoSkill;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Trả về dữ liệu thuộc tính kỹ năng tương ứng
        /// </summary>
        /// <param name="skillPropertyConfig"></param>
        /// <returns></returns>
        public static SkillConfigAttribute GetSkillAttributes(string skillPropertyConfig)
        {
            if (KSkill.attributeLists.TryGetValue(skillPropertyConfig, out SkillConfigAttribute attrib))
            {
                return attrib;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Trả về dữ liệu kỹ năng với ID tương ứng
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static SkillDataEx GetSkillData(int id)
        {
            if (KSkill.skillLists.TryGetValue(id, out SkillDataEx skillData))
            {
                return skillData;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Trả về dữ liệu cấu hình đạn bay với ID tương ứng
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static BulletConfig GetBulletConfig(int id)
        {
            if (KSkill.bulletLists.TryGetValue(id, out BulletConfig bulletConfig))
            {
                return bulletConfig;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Trả về dữ liệu nhóm kỹ năng bổ trợ
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static EnchantSkill GetEnchantSkill(int id)
        {
            if (KSkill.enchantSkillLists.TryGetValue(id, out EnchantSkill enchantSkill))
            {
                return enchantSkill;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Kiểm tra kỹ năng có tồn tại trong hệ thống không
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static bool IsSkillExist(int id)
        {
            return KSkill.GetSkillData(id) != null;
        }
    }
}