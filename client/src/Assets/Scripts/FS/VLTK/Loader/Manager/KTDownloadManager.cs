using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;

namespace FS.VLTK.Loader
{
    /// <summary>
    /// Quản lý tải dữ liệu
    /// </summary>
    public class KTDownloadManager : TTMonoBehaviour
    {
        #region Singleton - Instance
        /// <summary>
        /// Quản lý tải dữ liệu
        /// </summary>
        public static KTDownloadManager Instance { get; private set; }

        /// <summary>
        /// Hàm này gọi khi đối tượng được tạo ra
        /// </summary>
        private void Awake()
        {
            KTDownloadManager.Instance = this;
        }
        #endregion

        #region Define
        /// <summary>
        /// Đối tượng tải
        /// </summary>
        public class Downloader
        {
            /// <summary>
            /// Chạy ngầm không
            /// </summary>
            public bool IsBackground { get; set; }

            /// <summary>
            /// Đường dẫn tải
            /// </summary>
            public string URL { get; set; }

            /// <summary>
            /// Đường dẫn File kết quả
            /// <para>Đặt ở trong folder StreamingAssets</para>
            /// </summary>
            public string OutputFileDir { get; set; }

            /// <summary>
            /// Đã hoàn tất chưa
            /// </summary>
            public bool Completed { get; protected set; }

            /// <summary>
            /// Có lỗi xảy ra
            /// </summary>
            public bool HasError { get; protected set; }

            /// <summary>
            /// Mã trả về
            /// </summary>
            public int ResponseCode { get; protected set; }

            /// <summary>
            /// Sự kiện khi tải hoàn tất
            /// </summary>
            public Action Complete { get; set; }

            /// <summary>
            /// Sự kiện khi tải thất bại
            /// </summary>
            public Action<string> Faild { get; set; }

            /// <summary>
            /// Báo cáo tiến độ
            /// </summary>
            public Action<float> PreportProgress { get; set; }

            /// <summary>
            /// Đối tượng tải
            /// </summary>
            protected Downloader() { }
        }

        /// <summary>
        /// Đối tượng dùng nội bộ
        /// </summary>
        public class InnerDownloader : Downloader
        {
            /// <summary>
            /// Đã tải xuống hoàn tất chưa
            /// </summary>
            public bool IsCompleted
            {
                get
                {
                    return this.Completed;
                }
                set
                {
                    this.Completed = value;
                }
            }

            /// <summary>
            /// Có lỗi xảy ra
            /// </summary>
            public bool IsHasError
            {
                get
                {
                    return this.HasError;
                }
                set
                {
                    this.HasError = value;
                }
            }

            /// <summary>
            /// Mã trả về
            /// </summary>
            public int ResCode
            {
                get
                {
                    return this.ResponseCode;
                }
                set
                {
                    this.ResponseCode = value;
                }
            }

            /// <summary>
            /// Đối tượng dùng nội bộ
            /// </summary>
            public InnerDownloader() : base() { }
        }
        #endregion

        #region API
        /// <summary>
        /// Tạo mới yêu cầu tải
        /// </summary>
        /// <returns></returns>
        public static Downloader Create()
        {
            return new InnerDownloader();
        }

        /// <summary>
        /// Bắt đầu tải
        /// </summary>
        /// <param name="downloader"></param>
        public void Run(Downloader downloader)
        {
            InnerDownloader innerDownloader = (InnerDownloader) downloader;
            innerDownloader.IsCompleted = false;
            innerDownloader.IsHasError = false;
            this.StartCoroutine(this.StartDownload(innerDownloader));
        }
        #endregion

        #region Core
        /// <summary>
        /// Đói tượng tải tùy chọn, đoc
        /// </summary>
        public class DownloadHandlerToFile : DownloadHandlerScript
        {
            #region Define
            /// <summary>
            /// Đường dẫn File đầu ra
            /// </summary>
            public string OutputDir { get; set; }
            #endregion

            #region Private fields
            /// <summary>
            /// Đối tượng FS
            /// </summary>
            private FileStream fileStream = null;

            /// <summary>
            /// Đánh dấu đã hoàn thành chưa
            /// </summary>
            private bool success;

            /// <summary>
            /// Tổng số Bytes đã tải xuống
            /// </summary>
            private long totalDownloadedBytes = 0;

            /// <summary>
            /// Tổng số Bytes cần tải
            /// </summary>
            private long totalBytes = 0;
            #endregion

            /// <summary>
            /// Hàm khởi tạo
            /// </summary>
            /// <param name="buffer"></param>
            /// <param name="downloader"></param>
            public DownloadHandlerToFile(byte[] buffer, string outputDir) : base(buffer)
            {
                this.OutputDir = outputDir;
                this.Init();
            }

            /// <summary>
            /// Hàm này để lấy dữ liệu Bytes khi tải thành công
            /// </summary>
            /// <returns></returns>
            protected override byte[] GetData()
            {
                /// Đường dẫn File kết quả
                string fullPath = this.OutputDir;

                /// Nếu File đã tồn tại
                if (File.Exists(fullPath))
                {
                    using (FileStream fs = new FileStream(this.OutputDir, FileMode.Open, FileAccess.Read))
                    {
                        /// Chuỗi Bytes kết quả
                        byte[] bytes = new byte[fs.Length];
                        /// Tổng số Bytes cần đọc
                        int numBytesToRead = (int) fs.Length;
                        /// Tổng số Bytes đã đọc
                        int numBytesRead = 0;
                        while (numBytesToRead > 0)
                        {
                            // Read may return anything from 0 to numBytesToRead.
                            int n = fs.Read(bytes, numBytesRead, numBytesToRead);

                            // Break when the end of the file is reached.
                            if (n == 0)
                            {
                                break;
                            }

                            numBytesRead += n;
                            numBytesToRead -= n;
                        }
                        return bytes;
                    }
                }
                /// Nếu File không tồn tại
                else
                {
                    return null;
                }
            }

            /// <summary>
            /// Hàm này gọi khi có dữ liệu Bytes được đổ về
            /// </summary>
            /// <param name="byteFromServer"></param>
            /// <param name="dataLength"></param>
            /// <returns></returns>
            protected override bool ReceiveData(byte[] byteFromServer, int dataLength)
            {
                /// Nếu có lỗi gì đó
                if (byteFromServer == null || byteFromServer.Length < 1)
                {
                    return false;
                }

                /// Ghi dữ liệu hiện tại vào ổ cứng
                this.AppendFile(byteFromServer, dataLength);

                /// Trả ra kết quả
                return true;
            }

            /// <summary>
            /// Trả về % tiến độ tải
            /// </summary>
            /// <returns></returns>
            protected override float GetProgress()
            {
                /// Nếu tổng số Bytes <= 0 thì toác
                if (this.totalBytes <= 0)
                {
                    return 0f;
                }
                float percent = this.totalDownloadedBytes / (float) this.totalBytes;
                //KTDebug.LogError(percent);
                /// Trả về % tiến độ
                return percent;
            }

            /// <summary>
            /// Thực hiện chuẩn bị thư mục trước khi tải
            /// </summary>
            private void Init()
            {
                try
                {
                    /// Đường dẫn File kết quả
                    string fullPath = this.OutputDir;

                    /// Nếu File đã tồn tại
                    if (File.Exists(fullPath))
                    {
                        /// Xóa
                        File.Delete(fullPath);
                    }
                    /// Nếu đường dẫn chưa tồn tại
                    if (!Directory.Exists(Path.GetDirectoryName(fullPath)))
                    {
                        /// Tạo đường dẫn
                        Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
                    }

                    /// Mở File hiện tại để ghi
                    this.fileStream = new FileStream(fullPath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                    this.success = true;
                }
                catch (Exception ex)
                {
                    KTDebug.LogError(ex.ToString());
                    this.success = false;
                }
            }

            /// <summary>
            ///  Làm 1 cái hàm tải tới đâu lưu tới đó cho đỡ tốn RAM
            /// </summary>
            /// <param name="buffer"></param>
            /// <param name="length"></param>
            private void AppendFile(byte[] buffer, int length)
            {
                /// Nếu thành công
                if (this.success)
                {
                    try
                    {
                        /// Tăng tổng số Bytes đã tải lên
                        this.totalDownloadedBytes += length;

                        /// Ghi dữ liệu vào fiels 
                        this.fileStream.Write(buffer, 0, length);
                    }
                    catch (Exception ex)
                    {
                        KTDebug.LogError(ex.ToString());
                        /// Đánh dấu tải bị lỗi
                        this.success = false;
                    }
                }
            }

            /// <summary>
            /// Sự kiện gọi khi tải hoàn tất
            /// </summary>
            protected override void CompleteContent()
            {
                /// Đóng luồng đọc ghi File
                this.fileStream.Close();
            }

            /// <summary>
            /// Trả về thông tin trước khi tải
            /// </summary>
            /// <param name="contentLength"></param>
            protected override void ReceiveContentLengthHeader(ulong contentLength)
            {
                //KTDebug.LogError("File Size = " + contentLength);
                this.totalBytes = (long) contentLength;
            }
        }
        /// <summary>
        /// Thực hiện tải dữ liệu
        /// </summary>
        /// <param name="downloader"></param>
        /// <param name="webPath"></param>
        /// <param name="downloadOutputDir"></param>
        /// <returns></returns>
        private IEnumerator StartDownload(InnerDownloader downloader)
        {
            byte[] bytes = new byte[2000];
            /// Yêu cầu tải
            UnityWebRequest request = new UnityWebRequest(downloader.URL);
            string path = Path.Combine(Application.persistentDataPath, downloader.OutputFileDir);
            request.downloadHandler = new DownloadHandlerToFile(bytes, path);
            /// Gửi yêu cầu
            request.SendWebRequest();

            /// Chừng nào chưa tải xong
            while (!request.isDone)
            {
                /// Thực hiện báo cáo tiến độ
                downloader.PreportProgress(request.downloadProgress * 100);
                /// Đợi 0.1s
                yield return new WaitForSeconds(0.1f);
            }

            /// Nếu không tìm thấy
            if (request.responseCode != 200)
            {
                /// Thực hiện sự kiện tải lỗi
                downloader.Faild?.Invoke("Error code: " + request.responseCode);
                /// Đánh dấu có lỗi
                downloader.IsHasError = true;
            }
            /// Nếu tải xuống bị lỗi gì đó
            else if (!string.IsNullOrEmpty(request.error))
            {
                /// Thực hiện sự kiện tải lỗi
                downloader.Faild?.Invoke(request.error);
                /// Đánh dấu có lỗi
                downloader.IsHasError = true;
            }

            /// Ghi lại mã trả về
            downloader.ResCode = (int) request.responseCode;

            /// Hủy yêu cầu tải
            request.downloadHandler.Dispose();
            request.Dispose();

            /// Đánh dấu tải xuống hoàn tất
            downloader.IsCompleted = true;
            /// Thực hiện sự kiện tải xuống hoàn tất
            downloader.Complete?.Invoke();
        }
        #endregion
    }
}
