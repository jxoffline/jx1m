using GameServer.KiemThe.CopySceneEvents.DynamicArena;
using GameServer.KiemThe.Entities;
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
    /// Quản lý PK
    /// </summary>
    public static partial class KT_TCPHandler
    {
        #region Thách đấu

        /// <summary>
        /// Thực thi gửi lời mời thách đấu lên người chơi tương ứng
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
        public static TCPProcessCmdResults ResponseAskChallenge(TCPManager tcpMgr, TMSKSocket socket, TCPClientPool tcpClientPool, TCPRandKey tcpRandKey, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;
            string strCmd;

            try
            {
                strCmd = new ASCIIEncoding().GetString(data, 0, count);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Error while getting DATA, CMD={0}, Client={1}", (TCPGameServerCmds) nID, Global.GetSocketRemoteEndPoint(socket)));
                return TCPProcessCmdResults.RESULT_FAILED;
            }

            try
            {
                string[] fields = strCmd.Split(':');
                /// Nếu số lượng gửi về không thỏa mãn
                if (fields.Length != 1)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Error Socket params count not fit CMD={0}, Client={1}, Recv={2}", (TCPGameServerCmds) nID, Global.GetSocketRemoteEndPoint(socket), fields.Length));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// ID đối tượng tương ứng
                int targetID = int.Parse(fields[0]);

                /// Người chơi tương ứng gửi gói tin
                KPlayer client = KTPlayerManager.Find(socket);
                if (null == client)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Can not find player, CMD={0}, Client={1}", (TCPGameServerCmds) nID, Global.GetSocketRemoteEndPoint(socket)));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// Nếu chưa mở khóa an toàn
                if (client.NeedToShowInputSecondPassword())
                {
                    return TCPProcessCmdResults.RESULT_OK;
                }

                /// Nếu bản thân không ở trong khu vực được thách đấu
                if (!KTGlobal.IsInsideChallengeArea(client))
                {
                    KTPlayerManager.ShowNotification(client, "Khu vực này không thể thách đấu!");
                    return TCPProcessCmdResults.RESULT_OK;
                }

                /// Nếu bản thân không phải đội trưởng
                if (client.TeamID != -1 && KTTeamManager.IsTeamExist(client.TeamID) && client.TeamLeader != client)
                {
                    KTPlayerManager.ShowNotification(client, "Bạn không phải đội trưởng, không thể thách đấu!");
                    return TCPProcessCmdResults.RESULT_OK;
                }

                /// Đối tượng thách đấu
                KPlayer target = KTPlayerManager.Find(targetID);
                /// Nếu không tìm thấy đối tượng
                if (target == null)
                {
                    KTPlayerManager.ShowNotification(client, "Người chơi này không tồn tại hoặc đã rời mạng!");
                    return TCPProcessCmdResults.RESULT_OK;
                }

                /// Nếu đối tượng trùng với bản thân
                if (target == client)
                {
                    KTPlayerManager.ShowNotification(client, "Không thể tự thách đấu chính mình!");
                    return TCPProcessCmdResults.RESULT_OK;
                }

                /// Nếu đối phương đã chết
                if (target.IsDead())
                {
                    KTPlayerManager.ShowNotification(client, "Đối phương đã tử nạn, không thể thách đấu!");
                    return TCPProcessCmdResults.RESULT_OK;
                }

                /// Nếu đối phương ở bản đồ khác
                if (target.CurrentMapCode != client.CurrentMapCode)
                {
                    KTPlayerManager.ShowNotification(client, "Đối phương đang ở bản đồ khác, không thể thách đấu!");
                    return TCPProcessCmdResults.RESULT_OK;
                }

                /// Nếu đối phương không phải đội trưởng
                if (target.TeamID != -1 && KTTeamManager.IsTeamExist(target.TeamID) && target.TeamLeader != target)
                {
                    KTPlayerManager.ShowNotification(client, "Đối phương không phải đội trưởng, không thể thách đấu!");
                    return TCPProcessCmdResults.RESULT_OK;
                }

                /// Nếu đối phương không ở trong khu vực được thách đấu
                if (!KTGlobal.IsInsideChallengeArea(target))
                {
                    KTPlayerManager.ShowNotification(client, "Đối phương không ở khu vực có thể thách đấu!");
                    return TCPProcessCmdResults.RESULT_OK;
                }

                /// Nếu bản thân đang có dữ liệu thách đấu
                if (client.IsChallenging())
                {
                    KTPlayerManager.ShowNotification(client, "Bản thân đang thách đấu với người khác rồi!");
                    return TCPProcessCmdResults.RESULT_OK;
                }

                /// Nếu đối phương đang có dữ liệu thách đấu
                if (target.IsChallenging())
                {
                    KTPlayerManager.ShowNotification(client, "Đối phương đang thách đấu với người khác rồi!");
                    return TCPProcessCmdResults.RESULT_OK;
                }

                /// Nếu bản thân dưới cấp 30
                if (client.m_Level < 30)
                {
                    KTPlayerManager.ShowNotification(client, "Dưới cấp 30 không thể thách đấu!");
                    return TCPProcessCmdResults.RESULT_OK;
                }
                /// Nếu đối phương dưới cấp 30
                if (target.m_Level < 30)
                {
                    KTPlayerManager.ShowNotification(client, "Đối phương dưới cấp 30, không thể thách đấu!");
                    return TCPProcessCmdResults.RESULT_OK;
                }

                /// Gửi lời mời thách đấu
                KT_TCPHandler.SendAskChallengeRequest(client, target);

                return TCPProcessCmdResults.RESULT_OK;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, Global.GetDebugHelperInfo(socket), false);
            }

            return TCPProcessCmdResults.RESULT_FAILED;
        }

        /// <summary>
        /// Gửi lời mời thách đấu đến người chơi tương ứng
        /// </summary>
        /// <param name="player"></param>
        /// <param name="target"></param>
        public static void SendAskChallengeRequest(KPlayer player, KPlayer target)
        {
            try
            {
                string strCmd = string.Format("{0}:{1}", player.RoleID, player.RoleName);
                target.SendPacket((int) TCPGameServerCmds.CMD_KT_ASK_CHALLENGE, strCmd);
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Exception, ex.ToString());
            }
        }

        /// <summary>
        /// Thực thi yêu cầu đồng ý hoặc từ chối thách đấu với người chơi tương ứng
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
        public static TCPProcessCmdResults ResponseResponseChallenge(TCPManager tcpMgr, TMSKSocket socket, TCPClientPool tcpClientPool, TCPRandKey tcpRandKey, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;
            string strCmd;

            try
            {
                strCmd = new ASCIIEncoding().GetString(data, 0, count);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Error while getting DATA, CMD={0}, Client={1}", (TCPGameServerCmds) nID, Global.GetSocketRemoteEndPoint(socket)));
                return TCPProcessCmdResults.RESULT_FAILED;
            }

            try
            {
                string[] fields = strCmd.Split(':');
                /// Nếu số lượng gửi về không thỏa mãn
                if (fields.Length != 2)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Error Socket params count not fit CMD={0}, Client={1}, Recv={2}", (TCPGameServerCmds) nID, Global.GetSocketRemoteEndPoint(socket), fields.Length));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// ID đối tượng gửi lời mời thách đấu
                int inviterID = int.Parse(fields[0]);
                /// Loại thao tác (0: Từ chối, 1: Đồng ý)
                int type = int.Parse(fields[1]);

                /// Người chơi tương ứng gửi gói tin
                KPlayer client = KTPlayerManager.Find(socket);
                if (null == client)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Can not find player, CMD={0}, Client={1}", (TCPGameServerCmds) nID, Global.GetSocketRemoteEndPoint(socket)));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// Nếu bản thân không ở trong khu vực được thách đấu
                if (!KTGlobal.IsInsideChallengeArea(client))
                {
                    KTPlayerManager.ShowNotification(client, "Khu vực này không thể thách đấu!");
                    return TCPProcessCmdResults.RESULT_OK;
                }

                /// Đối tượng gửi lời mời thách đấu
                KPlayer inviter = KTPlayerManager.Find(inviterID);
                /// Nếu không tìm thấy đối tượng
                if (inviter == null)
                {
                    KTPlayerManager.ShowNotification(client, "Đối phương không tồn tại hoặc đã rời mạng!");
                    return TCPProcessCmdResults.RESULT_OK;
                }

                /// Nếu đối tượng trùng với bản thân
                if (inviter == client)
                {
                    KTPlayerManager.ShowNotification(client, "Không thể tự thách đấu với bản thân!");
                    return TCPProcessCmdResults.RESULT_OK;
                }

                /// Nếu thao tác là từ chối
                if (type == 0)
                {
                    KTPlayerManager.ShowNotification(inviter, "Đối phương từ chối lời mời thách đấu!");
                }
                /// Nếu thao tác là đồng ý
                else if (type == 1)
                {
                    /// Nếu đối phương đã chết
                    if (inviter.IsDead())
                    {
                        KTPlayerManager.ShowNotification(client, "Đối phương đã tử nạn, không thể thách đấu!");
                        return TCPProcessCmdResults.RESULT_OK;
                    }

                    /// Nếu đối phương ở bản đồ khác
                    if (inviter.CurrentMapCode != client.CurrentMapCode)
                    {
                        KTPlayerManager.ShowNotification(client, "Đối phương đang ở bản đồ khác, không thể thách đấu!");
                        return TCPProcessCmdResults.RESULT_OK;
                    }

                    /// Nếu bản thân không phải đội trưởng
                    if (client.TeamID != -1 && KTTeamManager.IsTeamExist(client.TeamID) && client.TeamLeader != client)
                    {
                        KTPlayerManager.ShowNotification(client, "Bạn không phải đội trưởng, không thể thách đấu!");
                        return TCPProcessCmdResults.RESULT_OK;
                    }

                    /// Nếu đối phương không phải đội trưởng
                    if (inviter.TeamID != -1 && KTTeamManager.IsTeamExist(inviter.TeamID) && inviter.TeamLeader != inviter)
                    {
                        KTPlayerManager.ShowNotification(client, "Đối phương không phải đội trưởng, không thể thách đấu!");
                        return TCPProcessCmdResults.RESULT_OK;
                    }

                    /// Nếu bản thân đang có dữ liệu thách đấu
                    if (client.IsChallenging())
                    {
                        KTPlayerManager.ShowNotification(client, "Bản thân đang thách đấu với người khác rồi!");
                        return TCPProcessCmdResults.RESULT_OK;
                    }

                    /// Nếu đối phương đang có dữ liệu thách đấu
                    if (inviter.IsChallenging())
                    {
                        KTPlayerManager.ShowNotification(client, "Đối phương đang thách đấu với người khác rồi!");
                        return TCPProcessCmdResults.RESULT_OK;
                    }

                    /// Nếu đối phương không ở trong khu vực được thách đấu
                    if (!KTGlobal.IsInsideChallengeArea(inviter))
                    {
                        KTPlayerManager.ShowNotification(client, "Đối phương không ở trong khu vực có thể thách đấu!");
                        return TCPProcessCmdResults.RESULT_OK;
                    }

                    /// Nếu bản thân dưới cấp 30
                    if (client.m_Level < 30)
                    {
                        KTPlayerManager.ShowNotification(client, "Dưới cấp 30 không thể thách đấu!");
                        return TCPProcessCmdResults.RESULT_OK;
                    }
                    /// Nếu đối phương dưới cấp 30
                    if (inviter.m_Level < 30)
                    {
                        KTPlayerManager.ShowNotification(inviter, "Đối phương dưới cấp 30, không thể thách đấu!");
                        return TCPProcessCmdResults.RESULT_OK;
                    }

                    /// Thông tin thách đấu
                    RoleChallengeData challengeData = new RoleChallengeData()
                    {
                        TeamPlayers = new Dictionary<int, List<RoleChallenge_PlayerData>>()
                        {
                            { 1, new List<RoleChallenge_PlayerData>() },
                            { 2, new List<RoleChallenge_PlayerData>() },
                        },
                        TeamMoneys = new Dictionary<int, int>()
                        {
                            { 1, 0 },
                            { 2, 0 },
                        },
                        TeamReadyStates = new Dictionary<int, bool>()
                        {
                            { 1, false },
                            { 2, false },
                        },
                    };

                    /// Chuyển dữ liệu người chơi tương ứng
                    RoleChallenge_PlayerData MakePlayerData(KPlayer player)
                    {
                        /// Chuyển qua RoleDataMini
                        RoleDataMini rd = KTGlobal.ClientToRoleDataMini(player);
                        /// Trả về kết quả
                        return new RoleChallenge_PlayerData()
                        {
                            RoleID = rd.RoleID,
                            RoleName = rd.RoleName,
                            RoleSex = rd.RoleSex,
                            Level = rd.Level,
                            HelmID = rd.HelmID,
                            ArmorID = rd.ArmorID,
                            WeaponID = rd.WeaponID,
                            FactionID = rd.FactionID,
                            WeaponEnhanceLevel = rd.WeaponEnhanceLevel,
                            WeaponSeries = rd.WeaponSeries,
                            IsTeamLeader = player.TeamID == -1 || !KTTeamManager.IsTeamExist(player.TeamID) ? true : player.TeamLeader == player,
                        };
                    }

                    /// Nếu thằng kia không có nhóm
                    if (inviter.TeamID == -1 || !KTTeamManager.IsTeamExist(inviter.TeamID))
                    {
                        /// Thêm chính nó vào danh sách
                        challengeData.TeamPlayers[1].Add(MakePlayerData(inviter));
                    }
                    /// Nếu thằng kia có nhóm
                    else
                    {
                        /// Duyệt danh sách đồng đội của thằng kia
                        foreach (KPlayer inviterTeammate in inviter.Teammates)
                        {
                            /// Thêm thằng này vào danh sách
                            challengeData.TeamPlayers[1].Add(MakePlayerData(inviterTeammate));
                        }
                    }
                    
                    /// Nếu bản thân không có nhóm
                    if (client.TeamID == -1 || !KTTeamManager.IsTeamExist(client.TeamID))
                    {
                        /// Thêm chính mình vào danh sách
                        challengeData.TeamPlayers[2].Add(MakePlayerData(client));
                    }
                    /// Nếu bản thân có nhóm
                    else
                    {
                        /// Duyệt danh sách đồng đội của bản thân
                        foreach (KPlayer teammate in client.Teammates)
                        {
                            /// Thêm thằng này vào danh sách
                            challengeData.TeamPlayers[2].Add(MakePlayerData(teammate));
                        }
                    }

                    /// Tạo dữ liệu thách đấu cho cả 2 thằng
                    inviter.CreateChallenge(client, 1);
                    client.CreateChallenge(inviter, 2);

                    /// Gửi gói tin đến cả 2 thằng
                    inviter.SendPacket<RoleChallengeData>((int) TCPGameServerCmds.CMD_KT_RECEIVE_CHALLENGE_INFO, challengeData);
                    client.SendPacket<RoleChallengeData>((int) TCPGameServerCmds.CMD_KT_RECEIVE_CHALLENGE_INFO, challengeData);
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
        /// Thực thi yêu cầu đồng ý hoặc từ chối thách đấu với người chơi tương ứng
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
        public static TCPProcessCmdResults ResponseDoChallengeCommand(TCPManager tcpMgr, TMSKSocket socket, TCPClientPool tcpClientPool, TCPRandKey tcpRandKey, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;
            string strCmd;

            try
            {
                strCmd = new ASCIIEncoding().GetString(data, 0, count);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Error while getting DATA, CMD={0}, Client={1}", (TCPGameServerCmds) nID, Global.GetSocketRemoteEndPoint(socket)));
                return TCPProcessCmdResults.RESULT_FAILED;
            }

            try
            {
                string[] fields = strCmd.Split(':');
                /// Nếu số lượng gửi về không thỏa mãn
                if (fields.Length < 1)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Error Socket params count not fit CMD={0}, Client={1}, Recv={2}", (TCPGameServerCmds) nID, Global.GetSocketRemoteEndPoint(socket), fields.Length));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// Loại thao tác
                int type = int.Parse(fields[0]);

                /// Người chơi tương ứng gửi gói tin
                KPlayer client = KTPlayerManager.Find(socket);
                if (null == client)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Can not find player, CMD={0}, Client={1}", (TCPGameServerCmds) nID, Global.GetSocketRemoteEndPoint(socket)));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// Nếu không có dữ liệu thách đấu
                if (client.ChallengeInfo == null)
                {
                    KTPlayerManager.ShowNotification(client, "Bản thân không thách đấu ai, không thể thao tác!");
                    return TCPProcessCmdResults.RESULT_OK;
                }

                /// Nếu thằng kia không tồn tại
                if (client.ChallengeInfo.OpponentTeamLeader == null)
                {
                    KTPlayerManager.ShowNotification(client, "Bản thân không thách đấu ai, không thể thao tác!");
                    return TCPProcessCmdResults.RESULT_OK;
                }

                /// Nếu bản thân không ở trong khu vực được thách đấu
                if (!KTGlobal.IsInsideChallengeArea(client))
                {
                    KTPlayerManager.ShowNotification(client, "Khu vực này không thể thách đấu!");
                    return TCPProcessCmdResults.RESULT_OK;
                }

                /// Thằng kia
                KPlayer opponent = client.ChallengeInfo.OpponentTeamLeader;

                /// Nếu thằng kia không có dữ liệu thách đấu
                if (!opponent.IsChallenging())
                {
                    KTPlayerManager.ShowNotification(client, "Đối phương không thách đấu với bản thân, không thể thao tác!");
                    return TCPProcessCmdResults.RESULT_OK;
                }

                /// Nếu dữ liệu 2 thằng này khác nhau
                if (opponent.ChallengeInfo.OpponentTeamLeader != client)
                {
                    KTPlayerManager.ShowNotification(client, "Đối phương không thách đấu với bản thân, không thể thao tác!");
                    return TCPProcessCmdResults.RESULT_OK;
                }

                /// Nếu đối phương không ở trong khu vực được thách đấu
                if (!KTGlobal.IsInsideChallengeArea(opponent))
                {
                    KTPlayerManager.ShowNotification(client, "Đối phương không ở trong khu vực có thể thách đấu!");
                    return TCPProcessCmdResults.RESULT_OK;
                }

                /// Nếu bản thân dưới cấp 30
                if (client.m_Level < 30)
                {
                    KTPlayerManager.ShowNotification(client, "Dưới cấp 30 không thể thách đấu!");
                    return TCPProcessCmdResults.RESULT_OK;
                }
                /// Nếu đối phương dưới cấp 30
                if (opponent.m_Level < 30)
                {
                    KTPlayerManager.ShowNotification(opponent, "Đối phương dưới cấp 30, không thể thách đấu!");
                    return TCPProcessCmdResults.RESULT_OK;
                }

                /// Loại thao tác là gì
                switch (type)
                {
                    /// Hủy yêu cầu
                    case -1:
                    {
                        client.RemoveChallenge(true, false);
                        break;
                    }
                    /// Bắt đầu thách đấu
                    case 0:
                    {
                        /// Nếu bản thân chưa xác nhận
                        if (!client.ChallengeInfo.MyselfReady)
                        {
                            KTPlayerManager.ShowNotification(client, "Bản chưa xác nhận, không thể thao tác!");
                            return TCPProcessCmdResults.RESULT_OK;
                        }
                        /// Nếu đối phương chưa xác nhận
                        if (!opponent.ChallengeInfo.MyselfReady)
                        {
                            KTPlayerManager.ShowNotification(client, "Đối phương xác nhận, không thể thao tác!");
                            return TCPProcessCmdResults.RESULT_OK;
                        }

                        /// Nếu bản thân đã yêu cầu rồi
                        if (client.ChallengeInfo.MyselfFight)
                        {
                            KTPlayerManager.ShowNotification(client, "Đã gửi yêu cầu chiến đấu, chờ đối phương xác nhận!");
                            return TCPProcessCmdResults.RESULT_OK;
                        }

                        /// Thiết lập trạng thái của bản thân
                        client.ChallengeInfo.MyselfFight = true;

                        /// Nếu đối phương chưa yêu cầu chiến đấu
                        if (!opponent.ChallengeInfo.MyselfFight)
                        {
                            KTPlayerManager.ShowNotification(client, "Đã gửi yêu cầu chiến đấu, chờ đối phương xác nhận!");
                            return TCPProcessCmdResults.RESULT_OK;
                        }

                        /// Số tiền bản thân cược
                        int myselfMoney = client.ChallengeInfo.MyselfMoney;
                        /// Số tiền đối phương cược
                        int opponentMoney = opponent.ChallengeInfo.MyselfMoney;

                        /// Nếu số tiền cược đéo thỏa mãn
                        if (myselfMoney < 0)
                        {
                            KTPlayerManager.ShowNotification(client, "Số bạc bản thân đặt cược không thỏa mãn!");
                            KTPlayerManager.ShowNotification(opponent, "Số bạc đối phương đặt cược không thỏa mãn!");
                            return TCPProcessCmdResults.RESULT_OK;
                        }

                        /// Hủy thách đấu của bọn nó
                        client.RemoveChallenge(false, false);

                        /// Nếu bản thân có nhóm
                        if (client.TeamID != -1 && KTTeamManager.IsTeamExist(client.TeamID))
                        {
                            /// Duyệt danh sách
                            foreach (KPlayer teammate in client.Teammates)
                            {
                                /// Nếu thành viên của bản thân không nằm trong cùng bản đồ hoặc không ở trong khu vực thách đấu
                                if (teammate.CurrentMapCode != client.CurrentMapCode || !KTGlobal.IsInsideChallengeArea(teammate))
                                {
                                    KTPlayerManager.ShowNotification(client, string.Format("Thành viên đội [{0}] không ở trong khu vực thách đấu, tự hủy yêu cầu!", teammate.RoleName));
                                    KTPlayerManager.ShowNotification(opponent, string.Format("Thành viên nhóm đối phương [{0}] không ở trong khu vực thách đấu, tự hủy yêu cầu!", teammate.RoleName));
                                    return TCPProcessCmdResults.RESULT_OK;
                                }
                            }
                        }

                        /// Nếu đối phương có nhóm
                        if (opponent.TeamID != -1 && KTTeamManager.IsTeamExist(opponent.TeamID))
                        {
                            /// Duyệt danh sách
                            foreach (KPlayer opponentTeammate in opponent.Teammates)
                            {
                                /// Nếu thành viên của bản thân không nằm trong cùng bản đồ hoặc không ở trong khu vực thách đấu
                                if (opponentTeammate.CurrentMapCode != opponent.CurrentMapCode || !KTGlobal.IsInsideChallengeArea(opponentTeammate))
                                {
                                    KTPlayerManager.ShowNotification(opponent, string.Format("Thành viên đội [{0}] không ở trong khu vực thách đấu, tự hủy yêu cầu!", opponentTeammate.RoleName));
                                    KTPlayerManager.ShowNotification(client, string.Format("Thành viên nhóm đối phương [{0}] không ở trong khu vực thách đấu, tự hủy yêu cầu!", opponentTeammate.RoleName));
                                    return TCPProcessCmdResults.RESULT_OK;
                                }
                            }
                        }

                        /// Nếu có bạc
                        if (myselfMoney > 0 || opponentMoney > 0)
                        {
                            /// Thực hiện trừ bạc của cả 2 thằng
                            KTGlobal.SubMoney(client, myselfMoney, MoneyType.Bac, "Challenge_Bet");
                            KTGlobal.SubMoney(opponent, opponentMoney, MoneyType.Bac, "Challenge_Bet");
                        }

                        /// Nếu cả 2 cùng xác nhận rồi thì đánh
                        KTPlayerManager.ShowNotification(client, "Đang tiến vào đấu trường!");
                        KTPlayerManager.ShowNotification(opponent, "Đang tiến vào đấu trường!");

                        /// Danh sách người chơi đội 1
                        List<KPlayer> firstTeamPlayers = new List<KPlayer>();
                        /// Danh sách người chơi đội 2
                        List<KPlayer> secondTeamPlayers = new List<KPlayer>();

                        /// Nếu bản thân không có đội
                        if (client.TeamID == -1 || !KTTeamManager.IsTeamExist(client.TeamID))
                        {
                            /// Thêm chính mình vào
                            firstTeamPlayers.Add(client);
                        }
                        /// Nếu bản thân có đội
                        else
                        {
                            /// Thêm toàn bộ thành viên vào
                            firstTeamPlayers.AddRange(client.Teammates);
                        }

                        /// Nếu đối phương không có đội
                        if (opponent.TeamID == -1 || !KTTeamManager.IsTeamExist(opponent.TeamID))
                        {
                            /// Thêm chính mình vào
                            secondTeamPlayers.Add(opponent);
                        }
                        /// Nếu đối phương có đội
                        else
                        {
                            /// Thêm toàn bộ thành viên vào
                            secondTeamPlayers.AddRange(opponent.Teammates);
                        }

                        /// Nếu có cá cược
                        if (myselfMoney > 0 || opponentMoney > 0)
                        {
                            /// Duyệt danh sách đội 1
                            foreach (KPlayer player in firstTeamPlayers)
                            {
                                KTGlobal.SendDefaultChat(client, string.Format("Hệ thống tạm giữ <color=yellow>{0} Bạc</color> của chiến đội, và <color=yellow>{1} Bạc</color> của đối phương để cược thi đấu.", KTGlobal.GetDisplayMoney(myselfMoney), KTGlobal.GetDisplayMoney(opponentMoney)));
                            }

                            /// Duyệt danh sách đội 2
                            foreach (KPlayer player in secondTeamPlayers)
                            {
                                KTGlobal.SendDefaultChat(opponent, string.Format("Hệ thống tạm giữ <color=yellow>{0} Bạc</color> của chiến đội, và <color=yellow>{1} Bạc</color> của đối phương để cược thi đấu.", KTGlobal.GetDisplayMoney(opponentMoney), KTGlobal.GetDisplayMoney(myselfMoney)));
                            }
                        }

                        /// Bắt đầu đấu trường
                        DynamicArena_EventScript.Begin("Thách đấu", firstTeamPlayers, secondTeamPlayers, 300000, client.CurrentMapCode, client.PosX, client.PosY, (winnerTeamPlayer) =>
                        {
                            /// Nếu không có thằng nào thắng thì là hòa
                            if (winnerTeamPlayer == null)
                            {
                                /// Duyệt danh sách thành viên đội 1
                                foreach (KPlayer player in firstTeamPlayers)
                                {
                                    /// Nếu đã offline
                                    if (!player.IsOnline())
                                    {
                                        /// Bỏ qua
                                        continue;
                                    }
                                    /// Gửi tin nhắn
                                    KTGlobal.SendDefaultChat(player, "Thách đấu bất phân thắng bại, không bên nào được nhận lại tiền cược!");
                                }
                                /// Duyệt danh sách thành viên đội 2
                                foreach (KPlayer player in secondTeamPlayers)
                                {
                                    /// Nếu đã offline
                                    if (!player.IsOnline())
                                    {
                                        /// Bỏ qua
                                        continue;
                                    }
                                    /// Gửi tin nhắn
                                    KTGlobal.SendDefaultChat(player, "Thách đấu bất phân thắng bại, không bên nào được nhận lại tiền cược!");
                                }

                                /// Bỏ qua
                                return;
                            }

                            /// Nếu nằm trong đội 1
                            if (firstTeamPlayers.Contains(winnerTeamPlayer))
                            {
                                /// Trưởng nhóm thắng
                                KPlayer teamLeader;

                                /// Nếu thằng kia mất nhóm
                                if (winnerTeamPlayer.TeamID == -1 || !KTTeamManager.IsTeamExist(winnerTeamPlayer.TeamID))
                                {
                                    /// Nó làm nhóm trưởng
                                    teamLeader = winnerTeamPlayer;

                                    /// Nếu không cược gì
                                    if (myselfMoney == 0 && opponentMoney == 0)
                                    {
                                        /// Gửi tin nhắn
                                        KTGlobal.SendDefaultChat(teamLeader, "Chúc mừng chiến đội dành thắng lợi thách đấu của đối phương!");
                                    }
                                    /// Nếu có cược
                                    else
                                    {
                                        /// Gửi tin nhắn
                                        KTGlobal.SendDefaultChat(teamLeader, string.Format("Chúc mừng chiến đội dành thắng lợi thách đấu, thu về <color=yellow>{0} Bạc</color> tiền cọc ban đầu và <color=yellow>{1} Bạc</color> tiền cọc của đối phương!", KTGlobal.GetDisplayMoney(myselfMoney), KTGlobal.GetDisplayMoney(opponentMoney)));
                                    }
                                }
                                /// Nếu nó có nhóm
                                else
                                {
                                    /// Chuyển qua thằng trưởng nhóm mới
                                    teamLeader = winnerTeamPlayer.TeamLeader;

                                    /// Duyệt danh sách thành viên đội
                                    foreach (KPlayer teammate in winnerTeamPlayer.Teammates)
                                    {
                                        /// Nếu không cược gì
                                        if (myselfMoney == 0 && opponentMoney == 0)
                                        {
                                            /// Gửi tin nhắn
                                            KTGlobal.SendDefaultChat(teamLeader, "Chúc mừng chiến đội dành thắng lợi thách đấu của đối phương!");
                                        }
                                        /// Nếu có cược
                                        else
                                        {
                                            /// Gửi tin nhắn
                                            KTGlobal.SendDefaultChat(teammate, string.Format("Chúc mừng chiến đội dành thắng lợi thách đấu, thu về <color=yellow>{0} Bạc</color> tiền cọc ban đầu và <color=yellow>{1} Bạc</color> tiền cọc của đối phương, chuyển cho <color=#71b5f9>[{2}]</color>!", KTGlobal.GetDisplayMoney(myselfMoney), KTGlobal.GetDisplayMoney(opponentMoney), teamLeader.RoleName));
                                        }
                                    }
                                }

                                /// Đội 2 thất bại
                                foreach (KPlayer player in secondTeamPlayers)
                                {
                                    /// Nếu đã offline
                                    if (!player.IsOnline())
                                    {
                                        /// Bỏ qua
                                        continue;
                                    }

                                    /// Nếu có tiền cọc
                                    if (opponentMoney > 0)
                                    {
                                        /// Gửi tin nhắn
                                        KTGlobal.SendDefaultChat(player, string.Format("Chiến đội đã thất bại trong đợt thách đấu này. Tiền cọc <color=yellow>{0} Bạc</color> được chuyển về tay chiến đội thắng cuộc!", KTGlobal.GetDisplayMoney(opponentMoney)));
                                    }
                                    /// Nếu không có tiền cọc
                                    else
                                    {
                                        /// Gửi tin nhắn
                                        KTGlobal.SendDefaultChat(player, "Chiến đội đã thất bại trong đợt thách đấu này.");
                                    }
                                }

                                /// Thêm bạc cho thằng đội trưởng
                                KTGlobal.AddMoney(teamLeader, myselfMoney + opponentMoney, MoneyType.Bac, "Challenge_Bet");
                            }
                            /// Nếu nằm trong đội 2
                            else if (secondTeamPlayers.Contains(winnerTeamPlayer))
                            {
                                /// Trưởng nhóm thắng
                                KPlayer teamLeader;

                                /// Nếu thằng kia mất nhóm
                                if (winnerTeamPlayer.TeamID == -1 || !KTTeamManager.IsTeamExist(winnerTeamPlayer.TeamID))
                                {
                                    /// Nó làm nhóm trưởng
                                    teamLeader = winnerTeamPlayer;


                                    /// Nếu không cược gì
                                    if (myselfMoney == 0 && opponentMoney == 0)
                                    {
                                        /// Gửi tin nhắn
                                        KTGlobal.SendDefaultChat(teamLeader, "Chúc mừng chiến đội dành thắng lợi thách đấu của đối phương!");
                                    }
                                    /// Nếu có cược
                                    else
                                    {
                                        /// Gửi tin nhắn
                                        KTGlobal.SendDefaultChat(teamLeader, string.Format("Chúc mừng chiến đội dành thắng lợi thách đấu, thu về <color=yellow>{0} Bạc</color> tiền cọc ban đầu và <color=yellow>{1} Bạc</color> tiền cọc của đối phương!", KTGlobal.GetDisplayMoney(opponentMoney), KTGlobal.GetDisplayMoney(myselfMoney)));
                                    }
                                }
                                /// Nếu nó có nhóm
                                else
                                {
                                    /// Chuyển qua thằng trưởng nhóm mới
                                    teamLeader = winnerTeamPlayer.TeamLeader;

                                    /// Duyệt danh sách thành viên đội
                                    foreach (KPlayer teammate in winnerTeamPlayer.Teammates)
                                    {
                                        /// Nếu không cược gì
                                        if (myselfMoney == 0 && opponentMoney == 0)
                                        {
                                            /// Gửi tin nhắn
                                            KTGlobal.SendDefaultChat(teamLeader, "Chúc mừng chiến đội dành thắng lợi thách đấu của đối phương!");
                                        }
                                        /// Nếu có cược
                                        else
                                        {
                                            /// Gửi tin nhắn
                                            KTGlobal.SendDefaultChat(teammate, string.Format("Chúc mừng chiến đội dành thắng lợi thách đấu, thu về <color=yellow>{0} Bạc</color> tiền cọc ban đầu và <color=yellow>{1} Bạc</color> tiền cọc của đối phương, chuyển cho <color=#71b5f9>[{2}]</color>!", KTGlobal.GetDisplayMoney(opponentMoney), KTGlobal.GetDisplayMoney(myselfMoney), teamLeader.RoleName));
                                        }
                                    }
                                }

                                /// Đội 1 thất bại
                                foreach (KPlayer player in firstTeamPlayers)
                                {
                                    /// Nếu đã offline
                                    if (!player.IsOnline())
                                    {
                                        /// Bỏ qua
                                        continue;
                                    }

                                    /// Nếu có cọc
                                    if (myselfMoney > 0)
                                    {
                                        /// Gửi tin nhắn
                                        KTGlobal.SendDefaultChat(player, string.Format("Chiến đội đã thất bại trong đợt thách đấu này. Tiền cọc <color=yellow>{0} Bạc</color> được chuyển về tay chiến đội thắng cuộc!", KTGlobal.GetDisplayMoney(myselfMoney)));
                                    }
                                    /// Nếu không có cọc
                                    else
                                    {
                                        /// Gửi tin nhắn
                                        KTGlobal.SendDefaultChat(player, "Chiến đội đã thất bại trong đợt thách đấu này.");
                                    }
                                }

                                /// Thêm bạc cho thằng đội trưởng
                                KTGlobal.AddMoney(teamLeader, myselfMoney + opponentMoney, MoneyType.Bac, "Challenge_Bet");
                            }
                        });

                        break;
                    }
                    /// Thiết lập số tiền
                    case 1:
                    {
                        /// Nếu đã xác nhận rồi
                        if (client.ChallengeInfo.MyselfReady)
                        {
                            KTPlayerManager.ShowNotification(client, "Bạn đã xác nhận, không thể thao tác!");
                            return TCPProcessCmdResults.RESULT_OK;
                        }

                        /// Số tiền
                        int moneyAmount = int.Parse(fields[1]);


                        /// Toác
                        if (moneyAmount < 0)
                        {
                            KTPlayerManager.ShowNotification(client, "Số bạc đặt cược không hợp lệ!");
                            return TCPProcessCmdResults.RESULT_OK;
                        }
                        /// Nếu không đủ bạc
                        else if (!KTGlobal.IsHaveMoney(client, moneyAmount, MoneyType.Bac))
                        {
                            KTPlayerManager.ShowNotification(client, "Số bạc mang theo không đủ!");
                            return TCPProcessCmdResults.RESULT_OK;
                        }

                        /// Thiết lập số tiền
                        client.ChallengeInfo.MyselfMoney = moneyAmount;

                        /// Số tiền nhóm 1
                        int firstTeamMoney = client.ChallengeInfo.MyselfIndex == 1 ? client.ChallengeInfo.MyselfMoney : opponent.ChallengeInfo.MyselfMoney;
                        /// Số tiền nhóm 2
                        int secondTeamMoney = client.ChallengeInfo.MyselfIndex == 2 ? client.ChallengeInfo.MyselfMoney : opponent.ChallengeInfo.MyselfMoney;
                        /// Gửi lại thông báo cho cả mình và thằng kia
                        client.SendPacket(nID, string.Format("1:{0}:{1}", firstTeamMoney, secondTeamMoney));
                        opponent.SendPacket(nID, string.Format("1:{0}:{1}", firstTeamMoney, secondTeamMoney));

                        break;
                    }
                    /// Thiết lập trạng thái xác nhận
                    case 2:
                    {
                        /// Nếu đã xác nhận rồi
                        if (client.ChallengeInfo.MyselfReady)
                        {
                            KTPlayerManager.ShowNotification(client, "Bạn đã xác nhận, không thể thao tác!");
                            return TCPProcessCmdResults.RESULT_OK;
                        }

                        /// Thiết lập trạng thái xác nhận
                        client.ChallengeInfo.MyselfReady = true;

                        /// Trạng thái xác nhận nhóm 1
                        bool firstTeamReadyState = client.ChallengeInfo.MyselfIndex == 1 ? client.ChallengeInfo.MyselfReady : opponent.ChallengeInfo.MyselfReady;
                        /// Trạng thái xác nhận nhóm 2
                        bool secondTeamReadyState = client.ChallengeInfo.MyselfIndex == 2 ? client.ChallengeInfo.MyselfReady : opponent.ChallengeInfo.MyselfReady;
                        /// Gửi lại thông báo cho cả mình và thằng kia
                        client.SendPacket(nID, string.Format("2:{0}:{1}", firstTeamReadyState ? 1 : 0, secondTeamReadyState ? 1 : 0));
                        opponent.SendPacket(nID, string.Format("2:{0}:{1}", firstTeamReadyState ? 1 : 0, secondTeamReadyState ? 1 : 0));

                        break;
                    }
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
        /// Gửi thông báo hủy thách đấu
        /// </summary>
        /// <param name="player"></param>
        public static void SendStopChallenge(KPlayer player)
        {
            string strCmd = string.Format("{0}", 0);
            player.SendPacket((int) TCPGameServerCmds.CMD_KT_DO_CHALLENGE_COMMAND, strCmd);
        }

        #endregion thách đấu

        #region Trạng thái PK
        /// <summary>
        /// Thay đổi trạng thái PK
        /// </summary>
        /// <param name="pool"></param>
        /// <param name="nID"></param>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static TCPProcessCmdResults ProcessSpriteChangePKModeCmd(TCPManager tcpMgr, TMSKSocket socket, TCPClientPool tcpClientPool, TCPRandKey tcpRandKey, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
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

                int roleID = Convert.ToInt32(fields[0]);
                int pkMode = Convert.ToInt32(fields[1]);

                KPlayer client = KTPlayerManager.Find(socket);
                if (null == client || client.RoleID != roleID)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Can not find player corresponding ID, CMD={0}, Client={1}", (TCPGameServerCmds) nID, Global.GetSocketRemoteEndPoint(socket)));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// Bản đồ tương ứng
                GameMap gameMap = KTMapManager.Find(client.CurrentMapCode);

                /// Nếu bản đồ không cho phép đổi trạng thái PK
                if (gameMap != null && !gameMap.AllowChangePKStatus)
                {
                    KTPlayerManager.ShowNotification(client, "Bản đồ này không cho phép chủ động chuyển trạng thái PK!");
                    return TCPProcessCmdResults.RESULT_OK;
                }
                /// nếu có mật khẩu cấp 2
                if (client.NeedToShowInputSecondPassword())
                {
                    return TCPProcessCmdResults.RESULT_OK;
                }
               

                /// Nếu bản thân dưới cấp 30
                if (client.m_Level < 30)
                {
                    KTPlayerManager.ShowNotification(client, "Dưới cấp 30 không thể thay đổi trạng thái PK!");
                    return TCPProcessCmdResults.RESULT_OK;
                }

                /// Nếu trạng thái PK không hợp lý
                if (pkMode < (int) PKMode.Peace || pkMode >= (int) PKMode.Count || pkMode == (int) PKMode.Custom)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("PK mode changed faild, CMD={0}, Client={1}, RoleID={2}, ReceivedPKMode={3}", (TCPGameServerCmds) nID, Global.GetSocketRemoteEndPoint(socket), roleID, pkMode));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// Nếu trạng thái PK trùng với trạng thái hiện tại
                if (client.PKMode == pkMode)
                {
                    return TCPProcessCmdResults.RESULT_OK;
                }

                /// Nếu chuyển về trạng thái hòa bình
                if (pkMode == (int) PKMode.Peace)
                {
                    if (KTGlobal.GetCurrentTimeMilis() - client.LastChangePKModeToFight < KTGlobal.TimeCooldownChangingPKModeToFight)
                    {
                        long tickLeft = KTGlobal.TimeCooldownChangingPKModeToFight - KTGlobal.GetCurrentTimeMilis() + client.LastChangePKModeToFight;
                        tickLeft /= 1000;
                        /// Số phút còn lại
                        int nMinute = (int) tickLeft / 60;
                        int nSec = (int) tickLeft - nMinute * 60;
                        KTPlayerManager.ShowNotification(client, string.Format("Phải chờ {0}:{1} nữa mới có thể chuyển về trạng thái Luyện công!", nMinute, nSec));
                        return TCPProcessCmdResults.RESULT_OK;
                    }
                }
                else
                {
                    /// Cập nhật thời gian đổi trạng thái PK mới
                    client.LastChangePKModeToFight = KTGlobal.GetCurrentTimeMilis();
                }

                /// Cập nhật trạng thái PK
                client.PKMode = pkMode;

                ///// Lưu trạng thái PK vào DB
                //GameManager.DBCmdMgr.AddDBCmd((int) TCPGameServerCmds.CMD_DB_UPDATEPKMODE_CMD, cmdData, null, client.ServerId);

                return TCPProcessCmdResults.RESULT_OK;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, Global.GetDebugHelperInfo(socket), false);
            }

            return TCPProcessCmdResults.RESULT_FAILED;
        }

        /// <summary>
        /// Gửi gói tin thông báo tới tất cả người chơi xung quanh trạng thái PK của đối tượng thay đổi
        /// </summary>
        /// <param name="go"></param>
        public static void SendToOthersMyPKModeAndCampChanged(GameObject go)
        {
            try
            {
                string cmdData = string.Format("{0}:{1}:{2}", go.RoleID, (go is KPlayer) ? (go as KPlayer).PKMode : 0, go.Camp);

                /// Tìm tất cả người chơi xung quanh để gửi gói tin
                List<KPlayer> listObjects = KTRadarMapManager.GetPlayersAround(go);
                if (listObjects == null)
                {
                    return;
                }

                KTPlayerManager.SendPacketToPlayers((int) TCPGameServerCmds.CMD_SPR_CHGPKMODE, listObjects, cmdData, go, null);
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Exception, ex.ToString());
            }
        }
        #endregion

        #region Trị PK
        /// <summary>
        /// Gửi gói tin thông báo tới tất cả người chơi xung quanh trị PK của đối tượng thay đổi
        /// </summary>
        /// <param name="client"></param>
        public static void SendToOthersMyPKValueChanged(KPlayer client)
        {
            try
            {
                string cmdData = string.Format("{0}:{1}", client.RoleID, client.PKValue);

                /// Tìm tất cả người chơi xung quanh để gửi gói tin
                List<KPlayer> listObjects = KTRadarMapManager.GetPlayersAround(client);
                if (listObjects == null)
                {
                    return;
                }

                KTPlayerManager.SendPacketToPlayers((int) TCPGameServerCmds.CMD_SPR_CHGPKVAL, listObjects, cmdData, client, null);
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Exception, ex.ToString());
            }
        }

        /// <summary>
        /// Cập nhật trị PK của đối tượng vào DB
        /// </summary>
        /// <param name="client"></param>
        public static void UpdatePKValueToDB(KPlayer client)
        {
            try
            {

               
                Global.ExecuteDBCmd((int)TCPGameServerCmds.CMD_DB_UPDATEPKVAL_CMD, string.Format("{0}:{1}:{2}", client.RoleID, client.PKValue, 0), client.ServerId);
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Exception, ex.ToString());
            }
        }
        #endregion

        #region Tuyên chiến
        /// <summary>
        /// Thực thi yêu cầu tuyên chiến với người chơi tương ứng
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
        public static TCPProcessCmdResults ResponseActiveFight(TCPManager tcpMgr, TMSKSocket socket, TCPClientPool tcpClientPool, TCPRandKey tcpRandKey, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;
            string strCmd;

            try
            {
                strCmd = new ASCIIEncoding().GetString(data, 0, count);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Error while getting DATA, CMD={0}, Client={1}", (TCPGameServerCmds) nID, Global.GetSocketRemoteEndPoint(socket)));
                return TCPProcessCmdResults.RESULT_FAILED;
            }

            try
            {
                string[] fields = strCmd.Split(':');
                /// Nếu số lượng gửi về không thỏa mãn
                if (fields.Length != 1)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Error Socket params count not fit CMD={0}, Client={1}, Recv={2}", (TCPGameServerCmds) nID, Global.GetSocketRemoteEndPoint(socket), fields.Length));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// ID đối tượng tuyên chiến
                int targetID = int.Parse(fields[0]);

                /// Người chơi tương ứng gửi gói tin
                KPlayer client = KTPlayerManager.Find(socket);
                if (null == client)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Can not find player, CMD={0}, Client={1}", (TCPGameServerCmds) nID, Global.GetSocketRemoteEndPoint(socket)));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// Bản đồ tương ứng
                GameMap gameMap = KTMapManager.Find(client.CurrentMapCode);
                /// Nếu bản đồ không cho phép tuyên chiến
                if (gameMap != null && (!gameMap.AllowFightTarget || !gameMap.AllowPK))
                {
                    KTPlayerManager.ShowNotification(client, "Bản đồ này không cho phép người tuyên chiến lẫn nhau!");
                    return TCPProcessCmdResults.RESULT_OK;
                }

               

                /// Đối tượng tuyên chiến
                KPlayer target = KTPlayerManager.Find(targetID);
                /// Nếu không tìm thấy đối tượng
                if (target == null)
                {
                    KTPlayerManager.ShowNotification(client, "Đối phương không tồn tại hoặc đã rời mạng!");
                    return TCPProcessCmdResults.RESULT_OK;
                }

                /// Nếu đối tượng trùng với bản thân
                if (target == client)
                {
                    KTPlayerManager.ShowNotification(client, "Không thể tự tuyên chiến với bản thân!");
                    return TCPProcessCmdResults.RESULT_OK;
                }


                /// Nếu đối phương đã chết
                if (target.IsDead())
                {
                    KTPlayerManager.ShowNotification(client, "Đối phương đã tử nạn, không thể tuyên chiến!");
                    return TCPProcessCmdResults.RESULT_OK;
                }

                /// Nếu đang trong khu an toàn
                if (client.IsInsideSafeZone || target.IsInsideSafeZone)
                {
                    KTPlayerManager.ShowNotification(client, "Không thể chủ động tuyên chiến đối phương khi đang trong khu an toàn!");
                    return TCPProcessCmdResults.RESULT_OK;
                }

                /// Nếu đối phương ở bản đồ khác
                if (target.CurrentMapCode != client.CurrentMapCode)
                {
                    KTPlayerManager.ShowNotification(client, "Đối phương đang ở bản đồ khác, không thể tuyên chiến!");
                    return TCPProcessCmdResults.RESULT_OK;
                }

                /// Nếu cùng tổ đội
                if (target.TeamID != -1 && client.TeamID == target.TeamID)
				{
                    KTPlayerManager.ShowNotification(client, "Trong cùng nhóm không thể tuyên chiến lẫn nhau!");
                    return TCPProcessCmdResults.RESULT_OK;
                }

                /// Nếu bản thân dưới cấp 30
                if (client.m_Level < 30)
                {
                    KTPlayerManager.ShowNotification(client, "Dưới cấp 30 không thể đồ sát người khác!");
                    return TCPProcessCmdResults.RESULT_OK;
                }
                /// Nếu đối phương dưới cấp 30
                if (target.m_Level < 30)
                {
                    KTPlayerManager.ShowNotification(client, "Đối phương dưới cấp 30, không thể đồ sát!");
                    return TCPProcessCmdResults.RESULT_OK;
                }

                /// Bắt đầu tuyên chiến
                client.StartActiveFight(target);

                return TCPProcessCmdResults.RESULT_OK;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, Global.GetDebugHelperInfo(socket), false);
            }

            return TCPProcessCmdResults.RESULT_FAILED;
        }

        /// <summary>
        /// Gửi thông báo đối tượng tuyên chiến nhau
        /// </summary>
        /// <param name="player"></param>
        /// <param name="target"></param>
        public static void SendStartActiveFight(KPlayer player, KPlayer target)
        {
            try
            {
                string strCmd = string.Format("{0}:{1}", target.RoleID, target.RoleName);
                player.SendPacket((int) TCPGameServerCmds.CMD_KT_G2C_START_ACTIVEFIGHT, strCmd);
                if (target != null)
                {
                    string strCmd1 = string.Format("{0}:{1}", player.RoleID, player.RoleName);
                    target.SendPacket((int) TCPGameServerCmds.CMD_KT_G2C_START_ACTIVEFIGHT, strCmd1);
                }
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Exception, ex.ToString());
            }
        }

        /// <summary>
        /// Gửi thông báo kết thúc tuyên chiến
        /// </summary>
        /// <param name="player"></param>
        /// <param name="winner"></param>
        public static void SendStopActiveFight(KPlayer player, KPlayer target)
        {
            try
            {
                string strCmd = string.Format("{0}:{1}", target.RoleID, target.RoleName);
                player.SendPacket((int) TCPGameServerCmds.CMD_KT_G2C_STOP_ACTIVEFIGHT, strCmd);
                if (target != null)
                {
                    string strCmd1 = string.Format("{0}:{1}", player.RoleID, player.RoleName);
                    target.SendPacket((int) TCPGameServerCmds.CMD_KT_G2C_STOP_ACTIVEFIGHT, strCmd1);
                }
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Exception, ex.ToString());
            }
        }
        #endregion
    }
}
