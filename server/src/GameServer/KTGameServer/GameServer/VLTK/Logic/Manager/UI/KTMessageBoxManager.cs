using GameServer.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.KiemThe.Logic
{
    /// <summary>
    /// Quản lý MessageBox
    /// </summary>
    public static class KTMessageBoxManager
    {
        /// <summary>
        /// Danh sách MessageBox trong hệ thống
        /// </summary>
        private static Dictionary<int, KMessageBox> messageBoxes = new Dictionary<int, KMessageBox>();

        /// <summary>
        /// Thêm MessageBox vào hệ thống
        /// </summary>
        /// <param name="messageBox"></param>
        public static void AddMessageBox(KMessageBox messageBox)
        {
            if (messageBox == null)
            {
                return;
            }

            lock (KTMessageBoxManager.messageBoxes)
            {
                KTMessageBoxManager.messageBoxes[messageBox.ID] = messageBox;
            }
        }

        /// <summary>
        /// Tìm MessageBox theo ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static KMessageBox FindMessageBox(int id)
        {
            lock (KTMessageBoxManager.messageBoxes)
            {
                if (KTMessageBoxManager.messageBoxes.TryGetValue(id, out KMessageBox messageBox))
                {
                    return messageBox;
                }
                else
                {
                    return null;
                }
            }
        }
        
        /*
        /// <summary>
        /// Xóa MessageBox có ID tương ứng
        /// </summary>
        /// <param name="id"></param>
        public static void RemoveMessageBox(int id)
        {
            lock (KTMessageBoxManager.messageBoxes)
            {
                if (KTMessageBoxManager.messageBoxes.TryGetValue(id, out KMessageBox messageBox))
                {
                    KTMessageBoxManager.messageBoxes.Remove(id);
                }
            }
        }

        /// <summary>
        /// Xóa MessageBox tương ứng
        /// </summary>
        /// <param name="messageBox"></param>
        public static void RemoveMessageBox(KMessageBox messageBox)
        {
            if (messageBox == null)
            {
                return;
            }

            KTMessageBoxManager.RemoveMessageBox(messageBox.ID);
        }

        /// <summary>
        /// Xóa tất cả MessageBox sinh ra bởi người chơi tương ứng
        /// </summary>
        /// <param name="player"></param>
        public static void RemoveAllMessageBoxesOf(KPlayer player)
        {
            lock (KTMessageBoxManager.messageBoxes)
            {
                /// Danh sách cần xóa
                List<int> toRemoveKeys = KTMessageBoxManager.messageBoxes.Where(x => x.Value.Owner == player).Select(x => x.Key).ToList();
                /// Duyệt danh sách và xóa
                foreach (int toRemoveKey in toRemoveKeys)
                {
                    /// Xóa đối tượng tương ứng
                    KTMessageBoxManager.messageBoxes.Remove(toRemoveKey);
                }
            }
        }
        */
    }
}
