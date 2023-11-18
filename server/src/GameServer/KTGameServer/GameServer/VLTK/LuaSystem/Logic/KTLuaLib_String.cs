using GameServer.KiemThe.LuaSystem.Entities.Math;
using MoonSharp.Interpreter;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GameServer.KiemThe.LuaSystem.Logic
{
    /// <summary>
    /// Cung cấp thư viện dùng cho Lua, xử lý chuỗi
    /// </summary>
    [MoonSharpUserData]
    public static class KTLuaLib_String
    {
        /// <summary>
        /// Tách chuỗi thành các Token dựa vào ký tự ngăn cách tương ứng
        /// </summary>
        /// <param name="str"></param>
        /// <param name="separate"></param>
        /// <returns></returns>
        public static string[] Split(string str, char separate)
        {
            return str.Split(separate);
        }

        /// <summary>
        /// Nối chuỗi tương ứng và ngăn cách bởi chuỗi cho trước
        /// </summary>
        /// <param name="separate"></param>
        /// <param name="strs"></param>
        /// <returns></returns>
        public static string Join(string separate, params string[] strs)
        {
            return string.Join(separate, strs);
        }
    }
}
