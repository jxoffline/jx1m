using GameServer.KiemThe.Core.Item;
using GameServer.KiemThe.Entities;
using GameServer.KiemThe.Logic.Manager;
using GameServer.Logic;
using GameServer.Server;
using Server.Data;
using Server.Protocol;
using Server.TCP;
using Server.Tools;
using System;
using System.Linq;
using System.Text;
using static GameServer.Logic.KTMapManager;

namespace GameServer.KiemThe.Logic
{
    /// <summary>
    /// Quản lý thiết lập hệ thống và Auto
    /// </summary>
    public static partial class KT_TCPHandler
    {
        #region Thiết lập hệ thống và Auto

        /// <summary>
        /// Phản hồi yêu cầu lưu thiết lập hệ thống
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
        public static TCPProcessCmdResults ResponseSaveSystemSettings(TCPManager tcpMgr, TMSKSocket socket, TCPClientPool tcpClientPool, TCPRandKey tcpRandKey, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;

            string cmdData = "";
            try
            {
                cmdData = new ASCIIEncoding().GetString(data);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Error while getting DATA, CMD={0}, Client={1}", (TCPGameServerCmds) nID, Global.GetSocketRemoteEndPoint(socket)));
                return TCPProcessCmdResults.RESULT_FAILED;
            }

            try
            {
                string[] fields = cmdData.Split('|');
                if (fields.Length != 16)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Error Socket params count not fit CMD={0}, Client={1}, Recv={2}",
                        (TCPGameServerCmds) nID, Global.GetSocketRemoteEndPoint(socket), fields.Length));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// Kiểm tra chuỗi có hợp lệ không
                if (cmdData.Any(x => x != '|' && x != '-' && x != ';' && (x < '0' || x > '9')) || cmdData.Contains("--"))
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Socket params are invalid, CMD={0}, Client={1}", (TCPGameServerCmds) nID, Global.GetSocketRemoteEndPoint(socket)));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// Người chơi tương ứng
                KPlayer client = KTPlayerManager.Find(socket);
                if (null == client)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Can not find player, CMD={0}, Client={1}", (TCPGameServerCmds) nID, Global.GetSocketRemoteEndPoint(socket)));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// Lưu vào DB
                Global.SaveRoleParamsStringWithNullToDB(client, RoleParamName.SystemSettings, cmdData, true);

                return TCPProcessCmdResults.RESULT_OK;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, Global.GetDebugHelperInfo(socket), false);
            }

            return TCPProcessCmdResults.RESULT_FAILED;
        }

        /// <summary>
        /// Phản hồi yêu cầu lưu thiết lập Auto
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
        public static TCPProcessCmdResults ResponseSaveAutoSettings(TCPManager tcpMgr, TMSKSocket socket, TCPClientPool tcpClientPool, TCPRandKey tcpRandKey, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;

            string cmdData = "";
            try
            {
                cmdData = new ASCIIEncoding().GetString(data);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Error while getting DATA, CMD={0}, Client={1}", (TCPGameServerCmds) nID, Global.GetSocketRemoteEndPoint(socket)));
                return TCPProcessCmdResults.RESULT_FAILED;
            }

            try
            {
                /// Người chơi tương ứng
                KPlayer client = KTPlayerManager.Find(socket);
                if (null == client)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Can not find player, CMD={0}, Client={1}", (TCPGameServerCmds) nID, Global.GetSocketRemoteEndPoint(socket)));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// Lưu vào DB
                Global.SaveRoleParamsStringWithNullToDB(client, RoleParamName.AutoSettings, cmdData, true);

                return TCPProcessCmdResults.RESULT_OK;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, Global.GetDebugHelperInfo(socket), false);
            }

            return TCPProcessCmdResults.RESULT_FAILED;
        }

        #endregion Thiết lập hệ thống và Auto

        #region Auto Path

        /// <summary>
        /// Phản hồi yêu cầu chuyển bản đồ tại vị trí tương ứng sử dụng vật phẩm tương ứng từ Client
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
        public static TCPProcessCmdResults ResponseClientAutoPathTransferMap(TCPManager tcpMgr, TMSKSocket socket, TCPClientPool tcpClientPool, TCPRandKey tcpRandKey, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;

            C2G_AutoPathChangeMap autoPathChangeMap = null;
            try
            {
                /// Giải mã gói tin
                autoPathChangeMap = DataHelper.BytesToObject<C2G_AutoPathChangeMap>(data, 0, count);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Error while getting DATA, CMD={0}, Client={1}", (TCPGameServerCmds) nID, Global.GetSocketRemoteEndPoint(socket)));
                return TCPProcessCmdResults.RESULT_FAILED;
            }

            try
            {
                /// Nếu Data NULL
                if (autoPathChangeMap == null)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Error while getting DATA, CMD={0}, Client={1}", (TCPGameServerCmds) nID, Global.GetSocketRemoteEndPoint(socket)));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// Tìm chủ nhân của gói tin tương ứng
                KPlayer player = KTPlayerManager.Find(socket);

                if (null == player)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Không tìm thấy thông tin người chơi, CMD={0}, Client={1}", (TCPGameServerCmds) nID, Global.GetSocketRemoteEndPoint(socket)));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// Thông tin trong gói tin
                int toMapCode = autoPathChangeMap.ToMapCode;
                int itemID = autoPathChangeMap.ItemID;
                bool useNPC = autoPathChangeMap.UseNPC;

                /// Nếu là dịch từ NPC
                if (useNPC)
                {
                    /// Nếu tìm thấy NPC dịch chuyển gần vị trí tương ứng đang đứng
                    if (KTAutoPathManager.Instance.CanTransferFromNPCToMap(player, toMapCode, out AutoPathXML.Node node))
                    {
                        /// Nếu ID bản đồ đích khác bản đồ hiện tại
                        if (player.CurrentMapCode != toMapCode)
                        {
                            /// Syns tọa độ
                            KTPlayerManager.ChangeMap(player, node.ToMapCode, node.ToX, node.ToY);
                        }
                        /// Nếu ID bản đồ đích trùng với bản đồ hiện tại
                        else
                        {
                            KTPlayerManager.ChangePos(player, node.ToX, node.ToY);
                        }
                    }
                    /// Nếu không thấy NPC dịch chuyển thì toang
                    else
                    {
                        KTPlayerManager.ShowNotification(player, "Không tìm thấy NPC dịch chuyển xung quanh!");
                    }
                }
                /// Nếu là dịch từ truyền tống phù
                else
                {
                    GameMap gameMap = KTMapManager.Find(player.CurrentMapCode);
                    /// Nếu bản đồ không cho phép dùng vật phẩm
                    if (gameMap != null && !gameMap.AllowUseItem)
                    {
                        KTPlayerManager.ShowNotification(player, "Bản đồ này không cho phép sử dụng vật phẩm!");
                        return TCPProcessCmdResults.RESULT_OK;
                    }
                    else if (gameMap != null && gameMap.BanItems.Contains(itemID))
					{
                        if (player.ClientSocket.IsKuaFuLogin)
                        {
                            KTPlayerManager.ShowNotification(player, "Tại liên máy chủ AUTO không thể sử dụng Truyền Tống Phù.Vui lòng sử dụng xa phu để di chuyển");
                            return TCPProcessCmdResults.RESULT_OK;
                        }
                        else
                        {
                            KTPlayerManager.ShowNotification(player, "Bản đồ này không cho phép sử dụng Truyền Tống Phù!");
                            return TCPProcessCmdResults.RESULT_OK;
                        }
                       
                    }

                    /// Vật phẩm tương ứng ưu tiên dùng vật phẩm khóa
                    GoodsData itemGD = player.GoodsData.FindAll(x => x.Site == 0 && x.GoodsID == itemID).OrderByDescending(x => x.Binding).FirstOrDefault();
                    /// Nếu vật phẩm không tồn tại
                    if (itemGD == null)
                    {
                        KTPlayerManager.ShowNotification(player, "Không có truyền tống phù!");
                        return TCPProcessCmdResults.RESULT_OK;
                    }

                    /// Nếu tìm thấy điểm dịch chuyển bằng truyền tống phù vị trí tương ứng đang đứng
                    if (KTAutoPathManager.Instance.CanTransferByUsingTeleportItem(player, itemID, toMapCode, out AutoPathXML.Node node))
                    {
                        /// Nếu ID bản đồ đích trùng với bản đồ hiện tại
                        if (player.CurrentMapCode != toMapCode)
                        {
                            /// Syns tọa độ
                            KTPlayerManager.ChangeMap(player, node.ToMapCode, node.ToX, node.ToY);
                        }
                        /// Nếu ID bản đồ đích khác bản đồ hiện tại
                        else
                        {
                            KTPlayerManager.ChangePos(player, node.ToX, node.ToY);
                        }

                        /// Xóa vật phẩm sau khi sử dụng nếu có
                        ItemManager.DeductItemOnUse(player, itemGD,"AUTOPATH");
                    }
                    /// Nếu không thấy NPC dịch chuyển thì toang
                    else
                    {
                        KTPlayerManager.ShowNotification(player, "Không tìm thấy vị trí dịch chuyển xung quanh!");
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

        #endregion Auto Path
    }
}
