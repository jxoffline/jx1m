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

        /// <summary>
        /// Hàm xử lý
        /// </summary>
        public static void Process(SocketListener sl, TCPOutPacketPool pool, KPlayer client, int npcID, int extensionID, int goodsID, TaskTypes taskType)
        {
            switch (taskType)
            {
                case TaskTypes.Talk:
                case TaskTypes.GetSomething:
                case TaskTypes.NeedYuanBao:
                    ProcessTalk(sl, pool, client, npcID, extensionID, goodsID, taskType);
                    break;

                case TaskTypes.TransferSomething:
                    ProcessTransferSomething(sl, pool, client, npcID, extensionID, goodsID, taskType);
                    break;

                case TaskTypes.KillMonster:
                case TaskTypes.KillMonsterForLevel:
                case TaskTypes.MonsterSomething:
                case TaskTypes.CaiJiGoods:
                    ProcessKillMonster(sl, pool, client, npcID, extensionID, goodsID, taskType);
                    break;

                case TaskTypes.BuySomething:
                    ProcessBuy(sl, pool, client, npcID, extensionID, goodsID, taskType);
                    break;

                case TaskTypes.UseSomething:
                    ProcessUsingSomething(sl, pool, client, npcID, extensionID, goodsID, taskType);
                    break;

                case TaskTypes.ZhiLiao:
                case TaskTypes.FangHuo:
                    ProcessLuaHandle(sl, pool, client, npcID, extensionID, goodsID, taskType);
                    break;

                default:
                    break;
            }
        }

        /// <summary>
        /// 解析物品名称
        /// </summary>
        /// <param name="propName"></param>
        /// <returns></returns>
        private static string GetPropNameGoodsName(string propName)
        {
            if (string.IsNullOrEmpty(propName))
            {
                return propName;
            }

            string[] fields = propName.Split('|');
            if (fields.Length <= 1)
            {
                return propName;
            }

            return fields[0];
        }

        /// <summary>
        /// 解析物品的级别
        /// </summary>
        /// <param name="propName"></param>
        /// <returns></returns>
        private static int GetPropNameGoodsLevel(string propName)
        {
            if (string.IsNullOrEmpty(propName))
            {
                return 0;
            }

            string[] fields = propName.Split('|');
            if (fields.Length < 3)
            {
                return 0;
            }

            return Global.SafeConvertToInt32(fields[1]);
        }

        /// <summary>
        /// 解析物品的级别
        /// </summary>
        /// <param name="propName"></param>
        /// <returns></returns>
        private static int GetPropNameGoodsQuality(string propName)
        {
            if (string.IsNullOrEmpty(propName))
            {
                return (int)GoodsQuality.White;
            }

            string[] fields = propName.Split('|');
            if (fields.Length < 3)
            {
                return (int)GoodsQuality.White;
            }

            return (int)Global.GetEnchanceQualityByColorName(fields[2]);
        }

        /// <summary>
        /// Check xem task có còn hợp lệ không
        /// </summary>
        /// <param name="taskXmlNode"></param>
        /// <returns></returns>
        private static bool IsTaskValid(KPlayer client, Task systemTask, TaskData taskData, long nowTicks)
        {
            if (systemTask.MinLevel <= client.m_Level && client.m_Level <= systemTask.MaxLevel)
            {
                return true;
            }
            else
            {
                return false;
            }
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

                    Task _taskfind = TaskManager.FindTaskById(taskid);

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

                                        int ItemID = Int32.Parse(ItemPram[0]);
                                        int ItemNumber = Int32.Parse(ItemPram[1]);
                                        ItemManager.CreateItem(pool, client, ItemID, ItemNumber, 0, "QUEST|" + _taskfind.ID, false, 1, false, Global.ConstGoodsEndTime);

                                        updateTask = true;
                                    }
                                }
                                else
                                {
                                    PlayerManager.ShowNotification(client, "Túi đồ không đủ chỗ trống để nhận phần thưởng!");
                                    // Nếu không add được vật phẩm cho người chơi thì ngừng hết
                                    updateTask = false;
                                }
                            }

                            if (updateTask)
                            {
                                // THực hiện add EXP
                                GameManager.ClientMgr.ProcessRoleExperience(client, _taskfind.Experienceaward, true, false, true, "EXP");

                                int buffID = _taskfind.BuffID;

                                if (buffID > 0) //Nếu mà có buff ID thì thực hiện ADD BUFF cho người chơi
                                {
                                    // TO DO ADD BUFF VÀO NHÂN VẬT NẾU MÀ BUFF
                                }

                                if (_taskfind.BacKhoa > 0)
                                {
                                    GameManager.ClientMgr.AddUserGold(client, _taskfind.BacKhoa, "DOQUEST | " + _taskfind.ID + "");
                                }

                                // ADd EXP cho người chơi

                                if (_taskfind.Experienceaward > 0)
                                {
                                    GameManager.ClientMgr.ProcessRoleExperience(client, _taskfind.Experienceaward, true, false, true, "EXP");
                                }

                                // Thực hiện add các loại điểm danh vọng khác cho người chơi

                                // TODO Ở ĐÂY

                                //end

                                // Thực hiện update nhiệm vụ cho người chơi
                                client.TaskDataList[i].DoingTaskVal1++;

                                //Thực hiện update vào DB
                                GameManager.DBCmdMgr.AddDBCmd((int)TCPGameServerCmds.CMD_DB_UPDATETASK_CMD,
                                    string.Format("{0}:{1}:{2}:{3}:{4}:{5}",
                                    client.RoleID,
                                    client.TaskDataList[i].DoingTaskID,
                                    client.TaskDataList[i].DbID,
                                    client.TaskDataList[i].DoingTaskFocus,
                                    client.TaskDataList[i].DoingTaskVal1,
                                    client.TaskDataList[i].DoingTaskVal2),
                                    null, client.ServerId);

                                updateTask = true;
                            }
                        } 
                        else if (_taskfind.TargetType == (int)TaskTypes.GetSomething && _taskfind.PropsName != "") // Nếu nhiệm vụ là kiểu nhận vật phẩm gì đó từ NPC
                        {
                            // nếu số lượng cần lấy chưa đủ
                            if (client.TaskDataList[i].DoingTaskVal1 < _taskfind.TargetNum)
                            {
                                bool toUpdateTask = true;

                                //Lấy ra các thông số của vật phẩm cần lấy
                                string ItemID = _taskfind.PropsName;

                                int ItemNum = _taskfind.TargetNum;


                                if (KTGlobal.IsHaveSpace(1, client))
                                { }                        
                                    ItemManager.CreateItem(pool, client, ItemID, ItemNum, 0, "QUEST|" + _taskfind.ID, false, 1, false, Global.ConstGoodsEndTime);

                                    updateTask = true;
                                }
                                else
                                {
                                    PlayerManager.ShowNotification(client, "Túi đồ không đủ chỗ trống để nhận phần thưởng!");
                                    // Nếu không add được vật phẩm cho người chơi thì ngừng hết
                                    toUpdateTask = false;
                                }



                                    int transferGoodsID = Global.GetGoodsByName(goodsName);
                                if (transferGoodsID >= 0)
                                {
                                    GoodsData goodsData = Global.GetNotUsingGoodsByID(client, transferGoodsID, goodsLevel, goodsQuality);
                                    if (null == goodsData) //已经拥有该物品，不用再给
                                    {
                                        if (Global.CanAddGoods(client, transferGoodsID, 1, 1))
                                        {
                                            //将物品自动交给角色
                                            //想DBServer请求加入某个新的物品到背包中
                                            //添加物品
                                            Global.AddGoodsDBCommand(pool, client, transferGoodsID, 1, 0, "", 0, 1, 0, "", true, 1, "获取任务道具");
                                        }
                                        else
                                        {
                                            toUpdateTask = false;

                                            /// 通知在线的对方(不限制地图)个人紧要消息
                                            GameManager.ClientMgr.NotifyImportantMsg(sl, pool, client, Global.GetLang("背包已满，请先清理出空格再接受任务物品"), GameInfoTypeIndexes.Error, ShowGameInfoTypes.ErrAndBox, (int)HintErrCodeTypes.NoBagGrid);
                                        }
                                    }
                                }

                                if (
                                    
                                    
                                    
                                    
                                    
                                    
                                    
                                    
                                    
                                    
                                    
                                    
                                    
                                    
                                    
                                    
                                    )
                                {
                                    client.TaskDataList[i].DoingTaskVal1++;

                                    //异步写数据库，更新任务
                                    GameManager.DBCmdMgr.AddDBCmd((int)TCPGameServerCmds.CMD_DB_UPDATETASK_CMD,
                                        string.Format("{0}:{1}:{2}:{3}:{4}:{5}",
                                        client.RoleID,
                                        client.TaskDataList[i].DoingTaskID,
                                        client.TaskDataList[i].DbID,
                                        client.TaskDataList[i].DoingTaskFocus,
                                        client.TaskDataList[i].DoingTaskVal1,
                                        client.TaskDataList[i].DoingTaskVal2),
                                        null, client.ServerId);

                                    updateTask = true;
                                }
                            }
                        }
                        else if (systemTask.GetIntValue("TargetType1") == (int)TaskTypes.NeedYuanBao)
                        {
                            if (client.TaskDataList[i].DoingTaskVal1 < systemTask.GetIntValue("TargetNum1"))
                            {
                                int totalChongZhiMoney = GameManager.ClientMgr.QueryTotaoChongZhiMoney(client);
                                if (totalChongZhiMoney > 0)
                                {
                                    client.TaskDataList[i].DoingTaskVal1++;

                                    //异步写数据库，更新任务
                                    GameManager.DBCmdMgr.AddDBCmd((int)TCPGameServerCmds.CMD_DB_UPDATETASK_CMD,
                                        string.Format("{0}:{1}:{2}:{3}:{4}:{5}",
                                        client.RoleID,
                                        client.TaskDataList[i].DoingTaskID,
                                        client.TaskDataList[i].DbID,
                                        client.TaskDataList[i].DoingTaskFocus,
                                        client.TaskDataList[i].DoingTaskVal1,
                                        client.TaskDataList[i].DoingTaskVal2),
                                        null, client.ServerId);

                                    updateTask = true;
                                }
                            }
                        }

                        if (updateTask)
                        {
                            //向客户端发布更新任务的信息
                            //任务更新通知
                            GameManager.ClientMgr.NotifyUpdateTask(sl, pool, client, client.TaskDataList[i].DbID, taskid, client.TaskDataList[i].DoingTaskVal1, client.TaskDataList[i].DoingTaskVal2, client.TaskDataList[i].DoingTaskFocus);

                            //如果任务已经完成
                            if (Global.JugeTaskComplete(client.TaskDataList[i].DoingTaskID, client.TaskDataList[i].DoingTaskVal1, client.TaskDataList[i].DoingTaskVal2))
                            {
                                // NPC的任务状态更新通知
                                int destNPC = systemTask.GetIntValue("DestNPC");
                                if (-1 != destNPC)
                                {
                                    int state = Global.ComputeNPCTaskState(client, client.TaskDataList, destNPC);
                                    GameManager.ClientMgr.NotifyUpdateNPCTaskSate(sl, pool, client, (destNPC + SpriteBaseIds.NpcBaseId), state);
                                }
                            }
                        }
                    }

                    if (extensionID == systemTask.GetIntValue("TargetNPC2"))
                    {
                        updateTask = false;

                        //根据具体的任务配置类型来判断是否完成
                        if (systemTask.GetIntValue("TargetType2") == (int)TaskTypes.Talk)
                        {
                            bool toAddVal = false;
                            int ingMaiID = systemTask.GetIntValue("JingMaiID");
                            int buffID = systemTask.GetIntValue("BuffID");
                            int wuXueID = systemTask.GetIntValue("WuXueID");
                            if (ingMaiID > 0) //如果需要检查新的经脉
                            {
                            }
                            else
                            {
                                toAddVal = true;
                            }

                            if (wuXueID > 0) //如果需要检查新的武学
                            {
                            }
                            else
                            {
                                toAddVal = toAddVal && true;
                            }

                            if (buffID > 0) //如果需要检查BuffID
                            {
                                BufferData bufferData = Global.GetBufferDataByID(client, buffID); //如果有Buffer
                                if (null != bufferData)
                                {
                                    if (!Global.IsBufferDataOver(bufferData, nowTicks)) //如果Buffer没过期
                                    {
                                        toAddVal = toAddVal && true;
                                    }
                                }
                            }
                            else
                            {
                                toAddVal = toAddVal && true;
                            }

                            if (toAddVal)
                            {
                                if (client.TaskDataList[i].DoingTaskVal2 < systemTask.GetIntValue("TargetNum2"))
                                {
                                    client.TaskDataList[i].DoingTaskVal2++;

                                    //异步写数据库，更新任务
                                    GameManager.DBCmdMgr.AddDBCmd((int)TCPGameServerCmds.CMD_DB_UPDATETASK_CMD,
                                        string.Format("{0}:{1}:{2}:{3}:{4}:{5}",
                                        client.RoleID,
                                        client.TaskDataList[i].DoingTaskID,
                                        client.TaskDataList[i].DbID,
                                        client.TaskDataList[i].DoingTaskFocus,
                                        client.TaskDataList[i].DoingTaskVal1,
                                        client.TaskDataList[i].DoingTaskVal2),
                                        null, client.ServerId);

                                    updateTask = true;
                                }
                            }
                        }
                        else if (systemTask.GetIntValue("TargetType2") == (int)TaskTypes.GetSomething && "" != systemTask.GetStringValue("PropsName2"))
                        {
                            if (client.TaskDataList[i].DoingTaskVal2 < systemTask.GetIntValue("TargetNum2"))
                            {
                                bool toUpdateTask = true;

                                //检测物品包中有没有指定的物品
                                string propsName = systemTask.GetStringValue("PropsName2");
                                string goodsName = GetPropNameGoodsName(propsName);
                                int goodsLevel = GetPropNameGoodsLevel(propsName);
                                int goodsQuality = GetPropNameGoodsQuality(propsName);

                                int transferGoodsID = Global.GetGoodsByName(goodsName);
                                if (transferGoodsID >= 0)
                                {
                                    GoodsData goodsData = Global.GetNotUsingGoodsByID(client, transferGoodsID, goodsLevel, goodsQuality);
                                    if (null == goodsData) //已经拥有该物品，不用再给
                                    {
                                        if (Global.CanAddGoods(client, transferGoodsID, 1, 1))
                                        {
                                            //将物品自动交给角色
                                            //想DBServer请求加入某个新的物品到背包中
                                            //添加物品
                                            Global.AddGoodsDBCommand(pool, client, transferGoodsID, 1, 0, "", 0, 1, 0, "", true, 1, "获取任务道具");
                                        }
                                        else
                                        {
                                            toUpdateTask = false;

                                            /// 通知在线的对方(不限制地图)个人紧要消息
                                            GameManager.ClientMgr.NotifyImportantMsg(sl, pool, client, Global.GetLang("背包已满，请先清理出空格再接受任务物品"), GameInfoTypeIndexes.Error, ShowGameInfoTypes.ErrAndBox, (int)HintErrCodeTypes.NoBagGrid);
                                        }
                                    }
                                }

                                if (toUpdateTask)
                                {
                                    client.TaskDataList[i].DoingTaskVal2++;

                                    //异步写数据库，更新任务
                                    GameManager.DBCmdMgr.AddDBCmd((int)TCPGameServerCmds.CMD_DB_UPDATETASK_CMD,
                                        string.Format("{0}:{1}:{2}:{3}:{4}:{5}",
                                        client.RoleID,
                                        client.TaskDataList[i].DoingTaskID,
                                        client.TaskDataList[i].DbID,
                                        client.TaskDataList[i].DoingTaskFocus,
                                        client.TaskDataList[i].DoingTaskVal1,
                                        client.TaskDataList[i].DoingTaskVal2),
                                        null, client.ServerId);

                                    updateTask = true;
                                }
                            }
                        }
                        else if (systemTask.GetIntValue("TargetType2") == (int)TaskTypes.NeedYuanBao)
                        {
                            if (client.TaskDataList[i].DoingTaskVal2 < systemTask.GetIntValue("TargetNum2"))
                            {
                                int totalChongZhiMoney = GameManager.ClientMgr.QueryTotaoChongZhiMoney(client);
                                if (totalChongZhiMoney > 0)
                                {
                                    client.TaskDataList[i].DoingTaskVal2++;

                                    //异步写数据库，更新任务
                                    GameManager.DBCmdMgr.AddDBCmd((int)TCPGameServerCmds.CMD_DB_UPDATETASK_CMD,
                                        string.Format("{0}:{1}:{2}:{3}:{4}:{5}",
                                        client.RoleID,
                                        client.TaskDataList[i].DoingTaskID,
                                        client.TaskDataList[i].DbID,
                                        client.TaskDataList[i].DoingTaskFocus,
                                        client.TaskDataList[i].DoingTaskVal1,
                                        client.TaskDataList[i].DoingTaskVal2),
                                        null, client.ServerId);

                                    updateTask = true;
                                }
                            }
                        }

                        if (updateTask)
                        {
                            //向客户端发布更新任务的信息
                            //任务更新通知
                            GameManager.ClientMgr.NotifyUpdateTask(sl, pool, client, client.TaskDataList[i].DbID, taskid, client.TaskDataList[i].DoingTaskVal1, client.TaskDataList[i].DoingTaskVal2, client.TaskDataList[i].DoingTaskFocus);

                            //如果任务已经完成
                            if (Global.JugeTaskComplete(client.TaskDataList[i].DoingTaskID, client.TaskDataList[i].DoingTaskVal1, client.TaskDataList[i].DoingTaskVal2))
                            {
                                // NPC的任务状态更新通知
                                int destNPC = systemTask.GetIntValue("DestNPC");
                                if (-1 != destNPC)
                                {
                                    int state = Global.ComputeNPCTaskState(client, client.TaskDataList, destNPC);
                                    GameManager.ClientMgr.NotifyUpdateNPCTaskSate(sl, pool, client, (destNPC + SpriteBaseIds.NpcBaseId), state);
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 处理物品交付任务
        /// </summary>
        /// <param name="client"></param>
        /// <param name="npcID"></param>
        /// <param name="goodsID"></param>
        /// <param name="taskType"></param>
        private static void ProcessTransferSomething(SocketListener sl, TCPOutPacketPool pool, KPlayer client, int npcID, int extensionID, int goodsID, TaskTypes taskType)
        {
            if (null == client.TaskDataList) return;

            long nowTicks = TimeUtil.NOW();

            bool updateTask = false;
            int taskid = -1;
            SystemXmlItem systemTask = null;

            lock (client.TaskDataList)
            {
                for (int i = 0; i < client.TaskDataList.Count; i++)
                {
                    taskid = client.TaskDataList[i].DoingTaskID;

                    //如果没有找到任务
                    if (!GameManager.SystemTasksMgr.SystemXmlItemDict.TryGetValue(taskid, out systemTask))
                    {
                        continue;
                    }

                    //判断任务是否依然有效
                    if (!IsTaskValid(client, systemTask, client.TaskDataList[i], nowTicks))
                    {
                        continue;
                    }

                    //判断任务是否属于当前的NPC
                    if (extensionID == systemTask.GetIntValue("TargetNPC1"))
                    {
                        updateTask = false;

                        //根据具体的任务配置类型来判断是否完成
                        if (systemTask.GetIntValue("TargetType1") == (int)TaskTypes.TransferSomething && "" != systemTask.GetStringValue("PropsName1"))
                        {
                            bool toUpdateTask = true;

                            //检测物品包中有没有指定的物品
                            string propsName = systemTask.GetStringValue("PropsName1");
                            string goodsName = GetPropNameGoodsName(propsName);
                            int goodsLevel = GetPropNameGoodsLevel(propsName);
                            int goodsQuality = GetPropNameGoodsQuality(propsName);

                            int transferGoodsID = Global.GetGoodsByName(goodsName);
                            if (transferGoodsID >= 0)
                            {
                                GoodsData goodsData = Global.GetNotUsingGoodsByID(client, transferGoodsID, goodsLevel, goodsQuality);
                                if (null != goodsData) //已经拥有该物品，可以完成交付操作
                                {
                                    //装备类的不扣除
                                    int catetoriy = Global.GetGoodsCatetoriy(transferGoodsID);
                                    if (catetoriy >= (int)ItemCategories.EquipMax)
                                    {
                                        //先从背包中扣除一个该物品, 因为可能多个在使用一个数据库记录
                                        GameManager.ClientMgr.NotifyUseGoods(sl, Global._TCPManager.tcpClientPool, pool, client, goodsData.Id, false);
                                    }
                                }
                                else
                                {
                                    toUpdateTask = false;
                                }
                            }

                            if (toUpdateTask)
                            {
                                if (client.TaskDataList[i].DoingTaskVal1 < systemTask.GetIntValue("TargetNum1"))
                                {
                                    client.TaskDataList[i].DoingTaskVal1++;

                                    //异步写数据库，更新任务
                                    GameManager.DBCmdMgr.AddDBCmd((int)TCPGameServerCmds.CMD_DB_UPDATETASK_CMD,
                                        string.Format("{0}:{1}:{2}:{3}:{4}:{5}",
                                        client.RoleID,
                                        client.TaskDataList[i].DoingTaskID,
                                        client.TaskDataList[i].DbID,
                                        client.TaskDataList[i].DoingTaskFocus,
                                        client.TaskDataList[i].DoingTaskVal1,
                                        client.TaskDataList[i].DoingTaskVal2),
                                        null, client.ServerId);

                                    updateTask = true;
                                }
                            }
                        }

                        if (updateTask)
                        {
                            //向客户端发布更新任务的信息
                            //任务更新通知
                            GameManager.ClientMgr.NotifyUpdateTask(sl, pool, client, client.TaskDataList[i].DbID, taskid, client.TaskDataList[i].DoingTaskVal1, client.TaskDataList[i].DoingTaskVal2, client.TaskDataList[i].DoingTaskFocus);

                            //如果任务已经完成
                            if (Global.JugeTaskComplete(client.TaskDataList[i].DoingTaskID, client.TaskDataList[i].DoingTaskVal1, client.TaskDataList[i].DoingTaskVal2))
                            {
                                // NPC的任务状态更新通知
                                int destNPC = systemTask.GetIntValue("DestNPC");
                                if (-1 != destNPC)
                                {
                                    int state = Global.ComputeNPCTaskState(client, client.TaskDataList, destNPC);
                                    GameManager.ClientMgr.NotifyUpdateNPCTaskSate(sl, pool, client, (destNPC + SpriteBaseIds.NpcBaseId), state);
                                }
                            }
                        }
                    }

                    if (extensionID == systemTask.GetIntValue("TargetNPC2"))
                    {
                        updateTask = false;

                        //根据具体的任务配置类型来判断是否完成
                        if (systemTask.GetIntValue("TargetType2") == (int)TaskTypes.TransferSomething && "" != systemTask.GetStringValue("PropsName2"))
                        {
                            bool toUpdateTask = true;

                            //检测物品包中有没有指定的物品
                            string propsName = systemTask.GetStringValue("PropsName2");
                            string goodsName = GetPropNameGoodsName(propsName);
                            int goodsLevel = GetPropNameGoodsLevel(propsName);
                            int goodsQuality = GetPropNameGoodsQuality(propsName);

                            int transferGoodsID = Global.GetGoodsByName(goodsName);
                            if (transferGoodsID >= 0)
                            {
                                GoodsData goodsData = Global.GetNotUsingGoodsByID(client, transferGoodsID, goodsLevel, goodsQuality);
                                if (null != goodsData) //已经拥有该物品，可以完成交付操作
                                {
                                    //装备类的不扣除
                                    int catetoriy = Global.GetGoodsCatetoriy(transferGoodsID);
                                    if (catetoriy >= (int)ItemCategories.EquipMax)
                                    {
                                        //先从背包中扣除一个该物品, 因为可能多个在使用一个数据库记录
                                        GameManager.ClientMgr.NotifyUseGoods(sl, Global._TCPManager.tcpClientPool, pool, client, goodsData.Id, false);
                                    }
                                }
                                else
                                {
                                    toUpdateTask = false;
                                }
                            }

                            if (toUpdateTask)
                            {
                                if (client.TaskDataList[i].DoingTaskVal2 < systemTask.GetIntValue("TargetNum2"))
                                {
                                    client.TaskDataList[i].DoingTaskVal2++;

                                    //异步写数据库，更新任务
                                    GameManager.DBCmdMgr.AddDBCmd((int)TCPGameServerCmds.CMD_DB_UPDATETASK_CMD,
                                        string.Format("{0}:{1}:{2}:{3}:{4}:{5}",
                                        client.RoleID,
                                        client.TaskDataList[i].DoingTaskID,
                                        client.TaskDataList[i].DbID,
                                        client.TaskDataList[i].DoingTaskFocus,
                                        client.TaskDataList[i].DoingTaskVal1,
                                        client.TaskDataList[i].DoingTaskVal2),
                                        null, client.ServerId);

                                    updateTask = true;
                                }
                            }
                        }

                        if (updateTask)
                        {
                            //向客户端发布更新任务的信息
                            //任务更新通知
                            GameManager.ClientMgr.NotifyUpdateTask(sl, pool, client, client.TaskDataList[i].DbID, taskid, client.TaskDataList[i].DoingTaskVal1, client.TaskDataList[i].DoingTaskVal2, client.TaskDataList[i].DoingTaskFocus);

                            //如果任务已经完成
                            if (Global.JugeTaskComplete(client.TaskDataList[i].DoingTaskID, client.TaskDataList[i].DoingTaskVal1, client.TaskDataList[i].DoingTaskVal2))
                            {
                                // NPC的任务状态更新通知
                                int destNPC = systemTask.GetIntValue("DestNPC");
                                if (-1 != destNPC)
                                {
                                    int state = Global.ComputeNPCTaskState(client, client.TaskDataList, destNPC);
                                    GameManager.ClientMgr.NotifyUpdateNPCTaskSate(sl, pool, client, (destNPC + SpriteBaseIds.NpcBaseId), state);
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 处理杀怪的任务
        /// </summary>
        /// <param name="client"></param>
        /// <param name="npcID"></param>
        /// <param name="goodsID"></param>
        /// <param name="taskType"></param>
        private static void ProcessKillMonster(SocketListener sl, TCPOutPacketPool pool, KPlayer client, int npcID, int extensionID, int goodsID, TaskTypes taskType)
        {
            if (null == client.TaskDataList) return;

            long nowTicks = TimeUtil.NOW();
            int focusCount = Global.GetFocusTaskCount(client);

            bool updateTask = false;
            int taskid = -1;
            SystemXmlItem systemTask = null;

            int monsterLevel = -1;
            SystemXmlItem monsterXml = null;
            if (GameManager.systemMonsterMgr.SystemXmlItemDict.TryGetValue(extensionID, out monsterXml))
            {
                monsterLevel = monsterXml.GetIntValue("Level");
            }

            lock (client.TaskDataList)
            {
                for (int i = 0; i < client.TaskDataList.Count; i++)
                {
                    taskid = client.TaskDataList[i].DoingTaskID;

                    //如果没有找到任务
                    if (!GameManager.SystemTasksMgr.SystemXmlItemDict.TryGetValue(taskid, out systemTask))
                    {
                        continue;
                    }

                    //判断任务是否依然有效
                    if (!IsTaskValid(client, systemTask, client.TaskDataList[i], nowTicks))
                    {
                        continue;
                    }

                    //杀不小于指定等级的怪
                    if ((int)TaskTypes.KillMonsterForLevel == systemTask.GetIntValue("TargetType1"))
                    {
                        if (monsterLevel >= systemTask.GetIntValue("TargetNPC1") && client.TaskDataList[i].DoingTaskVal1 < systemTask.GetIntValue("TargetNum1"))
                        {
                            client.TaskDataList[i].DoingTaskVal1++;

                            if (focusCount < Data.TaskMaxFocusCount && client.TaskDataList[i].DoingTaskFocus <= 0)
                            {
                                focusCount++;
                                client.TaskDataList[i].DoingTaskFocus = 1;
                            }

                            //异步写数据库，更新任务
                            GameManager.DBCmdMgr.AddDBCmd((int)TCPGameServerCmds.CMD_DB_UPDATETASK_CMD,
                               string.Format("{0}:{1}:{2}:{3}:{4}:{5}",
                                client.RoleID,
                                client.TaskDataList[i].DoingTaskID,
                                client.TaskDataList[i].DbID,
                                client.TaskDataList[i].DoingTaskFocus,
                                client.TaskDataList[i].DoingTaskVal1,
                                client.TaskDataList[i].DoingTaskVal2),
                                null, client.ServerId);

                            updateTask = true;
                        }

                        if (updateTask)
                        {
                            //向客户端发布更新任务的信息
                            //任务更新通知
                            GameManager.ClientMgr.NotifyUpdateTask(sl, pool, client, client.TaskDataList[i].DbID, taskid, client.TaskDataList[i].DoingTaskVal1, client.TaskDataList[i].DoingTaskVal2, client.TaskDataList[i].DoingTaskFocus);

                            //如果任务已经完成
                            if (Global.JugeTaskComplete(client.TaskDataList[i].DoingTaskID, client.TaskDataList[i].DoingTaskVal1, client.TaskDataList[i].DoingTaskVal2))
                            {
                                // NPC的任务状态更新通知
                                int destNPC = systemTask.GetIntValue("DestNPC");
                                if (-1 != destNPC)
                                {
                                    int state = Global.ComputeNPCTaskState(client, client.TaskDataList, destNPC);
                                    GameManager.ClientMgr.NotifyUpdateNPCTaskSate(sl, pool, client, (destNPC + SpriteBaseIds.NpcBaseId), state);
                                }
                            }
                        }
                    }
                    //判断任务是否属于当前的NPC
                    else if (extensionID == systemTask.GetIntValue("TargetNPC1"))
                    {
                        updateTask = false;

                        //根据具体的任务配置类型来判断是否完成
                        if (systemTask.GetIntValue("TargetType1") == (int)TaskTypes.KillMonster)
                        {
                            if (client.TaskDataList[i].DoingTaskVal1 < systemTask.GetIntValue("TargetNum1"))
                            {
                                client.TaskDataList[i].DoingTaskVal1++;

                                if (focusCount < Data.TaskMaxFocusCount && client.TaskDataList[i].DoingTaskFocus <= 0)
                                {
                                    focusCount++;
                                    client.TaskDataList[i].DoingTaskFocus = 1;
                                }

                                //异步写数据库，更新任务
                                GameManager.DBCmdMgr.AddDBCmd((int)TCPGameServerCmds.CMD_DB_UPDATETASK_CMD,
                                   string.Format("{0}:{1}:{2}:{3}:{4}:{5}",
                                    client.RoleID,
                                    client.TaskDataList[i].DoingTaskID,
                                    client.TaskDataList[i].DbID,
                                    client.TaskDataList[i].DoingTaskFocus,
                                    client.TaskDataList[i].DoingTaskVal1,
                                    client.TaskDataList[i].DoingTaskVal2),
                                    null, client.ServerId);

                                updateTask = true;
                            }
                        }
                        else if (systemTask.GetIntValue("TargetType1") == (int)TaskTypes.MonsterSomething)
                        {
                            int randNum = Global.GetRandomNumber(0, 101);
                            int randRange = systemTask.GetIntValue("FallPercent1");
                            if (randNum < randRange)
                            {
                                if (client.TaskDataList[i].DoingTaskVal1 < systemTask.GetIntValue("TargetNum1"))
                                {
                                    bool toUpdateTask = true;

                                    //检测物品包中有没有指定的物品
                                    string goodsName = systemTask.GetStringValue("PropsName1");
                                    int transferGoodsID = Global.GetGoodsByName(goodsName);
                                    if (transferGoodsID >= 0) //如果不是虚拟物品
                                    {
                                        if (Global.CanAddGoods(client, transferGoodsID, 1, 1))
                                        {
                                            //将物品自动交给角色
                                            //想DBServer请求加入某个新的物品到背包中
                                            //添加物品
                                            Global.AddGoodsDBCommand(pool, client, transferGoodsID, 1, 0, "", 0, 1, 0, "", true, 1, "获取杀怪掉落道具");
                                        }
                                        else
                                        {
                                            toUpdateTask = false;

                                            /// 通知在线的对方(不限制地图)个人紧要消息
                                            GameManager.ClientMgr.NotifyImportantMsg(sl, pool, client, StringUtil.substitute(Global.GetLang("背包已满，无法将【{0}】放入背包, 请清理出背包空格后再继续杀怪"), goodsName), GameInfoTypeIndexes.Error, ShowGameInfoTypes.ErrAndBox, (int)HintErrCodeTypes.NoBagGrid);
                                        }
                                    }

                                    if (toUpdateTask)
                                    {
                                        client.TaskDataList[i].DoingTaskVal1++;

                                        if (focusCount < Data.TaskMaxFocusCount && client.TaskDataList[i].DoingTaskFocus <= 0)
                                        {
                                            focusCount++;
                                            client.TaskDataList[i].DoingTaskFocus = 1;
                                        }

                                        //异步写数据库，更新任务
                                        GameManager.DBCmdMgr.AddDBCmd((int)TCPGameServerCmds.CMD_DB_UPDATETASK_CMD,
                                           string.Format("{0}:{1}:{2}:{3}:{4}:{5}",
                                            client.RoleID,
                                            client.TaskDataList[i].DoingTaskID,
                                            client.TaskDataList[i].DbID,
                                            client.TaskDataList[i].DoingTaskFocus,
                                            client.TaskDataList[i].DoingTaskVal1,
                                            client.TaskDataList[i].DoingTaskVal2),
                                            null, client.ServerId);

                                        updateTask = true;
                                    }
                                }
                            }
                        }
                        else if (systemTask.GetIntValue("TargetType1") == (int)TaskTypes.CaiJiGoods)
                        {
                            int randNum = Global.GetRandomNumber(0, 101);
                            int randRange = systemTask.GetIntValue("FallPercent1");
                            if (randNum < randRange)
                            {
                                if (client.TaskDataList[i].DoingTaskVal1 < systemTask.GetIntValue("TargetNum1"))
                                {
                                    bool toUpdateTask = true;

                                    //检测物品包中有没有指定的物品
                                    string goodsName = systemTask.GetStringValue("PropsName1");
                                    int transferGoodsID = Global.GetGoodsByName(goodsName);
                                    if (transferGoodsID >= 0) //如果不是虚拟物品
                                    {
                                        if (Global.CanAddGoods(client, transferGoodsID, 1, 1))
                                        {
                                            //将物品自动交给角色
                                            //想DBServer请求加入某个新的物品到背包中
                                            //添加物品
                                            Global.AddGoodsDBCommand(pool, client, transferGoodsID, 1, 0, "", 0, 1, 0, "", true, 1, "采集获取道具");
                                        }
                                        else
                                        {
                                            toUpdateTask = false;

                                            /// 通知在线的对方(不限制地图)个人紧要消息
                                            GameManager.ClientMgr.NotifyImportantMsg(sl, pool, client, StringUtil.substitute(Global.GetLang("背包已满，无法将【{0}】放入背包, 请清理出背包空格后再继续采集"), goodsName), GameInfoTypeIndexes.Error, ShowGameInfoTypes.ErrAndBox, (int)HintErrCodeTypes.NoBagGrid);
                                        }
                                    }

                                    if (toUpdateTask)
                                    {
                                        client.TaskDataList[i].DoingTaskVal1++;

                                        if (focusCount < Data.TaskMaxFocusCount && client.TaskDataList[i].DoingTaskFocus <= 0)
                                        {
                                            focusCount++;
                                            client.TaskDataList[i].DoingTaskFocus = 1;
                                        }

                                        //异步写数据库，更新任务
                                        GameManager.DBCmdMgr.AddDBCmd((int)TCPGameServerCmds.CMD_DB_UPDATETASK_CMD,
                                           string.Format("{0}:{1}:{2}:{3}:{4}:{5}",
                                            client.RoleID,
                                            client.TaskDataList[i].DoingTaskID,
                                            client.TaskDataList[i].DbID,
                                            client.TaskDataList[i].DoingTaskFocus,
                                            client.TaskDataList[i].DoingTaskVal1,
                                            client.TaskDataList[i].DoingTaskVal2),
                                            null, client.ServerId);

                                        updateTask = true;
                                    }
                                }
                            }
                        }

                        if (updateTask)
                        {
                            //向客户端发布更新任务的信息
                            //任务更新通知
                            GameManager.ClientMgr.NotifyUpdateTask(sl, pool, client, client.TaskDataList[i].DbID, taskid, client.TaskDataList[i].DoingTaskVal1, client.TaskDataList[i].DoingTaskVal2, client.TaskDataList[i].DoingTaskFocus);

                            //如果任务已经完成
                            if (Global.JugeTaskComplete(client.TaskDataList[i].DoingTaskID, client.TaskDataList[i].DoingTaskVal1, client.TaskDataList[i].DoingTaskVal2))
                            {
                                // NPC的任务状态更新通知
                                int destNPC = systemTask.GetIntValue("DestNPC");
                                if (-1 != destNPC)
                                {
                                    int state = Global.ComputeNPCTaskState(client, client.TaskDataList, destNPC);
                                    GameManager.ClientMgr.NotifyUpdateNPCTaskSate(sl, pool, client, (destNPC + SpriteBaseIds.NpcBaseId), state);
                                }
                            }
                        }
                    }

                    if (extensionID == systemTask.GetIntValue("TargetNPC2"))
                    {
                        updateTask = false;

                        //根据具体的任务配置类型来判断是否完成
                        if (systemTask.GetIntValue("TargetType2") == (int)TaskTypes.KillMonster)
                        {
                            if (client.TaskDataList[i].DoingTaskVal2 < systemTask.GetIntValue("TargetNum2"))
                            {
                                client.TaskDataList[i].DoingTaskVal2++;

                                if (focusCount < Data.TaskMaxFocusCount && client.TaskDataList[i].DoingTaskFocus <= 0)
                                {
                                    focusCount++;
                                    client.TaskDataList[i].DoingTaskFocus = 1;
                                }

                                //异步写数据库，更新任务
                                GameManager.DBCmdMgr.AddDBCmd((int)TCPGameServerCmds.CMD_DB_UPDATETASK_CMD,
                                   string.Format("{0}:{1}:{2}:{3}:{4}:{5}",
                                    client.RoleID,
                                    client.TaskDataList[i].DoingTaskID,
                                    client.TaskDataList[i].DbID,
                                    client.TaskDataList[i].DoingTaskFocus,
                                    client.TaskDataList[i].DoingTaskVal1,
                                    client.TaskDataList[i].DoingTaskVal2),
                                    null, client.ServerId);

                                updateTask = true;
                            }
                        }
                        else if (systemTask.GetIntValue("TargetType2") == (int)TaskTypes.MonsterSomething)
                        {
                            int randNum = Global.GetRandomNumber(0, 101);
                            int randRange = systemTask.GetIntValue("FallPercent2");
                            if (randNum < randRange)
                            {
                                if (client.TaskDataList[i].DoingTaskVal2 < systemTask.GetIntValue("TargetNum2"))
                                {
                                    bool toUpdateTask = true;

                                    //检测物品包中有没有指定的物品
                                    string goodsName = systemTask.GetStringValue("PropsName2");
                                    int transferGoodsID = Global.GetGoodsByName(goodsName);
                                    if (transferGoodsID >= 0) //如果不是虚拟物品
                                    {
                                        if (Global.CanAddGoods(client, transferGoodsID, 1, 1))
                                        {
                                            //将物品自动交给角色
                                            //想DBServer请求加入某个新的物品到背包中
                                            //添加物品
                                            Global.AddGoodsDBCommand(pool, client, transferGoodsID, 1, 0, "", 0, 1, 0, "", true, 1, "获取杀怪道具");
                                        }
                                        else
                                        {
                                            toUpdateTask = false;

                                            /// 通知在线的对方(不限制地图)个人紧要消息
                                            GameManager.ClientMgr.NotifyImportantMsg(sl, pool, client, StringUtil.substitute(Global.GetLang("背包已满，无法将【{0}】放入背包, 请清理出背包空格后再继续杀怪"), goodsName), GameInfoTypeIndexes.Error, ShowGameInfoTypes.ErrAndBox, (int)HintErrCodeTypes.NoBagGrid);
                                        }
                                    }

                                    if (toUpdateTask)
                                    {
                                        client.TaskDataList[i].DoingTaskVal2++;

                                        if (focusCount < Data.TaskMaxFocusCount && client.TaskDataList[i].DoingTaskFocus <= 0)
                                        {
                                            focusCount++;
                                            client.TaskDataList[i].DoingTaskFocus = 1;
                                        }

                                        //异步写数据库，更新任务
                                        GameManager.DBCmdMgr.AddDBCmd((int)TCPGameServerCmds.CMD_DB_UPDATETASK_CMD,
                                           string.Format("{0}:{1}:{2}:{3}:{4}:{5}",
                                            client.RoleID,
                                            client.TaskDataList[i].DoingTaskID,
                                            client.TaskDataList[i].DbID,
                                            client.TaskDataList[i].DoingTaskFocus,
                                            client.TaskDataList[i].DoingTaskVal1,
                                            client.TaskDataList[i].DoingTaskVal2),
                                            null, client.ServerId);

                                        updateTask = true;
                                    }
                                }
                            }
                        }
                        else if (systemTask.GetIntValue("TargetType2") == (int)TaskTypes.CaiJiGoods)
                        {
                            int randNum = Global.GetRandomNumber(0, 101);
                            int randRange = systemTask.GetIntValue("FallPercent2");
                            if (randNum < randRange)
                            {
                                if (client.TaskDataList[i].DoingTaskVal2 < systemTask.GetIntValue("TargetNum2"))
                                {
                                    bool toUpdateTask = true;

                                    //检测物品包中有没有指定的物品
                                    string goodsName = systemTask.GetStringValue("PropsName2");
                                    int transferGoodsID = Global.GetGoodsByName(goodsName);
                                    if (transferGoodsID >= 0) //如果不是虚拟物品
                                    {
                                        if (Global.CanAddGoods(client, transferGoodsID, 1, 1))
                                        {
                                            //将物品自动交给角色
                                            //想DBServer请求加入某个新的物品到背包中
                                            //添加物品
                                            Global.AddGoodsDBCommand(pool, client, transferGoodsID, 1, 0, "", 0, 1, 0, "", true, 1, "采集获取道具");
                                        }
                                        else
                                        {
                                            toUpdateTask = false;

                                            /// 通知在线的对方(不限制地图)个人紧要消息
                                            GameManager.ClientMgr.NotifyImportantMsg(sl, pool, client, StringUtil.substitute(Global.GetLang("背包已满，无法将【{0}】放入背包, 请清理出背包空格后再继续采集"), goodsName), GameInfoTypeIndexes.Error, ShowGameInfoTypes.ErrAndBox, (int)HintErrCodeTypes.NoBagGrid);
                                        }
                                    }

                                    if (toUpdateTask)
                                    {
                                        client.TaskDataList[i].DoingTaskVal2++;

                                        if (focusCount < Data.TaskMaxFocusCount && client.TaskDataList[i].DoingTaskFocus <= 0)
                                        {
                                            focusCount++;
                                            client.TaskDataList[i].DoingTaskFocus = 1;
                                        }

                                        //异步写数据库，更新任务
                                        GameManager.DBCmdMgr.AddDBCmd((int)TCPGameServerCmds.CMD_DB_UPDATETASK_CMD,
                                           string.Format("{0}:{1}:{2}:{3}:{4}:{5}",
                                            client.RoleID,
                                            client.TaskDataList[i].DoingTaskID,
                                            client.TaskDataList[i].DbID,
                                            client.TaskDataList[i].DoingTaskFocus,
                                            client.TaskDataList[i].DoingTaskVal1,
                                            client.TaskDataList[i].DoingTaskVal2),
                                            null, client.ServerId);

                                        updateTask = true;
                                    }
                                }
                            }
                        }

                        if (updateTask)
                        {
                            //向客户端发布更新任务的信息
                            //任务更新通知
                            GameManager.ClientMgr.NotifyUpdateTask(sl, pool, client, client.TaskDataList[i].DbID, taskid, client.TaskDataList[i].DoingTaskVal1, client.TaskDataList[i].DoingTaskVal2, client.TaskDataList[i].DoingTaskFocus);

                            //如果任务已经完成
                            if (Global.JugeTaskComplete(client.TaskDataList[i].DoingTaskID, client.TaskDataList[i].DoingTaskVal1, client.TaskDataList[i].DoingTaskVal2))
                            {
                                // NPC的任务状态更新通知
                                int destNPC = systemTask.GetIntValue("DestNPC");
                                if (-1 != destNPC)
                                {
                                    int state = Global.ComputeNPCTaskState(client, client.TaskDataList, destNPC);
                                    GameManager.ClientMgr.NotifyUpdateNPCTaskSate(sl, pool, client, (destNPC + SpriteBaseIds.NpcBaseId), state);
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 处理和NPC购买物品
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

                    //如果没有找到任务
                    if (!GameManager.SystemTasksMgr.SystemXmlItemDict.TryGetValue(taskid, out systemTask))
                    {
                        continue;
                    }

                    //判断任务是否依然有效
                    if (!IsTaskValid(client, systemTask, client.TaskDataList[i], nowTicks))
                    {
                        continue;
                    }

                    //根据具体的任务配置类型来判断是否完成
                    if (systemTask.GetIntValue("TargetType1") == (int)TaskTypes.BuySomething)
                    {
                        updateTask = false;

                        //检测物品包中有没有指定的物品
                        string propsName = systemTask.GetStringValue("PropsName1");
                        string goodsName = GetPropNameGoodsName(propsName);
                        int goodsLevel = GetPropNameGoodsLevel(propsName);
                        int goodsQuality = GetPropNameGoodsQuality(propsName);

                        int transferGoodsID = Global.GetGoodsByName(goodsName);
                        if (goodsID == transferGoodsID)
                        {
                            GoodsData goodsData = Global.GetNotUsingGoodsByID(client, transferGoodsID, goodsLevel, goodsQuality);
                            if (null != goodsData) //已经拥有该物品，可以完成操作
                            {
                                if (client.TaskDataList[i].DoingTaskVal1 < systemTask.GetIntValue("TargetNum1"))
                                {
                                    client.TaskDataList[i].DoingTaskVal1++;

                                    //异步写数据库，更新任务
                                    GameManager.DBCmdMgr.AddDBCmd((int)TCPGameServerCmds.CMD_DB_UPDATETASK_CMD,
                                        string.Format("{0}:{1}:{2}:{3}:{4}:{5}",
                                        client.RoleID,
                                        client.TaskDataList[i].DoingTaskID,
                                        client.TaskDataList[i].DbID,
                                        client.TaskDataList[i].DoingTaskFocus,
                                        client.TaskDataList[i].DoingTaskVal1,
                                        client.TaskDataList[i].DoingTaskVal2),
                                        null, client.ServerId);

                                    updateTask = true;
                                }
                            }
                        }

                        if (updateTask)
                        {
                            //向客户端发布更新任务的信息
                            //任务更新通知
                            GameManager.ClientMgr.NotifyUpdateTask(sl, pool, client, client.TaskDataList[i].DbID, taskid, client.TaskDataList[i].DoingTaskVal1, client.TaskDataList[i].DoingTaskVal2, client.TaskDataList[i].DoingTaskFocus);

                            //如果任务已经完成
                            if (Global.JugeTaskComplete(client.TaskDataList[i].DoingTaskID, client.TaskDataList[i].DoingTaskVal1, client.TaskDataList[i].DoingTaskVal2))
                            {
                                // NPC的任务状态更新通知
                                int destNPC = systemTask.GetIntValue("DestNPC");
                                if (-1 != destNPC)
                                {
                                    int state = Global.ComputeNPCTaskState(client, client.TaskDataList, destNPC);
                                    GameManager.ClientMgr.NotifyUpdateNPCTaskSate(sl, pool, client, (destNPC + SpriteBaseIds.NpcBaseId), state);
                                }
                            }
                        }
                    }

                    //根据具体的任务配置类型来判断是否完成
                    if (systemTask.GetIntValue("TargetType2") == (int)TaskTypes.BuySomething)
                    {
                        updateTask = false;

                        //检测物品包中有没有指定的物品
                        string propsName = systemTask.GetStringValue("PropsName2");
                        string goodsName = GetPropNameGoodsName(propsName);
                        int goodsLevel = GetPropNameGoodsLevel(propsName);
                        int goodsQuality = GetPropNameGoodsQuality(propsName);

                        int transferGoodsID = Global.GetGoodsByName(goodsName);
                        if (goodsID == transferGoodsID)
                        {
                            GoodsData goodsData = Global.GetNotUsingGoodsByID(client, transferGoodsID, goodsLevel, goodsQuality);
                            if (null != goodsData) //已经拥有该物品，可以完成操作
                            {
                                if (client.TaskDataList[i].DoingTaskVal2 < systemTask.GetIntValue("TargetNum2"))
                                {
                                    client.TaskDataList[i].DoingTaskVal2++;

                                    //异步写数据库，更新任务
                                    GameManager.DBCmdMgr.AddDBCmd((int)TCPGameServerCmds.CMD_DB_UPDATETASK_CMD,
                                        string.Format("{0}:{1}:{2}:{3}:{4}:{5}",
                                        client.RoleID,
                                        client.TaskDataList[i].DoingTaskID,
                                        client.TaskDataList[i].DbID,
                                        client.TaskDataList[i].DoingTaskFocus,
                                        client.TaskDataList[i].DoingTaskVal1,
                                        client.TaskDataList[i].DoingTaskVal2),
                                        null, client.ServerId);

                                    updateTask = true;
                                }
                            }
                        }

                        if (updateTask)
                        {
                            //向客户端发布更新任务的信息
                            //任务更新通知
                            GameManager.ClientMgr.NotifyUpdateTask(sl, pool, client, client.TaskDataList[i].DbID, taskid, client.TaskDataList[i].DoingTaskVal1, client.TaskDataList[i].DoingTaskVal2, client.TaskDataList[i].DoingTaskFocus);

                            //如果任务已经完成
                            if (Global.JugeTaskComplete(client.TaskDataList[i].DoingTaskID, client.TaskDataList[i].DoingTaskVal1, client.TaskDataList[i].DoingTaskVal2))
                            {
                                // NPC的任务状态更新通知
                                int destNPC = systemTask.GetIntValue("DestNPC");
                                if (-1 != destNPC)
                                {
                                    int state = Global.ComputeNPCTaskState(client, client.TaskDataList, destNPC);
                                    GameManager.ClientMgr.NotifyUpdateNPCTaskSate(sl, pool, client, (destNPC + SpriteBaseIds.NpcBaseId), state);
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 处理使用物品
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
            GameMap gameMap = GameManager.MapMgr.DictMaps[client.MapCode];

            lock (client.TaskDataList)
            {
                for (int i = 0; i < client.TaskDataList.Count; i++)
                {
                    taskid = client.TaskDataList[i].DoingTaskID;

                    //如果没有找到任务
                    if (!GameManager.SystemTasksMgr.SystemXmlItemDict.TryGetValue(taskid, out systemTask))
                    {
                        continue;
                    }

                    //判断任务是否依然有效
                    if (!IsTaskValid(client, systemTask, client.TaskDataList[i], nowTicks))
                    {
                        continue;
                    }

                    //根据具体的任务配置类型来判断是否完成
                    if (systemTask.GetIntValue("TargetType1") == (int)TaskTypes.UseSomething)
                    {
                        updateTask = false;

                        //检测物品包中有没有指定的物品
                        string goodsName = systemTask.GetStringValue("PropsName1");
                        int transferGoodsID = Global.GetGoodsByName(goodsName);
                        if (goodsID == transferGoodsID)
                        {
                            //判断范围是否正确
                            int targetMapCode1 = systemTask.GetIntValue("TargetMapCode1");
                            Point targetPos1 = Global.StrToPoint(systemTask.GetStringValue("TargetPos1"));
                            if (targetMapCode1 >= 0 && !double.IsNaN(targetPos1.X) && !double.IsNaN(targetPos1.Y))
                            {
                                Point clientGrid = client.CurrentGrid;
                                Point usingGoodsGrid = new Point((int)(targetPos1.X / gameMap.MapGridWidth), (int)(targetPos1.Y / gameMap.MapGridHeight));
                                bool inGrid = Math.Abs(usingGoodsGrid.X - clientGrid.X) < 3 && Math.Abs(usingGoodsGrid.Y - clientGrid.Y) < 3;
                                if (targetMapCode1 == client.MapCode && inGrid)
                                {
                                    if (client.TaskDataList[i].DoingTaskVal1 < systemTask.GetIntValue("TargetNum1"))
                                    {
                                        client.TaskDataList[i].DoingTaskVal1++;

                                        //异步写数据库，更新任务
                                        GameManager.DBCmdMgr.AddDBCmd((int)TCPGameServerCmds.CMD_DB_UPDATETASK_CMD,
                                            string.Format("{0}:{1}:{2}:{3}:{4}:{5}",
                                            client.RoleID,
                                            client.TaskDataList[i].DoingTaskID,
                                            client.TaskDataList[i].DbID,
                                            client.TaskDataList[i].DoingTaskFocus,
                                            client.TaskDataList[i].DoingTaskVal1,
                                            client.TaskDataList[i].DoingTaskVal2),
                                            null, client.ServerId);

                                        updateTask = true;
                                    }
                                }
                            }
                        }

                        if (updateTask)
                        {
                            //向客户端发布更新任务的信息
                            //任务更新通知
                            GameManager.ClientMgr.NotifyUpdateTask(sl, pool, client, client.TaskDataList[i].DbID, taskid, client.TaskDataList[i].DoingTaskVal1, client.TaskDataList[i].DoingTaskVal2, client.TaskDataList[i].DoingTaskFocus);

                            //如果任务已经完成
                            if (Global.JugeTaskComplete(client.TaskDataList[i].DoingTaskID, client.TaskDataList[i].DoingTaskVal1, client.TaskDataList[i].DoingTaskVal2))
                            {
                                // NPC的任务状态更新通知
                                int destNPC = systemTask.GetIntValue("DestNPC");
                                if (-1 != destNPC)
                                {
                                    int state = Global.ComputeNPCTaskState(client, client.TaskDataList, destNPC);
                                    GameManager.ClientMgr.NotifyUpdateNPCTaskSate(sl, pool, client, (destNPC + SpriteBaseIds.NpcBaseId), state);
                                }
                            }
                        }
                    }

                    //根据具体的任务配置类型来判断是否完成
                    if (systemTask.GetIntValue("TargetType2") == (int)TaskTypes.UseSomething)
                    {
                        updateTask = false;

                        //检测物品包中有没有指定的物品
                        string goodsName = systemTask.GetStringValue("PropsName2");
                        int transferGoodsID = Global.GetGoodsByName(goodsName);
                        if (goodsID == transferGoodsID)
                        {
                            //判断范围是否正确
                            int targetMapCode2 = systemTask.GetIntValue("TargetMapCode2");
                            Point targetPos2 = Global.StrToPoint(systemTask.GetStringValue("TargetPos2"));
                            if (targetMapCode2 >= 0 && !double.IsNaN(targetPos2.X) && !double.IsNaN(targetPos2.Y))
                            {
                                Point clientGrid = client.CurrentGrid;
                                Point usingGoodsGrid = new Point((int)(targetPos2.X / gameMap.MapGridWidth), (int)(targetPos2.Y / gameMap.MapGridHeight));
                                bool inGrid = Math.Abs(usingGoodsGrid.X - clientGrid.X) < 3 && Math.Abs(usingGoodsGrid.Y - clientGrid.Y) < 3;
                                if (targetMapCode2 == client.MapCode && inGrid)
                                {
                                    if (client.TaskDataList[i].DoingTaskVal2 < systemTask.GetIntValue("TargetNum2"))
                                    {
                                        client.TaskDataList[i].DoingTaskVal2++;

                                        //异步写数据库，更新任务
                                        GameManager.DBCmdMgr.AddDBCmd((int)TCPGameServerCmds.CMD_DB_UPDATETASK_CMD,
                                            string.Format("{0}:{1}:{2}:{3}:{4}:{5}",
                                            client.RoleID,
                                            client.TaskDataList[i].DoingTaskID,
                                            client.TaskDataList[i].DbID,
                                            client.TaskDataList[i].DoingTaskFocus,
                                            client.TaskDataList[i].DoingTaskVal1,
                                            client.TaskDataList[i].DoingTaskVal2),
                                            null, client.ServerId);

                                        updateTask = true;
                                    }
                                }
                            }
                        }

                        if (updateTask)
                        {
                            //向客户端发布更新任务的信息
                            //任务更新通知
                            GameManager.ClientMgr.NotifyUpdateTask(sl, pool, client, client.TaskDataList[i].DbID, taskid, client.TaskDataList[i].DoingTaskVal1, client.TaskDataList[i].DoingTaskVal2, client.TaskDataList[i].DoingTaskFocus);

                            //如果任务已经完成
                            if (Global.JugeTaskComplete(client.TaskDataList[i].DoingTaskID, client.TaskDataList[i].DoingTaskVal1, client.TaskDataList[i].DoingTaskVal2))
                            {
                                // NPC的任务状态更新通知
                                int destNPC = systemTask.GetIntValue("DestNPC");
                                if (-1 != destNPC)
                                {
                                    int state = Global.ComputeNPCTaskState(client, client.TaskDataList, destNPC);
                                    GameManager.ClientMgr.NotifyUpdateNPCTaskSate(sl, pool, client, (destNPC + SpriteBaseIds.NpcBaseId), state);
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 处理lua脚本控制完成(治疗，防火...)
        /// </summary>
        /// <param name="client"></param>
        /// <param name="npcID"></param>
        /// <param name="goodsID"></param>
        /// <param name="taskType"></param>
        private static void ProcessLuaHandle(SocketListener sl, TCPOutPacketPool pool, KPlayer client, int npcID, int extensionID, int goodsID, TaskTypes taskType)
        {
            if (null == client.TaskDataList) return;

            long nowTicks = TimeUtil.NOW();

            bool updateTask = false;
            int taskid = -1;
            SystemXmlItem systemTask = null;

            NPC npc = NPCGeneralManager.FindNPC(client.MapCode, extensionID);
            if (null == npc) return;

            Point clientGrid = client.CurrentGrid;
            Point npcGrid = npc.CurrentGrid;
            bool inGrid = Math.Abs(npcGrid.X - clientGrid.X) <= 9 && Math.Abs(npcGrid.Y - clientGrid.Y) <= 9;
            if (!inGrid)
            {
                return;
            }

            lock (client.TaskDataList)
            {
                for (int i = 0; i < client.TaskDataList.Count; i++)
                {
                    taskid = client.TaskDataList[i].DoingTaskID;

                    //如果没有找到任务
                    if (!GameManager.SystemTasksMgr.SystemXmlItemDict.TryGetValue(taskid, out systemTask))
                    {
                        continue;
                    }

                    //判断任务是否依然有效
                    if (!IsTaskValid(client, systemTask, client.TaskDataList[i], nowTicks))
                    {
                        continue;
                    }

                    //根据具体的任务配置类型来判断是否完成
                    if (systemTask.GetIntValue("TargetType1") == (int)TaskTypes.ZhiLiao ||
                        systemTask.GetIntValue("TargetType1") == (int)TaskTypes.FangHuo)
                    {
                        updateTask = false;
                        if (client.TaskDataList[i].DoingTaskVal1 < systemTask.GetIntValue("TargetNum1"))
                        {
                            client.TaskDataList[i].DoingTaskVal1++;

                            //异步写数据库，更新任务
                            GameManager.DBCmdMgr.AddDBCmd((int)TCPGameServerCmds.CMD_DB_UPDATETASK_CMD,
                                string.Format("{0}:{1}:{2}:{3}:{4}:{5}",
                                client.RoleID,
                                client.TaskDataList[i].DoingTaskID,
                                client.TaskDataList[i].DbID,
                                client.TaskDataList[i].DoingTaskFocus,
                                client.TaskDataList[i].DoingTaskVal1,
                                client.TaskDataList[i].DoingTaskVal2),
                                null, client.ServerId);

                            updateTask = true;
                        }

                        if (updateTask)
                        {
                            //向客户端发布更新任务的信息
                            //任务更新通知
                            GameManager.ClientMgr.NotifyUpdateTask(sl, pool, client, client.TaskDataList[i].DbID, taskid, client.TaskDataList[i].DoingTaskVal1, client.TaskDataList[i].DoingTaskVal2, client.TaskDataList[i].DoingTaskFocus);

                            //如果任务已经完成
                            if (Global.JugeTaskComplete(client.TaskDataList[i].DoingTaskID, client.TaskDataList[i].DoingTaskVal1, client.TaskDataList[i].DoingTaskVal2))
                            {
                                // NPC的任务状态更新通知
                                int destNPC = systemTask.GetIntValue("DestNPC");
                                if (-1 != destNPC)
                                {
                                    int state = Global.ComputeNPCTaskState(client, client.TaskDataList, destNPC);
                                    GameManager.ClientMgr.NotifyUpdateNPCTaskSate(sl, pool, client, (destNPC + SpriteBaseIds.NpcBaseId), state);
                                }
                            }
                        }
                    }

                    //根据具体的任务配置类型来判断是否完成
                    if (systemTask.GetIntValue("TargetType2") == (int)TaskTypes.ZhiLiao ||
                        systemTask.GetIntValue("TargetType2") == (int)TaskTypes.FangHuo)
                    {
                        updateTask = false;
                        if (client.TaskDataList[i].DoingTaskVal2 < systemTask.GetIntValue("TargetNum2"))
                        {
                            client.TaskDataList[i].DoingTaskVal2++;

                            //异步写数据库，更新任务
                            GameManager.DBCmdMgr.AddDBCmd((int)TCPGameServerCmds.CMD_DB_UPDATETASK_CMD,
                                string.Format("{0}:{1}:{2}:{3}:{4}:{5}",
                                client.RoleID,
                                client.TaskDataList[i].DoingTaskID,
                                client.TaskDataList[i].DbID,
                                client.TaskDataList[i].DoingTaskFocus,
                                client.TaskDataList[i].DoingTaskVal1,
                                client.TaskDataList[i].DoingTaskVal2),
                                null, client.ServerId);

                            updateTask = true;
                        }
                    }

                    if (updateTask)
                    {
                        //向客户端发布更新任务的信息
                        //任务更新通知
                        GameManager.ClientMgr.NotifyUpdateTask(sl, pool, client, client.TaskDataList[i].DbID, taskid, client.TaskDataList[i].DoingTaskVal1, client.TaskDataList[i].DoingTaskVal2, client.TaskDataList[i].DoingTaskFocus);

                        //如果任务已经完成
                        if (Global.JugeTaskComplete(client.TaskDataList[i].DoingTaskID, client.TaskDataList[i].DoingTaskVal1, client.TaskDataList[i].DoingTaskVal2))
                        {
                            // NPC的任务状态更新通知
                            int destNPC = systemTask.GetIntValue("DestNPC");
                            if (-1 != destNPC)
                            {
                                int state = Global.ComputeNPCTaskState(client, client.TaskDataList, destNPC);
                                GameManager.ClientMgr.NotifyUpdateNPCTaskSate(sl, pool, client, (destNPC + SpriteBaseIds.NpcBaseId), state);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// GM设置角色的主线任务进度
        /// </summary>
        /// <param name="client"></param>
        public static void GMSetMainTaskID(KPlayer client, int taskID = 2000)
        {
            int roleID = client.RoleID;
            client.OldTasks = new List<OldTaskData>();
            client.TaskDataList = new List<TaskData>();

            int mainTaskID = int.MaxValue;
            int npcID = 0;
            List<int> list = new List<int>();
            foreach (var kv in GameManager.SystemTasksMgr.SystemXmlItemDict)
            {
                SystemXmlItem systemTask = kv.Value;
                if (kv.Key < mainTaskID && kv.Key >= taskID)
                {
                    mainTaskID = kv.Key;
                    npcID = kv.Value.GetIntValue("DestNPC");
                }
                if (kv.Key < taskID)
                {
                    list.Add(kv.Key);
                    client.OldTasks.Add(new OldTaskData()
                    {
                        TaskID = kv.Key,
                        DoCount = 1,
                    });
                }
            }

            list.Sort();
            list.Insert(0, roleID);

            Global.sendToDB<int, byte[]>((int)TCPGameServerCmds.CMD_SPR_GM_SET_MAIN_TASK, DataHelper.ObjectToBytes(list), client.ServerId);

            client.sendCmd((int)TCPGameServerCmds.CMD_SPR_COMPTASK, string.Format("{0}:{1}:{2}:{3}", roleID, npcID, list[list.Count - 1], 0));

            TCPOutPacket tcpOutPacketTemp = null;
            TCPProcessCmdResults result = Global.TakeNewTask(TCPManager.getInstance(), client.ClientSocket, TCPManager.getInstance().tcpClientPool, TCPManager.getInstance().tcpRandKey, TCPManager.getInstance().TcpOutPacketPool, (int)TCPGameServerCmds.CMD_SPR_NEWTASK, client, roleID, mainTaskID, npcID, out tcpOutPacketTemp);
            if (result == TCPProcessCmdResults.RESULT_DATA && null != tcpOutPacketTemp)
            {
                client.sendCmd(tcpOutPacketTemp);
            }

            Global.ForceCloseClient(client);
        }

        /// <summary>
        /// 直接修改任务的数值
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

                    //如果没有找到任务
                    if (!GameManager.SystemTasksMgr.SystemXmlItemDict.TryGetValue(taskid, out systemTask))
                    {
                        continue;
                    }

                    //判断任务是否是指定的任务
                    if (taskName != systemTask.GetStringValue("Title"))
                    {
                        continue;
                    }

                    if (1 == valType) //目标1
                    {
                        bool updateTask = false;
                        if (client.TaskDataList[i].DoingTaskVal1 < systemTask.GetIntValue("TargetNum1"))
                        {
                            client.TaskDataList[i].DoingTaskVal1 = Global.GMin(taskVal, systemTask.GetIntValue("TargetNum1"));

                            //异步写数据库，更新任务
                            GameManager.DBCmdMgr.AddDBCmd((int)TCPGameServerCmds.CMD_DB_UPDATETASK_CMD,
                                string.Format("{0}:{1}:{2}:{3}:{4}:{5}",
                                client.RoleID,
                                client.TaskDataList[i].DoingTaskID,
                                client.TaskDataList[i].DbID,
                                client.TaskDataList[i].DoingTaskFocus,
                                client.TaskDataList[i].DoingTaskVal1,
                                client.TaskDataList[i].DoingTaskVal2),
                                null, client.ServerId);

                            updateTask = true;
                        }

                        if (updateTask)
                        {
                            //向客户端发布更新任务的信息
                            //任务更新通知
                            GameManager.ClientMgr.NotifyUpdateTask(sl, pool, client, client.TaskDataList[i].DbID, taskid, client.TaskDataList[i].DoingTaskVal1, client.TaskDataList[i].DoingTaskVal2, client.TaskDataList[i].DoingTaskFocus);

                            //如果任务已经完成
                            if (Global.JugeTaskComplete(client.TaskDataList[i].DoingTaskID, client.TaskDataList[i].DoingTaskVal1, client.TaskDataList[i].DoingTaskVal2))
                            {
                                // NPC的任务状态更新通知
                                int destNPC = systemTask.GetIntValue("DestNPC");
                                if (-1 != destNPC)
                                {
                                    int state = Global.ComputeNPCTaskState(client, client.TaskDataList, destNPC);
                                    GameManager.ClientMgr.NotifyUpdateNPCTaskSate(sl, pool, client, (destNPC + SpriteBaseIds.NpcBaseId), state);
                                }
                            }
                        }
                    }
                    else if (2 == valType)
                    {
                        bool updateTask = false;
                        if (client.TaskDataList[i].DoingTaskVal2 < systemTask.GetIntValue("TargetNum2"))
                        {
                            client.TaskDataList[i].DoingTaskVal2 = Global.GMin(taskVal, systemTask.GetIntValue("TargetNum2"));

                            //异步写数据库，更新任务
                            GameManager.DBCmdMgr.AddDBCmd((int)TCPGameServerCmds.CMD_DB_UPDATETASK_CMD,
                                string.Format("{0}:{1}:{2}:{3}:{4}:{5}",
                                client.RoleID,
                                client.TaskDataList[i].DoingTaskID,
                                client.TaskDataList[i].DbID,
                                client.TaskDataList[i].DoingTaskFocus,
                                client.TaskDataList[i].DoingTaskVal1,
                                client.TaskDataList[i].DoingTaskVal2),
                                null, client.ServerId);

                            updateTask = true;
                        }

                        if (updateTask)
                        {
                            //向客户端发布更新任务的信息
                            //任务更新通知
                            GameManager.ClientMgr.NotifyUpdateTask(sl, pool, client, client.TaskDataList[i].DbID, taskid, client.TaskDataList[i].DoingTaskVal1, client.TaskDataList[i].DoingTaskVal2, client.TaskDataList[i].DoingTaskFocus);

                            //如果任务已经完成
                            if (Global.JugeTaskComplete(client.TaskDataList[i].DoingTaskID, client.TaskDataList[i].DoingTaskVal1, client.TaskDataList[i].DoingTaskVal2))
                            {
                                // NPC的任务状态更新通知
                                int destNPC = systemTask.GetIntValue("DestNPC");
                                if (-1 != destNPC)
                                {
                                    int state = Global.ComputeNPCTaskState(client, client.TaskDataList, destNPC);
                                    GameManager.ClientMgr.NotifyUpdateNPCTaskSate(sl, pool, client, (destNPC + SpriteBaseIds.NpcBaseId), state);
                                }
                            }
                        }
                    }

                    break;
                }
            }
        }

        /// <summary>
        /// 直接清空任务道具
        /// </summary>
        /// <param name="client"></param>
        /// <param name="npcID"></param>
        /// <param name="goodsID"></param>
        /// <param name="taskType"></param>
        public static void ClearTaskGoods(SocketListener sl, TCPOutPacketPool pool, KPlayer client, int taskID)
        {
            if (null == client.TaskDataList) return;
            TaskData taskData = Global.GetTaskData(client, taskID);
            if (null == taskData)
            {
                return;
            }

            SystemXmlItem systemTask = null;

            //如果没有找到任务
            if (!GameManager.SystemTasksMgr.SystemXmlItemDict.TryGetValue(taskData.DoingTaskID, out systemTask))
            {
                return;
            }

            if (systemTask.GetIntValue("TargetType1") == (int)TaskTypes.TransferSomething && "" != systemTask.GetStringValue("PropsName1"))
            {
                //检测物品包中有没有指定的物品
                string propsName = systemTask.GetStringValue("PropsName1");
                string goodsName = GetPropNameGoodsName(propsName);
                int goodsLevel = GetPropNameGoodsLevel(propsName);
                int goodsQuality = GetPropNameGoodsQuality(propsName);

                int transferGoodsID = Global.GetGoodsByName(goodsName);
                if (transferGoodsID >= 0)
                {
                    GoodsData goodsData = Global.GetNotUsingGoodsByID(client, transferGoodsID, goodsLevel, goodsQuality);
                    if (null != goodsData) //已经拥有该物品，可以完成交付操作
                    {
                        //装备类的不扣除
                        int catetoriy = Global.GetGoodsCatetoriy(transferGoodsID);
                        if (catetoriy >= (int)ItemCategories.EquipMax)
                        {
                            //先从背包中扣除一个该物品, 因为可能多个在使用一个数据库记录
                            GameManager.ClientMgr.NotifyUseGoods(sl, Global._TCPManager.tcpClientPool, pool, client, goodsData.Id, false);
                        }
                    }
                }
            }
            else if (systemTask.GetIntValue("TargetType1") == (int)TaskTypes.UseSomething && "" != systemTask.GetStringValue("PropsName1"))
            {
                //检测物品包中有没有指定的物品
                string goodsName = systemTask.GetStringValue("PropsName1");
                int transferGoodsID = Global.GetGoodsByName(goodsName);
                if (transferGoodsID >= 0)
                {
                    GoodsData goodsData = Global.GetNotUsingGoodsByID(client, transferGoodsID, 0, 0);
                    if (null != goodsData) //已经拥有该物品，可以完成交付操作
                    {
                        //装备类的不扣除
                        int catetoriy = Global.GetGoodsCatetoriy(transferGoodsID);
                        if (catetoriy >= (int)ItemCategories.EquipMax)
                        {
                            //先从背包中扣除一个该物品, 因为可能多个在使用一个数据库记录
                            GameManager.ClientMgr.NotifyUseGoods(sl, Global._TCPManager.tcpClientPool, pool, client, goodsData.Id, false);
                        }
                    }
                }
            }

            if (systemTask.GetIntValue("TargetType2") == (int)TaskTypes.TransferSomething && "" != systemTask.GetStringValue("PropsName2"))
            {
                //检测物品包中有没有指定的物品
                string propsName = systemTask.GetStringValue("PropsName2");
                string goodsName = GetPropNameGoodsName(propsName);
                int goodsLevel = GetPropNameGoodsLevel(propsName);
                int goodsQuality = GetPropNameGoodsQuality(propsName);

                int transferGoodsID = Global.GetGoodsByName(goodsName);
                if (transferGoodsID >= 0)
                {
                    GoodsData goodsData = Global.GetNotUsingGoodsByID(client, transferGoodsID, goodsLevel, goodsQuality);
                    if (null != goodsData) //已经拥有该物品，可以完成交付操作
                    {
                        //装备类的不扣除
                        int catetoriy = Global.GetGoodsCatetoriy(transferGoodsID);
                        if (catetoriy >= (int)ItemCategories.EquipMax)
                        {
                            //先从背包中扣除一个该物品, 因为可能多个在使用一个数据库记录
                            GameManager.ClientMgr.NotifyUseGoods(sl, Global._TCPManager.tcpClientPool, pool, client, goodsData.Id, false);
                        }
                    }
                }
            }
            else if (systemTask.GetIntValue("TargetType2") == (int)TaskTypes.UseSomething && "" != systemTask.GetStringValue("PropsName2"))
            {
                //检测物品包中有没有指定的物品
                string goodsName = systemTask.GetStringValue("PropsName2");
                int transferGoodsID = Global.GetGoodsByName(goodsName);
                if (transferGoodsID >= 0)
                {
                    GoodsData goodsData = Global.GetNotUsingGoodsByID(client, transferGoodsID, 0, 0);
                    if (null != goodsData) //已经拥有该物品，可以完成交付操作
                    {
                        //装备类的不扣除
                        int catetoriy = Global.GetGoodsCatetoriy(transferGoodsID);
                        if (catetoriy >= (int)ItemCategories.EquipMax)
                        {
                            //先从背包中扣除一个该物品, 因为可能多个在使用一个数据库记录
                            GameManager.ClientMgr.NotifyUseGoods(sl, Global._TCPManager.tcpClientPool, pool, client, goodsData.Id, false);
                        }
                    }
                }
            }
        }

        #endregion Với nhiệm vụ đang được thực hiện

        #region 完成任务

        /// <summary>
        /// 完成任务   -- 增加一个参数  如果是一键完成日常跑环任务 几个条件都放过[12/10/2013 LiaoWei]
        /// </summary>
        public static bool Complete(SocketListener sl, TCPOutPacketPool pool, KPlayer client, int npcID, int extensionID, int taskID, int dbID, bool useYuanBao, double expBeiShu = 1.0, bool bIsOneClickComlete = false)
        {
            if (null == client.TaskDataList) return false;

            //首先判断任务是否真的已经完成
            //从内存中删除
            //添加到已经完成列表中
            //通知客户端此任务已经完成
            //奖励用户

            int findIndex = -1;

            lock (client.TaskDataList)
            {
                for (int i = 0; i < client.TaskDataList.Count; i++)
                {
                    if (client.TaskDataList[i].DbID == dbID)
                    {
                        // 一键完成日常跑环 [12/10/2013 LiaoWei]
                        if (bIsOneClickComlete == true)
                        {
                            findIndex = i;
                            break;
                        }
                        else
                        {
                            if (Global.JugeTaskComplete(taskID, client.TaskDataList[i].DoingTaskVal1, client.TaskDataList[i].DoingTaskVal2))
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

            Global.AddOldTask(client, taskID);

            //修改目标NPC的状态
            SystemXmlItem systemTask = null;
            if (!GameManager.SystemTasksMgr.SystemXmlItemDict.TryGetValue(taskID, out systemTask))
            {
                return false;
            }

            //如果是跑环任务，则跳过，后边单独处理
            int taskClass = systemTask.GetIntValue("TaskClass");
            if (taskClass >= (int)TaskClasses.CircleTaskStart && taskClass <= (int)TaskClasses.CircleTaskEnd) //如果是跑环任务  // 新增任务类型 -- 8号 日常任务(Mu项目) [12/3/2013 LiaoWei]
            {
                int minLevel = systemTask.GetIntValue("MinLevel");

                //更新日跑环的数据
                if (!Global.UpdateDailyTaskData(client, minLevel / 10, taskData.AddDateTime, taskClass, bIsOneClickComlete))
                    return false;
            }

            //向客户端发布更新任务的信息
            //任务更新通知
            GameManager.ClientMgr.NotifyUpdateTask(sl, pool, client, -1, taskID, 0, 0, 0);

            // NPC的任务状态更新通知
            int state = Global.ComputeNPCTaskState(client, client.TaskDataList, (npcID - SpriteBaseIds.NpcBaseId));
            GameManager.ClientMgr.NotifyUpdateNPCTaskSate(sl, pool, client, npcID, state);

            int sourceNPC = systemTask.GetIntValue("SourceNPC");
            if (-1 != sourceNPC && (npcID - SpriteBaseIds.NpcBaseId) != sourceNPC)
            {
                state = Global.ComputeNPCTaskState(client, client.TaskDataList, sourceNPC);
                GameManager.ClientMgr.NotifyUpdateNPCTaskSate(sl, pool, client, (sourceNPC + SpriteBaseIds.NpcBaseId), state);
            }

            bool usedBinding = false;
            bool usedTimeLimited = false;

            if ((systemTask.GetIntValue("TargetType1") == (int)TaskTypes.MonsterSomething || systemTask.GetIntValue("TargetType1") == (int)TaskTypes.CaiJiGoods) && "" != systemTask.GetStringValue("PropsName1"))
            {
                //检测物品包中有没有指定的物品
                int targetNum1 = systemTask.GetIntValue("TargetNum1");
                string goodsName = systemTask.GetStringValue("PropsName1");
                int transferGoodsID = Global.GetGoodsByName(goodsName);
                if (transferGoodsID >= 0) //真实物品，所以要扣除
                {
                    //装备类的不扣除
                    int catetoriy = Global.GetGoodsCatetoriy(transferGoodsID);
                    if (catetoriy >= (int)ItemCategories.EquipMax)
                    {
                        usedBinding = false;

                        //先从背包中扣除一个该物品, 因为可能多个在使用一个数据库记录
                        GameManager.ClientMgr.NotifyUseGoods(sl, Global._TCPManager.tcpClientPool, pool, client, transferGoodsID, targetNum1, false, out usedBinding, out usedTimeLimited);
                    }
                }
            }

            if ((systemTask.GetIntValue("TargetType2") == (int)TaskTypes.MonsterSomething || systemTask.GetIntValue("TargetType2") == (int)TaskTypes.CaiJiGoods) && "" != systemTask.GetStringValue("PropsName2"))
            {
                //检测物品包中有没有指定的物品
                int targetNum2 = systemTask.GetIntValue("TargetNum2");
                string goodsName = systemTask.GetStringValue("PropsName2");
                int transferGoodsID = Global.GetGoodsByName(goodsName);
                if (transferGoodsID >= 0) //真实物品，所以要扣除
                {
                    //装备类的不扣除
                    int catetoriy = Global.GetGoodsCatetoriy(transferGoodsID);
                    if (catetoriy >= (int)ItemCategories.EquipMax)
                    {
                        usedBinding = false;

                        //先从背包中扣除一个该物品, 因为可能多个在使用一个数据库记录
                        GameManager.ClientMgr.NotifyUseGoods(sl, Global._TCPManager.tcpClientPool, pool, client, transferGoodsID, targetNum2, false, out usedBinding, out usedTimeLimited);
                    }
                }
            }

            //判断是否需要元宝完成
            int needYuanBao = GameManager.TaskAwardsMgr.FindNeedYuanBao(client, taskID);

            // 跑环任务额外奖励
            int nAddExp = 0;
            int nAddMoJing = 0;
            int nAddGoodID = 0;
            int nGoodNum = 0;
            int nBinding = 1;
            int nAddXingHun = 0;
            bool bIsDailyCircleTask = false;
            TaskStarDataInfo TaskStarInfoTmp = null;

            //额外奖励的绑定元宝（绑钻）
            int nExBindYuanBao = 0;

            //日常任务的额外奖励
            if (taskClass == (int)TaskClasses.DailyTask)
            {
                DailyTaskData dailyTaskData = Global.FindDailyTaskDataByTaskClass(client, taskClass);
                if (dailyTaskData != null && taskData.StarLevel > 0 && taskData.StarLevel <= Data.TaskStarInfo.Count)
                {
                    if (dailyTaskData.RecNum == Global.GetMaxDailyTaskNum(client, taskClass, dailyTaskData))
                    {
                        int nIndex = Global.GetDailyCircleTaskAddAward(client);
                        if (nIndex > 0)
                        {
                            nAddExp = Data.DailyCircleTaskAward[nIndex].Experience;
                            nAddMoJing = Data.DailyCircleTaskAward[nIndex].MoJing;
                            nAddGoodID = Data.DailyCircleTaskAward[nIndex].GoodsID;
                            nGoodNum = Data.DailyCircleTaskAward[nIndex].GoodsNum;
                            nBinding = Data.DailyCircleTaskAward[nIndex].Binding;
                            nAddXingHun = Data.DailyCircleTaskAward[nIndex].XingHun;
                        }
                    }

                    // 日常跑环任务的星级 将会给额外的奖励
                    TaskStarInfoTmp = Data.TaskStarInfo[taskData.StarLevel - 1];

                    if (TaskStarInfoTmp != null)
                        bIsDailyCircleTask = true;
                }

                // 每日活跃中完成日常跑环任务的处理 用5的倍数来处理 节省服务器性能 [2/26/2014 LiaoWei]
                //if (dailyTaskData.RecNum % 5 == 0)
                {
                    DailyActiveManager.ProcessCompleteDailyTaskForDailyActive(client, dailyTaskData.RecNum);
                }
            }
            else if (taskClass == (int)TaskClasses.TaofaTask)    //讨伐任务的额外奖励
            {
                DailyTaskData dailyTaskData = Global.FindDailyTaskDataByTaskClass(client, taskClass);
                if (dailyTaskData != null)
                {   //完成了全部环
                    if (dailyTaskData.RecNum == Global.GetMaxDailyTaskNum(client, taskClass, dailyTaskData))
                    {
                        nExBindYuanBao = Data.TaofaTaskExAward.BangZuan;
                    }
                }
            }

            //要先奖励经验，否则升级时的装备将无法提示穿戴上去
            //奖励用户经验
            //异步写数据库，写入经验和级别
            long experience = GameManager.TaskAwardsMgr.FindExperience(client, taskID);

            if (experience > 0)
            {
                if (useYuanBao)
                {
                    experience *= 2;
                }

                experience = (long)(experience * expBeiShu);

                if (bIsDailyCircleTask)
                    experience = (long)(experience * TaskStarInfoTmp.ExpModulus);

                experience += nAddExp;

                //处理角色经验
                GameManager.ClientMgr.ProcessRoleExperience(client, experience, true, false);
            }

            if (Global.FilterFallGoods(client)) //是否奖励物品
            {
                //奖励用户装备物品
                //List<AwardsItemData> awardsItemList = GameManager.TaskAwardsMgr.FindTaskAwards(taskID);

                List<GoodsData> goodsDataList = null;
                goodsDataList = Global.GetTaskAwardsGoodsGridCount(client, taskID);

                if (nAddGoodID > 0 && nGoodNum > 0)
                {
                    GoodsData addGood = new GoodsData();
                    addGood.GoodsID = nAddGoodID;
                    addGood.GCount = nGoodNum;
                    addGood.Binding = nBinding;
                    addGood.Endtime = Global.ConstGoodsEndTime;

                    if (goodsDataList == null)
                        goodsDataList = new List<GoodsData>();

                    goodsDataList.Add(addGood);
                }

                if (null != goodsDataList && goodsDataList.Count > 0)
                {
                    // 如果没有背包格子 则发邮件  说明 -- 如果不是一键完成每日跑环任务 是不会执行这段代码的 因为 其他任务的提交提交都会检测包裹 [12/6/2013 LiaoWei]
                    if (!Global.CanAddGoodsDataList(client, goodsDataList))
                    {
                        SendMailWhenPacketFull(client, goodsDataList);
                    }
                    else
                    {
                        for (int i = 0; i < goodsDataList.Count; i++)
                        {
                            //像DBServer请求加入某个新的物品到背包中
                            //添加物品
                            //Global.AddGoodsDBCommand(pool, client, goodsDataList[i].GoodsID, goodsDataList[i].GCount, goodsDataList[i].Quality, "",
                            //                            goodsDataList[i].Forge_level, goodsDataList[i].Binding, 0, "", true, 1, "任务奖励", goodsDataList[i].Endtime,
                            //                            0, goodsDataList[i].BornIndex, goodsDataList[i].Lucky, 0, goodsDataList[i].ExcellenceInfo, goodsDataList[i].AppendPropLev);

                            // ADD PHẦN THƯỞNG VÀO DB
                        }
                    }
                }
            }

            //奖励用户金钱
            //异步写数据库，写入金钱
            int money = GameManager.TaskAwardsMgr.FindMoney(taskID);
            if (0 < money)
            {
                //过滤金币奖励
                money = Global.FilterValue(client, money);

                //更新用户的铜钱
                GameManager.ClientMgr.AddMoney1(Global._TCPManager.MySocketListener, Global._TCPManager.tcpClientPool, Global._TCPManager.TcpOutPacketPool, client, money, "完成任务：" + taskID, false);

                GameManager.SystemServerEvents.AddEvent(string.Format("角色获取金钱, roleID={0}({1}), Money={2}, newMoney={3}", client.RoleID, client.RoleName, client.Money1, money), EventLevels.Record);
            }

            //奖励用户绑定元宝
            //异步写数据库，写入金币
            int bindYuanBao = GameManager.TaskAwardsMgr.FindBindYuanBao(taskID);
            bindYuanBao += nExBindYuanBao;
            if (0 < bindYuanBao)
            {
                //过滤绑定元宝奖励
                bindYuanBao = Global.FilterValue(client, bindYuanBao);

                GameManager.ClientMgr.AddUserGold(Global._TCPManager.MySocketListener, Global._TCPManager.tcpClientPool, Global._TCPManager.TcpOutPacketPool, client, bindYuanBao, "完成任务：" + taskID);

                GameManager.SystemServerEvents.AddEvent(string.Format("角色获取绑定元宝, roleID={0}({1}), Money={2}, newMoney={3}", client.RoleID, client.RoleName, client.Gold, bindYuanBao), EventLevels.Record);
            }

            //20环额外奖励魔晶
            if (0 < nAddMoJing)
            {
                GameManager.ClientMgr.ModifyTianDiJingYuanValue(client, nAddMoJing, "日常跑环", false, true);
            }

            //奖励用户真气
            int zhenQi = GameManager.TaskAwardsMgr.FindZhenQi(client, taskID);
            if (zhenQi > 0)
            {
                //过滤奖励
                zhenQi = Global.FilterValue(client, zhenQi);

                //角色添加阵旗
                GameManager.ClientMgr.ModifyZhenQiValue(client, zhenQi, true, true);
                GameManager.SystemServerEvents.AddEvent(string.Format("角色获取任务阵旗成功, roleID={0}({1}), newBlessPoint={2}, RetCode={3}", client.RoleID, client.RoleName, zhenQi, 0), EventLevels.Record);
            }

            //奖励用户猎杀值
            int lieSha = GameManager.TaskAwardsMgr.FindLieSha(client, taskID);
            if (lieSha > 0)
            {
                //过滤奖励
                lieSha = Global.FilterValue(client, lieSha);
                if (useYuanBao)
                {
                    lieSha *= 2;
                }

                //角色添加猎杀之
                GameManager.ClientMgr.ModifyLieShaValue(client, lieSha, true, true);
                GameManager.SystemServerEvents.AddEvent(string.Format("角色获取任务猎杀值成功, roleID={0}({1}), newBlessPoint={2}, RetCode={3}", client.RoleID, client.RoleName, lieSha, 0), EventLevels.Record);
            }

            //奖励用户悟性值
            int wuXing = GameManager.TaskAwardsMgr.FindWuXing(client, taskID);
            if (wuXing > 0)
            {
                //过滤奖励
                wuXing = Global.FilterValue(client, wuXing);

                //角色添加悟性值
                GameManager.ClientMgr.ModifyWuXingValue(client, wuXing, true, true);
                GameManager.SystemServerEvents.AddEvent(string.Format("角色获取任务悟性值成功, roleID={0}({1}), newBlessPoint={2}, RetCode={3}", client.RoleID, client.RoleName, wuXing, 0), EventLevels.Record);
            }

            //奖励用户军功值
            int junGong = GameManager.TaskAwardsMgr.FindJunGong(client, taskID);
            if (junGong > 0)
            {
                //过滤奖励
                junGong = Global.FilterValue(client, junGong);

                //角色添加军功值
                GameManager.ClientMgr.ModifyJunGongValue(client, junGong, true, true);
                GameManager.SystemServerEvents.AddEvent(string.Format("角色获取任务军功值成功, roleID={0}({1}), newBlessPoint={2}, RetCode={3}", client.RoleID, client.RoleName, junGong, 0), EventLevels.Record);
            }

            //奖励用户荣誉值
            int rongYu = GameManager.TaskAwardsMgr.FindRongYu(client, taskID);
            if (rongYu > 0)
            {
                //过滤奖励
                rongYu = Global.FilterValue(client, rongYu);

                //角色添加荣誉值
                GameManager.ClientMgr.ModifyRongYuValue(client, rongYu, true, true);
                GameManager.SystemServerEvents.AddEvent(string.Format("角色获取任务荣誉值成功, roleID={0}({1}), newBlessPoint={2}, RetCode={3}", client.RoleID, client.RoleName, rongYu, 0), EventLevels.Record);
            }

            // 奖励魔晶 [4/10/2014 LiaoWei]
            int nMoJing = GameManager.TaskAwardsMgr.FindMoJing(client, taskID);
            if (nMoJing > 0)
            {
                //过滤奖励
                nMoJing = Global.FilterValue(client, nMoJing);

                if (bIsDailyCircleTask)
                    nMoJing = (int)(nMoJing * TaskStarInfoTmp.BindYuanBaoModulus);

                //角色添加魔晶值
                GameManager.ClientMgr.ModifyTianDiJingYuanValue(client, nMoJing, "过滤奖励", false, true);

                GameManager.SystemServerEvents.AddEvent(string.Format("角色获取任务魔晶值成功, roleID={0}({1}), newBlessPoint={2}, RetCode={3}", client.RoleID, client.RoleName, nMoJing, 0), EventLevels.Record);
            }

            // 奖励星魂 [8/11/2014 LiaoWei]
            int nXingHun = GameManager.TaskAwardsMgr.FindXingHun(client, taskID);

            if (nXingHun > 0)
            {
                nXingHun = Global.FilterValue(client, nXingHun);

                if (bIsDailyCircleTask)
                    nXingHun = (int)(nXingHun * TaskStarInfoTmp.StarSoulModulus);

                nXingHun += nAddXingHun;

                GameManager.ClientMgr.ModifyStarSoulValue(client, nXingHun, "过滤奖励", true, true);

                //client.StarSoul += nXingHun;
                //Global.SaveRoleParamsInt32ValueToDB(client, RoleParamName.StarSoul, client.StarSoul, true);
                //GameManager.ClientMgr.NotifySelfParamsValueChange(client, RoleCommonUseIntParamsIndexs.StarSoulValue, client.StarSoul);
            }

            // 由于引导而增加给随身仓库放奖励物品 本来是要在SystemTasks.xml加字段的 但考虑只有1个任务需要这样做 就在SystemParams.xml里写死任务和物品了 [4/9/2014 LiaoWei]
            if (taskID == Data.InsertAwardtPortableBagTaskID)
            {
                string[] strID = null;
                strID = Data.InsertAwardtPortableBagGoodsInfo.Split(',');

                if (strID != null)
                {
                    GoodsData goodsdata = new GoodsData();

                    goodsdata.GoodsID = Global.SafeConvertToInt32(strID[0]);
                    goodsdata.GCount = Global.SafeConvertToInt32(strID[1]);
                    goodsdata.Binding = Global.SafeConvertToInt32(strID[2]);
                    goodsdata.Forge_level = Global.SafeConvertToInt32(strID[3]);

                    goodsdata.Site = (int)SaleGoodsConsts.PortableGoodsID;
                    // ADD ĐỒ VÀO DB
                    //Global.AddPortableGoodsData(client, goodsdata);
                }
            }

            //写入角色完成任务的行为日志
            Global.AddRoleTaskEvent(client, taskID);

            //更新任务章节完成度
            if (taskClass == (int)TaskClasses.MainTask)
            {
                Global.UpdateTaskZhangJieProp(client, taskID);
            }

            return true;
        }

        /// <summary>
        /// 一键完成任务时 背包满了--发邮件 [6/24/2014 LiaoWei]
        /// </summary>
        public static void SendMailWhenPacketFull(KPlayer client, List<GoodsData> awardsItemList)
        {
            int nTotalGroup = 0;
            nTotalGroup = awardsItemList.Count / 5;

            int nRemain = 0;
            nRemain = awardsItemList.Count % 5;

            int nCount = 0;

            if (nTotalGroup > 0)
            {
                for (int i = 0; i < nTotalGroup; ++i)
                {
                    List<GoodsData> goods = new List<GoodsData>();

                    for (int n = 0; n < 5; ++n)
                    {
                        goods.Add(awardsItemList[nCount]);
                        ++nCount;
                    }

                    Global.UseMailGivePlayerAward2(client, goods, Global.GetLang("每日跑环任务奖励"), Global.GetLang("一键完成每日跑环任务"));
                }
            }

            if (nRemain > 0)
            {
                List<GoodsData> goods1 = new List<GoodsData>();
                for (int i = 0; i < nRemain; ++i)
                {
                    goods1.Add(awardsItemList[nCount]);
                    ++nCount;
                }

                Global.UseMailGivePlayerAward2(client, goods1, Global.GetLang("每日跑环任务奖励"), Global.GetLang("一键完成每日跑环任务"));
            }

            /*string mailGoodsString = "";
            foreach (var item in awardsItemList)
            {
                int useCount = item.GoodsNum;

                mailGoodsString += string.Format("{0}_{1}_{2}_{3}_{4}_{5}_{6}_{7}_{8}_{9}_{10}_{11}_{12}_{13}_{14}_{15}", item.GoodsID, item.Level, item.Quality, 0,
                                                    useCount, 0, 0, 0, 0, item.Binding, item.BornIndex, item.IsHaveLuckyProp, 0, item.ExcellencePorpValue, item.AppendLev, 0); // 卓越信息 [12/14/2013 LiaoWei]

                if (mailGoodsString.Length > 0)
                    mailGoodsString += "|";

                string strDbCmd = string.Format("{0}:{1}:{2}:{3}:{4}:{5}:{6}:{7}:{8}:{9}", -1, "系统", client.RoleID,
                                                    client.RoleName, "每日跑环任务奖励", "一键完成每日跑环任务", 0, 0, 0, mailGoodsString);

                Global.ExecuteDBCmd((int)TCPGameServerCmds.CMD_DB_SENDUSERMAIL, strDbCmd);
            }*/
        }

        #endregion 完成任务
    }
}