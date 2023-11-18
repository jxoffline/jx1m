using GameServer.KiemThe.Logic;
using MoonSharp.Interpreter;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GameServer.KiemThe.LuaSystem.Entities
{
    /// <summary>
    /// Thực thể Task dùng cho Lua
    /// </summary>
    [MoonSharpUserData]
    public class Lua_Task
    {
        /// <summary>
        /// Đối tượng Task tương ứng
        /// </summary>
        [MoonSharpHidden]
        public KTTaskManager.KTTask RefObject { get; set; }

        /// <summary>
        /// Thiết lập tên Task
        /// </summary>
        /// <param name="name"></param>
        public void SetName(string name)
        {
            this.RefObject.Name = name;
        }

        /// <summary>
        /// Đợi 1 khoảng thời gian
        /// </summary>
        /// <param name="sec"></param>
        public Lua_Task Wait(float sec)
        {
            this.RefObject.Wait(sec);
            return this;
        }

        /// <summary>
        /// Thêm công việc cần thực thi
        /// </summary>
        /// <param name="func"></param>
        public Lua_Task Then(Closure func)
        {
            this.RefObject.Then(() => {
                func?.Call();
            });
            return this;
        }

        /// <summary>
        /// Bắt đầu Task
        /// </summary>
        public void Begin()
        {
            KTTaskManager.Instance.AddTask(this.RefObject);
        }

        /// <summary>
        /// Ngừng Task
        /// </summary>
        public void Stop()
        {
            KTTaskManager.Instance.RemoveTask(this.RefObject);
        }
    }
}
