using FS.GameEngine.Logic;
using FS.GameEngine.Network;
using FS.VLTK.Logic;
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
        #region Bảng hồi sinh
        /// <summary>
        /// Nhận yêu cầu hiện khung nhân vật hồi sinh
        /// </summary>
        /// <param name="cmdID"></param>
        /// <param name="bytes"></param>
        /// <param name="length"></param>
        public static void ReceiveShowReviveFrame(int cmdID, byte[] bytes, int length)
        {
            /// Hủy tự đánh
            KTAutoFightManager.Instance.StopAutoFight();
            /// Ngừng tự làm nhiệm vụ
            AutoQuest.Instance.StopAutoQuest();
            /// Ngừng tự tìm đường
            AutoPathManager.Instance.StopAutoPath();
            /// Hủy dòng chữ tự tìm đường
            PlayZone.Instance.HideTextAutoFindPath();

            G2C_ShowReviveFrame showReviveFrame = DataHelper.BytesToObject<G2C_ShowReviveFrame>(bytes, 0, length);
            PlayZone.Instance.ShowReviveFrame(showReviveFrame.Message, showReviveFrame.AllowReviveAtPos);
        }

        /// <summary>
        /// Gửi gói tin về Server thông báo Client lựa chọn hồi sinh
        /// </summary>
        /// <param name="reviveMethodID"></param>
        public static void ClientRevive(int reviveMethodID)
        {
            C2G_ClientRevive data = new C2G_ClientRevive()
            {
                SelectedID = reviveMethodID,
            };
            byte[] bytes = DataHelper.ObjectToBytes<C2G_ClientRevive>(data);
            GameInstance.Game.GameClient.SendData(TCPOutPacket.MakeTCPOutPacket(GameInstance.Game.GameClient.OutPacketPool, bytes, 0, bytes.Length, (int) (TCPGameServerCmds.CMD_KT_C2G_CLIENTREVIVE)));
        }
        #endregion
    }
}
