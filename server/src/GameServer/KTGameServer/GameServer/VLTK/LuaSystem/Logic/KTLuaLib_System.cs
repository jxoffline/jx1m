using GameServer.KiemThe.Entities;
using MoonSharp.Interpreter;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GameServer.KiemThe.LuaSystem.Logic
{
    /// <summary>
    /// Cung cấp thư viện dùng cho Lua, hệ thống
    /// </summary>
    [MoonSharpUserData]
    public static class KTLuaLib_System
    {
        #region Dynamic Script
        /// <summary>
        /// Đăng ký Script mới vào hệ thống
        /// </summary>
        /// <param name="scriptID"></param>
        /// <param name="obj"></param>
        public static void ReloadScript(int scriptID)
        {
            KTLuaScript.Instance.ReloadScript(KTLuaEnvironment.LuaEnv, scriptID);
        }

        /// <summary>
        /// Thực thi hàm tương ứng ở script chỉ định
        /// </summary>
        /// <param name="scriptID"></param>
        /// <param name="functionName"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public static void CallScriptFunction(int scriptID, string functionName, params object[] param)
        {
            KTLuaScript.Instance.ExecuteFunctionAsync(KTLuaEnvironment.LuaEnv, scriptID, functionName, param, null);
        }
        #endregion

        #region Date and time
        /// <summary>
        /// Trả về Tick hiện tại của hệ thống
        /// </summary>
        /// <returns></returns>
        public static long GetCurrentTimeMilis()
        {
            return KTGlobal.GetCurrentTimeMilis();
        }

        /// <summary>
        /// Trả về giờ hệ thống
        /// </summary>
        /// <returns></returns>
        public static int GetHour()
        {
            return DateTime.Now.Hour;
        }

        /// <summary>
        /// Trả về phút hệ thống
        /// </summary>
        /// <returns></returns>
        public static int GetMinute()
        {
            return DateTime.Now.Minute;
        }

        /// <summary>
        /// Trả về số giây hệ thống
        /// </summary>
        /// <returns></returns>
        public static int GetSecond()
        {
            return DateTime.Now.Second;
        }

        /// <summary>
        /// Trả về ngày trong tháng
        /// </summary>
        /// <returns></returns>
        public static int GetDate()
        {
            return DateTime.Now.Day;
        }

        /// <summary>
        /// Trả về tháng trong năm
        /// </summary>
        /// <returns></returns>
        public static int GetMonth()
        {
            return DateTime.Now.Month;
        }

        /// <summary>
        /// Trả về năm
        /// </summary>
        /// <returns></returns>
        public static int GetYear()
        {
            return DateTime.Now.Year;
        }

        /// <summary>
        /// Trả về ngày trong tuần
        /// <para>Chủ nhật: 0</para>
        /// <para>...</para>
        /// <para>Thứ 7: 6</para>
        /// </summary>
        /// <returns></returns>
        public static int GetDayOfWeek()
        {
            return (int) DateTime.Now.DayOfWeek;
        }

        /// <summary>
        /// Trả về thứ tự tuần trong tháng
        /// </summary>
        /// <returns></returns>
        public static int GetWeek()
        {
            return KTGlobal.GetCurrentWeekOfMonth();
        }
        #endregion

        #region Debug
        /// <summary>
        /// In ra Console
        /// </summary>
        /// <param name="str"></param>
        public static void WriteToConsole(string str)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("[LUA]: " + str);
            Console.ResetColor();
        }
        #endregion

        #region Global Variables
        /// <summary>
        /// Danh sách các biến dữ liệu được lưu lại trong hệ thống
        /// </summary>
        private static readonly Dictionary<int, long> systemGlobalVariables = new Dictionary<int, long>();

        /// <summary>
        /// Trả về giá trị biến toàn cục hệ thống tại vị trí tương ứng
        /// </summary>
        /// <param name="idx"></param>
        /// <returns></returns>
        public static long GetGlobalVariable(int idx)
        {
            lock (KTLuaLib_System.systemGlobalVariables)
            {
                if (KTLuaLib_System.systemGlobalVariables.TryGetValue(idx, out long value))
                {
                    return value;
                }
                KTLuaLib_System.systemGlobalVariables[idx] = 0;
                return 0;
            }
        }

        /// <summary>
        /// Thiết lập giá trị biến toàn cục hệ thống tại vị trí tương ứng
        /// </summary>
        /// <param name="idx"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static void SetGlobalVariable(int idx, long value)
        {
            lock (KTLuaLib_System.systemGlobalVariables)
            {
                KTLuaLib_System.systemGlobalVariables[idx] = value;
            }
        }
        #endregion
    }
}
