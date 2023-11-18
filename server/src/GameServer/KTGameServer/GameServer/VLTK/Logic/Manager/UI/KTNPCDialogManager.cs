using System.Collections.Concurrent;

namespace GameServer.KiemThe.Logic
{
    /// <summary>
    /// Quản lý NPCDialog
    /// </summary>
    public static class KTNPCDialogManager
    {
        /// <summary>
        /// Danh sách NPCDialog trong hệ thống
        /// </summary>
        private static ConcurrentDictionary<int, KNPCDialog> NPCDialoges = new ConcurrentDictionary<int, KNPCDialog>();

        /// <summary>
        /// Thêm NPCDialog vào hệ thống
        /// </summary>
        /// <param name="NPCDialog"></param>
        public static void AddNPCDialog(KNPCDialog NPCDialog)
        {
            if (NPCDialog == null)
            {
                return;
            }

            KTNPCDialogManager.NPCDialoges[NPCDialog.ID] = NPCDialog;
        }

        /// <summary>
        /// Xóa NPCDialog khỏi hệ thống
        /// </summary>
        /// <param name="NPCDialog"></param>
        public static void RemoveNPCDialog(KNPCDialog NPCDialog)
        {
            KTNPCDialogManager.NPCDialoges.TryRemove(NPCDialog.ID, out _);
        }

        /// <summary>
        /// Tìm NPCDialog theo ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static KNPCDialog FindNPCDialog(int id)
        {
            if (KTNPCDialogManager.NPCDialoges.TryGetValue(id, out KNPCDialog NPCDialog))
            {
                return NPCDialog;
            }
            else
            {
                return null;
            }
        }
    }
}
