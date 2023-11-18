using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace FS.VLTK.Entities
{
    /// <summary>
    /// Định nghĩa Update dạng File
    /// </summary>
    public class UpdateFile
    {
        /// <summary>
        /// ID
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// Kích thước File
        /// </summary>
        public int FileSize { get; set; }

        /// <summary>
        /// Tên File
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// File có mã hóa không
        /// </summary>
        public bool IsEncryption { get; set; }

        /// <summary>
        /// Mã MD5 Checksum
        /// </summary>
        public string MD5 { get; set; }

        /// <summary>
        /// Chuyển đối tượng từ XMLNode
        /// </summary>
        /// <param name="xmlNode"></param>
        /// <returns></returns>
        public static UpdateFile Parse(XElement xmlNode)
        {
            return new UpdateFile()
            {
                ID = int.Parse(xmlNode.Attribute("ID").Value),
                FileSize = int.Parse(xmlNode.Attribute("FileSize").Value),
                FileName = xmlNode.Attribute("Name").Value,
                IsEncryption = bool.Parse(xmlNode.Attribute("IsEncryption").Value),
                MD5 = xmlNode.Attribute("MD5").Value,
            };
        }

        /// <summary>
        /// Chuyển đối tượng thành XMLNode
        /// </summary>
        /// <returns></returns>
        public XElement ToXML()
        {
            XElement node = new XElement("File");
            node.Add(new XAttribute("ID", this.ID));
            node.Add(new XAttribute("FileSize", this.FileSize));
            node.Add(new XAttribute("Name", this.FileName));
            node.Add(new XAttribute("IsEncryption", this.IsEncryption));
            node.Add(new XAttribute("MD5", this.MD5));
            return node;
        }

        /// <summary>
        /// Chuyển đối tượng thành dạng chuỗi XML
        /// </summary>
        /// <returns></returns>
        public string ToXMLString()
        {
            return this.ToXML().ToString();
        }

        /// <summary>
        /// Chuyển đối tượng thành dạng String
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("ID = {0}, Name = {1}, Size = {2}", this.ID, this.FileSize, this.FileName);
        }

        /// <summary>
        /// Tạo bản sao của đối tượng
        /// </summary>
        /// <returns></returns>
        public UpdateFile Clone()
        {
            return new UpdateFile()
            {
                ID = this.ID,
                FileSize = this.FileSize,
                FileName = this.FileName,
                IsEncryption = this.IsEncryption,
                MD5 = this.MD5,
            };
        }
    }

    /// <summary>
    /// Định nghĩa Update dạng File nén
    /// </summary>
    public class UpdateZipFile
    {
        /// <summary>
        /// ID
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// Kích thước File
        /// </summary>
        public int FileSize { get; set; }

        /// <summary>
        /// Tên File
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Tham biến đi kèm
        /// </summary>
        public int Flag { get; set; }

        /// <summary>
        /// Có phải bản đồ không
        /// </summary>
        public bool IsMap { get; set; }

        /// <summary>
        /// Có phải tải ngay đầu Game
        /// </summary>
        public bool IsFirstDownload { get; set; }

        /// <summary>
        /// Mã MD5 Checksum
        /// </summary>
        public string MD5 { get; set; }

        /// <summary>
        /// Chuyển đối tượng từ XMLNode
        /// </summary>
        /// <param name="xmlNode"></param>
        /// <returns></returns>
        public static UpdateZipFile Parse(XElement xmlNode)
        {
            return new UpdateZipFile()
            {
                ID = int.Parse(xmlNode.Attribute("ID").Value),
                FileSize = int.Parse(xmlNode.Attribute("FileSize").Value),
                FileName = xmlNode.Attribute("Name").Value,
                Flag = int.Parse(xmlNode.Attribute("Flag").Value),
                IsMap = bool.Parse(xmlNode.Attribute("IsMap").Value),
                IsFirstDownload = bool.Parse(xmlNode.Attribute("IsFistDownload").Value),
                MD5 = xmlNode.Attribute("MD5").Value,
            };
        }

        /// <summary>
        /// Chuyển đối tượng thành XMLNode
        /// </summary>
        /// <returns></returns>
        public XElement ToXML()
        {
            XElement node = new XElement("Zip");
            node.Add(new XAttribute("ID", this.ID));
            node.Add(new XAttribute("FileSize", this.FileSize));
            node.Add(new XAttribute("Name", this.FileName));
            node.Add(new XAttribute("Flag", this.Flag));
            node.Add(new XAttribute("IsMap", this.IsMap));
            node.Add(new XAttribute("IsFistDownload", this.IsFirstDownload));
            node.Add(new XAttribute("MD5", this.MD5));
            return node;
        }

        /// <summary>
        /// Chuyển đối tượng thành dạng chuỗi XML
        /// </summary>
        /// <returns></returns>
        public string ToXMLString()
        {
            return this.ToXML().ToString();
        }

        /// <summary>
        /// Chuyển đối tượng thành dạng String
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("ID = {0}, Name = {1}, Size = {2}", this.ID, this.FileSize, this.FileName);
        }

        /// <summary>
        /// Tạo bản sao của đối tượng
        /// </summary>
        /// <returns></returns>
        public UpdateZipFile Clone()
        {
            return new UpdateZipFile()
            {
                ID = this.ID,
                FileSize = this.FileSize,
                FileName = this.FileName,
                Flag = this.Flag,
                IsMap = this.IsMap,
                IsFirstDownload = this.IsFirstDownload,
                MD5 = this.MD5,
            };
        }
    }

    /// <summary>
    /// Danh sách File Update
    /// </summary>
    public class UpdateFiles
    {
        /// <summary>
        /// Danh sách File
        /// </summary>
        public List<UpdateFile> Files { get; set; }

        /// <summary>
        /// Danh sách File nén
        /// </summary>
        public List<UpdateZipFile> ZipFiles { get; set; }

        private long _TotalBytes = -1;
        /// <summary>
        /// Tổng số Bytes
        /// </summary>
        public long TotalBytes
        {
            get
            {
                if (this._TotalBytes != -1)
                {
                    return this._TotalBytes;
                }

                long total = 0;
                if (this.Files != null && this.Files.Count > 0)
                {
                    foreach (UpdateFile file in this.Files)
                    {
                        total += (long) file.FileSize;
                    }
                    //total += this.Files.Sum(x => x.FileSize);
                }
                if (this.ZipFiles != null && this.ZipFiles.Count > 0)
                {
                    foreach (UpdateZipFile file in this.ZipFiles)
                    {
                        if (file.IsFirstDownload)
                        {
                            total += (long) file.FileSize;
                        }
                    }
                    //total += this.ZipFiles.Sum(x => x.FileSize);
                }

                this._TotalBytes = total;
                return total;
            }
        }

        /// <summary>
        /// Tổng số File cần tải
        /// </summary>
        public int Count
        {
            get
            {
                int total = 0;
                if (this.Files != null)
                {
                    total += this.Files.Count;
                }
                if (this.ZipFiles != null)
                {
                    total += this.ZipFiles.Where(x => x.IsFirstDownload).ToList().Count;
                }
                return total;
            }
        }

        /// <summary>
        /// Chuyển đối tượng từ XMLNode
        /// </summary>
        /// <param name="xmlNode"></param>
        /// <returns></returns>
        public static UpdateFiles Parse(XElement xmlNode)
        {
            UpdateFiles updateFiles = new UpdateFiles()
            {
                Files = new List<UpdateFile>(),
                ZipFiles = new List<UpdateZipFile>(),
            };

            foreach (XElement node in xmlNode.Elements("File"))
            {
                updateFiles.Files.Add(UpdateFile.Parse(node));
            }
            foreach (XElement node in xmlNode.Elements("Zip"))
            {
                updateFiles.ZipFiles.Add(UpdateZipFile.Parse(node));
            }

            return updateFiles;
        }

        /// <summary>
        /// Chuyển đối tượng về XMLNode
        /// </summary>
        /// <returns></returns>
        public XElement ToXML()
        {
            XElement xmlNode = new XElement("UpdateList");
            foreach (UpdateFile file in this.Files)
            {
                xmlNode.Add(file.ToXML());
            }
            foreach (UpdateZipFile file in this.ZipFiles)
            {
                xmlNode.Add(file.ToXML());
            }
            return xmlNode;
        }

        /// <summary>
        /// Chuyển đối tượng về chuỗi XML
        /// </summary>
        /// <returns></returns>
        public string ToXMLString()
        {
            return this.ToXML().ToString();
        }

        /// <summary>
        /// Chuyển đối tượng thành dạng String
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("Count = {0}, Total Bytes = {1}", this.Count, KTGlobal.BytesToString(this.TotalBytes));
        }
    }
}
