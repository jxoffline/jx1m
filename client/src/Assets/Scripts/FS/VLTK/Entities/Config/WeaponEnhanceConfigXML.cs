using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using UnityEngine;
using static FS.VLTK.Entities.Enum;

namespace FS.VLTK.Entities.Config
{
    /// <summary>
    /// Đối tượng thiết lập cấu hình hiệu ứng cường hóa vũ khí
    /// </summary>
    public class WeaponEnhanceConfigXML
    {
        /// <summary>
        /// Thiết lập theo ngũ hành
        /// </summary>
        public class EffectBySeries
        {
            /// <summary>
            /// Thông tin hiệu ứng
            /// </summary>
            public class EffectDetails
            {
                /// <summary>
                /// Cấp cường hóa
                /// </summary>
                public int Level { get; private set; }

                /// <summary>
                /// Màu số 1 ở thân vũ khí
                /// </summary>
                public Color BodyGlowColorFrom { get; private set; }

                /// <summary>
                /// Màu số 2 ở thân vũ khí
                /// </summary>
                public Color BodyGlowColorTo { get; private set; }

                /// <summary>
                /// Ngưỡng ở thân vũ khí
                /// </summary>
                public int BodyGlowThreshold { get; private set; }

                /// <summary>
                /// Alpha ở thân vũ khí
                /// </summary>
                public float BodyAlpha { get; private set; }

                /// <summary>
                /// Màu ở hiệu ứng điểm nháy
                /// </summary>
                public Color StarColor { get; private set; }

                /// <summary>
                /// Alpha ở hiệu ứng điểm nháy
                /// </summary>
                public float StarAlpha { get; private set; }

                /// <summary>
                /// Chuyển đối tượng từ XML Node
                /// </summary>
                /// <param name="xmlNode"></param>
                /// <returns></returns>
                public static EffectDetails Parse(XElement xmlNode)
                {
                    Color color;
                    EffectDetails effectDetails = new EffectDetails();
                    effectDetails.Level = int.Parse(xmlNode.Attribute("Level").Value);
                    ColorUtility.TryParseHtmlString(xmlNode.Attribute("BodyGlowColorFrom").Value, out color);
                    effectDetails.BodyGlowColorFrom = color;
                    ColorUtility.TryParseHtmlString(xmlNode.Attribute("BodyGlowColorTo").Value, out color);
                    effectDetails.BodyGlowColorTo = color;
                    effectDetails.BodyGlowThreshold = int.Parse(xmlNode.Attribute("BodyGlowThreshold").Value);
                    effectDetails.BodyAlpha = int.Parse(xmlNode.Attribute("BodyAlpha").Value) / 100f;

                    ColorUtility.TryParseHtmlString(xmlNode.Attribute("StarColor").Value, out color);
                    effectDetails.StarColor = color;
                    effectDetails.StarAlpha = int.Parse(xmlNode.Attribute("StarAlpha").Value) / 100f;

                    return effectDetails;
                }

                /// <summary>
                /// Tạo bản sao của đối tượng
                /// </summary>
                /// <returns></returns>
                public EffectDetails Clone()
                {
                    return new EffectDetails()
                    {
                        Level = this.Level,
                        BodyAlpha = this.BodyAlpha,
                        BodyGlowColorFrom = this.BodyGlowColorFrom,
                        BodyGlowColorTo = this.BodyGlowColorTo,
                        BodyGlowThreshold = this.BodyGlowThreshold,
                        StarAlpha = this.StarAlpha,
                        StarColor = this.StarColor,
                    };
                }

                /// <summary>
                /// Thay đổi giá trị
                /// </summary>
                /// <param name="addBodyGlowThreshold"></param>
                /// <param name="addBodyAlpha"></param>
                /// <param name="addStarAlpha"></param>
                public void ChangeValue(int addBodyGlowThreshold, float addBodyAlpha, float addStarAlpha)
                {
                    this.BodyGlowThreshold += addBodyGlowThreshold;
                    this.BodyAlpha += addBodyAlpha;
                    this.StarAlpha += addStarAlpha;
                }
            }

            /// <summary>
            /// Ngũ hành
            /// </summary>
            public Elemental Series { get; private set; }

            /// <summary>
            /// Danh sách hiệu ứng theo cấp
            /// </summary>
            public Dictionary<int, EffectDetails> EffectsByLevel { get; private set; }

            /// <summary>
            /// Chuyển đối tượng từ XML
            /// </summary>
            /// <param name="xmlNode"></param>
            /// <returns></returns>
            public static EffectBySeries Parse(XElement xmlNode)
            {
                EffectBySeries effectBySeries = new EffectBySeries();
                effectBySeries.Series = (Elemental) int.Parse(xmlNode.Attribute("Series").Value);
                effectBySeries.EffectsByLevel = new Dictionary<int, EffectDetails>();

                foreach (XElement node in xmlNode.Elements("Detail"))
                {
                    EffectDetails effectDetails = EffectDetails.Parse(node);
                    effectBySeries.EffectsByLevel[effectDetails.Level] = effectDetails;
                }

                return effectBySeries;
            }

            /// <summary>
            /// Tạo bản sao của đối tượng
            /// </summary>
            /// <returns></returns>
            public EffectBySeries Clone()
            {
                EffectBySeries effectBySeries = new EffectBySeries();
                effectBySeries.Series = this.Series;
                effectBySeries.EffectsByLevel = new Dictionary<int, EffectDetails>();

                foreach (KeyValuePair<int, EffectDetails> pair in this.EffectsByLevel)
                {
                    effectBySeries.EffectsByLevel[pair.Key] = pair.Value.Clone();
                }

                return effectBySeries;
            }
        }

        /// <summary>
        /// Loại vũ khí
        /// </summary>
        public int Category { get; private set; }

        /// <summary>
        /// Danh sách hiệu ứng theo ngũ hành tương ứng
        /// </summary>
        public Dictionary<Elemental, EffectBySeries> EffectsBySeries { get; private set; }

        /// <summary>
        /// Chuyển đối tượng từ XML
        /// </summary>
        /// <param name="xmlNode"></param>
        /// <returns></returns>
        public static WeaponEnhanceConfigXML Parse(XElement xmlNode)
        {
            WeaponEnhanceConfigXML weaponEnhanceConfigXML = new WeaponEnhanceConfigXML();
            weaponEnhanceConfigXML.Category = int.Parse(xmlNode.Attribute("Category").Value);
            weaponEnhanceConfigXML.EffectsBySeries = new Dictionary<Elemental, EffectBySeries>();

            foreach (XElement node in xmlNode.Elements("Effect"))
            {
                EffectBySeries effectBySeries = EffectBySeries.Parse(node);
                weaponEnhanceConfigXML.EffectsBySeries[effectBySeries.Series] = effectBySeries;
            }

            return weaponEnhanceConfigXML;
        }

        /// <summary>
        /// Tạo bản sao của đối tượng
        /// </summary>
        /// <returns></returns>
        public WeaponEnhanceConfigXML Clone()
        {
            WeaponEnhanceConfigXML weaponEnhanceConfigXML = new WeaponEnhanceConfigXML();
            weaponEnhanceConfigXML.Category = this.Category;
            weaponEnhanceConfigXML.EffectsBySeries = new Dictionary<Elemental, EffectBySeries>();

            foreach (KeyValuePair<Elemental, EffectBySeries> pair in this.EffectsBySeries)
            {
                weaponEnhanceConfigXML.EffectsBySeries[pair.Key] = pair.Value.Clone();
            }

            return weaponEnhanceConfigXML;
        }

        /// <summary>
        /// Thay đổi giá trị loại vũ khí
        /// </summary>
        /// <param name="category"></param>
        public void ChangeCategory(int category)
        {
            this.Category = category;
        }
    }
}
