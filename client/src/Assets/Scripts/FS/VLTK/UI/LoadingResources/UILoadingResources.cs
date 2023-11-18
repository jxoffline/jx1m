using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using System.Collections;
using FS.VLTK.Loader;
using FS.VLTK.Entities;
using FS.GameEngine.Logic;
using FS.GameFramework.Logic;
using System.IO;
using FS.VLTK.Utilities;

namespace FS.VLTK.UI.LoadingResources
{
    /// <summary>
    /// Khung tải tài nguyên
    /// </summary>
    public class UILoadingResources : TTMonoBehaviour
    {
        #region Define
        /// <summary>
        /// Text Hint
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_Hint;

        /// <summary>
        /// Slider tiến độ tải File hiện tại
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Slider UISlider_CurrentProgress;

        /// <summary>
        /// Slider tiến độ tải tổng cộng
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Slider UISlider_TotalDownloadedProgress;

        /// <summary>
        /// Text thông tin tải
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_DownloadInfo;

        /// <summary>
        /// Text tiến độ tải File hiện tại
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_DownloadProgress;

        /// <summary>
        /// Text tiến độ tải
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_TotalDownloadedProgress;
        #endregion

        #region Constants
        /// <summary>
        /// Tổng số lượt thử tải lại tối đa
        /// </summary>
        private const int MaxTriedRedownloadTimes = 5;
        #endregion

        #region Private fields
        /// <summary>
        /// Tổng số File đã tải xuống
        /// </summary>
        private int TotalDownloadedFiles;

        /// <summary>
        /// Tổng số Bytes đã tải
        /// </summary>
        private long TotalDownloadedBytes;

        /// <summary>
        /// Thời điểm lần trước cập nhật số Bytes đã tải về
        /// </summary>
        private long LastUpdateTotalDownloadedBytesTick;
        #endregion

        #region Properties
        /// <summary>
        /// Bước tiếp theo
        /// </summary>
        public Action NextStep { get; set; }

        /// <summary>
        /// Sự kiện khi quá trình hỏng
        /// </summary>
        public Action Faild { get; set; }

        /// <summary>
        /// Danh sách các File cần tải
        /// </summary>
        public UpdateFiles ToDownloadFiles { get; set; }
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi ở Frame đầu tiên
        /// </summary>
        private void Start()
        {
            /// Đánh dấu tổng số File đã tải xuống
            this.TotalDownloadedFiles = 0;
            /// Đánh dấu tổng số Bytes đã tải
            this.TotalDownloadedBytes = 0;
            /// Đánh dấu thời điểm lần trước cập nhật số Bytes đã tải về
            this.LastUpdateTotalDownloadedBytesTick = 0;
            this.InitPrefabs();
            this.StartCoroutine(this.StartDownload());
        }
        #endregion

        #region Code UI
        /// <summary>
        /// Khởi tạo ban đầu
        /// </summary>
        private void InitPrefabs()
        {
            /// Cập nhật Text
            this.UIText_Hint.text = "Đang tải xuống tài nguyên...";
            /// Cập nhật tổng số File cần tải
            this.UpdateProgress(0, 0L);
            this.UploadDownloadInfo("Đang phân tích dữ liệu...");
            this.UpdateDownloadFileProgress();
            this.UpdateTotalDownloadedBytes();
        }
        #endregion

        #region Private fields
        /// <summary>
        /// Cập nhật tổng số Bytes đã tải
        /// </summary>
        private void UpdateTotalDownloadedBytes()
        {
            /// Nếu chưa đến 0.5s thì chưa Update
            if (KTGlobal.GetCurrentTimeMilis() - this.LastUpdateTotalDownloadedBytesTick < 500)
            {
                return;
            }

            /// Đánh dấu thời điểm lần trước cập nhật số Bytes đã tải về
            this.LastUpdateTotalDownloadedBytesTick = KTGlobal.GetCurrentTimeMilis();
            /// Tổng số Bytes cần tải
            long totalBytes = this.ToDownloadFiles.TotalBytes;
            /// Tổng số Bytes đã tải
            long downloadedBytes = this.TotalDownloadedBytes;

            /// Cập nhật Text
            this.UIText_Hint.text = string.Format("Tổng số đã tải: {0}/{1}", KTGlobal.BytesToString(downloadedBytes), KTGlobal.BytesToString(totalBytes));
        }

        /// <summary>
        /// Cập nhật Text thông tin tải
        /// </summary>
        /// <param name="text"></param>
        private void UploadDownloadInfo(string text)
        {
            this.UIText_DownloadInfo.text = text;
        }

        /// <summary>
        /// Cập nhật tiến độ tải
        /// </summary>
        /// <param name="progress"></param>
        /// <param name="speedBytes"></param>
        private void UpdateProgress(int progress, long speedBytes)
        {
            this.UISlider_CurrentProgress.value = progress;
            this.UIText_DownloadProgress.text = string.Format("Tiến độ: {0}% | Tốc độ: {1}/s", progress, KTGlobal.BytesToString(speedBytes));
        }

        /// <summary>
        /// Cập nhật thông tin số File tải
        /// </summary>
        private void UpdateDownloadFileProgress()
        {
            this.UIText_TotalDownloadedProgress.text = string.Format("Tổng số đã tải: {0}/{1}", this.TotalDownloadedFiles, this.ToDownloadFiles.Count);
            int percent = this.TotalDownloadedFiles * 100 / this.ToDownloadFiles.Count;
            this.UISlider_TotalDownloadedProgress.value = percent;
        }

        /// <summary>
        /// Bắt đầu tải
        /// </summary>
        /// <returns></returns>
        private IEnumerator StartDownload()
        {
            /// Tải File
            yield return this.StartDownloadFiles();
            /// Tải ZipFile
            yield return this.StartDownloadZipFiles();
            /// Cập nhật lại Text tải thành công
            this.UIText_Hint.text = "Tải tài nguyên thành công!";
            /// Đợi 1s
            yield return new WaitForSeconds(1f);
            /// Tải hoàn tất thì thực thi sự kiện tiếp theo
            this.NextStep?.Invoke();
            /// Hủy đối tượng
            this.Destroy();
        }

        /// <summary>
        /// Bắt đầu tải File
        /// </summary>
        /// <returns></returns>
        private IEnumerator StartDownloadFiles()
        {
            /// Tổng số lượt đã thử tải lại
            int totalTriedRedownloadTimes = 0;
            /// Duyệt danh sách các File cần tải
            foreach (UpdateFile file in this.ToDownloadFiles.Files)
            {
                /// Reset số lượt đã thử tải lại
                totalTriedRedownloadTimes = 0;
                TRY_DOWNLOADING:
                /// Tăng số lượt thử tải lại
                totalTriedRedownloadTimes++;

                /// Cập nhật Text đang tải
                this.UploadDownloadInfo(string.Format("Đang tải: {0}.", file.FileName));
                KTDownloadManager.Downloader downloader = KTDownloadManager.Create();
                downloader.URL = string.Format("{0}{1}/Zip/{2}", MainGame.GameInfo.CdnUrl, Global.GetDeviceForWebURL(), file.FileName);

                /// Thời điểm báo cáo tốc độ lần trước
                long lastReportSpeedTicks = KTGlobal.GetCurrentTimeMilis();
                /// Tốc độ lần trước
                long lastSpeed = 0L;
                /// Lượng bytes đã tải
                double lastDownloadedBytes = 0f;
                /// Lượng Bytes ban đầu trước khi tải
                long currentDownloadedBytes = this.TotalDownloadedBytes;
                /// Sự kiện báo cáo tiến độ
                downloader.PreportProgress = (percent) => {
                    /// Nếu dưới 1s thì lấy tốc độ cũ
                    if (KTGlobal.GetCurrentTimeMilis() - lastReportSpeedTicks < 1000)
                    {
                        /// Báo cáo tiến độ
                        this.UpdateProgress((int) percent, lastSpeed);
                    }
                    /// Nếu lớn hơn 1s thì tính tốc độ mới
                    else
                    {
                        /// Khoảng thời gian đã qua
                        float diffSec = (KTGlobal.GetCurrentTimeMilis() - lastReportSpeedTicks) / 1000f;
                        /// Cập nhật thời điểm báo cáo tốc độ
                        lastReportSpeedTicks = KTGlobal.GetCurrentTimeMilis();
                        /// Lượng Bytes đã tải về
                        double downloadedBytes = percent * file.FileSize / 100f;
                        /// Tốc độ tải (bytes/s)
                        long speed = (long) ((downloadedBytes - lastDownloadedBytes) / diffSec);
                        /// Cập nhật lại lượng Bytes đã tải về
                        lastDownloadedBytes = downloadedBytes;
                        /// Cập nhật tốc độ tải lần trước
                        lastSpeed = speed;
                        /// Báo cáo tiến độ
                        this.UpdateProgress((int) percent, speed);
                    }

                    {
                        /// Lượng Bytes đã tải về
                        double downloadedBytes = percent * file.FileSize / 100f;
                        /// Ghi lại tổng số Bytes đã tải
                        this.TotalDownloadedBytes = currentDownloadedBytes + (long) downloadedBytes;
                        /// Cập nhật tổng số Bytes đã tải
                        this.UpdateTotalDownloadedBytes();
                    }
                };
                downloader.OutputFileDir = string.Format("{0}", file.FileName);
                /// Gửi yêu cầu tải
                KTDownloadManager.Instance.Run(downloader);

                /// Chừng nào chưa tải xong
                while (!downloader.Completed)
                {
                    /// Bỏ qua Frame
                    yield return null;
                }

                /// Nếu là lỗi 404
                if (downloader.ResponseCode == 404)
                {
                    Super.ShowMessageBox("Lỗi tải tài nguyên", string.Format("Có lỗi khi tải xuống File {0}, hãy liên hệ với Admin hoặc hỗ trợ để được xử lý", file.FileName), () => {
                        /// Thực thi sự kiện khi quá trình hỏng
                        this.Faild?.Invoke();
                        /// Hủy đối tượng
                        this.Destroy();
                    });
                    /// Không làm gì cả
                    while (true)
                    {
                        yield return null;
                    }
                }

                /// Nếu có lỗi
                if (downloader.HasError)
                {
                    /// Nếu đã quá số lần thử tải lại
                    if (totalTriedRedownloadTimes > UILoadingResources.MaxTriedRedownloadTimes)
                    {
                        Super.ShowMessageBox("Lỗi tải tài nguyên", string.Format("Có lỗi khi tải xuống File {0}, đã thử {1} lần nhưng không có tác dụng. Hãy liên hệ với Admin hoặc hỗ trợ để được trợ giúp!", file.FileName, totalTriedRedownloadTimes), () =>
                        {
                            /// Thực thi sự kiện khi quá trình hỏng
                            this.Faild?.Invoke();
                            /// Hủy đối tượng
                            this.Destroy();
                        });
                        /// Không làm gì cả
                        while (true)
                        {
                            yield return null;
                        }
                    }
                    /// Hiện thông báo
                    Super.ShowMessageBox("Lỗi tải tài nguyên", string.Format("Có lỗi khi tải xuống File {0}, đang thử lại {1}/{2} lần.", file.FileName, totalTriedRedownloadTimes, UILoadingResources.MaxTriedRedownloadTimes));
                    /// Cập nhật lại tổng số Bytes đã tải
                    this.TotalDownloadedBytes = currentDownloadedBytes;
                    /// Đợi 2s
                    yield return new WaitForSeconds(2f);
                    /// Ẩn thông báo
                    Super.HideMessageBox();
                    /// Tải lại
                    goto TRY_DOWNLOADING;
                }

                /// Cập nhật trạng thái tải 100%
                this.UpdateProgress(100, 0);

                /// Cập nhật lại tổng số Bytes đã tải
                this.TotalDownloadedBytes = currentDownloadedBytes + file.FileSize;
                /// Cập nhật tổng số Bytes đã tải
                this.UpdateTotalDownloadedBytes();

                /// Tăng số File đã tải lên
                this.TotalDownloadedFiles++;
                /// Cập nhật thông tin số File đã tải
                this.UpdateDownloadFileProgress();

                /// File cũ
                UpdateFile oldFile = MainGame.UpdateFiles.Files.Where(x => x.ID == file.ID).FirstOrDefault();
                /// Nếu File cũ tồn tại
                if (oldFile != null)
                {
                    /// Thiết lập
                    oldFile.FileName = file.FileName;
                    oldFile.FileSize = file.FileSize;
                    oldFile.IsEncryption = file.IsEncryption;
                    oldFile.MD5 = file.MD5;
                }
                /// Nếu File cũ không tồn tại
                else
                {
                    /// Thêm File vào danh sách đã tải
                    MainGame.UpdateFiles.Files.Add(file);
                }

                /// Ghi lại File danh sách đã tải
                MainGame.ExportUpdateList();
            }
        }

        /// <summary>
        /// Bắt đầu tải ZipFile
        /// </summary>
        /// <returns></returns>
        private IEnumerator StartDownloadZipFiles()
        {
            /// Tổng số lượt đã thử tải lại
            int totalTriedRedownloadTimes = 0;
            /// Duyệt danh sách các File cần tải
            foreach (UpdateZipFile file in this.ToDownloadFiles.ZipFiles)
            {
                /// Nếu không đánh dấu tải ngay lần đầu
                if (!file.IsFirstDownload)
                {
                    /// Tạo bản sao
                    UpdateZipFile cloneFile = file.Clone();
                    /// Sửa MD5 cho tải lại sau
                    cloneFile.MD5 = "PENDING";
                    /// Thêm File vào danh sách đã tải
                    MainGame.UpdateFiles.ZipFiles.Add(cloneFile);
                    /// Ghi lại File danh sách đã tải
                    MainGame.ExportUpdateList();
                    /// Tiếp tục vòng lặp
                    continue;
                }

                /// Reset số lượt đã thử tải lại
                totalTriedRedownloadTimes = 0;
                TRY_DOWNLOADING:
                /// Tăng số lượt thử tải lại
                totalTriedRedownloadTimes++;

                /// Cập nhật Text đang tải
                this.UploadDownloadInfo(string.Format("Đang tải: {0}.", file.FileName));
                KTDownloadManager.Downloader downloader = KTDownloadManager.Create();
                downloader.URL = string.Format("{0}{1}/Zip/{2}", MainGame.GameInfo.CdnUrl, Global.GetDeviceForWebURL(), file.FileName);

                /// Thời điểm báo cáo tốc độ lần trước
                long lastReportSpeedTicks = KTGlobal.GetCurrentTimeMilis();
                /// Tốc độ lần trước
                long lastSpeed = 0;
                /// Lượng bytes đã tải
                double lastDownloadedBytes = 0f;
                /// Lượng Bytes ban đầu trước khi tải
                long currentDownloadedBytes = this.TotalDownloadedBytes;
                /// Sự kiện báo cáo tiến độ
                downloader.PreportProgress = (percent) => {
                    /// Nếu dưới 1s thì lấy tốc độ cũ
                    if (KTGlobal.GetCurrentTimeMilis() - lastReportSpeedTicks < 1000)
                    {
                        /// Báo cáo tiến độ
                        this.UpdateProgress((int) percent, lastSpeed);
                    }
                    /// Nếu lớn hơn 1s thì tính tốc độ mới
                    else
                    {
                        /// Khoảng thời gian đã qua
                        float diffSec = (KTGlobal.GetCurrentTimeMilis() - lastReportSpeedTicks) / 1000f;
                        /// Cập nhật thời điểm báo cáo tốc độ
                        lastReportSpeedTicks = KTGlobal.GetCurrentTimeMilis();
                        /// Lượng Bytes đã tải về
                        double downloadedBytes = percent * file.FileSize / 100f;
                        /// Tốc độ tải (bytes/s)
                        long speed = (long) ((downloadedBytes - lastDownloadedBytes) / diffSec);
                        /// Cập nhật lại lượng Bytes đã tải về
                        lastDownloadedBytes = downloadedBytes;
                        /// Cập nhật tốc độ tải lần trước
                        lastSpeed = speed;
                        /// Báo cáo tiến độ
                        this.UpdateProgress((int) percent, speed);
                    }

                    {
                        /// Lượng Bytes đã tải về
                        double downloadedBytes = percent * file.FileSize / 100f;
                        /// Ghi lại tổng số Bytes đã tải
                        this.TotalDownloadedBytes = currentDownloadedBytes + (long) downloadedBytes;
                        /// Cập nhật tổng số Bytes đã tải
                        this.UpdateTotalDownloadedBytes();
                    }
                };

                downloader.OutputFileDir = string.Format("Temp/{0}", file.FileName);
                /// Gửi yêu cầu tải
                KTDownloadManager.Instance.Run(downloader);

                /// Chừng nào chưa tải xong
                while (!downloader.Completed)
                {
                    /// Bỏ qua Frame
                    yield return null;
                }

                /// Nếu là lỗi 404
                if (downloader.ResponseCode == 404)
                {
                    Super.ShowMessageBox("Lỗi tải tài nguyên", string.Format("Có lỗi khi tải xuống File {0}, hãy liên hệ với Admin hoặc hỗ trợ để được xử lý", file.FileName), () => {
                        /// Thực thi sự kiện khi quá trình hỏng
                        this.Faild?.Invoke();
                        /// Hủy đối tượng
                        this.Destroy();
                    });
                    /// Không làm gì cả
                    while (true)
                    {
                        yield return null;
                    }
                }

                /// Nếu có lỗi
                if (downloader.HasError)
                {
                    /// Nếu đã quá số lần thử tải lại
                    if (totalTriedRedownloadTimes > UILoadingResources.MaxTriedRedownloadTimes)
                    {
                        Super.ShowMessageBox("Lỗi tải tài nguyên", string.Format("Có lỗi khi tải xuống File {0}, đã thử {1} lần nhưng không có tác dụng. Hãy liên hệ với Admin hoặc hỗ trợ để được trợ giúp!", file.FileName, totalTriedRedownloadTimes), () =>
                        {
                            /// Thực thi sự kiện khi quá trình hỏng
                            this.Faild?.Invoke();
                            /// Hủy đối tượng
                            this.Destroy();
                        });
                        /// Không làm gì cả
                        while (true)
                        {
                            yield return null;
                        }
                    }
                    /// Hiện thông báo
                    Super.ShowMessageBox("Lỗi tải tài nguyên", string.Format("Có lỗi khi tải xuống File {0}, đang thử lại {1}/{2} lần.", file.FileName, totalTriedRedownloadTimes, UILoadingResources.MaxTriedRedownloadTimes));
                    /// Cập nhật lại tổng số Bytes đã tải
                    this.TotalDownloadedBytes = currentDownloadedBytes;
                    /// Đợi 2s
                    yield return new WaitForSeconds(2f);
                    /// Ẩn thông báo
                    Super.HideMessageBox();
                    /// Tải lại
                    goto TRY_DOWNLOADING;
                }

                /// Cập nhật trạng thái tải 100%
                this.UpdateProgress(100, 0);

                /// Cập nhật lại tổng số Bytes đã tải
                this.TotalDownloadedBytes = currentDownloadedBytes + file.FileSize;
                /// Cập nhật tổng số Bytes đã tải
                this.UpdateTotalDownloadedBytes();

                /// Tăng số File đã tải lên
                this.TotalDownloadedFiles++;
                /// Cập nhật thông tin số File đã tải
                this.UpdateDownloadFileProgress();

                /// Cập nhật Text đang giải nén
                this.UploadDownloadInfo(string.Format("Đang giải nén {0}.", file.FileName));
                /// Bỏ qua Frame
                yield return null;

                /// Giải nén
                if (KTZipFileManager.UnZipFile(string.Format("{0}/{1}", Application.persistentDataPath, downloader.OutputFileDir), string.Format("{0}", Application.persistentDataPath)))
                {
                    /// Xóa File Temp
                    File.Delete(string.Format("{0}/{1}", Application.persistentDataPath, downloader.OutputFileDir));
                }
                /// Nếu giải nén thất bại
                else
                {
                    /// Nếu đã quá số lần thử tải lại
                    if (totalTriedRedownloadTimes > UILoadingResources.MaxTriedRedownloadTimes)
                    {
                        Super.ShowMessageBox("Lỗi tải tài nguyên", string.Format("Có lỗi giải nén File {0}, đã thử tải lại {1} lần nhưng không có tác dụng. Hãy liên hệ với Admin hoặc hỗ trợ để được trợ giúp!", file.FileName, totalTriedRedownloadTimes), () =>
                        {
                            /// Thực thi sự kiện khi quá trình hỏng
                            this.Faild?.Invoke();
                            /// Hủy đối tượng
                            this.Destroy();
                        });
                        /// Không làm gì cả
                        while (true)
                        {
                            yield return null;
                        }
                    }
                    /// Cập nhật Text trạng thái giải nén bị lỗi
                    Super.ShowMessageBox("Lỗi giải nén", string.Format("Giải nén {0} bị lỗi. Đang thử tải lại {1}/{2} lượt.", file.FileName, totalTriedRedownloadTimes, UILoadingResources.MaxTriedRedownloadTimes));
                    /// Đợi 2s
                    yield return new WaitForSeconds(2f);
                    /// Ẩn thông báo
                    Super.HideMessageBox();
                    /// Tải lại
                    goto TRY_DOWNLOADING;
                }

                /// File cũ
                UpdateZipFile oldFile = MainGame.UpdateFiles.ZipFiles.Where(x => x.ID == file.ID).FirstOrDefault();
                /// Nếu File cũ tồn tại
                if (oldFile != null)
                {
                    /// Thiết lập
                    oldFile.FileName = file.FileName;
                    oldFile.FileSize = file.FileSize;
                    oldFile.Flag = file.Flag;
                    oldFile.IsFirstDownload = file.IsFirstDownload;
                    oldFile.IsMap = file.IsMap;
                    oldFile.MD5 = file.MD5;
                }
                /// Nếu File cũ không tồn tại
                else
                {
                    /// Thêm File vào danh sách đã tải
                    MainGame.UpdateFiles.ZipFiles.Add(file);
                }

                /// Ghi lại File danh sách đã tải
                MainGame.ExportUpdateList();
            }
        }

        /// <summary>
        /// Hủy đối tượng
        /// </summary>
        private void Destroy()
        {
            GameObject.Destroy(this.gameObject);
        }
        #endregion
    }
}
