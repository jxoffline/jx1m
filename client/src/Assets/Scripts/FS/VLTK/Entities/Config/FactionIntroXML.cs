using System.Xml.Linq;

namespace FS.VLTK.Entities.Config
{
    /// <summary>
    /// Thông tin giới thiệu môn phái
    /// </summary>
    public class FactionIntroXML
    {
        /// <summary>
        /// ID phái
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// Mô tả môn phái
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Đặc điểm môn phái
        /// </summary>
        public string TypeDesc { get; set; }

        /// <summary>
        /// Độ khó
        /// </summary>
        public int DiffRate { get; set; }

        /// <summary>
        /// Icon phái
        /// </summary>
        public string Icon { get; set; }

        /// <summary>
        /// Icon phái ở trạng thái kích hoạt
        /// </summary>
        public string ActiveIcon { get; set; }

        /// <summary>
        /// Logo của phái
        /// </summary>
        public string Logo { get; set; }

        /// <summary>
        /// Bundle chứa Video giới thiệu môn phái
        /// </summary>
        public string VideoBundle { get; set; }

        /// <summary>
        /// Video giới thiệu môn phái
        /// </summary>
        public string Video { get; set; }

        /// <summary>
        /// Chuyển đối tượng từ XMLNode
        /// </summary>
        /// <param name="xmlNode"></param>
        /// <returns></returns>
        public static FactionIntroXML Parse(XElement xmlNode)
        {
            return new FactionIntroXML()
            {
                ID = int.Parse(xmlNode.Attribute("ID").Value),
                Description = xmlNode.Attribute("Description").Value,
                TypeDesc = xmlNode.Attribute("TypeDesc").Value,
                DiffRate = int.Parse(xmlNode.Attribute("DiffRate").Value),
                Icon = xmlNode.Attribute("Icon").Value,
                ActiveIcon = xmlNode.Attribute("ActiveIcon").Value,
                Logo = xmlNode.Attribute("Logo").Value,
                VideoBundle = xmlNode.Attribute("VideoBundle").Value,
                Video = xmlNode.Attribute("Video").Value,
            };
        }
    }
}
