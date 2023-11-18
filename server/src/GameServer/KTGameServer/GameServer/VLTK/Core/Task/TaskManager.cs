using GameServer.KiemThe.Logic;
using GameServer.Logic;
using GameServer.Server;
using GameServer.VLTK.Core.GuildManager;
using Server.Data;
using Server.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Xml.Linq;
using System.Xml.Serialization;
using static GameServer.Logic.KTMapManager;
using static UnityEngine.GridBrushBase;

namespace GameServer.KiemThe.Core.Task
{
    /// <summary>
    /// Class quản lý nhiệm vụ của cả game
    /// </summary>
    public class TaskManager
    {
        private static TaskManager instance = new TaskManager();

        public Dictionary<int, Task> TotalTask = new Dictionary<int, Task>();

        public Config _TotalConfig = new Config();

        public string TaskConfingFile = "Config/KT_Task/SystemTasks.xml";

        public void Setup()
        {
            LoadItemFromPath(TaskConfingFile);
        }

        /// <summary>
        /// Xử lý các nhiệm vụ đang giang dở hoặc từ bỏ
        /// </summary>
        /// <param name="client"></param>
        public static void ProcessTaskData(KPlayer client)
        {
            if (null == client.TaskDataList)
                return;

            lock (client.TaskDataList)
            {
                for (int i = 0; i < client.TaskDataList.Count; i++)
                {
                    OldTaskData oldTaskData = TaskManager.FindOldTaskByTaskID(client, client.TaskDataList[i].DoingTaskID);
                    if (null != oldTaskData)
                    {
                        client.TaskDataList[i].DoneCount = oldTaskData.DoCount;
                    }
                }
            }
        }

        /// <summary>
        /// Trả về danh sách nhiệm vụ đã làm trước đó
        /// </summary>
        /// <param name="taskID"></param>
        /// <returns></returns>
        public static OldTaskData FindOldTaskByTaskID(KPlayer client, int taskID)
        {
            if (null == client.OldTasks)
            {
                return null;
            }

            lock (client.OldTasks)
            {
                for (int i = 0; i < client.OldTasks.Count; i++)
                {
                    if (taskID == client.OldTasks[i].TaskID)
                    {
                        return client.OldTasks[i];
                    }
                }
            }

            return null;
        }

        public static void AddOldTask(KPlayer client, int taskID)
        {
            if (null == client.OldTasks)
            {
                client.OldTasks = new List<OldTaskData>();
            }

            int findIndex = -1;

            lock (client.OldTasks)
            {
                for (int i = 0; i < client.OldTasks.Count; i++)
                {
                    if (client.OldTasks[i].TaskID == taskID)
                    {
                        findIndex = i;
                        break;
                    }
                }

                if (findIndex >= 0)
                {
                    client.OldTasks[findIndex].DoCount++;
                }
                else
                {
                    client.OldTasks.Add(new OldTaskData()
                    {
                        TaskID = taskID,
                        DoCount = 1,
                    });
                }
            }
        }

        public static TaskData GetTaskData(KPlayer client, int taskID)
        {
            if (null == client.TaskDataList)
                return null;

            lock (client.TaskDataList)
            {
                for (int i = 0; i < client.TaskDataList.Count; i++)
                {
                    if (client.TaskDataList[i].DoingTaskID == taskID)
                    {
                        return client.TaskDataList[i];
                    }
                }
            }

            return null;
        }

        public static int GetFocusTaskCount(KPlayer client)
        {
            int ret = 0;
            if (null == client.TaskDataList)
                return ret;

            lock (client.TaskDataList)
            {
                for (int i = 0; i < client.TaskDataList.Count; i++)
                {
                    if (client.TaskDataList[i].DoingTaskFocus > 0)
                    {
                        ret++;
                    }
                }
            }

            return ret;
        }

        /// <summary>
        /// Gỡ bỏ task không hợp lý
        /// </summary>
        /// <param name="client"></param>
        public static bool RemoveInvalidTask(KPlayer client, TaskData taskData)
        {
            Task _Taskfind = TaskManager.getInstance().FindTaskById(taskData.DoingTaskID);

            if (_Taskfind != null)
            {
                return false;
            }

            int dbID = taskData.DbID;
            int taskID = taskData.DoingTaskID;

            //Xóa task

            Global.ExecuteDBCmd((int) TCPGameServerCmds.CMD_SPR_ABANDONTASK,
                string.Format("{0}:{1}:{2}", client.RoleID, dbID, taskID),
                client.ServerId);

            return true;
        }

        public static bool RemoveAllInvalidTasks(KPlayer client)
        {
            if (null == client.TaskDataList || client.TaskDataList.Count <= 0)
            {
                return false;
            }

            List<TaskData> abandonTaskList = new List<TaskData>();
            int abandonCount = 0;

            lock (client.TaskDataList)
            {
                for (int i = 0; i < client.TaskDataList.Count; i++)
                {
                    if (RemoveInvalidTask(client, client.TaskDataList[i]))
                    {
                        abandonTaskList.Add(client.TaskDataList[i]);
                        abandonCount++;
                    }
                }
            }

            //添加到角色的列表中
            for (int i = 0; i < abandonTaskList.Count; i++)
            {
                lock (client.TaskDataList)
                {
                    client.TaskDataList.Remove(abandonTaskList[i]);
                }

                LogManager.WriteLog(LogTypes.Error, string.Format("删除无效的任务, Client={0}({1}), TaskID={2}",
                    client.RoleID, client.RoleName, abandonTaskList[i].DoingTaskID));
            }

            return (abandonCount > 0);
        }

        /// <summary>
        /// Find Ra Task Cần tìm
        /// </summary>
        /// <param name="TaskID"></param>
        /// <returns></returns>
        public Task FindTaskById(int TaskID)
        {
            TotalTask.TryGetValue(TaskID, out Task _OutTask);
            if (_OutTask != null)
            {
                return _OutTask;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Tính toán trạng thái nhiệm vụ
        /// </summary>
        /// <param name="npcID"></param>
        public int ComputeNPCTaskState(KPlayer client, List<TaskData> TaskDataList, int npcID)
        {
            int ret = (int)NPCTaskStates.None;

            List<int> taskIDList = null;
            if (GameManager.NPCTasksMgr.SourceNPCTasksDict.TryGetValue(npcID, out taskIDList))
            {
                for (int i = 0; i < taskIDList.Count; i++)
                {
                    Task _FindTask = this.FindTaskById(taskIDList[i]);

                    if (_FindTask == null)
                    {
                        continue;
                    }

                    int taskClass = _FindTask.TaskClass;

                    // Nếu là nhiệm vụ bạo văn đồng kieiemr tra xem có nhận được nữa không
                    if (taskClass == (int)TaskClasses.NghiaQuan)
                    {
                        if (!TaskDailyArmyManager.getInstance().CanTakeNewTask(client, taskIDList[i]))
                        {
                            continue;
                        }
                    }
                    //Nếu là nhiệm vụ hải tặc thì kiểm tra điều kiện xem có nhận được không
                    else if (taskClass == (int)TaskClasses.HaiTac)
                    {
                        if (!PirateTaskManager.getInstance().CanTakeNewTask(client, taskIDList[i]))
                        {
                            continue;
                        }
                    }
                    else if (_FindTask.TaskClass == (int)TaskClasses.MainTask)
                    {
                        //Xác định xem đó có phải là một nhiệm vụ mới có thể được chấp nhận hay không
                        if (!MainTaskManager.getInstance().CanTakeNewTask(client, taskIDList[i]))
                        {
                            continue;
                        }
                    }
                    else if (_FindTask.TaskClass == (int)TaskClasses.ThuongHoi)
                    {
                        //Xác định xem đó có phải là một nhiệm vụ mới có thể được chấp nhận hay không
                        if (!FirmTaskManager.getInstance().CanTakeNewTask(client, taskIDList[i]))
                        {
                            continue;
                        }
                    }
                    //TODO ADD THÊM QUEST THƯƠNG HỘI Ở ĐÂY

                    if (_FindTask.TaskClass == (int)TaskClasses.MainTask)
                    {
                        ret = (int)NPCTaskStates.ToReceive_MainQuest;
                    }
                    else if (_FindTask.TaskClass == (int)TaskClasses.NghiaQuan || _FindTask.TaskClass == (int)TaskClasses.HaiTac || _FindTask.TaskClass == (int)TaskClasses.ThuongHoi)
                    {
                        ret = (int)NPCTaskStates.ToReceive_DailyQuest;
                    }
                    else if (_FindTask.TaskClass == (int)TaskClasses.TheGioi)
                    {
                        ret = (int)NPCTaskStates.ToReceive_SubQuest;
                    }

                    break;
                }
            }

            if (null == TaskDataList) return ret;
            lock (TaskDataList)
            {
                for (int i = 0; i < TaskDataList.Count; i++)
                {
                    Task _FindTask = this.FindTaskById(TaskDataList[i].DoingTaskID);

                    if (_FindTask == null)
                    {
                        continue;
                    }

                    int destNPC = _FindTask.DestNPC;

                    if (destNPC == npcID)
                    {
                        // nếu mà nhiệm vụ đã hoàn thành thì update là nhiệm vụ có thể trả
                        if (this.IsQuestComplete(TaskDataList[i].DoingTaskID, TaskDataList[i].DoingTaskVal1))
                        {
                            if (_FindTask.TaskClass == (int)TaskClasses.MainTask)
                            {
                                ret = (int)NPCTaskStates.ToReturn_MainQuest;
                            }
                            else if (_FindTask.TaskClass == (int)TaskClasses.NghiaQuan || _FindTask.TaskClass == (int)TaskClasses.HaiTac || _FindTask.TaskClass == (int)TaskClasses.ThuongHoi)
                            {
                                ret = (int)NPCTaskStates.ToReturn_DailyQuest;
                            }
                            else if (_FindTask.TaskClass == (int)TaskClasses.TheGioi)
                            {
                                ret = (int)NPCTaskStates.ToReturn_SubQuest;
                            }
                        }
                        else
                        {
                            // Update đang làm
                            if (_FindTask.TaskClass == (int)TaskClasses.MainTask)
                            {
                                ret = (int)NPCTaskStates.ToReturn_MainQuest;
                            }
                            else if (_FindTask.TaskClass == (int)TaskClasses.NghiaQuan || _FindTask.TaskClass == (int)TaskClasses.HaiTac || _FindTask.TaskClass == (int)TaskClasses.ThuongHoi)
                            {
                                ret = (int)NPCTaskStates.ToReturn_DailyQuest;
                            }
                            else if (_FindTask.TaskClass == (int)TaskClasses.TheGioi)
                            {
                                ret = (int)NPCTaskStates.ToReturn_SubQuest;
                            }
                        }
                    }
                }
            }

            return ret;
        }

        /// <summary>
        /// Fix Main TaskID
        /// </summary>
        /// <param name="client"></param>
        public void FixMainTaskID(KPlayer client)
        {

         
            if (client.TaskDataList == null)
            {
                return;
            }

            List<TaskData> RemoveTask = new List<TaskData>();

            lock (client.TaskDataList)
            {
                foreach (TaskData Task in client.TaskDataList)
                {
                    Task _TaskGet = this.FindTaskById(Task.DoingTaskID);
                    if (_TaskGet != null)
                    {
                        if (_TaskGet.TargetType == (int)TaskTypes.TransferSomething)
                        {
                            ProcessTask.Process(Global._TCPManager.MySocketListener, Global._TCPManager.TcpOutPacketPool, client, -1, -1, -1, TaskTypes.TransferSomething);
                        }

                        if (_TaskGet.TargetType == (int)TaskTypes.Crafting)
                        {
                            ProcessTask.Process(Global._TCPManager.MySocketListener, Global._TCPManager.TcpOutPacketPool, client, -1, -1, -1, TaskTypes.Crafting);
                        }

                        if (_TaskGet.TargetType == (int)TaskTypes.JoinFaction)
                        {
                            ProcessTask.Process(Global._TCPManager.MySocketListener, Global._TCPManager.TcpOutPacketPool, client, -1, -1, -1, TaskTypes.JoinFaction);
                        }
                    }
                    else
                    {
                        // Nếu mà nhiệm vụ ko còn trong tempalte thì xóa khỏi nhân vật
                        RemoveTask.Add(Task);
                    }
                }

                for (int i = 0; i < RemoveTask.Count; i++)
                {
                    TaskData _Task = RemoveTask[i];

                    this.CancelTask(client, _Task.DbID, _Task.DoingTaskID);
                }
            }

            ProcessTask.ProseccTaskBeforeDoTask(Global._TCPManager.MySocketListener, Global._TCPManager.TcpOutPacketPool, client);
        }

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

            Task _TaskFind = this.FindTaskById(taskID);

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
        public bool IsQuestComplete(int TaskID, int TaskValue)
        {
            bool IsComplete = false;

            Task _Task = this.FindTaskById(TaskID);

            if (_Task.TargetType == (int)TaskTypes.Talk || _Task.TargetType == (int)TaskTypes.GetItemWithSpcecialLine || _Task.TargetType == (int)TaskTypes.AnswerQuest || _Task.TargetType == (int)TaskTypes.JoinFaction)
            {
                if (TaskValue >= 1)
                {
                    IsComplete = true;
                }
            }

            if (_Task.TargetType == (int)TaskTypes.KillMonster || _Task.TargetType == (int)TaskTypes.MonsterSomething || _Task.TargetType == (int)TaskTypes.Crafting || _Task.TargetType == (int)TaskTypes.Enhance || _Task.TargetType == (int)TaskTypes.BuySomething || _Task.TargetType == (int)TaskTypes.UseSomething || _Task.TargetType == (int)TaskTypes.TransferSomething || _Task.TargetType == (int)TaskTypes.GetSomething || _Task.TargetType == (int)TaskTypes.Collect)
            {
                int NumberRequest = _Task.TargetNum;

                if (TaskValue >= NumberRequest)
                {
                    IsComplete = true;
                }
            }

            return IsComplete;
        }
        /// <summary>
        /// Trả về danh sách trạng thái nhiệm vụ
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public List<NPCTaskState> GetNPCTaskState(KPlayer player)
        {
            GameMap gameMap = KTMapManager.Find(player.MapCode);

            /// Tạo mới danh sách
            List<NPCTaskState> npcTaskStateList = new List<NPCTaskState>();
            /// Duyệt danh sách NPC trong hệ thống
            foreach (NPC npc in KTNPCManager.GetMapNPCs(player.MapCode))
            {
                npcTaskStateList.Add(new NPCTaskState()
                {
                    NPCID = npc.ResID,
                    TaskState = 0,
                });
            }

            for (int i = 0; i < npcTaskStateList.Count; i++)
            {
                npcTaskStateList[i].TaskState = this.ComputeNPCTaskState(player, player.TaskDataList, npcTaskStateList[i].NPCID);
            }

            return npcTaskStateList;
        }

        /// <summary>
        /// Tính toán trạng thái nhiệm vụ
        /// </summary>
        /// <param name="roleData"></param>
        public void ComputeNPCTaskState(KPlayer client)
        {
            List<NPCTaskState> npcTaskStateList = this.GetNPCTaskState(client);
            /// Toác
            if (npcTaskStateList == null)
            {
                return;
            }
            // Gửi về danh sách STATE về CLIENT
            KT_TCPHandler.NotifyNPCTaskStateList(client, npcTaskStateList);
        }

        public void LoadItemFromPath(string FilesPath)
        {
            string Files = KTGlobal.GetDataPath(FilesPath);
            using (var stream = System.IO.File.OpenRead(Files))
            {
                var serializer = new XmlSerializer(typeof(Config));

                _TotalConfig = serializer.Deserialize(stream) as Config;

                //Loading toàn bộ trạng thái quest cho NPC
                NPCTasksManager.LoadNPCTasks(_TotalConfig);

                Dictionary<int, Task> TotalTask = _TotalConfig.Tasks.Task.ToDictionary(x => x.ID, x => x);
                //Set danh sách nhiệm vụ chính tuyến cho class xử lý chuyên biệt
                this.TotalTask = TotalTask;
                //Loading all MainTask

                Dictionary<int, Task> MainTaskSelect = _TotalConfig.Tasks.Task.Where(x => x.TaskClass == (int)TaskClasses.MainTask).ToDictionary(x => x.ID, x => x);
                //Set danh sách nhiệm vụ chính tuyến cho class xử lý chuyên biệt
                MainTaskManager.getInstance().SetTask(MainTaskSelect);

                //Loading all hải tặc
                Dictionary<int, Task> PirateTaskSelect = _TotalConfig.Tasks.Task.Where(x => x.TaskClass == (int)TaskClasses.HaiTac).ToDictionary(x => x.ID, x => x);

                PirateTaskManager.getInstance().SetTask(PirateTaskSelect);

                //Loading all DayliTask
                Dictionary<int, Task> TaskDailySelect = _TotalConfig.Tasks.Task.Where(x => x.TaskClass == (int)TaskClasses.NghiaQuan).ToDictionary(x => x.ID, x => x);

                TaskDailyArmyManager.getInstance().SetTask(TaskDailySelect);


                Dictionary<int, Task> FrimTaskSelect = _TotalConfig.Tasks.Task.Where(x => x.TaskClass == (int)TaskClasses.ThuongHoi).ToDictionary(x => x.ID, x => x);

                FirmTaskManager.getInstance().SetTask(FrimTaskSelect);


                //Loading all Task nhiệm vụ bang
                Dictionary<int, Task> GuildTask = _TotalConfig.Tasks.Task.Where(x => x.TaskClass == (int)TaskClasses.BangHoi).ToDictionary(x => x.ID, x => x);


                GuildManager.getInstance().SetTask(GuildTask);
            }
        }

        /// <summary>
        /// Lấy ra task hiện tại của nhân vật
        /// </summary>
        /// <param name="TaskID"></param>
        /// <param name="client"></param>
        /// <returns></returns>
        public TaskData GetTaskData(int TaskID, KPlayer client)
        {
            TaskData _TaskData = null;

            if (client.TaskDataList == null)
            {
                return null;
            }

            _TaskData = client.TaskDataList.Where(x => x.DoingTaskID == TaskID).FirstOrDefault();

            return _TaskData;
        }

        public static TaskManager getInstance()
        {
            return instance;
        }
    }
}