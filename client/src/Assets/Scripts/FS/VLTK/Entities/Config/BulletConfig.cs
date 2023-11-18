using System.Xml.Linq;

namespace FS.VLTK.Entities.Config
{
    /// <summary>
    /// Cấu hình đạn
    /// </summary>
    public class BulletConfig
    {
        /// <summary>
        /// ID đạn
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// Tên đạn
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Hình dạng di chuyển
        /// <para>0: Đứng yên một chỗ</para>
        /// <para>1: Bay theo đường thẳng</para>
        /// </summary>
        public int MoveKind { get; set; }

        /// <summary>
        /// Đuổi mục tiêu
        /// </summary>
        public bool IsFollowTarget { get; set; }

        /// <summary>
        /// Độ cao ban đầu
        /// </summary>
        public int StartHeight { get; set; }

        /// <summary>
        /// Phạm vi nổ
        /// </summary>
        public int ExplodeRadius { get; set; }

        /// <summary>
        /// Thời gian nổ vi nổ
        /// </summary>
        public int DamageInterval { get; set; }

        /// <summary>
        /// Thời gian tồn tại
        /// </summary>
        public int LifeTime { get; set; }

        /// <summary>
        /// Tốc độ bay
        /// </summary>
        public int MoveSpeed { get; set; }

        /// <summary>
        /// Hiệu ứng có lặp lại không
        /// </summary>
        public bool LoopAnimation { get; set; }

        /// <summary>
        /// Phạm vi nổ cộng thêm
        /// <para>1: Gấp 2 giá trị hiện tại</para>
        /// <para>2: Gấp 3 giá trị hiện tại</para>
        /// </summary>
        public int ExplodeRadiusAddTimes { get; set; }

        /// <summary>
        /// Chuyển đối tượng từ XML Node
        /// </summary>
        /// <param name="xmlNode"></param>
        /// <returns></returns>
        public static BulletConfig Parse(XElement xmlNode)
        {
            return new BulletConfig()
            {
                ID = int.Parse(xmlNode.Attribute("ID").Value),
                Name = xmlNode.Attribute("Name").Value,
                MoveKind = int.Parse(xmlNode.Attribute("MoveKind").Value),
                IsFollowTarget = int.Parse(xmlNode.Attribute("IsFollowTarget").Value) == 1,
                StartHeight = int.Parse(xmlNode.Attribute("StartHeight").Value),
                ExplodeRadius = int.Parse(xmlNode.Attribute("ExplodeRadius").Value),
                DamageInterval = int.Parse(xmlNode.Attribute("DamageInterval").Value),
                LifeTime = int.Parse(xmlNode.Attribute("LifeTime").Value),
                MoveSpeed = int.Parse(xmlNode.Attribute("MoveSpeed").Value),
                LoopAnimation = int.Parse(xmlNode.Attribute("LoopAnimation").Value) == 1,
                ExplodeRadiusAddTimes = int.Parse(xmlNode.Attribute("ExplodeRadiusAddTimes").Value),
            };
        }
    }
}
