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
	public partial class KTMonsterStoryBoardEx
	{
        /// <summary>
        /// Gửi gói tin quái vật di chuyển tới người chơi xung quanh
        /// </summary>
        /// <param name="monster"></param>
        /// <param name="pathString"></param>
        /// <param name="fromPos"></param>
        /// <param name="toPos"></param>
        /// <param name="direction"></param>
        /// <param name="action"></param>
        private void SendMonsterMoveToClient(Monster monster, string pathString, Point fromPos, Point toPos, KE_NPC_DOING action)
        {
            try
            {
                SpriteNotifyOtherMoveData moveData = new SpriteNotifyOtherMoveData()
                {
                    RoleID = monster.RoleID,
                    FromX = (int) fromPos.X,
                    FromY = (int) fromPos.Y,
                    ToX = (int) toPos.X,
                    ToY = (int) toPos.Y,
                    PathString = pathString,
                    Action = (int) action,
                };
                KT_TCPHandler.NotifyObjectMove(monster, moveData);
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Exception, ex.ToString());
            }
        }

        /// <summary>
        /// Thông báo cho người chơi khác đối tượng ngừng di chuyển
        /// </summary>
        /// <param name="obj"></param>
        private void SendMonsterStopMoveToClient(Monster monster)
        {
            try
            {
                KT_TCPHandler.NotifyObjectStopMove(monster, false);
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Exception, ex.ToString());
            }
        }
    }
}
