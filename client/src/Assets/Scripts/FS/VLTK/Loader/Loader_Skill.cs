using FS.VLTK.Entities.ActionSet;
using FS.VLTK.Entities.Config;
using FS.VLTK.Factory.AnimationManager;
using GameServer.VLTK.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using static FS.VLTK.Entities.Enum;

namespace FS.VLTK.Loader
{
    /// <summary>
    /// Đối tượng chứa danh sách các cấu hình trong game
    /// </summary>
    public static partial class Loader
    {
        #region PropertyDictionary
        /// <summary>
        /// Đọc dữ liệu từ file PropertyDictionary.xml trong Bundle
        /// </summary>
        /// <param name="xmlNode"></param>
        public static void LoadPropertyDictionary(XElement xmlNode)
        {
            PropertyDefine.Parse(xmlNode);
        }
        #endregion

        #region SkillData
        /// <summary>
        /// Đường dẫn file Bundle chứa âm thanh hiệu ứng xuất chiêu
        /// </summary>
        public static string SkillCastSoundBundleDir { get; private set; }

        /// <summary>
        /// Danh sách kỹ năng
        /// </summary>
        public static Dictionary<int, SkillDataEx> Skills { get; private set; } = new Dictionary<int, SkillDataEx>();

        /// <summary>
        /// Danh sách thuộc tính kỹ năng tương ứng
        /// </summary>
        public static Dictionary<string, SkillConfigAttribute> SkillAttributeLists { get; private set; } = new Dictionary<string, SkillConfigAttribute>();

        /// <summary>
        /// Danh sách nhóm kỹ năng bổ trợ
        /// </summary>
        public static Dictionary<int, EnchantSkill> EnchantSkills { get; private set; } = new Dictionary<int, EnchantSkill>();

        /// <summary>
        /// Danh sách kỹ năng tự động xuất
        /// </summary>
        public static Dictionary<int, AutoSkill> AutoSkills { get; private set; } = new Dictionary<int, AutoSkill>();

        /// <summary>
        /// Đọc dữ liệu từ file SkillData.xml trong Bundle
        /// </summary>
        /// <param name="xmlNode"></param>
        public static void LoadSkillData(XElement xmlNode)
        {
            Loader.Skills.Clear();

            Loader.SkillCastSoundBundleDir = xmlNode.Attribute("SkillCastSoundBundleDir").Value;
            foreach (XElement node in xmlNode.Elements("Skill"))
            {
                SkillDataEx skill = SkillDataEx.Parse(node);
                Loader.Skills[skill.ID] = skill;
            }

        }

        /// <summary>
        /// Đọc dữ liệu từ file SkillPropertiesLua.xml
        /// </summary>
        /// <param name="xmlNode"></param>
        public static void LoadSkillAttribute(XElement xmlNode)
        {
            Loader.SkillAttributeLists.Clear();

            foreach (XElement node in xmlNode.Elements("Skill"))
            {
                SkillConfigAttribute attrib = SkillConfigAttribute.Parse(node);
                Loader.SkillAttributeLists[attrib.PropertyName] = attrib;
            }
        }

        /// <summary>
        /// Đọc dữ liệu từ file EnchantSkill.xml
        /// </summary>
        /// <param name="xmlNode"></param>
        public static void LoadEnchantSkill(XElement xmlNode)
        {
            Loader.EnchantSkills.Clear();

            foreach (XElement node in xmlNode.Elements("Skill"))
            {
                EnchantSkill enchantSkill = EnchantSkill.Parse(node);
                Loader.EnchantSkills[enchantSkill.ID] = enchantSkill;
            }
        }

        /// <summary>
        /// Đọc dữ liệu từ file AutoSkill.xml
        /// </summary>
        /// <param name="xmlNode"></param>
        public static void LoadAutoSkill(XElement xmlNode)
        {
            Loader.AutoSkills.Clear();

            foreach (XElement node in xmlNode.Elements("AutoSkill"))
            {
                try
                {
                    AutoSkill autoSkill = AutoSkill.Parse(node);
                    Loader.AutoSkills[autoSkill.ID] = autoSkill;
                }
                catch (Exception e)
                {
                    
                }
            }
        }

        #region BulletActionSet
        /// <summary>
        /// File XML chứa thông tin Res đạn
        /// </summary>
        public static BulletActionSetXML BulletActionSetXML { get; private set; }

        /// <summary>
        /// Thông tin Res đạn
        /// </summary>
        public static Dictionary<int, BulletConfig> BulletConfigs { get; private set; } = new Dictionary<int, BulletConfig>();

        /// <summary>
        /// Đọc dữ liệu file XML chứa thông tin Res đạn
        /// </summary>
        /// <param name="xmlNode"></param>
        public static void LoadBulletActionSetXML(XElement xmlNode)
        {
            Loader.BulletActionSetXML = BulletActionSetXML.Parse(xmlNode);
        }

        /// <summary>
        /// Đọc dữ liệu file XML chứa thông tin cấu hình Logic của đạn
        /// </summary>
        /// <param name="xmlNode"></param>
        public static void LoadBulletConfig(XElement xmlNode)
        {
            Loader.BulletConfigs.Clear();

            foreach (XElement node in xmlNode.Elements("Bullet"))
            {
                BulletConfig bulletConfig = BulletConfig.Parse(node);
                Loader.BulletConfigs[bulletConfig.ID] = bulletConfig;
            }
        }

        /// <summary>
        /// Đọc dữ liệu âm thanh đạn
        /// </summary>
        /// <param name="bytes"></param>
        public static void LoadBulletActionSetSound(byte[] bytes)
        {
            {
                int i = 0;
                while (i < bytes.Length)
                {
                    int effectIDLength = (int) bytes[i];
                    i++;

                    string effectID = "";
                    for (int j = 1; j <= effectIDLength; j++)
                    {
                        effectID += Loader.charTable[bytes[i]];
                        i++;
                    }



                    int flySoundLength = (int) bytes[i];
                    i++;

                    string flySound = "";
                    for (int j = 1; j <= flySoundLength; j++)
                    {
                        flySound += Loader.charTable[bytes[i]];
                        i++;
                    }

                    int fallDownSoundLength = (int) bytes[i];
                    i++;

                    string fallDownSound = "";
                    for (int j = 1; j <= fallDownSoundLength; j++)
                    {
                        fallDownSound += Loader.charTable[bytes[i]];
                        i++;
                    }

                    int explodeSoundLength = (int) bytes[i];
                    i++;

                    string explodeSound = "";
                    for (int j = 1; j <= explodeSoundLength; j++)
                    {
                        explodeSound += Loader.charTable[bytes[i]];
                        i++;
                    }

                    BulletAnimationManager.SoundInfo bullet = new BulletAnimationManager.SoundInfo()
                    {
                        ID = int.Parse(effectID),
                        SoundFly = flySound.Replace(".wav", "").Replace(".mp3", ""),
                        SoundFadeOut = fallDownSound.Replace(".wav", "").Replace(".mp3", ""),
                        SoundExplode = explodeSound.Replace(".wav", "").Replace(".mp3", ""),
                    };
                    BulletAnimationManager.Instance.Sounds[bullet.ID] = bullet;
                }
            }
        }
        #endregion
        #endregion
    }
}
