using GameServer.KiemThe.Core.Item;
using GameServer.KiemThe.Logic;
using GameServer.KiemThe.LuaSystem.Logic;
using GameServer.Logic;
using GameServer.Server;
using Server.Data;
using Server.Protocol;
using Server.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static GameServer.Logic.KTMapManager;

namespace GameServer.KiemThe.Core.Task
{
    /// <summary>
    /// Quản lý nhiệm vụ hải tặc
    /// </summary>
    public class PirateTaskManager
    {
        private static PirateTaskManager instance = new PirateTaskManager();

        public Dictionary<int, Task> _TotalTaskData = new Dictionary<int, Task>();

        public static PirateTaskManager getInstance()
        {
            return instance;
        }

        //Lấy ra số lượng nhiệm vụ trong ngày
        public int GetNumQuestThisDay(KPlayer client)
        {
            int Count = client.GetValueOfDailyRecore((int)DailyRecord.PirateTaskNumber);

            if (Count == -1)
            {
                Count = 0;
            }

            return Count;
        }

        /// <summary>
        /// Set số lượng quest đã làm trong ngày
        /// </summary>
        /// <param name="client"></param>
        /// <param name="Value"></param>
        public void SetNumQuestThisDay(KPlayer client, int Value)
        {
            client.SetValueOfDailyRecore((int)DailyRecord.PirateTaskNumber, Value);
        }

        /// <summary>
        /// Lấy ra quest ID trong ngày
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        public int GeQuestIDDay(KPlayer client)
        {
            int Count = client.GetValueOfDailyRecore((int)DailyRecord.PirateTaskID);

            if (Count == -1)
            {
                Count = 0;
            }

            return Count;
        }

        /// <summary>
        /// Set ra quest ID đã nhận trong ngày
        /// </summary>
        /// <param name="client"></param>
        /// <param name="Value"></param>
        public void SetQuestIDDay(KPlayer client, int Value)
        {
            client.SetValueOfDailyRecore((int)DailyRecord.PirateTaskID, Value);
        }

        /// <summary>
        /// Set vào danh sách nhiệm vụ
        /// </summary>
        /// <param name="_InPut"></param>
        public void SetTask(Dictionary<int, Task> _InPut)
        {
            this._TotalTaskData = _InPut;
        }

        public Task GetTaskTemplate(int ID)
        {
            if (_TotalTaskData.ContainsKey(ID))
            {
                return _TotalTaskData[ID];
            }
            else
            {
                return null;
            }
        }

        public bool IsHaveQuest(int NpcID, KPlayer client)
        {
            List<int> tasksList = null;

            if (!GameManager.NPCTasksMgr.SourceNPCTasksDict.TryGetValue(NpcID, out tasksList))

                return false;

            if (0 == tasksList.Count)

                return false;

            bool IsHaveQuest = false;

            foreach (int TaskID in tasksList)
            {
                if (this.CanTakeNewTask(client, TaskID))
                {
                    IsHaveQuest = true;
                    break;
                }

                Task _task = this.GetTaskTemplate(TaskID);
                if (_task.TargetType == (int)TaskTypes.AnswerQuest)
                {
                    TaskData _Task = TaskManager.getInstance().GetTaskData(TaskID, client);
                    if (_Task != null)
                    {
                        // Nếu đang trả lời dở thì cần phải trả lời tiếp
                        if (!TaskManager.getInstance().IsQuestComplete(TaskID, _Task.DoingTaskVal1))
                        {
                            IsHaveQuest = true;
                            break;
                        }
                    }
                }
            }

            return IsHaveQuest;
        }

        public bool IsHaveCompleteQuest(int NpcID, KPlayer Client)
        {
            List<int> tasksList = null;

            if (!GameManager.NPCTasksMgr.DestNPCTasksDict.TryGetValue(NpcID, out tasksList))
                return false;   //Nếu thằng nPC này không có nhiệm vụ này thì chim cút

            if (0 == tasksList.Count)
                return false;   /// Nếu mà số lượng task trả về ==0 thì cũng chim cút

            // Nếu danh sách nhiệm vụ hiện có mà đéo có task nào thì chứng tỏ thằng này chưa làm nhiệm vụ nào==> Đéo có nhiệm vụ nào có thể hoàn tất
            if (Client.TaskDataList == null)
            {
                return false;
            }

            TCPOutPacketPool pool = Global._TCPManager.TcpOutPacketPool;
            TCPClientPool tcpClientPool = Global._TCPManager.tcpClientPool;

            // Nếu thằng này có nhiệm vụ có thể trả thì cứ check nếu có dạng nhiệm vụ dạng nói chuyện
            ProcessTask.Process(Global._TCPManager.MySocketListener, pool, Client, NpcID, NpcID, -1, TaskTypes.Talk);

            bool IsHaveComplteQuest = false;

            foreach (int TaskSelect in tasksList)
            {
                // Xuống đây check lại 1 lượt nữa phòng khi ở trên đã hoàn thành nhiệm vụ
                if (Client.TaskDataList == null)
                {
                    break;
                }
                if (Client.TaskDataList.Count == 0)
                {
                    break;
                }

                var find = Client.TaskDataList.Where(x => x.DoingTaskID == TaskSelect).FirstOrDefault();
                if (find != null)
                {
                    if (TaskManager.getInstance().IsQuestComplete(find.DoingTaskID, find.DoingTaskVal1))
                    {
                        IsHaveComplteQuest = true;
                        break;
                    }
                }
            }

            return IsHaveComplteQuest;
        }

        public bool IsHavePitateQuest(KPlayer client)
        {
            if (client.TaskDataList != null)
            {
                foreach (TaskData task in client.TaskDataList)
                {
                    Task TaskCheck = _TotalTaskData.Values.Where(x => x.ID == task.DoingTaskID).FirstOrDefault();

                    if (TaskCheck != null)
                    {
                        if (TaskCheck.TaskClass == (int)TaskClasses.HaiTac)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        public bool CanGetTaskPirate(KPlayer client, NPC npc, int TaskSelect, int InputMapcode, KPlayer clienthost)
        {
            // Check thử có đang có nhiệm vụ hải tặc nào khác trên người không
            if (client.TaskDataList != null)
            {
                foreach (TaskData task in client.TaskDataList)
                {
                    Task TaskCheck = _TotalTaskData.Values.Where(x => x.ID == task.DoingTaskID).FirstOrDefault();

                    if (TaskCheck != null)
                    {
                        if (TaskCheck.TaskClass == (int)TaskClasses.HaiTac)
                        {
                            KTLuaLib_Player.SendMSG("Người chơi :" + client.RoleName + " đã nhận nhiệm vụ hải tặc.Không thể nhận thêm nhiệm vụ khác", clienthost, npc);
                            KTLuaLib_Player.SendMSG("Người chơi :" + client.RoleName + " đã nhận nhiệm vụ hải tặc.Không thể nhận thêm nhiệm vụ khác", client, npc);
                            return false;
                        }
                    }
                }
            }

            if (!CanTakeNewTask(client, TaskSelect))
            {
                KTLuaLib_Player.SendMSG("Người chơi :" + client.RoleName + " không đủ điều kiện nhận nhiệm vụ này", clienthost, npc);
                KTLuaLib_Player.SendMSG("Người chơi :" + client.RoleName + " không đủ điều kiện nhận nhiệm vụ này", client, npc);
                return false;
            }
            if (InputMapcode != client.CurrentMapCode)
            {
                KTLuaLib_Player.SendMSG("Người chơi :" + client.RoleName + " đang ở bản đồ khác không thể nhận nhiệm vụ", clienthost, npc);
                KTLuaLib_Player.SendMSG("Người chơi :" + client.RoleName + " đang ở bản đồ khác không thể nhận nhiệm vụ", client, npc);
                return false;
            }

            Task TaskConfig = _TotalTaskData.Values.Where(x => x.ID == TaskSelect).FirstOrDefault();

            if (TaskConfig != null)
            {
                if (TaskConfig.MinLevel <= client.m_Level && TaskConfig.MaxLevel >= client.m_Level)
                {
                    return true;
                }
                else
                {
                    KTLuaLib_Player.SendMSG("Người chơi :" + client.RoleName + " có cấp độ không phù hợp không thể nhận được nhiệm vụ hải tặc này", clienthost, npc);
                    KTLuaLib_Player.SendMSG("Người chơi :" + client.RoleName + " có cấp độ không phù hợp không thể nhận được nhiệm vụ hải tặc này", client, npc);
                    return false;
                }
            }

            return false;
        }

        /// <summary>
        /// Có thể nhận nhiệm vụ hải tặc này không
        /// </summary>
        /// <param name="client"></param>
        /// <param name="taskID"></param>
        /// <param name="systemTask"></param>
        /// <returns></returns>
        public bool CanTakeNewTask(KPlayer client, int taskID)
        {
            Task systemTask = this.GetTaskTemplate(taskID);

            if (systemTask == null)
            {
                return false;
            }

            ///Có đủ uy danh không
            if (client.Prestige < 20)
            {
                return false;
            }

            // TODO THÊM CHECK UY DANH
            if (client.m_Level < 50)
            {
                return false;
            }

            // Nếu hết lượt làm
            if (this.GetNumQuestThisDay(client) >= 6)
            {
                return false;
            }

            if (client.m_cPlayerFaction.GetFactionId() == 0)
            {
                return false;
            }

            // Nếu giới tính không phù hợp
            int taskSex = systemTask.SexCondition;
            if (0 != taskSex)
            {
                if (client.RoleSex != taskSex)
                {
                    return false;
                }
            }

            // Nếu phái không phù hợp
            int taskOccupation = systemTask.OccupCondition;
            if (0 != taskOccupation)
            {
                int nOcc = client.m_cPlayerFaction.GetFactionId();

                if (nOcc != taskOccupation)
                {
                    return false;
                }
            }

            int taskClass = systemTask.TaskClass;

            // Lấy ra cấp độ

            if (client.m_Level == 0)
            {
                client.m_Level = 1;
            }

            // Lấy ra MIN LEVEL
            int minLevel = systemTask.MinLevel;

            // Lấy ra MAX LEVEL
            int maxLevel = systemTask.MaxLevel;

            // Nếu không đủ cấp độ
            if (client.m_Level < minLevel || client.m_Level > maxLevel)
            {
                return false;
            }

            return true; //Return true
        }

        public void GetExchangeDialog(GameMap map, NPC npc, KPlayer client)
        {
            TCPOutPacketPool pool = Global._TCPManager.TcpOutPacketPool;
            TCPClientPool tcpClientPool = Global._TCPManager.tcpClientPool;
            KNPCDialog _NpcDialog = new KNPCDialog();

            Dictionary<int, string> Selections = new Dictionary<int, string>();

            Dictionary<int, string> OtherPram = new Dictionary<int, string>();

            string History = "Để đổi " + KTGlobal.CreateStringByColor("Võ Lâm Mật Tịch", ColorType.Yellow) + " hoặc " + KTGlobal.CreateStringByColor("Tẩy Tủy Kinh Sơ", ColorType.Yellow) + " người cần có 300 danh bổ lệnh\n\n";

            Selections.Add(100, KTGlobal.CreateStringByColor("Đổi Võ Lâm Mật Tịch", ColorType.Accpect));
            Selections.Add(200, KTGlobal.CreateStringByColor("Tẩy Tủy Kinh Sơ", ColorType.Accpect));

            Action<TaskCallBack> ActionWork = (x) => PirateQuestProsecc(map, npc, client, x, -1);

            Selections.Add(4, KTGlobal.CreateStringByColor("Thông tin", ColorType.Normal));

            _NpcDialog.Text = History;

            _NpcDialog.OnSelect = ActionWork;

            _NpcDialog.Selections = Selections;

            _NpcDialog.OtherParams = OtherPram;

            _NpcDialog.Show(npc, client);
        }

        public void AppcepTask(NPC npc, KPlayer client, int TaskID, bool IsCloseATEnd = true)
        {
            Task _FindTask = this.GetTaskTemplate(TaskID);

            TCPOutPacketPool pool = Global._TCPManager.TcpOutPacketPool;
            TCPClientPool tcpClientPool = Global._TCPManager.tcpClientPool;

            if (_FindTask == null)
            {
                // Thực hiện đóng cửa sổ nhiệm vụ và thông báo tới người chơi
                KT_TCPHandler.CloseDialog(client);

                KTPlayerManager.ShowNotification(client, "Nhiệm vụ bạn vừa chọn không tồn tại!");
                return;
            }

            int taskClass = _FindTask.TaskClass;

            //Kiểm tra xem có đủ điều kiện nhân nhiệm vụ mới không
            if (!this.CanTakeNewTask(client, TaskID))
            {
                KT_TCPHandler.CloseDialog(client);

                KTPlayerManager.ShowNotification(client, "Bạn không thể nhận nhiệm vụ này");

                return;
            }

            string strcmd = "";

            // Add Data
            TaskData taskData = new TaskData()
            {
                DbID = -1,
                DoingTaskID = TaskID,
            };

            if (_FindTask.PropsName != "")
            {
                bool IsHaveSpaceGetQuest = KTGlobal.IsHaveSpace(1, client);

                if (!IsHaveSpaceGetQuest)
                {
                    KT_TCPHandler.CloseDialog(client);

                    KTPlayerManager.ShowNotification(client, "Hành trang không đủ chỗ trống để nhận nhiệm vụ");

                    return;
                }
            }

            int focus = 1;

            if (TaskManager.GetFocusTaskCount(client) >= KTGlobal.TaskMaxFocusCount)
            {
                focus = 0;
            }



            if (npc == null)
            {
                strcmd = string.Format("{0}:{1}:{2}:{3}:{4}", client.RoleID, -1, TaskID, focus, _FindTask.TargetType);
            }
            else
            {
                strcmd = string.Format("{0}:{1}:{2}:{3}:{4}", client.RoleID, npc.ResID, TaskID, focus, _FindTask.TargetType);
            }
            // Viết CMD lưu vào DB

            // Thực hiện ghi lại quest vào DB
            string[] fieldsData = null;

            if (TCPProcessCmdResults.RESULT_FAILED == Global.RequestToDBServer(tcpClientPool, pool, (int)TCPGameServerCmds.CMD_SPR_NEWTASK, strcmd, out fieldsData, client.ServerId))
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Toang khi kết nối với CSDL, CMD={0}", (int)TCPGameServerCmds.CMD_SPR_NEWTASK));

                KT_TCPHandler.CloseDialog(client);

                KTPlayerManager.ShowNotification(client, "Có lỗi khi nhận nhiệm vụ! ErrorCode : 1");

                return;
            }

            strcmd = "";
            if (Convert.ToInt32(fieldsData[3]) < 0) //Nếu Pram gửi về mà lỗi thì toang
            {
                taskData.DbID = Convert.ToInt32(fieldsData[3]);

                KT_TCPHandler.CloseDialog(client);

                KTPlayerManager.ShowNotification(client, "Có lỗi khi nhận nhiệm vụ! ErrorCode : " + taskData.DbID);

                return;
            }

            //Nếu mà mọi thứ đều ổn
            // Check xem list task client có null không nếu null thì tạo mới
            if (null == client.TaskDataList)
            {
                client.TaskDataList = new List<TaskData>();
            }

            taskData.DbID = Convert.ToInt32(fieldsData[3]);
            taskData.DoingTaskVal1 = 0;
            taskData.DoingTaskVal2 = 0;
            taskData.DoingTaskFocus = focus;
            taskData.AddDateTime = Convert.ToInt64(fieldsData[2]);
            taskData.DoneCount = 0;
            taskData.TaskType = _FindTask.TargetType;

            OldTaskData oldTaskData = TaskManager.FindOldTaskByTaskID(client, TaskID);
            if (null != oldTaskData)
            {
                taskData.DoneCount = oldTaskData.DoCount;
            }

            lock (client.TaskDataList)
            {
                client.TaskDataList.Add(taskData);
            }

            // Thực hiện send DATA TASK VỀ CLLIENT
            byte[] DataSend = DataHelper.ObjectToBytes<TaskData>(taskData);
            client.SendPacket((int)TCPGameServerCmds.CMD_SPR_NEWTASK, DataSend);

            int state = 0;
            int sourceNPC = _FindTask.SourceNPC;
            if (sourceNPC >= 0)
            {
                // Update STATE CỦA CLIENT
                state = TaskManager.getInstance().ComputeNPCTaskState(client, client.TaskDataList, sourceNPC);

                KT_TCPHandler.NotifyUpdateNPCTaskSate(client, sourceNPC, state);
            }

            // NPC TRẢ NHIỆM VỤ
            int destNPC = _FindTask.DestNPC;

            // NẾU NHIỆM VỤ CÓ NPC TRẢ THÌ UPDATE TRẠNG THÁI CHO THẰNG NPC TRẢ NHIỆM VỤ
            if (-1 != destNPC)
            {
                state = TaskManager.getInstance().ComputeNPCTaskState(client, client.TaskDataList, destNPC);
                KT_TCPHandler.NotifyUpdateNPCTaskSate(client, destNPC, state);
            }
            if (IsCloseATEnd)
            {
                KTPlayerManager.ShowNotification(client, "Nhận thành công nhiệm vụ [" + _FindTask.Title + "]");
                KT_TCPHandler.CloseDialog(client);
            }

            //Gọi hàm cập nhật tiến độ trước khi bắt đầu
            ProcessTask.ProseccTaskBeforeDoTask(Global._TCPManager.MySocketListener, pool, client);
        }

        public void PirateQuestProsecc(GameMap map, NPC npc, KPlayer client, TaskCallBack TaskData, int TaskID)
        {
            TCPOutPacketPool pool = Global._TCPManager.TcpOutPacketPool;

            if (TaskData != null)
            {
                int SelectID = TaskData.SelectID;

                var findTask = client.TaskDataList?.Where(x => x != null && x.DoingTaskID == TaskID).FirstOrDefault();
                if (findTask != null)
                {
                    // Nếu là hủy nhiệm vụ
                    if (SelectID == 3)
                    {
                        if (CancelTask(client, findTask.DbID, TaskID))
                        {
                            KTLuaLib_Player.SendMSG("Hủy nhiệm vụ thành công", client, npc);

                            KT_TCPHandler.NotifyUpdateTask(client, findTask.DbID, TaskID, -1, 0, 0);

                            int NumberSet = this.GetNumQuestThisDay(client) + 1;

                            this.SetNumQuestThisDay(client, NumberSet);

                            GiveTaskPirate(client, true);
                        }
                    }
                }

                if (SelectID == 2) // Nếu là hoàn thành nhiệm vụ
                {
                    this.CompleteTask(map, npc, client, TaskID);
                }
                else if (SelectID == 1) // Nếu là nhận nhiệm vụ
                {
                    // nếu có đội
                    if (client.TeamID != -1)
                    {
                        List<KPlayer> TotalMember = client.Teammates;

                        bool isOK = true;
                        foreach (KPlayer play in TotalMember)
                        {
                            if (!CanGetTaskPirate(play, npc, TaskID, client.CurrentMapCode, client))
                            {
                                isOK = false;
                                break;
                            }
                        }

                        if (isOK)
                        {
                            foreach (KPlayer play in TotalMember)
                            {
                                //SET TASK ID CHO TOÀN BỘ BỌN ĐỒNG ĐỘI
                                this.SetQuestIDDay(play, TaskID);

                                this.AppcepTask(npc, play, TaskID);
                            }
                        }
                    }
                    else
                    {
                        this.AppcepTask(npc, client, TaskID);
                    }
                }
                else if (SelectID == 4) // Nếu là nhận nhiệm vụ
                {
                    KTLuaLib_Player.SendMSG("Người chơi đạt cấp " + KTGlobal.CreateStringByColor("50", ColorType.Blue) + " trở nên , cần tối thiểu " + KTGlobal.CreateStringByColor("10", ColorType.Blue) + " uy danh giang hồ, có thể đến chỗ NPC " + KTGlobal.CreateStringByColor("Bổ Đầu Hình Bộ", ColorType.Green) + " tại các thành thị nhận nhiệm vụ\nMỗi ngày người chơi có thể nhận tối đa " + KTGlobal.CreateStringByColor("6", ColorType.Blue) + " nhiệm vụ\nCó thể săn một mình hoặc tổ đội với người chơi khác tối đa 6 người, khi tổ đội với người chơi khác, chỉ cần người chơi trong đội đứng trong thành thị mà đội trưởng nhận nhiệm vụ là có thể nhận nhiệm vụ đi săn cùng nhau\nKhi hoàn thành có thể nhận được nhiều phần thưởng có giá trị!", client, npc);
                }
                else if (SelectID == 5)
                {
                    this.GetExchangeDialog(map, npc, client);
                }
                else if (SelectID == 6)
                {
                    KT_TCPHandler.CloseDialog(client);
                }
                else if (SelectID == 100) // Nếu là nhận nhiệm vụ
                {
                    DoExChange(client, 100, npc);
                }
                else if (SelectID == 200) // Nếu là nhận nhiệm vụ
                {
                    DoExChange(client, 200, npc);
                }
            }
        }

        public void DoExChange(KPlayer client, int Number, NPC npc)
        {
            int ItemCount = ItemManager.GetItemCountInBag(client, 489);

            if (!KTGlobal.IsHaveSpace(1, client))
            {
                KTLuaLib_Player.SendMSG("Túi đồ không đủ chỗ trống", client, npc);
            }
            else
            {
                if (ItemCount < 300)
                {
                    KTLuaLib_Player.SendMSG("Số lượng danh bổ lệnh không đủ", client, npc);
                }
                else
                {
                    if (ItemManager.RemoveItemFromBag(client, 489, 300, -1, "Đổi phần thưởng hải tặc"))
                    {
                        if (Number == 100)
                        {
                            if (!ItemManager.CreateItem(Global._TCPManager.TcpOutPacketPool, client, 490, 1, 0, "SPLITITEM", false, 1, false, ItemManager.ConstGoodsEndTime))
                            {
                                KTPlayerManager.ShowNotification(client, "Có lỗi khi nhận vật phẩm chế tạo");
                            }
                        }
                        else if (Number == 200)
                        {
                            if (!ItemManager.CreateItem(Global._TCPManager.TcpOutPacketPool, client, 492, 1, 0, "SPLITITEM", false, 1, false, ItemManager.ConstGoodsEndTime))
                            {
                                KTPlayerManager.ShowNotification(client, "Có lỗi khi nhận vật phẩm chế tạo");
                            }
                        }
                    }
                }
            }
        }

        public void CompleteTask(GameMap map, NPC npc, KPlayer client, int TaskID)
        {
            TCPOutPacketPool pool = Global._TCPManager.TcpOutPacketPool;
            TCPClientPool tcpClientPool = Global._TCPManager.tcpClientPool;

            Task _TaskFind = this.GetTaskTemplate(TaskID);

            // Nếu nhiệm vụ không tồn tại thì send packet về là toang
            if (_TaskFind == null)
            {
                KT_TCPHandler.CloseDialog(client);

                KTPlayerManager.ShowNotification(client, "Có lỗi khi nhận nhiệm vụ! ErrorCode : " + TaskID);

                return;
            }

            TaskData taskData = TaskManager.getInstance().GetTaskData(TaskID, client);

            if (null == taskData || taskData.DoingTaskID != TaskID) // NẾu task ko tồn tại trọng người
            {
                KT_TCPHandler.CloseDialog(client);

                KTPlayerManager.ShowNotification(client, "Nhiệm vụ bạn muốn trả không tồn tại : " + TaskID);
                return;
            }

            /// Nếu nhiệm chưa hoàn thành thì thông báo về là task chauw xong
            if (!TaskManager.getInstance().IsQuestComplete(TaskID, taskData.DoingTaskVal1))
            {
                KT_TCPHandler.CloseDialog(client);

                KTPlayerManager.ShowNotification(client, "Nhiệm vụ của bạn vẫn chưa hoàn thành.Hãy hoàn thành rồi quay lại");

                return;
            }

            // Check phát nữa chống bug vật phẩm
            if ((_TaskFind.TargetType == (int)TaskTypes.MonsterSomething || _TaskFind.TargetType == (int)TaskTypes.Collect || _TaskFind.TargetType == (int)TaskTypes.TransferSomething) || (_TaskFind.TaskClass == (int)TaskClasses.NghiaQuan && _TaskFind.TargetType == (int)TaskTypes.Crafting) && "" != _TaskFind.PropsName)
            {
                //Xóa vật phẩm
                int NumberRequest = _TaskFind.TargetNum;
                int GoodName = Int32.Parse(_TaskFind.PropsName);

                int CountItemInBag = ItemManager.GetItemCountInBag(client, GoodName);
                if (CountItemInBag < NumberRequest)
                {
                    KTPlayerManager.ShowNotification(client, "Vật phẩm trong người không đủ để trả nhiệm vụ");

                    return;
                }
            }

            int CountAward = this.CountAward(TaskID);

            // Kiểm tra số ô đồ trống xem có đủ để add thưởng không
            if (!KTGlobal.IsHaveSpace(CountAward, client))
            {
                KT_TCPHandler.CloseDialog(client);

                KTPlayerManager.ShowNotification(client, "Túi đồ của bạn không đủ để nhận thưởng");

                return;
            }

            int isMainTask = ((int)TaskClasses.MainTask == _TaskFind.TaskClass ? 1 : 0);

            string SendToDB = string.Format("{0}:{1}:{2}:{3}:{4}:{5}", client.RoleID, npc.ResID, _TaskFind.ID, taskData.DbID, isMainTask, _TaskFind.TaskClass);

            //LogManager.WriteLog(LogTypes.Quest, "SENDDB :" + SendToDB);

            // Đọc cái j đó từ DB ra
            byte[] sendBytesCmd = new UTF8Encoding().GetBytes(SendToDB);
            byte[] bytesData = null;

            if (TCPProcessCmdResults.RESULT_FAILED == Global.ReadDataFromDb((int)TCPGameServerCmds.CMD_SPR_COMPTASK, sendBytesCmd, sendBytesCmd.Length, out bytesData, client.ServerId))
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Không kết nối được với DBServer, CMD={0}", (int)TCPGameServerCmds.CMD_SPR_COMPTASK));

                KT_TCPHandler.CloseDialog(client);

                KTPlayerManager.ShowNotification(client, "Không kết nối được với DBServer");

                return;
            }

            Int32 length = BitConverter.ToInt32(bytesData, 0);
            string strData = new UTF8Encoding().GetString(bytesData, 6, length - 2);

            //Kết quả từ DB server trả về
            string[] fieldsData = strData.Split(':');

            // Nếu mà kết quả lấy từ DB Về mà toang thì set là toang
            if (fieldsData.Length < 3 || fieldsData[2] == "-1")
            {
                KT_TCPHandler.CloseDialog(client);

                KTPlayerManager.ShowNotification(client, "Có lỗi xảy ra khi trả nhiệm vụ ERROR CODE :" + fieldsData[2]);

                return;
            }
            else
            {
                //Nếu mà ổn thì ta sẽ
                if (ProcessTask.Complete(Global._TCPManager.MySocketListener, pool, client, npc.ResID, npc.ResID, _TaskFind.ID, taskData.DbID, false))
                {
                    // Nếu là main task
                    if (isMainTask > 0 && _TaskFind.ID > client.MainTaskID)
                    {
                        client.MainTaskID = _TaskFind.ID;
                    }

                    KT_TCPHandler.CloseDialog(client);

                    KTPlayerManager.ShowNotification(client, "Trả nhiệm vụ :" + _TaskFind.Title + " thành công!");

                    // Nếu trả thành công ta sẽ gen tiếp xem thằng này có quest nào tiếp theo ko
                    // Trừ đi 1 lượt chạy hải tặc
                    // UPdate thêm số nhiệm vụ đã làm

                    int NumberSet = this.GetNumQuestThisDay(client) + 1;

                    this.SetNumQuestThisDay(client, NumberSet);

                    // Add 1 nhiệm vụ mới cho hải tặc
                    this.GiveTaskPirate(client, true);

                    return;
                }
                else
                {
                    KT_TCPHandler.CloseDialog(client);

                    KTPlayerManager.ShowNotification(client, "Trả nhiệm vụ :" + _TaskFind.Title + " thất bại!");

                    return;
                }
            }
        }

        public int CountAward(int TaskID)
        {
            int Count = 0;

            Task _Task = this.GetTaskTemplate(TaskID);

            if (_Task.Taskaward.Length > 0)
            {
                string[] TotalItem = _Task.Taskaward.Split('#');

                Count = TotalItem.Length;
            }

            return Count;
        }

        public void GetNpcDataQuest(GameMap map, NPC npc, KPlayer client)
        {
            TCPOutPacketPool pool = Global._TCPManager.TcpOutPacketPool;
            TCPClientPool tcpClientPool = Global._TCPManager.tcpClientPool;
            KNPCDialog _NpcDialog = new KNPCDialog();

            Dictionary<int, string> Selections = new Dictionary<int, string>();

            Dictionary<int, string> OtherPram = new Dictionary<int, string>();


            Task _TaskGet = this.GetTaskTemplate(this.GeQuestIDDay(client));


            if (client.TaskDataList != null)
            {
                foreach (TaskData task in client.TaskDataList)
                {
                    Task TaskCheck = _TotalTaskData.Values.Where(x => x.ID == task.DoingTaskID).FirstOrDefault();

                    if (TaskCheck != null)
                    {
                        if (TaskCheck.TaskClass == (int)TaskClasses.HaiTac)
                        {
                            _TaskGet = TaskCheck;
                            break;
                        }
                    }
                }
            }
            // Nếu không có task nào thì giao nhiệm vụ mới
            if (_TaskGet == null)
            {
                GiveTaskPirate(client, true);
            }
            else
            {

                string Hisitory = "Tổng số lượt còn lại " + (6 - this.GetNumQuestThisDay(client)) + " nhiệm vụ<br><br>Uy danh hiện tại : " + client.Prestige;
                string QUA = "";

                if (_TaskGet.Taskaward.Length > 0)
                {
                    string[] Item = _TaskGet.Taskaward.Split(',');
                    ItemData _Temp = ItemManager.GetItemTemplate(Int32.Parse(Item[0]));

                    QUA = KTGlobal.CreateStringByColor(_Temp.Name, ColorType.Done) + "X" + Item[1];
                }

                string PhanThuong = "Phần Thưởng  : Uy Danh: " + KTGlobal.CreateStringByColor(_TaskGet.Point1 + "", ColorType.Done) + "|" + QUA;

                _NpcDialog.Text = Hisitory + "<br><br>" + KTGlobal.CreateStringByColor("Nhiệm Vụ :", ColorType.Accpect) + _TaskGet.Title + "<br><br>" + KTGlobal.CreateStringByColor("Mục Tiêu :", ColorType.Importal) + "<br><br>" + _TaskGet.AcceptTalk + "<br><br>" + PhanThuong;

                if (client.TaskDataList == null)
                {
                    client.TaskDataList = new List<TaskData>();
                }

                var findTaskClient = client.TaskDataList.Where(x => x.DoingTaskID == _TaskGet.ID).FirstOrDefault();

                if (findTaskClient != null)
                {
                    if (TaskManager.getInstance().IsQuestComplete(findTaskClient.DoingTaskID, findTaskClient.DoingTaskVal1))
                    {
                        Selections.Add(2, KTGlobal.CreateStringByColor("Hoàn Thành", ColorType.Done));
                    }
                    else
                    {
                        Selections.Add(3, KTGlobal.CreateStringByColor("Ta muốn hủy nhiệm vụ", ColorType.Importal));
                    }
                }
                else
                {
                    Selections.Add(1, KTGlobal.CreateStringByColor("Tiếp nhận", ColorType.Accpect));
                }

                // Đoạn này là nhét vào thêm để check xem tại sao

                Action<TaskCallBack> ActionWork = (x) => PirateQuestProsecc(map, npc, client, x, _TaskGet.ID);

                Selections.Add(5, KTGlobal.CreateStringByColor("Ta muốn đổi phần thưởng", ColorType.Normal));

                Selections.Add(4, KTGlobal.CreateStringByColor("Thông tin", ColorType.Normal));

                _NpcDialog.OnSelect = ActionWork;

                _NpcDialog.Selections = Selections;

                _NpcDialog.OtherParams = OtherPram;

                _NpcDialog.Show(npc, client);
            }
        }

        public void GetInfoHaiTac(GameMap map, NPC npc, KPlayer client)
        {
            TCPOutPacketPool pool = Global._TCPManager.TcpOutPacketPool;
            TCPClientPool tcpClientPool = Global._TCPManager.tcpClientPool;
            KNPCDialog _NpcDialog = new KNPCDialog();

            Dictionary<int, string> Selections = new Dictionary<int, string>();

            Dictionary<int, string> OtherPram = new Dictionary<int, string>();

            string Hisitory = "Tổng số lượt còn lại " + (6 - this.GetNumQuestThisDay(client)) + " nhiệm vụ<br><br>Uy danh hiện tại : " + client.Prestige;

            string LYDO = "";

            if (client.m_Level < 50)
            {
                LYDO = "Cấp độ không đủ 50";

                Hisitory += "<br><br>" + KTGlobal.CreateStringByColor(LYDO, ColorType.Importal) + " không thể nhận nhiệm vụ";
            }
            if (client.Prestige <= 20)
            {
                LYDO = "Uy danh phải đạt tối thiểu 20 uy danh mới có thể nhận được nhiệm vụ";

                Hisitory += "<br><br>" + KTGlobal.CreateStringByColor(LYDO, ColorType.Importal);
            }
            if (this.GetNumQuestThisDay(client) >= 6)
            {
                LYDO = "Số lượt nhiệm vụ giết Hải Tặc hôm nay đã hết";

                Hisitory += "<br><br>" + KTGlobal.CreateStringByColor(LYDO, ColorType.Importal) + " không thể nhận tiếp nhiệm vụ";
            }

            if (client.m_cPlayerFaction.GetFactionId() <= 0)
            {
                LYDO = "Bạn chưa vào môn phái";

                Hisitory += "<br><br>" + KTGlobal.CreateStringByColor(LYDO, ColorType.Importal) + " không thể nhận tiếp nhiệm vụ";
            }

            _NpcDialog.Text = Hisitory;

            Selections.Add(5, KTGlobal.CreateStringByColor("Ta muốn đổi phần thưởng", ColorType.Normal));

            Selections.Add(4, KTGlobal.CreateStringByColor("Thông tin", ColorType.Normal));

            Action<TaskCallBack> ActionWork = (x) => PirateQuestProsecc(map, npc, client, x, -1);

            _NpcDialog.OnSelect = ActionWork;
            _NpcDialog.Selections = Selections;

            _NpcDialog.OtherParams = OtherPram;

            _NpcDialog.Show(npc, client);
        }

        public bool IsHaveCompleteMainQuest(int NpcID, KPlayer Client)
        {
            List<int> tasksList = null;

            if (!GameManager.NPCTasksMgr.DestNPCTasksDict.TryGetValue(NpcID, out tasksList))
                return false;   //Nếu thằng nPC này không có nhiệm vụ này thì chim cút

            if (0 == tasksList.Count)
                return false;   /// Nếu mà số lượng task trả về ==0 thì cũng chim cút

            // Nếu danh sách nhiệm vụ hiện có mà đéo có task nào thì chứng tỏ thằng này chưa làm nhiệm vụ nào==> Đéo có nhiệm vụ nào có thể hoàn tất
            if (Client.TaskDataList == null)
            {
                return false;
            }

            TCPOutPacketPool pool = Global._TCPManager.TcpOutPacketPool;
            TCPClientPool tcpClientPool = Global._TCPManager.tcpClientPool;

            // Nếu thằng này có nhiệm vụ có thể trả thì cứ check nếu có dạng nhiệm vụ dạng nói chuyện
            ProcessTask.Process(Global._TCPManager.MySocketListener, pool, Client, NpcID, NpcID, -1, TaskTypes.Talk);

            bool IsHaveComplteQuest = false;

            foreach (int TaskSelect in tasksList)
            {
                // Xuống đây check lại 1 lượt nữa phòng khi ở trên đã hoàn thành nhiệm vụ
                if (Client.TaskDataList == null)
                {
                    break;
                }
                if (Client.TaskDataList.Count == 0)
                {
                    break;
                }

                var find = Client.TaskDataList.Where(x => x.DoingTaskID == TaskSelect).FirstOrDefault();
                if (find != null)
                {
                    Task _Find = this.GetTaskTemplate(find.DoingTaskID);
                    if (_Find != null)
                    {
                        // Nếu là nghĩa quân thì luôn cho thằng này ở trạng thái có QUEST
                        if (_Find.TaskClass == (int)TaskClasses.NghiaQuan)
                        {
                            IsHaveComplteQuest = true;
                            break;
                        }
                    }

                    if (TaskManager.getInstance().IsQuestComplete(find.DoingTaskID, find.DoingTaskVal1))
                    {
                        IsHaveComplteQuest = true;
                        break;
                    }
                }
            }

            return IsHaveComplteQuest;
        }

        /// <summary>
        /// Thực hiện hủy bỏ nhiệm vụ
        /// </summary>
        /// <param name="client"></param>
        /// <param name="dbID"></param>
        /// <param name="taskID"></param>
        /// <returns></returns>
        public bool CancelTask(KPlayer client, int dbID, int taskID)
        {
            string cmd2db = StringUtil.substitute("{0}:{1}:{2}", client.RoleID, dbID, taskID);
            string[] dbFields = Global.ExecuteDBCmd((int)TCPGameServerCmds.CMD_SPR_ABANDONTASK, cmd2db, client.ServerId);
            if (null == dbFields || dbFields.Length < 1 || dbFields[0] == "-1")
                return false;

            //Hủy bỏ nhiệm vụ
            if (null != client.TaskDataList)
            {
                ProcessTask.ClearTaskGoods(Global._TCPManager.MySocketListener, Global._TCPManager.TcpOutPacketPool, client, taskID);

                lock (client.TaskDataList)
                {
                    for (int i = 0; i < client.TaskDataList.Count; i++)
                    {
                        if (client.TaskDataList[i].DoingTaskID == taskID)
                        {
                            client.TaskDataList.RemoveAt(i);
                            break;
                        }
                    }
                }
            }

            Task _TaskFind = this.GetTaskTemplate(taskID);

            if (_TaskFind != null)
            {
                int state = 0;
                int sourceNPC = _TaskFind.SourceNPC;
                if (-1 != sourceNPC)
                {
                    state = TaskManager.getInstance().ComputeNPCTaskState(client, client.TaskDataList, sourceNPC);
                    KT_TCPHandler.NotifyUpdateNPCTaskSate(client, (sourceNPC), state);
                }

                int destNPC = _TaskFind.DestNPC;
                if (-1 != destNPC && sourceNPC != destNPC)
                {
                    state = TaskManager.getInstance().ComputeNPCTaskState(client, client.TaskDataList, destNPC);
                    KT_TCPHandler.NotifyUpdateNPCTaskSate(client, (destNPC), state);
                }


            }

            return true;
        }

        /// <summary>
        /// Nhiệm vụ hải tặc sẽ giao cho ngày mới hoặc khi làm xong
        /// </summary>
        /// <param name="client"></param>
        /// <param name="ForceChange"></param>
        /// <returns></returns>
        public bool GiveTaskPirate(KPlayer client, bool ForceChange = false)
        {
            if (!ForceChange)
            {
                if (client.TaskDataList != null)
                {
                    int TaskID = this.GetNumQuestThisDay(client);

                    var find = client.TaskDataList.Where(x => x.DoingTaskID == TaskID).FirstOrDefault();
                    if (find != null)
                    {
                        if (!TaskManager.getInstance().IsQuestComplete(find.DoingTaskID, find.DoingTaskVal1))
                        {
                            return false;
                        }
                    }
                }
            }

            int LevelHientai = client.m_Level;

            List<Task> _TotalTask = _TotalTaskData.Values.Where(x => x.TaskClass == (int)TaskClasses.HaiTac && x.MinLevel <= LevelHientai && x.MaxLevel >= LevelHientai).ToList();

            if (_TotalTask.Count == 0)
            {
                return false;
            }

            int Random = new Random().Next(_TotalTask.Count);

            Task SelectTask = _TotalTask[Random];

            this.SetQuestIDDay(client, SelectTask.ID);

            return true;
        }
    }
}