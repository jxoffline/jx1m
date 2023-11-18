using FS.VLTK;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Server.Data
{
    /// <summary>
    /// Thực thể vật phẩm sẽ ROLL
    /// </summary>
    [ProtoContract]
    [XmlRoot(ElementName = "AwardItem")]
    public class AwardItem
    {
        /// <summary>
        /// ID vật phẩm
        /// </summary>
        [ProtoMember(1)]
        [XmlAttribute(AttributeName = "ItemID")]
        public int ItemID { get; set; }

        /// <summary>
        /// Số lượng
        /// </summary>
        [ProtoMember(2)]
        [XmlAttribute(AttributeName = "Number")]
        public int Number { get; set; }

        /// <summary>
        /// RATE sẽ random ra
        /// </summary>
        [ProtoMember(3)]
        [XmlAttribute(AttributeName = "Rate")]
        public int Rate { get; set; }

    }

    /// <summary>
    /// Danh sách vật phẩm
    /// </summary>
    [ProtoContract]
    [XmlRoot(ElementName = "EveryDayOnLine")]
    public class EveryDayOnLine
    {
        /// <summary>
        /// Mốc nào
        /// </summary>
        [ProtoMember(1)]
        [XmlAttribute(AttributeName = "StepID")]
        public int StepID { get; set; }

        /// <summary>
        /// Thời gian online
        /// </summary>
        [ProtoMember(2)]
        [XmlAttribute(AttributeName = "TimeSecs")]
        public int TimeSecs { get; set; }

        /// <summary>
        /// Danh sách vật phẩm sẽ ROLLL
        /// </summary>
        [ProtoMember(3)]
        [XmlElement(ElementName = "RollAwardItem")]
        public List<AwardItem> RollAwardItem { get; set; }
    }

    [ProtoContract]
    [XmlRoot(ElementName = "EveryDayOnLineEvent")]
    public class EveryDayOnLineEvent
    {
        /// <summary>
        /// Tên sự kiện
        /// </summary>
        [ProtoMember(1)]
        [XmlAttribute(AttributeName = "Name")]
        public string Name { get; set; }

        /// <summary>
        /// Sự kiện có đang mở hay không
        /// </summary>
        [ProtoMember(2)]
        [XmlAttribute(AttributeName = "IsOpen")]
        public bool IsOpen { get; set; }

        /// <summary>
        /// Danh sách vật phẩm
        /// </summary>
        [ProtoMember(3)]
        [XmlElement(ElementName = "Item")]
        public List<EveryDayOnLine> Item { get; set; }

        /// <summary>
        /// Thằng này đã online được bao nhiêu giây rồi
        /// </summary>
        [ProtoMember(4)]
        [XmlIgnore]
        public int DayOnlineSecond { get; set; }

        /// <summary>
        /// Đã nhận mốc nào rồi
        /// </summary>
        [ProtoMember(5)]
        [XmlIgnore]
        public int EveryDayOnLineAwardStep { get; set; }

        /// <summary>
        /// Item đã nhận trước đó là gì
        /// </summary>
        [ProtoMember(6)]
        [XmlIgnore]
        public string EveryDayOnLineAwardGoodsID { get; set; }

        /// <summary>
        /// Thời điểm nhận được gói tin
        /// </summary>
        public long ReceiveTick { get; set; }

        /// <summary>
        /// Thời gian đã Online hiện tại (Do Client tự xử lý)
        /// </summary>
        public int LocalOnlineSec
        {
            get
            {
                return ((int) (KTGlobal.GetCurrentTimeMilis() - this.ReceiveTick) / 1000) + this.DayOnlineSecond;
            }
        }

        /// <summary>
        /// Có gì để nhận không
        /// </summary>
        public bool HasThingToGet
        {
            get
            {
                /// Duyệt danh sách
                foreach (EveryDayOnLine onlineInfo in this.Item)
                {
                    /// Nếu chưa nhận mốc này
                    if (onlineInfo.StepID > this.EveryDayOnLineAwardStep && this.DayOnlineSecond > onlineInfo.TimeSecs)
                    {
                        return true;
                    }
                }
                /// Không có gì để nhận
                return false;
            }
        }

        /// <summary>
        /// Phần thưởng ở mốc kế tiếp
        /// </summary>
        public int NextAward
        {
            get
            {
                /// Đánh dấu Step Min
                int minStep = int.MaxValue;
                /// Duyệt danh sách
                foreach (EveryDayOnLine onlineInfo in this.Item)
                {
                    /// Nếu chưa nhận mốc này
                    if (minStep > onlineInfo.StepID && onlineInfo.StepID > this.DayOnlineSecond)
                    {
                        minStep = onlineInfo.StepID;
                    }
                }
                /// Không có
                return minStep > this.Item.Count ? - 1 : minStep;
            }
        }

        /// <summary>
        /// Phần thưởng ở mốc hiện tại
        /// </summary>
        public int CurrentAward
        {
            get
            {
                /// Đánh dấu Step Min
                int minStep = int.MaxValue;
                /// Duyệt danh sách
                foreach (EveryDayOnLine onlineInfo in this.Item)
                {
                    /// Nếu chưa nhận mốc này
                    if (minStep > onlineInfo.StepID && onlineInfo.StepID > this.EveryDayOnLineAwardStep && this.DayOnlineSecond > onlineInfo.TimeSecs)
                    {
                        minStep = onlineInfo.StepID;
                    }
                }
                /// Không có
                return minStep > this.Item.Count ? - 1 : minStep;
            }
        }

        /// <summary>
        /// Thông tin phần quà ở mốc hiện tại
        /// </summary>
        public EveryDayOnLine CurrentAwardInfo
        {
            get
            {
                int stepID = this.CurrentAward;
                /// Nếu không có
                if (stepID == -1)
                {
                    return null;
                }

                return this.Item.Where(x => x.StepID == stepID).FirstOrDefault();
            }
        }

        /// <summary>
        /// Trả về ID phần quà đã nhận ở mốc tương ứng
        /// </summary>
        /// <param name="stepID"></param>
        /// <param name="itemID"></param>
        /// <param name="itemNumber"></param>
        /// <returns></returns>
        public bool GetReceivedAwardItemInfo(int stepID, out int itemID, out int itemNumber)
        {
            itemID = -1;
            itemNumber = -1;

            string[] infoParams = this.EveryDayOnLineAwardGoodsID.Split('|');
            try
            {
                string[] para = infoParams[stepID].Split(',');
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
