using GameServer.KiemThe.Core.Item;
using GameServer.KiemThe.Entities;
using GameServer.Logic;
using GameServer.VLTK.Core.GuildManager;
using Server.Data;
using Server.Protocol;
using Server.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace GameServer.KiemThe.Logic.Manager.Shop
{
    public class ShopManager
    {
        public static List<ShopTab> _TotalShopTab = new List<ShopTab>();

        public static List<ShopItem> _TotalShopTtem = new List<ShopItem>();

        public static GuildShop _ShopGuild = new GuildShop();

        /// <summary>
        /// Kỳ Trân Các
        /// </summary>
        public static TokenShop TokenShop { get; private set; } = new TokenShop()
        {
            Token = new List<ShopTab>(),
            BoundToken = new List<ShopTab>(),
            StoreProducts = new List<TokenShopStoreProduct>(),
        };

        public static string ShopItem_XML = "Config/KT_Shop/ShopItem.xml";

        public static string ShopTab_XML = "Config/KT_Shop/ShopTab.xml";

        // Kyf tr
        public static string TokenShopFileDir = "Config/KT_Shop/TokenShop.xml";

        //Shop Guild
        public static string Shop_Guild = "Config/KT_Shop/GuildShop.xml";

        public static void Setup()
        {
            LoadShopitem(ShopItem_XML);
            LoadShopTab(ShopTab_XML);
            LoadGuildShop(Shop_Guild);
            ShopManager.LoadTokenShop();
        }

        /// <summary>
        /// Load shop item
        /// </summary>
        /// <param name="FilesPath"></param>
        public static void LoadShopitem(string FilesPath)
        {
            string Files = KTGlobal.GetDataPath(FilesPath);
            using (var stream = System.IO.File.OpenRead(Files))
            {
                var serializer = new XmlSerializer(typeof(List<ShopItem>));
                _TotalShopTtem = serializer.Deserialize(stream) as List<ShopItem>;
            }
        }

        /// <summary>
        /// Shop bang hội
        /// </summary>
        /// <param name="FilesPath"></param>
        public static void LoadGuildShop(string FilesPath)
        {
            string Files = KTGlobal.GetDataPath(FilesPath);
            using (var stream = System.IO.File.OpenRead(Files))
            {
                var serializer = new XmlSerializer(typeof(GuildShop));
                _ShopGuild = serializer.Deserialize(stream) as GuildShop;
            }
        }

        /// <summary>
        /// Loading Shop Tab
        /// </summary>
        /// <param name="FilesPath"></param>
        public static void LoadShopTab(string FilesPath)
        {
            string Files = KTGlobal.GetDataPath(FilesPath);
            using (var stream = System.IO.File.OpenRead(Files))
            {
                var serializer = new XmlSerializer(typeof(List<ShopTab>));
                _TotalShopTab = serializer.Deserialize(stream) as List<ShopTab>;
            }
        }

        /// <summary>
        /// Tải dữ liệu Kỳ Trân Các
        /// </summary>
        private static void LoadTokenShop()
        {
            string files = KTGlobal.GetDataPath(ShopManager.TokenShopFileDir);
            string content = System.IO.File.ReadAllText(files);
            XElement xmlNode = XElement.Parse(content);

            /// Tiệm Đồng
            foreach (XElement tokenShopNode in xmlNode.Element("Token").Elements("Shop"))
            {
                int shopID = int.Parse(tokenShopNode.Attribute("ID").Value);

                ShopTab shopTab = ShopManager._TotalShopTab.Where(x => x.ShopID == shopID).FirstOrDefault();

                if (shopTab == null)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Không tìm thấy thông tin Shop tương ứng, ID={0}", shopID));
                }
                else
                {
                    shopTab.Items = ShopManager.GetItemShopFromList(shopTab.ShopItem);
                    ShopManager.TokenShop.Token.Add(shopTab);
                }
            }

            /// Tiệm Đồng khóa
            foreach (XElement tokenShopNode in xmlNode.Element("BoundToken").Elements("Shop"))
            {
                int shopID = int.Parse(tokenShopNode.Attribute("ID").Value);

                ShopTab shopTab = ShopManager._TotalShopTab.Where(x => x.ShopID == shopID).FirstOrDefault();
                if (shopTab == null)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Không tìm thấy thông tin Shop tương ứng, ID={0}", shopID));
                }
                else
                {
                    shopTab.Items = ShopManager.GetItemShopFromList(shopTab.ShopItem);
                    ShopManager.TokenShop.BoundToken.Add(shopTab);
                }
            }

            /// Gói hàng bán trên Store
            foreach (XElement node in xmlNode.Element("StoreProduct").Elements("Product"))
            {
                TokenShopStoreProduct storeProduct = new TokenShopStoreProduct()
                {
                    ID = node.Attribute("ID").Value,
                    Name = node.Attribute("Name").Value,
                    Hint = node.Attribute("Hint").Value,
                    Icon = node.Attribute("Icon").Value,
                    Recommend = bool.Parse(node.Attribute("Recommend").Value),
                    Price = int.Parse(node.Attribute("Price").Value),
                    Token = int.Parse(node.Attribute("Token").Value),
                    FirstBonus = int.Parse(node.Attribute("FirstBonus").Value),
                };
                ShopManager.TokenShop.StoreProducts.Add(storeProduct);
            }
        }

        public static TokenShopStoreProduct GetProductByID(string ID)
        {
            TokenShopStoreProduct _Product = new TokenShopStoreProduct();

            var find = TokenShop.StoreProducts.Where(x => x.ID == ID).FirstOrDefault();

            if (find != null)
            {
                return find;
            }
            else
            {
                return null;
            }
        }

        public static List<ShopItem> GetItemShopFromList(List<int> InputData)
        {
            List<ShopItem> _Items = new List<ShopItem>();

            foreach (int ID in InputData)
            {
                var findItem = _TotalShopTtem.Where(x => x.ID == ID).FirstOrDefault();
                if (findItem != null)
                {
                    _Items.Add(findItem.Clone());
                }
            }

            return _Items;
        }

        /// <summary>
        /// Lấy ra shop bang hội
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        public static ShopTab GetGuildShop(KPlayer client)
        {
            // Tạo mới 1 shop table fake
            ShopTab _ShopTab = new ShopTab();
            // Danh sách vật phẩm thì set là rỗng
            _ShopTab.TotalSellItem = null;
            _ShopTab.ShopName = _ShopGuild.ShopName;
            _ShopTab.ShopID = _ShopGuild.ShopID;
            _ShopTab.TimeSaleStart = "";
            _ShopTab.MoneyType = 4;
            _ShopTab.Discount = 0;

            _ShopTab.Items = new List<ShopItem>();

            // List ra toàn bộ shop item
            foreach (GuildItem _Item in _ShopGuild.SellItems)
            {
                ShopItem _ShopItem = new ShopItem();
                _ShopItem.ItemID = _Item.ItemID;
                _ShopItem.Bind = _Item.Bind;
                _ShopItem.TongFund = _Item.Money;
                _ShopItem.LimitType = _Item.LimitType;
                _ShopItem.Expiry = _Item.Expiry;
                _ShopItem.ReputeDBID = -1;
                _ShopItem.OfficialLevel = -1;
                _ShopItem.ReputeLevel = -1;
                if (_ShopItem.LimitType != -1)
                {
                    _ShopItem.LimitValue = _Item.BuyLimitRank[client.GuildRank];
                }
                else
                {
                    _ShopItem.LimitValue = -1;
                }

                _ShopItem.GoodsIndex = -1;
                _ShopItem.GoodsPrice = -1;
                _ShopItem.Honor = -1;
                _ShopItem.ID = _Item.ID;
                _ShopItem.Series = -1;
                // Nếu như thằng này đéo có quyền mua thì bỏ qua vật phẩm này ko cho nó nhìn thấy
                if (_ShopItem.LimitType != -1 && _ShopItem.LimitValue <= 0)
                {
                    continue;
                }
                else
                {
                    _ShopTab.Items.Add(_ShopItem);
                }
            }

            // Trả về shoptab
            return _ShopTab;
        }

        /// <summary>
        ///
        /// Lấy ra 1 cái shop bang
        /// </summary>
        /// <param name="ShopID"></param>
        /// <param name="player"></param>
        /// <returns></returns>
        public static ShopTab GetShopTable(int ShopID, KPlayer client)
        {
            ShopTab _Shop = null;

            if (ShopID != _ShopGuild.ShopID)
            {
                _Shop = _TotalShopTab.Where(x => x.ShopID == ShopID).FirstOrDefault();

                if (_Shop != null)
                {
                    _Shop.Items = ShopManager.GetItemShopFromList(_Shop.ShopItem);

                    _Shop.TotalSellItem = client.GetGoodAreadySellBefore();
                }
            }
            else
            {
                _Shop = GetGuildShop(client);
            }
            return _Shop;
        }

        public static int GetGuildMoneyNeed(int OfficeRank)
        {
            if (OfficeRank == 1)
            {
                return 100000;
            }
            else if (OfficeRank == 2)
            {
                return 200000;
            }
            else if (OfficeRank == 3)
            {
                return 400000;
            }
            else if (OfficeRank == 4)
            {
                return 600000;
            }
            else if (OfficeRank == 5)
            {
                return 800000;
            }
            return -1;
        }

        //Hàm xử lý mua vật phẩm ở shop bagn hội
        public static SubRep BuyGuildShopItem(KPlayer client, int ShopItemID, int Number)
        {
            SubRep _SubRep = new SubRep();
            _SubRep.IsOK = false;
            _SubRep.IsBuyBack = false;
            _SubRep.CountLess = 0;

            var FindItem = _ShopGuild.SellItems.Where(x => x.ID == ShopItemID).FirstOrDefault();

            if (FindItem == null)
            {
                KTPlayerManager.ShowNotification(client, "Vật phẩm muốn mua không tồn tại");
                return _SubRep;
            }

            var FindTempate = ItemManager.GetItemTemplate(FindItem.ItemID);

            if (FindTempate == null)
            {
                KTPlayerManager.ShowNotification(client, "Vật phẩm muốn mua không tồn tại");
                return _SubRep;
            }

            int TotalMoneyNeed = 0;

            TotalMoneyNeed = FindItem.Money * Number;

            int TotalSpaceNeed = ItemManager.TotalSpaceNeed(FindTempate.ItemID, Number);

            // Check xem nếu không đủ chỗ trống để mua
            if (!KTGlobal.IsHaveSpace(TotalSpaceNeed, client))
            {
                KTPlayerManager.ShowNotification(client, "Túi đồ không đủ chỗ trống! Cần " + TotalSpaceNeed + " vị trí trống để có thể mua vật phẩm này!");

                return _SubRep;
            }

            int Before = client.RoleGuildMoney;
            // Check xem nó có đủ tiền không
            if (!KTGlobal.IsHaveMoney(client, TotalMoneyNeed, MoneyType.GuildMoney))
            {
                KTPlayerManager.ShowNotification(client, "Điểm bang cống của bạn không đủ để mua vật phẩm này!");

                return _SubRep;
            }

            // Chọn ra kiểu giới hạn
            LimitType _ItemLimit = (LimitType)FindItem.LimitType;

            // Nếu mà giới hạn theo ngày
            if (_ItemLimit == LimitType.BuyCountPerDay)
            {
                int Mark = FindItem.ItemID + 1000000;

                int BuyInDay = client.GetValueOfDailyRecore(Mark);

                int LimitByRank = FindItem.BuyLimitRank[client.GuildRank];

                // Nếu như không get được gì tức là nó chưa mua gì
                if (BuyInDay == -1)
                {
                    BuyInDay = 0;
                }
                if (BuyInDay + Number > LimitByRank)
                {
                    KTPlayerManager.ShowNotification(client, "Ngày hôm nay bạn đã mua hết số lượng cho phép");

                    return _SubRep;
                }
            }

            //TUẦN TODO
            if (_ItemLimit == LimitType.BuyCountPerWeek)
            {
                int Mark = FindItem.ItemID + 1000000;

                int TotalBuyInWeek = client.GetValueOfWeekRecore(Mark);

                int LimitByRank = FindItem.BuyLimitRank[client.GuildRank];

                if (TotalBuyInWeek == -1)
                {
                    TotalBuyInWeek = 0;
                }
                if (TotalBuyInWeek + Number > LimitByRank)
                {
                    KTPlayerManager.ShowNotification(client, "Tuần này bạn đã mua hết số lượt");

                    return _SubRep;
                }
            }

            //LIMIT CẢ ĐỜI
            if (_ItemLimit == LimitType.LimitCount)
            {
                int Mark = FindItem.ItemID + 1000000;

                int TotalAreadyBuy = client.GetValueOfForeverRecore(Mark);

                int LimitByRank = FindItem.BuyLimitRank[client.GuildRank];

                if (TotalAreadyBuy == -1)
                {
                    TotalAreadyBuy = 0;
                }
                if (TotalAreadyBuy + Number > LimitByRank)
                {
                    KTPlayerManager.ShowNotification(client, "Bạn đã mua hết giới hạn của vật phẩm này");

                    return _SubRep;
                }
            }

            // Set thời gian xử dụng
            string TimeUsing = ItemManager.ConstGoodsEndTime;
            // Nếu vật phẩm có hạn sử dụng
            if (FindItem.Expiry != -1)
            {
                DateTime dt = DateTime.Now.AddMinutes(FindItem.Expiry);

                // "1900-01-01 12:00:00";
                TimeUsing = dt.ToString("yyyy-MM-dd HH:mm:ss");
            }

            _SubRep = KTGlobal.SubMoney(client, TotalMoneyNeed, MoneyType.GuildMoney, "SHOPBUYITEM");
            // nếu trừ tiền thành công thực hiện ADD ITEM
            if (!_SubRep.IsOK)
            {
                KTPlayerManager.ShowNotification(client, "Bang cống không đủ");
                return _SubRep;
            }

            int After = client.RoleGuildMoney;
            // Add vật phẩm vào túi đồ cho nó
            if (!ItemManager.CreateItem(Global._TCPManager.TcpOutPacketPool, client, FindItem.ItemID, Number, 0, "SHOPGETITEM", true, FindItem.Bind, false, TimeUsing))
            {
                KTPlayerManager.ShowNotification(client, "Có lỗi khi thực hiện thêm vật phẩm vào túi đồ vui lòng liên hệ ADM để được giúp đỡ!");

                LogManager.WriteLog(LogTypes.BuyNpc, "Có lỗi khi thực hiện add vật phẩm :" + client.RoleID + "|" + FindItem.ItemID + " x " + Number);
            }
            else
            {
                KTPlayerManager.ShowNotification(client, "Mua vật phẩm thành công!");

                LogManager.WriteLog(LogTypes.ShopGuild, "[" + client.RoleID + "][" + client.RoleName + "] Buy Item [" + FindItem.ItemID + "] Number :[" + Number + "] | BEFORE :" +Before + "| AFTER :" + After);


                if (client.GuildID > 0)
                {
                    // Mua vật phẩm trong shop guild
                    GuildManager.getInstance().TaskProsecc(client.GuildID, client, FindItem.ItemID, TaskTypes.BuyItemInShopGuild);
                }
                // Đánh đấu vào là đã mua
                if (_ItemLimit == LimitType.BuyCountPerDay)
                {
                    int Mark = FindItem.ItemID + 1000000;

                    int BuyInDay = client.GetValueOfDailyRecore(Mark);
                    if (BuyInDay == -1)
                    {
                        BuyInDay = 0;
                    }

                    int TotalBuy = BuyInDay + Number;

                    client.SetValueOfDailyRecore(Mark, TotalBuy);
                }

                if (_ItemLimit == LimitType.BuyCountPerWeek)
                {
                    int Mark = FindItem.ItemID + 1000000;

                    int BuyInWeek = client.GetValueOfWeekRecore(Mark);

                    if (BuyInWeek == -1)
                    {
                        BuyInWeek = 0;
                    }

                    int TotalBuy = BuyInWeek + Number;

                    client.SetValueOfWeekRecore(Mark, TotalBuy);
                }

                if (_ItemLimit == LimitType.LimitCount)
                {
                    int Mark = FindItem.ItemID + 1000000;

                    int BuyForever = client.GetValueOfForeverRecore(Mark);

                    if (BuyForever == -1)
                    {
                        BuyForever = 0;
                    }

                    int TotalBuy = BuyForever + Number;

                    client.SetValueOfForeverRecore(Mark, TotalBuy);
                }
            }

            return _SubRep;
        }

        public static SubRep BuyBackItem(KPlayer player, int ShopItemID)
        {
            SubRep _SubRep = new SubRep();
            _SubRep.IsOK = false;
            _SubRep.IsBuyBack = false;
            _SubRep.CountLess = 0;

            var findOldItem = player.GetGoodAreadySellBefore().ToList().Where(x => x.Id == ShopItemID).FirstOrDefault();
            // nếu tìm thấy vật phẩm đã bán trước đó
            if (findOldItem != null)
            {
                if (!KTGlobal.IsHaveSpace(1, player))
                {
                    KTPlayerManager.ShowNotification(player, "Túi đồ không đủ chỗ trống! Cần " + 1 + " vị trí trống để có thể mua vật phẩm này!");

                    return _SubRep;
                }

                /// Giá mua lại sẽ = giá đã bán ra
                int price = ItemManager.GetItemTemplate(findOldItem.GoodsID).Price;

                /// Nếu đồ khóa thì mua lại sẽ cần bạc khóa, đồ không khóa mua lại sẽ cần bạc thường
                MoneyType Type = findOldItem.Binding == 1 ? MoneyType.BacKhoa : MoneyType.Bac;

                if (!KTGlobal.IsHaveMoney(player, price, Type))
                {
                    KTPlayerManager.ShowNotification(player, "Tiền yêu cầu trên người không đủ!");
                }
                else
                {
                    _SubRep = KTGlobal.SubMoney(player, price, Type, "BUYBACK");

                    _SubRep.IsBuyBack = true;
                    // nếu trừ tiền thành công thực hiện ADD ITEM
                    if (_SubRep.IsOK)
                    {
                        // Trả lại vật phẩm này cho nó
                        if (!ItemManager.CreateItem(Global._TCPManager.TcpOutPacketPool, player, findOldItem.GoodsID, 1, 0, "BUYBACK", true, findOldItem.Binding, false, ItemManager.ConstGoodsEndTime, findOldItem.Props, findOldItem.Series,findOldItem.Creator))
                        {
                            KTPlayerManager.ShowNotification(player, "Có lỗi khi mua lại vui lòng liên hệ với ADMIN để được giúp đỡ!");

                            LogManager.WriteLog(LogTypes.BuyNpc, "Có lỗi khi update lại số lượng :" + player.RoleID + "|" + findOldItem.Id + " x " + 1);
                        }
                        else
                        {
                            player.RemoveItemSell(findOldItem);
                            //TODO : Ghi lại nhật ký tích nạp tích tiêu
                            KTPlayerManager.ShowNotification(player, "Mua vật phẩm thành công!");
                        }
                    }
                    else
                    {
                        KTPlayerManager.ShowNotification(player, "Trừ tiền không thành công!");
                    }
                }
            }
            else
            {
                KTPlayerManager.ShowNotification(player, "Vật phẩm không còn tồn tại!");
            }

            return _SubRep;
        }

        public static SubRep BuyItemByAuto(KPlayer player, int ItemID, int Number, int Type)
        {
            SubRep _SubRep = new SubRep();
            _SubRep.IsOK = false;
            _SubRep.IsBuyBack = false;
            _SubRep.CountLess = 0;

            //Xem vật phẩm này có nằm trong shop ko
            ShopItem _item = _TotalShopTtem.Where(x => x.ItemID == ItemID).FirstOrDefault();

            if (_item == null)
            {
                KTPlayerManager.ShowNotification(player, "Không tìm thấy cửa hàng bán vật phẩm này");
                return _SubRep;
            }

            //Xem cái shop này sử dụng loiaj tiền nào
            MoneyType Money = (MoneyType)Type;

            // Tìm ảnh của vật phẩm
            var FindTempate = ItemManager.GetItemTemplate(_item.ItemID);

            if (FindTempate == null)
            {
                KTPlayerManager.ShowNotification(player, "Vật phẩm muốn mua không tồn tại");
                return _SubRep;
            }

            int TotalMoneyNeed = FindTempate.Price * Number;

            // Lấy ra tổng số ô đồ cần
            int TotalSpaceNeed = ItemManager.TotalSpaceNeed(FindTempate.ItemID, Number);

            // Check xem nếu không đủ chỗ trống để mua
            if (!KTGlobal.IsHaveSpace(TotalSpaceNeed, player))
            {
                KTPlayerManager.ShowNotification(player, "Túi đồ không đủ chỗ trống! Cần " + TotalSpaceNeed + " vị trí trống để có thể mua vật phẩm này!");

                return _SubRep;
            }

            if (FindTempate.Genre != 17)
            {
                KTPlayerManager.ShowNotification(player, "Vật phẩm không hợp lệ");

                return _SubRep;
            }

            if (_item.GoodsIndex == -1 && !KTGlobal.IsHaveMoney(player, TotalMoneyNeed, Money) && _item.GoodsIndex != -1)
            {
                KTPlayerManager.ShowNotification(player, "Tiền yêu cầu trên người không đủ!");
                return _SubRep;
            }

            _SubRep = KTGlobal.SubMoney(player, TotalMoneyNeed, Money, "SHOPBUYITEM");
            // nếu trừ tiền thành công thực hiện ADD ITEM
            if (!_SubRep.IsOK)
            {
                KTPlayerManager.ShowNotification(player, "Bạc mang theo không đủ");
                return _SubRep;
            }

            string TimeUsing = ItemManager.ConstGoodsEndTime;
            // Tọa vật phẩm đã mua
            if (!ItemManager.CreateItem(Global._TCPManager.TcpOutPacketPool, player, _item.ItemID, Number, 0, "SHOPGETITEM", true, 1, false, TimeUsing, "", _item.Series, "", 0, 1, false))
            {
                KTPlayerManager.ShowNotification(player, "Có lỗi khi thực hiện thêm vật phẩm vào túi đồ vui lòng liên hệ ADM để được giúp đỡ!");

                LogManager.WriteLog(LogTypes.BuyNpc, "Có lỗi khi thực hiện add vật phẩm :" + player.RoleID + "|" + _item.ItemID + " x " + Number);
            }
            return _SubRep;
        }

        /// <summary>
        /// Xử lý sự kiện mua của người chơi
        /// </summary>
        /// <param name="player"></param>
        /// <param name="ShopID"></param>
        /// <param name="ShopItemID"></param>
        /// <param name="Number"></param>
        /// <param name="counponID"></param>
        /// <returns></returns>
        public static SubRep BuyItem(KPlayer player, int ShopID, int ShopItemID, int Number, int couponID)
        {
            SubRep _SubRep = new SubRep();
            _SubRep.IsOK = false;
            _SubRep.IsBuyBack = false;
            _SubRep.CountLess = 0;

            try
            {
                // Nếu như cần mật khẩu cấp 2
                if (player.NeedToShowInputSecondPassword())
                {
                    KTPlayerManager.ShowNotification(player, "Hãy mở khóa an toàn trước khi thao tác");
                    return _SubRep;
                }

                if (ShopID == 8888)
                {
                    return BuyItemByAuto(player, ShopItemID, Number, couponID);
                }
                // Nếu là cửa hàng bang hội
                if (ShopID == _ShopGuild.ShopID)
                {
                    return BuyGuildShopItem(player, ShopItemID, Number);
                }

                // Nếu là mua lại vật phẩm gì đó
                if (ShopID == 9999)
                {
                    return BuyBackItem(player, ShopItemID);
                }
                // TÌm ra cửa hàng này xem cos tồn tại hay không
                ShopTab ShopFind = _TotalShopTab.Where(x => x.ShopID == ShopID).FirstOrDefault();

                if (ShopFind == null)
                {
                    KTPlayerManager.ShowNotification(player, "Vật phẩm muốn mua không tồn tại");
                    return _SubRep;
                }
                // Xem có cần ghi logs hay không
                bool NeedWriterLogs = false;

                // Kiểm tra xem vật phẩm muốn mua có tồn tại hay không
                if (!ShopFind.ShopItem.Contains(ShopItemID))
                {
                    KTPlayerManager.ShowNotification(player, "Vật phẩm muốn mua không tồn tại");
                    return _SubRep;
                }

                // Tìm xem thấy vật phẩm đấy trong cái shoptab đó không
                ShopItem _item = _TotalShopTtem.Where(x => x.ID == ShopItemID).FirstOrDefault();

                if (_item == null)
                {
                    KTPlayerManager.ShowNotification(player, "Không tìm thấy cửa hàng tương ứng");
                    return _SubRep;
                }

                //Xem cái shop này sử dụng loiaj tiền nào
                MoneyType Money = (MoneyType)ShopFind.MoneyType;

                // Tìm ảnh của vật phẩm
                var FindTempate = ItemManager.GetItemTemplate(_item.ItemID);

                if (FindTempate == null)
                {
                    KTPlayerManager.ShowNotification(player, "Vật phẩm muốn mua không tồn tại");
                    return _SubRep;
                }

                // Xem được giảm giá hay không
                int RATE = ShopFind.Discount;

                int TotalMoneyNeed = 0;

                int ItemDiscount = 0;

                bool ItUsingTicket = false;

                //Nếu là shop đồng hoặc đông khóa
                if (Money == MoneyType.Dong || Money == MoneyType.DongKhoa)
                {
                    if (couponID != -1)
                    {
                        // tìm xem trên người thằng này có phiếu giảm giá hay không
                        GoodsData FindGoldById = player.GoodsData.Find(couponID, 0);

                        if (FindGoldById != null)
                        {
                            ItemData TMP = ItemManager.GetItemTemplate(FindGoldById.GoodsID);
                            if (TMP != null)
                            {
                                // Nếu có sử dụng phiếu giảm giá
                                if (TMP.Genre == 18 && TMP.DetailType == 1 && TMP.ParticularType >= 1550 && TMP.ParticularType <= 1554)
                                {
                                    ItUsingTicket = true;
                                    ItemDiscount = TMP.ItemValue;
                                }
                            }
                        }
                    }

                    // Chỉ ghi logs với shop đồng hoặc đồng khóa
                    NeedWriterLogs = true;

                    TotalMoneyNeed = _item.GoodsPrice * Number;
                }
                else
                {
                    TotalMoneyNeed = FindTempate.Price * Number;
                }

                // Nếu là có giảm giá
                if (RATE > 0)
                {
                    int Discount = (RATE * TotalMoneyNeed) / 100;
                    TotalMoneyNeed = TotalMoneyNeed - Discount;
                }

                // Nếu có sử dụng phiếu giảm giá
                if (ItemDiscount > 0)
                {
                    int Discount = (ItemDiscount * TotalMoneyNeed) / 100;

                    TotalMoneyNeed = TotalMoneyNeed - Discount;
                }

                // Lấy ra tổng số ô đồ cần
                int TotalSpaceNeed = ItemManager.TotalSpaceNeed(FindTempate.ItemID, Number);

                // Check xem nếu không đủ chỗ trống để mua
                if (!KTGlobal.IsHaveSpace(TotalSpaceNeed, player))
                {
                    KTPlayerManager.ShowNotification(player, "Túi đồ không đủ chỗ trống! Cần " + TotalSpaceNeed + " vị trí trống để có thể mua vật phẩm này!");

                    return _SubRep;
                }

                int TempCount = 0;

                if (_item.GoodsIndex == -1 && !KTGlobal.IsHaveMoney(player, TotalMoneyNeed, Money) && _item.GoodsIndex != -1)
                {
                    KTPlayerManager.ShowNotification(player, "Tiền yêu cầu trên người không đủ!");
                    return _SubRep;
                }
                else
                {
                    // Nếu yêu cầu danh vọng
                    if (_item.ReputeDBID != -1)
                    {
                        ReputeInfo _Info = player.GetRepute().Where(x => x.DBID == _item.ReputeDBID).FirstOrDefault();
                        if (_Info.Level < _item.ReputeLevel)
                        {
                            KTPlayerManager.ShowNotification(player, "Danh vọng không đủ");
                            return _SubRep;
                        }
                    }
                    // Nếu yêu cầu danh vọng lãnh thổ
                    if (_item.OfficialLevel != -1)
                    {
                        if (player.OfficeRank < _item.OfficialLevel)
                        {
                            KTPlayerManager.ShowNotification(player, "Danh vọng lãnh thổ không đủ không thể mua!");
                            return _SubRep;
                        }

                        int GuildMoneyNeed = GetGuildMoneyNeed(_item.OfficialLevel);

                        if (!KTGlobal.IsHaveMoney(player, GuildMoneyNeed, MoneyType.GuildMoney))
                        {
                            KTPlayerManager.ShowNotification(player, "Tài sản cá nhân bang hội không đủ!");
                            return _SubRep;
                        }
                    }

                    // Nếu yêu cầu vinh dự tài phú để mua
                    if (_item.Honor != -1)
                    {
                        int Rank = KTGlobal.GetRankHonor(player.GetTotalValue());
                        if (Rank < _item.Honor)
                        {
                            KTPlayerManager.ShowNotification(player, "Vinh dự tài phú không đủ");
                            return _SubRep;
                        }
                    }

                    //YÊU CẦU TIỀN CỐNG HIẾN BANG HỘI
                    if (_item.TongFund != -1)
                    {
                        int Request = _item.TongFund * Number;

                        if (player.RoleGuildMoney < Request)
                        {
                            KTPlayerManager.ShowNotification(player, "Tiền bang hội không đủ");
                        }
                    }

                    // Nếu yêu cầu vật phẩm nào đó để có thể mua
                    if (_item.GoodsIndex != -1)
                    {
                        int NumberRequest = _item.GoodsPrice * Number;

                        int CountInBag = ItemManager.GetItemCountInBag(player, _item.GoodsIndex);

                        if (CountInBag < NumberRequest)
                        {
                            KTPlayerManager.ShowNotification(player, "Đạo cụ mua không đủ");
                            return _SubRep;
                        }
                    }

                    LimitType _ItemLimit = (LimitType)_item.LimitType;

                    /// Limut theo ngày
                    if (_ItemLimit == LimitType.BuyCountPerDay)
                    {
                        int Mark = _item.ItemID + 2000000;

                        int BuyInDay = player.GetValueOfDailyRecore(Mark);

                        int LimitByRank = _item.LimitValue;

                        // Nếu như không get được gì tức là nó chưa mua gì
                        if (BuyInDay == -1)
                        {
                            BuyInDay = 0;
                        }
                        if (BuyInDay + Number > LimitByRank)
                        {
                            KTPlayerManager.ShowNotification(player, "Ngày hôm nay bạn đã mua hết số lượng cho phép");

                            return _SubRep;
                        }
                    }

                    //TUẦN TODO
                    if (_ItemLimit == LimitType.BuyCountPerWeek)
                    {
                        int Mark = _item.ItemID + 2000000;

                        int TotalBuyInWeek = player.GetValueOfWeekRecore(Mark);

                        int LimitByRank = _item.LimitValue;

                        if (TotalBuyInWeek == -1)
                        {
                            TotalBuyInWeek = 0;
                        }
                        if (TotalBuyInWeek + Number > LimitByRank)
                        {
                            KTPlayerManager.ShowNotification(player, "Tuần này bạn đã mua hết số lượt");

                            return _SubRep;
                        }
                    }

                    //LIMIT CẢ ĐỜI
                    if (_ItemLimit == LimitType.LimitCount)
                    {
                        int Mark = _item.ItemID + 2000000;

                        int TotalAreadyBuy = player.GetValueOfForeverRecore(Mark);

                        int LimitByRank = _item.LimitValue;

                        if (TotalAreadyBuy == -1)
                        {
                            TotalAreadyBuy = 0;
                        }
                        if (TotalAreadyBuy + Number > LimitByRank)
                        {
                            KTPlayerManager.ShowNotification(player, "Bạn đã mua hết giới hạn của vật phẩm này");

                            return _SubRep;
                        }
                    }

                    if (_item.OfficialLevel != -1)
                    {
                        int GuildMoneyNeed = GetGuildMoneyNeed(_item.OfficialLevel);

                        if (!KTGlobal.SubMoney(player, GuildMoneyNeed, MoneyType.GuildMoney, "SHOPBUYITEM").IsOK)
                        {
                            KTPlayerManager.ShowNotification(player, "Tài sản cá nhân bang hội không đủ!");

                            return _SubRep;
                        }
                    }

                    if (ItUsingTicket && _item.ItemID == 10460)
                    {
                        KTPlayerManager.ShowNotification(player, "Phiếu Kim Ngân không thể sử dụng cùng với phiếu giảm giá!");

                        return _SubRep;
                    }

                    // Nếu vật phẩm yêu cầu vật phẩm thì thực hiện trừ vậ tphaamr
                    if (_item.GoodsIndex != -1)
                    {
                        int NumberRequest = _item.GoodsPrice * Number;

                        if (!ItemManager.RemoveItemFromBag(player, _item.GoodsIndex, NumberRequest, -1, "BUYSHOPITEM"))
                        {
                            KTPlayerManager.ShowNotification(player, "Vật phẩm đạo cụ không đủ");

                            return _SubRep;
                        }
                    }
                    else if (_item.TongFund != -1) // nếu vật phẩm yêu cầu tiền bang hội thì thực hiện trừ tiền bang hội
                    {
                        int Request = _item.TongFund * Number;

                        _SubRep = KTGlobal.SubMoney(player, Request, MoneyType.GuildMoney, "SHOPBUYITEM");
                        // nếu trừ tiền thành công thực hiện ADD ITEM
                        if (!_SubRep.IsOK)
                        {
                            KTPlayerManager.ShowNotification(player, "Tích lũy cá nhân không thể sử dụng thêm");
                            return _SubRep;
                        }
                    }
                    else // Nếu vật phẩm yêu cầu tiền mặt thì sử dụng tiền mặt để mua
                    {
                        bool IsSaveCosume = true;

                        if (_item.ItemID == 10460)
                        {
                            IsSaveCosume = false;
                        }
                        _SubRep = KTGlobal.SubMoney(player, TotalMoneyNeed, Money, "SHOPBUYITEM", IsSaveCosume);
                        // nếu trừ tiền thành công thực hiện ADD ITEM
                        if (!_SubRep.IsOK)
                        {
                            KTPlayerManager.ShowNotification(player, "Tiền mang theo không đủ");
                            return _SubRep;
                        }
                        if (ItUsingTicket)
                        {
                            GoodsData FindGoldById = player.GoodsData.Find(couponID, 0);

                            ItemManager.RemoveItemByCount(player, FindGoldById, 1, "SHOPTICKETDISCOUNT");

                            LogManager.WriteLog(LogTypes.BuyNpc, "[" + player.RoleID + "][" + player.RoleName + "] Using Discount :" + FindGoldById.GoodsID);
                        }
                    }

                    TCPOutPacketPool pool = Global._TCPManager.TcpOutPacketPool;

                    int BILLING = 1;

                    if (Money == MoneyType.Dong)
                    {
                        BILLING = 0;
                    }

                    string TimeUsing = ItemManager.ConstGoodsEndTime;

                    if (_item.Expiry != -1)
                    {
                        DateTime dt = DateTime.Now.AddMinutes(_item.Expiry);

                        // "1900-01-01 12:00:00";
                        TimeUsing = dt.ToString("yyyy-MM-dd HH:mm:ss");
                    }

                    // Tọa vật phẩm đã mua
                    if (!ItemManager.CreateItem(Global._TCPManager.TcpOutPacketPool, player, _item.ItemID, Number, 0, "SHOPGETITEM", true, BILLING, false, TimeUsing, "", _item.Series, "", 0, 1, NeedWriterLogs))
                    {
                        KTPlayerManager.ShowNotification(player, "Có lỗi khi thực hiện thêm vật phẩm vào túi đồ vui lòng liên hệ ADM để được giúp đỡ!");

                        LogManager.WriteLog(LogTypes.BuyNpc, "Có lỗi khi thực hiện add vật phẩm :" + player.RoleID + "|" + _item.ItemID + " x " + Number);
                    }
                    else
                    {
                        if (Money == MoneyType.Dong)
                        {
                            LogManager.WriteLog(LogTypes.BuyNpc, "[SHOPKNB][" + player.RoleID + "][" + player.RoleName + "]Mua vật phẩm thành công :" + _item.ItemID + " x " + Number + "==> tiêu tốn " + TotalMoneyNeed);
                        }

                        //TODO : Ghi lại nhật ký tích nạp tích tiêu
                        KTPlayerManager.ShowNotification(player, "Mua vật phẩm thành công!");

                        // Đánh dấu vào DB đã mua vật phẩm này
                        if (_ItemLimit == LimitType.BuyCountPerDay)
                        {
                            int Mark = _item.ItemID + 2000000;

                            int BuyInDay = player.GetValueOfDailyRecore(Mark);
                            if (BuyInDay == -1)
                            {
                                BuyInDay = 0;
                            }

                            int TotalBuy = BuyInDay + Number;

                            player.SetValueOfDailyRecore(Mark, TotalBuy);
                        }

                        if (_ItemLimit == LimitType.BuyCountPerWeek)
                        {
                            int Mark = _item.ItemID + 2000000;

                            int BuyInWeek = player.GetValueOfWeekRecore(Mark);

                            if (BuyInWeek == -1)
                            {
                                BuyInWeek = 0;
                            }

                            int TotalBuy = BuyInWeek + Number;

                            player.SetValueOfWeekRecore(Mark, TotalBuy);
                        }

                        if (_ItemLimit == LimitType.LimitCount)
                        {
                            int Mark = _item.ItemID + 2000000;

                            int BuyForever = player.GetValueOfForeverRecore(Mark);

                            if (BuyForever == -1)
                            {
                                BuyForever = 0;
                            }

                            int TotalBuy = BuyForever + Number;

                            player.SetValueOfForeverRecore(Mark, TotalBuy);
                        }
                    }
                }
            }
            catch (Exception exx)
            {
                Console.WriteLine(exx.ToString());
                KTPlayerManager.ShowNotification(player, "Có lỗi khi mua vật phẩm này");
                return _SubRep;
            }

            return _SubRep;
        }
    }
}