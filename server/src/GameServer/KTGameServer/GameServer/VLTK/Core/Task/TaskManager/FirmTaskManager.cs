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
    public class FirmTaskManager
    {
        private static FirmTaskManager instance = new FirmTaskManager();

        public Dictionary<int, Task> _TotalTaskData = new Dictionary<int, Task>();

        /// <summary>
        /// Số bạc đã đổi trong tuần này
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        public int GetMoneyChangeThisWeek(KPlayer client)
        {
            int Count = client.GetValueOfWeekRecore((int)WeekRecord.FirmMoneyExchange);

            if (Count == -1)
            {
                Count = 0;
            }

            return Count;
        }

        /// <summary>
        /// Ghi lại số bạc đã đổi trong tuần
        /// </summary>
        /// <param name="client"></param>
        /// <param name="Value"></param>
        public void SetMoneyExchangeThisWeek(KPlayer client, int Value)
        {
            client.SetValueOfWeekRecore((int)WeekRecord.FirmMoneyExchange, Value);
        }

        /// <summary>
        /// Lấy ra số nhiệm vụ trong tuần này
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        public int GetNumQuestThisWeek(KPlayer client)
        {
            int Count = client.GetValueOfWeekRecore((int)WeekRecord.FirmTaskCurNum);

            if (Count == -1)
            {
                Count = 1;
            }

            return Count;
        }

        public void SetNumQuestThisWeek(KPlayer client, int Value)
        {
            client.SetValueOfWeekRecore((int)WeekRecord.FirmTaskCurNum, Value);
        }

        public int GetQuestID(KPlayer client)
        {
            int Count = client.GetValueOfWeekRecore((int)WeekRecord.FirmTaskCurID);

            return Count;
        }

        /// <summary>
        /// Set số nhiệm vụ update
        /// </summary>
        /// <param name="client"></param>
        /// <param name="Value"></param>
        public void SetQuestID(KPlayer client, int Value)
        {
            client.SetValueOfWeekRecore((int)WeekRecord.FirmTaskCurID, Value);
        }

        /// <summary>
        /// Contruction
        /// </summary>
        /// <returns></returns>
        public static FirmTaskManager getInstance()
        {
            return instance;
        }

        /// <summary>
        /// Set vào danh sách nhiệm vụ
        /// </summary>
        /// <param name="_InPut"></param>
        public void SetTask(Dictionary<int, Task> _InPut)
        {
            this._TotalTaskData = _InPut;
        }

        /// <summary>
        /// Lấy ra ID nhiệm vụ
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Kiểm tra xem có quest hay không
        /// </summary>
        /// <param name="NpcID"></param>
        /// <param name="client"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Có nhiệm vụ nào sắp hoàn thành không
        /// </summary>
        /// <param name="NpcID"></param>
        /// <param name="Client"></param>
        /// <returns></returns>
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
            if (client.Prestige < 50)
            {
                return false;
            }

            // TODO THÊM CHECK UY DANH
            if (client.m_Level < 60)
            {
                return false;
            }

            if (client.m_cPlayerFaction.GetFactionId() == 0)
            {
                return false;
            }

            // Nếu hết lượt làm
            if (this.GetNumQuestThisWeek(client) > 40)
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

        /// <summary>
        /// Hàm xử lý nhiệm vụ thương hội
        /// </summary>
        /// <param name="map"></param>
        /// <param name="npc"></param>
        /// <param name="client"></param>
        /// <param name="TaskData"></param>
        /// <param name="TaskID"></param>
        public void FirmTaskProsescc(GameMap map, NPC npc, KPlayer client, TaskCallBack TaskData, int TaskID)
        {
            TCPOutPacketPool pool = Global._TCPManager.TcpOutPacketPool;

            if (TaskData != null)
            {
                int SelectID = TaskData.SelectID;

                // if (client.TaskDataList != null)
                {
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



                                this.GiveTaskPrim(client, true);
                            }
                        }
                    }

                    if (SelectID == 2) // Nếu là hoàn thành nhiệm vụ
                    {
                        this.CompleteTask(map, npc, client, TaskID);
                    }
                    else if (SelectID == 1) // Nếu là nhận nhiệm vụ
                    {
                        this.AppcepTask(npc, client, TaskID);
                    }
                    else if (SelectID == 4) // Nếu là nhận nhiệm vụ
                    {
                        this.GetInfoThuongHoi(map, npc, client);
                    }
                    else if (SelectID == 5) // Nếu là nhận nhiệm vụ
                    {
                        this.GetExchangeDialog(map, npc, client);
                    }
                    else if (SelectID == 100) // Nếu là nhận nhiệm vụ
                    {
                        DoExChange(client, 10000, npc, 1);
                    }
                    else if (SelectID == 200) // Nếu là nhận nhiệm vụ
                    {
                        DoExChange(client, 20000, npc, 2);
                    }
                    else if (SelectID == 300) // Nếu là nhận nhiệm vụ
                    {
                        DoExChange(client, 30000, npc, 3);
                    }
                    else if (SelectID == 400) // Nếu là nhận nhiệm vụ
                    {
                        DoExChange(client, 40000, npc, 4);
                    }
                    else if (SelectID == 500) // Nếu là nhận nhiệm vụ
                    {
                        DoExChange(client, 50000, npc, 5);
                    }
                }
            }
        }

        /// <summary>
        /// Thưnc hiện đổi bạc khóa
        /// </summary>
        /// <param name="client"></param>
        /// <param name="Number"></param>
        /// <param name="npc"></param>
        /// <param name="NumberReduct"></param>
        public void DoExChange(KPlayer client, int Number, NPC npc, int NumberReduct)
        {
            if (client.Prestige < NumberReduct)
            {
                KTLuaLib_Player.SendMSG("Uy danh không đủ", client, npc);
            }
            else
            {
                int MoneyAreadyChange = this.GetMoneyChangeThisWeek(client);
                if (MoneyAreadyChange + Number > 120000)
                {
                    KTLuaLib_Player.SendMSG("Số tiền vượt quá 120.000 Bạc", client, npc);
                }
                else
                {
                    if (KTGlobal.IsHaveMoney(client, Number, Entities.MoneyType.BacKhoa))
                    {
                        client.Prestige = client.Prestige - NumberReduct;
                        this.SetMoneyExchangeThisWeek(client, MoneyAreadyChange + Number);

                        KTGlobal.SubMoney(client, Number, Entities.MoneyType.BacKhoa, "EXCHHANGE|THUONGHOI|" + Number);

                        KTGlobal.AddMoney(client, Number, Entities.MoneyType.Bac, "EXCHHANGE|THUONGHOI|" + Number);

                        KT_TCPHandler.CloseDialog(client);
                    }
                    else
                    {
                        KTLuaLib_Player.SendMSG("Số lượng bạc khóa không đủ", client, npc);
                    }
                }
            }
        }

        /// <summary>
        /// Nhiệm vụ có hoàn thành hay không
        /// </summary>
        /// <param name="map"></param>
        /// <param name="npc"></param>
        /// <param name="client"></param>
        /// <param name="TaskID"></param>
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

            // Kiểm tra số ô đồ trống xem có đủ để add thưởng không
            if (!KTGlobal.IsHaveSpace(1, client))
            {
                KT_TCPHandler.CloseDialog(client);

                KTPlayerManager.ShowNotification(client, "Túi đồ của bạn không đủ để nhận thưởng\nÍt nhất 2 ô trống");

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

                    int Number = this.GetNumQuestThisWeek(client);

                    this.SetNumQuestThisWeek(client, Number + 1);

                    // Add 1 nhiệm vụ mới cho hải tặc
                    this.GiveTaskPrim(client, true);

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

        /// <summary>
        /// Get ra NPC quest
        /// </summary>
        /// <param name="map"></param>
        /// <param name="npc"></param>
        /// <param name="client"></param>
        public void GetNpcDataQuest(GameMap map, NPC npc, KPlayer client)
        {
            TCPOutPacketPool pool = Global._TCPManager.TcpOutPacketPool;
            TCPClientPool tcpClientPool = Global._TCPManager.tcpClientPool;
            KNPCDialog _NpcDialog = new KNPCDialog();

            Dictionary<int, string> Selections = new Dictionary<int, string>();

            Dictionary<int, string> OtherPram = new Dictionary<int, string>();

            Task _TaskGet = this.GetTaskTemplate(GetQuestID(client));

            // CHÈN ĐOẠN NÀY FIX NHIỆM VỤ THƯƠNG HỘI GIỐNG BÊN HẢI TẶC
            if (client.TaskDataList != null)
            {
                foreach (TaskData task in client.TaskDataList)
                {
                    Task TaskCheck = _TotalTaskData.Values.Where(x => x.ID == task.DoingTaskID).FirstOrDefault();

                    if (TaskCheck != null)
                    {
                        if (TaskCheck.TaskClass == (int)TaskClasses.ThuongHoi)
                        {
                            _TaskGet = TaskCheck;
                            break;
                        }
                    }
                }
            }


            if (_TaskGet == null)
            {
                this.GiveTaskPrim(client, true);
                _TaskGet = this.GetTaskTemplate(GetQuestID(client));
            }

            string History = KTGlobal.CreateStringByColor("Tuần này bạn đã hoàn thành :" + this.GetNumQuestThisWeek(client) + "/" + 40, ColorType.Pure) + "\n";

            string PhanThuong = "Phần Thưởng  : " + KTGlobal.CreateStringByColor("100.0000 Exp", ColorType.Green) + " cho mỗi nhiệm vụ hoàn thành\n";

            PhanThuong += "Các mốc 10,20,30 sẽ nhận thêm " + KTGlobal.CreateStringByColor("2.000.000 Exp", ColorType.Green) + " | " + KTGlobal.CreateStringByColor("5 Vạn bạc khóa", ColorType.Green) + " | " + KTGlobal.CreateStringByColor("5 Vạn bạc thường", ColorType.Green) + " | " + KTGlobal.CreateStringByColor("2 Huyền Tinh Cấp 6", ColorType.Green) + "";

            PhanThuong += "\nRiêng mốc 40 sẽ nhận thêm " + KTGlobal.CreateStringByColor("10.000.000 Exp", ColorType.Green) + " | " + KTGlobal.CreateStringByColor("20 Vạn bạc khóa", ColorType.Green) + " | " + KTGlobal.CreateStringByColor("20 Vạn bạc thưởng", ColorType.Green) + " | " + KTGlobal.CreateStringByColor("40 Uy danh", ColorType.Green) + "|" + KTGlobal.CreateStringByColor("2 Huyền Tinh Cấp 7", ColorType.Green);

            _NpcDialog.Text = History + "\n" + KTGlobal.CreateStringByColor("Nhiệm Vụ :", ColorType.Accpect) + _TaskGet.Title + "<br><br>" + KTGlobal.CreateStringByColor("Mục Tiêu :", ColorType.Importal) + "<br><br>" + _TaskGet.AcceptTalk + "<br><br>" + PhanThuong;

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
            }
            else
            {
                Selections.Add(1, KTGlobal.CreateStringByColor("Tiếp nhận", ColorType.Accpect));
            }

            Action<TaskCallBack> ActionWork = (x) => FirmTaskProsescc(map, npc, client, x, _TaskGet.ID);

            Selections.Add(5, KTGlobal.CreateStringByColor("Đổi bạc khóa ra bạc thường", ColorType.Normal));

            Selections.Add(4, KTGlobal.CreateStringByColor("Thông tin", ColorType.Normal));

            _NpcDialog.OnSelect = ActionWork;

            _NpcDialog.Selections = Selections;

            _NpcDialog.OtherParams = OtherPram;

            _NpcDialog.Show(npc, client);
        }

        /// <summary>
        /// Dialog đổi bạc và quy đổi UY danh
        /// </summary>
        /// <param name="map"></param>
        /// <param name="npc"></param>
        /// <param name="client"></param>
        public void GetExchangeDialog(GameMap map, NPC npc, KPlayer client)
        {
            TCPOutPacketPool pool = Global._TCPManager.TcpOutPacketPool;
            TCPClientPool tcpClientPool = Global._TCPManager.tcpClientPool;
            KNPCDialog _NpcDialog = new KNPCDialog();

            Dictionary<int, string> Selections = new Dictionary<int, string>();

            Dictionary<int, string> OtherPram = new Dictionary<int, string>();

            string History = KTGlobal.CreateStringByColor("Tuần này bạn đã đổi :" + this.GetMoneyChangeThisWeek(client) + "/ 120.000 Bạc", ColorType.Pure) + "\n";

            Selections.Add(100, KTGlobal.CreateStringByColor("Đổi 10.000 Bạc (Cần 1 Uy Danh)", ColorType.Accpect));
            Selections.Add(200, KTGlobal.CreateStringByColor("Đổi 20.000 Bạc (Cần 2 Uy Danh)", ColorType.Accpect));
            Selections.Add(300, KTGlobal.CreateStringByColor("Đổi 30.000 Bạc (Cần 3 Uy Danh)", ColorType.Accpect));
            Selections.Add(400, KTGlobal.CreateStringByColor("Đổi 40.000 Bạc (Cần 4 Uy Danh)", ColorType.Accpect));
            Selections.Add(500, KTGlobal.CreateStringByColor("Đổi 50.000 Bạc (Cần 5 Uy Danh)", ColorType.Accpect));

            Action<TaskCallBack> ActionWork = (x) => FirmTaskProsescc(map, npc, client, x, -1);

            Selections.Add(4, KTGlobal.CreateStringByColor("Thông tin", ColorType.Normal));

            _NpcDialog.Text = History;

            _NpcDialog.OnSelect = ActionWork;

            _NpcDialog.Selections = Selections;

            _NpcDialog.OtherParams = OtherPram;

            _NpcDialog.Show(npc, client);
        }

        /// <summary>
        /// Get info thương hội
        /// </summary>
        /// <param name="map"></param>
        /// <param name="npc"></param>
        /// <param name="client"></param>
        public void GetInfoThuongHoi(GameMap map, NPC npc, KPlayer client)
        {
            TCPOutPacketPool pool = Global._TCPManager.TcpOutPacketPool;
            TCPClientPool tcpClientPool = Global._TCPManager.tcpClientPool;
            KNPCDialog _NpcDialog = new KNPCDialog();

            Dictionary<int, string> Selections = new Dictionary<int, string>();

            Dictionary<int, string> OtherPram = new Dictionary<int, string>();

            string Hisitory = "Chủ thương hội : Giúp Thương Hội hoàn thành 40 nhiệm vụ sẽ nhận được rất nhiều bạc vầ huyền tinh\nTham gia nhiệm vụ Thương Hội phải thỏa mãn những điều kiện sau :\n\n";

            if (client.m_Level < 60)
            {
                Hisitory += KTGlobal.CreateStringByColor("1.Đạt cấp 60", ColorType.Importal) + "\n";
            }
            else
            {
                Hisitory += KTGlobal.CreateStringByColor("1.Đạt cấp 60", ColorType.Yellow) + "\n";
            }

            if (client.Prestige < 50)
            {
                Hisitory += KTGlobal.CreateStringByColor("2.Uy danh giang hồ đạt 50 điểm", ColorType.Importal) + "\n";
            }
            else
            {
                Hisitory += KTGlobal.CreateStringByColor("2.Uy danh giang hồ đạt 50 điểm", ColorType.Yellow) + "\n";
            }

            Hisitory += KTGlobal.CreateStringByColor("3.Hoàn thành nhiệm vụ chính tuyến cấp 50", ColorType.Yellow) + "\n";

            if (this.GetNumQuestThisWeek(client) > 40)
            {
                Hisitory += KTGlobal.CreateStringByColor("4.Mỗi tuần chỉ được hoàn thành tối đa 40 nhiệm vụ", ColorType.Importal) + "\n";
            }
            else
            {
                Hisitory += KTGlobal.CreateStringByColor("4.Mỗi tuần chỉ được hoàn thành tối đa 40 nhiệm vụ", ColorType.Yellow) + "\n";
            }

            string PhanThuong = "Phần Thưởng  : " + KTGlobal.CreateStringByColor("100.0000 Exp", ColorType.Green) + " cho mỗi nhiệm vụ hoàn thành\n";

            PhanThuong += "Các mốc 10,20,30 sẽ nhận thêm " + KTGlobal.CreateStringByColor("2.000.000 Exp", ColorType.Green) + " | " + KTGlobal.CreateStringByColor("5 Vạn bạc khóa", ColorType.Green) + " | " + KTGlobal.CreateStringByColor("5 Vạn bạc thường", ColorType.Green) + " | " + KTGlobal.CreateStringByColor("2 Huyền Tinh Cấp 6", ColorType.Green) + "";

            PhanThuong += "\nRiêng mốc 40 sẽ nhận thêm " + KTGlobal.CreateStringByColor("10.000.000 Exp", ColorType.Green) + " | " + KTGlobal.CreateStringByColor("20 Vạn bạc khóa", ColorType.Green) + " | " + KTGlobal.CreateStringByColor("20 Vạn bạc thưởng", ColorType.Green) + " | " + KTGlobal.CreateStringByColor("40 Uy danh", ColorType.Green) + "|" + KTGlobal.CreateStringByColor("2 Huyền Tinh Cấp 7", ColorType.Green);

            _NpcDialog.Text = Hisitory + "\n\n" + PhanThuong;

            Action<TaskCallBack> ActionWork = (x) => FirmTaskProsescc(map, npc, client, x, -1);

            Selections.Add(5, KTGlobal.CreateStringByColor("Đổi bạc khóa ra bạc thường", ColorType.Normal));

            _NpcDialog.OnSelect = ActionWork;

            _NpcDialog.Selections = Selections;

            _NpcDialog.OtherParams = OtherPram;

            _NpcDialog.Show(npc, client);
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
        public bool GiveTaskPrim(KPlayer client, bool ForceChange = false)
        {
            if (!ForceChange)
            {
                if (client.TaskDataList != null)
                {
                    int TaskID = this.GetQuestID(client);

                    if (TaskID != -1)
                    {
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
            }

            int LevelHientai = client.m_Level;

            List<Task> _TotalTask = _TotalTaskData.Values.Where(x => x.TaskClass == (int)TaskClasses.ThuongHoi && x.MinLevel <= LevelHientai && x.MaxLevel >= LevelHientai).ToList();

            if (_TotalTask.Count == 0)
            {
                return false;
            }

            int Random = new Random().Next(_TotalTask.Count);

            Task SelectTask = _TotalTask[Random];

            this.SetQuestID(client, SelectTask.ID);

            return true;
        }
    }
}