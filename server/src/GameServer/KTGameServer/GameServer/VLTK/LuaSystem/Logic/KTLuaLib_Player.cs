using GameServer.KiemThe.Core.Item;
using GameServer.KiemThe.Core.Rechage;
using GameServer.KiemThe.Core.Shop;
using GameServer.KiemThe.Core.Task;
using GameServer.KiemThe.Entities;
using GameServer.KiemThe.GameEvents.BaiHuTang;
using GameServer.KiemThe.GameEvents.FactionBattle;
using GameServer.KiemThe.GameEvents.SpecialEvent;
using GameServer.KiemThe.GameEvents.TeamBattle;
using GameServer.KiemThe.Logic;
using GameServer.KiemThe.Logic.Manager.Battle;
using GameServer.KiemThe.Logic.Manager.Shop;
using GameServer.KiemThe.LuaSystem.Entities;
using GameServer.Logic;
using GameServer.Server;
using GameServer.VLTK.Core.GuildManager;
using GameServer.VLTK.Entities.Pet;
using MoonSharp.Interpreter;
using Server.Data;
using Server.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using static GameServer.Logic.KTMapManager;

namespace GameServer.KiemThe.LuaSystem.Logic
{
    /// <summary>
    /// Cung cấp thư viện dùng cho Lua, tương tác với người chơi
    /// </summary>
    [MoonSharpUserData]
    public static class KTLuaLib_Player
    {
        /// <summary>
        /// Mở Shop tương ứng
        /// </summary>
        /// <param name="npc"></param>
        /// <param name="player"></param>
        /// <param name="shopID"></param>
        public static void OpenShop(Lua_NPC npc, Lua_Player player, int shopID)
        {
            if (player == null || player.RefObject == null)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Lua error on NPCDialog:Show, Player is NULL."));
                return;
            }

            if (npc == null || npc.RefObject == null)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Lua error on NPCDialog:Show, NPC is NULL."));
                return;
            }

            UnityEngine.Vector2 _SelfPos = new UnityEngine.Vector2((int)player.RefObject.CurrentPos.X, (int)player.RefObject.CurrentPos.Y);

            UnityEngine.Vector2 NpcPos = new UnityEngine.Vector2((int)npc.RefObject.CurrentPos.X, (int)npc.RefObject.CurrentPos.Y);

            float Dis = UnityEngine.Vector2.Distance(_SelfPos, NpcPos);
            if (Dis > 100)
            {
                KTPlayerManager.ShowNotification(player.RefObject, "Khoảng cách quá xa");
                return;
            }

            /// Đánh dấu vị trí mở Shop lần trước từ NPC
            player.RefObject.LastShopNPC = npc.RefObject;

            ShopTab _Shop = ShopManager.GetShopTable(shopID, player.RefObject);

            if (_Shop != null)
            {
                KT_TCPHandler.SendShopData(player.RefObject, _Shop);
            }
            else
            {
                KTPlayerManager.ShowNotification(player.RefObject, "Cửa hàng bạn tìm không tồn tại");
                return;
            }
        }

        public static bool IsHaveEquipBody(Lua_Player player)
        {
            return player.RefObject.GetPlayEquipBody().IsHaveEquipBody();
        }

        public static void TokenTransClick(Lua_NPC npc, Lua_Player player)
        {
            RechageServiceManager.ClickRechage(npc.RefObject, player.RefObject);
        }

        public static void UpdateCosume(Lua_Player player,int Value)
        {
            Global.SaveConsumeLog(player.RefObject, Value, 1);
        }

        public static void BuyDiscount(Lua_NPC npc, Lua_Player player)
        {
            ShopSalePrestige.ClickNpcBuyRequest(npc.RefObject, player.RefObject);
        }

        /// <summary>
        /// Gửi thông báo về client
        /// </summary>
        /// <param name="MSG"></param>
        /// <param name="RefObject"></param>
        /// <param name="npc"></param>
        public static void SendMSG(string MSG, KPlayer RefObject, NPC npc)
        {
            KNPCDialog _NpcDialog = new KNPCDialog();

            _NpcDialog.Text = MSG;

            _NpcDialog.Show(npc, RefObject);
        }

        public static void SendItemMsg(string MSG, GoodsData data, KPlayer RefObject)
        {
            KItemDialog _NpcDialog = new KItemDialog();

            _NpcDialog.Text = MSG;

            _NpcDialog.Show(data, RefObject);
        }

        /// <summary>
        /// Ghi nhật ký ngày theo key
        /// </summary>
        /// <param name="player"></param>
        /// <param name="Key"></param>
        /// <returns></returns>
        public static int GetValueOfDailyRecore(Lua_Player player, int Key)
        {
            int VALUE = player.RefObject.GetValueOfDailyRecore(Key);
            return VALUE;
        }

        /// <summary>
        /// Getmoney
        /// </summary>
        /// <param name="player"></param>
        /// <param name="Key"></param>
        /// <returns></returns>
        public static int CheckMoney(Lua_Player player, int MoneyType)
        {
            int VALUE = KTGlobal.GetMoney(player.RefObject, (KiemThe.Entities.MoneyType)MoneyType);

            return VALUE;
        }

        /// <summary>
        /// Lấy ra tiền bang hội đang có
        /// </summary>
        /// <param name="player"></param>
        /// <param name="MoneyType"></param>
        /// <returns></returns>
        public static int CheckGuildStoreMoney(Lua_Player player)
        {
            if (player.RefObject.GuildID > 0)
            {
                MiniGuildInfo GetWinThisGame = GuildManager.getInstance()._GetInfoGuildByGuildID(player.RefObject.GuildID);
                if (GetWinThisGame != null)
                {
                    return GetWinThisGame.GuildMoney;
                }
                else
                {
                    return -1;
                }

            }
            else
            {
                return -1;
            }
        }

        public static void GuildChat(Lua_Player player,string MSG)
        {
            KTGlobal.SendGuildChat(player.RefObject.GuildID, MSG);
        }
        public static bool SetGuildMoneyStore(Lua_Player player, int MoneySet)
        {

            if (player.RefObject.GuildID > 0)
            {
                MiniGuildInfo GetWinThisGame = GuildManager.getInstance()._GetInfoGuildByGuildID(player.RefObject.GuildID);
                if (GetWinThisGame != null)
                {
                    GetWinThisGame.GuildMoney = MoneySet;
                    if (GuildManager.UpdateGuildResource(GetWinThisGame.GuildId, GUILD_RESOURCE.GUILD_MONEY, MoneySet + ""))
                    {
                        return true;
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

            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Hàm trừ tiền của người chơi
        /// </summary>
        /// <param name="player"></param>
        /// <param name="MoneyType"></param>
        /// <param name="SubNumbber"></param>
        /// <returns></returns>
        public static bool MinusMoney(Lua_Player player, int MoneyType, int SubNumbber)
        {
            if (MoneyType > (int)KiemThe.Entities.MoneyType.StoreMoney || MoneyType < (int)KiemThe.Entities.MoneyType.Bac)
            {
                return false;
            }
            else
            {
                SubRep _Submoney = KTGlobal.SubMoney(player.RefObject, SubNumbber, (KiemThe.Entities.MoneyType)MoneyType, "LUASCRIPT");

                return _Submoney.IsOK;
            }
        }

        /// <summary>
        /// Set trị Pk cho nhân vật
        /// </summary>
        /// <param name="player"></param>
        /// <param name="PkValue"></param>
        public static void SetPKValue(Lua_Player player, int PkValue)
        {
            player.RefObject.PKValue = PkValue;
        }

        /// <summary>
        /// Set giá trị ngày theo key
        /// </summary>
        /// <param name="player"></param>
        /// <param name="Key"></param>
        /// <param name="Value"></param>
        public static void SetValueOfDailyRecore(Lua_Player player, int Key, int Value)
        {
            player.RefObject.SetValueOfDailyRecore(Key, Value);
        }

        public static long GetBaseExp(Lua_Player player, int Level)
        {
            return KPlayerSetting.GetBaseExpLevel(Level);
        }

        public static int GetBCH(Lua_Player player, int Level)
        {
            if (Level == 1)
            {
                return player.RefObject.baijuwan;
            }
            else
            {
                return player.RefObject.baijuwanpro;
            }
        }

        public static void SetBCH(Lua_Player player, int Value, int Level)
        {
            if (Level == 1)
            {
                player.RefObject.baijuwan = player.RefObject.baijuwan + Value;
            }
            else
            {
                player.RefObject.baijuwanpro = player.RefObject.baijuwanpro + Value;
            }
        }

        /// <summary>
        /// Tăng điểm tiềm năng tương ứng
        /// </summary>
        /// <param name="player"></param>
        public static void UsingPotentialPoints(Lua_Player player)
        {
            int COUNT = ItemManager.GetItemCountInBag(player.RefObject, 8434);
            if (COUNT < 1)
            {
                KTPlayerManager.ShowMessageBox(player.RefObject, "Thông báo", "Không tìm thấy Tảy Tủy Kinh");
            }
            else
            {
                int UsingCount = player.RefObject.GetValueOfForeverRecore(ForeverRecord.UsingPotentialBook);
                if (UsingCount < 15)
                {
                    if (ItemManager.RemoveItemFromBag(player.RefObject, 8434, 1, -1, "UsingPotentialBook"))
                    {
                        if (UsingCount == -1)
                        {
                            UsingCount = 0;
                        }
                        player.RefObject.SetValueOfForeverRecore(ForeverRecord.UsingPotentialBook, UsingCount + 1);

                        int Before = player.RefObject.GetBonusRemainPotentialPoints();
                        player.RefObject.SetBonusRemainPotentialPoints(Before + 5);
                    }
                    else
                    {
                        KTPlayerManager.ShowMessageBox(player.RefObject, "Thông báo", "Không thể xóa vật phẩm");
                    }
                }
                else
                {
                    KTPlayerManager.ShowMessageBox(player.RefObject, "Thông báo", "Bạn đã sử dụng hết 15 lần");
                }
            }
        }

        /// <summary>
        /// Gọi tới việc phân giã vật phẩm
        /// </summary>
        /// <param name="player"></param>
        public static void CallBreakingItem(Lua_Player player)
        {
            ItemCraftingManager.BreakingItemRequest(player.RefObject);
        }



        /// <summary>
        /// Mở khóa trang bị
        /// </summary>
        /// <param name="player"></param>
        public static void UnlockItem(Lua_Player player)
        {
            ItemCraftingManager.UnlockItemRequest(player.RefObject);
        }

        /// <summary>
        /// Thêm điểm kỹ năng tương ứng cho nhân vật
        /// </summary>
        /// <param name="player"></param>
        /// <param name="nAdd"></param>
        public static void AddBonusSkillPoint(Lua_Player player, int nAdd)
        {
            int currentSkillPoint = player.RefObject.GetBonusSkillPoint();
            currentSkillPoint += nAdd;
            player.RefObject.SetBonusSkillPoint(currentSkillPoint);
        }

        /// <summary>
        /// Tăng điểm kỹ năng tương ứng
        /// </summary>
        /// <param name="player"></param>
        public static void UsingSkillBoundPoints(Lua_Player player)
        {
            int COUNT = ItemManager.GetItemCountInBag(player.RefObject, 8436);
            if (COUNT < 1)
            {
                KTPlayerManager.ShowMessageBox(player.RefObject, "Thông báo", "Không tìm thấy Võ Lâm Mật Tịch");
            }
            else
            {
                int UsingCount = player.RefObject.GetValueOfForeverRecore(ForeverRecord.UsingSkillBoundBook);
                if (UsingCount < 15)
                {
                    if (ItemManager.RemoveItemFromBag(player.RefObject, 8436, 1, -1, "UsingSkillBoundBook"))
                    {
                        if (UsingCount == -1)
                        {
                            UsingCount = 0;
                        }
                        player.RefObject.SetValueOfForeverRecore(ForeverRecord.UsingSkillBoundBook, UsingCount + 1);

                        int Before = player.RefObject.GetBonusSkillPoint();
                        player.RefObject.SetBonusSkillPoint(Before + 1);
                    }
                    else
                    {
                        KTPlayerManager.ShowMessageBox(player.RefObject, "Thông báo", "Không thể xóa vật phẩm");
                    }
                }
                else
                {
                    KTPlayerManager.ShowMessageBox(player.RefObject, "Thông báo", "Bạn đã sử dụng hết 15 lần");
                }
            }
        }

        public static int GetSeries(Lua_Player player)
        {
            return (int)player.RefObject.m_Series;
        }

        public static int GetMapCode(Lua_Player player)
        {
            if ((int)player.RefObject.CopyMapID != -1)
            {
                return (int)player.RefObject.MapCode;
            }
            else
            {
                return (int)player.RefObject.CopyMapID;
            }
        }

        /// <summary>
        /// Mở rương tương ứng
        /// </summary>
        /// <param name="player"></param>
        /// <param name="boxDbID"></param>
        /// <returns></returns>
        public static bool OpenRandomBox(Lua_Player player, int boxDbID)
        {
            return ItemRandomBox.OpenBoxRandom(player.RefObject, boxDbID);
        }

        public static int GetValueWeekRecorey(Lua_Player player, int Key)
        {
            return player.RefObject.GetValueOfWeekRecore(Key);
        }

        public static void SetValueOfWeekRecore(Lua_Player player, int Key, int Value)
        {
            player.RefObject.SetValueOfWeekRecore(Key, Value);
        }

        /// <summary>
        /// Lấy ra đánh dấu vĩnh viễn của nhân vật
        /// </summary>
        /// <param name="player"></param>
        /// <param name="Key"></param>
        /// <returns></returns>
        public static int GetValueForeverRecore(Lua_Player player, int Key)
        {
            return player.RefObject.GetValueOfForeverRecore((ForeverRecord)Key);
        }

        /// <summary>
        /// set giá trị cho đánh dấu vĩnh viễn của nhân vật
        /// </summary>
        /// <param name="player"></param>
        /// <param name="Key"></param>
        /// <param name="Value"></param>
        public static void SetValueOfForeverRecore(Lua_Player player, int Key, int Value)
        {
            player.RefObject.SetValueOfForeverRecore((ForeverRecord)Key, Value);
        }

        /// <summary>
        /// Thêm danh vọng tương ứng
        /// </summary>
        /// <param name="player"></param>
        /// <param name="key"></param>
        /// <param name="point"></param>
        public static void AddRetupeValue(Lua_Player player, int key, int point)
        {
            KTGlobal.AddRepute(player.RefObject, key, point);
        }

        /// <summary>
        /// Trả về danh vọng tương ứng
        /// </summary>
        /// <param name="player"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static int GetRepute(Lua_Player player, int key)
        {
            ReputeInfo reputeInfo = player.RefObject.GetRepute().Where(x => x.DBID == key).FirstOrDefault();
            if (reputeInfo != null)
            {
                return reputeInfo.Level;
            }
            return -1;
        }

        /// <summary>
        /// Add tiền từ lua
        /// </summary>
        /// <param name="player"></param>
        /// <param name="MoneyAdd"></param>
        /// <param name="_MoneyType"></param>
        /// <returns></returns>
        public static bool AddMoney(Lua_Player player, int MoneyAdd, int _MoneyType)
        {
            MoneyType _Money = (MoneyType)_MoneyType;

            SubRep _SUB = KTGlobal.AddMoney(player.RefObject, MoneyAdd, _Money);
            return _SUB.IsOK;
        }

        /// <summary>
        /// Kiểm tra túi của người chơi có đủ khoảng trống tương ứng không
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public static bool HasFreeBagSpaces(Lua_Player player, int number)
        {
            return KTGlobal.IsHaveSpace(number, player.RefObject);
        }

        /// <summary>
        /// Trả về số ô trống trong túi cần để lấy vật phẩm
        /// </summary>
        /// <param name="itemID"></param>
        /// <param name="number"></param>
        /// <returns></returns>
        public static int GetTotalSpacesNeedToTakeItem(int itemID, int number)
        {
            return KTGlobal.GetTotalSpacesNeedToTakeItem(itemID, number);
        }

        /// <summary>
        /// Add vật phẩm từ lua
        /// </summary>
        /// <param name="player"></param>
        /// <param name="ItemID"></param>
        /// <param name="Number"></param>
        /// <param name="LockStatus"></param>
        /// <param name="enhanceLevel"></param>
        /// <returns></returns>
        public static bool AddItemLua(Lua_Player player, int ItemID, int Number, int Series, int LockStatus, int enhanceLevel = 0, int ExpriesTime = -1)
        {
            int SpaceNeed = ItemManager.TotalSpaceNeed(ItemID, Number);

            if (!KTGlobal.IsHaveSpace(SpaceNeed, player.RefObject))
            {
                KTPlayerManager.ShowNotification(player.RefObject, "Túi đồ không đủ chỗ");
                return false;
            }
            else
            {
                string TimeUsing = ItemManager.ConstGoodsEndTime;
                if (ExpriesTime != -1)
                {
                    DateTime dt = DateTime.Now.AddMinutes(ExpriesTime);

                    // "1900-01-01 12:00:00";
                    TimeUsing = dt.ToString("yyyy-MM-dd HH:mm:ss");
                }

                if (!ItemManager.CreateItem(Global._TCPManager.TcpOutPacketPool, player.RefObject, ItemID, Number, 0, "LUAITEMADD", false, LockStatus, false, TimeUsing, "", Series, "", enhanceLevel, 0, true))
                {
                    KTPlayerManager.ShowNotification(player.RefObject, "Có lỗi khi add vật phẩm");

                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

        /// <summary>
        /// Add Exp cho người chơi
        /// </summary>
        /// <param name="player"></param>
        /// <param name="Exp"></param>
        public static void AddRoleExp(Lua_Player player, int Exp)
        {
            KTPlayerManager.AddExp(player.RefObject, Exp);
        }

        public static bool IsHaveFamily(Lua_Player player)
        {
            if (player != null)
            {
                if (player.RefObject.FamilyID > 0)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        ///  Tìm một vật phẩm từ TEMPALTE
        /// </summary>
        /// <param name="Genre"></param>
        /// <param name="DetailType"></param>
        /// <param name="ParticularType"></param>
        /// <param name="Level"></param>
        /// <returns></returns>
        public static int FindItemID(int Genre, int DetailType, int ParticularType, int Level)
        {
            var find = ItemManager.TotalItem.Where(x => x.Genre == Genre && x.DetailType == DetailType && x.ParticularType == ParticularType && x.Level == Level).FirstOrDefault();
            if (find != null)
            {
                return find.ItemID;
            }
            else
            {
                return -1;
            }
        }

        /// <summary>
        /// aDD HOẠT LỰC CHO NHÂN VẬT
        /// </summary>
        /// <param name="player"></param>
        /// <param name="Point"></param>
        public static void AddMakePoint(Lua_Player player, int Point)
        {
            player.RefObject.ChangeCurMakePoint(Point);
        }

        /// <summary>
        ///aDD TINH LỰC CHO NHÂN VẬT
        /// </summary>
        /// <param name="player"></param>
        /// <param name="Point"></param>
        public static void AddCurGatherPoint(Lua_Player player, int Point)
        {
            player.RefObject.ChangeCurGatherPoint(Point);
        }

        /// <summary>
        /// Đếm số lượng vật phẩm theo ITEMID
        /// </summary>
        /// <param name="player"></param>
        /// <param name="ItemID"></param>
        /// <returns></returns>
        public static int CountItemInBag(Lua_Player player, int ItemID)
        {
            return ItemManager.GetItemCountInBag(player.RefObject, ItemID);
        }

        /// <summary>
        /// Xóa vật phẩm từ LUA
        /// </summary>
        /// <param name="player"></param>
        /// <param name="ItemDbID"></param>
        /// <returns></returns>
        public static bool RemoveItem(Lua_Player player, int ItemDbID)
        {
            GoodsData gd = player.RefObject.GoodsData.Find(ItemDbID);
            if (gd == null)
            {
                return false;
            }

            if (ItemManager.RemoveItemByCount(player.RefObject, gd, 1, "LUASCRIPT"))
            {
                ProcessTask.Process(Global._TCPManager.MySocketListener, Global._TCPManager.TcpOutPacketPool, player.RefObject, -1, -1, gd.GoodsID, TaskTypes.UseSomething);

                return true;
            }
            else
            {
                return false;
            }
        }

        public static void RepairAllItem(Lua_Player player)
        {
            double TotalValueNeedFix = 0;

            foreach (KeyValuePair<KE_EQUIP_POSITION, KItem> entry in player.RefObject._KPlayerEquipBody.EquipBody)
            {
                if (entry.Value._GoodDatas.Strong < entry.Value.GetDurability())
                {
                    double nTypeRate = entry.Value.GetDurability();

                    double nRate = 1.0 * nTypeRate / entry.Value.GetDurability();

                    double nFinalValue = Math.Max(entry.Value._ItemData.ItemValue, ItemEnhance.ALL_EQUIP_MIN_VALUE * nRate / ItemEnhance.EQUIP_TOTAL_RATE);

                    double nCoinCostPerDur = (nFinalValue * 0.002);

                    TotalValueNeedFix += (entry.Value.GetDurability() - entry.Value._GoodDatas.Strong) * nCoinCostPerDur;
                }
            }

            if (TotalValueNeedFix > 0)
            {
                Action _OK = new Action(() =>
                {
                    if (KTGlobal.SubMoney(player.RefObject, (int)TotalValueNeedFix, MoneyType.BacKhoa, "FIXITEM").IsOK)
                    {
                        foreach (KeyValuePair<KE_EQUIP_POSITION, KItem> entry in player.RefObject._KPlayerEquipBody.EquipBody)
                        {
                            if (entry.Value._GoodDatas.Strong < entry.Value.GetDurability())
                            {
                                Dictionary<UPDATEITEM, object> TotalUpdate = new Dictionary<UPDATEITEM, object>();

                                TotalUpdate.Add(UPDATEITEM.ROLEID, player.RefObject.RoleID);
                                TotalUpdate.Add(UPDATEITEM.ITEMDBID, entry.Value._GoodDatas.Id);
                                TotalUpdate.Add(UPDATEITEM.STRONG, entry.Value.GetDurability());

                                player.RefObject.GoodsData.Update(entry.Value._GoodDatas, TotalUpdate, false);
                            }
                        }
                    }
                    else
                    {
                        KTPlayerManager.ShowNotification(player.RefObject, "Không đủ bạc khóa để sửa đồ");
                    }
                });

                KTPlayerManager.ShowMessageBox(player.RefObject, "Sửa đồ", "Cần :" + (int)TotalValueNeedFix + " bạc khóa để sửa đồ?Ngươi có chắc chắn muốn sửa không", _OK);
            }
            else
            {
                KTPlayerManager.ShowNotification(player.RefObject, "Không có trang bị nào cần sửa");
            }
        }

        /// <summary>
        /// Sự kiện Click vào NPC sự kiện
        /// </summary>
        /// <param name="npc"></param>
        /// <param name="player"></param>
        public static void SpecialEventClick(Lua_NPC npc, Lua_Player player)
        {
            SpecialEvent_Logic.BuildNPCDialog(npc.RefObject, player.RefObject);
        }

        /// <summary>
        ///  Hàm sử dụng kim tê
        /// </summary>
        public static void UsingJINXI(Lua_Player player, int ItemDbID)
        {
            GoodsData Good = player.RefObject.GoodsData.Find(ItemDbID);

            if (Good != null)
            {
                ItemData _Item = ItemManager.GetItemTemplate(Good.GoodsID);
                if (_Item != null)
                {
                    if (ItemManager.KD_ISJINXI(Good.GoodsID))
                    {
                        int Quanity = 0;
                        if (Good.OtherParams.TryGetValue(ItemPramenter.Pram_1, out string NumberCanRepair))
                        {
                            Quanity = Int32.Parse(NumberCanRepair);
                        }

                        KItemDialog _NpcDialog = new KItemDialog();

                        Dictionary<int, string> Selections = new Dictionary<int, string>();

                        string Text = "Sử dụng Kim Tê để sửa trang bị\nĐộ bền của kim tê hao hụt khi bạn sử dụng để sửa trạng bị\nĐộ bền hiện tại của kim tê là :" + KTGlobal.CreateStringByColor(Quanity + "", ColorType.Yellow) + "\nChọn vật phẩm muốn sửa :";

                        foreach (KeyValuePair<KE_EQUIP_POSITION, KItem> entry in player.RefObject._KPlayerEquipBody.EquipBody)
                        {
                            Selections.Add(entry.Value._GoodDatas.Id, entry.Value._ItemData.Name + "| Độ bền :" + entry.Value._GoodDatas.Strong);
                        }

                        Action<TaskCallBack> ActionWork = (x) => DoActionSelect(player, ItemDbID, x);

                        _NpcDialog.OnSelect = ActionWork;

                        _NpcDialog.Selections = Selections;

                        _NpcDialog.Text = Text;

                        _NpcDialog.ShowDialog(Good, player.RefObject);
                    }
                }
            }
        }

        /// <summary>
        ///  Hàm sử dụng càn khôn phù
        /// </summary>
        public static void UsingQianKunFu(Lua_Player player, int ItemDbID)
        {
            GoodsData Good = player.RefObject.GoodsData.Find(ItemDbID);

            if (Good != null)
            {
                ItemData _Item = ItemManager.GetItemTemplate(Good.GoodsID);
                if (_Item != null)
                {
                    // Nếu đây đúng là càn khôn phù
                    if (ItemManager.IsQianKunFu(Good.GoodsID))
                    {
                        // Lấy ra số lượt có thể sử dụng
                        int Quanity = 0;
                        if (Good.OtherParams.TryGetValue(ItemPramenter.Pram_1, out string NumberCanRepair))
                        {
                            Quanity = Int32.Parse(NumberCanRepair);
                        }

                        KItemDialog _NpcDialog = new KItemDialog();

                        Dictionary<int, string> Selections = new Dictionary<int, string>();

                        string Text = "Sử dụng càn khôn phù có thể phù tới vị trí của đồng đội trong giây lát\nSố lần sử dụng còn lại của càn khôn phù :" + KTGlobal.CreateStringByColor(Quanity + "", ColorType.Yellow) + "\nChọn đồng đội muốn dịch chuyển tới :";

                        if (player.RefObject.TeamID != -1)
                        {
                            List<KPlayer> TotalMember = player.RefObject.Teammates;

                            foreach (KPlayer Member in TotalMember)
                            {
                                if (Member.RoleID != player.RefObject.RoleID)
                                {
                                    GameMap gameMap = KTMapManager.Find(Member.MapCode);
                                    UnityEngine.Vector2 gridPos = KTGlobal.WorldPositionToGridPosition(gameMap, new UnityEngine.Vector2(Member.PosX, Member.PosY));
                                    Selections.Add(Member.RoleID, string.Format("{0} - <color=green>{1}</color> <color=#0afffb>({2}, {3})", Member.RoleName, gameMap.MapName, (int)gridPos.x, (int)gridPos.y));
                                }
                            }
                        }
                        else
                        {
                            Text += "\n\nBạn không có tổ đội không thể sử dụng vật phẩm này";
                        }

                        Action<TaskCallBack> ActionWork = (x) => DoActionKunFu(player, ItemDbID, x);

                        _NpcDialog.OnSelect = ActionWork;

                        _NpcDialog.Selections = Selections;

                        _NpcDialog.Text = Text;
                        _NpcDialog.ShowDialog(Good, player.RefObject);
                    }
                }
            }
        }

        private static void DoActionKunFu(Lua_Player player, int itemDbID, TaskCallBack x)
        {
            // Lấy ra kim tê
            GoodsData Good = player.RefObject.GoodsData.Find(itemDbID);

            if (Good != null)
            {
                if (ItemManager.IsQianKunFu(Good.GoodsID))
                {
                    int Quanity = 0;

                    if (Good.OtherParams.TryGetValue(ItemPramenter.Pram_1, out string NumberCanRepair))
                    {
                        Quanity = Int32.Parse(NumberCanRepair);
                    }

                    int RoleID = x.SelectID;

                    // tỉma người chơi
                    KPlayer client = KTPlayerManager.Find(RoleID);
                    if (client != null)
                    {
                        if (FactionBattleManager.IsInFactionBattle(client) || BaiHuTang.IsInBaiHuTang(client) || Battel_SonJin_Manager.IsInBattle(client) || TeamBattle.IsInTeamBattleMap(client) || client.CopyMapID > 0)
                        {
                            KTPlayerManager.ShowNotification(player.RefObject, "Người chơi đang ở trong bản đồ sự kiện không thể dịch chuyển tới");
                        }
                        else
                        {
                            //Đóng khung
                            KT_TCPHandler.CloseDialog(client);

                            Quanity = Quanity - 1;

                            if (Quanity <= 0)
                            {
                                ItemManager.AbadonItem(itemDbID, player.RefObject, false, "KIMTEUSING");
                            }

                            //Updat lại số lượt sử dụng càn khôn phù
                            Dictionary<UPDATEITEM, object> TotalUpdate = new Dictionary<UPDATEITEM, object>();

                            TotalUpdate.Add(UPDATEITEM.ROLEID, player.RefObject.RoleID);
                            TotalUpdate.Add(UPDATEITEM.ITEMDBID, Good.Id);

                            Dictionary<ItemPramenter, string> OtherParams = new Dictionary<ItemPramenter, string>();

                            OtherParams.Add(ItemPramenter.Pram_1, Quanity + "");

                            TotalUpdate.Add(UPDATEITEM.OTHER_PRAM, OtherParams);

                            player.RefObject.GoodsData.Update(Good, TotalUpdate, true);

                            KTPlayerManager.ChangeMap(player.RefObject, client.MapCode, client.PosX, client.PosY);
                        }
                    }
                    else
                    {
                        KTPlayerManager.ShowNotification(player.RefObject, "Người chơi này đã offline bạn không thể dịch chuyển tới");
                    }
                }
            }
        }

        private static void DoActionSelect(Lua_Player player, int itemDbID, TaskCallBack x)
        {
            // Lấy ra kim tê
            GoodsData Good = player.RefObject.GoodsData.Find(itemDbID);

            if (Good != null)
            {
                if (ItemManager.KD_ISJINXI(Good.GoodsID))
                {
                    int Quanity = 0;

                    if (Good.OtherParams.TryGetValue(ItemPramenter.Pram_1, out string NumberCanRepair))
                    {
                        Quanity = Int32.Parse(NumberCanRepair);
                    }

                    GoodsData ItemRepair = player.RefObject.GoodsData.Find(x.SelectID);
                    if (ItemRepair != null)
                    {
                        if (ItemRepair.Strong == 100)
                        {
                            SendItemMsg("Vật phẩm đã đạt độ bền tốn đa không cần sưa chữa!", Good, player.RefObject);
                            return;
                        }

                        int Dulation = ItemRepair.Strong;

                        int DualtionLoss = 100 - Dulation;

                        // Nếu kim tê còn giá trị
                        if (Quanity > DualtionLoss)
                        {
                            ItemRepair.Strong = 100;

                            Dictionary<UPDATEITEM, object> TotalUpdate = new Dictionary<UPDATEITEM, object>();

                            TotalUpdate.Add(UPDATEITEM.ROLEID, player.RefObject.RoleID);
                            TotalUpdate.Add(UPDATEITEM.ITEMDBID, ItemRepair.Id);
                            TotalUpdate.Add(UPDATEITEM.STRONG, ItemRepair.Strong);

                            player.RefObject.GoodsData.Update(ItemRepair, TotalUpdate, false);

                            Quanity = Quanity - DualtionLoss;
                            //Update lại giá trị cho kim tê
                            TotalUpdate = new Dictionary<UPDATEITEM, object>();

                            TotalUpdate.Add(UPDATEITEM.ROLEID, player.RefObject.RoleID);
                            TotalUpdate.Add(UPDATEITEM.ITEMDBID, Good.Id);

                            Dictionary<ItemPramenter, string> OtherParams = new Dictionary<ItemPramenter, string>();

                            OtherParams.Add(ItemPramenter.Pram_1, Quanity + "");

                            TotalUpdate.Add(UPDATEITEM.OTHER_PRAM, OtherParams);

                            player.RefObject.GoodsData.Update(Good, TotalUpdate);

                            SendItemMsg("Sửa trang bị thành công !", Good, player.RefObject);
                        }
                        else
                        {
                            ItemRepair.Strong = DualtionLoss - Quanity;

                            Dictionary<UPDATEITEM, object> TotalUpdate = new Dictionary<UPDATEITEM, object>();

                            TotalUpdate.Add(UPDATEITEM.ROLEID, player.RefObject.RoleID);
                            TotalUpdate.Add(UPDATEITEM.ITEMDBID, ItemRepair.Id);
                            TotalUpdate.Add(UPDATEITEM.STRONG, ItemRepair.Strong);

                            player.RefObject.GoodsData.Update(ItemRepair, TotalUpdate, false);

                            ItemManager.AbadonItem(itemDbID, player.RefObject, false, "KIMTEUSING");

                            SendItemMsg("Sửa trang bị thành công,Kim tê đã biến mất", Good, player.RefObject);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// lấy ra tên có thể jouin
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public static string GetBattleNameCanJoin(Lua_Player player)
        {
            return Battel_SonJin_Manager.GetBatteNameCanJoin(player.RefObject.m_Level);
        }

        /// <summary>
        /// </summary>
        /// <param name="npc"></param>
        /// <param name="player"></param>
        /// <param name="Camp"></param>
        /// <param name="BattleID"></param>
        public static void JoinSongJinBattle(Lua_NPC npc, Lua_Player player, string Camp, int BattleID)
        {
            if (player == null || player.RefObject == null)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Lua error on NPCDialog:Show, Player is NULL."));
                return;
            }

            if (npc == null || npc.RefObject == null)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Lua error on NPCDialog:Show, NPC is NULL."));
                return;
            }

            UnityEngine.Vector2 _SelfPos = new UnityEngine.Vector2((int)player.RefObject.CurrentPos.X, (int)player.RefObject.CurrentPos.Y);

            UnityEngine.Vector2 NpcPos = new UnityEngine.Vector2((int)npc.RefObject.CurrentPos.X, (int)npc.RefObject.CurrentPos.Y);

            float Dis = UnityEngine.Vector2.Distance(_SelfPos, NpcPos);

            if (Dis > 100)
            {
                KTPlayerManager.ShowNotification(player.RefObject, "Khoảng cách quá xa");
                return;
            }

            int CampInt = 10;

            if (Camp == "KIM")
            {
                CampInt = 20;
            }

            int Register = Battel_SonJin_Manager.BattleRegister(player.RefObject, CampInt, BattleID);

            if (Register == -100)
            {
                SendMSG("Số lượng báo danh vào phe Tống đang quá đông,Xin hãy quay lại sau hoặc báo danh sang bên Kim", player.RefObject, npc.RefObject);
            }
            else if (Register == -200)
            {
                SendMSG("Số lượng báo danh vào phe Kim đang quá đông,Xin hãy quay lại sau hoặc báo danh sang bên Tống", player.RefObject, npc.RefObject);
            }
            else if (Register == -2)
            {
                SendMSG("Người đã đăng ký rồi không thể đăng ký lại", player.RefObject, npc.RefObject);
            }
            else if (Register == -3)
            {
                SendMSG("Chiến trường hiện chưa mở", player.RefObject, npc.RefObject);
            }
            else if (Register == -4)
            {
                SendMSG("Không phải thời gian báo danh vui lòng quay lại sau", player.RefObject, npc.RefObject);
            }
            else if (Register == -1000)
            {
                SendMSG("Cấp độ của bạn không phù hợp với chiến trường này", player.RefObject, npc.RefObject);
            }
        }

        /// <summary>
        /// Tham gia thi đấu môn phái
        /// </summary>
        /// <param name="npc"></param>
        /// <param name="player"></param>
        /// <param name="Camp"></param>
        /// <param name="BattleID"></param>
        public static void JoinFactionBattle(Lua_NPC npc, Lua_Player player)
        {
            if (player == null || player.RefObject == null)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Lua error on NPCDialog:Show, Player is NULL."));
                return;
            }

            if (npc == null || npc.RefObject == null)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Lua error on NPCDialog:Show, NPC is NULL."));
                return;
            }

            UnityEngine.Vector2 _SelfPos = new UnityEngine.Vector2((int)player.RefObject.CurrentPos.X, (int)player.RefObject.CurrentPos.Y);

            UnityEngine.Vector2 NpcPos = new UnityEngine.Vector2((int)npc.RefObject.CurrentPos.X, (int)npc.RefObject.CurrentPos.Y);

            float Dis = UnityEngine.Vector2.Distance(_SelfPos, NpcPos);

            if (Dis > 100)
            {
                KTPlayerManager.ShowNotification(player.RefObject, "Khoảng cách quá xa");
                return;
            }

            int CampInt = 10;

            int Register = FactionBattleManager.FactionBattleJoin(player.RefObject);

            if (Register == -1)
            {
                SendMSG("Hiện tại không có trận thi đấu môn phái nào", player.RefObject, npc.RefObject);
            }
            else if (Register == -2)
            {
                SendMSG("Thi đấu môn phái đã bắt đầu lúc 20:00h，hiện tại không thể tiếp nhận đăng ký", player.RefObject, npc.RefObject);
            }
            else if (Register == -3)
            {
                SendMSG("Bạn không phải là đệ tử của môn phái này，không thể tham gia thi đấu!", player.RefObject, npc.RefObject);
            }
            else if (Register == -4)
            {
                SendMSG("Cấp độ của bạn không đủ không thể tham gia thi đấu môn phái", player.RefObject, npc.RefObject);
            }
            else if (Register == -5)
            {
                SendMSG("Số người đăng ký đã đến mức giới hạn 400 người，không thể tiếp nhận đăng ký，mời bạn tham gia các hoạt động khác tại đây", player.RefObject, npc.RefObject);
            }
            else if (Register == -6)
            {
                SendMSG("Ngươi phải vào 1 môn phái", player.RefObject, npc.RefObject);
            }
            else if (Register == -40)
            {
                SendMSG("Vui lòng giải tán tổ đội trước khi giam gia", player.RefObject, npc.RefObject);
            }
        }

        /// <summary>
        /// Mở thương khố
        /// </summary>
        /// <param name="npc"></param>
        /// <param name="player"></param>
        public static void PortableGoods(Lua_NPC npc, Lua_Player player)
        {
            UnityEngine.Vector2 _SelfPos = new UnityEngine.Vector2((int)player.RefObject.CurrentPos.X, (int)player.RefObject.CurrentPos.Y);

            UnityEngine.Vector2 NpcPos = new UnityEngine.Vector2((int)npc.RefObject.CurrentPos.X, (int)npc.RefObject.CurrentPos.Y);

            float Dis = UnityEngine.Vector2.Distance(_SelfPos, NpcPos);

            if (Dis > 100)
            {
                KTPlayerManager.ShowNotification(player.RefObject, "Khoảng cách quá xa");
                return;
            }

            int roleID = player.RefObject.RoleID;
            int site = 1;

            byte[] sendBytesCmd = new UTF8Encoding().GetBytes(string.Format("{0}:{1}", player.RefObject.RoleID, 1));
            byte[] bytesData = null;

            List<GoodsData> TotalGoodData = player.RefObject.GoodsData.FindAll(x => x.Site == 1);

            player.RefObject.SendPacket<List<GoodsData>>((int)TCPGameServerCmds.CMD_GETGOODSLISTBYSITE, TotalGoodData);

            // Loại bỏ luồng
            //Thread _OpenThread = new Thread(() =>
            //{
            //    TCPProcessCmdResults result = Global.ReadDataFromDb((int)TCPGameServerCmds.CMD_GETGOODSLISTBYSITE, sendBytesCmd, sendBytesCmd.Length, out bytesData, player.RefObject.ServerId);
            //    //{
            //    //    LogManager.WriteLog(LogTypes.Error, string.Format("Không kết nối được với DBServer, CMD={0}", (int)TCPGameServerCmds.CMD_SPR_COMPTASK));

            //    //    KT_TCPHandler.CloseDialog(client);

            //    //    PlayerManager.ShowNotification(client, "Không kết nối được với DBServer");

            //    //    return;
            //    //}

            //    if (TCPProcessCmdResults.RESULT_FAILED != result)
            //    {
            //        //Get đồ từ DB ra trả về client
            //        List<GoodsData> goodsDataList = DataHelper.BytesToObject<List<GoodsData>>(bytesData, 6, bytesData.Length - 6);

            //        player.RefObject.SendPacket<List<GoodsData>>((int)TCPGameServerCmds.CMD_GETGOODSLISTBYSITE, goodsDataList);
            //    }
            //});
            //_OpenThread.IsBackground = false;
            //_OpenThread.Start();

            //TCPProcessCmdResults result = Global.TransferRequestToDBServer(Global._TCPManager, player.RefObject.ClientSocket, tcpClientPool, tcpRandKey, pool, nID, data, count, out tcpOutPacket, client.ServerId);
        }

        /// <summary>
        /// Kick người chơi tương ứng ra khỏi Game
        /// </summary>
        /// <param name="player"></param>
        public static void KickOut(Lua_Player player)
        {
            KTPlayerManager.KickOut(player.RefObject);
        }

        /// <summary>
        /// Cường hóa trang bị
        /// </summary>
        /// <param name="player"></param>
        /// <param name="item"></param>
        /// <param name="enhanceLevel"></param>
        public static void EquipEnhance(Lua_Player player, Lua_Item item, int enhanceLevel)
        {
            ItemManager.SetEquipForgeLevel(item.RefObject, player.RefObject, enhanceLevel);
        }

        /// <summary>
        /// Giải tán bang hội
        /// </summary>
        /// <param name="player"></param>
        public static void RetainGuild(Lua_Player player)
        {
            /// Nếu không có bang hội
            if (player.RefObject.GuildID <= 0)
            {
                return;
            }
            /// Nếu không phải bang chủ
            else if (player.RefObject.GuildRank != (int)GuildRank.Master)
            {
                return;
            }

            /// Hiện bảng xác nhận
            KTPlayerManager.ShowMessageBox(player.RefObject, "Xác nhận", "Xác nhận giải tán bang hội hiện tại không?", () =>
            {
                //KT_TCPHandler.DesotryGuild(player.RefObject.RoleID, player.RefObject.GuildID);
            }, true);
        }

        /// <summary>
        /// Giải tán gia tộc
        /// </summary>
        /// <param name="player"></param>
        public static void RetainFamily(Lua_Player player)
        {
            /// Nếu không có tộc
            if (player.RefObject.FamilyID <= 0)
            {
                return;
            }
            /// Nếu không phải tộc trưởng
            else if (player.RefObject.FamilyRank != (int)FamilyRank.Master)
            {
                return;
            }
            /// Nếu có bang hội
            else if (player.RefObject.GuildID > 0)
            {
                return;
            }

            /// Hiện bảng xác nhận
            KTPlayerManager.ShowMessageBox(player.RefObject, "Xác nhận", "Xác nhận giải tán gia tộc hiện tại không?", () =>
            {
                //KT_TCPHandler.DestroyFamily(player.RefObject.RoleID, player.RefObject.FamilyID);
            }, true);
        }

        public static void AddGuildExp(Lua_Player player, int ExpNum)
        {
            if (player.RefObject.GuildID > 0)
            {
                MiniGuildInfo Find = GuildManager.getInstance()._GetInfoGuildByGuildID(player.RefObject.GuildID);

                if (Find != null)
                {
                    Find.GuildExp += ExpNum;

                    GuildManager.UpdateGuildResource(player.RefObject.GuildID, GUILD_RESOURCE.EXP, Find.GuildExp + "");
                }
                else
                {
                    KTPlayerManager.ShowNotification(player.RefObject, "Không tìm thấy vật phẩm này");
                }
            }
            else
            {
                KTPlayerManager.ShowNotification(player.RefObject, "Phải có bang hội mới có thể sử dụng vật phẩm này");
            }
        }

        /// <summary>
        /// Xóa vật phẩm có ID với số lượng tương ứng khỏi túi đồ của người chơi
        /// </summary>
        /// <param name="player"></param>
        /// <param name="itemID"></param>
        /// <param name="count"></param>
        public static bool RemoveItem(Lua_Player player, int itemID, int count)
        {
            return ItemManager.RemoveItemFromBag(player.RefObject, itemID, count, -1, "LUASCRIPTCALL");
        }

        /// <summary>
        /// Sử dụng Tu Luyện Châu
        /// </summary>
        /// <param name="player"></param>
        /// <param name="hour10"></param>
        /// <returns></returns>
        public static bool UseXiuLianZhu(Lua_Player player, int hour10)
        {
            return ItemXiuLianZhuManager.UseXiuLianZhu(player.RefObject, hour10);
        }

        /// <summary>
        /// Thiết lập NPC dã luyện đại sư được Click lần trước
        /// </summary>
        /// <param name="player"></param>
        /// <param name="npc"></param>
        public static void SetLastEquipMasterNPC(Lua_Player player, Lua_NPC npc)
        {
            player.RefObject.LastEquipMasterNPC = npc.RefObject;
        }

        /// <summary>
        /// Thực hiện đổi tên
        /// </summary>
        /// <param name="player"></param>
        /// <param name="newName"></param>
        /// <param name="onSuccess"></param>
        public static void ProcessChangeName(Lua_Player player, string newName, Closure onSuccess)
        {
            KTChangeNameManager.Instance.ProcessChangeName(player.RefObject, newName, (_oldName, _newName) =>
            {
                object[] parameters = new object[]
                {
                    _oldName, _newName
                };
                KTLuaScript.Instance.ExecuteFunctionAsync("Player.ProcessChangeName", onSuccess, parameters, null);
            });
        }

        /// <summary>
        /// Mở bảng nhập Captcha
        /// </summary>
        /// <param name="player"></param>
        /// <param name="onSuccess"></param>
        /// <param name="delayTicks"></param>
        /// <returns></returns>
        public static bool OpenCaptcha(Lua_Player player, Closure onSuccess, int delayTicks = 30000)
        {
            return KTPlayerManager.OpenCaptcha(player.RefObject, (isCorrect) =>
            {
                object[] parameters = new object[]
                {
                    isCorrect
                };
                KTLuaScript.Instance.ExecuteFunctionAsync("Player.OpenCaptcha", onSuccess, parameters, null);
            }, delayTicks);
        }

        /// <summary>
        /// Trả về số lượng Pet tối đa có thể mang theo
        /// </summary>
        /// <returns></returns>
        public static int GetMaxPetCanTake()
        {
            return KPet.Config.MaxCanTake;
        }

        public static void ShowDebug(Lua_Player player, string MSG)
        {
            KTPlayerManager.ShowNotification(player.RefObject, MSG);
        }

        /// <summary>
        /// Tạo pet cho người chơi tương ứng
        /// </summary>
        /// <param name="player"></param>
        /// <param name="petID"></param>
        /// <returns></returns>
        public static int CreatePet(Lua_Player player, int petID)
        {
            PetData pd = KTPlayerManager.CreatePet(player.RefObject, petID);
            /// Toác
            if (pd == null)
            {
                return -1;
            }
            return pd.ID;
        }
    }
}