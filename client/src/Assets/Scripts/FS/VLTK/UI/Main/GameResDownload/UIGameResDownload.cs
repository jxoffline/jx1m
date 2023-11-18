using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using Server.Data;
using FS.VLTK.Entities;
using System.Collections;
using FS.VLTK.Loader;
using FS.GameEngine.Logic;
using FS.VLTK.Utilities;
using System.IO;
using FS.VLTK.Utilities.UnityThreading;
using FS.VLTK.UI.Main.GameResDownload;
using System.Text.RegularExpressions;

namespace FS.VLTK.UI.Main
{
    /// <summary>
    /// Button Icon tải
    /// </summary>
    public class UIGameResDownload : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Button
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton;

        /// <summary>
        /// Text tiến độ
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_ProgressText;

        /// <summary>
        /// Khung chi tiết tải
        /// </summary>
        [SerializeField]
        private UIGameResDownload_Box UI_DownloadBox;

        /// <summary>
        /// Danh sách File cần tải xuống
        /// </summary>
        public List<UpdateZipFile> NeedDownloadFiles { get; set; }
        #endregion

        #region Properties
        /// <summary>
        /// Sự kiện đóng khung
        /// </summary>
        public Action Close { get; set; }

        /// <summary>
        /// Sự kiện nhận quà
        /// </summary>
        public Action GetAwards { get; set; }

        /// <summary>
        /// Dữ liệu
        /// </summary>
        public BonusDownload Data { get; set; }
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi khi đối tượng được tạo ra
        /// </summary>
        private void Start()
        {
            this.InitPrefabs();
            this.Refresh();
        }
        #endregion

        #region Code UI
        /// <summary>
        /// Khởi tạo ban đầu
        /// </summary>
        private void InitPrefabs()
        {
            this.UIButton.onClick.AddListener(this.Button_Clicked);
            this.UI_DownloadBox.Close = () => {
                this.UI_DownloadBox.Visible = false;
            };
            this.UI_DownloadBox.Download = () => {
                this.StartCoroutine(this.StartDownload());
            };
            this.UI_DownloadBox.GetAwards = this.GetAwards;
        }

        /// <summary>
        /// Sự kiện khi Button được ấn
        /// </summary>
        private void Button_Clicked()
        {
            this.UI_DownloadBox.Visible = !this.UI_DownloadBox.Visible;
        }
        #endregion

        #region Private fields
        /// <summary>
        /// Bắt đầu tải
        /// </summary>
        /// <returns></returns>
        private IEnumerator StartDownload()
        {
            /// Nếu không có File nào cần tải
            if (this.NeedDownloadFiles == null || this.NeedDownloadFiles.Count <= 0)
            {
                /// Đánh dấu đã hoàn tất tải
                this.UI_DownloadBox.Completed = true;
                /// Cập nhật tiến độ
                this.UIText_ProgressText.text = "100%";
                /// Thoát luồng
                yield break;
            }
            /// Bỏ đánh dấu đã hoàn tất tải
            this.UI_DownloadBox.Completed = false;
            /// Cập nhật tiến độ
            this.UIText_ProgressText.text = "0%";

            /// Tổng số File đã tải
            int totalDownloadedFiles = 0;
            /// Tổng số Bytes đã tải
            long totalDownloadedBytes = 0;
            /// Duyệt danh sách File cần tải
            foreach (UpdateZipFile file in this.NeedDownloadFiles)
            {
                /// Code bản đồ
                string mapCode = Regex.Match(file.FileName, @"Map\/(0).zip")?.Groups[1]?.Value;
                /// Nếu bản đồ này đã tồn tại
                if (KTResourceChecker.IsMapResExist(file.Flag) || (!string.IsNullOrEmpty(mapCode) && Global.Data.CurrentDownloadingMapName == mapCode))
                {
                    /// Tăng tổng số Files đã tải
                    totalDownloadedFiles++;
                    /// Tăng tổng số Bytes đã tải
                    totalDownloadedBytes += (long) file.FileSize;
                    /// Cập nhật trạng thái tải 100%
                    this.UI_DownloadBox.UpdateTotalDownloadedFiles(totalDownloadedFiles, totalDownloadedBytes);
                    /// Cập nhật trạng thái tải
                    this.UIText_ProgressText.text = string.Format("{0}%", (int) (totalDownloadedFiles * 100 / this.NeedDownloadFiles.Count));
                    /// Bỏ qua và tiếp tục với bản đồ khác
                    continue;
                }

                /// Đánh dấu File đang được tải
                Global.Data.CurrentDownloadingMapName = mapCode;
                /// Ghi lại tiến độ đang tải
                Global.Data.CurrentDownloadingMapProgress = 0;

                /// Bỏ đánh dấu tạm thời Pause
                bool isPausing = false;

                /// Yêu cầu tải
                KTDownloadManager.Downloader downloader = KTDownloadManager.Create();
                downloader.URL = string.Format("{0}{1}/Zip/{2}", MainGame.GameInfo.CdnUrl, Global.GetDeviceForWebURL(), file.FileName);

                /// Thời điểm báo cáo tốc độ lần trước
                long lastReportSpeedTicks = KTGlobal.GetCurrentTimeMilis();
                /// Sự kiện báo cáo tiến độ
                downloader.PreportProgress = (percent) => {
                    /// Cập nhật tiến độ
                    this.UI_DownloadBox.UpdateDownloadProgress((int) percent);
                    /// Ghi lại tiến độ đang tải
                    Global.Data.CurrentDownloadingMapProgress = (int) percent;
                };

                downloader.OutputFileDir = string.Format("Temp/{0}", file.FileName);
                /// Gửi yêu cầu tải
                KTDownloadManager.Instance.Run(downloader);

                /// Chừng nào chưa tải xong
                while (!downloader.Completed)
                {
                    /// Nếu đang tạm dừng
                    if (isPausing)
                    {
                        yield return null;
                    }

                    /// Nếu có lỗi
                    if (downloader.HasError)
                    {
                        KTGlobal.ShowMessageBox("Lỗi tải tài nguyên", string.Format("Có lỗi khi tải xuống File {0}, có muốn tải lại không?", file.FileName), () => {
                            /// Tải lại
                            KTDownloadManager.Instance.Run(downloader);
                            /// Bỏ đánh dấu tạm thời Pause
                            isPausing = false;
                        }, () => {
                            this.Close?.Invoke();
                        });

                        /// Tạm dừng
                        isPausing = true;
                    }

                    /// Bỏ qua Frame
                    yield return null;
                }
                /// Tăng tổng số Files đã tải
                totalDownloadedFiles++;
                /// Tăng tổng số Bytes đã tải
                totalDownloadedBytes += (long) file.FileSize;
                /// Cập nhật trạng thái tải 100%
                this.UI_DownloadBox.UpdateTotalDownloadedFiles(totalDownloadedFiles, totalDownloadedBytes);
                /// Cập nhật trạng thái tải
                this.UIText_ProgressText.text = string.Format("{0}%", (int) (totalDownloadedFiles * 100 / this.NeedDownloadFiles.Count));
                /// Bỏ qua Frame
                yield return null;

                /// Đánh dấu đang đợi giải nén
                bool waitingZip = true;
                /// Đường dẫn đến App
                string persitancePath = Application.persistentDataPath;
                /// Tạo BackgroundWorker
                UnityBackgroundWorkerManager.UnityBackgroundWorker worker = UnityBackgroundWorkerManager.Instance.NewBackgroundWorker();
                /// Thực hiện công việc
                worker.DoWork = () => {
                    /// Giải nén
                    if (KTZipFileManager.UnZipFile(string.Format("{0}/{1}", persitancePath, downloader.OutputFileDir), string.Format("{0}", persitancePath)))
                    {
                        /// Xóa File Temp
                        File.Delete(string.Format("{0}/{1}", persitancePath, downloader.OutputFileDir));
                    }
                    /// Nếu giải nén thất bại
                    else
                    {
                        MainGame.Instance.QueueOnMainThread(() => {
                            /// Cập nhật Text trạng thái giải nén bị lỗi
                            KTGlobal.ShowMessageBox("Lỗi giải nén", string.Format("Giải nén {0} bị lỗi. Hãy thử tải lại vào lần đăng nhập tiếp theo.", file.FileName), () => {
                                this.Close?.Invoke();
                            });
                        });
                    }
                };
                /// Sự kiện khi công việc hoàn tất
                worker.RunWorkerCompleted = () => {
                    /// Hủy đánh dấu đang đợi giải nén
                    waitingZip = false;
                };
                /// Thực thi Worker
                worker.RunWorkerAsync();
                
                /// Chừng nào còn đang đợi giải nén thì chờ
                while (waitingZip)
                {
                    yield return null;
                }
            }

            /// Đánh dấu đã hoàn tất tải
            this.UI_DownloadBox.Completed = true;
            /// Cập nhật tiến độ
            this.UIText_ProgressText.text = "100%";
            /// Làm mới khung thông tin
            this.UI_DownloadBox.NeedDownloadFiles.Clear();
            this.UI_DownloadBox.Refresh();

            /// Ghi lại tiến độ đang tải
            Global.Data.CurrentDownloadingMapProgress = 100;
            /// Đánh dấu File đang được tải
            Global.Data.CurrentDownloadingMapName = "";
        }

        /// <summary>
        /// Làm mới hiển thị
        /// </summary>
        private void Refresh()
        {
            /// Nếu không có File nào cần tải
            if (this.NeedDownloadFiles == null || this.NeedDownloadFiles.Count <= 0)
            {
                /// Bỏ đánh dấu đã hoàn tất tải
                this.UI_DownloadBox.Completed = true;
                /// Cập nhật tiến độ
                this.UIText_ProgressText.text = "100%";
            }
            else
            {
                /// Đánh dấu đã hoàn tất tải
                this.UI_DownloadBox.Completed = false;
                /// Cập nhật tiến độ
                this.UIText_ProgressText.text = "0%";
            }
            this.UI_DownloadBox.Data = this.Data;
            this.UI_DownloadBox.NeedDownloadFiles = this.NeedDownloadFiles;
        }
        #endregion
    }
}
