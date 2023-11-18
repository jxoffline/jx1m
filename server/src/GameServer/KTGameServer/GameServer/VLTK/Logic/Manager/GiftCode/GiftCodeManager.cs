using GameServer.Core.Executor;
using GameServer.KiemThe.Logic;
using GameServer.Logic;
using Server.Data;
using Server.Tools;
using System;
using System.Net;

namespace GameServer.KiemThe.Core.Item
{
    public class GiftCodeManager
    {
        /// <summary>
        /// Hàm xử lý kích hoạt giftcode
        /// Mở 1 luồng mới để kích hoạt giftcode
        /// Vì webrequest phụ thuộc vào webserver để response nên không thể bắt luồng hệ thống đợi theo sẽ gây tắc ngẽn tầng TCP
        /// </summary>
        /// <param name="player"></param>
        /// <param name="CodeAcive"></param>
        public static void ActiveGiftCode(KPlayer player, string CodeAcive)
        {
            long Now = KTGlobal.GetCurrentTimeMilis();

            if (Now - player.LastActiveGiftCodeTicks > 10000)
            {
                player.LastActiveGiftCodeTicks = Now;

                if (!KTGlobal.IsHaveSpace(6, player))
                {
                    KTPlayerManager.ShowMessageBox(player, "Thông báo", "Không đủ chỗ trống để nhận thưởng");
                    return;
                }

                GiftCodeRequest ActiveModel = new GiftCodeRequest();
                ActiveModel.CodeActive = CodeAcive;
                ActiveModel.RoleActive = player.RoleID;
                ActiveModel.ServerID = player.ZoneID;

                byte[] SendDATA = DataHelper.ObjectToBytes<GiftCodeRequest>(ActiveModel);

                WebClient wwc = new WebClient();

                try
                {
                    byte[] VL = wwc.UploadData(GameManager.ActiveGiftCodeUrl, SendDATA);

                    GiftCodeRep GiftCodeRequest = DataHelper.BytesToObject<GiftCodeRep>(VL, 0, VL.Length);

                    if (GiftCodeRequest.Status < 0)
                    {
                        KTPlayerManager.ShowMessageBox(player, "Thông báo", GiftCodeRequest.Msg);
                    }
                    else
                    {
                        string GiftItem = GiftCodeRequest.GiftItem;

                        string[] ItemList = GiftItem.Split('#');

                        foreach (string Item in ItemList)
                        {
                            string[] PramItem = Item.Split('|');

                            int ItemID = Int32.Parse(PramItem[0]);
                            int ItemNum = Int32.Parse(PramItem[1]);

                            if (!ItemManager.CreateItem(Global._TCPManager.TcpOutPacketPool, player, ItemID, ItemNum, 0, "ActiveGiftCode", false, 1, false, ItemManager.ConstGoodsEndTime))
                            {
                                KTPlayerManager.ShowNotification(player, "Có lỗi khi nhận vật phẩm");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogManager.WriteLog(LogTypes.Error, "ACTIVE GIFTCODE BUG :" + ex.ToString());
                }
            }
            else
            {
                KTPlayerManager.ShowNotification(player, "Hãy thử lại sau 10 giây");
            }
        }
    }
}