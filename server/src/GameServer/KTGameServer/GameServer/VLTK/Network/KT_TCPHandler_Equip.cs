using GameServer.KiemThe.Core.Item;
using GameServer.KiemThe.Entities;
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
using static GameServer.KiemThe.KTGlobal;
using static GameServer.Logic.Global;
using static GameServer.Logic.KTMapManager;

namespace GameServer.KiemThe.Logic
{
    /// <summary>
    /// Quản lý trang bị
    /// </summary>
    public static partial class KT_TCPHandler
    {
        #region Thay đổi trang bị

        /// <summary>
        /// Gửi gói tin tới tất cả người chơi xung quanh thông báo trang bị bản thân thay đổi
        /// </summary>
        /// <param name="player"></param>
        /// <param name="itemGD"></param>
        /// <param name="type"></param>
        public static void SendPlayerEquipChangeToNearbyPlayers(KPlayer player, GoodsData itemGD, int type)
        {
            try
            {
                ChangeEquipData changeEquipData = new ChangeEquipData()
                {
                    RoleID = player.RoleID,
                    EquipGoodsData = itemGD,
                    Type = type,
                };

                byte[] cmdData = DataHelper.ObjectToBytes<ChangeEquipData>(changeEquipData);

                /// Tìm tất cả người chơi xung quanh để gửi gói tin
                List<KPlayer> listObjects = KTRadarMapManager.GetPlayersAround(player);
                if (listObjects == null)
                {
                    return;
                }
                /// Gửi gói tin đến tất cả người chơi xung quanh
                KTPlayerManager.SendPacketToPlayers((int) TCPGameServerCmds.CMD_SPR_CHGCODE, listObjects, cmdData, player, null);
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Exception, ex.ToString());
            }
        }

        #endregion Thay đổi trang bị

        #region Cường hóa trang bị

        /// <summary>
        /// Phản hồi yêu cầu cường hóa trang bị từ Client
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
        public static TCPProcessCmdResults ResponseEquipEnhance(TCPManager tcpMgr, TMSKSocket socket, TCPClientPool tcpClientPool, TCPRandKey tcpRandKey, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;

            EquipEnhance equipEnhance = null;
            try
            {
                /// Giải mã gói tin đẩy về dạng ProtoBytes
                equipEnhance = DataHelper.BytesToObject<EquipEnhance>(data, 0, count);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Error while getting DATA, CMD={0}, Client={1}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket)));
                return TCPProcessCmdResults.RESULT_FAILED;
            }

            try
            {
                /// Nếu dữ liệu trong gói tin không tồn tại
                if (equipEnhance == null)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Error Socket params count not fit CMD={0}, Client={1}, Recv={2}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), 0));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// Tìm chủ nhân của gói tin tương ứng
                KPlayer client = KTPlayerManager.Find(socket);
                if (null == client)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Không tìm thấy thông tin người chơi, CMD={0}, Client={1}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket)));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }
                
                //Nếu có mạt khẩu cấp 2
                if (client.NeedToShowInputSecondPassword())
                {
                    return TCPProcessCmdResults.RESULT_OK;
                }
                /// Bản đồ hiện tại
                GameMap gameMap = KTMapManager.Find(client.CurrentMapCode);
                /// Nếu không phải ở thành thị, tần lăng
                //            if (gameMap.MapType != "village" && gameMap.MapType != "city" && gameMap.MapType != "qinshihuangling_1")
                //{
                //                /// Cho toác luôn
                //                return TCPProcessCmdResults.RESULT_FAILED;
                //}

                /// Nếu không thấy NPC dã luyện
                if (client.LastEquipMasterNPC == null || client.LastEquipMasterNPC.MapCode != client.CurrentMapCode)
                {
                    /// Cho toác luôn
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// Nếu khoảng cách quá xa so với NPC dã luyện
                if (KTGlobal.GetDistanceBetweenPoints(client.CurrentPos, client.LastEquipMasterNPC.CurrentPos) > 100)
                {
                    KTPlayerManager.ShowNotification(client, "Khoảng cách tới Dã Luyện Đại Sư quá xa!");
                    return TCPProcessCmdResults.RESULT_OK;
                }

                /// Vật phẩm tương ứng trong túi người chơi
                GoodsData itemGD = client.GoodsData.Find(equipEnhance.EquipDbID, 0);
                /// Nếu vật phẩm tương ứng khong tồn tại trong túi người chơi
                if (itemGD == null)
                {
                    KTPlayerManager.ShowNotification(client, "Vị trí trang bị không tồn tại!");
                    return TCPProcessCmdResults.RESULT_OK;
                }

                /// Nếu không có danh sách Huyền Tinh
                if (equipEnhance.CrystalStonesDbID == null || equipEnhance.CrystalStonesDbID.Count <= 0)
                {
                    KTPlayerManager.ShowNotification(client, "Không có Huyền Tinh, không thể cường hóa trang bị!");
                    return TCPProcessCmdResults.RESULT_OK;
                }

                /// Danh sách Huyền Tinh trong túi
                List<GoodsData> crystalStones = client.GoodsData.FindAll(x => x.Site == 0 && equipEnhance.CrystalStonesDbID.Contains(x.Id));
                /// Nếu độ dài truyền lên và danh sách tìm được không khớp
                if (crystalStones.Count != equipEnhance.CrystalStonesDbID.Count)
                {
                    KTPlayerManager.ShowNotification(client, "Danh sách Huyền Tinh không hợp lệ!");
                    return TCPProcessCmdResults.RESULT_OK;
                }

                int ret = ItemEnhance.DoEnhItem(crystalStones, itemGD, client, (MoneyType)equipEnhance.MoneyType);
                string cmdData = string.Format("{0}", ret);
                client.SendPacket((int)TCPGameServerCmds.CMD_KT_EQUIP_ENHANCE, cmdData);

                return TCPProcessCmdResults.RESULT_OK;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, Global.GetDebugHelperInfo(socket), false);
            }

            return TCPProcessCmdResults.RESULT_FAILED;
        }

        /// <summary>
        /// Phản hồi yêu cầu ghép Huyền Tinh từ Client
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
        public static TCPProcessCmdResults ResponseComposeCrystalStones(TCPManager tcpMgr, TMSKSocket socket, TCPClientPool tcpClientPool, TCPRandKey tcpRandKey, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;

            CrystalStoneCompose crystalStoneCompose = null;
            try
            {
                /// Giải mã gói tin đẩy về dạng ProtoBytes
                crystalStoneCompose = DataHelper.BytesToObject<CrystalStoneCompose>(data, 0, count);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Error while getting DATA, CMD={0}, Client={1}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket)));
                return TCPProcessCmdResults.RESULT_FAILED;
            }

            try
            {
                /// Nếu dữ liệu trong gói tin không tồn tại
                if (crystalStoneCompose == null)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Error Socket params count not fit CMD={0}, Client={1}, Recv={2}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), 0));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// Tìm chủ nhân của gói tin tương ứng
                KPlayer client = KTPlayerManager.Find(socket);
                if (null == client)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Không tìm thấy thông tin người chơi, CMD={0}, Client={1}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket)));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// Bản đồ hiện tại
                GameMap gameMap = KTMapManager.Find(client.CurrentMapCode);

                /// Mở khóa an toàn
                if (client.NeedToShowInputSecondPassword())
                {
                    return TCPProcessCmdResults.RESULT_OK;
                }


                /// Nếu không thấy NPC dã luyện
                if (client.LastEquipMasterNPC == null || client.LastEquipMasterNPC.MapCode != client.CurrentMapCode)
                {
                    /// Cho toác luôn
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// Nếu khoảng cách quá xa so với NPC dã luyện
                if (KTGlobal.GetDistanceBetweenPoints(client.CurrentPos, client.LastEquipMasterNPC.CurrentPos) > 100)
                {
                    KTPlayerManager.ShowNotification(client, "Khoảng cách tới Dã Luyện Đại Sư quá xa!");
                    return TCPProcessCmdResults.RESULT_OK;
                }

                /// Nếu không có danh sách Huyền Tinh
                if (crystalStoneCompose.CrystalStonesDbID == null || crystalStoneCompose.CrystalStonesDbID.Count <= 0)
                {
                    KTPlayerManager.ShowNotification(client, "Không có Huyền Tinh, không thể ghép!");
                    return TCPProcessCmdResults.RESULT_OK;
                }

                /// Danh sách Huyền Tinh trong túi
                List<GoodsData> crystalStones = client.GoodsData.FindAll(x => x.Site == 0 && crystalStoneCompose.CrystalStonesDbID.Contains(x.Id));
                /// Nếu độ dài truyền lên và danh sách tìm được không khớp
                if (crystalStones.Count != crystalStoneCompose.CrystalStonesDbID.Count)
                {
                    KTPlayerManager.ShowNotification(client, "Danh sách Huyền Tinh không hợp lệ!");
                    return TCPProcessCmdResults.RESULT_OK;
                }

                bool ret = ItemEnhance.ComposeItemCrystal(crystalStones, client, (MoneyType)crystalStoneCompose.MoneyType);
                string cmdData = string.Format("{0}", ret ? 1 : 0);
                client.SendPacket((int) TCPGameServerCmds.CMD_KT_COMPOSE_CRYSTALSTONES, cmdData);

                return TCPProcessCmdResults.RESULT_OK;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, Global.GetDebugHelperInfo(socket), false);
            }

            return TCPProcessCmdResults.RESULT_FAILED;
        }

        /// <summary>
        /// Phản hồi yêu cầu tách Huyền Tinh ở trang bị từ Client
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
        public static TCPProcessCmdResults ResponseEquipSplitCrystalStones(TCPManager tcpMgr, TMSKSocket socket, TCPClientPool tcpClientPool, TCPRandKey tcpRandKey, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;

            CrystalStoneSplit crystalStoneSplit = null;
            try
            {
                /// Giải mã gói tin đẩy về dạng ProtoBytes
                crystalStoneSplit = DataHelper.BytesToObject<CrystalStoneSplit>(data, 0, count);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Error while getting DATA, CMD={0}, Client={1}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket)));
                return TCPProcessCmdResults.RESULT_FAILED;
            }

            try
            {
                /// Nếu dữ liệu trong gói tin không tồn tại
                if (crystalStoneSplit == null)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Error Socket params count not fit CMD={0}, Client={1}, Recv={2}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), 0));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// Tìm chủ nhân của gói tin tương ứng
                KPlayer client = KTPlayerManager.Find(socket);
                if (null == client)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Không tìm thấy thông tin người chơi, CMD={0}, Client={1}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket)));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// Bản đồ hiện tại
                GameMap gameMap = KTMapManager.Find(client.CurrentMapCode);

                /// Mở khóa an toàn
                if (client.NeedToShowInputSecondPassword())
                {
                    return TCPProcessCmdResults.RESULT_OK;
                }
                /// Nếu không thấy NPC dã luyện
                if (client.LastEquipMasterNPC == null || client.LastEquipMasterNPC.MapCode != client.CurrentMapCode)
                {
                    /// Cho toác luôn
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// Nếu khoảng cách quá xa so với NPC dã luyện
                if (KTGlobal.GetDistanceBetweenPoints(client.CurrentPos, client.LastEquipMasterNPC.CurrentPos) > 100)
                {
                    KTPlayerManager.ShowNotification(client, "Khoảng cách tới Dã Luyện Đại Sư quá xa!");
                    return TCPProcessCmdResults.RESULT_OK;
                }

                /// Vật phẩm tương ứng trong túi người chơi
                GoodsData itemGD = client.GoodsData.Find(crystalStoneSplit.EquipDbID, 0);
                /// Nếu vật phẩm tương ứng khong tồn tại trong túi người chơi
                if (itemGD == null)
                {
                    KTPlayerManager.ShowNotification(client, "Vị trí trang bị không tồn tại!");
                    return TCPProcessCmdResults.RESULT_OK;
                }

                bool ret;
                /// Nếu là tách từ trang bị
                if (crystalStoneSplit.MoneyType == -1)
                {
                    ret = ItemEnhance.DoSplitCrystalFromEquip(itemGD, client);
                }
                /// Nếu là tách từ Huyền Tinh cấp cao
                else
                {
                    ret = ItemEnhance.DoSplitCrystal(itemGD, client);
                }
                string cmdData = string.Format("{0}", ret ? 1 : 0);
                client.SendPacket((int) TCPGameServerCmds.CMD_KT_SPLIT_CRYSTALSTONES, cmdData);

                return TCPProcessCmdResults.RESULT_OK;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, Global.GetDebugHelperInfo(socket), false);
            }

            return TCPProcessCmdResults.RESULT_FAILED;
        }

        #endregion Cường hóa trang bị

        #region Cường hóa Ngũ hành ấn

        /// <summary>
        /// Phản hồi yêu cầu cường hóa trang bị từ Client
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
        public static TCPProcessCmdResults ResponseSignetEnhance(TCPManager tcpMgr, TMSKSocket socket, TCPClientPool tcpClientPool, TCPRandKey tcpRandKey, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;

            SignetEnhance signetEnhance = null;
            try
            {
                /// Giải mã gói tin đẩy về dạng ProtoBytes
                signetEnhance = DataHelper.BytesToObject<SignetEnhance>(data, 0, count);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Error while getting DATA, CMD={0}, Client={1}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket)));
                return TCPProcessCmdResults.RESULT_FAILED;
            }

            try
            {
                /// Nếu dữ liệu trong gói tin không tồn tại
                if (signetEnhance == null)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Error Socket params count not fit CMD={0}, Client={1}, Recv={2}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), 0));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// Tìm chủ nhân của gói tin tương ứng
                KPlayer client = KTPlayerManager.Find(socket);
                if (null == client)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Không tìm thấy thông tin người chơi, CMD={0}, Client={1}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket)));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// Bản đồ hiện tại
                GameMap gameMap = KTMapManager.Find(client.CurrentMapCode);

                /// Nếu có khóa cấp 2
                if (client.NeedToShowInputSecondPassword())
                {
                    return TCPProcessCmdResults.RESULT_OK;
                }
                /// Nếu không thấy NPC dã luyện
                if (client.LastEquipMasterNPC == null || client.LastEquipMasterNPC.MapCode != client.CurrentMapCode)
                {
                    /// Cho toác luôn
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// Nếu khoảng cách quá xa so với NPC dã luyện
                if (KTGlobal.GetDistanceBetweenPoints(client.CurrentPos, client.LastEquipMasterNPC.CurrentPos) > 100)
                {
                    KTPlayerManager.ShowNotification(client, "Khoảng cách tới Dã Luyện Đại Sư quá xa!");
                    return TCPProcessCmdResults.RESULT_OK;
                }

                /// Vật phẩm tương ứng trong túi người chơi
                GoodsData itemGD = client.GoodsData.Find(signetEnhance.SignetDbID, 0);
                /// Nếu vật phẩm tương ứng khong tồn tại trong túi người chơi
                if (itemGD == null)
                {
                    KTPlayerManager.ShowNotification(client, "Vị trí trang bị không tồn tại!");
                    return TCPProcessCmdResults.RESULT_OK;
                }

                /// Nếu không có danh sách Huyền Tinh
                if (signetEnhance.FSDbID == null || signetEnhance.FSDbID.Count <= 0)
                {
                    KTPlayerManager.ShowNotification(client, "Không có Ngũ Hành Hồn Thạch, không thể cường hóa trang bị!");
                    return TCPProcessCmdResults.RESULT_OK;
                }

                /// Danh sách Ngũ Hành Hồn Thạch trong túi
                List<GoodsData> fsList = client.GoodsData.FindAll(x => x.Site == 0 && signetEnhance.FSDbID.Contains(x.Id));
                /// Nếu độ dài truyền lên và danh sách tìm được không khớp
                if (fsList.Count != fsList.Count)
                {
                    KTPlayerManager.ShowNotification(client, "Danh sách Ngũ Hành Hồn Thạch không hợp lệ!");
                    return TCPProcessCmdResults.RESULT_OK;
                }

                int ret = ItemEnhance.DoEnhSignet(fsList, itemGD, client, signetEnhance.Type);
                string cmdData = string.Format("{0}", ret);
                client.SendPacket((int) TCPGameServerCmds.CMD_KT_SIGNET_ENHANCE, cmdData);

                return TCPProcessCmdResults.RESULT_OK;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, Global.GetDebugHelperInfo(socket), false);
            }

            return TCPProcessCmdResults.RESULT_FAILED;
        }

        #endregion Cường hóa Ngũ hành ấn

        #region Trang bị dự phòng

        /// <summary>
        /// Phản hồi yêu cầu đổi trang bị dự phòng từ Client
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
        public static TCPProcessCmdResults ResponseChangeSubEquipSet(TCPManager tcpMgr, TMSKSocket socket, TCPClientPool tcpClientPool, TCPRandKey tcpRandKey, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;

            string cmdData = "";
            try
            {
                /// Giải mã gói tin đẩy về dạng ProtoBytes
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
                    LogManager.WriteLog(LogTypes.Error, string.Format("Error Socket params count not fit CMD={0}, Client={1}, Recv={2}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), cmdData));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// Tìm chủ nhân của gói tin tương ứng
                KPlayer client = KTPlayerManager.Find(socket);
                if (null == client)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Không tìm thấy thông tin người chơi, CMD={0}, Client={1}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket)));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// Nếu đang trong trạng thái bị khống chế
                if (!client.IsCanDoLogic())
                {
                    KTPlayerManager.ShowNotification(client, "Đang trong trạng thái bị khống chế, không thể thay đổi trạng thái cưỡi ngựa!");
                    return TCPProcessCmdResults.RESULT_OK;
                }

                /// Nếu chưa hết thời gian giãn cách thay đổi trạng thái ngựa
                if (KTGlobal.GetCurrentTimeMilis() - client.LastTickToggleHorseState < KTGlobal.TickHorseStateChange)
                {
                    long tickLeft = KTGlobal.TickHorseStateChange - KTGlobal.GetCurrentTimeMilis() + client.LastTickToggleHorseState;
                    int sec = Math.Max(1, (int)tickLeft / 1000);

                    KTPlayerManager.ShowNotification(client, string.Format("Không thể thay đổi trang bị dự phòng lúc này, hãy chờ {0} giây rồi thử lại!", sec));
                    return TCPProcessCmdResults.RESULT_OK;
                }
                /// Cập nhật thời gian thay trạng thái cưỡi
                client.LastTickToggleHorseState = KTGlobal.GetCurrentTimeMilis();

                /// Lấy thông tin ngựa tương ứng ở trang bị
                GoodsData horseGD = client.GoodsData.Find(x => x.Using == (int)KE_EQUIP_POSITION.emEQUIPPOS_HORSE);
                /// Nếu có ngựa
                if (horseGD != null)
                {
                    /// Nếu đang trong trạng thái cưỡi
                    if (client.IsRiding)
                    {
                        client.GetPlayEquipBody().ClearAllEffecEquipBody();
                        /// Chuyển trạng thái sang cưỡi ngựa
                        client.IsRiding = false;

                        client.GetPlayEquipBody().AttackAllEquipBody();

                        /// Lưu vào DB
                        Global.SaveRoleParamsInt32ValueToDB(client, RoleParamName.HorseToggleOn, client.IsRiding ? 1 : 0, true);

                        /// Gửi thông báo tới tất cả người chơi xung quanh
                        KT_TCPHandler.SendHorseStateChanged(client);
                    }
                }

                /// Thực hiện đổi trang bị dự phòng
                client.GetPlayEquipBody().SwapSet();

                /// Thông báo tới tất cả người chơi xung quanh
                KT_TCPHandler.NotifyOtherMyselfSwapEquipSet(client);

                /// Thông báo về bản thân cập nhật UI
                RoleAttributes atrtribute = client.GetRoleAttributes();
                KeyValuePair<RoleAttributes, bool> pairData = new KeyValuePair<RoleAttributes, bool>(atrtribute, true);
                client.SendPacket<KeyValuePair<RoleAttributes, bool>>((int)TCPGameServerCmds.CMD_KT_ROLE_ATRIBUTES, pairData);

                return TCPProcessCmdResults.RESULT_OK;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, Global.GetDebugHelperInfo(socket), false);
            }

            return TCPProcessCmdResults.RESULT_FAILED;
        }

        /// <summary>
        /// Thông báo tới tất cả người chơi xung quanh (không bao gồm bản thân) bản thân thay đổi Set dự phòng
        /// </summary>
        /// <param name="player"></param>
        public static void NotifyOtherMyselfSwapEquipSet(KPlayer player)
        {
            RoleMiniEquipID miniEquip = KTGlobal.GetRoleEquipDataMini(player);
            int armorID = miniEquip.ArmorID;
            int helmID = miniEquip.HelmID;
            int weaponID = miniEquip.WeaponID;
            int weaponEnhanceLevel = miniEquip.WeaponEnhanceLevel;
            int weaponSeries = miniEquip.WeaponSeries;
            int mantleID = miniEquip.MantleID;
            int horseID = miniEquip.HorseID;
            int maskID = miniEquip.MaskID;
            bool isRiding = player.IsRiding;

            RoleDataMini roleData = new RoleDataMini()
            {
                RoleID = player.RoleID,
                ArmorID = armorID,
                HelmID = helmID,
                WeaponID = weaponID,
                HorseID = horseID,
                IsRiding = isRiding,
                MantleID = mantleID,
                WeaponEnhanceLevel = weaponEnhanceLevel,
                WeaponSeries = weaponSeries,
                MaskID = maskID,
            };
            byte[] cmdData = DataHelper.ObjectToBytes<RoleDataMini>(roleData);

            /// Tìm tất cả người chơi xung quanh để gửi gói tin
            List<KPlayer> listObjects = KTRadarMapManager.GetPlayersAround(player);
            if (listObjects == null)
            {
                return;
            }
            /// Thông báo tới tất cả người chơi khác trang bị bản thân thay đổi
            KTPlayerManager.SendPacketToPlayers((int) TCPGameServerCmds.CMD_KT_G2C_UPDATE_OTHERROLE_EQUIP, listObjects, cmdData, player, player);
        }

        #endregion Trang bị dự phòng

        #region Luyện hóa trang bị
        /// <summary>
        /// Luyện hóa trang bị
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
        public static TCPProcessCmdResults CMD_KT_CLIENT_DO_REFINE(TCPManager tcpMgr, TMSKSocket socket, TCPClientPool tcpClientPool, TCPRandKey tcpRandKey, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;

            G2C_EquipRefine Client_Refine = null;
            try
            {
                /// Giải mã gói tin đẩy về dạng ProtoBytes
                Client_Refine = DataHelper.BytesToObject<G2C_EquipRefine>(data, 0, count);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Error while getting DATA, CMD={0}, Client={1}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket)));
                return TCPProcessCmdResults.RESULT_FAILED;
            }

            try
            {
                /// Nếu dữ liệu trong gói tin không tồn tại
                if (Client_Refine == null)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Error Socket params count not fit CMD={0}, Client={1}, Recv={2}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), 0));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// Tìm chủ nhân của gói tin tương ứng
                KPlayer client = KTPlayerManager.Find(socket);
                if (null == client)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Không tìm thấy thông tin người chơi, CMD={0}, Client={1}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket)));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// Bản đồ hiện tại
                GameMap gameMap = KTMapManager.Find(client.CurrentMapCode);

                /// Mở khóa an toàn
                if (client.NeedToShowInputSecondPassword())
                {
                    return TCPProcessCmdResults.RESULT_OK;
                }
                /// Nếu không thấy NPC dã luyện
                //if (client.LastEquipMasterNPC == null || client.LastEquipMasterNPC.MapCode != client.CurrentMapCode)
                //{
                //    /// Cho toác luôn
                //    return TCPProcessCmdResults.RESULT_FAILED;
                //}

                /// Nếu khoảng cách quá xa so với NPC dã luyện
                //if (KTGlobal.GetDistanceBetweenPoints(client.CurrentPos, client.LastEquipMasterNPC.CurrentPos) > 100)
                //{
                //    KTPlayerManager.ShowNotification(client, "Khoảng cách tới Dã Luyện Đại Sư quá xa!");
                //    return TCPProcessCmdResults.RESULT_OK;
                //}

                GoodsData InputItemm = client.GoodsData.Find(Client_Refine.EquipDbID, 0);

                if (InputItemm == null)
                {
                    KTPlayerManager.ShowNotification(client, "Trang bị đặt vào không tồn tại");
                    return TCPProcessCmdResults.RESULT_OK;
                }

                GoodsData Suff = client.GoodsData.Find(Client_Refine.RecipeDbID, 0);

                if (Suff == null)
                {
                    KTPlayerManager.ShowNotification(client, "Công thức không tồn tại");
                    return TCPProcessCmdResults.RESULT_OK;
                }

                List<GoodsData> ListCrytal = new List<GoodsData>();

                if (Client_Refine.CrystalStoneDbIDs != null)
                {
                    if (Client_Refine.CrystalStoneDbIDs.Count > 0)
                    {
                        foreach (int Item in Client_Refine.CrystalStoneDbIDs)
                        {
                            GoodsData CrytalItem = client.GoodsData.Find(Item, 0);
                            if (CrytalItem != null)
                            {
                                ListCrytal.Add(CrytalItem);
                            }
                        }
                    }
                }

                int Rep = ItemRefine.DoRefine(client, InputItemm, Suff, ListCrytal, Client_Refine.ProductGoodsID);
                if (Rep < 0)
                {
                    return TCPProcessCmdResults.RESULT_OK;
                }

                /// Vật phẩm tương ứng trong túi người chơi
                string cmdData = "";
                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, cmdData, nID);
                return TCPProcessCmdResults.RESULT_DATA;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, Global.GetDebugHelperInfo(socket), false);
            }

            return TCPProcessCmdResults.RESULT_FAILED;
        }

        #endregion Luyện hóa trang bị

        #region Tách Ngũ hành hồn thạch từ trang bị chế hoặc phi phong
        /// <summary>
        /// Phản hồi yêu cầu Tách Ngũ hành hồn thạch từ trang bị chế hoặc phi phong
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
        public static TCPProcessCmdResults ResponseRefineEquipToFS(TCPManager tcpMgr, TMSKSocket socket, TCPClientPool tcpClientPool, TCPRandKey tcpRandKey, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;

            string cmdData = "";
            try
            {
                /// Giải mã gói tin đẩy về dạng ProtoBytes
                cmdData = new ASCIIEncoding().GetString(data);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Error while getting DATA, CMD={0}, Client={1}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket)));
                return TCPProcessCmdResults.RESULT_FAILED;
            }

            try
            {
                string[] fields = cmdData.Split(':');
                if (fields.Length != 1)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Error Socket params count not fit CMD={0}, Client={1}, Recv={2}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), cmdData));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// Tìm chủ nhân của gói tin tương ứng
                KPlayer client = KTPlayerManager.Find(socket);
                if (null == client)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Không tìm thấy thông tin người chơi, CMD={0}, Client={1}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket)));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// Bản đồ hiện tại
                GameMap gameMap = KTMapManager.Find(client.CurrentMapCode);

                /// Yêu cầu mở khóa an toàn
                if (client.NeedToShowInputSecondPassword())
                {
                    return TCPProcessCmdResults.RESULT_OK;
                }
                /// Nếu không thấy NPC dã luyện
                //if (client.LastEquipMasterNPC == null || client.LastEquipMasterNPC.MapCode != client.CurrentMapCode)
                //{
                //    /// Cho toác luôn
                //    return TCPProcessCmdResults.RESULT_FAILED;
                //}

                ///// Nếu khoảng cách quá xa so với NPC dã luyện
                //if (KTGlobal.GetDistanceBetweenPoints(client.CurrentPos, client.LastEquipMasterNPC.CurrentPos) > 100)
                //{
                //    KTPlayerManager.ShowNotification(client, "Khoảng cách tới Dã Luyện Đại Sư quá xa!");
                //    return TCPProcessCmdResults.RESULT_OK;
                //}


                /// DbID trang bị
                int dbID = int.Parse(fields[0]);

                /// Trang bị tương ứng
                GoodsData equipGD = client.GoodsData.Find(dbID, 0);
                /// Nếu không tồn tại
                if (equipGD == null)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Không tìm thấy thông tin trang bị, CMD={0}, Client={1}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket)));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// Thực hiện tách ngũ hành hồn thạch từ trang bị chế
                if (ItemEnhance.ExchangeFS(client, equipGD))
                {
                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "", nID);
                    return TCPProcessCmdResults.RESULT_DATA;
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