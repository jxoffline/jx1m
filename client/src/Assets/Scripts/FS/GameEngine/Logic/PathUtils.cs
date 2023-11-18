using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Collections;

namespace FS.GameEngine.Logic
{
    /// <summary>
    /// Quản lý đường dẫn
    /// </summary>
	public static class PathUtils
    {
        /// <summary>
        /// Trả về đường dẫn đến Folder chứa tài nguyên
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetPersistentPath(string path)
        {
            return Application.persistentDataPath + "/" + path;
        }

        /// <summary>
        /// Trả về đường dẫn tương ứng
        /// </summary>
        /// <param name='path'></param>
        public static string GetWWWPath(string path)
        {
            if (path.StartsWith("http://") || path.StartsWith("ftp://") || path.StartsWith("https://") || path.StartsWith("file://") || path.StartsWith("jar:file://"))
            {
                return path;
            }

            if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
            {
                return path.Insert(0, "file://");
            }


            return path;
        }

        /// <summary>
        /// Đường dẫn đến Folder StreamingAssets dạng WEBREQUEST
        /// </summary>
        /// <returns></returns>
        public static string SteamingAssetsPath(string path = "")
        {
            if (Application.isEditor || Application.platform == RuntimePlatform.WindowsPlayer)
            {
                return "file:///" + Application.dataPath + "/StreamingAssets/" + path;
            }
            else if (Application.platform == RuntimePlatform.Android)
            {
                return Application.streamingAssetsPath + "/" + path;
            }
            else
            {
                return "file:///" + Application.streamingAssetsPath + "/" + path;
            }
        }

        /// <summary>
        /// Lấy đường dẫn đến Folder StreamingAssets
        /// </summary>
        /// <returns></returns>
        public static string StreamingAssetsPath_OnDisk(string path = "")
        {
            if (Application.isEditor || Application.platform == RuntimePlatform.WindowsPlayer)
            {
                return Application.dataPath + "/StreamingAssets/" + path;
            }
            else if (Application.platform == RuntimePlatform.Android)
            {
                return Application.streamingAssetsPath + "/" + path;
            }
            else
            {
                return Application.streamingAssetsPath + "/" + path;
            }
        }

        /// <summary>
        /// Danh sách đường dẫn đã lưu, dùng để đánh dấu
        /// </summary>
        private static Dictionary<string, byte> PersistentPathDict = new Dictionary<string, byte>();

        /// <summary>
        /// Lấy đường dẫn đến URL tương ứng
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="false"></param>
        /// <returns></returns>
        public static string WebPath(string uri, bool isFolder = false)
        {
            if (PathUtils.PersistentPathDict.TryGetValue(uri, out _))
            {
                string path = PathUtils.GetPersistentPath(uri);
                path = PathUtils.GetWWWPath(path);
                return path;
            }
            else
            {
                string path = PathUtils.GetPersistentPath(uri);
                if (isFolder && Directory.Exists(path))
                {
                    PathUtils.PersistentPathDict[path] = 1;
                    path = PathUtils.GetWWWPath(path);
                    return path;
                }
                else if (!isFolder && File.Exists(path))
                {
                    PathUtils.PersistentPathDict[path] = 1;
                    path = PathUtils.GetWWWPath(path);
                    return path;
                }
            }

            return PathUtils.SteamingAssetsPath(uri);
        }
    }
}
