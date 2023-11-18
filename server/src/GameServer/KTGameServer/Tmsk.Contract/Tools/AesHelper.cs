using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Security.Cryptography;

namespace Server.Tools
{
    /// <summary>
    /// AES加密辅助类
    /// </summary>
    public class AesHelper
    {
        /// <summary>
        /// AES加密字节流
        /// </summary>
        /// <param name="data">明文字节流</param>
        /// <param name="passwd">密码</param>
        /// <param name="saltValue">盐值</param>
        /// <returns>加密的字节流</returns>
        public static byte[] AesEncryptBytes(byte[] data, string passwd, string saltValue)
        {
            // 盐值(加解密的程序中这个值必须一致）
            // 密码值加解密的程序中这个值必须一致）
            byte[] saltBytes = UTF8Encoding.UTF8.GetBytes(saltValue);

            // AesManaged - 高级加密标准(AES) 对称算法的管理类
            AesManaged aes = new AesManaged();

            // Rfc2898DeriveBytes - 通过使用基于 HMACSHA1 的伪随机数生成器，实现基于密码的密钥派生功能 (PBKDF2 - 一种基于密码的密钥派生函数)
            // 通过密码和salt派生密钥
            Rfc2898DeriveBytes rfc = new Rfc2898DeriveBytes(passwd, saltBytes);

            /*
            * AesManaged.BlockSize - 加密操作的块大小（单位：bit）
            * AesManaged.LegalBlockSizes - 对称算法支持的块大小（单位：bit）
            * AesManaged.KeySize - 对称算法的密钥大小（单位：bit）
            * AesManaged.LegalKeySizes - 对称算法支持的密钥大小（单位：bit）
            * AesManaged.Key - 对称算法的密钥
            * AesManaged.IV - 对称算法的密钥大小
            * Rfc2898DeriveBytes.GetBytes(int 需要生成的伪随机密钥字节数) - 生成密钥
            */

            aes.BlockSize = aes.LegalBlockSizes[0].MaxSize;
            aes.KeySize = aes.LegalKeySizes[0].MaxSize;
            aes.Key = rfc.GetBytes(aes.KeySize / 8);
            aes.IV = rfc.GetBytes(aes.BlockSize / 8);

            // 用当前的 Key 属性和初始化向量 IV 创建对称加密器对象
            ICryptoTransform encryptTransform = aes.CreateEncryptor();

            // 加密后的输出流
            MemoryStream encryptStream = new MemoryStream();

            // 将加密后的目标流（encryptStream）与加密转换（encryptTransform）相连接
            CryptoStream encryptor = new CryptoStream(encryptStream, encryptTransform, CryptoStreamMode.Write);

            // 将一个字节序列写入当前 CryptoStream （完成加密的过程）
            encryptor.Write(data, 0, data.Length);
            encryptor.Close();

            // 将加密后所得到的流转换成字节数组
            return encryptStream.ToArray();
        }

        /// <summary>
        /// AES解密字节流
        /// </summary>
        /// <param name="encryptData">密文字节流</param>
        /// <param name="passwd">密码</param>
        /// <param name="saltValue">盐值</param>
        /// <returns>解密后的明文字节流</returns>
        public static byte[] AesDecryptBytes(byte[] encryptData, string passwd, string saltValue)
        {
            // 盐值（与加密时设置的值必须一致）
            // 密码值（与加密时设置的值必须一致）
            byte[] saltBytes = Encoding.UTF8.GetBytes(saltValue);

            AesManaged aes = new AesManaged();
            Rfc2898DeriveBytes rfc = new Rfc2898DeriveBytes(passwd, saltBytes);

            aes.BlockSize = aes.LegalBlockSizes[0].MaxSize;
            aes.KeySize = aes.LegalKeySizes[0].MaxSize;
            aes.Key = rfc.GetBytes(aes.KeySize / 8);
            aes.IV = rfc.GetBytes(aes.BlockSize / 8);

            // 用当前的 Key 属性和初始化向量 IV 创建对称解密器对象
            ICryptoTransform decryptTransform = aes.CreateDecryptor();

            // 解密后的输出流
            MemoryStream decryptStream = new MemoryStream();

            // 将解密后的目标流（decryptStream）与解密转换（decryptTransform）相连接
            CryptoStream decryptor = new CryptoStream(decryptStream, decryptTransform, CryptoStreamMode.Write);

            try
            {
                // 将一个字节序列写入当前 CryptoStream （完成解密的过程）
                decryptor.Write(encryptData, 0, encryptData.Length);
                decryptor.Close();
            }
            catch (System.Exception)
            {
                return null; //解密失败
            }

            // 将解密后所得到的流转换为字符串
            return decryptStream.ToArray();
        }
    }
}
