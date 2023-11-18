using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;

namespace GameServer.KiemThe
{
    /// <summary>
    /// Chứa các hàm hỗ trợ
    /// </summary>
    public static class Utils
    {
        public static string UrlEncode(string str)
        {
            if (str == null)
            {
                return null;
            }
            return HttpUtility.UrlEncode(str, Encoding.UTF8);
        }

        /// <summary>
        /// Gửi sang telegram
        /// </summary>
        /// <param name="NOIDUNG"></param>
        /// <returns></returns>
        public static bool SendTelegram(string NOIDUNG)
        {
            Thread _SendTeleGram = new Thread(() =>
            {
                try
                {
                    string URL = "https://api.telegram.org/bot6429774070:AAEgdzdw8gpJIvC1Nx6h7kypw79ocCJVQ58/sendMessage?chat_id=2123123889&text=" + UrlEncode(NOIDUNG);

                    ServicePointManager.Expect100Continue = true;
                    //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls
                    //       | SecurityProtocolType.Tls11
                    //       | SecurityProtocolType.Tls12
                    //       | SecurityProtocolType.Ssl3;

                    WebClient wc = new WebClient();

                    var result = wc.DownloadString(URL);


                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());

                }
            });
            _SendTeleGram.IsBackground = false;
            _SendTeleGram.Start();

            return true;
        }

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
        /// <param name="allowWhiteSpaces"></param>
        /// <returns></returns>
        public static bool CheckValidString(string inputString, bool allowWhiteSpaces = false)
        {
            if (!allowWhiteSpaces)
            {
                bool ret = Regex.Matches(inputString, @"[A-Za-z0-9_]+")?.Count == 1;
                ret &= !Regex.IsMatch(inputString, @"[^A-Za-z0-9_]");
                return ret;
            }
            else
            {
                bool ret = Regex.Matches(inputString, @"[A-Za-z0-9_ ]+")?.Count == 1;
                ret &= !Regex.IsMatch(inputString, @"[^A-Za-z0-9_]");
                return ret;
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
            using (DeflateStream dstream = new DeflateStream(output, CompressionLevel.Optimal))
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

        #region Enumerations

        /// <summary>
        /// Chuyển đối tượng Enum từ String
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="str"></param>
        /// <param name="enumV"></param>
        /// <returns></returns>
        public static bool TryParseEnum<T>(string str, out T enumV) where T : struct, IConvertible
        {
            enumV = default;

            Type enumType = typeof(T);
            if (!enumType.IsEnum)
            {
                return false;
            }

            List<T> values = Enum.GetValues(typeof(T)).Cast<T>().ToList();
            foreach (T value in values)
            {
                if (value.ToString() == str)
                {
                    enumV = value;
                    return true;
                }
            }
            return false;
        }

        #endregion Enumerations

        #region Linq Extends

        /// <summary>
        /// Clone the list into another one
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static List<TSource> Clone<TSource>(this List<TSource> source)
        {
            /// Null
            if (source == null)
            {
                return null;
            }

            List<TSource> result = new List<TSource>();
            lock (source)
            {
                foreach (TSource item in source)
                {
                    result.Add(item);
                }
            }
            return result;
        }

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

        /// <summary>
        /// Randomly selects items from a sequence.
        /// </summary>
        /// <typeparam name="T">The type of the items in the sequence.</typeparam>
        /// <param name="sequence">The sequence from which to randomly select items.</param>
        /// <param name="count">The number of items to randomly select from the sequence.</param>
        /// <returns>A sequence of randomly selected items.</returns>
        /// <remarks>This is an O(N) algorithm (N is the sequence length).</remarks>
        public static IEnumerable<T> RandomRange<T>(this IEnumerable<T> sequence, int count)
        {
            if (sequence == null)
            {
                throw new ArgumentNullException("sequence");
            }

            /// The number of items in the sequence among which to randomly select
            int sequenceLength = sequence.Count();
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

                if (rng.NextDouble() < remaining / (double)available)
                {
                    yield return iterator.Current;
                    --remaining;
                }

                --available;
            }
        }
        #endregion Linq Extends

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

        #endregion Bound array
    }
}