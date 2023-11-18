using GameServer.KiemThe.Core;
using GameServer.KiemThe.LuaSystem.Entities.Math;
using GameServer.Logic;
using MoonSharp.Interpreter;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GameServer.KiemThe.LuaSystem.Entities
{
    /// <summary>
    /// Đối tượng NPC
    /// </summary>
    [MoonSharpUserData]
    public class Lua_GrowPoint
    {
        #region Base for all objects
        /// <summary>
        /// Đối tượng tham chiếu trong hệ thống
        /// </summary>
        [MoonSharpHidden]
        public GrowPoint RefObject { get; set; }

        /// <summary>
        /// Trả về ID đối tượng
        /// </summary>
        /// <returns></returns>
        public int GetID()
        {
            return this.RefObject.ID;
        }

        /// <summary>
        /// Trả về tên đối tượng
        /// </summary>
        /// <returns></returns>
        public string GetName()
        {
            return this.RefObject.Name;
        }
        #endregion

        #region Base for all scene-objects
        /// <summary>
        /// Bản đồ hiện tại
        /// </summary>
        [MoonSharpHidden]
        public Lua_Scene CurrentScene { get; set; }

        /// <summary>
        /// Trả về tọa độ đối tượng
        /// </summary>
        /// <returns></returns>
        public Lua_Vector2 GetPos()
        {
            return new Lua_Vector2((float) this.RefObject.CurrentGrid.X, (float) this.RefObject.CurrentGrid.Y);
        }

        /// <summary>
        /// Trả về loại đối tượng
        /// </summary>
        /// <returns></returns>
        public int GetObjectType()
        {
            return (int) ObjectTypes.OT_GROWPOINT;
        }
        #endregion

        /// <summary>
        /// Trả về ID Res trong File cấu hình
        /// </summary>
        /// <returns></returns>
        public int GetResID()
        {
            return this.RefObject.Data.ResID;
        }
    }
}
