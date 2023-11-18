using FS.VLTK.Entities.Config;
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
        #region WeaponEnhanceEffect
        /// <summary>
        /// Danh sách thiết lập hiệu ứng cường hóa vũ khí theo loại
        /// </summary>
        public static Dictionary<int, WeaponEnhanceConfigXML> WeaponEnhanceConfigXMLs { get; private set; } = new Dictionary<int, WeaponEnhanceConfigXML>();

        /// <summary>
        /// Tải danh sách thiết lập hiệu ứng cường hóa vũ khí theo loại
        /// </summary>
        /// <param name="xmlNode"></param>
        public static void LoadWeaponEnhanceEffectConfig(XElement xmlNode)
        {
            /// Danh sách Config thêm dựa vào độ lệch so với thông số mặc định
            List<Tuple<int, int, float, float>> additions = new List<Tuple<int, int, float, float>>();
            foreach (XElement node in xmlNode.Element("ConfigDiv").Elements("WeaponDiv"))
            {
                int weaponCategory = int.Parse(node.Attribute("Category").Value);
                int bodyGlowThreshold = int.Parse(node.Attribute("BodyGlowThresholdAdd").Value);
                float bodyAlpha = int.Parse(node.Attribute("BodyAlphaAdd").Value) / 100f;
                float starAlpha = int.Parse(node.Attribute("StarAlphaAdd").Value) / 100f;

                Tuple<int, int, float, float> addTuple = new Tuple<int, int, float, float>(weaponCategory, bodyGlowThreshold, bodyAlpha, starAlpha);
                additions.Add(addTuple);
            }

            foreach (XElement node in xmlNode.Elements("Weapon"))
            {
                WeaponEnhanceConfigXML configXML = WeaponEnhanceConfigXML.Parse(node);
                Loader.WeaponEnhanceConfigXMLs[configXML.Category] = configXML;
            }

            /// Duyệt danh sách Config thêm
            foreach (Tuple<int, int, float, float> addTuple in additions)
            {
                int weaponCategory = addTuple.Item1;
                int bodyGlowThreshold = addTuple.Item2;
                float bodyAlpha = addTuple.Item3;
                float starAlpha = addTuple.Item4;

                WeaponEnhanceConfigXML configXML = Loader.WeaponEnhanceConfigXMLs[-1].Clone();
                configXML.ChangeCategory(weaponCategory);
                foreach (KeyValuePair<Elemental, WeaponEnhanceConfigXML.EffectBySeries> pairBySeries in configXML.EffectsBySeries)
                {
                    foreach (KeyValuePair<int, WeaponEnhanceConfigXML.EffectBySeries.EffectDetails> pairByLevel in pairBySeries.Value.EffectsByLevel)
                    {
                        WeaponEnhanceConfigXML.EffectBySeries.EffectDetails effectDetails = pairByLevel.Value;
                        effectDetails.ChangeValue(bodyGlowThreshold, bodyAlpha, starAlpha);
                    }
                }
                Loader.WeaponEnhanceConfigXMLs[configXML.Category] = configXML;
            }
        }
        #endregion

    }
}
