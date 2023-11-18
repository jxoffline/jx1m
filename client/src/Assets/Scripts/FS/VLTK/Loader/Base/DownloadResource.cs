using FS.GameEngine.Logic;
using FS.VLTK.Entities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace FS.VLTK.Loader
{
    /// <summary>
    /// Tải xuống tài nguyên của Game
    /// </summary>
    public class DownloadResource : TTMonoBehaviour
    {
        #region Private fields
        /// <summary>
        /// Thông tin các file đã tải hiện tại
        /// </summary>
        private UpdateFiles localUpdateList = null;

        /// <summary>
        /// Thông tin các file cần tải ở Server
        /// </summary>
        private UpdateFiles serverUpdateList = null;
        #endregion

        #region Properties
        /// <summary>
        /// Sự kiện tải xuống hoàn tất
        /// </summary>
        public Action<UpdateFiles, UpdateFiles> Done { get; set; }

        /// <summary>
        /// Sự kiện tải xuống thất bại
        /// </summary>
        public Action<string> Faild { get; set; }
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi khi đối tượng được khởi tạo
        /// </summary>
        private void Awake()
        {
            GameObject.DontDestroyOnLoad(this);
        }

        /// <summary>
        /// Hàm này gọi đến khi đối tượng được kích hoạt
        /// </summary>
        private void Start()
        {
            this.StartCoroutine(this.ReadData());
        }
        #endregion

        #region Core
        /// <summary>
        /// Thực thi đọc dữ liệu file UpdateList.xml
        /// </summary>
        private IEnumerator ReadData()
        {
            /// Đọc dữ liệu hiện tại
            yield return this.ReadLocalUpdateListFile();
            /// Đọc dữ liệu ở Server
            yield return this.ReadServerUpdateListFile();
            /// Nếu không có ListUpdate ở Server
            if (this.serverUpdateList == null)
            {
                yield break;
            }
            /// Thực hiện so sánh phiên bản
            this.CompareListUpdate();
            /// Hủy đối tượng
            this.Destroy();
        }

        /// <summary>
        /// Đọc dữ liệu file UpdateList.xml hiện có
        /// </summary>
        /// <returns></returns>
        private IEnumerator ReadLocalUpdateListFile()
        {
            /// Đường dẫn
            string fullPath = Global.WebPath(MainGame.GameInfo.FileUpdateURL);
            /*
            /// Nếu không có File
            if (!Utils.IsFileExist(fullPath))
            {
                /// Lấy đường dẫn nằm nội bộ trong App
                fullPath = PathUtils.SteamingAssetsPath_DontUseThis(MainGame.GameInfo.FileUpdateURL);
                /// Nếu vẫn không tồn tại File
                if (!Utils.IsFileExist(fullPath))
                {
                    yield break;
                }
            }
            */

            /// Nội dung File
            UnityWebRequest request = new UnityWebRequest(fullPath);
            request.downloadHandler = new DownloadHandlerBuffer();
            /// Gửi yêu cầu
            yield return request.SendWebRequest();

            /// Nếu có lỗi gì đó
            if (!string.IsNullOrEmpty(request.error))
            {
                yield break;
            }

            try
            {
                /// Chuỗi Byte kết quả
                byte[] byteData = request.downloadHandler.data;
                /// Chuyển thành chuỗi XML tương ứng
                string strText = new UTF8Encoding().GetString(byteData);
                /// Loại ký tự thừa ở đầu
                string _byteOrderMarkUtf8 = Encoding.UTF8.GetString(Encoding.UTF8.GetPreamble());
                if (strText.StartsWith(_byteOrderMarkUtf8))
                {
                    strText = strText.Replace(_byteOrderMarkUtf8, "");
                }
                /// Đọc dữ liệu
                this.localUpdateList = UpdateFiles.Parse(XElement.Parse(strText));
            }
            catch (Exception ex)
            {
                this.Faild?.Invoke(ex.ToString());
            }
        }

        /// <summary>
        /// Tải dữ liệu file UpdateList.xml ở Server
        /// </summary>
        /// <returns></returns>
        private IEnumerator ReadServerUpdateListFile()
        {
            /// Đường dẫn
            string fullPath = string.Format("{0}{1}/{2}", MainGame.GameInfo.URL, Global.GetDeviceForWebURL(), MainGame.GameInfo.FileUpdateURL);
            /// Yêu cầu tải
            UnityWebRequest request = new UnityWebRequest(fullPath);
            request.downloadHandler = new DownloadHandlerBuffer();
            /// Gửi yêu cầu
            yield return request.SendWebRequest();

            /// Nếu có lỗi gì đó
            if (!string.IsNullOrEmpty(request.error))
            {
                yield break;
            }

            try
            {
                /// Chuỗi Byte kết quả
                byte[] byteData = request.downloadHandler.data;
                /// TODO-Giải mã
                string strText = new UTF8Encoding().GetString(byteData);
                /// Loại ký tự thừa ở đầu
                string _byteOrderMarkUtf8 = Encoding.UTF8.GetString(Encoding.UTF8.GetPreamble());
                if (strText.StartsWith(_byteOrderMarkUtf8))
                {
                    strText = strText.Replace(_byteOrderMarkUtf8, "");
                }
                /// Đọc dữ liệu
                this.serverUpdateList = UpdateFiles.Parse(XElement.Parse(strText));
            }
            catch (Exception) { }
        }

        /// <summary>
        /// Hủy đối tượng
        /// </summary>
        private void Destroy()
        {
            GameObject.Destroy(this.gameObject);
        }

        /// <summary>
        /// Thực hiện so sánh danh sách Update
        /// </summary>
        private void CompareListUpdate()
        {
            /// Danh sách các File cần Update
            UpdateFiles toUpdateFiles = new UpdateFiles()
            {
                Files = new List<UpdateFile>(),
                ZipFiles = new List<UpdateZipFile>(),
            };
            /// Nếu danh sách File hiện có NULL thì tạo mới
            if (this.localUpdateList == null)
            {
                this.localUpdateList = new UpdateFiles()
                {
                    Files = new List<UpdateFile>(),
                    ZipFiles = new List<UpdateZipFile>(),
                };
            }

            /// Duyệt danh sách File trên Server
            foreach (UpdateFile updateFile in this.serverUpdateList.Files)
            {
                /// File hiện có ở Client
                UpdateFile localFile = this.localUpdateList.Files.Where(x => x.ID == updateFile.ID).FirstOrDefault();
                /// Nếu tồn tại
                if (localFile != null)
                {
                    /// Nếu mã MD5 khác nhau
                    if (localFile.MD5 != updateFile.MD5)
                    {
                        /// Thêm vào danh sách cần Update
                        toUpdateFiles.Files.Add(updateFile);
                    }
                }
                /// Nếu không tồn tại
                else
                {
                    /// Thêm vào danh sách cần Update
                    toUpdateFiles.Files.Add(updateFile);
                }
            }

            /// Duyệt danh sách ZipFile trên Server
            foreach (UpdateZipFile updateFile in this.serverUpdateList.ZipFiles)
            {
                /// File hiện có ở Client
                UpdateZipFile localFile = this.localUpdateList.ZipFiles.Where(x => x.ID == updateFile.ID).FirstOrDefault();
                /// Nếu tồn tại
                if (localFile != null)
                {
                    /// Nếu mã MD5 khác nhau
                    if (localFile.MD5 != updateFile.MD5)
                    {
                        /// Thêm vào danh sách cần Update
                        toUpdateFiles.ZipFiles.Add(updateFile);
                    }
                }
                /// Nếu không tồn tại
                else
                {
                    /// Thêm vào danh sách cần Update
                    toUpdateFiles.ZipFiles.Add(updateFile);
                    /// Thêm luôn vào danh sách cục bộ
                    this.localUpdateList.ZipFiles.Add(new UpdateZipFile()
                    {
                        FileName = updateFile.FileName,
                        MD5 = "PENDING",
                        FileSize = updateFile.FileSize,
                        Flag = updateFile.Flag,
                        ID = updateFile.ID,
                        IsFirstDownload = updateFile.IsFirstDownload,
                        IsMap = updateFile.IsMap,
                    });
                }
            }

            /// Thực thi sự kiện hoàn tất
            this.Done?.Invoke(this.localUpdateList, toUpdateFiles);
        }
#endregion
    }
}
