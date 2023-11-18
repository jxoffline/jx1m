using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Data
{
    /// <summary>
    ///  Bảng xếp hạng
    /// </summary>
    ///  Bảng xếp hạng
    [ProtoContract]
    public class ELIMINATION_SCOREBOARD
    {
        /// <summary>
        /// Tên người chơi 1
        /// </summary>
        /// 
        [ProtoMember(1)]
        public string Player_1 { get; set; }

        /// <summary>
        /// Tên người chơi 2
        /// </summary>
        /// 
        [ProtoMember(2)]
        public string Player_2 { get; set; }

        /// <summary>
        /// Trạng thái của trận
        /// </summary>
        /// 
        [ProtoMember(3)]
        public ROUNDSTATE _ROUNDSTATE { get; set; }

        /// <summary>
        /// Round này là round mấy
        /// </summary>
        /// 
        [ProtoMember(4)]
        public int ROUNDID { get; set; }

        /// <summary>
        /// Arena nào
        /// </summary>
        /// 
        [ProtoMember(5)]
        public int ARENAID { get; set; }

    }

    public enum ROUNDSTATE
    {
        NONE = 0,
        PREADING = 1,
        START = 2,
        END = 3,
    }

    /// <summary>
    /// Xếp hạng thi đấu môn phái
    /// </summary>
    [ProtoContract]
    public class FACTION_PVP_RANKING
    {
        /// <summary>
        /// Hạng
        /// </summary>
        [ProtoMember(1)]
        public int Rank { get; set; }

        /// <summary>
        /// Tên người chơi
        /// </summary>
        [ProtoMember(2)]
        public string PlayerName { get; set; }

        /// <summary>
        /// Cấp độ
        /// </summary>
        [ProtoMember(3)]
        public int Level { get; set; }

        /// <summary>
        /// Môn phái
        /// </summary>
        [ProtoMember(4)]
        public int Faction { get; set; }

        /// <summary>
        /// Số giết
        /// </summary>
        [ProtoMember(5)]
        public int KillCount { get; set; }

        /// <summary>
        /// Điểm
        /// </summary>
        [ProtoMember(6)]
        public int Score { get; set; }

        /// <summary>
        /// Liên trảm tối đa
        /// </summary>
        [ProtoMember(7)]
        public int MaxKillStreak { get; set; }
    }

    /// <summary>
    /// Xếp hạng người chơi
    /// </summary>
    [ProtoContract]
    public class FACTION_PVP_RANKING_INFO
    {
        /// <summary>
        /// Danh sách xếp hạng người chơi
        /// </summary>
        [ProtoMember(1)]
        public List<FACTION_PVP_RANKING> PlayerRanks { get; set; }

        /// <summary>
        /// Hạng hiện tại của người chơi
        /// </summary>
        [ProtoMember(2)]
        public int PlayRank { get; set; }

        /// <summary>
        /// Danh sách bản thi đấu
        /// </summary>
        [ProtoMember(3)]
        public List<ELIMINATION_SCOREBOARD> ELIMINATION_SCORE { get; set; }

        /// <summary>
        /// Trạng thái SHOW BẢNG XẾP HẠNG
        /// Nếu là 0 thì show PVP RANKING PK TỰ DO
        /// Nếu là 1 thì show RANK BẢNG SOLO 1VS1
        /// </summary>
        [ProtoMember(4)]
        public int State { get; set; }
    }
}
