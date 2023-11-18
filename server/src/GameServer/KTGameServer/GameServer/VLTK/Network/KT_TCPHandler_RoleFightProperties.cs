using GameServer.KiemThe.Entities;
using GameServer.Logic;
using GameServer.Logic.ProtoCheck;
using GameServer.Server;
using GameServer.Tools;
using Server.Data;
using Server.Protocol;
using Server.TCP;
using Server.Tools;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameServer.KiemThe.Logic
{
    /// <summary>
    /// Quản lý chỉ số nhân vật
    /// </summary>
    public static partial class KT_TCPHandler
    {
        #region Cộng điểm tiềm năng
        /// <summary>
        /// Hàm khuyến nghị cộng điểm chưa rõ ở CLIENT sẽ khuyến nghị kiểu gì
        /// </summary>
        /// <param name="pool"></param>
        /// <param name="nID"></param>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static TCPProcessCmdResults ProcessExecuteRecommendPropAddPointCmd(TCPManager tcpMgr, TMSKSocket socket, TCPClientPool tcpClientPool, TCPRandKey tcpRandKey, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;
            CSPropAddPoint cmdData = null;

            try
            {
                ProtoChecker.Instance().Check<CSPropAddPoint>(data, 0, count, socket.m_Socket);

                if (!CheckHelper.CheckTCPCmdHandle<CSPropAddPoint>(socket, nID, data, count, out cmdData))
                    return TCPProcessCmdResults.RESULT_FAILED;

                int roleID = cmdData.RoleID;
                int nStrengthPoint = cmdData.Strength;
                int nIntelligencePoint = cmdData.Intelligence;
                int nDexterityPoint = cmdData.Dexterity;
                int nConstitutionPoint = cmdData.Constitution;

                long nPoint = (long)nStrengthPoint + (long)nIntelligencePoint + (long)nDexterityPoint + (long)nConstitutionPoint;

                KPlayer client = KTPlayerManager.Find(socket);
                if (null == client || client.RoleID != roleID)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Can not find player corresponding ID, CMD={0}, Client={1}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket)));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                string strcmd = "";
                if (nStrengthPoint < 0 || nIntelligencePoint < 0 || nDexterityPoint < 0 || nConstitutionPoint < 0 || nPoint <= 0)
                {
                    strcmd = string.Format("{0}:{1}", -1, roleID);
                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, strcmd, nID);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                /// Điểm tiềm năng hiện có
                int nRemainPoint = client.GetRemainPotential();

                if (nRemainPoint < nPoint)
                {
                    strcmd = string.Format("{0}:{1}", -1, roleID);
                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, strcmd, nID);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                /// Cộng điểm
                bool ret = client.AssignPotential(nStrengthPoint, nDexterityPoint, nConstitutionPoint, nIntelligencePoint);

                /// Thông báo tiềm năng về Client
                KT_TCPHandler.NotifyAttribute(client, false);

                /// Gói tin gửi về
                strcmd = string.Format("{0}:{1}", ret ? 1 : -1, roleID);
                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, strcmd, nID);

                return TCPProcessCmdResults.RESULT_DATA;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, Global.GetDebugHelperInfo(socket), false);
            }

            return TCPProcessCmdResults.RESULT_FAILED;
        }
        #endregion

        #region Đối tượng chết
        /// <summary>
        /// Gửi gói tin đối tượng chết về Client
        /// </summary>
        /// <param name="go"></param>
        public static void SendObjectDiedToClients(GameObject go)
        {
            try
            {
                string strCmd = string.Format("{0}", go.RoleID);
                byte[] cmdData = new ASCIIEncoding().GetBytes(strCmd);

                /// Tìm tất cả người chơi xung quanh để gửi gói tin
                List<KPlayer> listObjects = KTRadarMapManager.GetPlayersAround(go);
                if (listObjects == null)
                {
                    return;
                }
                /// Gửi gói tin đến tất cả người chơi xung quanh
                KTPlayerManager.SendPacketToPlayers((int) TCPGameServerCmds.CMD_SPR_DEAD, listObjects, cmdData, go, null);
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Exception, ex.ToString());
            }
        }
        #endregion

        #region Chỉ số nhân vật

        /// <summary>
        /// Lấy thông tin chỉ số nhân vật
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
        public static TCPProcessCmdResults GetRoleAttributes(TCPManager tcpMgr, TMSKSocket socket, TCPClientPool tcpClientPool, TCPRandKey tcpRandKey, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;
            string cmdData = null;

            try
            {
                cmdData = new UTF8Encoding().GetString(data, 0, count);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Lỗi khi phân giải DATA, CMD={0}, Client={1}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket)));
                return TCPProcessCmdResults.RESULT_FAILED;
            }

            try
            {
                string[] fields = cmdData.Split(':');
                if (fields.Length != 2)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Error Socket params count not fit CMD={0}, Client={1}, Recv={2}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), fields.Length));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                int roleID = int.Parse(fields[0]);
                int skillId = int.Parse(fields[1]);

                KPlayer client = KTPlayerManager.Find(socket);

                if (null == client || client.RoleID != roleID)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Không tìm thấy thông tin người chơi, CMD={0}, Client={1}, RoleID={2}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), roleID));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// Nếu có kỹ năng
                if (client.Skills.HasSkill(skillId))
                {
                    client.LastMainSkillID = skillId;
                }

                RoleAttributes _Atrtribute = client.GetRoleAttributes();
                KeyValuePair<RoleAttributes, bool> pairData = new KeyValuePair<RoleAttributes, bool>(_Atrtribute, true);

                byte[] rspData = DataHelper.ObjectToBytes(pairData);

                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, rspData, nID);

                return TCPProcessCmdResults.RESULT_DATA;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, Global.GetDebugHelperInfo(socket), false);
            }

            return TCPProcessCmdResults.RESULT_FAILED;
        }

        /// <summary>
        /// Thông báo về Client chỉ số nhân vật
        /// </summary>
        /// <param name="sl"></param>
        /// <param name="pool"></param>
        /// <param name="client"></param>
        public static void NotifyAttribute(KPlayer client, bool isShowUI)
        {
            try
            {
                RoleAttributes _Atrtribute = client.GetRoleAttributes();
                KeyValuePair<RoleAttributes, bool> pairData = new KeyValuePair<RoleAttributes, bool>(_Atrtribute, isShowUI);
                byte[] cmdData = DataHelper.ObjectToBytes(pairData);
                client.SendPacket((int) TCPGameServerCmds.CMD_KT_ROLE_ATRIBUTES, cmdData);
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Exception, ex.ToString());
            }
        }

        /// <summary>
        /// Thông báo giá trị Sinh lực, nội lực, thể lực của bản thân thay đổi
        /// </summary>
        /// <param name="player"></param>
        /// <param name="hp"></param>
        public static void NotifySelfLifeChanged(KPlayer player, int hp)
        {
            try
            {
                SpriteLifeChangeData lifeChangeData = new SpriteLifeChangeData();

                lifeChangeData.RoleID = player.RoleID;
                lifeChangeData.MaxHP = player.m_CurrentLifeMax;
                lifeChangeData.MaxMP = player.m_CurrentManaMax;
                lifeChangeData.MaxStamina = player.m_CurrentStaminaMax;
                //lifeChangeData.HP = player.m_CurrentLife;
                lifeChangeData.HP = hp;
                lifeChangeData.MP = player.m_CurrentMana;
                lifeChangeData.Stamina = player.m_CurrentStamina;

                byte[] cmdData = DataHelper.ObjectToBytes<SpriteLifeChangeData>(lifeChangeData);
                player.SendPacket((int) TCPGameServerCmds.CMD_SPR_UPDATE_ROLEDATA, cmdData);
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Exception, ex.ToString());
            }
        }

        /// <summary>
        /// Thông báo giá trị Sinh lực của đối tượng thay đổi
        /// </summary>
        /// <param name="go"></param>
        /// <param name="hp"></param>
        public static void NotifyObjectLifeChangedToAllPlayers(GameObject go, int hp)
        {
            try
            {
                SpriteLifeChangeData lifeChangeData = new SpriteLifeChangeData
                {
                    RoleID = go.RoleID,
                    MaxHP = go.m_CurrentLifeMax,
                    //HP = go.m_CurrentLife,
                    HP = hp,
                    MP = 0,
                    MaxMP = 0,
                    Stamina = 0,
                    MaxStamina = 0,
                };

                byte[] cmdData = DataHelper.ObjectToBytes<SpriteLifeChangeData>(lifeChangeData);

                /// Tìm tất cả người chơi xung quanh để gửi gói tin
                List<KPlayer> listObjects = KTRadarMapManager.GetPlayersAround(go);
                if (listObjects == null)
                {
                    return;
                }
                /// Gửi gói tin đến tất cả người chơi xung quanh
                KTPlayerManager.SendPacketToPlayers((int) TCPGameServerCmds.CMD_SPR_UPDATE_ROLEDATA, listObjects, cmdData, go, go);
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Exception, ex.ToString());
            }
        }

        /// <summary>
        /// Thông báo giá trị Tinh lực, hoạt lực của bản thân thay đổi
        /// </summary>
        /// <param name="player"></param>
        public static void NotifySelfGatherMakePointChanged(KPlayer player)
        {
            try
            {
                string strCmd = string.Format("{0}:{1}", player.GetGatherPoint(), player.GetMakePoint());
                byte[] cmdData = new ASCIIEncoding().GetBytes(strCmd);
                player.SendPacket((int) TCPGameServerCmds.CMD_KT_G2C_UPDATE_ROLE_GATHERMAKEPOINT, cmdData);
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Exception, ex.ToString());
            }
        }

        /// <summary>
        /// Thông báo cấp độ và giá trị kinh nghiệm của kỹ năng sống tương ứng của bản thân
        /// </summary>
        /// <param name="player"></param>
        /// <param name="id"></param>
        /// <param name="level"></param>
        /// <param name="exp"></param>
        public static void NotifySelfLifeSkillLevelAndExpChanged(KPlayer player, int id, int level, int exp)
        {
            try
            {
                string strCmd = string.Format("{0}:{1}:{2}", id, level, exp);
                byte[] cmdData = new ASCIIEncoding().GetBytes(strCmd);
                player.SendPacket((int) TCPGameServerCmds.CMD_KT_G2C_UPDATE_LIFESKILL_LEVEL, cmdData);
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Exception, ex.ToString());
            }
        }

        #endregion Chỉ số nhân vật

        #region Vinh dự và danh vọng
        /// <summary>
        /// Thông báo điểm tài phú tương ứng của bản thân thay đổi
        /// </summary>
        /// <param name="player"></param>
        /// <param name="id"></param>
        /// <param name="level"></param>
        /// <param name="exp"></param>
        public static void NotifySelfRoleValueChanged(KPlayer player, long value)
        {
            try
            {
                string strCmd = string.Format("{0}:{1}", player.RoleID, value);
                byte[] cmdData = new ASCIIEncoding().GetBytes(strCmd);

                /// Tìm tất cả người chơi xung quanh để gửi gói tin
                List<KPlayer> listObjects = KTRadarMapManager.GetPlayersAround(player);
                if (listObjects == null)
                {
                    return;
                }
                /// Gửi gói tin đến tất cả người chơi xung quanh
                KTPlayerManager.SendPacketToPlayers((int) TCPGameServerCmds.CMD_KT_UPDATE_TOTALVALUE, listObjects, cmdData, player, null);
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Exception, ex.ToString());
            }
        }
        #endregion

        #region Thông tin người chơi khác
        /// <summary>
        /// Phản hồi yêu cầu kiểm tra thông tin trang bị người chơi khác
        /// </summary>
        /// <param name="pool"></param>
        /// <param name="nID"></param>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static TCPProcessCmdResults ResponseGetOtherRoleEquipInfo(TCPManager tcpMgr, TMSKSocket socket, TCPClientPool tcpClientPool, TCPRandKey tcpRandKey, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;
            string cmdData = null;

            try
            {
                cmdData = new UTF8Encoding().GetString(data, 0, count);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Decode data faild, CMD={0}, Client={1}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket)));
                return TCPProcessCmdResults.RESULT_FAILED;
            }

            try
            {
                string[] fields = cmdData.Split(':');
                if (fields.Length != 1)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Error Socket params count not fit CMD={0}, Client={1}, Recv={2}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), fields.Length));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                int ohterRoleID = Convert.ToInt32(fields[0]);

                /// Thông tin người chơi chủ nhân gói tin
                KPlayer client = KTPlayerManager.Find(socket);
                if (null == client)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Can not find player corresponding ID, CMD={0}, Client={1}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket)));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// Người chơi khác
                KPlayer otherClient = KTPlayerManager.Find(ohterRoleID);
                /// Nếu không tồn tại
                if (otherClient == null)
                {
                    KTPlayerManager.ShowNotification(client, "Người chơi không tồn tại hoặc đã rời mạng!");
                    return TCPProcessCmdResults.RESULT_OK;
                }

                /// Thông tin
                RoleData roleData = KTGlobal.GetOtherPlayerRoleData(otherClient);
                byte[] _cmdData = DataHelper.ObjectToBytes<RoleData>(roleData);
                /// Gửi gói tin lại cho người chơi
                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, _cmdData, nID);
                return TCPProcessCmdResults.RESULT_DATA;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, Global.GetDebugHelperInfo(socket), false);
            }

            return TCPProcessCmdResults.RESULT_FAILED;
        }
        #endregion

        #region Bang hội chức vụ
        /// <summary>
        /// Gửi gói tin thông báo chức vụ bang hội đối tượng thay đổi tới tất cả người chơi xung quanh
        /// </summary>
        /// <param name="player"></param>
        public static void NotifyOtherMyGuildRankChanged(KPlayer player)
        {
            try
            {
                string strCmd = string.Format("{0}:{1}:{2}", player.RoleID, player.GuildID, player.GuildRank);

                /// Tìm tất cả người chơi xung quanh để gửi gói tin
                List<KPlayer> listObjects = KTRadarMapManager.GetPlayersAround(player);
                if (listObjects == null)
                {
                    return;
                }
                /// Gửi gói tin đến tất cả người chơi xung quanh
                KTPlayerManager.SendPacketToPlayers((int) TCPGameServerCmds.CMD_KT_UPDATE_GUILDRANK, listObjects, strCmd, player, null);
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Exception, ex.ToString());
            }
        }
        #endregion

        #region Uy danh và vinh dự
        /// <summary>
        /// Gửi gói tin thông báo uy danh và vinh dự của bản thân thay đổi
        /// </summary>
        /// <param name="player"></param>
        public static void NotifyMyselfPrestigeAndWorldHonorChanged(KPlayer player)
        {
            try
            {
                string strCmd = string.Format("{0}:{1}:{2}:{3}", player.Prestige, player.WorldHonor, player.FactionHonor, player.WorldMartial);
                player.SendPacket((int)TCPGameServerCmds.CMD_KT_G2C_UPDATE_PRESTIGE_AND_HONOR, strCmd);
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Exception, ex.ToString());
            }
        }
        #endregion

        #region Kinh nghiệm và cấp độ
        /// <summary>
        /// Thông báo kinh nghiệm hoặc cấp độ thay đổi về Client
        /// </summary>
        /// <param name="client"></param>
        public static void NotifySelfExperience(KPlayer client)
        {
            string strcmd = string.Format("{0}:{1}:{2}:{3}", client.RoleID, client.m_Level, client.m_Experience, KPlayerSetting.GetNeedExpUpExp(client.m_Level));
            client.SendPacket((int) TCPGameServerCmds.CMD_SPR_EXPCHANGE, strcmd);

         
        }

        /// <summary>
        /// Thông báo cho những thằng xung quanh cấp độ bản thân thay đổi
        /// </summary>
        /// <param name="client"></param>
        public static void NotifySelfLevelChanged(KPlayer client)
        {
            /// Tìm người chơi xung quanhg
            List<KPlayer> objsList = KTRadarMapManager.GetPlayersAround(client);
            /// Toác
            if (null == objsList)
            {
                /// Bỏ qua
                return;
            }

            string strcmd = string.Format("{0}:{1}", client.RoleID, client.m_Level);
            KTPlayerManager.SendPacketToPlayers((int) TCPGameServerCmds.CMD_SPR_LEVEL_CHANGED, objsList, strcmd, client, client);
        }
        #endregion
    }
}
