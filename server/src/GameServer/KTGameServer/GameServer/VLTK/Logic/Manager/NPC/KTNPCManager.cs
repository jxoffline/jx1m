using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace GameServer.Logic
{
    /// <summary>
    /// Quản lý NPC
    /// </summary>
    public static partial class KTNPCManager
    {
        #region Define
        /// <summary>
        /// Danh sách NPC hiện có
        /// </summary>
        private static readonly ConcurrentDictionary<int, NPC> npcs = new ConcurrentDictionary<int, NPC>();

        /// <summary>
        /// Danh sách Template NPC trong hệ thống
        /// </summary>
        private static readonly Dictionary<int, NPCData> npcTemplates = new Dictionary<int, NPCData>();
        #endregion
    }
}
