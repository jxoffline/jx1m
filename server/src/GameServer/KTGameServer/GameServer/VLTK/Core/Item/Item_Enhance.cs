using GameServer.KiemThe.Entities;
using GameServer.KiemThe.Logic;
using GameServer.KiemThe.Logic.Manager.Shop;
using GameServer.Logic;
using GameServer.VLTK.Core.GuildManager;
using Server.Data;
using Server.Tools;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GameServer.KiemThe.Core.Item
{
    /// <summary>
    /// Classs cường hóa vật phẩm
    /// </summary>
    public class ItemEnhance
    {
        public static int MIN_COMMON_EQUIP = 0;
        public static int MAX_COMMON_EQUIP = 11;

        public static double PEEL_RESTORE_RATE_12 = 1.0 * 3 / 100;
        public static double PEEL_RESTORE_RATE_14 = 1.0 * 5 / 100;
        public static double ENHANCE_COST_RATE = 1.0 * 10 / 100;

        public static int MAX_SPLITNUM = 8;

        public static double DUR_COST_PER_YEAR = (2 / 5) * 3600 * 6 * 365 / 20;
        public static double EQUIP_TOTAL_RATE = 4 * 1 + 1.5 * 4 + 1 * 5;
        public static double ALL_EQUIP_MIN_VALUE = 5000000;
        public static double ALL_EQUIP_MAX_VALUE = 500000000;
        public static double VALUEPERCEN_PER_YEAR = 0.3;

        #region CUONGHOA

        /// <summary>
        /// Tính toán cấp cường hóa tối đa của trang bị
        /// </summary>
        /// <param name="itemData"></param>
        /// <returns></returns>
        public static int CalcMaxEnhanceTimes(ItemData itemData)
        {
            if (itemData.Genre == 7)
            {
                return 10;
            }

            if (itemData.ListEnhance != null)
            {
                int Max = itemData.ListEnhance.Max(x => x.EnhTimes);

                return Max;
            }
            else
            {
                return 0;
            }
            //Trả về số lần cường hóa to nhất của vật phẩm này

            //retru
            //int nMax;
            //int nLevel = itemData.Level;

            //if (nLevel <= 3)
            //{
            //    nMax = 4;
            //}
            //else if (nLevel <= 6)
            //{
            //    nMax = 8;
            //}
            //else if (nLevel <= 8)
            //{
            //    nMax = 12;
            //}
            //else if (nLevel <= 9)
            //{
            //    nMax = 14;
            //}
            //else
            //{
            //    /// Nếu là trang bị luyện hóa
            //    if (ItemRefine.IsRefinedEquip(itemData) || itemData.IsArtifact)
            //    {
            //        nMax = 16;
            //    }
            //    else
            //    {
            //        nMax = 14;
            //    }
            //}

            //return nMax;
        }

        public static bool IsHuyenTinh(ItemData _Input)
        {
            if (_Input.Genre == 18 && _Input.DetailType == 1 && (_Input.ParticularType == 1 || _Input.ParticularType == 114))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Hàm sử dụng để đổi ngũ hành hồn thạch từ vật phẩm sang
        /// </summary>
        /// <param name="player"></param>
        /// <param name="equip"></param>
        /// <returns></returns>
        public static bool ExchangeFS(KPlayer player, GoodsData equip)
        {
            /// Nếu vật phẩm không tồn tại
            if (!ItemManager._TotalGameItem.TryGetValue(equip.GoodsID, out ItemData itemData))
            {
                KTPlayerManager.ShowNotification(player, "Trang bị không tồn tại!");
                return false;
            }

            /// Số lượng ngũ hành hồn thạch nhận lại
            int fsCount;

            /// Loại trang bị
            KE_ITEM_EQUIP_DETAILTYPE itemType = (KE_ITEM_EQUIP_DETAILTYPE)itemData.DetailType;
            /// Nếu là phi phong
            if (itemType == KE_ITEM_EQUIP_DETAILTYPE.equip_mantle)
            {
                /// Hạn dùng Phi Phong
                int maxDays = 30;
                /// Nếu có hạn sử dụng
                if (!string.IsNullOrEmpty(equip.Endtime))
                {
                    /// Thời gian hết hạn
                    DateTime endTime = DateTime.Parse(equip.Endtime);
                    if (endTime > DateTime.Now)
                    {
                        TimeSpan diff = endTime - DateTime.Now;
                        int diffSec = (int)diff.TotalSeconds;

                        /// Toạch
                        if (diffSec == -1)
                        {
                            fsCount = 0;
                        }
                        /// Nếu còn dưới 1 ngày thì toạch
                        else if (diffSec < 86400)
                        {
                            fsCount = 0;
                        }
                        else
                        {
                            /// Số ngày còn lại
                            int daysLeft = diffSec / 86400;

                            /// Số Ngũ Hành Hồn Thạch đổi được = số ngày còn lại
                            fsCount = daysLeft * itemData.Price / 1000 / maxDays;
                        }
                    }
                    else
                    {
                        /// Toạch
                        fsCount = 0;
                    }
                }
                /// Nếu không có hạn sử dụng thì trả ra giá trị Max
                else
                {
                    fsCount = itemData.Price / 1000;
                }
            }
            else
            {
                /// Nếu không phải đồ chế
                if (string.IsNullOrEmpty(equip.Creator))
                {
                    KTPlayerManager.ShowNotification(player, "Chỉ có trang bị chế mới có thể tách thành Ngũ Hành Hồn Thạch!");
                    return false;
                }

                /// Nếu đã khóa
                if (equip.Binding == 1)
                {
                    KTPlayerManager.ShowNotification(player, "Chỉ trang bị chế không khóa mới có thể tách thành Ngũ Hành Hồn Thạch!");
                    return false;
                }

                /// Nếu trang bị có cường hóa
                if (equip.Forge_level > 0)
                {
                    KTPlayerManager.ShowNotification(player, "Trang bị đã cường hóa không thể tách thành Ngũ Hành Hồn Thạch!");
                    return false;
                }

                /// Nếu dưới cấp 7 thì không cho tách
                if (itemData.Level < 7)
                {
                    KTPlayerManager.ShowNotification(player, "Trang bị này không thể tách thành Ngũ Hành Hồn Thạch!");
                    return false;
                }

                /// Số lượng ngũ hành hồn thạch nhận lại
                fsCount = itemData.Price / 1000;
            }

            /// Nếu không có ngũ hành hồn thạch có thể đổi
            if (fsCount <= 0)
            {
                KTPlayerManager.ShowNotification(player, "Trang bị này không thể tách thành Ngũ Hành Hồn Thạch!");
                return false;
            }

            /// Vật phẩm Ngũ Hành Hồn Thạch
            ItemData fsItemData = ItemManager.GetItemTemplate(506);
            /// Khoảng trống cần
            int spaceNeed = KTGlobal.GetTotalSpacesNeedToTakeItem(fsItemData.ItemID, fsCount);

            /// Nếu không đủ chỗ trống
            if (!KTGlobal.IsHaveSpace(spaceNeed, player))
            {
                KTPlayerManager.ShowNotification(player, "Túi đồ không đủ chỗ trống, không thể tách trang bị!");
                return false;
            }

            /// Xóa trang bị
            if (!ItemManager.AbandonItem(equip, player, false, "Đổi ngũ hành hồn thạch"))
            {
                KTPlayerManager.ShowNotification(player, "Không thể xóa trang bị!");
                return false;
            }

            /// Thêm vật phẩm
            if (!ItemManager.CreateItem(Global._TCPManager.TcpOutPacketPool, player, fsItemData.ItemID, fsCount, 0, "ExchangeFS", true, equip.Binding, false, ItemManager.ConstGoodsEndTime))
            {
                KTPlayerManager.ShowNotification(player, "Có lỗi khi nhận Ngũ Hành Hồn Thạch!");
                return false;
            }

            /// Thao tác thành công
            return true;
        }

        /// <summary>
        /// Cường hóa ngũ hành ấn
        /// </summary>
        /// <param name="TotalFS"></param>
        /// <param name="ItemInput"></param>
        /// <param name="Player"></param>
        /// <returns></returns>
        public static int DoEnhSignet(List<GoodsData> TotalFS, GoodsData ItemInput, KPlayer client, int SelectRounter)
        {
            // Nếu ko tìm thấy bản thể của ngũ hành ấn thì return toạch
            ItemData _SignetInput = ItemManager.GetItemTemplate(ItemInput.GoodsID);

            // Dữ liệu không hợp lệ
            if (_SignetInput == null)
            {
                KTPlayerManager.ShowNotification(client, "Thông tin Ngũ Hành Ấn không tồn tại, hãy thử đóng khung và thao tác lại!");
                return -1;
            }

            // Nếu chọn sai ROUNTER thì cũng chim cút
            if (SelectRounter < 0 || SelectRounter > 1)
            {
                KTPlayerManager.ShowNotification(client, "Hãy chọn loại cường hóa!");
                return -200;
            }

            // Nếu vật phẩm đút vào ko phỉa ngũ hành ấn
            if (!ItemManager.KD_ISSIGNET(_SignetInput.DetailType))
            {
                KTPlayerManager.ShowNotification(client, "Hãy đặt vào Ngũ Hành Ấn!");
                return -100;
            }

            double nExp = 0;
            // Exp hiện tại
            int nLevel = 0;
            //Tổng số ngũ hành hồn thạch bỏ vào
            int TotalBF = 0;

            // lấy ra toàn bộ số ngũ hành hồn thạch
            foreach (GoodsData _BF in TotalFS)
            {
                // Nếu vật phẩm là ngũ hành hồn thạch
                if (_BF.GoodsID == 506)
                {
                    TotalBF += _BF.GCount;
                }
            }

            if (TotalBF == 0)
            {
                KTPlayerManager.ShowNotification(client, "Chưa đặt vào Ngũ Hành Hồn Thạch!");
                return -3;
            }

            int CountItemInBag = ItemManager.GetItemCountInBag(client, 506);

            // Nếu số lượng ngũ hành hồn thạch trong người ít hơn số bỏ vào thì có biến
            if (CountItemInBag < TotalBF)
            {
                KTPlayerManager.ShowNotification(client, "Không đủ Ngũ Hành Hồn Thạch!");
                return -5;
            }

            
            if (ItemInput.OtherParams == null)
            {
                KTPlayerManager.ShowNotification(client, "Thông tin Ngũ Hành Ấn bị lỗi, hãy liên hệ với hỗ trợ để được trợ giúp!");
                return -4;
            }

            // Nếu là cường hóa ngũ hành tương khắc
            if (SelectRounter == 0)
            {
                if (ItemInput.OtherParams.TryGetValue(ItemPramenter.Pram_1, out string enhance))
                {
                    string[] enhancePram = enhance.Split('|');

                    nLevel = Int32.Parse(enhancePram[0]);
                    nExp = Int32.Parse(enhancePram[1]);
                }
            } // Nếu là nhược hóa ngũ hành ấn
            else if (SelectRounter == 1)
            {
                if (ItemInput.OtherParams.TryGetValue(ItemPramenter.Pram_2, out string sabate))
                {
                    string[] sabatePram = sabate.Split('|');

                    nLevel = Int32.Parse(sabatePram[0]);
                    nExp = Int32.Parse(sabatePram[1]);
                }
            }

            // Return đã max level cường hóa
            if (nLevel >= 1000)
            {
                KTPlayerManager.ShowNotification(client, "Ngũ Hành Ấn đã đạt cấp tối đa!");
                return -2;
            }

            double nMuti = 100;

            nExp = nExp + Math.Floor(TotalBF * 10 * nMuti / 100);

            int MaxLevel = 1000;

            var ExpFind = ItemManager._TotalSingNetExp.Where(x => x.Level == nLevel).FirstOrDefault();

            int ExpFistFind = ExpFind.UpgardeExp;

            while (nLevel < MaxLevel && nExp >= ExpFistFind)
            {
                //Lấy ra lượng EXP cần để lên cấp
                ExpFistFind = ItemManager._TotalSingNetExp.Where(x => x.Level == nLevel).FirstOrDefault().UpgardeExp;

                nExp = nExp - ExpFistFind;
                nLevel = nLevel + 1;
            }

            double nResCount = 0;

            if (nLevel >= MaxLevel && nExp > 0)
            {
                nResCount = Math.Floor(nExp / (10 * nMuti / 100));

                nExp = 0;
            }

            int ItemWillBeRemove = TotalBF - (int)nResCount;

            // Báo lỗi nếu không xóa được vật phẩm
            if (!ItemManager.RemoveItemFromBag(client, 506, ItemWillBeRemove, -1, "Cường hóa ấn"))
            {
                KTPlayerManager.ShowNotification(client, "Xóa vật phẩm thất bại!");
                return -20;
            }

            int COUNTAGINA = ItemManager.GetItemCountInBag(client, 506);

            // Khởi tạo 1 cái DICT UPDATE VÀO DB
            Dictionary<UPDATEITEM, object> TotalUpdate = new Dictionary<UPDATEITEM, object>();

            TotalUpdate.Add(UPDATEITEM.ROLEID, client.RoleID);
            TotalUpdate.Add(UPDATEITEM.ITEMDBID, ItemInput.Id);

            // SET CÁC KIỂU VÀO DB
            if (SelectRounter == 0)
            {
                Dictionary<ItemPramenter, string> OtherParams = new Dictionary<ItemPramenter, string>();

                if (ItemInput.OtherParams.TryGetValue(ItemPramenter.Pram_2, out string OldValue))
                {
                    string NewValue = nLevel + "|" + nExp;

                    LogManager.WriteLog(LogTypes.NHHT, "[" + client.RoleID + "][" + client.RoleName + "][" + ItemInput.Id + "][" + ItemInput.GoodsID + "] cường hóa ngũ hành tương khắc gia tăng : " + NewValue + " | Tiêu tốn :" + ItemWillBeRemove);

                    OtherParams.Add(ItemPramenter.Pram_1, NewValue + "");
                    OtherParams.Add(ItemPramenter.Pram_2, OldValue + "");

                    TotalUpdate.Add(UPDATEITEM.OTHER_PRAM, OtherParams);
                }
            }
            else if (SelectRounter == 1)
            {
                Dictionary<ItemPramenter, string> OtherParams = new Dictionary<ItemPramenter, string>();
                if (ItemInput.OtherParams.TryGetValue(ItemPramenter.Pram_1, out string OldValue))
                {
                    string NewValue = nLevel + "|" + nExp;

                    LogManager.WriteLog(LogTypes.NHHT, "[" + client.RoleID + "][" + client.RoleName + "][" + ItemInput.Id + "][" + ItemInput.GoodsID + "] nhược khóa ngũ hành tương khắc gia tăng : " + NewValue + " | Tiêu tốn :" + ItemWillBeRemove);

                    OtherParams.Add(ItemPramenter.Pram_1, OldValue + "");
                    OtherParams.Add(ItemPramenter.Pram_2, NewValue + "");

                    TotalUpdate.Add(UPDATEITEM.OTHER_PRAM, OtherParams);
                }
            }

            /// Thực hiện update vào DB
            if (!client.GoodsData.Update(ItemInput, TotalUpdate, true, true, "EnchanceSigNet"))
            {
                KTPlayerManager.ShowNotification(client, "Có lỗi khi cường hóa ngũ hành ấn");
            }

            // Trả về luyện hóa thành công
            return 1;
        }

        // Hàm caclulation nếu  muốn đẩy dữ liệu về client đỡ phỉa load EXPFILES
        public static void Caclulation(int TotalBF, int nLevel, int nExp)
        {
            int nMuti = 100;

            nExp = nExp + TotalBF * 10 * nMuti / 100;

            int MaxLevel = ItemManager._TotalSingNetExp.Max(x => x.Level);

            var ExpFind = ItemManager._TotalSingNetExp.Where(x => x.Level == nLevel).FirstOrDefault();

            int ExpFistFind = ExpFind.UpgardeExp;

            while (nLevel < MaxLevel && nExp >= ExpFistFind)
            {
                ExpFistFind = ItemManager._TotalSingNetExp.Where(x => x.Level == nLevel).FirstOrDefault().UpgardeExp;
                nExp = nExp - ExpFistFind;
                nLevel = nLevel + 1;
            }
            int nResCount = 0;
            if (nLevel >= MaxLevel && nExp > 0)
            {
                nResCount = nExp / (10 * nMuti / 100);
            }

            //Console.WriteLine(nLevel);
            //Console.WriteLine(nExp);
            //Console.WriteLine(nLevel);

            //Số lượng  ngũ hành ấn thừa sẽ trả lại
            // Console.WriteLine(nResCount);
        }

        /// <summary>
        /// Thực hiện cường hóa
        /// </summary>
        /// <param name="TotalHuyenTinh"></param>
        /// <param name="Item"></param>
        /// <param name="_Player"></param>
        /// <param name="MoneyType"></param>
        public static int DoEnhItem(List<GoodsData> TotalHuyenTinh, GoodsData Item, KPlayer _Player, MoneyType Type)
        {
            // Check xem có đủ điều kiện cường hóa không
            if (!CheckEnhItem(Item, _Player))
            {
                return -1;
            }

            CalcProb _Caclution = GetEnhItem(TotalHuyenTinh, Item);
            if (_Caclution.nProb > 0)
            {
                // Nếu tỉ lệ quá thấp
                if (_Caclution.nTrueProb < 10)
                {
                    KTPlayerManager.ShowNotification(_Player, "Xác suất thành công quá thấp, không thể cường hóa");
                    return -1;
                }

                // Nếu tỉ lệ quá cao
                if (_Caclution.nTrueProb > 120)
                {
                    KTPlayerManager.ShowNotification(_Player, "Huyền Tinh đặt vào quá nhiều, xin đừng lãng phí");
                    return -1;
                }

                int MoneyNeed = (int)_Caclution.nMoney;

                if (!KTGlobal.IsHaveMoney(_Player, MoneyNeed, Type))
                {
                    KTPlayerManager.ShowNotification(_Player, "Bạc mang trên người không đủ");
                    return -1;
                }

                // Nếu ok hết thực hiện cường hóa
                SubRep _Submoney = KTGlobal.SubMoney(_Player, MoneyNeed, Type, "EnhItem");
                if (!_Submoney.IsOK)
                {
                    KTPlayerManager.ShowNotification(_Player, "Có lỗi khi trừ tiền");
                    return -1;
                }

                int LOCK = 0;

                if (Item.Binding == 1)
                {
                    LOCK = 1;
                }

                if (Type == MoneyType.BacKhoa)
                {
                    LOCK = 1;
                }

                var findlock = TotalHuyenTinh.Where(x => x.Binding == 1).FirstOrDefault();
                if (findlock != null)
                {
                    LOCK = 1;
                }
                // Thực hiện xóa huyền tinh
                foreach (GoodsData _HuyenTinh in TotalHuyenTinh)
                {
                    // Thực hiện xóa tất cả huyền tinh
                    ItemManager.AbandonItem(_HuyenTinh, _Player, false, "Cường hóa vật phẩm");
                }

                int Random = KTGlobal.GetRandomNumber(0, 100);

                LogManager.WriteLog(LogTypes.Enhance, "Người chơi [" + _Player.RoleID + "][" + _Player.RoleName + "][" + Item.GoodsID + "][" + Item.Id + "] mang " + ItemManager.GetNameItem(Item) + " cường hóa :[" + (Item.Forge_level + 1) + "] với tỉ lệ  :" + _Caclution.nProb);

                ProcessTask.Process(Global._TCPManager.MySocketListener, Global._TCPManager.TcpOutPacketPool, _Player, Item.GoodsID, Item.GoodsID, Item.GoodsID, TaskTypes.Enhance);

                // Nếu có bang thì thực hiện nhiệm vụ bang hội
                if (_Player.GuildID > 0)
                {
                    GuildManager.getInstance().TaskProsecc(_Player.GuildID, _Player, -1, TaskTypes.EnhanceTime);
                }
                if (Random <= _Caclution.nProb)
                {
                    KTPlayerManager.ShowNotification(_Player, "Cường hóa thành công");

                    // Cường hóa thành công
                    ItemManager.SetEquipForgeLevel(Item, _Player, Item.Forge_level + 1, LOCK);

                    if (Item.Forge_level >= 12)
                    {
                        string ShowHang = "Người chơi " + KTGlobal.CreateStringByColor(_Player.RoleName, ColorType.Green) + " với " + KTGlobal.CreateStringByColor(_Caclution.nProb + "", ColorType.Importal) + " % đem " + KTGlobal.GetItemDescInfoStringForChat(Item) + " cường hóa lên +" + KTGlobal.CreateStringByColor(Item.Forge_level + "", ColorType.Green) + " thành công!";
                        KTGlobal.SendSystemChat(ShowHang, new List<GoodsData>() { Item });
                    }

                    return 1;
                }
                else
                {
                    if (Item.Forge_level >= 12)
                    {
                        string ShowHang = "Thật đáng tiếc! " + KTGlobal.CreateStringByColor(_Player.RoleName, ColorType.Green) + " với " + KTGlobal.CreateStringByColor(_Caclution.nProb + "", ColorType.Importal) + " % đã cường hóa " + KTGlobal.GetItemDescInfoStringForChat(Item) + " thất bại!";
                        KTGlobal.SendSystemChat(ShowHang, new List<GoodsData>() { Item });
                    }
                    KTPlayerManager.ShowNotification(_Player, "Cường hóa thất bại");
                    return 0;
                }
            }
            else
            {
                KTPlayerManager.ShowNotification(_Player, "Tỉ lệ cường hóa quá thấp");
                return -1;
            }
        }

        public static CalcProb GetEnhItem(List<GoodsData> TotalHuyenTinh, GoodsData Item)
        {
            CalcProb _Prob = new CalcProb();

            _Prob.nMoney = -1;
            _Prob.nProb = 0;
            _Prob.nTrueProb = 0;

            long AllValue = 0;

            foreach (GoodsData FixItem in TotalHuyenTinh)
            {
                ItemData _Find = ItemManager.GetItemTemplate(FixItem.GoodsID);
                if (_Find != null)
                {
                    if (IsHuyenTinh(_Find))
                    {
                        AllValue = AllValue + _Find.ItemValue;
                    }
                }
                else
                {
                    return _Prob;
                }
            }

            ItemData _ItemSelect = ItemManager.GetItemTemplate(Item.GoodsID);

            if (!ItemManager.KD_ISEQUIP(_ItemSelect.Genre) && !ItemManager.KD_ISPETEQUIP(_ItemSelect.Genre))
            {
                return _Prob;
            }

            int CurLevel = Item.Forge_level;

            int NextLevel = CurLevel + 1;

            long NeedValue = 0;

            Enhance_Value _FindValue = ItemManager._Calutaion.List_Enhance_Value.Where(x => x.EnhanceTimes == NextLevel).FirstOrDefault();

            if (_FindValue != null)
            {
                NeedValue = _FindValue.Value;
            }

            double nTypeRate = 100;

            Equip_Type_Rate TypeRate = ItemManager._Calutaion.List_Equip_Type_Rate.Where(x => (int)x.EquipType == _ItemSelect.DetailType).FirstOrDefault();
            if (TypeRate != null)
            {
                nTypeRate = TypeRate.Value;
            }

            // Chia 100 cho cẩn thận
            nTypeRate = nTypeRate / 100;

            double nCostValue = nTypeRate * NeedValue;

            long Money = (long)(nCostValue * 0.1);

            nCostValue = nCostValue - Money;

            double nProb = Math.Floor(AllValue / nCostValue * 100);

            double nTrueProb = nProb;

            if (nProb > 100)
            {
                nProb = 100;
            }

            _Prob.nMoney = (long)(Money * 3.6);
            _Prob.nProb = nProb;
            _Prob.nTrueProb = nTrueProb;

            return _Prob;
        }

        /// <summary>
        /// Các loại có thể cường hóa
        /// </summary>
        /// <param name="detailType"></param>
        /// <returns></returns>
        public static bool KD_CanEnh(int detailType)
        {
            if (detailType == (int)KE_ITEM_EQUIP_DETAILTYPE.equip_armor ||
                detailType == (int)KE_ITEM_EQUIP_DETAILTYPE.equip_belt ||
                 detailType == (int)KE_ITEM_EQUIP_DETAILTYPE.equip_amulet ||

                  detailType == (int)KE_ITEM_EQUIP_DETAILTYPE.equip_ring ||
                    detailType == (int)KE_ITEM_EQUIP_DETAILTYPE.equip_pendant ||
                       detailType == (int)KE_ITEM_EQUIP_DETAILTYPE.equip_ornament ||
                detailType == (int)KE_ITEM_EQUIP_DETAILTYPE.equip_boots ||
                detailType == (int)KE_ITEM_EQUIP_DETAILTYPE.equip_cuff ||
                detailType == (int)KE_ITEM_EQUIP_DETAILTYPE.equip_rangeweapon ||
                detailType == (int)KE_ITEM_EQUIP_DETAILTYPE.equip_meleeweapon ||
                detailType == (int)KE_ITEM_EQUIP_DETAILTYPE.equip_helm)
            {
                return true;
            }

            return false;
        }

       

        public static bool CheckEnhItem(GoodsData Input, KPlayer player)
        {
            // Nếu vật phẩm không tồn tại thì không cho cường hóa
            if (!ItemManager.IsExits(Input))
            {
                KTPlayerManager.ShowNotification(player, "Vật phẩm không tồn tại!");

                return false;
            }

            ItemData _Data = ItemManager._TotalGameItem[Input.GoodsID];

            /// Nếu không phải trang bị thì không cho cường hóa
            if (!KD_CanEnh(_Data.DetailType))
            {
                KTPlayerManager.ShowNotification(player, "Vật phẩm này không thể cường hóa");

                return false;
            }

            //if (_Data.DetailType < MIN_COMMON_EQUIP || _Data.DetailType > MAX_COMMON_EQUIP)
            //{
            //    PlayerManager.ShowNotification(player, "Vật phẩm không đủ phẩm chất cường hóa");

            //    return false;
            //}

            if (Input.Forge_level >= CalcMaxEnhanceTimes(_Data))
            {
                KTPlayerManager.ShowNotification(player, "Đã cường hóa tới cấp độ tối đa");
                return false;
            }

            return true;
        }

        #endregion CUONGHOA

        #region GHEPHUYENTINH

        /// <summary>
        /// Hàm ghép huyền tinh
        /// </summary>
        /// <param name="TotalHuyenTinh"></param>
        public static bool ComposeItemCrystal(List<GoodsData> TotalHuyenTinh, KPlayer _Player, MoneyType Type)
        {
            if (_Player == null)
            {
                KTPlayerManager.ShowNotification(_Player, "Lỗi thao tác!");
                return false;
            }
            if (TotalHuyenTinh == null)
            {
                KTPlayerManager.ShowNotification(_Player, "Bỏ vào quá ít huyền tinh");
                return false;
            }

            if (TotalHuyenTinh.Count <= 0)
            {
                KTPlayerManager.ShowNotification(_Player, "Bỏ vào quá ít huyền tinh");
                return false;
            }

            ComposeItem _ComposeItem = ComposeItem(TotalHuyenTinh);

            if (_ComposeItem == null || _ComposeItem.nItemMaxLevel == null || _ComposeItem.nItemMinLevel == null)
            {
                KTPlayerManager.ShowNotification(_Player, "Thao tác bị lỗi, hãy thử lại!");
                return false;
            }

            if (_ComposeItem.nFee > 0)
            {
                int MoneyNeed = (int)_ComposeItem.nFee;

                if (!KTGlobal.IsHaveMoney(_Player, MoneyNeed, Type))
                {
                    KTPlayerManager.ShowNotification(_Player, "Bạc mang trên người không đủ");
                    return false;
                }

                // Nếu ok hết thực hiện cường hóa
                SubRep _Submoney = KTGlobal.SubMoney(_Player, MoneyNeed, Type, "COMPOSECRYTAL");
                if (!_Submoney.IsOK)
                {
                    KTPlayerManager.ShowNotification(_Player, "Có lỗi khi trừ tiền");
                    return false;
                }

                // Thực hiện xóa huyền tinh
                foreach (GoodsData _HuyenTinh in TotalHuyenTinh)
                {
                    // Thực hiện xóa tất cả huyền tinh
                    ItemManager.AbandonItem(_HuyenTinh, _Player, false, "Ghép huyền tinh");
                }

                int Random = KTGlobal.GetRandomNumber(0, 100);

                int MinRate = _ComposeItem.nMinLevelRate;
                int MaxRate = _ComposeItem.nMaxLevelRate;

                int Lock = 0;

                var Find = TotalHuyenTinh.Where(x => x.Binding == 1).FirstOrDefault();
                if (Find != null)
                {
                    Lock = 1;
                }

                if (Type == MoneyType.BacKhoa)
                {
                    Lock = 1;
                }

                // Nếu random ra mà nhỏ hơn min rate tức là vào huyền tinh cấp thấp
                if (Random < MinRate)
                {
                    if (ItemManager.CreateItem(Global._TCPManager.TcpOutPacketPool, _Player, _ComposeItem.nItemMinLevel.ItemID, 1, 0, "COMPOSECRYTAL", false, Lock, false, ItemManager.ConstGoodsEndTime))
                    {
                        KTPlayerManager.ShowNotification(_Player, "Hợp thành :" + _ComposeItem.nItemMinLevel.Name);

                        LogManager.WriteLog(LogTypes.Item, "[" + _Player.RoleID + "][" + _Player.RoleName + "][Ghép Huyền Tinh] : Hợp thành thành công HUYỀN TINH :" + _ComposeItem.nItemMinLevel.Name);
                    }
                    else
                    {
                        KTPlayerManager.ShowNotification(_Player, "Có lỗi khi thêm huyền tinh vào túi đồ vui lòng liên hệ ADMIN để được giúp đỡ");

                        LogManager.WriteLog(LogTypes.Item, "[" + _Player.RoleID + "][" + _Player.RoleName + "][Ghép Huyền Tinh][BUG] : Hợp thành thành công HUYỀN TINH lỗi :" + _ComposeItem.nItemMinLevel.Name);
                    }
                } // Nếu mà random ra max rate tức là vào huyền tinh cấp cao
                else
                {
                    if (ItemManager.CreateItem(Global._TCPManager.TcpOutPacketPool, _Player, _ComposeItem.nItemMaxLevel.ItemID, 1, 0, "COMPOSECRYTAL", false, Lock, false, ItemManager.ConstGoodsEndTime))
                    {
                        KTPlayerManager.ShowNotification(_Player, "Hợp thành :" + _ComposeItem.nItemMaxLevel.Name);

                        LogManager.WriteLog(LogTypes.Item, "[" + _Player.RoleID + "][" + _Player.RoleName + "][Ghép Huyền Tinh] : Hợp thành thành công HUYỀN TINH :" + _ComposeItem.nItemMaxLevel.Name);
                    }
                    else
                    {
                        KTPlayerManager.ShowNotification(_Player, "Có lỗi khi thêm huyền tinh vào túi đồ vui lòng liên hệ ADMIN để được giúp đỡ");

                        LogManager.WriteLog(LogTypes.Item, "[" + _Player.RoleID + "][" + _Player.RoleName + "][Ghép Huyền Tinh][BUG] : Hợp thành thành công HUYỀN TINH lỗi :" + _ComposeItem.nItemMaxLevel.Name);
                    }
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        public static ComposeItem ComposeItem(List<GoodsData> TotalHuyenTinh)
        {
            if (TotalHuyenTinh == null || TotalHuyenTinh.Count <= 0)
            {
                return null;
            }

            ComposeItem _ComposeItem = new ComposeItem();
            _ComposeItem.nFee = 0;

            _ComposeItem.nMaxLevelRate = -1;
            _ComposeItem.nMinLevelRate = -1;

            int AllValue = 0;

            foreach (GoodsData FixItem in TotalHuyenTinh)
            {
                ItemData _Find = ItemManager.GetItemTemplate(FixItem.GoodsID);
                if (_Find != null)
                {
                    if (IsHuyenTinh(_Find))
                    {
                        AllValue = AllValue + _Find.ItemValue;
                    }
                    else
                    {
                        return _ComposeItem;
                    }
                }
            }

            int MinLevel = 0;
            int MaxLevel = 0;

            int nMinLevelRate = 0;
            int nMaxLevelRate = 0;

            List<ItemData> _CristalItem = ItemManager.TotalItem.Where(x => x.Genre == 18 && x.DetailType == 1 && x.ParticularType == 1).OrderByDescending(x => x.Level).ToList();

            foreach (ItemData _Item in _CristalItem)
            {
                if (AllValue >= _Item.ItemValue)
                {
                    MinLevel = _Item.Level;
                    break;
                }
            }

            MaxLevel = MinLevel + 1;

            ItemData MinLevelItem = _CristalItem.Where(x => x.Level == MinLevel).FirstOrDefault();

            ItemData MaxLevelItem = _CristalItem.Where(x => x.Level == MaxLevel).FirstOrDefault();

            _ComposeItem.nItemMinLevel = MinLevelItem;
            _ComposeItem.nItemMaxLevel = MaxLevelItem;

            int MoneyNeed = AllValue / 10;

            if (MinLevel >= 12)
            {
                MinLevel = 11;
                nMinLevelRate = 0;
                nMaxLevelRate = 1;
            }
            else
            {
                nMinLevelRate = MaxLevelItem.ItemValue - AllValue;
                nMaxLevelRate = AllValue - MinLevelItem.ItemValue;

                /// Quy đổi ra đơn vị % tương ứng
                float nMinLevelRateP = nMinLevelRate * 100f / (nMinLevelRate + nMaxLevelRate);
                float nMaxLevelRateP = nMaxLevelRate * 100f / (nMinLevelRate + nMaxLevelRate);
                if (nMinLevelRateP - (int)nMinLevelRateP >= 0.5f)
                {
                    nMinLevelRateP += 1f;
                }
                if (nMaxLevelRateP - (int)nMaxLevelRateP >= 0.5f)
                {
                    nMaxLevelRateP += 1f;
                }

                nMinLevelRate = (int)nMinLevelRateP;
                nMaxLevelRate = (int)nMaxLevelRateP;
            }

            _ComposeItem.nMinLevelRate = nMinLevelRate;
            _ComposeItem.nMaxLevelRate = nMaxLevelRate;
            _ComposeItem.nFee = MoneyNeed;

            return _ComposeItem;
        }

        #endregion GHEPHUYENTINH

        #region TACHHUYENTINH

        /// <summary>
        /// Tách Huyền Tinh từ trang bị tương ứng
        /// </summary>
        /// <param name="InputCrystal"></param>
        /// <param name="_Player"></param>
        /// <param name="Type"></param>
        /// <returns></returns>
        public static bool DoSplitCrystal(GoodsData InputCrystal, KPlayer _Player)
        {
            ItemData _Input = ItemManager.GetItemTemplate(InputCrystal.GoodsID);

            if (_Input == null)
            {
                KTPlayerManager.ShowNotification(_Player, "Không tìm thấy vật phẩm");
                return false;
            }

            if (!IsHuyenTinh(_Input))
            {
                KTPlayerManager.ShowNotification(_Player, "Vật phẩm đặt vào không phải huyền tinh");
                return false;
            }

            if (_Player.NeedToShowInputSecondPassword())
            {
                KTPlayerManager.ShowNotification(_Player, "Mở khóa cấp 2 trước khi thực hiện thao tác");
                return false;
            }

            //int MoneyNeed = _Input.ItemValue / 10;

            List<ItemData> TryFindInfo = TotalCrystalSplit(InputCrystal);

            if (TryFindInfo == null)
            {
                KTPlayerManager.ShowNotification(_Player, "Không thể tách!");
                return false;
            }

            if (!KTGlobal.IsHaveSpace(TryFindInfo.Count, _Player))
            {
                KTPlayerManager.ShowNotification(_Player, "Túi đồ không đủ chỗ!");
                return false;
            }

            //if (!KTGlobal.IsHaveMoney(_Player, MoneyNeed, Type))
            //{
            //    PlayerManager.ShowNotification(_Player, "Bạc trên người không đủ!");
            //    return false;
            //}

            //SubRep MoneySub = KTGlobal.SubMoney(_Player, MoneyNeed, Type);
            //if (!MoneySub.IsOK)
            //{
            //    PlayerManager.ShowNotification(_Player, "Bạc trên người không đủ!");
            //    return false;
            //}

            int isBound = InputCrystal.Binding;

            // Xóa item source
            if (ItemManager.AbandonItem(InputCrystal, _Player, false, "Tách huyền tinh"))
            {
                // Thực hiện add item vào túi
                foreach (ItemData _ItemOut in TryFindInfo)
                {
                    if (!ItemManager.CreateItem(Global._TCPManager.TcpOutPacketPool, _Player, _ItemOut.ItemID, 1, 0, "SPLITCRYTAL", false, isBound, false, ItemManager.ConstGoodsEndTime, "", -1, "", 0, 1, true))
                    {
                        KTPlayerManager.ShowNotification(_Player, "Tách huyền tinh thành :" + _ItemOut.Name);

                        LogManager.WriteLog(LogTypes.Item, "[" + _Player.RoleID + "][" + _Player.RoleName + "][Tách huyền tinh] : Tách huyền tinh HUYỀN TINH :" + _ItemOut.Name);
                    }
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Hàm tách huyền tinh cấp cao ra huyền tinh cấp thấp
        /// </summary>
        /// <param name="InputCrystal"></param>
        /// <param name="_Input"></param>
        /// <param name="isBound"></param>
        /// <returns></returns>
        public static List<ItemData> TotalCrystalSplit(GoodsData InputCrystal, ItemData _Input = null)
        {
            List<ItemData> _OutputCrytals = null;

            if (_Input == null)
            {
                _Input = ItemManager.GetItemTemplate(InputCrystal.GoodsID);
            }

            if (_Input == null)
            {
                return _OutputCrytals;
            }

            if (!IsHuyenTinh(_Input))
            {
                return _OutputCrytals;
            }

            // Nếu cấp mà nhỏ hơn 10 thì không cho tách
            if (_Input.Level < 6)
            {
                return _OutputCrytals;
            }

            double nValue = _Input.ItemValue * KTGlobal.KD_SPLITCRYTAL_VALUE_COST;

            _OutputCrytals = new List<ItemData>();

            // Lấy ra toàn bộ huyền tinh theo cấp độ giảm dần
            List<ItemData> _CristalItem = ItemManager.TotalItem.Where(x => x.Genre == 18 && x.DetailType == 1 && x.ParticularType == 1).OrderByDescending(x => x.Level).ToList();

            var FindLevel1 = _CristalItem.Where(x => x.Level == 1).FirstOrDefault();

            // Tối đa tách ra 10 huyền tinh cấp thấp hơn
            for (int nCount = 1; nCount < MAX_SPLITNUM; nCount++)
            {
                foreach (ItemData item in _CristalItem)
                {
                    if (nValue / item.ItemValue > 1)
                    {
                        double nNum = Math.Floor(nValue / item.ItemValue);

                        if (nNum > 1)
                        {
                            for (int i = 0; i < nNum; i++)
                            {
                                // Chỉ lấy các huyền tinh có level > 4 còn lại thì người chơi phải chịu lỗ VALUE coi như phí
                                if (item.Level > 3)
                                {
                                    _OutputCrytals.Add(item);
                                }
                            }

                            nValue = nValue % item.ItemValue;

                            break;
                        }
                    }
                }
                if ((nValue / FindLevel1.ItemValue < 1) || nValue == 0)
                {
                    break;
                }
            }

            int SUM = _OutputCrytals.Sum(x => x.ItemValue);
            return _OutputCrytals;
        }

        #endregion TACHHUYENTINH

        ///VIẾT LẠI HÀM NÀY CHỈNH LẠI RATE

        #region TACHHUYENTINH_TUTRANGBI

        /// <summary>
        /// Tách Huyền Tinh khỏi trang bị
        /// </summary>
        /// <param name="InputItem"></param>
        /// <param name="_Player"></param>
        /// <param name="Type"></param>
        /// <returns></returns>
        public static bool DoSplitCrystalFromEquip(GoodsData InputItem, KPlayer _Player)
        {
            ItemData _Input = ItemManager.GetItemTemplate(InputItem.GoodsID);

            if (_Input == null)
            {
                KTPlayerManager.ShowNotification(_Player, "Không tìm thấy vật phẩm");
                return false;
            }

            int Forge_level = InputItem.Forge_level;

            if (Forge_level < KTGlobal.KD_MIN_ENHLEVEL_TO_SPLIT)
            {
                KTPlayerManager.ShowNotification(_Player, "Chỉ trang bị cường hóa lên cấp " + KTGlobal.KD_MIN_ENHLEVEL_TO_SPLIT + " mới có thể tách huyền tinh!");
                return false;
            }

            //if (_Input.Level < KTGlobal.KD_MIN_ENHLEVEL_TO_SPLIT)
            //{
            //    PlayerManager.ShowNotification(_Player, "Chỉ trang bị cấp " + KTGlobal.KD_MIN_ENHLEVEL_TO_SPLIT + " trở lên mới có thể tách huyền tinh ra!");
            //    return false;
            //}

            List<Enhance_Value> _FindValue = ItemManager._Calutaion.List_Enhance_Value.Where(x => x.EnhanceTimes <= Forge_level).ToList();

            if (_FindValue == null)
            {
                return false;
            }

            // lấy ra tổng tài phú của các huyền tinh
            double TotalValue = _FindValue.Sum(x => x.Value);

            double nTypeRate = 100;

            Equip_Type_Rate TypeRate = ItemManager._Calutaion.List_Equip_Type_Rate.Where(x => (int)x.EquipType == _Input.DetailType).FirstOrDefault();

            if (TypeRate != null)
            {
                nTypeRate = TypeRate.Value;
            }

            // Chia 100 cho cẩn thận
            nTypeRate = nTypeRate / 100;

            TotalValue = TotalValue * nTypeRate;

            List<ItemData> TryFindInfo = TotalCrystalSplitFromEquip(TotalValue * KTGlobal.KD_SPLIT_VALUE_COST);

            if (TryFindInfo == null)
            {
                KTPlayerManager.ShowNotification(_Player, "Không thể tách!");
                return false;
            }

            if (!KTGlobal.IsHaveSpace(TryFindInfo.Count, _Player))
            {
                KTPlayerManager.ShowNotification(_Player, "Túi đồ không đủ chỗ!");
                return false;
            }

            int nEnhLevel = InputItem.Forge_level;
            double nMoney = 0;
            if (nEnhLevel >= 12 && nEnhLevel <= 13)
            {
                nMoney = Math.Floor(TotalValue * ItemEnhance.PEEL_RESTORE_RATE_12);
            }
            else if (nEnhLevel >= 14 && nEnhLevel <= 16)
            {
                nMoney = Math.Floor(TotalValue * ItemEnhance.PEEL_RESTORE_RATE_14);
            }

            // Số Bạc Trả Về
            if (nMoney > 0)
            {
                KTPlayerManager.ShowNotification(_Player, KTGlobal.CreateStringByColor("Tách trang bị hoàn trả [" + nMoney + "] Bạc khóa", ColorType.Green));

                KTGlobal.AddMoney(_Player, (int)nMoney, MoneyType.BacKhoa, "TACHTRANGBI|" + nEnhLevel + "|" + InputItem.GoodsID);
            }
            ///// Số bạc khóa nhận được sau khi tách
            //int nMoneyAdd = (int) (_Input.ItemValue * KTGlobal.KD_SPLIT_VALUE_COST / 10);
            ///// Thêm bạc khóa cho người chơi
            //GameManager.ClientMgr.AddUserBoundMoney(_Player, nMoneyAdd, "BUY_SHOP_ITEM");

            // Xóa item source
            if (ItemManager.SetEquipForgeLevel(InputItem, _Player, 0, InputItem.Binding))
            {
                // Thực hiện add item vào túi
                foreach (ItemData _ItemOut in TryFindInfo)
                {
                    if (!ItemManager.CreateItem(Global._TCPManager.TcpOutPacketPool, _Player, _ItemOut.ItemID, 1, 0, "SPLITCRYTALFROMEQU", false, InputItem.Binding, false, ItemManager.ConstGoodsEndTime, "", -1, "", 0, 1, true))
                    {
                        KTPlayerManager.ShowNotification(_Player, "Tách huyền tinh thành :" + _ItemOut.Name);

                        LogManager.WriteLog(LogTypes.Item, "[" + _Player.RoleID + "][" + _Player.RoleName + "][Tách huyền tinh TỪ TRANG BỊ] : Tách huyền tinh HUYỀN TINH :" + _ItemOut.Name);
                    }
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Hàm tách huyền tinh cấp cao ra huyền tinh cấp thấp
        /// </summary>
        /// <param name="nValue"></param>
        /// <returns></returns>
        public static List<ItemData> TotalCrystalSplitFromEquip(double nValue)
        {
            List<ItemData> _OutputCrytals = null;

            _OutputCrytals = new List<ItemData>();

            // Lấy ra toàn bộ huyền tinh theo cấp độ giảm dần
            List<ItemData> _CristalItem = ItemManager.TotalItem.Where(x => x.Genre == 18 && x.DetailType == 1 && x.ParticularType == 1).OrderByDescending(x => x.Level).ToList();

            var FindLevel1 = _CristalItem.Where(x => x.Level == 1).FirstOrDefault();

            // Tối đa tách ra 10 huyền tinh cấp thấp hơn
            for (int nCount = 1; nCount < MAX_SPLITNUM; nCount++)
            {
                foreach (ItemData item in _CristalItem)
                {
                    if (nValue / item.ItemValue > 1)
                    {
                        double nNum = Math.Floor(nValue / item.ItemValue);

                        if (nNum > 1)
                        {
                            for (int i = 0; i < nNum; i++)
                            {
                                // Chỉ lấy các huyền tinh có level > 4 còn lại thì người chơi phải chịu lỗ VALUE coi như phí
                                if (item.Level > 3)
                                {
                                    _OutputCrytals.Add(item);
                                }
                            }

                            nValue = nValue % item.ItemValue;

                            break;
                        }
                    }
                }
                if ((nValue / FindLevel1.ItemValue < 1) || nValue == 0)
                {
                    break;
                }
            }

            int SUM = _OutputCrytals.Sum(x => x.ItemValue);
            Console.WriteLine(SUM);
            return _OutputCrytals;
        }

        #endregion TACHHUYENTINH_TUTRANGBI
    }
}