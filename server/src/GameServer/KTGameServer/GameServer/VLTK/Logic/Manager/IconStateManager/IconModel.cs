using System.Collections.Generic;
using System.Xml.Serialization;

namespace GameServer.KiemThe.Core.IconStateManager
{
    /// <summary>
    /// Sự kiện tương tác với Button chức năng
    /// </summary>
    public enum FunctionButtonAction
    {
        /// <summary>
        /// Hiện
        /// </summary>
        Show,
        /// <summary>
        /// Ẩn
        /// </summary>
        Hide,
        /// <summary>
        /// Kích hoạt
        /// </summary>
        Enable,
        /// <summary>
        /// Hủy kích hoạt
        /// </summary>
        Disable,
        /// <summary>
        /// Hint
        /// </summary>
        Hint,
    }

    /// <summary>
    /// Loại Button chức năng ở Main UI
    /// </summary>
    public enum FunctionButtonType
    {
        /// <summary>
        /// Mở Kỳ Trân Các
        /// </summary>
        OpenTokenShop,
        /// <summary>
        /// Mở khung quay sò
        /// </summary>
        OpenSeashellCircle,
        /// <summary>
        /// Mở khung phúc lợi nạp thẻ lần đầu
        /// </summary>
        OpenWelfareFirstRecharge,
        /// <summary>
        /// Mở khung phúc lợi
        /// </summary>
        OpenWelfare,
        /// <summary>
        /// Mở khung danh sách hoạt động
        /// </summary>
        OpenActivityList,
        /// <summary>
        /// Mở khung thiết lập hệ thống
        /// </summary>
        OpenSystemSetting,
        /// <summary>
        /// Mở khung bạn bè
        /// </summary>
        OpenFriendBox,
        /// <summary>
        /// Mở khung bang hội
        /// </summary>
        OpenGuildBox,
        /// <summary>
        /// Mở khung nhiệm vụ
        /// </summary>
        OpenTaskBox,
        /// <summary>
        /// Mở khung tìm người chơi
        /// </summary>
        OpenBrowsePlayer,
        /// <summary>
        /// Mở khung thư
        /// </summary>
        OpenMailBox,
        /// <summary>
        /// Mở khung túi đồ
        /// </summary>
        OpenBag,
        /// <summary>
        /// Mở khung thông tin nhân vật
        /// </summary>
        OpenRoleInfo,
        /// <summary>
        /// Mở khung kỹ năng
        /// </summary>
        OpenSkill,
        /// <summary>
        /// Mở khung kỹ năng sống
        /// </summary>
        OpenLifeSkill,
        /// <summary>
        /// Mở khung vòng quay may mắn
        /// </summary>
        OpenLuckyCircle,
    }

    [XmlRoot(ElementName = "MainButton")]
    public class MainButton
    {
        [XmlAttribute(AttributeName = "IconID")]
        public FunctionButtonType IconID { get; set; }

        [XmlAttribute(AttributeName = "Name")]
        public string Name { get; set; }

        [XmlAttribute(AttributeName = "State")]
        public FunctionButtonAction State { get; set; }
    }

    [XmlRoot(ElementName = "IconManager")]
    public class IconManager
    {
        [XmlElement(ElementName = "Icons")]
        public List<MainButton> Icons { get; set; }
    }
}