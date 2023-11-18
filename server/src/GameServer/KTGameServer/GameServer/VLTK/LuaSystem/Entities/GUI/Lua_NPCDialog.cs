using GameServer.KiemThe.Logic;
using MoonSharp.Interpreter;
using Server.Data;
using Server.Tools;
using System;

namespace GameServer.KiemThe.LuaSystem.Entities
{
    /// <summary>
    /// Đối tượng khung cửa sổ hội thoại của NPC
    /// </summary>
    [MoonSharpUserData]
    public class Lua_NPCDialog
    {
        /// <summary>
        /// Đối tượng tương ứng
        /// </summary>
        public KNPCDialog RefObject { get; set; }

        /// <summary>
        /// Tạo mới đối tượng khung cửa sổ hội thoại của NPC
        /// </summary>
        public Lua_NPCDialog()
        {
            this.RefObject = new KNPCDialog();
        }

        /// <summary>
        /// Thiết lập đánh dấu vật phẩm có được lựa chọn hay không
        /// </summary>
        /// <param name="isSelectable"></param>
        public void SetItemSelectable(bool isSelectable)
        {
            this.RefObject.ItemSelectable = isSelectable;
        }

        /// <summary>
        /// Thêm text vào nội dung
        /// </summary>
        /// <param name="text"></param>
        public void AddText(string text)
        {
            this.RefObject.Text += text + "\n";
        }

        /// <summary>
        /// Thiết lập Text danh sách vật phẩm
        /// </summary>
        /// <param name="text"></param>
        public void SetItemHeader(string text)
        {
            this.RefObject.ItemHeaderString = text;
        }

        /// <summary>
        /// Thêm sự lựa chọn
        /// <para>Chỉ dùng cho LUA</para>
        /// </summary>
        /// <param name="id"></param>
        /// <param name="text"></param>
        public void AddSelection(int id, string text)
        {
            if (id <= 0)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Lua error on NPCDialog:AddSelection, ID must be greater than zero"));
                return;
            }
            this.RefObject.Selections[-id] = text;
        }

        /// <summary>
        /// Thêm tham biến khác vào
        /// <para>Chỉ dùng cho LUA</para>
        /// </summary>
        /// <param name="id"></param>
        /// <param name="value"></param>
        public void AddParam(int id, string value)
        {
            if (id <= 0)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Lua error on NPCDialog:AddParam, ID must be greater than zero"));
                return;
            }
            this.RefObject.OtherParams[-id] = value;
        }

        /// <summary>
        /// Thêm vật phẩm vào danh sách
        /// </summary>
        /// <param name="itemID"></param>
        /// <param name="itemNumber"></param>
        /// <param name="binding"></param>
        public void AddItem(int itemID, int itemNumber, int binding)
        {
            this.RefObject.Items.Add(new DialogItemSelectionInfo()
            {
                ItemID = itemID,
                Quantity = itemNumber,
                Binding = binding,
            });
        }

        /// <summary>
        /// Gửi yêu cầu về Client để hiện khung
        /// </summary>
        /// <param name="npc"></param>
        /// <param name="player"></param>
        public void Show(Lua_NPC npc, Lua_Player player)
        {
            if (player == null || player.RefObject == null)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Lua error on NPCDialog:Show, Player is NULL."));
                return;
            }

            if (npc == null || npc.RefObject == null)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Lua error on NPCDialog:Show, NPC is NULL."));
                return;
            }

            /// Gửi yêu cầu hiển thị khung
            this.RefObject.Show(npc.RefObject, player.RefObject);
        }
    }
}
