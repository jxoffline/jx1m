using GameServer.KiemThe.Core.Item;
using GameServer.KiemThe.Entities;
using GameServer.KiemThe.Logic;
using GameServer.Logic;
using GameServer.Server;
using Server.Data;
using Server.Tools;
using System;
using System.Collections.Generic;

namespace GameServer.KiemThe.Core
{
    /// <summary>
    /// Quản lý thư
    /// </summary>
    public static partial class KTMailManager
    {
        /// <summary>
        /// Chuyển thông tin vật phẩm thành dạng gửi trong thư
        /// </summary>
        /// <param name="itemGD"></param>
        /// <returns></returns>
        public static string GoodsToMailGoodsString(GoodsData itemGD)
        {
            if (itemGD == null)
            {
                return "";
            }
            string otherParamString = itemGD.OtherParams == null ? "" : Convert.ToBase64String(DataHelper.ObjectToBytes<Dictionary<ItemPramenter, string>>(itemGD.OtherParams));
            return string.Format("{0}_{1}_{2}_{3}_{4}_{5}_{6}_{7}", itemGD.GoodsID, itemGD.Forge_level, itemGD.Props, itemGD.GCount, itemGD.Binding, itemGD.Series, otherParamString, itemGD.Strong);
        }

        /// <summary>
        /// Gửi thư cho người chơi với nội dung tương ứng
        /// </summary>
        /// <param name="client"></param>
        /// <param name="title"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public static int SendSystemMailToPlayer(KPlayer client, string title, string content)
        {
            return KTMailManager.SendSystemMailToPlayer(client, new List<GoodsData>(), title, content, 0, 0);
        }

        /// <summary>
        /// Gửi thư cho người chơi với nội dung tương ứng kèm bạc khóa và đồng khóa
        /// </summary>
        /// <param name="client"></param>
        /// <param name="title"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public static int SendSystemMailToPlayer(KPlayer client, string title, string content, int boundMoney, int boundToken)
        {
            return KTMailManager.SendSystemMailToPlayer(client, new List<GoodsData>(), title, content, boundMoney, boundToken);
        }

        /// <summary>
        /// Gửi thư cho người chơi kèm vật phẩm có ID tương ứng kèm bạc khóa và đồng khóa
        /// </summary>
        /// <param name="client"></param>
        /// <param name="itemID"></param>
        /// <param name="itemCount"></param>
        /// <param name="binding"></param>
        /// <param name="enhanceLevel"></param>
        /// <param name="title"></param>
        /// <param name="content"></param>
        /// <param name="boundMoney"></param>
        /// <param name="boundToken"></param>
        /// <returns></returns>
        public static int SendSystemMailToPlayerWithItemID(KPlayer client, int itemID, int itemCount, int binding, int enhanceLevel, string title, string content, int boundMoney, int boundToken)
        {
            /// Nếu không tồn tại vật phẩm
            if (!ItemManager._TotalGameItem.TryGetValue(itemID, out ItemData itemData))
            {
                return 0;
            }

            GoodsData goodsData = ItemManager.CreateGoodsFromItemID(itemID);
            goodsData.Id = -1;
            goodsData.Using = -1;
            goodsData.Forge_level = enhanceLevel;
            goodsData.GCount = itemCount;
            goodsData.Binding = binding;
            goodsData.Strong = 100;

            /// Thực thi hàm gửi thư
            return KTMailManager.SendSystemMailToPlayer(client, goodsData, title, content, boundToken, boundMoney);
        }

        /// <summary>
        /// Gửi thư cho người chơi kèm danh sách vật phẩm có ID tương ứng kèm bạc khóa và đồng khóa
        /// </summary>
        /// <param name="client"></param>
        /// <param name="itemInfos"></param>
        /// <param name="title"></param>
        /// <param name="content"></param>
        /// <param name="boundMoney"></param>
        /// <param name="boundToken"></param>
        /// <returns></returns>
        public static int SendSystemMailToPlayerWithItemIDs(KPlayer client, List<Tuple<int, int, int, int>> itemInfos, string title, string content, int boundMoney, int boundToken)
        {
            List<GoodsData> goods = new List<GoodsData>();

            /// Duyệt danh sách
            foreach (Tuple<int, int, int, int> itemInfo in itemInfos)
            {
                int itemID = itemInfo.Item1;
                int itemCount = itemInfo.Item2;
                int binding = itemInfo.Item3;
                int enhanceLevel = itemInfo.Item4;

                /// Tạo vật phẩm tương ứng
                GoodsData goodsData = ItemManager.CreateGoodsFromItemID(itemID);
                goodsData.Id = -1;
                goodsData.Using = -1;
                goodsData.Forge_level = enhanceLevel;
                goodsData.GCount = itemCount;
                goodsData.Binding = binding;
                goodsData.Strong = 100;

                /// Thêm vào danh sách
                goods.Add(goodsData);
            }

            /// Thực thi hàm gửi thư
            return KTMailManager.SendSystemMailToPlayer(client, goods, title, content, boundToken, boundMoney);
        }

        /// <summary>
        /// Gửi thư cho người chơi kèm vật phẩm tương ứng
        /// </summary>
        /// <param name="client"></param>
        /// <param name="goodsData"></param>
        /// <param name="title"></param>
        /// <param name="content"></param>
        /// <param name="boundToken"></param>
        /// <param name="boundMoney"></param>
        /// <returns></returns>
        public static int SendSystemMailToPlayer(KPlayer client, GoodsData goodsData, string title, string content, int boundToken, int boundMoney)
        {
            return KTMailManager.SendSystemMailToPlayer(client, new List<GoodsData>() { goodsData }, title, content, boundToken, boundMoney);
        }



        /// <summary>
        /// Gửi thư kèm vật phẩm, đồng khóa, bạc khóa cho người chơi tương ứng
        /// </summary>
        /// <param name="client"></param>
        /// <param name="goodsData"></param>
        /// <param name="title"></param>
        /// <param name="content"></param>
        /// <param name="boundToken"></param>
        /// <param name="boundMoney"></param>
        /// <returns></returns>
        public static int SendSystemMailToPlayer(KPlayer client, List<GoodsData> goodsData, string title, string content, int boundToken, int boundMoney)
        {
            string mailGoodsString = "";

            /// Nếu danh sách vật phẩm tồn tại
            if (null != goodsData && goodsData.Count > 0)
            {
                /// Duyệt danh sách vật phẩm
                foreach (GoodsData itemGD in goodsData)
                {
                    mailGoodsString += KTMailManager.GoodsToMailGoodsString(itemGD) + "|";
                }
                /// Xóa ký tự thừa ở cuối
                mailGoodsString = mailGoodsString.Remove(mailGoodsString.Length - 1);
            }

            string strDbCmd = string.Format("{0}:{1}:{2}:{3}:{4}:{5}:{6}:{7}:{8}:{9}", -1, "Hệ thống", client.RoleID, client.RoleName, title, content, boundToken, boundMoney, mailGoodsString,0);

            string[] fieldsData = null;
            /// Thực thi gửi thư
            fieldsData = Global.ExecuteDBCmd((int) TCPGameServerCmds.CMD_DB_SENDUSERMAIL, strDbCmd, client.ServerId);

            KT_TCPHandler.CheckEmailCount(client);

            /// Nếu có lỗi gì đó
            if (null == fieldsData || fieldsData.Length != 3)
            {
                return 0;
            }

            /// Trả về ID thư
            return Convert.ToInt32(fieldsData[1]);
        }

   
    }
}
