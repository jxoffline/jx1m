using GameServer.KiemThe.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static GameServer.KiemThe.Utilities.KTEnum;

namespace GameServer.KiemThe.Entities.Skill
{
    /// <summary>
    /// Đối tượng quản lý kỹ năng
    /// </summary>
    public class SkillDataXML
    {
        /// <summary>
        /// Đối tượng quản lý chỉ số theo cấp độ
        /// </summary>
        public class SkillLevelData
        {
            /// <summary>
            /// Cấp độ
            /// </summary>
            public int Level { get; set; }

            /// <summary>
            /// % nội lực tối thiểu
            /// </summary>
            public int MinMPPercent { get; set; }

            /// <summary>
            /// % sinh lực tối thiểu
            /// </summary>
            public int MinHPPercent { get; set; }

            /// <summary>
            /// % thể lực tối thiểu
            /// </summary>
            public int MinVitalityPercent { get; set; }

            /// <summary>
            /// % nội lực tiêu hao
            /// </summary>
            public int MPPercentCost { get; set; }

            /// <summary>
            /// % sinh lực tiêu hao
            /// </summary>
            public int HPPercentCost { get; set; }

            /// <summary>
            /// % thể lực tiêu hao
            /// </summary>
            public int VitalityPercentCost { get; set; }

            /// <summary>
            /// Nội lực tối thiểu
            /// </summary>
            public int MP { get; set; }

            /// <summary>
            /// Sinh lực tối thiểu
            /// </summary>
            public int HP { get; set; }

            /// <summary>
            /// Thể lực tối thiểu
            /// </summary>
            public int Vitality { get; set; }

            /// <summary>
            /// Tiêu hao nội lực
            /// </summary>
            public int MPCost { get; set; }

            /// <summary>
            /// Tiêu hao sinh lực
            /// </summary>
            public int HPCost { get; set; }

            /// <summary>
            /// Thể lực tiêu hao
            /// </summary>
            public int VitalityCost { get; set; }

            /// <summary>
            /// Cự ly xuất chiêu
            /// </summary>
            public int CastRange { get; set; }

            /// <summary>
            /// Thời gian phục hồi
            /// </summary>
            public int Cooldown { get; set; }

            #region Property
            /// <summary>
            /// Thuộc tính
            /// </summary>
            public PropertyDictionary Properties { get; set; }
            #endregion

            #region State
            /// <summary>
            /// Thông tin trạng thái
            /// </summary>
            public class State
            {
                /// <summary>
                /// Loại trạng thái
                /// </summary>
                public ElementState Type { get; set; }

                /// <summary>
                /// Thời gian duy trì
                /// </summary>
                public float Duration { get; set; }

                /// <summary>
                /// Tỷ lệ kích hoạt
                /// </summary>
                public int ActivatePercent { get; set; }
            }
            #endregion

            #region Buff
            /// <summary>
            /// Buff
            /// </summary>
            public class Buff
            {
                /// <summary>
                /// ID Buff
                /// </summary>
                public int ID { get; set; }

                /// <summary>
                /// Tỷ lệ kích hoạt
                /// </summary>
                public int ActivatePercent { get; set; }
            }
            #endregion

            #region Bullet
            /// <summary>
            /// Định nghĩa đạn
            /// </summary>
            public class Bullet
            {
                /// <summary>
                /// Khoảng cách đạn bay tối đa
                /// </summary>
                public int BulletFlyDistance { get; set; }

                /// <summary>
                /// Tổng số mục tiêu chạm phái tối đa
                /// </summary>
                public int BulletMaxTargetTouch { get; set; }

                /// <summary>
                /// Tốc độ bay của đạn
                /// </summary>
                public int BulletVelocity { get; set; }

                /// <summary>
                /// Phạm vi phát nổ của đạn
                /// </summary>
                public int BulletExplodeRadius { get; set; }

                /// <summary>
                /// Biểu diễn đường bay đạn bằng Code
                /// </summary>
                public string BulletFunction { get; set; }

                /// <summary>
                /// Trạng thái khi đạn nổ
                /// </summary>
                public List<State> States { get; set; }

                /// <summary>
                /// Hiệu ứng khi đạn nổ
                /// </summary>
                public List<Buff> Buffs { get; set; }

                /// <summary>
                /// Dữ liệu
                /// </summary>
                public PropertyDictionary Properties { get; set; }

                /// <summary>
                /// ID hiệu ứng
                /// </summary>
                public int AnimationID { get; set; }
            }
            #endregion

            #region Trap
            /// <summary>
            /// Bẫy
            /// </summary>
            public class Trap
            {
                /// <summary>
                /// Thời gian tồn tại bẫy tối đa
                /// </summary>
                public int TrapMaxLifeTime { get; set; }

                /// <summary>
                /// Số mục tiêu tối đa
                /// </summary>
                public int TrapTotalTarget { get; set; }

                /// <summary>
                /// Phạm vi nổ bẫy
                /// </summary>
                public int TrapExplodeRadius { get; set; }

                /// <summary>
                /// Thời gian giãn cách mỗi lần bẫy nổ (-1 nếu chạm mục tiêu thì bẫy tự động bị xóa)
                /// </summary>
                public int TrapExplodePeriodTime { get; set; }

                /// <summary>
                /// Số cạm bẫy tối đa cùng loại
                /// </summary>
                public int TrapMaxNumber { get; set; }

                /// <summary>
                /// Danh sách các đia đạn phát ra khi bẫy phát nổ
                /// </summary>
                public List<Bullet> ExplodeBullets { get; set; }

                /// <summary>
                /// Trạng thái khi bẫy nổ
                /// </summary>
                public List<State> States { get; set; }

                /// <summary>
                /// Hiệu ứng khi bẫy nổ
                /// </summary>
                public List<Buff> Buffs { get; set; }

                /// <summary>
                /// Dữ liệu
                /// </summary>
                public PropertyDictionary Properties { get; set; }

                /// <summary>
                /// ID hiệu ứng
                /// </summary>
                public int AnimationID { get; set; }
            }
            #endregion

            #region Skill
            /// <summary>
            /// Kỹ năng khác
            /// </summary>
            public class Skill
            {
                /// <summary>
                /// ID kỹ năng
                /// </summary>
                public int ID { get; set; }

                /// <summary>
                /// Thuộc tính hỗ trợ
                /// </summary>
                public PropertyDictionary Properties { get; set; }
            }
            #endregion

            /// <summary>
            /// Danh sách các tia đạn
            /// </summary>
            public List<Bullet> Bullets { get; set; }

            /// <summary>
            /// Danh sách bẫy
            /// </summary>
            public List<Trap> Traps { get; set; }

            /// <summary>
            /// Buff khi kích hoạt kỹ năng
            /// </summary>
            public List<Buff> Buffs { get; set; }

            /// <summary>
            /// Danh sách kỹ năng hỗ trợ
            /// </summary>
            public List<Skill> SupportSkills { get; set; }
        }

        /// <summary>
        /// ID kỹ năng
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// ID môn phái
        /// </summary>
        public int FactionID { get; set; }

        /// <summary>
        /// ID nhánh tu luyện
        /// </summary>
        public int FactionSubID { get; set; }

        /// <summary>
        /// Tên kỹ năng
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Mô tả ngắn
        /// </summary>
        public string ShortDescription { get; set; }

        /// <summary>
        /// Mô tả đầy đủ
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Loại kỹ năng
        /// </summary>
        public SkillType Type { get; set; }

        /// <summary>
        /// Loại vũ khí
        /// </summary>
        public int EquipType { get; set; }

        /// <summary>
        /// ID động tác xuất chiêu
        /// </summary>
        public int ActionID { get; set; }

        /// <summary>
        /// Tên Icon
        /// </summary>
        public string IconName { get; set; }

        /// <summary>
        /// Có thể chủ động học
        /// </summary>
        public bool CanInitiativeLearn { get; set; }

        /// <summary>
        /// Yêu cầu cấp độ
        /// </summary>
        public int RequireLevel { get; set; }

        /// <summary>
        /// Cấp kỹ năng học được tối đa
        /// </summary>
        public int MaxLevel { get; set; }

        /// <summary>
        /// Có thể chủ động
        /// </summary>
        public bool CanUseWhileRiding { get; set; }

        /// <summary>
        /// ID hiệu ứng xuất chiêu
        /// </summary>
        public int CastEffectID { get; set; }

        /// <summary>
        /// Âm thanh xuất chiêu của nhân vật nam
        /// </summary>
        public string MaleCastSound { get; set; }

        /// <summary>
        /// Âm thanh xuất chiêu của nhân vật nữ
        /// </summary>
        public string FemaleCastSound { get; set; }

        /// <summary>
        /// Danh sách chi tiết cấp độ kỹ năng
        /// </summary>
        public Dictionary<int, SkillLevelData> LevelData { get; set; }

        /// <summary>
        /// Chuyển đối tượng từ XML Node
        /// </summary>
        /// <param name="xmlNode"></param>
        /// <returns></returns>
        public static SkillDataXML Parse(XElement xmlNode)
        {
            SkillDataXML skillDataXML = new SkillDataXML
            {
                ID = int.Parse(xmlNode.Attribute("ID").Value),
                ShortDescription = xmlNode.Attribute("ShortDesc").Value,
                Description = xmlNode.Attribute("Desc").Value,
                FactionID = int.Parse(xmlNode.Attribute("FactionID").Value),
                FactionSubID = int.Parse(xmlNode.Attribute("FactionSubID").Value),
                IconName = xmlNode.Attribute("IconName").Value,
                EquipType = int.Parse(xmlNode.Attribute("EquipType").Value),
                Type = (SkillType)int.Parse(xmlNode.Attribute("Type").Value),
                ActionID = int.Parse(xmlNode.Attribute("ActionID").Value),
                CanInitiativeLearn = int.Parse(xmlNode.Attribute("CanInitiativeLearn").Value) == 1,
                RequireLevel = int.Parse(xmlNode.Attribute("RequireLevel").Value),
                MaxLevel = int.Parse(xmlNode.Attribute("MaxLevel").Value),
                CanUseWhileRiding = int.Parse(xmlNode.Attribute("CanUseWhileRiding").Value) == 1,
                CastEffectID = int.Parse(xmlNode.Attribute("CastEffectID").Value),
                MaleCastSound = xmlNode.Attribute("MaleCastSound").Value,
                FemaleCastSound = xmlNode.Attribute("FemaleCastSound").Value,
                LevelData = new Dictionary<int, SkillLevelData>()
            };

            foreach (XElement element in xmlNode.Elements("SkillLevel"))
            {
                SkillLevelData levelData = new SkillLevelData()
                {
                    Level = int.Parse(element.Attribute("Level").Value),
                    MinHPPercent = element.Attribute("MinHPPercent") != null ? int.Parse(element.Attribute("MinHPPercent").Value) : -1,
                    MinMPPercent = element.Attribute("MinMPPercent") != null ? int.Parse(element.Attribute("MinMPPercent").Value) : -1,
                    MinVitalityPercent = element.Attribute("MinVitalityPercent") != null ? int.Parse(element.Attribute("MinVitalityPercent").Value) : -1,
                    MPPercentCost = element.Attribute("MPPercentCost") != null ? int.Parse(element.Attribute("MPPercentCost").Value) : -1,
                    HPPercentCost = element.Attribute("HPPercentCost") != null ? int.Parse(element.Attribute("HPPercentCost").Value) : -1,
                    VitalityPercentCost = element.Attribute("VitalityPercentCost") != null ? int.Parse(element.Attribute("VitalityPercentCost").Value) : -1,
                    MP = element.Attribute("MP") != null ? int.Parse(element.Attribute("MP").Value) : -1,
                    HP = element.Attribute("HP") != null ? int.Parse(element.Attribute("HP").Value) : -1,
                    Vitality = element.Attribute("Vitality") != null ? int.Parse(element.Attribute("Vitality").Value) : -1,
                    MPCost = element.Attribute("MPCost") != null ? int.Parse(element.Attribute("MPCost").Value) : -1,
                    HPCost = element.Attribute("HPCost") != null ? int.Parse(element.Attribute("HPCost").Value) : -1,
                    VitalityCost = element.Attribute("VitalityCost") != null ? int.Parse(element.Attribute("VitalityCost").Value) : -1,
                    CastRange = element.Attribute("CastRange") != null ? int.Parse(element.Attribute("CastRange").Value) : -1,
                    Cooldown = element.Attribute("Cooldown") != null ? int.Parse(element.Attribute("Cooldown").Value) : -1,

                    Bullets = new List<SkillLevelData.Bullet>(),
                    Traps = new List<SkillLevelData.Trap>(),
                    Buffs = new List<SkillLevelData.Buff>(),
                    SupportSkills = new List<SkillLevelData.Skill>(),
                };
                skillDataXML.LevelData[levelData.Level] = levelData;

                levelData.Properties = PropertyDictionary.Parse(element.Element("Properties"));

                #region Bullet
                foreach (XElement bulletNode in element.Element("Bullets").Elements("Bullet"))
                {
                    SkillLevelData.Bullet bullet = new SkillLevelData.Bullet()
                    {
                        BulletFlyDistance = int.Parse(bulletNode.Attribute("FlyDistance").Value),
                        BulletVelocity = int.Parse(bulletNode.Attribute("Velocity").Value),
                        BulletMaxTargetTouch = int.Parse(bulletNode.Attribute("MaxTarget").Value),
                        BulletExplodeRadius = int.Parse(bulletNode.Attribute("ExplodeRadius").Value),
                        AnimationID = int.Parse(bulletNode.Attribute("AnimationID").Value),
                        BulletFunction = bulletNode.Attribute("Function").Value,
                        States = new List<SkillLevelData.State>(),
                        Buffs = new List<SkillLevelData.Buff>(),
                    };
                    levelData.Bullets.Add(bullet);

                    bullet.Properties = PropertyDictionary.Parse(bulletNode.Element("Properties"));

                    foreach (XElement stateNode in bulletNode.Element("States").Elements("State"))
                    {
                        SkillLevelData.State state = new SkillLevelData.State()
                        {
                            Type = (ElementState)int.Parse(stateNode.Attribute("Type").Value),
                            Duration = float.Parse(stateNode.Attribute("Duration").Value),
                            ActivatePercent = int.Parse(stateNode.Attribute("ActivatePercent").Value),
                        };
                        bullet.States.Add(state);
                    }

                    foreach (XElement stateNode in bulletNode.Element("Buffs").Elements("Buff"))
                    {
                        SkillLevelData.Buff buff = new SkillLevelData.Buff()
                        {
                            ID = int.Parse(stateNode.Attribute("ID").Value),
                            ActivatePercent = int.Parse(stateNode.Attribute("ActivatePercent").Value),
                        };
                        bullet.Buffs.Add(buff);
                    }
                }
                #endregion

                #region Traps
                foreach (XElement trapNode in element.Element("Traps").Elements("Trap"))
                {
                    SkillLevelData.Trap trap = new SkillLevelData.Trap()
                    {
                        TrapMaxLifeTime = int.Parse(trapNode.Attribute("LifeTime").Value),
                        TrapTotalTarget = int.Parse(trapNode.Attribute("TotalTarget").Value),
                        TrapExplodeRadius = int.Parse(trapNode.Attribute("ExplodeRadius").Value),
                        TrapExplodePeriodTime = int.Parse(trapNode.Attribute("ExplodePeriod").Value),
                        TrapMaxNumber = int.Parse(trapNode.Attribute("MaxCount").Value),
                        AnimationID = int.Parse(trapNode.Attribute("AnimationID").Value),
                        States = new List<SkillLevelData.State>(),
                        Buffs = new List<SkillLevelData.Buff>(),
                        ExplodeBullets = new List<SkillLevelData.Bullet>(),
                    };
                    levelData.Traps.Add(trap);

                    trap.Properties = PropertyDictionary.Parse(trapNode.Element("Properties"));

                    foreach (XElement stateNode in trapNode.Element("States").Elements("State"))
                    {
                        SkillLevelData.State state = new SkillLevelData.State()
                        {
                            Type = (ElementState)int.Parse(stateNode.Attribute("Type").Value),
                            Duration = float.Parse(stateNode.Attribute("Duration").Value),
                            ActivatePercent = int.Parse(stateNode.Attribute("ActivatePercent").Value),
                        };
                        trap.States.Add(state);
                    }

                    foreach (XElement stateNode in trapNode.Element("Buffs").Elements("Buff"))
                    {
                        SkillLevelData.Buff buff = new SkillLevelData.Buff()
                        {
                            ID = int.Parse(stateNode.Attribute("ID").Value),
                            ActivatePercent = int.Parse(stateNode.Attribute("ActivatePercent").Value),
                        };
                        trap.Buffs.Add(buff);
                    }


                    foreach (XElement bulletNode in element.Element("ExplodeBullets").Elements("Bullet"))
                    {
                        SkillLevelData.Bullet bullet = new SkillLevelData.Bullet()
                        {
                            BulletFlyDistance = int.Parse(bulletNode.Attribute("FlyDistance").Value),
                            BulletVelocity = int.Parse(bulletNode.Attribute("Velocity").Value),
                            BulletMaxTargetTouch = int.Parse(bulletNode.Attribute("MaxTarget").Value),
                            BulletExplodeRadius = int.Parse(bulletNode.Attribute("ExplodeRadius").Value),
                            AnimationID = int.Parse(bulletNode.Attribute("AnimationID").Value),
                            BulletFunction = bulletNode.Attribute("Function").Value,
                            States = new List<SkillLevelData.State>(),
                            Buffs = new List<SkillLevelData.Buff>(),
                        };
                        trap.ExplodeBullets.Add(bullet);

                        bullet.Properties = PropertyDictionary.Parse(bulletNode.Element("Properties"));

                        foreach (XElement stateNode in bulletNode.Element("States").Elements("State"))
                        {
                            SkillLevelData.State state = new SkillLevelData.State()
                            {
                                Type = (ElementState)int.Parse(stateNode.Attribute("Type").Value),
                                Duration = float.Parse(stateNode.Attribute("Duration").Value),
                                ActivatePercent = int.Parse(stateNode.Attribute("ActivatePercent").Value),
                            };
                            bullet.States.Add(state);
                        }

                        foreach (XElement stateNode in bulletNode.Element("Buffs").Elements("Buff"))
                        {
                            SkillLevelData.Buff buff = new SkillLevelData.Buff()
                            {
                                ID = int.Parse(stateNode.Attribute("ID").Value),
                                ActivatePercent = int.Parse(stateNode.Attribute("ActivatePercent").Value),
                            };
                            bullet.Buffs.Add(buff);
                        }
                    }
                }
                #endregion

                #region Buffs
                foreach (XElement buffNode in element.Element("Buffs").Elements("Buff"))
                {
                    SkillLevelData.Buff buff = new SkillLevelData.Buff()
                    {
                        ID = int.Parse(buffNode.Attribute("ID").Value),
                        ActivatePercent = int.Parse(buffNode.Attribute("ActivatePercent").Value),
                    };
                    levelData.Buffs.Add(buff);
                }
                #endregion

                #region SupportSkill
                foreach (XElement supportSkillNode in element.Element("SupportSkills").Elements("Skill"))
                {
                    SkillLevelData.Skill skill = new SkillLevelData.Skill()
                    {
                        ID = int.Parse(supportSkillNode.Attribute("ID").Value),
                    };
                    levelData.SupportSkills.Add(skill);

                    skill.Properties = PropertyDictionary.Parse(supportSkillNode);
                }
                #endregion
            }

            return skillDataXML;
        }
    }
}
