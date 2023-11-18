using GameServer.Core.Executor;
using GameServer.KiemThe.Core.Item;
using GameServer.KiemThe.Core.Task;
using GameServer.KiemThe.Logic;
using GameServer.Logic;
using GameServer.Server;
using Server.Tools;
using System;
using System.Collections.Generic;

namespace GameServer.KiemThe.Core.Shop
{
    /// <summary>
    /// Thực thể quản lý điểm uy danh
    /// </summary>
    public class ShopSalePrestige
    {
        public static void ClickNpcBuyRequest(NPC npc, KPlayer client)
        {
            KNPCDialog _NpcDialog = new KNPCDialog();

            string Text = "";

            long Now = TimeUtil.NOW();

            if (Now - client.LastRequestKTCoin > 10000)
            {
                //Lấy ra rank hiện tại của người
                int Rank = ShopSalePrestige.GetRankOfPlayerFromDatabase(client);

                int PERCENT_GROUP1 = 0;
                int PERCENT_GROUP2 = 0;

                string Title = "";
                if (Rank == -1)
                {
                    Title = "Hệ thống bảng xếp hạng đang cập nhật\nVui lòng quay lại sau";
                }
                else if (Rank == -100)
                {
                    Title = "Bạn không nằm trong TOP 100 người uy danh,Bạn không thể mua tinh hoạt lực ưu đãi";
                }
                else
                {
                    if (Rank >= 0 && Rank < 30)
                    {
                        PERCENT_GROUP1 = 80;
                        PERCENT_GROUP2 = 30;
                        Title = "<b><color=#00ff2a>Bạn đang xếp hạng thứ <color=red>" + Rank + "</color>\nBạn được mua Tinh Hoạt lực giảm giá " + PERCENT_GROUP1 + "% tối đa <color=red>5</color> bình\nThỏi vàng giảm giá " + PERCENT_GROUP2 + "% tối đa <color=red>1</color> thỏi</color></b>";
                    }
                    else if (Rank >= 30 && Rank < 60)
                    {
                        PERCENT_GROUP1 = 40;
                        PERCENT_GROUP2 = 20;
                        Title = "<b><color=#00ff2a>Bạn đang xếp hạng thứ <color=red>" + Rank + "</color>\nBạn được mua Tinh Hoạt lực giảm giá " + PERCENT_GROUP1 + "% tối đa <color=red>5</color> bình\nThỏi vàng giảm giá " + PERCENT_GROUP2 + "% tối đa <color=red>1</color> thỏi</color></b>";
                    }
                    else if (Rank >= 60 && Rank < 100)
                    {
                        PERCENT_GROUP1 = 20;
                        PERCENT_GROUP2 = 10;
                        Title = "<b><color=#00ff2a>Bạn đang xếp hạng thứ <color=red>" + Rank + "</color>\nBạn được mua Tinh Hoạt lực giảm giá " + PERCENT_GROUP1 + "% tối đa <color=red>5</color> bình\nThỏi vàng giảm giá " + PERCENT_GROUP2 + "% tối đa <color=red>1</color> thỏi</color></b>";
                    }
                    else
                    {
                        PERCENT_GROUP1 = 0;
                        PERCENT_GROUP2 = 0;
                        Title = "<b><color=#00ff2a>Bạn không nằm trong TOP 100 người được mua tinh hoạt lực ưu đãi</color></b>";
                    }
                }
                // Set số lần gần đây nhất mới thao tác
                client.LastRequestKTCoin = Now;

                Dictionary<int, string> Selections = new Dictionary<int, string>();

                Text = "Xin chào <b>" + client.RoleName + "</b>" + " bạn hiện có  <b>" + KTGlobal.CreateStringByColor(client.Prestige + " Uy Danh", ColorType.Yellow) + "</b>\n\nTop 100 người chơi có uy danh cao nhất sẽ được mua tinh hoạt lực giảm giá\n\n" + Title;

                if (PERCENT_GROUP1 > 0)
                {
                    int OLDPRINT_1 = 40;

                    int NEWPRINT_1 = (OLDPRINT_1 * (100 - PERCENT_GROUP1)) / 100;

                    Selections.Add((int)DailyRecord.BuyDiscount_1, "<b><color=#00ff2a>Mua Tinh Lực (Tiểu)</color></b><b> " + NEWPRINT_1 + " Đồng(giảm " + PERCENT_GROUP1 + "%)</b>");
                    Selections.Add((int)DailyRecord.BuyDiscount_2, "<b><color=#00ff2a>Mua Hoạt Lực (Tiểu)</color></b><b> " + NEWPRINT_1 + " Đồng(giảm " + PERCENT_GROUP1 + "%)</b>");

                    int OLDPRINT_2 = 1000;

                    int NEWPRINT_2 = (OLDPRINT_2 * (100 - PERCENT_GROUP2)) / 100;

                    Selections.Add((int)DailyRecord.BuyDiscount_3, "<b><color=#00ff2a>Thỏi vàng</color></b><b> " + NEWPRINT_2 + " Đồng(giảm " + PERCENT_GROUP2 + "%)</b>");
                }

                Action<TaskCallBack> ActionWork = (x) => ClickBuyCheck(client, npc, x, Rank);

                _NpcDialog.OnSelect = ActionWork;

                _NpcDialog.Selections = Selections;
            }
            else
            {
                long DIV = Now - client.LastRequestKTCoin;

                float TimeLess = (10000 - DIV) / 1000;

                Text = "Vui lòng quay sau :" + TimeLess + " giây nữa";
            }

            _NpcDialog.Text = Text;

            _NpcDialog.Show(npc, client);
        }

        private static void ClickBuyCheck(KPlayer client, NPC npc, TaskCallBack x, int Rank)
        {
            KT_TCPHandler.CloseDialog(client);

            int PERCENT_GROUP1 = 0;
            int PERCENT_GROUP2 = 0;

            if (Rank >= 0 && Rank < 30)
            {
                PERCENT_GROUP1 = 80;
                PERCENT_GROUP2 = 30;
            }
            else if (Rank >= 30 && Rank < 60)
            {
                PERCENT_GROUP1 = 40;
                PERCENT_GROUP2 = 20;
            }
            else if (Rank >= 60 && Rank < 100)
            {
                PERCENT_GROUP1 = 20;
                PERCENT_GROUP2 = 10;
            }
            else
            {
                PERCENT_GROUP1 = 0;
            }

            if (x.SelectID == (int)DailyRecord.BuyDiscount_1 || x.SelectID == (int)DailyRecord.BuyDiscount_2)
            {
                int ITEMID = 0;
                int OLDPRINT_1 = 40;
                int NEWPRINT_1 = (OLDPRINT_1 * (100 - PERCENT_GROUP1)) / 100;

                bool CanBuy = false;

                int Recore = 0;

                if (x.SelectID == (int)DailyRecord.BuyDiscount_1)
                {
                    Recore = client.GetValueOfDailyRecore((int)DailyRecord.BuyDiscount_1);
                    if (Recore < 4)
                    {
                        CanBuy = true;
                    }
                    ITEMID = 348;
                }
                else if (x.SelectID == (int)DailyRecord.BuyDiscount_2)
                {
                    Recore = client.GetValueOfDailyRecore((int)DailyRecord.BuyDiscount_2);
                    if (Recore < 4)
                    {
                        CanBuy = true;
                    }
                    ITEMID = 351;
                }

                if (!KTGlobal.IsHaveMoney(client, NEWPRINT_1, Entities.MoneyType.Dong))
                {
                    KTPlayerManager.ShowNotification(client, "Tiền không đủ");
                }
                else
                {
                    if (CanBuy)
                    {
                        client.SetValueOfDailyRecore(x.SelectID, Recore + 1);

                        if (KTGlobal.SubMoney(client, NEWPRINT_1, Entities.MoneyType.Dong, "DISCOUNTSHOP").IsOK)
                        {
                            string TimeUsing = ItemManager.ConstGoodsEndTime;
                            // Tọa vật phẩm đã mua
                            if (!ItemManager.CreateItem(Global._TCPManager.TcpOutPacketPool, client, ITEMID, 1, 0, "DISCOUNTSHOP", true, 1, false, TimeUsing, "", -1, "", 0, 1, false))
                            {
                                KTPlayerManager.ShowNotification(client, "Có lỗi khi thực hiện thêm vật phẩm vào túi đồ vui lòng liên hệ ADM để được giúp đỡ!");

                                LogManager.WriteLog(LogTypes.BuyNpc, "Có lỗi khi thực hiện add vật phẩm :" + client.RoleID + "|" + ITEMID + " x " + 1);
                            }
                        }
                    }
                    else
                    {
                        KTPlayerManager.ShowNotification(client, "Hôm nay bạn đã hết lượt mua");
                    }
                }
            }
            else if (x.SelectID == (int)DailyRecord.BuyDiscount_3)
            {
                int ITEMID = 0;
                int OLDPRINT_1 = 1000;

                int NEWPRINT_1 = (OLDPRINT_1 * (100 - PERCENT_GROUP2)) / 100;

                bool CanBuy = false;

                int Recore = 0;

                if (x.SelectID == (int)DailyRecord.BuyDiscount_3)
                {
                    Recore = client.GetValueOfDailyRecore((int)DailyRecord.BuyDiscount_3);

                    if (Recore < 0)
                    {
                        CanBuy = true;
                    }
                    ITEMID = 402;
                }


                if (!KTGlobal.IsHaveMoney(client, NEWPRINT_1, Entities.MoneyType.Dong))
                {
                    KTPlayerManager.ShowNotification(client, "Tiền không đủ");
                }
                else
                {
                    if (CanBuy)
                    {
                        client.SetValueOfDailyRecore(x.SelectID, Recore + 1);

                        if (KTGlobal.SubMoney(client, NEWPRINT_1, Entities.MoneyType.Dong, "DISCOUNTSHOP").IsOK)
                        {
                            string TimeUsing = ItemManager.ConstGoodsEndTime;
                            // Tọa vật phẩm đã mua
                            if (!ItemManager.CreateItem(Global._TCPManager.TcpOutPacketPool, client, ITEMID, 1, 0, "DISCOUNTSHOP", true, 1, false, TimeUsing, "", -1, "", 0, 1, false))
                            {
                                KTPlayerManager.ShowNotification(client, "Có lỗi khi thực hiện thêm vật phẩm vào túi đồ vui lòng liên hệ ADM để được giúp đỡ!");

                                LogManager.WriteLog(LogTypes.BuyNpc, "Có lỗi khi thực hiện add vật phẩm :" + client.RoleID + "|" + ITEMID + " x " + 1);
                            }
                        }
                    }
                    else
                    {
                        KTPlayerManager.ShowNotification(client, "Hôm nay bạn đã hết lượt mua");
                    }
                }
            }
        }

        /// <summary>
        /// Lấy ra ranking tương ứng của người chơi
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        public static int GetRankOfPlayerFromDatabase(KPlayer client)
        {
            int RankType = (int)RankMode.UyDanh;

            int RoleID = client.RoleID;

            string CMDBUILD = RankType + ":" + RoleID;

            string[] dbFields = Global.ExecuteDBCmd((int)TCPGameServerCmds.CMD_KT_RANKING_CHECKING, CMDBUILD, GameManager.ServerId);
            if (null == dbFields)
            {
                return -100;
            }
            if (dbFields.Length != 2)
            {
                return -100;
            }

            int RANKRETURN = Convert.ToInt32(dbFields[1]);

            return RANKRETURN;
        }
    }
}