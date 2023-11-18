using GameServer.Core.Executor;
using GameServer.KiemThe.Core.Item;
using GameServer.KiemThe.Logic;
using GameServer.Logic;
using GameServer.Server;
using Server.Protocol;
using Server.TCP;
using Server.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace GameServer.KiemThe.Core.Activity.EveryDayOnlineEvent
{
    /// <summary>
    /// Xử lý sự kiện online nhận thưởng
    /// </summary>
    public class EveryDayOnlineManager
    {
        public static EveryDayOnLineEvent _Event = new EveryDayOnLineEvent();

        public static string KTEveryDayEvent_XML = "Config/KT_Activity/KTEveryDayEvent.xml";

        public static void Setup()
        {
            string Files = KTGlobal.GetDataPath(KTEveryDayEvent_XML);

            using (var stream = System.IO.File.OpenRead(Files))
            {
                var serializer = new XmlSerializer(typeof(EveryDayOnLineEvent));
                _Event = serializer.Deserialize(stream) as EveryDayOnLineEvent;
            }
        }

        /// <summary>
        /// Lấy ra toàn bộ danh sách quà tặng có thể nhận khi online
        /// </summary>
        /// <param name="TimeSec"></param>
        /// <returns></returns>
        public static List<EveryDayOnLine> GetAllGiftCanGet(int TimeSec, int Step)
        {
            List<EveryDayOnLine> _Total = new List<EveryDayOnLine>();

            _Total = _Event.Item.Where(x => x.TimeSecs < TimeSec && x.StepID > Step).OrderBy(x => x.StepID).ToList();

            return _Total;
        }

        public static void GetAwardByTime(int Sec, AwardItem ItemGet, KPlayer player)
        {
            var ReviveItem = KTKTAsyncTask.Instance.ScheduleExecuteAsync(new DelayAsyncTask("OnlineRecvice", ItemGet, player, TimerProc), Sec * 1000);
        }

        private static void TimerProc(object sender, EventArgs e)
        {
            DelayAsyncTask _Task = (DelayAsyncTask)sender;

            KPlayer _Player = _Task.Player;

            AwardItem _ItemGet = (AwardItem)_Task.Tag;

            if (_ItemGet != null)
            {
                // Thưc hiện add vật phẩm vào kỹ năng sôngs
                // Mặc định toàn bộ vật phẩm nhận từ event này sẽ khóa hết
                if (!ItemManager.CreateItem(Global._TCPManager.TcpOutPacketPool, _Task.Player, _ItemGet.ItemID, _ItemGet.Number, 0, "EVERYDAYONLINE", false, 1, false, ItemManager.ConstGoodsEndTime, "", -1, "", 0, 1, false))
                {
                    KTPlayerManager.ShowNotification(_Player, "Có lỗi khi nhận vật phẩm chế tạo");
                }
            }
        }

        public static AwardItem GetItemAward(int StepID)
        {
            EveryDayOnLine _GiftSelect = _Event.Item.Where(x => x.StepID == StepID).FirstOrDefault();

            AwardItem _SelectItem = null;

            if (_GiftSelect != null)
            {
                List<AwardItem> RollAwardItem = _GiftSelect.RollAwardItem.ToList();

                int Random = KTGlobal.GetRandomNumber(0, 100);

                int idx = 0;
                foreach (AwardItem _Item in RollAwardItem)
                {
                    Random = Random - _Item.Rate;

                    if (Random <= 0)
                    {
                        _SelectItem = _Item;
                        break;
                    }
                    idx++;
                }
            }

            return _SelectItem;
        }

        #region TCP_NETWORK

        /// <summary>
        /// Lấy thông tin online nhận thưởng của người chơi
        /// </summary>
        /// <param name="pool"></param>
        /// <param name="nID"></param>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static TCPProcessCmdResults ProcessSpriteUpdateEverydayOnlineAwardGiftInfoCmd(TCPManager tcpMgr, TMSKSocket socket, TCPClientPool tcpClientPool, TCPRandKey tcpRandKey, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;
            string cmdData = null;

            try
            {
                cmdData = new UTF8Encoding().GetString(data, 0, count);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Onlien Nhận thưởng lỗi , CMD={0}, Client={1}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket)));
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

                string strcmd = "";

                KPlayer client = KTPlayerManager.Find(socket);
                if (null == client || client.RoleID != roleID)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Can not find player corresponding ID, CMD={0}, Client={1}, RoleID={2}",
                                                                        (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), roleID));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                int nDate = TimeUtil.NowDateTime().DayOfYear;

                // nếu ngày login khác nhau thì thực hiện reset lại toàn bộ thông tin
                if (nDate != client.RoleWelfareData.logindayid)
                {
                    // thực hiện reset lại hết
                    Global.UpdateWelfareRole(client);
                }

                string CurentRevice = client.RoleWelfareData.online_step;

                int Step = 0;
                string GoldRevice = "";

                if (CurentRevice != "NONE")
                {
                    if (CurentRevice.Contains("#"))
                    {
                        string[] Pram = CurentRevice.Split('#');
                        Step = Int32.Parse(Pram[0]);
                        GoldRevice = Pram[1];
                    }
                }

                EveryDayOnLineEvent _Online = new EveryDayOnLineEvent();
                _Online.IsOpen = EveryDayOnlineManager._Event.IsOpen;
                _Online.Item = EveryDayOnlineManager._Event.Item;
                _Online.DayOnlineSecond = client.DayOnlineSecond;
                _Online.EveryDayOnLineAwardStep = Step;
                // TODO Sữa lại 1 số quy tắc award chém lại chỗ quà đã nhận của tham số EveryDayOnLineAwardGoodsID [ITEMID|SL$ITEMID|SL$ITEMID|SL]
                _Online.EveryDayOnLineAwardGoodsID = GoldRevice;

                client.SendPacket<EveryDayOnLineEvent>(nID, _Online);

                return TCPProcessCmdResults.RESULT_DATA;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, Global.GetDebugHelperInfo(socket), false);
            }

            return TCPProcessCmdResults.RESULT_FAILED;
        }

        /// <summary>
        /// Class xử lý việc nhận quà onlien nhận thưởng
        /// </summary>
        /// <param name="pool"></param>
        /// <param name="nID"></param>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static TCPProcessCmdResults ProcessSpriteGetEveryDayOnLineAwardGiftCmd(TCPManager tcpMgr, TMSKSocket socket, TCPClientPool tcpClientPool, TCPRandKey tcpRandKey, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
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
                //Lấy ra thông tin gửi lên
                string[] fields = cmdData.Split(':');
                if (fields.Length != 1)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Error Socket params count not fit CMD={0}, Client={1}, Recv={2}",
                        (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), fields.Length));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                int roleID = Convert.ToInt32(fields[0]);

                // Nếu nTimer là 1 là thì là online nhận thưởng
                // Nếu nTimer là 2 thì là đăng nhập 7 ngày

                string strcmd = "";

                KPlayer client = KTPlayerManager.Find(socket);
                if (null == client || client.RoleID != roleID)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Can not find player corresponding ID, CMD={0}, Client={1}, RoleID={2}",
                                                                        (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), roleID));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }



                if (client.IsSpamClick())
                {
                    KTPlayerManager.ShowNotification(client, "Bạn đang thao tác quá nhanh");

                    return TCPProcessCmdResults.RESULT_DATA;
                }
                else
                {
                    client.SendClick();
                }


                if (!_Event.IsOpen)
                {
                    KTPlayerManager.ShowNotification(client, "Sự kiện đang không mở!");

                    strcmd = string.Format("{0}:{1}:{2}:{3}:{4}", roleID, -1, -1, client.DayOnlineSecond, "");

                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, strcmd, nID);

                    return TCPProcessCmdResults.RESULT_DATA;

                }

                int nDate = TimeUtil.NowDateTime().DayOfYear;
                // nếu ngày login khác nhau thì thực hiện reset lại toàn bộ thông tin
                if (nDate != client.RoleWelfareData.logindayid)
                {
                    // thực hiện reset lại hết
                    Global.UpdateWelfareRole(client);
                }

                string CurentRevice = client.RoleWelfareData.online_step;

                int Step = 0;

                string ItemReviced = "";

                if (CurentRevice != "NONE")
                {
                    if (CurentRevice.Contains("#"))
                    {
                        string[] Pram = CurentRevice.Split('#');
                        Step = Int32.Parse(Pram[0]);
                        ItemReviced = Pram[1];
                    }
                }

                //  Lấy ra toàn bộ danh sách quà tặng có thể nhận trọng ngày hôm nay

                List<EveryDayOnLine> GetAllGiftCanGet = EveryDayOnlineManager.GetAllGiftCanGet(client.DayOnlineSecond, Step);
                //// Nếu có quà có thể nhận
                if (GetAllGiftCanGet.Count > 0)
                {
                    // Lấy ra mốc đầu tiên
                    EveryDayOnLine _SelectQua = GetAllGiftCanGet.FirstOrDefault();

                    int SpaceNeed = 10;
                    // Nếu mà nó đủ túi đồ thì cho nó nhận
                    if (KTGlobal.IsHaveSpace(SpaceNeed, client))
                    {
                        Core.Activity.EveryDayOnlineEvent.AwardItem _ItemSelect = EveryDayOnlineManager.GetItemAward(_SelectQua.StepID);
                        // Thực hiện DELAY 7s xong add quà
                        EveryDayOnlineManager.GetAwardByTime(7, _ItemSelect, client);
                        //ITEMID|SL$ITEMID|SL$ITEMID|SL
                        if (ItemReviced != "")
                        {
                            ItemReviced = ItemReviced + "$" + _ItemSelect.ItemID + "|" + _ItemSelect.Number;
                        }
                        else
                        {
                            ItemReviced = _ItemSelect.ItemID + "|" + _ItemSelect.Number;
                        }

                        string GAMEDBBUILD = _SelectQua.StepID + "#" + ItemReviced;

                        client.RoleWelfareData.online_step = GAMEDBBUILD;


                        LogManager.WriteLog(LogTypes.Welfare, "[EveryDayOnLine][" + client.RoleID + "] Revice Gift : STEP :" + Step + "| ITEMID :" + _ItemSelect.ItemID + "| ITEMNUM :" + _ItemSelect.Number);
                        //Thực hiện ghi vào DB
                        Global.WriterWelfare(client);

                        //Gửi về client 6 PRAMENTER
                        //ID THẰNG QUAY - 1 : QUAY THÀNH CÔNG | MỐC SẼ THỰC HIỆN ANIMATION QUAY | THỜI GIAN ONLINE HIỆN TẠI | VẬT PHẨM SẼ QUAY VÀO
                        strcmd = string.Format("{0}:{1}:{2}:{3}:{4}", roleID, 1, _SelectQua.StepID, client.DayOnlineSecond, _ItemSelect.ItemID + "," + _ItemSelect.Number);

                        tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, strcmd, nID);

                        // TODO : THỰC HIỆN UPDATE ICON PHÁT SÁNG Ở ĐÂY NẾU KHI NÓ ĐÃ NHẬN

                        return TCPProcessCmdResults.RESULT_DATA;
                    }
                    else
                    {
                        KTPlayerManager.ShowNotification(client, "Cần sắp xếp 10 ô trống trong túi đồ để nhận thưởng!");

                        strcmd = string.Format("{0}:{1}:{2}:{3}:{4}", roleID, -1, Step, client.DayOnlineSecond, "");

                        tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, strcmd, nID);

                        return TCPProcessCmdResults.RESULT_DATA;
                    }
                }
                else // NẾU ĐÉO CÓ MỐC NÀO CÓ THỂ NHẬN NỮA ===> TRƯỜNG HỢP NÀY KHÔNG THỂ VÀO NHƯNG ĐỂ CHẮC CHẮN CLIENT CHECK KHÔNG CHUẨN TIMER THÌ GỬI VỀ THÔNG BÁO BẠN KHÔNG THỂ NHẬN QUÀ
                {
                    strcmd = string.Format("{0}:{1}:{2}:{3}:{4}", roleID, -1, Step, client.DayOnlineSecond, "");

                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, strcmd, nID);

                    return TCPProcessCmdResults.RESULT_DATA;
                }
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, Global.GetDebugHelperInfo(socket), false);
            }

            return TCPProcessCmdResults.RESULT_FAILED;
        }

        #endregion TCP_NETWORK
    }
}