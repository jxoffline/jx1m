using GameServer.KiemThe.Logic;
using MoonSharp.Interpreter;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GameServer.KiemThe.LuaSystem.Entities
{
    /// <summary>
    /// Đối tượng sự kện
    /// </summary>
    [MoonSharpUserData]
    public class Lua_Activity
    {
        /// <summary>
        /// Tham chiếu đối tượng tương ứng
        /// </summary>
        [MoonSharpHidden]
        public KTActivity RefObject { get; set; }

        /// <summary>
        /// Trả về ID sự kện
        /// </summary>
        /// <returns></returns>
        public int GetID()
        {
            return this.RefObject.Data.ID;
        }

        /// <summary>
        /// Trả về tên sự kiện
        /// </summary>
        /// <returns></returns>
        public string GetName()
        {
            return this.RefObject.Data.Name;
        }

        /// <summary>
        /// Hoạt động còn tồn tại không
        /// </summary>
        /// <returns></returns>
        public bool IsAlive()
        {
            return this.RefObject.IsStarted && !this.RefObject.IsOver;
        }

        /// <summary>
        /// Trả về thời gian tồn tại của hoạt động
        /// </summary>
        /// <returns></returns>
        public long GetLifeTime()
        {
            return this.RefObject.LifeTime;
        }

        /// <summary>
        /// Trả về tham biến nội bộ sự kiện tại vị trí tương ứng
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public long GetLocalVariable(int index)
        {
            return this.RefObject.GetParam(index);
        }

        /// <summary>
        /// Thiết lập giá trị cho tham biến nội bộ sự kiện tại vị trí tương ứng
        /// </summary>
        /// <param name="index"></param>
        /// <param name="value"></param>
        public void SetLocalVariable(int index, long value)
        {
            this.RefObject.SetParam(index, value);
        }

        /// <summary>
        /// Trả về thời gian duy trì hoạt động
        /// </summary>
        /// <returns></returns>
        public long GetDuration()
        {
            return this.RefObject.Data.DurationTicks;
        }
    }
}
