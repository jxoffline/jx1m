using GameServer.Core.Executor;
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
using System.Threading.Tasks;

namespace GameServer.KiemThe.Logic
{
    /// <summary>
    /// Quản lý bạn bè
    /// </summary>
    public static partial class KT_TCPHandler
    {
        /// <summary>
        /// Lấy danh sách bạn bè
        /// </summary>
        /// <param name="pool"></param>
        /// <param name="nID"></param>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static TCPProcessCmdResults ProcessSpriteGetFriendsCmd(TCPManager tcpMgr, TMSKSocket socket, TCPClientPool tcpClientPool, TCPRandKey tcpRandKey, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;

            try
            {
                string cmdData = new UTF8Encoding().GetString(data, 0, count);
                string[] fields = cmdData.Split(':');
                if (fields.Length < 1)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Error Socket params count not fit CMD={0}, Recv={1}, CmdData={2}",
                        (TCPGameServerCmds) nID, fields.Length, cmdData));

                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int) TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                int roleID = Global.SafeConvertToInt32(fields[0]);

                KPlayer client = KTPlayerManager.Find(socket);
                if (null == client || client.RoleID != roleID)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Can not find player corresponding ID, CMD={0}, Client={1}", (TCPGameServerCmds) nID, Global.GetSocketRemoteEndPoint(socket)));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// Truy vấn lại trên DB để lấy danh sách bạn bè
                TCPProcessCmdResults ret = Global.TransferRequestToDBServer(tcpMgr, socket, tcpClientPool, tcpRandKey, pool, nID, data, count, out tcpOutPacket, client.ServerId);
                /// Nếu có kết quả
                if (ret == TCPProcessCmdResults.RESULT_DATA)
                {
                    List<FriendData> friendData = DataHelper.BytesToObject<List<FriendData>>(tcpOutPacket.GetPacketBytes(), 6, tcpOutPacket.PacketDataSize - 6);
                    /// Lưu thông tin bạn bè vào
                    client.FriendDataList = friendData;
                }
                else
                {
                    return ret;
                }

                /// Tạo mới danh sách bạn bè tạm thời
                List<FriendData> friendList = new List<FriendData>(client.FriendDataList);
                /// Duyệt danh sách bạn bè
                foreach (FriendData friend in friendList)
                {
                    /// Đối tượng người chơi tương ứng
                    KPlayer friendPlayer = KTPlayerManager.Find(friend.OtherRoleID);
                    /// Nếu tìm thấy người chơi tức là đang online
                    if (friendPlayer != null)
                    {
                        friend.OnlineState = 1;
                        friend.MapCode = friendPlayer.CurrentMapCode;
                        /// Dấu tọa độ không cho biết
                        friend.PosX = 0;
                        friend.PosY = 0;
                    }
                    else
                    {
                        friend.OnlineState = 0;
                        friend.MapCode = -1;
                        /// Dấu tọa độ không cho biết
                        friend.PosX = 0;
                        friend.PosY = 0;
                    }
                }
                /// Sắp xếp ưu tiên Online ở trên
                client.FriendDataList = friendList.OrderByDescending(x => x.OnlineState).ToList();

                /// Gửi gói tin lại cho người chơi
                byte[] _cmdData = DataHelper.ObjectToBytes<List<FriendData>>(friendList);
                client.SendPacket((int) TCPGameServerCmds.CMD_SPR_GETFRIENDS, _cmdData);

                return TCPProcessCmdResults.RESULT_OK;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, Global.GetDebugHelperInfo(socket), false);
            }

            return TCPProcessCmdResults.RESULT_FAILED;
        }

        /// <summary>
        /// Thêm bạn
        /// </summary>
        /// <param name="pool"></param>
        /// <param name="nID"></param>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static TCPProcessCmdResults ProcessSpriteAddFriendCmd(TCPManager tcpMgr, TMSKSocket socket, TCPClientPool tcpClientPool, TCPRandKey tcpRandKey, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;
            string cmdData = null;

            try
            {
                cmdData = new UTF8Encoding().GetString(data, 0, count);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Decode data faild, CMD={0}, Client={1}", (TCPGameServerCmds) nID, Global.GetSocketRemoteEndPoint(socket)));
                return TCPProcessCmdResults.RESULT_FAILED;
            }

            try
            {
                string[] fields = cmdData.Split(':');
                if (fields.Length != 4)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Error Socket params count not fit CMD={0}, Client={1}, Recv={2}", (TCPGameServerCmds) nID, Global.GetSocketRemoteEndPoint(socket), fields.Length));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                int dbID = Convert.ToInt32(fields[0]);
                int roleID = Convert.ToInt32(fields[1]);
                string otherName = fields[2];
                int friendType = Convert.ToInt32(fields[3]);

                /// Xóa tiền tố GM nếu có

                KPlayer client = KTPlayerManager.Find(socket);
                if (null == client)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Can not find player corresponding ID, CMD={0}, Client={1}", (TCPGameServerCmds) nID, Global.GetSocketRemoteEndPoint(socket)));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// Nếu trùng với bản thân
                if (roleID == client.RoleID)
                {
                    KTPlayerManager.ShowNotification(client, "Không thể thêm bạn với chính mình!");
                    return TCPProcessCmdResults.RESULT_OK;
                }

                if (friendType < 0 || friendType > 2)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Socket params faild, CMD={0}, Client={1}, Recv={2}", (TCPGameServerCmds) nID, Global.GetSocketRemoteEndPoint(socket), fields.Length));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// Giới hạn Gửi yêu cầu kết bạn liên tục
                if (KTGlobal.GetCurrentTimeMilis() - client.LastAddFriendTicks[friendType] < 1000)
                {
                    KTPlayerManager.ShowNotification(client, "Thao tác quá nhanh, hãy đợi giây lát!");
                    return TCPProcessCmdResults.RESULT_OK;
                }
                client.LastAddFriendTicks[friendType] = KTGlobal.GetCurrentTimeMilis();

                /// Nếu là yêu cầu thêm bạn
                if (friendType == 0)
                {
                    /// Người chơi gửi yêu cầu
                    KPlayer player = KTPlayerManager.Find(otherName);
                    /// Nếu người chơi không tồn tại
                    if (player == null)
                    {
                        KTPlayerManager.ShowNotification(client, "Người chơi không tồn tại hoặc đã rời mạng!");
                        return TCPProcessCmdResults.RESULT_OK;
                    }

                    /// Nếu không có yêu cầu từ người chơi tương ứng
                    if (!player.IsAskingToBeFriendWith(client))
                    {
                        KTPlayerManager.ShowNotification(client, "Người chơi này không gửi yêu cầu kết bạn tới bạn!");
                        return TCPProcessCmdResults.RESULT_OK;
                    }

                    /// Xóa người chơi khỏi danh sách yêu cầu thêm bạn
                    player.RemoveAskingToBeFriend(client);

                    /// Thêm bạn
                    KT_TCPHandler.AddFriend(tcpMgr, tcpClientPool, pool, client, dbID, -1, otherName, friendType);
                }
                /// Nếu không phải thêm bạn thì chỉ xử lý 1 bên
                else
                {
                    KT_TCPHandler.AddFriend(tcpMgr, tcpClientPool, pool, client, dbID, -1, otherName, friendType);
                }

                return TCPProcessCmdResults.RESULT_OK;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, Global.GetDebugHelperInfo(socket), false);
            }

            return TCPProcessCmdResults.RESULT_FAILED;
        }

        /// <summary>
        /// Xóa bạn
        /// </summary>
        /// <param name="pool"></param>
        /// <param name="nID"></param>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static TCPProcessCmdResults ProcessSpriteRemoveFriendCmd(TCPManager tcpMgr, TMSKSocket socket, TCPClientPool tcpClientPool, TCPRandKey tcpRandKey, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;
            string cmdData = null;

            try
            {
                cmdData = new UTF8Encoding().GetString(data, 0, count);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Decode data faild, CMD={0}, Client={1}", (TCPGameServerCmds) nID, Global.GetSocketRemoteEndPoint(socket)));
                return TCPProcessCmdResults.RESULT_FAILED;
            }

            try
            {
                string[] fields = cmdData.Split(':');
                if (fields.Length != 2)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Error Socket params count not fit CMD={0}, Client={1}, Recv={2}",
                        (TCPGameServerCmds) nID, Global.GetSocketRemoteEndPoint(socket), fields.Length));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                int dbID = Convert.ToInt32(fields[0]);
                int roleID = Convert.ToInt32(fields[1]);

                KPlayer client = KTPlayerManager.Find(socket);
                if (null == client || client.RoleID != roleID)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Can not find player corresponding ID, CMD={0}, Client={1}", (TCPGameServerCmds) nID, Global.GetSocketRemoteEndPoint(socket)));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// Thực hiện xóa bạn
                KT_TCPHandler.RemoveFriend(tcpMgr, tcpClientPool, pool, client, dbID);

                return TCPProcessCmdResults.RESULT_OK;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, Global.GetDebugHelperInfo(socket), false);
            }

            return TCPProcessCmdResults.RESULT_FAILED;
        }

        /// <summary>
        /// Phản hồi yêu cầu gửi lời mời thêm bạn
        /// </summary>
        /// <param name="pool"></param>
        /// <param name="nID"></param>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static TCPProcessCmdResults ProcessSpriteAskFriend(TCPManager tcpMgr, TMSKSocket socket, TCPClientPool tcpClientPool, TCPRandKey tcpRandKey, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;

            try
            {
                string cmdData = new UTF8Encoding().GetString(data, 0, count);
                string[] fields = cmdData.Split(':');
                if (fields.Length < 1)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Error Socket params count not fit CMD={0}, Recv={1}, CmdData={2}",
                        (TCPGameServerCmds) nID, fields.Length, cmdData));

                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int) TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                /// ID người chơi muốn hỏi
                int roleID = Global.SafeConvertToInt32(fields[0]);

                KPlayer client = KTPlayerManager.Find(socket);
                if (null == client)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Can not find player corresponding ID, CMD={0}, Client={1}", (TCPGameServerCmds) nID, Global.GetSocketRemoteEndPoint(socket)));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// Nếu trùng với bản thân
                if (roleID == client.RoleID)
                {
                    KTPlayerManager.ShowNotification(client, "Không thể thêm bạn với chính mình!");
                    return TCPProcessCmdResults.RESULT_OK;
                }

                /// Người chơi tương ứng
                KPlayer player = KTPlayerManager.Find(roleID);
                /// Nếu không tìm thấy người chơi
                if (player == null)
                {
                    KTPlayerManager.ShowNotification(client, "Người chơi không tồn tại hoặc đã rời mạng.");
                    return TCPProcessCmdResults.RESULT_OK;
                }

                /// Nếu người chơi đã tồn tại trong danh sách yêu cầu thêm bạn của đối tượng
                if (client.IsAskingToBeFriendWith(player))
                {
                    KTPlayerManager.ShowNotification(client, "Đã gửi lời mời đến người chơi này, hãy kiên nhẫn đợi phản hồi!");
                    return TCPProcessCmdResults.RESULT_OK;
                }

                /// Nếu đã là bạn bè
                if (client.FriendDataList != null && client.FriendDataList.Any(x => x != null && x.OtherRoleID == roleID))
				{
                    KTPlayerManager.ShowNotification(client, "Hai bên đã là bằng hữu, không cần gửi yêu cầu kết ban thêm!");
                    return TCPProcessCmdResults.RESULT_OK;
                }

                /// Thêm người chơi vào danh sách yêu cầu thêm bạn
                client.AddAskingToBeFriend(player);

                /// Gói dữ liệu Mini của người chơi và gửi đi
                RoleDataMini rd = new RoleDataMini()
                {
                    RoleID = client.RoleID,
                    RoleName = client.RoleName,
                    FactionID = client.m_cPlayerFaction.GetFactionId(),
                    Level = client.m_Level,
                };
                byte[] _cmdData = DataHelper.ObjectToBytes<RoleDataMini>(rd);
                player.SendPacket((int) TCPGameServerCmds.CMD_SPR_ASKFRIEND, _cmdData);

                return TCPProcessCmdResults.RESULT_OK;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, Global.GetDebugHelperInfo(socket), false);
            }

            return TCPProcessCmdResults.RESULT_FAILED;
        }

        /// <summary>
        /// Phản hồi yêu cầu từ chối lời mời thêm bạn
        /// </summary>
        /// <param name="pool"></param>
        /// <param name="nID"></param>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static TCPProcessCmdResults ProcessSpriteRejectFriend(TCPManager tcpMgr, TMSKSocket socket, TCPClientPool tcpClientPool, TCPRandKey tcpRandKey, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;

            try
            {
                string cmdData = new UTF8Encoding().GetString(data, 0, count);
                string[] fields = cmdData.Split(':');
                if (fields.Length < 1)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Error Socket params count not fit CMD={0}, Recv={1}, CmdData={2}",
                        (TCPGameServerCmds) nID, fields.Length, cmdData));

                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int) TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                /// ID đối tượng mời
                int roleID = Global.SafeConvertToInt32(fields[0]);

                KPlayer client = KTPlayerManager.Find(socket);
                if (null == client)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Can not find player corresponding ID, CMD={0}, Client={1}", (TCPGameServerCmds) nID, Global.GetSocketRemoteEndPoint(socket)));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// Người chơi tương ứng
                KPlayer player = KTPlayerManager.Find(roleID);
                /// Nếu không tìm thấy người chơi
                if (player == null)
                {
                    KTPlayerManager.ShowNotification(client, "Người chơi không tồn tại hoặc đã rời mạng.");
                    return TCPProcessCmdResults.RESULT_OK;
                }

                /// Nếu đối tượng không đã tồn tại trong danh sách yêu cầu thêm bạn của người chơi
                if (!player.IsAskingToBeFriendWith(client))
                {
                    KTPlayerManager.ShowNotification(client, "Không tồn tại yêu cầu tương ứng!");
                    return TCPProcessCmdResults.RESULT_OK;
                }

                /// Xóa người chơi khỏi danh sách yêu cầu thêm bạn
                player.RemoveAskingToBeFriend(client);

                /// Gửi phản hồi từ chối yêu cầu thêm bạn
                byte[] _cmdData = new ASCIIEncoding().GetBytes(string.Format("{0}:{1}", client.RoleID, client.RoleName));
                player.SendPacket((int) TCPGameServerCmds.CMD_SPR_REJECTFRIEND, _cmdData);
                byte[] __cmdData = new ASCIIEncoding().GetBytes(string.Format("{0}:{1}", client.RoleID, player.RoleID));
                client.SendPacket((int) TCPGameServerCmds.CMD_SPR_REJECTFRIEND, __cmdData);

                return TCPProcessCmdResults.RESULT_OK;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, Global.GetDebugHelperInfo(socket), false);
            }

            return TCPProcessCmdResults.RESULT_FAILED;
        }

        /// <summary>
        /// Xóa khỏi danh sách bạn
        /// </summary>
        /// <param name="tcpMgr"></param>
        /// <param name="tcpClientPool"></param>
        /// <param name="pool"></param>
        /// <param name="client"></param>
        /// <param name="dbID"></param>
        private static bool RemoveFriend(TCPManager tcpMgr, TCPClientPool tcpClientPool, TCPOutPacketPool pool, KPlayer client, int dbID)
        {
            bool ret = false;

            try
            {
                string strcmd = string.Format("{0}:{1}", dbID, client.RoleID);
                byte[] bytesCmd = new UTF8Encoding().GetBytes(strcmd);

                /// Thông tin kết bạn
                FriendData friendData = KTPlayerManager.GetFriendData(client, dbID);
                /// Nếu thông tin không tồn tại
                if (friendData == null)
                {
                    KTPlayerManager.ShowNotification(client, "Không tìm thấy thông tin bạn bè tương ứng, hãy thử thoát Game vào lại!");
                    return false;
                }

                TCPOutPacket tcpOutPacket = null;
                TCPProcessCmdResults result = Global.TransferRequestToDBServer(tcpMgr, client.ClientSocket, tcpClientPool, null, pool, (int) TCPGameServerCmds.CMD_SPR_REMOVEFRIEND, bytesCmd, bytesCmd.Length, out tcpOutPacket, client.ServerId);
                if (TCPProcessCmdResults.RESULT_FAILED != result)
                {
                    string strData = new UTF8Encoding().GetString(tcpOutPacket.GetPacketBytes(), 6, tcpOutPacket.PacketDataSize - 6);
                    string[] fields = strData.Split(':');
                    if (fields.Length == 3 && Convert.ToInt32(fields[2]) >= 0)
                    {
                        KTPlayerManager.RemoveFriendData(client, dbID);
                        /// Gửi gói tin đi
                        byte[] _cmdData = new UTF8Encoding().GetBytes(string.Format("{0}:{1}:{2}", dbID, friendData.OtherRoleName, friendData.OtherRoleID));
                        client.SendPacket((int) TCPGameServerCmds.CMD_SPR_REMOVEFRIEND, _cmdData);

                        /// Thằng kia
                        KPlayer otherClient = KTPlayerManager.Find(friendData.OtherRoleID);
                        /// Nếu đang online
                        if (otherClient != null)
                        {
                            KTPlayerManager.RemoveFriendData(otherClient, dbID);
                            /// Gửi gói tin đi
                            byte[] __cmdData = new UTF8Encoding().GetBytes(string.Format("{0}:{1}:{2}", dbID, client.RoleName, client.RoleID));
                            otherClient.SendPacket((int) TCPGameServerCmds.CMD_SPR_REMOVEFRIEND, __cmdData);
                        }
                    }

                    ret = true;
                }
                return ret;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, Global.GetDebugHelperInfo(client.ClientSocket), false);
            }

            return ret;
        }

        /// <summary>
        /// Thêm bạn - Kẻ thù -Sổ đen
        /// </summary>
        /// <param name="roleID"></param>
        /// <param name="friendType"></param>
        private static bool AddFriend(TCPManager tcpMgr, TCPClientPool tcpClientPool, TCPOutPacketPool pool, KPlayer client, int dbID, int otherRoleID, string otherRoleName, int friendType)
        {
            bool ret = false;

            if (client.ClientSocket.IsKuaFuLogin)
            {
                return false;
            }

            // Bạn không thể thêm chính bạn
            if (friendType == 2 && otherRoleID == client.RoleID)
            {
                return false;
            }

            try
            {
                FriendData friendData = null;
                if (otherRoleID > 0)
                {
                    // nếu thằng kia đã có trong danh sách bạn rồi thì thôi
                    friendData = KTPlayerManager.FindFriendData(client, otherRoleID);
                    if (null != friendData)
                    {
                        if (friendData.FriendType == friendType) // Nếu đã là bạn rồi
                        {
                            return ret;
                        }
                    }
                }

                //Đếm xem type đã có bao nhiêu
                int friendTypeCount = KTPlayerManager.GetFriendCountByType(client, friendType);
                if (0 == friendType)
                {
                    if (friendTypeCount >= (int) FriendsConsts.MaxFriendsNum)
                    {
                        KTPlayerManager.ShowNotification(client, "Danh sách bạn bè đã đầy");
                        return ret;
                    }
                }
                else if (1 == friendType)
                {
                    if (friendTypeCount >= (int) FriendsConsts.MaxBlackListNum)
                    {
                        KTPlayerManager.ShowNotification(client, "Danh sách sổ đen đã đầy");
                        return ret;
                    }
                }
                else if (2 == friendType)
                {
                    if (friendTypeCount >= (int) FriendsConsts.MaxEnemiesNum)
                    {
                        KTPlayerManager.ShowNotification(client, "Danh sách kẻ thù đã đầy");
                        return ret;
                    }
                }

                string strcmd = string.Format("{0}:{1}:{2}:{3}", dbID, client.RoleID, otherRoleName, friendType);
                byte[] bytesCmd = new UTF8Encoding().GetBytes(strcmd);

                TCPOutPacket tcpOutPacket = null;
                TCPProcessCmdResults result = Global.TransferRequestToDBServer(tcpMgr, client.ClientSocket, tcpClientPool, null, pool, (int) TCPGameServerCmds.CMD_SPR_ADDFRIEND, bytesCmd, bytesCmd.Length, out tcpOutPacket, client.ServerId);

                if (null == tcpOutPacket)
                {
                    return ret;
                }

                friendData = DataHelper.BytesToObject<FriendData>(tcpOutPacket.GetPacketBytes(), 6, tcpOutPacket.PacketDataSize - 6);
                /// Thằng kia
                KPlayer otherClient = null;
                if (null != friendData && friendData.DbID >= 0)
                {
                    ret = true;
                    otherClient = KTPlayerManager.Find(friendData.OtherRoleID);
                }

                /// Pha của Client
                {
                    KTPlayerManager.RemoveFriendData(client, friendData.DbID);
                    KTPlayerManager.AddFriendData(client, friendData);

                    /// Cập nhật thông tin
                    friendData.MapCode = otherClient == null ? -1 : otherClient.CurrentMapCode;
                    /// Dấu tọa độ
                    friendData.PosX = 0;
                    friendData.PosY = 0;
                    /// Cập nhật trạng thái Online
                    friendData.OnlineState = otherClient != null ? 1 : 0;

                    /// Gửi gói tin đi
                    byte[] _cmdData = DataHelper.ObjectToBytes<FriendData>(friendData);
                    client.SendPacket((int) TCPGameServerCmds.CMD_SPR_ADDFRIEND, _cmdData);
                }

                /// Pha của OtherPlayer
                if (otherClient != null)
                {
                    /// Nếu là thêm bạn
                    if (friendData.FriendType == 0)
                    {
                        /// Tạo FriendData mới chứa thông tin của Client cho otherClient
                        FriendData otherFriendData = new FriendData()
                        {
                            DbID = friendData.DbID,
                            FriendType = friendData.FriendType,
                            Relationship = friendData.Relationship,
                            FactionID = client.m_cPlayerFaction.GetFactionId(),
                            GuildID = client.GuildID,
                            MapCode = client.CurrentMapCode,
                            /// Dấu tọa độ
                            PosX = 0,
                            PosY = 0,

                            OnlineState = 1,
                            OtherLevel = client.m_Level,
                            OtherRoleID = client.RoleID,
                            OtherRoleName = client.GetRoleData().RoleName,
                            PicCode = client.RolePic,
                            SpouseId = -1,
                        };

                        KTPlayerManager.RemoveFriendData(otherClient, otherFriendData.DbID);
                        KTPlayerManager.AddFriendData(otherClient, otherFriendData);

                        /// Gửi gói tin đi
                        byte[] _cmdData = DataHelper.ObjectToBytes<FriendData>(otherFriendData);
                        otherClient.SendPacket((int) TCPGameServerCmds.CMD_SPR_ADDFRIEND, _cmdData);
                    }
                }

                return ret;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, Global.GetDebugHelperInfo(client.ClientSocket), false);
            }
            return ret;
        }
    }
}
