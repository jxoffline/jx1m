using GameServer.KiemThe.Logic;
using GameServer.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace GameServer.KiemThe.GameEvents.BaiHuTang
{
    /// <summary>
    /// Định nghĩa Bạch Hổ Đường
    /// </summary>
    public static class BaiHuTang
    {
        #region Define
        /// <summary>
        /// Thông tin ải
        /// </summary>
        public class RoundInfo
        {
            /// <summary>
            /// Thông tin danh vọng
            /// </summary>
            public class ReputeInfo
			{
                /// <summary>
                /// Phần thưởng uy danh cho Bang hội giết Boss
                /// </summary>
                public int GuildKillBossPrestige { get; set; }

                /// <summary>
                /// Phần thưởng uy danh cho toàn bộ người chơi khác
                /// </summary>
                public int OtherPrestige { get; set; }

                /// <summary>
                /// Danh vọng cho Bang hội giết Boss
                /// </summary>
                public int GuildKillBossRepute { get; set; }

                /// <summary>
                /// Danh vọng cho toàn bộ người chơi khác
                /// </summary>
                public int OtherRepute { get; set; }

                /// <summary>
                /// Chuyển đối tượng từ XMLNode
                /// </summary>
                /// <param name="xmlNode"></param>
                /// <returns></returns>
                public static ReputeInfo Parse(XElement xmlNode)
				{
                    return new ReputeInfo()
                    {
                        GuildKillBossPrestige = int.Parse(xmlNode.Attribute("GuildKillBossPrestige").Value),
                        OtherPrestige = int.Parse(xmlNode.Attribute("OtherPrestige").Value),
                        GuildKillBossRepute = int.Parse(xmlNode.Attribute("GuildKillBossRepute").Value),
                        OtherRepute = int.Parse(xmlNode.Attribute("OtherRepute").Value),
                    };
				}
            }

            /// <summary>
            /// Định nghĩa hoạt động
            /// </summary>
            public class ActivityInfo
            {
                /// <summary>
                /// ID hoạt động
                /// </summary>
                public int ActivityID { get; set; }

                /// <summary>
                /// Cấp hoạt động
                /// </summary>
                public int ActivityLevel { get; set; }

                /// <summary>
                /// Danh sách bản đồ
                /// </summary>
                public List<int> Maps { get; set; }

                /// <summary>
                /// Chuyển đối tượng từ XMLNode
                /// </summary>
                /// <param name="xmlNode"></param>
                /// <returns></returns>
                public static ActivityInfo Parse(XElement xmlNode)
                {
                    ActivityInfo activity = new ActivityInfo()
                    {
                        ActivityID = int.Parse(xmlNode.Attribute("ActivityID").Value),
                        ActivityLevel = int.Parse(xmlNode.Attribute("ActivityLevel").Value),
                        Maps = new List<int>(),
                    };

                    string mapStrings = xmlNode.Attribute("MapIDs").Value;
                    if (!string.IsNullOrEmpty(mapStrings))
					{
                        foreach (string mapString in mapStrings.Split(';'))
                        {
                            activity.Maps.Add(int.Parse(mapString));
                        }
                    }

                    return activity;
                }
            }

            /// <summary>
            /// Thông tin quái
            /// </summary>
            public class MonsterInfo
            {
                /// <summary>
                /// ID quái tương ứng hoạt động
                /// </summary>
                public Dictionary<int, int> IDByActivity { get; set; }

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
                /// Máu cơ bản
                /// </summary>
                public int BaseHP { get; set; }

                /// <summary>
                /// Lượng máu tăng thêm tương ứng mỗi cấp quái
                /// </summary>
                public int HPIncreaseEachLevel { get; set; }

                /// <summary>
                /// Loại quái
                /// </summary>
                public int AIType { get; set; }

                /// <summary>
                /// Thời gian tái sinh
                /// </summary>
                public int RespawnTick { get; set; }

                /// <summary>
                /// ID Script AI điều khiển
                /// </summary>
                public int AIScriptID { get; set; }

                /// <summary>
                /// Chuyển đối tượng từ XMLNode
                /// </summary>
                /// <param name="xmlNode"></param>
                /// <returns></returns>
                public static MonsterInfo Parse(XElement xmlNode)
                {
                    MonsterInfo monsterInfo = new MonsterInfo()
                    {
                        IDByActivity = new Dictionary<int, int>(),
                        Name = xmlNode.Attribute("Name").Value,
                        Title = xmlNode.Attribute("Title").Value,
                        PosX = int.Parse(xmlNode.Attribute("PosX").Value),
                        PosY = int.Parse(xmlNode.Attribute("PosY").Value),
                        BaseHP = int.Parse(xmlNode.Attribute("BaseHP").Value),
                        HPIncreaseEachLevel = int.Parse(xmlNode.Attribute("HPIncreaseEachLevel").Value),
                        AIType = int.Parse(xmlNode.Attribute("AIType").Value),
                        RespawnTick = int.Parse(xmlNode.Attribute("RespawnTick").Value),
                        AIScriptID = int.Parse(xmlNode.Attribute("AIScriptID").Value),
                    };

                    string idsByActivityStrings = xmlNode.Attribute("IDs").Value;
                    foreach (string idsByActivityString in idsByActivityStrings.Split(';'))
                    {
                        string[] param = idsByActivityString.Split('_');
                        int activityID = int.Parse(param[0]);
                        int monsterID = int.Parse(param[1]);
                        monsterInfo.IDByActivity[activityID] = monsterID;
                    }

                    return monsterInfo;
                }
            }

            /// <summary>
            /// Thông tin Boss
            /// </summary>
            public class BossInfo
            {
                /// <summary>
                /// ID quái tương ứng hoạt động
                /// </summary>
                public Dictionary<int, int> IDByActivity { get; set; }

                /// <summary>
                /// Tên quái
                /// </summary>
                public string Name { get; set; }

                /// <summary>
                /// Danh hiệu quái
                /// </summary>
                public string Title { get; set; }

                /// <summary>
                /// Vị trí ngẫu nhiên
                /// </summary>
                public List<KeyValuePair<int, int>> RandomPos { get; set; }

                /// <summary>
                /// Máu cơ bản
                /// </summary>
                public int BaseHP { get; set; }

                /// <summary>
                /// Lượng máu tăng thêm tương ứng mỗi cấp quái
                /// </summary>
                public int HPIncreaseEachLevel { get; set; }

                /// <summary>
                /// Loại quái
                /// </summary>
                public int AIType { get; set; }

                /// <summary>
                /// Thời gian đếm ngược từ lúc bắt đầu hoạt động đến khi ra Boss
                /// </summary>
                public int SpawnAfter { get; set; }

                /// <summary>
                /// ID Script AI điều khiển
                /// </summary>
                public int AIScriptID { get; set; }

                /// <summary>
                /// Chuyển đối tượng từ XMLNode
                /// </summary>
                /// <param name="xmlNode"></param>
                /// <returns></returns>
                public static BossInfo Parse(XElement xmlNode)
                {
                    BossInfo monsterInfo = new BossInfo()
                    {
                        IDByActivity = new Dictionary<int, int>(),
                        Name = xmlNode.Attribute("Name").Value,
                        Title = xmlNode.Attribute("Title").Value,
                        RandomPos = new List<KeyValuePair<int, int>>(),
                        BaseHP = int.Parse(xmlNode.Attribute("BaseHP").Value),
                        HPIncreaseEachLevel = int.Parse(xmlNode.Attribute("HPIncreaseEachLevel").Value),
                        AIType = int.Parse(xmlNode.Attribute("AIType").Value),
                        SpawnAfter = int.Parse(xmlNode.Attribute("SpawnAfter").Value),
                        AIScriptID = int.Parse(xmlNode.Attribute("AIScriptID").Value),
                    };

                    string idsByActivityStrings = xmlNode.Attribute("IDs").Value;
                    foreach (string idsByActivityString in idsByActivityStrings.Split(';'))
                    {
                        string[] param = idsByActivityString.Split('_');
                        int activityID = int.Parse(param[0]);
                        int monsterID = int.Parse(param[1]);
                        monsterInfo.IDByActivity[activityID] = monsterID;
                    }

                    string randomPosStrings = xmlNode.Attribute("RandomPos").Value;
                    foreach (string randomPosString in randomPosStrings.Split(';'))
                    {
                        string[] param = randomPosString.Split('_');
                        int posX = int.Parse(param[0]);
                        int posY = int.Parse(param[1]);
                        monsterInfo.RandomPos.Add(new KeyValuePair<int, int>(posX, posY));
                    }

                    return monsterInfo;
                }
            }

            /// <summary>
            /// Thông tin cổng dịch chuyển
            /// </summary>
            public class TeleportInfo
            {
                /// <summary>
                /// ID Res
                /// </summary>
                public int ResID { get; set; }

                /// <summary>
                /// Tên cổng dịch chuyển
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
                /// Bán kính quét
                /// </summary>
                public int Radius { get; set; }

                /// <summary>
                /// Chuyển đối tượng từ XMLNode
                /// </summary>
                /// <param name="xmlNode"></param>
                /// <returns></returns>
                public static TeleportInfo Parse(XElement xmlNode)
                {
                    return new TeleportInfo()
                    {
                        ResID = int.Parse(xmlNode.Attribute("ID").Value),
                        Name = xmlNode.Attribute("Name").Value,
                        PosX = int.Parse(xmlNode.Attribute("PosX").Value),
                        PosY = int.Parse(xmlNode.Attribute("PosY").Value),
                        Radius = int.Parse(xmlNode.Attribute("Radius").Value),
                    };
                }
            }

            /// <summary>
            /// Thông tin bản đồ rời
            /// </summary>
            public class OutMapInfo
            {
                /// <summary>
                /// Từ bản đồ
                /// </summary>
                public int FromMapID { get; set; }

                /// <summary>
                /// Tới bản đồ
                /// </summary>
                public int OutMapID { get; set; }

                /// <summary>
                /// Vị trí X ở bản dồ tới
                /// </summary>
                public int OutPosX { get; set; }

                /// <summary>
                /// Vị trí Y ở bản dồ tới
                /// </summary>
                public int OutPosY { get; set; }

                /// <summary>
                /// Chuyển đối tượng từ XMLNode
                /// </summary>
                /// <param name="xmlNode"></param>
                /// <returns></returns>
                public static OutMapInfo Parse(XElement xmlNode)
                {
                    return new OutMapInfo()
                    {
                        FromMapID = int.Parse(xmlNode.Attribute("FromMapID").Value),
                        OutMapID = int.Parse(xmlNode.Attribute("OutMapID").Value),
                        OutPosX = int.Parse(xmlNode.Attribute("OutPosX").Value),
                        OutPosY = int.Parse(xmlNode.Attribute("OutPosY").Value),
                    };
                }
            }

            /// <summary>
            /// Thông tin bản đồ tiếp theo
            /// </summary>
            public class NextMapInfo
            {
                /// <summary>
                /// Từ bản đồ
                /// </summary>
                public int FromMapID { get; set; }

                /// <summary>
                /// Tới bản đồ
                /// </summary>
                public int NextMapID { get; set; }

                /// <summary>
                /// Vị trí X ở bản dồ tới
                /// </summary>
                public int NextPosX { get; set; }

                /// <summary>
                /// Vị trí Y ở bản dồ tới
                /// </summary>
                public int NextPosY { get; set; }

                /// <summary>
                /// Chuyển đối tượng từ XMLNode
                /// </summary>
                /// <param name="xmlNode"></param>
                /// <returns></returns>
                public static NextMapInfo Parse(XElement xmlNode)
                {
                    return new NextMapInfo()
                    {
                        FromMapID = int.Parse(xmlNode.Attribute("FromMapID").Value),
                        NextMapID = int.Parse(xmlNode.Attribute("NextMapID").Value),
                        NextPosX = int.Parse(xmlNode.Attribute("NextPosX").Value),
                        NextPosY = int.Parse(xmlNode.Attribute("NextPosY").Value),
                    };
                }
            }

            /// <summary>
            /// Tên ải
            /// </summary>
            public string RoundName { get; set; }

            /// <summary>
            /// Thông tin danh vọng nhận được
            /// </summary>
            public ReputeInfo Repute { get; set; }

            /// <summary>
            /// Danh sách hoạt động
            /// </summary>
            public Dictionary<int, ActivityInfo> Activities { get; set; }

            /// <summary>
            /// Danh sách quái
            /// </summary>
            public List<MonsterInfo> Monsters { get; set; }

            /// <summary>
            /// Thông tin Boss
            /// </summary>
            public BossInfo Boss { get; set; }

            /// <summary>
            /// Thông tin bản đồ rời
            /// </summary>
            public Dictionary<int, OutMapInfo> OutMaps { get; set; }

            /// <summary>
            /// Thông tin bản đồ tiếp
            /// </summary>
            public Dictionary<int, NextMapInfo> NextMaps { get; set; }

            /// <summary>
            /// Cổng dịch chuyển
            /// </summary>
            public TeleportInfo Teleport { get; set; }
        }
        #endregion

        #region Properties
        /// <summary>
        /// Chuẩn bị
        /// </summary>
        public static RoundInfo Prepare { get; private set; }

        /// <summary>
        /// Bạch Hổ Đường 1
        /// </summary>
        public static RoundInfo Round1 { get; private set; }

        /// <summary>
        /// Bạch Hổ Đường 2
        /// </summary>
        public static RoundInfo Round2 { get; private set; }

        /// <summary>
        /// Bạch Hổ Đường 3
        /// </summary>
        public static RoundInfo Round3 { get; private set; }

        /// <summary>
        /// Kết thúc Bạch Hổ Đường
        /// </summary>
        public static RoundInfo End { get; private set; }

        /// <summary>
        /// Bước hoạt động hiện tại
        /// </summary>
        public static int CurrentStage { get; set; }

        /// <summary>
        /// Bắt đầu mở báo danh chưa
        /// </summary>
        public static bool BeginRegistered { get; set; }

        /// <summary>
        /// Danh sách tọa độ ngẫu nhiên khi lên tầng
        /// </summary>
        public static List<UnityEngine.Vector2> RandomTeleportPositions { get; } = new List<UnityEngine.Vector2>()
        {
            new UnityEngine.Vector2(1516, 934),
            new UnityEngine.Vector2(1113, 1237),
            new UnityEngine.Vector2(1524, 1700),
            new UnityEngine.Vector2(2224, 2028),
            new UnityEngine.Vector2(2885, 2158),
            new UnityEngine.Vector2(3504, 1819),
            new UnityEngine.Vector2(4163, 1457),
            new UnityEngine.Vector2(3669, 1025),
            new UnityEngine.Vector2(2985, 672),
            new UnityEngine.Vector2(2578, 1389),
        };
        #endregion

        #region Public methods
        /// <summary>
        /// Khởi tạo dữ liệu Bạch Hổ Đường
        /// </summary>
        public static void Init()
        {
            XElement xmlNode = KTGlobal.ReadXMLData("Config/KT_GameEvents/BaiHuTang.xml");
            BaiHuTang.InitPrepare(xmlNode);
            BaiHuTang.InitEnd(xmlNode);
            BaiHuTang.InitRound1(xmlNode);
            BaiHuTang.InitRound2(xmlNode);
            BaiHuTang.InitRound3(xmlNode);
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Khởi tạo vòng chuẩn bị
        /// </summary>
        /// <param name="rootNode"></param>
        private static void InitPrepare(XElement rootNode)
        {
            BaiHuTang.Prepare = new RoundInfo()
            {
                Activities = new Dictionary<int, RoundInfo.ActivityInfo>(),
            };
            foreach (XElement node in rootNode.Element("Prepare").Elements("Activity"))
            {
                RoundInfo.ActivityInfo activityInfo = RoundInfo.ActivityInfo.Parse(node);
                BaiHuTang.Prepare.Activities[activityInfo.ActivityID] = activityInfo;
            }
        }

        /// <summary>
        /// Khởi tạo kết thúc sự kiện
        /// </summary>
        /// <param name="rootNode"></param>
        private static void InitEnd(XElement rootNode)
        {
            BaiHuTang.End = new RoundInfo()
            {
                Activities = new Dictionary<int, RoundInfo.ActivityInfo>(),
            };
            foreach (XElement node in rootNode.Element("End").Elements("Activity"))
            {
                RoundInfo.ActivityInfo activityInfo = RoundInfo.ActivityInfo.Parse(node);
                BaiHuTang.End.Activities[activityInfo.ActivityID] = activityInfo;
            }
        }

        /// <summary>
        /// Khởi tạo vòng 1
        /// </summary>
        /// <param name="rootNode"></param>
        private static void InitRound1(XElement rootNode)
        {
            BaiHuTang.Round1 = new RoundInfo()
            {
                RoundName = "Bạch Hổ Đường 1",
                Repute = RoundInfo.ReputeInfo.Parse(rootNode.Element("Round1").Element("ReputeInfo")),
                Activities = new Dictionary<int, RoundInfo.ActivityInfo>(),
                Monsters = new List<RoundInfo.MonsterInfo>(),
                Boss = RoundInfo.BossInfo.Parse(rootNode.Element("Round1").Element("Boss")),
                Teleport = RoundInfo.TeleportInfo.Parse(rootNode.Element("Round1").Element("Teleport")),
                NextMaps = new Dictionary<int, RoundInfo.NextMapInfo>(),
                OutMaps = new Dictionary<int, RoundInfo.OutMapInfo>(),
            };

            foreach (XElement node in rootNode.Element("Round1").Elements("Activity"))
            {
                RoundInfo.ActivityInfo activityInfo = RoundInfo.ActivityInfo.Parse(node);
                BaiHuTang.Round1.Activities[activityInfo.ActivityID] = activityInfo;
            }
            foreach (XElement node in rootNode.Element("Round1").Element("Monsters").Elements("Monster"))
            {
                RoundInfo.MonsterInfo monsterInfo = RoundInfo.MonsterInfo.Parse(node);
                BaiHuTang.Round1.Monsters.Add(RoundInfo.MonsterInfo.Parse(node));
            }
            foreach (XElement node in rootNode.Element("Round1").Element("OutMapInfo").Elements("Map"))
            {
                RoundInfo.OutMapInfo mapInfo = RoundInfo.OutMapInfo.Parse(node);
                BaiHuTang.Round1.OutMaps[mapInfo.FromMapID] = mapInfo;
            }
            foreach (XElement node in rootNode.Element("Round1").Element("NextMapInfo").Elements("Map"))
            {
                RoundInfo.NextMapInfo mapInfo = RoundInfo.NextMapInfo.Parse(node);
                BaiHuTang.Round1.NextMaps[mapInfo.FromMapID] = mapInfo;
            }
        }

        /// <summary>
        /// Khởi tạo vòng 2
        /// </summary>
        /// <param name="rootNode"></param>
        private static void InitRound2(XElement rootNode)
        {
            BaiHuTang.Round2 = new RoundInfo()
            {
                RoundName = "Bạch Hổ Đường 2",
                Repute = RoundInfo.ReputeInfo.Parse(rootNode.Element("Round2").Element("ReputeInfo")),
                Activities = new Dictionary<int, RoundInfo.ActivityInfo>(),
                Monsters = new List<RoundInfo.MonsterInfo>(),
                Boss = RoundInfo.BossInfo.Parse(rootNode.Element("Round2").Element("Boss")),
                Teleport = RoundInfo.TeleportInfo.Parse(rootNode.Element("Round2").Element("Teleport")),
                NextMaps = new Dictionary<int, RoundInfo.NextMapInfo>(),
                OutMaps = new Dictionary<int, RoundInfo.OutMapInfo>(),
            };

            foreach (XElement node in rootNode.Element("Round2").Elements("Activity"))
            {
                RoundInfo.ActivityInfo activityInfo = RoundInfo.ActivityInfo.Parse(node);
                BaiHuTang.Round2.Activities[activityInfo.ActivityID] = activityInfo;
            }
            foreach (XElement node in rootNode.Element("Round2").Element("Monsters").Elements("Monster"))
            {
                RoundInfo.MonsterInfo monsterInfo = RoundInfo.MonsterInfo.Parse(node);
                BaiHuTang.Round2.Monsters.Add(RoundInfo.MonsterInfo.Parse(node));
            }
            foreach (XElement node in rootNode.Element("Round2").Element("OutMapInfo").Elements("Map"))
            {
                RoundInfo.OutMapInfo mapInfo = RoundInfo.OutMapInfo.Parse(node);
                BaiHuTang.Round2.OutMaps[mapInfo.FromMapID] = mapInfo;
            }
            foreach (XElement node in rootNode.Element("Round2").Element("NextMapInfo").Elements("Map"))
            {
                RoundInfo.NextMapInfo mapInfo = RoundInfo.NextMapInfo.Parse(node);
                BaiHuTang.Round2.NextMaps[mapInfo.FromMapID] = mapInfo;
            }
        }

        /// <summary>
        /// Khởi tạo vòng 3
        /// </summary>
        /// <param name="rootNode"></param>
        private static void InitRound3(XElement rootNode)
        {
            BaiHuTang.Round3 = new RoundInfo()
            {
                RoundName = "Bạch Hổ Đường 3",
                Repute = RoundInfo.ReputeInfo.Parse(rootNode.Element("Round3").Element("ReputeInfo")),
                Activities = new Dictionary<int, RoundInfo.ActivityInfo>(),
                Monsters = new List<RoundInfo.MonsterInfo>(),
                Boss = RoundInfo.BossInfo.Parse(rootNode.Element("Round3").Element("Boss")),
                Teleport = RoundInfo.TeleportInfo.Parse(rootNode.Element("Round3").Element("Teleport")),
                OutMaps = new Dictionary<int, RoundInfo.OutMapInfo>(),
            };

            foreach (XElement node in rootNode.Element("Round3").Elements("Activity"))
            {
                RoundInfo.ActivityInfo activityInfo = RoundInfo.ActivityInfo.Parse(node);
                BaiHuTang.Round3.Activities[activityInfo.ActivityID] = activityInfo;
            }
            foreach (XElement node in rootNode.Element("Round3").Element("Monsters").Elements("Monster"))
            {
                RoundInfo.MonsterInfo monsterInfo = RoundInfo.MonsterInfo.Parse(node);
                BaiHuTang.Round3.Monsters.Add(RoundInfo.MonsterInfo.Parse(node));
            }
            foreach (XElement node in rootNode.Element("Round3").Element("OutMapInfo").Elements("Map"))
            {
                RoundInfo.OutMapInfo mapInfo = RoundInfo.OutMapInfo.Parse(node);
                BaiHuTang.Round3.OutMaps[mapInfo.FromMapID] = mapInfo;
            }
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Kiểm tra người chơi có đang ở trong bản đồ Bạch Hổ Đường không
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public static bool IsInBaiHuTang(KPlayer player)
		{
            return player.CurrentMapCode >= 225 && player.CurrentMapCode <= 240 && player.CurrentMapCode != 233 && player.CurrentMapCode != 225;

        }

        /// <summary>
        /// Bắt đầu báo danh Bạch Hổ Đường
        /// </summary>
        public static void BeginRegister(KTActivity activity)
        {
            BaiHuTang.CurrentStage = 1;
        }
        #endregion
    }
}
