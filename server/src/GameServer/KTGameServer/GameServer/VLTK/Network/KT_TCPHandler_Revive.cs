using GameServer.KiemThe.CopySceneEvents;
using GameServer.KiemThe.Core.Item;
using GameServer.KiemThe.Entities;
using GameServer.KiemThe.GameEvents;
using GameServer.KiemThe.GameEvents.FactionBattle;
using GameServer.KiemThe.Logic.Manager.Battle;
using GameServer.Logic;
using GameServer.Server;
using GameServer.VLTK.Core.GuildManager;
using GameServer.VLTK.GameEvents.GrowTree;
using Server.Data;
using Server.Protocol;
using Server.TCP;
using Server.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using static GameServer.Logic.KTMapManager;

namespace GameServer.KiemThe.Logic
{
    /// <summary>
    /// Quản lý hồi sinh người chơi
    /// </summary>
    public static partial class KT_TCPHandler
    {
        #region Bảng hồi sinh

        /// <summary>
        /// Gửi yêu cầu lên Client hiện bảng thông báo hồi sinh
        /// </summary>
        /// <param name="client">Client</param>
        /// <param name="message">Thông tin</param>
        /// <param name="allowReviveAtPos">Kích hoạt Button Hồi sinh tại chỗ (do kỹ năng hồi sinh của Nga My)</param>
        public static void ShowClientReviveFrame(KPlayer client, string message, bool allowReviveAtPos = false)
        {
            try
            {
                G2C_ShowReviveFrame showReviveFrame = new G2C_ShowReviveFrame()
                {
                    Message = message,
                    AllowReviveAtPos = allowReviveAtPos,
                };
                byte[] cmdData = DataHelper.ObjectToBytes<G2C_ShowReviveFrame>(showReviveFrame);
                client.SendPacket((int) TCPGameServerCmds.CMD_KT_G2C_SHOWREVIVEFRAME, cmdData);
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Exception, ex.ToString());
            }
        }

        /// <summary>
        /// Xử lý yêu cầu hồi sinh của Client
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
        public static TCPProcessCmdResults ResponseClientRevive(TCPManager tcpMgr, TMSKSocket socket, TCPClientPool tcpClientPool, TCPRandKey tcpRandKey, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;
            C2G_ClientRevive cmdData;

            try
            {
                cmdData = DataHelper.BytesToObject<C2G_ClientRevive>(data, 0, data.Length);
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

                /// Nếu không đang trong trạng thái chết thì không làm gì cả
                if (!client.IsDead())
                {
                    return TCPProcessCmdResults.RESULT_OK;
                }

                int selectedID = cmdData.SelectedID;
                /// Nếu đang ở trong phụ bản
                if (client.CopyMapID != -1 && CopySceneEventManager.IsCopySceneExist(client.CopyMapID, client.MapCode))
                {
                    /// Nếu không phải lựa chọn về thành
                    if (selectedID != 1)
                    {
                        KTPlayerManager.ShowNotification(client, "Trong phụ bản chỉ được phép sử dụng chức năng Về thành!");
                        return TCPProcessCmdResults.RESULT_OK;
                    }
                    else
                    {
                        /// Phụ bản tương ứng
                        KTCopyScene copyScene = CopySceneEventManager.GetCopyScene(client.CopyMapID, client.MapCode);
                        /// Vị trí hồi sinh
                        int reliveMapCode = copyScene.ReliveMapCode;
                        int relivePosX = copyScene.RelivePosX;
                        int relivePosY = copyScene.RelivePosY;
                        /// Nếu bản đồ hồi sinh trùng với phụ bản
                        if (reliveMapCode == -1)
                        {
                            KTPlayerManager.Relive(client, client.CurrentMapCode, relivePosX, relivePosY, copyScene.ReliveHPPercent, copyScene.ReliveMPPercent, copyScene.ReliveStaminaPercent);
                        }
                        /// Nếu bản đồ hồi sinh không trùng với phụ bản
                        else
                        {
                            KTPlayerManager.Relive(client, reliveMapCode, relivePosX, relivePosY, copyScene.ReliveHPPercent, copyScene.ReliveMPPercent, copyScene.ReliveStaminaPercent);
                        }
                    }
                }
                else
                {
                    /// Bản đồ hiện tại
                    GameMap gameMap = KTMapManager.Find(client.CurrentMapCode);

                    /// Về thành
                    if (selectedID == 1)
                    {
                        // Nếu đang ở tống kim
                        if (Battel_SonJin_Manager.IsInBattle(client))
                        {
                            Battel_SonJin_Manager.Revice(client);
                        }
                        // Xử lý sự kiện hồi sinh ở đây
                        else if (FactionBattleManager.IsInFactionBattle(client))
                        {
                            FactionBattleManager.Revice(client);
                        }
                        else if (GuildWarCity.IsInGuildWarCity(client))
                        {
                            GuildWarCity.Revice(client);
                        }
                        else if (GrowTreeManager.IsInEvent(client))
                        {
                            // thực hiện hồi sinh nó nếu đang trong hạt hoàng kim
                            GrowTreeManager.Revice(client);
                        }
                        else
                        {
                            /// Nếu không có điểm hồi sinh mặc định trong sự kiện đặc biệt
                            if (!GameMapEventsManager.OnPlayerClickReliveButton(client))
                            {
                                /// Nếu là ở liên máy chủ
                                if (GameManager.IsKuaFuServer)
                                {
                                    KTPlayerManager.Relive(client, 76, 8260, 4760, KTGlobal.DefaultReliveHPPercent, KTGlobal.DefaultReliveMPPercent, KTGlobal.DefaultReliveStaminaPercent);
                                }
                                /// Nếu đang ở Tẩy Tủy Đảo
                                else if (client.CurrentMapCode == 40)
                                {
                                    KTPlayerManager.Relive(client, 40, 2460, 2030, KTGlobal.DefaultReliveHPPercent, KTGlobal.DefaultReliveMPPercent, KTGlobal.DefaultReliveStaminaPercent);
                                }
                                else
                                {    /// Dữ liệu điểm hồi sinh
                                    KT_TCPHandler.GetPlayerDefaultRelivePos(client, out int mapCode, out int posX, out int posY);
                                    /// Thực hiện hồi sinh
                                    KTPlayerManager.Relive(client, mapCode, posX, posY, KTGlobal.DefaultReliveHPPercent, KTGlobal.DefaultReliveMPPercent, KTGlobal.DefaultReliveStaminaPercent);

                                }

                            }
                        }
                    }
                    /// Hồi sinh tại chỗ
                    else if (selectedID == 2)
                    {

                        if (client.PKMode == (int)PKMode.Custom)
                        {
                            KT_TCPHandler.ShowClientNotificationTip(client, "Chế độ chiến đấu hiện tại không thể hồi sinh tại chỗ!");
                        }
                        else
                        {

                            /// Nếu bản đồ không cho phép hồi sinh tại chỗ
                            if (!gameMap.AllowReviveAtPos)
                            {
                                KT_TCPHandler.ShowClientNotificationTip(client, "Bản đồ này không cho phép hồi sinh tại chỗ...");
                            }
                            else
                            {
                                /// Nếu có Buff hồi sinh
                                if (client.m_sReviveBuff != null)
                                {
                                    client.Buffs.AddBuff(client.m_sReviveBuff);
                                }
                                else
                                {
                                    KT_TCPHandler.ShowClientNotificationTip(client, "Bạn không có trạng thái được hồi sinh tại chỗ...");
                                }
                            }
                        }
                    }
                    /// Dùng Cửu Chuyển Tục Mệnh Hoàn
                    else if (selectedID == 3)
                    {
                        if (client.PKMode == (int)PKMode.Custom )
                        {
                            KT_TCPHandler.ShowClientNotificationTip(client, "Chế độ chiến đấu hiện tại không thể hồi sinh tại chỗ!");
                        }
                        else
                        {
                            /// Nếu bản đồ không cho phép dùng Cửu Chuyển Tục Mệnh Hoàn
                            if (!gameMap.AllowUseReviveMedicine)
                            {
                                KT_TCPHandler.ShowClientNotificationTip(client, "Bản đồ này không cho phép sử dụng Cửu Chuyển Tục Mệnh Hoàn...");
                            }
                            else
                            {
                                /// ID Cửu Chuyển Tục Mệnh Hoàn trong túi
                                int nItemID = -1;

                                /// Duyệt danh sách ID vật phẩm Cửu Chuyển Tục Mệnh Hoàn tương ứng trong hệ thống
                                foreach (int itemID in KTGlobal.ReviveMedicine)
                                {
                                    int itemCount = ItemManager.GetItemCountInBag(client, itemID);
                                    if (itemCount > 0)
                                    {
                                        nItemID = itemID;
                                    }
                                }

                                /// Nếu không tìm thấy Cửu Chuyển Tục Mệnh Hoàn trong túi
                                if (nItemID == -1)
                                {
                                    KT_TCPHandler.ShowClientNotificationTip(client, "Bạn không có Cửu Chuyển Tục Mệnh Hoàn...");
                                }
                                else
                                {
                                    ItemManager.RemoveItemFromBag(client, nItemID, 1, -1, "Hồi Sinh Tại Chỗ");
                                    KTPlayerManager.Relive(client, client.CurrentMapCode, (int)client.CurrentPos.X, (int)client.CurrentPos.Y, 100, 100, 100);
                                }
                            }
                        }
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

        #endregion Bảng hồi sinh

        #region Điểm hồi sinh ở thành

        /// <summary>
        /// Thiết lập thông tin điểm hồi sinh ở thành, lưu vào vào DB
        /// </summary>
        /// <param name="player"></param>
        /// <param name="mapCode"></param>
        /// <param name="posX"></param>
        /// <param name="posY"></param>
        public static void SetDefaultRelivePos(KPlayer player, int mapCode, int posX, int posY)
        {
            player.SetValueOfForeverRecore(ForeverRecord.DefaultReliveMapID, mapCode);
            player.SetValueOfForeverRecore(ForeverRecord.DefaultRelivePosX, posX);
            player.SetValueOfForeverRecore(ForeverRecord.DefaultRelivePosY, posY);
        }

        /// <summary>
        /// Đọc thông tin điểm hồi sinh ở thành từ DB
        /// </summary>
        /// <param name="player"></param>
        /// <param name="mapCode"></param>
        /// <param name="posX"></param>
        /// <param name="posY"></param>
        public static void GetPlayerDefaultRelivePos(KPlayer player, out int mapCode, out int posX, out int posY)
        {
            /// Truy vấn dữ liệu cũ
            mapCode = player.GetValueOfForeverRecore(ForeverRecord.DefaultReliveMapID);
            posX = player.GetValueOfForeverRecore(ForeverRecord.DefaultRelivePosX);
            posY = player.GetValueOfForeverRecore(ForeverRecord.DefaultRelivePosY);

            /// Nếu chưa thiết lập
            if (mapCode == -1 || posX == -1 || posY == -1)
            {
                /// Mặc định kết quả
                NewbieVillage newbieVillage = KTGlobal.NewbieVillages.RandomRange(1).FirstOrDefault();
                mapCode = newbieVillage.ID;
                posX = newbieVillage.Position.X;
                posY = newbieVillage.Position.Y;
            }
        }

        #endregion Điểm hồi sinh ở thành

        #region Đội mồ
        /// <summary>
        /// Thông báo bản thân sống lại
        /// </summary>
        /// <param name="client"></param>
        /// <param name="sendMyself"></param>
        /// <param name="sendToOthers"></param>
        public static void NotifyMyselfRelive(KPlayer client, bool sendMyself, bool sendToOthers)
        {
            /// Danh sách người chơi xung quanh
            List<KPlayer> objsList = KTRadarMapManager.GetPlayersAround(client);
            /// Toác
            if (null == objsList)
            {
                /// Bỏ qua
                return;
            }

            /// Dữ liệu gửi tới chính mình
            MonsterRealiveData myselfData = new MonsterRealiveData()
            {
                RoleID = client.RoleID,
                PosX = client.PosX,
                PosY = client.PosY,
                Direction = (int) client.CurrentDir,
                CurrentHP = client.m_CurrentLife,
                CurrentMP = client.m_CurrentMana,
                CurrentStamina = client.m_CurrentStamina,
            };
            /// Gửi tới chính mình
            client.SendPacket<MonsterRealiveData>((int) TCPGameServerCmds.CMD_SPR_REALIVE, myselfData);

            /// Nếu gửi tới bọn xung quanh
            if (sendToOthers)
            {
                /// Dữ liệu gửi tới người chơi khác
                MonsterRealiveData othersData = new MonsterRealiveData()
                {
                    RoleID = client.RoleID,
                    PosX = client.PosX,
                    PosY = client.PosY,
                    Direction = (int) client.CurrentDir,
                    CurrentHP = client.m_CurrentLife,
                };
                /// Gửi gói tin tới người chơi khác
                KTPlayerManager.SendPacketToPlayers<MonsterRealiveData>((int) TCPGameServerCmds.CMD_SPR_REALIVE, objsList, othersData, client, client);
            }
        }
        #endregion
    }
}