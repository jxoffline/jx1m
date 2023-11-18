using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameServer.KiemThe.Entities;
using GameServer.KiemThe.LuaSystem.Entities;
using GameServer.KiemThe.LuaSystem.Logic;
using MoonSharp;
using MoonSharp.Interpreter;

namespace GameServer.KiemThe.LuaSystem
{
    /// <summary>
    /// Hệ thống Lua
    /// </summary>
    public static partial class KTLuaEnvironment
    {
        /// <summary>
        /// Đối tượng thực thi Script
        /// </summary>
        public static readonly Script LuaEnv = new Script();

        /// <summary>
        /// Khởi tạo môi trường
        /// </summary>
        public static void Init()
        {
            // Automatically register all MoonSharpUserData types
            UserData.RegisterAssembly();
            //KTLuaEnvironment.LuaEnv.Options.CheckThreadAccess = false;

            KTLuaEnvironment.LuaEnv.Globals["System"] = typeof(KTLuaLib_System);
            KTLuaEnvironment.LuaEnv.Globals["Math"] = typeof(KTLuaLib_Math);
            KTLuaEnvironment.LuaEnv.Globals["String"] = typeof(KTLuaLib_String);
            KTLuaEnvironment.LuaEnv.Globals["GUI"] = typeof(KTLuaLib_GUI);
            KTLuaEnvironment.LuaEnv.Globals["Player"] = typeof(KTLuaLib_Player);
            KTLuaEnvironment.LuaEnv.Globals["EventManager"] = typeof(KTLuaLib_Event);
            KTLuaEnvironment.LuaEnv.Globals["Timer"] = typeof(KTLuaLib_Timer);
            KTLuaEnvironment.LuaEnv.Globals["Scripts"] = new Table(KTLuaEnvironment.LuaEnv);

            /// Đăng ký các hằng só mặc định
            KTLuaEnvironment.RegisterConstants();
            /// Đọc dữ liệu ScriptLua
            KTLuaScript.Init();
            /// Tải các Script mặc định khởi động cùng hệ thống
            KTLuaEnvironment.LoadDefaultScripts();
        }

        /// <summary>
        /// Load các Script mặc định khởi động cùng hệ thống
        /// </summary>
        private static void LoadDefaultScripts()
        {
            KTLuaScript.Instance.LoadScriptAsync(KTLuaEnvironment.LuaEnv, "ScriptGlobal.lua");
        }

        /// <summary>
        /// Đăng ký các hằng số mặc định
        /// </summary>
        private static void RegisterConstants()
        {
            string baseClass = @"
                BaseClass = {}
                function BaseClass:New(o)
                    o = o or {}
	                setmetatable(o, self)
	                self.__index = self
	                return o
                end
            ";
            KTLuaEnvironment.LuaEnv.DoString(baseClass);
        }
    }
}
