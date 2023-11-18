using GameServer.KiemThe.Logic;
using GameServer.Logic;
using MoonSharp.Interpreter;
using Server.Data;
using Server.Tools;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GameServer.KiemThe.LuaSystem.Entities
{
    /// <summary>
    /// Đối tượng khung cửa sổ hội thoại của vật phẩm
    /// </summary>
    [MoonSharpUserData]
    public class Lua_ItemDialog
    {
        /// <summary>
        /// Đối tượng tương ứng
        /// </summary>
        public KItemDialog RefObject { get; set; }

        /// <summary>
        /// Đối tượng khung cửa sổ hội thoại của vật phẩm
        /// </summary>
        [MoonSharpHidden]
        public Lua_ItemDialog()
        {
            this.RefObject = new KItemDialog();
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
        /// Thiết lập Text danh sách vật phẩm
        /// </summary>
        /// <param name="text"></param>
        public void SetItemHeader(string text)
        {
            this.RefObject.ItemHeaderString = text;
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
        /// Thêm sự lựa chọn
        /// <para>Chỉ dùng cho LUA</para>
        /// </summary>
        /// <param name="id"></param>
        /// <param name="text"></param>
        public void AddSelection(int id, string text)
        {
            if (id < 0)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Lua error on ItemDialog:AddSelection, ID must be greater than zero"));
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
                LogManager.WriteLog(LogTypes.Error, string.Format("Lua error on ItemDialog:AddParam, ID must be greater than zero"));
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
        /// <param name="item"></param>
        /// <param name="player"></param>
        public void Show(Lua_Item item, Lua_Player player)
        {
            if (player == null || player.RefObject == null)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Lua error on ItemDialog:Show, Player is NULL."));
                return;
            }

            if (item == null || item.RefObject == null)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Lua error on ItemDialog:Show, Item is NULL."));
                return;
            }
            /// Gửi gói tin về Client
            this.RefObject.Show(item.RefObject, player.RefObject);
        }
    }
}
