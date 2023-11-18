using GameServer.KiemThe.Entities;
using GameServer.Logic;
using Server.Tools;
using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace GameServer.KiemThe.CopySceneEvents.ShenMiBaoKu
{
    /// <summary>
    /// Định nghĩa phụ bản Thần Bí Bảo Khố
    /// </summary>
    public static class ShenMiBaoKu
    {
        #region Define
        /// <summary>
        /// Thông tin phụ bản Thần Bí Bảo Khố
        /// </summary>
        public class SMBKData
        {
            /// <summary>
            /// Thiết lập sự kiện
            /// </summary>
            public class EventConfig
            {
                /// <summary>
                /// Thời gian tồn tại
                /// </summary>
                public int Duration { get; set; }

                /// <summary>
                /// Số Boss tối đa được gọi ra bằng Câu Hồn Ngọc
                /// </summary>
                public int MaxCallBoss { get; set; }

                /// <summary>
                /// Số lượt tham gia tối đa trong tuần
                /// </summary>
                public int LimitRoundPerWeek { get; set; }

                /// <summary>
                /// Cấp độ tối thiểu có thể tham gia
                /// </summary>
                public int MinLevel { get; set; }

                /// <summary>
                /// Chuyển đối tượng từ XMLNode
                /// </summary>
                /// <param name="xmlNode"></param>
                /// <returns></returns>
                public static EventConfig Parse(XElement xmlNode)
                {
                    return new EventConfig()
                    {
                        Duration = int.Parse(xmlNode.Attribute("Duration").Value),
                        MaxCallBoss = int.Parse(xmlNode.Attribute("MaxCallBoss").Value),
                        LimitRoundPerWeek = int.Parse(xmlNode.Attribute("LimitRoundPerWeek").Value),
                        MinLevel = int.Parse(xmlNode.Attribute("MinLevel").Value),
                    };
                }
            }

            /// <summary>
            /// Thông tin bản đồ
            /// </summary>
            public class MapConfig
            {
                /// <summary>
                /// ID bản đồ phụ bản
                /// </summary>
                public int MapID { get; set; }

                /// <summary>
                /// Vị trí X tiến vào phụ bản
                /// </summary>
                public int EnterPosX { get; set; }

                /// <summary>
                /// Vị trí Y tiến vào phụ bản
                /// </summary>
                public int EnterPosY { get; set; }

                /// <summary>
                /// ID bản đồ ra
                /// </summary>
                public int OutMapID { get; set; }

                /// <summary>
                /// Vị trí X đồ ra
                /// </summary>
                public int OutPosX { get; set; }

                /// <summary>
                /// Vị trí Y đồ ra
                /// </summary>
                public int OutPosY { get; set; }

                /// <summary>
                /// Chuyển đối tượng từ XMLNode
                /// </summary>
                /// <param name="xmlNode"></param>
                /// <returns></returns>
                public static MapConfig Parse(XElement xmlNode)
                {
                    return new MapConfig()
                    {
                        MapID = int.Parse(xmlNode.Attribute("MapID").Value),
                        EnterPosX = int.Parse(xmlNode.Attribute("EnterPosX").Value),
                        EnterPosY = int.Parse(xmlNode.Attribute("EnterPosY").Value),
                        OutMapID = int.Parse(xmlNode.Attribute("OutMapID").Value),
                        OutPosX = int.Parse(xmlNode.Attribute("OutPosX").Value),
                        OutPosY = int.Parse(xmlNode.Attribute("OutPosY").Value),
                    };
                }
            }

            /// <summary>
            /// Thông tin tầng
            /// </summary>
            public class StageInfo
            {
                /// <summary>
                /// Thông tin nhiệm vụ tầng
                /// </summary>
                public class TaskInfo
                {
                    /// <summary>
                    /// Loại nhiệm vụ
                    /// </summary>
                    public enum TaskType
                    {
                        /// <summary>
                        /// Không làm gì, tự qua
                        /// </summary>
                        None = 0,
                        /// <summary>
                        /// Giết quái
                        /// </summary>
                        KillMonsters = 1,
                        /// <summary>
                        /// Mở cơ quan và tiêu diệt toàn bộ quái hộ vệ
                        /// </summary>
                        OpenTriggersAndKillAllMonsters = 2,
                        /// <summary>
                        /// Giết Boss
                        /// </summary>
                        KillBosses = 3,
                        /// <summary>
                        /// Mở cơ quan
                        /// </summary>
                        OpenTriggers = 4,
                    }

                    /// <summary>
                    /// Thông tin Boss
                    /// </summary>
                    public class BossInfo
                    {
                        /// <summary>
                        /// ID quái
                        /// </summary>
                        public int ID { get; set; }

                        /// <summary>
                        /// Tên quái
                        /// </summary>
                        public string Name { get; set; }

                        /// <summary>
                        /// Danh hiệu quái
                        /// </summary>
                        public string Title { get; set; }

                        /// <summary>
                        /// Vị trí X
                        /// </summary>
                        public int PosX { get; set; }

                        /// <summary>
                        /// Vị trí Y
                        /// </summary>
                        public int PosY { get; set; }

                        /// <summary>
                        /// Sinh lực cơ bản
                        /// </summary>
                        public int BaseHP { get; set; }

                        /// <summary>
                        /// Sinh lực tăng thêm mỗi cấp
                        /// </summary>
                        public int HPIncreaseEachLevel { get; set; }

                        /// <summary>
                        /// Loại AI
                        /// </summary>
                        public MonsterAIType AIType { get; set; }

                        /// <summary>
                        /// ID Script AI điều khiển
                        /// </summary>
                        public int AIScriptID { get; set; }

                        /// <summary>
                        /// Danh sách kỹ năng sẽ sử dụng
                        /// </summary>
                        public List<SkillLevelRef> Skills { get; set; }

                        /// <summary>
                        /// Danh sách vòng sáng sẽ sử dụng
                        /// </summary>
                        public List<SkillLevelRef> Auras { get; set; }

                        /// <summary>
                        /// Chuyển đối tượng từ XMLNode
                        /// </summary>
                        /// <param name="xmlNode"></param>
                        /// <returns></returns>
                        public static BossInfo Parse(XElement xmlNode)
                        {
                            BossInfo monsterInfo = new BossInfo()
                            {
                                ID = int.Parse(xmlNode.Attribute("ID").Value),
                                Name = xmlNode.Attribute("Name").Value,
                                Title = xmlNode.Attribute("Title").Value,
                                PosX = int.Parse(xmlNode.Attribute("PosX").Value),
                                PosY = int.Parse(xmlNode.Attribute("PosY").Value),
                                BaseHP = int.Parse(xmlNode.Attribute("BaseHP").Value),
                                HPIncreaseEachLevel = int.Parse(xmlNode.Attribute("HPIncreaseEachLevel").Value),
                                AIType = (MonsterAIType) int.Parse(xmlNode.Attribute("AIType").Value),
                                AIScriptID = int.Parse(xmlNode.Attribute("AIScriptID").Value),
                                Skills = new List<SkillLevelRef>(),
                                Auras = new List<SkillLevelRef>(),
                            };

                            /// Chuỗi mã hóa danh sách kỹ năng sử dụng
                            string skillsString = xmlNode.Attribute("Skills").Value;
                            /// Nếu có kỹ năng
                            if (!string.IsNullOrEmpty(skillsString))
                            {
                                /// Duyệt danh sách kỹ năng
                                foreach (string skillStr in skillsString.Split(';'))
                                {
                                    string[] fields = skillStr.Split('_');
                                    try
                                    {
                                        int skillID = int.Parse(fields[0]);
                                        int skillLevel = int.Parse(fields[1]);
                                        int cooldown = int.Parse(fields[2]);

                                        /// Thông tin kỹ năng tương ứng
                                        SkillDataEx skillData = KSkill.GetSkillData(skillID);
                                        /// Nếu kỹ năng không tồn tại
                                        if (skillData == null)
                                        {
                                            throw new Exception(string.Format("Skill ID = {0} not found!", skillID));
                                        }

                                        /// Nếu cấp độ dưới 0
                                        if (skillLevel <= 0)
                                        {
                                            throw new Exception(string.Format("Skill ID = {0} level must be greater than 0", skillID));
                                        }

                                        /// Kỹ năng theo cấp
                                        SkillLevelRef skillRef = new SkillLevelRef()
                                        {
                                            Data = skillData,
                                            AddedLevel = skillLevel,
                                            Exp = cooldown,
                                        };

                                        /// Thêm vào danh sách
                                        monsterInfo.Skills.Add(skillRef);
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine(ex.ToString());
                                        LogManager.WriteLog(LogTypes.Exception, ex.ToString());
                                    }
                                }
                            }

                            /// Chuỗi mã hóa danh sách vòng sáng sử dụng
                            string aurasString = xmlNode.Attribute("Auras").Value;
                            /// Nếu có kỹ năng
                            if (!string.IsNullOrEmpty(aurasString))
                            {
                                /// Duyệt danh sách kỹ năng
                                foreach (string skillStr in aurasString.Split(';'))
                                {
                                    string[] fields = skillStr.Split('_');
                                    try
                                    {
                                        int skillID = int.Parse(fields[0]);
                                        int skillLevel = int.Parse(fields[1]);

                                        /// Thông tin kỹ năng tương ứng
                                        SkillDataEx skillData = KSkill.GetSkillData(skillID);
                                        /// Nếu kỹ năng không tồn tại
                                        if (skillData == null)
                                        {
                                            throw new Exception(string.Format("Skill ID = {0} not found!", skillID));
                                        }

                                        /// Nếu cấp độ dưới 0
                                        if (skillLevel <= 0)
                                        {
                                            throw new Exception(string.Format("Skill ID = {0} level must be greater than 0", skillID));
                                        }

                                        /// Nếu không phải vòng sáng
                                        if (!skillData.IsArua)
                                        {
                                            throw new Exception(string.Format("Skill ID = {0} is not Aura!", skillID));
                                        }

                                        /// Kỹ năng theo cấp
                                        SkillLevelRef skillRef = new SkillLevelRef()
                                        {
                                            Data = skillData,
                                            AddedLevel = skillLevel,
                                        };

                                        /// Thêm vào danh sách
                                        monsterInfo.Auras.Add(skillRef);
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine(ex.ToString());
                                        LogManager.WriteLog(LogTypes.Exception, ex.ToString());
                                    }
                                }
                            }

                            /// Trả về kết quả
                            return monsterInfo;
                        }
                    }

                    /// <summary>
                    /// Thông tin quái
                    /// </summary>
                    public class MonsterInfo
                    {
                        /// <summary>
                        /// ID quái
                        /// </summary>
                        public int ID { get; set; }

                        /// <summary>
                        /// Tên quái
                        /// </summary>
                        public string Name { get; set; }

                        /// <summary>
                        /// Danh hiệu quái
                        /// </summary>
                        public string Title { get; set; }

                        /// <summary>
                        /// Vị trí X
                        /// </summary>
                        public int PosX { get; set; }

                        /// <summary>
                        /// Vị trí Y
                        /// </summary>
                        public int PosY { get; set; }

                        /// <summary>
                        /// Sinh lực cơ bản
                        /// </summary>
                        public int BaseHP { get; set; }

                        /// <summary>
                        /// Sinh lực tăng thêm mỗi cấp
                        /// </summary>
                        public int HPIncreaseEachLevel { get; set; }

                        /// <summary>
                        /// Loại AI
                        /// </summary>
                        public MonsterAIType AIType { get; set; }

                        /// <summary>
                        /// ID Script AI điều khiển
                        /// </summary>
                        public int AIScriptID { get; set; }

                        /// <summary>
                        /// Danh sách kỹ năng sẽ sử dụng
                        /// </summary>
                        public List<SkillLevelRef> Skills { get; set; }

                        /// <summary>
                        /// Danh sách vòng sáng sẽ sử dụng
                        /// </summary>
                        public List<SkillLevelRef> Auras { get; set; }

                        /// <summary>
                        /// Chuyển đối tượng từ XMLNode
                        /// </summary>
                        /// <param name="xmlNode"></param>
                        /// <returns></returns>
                        public static MonsterInfo Parse(XElement xmlNode)
                        {
                            MonsterInfo monsterInfo = new MonsterInfo()
                            {
                                ID = int.Parse(xmlNode.Attribute("ID").Value),
                                Name = xmlNode.Attribute("Name").Value,
                                Title = xmlNode.Attribute("Title").Value,
                                PosX = int.Parse(xmlNode.Attribute("PosX").Value),
                                PosY = int.Parse(xmlNode.Attribute("PosY").Value),
                                BaseHP = int.Parse(xmlNode.Attribute("BaseHP").Value),
                                HPIncreaseEachLevel = int.Parse(xmlNode.Attribute("HPIncreaseEachLevel").Value),
                                AIType = (MonsterAIType) int.Parse(xmlNode.Attribute("AIType").Value),
                                AIScriptID = int.Parse(xmlNode.Attribute("AIScriptID").Value),
                                Skills = new List<SkillLevelRef>(),
                                Auras = new List<SkillLevelRef>(),
                            };

                            /// Chuỗi mã hóa danh sách kỹ năng sử dụng
                            string skillsString = xmlNode.Attribute("Skills").Value;
                            /// Nếu có kỹ năng
                            if (!string.IsNullOrEmpty(skillsString))
                            {
                                /// Duyệt danh sách kỹ năng
                                foreach (string skillStr in skillsString.Split(';'))
                                {
                                    string[] fields = skillStr.Split('_');
                                    try
                                    {
                                        int skillID = int.Parse(fields[0]);
                                        int skillLevel = int.Parse(fields[1]);
                                        int cooldown = int.Parse(fields[2]);

                                        /// Thông tin kỹ năng tương ứng
                                        SkillDataEx skillData = KSkill.GetSkillData(skillID);
                                        /// Nếu kỹ năng không tồn tại
                                        if (skillData == null)
                                        {
                                            throw new Exception(string.Format("Skill ID = {0} not found!", skillID));
                                        }

                                        /// Nếu cấp độ dưới 0
                                        if (skillLevel <= 0)
                                        {
                                            throw new Exception(string.Format("Skill ID = {0} level must be greater than 0", skillID));
                                        }

                                        /// Kỹ năng theo cấp
                                        SkillLevelRef skillRef = new SkillLevelRef()
                                        {
                                            Data = skillData,
                                            AddedLevel = skillLevel,
                                            Exp = cooldown,
                                        };

                                        /// Thêm vào danh sách
                                        monsterInfo.Skills.Add(skillRef);
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine(ex.ToString());
                                        LogManager.WriteLog(LogTypes.Exception, ex.ToString());
                                    }
                                }
                            }

                            /// Chuỗi mã hóa danh sách vòng sáng sử dụng
                            string aurasString = xmlNode.Attribute("Auras").Value;
                            /// Nếu có kỹ năng
                            if (!string.IsNullOrEmpty(aurasString))
                            {
                                /// Duyệt danh sách kỹ năng
                                foreach (string skillStr in aurasString.Split(';'))
                                {
                                    string[] fields = skillStr.Split('_');
                                    try
                                    {
                                        int skillID = int.Parse(fields[0]);
                                        int skillLevel = int.Parse(fields[1]);

                                        /// Thông tin kỹ năng tương ứng
                                        SkillDataEx skillData = KSkill.GetSkillData(skillID);
                                        /// Nếu kỹ năng không tồn tại
                                        if (skillData == null)
                                        {
                                            throw new Exception(string.Format("Skill ID = {0} not found!", skillID));
                                        }

                                        /// Nếu cấp độ dưới 0
                                        if (skillLevel <= 0)
                                        {
                                            throw new Exception(string.Format("Skill ID = {0} level must be greater than 0", skillID));
                                        }

                                        /// Nếu không phải vòng sáng
                                        if (!skillData.IsArua)
                                        {
                                            throw new Exception(string.Format("Skill ID = {0} is not Aura!", skillID));
                                        }

                                        /// Kỹ năng theo cấp
                                        SkillLevelRef skillRef = new SkillLevelRef()
                                        {
                                            Data = skillData,
                                            AddedLevel = skillLevel,
                                        };

                                        /// Thêm vào danh sách
                                        monsterInfo.Auras.Add(skillRef);
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine(ex.ToString());
                                        LogManager.WriteLog(LogTypes.Exception, ex.ToString());
                                    }
                                }
                            }

                            /// Trả về kết quả
                            return monsterInfo;
                        }
                    }

                    /// <summary>
                    /// Thông tin cổng dịch chuyển
                    /// </summary>
                    public class TeleportInfo
                    {
                        /// <summary>
                        /// Tên cổng
                        /// </summary>
                        public string Name { get; set; }

                        /// <summary>
                        /// Vị trí X
                        /// </summary>
                        public int PosX { get; set; }

                        /// <summary>
                        /// Vị trí Y
                        /// </summary>
                        public int PosY { get; set; }

                        /// <summary>
                        /// Vị trí dịch tới X
                        /// </summary>
                        public int ToPosX { get; set; }

                        /// <summary>
                        /// Vị trí dịch tới Y
                        /// </summary>
                        public int ToPosY { get; set; }

                        /// <summary>
                        /// Tạo ra ngay lập tức cùng Task không
                        /// </summary>
                        public bool SpawnImmediate { get; set; }

                        /// <summary>
                        /// Chuyển đối tượng từ XMLNode
                        /// </summary>
                        /// <param name="xmlNode"></param>
                        /// <returns></returns>
                        public static TeleportInfo Parse(XElement xmlNode)
                        {
                            return new TeleportInfo()
                            {
                                Name = xmlNode.Attribute("Name").Value,
                                PosX = int.Parse(xmlNode.Attribute("PosX").Value),
                                PosY = int.Parse(xmlNode.Attribute("PosY").Value),
                                ToPosX = int.Parse(xmlNode.Attribute("ToPosX").Value),
                                ToPosY = int.Parse(xmlNode.Attribute("ToPosY").Value),
                                SpawnImmediate = bool.Parse(xmlNode.Attribute("SpawnImmediate").Value),
                            };
                        }
                    }

                    /// <summary>
                    /// Thông tin cơ quan
                    /// </summary>
                    public class TriggerInfo
                    {
                        /// <summary>
                        /// ID
                        /// </summary>
                        public int ID { get; set; }

                        /// <summary>
                        /// Tên cơ quan
                        /// </summary>
                        public string Name { get; set; }

                        /// <summary>
                        /// Thời gian mở
                        /// </summary>
                        public int CollectTick { get; set; }

                        /// <summary>
                        /// Tọa độ X
                        /// </summary>
                        public int PosX { get; set; }

                        /// <summary>
                        /// Tọa độ Y
                        /// </summary>
                        public int PosY { get; set; }

                        /// <summary>
                        /// Chuyển đối tượng từ XMLNode
                        /// </summary>
                        /// <param name="xmlNode"></param>
                        /// <returns></returns>
                        public static TriggerInfo Parse(XElement xmlNode)
                        {
                            return new TriggerInfo()
                            {
                                ID = int.Parse(xmlNode.Attribute("ID").Value),
                                Name = xmlNode.Attribute("Name").Value,
                                CollectTick = int.Parse(xmlNode.Attribute("CollectTick").Value),
                                PosX = int.Parse(xmlNode.Attribute("PosX").Value),
                                PosY = int.Parse(xmlNode.Attribute("PosY").Value),
                            };
                        }
                    }

                    /// <summary>
                    /// Thông tin Obs động
                    /// </summary>
                    public class DynamicObstacleInfo
                    {
                        /// <summary>
                        /// Danh sách Layer obs động
                        /// </summary>
                        public List<byte> Layers { get; set; }

                        /// <summary>
                        /// Khai mở ngay lập tức không
                        /// </summary>
                        public bool OpenImmediate { get; set; }

                        /// <summary>
                        /// Chuyển đối tượng từ XMLNode
                        /// </summary>
                        /// <param name="xmlNode"></param>
                        /// <returns></returns>
                        public static DynamicObstacleInfo Parse(XElement xmlNode)
                        {
                            /// Tạo mới
                            DynamicObstacleInfo info = new DynamicObstacleInfo()
                            {
                                Layers = new List<byte>(),
                                OpenImmediate = bool.Parse(xmlNode.Attribute("OpenImmediate").Value),
                            };

                            /// Chuỗi danh sách Layer
                            string layersString = xmlNode.Attribute("Layers").Value;
                            /// Nếu tồn tại
                            if (!string.IsNullOrEmpty(layersString))
                            {
                                /// Duyệt danh sách Layer
                                foreach (string layerString in layersString.Split(';'))
                                {
                                    /// Layer tương ứng
                                    byte layer = byte.Parse(layerString);
                                    /// Thêm vào danh sách
                                    info.Layers.Add(layer);
                                }
                            }


                            /// Trả về kết quả
                            return info;
                        }
                    }

                    /// <summary>
                    /// ID nhiệm vụ
                    /// </summary>
                    public int ID { get; set; }

                    /// <summary>
                    /// Loại nhiệm vụ
                    /// </summary>
                    public TaskType Type { get; set; }

                    /// <summary>
                    /// Danh sách nhiệm vụ yêu cầu trước đó
                    /// </summary>
                    public HashSet<int> RequireTasks { get; set; }

                    /// <summary>
                    /// Tên nhiệm vụ
                    /// </summary>
                    public string Name { get; set; }

                    /// <summary>
                    /// Danh sách quái
                    /// </summary>
                    public List<MonsterInfo> Monsters { get; set; }

                    /// <summary>
                    /// Danh sách Boss
                    /// </summary>
                    public List<BossInfo> Bosses { get; set; }

                    /// <summary>
                    /// Danh sách cổng dịch chuyển
                    /// </summary>
                    public List<TeleportInfo> Teleports { get; set; }

                    /// <summary>
                    /// Danh sách cơ quan
                    /// </summary>
                    public List<TriggerInfo> Triggers { get; set; }

                    /// <summary>
                    /// Danh sách Obs động sẽ khai mở
                    /// </summary>
                    public DynamicObstacleInfo DynamicObstacles { get; set; }

                    /// <summary>
                    /// Chuyển đối tượng từ XMLNode
                    /// </summary>
                    /// <param name="xmlNode"></param>
                    /// <returns></returns>
                    public static TaskInfo Parse(XElement xmlNode)
                    {
                        /// Tạo mới
                        TaskInfo taskInfo = new TaskInfo()
                        {
                            ID = int.Parse(xmlNode.Attribute("ID").Value),
                            Type = (TaskType) int.Parse(xmlNode.Attribute("Type").Value),
                            Name = xmlNode.Attribute("Name").Value,
                            RequireTasks = new HashSet<int>(),
                        };

                        /// Danh sách nhiệm vụ yêu cầu
                        string requireTasksString = xmlNode.Attribute("RequireTasks").Value;
                        /// Nếu có yêu cầu nhiệm vụ
                        if (requireTasksString != "-1")
                        {
                            /// Duyệt danh sách
                            foreach (string taskIDString in requireTasksString.Split(';'))
                            {
                                /// ID nhiệm vụ
                                int taskID = int.Parse(taskIDString);
                                /// Thêm vào danh sách
                                taskInfo.RequireTasks.Add(taskID);
                            }
                        }

                        /// Tạo mới danh sách cổng dịch chuyển
                        taskInfo.Teleports = new List<TeleportInfo>();
                        /// Nếu tồn tại cổng dịch chuyển
                        if (xmlNode.Element("Teleports") != null)
                        {
                            /// Duyệt danh sách cổng dịch chuyển
                            foreach (XElement node in xmlNode.Element("Teleports").Elements("Teleport"))
                            {
                                /// Thêm vào danh sách
                                taskInfo.Teleports.Add(TeleportInfo.Parse(node));
                            }
                        }

                        /// Nếu tồn tại Obs động
                        if (xmlNode.Element("DynamicObstacles") != null)
                        {
                            /// Lấy thông tin
                            taskInfo.DynamicObstacles = DynamicObstacleInfo.Parse(xmlNode.Element("DynamicObstacles"));
                        }

                        /// Loại nhiệm vụ
                        switch (taskInfo.Type)
                        {
                            /// Giết toàn bộ quái
                            case TaskType.KillMonsters:
                            {
                                /// Tạo mới danh sách quái
                                taskInfo.Monsters = new List<MonsterInfo>();

                                /// Duyệt danh sách quái
                                foreach (XElement node in xmlNode.Element("Monsters").Elements("Monster"))
                                {
                                    /// Thêm vào danh sách
                                    taskInfo.Monsters.Add(MonsterInfo.Parse(node));
                                }

                                /// Thoát
                                break;
                            }
                            /// Mở cơ quan và giết toàn bộ quái
                            case TaskType.OpenTriggersAndKillAllMonsters:
                            {
                                /// Tạo mới danh sách cơ quan
                                taskInfo.Triggers = new List<TriggerInfo>();
                                /// Tạo mới danh sách quái
                                taskInfo.Monsters = new List<MonsterInfo>();

                                /// Duyệt danh sách cơ quan
                                foreach (XElement node in xmlNode.Element("Triggers").Elements("Trigger"))
                                {
                                    /// Thêm vào danh sách
                                    taskInfo.Triggers.Add(TriggerInfo.Parse(node));
                                }

                                /// Duyệt danh sách quái
                                foreach (XElement node in xmlNode.Element("Monsters").Elements("Monster"))
                                {
                                    /// Thêm vào danh sách
                                    taskInfo.Monsters.Add(MonsterInfo.Parse(node));
                                }

                                /// Thoát
                                break;
                            }
                            /// Mở cơ quan
                            case TaskType.OpenTriggers:
                            {
                                /// Tạo mới danh sách cơ quan
                                taskInfo.Triggers = new List<TriggerInfo>();

                                /// Duyệt danh sách cơ quan
                                foreach (XElement node in xmlNode.Element("Triggers").Elements("Trigger"))
                                {
                                    /// Thêm vào danh sách
                                    taskInfo.Triggers.Add(TriggerInfo.Parse(node));
                                }

                                /// Thoát
                                break;
                            }
                            /// Giết Boss
                            case TaskType.KillBosses:
                            {
                                /// Tạo mới danh sách Boss
                                taskInfo.Bosses = new List<BossInfo>();

                                /// Duyệt danh sách Boss
                                foreach (XElement node in xmlNode.Element("Bosses").Elements("Boss"))
                                {
                                    /// Thêm vào danh sách
                                    taskInfo.Bosses.Add(BossInfo.Parse(node));
                                }

                                /// Thoát
                                break;
                            }
                        }

                        /// Trả về kết quả
                        return taskInfo;
                    }
                }

                /// <summary>
                /// ID tầng
                /// </summary>
                public int ID { get; set; }

                /// <summary>
                /// Cho phép gọi Boss không
                /// </summary>
                public bool AllowCallBoss { get; set; }

                /// <summary>
                /// Danh sách nhiệm vụ của tầng
                /// </summary>
                public List<TaskInfo> Tasks { get; set; }

                /// <summary>
                /// Chuyển đối tượng từ XMLNode
                /// </summary>
                /// <param name="xmlNode"></param>
                /// <returns></returns>
                public static StageInfo Parse(XElement xmlNode)
                {
                    /// Tạo mới
                    StageInfo stageInfo = new StageInfo()
                    {
                        ID = int.Parse(xmlNode.Attribute("ID").Value),
                        AllowCallBoss = bool.Parse(xmlNode.Attribute("AllowCallBoss").Value),
                        Tasks = new List<TaskInfo>(),
                    };

                    /// Duyệt danh sách nhiệm vụ tầng
                    foreach (XElement node in xmlNode.Elements("Task"))
                    {
                        /// Thêm vào danh sách
                        stageInfo.Tasks.Add(TaskInfo.Parse(node));
                    }

                    /// Trả về kết quả
                    return stageInfo;
                }
            }

            /// <summary>
            /// Thiết lập sự kiện
            /// </summary>
            public EventConfig Config { get; set; }

            /// <summary>
            /// Thông tin bản đồ
            /// </summary>
            public MapConfig Map { get; set; }

            /// <summary>
            /// Danh sách các tầng
            /// </summary>
            public List<StageInfo> Stages { get; set; }

            /// <summary>
            /// Chuyển đối tượng từ XMLNode
            /// </summary>
            /// <param name="xmlNode"></param>
            /// <returns></returns>
            public static SMBKData Parse(XElement xmlNode)
            {
                /// Tạo mới
                SMBKData data = new SMBKData()
                {
                    Config = EventConfig.Parse(xmlNode.Element("Config")),
                    Map = MapConfig.Parse(xmlNode.Element("Map")),
                    Stages = new List<StageInfo>(),
                };

                /// Duyệt danh sách các tầng
                foreach (XElement node in xmlNode.Element("EventStages").Elements("Stage"))
                {
                    /// Thêm vào danh sách
                    data.Stages.Add(StageInfo.Parse(node));
                }

                /// Trả về kết quả
                return data;
            }
        }
        #endregion

        #region Core
        /// <summary>
        /// Thông tin phụ bản
        /// </summary>
        public static SMBKData Data { get; private set; }

        /// <summary>
        /// Khởi tạo phụ bản Thần Bí Bảo Khố
        /// </summary>
        public static void Init()
        {
            XElement xmlNode = KTGlobal.ReadXMLData("Config/KT_CopyScenes/ShenMiBaoKu.xml");
            /// ID sự kiện
            ShenMiBaoKu.Data = SMBKData.Parse(xmlNode);
        }
        #endregion
    }
}
