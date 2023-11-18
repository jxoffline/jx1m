using GameServer.KiemThe.GameDbController;
using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace GameServer.KiemThe
{
    /// <summary>
    /// Các phương thức và đối tượng toàn cục của Kiếm Thế
    /// </summary>
    public static partial class KTGlobal
    {
        #region Test Check

        /// <summary>
        /// Bật chế độ Test
        /// </summary>
        public static bool IsTestModel { get; set; } = false;


        #endregion

        #region Đường dẫn
        /// <summary>
        /// Đường dẫn folder chứa dữ liệu (Config)
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public static string GetDataPath(string uri)
        {
            return string.Format("Config/{0}", uri);
        }

        /// <summary>
        /// Đọc dữ liệu từ File dữ liệu tương ứng (Config)
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public static XElement ReadXMLData(string uri)
        {
            uri = KTGlobal.GetDataPath(uri);
            return XElement.Parse(File.ReadAllText(uri));
        }
        #endregion

        #region System Global Parameters

        /// <summary>
        /// Lưu giá trị biến toàn cục hệ thống
        /// </summary>
        /// <param name="id"></param>
        /// <param name="value"></param>
        public static void SetSystemGlobalParameter(int id, int value)
        {
            GameDb.SetSystemGlobalParameters(id, value.ToString());
        }

        /// <summary>
        /// Lưu giá trị biến toàn cục hệ thống
        /// </summary>
        /// <param name="id"></param>
        /// <param name="value"></param>
        public static void SetSystemGlobalParameter(int id, long value)
        {
            GameDb.SetSystemGlobalParameters(id, value.ToString());
        }

        /// <summary>
        /// Lưu giá trị biến toàn cục hệ thống
        /// </summary>
        /// <param name="id"></param>
        /// <param name="value"></param>
        public static void SetSystemGlobalParameter(int id, float value)
        {
            GameDb.SetSystemGlobalParameters(id, value.ToString());
        }

        /// <summary>
        /// Lưu giá trị biến toàn cục hệ thống
        /// </summary>
        /// <param name="id"></param>
        /// <param name="value"></param>
        public static void SetSystemGlobalParameter(int id, double value)
        {
            GameDb.SetSystemGlobalParameters(id, value.ToString());
        }

        /// <summary>
        /// Lưu giá trị biến toàn cục hệ thống
        /// </summary>
        /// <param name="id"></param>
        /// <param name="value"></param>
        public static void SetSystemGlobalParameter(int id, string value)
        {
            GameDb.SetSystemGlobalParameters(id, value);
        }

        /// <summary>
        /// Trả về giá trị biến toàn cục hệ thống
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static int GetSystemGlobalParameterInt32(int id)
        {
            string value = GameDb.GetSystemGlobalParameter(id);
            if (string.IsNullOrEmpty(value))
            {
                return 0;
            }
            return Convert.ToInt32(value);
        }

        /// <summary>
        /// Trả về giá trị biến toàn cục hệ thống
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static long GetSystemGlobalParameterInt64(int id)
        {
            string value = GameDb.GetSystemGlobalParameter(id);
            if (string.IsNullOrEmpty(value))
            {
                return 0;
            }
            return Convert.ToInt64(value);
        }

        /// <summary>
        /// Trả về giá trị biến toàn cục hệ thống
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static float GetSystemGlobalParameterFloat(int id)
        {
            string value = GameDb.GetSystemGlobalParameter(id);
            if (string.IsNullOrEmpty(value))
            {
                return 0;
            }
            return (float) Convert.ToDouble(value);
        }

        /// <summary>
        /// Trả về giá trị biến toàn cục hệ thống
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static double GetSystemGlobalParameterDouble(int id)
        {
            string value = GameDb.GetSystemGlobalParameter(id);
            if (string.IsNullOrEmpty(value))
            {
                return 0;
            }
            return Convert.ToDouble(value);
        }

        /// <summary>
        /// Check ký tự đặc biệt
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static bool hasSpecialChar(string input)
        {
            string specialChar = @"\|!#$%&/()=?»«@£§€{}.-;'<>_,";
            foreach (var item in specialChar)
            {
                if (input.Contains(item))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Trả về giá trị biến toàn cục hệ thống
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static string GetSystemGlobalParameterString(int id)
        {
            string value = GameDb.GetSystemGlobalParameter(id);
            if (string.IsNullOrEmpty(value))
            {
                return "";
            }
            return value;
        }

        #endregion System Global Parameters
    }
}
