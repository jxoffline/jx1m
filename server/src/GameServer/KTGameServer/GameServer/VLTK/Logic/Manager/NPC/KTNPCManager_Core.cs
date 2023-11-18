using System;
using System.Collections.Generic;
using System.Linq;
using static GameServer.Logic.KTMapManager;

namespace GameServer.Logic
{
    /// <summary>
    /// Quản lý NPC
    /// </summary>
    public static partial class KTNPCManager
    {
        #region Thêm
        /// <summary>
        /// Thêm NPC tương ứng vào bản đồ
        /// </summary>
        /// <param name="npc"></param>
        public static void Add(NPC npc)
        {
            /// Thông tin bản đồ đang đứng
            GameMap gameMap = KTMapManager.Find(npc.MapCode);
            /// Toác
            if (gameMap == null)
            {
                /// Bỏ qua
                return;
            }

            /// Thêm vào danh sách
            KTNPCManager.npcs[npc.NPCID] = npc;

            /// Thêm vào bản đồ
            gameMap.Grid.MoveObject((int) npc.CurrentPos.X, (int) npc.CurrentPos.Y, npc);
        }
        #endregion

        #region Xóa
        /// <summary>
        /// Xóa NPC tương ứng khỏi bản đồ
        /// </summary>
        /// <param name="npc"></param>
        public static void Remove(NPC npc)
        {
            /// Thông tin bản đồ đang đứng
            GameMap gameMap = KTMapManager.Find(npc.MapCode);
            /// Toác
            if (gameMap == null)
            {
                /// Bỏ qua
                return;
            }

            /// Xóa khỏi danh sách
            KTNPCManager.npcs.TryRemove(npc.NPCID, out _);

            /// Xóa khỏi bản đồ
            gameMap.Grid.RemoveObject(npc);
        }

        /// <summary>
        /// Xóa NPC tương ứng khỏi bản đồ
        /// </summary>
        /// <param name="npcID"></param>
        public static void Remove(int npcID)
        {
            /// NPC tương ứng
            NPC npc = KTNPCManager.Find(npcID);
            /// Nếu không tìm thấy
            if (npc == null)
            {
                /// Bỏ qua
                return;
            }
            /// Xóa NPC tương ứng
            KTNPCManager.Remove(npc);
        }

        /// <summary>
        /// Xóa NPC đầu tiên thỏa mãn điều kiện tương ứng
        /// </summary>
        /// <param name="predicate"></param>
        public static void Remove(Predicate<NPC> predicate)
        {
            /// NPC tương ứng
            NPC npc = KTNPCManager.Find(predicate);
            /// Nếu không tìm thấy
            if (npc == null)
            {
                /// Bỏ qua
                return;
            }
            /// Xóa NPC tương ứng
            KTNPCManager.Remove(npc);
        }

        /// <summary>
        /// Xóa toàn bộ NPC thỏa mãn điều kiện tương ứng
        /// </summary>
        /// <param name="predicate"></param>
        public static void RemoveAll(Predicate<NPC> predicate)
        {
            /// Danh sách NPC thỏa mãn điều kiện
            List<NPC> npcs = KTNPCManager.FindAll(predicate);
            /// Duyệt danh sách
            foreach (NPC npc in npcs)
            {
                /// Xóa NPC tương ứng
                KTNPCManager.Remove(npc);
            }
        }
        #endregion

        #region Tìm kiếm
        /// <summary>
        /// Tìm NPC có ID tương ứng
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static NPC Find(int id)
        {
            /// Thông tin
            if (KTNPCManager.npcs.TryGetValue(id, out NPC npc))
            {
                /// Trả về kết quả
                return npc;
            }
            /// Không tìm thấy
            return null;
        }

        /// <summary>
        /// Tìm NPC thỏa mãn điều kiện tương ứng
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static NPC Find(Predicate<NPC> predicate)
        {
            /// Danh sách khóa
            List<int> keys = KTNPCManager.npcs.Keys.ToList();
            /// Duyệt danh sách
            foreach (int key in keys)
            {
                /// Nếu không tồn tại
                if (!KTNPCManager.npcs.TryGetValue(key, out NPC npc))
                {
                    /// Bỏ qua
                    continue;
                }
                /// Nếu thỏa mãn điều kiện
                if (predicate(npc))
                {
                    /// Trả về kết quả
                    return npc;
                }
            }
            /// Không tìm thấy
            return null;
        }

        /// <summary>
        /// Tìm toàn bộ NPC thỏa mãn điều kiện tương ứng
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static List<NPC> FindAll(Predicate<NPC> predicate)
        {
            /// Kết quả
            List<NPC> npcs = new List<NPC>();

            /// Danh sách khóa
            List<int> keys = KTNPCManager.npcs.Keys.ToList();
            /// Duyệt danh sách
            foreach (int key in keys)
            {
                /// Nếu không tồn tại
                if (!KTNPCManager.npcs.TryGetValue(key, out NPC npc))
                {
                    /// Bỏ qua
                    continue;
                }
                /// Nếu thỏa mãn điều kiện
                if (predicate(npc))
                {
                    /// Thêm vào danh sách
                    npcs.Add(npc);
                }
            }

            /// Trả về kết quả
            return npcs;
        }
        #endregion
    }
}
