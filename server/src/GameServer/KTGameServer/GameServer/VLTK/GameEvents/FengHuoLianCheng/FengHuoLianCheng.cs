using GameServer.KiemThe.Entities;
using GameServer.Logic;
using Server.Tools;
using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace GameServer.KiemThe.GameEvents.FengHuoLianCheng
{
    /// <summary>
    /// Sự kiện Phong Hỏa Liên Thành
    /// </summary>
    public static class FengHuoLianCheng
    {
        #region Define
        /// <summary>
        /// Dữ liệu sự kiện Phong Hỏa Liên Thành
        /// </summary>
        public class FHLCData
        {
            /// <summary>
            /// Thiết lập sự kiện
            /// </summary>
            public class EventConfig
            {
                /// <summary>
                /// Cấp độ yêu cầu
                /// </summary>
                public int RequireLevel { get; set; }

                /// <summary>
                /// Số người tham gia tối thiểu
                /// </summary>
                public int MinPlayers { get; set; }

                /// <summary>
                /// Số người tham gia tối đa
                /// </summary>
                public int MaxPlayers { get; set; }

                /// <summary>
                /// Chuyển đối tượng từ XMLNode
                /// </summary>
                /// <param name="xmlNode"></param>
                /// <returns></returns>
                public static EventConfig Parse(XElement xmlNode)
                {
                    return new EventConfig()
                    {
                        RequireLevel = int.Parse(xmlNode.Attribute("RequireLevel").Value),
                        MinPlayers = int.Parse(xmlNode.Attribute("MinPlayers").Value),
                        MaxPlayers = int.Parse(xmlNode.Attribute("MaxPlayers").Value),
                    };
                }
            }

            /// <summary>
            /// Thông tin bản đồ sự kiện
            /// </summary>
            public class EventMapData
            {
                /// <summary>
                /// ID bản đồ sự kiện
                /// </summary>
                public int EnterMapID { get; set; }

                /// <summary>
                /// Tọa độ X tiến vào (cũng là vị trí hồi sinh khi chết)
                /// </summary>
                public int EnterPosX { get; set; }

                /// <summary>
                /// Tọa độ Y tiến vào (cũng là vị trí hồi sinh khi chết)
                /// </summary>
                public int EnterPosY { get; set; }

                /// <summary>
                /// ID bản đồ thành thị
                /// </summary>
                public int CityMapID { get; set; }

                /// <summary>
                /// Tọa độ X ở thành thị khi thoát sự kiện
                /// </summary>
                public int CityPosX { get; set; }

                /// <summary>
                /// Tọa độ Y ở thành thị khi thoát sự kiện
                /// </summary>
                public int CityPosY { get; set; }

                /// <summary>
                /// Chuyển đối tượng từ XMLNode
                /// </summary>
                /// <param name="xmlNode"></param>
                /// <returns></returns>
                public static EventMapData Parse(XElement xmlNode)
                {
                    return new EventMapData()
                    {
                        EnterMapID = int.Parse(xmlNode.Attribute("EnterMapID").Value),
                        EnterPosX = int.Parse(xmlNode.Attribute("EnterPosX").Value),
                        EnterPosY = int.Parse(xmlNode.Attribute("EnterPosY").Value),
                        CityMapID = int.Parse(xmlNode.Attribute("CityMapID").Value),
                        CityPosX = int.Parse(xmlNode.Attribute("CityPosX").Value),
                        CityPosY = int.Parse(xmlNode.Attribute("CityPosY").Value),
                    };
                }
            }

            /// <summary>
            /// Điểm tích lũy trong sự kiện
            /// </summary>
            public class EventPointInfo
            {
                /// <summary>
                /// Thời gian bảo vệ nguyên soái mỗi lần tích tăng điểm (Mili-giây)
                /// </summary>
                public int ProtectMarshalAwardPeriod { get; set; }

                /// <summary>
                /// Số điểm tích lũy có được mỗi lần bảo vệ nguyên soái
                /// </summary>
                public int ProtectMarshalAwardPoint { get; set; }

                /// <summary>
                /// Số điểm có được khi giết quái
                /// </summary>
                public int KillMonsterPoint { get; set; }

                /// <summary>
                /// Số điểm có được khi giết xe công thành
                /// </summary>
                public int KillSiegePoint { get; set; }

                /// <summary>
                /// Số điểm các thành viên khác trong đội có được khi giết xe công thành
                /// </summary>
                public int KillSiegeTeamPoint { get; set; }

                /// <summary>
                /// Số điểm có được khi giết Boss
                /// </summary>
                public int KillBossPoint { get; set; }

                /// <summary>
                /// Số điểm các thành viên khác trong đội có được khi giết Boss
                /// </summary>
                public int KillBossTeamPoint { get; set; }

                /// <summary>
                /// Số điểm toàn bộ người chơi khác có được khi giết Boss
                /// </summary>
                public int KillBossAllPoint { get; set; }

                /// <summary>
                /// Chuyển đối tượng từ XMLNode
                /// </summary>
                /// <param name="xmlNode"></param>
                /// <returns></returns>
                public static EventPointInfo Parse(XElement xmlNode)
                {
                    return new EventPointInfo()
                    {
                        ProtectMarshalAwardPeriod = int.Parse(xmlNode.Attribute("ProtectMarshalAwardPeriod").Value),
                        ProtectMarshalAwardPoint = int.Parse(xmlNode.Attribute("ProtectMarshalAwardPoint").Value),
                        KillMonsterPoint = int.Parse(xmlNode.Attribute("KillMonsterPoint").Value),
                        KillSiegePoint = int.Parse(xmlNode.Attribute("KillSiegePoint").Value),
                        KillSiegeTeamPoint = int.Parse(xmlNode.Attribute("KillSiegeTeamPoint").Value),
                        KillBossPoint = int.Parse(xmlNode.Attribute("KillBossPoint").Value),
                        KillBossTeamPoint = int.Parse(xmlNode.Attribute("KillBossTeamPoint").Value),
                        KillBossAllPoint = int.Parse(xmlNode.Attribute("KillBossAllPoint").Value),
                    };
                }
            }

            /// <summary>
            /// Thông tin phần thưởng
            /// </summary>
            public class AwardInfo
            {
                /// <summary>
                /// Thông tin vật phẩm thưởng
                /// </summary>
                public class AwardItemData
                {
                    /// <summary>
                    /// ID vật phẩm
                    /// </summary>
                    public int ItemID { get; set; }

                    /// <summary>
                    /// Số lượng
                    /// </summary>
                    public int Quantity { get; set; }

                    /// <summary>
                    /// Nhận khóa hay không
                    /// </summary>
                    public bool Bound { get; set; }

                    /// <summary>
                    /// Chuyển đối tượng từ XMLNode
                    /// </summary>
                    /// <param name="xmlNode"></param>
                    /// <returns></returns>
                    public static AwardItemData Parse(XElement xmlNode)
                    {
                        return new AwardItemData()
                        {
                            ItemID = int.Parse(xmlNode.Attribute("ItemID").Value),
                            Quantity = int.Parse(xmlNode.Attribute("Quantity").Value),
                            Bound = bool.Parse(xmlNode.Attribute("Bound").Value),
                        };
                    }
                }

                /// <summary>
                /// Từ thứ hạng
                /// </summary>
                public int FromRank { get; set; }

                /// <summary>
                /// Đến thứ hạng
                /// </summary>
                public int ToRank { get; set; }

                /// <summary>
                /// ID danh vọng
                /// </summary>
                public int ReputeID { get; set; }

                /// <summary>
                /// Số điểm danh vọng
                /// </summary>
                public int ReputePoint { get; set; }

                /// <summary>
                /// Danh sách vật phẩm thưởng
                /// </summary>
                public List<AwardItemData> AwardItems { get; set; }

                /// <summary>
                /// Chuyển đối tượng từ XMLNode
                /// </summary>
                /// <param name="xmlNode"></param>
                /// <returns></returns>
                public static AwardInfo Parse(XElement xmlNode)
                {
                    /// Tạo mới
                    AwardInfo awardInfo = new AwardInfo()
                    {
                        FromRank = int.Parse(xmlNode.Attribute("FromRank").Value),
                        ToRank = int.Parse(xmlNode.Attribute("ToRank").Value),
                        ReputeID = int.Parse(xmlNode.Attribute("ReputeID").Value),
                        ReputePoint = int.Parse(xmlNode.Attribute("ReputePoint").Value),
                        AwardItems = new List<AwardItemData>(),
                    };

                    /// Duyệt danh sách phần thưởng vật phẩm
                    foreach (XElement node in xmlNode.Elements("AwardItem"))
                    {
                        /// Thêm vào danh sách
                        awardInfo.AwardItems.Add(AwardItemData.Parse(node));
                    }

                    /// Trả về kết quả
                    return awardInfo;
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
            /// Thông tin vị trí nhóm
            /// </summary>
            public class GroupPositionData
            {
                /// <summary>
                /// Thông tin vị trí
                /// </summary>
                public class Position
                {
                    /// <summary>
                    /// Tọa độ X
                    /// </summary>
                    public int PosX { get; set; }

                    /// <summary>
                    /// Tọa độ Y
                    /// </summary>
                    public int PosY { get; set; }

                    /// <summary>
                    /// Bán kính xuất hiện
                    /// </summary>
                    public int Radius { get; set; }

                    /// <summary>
                    /// Chuyển đối tượng từ XMLNode
                    /// </summary>
                    /// <param name="xmlNode"></param>
                    /// <returns></returns>
                    public static Position Parse(XElement xmlNode)
                    {
                        return new Position()
                        {
                            PosX = int.Parse(xmlNode.Attribute("PosX").Value),
                            PosY = int.Parse(xmlNode.Attribute("PosY").Value),
                            Radius = int.Parse(xmlNode.Attribute("Radius").Value),
                        };
                    }
                }

                /// <summary>
                /// ID nhóm
                /// </summary>
                public int ID { get; set; }

                /// <summary>
                /// Vị trí đích đến X
                /// </summary>
                public int DestPosX { get; set; }

                /// <summary>
                /// Vị trí đích đến Y
                /// </summary>
                public int DestPosY { get; set; }

                /// <summary>
                /// Các vị trí xuất hiện
                /// </summary>
                public List<Position> Positions { get; set; }

                /// <summary>
                /// Chuyển đối tượng từ XMLNode
                /// </summary>
                /// <param name="xmlNode"></param>
                /// <returns></returns>
                public static GroupPositionData Parse(XElement xmlNode)
                {
                    /// Tạo mới
                    GroupPositionData data = new GroupPositionData()
                    {
                        ID = int.Parse(xmlNode.Attribute("ID").Value),
                        DestPosX = int.Parse(xmlNode.Attribute("DestPosX").Value),
                        DestPosY = int.Parse(xmlNode.Attribute("DestPosY").Value),
                        Positions = new List<Position>(),
                    };

                    /// Duyệt danh sách vị trí xuất hiện
                    foreach (XElement node in xmlNode.Elements("Position"))
                    {
                        /// Thêm vào danh sách
                        data.Positions.Add(Position.Parse(node));
                    }

                    /// Trả về kết quả
                    return data;
                }
            }

            /// <summary>
            /// Thông tin Boss
            /// </summary>
            public class BossInfo
            {
                /// <summary>
                /// Thứ tự
                /// </summary>
                public int Order { get; set; }

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
                        Order = int.Parse(xmlNode.Attribute("OrderID").Value),
                        ID = int.Parse(xmlNode.Attribute("ID").Value),
                        Name = xmlNode.Attribute("Name").Value,
                        Title = xmlNode.Attribute("Title").Value,
                        PosX = xmlNode.Attribute("PosX") == null ? 0 : int.Parse(xmlNode.Attribute("PosX").Value),
                        PosY = xmlNode.Attribute("PosY") == null ? 0 : int.Parse(xmlNode.Attribute("PosY").Value),
                        BaseHP = int.Parse(xmlNode.Attribute("BaseHP").Value),
                        HPIncreaseEachLevel = int.Parse(xmlNode.Attribute("HPIncreaseEachLevel").Value),
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
                /// Thứ tự
                /// </summary>
                public int Order { get; set; }

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
                        Order = int.Parse(xmlNode.Attribute("OrderID").Value),
                        ID = int.Parse(xmlNode.Attribute("ID").Value),
                        Name = xmlNode.Attribute("Name").Value,
                        Title = xmlNode.Attribute("Title").Value,
                        PosX = xmlNode.Attribute("PosX") == null ? 0 : int.Parse(xmlNode.Attribute("PosX").Value),
                        PosY = xmlNode.Attribute("PosY") == null ? 0 : int.Parse(xmlNode.Attribute("PosY").Value),
                        BaseHP = int.Parse(xmlNode.Attribute("BaseHP").Value),
                        HPIncreaseEachLevel = int.Parse(xmlNode.Attribute("HPIncreaseEachLevel").Value),
                        AIScriptID = int.Parse(xmlNode.Attribute("AIScriptID").Value),
                        RespawnTicks = xmlNode.Attribute("RespawnTicks") == null ? -1 : int.Parse(xmlNode.Attribute("RespawnTicks").Value),
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
                    };
                }
            }

            /// <summary>
            /// Dữ liệu phe công thành
            /// </summary>
            public class AttackerData
            {
                /// <summary>
                /// Danh sách quái
                /// </summary>
                public List<MonsterInfo> Monsters { get; set; }

                /// <summary>
                /// Danh sách boss
                /// </summary>
                public List<BossInfo> Bosses { get; set; }

                /// <summary>
                /// Danh sách xe công thành
                /// </summary>
                public List<MonsterInfo> Sieges { get; set; }

                /// <summary>
                /// Chuyển đối tượng từ XMLNode
                /// </summary>
                /// <param name="xmlNode"></param>
                /// <returns></returns>
                public static AttackerData Parse(XElement xmlNode)
                {
                    /// Tọa mới
                    AttackerData data = new AttackerData()
                    {
                        Bosses = new List<BossInfo>(),
                        Monsters = new List<MonsterInfo>(),
                        Sieges = new List<MonsterInfo>(),
                    };

                    /// Duyệt danh sách quái
                    foreach (XElement node in xmlNode.Elements("Monster"))
                    {
                        /// Thêm vào danh sách
                        data.Monsters.Add(MonsterInfo.Parse(node));
                    }

                    /// Duyệt danh sách Boss
                    foreach (XElement node in xmlNode.Elements("Boss"))
                    {
                        /// Thêm vào danh sách
                        data.Bosses.Add(BossInfo.Parse(node));
                    }

                    /// Duyệt danh sách xe công thành
                    foreach (XElement node in xmlNode.Elements("Siege"))
                    {
                        /// Thêm vào danh sách
                        data.Sieges.Add(MonsterInfo.Parse(node));
                    }

                    /// Trả về kết quả
                    return data;
                }
            }

            /// <summary>
            /// Dữ liệu phe thủ thành
            /// </summary>
            public class DefenderData
            {
                /// <summary>
                /// Danh sách quái thủ thành
                /// </summary>
                public List<MonsterInfo> Monsters { get; set; }

                /// <summary>
                /// Danh sách nguyên soái thủ thành
                /// </summary>
                public List<BossInfo> Marshals { get; set; }

                /// <summary>
                /// Chuyển đối tượng từ XMLNode
                /// </summary>
                /// <param name="xmlNode"></param>
                /// <returns></returns>
                public static DefenderData Parse(XElement xmlNode)
                {
                    /// Tạo mới
                    DefenderData data = new DefenderData()
                    {
                        Marshals = new List<BossInfo>(),
                        Monsters = new List<MonsterInfo>(),
                    };

                    /// Duyệt danh sách quái
                    foreach (XElement node in xmlNode.Elements("Monster"))
                    {
                        /// Thêm vào danh sách
                        data.Monsters.Add(MonsterInfo.Parse(node));
                    }

                    /// Duyệt danh sách Boss
                    foreach (XElement node in xmlNode.Elements("Marshal"))
                    {
                        /// Thêm vào danh sách
                        data.Marshals.Add(BossInfo.Parse(node));
                    }

                    /// Trả về kết quả
                    return data;
                }
            }

            /// <summary>
            /// Thông tin các lượt tấn công
            /// </summary>
            public class RoundInfo
            {
                /// <summary>
                /// Thông tin lượt của từng nhóm
                /// </summary>
                public class RoundData
                {
                    /// <summary>
                    /// Danh sách quái
                    /// </summary>
                    public List<KeyValuePair<int, int>> Monsters { get; set; }

                    /// <summary>
                    /// Danh sách xe công thành
                    /// </summary>
                    public List<KeyValuePair<int, int>> Sieges { get; set; }

                    /// <summary>
                    /// Danh sách Boss
                    /// </summary>
                    public List<KeyValuePair<int, int>> Bosses { get; set; }

                    /// <summary>
                    /// Thời gian delay xuất hiện
                    /// </summary>
                    public int DelaySpawn { get; set; }

                    /// <summary>
                    /// Chuyển đối tượng từ XMLNode
                    /// </summary>
                    /// <param name="xmlNode"></param>
                    /// <returns></returns>
                    public static RoundData Parse(XElement xmlNode)
                    {
                        /// Tạo mới
                        RoundData data = new RoundData()
                        {
                            Monsters = new List<KeyValuePair<int, int>>(),
                            Sieges = new List<KeyValuePair<int, int>>(),
                            Bosses = new List<KeyValuePair<int, int>>(),
                            DelaySpawn = int.Parse(xmlNode.Attribute("DelaySpawn").Value),
                        };

                        /// Quái
                        {
                            /// Chuỗi thông tin
                            string listString = xmlNode.Attribute("Monsters").Value;
                            /// Nếu tồn tại
                            if (!string.IsNullOrEmpty(listString))
                            {
                                /// Duyệt danh sách
                                foreach (string str in listString.Split(';'))
                                {
                                    /// Danh sách trường
                                    string[] fields = str.Split('_');
                                    /// Thứ tự
                                    int order = int.Parse(fields[0]);
                                    /// Số lượng
                                    int count = int.Parse(fields[1]);
                                    /// Thêm vào danh sách
                                    data.Monsters.Add(new KeyValuePair<int, int>(order, count));
                                }
                            }
                        }

                        /// Boss
                        {
                            /// Chuỗi thông tin
                            string listString = xmlNode.Attribute("Bosses").Value;
                            /// Nếu tồn tại
                            if (!string.IsNullOrEmpty(listString))
                            {
                                /// Duyệt danh sách
                                foreach (string str in listString.Split(';'))
                                {
                                    /// Danh sách trường
                                    string[] fields = str.Split('_');
                                    /// Thứ tự
                                    int order = int.Parse(fields[0]);
                                    /// Số lượng
                                    int count = int.Parse(fields[1]);
                                    /// Thêm vào danh sách
                                    data.Bosses.Add(new KeyValuePair<int, int>(order, count));
                                }
                            }
                        }

                        /// Xe công thành
                        {
                            /// Chuỗi thông tin
                            string listString = xmlNode.Attribute("Sieges").Value;
                            /// Nếu tồn tại
                            if (!string.IsNullOrEmpty(listString))
                            {
                                /// Duyệt danh sách
                                foreach (string str in listString.Split(';'))
                                {
                                    /// Danh sách trường
                                    string[] fields = str.Split('_');
                                    /// Thứ tự
                                    int order = int.Parse(fields[0]);
                                    /// Số lượng
                                    int count = int.Parse(fields[1]);
                                    /// Thêm vào danh sách
                                    data.Sieges.Add(new KeyValuePair<int, int>(order, count));
                                }
                            }
                        }

                        /// Trả về kết quả
                        return data;
                    }
                }

                /// <summary>
                /// Số lượng thêm vào với mỗi khoảng người chơi tương ứng
                /// </summary>
                public class AdditionByPlayersData
                {
                    /// <summary>
                    /// Quái
                    /// </summary>
                    public int Monsters { get; set; }

                    /// <summary>
                    /// Xe công thành
                    /// </summary>
                    public int Sieges { get; set; }

                    /// <summary>
                    /// Boss
                    /// </summary>
                    public int Bosses { get; set; }

                    /// <summary>
                    /// Chuyển đối tượng từ XMLNode
                    /// </summary>
                    /// <param name="xmlNode"></param>
                    /// <returns></returns>
                    public static AdditionByPlayersData Parse(XElement xmlNode)
                    {
                        return new AdditionByPlayersData()
                        {
                            Monsters = int.Parse(xmlNode.Attribute("Monsters").Value),
                            Sieges = int.Parse(xmlNode.Attribute("Sieges").Value),
                            Bosses = int.Parse(xmlNode.Attribute("Bosses").Value),
                        };
                    }
                }

                /// <summary>
                /// Thời gian giãn cách giữa các lượt tấn công
                /// </summary>
                public int RoundPeriod { get; set; }

                /// <summary>
                /// Danh sách các lượt
                /// </summary>
                public List<RoundData> Rounds { get; set; }

                /// <summary>
                /// Số lượng thêm vào tùy theo số khoảng người chơi
                /// </summary>
                public AdditionByPlayersData Addition { get; set; }

                /// <summary>
                /// Chuyển đối tượng từ XMLNode
                /// </summary>
                /// <param name="xmlNode"></param>
                /// <returns></returns>
                public static RoundInfo Parse(XElement xmlNode)
                {
                    /// Tạo mới
                    RoundInfo roundInfo = new RoundInfo()
                    {
                        RoundPeriod = int.Parse(xmlNode.Attribute("RoundPeriod").Value),
                        Addition = AdditionByPlayersData.Parse(xmlNode.Element("AdditionByPlayers")),
                        Rounds = new List<RoundData>(),
                    };

                    /// Duyệt danh sách các lượt
                    foreach (XElement node in xmlNode.Elements("Round"))
                    {
                        /// Thêm vào
                        roundInfo.Rounds.Add(RoundData.Parse(node));
                    }

                    /// Trả về kết quả
                    return roundInfo;
                }
            }

            /// <summary>
            /// Thiết lập sự kiện
            /// </summary>
            public EventConfig Config { get; set; }

            /// <summary>
            /// Thông tin bản đồ sự kiện
            /// </summary>
            public EventMapData Map { get; set; }

            /// <summary>
            /// Thiết lập điểm tích lũy sự kiện
            /// </summary>
            public EventPointInfo EventPointConfig { get; set; }

            /// <summary>
            /// Danh sách phần thưởng sự kiện
            /// </summary>
            public List<AwardInfo> Awards { get; set; }

            /// <summary>
            /// Danh sách NPC
            /// </summary>
            public List<NPCInfo> NPCs { get; set; }

            /// <summary>
            /// Danh sách vị trí các nhóm
            /// </summary>
            public List<GroupPositionData> GroupPositions { get; set; }

            /// <summary>
            /// Danh sách cổng dịch chuyển
            /// </summary>
            public List<TeleportInfo> Teleports { get; set; }

            /// <summary>
            /// Thông tin phe công thành
            /// </summary>
            public AttackerData Attacker { get; set; }

            /// <summary>
            /// Thông tin phe thủ thành
            /// </summary>
            public DefenderData Defender { get; set; }

            /// <summary>
            /// Danh sách các lượt công thành
            /// </summary>
            public RoundInfo RoundData { get; set; }

            /// <summary>
            /// Chuyển đối tượng từ XMLNode
            /// </summary>
            /// <param name="xmlNode"></param>
            /// <returns></returns>
            public static FHLCData Parse(XElement xmlNode)
            {
                /// Tạo mới
                FHLCData data = new FHLCData()
                {
                    Config = EventConfig.Parse(xmlNode.Element("Config")),
                    Map = EventMapData.Parse(xmlNode.Element("EventMap")),
                    EventPointConfig = EventPointInfo.Parse(xmlNode.Element("EventPoints")),
                    Awards = new List<AwardInfo>(),
                    NPCs = new List<NPCInfo>(),
                    GroupPositions = new List<GroupPositionData>(),
                    Teleports = new List<TeleportInfo>(),
                    Attacker = AttackerData.Parse(xmlNode.Element("Attackers")),
                    Defender = DefenderData.Parse(xmlNode.Element("Defenders")),
                    RoundData = RoundInfo.Parse(xmlNode.Element("Rounds")),
                };

                /// Duyệt danh sách phần thưởng
                foreach (XElement node in xmlNode.Element("Awards").Elements("Award"))
                {
                    /// Thêm vào danh sách
                    data.Awards.Add(AwardInfo.Parse(node));
                }

                /// Duyệt danh sách NPC
                foreach (XElement node in xmlNode.Element("NPCs").Elements("NPC"))
                {
                    /// Thêm vào danh sách
                    data.NPCs.Add(NPCInfo.Parse(node));
                }

                /// Duyệt danh sách nhóm vị trí
                foreach (XElement node in xmlNode.Element("GroupPositions").Elements("Group"))
                {
                    /// Thêm vào danh sách
                    data.GroupPositions.Add(GroupPositionData.Parse(node));
                }

                /// Duyệt danh sách cổng dịch chuyển
                foreach (XElement node in xmlNode.Element("Teleports").Elements("Teleport"))
                {
                    /// Thêm vào danh sách
                    data.Teleports.Add(TeleportInfo.Parse(node));
                }

                /// Trả về kết quả
                return data;
            }
        }
        #endregion

        #region Core
        /// <summary>
        /// Dữ liệu sự kiện
        /// </summary>
        public static FHLCData Data { get; private set; }

        /// <summary>
        /// Khởi tạo sự kiện Phong Hỏa Liên Thành
        /// </summary>
        public static void Init()
        {
            /// File cấu hình
            XElement xmlNode = KTGlobal.ReadXMLData("Config/KT_GameEvents/FengHuoLianCheng.xml");
            /// Đọc dữ liệu thiết lập
			FengHuoLianCheng.Data = FHLCData.Parse(xmlNode);
        }
        #endregion

        #region Properties
        /// <summary>
        /// Có phải đang báo danh không
        /// </summary>
        public static bool IsRegisterTime { get; set; }
        #endregion

        #region Public methods
        /// <summary>
        /// Kiểm tra người chơi có đang ở trong bản đồ sự kiện Phong Hỏa Liên Thành không
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public static bool IsInsideFHLCMap(KPlayer player)
        {
            return player.CurrentMapCode == FengHuoLianCheng.Data.Map.EnterMapID;
        }
        #endregion
    }
}
