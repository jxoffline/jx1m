using GameServer.Core.Executor;
using GameServer.KiemThe;
using GameServer.KiemThe.Core.Task;
using GameServer.KiemThe.Entities;
using GameServer.KiemThe.Logic;
using GameServer.Logic;
using GameServer.Server;
using Server.Tools;
using System.Collections.Generic;
using System.Linq;

namespace GameServer.VLTK.Core.GuildManager
{
    //Quản lý nhiệm vụ cho người chơi
    public partial class GuildManager
    {
        // Tổng toàn bộ nhiệm vụ cho bang hội
        public Dictionary<int, Task> _TotalGuildTask = new Dictionary<int, Task>();

        //Set nhiệm vụ loading từ KTMAIN
        public void SetTask(Dictionary<int, Task> _InPut)
        {
            this._TotalGuildTask = _InPut;
        }

        public GuildTask GetGuildTask(int GuildID)
        {
            _TotalGuild.TryGetValue(GuildID, out MiniGuildInfo Find);

            // var Find = _TotalGuild.Where(x => x.GuildId == GuildID).FirstOrDefault();

            if (Find != null)
            {
                return Find.Task;
            }
            else
            {
                return null;
            }
        }

        public void GiveTaskForGuild(int GuildID, bool IsUpdateCount = true)
        {
            int Days = TimeUtil.NowDateTime().DayOfYear;

            int TotalTask = _TotalGuildTask.Count;

            if (TotalTask == 0)
            {
                // Ghi logs ra là đéo có nhiệm vụ nào để trao cho bang này
                LogManager.WriteLog(LogTypes.Task, "NO TASK GIVE FOR GUILDID :" + GuildID);
                return;
            }
            // Lấy ra 1 nhiệm vụ
            int SelectTask = KTGlobal.GetRandomNumber(0, TotalTask - 1);

            // Lấy ra thông tin nhiệm vụ
            Task _outTask = _TotalGuildTask.ElementAt(SelectTask).Value;

            if (_outTask != null)
            {
                _TotalGuild.TryGetValue(GuildID, out MiniGuildInfo Find);

                //var Find = _TotalGuild.Where(x => x.GuildId == GuildID).FirstOrDefault();
                if (Find != null)
                {
                    GuildTask _Task = new GuildTask();
                    _Task.GuildID = GuildID;
                    _Task.DayCreate = Days;

                    if (Find.Task == null)
                    {
                        _Task.TaskCountInDay = 0;
                    }
                    else
                    {
                        if (Find.Task.DayCreate != Days)
                        {
                            _Task.TaskCountInDay = 0;
                        }
                        else
                        {
                            if (IsUpdateCount)
                            {
                                _Task.TaskCountInDay = Find.Task.TaskCountInDay + 1;
                            }
                        }
                    }

                    _Task.TaskID = _outTask.ID;
                    _Task.TaskValue = 0;

                    //Set this task
                    Find.Task = _Task;

                    // Đây chắc chắn khong phải là cập nhật task
                    SendTaskForDB(_Task.GuildID, _Task.TaskID, _Task.TaskValue, _Task.TaskCountInDay, 0);

                    // Gửi thông báo có nhiệm vụ mới tới toàn thành viên
                    KTGlobal.SendGuildChat(GuildID, "Tiếp nhận nhiệm vụ bang hội <b>" + _outTask.Title + "</b>", null, "");
                }
            }
        }

        public bool SendTaskForDB(int GuildID, int TaskID, int Value, int CountInDay, int IsUpdate)
        {
            string Build = GuildID + ":" + TaskID + ":" + Value + ":" + CountInDay + ":" + IsUpdate;

            string[] Pram = Global.SendToDB((int)TCPGameServerCmds.CMD_KT_GUILD_TASKUPDATE, Build, GameManager.LocalServerId);

            if (Pram == null)
            {
                return false;
            }

            if (Pram.Length != 2)
            {
                return false;
            }

            if (Pram[0] == "-1")
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Xử lý task khi có sự kiện chết quái
        /// </summary>
        /// <param name="GuildID"></param>
        /// <param name="client"></param>
        /// <param name="extensionID"></param>
        /// <param name="taskType"></param>
        public void TaskProsecc(int GuildID, KPlayer client, int extensionID, TaskTypes taskType)
        {
            //Check xem nó là kiểu gì
            switch (taskType)
            {
                //SUPPORT
                case TaskTypes.Collect:
                // Giết con gì
                case TaskTypes.KillMonster:

                case TaskTypes.BuyItemInShopGuild:

                case TaskTypes.EnhanceTime:

                case TaskTypes.KillOtherGuildRoleTargetMapcode:

                case TaskTypes.CarriageTotalCount:

                case TaskTypes.KillOtherGuildRole:
                    {
                        ProsecTaskProsecc(GuildID, client, extensionID, taskType);
                        break;
                    }
            }
        }

        public void OnDieTaskProsecc(GameObject attacker, GameObject target)
        {
            if (attacker is KPlayer && target is KPlayer)
            {
                KPlayer kPlayer_Kill = (KPlayer)attacker;
                KPlayer kPlayer_BeKill = (KPlayer)target;

                if (kPlayer_Kill.GuildID > 0 && kPlayer_BeKill.GuildID > 0)
                {
                    if (kPlayer_Kill.GuildID != kPlayer_BeKill.GuildID)
                    {
                        this.TaskProsecc(kPlayer_Kill.GuildID, kPlayer_Kill, -1, TaskTypes.KillOtherGuildRole);
                        this.TaskProsecc(kPlayer_Kill.GuildID, kPlayer_Kill, kPlayer_BeKill.MapCode, TaskTypes.KillOtherGuildRoleTargetMapcode);
                    }
                }
            }
        }

        /// <summary>
        /// Cập nhật tiến độ nếu có nhiệm vụ thu thập
        /// </summary>
        /// <param name="GuildID"></param>
        /// <param name="client"></param>
        /// <param name="extensionID"></param>
        /// <param name="taskType"></param>
        private void ProsecTaskProsecc(int GuildID, KPlayer client, int extensionID, TaskTypes taskType)
        {
            // Nếu thằng này đéo có bagn thì chim cút
            if (client.GuildID <= 0)
            {
                return;
            }

            bool IsNeedCheckUpdate = false;

            _TotalGuild.TryGetValue(GuildID, out MiniGuildInfo Find);

            // Lấy ra cái bang này xem nhiệm vụ đang có là cái đéo j
            //  var Find = _TotalGuild.Where(x => x.GuildId == GuildID).FirstOrDefault();
            // Nếu thật sự có cái bang này
            if (Find != null)
            {
                // Và chắc chắn là trong này nó chưa hoàn thành nhiệm vụ
                if (!Find.IsFinishTaskInDay)
                {
                    GuildTask _Task = Find.Task;
                    // Nếu đây là nhiệm vụ cần phải check
                    // Nếu bang này đéo có nhiệm vụ nào

                    if (_Task == null)
                    {
                        return;
                    }

                    // Nếu bang này đang có nhiệm vụ
                    if (_Task.TaskID != -1)
                    {
                        _TotalGuildTask.TryGetValue(_Task.TaskID, out Task _TaskTmp);

                        if (_TaskTmp != null)
                        {
                            // Nếu đúng kiểu nhiệm vụ cần check
                            if (_TaskTmp.TargetType == (int)taskType)
                            {
                                // Nếu đây là nhiệm vụ giết quái hoặc nhiệm vụ thu thập
                                if (taskType == TaskTypes.KillMonster)
                                {
                                    // Nếu đéo phải con quái cần giết thì return luôn
                                    if (_TaskTmp.TargetNPC != extensionID)
                                    {
                                        return;
                                    }
                                }

                                if (taskType == TaskTypes.Collect)
                                {
                                    // Nếu đéo phải con quái cần giết thì return luôn
                                    if (_TaskTmp.TargetNPC != extensionID)
                                    {
                                        return;
                                    }
                                }
                                if (taskType == TaskTypes.KillOtherGuildRoleTargetMapcode)
                                {
                                    //Nếu không phải bản đồ yêu câu thì thôi
                                    if (_TaskTmp.TargetMapCode != extensionID)
                                    {
                                        return;
                                    }
                                }

                                // Tổng số lượng cần thu thập
                                int ValueRequest = _TaskTmp.TargetNum;

                                // nếu như nhiệu vụ này chưa xong
                                if (ValueRequest > _Task.TaskValue)
                                {
                                    // Tăng giá trị của VALUE lên 1
                                    _Task.TaskValue++;

                                    // Gửi về DB
                                    if (SendTaskForDB(GuildID, _Task.TaskID, _Task.TaskValue, _Task.TaskCountInDay, 1))
                                    {
                                        KTGlobal.SendGuildChat(GuildID, "Nhiệm vụ Bang [" + _TaskTmp.Title + "] tiến độ cập nhật : " + _Task.TaskValue + "/" + ValueRequest + " bởi [" + client.RoleName + "]", null, "");

                                        if (_TaskTmp.Point1 > 0 || _TaskTmp.Point2 > 0)
                                        {
                                            if (_TaskTmp.Point1 > 0)
                                            {
                                                if (KTGlobal.AddMoney(client, _TaskTmp.Point1, MoneyType.GuildMoney, "GUILDQUEST").IsOK)
                                                {
                                                    KTPlayerManager.ShowNotification(client, "Cống hiến bang hội của bạn tăng thêm :" + _TaskTmp.Point1);
                                                }
                                            }
                                            if (_TaskTmp.Point2 > 0)
                                            {
                                                int WeekPointHave = GuildManager.getInstance().GetWeekPoint(client);

                                                if (WeekPointHave == -1)
                                                {
                                                    WeekPointHave = 0;
                                                }

                                                WeekPointHave += _TaskTmp.Point2;

                                                GuildManager.getInstance().SetWeekPoint(client, WeekPointHave);

                                                KTPlayerManager.ShowNotification(client, "Điểm hoạt động tuần của bạn tăng thêm :" + _TaskTmp.Point2);
                                            }
                                        }

                                        // Nếu mà hoàn thành nhiệm vụ bang hội lần này
                                        if (ValueRequest == _Task.TaskValue)
                                        {
                                            // Tăng 1 nhiệm vụ
                                            _Task.TaskCountInDay++;

                                            // Thực hiện trả thưởng

                                            // Cộng Exp cho bang hội
                                            Find.GuildExp += _TaskTmp.Experienceaward;

                                            //Update Exp cho bang
                                            UpdateGuildResource(GuildID, GUILD_RESOURCE.EXP, Find.GuildExp + "");

                                            if (_TaskTmp.Point3 > 0)
                                            {
                                                Find.GuildMoney += _TaskTmp.Point3;

                                                if (UpdateGuildResource(GuildID, GUILD_RESOURCE.GUILD_MONEY, Find.GuildMoney + ""))
                                                {
                                                    KTGlobal.SendGuildChat(GuildID, "Hoàn thành nhiệm vụ [" + _TaskTmp.Title + "] Exp Bang Hôi Gia Tăng : " + _TaskTmp.Experienceaward + "| Quỹ bang tăng thêm :" + _TaskTmp.Point3, null, "");
                                                }
                                            }
                                            else
                                            {
                                                KTGlobal.SendGuildChat(GuildID, "Hoàn thành nhiệm vụ [" + _TaskTmp.Title + "] Exp Bang Hôi Gia Tăng : " + _TaskTmp.Experienceaward, null, "");
                                            }

                                            // Thực hiện giao nhiệm vụ khác
                                            // Nếu còn số lượt có thể làm nhiệm vụ này
                                            if (_Task.TaskCountInDay < _GuildConfig.MaxQuestPerDay)
                                            {
                                                GiveTaskForGuild(GuildID);
                                            }
                                            else
                                            {
                                                Find.IsFinishTaskInDay = true;
                                                KTGlobal.SendGuildChat(GuildID, "Số nhiệm vụ của bang ngày hôm nay đã hết!", null, "");
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Reload task cho 1 bang khi qua ngày hoặc là bang mới tạo
        /// Hoặc là bang đéo có nhiệm vụ nào
        /// </summary>
        /// <param name="Guild"></param>
        public void ReloadTaskOfGuild(int GuildId)
        {
            // Lấy ra xem nay là ngày nào
            int Days = TimeUtil.NowDateTime().DayOfYear;

            _TotalGuild.TryGetValue(GuildId, out MiniGuildInfo Find);

            //  var Find = _TotalGuild.Where(x => x.GuildId == GuildId).FirstOrDefault();
            if (Find != null)
            {
                if (Find.Task == null)
                {
                    GiveTaskForGuild(GuildId);
                }
                else
                {
                    if (Find.Task.DayCreate != Days)
                    {
                        // Give this GuildNew task
                        GiveTaskForGuild(GuildId);
                    }
                    else
                    {
                        // Nếu số nhiệm vụ trong ngày chưa vượt quá 20
                        if (Find.Task.TaskCountInDay < 20)
                        {
                            // Nếu bang này chưa có nhiệm vụ nào thì trao cho nó cái nhiệm vụ khác
                            if (Find.Task.TaskID == -1)
                            {
                                // Nếu như đéo có nhiệm vụ nào
                                GiveTaskForGuild(GuildId);
                            }
                        }
                    }
                }
            }
        }
    }
}