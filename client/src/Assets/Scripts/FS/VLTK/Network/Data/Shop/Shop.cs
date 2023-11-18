
using FS.GameEngine.Logic;
using FS.VLTK;
using FS.VLTK.Entities.Config;
using FS.VLTK.Loader;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using static FS.VLTK.Entities.Enum;

namespace Server.Data
{
    /// <summary>
    /// Giới hạn vật phẩm
    /// </summary>
    public enum LimitType
    {
        /// <summary>
        /// không có giới hạn
        /// </summary>
        NoLimit = -1,
        /// <summary>
        /// mua theo ngày
        /// </summary>
        BuyCountPerDay,
        /// <summary>
        /// Mua theo tuần
        /// </summary>
        BuyCountPerWeek,
        /// <summary>
        /// mua theo số lượng
        /// </summary>
        LimitCount,
        /// <summary>
        /// Limit theo thời gian
        /// </summary>
        LimitTime,
    }

    /// <summary>
    /// SHOP TABLE
    /// </summary>
    /// 
    [ProtoContract]
    [XmlRoot(ElementName = "ShopTab")]
    public class ShopTab
    {
        /// <summary>
        /// Tên SHOP
        /// </summary>
        [ProtoMember(1)]
        [XmlAttribute(AttributeName = "ShopName")]
        public string ShopName { get; set; }

        /// <summary>
        /// ID SHOP
        /// </summary>
        [ProtoMember(2)]
        [XmlAttribute(AttributeName = "ShopID")]
        public int ShopID { get; set; }

        /// <summary>
        /// DANH SÁCH VẬT PHẨM
        /// </summary>
        [XmlElement(ElementName = "ShopItem")]
        public List<int> ShopItem { get; set; }


        [ProtoMember(3)]
        [XmlIgnore]
        public List<ShopItem> Items { get; set; }
        /// <summary>
        /// KIỂU TIỀN | 0 Bạc khóa | 1  Bạc | 2  KNB | 3  KNB khóa
        /// </summary>
        [ProtoMember(4)]
        [XmlAttribute(AttributeName = "MoneyType")]
        public int MoneyType { get; set; }

        /// <summary>
        /// Có giảm giá không | Nếu cứ lớn hơn 0 thì lấy giá mặc định % vào là ra giá giảm
        /// </summary>
        [ProtoMember(5)]
        [XmlAttribute(AttributeName = "Discount")]
        public int Discount { get; set; }

        /// <summary>
        /// Thời gian bắt đầu bán cái này chỉ máy chủ quan tâm
        /// </summary>
        [ProtoMember(6)]
        [XmlAttribute(AttributeName = "TimeSaleStart")]
        public string TimeSaleStart { get; set; }

        /// <summary>
        /// Client có thể đọc và hiện luôn trên TOP của CỬA HÀNG
        /// </summary>
        [ProtoMember(7)]
        [XmlAttribute(AttributeName = "TimeSaleEnd")]
        public string TimeSaleEnd { get; set; }

        /// <summary>
        /// Danh sách vật phẩm đang bán
        /// <para>GS dùng</para>
        /// </summary>
        [ProtoMember(8)]
        [XmlIgnore]
        public List<GoodsData> TotalSellItem { get; set; }
        
        /// <summary>
        /// Chuyển đối tượng về dạng String
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("ID = {0}, Name = {1}", this.ShopID, this.ShopName);
        }
    }

    [ProtoContract]
    [XmlRoot(ElementName = "ShopItem")]
    public class ShopItem
    {
        /// <summary>
        /// Thông tin cửa hàng tương ứng
        /// </summary>
        public ShopTab ShopTab { get; set; }

        /// <summary>
        /// Giá bán cũ khi không có giảm giá
        /// </summary>
        public int OldPrice
        {
            get
            {
                if (Loader.Items.TryGetValue(this.ItemID, out ItemData itemData))
                {
                    return itemData.Price;
                }
                return 0;
            }
        }

        /// <summary>
        /// Giá bán hiện tại khi có giảm giá
        /// </summary>
        public int Price
        {
            get
            {
                /// Nếu là shop ở kỳ trân các
                if (this.ShopTab.MoneyType == (int) MoneyType.Dong || this.ShopTab.MoneyType == (int) MoneyType.DongKhoa)
                {
                    return this.GoodsPrice;
                }
                else
                {
                    if (Loader.Items.TryGetValue(this.ItemID, out ItemData itemData))
                    {
                        if (this.ShopTab.Discount > 0)
                        {
                            return itemData.Price * this.ShopTab.Discount / 100;
                        }
                        else
                        {
                            return itemData.Price;
                        }
                    }
                    return 0;
                }
            }
        }

        /// <summary>
        /// Có giảm giá không
        /// </summary>
        public bool IsDiscount
        {
            get
            {
                return this.ShopTab.Discount > 0;
            }
        }

        /// <summary>
        /// ID thứ tự vật phẩm trên shop
        /// </summary>
        ///
        [ProtoMember(1)]
        [XmlAttribute(AttributeName = "ID")]
        public int ID { get; set; }

        /// <summary>
        /// ID template
        /// </summary>
        ///
        [ProtoMember(2)]
        [XmlAttribute(AttributeName = "ItemID")]
        public int ItemID { get; set; }

        /// <summary>
        /// Ngũ hành đồ
        /// </summary>
        ///
        [ProtoMember(3)]
        [XmlAttribute(AttributeName = "Series")]
        public int Series { get; set; }

        /// <summary>
        /// Khóa hay không khóa
        /// </summary>
        ///
        [ProtoMember(4)]
        [XmlAttribute(AttributeName = "Bind")]
        public int Bind { get; set; }

        /// <summary>
        /// Yêu cầu danh vọng hay không
        /// Nếu là -1 thì không yêu càu
        /// </summary>
        [ProtoMember(5)]
        [XmlAttribute(AttributeName = "ReputeDBID")]
        public int ReputeDBID { get; set; }

        /// <summary>
        /// Cấp độ của loại danh vọng tương ứng
        /// </summary>
        [ProtoMember(6)]
        [XmlAttribute(AttributeName = "ReputeLevel")]
        public int ReputeLevel { get; set; }

        /// <summary>
        /// Cấp quan ấn | Trí sự v....
        /// </summary>
        [ProtoMember(7)]
        [XmlAttribute(AttributeName = "OfficialLevel")]
        public int OfficialLevel { get; set; }

        /// <summary>
        /// Yêu cầu bậc phi phong khi mua phi phong
        /// </summary>
        [ProtoMember(8)]
        [XmlAttribute(AttributeName = "Honor")]
        public int Honor { get; set; }

        /// <summary>
        /// Thời hạn sử dụng của vật phẩm sau khi mua
        /// </summary>
        [ProtoMember(9)]
        [XmlAttribute(AttributeName = "Expiry")]
        public int Expiry { get; set; }

        /// <summary>
        /// Điểm cống hiện bang hội khi mua
        /// </summary>
        [ProtoMember(10)]
        [XmlAttribute(AttributeName = "TongFund")]
        public int TongFund { get; set; }

        /// <summary>
        /// Yêu cầu vật phẩm nào đó
        /// </summary>
        [ProtoMember(11)]
        [XmlAttribute(AttributeName = "GoodsIndex")]
        public int GoodsIndex { get; set; }

        /// <summary>
        /// Số lượng vật phẩm yêu cầu
        /// </summary>
        [ProtoMember(12)]
        [XmlAttribute(AttributeName = "GoodsPrice")]
        public int GoodsPrice { get; set; }

        /// <summary>
        /// Giới hạn mua | -1  Không giới hạn | 0 : Giới hạn số lượng mua /1 ngày | 1 : Giới hạn số lượng mua trên tuần | 2 : Giới hạn mua cả đời
        /// </summary>
        [ProtoMember(13)]
        [XmlAttribute(AttributeName = "LimitType")]
        public int LimitType { get; set; }

        /// <summary>
        /// Giá trị giới hạn
        /// </summary>
        [ProtoMember(14)]
        [XmlAttribute(AttributeName = "LimitValue")]
        public int LimitValue { get; set; }

        /// <summary>
        /// Thời gian hết hạn nếu là Limit theo thời gian
        /// </summary>
        public TimeSpan EndTime { get; set; }

        /// <summary>
        /// Chuyển đối tượng về dạng string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("ID = {0}, ItemID = {1}", this.ID, this.ItemID);
        }
    }

    /// <summary>
    /// Kỳ Trân Các
    /// </summary>
    [ProtoContract]
    public class TokenShop
    {
        /// <summary>
        /// Danh sách Shop trong tiệm KNB
        /// </summary>
        [ProtoMember(1)]
        public List<ShopTab> Token { get; set; }

        /// <summary>
        /// Danh sách Shop trong tiệm KNB khóa
        /// </summary>
        [ProtoMember(2)]
        public List<ShopTab> BoundToken { get; set; }

        /// <summary>
        /// Danh sách gói hàng bán trên Store
        /// </summary>
        [ProtoMember(3)]
        public List<TokenShopStoreProduct> StoreProducts { get; set; }
    }



    /// <summary>
    /// Thông tin gói hàng bán trên Service Store (App store, Play store, ...)
    /// </summary>
    [ProtoContract]
    public class TokenShopStoreProduct
    {
        /// <summary>
        /// ID gói hàng
        /// </summary>
        [ProtoMember(1)]
        public string ID { get; set; }

        /// <summary>
        /// Tên gói hàng
        /// </summary>
        [ProtoMember(2)]
        public string Name { get; set; }

        /// <summary>
        /// Hint gói hàng
        /// </summary>
        [ProtoMember(3)]
        public string Hint { get; set; }

        /// <summary>
        /// Icon gói hàng
        /// </summary>
        [ProtoMember(4)]
        public string Icon { get; set; }

        /// <summary>
        /// Khuyến nghị mua không
        /// </summary>
        [ProtoMember(5)]
        public bool Recommend { get; set; }

        /// <summary>
        /// Giá mua
        /// </summary>
        [ProtoMember(6)]
        public int Price { get; set; }
    }
}