using GameServer.Core.Executor;
using GameServer.KiemThe;
using GameServer.KiemThe.Core.Item;
using GameServer.KiemThe.Core.Task;
using GameServer.KiemThe.Logic;
using GameServer.Server;
using Server.Data;
using Server.Protocol;
using Server.TCP;
using Server.Tools;
using System;
using System.Collections.Generic;
using System.Windows;
using static GameServer.Logic.KTMapManager;

namespace GameServer.Logic
{
    /// <summary>
    /// Class xử lý toàn bộ vè nhiệm vụ
    /// </summary>
    public class ProcessTask
    {
        public ProcessTask()
        {
        }

        #region Với nhiệm vụ đang được thực hiện

        //string BUILD = txtloaivatpham.SelectedIndex + "|" + (int)MyStatus + "|" + typenguhanh.SelectedIndex + "|" + ((Property)(txtdongmuontim.SelectedItem)).ID + "|" + txtvaluline.Text;

        /// <summary>
        /// Hàm xử lý
        /// </summary>
        public static void Process(SocketListener sl, TCPOutPacketPool pool, KPlayer client, int npcID, int extensionID, int goodsID, TaskTypes taskType)
        {
            switch (taskType)
            {
                case TaskTypes.Talk:
                case TaskTypes.GetSomething:
                    ProcessTalk(sl, pool, client, npcID, extensionID, goodsID, taskType);
                    break;

                case TaskTypes.TransferSomething:
                    ProcessTransferSomething(sl, pool, client, taskType);
                    break;

                case TaskTypes.Collect:
                    ProsecCollect(sl, pool, client, npcID, extensionID, goodsID, taskType);
                    break;

                case TaskTypes.KillMonster:
                case TaskTypes.MonsterSomething:
                    ProcessKillMonster(sl, pool, client, npcID, extensionID, goodsID, taskType);
                    break;

                case TaskTypes.BuySomething:
                    ProcessBuy(sl, pool, client, npcID, extensionID, goodsID, taskType);
                    break;

                case TaskTypes.Crafting:
                    ProseccCrafting(sl, pool, client, npcID, extensionID, goodsID, taskType);
                    break;

                case TaskTypes.Enhance:
                case TaskTypes.EnhanceTime:
                    ProseccEnhance(sl, pool, client, npcID, extensionID, goodsID, taskType);
                    break;

                case TaskTypes.UseSomething:
                    ProcessUsingSomething(sl, pool, client, npcID, extensionID, goodsID, taskType);
                    break;

                case TaskTypes.AnswerQuest:
                    ProcessAnswerQuestion(sl, pool, client, npcID, extensionID, goodsID, taskType);
                    break;

                case TaskTypes.JoinFaction:
                    JoinMemPhai(sl, pool, client, npcID, extensionID, goodsID, taskType);
                    break;

                case TaskTypes.GetItemWithSpcecialLine:
                    CheckItemWithSpecialLine(sl, pool, client, npcID, extensionID, goodsID, taskType);
                    break;

                default:
                    break;
            }
        }

        /// <summary>
        /// Check item xem có hợp lệ không
        /// </summary>
        /// <param name="sl"></param>
        /// <param name="pool"></param>
        /// <param name="client"></param>
        /// <param name="npcID"></param>
        /// <param name="extensionID"></param>
        /// <param name="goodsID"></param>
        /// <param name="taskType"></param>
        private static void CheckItemWithSpecialLine(SocketListener sl, TCPOutPacketPool pool, KPlayer client, int npcID, int extensionID, int goodsID, TaskTypes taskType)
        {
            if (null == client.TaskDataList) return;

            long nowTicks = TimeUtil.NOW();

            bool updateTask = false;
            int taskid = -1;

            lock (client.TaskDataList)
            {
                for (int i = 0; i < client.TaskDataList.Count; i++)
                {
                    taskid = client.TaskDataList[i].DoingTaskID;

                    Task _taskfind = TaskManager.getInstance().FindTaskById(taskid);

                    if (_taskfind == null)
                    {
                        continue;
                    }

                    //Xác định xem nhiệm vụ có hợp lệ không
                    if (!IsTaskValid(client, _taskfind, client.TaskDataList[i], nowTicks))
                    {
                        continue;
                    }

                    updateTask = false;
                    // Nếu có vật phẩm kiểu này
                    if (_taskfind.TargetType == (int)TaskTypes.GetItemWithSpcecialLine)
                    {
                        string DataRequest = _taskfind.PropsName;

                        string[] Pram = DataRequest.Split('|');
                        int ItemType = Int32.Parse(Pram[0]);
                        int ItemCatagory = Int32.Parse(Pram[1]);
                        int ItemSeries = Int32.Parse(Pram[2]);
                        int SymboyID = Int32.Parse(Pram[3]);

                        int ValueSymboy = Int32.Parse(Pram[4]);

                        // Tìm ra cái vật phẩm này
                        GoodsData goodsData = client.GoodsData.Find(goodsID, 0);

                        // Nếu vật phẩm này mà đéo phải trang bị thì đéo phải check chuyển nhiệm vụ tiếp theo luôn
                        if (!ItemManager.IsEquip(goodsData))
                        {
                            continue;
                        }
                        //Trong trường hợp đúng là trang bị thì tiếp tục check xem ngũ hành trang bị có hợp lý không

                        if (ItemSeries != 0)
                        {
                            // Nếu vật phẩm này có ngũ hành đéo thỏa mãn chim cút luôn
                            if (goodsData.Series != ItemSeries)
                            {
                                continue;
                            }
                        }

                        KItem goodItem = new KItem(goodsData);

                        // Nếu đéo phải loại yêu cầu thì chim cút luôn
                        if (ItemType != goodItem._ItemData.DetailType)
                        {
                            continue;
                        }

                        // Nếu đây là vũ khí thì check xem có phải loại cần hay không
                        if (ItemType == 0 || ItemType == 1)
                        {
                            if (goodItem._ItemData.Category != ItemCatagory)
                            {
                                continue;
                            }
                        }

                        // Nếu như không có dòng yêu cầu thì chim cút
                        if (goodItem.IsExisSymboy(SymboyID, ValueSymboy))
                        {
                            updateTask = true;

                            client.TaskDataList[i].DoingTaskVal1++;

                            //Thực hiện update vào DB
                            Global.ExecuteDBCmd((int)TCPGameServerCmds.CMD_DB_UPDATETASK_CMD,
                                string.Format("{0}:{1}:{2}:{3}:{4}:{5}",
                                client.RoleID,
                                client.TaskDataList[i].DoingTaskID,
                                client.TaskDataList[i].DbID,
                                client.TaskDataList[i].DoingTaskFocus,
                                client.TaskDataList[i].DoingTaskVal1,
                                client.TaskDataList[i].DoingTaskVal2),
                                client.ServerId);
                        }
                    }

                    if (updateTask)
                    {
                        //Update questClient State
                        KT_TCPHandler.NotifyUpdateTask(client, client.TaskDataList[i].DbID, taskid, client.TaskDataList[i].DoingTaskVal1, client.TaskDataList[i].DoingTaskVal2, client.TaskDataList[i].DoingTaskFocus);

                        //Nếu nhiệm vụ đã được hoàn thành
                        if (TaskManager.getInstance().IsQuestComplete(client.TaskDataList[i].DoingTaskID, client.TaskDataList[i].DoingTaskVal1))
                        {
                            // ADD TRẠNG THÁI CHO THẰNG NPC TRẢ NHIỆM VỤ
                            int destNPC = _taskfind.DestNPC;
                            if (-1 != destNPC)
                            {
                                int state = TaskManager.getInstance().ComputeNPCTaskState(client, client.TaskDataList, destNPC);
                                KT_TCPHandler.NotifyUpdateNPCTaskSate(client, destNPC, state);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Check xem task có còn hợp lệ không
        /// </summary>
        /// <param name="taskXmlNode"></param>
        /// <returns></returns>
        private static bool IsTaskValid(KPlayer client, Task systemTask, TaskData taskData, long nowTicks)
        {
            return true;
        }

        /// <summary>
        /// Hàm xử lý khi đối thoại NPC
        /// </summary>
        /// <param name="client"></param>
        /// <param name="npcID"></param>
        /// <param name="goodsID"></param>
        /// <param name="taskType"></param>
        private static void ProcessTalk(SocketListener sl, TCPOutPacketPool pool, KPlayer client, int npcID, int extensionID, int goodsID, TaskTypes taskType)
        {
            if (null == client.TaskDataList) return;

            long nowTicks = TimeUtil.NOW();

            bool updateTask = false;
            int taskid = -1;

            lock (client.TaskDataList)
            {
                for (int i = 0; i < client.TaskDataList.Count; i++)
                {
                    taskid = client.TaskDataList[i].DoingTaskID;

                    Task _taskfind = TaskManager.getInstance().FindTaskById(taskid);

                    if (_taskfind == null)
                    {
                        continue;
                    }

                    //Xác định xem nhiệm vụ có hợp lệ không
                    if (!IsTaskValid(client, _taskfind, client.TaskDataList[i], nowTicks))
                    {
                        continue;
                    }

                    //Nếu mà NPC đang nói chuyện = NPC taget
                    if (extensionID == _taskfind.TargetNPC)
                    {
                        updateTask = true;

                        //Nếu mà quest là kiểu trò chuyện
                        if (_taskfind.TargetType == (int)TaskTypes.Talk)
                        {
                            if (updateTask)
                            {
                                // Thực hiện update nhiệm vụ cho người chơi
                                client.TaskDataList[i].DoingTaskVal1++;

                                //Thực hiện update vào DB
                                Global.ExecuteDBCmd((int)TCPGameServerCmds.CMD_DB_UPDATETASK_CMD,
                                    string.Format("{0}:{1}:{2}:{3}:{4}:{5}",
                                    client.RoleID,
                                    client.TaskDataList[i].DoingTaskID,
                                    client.TaskDataList[i].DbID,
                                    client.TaskDataList[i].DoingTaskFocus,
                                    client.TaskDataList[i].DoingTaskVal1,
                                    client.TaskDataList[i].DoingTaskVal2),
                                    client.ServerId);

                                updateTask = true;
                            }
                        }
                        else if (_taskfind.TargetType == (int)TaskTypes.GetSomething && _taskfind.PropsName != "") // Nếu nhiệm vụ là kiểu nhận vật phẩm gì đó từ NPC
                        {
                            // nếu số lượng cần lấy chưa đủ
                            if (client.TaskDataList[i].DoingTaskVal1 < _taskfind.TargetNum)
                            {
                                //Lấy ra các thông số của vật phẩm cần lấy
                                int ItemID = Int32.Parse(_taskfind.PropsName);

                                int ItemNum = _taskfind.TargetNum;

                                if (KTGlobal.IsHaveSpace(1, client))
                                {
                                    ItemManager.CreateItem(pool, client, ItemID, ItemNum, 0, "QUEST|" + _taskfind.ID, true, 1, false, ItemManager.ConstGoodsEndTime, "", -1, "", 0, 1, false);

                                    client.TaskDataList[i].DoingTaskVal1++;

                                    //Thực hiện update vào DB
                                    Global.ExecuteDBCmd((int)TCPGameServerCmds.CMD_DB_UPDATETASK_CMD,
                                        string.Format("{0}:{1}:{2}:{3}:{4}:{5}",
                                        client.RoleID,
                                        client.TaskDataList[i].DoingTaskID,
                                        client.TaskDataList[i].DbID,
                                        client.TaskDataList[i].DoingTaskFocus,
                                        client.TaskDataList[i].DoingTaskVal1,
                                        client.TaskDataList[i].DoingTaskVal2),
                                        client.ServerId);

                                    updateTask = true;
                                }
                                else
                                {
                                    KTPlayerManager.ShowNotification(client, "Túi đồ không đủ chỗ để nhận vật phẩm này!");
                                }
                            }
                        }

                        if (updateTask)
                        {
                            //Update questClient State
                            KT_TCPHandler.NotifyUpdateTask(client, client.TaskDataList[i].DbID, taskid, client.TaskDataList[i].DoingTaskVal1, client.TaskDataList[i].DoingTaskVal2, client.TaskDataList[i].DoingTaskFocus);

                            //Nếu nhiệm vụ đã được hoàn thành
                            if (TaskManager.getInstance().IsQuestComplete(client.TaskDataList[i].DoingTaskID, client.TaskDataList[i].DoingTaskVal1))
                            {
                                // ADD TRẠNG THÁI CHO THẰNG NPC TRẢ NHIỆM VỤ
                                int destNPC = _taskfind.DestNPC;
                                if (-1 != destNPC)
                                {
                                    int state = TaskManager.getInstance().ComputeNPCTaskState(client, client.TaskDataList, destNPC);
                                    KT_TCPHandler.NotifyUpdateNPCTaskSate(client, destNPC, state);
                                }
                            }
                        }
                    }
                }
            }
        }

        private static void JoinMemPhai(SocketListener sl, TCPOutPacketPool pool, KPlayer client, int npcID, int extensionID, int goodsID, TaskTypes taskType)
        {
            if (null == client.TaskDataList) return;

            long nowTicks = TimeUtil.NOW();

            bool updateTask = false;
            int taskid = -1;

            lock (client.TaskDataList)
            {
                for (int i = 0; i < client.TaskDataList.Count; i++)
                {
                    taskid = client.TaskDataList[i].DoingTaskID;

                    Task _taskfind = TaskManager.getInstance().FindTaskById(taskid);

                    if (_taskfind == null)
                    {
                        continue;
                    }

                    //Xác định xem nhiệm vụ có hợp lệ không
                    if (!IsTaskValid(client, _taskfind, client.TaskDataList[i], nowTicks))
                    {
                        continue;
                    }

                    updateTask = true;
                    if (_taskfind.TargetType == (int)TaskTypes.JoinFaction)
                    {
                        if (client.m_cPlayerFaction.GetFactionId() != 0)
                        {
                            if (updateTask)
                            {
                                // Thực hiện update nhiệm vụ cho người chơi
                                client.TaskDataList[i].DoingTaskVal1++;

                                //Thực hiện update vào DB
                                Global.ExecuteDBCmd((int)TCPGameServerCmds.CMD_DB_UPDATETASK_CMD,
                                    string.Format("{0}:{1}:{2}:{3}:{4}:{5}",
                                    client.RoleID,
                                    client.TaskDataList[i].DoingTaskID,
                                    client.TaskDataList[i].DbID,
                                    client.TaskDataList[i].DoingTaskFocus,
                                    client.TaskDataList[i].DoingTaskVal1,
                                    client.TaskDataList[i].DoingTaskVal2),
                                    client.ServerId);

                                updateTask = true;
                            }
                        }
                    }

                    if (updateTask)
                    {
                        //Update questClient State
                        KT_TCPHandler.NotifyUpdateTask(client, client.TaskDataList[i].DbID, taskid, client.TaskDataList[i].DoingTaskVal1, client.TaskDataList[i].DoingTaskVal2, client.TaskDataList[i].DoingTaskFocus);

                        //Nếu nhiệm vụ đã được hoàn thành
                        if (TaskManager.getInstance().IsQuestComplete(client.TaskDataList[i].DoingTaskID, client.TaskDataList[i].DoingTaskVal1))
                        {
                            // ADD TRẠNG THÁI CHO THẰNG NPC TRẢ NHIỆM VỤ
                            int destNPC = _taskfind.DestNPC;
                            if (-1 != destNPC)
                            {
                                int state = TaskManager.getInstance().ComputeNPCTaskState(client, client.TaskDataList, destNPC);
                                KT_TCPHandler.NotifyUpdateNPCTaskSate(client, destNPC, state);
                            }
                        }
                    }
                }
            }
        }

        private static void ProcessAnswerQuestion(SocketListener sl, TCPOutPacketPool pool, KPlayer client, int npcID, int extensionID, int goodsID, TaskTypes taskType)
        {
            if (null == client.TaskDataList) return;

            long nowTicks = TimeUtil.NOW();

            bool updateTask = false;
            int taskid = -1;

            lock (client.TaskDataList)
            {
                for (int i = 0; i < client.TaskDataList.Count; i++)
                {
                    taskid = client.TaskDataList[i].DoingTaskID;

                    Task _taskfind = TaskManager.getInstance().FindTaskById(taskid);

                    if (_taskfind == null)
                    {
                        continue;
                    }

                    //Xác định xem nhiệm vụ có hợp lệ không
                    if (!IsTaskValid(client, _taskfind, client.TaskDataList[i], nowTicks))
                    {
                        continue;
                    }

                    //Nếu mà NPC đang nói chuyện = NPC taget
                    if (extensionID == _taskfind.TargetNPC)
                    {
                        updateTask = true;

                        //Nếu mà quest là kiểu trò chuyện
                        if (_taskfind.TargetType == (int)TaskTypes.AnswerQuest)
                        {
                            if (updateTask)
                            {
                                // Thực hiện update nhiệm vụ cho người chơi
                                client.TaskDataList[i].DoingTaskVal1++;

                                //Thực hiện update vào DB
                                Global.ExecuteDBCmd((int)TCPGameServerCmds.CMD_DB_UPDATETASK_CMD,
                                    string.Format("{0}:{1}:{2}:{3}:{4}:{5}",
                                    client.RoleID,
                                    client.TaskDataList[i].DoingTaskID,
                                    client.TaskDataList[i].DbID,
                                    client.TaskDataList[i].DoingTaskFocus,
                                    client.TaskDataList[i].DoingTaskVal1,
                                    client.TaskDataList[i].DoingTaskVal2),
                                     client.ServerId);

                                updateTask = true;
                            }
                        }

                        if (updateTask)
                        {
                            //Update questClient State
                            KT_TCPHandler.NotifyUpdateTask(client, client.TaskDataList[i].DbID, taskid, client.TaskDataList[i].DoingTaskVal1, client.TaskDataList[i].DoingTaskVal2, client.TaskDataList[i].DoingTaskFocus);

                            //Nếu nhiệm vụ đã được hoàn thành
                            if (TaskManager.getInstance().IsQuestComplete(client.TaskDataList[i].DoingTaskID, client.TaskDataList[i].DoingTaskVal1))
                            {
                                // ADD TRẠNG THÁI CHO THẰNG NPC TRẢ NHIỆM VỤ
                                int destNPC = _taskfind.DestNPC;
                                if (-1 != destNPC)
                                {
                                    int state = TaskManager.getInstance().ComputeNPCTaskState(client, client.TaskDataList, destNPC);
                                    KT_TCPHandler.NotifyUpdateNPCTaskSate(client, destNPC, state);
                                }
                            }
                        }
                    }
                }
            }
        }

        public static void AddQuestAward(Task _taskfind, KPlayer client, TCPOutPacketPool pool)
        {
            bool IsCanAddWARD = true;
            // Thực hiện add vật phẩm nếu có
            if (_taskfind.Taskaward.Length > 0)
            {
                string[] TotalItem = _taskfind.Taskaward.Split('#');

                int TotalItemCount = TotalItem.Length;

                if (KTGlobal.IsHaveSpace(TotalItemCount, client))
                {
                    foreach (string Item in TotalItem)
                    {
                        string[] ItemPram = Item.Split(',');

                        try
                        {
                            int ItemID = Int32.Parse(ItemPram[0]);
                            int ItemNumber = Int32.Parse(ItemPram[1]);

                            int BILLING = 1;

                            if (ItemID == 489 || ItemID == 343)
                            {
                                BILLING = 0;
                            }
                            string ENDTIME = ItemManager.ConstGoodsEndTime;
                            if (_taskfind.GoodsEndTime != -1)
                            {
                                DateTime Dt = DateTime.Now.AddMinutes(_taskfind.GoodsEndTime);
                                ENDTIME = Dt.ToString("yyyy-MM-dd HH:mm:ss");
                            }
                            ItemManager.CreateItem(pool, client, ItemID, ItemNumber, 0, "QUEST|" + _taskfind.ID, true, BILLING, false, ENDTIME, "", -1, "", 0, 1, false);
                        }
                        catch (Exception)
                        {
                            KTPlayerManager.ShowNotification(client, "Có lỗi khi tạo vật phẩm nhiệm vụ. Hãy liên hệ hỗ trợ!");
                        }
                    }
                }
            }

            if (IsCanAddWARD)
            {
                if (_taskfind.BacKhoa > 0)
                {
                    KTGlobal.AddMoney(client, _taskfind.BacKhoa, KiemThe.Entities.MoneyType.BacKhoa, "DOQUEST | " + _taskfind.ID + "");
                }
                if (_taskfind.Bac > 0)
                {
                    KTGlobal.AddMoney(client, _taskfind.Bac, KiemThe.Entities.MoneyType.Bac, "DOQUEST | " + _taskfind.ID + "");
                }

                // ADd EXP cho người chơi

                if (_taskfind.Experienceaward > 0)
                {
                    KTPlayerManager.AddExp(client, _taskfind.Experienceaward);
                }
                if (_taskfind.Point1 > 0)
                {
                    // + UY DANH GIANG HỒ
                    client.Prestige = client.Prestige + _taskfind.Point1;
                }
                if (_taskfind.Point2 > 0)
                {
                    KTGlobal.AddRepute(client, 101, _taskfind.Point2);
                }
            }

            //Nếu là nhiệm vụ nghĩa quan add thưởng thêm ở mỗi mốc nhiệm vụ 10-20-30-40-50
            if (_taskfind.TaskClass == (int)TaskClasses.NghiaQuan)
            {
                // Cứ 50 nhiệm vụ một thì cho bú 50 tiên thảo nộ
                if (TaskDailyArmyManager.getInstance().GetTaskDailyArmyTotalCount(client) % 50 == 0 && TaskDailyArmyManager.getInstance().GetTaskDailyArmyTotalCount(client) > 0)
                {
                    ItemManager.CreateItem(pool, client, 8483, 1, 0, "QUEST|" + _taskfind.ID, true, 1, false, ItemManager.ConstGoodsEndTime, "", -1, "", 0, 1, false);
                }

                //Nếu cứ xong 10 nhiệm vụ thì cho 1 vật phẩm 2157
                if (TaskDailyArmyManager.getInstance().GetTaskDailyArmyTotalCount(client) % 10 == 0 && TaskDailyArmyManager.getInstance().GetTaskDailyArmyTotalCount(client) > 0)
                {
                    ItemManager.CreateItem(pool, client, 2157, 1, 0, "QUEST|" + _taskfind.ID, true, 1, false, ItemManager.ConstGoodsEndTime, "", -1, "", 0, 1, false);
                }

                // Lấy ra mốc thưởng lớn nhất đã nhận
                int RewardValue = TaskDailyArmyManager.getInstance().GetTaskDailyArmyCurentAward(client);

                int TotalStreak = TaskDailyArmyManager.getInstance().GetTaskDailyArmyTotalCount(client);

                // Nếu như hoàn thành 500 nhiệm vụ đầu tiên
                if (TotalStreak > RewardValue && TotalStreak == 500)
                {
                    if (ItemManager.CreateItem(pool, client, 8230, 1, 0, "QUEST|" + _taskfind.ID, true, 1, false, ItemManager.ConstGoodsEndTime, "", -1, "", 0, 1, false))
                    {
                        TaskDailyArmyManager.getInstance().SetTaskDailyArmyCurentAward(client, 500);
                    }
                }

                if (TotalStreak > RewardValue && TotalStreak == 1000)
                {
                    if (ItemManager.CreateItem(pool, client, 11261, 1, 0, "QUEST|" + _taskfind.ID, true, 1, false, ItemManager.ConstGoodsEndTime, "", -1, "", 0, 1, false))
                    {
                        TaskDailyArmyManager.getInstance().SetTaskDailyArmyCurentAward(client, 1000);
                    }
                }

                if (TotalStreak > RewardValue && TotalStreak == 2000)
                {
                    if (ItemManager.CreateItem(pool, client, 11262, 1, 0, "QUEST|" + _taskfind.ID, true, 1, false, ItemManager.ConstGoodsEndTime, "", -1, "", 0, 1, false))
                    {
                        TaskDailyArmyManager.getInstance().SetTaskDailyArmyCurentAward(client, 2000);
                    }
                }

                if (TotalStreak > RewardValue && TotalStreak == 3000)
                {
                    if (ItemManager.CreateItem(pool, client, 11263, 1, 0, "QUEST|" + _taskfind.ID, true, 1, false, ItemManager.ConstGoodsEndTime, "", -1, "", 0, 1, false))
                    {
                        TaskDailyArmyManager.getInstance().SetTaskDailyArmyCurentAward(client, 3000);
                    }
                }

                if (TotalStreak > RewardValue && TotalStreak == 5000)
                {
                    if (ItemManager.CreateItem(pool, client, 11264, 1, 0, "QUEST|" + _taskfind.ID, true, 1, false, ItemManager.ConstGoodsEndTime, "", -1, "", 0, 1, false))
                    {
                        TaskDailyArmyManager.getInstance().SetTaskDailyArmyCurentAward(client, 5000);
                    }
                }

                //if (client.QuestBVDStreakCount == 9 || client.QuestBVDStreakCount == 19 || client.QuestBVDStreakCount == 29 || client.QuestBVDStreakCount == 39 || client.QuestBVDStreakCount == 49)
                //{
                //    // Nếu hoàn thành cả 50 mốc
                //    if (client.QuestBVDStreakCount == 49)
                //    {
                //        ItemManager.CreateItem(pool, client, 590, 1, 0, "QUEST|" + _taskfind.ID, true, 1, false, ItemManager.ConstGoodsEndTime, "", -1, "", 0, 1, false);
                //    }
                //    if (client.QuestBVDStreakCount == 9)
                //    {
                //        if (client.GetValueOfDailyRecore((int)AcitvityRecore.AreadyGetTen) == -1)
                //        {
                //            //+ 2 uy danh bạo văn đồng
                //            client.Prestige = client.Prestige + 2;

                //            client.SetValueOfDailyRecore((int)AcitvityRecore.AreadyGetTen, 1);
                //            // Lệnh bài nghĩa quân
                //            ItemManager.CreateItem(pool, client, 343, 1, 0, "QUEST|" + _taskfind.ID, true, 0, false, ItemManager.ConstGoodsEndTime, "", -1, "", 0, 1, false);
                //        }
                //    }
                //    //Exp
                //    GameManager.ClientMgr.ProcessRoleExperience(client, 100000, true, false, true, "EXP");
                //    //Bạc khóa
                //    GameManager.ClientMgr.AddUserBoundMoney(client, 10000, "DOQUEST | " + _taskfind.ID + "");
                //    //Danh vọng
                //    KTGlobal.AddRepute(client, 101, 20);

                //    client.ChangeCurGatherPoint(20);
                //    client.ChangeCurMakePoint(20);
                //}

                // TODO : Nếu đủ 50 nhiệm vụ thì add cái gì
            }

            if (_taskfind.TaskClass == (int)TaskClasses.ThuongHoi)
            {
                if (FirmTaskManager.getInstance().GetNumQuestThisWeek(client) == 10 || FirmTaskManager.getInstance().GetNumQuestThisWeek(client) == 20 || FirmTaskManager.getInstance().GetNumQuestThisWeek(client) == 30)
                {
                    KTPlayerManager.AddExp(client, 2000000);

                    KTPlayerManager.AddBoundMoney(client, 50000, "DOQUEST | " + _taskfind.ID + "");

                    KTPlayerManager.AddMoney(client, 50000, "DOQUEST | " + _taskfind.ID + "");

                    if (!ItemManager.CreateItem(Global._TCPManager.TcpOutPacketPool, client, 188, 2, 0, "QUESTTHUONGHOI", false, 1, false, ItemManager.ConstGoodsEndTime))
                    {
                        KTPlayerManager.ShowNotification(client, "Có lỗi khi thêm vật phẩm vào túi đồ");
                    }
                }

                if (FirmTaskManager.getInstance().GetNumQuestThisWeek(client) == 40)
                {
                    KTPlayerManager.AddExp(client, 10000000);

                    KTPlayerManager.AddBoundMoney(client, 200000, "DOQUEST | " + _taskfind.ID + "");

                    KTPlayerManager.AddMoney(client, 200000, "DOQUEST | " + _taskfind.ID + "");

                    if (!ItemManager.CreateItem(Global._TCPManager.TcpOutPacketPool, client, 189, 2, 0, "QUESTTHUONGHOI", false, 1, false, ItemManager.ConstGoodsEndTime))
                    {
                        KTPlayerManager.ShowNotification(client, "Có lỗi khi thêm vật phẩm vào túi đồ");
                    }
                }

                // TODO : Nếu đủ 50 nhiệm vụ thì add cái gì
            }
        }

        private static void ProcessTransferSomething(SocketListener sl, TCPOutPacketPool pool, KPlayer client, TaskTypes taskType)
        {
            if (null == client.TaskDataList) return;

            long nowTicks = TimeUtil.NOW();

            bool updateTask = false;
            int taskid = -1;

            lock (client.TaskDataList)
            {
                for (int i = 0; i < client.TaskDataList.Count; i++)
                {
                    taskid = client.TaskDataList[i].DoingTaskID;

                    Task _FindTask = TaskManager.getInstance().FindTaskById(taskid);
                    if (_FindTask == null)
                    {
                        continue;
                    }

                    if (!IsTaskValid(client, _FindTask, client.TaskDataList[i], nowTicks))
                    {
                        continue;
                    }

                    updateTask = false;

                    //Kiểm tra điều kiện
                    if (_FindTask.TargetType == (int)TaskTypes.TransferSomething && "" != _FindTask.PropsName)
                    {
                        bool toUpdateTask = true;

                        int ItemID = Int32.Parse(_FindTask.PropsName);

                        int NumumberNeed = _FindTask.TargetNum;

                        int ItemBag = ItemManager.GetItemCountInBag(client, ItemID);

                        // Kiểm tra nếu trong túi đồ có số vật phẩm cần chuyển cho NPC này thì ok
                        if (ItemBag >= NumumberNeed)
                        {
                            client.TaskDataList[i].DoingTaskVal1 = NumumberNeed;

                            Global.ExecuteDBCmd((int)TCPGameServerCmds.CMD_DB_UPDATETASK_CMD,
                                string.Format("{0}:{1}:{2}:{3}:{4}:{5}",
                                client.RoleID,
                                client.TaskDataList[i].DoingTaskID,
                                client.TaskDataList[i].DbID,
                                client.TaskDataList[i].DoingTaskFocus,
                                client.TaskDataList[i].DoingTaskVal1,
                                client.TaskDataList[i].DoingTaskVal2),
                               client.ServerId);

                            updateTask = true;
                        }
                    }

                    if (updateTask)
                    {
                        //Update questClient State
                        KT_TCPHandler.NotifyUpdateTask(client, client.TaskDataList[i].DbID, taskid, client.TaskDataList[i].DoingTaskVal1, client.TaskDataList[i].DoingTaskVal2, client.TaskDataList[i].DoingTaskFocus);

                        //Nếu nhiệm vụ đã được hoàn thành
                        if (TaskManager.getInstance().IsQuestComplete(client.TaskDataList[i].DoingTaskID, client.TaskDataList[i].DoingTaskVal1))
                        {
                            // AddAward(_FindTask, client, pool);
                            // Update trạng thái cho thằng NPC
                            int destNPC = _FindTask.DestNPC;
                            if (-1 != destNPC)
                            {
                                int state = TaskManager.getInstance().ComputeNPCTaskState(client, client.TaskDataList, destNPC);
                                KT_TCPHandler.NotifyUpdateNPCTaskSate(client, (destNPC), state);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Hàm thực thi nhiệm vụ giết monster
        /// </summary>
        /// <param name="client"></param>
        /// <param name="npcID"></param>
        /// <param name="goodsID"></param>
        /// <param name="taskType"></param>
        private static void ProcessKillMonster(SocketListener sl, TCPOutPacketPool pool, KPlayer client, int npcID, int extensionID, int goodsID, TaskTypes taskType)
        {
            if (null == client.TaskDataList) return;

            long nowTicks = TimeUtil.NOW();

            int focusCount = TaskManager.GetFocusTaskCount(client);

            bool updateTask = false;
            int taskid = -1;

            lock (client.TaskDataList)
            {
                for (int i = 0; i < client.TaskDataList.Count; i++)
                {
                    taskid = client.TaskDataList[i].DoingTaskID;

                    Task _FindTask = TaskManager.getInstance().FindTaskById(taskid);
                    if (_FindTask == null)
                    {
                        continue;
                    }
                    //Kiểm tra xem nhiệm vụ còn hiệu lực hay không
                    if (!IsTaskValid(client, _FindTask, client.TaskDataList[i], nowTicks))
                    {
                        continue;
                    }

                    // Nếu mà NPC truyền vào là con cần giết
                    if (extensionID == _FindTask.TargetNPC)
                    {
                        updateTask = false;

                        //Kiểm tra điều kiện cơ bản
                        if (_FindTask.TargetType == (int)TaskTypes.KillMonster)
                        {
                            // nếu số lượng mà chưa đạt yêu cầu
                            if (client.TaskDataList[i].DoingTaskVal1 < _FindTask.TargetNum)
                            {
                                // update số lượng
                                client.TaskDataList[i].DoingTaskVal1++;

                                KTPlayerManager.ShowNotification(client, "Đã tiêu diệt :" + client.TaskDataList[i].DoingTaskVal1 + "/" + _FindTask.TargetNum);

                                if (focusCount < KTGlobal.TaskMaxFocusCount && client.TaskDataList[i].DoingTaskFocus <= 0)
                                {
                                    focusCount++;
                                    client.TaskDataList[i].DoingTaskFocus = 1;
                                }

                                //Update vào DB
                                Global.ExecuteDBCmd((int)TCPGameServerCmds.CMD_DB_UPDATETASK_CMD,
                                   string.Format("{0}:{1}:{2}:{3}:{4}:{5}",
                                    client.RoleID,
                                    client.TaskDataList[i].DoingTaskID,
                                    client.TaskDataList[i].DbID,
                                    client.TaskDataList[i].DoingTaskFocus,
                                    client.TaskDataList[i].DoingTaskVal1,
                                    client.TaskDataList[i].DoingTaskVal2),
                                     client.ServerId);

                                updateTask = true;
                            }
                        }
                        else if (_FindTask.TargetType == (int)TaskTypes.MonsterSomething) // Nếu mà cần giết quái để được cái gì đó
                        {
                            LogManager.WriteLog(LogTypes.Task, "CALL MonsterSomething");
                            int randNum = KTGlobal.GetRandomNumber(0, 100);
                            // lấy ra tỉ lệ rơi của đồ
                            int randRange = _FindTask.FallPercent;
                            // nếu mà rơi thì
                            if (client.TaskDataList[i].DoingTaskVal1 < _FindTask.TargetNum)
                            {
                                bool toUpdateTask = false;
                                //Lấy ra vật phẩm cần rơi ra map
                                int goodsName = Int32.Parse(_FindTask.PropsName);

                                // nếu mà rơi thì
                                if (randNum < randRange)
                                {
                                    if (KTGlobal.IsHaveSpace(1, client))
                                    {
                                        if (!ItemManager.CreateItem(pool, client, goodsName, 1, 0, "QUEST|" + _FindTask.ID, true, 0, false, ItemManager.ConstGoodsEndTime, "", -1, "", 0, 1, false))
                                        {
                                            toUpdateTask = false;
                                        }
                                        else
                                        {
                                            toUpdateTask = true;
                                        }
                                    }
                                    else
                                    {
                                        KTPlayerManager.ShowNotification(client, "Không đủ chỗ chống để nhận nguyên liệu");
                                        // Nếu không add được vật phẩm cho người chơi thì ngừng hết
                                        toUpdateTask = false;
                                    }
                                }

                                if (toUpdateTask)
                                {
                                    client.TaskDataList[i].DoingTaskVal1 = ItemManager.GetItemCountInBag(client, goodsName);

                                    ItemData _ItemTemplate = ItemManager.GetItemTemplate(goodsName);

                                    if (_ItemTemplate != null)
                                    {
                                        KTPlayerManager.ShowNotification(client, "Nhận được [" + _ItemTemplate.Name + ":" + client.TaskDataList[i].DoingTaskVal1 + "/" + _FindTask.TargetNum);
                                    }
                                    if (focusCount < KTGlobal.TaskMaxFocusCount && client.TaskDataList[i].DoingTaskFocus <= 0)
                                    {
                                        focusCount++;
                                        client.TaskDataList[i].DoingTaskFocus = 1;
                                    }

                                    //Thực hiện update vào DB 1 cái
                                    Global.ExecuteDBCmd((int)TCPGameServerCmds.CMD_DB_UPDATETASK_CMD,
                                       string.Format("{0}:{1}:{2}:{3}:{4}:{5}",
                                        client.RoleID,
                                        client.TaskDataList[i].DoingTaskID,
                                        client.TaskDataList[i].DbID,
                                        client.TaskDataList[i].DoingTaskFocus,
                                        client.TaskDataList[i].DoingTaskVal1,
                                        client.TaskDataList[i].DoingTaskVal2),
                                        client.ServerId);

                                    updateTask = true;
                                }
                            }
                            else
                            {
                                LogManager.WriteLog(LogTypes.Task, "CALL KHONG VAO");
                            }
                        }

                        if (updateTask)
                        {
                            //Update questClient State
                            KT_TCPHandler.NotifyUpdateTask(client, client.TaskDataList[i].DbID, taskid, client.TaskDataList[i].DoingTaskVal1, client.TaskDataList[i].DoingTaskVal2, client.TaskDataList[i].DoingTaskFocus);
                            //Nếu nhiệm vụ đã được hoàn thành
                            if (TaskManager.getInstance().IsQuestComplete(client.TaskDataList[i].DoingTaskID, client.TaskDataList[i].DoingTaskVal1))
                            {
                                // AddAward(_FindTask, client, pool);
                                // Update trạng thái cho thằng NPC
                                int destNPC = _FindTask.DestNPC;
                                if (-1 != destNPC)
                                {
                                    int state = TaskManager.getInstance().ComputeNPCTaskState(client, client.TaskDataList, destNPC);
                                    KT_TCPHandler.NotifyUpdateNPCTaskSate(client, destNPC, state);
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Hàm này thanh làm ra để tránh việc trong túi đồ đã có vật phẩm rồi
        /// nhưng khi nhận quest thì lại báo chưa hoàn thành
        /// Update 10/9/2021
        /// </summary>
        /// <param name="sl"></param>
        /// <param name="pool"></param>
        /// <param name="client"></param>
        public static void ProseccTaskBeforeDoTask(SocketListener sl, TCPOutPacketPool pool, KPlayer client)
        {
            if (null == client.TaskDataList) return;

            long nowTicks = TimeUtil.NOW();

            int focusCount = TaskManager.GetFocusTaskCount(client);

            bool updateTask = false;

            lock (client.TaskDataList)
            {
                for (int i = 0; i < client.TaskDataList.Count; i++)
                {
                    int taskid = client.TaskDataList[i].DoingTaskID;

                    Task _FindTask = TaskManager.getInstance().FindTaskById(taskid);
                    if (_FindTask == null)
                    {
                        continue;
                    }
                    //Kiểm tra xem nhiệm vụ còn hiệu lực hay không
                    if (!IsTaskValid(client, _FindTask, client.TaskDataList[i], nowTicks))
                    {
                        continue;
                    }

                    if (_FindTask.TargetType == (int)TaskTypes.MonsterSomething || _FindTask.TargetType == (int)TaskTypes.GetSomething || _FindTask.TargetType == (int)TaskTypes.BuySomething || _FindTask.TargetType == (int)TaskTypes.Collect || _FindTask.TargetType == (int)TaskTypes.TransferSomething || _FindTask.TargetType == (int)TaskTypes.Crafting)
                    {
                        // Nếu nhiệm vụ này có vật phẩm
                        if (_FindTask.PropsName != "")
                        {
                            // lấy ra vật phẩm cần
                            int goodsName = Int32.Parse(_FindTask.PropsName);

                            // Đếm số lượng vật phẩm trong túi có để update tình trạng cho quest
                            int Number = ItemManager.GetItemCountInBag(client, goodsName);

                            // nếu có vật phẩm trong người
                            if (Number > 0)
                            {
                                // Nếu số lượng value ở task đang chưa đủ so với số yêu cầu
                                if (client.TaskDataList[i].DoingTaskVal1 < _FindTask.TargetNum)
                                {
                                    // Update số lượng của nhiệm vụ = số mới
                                    client.TaskDataList[i].DoingTaskVal1 = Number;

                                    //Update lại số lượng này vào DB
                                    Global.ExecuteDBCmd((int)TCPGameServerCmds.CMD_DB_UPDATETASK_CMD,
                                  string.Format("{0}:{1}:{2}:{3}:{4}:{5}",
                                   client.RoleID,
                                   client.TaskDataList[i].DoingTaskID,
                                   client.TaskDataList[i].DbID,
                                   client.TaskDataList[i].DoingTaskFocus,
                                   client.TaskDataList[i].DoingTaskVal1,
                                   client.TaskDataList[i].DoingTaskVal2),
                                   client.ServerId);

                                    updateTask = true;
                                }
                            }
                        }

                        // Nếu có sự thay đổi của task thì thông tin lại về client
                    } // Nếu nhiệm vụ này là tìm đồ
                    else if (_FindTask.TargetType == (int)TaskTypes.GetItemWithSpcecialLine)
                    {
                        string DataRequest = _FindTask.PropsName;

                        string[] Pram = DataRequest.Split('|');
                        int ItemType = Int32.Parse(Pram[0]);
                        int ItemCatagory = -1;
                        if (ItemType == 0 || ItemType == 1)
                        {
                            ItemCatagory = Int32.Parse(Pram[1]);
                        }

                        int ItemSeries = Int32.Parse(Pram[2]);
                        int SymboyID = Int32.Parse(Pram[3]);
                        int ValueSymboy = Int32.Parse(Pram[4]);



                        List<GoodsData> TotalItemHave = ItemManager.GetItemByRequest(client, ItemType, ItemCatagory, ItemSeries);

                        if (TotalItemHave != null)
                        {
                            foreach (GoodsData goodsData in TotalItemHave)
                            {
                                KItem goodItem = new KItem(goodsData);
                                if (goodItem.IsExisSymboy(SymboyID, ValueSymboy))
                                {
                                    updateTask = true;

                                    client.TaskDataList[i].DoingTaskVal1++;

                                    //Thực hiện update vào DB
                                    Global.ExecuteDBCmd((int)TCPGameServerCmds.CMD_DB_UPDATETASK_CMD,
                                        string.Format("{0}:{1}:{2}:{3}:{4}:{5}",
                                        client.RoleID,
                                        client.TaskDataList[i].DoingTaskID,
                                        client.TaskDataList[i].DbID,
                                        client.TaskDataList[i].DoingTaskFocus,
                                        client.TaskDataList[i].DoingTaskVal1,
                                        client.TaskDataList[i].DoingTaskVal2),
                                         client.ServerId);

                                    break;
                                }
                            }
                        }
                    }
                    // Nếu mà có update trạng thái quest về client
                    if (updateTask)
                    {
                        KT_TCPHandler.NotifyUpdateTask(client, client.TaskDataList[i].DbID, taskid, client.TaskDataList[i].DoingTaskVal1, client.TaskDataList[i].DoingTaskVal2, client.TaskDataList[i].DoingTaskFocus);
                        //Nếu nhiệm vụ đã được hoàn thành
                        if (TaskManager.getInstance().IsQuestComplete(client.TaskDataList[i].DoingTaskID, client.TaskDataList[i].DoingTaskVal1))
                        {   // Tính toán lại trạng thái cho NPC
                            int destNPC = _FindTask.DestNPC;
                            if (-1 != destNPC)
                            {
                                int state = TaskManager.getInstance().ComputeNPCTaskState(client, client.TaskDataList, destNPC);
                                KT_TCPHandler.NotifyUpdateNPCTaskSate(client, destNPC, state);
                            }
                        }
                    }
                }
            }
        }

        private static void ProsecCollect(SocketListener sl, TCPOutPacketPool pool, KPlayer client, int npcID, int extensionID, int goodsID, TaskTypes taskType)
        {
            if (null == client.TaskDataList) return;

            long nowTicks = TimeUtil.NOW();

            int focusCount = TaskManager.GetFocusTaskCount(client);

            bool updateTask = false;
            int taskid = -1;

            lock (client.TaskDataList)
            {
                for (int i = 0; i < client.TaskDataList.Count; i++)
                {
                    taskid = client.TaskDataList[i].DoingTaskID;

                    Task _FindTask = TaskManager.getInstance().FindTaskById(taskid);
                    if (_FindTask == null)
                    {
                        continue;
                    }
                    //Kiểm tra xem nhiệm vụ còn hiệu lực hay không
                    if (!IsTaskValid(client, _FindTask, client.TaskDataList[i], nowTicks))
                    {
                        continue;
                    }

                    // Nếu mà NPC truyền vào là con cần giết
                    if (extensionID == _FindTask.TargetNPC)
                    {
                        updateTask = false;

                        //Kiểm tra điều kiện cơ bản
                        if (_FindTask.TargetType == (int)TaskTypes.Collect)
                        {
                            if (client.TaskDataList[i].DoingTaskVal1 <= _FindTask.TargetNum)
                            {
                                bool toUpdateTask = false;

                                if (!string.IsNullOrEmpty(_FindTask.PropsName))
                                {
                                    //Lấy ra vật phẩm cần rơi ra map
                                    int ItemNeedConnect = Int32.Parse(_FindTask.PropsName);

                                    // Số lượng vật cần lấy
                                    int ItemNeedGet = _FindTask.TargetNum;

                                    int CountInBag = ItemManager.GetItemCountInBag(client, ItemNeedConnect);

                                    if (CountInBag > 0)
                                    {
                                        if (client.TaskDataList[i].DoingTaskVal1 != CountInBag)
                                        {
                                            client.TaskDataList[i].DoingTaskVal1 = CountInBag;
                                            toUpdateTask = true;
                                        }
                                    }

                                    if (toUpdateTask)
                                    {
                                        if (focusCount < KTGlobal.TaskMaxFocusCount && client.TaskDataList[i].DoingTaskFocus <= 0)
                                        {
                                            focusCount++;
                                            client.TaskDataList[i].DoingTaskFocus = 1;
                                        }

                                        //Thực hiện update vào DB 1 cái
                                        Global.ExecuteDBCmd((int)TCPGameServerCmds.CMD_DB_UPDATETASK_CMD,
                                           string.Format("{0}:{1}:{2}:{3}:{4}:{5}",
                                            client.RoleID,
                                            client.TaskDataList[i].DoingTaskID,
                                            client.TaskDataList[i].DbID,
                                            client.TaskDataList[i].DoingTaskFocus,
                                            client.TaskDataList[i].DoingTaskVal1,
                                            client.TaskDataList[i].DoingTaskVal2),
                                            client.ServerId);

                                        updateTask = true;
                                    }
                                }
                                else
                                {
                                    LogManager.WriteLog(LogTypes.Quest, "Toac Task Collect => " + taskid);
                                }
                            }
                        }

                        if (updateTask)
                        {
                            //Update questClient State
                            KT_TCPHandler.NotifyUpdateTask(client, client.TaskDataList[i].DbID, taskid, client.TaskDataList[i].DoingTaskVal1, client.TaskDataList[i].DoingTaskVal2, client.TaskDataList[i].DoingTaskFocus);
                            //Nếu nhiệm vụ đã được hoàn thành
                            if (TaskManager.getInstance().IsQuestComplete(client.TaskDataList[i].DoingTaskID, client.TaskDataList[i].DoingTaskVal1))
                            {
                                // AddAward(_FindTask, client, pool);
                                // Update trạng thái cho thằng NPC
                                int destNPC = _FindTask.DestNPC;
                                if (-1 != destNPC)
                                {
                                    int state = TaskManager.getInstance().ComputeNPCTaskState(client, client.TaskDataList, destNPC);
                                    KT_TCPHandler.NotifyUpdateNPCTaskSate(client, destNPC, state);
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Thực hiến cường hóa cái gì đó
        /// </summary>
        /// <param name="client"></param>
        /// <param name="npcID"></param>
        /// <param name="goodsID"></param>
        /// <param name="taskType"></param>
        private static void ProseccEnhance(SocketListener sl, TCPOutPacketPool pool, KPlayer client, int npcID, int extensionID, int goodsID, TaskTypes taskType)
        {
            if (null == client.TaskDataList) return;
            if (-1 == goodsID) return;

            long nowTicks = TimeUtil.NOW();

            bool updateTask = false;
            int taskid = -1;
            SystemXmlItem systemTask = null;

            lock (client.TaskDataList)
            {
                for (int i = 0; i < client.TaskDataList.Count; i++)
                {
                    taskid = client.TaskDataList[i].DoingTaskID;

                    Task _FindTask = TaskManager.getInstance().FindTaskById(taskid);
                    if (_FindTask == null)
                    {
                        continue;
                    }

                    //Kiểm tra xem nhiệm vụ còn hiệu lực hay không
                    if (!IsTaskValid(client, _FindTask, client.TaskDataList[i], nowTicks))
                    {
                        continue;
                    }

                    //Kiểm tra điều kiện cơ bản
                    if (_FindTask.TargetType == (int)TaskTypes.Enhance)
                    {
                        updateTask = false;

                        int GoodName = Int32.Parse(_FindTask.PropsName);
                        // Nếu vật phẩm cường hóa đúng với vật phẩm yêu cầu
                        if (goodsID == GoodName)
                        {
                            int NumberRequest = _FindTask.TargetNum;

                            int TotalItemHave = ItemManager.GetItemCountInBag(client, GoodName);

                            // Có thể nó mua phát đủ luôn thì sao?
                            if (TotalItemHave > 0)
                            {
                                // Nếu số lượng yêu cầu còn lớn hơn số lượng mà mình cso
                                if (_FindTask.TargetNum <= TotalItemHave)
                                {
                                    client.TaskDataList[i].DoingTaskVal1 = TotalItemHave;

                                    //Cập nhật trạng thái QUEST
                                    Global.ExecuteDBCmd((int)TCPGameServerCmds.CMD_DB_UPDATETASK_CMD,
                                        string.Format("{0}:{1}:{2}:{3}:{4}:{5}",
                                        client.RoleID,
                                        client.TaskDataList[i].DoingTaskID,
                                        client.TaskDataList[i].DbID,
                                        client.TaskDataList[i].DoingTaskFocus,
                                        client.TaskDataList[i].DoingTaskVal1,
                                        client.TaskDataList[i].DoingTaskVal2),
                                         client.ServerId);

                                    updateTask = true;
                                }
                            }

                            if (updateTask)
                            {
                                //Update questClient State
                                KT_TCPHandler.NotifyUpdateTask(client, client.TaskDataList[i].DbID, taskid, client.TaskDataList[i].DoingTaskVal1, client.TaskDataList[i].DoingTaskVal2, client.TaskDataList[i].DoingTaskFocus);
                                //Nếu nhiệm vụ đã được hoàn thành
                                if (TaskManager.getInstance().IsQuestComplete(client.TaskDataList[i].DoingTaskID, client.TaskDataList[i].DoingTaskVal1))
                                {
                                    //AddAward(_FindTask, client, pool);
                                    // Update trạng thái cho thằng NPC
                                    int destNPC = _FindTask.DestNPC;
                                    if (-1 != destNPC)
                                    {
                                        int state = TaskManager.getInstance().ComputeNPCTaskState(client, client.TaskDataList, destNPC);
                                        KT_TCPHandler.NotifyUpdateNPCTaskSate(client, (destNPC), state);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Thực hiến chế tạo cái gì đó
        /// </summary>
        /// <param name="client"></param>
        /// <param name="npcID"></param>
        /// <param name="goodsID"></param>
        /// <param name="taskType"></param>
        private static void ProseccCrafting(SocketListener sl, TCPOutPacketPool pool, KPlayer client, int npcID, int extensionID, int goodsID, TaskTypes taskType)
        {
            if (null == client.TaskDataList) return;
            if (-1 == goodsID) return;

            long nowTicks = TimeUtil.NOW();

            bool updateTask = false;
            int taskid = -1;

            lock (client.TaskDataList)
            {
                for (int i = 0; i < client.TaskDataList.Count; i++)
                {
                    taskid = client.TaskDataList[i].DoingTaskID;

                    Task _FindTask = TaskManager.getInstance().FindTaskById(taskid);
                    if (_FindTask == null)
                    {
                        continue;
                    }

                    //Kiểm tra xem nhiệm vụ còn hiệu lực hay không
                    if (!IsTaskValid(client, _FindTask, client.TaskDataList[i], nowTicks))
                    {
                        continue;
                    }

                    //Kiểm tra điều kiện cơ bản
                    if (_FindTask.TargetType == (int)TaskTypes.Crafting)
                    {
                        updateTask = false;

                        int GoodName = Int32.Parse(_FindTask.PropsName);
                        // Nếu vật phẩm chế ra = với vật phẩm yêu cầu trong tast
                        if (goodsID == GoodName)
                        {
                            int NumberRequest = _FindTask.TargetNum;

                            int TotalItemHave = ItemManager.GetItemCountInBag(client, GoodName);

                            // Có thể nó mua phát đủ luôn thì sao?
                            if (TotalItemHave > 0)
                            {
                                // Nếu số lượng yêu cầu còn lớn hơn số lượng mà mình cso
                                if (_FindTask.TargetNum <= TotalItemHave)
                                {
                                    client.TaskDataList[i].DoingTaskVal1 = TotalItemHave;

                                    //Cập nhật trạng thái QUEST
                                    Global.ExecuteDBCmd((int)TCPGameServerCmds.CMD_DB_UPDATETASK_CMD,
                                        string.Format("{0}:{1}:{2}:{3}:{4}:{5}",
                                        client.RoleID,
                                        client.TaskDataList[i].DoingTaskID,
                                        client.TaskDataList[i].DbID,
                                        client.TaskDataList[i].DoingTaskFocus,
                                        client.TaskDataList[i].DoingTaskVal1,
                                        client.TaskDataList[i].DoingTaskVal2),
                                         client.ServerId);

                                    updateTask = true;
                                }
                            }

                            if (updateTask)
                            {
                                //Update questClient State
                                KT_TCPHandler.NotifyUpdateTask(client, client.TaskDataList[i].DbID, taskid, client.TaskDataList[i].DoingTaskVal1, client.TaskDataList[i].DoingTaskVal2, client.TaskDataList[i].DoingTaskFocus);
                                //Nếu nhiệm vụ đã được hoàn thành
                                if (TaskManager.getInstance().IsQuestComplete(client.TaskDataList[i].DoingTaskID, client.TaskDataList[i].DoingTaskVal1))
                                {
                                    //AddAward(_FindTask, client, pool);
                                    // Update trạng thái cho thằng NPC
                                    int destNPC = _FindTask.DestNPC;
                                    if (-1 != destNPC)
                                    {
                                        int state = TaskManager.getInstance().ComputeNPCTaskState(client, client.TaskDataList, destNPC);
                                        KT_TCPHandler.NotifyUpdateNPCTaskSate(client, (destNPC), state);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Thực hiện mua cái j đó
        /// </summary>
        /// <param name="client"></param>
        /// <param name="npcID"></param>
        /// <param name="goodsID"></param>
        /// <param name="taskType"></param>
        private static void ProcessBuy(SocketListener sl, TCPOutPacketPool pool, KPlayer client, int npcID, int extensionID, int goodsID, TaskTypes taskType)
        {
            if (null == client.TaskDataList) return;
            if (-1 == goodsID) return;

            long nowTicks = TimeUtil.NOW();

            bool updateTask = false;
            int taskid = -1;
            SystemXmlItem systemTask = null;

            lock (client.TaskDataList)
            {
                for (int i = 0; i < client.TaskDataList.Count; i++)
                {
                    taskid = client.TaskDataList[i].DoingTaskID;

                    Task _FindTask = TaskManager.getInstance().FindTaskById(taskid);
                    if (_FindTask == null)
                    {
                        continue;
                    }

                    //Kiểm tra xem nhiệm vụ còn hiệu lực hay không
                    if (!IsTaskValid(client, _FindTask, client.TaskDataList[i], nowTicks))
                    {
                        continue;
                    }

                    //Kiểm tra điều kiện cơ bản
                    if (_FindTask.TargetType == (int)TaskTypes.BuySomething)
                    {
                        updateTask = false;

                        int GoodName = Int32.Parse(_FindTask.PropsName);
                        // Nếu vật phẩm cần mua giống vật phẩm yêu cầu
                        if (goodsID == GoodName)
                        {
                            int NumberRequest = _FindTask.TargetNum;

                            int TotalItemHave = ItemManager.GetItemCountInBag(client, GoodName);

                            // Có thể nó mua phát đủ luôn thì sao?
                            if (TotalItemHave > 0)
                            {
                                client.TaskDataList[i].DoingTaskVal1 = TotalItemHave;

                                //Cập nhật trạng thái QUEST
                                Global.ExecuteDBCmd((int)TCPGameServerCmds.CMD_DB_UPDATETASK_CMD,
                                    string.Format("{0}:{1}:{2}:{3}:{4}:{5}",
                                    client.RoleID,
                                    client.TaskDataList[i].DoingTaskID,
                                    client.TaskDataList[i].DbID,
                                    client.TaskDataList[i].DoingTaskFocus,
                                    client.TaskDataList[i].DoingTaskVal1,
                                    client.TaskDataList[i].DoingTaskVal2),
                                     client.ServerId);

                                updateTask = true;
                            }

                            if (updateTask)
                            {
                                //Update questClient State
                                KT_TCPHandler.NotifyUpdateTask(client, client.TaskDataList[i].DbID, taskid, client.TaskDataList[i].DoingTaskVal1, client.TaskDataList[i].DoingTaskVal2, client.TaskDataList[i].DoingTaskFocus);
                                //Nếu nhiệm vụ đã được hoàn thành
                                if (TaskManager.getInstance().IsQuestComplete(client.TaskDataList[i].DoingTaskID, client.TaskDataList[i].DoingTaskVal1))
                                {
                                    //AddAward(_FindTask, client, pool);
                                    // Update trạng thái cho thằng NPC
                                    int destNPC = _FindTask.DestNPC;
                                    if (-1 != destNPC)
                                    {
                                        int state = TaskManager.getInstance().ComputeNPCTaskState(client, client.TaskDataList, destNPC);
                                        KT_TCPHandler.NotifyUpdateNPCTaskSate(client, (destNPC), state);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private static Point StrToPoint(string str)
        {
            if (string.IsNullOrEmpty(str))
                return new Point(double.NaN, double.NaN);
            str = str.Trim();
            if (str == "")
                return new Point(double.NaN, double.NaN);

            string[] fields = str.Split('|');
            if (fields.Length != 2)
                return new Point(double.NaN, double.NaN);

            try
            {
                string str1 = fields[0].Trim();
                str1 = string.IsNullOrEmpty(str) ? "0" : str1;

                string str2 = fields[1].Trim();
                str2 = string.IsNullOrEmpty(str) ? "0" : str2;
                return new Point(Convert.ToDouble(str1), Convert.ToDouble(str2));
            }
            catch (Exception)
            {
            }

            return new Point(double.NaN, double.NaN);
        }

        /// <summary>
        /// Thực hiện nhiệm vụ sử dụng cái j đó
        /// </summary>
        /// <param name="client"></param>
        /// <param name="npcID"></param>
        /// <param name="goodsID"></param>
        /// <param name="taskType"></param>
        private static void ProcessUsingSomething(SocketListener sl, TCPOutPacketPool pool, KPlayer client, int npcID, int extensionID, int goodsID, TaskTypes taskType)
        {
            if (null == client.TaskDataList) return;
            if (-1 == goodsID) return;

            long nowTicks = TimeUtil.NOW();

            bool updateTask = false;

            int taskid = -1;

            SystemXmlItem systemTask = null;

            GameMap gameMap = KTMapManager.Find(client.MapCode);

            lock (client.TaskDataList)
            {
                for (int i = 0; i < client.TaskDataList.Count; i++)
                {
                    taskid = client.TaskDataList[i].DoingTaskID;

                    Task _FindTask = TaskManager.getInstance().FindTaskById(taskid);
                    if (_FindTask == null)
                    {
                        continue;
                    }

                    //Kiểm tra xem nhiệm vụ còn hiệu lực hay không
                    if (!IsTaskValid(client, _FindTask, client.TaskDataList[i], nowTicks))
                    {
                        continue;
                    }

                    //Kiểm tra điều kiện cơ bản
                    if (_FindTask.TargetType == (int)TaskTypes.UseSomething)
                    {
                        updateTask = false;

                        //Lấy ra vật phẩm cần sử dụng
                        int goodsName = Int32.Parse(_FindTask.PropsName);

                        //Lấy ra bản đồ sử dụng
                        int targetMapCode1 = _FindTask.TargetMapCode;

                        // lấy ra tọa độ sử dụng
                        Point targetPos1 = ProcessTask.StrToPoint(_FindTask.TargetPos);

                        bool RequestPostion = false;

                        if (targetPos1.X > 0 && targetPos1.Y > 0)
                        {
                            RequestPostion = true;
                        }

                        if (goodsID == goodsName)
                        {
                            if (RequestPostion)
                            {
                                Point clientGrid = client.CurrentGrid;

                                Point usingGoodsGrid = new Point((int)(targetPos1.X / gameMap.MapGridWidth), (int)(targetPos1.Y / gameMap.MapGridHeight));

                                bool inGrid = Math.Abs(usingGoodsGrid.X - clientGrid.X) < 3 && Math.Abs(usingGoodsGrid.Y - clientGrid.Y) < 3;

                                if (client.TaskDataList[i].DoingTaskVal1 < _FindTask.TargetNum)
                                {
                                    client.TaskDataList[i].DoingTaskVal1++;

                                    //Thực hiện update vào DB
                                    Global.ExecuteDBCmd((int)TCPGameServerCmds.CMD_DB_UPDATETASK_CMD,
                                        string.Format("{0}:{1}:{2}:{3}:{4}:{5}",
                                        client.RoleID,
                                        client.TaskDataList[i].DoingTaskID,
                                        client.TaskDataList[i].DbID,
                                        client.TaskDataList[i].DoingTaskFocus,
                                        client.TaskDataList[i].DoingTaskVal1,
                                        client.TaskDataList[i].DoingTaskVal2),
                                         client.ServerId);

                                    updateTask = true;
                                }
                            }
                            else // Nếu không yêu cầu vị trí
                            {
                                client.TaskDataList[i].DoingTaskVal1++;

                                //Thực hiện update vào DB
                                Global.ExecuteDBCmd((int)TCPGameServerCmds.CMD_DB_UPDATETASK_CMD,
                                    string.Format("{0}:{1}:{2}:{3}:{4}:{5}",
                                    client.RoleID,
                                    client.TaskDataList[i].DoingTaskID,
                                    client.TaskDataList[i].DbID,
                                    client.TaskDataList[i].DoingTaskFocus,
                                    client.TaskDataList[i].DoingTaskVal1,
                                    client.TaskDataList[i].DoingTaskVal2),
                                     client.ServerId);

                                updateTask = true;
                            }
                        }
                        if (updateTask)
                        {
                            //Update questClient State
                            KT_TCPHandler.NotifyUpdateTask(client, client.TaskDataList[i].DbID, taskid, client.TaskDataList[i].DoingTaskVal1, client.TaskDataList[i].DoingTaskVal2, client.TaskDataList[i].DoingTaskFocus);
                            //Nếu nhiệm vụ đã được hoàn thành
                            if (TaskManager.getInstance().IsQuestComplete(client.TaskDataList[i].DoingTaskID, client.TaskDataList[i].DoingTaskVal1))
                            {
                                //AddAward(_FindTask, client, pool);
                                // Update trạng thái cho thằng NPC
                                int destNPC = _FindTask.DestNPC;
                                if (-1 != destNPC)
                                {
                                    int state = TaskManager.getInstance().ComputeNPCTaskState(client, client.TaskDataList, destNPC);
                                    KT_TCPHandler.NotifyUpdateNPCTaskSate(client, (destNPC), state);
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Thực hiện task value
        /// </summary>
        /// <param name="client"></param>
        /// <param name="npcID"></param>
        /// <param name="goodsID"></param>
        /// <param name="taskType"></param>
        public static void ProcessTaskValue(SocketListener sl, TCPOutPacketPool pool, KPlayer client, string taskName, int valType, int taskVal)
        {
            if (null == client.TaskDataList) return;

            int taskid = -1;
            SystemXmlItem systemTask = null;

            lock (client.TaskDataList)
            {
                for (int i = 0; i < client.TaskDataList.Count; i++)
                {
                    taskid = client.TaskDataList[i].DoingTaskID;

                    Task _FindTask = TaskManager.getInstance().FindTaskById(taskid);
                    if (_FindTask == null)
                    {
                        continue;
                    }

                    //Nếu không có tên nhiệm vụ thì bỏ qua
                    if (taskName != _FindTask.Title)
                    {
                        continue;
                    }

                    if (1 == valType) //Giết Monster
                    {
                        bool updateTask = false;

                        if (client.TaskDataList[i].DoingTaskVal1 < _FindTask.TargetNum)
                        {
                            client.TaskDataList[i].DoingTaskVal1 = Math.Min(taskVal, _FindTask.TargetNum);

                            //Thực hiện update lại vào DB
                            Global.ExecuteDBCmd((int)TCPGameServerCmds.CMD_DB_UPDATETASK_CMD,
                                string.Format("{0}:{1}:{2}:{3}:{4}:{5}",
                                client.RoleID,
                                client.TaskDataList[i].DoingTaskID,
                                client.TaskDataList[i].DbID,
                                client.TaskDataList[i].DoingTaskFocus,
                                client.TaskDataList[i].DoingTaskVal1,
                                client.TaskDataList[i].DoingTaskVal2),
                                client.ServerId);

                            updateTask = true;
                        }

                        if (updateTask)
                        {
                            //Update questClient State
                            KT_TCPHandler.NotifyUpdateTask(client, client.TaskDataList[i].DbID, taskid, client.TaskDataList[i].DoingTaskVal1, client.TaskDataList[i].DoingTaskVal2, client.TaskDataList[i].DoingTaskFocus);
                            //Nếu nhiệm vụ đã được hoàn thành
                            if (TaskManager.getInstance().IsQuestComplete(client.TaskDataList[i].DoingTaskID, client.TaskDataList[i].DoingTaskVal1))
                            {
                                //AddAward(_FindTask, client, pool);
                                // Update trạng thái cho thằng NPC
                                int destNPC = _FindTask.DestNPC;
                                if (-1 != destNPC)
                                {
                                    int state = TaskManager.getInstance().ComputeNPCTaskState(client, client.TaskDataList, destNPC);
                                    KT_TCPHandler.NotifyUpdateNPCTaskSate(client, (destNPC), state);
                                }
                            }
                        }
                    }
                    else if (2 == valType) // //Giết monster và lấy cái j đó
                    {
                        bool updateTask = false;

                        if (client.TaskDataList[i].DoingTaskVal1 < _FindTask.TargetNum)
                        {
                            client.TaskDataList[i].DoingTaskVal1 = Math.Min(taskVal, _FindTask.TargetNum);

                            Global.ExecuteDBCmd((int)TCPGameServerCmds.CMD_DB_UPDATETASK_CMD,
                                string.Format("{0}:{1}:{2}:{3}:{4}:{5}",
                                client.RoleID,
                                client.TaskDataList[i].DoingTaskID,
                                client.TaskDataList[i].DbID,
                                client.TaskDataList[i].DoingTaskFocus,
                                client.TaskDataList[i].DoingTaskVal1,
                                client.TaskDataList[i].DoingTaskVal2),
                                client.ServerId);
                            updateTask = true;
                        }

                        if (updateTask)
                        {
                            //Update questClient State
                            KT_TCPHandler.NotifyUpdateTask(client, client.TaskDataList[i].DbID, taskid, client.TaskDataList[i].DoingTaskVal1, client.TaskDataList[i].DoingTaskVal2, client.TaskDataList[i].DoingTaskFocus);
                            //Nếu nhiệm vụ đã được hoàn thành
                            if (TaskManager.getInstance().IsQuestComplete(client.TaskDataList[i].DoingTaskID, client.TaskDataList[i].DoingTaskVal1))
                            {
                                // AddAward(_FindTask, client, pool);
                                // Update trạng thái cho thằng NPC
                                int destNPC = _FindTask.DestNPC;
                                if (-1 != destNPC)
                                {
                                    int state = TaskManager.getInstance().ComputeNPCTaskState(client, client.TaskDataList, destNPC);
                                    KT_TCPHandler.NotifyUpdateNPCTaskSate(client, (destNPC), state);
                                }
                            }
                        }
                    }

                    break;
                }
            }
        }

        public static void GMSetMainTaskID(KPlayer client, int taskID = 100)
        {
            int roleID = client.RoleID;

            client.OldTasks = new List<OldTaskData>();

            client.TaskDataList = new List<TaskData>();

            int mainTaskID = int.MaxValue;

            int npcID = 0;

            List<int> list = new List<int>();

            foreach (Task _Task in TaskManager.getInstance()._TotalConfig.Tasks.Task)
            {
                if (_Task.ID < mainTaskID && _Task.ID > taskID)
                {
                    mainTaskID = _Task.ID;
                    npcID = _Task.DestNPC;
                }

                if (_Task.ID < taskID)
                {
                    list.Add(_Task.ID);

                    client.OldTasks.Add(new OldTaskData()
                    {
                        TaskID = _Task.ID,
                        DoCount = 1,
                    });
                }
            }

            list.Sort();
            list.Insert(0, roleID);

            Global.SendToDB<int, byte[]>((int)TCPGameServerCmds.CMD_SPR_GM_SET_MAIN_TASK, DataHelper.ObjectToBytes(list), client.ServerId);

            client.SendPacket((int)TCPGameServerCmds.CMD_SPR_COMPTASK, string.Format("{0}:{1}:{2}:{3}", roleID, npcID, list[list.Count - 1], 0));

            TCPOutPacket tcpOutPacketTemp = null;

            // Add Data
            TaskData taskData = new TaskData()
            {
                DbID = -1,
                DoingTaskID = taskID,
            };

            byte[] DataSend = DataHelper.ObjectToBytes<TaskData>(taskData);
            client.SendPacket((int)TCPGameServerCmds.CMD_SPR_NEWTASK, DataSend);

            Global.ForceCloseClient(client);
        }

        /// <summary>
        /// Xóa vật phẩm nhiệm vụ
        /// </summary>
        /// <param name="client"></param>
        /// <param name="npcID"></param>
        /// <param name="goodsID"></param>
        /// <param name="taskType"></param>
        public static void ClearTaskGoods(SocketListener sl, TCPOutPacketPool pool, KPlayer client, int taskID)
        {
            if (null == client.TaskDataList) return;
            TaskData taskData = TaskManager.GetTaskData(client, taskID);
            if (null == taskData)
            {
                return;
            }

            Task _FindTask = TaskManager.getInstance().FindTaskById(taskID);
            if (_FindTask == null)
            {
                return;
            }

            //TODO : LIỆU THẬT SỰ CẦN XÓA VẬT PHẨM KHI HỦY NHIỆM VỤ?
        }

        #endregion Với nhiệm vụ đang được thực hiện

        #region Hoàn thành task

        /// <summary>
        /// Nhấn vào sự kiện trả nhiệm vụ
        /// </summary>
        public static bool Complete(SocketListener sl, TCPOutPacketPool pool, KPlayer client, int npcID, int extensionID, int taskID, int dbID, bool useYuanBao, double expBeiShu = 1.0, bool bIsOneClickComlete = false)
        {
            if (null == client.TaskDataList) return false;

            int findIndex = -1;

            lock (client.TaskDataList)
            {
                for (int i = 0; i < client.TaskDataList.Count; i++)
                {
                    if (client.TaskDataList[i].DbID == dbID)
                    {
                        if (bIsOneClickComlete == true)
                        {
                            findIndex = i;
                            break;
                        }
                        else
                        {
                            if (TaskManager.getInstance().IsQuestComplete(taskID, client.TaskDataList[i].DoingTaskVal1))
                            {
                                findIndex = i;
                                break;
                            }
                        }
                    }
                }
            }

            if (findIndex < 0) return false;

            TaskData taskData = null;
            lock (client.TaskDataList)
            {
                if (findIndex < client.TaskDataList.Count)
                {
                    taskData = client.TaskDataList[findIndex];
                    client.TaskDataList.RemoveAt(findIndex);
                }
            }

            if (null == taskData) return false;

            // Thực hiện add OLD TASK
            TaskManager.AddOldTask(client, taskID);

            // lấy ra old task
            Task _FindTask = TaskManager.getInstance().FindTaskById(taskID);
            if (_FindTask == null)
            {
                return false;
            }

            //Thực hiện task
            int taskClass = _FindTask.TaskClass;

            //Update task data về client
            KT_TCPHandler.NotifyUpdateTask(client, taskData.DbID, taskID, -2, 0, 0);

            //Update trạng thái của NPC
            int state = TaskManager.getInstance().ComputeNPCTaskState(client, client.TaskDataList, npcID);

            KT_TCPHandler.NotifyUpdateNPCTaskSate(client, npcID, state);

            int sourceNPC = _FindTask.SourceNPC;

            if (-1 != sourceNPC)
            {
                state = TaskManager.getInstance().ComputeNPCTaskState(client, client.TaskDataList, sourceNPC);
                KT_TCPHandler.NotifyUpdateNPCTaskSate(client, sourceNPC, state);
            }

            // Nếu nhiệm vụ là đi giết quái và thu thập nguyên liệu
            if ((_FindTask.TargetType == (int)TaskTypes.MonsterSomething || _FindTask.TargetType == (int)TaskTypes.Collect || _FindTask.TargetType == (int)TaskTypes.TransferSomething) || (_FindTask.TaskClass == (int)TaskClasses.NghiaQuan && _FindTask.TargetType == (int)TaskTypes.Crafting) && "" != _FindTask.PropsName)
            {
                //Xóa vật phẩm
                int NumberRequest = _FindTask.TargetNum;
                int GoodName = Int32.Parse(_FindTask.PropsName);

                if (ItemManager.RemoveItemFromBag(client, GoodName, NumberRequest, -1, "Xóa bởi nhiệm vụ :" + _FindTask.ID))
                {
                    // LogManager.WriteLog(LogTypes.task, "Remove Item By TaskID :" + _FindTask.ID);
                }
                else
                {
                    KTPlayerManager.ShowNotification(client, "Không đủ vật phẩm để trả nhiệm vụ");
                }
            }

            // Thực hiện add phần thưởng.
            AddQuestAward(_FindTask, client, pool);

            KTGlobal.AddRoleTaskEvent(client, taskID);

            return true;
        }

        #endregion Hoàn thành task
    }
}