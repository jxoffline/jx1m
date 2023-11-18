using GameServer.KiemThe.Entities;
using GameServer.Logic;
using Server.Tools;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using static GameServer.Logic.KTMapManager;

namespace GameServer.KiemThe.Logic
{
    /// <summary>
    /// Quản lý quái
    /// </summary>
    public static partial class KTMonsterManager
    {
        /// <summary>
        /// Quản lý Boss thế giới
        /// </summary>
        private static class WorldBossManager
        {
            /// <summary>
            /// Thời gian tồn tại Boss
            /// </summary>
            public const int LifeTimeTicks = 3600000;

            /// <summary>
            /// Thông tin Boss thế giới
            /// </summary>
            public class WorldBossData
            {
                /// <summary>
                /// Loại xuất hiện
                /// </summary>
                public enum BossSpawnType
                {
                    /// <summary>
                    /// Ngay lập tức khi chạy Server
                    /// </summary>
                    AfterStarting = 0,
                    /// <summary>
                    /// Hàng ngày
                    /// </summary>
                    Everyday = 1,
                    /// <summary>
                    /// Hàng tuần
                    /// </summary>
                    EveryWeek = 2,
                    /// <summary>
                    /// Hàng tháng
                    /// </summary>
                    EveryMonth = 3,
                    /// <summary>
                    /// Hàng năm
                    /// </summary>
                    EveryYear = 4,
                    /// <summary>
                    /// Một lần duy nhất vào thời điểm cụ thể
                    /// </summary>
                    Once = 5,
                }

                /// <summary>
                /// Thông tin thời gian xuất hiện
                /// </summary>
                public class SpawnTimeInfo
                {
                    /// <summary>
                    /// Năm
                    /// </summary>
                    public int Year { get; set; }

                    /// <summary>
                    /// Tháng
                    /// </summary>
                    public int Month { get; set; }

                    /// <summary>
                    /// Ngày
                    /// </summary>
                    public int Day { get; set; }

                    /// <summary>
                    /// Ngày trong tuần
                    /// </summary>
                    public DayOfWeek DayOfWeek { get; set; }

                    /// <summary>
                    /// Ngày trong tháng
                    /// </summary>
                    public int DayOfMonth { get; set; }

                    /// <summary>
                    /// Giờ
                    /// </summary>
                    public int Hour { get; set; }

                    /// <summary>
                    /// Phút
                    /// </summary>
                    public int Minute { get; set; }

                    /// <summary>
                    /// So sánh bằng
                    /// </summary>
                    /// <param name="t1"></param>
                    /// <param name="t2"></param>
                    /// <returns></returns>
                    public static bool operator ==(SpawnTimeInfo t1, SpawnTimeInfo t2)
                    {
                        /// NULL
                        if (t1 is null)
                        {
                            return t2 is null;
                        }
                        else if (t2 is null)
                        {
                            return false;
                        }

                        return t1.Year == t2.Year && t1.Month == t2.Month && t1.Day == t2.Day && t1.DayOfMonth == t2.DayOfMonth && t1.DayOfWeek == t2.DayOfWeek && t1.Hour == t2.Hour && t1.Minute == t2.Minute;
                    }

                    /// <summary>
                    /// So sánh khác
                    /// </summary>
                    /// <param name="t1"></param>
                    /// <param name="t2"></param>
                    /// <returns></returns>
                    public static bool operator !=(SpawnTimeInfo t1, SpawnTimeInfo t2)
                    {
                        return !(t1 == t2);
                    }

                    /// <summary>
                    /// So sánh bằng
                    /// </summary>
                    /// <param name="obj"></param>
                    /// <returns></returns>
                    public override bool Equals(object obj)
                    {
                        if (!(obj is SpawnTimeInfo))
                        {
                            return false;
                        }
                        return this == obj as SpawnTimeInfo;
                    }

                    /// <summary>
                    /// Hash code
                    /// </summary>
                    /// <returns></returns>
                    public override int GetHashCode()
                    {
                        return base.GetHashCode();
                    }
                }

                /// <summary>
                /// Thông tin thông báo
                /// </summary>
                public class NotificationInfo
                {
                    /// <summary>
                    /// Thông báo khi xuất hiện
                    /// </summary>
                    public bool NotifyOnSpawn { get; set; }

                    /// <summary>
                    /// Thông báo vị trí cụ thể xuất hiện
                    /// </summary>
                    public bool NotifyLocation { get; set; }

                    /// <summary>
                    /// Thông báo khi chết
                    /// </summary>
                    public bool NotifyOnDead { get; set; }

                    /// <summary>
                    /// Chuyển đối tượng từ XMLNode
                    /// </summary>
                    /// <param name="xmlNode"></param>
                    /// <returns></returns>
                    public static NotificationInfo Parse(XElement xmlNode)
                    {
                        return new NotificationInfo()
                        {
                            NotifyOnSpawn = bool.Parse(xmlNode.Attribute("NotifyOnSpawn").Value),
                            NotifyLocation = bool.Parse(xmlNode.Attribute("NotifyLocation").Value),
                            NotifyOnDead = bool.Parse(xmlNode.Attribute("NotifyOnDead").Value),
                        };
                    }
                }

                /// <summary>
                /// Thông tin phần thưởng khi tiêu diệt
                /// </summary>
                public class AwardInfo
                {
                    /// <summary>
                    /// Số bạc bang hội nhận được
                    /// </summary>
                    public int GuildMoney { get; set; }

                    /// <summary>
                    /// Số bạc toàn đội nhận được
                    /// </summary>
                    public int TeamMoney { get; set; }

                    /// <summary>
                    /// Chuyển đối tượng từ XMLNode
                    /// </summary>
                    /// <param name="xmlNode"></param>
                    /// <returns></returns>
                    public static AwardInfo Parse(XElement xmlNode)
                    {
                        return new AwardInfo()
                        {
                            GuildMoney = int.Parse(xmlNode.Attribute("GuildMoney").Value),
                            TeamMoney = int.Parse(xmlNode.Attribute("PartyMoney").Value),
                        };
                    }
                }

                /// <summary>
                /// Thông tin Boss
                /// </summary>
                public class BossInfo
                {
                    /// <summary>
                    /// Thông tin vị trí xuất hiện
                    /// </summary>
                    public class LocationInfo
                    {
                        /// <summary>
                        /// ID bản đồ
                        /// </summary>
                        public int MapID { get; set; }

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
                        public static LocationInfo Parse(XElement xmlNode)
                        {
                            return new LocationInfo()
                            {
                                MapID = int.Parse(xmlNode.Attribute("MapID").Value),
                                PosX = int.Parse(xmlNode.Attribute("PosX").Value),
                                PosY = int.Parse(xmlNode.Attribute("PosY").Value),
                            };
                        }
                    }

                    /// <summary>
                    /// ID
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
                    /// Cấp độ
                    /// </summary>
                    public int Level { get; set; }

                    /// <summary>
                    /// Sinh lực tối đa
                    /// </summary>
                    public int MaxHP { get; set; }

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
                    /// Danh sách vị trí xuất hiện
                    /// </summary>
                    public List<LocationInfo> Locations { get; set; }

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
                            Level = int.Parse(xmlNode.Attribute("Level").Value),
                            MaxHP = int.Parse(xmlNode.Attribute("MaxHP").Value),
                            AIScriptID = int.Parse(xmlNode.Attribute("AIScriptID").Value),
                            Skills = new List<SkillLevelRef>(),
                            Auras = new List<SkillLevelRef>(),
                            Locations = new List<LocationInfo>(),
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

                        /// Duyệt danh sách khu vực xuất hiện
                        foreach (XElement node in xmlNode.Elements("Location"))
                        {
                            /// Thêm vào danh sách
                            monsterInfo.Locations.Add(LocationInfo.Parse(node));
                        }

                        /// Trả về kết quả
                        return monsterInfo;
                    }
                }

                /// <summary>
                /// Thời điểm xuất hiện
                /// </summary>
                public BossSpawnType SpawnType { get; set; }

                /// <summary>
                /// Thời gian xuất hiện
                /// </summary>
                public List<SpawnTimeInfo> SpawnTimes { get; set; }

                /// <summary>
                /// Tổng số Boss sẽ xuất hiện
                /// </summary>
                public int SpawnCount { get; set; }

                /// <summary>
                /// Thông báo
                /// </summary>
                public NotificationInfo Notification { get; set; }

                /// <summary>
                /// Phần thưởng
                /// </summary>
                public AwardInfo Award { get; set; }

                /// <summary>
                /// Danh sách Boss
                /// </summary>
                public List<BossInfo> Bosses { get; set; }

                /// <summary>
                /// Chuyển đối tượng từ XMLNode
                /// </summary>
                /// <param name="xmlNode"></param>
                /// <returns></returns>
                public static WorldBossData Parse(XElement xmlNode)
                {
                    /// Tạo mới
                    WorldBossData data = new WorldBossData()
                    {
                        SpawnType = (BossSpawnType) int.Parse(xmlNode.Attribute("SpawnType").Value),
                        SpawnCount = int.Parse(xmlNode.Attribute("SpawnCount").Value),
                        SpawnTimes = new List<SpawnTimeInfo>(),
                        Notification = NotificationInfo.Parse(xmlNode.Element("Notification")),
                        Award = AwardInfo.Parse(xmlNode.Element("Award")),
                        Bosses = new List<BossInfo>(),
                    };

                    /// Chuỗi mã hóa thời gian xuất hiện
                    string spawnTimesStrings = xmlNode.Attribute("SpawnTimes").Value;
                    /// Nếu có dữ liệu
                    if (!string.IsNullOrEmpty(spawnTimesStrings))
                    {
                        /// Loại là gì
                        switch (data.SpawnType)
                        {
                            /// Xuất hiện ngay khi chạy Server
                            case BossSpawnType.AfterStarting:
                            {
                                break;
                            }
                            /// Hàng ngày
                            case BossSpawnType.Everyday:
                            {
                                /// Duyệt danh sách các mốc
                                foreach (string spawnTimesString in spawnTimesStrings.Split(';'))
                                {
                                    /// Các trường
                                    string[] fields = spawnTimesString.Split(':');
                                    /// Giờ
                                    int hour = int.Parse(fields[0]);
                                    /// Phút
                                    int minute = int.Parse(fields[1]);
                                    /// Thêm vào danh sách
                                    data.SpawnTimes.Add(new SpawnTimeInfo()
                                    {
                                        Hour = hour,
                                        Minute = minute,
                                    });
                                }
                                break;
                            }
                            /// Hàng tuần
                            case BossSpawnType.EveryWeek:
                            {
                                /// Duyệt danh sách các mốc
                                foreach (string spawnTimesString in spawnTimesStrings.Split(';'))
                                {
                                    /// Các trường
                                    string[] fields = spawnTimesString.Split(':');
                                    /// Thứ
                                    int dayOfWeek = int.Parse(fields[0]);
                                    /// Giờ
                                    int hour = int.Parse(fields[1]);
                                    /// Phút
                                    int minute = int.Parse(fields[2]);
                                    /// Thêm vào danh sách
                                    data.SpawnTimes.Add(new SpawnTimeInfo()
                                    {
                                        DayOfWeek = (DayOfWeek) dayOfWeek,
                                        Hour = hour,
                                        Minute = minute,
                                    });
                                }
                                break;
                            }
                            /// Hàng tháng
                            case BossSpawnType.EveryMonth:
                            {
                                /// Duyệt danh sách các mốc
                                foreach (string spawnTimesString in spawnTimesStrings.Split(';'))
                                {
                                    /// Các trường
                                    string[] fields = spawnTimesString.Split(':');
                                    /// Ngày trong tháng
                                    int dayOfMonth = int.Parse(fields[0]);
                                    /// Giờ
                                    int hour = int.Parse(fields[1]);
                                    /// Phút
                                    int minute = int.Parse(fields[2]);
                                    /// Thêm vào danh sách
                                    data.SpawnTimes.Add(new SpawnTimeInfo()
                                    {
                                        DayOfMonth = dayOfMonth,
                                        Hour = hour,
                                        Minute = minute,
                                    });
                                }
                                break;
                            }
                            /// Hàng năm
                            case BossSpawnType.EveryYear:
                            {
                                /// Duyệt danh sách các mốc
                                foreach (string spawnTimesString in spawnTimesStrings.Split(';'))
                                {
                                    /// Các trường
                                    string[] fields = spawnTimesString.Split(':');
                                    /// Ngày
                                    int day = int.Parse(fields[0]);
                                    /// Tháng
                                    int month = int.Parse(fields[1]);
                                    /// Giờ
                                    int hour = int.Parse(fields[2]);
                                    /// Phút
                                    int minute = int.Parse(fields[3]);
                                    /// Thêm vào danh sách
                                    data.SpawnTimes.Add(new SpawnTimeInfo()
                                    {
                                        Day = day,
                                        Month = month,
                                        Hour = hour,
                                        Minute = minute,
                                    });
                                }
                                break;
                            }
                            /// Một lần duy nhất
                            case BossSpawnType.Once:
                            {
                                /// Duyệt danh sách các mốc
                                foreach (string spawnTimesString in spawnTimesStrings.Split(';'))
                                {
                                    /// Các trường
                                    string[] fields = spawnTimesString.Split(':');
                                    /// Ngày
                                    int day = int.Parse(fields[0]);
                                    /// Tháng
                                    int month = int.Parse(fields[1]);
                                    /// Năm
                                    int year = int.Parse(fields[2]);
                                    /// Giờ
                                    int hour = int.Parse(fields[3]);
                                    /// Phút
                                    int minute = int.Parse(fields[4]);
                                    /// Thêm vào danh sách
                                    data.SpawnTimes.Add(new SpawnTimeInfo()
                                    {
                                        Day = day,
                                        Month = month,
                                        Year = year,
                                        Hour = hour,
                                        Minute = minute,
                                    });
                                }
                                break;
                            }
                        }
                    }

                    /// Duyệt danh sách Boss sẽ xuất hiện
                    foreach (XElement node in xmlNode.Element("Bosses").Elements("Boss"))
                    {
                        /// Thêm vào danh sách
                        data.Bosses.Add(BossInfo.Parse(node));
                    }

                    /// Trả về kết quả
                    return data;
                }
            }

            /// <summary>
            /// Dữ liệu Boss thế giới dùng nội bộ
            /// </summary>
            private class WorldBossDataLocal
            {
                /// <summary>
                /// Dữ liệu
                /// </summary>
                public WorldBossData Data { get; set; }

                /// <summary>
                /// Thời điểm sinh ra lần trước
                /// </summary>
                public WorldBossData.SpawnTimeInfo LastSpawnTime { get; set; }
            }

            /// <summary>
            /// Thông tin Boss đang tồn tại
            /// </summary>
            public class AliveBossInfo
            {
                /// <summary>
                /// Đối tượng Boss
                /// </summary>
                public Monster Boss { get; set; }

                /// <summary>
                /// Phần thưởng tương ứng
                /// </summary>
                public WorldBossData.AwardInfo Award { get; set; }

                /// <summary>
                /// Thời điểm tạo ra
                /// </summary>
                public long CreateTicks { get; set; }

                /// <summary>
                /// Thông báo khi chết không
                /// </summary>
                public bool NotificationOnDead { get; set; }

                /// <summary>
                /// Đã hết thời gian
                /// </summary>
                public bool Timeout
                {
                    get
                    {
                        return KTGlobal.GetCurrentTimeMilis() - this.CreateTicks >= WorldBossManager.LifeTimeTicks;
                    }
                }
            }

            /// <summary>
            /// Danh sách Boss thế giới
            /// </summary>
            private static readonly List<WorldBossDataLocal> worldBosses = new List<WorldBossDataLocal>();

            /// <summary>
            /// Danh sách Boss đang tồn tại
            /// </summary>
            private static readonly ConcurrentDictionary<int, AliveBossInfo> aliveBosses = new ConcurrentDictionary<int, AliveBossInfo>();

            /// <summary>
            /// Đã tạo Boss khi mới chạy Server xong chưa
            /// </summary>
            private static bool startServerBossesSpawned = false;

            #region API
            /// <summary>
            /// Thêm Boss thế giới vào danh sách
            /// </summary>
            /// <param name="data"></param>
            public static void Add(WorldBossData data)
            {
                WorldBossManager.worldBosses.Add(new WorldBossDataLocal()
                {
                    Data = data,
                    LastSpawnTime = null,
                });
            }
            #endregion

            #region Event
            /// <summary>
            /// Khởi tạo Boss thế giới
            /// </summary>
            public static void Initlaize()
            {
                
            }

            /// <summary>
            /// Sự kiện Tick
            /// </summary>
            public static void Tick()
            {
                /// Nếu chưa chạy xong Server
                if (GameManager.ServerStarting)
                {
                    /// Bỏ qua
                    return;
                }

                /// Duyệt danh sách nhóm
                foreach (WorldBossDataLocal data in WorldBossManager.worldBosses)
                {
                    /// Nếu đã đến thời gian
                    if (WorldBossManager.IsInTime(data, out WorldBossData.SpawnTimeInfo timeInfo))
                    {
                        /// Đánh dấu thời điểm tạo lần cuối
                        data.LastSpawnTime = timeInfo;
                        /// Tạo Boss
                        WorldBossManager.Begin(data);
                    }
                }

                /// Danh sách Boss đang còn sống theo ID
                List<int> keys = WorldBossManager.aliveBosses.Keys.ToList();
                /// Duyệt danh sách
                foreach (int key in keys)
                {
                    /// Nếu không tồn tại
                    if (!WorldBossManager.aliveBosses.TryGetValue(key, out AliveBossInfo bossInfo))
                    {
                        /// Bỏ qua
                        continue;
                    }

                    /// Nếu đã hết thời gian
                    if (bossInfo.Timeout)
                    {
                        /// Xóa Boss này
                        KTMonsterManager.Remove(bossInfo.Boss);
                        /// Xóa khỏi danh sách quản lý
                        WorldBossManager.aliveBosses.TryRemove(key, out _);
                    }
                }
            }

            /// <summary>
            /// Sự kiện khi Boss bị người chơi giết
            /// </summary>
            /// <param name="player"></param>
            /// <param name="boss"></param>
            public static void OnBossDie(KPlayer player, Monster boss)
            {
                /// Thông tin tương ứng
                if (!WorldBossManager.aliveBosses.TryGetValue(boss.RoleID, out AliveBossInfo bossInfo))
                {
                    /// Không tìm thấy thì bỏ qua
                    return;
                }

                /// Xóa khỏi danh sách
                WorldBossManager.aliveBosses.TryRemove(boss.RoleID, out _);

                /// Số bạc bang hội sẽ nhận được
                int guildMoneyGet = bossInfo.Award.GuildMoney;
                /// Số bạc toàn đội sẽ nhận được
                int teamMoneyGet = bossInfo.Award.TeamMoney;
                
                /// Nếu người chơi có bang
                if (player.GuildID > 0)
                {
                    /// Nếu có số bạc cả đội được
                    if (teamMoneyGet > 0)
                    {
                        /// Nếu thằng này không có nhóm
                        if (player.TeamID == -1 || !KTTeamManager.IsTeamExist(player.TeamID))
                        {
                            /// Thêm cho nó
                            KTGlobal.AddMoney(player, teamMoneyGet, MoneyType.GuildMoney, "BOSSVLCT");

                            
                        }
                        /// Nếu thằng này có nhóm
                        else
                        {
                            /// Duyệt danh sách thành viên nhóm
                            foreach (KPlayer teammate in player.Teammates)
                            {
                                /// Thêm cho thằng này
                                KTGlobal.AddMoney(teammate, teamMoneyGet, MoneyType.GuildMoney, "BOSSVLCT");
                            }
                        }
                    }

                    /// Nếu có số tài sản bang
                    if (guildMoneyGet > 0)
                    {
                        /// Thêm bạc vào ngân quỹ bang
                        KTGlobal.UpdateGuildMoney(guildMoneyGet, player.GuildID, player);

                        /// Bản đồ
                        GameMap gameMap = KTMapManager.Find(boss.MapCode);
                        /// Tên bản đồ
                        string mapName = gameMap.MapName;
                        /// Thông báo
                        string msgText = string.Format("<color=yellow>[{0}]</color> tại <color=green>{1}</color> đã bị <color=#47c1e6>[{2}]</color> và đồng đội tiêu diệt, ngân quỹ bang hội tăng <color=orange>{3}</color>.", boss.RoleName, mapName, player.RoleName, KTGlobal.GetDisplayMoney(guildMoneyGet));
                        /// Thông báo kênh bang
                        KTGlobal.SendGuildChat(player.GuildID, msgText, null, "");
                    }
                }

                /// Nếu có thông báo
                if (bossInfo.NotificationOnDead)
                {
                    /// Bản đồ
                    GameMap gameMap = KTMapManager.Find(boss.MapCode);
                    /// Tên bản đồ
                    string mapName = gameMap.MapName;
                    /// Thông báo
                    string msgText = string.Format("Cấp báo. <color=yellow>[{0}]</color> tại <color=green>{1}</color> đã bị <color=#47c1e6>[{2}]</color> và đồng đội tiêu diệt.", boss.RoleName, mapName, player.RoleName);
                    /// Thông báo kênh hệ thống
                    KTGlobal.SendSystemChat(msgText);
                }
            }
            #endregion

            #region Private methods
            /// <summary>
            /// Kiểm tra đã đến thời gian tạo Boss thế giới chưa
            /// </summary>
            /// <param name="data"></param>
            /// <returns></returns>
            private static bool IsInTime(WorldBossDataLocal data, out WorldBossData.SpawnTimeInfo timeInfo)
            {
                /// Thời gian hiện tại
                DateTime now = DateTime.Now;
                /// Múi giờ tương ứng
                timeInfo = null;

                /// Loại xuất hiện
                switch (data.Data.SpawnType)
                {
                    /// Ngay khi chạy Server
                    case WorldBossData.BossSpawnType.AfterStarting:
                    {
                        /// Nếu đã gọi rồi
                        if (WorldBossManager.startServerBossesSpawned)
                        {
                            /// Không thể gọi thêm nữa
                            return false;
                        }

                        /// Đánh dấu đã gọi rồi
                        WorldBossManager.startServerBossesSpawned = true;
                        /// Có thể gọi
                        return true;
                    }
                    /// Mỗi ngày
                    case WorldBossData.BossSpawnType.Everyday:
                    {
                        /// Kiểm tra có mốc nào phù hợp không
                        WorldBossData.SpawnTimeInfo time = data.Data.SpawnTimes.Where(x => x.Hour == now.Hour && x.Minute == now.Minute).FirstOrDefault();
                        /// Nếu tìm thấy
                        if (time != null)
                        {
                            /// Nếu đã gọi ở mốc này rồi thì thôi
                            if (data.LastSpawnTime == time)
                            {
                                return false;
                            }

                            /// Đánh dấu thời gian cần gọi
                            timeInfo = time;
                            /// Có thể gọi
                            return true;
                        }
                        /// Không tìm thấy
                        else
                        {
                            /// Chưa đến giờ
                            return false;
                        }
                    }
                    /// Mỗi tuần
                    case WorldBossData.BossSpawnType.EveryWeek:
                    {
                        /// Kiểm tra có mốc nào phù hợp không
                        WorldBossData.SpawnTimeInfo time = data.Data.SpawnTimes.Where(x => x.Hour == now.Hour && x.Minute == now.Minute && x.DayOfWeek == now.DayOfWeek).FirstOrDefault();
                        /// Nếu tìm thấy
                        if (time != null)
                        {
                            /// Nếu đã gọi ở mốc này rồi thì thôi
                            if (data.LastSpawnTime == time)
                            {
                                return false;
                            }

                            /// Đánh dấu thời gian cần gọi
                            timeInfo = time;
                            /// Có thể gọi
                            return true;
                        }
                        /// Không tìm thấy
                        else
                        {
                            /// Chưa đến giờ
                            return false;
                        }
                    }
                    /// Mỗi tháng
                    case WorldBossData.BossSpawnType.EveryMonth:
                    {
                        /// Kiểm tra có mốc nào phù hợp không
                        WorldBossData.SpawnTimeInfo time = data.Data.SpawnTimes.Where(x => x.Hour == now.Hour && x.Minute == now.Minute && x.DayOfMonth == now.Day).FirstOrDefault();
                        /// Nếu tìm thấy
                        if (time != null)
                        {
                            /// Nếu đã gọi ở mốc này rồi thì thôi
                            if (data.LastSpawnTime == time)
                            {
                                return false;
                            }

                            /// Đánh dấu thời gian cần gọi
                            timeInfo = time;
                            /// Có thể gọi
                            return true;
                        }
                        /// Không tìm thấy
                        else
                        {
                            /// Chưa đến giờ
                            return false;
                        }
                    }
                    /// Mỗi năm
                    case WorldBossData.BossSpawnType.EveryYear:
                    {
                        /// Kiểm tra có mốc nào phù hợp không
                        WorldBossData.SpawnTimeInfo time = data.Data.SpawnTimes.Where(x => x.Hour == now.Hour && x.Minute == now.Minute && x.DayOfMonth == now.Day && x.Month == now.Month).FirstOrDefault();
                        /// Nếu tìm thấy
                        if (time != null)
                        {
                            /// Nếu đã gọi ở mốc này rồi thì thôi
                            if (data.LastSpawnTime == time)
                            {
                                return false;
                            }

                            /// Đánh dấu thời gian cần gọi
                            timeInfo = time;
                            /// Có thể gọi
                            return true;
                        }
                        /// Không tìm thấy
                        else
                        {
                            /// Chưa đến giờ
                            return false;
                        }
                    }
                    /// Một lần duy nhất
                    case WorldBossData.BossSpawnType.Once:
                    {
                        /// Kiểm tra có mốc nào phù hợp không
                        WorldBossData.SpawnTimeInfo time = data.Data.SpawnTimes.Where(x => x.Hour == now.Hour && x.Minute == now.Minute && x.DayOfMonth == now.Day && x.Month == now.Month && x.Year == now.Year).FirstOrDefault();
                        /// Nếu tìm thấy
                        if (time != null)
                        {
                            /// Nếu đã gọi ở mốc này rồi thì thôi
                            if (data.LastSpawnTime == time)
                            {
                                return false;
                            }

                            /// Đánh dấu thời gian cần gọi
                            timeInfo = time;
                            /// Có thể gọi
                            return true;
                        }
                        /// Không tìm thấy
                        else
                        {
                            /// Chưa đến giờ
                            return false;
                        }
                    }
                }

                /// Chưa đến giờ
                return false;
            }

            /// <summary>
            /// Bắt đầu Boss thế giới tương ứng
            /// </summary>
            /// <param name="data"></param>
            private static void Begin(WorldBossDataLocal data)
            {
                /// Nếu tổng số Boss sẽ xuất hiện lớn hơn tổng số Boss hiện có
                if (data.Data.SpawnCount > data.Data.Bosses.Count)
                {
                    /// Tổng số lượt trùng lặp sẽ tạo
                    int totalDuplicates = data.Data.SpawnCount / data.Data.Bosses.Count;
                    /// Duyệt số lần trùng lặp
                    for (int i = 1; i <= totalDuplicates; i++)
                    {
                        /// Duyệt danh sách
                        foreach (WorldBossData.BossInfo bossInfo in data.Data.Bosses)
                        {
                            /// Tạo Boss tương ứng
                            WorldBossManager.Create(data, bossInfo);
                        }
                    }
                }

                /// Tổng số ngẫu nhiên sẽ chọn
                int randomCount = data.Data.SpawnCount % data.Data.Bosses.Count;
                /// Nếu tổng số Boss sẽ xuất hiện nhỏ hơn tổng số Boss hiện có
                if (data.Data.SpawnCount <= data.Data.Bosses.Count)
                {
                    /// Nếu chia hết
                    if (randomCount == 0)
                    {
                        /// Lấy giá trị cao nhất
                        randomCount = data.Data.SpawnCount;
                    }
                }
                /// Nếu không có
                if (randomCount <= 0)
                {
                    /// Bỏ qua
                    return;
                }

                /// Danh sách ngẫu nhiên sẽ chọn ra
                List<WorldBossData.BossInfo> randBosses = data.Data.Bosses.RandomRange(randomCount).ToList();
                /// Duyệt danh sách
                foreach (WorldBossData.BossInfo bossInfo in randBosses)
                {
                    /// Tạo Boss tương ứng
                    WorldBossManager.Create(data, bossInfo);
                }
            }

            #region Tạo Boss
            /// <summary>
            /// Tạo Boss thế giới tương ứng
            /// </summary>
            /// <param name="data"></param>
            /// <param name="bossData"></param>
            private static void Create(WorldBossDataLocal data, WorldBossData.BossInfo bossData)
            {
                /// Chọn vị trí ngẫu nhiên
                WorldBossData.BossInfo.LocationInfo location = bossData.Locations.RandomRange(1).FirstOrDefault();

                /// Tạo Boss
                Monster boss = KTMonsterManager.Create(new KTMonsterManager.DynamicMonsterBuilder()
                {
                    MapCode = location.MapID,
                    ResID = bossData.ID,
                    PosX = location.PosX,
                    PosY = location.PosY,
                    Name = bossData.Name,
                    Title = bossData.Title,
                    Level = bossData.Level,
                    MaxHP = bossData.MaxHP,
                    MonsterType = MonsterAIType.Boss,
                    AIID = bossData.AIScriptID,
                    Tag = "WorldBoss",
                    Camp = 65535,
                    AllowRecordDamage = true,
                });

                /// Nếu có kỹ năng
                if (bossData.Skills.Count > 0)
                {
                    /// Duyệt danh sách kỹ năng
                    foreach (SkillLevelRef skill in bossData.Skills)
                    {
                        /// Thêm vào danh sách kỹ năng dùng của quái
                        boss.CustomAISkills.Add(skill);
                    }
                }

                /// Nếu có vòng sáng
                if (bossData.Auras.Count > 0)
                {
                    /// Duyệt danh sách vòng sáng
                    foreach (SkillLevelRef aura in bossData.Auras)
                    {
                        /// Kích hoạt vòng sáng
                        boss.UseSkill(aura.SkillID, aura.Level, boss);
                    }
                }

                /// Lưu vào danh sách Boss đang tồn tại
                WorldBossManager.aliveBosses[boss.RoleID] = new AliveBossInfo()
                {
                    Boss = boss,
                    Award = data.Data.Award,
                    CreateTicks = KTGlobal.GetCurrentTimeMilis(),
                    NotificationOnDead = data.Data.Notification.NotifyOnDead,
                };

                /// Nếu có thông báo
                if (data.Data.Notification.NotifyOnSpawn)
                {
                    /// Bản đồ
                    GameMap gameMap = KTMapManager.Find(boss.MapCode);
                    /// Tên bản đồ
                    string mapName = gameMap.MapName;
                    /// Thông báo
                    string msgText;
                    /// Nếu thông báo đầy đủ vị trí đứng
                    if (data.Data.Notification.NotifyLocation)
                    {
                        msgText = string.Format("Có người nhìn thấy <color=yellow>[{0}]</color> đã xuất hiện tại <color=green>{1}</color> <color=#47c1e6>({2},{3})</color>. Các vị anh hùng hãy mau mau tiêu diệt.", boss.RoleName, mapName, (int) boss.CurrentGrid.X, (int) boss.CurrentGrid.Y);
                        //msgText = string.Format("Có người nhìn thấy <color=yellow>[{0}]</color> đã xuất hiện tại <color=green>{1}</color> <color=#47c1e6>({2},{3})</color>. Các vị anh hùng hãy mau mau tiêu diệt.", boss.RoleName, mapName, (int) boss.CurrentPos.X, (int) boss.CurrentPos.Y);
                    }
                    /// Nếu chỉ thông báo bản đồ
                    else
                    {
                        msgText = string.Format("Có người nhìn thấy <color=yellow>[{0}]</color> đã xuất hiện tại <color=green>{1}</color>. Các vị anh hùng hãy mau mau tiêu diệt.", boss.RoleName, mapName);
                    }
                    /// Thông báo kênh hệ thống
                    KTGlobal.SendSystemEventNotification(msgText);
                }
            }
            #endregion
            #endregion
        }

        #region Event
        /// <summary>
        /// Tick quản lý Boss thế giới
        /// </summary>
        private static void WorldBoss_Tick()
        {
            WorldBossManager.Tick();
        }
        #endregion
    }
}
