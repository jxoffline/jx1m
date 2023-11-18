using GameServer.KiemThe.Entities;
using GameServer.KiemThe.Logic;
using GameServer.KiemThe.Utilities;
using GameServer.Logic;
using GameServer.Server;
using Server.Data;
using Server.Protocol;
using Server.TCP;
using Server.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tmsk.Contract;
using UnityEngine;

namespace GameServer.KiemThe.Logic
{
    /// <summary>
    /// Quản lý lệnh GM
    /// </summary>
    public static partial class KT_TCPHandler
    {
        #region GM-Command

        /// <summary>
        /// Thực hiện GM-Command
        /// </summary>
        /// <param name="tcpMgr"></param>
        /// <param name="socket"></param>
        /// <param name="tcpClientPool"></param>
        /// <param name="tcpRandKey"></param>
        /// <param name="pool"></param>
        /// <param name="nID"></param>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <param name="tcpOutPacket"></param>
        /// <returns></returns>
        public static TCPProcessCmdResults ResponseGMCommand(TCPManager tcpMgr, TMSKSocket socket, TCPClientPool tcpClientPool, TCPRandKey tcpRandKey, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;
            string command = "";

            try
            {
                command = KTCrypto.Decrypt(data);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Error while getting DATA, CMD={0}, Client={1}", (TCPGameServerCmds) nID, Global.GetSocketRemoteEndPoint(socket)));
                return TCPProcessCmdResults.RESULT_FAILED;
            }

            try
            {
                KPlayer client = KTPlayerManager.Find(socket);
                if (null == client)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Can not find player, CMD={0}, Client={1}", (TCPGameServerCmds) nID, Global.GetSocketRemoteEndPoint(socket)));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// Check xem có phải GM không
                if (!KTGMCommandManager.IsGM(client))
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Trying to use GM Command but not GM, CMD={0}, Client={1}", (TCPGameServerCmds) nID, Global.GetSocketRemoteEndPoint(socket)));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// Thực hiện
                KTGMCommandManager.Process(client, command);

                return TCPProcessCmdResults.RESULT_OK;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, Global.GetDebugHelperInfo(socket), false);
            }

            return TCPProcessCmdResults.RESULT_FAILED;
        }

        #endregion GM-Command

        #region Ban người chơi
        /// <summary>
        /// Thực hiện gửi yêu cầu lên GameDB Ban người chơi tương ứng
        /// </summary>
        /// <param name="banType">Loại Ban - 0: Cấm đăng nhập, 1: Cấm chat, 2: Bỏ cấm đăng nhập, 3: Bỏ cấm chat</param>
        /// <param name="player"></param>
        /// <param name="durationTicks"></param>
        /// <param name="reason"></param>
        /// <param name="bannedBy"></param>
        public static bool SendDBBanPlayer(int banType, KPlayer player, long durationTicks, string reason, string bannedBy)
        {
            /// Toác
            return KT_TCPHandler.SendDBBanPlayer(banType, player.RoleID, player.ServerId, durationTicks, reason, bannedBy);
        }

        /// <summary>
        /// Thực hiện gửi yêu cầu lên GameDB Ban người chơi tương ứng
        /// </summary>
        /// <param name="banType">Loại Ban - 0: Cấm đăng nhập, 1: Cấm chat, 2: Bỏ cấm đăng nhập, 3: Bỏ cấm chat</param>
        /// <param name="player"></param>
        /// <param name="durationTicks"></param>
        /// <param name="reason"></param>
        /// <param name="bannedBy"></param>
        public static bool SendDBBanPlayer(int banType, int roleID, int serverID, long durationTicks, string reason, string bannedBy)
        {
            /// Chuỗi Byte chuyển lên
            byte[] cmdData = DataHelper.ObjectToBytes<KeyValuePair<BanUser, int>>(new KeyValuePair<BanUser, int>(new BanUser()
            {
                RoleID = roleID,
                Duration = durationTicks,
                Reason = reason,
                BannedBy = bannedBy,
            }, banType));
            /// Chuyển lên GameDB
            TCPProcessCmdResults result = Global.ReadDataFromDb((int) TCPGameServerCmds.CMD_DB_BAN_USER, cmdData, cmdData.Length, out byte[] returnBytesData, serverID);
            /// Nếu có kết quả
            if (result == TCPProcessCmdResults.RESULT_DATA)
            {
                int length = BitConverter.ToInt32(returnBytesData, 0);
                string[] strData = new UTF8Encoding().GetString(returnBytesData, 6, length - 2).Split(':');
                /// Kết quả
                int ret = int.Parse(strData[0]);
                /// Trả về kết quả
                return ret == 1;
            }
            /// Toác
            return false;
        }


        /// <summary>
        /// Thực hiện gửi yêu cầu lên GameDB Ban người chơi chức năng tương ứng
        /// </summary>
        /// <param name="banType">Loại Ban - 0: Cấm đăng nhập, 1: Cấm chat, 2: Bỏ cấm đăng nhập, 3: Bỏ cấm chat</param>
        /// <param name="type"></param>
        /// <param name="player"></param>
        /// <param name="durationTicks"></param>
        /// <param name="bannedBy"></param>
        public static bool SendDBBanPlayerByType(int banType, RoleBannedFeature type, KPlayer player, long durationTicks, string bannedBy)
        {
            /// Toác
            return KT_TCPHandler.SendDBBanPlayerByType(banType, type, player.RoleID, player.ServerId, durationTicks, bannedBy);
        }

        /// <summary>
        /// Thực hiện gửi yêu cầu lên GameDB Ban người chơi chức năng tương ứng
        /// </summary>
        /// <param name="banType">Loại Ban - 0: Cấm đăng nhập, 1: Cấm chat, 2: Bỏ cấm đăng nhập, 3: Bỏ cấm chat</param>
        /// <param name="type"></param>
        /// <param name="roleID"></param>
        /// <param name="serverID"></param>
        /// <param name="durationTicks"></param>
        /// <param name="bannedBy"></param>
        public static bool SendDBBanPlayerByType(int banType, RoleBannedFeature type, int roleID, int serverID, long durationTicks, string bannedBy)
        {
            /// Chuỗi Byte chuyển lên
            byte[] cmdData = DataHelper.ObjectToBytes<KeyValuePair<BanUserByType, int>>(new KeyValuePair<BanUserByType, int>(new BanUserByType()
            {
                RoleID = roleID,
                BanType = (int) type,
                Duration = durationTicks,
                BannedBy = bannedBy,
            }, banType));
            /// Chuyển lên GameDB
            TCPProcessCmdResults result = Global.ReadDataFromDb((int) TCPGameServerCmds.CMD_DB_BAN_USER_BY_TYPE, cmdData, cmdData.Length, out byte[] returnBytesData, serverID);
            /// Nếu có kết quả
            if (result == TCPProcessCmdResults.RESULT_DATA)
            {
                int length = BitConverter.ToInt32(returnBytesData, 0);
                string[] strData = new UTF8Encoding().GetString(returnBytesData, 6, length - 2).Split(':');
                /// Kết quả
                int ret = int.Parse(strData[0]);
                /// Trả về kết quả
                return ret == 1;
            }
            /// Toác
            return false;
        }
        #endregion
    }
}
