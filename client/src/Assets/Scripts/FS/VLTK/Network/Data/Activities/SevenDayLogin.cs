using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using ProtoBuf;

namespace Server.Data
{
	[XmlRoot(ElementName = "AwardItem")]
	[ProtoContract]
	public class RollAwardItem
	{
		[XmlAttribute(AttributeName = "ItemID")]
		[ProtoMember(1)]
		public int ItemID { get; set; }

		[XmlAttribute(AttributeName = "Number")]
		[ProtoMember(2)]
		public int Number { get; set; }

		[XmlAttribute(AttributeName = "Rate")]
		[ProtoMember(3)]
		public int Rate { get; set; }
	}

    /// <summary>
    /// Gửi về client cả mả này
    /// </summary>
    [ProtoContract]
    public class SevenDayEvent
    {
        [ProtoMember(1)]
        public SevenDaysLogin SevenDaysLogin { get; set; }
        [ProtoMember(2)]
        public SevenDaysLoginContinus SevenDaysLoginContinus { get; set; }
    }


    [XmlRoot(ElementName = "SevenDaysLoginItem")]
    [ProtoContract]
    public class SevenDaysLoginItem
    {
        [XmlElement(ElementName = "RollAwardItem")]
        [ProtoMember(1)]
        public List<RollAwardItem> RollAwardItem { get; set; }

        [XmlAttribute(AttributeName = "ID")]
        [ProtoMember(2)]
        public int ID { get; set; }

        [XmlAttribute(AttributeName = "Days")]
        [ProtoMember(3)]
        public int Days { get; set; }
    }

    [XmlRoot(ElementName = "SevenDaysLogin")]
    [ProtoContract]
    public class SevenDaysLogin
    {
        [XmlElement(ElementName = "Item")]
        [ProtoMember(1)]
        public List<SevenDaysLoginItem> SevenDaysLoginItem { get; set; }

        [XmlAttribute(AttributeName = "Name")]
        [ProtoMember(2)]
        public string Name { get; set; }

        [XmlAttribute(AttributeName = "IsOpen")]
        [ProtoMember(3)]
        public bool IsOpen { get; set; }

        /// <summary>
        /// Đây đang là ngày thứ mấy tính từ lúc nhân vật được tạo
        /// </summary>
        [ProtoMember(4)]
        public int DayID { get; set; }

        /// <summary>
        /// Thằng này đã nhận những mốc nào rồi
        /// For từ 1 tới DayID nếu mà không nằm trong RevicedHistory tức là thằng này đã bị quá hạn nhận
        /// Cái nào nằm trong ReviceHistory thì tức là đã nhận
        /// Nếu DAYID chưa quá mốc tối đa trong SevenDaysLoginItem thì hiện có thể nhận cho nhân vật nhận
        /// </summary>
        [ProtoMember(5)]
        public List<SevenDayLoginHistoryItem> RevicedHistory { get; set; }

        /// <summary>
        /// Có phần quà có thể nhận hôm nay không
        /// </summary>
        public bool HasSomethingToGet
        {
            get
            {
                /// Kiểm tra nếu có trong lịch sử nghĩa là đã nhận rồi
                if (this.RevicedHistory != null && this.RevicedHistory.Any(x => x.DayID == this.DayID))
                {
                    return false;
                }
                return true;
            }
        }

        /// <summary>
        /// Thông tin phần quà sẽ nhận được hôm nay
        /// </summary>
        public SevenDaysLoginItem CurrentAwardInfo
        {
            get
            {
                /// Nếu không có quà nhận
                if (!this.HasSomethingToGet)
                {
                    return null;
                }

                /// Thông tin quà tương ứng
                SevenDaysLoginItem awardInfo = this.SevenDaysLoginItem.Where(x => x.Days == this.DayID).FirstOrDefault();
                /// Trả ra kết quả
                return awardInfo;
            }
        }

        /// <summary>
        /// Chưa nhận quà ở ngày tương ứng và đã bị quá hạn không
        /// </summary>
        /// <param name="day"></param>
        /// <returns></returns>
        public bool IsOutOfDate(int day)
        {
            /// Đã nhận rồi thì không phải
            if (this.HasAlreadyGotten(day))
            {
                return false;
            }
            /// Trả về kết quả
            return day < this.DayID;
        }

        /// <summary>
        /// Đã nhận phần quà ở ngày tương ứng chưa
        /// </summary>
        /// <param name="day"></param>
        /// <returns></returns>
        public bool HasAlreadyGotten(int day)
        {
            /// Kiểm tra nếu có trong lịch sử nghĩa là đã nhận rồi
            if (this.RevicedHistory != null && this.RevicedHistory.Any(x => x.DayID == day))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Có thể nhận phần quà ở ngày này không
        /// </summary>
        /// <param name="day"></param>
        /// <returns></returns>
        public bool CanGet(int day)
        {
            /// Nếu đã nhận rồi thì thôi
            if (this.HasAlreadyGotten(day))
            {
                return false;
            }
            /// Nếu quá hạn
            else if (day < this.DayID)
            {
                return false;
            }
            /// Nếu chưa đến ngày thì thôi
            else if (day > this.DayID)
            {
                return false;
            }
            /// Mút
            return true;
        }

        /// <summary>
        /// Trả về ID phần quà đã nhận ở mốc tương ứng
        /// </summary>
        /// <param name="days"></param>
        /// <param name="itemID"></param>
        /// <param name="itemNumber"></param>
        /// <returns></returns>
        public bool GetReceivedAwardItemInfo(int days, out int itemID, out int itemNumber)
        {
            itemID = -1;
            itemNumber = -1;

            /// Thông tin
            SevenDayLoginHistoryItem historyItem = this.RevicedHistory?.Where(x => x.DayID == days).FirstOrDefault();
            /// Toác
            if (historyItem == null)
            {
                return false;
            }
            /// Trả về kết quả
            itemID = historyItem.GoodIDs;
            itemNumber = historyItem.GoodNum;
            /// OK
            return true;
        }
    }

    /// <summary>
    /// Lịch sử lưu lại những gì thằng này đã nhận
    /// </summary>
    [ProtoContract]
    public class SevenDayLoginHistoryItem
    {
        /// <summary>
        /// Ngày nào
        /// </summary>
        [ProtoMember(1)]
        public int DayID { get; set; }

        /// <summary>
        /// Vật phẩm đã nhận
        /// </summary>
        [ProtoMember(2)]
        public int GoodIDs { get; set; }

        /// <summary>
        /// ID vật phẩm đã nhận
        /// </summary>
        [ProtoMember(3)]
        public int GoodNum { get; set; }
    }


    [XmlRoot(ElementName = "SevenDaysLoginContinus")]
    [ProtoContract]
    public class SevenDaysLoginContinus
    {
        [XmlElement(ElementName = "Item")]
        [ProtoMember(1)]
        public List<SevenDaysLoginItem> SevenDaysLoginItem { get; set; }

        [XmlAttribute(AttributeName = "Name")]
        [ProtoMember(2)]
        public string Name { get; set; }

        [XmlAttribute(AttributeName = "IsOpen")]
        [ProtoMember(3)]
        public bool IsOpen { get; set; }

        /// <summary>
        /// Số ngày đã đăng nhập liên tiếp
        /// Chỗ này sẽ hiện ở client là bạn đã đăng nhập liên tiếp X ngày
        /// Bạn có thể nhận thưởng mốc tương ứng
        /// </summary>
        [ProtoMember(4)]
        public int TotalDayLoginContinus { get; set; }

        /// <summary>
        /// Đã nhận tới mốc nào rồi
        /// </summary>
        [ProtoMember(5)]
        public int Step { get; set; }

        /// <summary>
        /// Quà đã nhận khi đăng nhập liên tiếp trong 7 ngày | Cái này sẽ ko reset suốt đời
        /// Lịch sử đã mút cái j trước đó
        /// ITEMID_ITEMNUM _ ITEMID_ITEMNUM
        /// </summary>
        [ProtoMember(6)]
        public string SevenDayLoginAward { get; set; }

        /// <summary>
        /// Có phần quà có thể nhận hôm nay không
        /// </summary>
        public bool HasSomethingToGet
        {
            get
            {
                if (this.Step >= this.TotalDayLoginContinus)
                {
                    return false;
                }
                return true;
            }
        }

        /// <summary>
        /// Trả về phần quà có thể nhận ngày hôm nay
        /// </summary>
        public SevenDaysLoginItem CurrentAwardInfo
        {
            get
            {
                /// Nếu không có gì có thể nhận thì thôi
                if (!this.HasSomethingToGet)
                {
                    return null;
                }
                /// Nếu có thì là
                return this.SevenDaysLoginItem.Where(x => x.Days == this.Step + 1).FirstOrDefault();
            }
        }

        /// <summary>
        /// Đã nhận phần quà ở ngày tương ứng chưa
        /// </summary>
        /// <param name="day"></param>
        /// <returns></returns>
        public bool HasAlreadyGotten(int day)
        {
            return this.Step >= day;
        }

        /// <summary>
        /// Có thể nhận phần quà ở ngày này không
        /// </summary>
        /// <param name="day"></param>
        /// <returns></returns>
        public bool CanGet(int day)
        {
            /// Nếu đã nhận rồi thì thôi
            if (this.HasAlreadyGotten(day))
            {
                return false;
            }
            /// Nếu chưa đến ngày thì thôi
            else if (day > this.TotalDayLoginContinus)
            {
                return false;
            }
            /// Mút
            return true;
        }

        /// <summary>
        /// Trả về ID phần quà đã nhận ở mốc tương ứng
        /// </summary>
        /// <param name="days"></param>
        /// <param name="itemID"></param>
        /// <param name="itemNumber"></param>
        /// <returns></returns>
        public bool GetReceivedAwardItemInfo(int days, out int itemID, out int itemNumber)
        {
            itemID = -1;
            itemNumber = -1;

            string[] infoParams = this.SevenDayLoginAward.Split('|');
            try
            {
                string[] para = infoParams[days].Split('_');
                if (para.Length != 2)
                {
                    return false;
                }

                itemID = int.Parse(para[0]);
                itemNumber = int.Parse(para[1]);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }
    }
}
