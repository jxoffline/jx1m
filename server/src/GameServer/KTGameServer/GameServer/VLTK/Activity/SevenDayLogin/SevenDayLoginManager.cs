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

namespace GameServer.KiemThe.Core.Activity.DaySeriesLoginEvent
{
    /// <summary>
    /// 7 Ngày đăng nhập
    /// 7 Ngày đăng nhập liên tiếp
    /// </summary>
    public class SevenDayLoginManager
    {
        public static string KTSevenDaysLogin_XML = "Config/KT_Activity/KTSevenDaysLogin.xml";
        public static string KTSevenDaysLoginContinus_XML = "Config/KT_Activity/KTSevenDaysLoginContinus.xml";

        public static SevenDaysLogin _Event = new SevenDaysLogin();
        public static SevenDaysLoginContinus _EventContinus = new SevenDaysLoginContinus();

        /// <summary>
        /// Thiết lập
        /// </summary>
        public static void Setup()
        {
            string Files = KTGlobal.GetDataPath(KTSevenDaysLogin_XML);

            using (var stream = System.IO.File.OpenRead(Files))
            {
                var serializer = new XmlSerializer(typeof(SevenDaysLogin));
                _Event = serializer.Deserialize(stream) as SevenDaysLogin;
            }

            Files = KTGlobal.GetDataPath(KTSevenDaysLoginContinus_XML);

            using (var stream = System.IO.File.OpenRead(Files))
            {
                var serializer = new XmlSerializer(typeof(SevenDaysLoginContinus));
                _EventContinus = serializer.Deserialize(stream) as SevenDaysLoginContinus;
            }
        }

  

        /// <summary>
        /// Nhận quà tặng bởi thời gian
        /// </summary>
        /// <param name="Sec"></param>
        /// <param name="ItemGet"></param>
        /// <param name="player"></param>
        public static void GetAwardByTime(int Sec, RollAwardItem ItemGet, KPlayer player)
        {
            var ReviveItem = KTKTAsyncTask.Instance.ScheduleExecuteAsync(new DelayAsyncTask("SEVENRDAYLOGIN", ItemGet, player, TimerProc), Sec * 1000);
        }

        /// <summary>
        /// Timer prosecc
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void TimerProc(object sender, EventArgs e)
        {
            DelayAsyncTask _Task = (DelayAsyncTask)sender;

            RollAwardItem _ItemGet = (RollAwardItem)_Task.Tag;

            KPlayer _Player = _Task.Player;

            if (_ItemGet != null)
            {
                // Thưc hiện add vật phẩm vào kỹ năng sôngs
                // Mặc định toàn bộ vật phẩm nhận từ event này sẽ khóa hết
                if (!ItemManager.CreateItem(Global._TCPManager.TcpOutPacketPool, _Task.Player, _ItemGet.ItemID, _ItemGet.Number, 0, "SEVENRDAYLOGIN", false, 1, false, ItemManager.ConstGoodsEndTime))
                {
                    KTPlayerManager.ShowNotification(_Player, "Có lỗi khi nhận vật phẩm chế tạo");
                }
            }
        }

        public static RollAwardItem GetItemAward(int Days, int TYPE)
        {
            SevenDaysLoginItem _GiftSelect = new SevenDaysLoginItem();
            if (TYPE == 0)
            {
                _GiftSelect = _Event.SevenDaysLoginItem.Where(x => x.Days == Days).FirstOrDefault();
            }
            else
            {
                _GiftSelect = _EventContinus.SevenDaysLoginItem.Where(x => x.Days == Days).FirstOrDefault();
            }

            RollAwardItem _SelectItem = null;

            if (_GiftSelect != null)
            {
                List<RollAwardItem> RollAwardItem = _GiftSelect.RollAwardItem.ToList();

                int Random = KTGlobal.GetRandomNumber(0, 100);

                foreach (RollAwardItem _Item in RollAwardItem)
                {
                    Random = Random - _Item.Rate;

                    if (Random <= 0)
                    {
                        _SelectItem = _Item;

                        break;
                    }
                }
            }

            return _SelectItem;
        }

        #region SEVENDAYLOGIN_AWARD

        /// <summary>
        /// Lấy ra thông tin quà dăng nhập 7 ngày
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
        public static TCPProcessCmdResults ProcessSpriteUpdateEverydaySeriesLoginInfoCmd(TCPManager tcpMgr, TMSKSocket socket, TCPClientPool tcpClientPool, TCPRandKey tcpRandKey, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
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
                    LogManager.WriteLog(LogTypes.Error, string.Format("Can not find player corresponding ID, CMD={0}, Client={1}, RoleID={2}",
                                                                        (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), roleID));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                SevenDayEvent _SenvenDayEvent = new SevenDayEvent();
                // Gán cho nó = config
                _SenvenDayEvent.SevenDaysLogin = _Event;
                _SenvenDayEvent.SevenDaysLogin.RevicedHistory = RevicedHistoryBuild(client);
                // Xem ngày này là cái ngày đéo nào
                // Căn cứ vào đây có thể biết được nó nhận chưa hay nhận rồi
                _SenvenDayEvent.SevenDaysLogin.DayID = GetDayLessLogin(client) + 1;

                // Set cho nó  = thằng host
                _SenvenDayEvent.SevenDaysLoginContinus = _EventContinus;
                // Đăng nhập liên tục bao nhiêu ngày
                _SenvenDayEvent.SevenDaysLoginContinus.TotalDayLoginContinus = client.RoleWelfareData.logincontinus+1;
                _SenvenDayEvent.SevenDaysLoginContinus.Step = client.RoleWelfareData.sevenday_continus_step;
                _SenvenDayEvent.SevenDaysLoginContinus.SevenDayLoginAward = client.RoleWelfareData.sevenday_continus_note;

                // Gửi về client thông tin
                byte[] _cmdData = DataHelper.ObjectToBytes<SevenDayEvent>(_SenvenDayEvent);

                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, _cmdData, nID);

                return TCPProcessCmdResults.RESULT_DATA;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, Global.GetDebugHelperInfo(socket), false);
            }

            return TCPProcessCmdResults.RESULT_FAILED;
        }

        /// <summary>
        /// Trả về ngày login
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        public static int GetDayLessLogin(KPlayer client)
        {
            // TÍNH TOÁN XEM LÀ NGÀY THỨ MẤY KỂ TỪ NGÀY MỞ SV
            int nDate = TimeUtil.NowDateTime().DayOfYear;

            int DayID = client.RoleWelfareData.createdayid;

            // Tạo nhân vật từ ngày trước tết
            if (nDate >= DayID)
            {
                return (nDate - DayID);
            }
            else
            {
                return ((365 + nDate) - DayID);
            }
        }

        /// <summary>
        /// Build ra history đã nhận
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        public static List<SevenDayLoginHistoryItem> RevicedHistoryBuild(KPlayer client)
        {
            List<SevenDayLoginHistoryItem> _Items = new List<SevenDayLoginHistoryItem>();

            string REVICE = client.RoleWelfareData.sevendaylogin_note;

            if (REVICE != "NONE")
            {
                string[] Pram = REVICE.Split('_');

                foreach (string DayRecore in Pram)
                {
                    SevenDayLoginHistoryItem _Item = new SevenDayLoginHistoryItem();

                    string[] ItemPram = DayRecore.Split('#');

                    int DayID = Int32.Parse(ItemPram[0]);

                    int ItemID = Int32.Parse(ItemPram[1].Split('|')[0]);

                    int ItemNum = Int32.Parse(ItemPram[1].Split('|')[1]);

                    _Item.DayID = DayID;
                    _Item.GoodIDs = ItemID;
                    _Item.GoodNum = ItemNum;

                    // Thêm vật phẩm vào list
                    _Items.Add(_Item);
                }
            }

            return _Items;
        }

        /// <summary>
        /// Nhận quà 7 ngày đăng nhập
        /// </summary>
        /// <param name="pool"></param>
        /// <param name="nID"></param>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static TCPProcessCmdResults ProcessSpriteGetSeriesLoginAwardGiftCmd(TCPManager tcpMgr, TMSKSocket socket, TCPClientPool tcpClientPool, TCPRandKey tcpRandKey, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
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
                int typeRecive = Convert.ToInt32(fields[1]);

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

                // Nếu là nhận đăng nhập 7 ngày thường
                if (typeRecive == 0)
                {
                    int PreDay = GetDayLessLogin(client);

                    // Đã quá 7 ngày không còn j để nhận
                    if (PreDay > 6)
                    {
                        KTPlayerManager.ShowNotification(client, "Không có phần thưởng để nhận!");
                        return TCPProcessCmdResults.RESULT_OK;
                    }
                    else // Nếu mà nhỏ hơn hoặc = 6 thì cho nhận
                    {
                        int DAYGET = PreDay + 1;

                        List<SevenDayLoginHistoryItem> _TotalItem = RevicedHistoryBuild(client);

                        var Find = _TotalItem.Where(x => x.DayID == DAYGET).FirstOrDefault();
                        // nếu đã nhận rồi thì chim cút
                        if (Find != null)
                        {
                            KTPlayerManager.ShowNotification(client, "Bạn đã nhận thưởng rồi không thể nhận lại!");
                            return TCPProcessCmdResults.RESULT_OK;
                        }
                        else
                        {
                            // LẤY RA QUÀ TẶNG CỦA NGÀY HÔM NAY
                            RollAwardItem _ItemSelect = SevenDayLoginManager.GetItemAward(DAYGET, typeRecive);
                            /// Toác
                            if (_ItemSelect == null)
                            {
                                KTPlayerManager.ShowNotification(client, "Không có phần thưởng để nhận!");
                                return TCPProcessCmdResults.RESULT_OK;
                            }

                            if (!KTGlobal.IsHaveSpace(10, client))
                            {
                                KTPlayerManager.ShowNotification(client, "Túi đồ không đủ chỗ trống để nhận thưởng\n Cần ít nhất 10 ô trống");
                                strcmd = string.Format("{0}:{1}:{2}:{3}:{4}", roleID, -1, client.RoleWelfareData.logindayid, _ItemSelect.ItemID + "," + _ItemSelect.Number, typeRecive);
                                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, strcmd, nID);
                                return TCPProcessCmdResults.RESULT_DATA;
                            }

                            // Thực hiện DELAY 5s xong add quà
                            SevenDayLoginManager.GetAwardByTime(7, _ItemSelect, client);
                            // Thực hiện ghi vào là nó đã nhận
                            if (client.RoleWelfareData.sevendaylogin_note != "NONE")
                            {
                                client.RoleWelfareData.sevendaylogin_note = client.RoleWelfareData.sevendaylogin_note + "_" + DAYGET + "#" + _ItemSelect.ItemID + "|" + _ItemSelect.Number;
                            }
                            else
                            {
                                client.RoleWelfareData.sevendaylogin_note = DAYGET + "#" + _ItemSelect.ItemID + "|" + _ItemSelect.Number;
                            }

                            // Thực hiện ghi vào DB cho nóng
                            Global.WriterWelfare(client);


                            LogManager.WriteLog(LogTypes.Welfare, "[SevenDayLogin][" + client.RoleID + "] Revice Gift : STEP :" + DAYGET + "| ITEMID :" + _ItemSelect.ItemID + "| ITEMNUM :" + _ItemSelect.Number);
                            // Thực hiện gửi về cho client
                            // ROLEID | 1 LÀ THÀNH CÔNG | DAYGET là mốc quà thực hiện animation |  Vật phẩm sẽ vào |  Và SỐ LƯỢNG
                            strcmd = string.Format("{0}:{1}:{2}:{3}:{4}", roleID, 1, DAYGET, _ItemSelect.ItemID + "," + _ItemSelect.Number, typeRecive);

                            tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, strcmd, nID);
                        }
                    }
                } // Nếu là đăng nhập 7 ngày liên túc
                else if (typeRecive == 1)
                {
                    // Số ngày đã đăng nhập liên tục
                    int TotalLoginContinus = client.RoleWelfareData.logincontinus+1;
                    // Step Will Be Revice
                    int CurentStep = client.RoleWelfareData.sevenday_continus_step + 1;

                    var Find = _EventContinus.SevenDaysLoginItem.Where(x => x.ID == CurentStep).FirstOrDefault();

                    if (Find != null)
                    {
                        int DayRequest = Find.Days;

                        // Nếu số ngày đăng nhập liên tục mà nhỏ hơn số ngày mong muốn nhận
                        if (TotalLoginContinus < DayRequest)
                        {
                            KTPlayerManager.ShowNotification(client, "Cần đăng nhập " + DayRequest + " để nhận mốc này!");
                            return TCPProcessCmdResults.RESULT_OK;
                        }
                        else
                        {
                            RollAwardItem _ItemSelect = SevenDayLoginManager.GetItemAward(CurentStep, Find.Days);

                            /// Toác
                            if (_ItemSelect == null)
                            {
                                KTPlayerManager.ShowNotification(client, "Không có phần thưởng để nhận!");
                                return TCPProcessCmdResults.RESULT_OK;
                            }

                            if (!KTGlobal.IsHaveSpace(10, client))
                            {
                                KTPlayerManager.ShowNotification(client, "Túi đồ không đủ chỗ trống để nhận thưởng\n Cần ít nhất 10 ô trống");
                                strcmd = string.Format("{0}:{1}:{2}:{3}:{4}", roleID, -1, client.RoleWelfareData.logindayid, _ItemSelect.ItemID + "," + _ItemSelect.Number, typeRecive);
                                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, strcmd, nID);
                                return TCPProcessCmdResults.RESULT_DATA;
                            }

                            // Thực hiện DELAY 5s xong add quà
                            SevenDayLoginManager.GetAwardByTime(7, _ItemSelect, client);

                            client.RoleWelfareData.sevenday_continus_step = CurentStep;
                            if (client.RoleWelfareData.sevenday_continus_note != "NONE")
                            {
                                client.RoleWelfareData.sevenday_continus_note = client.RoleWelfareData.sevenday_continus_note + "|" + _ItemSelect.ItemID + "_" +  _ItemSelect.Number;
                            }
                            else
                            {
                                client.RoleWelfareData.sevenday_continus_note = _ItemSelect.ItemID + "_" + _ItemSelect.Number;
                            }

                            // Ghi lại vào DB
                            Global.WriterWelfare(client);


                            LogManager.WriteLog(LogTypes.Welfare, "[SevenDayLoginContinus][" + client.RoleID + "] Revice Gift : STEP :" + CurentStep + "| ITEMID :" + _ItemSelect.ItemID + "| ITEMNUM :" + _ItemSelect.Number);
                            // Thực hiện gửi về cho client
                            // ROLEID | 1 LÀ THÀNH CÔNG | DAYGET là mốc quà thực hiện animation |  Vật phẩm sẽ vào |  Và SỐ LƯỢNG
                            strcmd = string.Format("{0}:{1}:{2}:{3}:{4}", roleID, 1, CurentStep, _ItemSelect.ItemID + "," + _ItemSelect.Number, typeRecive);

                            tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, strcmd, nID);
                        }
                    }
                    else
                    {
                        KTPlayerManager.ShowNotification(client, "Không có phần thưởng để nhận!");
                        return TCPProcessCmdResults.RESULT_OK;
                    }
                }

                // TODO : THỰC HIỆN UPDATE ICON PHÁT SÁNG Ở ĐÂY NẾU KHI NÓ ĐÃ NHẬN

                return TCPProcessCmdResults.RESULT_DATA;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, Global.GetDebugHelperInfo(socket), false);
            }

            return TCPProcessCmdResults.RESULT_FAILED;
        }

        #endregion SEVENDAYLOGIN_AWARD
    }
}