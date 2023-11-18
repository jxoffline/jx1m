using GameServer.KiemThe.Core.Item;
using GameServer.KiemThe.Core.Task;
using GameServer.Logic;
using GameServer.Server;
using Server.Data;
using Server.Protocol;
using Server.TCP;
using Server.Tools;
using System;

namespace GameServer.KiemThe.Logic
{
    /// <summary>
    /// Quản lý gói tin
    /// </summary>
    public static partial class KT_TCPHandler
    {
        /// <summary>
        /// Phản hồi gói tin nhập danh sách vật phẩm
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
        public static TCPProcessCmdResults ResponseInputItems(TCPManager tcpMgr, TMSKSocket socket, TCPClientPool tcpClientPool, TCPRandKey tcpRandKey, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;

            C2G_InputItems inputItems = null;
            try
            {
                /// Giải mã gói tin đẩy về dạng ProtoBytes
                inputItems = DataHelper.BytesToObject<C2G_InputItems>(data, 0, count);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Error while getting DATA, CMD={0}, Client={1}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket)));
                return TCPProcessCmdResults.RESULT_FAILED;
            }

            try
            {
                /// Nếu dữ liệu trong gói tin không tồn tại
                if (inputItems == null)
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

                /// Nếu là tiêu hủy vật phẩm
                if (inputItems.Tag == "RemoveItems")
                {
                    KTPlayerManager.ResolveRemoveItems(client, inputItems.Items);
                }
                /// Nếu là ghép vật phẩm
                else if (inputItems.Tag == "MergeItems")
                {
                    ItemManager.MergeItems(inputItems.Items, client);
                }
                /// Nếu là đổi ngũ hành ấn, quan ấn và phi phong về đúng hệ
                else if (inputItems.Tag == "ChangeSignetMantleAndChopstick")
                {
                    KTPlayerManager.ResolveChangeSignetMantleAndChopstick(client, inputItems.Items);
                }
                // Nếu đây là trả nhiệm vụ
                else if (inputItems.Tag.Contains("QuestItem"))
                {
                    TaskDailyArmyManager.getInstance().ProsecCallBackItem(client, inputItems.Items, inputItems.Tag);
                }
                else if (inputItems.Tag.Contains("BreakingItemRequest"))
                {
                    ItemCraftingManager.ProsecCallBackBreakingItem(client, inputItems.Items, inputItems.Tag);
                }
                else if (inputItems.Tag.Contains("UnlockItemRequest"))
                {
                    ItemCraftingManager.ProseccUnlockItem(client, inputItems.Items, inputItems.Tag);
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
        /// Gửi yêu cầu mở khung nhập danh sách vật phẩm
        /// </summary>
        /// <param name="player"></param>
        /// <param name="title"></param>
        /// <param name="description"></param>
        /// <param name="otherDetail"></param>
        /// <param name="tag"></param>
        public static void SendOpenInputItems(KPlayer player, string title, string description, string otherDetail, string tag)
        {
            try
            {
                G2C_InputItems inputItems = new G2C_InputItems()
                {
                    Title = title,
                    Description = description,
                    OtherDetail = otherDetail,
                    Tag = tag,
                };
                byte[] bytesCmd = DataHelper.ObjectToBytes<G2C_InputItems>(inputItems);
                player.SendPacket((int) TCPGameServerCmds.CMD_KT_SHOW_INPUTITEMS, bytesCmd);
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, "Exception", false);
            }
        }


        /// <summary>
        /// Phản hồi gói tin nhập trang bị và danh sách vật phẩm nguyên liệu
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
        public static TCPProcessCmdResults ResponseInputEquipAndMaterials(TCPManager tcpMgr, TMSKSocket socket, TCPClientPool tcpClientPool, TCPRandKey tcpRandKey, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;

            C2G_InputEquipAndMaterials inputItems = null;
            try
            {
                /// Giải mã gói tin đẩy về dạng ProtoBytes
                inputItems = DataHelper.BytesToObject<C2G_InputEquipAndMaterials>(data, 0, count);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Error while getting DATA, CMD={0}, Client={1}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket)));
                return TCPProcessCmdResults.RESULT_FAILED;
            }

            try
            {
                /// Nếu dữ liệu trong gói tin không tồn tại
                if (inputItems == null)
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

                /// TODO

                return TCPProcessCmdResults.RESULT_OK;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, Global.GetDebugHelperInfo(socket), false);
            }

            return TCPProcessCmdResults.RESULT_FAILED;
        }

        /// <summary>
        /// Gửi yêu cầu mở khung nhập trang bị và danh sách vật phẩm nguyên liệu
        /// </summary>
        /// <param name="player"></param>
        /// <param name="title"></param>
        /// <param name="description"></param>
        /// <param name="otherDetail"></param>
        /// <param name="mustIncludeMaterials"></param>
        /// <param name="tag"></param>
        public static void SendOpenInputEquipAndMaterials(KPlayer player, string title, string description, string otherDetail, bool mustIncludeMaterials, string tag)
        {
            try
            {
                G2C_InputEquipAndMaterials inputItems = new G2C_InputEquipAndMaterials()
                {
                    Title = title,
                    Description = description,
                    OtherDetail = otherDetail,
                    MustIncludeMaterials = mustIncludeMaterials,
                    Tag = tag,
                };
                byte[] bytesCmd = DataHelper.ObjectToBytes<G2C_InputEquipAndMaterials>(inputItems);
                player.SendPacket((int) TCPGameServerCmds.CMD_KT_SHOW_INPUTEQUIPANDMATERIALS, bytesCmd);
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, "Exception", false);
            }
        }
    }
}
