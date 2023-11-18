using System.Xml.Linq;

namespace FS.VLTK.Entities.Config
{
    /// <summary>
    /// Dữ liệu hiệu ứng
    /// </summary>
    public class StateEffectXML
    {
        /// <summary>
        /// ID hiệu ứng
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// Vị trí hiệu ứng
        /// </summary>
        public string PosType { get; set; }

        /// <summary>
        /// Vị trí đặt X
        /// </summary>
        public int PosX { get; set; }

        /// <summary>
        /// Vị trí đặt Y
        /// </summary>
        public int PosY { get; set; }

        /// <summary>
        /// Layer
        /// </summary>
        public int Layer { get; set; }

        /// <summary>
        /// Đường dẫn file Bundle chứa hiệu ứng
        /// </summary>
        public string EffectBundle { get; set; }

        /// <summary>
        /// Đường dẫn file Bundle chứa Icon
        /// </summary>
        public string IconBundle { get; set; }

        /// <summary>
        /// Tên Icon
        /// </summary>
        public string IconName { get; set; }

        /// <summary>
        /// Lặp
        /// </summary>
        public bool Loop { get; set; }

        /// <summary>
        /// Có phải Buff không
        /// </summary>
        public bool IsBuff { get; set; }

        /// <summary>
        /// Chú thích
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Chuyển đổi tượng từ XML Node
        /// </summary>
        /// <param name="xmlNode"></param>
        /// <returns></returns>
        public static StateEffectXML Parse(XElement xmlNode)
        {
            return new StateEffectXML()
            {
                ID = int.Parse(xmlNode.Attribute("Id").Value),
                PosType = xmlNode.Attribute("PosType").Value,
                PosX = int.Parse(xmlNode.Attribute("PosX").Value),
                PosY = int.Parse(xmlNode.Attribute("PosY").Value),
                Layer = int.Parse(xmlNode.Attribute("Layer").Value),
                EffectBundle = xmlNode.Attribute("EffectBundle").Value,
                IconBundle = xmlNode.Attribute("IconBundle").Value,
                IconName = xmlNode.Attribute("IconName").Value,
                Loop = int.Parse(xmlNode.Attribute("Loop").Value) == 1,
                IsBuff = int.Parse(xmlNode.Attribute("Buff").Value) == 1,
                Description = xmlNode.Attribute("Tip").Value,
            };
        }

        /// <summary>
        /// Tạo bản sao của đối tượng
        /// </summary>
        /// <returns></returns>
        public StateEffectXML Clone()
        {
            return new StateEffectXML()
            {
                ID = this.ID,
                PosType = this.PosType,
                PosX = this.PosX,
                PosY = this.PosY,
                Layer = this.Layer,
                EffectBundle = this.EffectBundle,
                IconBundle = this.IconBundle,
                IconName = this.IconName,
                Loop = this.Loop,
                IsBuff = this.IsBuff,
                Description = this.Description,
            };
        }
    }
}
