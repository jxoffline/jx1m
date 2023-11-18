using GameServer.KiemThe.Core.Item;
using GameServer.Logic;

namespace GameServer.KiemThe
{
    /// <summary>
    /// Các phương thức và đối tượng toàn cục của Kiếm Thế
    /// </summary>
    public static partial class KTGlobal
    {
        /// <summary>
        /// Số ô tối đa trong thương khố
        /// </summary>
        public const int MaxPortableBagItemCount = 100;

        /// <summary>
        /// Số ô tối đa trong túi đồ
        /// </summary>
        public const int MaxBagItemCount = 100;

        /// <summary>
        /// Trả về tổng số ô trống cần để nhận vật phẩm tương ứng
        /// </summary>
        /// <param name="itemID"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static int GetTotalSpacesNeedToTakeItem(int itemID, int count)
        {
            if (ItemManager._TotalGameItem.TryGetValue(itemID, out ItemData itemData))
            {
                /// Nếu là trang bị
                if (ItemManager.KD_ISEQUIP(itemData.Genre))
                {
                    return count;
                }
                /// Tổng stack trên 1 ô
                int nStack = itemData.Stack;
                if (nStack <= 0)
                {
                    nStack = 1;
                }

                /// Trả về số lượng
                return count / nStack + (count % nStack == 0 ? 0 : 1);
            }
            return count;
        }

        /// <summary>
        /// Kiểm tra người chơi có đủ số ô trống tương ứng trong túi đồ không
        /// </summary>
        /// <param name="spaceNeed"></param>
        /// <param name="player"></param>
        /// <returns></returns>
        public static bool IsHaveSpace(int spaceNeed, KPlayer player)
        {
            return player.GoodsData.GetFreeBagSpaces(0) >= spaceNeed;
        }
    }
}
