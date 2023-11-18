using FS.GameEngine.Logic;
using FS.GameEngine.Network;
using FS.VLTK.Logic.Settings;
using HSGameEngine.GameEngine.Network.Protocol;
using Server.Data;
using Server.Tools;
using System.Collections.Generic;
using System.Text;

namespace FS.VLTK.Network
{
    /// <summary>
    /// Quản lý tương tác với Socket
    /// </summary>
    public static partial class KT_TCPHandler
    {
        #region Tổ đội
        /// <summary>
        /// Nhận gói tin thông báo có người chơi xin vào nhóm
        /// </summary>
        /// <param name="cmdID"></param>
        /// <param name="bytes"></param>
        /// <param name="length"></param>
        public static void ReceiveAskToJoinTeam(int cmdID, byte[] bytes, int length)
        {
            if (!Global.Data.PlayGame)
            {
                return;
            }

            RoleDataMini roleData = DataHelper.BytesToObject<RoleDataMini>(bytes, 0, length);
            UI.Main.UITeamFrame.ListAskingToJoinTeam[roleData.RoleID] = roleData;

            KTGlobal.AddNotification(string.Format("{0} muốn xin vào nhóm.", roleData.RoleName));

            if (PlayZone.Instance.UITeamFrame != null)
            {
                PlayZone.Instance.UITeamFrame.AddWaitingPlayer(roleData);
            }
        }

        /// <summary>
        /// Gửi gói tin xin vào nhóm của người chơi tương ứng
        /// </summary>
        /// <param name="roleID"></param>
        public static void SendAskToJoinTeam(int roleID)
        {
            string strCmd = string.Format("{0}", roleID);
            byte[] bytes = new ASCIIEncoding().GetBytes(strCmd);
            GameInstance.Game.GameClient.SendData(TCPOutPacket.MakeTCPOutPacket(GameInstance.Game.GameClient.OutPacketPool, bytes, 0, bytes.Length, (int) (TCPGameServerCmds.CMD_KT_ASKTOJOINTEAM)));
        }

        /// <summary>
        /// Nhận gói tin thông báo tổ đội của đối tượng thay đổi
        /// </summary>
        /// <param name="fields"></param>
        public static void ReceiveSpriteUpdateTeamInfo(string[] fields)
        {
            if (!Global.Data.PlayGame)
            {
                return;
            }

            /// ID người chơi
            int playerID = int.Parse(fields[0]);
            /// ID nhóm
            int teamID = int.Parse(fields[1]);
            /// ID đội trưởng
            int teamLeaderID = int.Parse(fields[2]);

            if (Global.Data.OtherRoles.TryGetValue(playerID, out RoleData roleData))
            {
                roleData.TeamID = teamID;
                roleData.TeamLeaderRoleID = teamLeaderID;
            }
        }

        /// <summary>
        /// Nhận gói tin tạo nhóm
        /// </summary>
        /// <param name="fields"></param>
        public static void ReceiveCreateTeam(string[] fields)
        {
            if (!Global.Data.PlayGame)
            {
                return;
            }

            /// ID nhóm
            int teamID = int.Parse(fields[0]);

            KTGlobal.AddNotification("Tạo nhóm thành công!");

            /// Cập nhật ID nhóm cho bản thân
            Global.Data.RoleData.TeamID = teamID;
            Global.Data.RoleData.TeamLeaderRoleID = Global.Data.RoleData.RoleID;

            if (PlayZone.Instance.UITeamFrame != null)
            {
                PlayZone.Instance.UITeamFrame.ServerCreateTeam();
            }

            if (PlayZone.Instance.UIMiniTaskAndTeamFrame != null)
            {
                PlayZone.Instance.UIMiniTaskAndTeamFrame.UITeamFrame.ServerCreateTeam();
            }

            /// Làm rỗng danh sách chờ thêm nhóm
            UI.Main.UITeamFrame.ListAskingToJoinTeam.Clear();

            /// Làm rỗng danh sách đội viên
            Global.Data.Teammates.Clear();

            /// Làm rỗng danh sách mời vào nhóm
            Global.Data.InvitedToTeamPlayers.Clear();
        }

        /// <summary>
        /// Gửi gói tin về Server thông báo tạo nhóm
        /// </summary>
        public static void SendCreateTeam()
        {
            string strCmd = string.Format("");
            byte[] bytes = new ASCIIEncoding().GetBytes(strCmd);
            GameInstance.Game.GameClient.SendData(TCPOutPacket.MakeTCPOutPacket(GameInstance.Game.GameClient.OutPacketPool, bytes, 0, bytes.Length, (int) (TCPGameServerCmds.CMD_KT_CREATETEAM)));
        }

        /// <summary>
        /// Nhận gói tin mời vào nhóm
        /// </summary>
        /// <param name="fields"></param>
        public static void ReceiveInviteToTeam(string[] fields)
        {
            if (!Global.Data.PlayGame)
            {
                return;
            }

            int teamID = int.Parse(fields[0]);
            int invitedPlayerID = int.Parse(fields[1]);
            string invitedPlayerName = fields[2];
            int invitedPlayerAvartaID = int.Parse(fields[3]);
            int invitedPlayerLevel = int.Parse(fields[4]);
            int invitedPlayerFactionID = int.Parse(fields[5]);

            /// Nếu từ chối tổ đội
            if (KTAutoAttackSetting.Config.General.RefuseTeam)
            {
                KT_TCPHandler.SendRefuseToJoinTeam(invitedPlayerID, -1, 0);
                return;
            }

            /// Tạo mới RoleData
            RoleDataMini rd = new RoleDataMini()
            {
                RoleID = invitedPlayerID,
                RoleName = invitedPlayerName,
                AvartaID = invitedPlayerAvartaID,
                TeamID = teamID,
                Level = invitedPlayerLevel,
                FactionID = invitedPlayerFactionID,
            };
            /// Dữ liệu cũ
            Global.Data.InvitedToTeamPlayers.RemoveAll(x => x.RoleID == invitedPlayerID);
            /// Thêm vào dữ liệu
            Global.Data.InvitedToTeamPlayers.Add(rd);

            /// Thông báo
            KTGlobal.AddNotification(string.Format("<color=#66daf4>[{0}]</color> muốn mời bạn vào tổ đội.", invitedPlayerName));

            /// Nếu có thiết lập tự đồng ý yêu cầu vào nhóm
            if (KTAutoAttackSetting.Config.PK.AutoAccectJoinTeam)
            {
                KT_TCPHandler.SendAgreeToJoinTeam(-1, teamID);
            }
            /// Nếu không mở Auto hoặc không có thiết lập tự đồng ý yêu cầu vào nhóm
            else
            {
                /// Làm mới dữ liệu khung
                PlayZone.Instance.UIInvitedToTeamList.RefreshData();
            }
        }

        /// <summary>
        /// Gửi gói tin về Server thông báo gửi lời mời vào nhóm tới người chơi tương ứng
        /// </summary>
        /// <param name="roleID"></param>
        public static void SendInviteToTeam(int roleID)
        {
            string strCmd = string.Format("{0}", roleID);
            byte[] bytes = new ASCIIEncoding().GetBytes(strCmd);
            GameInstance.Game.GameClient.SendData(TCPOutPacket.MakeTCPOutPacket(GameInstance.Game.GameClient.OutPacketPool, bytes, 0, bytes.Length, (int) (TCPGameServerCmds.CMD_KT_INVITETOTEAM)));
        }

        /// <summary>
        /// Nhận gói tin đồng ý vào nhóm
        /// </summary>
        /// <param name="cmdID"></param>
        /// <param name="bytes"></param>
        /// <param name="length"></param>
        public static void ReceiveAgreeToJoinTeam(int cmdID, byte[] bytes, int length)
        {
            if (!Global.Data.PlayGame)
            {
                return;
            }

            TeamInfo teamInfo = DataHelper.BytesToObject<TeamInfo>(bytes, 0, length);
            Global.Data.RoleData.TeamID = teamInfo.TeamID;
            Global.Data.RoleData.TeamLeaderRoleID = teamInfo.TeamLeaderID;

            KTGlobal.AddNotification("Bạn đã gia nhập nhóm thành công.");

            if (PlayZone.Instance.UITeamFrame != null)
            {
                PlayZone.Instance.UITeamFrame.RemoveAllTeammates();
                PlayZone.Instance.UITeamFrame.AddTeammates(teamInfo.Members);
            }

            if (PlayZone.Instance.UIMiniTaskAndTeamFrame != null)
            {
                PlayZone.Instance.UIMiniTaskAndTeamFrame.UITeamFrame.RemoveAllTeamMembers();
                PlayZone.Instance.UIMiniTaskAndTeamFrame.UITeamFrame.AddTeamMembers(teamInfo.Members);
            }

            /// Xóa đối tượng khỏi danh sách chờ thêm đội
            if (UI.Main.UITeamFrame.ListAskingToJoinTeam.TryGetValue(Global.Data.RoleData.RoleID, out _))
            {
                UI.Main.UITeamFrame.ListAskingToJoinTeam.Remove(Global.Data.RoleData.RoleID);
            }

            /// Làm rỗng danh sách đội viên
            Global.Data.Teammates.Clear();
            /// Thêm các thành viên tương ứng vào danh sách
            foreach (RoleDataMini teammate in teamInfo.Members)
            {
                Global.Data.Teammates.Add(teammate);
            }

            /// Làm rỗng danh sách mời vào nhóm
            Global.Data.InvitedToTeamPlayers.Clear();
        }

        /// <summary>
        /// Gửi gói tin về Server thông báo gửi lời mời vào nhóm tới người chơi tương ứng
        /// </summary>
        /// <param name="targetID"></param>
        /// <param name="teamID"></param>
        public static void SendAgreeToJoinTeam(int targetID, int teamID)
        {
            string strCmd = string.Format("{0}|{1}", targetID, teamID);
            byte[] bytes = new ASCIIEncoding().GetBytes(strCmd);
            GameInstance.Game.GameClient.SendData(TCPOutPacket.MakeTCPOutPacket(GameInstance.Game.GameClient.OutPacketPool, bytes, 0, bytes.Length, (int) (TCPGameServerCmds.CMD_KT_AGREEJOINTEAM)));
        }

        /// <summary>
        /// Nhận gói tin từ chối lời mời vào nhóm
        /// </summary>
        /// <param name="fields"></param>
        public static void ReceiveRefuseToJoinTeam(string[] fields)
        {
            if (!Global.Data.PlayGame)
            {
                return;
            }

            /// ID người chơi tương ứng gửi lời mời
            int fromPlayerID = int.Parse(fields[0]);
            /// ID người chơi tương ứng nhận lời mời
            int toPlayerID = int.Parse(fields[1]);
            /// Loại thao tác (0: Người chơi nhận từ chối lời mời, 1: Người chơi gửi từ chối lời xin vào nhóm
            int type = int.Parse(fields[2]);

            /// Nếu người gửi là bản thân
            if (Global.Data.RoleData.RoleID == fromPlayerID)
            {
                /// Nếu người chơi từ chối lời mời của bản thân
                if (type == 0)
                {
                    KTGlobal.AddNotification("Đối phương từ chối lời mời gia nhập tổ đội của bạn!");
                }
                else
                {
                    if (PlayZone.Instance.UITeamFrame != null)
                    {
                        PlayZone.Instance.UITeamFrame.RemoveWaitingPlayer(toPlayerID);
                    }
                    /// Xóa khỏi danh sách chờ
                    if (FS.VLTK.UI.Main.UITeamFrame.ListAskingToJoinTeam.TryGetValue(toPlayerID, out _))
                    {
                        FS.VLTK.UI.Main.UITeamFrame.ListAskingToJoinTeam.Remove(toPlayerID);
                    }
                }
            }
            /// Nếu người gửi không phải bản thân
            else if (Global.Data.RoleData.RoleID == toPlayerID)
            {
                /// Nếu chủ nhóm từ chối lời xin của bản thân
                if (type == 1)
                {
                    KTGlobal.AddNotification("Yêu cầu xin gia nhập nhóm của bạn bị trưởng nhóm cự tuyệt!");
                }
            }

            /// Xóa đối tượng khỏi danh sách chờ thêm đội
            if (UI.Main.UITeamFrame.ListAskingToJoinTeam.TryGetValue(toPlayerID, out _))
            {
                UI.Main.UITeamFrame.ListAskingToJoinTeam.Remove(toPlayerID);
            }
        }

        /// <summary>
        /// Gửi gói tin về Server thông báo từ chối lời mời vào nhóm
        /// </summary>
        /// <param name="fromPlayerID"></param>
        /// <param name="toPlayerID"></param>
        /// <param name="type"></param>
        public static void SendRefuseToJoinTeam(int fromPlayerID, int toPlayerID, int type)
        {
            string strCmd = string.Format("{0}|{1}|{2}", fromPlayerID, toPlayerID, type);
            byte[] bytes = new ASCIIEncoding().GetBytes(strCmd);
            GameInstance.Game.GameClient.SendData(TCPOutPacket.MakeTCPOutPacket(GameInstance.Game.GameClient.OutPacketPool, bytes, 0, bytes.Length, (int) (TCPGameServerCmds.CMD_KT_REFUSEJOINTEAM)));
        }

        /// <summary>
        /// Nhận gói tin lấy thông tin nhóm
        /// </summary>
        /// <param name="cmdID"></param>
        /// <param name="bytes"></param>
        /// <param name="length"></param>
        public static void ReceiveGetTeamInfo(int cmdID, byte[] bytes, int length)
        {
            if (!Global.Data.PlayGame)
            {
                return;
            }

            TeamInfo teamInfo = DataHelper.BytesToObject<TeamInfo>(bytes, 0, length);
            if (PlayZone.Instance.UITeamFrame == null)
            {
                PlayZone.Instance.ShowTeamFrame(teamInfo.Members);
            }
        }

        /// <summary>
        /// Gửi gói tin về Server lấy dữ liệu nhóm
        /// </summary>
        public static void SendGetTeamInfo()
        {
            string strCmd = string.Format("");
            byte[] bytes = new ASCIIEncoding().GetBytes(strCmd);
            GameInstance.Game.GameClient.SendData(TCPOutPacket.MakeTCPOutPacket(GameInstance.Game.GameClient.OutPacketPool, bytes, 0, bytes.Length, (int) (TCPGameServerCmds.CMD_KT_GETTEAMINFO)));
        }

        /// <summary>
        /// Nhận gói tin trục xuất thành viên khỏi nhóm
        /// </summary>
        /// <param name="cmdID"></param>
        /// <param name="bytes"></param>
        /// <param name="length"></param>
        public static void ReceiveKickOutTeammate(int cmdID, byte[] bytes, int length)
        {
            if (!Global.Data.PlayGame)
            {
                return;
            }

            TeamMemberChanged teamMember = DataHelper.BytesToObject<TeamMemberChanged>(bytes, 0, length);
            if (teamMember.Type != 0)
            {
                return;
            }

            /// Nếu đối tượng bị trục xuất là bản thân
            if (teamMember.RoleID == Global.Data.RoleData.RoleID)
            {
                KTGlobal.AddNotification(string.Format("Bạn bị trục xuất khỏi nhóm."));
                Global.Data.RoleData.TeamID = -1;
                Global.Data.RoleData.TeamLeaderRoleID = -1;

                /// Làm rỗng danh sách chờ vào nhóm
                UI.Main.UITeamFrame.ListAskingToJoinTeam.Clear();

                /// Làm rỗng danh sách đội viên
                Global.Data.Teammates.Clear();
            }
            else
            {
                KTGlobal.AddNotification(string.Format("{0} bị trục xuất khỏi nhóm.", teamMember.RoleName));
            }

            if (PlayZone.Instance.UITeamFrame != null)
            {
                /// Nếu đối tượng bị trục xuất là bản thân
                if (teamMember.RoleID == Global.Data.RoleData.RoleID)
                {
                    PlayZone.Instance.UITeamFrame.RemoveAllTeammates();
                }
                else
                {
                    PlayZone.Instance.UITeamFrame.RemoveTeammate(teamMember.RoleID);
                }
            }

            /// Nếu đối tượng bị trục xuất là bản thân
            if (teamMember.RoleID == Global.Data.RoleData.RoleID)
            {
                UI.Main.UITeamFrame.ListAskingToJoinTeam.Clear();
            }

            if (PlayZone.Instance.UIMiniTaskAndTeamFrame != null)
            {
                /// Nếu đối tượng bị trục xuất là bản thân
                if (teamMember.RoleID == Global.Data.RoleData.RoleID)
                {
                    PlayZone.Instance.UIMiniTaskAndTeamFrame.UITeamFrame.RemoveAllTeamMembers();
                }
                else
                {
                    PlayZone.Instance.UIMiniTaskAndTeamFrame.UITeamFrame.RemoveTeamMember(teamMember.RoleID);
                }
            }

            /// Xóa đối tượng khỏi danh sách chờ thêm đội
            if (UI.Main.UITeamFrame.ListAskingToJoinTeam.TryGetValue(teamMember.RoleID, out _))
            {
                UI.Main.UITeamFrame.ListAskingToJoinTeam.Remove(teamMember.RoleID);
            }

            /// Xóa thành viên tương ứng khỏi nhóm
            Global.Data.Teammates.RemoveAll(x => x.RoleID == teamMember.RoleID);
        }

        /// <summary>
        /// Gửi gói tin về Server thông báo trục xuất thành viên khỏi nhóm
        /// </summary>
        /// <param name="playerID"></param>
        public static void SendKickOutTeammate(int playerID)
        {
            string strCmd = string.Format("{0}", playerID);
            byte[] bytes = new ASCIIEncoding().GetBytes(strCmd);
            GameInstance.Game.GameClient.SendData(TCPOutPacket.MakeTCPOutPacket(GameInstance.Game.GameClient.OutPacketPool, bytes, 0, bytes.Length, (int) (TCPGameServerCmds.CMD_KT_KICKOUTTEAMMATE)));
        }

        /// <summary>
        /// Nhận gói tin bổ nhiệm làm đội trưởng
        /// </summary>
        /// <param name="fields"></param>
        public static void ReceiveApproveTeamLeader(string[] fields)
        {
            if (!Global.Data.PlayGame)
            {
                return;
            }

            /// ID đối tượng
            int roleID = int.Parse(fields[0]);
            /// Tên đối tượng
            string roleName = fields[1];

            /// Nếu đối tượng mới được bổ nhiệm là bản thân
            if (Global.Data.RoleData.RoleID == roleID)
            {
                KTGlobal.AddNotification("Bạn được bổ nhiệm làm đội trưởng.");
            }
            else
            {
                KTGlobal.AddNotification(string.Format("{0} được bổ nhiệm làm đội trưởng.", roleName));
            }

            /// Cập nhật ID đội trưởng
            Global.Data.RoleData.TeamLeaderRoleID = roleID;

            if (PlayZone.Instance.UITeamFrame != null)
            {
                PlayZone.Instance.UITeamFrame.ServerChangeTeamLeader(roleID);
            }

            if (PlayZone.Instance.UIMiniTaskAndTeamFrame != null)
            {
                PlayZone.Instance.UIMiniTaskAndTeamFrame.UITeamFrame.ServerChangeTeamLeader(roleID);
            }

            /// Duyệt danh sách đội viên và cập nhật đội trưởng
            foreach (RoleDataMini teammate in Global.Data.Teammates)
            {
                if (Global.Data.OtherRoles.TryGetValue(teammate.RoleID, out RoleData rd))
                {
                    rd.TeamLeaderRoleID = roleID;
                }
            }

            /// Làm rỗng danh sách chờ thêm nhóm
            UI.Main.UITeamFrame.ListAskingToJoinTeam.Clear();
        }

        /// <summary>
        /// Gửi gói tin về Server thông báo bổ nhiệm đội trưởng
        /// </summary>
        /// <param name="playerID"></param>
        public static void SendApproveTeamLeader(int playerID)
        {
            string strCmd = string.Format("{0}", playerID);
            byte[] bytes = new ASCIIEncoding().GetBytes(strCmd);
            GameInstance.Game.GameClient.SendData(TCPOutPacket.MakeTCPOutPacket(GameInstance.Game.GameClient.OutPacketPool, bytes, 0, bytes.Length, (int) (TCPGameServerCmds.CMD_KT_APPROVETEAMLEADER)));
        }

        /// <summary>
        /// Nhận gói tin cập nhật thông tin thuộc tính thành viên nhóm
        /// </summary>
        /// <param name="cmdID"></param>
        /// <param name="bytes"></param>
        /// <param name="length"></param>
        public static void ReceiveRefreshTeamMemberAttributes(int cmdID, byte[] bytes, int length)
        {
            if (!Global.Data.PlayGame)
            {
                return;
            }

            List<TeamMemberAttributes> teamMemberInfos = DataHelper.BytesToObject<List<TeamMemberAttributes>>(bytes, 0, length);

            /// Cập nhật thông tin đội viên tương ứng
            foreach (TeamMemberAttributes member in teamMemberInfos)
            {
                KTGlobal.UpdateTeammateAttributes(member);
            }

            if (PlayZone.Instance.UITeamFrame != null)
            {
                foreach (TeamMemberAttributes teamMember in teamMemberInfos)
                {
                    PlayZone.Instance.UITeamFrame.UpdateTeammateAttributes(teamMember);
                }
            }

            if (PlayZone.Instance.UIMiniTaskAndTeamFrame != null)
            {
                foreach (TeamMemberAttributes teamMember in teamMemberInfos)
                {
                    PlayZone.Instance.UIMiniTaskAndTeamFrame.UITeamFrame.UpdateTeammateAttributes(teamMember);
                }
            }

            /// Cập nhật LocalMap
            if (PlayZone.Instance.UILocalMap != null && PlayZone.Instance.UILocalMap.Visible)
            {
                PlayZone.Instance.UILocalMap.RefreshTeamMembers();
            }

            /*
            /// Làm rỗng danh sách đội viên
            Global.Data.Teammates.Clear();
            /// Thêm các đội viên tương ứng vào danh sách
            foreach (TeamMemberAttributes member in teamMemberInfos)
            {
                Global.Data.Teammates.Add(member.RoleID);
            }
            */
        }

        /// <summary>
        /// Nhận gói tin thông báo thêm thành viên mới hoặc có thành viên cũ rời đội
        /// </summary>
        /// <param name="cmdID"></param>
        /// <param name="bytes"></param>
        /// <param name="length"></param>
        public static void ReceiveTeamMemberChanged(int cmdID, byte[] bytes, int length)
        {
            if (!Global.Data.PlayGame)
            {
                return;
            }

            TeamMemberChanged teamMember = DataHelper.BytesToObject<TeamMemberChanged>(bytes, 0, length);

            /// Nếu có thành viên rời nhóm
            if (teamMember.Type == 0)
            {
                KTGlobal.AddNotification(string.Format("{0} đã rời nhóm.", teamMember.RoleName));

                if (PlayZone.Instance.UITeamFrame != null)
                {
                    PlayZone.Instance.UITeamFrame.RemoveTeammate(teamMember.RoleID);
                }

                if (PlayZone.Instance.UIMiniTaskAndTeamFrame != null)
                {
                    PlayZone.Instance.UIMiniTaskAndTeamFrame.UITeamFrame.RemoveTeamMember(teamMember.RoleID);
                }

                /// Xóa thành viên khỏi nhóm
                Global.Data.Teammates.RemoveAll(x => x.RoleID == teamMember.RoleID);
            }
            /// Nếu có thành viên mới vào nhóm
            else if (teamMember.Type == 1)
            {
                KTGlobal.AddNotification(string.Format("{0} đã gia nhập nhóm.", teamMember.RoleName));

                RoleDataMini rd = new RoleDataMini()
                {
                    RoleID = teamMember.RoleID,
                    TeamLeaderID = teamMember.TeamLeaderID,
                    MapCode = teamMember.MapCode,
                    PosX = teamMember.PosX,
                    PosY = teamMember.PosY,
                    HP = teamMember.HP,
                    MaxHP = teamMember.MaxHP,
                    ArmorID = teamMember.ArmorID,
                    HelmID = teamMember.HelmID,
                    WeaponID = teamMember.WeaponID,
                    MantleID = teamMember.MantleID,
                    WeaponEnhanceLevel = teamMember.WeaponEnhanceLevel,
                    AvartaID = teamMember.AvartaID,
                    FactionID = teamMember.FactionID,
                    Level = teamMember.Level,
                    RoleName = teamMember.RoleName,
                };

                if (PlayZone.Instance.UITeamFrame != null)
                {
                    PlayZone.Instance.UITeamFrame.AddTeammate(rd);

                    PlayZone.Instance.UITeamFrame.RemoveWaitingPlayer(teamMember.RoleID);
                }

                if (PlayZone.Instance.UIMiniTaskAndTeamFrame != null)
                {
                    PlayZone.Instance.UIMiniTaskAndTeamFrame.UITeamFrame.AddTeamMember(rd);
                }

                /// Thêm thành viên vào nhóm
                Global.Data.Teammates.Add(rd);
            }

            /// Xóa đối tượng khỏi danh sách chờ thêm đội
            if (UI.Main.UITeamFrame.ListAskingToJoinTeam.TryGetValue(teamMember.RoleID, out _))
            {
                UI.Main.UITeamFrame.ListAskingToJoinTeam.Remove(teamMember.RoleID);
            }
        }

        /// <summary>
        /// Nhận gói tin thông báo bản thân rời nhóm
        /// </summary>
        /// <param name="fields"></param>
        public static void ReceiveLeaveTeam(string[] fields)
        {
            if (!Global.Data.PlayGame)
            {
                return;
            }

            Global.Data.RoleData.TeamID = -1;
            Global.Data.RoleData.TeamLeaderRoleID = -1;

            KTGlobal.AddNotification("Bạn đã rời nhóm.");

            if (PlayZone.Instance.UITeamFrame != null)
            {
                PlayZone.Instance.UITeamFrame.RemoveAllTeammates();
            }

            if (PlayZone.Instance.UIMiniTaskAndTeamFrame != null)
            {
                PlayZone.Instance.UIMiniTaskAndTeamFrame.UITeamFrame.RemoveAllTeamMembers();
            }

            /// Xóa đối tượng khỏi danh sách chờ thêm đội
            if (UI.Main.UITeamFrame.ListAskingToJoinTeam.TryGetValue(Global.Data.RoleData.RoleID, out _))
            {
                UI.Main.UITeamFrame.ListAskingToJoinTeam.Remove(Global.Data.RoleData.RoleID);
            }

            /// Làm rỗng danh sách đội viên
            Global.Data.Teammates.Clear();
        }

        /// <summary>
        /// Gửi gói tin về Server thông báo bản thân rời nhóm
        /// </summary>
        public static void SendLeaveTeam()
        {
            string strCmd = string.Format("");
            byte[] bytes = new ASCIIEncoding().GetBytes(strCmd);
            GameInstance.Game.GameClient.SendData(TCPOutPacket.MakeTCPOutPacket(GameInstance.Game.GameClient.OutPacketPool, bytes, 0, bytes.Length, (int) (TCPGameServerCmds.CMD_KT_LEAVETEAM)));
        }
        #endregion
    }
}
