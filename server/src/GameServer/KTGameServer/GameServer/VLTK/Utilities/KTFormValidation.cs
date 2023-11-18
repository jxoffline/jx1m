using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace GameServer.KiemThe.Utilities
{
    /// <summary>
    /// Kiểm tra tính hợp lệ của Form nhập liệu
    /// </summary>
    public static class KTFormValidation
    {
        /// <summary>
        /// Danh sách các ký tự hợp lệ
        /// </summary>
        private static readonly HashSet<char> ValidChars = new HashSet<char>()
        {
            'a', 'á', 'à', 'ả', 'ã', 'ạ', 'ă', 'ắ', 'ằ', 'ẳ', 'ẵ', 'ặ', 'â', 'ấ', 'ầ', 'ẩ', 'ẫ', 'ậ', 'b', 'c', 'd', 'đ', 'e', 'é', 'è', 'ẻ', 'ẽ', 'ẹ', 'ê', 'ế', 'ề', 'ể', 'ễ', 'ệ', 'f', 'g', 'h', 'i', 'í', 'ì', 'ỉ', 'ĩ', 'ị', 'j', 'k', 'l', 'm', 'n', 'o', 'ó', 'ò', 'ỏ', 'õ', 'ọ', 'ô', 'ố', 'ồ', 'ổ', 'ỗ', 'ộ', 'ơ', 'ớ', 'ờ', 'ở', 'ỡ', 'ợ', 'p', 'q', 'r', 's', 't', 'u', 'ú', 'ù', 'ủ', 'ũ', 'ụ', 'ư', 'ứ', 'ừ', 'ử', 'ữ', 'ự', 'v', 'x', 'w', 'y', 'ý', 'ỳ', 'ỷ', 'ỹ', 'ỵ', 'z',
            'A', 'Á', 'À', 'Ả', 'Ã', 'Ạ', 'Ă', 'Ắ', 'Ằ', 'Ẳ', 'Ẵ', 'Ặ', 'Â', 'Ấ', 'Ầ', 'Ẩ', 'Ẫ', 'Ậ', 'B', 'C', 'D', 'Đ', 'E', 'É', 'È', 'Ẻ', 'Ẽ', 'Ẹ', 'Ê', 'Ế', 'Ề', 'Ể', 'Ễ', 'Ệ', 'F', 'G', 'H', 'I', 'Í', 'Ì', 'Ỉ', 'Ĩ', 'Ị', 'J', 'K', 'L', 'M', 'N', 'O', 'Ó', 'Ò', 'Ỏ', 'Õ', 'Ọ', 'Ô', 'Ố', 'Ồ', 'Ổ', 'Ỗ', 'Ộ', 'Ơ', 'Ớ', 'Ờ', 'Ở', 'Ỡ', 'Ợ', 'P', 'Q', 'R', 'S', 'T', 'U', 'Ú', 'Ù', 'Ủ', 'Ũ', 'Ụ', 'Ư', 'Ứ', 'Ừ', 'Ử', 'Ữ', 'Ự', 'V', 'X', 'W', 'Y', 'Ý', 'Ỳ', 'Ỷ', 'Ỹ', 'Ỵ', 'Z',
            '0', '1', '2', '3', '4', '5', '6', '7', '8', '9',
            ' ',
            '.', ',', ';', ':', '+', '-', '*', '/', '(', ')',
        };

        /// <summary>
        /// Kiểm tra chuỗi nhập vào có hợp lệ không, nếu hợp lệ thì chuẩn hóa
        /// </summary>
        /// <param name="inputString">Chuỗi đầu vào</param>
        /// <param name="isAllowNumber">Chấp nhận số không</param>
        /// <param name="isAllowEmptyString">Chấp nhận chuỗi rỗng không</param>
        /// <param name="isAllowWhiteSpace">Chấp nhận ký tự khoảng trắng không</param>
        /// <param name="isAllowParagraphInteruptor">Chấp nhận ký tự ngắt trong đoạn văn (chấm, phẩy, hai chấm, chấm phẩy, cộng, trừ, nhân, chia, ngoặc tròn)</param>
        /// <returns></returns>
        public static bool IsValidString(string inputString, bool isAllowEmptyString = false, bool isAllowNumber = true, bool isAllowWhiteSpace = true, bool isAllowParagraphInteruptor = true)
        {
            if (!isAllowEmptyString && string.IsNullOrEmpty(inputString))
            {
                return false;
            }
            if (!isAllowNumber && Regex.IsMatch(inputString, @"\d"))
            {
                return false;
            }
            if (!isAllowWhiteSpace && Regex.IsMatch(inputString, @" "))
            {
                return false;
            }
            if (!isAllowParagraphInteruptor && Regex.IsMatch(inputString, @"[\.\,\;\:\+\-\*\/\(\)]"))
            {
                return false;
            }

            foreach (char c in inputString)
            {
                if (!KTFormValidation.ValidChars.Contains(c))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Kiểm tra chuỗi nhập vào có phải số không
        /// </summary>
        /// <param name="inputString">Chuỗi đầu vào</param>
        /// <returns></returns>
        public static bool IsValidNumber(string inputString)
        {
            try
            {
                int.Parse(inputString);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Chuẩn hóa chuỗi
        /// </summary>
        /// <param name="inputString">Chuỗi đầu vào</param>
        /// <param name="removeAllWhiteSpaces">Xóa toàn bộ các ký tự khoảng trống</param>
        /// <param name="standardizeWhiteSpace">Chuẩn hóa các ký tự khoảng trống không</param>
        /// <returns></returns>
        public static string StandardizeString(string inputString, bool removeAllWhiteSpaces, bool standardizeWhiteSpace = true)
        {
            string outputString = inputString.Trim();

            if (removeAllWhiteSpaces)
            {
                outputString = Regex.Replace(inputString.Trim(), @" ", "");
            }
            if (standardizeWhiteSpace)
            {
                outputString = Regex.Replace(inputString.Trim(), @"  ", " ");
            }

            return outputString;
        }

        /// <summary>
        /// Kiểm tra địa chỉ Email nhập vào có hợp lệ không
        /// </summary>
        /// <param name="inputString"></param>
        /// <returns></returns>
        public static bool IsValidEmail(string inputString)
        {
            if (string.IsNullOrEmpty(inputString))
            {
                return false;
            }
            return Regex.IsMatch(inputString, @"([A-Za-z0-9_.]+[A-Za-z0-9_])@([A-Za-z0-9_.]+[A-Za-z0-9_])");
        }
    }
}