using GameServer.KiemThe.LuaSystem.Entities.Math;
using MoonSharp.Interpreter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.KiemThe.LuaSystem.Logic
{
    /// <summary>
    /// Cung cấp thư viện dùng cho Lua, toán học
    /// </summary>
    [MoonSharpUserData]
    public static class KTLuaLib_Math
    {
        /// <summary>
        /// Đối tượng Vector trên mặt phẳng 2 chiều
        /// </summary>
        public static Type Vector2
        {
            get
            {
                return typeof(Lua_Vector2);
            }
        }
    }
}
