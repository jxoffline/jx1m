using GameServer.KiemThe;
using GameServer.KiemThe.Core.Item;
using GameServer.KiemThe.Entities;
using GameServer.KiemThe.Logic;
using GameServer.Server;
using GameServer.VLTK.Core.GuildManager;
using Server.Data;
using Server.Protocol;
using Server.TCP;
using Server.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameServer.Logic
{
    /// <summary>
    /// Quản lý giao dịch
    /// </summary>
    public static partial class KTTradeManager
    {
        /// <summary>
        /// Xóa vật phẩm khỏi giao dịch
        /// </summary>
        /// <param name="client"></param>
        /// <param name="goodsDbID"></param>
        /// <param name="ed"></param>
        /// <returns></returns>
        public static bool RemoveGoodsDataFromExchangeData(KPlayer client, int goodsDbID, ExchangeData ed)
        {
            if (IsLockExchangeData(client.RoleID, ed))
            {
                return true;
            }

            GoodsData gd = null;
            lock (ed)
            {
                List<GoodsData> goodsDataList = null;
                if (ed.GoodsDict.TryGetValue(client.RoleID, out goodsDataList))
                {
                    for (int i = 0; i < goodsDataList.Count; i++)
                    {
                        if (goodsDataList[i].Id == goodsDbID)
                        {
                            gd = goodsDataList[i];
                            goodsDataList.RemoveAt(i);
                            break;
                        }
                    }
                }
            }

            if (null == gd)
                return false;
            client.GoodsData.Add(gd);
            KT_TCPHandler.NotifyMoveGoods(client, gd, 1);

            return true;
        }

        /// <summary>
        /// Update tiền vào phiên giao dịch
        /// </summary>
        /// <param name="client"></param>
        /// <param name="money"></param>
        /// <param name="ed"></param>
        /// <returns></returns>
        public static bool UpdateExchangeDataMoney(KPlayer client, int money, ExchangeData ed)
        {
            //Nếu đã khóa rồi
            if (IsLockExchangeData(client.RoleID, ed))
            {
                return true;
            }

            lock (ed)
            {
                ed.MoneyDict[client.RoleID] = money;
            }

            return true;
        }

        /// <summary>
        /// Khôi phục lại đồ vào túi đồ cho thằng hủy giao dịch
        /// </summary>
        /// <param name="client"></param>
        /// <param name="ed"></param>
        public static void RestoreExchangeData(KPlayer client, ExchangeData ed)
        {
            lock (ed)
            {
                List<GoodsData> goodsDataList = null;

                if (ed.GoodsDict.TryGetValue(client.RoleID, out goodsDataList))
                {
                    for (int i = 0; i < goodsDataList.Count; i++)
                    {
                        client.GoodsData.Add(goodsDataList[i]);

                        KT_TCPHandler.NotifyMoveGoods(client, goodsDataList[i], 1);
                    }

                    ed.GoodsDict.Remove(client.RoleID);
                }
            }
        }

        /// <summary>
        /// Khóa giao dịch
        /// </summary>
        /// <param name="client"></param>
        /// <param name="ed"></param>
        /// <param name="locked"></param>
        public static void LockExchangeData(int roleID, ExchangeData ed, int locked)
        {
            lock (ed)
            {
                ed.LockDict[roleID] = locked;
            }
        }

        /// <summary>
        /// Kiểm tra xem đã kháo giao dịch chưa
        /// </summary>
        /// <param name="client"></param>
        /// <param name="ed"></param>
        public static bool IsLockExchangeData(int roleID, ExchangeData ed)
        {
            int locked = 0;
            lock (ed)
            {
                ed.LockDict.TryGetValue(roleID, out locked);
            }

            return (locked > 0);
        }

        /// <summary>
        /// Hoàn thành giao dịch
        /// </summary>
        /// <param name="roleID"></param>
        /// <param name="ed"></param>
        /// <returns></returns>
        public static bool DoneExchangeData(int roleID, ExchangeData ed)
        {
            bool ret = false;
            lock (ed)
            {
                if (!ed.DoneDict.ContainsKey(roleID))
                {
                    ed.DoneDict[roleID] = 1;
                    ret = true;
                }
            }

            return ret;
        }

        public static bool IsDoneExchangeData(int roleID, ExchangeData ed)
        {
            int done = 0;
            lock (ed)
            {
                ed.DoneDict.TryGetValue(roleID, out done);
            }

            return (done > 0);
        }

        /// <summary>
        /// Add đồ vào ô giao dịch
        /// </summary>
        /// <param name="client"></param>
        /// <param name="goodsDbID"></param>
        /// <param name="ed"></param>
        /// <returns></returns>
        public static bool AddGoodsDataIntoExchangeData(KPlayer client, int goodsDbID, ExchangeData ed)
        {
            //Check xem đã lock chưa
            if (IsLockExchangeData(client.RoleID, ed))
            {
                return true;
            }

            lock (ed)
            {
                List<GoodsData> goodsDataList = null;

                if (!ed.GoodsDict.TryGetValue(client.RoleID, out goodsDataList))
                {
                    goodsDataList = new List<GoodsData>();
                    ed.GoodsDict[client.RoleID] = goodsDataList;
                }

                // Nếu chưa kín 12 vật phẩm
                if (goodsDataList.Count < 10)
                {
                    GoodsData gd = client.GoodsData.Find(goodsDbID, 0);
                    if (null == gd)
                        return false;
                    if (gd.Binding > 0)
                        return false; //Nếu mà vật phẩm khóa thì cút
                    if (ItemManager.IsTimeLimitGoods(gd))
                        return false; //Nếu hết thời gian giao dịch thì cút
                    client.GoodsData.Remove(gd, false);

                    // Notfify về client move GOOD vào lưới
                    KT_TCPHandler.NotifyMoveGoods(client, gd, 0);

                    if (-1 == goodsDataList.IndexOf(gd))
                    {
                        goodsDataList.Add(gd);
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Thêm pet vào phiên giao dịch
        /// </summary>
        /// <param name="client"></param>
        /// <param name="PetIdDB"></param>
        /// <param name="ed"></param>
        /// <returns></returns>
        public static bool AddPetDataToSessionTrade(KPlayer client, int PetIdDB, ExchangeData ed)
        {
            /// Check xem đã lock chưa
            if (IsLockExchangeData(client.RoleID, ed))
            {
                return true;
            }

            /// Lock lại sessison giao dịch
            lock (ed)
            {
                /// Nếu trong phiên chưa tồn tại
                if (ed.PetDict == null)
                {
                    /// Tạo mới
                    ed.PetDict = new Dictionary<int, List<PetData>>();
                }

                /// Nếu session chưa có thằng này
                if (!ed.PetDict.ContainsKey(client.RoleID))
                {
                    /// Tạo mới
                    ed.PetDict[client.RoleID] = new List<PetData>();
                }

                /// Danh sách pet
                List<PetData> petList = ed.PetDict[client.RoleID];

                /// Nếu đã đặt quá số lượng
                if (petList.Count >= 1)
                {
                    KTPlayerManager.ShowNotification(client, "Mỗi lần chỉ có thể giao dịch tối đa 1 tinh linh!");
                    return false;
                }

                /// Tìm ra con PET của thằng nhân vật theo DBID
                PetData petData = client.PetList.Where(x => x.ID == PetIdDB).FirstOrDefault();
                /// Nếu không tồn tại
                if (petData == null)
                {
                    KTPlayerManager.ShowNotification(client, "Tinh linh không tồn tại!");
                    return false;
                }

                /// Nếu pet đang xuất chiến thì báo toác
                if (client.CurrentPet != null && client.CurrentPet.RoleID == petData.ID + (int)ObjectBaseID.Pet)
                {
                    KTPlayerManager.ShowNotification(client, "Tinh linh đang tham chiến không thể giao dịch!");
                    return false;
                }

                /// Nếu đã tồn tại trong phiên
                if (petList.Any(x => x.ID == petData.ID))
                {
                    KTPlayerManager.ShowNotification(client, "Tinh linh này đã được bỏ lên khung giao dịch!");
                    return false;
                }

                /// Dữ liệu ảo
                petData = KTPetManager.GetPetDataWithAttributes(petData, client);
                /// Thêm list pet này vào session giao dịch
                petList.Add(petData);
            }

            /// OK
            return true;
        }

        /// <summary>
        /// Xóa pet khỏi ô giao dịch
        /// </summary>
        /// <param name="client"></param>
        /// <param name="petDbID"></param>
        /// <param name="ed"></param>
        /// <returns></returns>
        public static bool RemovePetFromExchangeData(KPlayer client, int petDbID, ExchangeData ed)
        {
            if (IsLockExchangeData(client.RoleID, ed))
            {
                return true;
            }

            lock (ed)
            {
                /// Nếu tồn tại danh sách pet
                if (ed.PetDict.TryGetValue(client.RoleID, out List<PetData> petList))
                {
                    /// Xóa con pet khỏi list
                    petList.RemoveAll(x => x.ID == petDbID);
                }
                /// Đéo tồn tại
                else
                {
                    /// Thì toác
                    return false;
                }
            }

            /// OK
            return true;
        }

        private static string BuildTradeAnalysisLog(
            KPlayer from, KPlayer to,
            List<GoodsData> outGoods, List<GoodsData> inGoods,
            int outMoney, int inMoney,
            int outJinbi, int inJinbi)
        {
            Dictionary<int, int> outDict = new Dictionary<int, int>();
            Dictionary<int, int> inDict = new Dictionary<int, int>();

            for (int i = 0; outGoods != null && i < outGoods.Count; ++i)
            {
                if (!outDict.ContainsKey(outGoods[i].GoodsID))
                    outDict.Add(outGoods[i].GoodsID, outGoods[i].GCount);
                else
                    outDict[outGoods[i].GoodsID] += outGoods[i].GCount;
            }

            for (int i = 0; inGoods != null && i < inGoods.Count; ++i)
            {
                if (!inDict.ContainsKey(inGoods[i].GoodsID))
                    inDict.Add(inGoods[i].GoodsID, inGoods[i].GCount);
                else
                    inDict[inGoods[i].GoodsID] += inGoods[i].GCount;
            }

            if (outJinbi > 0)
                outDict[-100] = outJinbi;
            if (inJinbi > 0)
                inDict[-100] = inJinbi;
            if (outMoney > 0)
                outDict[-101] = outMoney;
            if (inMoney > 0)
                inDict[-101] = inMoney;

            StringBuilder inSb = new StringBuilder(), outSb = new StringBuilder();
            foreach (var kvp in inDict)
            {
                inSb.Append(kvp.Key).Append(':').Append(kvp.Value).Append(',');
            }
            if (inSb.Length > 0)
                inSb.Remove(inSb.Length - 1, 1);

            foreach (var kvp in outDict)
            {
                outSb.Append(kvp.Key).Append(':').Append(kvp.Value).Append(',');
            }
            if (outSb.Length > 0)
                outSb.Remove(outSb.Length - 1, 1);

            FriendData fd = KTPlayerManager.FindFriendData(from, to.RoleID);
            int isFriend = 0;
            if (fd != null && fd.FriendType == 0)
                isFriend = 100;

            string sip = KTGlobal.GetIPAddress(from);
            string tip = KTGlobal.GetIPAddress(to);

            string analysisLog = string.Format("server={0} source={1} srcPlayer={2} target={3} dstPlayer={4} in={5} out={6} map={7} sviplevel={8} tviplevel={9} sexp={10} texp={11} friendDegree={12}",
                GameManager.ServerId, from.strUserID, from.RoleID, to.strUserID, to.RoleID, inSb.ToString(), outSb.ToString(), from.MapCode,
                0, 0, sip, tip, isFriend);

            return analysisLog;
        }

        /// <summary>
        /// Hàm thực hiện giao dịch cho 2 thằng dựa trên dữ liệu phiên giao dịch
        /// </summary>
        /// <param name="client"></param>
        /// <param name="ed"></param>
        public static int CompleteExchangeData(KPlayer client, KPlayer otherClient, ExchangeData ed)
        {
            int ret = 0;
            // KHóa session giao dịch này lại
            lock (ed)
            {
                List<GoodsData> goodsDataList1 = null;

                if (ed.GoodsDict.TryGetValue(client.RoleID, out goodsDataList1))
                {
                    // Kiểm tra xem có thể lấy đồ của thằng đầu tiên add cho thằng thứu 2 không
                    if (!ItemManager.CanAddGoodsDataList(otherClient, goodsDataList1))
                    {
                        return -1;
                    }
                }

                List<GoodsData> goodsDataList2 = null;
                if (ed.GoodsDict.TryGetValue(otherClient.RoleID, out goodsDataList2))
                {
                    // Kiểm tra xem có thể lấy đồ của thằng thứ 2 add cho thằng thứ 1 không
                    if (!ItemManager.CanAddGoodsDataList(client, goodsDataList2))
                    {
                        return -11;
                    }
                }

                int moveMoney = 0;
                if (ed.MoneyDict.TryGetValue(client.RoleID, out moveMoney))
                {
                    moveMoney = Math.Max(moveMoney, 0);
                    if (moveMoney > client.Money)
                    {
                        return -2;
                    }
                }

                int moveMoney2 = 0;
                if (ed.MoneyDict.TryGetValue(otherClient.RoleID, out moveMoney2))
                {
                    moveMoney2 = Math.Max(moveMoney2, 0);
                    if (moveMoney2 > otherClient.Money)
                    {
                        return -12;
                    }
                }

                try
                {
                    string analysisLog = BuildTradeAnalysisLog(client, otherClient, goodsDataList1, goodsDataList2, 0, 0, moveMoney, moveMoney2);

                    analysisLog = "[TRADE] " + analysisLog;

                    LogManager.WriteLog(LogTypes.Trade, analysisLog);
                }
                catch { }

                for (int i = 0; goodsDataList1 != null && i < goodsDataList1.Count; i++)
                {
                    string result = "[Giao dịch thành công]";

                    // Lệnh vào DB để move đồ của thằng A sang thằng B
                    if (!KTTradeManager.MoveGoodsDataToOtherRole(Global._TCPManager.MySocketListener, Global._TCPManager.tcpClientPool, Global._TCPManager.TcpOutPacketPool,
                        goodsDataList1[i], client, otherClient, true, "TRADE", ed.ExchangeID))
                    {
                        GameManager.SystemServerEvents.AddEvent(string.Format("[TRADE] không thể chuyển vật phẩm, FromRole={0}({1}), ToRole={2}({3}), GoodsDbID={4}, GoodsID={5}, GoodsNum={6}",
                            client.RoleID, client.RoleName, otherClient.RoleID, otherClient.RoleName,
                            goodsDataList1[i].Id,
                            goodsDataList1[i].GoodsID,
                            goodsDataList1[i].GCount
                            ),
                            EventLevels.Important);

                        result = "[Thất bại]";
                    }
                }
                //X óa dict giao dịch của  thằng A
                ed.GoodsDict.Remove(client.RoleID);

                for (int i = 0; goodsDataList2 != null && i < goodsDataList2.Count; i++)
                {
                    // Lệnh vào DB để move ĐỒ từ thằng B sang thăng fA
                    string result = "[Giao dịch thành công]";
                    if (!KTTradeManager.MoveGoodsDataToOtherRole(Global._TCPManager.MySocketListener, Global._TCPManager.tcpClientPool, Global._TCPManager.TcpOutPacketPool,
                        goodsDataList2[i], otherClient, client))
                    {
                        GameManager.SystemServerEvents.AddEvent(string.Format("[TRADE] không thể chuyển vật phẩm, FromRole={0}({1}), ToRole={2}({3}), GoodsDbID={4}, GoodsID={5}, GoodsNum={6}",
                            otherClient.RoleID, otherClient.RoleName, client.RoleID, client.RoleName,
                            goodsDataList2[i].Id,
                            goodsDataList2[i].GoodsID,
                            goodsDataList2[i].GCount
                            ),
                            EventLevels.Important);

                        result = "[Thất bại]";
                    }
                }
                //Xóa dict giao dịch của thằng B
                ed.GoodsDict.Remove(otherClient.RoleID);

                if (moveMoney > 0)
                {
                    // Trừ bạc của thằng này
                    if (KTPlayerManager.SubMoney(client, moveMoney, "TRADE", true, ed.ExchangeID))
                    {
                        /// Bạc sau khi trừ thuế 10%
                        double nMoney = moveMoney * 0.9;

                        double MoneyForHostCity = moveMoney * 0.03;
                        if (MoneyForHostCity > 0)
                        {
                            GuildManager.getInstance().AddBoundMoneyForHostCity((int)MoneyForHostCity);
                        }
                        // Add bạc cho thằng kia
                        KTPlayerManager.AddMoney(otherClient, (int)nMoney, "TRADE", true, true, ed.ExchangeID);

                        ed.MoneyDict.Remove(client.RoleID);
                    }
                }

                if (moveMoney2 > 0)
                {  // Trừ bạc của thằng này
                    if (KTPlayerManager.SubMoney(otherClient, moveMoney2, "TRADE", true, ed.ExchangeID))
                    {
                        /// Bạc sau khi trừ thuế 10%
                        double nMoney = moveMoney2 * 0.9;
                        double MoneyForHostCity = moveMoney * 0.03;
                        if (MoneyForHostCity > 0)
                        {
                            GuildManager.getInstance().AddBoundMoneyForHostCity((int)MoneyForHostCity);
                        }
                        // Add bạc cho thằng kia
                        KTPlayerManager.AddMoney(client, (int)nMoney, "TRADE", true, true, ed.ExchangeID);

                        ed.MoneyDict.Remove(otherClient.RoleID);
                    }
                }

                // Lấy ra toàn bộ pet của thằng thứ 1 bỏ lên khung giao dịch
                List<PetData> TotalPetDict1 = null;
                if (ed.PetDict != null && ed.PetDict.TryGetValue(client.RoleID, out TotalPetDict1))
                {
                }

                List<PetData> TotalPetDict2 = null;
                if (ed.PetDict != null && ed.PetDict.TryGetValue(otherClient.RoleID, out TotalPetDict2))
                {
                }

                if (TotalPetDict1 != null)
                {
                    // Duyệt tất cả pet của thằng đầu tiên
                    for (int i = 0; TotalPetDict1 != null && i < TotalPetDict1.Count; i++)
                    {
                        string result = "[Giao dịch thành công]";

                        // Chuyển PET từ thằng A sang thằng B
                        if (!KTTradeManager.MovePetDataToOtherRole(Global._TCPManager.MySocketListener, Global._TCPManager.tcpClientPool, Global._TCPManager.TcpOutPacketPool,
                            TotalPetDict1[i].ID, client, otherClient, true, "TRADE", ed.ExchangeID))
                        {
                            GameManager.SystemServerEvents.AddEvent(string.Format("[TRADE] Không thể chuyển PET từ role ID A sang B, FromRole={0}({1}), ToRole={2}({3}), PETDBID={4}",
                              client.RoleID, client.RoleName, otherClient.RoleID, otherClient.RoleName,
                              TotalPetDict1[i]
                              ),
                              EventLevels.Important);
                            result = "[Thất bại]";
                        }
                    }
                    ed.PetDict.Remove(client.RoleID);
                }

                if (TotalPetDict2 != null)
                {
                    // Lấy toàn bộ pet của thằng 2
                    for (int i = 0; TotalPetDict2 != null && i < TotalPetDict2.Count; i++)
                    {
                        string result = "[Giao dịch thành công]";

                        // Chuyển PET từ thằng A sang thằng B
                        if (!KTTradeManager.MovePetDataToOtherRole(Global._TCPManager.MySocketListener, Global._TCPManager.tcpClientPool, Global._TCPManager.TcpOutPacketPool,
                            TotalPetDict2[i].ID, otherClient, client, true, "TRADE", ed.ExchangeID))
                        {
                            GameManager.SystemServerEvents.AddEvent(string.Format("[TRADE] Không thể chuyển PET từ role ID B sang A, FromRole={0}({1}), ToRole={2}({3}), PETDBID={4}",
                              otherClient.RoleID, otherClient.RoleName, client.RoleID, client.RoleName,
                              TotalPetDict1[i]
                              ),
                              EventLevels.Important);
                            result = "[Thất bại]";
                        }
                    }
                    ed.PetDict.Remove(otherClient.RoleID);
                }
            }

            return ret;
        }

        /// <summary>
        /// Hàm chuyển vật phẩm từ thằng này cho thằng khác
        /// </summary>
        /// <param name="sl"></param>
        /// <param name="tcpClientPool"></param>
        /// <param name="pool"></param>
        /// <param name="gd"></param>
        /// <param name="toClient"></param>
        /// <param name="bAddToTarget">Có thưc hiện update lại vị trí cho thằng nhận không, Nếu là add vào túi đồ thì cần,Nếu add vào kho thì kệ mẹ nó</param>
        /// <returns></returns>
        public static bool MoveGoodsDataToOtherRole(SocketListener sl, TCPClientPool tcpClientPool, TCPOutPacketPool pool, GoodsData gd, KPlayer fromClient, KPlayer toClient, bool bAddToTarget = true, string FROM = "", int SesionI = -1)
        {
            //Thao tác với DB SV thực hiện xáo vật phẩm của thằng đầu tiên và move cho thằng thứ 2

            string[] dbFields = null;
            string strcmd = string.Format("{0}:{1}:{2}:{3}", toClient.RoleID, fromClient.RoleID, gd.Id, 0);
            TCPProcessCmdResults dbRequestResult = Global.RequestToDBServer(tcpClientPool, pool, (int)TCPGameServerCmds.CMD_DB_MOVEGOODS_CMD, strcmd, out dbFields, GameManager.LocalServerId);
            if (dbRequestResult == TCPProcessCmdResults.RESULT_FAILED)
            {
                return false;
            }

            if (dbFields.Length < 4 || Convert.ToInt32(dbFields[3]) < 0)
            {
                return false;
            }

            {
                int RoleID = fromClient.RoleID;
                string AccountName = fromClient.strUserID;
                string RecoreType = FROM;

                string RecoreDesc = "Delete item [" + gd.GoodsID + "][" + gd.Id + "] from [" + fromClient.RoleID + "] move to [" + toClient.RoleID + "]";

                string Source = fromClient.RoleID + "";

                string Taget = toClient.RoleID + "";

                string OptType = "SUB";

                int ZONEID = fromClient.ZoneID;

                string OtherPram_1 = gd.Id + "";
                string OtherPram_2 = gd.GoodsID + "";
                string OtherPram_3 = gd.GCount + "";
                string OtherPram_4 = SesionI + "";

                GameManager.logDBCmdMgr.WriterLogs(RoleID, AccountName, RecoreType, RecoreDesc, Source, Taget, OptType, ZONEID, OtherPram_1, OtherPram_2, OtherPram_3, OtherPram_4, fromClient.ServerId);
            }

            if (bAddToTarget)
            {
                //Update lại vị trí đồ cho thằng thứ 2 nhận đồ
                string[] dbFields2 = null;

                gd.BagIndex = toClient.GoodsData.GetBagFirstEmptyPosition(0);

                gd.Site = 0;

                Dictionary<UPDATEITEM, object> TotalUpdate = new Dictionary<UPDATEITEM, object>();

                TotalUpdate.Add(UPDATEITEM.ROLEID, toClient.RoleID);
                TotalUpdate.Add(UPDATEITEM.ITEMDBID, gd.Id);
                TotalUpdate.Add(UPDATEITEM.SITE, gd.Site);
                TotalUpdate.Add(UPDATEITEM.BAGINDEX, gd.BagIndex);

                string ScriptUpdateBuild = ItemManager.ItemUpdateScriptBuild(TotalUpdate);

                dbRequestResult = Global.RequestToDBServer(tcpClientPool, pool, (int)TCPGameServerCmds.CMD_DB_UPDATEGOODS_CMD, ScriptUpdateBuild, out dbFields, GameManager.LocalServerId);

                if (dbRequestResult == TCPProcessCmdResults.RESULT_FAILED)
                {
                    return false;
                }
                else if (dbRequestResult == TCPProcessCmdResults.RESULT_DATA)
                {
                    toClient.GoodsData.Add(gd);

                    {
                        int RoleID = toClient.RoleID;
                        string AccountName = toClient.strUserID;
                        string RecoreType = FROM;

                        string RecoreDesc = "Revice Item [" + gd.Id + "] from [" + fromClient.RoleID + "]";

                        string Source = fromClient.RoleID + "";

                        string Taget = toClient.RoleID + "";

                        string OptType = "ADD";

                        int ZONEID = fromClient.ZoneID;

                        string OtherPram_1 = gd.Id + "";
                        string OtherPram_2 = gd.GoodsID + "";
                        string OtherPram_3 = gd.GCount + "";
                        string OtherPram_4 = SesionI + "";

                        GameManager.logDBCmdMgr.WriterLogs(RoleID, AccountName, RecoreType, RecoreDesc, Source, Taget, OptType, ZONEID, OtherPram_1, OtherPram_2, OtherPram_3, OtherPram_4, fromClient.ServerId);
                    }

                    // Thực hiện task mua cái gì đó
                    ProcessTask.ProseccTaskBeforeDoTask(Global._TCPManager.MySocketListener, Global._TCPManager.TcpOutPacketPool, toClient);

                    // NOTIFY ADD GOOLDS VỀ CLIENT
                    KT_TCPHandler.NotifySelfAddGoods(toClient, gd.Id, gd.GoodsID, gd.Forge_level, gd.GCount, gd.Binding, gd.Site, 1, gd.Endtime, gd.Strong, gd.BagIndex, gd.Using, gd.Props, gd.Series, gd.OtherParams);

                    return true;
                }
            }

            return true;
        }

        /// <summary>
        /// Chuyển đồ từ thằng A sang thằng B khi thằng A OFFLINE
        /// Phục vụ cho việc mua sạp hàng offfline
        /// </summary>
        /// <param name="sl"></param>
        /// <param name="tcpClientPool"></param>
        /// <param name="pool"></param>
        /// <param name="gd"></param>
        /// <param name="fromClientID"></param>
        /// <param name="toClient"></param>
        /// <param name="bAddToTarget"></param>
        /// <param name="FROM"></param>
        /// <param name="SesionI"></param>
        /// <returns></returns>
        public static bool MoveGoodsDataToOtherRoleOffline(SocketListener sl, TCPClientPool tcpClientPool, TCPOutPacketPool pool, GoodsData gd, int fromClientID, KPlayer toClient, bool bAddToTarget = true, string FROM = "", int SesionI = -1)
        {
            //Thao tác với DB SV thực hiện xáo vật phẩm của thằng đầu tiên và move cho thằng thứ 2

            string[] dbFields = null;
            string strcmd = string.Format("{0}:{1}:{2}:{3}", toClient.RoleID, fromClientID, gd.Id, 0);
            TCPProcessCmdResults dbRequestResult = Global.RequestToDBServer(tcpClientPool, pool, (int)TCPGameServerCmds.CMD_DB_MOVEGOODS_CMD, strcmd, out dbFields, GameManager.LocalServerId);
            if (dbRequestResult == TCPProcessCmdResults.RESULT_FAILED)
            {
                return false;
            }

            if (dbFields.Length < 4 || Convert.ToInt32(dbFields[3]) < 0)
            {
                return false;
            }

            {
                int RoleID = fromClientID;
                string AccountName = "STACK";
                string RecoreType = FROM;

                string RecoreDesc = "Delete item [" + gd.GoodsID + "][" + gd.Id + "] from [" + fromClientID + "] move to [" + toClient.RoleID + "]";

                string Source = fromClientID + "";

                string Taget = toClient.RoleID + "";

                string OptType = "SUB";

                int ZONEID = GameManager.LocalServerId;

                string OtherPram_1 = gd.Id + "";
                string OtherPram_2 = gd.GoodsID + "";
                string OtherPram_3 = gd.GCount + "";
                string OtherPram_4 = SesionI + "";

                GameManager.logDBCmdMgr.WriterLogs(RoleID, AccountName, RecoreType, RecoreDesc, Source, Taget, OptType, ZONEID, OtherPram_1, OtherPram_2, OtherPram_3, OtherPram_4, GameManager.LocalServerId);
            }

            if (bAddToTarget)
            {
                //Update lại vị trí đồ cho thằng thứ 2 nhận đồ
                string[] dbFields2 = null;
                gd.BagIndex = toClient.GoodsData.GetBagFirstEmptyPosition(0);
                //SEt lại cho nó là túi đồ trên người
                gd.Site = 0;
                Dictionary<UPDATEITEM, object> TotalUpdate = new Dictionary<UPDATEITEM, object>();

                TotalUpdate.Add(UPDATEITEM.ROLEID, toClient.RoleID);
                TotalUpdate.Add(UPDATEITEM.ITEMDBID, gd.Id);
                TotalUpdate.Add(UPDATEITEM.SITE, 0);
                TotalUpdate.Add(UPDATEITEM.BAGINDEX, gd.BagIndex);

                string ScriptUpdateBuild = ItemManager.ItemUpdateScriptBuild(TotalUpdate);

                dbRequestResult = Global.RequestToDBServer(tcpClientPool, pool, (int)TCPGameServerCmds.CMD_DB_UPDATEGOODS_CMD, ScriptUpdateBuild, out dbFields, GameManager.LocalServerId);

                if (dbRequestResult == TCPProcessCmdResults.RESULT_FAILED)
                {
                    return false;
                }
                else if (dbRequestResult == TCPProcessCmdResults.RESULT_DATA)
                {
                    toClient.GoodsData.Add(gd);

                    {
                        int RoleID = toClient.RoleID;
                        string AccountName = toClient.strUserID;
                        string RecoreType = FROM;

                        string RecoreDesc = "Received Item [" + gd.Id + "] from [" + fromClientID + "]";

                        string Source = fromClientID + "";

                        string Taget = toClient.RoleID + "";

                        string OptType = "ADD";

                        int ZONEID = GameManager.LocalServerId;

                        string OtherPram_1 = gd.Id + "";
                        string OtherPram_2 = gd.GoodsID + "";
                        string OtherPram_3 = gd.GCount + "";
                        string OtherPram_4 = SesionI + "";

                        GameManager.logDBCmdMgr.WriterLogs(RoleID, AccountName, RecoreType, RecoreDesc, Source, Taget, OptType, ZONEID, OtherPram_1, OtherPram_2, OtherPram_3, OtherPram_4, GameManager.LocalServerId);
                    }

                    // Thực hiện task mua cái gì đó
                    ProcessTask.ProseccTaskBeforeDoTask(Global._TCPManager.MySocketListener, Global._TCPManager.TcpOutPacketPool, toClient);

                    // NOTIFY ADD GOOLDS VỀ CLIENT
                    KT_TCPHandler.NotifySelfAddGoods(toClient, gd.Id, gd.GoodsID, gd.Forge_level, gd.GCount, gd.Binding, gd.Site, 1, gd.Endtime, gd.Strong, gd.BagIndex, gd.Using, gd.Props, gd.Series, gd.OtherParams);

                    return true;
                }
            }

            return true;
        }

        /// <summary>
        /// Nhận vật phẩm từ thằng khác
        /// </summary>
        /// <param name="sl"></param>
        /// <param name="tcpClientPool"></param>
        /// <param name="pool"></param>
        /// <param name="gd"></param>
        /// <param name="fromClientID"></param>
        /// <param name="toClient"></param>
        /// <param name="bAddToTarget"></param>
        /// <param name="FROM"></param>
        /// <param name="SesionI"></param>
        /// <returns></returns>
        public static bool TakeItemFromOther(SocketListener sl, TCPClientPool tcpClientPool, TCPOutPacketPool pool, GoodsData gd, int fromClientID, KPlayer toClient, bool bAddToTarget = true, string FROM = "", int SesionI = -1)
        {
            //Thao tác với DB SV thực hiện xáo vật phẩm của thằng đầu tiên và move cho thằng thứ 2

            string[] dbFields = null;
            string strcmd = string.Format("{0}:{1}:{2}:{3}", toClient.RoleID, fromClientID, gd.Id, 1);
            TCPProcessCmdResults dbRequestResult = Global.RequestToDBServer(tcpClientPool, pool, (int)TCPGameServerCmds.CMD_DB_MOVEGOODS_CMD, strcmd, out dbFields, GameManager.LocalServerId);
            if (dbRequestResult == TCPProcessCmdResults.RESULT_FAILED)
            {
                return false;
            }

            if (dbFields.Length < 4 || Convert.ToInt32(dbFields[3]) < 0)
            {
                return false;
            }

            //{
            //    int RoleID = fromClientID;
            //    string AccountName = "STACK";
            //    string RecoreType = FROM;

            //    string RecoreDesc = "Delete item [" + gd.GoodsID + "][" + gd.Id + "] from [" + fromClientID + "] move to [" + toClient.RoleID + "]";

            //    string Source = fromClientID + "";

            //    string Taget = toClient.RoleID + "";

            //    string OptType = "SUB";

            //    int ZONEID = GameManager.LocalServerId;

            //    string OtherPram_1 = gd.Id + "";
            //    string OtherPram_2 = gd.GoodsID + "";
            //    string OtherPram_3 = gd.GCount + "";
            //    string OtherPram_4 = SesionI + "";

            //    GameManager.logDBCmdMgr.WriterLogs(RoleID, AccountName, RecoreType, RecoreDesc, Source, Taget, OptType, ZONEID, OtherPram_1, OtherPram_2, OtherPram_3, OtherPram_4, GameManager.LocalServerId);
            //}

            if (bAddToTarget)
            {
                //Update lại vị trí đồ cho thằng thứ 2 nhận đồ
                string[] dbFields2 = null;
                gd.BagIndex = toClient.GoodsData.GetBagFirstEmptyPosition(0);

                Dictionary<UPDATEITEM, object> TotalUpdate = new Dictionary<UPDATEITEM, object>();

                TotalUpdate.Add(UPDATEITEM.ROLEID, toClient.RoleID);
                TotalUpdate.Add(UPDATEITEM.ITEMDBID, gd.Id);

                TotalUpdate.Add(UPDATEITEM.BAGINDEX, gd.BagIndex);

                string ScriptUpdateBuild = ItemManager.ItemUpdateScriptBuild(TotalUpdate);

                dbRequestResult = Global.RequestToDBServer(tcpClientPool, pool, (int)TCPGameServerCmds.CMD_DB_UPDATEGOODS_CMD, ScriptUpdateBuild, out dbFields, GameManager.LocalServerId);

                if (dbRequestResult == TCPProcessCmdResults.RESULT_FAILED)
                {
                    return false;
                }
                else if (dbRequestResult == TCPProcessCmdResults.RESULT_DATA)
                {
                    toClient.GoodsData.Add(gd);

                    {
                        int RoleID = toClient.RoleID;
                        string AccountName = toClient.strUserID;
                        string RecoreType = FROM;

                        string RecoreDesc = "Received Item from DROP [" + gd.Id + "] from [" + fromClientID + "]";

                        string Source = fromClientID + "";

                        string Taget = toClient.RoleID + "";

                        string OptType = "ADD";

                        int ZONEID = GameManager.LocalServerId;

                        string OtherPram_1 = gd.Id + "";
                        string OtherPram_2 = gd.GoodsID + "";
                        string OtherPram_3 = gd.GCount + "";
                        string OtherPram_4 = SesionI + "";

                        GameManager.logDBCmdMgr.WriterLogs(RoleID, AccountName, RecoreType, RecoreDesc, Source, Taget, OptType, ZONEID, OtherPram_1, OtherPram_2, OtherPram_3, OtherPram_4, GameManager.LocalServerId);
                    }

                    // Thực hiện task mua cái gì đó
                    ProcessTask.ProseccTaskBeforeDoTask(Global._TCPManager.MySocketListener, Global._TCPManager.TcpOutPacketPool, toClient);

                    // NOTIFY ADD GOOLDS VỀ CLIENT
                    KT_TCPHandler.NotifySelfAddGoods(toClient, gd.Id, gd.GoodsID, gd.Forge_level, gd.GCount, gd.Binding, gd.Site, 1, gd.Endtime, gd.Strong, gd.BagIndex, gd.Using, gd.Props, gd.Series, gd.OtherParams);

                    return true;
                }
            }

            return true;
        }

        /// <summary>
        /// Chuyển pet sang thằng khác
        /// </summary>
        /// <param name="sl"></param>
        /// <param name="tcpClientPool"></param>
        /// <param name="pool"></param>
        /// <param name="PetDBID"></param>
        /// <param name="fromClient"></param>
        /// <param name="toClient"></param>
        /// <param name="bAddToTarget"></param>
        /// <param name="FROM"></param>
        /// <param name="SesionI"></param>
        /// <returns></returns>
        public static bool MovePetDataToOtherRole(SocketListener sl, TCPClientPool tcpClientPool, TCPOutPacketPool pool, int PetDBID, KPlayer fromClient, KPlayer toClient, bool bAddToTarget = true, string FROM = "", int SesionI = -1)
        {
            //Thao tác với DB SV thực hiện xáo vật phẩm của thằng đầu tiên và move cho thằng thứ 2

            string[] dbFields = null;
            string strcmd = string.Format("{0}:{1}:{2}", toClient.RoleID, fromClient.RoleID, PetDBID);
            TCPProcessCmdResults dbRequestResult = Global.RequestToDBServer(tcpClientPool, pool, (int)TCPGameServerCmds.CMD_KT_DB_PET_UPDATEROLEID, strcmd, out dbFields, GameManager.LocalServerId);
            if (dbRequestResult == TCPProcessCmdResults.RESULT_FAILED)
            {
                return false;
            }

            if (dbFields.Length < 2 || Convert.ToInt32(dbFields[0]) < 0)
            {
                return false;
            }

            // Nếu như là mút thì  ghi logs
            {
                int RoleID = fromClient.RoleID;
                string AccountName = fromClient.strUserID;
                string RecoreType = FROM;

                string RecoreDesc = "Remove PET [" + PetDBID + "] from [" + fromClient.RoleID + "] move to [" + toClient.RoleID + "]";

                string Source = fromClient.RoleID + "";

                string Taget = toClient.RoleID + "";

                string OptType = "SUB";

                int ZONEID = fromClient.ZoneID;

                string OtherPram_1 = PetDBID + "";
                string OtherPram_2 = PetDBID + "";
                string OtherPram_3 = PetDBID + "";
                string OtherPram_4 = SesionI + "";

                GameManager.logDBCmdMgr.WriterLogs(RoleID, AccountName, RecoreType, RecoreDesc, Source, Taget, OptType, ZONEID, OtherPram_1, OtherPram_2, OtherPram_3, OtherPram_4, fromClient.ServerId);
            }

            if (bAddToTarget)
            {
                lock (fromClient.PetList)
                {
                    var FindSource = fromClient.PetList?.Where(x => x.ID == PetDBID).FirstOrDefault();
                    if (FindSource != null)
                    {
                        /// Set lại ID chủ nhân cho con PET
                        FindSource.RoleID = toClient.RoleID;

                        /// Xóa con pet này khỏi thằng thứ nhất
                        fromClient.PetList?.Remove(FindSource);

                        /// Nếu thằng kia chưa có pet
                        if (toClient.PetList == null)
                        {
                            /// Tạo mới
                            toClient.PetList = new List<PetData>();
                        }
                        /// Thực hiện add con pet này cho thằng mua
                        toClient.PetList.Add(FindSource);

                        /// Gửi gói tin cập nhật danh sách pet cho cả 2 thằng
                        KT_TCPHandler.SendUpdatePetList(fromClient);
                        KT_TCPHandler.SendUpdatePetList(toClient);
                    }
                }
                return true;
            }

            return true;
        }
    }
}