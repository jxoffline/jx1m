using GameServer.KiemThe.Core.Item;
using GameServer.KiemThe.Logic;
using GameServer.KiemThe.LuaSystem;
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
    /// Classs quản lý nhiệm vụ chính tuyến
    /// </summary>
    public class MainTaskManager
    {
        private static MainTaskManager instance = new MainTaskManager();

        public Dictionary<int, Task> _TotalTaskData = new Dictionary<int, Task>();

        // Danh sách nhiệm vụ chính tuyến
        public List<int> MainTaskList = new List<int>();

        public static MainTaskManager getInstance()
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
        /// Kiểm tra xem có nhiệm vụ chính tuyến có thể nhận không
        /// </summary>
        /// <param name="NpcID"></param>
        /// <param name="client"></param>
        /// <returns></returns>
        public bool IsHaveMainQuest(int NpcID, KPlayer client)
        {
            List<int> tasksList = null;

            if (!GameManager.NPCTasksMgr.SourceNPCTasksDict.TryGetValue(NpcID, out tasksList))
                return false;

            if (tasksList == null)
            {
                return false;
            }

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

                //Lấy ra task
                Task _task = this.GetTaskTemplate(TaskID);

                if (_task != null)
                {
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
            }

            return IsHaveQuest;
        }

        /// <summary>
        /// Có thể nhận nhiệm vụ mới không
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

            if (systemTask.TaskClass == (int)TaskClasses.MainTask || systemTask.TaskClass == (int)TaskClasses.TheGioi)
            {
                if (null != TaskManager.getInstance().GetTaskData(taskID, client))
                {
                    return false;
                }
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

            int nLev = client.m_Level;
            // Số lần chuyển sinh

            // Lấy ra MIN LEVEL
            int minLevel = systemTask.MinLevel;

            // Lấy ra MAX LEVEL
            int maxLevel = systemTask.MaxLevel;

            // Nếu không đủ cấp độ
            if (nLev < minLevel || nLev > maxLevel)
            {
                return false;
            }

            // Nếu là nhiệm vụ chính tuyến hoặc nhiệm vụ phụ tuyến thì nhiệm vụ chỉ được làm 1 lần
            if ((int)TaskClasses.MainTask == taskClass || (int)TaskClasses.TheGioi == taskClass)
            {
                if (client.OldTasks != null)
                {
                    // Check xem nhiệm vụ này đã làm trong quá khứ chưa | NẾu đã làm rồi thì thì chim cút
                    if (client.OldTasks.Count > 0)
                    {
                        var find = client.OldTasks.Where(x => x.TaskID == taskID).FirstOrDefault();

                        if (find != null)
                        {
                            return false;
                        }
                    }
                }
            }

            // Nếu chưa làm trong quá khứ thì check xem nhiệm vụ trước của nó đã làm chưa
            int prevTask = systemTask.PrevTask;
            if (-1 != prevTask)
            {
                if (client.OldTasks != null)
                {
                    var find = client.OldTasks.Where(x => x.TaskID == prevTask).FirstOrDefault();

                    if (find == null)
                    {
                        return false;
                    }
                }
                else
                {
                    // Nếu list là rỗng thì chứng tỏ thằng này chưa hề làm 1 nhiệm vụ nào
                    return false;
                }
            }

            return true; //Return true
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
        /// Trả lời câu hỏi Maintassk
        /// </summary>
        /// <param name="map"></param>
        /// <param name="npc"></param>
        /// <param name="client"></param>
        /// <param name="TaskData"></param>
        /// <param name="TaskID"></param>
        public void AswerQuestion(GameMap map, NPC npc, KPlayer client, TaskCallBack TaskData, int TaskID)
        {
            TCPOutPacketPool pool = Global._TCPManager.TcpOutPacketPool;
            TCPClientPool tcpClientPool = Global._TCPManager.tcpClientPool;
            KNPCDialog _NpcDialog = new KNPCDialog();

            Task _TaskGet = this.GetTaskTemplate(TaskID);

            if (_TaskGet != null)
            {
                int ReadSelect = TaskData.SelectID;

                if (ReadSelect == 100)
                {
                    KT_TCPHandler.CloseDialog(client);
                }
                else
                {
                    ReadSelect = ReadSelect - 1;
                    string[] TaskPram = _TaskGet.QuestionTable.Split('|');

                    if (TaskPram.Length > 0)
                    {
                        string[] TotalAnswer = TaskPram[1].Split('\n');

                        
                        if (ReadSelect >= TotalAnswer.Length)
                        {
                            _NpcDialog.Text = string.Format("Thông tin nhiệm vụ bị lỗi, tên nhiệm vụ <color=green>{0}</color>. Hãy thử lại và liên hệ với hỗ trợ nếu vẫn không được.", _TaskGet.Title);
                        }
                        else
                        {
                            string Aswer = TotalAnswer[ReadSelect];

                            if (Aswer.Contains("#"))
                            {
                                ProcessTask.Process(Global._TCPManager.MySocketListener, pool, client, npc.ResID, npc.ResID, -1, TaskTypes.AnswerQuest);

                                KTPlayerManager.ShowNotification(client, "Rất tốt người đã trả lời đúng!");

                                // Trả luôn nhiệm vụ
                                this.CompleteTask(map, npc, client, TaskID);
                            }
                            else
                            {
                                _NpcDialog.Text = "Ta rất tiếc!,Ngươi đã trả lời sai rồi.";

                                _NpcDialog.Show(npc, client);
                            }
                        }
                    }
                }
            }
            else
            {
                KTPlayerManager.ShowNotification(client, "Có lỗi khi nhận nhiệm vụ");
            }
        }

        public void GenEndQuestData(GameMap map, NPC npc, KPlayer client, int TaskID)
        {
            // LogManager.WriteLog(LogTypes.Quest, "GEN ENDQEST DATA :" + TaskID);
            TCPOutPacketPool pool = Global._TCPManager.TcpOutPacketPool;
            TCPClientPool tcpClientPool = Global._TCPManager.tcpClientPool;
            KNPCDialog _NpcDialog = new KNPCDialog();

            Dictionary<int, string> Selections = new Dictionary<int, string>();

            Dictionary<int, string> OtherPram = new Dictionary<int, string>();

            Task _TaskGet = this.GetTaskTemplate(TaskID);

            if (_TaskGet != null)
            {
                _NpcDialog.Text = KTGlobal.CreateStringByColor("Nhiệm Vụ :", ColorType.Accpect) + "<color=green>" + _TaskGet.Title + "</color><br>" + KTGlobal.CreateStringByColor(npc.Name + ":", ColorType.Accpect) + "<br>" + _TaskGet.CompleteTalk;

                Selections.Add(_TaskGet.ID, KTGlobal.CreateStringByColor("[Hoàn Thành]" + _TaskGet.Title, ColorType.Done));

                _NpcDialog.Selections = Selections;

                _NpcDialog.OtherParams = OtherPram;

                Action<TaskCallBack> ActionWork = (x) => this.CompleteTask(map, npc, client, TaskID);

                _NpcDialog.OnSelect = ActionWork;

                //LogManager.WriteLog(LogTypes.Quest, "SHOW ENDQEST DATA :" + TaskID);

                _NpcDialog.Show(npc, client);
            }
            else
            {
                KTPlayerManager.ShowNotification(client, "Có lỗi khi trả nhiệm vụ");
            }
        }

        /// <summary>
        /// Class xử lý các thao tác của MAIN QUEST
        /// </summary>
        /// <param name="map"></param>
        /// <param name="npc"></param>
        /// <param name="client"></param>
        /// <param name="TaskData"></param>
        public void MainTaskProsec(GameMap map, NPC npc, KPlayer client, TaskCallBack TaskData)
        {
            int Select = TaskData.SelectID;

            if (Select == 8888)
            {
                KT_TCPHandler.CloseDialog(client);
            }
            else if (Select == 9999)
            {
                KTLuaEnvironment.ExecuteNPCScript_Open(map, npc, client, npc.ScriptID, TaskData.OtherParams);
            }
            else if (TaskData.OtherParams != null && TaskData.OtherParams.Count > 0)
            {
                string Value = "";

                if (TaskData.OtherParams.TryGetValue(TaskData.SelectID, out Value))
                {
                    string[] Pram = Value.Split('|');

                    if (Pram.Length > 0)
                    {
                        string Action = Pram[0];
                        string Step = Pram[1];

                        if (Action == "ACCPECT")
                        {
                            if (Step == "STEP1")
                            {
                                this.GenAccpectQuestData(map, npc, client, TaskData.SelectID);
                            }
                            if (Step == "STEP2")
                            {
                                this.AppcepTask(npc, client, TaskData.SelectID);
                            }
                        }

                        if (Action == "END")
                        {
                            if (Step == "STEP1")
                            {
                                this.GenEndQuestData(map, npc, client, TaskData.SelectID);
                            }
                        }
                        if (Action == "QUEST")
                        {
                            if (Step == "STEP1")
                            {
                                this.AppcepTask(npc, client, TaskData.SelectID, false);
                                this.GenQuestData(map, npc, client, TaskData.SelectID);
                            }
                        }
                    }
                }
                else
                {
                    KTLuaEnvironment.ExecuteNPCScript_Open(map, npc, client, npc.ScriptID, TaskData.OtherParams);
                }
            }
        }

        /// <summary>
        /// Gen ra text quest cho NPC
        /// </summary>
        /// <param name="map"></param>
        /// <param name="npc"></param>
        /// <param name="client"></param>
        public void GetNpcDataQuest(GameMap map, NPC npc, KPlayer client)
        {
            TCPOutPacketPool pool = Global._TCPManager.TcpOutPacketPool;
            TCPClientPool tcpClientPool = Global._TCPManager.tcpClientPool;

            List<int> tasksList = null;

            KNPCDialog _NpcDialog = new KNPCDialog();

            Dictionary<int, string> Selections = new Dictionary<int, string>();

            Dictionary<int, string> OtherPram = new Dictionary<int, string>();

            _NpcDialog.Text = "Đến thật đúng lúc, ta có hoạt động cho ngươi";

            if (GameManager.NPCTasksMgr.SourceNPCTasksDict.TryGetValue(npc.ResID, out tasksList))
            {
                //Lấy ra toàn bộ danh sách nhiệm thằng này có
                for (int i = 0; i < tasksList.Count; i++)
                {
                    //Lấy ra temp nhiệm vụ
                    Task _TaskGet = this.GetTaskTemplate(tasksList[i]);

                    if (_TaskGet != null)
                    {
                        if (_TaskGet.TaskClass == (int)TaskClasses.MainTask || _TaskGet.TaskClass == (int)TaskClasses.TheGioi)
                        {
                            if (CanTakeNewTask(client, _TaskGet.ID))
                            {
                                OtherPram.Add(_TaskGet.ID, "ACCPECT|STEP1");

                                Selections.Add(_TaskGet.ID, KTGlobal.CreateStringByColor("[Nhiệm Vụ]" + _TaskGet.Title, ColorType.Accpect));
                            }
                        }
                    }
                }
            }

            List<int> ListDoneQuest = null;

            if (GameManager.NPCTasksMgr.DestNPCTasksDict.TryGetValue(npc.ResID, out ListDoneQuest))
            {
                for (int i = 0; i < ListDoneQuest.Count; i++)
                {
                    Task _TaskGet = this.GetTaskTemplate(ListDoneQuest[i]);

                    if (_TaskGet != null)
                    {
                        TaskData _FindTaskData = TaskManager.getInstance().GetTaskData(_TaskGet.ID, client);

                        if (_FindTaskData != null)
                        {
                            if (!TaskManager.getInstance().IsQuestComplete(_TaskGet.ID, _FindTaskData.DoingTaskVal1) && _TaskGet.TargetType == (int)TaskTypes.Talk)
                            {
                                ProcessTask.Process(Global._TCPManager.MySocketListener, pool, client, npc.ResID, npc.ResID, -1, TaskTypes.Talk);

                                if (TaskManager.getInstance().IsQuestComplete(_TaskGet.ID, _FindTaskData.DoingTaskVal1))
                                {
                                    _NpcDialog.Text = "Có vẻ như người đã hoàn thành nhiệm vụ :" + _TaskGet.Title + ".Hãy xem ta giúp được người điều gì?";

                                    OtherPram.Add(_TaskGet.ID, "END|STEP1");

                                    Selections.Add(_TaskGet.ID, KTGlobal.CreateStringByColor("[Nhiệm Vụ]" + _TaskGet.Title, ColorType.Accpect));
                                }
                                else
                                {
                                    Selections.Add(_TaskGet.ID, "Ta sẽ quay lại sau");
                                }
                            }
                            else if (!TaskManager.getInstance().IsQuestComplete(_TaskGet.ID, _FindTaskData.DoingTaskVal1) && _TaskGet.TargetType == (int)TaskTypes.AnswerQuest)
                            {
                                string[] TaskPram = _TaskGet.QuestionTable.Split('|');

                                if (TaskPram.Length > 0)
                                {
                                    _NpcDialog.Text = KTGlobal.CreateStringByColor("Câu hỏi :", ColorType.Accpect) + TaskPram[0] + "<br><br>" + KTGlobal.CreateStringByColor("Chọn 1 trong các câu trả lời :", ColorType.Done) + "<br><br>";

                                    string[] TotalAnswer = TaskPram[1].Split('\n');

                                    int j = 1;

                                    foreach (string awer in TotalAnswer)
                                    {
                                        Selections.Add(j, awer.Replace("#", ""));
                                        j = j + 1;
                                    }

                                    Selections.Add(100, KTGlobal.CreateStringByColor("Ta sẽ trả lời sau", ColorType.Importal));

                                    Action<TaskCallBack> ActionWork = (x) => AswerQuestion(map, npc, client, x, _TaskGet.ID);

                                    _NpcDialog.OnSelect = ActionWork;

                                    _NpcDialog.Selections = Selections;

                                    _NpcDialog.OtherParams = OtherPram;

                                    _NpcDialog.Show(npc, client);

                                    return;
                                }
                                else
                                {
                                    KTPlayerManager.ShowNotification(client, "Config nhiệm vụ bị lỗi");
                                    return;
                                }
                            }
                            else
                            {
                                OtherPram.Add(_TaskGet.ID, "END|STEP1");
                                Selections.Add(_TaskGet.ID, KTGlobal.CreateStringByColor("[Nhiệm Vụ]" + _TaskGet.Title, ColorType.Accpect));
                            }
                        }
                    }
                }
            }

            if (Selections.Count > 0)
            {
                Action<TaskCallBack> ActionWork = (x) => this.MainTaskProsec(map, npc, client, x);

                if (!Selections.ContainsKey(999))
                {
                    Selections.Add(9999, KTGlobal.CreateStringByColor("Ta muốn hỏi chuyện khác", ColorType.Normal));
                }
                if (!Selections.ContainsKey(8888))
                {
                    Selections.Add(8888, KTGlobal.CreateStringByColor("Kết thúc đối thoại", ColorType.Normal));
                }

                _NpcDialog.OnSelect = ActionWork;

                _NpcDialog.Selections = Selections;

                _NpcDialog.OtherParams = OtherPram;

                _NpcDialog.Show(npc, client);
            }
            else
            {
                KTLuaEnvironment.ExecuteNPCScript_Open(map, npc, client, npc.ScriptID, new Dictionary<int, string>());
            }
        }

        /// <summary>
        /// Gen kiểu task câu hỏi
        /// </summary>
        /// <param name="map"></param>
        /// <param name="npc"></param>
        /// <param name="client"></param>
        /// <param name="TaskID"></param>
        public void GenQuestData(GameMap map, NPC npc, KPlayer client, int TaskID)
        {
            TCPOutPacketPool pool = Global._TCPManager.TcpOutPacketPool;
            TCPClientPool tcpClientPool = Global._TCPManager.tcpClientPool;
            KNPCDialog _NpcDialog = new KNPCDialog();

            Dictionary<int, string> Selections = new Dictionary<int, string>();

            Dictionary<int, string> OtherPram = new Dictionary<int, string>();

            Task _TaskGet = this.GetTaskTemplate(TaskID);

            if (_TaskGet != null)
            {
                string[] TaskPram = _TaskGet.QuestionTable.Split('|');

                if (TaskPram.Length > 0)
                {
                    _NpcDialog.Text = KTGlobal.CreateStringByColor("Câu hỏi :", ColorType.Accpect) + TaskPram[0] + "<br><br>" + KTGlobal.CreateStringByColor("Chọn 1 trong các câu trả lời :", ColorType.Done) + "<br><br>";

                    string[] TotalAnswer = TaskPram[1].Split('\n');

                    int j = 1;

                    foreach (string awer in TotalAnswer)
                    {
                        Selections.Add(j, awer.Replace("#", ""));
                        j = j + 1;
                    }

                    Selections.Add(100, KTGlobal.CreateStringByColor("Ta sẽ trả lời sau", ColorType.Importal));

                    Action<TaskCallBack> ActionWork = (x) => AswerQuestion(map, npc, client, x, _TaskGet.ID);

                    _NpcDialog.OnSelect = ActionWork;

                    _NpcDialog.Selections = Selections;

                    _NpcDialog.OtherParams = OtherPram;

                    _NpcDialog.Show(npc, client);
                    return;
                }
                else
                {
                    KTPlayerManager.ShowNotification(client, "Config nhiệm vụ bị lỗi");
                    return;
                }
            }
            else
            {
                KTPlayerManager.ShowNotification(client, "Có lỗi khi nhận nhiệm vụ");
            }
        }

        /// <summary>
        /// Trả nhiệm vụ
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

                    if (_TaskFind.TaskClass == (int)TaskClasses.MainTask)
                    {
                        if (this.IsHaveMainQuest(npc.ResID, client))
                        {
                            this.GetNpcDataQuest(map, npc, client);
                        }
                    }

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

        public string GetTaskAdward(Task _Task)
        {
            string NOTE = "";

            if (_Task != null)
            {
                if (_Task.Experienceaward != 0)
                {
                    NOTE += "<color=green>Kinh Nghiệm : </color>" + _Task.Experienceaward + "\n";
                }

                if (_Task.BacKhoa > 0)
                {
                    NOTE += "<color=green>Bạc Khóa : </color>" + _Task.BacKhoa + "\n";
                }

                if (_Task.Bac > 0)
                {
                    NOTE += "<color=green>Bạc : </color>" + _Task.Bac + "\n";
                }

                if (_Task.DongKhoa > 0)
                {
                    NOTE += "<color=green>Đồng Khóa : </color>" + _Task.DongKhoa + "\n";
                }

              

                if (_Task.Taskaward != "")
                {
                  
                    string[] Splist = _Task.Taskaward.Split('#');

                    foreach (string Item in Splist)
                    {
                        string[] ItemPram = Item.Split(',');
                        int ItemID = Int32.Parse(ItemPram[0]);
                        int ItemNum = Int32.Parse(ItemPram[1]);
                        int LockStatus = Int32.Parse(ItemPram[2]);

                        ItemData _Item = ItemManager.GetItemTemplate(ItemID);

                        NOTE += "<color=blue> " + _Item.Name + ": X" + ItemNum + "</color>\n";
                    }
                }
            }

            return NOTE;
        }
        
        /// <summary>
        /// Gen ra accpect task
        /// </summary>
        /// <param name="map"></param>
        /// <param name="npc"></param>
        /// <param name="client"></param>
        /// <param name="TaskID"></param>
        public void GenAccpectQuestData(GameMap map, NPC npc, KPlayer client, int TaskID)
        {
            TCPOutPacketPool pool = Global._TCPManager.TcpOutPacketPool;
            TCPClientPool tcpClientPool = Global._TCPManager.tcpClientPool;
            KNPCDialog _NpcDialog = new KNPCDialog();

            Dictionary<int, string> Selections = new Dictionary<int, string>();

            Dictionary<int, string> OtherPram = new Dictionary<int, string>();

            Task _TaskGet = this.GetTaskTemplate(TaskID);

            if (_TaskGet != null)
            {
                //Nếu là kiểu trả lời câu hỏi thì GEN Bộ câu hỏi
                if (_TaskGet.TargetType == (int)TaskTypes.AnswerQuest)
                {
                    this.AppcepTask(npc, client, _TaskGet.ID, false);

                    this.GenQuestData(map, npc, client, _TaskGet.ID);

                    return;
                }
                else
                {
                    _NpcDialog.Text = KTGlobal.CreateStringByColor("Nhiệm Vụ : ", ColorType.Accpect) + "<color=green>" + _TaskGet.Title + "</color><br>" + KTGlobal.CreateStringByColor("Mô Tả :", ColorType.Accpect) + "<br>" + _TaskGet.AcceptTalk + "<br>" + KTGlobal.CreateStringByColor("Mục Tiêu :", ColorType.Accpect) + "<br>" + _TaskGet.DoingTalk;

                    string Adward = GetTaskAdward(_TaskGet);

                    if (Adward.Length>0)
                    {
                        _NpcDialog.Text += "\n<color=yellow>Phần Thưởng: </color>\n" + Adward;

                    }    
                    OtherPram.Add(_TaskGet.ID, "ACCPECT|STEP2");

                    Selections.Add(_TaskGet.ID, KTGlobal.CreateStringByColor("Tiếp nhận", ColorType.Accpect));

                    Selections.Add(8888, KTGlobal.CreateStringByColor("Ta sẽ làm sau", ColorType.Importal));

                    _NpcDialog.Selections = Selections;

                    _NpcDialog.OtherParams = OtherPram;

                    Action<TaskCallBack> ActionWork = (x) => MainTaskProsec(map, npc, client, x);

                    _NpcDialog.OnSelect = ActionWork;

                    _NpcDialog.Show(npc, client);
                }
            }
            else
            {
                KTPlayerManager.ShowNotification(client, "Có lỗi khi nhận nhiệm vụ");
            }
        }

        /// <summary>
        /// Nhận nhiệm vụ mới
        /// </summary>
        /// <param name="npc"></param>
        /// <param name="client"></param>
        /// <param name="TaskID"></param>
        /// <param name="IsCloseATEnd"></param>
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
    }
}