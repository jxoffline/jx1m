using GameServer.Core.Executor;
using GameServer.KiemThe.Entities;
using GameServer.KiemThe.Logic;
using GameServer.Logic;
using GameServer.Server;
using Server.Data;
using Server.Protocol;
using Server.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace GameServer.KiemThe.Core.Item
{
    /// <summary>
    /// Quản lý chế tạo
    /// </summary>
    public class ItemCraftingManager
    {
        public static string LifeSkill_XML = "Config/KT_Item/LifeSkill.xml";

        public static string ItemBreakConfig_XML = "Config/KT_Item/ItemBreakConfig.xml";

        public static LifeSkill _LifeSkill = new LifeSkill();

        public static ItemBreakConfig _ItemBreakConfig = new ItemBreakConfig();

        /// <summary>
        /// Loading all Drop
        /// </summary>
        public static void Setup()
        {
            string Files = KTGlobal.GetDataPath(LifeSkill_XML);
            using (var stream = System.IO.File.OpenRead(Files))
            {
                var serializer = new XmlSerializer(typeof(LifeSkill));
                _LifeSkill = serializer.Deserialize(stream) as LifeSkill;
            }

            string BearkFiles = KTGlobal.GetDataPath(ItemBreakConfig_XML);

            using (var stream = System.IO.File.OpenRead(BearkFiles))
            {
                var serializer = new XmlSerializer(typeof(ItemBreakConfig));
                _ItemBreakConfig = serializer.Deserialize(stream) as ItemBreakConfig;
            }
        }

        /// <summary>
        /// Tách trang bị
        /// </summary>
        public static void BreakingItemRequest(KPlayer player)
        {
            // Thực hiện tách trang bị
            KT_TCPHandler.SendOpenInputItems(player, "Tách vật phẩm lấy nguyên liệu", "Đặt vật phẩm muốn tách vào đây\n<color=red>-Vật phẩm muốn tách phải từ cấp độ " + _ItemBreakConfig.MinItemLevel + " trở lên\n-Phải có " + _ItemBreakConfig.MinLine + " dòng trở lên</color>\nCác vật phẩm hỗ trợ tách bao gồm <color=green>Trang Sức,Phòng Cụ,Vũ Khí</color>", "<color=red>Chỉ đặt 1 vật phẩm 1 lần tách\nVật phẩm đã cường hóa không thể tách\nVật phẩm khóa không thể tách</color>", "BreakingItemRequest");
        }

        /// <summary>
        /// Mở khóa trang bị
        /// </summary>
        /// <param name="player"></param>
        public static void UnlockItemRequest(KPlayer player)
        {
            // Thực hiện mở khóa trang bị
            KT_TCPHandler.SendOpenInputItems(player, "Mở khóa trang bị", "Đặt vật phẩm muốn mở khóa vào đây\n<color=red></color>", "<color=red>Đặt vật phẩm mở khóa kèm 1 Chía Khóa Như Ý để mở khóa\nSau khi mở khóa vật phẩm sẽ về trạng thái không khóa\nChỉ có thể mở khóa TRANG BỊ không thể mở khóa nguyên liệu</color>", "UnlockItemRequest");
        }

        public static void ProseccUnlockItem(KPlayer client, List<GoodsData> TotalData, string CallBack)
        {
            if (TotalData.Count > 2)
            {
                KTPlayerManager.ShowMessageBox(client, "Lỗi", "Vui lòng chỉ đặt CHÌA KHÓA NHƯ Ý và vật phẩm cần mở khóa");
                return;
            }

            var FINDKEY = TotalData.Where(x => x.GoodsID == 11774).FirstOrDefault();
            if (FINDKEY == null)
            {
                KTPlayerManager.ShowMessageBox(client, "Lỗi", "Phải có CHÌA KHÓA NHƯ Ý mới có thể mở khóa vật phẩm!");
                return;
            }

            var FINDOTHER = TotalData.Where(x => x.GoodsID != 11774).FirstOrDefault();
            if (FINDOTHER == null)
            {
                KTPlayerManager.ShowMessageBox(client, "Lỗi", "Vui lòng đặt vật phẩm cần mở khóa");
                return;
            }

            if (!ItemManager.IsCanUnlockItem(FINDOTHER))
            {
                KTPlayerManager.ShowMessageBox(client, "Lỗi", "Không hỗ trợ mở khóa loại trang bị này!");
                return;
            }

            if (FINDOTHER.Binding == 0)
            {
                KTPlayerManager.ShowMessageBox(client, "Lỗi", "Vật phẩm đã mở khóa rồi nên không thể mở khóa tiếp");
                return;
            }

            ItemManager.ItemValueCalculation(FINDOTHER, out long ItemValue, out int LinesCount);

            string BUILD = "Khi thực hiện mở khóa CHÌA KHÓA NHƯ Ý sẽ biến mất";

            double MoneyNeed = ItemValue * 1.0;

            BUILD += "\nVà tiêu tốn <color=red>" + KTGlobal.GetDisplayMoney((long)MoneyNeed) + "</color> bạc để thực hiện mở khóa trang bị\nBạn có muốn thực hiện phân giải?";

            if (!KTGlobal.IsHaveMoney(client, (int)MoneyNeed, MoneyType.Bac))
            {
                KTPlayerManager.ShowMessageBox(client, "Lỗi", "Bạc mang theo người không đủ!");
                return;
            }

            KTPlayerManager.ShowMessageBox(client, "Mở khóa vật phẩm", BUILD, () =>
            {
                // Nếu xóa vật phẩm thành công
                if (ItemManager.RemoveItemByCount(client, FINDKEY, 1, "ITEMUNLOCK"))
                {
                    if (KTGlobal.SubMoney(client, (int)MoneyNeed, MoneyType.Bac, "ITEMUNLOCK").IsOK)
                    {
                        //Tạo 1 dict để unlock vật phẩm kia
                        Dictionary<UPDATEITEM, object> TotalUpdate = new Dictionary<UPDATEITEM, object>();

                        TotalUpdate.Add(UPDATEITEM.ROLEID, client.RoleID);
                        TotalUpdate.Add(UPDATEITEM.ITEMDBID, FINDOTHER.Id);
                        //Thực hiện yêu cầu mở khóa
                        TotalUpdate.Add(UPDATEITEM.BINDING, 0);

                        string ScriptUpdateBuild = ItemManager.ItemUpdateScriptBuild(TotalUpdate);
                        string[] dbFields = null;
                        TCPProcessCmdResults dbRequestResult = Global.RequestToDBServer(Global._TCPManager.tcpClientPool, Global._TCPManager.TcpOutPacketPool, (int)TCPGameServerCmds.CMD_DB_UPDATEGOODS_CMD, ScriptUpdateBuild, out dbFields, client.ServerId);

                        if (dbRequestResult == TCPProcessCmdResults.RESULT_FAILED)
                        {
                            LogManager.WriteLog(LogTypes.Item, "[" + client.strUserID + "][" + client.RoleID + "][" + client.RoleName + "]Mở khóa vật phẩm thất bại!");

                            return;
                        }
                        else if (dbRequestResult == TCPProcessCmdResults.RESULT_DATA)
                        {
                            /// Thực hiện update lại vật phẩm trong túi đồ
                            client.GoodsData.LocalModify(FINDOTHER.Id, TotalUpdate);

                            string strcmd = string.Format("{0}:{1}", client.RoleID, FINDOTHER.Id);

                            // Packet xóa đồ đi
                            client.SendPacket((int)TCPGameServerCmds.CMD_SPR_MOVEGOODSDATA, strcmd);

                            KT_TCPHandler.NotifySelfAddGoods(client, FINDOTHER.Id, FINDOTHER.GoodsID, FINDOTHER.Forge_level, FINDOTHER.GCount, 0, 0, 1, FINDOTHER.Endtime, FINDOTHER.Strong, FINDOTHER.BagIndex, -1, FINDOTHER.Props, FINDOTHER.Series, FINDOTHER.OtherParams);

                            KTPlayerManager.ShowMessageBox(client, "Thông Báo", "Mở khóa vật phẩm thành công");
                            return;
                        }
                    }
                }
            }, true);
        }

        public static void ProsecCallBackBreakingItem(KPlayer client, List<GoodsData> TotalData, string CallBack)
        {
            if (TotalData.Count > 1)
            {
                KTPlayerManager.ShowMessageBox(client, "Lỗi", "Vui lòng chỉ đặt 1 vật phẩm muốn tách");
                return;
            }

            GoodsData _Item = TotalData[0];
            // Nếu vật phẩm trôgns
            if (_Item == null)
            {
                KTPlayerManager.ShowMessageBox(client, "Lỗi", "Dữ liệu vật phẩm bị lỗi");
                return;
            }
            // Vật phẩm khóa không thể tách
            if (_Item.Binding == 1)
            {
                KTPlayerManager.ShowMessageBox(client, "Lỗi", "Vật phẩm khóa không thể tách");
                return;
            }

            if (_Item.Creator != "")
            {
                KTPlayerManager.ShowMessageBox(client, "Lỗi", "Vật phẩm chế tạo không thể tách");
                return;
            }

            //Vật phẩm cường hóa không thể tách
            if (_Item.Forge_level > 0)
            {
                KTPlayerManager.ShowMessageBox(client, "Lỗi", "Vật phẩm đã cường hóa không thể tách");
                return;
            }
            // Nếu cầu hình vật phẩm lỗi
            ItemData _Tempate = ItemManager.GetItemTemplate(_Item.GoodsID);
            if (_Tempate == null)
            {
                KTPlayerManager.ShowMessageBox(client, "Lỗi", "Cầu hình vật phẩm bị lỗi");
                return;
            }

            // Nếu đéo phải loại hỗ trợ
            if (_Tempate.DetailType > (int)KE_ITEM_EQUIP_DETAILTYPE.equip_pendant)
            {
                KTPlayerManager.ShowMessageBox(client, "Lỗi", "Trang bị không hỗ trợ phân giải");
                return;
            }

            // Nếu đéo đủ cấp độ yêu cầu
            if (_Tempate.Level < _ItemBreakConfig.MinItemLevel)
            {
                KTPlayerManager.ShowMessageBox(client, "Lỗi", "Cấp độ vật phẩm muốn tách phải từ " + _ItemBreakConfig.MinItemLevel + " trở lên");
                return;
            }

            KItem _ItemModel = new KItem(_Item);

            //Nếu đéo thể khởi tạo vật phẩm
            if (_ItemModel == null)
            {
                KTPlayerManager.ShowMessageBox(client, "Lỗi", "Khởi tạo thông tin trang bị lỗi");
                return;
            }

            // nếu mà số dòng đéo đủ để tách
            if (_ItemModel.CountLines() < _ItemBreakConfig.MinLine)
            {
                KTPlayerManager.ShowMessageBox(client, "Lỗi", "Số dòng yêu cầu không đủ để tách trang bị");
                return;
            }

            var FindConfig = _ItemBreakConfig.BreakItems.Where(x => x.DETAILTYPE == _Tempate.DetailType && x.LEVEL == _Tempate.Level).FirstOrDefault();
            if (FindConfig == null)
            {
                KTPlayerManager.ShowMessageBox(client, "Lỗi", "Vật phẩm này không hỗ trợ phân giải");
                return;
            }

            List<int> TotalItem = FindConfig.ITEMS;

            List<ItemData> TotalOutData = new List<ItemData>();

            foreach (int OutItem in TotalItem)
            {
                ItemData OutItem_Data = ItemManager.GetItemTemplate(OutItem);
                if (OutItem_Data != null)
                {
                    TotalOutData.Add(OutItem_Data);
                }
            }
            /// Lấy ra toàn bộ vật phẩm
            if (TotalOutData.Count <= 0)
            {
                KTPlayerManager.ShowMessageBox(client, "Lỗi", "Vật phẩm này không hỗ trợ phân giải");
                return;
            }

            ItemManager.ItemValueCalculation(_Item, out long ItemValue, out int LinesCount);

            //Nhân với rate để ra vật phẩm
            int FinalItemValue = (int)((double)ItemValue * _ItemBreakConfig.Rate);

            List<RateBuild> TotalRateBuild = new List<RateBuild>();

            //sort lại theo chiều tăng dần
            TotalOutData.OrderBy(x => x.ItemValue);

            string BUILD = "Khi phân giải vật phẩm này bạn có thể nhận được\n";

            while (FinalItemValue > 0)
            {
                foreach (ItemData Items in TotalOutData)
                {
                    RateBuild _Build = new RateBuild();
                    _Build.ItemID = Items.ItemID;
                    _Build.ItemName = Items.Name;

                    if (FinalItemValue > Items.ItemValue)
                    {
                        _Build.Rate = 100;

                        FinalItemValue -= Items.ItemValue;
                    }
                    else
                    {
                        int Rate = (int)(((double)FinalItemValue / (double)Items.ItemValue) * 100);
                        if (Rate < 0)
                        {
                            Rate = 0;
                        }

                        _Build.Rate = Rate;

                        FinalItemValue -= Items.ItemValue;
                    }

                    if (_Build.Rate > 0)
                    {
                        BUILD += "<color=green>" + Items.Name + "X1 : Với Tỉ lệ :";
                        BUILD += _Build.Rate + "%</color>\n";

                        TotalRateBuild.Add(_Build);
                    }
                }
            }

            ///Check xem có đủ không
            if (!KTGlobal.IsHaveSpace(TotalRateBuild.Count, client))
            {
                KTPlayerManager.ShowMessageBox(client, "Lỗi", "Cần thêm " + TotalRateBuild.Count + "ô trống để tiến hành phân giải");
                return;
            }

            double MoneyNeed = ItemValue * 2.0;

            BUILD += "\nVà tiêu tốn <color=red>" + KTGlobal.GetDisplayMoney((long)MoneyNeed) + "</color> bạc để phân giải\nBạn có muốn thực hiện phân giải?";

            // Check xem có đủ bạc không
            if (!KTGlobal.IsHaveMoney(client, (int)MoneyNeed, MoneyType.Bac))
            {
                KTPlayerManager.ShowMessageBox(client, "Lỗi", "Cần thêm <color=red>" + KTGlobal.GetDisplayMoney((long)MoneyNeed) + "</color> bạc để tiến hành phân giải");
                return;
            }

            KTPlayerManager.ShowMessageBox(client, "Phân giải vật phẩm", BUILD, () =>
            {
                // Nếu xóa vật phẩm thành công
                if (ItemManager.RemoveItemByCount(client, _Item, 1, "ITEMBREAK"))
                {
                    if (KTGlobal.SubMoney(client, (int)MoneyNeed, MoneyType.Bac, "BREAKITEM").IsOK)
                    {
                        foreach (RateBuild _ItemRandom in TotalRateBuild)
                        {
                            if (_ItemRandom.Rate > 100)
                            {
                                if (!ItemManager.CreateItem(Global._TCPManager.TcpOutPacketPool, client, _ItemRandom.ItemID, 1, 0, "ITEMBREAK", true, 0, false, ItemManager.ConstGoodsEndTime))
                                {
                                    KTPlayerManager.ShowNotification(client, "Có lỗi khi nhận vật phẩm chế tạo");
                                }
                                else
                                {
                                    KTPlayerManager.ShowNotification(client, "Tách vật phẩm <color=green>[" + _Item.ItemName + "]</color> thành công");
                                }
                            }
                            else
                            {
                                int Radom = KTGlobal.GetRandomNumber(0, 100);
                                if (Radom < _ItemRandom.Rate)
                                {
                                    if (!ItemManager.CreateItem(Global._TCPManager.TcpOutPacketPool, client, _ItemRandom.ItemID, 1, 0, "ITEMBREAK", true, 0, false, ItemManager.ConstGoodsEndTime))
                                    {
                                        KTPlayerManager.ShowNotification(client, "Có lỗi khi nhận vật phẩm chế tạo");
                                    }
                                    else
                                    {
                                        KTPlayerManager.ShowNotification(client, "Tách vật phẩm <color=green>[" + _Item.ItemName + "]</color> thành công");
                                    }
                                }
                                else
                                {
                                    KTPlayerManager.ShowNotification(client, "Tách vật phẩm <color=red>[" + _Item.ItemName + "]</color> thất bại");
                                }
                            }
                        }
                    }
                }
            }, true);

            //Đọc ra tài phú của trang bị
        }

        /// <summary>
        /// Truy vấn thông tin cấp độ và kinh nghiệm kỹ năng sống của người chơi tương ứng
        /// </summary>
        /// <param name="id"></param>
        /// <param name="player"></param>
        /// <param name="level"></param>
        /// <param name="exp"></param>
        /// <returns></returns>
        public static bool QueryLifeSkillLevelAndExp(int id, KPlayer player, out int level, out int exp)
        {
            level = 0;
            exp = 0;
            LifeSkillPram lifeSkillParam = player.GetLifeSkill(id);
            if (lifeSkillParam != null)
            {
                level = lifeSkillParam.LifeSkillLevel;
                exp = lifeSkillParam.LifeSkillExp;
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Trả về cấp độ kỹ năng sống tương ứng
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="player"></param>
        /// <returns></returns>
        private static int GetLifeSkilLLevel(int id, KPlayer player)
        {
            LifeSkillPram lifeSkillParam = player.GetLifeSkill(id);
            if (lifeSkillParam != null)
            {
                return lifeSkillParam.LifeSkillLevel;
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// Thêm kinh nghiệm cho kỹ năng sống
        /// </summary>
        /// <param name="lifeSkillID"></param>
        /// <param name="player"></param>
        /// <param name="nExp"></param>
        public static void AddExp(int lifeSkillID, KPlayer player, int nExp)
        {
            LifeSkillPram lifeSkillParam = player.GetLifeSkill(lifeSkillID);
            if (lifeSkillParam != null)
            {
                /// Kinh nghiệm sau khi thêm vào
                int expAdd = nExp + lifeSkillParam.LifeSkillExp;
                do
                {
                    /// Cấp tiếp theo
                    LifeSkillExp nextLevelExp = ItemCraftingManager._LifeSkill.TotalExp.Where(x => x.Level == lifeSkillParam.LifeSkillLevel + 1).FirstOrDefault();

                    /// Nếu không tồn tại kinh nghiệm cấp hiện tại hoặc tiếp theo
                    if (nextLevelExp == null)
                    {
                        /// Thoát lặp
                        break;
                    }

                    /// Kinh nghiệm tối đa ở cấp hiện tại
                    int nMaxExp = nextLevelExp.Exp;

                    /// Nếu đủ để lên cấp
                    if (expAdd >= nMaxExp)
                    {
                        /// Trừ điểm kinh nghiệm còn lại
                        expAdd -= nMaxExp;
                        /// Tăng cấp
                        lifeSkillParam.LifeSkillLevel++;
                    }
                    else
                    {
                        /// Cập nhật kinh nghiệm ở cấp hiện tại
                        lifeSkillParam.LifeSkillExp = expAdd;
                        /// Thoát lặp
                        break;
                    }
                }
                while (true);
                /// Cập nhật kinh nghiệm ở cấp hiện tại
                lifeSkillParam.LifeSkillExp = expAdd;
            }

            LogManager.WriteLog(LogTypes.LifeSkill, "[" + player.RoleID + "][" + player.RoleName + "] Kỹ năng sống ID :" + lifeSkillID + " | LEVEL :" + lifeSkillParam.LifeSkillLevel + " | EXP :" + lifeSkillParam.LifeSkillExp);
            /// Gửi thông báo về Client
            KT_TCPHandler.NotifySelfLifeSkillLevelAndExpChanged(player, lifeSkillID, lifeSkillParam.LifeSkillLevel, lifeSkillParam.LifeSkillExp);
        }

        /// <summary>
        /// Thực hiện chế đồ
        /// </summary>
        /// <param name="recipeID"></param>
        /// <param name="player"></param>
        /// <param name="done"></param>
        /// <returns></returns>
        public static bool DoCrafting(int recipeID, KPlayer player, Action done)
        {
            /// Nếu đang chế đồ
            if (player.IsCrafting)
            {
                KTPlayerManager.ShowNotification(player, "Thao tác quá nhanh, xin hãy đợi giây lát!");
                return false;
            }

            bool IsCheThanhCong = false;

            var Find = _LifeSkill.TotalRecipe.Where(x => x.ID == recipeID).FirstOrDefault();
            if (Find != null)
            {
                int Belong = Find.Belong;

                int LevelSkill = GetLifeSkilLLevel(Belong, player);

                if (LevelSkill == 0)
                {
                    KTPlayerManager.ShowNotification(player, "Kỹ năng chưa được học!");
                    return false;
                }

                if (LevelSkill < Find.SkillLevel)
                {
                    KTPlayerManager.ShowNotification(player, "Kỹ năng chế tạo không đủ cấp độ để chế vật phẩm này!");
                    return false;
                }

                //if (player.m_eDoing != Entities.KE_NPC_DOING.do_stand)
                //{
                //    PlayerManager.ShowNotification(player, "Chỉ trạng thái đứng im mới được dùng kỹ năng sống");
                //    return false;
                //}

                int Cost = Find.Cost;

                var FindLIfeSkillData = _LifeSkill.TotalSkill.Where(x => x.Belong == Belong).FirstOrDefault();

                if (FindLIfeSkillData.Gene == 1)
                {
                    if (player.GetGatherPoint() < Cost)
                    {
                        KTPlayerManager.ShowNotification(player, "Hoạt lực không đủ, không thể hợp thành vật phẩm");
                        return false;
                    }
                }
                if (FindLIfeSkillData.Gene == 0)
                {
                    if (player.GetMakePoint() < Cost)
                    {
                        KTPlayerManager.ShowNotification(player, "Tinh lực không đủ, không thể hợp thành vật phẩm");
                        return false;
                    }
                }

                List<ItemStuff> ListStuffRequest = Find.ListStuffRequest;

                bool IsHaveStuff = true;

                /// Nếu là các công thức chế sò thì yêu cầu nguyên liệu không khóa
                bool isSeashellRecipe = false;
                if (recipeID >= 1557 && recipeID <= 1586)
                {
                    isSeashellRecipe = true;
                }

                foreach (ItemStuff _ItemRequest in ListStuffRequest)
                {
                    int NumberReqest = _ItemRequest.Number;

                    /// Tổng số vật phẩm có
                    int CountInBag = ItemManager.GetItemCountInBag(player, _ItemRequest.ItemTemplateID, isSeashellRecipe ? 0 : -1);
                    if (CountInBag < NumberReqest)
                    {
                        IsHaveStuff = false;
                        break;
                    }
                }

                if (!IsHaveStuff)
                {
                    KTPlayerManager.ShowNotification(player, "Nguyên liệu yêu cầu không đủ không thể hợp thành");
                    return false;
                }

                if (!KTGlobal.IsHaveSpace(1, player))
                {
                    KTPlayerManager.ShowNotification(player, "Túi trên người không đủ 1 ô trống không thể tiến hành chế tạo");
                    return false;
                }

                // Trừ hoạt lực
                if (FindLIfeSkillData.Gene == 1)
                {
                    player.ChangeCurGatherPoint(-Cost);
                }
                // Trừ tinh lực
                if (FindLIfeSkillData.Gene == 0)
                {
                    player.ChangeCurMakePoint(-Cost);
                }

                // Xóa nguyên liệu
                foreach (ItemStuff _ItemRequest in ListStuffRequest)
                {
                    int NumberReqest = _ItemRequest.Number;

                    if (!ItemManager.RemoveItemFromBag(player, _ItemRequest.ItemTemplateID, NumberReqest, isSeashellRecipe ? 0 : -1, "Chế đồ"))
                    {
                        KTPlayerManager.ShowNotification(player, "Có lỗi khi xóa vật phẩm trên người vui lòng liên hệ ADM để được giúp đỡ");
                        return false;
                    }
                }

                List<ItemCraf> TotalOutPut = Find.ListProduceOut;

                int Random = KTGlobal.GetRandomNumber(0, 100);

                ItemCraf _SelectItem = null;

                foreach (ItemCraf _Item in TotalOutPut)
                {
                    Random = Random - _Item.Rate;
                    if (Random <= 0)
                    {
                        _SelectItem = _Item;

                        break;
                    }
                }

                LifeSkillPram lifeSkillParam = player.GetLifeSkill(Belong);

                /// Đánh dấu đang chế đồ
                player.IsCrafting = true;

                /// Tạo DelayTask
                DelayAsyncTask asyncTask = new DelayAsyncTask()
                {
                    Player = player,
                    Name = "CraftingItem",
                    Tag = new Tuple<ItemCraf, Action>(_SelectItem, () =>
                    {
                        /// Thực hiện add EXP vào kỹ năng sống
                        AddExp(Belong, player, Find.ExpGain);
                        /// Thực hiện hàm Callback
                        done?.Invoke();
                    }),
                    Callback = ItemCraftingManager.TimerProc,
                };
                /// Thực thi Task tương ứng
                System.Threading.Tasks.Task executeTask = KTKTAsyncTask.Instance.ScheduleExecuteAsync(asyncTask, Find.MakeTime * 1000 / 18);

                return true;
            }
            else
            {
                KTPlayerManager.ShowNotification(player, "Vật phẩm muốn chế tạo không tồn tại!");
                return false;
            }
        }

        /// <summary>
        ///  Thực hiện tạo đồ sau khi chạy xong hàm DELAY
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void TimerProc(object sender, EventArgs e)
        {
            DelayAsyncTask task = (DelayAsyncTask)sender;

            /// Nếu đối tượng không hợp lệ
            if (!(task.Tag is Tuple<ItemCraf, Action> pair))
            {
                return;
            }

            ItemCraf _SelectItem = pair.Item1;
            Action done = pair.Item2;

            KPlayer player = task.Player;
            if (_SelectItem != null)
            {
                ItemData _FindOutPut = ItemManager.GetItemTemplate(_SelectItem.ItemTemplateID);

                int Series = _SelectItem.Series;

                if (_FindOutPut != null)
                {
                    if (_FindOutPut.Series != -1)
                    {
                        Series = _FindOutPut.Series;
                    }
                }
                // Thưc hiện add vật phẩm vào kỹ năng sôngs
                if (!ItemManager.CreateItem(Global._TCPManager.TcpOutPacketPool, task.Player, _SelectItem.ItemTemplateID, 1, 0, "ITEMCRAFTING", true, _SelectItem.Bind, false, ItemManager.ConstGoodsEndTime, "", Series, player.RoleName))
                {
                    KTPlayerManager.ShowNotification(player, "Có lỗi khi nhận vật phẩm chế tạo");
                }

                TCPOutPacketPool pool = Global._TCPManager.TcpOutPacketPool;

                //  ProcessTask.Process(Global._TCPManager.MySocketListener, pool, player, -1, -1, _SelectItem.ItemTemplateID, TaskTypes.Crafting);
            }

            /// Thực hiện hàm Callback
            done?.Invoke();

            /// Hủy đánh dấu đang chế đồ
            player.IsCrafting = false;
        }
    }
}