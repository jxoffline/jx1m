using System.Collections.Generic;
using System.Xml.Linq;

namespace FS.VLTK.Entities.Config
{
    /// <summary>
    /// Thiết lập Pet
    /// </summary>
    public class PetConfigXML
    {
        /// <summary>
        /// Thông tin sách kỹ năng
        /// </summary>
        public class SkillScroll
        {
            /// <summary>
            /// ID vật phẩm sách
            /// </summary>
            public int ItemID { get; set; }

            /// <summary>
            /// ID kỹ năng tương ứng
            /// </summary>
            public int SkillID { get; set; }

            /// <summary>
            /// Chuyển đối tượng từ XMLNode
            /// </summary>
            /// <param name="xmlNode"></param>
            /// <returns></returns>
            public static SkillScroll Parse(XElement xmlNode)
            {
                return new SkillScroll()
                {
                    ItemID = int.Parse(xmlNode.Attribute("ItemID").Value),
                    SkillID = int.Parse(xmlNode.Attribute("SkillID").Value),
                };
            }
        }

        /// <summary>
        /// Thông tin thăng cấp kỹ năng
        /// </summary>
        public class SkillLevelUpData
        {
            /// <summary>
            /// Tỷ lệ thành công %
            /// </summary>
            public int Rate { get; set; }

            /// <summary>
            /// Số điểm lĩnh ngộ yêu cầu
            /// </summary>
            public int RequireEnlightenment { get; set; }

            /// <summary>
            /// Chuyển đối tượng từ XMLNode
            /// </summary>
            /// <param name="xmlNode"></param>
            /// <returns></returns>
            public static SkillLevelUpData Parse(XElement xmlNode)
            {
                return new SkillLevelUpData()
                {
                    Rate = int.Parse(xmlNode.Attribute("Rate").Value),
                    RequireEnlightenment = int.Parse(xmlNode.Attribute("RequireEnlightenment").Value),
                };
            }
        }

        /// <summary>
        /// Thông tin vật phẩm chức năng
        /// </summary>
        public class FeedItem
        {
            /// <summary>
            /// ID vật phẩm
            /// </summary>
            public int ItemID { get; set; }

            /// <summary>
            /// Số điểm tăng thêm
            /// </summary>
            public int Point { get; set; }

            /// <summary>
            /// Chuyển đối tượng từ XMLNode
            /// </summary>
            /// <param name="xmlNode"></param>
            /// <returns></returns>
            public static FeedItem Parse(XElement xmlNode)
            {
                return new FeedItem()
                {
                    ItemID = int.Parse(xmlNode.Attribute("ItemID").Value),
                    Point = int.Parse(xmlNode.Attribute("Point").Value),
                };
            }
        }

        /// <summary>
        /// Số lượng Pet tối đa mỗi người chơi có thể mang theo
        /// </summary>
        public int MaxCanTake { get; set; }

        /// <summary>
        /// Cấp độ tối đa
        /// </summary>
        public int MaxLevel { get; set; }

        /// <summary>
        /// Số kỹ năng tối đa
        /// </summary>
        public int MaxSkill { get; set; }

        /// <summary>
        /// Số điểm vui vẻ tối đa
        /// </summary>
        public int MaxJoy { get; set; }

        /// <summary>
        /// Số điểm tuổi thọ tối đa
        /// </summary>
        public int MaxLife { get; set; }

        /// <summary>
        /// ID kỹ năng cơ bản đánh thường
        /// </summary>
        public int BaseAttackSkillID { get; set; }

        /// <summary>
        /// Thăng cấp được bao nhiêu điểm tiềm năng
        /// </summary>
        public int LevelUpRemainPoints { get; set; }

        /// <summary>
        /// Số điểm tuổi thọ mất khi chết
        /// </summary>
        public int DieLoseLife { get; set; }

        /// <summary>
        /// Xuất chiến yêu cầu phải có tối thiểu bao nhiêu điểm vui vẻ
        /// </summary>
        public int CallFightRequịreJoyOver { get; set; }

        /// <summary>
        /// Xuất chiến yêu cầu phải có tối thiểu bao nhiêu điểm tuổi thọ
        /// </summary>
        public int CallFightRequịreLifeOver { get; set; }

        /// <summary>
        /// Xuất chiến tiêu hao bao nhiêu điểm vui vẻ
        /// </summary>
        public int CallFightCostJoy { get; set; }

        /// <summary>
        /// Thời gian mỗi khoảng sẽ giảm độ vui vẻ của tinh linh đi 1 điểm
        /// </summary>
        public int SubJoyInterval { get; set; }

        /// <summary>
        /// % kinh nghiệm nhận được từ chủ nhân khi giết quái
        /// </summary>
        public int OwnerExpP { get; set; }

        /// <summary>
        /// Vật phẩm yêu cầu để tẩy điểm thuộc tính pet
        /// </summary>
        public int ResetAttributesItemID { get; set; }

        /// <summary>
        /// Vật phẩm cần có khi học kỹ năng nếu không muốn đè lên kỹ năng cũ
        /// </summary>
        public int SkillStudyMedicineItemID { get; set; }

        /// <summary>
        /// Danh sách vật phẩm tăng độ vui vẻ
        /// </summary>
        public Dictionary<int, FeedItem> FeedJoyItems { get; set; }

        /// <summary>
        /// Danh sách vật phẩm tăng tuổi thọ
        /// </summary>
        public Dictionary<int, FeedItem> FeedLifeItems { get; set; }

        /// <summary>
        /// Danh sách vật phẩm tăng lĩnh ngộ
        /// </summary>
        public Dictionary<int, FeedItem> FeedEnlightenmentItems { get; set; }

        /// <summary>
        /// Danh sách kinh nghiệm yêu cầu thăng cấp
        /// </summary>
        public List<int> LevelUpExps { get; set; }

        /// <summary>
        /// Danh sách sách kỹ năng pet
        /// </summary>
        public Dictionary<int, SkillScroll> SkillScrolls { get; set; }

        /// <summary>
        /// Danh sách thông tin thăng cấp kỹ năng
        /// </summary>
        public List<SkillLevelUpData> SkillLevelUps { get; set; }

        /// <summary>
        /// Danh sách tỷ lệ học mới kỹ năng
        /// </summary>
        public List<int> SkillStudyRates { get; set; }

        /// <summary>
        /// Chuyển đối tượng từ XMLNode
        /// </summary>
        /// <param name="xmlNode"></param>
        /// <returns></returns>
        public static PetConfigXML Parse(XElement xmlNode)
        {
            /// Tạo mới
            PetConfigXML petConfig = new PetConfigXML()
            {
                MaxCanTake = int.Parse(xmlNode.Element("Limitation").Attribute("MaxCanTake").Value),
                MaxLevel = int.Parse(xmlNode.Element("Limitation").Attribute("MaxLevel").Value),
                MaxSkill = int.Parse(xmlNode.Element("Limitation").Attribute("MaxSkill").Value),
                MaxJoy = int.Parse(xmlNode.Element("Limitation").Attribute("MaxJoy").Value),
                MaxLife = int.Parse(xmlNode.Element("Limitation").Attribute("MaxLife").Value),
                BaseAttackSkillID = int.Parse(xmlNode.Element("Base").Attribute("BaseAttackSkillID").Value),
                LevelUpRemainPoints = int.Parse(xmlNode.Element("Base").Attribute("LevelUpRemainPoints").Value),
                DieLoseLife = int.Parse(xmlNode.Element("Base").Attribute("DieLoseLife").Value),
                CallFightRequịreJoyOver = int.Parse(xmlNode.Element("Base").Attribute("CallFightRequịreJoyOver").Value),
                CallFightRequịreLifeOver = int.Parse(xmlNode.Element("Base").Attribute("CallFightRequịreLifeOver").Value),
                CallFightCostJoy = int.Parse(xmlNode.Element("Base").Attribute("CallFightCostJoy").Value),
                SubJoyInterval = int.Parse(xmlNode.Element("Base").Attribute("SubJoyInterval").Value),
                OwnerExpP = int.Parse(xmlNode.Element("Base").Attribute("OwnerExpP").Value),
                ResetAttributesItemID = int.Parse(xmlNode.Element("Items").Attribute("ResetAttributesItemID").Value),
                SkillStudyMedicineItemID = int.Parse(xmlNode.Element("Items").Attribute("SkillStudyMedicineItemID").Value),
                FeedJoyItems = new Dictionary<int, FeedItem>(),
                FeedLifeItems = new Dictionary<int, FeedItem>(),
                FeedEnlightenmentItems = new Dictionary<int, FeedItem>(),
                LevelUpExps = new List<int>(),
                SkillScrolls = new Dictionary<int, SkillScroll>(),
                SkillLevelUps = new List<SkillLevelUpData>(),
                SkillStudyRates = new List<int>(),
            };

            /// Duyệt danh sách vật phẩm tăng độ vui vẻ
            foreach (XElement node in xmlNode.Element("Feed").Element("Joyful").Elements("Item"))
            {
                /// Thông tin vật phẩm
                FeedItem item = FeedItem.Parse(node);
                /// Thêm vào danh sách
                petConfig.FeedJoyItems[item.ItemID] = item;
            }
            /// Duyệt danh sách vật phẩm tăng tuổi thọ
            foreach (XElement node in xmlNode.Element("Feed").Element("Life").Elements("Item"))
            {
                /// Thông tin vật phẩm
                FeedItem item = FeedItem.Parse(node);
                /// Thêm vào danh sách
                petConfig.FeedLifeItems[item.ItemID] = item;
            }
            /// Duyệt danh sách vật phẩm tăng điểm lĩnh ngộ
            foreach (XElement node in xmlNode.Element("Feed").Element("Enlightenment").Elements("Item"))
            {
                /// Thông tin vật phẩm
                FeedItem item = FeedItem.Parse(node);
                /// Thêm vào danh sách
                petConfig.FeedEnlightenmentItems[item.ItemID] = item;
            }

            /// Duyệt danh sách kinh nghiệm thăng cấp
            foreach (XElement node in xmlNode.Element("ExpList").Elements("Exp"))
            {
                /// Kinh nghiệm yêu cầu
                int exp = int.Parse(node.Attribute("Value").Value);
                /// Thêm vào danh sách
                petConfig.LevelUpExps.Add(exp);
            }

            /// Duyệt danh sách sách kỹ năng
            foreach (XElement node in xmlNode.Element("Scrolls").Elements("Scroll"))
            {
                SkillScroll scroll = SkillScroll.Parse(node);
                petConfig.SkillScrolls[scroll.ItemID] = scroll;
            }

            /// Duyệt danh sách thăng cấp kỹ năng
            foreach (XElement node in xmlNode.Element("SkillLevelUp").Elements("LevelUpData"))
            {
                SkillLevelUpData skillLevelUpData = SkillLevelUpData.Parse(node);
                petConfig.SkillLevelUps.Add(skillLevelUpData);
            }

            /// Duyệt danh sách tỷ lệ học mới kỹ năng
            foreach (XElement node in xmlNode.Element("SkillStudyRates").Elements("Data"))
            {
                int value = int.Parse(node.Attribute("StudyNewRate").Value);
                petConfig.SkillStudyRates.Add(value);
            }

            /// Trả về kết quả
            return petConfig;
        }
    }
}
