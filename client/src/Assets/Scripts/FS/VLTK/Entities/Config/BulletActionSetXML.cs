using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace FS.VLTK.Entities.Config
{
    /// <summary>
    /// Đối tượng quản lý Res đạn
    /// </summary>
    public class BulletActionSetXML
    {
        /// <summary>
        /// Đường dẫn file Bundle chứa âm thanh hiệu ứng đạn
        /// </summary>
        public string SoundBundleDir { get; set; }

        /// <summary>
        /// Đối tượng dữ liệu Res đạn
        /// </summary>
        public class BulletResData
        {
            /// <summary>
            /// ID Res
            /// </summary>
            public int ResID { get; set; }

            /// <summary>
            /// Tên Res
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Đường dẫn file Bundle chứa ảnh hiệu ứng đạn
            /// </summary>
            public string BundleDir { get; set; }

            /// <summary>
            /// Vị trí X
            /// </summary>
            public short PosX { get; set; }

            /// <summary>
            /// Vị trí Y
            /// </summary>
            public short PosY { get; set; }

            /// <summary>
            /// Vị trí nổ X
            /// </summary>
            public short ExplodePosX { get; set; }

            /// <summary>
            /// Vị trí nổ Y
            /// </summary>
            public short ExplodePosY { get; set; }

            /// <summary>
            /// Có động tác bay
            /// </summary>
            public bool HasFlyAction { get; set; }

            /// <summary>
            /// Có động tác tan
            /// </summary>
            public bool HasFadeOutAction { get; set; }

            /// <summary>
            /// Có hiệu ứng nổ
            /// </summary>
            public bool HasExplodeAction { get; set; }

            /// <summary>
            /// Thời gian thực hiện hiệu ứng bay
            /// </summary>
            public int FlyAnimDuration { get; set; }

            /// <summary>
            /// Thời gian thực hiện hiệu ứng tan biến
            /// </summary>
            public int FadeOutAnimDuration { get; set; }

            /// <summary>
            /// Thời gian thực hiện hiệu ứng nổ
            /// </summary>
            public int ExplodeAnimDuration { get; set; }

            /// <summary>
            /// Sử dụng 8 hướng
            /// </summary>
            public bool Use8Dir { get; set; }

            /// <summary>
            /// Sử dụng 16 hướng
            /// </summary>
            public bool Use16Dir { get; set; }

            /// <summary>
            /// Sự dụng 32 hướng
            /// </summary>
            public bool Use32Dir { get; set; }

            /// <summary>
            /// Tự động quay (nếu sử dụng 16 hướng thì lựa chọn này không có tác dụng)
            /// </summary>
            public bool AutoRotate { get; set; }

            /// <summary>
            /// Sử dụng hiệu ứng tạo bóng di chuyển
            /// </summary>
            public bool UseTrailEffect { get; set; }
            
            /// <summary>
            /// Thời gian duy trì bóng
            /// </summary>
            public int TrailDuration { get; set; }

            /// <summary>
            /// Thời gian giãn cách mỗi lần tạo bóng
            /// </summary>
            public int TrailPeriod { get; set; }

            /// <summary>
            /// Chuyển đối tượng từ XMLNode
            /// </summary>
            /// <param name="xmlNode"></param>
            /// <returns></returns>
            public static BulletResData Parse(XElement xmlNode)
			{
                return new BulletResData()
                {
                    ResID = int.Parse(xmlNode.Attribute("ID").Value),
                    Name = xmlNode.Attribute("Name").Value,
                    BundleDir = xmlNode.Attribute("BundleDir").Value,
                    PosX = short.Parse(xmlNode.Attribute("PosX").Value),
                    PosY = short.Parse(xmlNode.Attribute("PosY").Value),
                    ExplodePosX = short.Parse(xmlNode.Attribute("ExplodePosX").Value),
                    ExplodePosY = short.Parse(xmlNode.Attribute("ExplodePosY").Value),
                    HasFlyAction = int.Parse(xmlNode.Attribute("HasFlyAction").Value) == 1,
                    HasFadeOutAction = int.Parse(xmlNode.Attribute("HasFadeOutAction").Value) == 1,
                    HasExplodeAction = int.Parse(xmlNode.Attribute("HasExplodeAction").Value) == 1,
                    FlyAnimDuration = short.Parse(xmlNode.Attribute("FlyAnimDuration").Value),
                    FadeOutAnimDuration = short.Parse(xmlNode.Attribute("FadeOutAnimDuration").Value),
                    ExplodeAnimDuration = short.Parse(xmlNode.Attribute("ExplodeAnimDuration").Value),
                    Use8Dir = int.Parse(xmlNode.Attribute("Use8Dir").Value) == 1,
                    Use16Dir = int.Parse(xmlNode.Attribute("Use16Dir").Value) == 1,
                    Use32Dir = int.Parse(xmlNode.Attribute("Use32Dir").Value) == 1,
                    AutoRotate = int.Parse(xmlNode.Attribute("AutoRotate").Value) == 1,
                    UseTrailEffect = int.Parse(xmlNode.Attribute("UseTrailEffect").Value) == 1,
                    TrailDuration = int.Parse(xmlNode.Attribute("TrailDuration").Value),
                    TrailPeriod = int.Parse(xmlNode.Attribute("TrailPeriod").Value),
                };
            }
        }

        /// <summary>
        /// Danh sách hiệu ứng theo ID Res
        /// </summary>
        public Dictionary<int, BulletResData> ResDatas { get; set; }

        /// <summary>
        /// Chuyển đối tượng từ XML Node
        /// </summary>
        /// <param name="xmlNode"></param>
        /// <returns></returns>
        public static BulletActionSetXML Parse(XElement xmlNode)
        {
            BulletActionSetXML actionSetXML = new BulletActionSetXML()
            {
                SoundBundleDir = xmlNode.Element("Sound").Attribute("BundleDir").Value,
                ResDatas = new Dictionary<int, BulletResData>(),
            };

            foreach (XElement node in xmlNode.Elements("Bullet"))
            {
                BulletResData resData = BulletResData.Parse(node);
                actionSetXML.ResDatas[resData.ResID] = resData;
            }

            return actionSetXML;
        }
    }
}
