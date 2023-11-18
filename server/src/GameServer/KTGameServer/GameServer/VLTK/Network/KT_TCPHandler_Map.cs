using GameServer.KiemThe.Core.Task;
using GameServer.KiemThe.Logic.Manager.Battle;
using GameServer.Logic;
using GameServer.Server;
using GameServer.VLTK.Core.StallManager;
using Server.Data;
using Server.Protocol;
using Server.TCP;
using Server.Tools;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using static GameServer.Logic.KTMapManager;

namespace GameServer.KiemThe.Logic
{
    /// <summary>
    /// Quản lý chuyển cảnh
    /// </summary>
    public static partial class KT_TCPHandler
    {
        #region Enter map
        /// <summary>
        /// Xử lý gói tin gửi từ Client về Server thông báo đối tượng tải bản đồ thành công
        /// </summary>
        /// <param name="pool"></param>
        /// <param name="nID"></param>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static TCPProcessCmdResults ProcessSpriteEnterMap(TCPManager tcpMgr, TMSKSocket socket, TCPClientPool tcpClientPool, TCPRandKey tcpRandKey, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;
            string cmdData = null;

            try
            {
                cmdData = new ASCIIEncoding().GetString(data, 0, count);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Decode data faild, CMD={0}, Client={1}", (TCPGameServerCmds) nID, Global.GetSocketRemoteEndPoint(socket)));
                return TCPProcessCmdResults.RESULT_FAILED;
            }

            try
            {
                if (!string.IsNullOrEmpty(cmdData))
                {
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// Đối tượng gnười chơi tương ứng
                KPlayer client = KTPlayerManager.Find(socket);
                if (null == client)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Can not find player corresponding ID, CMD={0}, Client={1}, RoleID={2}", (TCPGameServerCmds) nID, Global.GetSocketRemoteEndPoint(socket), client.RoleID));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// Bỏ đánh dấu chờ chuyển Map
                client.WaitingForChangeMap = false;

                /// Thực hiện hàm OnEnterMap
                client.OnEnterMap();

                /// Gửi gói tin đồng bộ tốc đánh và tốc chạy
                KT_TCPHandler.NotifyTargetMoveSpeedChanged(client);
                KT_TCPHandler.NotifyTargetAttackSpeedChanged(client);

                /// Trả về NPC STATE của client này
                TaskManager.getInstance().ComputeNPCTaskState(client);

                /// Điểm mặc định
                Point zeroP = new Point(0, 0);
                /// Nếu vị trí hiện tại khác vị trí được thiết lập qua hàm chuyển
                if (client.LastChangedPosition != zeroP && client.CurrentPos != client.LastChangedPosition)
                {
                    /// Ghi Log
                    LogManager.WriteLog(LogTypes.RolePosition, string.Format("Bug change map pos of Client = {0} (ID: {1}), CurrentPos = {2}, LastChangedPos = {3}", client.RoleName, client.RoleID, client.CurrentPos.ToString(), client.LastChangedPosition.ToString()));
                    /// Gắn lại vị trí
                    client.CurrentPos = client.LastChangedPosition;
                    /// Thay đổi vị trí hiện tại của Client
					KTPlayerManager.ChangePos(client, client.PosX, client.PosY);
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

        #region Change map
        /// <summary>
        /// Xử lý gói tin gửi từ Client về Server thông báo đối tượng chuyển map
        /// </summary>
        /// <param name="pool"></param>
        /// <param name="nID"></param>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static TCPProcessCmdResults ProcessSpriteMapChangeCmd(TCPManager tcpMgr, TMSKSocket socket, TCPClientPool tcpClientPool, TCPRandKey tcpRandKey, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;
            SCMapChange cmdData = null;

            try
            {
                cmdData = DataHelper.BytesToObject<SCMapChange>(data, 0, count);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Decode data faild, CMD={0}, Client={1}", (TCPGameServerCmds) nID, Global.GetSocketRemoteEndPoint(socket)));
                return TCPProcessCmdResults.RESULT_FAILED;
            }

            try
            {
                int roleID = cmdData.RoleID;
                int teleportID = cmdData.TeleportID;
                int newMapCode = cmdData.MapCode;
                int toNewMapX = cmdData.PosX;
                int toNewMapY = cmdData.PosY;

                /// Đối tượng gnười chơi tương ứng
                KPlayer client = KTPlayerManager.Find(socket);
                if (null == client || client.RoleID != roleID)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Can not find player corresponding ID, CMD={0}, Client={1}", (TCPGameServerCmds) nID, Global.GetSocketRemoteEndPoint(socket)));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// Nếu đang bày bán
                if (StallManager.IsDirectStall(client))
                {
                    KTPlayerManager.ShowNotification(client, "Đang trong trạng thái bán hàng, không thể chuyển bản đồ!");
                    return TCPProcessCmdResults.RESULT_OK;
                }

                /// Dữ liệu trả lại Client
                SCMapChange scData = null;

                /// Bản đồ
                GameMap oldMap = KTMapManager.Find(client.CurrentMapCode);

                /// Di chuyển trực tiếp không qua điểm truyền tống
                if (teleportID < 0)
                {
                    /// Nếu không có đánh dấu đợi chuyển Map
                    if (!client.WaitingForChangeMap)
					{
                        LogManager.WriteLog(LogTypes.Robot, string.Format("{0} (ID: {1}) used Auto, BUG using direct transfer, not waiting for change map => Disconnect!", client.RoleName, client.RoleID));
                        /// Cho toác luôn
                        return TCPProcessCmdResults.RESULT_FAILED;
                    }

                    /// Gắn lại vị trí
                    newMapCode = client.WaitingChangeMapCode;
                    toNewMapX = client.WaitingChangeMapPosX;
                    toNewMapY = client.WaitingChangeMapPosY;

                    //               /// Vị trí đến khác vị trí đang chờ
                    //               if (client.WaitingChangeMapCode != newMapCode || client.WaitingChangeMapPosX != toNewMapX || client.WaitingChangeMapPosY != toNewMapY)
                    //{
                    //                   LogManager.WriteLog(LogTypes.Robot, string.Format("{0} (ID: {1}) used Auto, BUG using direct transfer, Server (ToMapCode: {2}, ToPosX: {3}, ToPosY: {4}), Client (ToMapCode: {5}, ToPosX: {6}, ToPosY: {7}) => Disconnect!", client.RoleName, client.RoleID, client.WaitingChangeMapCode, client.WaitingChangeMapPosX, client.WaitingChangeMapPosY, newMapCode, toNewMapX, toNewMapY));
                    //                   /// Cho toác luôn
                    //                   return TCPProcessCmdResults.RESULT_FAILED;
                    //               }

                    /// Bản đồ đích đến
                    GameMap toGameMap = KTMapManager.Find(newMapCode);
                    /// Nếu ID bản đồ đích không tồn tại
                    if (toGameMap == null)
                    {
                        scData = new SCMapChange()
                        {
                            RoleID = client.RoleID,
                            ErrorCode = 1,
                        };
                        client.SendPacket((int) TCPGameServerCmds.CMD_SPR_MAPCHANGE, scData);

                        return TCPProcessCmdResults.RESULT_OK;
                    }

                    int toLevel = toGameMap.MapLevel;
                    /// Nếu cấp độ không đủ vào bản đồ
                    if (client.m_Level < toLevel)
                    {
                        scData = new SCMapChange()
                        {
                            RoleID = client.RoleID,
                            ErrorCode = 2,
                        };
                        client.SendPacket((int) TCPGameServerCmds.CMD_SPR_MAPCHANGE, scData);

                        return TCPProcessCmdResults.RESULT_OK;
                    }

                    /// Nếu bản đồ hiện tại không phải bản đồ liên máy chủ nhưng bản đồ muốn tới lại là bản đồ liên máy chủ
                    if (!KuaFuMapManager.getInstance().IsKuaFuMap(client.MapCode) && KuaFuMapManager.getInstance().IsKuaFuMap(newMapCode))
                    {
                        if (ServerConfig.Instance.EnableCrossServer)
                        {
                            /// Lưu lại vị trí trước khi sang liên máy chủ
                            KT_TCPHandler.UpdateLastMapInfo(client, client.CurrentMapCode, client.PosX, client.PosY);

                            string[] cmdParams = new string[6];
                            cmdParams[0] = newMapCode + "";
                            cmdParams[1] = 1 + "";
                            cmdParams[2] = -1 + "";
                            cmdParams[3] = teleportID + "";
                            cmdParams[4] = toNewMapX + "";
                            cmdParams[5] = toNewMapY + "";

                            KuaFuMapManager.getInstance().ProcessKuaFuMapEnterCmd(client, (int)(TCPGameServerCmds.CMD_SPR_KUAFU_MAP_ENTER), null, cmdParams);
                        }
                        else
                        {
                            KTPlayerManager.ShowNotification(client, "Liên máy chủ hiện chưa mở");
                        }
                        return TCPProcessCmdResults.RESULT_OK;
                    }
                }
                /// Di chuyển thông qua điểm truyền tống
                else
                {
                    // NẾU CÓ TELEPORT ID
                    //Nếu bản đồ hiện tại không phải bản đồ liên máy chủ nhưng bản đồ muốn tới lại là bản đồ liên máy chủ
                    if (!KuaFuMapManager.getInstance().IsKuaFuMap(client.MapCode) && KuaFuMapManager.getInstance().IsKuaFuMap(newMapCode))
                    {
                        if (ServerConfig.Instance.EnableCrossServer)
                        {
                            /// Lưu lại vị trí trước khi sang liên máy chủ
                            KT_TCPHandler.UpdateLastMapInfo(client, client.CurrentMapCode, client.PosX, client.PosY);

                            string[] cmdParams = new string[4];

                            cmdParams[0] = newMapCode + "";
                            cmdParams[1] = 1 + "";
                            cmdParams[2] = -1 + "";
                            cmdParams[3] = teleportID + "";

                            KuaFuMapManager.getInstance().ProcessKuaFuMapEnterCmd(client, (int)(TCPGameServerCmds.CMD_SPR_KUAFU_MAP_ENTER), null, cmdParams);
                        }
                        else
                        {
                            KTPlayerManager.ShowNotification(client, "Liên máy chủ hiện chưa mở");
                        }
                        return TCPProcessCmdResults.RESULT_OK;
                    }

                    /// Bản đồ đang đứng
                    GameMap gameMap = KTMapManager.Find(client.MapCode);
                    /// Nếu ID bản đồ đang đứng không tồn tại
                    if (gameMap == null)
                    {
                        scData = new SCMapChange()
                        {
                            RoleID = client.RoleID,
                            ErrorCode = 0,
                        };
                        client.SendPacket((int) TCPGameServerCmds.CMD_SPR_MAPCHANGE, scData);

                        return TCPProcessCmdResults.RESULT_OK;
                    }

                    MapTeleport mapTeleport = null;
                    /// Nếu cổng Teleport không tồn tại
                    if (!gameMap.MapTeleportDict.TryGetValue(teleportID, out mapTeleport))
                    {
                        scData = new SCMapChange()
                        {
                            RoleID = client.RoleID,
                            ErrorCode = 3,
                        };
                        client.SendPacket((int) TCPGameServerCmds.CMD_SPR_MAPCHANGE, scData);

                        return TCPProcessCmdResults.RESULT_OK;
                    }

                    /// Nếu ở quá xa điểm truyền tống
                    if (KTGlobal.GetDistanceBetweenPoints(client.CurrentPos, new Point(mapTeleport.X, mapTeleport.Y)) >= mapTeleport.Radius)
					{
                        /// Toác
                        return TCPProcessCmdResults.RESULT_OK;
                    }

                    /// Bản đồ đích đến
                    GameMap toGameMap = KTMapManager.Find(mapTeleport.ToMapID);
                    /// Nếu bản đồ đích không tồn tại
                    if (toGameMap == null)
                    {
                        scData = new SCMapChange()
                        {
                            RoleID = client.RoleID,
                            ErrorCode = 1,
                        };
                        client.SendPacket((int) TCPGameServerCmds.CMD_SPR_MAPCHANGE, scData);

                        return TCPProcessCmdResults.RESULT_OK;
                    }

                    /// Thực thi sự kiện trước khi chuyển bản đồ
                    client.OnPreChangeMap(toGameMap);

                    int toLevel = toGameMap.MapLevel;
                    /// Nếu cấp độ không đủ vào bản đồ
                    if (client.m_Level < toLevel)
                    {
                        scData = new SCMapChange()
                        {
                            RoleID = client.RoleID,
                            ErrorCode = 2,
                        };
                        client.SendPacket((int) TCPGameServerCmds.CMD_SPR_MAPCHANGE, scData);

                        return TCPProcessCmdResults.RESULT_OK;
                    }

                    /// Nếu là tele trong tống kim
                    if (mapTeleport.Camp == 10 || mapTeleport.Camp == 20)
                    {
                        if (!Battel_SonJin_Manager.CanUsingTeleport(client))
                        {
                            KTPlayerManager.ShowNotification(client, "Chiến trường chưa bắt đầu");
                            return TCPProcessCmdResults.RESULT_OK;
                        }
						else
						{
                            Battel_SonJin_Manager.UseTeleport(client);
                            return TCPProcessCmdResults.RESULT_OK;
						}
                    }

                    /// Nếu thông tin ở Client gửi lên sai thì cho cút luôn
                    if (newMapCode != mapTeleport.ToMapID || toNewMapX != mapTeleport.ToX || toNewMapY != mapTeleport.ToY)
					{
                        LogManager.WriteLog(LogTypes.Robot, string.Format("{0} (ID: {1}) used Auto, BUG using Teleport => Disconnect!", client.RoleName, client.RoleID));
                        return TCPProcessCmdResults.RESULT_FAILED;
                    }

                    /// Thiết lập lại vị trí tương đương cổng teleport
                    newMapCode = mapTeleport.ToMapID;
                    toNewMapX = mapTeleport.ToX;
                    toNewMapY = mapTeleport.ToY;
                }


                /// Nếu dùng điểm truyền tống dịch chuyển đến vị trí trong cùng bản đồ
                if (teleportID >= 0 && client.MapCode == newMapCode)
                {
                    KTPlayerManager.ChangePos(client, toNewMapX, toNewMapY);

                    return TCPProcessCmdResults.RESULT_OK;
                }

                // Nếu là đang là bản đồ liên máy chủ mà bản đồ muốn tele tới lại ko phỉa liên máy chủ =====> Đây là command quay về
                if (KuaFuMapManager.getInstance().IsKuaFuMap(client.MapCode) && !KuaFuMapManager.getInstance().IsKuaFuMap(newMapCode))
                {
                    // Quay về vị trí trước đó đã sang bản đồ thế giới
                    KuaFuManager.getInstance().GotoLastMap(client);

                    return TCPProcessCmdResults.RESULT_OK;
                }

                /// Thực hiện chuyển bản đồ
                KT_TCPHandler.ProcessChangeMap(client, teleportID, newMapCode, toNewMapX, toNewMapY, (int) client.CurrentDir);

                /// Thiết lập vị trí đích đến
                client.ToPos = new Point(toNewMapX, toNewMapY);

                /// Thực thi sự kiện OnChangeMap
                client.OnChangeMap(oldMap);

                return TCPProcessCmdResults.RESULT_OK;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, Global.GetDebugHelperInfo(socket), false);
            }

            return TCPProcessCmdResults.RESULT_FAILED;
        }

        /// <summary>
        /// Thực hiện chuyển bản đồ
        /// </summary>
        /// <param name="client"></param>
        /// <param name="teleport"></param>
        /// <param name="toMapCode"></param>
        /// <param name="toMapX"></param>
        /// <param name="toMapY"></param>
        /// <param name="toMapDirection"></param>
        /// <returns></returns>
        private static void ProcessChangeMap(KPlayer client, int teleport, int toMapCode, int toMapX, int toMapY, int toMapDirection)
        {
            /// Ngừng di chuyển
            KTPlayerStoryBoardEx.Instance.Remove(client);
            /// Ngừng Blink
            client.StopBlink();

            /// Xóa đối tượng khỏi vị trí hiện tại
            KTPlayerManager.PlayerContainer.Remove(client);

            if (toMapCode > 0)
            {
                GameMap gameMap = KTMapManager.Find(toMapCode);
                /// Nếu bản đồ tồn tại
                if (null != gameMap)
                {
                    /// Nếu bản đồ không thể vào được
                    if (!gameMap.CanMove(toMapX / gameMap.MapGridWidth, toMapY / gameMap.MapGridHeight, client.CurrentCopyMapID))
                    {
                        toMapX = -1;
                        toMapY = -1;
                    }
                }
                /// Nếu bản đồ không tồn tại
                else
                {
                    toMapCode = -1;
                }
            }

            GameMap toMap = null;
            /// Nếu tồn tại vị trí đích đến
            if (toMapX <= 0 || toMapY <= 0)
            {
                KiemThe.Entities.NewbieVillage newbieVillage = KTGlobal.NewbieVillages[KTGlobal.GetRandomNumber(0, KTGlobal.NewbieVillages.Count - 1)];
                int newbieMapCode = newbieVillage.ID;
                int newbiePosX = newbieVillage.Position.X;
                int newbiePosY = newbieVillage.Position.Y;
                toMap = KTMapManager.Find(newbieMapCode);
                Point newPos = new Point(newbiePosX, newbiePosY);
                toMapX = (int) newPos.X;
                toMapY = (int) newPos.Y;
            }

            /// Đánh dấu đang đợi chuyển Map
            client.WaitingForChangeMap = true;

            /// Thiết lập vị trí và bản đồ mới
            client.MapCode = toMapCode;
            client.PosX = toMapX;
            client.PosY = toMapY;
            /// Ghi lại vị trí hợp lệ
            client.LastValidPos = new Point(toMapX, toMapY);

            /// Ghi lại vị trí đích đến
            client.LastChangedPosition = new Point(toMapX, toMapY);

            /// Cập nhật động tác của đối tượng
            client.CurrentAction = (int) GameServer.KiemThe.Entities.KE_NPC_DOING.do_stand;

            /// Thêm đối tượng vào danh sách quản lý
            KTPlayerManager.PlayerContainer.Add(client);

            /// Gửi gói tin về Client
            SCMapChange scData = new SCMapChange()
            {
                MapCode = toMapCode,
                PosX = toMapX,
                PosY = toMapY,
                RoleID = client.RoleID,
                TeleportID = teleport,
                ErrorCode = -1,
                Direction = toMapDirection,
            };
            client.SendPacket((int) TCPGameServerCmds.CMD_SPR_MAPCHANGE, scData);
        }

        /// <summary>
        /// Thông báo tới bản thân chuyển bản đồ
        /// </summary>
        /// <param name="player"></param>
        /// <param name="mapCode"></param>
        /// <param name="posX"></param>
        /// <param name="posY"></param>
        public static void NotifyChangeMap(KPlayer player, int mapCode, int posX, int posY)
        {
            /// Dữ liệu
            string strcmd = string.Format("{0}:{1}:{2}:{3}:{4}:{5}", player.RoleID, mapCode, posX, posY, (int) player.CurrentDir, 0);
            /// Gửi gói tin
            player.SendPacket((int) TCPGameServerCmds.CMD_SPR_NOTIFYCHGMAP, strcmd);
        }
        #endregion

        #region Special monsters
        /// <summary>
        /// Xử lý gói tin gửi từ Client về Server yêu cầu cập nhật vị trí quái đặc biệt trong toàn bản đồ
        /// </summary>
        /// <param name="pool"></param>
        /// <param name="nID"></param>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static TCPProcessCmdResults ProcessGetLocalMapSpecialMonsters(TCPManager tcpMgr, TMSKSocket socket, TCPClientPool tcpClientPool, TCPRandKey tcpRandKey, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;
            string cmdData = null;

            try
            {
                cmdData = new ASCIIEncoding().GetString(data, 0, count);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Decode data faild, CMD={0}, Client={1}", (TCPGameServerCmds) nID, Global.GetSocketRemoteEndPoint(socket)));
                return TCPProcessCmdResults.RESULT_FAILED;
            }

            try
            {
                if (!string.IsNullOrEmpty(cmdData))
                {
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// Đối tượng gnười chơi tương ứng
                KPlayer client = KTPlayerManager.Find(socket);
                if (null == client)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Can not find player corresponding ID, CMD={0}, Client={1}", (TCPGameServerCmds) nID, Global.GetSocketRemoteEndPoint(socket)));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// Nếu chưa đến thời gian
                if (KTGlobal.GetCurrentTimeMilis() - client.LastUpdateLocalMapMonsterTicks < 5000)
                {
                    /// Bỏ qua
                    return TCPProcessCmdResults.RESULT_OK;
                }
                /// Đánh dấu thời gian
                client.LastUpdateLocalMapMonsterTicks = KTGlobal.GetCurrentTimeMilis();

                /// Kết quả
                List<LocalMapMonsterData> monsters = new List<LocalMapMonsterData>();

                ///// Tạm thời Diss bỏ
                ///// Duyệt danh sách quái trong bản đồ
                //List<Monster> objs = GameManager.MonsterMgr.GetContinuouslyUpdateToMiniMapMonstersByMap(client.CurrentMapCode, client.CurrentCopyMapID);
                ///// Toác
                //if (objs == null)
                //{
                //    /// Bỏ qua
                //    return TCPProcessCmdResults.RESULT_OK;
                //}
                ///// Duyệt danh sách
                //foreach (Monster obj in objs)
                //{
                //    monsters.Add(new LocalMapMonsterData()
                //    {
                //        Name = string.IsNullOrEmpty(obj.LocalMapName) ? obj.RoleName : obj.LocalMapName,
                //        PosX = (int) obj.CurrentPos.X,
                //        PosY = (int) obj.CurrentPos.Y,
                //        IsBoss = obj.MonsterType == Entities.MonsterAIType.Special_Boss || obj.MonsterType == Entities.MonsterAIType.Boss || obj.MonsterType == Entities.MonsterAIType.Pirate || obj.MonsterType == Entities.MonsterAIType.Elite || obj.MonsterType == Entities.MonsterAIType.Leader,
                //    });
                //}

                ///// Nếu danh sách rỗng
                //if (monsters.Count <= 0)
                //{
                //    /// Bỏ qua
                //    return TCPProcessCmdResults.RESULT_OK;
                //}

                ///// Gửi lại gói tin về Client
                //byte[] byteData = DataHelper.ObjectToBytes<List<LocalMapMonsterData>>(monsters);
                //tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, byteData, nID);
                //return TCPProcessCmdResults.RESULT_DATA;

                return TCPProcessCmdResults.RESULT_OK;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, Global.GetDebugHelperInfo(socket), false);
            }

            return TCPProcessCmdResults.RESULT_FAILED;
        }
        #endregion

        #region Last map
        /// <summary>
        /// Ghi lại thông tin bản đồ lần trước của người chơi
        /// </summary>
        /// <param name="player"></param>
        /// <param name="mapCode"></param>
        /// <param name="posX"></param>
        /// <param name="posY"></param>
        public static void UpdateLastMapInfo(KPlayer player, int mapCode, int posX, int posY)
        {
            player.SetValueOfForeverRecore(ForeverRecord.LastMapID, mapCode);
            player.SetValueOfForeverRecore(ForeverRecord.LastMapPosX, posX);
            player.SetValueOfForeverRecore(ForeverRecord.LastMapPosY, posY);
        }

        /// <summary>
        /// Trả về thông tin bản đồ lần trước của người chơi
        /// </summary>
        /// <param name="player"></param>
        /// <param name="mapCode"></param>
        /// <param name="posX"></param>
        /// <param name="posY"></param>
        public static void GetLastMapInfo(KPlayer player, out int mapCode, out int posX, out int posY)
        {
            mapCode = player.GetValueOfForeverRecore(ForeverRecord.LastMapID);
            posX = player.GetValueOfForeverRecore(ForeverRecord.LastMapPosX);
            posY = player.GetValueOfForeverRecore(ForeverRecord.LastMapPosY);
        }
        #endregion

        #region CopyScene
        /// <summary>
        /// Ghi lại thông tin phụ bản của người chơi
        /// </summary>
        /// <param name="player"></param>
        /// <param name="copySceneID"></param>
        /// <param name="copySceneCreateTicks"></param>
        public static void UpdateCopySceneInfo(KPlayer player, int copySceneID, int copySceneCreateTicks)
        {
            player.SetValueOfForeverRecore(ForeverRecord.LastCopySceneID, copySceneID);
            player.SetValueOfForeverRecore(ForeverRecord.LastCopySceneCreatedTicks, copySceneCreateTicks);
        }

        /// <summary>
        /// Trả về thông tin phụ bản của người chơi
        /// </summary>
        /// <param name="player"></param>
        /// <param name="copySceneID"></param>
        /// <param name="copySceneCreateTicks"></param>
        public static void GetCopySceneInfo(KPlayer player, out int copySceneID, out int copySceneCreateTicks)
        {
            /// Kết quả
            copySceneID = player.GetValueOfForeverRecore(ForeverRecord.LastCopySceneID);
            copySceneCreateTicks = player.GetValueOfForeverRecore(ForeverRecord.LastCopySceneCreatedTicks);
        }
        #endregion
    }
}
