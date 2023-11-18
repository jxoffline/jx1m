namespace GameDBServer.Data
{
    /// <summary>
    /// Kênh Chat
    /// </summary>
    public enum ChatChannel
    {
        /// <summary>
        /// Không rõ
        /// </summary>
        Default = -1,

        /// <summary>
        /// Hệ thống
        /// </summary>
        System = 0,

        /// <summary>
        /// Hệ thống, hiển thị cả trên dòng chữ chạy ngang
        /// </summary>
        System_Broad_Chat = 1,

        /// <summary>
        /// Bang hội
        /// </summary>
        Guild = 2,

        /// <summary>
        /// Gia tộc
        /// </summary>
        Family = 3,

        /// <summary>
        /// Đội ngũ
        /// </summary>
        Team = 4,

        /// <summary>
        /// Lân cận
        /// </summary>
        Near = 5,

        /// <summary>
        /// Phái
        /// </summary>
        Faction = 6,

        /// <summary>
        /// Mật
        /// </summary>
        Private = 7,

        /// <summary>
        /// Thế giới
        /// </summary>
        Global = 8,

        /// <summary>
        /// Kênh đặc biệt
        /// </summary>
        Special = 9,

        /// <summary>
        /// Toàn bộ
        /// </summary>
        All,
    }

    public class PacketSendToGs
    {
        public int RoleID { get; set; }

        public string roleName { get; set; }

        public int status { get; set; }

        public string toRoleName { get; set; }

        public ChatChannel index { get; set; }

        public string Msg { get; set; }

        public int chatType { get; set; }

        /// <summary>
        /// Nếu là Faction thì extTag1 là FactionID
        /// Nếu là Guild thì extTag1 là GuildID
        /// Nếu là Family thì extTag1 là FamilyID
        /// Nếu là Private thì extTag1 là Private
        /// </summary>
        public int extTag1 { get; set; }

        public int serverLineID { get; set; }
    }
}