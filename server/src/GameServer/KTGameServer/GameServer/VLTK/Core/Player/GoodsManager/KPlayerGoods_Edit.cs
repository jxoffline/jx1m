using GameServer.KiemThe.Entities;
using GameServer.Logic;
using GameServer.Server;
using Server.Data;
using Server.Protocol;
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
        #region Thêm vật phẩm
        /// <summary>
        /// Thêm vật phẩm vào danh sách tương ứng
        /// </summary>
        /// <param name="itemGD"></param>
        public void Add(GoodsData itemGD)
        {
            this.goodsData[itemGD.Id] = itemGD;
        }

        /// <summary>
        /// Thêm toàn bộ các vật phẩm vào danh sách tương ứng
        /// </summary>
        /// <param name="goodsData"></param>
        public void AddAll(IEnumerable<GoodsData> goodsData)
        {
            /// Duyệt danh sách
            foreach (GoodsData itemGD in goodsData)
            {
                /// Thêm vật phẩm tương ứng
                this.Add(itemGD);
            }
        }
        #endregion

        #region Xóa vật phẩm
        /// <summary>
        /// Xóa toàn bộ các vật phẩm thỏa mãn điều kiện tương ứng
        /// </summary>
        /// <param name="predicate"></param>
        public void RemoveAll(Predicate<GoodsData> predicate)
        {
            /// Danh sách khóa
            List<int> keys = this.goodsData.Keys.ToList();
            /// Duyệt danh sách khóa
            foreach (int key in keys)
            {
                /// Nếu không tồn tại
                if (!this.goodsData.TryGetValue(key, out GoodsData itemGD))
                {
                    /// Bỏ qua
                    continue;
                }

                /// Nếu thỏa mãn Predicate
                if (predicate(itemGD))
                {
                    /// Xóa vật phẩm tương ứng
                    this.Remove(itemGD);
                }
            }
        }

        /// <summary>
        /// Xóa toàn bộ các vật phẩm tương ứng trong danh sách
        /// </summary>
        /// <param name="goodsData"></param>
        public void RemoveAll(IEnumerable<GoodsData> goodsData)
        {
            this.RemoveAll(x => goodsData.Contains(x));
        }

        /// <summary>
        /// Xóa vật phẩm tương ứng
        /// </summary>
        /// <param name="itemGD">Vật phẩm</param>
        /// <param name="alsoClearQuantity">Đồng thời thiết lập số lượng về 0 (cái này trong giao dịch đặt vật phẩm lên sẽ không dùng)</param>
        public void Remove(GoodsData itemGD, bool alsoClearQuantity = true)
        {
            /// Xóa khỏi danh sách
            this.goodsData.TryRemove(itemGD.Id, out itemGD);
            /// Xóa số lượng do BUG cái đéo gì đó (quan trọng)
            if (alsoClearQuantity)
            {
                itemGD.GCount = 0;
            }
        }
        #endregion

        #region Sửa vật phẩm
        /// <summary>
        /// Thực hiện thay đổi cục bộ giá trị của vật phẩm theo danh sách tương ứng
        /// </summary>
        /// <param name="itemDbID"></param>
        /// <param name="modifyDict"></param>
        public void LocalModify(int itemDbID, Dictionary<UPDATEITEM, object> modifyDict)
        {
            /// Thông tin vật phẩm
            GoodsData itemGD = this.Find(itemDbID);
            /// Toác
            if (itemGD == null)
            {
                return;
            }

            /// Vị trí trong túi
            if (modifyDict.ContainsKey(UPDATEITEM.BAGINDEX))
            {
                itemGD.BagIndex = (int) modifyDict[UPDATEITEM.BAGINDEX];
            }

            /// Ngũ hành
            if (modifyDict.ContainsKey(UPDATEITEM.SERIES))
            {
                itemGD.Series = (int) modifyDict[UPDATEITEM.SERIES];
            }

            /// Vị trí túi nào
            if (modifyDict.ContainsKey(UPDATEITEM.SITE))
            {
                itemGD.Site = (int) modifyDict[UPDATEITEM.SITE];
            }

            /// Hạn sử dụng
            if (modifyDict.ContainsKey(UPDATEITEM.END_TIME))
            {
                itemGD.Endtime = ((string) modifyDict[UPDATEITEM.END_TIME]).Replace("#", ":");
            }

            /// Tham biến khác
            if (modifyDict.ContainsKey(UPDATEITEM.OTHER_PRAM))
            {
                itemGD.OtherParams = (Dictionary<ItemPramenter, string>) modifyDict[UPDATEITEM.OTHER_PRAM];
            }

            /// Thuộc tính
            if (modifyDict.ContainsKey(UPDATEITEM.PROPS))
            {
                itemGD.Props = (string) modifyDict[UPDATEITEM.PROPS];
            }

            /// Độ bền
            if (modifyDict.ContainsKey(UPDATEITEM.STRONG))
            {
                itemGD.Strong = (int) modifyDict[UPDATEITEM.STRONG];
            }

            /// Khóa
            if (modifyDict.ContainsKey(UPDATEITEM.BINDING))
            {
                itemGD.Binding = (int) modifyDict[UPDATEITEM.BINDING];
            }

            /// Cường hóa
            if (modifyDict.ContainsKey(UPDATEITEM.FORGE_LEVEL))
            {
                itemGD.Forge_level = (int) modifyDict[UPDATEITEM.FORGE_LEVEL];
            }

            /// Trang bị
            if (modifyDict.ContainsKey(UPDATEITEM.USING))
            {
                itemGD.Using = (int) modifyDict[UPDATEITEM.USING];
            }

            /// Số lượng
            if (modifyDict.ContainsKey(UPDATEITEM.GCOUNT))
            {
                /// Nếu đã hết
                if ((int) modifyDict[UPDATEITEM.GCOUNT] <= 0)
                {
                    /// Xóa vật phẩm
                    this.Remove(itemGD);
                }
                /// Nếu chưa hết
                else
                {
                    /// Cập nhật số lượng
                    itemGD.GCount = (int) modifyDict[UPDATEITEM.GCOUNT];
                }
            }
        }

        /// <summary>
        /// Cập nhật vật phẩm vào hệ thống
        /// </summary>
        /// <param name="itemGD">Vật phẩm cần cập nhật</param>
        /// <param name="updateList">Danh sách cập nhật</param>
        /// <param name="updateDB">Cập nhật luôn vào DB không hay để ghi sau khi nhân vật thoát Game</param>
        /// <param name="notifyClient">Thông báo về client luôn không</param>
        /// <param name="fromSource">Từ nguồn nào (dùng để ghi Log)</param>
        /// <returns></returns>
        public bool Update(GoodsData itemGD, Dictionary<UPDATEITEM, object> updateList, bool updateDB = true, bool notifyClient = true, string fromSource = "")
        {
            /// Thông tin vật phẩm tương ứng
            ItemData itemData = ItemManager.GetItemTemplate(itemGD.GoodsID);
            /// Không tồn tại
            if (itemData == null)
            {
                LogManager.WriteLog(LogTypes.Item, "[" + this.Owner.strUserID + "][" + this.Owner.RoleID + "][" + this.Owner.RoleName + "][" + fromSource + "] Cập nhật thất bại vật phẩm không tồn tại");
                return false;
            }

            /// Toác ID
            if (itemGD.Id == -1)
            {
                LogManager.WriteLog(LogTypes.Item, "[" + this.Owner.strUserID + "][" + this.Owner.RoleID + "][" + this.Owner.RoleName + "][" + fromSource + "] Cập nhật thất bại vật phẩm không hợp lệ");
                return false;
            }

            /// Toác số lượng
            if (itemGD.GCount <= 0)
            {
                LogManager.WriteLog(LogTypes.Item, "[" + this.Owner.strUserID + "][" + this.Owner.RoleID + "][" + this.Owner.RoleName + "][" + fromSource + "Cập nhật thất bại số lượng còn lại không hợp lệ ");
                return false;
            }

            /// Tìm lại trên người cho chắc
            GoodsData findAgainItemGD = this.Find(itemGD.Id);
            if (findAgainItemGD == null)
            {
                LogManager.WriteLog(LogTypes.Item, "[" + this.Owner.strUserID + "][" + this.Owner.RoleID + "][" + this.Owner.RoleName + "][" + fromSource + "] Cập nhật thất bại do vật phẩm không phải của chủ nhân");
                return false;
            }

            /// Nếu có yêu cầu cập nhật vào GameDB
            if (updateDB)
            {
                /// Chuỗi mã hóa
                string updateString = ItemManager.ItemUpdateScriptBuild(updateList);
                /// Gửi yêu cầu lên vào DB
                TCPProcessCmdResults dbRequestResult = Global.RequestToDBServer(Global._TCPManager.tcpClientPool, Global._TCPManager.TcpOutPacketPool, (int) TCPGameServerCmds.CMD_DB_UPDATEGOODS_CMD, updateString, out string[] dbFields, this.Owner.ServerId);

                /// Toác
                if (dbRequestResult == TCPProcessCmdResults.RESULT_FAILED)
                {
                    LogManager.WriteLog(LogTypes.Item, "[" + this.Owner.strUserID + "][" + this.Owner.RoleID + "][" + this.Owner.RoleName + "][" + fromSource + "] Cập nhật vật phẩm thất bại: [" + itemGD.Id + "|" + itemGD.GoodsID + "] Số lượng không hợp lệ");
                    return false;
                }
                /// Nếu có dữ liệu
                else if (dbRequestResult == TCPProcessCmdResults.RESULT_DATA)
                {
                    /// Thực hiện update lại vật phẩm trong túi đồ
                    this.LocalModify(itemGD.Id, updateList);

                    /// Nếu có thông báo về Client
                    if (notifyClient)
                    {
                        /// Tìm vật phẩm trong túi đồ
                        GoodsData goods = this.Find(itemGD.Id);
                        /// Nếu tìm thấy
                        if (goods != null)
                        {
                            /// Gửi thông báo cho Client
                            ItemManager.NotifyGoodsInfo(this.Owner, goods);
                        }
                    }
                    
                    /// OK
                    return true;
                }

                /// Toác
                return false;
            }
            /// Nếu không có yêu cầu cập nhật vào GameDB
            else
            {
                /// Update tạm
                this.LocalModify(itemGD.Id, updateList);

                /// Nếu có thông báo về Client
                if (notifyClient)
                {
                    /// Tìm vật phẩm trong túi đồ
                    GoodsData goods = this.Find(itemGD.Id);
                    /// Nếu tìm thấy
                    if (goods != null)
                    {
                        /// Gửi thông báo cho Client
                        ItemManager.NotifyGoodsInfo(this.Owner, goods);
                    }
                }

                /// OK
                return true;
            }
        }
        #endregion
    }
}
