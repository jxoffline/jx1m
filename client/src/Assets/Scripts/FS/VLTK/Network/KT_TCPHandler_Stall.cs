using FS.GameEngine.Logic;
using FS.GameEngine.Network;
using FS.GameEngine.Sprite;
using FS.VLTK.Factory.UIManager;
using HSGameEngine.GameEngine.Network.Protocol;
using Server.Data;
using Server.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FS.VLTK.Network
{
    /// <summary>
    /// Quản lý tương tác với Socket
    /// </summary>
    public static partial class KT_TCPHandler
    {
        #region Có sạp mới
        /// <summary>
        /// Nhận gói tin thông báo có gian hàng người chơi mới mở
        /// </summary>
        /// <param name="fields"></param>
        public static void ReceiveRoleNewStall(List<string> fields)
        {
            /// Nếu chưa tải map xong
            if (!Global.Data.GameScene.MapLoadingCompleted)
            {
                /// Bỏ qua
                return;
            }

            /// ID người chơi
            int roleID = int.Parse(fields[0]);
            /// Tên sạp hàng
            string stallName = fields[1];

            /// Đối tượng tương ứng
            GSprite sprite = null;

            /// Nếu là bản thân
            if (roleID == Global.Data.RoleData.RoleID)
            {
                Global.Data.RoleData.StallName = stallName;
                sprite = Global.Data.Leader;
            }
            /// Nếu là người chơi khác
            else if (Global.Data.OtherRoles.TryGetValue(roleID, out RoleData roleData))
            {
                roleData.StallName = stallName;
                sprite = KTGlobal.FindSpriteByID(roleID);
            }

            /// Nếu đối tượng tồn tại
            if (sprite != null)
            {
                /// Ngừng StoryBoard
                sprite.StopMove();

                /// Hiển thị tên cửa hàng
                sprite.ComponentCharacter.ShowMyselfShopName(stallName);
                /// Thực hiện động tác ngồi
                sprite.DoSit();
            }
        }

        /// <summary>
        /// Nhận gói tin thông báo có gian hàng người chơi bị đóng
        /// </summary>
        /// <param name="fields"></param>
        public static void ReceiveRoleStopStall(string[] fields)
        {
            /// Nếu chưa tải map xong
            if (!Global.Data.GameScene.MapLoadingCompleted)
            {
                /// Bỏ qua
                return;
            }

            /// ID người chơi
            int roleID = Convert.ToInt32(fields[0]);

            /// Đối tượng tương ứng
            GSprite sprite = null;

            /// Nếu là bản thân
            if (roleID == Global.Data.RoleData.RoleID)
            {
                Global.Data.RoleData.StallName = "";
                sprite = Global.Data.Leader;
            }
            /// Nếu là người chơi khác
            else if (Global.Data.OtherRoles.TryGetValue(roleID, out RoleData rd))
            {
                rd.StallName = "";
                sprite = KTGlobal.FindSpriteByID(roleID);
            }

            /// Nếu đối tượng tồn tại
            if (sprite != null)
            {
                /// Ngừng StoryBoard
                sprite.StopMove();

                /// Đóng bảng tên cửa hàng
                sprite.ComponentCharacter.HideMyselfShopName();
                /// Thực hiện đứng lên
                sprite.DoStand();
            }
        }
        #endregion

        #region Mở sạp hàng
        /// <summary>
        /// Nhận thông báo thông tin sạp hàng cũ
        /// </summary>
        /// <param name="cmdID"></param>
        /// <param name="cmdData"></param>
        /// <param name="length"></param>
        public static void ReceiveStallData(int cmdID, byte[] cmdData, int length)
        {
            /// Thông tin sạp hàng
            StallData stallData = DataHelper.BytesToObject<StallData>(cmdData, 0, length);
            /// Toác
            if (stallData == null)
            {
                /// Bỏ qua
                return;
            }

            /// Nếu là sạp hàng của bản thân
            if (stallData.RoleID == Global.Data.RoleData.RoleID)
            {
                /// Ghi lại thông tin sạp hàng bản thân
                Global.Data.StallDataItem = stallData;

                /// Nếu đang mở sạp hàng
                if (PlayZone.Instance.UIPlayerShop_Sell != null)
                {
                    /// Cập nhật dữ liệu
                    PlayZone.Instance.UIPlayerShop_Sell.RefreshShop();
                }
                /// Nếu chưa mở sạp hàng
                else
                {
                    /// Mở sạp
                    PlayZone.Instance.ShowUIPlayerShop_Sell();
                }
            }
            else
            {
                /// Nếu đang mở khung
                if (PlayZone.Instance.UIPlayerShop_Buy != null)
                {
                    PlayZone.Instance.UIPlayerShop_Buy.Data = stallData;
                }
                /// Nếu chưa mở khung
                else
                {
                    /// Mở khung
                    PlayZone.Instance.ShowUIPlayerShop_Buy(stallData);
                }
            }
        }

        /// <summary>
        /// Gửi yêu cầu mở sạp hàng của thằng tương ứng
        /// </summary>
        /// <param name="roleID"></param>
        public static void SendOpenStall(int roleID)
        {
            string cmdData = string.Format("{0}", roleID);
            byte[] bytes = new ASCIIEncoding().GetBytes(cmdData);
            GameInstance.Game.GameClient.SendData(TCPOutPacket.MakeTCPOutPacket(GameInstance.Game.GameClient.OutPacketPool, bytes, 0, bytes.Length, (int) (TCPGameServerCmds.CMD_SPR_STALLDATA)));
        }
        #endregion

        #region Bắt đầu bán hàng
        /// <summary>
        /// Gửi gói tin bắt đầu bán hàng
        /// </summary>
        /// <param name="stallName"></param>
        /// <param name="commissionBot"></param>
        public static void SendStartStall(string stallName, bool commissionBot)
        {
            /// Dữ liệu
            StallAction data = new StallAction()
            {
                Type = (int) GoodsStallCmds.Start,
                Fields = new List<string>()
                {
                    stallName,
                    commissionBot.ToString(),
                },
            };
            /// Chuỗi Byte dữ liệu
            byte[] bytes = DataHelper.ObjectToBytes<StallAction>(data);
            GameInstance.Game.GameClient.SendData(TCPOutPacket.MakeTCPOutPacket(GameInstance.Game.GameClient.OutPacketPool, bytes, 0, bytes.Length, (int) (TCPGameServerCmds.CMD_SPR_GOODSSTALL)));
        }

        /// <summary>
        /// Nhận thông báo bắt đầu gian hàng
        /// </summary>
        /// <param name="fields"></param>
        public static void ReceiveStartStall(List<string> fields)
        {
            /// Nếu không mở gian hàng
            if (PlayZone.Instance.UIPlayerShop_Sell == null)
            {
                /// Bỏ qua
                return;
            }
            /// Nếu không có dữ liệu
            else if (Global.Data.StallDataItem == null)
            {
                /// Bỏ qua
                return;
            }

            /// Tên gian hàng
            string stallName = fields[0];
            /// Có phải bot không
            bool commissionBot = bool.Parse(fields[1]);

            /// Đồng bộ dữ liệu
            Global.Data.StallDataItem.Start = 1;
            Global.Data.StallDataItem.StallName = stallName;
            Global.Data.StallDataItem.IsBot = commissionBot;

            /// Cập nhật dữ liệu khung
            PlayZone.Instance.UIPlayerShop_Sell.RefreshShop();
        }
        #endregion

        #region Hủy bán hàng
        /// <summary>
        /// Gửi gói tin hủy bán hàng
        /// </summary>
        /// <param name="stallName"></param>
        /// <param name="commissionBot"></param>
        public static void SendStopStall()
        {
            /// Dữ liệu
            StallAction data = new StallAction()
            {
                Type = (int) GoodsStallCmds.Cancel,
                Fields = null,
            };
            /// Chuỗi Byte dữ liệu
            byte[] bytes = DataHelper.ObjectToBytes<StallAction>(data);
            GameInstance.Game.GameClient.SendData(TCPOutPacket.MakeTCPOutPacket(GameInstance.Game.GameClient.OutPacketPool, bytes, 0, bytes.Length, (int) (TCPGameServerCmds.CMD_SPR_GOODSSTALL)));
        }

        /// <summary>
        /// Nhận thông báo hủy gian hàng
        /// </summary>
        /// <param name="fields"></param>
        public static void ReceiveStopStall()
        {
            /// Nếu không có dữ liệu
            if (Global.Data.StallDataItem == null)
            {
                /// Bỏ qua
                return;
            }

            /// Cập nhật trạng thái
            Global.Data.StallDataItem.Start = 0;

            /// Xóa shop
            Global.Data.Leader.ComponentCharacter.HideMyselfShopName();
        }
        #endregion

        #region Rao bán / Thu hồi vật phẩm
        /// <summary>
        /// Gửi gói tin thêm vật phẩm vào sạp hàng tương ứng
        /// </summary>
        /// <param name="itemDbID"></param>
        /// <param name="price"></param>
        public static void SendAddItemToStall(int itemDbID, int price)
        {
            /// Dữ liệu
            StallAction data = new StallAction()
            {
                Type = (int) GoodsStallCmds.AddGoods,
                Fields = new List<string>()
                {
                    itemDbID.ToString(),
                    price.ToString(),
                },
            };
            /// Chuỗi Byte dữ liệu
            byte[] bytes = DataHelper.ObjectToBytes<StallAction>(data);
            GameInstance.Game.GameClient.SendData(TCPOutPacket.MakeTCPOutPacket(GameInstance.Game.GameClient.OutPacketPool, bytes, 0, bytes.Length, (int) (TCPGameServerCmds.CMD_SPR_GOODSSTALL)));
        }

        /// <summary>
        /// Nhận gói tin thêm vật phẩm vào sạp hàng
        /// </summary>
        /// <param name="itemGD"></param>
        /// <param name="fields"></param>
        public static void ReceiveAddItemToStall(GoodsData itemGD, List<string> fields)
        {
            /// Giá
            int price = int.Parse(fields[0]);

            /// Nếu không có thông tin
            if (Global.Data.StallDataItem == null)
            {
                /// Bỏ qua
                return;
            }
            /// Toác
            else if (itemGD == null)
            {
                /// Bỏ qua
                return;
            }

            /// Thêm vào sạp hàng
            if (Global.Data.StallDataItem.GoodsList == null)
            {
                Global.Data.StallDataItem.GoodsList = new List<GoodsData>();
            }
            if (Global.Data.StallDataItem.GoodsPriceDict == null)
            {
                Global.Data.StallDataItem.GoodsPriceDict = new Dictionary<int, int>();
            }
            Global.Data.StallDataItem.GoodsList.Add(itemGD);
            Global.Data.StallDataItem.GoodsPriceDict[itemGD.Id] = price;

            /// Nếu đang mở khung
            if (PlayZone.Instance.UIPlayerShop_Sell != null)
            {
                /// Thêm vào sạp hàng
                PlayZone.Instance.UIPlayerShop_Sell.AddItem(itemGD, price);
            }
        }

        /// <summary>
        /// Gửi gói tin xóa vật phẩm khỏi sạp hàng tương ứng
        /// </summary>
        /// <param name="itemDbID"></param>
        public static void SendRemoveItemFromStall(int itemDbID)
        {
            /// Dữ liệu
            StallAction data = new StallAction()
            {
                Type = (int) GoodsStallCmds.RemoveGoods,
                Fields = new List<string>()
                {
                    itemDbID.ToString(),
                },
            };
            /// Chuỗi Byte dữ liệu
            byte[] bytes = DataHelper.ObjectToBytes<StallAction>(data);
            GameInstance.Game.GameClient.SendData(TCPOutPacket.MakeTCPOutPacket(GameInstance.Game.GameClient.OutPacketPool, bytes, 0, bytes.Length, (int) (TCPGameServerCmds.CMD_SPR_GOODSSTALL)));
        }

        /// <summary>
        /// Nhận gói tin xóa vật phẩm khỏi sạp hàng
        /// </summary>
        /// <param name="fields"></param>
        public static void ReceiveRemoveItemFromStall(List<string> fields)
        {
            /// ID người chơi
            int roleID = int.Parse(fields[0]);
            /// ID vật phẩm
            int itemDbID = int.Parse(fields[1]);

            /// Nếu là bản thân
            if (roleID == Global.Data.RoleData.RoleID)
            {
                /// Nếu không có thông tinto
                if (Global.Data.StallDataItem == null)
                {
                    /// Bỏ qua
                    return;
                }
                else if (Global.Data.StallDataItem.GoodsList == null || Global.Data.StallDataItem.GoodsPriceDict == null)
                {
                    /// Bỏ qua
                    return;
                }

                /// Vật phẩm tương ứng
                GoodsData itemGD = Global.Data.StallDataItem.GoodsList.Where(x => x.Id == itemDbID).FirstOrDefault();
                /// Nếu không tồn tại
                if (itemGD == null)
                {
                    /// Bỏ qua
                    return;
                }

                /// Xóa khỏi sạp hàng
                Global.Data.StallDataItem.GoodsList.Remove(itemGD);
                Global.Data.StallDataItem.GoodsPriceDict.Remove(itemGD.Id);

                /// Nếu đang mở khung
                if (PlayZone.Instance.UIPlayerShop_Sell != null)
                {
                    /// Xóa khỏi sạp hàng
                    PlayZone.Instance.UIPlayerShop_Sell.RemoveItem(itemDbID);
                }
            }
            /// Nếu là đứa khác
            else
            {
                /// Nếu đang mở khung
                if (PlayZone.Instance.UIPlayerShop_Buy != null && PlayZone.Instance.UIPlayerShop_Buy.Data.RoleID == roleID)
                {
                    /// Xóa khỏi sạp hàng
                    PlayZone.Instance.UIPlayerShop_Buy.RemoveItem(itemDbID);
                }
            }
        }
        #endregion

        #region Mua vật phẩm từ cửa hàng
        /// <summary>
        /// Gửi gói tin mua vật phẩm từ cửa hàng người chơi tương ứng
        /// </summary>
        /// <param name="roleID"></param>
        /// <param name="itemDbID"></param>
        public static void SendBuyItemFromStall(int roleID, int itemDbID)
        {
            /// Dữ liệu
            StallAction data = new StallAction()
            {
                Type = (int) GoodsStallCmds.BuyGoods,
                Fields = new List<string>()
                {
                    roleID.ToString(),
                    itemDbID.ToString(),
                },
            };
            /// Chuỗi Byte dữ liệu
            byte[] bytes = DataHelper.ObjectToBytes<StallAction>(data);
            GameInstance.Game.GameClient.SendData(TCPOutPacket.MakeTCPOutPacket(GameInstance.Game.GameClient.OutPacketPool, bytes, 0, bytes.Length, (int) (TCPGameServerCmds.CMD_SPR_GOODSSTALL)));
        }
        #endregion
    }
}
