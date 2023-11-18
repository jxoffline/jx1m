using GameServer.Core.Executor;
using GameServer.KiemThe;
using GameServer.KiemThe.Core.Item;
using GameServer.KiemThe.GameDbController;
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

namespace GameServer.VLTK.Core.Activity.TopRankingEvent
{
    /// <summary>
    /// Quản lý đua top
    /// </summary>
    public class TopRankingManager
    {
        public static List<TopRankingConfig> topRankingConfig { get; set; }

        //Cache lại
        public static Dictionary<RankMode, List<PlayerRanking>> RankServer = new Dictionary<RankMode, List<PlayerRanking>>();

        public static long LastRequestDb = 0;

        // LOADING RANKING
        public static string TOP_RANK_CONFIG_FILE = "Config/KT_Activity/KTTopRankingConfig.xml";

        /// <summary>
        /// Load config
        /// </summary>
        public static void Setup()
        {
            string Files = KTGlobal.GetDataPath(TOP_RANK_CONFIG_FILE);

            using (var stream = System.IO.File.OpenRead(Files))
            {
                var serializer = new XmlSerializer(typeof(List<TopRankingConfig>));
                topRankingConfig = serializer.Deserialize(stream) as List<TopRankingConfig>;
            }

            // Gọi tới gamedb để cache rank
            RankInit();

            // Đánh dấu thời gian gần đây nhất
            LastRequestDb = TimeUtil.NOW();

            //Tạo 1 timer check choác
            ScheduleExecutor2.Instance.scheduleExecute(new NormalScheduleTask("RankingEvent.................", LoopCheck), 5 * 1000, 2000);
        }

        /// <summary>
        ///  Hàm này sẽ chọc vào DB để cache ra danh sách 10 play đang đứng hạng
        ///  Trong trường hợp sự kiện kết thúc sẽ lấy ra cache của hạng đã lưu trong DB
        ///  Hàm này sẽ reload 5 phút 1 lần nếu sự kiện chưa kết thúc
        /// </summary>
        public static void RankInit(int ForceUpdate = -1)
        {
            // Duyệt tất cả các loại rank
            foreach (TopRankingConfig _RankElement in topRankingConfig)
            {
                // State = 0 sẽ đọc dữ liệu read time ra
                int State = 0;

                DateTime Now = DateTime.Now;

                double LessTime = (_RankElement.EndTime - Now).TotalSeconds;

                _RankElement.TimeLess = (int)LessTime;

                if (LessTime < 0)
                {
                    // Sự kiện đã kết thúc gamedb sẽ lấy trong cache ra đéo lấy readtime nữa
                    State = 1;
                }
                else
                {
                    State = 2;
                }
                // Nếu có chỉ định Update rank thì sẽ gửi lệnh vào DB sysn toàn bộ dữ liệu mới nhất trước khi kết thúc
                if (ForceUpdate == 1)
                {
                    State = 3;
                }

                int RankType = _RankElement.RankType;

                RankMode _Mode = (RankMode)_RankElement.RankType;

                byte[] bytesData = null;
                if (TCPProcessCmdResults.RESULT_FAILED == Global.RequestToDBServer3(Global._TCPManager.tcpClientPool, Global._TCPManager.TcpOutPacketPool, (int)TCPGameServerCmds.CMD_KT_TOPRANKING, string.Format("{0}:{1}", RankType, State), out bytesData, GameManager.LocalServerId))
                {
                    LogManager.WriteLog(LogTypes.Fatal, "QUERRY RANK BUG");
                    return;
                }

                if (null == bytesData || bytesData.Length <= 6)
                {
                    LogManager.WriteLog(LogTypes.Fatal, "QUERRY RANK BUG");
                    return;
                }

                Int32 length = BitConverter.ToInt32(bytesData, 0);

                //Đọc ra danh sách bang hội mà gamedb trả về
                List<PlayerRanking> _TMPRANK = DataHelper.BytesToObject<List<PlayerRanking>>(bytesData, 6, length - 2);

                // Set lại danh sách người chơi cho danh sách tương ứng
                RankServer[_Mode] = _TMPRANK;
            }
        }

        public static int GetRankOfPlayer(KPlayer client, RankMode _Mode)
        {
            int RankType = (int)_Mode;

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

        public static int SetStatusGetAward(KPlayer client, RankMode _Mode)
        {
            int RankType = (int)_Mode;

            int RoleID = client.RoleID;

            string CMDBUILD = RoleID + ":" + RankType;

            string[] dbFields = Global.ExecuteDBCmd((int)TCPGameServerCmds.CMD_KT_UPDATE_REVICE_STATUS, CMDBUILD, GameManager.ServerId);
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

        /// <summary>
        /// Gm forceupdate kết quả đuatop
        /// </summary>
        public static void GmRankingReadTimeUpdate()
        {
            // Lấy ra toàn bộ người chơi
            List<KPlayer> TotalPlayer = KTPlayerManager.GetAll();

            //Lấy ra toàn bộ người chơi
            foreach (KPlayer player in TotalPlayer)
            {
                // Xử dụng lệnh này trước khi kết thục TOP
                GameDb.ForceUpdateRanking(player);
            }

            foreach (TopRankingConfig _RankElement in topRankingConfig)
            {
                // Nếu như nay đéo phải ngày kết thúc sự kiện thì đéo check j cả coi như bỏ qua
                if (_RankElement.EndTime.Date == DateTime.Now.Date)
                {
                    // Lấy ra thời gian hiện tại
                    DateTime Now = DateTime.Now;
                    // Nếu như hôm nay là ngày check
                    double LessTime = (_RankElement.EndTime - Now).TotalSeconds;

                    if (LessTime > 60)
                    {
                        int Mimutes = (int)LessTime / 60;
                        KTGlobal.SendSystemEventNotification("Sự kiện đua TOP <color=yellow>" + _RankElement.RankName + "</color> còn "+ Mimutes + "phút nữa sẽ kết thúc.");
                    }
                }
            }
        }

        /// <summary>
        /// Thực hiện ghi vào DB
        /// </summary>
        public static void UpdateRankingToDb()
        {
            //Thực hiện ghi ở gameDB
            RankInit(1);
        }

        /// <summary>
        /// Hàm này sẽ kiểm tra xem khi nào sự kiện kết thúc để gọi vào gameDB chốt lại cơ sở dữ liệu lần cuối
        /// </summary>
        private static void LoopCheck(object sender, EventArgs e)
        {
            //Console.WriteLine("FUCK");
            // Check toàn bộ xem thế nào
            foreach (TopRankingConfig _RankElement in topRankingConfig)
            {
                // Nếu như nay đéo phải ngày kết thúc sự kiện thì đéo check j cả coi như bỏ qua
                 if (_RankElement.EndTime.Date == DateTime.Now.Date)
                {
                    // Lấy ra thời gian hiện tại
                    DateTime Now = DateTime.Now;
                    // Nếu như hôm nay là ngày check
                    double LessTime = (_RankElement.EndTime - Now).TotalSeconds;

                    if (LessTime > 0)
                    {
                        if (LessTime < (60 * 60 * 2) && _RankElement.ArletStatus == 0)
                        {
                            _RankElement.ArletStatus = 1;

                            KTGlobal.SendSystemEventNotification("Sự kiện đua TOP <color=yellow>" + _RankElement.RankName + "</color> chưa đầy 2 giờ nữa sẽ kết thúc.");

                            // Lấy ra toàn bộ người chơi
                            List<KPlayer> TotalPlayer = KTPlayerManager.GetAll();

                            //Lấy ra toàn bộ người chơi
                            foreach (KPlayer player in TotalPlayer)
                            {
                                // Xử dụng lệnh này trước khi kết thục TOP
                                GameDb.ForceUpdateRanking(player);
                            }
                        }

                        if (LessTime < (60 * 60 * 1) && _RankElement.ArletStatus == 1)
                        {
                            _RankElement.ArletStatus = 2;

                            KTGlobal.SendSystemEventNotification("Sự kiện đua TOP <color=yellow>" + _RankElement.RankName + "</color> chưa đầy 1 giờ nữa sẽ kết thúc.");

                            // Lấy ra toàn bộ người chơi
                            List<KPlayer> TotalPlayer = KTPlayerManager.GetAll();

                            //Lấy ra toàn bộ người chơi
                            foreach (KPlayer player in TotalPlayer)
                            {
                                // Xử dụng lệnh này trước khi kết thục TOP
                                GameDb.ForceUpdateRanking(player);
                            }
                        }
                        // trước khi sự kiện kết thúc 5 phút gửi lệnh vào GAMEDB gửi về dữ liệu mới nhất
                        if (LessTime < (60 * 5) && _RankElement.ArletStatus == 2)
                        {
                            _RankElement.ArletStatus = 3;

                            // Lấy ra toàn bộ người chơi
                            List<KPlayer> TotalPlayer = KTPlayerManager.GetAll();

                            //Lấy ra toàn bộ người chơi
                            foreach (KPlayer player in TotalPlayer)
                            {
                                // Xử dụng lệnh này trước khi kết thục TOP
                                GameDb.ForceUpdateRanking(player);
                            }
                        }

                        // Còn 1 phút cuối cùng thực hiện ghi vào DB chốt danh sách mấy thằng cuối cùng
                        if (LessTime < (60 * 1) && _RankElement.ArletStatus == 3)
                        {
                            _RankElement.ArletStatus = 4;

                            // Lấy ra toàn bộ người chơi
                            List<KPlayer> TotalPlayer = KTPlayerManager.GetAll();

                            //Lấy ra toàn bộ người chơi
                            foreach (KPlayer player in TotalPlayer)
                            {
                                // Xử dụng lệnh này trước khi kết thục TOP
                                GameDb.ForceUpdateRanking(player);
                            }

                            //Thực hiện ghi ở gameDB
                            RankInit(1);
                        }
                        if (LessTime < 0 && _RankElement.ArletStatus == 4)
                        {
                            // Send lệnh toDB để RefershRank lần cuối

                            _RankElement.ArletStatus = 5;

                            KTGlobal.SendSystemEventNotification("Sự kiện đua TOP <color=yellow>" + _RankElement.RankName + "</color> đã kết thúc! Quý bằng hữu có thể mở giao diện TOP để nhận thưởng");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Comment lấy ra bảng xếp hạng
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        public static List<TopRankingConfig> GetRankingEvent(KPlayer client)
        {
            long NowTime = TimeUtil.NOW();

            // Nếu như đã quá 300s thì thực hiện truy vấn DB để refresh lại thông tin
            if (NowTime - LastRequestDb > 300000)
            {
                RankInit();
                // Đánh dấu thời gian gần đây nhất
                LastRequestDb = TimeUtil.NOW();
            }

            // Coppy iamge from HOst
            List<TopRankingConfig> CoppyOfConfig = new List<TopRankingConfig>(topRankingConfig);

            if (CoppyOfConfig != null)
            {
                ///Duyệt tất cả
                foreach (TopRankingConfig _RankElement in CoppyOfConfig)
                {
                    List<PlayerRanking> _TmpPlayer = null;

                    RankMode _Mode = (RankMode)_RankElement.RankType;

                    RankServer.TryGetValue(_Mode, out _TmpPlayer);

                    if (_TmpPlayer != null)
                    {
                        if (_Mode == RankMode.CapDo)
                        {
                            _TmpPlayer.ToList().ForEach(c => c.Value = c.Level);
                        }

                        _RankElement.ListPlayer = _TmpPlayer;
                        // Lấy ra rank của bản thân nó trong này
                        _RankElement.SelfIndex = GetRankOfPlayer(client, _Mode) + 1;

                        DateTime Now = DateTime.Now;

                        double LessTime = (_RankElement.EndTime - Now).TotalSeconds;

                        _RankElement.TimeLess = (int)LessTime;

                        if (LessTime < 0)
                        {
                            _RankElement.State = 1;

                            var find = _TmpPlayer.Where(x => x.RoleID == client.RoleID).FirstOrDefault();
                            if (find != null)
                            {
                                if (find.Status == 1)
                                {
                                    // Gửi về state là thằng này đã nhận rồi
                                    _RankElement.State = 2;
                                }
                                else
                                {
                                    _RankElement.State = 3;
                                }
                            }
                        }
                        else
                        {
                            _RankElement.State = 0;
                        }
                    }
                }

                return CoppyOfConfig;
            }
            else
            {
                return new List<TopRankingConfig>();
            }
        }

        #region NETWORKZONE

        /// <summary>
        /// Lấy ra tổng thông tin rank
        /// </summary>
        /// <param name="tcpMgr"></param>
        /// <param name="socket"></param>
        /// <param name="tcpClientPool"></param>
        /// <param name="pool"></param>
        /// <param name="nID"></param>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <param name="tcpOutPacket"></param>
        /// <returns></returns>
        public static TCPProcessCmdResults CMD_KT_TOPRANKING_INFO(TCPManager tcpMgr, TMSKSocket socket, TCPClientPool tcpClientPool, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;

            try
            {
                KPlayer client = KTPlayerManager.Find(socket);
                if (null == client)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Can not find player corresponding ID, CMD={0}, Client={1}, RoleID={2}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), client.RoleID));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                List<TopRankingConfig> _RankInfo = GetRankingEvent(client);

                // Trả về thông tin bang hội
                tcpOutPacket = DataHelper.ObjectToTCPOutPacket<List<TopRankingConfig>>(_RankInfo, pool, nID);

                return TCPProcessCmdResults.RESULT_DATA;

                // trả vào DB xử lý
                // return Global.TransferRequestToDBServer(tcpMgr, socket, tcpClientPool, tcpRandKey, pool, nID, data, count, out tcpOutPacket, client.ServerId);
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, Global.GetDebugHelperInfo(socket), false);
            }

            return TCPProcessCmdResults.RESULT_FAILED;
        }

        /// <summary>
        /// 1 người chơi nào đó quyết định nhận thưởng
        /// </summary>
        /// <param name="client"></param>
        /// <param name="_Mode"></param>
        /// <param name="Index"></param>
        /// <returns></returns>
        public static int GetAward(KPlayer client, RankMode _Mode, int Index)
        {
            var FindRankConfig = topRankingConfig.Where(x => x.RankType == (int)_Mode).FirstOrDefault();
            // Nếu như có loại rank này
            if (FindRankConfig != null)
            {
                DateTime Now = DateTime.Now;

                double LessTime = (FindRankConfig.EndTime - Now).TotalSeconds;

                if (LessTime < 0)
                {
                    List<PlayerRanking> _TmpPlayer = null;

                    RankServer.TryGetValue(_Mode, out _TmpPlayer);

                    if (_TmpPlayer != null)
                    {
                        // Nếu sự kiện chưa kết thúc thì ta sẽ check xem thằng này có đủ tiêu chuẩn nhận thưởng không
                        var find = _TmpPlayer.Where(x => x.RoleID == client.RoleID).FirstOrDefault();
                        if (find != null)
                        {
                            if (find.Status == 1)
                            {
                                KTPlayerManager.ShowNotification(client, "Bạn đã nhận thưởng rồi không thể nhận thưởng nữa");
                                return -3;
                            }
                            else
                            {
                                // Check xem nó đang đứng thứ tự bao nhiêu | có thỏa mãn điều kiện không

                                var FindAward = FindRankConfig.AwardConfig.Where(x => x.Index == Index).FirstOrDefault();
                                if (FindAward != null)
                                {
                                    // Lấy ra Index hiện tại
                                    int CurentIndex = find.ID + 1;

                                    if (FindAward.RankStart == CurentIndex || FindAward.RankEnd == CurentIndex || (FindAward.RankStart < CurentIndex && FindAward.RankEnd > CurentIndex))
                                    {
                                        string TotalAwardList = FindAward.AwardList;

                                        string[] TotalItem = TotalAwardList.Split('|');

                                        int Count = TotalItem.Count();

                                        if (!KTGlobal.IsHaveSpace(10, client))
                                        {
                                            KTPlayerManager.ShowNotification(client, "Cần ít nhất 10 ô đồ trống để có thể nhận thưởng");
                                            return -3;
                                        }

                                        int Mark = SetStatusGetAward(client, _Mode);

                                        if (Mark == 0)
                                        {
                                            find.Status = 1;
                                            foreach (string Item in TotalItem)
                                            {
                                                int ItemID = Int32.Parse(Item.Split(':')[0]);
                                                int ItemNum = Int32.Parse(Item.Split(':')[1]);

                                                if (!ItemManager.CreateItem(Global._TCPManager.TcpOutPacketPool, client, ItemID, ItemNum, 0, "RANKINGREVICE", false, 0, false, ItemManager.ConstGoodsEndTime, "", -1, "", 0, 1, false))
                                                {
                                                    KTPlayerManager.ShowNotification(client, "Có lỗi khi quà tặng đua top");
                                                }
                                            }

                                            return 0;
                                        }
                                        else
                                        {
                                            KTPlayerManager.ShowNotification(client, "Có lỗi khi nhận quà đua top! Liên hệ với admin để được hỗ trợ");
                                            return -100;
                                        }

                                        //TODO : Ghi vào DB là thằng này đã nhận rồi
                                    }
                                    else
                                    {
                                        KTPlayerManager.ShowNotification(client, "Xếp hạng của bạn không thể nhận mốc này");
                                        return -4;
                                    }
                                }
                                else
                                {
                                    KTPlayerManager.ShowNotification(client, "Mốc quà này không tồn tại");
                                    return -4;
                                }
                            }
                        }
                    }
                }
                else
                {
                    // Sự kiện chưa kết thúc
                    KTPlayerManager.ShowNotification(client, "Sự kiện chưa kết thúc không thể nhận thưởng");
                    return -2;
                }
            }
            return -1;
        }

        /// <summary>
        /// Nhận thưởng đua top
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
        public static TCPProcessCmdResults CMD_KT_TOPRANKING_GETAWARD(TCPManager tcpMgr, TMSKSocket socket, TCPClientPool tcpClientPool, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
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
                KPlayer client = KTPlayerManager.Find(socket);
                if (null == client)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Can not find player corresponding ID, CMD={0}, Client={1}, RoleID={2}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), client.RoleID));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                string[] fields = cmdData.Split(':');
                if (fields.Length != 2)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Có lỗi ở tham số gửi lên, CMD={0}, Client={1}, Recv={2}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), fields.Length));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                int RankType = Int32.Parse(fields[0]);

                int Index = Int32.Parse(fields[1]);

                RankMode _Mode = (RankMode)RankType;

                int Status = GetAward(client, _Mode, Index);
                /// Toác
                if (Status != 0)
                {
                    return TCPProcessCmdResults.RESULT_OK;
                }

                // Trả về client chuỗi dữ liệu
                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, cmdData, nID);
                return TCPProcessCmdResults.RESULT_DATA;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, Global.GetDebugHelperInfo(socket), false);
            }

            return TCPProcessCmdResults.RESULT_FAILED;
        }

        #endregion NETWORKZONE
    }
}