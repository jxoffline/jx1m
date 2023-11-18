using FS.GameEngine.Logic;
using FS.GameEngine.Network;
using HSGameEngine.GameEngine.Network.Protocol;
using Server.Data;
using Server.Tools;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FS.VLTK.Network
{
    /// <summary>
    /// Quản lý tương tác với Socket
    /// </summary>
    public static partial class KT_TCPHandler
    {
        #region NPCDialog, ItemDialog
        /// <summary>
        /// Nhận dữ liệu mở khung hội thoại NPC Dialog
        /// </summary>
        /// <param name="cmdID"></param>
        /// <param name="bytes"></param>
        /// <param name="length"></param>
        public static void ReceiveOpenNPCDialog(int cmdID, byte[] bytes, int length)
        {
            if (!Global.Data.PlayGame)
            {
                return;
            }

            G2C_LuaNPCDialog result = DataHelper.BytesToObject<G2C_LuaNPCDialog>(bytes, 0, length);
            if (result == null)
            {
                return;
            }

            PlayZone.Instance.ShowNPCLuaDialog(result.NPCID, result.ID, result.Text, result.Selections, result.Items, result.ItemSelectable, result.ItemHeaderString, result.OtherParams);
        }

        /// <summary>
        /// Gửi phản hồi người chơi ấn vào lựa chọn khung NPC Dialog
        /// </summary>
        /// <param name="npcID"></param>
        /// <param name="dialogID"></param>
        /// <param name="selectItemInfo"></param>
        /// <param name="itemID"></param>
        public static void SendNPCSelection(int npcID, int dialogID, int selectionID, DialogItemSelectionInfo selectItemInfo, Dictionary<int, string> otherParams)
        {
            if (!Global.Data.PlayGame)
            {
                return;
            }

            C2G_LuaNPCDialog data = new C2G_LuaNPCDialog()
            {
                ID = dialogID,
                NPCID = npcID,
                SelectionID = selectionID,
                SelectedItem = selectItemInfo,
                OtherParams = otherParams,
            };
            byte[] bytes = DataHelper.ObjectToBytes<C2G_LuaNPCDialog>(data);
            GameInstance.Game.GameClient.SendData(TCPOutPacket.MakeTCPOutPacket(GameInstance.Game.GameClient.OutPacketPool, bytes, 0, bytes.Length, (int) (TCPGameServerCmds.CMD_KT_C2G_NPCDIALOG)));
        }

        /// <summary>
        /// Nhận dữ liệu mở khung hội thoại Item Dialog
        /// </summary>
        /// <param name="cmdID"></param>
        /// <param name="bytes"></param>
        /// <param name="length"></param>
        public static void ReceiveOpenItemDialog(int cmdID, byte[] bytes, int length)
        {
            if (!Global.Data.PlayGame)
            {
                return;
            }

            G2C_LuaItemDialog result = DataHelper.BytesToObject<G2C_LuaItemDialog>(bytes, 0, length);
            if (result == null)
            {
                return;
            }

            PlayZone.Instance.ShowNPCLuaDialog(result.ItemID, result.DbID, result.ID, result.Text, result.Selections, result.Items, result.ItemSelectable, result.ItemHeaderString, result.OtherParams);
        }

        /// <summary>
        /// Gửi phản hồi người chơi ấn vào lựa chọn khung Item Dialog
        /// </summary>
        /// <param name="itemDbID"></param>
        /// <param name="itemID"></param>
        /// <param name="selectItemInfo"></param>
        /// <param name="itemID"></param>
        public static void SendItemSelection(int itemDbID, int itemID, int dialogID, int selectionID, DialogItemSelectionInfo selectItemInfo, Dictionary<int, string> otherParams)
        {
            if (!Global.Data.PlayGame)
            {
                return;
            }

            C2G_LuaItemDialog data = new C2G_LuaItemDialog()
            {
                ID = dialogID,
                ItemID = itemID,
                DbID = itemDbID,
                SelectionID = selectionID,
                SelectedItem = selectItemInfo,
                OtherParams = otherParams,
            };
            byte[] bytes = DataHelper.ObjectToBytes<C2G_LuaItemDialog>(data);
            GameInstance.Game.GameClient.SendData(TCPOutPacket.MakeTCPOutPacket(GameInstance.Game.GameClient.OutPacketPool, bytes, 0, bytes.Length, (int) (TCPGameServerCmds.CMD_KT_C2G_ITEMDIALOG)));
        }

        /// <summary>
        /// Nhận yêu cầu đóng bảng thoại NPCDialog hoặc ItemDialog từ Server
        /// </summary>
        public static void ReceiveCloseDialog()
        {
            if (!Global.Data.PlayGame)
            {
                return;
            }

            PlayZone.Instance.CloseNPCDialog();
        }
        #endregion

        #region NPC Click
        /// <summary>
        /// Gửi gói tin lên Server, thực hiện Click vào NPC
        /// </summary>
        /// <param name="npcID"></param>
        public static void NPCClick(int npcID)
        {
            if (!Global.Data.PlayGame)
            {
                return;
            }

            string strcmd = "";
            strcmd = npcID.ToString();

            GameInstance.Game.GameClient.SendData(TCPOutPacket.MakeTCPOutPacket(GameInstance.Game.GameClient.OutPacketPool, strcmd, (int) (TCPGameServerCmds.CMD_KT_CLICKON_NPC)));
        }
        #endregion

        #region GrowPoint Click
        /// <summary>
        /// Gửi gói tin lên Server, thực hiện Click vào điểm thu thập
        /// </summary>
        /// <param name="id"></param>
        public static void GrowPointClick(int id)
        {
            if (!Global.Data.PlayGame)
            {
                return;
            }

            string strcmd = "";
            strcmd = id.ToString();

            GameInstance.Game.GameClient.SendData(TCPOutPacket.MakeTCPOutPacket(GameInstance.Game.GameClient.OutPacketPool, strcmd, (int) (TCPGameServerCmds.CMD_KT_C2G_GROWPOINT_CLICK)));
        }
        #endregion

        #region Lưu thiết lập vật phẩm dùng trong khay dùng nhanh
        /// <summary>
        /// Lưu thiết lập vật phẩm dùng trong khay dùng nhanh
        /// </summary>
        public static void SendSaveQuickItems()
        {
            string strcmd = Global.Data.RoleData.QuickItems;
            GameInstance.Game.GameClient.SendData(TCPOutPacket.MakeTCPOutPacket(GameInstance.Game.GameClient.OutPacketPool, strcmd, (int) (TCPGameServerCmds.CMD_KT_C2G_SET_QUICK_ITEMS)));
        }
        #endregion
    }
}
