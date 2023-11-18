using GameServer.KiemThe.Core;
using GameServer.KiemThe.Core.Activity.LuckyCircle;
using GameServer.KiemThe.Core.Item;
using GameServer.KiemThe.Entities;
using GameServer.KiemThe.Logic;
using GameServer.KiemThe.Logic.Manager;
using GameServer.Logic;
using Server.Tools;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GameServer.KiemThe.GameEvents.SpecialEvent
{
    /// <summary>
    /// Logic sự kiện đặc biệt
    /// </summary>
    public static class SpecialEvent_Logic
    {
        #region NPC
        /// <summary>
        /// Xây hội thoại NPC
        /// </summary>
        /// <param name="npc"></param>
        /// <param name="player"></param>
        public static void BuildNPCDialog(NPC npc, KPlayer player)
        {
            KNPCDialog dialog = new KNPCDialog();
            dialog.Text = string.Format("Xin chào {0}! Ta là sứ giả hoạt động, có rất nhiều sự kiện ngày lễ đặc biệt được tổ chức ở chỗ ta, các hạ muốn tìm hiểu loại nào?", player.RoleName);
            //dialog.Selections[-1000000] = "Mở Vòng quay may mắn";
            /// Duyệt danh sách sự kiện khả dụng
            foreach (SpecialEvent.EventInfo eventInfo in SpecialEvent.Events.Values)
            {
                dialog.Selections[-eventInfo.ID] = eventInfo.Name;
            }
            dialog.Selections[-999] = "Kết thúc đối thoại";
            /// Sự kiện Select
            dialog.OnSelect = (x) =>
            {
                SpecialEvent_Logic.OnNPCSelect(npc, player, x.SelectID);
            };
            /// Hiện NPCDialog
            KTNPCDialogManager.AddNPCDialog(dialog);
            dialog.Show(npc, player);
        }

        /// <summary>
        /// Sự kiện người chơi Click vào NPCDialog
        /// </summary>
        /// <param name="npc"></param>
        /// <param name="player"></param>
        /// <param name="selectionID"></param>
        private static void OnNPCSelect(NPC npc, KPlayer player, int selectionID)
        {
            /// Nếu là đóng khung
            if (selectionID == -999)
            {
                /// Đóng Dialog
                KT_TCPHandler.CloseDialog(player);
            }
            /// Nếu là mở vòng quay may mắn
            else if (selectionID == -1000000)
            {
                /// Mở khung Vòng quay may mắn
                KTLuckyCircleManager.OpenCircle(player);
                /// Đóng Dialog
                KT_TCPHandler.CloseDialog(player);
            }
            /// Nếu là bước 1
            else if (selectionID > -1000)
            {
                /// ID sự kiện
                int eventID = -selectionID;
                /// Thông tin sự kiện
                if (!SpecialEvent.AllEvents.TryGetValue(eventID, out SpecialEvent.EventInfo eventInfo))
                {
                    SpecialEvent_Logic.ShowNotify(npc, player, "Thật thứ lỗi, sự kiện không tồn tại!");
                    return;
                }

                KNPCDialog dialog = new KNPCDialog();
                dialog.Text = eventInfo.NPCTalk;
                dialog.Text += string.Format("<br><color=yellow>Thời gian:</color> <color=green>{0}</color> - <color=green>{1}</color>", eventInfo.TimeConfig.FromDay.ToString("dd/MM/yyyy"), eventInfo.TimeConfig.ToDay.ToString("dd/MM/yyyy"));
                /// Duyệt danh sách phần quà
                foreach (SpecialEvent.EventInfo.AwardInfo awardInfo in eventInfo.Awards.Values)
                {
                    int _selectionID = eventInfo.ID * 1000 + awardInfo.SelectionID;
                    dialog.Selections[-_selectionID] = awardInfo.Selection;
                }
                /// Sự kiện Select
                dialog.OnSelect = (x) =>
                {
                    SpecialEvent_Logic.OnNPCSelect(npc, player, x.SelectID);
                };
                /// Hiện NPCDialog
                KTNPCDialogManager.AddNPCDialog(dialog);
                dialog.Show(npc, player);
            }
            /// Nếu là bước 2
            else
            {
                /// ID sự kiện
                int eventID = -selectionID / 1000;
                /// Thông tin sự kiện
                if (!SpecialEvent.AllEvents.TryGetValue(eventID, out _))
                {
                    SpecialEvent_Logic.ShowNotify(npc, player, "Thật thứ lỗi, sự kiện không tồn tại!");
                    return;
                }
                /// Nếu sự kiện chưa kích hoạt hoặc đã hết hạn
                if (!SpecialEvent.Events.TryGetValue(eventID, out SpecialEvent.EventInfo eventInfo))
                {
                    SpecialEvent_Logic.ShowNotify(npc, player, "Sự kiện chưa mở hoặc đã hết thời gian. Vui lòng quay lại sau!");
                    return;
                }

                /// ID lựa chọn
                selectionID = System.Math.Abs(eventID * 1000 + selectionID);
                /// Nếu lựa chọn không tồn tại
                if (!eventInfo.Awards.TryGetValue(selectionID, out SpecialEvent.EventInfo.AwardInfo awardInfo))
                {
                    SpecialEvent_Logic.ShowNotify(npc, player, "Có lỗi phát sinh!");
                    return;
                }

                /// Nếu có yêu cầu bạc
                if (awardInfo.RequireMoney > 0)
                {
                    /// Nếu số bạc không đủ
                    if (player.Money < awardInfo.RequireMoney)
                    {
                        SpecialEvent_Logic.ShowNotify(npc, player, "Số bạc mang theo không đủ!");
                        return;
                    }
                }
                /// Nếu có yêu cầu bạc khóa
                if (awardInfo.RequireBoundMoney > 0)
                {
                    /// Nếu số bạc khóa không đủ
                    if (player.BoundMoney < awardInfo.RequireBoundMoney)
                    {
                        SpecialEvent_Logic.ShowNotify(npc, player, "Số bạc khóa mang theo không đủ!");
                        return;
                    }
                }
                /// Nếu có yêu cầu đồng
                if (awardInfo.RequireToken > 0)
                {
                    /// Nếu số đồng không đủ
                    if (player.Token < awardInfo.RequireToken)
                    {
                        SpecialEvent_Logic.ShowNotify(npc, player, "Số đồng mang theo không đủ!");
                        return;
                    }
                }
                /// Nếu có yêu cầu đồng khóa
                if (awardInfo.RequireBoundToken > 0)
                {
                    /// Nếu số đồng khóa không đủ
                    if (player.BoundToken < awardInfo.RequireBoundToken)
                    {
                        SpecialEvent_Logic.ShowNotify(npc, player, "Số đồng khóa mang theo không đủ!");
                        return;
                    }
                }

                /// Duyệt danh sách vật phẩm yêu cầu
                foreach (SpecialEvent.EventInfo.AwardInfo.RequireItemInfo requireItemInfo in awardInfo.RequireItems)
                {
                    /// Nếu số lượng không đủ
                    if (ItemManager.GetItemCountInBag(player, requireItemInfo.ItemID) < requireItemInfo.Quantity)
                    {
                        SpecialEvent_Logic.ShowNotify(npc, player, "Số lượng nguyên liệu thu thập không đủ!");
                        return;
                    }
                }

                /// Nếu túi không đủ
                if (!KTGlobal.IsHaveSpace(awardInfo.RequireFreeBagSpace, player))
                {
                    SpecialEvent_Logic.ShowNotify(npc, player, string.Format("Túi đã đầy, cần sắp xếp tối thiểu {0} ô trống để nhận thưởng!", awardInfo.RequireFreeBagSpace));
                    return;
                }

                /// Duyệt danh sách vật phẩm yêu cầu
                foreach (SpecialEvent.EventInfo.AwardInfo.RequireItemInfo requireItemInfo in awardInfo.RequireItems)
                {
                    /// Xóa vật phẩm tương ứng
                    ItemManager.RemoveItemFromBag(player, requireItemInfo.ItemID, requireItemInfo.Quantity);
                }

                /// Nếu có yêu cầu bạc
                if (awardInfo.RequireMoney > 0)
                {
                    /// Trừ bạc
                    KTGlobal.SubMoney(player, awardInfo.RequireMoney, MoneyType.Bac, "SpecialEvent");
                }
                /// Nếu có yêu cầu bạc khóa
                if (awardInfo.RequireBoundMoney > 0)
                {
                    /// Trừ bạc khóa
                    KTGlobal.SubMoney(player, awardInfo.RequireBoundMoney, MoneyType.BacKhoa, "SpecialEvent");
                }
                /// Nếu có yêu cầu đồng
                if (awardInfo.RequireToken > 0)
                {
                    /// Trừ đồng
                    KTGlobal.SubMoney(player, awardInfo.RequireToken, MoneyType.Dong, "SpecialEvent");
                }
                /// Nếu có yêu cầu đồng khóa
                if (awardInfo.RequireBoundToken > 0)
                {
                    /// Trừ đồng khóa
                    KTGlobal.SubMoney(player, awardInfo.RequireBoundToken, MoneyType.DongKhoa, "SpecialEvent");
                }

                /// Đối tượng Random
                System.Random rand = new System.Random();
                /// Duyệt danh sách phần thưởng
                for (int total = 1; total <= awardInfo.AwardCount; total++)
                {
                    /// Tỷ lệ
                    int rate = KTGlobal.GetRandomNumber(1, 10000);

                    /// Danh sách vật phẩm thưởng
                    List<SpecialEvent.EventInfo.AwardInfo.AwardItemInfo> awardItems = awardInfo.AwardItems.OrderBy(x => rand.Next()).ToList();
                    /// Chọn một vật phẩm ngẫu nhiên trong danh sách thỏa mãn
                    SpecialEvent.EventInfo.AwardInfo.AwardItemInfo award = awardItems.Where(x => x.Rate >= rate).FirstOrDefault();
                    /// Nếu không có
                    if (award == null)
                    {
                        /// Chọn vật phẩm có tỷ lệ cao nhất
                        award = awardItems.MaxBy(x => x.Rate);
                    }

                    /// Nếu vẫn không có
                    if (award == null)
                    {
                        SpecialEvent_Logic.ShowNotify(npc, player, "Đổi thưởng bị lỗi!");
                        return;
                    }

                    /// Thêm vật phẩm tương ứng
                    ItemManager.CreateItem(Global._TCPManager.TcpOutPacketPool, player, award.ItemID, award.Quantity, 0, "SpecialEvent_Logic", true, award.Bound ? 1 : 0, false, ItemManager.ConstGoodsEndTime, "", -1);

                    /// Thông tin vật phẩm
                    ItemData awardItemData = ItemManager.GetItemTemplate(award.ItemID);
                    /// Nếu tồn tại thông tin vật phẩm
                    if (awardItemData != null)
                    {
                        /// Ghi Log
                        LogManager.WriteLog(LogTypes.SpecialEvent, string.Format("[{0}] Player {1} (ID: {2}) got {3}x [{4}]", eventInfo.Name, player.RoleName, player.RoleID, award.Quantity, awardItemData.Name));
                    }
                }

                /// Thông báo nhận thưởng thành công
                SpecialEvent_Logic.ShowNotify(npc, player, "Đổi phần thưởng thành công!");
            }
        }

        /// <summary>
        /// Hiện thông báo tương ứng
        /// </summary>
        /// <param name="npc"></param>
        /// <param name="player"></param>
        /// <param name="text"></param>
        private static void ShowNotify(NPC npc, KPlayer player, string text)
        {
            KNPCDialog dialog = new KNPCDialog();
            dialog.Text = text;
            dialog.Selections[-999] = "Kết thúc đối thoại";
            /// Sự kiện Select
            dialog.OnSelect = (x) =>
            {
                SpecialEvent_Logic.OnNPCSelect(npc, player, x.SelectID);
            };
            KTNPCDialogManager.AddNPCDialog(dialog);
            dialog.Show(npc, player);
        }
        #endregion

        #region Drop
        /// <summary>
        /// Sự kiện khi quái bị chết
        /// </summary>
        /// <param name="killer"></param>
        /// <param name="monster"></param>
        public static void ProcessMonsterDie(GameObject killer, Monster monster)
        {
            /// Nếu không phải bị người chơi giết
            if (!(killer is KPlayer player))
            {
                return;
            }

            /// Toác
            if (monster == null)
            {
                return;
            }

            /// Duyệt danh sách sự kiện thỏa mãn
            foreach (SpecialEvent.EventInfo eventInfo in SpecialEvent.Events.Values)
            {
                /// Nếu sự kiện chưa bắt đầu
                if (!eventInfo.TimeConfig.InTime)
                {
                    /// Bỏ qua
                    continue;
                }
                /// Nếu sự kiện không có rơi nguyên liệu ở quái tương ứng
                else if (!eventInfo.DropMonsters.Contains(monster.GetMonsterData().ExtensionID))
                {
                    /// Bỏ qua
                    continue;
                }

                /// Thông tin Drop tương ứng
                SpecialEvent.EventInfo.DropInfo dropInfo = eventInfo.Materials.Where(x => x.MonsterID == monster.GetMonsterData().ExtensionID && (x.MapIDs.Contains(-1) || x.MapIDs.Contains(monster.CurrentMapCode))).FirstOrDefault();
                /// Nếu không tồn tại
                if (dropInfo == null)
                {
                    /// Bỏ qua
                    continue;
                }

                /// Danh sách vật phẩm sẽ rơi
                List<ItemData> dropItems = null;
                /// Duyệt danh sách rơi
                foreach (SpecialEvent.EventInfo.DropInfo.DropItemInfo dropItemInfo in dropInfo.Items)
                {
                    /// Tỷ lệ
                    int rate = KTGlobal.GetRandomNumber(1, 10000);
                    /// Nếu có thể rơi
                    if (dropItemInfo.Rate >= rate)
                    {
                        /// Thông tin vật phẩm tương ứng
                        ItemData itemData = ItemManager.GetItemTemplate(dropItemInfo.ID);
                        /// Toác
                        if (itemData == null)
                        {
                            /// Bỏ qua
                            continue;
                        }

                        /// Nếu danh sách vật phẩm rơi không tồn tại
                        if (dropItems == null)
                        {
                            /// Tạo mới
                            dropItems = new List<ItemData>();
                        }
                        /// Cho rơi vật phẩm này
                        dropItems.Add(itemData);
                    }
                }

                /// Nếu tồn tại danh sách vật phẩm sẽ rơi
                if (dropItems != null)
                {
                    /// Tạo vật phẩm rơi dưới đất tương ứng
                    KTGoodsPackManager.CreateDropToMap(player, dropItems, monster);
                    /// Giải phóng bộ nhớ
                    dropItems.Clear();
                }
            }
        }
        #endregion

        #region GrowPoint
        /// <summary>
        /// Tái sinh điểm thu thập sự kiện tương ứng
        /// </summary>
        /// <param name="growPointInfo"></param>
        public static void RespawnGrowPoint(SpecialEvent.EventInfo.GrowPointInfo growPointInfo)
        {
            /// Đối tượng Random
            Random rand = new Random();
            /// Chọn danh sách vị trí sẽ sinh ra
            List<UnityEngine.Vector2Int> spawnPositions = growPointInfo.SpawnCount == -1 ? growPointInfo.Pos : growPointInfo.Pos.OrderBy(x => rand.Next()).Take(growPointInfo.SpawnCount).ToList();
            /// Duyệt danh sách vị trí
            foreach (UnityEngine.Vector2Int spawnPos in spawnPositions)
            {
                /// Tạo điểm thu thập
                GrowPoint growPoint = KTGrowPointManager.Add(growPointInfo.MapID, -1, GrowPointXML.Parse(growPointInfo.ID, growPointInfo.Name, -1, -1, growPointInfo.CollectTicks, false), spawnPos.x, spawnPos.y, growPointInfo.DurationTicks);
                /// Sự kiện trước khi thu thập
                growPoint.ConditionCheck = (player) =>
                {
                    /// Toác
                    if (!KTGrowPointManager.GrowPoints.ContainsKey(growPoint.ID))
                    {
                        KTPlayerManager.ShowNotification(player, "Đối tượng đã bị kẻ khác thu thập mất!");
                        return false;
                    }

                    /// Nếu số ô trống trong túi không đủ
                    if (!KTGlobal.IsHaveSpace(growPointInfo.RequireBagSpaces, player))
                    {
                        KTPlayerManager.ShowNotification(player, string.Format("Yêu cầu tối thiểu {0} ô trống trong túi mới có thể thu thập!", growPointInfo.RequireBagSpaces));
                        return false;
                    }
                    /// Có thể thu thập
                    return true;
                };
                /// Sự kiện thu thập thành công
                growPoint.GrowPointCollectCompleted = (player) =>
                {
                    /// Toác
                    if (!KTGrowPointManager.GrowPoints.ContainsKey(growPoint.ID))
                    {
                        KTPlayerManager.ShowNotification(player, "Đối tượng đã bị kẻ khác thu thập mất!");
                        return;
                    }

                    /// Xóa điểm thu thập
                    KTGrowPointManager.Remove(growPoint);

                    /// Danh sách vật phẩm sẽ thu thập được
                    List<ItemData> collectItems = null;
                    /// Duyệt danh sách thu thập
                    foreach (SpecialEvent.EventInfo.GrowPointInfo.CollectItemInfo collectItemInfo in growPointInfo.Items)
                    {
                        /// Tỷ lệ
                        int rate = KTGlobal.GetRandomNumber(1, 10000);
                        /// Nếu có thể thu thập
                        if (collectItemInfo.Rate >= rate)
                        {
                            /// Thông tin vật phẩm tương ứng
                            ItemData itemData = ItemManager.GetItemTemplate(collectItemInfo.ID);
                            /// Toác
                            if (itemData == null)
                            {
                                /// Bỏ qua
                                continue;
                            }

                            /// Nếu danh sách vật phẩm thu thập không tồn tại
                            if (collectItems == null)
                            {
                                /// Tạo mới
                                collectItems = new List<ItemData>();
                            }
                            /// Cho thu thập vật phẩm này
                            collectItems.Add(itemData);
                        }
                    }

                    /// Nếu danh sách rỗng
                    if (collectItems == null)
                    {
                        /// Chọn vật phẩm có tỷ lệ thu thập được cao nhất
                        SpecialEvent.EventInfo.GrowPointInfo.CollectItemInfo itemInfo = growPointInfo.Items.MaxBy(x => x.Rate);
                        /// Thông tin vật phẩm tương ứng
                        ItemData itemData = ItemManager.GetItemTemplate(itemInfo.ID);
                        /// Toác
                        if (itemData == null)
                        {
                            /// Bỏ qua
                            return;
                        }
                        /// Tạo mới
                        collectItems = new List<ItemData>() { itemData };
                    }

                    /// Duyệt danh sách vật phẩm
                    foreach (ItemData itemData in collectItems)
                    {
                        /// Thêm vật phẩm vào túi
                        ItemManager.CreateItem(Global._TCPManager.TcpOutPacketPool, player, itemData.ItemID, 1, 0, "SpecialEvent_Logic", true, 1, false, ItemManager.ConstGoodsEndTime, "", -1);
                    }

                };
            }
        }
        #endregion
    }
}
