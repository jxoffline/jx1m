using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace FS.VLTK.Entities
{
    /// <summary>
    /// Quản lý thông tin Game
    /// </summary>
    public class GameInfo
    {
        /// <summary>
        /// URL Server
        /// </summary>
        public string URL { get; set; }

        /// <summary>
        /// URL CDN tải
        /// </summary>
        public string CdnUrl { get; set; }

        #region Game
        /// <summary>
        /// Tên trò chơi
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Phiên bản
        /// </summary>
        public string Version { get; set; }
        #endregion

        #region Resources
        /// <summary>
        /// Tên tài nguyên
        /// </summary>
        public string ResourceName { get; set; }

        /// <summary>
        /// Phiên bản tài nguyên
        /// </summary>
        public string ResourceVersion { get; set; }
        #endregion

        #region Thông tin Game trên Store
        /// <summary>
        /// Đường dẫn Game trên AppStore
        /// </summary>
        public string GameIoSURL { get; set; }

        /// <summary>
        /// Đường dẫn Game trên GooglePlayStore
        /// </summary>
        public string GameAndroidURL { get; set; }

        /// <summary>
        /// Text khi có phiên bản mới
        /// </summary>
        public string NewVersionHint { get; set; }
        #endregion

        #region Update Info
        /// <summary>
        /// Đường dẫn SDK VoiceChat-Push
        /// </summary>
        public string PushVoiceUrl { get; set; }

        /// <summary>
        /// Đường dẫn SDK VoiceChat-Get
        /// </summary>
        public string GetVoiceUrl { get; set; }

        /// <summary>
        /// Đường dẫn thông tin danh sách máy chủ
        /// </summary>
        public string ServerListURL { get; set; }

        /// <summary>
        /// Đường dẫn thông tin danh sách máy chủ dự phòng trường hợp URL chính bị lỗi
        /// </summary>
        public string ServerListBackUpURL { get; set; }

        /// <summary>
        /// Đường dẫn SDK xác nhận tài khoản
        /// </summary>
        public string VerifyAccountSDK { get; set; }

        /// <summary>
        /// Đường dẫn SDK đăng nhập
        /// </summary>
        public string LoginAccountSDK { get; set; }

        /// <summary>
        /// Đường dẫn SDK đăng ký
        /// </summary>
        public string RegisterAccountSDK { get; set; }

        /// <summary>
        /// Đường dẫn Update File
        /// </summary>
        public string FileUpdateURL { get; set; }
        #endregion

        /// <summary>
        /// Chuyển đối tượng từ XMLNode
        /// </summary>
        /// <param name="xmlNode"></param>
        /// <returns></returns>
        public static GameInfo Parse(XElement xmlNode)
        {
            return new GameInfo()
            {
                URL = xmlNode.Attribute("URL").Value,
                CdnUrl = xmlNode.Element("CdnUrl").Attribute("Url").Value,

                Name = xmlNode.Element("Application").Attribute("VerText").Value,
                Version = xmlNode.Element("Application").Attribute("VerCode").Value,

                ResourceName = xmlNode.Element("Resource").Attribute("VerText").Value,
                ResourceVersion = xmlNode.Element("Resource").Attribute("VerCode").Value,

                GameIoSURL = xmlNode.Element("Store").Attribute("AppsIosUrl").Value,
                GameAndroidURL = xmlNode.Element("Store").Attribute("AppsAndroidUrl").Value,
                NewVersionHint = xmlNode.Element("Store").Attribute("Msg").Value,

                PushVoiceUrl = xmlNode.Element("UpdateInfo").Attribute("PushVoiceUrl").Value,
                GetVoiceUrl = xmlNode.Element("UpdateInfo").Attribute("GetVoiceUrl").Value,
                ServerListURL = xmlNode.Element("UpdateInfo").Attribute("ServerListUrl").Value,
                ServerListBackUpURL = xmlNode.Element("UpdateInfo").Attribute("ServerListUrlBackUp").Value,
                VerifyAccountSDK = xmlNode.Element("UpdateInfo").Attribute("VerifyAccountSDK").Value,
                LoginAccountSDK = xmlNode.Element("UpdateInfo").Attribute("LoginAccountSDK").Value,
                RegisterAccountSDK = xmlNode.Element("UpdateInfo").Attribute("RegisterAccountSDK").Value,
                FileUpdateURL = xmlNode.Element("UpdateInfo").Attribute("FileUpdateUrl").Value,
            };
        }
        
        /// <summary>
        /// Chuyển đối tượng về dạng XElement
        /// </summary>
        /// <returns></returns>
        public XElement ToXML()
        {
            XElement xmlNode = new XElement("Config");
            xmlNode.Add(new XAttribute("URL", this.URL));

            XElement cdnNode = new XElement("CdnUrl");
            cdnNode.Add(new XAttribute("Url", this.CdnUrl));
            xmlNode.Add(cdnNode);

            XElement applicationNode = new XElement("Application");
            applicationNode.Add(new XAttribute("VerText", this.Name));
            applicationNode.Add(new XAttribute("VerCode", this.Version));
            xmlNode.Add(applicationNode);

            XElement resourceNode = new XElement("Resource");
            resourceNode.Add(new XAttribute("VerText", this.ResourceName));
            resourceNode.Add(new XAttribute("VerCode", this.ResourceVersion));
            xmlNode.Add(resourceNode);

            XElement storeNode = new XElement("Store");
            storeNode.Add(new XAttribute("AppsIosUrl", this.GameIoSURL));
            storeNode.Add(new XAttribute("AppsAndroidUrl", this.GameAndroidURL));
            storeNode.Add(new XAttribute("Msg", this.NewVersionHint));
            xmlNode.Add(storeNode);

            XElement updateInfoNode = new XElement("UpdateInfo");
            updateInfoNode.Add(new XAttribute("PushVoiceUrl", this.PushVoiceUrl));
            updateInfoNode.Add(new XAttribute("GetVoiceUrl", this.GetVoiceUrl));
            updateInfoNode.Add(new XAttribute("ServerListUrl", this.ServerListURL));
            updateInfoNode.Add(new XAttribute("ServerListUrlBackUp", this.ServerListBackUpURL));
            updateInfoNode.Add(new XAttribute("VerifyAccountSDK", this.VerifyAccountSDK));
            updateInfoNode.Add(new XAttribute("LoginAccountSDK", this.LoginAccountSDK));
            updateInfoNode.Add(new XAttribute("RegisterAccountSDK", this.RegisterAccountSDK));
            updateInfoNode.Add(new XAttribute("FileUpdateUrl", this.FileUpdateURL));
            xmlNode.Add(updateInfoNode);

            return xmlNode;
        }

        /// <summary>
        /// Chuyển đối tượng về dạng XMLString
        /// </summary>
        /// <returns></returns>
        public string ToXMLString()
        {
            return this.ToXML().ToString();
        }

        /// <summary>
        /// Chuyển đối tượng về dạng String
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("{0} - Version {1}", this.Name, this.Version);
        }
    }
}
