using GameServer.KiemThe.Entities;
using GameServer.Server;
using GameServer.VLTK.Core.StallManager;
using Server.Data;
using Server.Tools;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GameServer.KiemThe.Core.Item
{
    /// <summary>
    /// Quản lý vật phẩm của người chơi
    /// </summary>
    public partial class KPlayerGoods
    {
        #region Túi đồ

        /// <summary>
        /// Thực hiện sắp xếp túi đồ tương ứng
        /// </summary>
        /// <param name="player"></param>
        /// <param name="site"></param>
        /// <param name="notifyClient"></param>
        public void SortBag(int site, bool notifyClient = true)
        {
            /// Thực hiện cập nhật thông tin vật phẩm
            bool DoUpdate(GoodsData itemGD)
            {
                /// Danh sách sẽ Update
                Dictionary<UPDATEITEM, object> updateList = new Dictionary<UPDATEITEM, object>();
                updateList.Add(UPDATEITEM.ROLEID, this.Owner.RoleID);
                updateList.Add(UPDATEITEM.ITEMDBID, itemGD.Id);
                updateList.Add(UPDATEITEM.BAGINDEX, itemGD.BagIndex);

                /// Trả về kết quả
                return this.Update(itemGD, updateList, true, true, "SortBag");
            }

            /// Danh sách vật phẩm trong túi
            List<GoodsData> goods = this.FindAll(x => x.Site == site);
            /// ID tự tăng
            int index = -1;
            /// Duyệt danh sách
            foreach (GoodsData itemGD in goods)
            {
                /// Nếu là trang bị
                if (itemGD.Using >= 0)
                {
                    /// Bỏ qua
                    continue;
                }

                /// Tăng ID
                index++;

                /// Nếu vị trí không đổi
                if (itemGD.BagIndex == index)
                {
                    /// Bỏ qua
                    continue;
                }

                /// Cập nhật vị trí
                itemGD.BagIndex = index;
                /// Cập nhật
                if (!DoUpdate(itemGD))
                {
                    /// Toác thì thoát luôn
                    break;
                }
            }

            /// Nếu có thông báo về Client
            if (notifyClient)
            {
                /// Loại túi là gì
                switch (site)
                {
                    /// Túi đồ
                    case 0:
                        {
                            /// Gửi gói tin đi
                            this.Owner.SendPacket<List<GoodsData>>((int)TCPGameServerCmds.CMD_SPR_RESETBAG, goods);
                            break;
                        }
                    /// Thương khố
                    case 1:
                        {
                            /// Gửi gói tin đi
                            this.Owner.SendPacket<List<GoodsData>>((int)TCPGameServerCmds.CMD_SPR_RESETPORTABLEBAG, goods);
                            break;
                        }
                }
            }
        }

        /// <summary>
        /// Trả về số ô trống hiện có trong túi đồ
        /// </summary>
        /// <param name="site"></param>
        /// <returns></returns>
        public int GetFreeBagSpaces(int site)
        {
            /// Tổng số ô đã dùng
            int totalUsedCells = this.FindAll(x => x.Site == site && x.GCount > 0 && x.Using < 0).Count;

            /// Kích thước túi tối đa
            int bagCapacity = -1;
            /// Loại túi là gì
            switch (site)
            {
                /// Túi đồ thường
                case 0:
                    {
                        bagCapacity = KTGlobal.MaxBagItemCount;
                        break;
                    }
                /// Thương khố
                case 1:
                    {
                        bagCapacity = KTGlobal.MaxPortableBagItemCount;
                        break;
                    }
            }

            /// Trả về kết quả
            return bagCapacity - totalUsedCells;
        }

        /// <summary>
        /// Trả về vị trí đầu tiên trống trống ở túi đồ
        /// </summary>
        /// <param name="site"></param>
        /// <returns></returns>
        public int GetBagFirstEmptyPosition(int site)
        {
            /// Danh sách các vị trí đã có vật phẩm
            HashSet<int> usedSlots = this.FindAll(x => x.Site == site).OrderBy(x => x.BagIndex).Select(x => x.BagIndex).ToHashSet();

            /// Kích thước túi tối đa
            int bagCapacity = -1;
            /// Loại túi là gì
            switch (site)
            {
                /// Túi đồ thường
                case 0:
                    {
                        bagCapacity = KTGlobal.MaxBagItemCount;
                        break;
                    }
                /// Thương khố
                case 1:
                    {
                        bagCapacity = KTGlobal.MaxPortableBagItemCount;
                        break;
                    }
            }

            /// Duyệt danh sách ô trong túi đồ
            for (int i = 0; i <= bagCapacity; i++)
            {
                /// Nếu vị trí chưa tồn tại
                if (!usedSlots.Contains(i))
                {
                    /// Trả về kết quả
                    return i;
                }
            }

            /// Không tìm thấy
            return -1;
        }

        #endregion Túi đồ

        #region Kiểm tra hợp lệ

        /// <summary>
        /// Thực hiện kiểm tra các vật phẩm
        /// </summary>
        public void Validate()
        {
            /// Nếu là center server thì bỏ qua việc check xóa bỏ
            if (this.Owner.ClientSocket.IsKuaFuLogin)
            {
                return;
            }

            /// Danh sách các vật phẩm cần xóa
            List<GoodsData> toRemoveList = this.FindAll((itemGD) =>
            {
                /// Template tương ứng
                ItemData itemData = ItemManager.GetItemTemplate(itemGD.GoodsID);
                /// Nếu không tồn tại Template
                if (itemData == null)
                {
                    /// Cần xóa
                    return true;
                }

                /// Không cần xóa
                return false;
            });

            /// Duyệt danh sách vật phẩm cần xóa
            foreach (GoodsData itemGD in toRemoveList)
            {
                /// Xóa khỏi danh sách
                this.Remove(itemGD);

                /// Danh sách sẽ Update
                Dictionary<UPDATEITEM, object> updateList = new Dictionary<UPDATEITEM, object>();
                updateList.Add(UPDATEITEM.ROLEID, this.Owner.RoleID);
                updateList.Add(UPDATEITEM.ITEMDBID, itemGD.Id);
                updateList.Add(UPDATEITEM.BAGINDEX, itemGD.BagIndex);
                /// Cập nhật
                this.Update(itemGD, updateList);
            }

            /// Danh sách vật phẩm cần Fix thời gian
            List<GoodsData> requireFixTimeList = this.FindAll((itemGD) =>
            {
                /// Template tương ứng
                ItemData itemData = ItemManager.GetItemTemplate(itemGD.GoodsID);

                /// Nếu là nguyên liệu hoawcj
                if (ItemManager.IsMaterial(itemData) || itemData.ItemID == 9679)
                {
                    /// Nếu lỗi thời hạn
                    if (itemGD.Endtime == ItemManager.ConstGoodsEndTime)
                    {
                        /// Thêm vào danh sách
                        return true;
                    }
                }

                /// Không thêm vào danh sách
                return false;
            });

            /// Duyệt danh sách vật phẩm cần Fix thời hạn
            foreach (GoodsData itemGD in requireFixTimeList)
            {
                /// Thêm 30 ngày vào
                DateTime dt = DateTime.Now.AddDays(30);
                /// Chuỗi thời gian kết thúc
                string endTimeString = dt.ToString("yyyy-MM-dd HH#mm#ss");

                /// Danh sách cập nhật
                Dictionary<UPDATEITEM, object> updateList = new Dictionary<UPDATEITEM, object>();
                updateList.Add(UPDATEITEM.ROLEID, this.Owner.RoleID);
                updateList.Add(UPDATEITEM.ITEMDBID, itemGD.Id);
                updateList.Add(UPDATEITEM.END_TIME, endTimeString);

                if (this.Update(itemGD, updateList, true, true, "FixUsageTime"))
                {
                    LogManager.WriteLog(LogTypes.Item, "[" + this.Owner.RoleID + "][" + this.Owner.RoleName + "] Fix hạn sử dụng: " + itemGD.ItemName);
                }
            }

            // Fix vật phẩm có site == 1 nhưng lại trang bị trên người
            List<GoodsData> RequestFixSite = this.FindAll((itemGD) =>
            {
                if (itemGD.Site == 1 && itemGD.Using >= 0)
                {
                    return true;
                }

                /// Không thêm vào danh sách
                return false;
            });

            foreach (GoodsData itemGD in RequestFixSite)
            {
                /// Danh sách cập nhật
                Dictionary<UPDATEITEM, object> updateList = new Dictionary<UPDATEITEM, object>();
                updateList.Add(UPDATEITEM.ROLEID, this.Owner.RoleID);
                updateList.Add(UPDATEITEM.ITEMDBID, itemGD.Id);
                updateList.Add(UPDATEITEM.SITE, 1);
                updateList.Add(UPDATEITEM.USING, -1);

                if (this.Update(itemGD, updateList, true, true, "FIXSITE"))
                {
                    LogManager.WriteLog(LogTypes.Item, "[" + this.Owner.RoleID + "][" + this.Owner.RoleName + "] FIX SITE CHO VẬT PHẨM: " + itemGD.ItemName);
                }
            }

            //Fix tất cả các vật phẩm mà đang bán ở db nhưng lại ko có trong stalldata
            List<GoodsData> RequestFixStall = this.FindAll((itemGD) =>
            {
                if (itemGD.Site == 3)
                {
                    if (!StallManager.CheckItemExitsInStallData(this.Owner.RoleID, itemGD.Id))
                    {
                        return true;
                    }
                }

                /// Không thêm vào danh sách
                return false;
            });

            //List tất cả để update lại cho nó về túi đồ
            foreach (GoodsData itemGD in RequestFixStall)
            {
                // Lấy ra vị trí trống trong túi đồ
                int SlotIndex = this.Owner.GoodsData.GetBagFirstEmptyPosition(0);
                /// Danh sách cập nhật
                Dictionary<UPDATEITEM, object> updateList = new Dictionary<UPDATEITEM, object>();
                if (SlotIndex != -1)
                {
                    updateList.Add(UPDATEITEM.ROLEID, this.Owner.RoleID);
                    updateList.Add(UPDATEITEM.ITEMDBID, itemGD.Id);
                    updateList.Add(UPDATEITEM.SITE, 0);
                    updateList.Add(UPDATEITEM.BAGINDEX, SlotIndex);

                    if (this.Update(itemGD, updateList, true, true, "FIXSTALL"))
                    {
                        LogManager.WriteLog(LogTypes.Item, "[" + this.Owner.RoleID + "][" + this.Owner.RoleName + "] FIXSTALL CHO VẬT PHẨM: " + itemGD.ItemName + "|ITEMDBID :" + itemGD.Id + "| ITEMID :" + itemGD.GoodsID);
                    }
                }
            }
        }

        #endregion Kiểm tra hợp lệ

        #region Xóa vật phẩm hết hạn

        /// <summary>
        /// Kiểm tra và xóa các vật phẩm hết hạn sử dụng
        /// </summary>
        public void RemoveExpiredItems()
        {
            /// Nếu chưa đến giờ
            if (KTGlobal.GetCurrentTimeMilis() - this.Owner.LastGoodsLimitUpdateTicks < 30000)
            {
                /// Bỏ qua
                return;
            }
            /// Cập nhật thời gian
            this.Owner.LastGoodsLimitUpdateTicks = KTGlobal.GetCurrentTimeMilis();

            /// Danh sách vật phẩm đã hết hạn
            List<GoodsData> expiredList = this.FindAll(x => ItemManager.IsGoodsTimeOver(x));
            /// Duyệt danh sách
            foreach (GoodsData itemGD in expiredList)
            {
                /// Xóa vật phẩm
                if (ItemManager.DestroyGoods(this.Owner, itemGD, "HẾT HẠN VẬT PHẨM"))
                {
                    /// Chuỗi nội dung
                    string msg = string.Format("Vật phẩm <color=yellow>[{0}]</color> đã hết hạn, tự động bị hệ thống xóa bỏ.", KTGlobal.GetItemName(itemGD));
                    /// Gửi tin nhắn đến kênh mặc định
                    KTGlobal.SendDefaultChat(this.Owner, msg);
                    /// Gửi kèm thư
                    KTMailManager.SendSystemMailToPlayer(this.Owner, "Vật phẩm hết hạn", msg);
                }
            }
        }

        #endregion Xóa vật phẩm hết hạn
    }
}