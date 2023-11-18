using FS.VLTK.Entities;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace Server.Data
{
    #region XML data
    /// <summary>
    /// Thuộc tính kỹ năng bang
    /// </summary>
    public class SkillProperty
    {
        /// <summary>
        /// Symbol thuộc tính
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Giá trị 1
        /// </summary>
        public int Value1 { get; set; }

        /// <summary>
        /// Giá trị 2
        /// </summary>
        public int Value2 { get; set; }

        /// <summary>
        /// Giá trị 3
        /// </summary>
        public int Value3 { get; set; }

        /// <summary>
        /// Chuyển đối tượng từ XMLNode
        /// </summary>
        /// <param name="xmlNode"></param>
        /// <returns></returns>
        public static SkillProperty Parse(XElement xmlNode)
        {
            return new SkillProperty()
            {
                Type = xmlNode.Attribute("PropType").Value,
                Value1 = int.Parse(xmlNode.Attribute("Value1").Value),
                Value2 = int.Parse(xmlNode.Attribute("Value2").Value),
                Value3 = int.Parse(xmlNode.Attribute("Value3").Value),
            };
        }
    }

    /// <summary>
    /// Vật phẩm yêu cầu
    /// </summary>
    public class ItemRequest
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
        /// Chuyển đối tượng từ XMLNode
        /// </summary>
        /// <param name="xmlNode"></param>
        /// <returns></returns>
        public static ItemRequest Parse(XElement xmlNode)
        {
            return new ItemRequest()
            {
                ItemID = int.Parse(xmlNode.Attribute("ItemID").Value),
                Quantity = int.Parse(xmlNode.Attribute("ItemNum").Value),
            };
        }
    }

    /// <summary>
    /// Thông tin kỹ năng theo cấp
    /// </summary>
    public class GuildSkillLevelData
    {
        /// <summary>
        /// Cấp độ của kỹ năng
        /// </summary>
        public int Level { get; set; }

        /// <summary>
        /// Những thuộc tính của cấp độ đó
        /// </summary>
        public List<SkillProperty> SkillProperties { get; set; }

        /// <summary>
        /// Lượng bang cống cần để thăng cấp
        /// </summary>
        public int RequireMoney { get; set; }

        /// <summary>
        /// Danh sách vật phẩm cần thăng cấp
        /// </summary>
        public List<ItemRequest> RequireItems { get; set; }

        /// <summary>
        /// Chuyển đối tượng từ XMLNode
        /// </summary>
        /// <param name="xmlNode"></param>
        /// <returns></returns>
        public static GuildSkillLevelData Parse(XElement xmlNode)
        {
            /// Tạo mới
            GuildSkillLevelData skillLevelData = new GuildSkillLevelData()
            {
                Level = int.Parse(xmlNode.Attribute("Level").Value),
                RequireMoney = int.Parse(xmlNode.Attribute("PointUpdate").Value),
                SkillProperties = new List<SkillProperty>(),
                RequireItems = new List<ItemRequest>(),
            };

            /// Duyệt danh sách thuộc tính kỹ năng
            foreach (XElement node in xmlNode.Elements("SkillProb"))
            {
                skillLevelData.SkillProperties.Add(SkillProperty.Parse(node));
            }

            /// Duyệt danh sách vật phẩm yêu cầu
            foreach (XElement node in xmlNode.Elements("ItemRequest"))
            {
                skillLevelData.RequireItems.Add(ItemRequest.Parse(node));
            }

            /// Trả về kết quả
            return skillLevelData;
        }
    }

    /// <summary>
    /// Thông tin kỹ năng bang hội
    /// </summary>
    public class GuildSkill
    {
        /// <summary>
        /// ID của kỹ năng
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// Tên của kỹ năng
        /// </summary>

        public string Name { get; set; }

        /// <summary>
        /// Icon của kỹ năng sẽ hiển thị ở client
        /// </summary>
        public string Icon { get; set; }

        /// <summary>
        /// Danh sách thuộc tính của kỹ năng bang
        /// </summary>
        public List<GuildSkillLevelData> LevelData { get; set; }

        /// <summary>
        /// Mô tả về kỹ năng này
        /// </summary>
        public string Desc { get; set; }

        /// <summary>
        /// Chuyển đối tượng từ XMLNode
        /// </summary>
        /// <param name="xmlNode"></param>
        /// <returns></returns>
        public static GuildSkill Parse(XElement xmlNode)
        {
            /// Tạo mới
            GuildSkill guildSkill = new GuildSkill()
            {
                ID = int.Parse(xmlNode.Attribute("ID").Value),
                Name = xmlNode.Attribute("Name").Value,
                Icon = xmlNode.Attribute("Icon").Value,
                Desc = xmlNode.Attribute("Desc").Value,
                LevelData = new List<GuildSkillLevelData>(),
            };

            /// Duyệt danh sách thăng cấp kỹ năng
            foreach (XElement node in xmlNode.Elements("LevelData"))
            {
                guildSkill.LevelData.Add(GuildSkillLevelData.Parse(node));
            }

            /// Trả về kết quả
            return guildSkill;
        }
    }

    /// <summary>
    /// Thông tin thăng cấp bang hội
    /// </summary>
    public class GuildLevelUp
    {
        /// <summary>
        /// Cập độ
        /// </summary>
        public int Level { get; set; }

        /// <summary>
        /// Kinh nghiệm yêu cầu
        /// </summary>
        public int RequireExp { get; set; }

        /// <summary>
        /// Bang cống yêu cầu
        /// </summary>
        public int RequireMoney { get; set; }

        /// <summary>
        /// Vật phẩm yêu cầu
        /// </summary>
        public List<ItemRequest> RequireItems { get; set; }

        /// <summary>
        /// Chuyển đối tượng từ XMLNode
        /// </summary>
        /// <param name="xmlNode"></param>
        /// <returns></returns>
        public static GuildLevelUp Parse(XElement xmlNode)
        {
            /// Tạo mới
            GuildLevelUp guildLevelUp = new GuildLevelUp()
            {
                Level = int.Parse(xmlNode.Attribute("Level").Value),
                RequireExp = int.Parse(xmlNode.Attribute("ExpRequest").Value),
                RequireMoney = int.Parse(xmlNode.Attribute("PointUpdate").Value),
                RequireItems = new List<ItemRequest>(),
            };

            /// Duyệt danh sách vật phẩm yêu cầu
            foreach (XElement node in xmlNode.Elements("ItemRequest"))
            {
                guildLevelUp.RequireItems.Add(ItemRequest.Parse(node));
            }

            /// Trả về kết quả
            return guildLevelUp;
        }
    }

    /// <summary>
    /// Yêu cầu cống hiến
    /// </summary>
    public class GuildDedication
    {
        /// <summary>
        /// Số điểm cống hiến cá nhân sẽ nhận được trên 1 bạc cống hiến
        /// </summary>
        public double PointPerGold { get; set; }

        /// <summary>
        /// Danh sách vật phẩm cống hiến quy đổi sang cống hiến cá nhân
        /// </summary>
        public List<DedicationItem> PointForItems { get; set; }

        /// <summary>
        /// Chuyển đối tượng từ XMLNode
        /// </summary>
        /// <param name="xmlNode"></param>
        /// <returns></returns>
        public static GuildDedication Parse(XElement xmlNode)
        {
            /// Tạo mới
            GuildDedication request = new GuildDedication()
            {
                PointPerGold = double.Parse(xmlNode.Attribute("PointPerGold").Value),
                PointForItems = new List<DedicationItem>(),
            };

            /// Duyệt danh sách vật phẩm yêu cầu
            foreach (XElement node in xmlNode.Elements("ItemDonate"))
            {
                request.PointForItems.Add(DedicationItem.Parse(node));
            }

            /// Trả về kết quả
            return request;
        }
    }

    /// <summary>
    /// Thông tin vật phẩm cống hiến quy sang cống hiến cá nhân tương ứng
    /// </summary>
    public class DedicationItem
    {
        /// <summary>
        /// ID vật phẩm
        /// </summary>
        public int ItemID { get; set; }

        /// <summary>
        /// Số điểm quy đổi
        /// </summary>
        public int Point { get; set; }

        /// <summary>
        /// Chuyển đối tượng từ XMLNode
        /// </summary>
        /// <param name="xmlNode"></param>
        /// <returns></returns>
        public static DedicationItem Parse(XElement xmlNode)
        {
            return new DedicationItem()
            {
                ItemID = int.Parse(xmlNode.Attribute("ItemID").Value),
                Point = int.Parse(xmlNode.Attribute("Point").Value),
            };
        }
    }

    /// <summary>
    /// Định nghĩa thông tin bang hội ở File cấu hình XML
    /// </summary>
    public class GuildInfoXML
    {
        /// <summary>
        /// Số bạc tiêu hao khi đổi nhiệm vụ
        /// </summary>
        public int ChangeQuestCost { get; set; }

        /// <summary>
        /// Một ngày làm được tối đa bao nhiêu nhiệm vụ
        /// </summary>
        public int MaxQuestPerDay { get; set; }

        /// <summary>
        /// Yêu cầu vật phẩm
        /// </summary>
        public int RequireItem { get; set; }

        /// <summary>
        /// Yêu cầu bạc
        /// </summary>
        public int RequireMoney { get; set; }

        /// <summary>
        /// Yêu cầu thêm
        /// </summary>
        public string RequireAdditionString { get; set; }

        /// <summary>
        /// Thông tin thăng cấp
        /// </summary>
        public List<GuildLevelUp> LevelUps { get; set; }

        /// <summary>
        /// Danh sách kỹ năng
        /// </summary>
        public List<GuildSkill> Skills { get; set; }

        /// <summary>
        /// Cống hiến
        /// </summary>
        public GuildDedication Dedication { get; set; }

        /// <summary>
        /// Chuyển đối tượng từ XMLNode
        /// </summary>
        /// <param name="xmlNode"></param>
        /// <returns></returns>
        public static GuildInfoXML Parse(XElement xmlNode)
        {
            /// Tạo mới
            GuildInfoXML guildInfo = new GuildInfoXML()
            {
                ChangeQuestCost = int.Parse(xmlNode.Attribute("ChangeQuestCost").Value),
                MaxQuestPerDay = int.Parse(xmlNode.Attribute("MaxQuestPerDay").Value),
                RequireItem = int.Parse(xmlNode.Attribute("RequireItem").Value),
                RequireMoney = int.Parse(xmlNode.Attribute("RequireMoney").Value),
                RequireAdditionString = xmlNode.Attribute("RequireAdditionString").Value,
                LevelUps = new List<GuildLevelUp>(),
                Skills = new List<GuildSkill>(),
            };

            /// Duyệt danh sách thăng cấp bang hội
            foreach (XElement node in xmlNode.Elements("LevelUps"))
            {
                guildInfo.LevelUps.Add(GuildLevelUp.Parse(node));
            }

            /// Duyệt danh sách kỹ năng bang hội
            foreach (XElement node in xmlNode.Elements("Skills"))
            {
                guildInfo.Skills.Add(GuildSkill.Parse(node));
            }

            /// Cống hiến bang hội
            guildInfo.Dedication = GuildDedication.Parse(xmlNode.Element("Donates"));

            /// Trả về kết quả
            return guildInfo;
        }

        /// <summary>
        /// Trả về thông tin yêu cầu kinh nghiệm thăng cấp theo cấp độ hiện tại
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        public int GetLevelRequireExp(int level)
        {
            /// Thông tin thăng cấp
            GuildLevelUp levelUpInfo = this.LevelUps.Where(x => x.Level == level).FirstOrDefault();
            /// Không tồn tại
            if (levelUpInfo == null)
            {
                /// Không có kết quả
                return 0;
            }
            /// Trả về kết quả
            return levelUpInfo.RequireExp;
        }
    }
    #endregion

    #region Network data
    /// <summary>
    /// Thông tin bang hội gửi về từ GS
    /// </summary>
    [ProtoContract]
    public class GuildInfo
    {
        /// <summary>
        /// Tổng cống hiến của bản thân
        /// </summary>
        [ProtoMember(1)]
        public int TotalDedications { get; set; }

        /// <summary>
        /// Cống hiến trong tuần của bản thân
        /// </summary>
        [ProtoMember(2)]
        public int WeekDedications { get; set; }

        /// <summary>
        /// Dữ liệu bang hội
        /// </summary>
        [ProtoMember(3)]
        public MiniGuildInfo Data { get; set; }
    }

    /// <summary>
    /// Thông tin nhiệm vụ bang
    /// </summary>
    [ProtoContract]
    public class GuildTask
    {
        /// <summary>
        /// ID bang
        /// </summary>
        [ProtoMember(1)]
        public int GuildID { get; set; } 
        /// <summary>
        /// ID nhiệm vụ
        /// </summary>
        [ProtoMember(2)]
        public int TaskID { get; set; } 

        /// <summary>
        /// Số lượng yêu cầu
        /// </summary>
        [ProtoMember(3)]
        public int TaskValue { get; set; }

        /// <summary>
        /// Ngày bắt đầu
        /// </summary>
        [ProtoMember(4)]
        public int DayCreate { get; set; } 

        /// <summary>
        /// Tổng số nhiệm vụ đã làm trong ngày
        /// </summary>
        [ProtoMember(5)]
        public int TaskCountInDay { get; set; } 
    }

    /// <summary>
    /// Dữ liệu bang hội
    /// </summary>
    [ProtoContract]
    public class MiniGuildInfo
    {
        /// <summary>
        /// ID của bang
        /// </summary>
        [ProtoMember(1)]
        public int GuildId { get; set; }

        /// <summary>
        /// Tên bang hội
        /// </summary>
        [ProtoMember(2)]
        public string GuildName { get; set; }

        /// <summary>
        /// Cấp bang hội
        /// </summary>
        [ProtoMember(3)]
        public int GuilLevel { get; set; }

        /// <summary>
        /// Exp hiện tại của bang
        /// </summary>
        [ProtoMember(4)]
        public int GuildExp { get; set; }

        /// <summary>
        /// Tên bang chủ
        /// </summary>
        [ProtoMember(5)]
        public string MasterName { get; set; }

        /// <summary>
        /// Tổng số thành viên
        /// </summary>
        [ProtoMember(6)]
        public int TotalMember { get; set; }


        /// <summary>
        /// Thông tin kỹ năng mở rộng
        /// </summary>
        [ProtoMember(7)]
        public string SkillNote { get; set; }

        /// <summary>
        /// Bang cống
        /// </summary>
        [ProtoMember(8)]
        public int GuildMoney { get; set; }

        /// <summary>
        /// Danh sách vật phẩm cống hiến vào bang
        /// </summary>
        /// <para>Mã hóa theo dạng ID_SL|...</para>
        [ProtoMember(9)]
        public string ItemStore { get; set; }

        /// <summary>
        /// Công cáo
        /// </summary>
        [ProtoMember(10)]
        public string Notification { get; set; }

        /// <summary>
        /// Danh sách nhiệm vụ
        /// </summary>
        [ProtoMember(11)]
        public GuildTask Tasks { get; set; }

        [ProtoMember(12)]
        public GuildWar GuildWar { get; set; }

        /// <summary>
        /// Trạng thái  
        /// 0 : Bang này chưa có cái đéo j cả.
        /// 1 : Bang này đang là chủ thành
        /// 2 : Bang này đã đang ký thi đấu
        /// 3 : Bang này sẽ là bang được chọn công thành cho ngày hôm sau
        /// </summary>
        [ProtoMember(13)]
        public int IsMainCity { get; set; }

        /// <summary>
        /// Số lượt vào phụ bản trong tuần
        /// </summary>
        [ProtoMember(14)]
        public int Total_Copy_Scenes_This_Week { get; set; }

    }

    [ProtoContract]
    public class GuildWar
    {
        [ProtoMember(1)]
        public int GuildID { get; set; }

        [ProtoMember(2)]
        public string GuildName { get; set; }

        [ProtoMember(3)]
        public string TeamList { get; set; }


        [ProtoMember(4)]
        public int WeekID { get; set; }
    }
    /// <summary>
    /// Tài nguyên bang
    /// </summary>
    public enum GUILD_RESOURCE
    {
        /// <summary>
        /// Bang cống
        /// </summary>
        GUILD_MONEY = 0,
        /// <summary>
        /// Vật phẩm
        /// </summary>
        ITEM = 1,
        /// <summary>
        /// Kỹ năng
        /// </summary>
        SKILL = 2,
        /// <summary>
        /// Kinh nghiệm
        /// </summary>
        EXP = 3,
    }

    /// <summary>
    /// Thông tin kỹ năng
    /// </summary>
    [ProtoContract]
    public class SkillDef
    {
        /// <summary>
        /// ID kỹ năng
        /// </summary>
        [ProtoMember(1)]
        public int SkillID { get; set; }

        /// <summary>
        /// Cấp độ
        /// </summary>
        [ProtoMember(2)]
        public int Level { get; set; }
    }

    /// <summary>
    /// Thông tin thành viên bang hội
    /// </summary>
    [ProtoContract]
    public class GuildMember
    {
        /// <summary>
        /// ID
        /// </summary>
        [ProtoMember(1)]
        public int RoleID { get; set; }

        /// <summary>
        /// Tên của thành viên
        /// </summary>
        [ProtoMember(2)]
        public string RoleName { get; set; }

        /// <summary>
        /// Phái nào
        /// </summary>
        [ProtoMember(3)]
        public int FactionID { get; set; }

        /// <summary>
        /// Cấp bậc
        /// </summary>
        [ProtoMember(4)]
        public int Rank { get; set; }

        /// <summary>
        /// Cấp độ
        /// </summary>
        [ProtoMember(5)]
        public int Level { get; set; }

        /// <summary>
        /// ID của GUild hiện tại
        /// </summary>
        [ProtoMember(6)]
        public int GuildID { get; set; }

        /// <summary>
        /// Cống hiến cá nhân
        /// </summary>
        [ProtoMember(7)]
        public int GuildMoney { get; set; }

        /// <summary>
        /// Trạng thái online
        /// </summary>
        [ProtoMember(8)]
        public int OnlineStatus { get; set; }

        /// <summary>
        /// ZoneID nào
        /// </summary>
        [ProtoMember(9)]
        public int ZoneID { get; set; }

        /// <summary>
        /// Tài phú
        /// </summary>
        [ProtoMember(10)]
        public long TotalValue { get; set; }

        /// <summary>
        /// Điểm hoạt động tuần
        /// </summary>
        [ProtoMember(11)]
        public int WeekPoint { get; set; }
    }

    /// <summary>
    /// Thông tin danh sách thành viên bang hội
    /// </summary>
    [ProtoContract]
    public class GuildMemberData
    {
        /// <summary>
        /// Danh sách thành viên
        /// </summary>
        [ProtoMember(1)]
        public List<GuildMember> TotalGuildMember { get; set; }

        /// <summary>
        /// Số trang hiện tại
        /// </summary>
        [ProtoMember(2)]
        public int PageIndex { get; set; }

        /// <summary>
        /// Tổng số trang
        /// </summary>
        [ProtoMember(3)]
        public int TotalPage { get; set; }
    }

    /// <summary>
    /// Yêu cầu xin vào bang
    /// </summary>
    [ProtoContract]
    public class RequestJoinInfo
    {
        /// <summary>
        /// Danh sách yêu cầu
        /// </summary>
        [ProtoMember(1)]
        public List<RequestJoin> TotalRequestJoin { get; set; }

        /// <summary>
        /// Tự tiếp nhận thành viên không
        /// </summary>
        [ProtoMember(2)]
        public int AutoAccept { get; set; }

        /// <summary>
        /// Quy tắc tự tiếp nhận thành viên
        /// Cấp|Phái|Tài phú
        /// </summary>
        [ProtoMember(3)]
        public string AutoAcceptRule { get; set; }

        /// <summary>
        /// Số trang hiện tại
        /// </summary>
        [ProtoMember(4)]
        public int PageIndex { get; set; }

        /// <summary>
        /// Tổng số trang
        /// </summary>
        [ProtoMember(5)]
        public int TotalPage { get; set; }
    }

    /// <summary>
    /// Thông tin người chơi yêu cầu vào bang
    /// </summary>
    [ProtoContract]
    public class RequestJoin
    {
        /// <summary>
        /// ID
        /// </summary>
        [ProtoMember(1)]
        public int ID { get; set; }

        /// <summary>
        /// ID người chơi
        /// </summary>
        [ProtoMember(2)]
        public int RoleID { get; set; }

        /// <summary>
        /// Tên người chơi
        /// </summary>
        [ProtoMember(3)]
        public string RoleName { get; set; }

        /// <summary>
        /// ID phái
        /// </summary>
        [ProtoMember(4)]
        public int RoleFactionID { get; set; }

        /// <summary>
        /// Tài phú
        /// </summary>
        [ProtoMember(5)]
        public long RoleValue { get; set; }

        /// <summary>
        /// ID bang hội xin vào
        /// </summary>
        [ProtoMember(6)]
        public int GuildID { get; set; }

        /// <summary>
        /// Thời gian xin vào
        /// </summary>
        [ProtoMember(7)]
        public DateTime TimeRequest { get; set; }

        /// <summary>
        /// Cấp độ người chơi
        /// </summary>
        [ProtoMember(8)]
        public int Level { get; set; }
    }

    /// <summary>
	/// Sửa tôn chỉ bang hội
	/// </summary>
	[ProtoContract]
    public class GuildChangeSlogan
    {
        /// <summary>
        /// ID bang hội
        /// </summary>
        [ProtoMember(1)]
        public int GuildID { get; set; }

        /// <summary>
        /// Tôn chỉ bang hội
        /// </summary>
        [ProtoMember(2)]
        public string Slogan { get; set; }
    }

    /// <summary>
    /// Thực thể công thành chiến
    /// </summary>
    [ProtoContract]
    public class CityInfo
    {
        /// <summary>
        /// Tên thành
        /// </summary>
        [ProtoMember(1)]
        public string CityName { get; set; }

        /// <summary>
        /// ID thành
        /// </summary>
        [ProtoMember(2)]
        public int CityID { get; set; }

        /// <summary>
        /// Thành chủ là bọn nào
        /// </summary>
        [ProtoMember(3)]
        public string HostName { get; set; }

        /// <summary>
        /// Ngày thi đấu loại
        /// </summary>
        [ProtoMember(4)]
        public int TeamFightDay { get; set; }

        /// <summary>
        /// Ngày công thành
        /// </summary>
        [ProtoMember(5)]
        public int CityFightDay { get; set; }

        /// <summary>
        /// Bản đồ sẽ diễn ra công thành chiến
        /// </summary>
        public int FightMapID { get; set; }
    }

    /// <summary>
    /// Thông tin thành viên
    /// </summary>
    [ProtoContract]
    public class MemberRegister
    {
        /// <summary>
        /// ID
        /// </summary>
        [ProtoMember(1)]
        public int RoleID { get; set; }

        /// <summary>
        /// Tên
        /// </summary>
        [ProtoMember(2)]
        public string RoleName { get; set; }

        /// <summary>
        /// Cấp
        /// </summary>
        [ProtoMember(3)]
        public int Level { get; set; }

        /// <summary>
        /// Phái
        /// </summary>
        [ProtoMember(4)]
        public int Faction { get; set; }
    }

    /// <summary>
    /// Thông tin khung công thành chiến
    /// </summary>
    [ProtoContract]
    public class GuildWarInfo
    {
        /// <summary>
        /// Danh sách thành viên tham gia vòng loại
        /// </summary>
        [ProtoMember(1)]
        public List<string> MemberRegister { get; set; }

        /// <summary>
        /// Danh sách bang đã đăng ký
        /// </summary>
        [ProtoMember(2)]
        public List<string> GuildResgister { get; set; }

        /// <summary>
        /// Trạng thái công thành
        /// <para>0 : Có thể đăng ký công thành | 1 : Đã hết thời gian đăng ký | 2 : Đang diễn ra công thành  |3 : Hết thời gian công thành</para>
        /// </summary>
        [ProtoMember(3)]
        public int Status { get; set; }

        /// <summary>
        /// Dánh sách các thành sẽ diễn ra công thành
        /// Nhưng theo vận hành nói nên giờ chỉ dùng 1 thành
        /// </summary>
        [ProtoMember(4)]
        public List<CityInfo> ListCity { get; set; }

        /// <summary>
        /// Thành nào sẽ diễn ra công thành trong tuần này
        /// </summary>
        [ProtoMember(5)]
        public int CityFightID { get; set; }

        /// <summary>
        /// Dánh sách bọn người chơi trong bang có thể chọn cho lên lôi đài thi đấu
        /// </summary>
        [ProtoMember(6)]
        public List<MemberRegister> ListMemberCanPick { get; set; }
    }
    #endregion
}