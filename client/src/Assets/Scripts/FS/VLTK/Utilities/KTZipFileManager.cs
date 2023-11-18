using FS.GameFramework.Logic;
using FS.VLTK.Utilities;
using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FS.VLTK.Utilities
{
    /// <summary>
    /// Quản lý Zip file
    /// </summary>
    public static class KTZipFileManager
    {
        /// <summary>
        /// Nén đường dẫn
        /// </summary>
        /// <param name="Root"></param>
        /// <param name="Folder"></param>
        /// <param name="Path"></param>
        /// <param name="MD5"></param>
        /// <param name="FileLeng"></param>
        public static void CompressDirectory(string Root, string Folder, string Path, out string MD5, out long FileLeng)
        {
            ZipOutputStream zip = new ZipOutputStream(File.Create(Path));

            zip.SetLevel(9);

            string folder = Folder;

            KTZipFileManager.ZipFolder(Root, folder, zip);
            zip.Finish();
            zip.Close();

            MD5 = KTResourceCrypto.CalculateMD5(Path);
            FileLeng = new System.IO.FileInfo(Path).Length;
        }

        /// <summary>
        /// Nén Folder
        /// </summary>
        /// <param name="RootFolder"></param>
        /// <param name="CurrentFolder"></param>
        /// <param name="zStream"></param>
        public static void ZipFolder(string RootFolder, string CurrentFolder, ZipOutputStream zStream)
        {
            RootFolder = RootFolder + "\\";
            string[] SubFolders = Directory.GetDirectories(CurrentFolder);

            foreach (string Folder in SubFolders)
                ZipFolder(RootFolder, Folder, zStream);

            string relativePath = CurrentFolder.Replace(RootFolder, "") + @"\";

            if (relativePath.Length > 1)
            {
                ZipEntry dirEntry;

                dirEntry = new ZipEntry(relativePath);
                dirEntry.DateTime = DateTime.Now;
            }

            foreach (string file in Directory.GetFiles(CurrentFolder, "*.unity3d"))
            {
                AddFileToZip(zStream, relativePath, file);
            }
        }

        /// <summary>
        /// Thêm File nén
        /// </summary>
        /// <param name="zStream"></param>
        /// <param name="relativePath"></param>
        /// <param name="file"></param>
        private static void AddFileToZip(ZipOutputStream zStream, string relativePath, string file)
        {
            byte[] buffer = new byte[4096];
            string fileRelativePath = (relativePath.Length > 1 ? relativePath : string.Empty) + Path.GetFileName(file);
            ZipEntry entry = new ZipEntry(fileRelativePath);

            entry.DateTime = DateTime.Now;
            zStream.PutNextEntry(entry);

            using (FileStream fs = File.OpenRead(file))
            {
                int sourceBytes;

                do
                {
                    sourceBytes = fs.Read(buffer, 0, buffer.Length);
                    zStream.Write(buffer, 0, sourceBytes);
                } while (sourceBytes > 0);
            }
        }

        /// <summary>
        /// Giải nén File
        /// </summary>
        /// <param name="targetFile"></param>
        /// <param name="fileDir"></param>
        /// <returns></returns>
        public static bool UnZipFile(string targetFile, string fileDir)
        {
            ZipInputStream s = null;
            FileStream fs = null;
            try
            {
                fs = File.OpenRead(targetFile.Trim());
                s = new ZipInputStream(fs);
                ZipEntry theEntry;
                string path = fileDir;

                string rootDir = " ";

                while ((theEntry = s.GetNextEntry()) != null)
                {
                    string entryName = theEntry.Name.Replace("\\", Path.DirectorySeparatorChar.ToString());

                    rootDir = Path.GetDirectoryName(entryName);
                    rootDir = rootDir.Replace("\\", Path.DirectorySeparatorChar.ToString());

                    string fileName = Path.GetFileName(entryName);

                    path = fileDir + Path.DirectorySeparatorChar.ToString() + rootDir;

                    if (!Directory.Exists(fileDir + Path.DirectorySeparatorChar.ToString() + rootDir))
                    {
                        Directory.CreateDirectory(path);
                    }

                    if (!string.IsNullOrEmpty(fileName))
                    {
                        FileStream streamWriter = File.Create(path + Path.DirectorySeparatorChar.ToString() + fileName);

                        int size = 2048;
                        byte[] data = new byte[2048];
                        while (true)
                        {
                            size = s.Read(data, 0, data.Length);
                            if (size > 0)
                            {
                                streamWriter.Write(data, 0, size);
                            }
                            else
                            {
                                break;
                            }
                        }

                        streamWriter.Close();
                    }
                }
                fs.Close();
                s.Close();

                return true;
            }
            catch (Exception ex)
            {
                fs?.Close();
                s?.Close();
                return false;
            }
            
        }

        /// <summary>
        /// Luồng Async thực hiện giải nén File Zip
        /// </summary>
        /// <param name="targetFile"></param>
        /// <param name=""></param>
        /// <returns></returns>
        public static IEnumerator UnZipFileAsync(string targetFile, string fileDir)
        {
            /// Xóa khoảng trống thừa
            targetFile = targetFile.Trim();

            /// Nếu File không tồn tại
            if (!File.Exists(targetFile))
            {
                yield break;
            }

            ZipInputStream s = new ZipInputStream(File.OpenRead(targetFile.Trim()));
            ZipEntry theEntry;
            string path = fileDir;

            string rootDir = " ";

            /// Duyệt liên tục
            while ((theEntry = s.GetNextEntry()) != null)
            {
                string entryName = theEntry.Name.Replace("\\", Path.DirectorySeparatorChar.ToString());

                rootDir = Path.GetDirectoryName(entryName);
                rootDir = rootDir.Replace("\\", Path.DirectorySeparatorChar.ToString());

                string fileName = Path.GetFileName(entryName);

                path = fileDir + Path.DirectorySeparatorChar.ToString() + rootDir;

                if (!Directory.Exists(fileDir + Path.DirectorySeparatorChar.ToString() + rootDir))
                {
                    Directory.CreateDirectory(path);
                }

                if (!string.IsNullOrEmpty(fileName))
                {
                    FileStream streamWriter = File.Create(path + Path.DirectorySeparatorChar.ToString() + fileName);

                    int totalResolvedBytesThisFrame = 0;
                    int size = 2048;
                    byte[] data = new byte[2048];
                    while (true)
                    {
                        size = s.Read(data, 0, data.Length);
                        if (size > 0)
                        {
                            streamWriter.Write(data, 0, size);
                            /// Tăng tổng số Bytes đã xử lý lên
                            totalResolvedBytesThisFrame += size;
                            /// Nếu tổng số Bytes đã xử lý trong Frame này vượt quá 1 MB
                            if (totalResolvedBytesThisFrame >= 1048576)
                            {
                                /// Bỏ qua Frame
                                yield return null;
                            }
                        }
                        else
                        {
                            break;
                        }
                    }

                    streamWriter.Close();
                }
                /// Bỏ qua Frame
                yield return null;
            }
            s.Close();
        }
    }
}
