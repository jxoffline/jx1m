using Server.Tools;
using System;
using System.Collections.Generic;
using System.Windows;

namespace GameServer.Logic
{
    /// <summary>
    /// Quản lý NPC
    /// </summary>
    public static partial class KTNPCManager
    {
        #region Thêm
        /// <summary>
        /// Tạo NPC tương ứng
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static NPC Create(NPCBuilder builder)
        {
            /// Thông tin Res
            if (!KTNPCManager.npcTemplates.TryGetValue(builder.ResID, out NPCData templateData))
            {
                /// Thông báo
                LogManager.WriteLog(LogTypes.Error, string.Format("Create NPC failed. NPC ResID = {0} not exist.", builder.ResID));
                /// Bỏ qua
                return null;
            }

            /// Tạo NPC
            NPC npc = new NPC()
            {
                MapCode = builder.MapCode,
                CopyMapID = builder.CopySceneID,
                ResID = builder.ResID,
                CurrentPos = new Point(builder.PosX, builder.PosY),
                Name = builder.Name,
                Title = builder.Title,
                CurrentDir = builder.Direction,
                ScriptID = builder.ScriptID,
                Tag = builder.Tag,
                MinimapName = builder.MinimapName,
                VisibleOnMinimap = builder.VisibleOnMinimap,
            };

            /// Nếu không có tên
            if (string.IsNullOrEmpty(npc.Name))
            {
                /// Lấy tên ở Template
                npc.Name = templateData.Name;
            }
            /// Nếu không có danh hiệu
            if (string.IsNullOrEmpty(npc.Title))
            {
                /// Lấy tên ở Template
                npc.Title = templateData.Title;
            }
            /// Nếu không có tên ở Minimap
            if (string.IsNullOrEmpty(npc.MinimapName))
            {
                /// Lấy tên ở Template
                npc.MinimapName = templateData.Name;
            }

            /// Thêm NPC
            KTNPCManager.Add(npc);

            /// Trả về kết quả
            return npc;
        }
        #endregion

        #region Tìm
        /// <summary>
        /// Trả về toàn bộ NPC trong bản đồ hoặc phụ bản tương ứng
        /// </summary>
        /// <param name="mapCode"></param>
        /// <param name="copyMapID"></param>
        public static List<NPC> GetMapNPCs(int mapCode, int copyMapID = -1)
        {
            /// Trả về kết quả
            return KTNPCManager.FindAll(x => x.MapCode == mapCode && x.CopyMapID == copyMapID);
        }
        #endregion

        #region Xóa
        /// <summary>
        /// Xóa toàn bộ NPC trong bản đồ hoặc phụ bản tương ứng
        /// </summary>
        /// <param name="mapCode"></param>
        /// <param name="copyMapID"></param>
        public static void RemoveMapNpcs(int mapCode, int copyMapID = -1)
        {
            KTNPCManager.RemoveAll(x => x.MapCode == mapCode && x.CopyMapID == copyMapID);
        }
        #endregion
    }
}
