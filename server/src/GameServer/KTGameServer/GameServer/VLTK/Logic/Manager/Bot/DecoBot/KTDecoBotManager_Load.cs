using GameServer.KiemThe;
using GameServer.KiemThe.Logic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace GameServer.Logic
{
    /// <summary>
    /// Quản lý bot biểu diễn
    /// </summary>
    public static partial class KTDecoBotManager
    {
        /// <summary>
        /// Tải danh sách Bot biểu diễn trong bản đồ
        /// </summary>
        /// <param name="mapCode"></param>
        /// <param name="mapName"></param>
        public static bool LoadMapDecoBot(int mapCode, string mapName)
        {
            /// File tương ứng
            string fileName = KTGlobal.GetDataPath(string.Format("MapConfig/{0}/DecoBot.xml", mapName));
            /// Nếu File không tồn tại tức Map này không có Bot
            if (!File.Exists(fileName))
            {
                return true;
            }

            /// Nội dung File XML
            string xmlContent = File.ReadAllText(fileName);
            /// Đối tượng XElement chứa nội dung tương ứng
            XElement xmlNode = XElement.Parse(xmlContent);
            if (xmlNode == null)
            {
                return false;
            }

            /// Duyệt danh sách bot
            foreach (XElement node in xmlNode.Element("DecoBots").Elements("Bot"))
            {
                /// Tên
                string name = node.Attribute("Name").Value;
                /// Danh hiệu
                string title = node.Attribute("Title").Value;
                /// Vị trí
                int posX = int.Parse(node.Attribute("PosX").Value);
                int posY = int.Parse(node.Attribute("PosY").Value);
                /// Giới tính
                int sex = int.Parse(node.Attribute("Sex").Value);
                /// Trang bị
                int helmID = int.Parse(node.Attribute("HelmID").Value);
                int armorID = int.Parse(node.Attribute("ArmorID").Value);
                int weaponID = int.Parse(node.Attribute("WeaponID").Value);
                int weaponEnhanceLevel = int.Parse(node.Attribute("WeaponEnhanceLevel").Value);
                int weaponSeries = int.Parse(node.Attribute("WeaponSeries").Value);
                int mantleID = int.Parse(node.Attribute("MantleID").Value);
                int horseID = int.Parse(node.Attribute("HorseID").Value);
                /// Vòng sáng
                List<KDecoBot.BotSkillData> auras = new List<KDecoBot.BotSkillData>();
                {
                    /// Chuỗi thông tin
                    string infoStrings = node.Attribute("Auras").Value;
                    /// Nếu tồn tại
                    if (!string.IsNullOrEmpty(infoStrings))
                    {
                        /// Duyệt danh sách
                        foreach (string infoString in infoStrings.Split(';'))
                        {
                            /// Các trường
                            string[] fields = infoString.Split('_');
                            /// ID
                            int skillID = int.Parse(fields[0]);
                            /// Cấp
                            int skillLevel = int.Parse(fields[1]);
                            /// Thêm vào danh sách
                            auras.Add(new KDecoBot.BotSkillData()
                            {
                                SkillID = skillID,
                                SkillLevel = skillLevel,
                                Cooldown = -1,
                            });
                        }
                    }
                }
                /// Kỹ năng
                List<KDecoBot.BotSkillData> skills = new List<KDecoBot.BotSkillData>();
                {
                    /// Chuỗi thông tin
                    string infoStrings = node.Attribute("Skills").Value;
                    /// Nếu tồn tại
                    if (!string.IsNullOrEmpty(infoStrings))
                    {
                        /// Duyệt danh sách
                        foreach (string infoString in infoStrings.Split(';'))
                        {
                            /// Các trường
                            string[] fields = infoString.Split('_');
                            /// ID
                            int skillID = int.Parse(fields[0]);
                            /// Cấp
                            int skillLevel = int.Parse(fields[1]);
                            /// Phục hồi
                            int cooldown = int.Parse(fields[2]);
                            /// Thêm vào danh sách
                            skills.Add(new KDecoBot.BotSkillData()
                            {
                                SkillID = skillID,
                                SkillLevel = skillLevel,
                                Cooldown = cooldown,
                            });
                        }
                    }
                }

                /// Tạo Bot
                KDecoBot bot = new KDecoBot()
                {
                    RoleName = name,
                    MapCode = mapCode,
                    Title = title,
                    CurrentCopyMapID = -1,
                    CurrentPos = new System.Windows.Point(posX, posY),
                    RoleSex = sex,
                    HelmID = helmID,
                    ArmorID = armorID,
                    WeaponID = weaponID,
                    WeaponEnhanceLevel = weaponEnhanceLevel,
                    WeaponSeries = weaponSeries,
                    MantleID = mantleID,
                    HorseID = horseID,
                };
                /// Đổi camp
                bot.Camp = -1;
                /// Máu
                bot.ChangeLifeMax(1000, 0, 0);
                bot.m_CurrentLife = bot.m_CurrentLifeMax;
                /// Tốc chạy
                bot.ChangeRunSpeed(15, 0, 0);
                /// Tốc đánh
                bot.ChangeCastSpeed(50, 0);
                bot.ChangeAttackSpeed(50, 0);
                /// Miễn dịch toàn bộ
                bot.m_CurrentInvincibility = 1;
                bot.m_IgnoreAllSeriesStates = true;
                /// Vòng sáng
                bot.SetAuras(auras.ToArray());
                /// Kỹ năng
                bot.SetSkills(skills.ToArray());
                /// Tạo Bot
                KTDecoBotManager.Add(bot);
            }

            /// Duyệt danh sách cọc
            foreach (XElement node in xmlNode.Element("DecoBots").Elements("Column"))
            {
                /// ID Res
                int resID = int.Parse(node.Attribute("ResID").Value);
                /// Tên
                string name = node.Attribute("Name").Value;
                /// Vị trí
                int posX = int.Parse(node.Attribute("PosX").Value);
                int posY = int.Parse(node.Attribute("PosY").Value);

                /// Tạo quái
                KTMonsterManager.Create(new KTMonsterManager.DynamicMonsterBuilder()
                {
                    MapCode = mapCode,
                    ResID = resID,
                    PosX = posX,
                    PosY = posY,
                    Name = name,
                    MaxHP = 1,
                    Level = 1,
                    MonsterType = KiemThe.Entities.MonsterAIType.Static_ImmuneAll,
                    Tag = "DecoBotColumn",
                    Camp = 65535,
                    RespawnTick = 1000,
                });
            }

            return true;
        }
    }
}
