using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace FS.VLTK.Entities.Config
{
    /// <summary>
    /// Thiết lập bản đồ
    /// </summary>
    public class MapSetting : IDisposable
    {
        /// <summary>
        /// Tỷ lệ Drop (đơn vị %)
        /// </summary>
        public int DropRate { get; set; }

        /// <summary>
        /// Tỷ lệ nhận kinh nghiệm (đơn vị %)
        /// </summary>
        public int ExpRate { get; set; }

        /// <summary>
        /// Tỷ lệ rơi tiền (đơn vị %)
        /// </summary>
        public int MoneyRate { get; set; }

        /// <summary>
        /// Có phải phụ bản không
        /// </summary>
        public bool IsCopyScene { get; set; }

        /// <summary>
        /// Cho phép PK không tăng sát khí không
        /// </summary>
        public bool FreePK { get; set; }

        /// <summary>
        /// Cho phép PK không
        /// </summary>
        public bool AllowPK { get; set; }

        /// <summary>
        /// Cho phép mở sạp không
        /// </summary>
        public bool AllowStall { get; set; }

        /// <summary>
        /// Cho phép giao dịch không
        /// </summary>
        public bool AllowTrade { get; set; }

        /// <summary>
        /// Khi ở trạng thái PK mà chạy thì giảm thể lực bao nhiêu điểm
        /// </summary>
        public int PKAllSubStamina { get; set; }

        /// <summary>
        /// Khi ở trạng thái ngồi thì hồi sinh lực bao nhiêu điểm
        /// </summary>
        public int SitHealHP { get; set; }

        /// <summary>
        /// Khi ở trạng thái ngồi thì hồi nội lực bao nhiêu điểm
        /// </summary>
        public int SitHealMP { get; set; }

        /// <summary>
        /// Khi ở trạng thái ngồi thì hồi thể lực bao nhiêu điểm
        /// </summary>
        public int SitHealStamina { get; set; }

        /// <summary>
        /// Thời gian mỗi lần giảm sát khí đi 1 điểm
        /// <para>-1 nếu vô hiệu</para>
        /// </summary>
        public long SubPKValueTick { get; set; }

        /// <summary>
        /// Buộc chuyển trạng thái PK thành
        /// <para>-1 nếu không yêu cầu</para>
        /// </summary>
        public int ForceChangePKStatusTo { get; set; }

        /// <summary>
        /// Cho phép chủ động thay đổi trạng thái PK
        /// </summary>
        public bool AllowChangePKStatus { get; set; }

        /// <summary>
        /// Cho phép chủ động tuyên chiến với người chơi tương ứng
        /// </summary>
        public bool AllowFightTarget { get; set; }

        /// <summary>
        /// Cho phép tỷ thí không
        /// </summary>
        public bool AllowChallenge { get; set; }

        /// <summary>
        /// Cho phép sử dụng kỹ năng không
        /// </summary>
        public bool AllowUseSkill { get; set; }

        /// <summary>
        /// Cho phép chủ động sử dụng kỹ năng tấn công
        /// </summary>
        public bool AllowUseOffensiveSkill { get; set; }

        /// <summary>
        /// Danh sách kỹ năng bị cấm
        /// </summary>
        public HashSet<int> BanSkills { get; set; }

        /// <summary>
        /// Cho phép tổ đội không, nếu không có thì khi vào toàn bộ người sẽ bị xóa trạng thái đội
        /// </summary>
        public bool AllowTeam { get; set; }

        /// <summary>
        /// Cho phép tạo nhóm không
        /// </summary>
        public bool AllowCreateTeam { get; set; }

        /// <summary>
        /// Cho phép mời người chơi khác vào nhóm không
        /// </summary>
        public bool AllowInviteToTeam { get; set; }

        /// <summary>
        /// Cho phép trục xuất thành viên khỏi nhóm không
        /// </summary>
        public bool AllowKickFromTeam { get; set; }

        /// <summary>
        /// Cho phép chủ động rời nhóm không
        /// </summary>
        public bool AllowLeaveTeam { get; set; }

        /// <summary>
        /// Cho phép thành viên gia nhập nhóm không
        /// </summary>
        public bool AllowJoinTeam { get; set; }

        /// <summary>
        /// Cho phép thay đổi nhóm trưởng không
        /// </summary>
        public bool AllowChangeTeamLeader { get; set; }

        /// <summary>
        /// Cho phép dùng vật phẩm không
        /// </summary>
        public bool AllowUseItem { get; set; }

        /// <summary>
        /// Danh sách vật phẩm bị cấm sử dụng
        /// </summary>
        public HashSet<int> BanItems { get; set; }

        /// <summary>
        /// Cho phép hồi sinh tại chỗ (có thể bởi kỹ năng) không
        /// </summary>
        public bool AllowReviveAtPos { get; set; }

        /// <summary>
        /// Cho phép dùng Cửu Chuyển Hoàn Hồn Đơn không
        /// </summary>
        public bool AllowUseReviveMedicine { get; set; }

        /// <summary>
        /// Chuyển đối tượng từ XMLNode
        /// </summary>
        /// <param name="xmlNode"></param>
        /// <returns></returns>
        public static MapSetting Parse(XElement xmlNode)
        {
            MapSetting mapSetting = new MapSetting();

            XElement rateConfigNode = xmlNode.Element("RateConfig");
            mapSetting.DropRate = int.Parse(rateConfigNode.Attribute("DropRate").Value);
            mapSetting.ExpRate = int.Parse(rateConfigNode.Attribute("ExpRate").Value);
            mapSetting.MoneyRate = int.Parse(rateConfigNode.Attribute("MoneyRate").Value);

            XElement baseNode = xmlNode.Element("Base");
            mapSetting.IsCopyScene = bool.Parse(baseNode.Attribute("IsCopyScene").Value);
            mapSetting.FreePK = bool.Parse(baseNode.Attribute("FreePK").Value);
            mapSetting.AllowPK = bool.Parse(baseNode.Attribute("AllowPK").Value);
            mapSetting.AllowStall = bool.Parse(baseNode.Attribute("AllowStall").Value);
            mapSetting.AllowTrade = bool.Parse(baseNode.Attribute("AllowTrade").Value);
            mapSetting.PKAllSubStamina = int.Parse(baseNode.Attribute("PKAllSubStamina").Value);
            mapSetting.SitHealHP = int.Parse(baseNode.Attribute("SitHealHP").Value);
            mapSetting.SitHealMP = int.Parse(baseNode.Attribute("SitHealMP").Value);
            mapSetting.SitHealStamina = int.Parse(baseNode.Attribute("SitHealStamina").Value);

            XElement pkNode = xmlNode.Element("PK");
            mapSetting.SubPKValueTick = long.Parse(pkNode.Attribute("SubPKValueTick").Value);
            mapSetting.ForceChangePKStatusTo = string.IsNullOrEmpty(pkNode.Attribute("ForceChangePKStatusTo").Value) ? -1 : int.Parse(pkNode.Attribute("ForceChangePKStatusTo").Value);
            mapSetting.AllowChangePKStatus = bool.Parse(pkNode.Attribute("AllowChangePKStatus").Value);
            mapSetting.AllowFightTarget = bool.Parse(pkNode.Attribute("AllowFightTarget").Value);

            XElement challengeNode = xmlNode.Element("Challenge");
            mapSetting.AllowChallenge = bool.Parse(challengeNode.Attribute("AllowChallenge").Value);

            XElement skillNode = xmlNode.Element("Skill");
            mapSetting.AllowUseSkill = bool.Parse(skillNode.Attribute("AllowUseSkill").Value);
            mapSetting.AllowUseOffensiveSkill = bool.Parse(skillNode.Attribute("AllowUseOffensiveSkill").Value);
            mapSetting.BanSkills = new HashSet<int>();
            string banSkillsStr = skillNode.Attribute("BanSkills").Value;
            if (!string.IsNullOrEmpty(banSkillsStr))
            {
                string[] skillIDs = banSkillsStr.Split(';');
                foreach (string idStr in skillIDs)
                {
                    mapSetting.BanSkills.Add(int.Parse(idStr));
                }
            }

            XElement teamNode = xmlNode.Element("Team");
            mapSetting.AllowTeam = bool.Parse(teamNode.Attribute("AllowTeam").Value);
            mapSetting.AllowCreateTeam = bool.Parse(teamNode.Attribute("AllowCreateTeam").Value);
            mapSetting.AllowInviteToTeam = bool.Parse(teamNode.Attribute("AllowInviteToTeam").Value);
            mapSetting.AllowKickFromTeam = bool.Parse(teamNode.Attribute("AllowKickFromTeam").Value);
            mapSetting.AllowLeaveTeam = bool.Parse(teamNode.Attribute("AllowLeaveTeam").Value);
            mapSetting.AllowJoinTeam = bool.Parse(teamNode.Attribute("AllowJoinTeam").Value);
            mapSetting.AllowChangeTeamLeader = bool.Parse(teamNode.Attribute("AllowChangeTeamLeader").Value);

            XElement itemNode = xmlNode.Element("Item");
            mapSetting.AllowUseItem = bool.Parse(itemNode.Attribute("AllowUseItem").Value);
            mapSetting.BanItems = new HashSet<int>();
            string banItemsStr = itemNode.Attribute("BanItems").Value;
            if (!string.IsNullOrEmpty(banItemsStr))
            {
                string[] itemIDs = banItemsStr.Split(';');
                foreach (string idStr in itemIDs)
                {
                    mapSetting.BanItems.Add(int.Parse(idStr));
                }
            }

            XElement reviveNode = xmlNode.Element("Revive");
            mapSetting.AllowReviveAtPos = bool.Parse(reviveNode.Attribute("AllowReviveAtPos").Value);
            mapSetting.AllowUseReviveMedicine = bool.Parse(reviveNode.Attribute("AllowUseReviveMedicine").Value);

            return mapSetting;
        }

        /// <summary>
        /// Hủy đối tượng
        /// </summary>
        public void Dispose()
        {
            this.BanSkills.Clear();
            this.BanSkills = null;
            this.BanItems.Clear();
            this.BanItems = null;
        }
    }

    /// <summary>
    /// Đối tượng bản đồ đọc từ file
    /// </summary>
    public class Map
    {
        /// <summary>
        /// ID bản đồ
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// Tên bản đồ
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Mã Code của bản đồ
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Chiều rộng
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// Chiều cao
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// Đường dẫn Bundle chứa nhạc map
        /// </summary>
        public string MusicBundle { get; set; }

        /// <summary>
        /// Đường dẫn folder chứa ảnh
        /// </summary>
        public string ImageFolder { get; set; }

        /// <summary>
        /// Tên Minimap
        /// </summary>
        public string MinimapName { get; set; }

        /// <summary>
        /// Chiều rộng ảnh cắt nhỏ
        /// </summary>
        public int PartWidth { get; set; }

        /// <summary>
        /// Chiều cao ảnh cắt nhỏ
        /// </summary>
        public int PartHeight { get; set; }

        /// <summary>
        /// Tổng số ảnh chiều ngang
        /// </summary>
        public int HorizontalCount { get; set; }

        /// <summary>
        /// Tổng số ảnh chiều dọc
        /// </summary>
        public int VerticalCount { get; set; }

        /// <summary>
        /// Cấp bản đồ
        /// </summary>
        public int Level { get; set; }

        /// <summary>
        /// Loại bản đồ
        /// </summary>
        public string MapType { get; set; }

        /// <summary>
        /// Có hiển thị bản đồ nhỏ ở góc và bản đồ khu vực không
        /// </summary>
        public bool ShowMiniMap { get; set; }

        /// <summary>
        /// Chuyển đối tượng từ XML Node
        /// </summary>
        /// <param name="xmlNode"></param>
        /// <returns></returns>
        public static Map Parse(XElement xmlNode)
        {
            return new Map
            {
                ID = int.Parse(xmlNode.Attribute("ID").Value),
                Code = xmlNode.Attribute("MapCode").Value,
                Name = xmlNode.Attribute("Name").Value,
                MinimapName = xmlNode.Attribute("MinimapName").Value,
                Width = int.Parse(xmlNode.Attribute("Width").Value),
                Height = int.Parse(xmlNode.Attribute("Height").Value),
                MusicBundle = xmlNode.Attribute("MusicBundle").Value,
                ImageFolder = xmlNode.Attribute("ImageFolder").Value,
                PartWidth = int.Parse(xmlNode.Attribute("PartWidth").Value),
                PartHeight = int.Parse(xmlNode.Attribute("PartHeight").Value),
                HorizontalCount = int.Parse(xmlNode.Attribute("HorizontalCount").Value),
                VerticalCount = int.Parse(xmlNode.Attribute("VerticalCount").Value),
                Level = int.Parse(xmlNode.Attribute("MapLevel").Value),
                MapType = xmlNode.Attribute("MapType").Value,
                ShowMiniMap = bool.Parse(xmlNode.Attribute("ShowMiniMap").Value),
            };
        }
    }
}