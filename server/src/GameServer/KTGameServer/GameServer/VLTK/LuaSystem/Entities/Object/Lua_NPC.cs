using GameServer.KiemThe.LuaSystem.Entities.Math;
using GameServer.Logic;
using MoonSharp.Interpreter;

namespace GameServer.KiemThe.LuaSystem.Entities
{
    /// <summary>
    /// Đối tượng NPC
    /// </summary>
    [MoonSharpUserData]
    public class Lua_NPC
    {
        #region Base for all objects
        /// <summary>
        /// Đối tượng tham chiếu trong hệ thống
        /// </summary>
        [MoonSharpHidden]
        public NPC RefObject { get; set; }

        /// <summary>
        /// Trả về ID đối tượng
        /// </summary>
        /// <returns></returns>
        public int GetID()
        {
            return this.RefObject.NPCID;
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
        /// Trả về bản đồ hiện tại
        /// </summary>
        /// <returns></returns>
        public Lua_Scene GetScene()
        {
            return this.CurrentScene;
        }

        /// <summary>
        /// Trả về tọa độ đối tượng
        /// </summary>
        /// <returns></returns>
        public Lua_Vector2 GetPos()
        {
            return new Lua_Vector2((float)this.RefObject.CurrentPos.X, (float)this.RefObject.CurrentPos.Y);
        }

        /// <summary>
        /// Trả về danh hiệu đối tượng
        /// </summary>
        /// <returns></returns>
        public string GetTitle()
        {
            return this.RefObject.Title;
        }

        /// <summary>
        /// Trả về loại đối tượng
        /// </summary>
        /// <returns></returns>
        public int GetObjectType()
        {
            return (int) ObjectTypes.OT_NPC;
        }
        #endregion
    }
}
