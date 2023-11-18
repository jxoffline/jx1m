using FS.GameEngine.Logic;
using FS.GameEngine.Network;
using HSGameEngine.GameEngine.Network.Protocol;
using Server.Data;
using Server.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FS.VLTK.Network
{
    /// <summary>
    /// Quản lý tương tác với Socket
    /// </summary>
    public static partial class KT_TCPHandler
    {
        #region Tìm người chơi
        /// <summary>
        /// Gửi yêu cầu tìm kiếm người chơi
        /// </summary>
        /// <param name="playerName"></param>
        public static void SendBrowsePlayer(string playerName)
        {
            string strCmd = string.Format("{0}", playerName);
            byte[] bytes = new ASCIIEncoding().GetBytes(strCmd);
            GameInstance.Game.GameClient.SendData(TCPOutPacket.MakeTCPOutPacket(GameInstance.Game.GameClient.OutPacketPool, bytes, 0, bytes.Length, (int) (TCPGameServerCmds.CMD_KT_BROWSE_PLAYER)));
        }

        /// <summary>
        /// Nhận gói tin thông báo danh sách người chơi tìm kiếm
        /// </summary>
        /// <param name="cmdID"></param>
        /// <param name="bytes"></param>
        /// <param name="length"></param>
        public static void ReceiveBrowsePlayer(int cmdID, byte[] bytes, int length)
        {
            try
            {
                List<RoleDataMini> roleDatas = DataHelper.BytesToObject<List<RoleDataMini>>(bytes, 0, length);

                /// Nếu chưa hiện khung
                if (PlayZone.Instance.UIBrowsePlayer == null)
                {
                    PlayZone.Instance.ShowUIBrowsePlayer();
                }
                PlayZone.Instance.UIBrowsePlayer.Players = roleDatas;
            }
            catch (Exception) { }
        }

        /// <summary>
        /// Gửi yêu cầu kiểm tra vị trí người chơi
        /// </summary>
        /// <param name="roleID"></param>
        public static void SendCheckPlayerLocation(int roleID)
        {
            string strCmd = string.Format("{0}", roleID);
            byte[] bytes = new ASCIIEncoding().GetBytes(strCmd);
            GameInstance.Game.GameClient.SendData(TCPOutPacket.MakeTCPOutPacket(GameInstance.Game.GameClient.OutPacketPool, bytes, 0, bytes.Length, (int) (TCPGameServerCmds.CMD_KT_CHECK_PLAYER_LOCATION)));
        }

        /// <summary>
        /// Nhận gói tin thông kiểm tra vị trí người chơi
        /// </summary>
        /// <param name="cmdID"></param>
        /// <param name="bytes"></param>
        /// <param name="length"></param>
        public static void ReceiveCheckPlayerLocation(int cmdID, byte[] bytes, int length)
        {
            try
            {
                RoleDataMini roleData = DataHelper.BytesToObject<RoleDataMini>(bytes, 0, length);

                /// Nếu đang hiển thị khung
                if (PlayZone.Instance.UIBrowsePlayer != null)
                {
                    PlayZone.Instance.UIBrowsePlayer.ShowPlayerLocationReport(roleData);
                }
            }
            catch (Exception) { }
        }

        /// <summary>
        /// Gửi yêu cầu kiểm tra thông tin người chơi
        /// </summary>
        /// <param name="roleID"></param>
        public static void SendCheckPlayerInfo(int roleID)
        {
            string strCmd = string.Format("{0}", roleID);
            byte[] bytes = new ASCIIEncoding().GetBytes(strCmd);
            GameInstance.Game.GameClient.SendData(TCPOutPacket.MakeTCPOutPacket(GameInstance.Game.GameClient.OutPacketPool, bytes, 0, bytes.Length, (int) (TCPGameServerCmds.CMD_KT_GET_PLAYER_INFO)));
        }

        /// <summary>
        /// Nhận gói tin thông kiểm tra thông tin người chơi
        /// </summary>
        /// <param name="cmdID"></param>
        /// <param name="bytes"></param>
        /// <param name="length"></param>
        public static void ReceiveCheckPlayerInfo(int cmdID, byte[] bytes, int length)
        {
            try
            {
                RoleDataMini roleData = DataHelper.BytesToObject<RoleDataMini>(bytes, 0, length);
                PlayZone.Instance.OpenUIPlayerInfo(roleData);
            }
            catch (Exception) { }
        }
        #endregion

        #region Soi thông tin người chơi khác
        /// <summary>
        /// Gửi yêu cầu kiểm tra thông tin trang bị của người chơi khác
        /// </summary>
        /// <param name="roleID"></param>
        public static void RequestGetOtherPlayerEquipInfo(int roleID)
        {
            string strCmd = string.Format("{0}", roleID);
            byte[] bytes = new ASCIIEncoding().GetBytes(strCmd);
            GameInstance.Game.GameClient.SendData(TCPOutPacket.MakeTCPOutPacket(GameInstance.Game.GameClient.OutPacketPool, bytes, 0, bytes.Length, (int) (TCPGameServerCmds.CMD_OTHER_ROLE_DATA)));
        }

        /// <summary>
        /// Nhận gói tin thông tin trang bị người chơi khác
        /// </summary>
        /// <param name="cmdID"></param>
        /// <param name="bytes"></param>
        /// <param name="length"></param>
        public static void ReceiveGetOtherPlayerEquipInfo(int cmdID, byte[] bytes, int length)
        {
            try
            {
                RoleData roleData = DataHelper.BytesToObject<RoleData>(bytes, 0, length);
                /// Nếu không có thông tin
                if (roleData == null)
                {
                    KTGlobal.AddNotification("Kiểm tra thông tin người chơi thất bại, hãy thử lại sau giây lát!");
                    return;
                }

                /// Nếu chưa hiện khung thông tin người chơi khác
                if (PlayZone.Instance.UIOtherRoleInfo == null)
                {
                    PlayZone.Instance.OpenUIOtherRoleInfo(roleData);
                }
            }
            catch (Exception) { }
        }
        #endregion

        #region Bảng xếp hạng
        /// <summary>
        /// Gửi yêu cầu truy vấn thông tin bảng xếp hạng
        /// </summary>
        /// <param name="type"></param>
        /// <param name="pageID"></param>
        public static void SendQueryPlayerRanking(int type, int pageID)
        {
            string strCmd = string.Format("{0}:{1}:{2}", Global.Data.RoleData.RoleID, type, pageID);
            byte[] bytes = new ASCIIEncoding().GetBytes(strCmd);
            GameInstance.Game.GameClient.SendData(TCPOutPacket.MakeTCPOutPacket(GameInstance.Game.GameClient.OutPacketPool, bytes, 0, bytes.Length, (int) (TCPGameServerCmds.CMD_KT_QUERY_PLAYERRANKING)));
        }

        /// <summary>
        /// Nhận gói tin thông tin xếp hạng
        /// </summary>
        /// <param name="cmdID"></param>
        /// <param name="bytes"></param>
        /// <param name="length"></param>
        public static void ReceivePlayerRanking(int cmdID, byte[] bytes, int length)
        {
            Ranking ranking = DataHelper.BytesToObject<Ranking>(bytes, 0, length);
            if (ranking == null)
            {
                return;
            }

            /// Nếu đang mở khung bảng xếp hạng
            if (PlayZone.Instance.UIRanking != null)
            {
                KTGlobal.HideLoadingFrame();

                PlayZone.Instance.UIRanking.Data = ranking;
                PlayZone.Instance.UIRanking.RefreshData();
            }
        }
        #endregion
    }
}
