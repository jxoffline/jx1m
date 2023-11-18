using GameServer.Core.Executor;
using GameServer.KiemThe.Core.Item;
using GameServer.KiemThe.Logic;
using GameServer.KiemThe.Logic.Manager.Shop;
using GameServer.Logic;
using GameServer.Server;
using Server.Data;
using Server.TCP;
using Server.Tools;
using System;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace GameServer.KiemThe.Core.Activity.CardMonth
{
    public class CardMonthManager
    {
        public static int DAYS_PER_YUE_KA = 30;

        public static int BoundTokenRevicie = 500;

        public readonly static int YUE_KA_MONEY_ID_IN_CHARGE_FILE = 12000;

        /// <summary>
        ///  DIC PHẦN THƯỞNG THEO NGÀY
        /// </summary>
        private static ConfigCard _ConfigCard = new ConfigCard();

        public static string YUE_KA_GOODS_FILE = "Config/KT_Activity/KTCardMonth.xml";

        public static void Setup()
        {
            string Files = KTGlobal.GetDataPath(YUE_KA_GOODS_FILE);

            using (var stream = System.IO.File.OpenRead(Files))
            {
                var serializer = new XmlSerializer(typeof(ConfigCard));
                _ConfigCard = serializer.Deserialize(stream) as ConfigCard;
            }
        }




        /// <summary>
        /// CMD call mua thẻ tháng | Cái này sẽ call từ WEBSERVICE Khi thanh toán với STORE THÀNH CÔNG
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="roleID"></param>
        public static void HandleUserBuyYueKa(string userID, int roleID)
        {
            KPlayer client = KTPlayerManager.Find(roleID);

            if (null != client)
            {
                lock (client.YKDetail)
                {
                    if (client.YKDetail.HasYueKa == 0)
                    {
                        DateTime nowDate = TimeUtil.NowDateTime();
                        client.YKDetail.HasYueKa = 1;
                        client.YKDetail.BegOffsetDay = KTGlobal.GetOffsetDay(nowDate);
                        client.YKDetail.EndOffsetDay = client.YKDetail.BegOffsetDay + DAYS_PER_YUE_KA;
                        client.YKDetail.CurOffsetDay = KTGlobal.GetOffsetDay(nowDate);
                        client.YKDetail.AwardInfo = "0";
                    }
                    else
                    {
                        client.YKDetail.EndOffsetDay += DAYS_PER_YUE_KA;
                    }

                    _UpdateYKDetail2DB(client, client.YKDetail);
                }
            }
            else
            {
                // Mua offline có thể là mua qua web
                int beginOffsetDay = KTGlobal.GetOffsetDay(TimeUtil.NowDateTime());
                string strcmd = string.Format("{0}:{1}:{2}", roleID, beginOffsetDay, RoleParamName.YueKaInfo);
                string[] dbFields = Global.ExecuteDBCmd((int)TCPGameServerCmds.CMD_DB_ROLE_BUY_YUE_KA_BUT_OFFLINE, strcmd, GameManager.LocalServerId);
                if (null == dbFields || dbFields.Length != 1 || dbFields[0] != "0")
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Có lỗi khi mua thẻ tháng, UserID={0}, last roldid={1}, Có sự cố khi gửi thông tin vào DB", userID, roleID));
                }
            }
        }

        #region TCP_NETWORK

        /// <summary>
        /// Lấy ra dữ liệu thẻ tháng
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
        public static TCPProcessCmdResults ProcessGetYueKaData(TCPManager tcpMgr, TMSKSocket socket, TCPClientPool tcpClientPool, TCPRandKey tcpRandKey, global::Server.Protocol.TCPOutPacketPool pool, int nID, byte[] data, int count, out global::Server.Protocol.TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;
            string cmdData = null;
            string[] fields = null;
            try
            {
                cmdData = new UTF8Encoding().GetString(data, 0, count);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Không thể phân giải dữ liệu, CMD={0}, Client={1}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket)));
                return TCPProcessCmdResults.RESULT_FAILED;
            }

            try
            {
                fields = cmdData.Split(':');
                if (1 != fields.Length)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Pram gửi lên không hợp lệ, CMD={0}, Client={1}, Recv={2}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), fields.Length));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                int roleID = Convert.ToInt32(fields[0]);
                KPlayer client = KTPlayerManager.Find(socket);
                if (null == client || client.RoleID != roleID)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("RoleID gửi lên toang, CMD={0}, Client={1}, RoleID={2}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), roleID));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                YueKaData ykData = null;

                lock (client.YKDetail)
                {
                    ykData = client.YKDetail.ToYueKaData();
                    ykData.Config = _ConfigCard;
                    ykData.BoundToken = BoundTokenRevicie;
                    ykData.Slogan = "30 ngày có thể tích lũy hoàn trả " + KTGlobal.CreateStringByColor("15.000 Đồng Khóa", ColorType.Yellow) + " và rất nhiều phần quá giá trị";
                }

                client.SendPacket(nID, DataHelper.ObjectToBytes<YueKaData>(ykData));
                return TCPProcessCmdResults.RESULT_OK;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, "ProcessGetYueKaData", false);
            }

            return TCPProcessCmdResults.RESULT_DATA;
        }

        public static TCPProcessCmdResults ActiveCardMoth(TCPManager tcpMgr, TMSKSocket socket, TCPClientPool tcpClientPool, TCPRandKey tcpRandKey, global::Server.Protocol.TCPOutPacketPool pool, int nID, byte[] data, int count, out global::Server.Protocol.TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;
            string cmdData = null;
            string[] fields = null;
            try
            {
                cmdData = new UTF8Encoding().GetString(data, 0, count);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Không thể phân giải dữ liệu, CMD={0}, Client={1}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket)));
                return TCPProcessCmdResults.RESULT_FAILED;
            }

            try
            {
                KPlayer client = KTPlayerManager.Find(socket);
                if (null == client)
                {
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                KTPlayerManager.ShowMessageBox(client, "Chú ý ", "Bạn có muốn mua thẻ tháng này với giá :" + YUE_KA_MONEY_ID_IN_CHARGE_FILE + " Đồng?", new Action(() => SureBuy(client)),true);

                return TCPProcessCmdResults.RESULT_OK;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, "ProcessGetYueKaData", false);
            }

            return TCPProcessCmdResults.RESULT_OK;
        }

        private static void SureBuy(KPlayer client)
        {
            if (!KTGlobal.IsHaveMoney(client, YUE_KA_MONEY_ID_IN_CHARGE_FILE, Entities.MoneyType.Dong))
            {
                KTPlayerManager.ShowNotification(client, "Đồng không đủ");
                return;
            }

            SubRep _REP = KTGlobal.SubMoney(client, YUE_KA_MONEY_ID_IN_CHARGE_FILE, Entities.MoneyType.Dong, "CARDMONTH",true);

            if (_REP.IsOK)
            {
                HandleUserBuyYueKa(client.strUserID, client.RoleID);
                KTPlayerManager.ShowNotification(client, "Kích hoạt thẻ tháng thành công");

                YueKaData ykData = null;

                lock (client.YKDetail)
                {
                    ykData = client.YKDetail.ToYueKaData();
                    ykData.Config = _ConfigCard;
                    ykData.BoundToken = BoundTokenRevicie;
                    ykData.Slogan = "30 ngày có thể tích lũy hoàn trả " + KTGlobal.CreateStringByColor("15.000 Đồng Khóa", ColorType.Yellow) + " và rất nhiều phần quá giá trị";
                }

                client.SendPacket((int) TCPGameServerCmds.CMD_SPR_GET_YUEKA_DATA, DataHelper.ObjectToBytes<YueKaData>(ykData));
                return;
            }
            else
            {
                KTPlayerManager.ShowNotification(client, "Đồng không đủ");
                return;
            }
        }

        /// <summary>
        /// Funtion thực hiện nhận thưởng thẻ tháng
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
        public static TCPProcessCmdResults ProcessGetYueKaAward(TCPManager tcpMgr, TMSKSocket socket, TCPClientPool tcpClientPool, TCPRandKey tcpRandKey, global::Server.Protocol.TCPOutPacketPool pool, int nID, byte[] data, int count, out global::Server.Protocol.TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;
            string cmdData = null;
            string[] fields = null;
            try
            {
                cmdData = new UTF8Encoding().GetString(data, 0, count);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Có lỗi khi khi phân giải giữ liệu, CMD={0}, Client={1}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket)));
                return TCPProcessCmdResults.RESULT_FAILED;
            }

            try
            {
                fields = cmdData.Split(':');
                if (2 != fields.Length)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Dữ liệu gửi lên có lỗi, CMD={0}, Client={1}, Recv={2}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), fields.Length));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                int roleID = Convert.ToInt32(fields[0]);
                int day = Convert.ToInt32(fields[1]);

                KPlayer client = KTPlayerManager.Find(socket);
                if (null == client || client.RoleID != roleID)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Client ROLEID không đúng, CMD={0}, Client={1}, RoleID={2}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), roleID));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                YueKaError err = _GetYueKaAward(client, day);
                string cmd = string.Format("{0}:{1}:{2}", roleID, (int)err, day);
                client.SendPacket(nID, cmd);
                return TCPProcessCmdResults.RESULT_OK;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, "ProcessGetYueKaData", false);
            }

            return TCPProcessCmdResults.RESULT_DATA;
        }

        #endregion TCP_NETWORK

        /// <summary>
        /// Nhận quà
        /// </summary>
        /// <param name="client"></param>
        /// <param name="day"></param>
        /// <returns></returns>
        private static YueKaError _GetYueKaAward(KPlayer client, int day)
        {
            if (day <= 0 || day > DAYS_PER_YUE_KA)
            {
                return YueKaError.YK_CannotAward_ParamInvalid;
            }

            lock (client.YKDetail)
            {
                if (client.YKDetail.HasYueKa == 0)
                {
                    return YueKaError.YK_CannotAward_HasNotYueKa;
                }

                if (day < client.YKDetail.CurDayOfPerYueKa())
                {
                    return YueKaError.YK_CannotAward_DayHasPassed;
                }

                if (day > client.YKDetail.CurDayOfPerYueKa())
                {
                    return YueKaError.YK_CannotAward_TimeNotReach;
                }

                string awardInfo = client.YKDetail.AwardInfo;

                if (awardInfo.Length < day || awardInfo[day - 1] == '1')
                {
                    return YueKaError.YK_CannotAward_AlreadyAward;
                }

                Card _CardSelect = _ConfigCard.Card.Where(x => x.Day == day).FirstOrDefault();

                if (_CardSelect == null)
                {
                    return YueKaError.YK_CannotAward_ConfigError;
                }

                string ItemCode = _CardSelect.ItemCard;

                string[] ItemPram = ItemCode.Split(',');

                int ItemID = Int32.Parse(ItemPram[0]);

                int ItemNumer = Int32.Parse(ItemPram[1]);

                if (!ItemManager.CreateItem(Global._TCPManager.TcpOutPacketPool, client, ItemID, ItemNumer, 0, "CARDMONTH", false, 1, false, ItemManager.ConstGoodsEndTime,"",-1,"",0,1,true))
                {
                    KTPlayerManager.ShowNotification(client, "Có lỗi khi nhận vật phẩm chế tạo");
                }

                // Thực hiện add đồng khóa
                KTGlobal.AddMoney(client, BoundTokenRevicie, Entities.MoneyType.DongKhoa, "CARDMONTH|" + day);

                client.YKDetail.AwardInfo = awardInfo.Substring(0, day - 1) + "1";

                _UpdateYKDetail2DB(client, client.YKDetail);

                //TODO : UPDATE ICON CHO ACTIVITY Ở ĐÂY
            }

            return YueKaError.YK_Success;
        }

        /// <summary>
        /// Kiểm tra thẻ tháng còn hợp lệ không
        /// </summary>
        /// <param name="gameClient"></param>
        public static void CheckValid(KPlayer client)
        {
            if (client == null) return;

            lock (client.YKDetail)
            {
                if (client.YKDetail.HasYueKa == 0) return;

                while (true)
                {
                    int todayOffset = KTGlobal.GetOffsetDay(TimeUtil.NowDateTime());
                    if (todayOffset >= client.YKDetail.EndOffsetDay)
                    {
                        client.YKDetail.HasYueKa = 0;
                        break;
                    }

                    int curBegOffsetDay = client.YKDetail.CurOffsetDay - client.YKDetail.AwardInfo.Length + 1;

                    if (todayOffset >= curBegOffsetDay + DAYS_PER_YUE_KA)
                    {
                        client.YKDetail.CurOffsetDay = todayOffset;
                        client.YKDetail.AwardInfo = "";

                        for (int i = curBegOffsetDay + DAYS_PER_YUE_KA; i <= todayOffset; ++i)
                        {
                            client.YKDetail.AwardInfo += "0";
                        }
                        break;
                    }

                    for (int i = client.YKDetail.CurOffsetDay + 1; i <= todayOffset; ++i)
                    {
                        client.YKDetail.AwardInfo += "0";
                    }
                    client.YKDetail.CurOffsetDay = todayOffset;

                    break;
                }

                _UpdateYKDetail2DB(client, client.YKDetail);
            }
        }

        /// <summary>
        /// Ghi lại vào DB thông tin thẻ tháng
        /// </summary>
        /// <param name="yueKaDetail"></param>
        private static void _UpdateYKDetail2DB(KPlayer client, YueKaDetail YKDetail)
        {
            string value = client.YKDetail.SerializeToString();

            Global.SaveRoleParamsStringToDB(client, RoleParamName.YueKaInfo, value, true);
        }

        /// <summary>
        /// Cập nhật ngày mới cho thẻ tháng
        /// </summary>
        /// <param name="client"></param>
        public static void UpdateNewDay(KPlayer client)
        {
            if (client == null)
            {
                return;
            }

            CheckValid(client);
        }
    }
}