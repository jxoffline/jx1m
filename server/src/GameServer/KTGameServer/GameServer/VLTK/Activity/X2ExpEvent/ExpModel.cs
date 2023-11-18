using ProtoBuf;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace GameServer.VLTK.Core.Activity.X2ExpEvent
{

    [ProtoContract]
    public class UpdateServerModel
    {
        /// <summary>
        /// Status này trả về trạng thái của gameserver
        /// 1 : Bảo trì
        /// 2 : Đang Đầy
        /// 3 : Gần Đầy
        /// 4 : Tốt
        /// </summary>
        [ProtoMember(1)]
        public int Status { get; set; }

        /// <summary>
        /// Trạng thái Update 
        /// </summary>
        /// 
        [ProtoMember(2)]
        public string NotifyUpdate { get; set; }


        /// <summary>
        /// Đây là máy chủ nào
        /// </summary>
        [ProtoMember(3)]
        public int SeverID { get; set; }

    }
    [XmlRoot(ElementName = "StartTime")]
    public class StartTime
    {
        [XmlAttribute(AttributeName = "HOUR")]
        public int HOUR { get; set; }

        [XmlAttribute(AttributeName = "MINUTE")]
        public int MINUTE { get; set; }

        [XmlAttribute(AttributeName = "SECOND")]
        public int SECOND { get; set; }

        [XmlAttribute(AttributeName = "DUALTION")]
        public int DUALTION { get; set; }
    }

    [XmlRoot(ElementName = "ExpMutipleModel")]
    public class ExpMutipleModel
    {

        [XmlAttribute(AttributeName = "ExpRate")]
        public double ExpRate { get; set; } = 1.0;


        [XmlAttribute(AttributeName = "MoneyRate")]
        public double MoneyRate { get; set; } = 1.0;


        [XmlAttribute(AttributeName = "DayOfWeek")]
        public List<int> DayOfWeek { get; set; }


        [XmlElement(ElementName = "StartTime")]
        public StartTime StartTime { get; set; }
    }


    public enum EXPSTATE
    {
        NOT_OPEN,
        OPEN,
        CLOSE,
    }
}
