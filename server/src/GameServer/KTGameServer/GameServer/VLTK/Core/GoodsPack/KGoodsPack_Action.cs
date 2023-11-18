using GameServer.KiemThe;
using GameServer.KiemThe.Core.Item;
using GameServer.KiemThe.Entities;
using GameServer.KiemThe.Logic;
using Server.Tools;
using System.Collections.Generic;

namespace GameServer.Logic
{
    /// <summary>
    /// Định nghĩa vật phẩm rơi dưới đất
    /// </summary>
    public partial class KGoodsPack
    {
        /// <summary>
        /// Thời điểm sẽ cho phép toàn bộ người chơi có thể nhặt được
        /// </summary>
        private const int EnableGlobalPickingAfterTicks = 15000;

        /// <summary>
        /// Thời gian tồn tại tối đa
        /// </summary>
        public const int GoodsPackKeepTimes = 60000;

        /// <summary>
        /// Người chơi nhặt vật phẩm
        /// </summary>
        /// <param name="player"></param>
        public void PickUp(KPlayer player)
        {
            /// Toác
            if (player == null)
            {
                return;
            }
            /// Nếu không cùng bản đồ
            else if (player.CurrentMapCode != this.CurrentMapCode || player.CurrentCopyMapID != this.CurrentCopyMapID)
            {
                /// Thông báo
                KTPlayerManager.ShowNotification(player, "Vật phẩm không tồn tại!");
                /// Xóa ở Client cho chắc
                KTGoodsPackManager.DelMySelfGoodsPacks(player, this);
                /// Bỏ qua
                return;
            }
            /// Nếu không còn tồn tại nữa
            else if (!this.IsAlive)
            {
                /// Thông báo
                KTPlayerManager.ShowNotification(player, "Vật phẩm không tồn tại!");
                /// Xóa ở Client cho chắc
                KTGoodsPackManager.DelMySelfGoodsPacks(player, this);
                /// Bỏ qua
                return;
            }
            /// Nếu đã hết thời gian
            else if (this.LifeTime >= KGoodsPack.GoodsPackKeepTimes)
            {
                /// Thông báo
                KTPlayerManager.ShowNotification(player, "Vật phẩm không tồn tại!");
                /// Xóa ở Client cho chắc
                KTGoodsPackManager.DelMySelfGoodsPacks(player, this);
                /// Bỏ qua
                return;
            }

            /// Vị trí của người chơi
            UnityEngine.Vector2 playerPos = new UnityEngine.Vector2((int) player.CurrentPos.X, (int) player.CurrentPos.Y);
            /// Vị trí hiện tại của vật phẩm
            UnityEngine.Vector2 gpPos = new UnityEngine.Vector2((int) this.CurrentPos.X, (int) this.CurrentPos.Y);
            /// Khoảng cách
            float distance = UnityEngine.Vector2.Distance(gpPos, playerPos);
            /// Nếu quá xa
            if (distance > 100)
            {
                KTPlayerManager.ShowNotification(player, "Khoảng cách quá xa, không thể nhặt vật phẩm!");
                return;
            }

            /// Nếu có chủ nhân
            if (this.OwnerRoleIDs != null)
            {
                /// Nếu bản thân không phải chủ nhân
                if (!this.OwnerRoleIDs.Contains(player.RoleID))
                {
                    /// Nếu chưa đến thời gian Public ra ngoài
                    if (this.LifeTime < KGoodsPack.EnableGlobalPickingAfterTicks)
                    {
                        /// Số giây sau có thể nhặt được
                        int secLeft = (int) ((KGoodsPack.EnableGlobalPickingAfterTicks - this.LifeTime) / 1000);
                        /// Thông báo
                        KTPlayerManager.ShowNotification(player, string.Format("Vật phẩm thuộc về người khác, {0} giây sau mới có thể nhặt!", secLeft));
                        /// Bỏ qua
                        return;
                    }
                }
            }

            /// Số ô trống cần để nhặt vật phẩm
            int spacesNeed = KTGlobal.GetTotalSpacesNeedToTakeItem(this.GoodsData.GoodsID, this.GoodsData.GCount);
            /// Nếu không đủ ô trống
            if (!KTGlobal.IsHaveSpace(spacesNeed, player))
            {
                /// Thông báo
                KTPlayerManager.ShowNotification(player, string.Format("Túi đã đầy, cần sắp xếp ít nhất {0} ô trống để nhặt!", spacesNeed));
            }

            ///// Thêm cái này nữa cho chắc
            //if (!this.IsAlive)
            //{
            //    /// Thông báo
            //    PlayerManager.ShowNotification(player, "Vật phẩm không tồn tại!");
            //    /// Bỏ qua
            //    return;
            //}
            ///// Nếu đã hết thời gian
            //else if (this.LifeTime >= KGoodsPack.GoodsPackKeepTimes)
            //{
            //    /// Thông báo
            //    PlayerManager.ShowNotification(player, "Vật phẩm không tồn tại!");
            //    /// Bỏ qua
            //    return;
            //}

            /// Hủy vật phẩm
            this.Destroy();

            /// Nếu không có DbID tức rơi từ nguồn không phải người chơi
            if (this.GoodsData.Id == -1)
            {
                /// Thực hiện tạo vật phẩm
                if (ItemManager.CreateItem(Global._TCPManager.TcpOutPacketPool, player, this.GoodsData.GoodsID, this.GoodsData.GCount, 0, "DropMap", true, this.GoodsData.Binding, true, ItemManager.ConstGoodsEndTime, this.GoodsData.Props, this.GoodsData.Series, this.GoodsData.Creator, this.GoodsData.Forge_level, 1, this.EnableWriteLogOnPickUp))
                {
                    /// Gọi hàm xử lý khi nhặt được vật phẩm
                    KTMonsterManager.MonsterDropManager.ProcessPickUpItem(player, this.GoodsData, this.Source);
                    /// Thực thi sự kiện người chơi nhặt vật phẩm rơi dưới đất
                    player.OnPickUpItem(this.GoodsData);

                    /// Nếu có nguồn rơi
                    if (this.Source != null)
                    {
                        /// Ghi Log
                        LogManager.WriteLog(LogTypes.PickUpNotify, "[" + player.strUserID + "][" + player.RoleID + "][" + player.RoleName + "][NHẶT VẬT PHẨM]["+this.GoodsData.Id+"][" + this.GoodsData.GoodsID + "][" + ItemManager.GetNameItem(this.GoodsData) + "] từ [" + this.Source.RoleID + "] [" + this.Source.RoleName + "]");
                    }
                    /// Nếu không có nguồn rơi
                    else
                    {
                        /// Ghi Log
                        LogManager.WriteLog(LogTypes.PickUpNotify, "[" + player.strUserID + "][" + player.RoleID + "][" + player.RoleName + "][NHẶT VẬT PHẨM]["+this.GoodsData.Id+"][" + this.GoodsData.GoodsID + "][" + ItemManager.GetNameItem(this.GoodsData) + "]");
                    }
                }
                else
                {
                    KTPlayerManager.ShowNotification(player, "Có lỗi khi nhặt vật phẩm!");
                }
            }
            /// Nếu có DbID tức rơi từ nguồn là người chơi
            else
            {
                /// Lấy vị trí trống đầu tiên trong túi đồ thằng này
                int bagPos = player.GoodsData.GetBagFirstEmptyPosition(0);
                /// Nếu không tìm thấy thì thôi
                if (bagPos == -1)
                {
                    KTPlayerManager.ShowNotification(player, "Túi đồ đã đầy, không thể nhặt vật phẩm!");
                    return;
                }

                /// Cập nhật thông tin vật phẩm
                if (!KTTradeManager.TakeItemFromOther(Global._TCPManager.MySocketListener, Global._TCPManager.tcpClientPool, Global._TCPManager.TcpOutPacketPool, this.GoodsData, this.Source.RoleID, player, true, "TAKEITEMFORMOTHER", -1))
                {
                    LogManager.WriteLog(LogTypes.Item, "TAKE ITEM BUG: " + this.GoodsData.Id);
                }

                /// Gọi hàm xử lý khi nhặt được vật phẩm
                KTMonsterManager.MonsterDropManager.ProcessPickUpItem(player, this.GoodsData, this.Source);
                /// Thực thi sự kiện người chơi nhặt vật phẩm rơi dưới đất
                player.OnPickUpItem(this.GoodsData);
            }
        }

        /// <summary>
        /// Hủy đối tượng
        /// </summary>
        /// <param name="alsoRemoveFromManager">Tham biến này mặc định là True, nếu được gọi từ chính Manager thì sẽ là False</param>
        public void Destroy(bool alsoRemoveFromManager = true)
        {
            /// Đánh dấu là không tồn tại
            this.IsAlive = false;

            /// Nếu đồng thời xóa khỏi Manager
            if (alsoRemoveFromManager)
            {
                /// Xóa khỏi luồng quản lý
                KTGoodsPackManager.Remove(this);
            }
        }
    }
}
