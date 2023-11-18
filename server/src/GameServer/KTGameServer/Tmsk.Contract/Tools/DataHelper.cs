using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ProtoBuf;
using ComponentAce.Compression.Libs.zlib;
using System.Diagnostics;
using Server.Tools;
using GameServer.Core.Executor;
using Tmsk.Tools;
using Tmsk.Contract;
using System.Net.Sockets;

namespace Server.Tools
{
    /// <summary>
    /// 数据操作辅助
    /// </summary>
    public class DataHelper2
    {
        // SortBytes的key
        private static byte SortKey = 0;

        // SortBytes的key64
        private static ulong SortKey64 = 0;

        /// <summary>
        /// 压缩的大小
        /// </summary>
        public static int MinZipBytesSize = 1024000;

        /// <summary>
        /// 当前工作路径
        /// </summary>
        public static string CurrentDirectory;

        static DataHelper2()
        {
            CurrentDirectory = Directory.GetCurrentDirectory() + "\\";

            byte[] keyBytes = BitConverter.GetBytes((int)1695843216);
            for (int i = 0; i < keyBytes.Length; i++)
            {
                SortKey += keyBytes[i];
            }
			
            ulong _SortKey = SortKey;
            SortKey64 |= _SortKey;
            SortKey64 |= (ulong)(_SortKey << 8);
            SortKey64 |= (ulong)(_SortKey << 16);
            SortKey64 |= (ulong)(_SortKey << 24);
            SortKey64 |= (ulong)(_SortKey << 32);
            SortKey64 |= (ulong)(_SortKey << 40);
            SortKey64 |= (ulong)(_SortKey << 48);
            SortKey64 |= (ulong)(_SortKey << 56);
        }
		
        /// <summary>
        /// 字节数据拷贝
        /// </summary>
        /// <param name="copyTo">目标字节数组</param>
        /// <param name="offsetTo">目标字节数组的拷贝偏移量</param>
        /// <param name="copyFrom">源字节数组</param>
        /// <param name="offsetFrom">源字节数组的拷贝偏移量</param>
        /// <param name="count">拷贝的字节个数</param>
        public static void CopyBytes(byte[] copyTo, int offsetTo, byte[] copyFrom, int offsetFrom, int count)
        {
            /*for (int i = 0; i < count; i++)
            {
                copyTo[offsetTo + i] = copyFrom[offsetFrom + i];
            }*/
            Array.Copy(copyFrom, offsetFrom, copyTo, offsetTo, count);
        }

        /// <summary>
        /// 字节数据排序
        /// </summary>
        /// <param name="copyTo"></param>
        /// <param name="offsetTo"></param>
        /// <param name="count"></param>
        public static void SortBytes(byte[] bytesData, int offsetTo, int count)
        {
//             if( count <= 32 )
//             {
                int tc = offsetTo + count;
                for( int x = offsetTo; x < tc; x++ )
                {
                    bytesData[x] ^= SortKey;
                }
//             }
//             else
//             {
//                 int t = count / 8;
//                 unsafe
//                 {
//                     fixed( byte* p = &bytesData[offsetTo] )
//                     {
//                         ulong* pl = (ulong*)p;
//                         for( int n = 0; n < t; n++ )
//                         {
//                             pl[n] ^= SortKey64;
//                         }
//                     }
//                 }
// 
//                 int tc = offsetTo + count;
//                 for( int x = offsetTo + t*8; x < tc; x++ )
//                 {
//                     bytesData[x] ^= SortKey;
//                 }
//             }
        }

        /// <summary>
        /// 比较两个字节数组是否相同
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool CompBytes(byte[] left, byte[] right)
        {
            if (left.Length != right.Length)
            {
                return false;
            }

            bool ret = true;
            for (int i = 0; i < left.Length; i++)
            {
                if (left[i] != right[i])
                {
                    ret = false;
                    break;
                }
            }

            return ret;
        }

        /// <summary>
        /// 比较两个字节数组是否相同
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool CompBytes(byte[] left, byte[] right, int count)
        {
            if (left.Length < count || right.Length < count)
            {
                return false;
            }

            bool ret = true;
            for (int i = 0; i < count; i++)
            {
                if (left[i] != right[i])
                {
                    ret = false;
                    break;
                }
            }

            return ret;
        }

        /// <summary>
        /// 产生并填充随机数
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        public static void RandBytes(byte[] buffer, int offset, int count)
        {
            long tick = TimeUtil.NOW() * 10000;
            Random rnd = new Random((int)(tick & 0xffffffffL) | (int)(tick >> 32));
            for (int i = 0; i < count; i++)
            {
                buffer[offset + i] = (byte)rnd.Next(0, 0xFF);
            }
        }

        /// <summary>
        /// 将字节流转换为Hex编码的字符串(无分隔符号)
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        public static string Bytes2HexString(byte[] b)
        {
            int ch = 0;
            string ret = "";
            for (int i = 0; i < b.Length; i++)
            {
                ch = (b[i] & 0xFF);
                ret += ch.ToString("X2").ToUpper();
            }

            return ret;
        }

        /// <summary>
        /// 将Hex编码的字符串转换为字节流(无分隔符号)
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static byte[] HexString2Bytes(string s)
        {
            if (s.Length % 2 != 0) //非法的字符串
            {
                return null;
            }

            int b = 0;
            string hexstr = "";
            byte[] bytesData = new byte[s.Length / 2];
            for (int i = 0; i < s.Length / 2; i++)
            {
                hexstr = s.Substring(i * 2, 2);
                b = Int32.Parse(hexstr, System.Globalization.NumberStyles.HexNumber) & 0xFF;
                bytesData[i] = (byte)b;
            }

            return bytesData;
        }

        /// <summary>
        /// 格式化异常错误信息[控制台版这个函数理论上是线程安全的]
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public static void WriteFormatExceptionLog(Exception e, string extMsg, bool showMsgBox, bool finalReport = false)
        {
            try
            {
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.AppendFormat("应用程序出现了异常[{0}]:\r\n{1}\r\n", finalReport ? 1 : 0, e.Message);

                stringBuilder.AppendFormat("\r\n 额外信息: {0}", extMsg);
                if (null != e)
                {
                    if (e.InnerException != null)
                    {
                        stringBuilder.AppendFormat("\r\n {0}", e.InnerException.Message);
                    }
                    stringBuilder.AppendFormat("\r\n {0}", e.StackTrace);
                }

                //记录异常日志文件
                LogManager.WriteException(stringBuilder.ToString());
                if (showMsgBox)
                {
                    //弹出异常日志窗口
                    SysConOut.WriteLine(stringBuilder.ToString());
                }
            }
            catch (Exception)
            {
            }
        }

        public static void WriteExceptionLogEx(Exception ex, string extMsg)
        {
            try
            {
                System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(2, true);
                string logStr = string.Format("{0}\r\n{1}\r\n{2}", extMsg, ex.ToString(), stackTrace.ToString());
                LogManager.WriteException(logStr.ToString());
            }
            catch (Exception)
            {
            }
        }

        public static void WriteStackTraceLog(string extMsg)
        {
            try
            {
                System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(1, true);

                //记录异常日志文件
                string logStr = string.Format("{0}\r\n{1}", extMsg, stackTrace.ToString());
                LogManager.WriteException(logStr);
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// 格式化堆栈信息
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public static void WriteFormatStackLog(System.Diagnostics.StackTrace stackTrace, string extMsg)
        {
            try
            {
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.AppendFormat("应   用程序出现了对象锁定超时错误:\r\n");

                stringBuilder.AppendFormat("\r\n 额外信息: {0}", extMsg);
                stringBuilder.AppendFormat("\r\n {0}", stackTrace.ToString());

                //记录异常日志文件
                LogManager.WriteException(stringBuilder.ToString());
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// 判断如果不是 "*", 则转为指定的值, 否则默认值
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static Int32 ConvertToInt32(string str, Int32 defVal)
        {
            try
            {
                if ("*" != str)
                {
                    return Convert.ToInt32(str);
                }

                return defVal;
            }
            catch (Exception)
            {
            }

            return defVal;
        }

        /// <summary>
        /// 判断如果不是 "*", 则转为指定的值, 否则默认值
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string ConvertToStr(string str, string defVal)
        {
            if ("*" != str)
            {
                return str;
            }

            return defVal;
        }

        /// <summary>
        /// 将日期时间字符串转为整数表示
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static long ConvertToTicks(string str, long defVal)
        {
            if ("*" == str)
            {
                return defVal;
            }

            str = str.Replace('$', ':');

            try
            {
                DateTime dt;
                if (!DateTime.TryParse(str, out dt))
                {
                    return 0L;
                }

                return dt.Ticks / 10000;
            }
            catch (Exception)
            {
            }

            return 0L;
        }

        /// <summary>
        /// 将日期时间字符串转为整数表示
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static long ConvertToTicks(string str)
        {
            try
            {
                DateTime dt;
            if (!DateTime.TryParse(str, out dt))
            {
                return 0L;
            }

            return dt.Ticks / 10000;
            }
            catch (Exception)
            {
            }

            return 0L;
        }

        /// <summary>
        /// Unix秒的起始计算毫秒时间(相对系统时间)
        /// </summary>
        private static long UnixStartTicks = DataHelper2.ConvertToTicks("1970-01-01 08:00");

        /// <summary>
        /// 将Unix秒表示的时间转换为系统毫秒表示的时间
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static long UnixSecondsToTicks(int secs)
        {
            return UnixStartTicks + ((long)secs * 1000);
        }

        /// <summary>
        /// 将Unix秒表示的时间转换为系统毫秒表示的时间
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static long UnixSecondsToTicks(string secs)
        {
            int intSecs = Convert.ToInt32(secs);
            return UnixSecondsToTicks(intSecs);
        }

        /// <summary>
        /// 获取Unix秒表示的当前时间
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static int UnixSecondsNow()
        {
            long ticks = TimeUtil.NOW();
            return SysTicksToUnixSeconds(ticks);
        }

        /// <summary>
        /// 将系统毫秒表示的时间转换为Unix秒表示的时间
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static int SysTicksToUnixSeconds(long ticks)
        {
            long secs = (ticks - UnixStartTicks) / 1000;
            return (int)secs;
        }

        /// <summary>
        /// 将对象转为字节流
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="instance"></param>
        /// <param name="pool"></param>
        /// <param name="cmdID"></param>
        /// <returns></returns>
        public static byte[] ObjectToBytes<T>(T instance)
        {
            try
            {
                byte[] bytesCmd = null;
                if (null == instance)
                {
                    bytesCmd = new byte[0];
                }
                else
                {
                        MemoryStream ms = new MemoryStream();
                        Serializer.Serialize<T>(ms, instance);
                        bytesCmd = new byte[ms.Length];
                        ms.Position = 0;
                        ms.Read(bytesCmd, 0, bytesCmd.Length);
                        ms.Dispose();
                        ms = null;
                }

                if (bytesCmd.Length > DataHelper2.MinZipBytesSize) //大于256字节的才压缩, 节省cpu占用，想一想，每秒10兆小流量的吐出，都在压缩，cpu占用当然会高, 带宽其实不是问题, 不会达到上限(100兆共享)
                {
                    //zlib压缩算法
                    byte[] newBytes = DataHelper2.Compress(bytesCmd);
                    if (null != newBytes)
                    {
                        if (newBytes.Length < bytesCmd.Length)
                        {
                            //System.Diagnostics.Debug.WriteLine(string.Format("{0}压缩率: {1}", instance.GetType(), ((double)newBytes.Length / bytesCmd.Length) * 100.0));
                            bytesCmd = newBytes;
                        }
                    }
                }

                return bytesCmd;
            }
            catch (Exception ex)
            {
                WriteExceptionLogEx(ex, "将对象转为字节流发生异常:");
            }

            return new byte[0];
        }

        public static T BytesToObject2<T>(byte[] bytesData, int offset, int length, Socket socket) where T : class, IProtoBuffData, new()
        {
            T t = new T();
            try
            {
                t.fromBytes(bytesData, offset, length);
                return t;
            }
            catch (System.Exception ex)
            {
                //LogManager.WriteExceptionUseCache(ex.ToString());
                LogManager.WriteLog(LogTypes.Data, string.Format("解析客户端发上来的数据{0}异常,IP:{1},数据内容：{2}", t.ToString(), socket.RemoteEndPoint.ToString(), Convert.ToBase64String(bytesData, offset, length)));
            }

            return default(T);
        }
		
        /// <summary>
        /// 将字节数据转为对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="bytesData"></param>
        /// <returns></returns>
        public static T BytesToObject<T>(byte[] bytesData, int offset, int length)
        {
            if (bytesData.Length == 0) return default(T);

            try
            {
                //zlib解压缩算法
                byte[] copyData = new byte[length];
                DataHelper2.CopyBytes(copyData, 0, bytesData, offset, length);
                copyData = DataHelper2.Uncompress(copyData);

                MemoryStream ms = new MemoryStream();
                ms.Write(copyData, 0, copyData.Length);
                ms.Position = 0;
                T t = Serializer.Deserialize<T>(ms);
                ms.Dispose();
                ms = null;
                return t;
            }
            catch (Exception ex)
            {
                WriteExceptionLogEx(ex, "将字节数据转为对象发生异常:");
            }

            return default(T);
        }

        /// <summary>
        /// zlib 压缩算法
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static byte[] Compress(byte[] bytes)
        {
            using (var ms = new MemoryStream())
            {
                using (ZOutputStream outZStream = new ZOutputStream(ms, zlibConst.Z_BEST_SPEED))
                {
                    outZStream.Write(bytes, 0, bytes.Length);
                    outZStream.Flush();
                }

                return ms.ToArray();
            }
        }

        /// <summary>
        /// zlib 解压缩算法
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static byte[] Uncompress(byte[] bytes)
        {
            //小于2个字节肯定是非压缩的
            if (bytes.Length < 2)
            {
                return bytes;
            }

            //判断是否是压缩数据，是才执行解开压缩操作
            if (0x78 != bytes[0])
            {
                return bytes;
            }

            if (0x9C != bytes[1] && 0xDA != bytes[1])
            {
                return bytes;
            }

            using (var ms = new MemoryStream())
            {
                using (ZOutputStream outZStream = new ZOutputStream(ms))
                {
                    outZStream.Write(bytes, 0, bytes.Length);
                    outZStream.Flush();
                }

                return ms.ToArray();
            }
        }

        /// <summary>
        /// UTF8 汉字字节流转成 Unicode 汉字字节流
        /// </summary>
        /// <param name="input"></param>
        /// <see cref="http://hi.baidu.com/hyqsoft/blog/item/263795a164d1728346106464.html"/>
        public static byte[] Utf8_2_Unicode(byte[] input)
        {
            var ret = new List<byte>();
            for (var i = 0; i < input.Length; i++)
            {
                if (input[i] >= 240) // 11110xxx
                {
                    //i += 3;
                    //throw new Exception("四字节的 UTF- 8 字符不能转换成两字节的 Unicode 字符！");
                    return null;
                }
                //else if (input[i] >= 224)
                if (input[i] >= 224) // 1110xxxx
                {
                    ret.Add((byte)((input[i + 2] & 63) | ((input[i + 1] & 3) << 6)));
                    ret.Add((byte)((input[i] << 4) | ((input[i + 1] & 60) >> 2)));
                    i += 2;
                }
                else if (input[i] >= 192) // 110xxxxx
                {
                    ret.Add((byte)((input[i + 1] & 63) | ((input[i] & 3) << 6)));
                    ret.Add((byte)((input[i] & 28) >> 2));
                    i += 1;
                }
                else
                {
                    ret.Add(input[i]);
                    ret.Add(0);
                }
            }
            return ret.ToArray();
        }

        #region 字符串压缩并且做base64转换

        /// <summary>
        /// 将原来的字符串=>字节=>压缩=>base64
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string ZipStringToBase64(string text)
        {
            try
            {
                if (GameManager.FlagOptimizePathString)
                {
                    return text;
                }
                else
                {
                    if (string.IsNullOrEmpty(text))
                    {
                        return "";
                    }

                    byte[] bytes = new UTF8Encoding().GetBytes(text);

                    //zlib压缩算法
                    if (bytes.Length > 128)
                    {
                        bytes = DataHelper2.Compress(bytes);
                    }

                    return Convert.ToBase64String(bytes);
                }
            }
            catch (Exception)
            {
            }

            return "";
        }

        /// <summary>
        /// 将原来的base64=>字节=>解开压缩=>原字符串
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string UnZipStringToBase64(string base64)
        {
            try
            {
                if (GameManager.FlagOptimizePathString)
                {
                    return base64;
                }
                else
                {
                    if (string.IsNullOrEmpty(base64))
                    {
                        return "";
                    }

                    byte[] bytes = Convert.FromBase64String(base64);
                    bytes = DataHelper2.Uncompress(bytes);
                    return new UTF8Encoding().GetString(bytes, 0, bytes.Length);
                }
            }
            catch (Exception)
            {
            }

            return "";
        }

        #endregion 字符串压缩并且做base64转换
		
		#region 基准时间转换

        public static string EncodeBase64(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                str = "null";
            }

            byte[] bytes = Encoding.UTF8.GetBytes(str);
            return Convert.ToBase64String(bytes);
        }

        public static string DecodeBase64(string base64Str)
        {
            try
            {
                if (!string.IsNullOrEmpty(base64Str))
                {
                    byte[] bytes = Convert.FromBase64String(base64Str);
                    return Encoding.UTF8.GetString(bytes);
                }
            }
            catch
            {
            }

            return null;
        }

        private static DateTime StartDate = new DateTime(2011, 11, 11);

        /// <summary>
        /// 返回服务器时间相对于"2011-11-11"经过了多少天
        /// 可以避免使用DayOfYear产生的跨年问题
        /// </summary>
        /// <returns></returns>
        public static double GetOffsetSecond(DateTime date)
        {
            return (date - StartDate).TotalSeconds;
        }

        /// <summary>
        /// 返回服务器时间相对于"2011-11-11"经过了多少天
        /// 可以避免使用DayOfYear产生的跨年问题
        /// </summary>
        /// <returns></returns>
        public static int GetOffsetDay(DateTime now)
        {
            return (int)(now - StartDate).TotalDays;
        }

        /// <summary>
        /// 当前时间相对于"2011-11-11"经过了多少天
        /// </summary>
        /// <returns></returns>
        public static int GetOffsetDayNow()
        {
            return GetOffsetDay(TimeUtil.NowDateTime());
        }

        /// <summary>
        /// 使用服务器时间相对于"2011-11-11"经过了多少天 来返回具体的日期
        /// 可以避免使用DayOfYear产生的跨年问题
        /// </summary>
        /// <returns></returns>
        public static DateTime GetRealDate(int day)
        {
            return StartDate.AddDays(day);
        }
		
		#endregion 基准时间转换

        /// <summary>
        /// 交换两个值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        public static void Swap<T>(ref T lhs, ref T rhs)
        {
            T tmp = lhs;
            lhs = rhs;
            rhs = tmp;
        }
    }
}
