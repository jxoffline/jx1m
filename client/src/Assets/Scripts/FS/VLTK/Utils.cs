using FS.GameEngine.Logic;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace FS.VLTK
{
    /// <summary>
    /// Đối tượng chứa các thư viện cần dùng
    /// </summary>
    public static class Utils
    {
        /// <summary>
        /// Hàng đợi ưu tiên
        /// </summary>
        /// <typeparam name="S"></typeparam>
        public class PriorityQueue<S>
        {
            /// <summary>
            /// Đối tượng nút trong Queue
            /// </summary>
            private class Node
            {
                /// <summary>
                /// Tên
                /// </summary>
                public string Name { get; set; }

                /// <summary>
                /// Giá trị của nút
                /// </summary>
                public S Value { get; set; }

                /// <summary>
                /// Nút liền trước
                /// </summary>
                public Node Previous { get; set; }

                /// <summary>
                /// Nút liền sau
                /// </summary>
                public Node Next { get; set; }
            }

            /// <summary>
            /// Từ điển chứa danh sách các nút
            /// </summary>
            private readonly ConcurrentDictionary<string, Node> listNode = new ConcurrentDictionary<string, Node>();

            /// <summary>
            /// Nút ở trên cùng
            /// </summary>
            private Node topNode;

            /// <summary>
            /// Nút ở dưới cùng
            /// </summary>
            private Node bottomNode;

            /// <summary>
            /// Kích thước Queue
            /// </summary>
            public int Count { get; private set; } = 0;

            /// <summary>
            /// Kiểm tra nút có tên tương ứng có tồn tại không
            /// </summary>
            /// <param name="name"></param>
            /// <returns></returns>
            public bool Contains(string name)
            {
                return this.listNode.TryGetValue(name, out Node _);
            }

            /// <summary>
            /// Trả về giá trị nút có tên tương ứng
            /// </summary>
            /// <param name="name"></param>
            /// <returns></returns>
            public S Get(string name)
            {
                if (this.listNode.TryGetValue(name, out Node _))
                {
                    return this.listNode[name].Value;
                }
                else
                {
                    return default;
                }
            }

            /// <summary>
            /// Xóa nút có tên tương ứng khỏi Queue
            /// </summary>
            /// <param name="name"></param>
            /// <returns></returns>
            public bool Remove(string name)
            {
                if (!this.listNode.TryGetValue(name, out Node _))
                {
                    return false;
                }
                Node node = this.listNode[name];
                this.listNode.TryRemove(name, out Node _);
                this.Count--;

                if (node == this.topNode || node == this.bottomNode)
                {
                    if (node == this.topNode && node == this.bottomNode)
                    {
                        this.topNode = null;
                        this.bottomNode = null;
                    }
                    else if (node == this.topNode)
                    {
                        this.topNode = this.topNode.Previous;
                        this.topNode.Next = null;
                    }
                    else if (node == this.bottomNode)
                    {
                        this.bottomNode = this.bottomNode.Next;
                        this.bottomNode.Previous = null;
                    }
                }
                else
                {
                    Node previous = node.Previous;
                    Node next = node.Next;
                    if (previous != null)
                    {
                        previous.Next = next;
                        if (next != null)
                        {
                            next.Previous = previous;
                        }
                    }
                    else if (next != null)
                    {
                        next.Previous = null;
                    }
                    else
                    {
                        return false;
                    }
                }

                return true;
            }

            /// <summary>
            /// Thêm nút vào Queue
            /// </summary>
            /// <param name="name"></param>
            /// <param name="value"></param>
            /// <returns></returns>
            public void SetOrPut(string name, S value)
            {
                if (this.listNode.TryGetValue(name, out Node _))
                {
                    this.listNode[name].Value = value;
                }
                else
                {
                    this.listNode[name] = new Node()
                    {
                        Name = name,
                        Value = value,
                        Previous = null,
                        Next = null,
                    };
                    this.Count++;

                    if (this.topNode == null)
                    {
                        this.topNode = this.listNode[name];
                    }
                    else
                    {
                        this.topNode.Next = this.listNode[name];
                        this.listNode[name].Previous = this.topNode;
                        this.topNode = this.listNode[name];
                    }
                    if (this.bottomNode == null)
                    {
                        this.bottomNode = this.listNode[name];
                    }
                }
            }

            /// <summary>
            /// Lấy giá trị ở đáy hàng đợi
            /// </summary>
            /// <returns></returns>
            public S Peek()
            {
                if (this.bottomNode == null)
                {
                    return default;
                }
                else
                {
                    return this.bottomNode.Value;
                }
            }

            /// <summary>
            /// Lấy giá trị ở đáy hàng đợi và xóa khỏi danh sách
            /// </summary>
            /// <returns></returns>
            public S Pop()
            {
                if (this.bottomNode == null)
                {
                    return default;
                }
                else
                {
                    Node result = this.bottomNode;
                    this.listNode.TryRemove(this.bottomNode.Name, out Node _);
                    this.Count--;
                    if (this.bottomNode == this.topNode)
                    {
                        this.bottomNode = null;
                        this.topNode = null;
                    }
                    else
                    {
                        Node next = this.bottomNode.Next;
                        if (next != null)
                        {
                            this.bottomNode = next;
                        }
                    }
                    return result.Value;
                }
            }

            /// <summary>
            /// Dịch chuyển nút lên đỉnh hàng đợi
            /// </summary>
            /// <param name="name"></param>
            /// <returns></returns>
            public bool MoveTop(string name)
            {
                if (!this.listNode.TryGetValue(name, out Node _))
                {
                    return false;
                }
                else
                {
                    Node node = this.listNode[name];
                    bool result = this.Remove(name);
                    this.SetOrPut(name, node.Value);
                    return result;
                }
            }

            /// <summary>
            /// Chuyển về danh sách
            /// </summary>
            /// <returns></returns>
            public List<S> ToList()
            {
                List<S> list = new List<S>();

                if (this.Count > 0)
                {
                    Node node = this.bottomNode;
                    while (node != null)
                    {
                        list.Add(node.Value);
                        node = node.Next;
                    }
                }

                return list;
            }
        }

        #region Byte Array compression
        /// <summary>
        /// Mã hóa chuỗi Byte
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static byte[] Compress(this byte[] data)
        {
            MemoryStream output = new MemoryStream();
            using (DeflateStream dstream = new DeflateStream(output, System.IO.Compression.CompressionLevel.Optimal))
            {
                dstream.Write(data, 0, data.Length);
            }
            return output.ToArray();
        }

        /// <summary>
        /// Giải mã chuỗi Byte
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static byte[] Decompress(this byte[] data)
        {
            MemoryStream input = new MemoryStream(data);
            MemoryStream output = new MemoryStream();
            using (DeflateStream dstream = new DeflateStream(input, CompressionMode.Decompress))
            {
                dstream.CopyTo(output);
            }
            return output.ToArray();
        }
        #endregion

        #region Support Functions
        /// <summary>
        /// Kiểm tra nếu đang Click vào Unity GUI
        /// </summary>
        /// <returns></returns>
        public static bool IsClickOnGUI()
        {
            if (Application.isEditor || Application.platform == RuntimePlatform.WindowsPlayer)
            {
                return EventSystem.current.IsPointerOverGameObject();
            }
            else
            {
                foreach (Touch touch in Input.touches)
                {
                    int id = touch.fingerId;
                    if (EventSystem.current.IsPointerOverGameObject(id))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Kiểm tra nếu đang Click vào RectTransform tương ứng
        /// </summary>
        /// <param name="transform"></param>
        /// <returns></returns>
        public static bool IsClickOnGUI(RectTransform transform)
        {
            if (Application.isEditor || Application.platform == RuntimePlatform.WindowsPlayer)
            {
                Vector2 localMousePosition = transform.InverseTransformPoint(Input.mousePosition);
                if (transform.rect.Contains(localMousePosition))
                {
                    return true;
                }
            }
            else
            {
                if (Input.touchCount > 0)
                {
                    Touch touch = Input.GetTouch(0);
                    Vector2 localMousePosition = transform.InverseTransformPoint(touch.position);
                    if (transform.rect.Contains(localMousePosition))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        #endregion

        #region Linq Extends
        /// <summary>
        /// Get min element of collection by its properties
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="source"></param>
        /// <param name="selector"></param>
        /// <returns></returns>
        public static TSource MinBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> selector)
        {
            return source.MinBy(selector, null);
        }

        /// <summary>
        /// Get min element of collection by its properties
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="source"></param>
        /// <param name="selector"></param>
        /// <param name="comparer"></param>
        /// <returns></returns>
        public static TSource MinBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> selector, IComparer<TKey> comparer)
        {
            if (source == null)
            {
                return default;
            }
            if (selector == null)
            {
                return default;
            }
            comparer ??= Comparer<TKey>.Default;

            using (var sourceIterator = source.GetEnumerator())
            {
                if (!sourceIterator.MoveNext())
                {
                    return default;
                }
                var min = sourceIterator.Current;
                var minKey = selector(min);
                while (sourceIterator.MoveNext())
                {
                    var candidate = sourceIterator.Current;
                    var candidateProjected = selector(candidate);
                    if (comparer.Compare(candidateProjected, minKey) < 0)
                    {
                        min = candidate;
                        minKey = candidateProjected;
                    }
                }
                return min;
            }
        }

        /// <summary>
        /// Get max element of collection by its properties
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="source"></param>
        /// <param name="selector"></param>
        /// <returns></returns>
        public static TSource MaxBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> selector)
        {
            return source.MaxBy(selector, null);
        }

        /// <summary>
        /// Get max element of collection by its properties
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="source"></param>
        /// <param name="selector"></param>
        /// <param name="comparer"></param>
        /// <returns></returns>
        public static TSource MaxBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> selector, IComparer<TKey> comparer)
        {
            if (source == null)
            {
                return default;
            }
            if (selector == null)
            {
                return default;
            }

            if (Comparer<TKey>.Default != null)
            {
                comparer = Comparer<TKey>.Default;
            }

            using (var sourceIterator = source.GetEnumerator())
            {
                if (!sourceIterator.MoveNext())
                {
                    return default;
                }
                var max = sourceIterator.Current;
                var maxKey = selector(max);
                while (sourceIterator.MoveNext())
                {
                    var candidate = sourceIterator.Current;
                    var candidateProjected = selector(candidate);
                    if (comparer.Compare(candidateProjected, maxKey) > 0)
                    {
                        max = candidate;
                        maxKey = candidateProjected;
                    }
                }
                return max;
            }
        }

        /// <summary>Randomly selects items from a sequence.</summary>
        /// <typeparam name="T">The type of the items in the sequence.</typeparam>
        /// <param name="sequence">The sequence from which to randomly select items.</param>
        /// <param name="count">The number of items to randomly select from the sequence.</param>
        /// <param name="sequenceLength">The number of items in the sequence among which to randomly select.</param>
        /// <returns>A sequence of randomly selected items.</returns>
        /// <remarks>This is an O(N) algorithm (N is the sequence length).</remarks>
        public static IEnumerable<T> RandomRange<T>(this IEnumerable<T> sequence, int count, int sequenceLength)
        {
            if (sequence == null)
            {
                throw new ArgumentNullException("sequence");
            }

            if (count < 0 || count > sequenceLength)
            {
                throw new ArgumentOutOfRangeException("count", count, "count must be between 0 and sequenceLength");
            }

            System.Random rng = new System.Random();
            int available = sequenceLength;
            int remaining = count;
            var iterator = sequence.GetEnumerator();

            for (int current = 0; current < sequenceLength; ++current)
            {
                iterator.MoveNext();

                if (rng.NextDouble() < remaining / (double) available)
                {
                    yield return iterator.Current;
                    --remaining;
                }

                --available;
            }
        }
        #endregion

        /// <summary>
        /// Thay thế chuỗi ở vị trí tìm thấy đầu tiên
        /// </summary>
        /// <param name="text"></param>
        /// <param name="search"></param>
        /// <param name="replace"></param>
        /// <returns></returns>
        public static string StringReplaceFirst(string text, string search, string replace)
        {
            int pos = text.IndexOf(search);
            if (pos < 0)
            {
                return text;
            }
            return text.Substring(0, pos) + replace + text.Substring(pos + search.Length);
        }

        /// <summary>
        /// Cắt lấy số chữ số sau dấu chấm động
        /// </summary>
        /// <param name="value"></param>
        /// <param name="digits"></param>
        /// <returns></returns>
        public static float Truncate(this float value, int digits)
        {
            double mult = Math.Pow(10.0, digits);
            double result = Math.Truncate(mult * value) / mult;
            return (float)result;
        }

        /// <summary>
        /// Cắt lấy số chữ số sau dấu chấm động
        /// </summary>
        /// <param name="value"></param>
        /// <param name="digits"></param>
        /// <returns></returns>
        public static double Truncate(this double value, int digits)
        {
            double mult = Math.Pow(10.0, digits);
            double result = Math.Truncate(mult * value) / mult;
            return result;
        }

        /// <summary>
        /// Chuẩn hóa chuỗi cơ bản
        /// </summary>
        /// <param name="inputString"></param>
        /// <returns></returns>
        public static string BasicNormalizeString(string inputString)
        {
            inputString = inputString.Trim();
            inputString = inputString.Replace("\t", " ");
            while (inputString.IndexOf("  ") != -1)
            {
                inputString = inputString.Replace("  ", " ");
            }
            return inputString;
        }

        /// <summary>
        /// Xóa toàn bộ thẻ HTML trong chuỗi đầu vào
        /// </summary>
        /// <param name="inputString"></param>
        /// <returns></returns>
        public static string RemoveAllHTMLTags(string inputString)
        {
            return Regex.Replace(inputString, @"<[a-zA-Z\/].*?>", "");
        }

        /// <summary>
        /// Xóa toàn bộ các ký tự khoảng trống
        /// </summary>
        /// <param name="inputString"></param>
        /// <returns></returns>
        public static bool CheckValidString(this string inputString)
		{
            return Regex.IsMatch(inputString, @"[\w]");
		}

        #region Number parser
        /// <summary>
        /// Chuyển chuỗi thành dạng format tương ứng
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="str"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        private static T ParseNumber<T>(string str, T defaultValue) where T : struct
        {
            if (typeof(T) == typeof(int))
            {
                if (int.TryParse(str, out int result))
                {
                    return (T) (object) result;
                }
                else
                {
                    return defaultValue;
                }
            }
            else if (typeof(T) == typeof(uint))
            {
                if (uint.TryParse(str, out uint result))
                {
                    return (T) (object) result;
                }
                else
                {
                    return defaultValue;
                }
            }
            else if (typeof(T) == typeof(float))
            {
                if (float.TryParse(str, out float result))
                {
                    return (T) (object) result;
                }
                else
                {
                    return defaultValue;
                }
            }
            else if (typeof(T) == typeof(double))
            {
                if (double.TryParse(str, out double result))
                {
                    return (T) (object) result;
                }
                else
                {
                    return defaultValue;
                }
            }
            else if (typeof(T) == typeof(long))
            {
                if (long.TryParse(str, out long result))
                {
                    return (T) (object) result;
                }
                else
                {
                    return defaultValue;
                }
            }
            else if (typeof(T) == typeof(ulong))
            {
                if (ulong.TryParse(str, out ulong result))
                {
                    return (T) (object) result;
                }
                else
                {
                    return defaultValue;
                }
            }
            else if (typeof(T) == typeof(byte))
            {
                if (byte.TryParse(str, out byte result))
                {
                    return (T) (object) result;
                }
                else
                {
                    return defaultValue;
                }
            }
            else if (typeof(T) == typeof(short))
            {
                if (short.TryParse(str, out short result))
                {
                    return (T) (object) result;
                }
                else
                {
                    return defaultValue;
                }
            }
            else if (typeof(T) == typeof(ushort))
            {
                if (ushort.TryParse(str, out ushort result))
                {
                    return (T) (object) result;
                }
                else
                {
                    return defaultValue;
                }
            }
            else
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// Chuyển chuỗi thành int hoặc giá trị mặc định nếu không thể chuyển
        /// </summary>
        /// <param name="str"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static int ParseNumber(string str, int defaultValue)
        {
            return Utils.ParseNumber<int>(str, defaultValue);
        }

        /// <summary>
        /// Chuyển chuỗi thành uint hoặc giá trị mặc định nếu không thể chuyển
        /// </summary>
        /// <param name="str"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static uint ParseNumber(string str, uint defaultValue)
        {
            return Utils.ParseNumber<uint>(str, defaultValue);
        }

        /// <summary>
        /// Chuyển chuỗi thành short hoặc giá trị mặc định nếu không thể chuyển
        /// </summary>
        /// <param name="str"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static short ParseNumber(string str, short defaultValue)
        {
            return Utils.ParseNumber<short>(str, defaultValue);
        }

        /// <summary>
        /// Chuyển chuỗi thành ushort hoặc giá trị mặc định nếu không thể chuyển
        /// </summary>
        /// <param name="str"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static ushort ParseNumber(string str, ushort defaultValue)
        {
            return Utils.ParseNumber<ushort>(str, defaultValue);
        }

        /// <summary>
        /// Chuyển chuỗi thành byte hoặc giá trị mặc định nếu không thể chuyển
        /// </summary>
        /// <param name="str"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static byte ParseNumber(string str, byte defaultValue)
        {
            return Utils.ParseNumber<byte>(str, defaultValue);
        }

        /// <summary>
        /// Chuyển chuỗi thành long hoặc giá trị mặc định nếu không thể chuyển
        /// </summary>
        /// <param name="str"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static long ParseNumber(string str, long defaultValue)
        {
            return Utils.ParseNumber<long>(str, defaultValue);
        }

        /// <summary>
        /// Chuyển chuỗi thành ulong hoặc giá trị mặc định nếu không thể chuyển
        /// </summary>
        /// <param name="str"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static ulong ParseNumber(string str, ulong defaultValue)
        {
            return Utils.ParseNumber<ulong>(str, defaultValue);
        }

        /// <summary>
        /// Chuyển chuỗi thành float hoặc giá trị mặc định nếu không thể chuyển
        /// </summary>
        /// <param name="str"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static float ParseNumber(string str, float defaultValue)
        {
            return Utils.ParseNumber<float>(str, defaultValue);
        }

        /// <summary>
        /// Chuyển chuỗi thành double hoặc giá trị mặc định nếu không thể chuyển
        /// </summary>
        /// <param name="str"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static double ParseNumber(string str, double defaultValue)
        {
            return Utils.ParseNumber<double>(str, defaultValue);
        }
        #endregion

        #region File
        /// <summary>
        /// Kiểm tra File tương ứng có tồn tại không
        /// </summary>
        /// <param name="webPath"></param>
        /// <returns></returns>
        public static bool IsFileExist(string webPath)
        {
            /// Nếu là Editor
            webPath = webPath.Replace("file:///", "");

            /// Trả về kết quả
            return File.Exists(webPath);
        }

        /// <summary>
        /// Kiểm tra Folder tương ứng có tồn tại không
        /// </summary>
        /// <param name="webPath"></param>
        /// <returns></returns>
        public static bool IsFolderExist(string webPath)
        {
            /// Nếu là Editor
            webPath = webPath.Replace("file:///", "");

            /// Trả về kết quả
            return Directory.Exists(webPath);
        }

        /// <summary>
        /// Ghi đối tượng XElement ra File lưu trong folder StreamingAssets
        /// </summary>
        /// <param name="path"></param>
        /// <param name="root"></param>
        public static void SaveXElementToFile(string path, XElement root)
        {
            string fullPath = Path.Combine(Application.persistentDataPath, path);
            //UnityEngine.Debug.LogError("Save XElement to File => " + fullPath);
            using (StreamWriter sw = new StreamWriter(fullPath, false, Encoding.UTF8))
            {
                sw.Write(root.ToString());
            }
        }

        /// <summary>
        /// Chuyển chuỗi Byte thành File lưu trong folder StreamingAssets
        /// </summary>
        /// <param name="path"></param>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static bool SaveBytesToFile(string path, byte[] bytes)
        {
            try
            {
                string fullPath = Path.Combine(Application.persistentDataPath, path);
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                }

                int index = fullPath.LastIndexOf('/');
                if (-1 != index)
                {
                    Directory.CreateDirectory(fullPath.Substring(0, index));
                }

                using (FileStream fs = new FileStream(fullPath, FileMode.Create, FileAccess.Write))
                {
                    fs.Write(bytes, 0, bytes.Length);
                    fs.Flush();
                    fs.Close();
                    fs.Dispose();
                }

                return true;
            }
            catch (Exception ex)
            {
                KTDebug.LogException(ex);
            }

            return false;
        }
        #endregion

        #region Chuyển đổi đơn vị
        /// <summary>
        /// Chuyển Bytes về MegaBytes
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static double ConvertBytesToMegabytes(long bytes)
        {
            return (bytes / 1024f) / 1024f;
        }

        /// <summary>
        /// Chuyển Kilobytes về Megabytes
        /// </summary>
        /// <param name="kilobytes"></param>
        /// <returns></returns>
        public static double ConvertKilobytesToMegabytes(long kilobytes)
        {
            return kilobytes / 1024f;
        }

        /// <summary>
        /// Chuyển Megabytes sang Gigabytes
        /// </summary>
        /// <param name="megabytes"></param>
        /// <returns></returns>
        public static double ConvertMegabytesToGigabytes(double megabytes)
        {
            return megabytes / 1024.0;
        }

        /// <summary>
        /// Chuyển Megabytes sang Terabytes
        /// </summary>
        /// <param name="megabytes"></param>
        /// <returns></returns>
        public static double ConvertMegabytesToTerabytes(double megabytes)
        {
            return megabytes / (1024.0 * 1024.0);
        }

        /// <summary>
        /// Chuyển Gigabytes sang Megabytes
        /// </summary>
        /// <param name="gigabytes"></param>
        /// <returns></returns>
        public static double ConvertGigabytesToMegabytes(double gigabytes)
        {
            return gigabytes * 1024.0;
        }

        /// <summary>
        /// Chuyển Gigabytes sang Terabytes
        /// </summary>
        /// <param name="gigabytes"></param>
        /// <returns></returns>
        public static double ConvertGigabytesToTerabytes(double gigabytes)
        {
            return gigabytes / 1024.0;
        }

        /// <summary>
        /// Chuyển Terabytes sang Megabytes
        /// </summary>
        /// <param name="terabytes"></param>
        /// <returns></returns>
        public static double ConvertTerabytesToMegabytes(double terabytes)
        {
            return terabytes * (1024.0 * 1024.0);
        }

        /// <summary>
        /// Chuyển Terabytes sang Gigabytes
        /// </summary>
        /// <param name="terabytes"></param>
        /// <returns></returns>
        public static double ConvertTerabytesToGigabytes(double terabytes)
        {
            return terabytes * 1024.0;
        }
        #endregion

        #region Bound array
        /// <summary>
        /// Chuyển Bound array thành dãy bytes tương ứng
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        /// <returns></returns>
        public static byte[] ToBytes<T>(this T[,] array) where T : struct
        {
            var buffer = new byte[array.GetLength(0) * array.GetLength(1) * System.Runtime.InteropServices.Marshal.SizeOf(typeof(T))];
            Buffer.BlockCopy(array, 0, buffer, 0, buffer.Length);
            return buffer;
        }

        /// <summary>
        /// Chuyển dãy bytes thành Bound array tương ứng
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        /// <param name="buffer"></param>
        public static void FromBytes<T>(this T[,] array, byte[] buffer) where T : struct
        {
            var len = Math.Min(array.GetLength(0) * array.GetLength(1) * System.Runtime.InteropServices.Marshal.SizeOf(typeof(T)), buffer.Length);
            Buffer.BlockCopy(buffer, 0, array, 0, len);
        }
        #endregion
    }
}
