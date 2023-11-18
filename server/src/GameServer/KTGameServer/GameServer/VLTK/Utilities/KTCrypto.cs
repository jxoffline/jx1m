using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace GameServer.KiemThe.Utilities
{
    /// <summary>
    /// Lớp cung cấp các hàm mã hóa và giải mã
    /// </summary>
    public static class KTCrypto
    {
        #region Define
        /// <summary>
        /// Độ dài tối thiểu của mật khẩu
        /// </summary>
        private const int MinPasswordLength = 4;

        /// <summary>
        /// Độ dài tối đa của mật khẩu
        /// </summary>
        private const int MaxPasswordLength = 8;
        #endregion

        #region Const
        /// <summary>
        /// Bảng giải mã ký tự theo byte
        /// </summary>
        private static readonly Dictionary<byte, char> charTable = new Dictionary<byte, char>()
        {
            { (byte) 0, '0' },
            { (byte) 1, '1' },
            { (byte) 2, '2' },
            { (byte) 3, '3' },
            { (byte) 4, '4' },
            { (byte) 5, '5' },
            { (byte) 6, '6' },
            { (byte) 7, '7' },
            { (byte) 8, '8' },
            { (byte) 9, '9' },
            { (byte) 10, 'a' },
            { (byte) 11, 'b' },
            { (byte) 12, 'c' },
            { (byte) 13, 'd' },
            { (byte) 14, 'e' },
            { (byte) 15, 'f' },
            { (byte) 16, 'g' },
            { (byte) 17, 'h' },
            { (byte) 18, 'i' },
            { (byte) 19, 'j' },
            { (byte) 20, 'k' },
            { (byte) 21, 'l' },
            { (byte) 22, 'm' },
            { (byte) 23, 'n' },
            { (byte) 24, 'o' },
            { (byte) 25, 'p' },
            { (byte) 26, 'q' },
            { (byte) 27, 'r' },
            { (byte) 28, 's' },
            { (byte) 29, 't' },
            { (byte) 30, 'u' },
            { (byte) 31, 'v' },
            { (byte) 32, 'w' },
            { (byte) 33, 'x' },
            { (byte) 34, 'y' },
            { (byte) 35, 'z' },
            { (byte) 36, '_' },
            { (byte) 37, 'A' },
            { (byte) 38, 'B' },
            { (byte) 39, 'C' },
            { (byte) 40, 'D' },
            { (byte) 41, 'E' },
            { (byte) 42, 'F' },
            { (byte) 43, 'G' },
            { (byte) 44, 'H' },
            { (byte) 45, 'I' },
            { (byte) 46, 'J' },
            { (byte) 47, 'K' },
            { (byte) 48, 'L' },
            { (byte) 49, 'M' },
            { (byte) 50, 'N' },
            { (byte) 51, 'O' },
            { (byte) 52, 'P' },
            { (byte) 53, 'Q' },
            { (byte) 54, 'R' },
            { (byte) 55, 'S' },
            { (byte) 56, 'T' },
            { (byte) 57, 'U' },
            { (byte) 58, 'V' },
            { (byte) 59, 'W' },
            { (byte) 60, 'X' },
            { (byte) 61, 'Y' },
            { (byte) 62, 'Z' },
            { (byte) 63, '.' },
            { (byte) 64, '/' },
        };

        /// <summary>
        /// Bảng mã hóa ký tự dạng byte
        /// </summary>
        private static Dictionary<char, byte> byteTable = new Dictionary<char, byte>()
        {
            { '0', (byte) 0 },
            { '1', (byte) 1 },
            { '2', (byte) 2 },
            { '3', (byte) 3 },
            { '4', (byte) 4 },
            { '5', (byte) 5 },
            { '6', (byte) 6 },
            { '7', (byte) 7 },
            { '8', (byte) 8 },
            { '9', (byte) 9 },
            { 'a', (byte) 10 },
            { 'b', (byte) 11 },
            { 'c', (byte) 12 },
            { 'd', (byte) 13 },
            { 'e', (byte) 14 },
            { 'f', (byte) 15 },
            { 'g', (byte) 16 },
            { 'h', (byte) 17 },
            { 'i', (byte) 18 },
            { 'j', (byte) 19 },
            { 'k', (byte) 20 },
            { 'l', (byte) 21 },
            { 'm', (byte) 22 },
            { 'n', (byte) 23 },
            { 'o', (byte) 24 },
            { 'p', (byte) 25 },
            { 'q', (byte) 26 },
            { 'r', (byte) 27 },
            { 's', (byte) 28 },
            { 't', (byte) 29 },
            { 'u', (byte) 30 },
            { 'v', (byte) 31 },
            { 'w', (byte) 32 },
            { 'x', (byte) 33 },
            { 'y', (byte) 34 },
            { 'z', (byte) 35 },
            { '_', (byte) 36 },
            { 'A', (byte) 37 },
            { 'B', (byte) 38 },
            { 'C', (byte) 39 },
            { 'D', (byte) 40 },
            { 'E', (byte) 41 },
            { 'F', (byte) 42 },
            { 'G', (byte) 43 },
            { 'H', (byte) 44 },
            { 'I', (byte) 45 },
            { 'J', (byte) 46 },
            { 'K', (byte) 47 },
            { 'L', (byte) 48 },
            { 'M', (byte) 49 },
            { 'N', (byte) 50 },
            { 'O', (byte) 51 },
            { 'P', (byte) 52 },
            { 'Q', (byte) 53 },
            { 'R', (byte) 54 },
            { 'S', (byte) 55 },
            { 'T', (byte) 56 },
            { 'U', (byte) 57 },
            { 'V', (byte) 58 },
            { 'W', (byte) 59 },
            { 'X', (byte) 60 },
            { 'Y', (byte) 61 },
            { 'Z', (byte) 62 },
            { '.', (byte) 63 },
            { '/', (byte) 64 },
        };
        #endregion

        /// <summary>
        /// Lớp mã hóa chuỗi
        /// </summary>
        private static class Cryptography
        {
            #region Settings

            private static int _iterations = 2;
            private static int _keySize = 256;

            private static string _hash = "SHA1";
            private static string _salt = "aselrias38490a32"; // Random
            private static string _vector = "8947az34awl34kjq"; // Random

            #endregion

            /// <summary>
            /// Mã hóa chuỗi theo AES
            /// </summary>
            /// <param name="value"></param>
            /// <param name="password"></param>
            /// <returns></returns>
            public static string Encrypt(string value, string password)
            {
                return Cryptography.Encrypt<AesManaged>(value, password);
            }

            /// <summary>
            /// Mã hóa chuỗi theo thuật toán T
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="value"></param>
            /// <param name="password"></param>
            /// <returns></returns>
            public static string Encrypt<T>(string value, string password) where T : SymmetricAlgorithm, new()
            {
                byte[] vectorBytes = new ASCIIEncoding().GetBytes(_vector);
                byte[] saltBytes = new ASCIIEncoding().GetBytes(_salt);
                byte[] valueBytes = new UTF8Encoding().GetBytes(value);

                byte[] encrypted;
                using (T cipher = new T())
                {
                    PasswordDeriveBytes _passwordBytes =
                        new PasswordDeriveBytes(password, saltBytes, _hash, _iterations);
                    byte[] keyBytes = _passwordBytes.GetBytes(_keySize / 8);

                    cipher.Mode = CipherMode.CBC;

                    using (ICryptoTransform encryptor = cipher.CreateEncryptor(keyBytes, vectorBytes))
                    {
                        using (MemoryStream to = new MemoryStream())
                        {
                            using (CryptoStream writer = new CryptoStream(to, encryptor, CryptoStreamMode.Write))
                            {
                                writer.Write(valueBytes, 0, valueBytes.Length);
                                writer.FlushFinalBlock();
                                encrypted = to.ToArray();
                            }
                        }
                    }
                    cipher.Clear();
                }
                return Convert.ToBase64String(encrypted);
            }

            /// <summary>
            /// Giải mã chuỗi theo AES
            /// </summary>
            /// <param name="value"></param>
            /// <param name="password"></param>
            /// <returns></returns>
            public static string Decrypt(string value, string password)
            {
                return Decrypt<AesManaged>(value, password);
            }

            /// <summary>
            /// Giải mã chuỗi theo thuật toán T
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="value"></param>
            /// <param name="password"></param>
            /// <returns></returns>
            public static string Decrypt<T>(string value, string password) where T : SymmetricAlgorithm, new()
            {
                byte[] vectorBytes = new ASCIIEncoding().GetBytes(_vector);
                byte[] saltBytes = new ASCIIEncoding().GetBytes(_salt);
                byte[] valueBytes = Convert.FromBase64String(value);

                byte[] decrypted;
                int decryptedByteCount = 0;

                using (T cipher = new T())
                {
                    PasswordDeriveBytes _passwordBytes = new PasswordDeriveBytes(password, saltBytes, _hash, _iterations);
                    byte[] keyBytes = _passwordBytes.GetBytes(_keySize / 8);

                    cipher.Mode = CipherMode.CBC;

                    try
                    {
                        using (ICryptoTransform decryptor = cipher.CreateDecryptor(keyBytes, vectorBytes))
                        {
                            using (MemoryStream from = new MemoryStream(valueBytes))
                            {
                                using (CryptoStream reader = new CryptoStream(from, decryptor, CryptoStreamMode.Read))
                                {
                                    decrypted = new byte[valueBytes.Length];
                                    decryptedByteCount = reader.Read(decrypted, 0, decrypted.Length);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        return String.Empty;
                    }

                    cipher.Clear();
                }
                return Encoding.UTF8.GetString(decrypted, 0, decryptedByteCount);
            }
        }

        /// <summary>
        /// Đối tượng Random
        /// </summary>
        private static readonly Random random = new Random();

        /// <summary>
        /// Trả về chuỗi ngẫu nhiên nằm trong danh sách
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        private static string GetRandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789_/.";
            return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
        }

        /// <summary>
        /// Mã hóa chuỗi đầu vào thành dạng byte[]
        /// </summary>
        /// <param name="input"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static byte[] Encrypt(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return new byte[] { };
            }

            /// Độ dài chuỗi mật khẩu ngẫu nhiên sẽ được sinh ra
            int passwordLength = KTCrypto.random.Next(KTCrypto.MinPasswordLength, KTCrypto.MaxPasswordLength);
            /// Sinh chuỗi mật khẩu ngẫu nhiên với độ dài tương ứng
            string password = KTCrypto.GetRandomString(passwordLength);

            byte[] encryptedByte = new ASCIIEncoding().GetBytes(Cryptography.Encrypt(input, password));

            /// Danh sách byte tương ứng
            List<byte> output = new List<byte>();

            /// Byte đầu tiên chứa độ dài chuỗi mật khẩu
            output.Add((byte)password.Length);

            /// Các byte tiếp theo mã hóa cho mật khẩu
            foreach (char c in password)
            {
                output.Add(KTCrypto.byteTable[c]);
            }

            /// Cuối cùng theo sau là chuỗi byte mã hóa cho chuỗi mã hóa
            output.AddRange(encryptedByte);

            return output.ToArray();
        }

        /// <summary>
        /// Giải mã dãy byte[] đầu vào
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string Decrypt(byte[] input)
        {
            if (input == null || input.Length <= 0)
            {
                return "";
            }

            /// Byte đầu tiên chứa độ dài chuỗi mật khẩu
            int passwordLength = (int)input[0];

            /// Các Byte sau chứa thông tin mã hóa cho mật khẩu
            string password = "";
            for (int i = 1; i <= passwordLength; i++)
            {
                password += KTCrypto.charTable[input[i]];
            }

            /// Các Byte sau kể từ vị trí cuối cùng của Byte mã hóa cho mật khẩu, cho đến hết dãy là chuỗi đã được mã hóa
            byte[] inputBytes = new byte[input.Length - passwordLength - 1];
            Array.Copy(input, passwordLength + 1, inputBytes, 0, input.Length - passwordLength - 1);

            string inputString = new ASCIIEncoding().GetString(inputBytes);
            return Cryptography.Decrypt(inputString, password);
        }
    }
}
