using GameServer.Entities.Skill.Other;
using GameServer.KiemThe.Entities;
using GameServer.KiemThe.GameDbController;
using GameServer.KiemThe.Utilities;
using GameServer.Logic;
using GameServer.Server;
using GameServer.VLTK.Core.StallManager;
using Server.Data;
using Server.Protocol;
using Server.TCP;
using Server.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static GameServer.Logic.KTMapManager;

namespace GameServer.KiemThe.Logic
{
    /// <summary>
    /// Quản lý kỹ năng
    /// </summary>
    public static partial class KT_TCPHandler
    {
        #region Kỹ năng

        /// <summary>
        /// Gửi tín hiệu về Client làm mới danh sách kỹ năng
        /// </summary>
        /// <param name="client"></param>
        public static void SendRenewSkillList(KPlayer client)
        {
            try
            {
                List<SkillData> skills = new List<SkillData>();
                foreach (SkillLevelRef skill in client.Skills.ListSkills)
                {
                    skills.Add(new SkillData()
                    {
                        SkillID = skill.SkillID,
                        SkillLevel = skill.AddedLevel,
                        BonusLevel = client.Skills.AllSkillBonusLevel + client.Skills.GetAdditionSkillLevel(skill.SkillID),
                        CanStudy = skill.CanStudy,
                        LastUsedTick = client.Skills.GetSkillLastUsedTick(skill.SkillID),
                        CooldownTick = client.Skills.GetSkillCooldown(skill.SkillID),
                    });
                }

                client.SendPacket<KeyValuePair<int, List<SkillData>>>((int)TCPGameServerCmds.CMD_KT_G2C_RENEW_SKILLLIST, new KeyValuePair<int, List<SkillData>>(client.GetCurrentSkillPoints(), skills));
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Exception, ex.ToString());
            }
        }

        /// <summary>
        /// Thêm kỹ năng vào DB
        /// </summary>
        /// <param name="player"></param>
        /// <param name="skillID"></param>
        public static void AddSkillToDB(KPlayer player, int skillID)
        {
            try
            {
                string strcmd = string.Format("{0}:{1}", player.RoleID, skillID);

                Global.ExecuteDBCmd((int)TCPGameServerCmds.CMD_DB_ADDSKILL, strcmd, player.ServerId);
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Exception, ex.ToString());
            }
        }

        /// <summary>
        /// Xóa kỹ năng khỏi DB
        /// </summary>
        /// <param name="player"></param>
        /// <param name="skillID"></param>
        public static void DeleteSkillFromDB(KPlayer player, int skillID)
        {
            try
            {
                string strcmd = string.Format("{0}:{1}", player.RoleID, skillID);
             

                Global.ExecuteDBCmd((int)TCPGameServerCmds.CMD_DB_DEL_SKILL, strcmd, player.ServerId);
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Exception, ex.ToString());
            }
        }

        /// <summary>
        /// Cập nhật thông tin kỹ năng vào DB
        /// </summary>
        /// <param name="player"></param>
        /// <param name="skillID"></param>
        /// <param name="skillLevel"></param>
        /// <param name="lastUsedTick"></param>
        /// <param name="cooldownTick"></param>
        /// <param name="exp"></param>
        public static void UpdateSkillInfoFromDB(KPlayer player, int skillID, int skillLevel, long lastUsedTick, int cooldownTick, int exp)
        {
            //Console.WriteLine("Update skill info\n" + new System.Diagnostics.StackTrace());

            try
            {
                string strcmd = string.Format("{0}:{1}:{2}:{3}:{4}:{5}", player.RoleID, skillID, skillLevel, lastUsedTick, cooldownTick, exp);
          
                Global.ExecuteDBCmd((int)TCPGameServerCmds.CMD_DB_UPSKILLINFO, strcmd, player.ServerId);

                //Console.WriteLine("Update skill To DB => " + skillID + " - " + skillLevel);
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Exception, ex.ToString());
            }
        }

        /// <summary>
        /// Phản hồi yêu cầu cộng điểm kỹ năng của Client
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
        public static TCPProcessCmdResults ResponseDistributeSkillPoints(TCPManager tcpMgr, TMSKSocket socket, TCPClientPool tcpClientPool, TCPRandKey tcpRandKey, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;
            C2G_DistributeSkillPoint distributeSkillPoint = null;

            try
            {
                distributeSkillPoint = DataHelper.BytesToObject<C2G_DistributeSkillPoint>(data, 0, data.Length);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Error while getting DATA, CMD={0}, Client={1}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket)));
                return TCPProcessCmdResults.RESULT_FAILED;
            }

            try
            {
                KPlayer client = KTPlayerManager.Find(socket);
                if (null == client)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Can not find player, CMD={0}, Client={1}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket)));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// Nếu không giải mã được gói tin
                if (distributeSkillPoint == null)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Error while getting DATA, CMD={0}, Client={1}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket)));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// Nếu môn phái không tồn tại thì đào đâu ra mà cộng điểm
                KFaction.KFactionAttirbute factionInfo = KFaction.GetFactionInfo(client.m_cPlayerFaction.GetFactionId());
                if (factionInfo == null)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Player's faction is not exist, CMD={0}, Client={1}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket)));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// Kiểm tra toàn bộ kỹ năng, nếu không nằm trong danh sách hiện có của nhân vật, hoặc có nhưng không thể học được thì toang
                foreach (KeyValuePair<int, int> pair in distributeSkillPoint.ListDistributedSkills)
                {
                    SkillLevelRef skillRef = client.Skills.GetSkillLevelRef(pair.Key);
                    if (skillRef == null || (skillRef != null && !skillRef.CanAddPoint))
                    {
                        LogManager.WriteLog(LogTypes.Error, string.Format("Skill not found in player's list or can not add point, CMD={0}, Client={1}, SkillID={2}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), pair.Key));
                        return TCPProcessCmdResults.RESULT_FAILED;
                    }
                }

                ///// Nhánh hiện tại của người chơi
                //int routeID = client.m_cPlayerFaction.GetRouteId();

                ///// Nếu chưa có nhánh, thì lấy nhánh gửi về từ Client
                //if (routeID == 0)
                //{
                //    /// Nếu Client truyền về ID nhánh là 0 là toang
                //    if (distributeSkillPoint.SelectedRouteID == 0 || factionInfo.arRoute.Where(x => x.nID == distributeSkillPoint.SelectedRouteID).FirstOrDefault() == null)
                //    {
                //        LogManager.WriteLog(LogTypes.Error, string.Format("RouteID sent from Client ERROR, CMD={0}, Client={1}, RouteID={2}", (TCPGameServerCmds) nID, Global.GetSocketRemoteEndPoint(socket), distributeSkillPoint.SelectedRouteID));
                //        return TCPProcessCmdResults.RESULT_FAILED;
                //    }

                //    routeID = distributeSkillPoint.SelectedRouteID;
                //}

                ///// Kiểm tra toàn bộ danh sách xem có kỹ năng nào không nằm trong nhánh không, nếu có là toang
                //List<int> routeSkills = KFaction.GetRouteSkills(client.m_cPlayerFaction.GetFactionId(), routeID);
                //foreach (KeyValuePair<int, int> pair in distributeSkillPoint.ListDistributedSkills)
                //{
                //    /// Nếu kỹ năng không thuộc nhánh này thì toang
                //    if (!routeSkills.Contains(pair.Key))
                //    {
                //        LogManager.WriteLog(LogTypes.Error, string.Format("Skill data not found in corresponding route, can not find corresponding route, CMD={0}, Client={1}, SkillID={2}, FactionID={3}, RouteID={4}", (TCPGameServerCmds) nID, Global.GetSocketRemoteEndPoint(socket), pair.Key, client.m_cPlayerFaction.GetFactionId(), routeID));
                //        return TCPProcessCmdResults.RESULT_FAILED;
                //    }
                //}

                /// Kiểm tra các kỹ năng có thỏa mãn yêu cầu cấp độ học không
                foreach (KeyValuePair<int, int> pair in distributeSkillPoint.ListDistributedSkills)
                {
                    SkillLevelRef skillRef = client.Skills.GetSkillLevelRef(pair.Key);

                    /// Cấp độ tối thiểu cần để học toàn bộ kỹ năng này
                    int skillMaxRequireLevel = skillRef.Data.RequireLevel + skillRef.Data.MaxSkillLevel - 1;
                    /// Căn cứ theo đẳng cấp hiện tại, có thể học đến tối đa kỹ năng này cấp độ
                    int maxSkillLevelCanStudy;
                    if (skillMaxRequireLevel <= client.m_Level)
                    {
                        maxSkillLevelCanStudy = skillRef.Data.MaxSkillLevel;
                    }
                    else
                    {
                        maxSkillLevelCanStudy = client.m_Level - skillRef.Data.RequireLevel + 1;
                    }

                    if (skillRef.AddedLevel + pair.Value > maxSkillLevelCanStudy)
                    {
                        LogManager.WriteLog(LogTypes.Error, string.Format("Not enough role level to add for skill, CMD={0}, Client={1}, RoleLevel={2}, SkillID={3}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), client.m_Level, pair.Key));
                        return TCPProcessCmdResults.RESULT_FAILED;
                    }
                }

                ///// Nếu nhánh hiện tại = 0, và đã học kỹ năng của một nhánh trong phái, thì tiến hành thiết lập nhánh mới
                //if (client.m_cPlayerFaction.GetRouteId() == 0)
                //{
                //    PlayerManager.ChangeRoute(client, routeID);
                //}

                /// Tiến hành cập nhật giá trị cấp độ kỹ năng
                client.Skills.AddSkillsLevel(distributeSkillPoint.ListDistributedSkills.ToList());

                /// Làm rỗng danh sách hỗ trợ sát thương kỹ năng khác
                client.Skills.ClearPassiveSkillAppendDamages();

                /// Làm rỗng danh sách kỹ năng hỗ trợ kỹ năng khác
                client.Skills.ClearEnchantSkills();

                /// ID vòng sáng hiện tại
                SkillLevelRef currentAuraSkill = client.Buffs.CurrentArua == null ? null : client.Buffs.CurrentArua.Skill;

                /// Xóa vòng sáng
                client.Buffs.RemoveAllAruas();

                /// Thực thi kỹ năng bị động
                client.Skills.ProcessPassiveSkills();

                /// Thực hiện hỗ trợ sát thương kỹ năng khác
                client.Skills.ProcessPassiveSkillsAppendDamages();

                /// Thực hiện kỹ năng hỗ trợ kỹ năng khác
                client.Skills.ProcessEnchantSkills();

                /// Nếu có kỹ năng vòng sáng hiện tại
                if (currentAuraSkill != null)
                {
                    /// Thực hiện kỹ năng vòng sáng
                    KTSkillManager.UseSkill(client, client, null, currentAuraSkill, true);
                }

                /// Làm mới danh sách kỹ năng
                KT_TCPHandler.SendRenewSkillList(client);

                return TCPProcessCmdResults.RESULT_OK;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, Global.GetDebugHelperInfo(socket), false);
            }

            return TCPProcessCmdResults.RESULT_FAILED;
        }

        /// <summary>
        /// Phản hồi yêu cầu thiết lập kỹ năng vào khung kỹ năng
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
        public static TCPProcessCmdResults ResponseSetQuickKey(TCPManager tcpMgr, TMSKSocket socket, TCPClientPool tcpClientPool, TCPRandKey tcpRandKey, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;

            string cmdData = "";
            try
            {
                cmdData = new UTF8Encoding().GetString(data);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Error while getting DATA, CMD={0}, Client={1}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket)));
                return TCPProcessCmdResults.RESULT_FAILED;
            }

            try
            {
                string[] fields = cmdData.Split('|');
                if (fields.Length != 20)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Error Socket params count not fit CMD={0}, Client={1}, Recv={2}",
                        (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), fields.Length));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                List<int> skills = new List<int>();
                try
                {
                    foreach (string p in fields)
                    {
                        skills.Add(int.Parse(p));
                    }
                }
                catch (Exception)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Error Socket params error CMD={0}, Client={1}, Data={1}",
                        (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), cmdData));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                KPlayer client = KTPlayerManager.Find(socket);
                if (null == client)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Can not find player, CMD={0}, Client={1}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket)));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// Kiểm tra toàn bộ kỹ năng, nếu không nằm trong danh sách hiện có của nhân vật hoặc có nhưng cấp 0 thì xóa khỏi danh sách thiết lập
                for (int i = 0; i < skills.Count; i++)
                {
                    int skillID = skills[i];
                    if (skillID == -1)
                    {
                        continue;
                    }
                    SkillLevelRef skillRef = client.Skills.GetSkillLevelRef(skillID);
                    if (skillRef == null || (skillRef != null && skillRef.Level <= 0))
                    {
                        //LogManager.WriteLog(LogTypes.Error, string.Format("Skill not found in player's list or not yet studied, CMD={0}, Client={1}, SkillID={2}", (TCPGameServerCmds) nID, Global.GetSocketRemoteEndPoint(socket), skillID));
                        //return TCPProcessCmdResults.RESULT_FAILED;
                        skills[i] = -1;
                    }
                }

                /// Dãy lưu trữ tương ứng
                string quickKey = string.Join("|", skills);

                /// Thiết lập dữ liệu vào RoleData
                client.MainQuickBarKeys = quickKey;

                ///// Lưu vào DB
                //KT_TCPHandler.SendSaveQuickKeyToDB(client);

                return TCPProcessCmdResults.RESULT_OK;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, Global.GetDebugHelperInfo(socket), false);
            }

            return TCPProcessCmdResults.RESULT_FAILED;
        }

        /// <summary>
        /// Phản hồi yêu cầu thiết lập và kích hoạt kỹ năng vòng sáng
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
        public static TCPProcessCmdResults ResponseSetAndActivateAura(TCPManager tcpMgr, TMSKSocket socket, TCPClientPool tcpClientPool, TCPRandKey tcpRandKey, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;

            string cmdData = "";
            try
            {
                cmdData = new UTF8Encoding().GetString(data);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Error while getting DATA, CMD={0}, Client={1}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket)));
                return TCPProcessCmdResults.RESULT_FAILED;
            }

            try
            {
                string[] fields = cmdData.Split('_');
                if (fields.Length != 2)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Error Socket params count not fit CMD={0}, Client={1}, Recv={2}",
                        (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), fields.Length));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                KPlayer client = KTPlayerManager.Find(socket);
                if (null == client)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Can not find player, CMD={0}, Client={1}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket)));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                int skillID = -1;
                int status = 0;
                try
                {
                    skillID = int.Parse(fields[0]);
                    status = int.Parse(fields[1]);
                }
                catch (Exception ex)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Error Socket params error CMD={0}, Client={1}, Recv={2}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), fields.Length));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// Dữ liệu kỹ năng tương ứng
                SkillLevelRef skill = client.Skills.GetSkillLevelRef(skillID);
                /// Nếu kỹ năng không tồn tại hoặc không phải vòng sáng
                if (skill == null || skill.Level <= 0 || skill.Data.Form == 3 || !skill.Data.IsArua)
                {
                    skillID = -1;
                    status = 0;
                }

                /// Dãy lưu thông tin vòng sáng
                string aruaKey = string.Format("{0}_{1}", skillID, status);

                /// Thiết lập dữ liệu vào RoleData
                client.OtherQuickBarKeys = aruaKey;

                ///// Lưu vào DB
                //KT_TCPHandler.SendSaveAruaKeyToDB(client);

                /// Kích hoạt hoặc hủy kỹ năng bị động nếu được thiết lập
                if (skillID != -1)
                {
                    /// Nếu trạng thái là hủy kích hoạt
                    if (status == 0)
                    {
                        /// Xóa toàn bộ vòng sáng
                        client.Buffs.RemoveAllAruas();
                    }
                    /// Nếu trạng thái là kích hoạt
                    else if (status == 1)
                    {
                        /// Sử dụng kỹ năng vòng sáng
                        KTSkillManager.UseSkill(client, null, null, skill);
                    }
                }
                else
                {
                    /// Xóa toàn bộ vòng sáng
                    client.Buffs.RemoveAllAruas();
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
        /// Lưu dữ liệu kỹ năng thiết lập vào ô kỹ năng nhanh vào DB
        /// </summary>
        public static void SendSaveQuickKeyToDB(KPlayer player)
        {
            string cmdData = player.MainQuickBarKeys;
          
            Global.ExecuteDBCmd((int)TCPGameServerCmds.CMD_DB_UPDATEKEYS, player.RoleID + ":" + 0 + ":" + cmdData, player.ServerId);
        }

        /// <summary>
        /// Lưu dữ liệu kích hoạt kỹ năng vòng sáng vào DB
        /// </summary>
        /// <param name="player"></param>
        public static void SendSaveAruaKeyToDB(KPlayer player)
        {
            string cmdData = player.OtherQuickBarKeys;
          
            Global.ExecuteDBCmd((int)TCPGameServerCmds.CMD_DB_UPDATEKEYS, player.RoleID + ":" + 1 + ":" + cmdData, player.ServerId);

        }

        /// <summary>
        /// Phản hồi yêu cầu sử dụng kỹ năng
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
        public static TCPProcessCmdResults ResponseUseSkill(TCPManager tcpMgr, TMSKSocket socket, TCPClientPool tcpClientPool, TCPRandKey tcpRandKey, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;

            C2G_UseSkill useSkill = null;
            try
            {
                useSkill = DataHelper.BytesToObject<C2G_UseSkill>(data, 0, data.Length);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Error while getting DATA, CMD={0}, Client={1}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket)));
                return TCPProcessCmdResults.RESULT_FAILED;
            }

            try
            {
                int posX = useSkill.PosX;
                int posY = useSkill.PosY;

                KPlayer client = KTPlayerManager.Find(socket);
                if (null == client)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Can not find player, CMD={0}, Client={1}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket)));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// Kỹ năng tương ứng
                SkillDataEx skillData = KSkill.GetSkillData(useSkill.SkillID);
                if (skillData == null)
                {
                    KTSkillManager.UseSkillResult _result = KTSkillManager.UseSkillResult.No_Corresponding_Skill_Found;
                    string _resultData = string.Format("{0}", (int)_result);
                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, _resultData, nID);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                /// Bản đồ hiện tại
                GameMap gameMap = KTMapManager.Find(client.CurrentMapCode);
                /// Nếu bản đồ không cho phép dùng kỹ năng
                if (!gameMap.AllowUseSkill)
                {
                    KTPlayerManager.ShowNotification(client, "Bản đồ hiện tại không cho phép sử dụng kỹ năng!");
                    KTSkillManager.UseSkillResult _result = KTSkillManager.UseSkillResult.None;
                    string _resultData = string.Format("{0}", (int)_result);
                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, _resultData, nID);
                    return TCPProcessCmdResults.RESULT_DATA;
                }
                /// Nếu là kỹ năng đánh nhưng bản đồ hiện tại không cho phép dùng
                else if (!gameMap.AllowUseOffensiveSkill && (skillData.Type != 2 || (skillData.TargetType != "self" && skillData.TargetType != "team")))
                {
                    KTPlayerManager.ShowNotification(client, "Bản đồ hiện tại không cho phép sử dụng kỹ năng này!");
                    KTSkillManager.UseSkillResult _result = KTSkillManager.UseSkillResult.None;
                    string _resultData = string.Format("{0}", (int)_result);
                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, _resultData, nID);
                    return TCPProcessCmdResults.RESULT_DATA;
                }
                /// Nếu kỹ năng bị cấm
                else if (gameMap.BanSkills.Contains(useSkill.SkillID))
                {
                    KTPlayerManager.ShowNotification(client, "Bản đồ hiện tại không cho phép sử dụng kỹ năng này!");
                    KTSkillManager.UseSkillResult _result = KTSkillManager.UseSkillResult.None;
                    string _resultData = string.Format("{0}", (int)_result);
                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, _resultData, nID);
                    return TCPProcessCmdResults.RESULT_DATA;
                }
                /// Nếu đang bày bán thì không cho dùng kỹ năng
                else if (StallManager.IsDirectStall(client))
                {
                    KTPlayerManager.ShowNotification(client, "Đang trong trạng thái bán hàng, không thể sử dụng kỹ năng!");
                    KTSkillManager.UseSkillResult _result = KTSkillManager.UseSkillResult.None;
                    string _resultData = string.Format("{0}", (int) _result);
                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, _resultData, nID);
                    return TCPProcessCmdResults.RESULT_DATA;
                }
                /// Nếu có trạng thái cấm dùng kỹ năng
                else if (client.ForbidUsingSkill)
                {
                    KTPlayerManager.ShowNotification(client, "Bạn hiện không thể sử dụng kỹ năng!");
                    KTSkillManager.UseSkillResult _result = KTSkillManager.UseSkillResult.None;
                    string _resultData = string.Format("{0}", (int)_result);
                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, _resultData, nID);
                    return TCPProcessCmdResults.RESULT_DATA;
                }
                /// Nếu đang ở trong khu an toàn
                else if (client.IsInsideSafeZone && (skillData.Type != 2 || (skillData.TargetType != "self" && skillData.TargetType != "team")))
                {
                    KTPlayerManager.ShowNotification(client, "Trong khu an toàn không thể sử dụng kỹ năng tấn công mục tiêu!");
                    KTSkillManager.UseSkillResult _result = KTSkillManager.UseSkillResult.None;
                    string _resultData = string.Format("{0}", (int)_result);
                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, _resultData, nID);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                /// Nếu là kỹ năng khinh công
                if (useSkill.SkillID == 10)
                {
                    /// Nếu đang khinh công thì thôi
                    if (client.IsBlinking())
                    {
                        KTSkillManager.UseSkillResult _result = KTSkillManager.UseSkillResult.None;
                        string _resultData = string.Format("{0}", (int)_result);
                        tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, _resultData, nID);
                        return TCPProcessCmdResults.RESULT_DATA;
                    }

                    /// Nếu chưa đến thời gian dùng
                    if (KTGlobal.GetCurrentTimeMilis() - client.LastUseFlyingTicks < 1000)
                    {
                        KTSkillManager.UseSkillResult _result = KTSkillManager.UseSkillResult.None;
                        string _resultData = string.Format("{0}", (int)_result);
                        tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, _resultData, nID);
                        return TCPProcessCmdResults.RESULT_DATA;
                    }

                    /// Nếu đang bị bất động thì thôi
                    if (!client.IsCanMove())
                    {
                        KTPlayerManager.ShowNotification(client, "Đang trong trạng thái bị không thể sử dụng kỹ năng!");
                        KTSkillManager.UseSkillResult _result = KTSkillManager.UseSkillResult.None;
                        string _resultData = string.Format("{0}", (int)_result);
                        tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, _resultData, nID);
                        return TCPProcessCmdResults.RESULT_DATA;
                    }

                    /// Đánh dấu thời gian lần cuối dùng khinh công
                    client.LastUseFlyingTicks = KTGlobal.GetCurrentTimeMilis();
                }

                /// Hủy Buff bảo vệ
                client.RemoveChangeMapProtectionBuff();

                /// Ngừng di chuyển
                KTPlayerStoryBoardEx.Instance.Remove(client, false);

                /// Kiểm tra kỹ năng, nếu không nằm trong danh sách hiện có của nhân vật hoặc có nhưng cấp 0 thì toang
                SkillLevelRef skillRef = client.Skills.GetSkillLevelRef(useSkill.SkillID);
                if (skillRef == null || (skillRef != null && skillRef.Level <= 0))
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Skill not found in player's list or not yet studied, CMD={0}, Client={1}, SkillID={2}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), useSkill.SkillID));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                GameObject target = null;
                Interface.IObject obj = KTSkillManager.FindSpriteByID(useSkill.TargetID);
                /// Nếu không phải GameObject thì không đánh được
                if (obj != null && (obj is GameObject))
                {
                    target = (GameObject)obj;
                }

                /// Thiết lập hướng quay truyền từ Client lên
                try
                {
                    client.CurrentDir = (Entities.Direction)useSkill.Direction;
                }
                catch (Exception) { }

                /// Kiểm tra vị trí hiện tại của người chơi và vị trí truyền về từ Client xem có hợp lệ không
                if (!client.SpeedCheatDetector.Validate(posX, posY))
                {
                    posX = client.PosX;
                    posY = client.PosY;
                }

                UnityEngine.Vector2? targetPos = null;
                /// Nếu có vị trí đích
                if (useSkill.TargetPosX != -1 && useSkill.TargetPosY != -1)
                {
                    targetPos = new UnityEngine.Vector2(useSkill.TargetPosX, useSkill.TargetPosY);
                }

                /// Đánh dấu mục tiêu hiện tại
                client.CurrentTarget = target;

                /// Dùng kỹ năng
                KTSkillManager.UseSkillResult result = KTSkillManager.UseSkill(client, target, targetPos, skillRef);
                //if (result == KTSkillManager.UseSkillResult.Target_Not_In_Range)
                //{
                //    Console.WriteLine("FUCK: " + target.CurrentPos.ToString());
                //}

                string resultData = string.Format("{0}", (int)result);
                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, resultData, nID);

                return TCPProcessCmdResults.RESULT_DATA;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, Global.GetDebugHelperInfo(socket), false);
            }

            return TCPProcessCmdResults.RESULT_FAILED;
        }

        /// <summary>
        /// Thông báo kỹ năng trong trạng thái phục hồi
        /// </summary>
        /// <param name="player"></param>
        /// <param name="skill"></param>
        public static void NotifySkillCooldown(KPlayer player, SkillCooldown skill)
        {
            try
            {
                G2C_SkillCooldown skillCooldown = new G2C_SkillCooldown()
                {
                    SkillID = skill.SkillID,
                    StartTick = skill.StartTick,
                    CooldownTick = skill.CooldownTick,
                };
                byte[] cmdData = DataHelper.ObjectToBytes<G2C_SkillCooldown>(skillCooldown);

                player.SendPacket((int)TCPGameServerCmds.CMD_KT_G2C_NOTIFYSKILLCOOLDOWN, cmdData);
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Exception, ex.ToString());
            }
        }

        /// <summary>
        /// Thông báo kỹ năng trong trạng thái phục hồi
        /// </summary>
        /// <param name="player"></param>
        /// <param name="excludedSkills"></param>
        public static void ResetSkillCooldown(KPlayer player, List<int> excludedSkills = null)
        {
            try
            {
                byte[] cmdData = new ASCIIEncoding().GetBytes(excludedSkills == null ? "" : string.Join(":", excludedSkills));

                player.SendPacket((int)TCPGameServerCmds.CMD_KT_G2C_RESETSKILLCOOLDOWN, cmdData);
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Exception, ex.ToString());
            }
        }

        /// <summary>
        /// Thông báo tới tất cả người chơi xung quanh đối tượng đang dùng kỹ năng
        /// </summary>
        /// <param name="go">Đối tượng</param>
        /// <param name="skill">Dữ liệu kỹ năng</param>
        /// <param name="isSpecialAttack">Có phải tấn công với động tác đặc biệt không</param>
        public static void SendObjectUseSkill(GameObject go, SkillDataEx skill, bool isSpecialAttack)
        {
            try
            {
                G2C_UseSkill useSkill = new G2C_UseSkill()
                {
                    RoleID = go.RoleID,
                    Direction = (int)go.CurrentDir,
                    SkillID = skill.ID,
                    PosX = (int)go.CurrentPos.X,
                    PosY = (int)go.CurrentPos.Y,
                    IsSpecialAttack = isSpecialAttack,
                };
                byte[] cmdData = DataHelper.ObjectToBytes<G2C_UseSkill>(useSkill);

                /// Tìm tất cả người chơi xung quanh để gửi gói tin
                List<KPlayer> listObjects = KTRadarMapManager.GetPlayersAround(go);
                if (listObjects == null)
                {
                    return;
                }
                /// Gửi gói tin đến tất cả người chơi xung quanh
                KTPlayerManager.SendPacketToPlayers((int)TCPGameServerCmds.CMD_KT_G2C_USESKILL, listObjects, cmdData, go, null);
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Exception, ex.ToString());
            }
        }

        /// <summary>
        /// Thông báo tới tất cả người chơi xung quanh đối tượng có viên đạn được tạo ra
        /// </summary>
        /// <param name="caster">Đối tượng ra chiêu</param>
        /// <param name="bulletID">ID đạn</param>
        /// <param name="resID">ID Res của đạn</param>
        /// <param name="fromPos">Vị trí xuất phát</param>
        /// <param name="toPos">Vị trí đích</param>
        /// <param name="chaseTarget">Mục tiêu đuổi theo</param>
        /// <param name="isTrap">Có phải bẫy không</param>
        /// <param name="velocity">Vận tốc đạn bay</param>
        /// <param name="lifeTime">Thời gian tồn tại</param>
        /// <param name="loopAnimation">Lặp lại hiệu ứng liên tục đến hết thời gian tồn tại</param>
        /// <param name="delay">Thời gian delay trước khi ra đạn</param>
        /// <param name="delay">Thời gian delay trước khi ra đạn</param>
        public static void SendCreateBullet(GameObject caster, int bulletID, int resID, UnityEngine.Vector2 fromPos, UnityEngine.Vector2 toPos, GameObject chaseTarget, bool isTrap, int velocity, float lifeTime, bool loopAnimation, float delay)
        {
            try
            {
                G2C_CreateBullet createBullet = new G2C_CreateBullet()
                {
                    BulletID = bulletID,
                    ResID = resID,
                    FromX = (int)fromPos.x,
                    FromY = (int)fromPos.y,
                    ToX = (int)toPos.x,
                    ToY = (int)toPos.y,
                    TargetID = chaseTarget == null ? -1 : chaseTarget.RoleID,
                    Velocity = velocity,
                    LifeTime = lifeTime,
                    LoopAnimation = loopAnimation,
                    Delay = delay,
                    CasterID = caster.RoleID,
                };
                byte[] cmdData = DataHelper.ObjectToBytes<G2C_CreateBullet>(createBullet);

                /// Tìm tất cả người chơi xung quanh để gửi gói tin
                List<KPlayer> listObjects = KTRadarMapManager.GetPlayersAround(caster);
                if (listObjects == null)
                {
                    return;
                }
                /// Gửi gói tin đến tất cả người chơi xung quanh
                KTPlayerManager.SendPacketToPlayers((int)TCPGameServerCmds.CMD_KT_G2C_CREATEBULLET, listObjects, cmdData, null, null);
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Exception, ex.ToString());
            }
        }

        /// <summary>
        /// Thông báo tới tất cả người chơi xung quanh đối tượng có viên đạn được tạo ra
        /// </summary>
        /// <param name="go">Đối tượng</param>
        /// <param name="packets">Danh sách gói tin</param>
        public static void SendCreateBullets(GameObject go, List<G2C_CreateBullet> packets)
        {
            try
            {
                byte[] cmdData = DataHelper.ObjectToBytes<List<G2C_CreateBullet>>(packets);

                /// Tìm tất cả người chơi xung quanh để gửi gói tin
                List<KPlayer> listObjects = KTRadarMapManager.GetPlayersAround(go);
                if (listObjects == null)
                {
                    return;
                }
                /// Gửi gói tin đến tất cả người chơi xung quanh
                KTPlayerManager.SendPacketToPlayers((int)TCPGameServerCmds.CMD_KT_G2C_CREATEBULLETS, listObjects, cmdData, null, null);
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Exception, ex.ToString());
            }
        }

        /// <summary>
        /// Thông báo tới tất cả người chơi xung quanh đối tượng có hiệu ứng đạn nổ
        /// </summary>
        /// <param name="go">Đối tượng</param>
        /// <param name="bulletID">ID đạn</param>
        /// <param name="resID">ID Res của đạn</param>
        /// <param name="pos">Vị trí nổ</param>
        /// <param name="delay">Thời gian Delay trước khi nổ</param>
        /// <param name="target">Mục tiêu</param>
        public static void SendBulletExplode(GameObject go, int bulletID, int resID, UnityEngine.Vector2 pos, float delay, GameObject target)
        {
            try
            {
                G2C_BulletExplode bulletExplode = new G2C_BulletExplode()
                {
                    BulletID = bulletID,
                    ResID = resID,
                    PosX = (int)pos.x,
                    PosY = (int)pos.y,
                    Delay = delay,
                    TargetID = target != null ? target.RoleID : -1,
                };
                byte[] cmdData = DataHelper.ObjectToBytes<G2C_BulletExplode>(bulletExplode);

                /// Tìm tất cả người chơi xung quanh để gửi gói tin
                List<KPlayer> listObjects = KTRadarMapManager.GetPlayersAround(go);
                if (listObjects == null)
                {
                    return;
                }
                /// Gửi gói tin đến tất cả người chơi xung quanh
                KTPlayerManager.SendPacketToPlayers((int)TCPGameServerCmds.CMD_KT_G2C_BULLETEXPLODE, listObjects, cmdData, null, null);
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Exception, ex.ToString());
            }
        }

        /// <summary>
        /// Thông báo tới tất cả người chơi xung quanh đối tượng có hiệu ứng đạn nổ nhiều vị trí
        /// </summary>
        /// <param name="go">Đối tượng</param>
        /// <param name="explodes">Danh sách gói tin</param>
        public static void SendBulletExplodes(GameObject go, List<G2C_BulletExplode> explodes)
        {
            try
            {
                byte[] cmdData = DataHelper.ObjectToBytes<List<G2C_BulletExplode>>(explodes);

                /// Tìm tất cả người chơi xung quanh để gửi gói tin
                List<KPlayer> listObjects = KTRadarMapManager.GetPlayersAround(go);
                if (listObjects == null)
                {
                    return;
                }
                /// Gửi gói tin đến tất cả người chơi xung quanh
                KTPlayerManager.SendPacketToPlayers((int)TCPGameServerCmds.CMD_KT_G2C_BULLETEXPLODES, listObjects, cmdData, null, null);
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Exception, ex.ToString());
            }
        }

        /// <summary>
        /// Trả về kết quả kỹ năng
        /// </summary>
        /// <param name="caster">Đối tượng xuất chiêu</param>
        /// <param name="target">Mục tiêu</param>
        /// <param name="type">Kết quả kỹ năng</param>
        /// <param name="damage">Sát thương</param>
        public static void SendSkillResult(GameObject caster, GameObject target, KTSkillManager.SkillResult type, int damage)
        {
            try
            {
                SkillResult skillResult = new SkillResult()
                {
                    CasterID = caster.RoleID,
                    TargetID = target.RoleID,
                    Type = (int)type,
                    Damage = damage,
                    TargetCurrentHP = target.m_CurrentLife,
                };
                byte[] cmdData = DataHelper.ObjectToBytes<SkillResult>(skillResult);

                /// Tìm tất cả người chơi xung quanh để gửi gói tin
                List<KPlayer> listObjects = KTRadarMapManager.GetPlayersAround(target);
                if (listObjects == null)
                {
                    return;
                }
                /// Gửi gói tin đến tất cả người chơi xung quanh
                KTPlayerManager.SendPacketToPlayers((int)TCPGameServerCmds.CMD_KT_G2C_SKILLRESULT, listObjects, cmdData, null, null);
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Exception, ex.ToString());
            }
        }

        /// <summary>
        /// Trả về danh sách kết quả kỹ năng
        /// </summary>
        /// <param name="target">Mục tiêu</param>
        /// <param name="skillResults">Danh sách gói tin</param>
        public static void SendSkillResults(GameObject target, List<SkillResult> skillResults)
        {
            try
            {
                byte[] cmdData = DataHelper.ObjectToBytes<List<SkillResult>>(skillResults);

                /// Tìm tất cả người chơi xung quanh để gửi gói tin
                List<KPlayer> listObjects = KTRadarMapManager.GetPlayersAround(target);
                if (listObjects == null)
                {
                    return;
                }
                /// Gửi gói tin đến tất cả người chơi xung quanh
                KTPlayerManager.SendPacketToPlayers((int)TCPGameServerCmds.CMD_KT_G2C_SKILLRESULTS, listObjects, cmdData, null, null);
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Exception, ex.ToString());
            }
        }

        /// <summary>
        /// Trả về danh sách kết quả kỹ năng cho bản thân
        /// </summary>
        /// <param name="player"></param>
        /// <param name="skillResults"></param>
        public static void SendSkillResultsToMySelf(KPlayer player, List<SkillResult> skillResults)
        {
            try
            {
                /// Gửi gói tin đến tất cả người chơi xung quanh
                player.SendPacket<List<SkillResult>>((int)TCPGameServerCmds.CMD_KT_G2C_SKILLRESULTS, skillResults);
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Exception, ex.ToString());
            }
        }

        /// <summary>
        /// Trả về danh sách kết quả kỹ năng cho bản thân
        /// </summary>
        /// <param name="player"></param>
        /// <param name="skillResults"></param>
        public static void SendSkillResultToMySelf(KPlayer player, GameObject caster, GameObject target, KTSkillManager.SkillResult type, int damage)
        {
            try
            {
                SkillResult skillResult = new SkillResult()
                {
                    CasterID = caster.RoleID,
                    TargetID = target.RoleID,
                    Type = (int)type,
                    Damage = damage,
                    TargetCurrentHP = target.m_CurrentLife,
                };
                /// Gửi gói tin đến tất cả người chơi xung quanh
                player.SendPacket<SkillResult>((int)TCPGameServerCmds.CMD_KT_G2C_SKILLRESULT, skillResult);
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Exception, ex.ToString());
            }
        }

        /// <summary>
        /// Gửi gói tin đối tượng tốc biến đến vị trí chỉ định
        /// </summary>
        /// <param name="caster">Đối tượng</param>
        /// <param name="posX">Tọa độ X</param>
        /// <param name="posY">Tọa độ Y</param>
        /// <param name="duration">Thời gian hiệu ứng</param>
        public static void SendBlinkToPosition(GameObject caster, int posX, int posY, float duration)
        {
            try
            {
                G2C_BlinkToPosition blink = new G2C_BlinkToPosition()
                {
                    RoleID = caster.RoleID,
                    PosX = posX,
                    PosY = posY,
                    Duration = duration,
                    Direction = (int)caster.CurrentDir,
                };
                byte[] cmdData = DataHelper.ObjectToBytes<G2C_BlinkToPosition>(blink);

                /// Tìm tất cả người chơi xung quanh để gửi gói tin
                List<KPlayer> listObjects = KTRadarMapManager.GetPlayersAround(caster);
                if (listObjects == null)
                {
                    return;
                }
                /// Gửi gói tin đến tất cả người chơi xung quanh
                KTPlayerManager.SendPacketToPlayers((int)TCPGameServerCmds.CMD_KT_G2C_BLINKTOPOSITION, listObjects, cmdData, caster, null);
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Exception, ex.ToString());
            }
        }

        /// <summary>
        /// Gửi gói tin đối tượng khinh công đến vị trí chỉ định
        /// </summary>
        /// <param name="caster">Đối tượng</param>
        /// <param name="posX">Tọa độ X</param>
        /// <param name="posY">Tọa độ Y</param>
        /// <param name="duration">Thời gian hiệu ứng</param>
        public static void SendFlyToPosition(GameObject caster, int posX, int posY, float duration)
        {
            try
            {
                G2C_FlyToPosition fly = new G2C_FlyToPosition()
                {
                    RoleID = caster.RoleID,
                    PosX = posX,
                    PosY = posY,
                    Duration = duration,
                    Direction = (int)caster.CurrentDir,
                };
                byte[] cmdData = DataHelper.ObjectToBytes<G2C_FlyToPosition>(fly);

                /// Tìm tất cả người chơi xung quanh để gửi gói tin
                List<KPlayer> listObjects = KTRadarMapManager.GetPlayersAround(caster);
                if (listObjects == null)
                {
                    return;
                }
                /// Gửi gói tin đến tất cả người chơi xung quanh
                KTPlayerManager.SendPacketToPlayers((int)TCPGameServerCmds.CMD_KT_G2C_FLYTOPOSITION, listObjects, cmdData, caster, null);
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Exception, ex.ToString());
            }
        }

        /// <summary>
        /// Thông báo thêm Buff cho đối tượng
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="buff"></param>
        public static void SendSpriteAddBuff(GameObject target, BuffDataEx buff)
        {
            try
            {
                /// Dữ liệu Buff đầy đủ cả thuộc tính gửi cho bản thân
                G2C_SpriteBuff sprBuffForMeOnly = new G2C_SpriteBuff()
                {
                    RoleID = target.RoleID,
                    SkillID = buff.Skill.SkillID,
                    Duration = buff.Duration,
                    Level = buff.Level,
                    ResID = buff.Skill.Data.StateEffectID,
                    StartTick = buff.StartTick,
                    PacketType = 1,
                };
                PropertyDictionary buffPd = buff.CustomProperties != null ? buff.CustomProperties : buff.Properties;
                if (buffPd != null && buff.StackCount > 0)
                {
                    //Console.WriteLine("Stack = " + buff.StackCount);
                    PropertyDictionary finalPd = KTAttributesModifier.GetStackProperties(buffPd, buff.StackCount);
                    sprBuffForMeOnly.Properties = finalPd.ToPortableDBString();
                }
                byte[] cmdDataMyself = DataHelper.ObjectToBytes<G2C_SpriteBuff>(sprBuffForMeOnly);
                /// Gửi đến bản thân
                if (target is KPlayer)
                {
                    (target as KPlayer).SendPacket((int)TCPGameServerCmds.CMD_KT_G2C_SPRITEBUFF, cmdDataMyself);
                }

                /// Dữ liệu Buff gửi người chơi khác
                G2C_SpriteBuff sprBuff = new G2C_SpriteBuff()
                {
                    RoleID = target.RoleID,
                    SkillID = buff.Skill.SkillID,
                    Duration = buff.Duration,
                    Level = buff.Level,
                    ResID = buff.Skill.Data.StateEffectID,
                    StartTick = buff.StartTick,
                    PacketType = 1,
                };
                byte[] cmdData = DataHelper.ObjectToBytes<G2C_SpriteBuff>(sprBuff);

                /// Tìm tất cả người chơi xung quanh để gửi gói tin
                List<KPlayer> listObjects = KTRadarMapManager.GetPlayersAround(target);
                if (listObjects == null)
                {
                    return;
                }
                /// Gửi gói tin đến tất cả người chơi xung quanh
                KTPlayerManager.SendPacketToPlayers((int)TCPGameServerCmds.CMD_KT_G2C_SPRITEBUFF, listObjects, cmdData, target, target);
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Exception, ex.ToString());
            }
        }

        /// <summary>
        /// Thông báo xóa Buff cho đối tượng
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="buff"></param>
        public static void SendSpriteRemoveBuff(GameObject target, BuffDataEx buff)
        {
            try
            {
                G2C_SpriteBuff sprBuff = new G2C_SpriteBuff()
                {
                    RoleID = target.RoleID,
                    SkillID = buff.Skill.SkillID,
                    Duration = buff.Duration,
                    Level = buff.Level,
                    ResID = buff.Skill.Data.StateEffectID,
                    StartTick = buff.StartTick,
                    PacketType = 0,
                };
                byte[] cmdData = DataHelper.ObjectToBytes<G2C_SpriteBuff>(sprBuff);

                /// Tìm tất cả người chơi xung quanh để gửi gói tin
                List<KPlayer> listObjects = KTRadarMapManager.GetPlayersAround(target);
                if (listObjects == null)
                {
                    return;
                }
                /// Gửi gói tin đến tất cả người chơi xung quanh
                KTPlayerManager.SendPacketToPlayers((int)TCPGameServerCmds.CMD_KT_G2C_SPRITEBUFF, listObjects, cmdData, target, null);
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Exception, ex.ToString());
            }
        }

        /// <summary>
        /// Gói tin gửi về Client thông báo tốc độ di chuyển của đối tượng thay đổi
        /// </summary>
        /// <param name="target"></param>
        public static void NotifyTargetMoveSpeedChanged(GameObject target)
        {
            try
            {
                G2C_MoveSpeedChanged moveSpeedChanged = new G2C_MoveSpeedChanged()
                {
                    RoleID = target.RoleID,
                    MoveSpeed = target.GetCurrentRunSpeed(),
                };
                byte[] cmdData = DataHelper.ObjectToBytes<G2C_MoveSpeedChanged>(moveSpeedChanged);

                /// Tìm tất cả người chơi xung quanh để gửi gói tin
                List<KPlayer> listObjects = KTRadarMapManager.GetPlayersAround(target);
                if (listObjects == null)
                {
                    return;
                }
                /// Gửi gói tin đến tất cả người chơi xung quanh
                KTPlayerManager.SendPacketToPlayers((int)TCPGameServerCmds.CMD_KT_G2C_MOVESPEEDCHANGED, listObjects, cmdData, target, null);
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Exception, ex.ToString());
            }
        }

        /// <summary>
        /// Gói tin gửi về Client thông báo tốc độ xuất chiêu của đối tượng thay đổi
        /// </summary>
        /// <param name="target"></param>
        public static void NotifyTargetAttackSpeedChanged(GameObject target)
        {
            try
            {
                G2C_AttackSpeedChanged attackSpeedChanged = new G2C_AttackSpeedChanged()
                {
                    RoleID = target.RoleID,
                    AttackSpeed = target.GetCurrentAttackSpeed(),
                    CastSpeed = target.GetCurrentCastSpeed(),
                };
                byte[] cmdData = DataHelper.ObjectToBytes<G2C_AttackSpeedChanged>(attackSpeedChanged);

                /// Tìm tất cả người chơi xung quanh để gửi gói tin
                List<KPlayer> listObjects = KTRadarMapManager.GetPlayersAround(target);
                if (listObjects == null)
                {
                    return;
                }
                /// Gửi gói tin đến tất cả người chơi xung quanh
                KTPlayerManager.SendPacketToPlayers((int)TCPGameServerCmds.CMD_KT_G2C_ATTACKSPEEDCHANGED, listObjects, cmdData, target, null);
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Exception, ex.ToString());
            }
        }

        /// <summary>
        /// Thông báo tới tất cả người chơi xung quanh đối tượng thay đổi trạng thái ẩn thân
        /// </summary>
        /// <param name="target">Đối tượng</param>
        /// <param name="type">Loại 0: Mất trạng thái, 1: Vào trạng thái</param>
        public static void SendObjectInvisibleState(GameObject target, int type)
        {
            try
            {
                G2C_SpriteInvisibleStateChanged spriteInvisibleStateChanged = new G2C_SpriteInvisibleStateChanged()
                {
                    RoleID = target.RoleID,
                    Type = type,
                };
                byte[] cmdData = DataHelper.ObjectToBytes<G2C_SpriteInvisibleStateChanged>(spriteInvisibleStateChanged);

                /// Tìm tất cả người chơi xung quanh để gửi gói tin
                List<KPlayer> listObjects = KTRadarMapManager.GetPlayersAround(target);
                if (listObjects == null)
                {
                    return;
                }
                /// Gửi gói tin đến tất cả người chơi xung quanh
                KTPlayerManager.SendPacketToPlayers((int)TCPGameServerCmds.CMD_KT_G2C_OBJECTINVISIBLESTATECHANGED, listObjects, cmdData, null, null);
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Exception, ex.ToString());
            }
        }

        #endregion Kỹ năng

        #region Buff

        /// <summary>
        /// Lưu toàn bộ Buff của nhân vật vào DB
        /// </summary>
        /// <param name="client"></param>
        public static void UpdatePlayerBuffsToDB(KPlayer client)
        {
            /// Toác
            if (client.BufferDataList == null)
            {
                return;
            }

            lock (client.BufferDataList)
            {
                /// Duyệt danh sách Buff
                for (int i = 0; i < client.BufferDataList.Count; i++)
                {
                    /// Lưu vào DB
                    GameDb.UpdateDBBufferData(client, client.BufferDataList[i]);
                }
            }
        }
        #endregion Buff

        #region Thiết lập kỹ năng tu luyện

        /// <summary>
        /// Phản hồi yêu cầu truy vấn danh sách kỹ năng tu luyện
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
        public static TCPProcessCmdResults ResponseGetExpSkillList(TCPManager tcpMgr, TMSKSocket socket, TCPClientPool tcpClientPool, TCPRandKey tcpRandKey, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;
            string cmdData = "";

            try
            {
                cmdData = new UTF8Encoding().GetString(data);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Error while getting DATA, CMD={0}, Client={1}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket)));
                return TCPProcessCmdResults.RESULT_FAILED;
            }

            try
            {
                /// Toác
                if (!string.IsNullOrEmpty(cmdData))
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Error Socket params count not fit CMD={0}, Client={1}, Recv={2}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), cmdData));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// Người chơi tương ứng
                KPlayer client = KTPlayerManager.Find(socket);
                if (null == client)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Can not find player, CMD={0}, Client={1}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket)));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// Kiểm tra spam-click
                if (client.IsSpamClick())
                {
                    KTPlayerManager.ShowNotification(client, "Thao tác quá nhanh, xin hãy đợi giây lát và thử lại!");
                    return TCPProcessCmdResults.RESULT_OK;
                }
                /// Lưu lại thời điểm thao tác
                client.SendClick();

                /// ID kỹ năng đang luyện
                int currentExpSkillID = client.GetValueOfForeverRecore(ForeverRecord.CurrentExpSkill);

                /// Danh sách kỹ năng
                List<ExpSkillData> skills = new List<ExpSkillData>();
                /// Duyệt danh sách kỹ năng
                foreach (SkillLevelRef skill in client.Skills.GetExpSkills())
                {
                    /// Nếu chưa học
                    if (skill.Level <= 0)
                    {
                        /// Bỏ qua
                        continue;
                    }
                    /// Nếu không phải kỹ năng có kinh nghiệm
                    else if (!skill.Data.IsExpSkill)
                    {
                        /// Bỏ qua
                        continue;
                    }

                    /// Tạo mới
                    ExpSkillData expSkill = new ExpSkillData()
                    {
                        SkillID = skill.SkillID,
                        Level = skill.AddedLevel,
                        CurrentExp = skill.Exp,
                        LevelUpExp = KSkillExp.GetSkillLevelUpExp(skill),
                        IsCurrentExpSkill = currentExpSkillID == skill.SkillID,
                    };
                    /// Thêm vào danh sách
                    skills.Add(expSkill);
                }

                /// Dữ liệu
                byte[] byteData = DataHelper.ObjectToBytes<List<ExpSkillData>>(skills);
                /// Gửi gói tin
                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, byteData, nID);
                return TCPProcessCmdResults.RESULT_DATA;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, Global.GetDebugHelperInfo(socket), false);
            }

            return TCPProcessCmdResults.RESULT_FAILED;
        }

        /// <summary>
        /// Phản hồi yêu cầu truy vấn danh sách kỹ năng tu luyện
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
        public static TCPProcessCmdResults ResponseSetExpSkill(TCPManager tcpMgr, TMSKSocket socket, TCPClientPool tcpClientPool, TCPRandKey tcpRandKey, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;
            string cmdData = "";

            try
            {
                cmdData = new UTF8Encoding().GetString(data);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Error while getting DATA, CMD={0}, Client={1}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket)));
                return TCPProcessCmdResults.RESULT_FAILED;
            }

            try
            {
                string[] fields = cmdData.Split(':');
                /// Toác
                if (fields.Length != 1)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Error Socket params count not fit CMD={0}, Client={1}, Recv={2}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), fields.Length));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// Người chơi tương ứng
                KPlayer client = KTPlayerManager.Find(socket);
                if (null == client)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Can not find player, CMD={0}, Client={1}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket)));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// Kiểm tra spam-click
                if (client.IsSpamClick())
                {
                    KTPlayerManager.ShowNotification(client, "Thao tác quá nhanh, xin hãy đợi giây lát và thử lại!");
                    return TCPProcessCmdResults.RESULT_OK;
                }
                /// Lưu lại thời điểm thao tác
                client.SendClick();

                /// ID kỹ năng sẽ thiết lập
                int expSkillID = int.Parse(fields[0]);

                /// ID kỹ năng đang luyện
                int currentExpSkillID = client.GetValueOfForeverRecore(ForeverRecord.CurrentExpSkill);

                /// Nếu trùng thì thôi
                if (currentExpSkillID == expSkillID)
                {
                    KTPlayerManager.ShowNotification(client, "Kỹ năng này đang được tu luyện rồi!");
                    return TCPProcessCmdResults.RESULT_OK;
                }

                /// Hợp lệ không
                bool isValid = false;

                /// Duyệt danh sách kỹ năng
                foreach (SkillLevelRef skill in client.Skills.GetExpSkills())
                {
                    /// Nếu chưa học
                    if (skill.Level <= 0)
                    {
                        /// Bỏ qua
                        continue;
                    }
                    /// Nếu không phải kỹ năng có kinh nghiệm
                    else if (!skill.Data.IsExpSkill)
                    {
                        /// Bỏ qua
                        continue;
                    }

                    /// Nếu là kỹ năng cần thiết lập
                    if (skill.SkillID == expSkillID)
                    {
                        /// Hợp lệ
                        isValid = true;
                        /// Thoát luôn
                        break;
                    }
                }

                /// Toác
                if (!isValid)
                {
                    KTPlayerManager.ShowNotification(client, "Kỹ năng không tồn tại hoặc chưa học được!");
                    return TCPProcessCmdResults.RESULT_OK;
                }

                /// Lưu lại
                client.SetValueOfForeverRecore(ForeverRecord.CurrentExpSkill, expSkillID);
                /// Thông báo
                KTPlayerManager.ShowNotification(client, "Thiết lập kỹ năng tu luyện thành công!");

                /// Gửi gói tin
                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, cmdData, nID);
                return TCPProcessCmdResults.RESULT_DATA;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, Global.GetDebugHelperInfo(socket), false);
            }

            return TCPProcessCmdResults.RESULT_FAILED;
        }

        #endregion Thiết lập kỹ năng tu luyện
    }
}