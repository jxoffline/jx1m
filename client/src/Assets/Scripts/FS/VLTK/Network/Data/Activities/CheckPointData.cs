using FS.VLTK.Entities.Config;
using FS.VLTK.Loader;
using FS.VLTK;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace Server.Data
{
    [XmlRoot(ElementName = "CheckPointItem")]
    [ProtoContract]
    public class CheckPointItem
    {
        /// <summary>
        /// Số thứ tự của ngày tính từ 1-30 không tính ngày 31
        /// </summary>
        [ProtoMember(1)]
        [XmlAttribute(AttributeName = "Day")]
        public int Day { get; set; }

        /// <summary>
        /// Vật phẩm
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

    [ProtoContract]
    [XmlRoot(ElementName = "ConfigCard")]
    public class CheckPontConfig
    {
        [ProtoMember(1)]
        [XmlElement(ElementName = "CheckPointItem")]
        public List<CheckPointItem> CheckPointItem { get; set; }

        [ProtoMember(2)]
        [XmlAttribute(AttributeName = "Name")]
        public string Name { get; set; }

        [ProtoMember(3)]
        [XmlAttribute(AttributeName = "IsOpen")]
        public bool IsOpen { get; set; }

        /// <summary>
        /// Lịch sử các ngày đã nhận của thằng này
        /// Còn các ngày còn lại không nằm trong chuỗi này nếu nhỏ hơn DAYID thì là quá hạn
        /// Cao hơn thì là chưa nhận
        /// = DayID là có thể nhận SIMPLE
        /// Cấu trúc 1_2_3_5 là đã nhận ngày 1 2 3 5
        /// </summary>
        [ProtoMember(4)]
        public string HistoryRevice { get; set; }

        /// <summary>
        /// Ngày hiện tại là ngày nào
        /// </summary>
        [ProtoMember(5)]
        public int DayID { get; set; }

        /// <summary>
        /// Trả về trạng thái ở ngày tương ứng
        /// </summary>
        /// <param name="day"></param>
        /// <returns></returns>
        public int GetStateAtDay(int day)
        {
            /// Nếu chưa đến ngày
            if (day > this.DayID)
            {
                return -1;
            }

            /// Lịch sử
            List<int> history = string.IsNullOrEmpty(this.HistoryRevice) ? new List<int>() : this.HistoryRevice.Split('_').Select(x => int.Parse(x)).ToList();

            /// Nếu đã nhận rồi
            if (history.Contains(day))
            {
                return 1;
            }

            /// Nếu quá hạn
            if (day < this.DayID)
            {
                return 2;
            }

            /// Có thể nhận
            return 0;
        }

        /// <summary>
        /// Thiết lập trạng thái ở ngày tương ứng
        /// </summary>
        /// <param name="day"></param>
        /// <param name="state"></param>
        public void SetStateAtDay(int day, int state)
        {
            /// Lịch sử
            List<int> history = string.IsNullOrEmpty(this.HistoryRevice) ? new List<int>() : this.HistoryRevice.Split('_').Select(x => int.Parse(x)).ToList();
            /// Nếu đã tồn tại thì bỏ qua
            if (history.Contains(day))
            {
                return;
            }
            /// Thêm vào
            history.Add(day);
            /// Lưu lại lịch sử
            this.HistoryRevice = string.Join("_", history);
        }
    }
}
