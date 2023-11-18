using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Server.Tools
{
    /// <summary>
    /// RC4加密辅助类
    /// </summary>
    public class RC4Helper
    {
        /// <summary>
        /// RC4加解密函数
        /// </summary>
        /// <param name="bytesData">待加(解)密的字节流</param>
        /// <param name="key">字节流密码</param>
        public static void RC4(byte[] bytesData, int offset, int count, string key)
        {
            byte[] keyBytes = UTF8Encoding.UTF8.GetBytes(key);
            RC4(bytesData, offset, count, keyBytes);
        }

        /// <summary>
        /// RC4加解密函数
        /// </summary>
        /// <param name="bytesData">待加(解)密的字节流</param>
        /// <param name="key">字节流密码</param>
        public static void RC4(byte[] bytesData, int offset, int count, byte[] key)
        {
            byte[] s = new byte[256];
            byte[] k = new byte[256];
            byte temp;
            int i, j;

            for (i = 0; i < 256; i++)
            {
                s[i] = (byte)i;
                k[i] = key[i % key.GetLength(0)];
            }

            j = 0;
            for (i = 0; i < 256; i++)
            {
                j = (j + s[i] + k[i]) % 256;
                temp = s[i];
                s[i] = s[j];
                s[j] = temp;
            }

            i = j = 0;
            for (int x = offset; x < (offset + count); x++)
            {
                i = (i + 1) % 256;
                j = (j + s[i]) % 256;
                temp = s[i];
                s[i] = s[j];
                s[j] = temp;
                int t = (s[i] + s[j]) % 256;
                bytesData[x] ^= s[t];
            }
        }

        /// <summary>
        /// RC4加解密函数
        /// </summary>
        /// <param name="bytesData">待加(解)密的字节流</param>
        /// <param name="key">字节流密码</param>
        public static void RC4(byte[] bytesData, byte[] key)
        {
            RC4(bytesData, 0, bytesData.Length, key);
        }

        /// <summary>
        /// RC4加解密函数
        /// </summary>
        /// <param name="bytesData">待加(解)密的字节流</param>
        /// <param name="key">字节流密码</param>
        public static void RC4(byte[] bytesData, string key)
        {
            byte[] keyBytes = UTF8Encoding.UTF8.GetBytes(key);
            RC4(bytesData, keyBytes);
        }
    }
}
