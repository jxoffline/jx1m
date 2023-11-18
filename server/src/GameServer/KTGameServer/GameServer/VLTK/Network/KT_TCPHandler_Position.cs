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
using System.Windows;
using static GameServer.Logic.KTMapManager;

namespace GameServer.KiemThe.Logic
{
    /// <summary>
    /// Quản lý gói tin
    /// </summary>
    public static partial class KT_TCPHandler
    {
        /// <summary>
        /// Tin tưởng vị trí gửi lên từ Client không
        /// </summary>
        public const bool TrustClientPos = false;

		#region Position
		/// <summary>
		/// Tick kiểm tra vị trí của Client và Server
		/// </summary>
		/// <param name="pool"></param>
		/// <param name="nID"></param>
		/// <param name="data"></param>
		/// <param name="count"></param>
		/// <returns></returns>
		public static TCPProcessCmdResults ProcessSpritePosCmd(TCPManager tcpMgr, TMSKSocket socket, TCPClientPool tcpClientPool, TCPRandKey tcpRandKey, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;
            SpritePositionData cmdData = null;

            try
            {
                cmdData = DataHelper.BytesToObject<SpritePositionData>(data, 0, count);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Decode data faild, CMD={0}, Client={1}", (TCPGameServerCmds) nID, Global.GetSocketRemoteEndPoint(socket)));
                return TCPProcessCmdResults.RESULT_FAILED;
            }

            try
            {
                if (null == cmdData)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Wrong packet params, CMD={0}, Client={1}", (TCPGameServerCmds) nID, Global.GetSocketRemoteEndPoint(socket)));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                int roleID = cmdData.RoleID;
                /// Bản đồ đang đứng
                int mapCode = cmdData.MapCode;
                int posX = cmdData.PosX;
                int posY = cmdData.PosY;

                /// Đối tượng người chơi tương ứng
                KPlayer client = KTPlayerManager.Find(socket);
                if (null == client || client.RoleID != roleID)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Can not find player corresponding ID, CMD={0}, Client={1}", (TCPGameServerCmds) nID, Global.GetSocketRemoteEndPoint(socket)));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// Thông tin bản đồ hiện tại
                GameMap gameMap = KTMapManager.Find(client.CurrentMapCode);
                if (null == gameMap)
                {
                    return TCPProcessCmdResults.RESULT_OK;
                }

                /// Nếu bản đồ ở Client khác bản đồ hiện tại và không phải đang đợi chuyển map
                if (mapCode != client.CurrentMapCode && !client.WaitingForChangeMap)
				{
                    LogManager.WriteLog(LogTypes.RolePosition, string.Format("Bug VKL different map => Server MapID = {0}, Client MapID = {1}", client.CurrentMapCode, mapCode));

                    /// Buộc thực hiện chuyển bản đồ
                    SCMapChange mapChange = new SCMapChange()
                    {
                        MapCode = client.CurrentMapCode,
                        PosX = posX,
                        PosY = posY,
                        RoleID = client.RoleID,
                        Direction = client.RoleDirection,
                        TeleportID = -1,
                        ErrorCode = -1,
                    };
                    client.SendPacket<SCMapChange>((int) TCPGameServerCmds.CMD_SPR_MAPCHANGE, mapChange);
                    return TCPProcessCmdResults.RESULT_OK;
                }

                /// Danh sách các khu Obs động hiện có ở Client
                byte[] dynamicObsLabels = cmdData.DynamicObsLabel;

                int CurentMapID = client.CurrentCopyMapID;

                if(client.CurrentCopyMapID==-1)
                {
                    CurentMapID = client.CurrentMapCode; 
                }    
                /// Danh sách các nhãn khu Obs động đã mở trên GS
                byte[] gsOpenDynamicObsLabels = gameMap.MyNodeGrid.GetOpenDynamicObsLabels(CurentMapID);
                /// Nếu không có gì ở Client gửi lên
                if (dynamicObsLabels == null)
                {
                    /// Nếu có danh sách trên GS
                    if (gsOpenDynamicObsLabels.Length > 0)
                    {
                        /// Gửi lại cho Client update
                        client.SendPacket((int) TCPGameServerCmds.CMD_SPR_UPDATE_DYNAMIC_OBJECT_LABELS, string.Join(":", gsOpenDynamicObsLabels));
                    }
                }
                /// Nếu có dữ liệu ở Client gửi lên
                else
                {
                    /// Nếu độ dài khác với GS
                    if (dynamicObsLabels.Length != gsOpenDynamicObsLabels.Length)
                    {
                        /// Gửi lại cho Client update
                        client.SendPacket((int) TCPGameServerCmds.CMD_SPR_UPDATE_DYNAMIC_OBJECT_LABELS, string.Join(":", gsOpenDynamicObsLabels));
                    }
                    /// Nếu độ dài khớp với GS
                    else
                    {
                        /// Duyệt danh sách từ Client gửi lên
                        foreach (byte clientLabel in dynamicObsLabels)
                        {
                            /// Nếu không tồn tại trong danh sách hiện có ở GS
                            if (!gsOpenDynamicObsLabels.Contains(clientLabel))
                            {
                                /// Gửi lại cho Client update
                                client.SendPacket((int) TCPGameServerCmds.CMD_SPR_UPDATE_DYNAMIC_OBJECT_LABELS, string.Join(":", gsOpenDynamicObsLabels));
                                /// Không cần lặp nữa
                                break;
                            }
                        }
                    }
                }

                /// Nếu đang đứng yên
                if (!KTPlayerStoryBoardEx.Instance.HasStoryBoard(client))
                {
                    /// Nếu có lỗi
                    if (KTGlobal.InObs(mapCode, posX, posY, client.CurrentCopyMapID))
                    {
                        /// Thực hiện Rollback lại vị trí khi có lỗi
                        client.RollbackPosition(true);
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
        /// Thông báo thay đổi vị trí của người chơi tới tất cả người chơi khác
        /// </summary>
        /// <param name="client"></param>
        /// <param name="posX"></param>
        /// <param name="posY"></param>
        /// <param name="notifyOthers"></param>
        /// <returns></returns>
        public static void NotifyMyselfChangePosition(KPlayer client, int posX, int posY, bool notifyOthers = true)
        {
            /// Dữ liệu
            string strcmd = string.Format("{0}:{1}:{2}:{3}", client.RoleID, posX, posY, (int) client.CurrentDir);

            /// Thông báo đến chính mình
            client.SendPacket((int) TCPGameServerCmds.CMD_SPR_CHANGEPOS, strcmd);

            /// Nếu thông báo đến bọn xung quanh
            if (notifyOthers)
            {
                /// Thông báo thằng này thay đổi tọa độ đến bọn xung quanh
                List<KPlayer> objsList = KTRadarMapManager.GetPlayersAround(client);
                if (null == objsList)
                {
                    return;
                }

                /// Gửi gói tin
                KTPlayerManager.SendPacketToPlayers((int) TCPGameServerCmds.CMD_SPR_CHANGEPOS, objsList, strcmd, client, client);
            }
        }
        #endregion

        #region New objects
        /// <summary>
        /// Có đối tượng mới xung quanh
        /// </summary>
        /// <param name="pool"></param>
        /// <param name="nID"></param>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static TCPProcessCmdResults ProcessSpriteLoadAlreadyCmd(TCPManager tcpMgr, TMSKSocket socket, TCPClientPool tcpClientPool, TCPRandKey tcpRandKey, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
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
                int otherRoleID = Convert.ToInt32(fields[1]);

                KPlayer client = KTPlayerManager.Find(socket);
                if (null == client || client.RoleID != roleID)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Can not find player corresponding ID, CMD={0}, Client={1}", (TCPGameServerCmds) nID, Global.GetSocketRemoteEndPoint(socket)));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// Người chơi khác
                KPlayer otherClient = KTPlayerManager.Find(otherRoleID);
                /// Nếu là người chơi
                if (otherClient != null)
                {
                    KTPlayerManager.HandlePlayerLoaded(client, otherClient);
                }
                else
                {
                    /// Quái
                    Monster monster = KTMonsterManager.Find(otherRoleID);
                    /// Nếu là quái
                    if (monster != null)
                    {
                        KTMonsterManager.HandleMonsterLoaded(client, monster);
                        return TCPProcessCmdResults.RESULT_OK;
                    }

                    /// Pet
                    Pet pet = KTPetManager.FindPet(otherRoleID);
                    /// Nếu là pet
                    if (pet != null)
                    {
                        KTPetManager.HandlePetLoaded(client, pet);
                        return TCPProcessCmdResults.RESULT_OK;
                    }

                    /// Xe tiêu
                    TraderCarriage carriage = KTTraderCarriageManager.FindTraderCarriage(otherRoleID);
                    /// Nếu là xe tiêu
                    if (carriage != null)
                    {
                        KTTraderCarriageManager.HandleTraderCarriageLoaded(client, carriage);
                        return TCPProcessCmdResults.RESULT_OK;
                    }

                    /// Bot bán hàng
                    KStallBot stallBot = KTStallBotManager.FindBot(otherRoleID);
                    /// Nếu là Bot bán hàng
                    if (stallBot != null)
                    {
                        KTStallBotManager.HandleStallBotLoaded(client, stallBot);
                        return TCPProcessCmdResults.RESULT_OK;
                    }

                    /// Bot
                    KDecoBot bot = KTDecoBotManager.Find(otherRoleID);
                    /// Nếu là bot
                    if (bot != null)
                    {
                        KTDecoBotManager.HandleDecoBotLoaded(client, bot);
                        return TCPProcessCmdResults.RESULT_OK;
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
		#endregion
	}
}
