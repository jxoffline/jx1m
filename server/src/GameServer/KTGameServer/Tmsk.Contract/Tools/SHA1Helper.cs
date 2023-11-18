using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using Tmsk.Tools;

namespace Server.Tools
{
    /// <summary>
    /// SHA1辅助类
    /// </summary>
    public class SHA1Helper
    {
        /// <summary>
        /// 获取字符串的SHA1值(HEX编码)
        /// </summary>
        /// <param name="str">输入的字符串</param>
        /// <returns></returns>
        public static string get_sha1_string(string str)
        {
            byte[] bytes_sha1_out = get_sha1_bytes(str);
            return DataHelper2.Bytes2HexString(bytes_sha1_out);
        }

        /// <summary>
        /// 获取指定字节流的SHA1值(HEX编码)
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string get_sha1_string(byte[] data)
        {
            byte[] bytes_sha1_out = get_sha1_bytes(data);
            return DataHelper2.Bytes2HexString(bytes_sha1_out);
        }

        /// <summary>
        /// 获取字符串的SHA1值(字节流)
        /// </summary>
        /// <param name="str">输入的字符串</param>
        /// <returns></returns>
        public static byte[] get_sha1_bytes(string str)
        {
            SHA1 sha1 = new SHA1Managed();
            byte[] bytes_sha1_in = new UTF8Encoding().GetBytes(str);
            byte[] hash = sha1.ComputeHash(bytes_sha1_in);
            //sha1.Dispose();
            sha1 = null;
            return hash;
        }

        /// <summary>
        /// 获取指定字节流的SHA1值(HEX编码)
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static byte[] get_sha1_bytes(byte[] data)
        {
            SHA1 sha1 = new SHA1Managed();
            byte[] hash = sha1.ComputeHash(data);
            //sha1.Dispose();
            sha1 = null;
            return hash;
        }

        /// <summary>
        /// 获取字符串的SHA1值(HEX编码), 加入密码保护
        /// </summary>
        /// <param name="str">输入的字符串</param>
        /// <returns></returns>
        public static string get_macksha1_string(string str, string key)
        {
            byte[] bytes_sha1_out = get_macsha1_bytes(str, key);
            return DataHelper2.Bytes2HexString(bytes_sha1_out);
        }

        /// <summary>
        /// 获取指定字节流的SHA1值(HEX编码), 加入密码保护
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string get_macsha1_string(byte[] data, string key)
        {
            byte[] bytes_sha1_out = get_macsha1_bytes(data, key);
            return DataHelper2.Bytes2HexString(bytes_sha1_out);
        }

        /// <summary>
        /// 获取字符串的SHA1值(字节流), 加入密码保护
        /// </summary>
        /// <param name="str">输入的字符串</param>
        /// <returns></returns>
        public static byte[] get_macsha1_bytes(string str, string key)
        {
            byte[] keyBytes = new UTF8Encoding().GetBytes(key);
            HMACSHA1 hmacsha1 = new HMACSHA1(keyBytes);
            byte[] bytes_sha1_in = new UTF8Encoding().GetBytes(str);
            byte[] hash = hmacsha1.ComputeHash(bytes_sha1_in);
            //hmacsha1.Dispose();
            hmacsha1 = null;
            return hash; 
        }

        /// <summary>
        /// 获取指定字节流的SHA1值(HEX编码), 加入密码保护
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static byte[] get_macsha1_bytes(byte[] data, string key)
        {
            byte[] keyBytes = new UTF8Encoding().GetBytes(key);
            HMACSHA1 hmacsha1 = new HMACSHA1(keyBytes);
            byte[] hash = hmacsha1.ComputeHash(data);
            //hmacsha1.Dispose();
            hmacsha1 = null;
            return hash;
        }
    }
}
