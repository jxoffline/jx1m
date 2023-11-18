using GameServer.KiemThe.Core.Item;
using GameServer.KiemThe.Core.Task;
using GameServer.KiemThe.Entities;
using GameServer.KiemThe.GameEvents;
using GameServer.KiemThe.GameEvents.FactionBattle;
using GameServer.KiemThe.GameEvents.KnowledgeChallenge;
using GameServer.KiemThe.GameEvents.SpecialEvent;
using GameServer.KiemThe.Logic.Manager;
using GameServer.KiemThe.Logic.Manager.Battle;
using GameServer.KiemThe.LuaSystem;
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
using System.Text;
using static GameServer.Logic.KTMapManager;

namespace GameServer.KiemThe.Logic
{
    /// <summary>
    /// Quản lý Logic NPC, điểm thu thập và vật phẩm
    /// </summary>
    public static partial class KT_TCPHandler
    {
        #region Click NPC

        /// <summary>
        /// Thực hiện yêu cầu click vào NPC từ Client
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
        public static TCPProcessCmdResults ProcessClickOnNPC(TCPManager tcpMgr, TMSKSocket socket, TCPClientPool tcpClientPool, TCPRandKey tcpRandKey, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;
            string cmdData = null;

            try
            {
                cmdData = new UTF8Encoding().GetString(data, 0, count);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Error while getting DATA, CMD={0}, Client={1}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket)));
                return TCPProcessCmdResults.RESULT_FAILED;
            }

            try
            {
                int npcID = -1;
                if (!int.TryParse(cmdData, out npcID))
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Error Socket wrong NPCID, CMD = {0}, CmdData = {1}", (TCPGameServerCmds)nID, cmdData));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                KPlayer client = KTPlayerManager.Find(socket);
                if (null == client)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Can not find player, CMD={0}, Client={1}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket)));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                NPC npc = KTNPCManager.Find(npcID);
                if (npc == null)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Can not find NPC, CMD={0}, Client={1}, NPCID={2}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), npcID));
                    //return TCPProcessCmdResults.RESULT_FAILED;
                    return TCPProcessCmdResults.RESULT_OK;
                }

                GameMap map = KTMapManager.Find(client.MapCode);
                if (map == null)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Can not find Map, CMD={0}, Client={1}, MapID={2}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), client.MapCode));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// Vị trí hiện tại của NPC
                UnityEngine.Vector2 npcPos = new UnityEngine.Vector2((int)npc.CurrentPos.X, (int)npc.CurrentPos.Y);
                /// Vị trí của người chơi
                UnityEngine.Vector2 playerPos = new UnityEngine.Vector2((int)client.CurrentPos.X, (int)client.CurrentPos.Y);
                /// Khoảng cách
                float distance = UnityEngine.Vector2.Distance(npcPos, playerPos);
                /// Nếu quá xa
                if (distance > 400)
                {
                    KTPlayerManager.ShowNotification(client, "Khoảng cách quá xa, không thể tương tác cùng NPC!");
                    return TCPProcessCmdResults.RESULT_OK;
                }

                //Nếu là NPC nhận nhiệm vụ bạo văn đồng
                if (npc.ResID == 110)
                {
                    if (TaskDailyArmyManager.getInstance().IsHaveArmyQuest(npc.ResID, client) || TaskDailyArmyManager.getInstance().IsHaveCompleteQuest(npc.ResID, client))
                    {
                        TaskDailyArmyManager.getInstance().GetNpcDataQuest(map, npc, client);
                    }
                    else
                    {
                        TaskDailyArmyManager.getInstance().GenInfoTaskDesc(map, npc, client);
                    }
                }
                else if (npc.ResID == 771)
                {
                    if (PirateTaskManager.getInstance().IsHaveQuest(npc.ResID, client) || PirateTaskManager.getInstance().IsHaveCompleteQuest(npc.ResID, client))
                    {
                        PirateTaskManager.getInstance().GetNpcDataQuest(map, npc, client);
                    }
                    else
                    {
                        PirateTaskManager.getInstance().GetInfoHaiTac(map, npc, client);
                    }
                }
                else if (npc.ResID == 2965)
                {
                    if (FirmTaskManager.getInstance().IsHaveQuest(npc.ResID, client) || FirmTaskManager.getInstance().IsHaveCompleteQuest(npc.ResID, client))
                    {
                        FirmTaskManager.getInstance().GetNpcDataQuest(map, npc, client);
                    }
                    else
                    {
                        FirmTaskManager.getInstance().GetInfoThuongHoi(map, npc, client);
                    }
                }
                else if (npc.ScriptID == 6000)
                {
                    FactionBattleManager.NpcClick(map, npc, client);
                }
                else if (MainTaskManager.getInstance().IsHaveMainQuest(npc.ResID, client) || MainTaskManager.getInstance().IsHaveCompleteMainQuest(npc.ResID, client))
                {
                    MainTaskManager.getInstance().GetNpcDataQuest(map, npc, client);
                }
                ///// Nếu là sứ giả hoạt động Cổ Phong Hà
                else if (npc.ResID == 375)
                {
                    GuildWarCity.getInstance().NpcClickJoin(npc, client);
                }
                else if (npc.ResID == 20003 || npc.ResID== 20004)
                {
                    GuildWarCity.getInstance().NpcSupport(npc, client);
                }
                else
                {
                    /// Nếu là Npc trong tống kim
                    if (npc.ScriptID == 5000)
                    {
                        Battel_SonJin_Manager.NpcClick(map, npc, client);
                    }
                    else if (npc.ScriptID == 320)
                    {
                        GrowTreeManager.NpcClickJoin(npc, client);
                    }
                    /// Nếu là NPC sự kiện đoán hoa đăng
                    else if (!string.IsNullOrEmpty(npc.Tag) && npc.Tag == "KnowledgeChallenge")
                    {
                        /// Script tương ứng
                        KnowledgeChallenge_ActivityScript activityScript = GameMapEventsManager.GetActivityScript<KnowledgeChallenge_ActivityScript>(301);
                        /// Nếu Script tồn tại
                        if (activityScript != null)
                        {
                            /// Script điều khiển Logic hoạt động
                            KnowledgeChallenge_Script_Main script = activityScript.GetScript(client.CurrentMapCode);
                            /// Nếu tồn tại
                            if (script != null)
                            {
                                /// Thực thi sự kiện Click NPC
                                script.NPCClick(npc, client);
                                /// Bỏ qua
                                return TCPProcessCmdResults.RESULT_OK;
                            }
                        }
                    }
                    else
                    {
                        KTLuaEnvironment.ExecuteNPCScript_Open(map, npc, client, npc.ScriptID, new Dictionary<int, string>());
                    }

                    /// Thực thi sự kiện Click
                    npc.Click?.Invoke(client);
                }

                return TCPProcessCmdResults.RESULT_OK;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, Global.GetDebugHelperInfo(socket), false);
            }

            return TCPProcessCmdResults.RESULT_FAILED;
        }

        #endregion Click NPC

        #region Grow Point

        /// <summary>
        /// Phản hồi sự kiện Click vào điểm thu thập từ Client
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
        public static TCPProcessCmdResults ResponseGrowPointClick(TCPManager tcpMgr, TMSKSocket socket, TCPClientPool tcpClientPool, TCPRandKey tcpRandKey, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;

            string cmdData = "";
            try
            {
                /// Giải mã gói tin đẩy về dạng string
                cmdData = new ASCIIEncoding().GetString(data);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Error while getting DATA, CMD={0}, Client={1}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket)));
                return TCPProcessCmdResults.RESULT_FAILED;
            }

            try
            {
                /// ID điểm thu thập
                int growPointID = int.Parse(cmdData);

                /// Tìm chủ nhân của gói tin tương ứng
                KPlayer client = KTPlayerManager.Find(socket);
                if (null == client)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Không tìm thấy thông tin người chơi, CMD={0}, Client={1}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket)));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// Thực hiện Logic
                KTGrowPointManager.GrowPointClick(client, growPointID);

                return TCPProcessCmdResults.RESULT_OK;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, Global.GetDebugHelperInfo(socket), false);
            }

            return TCPProcessCmdResults.RESULT_FAILED;
        }

        /// <summary>
        /// Gửi yêu cầu dừng thao tác ProgressBar về Client
        /// </summary>
        /// <param name="player"></param>
        public static void SendStopProgressBar(KPlayer player)
        {
            G2C_ProgressBar progressBar = new G2C_ProgressBar()
            {
                Type = 2,
                Duration = 0,
                CurrentLifeTime = 0,
                Text = "",
            };
            byte[] cmdData = DataHelper.ObjectToBytes<G2C_ProgressBar>(progressBar);
            player.SendPacket((int) TCPGameServerCmds.CMD_KT_G2C_UPDATE_PROGRESSBAR, cmdData);
        }

        /// <summary>
        /// Gửi yêu cầu bắt đầu thao tác ProgressBar về Client
        /// </summary>
        /// <param name="player"></param>
        /// <param name="hint"></param>
        /// <param name="durationTick"></param>
        /// <param name="currentLifeTime"></param>
        public static void SendStartProgressBar(KPlayer player, string hint, long durationTick, long currentLifeTime)
        {
            G2C_ProgressBar progressBar = new G2C_ProgressBar()
            {
                Type = 1,
                Duration = durationTick / 1000f,
                CurrentLifeTime = currentLifeTime / 1000f,
                Text = hint,
            };
            byte[] cmdData = DataHelper.ObjectToBytes<G2C_ProgressBar>(progressBar);
            player.SendPacket((int) TCPGameServerCmds.CMD_KT_G2C_UPDATE_PROGRESSBAR, cmdData);
        }

        #endregion Grow Point

        #region NPC, Item dialog

        /// <summary>
        /// Thực thi báo cáo của Client về sự lựa chọn trên khung
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
        public static TCPProcessCmdResults ResponseNPCDialog(TCPManager tcpMgr, TMSKSocket socket, TCPClientPool tcpClientPool, TCPRandKey tcpRandKey, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;
            C2G_LuaNPCDialog result;

            try
            {
                result = DataHelper.BytesToObject<C2G_LuaNPCDialog>(data, 0, data.Length);
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

                NPC npc = KTNPCManager.Find(result.NPCID);
                if (npc == null && result.NPCID != -1)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Can not find NPC, CMD={0}, Client={1}, NPCID={2}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), result.NPCID));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                GameMap map = KTMapManager.Find(client.MapCode);
                if (map == null)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Can not find Map, CMD={0}, Client={1}, MapID={2}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), client.MapCode));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// Nếu chưa đến thời gian Click (tránh BUG click nhiều lần 1 chức năng)
                if (KTGlobal.GetCurrentTimeMilis() - client.LastClickDialog < KTGlobal.DialogClickDelay)
                {
                    return TCPProcessCmdResults.RESULT_OK;
                }
                /// Đánh dấu thời gian Click
                client.LastClickDialog = KTGlobal.GetCurrentTimeMilis();

                if (result.SelectionID != 0)
                {
                    /// Đối tượng tương ứng
                    KNPCDialog dialog = KTNPCDialogManager.FindNPCDialog(result.ID);
                    /// Thực hiện sự kiện người chơi Click vào Selection
                    if (dialog != null)
                    {
                        TaskCallBack _TaskCallBack = new TaskCallBack();
                        _TaskCallBack.SelectID = result.SelectionID;
                        _TaskCallBack.OtherParams = result.OtherParams;
                        dialog.OnSelect?.Invoke(_TaskCallBack);
                    }

                    if (npc != null)
                    {
                        KTLuaEnvironment.ExecuteNPCScript_Selection(map, npc, client, npc.ScriptID, result.SelectionID, result.OtherParams);
                    }
                }
                else if (result.SelectedItem != null)
                {
                    /// Đối tượng tương ứng
                    KNPCDialog dialog = KTNPCDialogManager.FindNPCDialog(result.ID);
                    /// Thực hiện sự kiện người chơi Click vào Selection
                    if (dialog != null)
                    {
                        TaskCallBack _TaskCallBack = new TaskCallBack();
                        _TaskCallBack.SelectID = result.SelectionID;
                        _TaskCallBack.OtherParams = result.OtherParams;
                        dialog.OnSelect?.Invoke(_TaskCallBack);

                        dialog.OnItemSelect?.Invoke(result.SelectedItem);
                    }

                    if (npc != null)
                    {
                        KTLuaEnvironment.ExecuteNPCScript_ItemSelected(map, npc, client, npc.ScriptID, result.SelectedItem, result.OtherParams);
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
        /// Gửi yêu cầu về Client mở bảng hội thoại NPC
        /// </summary>
        /// <param name="client"></param>
        /// <param name="dialogID"></param>
        /// <param name="npcID"></param>
        /// <param name="text"></param>
        /// <param name="selections"></param>
        /// <param name="itemSelectable"></param>
        /// <param name="itemHeaderString"></param>
        /// <param name="otherParams"></param>
        public static void OpenNPCDialog(KPlayer client, int npcID, int dialogID, string text, Dictionary<int, string> selections, List<DialogItemSelectionInfo> items, bool itemSelectable, string itemHeaderString, Dictionary<int, string> otherParams)
        {
            try
            {
                G2C_LuaNPCDialog dialog = new G2C_LuaNPCDialog()
                {
                    ID = dialogID,
                    NPCID = npcID,
                    Text = text,
                    Selections = selections,
                    Items = items,
                    ItemSelectable = itemSelectable,
                    OtherParams = otherParams,
                    ItemHeaderString = itemHeaderString,
                };

                byte[] cmdData = DataHelper.ObjectToBytes<G2C_LuaNPCDialog>(dialog);
                client.SendPacket((int) TCPGameServerCmds.CMD_KT_G2C_NPCDIALOG, cmdData);
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Exception, ex.ToString());
            }
        }

        /// <summary>
        /// Thực thi báo cáo của Client về sự lựa chọn trên khung
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
        public static TCPProcessCmdResults ResponseItemDialog(TCPManager tcpMgr, TMSKSocket socket, TCPClientPool tcpClientPool, TCPRandKey tcpRandKey, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;
            C2G_LuaItemDialog result;

            try
            {
                result = DataHelper.BytesToObject<C2G_LuaItemDialog>(data, 0, data.Length);
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

                GoodsData itemGD = client.GoodsData.Find(result.DbID, 0);
                if (itemGD == null)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Can not find Item, CMD={0}, Client={1}, ItemDbID={2}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), result.DbID));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }
                else if (itemGD.GoodsID != result.ItemID)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Can not find Item corresponding to DbID, CMD={0}, Client={1}, ItemDbID={2}, ItemID={3}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), result.DbID, result.ItemID));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// Vật phẩm tương ứng
                if (!ItemManager._TotalGameItem.TryGetValue(result.ItemID, out ItemData itemData))
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Can not find Item corresponding to DbID, CMD={0}, Client={1}, ItemDbID={2}, ItemID={3}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), result.DbID, result.ItemID));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// Nếu không có Script
                if (!itemData.IsScriptItem)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Item has no script, CMD={0}, Client={1}, ItemDbID={2}, ItemID={3}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), result.DbID, result.ItemID));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                GameMap map = KTMapManager.Find(client.MapCode);
                if (map == null)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Can not find Map, CMD={0}, Client={1}, MapID={2}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), client.MapCode));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// Nếu chưa đến thời gian Click (tránh BUG click nhiều lần 1 chức năng)
                if (KTGlobal.GetCurrentTimeMilis() - client.LastClickDialog < KTGlobal.DialogClickDelay)
                {
                    return TCPProcessCmdResults.RESULT_OK;
                }
                /// Đánh dấu thời gian Click
                client.LastClickDialog = KTGlobal.GetCurrentTimeMilis();

                if (result.SelectionID != 0)
                {
                    /// Đối tượng tương ứng
                    KItemDialog dialog = KTItemDialogManager.FindItemDialog(result.ID);
                    /// Thực hiện sự kiện người chơi Click vào Selection
                    if (dialog != null)
                    {
                        TaskCallBack _TaskCallBack = new TaskCallBack();
                        _TaskCallBack.SelectID = result.SelectionID;
                        _TaskCallBack.OtherParams = result.OtherParams;
                        dialog.OnSelect?.Invoke(_TaskCallBack);
                    }

                    KTLuaEnvironment.ExecuteItemScript_Selection(map, itemGD, client, itemData.ScriptID, result.SelectionID, result.OtherParams);
                }
                else if (result.SelectedItem != null)
                {
                    /// Đối tượng tương ứng
                    KItemDialog dialog = KTItemDialogManager.FindItemDialog(result.ID);
                    /// Thực hiện sự kiện người chơi Click vào vật phẩm
                    if (dialog != null)
                    {
                        dialog.OnItemSelect?.Invoke(result.SelectedItem);
                    }

                    KTLuaEnvironment.ExecuteItemScript_ItemSelected(map, itemGD, client, itemData.ScriptID, result.SelectedItem, result.OtherParams);
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
        /// Gửi yêu cầu về Client mở bảng hội thoại NPC
        /// </summary>
        /// <param name="client"></param>
        /// <param name="itemID"></param>
        /// <param name="itemDbID"></param>
        /// <param name="dialogID"></param>
        /// <param name="text"></param>
        /// <param name="selections"></param>
        /// <param name="itemSelectable"></param>
        /// <param name="otherParams"></param>
        public static void OpenItemDialog(KPlayer client, int itemID, int itemDbID, int dialogID, string text, Dictionary<int, string> selections, List<DialogItemSelectionInfo> items, bool itemSelectable, string itemHeaderString, Dictionary<int, string> otherParams)
        {
            try
            {
                G2C_LuaItemDialog dialog = new G2C_LuaItemDialog()
                {
                    ID = dialogID,
                    ItemID = itemID,
                    DbID = itemDbID,
                    Text = text,
                    Selections = selections,
                    Items = items,
                    ItemSelectable = itemSelectable,
                    ItemHeaderString = itemHeaderString,
                    OtherParams = otherParams,
                };

                byte[] cmdData = DataHelper.ObjectToBytes<G2C_LuaItemDialog>(dialog);
                client.SendPacket((int) TCPGameServerCmds.CMD_KT_G2C_ITEMDIALOG, cmdData);
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Exception, ex.ToString());
            }
        }

        /// <summary>
        /// Gửi yêu cầu về Client đóng bảng hội thoại NPCDialog hoặc ItemDialog
        /// </summary>
        /// <param name="client"></param>
        /// <param name="npcID"></param>
        /// <param name="text"></param>
        /// <param name="selections"></param>
        /// <param name="itemSelectable"></param>
        public static void CloseDialog(KPlayer client)
        {
            try
            {
                byte[] cmdData = new ASCIIEncoding().GetBytes("");
                client.SendPacket((int) TCPGameServerCmds.CMD_KT_G2C_CLOSENPCITEMDIALOG, cmdData);
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Exception, ex.ToString());
            }
        }

        #endregion NPC, Item dialog

        #region Sử dụng vật phẩm

        /// <summary>
        /// Xử lý gói tin đối tượng sử dụng vật phẩm
        /// </summary>
        /// <param name="pool"></param>
        /// <param name="nID"></param>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static TCPProcessCmdResults ProcessSpriteUseGoodsCmd(TCPManager tcpMgr, TMSKSocket socket, TCPClientPool tcpClientPool, TCPRandKey tcpRandKey, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;
            CS_SprUseGoods useGoods;

            try
            {
                useGoods = DataHelper.BytesToObject<CS_SprUseGoods>(data, 0, data.Length);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Error while getting DATA, CMD={0}, Client={1}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket)));
                return TCPProcessCmdResults.RESULT_FAILED;
            }

            try
            {
                int roleID = useGoods.RoleID;
                List<int> dbIDs = useGoods.DbIds;

                KPlayer client = KTPlayerManager.Find(socket);
                if (null == client || client.RoleID != roleID)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Can not find player corresponding ID, CMD={0}, Client={1}, RoleID={2}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), roleID));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// Nếu toác
                if (dbIDs == null || dbIDs.Count < 1)
                {
                    KTPlayerManager.ShowNotification(client, "Không tìm thấy vật phẩm!");
                    return TCPProcessCmdResults.RESULT_OK;
                }

                /// Bản đồ tương ứng
                GameMap gameMap = KTMapManager.Find(client.CurrentMapCode);
                /// Nếu bản đồ không cho phép dùng vật phẩm
                if (gameMap != null && !gameMap.AllowUseItem)
                {
                    KTPlayerManager.ShowNotification(client, "Bản đồ này không cho phép sử dụng vật phẩm!");
                    return TCPProcessCmdResults.RESULT_OK;
                }

                /// Nếu chưa đến thời gian Click (tránh BUG click nhiều lần 1 chức năng)
                if (KTGlobal.GetCurrentTimeMilis() - client.LastClickDialog < KTGlobal.ItemClickDelay)
                {
                    return TCPProcessCmdResults.RESULT_OK;
                }
                /// Đánh dấu thời gian Click
                client.LastClickDialog = KTGlobal.GetCurrentTimeMilis();

                /// ID vật phẩm 1
                int itemDbID1 = dbIDs[0];

                /// Chỉ xử lý đồng thời trong trường hợp là thuốc, và chỉ xử lý tối đa 2 loại thuốc
                if (dbIDs.Count == 2)
                {
                    /// ID vật phẩm 2
                    int itemDbID2 = dbIDs[1];

                    /// Thông tin vật phẩm tương ứng
                    GoodsData goodsData1 = client.GoodsData.Find(itemDbID1, 0);
                    GoodsData goodsData2 = client.GoodsData.Find(itemDbID2, 0);

                    if (goodsData1 != null && goodsData2 != null)
                    {
                        /// Thông tin vật phẩm
                        ItemManager._TotalGameItem.TryGetValue(goodsData1.GoodsID, out ItemData itemData1);
                        ItemManager._TotalGameItem.TryGetValue(goodsData2.GoodsID, out ItemData itemData2);

                        /// Nếu cả 2 đều là thuốc
                        if (itemData1 != null && itemData2 != null && itemData1.IsMedicine && itemData2.IsMedicine)
                        {
                            /// Thực hiện sử dụng vật phẩm 2
                            TCPProcessCmdResults ret2 = KT_TCPHandler.DoUseGoods(client, itemDbID2);
                            /// Nếu toác
                            if (ret2 == TCPProcessCmdResults.RESULT_FAILED)
                            {
                                return ret2;
                            }
                        }
                    }
                }

                /// Sử dụng vật phẩm đầu tiên
                return KT_TCPHandler.DoUseGoods(client, itemDbID1);
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, Global.GetDebugHelperInfo(socket), false);
            }

            return TCPProcessCmdResults.RESULT_FAILED;
        }

        /// <summary>
        /// Thực hiện sử dụng vật phẩm
        /// </summary>
        /// <param name="client"></param>
        /// <param name="dbID"></param>
        /// <returns></returns>
        private static TCPProcessCmdResults DoUseGoods(KPlayer client, int dbID)
        {
            try
            {
                /// Bản đồ tương ứng
                GameMap gameMap = KTMapManager.Find(client.CurrentMapCode);

                /// Thông tin vật phẩm tương ứng
                GoodsData goodsData = client.GoodsData.Find(dbID, 0);
                if (goodsData == null)
                {
                    KTPlayerManager.ShowNotification(client, "Vật phẩm không tồn tại!");
                    return TCPProcessCmdResults.RESULT_OK;
                }

                /// Nếu không có ở trong túi đồ
                if (goodsData.Site != 0)
                {
                    KTPlayerManager.ShowNotification(client, "Vật phẩm không tồn tại trong túi!");
                    return TCPProcessCmdResults.RESULT_OK;
                }

                /// Nếu số lượng vật phẩm > 0
                if (goodsData.GCount > 0)
                {
                    /// Nếu bản đồ không cho phép dùng kỹ năng
                    if (gameMap != null && gameMap.BanItems.Contains(goodsData.GoodsID))
                    {
                        KTPlayerManager.ShowNotification(client, "Bản đồ này không cho phép sử dụng vật phẩm này!");
                        return TCPProcessCmdResults.RESULT_OK;
                    }

                    ItemManager.UseItem(client, goodsData);
                }
                else
                {
                    KTPlayerManager.ShowNotification(client, "Vật phẩm không tồn tại!");
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                return TCPProcessCmdResults.RESULT_OK;
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Exception, ex.ToString());
            }

            return TCPProcessCmdResults.RESULT_FAILED;
        }

        #endregion Sử dụng vật phẩm

        #region Tương tác với vật phẩm
        /// <summary>
        /// Packet thao tác với đồ đạc
        /// </summary>
        /// <param name="pool"></param>
        /// <param name="nID"></param>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static TCPProcessCmdResults ProcessSpriteModGoodsCmd(TCPManager tcpMgr, TMSKSocket socket, TCPClientPool tcpClientPool, TCPRandKey tcpRandKey, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
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
                //Danh sách các thành phần
                string[] fields = cmdData.Split(':');
                if (fields.Length != 9)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Error Socket params count not fit CMD={0}, Client={1}, Recv={2}",
                        (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), fields.Length));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                int roleID = Convert.ToInt32(fields[0]);

                KPlayer client = KTPlayerManager.Find(socket);
                if (null == client || client.RoleID != roleID)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Can not find player corresponding ID, CMD={0}, Client={1}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket)));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// Nếu chưa đến thời gian được thao tác
                if (KTGlobal.GetCurrentTimeMilis() - client.LastPickUpDropItemTicks < 1000)
                {
                    KTPlayerManager.ShowNotification(client, "Thao tác quá nhanh!");
                    /// Không cho nhặt
                    return TCPProcessCmdResults.RESULT_OK;
                }
                /// Thiết lập thời gian nhặt lần trước
                client.LastPickUpDropItemTicks = KTGlobal.GetCurrentTimeMilis();

                return ItemManager.ModifyGoodsByCmdParams(client, cmdData);
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, Global.GetDebugHelperInfo(socket), false);
            }

            return TCPProcessCmdResults.RESULT_FAILED;
        }
        #endregion

        #region Tách và gộp vật phẩm
        /// <summary>
        /// Tách và merge vật phẩm
        /// </summary>
        /// <param name="pool"></param>
        /// <param name="nID"></param>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static TCPProcessCmdResults ProcessSpriteMergeGoodsCmd(TCPManager tcpMgr, TMSKSocket socket, TCPClientPool tcpClientPool, TCPRandKey tcpRandKey, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;
            string cmdData = null;

            try
            {
                cmdData = new UTF8Encoding().GetString(data, 0, count);
            }
            catch (Exception) //解析错误
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Decode data faild, CMD={0}, Client={1}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket)));
                return TCPProcessCmdResults.RESULT_FAILED;
            }

            try
            {

                string[] fields = cmdData.Split(':');
                if (fields.Length < 2)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Error Socket params count not fit CMD={0}, Client={1}, Recv={2}",
                        (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), fields.Length));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                int roleID = Convert.ToInt32(fields[0]);
                int id = Convert.ToInt32(fields[1]);
                int site = Convert.ToInt32(fields[2]);
                int goodsID = Convert.ToInt32(fields[3]);
                int otherId = Convert.ToInt32(fields[4]);
                int otherGoodsID = Convert.ToInt32(fields[5]);

                KPlayer client = KTPlayerManager.Find(socket);
                if (null == client || client.RoleID != roleID)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Can not find player corresponding ID, CMD={0}, Client={1}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket)));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

              
                GoodsData goodsData1 = null;
                if (site == 0)
                {
                    goodsData1 = client.GoodsData.Find(id, 0);
                }
            

                if (null == goodsData1)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Source good does not exist, CMD={0}, Client={1}, RoleID={2}, GoodsDbID={3}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), roleID, id));
                    return TCPProcessCmdResults.RESULT_OK;
                }

                if (goodsData1.GoodsID != goodsID)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Source good is not fit, CMD={0}, Client={1}, RoleID={2}, GoodsDbID={3}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), roleID, id));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

              
                GoodsData goodsData2 = null;
                if (site == 0)
                {
                    goodsData2 = client.GoodsData.Find(otherId, 0);
                }
              

                if (null == goodsData2)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Target good does not exist, CMD={0}, Client={1}, RoleID={2}, GoodsDbID={3}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), roleID, otherId));
                    return TCPProcessCmdResults.RESULT_OK;
                }

                if (goodsData2.GoodsID != otherGoodsID)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Target good is not fit, CMD={0}, Client={1}, RoleID={2}, GoodsDbID={3}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), roleID, otherId));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                
                if (goodsData1.GoodsID != goodsData2.GoodsID)
                {
                    return TCPProcessCmdResults.RESULT_OK;
                }

                //判断两个物品项是否绑定状态相同
                if (goodsData1.Binding != goodsData2.Binding)
                {
                    return TCPProcessCmdResults.RESULT_OK;
                }

                //判断两个物品项是否限时时间相同
                if (!ItemManager.DateTimeEqual(goodsData1.Endtime, goodsData2.Endtime))
                {
                    return TCPProcessCmdResults.RESULT_OK;
                }

                int gridNum = ItemManager.GetGoodsGridNumByID(goodsID);

                //不做任何处理
                if (gridNum <= 1)
                {
                    return TCPProcessCmdResults.RESULT_OK;
                }

                //不做任何处理
                if (goodsData1.GCount >= gridNum)
                {
                    return TCPProcessCmdResults.RESULT_OK;
                }

                int usingNum = ItemManager.GetGoodsUsingNum(goodsID);

                //不做任何处理
                if (usingNum > 1)
                {
                    return TCPProcessCmdResults.RESULT_OK;
                }

                /// Nếu chưa đến thời gian được thao tác
                if (KTGlobal.GetCurrentTimeMilis() - client.LastPickUpDropItemTicks < 1000)
                {
                    KTPlayerManager.ShowNotification(client, "Thao tác quá nhanh!");
                    /// Không cho nhặt
                    return TCPProcessCmdResults.RESULT_OK;
                }

                int moveNum = Math.Min(gridNum - goodsData1.GCount, goodsData2.GCount);

                string strcmd = "";

                //向DBServer请求修改物品2
                string[] dbFields = null;
                strcmd = ItemManager.FormatUpdateDBGoodsStr(roleID, otherId, "*", "*", "*", "*", "*", "*", "*", goodsData2.GCount - moveNum, "*", "*", "*", "*", "*", "*", "*", "*", "*", "*", "*", "*", "*"); // 卓越属性 [12/13/2013 LiaoWei] 装备转生
                TCPProcessCmdResults dbRequestResult = Global.RequestToDBServer(tcpClientPool, pool, (int)TCPGameServerCmds.CMD_DB_UPDATEGOODS_CMD, strcmd, out dbFields, client.ServerId);
                if (dbRequestResult == TCPProcessCmdResults.RESULT_FAILED)
                {
                    strcmd = string.Format("{0}:{1}:{2}:{3}:{4}:{5}:{6}:{7}:{8}", -1, roleID, id, site, goodsID, goodsData1.GCount, otherId, otherGoodsID, goodsData2.GCount);
                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, strcmd, nID);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                if (dbFields.Length <= 0 || Convert.ToInt32(dbFields[1]) < 0)
                {
                    strcmd = string.Format("{0}:{1}:{2}:{3}:{4}:{5}:{6}:{7}:{8}", -2, roleID, id, site, goodsID, goodsData1.GCount, otherId, otherGoodsID, goodsData2.GCount);
                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, strcmd, nID);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                goodsData2.GCount -= moveNum;

                EventLogManager.AddGoodsEvent(client, OpTypes.AddOrSub, OpTags.None, goodsData2.GoodsID, goodsData2.Id, -moveNum, goodsData2.GCount, "合并减少");

                //修改内存中物品记录
                if (goodsData2.GCount <= 0)
                {
                    if (site == 0)
                    {
                        client.GoodsData.Remove(goodsData2);
                    }
                   
                }

                //向DBServer请求修改物品
                dbFields = null;
                strcmd = ItemManager.FormatUpdateDBGoodsStr(roleID, id, "*", "*", "*", "*", "*", "*", "*", goodsData1.GCount + moveNum, "*", "*", "*", "*", "*", "*", "*", "*", "*", "*", "*", "*", "*"); // 卓越一击 [12/13/2013 LiaoWei] 装备转生
                dbRequestResult = Global.RequestToDBServer(tcpClientPool, pool, (int)TCPGameServerCmds.CMD_DB_UPDATEGOODS_CMD, strcmd, out dbFields, client.ServerId);
                if (dbRequestResult == TCPProcessCmdResults.RESULT_FAILED)
                {
                    strcmd = string.Format("{0}:{1}:{2}:{3}:{4}:{5}:{6}:{7}:{8}", -3, roleID, id, site, goodsID, goodsData1.GCount, otherId, otherGoodsID, goodsData2.GCount);
                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, strcmd, nID);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                if (dbFields.Length <= 0 || Convert.ToInt32(dbFields[1]) < 0)
                {
                    strcmd = string.Format("{0}:{1}:{2}:{3}:{4}:{5}:{6}:{7}:{8}", -4, roleID, id, site, goodsID, goodsData1.GCount, otherId, otherGoodsID, goodsData2.GCount);
                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, strcmd, nID);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                goodsData1.GCount += moveNum;

                EventLogManager.AddGoodsEvent(client, OpTypes.AddOrSub, OpTags.None, goodsData1.GoodsID, goodsData1.Id, moveNum, goodsData1.GCount, "合并增加");

                strcmd = string.Format("{0}:{1}:{2}:{3}:{4}:{5}:{6}:{7}:{8}", 0, roleID, id, site, goodsID, goodsData1.GCount, otherId, otherGoodsID, goodsData2.GCount);
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

        #region Nhặt vật phẩm
        /// <summary>
        /// Nhặt vật phẩm rơi dưới đất
        /// </summary>
        /// <param name="pool"></param>
        /// <param name="nID"></param>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static TCPProcessCmdResults ProcessSpriteGetThingCmd(TCPManager tcpMgr, TMSKSocket socket, TCPClientPool tcpClientPool, TCPRandKey tcpRandKey, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
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

                /// ID vật phẩm rơi
                int id = Convert.ToInt32(fields[0]);

                /// Người chơi tương ứng
                KPlayer client = KTPlayerManager.Find(socket);
                if (null == client)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Can not find player corresponding ID, CMD={0}, Client={1}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket)));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// Nếu chưa đến thời gian được nhặt
                if (KTGlobal.GetCurrentTimeMilis() - client.LastPickUpDropItemTicks < 700)
                {
                    /// Không cho nhặt
                    return TCPProcessCmdResults.RESULT_OK;
                }
                /// Thiết lập thời gian nhặt lần trước
                client.LastPickUpDropItemTicks = KTGlobal.GetCurrentTimeMilis();

                /// Vật phẩm rơi tương ứng
                KGoodsPack goodsPack = KTGoodsPackManager.FindGoodsPack(id);
                /// Nếu không tìm thấy
                if (goodsPack == null)
                {
                    /// Thông báo
                    KTPlayerManager.ShowNotification(client, "Vật phẩm không tồn tại!");
                    return TCPProcessCmdResults.RESULT_OK;
                }

                /// Thực hiện nhặt vật phẩm
                goodsPack.PickUp(client);

                return TCPProcessCmdResults.RESULT_OK;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, Global.GetDebugHelperInfo(socket), false);
            }

            return TCPProcessCmdResults.RESULT_FAILED;
        }
        #endregion

        #region Vật phẩm trong túi đồ
        /// <summary>
        /// Packet thực hiện Sắp xếp lại túi đồ
        /// </summary>
        /// <param name="pool"></param>
        /// <param name="nID"></param>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static TCPProcessCmdResults ProcessSpriteResetBagCmd(TCPManager tcpMgr, TMSKSocket socket, TCPClientPool tcpClientPool, TCPRandKey tcpRandKey, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
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
                    LogManager.WriteLog(LogTypes.Error, string.Format("Error Socket params count not fit CMD={0}, Client={1}, Recv={2}",
                        (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), fields.Length));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                int roleID = Convert.ToInt32(fields[0]);

                KPlayer client = KTPlayerManager.Find(socket);
                if (null == client || client.RoleID != roleID)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Can not find player corresponding ID, CMD={0}, Client={1}, RoleID={2}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), roleID));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// Nếu chưa đến thời gian Click (tránh BUG click nhiều lần 1 chức năng)
                if (KTGlobal.GetCurrentTimeMilis() - client.LastClickDialog < KTGlobal.ItemClickDelay)
                {
                    KTPlayerManager.ShowNotification(client, "Thao tác quá nhanh, xin hãy chờ giây lát!");
                    return TCPProcessCmdResults.RESULT_OK;
                }
                /// Đánh dấu thời gian Click
                client.LastClickDialog = KTGlobal.GetCurrentTimeMilis();

                /// Thực hiện sắp xếp lại túi đồ
                client.GoodsData.SortBag(0);

                return TCPProcessCmdResults.RESULT_OK;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, Global.GetDebugHelperInfo(socket), false);
            }

            return TCPProcessCmdResults.RESULT_FAILED;
        }

        /// <summary>
        /// Gọi hàm reset lại thủ khố
        /// </summary>
        /// <param name="pool"></param>
        /// <param name="nID"></param>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static TCPProcessCmdResults ProcessSpriteResetPortableBagCmd(TCPManager tcpMgr, TMSKSocket socket, TCPClientPool tcpClientPool, TCPRandKey tcpRandKey, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
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
                    LogManager.WriteLog(LogTypes.Error, string.Format("Error Socket params count not fit CMD={0}, Client={1}, Recv={2}",
                        (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), fields.Length));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                int roleID = Convert.ToInt32(fields[0]);

                KPlayer client = KTPlayerManager.Find(socket);
                if (null == client || client.RoleID != roleID)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Can not find player corresponding ID, CMD={0}, Client={1}, RoleID={2}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), roleID));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// Nếu chưa đến thời gian Click (tránh BUG click nhiều lần 1 chức năng)
                if (KTGlobal.GetCurrentTimeMilis() - client.LastClickDialog < KTGlobal.ItemClickDelay)
                {
                    KTPlayerManager.ShowNotification(client, "Thao tác quá nhanh, xin hãy chờ giây lát!");
                    return TCPProcessCmdResults.RESULT_OK;
                }
                /// Đánh dấu thời gian Click
                client.LastClickDialog = KTGlobal.GetCurrentTimeMilis();

                /// Gọi hàm sắp xếp lại kho
                client.GoodsData.SortBag(1);

                return TCPProcessCmdResults.RESULT_OK;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, Global.GetDebugHelperInfo(socket), false);
            }

            return TCPProcessCmdResults.RESULT_FAILED;
        }

        /// <summary>
        /// Đọc vật phẩm từ túi đồ ra
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
        public static TCPProcessCmdResults ProcessSpriteGetGoodsListBySiteCmd(TCPManager tcpMgr, TMSKSocket socket, TCPClientPool tcpClientPool, TCPRandKey tcpRandKey, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
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
                if (fields.Length != 2)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Error Socket params count not fit CMD={0}, Client={1}, Recv={2}",
                        (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), fields.Length));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                int roleID = Convert.ToInt32(fields[0]);
                int site = Convert.ToInt32(fields[1]);

                KPlayer client = KTPlayerManager.Find(socket);
                if (null == client || client.RoleID != roleID)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Can not find player corresponding ID, CMD={0}, Client={1}, RoleID={2}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), roleID));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                TCPProcessCmdResults result = Global.TransferRequestToDBServer(tcpMgr, socket, tcpClientPool, tcpRandKey, pool, nID, data, count, out tcpOutPacket, client.ServerId);
                if (TCPProcessCmdResults.RESULT_FAILED != result)
                {
                    //Get đồ từ DB ra trả về client
                    List<GoodsData> goodsDataList = DataHelper.BytesToObject<List<GoodsData>>(tcpOutPacket.GetPacketBytes(), 6, tcpOutPacket.PacketDataSize - 6);

                  
                }

                return result;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, Global.GetDebugHelperInfo(socket), false);
            }

            return TCPProcessCmdResults.RESULT_FAILED;
        }
        #endregion

        #region Vật phẩm dùng nhanh
        /// <summary>
        /// Phản hồi yêu cầu thiết lập vật phẩm vào khay dùng nhanh
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
        public static TCPProcessCmdResults ResponseSetQuickItems(TCPManager tcpMgr, TMSKSocket socket, TCPClientPool tcpClientPool, TCPRandKey tcpRandKey, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
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
                if (fields.Length != 5)
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

                /// Dãy lưu trữ tương ứng
                string quickKey = string.Join("|", fields);

                /// Thiết lập dữ liệu vào RoleData
                client.QuickItems = quickKey;

                ///// Lưu vào DB
                //KT_TCPHandler.SendSaveQuickItemsToDB(client);

                return TCPProcessCmdResults.RESULT_OK;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, Global.GetDebugHelperInfo(socket), false);
            }

            return TCPProcessCmdResults.RESULT_FAILED;
        }

        /// <summary>
        /// Lưu dữ liệu thiết lập vật phẩm vào khay dùng nhanh vào DB
        /// </summary>
        public static void SendSaveQuickItemsToDB(KPlayer player)
        {
            string cmdData = player.QuickItems;
         
            Global.ExecuteDBCmd((int)TCPGameServerCmds.CMD_DB_UPDATEKEYS, player.RoleID + ":" + 2 + ":" + cmdData, player.ServerId);
        }
        #endregion

        #region Thông báo vật phẩm
        /// <summary>
        /// Thông báo add vật phẩm về CLIENT
        /// </summary>
        /// <param name="client"></param>
        public static void NotifySelfAddGoods(KPlayer client, int id, int goodsID, int forgeLevel, int goodsNum, int binding, int site, int newHint, string newEndTime, int strong, int bagIndex = 0, int isusing = -1, String Props = "", int Series = -1, Dictionary<ItemPramenter, string> OtherParams = null)
        {
            newEndTime = newEndTime.Replace(":", "$");

            AddGoodsData addGoodsData = new AddGoodsData()
            {
                RoleID = client.RoleID,
                ID = id,
                GoodsID = goodsID,
                ForgeLevel = forgeLevel,
                GoodsNum = goodsNum,
                Binding = binding,
                Site = site,
                NewHint = newHint,
                NewEndTime = newEndTime,
                Strong = strong,
                BagIndex = bagIndex,
                Using = isusing,
                OtherParams = OtherParams,
                Props = Props,
                Series = Series,
            };
            client.SendPacket<AddGoodsData>((int) TCPGameServerCmds.CMD_SPR_ADD_GOODS, addGoodsData);
        }

        /// <summary>
        /// Move GoodData
        /// </summary>
        /// <param name="client"></param>
        public static void NotifyMoveGoods(KPlayer client, GoodsData gd, int moveType)
        {
            if (0 == moveType)
            {
                string strcmd = string.Format("{0}:{1}", client.RoleID, gd.Id);
                client.SendPacket((int) TCPGameServerCmds.CMD_SPR_MOVEGOODSDATA, strcmd);
            }
            else
            {
                KT_TCPHandler.NotifySelfAddGoods(client, gd.Id, gd.GoodsID, gd.Forge_level, gd.GCount, gd.Binding, gd.Site, 0, gd.Endtime, gd.Strong, gd.BagIndex, gd.Using, gd.Props, gd.Series, gd.OtherParams);
            }
        }
        #endregion
    }
}