using GameServer.KiemThe.Entities;
using GameServer.KiemThe.Logic;
using GameServer.KiemThe.Logic.Manager.Shop;
using GameServer.Logic;
using Server.Data;
using System;
using System.Linq;
using System.Xml.Serialization;

namespace GameServer.KiemThe.Core.Item
{
    public class ItemRandomBox
    {
        public static string RandomBoxConfig = "Config/KT_Item/KT_ConfigRandomBox.xml";

        public static ConfigBox _ConfigBox = new ConfigBox();

        /// <summary>
        /// Loading all Drop
        /// </summary>
        public static void Setup()
        {
            string Files = KTGlobal.GetDataPath(RandomBoxConfig);

            using (var stream = System.IO.File.OpenRead(Files))
            {
                var serializer = new XmlSerializer(typeof(ConfigBox));
                _ConfigBox = serializer.Deserialize(stream) as ConfigBox;
            }
        }

        /// <summary>
        /// Mở rương vật phẩm
        /// </summary>
        /// <param name="player"></param>
        /// <param name="boxDbID"></param>
        /// <returns></returns>
        public static bool OpenBoxRandom(KPlayer player, int boxDbID)
        {
            /// Nếu ko đủ 1 ô để nhận thưởng thì thôi
            if (!KTGlobal.IsHaveSpace(6, player))
            {
                KTPlayerManager.ShowNotification(player, "Cần ít nhất 6 ô trống để mở");
                return false;
            }

            /// Vật phẩm tương ứng
            GoodsData itemGD = player.GoodsData.Find(boxDbID, 0);
            /// Nếu toác
            if (itemGD == null)
            {
                KTPlayerManager.ShowNotification(player, "Vật phẩm không khả dụng!");
                return false;
            }
            /// Thông tin vật phẩm
            ItemData itemData = ItemManager.GetItemTemplate(itemGD.GoodsID);
            /// Nếu toác
            if (itemData == null)
            {
                KTPlayerManager.ShowNotification(player, "Vật phẩm không khả dụng!");
                return false;
            }
            /// ID loại hộp
            int boxID = itemData.ItemValue;

            var findbox = _ConfigBox.Boxs.Where(x => x.IDBox == boxID).FirstOrDefault();
            if (findbox != null)
            {
                int TotalRate = findbox.TotalRate;

                int LimitDay = findbox.LimitDay;

                int LimitWeek = findbox.LimitWeek;

                if (LimitWeek > 0)
                {
                    int TotalOpenInWeek = player.GetValueOfWeekRecore(boxID);

                    if (TotalOpenInWeek > LimitWeek)
                    {
                        KTPlayerManager.ShowNotification(player, "Tuần này bạn đã mở hết số lượt cho phép");
                        return false;
                    }
                }

                if (LimitDay > 0)
                {
                    int TotalOpenDay = player.GetValueOfDailyRecore(boxID);

                    if (TotalOpenDay > LimitDay)
                    {
                        KTPlayerManager.ShowNotification(player, "Hôm nay bạn đã mở hết số lượt cho phép");
                        return false;
                    }
                }

                int RanndomValue = KTGlobal.GetRandomNumber(0, TotalRate);

                int Add = 0;

                ItemRandom _SelectItem = null;

                foreach (ItemRandom _Item in findbox.Items)
                {
                    Add = Add + _Item.Rate;
                    if (Add >= RanndomValue)
                    {
                        _SelectItem = _Item;
                        break;
                    }
                }

                if (_SelectItem != null)
                {
                    if (_SelectItem.Type == 1)
                    {
                        SubRep AddMoneyKq = KTGlobal.AddMoney(player, _SelectItem.Number, Entities.MoneyType.BacKhoa, "RANDOMBOX_" + boxID);

                        if (AddMoneyKq.IsOK)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }

                    if (_SelectItem.Type == 2)
                    {
                        SubRep AddMoneyKq = KTGlobal.AddMoney(player, _SelectItem.Number, Entities.MoneyType.Bac, "RANDOMBOX_" + boxID);

                        if (AddMoneyKq.IsOK)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }

                    if (_SelectItem.Type == 3)
                    {
                        SubRep AddMoneyKq = KTGlobal.AddMoney(player, _SelectItem.Number, Entities.MoneyType.DongKhoa, "RANDOMBOX_" + boxID);

                        if (AddMoneyKq.IsOK)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }

                    if (_SelectItem.Type == 4)
                    {
                        SubRep AddMoneyKq = KTGlobal.AddMoney(player, _SelectItem.Number, Entities.MoneyType.Dong, "RANDOMBOX_" + boxID);

                        if (AddMoneyKq.IsOK)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }


                    if (_SelectItem.Type == 5)
                    {
                        KTPlayerManager.AddExp(player, _SelectItem.Number);
                        return true;
                    }

                    if (_SelectItem.Type == 0)
                    {
                        string TimeFix = ItemManager.ConstGoodsEndTime;


                        if (_SelectItem.TimeLimit>0)
                        {
                            string TMP = "1900-01-01 12:00:00";

                            DateTime Dt = DateTime.Now.AddMinutes(_SelectItem.TimeLimit);

                            TimeFix = Dt.ToString("yyyy-MM-dd HH:mm:ss");
                        }

                        /// Có khóa không
                        int bindState = _SelectItem.Lock;
                        /// Add thêm đoạn này để mở khóa rương tranh đoạt lãnh thổ
                        if (boxID == 2176 || boxID == 2177)
                        {
                            bindState = _SelectItem.Lock == 1 ? 1 : itemGD.Binding;
                        }

                        if (!ItemManager.CreateItem(Global._TCPManager.TcpOutPacketPool, player, _SelectItem.ItemID, _SelectItem.Number, 0, "RANDOMBOX_" + boxID, true, bindState, false, TimeFix,"", _SelectItem.Series))
                        {
                            KTPlayerManager.ShowNotification(player, "Có lỗi khi nhận vật phẩm chế tạo");

                            return false;
                        }
                        else
                        {

                            if (LimitWeek > 0)
                            {
                               
                                int TotalOpenInWeek = player.GetValueOfWeekRecore(boxID);

                                player.SetValueOfWeekRecore(boxID, TotalOpenInWeek + 1);
                              
                            }

                            if (LimitDay > 0)
                            {
                                int TotalOpenDay = player.GetValueOfDailyRecore(boxID);

                                player.SetValueOfDailyRecore(boxID, TotalOpenDay + 1);

                            }

                            return true;
                        }
                    }
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }

            return false;
        }
    }
}