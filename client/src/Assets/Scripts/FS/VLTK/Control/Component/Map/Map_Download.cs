using FS.GameEngine.Logic;
using FS.GameFramework.Logic;
using FS.VLTK.Entities;
using FS.VLTK.Loader;
using FS.VLTK.Utilities;
using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace FS.VLTK.Control.Component
{
    /// <summary>
    /// Đối tượng bản đồ
    /// </summary>
    public partial class Map
    {
        /// <summary>
        /// Tổng số lượt đã thử tải xuống bản đồ
        /// </summary>
        private int totalTriedDownloadTimes;

        /// <summary>
        /// Số lượt thử lại tải xuống tối đa
        /// </summary>
        private const int MaxTriedDownloadTimes = 5;

        /// <summary>
        /// Thực hiện tải xuống dữ liệu tài nguyên bản đồ
        /// </summary>
        /// <param name="mapData"></param>
        /// <param name="done"></param>
        /// <returns></returns>
        private IEnumerator StartDownload(Entities.Config.Map mapData, Action done)
        {
            /// Thông báo đang tải xuống cái gì
            this.UpdateProgressText?.Invoke(string.Format("Đang tải xuống: {0}", mapData.Name));

            /// Tên Res bản đồ cần tải
            string mapResFolderName = Regex.Match(mapData.ImageFolder, @"Resources\/Map\/(\w+)\/Image")?.Groups[1]?.Value;

            /// Nếu bản đồ hiện tại đang được tải
            if (Global.Data.CurrentDownloadingMapName == mapResFolderName)
            {
                /// Chừng nào chưa tải xong thì đợi
                while (Global.Data.CurrentDownloadingMapName == mapResFolderName)
                {
                    /// Cập nhật Text đang tải
                    this.ReportProgress?.Invoke(Global.Data.CurrentDownloadingMapProgress);
                    /// Bỏ qua Frame
                    yield return null;
                }

                /// Sau khi đã tải xong thì thoát
                done?.Invoke();
                yield break;
            }

            /// Đánh dấu bản đồ đang tải
            Global.Data.CurrentDownloadingMapName = mapResFolderName;

            /// Tìm thông tin bản đồ trong File tải tài nguyên tương ứng
            UpdateZipFile updateFile = MainGame.UpdateFiles.ZipFiles.Where(x => x.FileName == string.Format("Data/Resources/Map/{0}.zip", mapResFolderName)).FirstOrDefault();
            /// Nếu không tồn tại thì báo lỗi và thoát Game
            if (updateFile == null)
            {
                KTGlobal.ShowMessageBox("Lỗi nghiêm trọng", string.Format("Thông tin bản đồ ID '{0}' không tìm thấy, hãy thoát game và liên hệ với hỗ trợ để được xử lý!", mapData.ID), () => {
                    Application.Quit();
                }, false);
                yield break;
            }

            TRY_DOWNLOADING:

            /// Tăng tổng số lượt đã thử
            this.totalTriedDownloadTimes++;

            /// Cập nhật Text đang tải
            this.ReportProgress?.Invoke(0);
            /// Thực hiện tải File
            KTDownloadManager.Downloader downloader = KTDownloadManager.Create();
            downloader.URL = string.Format("{0}{1}/Zip/{2}", MainGame.GameInfo.CdnUrl, Global.GetDeviceForWebURL(), updateFile.FileName);
            /// Sự kiện báo cáo tiến độ
            downloader.PreportProgress = (percent) => {
                /// Cập nhật Text đang tải
                this.ReportProgress?.Invoke((int) percent);
                /// Ghi lại tiến độ đang tải
                Global.Data.CurrentDownloadingMapProgress = (int) percent;
            };

            downloader.OutputFileDir = string.Format("Temp/{0}", updateFile.FileName);
            /// Gửi yêu cầu tải
            KTDownloadManager.Instance.Run(downloader);

            /// Chừng nào chưa tải xong
            while (!downloader.Completed)
            {
                /// Bỏ qua Frame
                yield return null;
            }

            /// Nếu có lỗi
            if (downloader.ResponseCode == 404)
            {
                Super.ShowMessageBox("Lỗi tải tài nguyên", string.Format("Có lỗi khi tải xuống File {0}, liên hệ với Admin hoặc hõ trợ để được giúp đỡ.", updateFile.FileName), () =>
                {
                    /// Thoát Game
                    Application.Quit();
                });
                yield break;
            }

            /// Nếu có lỗi
            if (downloader.HasError)
            {
                /// Nếu đã quá số lần thử
                if (this.totalTriedDownloadTimes > Map.MaxTriedDownloadTimes)
                {
                    /// Cập nhật Text trạng thái giải nén bị lỗi
                    Super.ShowMessageBox("Lỗi tải tài nguyên", string.Format("Có lỗi khi tải xuống File {0}, đã thử {1} lần nhưng không có tác dụng. Hãy liên hệ với Admin hoặc hỗ trợ để được trợ giúp!", updateFile.FileName, Map.MaxTriedDownloadTimes), () => {
                        /// Thoát
                        Application.Quit();
                    });
                    yield break;
                }
                /// Cập nhật Text trạng thái giải nén bị lỗi
                Super.ShowMessageBox("Lỗi tải tài nguyên", string.Format("Có lỗi khi tải xuống File {0}, đang thử lại {1}/{2} lần.", updateFile.FileName, this.totalTriedDownloadTimes, Map.MaxTriedDownloadTimes));
                /// Đợi 2s
                yield return new WaitForSeconds(2f);
                /// Ẩn thông báo
                Super.HideMessageBox();
                /// Tải lại
                goto TRY_DOWNLOADING;
            }

            /// Cập nhật Text đang tải
            this.ReportProgress?.Invoke(100);

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
                /// Nếu đã quá số lần thử
                if (this.totalTriedDownloadTimes >= Map.MaxTriedDownloadTimes)
                {
                    /// Cập nhật Text trạng thái giải nén bị lỗi
                    Super.ShowMessageBox("Lỗi giải nén", string.Format("Giải nén {0} bị lỗi. Đã thử tải lại {1} lượt nhưng không có tác dụng. Hãy liên hệ với hỗ trợ để được xử lý.", updateFile.FileName, Map.MaxTriedDownloadTimes), () => {
                        /// Thoát
                        Application.Quit();
                    });
                    yield break;
                }
                /// Cập nhật Text trạng thái giải nén bị lỗi
                Super.ShowMessageBox("Lỗi giải nén", string.Format("Giải nén {0} bị lỗi. Đang thử tải lại, số lần thử: {1}/{2}.", updateFile.FileName, this.totalTriedDownloadTimes, Map.MaxTriedDownloadTimes));
                
                try
                {
                    /// Xóa File Temp
                    File.Delete(string.Format("{0}/{1}", Application.persistentDataPath, downloader.OutputFileDir));
                }
                catch (Exception) { }
                
                /// Đợi 2s
                yield return new WaitForSeconds(2f);
                /// Ẩn MessageBox
                Super.HideMessageBox();
                /// Thử lại
                goto TRY_DOWNLOADING;
            }

            /// Thông tin trong File chờ Update sau
            UpdateZipFile updateLaterInfo = MainGame.ListUpdateLaterFiles.ZipFiles.Where(x => x.FileName == string.Format("Data/Resources/Map/{0}.zip", mapResFolderName)).FirstOrDefault();
            /// Toác cái đéo gì đó
            if (updateLaterInfo != null)
            {
                /// Cập nhật lại File UpdateList
                updateFile.MD5 = updateLaterInfo.MD5;
                /// Xóa khỏi danh sách chờ tải sau
                MainGame.ListUpdateLaterFiles.ZipFiles.Remove(updateLaterInfo);
                /// Ghi lại File UpdateList
                MainGame.ExportUpdateList();
            }

            /// Đánh dấu bản đồ đang tải
            Global.Data.CurrentDownloadingMapName = "";
            Global.Data.CurrentDownloadingMapProgress = 0;

            /// Dọn rác
            yield return Resources.UnloadUnusedAssets();

            /// Thực hiện callback khi tải hoàn tất
            done?.Invoke();
        }
    }
}
