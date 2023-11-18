using GameServer.KiemThe.LuaSystem.Entities.Math;
using GameServer.Logic;
using MoonSharp.Interpreter;

namespace GameServer.KiemThe.LuaSystem.Entities
{
    /// <summary>
    /// Đối tượng bất kỳ
    /// </summary>
    [MoonSharpUserData]
    public class Lua_Object
    {
        #region Base for all objects
        /// <summary>
        /// Đối tượng tham chiếu trong hệ thống
        /// </summary>
        [MoonSharpHidden]
        public GameObject RefObject { get; set; }

        /// <summary>
        /// Trả về ID đối tượng
        /// </summary>
        /// <returns></returns>
        public int GetID()
        {
            return this.RefObject.RoleID;
        }

        /// <summary>
        /// Trả về tên đối tượng
        /// </summary>
        /// <returns></returns>
        public string GetName()
        {
            return this.RefObject.RoleName;
        }
        #endregion

        #region Base for all scene-objects
        /// <summary>
        /// Bản đồ hiện tại
        /// </summary>
        [MoonSharpHidden]
        public Lua_Scene CurrentScene { get; set; }

        /// <summary>
        /// Trả về vị trí của đối tượng
        /// </summary>
        /// <returns></returns>
        public Lua_Vector2 GetPos()
        {
            return new Lua_Vector2((int)this.RefObject.CurrentPos.X, (int)this.RefObject.CurrentPos.Y);
        }

        /// <summary>
        /// Trả về bản đồ hiện tại
        /// </summary>
        /// <returns></returns>
        public Lua_Scene GetScene()
        {
            return this.CurrentScene;
        }

        /// <summary>
        /// Trả về danh hiệu hiện tại
        /// </summary>
        /// <returns></returns>
        public string GetTitle()
        {
            return this.RefObject.Title;
        }

        /// <summary>
        /// Kiểm tra đối tượng có phải quái không
        /// </summary>
        /// <returns></returns>
        public bool IsMonster()
        {
            return this.RefObject is Monster;
        }

        /// <summary>
        /// Kiểm tra đối tượng có phải người chơi không
        /// </summary>
        /// <returns></returns>
        public bool IsPlayer()
        {
            return this.RefObject is KPlayer;
        }

        /// <summary>
        /// Chuyển đối tượng về quái
        /// </summary>
        /// <returns></returns>
        public Lua_Monster AsMonster()
        {
            if (this.RefObject is Monster)
            {
                return new Lua_Monster()
                {
                    RefObject = this.RefObject as Monster,
                    CurrentScene = this.CurrentScene,
                };
            }
            return null;
        }

        /// <summary>
        /// Chuyển đối tượng về người chơi
        /// </summary>
        /// <returns></returns>
        public Lua_Player AsPlayer()
        {
            if (this.RefObject is KPlayer)
            {
                return new Lua_Player()
                {
                    RefObject = this.RefObject as KPlayer,
                    CurrentScene = this.CurrentScene,
                };
            }
            return null;
        }
        #endregion
    }
}
