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
    /// Kết quả sau khi tải File Version.xml
    /// </summary>
    public enum LoadVersionResult
    {
        /// <summary>
        /// Vào trò chơi
        /// </summary>
        PlayGame,
        /// <summary>
        /// Yêu cầu tải lại App mới nhất ở trên chợ
        /// </summary>
        RequireReDownloadApp,
        /// <summary>
        /// Yêu cầu tải tài nguyên
        /// </summary>
        RequireDownloadResource,
    }

    /// <summary>
    /// Tải File Version.XML
    /// </summary>
    public class LoadVersion : TTMonoBehaviour
    {
        #region Define
        /// <summary>
        /// Tên File Version
        /// </summary>
        public const string VersionFile = "Version.xml";
        #endregion

        #region Private fields
        /// <summary>
        /// Thông tin file Version.XML hiện có
        /// </summary>
        private GameInfo localGameInfo = null;

        /// <summary>
        /// Thông tin file Version.XML hiện có trong StreamingAsset
        /// </summary>
        private GameInfo streamingAssetGameInfo = null;

        /// <summary>
        /// Thông tin file Version.XML trên Server
        /// </summary>
        private GameInfo serverGameInfo = null;
        #endregion

        #region Properties
        /// <summary>
        /// Sự kiện tải xuống hoàn tất
        /// </summary>
        public Action<GameInfo, LoadVersionResult> Done { get; set; }

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
            GameObject.DontDestroyOnLoad(this.gameObject);
        }

        /// <summary>
        /// Hàm này gọi ở Frame đầu tiên
        /// </summary>
        private void Start()
        {
            this.StartCoroutine(this.ReadData());
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Thực hiện đọc dữ liệu
        /// </summary>
        /// <returns></returns>
        private IEnumerator ReadData()
        {
            /// Đọc dữ liệu từ File Version trong StreamingAsset
            yield return this.ReadStreamingAssetVersion();
            /// Đọc file Version.XML hiện có
            yield return this.ReadLocalVersionLocal();
            /// Nếu không có thông tin
            if (this.localGameInfo == null)
            {
                yield break;
            }
            /// Đọc file Version.XML trên Server
            yield return this.ReadServerVersion();
            /// Nếu không có thông tin
            if (this.serverGameInfo == null)
            {
                yield break;
            }
            /// Thực hiện so sánh phiên bản
            this.CompareVersion();
            /// Hủy đối tượng
            this.Destroy();
        }

        /// <summary>
        /// Đọc File Version.XML ở StreamingAsset
        /// </summary>
        /// <returns></returns>
        private IEnumerator ReadStreamingAssetVersion()
        {
            /// Đường dẫn
            string fullPath = PathUtils.SteamingAssetsPath(LoadVersion.VersionFile);

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
                /// Xóa yêu càu
                request.downloadHandler.Dispose();
                request.Dispose();
                /// Chuyển thành chuỗi XML tương ứng
                string strText = new UTF8Encoding().GetString(byteData);
                /// Loại ký tự thừa ở đầu
                string _byteOrderMarkUtf8 = Encoding.UTF8.GetString(Encoding.UTF8.GetPreamble());
                if (strText.StartsWith(_byteOrderMarkUtf8))
                {
                    strText = strText.Replace(_byteOrderMarkUtf8, "");
                }
                /// Đọc dữ liệu
                this.streamingAssetGameInfo = GameInfo.Parse(XElement.Parse(strText));
            }
            catch (Exception ex)
            {

            }
        }

        /// <summary>
        /// Đọc File Version.XML hiện có
        /// </summary>
        /// <returns></returns>
        private IEnumerator ReadLocalVersionLocal()
        {
            /// Đường dẫn
            string fullPath = Global.WebPath(LoadVersion.VersionFile);

            /// Nội dung File
            UnityWebRequest request = new UnityWebRequest(fullPath);
            request.downloadHandler = new DownloadHandlerBuffer();
            /// Gửi yêu cầu
            yield return request.SendWebRequest();

            /// Nếu có lỗi gì đó
            if (!string.IsNullOrEmpty(request.error))
            {
                this.Faild?.Invoke(request.error);
                yield break;
            }

            try
            {
                /// Chuỗi Byte kết quả
                byte[] byteData = request.downloadHandler.data;
                /// Xóa yêu càu
                request.downloadHandler.Dispose();
                request.Dispose();
                /// Chuyển thành chuỗi XML tương ứng
                string strText = new UTF8Encoding().GetString(byteData);
                /// Loại ký tự thừa ở đầu
                string _byteOrderMarkUtf8 = Encoding.UTF8.GetString(Encoding.UTF8.GetPreamble());
                if (strText.StartsWith(_byteOrderMarkUtf8))
                {
                    strText = strText.Replace(_byteOrderMarkUtf8, "");
                }
                /// Đọc dữ liệu
                GameInfo gameInfo = GameInfo.Parse(XElement.Parse(strText));

                /// Nếu phiên bản hiện tại nhỏ hơn phiên bản trong StreamingAsset
                if (int.Parse(this.streamingAssetGameInfo.Version) > int.Parse(gameInfo.Version))
                {
                    /// Gắn lại phiên bản hiện tại thành phiên bản trong StreamingAsset
                    this.localGameInfo = this.streamingAssetGameInfo;
                }
                else
                {
                    /// Lấy phiên bản hiện tại mới đọc được
                    this.localGameInfo = gameInfo;
                }
            }
            catch (Exception ex)
            {
                //this.Faild?.Invoke(ex.ToString());
                /// Toạch thì lấy file ở trong StreamingAsset
                this.localGameInfo = this.streamingAssetGameInfo;
            }
        }

        /// <summary>
        /// Đọc dữ liệu Version.XML ở Server
        /// </summary>
        /// <returns></returns>
        private IEnumerator ReadServerVersion()
        {
            /// Đường dẫn
            string fullPath = string.Format("{0}{1}/{2}", this.localGameInfo.URL, Global.GetDeviceForWebURL(), LoadVersion.VersionFile);
            /// Yêu cầu tải
            UnityWebRequest request = new UnityWebRequest(fullPath);
            request.downloadHandler = new DownloadHandlerBuffer();
            /// Gửi yêu cầu
            yield return request.SendWebRequest();

            /// Nếu có lỗi gì đó
            if (!string.IsNullOrEmpty(request.error))
            {
                this.Faild?.Invoke(request.error);
                yield break;
            }

            try
            {
                /// Chuỗi Byte kết quả
                byte[] byteData = request.downloadHandler.data;
                /// Xóa yêu càu
                request.downloadHandler.Dispose();
                request.Dispose();
                /// TODO-Giải mã
                string strText = new UTF8Encoding().GetString(byteData);
                /// Loại ký tự thừa ở đầu
                string _byteOrderMarkUtf8 = Encoding.UTF8.GetString(Encoding.UTF8.GetPreamble());
                if (strText.StartsWith(_byteOrderMarkUtf8))
                {
                    strText = strText.Replace(_byteOrderMarkUtf8, "");
                }
                /// Đọc dữ liệu
                this.serverGameInfo = GameInfo.Parse(XElement.Parse(strText));
            }
            catch (Exception ex)
            {
                this.Faild?.Invoke(ex.ToString());
            }
        }
        
        /// <summary>
        /// Hủy đối tượng
        /// </summary>
        private void Destroy()
        {
            GameObject.Destroy(this.gameObject);
        }

        /// <summary>
        /// Thực hiện so sánh 2 phiên bản của Client và Server để đưa ra kết luận có cần Update không
        /// </summary>
        private void CompareVersion()
        {
            /// Nếu phiên bản ở Client thấp hơn ở Server
            if (int.Parse(this.localGameInfo.Version) < int.Parse(this.serverGameInfo.Version))
            {
                this.Done?.Invoke(this.serverGameInfo, LoadVersionResult.RequireReDownloadApp);
            }
            /// Nếu tài nguyên khác nhau
            else if (int.Parse(this.localGameInfo.ResourceVersion) < int.Parse(this.serverGameInfo.ResourceVersion))
            {
                this.Done?.Invoke(this.serverGameInfo, LoadVersionResult.RequireDownloadResource);
            }
            /// Nếu khớp với Server thì vào Game
            else
            {
                this.Done?.Invoke(this.serverGameInfo, LoadVersionResult.PlayGame);
            }
        }
#endregion
    }
}
