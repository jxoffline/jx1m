using GameServer.KiemThe.Core.Task;
using GameServer.Logic;
using Server.Tools;
using System;

namespace GameServer.KiemThe.Logic
{
    /// <summary>
    /// Quản lý người chơi
    /// </summary>
    public static partial class KTPlayerManager
    {
        /// <summary>
        /// Giao nhiệm vụ đầu tiên cho người chơi
        /// </summary>
        /// <param name="tcpMgr"></param>
        /// <param name="tcpClientPool"></param>
        /// <param name="pool"></param>
        /// <param name="tcpRandKey"></param>
        /// <param name="client"></param>
        public static bool GiveFirstTask(KPlayer client)
        {
            if (null == client)
            {
                return false;
            }

            try
            {
                if (MainTaskManager.getInstance().CanTakeNewTask(client, 100))
                {
                    MainTaskManager.getInstance().AppcepTask(null, client, 100);
                }

                return true;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, Global.GetDebugHelperInfo(client.ClientSocket), false);
            }
            return false;
        }
    }
}
