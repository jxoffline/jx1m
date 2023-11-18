using GameServer.KiemThe.Core.Task;
using GameServer.Logic;
using GameServer.Server;
using Server.Data;
using Server.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.KiemThe.Logic
{
    /// <summary>
    /// Quản lý nhiệm vụ
    /// </summary>
    public static partial class KT_TCPHandler
    {
        /// <summary>
        /// Update tình trạng nhiệm vụ
        /// </summary>
        public static void NotifyUpdateTask(KPlayer client, int dbID, int taskID, int taskVal1, int taskVal2, int taskFocus)
        {
            /// Chuỗi thông tin
            string strcmd = string.Format("{0}:{1}:{2}:{3}:{4}", dbID, taskID, taskVal1, taskVal2, taskFocus);
            client.SendPacket((int) TCPGameServerCmds.CMD_SPR_MODTASK, strcmd);
        }

        /// <summary>
        /// Notify về trạng thái quest của NPC
        /// </summary>
        /// <param name="sl"></param>
        /// <param name="pool"></param>
        /// <param name="client"></param>
        /// <param name="npcID"></param>
        /// <param name="state"></param>
        public static void NotifyUpdateNPCTaskSate(KPlayer client, int npcID, int state)
        {
            /// Chuỗi thông tin
            string strcmd = string.Format("{0}:{1}", npcID, state);
            client.SendPacket((int) TCPGameServerCmds.CMD_SPR_UPDATENPCSTATE, strcmd);
        }

        /// <summary>
        /// Gửi trạng thái về cho NPC có task hay không có task
        /// </summary>
        /// <param name="sl"></param>
        /// <param name="pool"></param>
        /// <param name="client"></param>
        /// <param name="npcTaskStatList"></param>
        public static void NotifyNPCTaskStateList(KPlayer client, List<NPCTaskState> npcTaskStatList)
        {
            client.SendPacket<List<NPCTaskState>>((int) TCPGameServerCmds.CMD_SPR_NPCSTATELIST, npcTaskStatList);
        }
    }
}
