using System.Collections.Concurrent;

namespace GameServer.KiemThe.Logic
{
    /// <summary>
    /// Quản lý ItemDialog
    /// </summary>
    public static class KTItemDialogManager
    {
        /// <summary>
        /// Danh sách ItemDialog trong hệ thống
        /// </summary>
        private static ConcurrentDictionary<int, KItemDialog> ItemDialoges = new ConcurrentDictionary<int, KItemDialog>();

        /// <summary>
        /// Thêm ItemDialog vào hệ thống
        /// </summary>
        /// <param name="ItemDialog"></param>
        public static void AddItemDialog(KItemDialog ItemDialog)
        {
            if (ItemDialog == null)
            {
                return;
            }

            KTItemDialogManager.ItemDialoges[ItemDialog.ID] = ItemDialog;
        }

        /// <summary>
        /// Xóa NPCDialog khỏi hệ thống
        /// </summary>
        /// <param name="NPCDialog"></param>
        public static void RemoveItemDialog(KItemDialog ItemDialog)
        {
            KTItemDialogManager.ItemDialoges.TryRemove(ItemDialog.ID, out _);
        }

        /// <summary>
        /// Tìm ItemDialog theo ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static KItemDialog FindItemDialog(int id)
        {
            if (KTItemDialogManager.ItemDialoges.TryGetValue(id, out KItemDialog ItemDialog))
            {
                return ItemDialog;
            }
            else
            {
                return null;
            }
        }
    }
}
