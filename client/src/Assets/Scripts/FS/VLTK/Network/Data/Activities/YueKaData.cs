using FS.VLTK;
using FS.VLTK.Entities.Config;
using FS.VLTK.Loader;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Server.Data
{
    /// <summary>
    /// Thông tin phần thưởng trong thẻ ngày tương ứng
    /// </summary>
    [XmlRoot(ElementName = "Card")]
    [ProtoContract]
    public class Card
    {
        /// <summary>
        /// Ngày
        /// </summary>
        [ProtoMember(1)]
        [XmlAttribute(AttributeName = "Day")]
        public int Day { get; set; }

        /// <summary>
        /// Thông tin vật phẩm (ID,Số lượng)
        /// </summary>
        [ProtoMember(2)]
        [XmlAttribute(AttributeName = "ItemCard")]
        public string ItemCard { get; set; }

        /// <summary>
        /// Vật phẩm tương ứng
        /// </summary>
        public GoodsData Item
		{
			get
			{
                try
				{
                    string[] fields = this.ItemCard.Split(',');
                    int itemID = int.Parse(fields[0]);
                    int number = int.Parse(fields[1]);

                    /// Thông tin vật phẩm
                    if (Loader.Items.TryGetValue(itemID, out ItemData itemData))
					{
                        GoodsData itemGD = KTGlobal.CreateItemPreview(itemData);
                        itemGD.Binding = 1;
                        itemGD.GCount = number;
                        return itemGD;
					}
                    return null;
                }
                catch (Exception)
				{
                    return null;
				}
                
			}
		}
    }

    /// <summary>
    /// Cấu hình thẻ tháng
    /// </summary>
    [ProtoContract]
    [XmlRoot(ElementName = "ConfigCard")]
    public class ConfigCard
    {
        /// <summary>
        /// Danh sách phần thưởng ngày
        /// </summary>
        [ProtoMember(1)]
        [XmlElement(ElementName = "Card")]
        public List<Card> Card { get; set; }
    }

    [ProtoContract]
    public class YueKaData
    {
        /// <summary>
        /// Có thẻ tháng hay không
        /// </summary>
        [ProtoMember(1)]
        public bool HasYueKa { get; set; }

        /// <summary>
        /// Ngày thứ bao nhiêu
        /// </summary>
        [ProtoMember(2)]
        public int CurrDay { get; set; }

        /// <summary>
        /// Đánh dấu đã nhận những ngày nào rồi
        /// <para>Quy tắc là: Các ngày viết liền nhau, 2 giá trị 0: Chưa nhận, 1: Đã nhận. Tổng số ngày là độ dài của chuỗi</para>
        /// </summary>
        [ProtoMember(3)]
        public string AwardInfo { get; set; }

        /// <summary>
        /// Còn lại bao nhiêu ngày
        /// </summary>
        [ProtoMember(4)]
        public int RemainDay { get; set; }

        /// <summary>
        /// Config của nó
        /// </summary>
        [ProtoMember(5)]
        public ConfigCard Config { get; set; }

        /// <summary>
        /// Số KNB khóa nhận thêm (mỗi ngày đều được nhận)
        /// </summary>
        [ProtoMember(6)]
        public int BoundToken { get; set; }

        /// <summary>
        /// Slogan thẻ tháng
        /// </summary>
        [ProtoMember(7)]
        public string Slogan { get; set; }

        /// <summary>
        /// Trả về trạng thái ở ngày tương ứng
        /// <para>0: Chưa nhận, 1: Đã nhận, 2: Chưa đến ngày</para>
        /// </summary>
        /// <param name="day"></param>
        /// <returns></returns>
        public int GetStateAtDay(int day)
		{
            /// Nếu không có thông tin ngày
            if (string.IsNullOrEmpty(this.AwardInfo))
			{
                return -1;
			}
            /// Nếu toác
            else if (day < 0)
			{
                return 1;
			}
            /// Nếu chưa đến ngày này
            else if (day > this.CurrDay)
			{
                return -1;
			}

            /// Nếu là ngày hôm nay và chưa nhận
            if (day == this.CurrDay && this.AwardInfo.Length < this.CurrDay)
			{
                return 0;
			}

            /// Nếu đã quá hạn
            if (day < this.CurrDay)
            {
                return 2;
            }

            /// Trả về trạng thái ở ngày tương ứng
            return this.AwardInfo[day - 1] - '0';
		}

        /// <summary>
        /// Thiết lập trạng thái ngày tương ứng
        /// </summary>
        /// <param name="day"></param>
        /// <param name="state"></param>
        public void SetStateAtDay(int day, int state)
		{
            /// Nếu không có thông tin ngày
            if (string.IsNullOrEmpty(this.AwardInfo))
			{
                return;
			}

            string result = "";
            for (int i = 0; i < this.AwardInfo.Length; i++)
			{
                /// Nếu là ngày cần thiết lập
                if (i == day - 1)
				{
                    result += state.ToString();
				}
				else
				{
                    result += this.AwardInfo[i];
				}
			}
            this.AwardInfo = result;
		}
    }
}