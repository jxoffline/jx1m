using GameServer.KiemThe.Core.Item;
using GameServer.KiemThe.Entities;
using GameServer.KiemThe.Logic;
using GameServer.KiemThe.LuaSystem.Logic;
using GameServer.KiemThe.Utilities;
using GameServer.Logic;
using GameServer.Server;
using Server.Data;
using Server.Protocol;
using Server.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.Networking.Types;
using static GameServer.KiemThe.Logic.KTMonsterManager;
using static GameServer.Logic.KTMapManager;

namespace GameServer.KiemThe.Core.Task
{
    /// <summary>
    /// Xử lý nhiệm vụ nghĩa quân
    /// </summary>
    public class TaskDailyArmyManager
    {
        private static TaskDailyArmyManager instance = new TaskDailyArmyManager();

        public Dictionary<int, Task> _TotalTaskData = new Dictionary<int, Task>();

        /// <summary>
        /// Số nhiệm vụ tối đa có thể alfm trong ngày
        /// </summary>
        public int MaxQuestPerDay = 20;

        public static TaskDailyArmyManager getInstance()
        {
            return instance;
        }

        #region QuestSaveData

        /// <summary>
        /// Lấy ra số lượt đã làm dã tẩu trong ngày
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        public int GetTaskDailyArmyCurentCount(KPlayer client)
        {
            int totalRounds = client.GetValueOfDailyRecore((int)DailyRecord.TaskDailyArmyCurentCount);

            if (totalRounds <= 0)
            {
                totalRounds = 0;
            }

            return totalRounds;
        }

        /// <summary>
        /// Lấy ra số nhiệm vụ tối đa có thể làm của thằng này trong ngày
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        public int GetLessQuestInDay(KPlayer client)
        {
            int Total = GetTaskDailyArmyCurentCountWithCancel(client);

            return (this.MaxQuestPerDay - Total);
        }

        public int GetTaskDailyArmyCurentCountWithCancel(KPlayer client)
        {
            int totalRounds = client.GetValueOfDailyRecore((int)DailyRecord.TaskDailyArmyCurentCountWithCancel);

            if (totalRounds <= 0)
            {
                totalRounds = 0;
            }

            return totalRounds;
        }

        public void SetTaskDailyArmyCurentCountWithCancel(KPlayer client, int Value)
        {
            client.SetValueOfDailyRecore((int)DailyRecord.TaskDailyArmyCurentCountWithCancel, Value);
        }

        /// <summary>
        /// Set số lần đã hoàn thành nhiệm vụ trong ngày
        /// </summary>
        /// <param name="client"></param>
        public void SetTaskDailyArmyCurentCount(KPlayer client, int Value)
        {
            client.SetValueOfDailyRecore((int)DailyRecord.TaskDailyArmyCurentCount, Value);
        }

        /// <summary>
        /// Lấy ra nhiệm vụ hiện tại đang làm là nhiệm vụ nào
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        public int GetTaskDailyArmyCurentTaskID(KPlayer client)
        {
            int CurentTaskID = client.GetValueOfForeverRecore(ForeverRecord.TaskDailyArmyCurentTaskID);

            return CurentTaskID;
        }

        /// <summary>
        /// Set nhiệm vụ đang làm là nhiệm vụ nào
        /// </summary>
        /// <param name="client"></param>
        public void SetTaskDailyArmyCurentTaskID(KPlayer client, int TaskID)
        {
            //Set giá trị cho recore
            client.SetValueOfForeverRecore(ForeverRecord.TaskDailyArmyCurentTaskID, TaskID);
        }

        public int GetTaskDailyArmyTotalCount(KPlayer client)
        {
            int TotalCount = client.GetValueOfForeverRecore(ForeverRecord.TaskDailyArmyTotalCount);
            if (TotalCount < 0)
            {
                TotalCount = 0;
            }
            return TotalCount;
        }

        /// <summary>
        /// Set nhiệm vụ đang làm là nhiệm vụ nào
        /// </summary>
        /// <param name="client"></param>
        public void SetTaskDailyArmyTotalCount(KPlayer client, int TaskID)
        {
            //Set giá trị cho recore
            client.SetValueOfForeverRecore(ForeverRecord.TaskDailyArmyTotalCount, TaskID);
        }

        public int GetTaskDailyArmyCancelQuest(KPlayer client)
        {
            int CurentTaskID = client.GetValueOfForeverRecore(ForeverRecord.TaskDailyArmyCancelQuest);

            return CurentTaskID;
        }

        /// <summary>
        /// Set nhiệm vụ đang làm là nhiệm vụ nào
        /// </summary>
        /// <param name="client"></param>
        public void SetTaskDailyArmyCancelQuest(KPlayer client, int TaskID)
        {
            //Set giá trị cho recore
            client.SetValueOfForeverRecore(ForeverRecord.TaskDailyArmyCancelQuest, TaskID);
        }

        public int GetTaskDailyArmyMaxStreakCount(KPlayer client)
        {
            int CurentTaskID = client.GetValueOfForeverRecore(ForeverRecord.TaskDailyArmyMaxStreakCount);

            return CurentTaskID;
        }

        /// <summary>
        /// Set nhiệm vụ đang làm là nhiệm vụ nào
        /// </summary>
        /// <param name="client"></param>
        public void SetTaskDailyArmyMaxStreakCount(KPlayer client, int TaskID)
        {
            //Set giá trị cho recore
            client.SetValueOfForeverRecore(ForeverRecord.TaskDailyArmyMaxStreakCount, TaskID);
        }

        public int GetTaskDailyArmyCurentAward(KPlayer client)
        {
            int CurentTaskID = client.GetValueOfForeverRecore(ForeverRecord.TaskDailyArmyCurentAward);
            if(CurentTaskID<0)
            {
                CurentTaskID = 0;
            }    

            return CurentTaskID;
        }

        /// <summary>
        /// Set nhiệm vụ đang làm là nhiệm vụ nào
        /// </summary>
        /// <param name="client"></param>
        public void SetTaskDailyArmyCurentAward(KPlayer client, int TaskID)
        {
            //Set giá trị cho recore
            client.SetValueOfForeverRecore(ForeverRecord.TaskDailyArmyCurentAward, TaskID);
        }

        #endregion QuestSaveData

        public bool IsHaveCompleteArmyQuest(int NpcID, KPlayer Client)
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
                        IsHaveComplteQuest = true;
                        break;
                    }

                    if (this.IsQuestComplete(find.DoingTaskID, find.DoingTaskVal1))
                    {
                        IsHaveComplteQuest = true;
                        break;
                    }
                }
            }

            return IsHaveComplteQuest;
        }

        public bool IsQuestComplete(int TaskID, int TaskValue)
        {
            bool IsComplete = false;

            Task _Task = this.GetTaskTemplate(TaskID);

            if (_Task.TargetType == (int)TaskTypes.Talk || _Task.TargetType == (int)TaskTypes.AnswerQuest || _Task.TargetType == (int)TaskTypes.JoinFaction || _Task.TargetType == (int)TaskTypes.GetItemWithSpcecialLine)
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

        public bool IsHaveArmyQuest(int NpcID, KPlayer client)
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
            }

            return IsHaveQuest;
        }

        /// <summary>
        /// Set vào danh sách nhiệm vụ
        /// </summary>
        /// <param name="_InPut"></param>
        public void SetTask(Dictionary<int, Task> _InPut)
        {
            this._TotalTaskData = _InPut;
        }

        public bool CanTakeNewTask(KPlayer client, int taskID)
        {
            Task systemTask = this.GetTaskTemplate(taskID);

            if (systemTask == null)
            {
                return false;
            }

            // Nếu trên người đang có 1 task BVĐ đang làm dở thì méo cho nhận nữa
            if (client.TaskDataList != null)
            {
                if (client.TaskDataList.Count > 0)
                {
                    //Kiểm tra xem nó đang có nhiệm vụ bạo vặn đồng nào khác không
                    foreach (TaskData task in client.TaskDataList)
                    {
                        Task _Task = this.GetTaskTemplate(task.DoingTaskID);
                        {
                            if (_Task != null)
                            {
                                if ((int)TaskClasses.NghiaQuan == _Task.TaskClass)
                                {
                                    return false;
                                }
                            }
                        }
                    }
                }
            }
            //Nếu ko đủ cấp thì không được nhận
            if (client.m_Level < 20)
            {
                return false;
            }
            //Nếu phái không hợp
            if (client.m_cPlayerFaction.GetFactionId() == 0)
            {
                return false;
            }
            // Nếu đã làm tổng 50 nhiệm vụ 1 ngày thì thôi
            if (this.GetTaskDailyArmyCurentCountWithCancel(client) >= this.MaxQuestPerDay)
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
        /// Lấy ra thông tin giã tảu của nhân vật
        /// </summary>
        /// <param name="map"></param>
        /// <param name="npc"></param>
        /// <param name="client"></param>
        public void GenInfoTaskDesc(GameMap map, NPC npc, KPlayer client)
        {
            TCPOutPacketPool pool = Global._TCPManager.TcpOutPacketPool;
            TCPClientPool tcpClientPool = Global._TCPManager.tcpClientPool;
            KNPCDialog _NpcDialog = new KNPCDialog();

            Dictionary<int, string> Selections = new Dictionary<int, string>();

            Dictionary<int, string> OtherPram = new Dictionary<int, string>();

            _NpcDialog.Text = "<color=red>Mô Tả:</color> Dã tẩu là 1 hệ thống nhiệm vụ tuần hoàn bao gồm (<color=green>Thu Thập,Tìm Vật Phẩm,Tiêu Diệt Quái</color>)!\nSau khi hoàn thành chuỗi các hệ thống nhiệm vụ quý bằng hữu có thể nhận được nhiều phần thưởng giá trị.\n<color=red>Điều Kiện Tham Gia:</color>\n  -Đã gia nhập môn phái\n  -Cấp độ 40 trở lên\n<color=red>Cách Thức Tham Gia:</color>\n Đến gặp NPC Dã Tẩu ở các thôn và thành thị để nhận nhiệm vụ,Mỗi nhân vật chỉ có thể làm tối đa 20 nhiệm vụ mỗi ngày\n<color=red>Phần Thưởng Tuần Hoàn:</color>\nCứ mỗi 50 nhiệm vụ : 1 Tiên Thảo Lộ\n<color=red>Phần Thưởng Chuỗi Khi Đạt Các Mốc:</color>\n500 Nhiệm Vụ - Kim Tinh Hổ Vương\n1000 Nhiệm Vụ - Rương một trang bị Định Quốc ( <color=green>Ngẫu Nhiên</color> ) ( <color=green>Không Khóa</color> )\n2000 Nhiệm Vụ - Rương một trang bị An Bang ( <color=green>Ngẫu Nhiên</color> ) ( <color=green>Không Khóa</color> )\n3000 Nhiệm Vụ - Rương một trang bị Hoàng Kim Kim Quang ( <color=green>Ngẫu Nhiên</color>) ( <color=green>Không Khóa</color> )\n5000 Nhiệm Vụ  - Rương một trang bị Hoàng Kim Môn Phái ( <color=green>Ngẫu Nhiên</color>) ( <color=green>Không Khóa</color> )\n<color=red>Lưu ý:</color>\nKhi bạn hủy nhiệm vụ nếu sử dụng 200 mảnh <color=green>THẦN BÍ ĐỒ CHÍ</color> thì chuỗi nhiệm vụ đang làm sẽ không bị reset,nếu không sử dụng mảnh chuỗi nhiệm vụ của bạn sẽ reset về 0";

            Selections.Add(5, KTGlobal.CreateStringByColor("Ta Đã Hiểu", ColorType.Normal));

            Action<TaskCallBack> ActionWork = (x) => QuestArmyProsecc(map, npc, client, x, null);

            _NpcDialog.OnSelect = ActionWork;

            _NpcDialog.Selections = Selections;

            _NpcDialog.OtherParams = OtherPram;

            _NpcDialog.Show(npc, client);
        }

        public string GetDesc(Task _TaskData)
        {
            string INPUT = "";

            if (_TaskData != null)
            {
                if (_TaskData.TargetType == (int)TaskTypes.GetItemWithSpcecialLine)
                {
                    INPUT += "Hãy đi tìm cho ta một ";
                    string DataRequest = _TaskData.PropsName;

                    string[] Pram = DataRequest.Split('|');
                    int ItemType = Int32.Parse(Pram[0]);
                    int ItemCatagory = -1;
                    if (ItemType == 0 || ItemType == 1)
                    {
                        ItemCatagory = Int32.Parse(Pram[1]);

                        INPUT += "<color=green>" + ItemManager.GetWeaponKind(ItemCatagory) + "</color>";
                    }
                    else
                    {
                        KE_ITEM_EQUIP_DETAILTYPE _ITEM = (KE_ITEM_EQUIP_DETAILTYPE)ItemType;

                        INPUT += "<color=green>" + ItemManager.GetEquipTypeString(_ITEM) + "</color>";
                    }

                    int ItemSeries = Int32.Parse(Pram[2]);
                    if (ItemSeries != 0)
                    {
                        INPUT += "và có ngũ hành trang bị là : " + KTGlobal.GetSeriesText(ItemSeries) + "";
                    }

                    int SymboyID = Int32.Parse(Pram[3]);
                    int ValueSymboy = Int32.Parse(Pram[4]);

                    if (SymboyID != -1)
                    {
                        if (PropertyDefine.PropertiesByID.TryGetValue(SymboyID, out PropertyDefine.Property property))
                        {
                            if (property.Description.Contains(':'))
                            {
                                if (!property.IsPercent)
                                {
                                    // Lấy vế đầu tiên
                                    INPUT += " có dòng <color=green>" + property.Description.Split(':')[0] + "</color> tối thiểu giá trị <color=red>" + ValueSymboy + " điểm </color>";
                                }
                                else
                                {
                                    INPUT += " có dòng <color=green>" + property.Description.Split(':')[0] + "</color> tối thiểu giá trị <color=red>" + ValueSymboy + " % </color>";
                                }
                            }
                        }
                    }
                }
                else if (_TaskData.TargetType == (int)TaskTypes.KillMonster)
                {
                    MonsterTemplateData _Monster = KTMonsterManager.GetTemplate(_TaskData.TargetNPC);
                    if (_Monster!=null)
                    {
                        GameMap map = KTMapManager.Find(_TaskData.TargetMapCode);
                        if (map != null)
                        {
                            INPUT += "Hãy đi tiêu diệt cho ta <color=red>" + _Monster.Name + "</color> với số lượng <color=red>" + _TaskData.TargetNum + "</color> " + "tại bản đồ ["+map.MapName+"]";
                        }
                       
                    }    

                   
                }
                else if (_TaskData.TargetType == (int)TaskTypes.Collect)
                {

                    NPC npc = KTNPCManager.Find(x => x.MapCode == _TaskData.TargetMapCode && x.CopyMapID == -1 && x.ResID == _TaskData.TargetNPC);
                   

                    if (npc != null)
                    {
                        GameMap map = KTMapManager.Find(_TaskData.TargetMapCode);
                        if (map != null)
                        {
                            INPUT += "Hãy đi hái cho ta <color=red>" + npc.Name + "</color> với số lượng <color=red>" + _TaskData.TargetNum + "</color> " + "tại bản đồ [" + map.MapName + "]";
                        }

                    }


                }
                else if (_TaskData.TargetType == (int)TaskTypes.Crafting || _TaskData.TargetType == (int)TaskTypes.BuySomething)
                {

                    NPC npc = KTNPCManager.Find(x => x.MapCode == _TaskData.TargetMapCode && x.CopyMapID == -1 && x.ResID == _TaskData.TargetNPC);


                    if (npc != null)
                    {
                        GameMap map = KTMapManager.Find(_TaskData.TargetMapCode);
                        if (map != null)
                        {
                            INPUT += "Hãy đi hái cho ta <color=red>" + npc.Name + "</color> với số lượng <color=red>" + _TaskData.TargetNum + "</color> " + "tại bản đồ [" + map.MapName + "]";
                        }

                    }


                }
            }

            return INPUT;
        }

        /// <summary>
        /// Gen NPC data quest
        /// </summary>
        /// <param name="map"></param>
        /// <param name="npc"></param>
        /// <param name="client"></param>
        public void GetNpcDataQuest(GameMap map, NPC npc, KPlayer client)
        {
            TCPOutPacketPool pool = Global._TCPManager.TcpOutPacketPool;
            TCPClientPool tcpClientPool = Global._TCPManager.tcpClientPool;
            KNPCDialog _NpcDialog = new KNPCDialog();

            //List<DialogItemSelectionInfo> Items = new List<DialogItemSelectionInfo>();

            //DialogItemSelectionInfo _ItemA = new DialogItemSelectionInfo();
            //_ItemA.ItemID = 183;
            //_ItemA.Quantity = 1;
            //_ItemA.Binding = 1;

            //Items.Add(_ItemA);

            //_NpcDialog.Items = Items;
            //_NpcDialog.ItemHeaderString = "Phần thưởng";

            Dictionary<int, string> Selections = new Dictionary<int, string>();

            Dictionary<int, string> OtherPram = new Dictionary<int, string>();

            // Lấy nhiệm vụ hiện tại nếu là -1 thì cho nó cái task vào
            if (this.GetTaskDailyArmyCurentTaskID(client) == -1)
            {
                this.GiveTaskArmyDaily(client, true);
            }

            // Lấy ra nhiệm vụ hiện tại
            Task _TaskGet = this.GetTaskTemplate(this.GetTaskDailyArmyCurentTaskID(client));

            if (client.TaskDataList != null)
            {
                // Đoạn này để tránh bug nếu mà nhiệm vụ hiện tại khác nhiệm vụ đã nhận trước đó
                foreach (TaskData TaskArmy in client.TaskDataList)
                {
                    Task _Task = this.GetTaskTemplate(TaskArmy.DoingTaskID);

                    //Tức là đang có nhiệm vụ BVD đang nhận
                    if (_Task != null)
                    {
                        if (_TaskGet.ID != _Task.ID)
                        {
                            // Gán nhiện vụ hiện tại = nhiệm vụ đang hoàn thành
                            _TaskGet = _Task;
                        }
                    }
                }
            }

            string Hisitory = "<color=yellow>Thông Tin :</color>\n<color=green>Số nhiệm vụ đã hoàn thành trong ngày :</color> " + this.GetTaskDailyArmyCurentCount(client) + "\n<color=green>Số nhiệm vụ còn lại trong ngày : </color>" + this.GetLessQuestInDay(client) + "\n<color=green>Tổng số chuỗi nhiệm vụ đã hoàn thành : </color>" + this.GetTaskDailyArmyTotalCount(client) + "\n<color=green>Mốc quà tặng chuỗi đã nhận : </color>"+ this.GetTaskDailyArmyCurentAward(client);

            string Adward = _TaskGet.Taskaward;

            string PhanThuong = "";

            if (Adward.Length > 0)
            {
                string[] Pram = Adward.Split(',');
                int ItemID = Int32.Parse(Pram[0]);
                int ItemNum = Int32.Parse(Pram[1]);

                ItemData _Find = ItemManager.GetItemTemplate(ItemID);

                if (_Find != null)
                {
                    PhanThuong = "<color=green>" + _Find.Name + "X" + ItemNum + "</color><br>";
                }
            }

            if(_TaskGet.Point2>0)
            {
                PhanThuong += "<color=green>Danh vọng Dã Tẩu</color>";
            }    

            _NpcDialog.Text = Hisitory + "<br>" + KTGlobal.CreateStringByColor("Nhiệm Vụ : ", ColorType.Accpect) + _TaskGet.Title + "<br>" + KTGlobal.CreateStringByColor("Mục Tiêu :", ColorType.Accpect) + "<br>" + GetDesc(_TaskGet) + "\n" + KTGlobal.CreateStringByColor("Phần Thưởng :", ColorType.Accpect) + "<br>" + PhanThuong;

            if (client.TaskDataList == null)
            {
                client.TaskDataList = new List<TaskData>();
            }

            var findTaskClient = client.TaskDataList.Where(x => x.DoingTaskID == _TaskGet.ID).FirstOrDefault();

            if (findTaskClient != null)
            {
                if (IsQuestComplete(findTaskClient.DoingTaskID, findTaskClient.DoingTaskVal1))
                {
                    Selections.Add(2, KTGlobal.CreateStringByColor("Hoàn Thành", ColorType.Done));
                }
                else
                {
                    Selections.Add(30, KTGlobal.CreateStringByColor("Hủy Nhiệm Vụ Sử Dụng Mật Đồ Thần Bí", ColorType.Importal));
                    Selections.Add(3, KTGlobal.CreateStringByColor("Hủy Nhiệm Vụ", ColorType.Importal));
                }
            }
            else
            {
                // Nếu số nhiệm vụ trong ngày đã làm còn nhỏ hơn 20 thì cho nhận còn ko thì thôi
                if (this.GetTaskDailyArmyCurentCountWithCancel(client) < this.MaxQuestPerDay)
                {
                    Selections.Add(1, KTGlobal.CreateStringByColor("Tiếp nhận", ColorType.Accpect));
                }
                else
                {
                    Selections.Add(5, KTGlobal.CreateStringByColor("Ngày mai quay lại", ColorType.Normal));
                }
            }

            Action<TaskCallBack> ActionWork = (x) => QuestArmyProsecc(map, npc, client, x, _TaskGet);

            Selections.Add(4, KTGlobal.CreateStringByColor("Thông tin", ColorType.Normal));

            _NpcDialog.OnSelect = ActionWork;

            _NpcDialog.Selections = Selections;

            _NpcDialog.OtherParams = OtherPram;

            _NpcDialog.ItemSelectable = false;

            _NpcDialog.Show(npc, client);
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

                    if (this.IsQuestComplete(find.DoingTaskID, find.DoingTaskVal1))
                    {
                        IsHaveComplteQuest = true;
                        break;
                    }
                }
            }

            return IsHaveComplteQuest;
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
            if (!this.IsQuestComplete(TaskID, taskData.DoingTaskVal1))
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

            // LogManager.WriteLog(LogTypes.Quest, "SENDDB :" + SendToDB);

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

                    this.GiveTaskArmyDaily(client, true);

               
                    //Tổng số nhiệm vụ đã làm hôm nay không tính canler
                    int CurentCount = this.GetTaskDailyArmyCurentCount(client);

                    //Set tổng số nhiệm vụ đã làm trong ngày
                    this.SetTaskDailyArmyCurentCount(client, CurentCount + 1);

                    //Tổng số nhiệm vụ đã làm
                    int TotalCount = this.GetTaskDailyArmyTotalCount(client);
                    //Set tổng số lượt đã làm
                    this.SetTaskDailyArmyTotalCount(client, TotalCount + 1);

                    //Tổng số nhiệm vụ đã làm bao gồm cả nhiệm vụ hủy
                    int TotalCountWithCancel = this.GetTaskDailyArmyCurentCountWithCancel(client);

                    this.SetTaskDailyArmyCurentCountWithCancel(client, TotalCountWithCancel + 1);

                    //Ghi lại chuỗi dài nhất
                    int MaxStreak = this.GetTaskDailyArmyMaxStreakCount(client);
                    if (TotalCount > MaxStreak)
                    {
                        this.SetTaskDailyArmyMaxStreakCount(client, TotalCount);
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

        /// <summary>
        /// Xử lý chuỗi nhiệm vụ
        /// </summary>
        /// <param name="map"></param>
        /// <param name="npc"></param>
        /// <param name="client"></param>
        /// <param name="TaskData"></param>
        /// <param name="TaskInput"></param>
        public void QuestArmyProsecc(GameMap map, NPC npc, KPlayer client, TaskCallBack TaskData, Task TaskInput)
        {
            if (TaskData.SelectID == 5) // Nếu là nhận nhiệm vụ
            {
                KT_TCPHandler.CloseDialog(client);
            }
            else
            {
                TCPOutPacketPool pool = Global._TCPManager.TcpOutPacketPool;

                if (TaskData != null)
                {
                    int SelectID = TaskData.SelectID;

                    var findTask = client.TaskDataList.Where(x => x.DoingTaskID == TaskInput.ID).FirstOrDefault();
                    if (findTask != null)
                    {
                        // Nếu là hủy nhiệm vụ
                        if (SelectID == 3)
                        {
                            // Nếu là chọn hủy nhiệm vụ
                            if (this.CancelTask(client, findTask.DbID, TaskInput.ID))
                            {
                                KTLuaLib_Player.SendMSG("Hủy nhiệm vụ thành công", client, npc);

                                KT_TCPHandler.NotifyUpdateTask(client, findTask.DbID, TaskInput.ID, -1, 0, 0);
                                int TotalCountWithCancel = this.GetTaskDailyArmyCurentCountWithCancel(client);
                                this.SetTaskDailyArmyCurentCountWithCancel(client, TotalCountWithCancel + 1);
                                this.SetTaskDailyArmyTotalCount(client,0);
                            
                                //Trao 1 nhiệm vụ mới
                                this.GiveTaskArmyDaily(client, true);
                            }
                        }
                        else if (SelectID == 30)
                        {

                            int COUNT = ItemManager.GetItemCountInBag(client, 8617);
                            if(COUNT < 200)
                            {
                                KTLuaLib_Player.SendMSG("Thần Bí Đồ Chí không đủ!", client, npc);
                            }
                            else
                            {
                                if(ItemManager.RemoveItemFromBag(client, 8617, 200))
                                {
                                    if (this.CancelTask(client, findTask.DbID, TaskInput.ID))
                                    {
                                        KTLuaLib_Player.SendMSG("Hủy nhiệm vụ thành công", client, npc);

                                        KT_TCPHandler.NotifyUpdateTask(client, findTask.DbID, TaskInput.ID, -1, 0, 0);

                                        int TotalCountWithCancel = this.GetTaskDailyArmyCurentCountWithCancel(client);

                                        this.SetTaskDailyArmyCurentCountWithCancel(client, TotalCountWithCancel + 1);
                                        //Trao 1 nhiệm vụ mới
                                        this.GiveTaskArmyDaily(client, true);
                                    }
                                }    
                               
                            }
                            // Nếu là chọn hủy nhiệm vụ
                         
                        }
                    }

                    if (SelectID == 2) // Nếu là hoàn thành nhiệm vụ
                    {
                        // Nếu là nhiệm vụ tìm đồ
                        if (TaskInput.TargetType == (int)TaskTypes.GetItemWithSpcecialLine)
                        {
                            // Đóng dialog Client
                            KT_TCPHandler.CloseDialog(client);
                            KTPlayerManager.SendOpenQuestReviceItem(client, TaskInput.ID, npc.NPCID);
                        }
                        else
                        {
                            this.CompleteTask(map, npc, client, TaskInput.ID);
                        }
                    }
                    else if (SelectID == 1) // Nếu là nhận nhiệm vụ
                    {
                        this.AppcepTask(npc, client, TaskInput.ID);
                    }
                    else if (SelectID == 4) // Nếu là nhận nhiệm vụ
                    {
                        // ấy ra thông tin thuyết minh hoạt động
                        this.GenInfoTaskDesc(map, npc, client);
                    }
                }
            }
        }

        public void ProsecCallBackItem(KPlayer client, List<GoodsData> TotalData, string CallBack)
        {
            if (TotalData.Count > 0)
            {
                int TaskID = Int32.Parse(CallBack.Split('_')[1]);
                int NpcID = Int32.Parse(CallBack.Split('_')[2]);

                NPC npc = KTNPCManager.Find(NpcID);
                if (npc == null)
                {
                    KTPlayerManager.ShowNotification(client, "Hãy đứng gần NPC để trả nhiệm vụ");
                    return;
                }

                GameMap map = KTMapManager.Find(client.MapCode);
                if (map == null)
                {
                    KTPlayerManager.ShowNotification(client, "Hãy đứng gần NPC để trả nhiệm vụ");
                    return;
                }

                Task _FindTask = GetTaskTemplate(TaskID);
                if (_FindTask != null)
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

                    List<GoodsData> TotalItemHave = TotalData;

                    bool IsHaveNeedItem = false;

                    if (TotalItemHave != null)
                    {
                        foreach (GoodsData goodsData in TotalItemHave)
                        {
                            KItem goodItem = new KItem(goodsData);
                            if (goodItem.IsExisSymboy(SymboyID, ValueSymboy))
                            {
                                if (ItemManager.RemoveItemByCount(client, goodsData, 1, "QUESTDATAU"))
                                {
                                    IsHaveNeedItem = true;
                                    this.CompleteTask(map, npc, client, TaskID);
                                    break;
                                }
                                else
                                {
                                    KTPlayerManager.ShowNotification(client, "Có lỗi khi trả nhiệm vụ");
                                }
                            }
                        }
                    }
                    if (!IsHaveNeedItem)
                    {
                        KTPlayerManager.ShowMessageBox(client, "Thông Báo", "Các vật phẩm người đặt lên không phải vật phẩm ta đang cần,Người định lừa lão già này ư?");
                    }
                }
                else
                {
                    KTPlayerManager.ShowNotification(client, "Có lỗi khi trả nhiệm vụ");
                }
            }
            else
            {
                KTPlayerManager.ShowNotification(client, "Vui lòng đặt vào vật phẩm trả nhiệm vụ");
            }
        }

        /// <summary>
        /// Nhận 1 nhiệm vụ mới
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

            if (client.m_Level < 20)
            {
                KTPlayerManager.ShowNotification(client, "Cấp độ không đủ để nhận!");
                return;
            }
            if (client.m_cPlayerFaction.GetFactionId() == 0)
            {
                KTPlayerManager.ShowNotification(client, "Vào phái mới có thể nhận nhiệm vụ này");
                return;
            }

            if (this.GetTaskDailyArmyCurentCountWithCancel(client) > 20)
            {
                KTPlayerManager.ShowNotification(client, "Nay người đã làm hết số lần rồi!");
                return;
            }

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

            // Nếu là nhiệm vụ tuần hoàn
            int nStarLevel = 1;

            if (npc == null)
            {
                strcmd = string.Format("{0}:{1}:{2}:{3}:{4}", client.RoleID, -1, TaskID, focus, nStarLevel);
            }
            else
            {
                strcmd = string.Format("{0}:{1}:{2}:{3}:{4}", client.RoleID, npc.ResID, TaskID, focus, nStarLevel);
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

            // Thực hiện do task phát đầu tiên cho các nhiệm vụ tìm kiếm vật phẩm phòng khi trong người nó có sẵn rồi
            //if (_FindTask.TargetType == (int)TaskTypes.Crafting)
            //{
            //    ProcessTask.Process(Global._TCPManager.MySocketListener, pool, client, -1, -1, Int32.Parse(_FindTask.PropsName), TaskTypes.Crafting);
            //}

            //// Nếu vật phẩm là tìm kiếm vật phẩm thì xem trong túi đồ nó đã có vật phẩm này chưa
            //if (_FindTask.TargetType == (int)TaskTypes.GetItemWithSpcecialLine)
            //{
            //    ProcessTask.Process(Global._TCPManager.MySocketListener, pool, client, -1, -1, -1, TaskTypes.GetItemWithSpcecialLine);
            //}

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
        /// Set nhiệm vụ bạo văn đồng cho người chơi
        /// </summary>
        /// <param name="client"></param>
        /// <param name="ForceChange"></param>
        /// <returns></returns>
        public bool GiveTaskArmyDaily(KPlayer client, bool ForceChange = false)
        {
            int TaskID = this.GetTaskDailyArmyCurentTaskID(client);

            if (ForceChange)
            {
                int LevelHientai = client.m_Level;

                

                List<Task> _TotalTask = _TotalTaskData.Values.Where(x => x.MinLevel <= LevelHientai && x.MaxLevel >= LevelHientai).ToList();

                if (_TotalTask.Count == 0)
                {
                    return false;
                }

                int Random = new System.Random().Next(_TotalTask.Count);

                Task SelectTask = _TotalTask[Random];

                //Set nhiệm vụ hiện tại vào
                this.SetTaskDailyArmyCurentTaskID(client, SelectTask.ID);

                return true;
            }
            else
            {
                // lấy ra danh sách nhiệm vụ trên người
                if (client.TaskDataList != null)
                {
                    lock (client.TaskDataList)
                    {
                        if (client.TaskDataList.Count > 0)
                        {
                            foreach (TaskData task in client.TaskDataList)
                            {
                                Task _Task = this.GetTaskTemplate(task.DoingTaskID);
                                {
                                    return false;
                                }
                            }
                        }
                    }

                    // nếu ko làm được thực hiện tìm 1 nhiệm vụ khác phù hợp
                    {
                        int LevelHientai = client.m_Level;

                        List<Task> _TotalTask = _TotalTaskData.Values.Where(x => x.MinLevel <= LevelHientai && x.MaxLevel >= LevelHientai).ToList();

                        if (_TotalTask.Count == 0)
                        {
                            return false;
                        }

                        int Random = new System.Random().Next(_TotalTask.Count);

                        Task SelectTask = _TotalTask[Random];

                        // Lưu vào RECORE
                        this.SetTaskDailyArmyCurentTaskID(client, SelectTask.ID);

                        return true;
                    }
                }
                return false;
            }
        }
    }
}