using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tmsk.Tools;

namespace Server.Tools
{
    public class MD5Helper
    {
        /// <summary>
        /// 获取字符串的MD5值(HEX编码)
        /// </summary>
        /// <param name="str">输入的字符串</param>
        /// <returns></returns>
        public static string get_md5_string(string str)
        {
            byte[] bytes_md5_out = MD5Core.GetHash(str);
            return DataHelper2.Bytes2HexString(bytes_md5_out);
        }

        /// <summary>
        /// 获取指定字节流的MD5值(HEX编码)
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string get_md5_string(byte[] data)
        {
            byte[] bytes_md5_out = MD5Core.GetHash(data);
            return DataHelper2.Bytes2HexString(bytes_md5_out);
        }

        /// <summary>
        /// 获取字符串的MD5值(字节流)
        /// </summary>
        /// <param name="str">输入的字符串</param>
        /// <returns></returns>
        public static byte[] get_md5_bytes(string str)
        {
            byte[] bytes_md5_out = MD5Core.GetHash(str);
            return bytes_md5_out;
        }

        /// <summary>
        /// 获取指定字节流的MD5值(HEX编码)
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static byte[] get_md5_bytes(byte[] data)
        {
            byte[] bytes_md5_out = MD5Core.GetHash(data);
            return bytes_md5_out;
        }
    }
}
