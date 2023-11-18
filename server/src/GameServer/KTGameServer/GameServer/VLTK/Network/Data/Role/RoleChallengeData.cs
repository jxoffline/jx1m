using System.Collections.Generic;
using ProtoBuf;

namespace Server.Data
{
    /// <summary>
    /// Thông tin người chơi trong dữ liệu thách đấu
    /// </summary>
    [ProtoContract]
    public class RoleChallenge_PlayerData
    {
        /// <summary>
        /// ID
        /// </summary>
        [ProtoMember(1)]
        public int RoleID { get; set; }

        /// <summary>
        /// Tên nhân vật
        /// </summary>
        [ProtoMember(2)]
        public string RoleName { get; set; }

        /// <summary>
        /// Cấp độ
        /// </summary>
        [ProtoMember(3)]
        public int Level { get; set; }

        /// <summary>
        /// ID phái
        /// </summary>
        [ProtoMember(4)]
        public int FactionID { get; set; }

        /// <summary>
        /// ID mũ
        /// </summary>
        [ProtoMember(5)]
        public int HelmID { get; set; }

        /// <summary>
        /// ID áo
        /// </summary>
        [ProtoMember(6)]
        public int ArmorID { get; set; }

        /// <summary>
        /// ID vũ khí
        /// </summary>
        [ProtoMember(7)]
        public int WeaponID { get; set; }

        /// <summary>
        /// Ngũ hành vũ khí
        /// </summary>
        [ProtoMember(8)]
        public int WeaponSeries { get; set; }

        /// <summary>
        /// Cấp cường hóa vũ khí
        /// </summary>
        [ProtoMember(9)]
        public int WeaponEnhanceLevel { get; set; }

        /// <summary>
        /// Có phải trưởng nhóm không
        /// </summary>
        [ProtoMember(10)]
        public bool IsTeamLeader { get; set; }

        /// <summary>
        /// Giới tính
        /// </summary>
        [ProtoMember(11)]
        public int RoleSex { get; set; }
    }

    /// <summary>
    /// Dữ liệu thách đấu
    /// </summary>
    [ProtoContract]
    public class RoleChallengeData
    {
        /// <summary>
        /// Danh sách người chơi các nhóm
        /// </summary>
        [ProtoMember(1)]
        public Dictionary<int, List<RoleChallenge_PlayerData>> TeamPlayers { get; set; }

        /// <summary>
        /// Danh sách tiền cược các nhóm
        /// </summary>
        [ProtoMember(2)]
        public Dictionary<int, int> TeamMoneys { get; set; }

        /// <summary>
        /// Trạng thái xác nhận của nhóm
        /// </summary>
        [ProtoMember(3)]
        public Dictionary<int, bool> TeamReadyStates { get; set; }
    }
}
