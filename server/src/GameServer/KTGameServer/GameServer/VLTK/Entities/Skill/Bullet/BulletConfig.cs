using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace GameServer.KiemThe.Entities
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
        /// Có quay trở lại vị trí ban đầu không
        /// <para>Chỉ áp dụng với đạn bay từ A đến B</para>
        /// </summary>
        public bool IsComeback { get; set; }

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
        /// Tỷ lệ xuyên suốt mục tiêu
        /// </summary>
        public int PieceThroughTargetsPercent { get; set; }

        /// <summary>
        /// Số mục tiêu chạm phải tối đa
        /// <para>Thuộc tính này KHÔNG quy định kỹ năng có xuyên suốt mục tiêu không</para>
        /// </summary>
        public int MaxTargetTouch { get; set; }

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
                IsComeback = int.Parse(xmlNode.Attribute("IsComeback").Value) == 1,
                StartHeight = int.Parse(xmlNode.Attribute("StartHeight").Value),
                ExplodeRadius = int.Parse(xmlNode.Attribute("ExplodeRadius").Value),
                DamageInterval = int.Parse(xmlNode.Attribute("DamageInterval").Value),
                LifeTime = int.Parse(xmlNode.Attribute("LifeTime").Value),
                MoveSpeed = int.Parse(xmlNode.Attribute("MoveSpeed").Value),
                LoopAnimation = int.Parse(xmlNode.Attribute("LoopAnimation").Value) == 1,
                ExplodeRadiusAddTimes = int.Parse(xmlNode.Attribute("ExplodeRadiusAddTimes").Value),
                PieceThroughTargetsPercent = int.Parse(xmlNode.Attribute("PieceThroughTargetsPercent").Value),
                MaxTargetTouch = int.Parse(xmlNode.Attribute("MaxTargetTouch").Value),
            };
        }
    }
}
