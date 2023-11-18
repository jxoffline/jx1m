using GameServer.KiemThe.Logic;
using GameServer.KiemThe.Logic.Manager.Shop;
using GameServer.Logic;
using Server.Data;
using Server.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace GameServer.KiemThe.Core.Item
{
    public class ItemRefine
    {
        public static List<Refine> TotaRefine = new List<Refine>();

        public static string ConfigURL = "Config/KT_Item/Refine.xml";

        public static double PEEL_RESTORE_RATE_12 = 3 / 100;
        public static double PEEL_RESTORE_RATE_14 = 5 / 100;
        public static double ENHANCE_COST_RATE = 10 / 100;

        public static void Setup()
        {
            string Files = KTGlobal.GetDataPath(ConfigURL);
            using (var stream = System.IO.File.OpenRead(Files))
            {
                var serializer = new XmlSerializer(typeof(List<Refine>));
                List<Refine> _Item = serializer.Deserialize(stream) as List<Refine>;
                TotaRefine = _Item;
            }
        }

        public static double CalcEnhanceValue(GoodsData InputItem)
        {
            ItemData _itemData = ItemManager.GetItemTemplate(InputItem.GoodsID);

            int nEnhTimes = InputItem.Forge_level;
            double nPeelValue = 0;

            if (_itemData != null)
            {
                if (nEnhTimes > 0)
                {
                    List<Enhance_Value> _TotalEnchaceValue = ItemManager._Calutaion.List_Enhance_Value.Where(x => x.EnhanceTimes <= nEnhTimes).ToList();

                    nPeelValue = _TotalEnchaceValue.Sum(x => x.Value);
                }
            }

            Equip_Type_Rate TypeRate = ItemManager._Calutaion.List_Equip_Type_Rate.Where(x => (int)x.EquipType == _itemData.DetailType).FirstOrDefault();

            if (TypeRate != null)
            {
                double Rate = ((double)TypeRate.Value / 100);

                nPeelValue = nPeelValue * Rate;
            }

            return nPeelValue;
        }

        public static double CalcRefineMoney(GoodsData InputItem)
        {
            double MoneyNeed = 100;

            ItemData _itemData = ItemManager.GetItemTemplate(InputItem.GoodsID);

            if (_itemData != null)
            {
                if (ItemManager.IsEquip(InputItem))
                {
                    double nEnhanceValue = ItemRefine.CalcEnhanceValue(InputItem);
                    int nEnhLevel = InputItem.Forge_level;
                    double nRefineMoney = 0;
                    int nJbPrice = 1;

                    if (nEnhLevel >= 12 && nEnhLevel <= 13)
                    {
                        nRefineMoney = nEnhanceValue * ENHANCE_COST_RATE * nJbPrice - nEnhanceValue * PEEL_RESTORE_RATE_12;
                    }
                    else if (nEnhLevel >= 14 && nEnhLevel <= 16)
                    {
                        nRefineMoney = nEnhanceValue * ENHANCE_COST_RATE * nJbPrice - nEnhanceValue * PEEL_RESTORE_RATE_14;
                    }
                    else
                    {
                        nRefineMoney = nEnhanceValue * ENHANCE_COST_RATE * nJbPrice;
                    }

                    MoneyNeed = nRefineMoney;
                }
            }

            return MoneyNeed;
        }

        public static int DoRefine(KPlayer client, GoodsData InputData, GoodsData Suff, List<GoodsData> TotalCrytals, int ItemDescSelect)
        {
            ItemData _SourceFind = ItemManager.GetItemTemplate(InputData.GoodsID);
            if (_SourceFind == null)
            {
                KTPlayerManager.ShowNotification(client, "Vật phẩm luyện hóa không hợp lệ");
                return -1;
            }

            double CrytalValue = 0;

            ItemData SuffItem = ItemManager.GetItemTemplate(Suff.GoodsID);

            if (SuffItem == null)
            {
                KTPlayerManager.ShowNotification(client, "Chế tạo đồ đặt vào không hợp lệ");
                return -2;
            }

            ItemData DestItem = ItemManager.GetItemTemplate(ItemDescSelect);

            if (DestItem == null)
            {
                KTPlayerManager.ShowNotification(client, "Vật phẩm muốn nhận không hợp lệ");
                return -20;
            }

            if (SuffItem.Genre != 18 || SuffItem.DetailType != 2)
            {
                KTPlayerManager.ShowNotification(client, "Chế tạo đồ đặt vào không hợp lệ");
                return -2;
            }

            Refine find = TotaRefine.Where(x => x.SourceItem == _SourceFind.ItemID && x.RefineId == SuffItem.ItemID && x.ProduceItem == ItemDescSelect).FirstOrDefault();
            if (find == null)
            {
                KTPlayerManager.ShowNotification(client, "Không tìm thấy công thức này");
                return -3;
            }

            if (TotalCrytals != null)
            {
                if (TotalCrytals.Count > 0)
                {
                    foreach (GoodsData FixItem in TotalCrytals)
                    {
                        ItemData _Find = ItemManager.GetItemTemplate(FixItem.GoodsID);
                        if (_Find != null)
                        {
                            if (ItemEnhance.IsHuyenTinh(_Find))
                            {
                                CrytalValue = CrytalValue + _Find.ItemValue;
                            }
                        }
                        else
                        {
                            KTPlayerManager.ShowNotification(client, "Vật phẩm đặt vào không đúng");
                            return -4;
                        }
                    }
                }
            }

            double Percent = 0;
            if (InputData.Forge_level > 0)
            {
                Percent = Math.Floor((ItemRefine.CalcEnhanceValue(InputData) * 8 + CrytalValue * 10) / (ItemRefine.CalcEnhanceValue(InputData) * 9) * 100);

                if (Percent > 100)
                {
                    Percent = 100;
                }
            }
            else
            {
                Percent = 100;
            }

            if (Percent < 100)
            {
                KTPlayerManager.ShowNotification(client, "Tỉ lệ phải đạt 100% mới có thể luyện hóa vui lòng bỏ thêm huyền tinh");
                return -4;
            }

            double nMoney = find.Fee + ItemRefine.CalcRefineMoney(InputData);

            // Nếu tổng bạc khóa + bạc thường mà không đủ thì báo bạc không đủ
            if (client.BoundMoney + client.Money < nMoney)
            {
                KTPlayerManager.ShowNotification(client, "Bạc trên người không đủ");
                return -5;
            }

            // nếu không đủ 1 chỗ trống thì báo thiếu 1 chỗ trống
            if (!KTGlobal.IsHaveSpace(1, client))
            {
                KTPlayerManager.ShowNotification(client, "Cần tối thiểu 1 chỗ trống trong túi đồ mới có thể luyện hóa");
                return -6;
            }

            if (nMoney < 0)
            {
                KTPlayerManager.ShowNotification(client, "Có lỗi trong công thức tính toán");
                return -8;
            }

            // Tính xem bạc khóa có đủ để trừ không
            int MoneyLess = client.BoundMoney - (int)nMoney;

            // nếu như không đủ bạc khóa
            if (MoneyLess < 0)
            {
                // trừ toàn bộ số bạc khóa hiện có
                SubRep SubMoney = KTGlobal.SubMoney(client, client.BoundMoney, Entities.MoneyType.BacKhoa, "REFINE_" + InputData.Id);

                if (!SubMoney.IsOK)
                {
                    KTPlayerManager.ShowNotification(client, "Không thể trừ bạc khóa tiền");
                    return -6;
                }

                //Tiếp tục trừ tới chỗ bạc còn thiếu sang bạc thường
                SubMoney = KTGlobal.SubMoney(client, Math.Abs(MoneyLess), Entities.MoneyType.Bac, "REFINE_" + InputData.Id);

                if (!SubMoney.IsOK)
                {
                    KTPlayerManager.ShowNotification(client, "Không thể trừ bạc thương");
                    return -6;
                }
            }
            else
            {
                // nếu bạc khóa mà đủ thì chỉ trừ mình bạc khóa
                SubRep SubMoney = KTGlobal.SubMoney(client, (int)nMoney, Entities.MoneyType.BacKhoa, "REFINE_" + InputData.Id);

                if (!SubMoney.IsOK)
                {
                    KTPlayerManager.ShowNotification(client, "Không thể trừ bạc khóa tiền");
                    return -6;
                }
            }

            // Thực hiên luyện hóa

            if (!ItemManager.AbandonItem(InputData, client, false, "REFINE_" + InputData.Id))
            {
                KTPlayerManager.ShowNotification(client, "Không thể xóa trang bị nguồn");
                return -7;
            }

            if (!ItemManager.AbandonItem(Suff, client, false, "REFINE_" + InputData.Id))
            {
                KTPlayerManager.ShowNotification(client, "Không thể xóa luyện hóa đồ");
                return -8;
            }

            if (TotalCrytals != null)
            {
                if (TotalCrytals.Count > 0)
                {
                    foreach (GoodsData FixItem in TotalCrytals)
                    {
                        if (!ItemManager.AbandonItem(FixItem, client, false, "REFINE_" + InputData.Id))
                        {
                            KTPlayerManager.ShowNotification(client, "Không thể huyền tinh");
                            return -9;
                        }
                    }
                }
            }

            if (ItemManager.CreateItem(Global._TCPManager.TcpOutPacketPool, client, ItemDescSelect, 1, 0, "COMPOSECRYTAL", false, 1, false, ItemManager.ConstGoodsEndTime, "", -1, "", InputData.Forge_level, 0, true))
            {
                KTPlayerManager.ShowNotification(client, "Luyện hóa thành công :" + DestItem.Name);

                LogManager.WriteLog(LogTypes.Item, "[" + client.RoleID + "][" + client.RoleName + "][Luyện hóa] : Luyện hóa thành công :" + DestItem.Name);

                return 0;
            }
            else
            {
                KTPlayerManager.ShowNotification(client, "Có lỗi khi thêm huyền tinh vào túi đồ vui lòng liên hệ ADMIN để được giúp đỡ");

                LogManager.WriteLog(LogTypes.Item, "[" + client.RoleID + "][" + client.RoleName + "][Luyện hóa][BUG] : Luyện hóa thành vật phẩm :" + DestItem.Name + "|DestItemID :" + DestItem.ItemID);
            }

            return -100;
        }



        /// <summary>
        /// Kiểm tra trang bị tương ứng có luyện hóa được không
        /// </summary>
        /// <param name="itemData"></param>
        /// <returns></returns>
        public static bool IsRefinedEquip(ItemData itemData)
        {
            /// Toác
            if (itemData == null)
            {
                return false;
            }
            return ItemRefine.TotaRefine.Any(x => x.ProduceItem == itemData.ItemID);
        }
    }
}