using GameServer.KiemThe.Entities;
using GameServer.Logic;
using GameServer.Server;
using GameServer.VLTK.Core.StallManager;
using Server.Data;
using Server.Protocol;
using Server.TCP;
using Server.Tools;
using System;
using System.Collections.Generic;
using System.Text;
using static GameServer.Logic.KTMapManager;

namespace GameServer.KiemThe.Logic
{
    /// <summary>
    /// Quản lý gói tin
    /// </summary>
    public static partial class KT_TCPHandler
    {
        #region Di chuyển
        /// <summary>
        /// Client bắt đầu di chuyển
        /// </summary>
        /// <param name="pool"></param>
        /// <param name="nID"></param>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static TCPProcessCmdResults ProcessSpriteMoveCmd(TCPManager tcpMgr, TMSKSocket socket, TCPClientPool tcpClientPool, TCPRandKey tcpRandKey, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;

            SpriteMoveData cmdData = null;

            try
            {
                cmdData = DataHelper.BytesToObject<SpriteMoveData>(data, 0, count);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Decode data faild, CMD={0}, Client={1}", (TCPGameServerCmds) nID, Global.GetSocketRemoteEndPoint(socket)));
                return TCPProcessCmdResults.RESULT_FAILED;
            }

            try
            {
                if (null == cmdData)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Decode data faild, CMD={0}, Client={1}", (TCPGameServerCmds) nID, Global.GetSocketRemoteEndPoint(socket)));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// ID đối tượng
                int roleID = cmdData.RoleID;
                /// Vị trí đích X
                int toX = cmdData.ToX;
                /// Vị trí đích Y
                int toY = cmdData.ToY;
                /// Vị trí bắt đầu X
                int fromX = cmdData.FromX;
                /// Vị trí bắt đầu Y
                int fromY = cmdData.FromY;

                /// Người chơi tương ứng
                KPlayer client = KTPlayerManager.Find(socket);
                if (null == client || client.RoleID != roleID)
                {
                    //LogManager.WriteLog(LogTypes.Error, string.Format("Can not find player corresponding ID, CMD={0}, Client={1}", (TCPGameServerCmds) nID, Global.GetSocketRemoteEndPoint(socket)));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// Nếu đã chết thì thôi
                if (client.IsDead() || KTGlobal.GetCurrentTimeMilis() - client.LastDeadTicks < 500)
				{
                    /// Thông báo
                    KTPlayerManager.ShowNotification(client, "Đang trong trạng thái bị khống chế, tạm thời không thể di chuyển!");
                    return TCPProcessCmdResults.RESULT_OK;
				}

                /// Dừng StoryBoard hiện tại
                KTPlayerStoryBoardEx.Instance.Remove(client);

                /// Vị trí hiện tại
                int currentPosX = client.PosX;
                int currentPosY = client.PosY;

                /// Nếu đang bày bán
                if (StallManager.IsDirectStall(client))
                {
                    /// Thông báo
                    KTPlayerManager.ShowNotification(client, "Đang trong trạng thái bày bán, không thể di chuyển!");
                    return TCPProcessCmdResults.RESULT_OK;
                }

                ///// Nếu đang đợi chuyển cảnh - BỎ VÌ ĐÉO CÓ TÁC DỤNG
                //if (client.WaitingForChangeMap)
                //{
                //    return TCPProcessCmdResults.RESULT_OK;
                //}

                /// ID bản đồ hiện tại
                int mapCode = client.CurrentMapCode;

                /// Thông tin bản đồ hiện tại
                GameMap gameMap = KTMapManager.Find(mapCode);
                if (null == gameMap)
                {
                    return TCPProcessCmdResults.RESULT_OK;
                }

                /// Nếu đang Blink thì thôi
                if (client.IsBlinking())
                {
                    /// Thông báo
                    KTPlayerManager.ShowNotification(client, "Đang khinh công, tạm không thể di chuyển!");
                    return TCPProcessCmdResults.RESULT_OK;
                }

                /// Đoạn đường di chuyển
                string pathString = cmdData.PathString;

                /// Tốc độ di chuyển hiện tại
                int moveSpeed = client.GetCurrentRunSpeed();

                /// Kiểm tra vị trí hiện tại của người chơi và vị trí truyền về từ Client xem có hợp lệ không
                if (!client.SpeedCheatDetector.Validate(fromX, fromY))
                {
                    /// Thông báo
                    KTPlayerManager.ShowNotification(client, "Vị trí không hợp lệ, tự động quay về vị trí ban đầu!");
                    /// Thay đổi vị trí hiện tại của Client
                    KTPlayerManager.ChangePos(client, fromX, fromY, true);
                    return TCPProcessCmdResults.RESULT_OK;
                }

                /// Nếu không thể chủ động di chuyển
                if (!client.IsCanPositiveMove())
                {
                    //Console.WriteLine("CMD_MOVE => can not positive move");
                    //LogManager.WriteLog(LogTypes.RolePosition, string.Format("SPR_MOVE => can not positive move. RoleID = {0}, ClientPos = ({1},{2})", roleID, fromX, fromY, client.PosX, client.PosY));

                    /// Thông báo
                    KTPlayerManager.ShowNotification(client, "Đang trong trạng thái bị khống chế, không thể di chuyển!");
                    /// Thay đổi vị trí hiện tại của Client
                    KTPlayerManager.ChangePos(client, fromX, fromY, false);
                    return TCPProcessCmdResults.RESULT_OK;
                }

                /// Gói tin gửi cho người chơi khác thông báo đối tượng di chuyển
                SpriteNotifyOtherMoveData moveData = new SpriteNotifyOtherMoveData()
                {
                    RoleID = client.RoleID,
                    FromX = currentPosX,
                    FromY = currentPosY,
                    ToX = toX,
                    ToY = toY,
                    PathString = cmdData.PathString,
                    Action = (int) KE_NPC_DOING.do_run,
                };
                /// Thông báo cho người chơi khác
                KT_TCPHandler.NotifyObjectMove(client, moveData, false);

                pathString += string.Format("|{0}_{1}", toX, toY);
                /// Thực hiện StoryBoard
                KTPlayerStoryBoardEx.Instance.Add(client, pathString);

                return TCPProcessCmdResults.RESULT_OK;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, Global.GetDebugHelperInfo(socket), false);
            }

            return TCPProcessCmdResults.RESULT_FAILED;
        }

        /// <summary>
        /// Gói tin từ Client gửi lên Server thông báo đối tượng ngừng di chuyển
        /// </summary>
        /// <param name="pool"></param>
        /// <param name="nID"></param>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static TCPProcessCmdResults ProcessSpriteStopMoveCmd(TCPManager tcpMgr, TMSKSocket socket, TCPClientPool tcpClientPool, TCPRandKey tcpRandKey, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;
            string cmdData = "";
            try
            {
                /// Giải mã gói tin đẩy về dạng string
                cmdData = new UTF8Encoding().GetString(data);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Error while getting DATA, CMD={0}, Client={1}", (TCPGameServerCmds) nID, Global.GetSocketRemoteEndPoint(socket)));
                return TCPProcessCmdResults.RESULT_FAILED;
            }

            try
            {
                /// Chia thành các trường
                string[] fields = cmdData.Split(':');
                if (fields.Length != 3)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Wrong parameters, CMD={0}, Client={1}, Recv={2}", (TCPGameServerCmds) nID, Global.GetSocketRemoteEndPoint(socket), fields.Length));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                int posX = int.Parse(fields[0]);
                int posY = int.Parse(fields[1]);
                int direction = int.Parse(fields[1]);

                KPlayer client = KTPlayerManager.Find(socket);
                if (null == client)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Can not find player corresponding ID, CMD={0}, Client={1}", (TCPGameServerCmds) nID, Global.GetSocketRemoteEndPoint(socket)));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// Vị trí hiện tại
                int currentPosX = client.PosX;
                int currentPosY = client.PosY;

                /// Dừng thực thi StoryBoard
                KTPlayerStoryBoardEx.Instance.Remove(client, false);

                /// Nếu đã chết thì thôi
                if (client.IsDead() || KTGlobal.GetCurrentTimeMilis() - client.LastDeadTicks < 500)
                {
                    return TCPProcessCmdResults.RESULT_OK;
                }

                /// Kiểm tra vị trí hiện tại của người chơi và vị trí truyền về từ Client xem có hợp lệ không
                if (client.SpeedCheatDetector.Validate(currentPosX, currentPosY))
                {
                    /// Đồng bộ vị trí
                    client.PosX = currentPosX;
                    client.PosY = currentPosY;

                    /// Bản đồ
                    GameMap map = KTMapManager.Find(client.MapCode);
                    /// Cập nhật vào MapGrid
                    map.Grid.MoveObject(currentPosX, currentPosY, client);
                }
                /// Toác thì dịch nó về vị trí của nó thỏa mãn
                else
                {
                    KTPlayerManager.ChangePos(client, client.PosX, client.PosY, true);
                }

                /// Gửi vị trí hiện tại cho các Client khác
                KT_TCPHandler.NotifyObjectStopMove(client, false);

                return TCPProcessCmdResults.RESULT_OK;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, Global.GetDebugHelperInfo(socket), false);
            }

            return TCPProcessCmdResults.RESULT_FAILED;
        }

        /// <summary>
        /// Thông báo tới người chơi xung quanh đối tượng di chuyển
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="moveData"></param>
        /// <param name="notifySelf"></param>
        public static void NotifyObjectMove(GameObject obj, SpriteNotifyOtherMoveData moveData, bool notifySelf = true)
        {
            /// Tìm người chơi xung quanhg
            List<KPlayer> objsList = KTRadarMapManager.GetPlayersAround(obj);
            /// Toác
            if (null == objsList)
            {
                /// Bỏ qua
                return;
            }

            /// Gửi gói tin tới toàn bộ người chơi khác
            KTPlayerManager.SendPacketToPlayers<SpriteNotifyOtherMoveData>((int) TCPGameServerCmds.CMD_SPR_MOVE, objsList, moveData, obj, notifySelf ? null : obj);
        }

        /// <summary>
        /// Thông báo tới người chơi xung quanh đối tượng ngừng di chuyển
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="notifySelf"></param>
        public static void NotifyObjectStopMove(GameObject obj, bool notifySelf = true)
        {
            /// Tìm người chơi xung quanhg
            List<KPlayer> objsList = KTRadarMapManager.GetPlayersAround(obj);
            /// Toác
            if (null == objsList)
            {
                /// Bỏ qua
                return;
            }

            /// Tạo mới gói tin
            SpriteStopMove data = new SpriteStopMove()
            {
                RoleID = obj.RoleID,
                PosX = (int) obj.CurrentPos.X,
                PosY = (int) obj.CurrentPos.Y,
                MoveSpeed = obj.GetCurrentRunSpeed(),
                Direction = (int) obj.CurrentDir,
            };

            KTPlayerManager.SendPacketToPlayers<SpriteStopMove>((int) TCPGameServerCmds.CMD_SPR_STOPMOVE, objsList, data, obj, notifySelf ? null : obj);
        }
        #endregion
    }
}
