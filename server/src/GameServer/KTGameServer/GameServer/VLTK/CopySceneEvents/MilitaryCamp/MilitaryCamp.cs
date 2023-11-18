using GameServer.KiemThe.Core.Item;
using GameServer.KiemThe.Entities;
using GameServer.Logic;
using Server.Data;
using Server.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace GameServer.KiemThe.CopySceneEvents.MilitaryCampFuBen
{
    /// <summary>
    /// Phụ bản Quân doanh
    /// </summary>
    public static class MilitaryCamp
    {
        #region Define
        /// <summary>
        /// Loại phụ bản
        /// </summary>
        public enum EventType
        {
            /// <summary>
            /// Hậu Sơn Phục Ngưu
            /// </summary>
            FootHills = 0,
            /// <summary>
            /// Bách Man Sơn
            /// </summary>
            MountainPeak = 1,
            /// <summary>
            /// Hải Lăng Vương Mộ
            /// </summary>
            RoyalTomb = 2,
        }

        /// <summary>
        /// Điều kiện tham gia
        /// </summary>
        public class EventCondition
        {
            /// <summary>
            /// Yêu cầu cấp độ
            /// </summary>
            public int RequireLevel { get; set; }

            /// <summary>
            /// Số thành viên nhóm tối thiểu
            /// </summary>
            public int LimitMembers { get; set; }

            /// <summary>
            /// Số lượt đi trong tuần
            /// </summary>
            public int RoundsPerDay { get; set; }

            /// <summary>
            /// Chuyển đối tượng từ XMLNode
            /// </summary>
            /// <param name="xmlNode"></param>
            /// <returns></returns>
            public static EventCondition Parse(XElement xmlNode)
            {
                return new EventCondition()
                {
                    RequireLevel = int.Parse(xmlNode.Attribute("RequireLevel").Value),
                    LimitMembers = int.Parse(xmlNode.Attribute("LimitMembers").Value),
                    RoundsPerDay = int.Parse(xmlNode.Attribute("RoundsPerDay").Value),
                };
            }
        }

        /// <summary>
        /// Thông tin phụ bản
        /// </summary>
        public class EventInfo
        {
            /// <summary>
            /// Thiết lập sự kiện
            /// </summary>
            public class EventConfig
            {
                /// <summary>
                /// ID bản đồ phụ bản
                /// </summary>
                public int MapID { get; set; }

                /// <summary>
                /// Tọa độ X tiến vào
                /// </summary>
                public int EnterPosX { get; set; }

                /// <summary>
                /// Tọa độ Y tiến vào
                /// </summary>
                public int EnterPosY { get; set; }

                /// <summary>
                /// Tọa độ X hồi sinh
                /// </summary>
                public int RelivePosX { get; set; }

                /// <summary>
                /// Tọa độ Y hồi sinh
                /// </summary>
                public int RelivePosY { get; set; }

                /// <summary>
                /// ID bản đồ báo danh
                /// </summary>
                public int OutMapID { get; set; }

                /// <summary>
                /// Vị trí X đẩy ra ở bản đồ báo danh
                /// </summary>
                public int OutPosX { get; set; }

                /// <summary>
                /// Vị trí Y đẩy ra ở bản đồ báo danh
                /// </summary>
                public int OutPosY { get; set; }

                /// <summary>
                /// Chuyển đối tượng từ XMLNode
                /// </summary>
                /// <param name="xmlNode"></param>
                /// <returns></returns>
                public static EventConfig Parse(XElement xmlNode)
                {
                    return new EventConfig()
                    {
                        MapID = int.Parse(xmlNode.Attribute("MapID").Value),
                        EnterPosX = int.Parse(xmlNode.Attribute("EnterPosX").Value),
                        EnterPosY = int.Parse(xmlNode.Attribute("EnterPosY").Value),
                        RelivePosX = int.Parse(xmlNode.Attribute("RelivePosX").Value),
                        RelivePosY = int.Parse(xmlNode.Attribute("RelivePosY").Value),
                        OutMapID = int.Parse(xmlNode.Attribute("OutMapID").Value),
                        OutPosX = int.Parse(xmlNode.Attribute("OutPosX").Value),
                        OutPosY = int.Parse(xmlNode.Attribute("OutPosY").Value),
                    };
                }
            }

            /// <summary>
            /// Thông tin NPC
            /// </summary>
            public class NPCInfo
            {
                /// <summary>
                /// ID NPC
                /// </summary>
                public int ID { get; set; }

                /// <summary>
                /// Tên
                /// </summary>
                public string Name { get; set; }

                /// <summary>
                /// Danh hiệu
                /// </summary>
                public string Title { get; set; }

                /// <summary>
                /// Tọa độ X
                /// </summary>
                public int PosX { get; set; }

                /// <summary>
                /// Tọa độ Y
                /// </summary>
                public int PosY { get; set; }

                /// <summary>
                /// ID Script Lua điều khiển
                /// </summary>
                public int ScriptID { get; set; }

                /// <summary>
                /// Chuyển đối tượng từ XMLNode
                /// </summary>
                /// <param name="xmlNode"></param>
                /// <returns></returns>
                public static NPCInfo Parse(XElement xmlNode)
                {
                    return new NPCInfo()
                    {
                        ID = int.Parse(xmlNode.Attribute("ID").Value),
                        Name = xmlNode.Attribute("Name").Value,
                        Title = xmlNode.Attribute("Title").Value,
                        PosX = int.Parse(xmlNode.Attribute("PosX").Value),
                        PosY = int.Parse(xmlNode.Attribute("PosY").Value),
                        ScriptID = int.Parse(xmlNode.Attribute("ScriptID").Value),
                    };
                }
            }

            /// <summary>
            /// Thông tin tầng
            /// </summary>
            public class StageInfo
            {
                /// <summary>
                /// Thông tin cơ quan
                /// </summary>
                public class TriggerData
                {
                    /// <summary>
                    /// Thông tin cơ quan mở đường
                    /// </summary>
                    public class KeyTriggerInfo
                    {
                        /// <summary>
                        /// ID cơ quan
                        /// </summary>
                        public int ID { get; set; }

                        /// <summary>
                        /// ID cơ quan trong bản đồ
                        /// </summary>
                        public int TriggerID { get; set; }

                        /// <summary>
                        /// ID cơ quan vật cản sẽ mở
                        /// </summary>
                        public int ObstacleTriggerID { get; set; }

                        /// <summary>
                        /// Nhãn Obs động sẽ mở
                        /// </summary>
                        public byte DynamicObsLabel { get; set; }

                        /// <summary>
                        /// Tên cơ quan
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
                        /// Thời gian mở cơ quan
                        /// </summary>
                        public int CollectTick { get; set; }

                        /// <summary>
                        /// Chuyển đối tượng từ XMLNode
                        /// </summary>
                        /// <param name="xmlNode"></param>
                        /// <returns></returns>
                        public static KeyTriggerInfo Parse(XElement xmlNode)
                        {
                            return new KeyTriggerInfo()
                            {
                                ID = int.Parse(xmlNode.Attribute("ID").Value),
                                TriggerID = int.Parse(xmlNode.Attribute("TriggerID").Value),
                                ObstacleTriggerID = int.Parse(xmlNode.Attribute("ObstacleTriggerID").Value),
                                DynamicObsLabel = byte.Parse(xmlNode.Attribute("DynamicObsLabel").Value),
                                Name = xmlNode.Attribute("Name").Value,
                                PosX = int.Parse(xmlNode.Attribute("PosX").Value),
                                PosY = int.Parse(xmlNode.Attribute("PosY").Value),
                                CollectTick = int.Parse(xmlNode.Attribute("CollectTick").Value),
                            };
                        }
                    }

                    /// <summary>
                    /// Thông tin cơ quan cản đường
                    /// </summary>
                    public class ObstacleTriggerInfo
                    {
                        /// <summary>
                        /// ID cơ quan
                        /// </summary>
                        public int ID { get; set; }

                        /// <summary>
                        /// ID cơ quan trong bản đồ
                        /// </summary>
                        public int TriggerID { get; set; }

                        /// <summary>
                        /// Tên cơ quan
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
                        /// Chuyển đối tượng từ XMLNode
                        /// </summary>
                        /// <param name="xmlNode"></param>
                        /// <returns></returns>
                        public static ObstacleTriggerInfo Parse(XElement xmlNode)
                        {
                            return new ObstacleTriggerInfo()
                            {
                                ID = int.Parse(xmlNode.Attribute("ID").Value),
                                TriggerID = int.Parse(xmlNode.Attribute("TriggerID").Value),
                                Name = xmlNode.Attribute("Name").Value,
                                PosX = int.Parse(xmlNode.Attribute("PosX").Value),
                                PosY = int.Parse(xmlNode.Attribute("PosY").Value),
                            };
                        }
                    }

                    /// <summary>
                    /// Thông tin cạm bẫy
                    /// </summary>
                    public class TrapInfo
                    {
                        /// <summary>
                        /// ID
                        /// </summary>
                        public int ID { get; set; }

                        /// <summary>
                        /// Tên
                        /// </summary>
                        public string Name { get; set; }

                        /// <summary>
                        /// Tọa độ X
                        /// </summary>
                        public int PosX { get; set; }

                        /// <summary>
                        /// Tọa độ Y
                        /// </summary>
                        public int PosY { get; set; }

                        /// <summary>
                        /// Thông báo khi bị phát nổ
                        /// </summary>
                        public string TouchMessage { get; set; }

                        /// <summary>
                        /// Chuyển đối tượng từ XMLNode
                        /// </summary>
                        /// <param name="xmlNode"></param>
                        /// <returns></returns>
                        public static TrapInfo Parse(XElement xmlNode)
                        {
                            return new TrapInfo()
                            {
                                ID = int.Parse(xmlNode.Attribute("ID").Value),
                                Name = xmlNode.Attribute("Name").Value,
                                PosX = int.Parse(xmlNode.Attribute("PosX").Value),
                                PosY = int.Parse(xmlNode.Attribute("PosY").Value),
                                TouchMessage = xmlNode.Attribute("TouchMessage").Value,
                            };
                        }
                    }

                    /// <summary>
                    /// Danh sách cơ quan mở đường
                    /// </summary>
                    public Dictionary<int, KeyTriggerInfo> KeyTriggers { get; set; }

                    /// <summary>
                    /// Danh sách cơ quan cản đường
                    /// </summary>
                    public Dictionary<int, ObstacleTriggerInfo> ObstacleTriggers { get; set; }

                    /// <summary>
                    /// Danh sách cạm bẫy
                    /// </summary>
                    public List<TrapInfo> Traps { get; set; }

                    /// <summary>
                    /// Chuyển đối tượng từ XMLNode
                    /// </summary>
                    /// <param name="xmlNode"></param>
                    /// <returns></returns>
                    public static TriggerData Parse(XElement xmlNode)
                    {
                        TriggerData triggerInfo = new TriggerData()
                        {
                            KeyTriggers = new Dictionary<int, KeyTriggerInfo>(),
                            ObstacleTriggers = new Dictionary<int, ObstacleTriggerInfo>(),
                            Traps = new List<TrapInfo>(),
                        };

                        /// Duyệt danh sách cơ quan mở đường
                        foreach (XElement node in xmlNode.Elements("KeyTrigger"))
                        {
                            KeyTriggerInfo trigger = KeyTriggerInfo.Parse(node);
                            triggerInfo.KeyTriggers[trigger.TriggerID] = trigger;
                        }

                        /// Duyệt danh sách cơ quan cản đường
                        foreach (XElement node in xmlNode.Elements("ObstacleTrigger"))
                        {
                            ObstacleTriggerInfo trigger = ObstacleTriggerInfo.Parse(node);
                            triggerInfo.ObstacleTriggers[trigger.TriggerID] = trigger;
                        }

                        /// Duyệt danh sách cạm bẫy
                        foreach (XElement node in xmlNode.Elements("Trap"))
                        {
                            TrapInfo trap = TrapInfo.Parse(node);
                            triggerInfo.Traps.Add(trap);
                        }

                        return triggerInfo;
                    }
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
                    /// Thời gian tái sinh
                    /// </summary>
                    public int RespawnTicks { get; set; }

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
                            RespawnTicks = int.Parse(xmlNode.Attribute("RespawnTicks").Value),
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
                /// Thông tin nhiệm vụ
                /// </summary>
                public class TaskInfo
                {
                    /// <summary>
                    /// Loại nhiệm vụ
                    /// </summary>
                    public enum TaskType
                    {
                        /// <summary>
                        /// Không có
                        /// </summary>
                        None = 0,
                        /// <summary>
                        /// Thu thập nguyên liệu từ điểm thu thập
                        /// </summary>
                        CollectGrowPoints = 1,
                        /// <summary>
                        /// Giết Boss
                        /// </summary>
                        KillBoss = 2,
                        /// <summary>
                        /// Giao vật phẩm cho NPC
                        /// </summary>
                        GiveGoodsToNPC = 3,
                        /// <summary>
                        /// Mở cơ quan và tiêu diệt quái
                        /// </summary>
                        OpenTriggerAndKillMonsters = 4,
                        /// <summary>
                        /// Hộ tống NPC
                        /// </summary>
                        TransferNPC = 5,
                        /// <summary>
                        /// Mở các cơ quan theo thứ tự
                        /// </summary>
                        OpenOrderedTriggers = 6,
                        /// <summary>
                        /// Mở cơ quan
                        /// </summary>
                        OpenTrigger = 7,
                        /// <summary>
                        /// Đoán số
                        /// </summary>
                        GuessNumber = 8,
                        /// <summary>
                        /// Mở toàn bộ cơ quan cùng lúc
                        /// </summary>
                        OpenAllTriggersSameMoment = 9,
                        /// <summary>
                        /// Mở toàn bộ cơ quan và dập lửa
                        /// </summary>
                        OpenAndProtectAllTriggers = 10,
                        /// <summary>
                        /// Tổng số
                        /// </summary>
                        Count,
                    }

                    /// <summary>
                    /// Yêu cầu nhiệm vụ
                    /// </summary>
                    public class TargetInfo
                    {
                        /// <summary>
                        /// Danh sách vật phẩm yêu cầu và số lượng tương ứng
                        /// </summary>
                        public Dictionary<int, int> Items { get; set; }

                        /// <summary>
                        /// Yêu cầu toàn thể người trong đội phải thu thập đủ
                        /// </summary>
                        public bool RequireAllMembers { get; set; }

                        /// <summary>
                        /// Chuyển đối tượng từ XMLNode
                        /// </summary>
                        /// <param name="xmlNode"></param>
                        /// <returns></returns>
                        public static TargetInfo Parse(XElement xmlNode)
                        {
                            TargetInfo targetInfo = new TargetInfo()
                            {
                                Items = new Dictionary<int, int>(),
                                RequireAllMembers = bool.Parse(xmlNode.Attribute("RequireAllMembers").Value),
                            };

                            /// Chuỗi danh sách vật phẩm yêu cầu
                            string itemsString = xmlNode.Attribute("Items").Value;
                            /// Nếu tồn tại
                            if (itemsString != "-1")
                            {
                                /// Duyệt danh sách vật phẩm yêu cầu
                                foreach (string itemString in itemsString.Split(';'))
                                {
                                    /// Tách chuỗi
                                    string[] fields = itemString.Split('_');
                                    /// ID vật phẩm
                                    int itemID = int.Parse(fields[0]);
                                    /// Số lượng
                                    int quantity = int.Parse(fields[1]);
                                    /// Thêm vào danh sách
                                    targetInfo.Items[itemID] = quantity;

                                    /// Thêm vào danh sách vật phẩm sự kiện
                                    MilitaryCamp.EventItems.Add(itemID);
                                }
                            }

                            return targetInfo;
                        }
                    }

                    /// <summary>
                    /// Thông tin điểm thu thập
                    /// </summary>
                    public class GrowPointInfo
                    {
                        /// <summary>
                        /// ID điểm thu thập
                        /// </summary>
                        public int ID { get; set; }

                        /// <summary>
                        /// Tên điểm thu thập
                        /// </summary>
                        public string Name { get; set; }

                        /// <summary>
                        /// Thời gian tái sinh (đơn vị Mili-giây, -1 sẽ không tái sinh, nếu DisapearAfterBeenCollected = true thì không có tác dụng)
                        /// </summary>
                        public int RespawnTicks { get; set; }

                        /// <summary>
                        /// Thời gian mở (mili-giây)
                        /// </summary>
                        public int CollectTick { get; set; }

                        /// <summary>
                        /// Biến mất sau khi bị thu thập
                        /// </summary>
                        public bool DisapearAfterBeenCollected { get; set; }

                        /// <summary>
                        /// ID vật phẩm thu thập
                        /// </summary>
                        public int ItemID { get; set; }

                        /// <summary>
                        /// Vật phẩm thu thập sẽ khóa sau khi thu thập
                        /// </summary>
                        public bool BoundAfterBeenCollected { get; set; }

                        /// <summary>
                        /// Tổng số điểm thu thập (nếu ít hơn số lượng điểm thu thập thì sẽ chọn ngẫu nhiên trong danh sách, nếu ít hơn số lượng thì sẽ xếp đè các điểm thu thập lên nhau)
                        /// </summary>
                        public int Count { get; set; }

                        /// <summary>
                        /// Các vị trí xuất hiện
                        /// </summary>
                        public List<UnityEngine.Vector2Int> Positions { get; set; }

                        /// <summary>
                        /// Chuyển đối tượng từ XMLNode
                        /// </summary>
                        /// <param name="xmlNode"></param>
                        /// <returns></returns>
                        public static GrowPointInfo Parse(XElement xmlNode)
                        {
                            GrowPointInfo growPointInfo = new GrowPointInfo()
                            {
                                ID = int.Parse(xmlNode.Attribute("ID").Value),
                                Name = xmlNode.Attribute("Name").Value,
                                RespawnTicks = int.Parse(xmlNode.Attribute("RespawnTicks").Value),
                                CollectTick = int.Parse(xmlNode.Attribute("CollectTick").Value),
                                DisapearAfterBeenCollected = bool.Parse(xmlNode.Attribute("DisapearAfterBeenCollected").Value),
                                ItemID = int.Parse(xmlNode.Attribute("ItemID").Value),
                                BoundAfterBeenCollected = bool.Parse(xmlNode.Attribute("BoundAfterBeenCollected").Value),
                                Count = int.Parse(xmlNode.Attribute("Count").Value),
                                Positions = new List<UnityEngine.Vector2Int>(),
                            };

                            /// Duyệt danh sách vị trí
                            foreach (XElement node in xmlNode.Elements("Position"))
                            {
                                /// Thêm vào danh sách
                                growPointInfo.Positions.Add(new UnityEngine.Vector2Int(int.Parse(node.Attribute("PosX").Value), int.Parse(node.Attribute("PosY").Value)));
                            }

                            return growPointInfo;
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
                    /// Thông tin NPC
                    /// </summary>
                    public class NPCInfo
                    {
                        /// <summary>
                        /// ID
                        /// </summary>
                        public int ID { get; set; }

                        /// <summary>
                        /// Tên NPC
                        /// </summary>
                        public string Name { get; set; }

                        /// <summary>
                        /// Danh hiệu
                        /// </summary>
                        public string Title { get; set; }

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
                        public static NPCInfo Parse(XElement xmlNode)
                        {
                            return new NPCInfo()
                            {
                                ID = int.Parse(xmlNode.Attribute("ID").Value),
                                Name = xmlNode.Attribute("Name").Value,
                                Title = xmlNode.Attribute("Title").Value,
                                PosX = int.Parse(xmlNode.Attribute("PosX").Value),
                                PosY = int.Parse(xmlNode.Attribute("PosY").Value),
                            };
                        }
                    }

                    /// <summary>
                    /// Thông tin NPC di chuyển
                    /// </summary>
                    public class MovingNPCInfo
                    {
                        /// <summary>
                        /// ID
                        /// </summary>
                        public int ID { get; set; }

                        /// <summary>
                        /// Tên NPC
                        /// </summary>
                        public string Name { get; set; }

                        /// <summary>
                        /// Danh hiệu
                        /// </summary>
                        public string Title { get; set; }

                        /// <summary>
                        /// Tọa độ X
                        /// </summary>
                        public int PosX { get; set; }

                        /// <summary>
                        /// Tọa độ Y
                        /// </summary>
                        public int PosY { get; set; }

                        /// <summary>
                        /// Tọa độ đích X
                        /// </summary>
                        public int ToPosX { get; set; }

                        /// <summary>
                        /// Tọa độ đích Y
                        /// </summary>
                        public int ToPosY { get; set; }

                        /// <summary>
                        /// Bán kính hộ tống
                        /// </summary>
                        public int Radius { get; set; }

                        /// <summary>
                        /// Chuyển đối tượng từ XMLNode
                        /// </summary>
                        /// <param name="xmlNode"></param>
                        /// <returns></returns>
                        public static MovingNPCInfo Parse(XElement xmlNode)
                        {
                            return new MovingNPCInfo()
                            {
                                ID = int.Parse(xmlNode.Attribute("ID").Value),
                                Name = xmlNode.Attribute("Name").Value,
                                Title = xmlNode.Attribute("Title").Value,
                                PosX = int.Parse(xmlNode.Attribute("PosX").Value),
                                PosY = int.Parse(xmlNode.Attribute("PosY").Value),
                                ToPosX = int.Parse(xmlNode.Attribute("ToPosX").Value),
                                ToPosY = int.Parse(xmlNode.Attribute("ToPosY").Value),
                                Radius = int.Parse(xmlNode.Attribute("Radius").Value),
                            };
                        }
                    }

                    /// <summary>
                    /// Thông tin cơ quan mở theo thứ tự
                    /// </summary>
                    public class IndexTriggerInfo
                    {
                        /// <summary>
                        /// Thông tin cơ quan
                        /// </summary>
                        public class OrderedTriggerInfo
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
                            /// Thứ tự
                            /// </summary>
                            public int Order { get; set; }

                            /// <summary>
                            /// Thời gian mở
                            /// </summary>
                            public int CollectTick { get; set; }

                            /// <summary>
                            /// Chuyển đối tượng từ XMLNode
                            /// </summary>
                            /// <param name="xmlNode"></param>
                            /// <returns></returns>
                            public static OrderedTriggerInfo Parse(XElement xmlNode)
                            {
                                return new OrderedTriggerInfo()
                                {
                                    ID = int.Parse(xmlNode.Attribute("ID").Value),
                                    Name = xmlNode.Attribute("Name").Value,
                                    CollectTick = int.Parse(xmlNode.Attribute("CollectTick").Value),
                                    Order = int.Parse(xmlNode.Attribute("Order").Value),
                                };
                            }
                        }

                        /// <summary>
                        /// Danh sách vị trí
                        /// </summary>
                        public List<UnityEngine.Vector2Int> Positions { get; set; }

                        /// <summary>
                        /// Danh sách cơ quan
                        /// </summary>
                        public List<OrderedTriggerInfo> Triggers { get; set; }

                        /// <summary>
                        /// Chuyển đối tượng từ XMLNode
                        /// </summary>
                        /// <param name="xmlNode"></param>
                        /// <returns></returns>
                        public static IndexTriggerInfo Parse(XElement xmlNode)
                        {
                            IndexTriggerInfo indexTriggerInfo = new IndexTriggerInfo()
                            {
                                Positions = new List<UnityEngine.Vector2Int>(),
                                Triggers = new List<OrderedTriggerInfo>(),
                            };

                            /// Duyệt danh sách vị trí
                            foreach (XElement node in xmlNode.Element("Positions").Elements("Position"))
                            {
                                indexTriggerInfo.Positions.Add(new UnityEngine.Vector2Int(int.Parse(node.Attribute("PosX").Value), int.Parse(node.Attribute("PosY").Value)));
                            }

                            /// Duyệt danh sách cơ quan
                            foreach (XElement node in xmlNode.Elements("Trigger"))
                            {
                                indexTriggerInfo.Triggers.Add(OrderedTriggerInfo.Parse(node));
                            }

                            return indexTriggerInfo;
                        }
                    }

                    /// <summary>
                    /// Thiết lập đoán số
                    /// </summary>
                    public class GuessNumberConfigInfo
                    {
                        /// <summary>
                        /// Giá trị Min
                        /// </summary>
                        public int MinNumber { get; set; }

                        /// <summary>
                        /// Giá trị Max
                        /// </summary>
                        public int MaxNumber { get; set; }

                        /// <summary>
                        /// Số lượt đoán tối đa
                        /// </summary>
                        public int MaxTurns { get; set; }

                        /// <summary>
                        /// Chuyển đối tượng từ XMLNode
                        /// </summary>
                        /// <param name="xmlNode"></param>
                        /// <returns></returns>
                        public static GuessNumberConfigInfo Parse(XElement xmlNode)
                        {
                            return new GuessNumberConfigInfo()
                            {
                                MinNumber = int.Parse(xmlNode.Attribute("MinNumber").Value),
                                MaxNumber = int.Parse(xmlNode.Attribute("MaxNumber").Value),
                                MaxTurns = int.Parse(xmlNode.Attribute("MaxTurns").Value),
                            };
                        }
                    }

                    /// <summary>
                    /// Bảo vệ cơ quan dập lửa
                    /// </summary>
                    public class ProtectTriggerInfo
                    {
                        /// <summary>
                        /// Thông tin Thánh hỏa
                        /// </summary>
                        public class HolyFireInfo
                        {
                            /// <summary>
                            /// ID
                            /// </summary>
                            public int ID { get; set; }

                            /// <summary>
                            /// Tên
                            /// </summary>
                            public string Name { get; set; }

                            /// <summary>
                            /// Thời gian dập
                            /// </summary>
                            public int CollectTick { get; set; }

                            /// <summary>
                            /// Thời gian phát nổ
                            /// </summary>
                            public int ActivateTicks { get; set; }

                            /// <summary>
                            /// Thời gian xuất hiện liên tục
                            /// </summary>
                            public int SpawnEveryTicks { get; set; }

                            /// <summary>
                            /// Phạm vi sinh xung quanh cơ quan (tối thiểu)
                            /// </summary>
                            public int RandomRadiusMin { get; set; }

                            /// <summary>
                            /// Phạm vi sinh xung quanh cơ quan (tối đa)
                            /// </summary>
                            public int RandomRadiusMax { get; set; }

                            /// <summary>
                            /// Chuyển đối tượng từ XMLNode
                            /// </summary>
                            /// <param name="xmlNode"></param>
                            /// <returns></returns>
                            public static HolyFireInfo Parse(XElement xmlNode)
                            {
                                return new HolyFireInfo()
                                {
                                    ID = int.Parse(xmlNode.Attribute("ID").Value),
                                    Name = xmlNode.Attribute("Name").Value,
                                    CollectTick = int.Parse(xmlNode.Attribute("CollectTick").Value),
                                    ActivateTicks = int.Parse(xmlNode.Attribute("ActivateTicks").Value),
                                    SpawnEveryTicks = int.Parse(xmlNode.Attribute("SpawnEveryTicks").Value),
                                    RandomRadiusMin = int.Parse(xmlNode.Attribute("RandomRadiusMin").Value),
                                    RandomRadiusMax = int.Parse(xmlNode.Attribute("RandomRadiusMax").Value),
                                };
                            }
                        }

                        /// <summary>
                        /// Thánh hỏa
                        /// </summary>
                        public HolyFireInfo HolyFire { get; set; }

                        /// <summary>
                        /// Danh sách cơ quan
                        /// </summary>
                        public List<TriggerInfo> Triggers { get; set; }

                        /// <summary>
                        /// Chuyển đối tượng từ XMLNode
                        /// </summary>
                        /// <param name="xmlNode"></param>
                        /// <returns></returns>
                        public static ProtectTriggerInfo Parse(XElement xmlNode)
                        {
                            ProtectTriggerInfo protectTriggerInfo = new ProtectTriggerInfo()
                            {
                                HolyFire = HolyFireInfo.Parse(xmlNode.Element("HolyFire")),
                                Triggers = new List<TriggerInfo>(),
                            };

                            /// Duyệt danh sách cơ quan
                            foreach (XElement node in xmlNode.Elements("Trigger"))
                            {
                                protectTriggerInfo.Triggers.Add(TriggerInfo.Parse(node));
                            }


                            return protectTriggerInfo;
                        }
                    }

                    /// <summary>
                    /// ID nhiệm vụ
                    /// </summary>
                    public int ID { get; set; }

                    /// <summary>
                    /// Danh sách ID nhiệm vụ cần hoàn thành trước đó
                    /// </summary>
                    public List<int> RequireTasks { get; set; }

                    /// <summary>
                    /// Tên nhiệm vụ
                    /// </summary>
                    public string Name { get; set; }

                    /// <summary>
                    /// Loại nhiệm vụ
                    /// </summary>
                    public TaskType Type { get; set; }

                    /// <summary>
                    /// Mục tiêu nhiệm vụ (chỉ có tác dụng với loại nhiệm vụ thu thập nguyên liệu, giao vật phẩm cho NPC)
                    /// </summary>
                    public TargetInfo Target { get; set; }

                    /// <summary>
                    /// Danh sách điểm thu thập (chỉ có tác dụng với loại nhiệm vụ thu thập nguyên liệu)
                    /// </summary>
                    public GrowPointInfo GrowPoints { get; set; }

                    /// <summary>
                    /// Boss
                    /// </summary>
                    public BossInfo Boss { get; set; }

                    /// <summary>
                    /// Boss phụ
                    /// </summary>
                    public BossInfo ChildBoss { get; set; }

                    /// <summary>
                    /// Danh sách quái
                    /// </summary>
                    public List<MonsterInfo> Monsters { get; set; }

                    /// <summary>
                    /// Cơ quan
                    /// </summary>
                    public TriggerInfo Trigger { get; set; }

                    /// <summary>
                    /// NPC
                    /// </summary>
                    public NPCInfo NPC { get; set; }
                    
                    /// <summary>
                    /// NPC di chuyển
                    /// </summary>
                    public MovingNPCInfo MovingNPC { get; set; }

                    /// <summary>
                    /// Cơ quan mở theo thứ tự
                    /// </summary>
                    public IndexTriggerInfo IndexTriggers { get; set; }

                    /// <summary>
                    /// Thiết lập sự kiện đoán số
                    /// </summary>
                    public GuessNumberConfigInfo GuessNumberConfig { get; set; }

                    /// <summary>
                    /// Bảo vệ cơ quan
                    /// </summary>
                    public ProtectTriggerInfo ProtectTrigger { get; set; }

                    /// <summary>
                    /// Chuyển đối tượng từ XMLNode
                    /// </summary>
                    /// <param name="xmlNode"></param>
                    /// <returns></returns>
                    public static TaskInfo Parse(XElement xmlNode)
                    {
                        TaskInfo taskInfo = new TaskInfo()
                        {
                            ID = int.Parse(xmlNode.Attribute("ID").Value),
                            Name = xmlNode.Attribute("Name").Value,
                            Type = (TaskType) int.Parse(xmlNode.Attribute("Type").Value),
                            RequireTasks = new List<int>(),
                        };

                        /// Danh sách nhiệm vụ trước đó yêu cầu
                        foreach (string requireTaskString in xmlNode.Attribute("RequireTasks").Value.Split(';'))
                        {
                            /// ID nhiệm vụ tương ứng
                            int taskID = int.Parse(requireTaskString);
                            /// Nếu có nhiệm vụ
                            if (taskID > 0)
                            {
                                /// Thêm vào danh sách
                                taskInfo.RequireTasks.Add(taskID);
                            }
                        }

                        /// Loại nhiệm vụ
                        switch (taskInfo.Type)
                        {
                            /// Thu thập
                            case TaskType.CollectGrowPoints:
                            {
                                taskInfo.Target = TargetInfo.Parse(xmlNode.Element("Target"));
                                taskInfo.GrowPoints = GrowPointInfo.Parse(xmlNode.Element("GrowPoints"));
                                break;
                            }
                            /// Giết Boss
                            case TaskType.KillBoss:
                            {
                                taskInfo.Boss = BossInfo.Parse(xmlNode.Element("Boss"));
                                /// Nếu có Boss con
                                if (xmlNode.Element("ChildBoss") != null)
                                {
                                    taskInfo.ChildBoss = BossInfo.Parse(xmlNode.Element("ChildBoss"));
                                }
                                break;
                            }
                            /// Giao vật phẩm cho NPC
                            case TaskType.GiveGoodsToNPC:
                            {
                                taskInfo.Target = TargetInfo.Parse(xmlNode.Element("Target"));
                                taskInfo.NPC = NPCInfo.Parse(xmlNode.Element("NPC"));
                                break;
                            }
                            /// Mở cơ quan tiêu diệt quái
                            case TaskType.OpenTriggerAndKillMonsters:
                            {
                                taskInfo.Trigger = TriggerInfo.Parse(xmlNode.Element("Trigger"));
                                taskInfo.Monsters = new List<MonsterInfo>();
                                /// Duyệt danh sách quái
                                foreach (XElement node in xmlNode.Element("Monsters").Elements("Monster"))
                                {
                                    taskInfo.Monsters.Add(MonsterInfo.Parse(node));
                                }
                                break;
                            }
                            /// Hộ tống NPC
                            case TaskType.TransferNPC:
                            {
                                taskInfo.MovingNPC = MovingNPCInfo.Parse(xmlNode.Element("MovingNPC"));
                                break;
                            }
                            /// Mở toàn bộ cơ quan theo thứ tự
                            case TaskType.OpenOrderedTriggers:
                            {
                                taskInfo.IndexTriggers = IndexTriggerInfo.Parse(xmlNode.Element("IndexTriggers"));
                                break;
                            }
                            /// Mở cơ quan
                            case TaskType.OpenTrigger:
                            {
                                taskInfo.Trigger = TriggerInfo.Parse(xmlNode.Element("Trigger"));
                                break;
                            }
                            /// Đoán số
                            case TaskType.GuessNumber:
                            {
                                taskInfo.GuessNumberConfig = GuessNumberConfigInfo.Parse(xmlNode.Element("Config"));
                                taskInfo.NPC = NPCInfo.Parse(xmlNode.Element("NPC"));
                                break;
                            }
                            /// Mở toàn bộ cơ quan cùng lúc
                            case TaskType.OpenAllTriggersSameMoment:
                            {
                                taskInfo.IndexTriggers = IndexTriggerInfo.Parse(xmlNode.Element("IndexTriggers"));
                                break;
                            }
                            /// Mở cơ quan, dập lửa
                            case TaskType.OpenAndProtectAllTriggers:
                            {
                                taskInfo.ProtectTrigger = ProtectTriggerInfo.Parse(xmlNode.Element("ProtectTriggers"));
                                break;
                            }
                        }


                        return taskInfo;
                    }
                }

                /// <summary>
                /// Danh sách cơ quan
                /// </summary>
                public TriggerData Triggers { get; set; }

                /// <summary>
                /// Danh sách nhiệm vụ
                /// </summary>
                public List<TaskInfo> Tasks { get; set; }

                /// <summary>
                /// Danh sách quái
                /// </summary>
                public List<MonsterInfo> Monsters { get; set; }

                /// <summary>
                /// Danh sách cổng dịch chuyển
                /// </summary>
                public List<TeleportInfo> Teleports { get; set; }

                /// <summary>
                /// Chuyển đối tượng từ XMLNode
                /// </summary>
                /// <param name="xmlNode"></param>
                /// <returns></returns>
                public static StageInfo Parse(XElement xmlNode)
                {
                    StageInfo stageInfo = new StageInfo()
                    {
                        Triggers = TriggerData.Parse(xmlNode.Element("Triggers")),
                        Tasks = new List<TaskInfo>(),
                        Monsters = new List<MonsterInfo>(),
                        Teleports = new List<TeleportInfo>(),
                    };

                    /// Duyệt danh sách nhiệm vụ
                    foreach (XElement node in xmlNode.Element("Tasks").Elements("Task"))
                    {
                        TaskInfo taskInfo = TaskInfo.Parse(node);
                        stageInfo.Tasks.Add(taskInfo);
                    }
                    /// Sắp xếp tăng dần theo ID
                    stageInfo.Tasks.Sort((x, y) => x.ID - y.ID);

                    /// Duyệt danh sách quái
                    foreach (XElement node in xmlNode.Element("Monsters").Elements("Monster"))
                    {
                        stageInfo.Monsters.Add(MonsterInfo.Parse(node));
                    }

                    /// Duyệt danh sách cổng dịch chuyển
                    foreach (XElement node in xmlNode.Element("Teleports").Elements("Teleport"))
                    {
                        stageInfo.Teleports.Add(TeleportInfo.Parse(node));
                    }

                    return stageInfo;
                }
            }

            /// <summary>
            /// Tên phụ bản
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Thời gian tồn tại phụ bản
            /// </summary>
            public int Duration { get; set; }

            /// <summary>
            /// Loại phụ bản
            /// </summary>
            public EventType Type { get; set; }

            /// <summary>
            /// Thiết lập sự kiện
            /// </summary>
            public EventConfig Config { get; set; }

            /// <summary>
            /// Danh sách NPC
            /// </summary>
            public List<NPCInfo> NPCs { get; set; }

            /// <summary>
            /// Danh sách tầng
            /// </summary>
            public List<StageInfo> Stages { get; set; }

            /// <summary>
            /// Chuyển đối tượng từ XMLNode
            /// </summary>
            /// <param name="xmlNode"></param>
            /// <returns></returns>
            public static EventInfo Parse(XElement xmlNode)
            {
                EventInfo eventInfo = new EventInfo()
                {
                    Name = xmlNode.Attribute("Name").Value,
                    Duration = int.Parse(xmlNode.Attribute("Duration").Value),
                    Config = EventConfig.Parse(xmlNode.Element("Config")),
                    NPCs = new List<NPCInfo>(),
                    Stages = new List<StageInfo>(),
                };

                /// Duyệt danh sách NPC
                foreach (XElement node in xmlNode.Element("NPCs").Elements("NPC"))
                {
                    eventInfo.NPCs.Add(NPCInfo.Parse(node));
                }
                
                /// Duyệt danh sách tầng
                foreach (XElement node in xmlNode.Elements("Stage"))
                {
                    eventInfo.Stages.Add(StageInfo.Parse(node));
                }

                return eventInfo;
            }
        }
        #endregion

        #region Core
        /// <summary>
        /// Điều kiện tham gia
        /// </summary>
        public static EventCondition Condition { get; set; }

        /// <summary>
        /// Phụ bản Hậu Sơn Phục Ngưu
        /// </summary>
        public static EventInfo FootHills { get; set; }

        /// <summary>
        /// Phụ bản Bách Man Sơn
        /// </summary>
        public static EventInfo MountainPeak { get; set; }

        /// <summary>
        /// Phụ bản Hải Lăng Vương Mộ
        /// </summary>
        public static EventInfo RoyalTomb { get; set; }

        /// <summary>
        /// Danh sách vật phẩm sự kiện
        /// </summary>
        public static HashSet<int> EventItems { get; } = new HashSet<int>();

        /// <summary>
        /// Khởi tạo phụ bản Quân doanh
        /// </summary>
        public static void Init()
        {
            /// Xóa danh sách vật phẩm yêu cầu
            MilitaryCamp.EventItems.Clear();

            XElement xmlNode = KTGlobal.ReadXMLData("Config/KT_CopyScenes/MilitaryCamp.xml");
            /// Đọc dữ liệu thiết lập
			MilitaryCamp.Condition = EventCondition.Parse(xmlNode.Element("Condition"));
            /// Đọc dữ liệu phụ bản Hậu Sơn Phục Ngưu
			MilitaryCamp.FootHills = EventInfo.Parse(xmlNode.Element("FootHills"));
            MilitaryCamp.FootHills.Type = EventType.FootHills;
            /// Đọc dữ liệu phụ bản Bách Man Sơn
			MilitaryCamp.MountainPeak = EventInfo.Parse(xmlNode.Element("MountainPeak"));
            MilitaryCamp.MountainPeak.Type = EventType.MountainPeak;
            /// Đọc dữ liệu phụ bản Hải Lăng Vương Mộ
			MilitaryCamp.RoyalTomb = EventInfo.Parse(xmlNode.Element("RoyalTomb"));
            MilitaryCamp.RoyalTomb.Type = EventType.RoyalTomb;
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Xóa toàn bộ vật phẩm sự kiện
        /// </summary>
        public static void RemoveAllEventItems(KPlayer player)
        {
            /// Danh sách vật phẩm sự kiện hiện có
            List<GoodsData> eventItems = player.GoodsData.FindAll(x => MilitaryCamp.EventItems.Contains(x.GoodsID)).ToList();
            /// Duyệt danh sách vật phẩm sự kiện đang có
            foreach (GoodsData itemGD in eventItems)
            {
                /// Xóa vật phẩm
                ItemManager.AbandonItem(itemGD, player, false);
            }
        }
        #endregion
    }
}
