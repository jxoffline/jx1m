using FS.GameEngine.Logic;
using FS.GameEngine.Sprite;
using FS.VLTK.Entities.Config;
using FS.VLTK.Logic;
using FS.VLTK.Network;
using GameServer.VLTK.Utilities;
using Server.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using static FS.VLTK.Entities.Enum;

namespace FS.VLTK
{
    /// <summary>
    /// Các hàm toàn cục dùng trong Kiếm Thế
    /// </summary>
    public static partial class KTGlobal
    {
        #region Nhiệm vụ

        /// <summary>
        /// Danh sách hiệu ứng theo loại nhiệm vụ
        /// </summary>
        private static readonly Dictionary<NPCTaskStates, int> NpcTaskStateEffects = new Dictionary<NPCTaskStates, int>()
        {
            { NPCTaskStates.ToReceive_MainQuest, 1011 },
            { NPCTaskStates.ToReturn_MainQuest, 1012 },
            { NPCTaskStates.ToReceive_SubQuest, 1067 },
            { NPCTaskStates.ToReturn_SubQuest, 1068 },
            { NPCTaskStates.ToReceive_DailyQuest, 1056 },
            { NPCTaskStates.ToReturn_DailyQuest, 1057 },
        };

        /// <summary>
        /// Hiệu ứng nhận nhiệm vụ của nhân vật
        /// </summary>
        private static readonly int RoleReceiveTaskEffect = 1083;

        /// <summary>
        /// Hiệu ứng hoàn thành nhiệm vụ của nhân vật
        /// </summary>
        private static readonly int RoleCompleteTaskEffect = 1085;

        /// <summary>
        /// Hiển thị trạng thái nhiệm vụ tương ứng của NPC
        /// </summary>
        /// <param name="npc"></param>
        /// <param name="state"></param>
        public static void ShowNPCTaskState(GSprite npc, NPCTaskStates state)
        {
            /// Nếu không có NPC
            if (npc == null)
            {
                return;
            }
            /// Nếu không có biểu diễn NPC
            else if (npc.ComponentMonster == null)
            {
                return;
            }
            /// Nếu không phải NPC
            else if (npc.SpriteType != GSpriteTypes.NPC)
            {
                return;
            }

            /// Xóa trạng thái cũ trên gầu
            KTGlobal.HideNPCTaskState(npc);

            /// Cập nhật trạng thái ở Minimap
            npc.ComponentMonster.UpdateMinimapNPCTaskState(state);

            /// Nếu tồn tại hiệu ứng trạng thái tương ứng
            if (KTGlobal.NpcTaskStateEffects.TryGetValue(state, out int effectID))
            {
                /// Thêm hiệu ứng trạng thái tương ứng
                npc.AddBuff(effectID, -1);
            }
        }

        /// <summary>
        /// Xóa trạng thái nhiệm vụ tương ứng của NPC
        /// </summary>
        /// <param name="npc"></param>
        public static void HideNPCTaskState(GSprite npc)
        {
            /// Nếu không có NPC
            if (npc == null)
            {
                return;
            }
            /// Nếu không có biểu diễn NPC
            else if (npc.ComponentMonster == null)
            {
                return;
            }
            /// Nếu không phải NPC
            else if (npc.SpriteType != GSpriteTypes.NPC)
            {
                return;
            }

            /// Duyệt danh sách trạng thái nhiệm vụ
            foreach (KeyValuePair<NPCTaskStates, int> taskType in KTGlobal.NpcTaskStateEffects)
            {
                /// Xóa hiệu ứng trạng thái tương ứng
                npc.ComponentMonster.RemoveEffect(taskType.Value);
            }
        }

        /// <summary>
        /// Thực thi hiệu ứng nhận nhiệm vụ thành công
        /// </summary>
        public static void PlayRoleReceiveQuestEffect()
        {
            Global.Data.Leader.ComponentCharacter.AddEffect(KTGlobal.RoleReceiveTaskEffect, EffectType.CastEffect);
        }

        /// <summary>
        /// Thực thi hiệu ứng hoàn thành nhiệm vụ
        /// </summary>
        public static void PlayRoleCompleteQuestEffect()
        {
            Global.Data.Leader.ComponentCharacter.AddEffect(KTGlobal.RoleCompleteTaskEffect, EffectType.CastEffect);
        }

        /// <summary>
        /// Kiểm tra nhiệm vụ hiện tại đã hoàn thành trước đó chưa
        /// </summary>
        /// <param name="taskID"></param>
        /// <returns></returns>
        public static bool HadTaskBeenCompleted(int taskID)
        {
            /// Thông tin nhiệm vụ tương ứng
            if (!Loader.Loader.Tasks.TryGetValue(taskID, out TaskDataXML taskDataXML))
            {
                return false;
            }
            return Global.Data.CompletedTasks[taskDataXML.TaskClass].Contains(taskDataXML.ID);
        }

        /// <summary>
        /// Tìm nhiệm vụ tương ứng theo ResID của NPC
        /// </summary>
        /// <param name="npcResID"></param>
        /// <returns></returns>
        public static NPCTaskState FindTaskByNPCResID(int npcResID)
        {
            NPCTaskState taskState = Global.Data.RoleData.NPCTaskStateList?.Where(x => x != null && x.NPCID == npcResID).FirstOrDefault();
            return taskState;
        }

        /// <summary>
        /// Trả về thông tin nhiệm vụ tương ứng của Leader
        /// </summary>
        /// <param name="dbID"></param>
        /// <returns></returns>
        public static TaskData GetTaskData(int dbID)
        {
            return Global.Data.RoleData.TaskDataList?.Where(x => x.DbID == dbID).FirstOrDefault();
        }

        /// <summary>
        /// Trả về thông tin nhiệm vụ tương ứng của Leader theo TaskID
        /// </summary>
        /// <param name="taskID"></param>
        /// <returns></returns>
        public static TaskData GetTaskByStaticID(int taskID)
        {
            return Global.Data.RoleData.TaskDataList?.Where(x => x.DoingTaskID == taskID).FirstOrDefault();
        }

        /// <summary>
        /// Kiểm tra nhiệm vụ tương ứng đã hoàn thành chưa
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        public static bool IsQuestCompleted(TaskData task)
        {
            /// Nếu nhiệm vụ không tồn tại thì bỏ qua
            if (!Loader.Loader.Tasks.TryGetValue(task.DoingTaskID, out TaskDataXML taskDataXML))
            {
                return false;
            }

            if (taskDataXML.TargetType == (int) TaskTypes.Talk || taskDataXML.TargetType == (int) TaskTypes.AnswerQuest || taskDataXML.TargetType == (int) TaskTypes.JoinFaction)
            {
                if (task.DoingTaskVal1 >= 1)
                {
                    return true;
                }
            }

            if (taskDataXML.TargetType == (int) TaskTypes.KillMonster || taskDataXML.TargetType == (int) TaskTypes.MonsterSomething || taskDataXML.TargetType == (int) TaskTypes.BuySomething || taskDataXML.TargetType == (int) TaskTypes.UseSomething || taskDataXML.TargetType == (int) TaskTypes.TransferSomething || taskDataXML.TargetType == (int) TaskTypes.GetSomething || taskDataXML.TargetType == (int) TaskTypes.Collect || taskDataXML.TargetType == (int) TaskTypes.Crafting || taskDataXML.TargetType == (int) TaskTypes.Enhance || taskDataXML.TargetType == (int) TaskTypes.JoinActivity)
            {
                int NumberRequest = taskDataXML.TargetNum;

                if (task.DoingTaskVal1 >= NumberRequest)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Đang trong trạng thái thu thập
        /// </summary>
        /// <returns></returns>
        public static bool IsCollecting()
        {
            return PlayZone.Instance.IsProgressBarVisible();
        }

        /// <summary>
        /// Trả về tiền tố loại nhiệm vụ
        /// </summary>
        /// <param name="taskID"></param>
        /// <returns></returns>
        public static string GetTaskPrefix(int taskID)
        {
            if (Loader.Loader.Tasks.TryGetValue(taskID, out TaskDataXML taskDataXML))
            {
                switch (taskDataXML.TaskClass)
                {
                    /// Chính tuyến
                    case 0:
                    {
                        return "[Chính]";
                    }
                    /// Bao Vạn KNB
                    case 2:
                    {
                        return "[Dã Tẩu]";
                    }
                    /// Thương hội
                    case 3:
                    {
                        return "[Thương hội]";
                    }
                    /// Hải tặc
                    case 4:
                    {
                        return "[Sát Thủ]";
                    }
                    /// Phụ tuyến
                    case 5:
                    {
                        return "[Phụ]";
                    }
                    default:
                    {
                        return "[Khác]";
                    }
                }
            }
            return "";
        }

        /// <summary>
        /// Trả về màu nhiệm vụ
        /// </summary>
        /// <param name="taskID"></param>
        /// <returns></returns>
        public static Color GetTaskColorXML(int taskID)
        {
            Color color = default;
            if (Loader.Loader.Tasks.TryGetValue(taskID, out TaskDataXML taskDataXML))
            {
                switch (taskDataXML.TaskClass)
                {
                    /// Chính tuyến
                    case 0:
                    {
                        ColorUtility.TryParseHtmlString("#ffff42", out color);
                        break;
                    }
                    /// Phụ tuyến
                    case 5:
                    {
                        ColorUtility.TryParseHtmlString("#61ddff", out color);
                        break;
                    }
                    default:
                    {
                        ColorUtility.TryParseHtmlString("#7bff42", out color);
                        break;
                    }
                }
            }
            return color;
        }

        /// <summary>
        /// Trả về thông tin nhiệm vụ kế
        /// </summary>
        /// <param name="taskClass"></param>
        /// <returns></returns>
        public static TaskDataXML GetNextTask(int taskClass)
        {
            /// Nhiệm vụ đã hoàn thành gần nhất ở nhóm hiện tại
            int lastTaskID = Global.Data.RoleData.QuestInfo.Where(x => x.TaskClass == taskClass).FirstOrDefault().CurTaskIndex;

            /// Nếu không có nhiệm vụ đã hoàn thành gần nhất ở nhóm hiện tại
            if (!Loader.Loader.Tasks.TryGetValue(lastTaskID, out TaskDataXML previousTaskDataXML))
            {
                return null;
            }

            /// Nhiệm vụ tiếp theo
            int nextTaskID = previousTaskDataXML.NextTask;

            /// Nếu không tồn tại nhiệm vụ tiếp theo
            if (!Loader.Loader.Tasks.TryGetValue(nextTaskID, out TaskDataXML taskDataXML))
            {
                return null;
            }

            /// Thông tin nhiệm vụ
            return taskDataXML;
        }

        /// <summary>
        /// Trả về chuỗi mô tả nhiệm vụ tiếp theo
        /// </summary>
        /// <param name="taskClass"></param>
        /// <param name="nextTask"></param>
        /// <param name="clickEvent"></param>
        /// <returns></returns>
        public static string GetNextTasDetailString(int taskClass, out Action clickEvent)
        {
            StringBuilder builder = new StringBuilder();

            /// Thiết lập sự kiện Click
            clickEvent = null;

            /// Nhiệm vụ tiếp theo
            TaskDataXML taskDataXML = KTGlobal.GetNextTask(taskClass);
            /// Nếu không tồn tại nhiệm vụ tiếp theo
            if (taskDataXML == null)
            {
                goto BREAK;
            }

            /// Cấp độ yêu cầu
            int requireLevelMin = taskDataXML.MinLevel;
            int requireLevelMax = taskDataXML.MaxLevel;
            /// Nếu cấp độ hiện tại vượt quá cấp độ Max
            if (Global.Data.RoleData.Level > requireLevelMax)
            {
                goto BREAK;
            }

            /// ID bản đồ chứa NPC tương ứng
            int mapCode = taskDataXML.SourceMapCode;
            /// ID NPC tương ứng
            int npcID = taskDataXML.SourceNPC;

            /// Tên bản đồ
            string mapName = "";
            /// Nếu là khu vực
            if (mapCode < 0)
            {
                if (Loader.Loader.AutoPaths.MapGroups.TryGetValue(mapCode, out AutoPathXML.MapGroup mapGroup))
                {
                    mapName = mapGroup.Name;
                }
            }
            else
            {
                /// Thông tin bản đồ tương ứng
                if (Loader.Loader.Maps.TryGetValue(mapCode, out Entities.Config.Map mapData))
                {
                    mapName = mapData.Name;
                }
            }
            /// Thông tin NPC tương ứng
            if (!Loader.Loader.ListMonsters.TryGetValue(npcID, out MonsterDataXML npcData))
            {
                goto BREAK;
            }

            /// Nếu không có bản đồ
            if (string.IsNullOrEmpty(mapName))
            {
                builder.AppendFormat("Nhận nhiệm vụ tại <color=#f2e33a>[{0}]</color>", npcData.Name);
            }
            else
            {
                builder.AppendFormat("Nhận nhiệm vụ tại <color=#f2e33a>[{0}]</color> ở <color=#3af23e>{1}</color>.", npcData.Name, mapName);
            }

            /// Cập nhật sự kiện Click
            clickEvent = () =>
            {
                KTGlobal.QuestAutoFindPathToNPC(mapCode, npcID, () =>
                {
                    AutoQuest.Instance.StopAutoQuest();
                    AutoPathManager.Instance.StopAutoPath();
                    GSprite sprite = KTGlobal.FindNearestNPCByResID(npcID);
                    if (sprite == null)
                    {
                        KTGlobal.AddNotification("Không tìm thấy NPC tương ứng!");
                        return;
                    }
                    Global.Data.TargetNpcID = sprite.RoleID;
                    Global.Data.GameScene.NPCClick(sprite);
                });
            };

            /// Nếu cấp hiện tại chưa đủ
            if (Global.Data.RoleData.Level < requireLevelMin)
            {
                builder.AppendLine();
                builder.AppendFormat("<color=red>Yêu cầu cấp độ <color=#f2e33a>{0}</color>.</color>", requireLevelMin);
            }

            BREAK:
            return builder.ToString();
        }

        public static string GetTaskGuildDetailString(TaskData taskData, TaskDataXML taskDataXML, bool isCompleted, out Action clickEvent)
        {
            StringBuilder builder = new StringBuilder();

            clickEvent = null;
            switch (taskDataXML.TargetType)
            {
                case (int) TaskTypes.KillMonster:
                {
                    /// ID bản đồ chứa NPC tương ứng
                    int mapCode = taskDataXML.TargetMapCode;
                    /// ID quái tương ứng
                    int monsterID = taskDataXML.TargetNPC;
                    /// Số lượng
                    int monsterNum = taskDataXML.TargetNum;
                    /// ID NPC trả nhiệm vụ
                    int destNPCID = taskDataXML.DestNPC;
                    /// ID bản đồ NPC trả nhiệm vụ
                    int destMapCode = taskDataXML.DestMapCode;
                    /// Tên bản đồ
                    string mapName = "";
                    /// Nếu là khu vực
                    if (mapCode < 0)
                    {
                        if (Loader.Loader.AutoPaths.MapGroups.TryGetValue(mapCode, out AutoPathXML.MapGroup mapGroup))
                        {
                            mapName = mapGroup.Name;
                        }
                    }
                    else
                    {
                        /// Thông tin bản đồ tương ứng
                        if (Loader.Loader.Maps.TryGetValue(mapCode, out Entities.Config.Map mapData))
                        {
                            mapName = mapData.Name;
                        }
                    }
                    /// Tên bản đồ giao nhiệm vụ
                    string destMapName = "";
                    /// Nếu là khu vực
                    if (destMapCode < 0)
                    {
                        if (Loader.Loader.AutoPaths.MapGroups.TryGetValue(destMapCode, out AutoPathXML.MapGroup mapGroup))
                        {
                            destMapName = mapGroup.Name;
                        }
                    }
                    else
                    {
                        /// Thông tin bản đồ tương ứng
                        if (Loader.Loader.Maps.TryGetValue(destMapCode, out Entities.Config.Map mapData))
                        {
                            destMapName = mapData.Name;
                        }
                    }
                    /// Thông tin quái tương ứng
                    if (!Loader.Loader.ListMonsters.TryGetValue(monsterID, out MonsterDataXML monsterData))
                    {
                        goto BREAK;
                    }

                    /// Tên sự kiện
                    if (string.IsNullOrEmpty(mapName))
                    {
                        if (string.IsNullOrEmpty(destMapName))
                        {
                            if (isCompleted)
                            {
                                builder.AppendFormat("<color=#f2e33a>NHIỆM VỤ ĐÃ HOÀN THÀNH</color>");
                            }
                            else
                            {
                                if (monsterData.ResID.Contains("ani"))
                                {
                                    builder.AppendFormat("Tiêu diệt <color=#3af2ec>{0}</color> con <color=#f2e33a>[{1}]</color>", monsterNum, monsterData.Name);
                                }
                                else
                                {
                                    builder.AppendFormat("Tiêu diệt <color=#3af2ec>{0}</color> tên <color=#f2e33a>[{1}]</color>", monsterNum, monsterData.Name);
                                }
                            }
                        }
                        else
                        {
                            if (isCompleted)
                            {
                                builder.AppendFormat("<color=#f2e33a>NHIỆM VỤ ĐÃ HOÀN THÀNH</color>");
                            }
                            else
                            {
                                if (monsterData.ResID.Contains("ani"))
                                {
                                    builder.AppendFormat("Tiêu diệt <color=#3af2ec>{0}</color> con <color=#f2e33a>[{1}]</color> ở <color=#3af23e>{2}</color>", monsterNum, monsterData.Name, mapName);
                                }
                                else
                                {
                                    builder.AppendFormat("Tiêu diệt <color=#3af2ec>{0}</color> tên <color=#f2e33a>[{1}]</color> ở <color=#3af23e>{2}</color>", monsterNum, monsterData.Name, mapName);
                                }
                            }
                        }
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(destMapName))
                        {
                            if (isCompleted)
                            {
                                builder.AppendFormat("<color=#f2e33a>NHIỆM VỤ ĐÃ HOÀN THÀNH</color>");
                            }
                            else
                            {
                                if (monsterData.ResID.Contains("ani"))
                                {
                                    builder.AppendFormat("Tiêu diệt <color=#3af2ec>{0}</color> con <color=#f2e33a>[{1}]</color> ở <color=#3af23e>{2}</color>", monsterNum, monsterData.Name, mapName);
                                }
                                else
                                {
                                    builder.AppendFormat("Tiêu diệt <color=#3af2ec>{0}</color> tên <color=#f2e33a>[{1}]</color> ở <color=#3af23e>{2}</color>", monsterNum, monsterData.Name, mapName);
                                }
                            }
                        }
                        else
                        {
                            if (isCompleted)
                            {
                                builder.AppendFormat("<color=#f2e33a>NHIỆM VỤ ĐÃ HOÀN THÀNH</color>");
                            }
                            else
                            {
                                if (monsterData.ResID.Contains("ani"))
                                {
                                    builder.AppendFormat("Tiêu diệt <color=#3af2ec>{0}</color> con <color=#f2e33a>[{1}]</color> ở <color=#3af23e>{2}</color>", monsterNum, monsterData.Name, mapName);
                                }
                                else
                                {
                                    builder.AppendFormat("Tiêu diệt <color=#3af2ec>{0}</color> tên <color=#f2e33a>[{1}]</color> ở <color=#3af23e>{2}</color>", monsterNum, monsterData.Name, mapName);
                                }
                            }
                        }
                    }
                    if (!isCompleted)
                    {
                        clickEvent = () =>
                        {
                            KTGlobal.QuestAutoFindPathToMonster(mapCode, monsterID, () =>
                            {
                                AutoQuest.Instance.Task = taskData;
                                AutoQuest.Instance.Done = () =>
                                {
                                    KTGlobal.QuestAutoFindPathToNPC(destMapCode, destNPCID, () =>
                                    {
                                        AutoQuest.Instance.StopAutoQuest();
                                        AutoPathManager.Instance.StopAutoPath();
                                        GSprite sprite = KTGlobal.FindNearestNPCByResID(destNPCID);
                                        if (sprite == null)
                                        {
                                            KTGlobal.AddNotification("Không tìm thấy NPC tương ứng!");
                                            return;
                                        }
                                        Global.Data.TargetNpcID = sprite.RoleID;
                                        Global.Data.GameScene.NPCClick(sprite);
                                    });
                                };
                                AutoQuest.Instance.StartAutoQuest();
                            });
                        };
                    }
                    else
                    {
                        clickEvent = () =>
                        {
                            KTGlobal.QuestAutoFindPathToNPC(destMapCode, destNPCID, () =>
                            {
                                AutoQuest.Instance.StopAutoQuest();
                                AutoPathManager.Instance.StopAutoPath();
                                GSprite sprite = KTGlobal.FindNearestNPCByResID(destNPCID);
                                if (sprite == null)
                                {
                                    KTGlobal.AddNotification("Không tìm thấy NPC tương ứng!");
                                    return;
                                }
                                Global.Data.TargetNpcID = sprite.RoleID;
                                Global.Data.GameScene.NPCClick(sprite);
                            });
                        };
                    }
                    break;
                }
                case (int) TaskTypes.KillOtherGuildRole:
                {
                    int TargetNum = taskDataXML.TargetNum;

                    builder.AppendFormat("Hạ sát thành viên ở bang hội khác tổng cộng <color=#f2e33a>[{0}]</color> lần.", TargetNum);

                    break;
                }

                case (int) TaskTypes.KillOtherGuildRoleTargetMapcode:
                {
                    int mapCode = taskDataXML.TargetMapCode;
                    string mapName = "";
                    if (mapCode < 0)
                    {
                        if (Loader.Loader.AutoPaths.MapGroups.TryGetValue(mapCode, out AutoPathXML.MapGroup mapGroup))
                        {
                            mapName = mapGroup.Name;
                        }
                    }
                    else
                    {
                        /// Thông tin bản đồ tương ứng
                        if (Loader.Loader.Maps.TryGetValue(mapCode, out Entities.Config.Map mapData))
                        {
                            mapName = mapData.Name;
                        }
                    }

                    int TargetNum = taskDataXML.TargetNum;

                    builder.AppendFormat("Hạ sát thành viên ở bang hội khác tổng cộng <color=#f2e33a>[{0}]</color> lần.Tại bản đồ <color=#3af23e>{1}</color>", TargetNum, mapName);

                    break;
                }
                // Mua vật phẩm ở shop bang
                case (int) TaskTypes.BuyItemInShopGuild:
                {
                    int TargetNum = taskDataXML.TargetNum;

                    string ItemName = KTGlobal.QuestFormatItems(taskDataXML.PropsName);

                    builder.AppendFormat("Mua vật phẩm <color=#f2e33a>[{0}]</color> với số lượng <color=#3af23e>{1}</color> tại của hang BANG HỘI", ItemName, TargetNum);

                    break;
                }
                case (int) TaskTypes.CarriageTotalCount:
                {
                    int TargetNum = taskDataXML.TargetNum;

                    builder.AppendFormat("Tham gia vận tiêu tổng cộng <color=#f2e33a>[{0}]</color> lần.", TargetNum);

                    break;
                }

                case (int) TaskTypes.EnhanceTime:
                {
                    int TargetNum = taskDataXML.TargetNum;

                    builder.AppendFormat("Cường hóa vật phẩm tổng cộng <color=#f2e33a>[{0}]</color> lần.", TargetNum);

                    break;
                }
                case (int) TaskTypes.Collect:
                {
                    /// ID bản đồ chứa NPC tương ứng
                    int mapCode = taskDataXML.TargetMapCode;
                    /// ID điểm thu thập tương ứng
                    int growPointID = taskDataXML.TargetNPC;

                    /// Số lượng
                    int itemNum = taskDataXML.TargetNum;
                    /// Danh sách vật phẩm cần lấy
                    string itemsString = taskDataXML.PropsName;
                    /// Tên bản đồ
                    string mapName = "";
                    /// Nếu là khu vực
                    if (mapCode < 0)
                    {
                        if (Loader.Loader.AutoPaths.MapGroups.TryGetValue(mapCode, out AutoPathXML.MapGroup mapGroup))
                        {
                            mapName = mapGroup.Name;
                        }
                    }
                    else
                    {
                        /// Thông tin bản đồ tương ứng
                        if (Loader.Loader.Maps.TryGetValue(mapCode, out Entities.Config.Map mapData))
                        {
                            mapName = mapData.Name;
                        }
                    }

                    /// Thông tin điểm thu thập tương ứng
                    if (!Loader.Loader.ListMonsters.TryGetValue(growPointID, out MonsterDataXML growPointData))
                    {
                        goto BREAK;
                    }

                    builder.AppendFormat("Thu thập <color=#2effdc>{0} cái</color> {1} ở <color=#3af23e>{2}</color>", itemNum, KTGlobal.QuestFormatItems(itemsString), mapName);

                    if (!isCompleted)
                    {
                        clickEvent = () =>
                        {
                            KTGlobal.QuestAutoFindPathToGrowPoint(mapCode, growPointID, () =>
                            {
                                AutoQuest.Instance.Task = taskData;
                                AutoQuest.Instance.Done = () =>
                                {
                                };
                                AutoQuest.Instance.StartAutoQuest();
                            });
                        };
                    }

                    break;
                }
            }
            BREAK:
            return builder.ToString();
        }

        /// <summary>
        /// Trả về chuỗi mô tả nhiệm vụ tương ứng
        /// </summary>
        /// <param name="taskData"></param>
        /// <param name="taskDataXML"></param>
        /// <param name="isCompleted"></param>
        /// <param name="clickEvent"></param>
        /// <returns></returns>
        public static string GetTaskDetailString(TaskData taskData, TaskDataXML taskDataXML, bool isCompleted, out Action clickEvent)
        {
            StringBuilder builder = new StringBuilder();

            /// Thiết lập sự kiện Click
            clickEvent = null;

            switch (taskDataXML.TargetType)
            {
                case (int) TaskTypes.Talk:
                {
                    /// ID bản đồ chứa NPC tương ứng
                    int mapCode = taskDataXML.DestMapCode;
                    /// ID NPC tương ứng
                    int npcID = taskDataXML.DestNPC;

                    /// Tên bản đồ
                    string mapName = "";
                    /// Nếu là khu vực
                    if (mapCode < 0)
                    {
                        if (Loader.Loader.AutoPaths.MapGroups.TryGetValue(mapCode, out AutoPathXML.MapGroup mapGroup))
                        {
                            mapName = mapGroup.Name;
                        }
                    }
                    else
                    {
                        /// Thông tin bản đồ tương ứng
                        if (Loader.Loader.Maps.TryGetValue(mapCode, out Entities.Config.Map mapData))
                        {
                            mapName = mapData.Name;
                        }
                    }
                    /// Thông tin NPC tương ứng
                    if (!Loader.Loader.ListMonsters.TryGetValue(npcID, out MonsterDataXML npcData))
                    {
                        goto BREAK;
                    }

                    /// Tên sự kiện
                    if (string.IsNullOrEmpty(mapName))
                    {
                        builder.AppendFormat("Tìm gặp <color=#f2e33a>[{0}]</color>.", npcData.Name);
                    }
                    else
                    {
                        builder.AppendFormat("Tìm gặp <color=#f2e33a>[{0}]</color> ở <color=#3af23e>{1}</color>.", npcData.Name, mapName);
                    }
                    clickEvent = () =>
                    {
                        KTGlobal.QuestAutoFindPathToNPC(mapCode, npcID, () =>
                        {
                            AutoQuest.Instance.StopAutoQuest();
                            AutoPathManager.Instance.StopAutoPath();
                            GSprite sprite = KTGlobal.FindNearestNPCByResID(npcID);
                            if (sprite == null)
                            {
                                KTGlobal.AddNotification("Không tìm thấy NPC tương ứng!");
                                return;
                            }
                            Global.Data.TargetNpcID = sprite.RoleID;
                            Global.Data.GameScene.NPCClick(sprite);
                        });
                    };
                    break;
                }
                case (int) TaskTypes.AnswerQuest:
                {
                    /// ID bản đồ chứa NPC tương ứng
                    int mapCode = taskDataXML.DestMapCode;
                    /// ID NPC tương ứng
                    int npcID = taskDataXML.DestNPC;

                    /// Tên bản đồ
                    string mapName = "";
                    /// Nếu là khu vực
                    if (mapCode < 0)
                    {
                        if (Loader.Loader.AutoPaths.MapGroups.TryGetValue(mapCode, out AutoPathXML.MapGroup mapGroup))
                        {
                            mapName = mapGroup.Name;
                        }
                    }
                    else
                    {
                        /// Thông tin bản đồ tương ứng
                        if (Loader.Loader.Maps.TryGetValue(mapCode, out Entities.Config.Map mapData))
                        {
                            mapName = mapData.Name;
                        }
                    }
                    /// Thông tin NPC tương ứng
                    if (!Loader.Loader.ListMonsters.TryGetValue(npcID, out MonsterDataXML npcData))
                    {
                        goto BREAK;
                    }

                    /// Tên sự kiện
                    if (string.IsNullOrEmpty(mapName))
                    {
                        builder.AppendFormat("Trả lời câu hỏi của <color=#f2e33a>[{0}]</color>.", npcData.Name);
                    }
                    else
                    {
                        builder.AppendFormat("Trả lời câu hỏi của <color=#f2e33a>[{0}]</color> ở <color=#3af23e>{1}</color>.", npcData.Name, mapName);
                    }
                    clickEvent = () =>
                    {
                        KTGlobal.QuestAutoFindPathToNPC(mapCode, npcID, () =>
                        {
                            AutoQuest.Instance.StopAutoQuest();
                            AutoPathManager.Instance.StopAutoPath();
                            GSprite sprite = KTGlobal.FindNearestNPCByResID(npcID);
                            if (sprite == null)
                            {
                                KTGlobal.AddNotification("Không tìm thấy NPC tương ứng!");
                                return;
                            }
                            Global.Data.TargetNpcID = sprite.RoleID;
                            Global.Data.GameScene.NPCClick(sprite);
                        });
                    };
                    break;
                }
                case (int) TaskTypes.KillMonster:
                {
                    /// ID bản đồ chứa NPC tương ứng
                    int mapCode = taskDataXML.TargetMapCode;
                    /// ID quái tương ứng
                    int monsterID = taskDataXML.TargetNPC;
                    /// Số lượng
                    int monsterNum = taskDataXML.TargetNum;
                    /// ID NPC trả nhiệm vụ
                    int destNPCID = taskDataXML.DestNPC;
                    /// ID bản đồ NPC trả nhiệm vụ
                    int destMapCode = taskDataXML.DestMapCode;
                    /// Tên bản đồ
                    string mapName = "";
                    /// Nếu là khu vực
                    if (mapCode < 0)
                    {
                        if (Loader.Loader.AutoPaths.MapGroups.TryGetValue(mapCode, out AutoPathXML.MapGroup mapGroup))
                        {
                            mapName = mapGroup.Name;
                        }
                    }
                    else
                    {
                        /// Thông tin bản đồ tương ứng
                        if (Loader.Loader.Maps.TryGetValue(mapCode, out Entities.Config.Map mapData))
                        {
                            mapName = mapData.Name;
                        }
                    }
                    /// Tên bản đồ giao nhiệm vụ
                    string destMapName = "";
                    /// Nếu là khu vực
                    if (destMapCode < 0)
                    {
                        if (Loader.Loader.AutoPaths.MapGroups.TryGetValue(destMapCode, out AutoPathXML.MapGroup mapGroup))
                        {
                            destMapName = mapGroup.Name;
                        }
                    }
                    else
                    {
                        /// Thông tin bản đồ tương ứng
                        if (Loader.Loader.Maps.TryGetValue(destMapCode, out Entities.Config.Map mapData))
                        {
                            destMapName = mapData.Name;
                        }
                    }
                    /// Thông tin quái tương ứng
                    if (!Loader.Loader.ListMonsters.TryGetValue(monsterID, out MonsterDataXML monsterData))
                    {
                        goto BREAK;
                    }
                    /// Thông tin NPC tương ứng
                    if (!Loader.Loader.ListMonsters.TryGetValue(destNPCID, out MonsterDataXML npcData))
                    {
                        goto BREAK;
                    }

                    /// Tên sự kiện
                    if (string.IsNullOrEmpty(mapName))
                    {
                        if (string.IsNullOrEmpty(destMapName))
                        {
                            if (isCompleted)
                            {
                                builder.AppendFormat("Báo cáo với <color=#f2e33a>[{0}]</color>", npcData.Name);
                            }
                            else
                            {
                                if (monsterData.ResID.Contains("ani"))
                                {
                                    builder.AppendFormat("Tiêu diệt <color=#3af2ec>{0}</color> con <color=#f2e33a>[{1}]</color>", monsterNum, monsterData.Name);
                                }
                                else
                                {
                                    builder.AppendFormat("Tiêu diệt <color=#3af2ec>{0}</color> tên <color=#f2e33a>[{1}]</color>", monsterNum, monsterData.Name);
                                }
                            }
                        }
                        else
                        {
                            if (isCompleted)
                            {
                                builder.AppendFormat("Báo cáo với <color=#f2e33a>[{0}]</color> ở <color=#3af23e>{1}</color>", npcData.Name, destMapName);
                            }
                            else
                            {
                                if (monsterData.ResID.Contains("ani"))
                                {
                                    builder.AppendFormat("Tiêu diệt <color=#3af2ec>{0}</color> con <color=#f2e33a>[{1}]</color> ở <color=#3af23e>{2}</color>", monsterNum, monsterData.Name, mapName);
                                }
                                else
                                {
                                    builder.AppendFormat("Tiêu diệt <color=#3af2ec>{0}</color> tên <color=#f2e33a>[{1}]</color> ở <color=#3af23e>{2}</color>", monsterNum, monsterData.Name, mapName);
                                }
                            }
                        }
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(destMapName))
                        {
                            if (isCompleted)
                            {
                                builder.AppendFormat("Báo cáo với <color=#f2e33a>[{0}]</color>", npcData.Name);
                            }
                            else
                            {
                                if (monsterData.ResID.Contains("ani"))
                                {
                                    builder.AppendFormat("Tiêu diệt <color=#3af2ec>{0}</color> con <color=#f2e33a>[{1}]</color> ở <color=#3af23e>{2}</color>", monsterNum, monsterData.Name, mapName);
                                }
                                else
                                {
                                    builder.AppendFormat("Tiêu diệt <color=#3af2ec>{0}</color> tên <color=#f2e33a>[{1}]</color> ở <color=#3af23e>{2}</color>", monsterNum, monsterData.Name, mapName);
                                }
                            }
                        }
                        else
                        {
                            if (isCompleted)
                            {
                                builder.AppendFormat("Báo cáo với <color=#f2e33a>[{0}]</color> ở <color=#3af23e>{1}</color>", npcData.Name, destMapName);
                            }
                            else
                            {
                                if (monsterData.ResID.Contains("ani"))
                                {
                                    builder.AppendFormat("Tiêu diệt <color=#3af2ec>{0}</color> con <color=#f2e33a>[{1}]</color> ở <color=#3af23e>{2}</color>", monsterNum, monsterData.Name, mapName);
                                }
                                else
                                {
                                    builder.AppendFormat("Tiêu diệt <color=#3af2ec>{0}</color> tên <color=#f2e33a>[{1}]</color> ở <color=#3af23e>{2}</color>", monsterNum, monsterData.Name, mapName);
                                }
                            }
                        }
                    }
                    if (!isCompleted)
                    {
                        clickEvent = () =>
                        {
                            KTGlobal.QuestAutoFindPathToMonster(mapCode, monsterID, () =>
                            {
                                AutoQuest.Instance.Task = taskData;
                                AutoQuest.Instance.Done = () =>
                                {
                                    KTGlobal.QuestAutoFindPathToNPC(destMapCode, destNPCID, () =>
                                    {
                                        AutoQuest.Instance.StopAutoQuest();
                                        AutoPathManager.Instance.StopAutoPath();
                                        GSprite sprite = KTGlobal.FindNearestNPCByResID(destNPCID);
                                        if (sprite == null)
                                        {
                                            KTGlobal.AddNotification("Không tìm thấy NPC tương ứng!");
                                            return;
                                        }
                                        Global.Data.TargetNpcID = sprite.RoleID;
                                        Global.Data.GameScene.NPCClick(sprite);
                                    });
                                };
                                AutoQuest.Instance.StartAutoQuest();
                            });
                        };
                    }
                    else
                    {
                        clickEvent = () =>
                        {
                            KTGlobal.QuestAutoFindPathToNPC(destMapCode, destNPCID, () =>
                            {
                                AutoQuest.Instance.StopAutoQuest();
                                AutoPathManager.Instance.StopAutoPath();
                                GSprite sprite = KTGlobal.FindNearestNPCByResID(destNPCID);
                                if (sprite == null)
                                {
                                    KTGlobal.AddNotification("Không tìm thấy NPC tương ứng!");
                                    return;
                                }
                                Global.Data.TargetNpcID = sprite.RoleID;
                                Global.Data.GameScene.NPCClick(sprite);
                            });
                        };
                    }
                    break;
                }
                case (int) TaskTypes.MonsterSomething:
                {
                    /// ID bản đồ chứa NPC tương ứng
                    int mapCode = taskDataXML.TargetMapCode;
                    /// ID quái tương ứng
                    int monsterID = taskDataXML.TargetNPC;
                    /// ID NPC trả nhiệm vụ
                    int destNPCID = taskDataXML.DestNPC;
                    /// ID bản đồ NPC trả nhiệm vụ
                    int destMapCode = taskDataXML.DestMapCode;
                    /// Số lượng
                    int itemNum = taskDataXML.TargetNum;
                    /// Danh sách vật phẩm cần lấy
                    string itemsString = taskDataXML.PropsName;
                    /// Tên bản đồ
                    string mapName = "";
                    /// Nếu là khu vực
                    if (mapCode < 0)
                    {
                        if (Loader.Loader.AutoPaths.MapGroups.TryGetValue(mapCode, out AutoPathXML.MapGroup mapGroup))
                        {
                            mapName = mapGroup.Name;
                        }
                    }
                    else
                    {
                        /// Thông tin bản đồ tương ứng
                        if (Loader.Loader.Maps.TryGetValue(mapCode, out Entities.Config.Map mapData))
                        {
                            mapName = mapData.Name;
                        }
                    }
                    /// Tên bản đồ giao nhiệm vụ
                    string destMapName = "";
                    /// Nếu là khu vực
                    if (destMapCode < 0)
                    {
                        if (Loader.Loader.AutoPaths.MapGroups.TryGetValue(destMapCode, out AutoPathXML.MapGroup mapGroup))
                        {
                            destMapName = mapGroup.Name;
                        }
                    }
                    else
                    {
                        /// Thông tin bản đồ tương ứng
                        if (Loader.Loader.Maps.TryGetValue(destMapCode, out Entities.Config.Map mapData))
                        {
                            destMapName = mapData.Name;
                        }
                    }
                    /// Thông tin quái tương ứng
                    if (!Loader.Loader.ListMonsters.TryGetValue(monsterID, out MonsterDataXML monsterData))
                    {
                        goto BREAK;
                    }
                    /// Thông tin NPC tương ứng
                    if (!Loader.Loader.ListMonsters.TryGetValue(destNPCID, out MonsterDataXML npcData))
                    {
                        goto BREAK;
                    }

                    /// Tên sự kiện
                    if (string.IsNullOrEmpty(mapName))
                    {
                        if (string.IsNullOrEmpty(destMapName))
                        {
                            if (isCompleted)
                            {
                                builder.AppendFormat("Đem giao cho <color=#f2e33a>[{0}]</color>", npcData.Name);
                            }
                            else
                            {
                                builder.AppendFormat("Tiêu diệt <color=#f2e33a>[{0}]</color>, đoạt lấy <color=#2effdc>{1} cái</color> {2}", monsterData.Name, itemNum, KTGlobal.QuestFormatItems(itemsString));
                            }
                        }
                        else
                        {
                            if (isCompleted)
                            {
                                builder.AppendFormat("Đem giao cho <color=#f2e33a>[{0}]</color> ở <color=#3af23e>{1}</color>", npcData.Name, destMapName);
                            }
                            else
                            {
                                builder.AppendFormat("Tiêu diệt <color=#f2e33a>[{0}]</color>, đoạt lấy <color=#2effdc>{1} cái</color> {2}", monsterData.Name, itemNum, KTGlobal.QuestFormatItems(itemsString));
                            }
                        }
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(destMapName))
                        {
                            if (isCompleted)
                            {
                                builder.AppendFormat("Đem giao cho <color=#f2e33a>[{0}]</color>", npcData.Name);
                            }
                            else
                            {
                                builder.AppendFormat("Tiêu diệt <color=#f2e33a>[{0}]</color> ở <color=#3af23e>{1}</color>, đoạt lấy <color=#2effdc>{2} cái</color> {3}", monsterData.Name, mapName, itemNum, KTGlobal.QuestFormatItems(itemsString));
                            }
                        }
                        else
                        {
                            if (isCompleted)
                            {
                                builder.AppendFormat("Đem giao cho <color=#f2e33a>[{0}]</color> ở <color=#3af23e>{1}</color>", npcData.Name, destMapName);
                            }
                            else
                            {
                                builder.AppendFormat("Tiêu diệt <color=#f2e33a>[{0}]</color> ở <color=#3af23e>{1}</color>, đoạt lấy <color=#2effdc>{2} cái</color> {3}", monsterData.Name, mapName, itemNum, KTGlobal.QuestFormatItems(itemsString));
                            }
                        }
                    }
                    if (!isCompleted)
                    {
                        clickEvent = () =>
                        {
                            KTGlobal.QuestAutoFindPathToMonster(mapCode, monsterID, () =>
                            {
                                AutoQuest.Instance.Task = taskData;
                                AutoQuest.Instance.Done = () =>
                                {
                                    KTGlobal.QuestAutoFindPathToNPC(destMapCode, destNPCID, () =>
                                    {
                                        AutoQuest.Instance.StopAutoQuest();
                                        AutoPathManager.Instance.StopAutoPath();
                                        GSprite sprite = KTGlobal.FindNearestNPCByResID(destNPCID);
                                        if (sprite == null)
                                        {
                                            KTGlobal.AddNotification("Không tìm thấy NPC tương ứng!");
                                            return;
                                        }
                                        Global.Data.TargetNpcID = sprite.RoleID;
                                        Global.Data.GameScene.NPCClick(sprite);
                                    });
                                };
                                AutoQuest.Instance.StartAutoQuest();
                            });
                        };
                    }
                    else
                    {
                        clickEvent = () =>
                        {
                            KTGlobal.QuestAutoFindPathToNPC(destMapCode, destNPCID, () =>
                            {
                                AutoQuest.Instance.StopAutoQuest();
                                AutoPathManager.Instance.StopAutoPath();
                                GSprite sprite = KTGlobal.FindNearestNPCByResID(destNPCID);
                                if (sprite == null)
                                {
                                    KTGlobal.AddNotification("Không tìm thấy NPC tương ứng!");
                                    return;
                                }
                                Global.Data.TargetNpcID = sprite.RoleID;
                                Global.Data.GameScene.NPCClick(sprite);
                            });
                        };
                    }
                    break;
                }
                case (int) TaskTypes.TransferSomething:
                {
                    /// ID bản đồ chứa NPC tương ứng
                    int mapCode = taskDataXML.DestMapCode;
                    /// ID NPC tương ứng
                    int npcID = taskDataXML.DestNPC;
                    /// Danh sách vật phẩm tương ứng
                    string itemsString = taskDataXML.PropsName;

                    /// Tên bản đồ
                    string mapName = "";
                    /// Nếu là khu vực
                    if (mapCode < 0)
                    {
                        if (Loader.Loader.AutoPaths.MapGroups.TryGetValue(mapCode, out AutoPathXML.MapGroup mapGroup))
                        {
                            mapName = mapGroup.Name;
                        }
                    }
                    else
                    {
                        /// Thông tin bản đồ tương ứng
                        if (Loader.Loader.Maps.TryGetValue(mapCode, out Entities.Config.Map mapData))
                        {
                            mapName = mapData.Name;
                        }
                    }
                    /// Thông tin NPC tương ứng
                    if (!Loader.Loader.ListMonsters.TryGetValue(npcID, out MonsterDataXML npcData))
                    {
                        goto BREAK;
                    }

                    /// Tên sự kiện
                    if (string.IsNullOrEmpty(mapName))
                    {
                        builder.AppendFormat("Đem {0} tới cho <color=#f2e33a>[{1}]</color>.", KTGlobal.QuestFormatItems(itemsString), npcData.Name);
                    }
                    else
                    {
                        builder.AppendFormat("Đem {0} tới cho <color=#f2e33a>[{1}]</color> ở <color=#3af23e>{2}</color>.", KTGlobal.QuestFormatItems(itemsString), npcData.Name, mapName);
                    }
                    clickEvent = () =>
                    {
                        KTGlobal.QuestAutoFindPathToNPC(mapCode, npcID, () =>
                        {
                            AutoQuest.Instance.StopAutoQuest();
                            AutoPathManager.Instance.StopAutoPath();
                            GSprite sprite = KTGlobal.FindNearestNPCByResID(npcID);
                            if (sprite == null)
                            {
                                KTGlobal.AddNotification("Không tìm thấy NPC tương ứng!");
                                return;
                            }
                            Global.Data.TargetNpcID = sprite.RoleID;
                            Global.Data.GameScene.NPCClick(sprite);
                        });
                    };
                    break;
                }
                case (int) TaskTypes.UseSomething:
                {
                    /// ID bản đồ chứa NPC tương ứng
                    int mapCode = taskDataXML.TargetMapCode;
                    /// Danh sách vật phẩm tương ứng
                    string itemsString = taskDataXML.PropsName;
                    /// Tọa độ
                    string positionString = taskDataXML.TargetPos;
                    string[] posStr = positionString.Split('|');
                    int posX = int.Parse(posStr[0]);
                    int posY = int.Parse(posStr[1]);
                    /// ID NPC trả nhiệm vụ
                    int destNPCID = taskDataXML.DestNPC;
                    /// ID bản đồ trả nhiệm vụ
                    int destMapCode = taskDataXML.DestMapCode;

                    /// Tên bản đồ
                    string mapName = "";
                    /// Nếu là khu vực
                    if (mapCode < 0)
                    {
                        if (Loader.Loader.AutoPaths.MapGroups.TryGetValue(mapCode, out AutoPathXML.MapGroup mapGroup))
                        {
                            mapName = mapGroup.Name;
                        }
                    }
                    else
                    {
                        /// Thông tin bản đồ tương ứng
                        if (Loader.Loader.Maps.TryGetValue(mapCode, out Entities.Config.Map mapData))
                        {
                            mapName = mapData.Name;
                        }
                    }
                    /// Tên bản đồ trả
                    string destMapName = "";
                    /// Nếu là khu vực
                    if (destMapCode < 0)
                    {
                        if (Loader.Loader.AutoPaths.MapGroups.TryGetValue(destMapCode, out AutoPathXML.MapGroup mapGroup))
                        {
                            destMapName = mapGroup.Name;
                        }
                    }
                    else
                    {
                        /// Thông tin bản đồ tương ứng
                        if (Loader.Loader.Maps.TryGetValue(destMapCode, out Entities.Config.Map mapData))
                        {
                            destMapName = mapData.Name;
                        }
                    }
                    /// Tên NPC trả
                    string destNpcName = "";
                    /// Thông tin NPC tương ứng
                    if (Loader.Loader.ListMonsters.TryGetValue(destNPCID, out MonsterDataXML _npcData))
                    {
                        /// Nếu là nhóm bản đồ
                        if (Loader.Loader.AutoPaths.MapGroups.TryGetValue(destMapCode, out AutoPathXML.MapGroup mapGroup))
                        {
                            /// Nếu trùng với bản đồ hiện tại
                            if (mapGroup.Maps.Contains(Global.Data.GameScene.MapCode))
                            {
                                destMapCode = Global.Data.GameScene.MapCode;
                            }
                            /// Nếu khác bản đồ hiện tại thì lấy bản đồ đầu tiên làm chuẩn
                            else
                            {
                                /// Truyền tống phù tương ứng
                                GoodsData teleportItem = Global.Data.RoleData.GoodsDataList?.Where(x => Loader.Loader.AutoPaths.TeleportItems.ContainsKey(x.GoodsID)).FirstOrDefault();
                                /// Nếu có truyền tống phù
                                if (teleportItem != null)
                                {
                                    List<int> paths = AutoPathManager.Instance.FindPathWithoutTeleportItem(Global.Data.GameScene.MapCode, mapGroup.Maps);
                                    /// Nếu có kết quả
                                    if (paths.Count > 0)
                                    {
                                        destMapCode = paths.LastOrDefault();
                                    }
                                    else
                                    {
                                        destMapCode = mapGroup.Maps.FirstOrDefault();
                                    }
                                }
                                /// Nếu có truyền tống phù
                                else
                                {
                                    List<int> paths = AutoPathManager.Instance.FindPathWithTeleportItem(teleportItem.GoodsID, Global.Data.GameScene.MapCode, mapGroup.Maps);
                                    /// Nếu có kết quả
                                    if (paths.Count > 0)
                                    {
                                        destMapCode = paths.LastOrDefault();
                                    }
                                    else
                                    {
                                        destMapCode = mapGroup.Maps.FirstOrDefault();
                                    }
                                }
                            }
                        }

                        /// Nếu trong cùng bản đồ
                        if (Global.Data.GameScene.MapCode == destMapCode)
                        {
                            /// Nếu không có tọa độ thì lấy vị trí của Dest NPC
                            if (posX == -1 || posY == -1)
                            {
                                if (Global.Data.GameScene.CurrentMapData.NpcListByID.TryGetValue(destNPCID, out List<Drawing.Point> points))
                                {
                                    if (points.Count > 0)
                                    {
                                        /// Chọn gần nhất
                                        Drawing.Point point = points.MinBy(x => Vector2.Distance(Global.Data.Leader.PositionInVector2, new Vector2(x.X, x.Y)));

                                        posX = point.X;
                                        posY = point.Y;
                                    }
                                }
                            }

                            Vector2 gridPos = KTGlobal.WorldPositionToGridPosition(new Vector2(posX, posY));
                            posX = (int) gridPos.x;
                            posY = (int) gridPos.y;
                        }
                        /// Nếu khác bản đồ
                        else
                        {
                            //posX = -1;
                            //posY = -1;
                        }

                        destNpcName = _npcData.Name;
                    }

                    /// Tọa độ thực
                    Vector2 worldPos = KTGlobal.GridPositionToWorldPosition(new Vector2(posX, posY));

                    /// Nếu đã hoàn thành
                    if (isCompleted)
                    {
                        if (string.IsNullOrEmpty(destMapName))
                        {
                            builder.AppendFormat("Báo cáo với <color=#f2e33a>[{0}]</color>", destNpcName);
                        }
                        else
                        {
                            builder.AppendFormat("Báo cáo với <color=#f2e33a>[{0}]</color> ở <color=#3af23e>{1}</color>.", destNpcName, destMapName);
                        }

                        clickEvent = () =>
                        {
                            KTGlobal.QuestAutoFindPathToNPC(destMapCode, destNPCID, () =>
                            {
                                AutoQuest.Instance.StopAutoQuest();
                                AutoPathManager.Instance.StopAutoPath();
                                GSprite sprite = KTGlobal.FindNearestNPCByResID(destNPCID);
                                if (sprite == null)
                                {
                                    KTGlobal.AddNotification("Không tìm thấy NPC tương ứng!");
                                    return;
                                }
                                Global.Data.TargetNpcID = sprite.RoleID;
                                Global.Data.GameScene.NPCClick(sprite);
                            });
                        };
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(mapName))
                        {
                            /// Nếu cùng bản đồ
                            if (Global.Data.GameScene.MapCode == destMapCode)
                            {
                                builder.AppendFormat("Sử dụng <color=#f2e33a>{0}</color> tại <color=#3af23e>[({1}, {2})]</color>.", KTGlobal.QuestFormatItems(itemsString), posX, posY);
                                clickEvent = () =>
                                {
                                    KTGlobal.QuestAutoFindPath(Global.Data.RoleData.MapCode, (int) worldPos.x, (int) worldPos.y, () =>
                                    {
                                        AutoQuest.Instance.StopAutoQuest();
                                        AutoPathManager.Instance.StopAutoPath();
                                    });
                                };
                            }
                            /// Nếu khác bản đồ
                            else
                            {
                                builder.AppendFormat("Sử dụng <color=#f2e33a>{0}</color> ở <color=#3af23e>{1}</color>.", KTGlobal.QuestFormatItems(itemsString), mapName);
                                clickEvent = () =>
                                {
                                    KTGlobal.QuestAutoFindPath(Global.Data.RoleData.MapCode, (int) worldPos.x, (int) worldPos.y, () =>
                                    {
                                        AutoQuest.Instance.StopAutoQuest();
                                        AutoPathManager.Instance.StopAutoPath();
                                    });
                                };
                            }
                        }
                        else
                        {
                            /// Nếu cùng bản đồ
                            if (Global.Data.GameScene.MapCode == destMapCode)
                            {
                                builder.AppendFormat("Sử dụng <color=#f2e33a>{0}</color> tại <color=#3af23e>[{1} ({2}, {3})]</color>.", KTGlobal.QuestFormatItems(itemsString), mapName, posX, posY);
                                clickEvent = () =>
                                {
                                    KTGlobal.QuestAutoFindPathToMap(mapCode, () =>
                                    {
                                        AutoQuest.Instance.StopAutoQuest();
                                        AutoPathManager.Instance.StopAutoPath();

                                        KTGlobal.QuestAutoFindPath(mapCode, (int) worldPos.x, (int) worldPos.y, () =>
                                        {
                                        });
                                    });
                                };
                            }
                            /// Nếu khác bản đồ
                            else
                            {
                                builder.AppendFormat("Sử dụng <color=#f2e33a>{0}</color> ở <color=#3af23e>{1}</color>.", KTGlobal.QuestFormatItems(itemsString), mapName);
                                clickEvent = () =>
                                {
                                    KTGlobal.QuestAutoFindPathToMap(mapCode, () =>
                                    {
                                        AutoQuest.Instance.StopAutoQuest();
                                        AutoPathManager.Instance.StopAutoPath();

                                        /// Nếu không có tọa độ thì lấy vị trí của Dest NPC
                                        if (posX == -1 || posY == -1)
                                        {
                                            if (Global.Data.GameScene.CurrentMapData.NpcListByID.TryGetValue(destNPCID, out List<Drawing.Point> points))
                                            {
                                                if (points.Count > 0)
                                                {
                                                    /// Chọn gần nhất
                                                    Drawing.Point point = points.MinBy(x => Vector2.Distance(Global.Data.Leader.PositionInVector2, new Vector2(x.X, x.Y)));
                                                    Vector2 gridPos = KTGlobal.WorldPositionToGridPosition(new Vector2(point.X, point.Y));
                                                    posX = (int) gridPos.x;
                                                    posY = (int) gridPos.y;
                                                }
                                            }
                                        }

                                        /// Tọa độ thực
                                        Vector2 worldPos = KTGlobal.GridPositionToWorldPosition(new Vector2(posX, posY));

                                        KTGlobal.QuestAutoFindPath(mapCode, (int) worldPos.x, (int) worldPos.y, () =>
                                        {
                                        });
                                    });
                                };
                            }
                        }
                    }

                    break;
                }
                case (int) TaskTypes.GetSomething:
                {
                    /// ID bản đồ chứa NPC tương ứng
                    int mapCode = taskDataXML.DestMapCode;
                    /// ID NPC tương ứng
                    int npcID = taskDataXML.DestNPC;
                    /// Danh sách vật phẩm tương ứng
                    string itemsString = taskDataXML.PropsName;

                    /// Tên bản đồ
                    string mapName = "";
                    /// Nếu là khu vực
                    if (mapCode < 0)
                    {
                        if (Loader.Loader.AutoPaths.MapGroups.TryGetValue(mapCode, out AutoPathXML.MapGroup mapGroup))
                        {
                            mapName = mapGroup.Name;
                        }
                    }
                    else
                    {
                        /// Thông tin bản đồ tương ứng
                        if (Loader.Loader.Maps.TryGetValue(mapCode, out Entities.Config.Map mapData))
                        {
                            mapName = mapData.Name;
                        }
                    }
                    /// Thông tin NPC tương ứng
                    if (!Loader.Loader.ListMonsters.TryGetValue(npcID, out MonsterDataXML npcData))
                    {
                        goto BREAK;
                    }

                    /// Tên sự kiện
                    if (string.IsNullOrEmpty(mapName))
                    {
                        builder.AppendFormat("Nhận {0} từ <color=#f2e33a>[{1}]</color>.", KTGlobal.QuestFormatItems(itemsString), npcData.Name);
                    }
                    else
                    {
                        builder.AppendFormat("Nhận {0} từ <color=#f2e33a>[{1}]</color> ở <color=#3af23e>{2}</color>.", KTGlobal.QuestFormatItems(itemsString), npcData.Name, mapName);
                    }
                    clickEvent = () =>
                    {
                        KTGlobal.QuestAutoFindPathToNPC(mapCode, npcID, () =>
                        {
                            AutoQuest.Instance.StopAutoQuest();
                            AutoPathManager.Instance.StopAutoPath();
                            GSprite sprite = KTGlobal.FindNearestNPCByResID(npcID);
                            if (sprite == null)
                            {
                                KTGlobal.AddNotification("Không tìm thấy NPC tương ứng!");
                                return;
                            }
                            Global.Data.TargetNpcID = sprite.RoleID;
                            Global.Data.GameScene.NPCClick(sprite);
                        });
                    };
                    break;
                }
                case (int) TaskTypes.BuySomething:
                {
                    /// ID bản đồ chứa NPC tương ứng
                    int mapCode = taskDataXML.TargetMapCode;
                    /// ID NPC tương ứng
                    int npcID = taskDataXML.TargetNPC;
                    /// ID NPC trả nhiệm vụ
                    int destNPCID = taskDataXML.DestNPC;
                    /// ID bản đồ trả nhiệm vụ
                    int destMapCode = taskDataXML.DestMapCode;
                    /// Danh sách vật phẩm tương ứng
                    string itemsString = taskDataXML.PropsName;

                    /// Tên bản đồ
                    string mapName = "";
                    /// Nếu là khu vực
                    if (mapCode < 0)
                    {
                        if (Loader.Loader.AutoPaths.MapGroups.TryGetValue(mapCode, out AutoPathXML.MapGroup mapGroup))
                        {
                            mapName = mapGroup.Name;
                        }
                    }
                    else
                    {
                        /// Thông tin bản đồ tương ứng
                        if (Loader.Loader.Maps.TryGetValue(mapCode, out Entities.Config.Map mapData))
                        {
                            mapName = mapData.Name;
                        }
                    }
                    /// Tên bản đồ trả
                    string destMapName = "";
                    /// Nếu là khu vực
                    if (destMapCode < 0)
                    {
                        if (Loader.Loader.AutoPaths.MapGroups.TryGetValue(destMapCode, out AutoPathXML.MapGroup mapGroup))
                        {
                            destMapName = mapGroup.Name;
                        }
                    }
                    else
                    {
                        /// Thông tin bản đồ tương ứng
                        if (Loader.Loader.Maps.TryGetValue(destMapCode, out Entities.Config.Map mapData))
                        {
                            destMapName = mapData.Name;
                        }
                    }
                    /// Tên NPC
                    string npcName = "";
                    /// Thông tin NPC tương ứng
                    if (Loader.Loader.ListMonsters.TryGetValue(npcID, out MonsterDataXML npcData))
                    {
                        npcName = npcData.Name;
                    }
                    /// Tên NPC trả
                    string destNpcName = "";
                    /// Thông tin NPC tương ứng
                    if (Loader.Loader.ListMonsters.TryGetValue(destNPCID, out MonsterDataXML _npcData))
                    {
                        destNpcName = _npcData.Name;
                    }

                    /// Nếu đã hoàn thành
                    if (isCompleted)
                    {
                        if (string.IsNullOrEmpty(destMapName))
                        {
                            builder.AppendFormat("Báo cáo với <color=#f2e33a>[{0}]</color>", destNpcName);
                        }
                        else
                        {
                            builder.AppendFormat("Báo cáo với <color=#f2e33a>[{0}]</color> ở <color=#3af23e>{1}</color>.", destNpcName, destMapName);
                        }

                        clickEvent = () =>
                        {
                            KTGlobal.QuestAutoFindPathToNPC(destMapCode, destNPCID, () =>
                            {
                                AutoQuest.Instance.StopAutoQuest();
                                AutoPathManager.Instance.StopAutoPath();
                                GSprite sprite = KTGlobal.FindNearestNPCByResID(destNPCID);
                                if (sprite == null)
                                {
                                    KTGlobal.AddNotification("Không tìm thấy NPC tương ứng!");
                                    return;
                                }
                                Global.Data.TargetNpcID = sprite.RoleID;
                                Global.Data.GameScene.NPCClick(sprite);
                            });
                        };
                    }
                    /// Nếu chưa hoàn thành
                    else
                    {
                        if (string.IsNullOrEmpty(mapName))
                        {
                            if (string.IsNullOrEmpty(npcName))
                            {
                                builder.AppendFormat("Mua {0}.", KTGlobal.QuestFormatItems(itemsString));
                            }
                            else
                            {
                                builder.AppendFormat("Mua {0} từ <color=#f2e33a>[{1}]</color>.", KTGlobal.QuestFormatItems(itemsString), npcName);
                                clickEvent = () =>
                                {
                                    KTGlobal.QuestAutoFindPathToNPC(destMapCode, npcID, () =>
                                    {
                                        AutoQuest.Instance.StopAutoQuest();
                                        AutoPathManager.Instance.StopAutoPath();
                                        GSprite sprite = KTGlobal.FindNearestNPCByResID(npcID);
                                        if (sprite == null)
                                        {
                                            KTGlobal.AddNotification("Không tìm thấy NPC tương ứng!");
                                            return;
                                        }
                                        Global.Data.TargetNpcID = sprite.RoleID;
                                        Global.Data.GameScene.NPCClick(sprite);
                                    });
                                };
                            }
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(npcName))
                            {
                                builder.AppendFormat("Mua {0} ở <color=#3af23e>{1}</color>.", KTGlobal.QuestFormatItems(itemsString), mapName);
                                clickEvent = () =>
                                {
                                    KTGlobal.QuestAutoFindPathToMap(mapCode, () =>
                                    {
                                        AutoQuest.Instance.StopAutoQuest();
                                        AutoPathManager.Instance.StopAutoPath();
                                    });
                                };
                            }
                            else
                            {
                                builder.AppendFormat("Mua {0} từ <color=#f2e33a>[{1}]</color> ở <color=#3af23e>{2}</color>.", KTGlobal.QuestFormatItems(itemsString), npcName, mapName);
                                clickEvent = () =>
                                {
                                    KTGlobal.QuestAutoFindPathToNPC(destMapCode, npcID, () =>
                                    {
                                        AutoQuest.Instance.StopAutoQuest();
                                        AutoPathManager.Instance.StopAutoPath();
                                        GSprite sprite = KTGlobal.FindNearestNPCByResID(npcID);
                                        if (sprite == null)
                                        {
                                            KTGlobal.AddNotification("Không tìm thấy NPC tương ứng!");
                                            return;
                                        }
                                        Global.Data.TargetNpcID = sprite.RoleID;
                                        Global.Data.GameScene.NPCClick(sprite);
                                    });
                                };
                            }
                        }
                    }

                    break;
                }
                case (int) TaskTypes.Collect:
                {
                    /// ID bản đồ chứa NPC tương ứng
                    int mapCode = taskDataXML.TargetMapCode;
                    /// ID điểm thu thập tương ứng
                    int growPointID = taskDataXML.TargetNPC;
                    /// ID NPC trả nhiệm vụ
                    int destNPCID = taskDataXML.DestNPC;
                    /// ID bản đồ NPC trả nhiệm vụ
                    int destMapCode = taskDataXML.DestMapCode;
                    /// Số lượng
                    int itemNum = taskDataXML.TargetNum;
                    /// Danh sách vật phẩm cần lấy
                    string itemsString = taskDataXML.PropsName;
                    /// Tên bản đồ
                    string mapName = "";
                    /// Nếu là khu vực
                    if (mapCode < 0)
                    {
                        if (Loader.Loader.AutoPaths.MapGroups.TryGetValue(mapCode, out AutoPathXML.MapGroup mapGroup))
                        {
                            mapName = mapGroup.Name;
                        }
                    }
                    else
                    {
                        /// Thông tin bản đồ tương ứng
                        if (Loader.Loader.Maps.TryGetValue(mapCode, out Entities.Config.Map mapData))
                        {
                            mapName = mapData.Name;
                        }
                    }
                    /// Tên bản đồ giao nhiệm vụ
                    string destMapName = "";
                    /// Nếu là khu vực
                    if (destMapCode < 0)
                    {
                        if (Loader.Loader.AutoPaths.MapGroups.TryGetValue(destMapCode, out AutoPathXML.MapGroup mapGroup))
                        {
                            destMapName = mapGroup.Name;
                        }
                    }
                    else
                    {
                        /// Thông tin bản đồ tương ứng
                        if (Loader.Loader.Maps.TryGetValue(destMapCode, out Entities.Config.Map mapData))
                        {
                            destMapName = mapData.Name;
                        }
                    }
                    /// Thông tin điểm thu thập tương ứng
                    if (!Loader.Loader.ListMonsters.TryGetValue(growPointID, out MonsterDataXML growPointData))
                    {
                        goto BREAK;
                    }
                    /// Thông tin NPC tương ứng
                    if (!Loader.Loader.ListMonsters.TryGetValue(destNPCID, out MonsterDataXML npcData))
                    {
                        goto BREAK;
                    }

                    /// Tên sự kiện
                    if (string.IsNullOrEmpty(mapName))
                    {
                        if (string.IsNullOrEmpty(destMapName))
                        {
                            if (isCompleted)
                            {
                                builder.AppendFormat("Đem giao cho <color=#f2e33a>[{0}]</color>", npcData.Name);
                            }
                            else
                            {
                                builder.AppendFormat("Thu thập <color=#2effdc>{0} cái</color> {1}", itemNum, KTGlobal.QuestFormatItems(itemsString));
                            }
                        }
                        else
                        {
                            if (isCompleted)
                            {
                                builder.AppendFormat("Đem giao cho <color=#f2e33a>[{0}]</color> ở <color=#3af23e>{1}</color>", npcData.Name, destMapName);
                            }
                            else
                            {
                                builder.AppendFormat("Thu thập <color=#2effdc>{0} cái</color> {1}", itemNum, KTGlobal.QuestFormatItems(itemsString));
                            }
                        }
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(destMapName))
                        {
                            if (isCompleted)
                            {
                                builder.AppendFormat("Đem giao cho <color=#f2e33a>[{0}]</color>", npcData.Name);
                            }
                            else
                            {
                                builder.AppendFormat("Thu thập <color=#2effdc>{0} cái</color> {1} ở <color=#3af23e>{2}</color>", itemNum, KTGlobal.QuestFormatItems(itemsString), mapName);
                            }
                        }
                        else
                        {
                            if (isCompleted)
                            {
                                builder.AppendFormat("Đem giao cho <color=#f2e33a>[{0}]</color> ở <color=#3af23e>{1}</color>", npcData.Name, destMapName);
                            }
                            else
                            {
                                builder.AppendFormat("Thu thập <color=#2effdc>{0} cái</color> {1} ở <color=#3af23e>{2}</color>", itemNum, KTGlobal.QuestFormatItems(itemsString), mapName);
                            }
                        }
                    }
                    if (!isCompleted)
                    {
                        clickEvent = () =>
                        {
                            KTGlobal.QuestAutoFindPathToGrowPoint(mapCode, growPointID, () =>
                            {
                                AutoQuest.Instance.Task = taskData;
                                AutoQuest.Instance.Done = () =>
                                {
                                    KTGlobal.QuestAutoFindPathToNPC(destMapCode, destNPCID, () =>
                                    {
                                        AutoQuest.Instance.StopAutoQuest();
                                        AutoPathManager.Instance.StopAutoPath();
                                        GSprite sprite = KTGlobal.FindNearestNPCByResID(destNPCID);
                                        if (sprite == null)
                                        {
                                            KTGlobal.AddNotification("Không tìm thấy NPC tương ứng!");
                                            return;
                                        }
                                        Global.Data.TargetNpcID = sprite.RoleID;
                                        Global.Data.GameScene.NPCClick(sprite);
                                    });
                                };
                                AutoQuest.Instance.StartAutoQuest();
                            });
                        };
                    }
                    else
                    {
                        clickEvent = () =>
                        {
                            KTGlobal.QuestAutoFindPathToNPC(destMapCode, destNPCID, () =>
                            {
                                AutoQuest.Instance.StopAutoQuest();
                                AutoPathManager.Instance.StopAutoPath();
                                GSprite sprite = KTGlobal.FindNearestNPCByResID(destNPCID);
                                if (sprite == null)
                                {
                                    KTGlobal.AddNotification("Không tìm thấy NPC tương ứng!");
                                    return;
                                }
                                Global.Data.TargetNpcID = sprite.RoleID;
                                Global.Data.GameScene.NPCClick(sprite);
                            });
                        };
                    }
                    break;
                }
                case (int) TaskTypes.JoinFaction:
                {
                    /// ID NPC trả nhiệm vụ
                    int destNPCID = taskDataXML.DestNPC;
                    /// ID bản đồ NPC trả nhiệm vụ
                    int destMapCode = taskDataXML.DestMapCode;

                    /// Tên bản đồ giao nhiệm vụ
                    string destMapName = "";
                    /// Nếu là khu vực
                    if (destMapCode < 0)
                    {
                        if (Loader.Loader.AutoPaths.MapGroups.TryGetValue(destMapCode, out AutoPathXML.MapGroup mapGroup))
                        {
                            destMapName = mapGroup.Name;
                        }
                    }
                    else
                    {
                        /// Thông tin bản đồ tương ứng
                        if (Loader.Loader.Maps.TryGetValue(destMapCode, out Entities.Config.Map mapData))
                        {
                            destMapName = mapData.Name;
                        }
                    }
                    /// Thông tin NPC tương ứng
                    if (!Loader.Loader.ListMonsters.TryGetValue(destNPCID, out MonsterDataXML npcData))
                    {
                        goto BREAK;
                    }

                    if (!isCompleted)
                    {
                        builder.AppendFormat("Gia nhập <color=#f2e33a>một môn phái bất kỳ</color>");
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(destMapName))
                        {
                            builder.AppendFormat("Báo cáo kết quả với <color=#f2e33a>[{0}]</color>", npcData.Name);
                        }
                        else
                        {
                            builder.AppendFormat("Báo cáo kết quả với <color=#f2e33a>[{0}]</color> ở <color=#3af23e>{1}</color>", npcData.Name, destMapName);
                        }

                        clickEvent = () =>
                        {
                            KTGlobal.QuestAutoFindPathToNPC(destMapCode, destNPCID, () =>
                            {
                                AutoQuest.Instance.StopAutoQuest();
                                AutoPathManager.Instance.StopAutoPath();
                                GSprite sprite = KTGlobal.FindNearestNPCByResID(destNPCID);
                                if (sprite == null)
                                {
                                    KTGlobal.AddNotification("Không tìm thấy NPC tương ứng!");
                                    return;
                                }
                                Global.Data.TargetNpcID = sprite.RoleID;
                                Global.Data.GameScene.NPCClick(sprite);
                            });
                        };
                    }

                    break;
                }
                case (int) TaskTypes.Crafting:
                {
                    /// ID NPC trả nhiệm vụ
                    int destNPCID = taskDataXML.DestNPC;
                    /// ID bản đồ NPC trả nhiệm vụ
                    int destMapCode = taskDataXML.DestMapCode;
                    /// Số lượng
                    int itemNum = taskDataXML.TargetNum;
                    /// Danh sách vật phẩm cần lấy
                    string itemsString = taskDataXML.PropsName;

                    /// Tên bản đồ giao nhiệm vụ
                    string destMapName = "";
                    /// Nếu là khu vực
                    if (destMapCode < 0)
                    {
                        if (Loader.Loader.AutoPaths.MapGroups.TryGetValue(destMapCode, out AutoPathXML.MapGroup mapGroup))
                        {
                            destMapName = mapGroup.Name;
                        }
                    }
                    else
                    {
                        /// Thông tin bản đồ tương ứng
                        if (Loader.Loader.Maps.TryGetValue(destMapCode, out Entities.Config.Map mapData))
                        {
                            destMapName = mapData.Name;
                        }
                    }
                    /// Thông tin NPC tương ứng
                    if (!Loader.Loader.ListMonsters.TryGetValue(destNPCID, out MonsterDataXML npcData))
                    {
                        goto BREAK;
                    }

                    if (!isCompleted)
                    {
                        builder.AppendFormat("Chế tạo <color=#2effdc>{0} cái</color> {1}.", itemNum, KTGlobal.QuestFormatItems(itemsString));
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(destMapName))
                        {
                            builder.AppendFormat("Báo cáo với <color=#f2e33a>[{0}]</color>", npcData.Name);
                        }
                        else
                        {
                            builder.AppendFormat("Báo cáo với <color=#f2e33a>[{0}]</color> ở <color=#3af23e>{1}</color>", npcData.Name, destMapName);
                        }

                        clickEvent = () =>
                        {
                            KTGlobal.QuestAutoFindPathToNPC(destMapCode, destNPCID, () =>
                            {
                                AutoQuest.Instance.StopAutoQuest();
                                AutoPathManager.Instance.StopAutoPath();
                                GSprite sprite = KTGlobal.FindNearestNPCByResID(destNPCID);
                                if (sprite == null)
                                {
                                    KTGlobal.AddNotification("Không tìm thấy NPC tương ứng!");
                                    return;
                                }
                                Global.Data.TargetNpcID = sprite.RoleID;
                                Global.Data.GameScene.NPCClick(sprite);
                            });
                        };
                    }

                    break;
                }
                case (int) TaskTypes.Enhance:
                {
                    /// ID NPC trả nhiệm vụ
                    int destNPCID = taskDataXML.DestNPC;
                    /// ID bản đồ NPC trả nhiệm vụ
                    int destMapCode = taskDataXML.DestMapCode;
                    /// Cấp
                    int itemLevel = taskDataXML.TargetNum;
                    /// Danh sách vật phẩm cần lấy
                    string itemsString = taskDataXML.PropsName;

                    /// Tên bản đồ giao nhiệm vụ
                    string destMapName = "";
                    /// Nếu là khu vực
                    if (destMapCode < 0)
                    {
                        if (Loader.Loader.AutoPaths.MapGroups.TryGetValue(destMapCode, out AutoPathXML.MapGroup mapGroup))
                        {
                            destMapName = mapGroup.Name;
                        }
                    }
                    else
                    {
                        /// Thông tin bản đồ tương ứng
                        if (Loader.Loader.Maps.TryGetValue(destMapCode, out Entities.Config.Map mapData))
                        {
                            destMapName = mapData.Name;
                        }
                    }
                    /// Thông tin NPC tương ứng
                    if (!Loader.Loader.ListMonsters.TryGetValue(destNPCID, out MonsterDataXML npcData))
                    {
                        goto BREAK;
                    }

                    if (!isCompleted)
                    {
                        builder.AppendFormat("Cường hóa {0} lên cấp <color=#2effdc>{1}</color>.", KTGlobal.QuestFormatItems(itemsString), itemLevel);
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(destMapName))
                        {
                            builder.AppendFormat("Báo cáo với <color=#f2e33a>[{0}]</color>", npcData.Name);
                        }
                        else
                        {
                            builder.AppendFormat("Báo cáo với <color=#f2e33a>[{0}]</color> ở <color=#3af23e>{1}</color>", npcData.Name, destMapName);
                        }

                        clickEvent = () =>
                        {
                            KTGlobal.QuestAutoFindPathToNPC(destMapCode, destNPCID, () =>
                            {
                                AutoQuest.Instance.StopAutoQuest();
                                AutoPathManager.Instance.StopAutoPath();
                                GSprite sprite = KTGlobal.FindNearestNPCByResID(destNPCID);
                                if (sprite == null)
                                {
                                    KTGlobal.AddNotification("Không tìm thấy NPC tương ứng!");
                                    return;
                                }
                                Global.Data.TargetNpcID = sprite.RoleID;
                                Global.Data.GameScene.NPCClick(sprite);
                            });
                        };
                    }

                    break;
                }
                case (int) TaskTypes.JoinActivity:
                {
                    /// ID NPC trả nhiệm vụ
                    int destNPCID = taskDataXML.DestNPC;
                    /// ID bản đồ NPC trả nhiệm vụ
                    int destMapCode = taskDataXML.DestMapCode;
                    /// TODO

                    /// Tên bản đồ giao nhiệm vụ
                    string destMapName = "";
                    /// Nếu là khu vực
                    if (destMapCode < 0)
                    {
                        if (Loader.Loader.AutoPaths.MapGroups.TryGetValue(destMapCode, out AutoPathXML.MapGroup mapGroup))
                        {
                            destMapName = mapGroup.Name;
                        }
                    }
                    else
                    {
                        /// Thông tin bản đồ tương ứng
                        if (Loader.Loader.Maps.TryGetValue(destMapCode, out Entities.Config.Map mapData))
                        {
                            destMapName = mapData.Name;
                        }
                    }
                    /// Thông tin NPC tương ứng
                    if (!Loader.Loader.ListMonsters.TryGetValue(destNPCID, out MonsterDataXML npcData))
                    {
                        goto BREAK;
                    }

                    //if (!isCompleted)
                    //{
                    //    builder.AppendFormat("Cường hóa {0} lên cấp <color=#2effdc>{1}</color>.", KTGlobal.QuestFormatItems(itemsString), itemLevel);
                    //}
                    //else
                    //{
                    //    if (string.IsNullOrEmpty(destMapName))
                    //    {
                    //        builder.AppendFormat("Báo cáo với <color=#f2e33a>[{0}]</color>", npcData.Name);
                    //    }
                    //    else
                    //    {
                    //        builder.AppendFormat("Báo cáo với <color=#f2e33a>[{0}]</color> ở <color=#3af23e>{1}</color>", npcData.Name, destMapName);
                    //    }

                    //    clickEvent = () => {
                    //        KTGlobal.QuestAutoFindPathToNPC(destMapCode, destNPCID, () => {
                    //            AutoQuest.Instance.StopAutoQuest();
                    //            AutoPathManager.Instance.StopAutoPath();
                    //            GSprite sprite = KTGlobal.FindNearestNPCByResID(destNPCID);
                    //            if (sprite == null)
                    //            {
                    //                KTGlobal.AddNotification("Không tìm thấy NPC tương ứng!");
                    //                return;
                    //            }
                    //            Global.Data.TargetNpcID = sprite.RoleID;
                    //            Global.Data.GameScene.NPCClick(sprite);
                    //        });
                    //    };
                    //}

                    break;
                }
                // Tìm vật phẩm gì đó của giã tẩu
                case (int) TaskTypes.GetItemWithSpcecialLine:
                {
                    /// ID NPC trả nhiệm vụ
                    int destNPCID = taskDataXML.DestNPC;
                    /// ID bản đồ NPC trả nhiệm vụ
                    int destMapCode = taskDataXML.DestMapCode;

                    /// Danh sách vật phẩm cần lấy
                    string itemsString = taskDataXML.PropsName;

                    /// Tên bản đồ giao nhiệm vụ
                    string destMapName = "";
                    /// Nếu là khu vực
                    if (destMapCode < 0)
                    {
                        if (Loader.Loader.AutoPaths.MapGroups.TryGetValue(destMapCode, out AutoPathXML.MapGroup mapGroup))
                        {
                            destMapName = mapGroup.Name;
                        }
                    }
                    else
                    {
                        /// Thông tin bản đồ tương ứng
                        if (Loader.Loader.Maps.TryGetValue(destMapCode, out Entities.Config.Map mapData))
                        {
                            destMapName = mapData.Name;
                        }
                    }

                    /// Thông tin NPC tương ứng
                    if (!Loader.Loader.ListMonsters.TryGetValue(destNPCID, out MonsterDataXML npcData))
                    {
                        goto BREAK;
                    }

                    // Nếu vật phẩm chưa hoàn thành
                    if (!isCompleted)
                    {
                        //string BUILD = txtloaivatpham.SelectedIndex + "|" + (int)MyStatus + "|" + typenguhanh.SelectedIndex + "|" + ((Property)(txtdongmuontim.SelectedItem)).ID + "|" + txtvaluline.Text;

                        string[] Pram = itemsString.Split('|');

                        int ItemType = Int32.Parse(Pram[0]);
                        int ItemCatagory = Int32.Parse(Pram[1]);
                        int Series = Int32.Parse(Pram[2]);
                        int SymboyID = Int32.Parse(Pram[3]);
                        int SymboyValue = Int32.Parse(Pram[4]);

                        if (ItemType == 0 || ItemType == 1)
                        {
                            string Catagory = KTGlobal.GetWeaponKind(ItemCatagory);

                            string LINEINFO = "";

                            if (PropertyDefine.PropertiesByID.TryGetValue(SymboyID, out PropertyDefine.Property property))
                            {
                                if (property.Description.Contains(':'))
                                {
                                    // Lấy vế đầu tiên
                                    LINEINFO = "<color=green>" + property.Description.Split(':')[0] + "</color> tối thiểu <color=red>" + SymboyValue + "</color>";
                                }
                            }

                            builder.AppendFormat("Thu thập 1 cái <color=#2effdc>" + Catagory + "</color> và có dòng " + LINEINFO);
                        }
                        else
                        {
                            KE_ITEM_EQUIP_DETAILTYPE itemType = KTGlobal.GetEquipType(ItemType);

                            string Catagory = KTGlobal.GetEquipTypeString(itemType);

                            string LINEINFO = "";

                            if (PropertyDefine.PropertiesByID.TryGetValue(SymboyID, out PropertyDefine.Property property))
                            {
                                if (property.Description.Contains(':'))
                                {
                                    // Lấy vế đầu tiên
                                    LINEINFO = "<color=green>" + property.Description.Split(':')[0] + "</color> tối thiểu <color=red>" + SymboyValue + "</color>";
                                }
                            }

                            builder.AppendFormat("Thu thập 1 cái <color=#2effdc>" + Catagory + "</color> và có dòng " + LINEINFO);
                        }
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(destMapName))
                        {
                            builder.AppendFormat("Giao cho <color=#f2e33a>[{0}]</color>", npcData.Name);
                        }
                        else
                        {
                            builder.AppendFormat("Giao cho <color=#f2e33a>[{0}]</color> ở <color=#3af23e>{1}</color>", npcData.Name, destMapName);
                        }

                        clickEvent = () =>
                        {
                            KTGlobal.QuestAutoFindPathToNPC(destMapCode, destNPCID, () =>
                            {
                                AutoQuest.Instance.StopAutoQuest();
                                AutoPathManager.Instance.StopAutoPath();
                                GSprite sprite = KTGlobal.FindNearestNPCByResID(destNPCID);
                                if (sprite == null)
                                {
                                    KTGlobal.AddNotification("Không tìm thấy NPC tương ứng!");
                                    return;
                                }
                                Global.Data.TargetNpcID = sprite.RoleID;
                                Global.Data.GameScene.NPCClick(sprite);
                            });
                        };
                    }

                    break;
                }
            }

            BREAK:
            return builder.ToString();
        }

        /// <summary>
        /// Trả về chuỗi mô tả nhiệm vụ tương ứng
        /// </summary>
        /// <param name="taskData"></param>
        /// <param name="clickEvent"></param>
        /// <returns></returns>
        public static string GetTaskDetailString(TaskData taskData, out Action clickEvent)
        {
            string result = "";

            /// Thiết lập sự kiện Click
            clickEvent = null;

            /// Lấy thông tin nhiệm vụ tương ứng
            if (Loader.Loader.Tasks.TryGetValue(taskData.DoingTaskID, out TaskDataXML taskDataXML))
            {
                result = KTGlobal.GetTaskDetailString(taskData, taskDataXML, KTGlobal.IsQuestCompleted(taskData), out clickEvent);
            }

            return result;
        }

        /// <summary>
        /// Trả về tên các vật phẩm tương ứng của nhiệm vụ
        /// </summary>
        /// <param name="itemsString"></param>
        /// <returns></returns>
        private static string QuestFormatItems(string itemsString)
        {
            /// Tên các vật phẩm tương ứng
            List<string> itemsName = new List<string>();
            /// Duyệt danh sách vật phẩm tương ứng
            foreach (string itemString in itemsString.Split(','))
            {
                try
                {
                    int itemID = int.Parse(itemString);
                    /// Tìm vật phẩm tương ứng trong hệ thống
                    if (Loader.Loader.Items.TryGetValue(itemID, out ItemData itemData))
                    {
                        itemsName.Add(string.Format("<color=#fffc2e>[{0}]</color>", itemData.Name));
                    }
                }
                catch (Exception) { }
            }
            /// Chuỗi mô tả vật phẩm tương ứng cần lấy
            string result = string.Join(", ", itemsName);
            /// Trả về kết quả
            return result;
        }

        #endregion Nhiệm vụ

        #region Auto tìm đường nhiệm vụ

        /// <summary>
        /// Kiểm tra có cần tìm đường đến bản đồ khác không
        /// </summary>
        /// <param name="mapCode"></param>
        /// <returns></returns>
        private static bool IsNeedFindPathToOtherMaps(int mapCode)
        {
            /// Nếu là nhóm bản đồ
            if (mapCode < 0 && Loader.Loader.AutoPaths.MapGroups.TryGetValue(mapCode, out AutoPathXML.MapGroup mapGroup))
            {
                return !mapGroup.Maps.Contains(Global.Data.RoleData.MapCode);
            }
            /// Nếu là bản đồ thường
            else if (mapCode > 0)
            {
                return mapCode != Global.Data.RoleData.MapCode;
            }
            /// Trường hợp khác thì toang không xử lý
            return false;
        }

        /// <summary>
        /// Nhiệm vụ tự tìm đường đến bản đồ tương ứng
        /// </summary>
        /// <param name="mapCode"></param>
        /// <param name="moveDone"></param>
        public static void QuestAutoFindPathToMap(int mapCode, Action moveDone)
        {
            /// Nếu không cần tìm đường
            if (!KTGlobal.IsNeedFindPathToOtherMaps(mapCode))
            {
                moveDone?.Invoke();
                return;
            }
            /// Nếu không thể di chuyển
            else if (!Global.Data.Leader.CanPositiveMove)
            {
                KTGlobal.AddNotification("Bạn đang bị trạng thái khống chế, không thể di chuyển!");
                return;
            }
            /// Nếu đang di chuyển
            else if (Global.Data.Leader.IsMoving)
            {
                /// Ngừng di chuyển
                KTLeaderMovingManager.StopMove();
                KTLeaderMovingManager.StopChasingTarget();
            }

            /// Danh sách các điểm đến trên đường đi
            List<int> maps = null;

            /// Nếu đây là nhóm bản đồ
            if (mapCode < 0 && Loader.Loader.AutoPaths.MapGroups.TryGetValue(mapCode, out AutoPathXML.MapGroup mapGroup))
            {
                /// Truyền tống phù tương ứng
                GoodsData teleportItem = Global.Data.RoleData.GoodsDataList?.Where(x => Loader.Loader.AutoPaths.TeleportItems.ContainsKey(x.GoodsID)).FirstOrDefault();
                /// Nếu không có truyền tống phù
                if (teleportItem == null)
                {
                    maps = AutoPathManager.Instance.FindPathWithoutTeleportItem(Global.Data.RoleData.MapCode, mapGroup.Maps);
                }
                /// Nếu có truyền tống phù
                else
                {
                    maps = AutoPathManager.Instance.FindPathWithTeleportItem(teleportItem.GoodsID, Global.Data.RoleData.MapCode, mapGroup.Maps);
                }

                AutoPathManager.Instance.FinishMoveByPaths = moveDone;
            }
            /// Nếu là bản đồ thường
            else if (mapCode > 0)
            {
                /// Truyền tống phù tương ứng
                GoodsData teleportItem = Global.Data.RoleData.GoodsDataList?.Where(x => Loader.Loader.AutoPaths.TeleportItems.ContainsKey(x.GoodsID)).FirstOrDefault();
                /// Nếu không có truyền tống phù
                if (teleportItem == null)
                {
                    maps = AutoPathManager.Instance.FindPathWithoutTeleportItem(Global.Data.RoleData.MapCode, mapCode);
                }
                /// Nếu có truyền tống phù
                else
                {
                    maps = AutoPathManager.Instance.FindPathWithTeleportItem(teleportItem.GoodsID, Global.Data.RoleData.MapCode, mapCode);
                }

                AutoPathManager.Instance.FinishMoveByPaths = moveDone;
            }
            else
            {
                AutoPathManager.Instance.StopAutoPath();
            }

            /// Nếu không có đường đi
            if (maps == null || maps.Count <= 0)
            {
                AutoPathManager.Instance.StopAutoPath();
                KTGlobal.AddNotification("Không tìm thấy đường đi tương ứng!");
                return;
            }

            List<string> resultMapName = new List<string>();
            foreach (int mapID in maps)
            {
                string mapName = Loader.Loader.Maps[mapID].Name;
                resultMapName.Add(string.Format("<color=yellow>[{0}]</color>", mapName));
            }
            KTGlobal.AddNotificationChatBoxMini("Tự tìm đường: " + string.Join(" -> ", resultMapName));
        }

        /// <summary>
        /// Nhiệm vụ tự tìm đường đến NPC tương ứng
        /// </summary>
        /// <param name="mapCode"></param>
        /// <param name="npcID"></param>
        /// <param name="moveDone"></param>
        public static void QuestAutoFindPathToNPC(int mapCode, int npcID, Action moveDone)
        {
            //KTDebug.LogError("FIND PATH TO NPC  :" + npcID + "MAPCODE :" + mapCode);

            /// Nếu không thể di chuyển
            if (!Global.Data.Leader.CanPositiveMove)
            {
                KTGlobal.AddNotification("Bạn đang bị trạng thái khống chế, không thể di chuyển!");
                return;
            }
            /// Nếu đang di chuyển
            else if (Global.Data.Leader.IsMoving)
            {
                /// Ngừng di chuyển
                KTLeaderMovingManager.StopMove();
                KTLeaderMovingManager.StopChasingTarget();
            }
            /// Nếu không có bản đồ
            else if (Global.Data.GameScene == null)
            {
                return;
            }

            /// Nếu không cùng bản đồ
            if (KTGlobal.IsNeedFindPathToOtherMaps(mapCode))
            {
                KTGlobal.QuestAutoFindPathToMap(mapCode, () =>
                {
                    KTGlobal.QuestAutoFindPathToNPC(mapCode, npcID, moveDone);
                });
                return;
            }

            if (Global.Data.GameScene.CurrentMapData.NpcListByID != null && Global.Data.GameScene.CurrentMapData.NpcListByID.TryGetValue(npcID, out List<Drawing.Point> npcPosList))
            {
                /// Toác
                if (npcPosList.Count <= 0)
                {
                    KTGlobal.AddNotification("Không tìm thấy mục tiêu!");
                    return;
                }

                /// Chọn vị trí gần nhất
                Drawing.Point npcPos = npcPosList.MinBy(x => Vector2.Distance(Global.Data.Leader.PositionInVector2, new Vector2(x.X, x.Y)));
                /// Thực hiện tìm đường
                KTGlobal.QuestAutoFindPath(mapCode, npcPos.X, npcPos.Y, moveDone);
            }
        }

        /// <summary>
        /// Nhiệm vụ tự tìm đường đến quái tương ứng
        /// </summary>
        /// <param name="mapCode"></param>
        /// <param name="monsterID"></param>
        /// <param name="moveDone"></param>
        public static void QuestAutoFindPathToMonster(int mapCode, int monsterID, Action moveDone)
        {
            /// Nếu không thể di chuyển
            if (!Global.Data.Leader.CanPositiveMove)
            {
                KTGlobal.AddNotification("Bạn đang bị trạng thái khống chế, không thể di chuyển!");
                return;
            }
            /// Nếu đang di chuyển
            else if (Global.Data.Leader.IsMoving)
            {
                /// Ngừng di chuyển
                KTLeaderMovingManager.StopMove();
                KTLeaderMovingManager.StopChasingTarget();
            }
            /// Nếu không có bản đồ
            else if (Global.Data.GameScene == null)
            {
                return;
            }

            /// Nếu không cùng bản đồ
            if (KTGlobal.IsNeedFindPathToOtherMaps(mapCode))
            {
                KTGlobal.QuestAutoFindPathToMap(mapCode, () =>
                {
                    KTGlobal.QuestAutoFindPathToMonster(mapCode, monsterID, moveDone);
                });
                return;
            }

            if (Global.Data.GameScene.CurrentMapData.MonsterListByID != null && Global.Data.GameScene.CurrentMapData.MonsterListByID.TryGetValue(monsterID, out List<Drawing.Point> npcPosList))
            {
                /// Toác
                if (npcPosList.Count <= 0)
                {
                    KTGlobal.AddNotification("Không tìm thấy mục tiêu!");
                    return;
                }

                /// Chọn vị trí gần nhất
                Drawing.Point npcPos = npcPosList.MinBy(x => Vector2.Distance(Global.Data.Leader.PositionInVector2, new Vector2(x.X, x.Y)));
                /// Thực hiện tìm đường
                KTGlobal.QuestAutoFindPath(mapCode, npcPos.X, npcPos.Y, moveDone);
            }
        }

        /// <summary>
        /// Nhiệm vụ tự tìm đường đến điểm thu thập tương ứng
        /// </summary>
        /// <param name="mapCode"></param>
        /// <param name="growPointID"></param>
        /// <param name="moveDone"></param>
        public static void QuestAutoFindPathToGrowPoint(int mapCode, int growPointID, Action moveDone)
        {
            /// Nếu không thể di chuyển
            if (!Global.Data.Leader.CanPositiveMove)
            {
                KTGlobal.AddNotification("Bạn đang bị trạng thái khống chế, không thể di chuyển!");
                return;
            }
            /// Nếu đang di chuyển
            else if (Global.Data.Leader.IsMoving)
            {
                /// Ngừng di chuyển
                KTLeaderMovingManager.StopMove();
                KTLeaderMovingManager.StopChasingTarget();
            }
            /// Nếu không có bản đồ
            else if (Global.Data.GameScene == null)
            {
                return;
            }

            /// Nếu không cùng bản đồ
            if (KTGlobal.IsNeedFindPathToOtherMaps(mapCode))
            {
                KTGlobal.QuestAutoFindPathToMap(mapCode, () =>
                {
                    KTGlobal.QuestAutoFindPathToGrowPoint(mapCode, growPointID, moveDone);
                });
                return;
            }

            if (Global.Data.GameScene.CurrentMapData.GrowPointListByID != null && Global.Data.GameScene.CurrentMapData.GrowPointListByID.TryGetValue(growPointID, out List<Drawing.Point> npcPosList))
            {
                /// Toác
                if (npcPosList.Count <= 0)
                {
                    KTGlobal.AddNotification("Không tìm thấy mục tiêu!");
                    return;
                }

                /// Chọn vị trí gần nhất
                Drawing.Point npcPos = npcPosList.MinBy(x => Vector2.Distance(Global.Data.Leader.PositionInVector2, new Vector2(x.X, x.Y)));
                /// Thực hiện tìm đường
                KTGlobal.QuestAutoFindPath(mapCode, npcPos.X, npcPos.Y, moveDone);
            }
        }

        /// <summary>
        /// Nhiệm vụ tự tìm đường đến vị trí đích
        /// </summary>
        /// <param name="mapCode"></param>
        /// <param name="posX"></param>
        /// <param name="posY"></param>
        /// <param name="moveDone"></param>
        public static void QuestAutoFindPath(int mapCode, int posX, int posY, Action moveDone)
        {
            /// Nếu không thể di chuyển
            if (!Global.Data.Leader.CanPositiveMove)
            {
                KTGlobal.AddNotification("Bạn đang bị trạng thái khống chế, không thể di chuyển!");
                return;
            }
            /// Nếu đang di chuyển
            if (Global.Data.Leader.IsMoving)
            {
                /// Ngừng di chuyển
                KTLeaderMovingManager.StopMove();
                KTLeaderMovingManager.StopChasingTarget();
            }

            /// Hiện dòng chữ tự tìm đường
            PlayZone.Instance.ShowTextAutoFindPath();

            /// Nếu bản đồ hiện tại không trùng với bản đồ đích
            if (KTGlobal.IsNeedFindPathToOtherMaps(mapCode))
            {
                KTGlobal.QuestAutoFindPathToMap(mapCode, () =>
                {
                    KTGlobal.QuestAutoFindPath(mapCode, posX, posY, () =>
                    {
                        /// Hủy dòng chữ tự tìm đường
                        PlayZone.Instance.HideTextAutoFindPath();
                        /// Thực hiện hàm Callback
                        moveDone?.Invoke();
                    });
                });
                return;
            }

            /// Nếu vị trí đích không đến được
            if (!Global.Data.GameScene.CanMoveByWorldPos(new Drawing.Point(posX, posY)))
            {
                KTGlobal.AddNotification("Vị trí đích không thể đến được!");
                return;
            }

            /// Khoảng cách đến vị trí đích
            float distance = Vector2.Distance(new Vector2(posX, posY), Global.Data.Leader.PositionInVector2);
            /// Nếu xa thì cho lên ngựa
            if (distance >= 1000)
            {
                /// Nếu có ngựa nhưng không trong trạng thái cưỡi
                GoodsData horseGD = Global.Data.RoleData.GoodsDataList?.Where(x => x.Using == (int) KE_EQUIP_POSITION.emEQUIPPOS_HORSE).FirstOrDefault();
                if (horseGD != null && !Global.Data.Leader.ComponentCharacter.Data.IsRiding)
                {
                    KT_TCPHandler.SendChangeToggleHorseState();
                }
            }

            //KTDebug.LogError("Start find path to pos");
            /// Thực hiện tìm đường đến vị trí đích
            KTLeaderMovingManager.AutoFindRoad(new Drawing.Point(posX, posY), () =>
            {
                /// Hủy dòng chữ tự tìm đường
                PlayZone.Instance.HideTextAutoFindPath();
                /// Thực hiện hàm Callback
                moveDone?.Invoke();
            });
        }

        #endregion Auto tìm đường nhiệm vụ
    }
}