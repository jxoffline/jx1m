using FS.GameEngine.Logic;
using FS.GameFramework.Logic;
using FS.VLTK.Entities;
using FS.VLTK.Entities.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace FS.VLTK.Loader
{
    /// <summary>
    /// Kiểm tra tài nguyên
    /// </summary>
    public static class KTResourceChecker
    {
        /// <summary>
        /// Kiểm tra tài nguyên bản đồ tương ứng có tồn tại không
        /// </summary>
        /// <param name="mapCode"></param>
        /// <returns></returns>
        public static bool IsMapResExist(int mapCode)
        {
            /// Thông tin bản đồ tương ứng
            if (Loader.Maps.TryGetValue(mapCode, out Map mapData))
            {
                string mapResFolderName = Regex.Match(mapData.ImageFolder, @"Resources\/Map\/(\w+)\/Image")?.Groups[1]?.Value;
                /// Đường dẫn đến Folder chứa tài nguyên tương ứng
                string resFolder = Global.WebPath(string.Format("Data/Resources/Map/{0}", mapResFolderName), true);
                /// Trả về kết quả
                return Utils.IsFolderExist(resFolder);
            }
            /// Không tìm thấy Res
            return true;
        }

        /// <summary>
        /// Trả về danh sách bản đồ thiếu Res
        /// </summary>
        /// <returns></returns>
        public static List<UpdateZipFile> GetListMissingMapResources()
        {
            /// Tạo mới danh sách
            List<UpdateZipFile> files = new List<UpdateZipFile>();

            /// Duyệt danh sách các File trong phần tải
            foreach (UpdateZipFile file in MainGame.UpdateFiles.ZipFiles)
            {
                /// Nếu đây là bản đồ
                if (file.IsMap)
                {
                    /// ID bản đồ
                    int mapID = file.Flag;
                    /// Nếu bản đồ không tồn tại
                    if (!KTResourceChecker.IsMapResExist(mapID))
                    {
                        /// Thêm File vào danh sách cần tải
                        files.Add(file);
                    }
                }
            }

            /// Trả về kết quả
            return files;
        }
    }
}
