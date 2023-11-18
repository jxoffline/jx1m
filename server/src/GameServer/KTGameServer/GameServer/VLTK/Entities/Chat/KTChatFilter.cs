using GameServer.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace GameServer.KiemThe.Entities
{
    /// <summary>
    /// Đối tượng lọc nội dung Chat
    /// </summary>
    public static class KTChatFilter
    {
        /// <summary>
        /// Danh sách các từ/cụm từ cấm sử dụng
        /// </summary>
        private static readonly HashSet<string> forbiddenWords = new HashSet<string>();

        /// <summary>
        /// Ký tự thay thế vào vị trí có từ ngữ cấm
        /// </summary>
        private const char ReplaceChar = '*';

        /// <summary>
        /// Khởi tạo
        /// </summary>
        public static void Init()
        {
            KTChatFilter.forbiddenWords.Clear();

            XElement xmlNode = KTGlobal.ReadXMLData("Config/KT_Chat/ChatFilter.xml");
            foreach (XElement node in xmlNode.Elements("Word"))
            {
                string word = node.Attribute("Value").Value;
                KTChatFilter.forbiddenWords.Add(word);
            }
        }

        /// <summary>
        /// Lọc dữ liệu tin nhắn
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static string Filter(string message)
        {
            message = KTChatFilter.DoFilter(message);
            return message;
        }

        #region Filter core
        /// <summary>
        /// Thực hiện lọc dữ liệu
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        private static string DoFilter(string message)
        {
            /// Chuẩn hóa
            message = KTChatFilter.Normalize(message);
            /// Loại bỏ các từ ngữ cấm
            message = KTChatFilter.RemoveForbiddenWords(message);

            /// Trả ra kết quả
            return message;
        }

        /// <summary>
        /// Chuẩn hóa chuỗi
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        private static string Normalize(string message)
        {
            string result = message;
            result = result.Trim();
            result = Regex.Replace(result, @"\t", " ");
            while (result.IndexOf("  ") != -1)
            {
                result = Regex.Replace(result, @"  ", " ");
            }
            return result;
        }

        /// <summary>
        /// Bỏ các từ/cụm từ cấm
        /// </summary>
        /// <param name="message"></param>
        private static string RemoveForbiddenWords(string message)
        {
            //message = Regex.Replace(@"[\+\-\*\/\.\,\/\'\;\]\[\=\`\~\!\@\#\$\%\^\&\(\)\{\}\:\"\<\>\?]")
            //string[] tokens = message.Split(' ', '.', ',', '/', '"', '\'', '`', '~', '<', '>', '?', '(', ')', '{', '}', '[', ']', '-', '_', '=', '+', '*', '&', '^', '%', '$', '#', '@', '!', ':', ';', '\\', '|', '=', '_');
            string[] tokens = message.Split(' ');
            for (int i = 0; i < tokens.Length; i++)
            {
                string token = tokens[i];
                if (KTChatFilter.forbiddenWords.Contains(token))
                {
                    tokens[i] = "";
                    for (int j = 1; j <= token.Length; j++)
                    {
                        tokens[i] += KTChatFilter.ReplaceChar.ToString();
                    }
                }
            }
            string result = string.Join(" ", tokens);
            return result;
        }
        #endregion
    }
}
