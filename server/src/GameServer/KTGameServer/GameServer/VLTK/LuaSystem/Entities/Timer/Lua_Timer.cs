using GameServer.KiemThe.Logic;
using MoonSharp.Interpreter;
using Server.Tools;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GameServer.KiemThe.LuaSystem.Entities
{
    /// <summary>
    /// Thực thể Timer dùng cho Lua
    /// </summary>
    [MoonSharpUserData]
    public class Lua_Timer
    {
        /// <summary>
        /// Đối tượng Timer tương ứng
        /// </summary>
        [MoonSharpHidden]
        public KTTaskManager.KTSchedule RefObject { get; set; }

        /// <summary>
        /// Thiết lập thời gian tồn tại
        /// </summary>
        /// <param name="interval"></param>
        public void SetInterval(float interval)
        {
            this.RefObject.Interval = interval;
        }

        /// <summary>
        /// Thiết lập có lặp đi lặp lại không
        /// </summary>
        /// <param name="loop"></param>
        public void SetLoop(bool loop)
        {
            this.RefObject.Loop = loop;
        }

        /// <summary>
        /// Thiết lập công việc thực thi
        /// </summary>
        /// <param name="func"></param>
        public void SetWork(Closure func)
        {
            func?.Call();
        }

        /// <summary>
        /// Thiết lập tên Timer
        /// </summary>
        /// <param name="name"></param>
        public void SetName(string name)
        {
            this.RefObject.Name = name;
        }

        /// <summary>
        /// Bắt đầu Timer
        /// </summary>
        public void Start()
        {
            KTTaskManager.Instance.AddSchedule(this.RefObject);
        }

        /// <summary>
        /// Ngừng Timer
        /// </summary>
        public void Stop()
        {
            KTTaskManager.Instance.RemoveSchedule(this.RefObject);
        }
    }
}
