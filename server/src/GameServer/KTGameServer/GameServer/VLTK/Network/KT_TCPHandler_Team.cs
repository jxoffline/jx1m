using GameServer.Logic;
using GameServer.Server;
using Server.Data;
using Server.Protocol;
using Server.TCP;
using Server.Tools;
using System;
using System.Collections.Generic;
using System.Text;
using static GameServer.Logic.KTMapManager;

namespace GameServer.KiemThe.Logic
{
    /// <summary>
    /// Quản lý tổ đội
    /// </summary>
    public static partial class KT_TCPHandler
    {
        #region Tổ đội

        /// <summary>
        /// Phản hồi yêu cầu tạo nhóm
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
        public static TCPProcessCmdResults ResponseCreateTeam(TCPManager tcpMgr, TMSKSocket socket, TCPClientPool tcpClientPool, TCPRandKey tcpRandKey, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;

            string cmdData = "";
            try
            {
                cmdData = new ASCIIEncoding().GetString(data);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Error while getting DATA, CMD={0}, Client={1}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket)));
                return TCPProcessCmdResults.RESULT_FAILED;
            }

            try
            {
                if (!string.IsNullOrEmpty(cmdData))
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Error Socket params count not fit CMD={0}, Client={1}, Recv={2}",
                        (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), cmdData.Length));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// Người chơi tương ứng
                KPlayer client = KTPlayerManager.Find(socket);
                if (null == client)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Can not find player, CMD={0}, Client={1}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket)));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// Bản đồ tương ứng
                GameMap gameMap = KTMapManager.Find(client.CurrentMapCode);
                /// Nếu bản đồ không cho phép tạo nhóm
                if (gameMap != null && !gameMap.AllowCreateTeam)
                {
                    KTPlayerManager.ShowNotification(client, "Bản đồ này không cho phép tạo nhóm!");
                    return TCPProcessCmdResults.RESULT_OK;
                }

                /// Nếu đã có nhóm
                if (client.TeamID != -1 && KTTeamManager.IsTeamExist(client.TeamID))
                {
                    KTPlayerManager.ShowNotification(client, "Bạn đã có nhóm, không thể tạo thêm!");
                    return TCPProcessCmdResults.RESULT_OK;
                }

                /// Tạo nhóm tương ứng
                KTTeamManager.CreateTeam(client);

                /// Gửi gói tin thông báo nhóm lại
                string rspString = string.Format("{0}", client.TeamID);
                //tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, rspString, nID);
                client.SendPacket(nID, rspString);

                /// Gửi gói tin đến tất cả người chơi xung quanh đối tượng thông báo tổ đội thay đổi
                KT_TCPHandler.SendSpriteTeamChangedToAllOthers(client);

                return TCPProcessCmdResults.RESULT_OK;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, Global.GetDebugHelperInfo(socket), false);
            }

            return TCPProcessCmdResults.RESULT_FAILED;
        }

        /// <summary>
        /// Phản hồi yêu cầu mời thành viên vào nhóm
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
        public static TCPProcessCmdResults ResponseInviteTeammate(TCPManager tcpMgr, TMSKSocket socket, TCPClientPool tcpClientPool, TCPRandKey tcpRandKey, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;

            string cmdData = "";
            try
            {
                cmdData = new ASCIIEncoding().GetString(data);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Error while getting DATA, CMD={0}, Client={1}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket)));
                return TCPProcessCmdResults.RESULT_FAILED;
            }

            try
            {
                string[] fields = cmdData.Split('|');
                if (fields.Length != 1)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Error Socket params count not fit CMD={0}, Client={1}, Recv={2}",
                        (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), fields.Length));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// ID người chơi tương ứng được thêm vào nhóm
                int playerID = int.Parse(fields[0]);

                /// Người chơi tương ứng
                KPlayer client = KTPlayerManager.Find(socket);
                if (null == client)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Can not find player, CMD={0}, Client={1}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket)));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// Bản đồ tương ứng
                GameMap gameMap = KTMapManager.Find(client.CurrentMapCode);
                /// Nếu bản đồ không cho phép tạo nhóm
                if (gameMap != null && !gameMap.AllowInviteToTeam)
                {
                    KTPlayerManager.ShowNotification(client, "Bản đồ này không cho phép mời vào nhóm!");
                    return TCPProcessCmdResults.RESULT_OK;
                }

                /// Nếu người chơi chưa có nhóm
                if (client.TeamID == -1 || !KTTeamManager.IsTeamExist(client.TeamID))
                {
                    KTPlayerManager.ShowNotification(client, "Bạn chưa có nhóm, không thể thêm thành viên!");
                    return TCPProcessCmdResults.RESULT_OK;
                }
                /// Nếu không phải đội trưởng
                else if (client.TeamLeader != client)
                {
                    KTPlayerManager.ShowNotification(client, "Bạn không phải đội trưởng, không thể thao tác!");
                    return TCPProcessCmdResults.RESULT_OK;
                }
                else if (client.Teammates.Count >= KTTeamManager.MaxTeamSize)
                {
                    KTPlayerManager.ShowNotification(client, "Nhóm đã đầy, không thể thêm thành viên!");
                    return TCPProcessCmdResults.RESULT_OK;
                }

                /// Nếu tự mời chính mình
                if (playerID == client.RoleID)
                {
                    KTPlayerManager.ShowNotification(client, "Không thể tự gửi lời mời tới chính mình!");
                    return TCPProcessCmdResults.RESULT_OK;
                }

                /// Người chơi tương ứng được thêm vào nhóm
                KPlayer player = KTPlayerManager.Find(playerID);

                /// Nếu người chơi không tồn tại
                if (player == null)
                {
                    KTPlayerManager.ShowNotification(client, "Người chơi không tồn tại hoặc đã rời mạng!");
                    return TCPProcessCmdResults.RESULT_OK;
                }
                /// Nếu người chơi đã trong nhóm
                else if (player.TeamID != -1 && player.TeamID == client.TeamID)
                {
                    KTPlayerManager.ShowNotification(client, "Người chơi đã ở trong nhóm rồi!");
                    return TCPProcessCmdResults.RESULT_OK;
                }
                /// Nếu người chơi đã trong nhóm khác
                else if (player.TeamID != -1 && KTTeamManager.IsTeamExist(player.TeamID))
                {
                    KTPlayerManager.ShowNotification(client, "Người chơi đã ra nhập nhóm khác!");
                    return TCPProcessCmdResults.RESULT_OK;
                }

                /// Gửi yêu cầu mời vào nhóm đến người chơi tương ứng
                string strCmd = string.Format("{0}:{1}:{2}:{3}:{4}:{5}", client.TeamID, client.RoleID, client.RoleName, client.RolePic, client.m_Level, client.m_cPlayerFaction.GetFactionId());
                player.SendPacket((int) TCPGameServerCmds.CMD_KT_INVITETOTEAM, strCmd);

                KTPlayerManager.ShowNotification(client, "Gửi lời mời vào nhóm tới " + player.RoleName + " thành công!");
                return TCPProcessCmdResults.RESULT_OK;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, Global.GetDebugHelperInfo(socket), false);
            }

            return TCPProcessCmdResults.RESULT_FAILED;
        }

        /// <summary>
        /// Phản hồi yêu cầu mời thành viên vào nhóm
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
        public static TCPProcessCmdResults ResponseAgreeJoinTeam(TCPManager tcpMgr, TMSKSocket socket, TCPClientPool tcpClientPool, TCPRandKey tcpRandKey, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;

            string cmdData = "";
            try
            {
                cmdData = new ASCIIEncoding().GetString(data);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Error while getting DATA, CMD={0}, Client={1}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket)));
                return TCPProcessCmdResults.RESULT_FAILED;
            }

            try
            {
                string[] fields = cmdData.Split('|');
                if (fields.Length != 2)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Error Socket params count not fit CMD={0}, Client={1}, Recv={2}",
                        (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), fields.Length));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// ID người chơi được thêm vào nhóm (-1 nếu là bản thân)
                int playerID = int.Parse(fields[0]);
                /// ID nhóm
                int teamID = int.Parse(fields[1]);

                /// Người chơi tương ứng
                KPlayer client = KTPlayerManager.Find(socket);
                if (null == client)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Can not find player, CMD={0}, Client={1}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket)));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// Bản đồ tương ứng
                GameMap gameMap = KTMapManager.Find(client.CurrentMapCode);
                /// Nếu bản đồ không cho phép tạo nhóm
                if (gameMap != null && !gameMap.AllowJoinTeam)
                {
                    KTPlayerManager.ShowNotification(client, "Bản đồ này không cho phép gia nhập nhóm!");
                    return TCPProcessCmdResults.RESULT_OK;
                }

                /// Nếu ID người chơi trùng với ID bản thân
                if (playerID == client.RoleID)
                {
                    playerID = -1;
                }

                /// Nếu nhóm không tồn tại
                if (!KTTeamManager.IsTeamExist(teamID))
                {
                    KTPlayerManager.ShowNotification(client, "Nhóm không tồn tại hoặc đã giải tán!");
                    return TCPProcessCmdResults.RESULT_OK;
                }

                KPlayer player = client;
                /// Nếu có người chơi tương ứng
                if (playerID != -1)
                {
                    /// Người chơi tương ứng
                    player = KTPlayerManager.Find(playerID);
                    /// Nếu người chơi không tồn tại
                    if (player == null)
                    {
                        KTPlayerManager.ShowNotification(client, "Người chơi không tồn tại hoặc đã rời!");
                        return TCPProcessCmdResults.RESULT_OK;
                    }

                    /// Nếu người chơi đã có trong nhóm
                    if (player.TeamID == teamID)
                    {
                        KTPlayerManager.ShowNotification(client, "Người chơi này đã là thành viên tổ đội rồi!");
                        return TCPProcessCmdResults.RESULT_OK;
                    }
                    /// Nếu người chơi đã có nhóm
                    else if (player.TeamID != -1 && KTTeamManager.IsTeamExist(player.TeamID))
                    {
                        KTPlayerManager.ShowNotification(client, "Người chơi này đã gia nhập nhóm khác!");
                        return TCPProcessCmdResults.RESULT_OK;
                    }
                }
                else
                {
                    /// Nếu đã trong nhóm
                    if (teamID == client.TeamID)
                    {
                        KTPlayerManager.ShowNotification(client, "Bạn đã ở trong nhóm này rồi!");
                        return TCPProcessCmdResults.RESULT_OK;
                    }
                    /// Nếu đã có nhóm
                    else if (client.TeamID != -1 && KTTeamManager.IsTeamExist(client.TeamID))
                    {
                        KTPlayerManager.ShowNotification(client, "Bạn đã có nhóm, không thể gia nhập nhóm khác cho đến khi rời nhóm hiện tại!");
                        return TCPProcessCmdResults.RESULT_OK;
                    }
                }

                /// Nếu nhóm đã đầy
                if (KTTeamManager.GetTeamSize(teamID) >= KTTeamManager.MaxTeamSize)
                {
                    KTPlayerManager.ShowNotification(client, "Nhóm đã đầy, không thể thêm thành viên!");
                    return TCPProcessCmdResults.RESULT_OK;
                }

                /// Thêm người chơi vào nhóm
                KTTeamManager.AssignTeam(teamID, player);

                /// Gửi thông tin có thành viên mới gia nhập nhóm
                foreach (KPlayer teammate in client.Teammates)
                {
                    if (teammate != player)
                    {
                        TeamMemberChanged teamMember = new TeamMemberChanged()
                        {
                            RoleID = player.RoleID,
                            MapCode = player.CurrentMapCode,
                            PosX = player.PosX,
                            PosY = player.PosY,
                            HP = player.m_CurrentLife,
                            MaxHP = player.m_CurrentLifeMax,
                            Type = 1,
                            TeamLeaderID = player.TeamLeader.RoleID,
                            FactionID = player.m_cPlayerFaction.GetFactionId(),
                            Level = player.m_Level,
                            RoleName = player.RoleName,

                            /// TODO
                            ArmorID = 0,
                            HelmID = 0,
                            WeaponID = 0,
                            MantleID = 0,
                            WeaponEnhanceLevel = 0,
                            AvartaID = player.RolePic,
                        };
                        byte[] _cmdData = DataHelper.ObjectToBytes<TeamMemberChanged>(teamMember);
                        teammate.SendPacket((int) TCPGameServerCmds.CMD_KT_TEAMMEMBERCHANGED, _cmdData);
                    }
                }

                /// Làm mới thông tin nhóm
                TeamInfo teamInfo = new TeamInfo()
                {
                    TeamID = player.TeamID,
                    TeamLeaderID = player.TeamLeader == null ? -1 : player.TeamLeader.RoleID,
                    Members = new List<RoleDataMini>(),
                };
                foreach (KPlayer teammate in player.Teammates)
                {
                    RoleDataMini roleData = KTGlobal.ClientToRoleDataMini(teammate, false);
                    teamInfo.Members.Add(roleData);
                }
                byte[] __cmdData = DataHelper.ObjectToBytes<TeamInfo>(teamInfo);

                /// Gửi gói tin đến tất cả người chơi xung quanh đối tượng thông báo tổ đội thay đổi
                KT_TCPHandler.SendSpriteTeamChangedToAllOthers(player);

                /// Gửi lại thông tin nhóm cho người chơi tương ứng
                player.SendPacket((int) TCPGameServerCmds.CMD_KT_AGREEJOINTEAM, __cmdData);

                return TCPProcessCmdResults.RESULT_OK;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, Global.GetDebugHelperInfo(socket), false);
            }

            return TCPProcessCmdResults.RESULT_FAILED;
        }

        /// <summary>
        /// Phản hồi yêu cầu rời nhóm
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
        public static TCPProcessCmdResults ResponseLeaveTeam(TCPManager tcpMgr, TMSKSocket socket, TCPClientPool tcpClientPool, TCPRandKey tcpRandKey, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;

            string cmdData = "";
            try
            {
                cmdData = new ASCIIEncoding().GetString(data);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Error while getting DATA, CMD={0}, Client={1}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket)));
                return TCPProcessCmdResults.RESULT_FAILED;
            }

            try
            {
                if (!string.IsNullOrEmpty(cmdData))
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Error Socket params count not fit CMD={0}, Client={1}, Recv={2}",
                        (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), cmdData.Length));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// Người chơi tương ứng
                KPlayer client = KTPlayerManager.Find(socket);
                if (null == client)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Can not find player, CMD={0}, Client={1}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket)));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// Bản đồ tương ứng
                GameMap gameMap = KTMapManager.Find(client.CurrentMapCode);
                /// Nếu bản đồ không cho phép tạo nhóm
                if (gameMap != null && !gameMap.AllowLeaveTeam)
                {
                    KTPlayerManager.ShowNotification(client, "Bản đồ này không cho phép chủ động rời khỏi nhóm!");
                    return TCPProcessCmdResults.RESULT_OK;
                }

                /// Nếu nhóm không tồn tại
                if (client.TeamID == -1 || !KTTeamManager.IsTeamExist(client.TeamID))
                {
                    KTPlayerManager.ShowNotification(client, "Nhóm không tồn tại hoặc đã giải tán!");
                    return TCPProcessCmdResults.RESULT_OK;
                }

                /// Nếu bản thân là đội trưởng
                client.LeaveTeam();

                return TCPProcessCmdResults.RESULT_OK;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, Global.GetDebugHelperInfo(socket), false);
            }

            return TCPProcessCmdResults.RESULT_FAILED;
        }

        /// <summary>
        /// Phản hồi yêu cầu trục xuất thành viên khỏi nhóm
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
        public static TCPProcessCmdResults ResponseKickOutTeammate(TCPManager tcpMgr, TMSKSocket socket, TCPClientPool tcpClientPool, TCPRandKey tcpRandKey, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;

            string cmdData = "";
            try
            {
                cmdData = new ASCIIEncoding().GetString(data);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Error while getting DATA, CMD={0}, Client={1}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket)));
                return TCPProcessCmdResults.RESULT_FAILED;
            }

            try
            {
                string[] fields = cmdData.Split('|');
                if (fields.Length != 1)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Error Socket params count not fit CMD={0}, Client={1}, Recv={2}",
                        (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), fields.Length));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// ID người chơi bị đuổi khỏi nhóm
                int kickedOutTeammateID = int.Parse(fields[0]);

                /// Người chơi tương ứng
                KPlayer client = KTPlayerManager.Find(socket);
                if (null == client)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Can not find player, CMD={0}, Client={1}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket)));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// Bản đồ tương ứng
                GameMap gameMap = KTMapManager.Find(client.CurrentMapCode);
                /// Nếu bản đồ không cho phép tạo nhóm
                if (gameMap != null && !gameMap.AllowKickFromTeam)
                {
                    KTPlayerManager.ShowNotification(client, "Bản đồ này không cho phép trục xuất thành viên khỏi nhóm!");
                    return TCPProcessCmdResults.RESULT_OK;
                }

                /// Nếu nhóm không tồn tại
                if (client.TeamID == -1 || !KTTeamManager.IsTeamExist(client.TeamID))
                {
                    KTPlayerManager.ShowNotification(client, "Nhóm không tồn tại hoặc đã giải tán!");
                    return TCPProcessCmdResults.RESULT_OK;
                }
                /// Nếu không phải đội trưởng
                else if (client.TeamLeader != client)
                {
                    KTPlayerManager.ShowNotification(client, "Bạn không phải đội trưởng, không thể thực hiện thao tác này!");
                    return TCPProcessCmdResults.RESULT_OK;
                }
                /// Nếu ID người chơi bị đuổi là bản thân
                else if (client.RoleID == kickedOutTeammateID)
                {
                    KTPlayerManager.ShowNotification(client, "Không thể tự trục xuất bản thân mình ra khỏi nhóm!");
                    return TCPProcessCmdResults.RESULT_OK;
                }

                /// Người chơi bị đuổi khỏi nhóm
                KPlayer kickOutPlayer = KTPlayerManager.Find(kickedOutTeammateID);
                if (kickOutPlayer == null)
                {
                    KTPlayerManager.ShowNotification(client, "Người chơi không tồn tại hoặc đã rời mạng!");
                    return TCPProcessCmdResults.RESULT_OK;
                }

                /// Xóa người chơi khỏi nhóm tương ứng
                KTTeamManager.LeaveTeam(kickOutPlayer);

                /// Gửi thông tin có thành viên rời nhóm
                foreach (KPlayer teammate in client.Teammates)
                {
                    TeamMemberChanged teamMember = new TeamMemberChanged()
                    {
                        RoleID = kickOutPlayer.RoleID,
                        RoleName = kickOutPlayer.RoleName,
                        Type = 0,
                    };
                    byte[] _cmdData = DataHelper.ObjectToBytes<TeamMemberChanged>(teamMember);
                    teammate.SendPacket((int) TCPGameServerCmds.CMD_KT_KICKOUTTEAMMATE, _cmdData);
                }

                /// Gửi thông tin đến người chơi bị đuổi khỏi nhóm
                TeamMemberChanged _kickOutPlayer = new TeamMemberChanged()
                {
                    RoleID = kickOutPlayer.RoleID,
                    RoleName = kickOutPlayer.RoleName,
                    Type = 0,
                };
                byte[] __cmdData = DataHelper.ObjectToBytes<TeamMemberChanged>(_kickOutPlayer);
                kickOutPlayer.SendPacket((int) TCPGameServerCmds.CMD_KT_KICKOUTTEAMMATE, __cmdData);

                /// Gửi gói tin đến tất cả người chơi xung quanh đối tượng thông báo tổ đội thay đổi
                KT_TCPHandler.SendSpriteTeamChangedToAllOthers(kickOutPlayer);

                return TCPProcessCmdResults.RESULT_OK;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, Global.GetDebugHelperInfo(socket), false);
            }

            return TCPProcessCmdResults.RESULT_FAILED;
        }

        /// <summary>
        /// Phản hồi yêu cầu mời từ chối gia nhập nhóm
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
        public static TCPProcessCmdResults ResponseRefuseJoinTeam(TCPManager tcpMgr, TMSKSocket socket, TCPClientPool tcpClientPool, TCPRandKey tcpRandKey, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;

            string cmdData = "";
            try
            {
                cmdData = new ASCIIEncoding().GetString(data);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Error while getting DATA, CMD={0}, Client={1}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket)));
                return TCPProcessCmdResults.RESULT_FAILED;
            }

            try
            {
                string[] fields = cmdData.Split('|');
                if (fields.Length != 3)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Error Socket params count not fit CMD={0}, Client={1}, Recv={2}",
                        (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), fields.Length));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// ID người chơi tương ứng gửi lời mời
                int fromPlayerID = int.Parse(fields[0]);
                /// ID người chơi tương ứng nhận lời mời
                int toPlayerID = int.Parse(fields[1]);
                /// Loại thao tác (0: Người chơi nhận từ chối lời mời, 1: Người chơi gửi từ chối lời xin vào nhóm
                int type = int.Parse(fields[2]);

                /// Người chơi tương ứng
                KPlayer client = KTPlayerManager.Find(socket);
                if (null == client)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Can not find player, CMD={0}, Client={1}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket)));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// Người chơi gửi lời mời
                KPlayer fromPlayer;
                /// Nếu người gửi lời mời là bản thân
                if (fromPlayerID == -1 || fromPlayerID == client.RoleID)
                {
                    fromPlayer = client;
                }
                else
                {
                    fromPlayer = KTPlayerManager.Find(fromPlayerID);
                    /// Nếu không tìm thấy người gửi lời mời
                    if (fromPlayer == null)
                    {
                        return TCPProcessCmdResults.RESULT_OK;
                    }
                }

                /// Người chơi nhận lời mời
                KPlayer toPlayer;
                /// Nếu người nhận lời mời là bản thân
                if (toPlayerID == -1 || toPlayerID == client.RoleID)
                {
                    toPlayer = client;
                }
                else
                {
                    toPlayer = KTPlayerManager.Find(fromPlayerID);
                    /// Nếu không tìm thấy người nhận lời mời
                    if (toPlayer == null)
                    {
                        return TCPProcessCmdResults.RESULT_OK;
                    }
                }

                /// Nếu người gửi và người nhận cùng là một
                if (toPlayer == fromPlayer)
                {
                    return TCPProcessCmdResults.RESULT_OK;
                }

                /// Nếu nhóm không tồn tại
                if (!KTTeamManager.IsTeamExist(fromPlayer.TeamID))
                {
                    return TCPProcessCmdResults.RESULT_OK;
                }

                string _cmdData = string.Format("{0}:{1}:{2}", fromPlayerID, toPlayerID, type);

                /// Gửi gói tin thông báo người chơi từ chối gia nhập nhóm
                fromPlayer.SendPacket((int) TCPGameServerCmds.CMD_KT_REFUSEJOINTEAM, _cmdData);
                toPlayer.SendPacket((int) TCPGameServerCmds.CMD_KT_REFUSEJOINTEAM, _cmdData);

                return TCPProcessCmdResults.RESULT_OK;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, Global.GetDebugHelperInfo(socket), false);
            }

            return TCPProcessCmdResults.RESULT_FAILED;
        }

        /// <summary>
        /// Phản hồi yêu cầu bổ nhiệm đội trưởng
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
        public static TCPProcessCmdResults ResponseApproveTeamLeader(TCPManager tcpMgr, TMSKSocket socket, TCPClientPool tcpClientPool, TCPRandKey tcpRandKey, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;

            string cmdData = "";
            try
            {
                cmdData = new ASCIIEncoding().GetString(data);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Error while getting DATA, CMD={0}, Client={1}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket)));
                return TCPProcessCmdResults.RESULT_FAILED;
            }

            try
            {
                string[] fields = cmdData.Split('|');
                if (fields.Length != 1)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Error Socket params count not fit CMD={0}, Client={1}, Recv={2}",
                        (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), fields.Length));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// ID người chơi được bổ nhiệm
                int playerID = int.Parse(fields[0]);

                /// Người chơi tương ứng
                KPlayer client = KTPlayerManager.Find(socket);
                if (null == client)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Can not find player, CMD={0}, Client={1}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket)));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// Bản đồ tương ứng
                GameMap gameMap = KTMapManager.Find(client.CurrentMapCode);
                /// Nếu bản đồ không cho phép tạo nhóm
                if (gameMap != null && !gameMap.AllowChangeTeamLeader)
                {
                    KTPlayerManager.ShowNotification(client, "Bản đồ này không cho phép thay đổi nhóm trưởng!");
                    return TCPProcessCmdResults.RESULT_OK;
                }

                /// Nếu nhóm không tồn tại
                if (client.TeamID == -1 || !KTTeamManager.IsTeamExist(client.TeamID))
                {
                    KTPlayerManager.ShowNotification(client, "Bạn không có tổ đội!");
                    return TCPProcessCmdResults.RESULT_OK;
                }
                /// Nếu bản thân không phải đội trưởng
                else if (client != client.TeamLeader)
                {
                    KTPlayerManager.ShowNotification(client, "Bạn không phải đội trưởng, không thể bổ nhiệm thành viên khác làm đội trưởng!");
                    return TCPProcessCmdResults.RESULT_OK;
                }

                /// Người chơi tương ứng
                KPlayer player = KTPlayerManager.Find(playerID);
                if (null == player)
                {
                    KTPlayerManager.ShowNotification(client, "Đội viên không tồn tại hoặc đã rời mạng!");
                    return TCPProcessCmdResults.RESULT_OK;
                }

                /// Nếu bản thân là người được bổ nhiệm
                if (player == client)
                {
                    KTPlayerManager.ShowNotification(client, "Bạn không thể bổ nhiệm chính mình làm đội trưởng!");
                    return TCPProcessCmdResults.RESULT_OK;
                }

                /// Bổ nhiệm người chơi tương ứng
                KTTeamManager.ApproveTeamLeader(player);

                /// Gửi thông tin đội trưởng mới được bổ nhiệm
                foreach (KPlayer teammate in player.Teammates)
                {
                    string strCmd = string.Format("{0}:{1}", player.RoleID, player.RoleName);
                    teammate.SendPacket((int) TCPGameServerCmds.CMD_KT_APPROVETEAMLEADER, strCmd);
                }

                /// Gửi gói tin đến tất cả người chơi xung quanh đối tượng thông báo tổ đội thay đổi
                KT_TCPHandler.SendSpriteTeamChangedToAllOthers(player);

                return TCPProcessCmdResults.RESULT_OK;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, Global.GetDebugHelperInfo(socket), false);
            }

            return TCPProcessCmdResults.RESULT_FAILED;
        }

        /// <summary>
        /// Phản hồi yêu cầu xin gia nhập nhóm
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
        public static TCPProcessCmdResults ResponseAskToJoinTeam(TCPManager tcpMgr, TMSKSocket socket, TCPClientPool tcpClientPool, TCPRandKey tcpRandKey, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;

            string cmdData = "";
            try
            {
                cmdData = new ASCIIEncoding().GetString(data);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Error while getting DATA, CMD={0}, Client={1}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket)));
                return TCPProcessCmdResults.RESULT_FAILED;
            }

            try
            {
                string[] fields = cmdData.Split('|');
                if (fields.Length != 1)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Error Socket params count not fit CMD={0}, Client={1}, Recv={2}",
                        (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), fields.Length));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// ID người chơi có nhóm mà bản thân muốn xin vào
                int playerID = int.Parse(fields[0]);

                /// Người chơi tương ứng
                KPlayer client = KTPlayerManager.Find(socket);
                if (null == client)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Can not find player, CMD={0}, Client={1}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket)));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// Bản đồ tương ứng
                GameMap gameMap = KTMapManager.Find(client.CurrentMapCode);
                /// Nếu bản đồ không cho phép tạo nhóm
                if (gameMap != null && !gameMap.AllowJoinTeam)
                {
                    KTPlayerManager.ShowNotification(client, "Bản đồ này không cho phép gia nhập nhóm!");
                    return TCPProcessCmdResults.RESULT_OK;
                }

                /// Nếu bản thân đã có nhóm
                if (client.TeamID != -1 && KTTeamManager.IsTeamExist(client.TeamID))
                {
                    KTPlayerManager.ShowNotification(client, "Bạn đã có tổ đội, không thể xin gia nhập tổ đội khác!");
                    return TCPProcessCmdResults.RESULT_OK;
                }

                /// Người chơi có nhóm tương ứng
                KPlayer player = KTPlayerManager.Find(playerID);
                if (player == null)
                {
                    KTPlayerManager.ShowNotification(client, "Người chơi không tồn tại hoặc đã rời mạng!");
                    return TCPProcessCmdResults.RESULT_OK;
                }

                /// Nếu bản thân là đối tượng xem xét
                if (player == client)
                {
                    KTPlayerManager.ShowNotification(client, "Có lỗi xảy ra. Hãy báo cáo với hỗ trợ viên để được giải đáp!");
                    return TCPProcessCmdResults.RESULT_OK;
                }

                /// Nếu đối phương không có nhóm
                if (player.TeamID == -1 || !KTTeamManager.IsTeamExist(player.TeamID))
                {
                    KTPlayerManager.ShowNotification(client, "Đối phượng không có tổ đội, không thể gia nhập!");
                    return TCPProcessCmdResults.RESULT_OK;
                }

                /// Đội trưởng nhóm người chơi tương ứng
                KPlayer teamLeader = player.TeamLeader;
                /// Nếu không tìm thấy trưởng nhóm
                if (teamLeader == null)
                {
                    KTPlayerManager.ShowNotification(client, "Đối phượng không có tổ đội, không thể gia nhập!");
                    return TCPProcessCmdResults.RESULT_OK;
                }

                /// Gửi thông tin người chơi mới muốn gia nhập nhóm đến đội trưởng nhóm tương ứng
                RoleDataMini roleData = new RoleDataMini()
                {
                    RoleID = client.RoleID,
                    FactionID = client.m_cPlayerFaction.GetFactionId(),
                    Level = client.m_Level,
                    AvartaID = client.RolePic,
                    RoleName = client.RoleName,
                };
                byte[] _cmdData = DataHelper.ObjectToBytes<RoleDataMini>(roleData);
                teamLeader.SendPacket((int) TCPGameServerCmds.CMD_KT_ASKTOJOINTEAM, _cmdData);

                /// Gửi gói tin đến tất cả người chơi xung quanh đối tượng thông báo tổ đội thay đổi
                KT_TCPHandler.SendSpriteTeamChangedToAllOthers(player);

                return TCPProcessCmdResults.RESULT_OK;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, Global.GetDebugHelperInfo(socket), false);
            }

            return TCPProcessCmdResults.RESULT_FAILED;
        }

        /// <summary>
        /// Phản hồi lấy thông tin thành viên nhóm
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
        public static TCPProcessCmdResults ResponseGetTeamInfo(TCPManager tcpMgr, TMSKSocket socket, TCPClientPool tcpClientPool, TCPRandKey tcpRandKey, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;

            string cmdData = "";
            try
            {
                cmdData = new ASCIIEncoding().GetString(data);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Error while getting DATA, CMD={0}, Client={1}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket)));
                return TCPProcessCmdResults.RESULT_FAILED;
            }

            try
            {
                if (!string.IsNullOrEmpty(cmdData))
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Error Socket params count not fit CMD={0}, Client={1}, Recv={2}",
                        (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), cmdData.Length));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// Người chơi tương ứng
                KPlayer client = KTPlayerManager.Find(socket);
                if (null == client)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Can not find player, CMD={0}, Client={1}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket)));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// Làm mới thông tin nhóm
                TeamInfo teamInfo = new TeamInfo()
                {
                    TeamID = client.TeamID,
                    TeamLeaderID = client.TeamLeader == null ? -1 : client.TeamLeader.RoleID,
                    Members = new List<RoleDataMini>(),
                };
                foreach (KPlayer teammate in client.Teammates)
                {
                    RoleDataMini roleData = KTGlobal.ClientToRoleDataMini(teammate, false);
                    teamInfo.Members.Add(roleData);
                }
                byte[] _cmdData = DataHelper.ObjectToBytes<TeamInfo>(teamInfo);

                /// Gửi lại thông tin nhóm cho Client
                //tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, _cmdData, nID);
                client.SendPacket(nID, _cmdData);

                return TCPProcessCmdResults.RESULT_OK;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, Global.GetDebugHelperInfo(socket), false);
            }

            return TCPProcessCmdResults.RESULT_FAILED;
        }

        /// <summary>
        /// Gửi yêu cầu làm mới thông tin chỉ số tất cả thành viên trong nhóm tương ứng
        /// </summary>
        /// <param name="player"></param>
        public static void SendRefreshTeamMemberAttrib(KPlayer player)
        {
            try
            {
                List<TeamMemberAttributes> members = new List<TeamMemberAttributes>();
                foreach (KPlayer teammate in player.Teammates)
                {
                    if (teammate == null)
                    {
                        continue;
                    }

                    TeamMemberAttributes teamMemberInfo = new TeamMemberAttributes()
                    {
                        RoleID = teammate.RoleID,
                        MapCode = teammate.MapCode,
                        PosX = teammate.PosX,
                        PosY = teammate.PosY,
                        HP = teammate.m_CurrentLife,
                        MaxHP = teammate.m_CurrentLifeMax,
                        AvartaID = teammate.RolePic,
                        FactionID = teammate.m_cPlayerFaction.GetFactionId(),
                        Level = teammate.m_Level,
                    };

                    /// Nếu không chung bản đồ hoặc phụ bản
                    if (teammate.CurrentMapCode != player.CurrentMapCode || teammate.CurrentCopyMapID != player.CurrentCopyMapID)
                    {
                        teamMemberInfo.PosX = -1;
                        teamMemberInfo.PosY = -1;
                        teamMemberInfo.HP = -1;
                        teamMemberInfo.MaxHP = -1;
                    }

                    members.Add(teamMemberInfo);
                }
                byte[] cmdData = DataHelper.ObjectToBytes<List<TeamMemberAttributes>>(members);
                player.SendPacket((int) TCPGameServerCmds.CMD_KT_REFRESHTEAMMEMBERATTRIBUTES, cmdData);
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Exception, ex.ToString());
            }
        }

        /// <summary>
        /// Gửi yêu cầu làm mới thông tin chỉ số tất cả thành viên trong nhóm tương ứng
        /// </summary>
        /// <param name="player"></param>
        public static void SendRefreshTeamMemberAttribToAllTeammates(KPlayer player)
        {
            try
            {
                List<TeamMemberAttributes> members = new List<TeamMemberAttributes>();
                foreach (KPlayer teammate in player.Teammates)
                {
                    TeamMemberAttributes teamMemberInfo = new TeamMemberAttributes()
                    {
                        RoleID = teammate.RoleID,
                        MapCode = teammate.MapCode,
                        PosX = teammate.PosX,
                        PosY = teammate.PosY,
                        HP = teammate.m_CurrentLife,
                        MaxHP = teammate.m_CurrentLifeMax,
                        AvartaID = teammate.RolePic,
                        FactionID = teammate.m_cPlayerFaction.GetFactionId(),
                        Level = teammate.m_Level,
                    };

                    /// Nếu không chung bản đồ hoặc phụ bản
                    if (teammate.CurrentMapCode != player.CurrentMapCode || teammate.CurrentCopyMapID != player.CurrentCopyMapID)
                    {
                        teamMemberInfo.PosX = -1;
                        teamMemberInfo.PosY = -1;
                        teamMemberInfo.HP = -1;
                        teamMemberInfo.MaxHP = -1;
                    }

                    members.Add(teamMemberInfo);
                }
                byte[] cmdData = DataHelper.ObjectToBytes<List<TeamMemberAttributes>>(members);

                foreach (KPlayer teammate in player.Teammates)
                {
                    teammate.SendPacket((int) TCPGameServerCmds.CMD_KT_REFRESHTEAMMEMBERATTRIBUTES, cmdData);
                }
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Exception, ex.ToString());
            }
        }

        /// <summary>
        /// Gửi thông báo tới tất cả người chơi xung quanh thông tin tổ đội của đối tượng thay đổi
        /// </summary>
        /// <param name="player"></param>
        public static void SendSpriteTeamChangedToAllOthers(KPlayer player)
        {
            try
            {
                string strCmd = string.Format("{0}:{1}:{2}", player.RoleID, player.TeamID, player.TeamLeader == null ? -1 : player.TeamLeader.RoleID);

                /// Tìm tất cả người chơi xung quanh để gửi gói tin
                List<KPlayer> listObjects = KTRadarMapManager.GetPlayersAround(player);
                if (listObjects == null)
                {
                    return;
                }
                /// Gửi gói tin đến tất cả người chơi xung quanh
                KTPlayerManager.SendPacketToPlayers((int) TCPGameServerCmds.CMD_KT_G2C_UPDATESPRITETEAMDATA, listObjects, strCmd, player, null);
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Exception, ex.ToString());
            }
        }

        #endregion Tổ đội
    }
}
