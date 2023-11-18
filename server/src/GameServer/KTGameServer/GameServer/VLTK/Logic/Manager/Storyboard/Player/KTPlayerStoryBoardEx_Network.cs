using GameServer.KiemThe.Entities;
using GameServer.Logic;
using GameServer.Server;
using Server.Data;
using Server.Tools;
using System;
using System.Windows;

namespace GameServer.KiemThe.Logic
{
	/// <summary>
	/// Quản lý tầng Network của StoryBoard
	/// </summary>
	public partial class KTPlayerStoryBoardEx
	{
        #region Network
        /// <summary>
        /// Gửi gói tin người chơi di chuyển tới người chơi xung quanh
        /// </summary>
        /// <param name="monster"></param>
        /// <param name="pathString"></param>
        /// <param name="fromPos"></param>
        /// <param name="toPos"></param>
        /// <param name="direction"></param>
        /// <param name="action"></param>
        private void SendPlayerMoveToClient(KPlayer player, string pathString, Point fromPos, Point toPos, KE_NPC_DOING action)
        {
            try
            {
                SpriteNotifyOtherMoveData moveData = new SpriteNotifyOtherMoveData()
                {
                    RoleID = player.RoleID,
                    FromX = (int) fromPos.X,
                    FromY = (int) fromPos.Y,
                    ToX = (int) toPos.X,
                    ToY = (int) toPos.Y,
                    PathString = pathString,
                    Action = (int) action,
                };
                KT_TCPHandler.NotifyObjectMove(player, moveData);
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Exception, ex.ToString());
            }
        }

        /// <summary>
        /// Gửi gói tin thông báo người chơi ngừng di chuyển tới người chơi xung quanh
        /// </summary>
        /// <param name="player"></param>
        private void SendPlayerStopMove(KPlayer player)
        {
            try
            {
                KT_TCPHandler.NotifyObjectStopMove(player, false);
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Exception, ex.ToString());
            }
        }
        #endregion
    }
}
