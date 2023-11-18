using GameServer.KiemThe.Entities;
using GameServer.KiemThe.Logic;
using GameServer.KiemThe.LuaSystem.Entities;
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
    public static class KTLuaLib_Timer
    {
        /// <summary>
        /// Tạo Timer tương ứng
        /// </summary>
        /// <returns></returns>
        public static Lua_Timer CreateTimer()
        {
            return new Lua_Timer()
            {
                RefObject = new KTTaskManager.KTSchedule()
                {
                    Name = "Lua-Schedule",
                },
            };
        }

        /// <summary>
        /// Tạo Task tương ứng
        /// </summary>
        /// <returns></returns>
        public static Lua_Task CreateTask()
        {
            KTTaskManager.KTTask task = KTTaskManager.KTTask.New();
            task.Name = "Lua-Task";

            return new Lua_Task()
            {
                RefObject = KTTaskManager.KTTask.New(),
            };
        }

        /// <summary>
        /// Tạo Task tương ứng
        /// </summary>
        /// <param name="func"></param>
        /// <returns></returns>
        public static Lua_Task CreateTask(Closure func)
        {
            KTTaskManager.KTTask task = KTTaskManager.KTTask.New(() => {
                func?.Call();
            });
            task.Name = "Lua-Task";

            return new Lua_Task()
            {
                RefObject = task,
            };
        }

        /// <summary>
        /// Thực thi công việc sau khoảng thời gian tương ứng
        /// </summary>
        /// <param name="sec"></param>
        /// <param name="func"></param>
        public static Lua_Timer SetTimeout(float sec, Closure func)
        {
            KTTaskManager.KTSchedule timer = new KTTaskManager.KTSchedule()
            {
                Name = "Lua-Schedule",
                Interval = sec,
                Loop = false,
                Work = () =>
                {
                    func?.Call();
                },
            };
            KTTaskManager.Instance.AddSchedule(timer);
            return new Lua_Timer()
            {
                RefObject = timer,
            };
        }

        /// <summary>
        /// Thực thi công việc sau khoảng thời gian tương ứng và lặp đi lặp lại
        /// </summary>
        /// <param name="sec"></param>
        /// <param name="func"></param>
        public static Lua_Timer SetInterval(float sec, Closure func)
        {
            KTTaskManager.KTSchedule timer = new KTTaskManager.KTSchedule()
            {
                Name = "Lua-Schedule",
                Interval = sec,
                Loop = true,
                Work = () =>
                {
                    func?.Call();
                },
            };
            KTTaskManager.Instance.AddSchedule(timer);
            return new Lua_Timer()
            {
                RefObject = timer,
            };
        }
    }
}
