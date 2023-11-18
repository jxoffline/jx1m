using MoonSharp.Interpreter;
using Server.Data;

namespace GameServer.KiemThe.LuaSystem.Entities
{
    /// <summary>
    /// Thông tin vật phẩm trọn trong NPCDialog hoặc ItemDialog
    /// </summary>
    [MoonSharpUserData]
    public class Lua_ItemSelectionInfo
    {
        /// <summary>
        /// Thông tin vật phẩm chọn tương ứng
        /// </summary>
        [MoonSharpHidden]
        public DialogItemSelectionInfo RefObject { get; set; }

        /// <summary>
        /// Trả về ID vật phẩm
        /// </summary>
        /// <returns></returns>
        public int GetItemID()
        {
            return this.RefObject.ItemID;
        }

        /// <summary>
        /// Trả về số lượng vật phẩm
        /// </summary>
        /// <returns></returns>
        public int GetItemCount()
        {
            return this.RefObject.Quantity;
        }

        /// <summary>
        /// Trả về trạng thái khóa
        /// </summary>
        /// <returns></returns>
        public int GetBinding()
        {
            return this.RefObject.Binding;
        }
    }
}
