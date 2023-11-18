using GameServer.KiemThe.Entities;
using GameServer.Logic;
using GameServer.Server;
using Server.Data;
using Server.Protocol;
using Server.TCP;
using Server.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static GameServer.Logic.KTMapManager;

namespace GameServer.KiemThe.Logic
{
    /// <summary>
    /// Quản lý động tác của người chơi
    /// </summary>
    public static partial class KT_TCPHandler
    {
        #region Actions

        /// <summary>
        /// Xử lý yêu cầu từ Client thay đổi động tác của dối tượng
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
        public static TCPProcessCmdResults ResponseSpriteChangeAction(TCPManager tcpMgr, TMSKSocket socket, TCPClientPool tcpClientPool, TCPRandKey tcpRandKey, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;
            C2G_SpriteChangeAction cmdData;

            try
            {
                cmdData = DataHelper.BytesToObject<C2G_SpriteChangeAction>(data, 0, data.Length);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Error while getting DATA, CMD={0}, Client={1}", (TCPGameServerCmds) nID, Global.GetSocketRemoteEndPoint(socket)));
                return TCPProcessCmdResults.RESULT_FAILED;
            }

            try
            {
                KPlayer client = KTPlayerManager.Find(socket);
                if (null == client)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Can not find player, CMD={0}, Client={1}", (TCPGameServerCmds) nID, Global.GetSocketRemoteEndPoint(socket)));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// Nếu đang trong trạng thái chết thì không làm gì cả
                if (client.m_eDoing == KE_NPC_DOING.do_death)
                {
                    return TCPProcessCmdResults.RESULT_OK;
                }
                /// Nếu đang trong trạng thái bị khống chế thì không làm gì cả
                else if (!client.IsCanDoLogic())
                {
                    return TCPProcessCmdResults.RESULT_OK;
                }
                /// Vị trí XY
                int posX = cmdData.PosX;
                int posY = cmdData.PosY;

                /// Vị trí hiện tại
                int currentPosX = client.PosX;
                int currentPosY = client.PosY;

                long startTick = cmdData.StartTick;

                startTick -= 50;

                /// Khoảng Delay trong quá trình gửi Packet từ Client lên
                long delayPacket = KTGlobal.GetCurrentTimeMilis() - startTick;

                //Console.WriteLine("Delay Packet = " + delayPacket);


                /// Nếu thời gian Delay trên ngưỡng cho phép
                if (delayPacket > KTGlobal.MaxClientPacketDelayAllowed)
                {
                    delayPacket = 0;
                }
                else if (delayPacket < 0)
                {
                    delayPacket = 0;
                }

                /// Dừng thực thi StoryBoard
                KTPlayerStoryBoardEx.Instance.Remove(client);

                /// Thông tin bản đồ hiện tại
                GameMap gameMap = KTMapManager.Find(client.CurrentMapCode);
                if (null == gameMap)
                {
                    return TCPProcessCmdResults.RESULT_OK;
                }

                /// Kiểm tra vị trí hiện tại của người chơi và vị trí truyền về từ Client xem có hợp lệ không
                client.SpeedCheatDetector.Validate(posX, posY);

                /// Chuyển trạng thái về trạng thái tương ứng
                client.m_eDoing = (KE_NPC_DOING) cmdData.ActionID;
                /// Thiết lập hướng
                client.CurrentDir = (Entities.Direction) cmdData.Direction;

                /// Thông báo trở lại Client đối tượng thay đổi động tác
                KT_TCPHandler.NotifySpriteChangeAction(client);

                return TCPProcessCmdResults.RESULT_OK;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, Global.GetDebugHelperInfo(socket), false);
            }

            return TCPProcessCmdResults.RESULT_FAILED;
        }

        /// <summary>
        /// Gửi gói tin về Client thông báo đối tượng thay đổi động tác
        /// </summary>
        /// <param name="target"></param>
        public static void NotifySpriteChangeAction(GameObject target)
        {
            try
            {
                G2C_SpriteChangeAction spriteChangeAction = new G2C_SpriteChangeAction()
                {
                    RoleID = target.RoleID,
                    Direction = (int) target.CurrentDir,
                    ActionID = (int) target.m_eDoing,
                    PosX = (int) target.CurrentPos.X,
                    PosY = (int) target.CurrentPos.Y,
                };
                byte[] cmdData = DataHelper.ObjectToBytes<G2C_SpriteChangeAction>(spriteChangeAction);

                /// Tìm tất cả người chơi xung quanh để gửi gói tin
                List<KPlayer> listObjects = KTRadarMapManager.GetPlayersAround(target);
                if (listObjects == null)
                {
                    return;
                }
                /// Gửi gói tin đến tất cả người chơi xung quanh
                KTPlayerManager.SendPacketToPlayers((int) TCPGameServerCmds.CMD_KT_G2C_CHANGEACTION, listObjects, cmdData, target, null);
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Exception, ex.ToString());
            }
        }

        /// <summary>
        /// Gửi gói tin về Client thông báo đối tượng thay đổi trạng thái ngũ hành
        /// </summary>
        /// <param name="target">Đối tượng</param>
        /// <param name="state">Trạng thái</param>
        /// <param name="type">Loại: 0 - Xóa, 1 - Thêm</param>
        /// <param name="timeSecond">Thời gian (nếu là thêm, còn xóa thì để mặc định 0)</param>
        /// <param name="dragPosX">Vị trí giật đến X</param>
        /// <param name="dragPosY">Vị trí giật đến Y</param>
        public static void NotifySpriteSeriesState(GameObject target, KE_STATE state, int type, float timeSecond = 0, int dragPosX = 0, int dragPosY = 0)
        {
            try
            {
                G2C_SpriteSeriesState spriteChangeState = new G2C_SpriteSeriesState()
                {
                    RoleID = target.RoleID,
                    SeriesID = (int) state,
                    Type = type,
                    Time = timeSecond,
                    DragPosX = dragPosX,
                    DragPosY = dragPosY,
                };
                byte[] cmdData = DataHelper.ObjectToBytes<G2C_SpriteSeriesState>(spriteChangeState);

                /// Tìm tất cả người chơi xung quanh để gửi gói tin
                List<KPlayer> listObjects = KTRadarMapManager.GetPlayersAround(target);
                if (listObjects == null)
                {
                    return;
                }
                /// Gửi gói tin đến tất cả người chơi xung quanh
                KTPlayerManager.SendPacketToPlayers((int) TCPGameServerCmds.CMD_KT_G2C_SPRITESERIESSTATE, listObjects, cmdData, target, null);
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Exception, ex.ToString());
            }
        }

        #endregion Actions

        #region Trạng thái cưỡi thay đổi

        /// <summary>
        /// Phản hồi yêu cầu CMDTest từ Client
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
        public static TCPProcessCmdResults ResponseToggleHorseState(TCPManager tcpMgr, TMSKSocket socket, TCPClientPool tcpClientPool, TCPRandKey tcpRandKey, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
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
                LogManager.WriteLog(LogTypes.Error, string.Format("Error while getting DATA, CMD={0}, Client={1}", (TCPGameServerCmds) nID, Global.GetSocketRemoteEndPoint(socket)));
                return TCPProcessCmdResults.RESULT_FAILED;
            }

            try
            {
                if (!string.IsNullOrEmpty(cmdData))
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Error Socket params count not fit CMD={0}, Client={1}, Recv={2}", (TCPGameServerCmds) nID, Global.GetSocketRemoteEndPoint(socket), cmdData.Length));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// Tìm chủ nhân của gói tin tương ứng
                KPlayer client = KTPlayerManager.Find(socket);

                if (null == client)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Không tìm thấy thông tin người chơi, CMD={0}, Client={1}", (TCPGameServerCmds) nID, Global.GetSocketRemoteEndPoint(socket)));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// Nếu đang trong trạng thái bị khống chế
                if (!client.IsCanDoLogic())
                {
                    KTPlayerManager.ShowNotification(client, "Đang trong trạng thái bị khống chế, không thể thay đổi trạng thái cưỡi ngựa!");
                    return TCPProcessCmdResults.RESULT_OK;
                }

                /// Lấy thông tin ngựa tương ứng ở trang bị
                GoodsData horseGD = client.GoodsData.Find(x => x.Using == (int) KE_EQUIP_POSITION.emEQUIPPOS_HORSE);
                /// Nếu không có ngựa
                if (horseGD == null)
                {
                    KTPlayerManager.ShowNotification(client, "Không có ngựa cưỡi!");
                    return TCPProcessCmdResults.RESULT_OK;
                }

                /// Nếu chưa hết thời gian giãn cách thay đổi trạng thái ngựa
                if (KTGlobal.GetCurrentTimeMilis() - client.LastTickToggleHorseState < KTGlobal.TickHorseStateChange)
                {
                    long tickLeft = KTGlobal.TickHorseStateChange - KTGlobal.GetCurrentTimeMilis() + client.LastTickToggleHorseState;
                    int sec = Math.Max(1, (int) tickLeft / 1000);

                    KTPlayerManager.ShowNotification(client, string.Format("Không thể thay đổi trạng thái cưỡi ngựa lúc này, hãy chờ {0} giây rồi thử lại!", sec));
                    return TCPProcessCmdResults.RESULT_OK;
                }

                client.GetPlayEquipBody().ClearAllEffecEquipBody();
                /// Chuyển trạng thái sang cưỡi ngựa
                client.IsRiding = !client.IsRiding;

                client.GetPlayEquipBody().AttackAllEquipBody();

                /// Cập nhật thời gian thay trạng thái cưỡi
                client.LastTickToggleHorseState = KTGlobal.GetCurrentTimeMilis();

                /// Lưu vào DB
                Global.SaveRoleParamsInt32ValueToDB(client, RoleParamName.HorseToggleOn, client.IsRiding ? 1 : 0, true);

                /// Cập nhật chỉ số ngựa từ trang bị, từ đó chuyển gói tin về Client thông báo tốc độ di chuyển thay đổi như nào
              //  client.GetPlayEquipBody().RefreshEquip();

                /// Gửi thông báo tới tất cả người chơi xung quanh
                KT_TCPHandler.SendHorseStateChanged(client);

                return TCPProcessCmdResults.RESULT_OK;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, Global.GetDebugHelperInfo(socket), false);
            }

            return TCPProcessCmdResults.RESULT_FAILED;
        }

        /// <summary>
        /// Gửi gói tin từ Server về client thông báo trạng thái cưỡi ngựa thay đổi tới tất cả người chơi xung quanh
        /// </summary>
        /// <param name="client"></param>
        public static void SendHorseStateChanged(KPlayer client)
        {
            try
            {
                byte[] cmdData = new ASCIIEncoding().GetBytes(string.Format("{0}:{1}", client.RoleID, client.IsRiding ? 1 : 0));

                /// Tìm tất cả người chơi xung quanh để gửi gói tin
                List<KPlayer> listObjects = KTRadarMapManager.GetPlayersAround(client);
                if (listObjects == null)
                {
                    return;
                }
                /// Gửi gói tin đến tất cả người chơi xung quanh
                KTPlayerManager.SendPacketToPlayers((int) TCPGameServerCmds.CMD_KT_TOGGLE_HORSE_STATE, listObjects, cmdData, client, null);
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Exception, ex.ToString());
            }
        }

        #endregion Trạng thái cưỡi thay đổi
    }
}
