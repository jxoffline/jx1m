using GameServer.Core.Executor;
using GameServer.KiemThe.CopySceneEvents;
using GameServer.KiemThe.Core.Activity.CardMonth;
using GameServer.KiemThe.Core.Task;
using GameServer.KiemThe.Entities;
using GameServer.KiemThe.GameDbController;
using GameServer.KiemThe.GameEvents.BaiHuTang;
using GameServer.KiemThe.GameEvents.EmperorTomb;
using GameServer.KiemThe.GameEvents.FactionBattle;
using GameServer.KiemThe.GameEvents.FengHuoLianCheng;
using GameServer.KiemThe.GameEvents.TeamBattle;
using GameServer.KiemThe.Logic.Manager.Battle;
using GameServer.Logic;
using GameServer.Server;
using GameServer.VLTK.Core.GuildManager;
using GameServer.VLTK.GameEvents.GrowTree;
using Server.Data;
using Server.Protocol;
using Server.TCP;
using Server.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Tmsk.Contract;
using static GameServer.Logic.KTMapManager;

namespace GameServer.KiemThe.Logic
{
    /// <summary>
    /// Quản lý gói tin
    /// </summary>
    public static partial class KT_TCPHandler
    {
        #region Play Game

        /// <summary>
        /// Gói tin bắt đầu vào Game hoặc kết nối lại
        /// </summary>
        /// <param name="pool"></param>
        /// <param name="nID"></param>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static TCPProcessCmdResults ProcessStartPlayGameCmd(TCPManager tcpMgr, TMSKSocket socket, TCPClientPool tcpClientPool, TCPRandKey tcpRandKey, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
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
                    LogManager.WriteLog(LogTypes.Error, string.Format("Can not find player corresponding ID, CMD={0}, Client={1}, RoleID={2}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), roleID));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// Thiết lập hướng mặc định
                client.CurrentDir = KiemThe.Entities.Direction.DOWN;

                /// Bản đồ
                GameMap map = KTMapManager.Find(client.MapCode);
                /// Thực hiện di chuyển đối tượng vào Grid
                map.Grid.MoveObject(client.PosX, client.PosY, client);
                /// Cập nhật vị trí hợp lệ
                client.LastValidPos = client.CurrentPos;

                /// Thiết lập thời gian cập nhật tiền
                long nowTicks = TimeUtil.NOW();
                Global.SetLastDBCmdTicks(client, (int)TCPGameServerCmds.CMD_DB_UPDATEMoney_CMD, nowTicks);
                Global.SetLastDBCmdTicks(client, (int)TCPGameServerCmds.CMD_DB_UPDATE_ROLE_AVARTA, nowTicks);
                Global.SetLastDBCmdTicks(client, (int)TCPGameServerCmds.CMD_DB_UPDATE_EXPLEVEL, nowTicks);

                /// Clear tầm hình của nhân vật
                client.ClearVisibleObjects(false);

                /// Gửi gói tin thông báo danh sách kỹ năng về Client
                KT_TCPHandler.SendRenewSkillList(client);
                /// Khởi tạo Buff
                client.Buffs.ExportBuffTree();

                ///// Đánh dấu không đợi chuyển Map
                //client.WaitingForChangeMap = false;

                socket.session.SetSocketTime(4);

                string strcmd = "";
                strcmd = string.Format("{0}", roleID);
                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, strcmd, nID);

                /// Nếu lần đầu vào Game
                if (client.FirstEnterPlayGameCmd)
                {
                    /// Hủy đánh dấu lần đầu vào Game
                    client.FirstEnterPlayGameCmd = false;
                    /// Thực hiện gửi thông tin tải lần đầu
                    //KT_TCPHandler.SendDownloadBonus(client);
                }

                return TCPProcessCmdResults.RESULT_DATA;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, Global.GetDebugHelperInfo(socket), false);
            }

            return TCPProcessCmdResults.RESULT_FAILED;
        }

        #endregion Play Game

        #region Init Game

        /// <summary>
        /// Hàm Định nghĩa khi nhân vật vào game
        /// </summary>
        /// <param name="pool"></param>
        /// <param name="nID"></param>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static TCPProcessCmdResults ProcessInitGameCmd(TCPManager tcpMgr, TMSKSocket socket, TCPClientPool tcpClientPool, TCPRandKey tcpRandKey, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;
            string cmdData = null;

            try
            {
                cmdData = new UTF8Encoding().GetString(data, 0, count);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Decode data faild, CMD={0}", (TCPGameServerCmds)nID));
                return TCPProcessCmdResults.RESULT_FAILED;
            }

            try
            {
                string[] fields = cmdData.Split(':');
                /// Toác
                if (fields.Length != 3)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Error Socket params count not fit CMD={0}, Client={1}, Recv={2}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), cmdData));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                string userID = fields[0];
                int roleID = Convert.ToInt32(fields[1]);
                string deviceID = fields[2];

                KPlayer client = KTPlayerManager.Find(socket);
                if (null != client)
                {
                    if (client.RoleID == roleID)
                    {
                        LogManager.WriteLog(LogTypes.Error, string.Format("Role ID đã tồn tại không cần tìm từ DB(ProcessInitGameCmd), CMD={0}, Client={1}, RoleID={2}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), roleID));
                        return TCPProcessCmdResults.RESULT_OK;
                    }
                    else
                    {
                        LogManager.WriteLog(LogTypes.Error, string.Format("Role ID không nhất quán, CMD={0}, Client={1}, RoleID={2}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), roleID));
                        return TCPProcessCmdResults.RESULT_FAILED;
                    }
                }

                /// Thông tin Socket
                IPEndPoint socketInfo = socket.RemoteEndPoint as IPEndPoint;
                /// Địa chỉ IP
                string ipAddress = socketInfo.Address.ToString();
                /// Nếu đã vượt quá số lượng kết nối
                if (GameManager.OnlineUserSession.GetClientCountsByIPAddress(ipAddress) >= ServerConfig.Instance.LimitAccountPerIPAddress)
                {
                    tcpOutPacket = DataHelper.ObjectToTCPOutPacket<RoleData>(new RoleData() { RoleID = -1 }, pool, nID);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                if (roleID < 0 || !GameManager.TestGamePerformanceMode && userID != GameManager.OnlineUserSession.FindUserID(socket))
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Plugin login, no need SocketSession, CMD={0}, Client={1}, RoleID={2}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), roleID));
                    return TCPProcessCmdResults.RESULT_OK;
                }

                // Thời gian hàng đợi đăng nhập
                int waitSecs = Global.GetSwitchServerWaitSecs(socket);
                if (waitSecs > 0)
                {
                    tcpOutPacket = DataHelper.ObjectToTCPOutPacket<RoleData>(new RoleData() { RoleID = StdErrorCode.Error_40, TotalValue = waitSecs }, pool, nID);
                    socket.session.SetSocketTime(5);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                if (socket.IsKuaFuLogin)
                {
                    //Nếu là liên sv thì trả về dữ liệu login của máy chủ liên sv
                    roleID = socket.ClientKuaFuServerLoginData.RoleId;
                }

                // Truy vấn nhân vật từ GAME DB SERVER
                byte[] bytesData = null;
                if (TCPProcessCmdResults.RESULT_FAILED == Global.TransferRequestToDBServer2(tcpMgr, socket, tcpClientPool, tcpRandKey, pool, nID, data, count, out bytesData, socket.ServerId))
                {
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                Int32 length = BitConverter.ToInt32(bytesData, 0);
                UInt16 cmd = BitConverter.ToUInt16(bytesData, 4);

                ///Chuyển đội BYTE thành các đối tượng
                RoleDataEx roleDataEx = DataHelper.BytesToObject<RoleDataEx>(bytesData, 6, length - 2);

                if (roleDataEx.RoleID < 0)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Không lấy được nhân vật từ CSDL: Cmd={0}, RoleID={1}, đóng kết nối ", (TCPGameServerCmds)nID, roleID));

                    tcpOutPacket = DataHelper.ObjectToTCPOutPacket<RoleData>(new RoleData() { RoleID = roleDataEx.RoleID }, pool, cmd);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                // Nếu không phải là máy chủ liên server
                if (!socket.IsKuaFuLogin)
                {
                    bool isKuaFuMap = KuaFuManager.getInstance().IsKuaFuMap(roleDataEx.MapCode);

                    if (isKuaFuMap)
                    {
                        KPlayer fakeClient = Global.MakeGameClientForGetRoleParams(roleDataEx);

                        /// Thông tin bản đồ lần trước
                        KT_TCPHandler.GetLastMapInfo(fakeClient, out int MapcodeOld, out int XPosOld, out int YPosOld);
                        /// Thiết lập
                        roleDataEx.MapCode = MapcodeOld;
                        roleDataEx.PosX = (int)XPosOld;
                        roleDataEx.PosY = (int)YPosOld;
                    }
                }

                //FILL RA TOÀN BỘ ĐỒ ĐẠC | CHỖ NÀY SẼ REMOVE ĐI KHI LÀM ITEM
                if (null == roleDataEx.SaleGoodsDataList)
                {
                    roleDataEx.SaleGoodsDataList = new List<GoodsData>();
                }
                if (null == roleDataEx.GoodsDataList)
                {
                    roleDataEx.GoodsDataList = new List<GoodsData>();
                }
                if (null == roleDataEx.GroupMailRecordList)
                {
                    roleDataEx.GroupMailRecordList = new List<int>();
                }

                if (!socket.IsKuaFuLogin)
                {
                    /// Bản đồ
                    GameMap _gameMap = KTMapManager.Find(roleDataEx.MapCode);
                    //Nếu bản đồ không tồn tại
                    if (_gameMap == null)
                    {
                        LogManager.WriteLog(LogTypes.Warning, string.Format("Bản đồ không tồn tại ==> chuyển về bản đồ mặc định: MapCode={0}", roleDataEx.MapCode));

                        KPlayer fakeClient = Global.MakeGameClientForGetRoleParams(roleDataEx);

                        KT_TCPHandler.GetPlayerDefaultRelivePos(fakeClient, out int MapCodeOut, out int PosXOut, out int PosYOut);

                        // FILL TỌA ĐỘ HỒI SINH VÀO DÂY
                        roleDataEx.MapCode = MapCodeOut;
                        roleDataEx.PosX = PosXOut;
                        roleDataEx.PosY = PosYOut;
                    }
                }

                /// Nếu đã vượt quá số CCU và không phải GM
                if (KTPlayerManager.GetPlayersCount() >= ServerConfig.Instance.MaxCCU && !KTGMCommandManager.IsGM(socket, roleDataEx.RoleID))
                {
                    tcpOutPacket = DataHelper.ObjectToTCPOutPacket<RoleData>(new RoleData() { RoleID = -2 }, pool, cmd);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                /// KHỞI TẠO GAMECLIENT
                KPlayer gameClient = new KPlayer()
                {
                    ClientSocket = socket,
                    RoleData = roleDataEx,
                    WaitingForChangeMap = true,
                    TeamID = -1,
                    m_eDoing = KE_NPC_DOING.do_stand,
                };
                /// Kiểm tra Avarta nếu không hợp lệ thì chọn Avarta mặc định
                if (!KRoleAvarta.IsAvartaValid(gameClient, gameClient.RolePic))
                {
                    /// Chọn 1 Avarta đầu tiên trong danh sách có giới tính phù hợp
                    RoleAvartaXML roleAvarta = KRoleAvarta.GetMyDefaultAvarta(gameClient);
                    /// Nếu tồn tại
                    if (roleAvarta != null)
                    {
                        gameClient.RolePic = roleAvarta.ID;
                    }
                }

                /// Chuyển PKMode về hòa bình
                gameClient.PKMode = (int)PKMode.Peace;

                /// Thêm kỹ năng tân thủ cho người chơi
                KTGlobal.AddNewbieAttackSkills(gameClient);

                // Khởi tạo lại danh sách TMP lưu lại vật phẩm của thằng người chơi đã bán trong phiên login đó

                gameClient.ClearOwnShop();
                /// Gọi tới khi hoàn tất quá trình tải xuống RoleDataEx
                gameClient.OnEnterGame();

                // SET LẠI DỮ LIỆU TÀI KHOẢN | THẺ THÁNG
                gameClient.strUserID = GameManager.OnlineUserSession.FindUserID(gameClient.ClientSocket);
                gameClient.strUserName = GameManager.OnlineUserSession.FindUserName(socket);
                gameClient.DeviceID = deviceID;

                /// Giao nhiệm vụ cho người chơi mới
                KTPlayerManager.GiveFirstTask(gameClient);

                /// Nếu là liên Server
                if (socket.IsKuaFuLogin)
                {
                    if (!KuaFuManager.getInstance().OnInitGame(gameClient))
                    {
                        tcpOutPacket = DataHelper.ObjectToTCPOutPacket<RoleData>(new RoleData() { RoleID = StdErrorCode.Error_Operation_Denied }, pool, cmd);
                        return TCPProcessCmdResults.RESULT_DATA;
                    }
                }

                /// Bản đồ tương ứng
                GameMap gameMap = KTMapManager.Find(roleDataEx.MapCode);

                /// Thông tin phụ bản lần tước
                KT_TCPHandler.GetCopySceneInfo(gameClient, out int copySceneID, out int copySceneCreateTicks);

                /// Nếu có phụ bản
                if (copySceneID != -1)
                {
                    /// Nếu phụ bản vẫn đang tồn tại
                    if (CopySceneEventManager.IsCopySceneExist(copySceneID, gameClient.CurrentMapCode, copySceneCreateTicks))
                    {
                        /// Thông tin phụ bản tương ứng
                        KTCopyScene copyScene = CopySceneEventManager.GetCopyScene(copySceneID, gameClient.CurrentMapCode);
                        /// Nếu phụ bản không chấp nhận cho kết nối lại
                        if (!copyScene.AllowReconnect)
                        {
                            /// Xóa thông tin phụ bản tương ứng
                            gameClient.CurrentCopyMapID = -1;
                            /// Thông tin điểm về thành gần nhất
                            KT_TCPHandler.GetPlayerDefaultRelivePos(gameClient, out int mapCode, out int posX, out int posY);
                            /// Đưa người chơi trở về bản đồ mặc định
                            gameClient.MapCode = mapCode;
                            gameClient.PosX = posX;
                            gameClient.PosY = posY;
                        }
                        else
                        {
                            /// Đưa người chơi trở lại phụ bản
                            gameClient.CurrentCopyMapID = copySceneID;
                            /// Thực hiện sự kiện Reconnect
                            CopySceneEventManager.OnPlayerReconnected(copyScene, gameClient);
                        }
                    }
                    /// Nếu phụ bản đã hết thời gian
                    else
                    {
                        /// Xóa thông tin phụ bản tương ứng
                        gameClient.CurrentCopyMapID = -1;

                        KT_TCPHandler.GetPlayerDefaultRelivePos(gameClient, out int mapCode, out int posX, out int posY);
                        /// Đưa người chơi trở về bản đồ mặc định
                        gameClient.MapCode = mapCode;
                        gameClient.PosX = posX;
                        gameClient.PosY = posY;
                        /// Xóa thông tin phụ bản cũ
                        KT_TCPHandler.UpdateCopySceneInfo(gameClient, -1, -1);
                    }
                }
                /// Nếu không tồn tại bản đồ tương ứng hoặc đây là bản đồ phụ bản
                else if (gameMap == null || gameMap.IsCopyScene)
                {
                    /// Thông tin điểm về thành gần nhất
                    KT_TCPHandler.GetPlayerDefaultRelivePos(gameClient, out int mapCode, out int posX, out int posY);
                    /// Đưa người chơi trở về bản đồ mặc định
                    gameClient.MapCode = mapCode;
                    gameClient.PosX = posX;
                    gameClient.PosY = posY;
                    /// Xóa thông tin phụ bản cũ
                    KT_TCPHandler.UpdateCopySceneInfo(gameClient, -1, -1);
                }

                /// Nếu ở Tống Kim
                if (Battel_SonJin_Manager.IsInBattle(gameClient) || GrowTreeManager.IsInEvent(gameClient) || FactionBattleManager.IsInFactionBattle(gameClient) || GuildWarCity.IsInGuildWarCity(gameClient))
                {
                    /// Thông tin điểm về thành gần nhất
                    KT_TCPHandler.GetPlayerDefaultRelivePos(gameClient, out int mapCode, out int posX, out int posY);
                    /// Đưa người chơi trở về bản đồ mặc định
                    gameClient.MapCode = mapCode;
                    gameClient.PosX = posX;
                    gameClient.PosY = posY;
                }
                /// Nếu ở bản đồ Phong Hỏa Liên Thành
                else if (FengHuoLianCheng.IsInsideFHLCMap(gameClient) && !FengHuoLianCheng.IsRegisterTime)
                {
                    /// Đẩy về thành thị
                    gameClient.MapCode = FengHuoLianCheng.Data.Map.CityMapID;
                    gameClient.PosX = FengHuoLianCheng.Data.Map.CityPosX;
                    gameClient.PosY = FengHuoLianCheng.Data.Map.CityPosY;
                }
                /// Nếu đang ở Bạch Hổ Đường
                else if (BaiHuTang.IsInBaiHuTang(gameClient))
                {
                    /// Thông tin điểm về thành gần nhất
                    KT_TCPHandler.GetPlayerDefaultRelivePos(gameClient, out int mapCode, out int posX, out int posY);
                    /// Đưa người chơi trở về bản đồ mặc định
                    gameClient.MapCode = mapCode;
                    gameClient.PosX = posX;
                    gameClient.PosY = posY;
                }
                /// Nếu đang ở Hội trường liên đấu
                else if (TeamBattle.IsInTeamBattleMap(gameClient))
                {
                    /// Thông tin điểm về thành gần nhất
                    KT_TCPHandler.GetPlayerDefaultRelivePos(gameClient, out int mapCode, out int posX, out int posY);
                    /// Đưa người chơi trở về bản đồ mặc định
                    gameClient.MapCode = mapCode;
                    gameClient.PosX = posX;
                    gameClient.PosY = posY;
                }
                /// Nếu đang ở bản đồ Tần Lăng
                else if (EmperorTomb.IsInsideEmperorTombMap(gameClient))
                {
                    /// Thông tin điểm về thành gần nhất
                    KT_TCPHandler.GetPlayerDefaultRelivePos(gameClient, out int mapCode, out int posX, out int posY);
                    /// Đưa người chơi trở về bản đồ mặc định
                    gameClient.MapCode = mapCode;
                    gameClient.PosX = posX;
                    gameClient.PosY = posY;
                }

                ///// Nếu không phải ở bản đồ bí cảnh
                //if (gameClient.CurrentMapCode != MiJing.Map.ID)
                //{
                //    /// Xóa Buff x2 kinh nghiệm bí cảnh
                //    gameClient.Buffs.RemoveBuff(MiJing.DoubleExpBuff);
                //}

                /// Cập nhật vị trí hợp lệ
                gameClient.LastValidPos = gameClient.CurrentPos;

                /// INIT TOÀN BỘ THUỘC TÍNH PHỤ CỦA NGƯỜI CHƠI
                KTGlobal.InitRoleLoginPrams(gameClient);

                /// Lấy thông tin Bách Bảo Rương
                gameClient.ReadSeashellCircleParamFromDB();

                /// Lấy thông tin Chúc phúc
                gameClient.ReadPrayDataFromDB();

                /// Lấy thông tin Tu Luyện
                int xiuLianZhu_Exp = Global.GetRoleParamsInt32FromDB(gameClient, RoleParamName.XiuLianZhu);
                /// BUG
                if (xiuLianZhu_Exp > 500000)
                {
                    xiuLianZhu_Exp = 500000;
                }
                gameClient.XiuLianZhu_Exp = xiuLianZhu_Exp;
                gameClient.XiuLianZhu_TotalTime = Global.GetRoleParamsInt32FromDB(gameClient, RoleParamName.XiuLianZhu_TotalTime);

                // set -1 BEFORE
                // gameClient.CurenQuestIDBVD = -1;

                //Fix nhiệm vụ chính tuyến cho bọn bị lỗi
                TaskManager.getInstance().FixMainTaskID(gameClient);

                // Give nhiệm vụ dã tẩu
                TaskDailyArmyManager.getInstance().GiveTaskArmyDaily(gameClient);

                PirateTaskManager.getInstance().GiveTaskPirate(gameClient);
                // Kiểm tra xem đã hết hạn thẻ tháng chưa
                CardMonthManager.CheckValid(gameClient);
                    

                // Code xử lý toàn bộ phần phúc lợi
                Global.UpdateWelfareRole(gameClient);

                // Lưu lại số lần login
                Global.UpdateRoleLoginRecord(gameClient);

                // Lấy ra ngày hiện tại
                int dayID = TimeUtil.NowDateTime().DayOfYear;

                //Lọc tất cả các task không hợp lệ
                TaskManager.RemoveAllInvalidTasks(gameClient);

                //Xử lý các task còn đang dang dở hoặc từ bỏ
                TaskManager.ProcessTaskData(gameClient);

                /// Kiểm tra các vật phẩm hợp lệ
                gameClient.GoodsData.Validate();

                // Thực hiện attak toàn bộ đồ đang sử dụng vào Dict
                gameClient.GetPlayEquipBody().InitEquipBody();

                ///Attack toàn bộ thuộc tính bang hội vào người chơi này
                GuildManager.AttackEffectGuildSkill(gameClient);

                /// Thực hiện add người chơi vào bộ quản lý
                KTPlayerManager.Add(gameClient);


                // Khởi tạo tên người chơi nếu đang ở máy chủ liên server
                string roleName;
                if (socket.IsKuaFuLogin)
                {
                    roleName = KTGlobal.FormatRoleNameWithZoneId(gameClient);

                    gameClient.GetRoleData().RoleName = roleName;
                }
                else
                {
                    roleName = gameClient.RoleName;
                }

                //Ghi lại nhật ký login
                KTGlobal.AddRoleLoginEvent(gameClient);

                //Nếu số làn login < 0 thì ghi lại số lần offline của người chơi
                if (gameClient.LoginNum <= 0)
                {
                    double currSec = KTGlobal.GetOffsetSecond(TimeUtil.NowDateTime());
                    GameDb.UpdateRoleParamByName(gameClient, RoleParamName.CallPetFreeTime, currSec.ToString(), true);
                }

                /// Lấy thông tin ngựa tương ứng ở trang bị
                GoodsData horseGD = gameClient.GoodsData.Find(x => x.Using == (int)KE_EQUIP_POSITION.emEQUIPPOS_HORSE);
                /// Nếu không có ngựa
                if (horseGD == null)
                {
                    /// Hủy trạng thái cưỡi
                    gameClient.IsRiding = false;
                }

                /// Thêm trạng thái bảo hộ
                gameClient.SendChangeMapProtectionBuff();

                /// Nếu máu, mana, thể lực = 0 hết thì BUG gì đó
                if (gameClient.m_CurrentLife <= 0)
                {
                    gameClient.m_CurrentLife = gameClient.m_CurrentLifeMax;
                    gameClient.m_CurrentMana = gameClient.m_CurrentManaMax;
                    gameClient.m_CurrentStamina = gameClient.m_CurrentStaminaMax;
                }
                gameClient.m_eDoing = KE_NPC_DOING.do_stand;

                //FILL DỮ LIỆU CHO ROLEDATA
                RoleData roleData = KTGlobal.GetMyselfRoleData(gameClient);

                ///// Ghi LOG lại tài phú
                //LogManager.WriteLog(LogTypes.Analysis, string.Format("{0} (ID: {1}), TotalValue = {2}", gameClient.RoleName, gameClient.RoleID, gameClient.GetTotalValue()));

                roleData.RoleName = roleName;
                ////Thu dọn lại ba lô cho người chơi
                //if (GameManager.Flag_OptimizationBagReset)
                //{
                //    Global.ResetBagAllGoods(gameClient, false);
                //}

                socket.session.SetSocketTime(3);

                /// Thêm vào danh sách IP
                GameManager.OnlineUserSession.AddClientToIPAddressList(gameClient);

                tcpOutPacket = DataHelper.ObjectToTCPOutPacket<RoleData>(roleData, pool, cmd);
                return TCPProcessCmdResults.RESULT_DATA;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, Global.GetDebugHelperInfo(socket), false);
            }

            return TCPProcessCmdResults.RESULT_FAILED;
        }

        #endregion Init Game

        #region Get Role List

        /// <summary>
        /// Trả về danh sách nhân vật trong tài khoản tương ứng
        /// </summary>
        /// <param name="pool"></param>
        /// <param name="nID"></param>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static TCPProcessCmdResults ProcessGetRoleListCmd(TCPManager tcpMgr, TMSKSocket socket, TCPClientPool tcpClientPool, TCPRandKey tcpRandKey, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;

            try
            {
                string cmdData = new UTF8Encoding().GetString(data, 0, count);
                string[] fields = cmdData.Split(':');
                if (fields.Length < 2)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Error Socket params count not fit CMD={0}, Recv={1}, CmdData={2}",
                        (TCPGameServerCmds)nID, fields.Length, cmdData));

                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                string userID = fields[0];
                int zoneID = Convert.ToInt32(fields[1]);

                if (!GameManager.TestGamePerformanceMode && userID != GameManager.OnlineUserSession.FindUserID(socket))
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("No SocketSession, CMD={0}, Client={1}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket)));
                    return TCPProcessCmdResults.RESULT_OK;
                }

                socket.session.SetSocketTime(0);
                socket.session.SetSocketTime(2);
                return Global.TransferRequestToDBServer(tcpMgr, socket, tcpClientPool, tcpRandKey, pool, nID, data, count, out tcpOutPacket, socket.ServerId);
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, Global.GetDebugHelperInfo(socket), false);
            }

            return TCPProcessCmdResults.RESULT_FAILED;
        }

        #endregion Get Role List

        #region Get Newbie villages

        /// <summary>
        /// Lấy danh sách Tân Thủ Thôn
        /// </summary>
        /// <param name="pool"></param>
        /// <param name="nID"></param>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static TCPProcessCmdResults PrecessGetNewbieVillages(TCPManager tcpMgr, TMSKSocket socket, TCPClientPool tcpClientPool, TCPRandKey tcpRandKey, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;

            try
            {
                string cmdData = new UTF8Encoding().GetString(data, 0, count);
                string[] fields = cmdData.Split(':');
                if (fields.Length != 2)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Error Socket params count not fit CMD={0}, Recv={1}, CmdData={2}",
                        (TCPGameServerCmds)nID, fields.Length, cmdData));

                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                /// UserID
                string userID = fields[0];
                /// UserName
                string userName = fields[1];

                if (!GameManager.TestGamePerformanceMode && userID != GameManager.OnlineUserSession.FindUserID(socket))
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Plugin login, no need SocketSession, CMD={0}, Client={1}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket)));
                    return TCPProcessCmdResults.RESULT_OK;
                }

                List<int> villages = new List<int>();
                foreach (NewbieVillage village in KTGlobal.NewbieVillages)
                {
                    villages.Add(village.ID);
                }
                string strcmd = string.Join(":", villages);
                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, strcmd, nID);
                return TCPProcessCmdResults.RESULT_DATA;
            }
            catch (Exception ex)
            {
                //System.Windows.Application.Current.Dispatcher.Invoke((MethodInvoker)delegate
                //{
                // 格式化异常错误信息
                DataHelper.WriteFormatExceptionLog(ex, Global.GetDebugHelperInfo(socket), false);
                //throw ex;
                //});
            }

            return TCPProcessCmdResults.RESULT_FAILED;
        }

        #endregion Get Newbie villages

        #region Create Role

        /// <summary>
        /// Xử lý packet tạo nhân vật
        /// </summary>
        /// <param name="pool"></param>
        /// <param name="nID"></param>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static TCPProcessCmdResults ProcessCreateRoleCmd(TCPManager tcpMgr, TMSKSocket socket, TCPClientPool tcpClientPool, TCPRandKey tcpRandKey, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;

            try
            {
                string cmdData = new UTF8Encoding().GetString(data, 0, count);
                string[] fields = cmdData.Split(':');
                if (fields.Length != 6)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Error Socket params count not fit CMD={0}, Recv={1}, CmdData={2}", (TCPGameServerCmds)nID, fields.Length, cmdData));

                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                /// UserID
                string userID = fields[0];
                /// UserName
                string userName = fields[1];
                /// Giới tính
                int sex = Convert.ToInt32(fields[2]);
                /// ID môn phái
                int factionID = Convert.ToInt32(fields[3]);
                /// Tên và loại thiết bị
                string name = fields[4];
                /// ID máy chủ
                int serverID = Convert.ToInt32(fields[5]);

                NewbieVillage village = KTGlobal.NewbieVillages.FirstOrDefault();
                string positionInfo = string.Format("{0},{1},{2},{3}", village.ID, 1, village.Position.X, village.Position.Y);
                cmdData += ":" + village.ID + ":" + positionInfo;
                data = new UTF8Encoding().GetBytes(cmdData);

                /// ID thiết bị
                string deviceID = socket.DeviceID;

                //if (!GameManager.TestGamePerformanceMode && userID != GameManager.OnlineUserSession.FindUserID(socket))
                //{
                //    LogManager.WriteLog(LogTypes.Error, string.Format("Plugin login, no need SocketSession, CMD={0}, Client={1}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket)));
                //    return TCPProcessCmdResults.RESULT_OK;
                //}

                string strcmd = "";
                if (socket.IsKuaFuLogin || sex < 0 || sex > 1 || factionID != 0)
                {
                    strcmd = string.Format("{0}:{1}", -12, string.Format("{0}${1}${2}${3}${4}${5}", "", "", "", "", "", ""));
                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, strcmd, nID);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                /// Tên không hợp lệ
                if (!Utils.CheckValidString(name))
                {
                    strcmd = string.Format("{0}:{1}", -3, string.Format("{0}${1}${2}${3}${4}${5}", "", "", "", "", "", ""));
                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, strcmd, nID);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                /// Kiểm tra độ dài tên hợp lệ không
                if (!KTChangeNameManager.Instance.IsNameLengthOK(name))
                {
                    strcmd = string.Format("{0}:{1}", -3, string.Format("{0}${1}${2}${3}${4}${5}", "", "", "", "", "", ""));
                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, strcmd, nID);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                TCPProcessCmdResults result = Global.TransferRequestToDBServer(tcpMgr, socket, tcpClientPool, tcpRandKey, pool, nID, data, count, out tcpOutPacket, socket.ServerId);
                if (null != tcpOutPacket)
                {
                    tcpOutPacket.GetPacketCmdData(out _);
                }
                return result;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, Global.GetDebugHelperInfo(socket), false);
            }

            return TCPProcessCmdResults.RESULT_FAILED;
        }

        #endregion Create Role

        #region Login

        /// <summary>
        /// Mã SHA
        /// </summary>
        public static string KeySHA1 { get; set; } = "abcde";

        /// <summary>
        /// Mã khóa
        /// </summary>
        public static string KeyData { get; set; } = "12345";

        /// <summary>
        /// Mã ở Web
        /// </summary>
        public static string WebKey { get; set; } = "12345";

        /// <summary>
        /// Mã Web local
        /// </summary>
        public static string WebKeyLocal { get; set; } = "12345";

        /// <summary>
        /// Thời gian hết hạn phiên đăng nhập
        /// </summary>
        public static long MaxTicks { get; set; } = (60L * 60L * 24 * 1000L * 10000L);

        /// <summary>
        /// Gói tin đăng nhập hệ thống
        /// </summary>
        /// <param name="pool"></param>
        /// <param name="nID"></param>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static TCPProcessCmdResults ProcessUserLogin2Cmd(TCPManager tcpMgr, TMSKSocket socket, TCPClientPool tcpClientPool, TCPRandKey tcpRandKey, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
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
                if (fields.Length != 6)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Error Socket params count not fit, CMD={0}, Client={1}, Recv={2}",
                        (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), cmdData));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                int verSign = 0;
                string userID = fields[1];
                string userName = fields[2];
                string lastTime = fields[3];
                string isadult = fields[4];
                string signCode = fields[5].ToLower();

                if (!int.TryParse(fields[0], out verSign))
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("ProcessUserLogin2Cmd, verSign={0} userID={1}", fields[0], userID));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                string key = KT_TCPHandler.WebKey;
                key = "9377(*)#mst9";
                string strVal = userID + userName + lastTime + isadult + key;
                string strMD5 = MD5Helper.get_md5_string(strVal).ToLower();
                if (strMD5 != signCode)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Sign code check failed, CMD={0}, Client={1}, UserID={2}, IsAdult={3}, LastTime={4}, SignCode={5}",
                        (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), userID, isadult, lastTime, signCode));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                if (!KuaFuManager.getInstance().OnUserLogin2(socket, verSign, userID, userName, lastTime, isadult, signCode))
                {
                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, string.Format("{0}:{1}:{2}:{3}", StdErrorCode.Error_Connection_Disabled, "", "", ""), (int)TCPGameServerCmds.CMD_LOGIN_ON2);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                string strcmd = "";
                string clientIPPort = Global.GetSocketRemoteEndPoint(socket);
                if (clientIPPort.IndexOf("127.0.0.1") < 0 && GameManager.GM_NoCheckTokenTimeRemainMS <= 0)
                {
                    int oldLastTime = Convert.ToInt32(lastTime);
                    int nowSecs = DataHelper.UnixSecondsNow();

                    if (nowSecs - oldLastTime >= (60 * 60 * 24))
                    {
                        strcmd = string.Format("{0}:{1}:{2}:{3}", StdErrorCode.Error_Token_Expired, "", "", "");
                        tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, strcmd, (int)TCPGameServerCmds.CMD_LOGIN_ON2);
                        return TCPProcessCmdResults.RESULT_DATA;
                    }
                }

                if (verSign != (int)TCPCmdProtocolVer.VerSign)
                {
                    strcmd = string.Format("{0}:{1}:{2}:{3}", StdErrorCode.Error_Version_Not_Match, "", "", "");
                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, strcmd, (int)TCPGameServerCmds.CMD_LOGIN_ON2);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                UserLoginToken ult = new UserLoginToken()
                {
                    UserID = userID,
                    RandomPwd = tcpRandKey.GetKey()
                };

                string userToken = ult.GetEncryptString(KeySHA1, KeyData);
                strcmd = string.Format("{0}:{1}:{2}:{3}", userID, userName, userToken, isadult);

                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, strcmd, (int)TCPGameServerCmds.CMD_LOGIN_ON2);
                return TCPProcessCmdResults.RESULT_DATA;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, Global.GetDebugHelperInfo(socket), false);
            }

            return TCPProcessCmdResults.RESULT_FAILED;
        }

        /// <summary>
        /// Thực hiện Cho USER LOGIN
        /// </summary>
        /// <param name="pool"></param>
        /// <param name="nID"></param>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static TCPProcessCmdResults ProcessUserLoginCmd(TCPManager tcpMgr, TMSKSocket socket, TCPClientPool tcpClientPool, TCPRandKey tcpRandKey, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
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
                if (fields.Length != 6 && fields.Length != 12 && fields.Length != 13)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Error Socket params count not fit CMD={0}, Client={1}, Recv={2}",
                        (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), cmdData));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                string userID = fields[0];
                string userName = fields[1];
                string userToken = fields[2];

                LogManager.WriteLog(LogTypes.Analysis, "Login Token: " + userToken);

                int roleRandToken = Convert.ToInt32(fields[3]);
                int verSign = Convert.ToInt32(fields[4]);
                int userIsAdult = Convert.ToInt32(fields[5]);

                string strcmd = "";

                /// Đánh dấu có phải GM không
                socket.session.IsGM = KTGMCommandManager.IsGM(socket, Convert.ToInt32(fields[7]));


                if ((tcpMgr.MySocketListener.ConnectedSocketsCount - 1) >= (tcpMgr.MaxConnectedClientLimit + (tcpMgr.MaxConnectedClientLimit / 20)))
                {
                    if (!socket.session.IsGM)
                    {
                        LogManager.WriteLog(LogTypes.Error, string.Format("Already max CCU, can not login, CMD={0}, Client={1}, UserID={2}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), userID));

                        strcmd = string.Format("{0}", StdErrorCode.Error_Server_Connections_Limit);
                        tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, strcmd, (int)TCPGameServerCmds.CMD_LOGIN_ON);
                        return TCPProcessCmdResults.RESULT_DATA;
                    }
                }

                if (verSign != (int)TCPCmdProtocolVer.VerSign)
                {
                    strcmd = string.Format("{0}", StdErrorCode.Error_Version_Not_Match2);
                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, strcmd, (int)TCPGameServerCmds.CMD_LOGIN_ON);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                bool verified = true;
                int RandPwd = -1;
                UserLoginToken ult = new UserLoginToken();
                int verifyResult = ult.SetEncryptString(userToken, KeySHA1, KeyData, MaxTicks);
                if (verifyResult < 0)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Login Token is not correct, CMD={0}, Client={1}, VerifyResult={2}",
                        (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), verifyResult));
                    verified = false;
                }
                else
                {
                    userID = ult.UserID;
                    RandPwd = ult.RandomPwd;
                    if (!tcpRandKey.FindKey(RandPwd))
                    {
                        verified = false;
                    }
                }

                if (fields.Length >= 12)
                {
                    int roleId = 0;
                    long gameId = 0;
                    int gameType = 0;
                    int serverId = 0;
                    string ip = "";
                    int port = 0;

                    if (fields.Length == 13)
                    {
                        socket.DeviceID = fields[6];
                        roleId = Convert.ToInt32(fields[7]);
                        gameId = Convert.ToInt64(fields[8]);
                        gameType = Convert.ToInt32(fields[9]);
                        serverId = Convert.ToInt32(fields[10]);
                        ip = fields[11];
                        port = Convert.ToInt32(fields[12]);
                    }
                    else
                    {
                        roleId = Convert.ToInt32(fields[6]);
                        gameId = Convert.ToInt64(fields[7]);
                        gameType = Convert.ToInt32(fields[8]);
                        serverId = Convert.ToInt32(fields[9]);
                        ip = fields[10];
                        port = Convert.ToInt32(fields[11]);
                    }

                    string lastTime = DataHelper.UnixSecondsNow().ToString();
                    string strVal = userID + userName + lastTime + userIsAdult + WebKey;
                    string signCode = MD5Helper.get_md5_string(strVal).ToLower();

                    bool result = KuaFuManager.getInstance().OnUserLogin(socket, verSign, userID, userName, lastTime, userToken, userIsAdult.ToString(), signCode, serverId, ip, port, roleId, gameType, gameId);
                    if (!result)
                    {
                        string strResult = string.Format("{0}:{1}:{2}:{3}", StdErrorCode.Error_Redirect_Orignal_Server, "", "", "");
                        tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, strResult, (int)TCPGameServerCmds.CMD_LOGIN_ON);
                        return TCPProcessCmdResults.RESULT_DATA;
                    }
                }

                if (socket.ServerId == 0)
                {
                    socket.ServerId = GameManager.ServerId;
                }

                bool alreadyOnline = false;

                if (!verified)
                {
                    strcmd = string.Format("{0}", StdErrorCode.Error_Token_Expired);
                }
                else
                {
                    if (!GameManager.OnlineUserSession.AddSession(socket, userID))
                    {
                        strcmd = string.Format("{0}", StdErrorCode.Error_Connection_Closing2);

                        LogManager.WriteLog(LogTypes.Error, string.Format("Already login, Client={0}, UserName={1}", Global.GetSocketRemoteEndPoint(socket), userName));

                        alreadyOnline = true;
                    }
                    else
                    {
                        int regUserID = GameDb.RegisterUserIDToDBServer(userID, 1, socket.ServerId, ref socket.session.LastLogoutServerTicks);
                        if (regUserID <= 0)
                        {
                            strcmd = string.Format("{0}", StdErrorCode.Error_Connection_Closing2);

                            LogManager.WriteLog(LogTypes.Error, string.Format("Tài khoản đã có người khác đăng ký，Client={0}, UserName={1}", Global.GetSocketRemoteEndPoint(socket), userName));

                            alreadyOnline = true;
                        }
                        else
                        {
                            /// Thêm phiên đăng nhập tương ứng
                            GameManager.OnlineUserSession.AddUserName(socket, userName);

                            int waitSecs = Global.GetSwitchServerWaitSecs(socket);
                            strcmd = string.Format("{0}:{1}", tcpRandKey.GetKey(), waitSecs);
                        }
                    }
                }

                bool bHasOtherSocket = false;
                /// Nếu đang đăng nhập nơi khác
                if (alreadyOnline)
                {
                    TMSKSocket clientSocket = GameManager.OnlineUserSession.FindSocketByUserName(userName);
                    if (null != clientSocket)
                    {
                        bHasOtherSocket = true;
                        if (clientSocket == socket)
                        {
                            LogManager.WriteLog(LogTypes.Error, string.Format("Người dùng đã đăng nhập sẽ gửi lại hướng dẫn đăng nhập để đóng kết nối Client ={0}, UserName={1}", Global.GetSocketRemoteEndPoint(socket), userName));
                            return TCPProcessCmdResults.RESULT_FAILED;
                        }

                        KPlayer otherClient = KTPlayerManager.Find(clientSocket);
                        if (null == otherClient)
                        {
                            Global.ForceCloseSocket(clientSocket, "Other client NULL");
                        }
                        else
                        {
                            Global.ForceCloseClient(otherClient, "Rejected");
                        }
                    }
                    else
                    {
                        string gmCmdData = string.Format("-kicku {0} {1} {2}", userName, GameManager.ServerLineID, TimeUtil.NowRealTime());

                        Global.ExecuteDBCmd((int)TCPGameServerCmds.CMD_SPR_CHAT,
                            string.Format("{0}:{1}:{2}:{3}:{4}:{5}:{6}:{7}:{8}", 0, "", 0, "", 0, gmCmdData, 0, 0, GameManager.ServerLineIdAllLineExcludeSelf),
                             socket.ServerId);
                    }

                    LogManager.WriteLog(LogTypes.Error, string.Format("Account is already online, disconnect the last one..., Client={0}, UserName={1}", Global.GetSocketRemoteEndPoint(socket), userName));
                }

                socket.session.SetSocketTime(1);

                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, strcmd, (int)TCPGameServerCmds.CMD_LOGIN_ON);
                return TCPProcessCmdResults.RESULT_DATA;
            }
            catch (Exception ex)
            {
                //LogManager.WriteLog(LogTypes.Exception, ex.ToString());
            }

            return TCPProcessCmdResults.RESULT_FAILED;
        }

        #endregion Login

        #region Client heart

        /// <summary>
        /// Xử lý phản hồi PING từ Client
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
        public static TCPProcessCmdResults ProcessSpriteClientHeartCmd(TCPManager tcpMgr, TMSKSocket socket, TCPClientPool tcpClientPool, TCPRandKey tcpRandKey, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;
            SCClientHeart cmdData = null;

            try
            {
                cmdData = DataHelper.BytesToObject<SCClientHeart>(data, 0, count);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Decode data faild, CMD={0}, Client={1}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket)));
                return TCPProcessCmdResults.RESULT_FAILED;
            }

            try
            {
                int roleID = cmdData.RoleID;
                int roleRandToken = cmdData.RandToken;

                /// Người chơi tương ứng
                KPlayer client = KTPlayerManager.Find(socket);
                if (null == client || client.RoleID != roleID)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Client not found, CMD={0}, Client={1}, RoleID={2}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), roleID));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// Kiểm tra Random Token có hợp lệ không
                if (!tcpRandKey.FindKey(roleRandToken))
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Random token check faild, CMD={0}, Client={1}, RoleID={2}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), roleID));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                long nowTicks = KTGlobal.GetCurrentTimeMilis();
                client.LastClientHeartTicks = nowTicks;
                client.ClientHeartCount++;

                SCClientHeart scData = new SCClientHeart()
                {
                    RoleID = roleID,
                    RandToken = roleRandToken,
                    Ticks = KTGlobal.GetCurrentTimeMilis(),
                };
                client.SendPacket((int)TCPGameServerCmds.CMD_SPR_CLIENTHEART, scData);

                return TCPProcessCmdResults.RESULT_DATA;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, Global.GetDebugHelperInfo(socket), false);
            }

            return TCPProcessCmdResults.RESULT_FAILED;
        }

        #endregion Client heart

        #region Second password

        /// <summary>
        /// Xử lý yêu cầu từ Client nhập mật khẩu cấp 2
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
        public static TCPProcessCmdResults ResponseDoSecondPasswordCmd(TCPManager tcpMgr, TMSKSocket socket, TCPClientPool tcpClientPool, TCPRandKey tcpRandKey, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;
            string cmdData;

            try
            {
                cmdData = new ASCIIEncoding().GetString(data);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Error while getting DATA, CMD={0}, Client={1}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket)));
                return TCPProcessCmdResults.RESULT_FAILED;
            }

            try
            {
                string[] fields = cmdData.Split(':');
                if (fields.Length < 1)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Error Socket params count not fit CMD={0}, Client={1}, Recv={2}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), cmdData.Length));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                KPlayer client = KTPlayerManager.Find(socket);
                if (null == client)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Can not find player, CMD={0}, Client={1}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket)));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// Loại thao tác
                int type = int.Parse(fields[0]);
                /// Loại
                switch (type)
                {
                    /// Mở khung
                    case 0:
                        {
                            /// Toác
                            if (fields.Length != 2)
                            {
                                LogManager.WriteLog(LogTypes.Error, string.Format("Error Socket params count not fit CMD={0}, Client={1}, Recv={2}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), cmdData.Length));
                                return TCPProcessCmdResults.RESULT_FAILED;
                            }

                            /// ID khung
                            int frameID = int.Parse(fields[1]);
                            /// Loại khung là gì
                            switch (frameID)
                            {
                                /// Mở khung quản lý
                                case 0:
                                    {
                                        /// Yêu cầu nhập trước
                                        if (client.NeedToShowInputSecondPassword())
                                        {
                                            return TCPProcessCmdResults.RESULT_OK;
                                        }
                                        /// Nếu chưa có mật khẩu
                                        else if (string.IsNullOrEmpty(client.SecondPassword))
                                        {
                                            /// Yêu cầu mở khung thiết lập
                                            KT_TCPHandler.SendOpenSetSecondPassword(client);
                                            break;
                                        }
                                        /// Hiện yêu cầu mở khung
                                        KT_TCPHandler.SendOpenSecondPasswordManager(client);
                                        break;
                                    }
                                /// Nhập khóa an toàn
                                case 1:
                                    {
                                        /// Nếu đã nhập rồi thì thôi
                                        if (client.IsSecondPasswordInput)
                                        {
                                            return TCPProcessCmdResults.RESULT_OK;
                                        }
                                        /// Nếu chưa có mật khẩu
                                        else if (string.IsNullOrEmpty(client.SecondPassword))
                                        {
                                            /// Yêu cầu mở khung thiết lập
                                            KT_TCPHandler.SendOpenSetSecondPassword(client);
                                            break;
                                        }
                                        /// Hiện yêu cầu mở khung
                                        KT_TCPHandler.SendOpenInputSecondPassword(client);
                                        break;
                                    }
                                /// Đổi khóa an toàn
                                case 2:
                                    {
                                        /// Yêu cầu nhập trước
                                        if (client.NeedToShowInputSecondPassword())
                                        {
                                            return TCPProcessCmdResults.RESULT_OK;
                                        }
                                        /// Nếu chưa có mật khẩu
                                        else if (string.IsNullOrEmpty(client.SecondPassword))
                                        {
                                            /// Yêu cầu mở khung thiết lập
                                            KT_TCPHandler.SendOpenSetSecondPassword(client);
                                            break;
                                        }
                                        /// Hiện yêu cầu mở khung
                                        KT_TCPHandler.SendOpenChangeSecondPassword(client);
                                        break;
                                    }
                            }

                            break;
                        }
                    /// Nhập
                    case 1:
                        {
                            /// Toác
                            if (fields.Length != 2)
                            {
                                LogManager.WriteLog(LogTypes.Error, string.Format("Error Socket params count not fit CMD={0}, Client={1}, Recv={2}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), cmdData.Length));
                                return TCPProcessCmdResults.RESULT_FAILED;
                            }

                            /// Mật khẩu
                            string secondPassword = fields[1];
                            /// Nếu không thỏa mãn
                            if (string.IsNullOrEmpty(secondPassword) || secondPassword.Length != 8 || !int.TryParse(secondPassword, out _))
                            {
                                /// Tăng số lần nhập sai lên
                                client.TotalInputIncorrectSecondPasswordTimes++;
                                /// Nếu quá 10 lần
                                if (client.TotalInputIncorrectSecondPasswordTimes >= 10)
                                {
                                    /// Thông báo
                                    KTPlayerManager.ShowNotification(client, "Bạn đã nhập sai khóa an toàn quá 10 lần. Ngày mai mới có thể nhập lại.");
                                }
                                else
                                {
                                    /// Thông báo
                                    KTPlayerManager.ShowNotification(client, string.Format("Mật mã không chính xác. Bạn đã nhập sai {0}/10 lần trong ngày!", client.TotalInputIncorrectSecondPasswordTimes));
                                }
                                /// Toác
                                return TCPProcessCmdResults.RESULT_OK;
                            }
                            /// Nếu không đúng
                            else if (client.SecondPassword != secondPassword)
                            {
                                /// Tăng số lần nhập sai lên
                                client.TotalInputIncorrectSecondPasswordTimes++;
                                /// Nếu quá 10 lần
                                if (client.TotalInputIncorrectSecondPasswordTimes >= 10)
                                {
                                    /// Thông báo
                                    KTPlayerManager.ShowNotification(client, "Bạn đã nhập sai khóa an toàn quá 10 lần. Ngày mai mới có thể nhập lại.");
                                }
                                else
                                {
                                    /// Thông báo
                                    KTPlayerManager.ShowNotification(client, string.Format("Mật mã không chính xác. Bạn đã nhập sai {0}/10 lần trong ngày!", client.TotalInputIncorrectSecondPasswordTimes));
                                }
                                /// Toác
                                return TCPProcessCmdResults.RESULT_OK;
                            }

                            /// Reset số lần đã nhập sai
                            client.TotalInputIncorrectSecondPasswordTimes = 0;
                            /// Thiết lập đã nhập khóa an toàn
                            client.IsSecondPasswordInput = true;
                            /// Thông báo
                            KTPlayerManager.ShowNotification(client, "Mở khóa thành công!");
                            break;
                        }
                    /// Đổi
                    case 2:
                        {
                            /// Toác
                            if (fields.Length != 4)
                            {
                                LogManager.WriteLog(LogTypes.Error, string.Format("Error Socket params count not fit CMD={0}, Client={1}, Recv={2}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), cmdData.Length));
                                return TCPProcessCmdResults.RESULT_FAILED;
                            }

                            /// Mật khẩu cũ
                            string oldPassword = fields[1];
                            /// Mật khẩu mới
                            string newPassword = fields[2];
                            /// Xác nhân mật khẩu mới
                            string reinputNewPassword = fields[3];

                            /// Nếu không thỏa mãn
                            if (string.IsNullOrEmpty(oldPassword) || oldPassword.Length != 8 || !int.TryParse(oldPassword, out _))
                            {
                                /// Tăng số lần nhập sai lên
                                client.TotalInputIncorrectSecondPasswordTimes++;
                                /// Nếu quá 10 lần
                                if (client.TotalInputIncorrectSecondPasswordTimes >= 10)
                                {
                                    /// Thông báo
                                    KTPlayerManager.ShowNotification(client, "Bạn đã nhập sai khóa an toàn quá 10 lần. Ngày mai mới có thể nhập lại.");
                                }
                                else
                                {
                                    /// Thông báo
                                    KTPlayerManager.ShowNotification(client, string.Format("Mật mã không chính xác. Bạn đã nhập sai {0}/10 lần trong ngày!", client.TotalInputIncorrectSecondPasswordTimes));
                                }
                                /// Toác
                                return TCPProcessCmdResults.RESULT_OK;
                            }
                            /// Nếu không đúng
                            else if (client.SecondPassword != oldPassword)
                            {
                                /// Tăng số lần nhập sai lên
                                client.TotalInputIncorrectSecondPasswordTimes++;
                                /// Nếu quá 10 lần
                                if (client.TotalInputIncorrectSecondPasswordTimes >= 10)
                                {
                                    /// Thông báo
                                    KTPlayerManager.ShowNotification(client, "Bạn đã nhập sai khóa an toàn quá 10 lần. Ngày mai mới có thể nhập lại.");
                                }
                                else
                                {
                                    /// Thông báo
                                    KTPlayerManager.ShowNotification(client, string.Format("Mật mã không chính xác. Bạn đã nhập sai {0}/10 lần trong ngày!", client.TotalInputIncorrectSecondPasswordTimes));
                                }
                                /// Toác
                                return TCPProcessCmdResults.RESULT_OK;
                            }
                            /// Nếu mật khẩu mới không thỏa mãn
                            else if (string.IsNullOrEmpty(newPassword) || newPassword.Length != 8 || !int.TryParse(newPassword, out _))
                            {
                                /// Thông báo
                                KTPlayerManager.ShowNotification(client, "Mật mã mới nhập không thỏa mãn. Hãy nhập lại!");
                                /// Toác
                                return TCPProcessCmdResults.RESULT_OK;
                            }
                            /// Nếu mật khẩu mới không khớp
                            else if (newPassword != reinputNewPassword)
                            {
                                /// Thông báo
                                KTPlayerManager.ShowNotification(client, "Mật mã mới nhập không khớp. Hãy nhập lại!");
                                /// Toác
                                return TCPProcessCmdResults.RESULT_OK;
                            }

                            /// Cập nhật
                            client.SecondPassword = newPassword;
                            /// Đánh dấu đã nhập
                            client.IsSecondPasswordInput = true;
                            /// Reset số lượt đã nhập sai
                            client.TotalInputIncorrectSecondPasswordTimes = 0;
                            /// Gửi yêu cầu lên GameDB
                            if (!KT_TCPHandler.SendDBUpdateSecondPassword(client))
                            {
                                /// Rollback mật khẩu
                                client.SecondPassword = oldPassword;
                                /// Thông báo
                                KTPlayerManager.ShowNotification(client, "Không thể lưu mật mã mới. Hãy thử thao tác lại!");
                                /// Toác
                                return TCPProcessCmdResults.RESULT_OK;
                            }

                            /// Thông báo
                            KTPlayerManager.ShowNotification(client, "Thay đổi khóa an toàn thành công!");

                            break;
                        }
                    /// Xóa
                    case 3:
                        {
                            /// Toác
                            if (fields.Length != 1)
                            {
                                LogManager.WriteLog(LogTypes.Error, string.Format("Error Socket params count not fit CMD={0}, Client={1}, Recv={2}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), cmdData.Length));
                                return TCPProcessCmdResults.RESULT_FAILED;
                            }

                            /// Nếu đã có yêu cầu xóa thì thôi
                            if (client.RequestRemoveSecondPasswordTicks != -1)
                            {
                                /// Thông báo
                                KTPlayerManager.ShowNotification(client, "Đã yêu cầu xóa khóa an toàn, hãy đợi hết thời gian khóa sẽ tự động bị xóa!");
                                /// Toác
                                return TCPProcessCmdResults.RESULT_OK;
                            }

                            /// Yêu cầu
                            client.RequestRemoveSecondPasswordTicks = (int)KTGlobal.GetOffsetSecond(DateTime.Now);

                            /// Thông báo
                            KTPlayerManager.ShowNotification(client, "Yêu cầu xóa khóa an toàn thành công, sau 3 ngày nếu không hủy sẽ tự xóa!");

                            break;
                        }
                    /// Hủy yêu cầu xóa
                    case 4:
                        {
                            /// Toác
                            if (fields.Length != 1)
                            {
                                LogManager.WriteLog(LogTypes.Error, string.Format("Error Socket params count not fit CMD={0}, Client={1}, Recv={2}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), cmdData.Length));
                                return TCPProcessCmdResults.RESULT_FAILED;
                            }

                            /// Nếu không có yêu cầu xóa thì thôi
                            if (client.RequestRemoveSecondPasswordTicks == -1)
                            {
                                /// Thông báo
                                KTPlayerManager.ShowNotification(client, "Không yêu cầu xóa khóa an toàn, không thể hủy!");
                                /// Toác
                                return TCPProcessCmdResults.RESULT_OK;
                            }

                            /// Xóa yêu cầu
                            client.RequestRemoveSecondPasswordTicks = -1;

                            /// Thông báo
                            KTPlayerManager.ShowNotification(client, "Hủy yêu cầu xóa khóa an toàn thành công!");

                            break;
                        }
                    /// Thiết lập
                    case 5:
                        {
                            /// Toác
                            if (fields.Length != 3)
                            {
                                LogManager.WriteLog(LogTypes.Error, string.Format("Error Socket params count not fit CMD={0}, Client={1}, Recv={2}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), cmdData.Length));
                                return TCPProcessCmdResults.RESULT_FAILED;
                            }

                            /// Mật khẩu
                            string password = fields[1];
                            /// Xác nhân mật khẩu
                            string reinputPassword = fields[2];

                            /// Nếu đã có mật khẩu
                            if (!string.IsNullOrEmpty(client.SecondPassword))
                            {
                                /// Thông báo
                                KTPlayerManager.ShowNotification(client, "Đã có mật mã khóa an toàn, không thể thiết lập thêm!");
                                /// Toác
                                return TCPProcessCmdResults.RESULT_OK;
                            }
                            /// Nếu không thỏa mãn
                            else if (string.IsNullOrEmpty(password) || password.Length != 8 || !int.TryParse(password, out _))
                            {
                                /// Thông báo
                                KTPlayerManager.ShowNotification(client, "Mật mã không thỏa mãn!");
                                /// Toác
                                return TCPProcessCmdResults.RESULT_OK;
                            }
                            /// Nếu không đúng
                            else if (password != reinputPassword)
                            {
                                /// Thông báo
                                KTPlayerManager.ShowNotification(client, "Mật mã nhập lại không khớp!");
                                /// Toác
                                return TCPProcessCmdResults.RESULT_OK;
                            }

                            /// Cập nhật
                            client.SecondPassword = password;
                            /// Đánh dấu chưa nhập để khóa an toàn có tác dụng luôn phát đầu tiên ngay sau khi thiết lập
                            client.IsSecondPasswordInput = false;
                            /// Reset số lượt đã nhập sai
                            client.TotalInputIncorrectSecondPasswordTimes = 0;
                            /// Gửi yêu cầu lên GameDB
                            if (!KT_TCPHandler.SendDBUpdateSecondPassword(client))
                            {
                                /// Rollback mật khẩu
                                client.SecondPassword = "";
                                /// Thông báo
                                KTPlayerManager.ShowNotification(client, "Không thể lưu mật mã khóa an toàn. Hãy thử thao tác lại!");
                                /// Toác
                                return TCPProcessCmdResults.RESULT_OK;
                            }

                            /// Thông báo
                            KTPlayerManager.ShowNotification(client, "Thiết lập khóa an toàn thành công!");

                            break;
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
        /// Gửi yêu cầu mở khung thiết lập mật khẩu cấp 2 về Client
        /// </summary>
        /// <param name="player"></param>
        public static void SendOpenSetSecondPassword(KPlayer player)
        {
            try
            {
                player.SendPacket((int)TCPGameServerCmds.CMD_KT_DO_SECONDPASSWORD_CMD, string.Format("0"));
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Exception, ex.ToString());
            }
        }

        /// <summary>
        /// Gửi yêu cầu mở khung quản lý mật khẩu cấp 2 về Client
        /// </summary>
        /// <param name="player"></param>
        public static void SendOpenSecondPasswordManager(KPlayer player)
        {
            try
            {
                /// Số giây còn lại
                int secLeft = player.RequestRemoveSecondPasswordTicks == -1 ? -1 : 259200 - (KTGlobal.GetOffsetSecond(DateTime.Now) - player.RequestRemoveSecondPasswordTicks);
                player.SendPacket((int)TCPGameServerCmds.CMD_KT_DO_SECONDPASSWORD_CMD, string.Format("1:{0}", secLeft));
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Exception, ex.ToString());
            }
        }

        /// <summary>
        /// Gửi yêu cầu mở khung đổi mật khẩu cấp 2 về Client
        /// </summary>
        /// <param name="player"></param>
        public static void SendOpenChangeSecondPassword(KPlayer player)
        {
            try
            {
                player.SendPacket((int)TCPGameServerCmds.CMD_KT_DO_SECONDPASSWORD_CMD, string.Format("2"));
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Exception, ex.ToString());
            }
        }

        /// <summary>
        /// Gửi yêu cầu mở khung nhập mật khẩu cấp 2 về Client
        /// </summary>
        /// <param name="player"></param>
        public static void SendOpenInputSecondPassword(KPlayer player)
        {
            try
            {
                /// Số giây còn lại
                int secLeft = player.RequestRemoveSecondPasswordTicks == -1 ? -1 : 259200 - (KTGlobal.GetOffsetSecond(DateTime.Now) - player.RequestRemoveSecondPasswordTicks);
                player.SendPacket((int)TCPGameServerCmds.CMD_KT_DO_SECONDPASSWORD_CMD, string.Format("3:{0}", secLeft));
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Exception, ex.ToString());
            }
        }

        /// <summary>
        /// Gửi yêu cầu cập nhật mật khẩu cấp 2 lên GameDB
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public static bool SendDBUpdateSecondPassword(KPlayer player)
        {
            string cmdData = string.Format("{0}:{1}", player.RoleID, player.SecondPassword);
            string[] resultData = Global.ExecuteDBCmd((int)TCPGameServerCmds.CMD_KT_DB_UPDATE_SECONDPASSWORD, cmdData, player.ServerId);
            /// Nếu có kết quả
            if (resultData != null)
            {
                /// Mã trả về
                int retCode = int.Parse(resultData[0]);
                /// OK là 1
                return retCode == 1;
            }
            /// Toác
            else
            {
                return false;
            }
        }

        #endregion Second password

        #region Check

        /// <summary>
        /// Kiểm tra Ping các thứ
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
        public static TCPProcessCmdResults ProcessCheck(TCPManager tcpMgr, TMSKSocket socket, TCPClientPool tcpClientPool, TCPRandKey tcpRandKey, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
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
                if (fields.Length != 3)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Error Socket params count not fit CMD={0}, Client={1}, Recv={2}",
                        (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), fields.Length));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                int roleID = Convert.ToInt32(fields[0]);
                int processSubTicks = Convert.ToInt32(fields[1]);
                int dateTimeSubTicks = Convert.ToInt32(fields[2]);

                string strcmd = "";

                KPlayer client = KTPlayerManager.Find(socket);
                if (null == client || client.RoleID != roleID)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Can not find player corresponding ID, CMD={0}, Client={1}, RoleID={2}",
                                                                        (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), roleID));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                client.LastClientHeartTicks = TimeUtil.NOW();

                strcmd = string.Format("{0}", 1);
                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, strcmd, nID);

                return TCPProcessCmdResults.RESULT_DATA;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, Global.GetDebugHelperInfo(socket), false);
            }

            return TCPProcessCmdResults.RESULT_FAILED;
        }

        #endregion Check
    }
}