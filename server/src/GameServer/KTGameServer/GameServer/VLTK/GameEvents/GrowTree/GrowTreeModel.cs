using GameServer.Logic;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace GameServer.VLTK.GameEvents.GrowTree
{


    /// <summary>
    /// Grow tree
    /// </summary>
    public class GrowTreePlayer
    {

        /// <summary>
        ///  Thẳng này là ở bang nào để tý nữa tính tích lũy xem bang nào nhất
        /// </summary>
        public int GuildID { get; set; }

        public KPlayer client { get; set; }

        public int Point { get; set; }

        public int CollectTotal { get; set; }


        public int KillCount { get; set; }

        public int CurentKillSteak { get; set; }
        /// <summary>
        /// Giết liên tiếp bao nhiêu thằng
        /// </summary>
        public int MaxKillSteak { get; set; }
    }

    public enum GROWTREE_STATE
    {
        NOT_OPEN,
        PREDING_OPEN,
        OPEN,
        CLOSE,
        CLEAR,

    }
    /// <summary>
    /// Thực thể hạt hoàng kim
    /// </summary>

    [XmlRoot(ElementName = "GrowTreeModel")]
    public class GrowTreeModel
    {
        /// <summary>
        /// Tọa độ X đặt cây
        /// </summary>
        ///

        [XmlAttribute(AttributeName = "Name")]
        public string Name { get; set; }

        [XmlAttribute(AttributeName = "PosX")]
        public int PosX { get; set; }

        /// <summary>
        /// Tọa độ đặt Y của cây
        /// </summary>

        [XmlAttribute(AttributeName = "PosY")]
        public int PosY { get; set; }


        /// <summary>
        /// Res ID của cây
        /// </summary>

        [XmlAttribute(AttributeName = "ResID")]
        public int ResID { get; set; }


        /// <summary>
        /// Thời gian mà cây sẽ hồi sinh
        /// </summary>

        [XmlAttribute(AttributeName = "RespwanTime")]
        public int RespwanTime { get; set; }


        /// <summary>
        ///  Thời gian sẽ thu thập
        /// </summary>
        [XmlAttribute(AttributeName = "CollectTick")]
        public int CollectTick { get; set; }



        /// <summary>
        /// Chất lượng của
        /// </summary>
        ///
        [XmlAttribute(AttributeName = "Quality")]
        public int Quality { get; set; }

    }


    /// <summary>
    /// Thực thể quà random sẽ rơi ra khi tham gia hạn hoàng kim
    /// </summary>

    [XmlRoot(ElementName = "GrowAward")]
    public class GrowAward
    {
        /// <summary>
        /// Phẩm chất của hạt sẽ rơi ra tùy theo loại quả đã config
        /// </summary>

        [XmlAttribute(AttributeName = "Quality")]
        public int Quality { get; set; }

        /// <summary>
        /// Vật phẩm ID sẽ rơi ra
        /// </summary>

        [XmlAttribute(AttributeName = "ItemID")]
        public int ItemID { get; set; }

        /// <summary>
        /// Số lượng vật phẩm sẽ rơi ra
        /// </summary>

        [XmlAttribute(AttributeName = "ItemNum")]
        public int ItemNum { get; set; }

        /// <summary>
        /// Tỉ lệ ra cái đồ này là bao nhiêu Tổng rate là 100%
        /// </summary>

        [XmlAttribute(AttributeName = "Rate")]
        public int Rate { get; set; }

        /// <summary>
        /// Quả này có ra EXP không
        /// </summary>

        [XmlAttribute(AttributeName = "IsExp")]
        public bool IsExp { get; set; }

        /// <summary>
        /// Quả này có ra bạc hay không?
        /// </summary>

        [XmlAttribute(AttributeName = "IsMoney")]
        public bool IsMoney { get; set; }

    }

    [XmlRoot(ElementName = "RespwanPoint")]
    public class RespwanPoint
    {
        [XmlAttribute(AttributeName = "PosX")]
        public int PosX { get; set; }

        [XmlAttribute(AttributeName = "PosY")]
        public int PosY { get; set; }
    }


    /// <summary>
    /// Config
    /// </summary>

    [XmlRoot(ElementName = "GrowTreeConfig")]
    public class GrowTreeConfig
    {
        /// <summary>
        /// Danh sách cây
        /// </summary>

        [XmlElement(ElementName = "GrowTrees")]
        public List<GrowTreeModel> GrowTrees { get; set; }

        /// <summary>
        /// Tổng số phần thưởng có thể rơi ra từ hạt tùy theo phẩm chất
        /// </summary>
        ///
        [XmlElement(ElementName = "GrowAwards")]
        public List<GrowAward> GrowAwards { get; set; }

        /// <summary>
        /// Thời gian sẽ bắt đầu
        /// Giờ phút giây
        /// </summary>

        [XmlAttribute(AttributeName = "TimeStart")]
        public List<int> TimeStart { get; set; }


        /// <summary>
        /// Thời gian sẽ diễn ra sự kiện tính bằng giây
        /// </summary>

        [XmlAttribute(AttributeName = "Dualtion")]
        public int Dualtion { get; set; }

        /// <summary>
        /// Diễn ra tại bản đồ nào
        /// </summary>

        [XmlAttribute(AttributeName = "MapID")]
        public int MapID { get; set; }


        /// <summary>
        /// Ngày nào sẽ diễn ra sự kiện
        /// </summary>

        [XmlAttribute(AttributeName = "DayOfWeek")]
        public List<int> DayOfWeek { get; set; }


        /// <summary>
        /// Cấp  độ tối thiểu có thể giam gia
        /// </summary>
        ///
        [XmlAttribute(AttributeName = "MinLevelCanJoin")]
        public int MinLevelCanJoin { get; set; }


        [XmlElement(ElementName = "RespwanPoints")]
        public List<RespwanPoint> RespwanPoints { get; set; }
    }

}
