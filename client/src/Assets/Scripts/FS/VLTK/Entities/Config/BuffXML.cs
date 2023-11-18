using GameServer.VLTK.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static FS.VLTK.Entities.Enum;

namespace FS.VLTK.Entities.Config
{
    /// <summary>
    /// Đối tượng quản lý Buff
    /// </summary>
    public class BuffXML
    {
        /// <summary>
        /// Thông tin Buff
        /// </summary>
        public class BuffProperty
        {
            /// <summary>
            /// Loại thuộc tính Buff
            /// </summary>
            public BuffEffectType Type { get; set; }

            /// <summary>
            /// Loại mục tiêu
            /// </summary>
            public BuffTargetType TargetType { get; set; }

            /// <summary>
            /// Thuộc tính
            /// </summary>
            public PropertyDictionary Properties { get; set; }
        }

        /// <summary>
        /// Thông tin Buff hỗ trợ
        /// </summary>
        public class SupportBuffProperty
        {
            /// <summary>
            /// ID buff được hỗ trợ
            /// </summary>
            public int SupportBuffID { get; set; }

            /// <summary>
            /// Loại thuộc tính Buff
            /// </summary>
            public BuffEffectType Type { get; set; }

            /// <summary>
            /// Loại mục tiêu
            /// </summary>
            public BuffTargetType TargetType { get; set; }

            /// <summary>
            /// Thuộc tính
            /// </summary>
            public PropertyDictionary Properties { get; set; }
        }

        /// <summary>
        /// ID Buff
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// ID hiệu ứng
        /// </summary>
        public int StateEffectID { get; set; }

        /// <summary>
        /// Buff có lợi
        /// </summary>
        public bool IsPositive { get; set; }

        /// <summary>
        /// Tên Buff
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Mô tả Buff
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// ID nhóm Buff
        /// </summary>
        public int GroupID { get; set; }

        /// <summary>
        /// Thứ tự ưu tiên trong nhóm
        /// </summary>
        public int GroupOrder { get; set; }

        /// <summary>
        /// Thời gian duy trì (-1 nếu vĩnh viễn)
        /// </summary>
        public float Duration { get; set; }

        /// <summary>
        /// Thời gian kích hoạt mỗi khoảng (-1 nếu không có tác dụng)
        /// </summary>
        public float ActivatePeriod { get; set; }

        /// <summary>
        /// Khi chết vẫn bảo lưu
        /// </summary>
        public bool KeepOnDeath { get; set; }

        /// <summary>
        /// Sinh lực tối thiểu, thấp hơn ngưỡng này thì Buff tự xóa (-1 vô hiệu)
        /// </summary>
        public int HP { get; set; }

        /// <summary>
        /// Nội lực tối thiểu, thấp hơn ngưỡng này thì Buff tự xóa (-1 vô hiệu)
        /// </summary>
        public int MP { get; set; }

        /// <summary>
        /// Thể lực tối thiểu, thấp hơn ngưỡng này thì Buff tự xóa (-1 vô hiệu)
        /// </summary>
        public int Vitality { get; set; }

        /// <summary>
        /// % sinh lực tối thiểu, thấp hơn ngưỡng này thì Buff tự xóa (-1 vô hiệu)
        /// </summary>
        public int MinHPPercent { get; set; }

        /// <summary>
        /// % nội lực tối thiểu, thấp hơn ngưỡng này thì Buff tự xóa (-1 vô hiệu)
        /// </summary>
        public int MinMPPercent { get; set; }

        /// <summary>
        /// % thể lực tối thiểu, thấp hơn ngưỡng này thì Buff tự xóa (-1 vô hiệu)
        /// </summary>
        public int MinVitalityPercent { get; set; }

        /// <summary>
        /// Thuộc tính Buff khi kích hoạt
        /// </summary>
        public List<BuffProperty> Activate { get; set; }

        /// <summary>
        /// Thuộc tính Buff hỗ trợ Buff khác khi kích hoạt
        /// </summary>
        public List<SupportBuffProperty> SupportActivateBuff { get; set; }

        /// <summary>
        /// Thuộc tính Buff chạy liên tục mỗi khoảng
        /// </summary>
        public List<BuffProperty> Period { get; set; }

        /// <summary>
        /// Thuộc tính Buff hỗ trợ Buff khác khi chạy liên tục mỗi khoảng
        /// </summary>
        public List<SupportBuffProperty> SupportPeriodBuff { get; set; }

        /// <summary>
        /// Thuộc tính Buff khi hết thời gian
        /// </summary>
        public List<BuffProperty> Timeout { get; set; }

        /// <summary>
        /// Thuộc tính Buff hỗ trợ Buff khác khi hết thời gian
        /// </summary>
        public List<SupportBuffProperty> SupportTimeoutBuff { get; set; }


        /// <summary>
        /// Chuyển đối tượng từ XML Node
        /// </summary>
        /// <param name="xmlNode"></param>
        /// <returns></returns>
        public static BuffXML Parse(XElement xmlNode)
        {
            BuffXML buff = new BuffXML()
            {
                ID = int.Parse(xmlNode.Attribute("ID").Value),
                StateEffectID = int.Parse(xmlNode.Attribute("StateEffectID").Value),
                IsPositive = int.Parse(xmlNode.Attribute("IsPositive").Value) == 1,
                Name = xmlNode.Attribute("Name").Value,
                Description = xmlNode.Attribute("Desc").Value,
                GroupID = int.Parse(xmlNode.Attribute("GroupID").Value),
                GroupOrder = int.Parse(xmlNode.Attribute("GroupOrder").Value),
                Duration = float.Parse(xmlNode.Attribute("Duration").Value),
                ActivatePeriod = int.Parse(xmlNode.Attribute("ActivatePeriod").Value),
                KeepOnDeath = int.Parse(xmlNode.Attribute("KeepOnDeath").Value) == 1,

                MinHPPercent = xmlNode.Attribute("MinHPPercent") != null ? int.Parse(xmlNode.Attribute("MinHPPercent").Value) : -1,
                MinMPPercent = xmlNode.Attribute("MinMPPercent") != null ? int.Parse(xmlNode.Attribute("MinMPPercent").Value) : -1,
                MinVitalityPercent = xmlNode.Attribute("MinVitalityPercent") != null ? int.Parse(xmlNode.Attribute("MinVitalityPercent").Value) : -1,

                MP = xmlNode.Attribute("MP") != null ? int.Parse(xmlNode.Attribute("MP").Value) : -1,
                HP = xmlNode.Attribute("HP") != null ? int.Parse(xmlNode.Attribute("HP").Value) : -1,
                Vitality = xmlNode.Attribute("Vitality") != null ? int.Parse(xmlNode.Attribute("Vitality").Value) : -1,

                Activate = new List<BuffProperty>(),
                SupportActivateBuff = new List<SupportBuffProperty>(),
                Period = new List<BuffProperty>(),
                SupportPeriodBuff = new List<SupportBuffProperty>(),
                Timeout = new List<BuffProperty>(),
                SupportTimeoutBuff = new List<SupportBuffProperty>(),
            };

            #region Activate
            foreach (XElement element in xmlNode.Element("Activate").Elements("Property"))
            {
                BuffProperty property = new BuffProperty()
                {
                    Type = (BuffEffectType) int.Parse(element.Attribute("Type").Value),
                    TargetType = (BuffTargetType) int.Parse(element.Attribute("TargetType").Value),
                };
                buff.Activate.Add(property);

                property.Properties = PropertyDictionary.Parse(element);
            }
            #endregion

            #region Period
            foreach (XElement element in xmlNode.Element("Period").Elements("Property"))
            {
                BuffProperty property = new BuffProperty()
                {
                    Type = (BuffEffectType) int.Parse(element.Attribute("Type").Value),
                    TargetType = (BuffTargetType) int.Parse(element.Attribute("TargetType").Value),
                };
                buff.Period.Add(property);

                property.Properties = PropertyDictionary.Parse(element);
            }
            #endregion

            #region Timeout
            foreach (XElement element in xmlNode.Element("Timeout").Elements("Property"))
            {
                BuffProperty property = new BuffProperty()
                {
                    Type = (BuffEffectType) int.Parse(element.Attribute("Type").Value),
                    TargetType = (BuffTargetType) int.Parse(element.Attribute("TargetType").Value),
                };
                buff.Timeout.Add(property);

                property.Properties = PropertyDictionary.Parse(element);
            }
            #endregion

            #region SupportActivateBuff
            foreach (XElement element in xmlNode.Element("SupportBuff").Element("Activate").Elements("Property"))
            {
                SupportBuffProperty property = new SupportBuffProperty()
                {
                    Type = (BuffEffectType) int.Parse(element.Attribute("Type").Value),
                    TargetType = (BuffTargetType) int.Parse(element.Attribute("TargetType").Value),
                };
                buff.SupportActivateBuff.Add(property);

                property.Properties = PropertyDictionary.Parse(element);
            }
            #endregion

            #region SupportPeriodBuff
            foreach (XElement element in xmlNode.Element("SupportBuff").Element("Period").Elements("Property"))
            {
                SupportBuffProperty property = new SupportBuffProperty()
                {
                    Type = (BuffEffectType) int.Parse(element.Attribute("Type").Value),
                    TargetType = (BuffTargetType) int.Parse(element.Attribute("TargetType").Value),
                };
                buff.SupportPeriodBuff.Add(property);

                property.Properties = PropertyDictionary.Parse(element);
            }
            #endregion

            #region SupportTimeoutBuff
            foreach (XElement element in xmlNode.Element("SupportBuff").Element("Timeout").Elements("Property"))
            {
                SupportBuffProperty property = new SupportBuffProperty()
                {
                    Type = (BuffEffectType) int.Parse(element.Attribute("Type").Value),
                    TargetType = (BuffTargetType) int.Parse(element.Attribute("TargetType").Value),
                };
                buff.SupportTimeoutBuff.Add(property);

                property.Properties = PropertyDictionary.Parse(element);
            }
            #endregion

            return buff;
        }
    }
}
