using GameServer.Core.Executor;
using GameServer.KiemThe;
using GameServer.KiemThe.Core.Item;
using GameServer.KiemThe.Entities;
using GameServer.KiemThe.Logic;
using GameServer.Logic;
using GameServer.Server;
using Server.Data;
using GameServer.VLTK.Core.GuildManager;
using Server.Protocol;
using Server.TCP;
using Server.Tools;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using static GameServer.Logic.KTMapManager;

namespace GameServer.VLTK.Core.StallManager
{
    /// <summary>
    /// Class quản lý toàn bộ các sạp hàng ở máy chủ
    /// </summary>
    public class StallManager
    {
        /// <summary>
        /// Danh sách dữ liệu sạp hàng của toàn bộ người trong trong máy chủ
        /// </summary>
        public static ConcurrentDictionary<int, StallData> TotalServerStall = new ConcurrentDictionary<int, StallData>();

        /// <summary>
        /// Check xem vật phẩm có tồn tại không
        /// </summary>
        /// <param name="RoleID"></param>
        /// <param name="ItemID"></param>
        /// <returns></returns>
        public static bool CheckItemExitsInStallData(int RoleID, int ItemID)
        {
            TotalServerStall.TryGetValue(RoleID, out StallData _Stall);
            if (_Stall != null)
            {
                if (_Stall.GoodsPriceDict.ContainsKey(ItemID))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Số giờ bán = phiếu bán hàng
        /// </summary>
        public static int TOTALTIMEBOTFREE = 259200;

        /// <summary>
        /// Chỉ có 10 ô trống
        /// </summary>
        public static int STALLBAGNUM = 10;

        /// <summary>
        /// Lấy ra vị trí trên sập hàng
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        public static int GetFreeSlot(KPlayer client)
        {
            int idelPos = 0;

            for (int i = 0; i <= STALLBAGNUM; i++)
            {
                var FindByPos = client.GoodsData.Find(x => x.BagIndex == i && x.Site == 3);
                if (FindByPos == null)
                {
                    idelPos = i;
                    break;
                }
            }

            return idelPos;
        }

        /// <summary>
        /// Kiểm tra người chơi có sạp hàng ngồi bán trực tiếp không
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public static bool IsDirectStall(KPlayer player)
        {
            return player.CurrentAction == (int)KE_NPC_DOING.do_sale;
        }

        /// <summary>
        /// Chuyển đổi sang dữ liệu mini
        /// </summary>
        /// <param name="Data"></param>
        /// <returns></returns>
        public static MiniStallData ConverStallToMiniData(StallData Data)
        {
            if (Data != null)
            {
                MiniStallData MiniStallData = new MiniStallData();
                MiniStallData.StallID = Data.StallID;
                MiniStallData.RoleID = Data.RoleID;
                MiniStallData.StallName = Data.StallName;
                MiniStallData.IsBot = Data.IsBot;
                MiniStallData.GoodsPriceDict = Data.GoodsPriceDict;
                MiniStallData.AddDateTime = Data.AddDateTime;
                MiniStallData.Start = Data.Start;
                MiniStallData.ListResID = Data.ListResID;
                MiniStallData.MapCode = Data.MapCode;
                MiniStallData.PosX = Data.PosX;
                MiniStallData.PosY = Data.PosY;
                MiniStallData.RoleName = Data.RoleName;

                return MiniStallData;
            }
            return null;
        }

        /// <summary>
        /// Khôi phục lại toàn bộ dữ liệu bày bán==> Hủy Bán
        /// </summary>
        /// <param name="client"></param>
        /// <param name="sd"></param>
        public static void RestoreStallData(KPlayer client, StallData sd)
        {
            lock (sd)
            {
                for (int i = 0; i < sd.GoodsList.Count; i++)
                {
                    StallManager.StallMoveToBag(client, sd.GoodsList[i].Id);

                    //  Global.AddGoodsData(client, sd.GoodsList[i]);

                    // GameManager.ClientMgr.NotifyMoveGoods(_TCPManager.MySocketListener, _TCPManager.TcpOutPacketPool, client, sd.GoodsList[i], 1);
                }

                sd.GoodsList.Clear();
            }
        }

        /// <summary>
        /// SỬa packet chuyển vật phẩm từ sạp hàng lên túi đồ
        /// </summary>
        /// <param name="client"></param>
        /// <param name="Item"></param>
        /// <returns></returns>
        public static bool StallMoveToBag(KPlayer client, int ItemDBID)
        {
            GoodsData gd = client.GoodsData.Find(ItemDBID);

            int SlotIndex = client.GoodsData.GetBagFirstEmptyPosition(0);

            if (gd != null)
            {
                Dictionary<UPDATEITEM, object> TotalUpdate = new Dictionary<UPDATEITEM, object>();

                if (SlotIndex != -1)
                {
                    TotalUpdate.Add(UPDATEITEM.ROLEID, client.RoleID);
                    TotalUpdate.Add(UPDATEITEM.ITEMDBID, gd.Id);
                    // Set Lại Site cho vật phẩm này là ở túi đồ
                    TotalUpdate.Add(UPDATEITEM.SITE, 0);
                    // Set lại Vị trí của vật phẩm này trong túi đồ
                    TotalUpdate.Add(UPDATEITEM.BAGINDEX, SlotIndex);
                }
                else  // Nếu mà đầy túi đồ thì thôi
                {
                    KTPlayerManager.ShowMessageBox(client, "Thông báo", "Túi đồ đã đầy không thể lấy ra");
                    return false;
                }

                return client.GoodsData.Update(gd, TotalUpdate, true, true, "StallItemModify");
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Add vật phẩm vào khung bày bán
        /// </summary>
        /// <param name="client"></param>
        /// <param name="goodsDbID"></param>
        /// <param name="ed"></param>
        /// <returns></returns>
        public static bool AddGoodsDataIntoStallData(KPlayer client, int goodsDbID, StallData sd, int price)
        {
            lock (sd)
            {
                //Tổng có bao nhiêu ô đồ
                if (sd.GoodsList.Count < StallManager.STALLBAGNUM)
                {
                    GoodsData gd = client.GoodsData.Find(goodsDbID);

                    if (null == gd)
                    {
                        return false;
                    }

                    if (gd.Binding > 0)
                    {
                        return false; //Nếu mà khóa thì thôi
                    }

                    // Thực hiện update vật phẩm sửa lại SITE và BagIndex Cho nó
                    if (StallManager.BagToStall(client, goodsDbID))
                    {  //GameManager.ClientMgr.NotifyMoveGoods(Global._TCPManager.MySocketListener, Global._TCPManager.TcpOutPacketPool, client, gd, 0);
                        // Nếu vật phẩm này chưa có trong sạp thì thực hiện add vào
                        if (-1 == sd.GoodsList.IndexOf(gd))
                        {
                            sd.GoodsList.Add(gd);
                        }

                        sd.GoodsPriceDict[gd.Id] = price;
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Hàm mua vật phẩm của 1 thằng khác
        /// </summary>
        /// <param name="client"></param>
        /// <param name="otherClientID"></param>
        /// <param name="sd"></param>
        /// <param name="goodsDbID"></param>
        /// <returns></returns>
        public static int BuyFromStallData(KPlayer client, int otherClientID, StallData sd, int goodsDbID)
        {
            lock (sd)
            {
                KPlayer otherClient = KTPlayerManager.Find(otherClientID);

                if (sd.GoodsList.Count <= 0)
                {
                    KTPlayerManager.ShowNotification(client, "Vật phẩm yêu cầu không còn nữa");
                    return -11;
                }

                int goodsPrice = 0;
                if (sd.GoodsPriceDict.TryGetValue(goodsDbID, out goodsPrice))
                {
                    goodsPrice = Math.Max(goodsPrice, 0);
                }

                int ret = -12;
                bool found = false;
                for (int i = 0; i < sd.GoodsList.Count; i++)
                {
                    if (sd.GoodsList[i].Id == goodsDbID)
                    {
                        if (goodsPrice > 0)
                        {
                            if (client.Money - goodsPrice < 0) //Nếu thằng mua ko đủ tiền
                            {
                                KTPlayerManager.ShowNotification(client, "Tiền trên người không đủ");
                                ret = -13;
                                break;
                            }
                        }

                        //Thực hiện move good data cho thằng mua
                        if (!KTTradeManager.MoveGoodsDataToOtherRoleOffline(Global._TCPManager.MySocketListener, Global._TCPManager.tcpClientPool, Global._TCPManager.TcpOutPacketPool,
                            sd.GoodsList[i], otherClientID, client, true, "BUYFROMOTHER", -1))
                        {
                            return -15;
                        }
                        else
                        {
                            //Nếu chuyển vật phẩm thành công
                            KTPlayerManager.SubMoney(client, goodsPrice, "[TRỪ TIỀN TỪ VIỆC MUA VẬT PHẨM]");

                            // nếu thằng này có online thì thực hiện add tiền vào người luôn
                            if (otherClient != null)
                            {
                                otherClient.GoodsData.Remove(sd.GoodsList[i]);
                                KTPlayerManager.AddMoney(otherClient, (int)(goodsPrice * 0.90), "[NHẬN TIỀN TỪ VIỆC BÁN VẬT PHẨM]");

                                double MoneyForHostCity = goodsPrice * 0.03;
                                if (MoneyForHostCity > 0)
                                {
                                   GuildManager.GuildManager.getInstance().AddBoundMoneyForHostCity((int)MoneyForHostCity);
                                }
                            }
                            else
                            {
                                ItemData _Temp = ItemManager.GetItemTemplate(sd.GoodsList[i].GoodsID);
                                if (_Temp != null)
                                {
                                    int Money = (int)(goodsPrice * 0.90);
                                    double MoneyForHostCity = goodsPrice * 0.03;
                                    if (MoneyForHostCity > 0)
                                    {
                                        GuildManager.GuildManager.getInstance().AddBoundMoneyForHostCity((int)MoneyForHostCity);
                                    }
                                    KTGlobal.SendMail(null, otherClientID, "Shop", "Bán vật paharm", "Vật phẩm [" + _Temp.Name + "] đã được bán trong khi bạn offline hãy nhận bạc đính kèm", 0, Money, 1);
                                }
                            }
                            //Xóa khỏi danh sách giá
                            sd.GoodsPriceDict.Remove(goodsDbID);
                            found = true;
                            sd.GoodsList.RemoveAt(i);
                            break;
                        }
                    }
                }

                if (!found)
                {
                    return ret;
                }

                return 0;
            }
        }

        /// <summary>
        /// Xóa vật phẩm khỏi sạp hàng
        /// </summary>
        /// <param name="client"></param>
        /// <param name="goodsDbID"></param>
        /// <param name="ed"></param>
        /// <returns></returns>
        public static bool RemoveGoodsDataFromStallData(KPlayer client, int goodsDbID, StallData sd)
        {
            GoodsData gd = null;
            lock (sd)
            {
                int SlotIndex = client.GoodsData.GetBagFirstEmptyPosition(0);
                // Check trước nếu không có vị trí trống thì thôi
                if (SlotIndex == -1)
                {
                    return false;
                }

                for (int i = 0; i < sd.GoodsList.Count; i++)
                {
                    if (sd.GoodsList[i].Id == goodsDbID)
                    {
                        gd = sd.GoodsList[i];
                        sd.GoodsList.RemoveAt(i);
                        sd.GoodsPriceDict.Remove(gd.Id);
                        break;
                    }
                }
            }

            if (null == gd)
            {
                return false;
            }

            //Chuyển từ stall về túi đồ
            StallManager.StallMoveToBag(client, goodsDbID);

            return true;
        }

        /// <summary>
        /// Chuyển vật phẩm từ túi đồ lên sạp hàng để bán
        /// </summary>
        /// <param name="client"></param>
        /// <param name="ItemDBID"></param>
        /// <returns></returns>
        public static bool BagToStall(KPlayer client, int ItemDBID)
        {
            // Lấy ra vật phẩm theo ITEMDBID
            GoodsData gd = client.GoodsData.Find(ItemDBID);

            // Lấy ra vị trí trống
            int SlotIndex = StallManager.GetFreeSlot(client);

            if (gd != null)
            {
                Dictionary<UPDATEITEM, object> TotalUpdate = new Dictionary<UPDATEITEM, object>();

                if (SlotIndex != -1 && SlotIndex < STALLBAGNUM)
                {
                    TotalUpdate.Add(UPDATEITEM.ROLEID, client.RoleID);
                    TotalUpdate.Add(UPDATEITEM.ITEMDBID, gd.Id);
                    // Set lại SITE của thằng này trên sập hàng
                    TotalUpdate.Add(UPDATEITEM.SITE, 3);
                    // Set lại Vị trí của vật phẩm này trong túi đồ
                    TotalUpdate.Add(UPDATEITEM.BAGINDEX, SlotIndex);
                }
                else  // Nếu mà đầy túi đồ thì thôi
                {
                    KTPlayerManager.ShowMessageBox(client, "Thông báo", "Sập hàng đã đầy không thể bỏ thêm đồ lên");
                    return false;
                }

                return client.GoodsData.Update(gd, TotalUpdate, true, true, "StallItemModify");
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Lấy dữ liệu bày bán từ gamedb ra thông tin quản lý sập hàng khi khởi động máy chủ xong
        /// </summary>
        public static bool InitServerData()
        {
            try
            {
                byte[] bytesData = null;
                if (TCPProcessCmdResults.RESULT_FAILED == Global.RequestToDBServer3(Global._TCPManager.tcpClientPool, Global._TCPManager.TcpOutPacketPool, (int)TCPGameServerCmds.CMD_SPR_STALL_QUERRY, string.Format("{0}:{1}", GameManager.LocalServerId, 1), out bytesData, GameManager.LocalServerId))
                {
                    return false;
                }

                if (null == bytesData || bytesData.Length <= 6)
                {
                    return false;
                }

                Int32 length = BitConverter.ToInt32(bytesData, 0);

                //Lấy ra toàn bộ dữ liệu bán hàng của người chơi
                ConcurrentDictionary<int, StallData> Stall_Info = DataHelper.BytesToObject<ConcurrentDictionary<int, StallData>>(bytesData, 6, length - 2);

                TotalServerStall = Stall_Info;

                ///Thực hiện dump toàn bộ object đang ngồi lên bản đồ trên các bản đồ
                ProseccDumpObjectInClient();

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Trả ra tên sạp hàng
        /// </summary>
        /// <param name="roleID"></param>
        /// <param name="includeBot"></param>
        /// <returns></returns>
        public static string GetStallName(int roleID, bool includeBot)
        {
            /// Thông tin sạp hàng
            if (StallManager.TotalServerStall.TryGetValue(roleID, out StallData stallData))
            {
                /// Nếu là sạp hàng ủy thác
                if (stallData.IsBot)
                {
                    /// Nếu lấy cả thông tin khi sạp hàng ủy thác
                    if (includeBot)
                    {
                        /// Trả về kết quả
                        return stallData.StallName;
                    }
                }
            }
            /// Không tìm thấy
            return "";
        }

        /// <summary>
        /// Gọi hàm này để load đống shop ngồi bán vào trong MapGirlManager
        /// Khả năng phải thêm 1 thực thể nữa vào client dạng shop
        /// </summary>
        public static void ProseccDumpObjectInClient()
        {
            foreach (StallData _StallData in TotalServerStall.Values)
            {
                // Nếu mà thằng này sử dụng bot thì sẽ tạo bot cho nó khi khởi động xong máy chủ
                if (_StallData.IsBot == true)
                {
                    int TotalSecFromNow = KTGlobal.GetOffsetSecond(DateTime.Now);
                    // Nếu còn thời gian bày bán
                    if (_StallData.AddDateTime - TotalSecFromNow > 0)
                    {
                        _StallData.Start = 1;
                        CreateBotStall(_StallData);
                    }
                }
                else
                {
                    // Nếu không phải bot thì set sạp hàng là chưa bán
                    _StallData.Start = 0;
                }

                // PROSCC DUMP HERE
            }
        }

        /// <summary>
        /// Hàm này sẽ xử lý trước khi người chơi thoát game
        /// </summary>
        public static void ProseccDataBeforeExitsGame(KPlayer client)
        {
            if (TotalServerStall.TryGetValue(client.RoleID, out StallData _OUTVALUE))
            {
                if (_OUTVALUE != null)
                {
                    client.CurrentAction = (int)KE_NPC_DOING.do_idle;
                    _OUTVALUE.Start = 0;
                }
            }
        }

        // TODO : TẠO BOT BÁN HÀNG GỬI VỀ CLIENT
        /// <summary>
        ///
        /// Tạo con bot bán hàng gửi về client
        /// </summary>
        /// <param name="_StallData"></param>
        public static void CreateBotStall(StallData _StallData)
        {
            if (_StallData == null)
            {
                return;
            }
            KTStallBotManager.Create(_StallData.MapCode, _StallData.PosX, _StallData.PosY, _StallData.RoleID, _StallData.RoleName, _StallData.ListResID[0], _StallData.StallName, _StallData.ListResID[1], _StallData.ListResID[2], _StallData.ListResID[3], -1);
        }

        /// <summary>
        /// Update StalLResource
        /// </summary>
        /// <param name="GuildID"></param>
        /// <param name="Type"></param>
        /// <param name="Value"></param>
        /// <returns></returns>
        public static bool UpdateStallData(StallDbAction ActionStall)
        {
            string[] Pram = Global.SendToDB<StallDbAction>((int)TCPGameServerCmds.CMD_SPR_STALL_UDPATE_DB, ActionStall, GameManager.LocalServerId);

            if (Pram == null)
            {
                return false;
            }

            if (Pram.Length != 2)
            {
                return false;
            }

            // Nếu như đéo update được thì thông báo toác
            if (Int32.Parse(Pram[0]) != 0)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Thực hiện hủy bán
        /// </summary>
        /// <param name="RoleID"></param>
        /// <returns></returns>
        public bool IsCancelSell(int RoleID)
        {
            TotalServerStall.TryGetValue(RoleID, out StallData _OutStall);

            if (_OutStall != null)
            {
                // Thực hiện remove trong DB
            }

            return false;
        }

        #region NEWORK

        /// <summary>
        /// Khi thằng kia click vào xem shop
        /// </summary>
        /// <param name="client"></param>
        /// <param name="otherRoleID"></param>
        public static void ShowStall(KPlayer client, int otherRoleID)
        {
            TotalServerStall.TryGetValue(otherRoleID, out StallData stallData);

            if (stallData != null)
            {
                UnityEngine.Vector2 ownerPos = new UnityEngine.Vector2(stallData.PosX, stallData.PosY);
                /// Vị trí của người chơi
                UnityEngine.Vector2 playerPos = new UnityEngine.Vector2((int)client.CurrentPos.X, (int)client.CurrentPos.Y);
                /// Khoảng cách
                float distance = UnityEngine.Vector2.Distance(ownerPos, playerPos);
                /// Nếu quá xa
                if (distance > 50)
                {
                    KTPlayerManager.ShowNotification(client, "Khoảng cách quá xa, không thể xem cửa hàng!");
                    return;
                }

                ///Trả về thông tin gian hàng cho thằng muốn xem
                KT_TCPHandler.NotifyGoodsStallData(client, stallData);
            }
            else
            {
                KTPlayerManager.ShowNotification(client, "Không tìm thấy cửa hàng này");
            }
        }

        /// <summary>
        /// Lấy dữ liệu sạp hàng của ai đó
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
        public static TCPProcessCmdResults CMD_SPR_STALLDATA(TCPManager tcpMgr, TMSKSocket socket, TCPClientPool tcpClientPool, TCPRandKey tcpRandKey, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;
            string cmdData = null;
            try
            {
                cmdData = new UTF8Encoding().GetString(data, 0, count);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Có lỗi khi kiểm tra dữ liệu gửi lên, CMD={0}, Client={1}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket)));
                return TCPProcessCmdResults.RESULT_FAILED;
            }

            try
            {
                string[] fields = cmdData.Split(':');
                if (fields.Length != 1)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Có lỗi ở tham số gửi lên, CMD={0}, Client={1}, Recv={2}",
                        (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), fields.Length));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                KPlayer client = KTPlayerManager.Find(socket);
                if (null == client)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Không tìm thấy người chơi, CMD={0}, Client={1}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket)));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// ID đối tượng
                int roleID = Convert.ToInt32(fields[0]);

                /// Nếu là chính mình
                if (roleID == client.RoleID)
                {
                    /// Ngừng di chuyển
                    KTPlayerStoryBoardEx.Instance.Remove(client);

                    /// Bản đồ tương ứng
                    GameMap map = KTMapManager.Find(client.MapCode);
                    /// Nếu bản đồ không cho phép mở sạp
                    if (!map.AllowStall || !client.IsInsideSafeZone)
                    {
                        KTPlayerManager.ShowNotification(client, "Khu vực hiện tại không cho phép mở sạp hàng!");
                        return TCPProcessCmdResults.RESULT_OK;
                    }
                    else if (client.ClientSocket.IsKuaFuLogin)
                    {
                        KTPlayerManager.ShowNotification(client, "Liên máy chủ không thể bán hàng!");
                        return TCPProcessCmdResults.RESULT_OK;
                    }

                    /// Thông tin sạp hàng cũ
                    StallManager.TotalServerStall.TryGetValue(roleID, out StallData stallData);

                    /// Nếu chưa có sạp hàng thì send 1 sạp mới về
                    if (stallData == null)
                    {
                        /// Tạo mới
                        stallData = new StallData();
                        stallData.StallID = client.RoleID;
                        stallData.IsBot = false;
                        stallData.GoodsList = new List<GoodsData>();
                        stallData.GoodsPriceDict = new Dictionary<int, int>();
                        stallData.AddDateTime = TimeUtil.NOW();
                        stallData.ListResID = client.GetPlayEquipBody().GetItemResID();
                        stallData.MapCode = client.MapCode;
                        stallData.PosX = client.PosX;
                        stallData.PosY = client.PosY;
                        stallData.Start = 0;
                        stallData.RoleID = client.RoleID;
                        stallData.StallName = "";
                        stallData.RoleName = client.RoleName;

                        /// Thêm vào danh sách
                        StallManager.TotalServerStall[roleID] = stallData;
                    }
                    //Tạo 1 sạp hàng và ném vào Db
                    MiniStallData _StallMini = ConverStallToMiniData(stallData);

                    StallDbAction _Aciton = new StallDbAction();
                    _Aciton.Type = (int)StallCommand.UPDATE;
                    _Aciton.MiniData = _StallMini;
                    _Aciton.Fields.Add(client.RoleID);

                    if (!StallManager.UpdateStallData(_Aciton))
                    {
                        KTPlayerManager.ShowNotification(client, "Có lỗi khi khởi tạo sạp hàng!");

                        LogManager.WriteLog(LogTypes.Stall, "Update stall data to DB failed.");

                        return TCPProcessCmdResults.RESULT_OK;
                    }

                    /// Gửi về dữ liệu sạp hàng
                    KT_TCPHandler.NotifyGoodsStallData(client, stallData);
                }
                /// Nếu là đối tượng khác
                else
                {
                    /// Người chơi khác
                    KPlayer otherClient = KTPlayerManager.Find(roleID);
                    /// Nếu tìm thấy
                    if (otherClient != null)
                    {
                        /// Hiện cửa hàng của người chơi tương ứng
                        StallManager.ShowStall(client, roleID);
                    }
                    /// Nếu không tìm thấy
                    else
                    {
                        /// Bot tương ứng
                        KStallBot bot = KTStallBotManager.FindBot(roleID);
                        /// Nếu không tìm thấy
                        if (bot == null)
                        {
                            /// Thông báo
                            KTPlayerManager.ShowNotification(client, "Đối tượng không tồn tại!");
                            return TCPProcessCmdResults.RESULT_OK;
                        }

                        /// Nếu là Bot của bản thân
                        if (bot.OwnerRoleID == client.RoleID)
                        {
                            /// Thông báo
                            KTPlayerManager.ShowNotification(client, "Để xem gian hàng của chính mình, hãy mở giao diện túi đồ và ấn Bày bán!");
                            return TCPProcessCmdResults.RESULT_OK;
                        }

                        /// Hiện cửa hàng của chủ nhân Bot tương ứng
                        StallManager.ShowStall(client, bot.OwnerRoleID);
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
        /// Thực hiện công việc bày bán
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
        public static TCPProcessCmdResults ProcessSpriteGoodsStallCmd(TCPManager tcpMgr, TMSKSocket socket, TCPClientPool tcpClientPool, TCPRandKey tcpRandKey, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;
            StallAction cmdData = null;

            try
            {
                cmdData = DataHelper.BytesToObject<StallAction>(data, 0, count);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Decode data faild, CMD={0}, Client={1}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket)));
                return TCPProcessCmdResults.RESULT_FAILED;
            }

            try
            {
                /// Người chơi tương ứng
                KPlayer client = KTPlayerManager.Find(socket);
                if (null == client)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Không tìm thấy người chơi, CMD={0}, Client={1}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket)));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// Ngừng di chuyển
                KTPlayerStoryBoardEx.Instance.Remove(client);

                /// Bản đồ tương ứng
                GameMap map = KTMapManager.Find(client.MapCode);
                /// Nếu bản đồ không cho phép mở sạp
                if (!map.AllowStall || !client.IsInsideSafeZone)
                {
                    KTPlayerManager.ShowNotification(client, "Khu vực hiện tại không cho phép mở sạp hàng!");
                    return TCPProcessCmdResults.RESULT_OK;
                }

                if (client.ClientSocket.IsKuaFuLogin)
                {
                    return TCPProcessCmdResults.RESULT_OK;
                }

                /// Nếu đang có khóa cấp 2
                if (client.NeedToShowInputSecondPassword())
                {
                    return TCPProcessCmdResults.RESULT_OK;
                }

                /// nếu bị khóa bán hàng
                if (client.IsBannedFeature(RoleBannedFeature.SaleGoods, out int timeLeftSec))
                {
                    KTPlayerManager.ShowNotification(client, "Bạn đang bị khóa chức năng bán hàng. Thời gian còn " + KTGlobal.DisplayFullDateAndTime(timeLeftSec) + "!");
                    return TCPProcessCmdResults.RESULT_OK;
                }

                /// Loại thao tác
                int stallType = cmdData.Type;

                /// Bắt đầu bán hàng
                if (stallType == (int)GoodsStallCmds.Start)
                {
                    /// Nếu đang cưỡi ngựa thì bỏ qua
                    if (client.IsRiding)
                    {
                        KTPlayerManager.ShowNotification(client, "Đang cưỡi ngựa không thể mở cửa hàng");

                        return TCPProcessCmdResults.RESULT_OK;
                    }
                    /// Nếu chưa đủ cấp 10 thì không cho bán
                    if (client.m_Level < 10)
                    {
                        KTPlayerManager.ShowNotification(client, "Phải cấp 10 mới có thể bán hàng");

                        return TCPProcessCmdResults.RESULT_OK;
                    }

                    /// Tên cửa hàng
                    string stallName = (string)cmdData.Fields[0];
                    /// Có phải ủy thác không
                    bool isBot = bool.Parse(cmdData.Fields[1]);

                    /// Thông tin sạp hàng
                    StallManager.TotalServerStall.TryGetValue(client.RoleID, out StallData stallData);
                    /// Nếu tìm thấy
                    if (stallData != null)
                    {
                        if (stallData.Start == 1)
                        {
                            KTPlayerManager.ShowNotification(client, "Bạn đang bán hàng rồi");
                            return TCPProcessCmdResults.RESULT_OK;
                        }

                        // Làm 1 bool xem thằng này có thể bày bán ko
                        bool IsCanSell = false;
                        /// Nếu ủy thác
                        if (isBot)
                        {   /// Đánh dấu dùng Bot
                            stallData.IsBot = true;

                            //Lấy ra tổng số thời gian bán hàng bởi bot xem có còn không
                            int TotalSecLess = client.GetValueOfForeverRecore(ForeverRecord.TimeSaleByBot);

                            //Thời gian bây giờ

                            int TotalSecFromNow = KTGlobal.GetOffsetSecond(DateTime.Now);

                            // Nếu cón thời gian bán hàng
                            if (TotalSecLess != -1)
                            {
                                int LessTime = TotalSecLess - TotalSecFromNow;

                                if (LessTime > 0)
                                {
                                    KTPlayerManager.ShowNotification(client, "Bắt đầu bán hàng! Thời gian ủy thác bằng bot còn " + LessTime + " giây!");
                                    IsCanSell = true;
                                    stallData.AddDateTime = TotalSecLess;
                                }
                            }

                            // Nếu đéo thể bán hàng thì check xem người có phiếu hay không
                            if (!IsCanSell)
                            {
                                int Count = ItemManager.GetItemCountInBag(client, 8464);
                                if (Count == 0)
                                {
                                    KTPlayerManager.ShowNotification(client, "Cần vật phẩm phiếu bán hàng để có thể ủy thác bày bán!");
                                    return TCPProcessCmdResults.RESULT_OK;
                                }
                                else
                                {
                                    KTPlayerManager.ShowMessageBox(client, "Thông báo", "Sẽ tiêu tốn 1 vật phẩm <color=red>Phiếu bán hàng (3 ngày)</color> để có thể ủy thác bày bán,bạn có chắc chắn muốn ủy thác bày bán không?", () =>
                                    {
                                        if (ItemManager.RemoveItemFromBag(client, 8464, 1))
                                        {
                                            int TotalSecCanSell = KTGlobal.GetOffsetSecond(DateTime.Now.AddDays(3));
                                            //Set vào tổng thời gian có thể bày bán
                                            client.SetValueOfForeverRecore(ForeverRecord.TimeSaleByBot, TotalSecCanSell);

                                            stallData.AddDateTime = TotalSecLess;

                                            IsCanSell = true;

                                            stallData.StallName = stallName;
                                            /// Ghi vào DB
                                            /// Đánh dấu là đang bày bán
                                            stallData.Start = 1;
                                            /// Lưu lại tên cửa hàng
                                            /// Gửi lại cho Client để Update vào khung
                                            client.SendPacket<StallAction>(nID, cmdData);

                                            /// Thông báo
                                            KTPlayerManager.ShowNotification(client, "Bắt đầu bán hàng! Thời gian ủy thác bằng bot còn 3 ngày");

                                            //Tạo 1 sạp hàng và ném vào Db
                                            MiniStallData _StallMini = ConverStallToMiniData(stallData);

                                            StallDbAction _Aciton = new StallDbAction();
                                            _Aciton.Type = (int)StallCommand.UPDATE;
                                            _Aciton.MiniData = _StallMini;
                                            _Aciton.Fields.Add(client.RoleID);

                                            if (!StallManager.UpdateStallData(_Aciton))
                                            {
                                                KTPlayerManager.ShowNotification(client, "Có lỗi khi khởi tạo sạp hàng!");

                                                LogManager.WriteLog(LogTypes.Stall, "Update stall data to DB failed.");
                                            }
                                            /// Nếu ủy thác
                                            if (isBot)
                                            {
                                                /// Tạo Bot bán hàng
                                                StallManager.CreateBotStall(stallData);
                                            }

                                            /// Nếu bán thường
                                        }
                                        else
                                        {
                                            KTPlayerManager.ShowNotification(client, "Có lỗi khi trừ vật phẩm!");
                                            return;
                                        }
                                    }, true);
                                }
                            }
                        }
                        else // Nếu đéo dùng bot thì bán được luôn
                        {
                            IsCanSell = true;
                        }

                        if (IsCanSell)
                        {
                            stallData.StallName = stallName;
                            /// Ghi vào DB
                            /// Đánh dấu là đang bày bán
                            stallData.Start = 1;
                            /// Lưu lại tên cửa hàng
                            /// Gửi lại cho Client để Update vào khung
                            client.SendPacket<StallAction>(nID, cmdData);

                            /// Thông báo
                            KTPlayerManager.ShowNotification(client, "Bắt đầu bán hàng!");

                            //Tạo 1 sạp hàng và ném vào Db
                            MiniStallData _StallMini = ConverStallToMiniData(stallData);

                            StallDbAction _Aciton = new StallDbAction();
                            _Aciton.Type = (int)StallCommand.UPDATE;
                            _Aciton.MiniData = _StallMini;
                            _Aciton.Fields.Add(client.RoleID);

                            if (!StallManager.UpdateStallData(_Aciton))
                            {
                                KTPlayerManager.ShowNotification(client, "Có lỗi khi khởi tạo sạp hàng!");

                                LogManager.WriteLog(LogTypes.Stall, "Update stall data to DB failed.");

                                return TCPProcessCmdResults.RESULT_OK;
                            }
                            /// Nếu ủy thác
                            if (isBot)
                            {
                                /// Tạo Bot bán hàng
                                StallManager.CreateBotStall(stallData);
                            }
                            /// Nếu bán thường
                            else
                            {
                                client.CurrentAction = (int)KE_NPC_DOING.do_sale;
                                /// Thông báo thằng này bắt đầu bán hàng cho bọn xung quanh
                                KT_TCPHandler.NotifySpriteStartStall(client);
                            }
                        }
                    }
                    /// Toác
                    else
                    {
                        KTPlayerManager.ShowNotification(client, "Chưa khởi tạo gian hàng không thể bán");
                        return TCPProcessCmdResults.RESULT_OK;
                    }
                }
                else if (stallType == (int)GoodsStallCmds.Cancel)
                {
                    /// Thông tin sạp hàng
                    StallManager.TotalServerStall.TryGetValue(client.RoleID, out StallData stallData);
                    /// Nếu tìm thấy
                    if (stallData != null)
                    {
                        /// Số vật phẩm đang bán
                        int totalSellingItems = stallData.GoodsList.Count;
                        /// Nếu không đủ khoảng trống
                        if (!KTGlobal.IsHaveSpace(totalSellingItems, client))
                        {
                            KTPlayerManager.ShowNotification(client, "Túi đồ không đủ chỗ để thu sạp hàng về!");
                            return TCPProcessCmdResults.RESULT_OK;
                        }
                        /// Set thằng này không bán
                        stallData.Start = 0;

                        /// Nếu ủy thác
                        if (stallData.IsBot)
                        {
                            KStallBot bot = KTStallBotManager.FindBotByOwnerID(client.RoleID);
                            bot?.Destroy();
                        }

                        /// Khôi phục dữ liệu bày bán
                        StallManager.RestoreStallData(client, stallData);
                        /// Xóa sạp hàng của thằng này
                        ///
                        StallDbAction _Action = new StallDbAction();
                        _Action.Type = (int)StallCommand.DELETE_STALL;
                        _Action.Fields.Add(client.RoleID);

                        if (!UpdateStallData(_Action))
                        {
                            LogManager.WriteLog(LogTypes.Stall, "Update stall data to DB failed.");
                        }
                        else
                        {
                            StallManager.TotalServerStall.TryRemove(client.RoleID, out _);
                        }

                        /// Gửi gói tin này về
                        KT_TCPHandler.NotifySpriteStopStall(client);
                        /// Gửi lại cho Client để Update vào khung
                        client.SendPacket<StallAction>(nID, cmdData);
                        KT_TCPHandler.NotifyGoodsStallData(client, stallData);
                    }
                    /// Toác
                    else
                    {
                        KTPlayerManager.ShowNotification(client, "Sạp hàng không tồn tại!");
                        return TCPProcessCmdResults.RESULT_OK;
                    }
                }
                /// Thêm vật phẩm vào sạp hàng
                else if (stallType == (int)GoodsStallCmds.AddGoods)
                {
                    /// ID vật phẩm
                    int addGoodsDbID = int.Parse(cmdData.Fields[0]);
                    /// Giá bán
                    int price = int.Parse(cmdData.Fields[1]);

                    /// Thông tin cửa hàng
                    StallManager.TotalServerStall.TryGetValue(client.RoleID, out StallData stallData);
                    /// Nếu tìm thấy
                    if (stallData != null)
                    {
                        /// Thêm vật phẩm vào cửa hàng
                        if (StallManager.AddGoodsDataIntoStallData(client, addGoodsDbID, stallData, price))
                        {
                            /// Gửi lại cho Client để Update vào khung
                            client.SendPacket<StallAction>(nID, new StallAction()
                            {
                                Type = cmdData.Type,
                                Fields = new List<string>()
                                {
                                    price.ToString(),
                                },
                                GoodsData = client.GoodsData.Find(addGoodsDbID),
                            });

                            // Thêm một vật phẩm vào cache ở gameDb
                            StallDbAction _Action = new StallDbAction();
                            _Action.Type = (int)StallCommand.INSERT_ITEM;
                            _Action.Fields.Add(client.RoleID);
                            _Action.Fields.Add(addGoodsDbID);
                            _Action.Fields.Add(price);

                            if (!UpdateStallData(_Action))
                            {
                                LogManager.WriteLog(LogTypes.Stall, "Update stall data to DB failed.");
                            }
                        }
                    }
                    /// Toác
                    else
                    {
                        KTPlayerManager.ShowNotification(client, "Sạp hàng không tồn tại!");
                        return TCPProcessCmdResults.RESULT_OK;
                    }
                }
                /// Xóa vật phẩm khỏi sạp hàng
                else if (stallType == (int)GoodsStallCmds.RemoveGoods)
                {
                    /// ID vật phẩm
                    int addGoodsDbID = int.Parse(cmdData.Fields[0]);

                    /// Thông tin cửa hàng
                    StallManager.TotalServerStall.TryGetValue(client.RoleID, out StallData stallData);
                    /// Nếu tìm thấy
                    if (stallData != null)
                    {
                        /// Xóa vật phẩm khỏi cửa hàng
                        if (StallManager.RemoveGoodsDataFromStallData(client, addGoodsDbID, stallData))
                        {
                            /// Gửi lại cho Client để Update vào khung
                            client.SendPacket<StallAction>(nID, new StallAction()
                            {
                                Type = (int)GoodsStallCmds.RemoveGoods,
                                Fields = new List<string>()
                                {
                                    client.RoleID.ToString(),
                                    addGoodsDbID.ToString(),
                                },
                            });

                            // Thực hiện gỡ cache ở trong gamedb
                            StallDbAction _Action = new StallDbAction();
                            _Action.Type = (int)StallCommand.REMOVE_ITEM;
                            _Action.Fields.Add(client.RoleID);
                            _Action.Fields.Add(addGoodsDbID);
                            if (!UpdateStallData(_Action))
                            {
                                LogManager.WriteLog(LogTypes.Stall, "Update stall data to DB failed.");
                            }
                        }
                    }
                    /// Toác
                    else
                    {
                        KTPlayerManager.ShowNotification(client, "Sạp hàng không tồn tại!");
                        return TCPProcessCmdResults.RESULT_OK;
                    }
                }
                else if (stallType == (int)GoodsStallCmds.BuyGoods)
                {
                    /// ID chủ shop
                    int otherRoleID = int.Parse(cmdData.Fields[0]);
                    /// ID vật phẩm
                    int goodsDbId = int.Parse(cmdData.Fields[1]);

                    /// Thông tin sạp hàng
                    StallData stallData = null;

                    //StallManager.TotalServerStall.TryGetValue(otherRoleID, out stallData);

                    /// Người chơi tương ứng nếu có
                    KPlayer otherClient = KTPlayerManager.Find(otherRoleID);
                    /// Nếu là người chơi
                    if (otherClient != null)
                    {
                        /// Thông tin cửa hàng
                        StallManager.TotalServerStall.TryGetValue(otherClient.RoleID, out stallData);
                    }
                    /// Nếu là Bot
                    else
                    {
                        /// Bot nếu có
                        KStallBot bot = KTStallBotManager.FindBotByOwnerID(otherRoleID);
                        /// Toác
                        if (bot == null)
                        {
                            KTPlayerManager.ShowNotification(client, "Sạp hàng không tồn tại!");
                            return TCPProcessCmdResults.RESULT_OK;
                        }

                        /// Thông tin cửa hàng
                        StallManager.TotalServerStall.TryGetValue(bot.OwnerRoleID, out stallData);

                        /// ID là ID chủ nhân
                        otherRoleID = bot.OwnerRoleID;
                        /// Tìm thằng chủ nhân của nó nếu Online
                        otherClient = KTPlayerManager.Find(otherRoleID);
                    }

                    /// Nếu không tìm thấy
                    if (stallData == null)
                    {
                        KTPlayerManager.ShowNotification(client, "Sạp hàng không tồn tại!");
                        return TCPProcessCmdResults.RESULT_OK;
                    }
                    /// Nếu chưa bán
                    else if (stallData.Start == 0)
                    {
                        KTPlayerManager.ShowNotification(client, "Sạp hàng không tồn tại!");
                        return TCPProcessCmdResults.RESULT_OK;
                    }

                    /// Vị trí hiện tại của chủ cửa hàng
                    UnityEngine.Vector2 ownerPos = new UnityEngine.Vector2(stallData.PosX, stallData.PosY);
                    /// Vị trí của người chơi
                    UnityEngine.Vector2 playerPos = new UnityEngine.Vector2((int)client.CurrentPos.X, (int)client.CurrentPos.Y);
                    /// Khoảng cách
                    float distance = UnityEngine.Vector2.Distance(ownerPos, playerPos);
                    /// Nếu quá xa
                    if (distance > 50)
                    {
                        KTPlayerManager.ShowNotification(client, "Khoảng cách quá xa, không thể mua vật phẩm!");
                        return TCPProcessCmdResults.RESULT_OK;
                    }

                    /// Thực hiện mua hàng
                    int ret = StallManager.BuyFromStallData(client, otherRoleID, stallData, goodsDbId);
                    /// Toác
                    if (ret != 0)
                    {
                        KTPlayerManager.ShowNotification(client, "Có lỗ khi thực hiện mua vật phẩm!");
                        return TCPProcessCmdResults.RESULT_OK;
                    }

                    client.SendPacket<StallAction>(nID, new StallAction()
                    {
                        Type = (int)GoodsStallCmds.RemoveGoods,
                        Fields = new List<string>()
                        {
                            otherRoleID.ToString(),
                            goodsDbId.ToString(),
                        },
                    });

                    /// Nếu tìm thấy thằng bán
                    if (otherClient != null)
                    {
                        ///// Cập nhật lại cho nó
                        //GameManager.ClientMgr.NotifyGoodsStallData(tcpMgr.MySocketListener, pool, otherClient, stallData);

                        /// Xóa vật phẩm tương ứng cho nó
                        otherClient.SendPacket<StallAction>(nID, new StallAction()
                        {
                            Type = (int)GoodsStallCmds.RemoveGoods,
                            Fields = new List<string>()
                            {
                                otherRoleID.ToString(),
                                goodsDbId.ToString(),
                            },
                        });
                    }

                    StallDbAction _Action = new StallDbAction();
                    _Action.Type = (int)StallCommand.REMOVE_ITEM;
                    _Action.Fields.Add(otherRoleID);
                    _Action.Fields.Add(goodsDbId);
                    /// Thực hiện cập nhật dữ liệu sạp hàng vào DB
                    if (!StallManager.UpdateStallData(_Action))
                    {
                        LogManager.WriteLog(LogTypes.Stall, "Update stall data to DB failed.");
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

        #endregion NEWORK
    }
}