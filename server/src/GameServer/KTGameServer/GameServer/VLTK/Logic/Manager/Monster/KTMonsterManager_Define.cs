using GameServer.KiemThe.Entities;
using System;
using System.Xml.Linq;

namespace GameServer.KiemThe.Logic
{
    /// <summary>
    /// Quản lý quái
    /// </summary>
    public static partial class KTMonsterManager
    {
        #region Template

        /// <summary>
        /// Template quái
        /// </summary>
        public class MonsterTemplateData
        {
            /// <summary>
            /// Tên
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Tên
            /// </summary>
            public string ResName { get; set; }

            /// <summary>
            /// ID
            /// </summary>
            public int Code { get; set; }

            /// <summary>
            /// Cấp độ
            /// </summary>
            public int Level { get; set; }

            /// <summary>
            /// Danh hiệu quái
            /// </summary>
            public string Title { get; set; }

            /// <summary>
            /// Sinh lực tối đa
            /// </summary>
            public int MaxHP { get; set; }
           

            /// <summary>
            /// Ngũ hành
            /// </summary>
            public KE_SERIES_TYPE Series { get; set; }

            /// <summary>
            /// Tốc độ di chuyển
            /// </summary>
            public int MoveSpeed { get; set; }

            /// <summary>
            /// Tốc đánh
            /// </summary>
            public int AtkSpeed { get; set; }

            /// <summary>
            /// Camp
            /// </summary>
            public int Camp { get; set; }

            /// <summary>
            /// Tổng số vật phẩm sẽ rơi
            /// </summary>
            public int Treasure { get; set; }

            /// <summary>
            /// Drop sẽ rơi
            /// </summary>
            public string DropProfile { get; set; }

            /// <summary>
            /// ID Script AI điều khiển
            /// </summary>
            public int AIID { get; set; }

            /// <summary>
            /// Loại quái
            /// </summary>
            public MonsterAIType MonsterType { get; set; }

            /// <summary>
            /// Danh sách kỹ năng vòng sáng
            /// </summary>
            public string Auras { get; set; }

            /// <summary>
            /// Danh sách kỹ năng được sử dụng
            /// </summary>
            public string Skills { get; set; }

            /// <summary>
            /// Chuyển đối tượng từ XMLNode
            /// </summary>
            /// <param name="xmlNode"></param>
            /// <returns></returns>
            public static MonsterTemplateData Parse(XElement xmlNode)
            {
                /// Tạo mới
                MonsterTemplateData monsterData = new MonsterTemplateData()
                {
                    Code = int.Parse(xmlNode.Attribute("ID").Value),
                    ResName = xmlNode.Attribute("ResName").Value,
                    Name = xmlNode.Attribute("Name").Value,
                    Title = xmlNode.Attribute("Title").Value,
                    MonsterType = (MonsterAIType)int.Parse(xmlNode.Attribute("MonsterType").Value),
                    Level = int.Parse(xmlNode.Attribute("Level").Value),
                    Treasure = int.Parse(xmlNode.Attribute("Treasure").Value),
                    Series = (KE_SERIES_TYPE)int.Parse(xmlNode.Attribute("Elemental").Value),
                    MaxHP = int.Parse(xmlNode.Attribute("MaxHP").Value),
                    MoveSpeed = int.Parse(xmlNode.Attribute("MoveSpeed").Value),
                    AtkSpeed = int.Parse(xmlNode.Attribute("AtkSpeed").Value),
                    AIID = int.Parse(xmlNode.Attribute("AIID").Value),
                    Camp = int.Parse(xmlNode.Attribute("Camp").Value),
                    DropProfile = xmlNode.Attribute("DropProfile").Value,
                    Auras = xmlNode.Attribute("Auras").Value,
                    Skills = xmlNode.Attribute("Skills").Value,
                };

                /// Nếu không có thì lấy mặc định
                if (monsterData.MaxHP == -1)
                {
                    monsterData.MaxHP = MonsterUtilities.GetMaxHPByLevel(monsterData.Level);
                }

                /// Tốc độ di chuyển
                if (monsterData.MonsterType == MonsterAIType.Static_ImmuneAll || monsterData.MonsterType == MonsterAIType.Static)
                {
                    monsterData.MoveSpeed = 0;
                }
                //else if (monsterData.MonsterType == MonsterAIType.Normal || monsterData.MonsterType == MonsterAIType.Hater || monsterData.MonsterType == MonsterAIType.Special_Normal)
                //{
                //    monsterData.MoveSpeed = 8;
                //}
                //else if (monsterData.MonsterType == MonsterAIType.Elite)
                //{
                //    monsterData.MoveSpeed = 10;
                //}
                //else if (monsterData.MonsterType == MonsterAIType.Leader)
                //{
                //    monsterData.MoveSpeed = 12;
                //}
                //else if (monsterData.MonsterType == MonsterAIType.Boss || monsterData.MonsterType == MonsterAIType.Pirate || monsterData.MonsterType == MonsterAIType.Special_Boss)
                //{
                //    monsterData.MoveSpeed = 14;
                //}

                /// Trả về kết quả
                return monsterData;
            }
        }

        #endregion Template

        #region Map Monster Zone

        /// <summary>
        /// Thông tin khu vực quái
        /// </summary>
        public class MapMonsterZoneData
        {
            /// <summary>
            /// ID template quái
            /// </summary>
            public int Code { get; set; }

            /// <summary>
            /// Vị trí X
            /// </summary>
            public int PosX { get; set; }

            /// <summary>
            /// Vị trí Y
            /// </summary>
            public int PosY { get; set; }

            /// <summary>
            /// Bán kính khu (số ô)
            /// </summary>
            public int Radius { get; set; }

            /// <summary>
            /// Tổng số quái trong khu
            /// </summary>
            public int MonsterCount { get; set; }

            /// <summary>
            /// Thời gian tái sinh quái (giây)
            /// </summary>
            public int RespawnTime { get; set; }

            /// <summary>
            /// Bãi quái này có hiện trên bản đồ khu vực không (chỉ áp dụng ở Client)
            /// </summary>
            public bool VisibleOnMinimap { get; set; }

            /// <summary>
            /// Chuyển đối tượng từ XMLNode
            /// </summary>
            /// <param name="xmlNode"></param>
            /// <returns></returns>
            public static MapMonsterZoneData Parse(XElement xmlNode)
            {
                MapMonsterZoneData monsterZone = new MapMonsterZoneData()
                {
                    Code = int.Parse(xmlNode.Attribute("Code").Value),
                    PosX = int.Parse(xmlNode.Attribute("X").Value),
                    PosY = int.Parse(xmlNode.Attribute("Y").Value),
                    Radius = int.Parse(xmlNode.Attribute("Radius").Value),
                    MonsterCount = int.Parse(xmlNode.Attribute("Num").Value),
                    RespawnTime = int.Parse(xmlNode.Attribute("Timeslot").Value),
                    VisibleOnMinimap = int.Parse(xmlNode.Attribute("VisibleOnMinimap").Value) == 1,
                };

                /// Trả về kết quả
                return monsterZone;
            }
        }

        #endregion Map Monster Zone

        #region Dynamic builder

        /// <summary>
        /// Tạo đối tượng quái di động (không quản lý bởi khu vực)
        /// </summary>
        public class DynamicMonsterBuilder
        {
            /// <summary>
            /// ID Res
            /// </summary>
            public int ResID { get; set; }

            /// <summary>
            /// ID bản đồ
            /// </summary>
            public int MapCode { get; set; }

            /// <summary>
            /// ID phụ bản (Mặc định là -1)
            /// </summary>
            public int CopySceneID { get; set; } = -1;

            /// <summary>
            /// Tọa độ X
            /// </summary>
            public int PosX { get; set; }

            /// <summary>
            /// Tọa độ Y
            /// </summary>
            public int PosY { get; set; }

            /// <summary>
            /// Tên quái (bỏ trống sẽ lấy ở File cấu hình hệ thống)
            /// </summary>
            public string Name { get; set; } = "";

            /// <summary>
            /// Danh hiệu quái (bỏ trống sẽ lấy ở File cấu hình hệ thống)
            /// </summary>
            public string Title { get; set; } = "";

            /// <summary>
            /// Cấp độ (-1 sẽ lấy ở file cấu hình)
            /// </summary>
            public int Level { get; set; } = -1;

            /// <summary>
            /// Sinh lực tối đa (-1 sẽ lấy ở file cấu hình)
            /// </summary>
            public int MaxHP { get; set; } = -1;

            /// <summary>
            /// Hướng quay quái (Direction.NONE sẽ lấy hướng ngẫu nhiên)
            /// </summary>
            public KiemThe.Entities.Direction Direction { get; set; } = KiemThe.Entities.Direction.NONE;

            /// <summary>
            /// Ngũ hành quái (KE_SERIES_TYPE.series_none sẽ lấy ngũ hành ngẫu nhiên)
            /// </summary>
            public KE_SERIES_TYPE Series { get; set; } = KE_SERIES_TYPE.series_none;

            /// <summary>
            /// ID Script AI điều khiển (-1 sẽ lấy ở File cấu hình)
            /// </summary>
            public int AIID { get; set; } = -1;

            /// <summary>
            /// Tag
            /// </summary>
            public string Tag { get; set; } = "";

            /// <summary>
            /// Thời điểm tái sinh sau khi chết (-1 sẽ không tái sinh)
            /// </summary>
            public long RespawnTick { get; set; } = -1;

            /// <summary>
            /// Điều kiện tái sinh sau khi chết
            /// </summary>
            public Func<bool> RespawnPredicate { get; set; } = null;

            /// <summary>
            /// Camp
            /// </summary>
            public int Camp { get; set; }

            /// <summary>
            /// Ghi lại lịch sử sát thương không
            /// </summary>
            public bool AllowRecordDamage { get; set; } = false;

            /// <summary>
            /// Loại quái (mặc định sẽ là quái thường tương ứng MonsterAIType.Normal)
            /// </summary>
            public MonsterAIType MonsterType { get; set; } = MonsterAIType.Normal;
        }

        #endregion Dynamic builder
    }
}