using GameServer.KiemThe.Entities;
using GameServer.Logic;
using GameServer.Server;
using Server.Data;
using Server.Tools;
using System;
using System.Collections.Generic;
using System.Windows;

namespace GameServer.KiemThe.Logic
{
    /// <summary>
    /// Quản lý Network StoryBoard
    /// </summary>
	public partial class KTTraderCarriageStoryBoardEx
    {
        /// <summary>
        /// Gửi gói tin đối tượng di chuyển tới người chơi xung quanh
        /// </summary>
        /// <param name="carriage"></param>
        /// <param name="pathString"></param>
        /// <param name="fromPos"></param>
        /// <param name="toPos"></param>
        /// <param name="direction"></param>
        /// <param name="action"></param>
        private void SendTraderCarriageMoveToClient(TraderCarriage carriage, string pathString, Point fromPos, Point toPos, KE_NPC_DOING action)
        {
            try
            {
                SpriteNotifyOtherMoveData moveData = new SpriteNotifyOtherMoveData()
                {
                    RoleID = carriage.RoleID,
                    FromX = (int) fromPos.X,
                    FromY = (int) fromPos.Y,
                    ToX = (int) toPos.X,
                    ToY = (int) toPos.Y,
                    PathString = pathString,
                    Action = (int) action,
                };
                KT_TCPHandler.NotifyObjectMove(carriage, moveData);
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Exception, ex.ToString());
            }
        }

        /// <summary>
        /// Thông báo cho người chơi khác đối tượng ngừng di chuyển
        /// </summary>
        /// <param name="carriage"></param>
        private void SendTraderCarriageStopMoveToClient(TraderCarriage carriage)
        {
            try
            {
                KT_TCPHandler.NotifyObjectStopMove(carriage, false);
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Exception, ex.ToString());
            }
        }
    }
}
