using GameServer.KiemThe.Core.Item;
using GameServer.KiemThe.Logic.Manager.Shop;
using GameServer.Logic;
using GameServer.Server;
using Server.Data;
using Server.Protocol;
using Server.TCP;
using Server.Tools;
using System;
using System.Text;

namespace GameServer.KiemThe.Logic
{
    /// <summary>
    /// Quản lý Shop
    /// </summary>
    public static partial class KT_TCPHandler
    {
        #region Shop

        /// <summary>
        /// Gửi gói tin về Client thông báo mở Shop tương ứng
        /// </summary>
        /// <param name="client"></param>
        /// <param name="_ShopData"></param>
        public static void SendShopData(KPlayer client, ShopTab _ShopData)
        {
            try
            {
                byte[] cmdData = DataHelper.ObjectToBytes<ShopTab>(_ShopData);
                client.SendPacket((int) TCPGameServerCmds.CMD_KT_C2G_OPENSHOP, cmdData);
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Exception, ex.ToString());
            }
        }

        /// <summary>
        /// Phản hồi sự kiện mở Kỳ Trân Các từ Client
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
        public static TCPProcessCmdResults ResponseOpenTokenShop(TCPManager tcpMgr, TMSKSocket socket, TCPClientPool tcpClientPool, TCPRandKey tcpRandKey, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
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
                /// Tìm chủ nhân của gói tin tương ứng
                KPlayer client = KTPlayerManager.Find(socket);
                if (null == client)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Không tìm thấy thông tin người chơi, CMD={0}, Client={1}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket)));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                byte[] bytesData = DataHelper.ObjectToBytes<TokenShop>(ShopManager.TokenShop);
                client.SendPacket((int) TCPGameServerCmds.CMD_KT_OPEN_TOKENSHOP, bytesData);

                return TCPProcessCmdResults.RESULT_OK;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, Global.GetDebugHelperInfo(socket), false);
            }

            return TCPProcessCmdResults.RESULT_FAILED;
        }

        #endregion Shop

        #region Buy items

        /// <summary>
        /// Mua 1 vật phẩm từ cửa hàng
        /// </summary>
        /// <param name="pool"></param>
        /// <param name="nID"></param>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static TCPProcessCmdResults ProcessSpriteNPCBuyCmd(TCPManager tcpMgr, TMSKSocket socket, TCPClientPool tcpClientPool, TCPRandKey tcpRandKey, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
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
                //Độ dài gói tin gửi lên phải là 5
                string[] fields = cmdData.Split(':');
                if (fields.Length != 5)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Error Socket params count not fit CMD={0}, Client={1}, Recv={2}",
                        (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), fields.Length));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// Id role
                int roleID = Convert.ToInt32(fields[0]);
                /// ID vật phẩm
                int goodsID = Convert.ToInt32(fields[1]);
                /// Số lượng
                int goodsNum = Convert.ToInt32(fields[2]);
                /// ID cửa hàng
                int saleType = Convert.ToInt32(fields[3]);
                /// ID phiếu giảm giá
                int couponID = Convert.ToInt32(fields[4]);

                KPlayer client = KTPlayerManager.Find(socket);
                if (null == client || client.RoleID != roleID)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Can not find player corresponding ID, CMD={0}, Client={1}, RoleID={2}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), roleID));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                if ((goodsNum <= 0 && saleType != 9999) || goodsNum >= 10000)
                {
                    KTPlayerManager.ShowNotification(client, "Số lượng không hợp lệ");

                    return TCPProcessCmdResults.RESULT_OK;
                }

                string strcmd = "";

                /// Thực hiện mua vật phẩm
                SubRep _REP = ShopManager.BuyItem(client, saleType, goodsID, goodsNum, couponID);

                if (_REP.IsOK && _REP.IsBuyBack)
                {
                    // Xử lý nhiệm vụ mua cái gì đó

                    strcmd = "1:" + _REP.CountLess + ":" + goodsID;
                }
                else if (_REP.IsOK && !_REP.IsBuyBack)
                {
                    ProcessTask.Process(Global._TCPManager.MySocketListener, pool, client, -1, -1, goodsID, TaskTypes.BuySomething);
                    strcmd = "0:" + _REP.CountLess + ":" + goodsID;
                }
                else
                {
                    strcmd = "-1:" + _REP.CountLess + ":" + goodsID;
                }

                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, strcmd, nID);
                tcpMgr.MySocketListener.SendData(socket, tcpOutPacket);

                return TCPProcessCmdResults.RESULT_OK;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, Global.GetDebugHelperInfo(socket), false);
            }

            return TCPProcessCmdResults.RESULT_FAILED;
        }

        #endregion Buy items

        #region Sell item

        /// <summary>
        /// Hàm bán vật phẩm vào NPC
        /// </summary>
        /// <param name="pool"></param>
        /// <param name="nID"></param>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static TCPProcessCmdResults ProcessSpriteNPCSaleOutCmd(TCPManager tcpMgr, TMSKSocket socket, TCPClientPool tcpClientPool, TCPRandKey tcpRandKey, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;
            string cmdData = null;

            try
            {
                cmdData = new UTF8Encoding().GetString(data, 0, count);
            }
            catch (Exception) //Toang j đó
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Decode data faild, CMD={0}, Client={1}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket)));
                return TCPProcessCmdResults.RESULT_FAILED;
            }

            try
            {
                string[] fields = cmdData.Split(':');

                bool UsAutoSell = false;

                if (fields.Length <2 || fields.Length > 3)
                {
                   
                    LogManager.WriteLog(LogTypes.Error, string.Format("Pram gửi lên đéo đúng CMD={0}, Client={1}, Recv={2}",
                        (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), fields.Length));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                if(fields.Length==3)
                {
                    UsAutoSell = true;
                }    

                int roleID = Convert.ToInt32(fields[0]);

                KPlayer client = KTPlayerManager.Find(socket);
                if (null == client || client.RoleID != roleID)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Không tìm thấy client, CMD={0}, Client={1}, RoleID={2}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), roleID));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// Nếu cần mật khẩu cấp 2
                if (client.NeedToShowInputSecondPassword())
                {
                    return TCPProcessCmdResults.RESULT_OK;
                }

                if (!UsAutoSell)
                {
                    /// Nếu không có NPC
                    if (client.LastShopNPC == null || client.LastShopNPC.MapCode != client.CurrentMapCode)
                    {
                        /// Cho toác luôn
                        return TCPProcessCmdResults.RESULT_FAILED;
                    }

                    /// Nếu vị trí quá xa
                    if (KTGlobal.GetDistanceBetweenPoints(client.CurrentPos, client.LastShopNPC.CurrentPos) > 100)
                    {
                        KTPlayerManager.ShowNotification(client, "Khoảng cách tới NPC quá xa!");
                        return TCPProcessCmdResults.RESULT_OK;
                    }
                }

               

                /// Nếu chưa đến thời gian được nhặt
                if (KTGlobal.GetCurrentTimeMilis() - client.LastPickUpDropItemTicks < 500)
                {
                    /// Không cho nhặt
                    return TCPProcessCmdResults.RESULT_OK;
                }
                /// Thiết lập thời gian nhặt lần trước
                client.LastPickUpDropItemTicks = KTGlobal.GetCurrentTimeMilis();


                int ItemIDDb = Convert.ToInt32(fields[1]);

                int price = 0;
                string strcmd = "";

                GoodsData ItemSell = client.GoodsData.Find(ItemIDDb, 0);
                bool isNotifyGoodsToClient = false;

                if (ItemSell != null)
                {
                    ItemData _Item = ItemManager.GetItemTemplate(ItemSell.GoodsID);
                    if (_Item != null)
                    {
                        /// Nếu là Ngũ Hành Ấn
                        if (ItemManager.KD_ISSIGNET(_Item.DetailType))
                        {
                            KTPlayerManager.ShowNotification(client, "Vật phẩm này không bán được!");
                            /// Không cho bán
                            return TCPProcessCmdResults.RESULT_OK;
                        }
                        /// Nếu là trang bị đã cường hóa
                        else if (ItemSell.Forge_level >= 8)
                        {
                            KTPlayerManager.ShowNotification(client, "Trang bị đã cường hóa trên cấp 8, không thể bán được!");
                            /// Không cho bán
                            return TCPProcessCmdResults.RESULT_OK;
                        }

                        else if (ItemSell.Endtime != ItemManager.ConstGoodsEndTime)
                        {
                            KTPlayerManager.ShowNotification(client, "Đồ có hạn sử dụng không thể bán!");
                            /// Không cho bán
                            return TCPProcessCmdResults.RESULT_OK;
                        }

                        /// Nếu là trang bị thì mới Notify về Client
                        if (ItemManager.KD_ISEQUIP(_Item.Genre))
                        {
                            isNotifyGoodsToClient = true;
                        }

                        /// Giá giảm nửa khi bán vào SHOP
                        price = _Item.Price / 2;
                        if (price < 0)
                        {
                            price = 0;
                        }

                        /// Xóa vật phẩm trước
                        if (ItemManager.AbandonItem(ItemSell, client, false, "SELL ITEM"))
                        {
                            if (ItemManager.KD_ISEQUIP(_Item.Genre))
                            {
                                client.AddItemSellGoodToDic(ItemSell);
                            }

                            /// Nếu có giá bán
                            if (price > 0)
                            {
                                /// Nếu là đồ khóa thì bán ra bạc khóa
                                if (ItemSell.Binding == 1)
                                {
                                    KTGlobal.AddMoney(client, price, Entities.MoneyType.BacKhoa, "SELL_NPC_ITEM_" + _Item.ItemID);
                                }
                                /// Nếu là đồ không khóa thì bán ra bạc thường
                                else
                                {
                                    KTGlobal.AddMoney(client, price, Entities.MoneyType.Bac, "SELL_NPC_ITEM_" + _Item.ItemID);
                                }
                            }
                        }
                        else
                        {
                            LogManager.WriteLog(LogTypes.Error, string.Format("Xóa vật phẩm không thành công, CMD={0}, Client={1}, RoleID={2}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), roleID));
                            return TCPProcessCmdResults.RESULT_FAILED;
                        }
                    }
                    else
                    {
                        LogManager.WriteLog(LogTypes.Error, string.Format("Template không tồn tại, CMD={0}, Client={1}, RoleID={2}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), roleID));
                        return TCPProcessCmdResults.RESULT_FAILED;
                    }
                }
                else
                {
                    string SendRemove = string.Format("{0}:{1}", client.RoleID, ItemIDDb);

                    // SEND VỀ CLLIENT XÓA Ở THỦ KHỐ
                    client.SendPacket((int)TCPGameServerCmds.CMD_SPR_MOVEGOODSDATA, SendRemove);

                    KTPlayerManager.ShowNotification(client, "Vật phẩm không tồn tại!");
                    return TCPProcessCmdResults.RESULT_OK;
                }

                G2C_PlayerSellItemToNPCShop playerSellItemToNPCShop = new G2C_PlayerSellItemToNPCShop()
                {
                    RoleID = roleID,
                    ItemGD = isNotifyGoodsToClient ? ItemSell : null,
                    BoundMoneyHave = client.BoundMoney,
                };
                byte[] _cmdData = DataHelper.ObjectToBytes<G2C_PlayerSellItemToNPCShop>(playerSellItemToNPCShop);
                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, _cmdData, nID);

                return TCPProcessCmdResults.RESULT_DATA;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, Global.GetDebugHelperInfo(socket), false);
            }

            return TCPProcessCmdResults.RESULT_FAILED;
        }

        #endregion Sell item
    }
}