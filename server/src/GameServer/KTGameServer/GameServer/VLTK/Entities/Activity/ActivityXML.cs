using System.Xml.Linq;

namespace GameServer.KiemThe.Entities
{
    /// <summary>
    /// Mô tả thông tin sự kiện trong Game
    /// </summary>
    public class ActivityXML
    {
        /// <summary>
        /// ID sự kiện
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// Tên sự kiện
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Loại sự kiện
        /// <para>Tham biến 1: Loại thời gian (0: 1 lần duy nhất sau khi khởi động lại Server, 1: Mỗi ngày, 2: Mỗi tuần, 3: Mỗi tháng vào ngày chỉ định, 4: Mỗi tháng vào tuần chỉ định, 5: Thời gian cố định)</para>
        /// </summary>
        public ActivityType Type { get; set; }

        /// <summary>
        /// Thiết lập thời gian, ngăn cách nhau bởi dấu _
        /// <para>Nếu Type = 0 thì sẽ là chuỗi rỗng ""</para>
        /// <para>Nếu Type = 1, thì sẽ là dạng "HH;MM" tương ứng Giờ, phút</para>
        /// <para>Nếu Type = 2, thì sẽ là dạng "dd;HH;MM" tương ứng Thứ tự ngày trong tuần (0-6), Giờ, phút</para>
        /// <para>Nếu Type = 3, thì sẽ là dạng "DD;HH;MM" tương ứng Thứ tự ngày trong tháng (1-31), Giờ, phút</para>
        /// <para>Nếu Type = 4, thì sẽ là dạng "WW;dd;HH;MM" tương ứng Thứ tự tuần trong tháng (0-3), Thứ tự ngày trong tuần (0-6), Giờ, phút</para>
        /// <para>Nếu Type = 5, thì sẽ là dạng "DD;MM;YYYY;HH;MM" tương ứng Ngày, Tháng, Năm, Giờ, phút</para>
        /// </summary>
        public string Time { get; set; }

        /// <summary>
        /// Thời gian tồn tại
        /// </summary>
        public long DurationTicks { get; set; }

        /// <summary>
        /// Tên Script điều khiển
        /// </summary>
        public string CoreScript { get; set; }

        /// <summary>
        /// Chuyển đối tượng từ XML Node
        /// </summary>
        /// <param name="xmlNode"></param>
        /// <returns></returns>
        public static ActivityXML Parse(XElement xmlNode)
        {
            int activityType = int.Parse(xmlNode.Attribute("Type").Value);
            if (activityType < (int)ActivityType.None || activityType >= (int)ActivityType.Count)
            {
                activityType = (int)ActivityType.None;
            }
            return new ActivityXML()
            {
                ID = int.Parse(xmlNode.Attribute("ID").Value),
                Name = xmlNode.Attribute("Name").Value,
                Type = (ActivityType)activityType,
                Time = xmlNode.Attribute("Time").Value,
                DurationTicks = long.Parse(xmlNode.Attribute("DurationTicks").Value),
                CoreScript = xmlNode.Attribute("CoreScript").Value,
            };
        }
    }
}
