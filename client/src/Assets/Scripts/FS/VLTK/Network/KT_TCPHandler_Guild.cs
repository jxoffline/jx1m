using FS.GameEngine.Logic;
using FS.GameEngine.Network;
using HSGameEngine.GameEngine.Network.Protocol;
using Server.Data;
using Server.Tools;
using System.Collections.Generic;
using System.Text;
using static FS.VLTK.Entities.Enum;

namespace FS.VLTK.Network
{
	/// <summary>
	/// Quản lý tương tác với Socket
	/// </summary>
	public static partial class KT_TCPHandler
	{
		#region Tạo bang
		/// <summary>
		/// Gửi yêu cầu tạo bang
		/// </summary>
		/// <param name="guildName"></param>
		public static void SendCreateGuild(string guildName)
		{
			string strCmd = string.Format("{0}", guildName);
			byte[] bytes = new ASCIIEncoding().GetBytes(strCmd);
			GameInstance.Game.GameClient.SendData(TCPOutPacket.MakeTCPOutPacket(GameInstance.Game.GameClient.OutPacketPool, bytes, 0, bytes.Length, (int) (TCPGameServerCmds.CMD_KT_GUILD_CREATE)));
		}

		/// <summary>
		/// Nhận thông báo kết quả tạo bang hội
		/// </summary>
		/// <param name="fields"></param>
		public static void ReceiveCreateGuild(string[] fields)
		{
			/// Kết quả trả về
			int ret = int.Parse(fields[0]);
			/// ID bang hội
			int guildID = int.Parse(fields[1]);
			/// Tên bang hội
			string guildName = fields[2];

			/// Nếu thành công
			if (ret == 0)
			{
				/// Thiết lập ID bang hội bản thân
				Global.Data.RoleData.GuildID = guildID;
				Global.Data.RoleData.GuildRank = (int) GuildRank.Master;
				/// Mở khung bang hội tổng quan
				KT_TCPHandler.SendGetGuildInfo();
			}
		}
		#endregion

		#region Lấy thông tin bang
		/// <summary>
		/// Gửi yêu cầu lấy thông tin bang hội của bản thân
		/// </summary>
		public static void SendGetGuildInfo()
		{
			string strCmd = string.Format("{0}:{1}", Global.Data.RoleData.RoleID, Global.Data.RoleData.GuildID);
			byte[] bytes = new ASCIIEncoding().GetBytes(strCmd);
			GameInstance.Game.GameClient.SendData(TCPOutPacket.MakeTCPOutPacket(GameInstance.Game.GameClient.OutPacketPool, bytes, 0, bytes.Length, (int) (TCPGameServerCmds.CMD_KT_GUILD_GETINFO)));
		}

		/// <summary>
		/// Nhận thông báo thông tin bang hội
		/// </summary>
		/// <param name="cmdID"></param>
		/// <param name="cmdData"></param>
		/// <param name="length"></param>
		public static void ReceiveGetGuildInfo(int cmdID, byte[] cmdData, int length)
		{
            GuildInfo guildInfo = DataHelper.BytesToObject<GuildInfo>(cmdData, 0, length);
			if (guildInfo == null)
			{
				KTGlobal.AddNotification("Có lỗi khi truy vấn thông tin bang hội, hãy thử lại!");
				return;
			}

			/// Nếu chưa mở khung bang hội tổng quan và khung cống hiến vật phẩm
			if (PlayZone.Instance.UIGuild == null && PlayZone.Instance.UIGuildDedication == null)
			{
                /// Mở khung bang hội tổng quan
                PlayZone.Instance.OpenUIGuild(guildInfo);
            }
			else
			{
				/// Nếu đang mở khung bang hội tổng quang
				if (PlayZone.Instance.UIGuild != null)
				{
					/// Cập nhật dữ liệu
					PlayZone.Instance.UIGuild.Data = guildInfo;
				}
				/// Nếu đang mở khung cống hiến
				if (PlayZone.Instance.UIGuildDedication != null)
				{
					/// Cập nhật dữ liệu
					PlayZone.Instance.UIGuildDedication.Data = guildInfo;
				}
			}
		}
		#endregion

		#region Danh sách thành viên
		/// <summary>
		/// Gửi yêu cầu truy vấn danh sách thành viên bang hội
		/// </summary>
		/// <param name="pageID"></param>
		public static void SendGetGuildMembers(int pageID)
		{
			string strCmd = string.Format("{0}:{1}:{2}", Global.Data.RoleData.RoleID, Global.Data.RoleData.GuildID, pageID);
			byte[] bytes = new ASCIIEncoding().GetBytes(strCmd);
			GameInstance.Game.GameClient.SendData(TCPOutPacket.MakeTCPOutPacket(GameInstance.Game.GameClient.OutPacketPool, bytes, 0, bytes.Length, (int) (TCPGameServerCmds.CMD_KT_GUILD_GETMEMBERLIST)));
		}

		/// <summary>
		/// Nhận thông báo truy vấn danh sách thành viên bang hội
		/// </summary>
		/// <param name="cmdID"></param>
		/// <param name="cmdData"></param>
		/// <param name="length"></param>
		public static void ReceiveGetGuildMembers(int cmdID, byte[] cmdData, int length)
		{
			GuildMemberData guildMemberData = DataHelper.BytesToObject<GuildMemberData>(cmdData, 0, length);
			if (guildMemberData == null)
			{
				KTGlobal.AddNotification("Có lỗi khi truy vấn danh sách thành viên bang hội, hãy thử lại!");
				return;
			}

			/// Nếu chưa mở khung
			if (PlayZone.Instance.UIGuildMemberList == null)
			{
				/// Mở khung thành viên bang hội
				PlayZone.Instance.OpenUIGuildMemberList(guildMemberData);
			}
			/// Nếu đã mở khung
			else
			{
				PlayZone.Instance.UIGuildMemberList.Data = guildMemberData;
			}
		}
        #endregion

        #region Thăng cấp bang hội
        /// <summary>
        /// Gửi yêu cầu thăng cấp bang hội
        /// </summary>
        public static void SendGuildLevelUp()
        {
            string strCmd = "";
            byte[] bytes = new ASCIIEncoding().GetBytes(strCmd);
            GameInstance.Game.GameClient.SendData(TCPOutPacket.MakeTCPOutPacket(GameInstance.Game.GameClient.OutPacketPool, bytes, 0, bytes.Length, (int) (TCPGameServerCmds.CMD_KT_GUILD_LEVELUP)));
        }
        #endregion

        #region Kỹ năng bang hội
        /// <summary>
        /// Gửi yêu cầu truy vấn danh sách kỹ năng bang hội
        /// </summary>
        /// <param name="pageID"></param>
        public static void SendGetGuildSkills()
        {
            string strCmd = string.Format("{0}:{1}", Global.Data.RoleData.RoleID, Global.Data.RoleData.GuildID);
            byte[] bytes = new ASCIIEncoding().GetBytes(strCmd);
            GameInstance.Game.GameClient.SendData(TCPOutPacket.MakeTCPOutPacket(GameInstance.Game.GameClient.OutPacketPool, bytes, 0, bytes.Length, (int) (TCPGameServerCmds.CMD_KT_GUILD_SKILL_LIST)));
        }

        /// <summary>
        /// Nhận thông báo truy vấn danh sách kỹ năng bang hội
        /// </summary>
        /// <param name="cmdID"></param>
        /// <param name="cmdData"></param>
        /// <param name="length"></param>
        public static void ReceiveGetGuildSkills(int cmdID, byte[] cmdData, int length)
        {
            List<SkillDef> guildSkills = DataHelper.BytesToObject<List<SkillDef>>(cmdData, 0, length);
            if (guildSkills == null)
            {
                //KTGlobal.AddNotification("Có lỗi khi truy vấn thông tin kỹ năng bang hội, hãy thử lại!");
                //return;
                guildSkills = new List<SkillDef>();
            }

            /// Nếu chưa mở khung
            if (PlayZone.Instance.UIGuildSkill == null)
            {
                /// Mở khung
                PlayZone.Instance.OpenUIGuildSkills(guildSkills);
            }
            /// Nếu đã mở khung
            else
            {
                PlayZone.Instance.UIGuildSkill.Data = guildSkills;
            }
        }
        #endregion

        #region Thăng cấp kỹ năng bang hội
        /// <summary>
        /// Gửi yêu cầu thăng cấp kỹ năng bang hội
        /// </summary>
        /// <param name="skillID"></param>
        public static void SendGuildSkillLevelUp(int skillID)
        {
            string strCmd = string.Format("{0}", skillID);
            byte[] bytes = new ASCIIEncoding().GetBytes(strCmd);
            GameInstance.Game.GameClient.SendData(TCPOutPacket.MakeTCPOutPacket(GameInstance.Game.GameClient.OutPacketPool, bytes, 0, bytes.Length, (int) (TCPGameServerCmds.CMD_KT_GUILD_SKILL_LEVELUP)));
        }
        #endregion

        #region Nhiệm vụ bang hội
        /// <summary>
        /// Gửi yêu cầu truy vấn danh sách nhiệm vụ bang hội
        /// </summary>
        /// <param name="pageID"></param>
        public static void SendGetGuildQuest()
        {
            string strCmd = string.Format("{0}:{1}", Global.Data.RoleData.RoleID, Global.Data.RoleData.GuildID);
            byte[] bytes = new ASCIIEncoding().GetBytes(strCmd);
            GameInstance.Game.GameClient.SendData(TCPOutPacket.MakeTCPOutPacket(GameInstance.Game.GameClient.OutPacketPool, bytes, 0, bytes.Length, (int) (TCPGameServerCmds.CMD_KT_GUILD_QUEST_LIST)));
        }

        /// <summary>
        /// Nhận thông báo truy vấn danh sách nhiệm vụ bang hội
        /// </summary>
        /// <param name="cmdID"></param>
        /// <param name="cmdData"></param>
        /// <param name="length"></param>
        public static void ReceiveGetGuildQuest(int cmdID, byte[] cmdData, int length)
        {
            GuildTask guildTask = DataHelper.BytesToObject<GuildTask>(cmdData, 0, length);
            if (guildTask == null)
            {
                KTGlobal.AddNotification("Có lỗi khi truy vấn thông tin nhiệm vụ bang hội, hãy thử lại!");
                return;
            }

            /// Nếu chưa mở khung
            if (PlayZone.Instance.UIGuildQuest == null)
            {
                /// Mở khung
                PlayZone.Instance.OpenUIGuildQuest(guildTask);
            }
            /// Nếu đã mở khung
            else
            {
                PlayZone.Instance.UIGuildQuest.Data = guildTask;
            }
        }
        #endregion

        #region Đổi hoặc bỏ nhiệm vụ
        /// <summary>
        /// Gửi yêu cầu đổi nhiệm vụ bang hội
        /// </summary>
        /// <param name="pageID"></param>
        public static void SendChangeGuildTask()
        {
            string strCmd = string.Format("{0}", 0);
            byte[] bytes = new ASCIIEncoding().GetBytes(strCmd);
            GameInstance.Game.GameClient.SendData(TCPOutPacket.MakeTCPOutPacket(GameInstance.Game.GameClient.OutPacketPool, bytes, 0, bytes.Length, (int) (TCPGameServerCmds.CMD_KT_GUILD_QUEST_CMD)));
        }

        /// <summary>
        /// Gửi yêu cầu hủy nhiệm vụ bang hội
        /// </summary>
        /// <param name="pageID"></param>
        public static void SendAbandonGuildTask()
        {
            string strCmd = string.Format("{0}", 1);
            byte[] bytes = new ASCIIEncoding().GetBytes(strCmd);
            GameInstance.Game.GameClient.SendData(TCPOutPacket.MakeTCPOutPacket(GameInstance.Game.GameClient.OutPacketPool, bytes, 0, bytes.Length, (int) (TCPGameServerCmds.CMD_KT_GUILD_QUEST_CMD)));
        }
        #endregion

        #region Thay đổi chức vị
        /// <summary>
        /// Gửi yêu cầu thay đổi chức vị thành viên
        /// </summary>
        /// <param name="roleID"></param>
        /// <param name="rank"></param>
        public static void SendChangeGuildMemberRank(int roleID, int rank)
		{
			string strCmd = string.Format("{0}:{1}", roleID, rank);
			byte[] bytes = new ASCIIEncoding().GetBytes(strCmd);
			GameInstance.Game.GameClient.SendData(TCPOutPacket.MakeTCPOutPacket(GameInstance.Game.GameClient.OutPacketPool, bytes, 0, bytes.Length, (int) (TCPGameServerCmds.CMD_KT_GUILD_CHANGERANK)));
		}

		/// <summary>
		/// Nhận thông báo chức vụ bản thân thay đổi
		/// </summary>
		/// <param name="fields"></param>
		public static void ReceiveChangeGuildMemberRank(string[] fields)
		{
            int roleID = int.Parse(fields[0]);
			int rank = int.Parse(fields[1]);

			/// Cập nhật chức vị bản thân
            if (roleID == Global.Data.RoleData.RoleID)
            {
                Global.Data.RoleData.GuildRank = rank;
            }
			
			/// Nếu đang mở khung
			if (PlayZone.Instance.UIGuildMemberList != null)
			{
				/// Đóng khung
				PlayZone.Instance.CloseUIGuildMemberList();

				/// Mở lại khung
				KT_TCPHandler.SendGetGuildMembers(1);
			}
		}
        #endregion

        #region Trục xuất thành viên
        /// <summary>
        /// Gửi yêu càu trục xuất thành viên
        /// </summary>
        /// <param name="roleID"></param>
        public static void SendGuildKickoutMember(int roleID)
		{
			string strCmd = string.Format("{0}:{1}", Global.Data.RoleData.GuildID, roleID);
			byte[] bytes = new ASCIIEncoding().GetBytes(strCmd);
			GameInstance.Game.GameClient.SendData(TCPOutPacket.MakeTCPOutPacket(GameInstance.Game.GameClient.OutPacketPool, bytes, 0, bytes.Length, (int) (TCPGameServerCmds.CMD_KT_GUILD_KICK_ROLE)));
		}

        /// <summary>
        /// Nhận thông báo trục xuất thành viên
        /// </summary>
        /// <param name="fields"></param>
        public static void ReceiveKickoutMember(string[] fields)
		{
            int roleID = int.Parse(fields[0]);
            /// Nếu là bản thân
            if (roleID == Global.Data.RoleData.RoleID)
            {
                /// Cập nhật bang hội bản thân
                Global.Data.RoleData.GuildID = 0;
                Global.Data.RoleData.GuildRank = (int) GuildRank.Member;
            }

			/// Nếu đang mở khung
			if (PlayZone.Instance.UIGuildMemberList != null)
			{
				/// Đóng khung
				PlayZone.Instance.CloseUIGuildMemberList();

				/// Mở lại khung
				KT_TCPHandler.SendGetGuildMembers(1);
			}
		}
        #endregion

        #region Cống hiến vào bang
        /// <summary>
        /// Gửi yêu cầu cống hiến bạc vào bang
        /// </summary>
        /// <param name="amount"></param>
        public static void SendDedicateMoneyToGuild(int amount)
        {
            string strCmd = string.Format("{0}:{1}", 0, amount);
            byte[] bytes = new ASCIIEncoding().GetBytes(strCmd);
            GameInstance.Game.GameClient.SendData(TCPOutPacket.MakeTCPOutPacket(GameInstance.Game.GameClient.OutPacketPool, bytes, 0, bytes.Length, (int) (TCPGameServerCmds.CMD_KT_GUILD_DONATE)));
        }

        /// <summary>
        /// Gửi yêu cầu cống vật phẩm vào bang
        /// </summary>
        /// <param name="items"></param>
        public static void SendDedicateItemsToGuild(Dictionary<int, int> items)
        {
			List<string> itemString = new List<string>();
			foreach (KeyValuePair<int, int> item in items)
			{
				itemString.Add(string.Format("{0}_{1}", item.Key, item.Value));
			}

            string strCmd = string.Format("{0}:{1}", 1, string.Join("|", itemString));
            byte[] bytes = new ASCIIEncoding().GetBytes(strCmd);
            GameInstance.Game.GameClient.SendData(TCPOutPacket.MakeTCPOutPacket(GameInstance.Game.GameClient.OutPacketPool, bytes, 0, bytes.Length, (int) (TCPGameServerCmds.CMD_KT_GUILD_DONATE)));
        }
		#endregion

		#region Thoát khỏi bang
		/// <summary>
		/// Gửi yêu cầu thoát khỏi bang hội
		/// </summary>
		public static void SendQuitGuild()
		{
			string strCmd = string.Format("{0}", Global.Data.RoleData.GuildID);
			byte[] bytes = new ASCIIEncoding().GetBytes(strCmd);
			GameInstance.Game.GameClient.SendData(TCPOutPacket.MakeTCPOutPacket(GameInstance.Game.GameClient.OutPacketPool, bytes, 0, bytes.Length, (int) (TCPGameServerCmds.CMD_KT_GUILD_QUIT)));
		}

        /// <summary>
        /// Nhận phản hồi thoát khỏi bang hội
        /// </summary>
        /// <param name="fields"></param>
        public static void ReceiveQuitGuild(string[] fields)
        {
            /// Nếu đã mở khung
            if (PlayZone.Instance.UIGuild != null)
            {
                PlayZone.Instance.CloseUIGuild();
            }
        }
        #endregion

        #region Thay đổi công cáo
        /// <summary>
        /// Gửi yêu cầu thay đổi công cáo bang
        /// </summary>
        /// <param name="slogan"></param>
        public static void SendChangeGuildNotification(string slogan)
		{
            GuildChangeSlogan changeSlogan = new GuildChangeSlogan()
			{
				Slogan = slogan,
				GuildID = Global.Data.RoleData.GuildID,
			};
			byte[] bytes = DataHelper.ObjectToBytes<GuildChangeSlogan>(changeSlogan);
			GameInstance.Game.GameClient.SendData(TCPOutPacket.MakeTCPOutPacket(GameInstance.Game.GameClient.OutPacketPool, bytes, 0, bytes.Length, (int) (TCPGameServerCmds.CMD_KT_GUILD_CHANGE_NOTIFY)));
		}

        /// <summary>
        /// Nhận thông báo sửa công cáo bang hội thành công
        /// </summary>
        /// <param name="cmdID"></param>
        /// <param name="cmdData"></param>
        /// <param name="length"></param>
        public static void ReceiveChangeGuildNotification(int cmdID, byte[] cmdData, int length)
		{
			GuildChangeSlogan changeSlogan = DataHelper.BytesToObject<GuildChangeSlogan>(cmdData, 0, length);
			if (changeSlogan == null)
			{
				return;
			}

			/// Nếu đang mở khung
			if (PlayZone.Instance.UIGuild != null)
			{
				PlayZone.Instance.UIGuild.Data.Data.Notification = changeSlogan.Slogan;
				PlayZone.Instance.UIGuild.UpdateNotification();
			}
		}
        #endregion

        #region Danh sách xin vào bang
        /// <summary>
        /// Gửi yêu cầu truy vấn danh sách người chơi xin vào bang hội
        /// </summary>
        /// <param name="pageID"></param>
        public static void SendGetAskToJoinList(int pageID)
        {
            string strCmd = string.Format("{0}:{1}:{2}", Global.Data.RoleData.RoleID, Global.Data.RoleData.GuildID, pageID);
            byte[] bytes = new ASCIIEncoding().GetBytes(strCmd);
            GameInstance.Game.GameClient.SendData(TCPOutPacket.MakeTCPOutPacket(GameInstance.Game.GameClient.OutPacketPool, bytes, 0, bytes.Length, (int) (TCPGameServerCmds.CMD_KT_GUILD_REQUEST_JOIN_LIST)));
        }

        /// <summary>
        /// Nhận thông báo truy vấn danh sách người chơi xin vào bang hội
        /// </summary>
        /// <param name="cmdID"></param>
        /// <param name="cmdData"></param>
        /// <param name="length"></param>
        public static void ReceiveGetAskToJoinList(int cmdID, byte[] cmdData, int length)
        {
            RequestJoinInfo requestJoinInfo = DataHelper.BytesToObject<RequestJoinInfo>(cmdData, 0, length);
            if (requestJoinInfo == null)
            {
                //KTGlobal.AddNotification("Có lỗi khi truy vấn danh sách người chơi xin vào bang hội, hãy thử lại!");
                //return;
                requestJoinInfo = new RequestJoinInfo();
            }

            /// Nếu chưa mở khung
            if (PlayZone.Instance.UIGuildAskToJoinList == null)
            {
                /// Mở khung thành viên bang hội
                PlayZone.Instance.OpenUIGuildAskToJoinList(requestJoinInfo);
            }
            /// Nếu đã mở khung
            else
            {
                PlayZone.Instance.UIGuildAskToJoinList.Data = requestJoinInfo;
            }
        }
        #endregion

        #region Phản hồi yêu cầu xin vào bang
        /// <summary>
        /// Gửi yêu cầu trả lời đơn xin vào bang
        /// </summary>
        /// <param name="response"></param>
        /// <param name="partnerRoleID"></param>
        public static void SendResponseAskToJoinGuildRequest(int response, int partnerRoleID)
		{
			string strCmd = string.Format("{0}:{1}:{2}", response, partnerRoleID, Global.Data.RoleData.GuildID);
			byte[] bytes = new ASCIIEncoding().GetBytes(strCmd);
			GameInstance.Game.GameClient.SendData(TCPOutPacket.MakeTCPOutPacket(GameInstance.Game.GameClient.OutPacketPool, bytes, 0, bytes.Length, (int) (TCPGameServerCmds.CMD_KT_GUILD_RESPONSEASK)));
		}
        #endregion

        #region Lưu thiết lập tự động duyệt người chơi vào bang
        /// <summary>
        /// Gửi yêu cầu thiết lập tự động duyệt người chơi vào bang
        /// </summary>
        /// <param name="paramString"></param>
        public static void SendSaveAutoAcceptJoinGuildSetting(string paramString)
        {
            string strCmd = paramString;
            byte[] bytes = new ASCIIEncoding().GetBytes(strCmd);
            GameInstance.Game.GameClient.SendData(TCPOutPacket.MakeTCPOutPacket(GameInstance.Game.GameClient.OutPacketPool, bytes, 0, bytes.Length, (int) (TCPGameServerCmds.CMD_KT_GUILD_AUTO_ACCPECT_SETTING)));
        }

        /// <summary>
        /// Nhận thông báo thiết lập tự động duyệt người chơi vào bang
        /// </summary>
        /// <param name="fields"></param>
        public static void ReceiveSaveAutoAcceptJoinGuildSetting(string[] fields)
        {
            /// Nếu đã mở khung
            if (PlayZone.Instance.UIGuildAskToJoinList != null)
            {
                PlayZone.Instance.UIGuildAskToJoinList.Data.AutoAccept = int.Parse(fields[3]);
                PlayZone.Instance.UIGuildAskToJoinList.Data.AutoAcceptRule = string.Join("|", fields[0], fields[1], fields[2]);
                PlayZone.Instance.UIGuildAskToJoinList.RefreshAutoAcceptRules();
            }
        }
        #endregion

        #region Xin vào bang
        /// <summary>
        /// Gửi yêu cầu xin vào bang hội
        /// </summary>
        /// <param name="guildID"></param>
        /// <param name="roleID"></param>
        public static void SendAskToJoinGuild(int guildID, int roleID)
        {
            string strCmd = string.Format("{0}:{1}", guildID, roleID);
            byte[] bytes = new ASCIIEncoding().GetBytes(strCmd);
            GameInstance.Game.GameClient.SendData(TCPOutPacket.MakeTCPOutPacket(GameInstance.Game.GameClient.OutPacketPool, bytes, 0, bytes.Length, (int) (TCPGameServerCmds.CMD_KT_GUILD_ASKJOIN)));
        }

        /// <summary>
        /// Nhận yêu cầu xin vào bang hội
        /// </summary>
        /// <param name="fields"></param>
        public static void ReceiveAskToJoinGuild(string[] fields)
        {
            /// Nếu bản thân chưa có bang
            if (Global.Data.RoleData.GuildID <= 0)
            {
                return;
            }
            /// Nếu bản thân không phải bang chủ hoặc phó bang chủ
            else if (Global.Data.RoleData.GuildRank != (int) GuildRank.Master && Global.Data.RoleData.GuildRank != (int) GuildRank.ViceMaster)
            {
                return;
            }

            /// ID người chơi xin vào
            int partnerRoleID = int.Parse(fields[0]);
            /// Tên người chơi xin vào
            string partnerRoleName = fields[1];
            /// Avarta người chơi xin vào
            int partnerAvartaID = int.Parse(fields[2]);
            /// Cấp độ người chơi xin vào
            int partnerLevel = int.Parse(fields[3]);
            /// Cấp độ người chơi xin vào
            int partnerFactionID = int.Parse(fields[4]);

            /// Tạo mới RoleData
            RoleDataMini rd = new RoleDataMini()
            {
                RoleID = partnerRoleID,
                RoleName = partnerRoleName,
                AvartaID = partnerAvartaID,
                Level = partnerLevel,
                FactionID = partnerFactionID,
            };
            /// Dữ liệu cũ
            Global.Data.GuildRequestJoinPlayers.RemoveAll(x => x.RoleID == partnerRoleID);
            /// Thêm vào dữ liệu
            Global.Data.GuildRequestJoinPlayers.Add(rd);

            /// Thông báo
            KTGlobal.AddNotification(string.Format("<color=#66daf4>[{0}]</color> muốn xin vào bang hội của bạn.", partnerRoleName));

            /// Làm mới dữ liệu khung
            PlayZone.Instance.UIGuildRequestList.RefreshData();
        }
        #endregion

        #region Mời vào bang
        /// <summary>
        /// Gửi lời mời vào bang tới người chơi tương ứng
        /// </summary>
        /// <param name="partnerRoleID"></param>
        public static void SendInviteToGuild(int partnerRoleID)
        {
            string strCmd = string.Format("{0}", partnerRoleID);
            byte[] bytes = new ASCIIEncoding().GetBytes(strCmd);
            GameInstance.Game.GameClient.SendData(TCPOutPacket.MakeTCPOutPacket(GameInstance.Game.GameClient.OutPacketPool, bytes, 0, bytes.Length, (int) (TCPGameServerCmds.CMD_KT_GUILD_INVITE)));
        }

        /// <summary>
        /// Nhận thông báo lời mời vào bang
        /// </summary>
        /// <param name="fields"></param>
        public static void ReceiveInviteToGuild(string[] fields)
        {
            /// Nếu bản thân đã có bang
            if (Global.Data.RoleData.GuildID > 0)
            {
                return;
            }

            /// ID người mời
            int inviterRoleID = int.Parse(fields[0]);
            /// Tên người mời
            string inviterRoleName = fields[1];
            /// ID bang
            int inviterGuildID = int.Parse(fields[2]);
            /// Tên bang
            string inviterGuildName = fields[3];
            /// Avarta người mời
            int inviterAvartaID = int.Parse(fields[4]);
            /// Cấp độ người mời
            int inviterLevel = int.Parse(fields[5]);
            /// Cấp độ người mời
            int inviterFactionID = int.Parse(fields[6]);


            /// Tạo mới RoleData
            RoleDataMini rd = new RoleDataMini()
            {
                RoleID = inviterRoleID,
                RoleName = inviterRoleName,
                AvartaID = inviterAvartaID,
                Level = inviterLevel,
                FactionID = inviterFactionID,
                GuildName = inviterGuildName,
                GuildID = inviterGuildID,
            };
            /// Dữ liệu cũ
            Global.Data.GuildInvitationPlayers.RemoveAll(x => x.RoleID == inviterRoleID);
            /// Thêm vào dữ liệu
            Global.Data.GuildInvitationPlayers.Add(rd);

            /// Thông báo
            KTGlobal.AddNotification(string.Format("<color=#66daf4>[{0}]</color> muốn mời bạn vào bang hội <color=yellow>[{1}]</color>.", inviterRoleName, inviterGuildName));

            /// Làm mới dữ liệu khung
            PlayZone.Instance.UIGuildRequestList.RefreshData();
        }

        /// <summary>
        /// Gửi phản hồi lời mời vào bang
        /// </summary>
        /// <param name="inviterRoleID"></param>
        /// <param name="inviterGuildID"></param>
        /// <param name="response"></param>
        public static void SendResponseInviteToGuild(int inviterRoleID, int inviterGuildID, int response)
        {
            string strCmd = string.Format("{0}:{1}:{2}", response, inviterRoleID, inviterGuildID);
            byte[] bytes = new ASCIIEncoding().GetBytes(strCmd);
            GameInstance.Game.GameClient.SendData(TCPOutPacket.MakeTCPOutPacket(GameInstance.Game.GameClient.OutPacketPool, bytes, 0, bytes.Length, (int) (TCPGameServerCmds.CMD_KT_GUILD_RESPONSEINVITE)));
        }
        #endregion

        #region Chức vị thay đổi
        /// <summary>
        /// Nhận thông báo chức vị bang hội của người chơi thay đổi
        /// </summary>
        /// <param name="fields"></param>
        public static void ReceiveRoleGuildRankChanged(string[] fields)
        {
            int roleID = int.Parse(fields[0]);
            int guildID = int.Parse(fields[1]);
            int guildRank = int.Parse(fields[2]);

            RoleData rd = null;
            /// Nếu là bản thân
            if (Global.Data.RoleData.RoleID == roleID)
            {
                rd = Global.Data.RoleData;
            }
            /// Nếu là người chơi khác
            else if (Global.Data.OtherRoles.TryGetValue(roleID, out rd))
            {

            }

            /// Nếu toác
            if (rd == null)
            {
                return;
            }

            /// Thiết lập dữ liệu
            rd.GuildID = guildID;
            rd.GuildRank = guildRank;
        }
        #endregion

        #region Danh sách bang hội
        /// <summary>
        /// Gửi yêu cầu truy vấn danh sách thành viên bang hội
        /// </summary>
        public static void SendGetGuildList()
        {
            string strCmd = "";
            byte[] bytes = new ASCIIEncoding().GetBytes(strCmd);
            GameInstance.Game.GameClient.SendData(TCPOutPacket.MakeTCPOutPacket(GameInstance.Game.GameClient.OutPacketPool, bytes, 0, bytes.Length, (int) (TCPGameServerCmds.CMD_KT_GUILD_OTHER_LIST)));
        }

        /// <summary>
        /// Nhận thông báo truy vấn danh sách thành viên bang hội
        /// </summary>
        /// <param name="cmdID"></param>
        /// <param name="cmdData"></param>
        /// <param name="length"></param>
        public static void ReceiveGetGuildList(int cmdID, byte[] cmdData, int length)
        {
            List<MiniGuildInfo> guildList = DataHelper.BytesToObject<List<MiniGuildInfo>>(cmdData, 0, length);
            if (guildList == null)
            {
                //KTGlobal.AddNotification("Có lỗi khi truy vấn danh sách bang hội, hãy thử lại!");
                //return;
                /// Tạo mới
                guildList = new List<MiniGuildInfo>();
            }

            /// Nếu chưa mở khung
            if (PlayZone.Instance.UIGuildList == null)
            {
                /// Mở khung thành viên bang hội
                PlayZone.Instance.OpenUIGuildList(guildList);
            }
            /// Nếu đã mở khung
            else
            {
                PlayZone.Instance.UIGuildList.Data = guildList;
            }
        }
        #endregion

        #region Xin vào bang hội thông qua danh sách bang hội tổng
        /// <summary>
        /// Gửi yêu cầu xin vào bang hội thông qua danh sách bang hội tổng
        /// </summary>
        /// <param name="guildID"></param>
        public static void SendRequestJoinGuild(int guildID)
        {
            string strCmd = string.Format("{0}:{1}", Global.Data.RoleData.RoleID, guildID);
            byte[] bytes = new ASCIIEncoding().GetBytes(strCmd);
            GameInstance.Game.GameClient.SendData(TCPOutPacket.MakeTCPOutPacket(GameInstance.Game.GameClient.OutPacketPool, bytes, 0, bytes.Length, (int) (TCPGameServerCmds.CMD_KT_GUILD_REQUEST_JOIN)));
        }
        #endregion

        #region Cửa hàng bang hội
        /// <summary>
        /// Gửi gói tin mở cửa hàng bang hội
        /// </summary>
        public static void SendOpenGuildShop()
        {
            string strCmd = "";
            byte[] bytes = new ASCIIEncoding().GetBytes(strCmd);
            GameInstance.Game.GameClient.SendData(TCPOutPacket.MakeTCPOutPacket(GameInstance.Game.GameClient.OutPacketPool, bytes, 0, bytes.Length, (int) (TCPGameServerCmds.CMD_KT_GUILD_OPENSHOP)));
        }
        #endregion

        #region Công thành chiến
        /// <summary>
        /// Gửi yêu cầu mở khung công thành chiến của bang hội
        /// </summary>
        public static void SendOpenGuildWar()
        {
            string strCmd = "";
            byte[] bytes = new ASCIIEncoding().GetBytes(strCmd);
            GameInstance.Game.GameClient.SendData(TCPOutPacket.MakeTCPOutPacket(GameInstance.Game.GameClient.OutPacketPool, bytes, 0, bytes.Length, (int) (TCPGameServerCmds.CMD_KT_GUILD_WARINFO)));
        }

        /// <summary>
        /// Nhận gói tin thông báo dữ liệu công thành chiến
        /// </summary>
        /// <param name="cmdID"></param>
        /// <param name="cmdData"></param>
        /// <param name="length"></param>
        public static void ReceiveGuildWarData(int cmdID, byte[] cmdData, int length)
        {
            GuildWarInfo data = DataHelper.BytesToObject<GuildWarInfo>(cmdData, 0, length);
            /// Toác
            if (data == null)
            {
                /// Bỏ qua
                return;
            }

            /// Nếu chưa mở khung
            if (PlayZone.Instance.UIGuildBattle == null)
            {
                PlayZone.Instance.OpenUIGuildBattle(data);
            }
            /// Nếu đã mở khung
            else
            {
                /// Đồng bộ dữ liệu
                PlayZone.Instance.UIGuildBattle.Data = data;
            }
        }

        /// <summary>
        /// Đăng ký tham gia công thành chiến
        /// </summary>
        /// <param name="roleIDs"></param>
        public static void SendRegisterGuildWar(params int[] roleIDs)
        {
            string strCmd = string.Format("{0}:{1}", Global.Data.RoleData.GuildID, string.Join(":", roleIDs));
            byte[] bytes = new ASCIIEncoding().GetBytes(strCmd);
            GameInstance.Game.GameClient.SendData(TCPOutPacket.MakeTCPOutPacket(GameInstance.Game.GameClient.OutPacketPool, bytes, 0, bytes.Length, (int) (TCPGameServerCmds.CMD_KT_GUILD_WAR_REGISTER)));
        }
        #endregion
    }
}
