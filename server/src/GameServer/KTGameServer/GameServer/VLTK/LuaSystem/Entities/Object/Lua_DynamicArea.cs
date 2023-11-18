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
    /// Đối tượng khu vực động
    /// </summary>
    [MoonSharpUserData]
    public class Lua_DynamicArea
    {
        #region Base for all objects
        /// <summary>
        /// Đối tượng tham chiếu trong hệ thống
        /// </summary>
        [MoonSharpHidden]
        public KDynamicArea RefObject { get; set; }

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
            return (int) ObjectTypes.OT_DYNAMIC_AREA;
        }
        #endregion

        /// <summary>
        /// Trả về Tag của đối tượng
        /// </summary>
        /// <returns></returns>
        public string GetTag()
        {
            return this.RefObject.Tag;
        }
    }
}
