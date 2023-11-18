using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Server.Data
{
    /// <summary>
    /// Thông tin xếp hạng Tống Kim
    /// </summary>
    [ProtoContract]
    public class SongJinBattleRankingInfo
    {
        /// <summary>
        /// Danh sách xếp hạng người chơi
        /// </summary>
        [ProtoMember(1)]
        public List<SongJinRanking> PlayerRanks { get; set; }

        /// <summary>
        /// Tổng điểm phe Tống
        /// </summary>
        [ProtoMember(2)]
        public string SongTotalScore { get; set; }

        /// <summary>
        /// Tổng điểm phe Kim
        /// </summary>
        [ProtoMember(3)]
        public string JinTotalScore { get; set; }
    }

    /// <summary>
    /// Xếp hạng tống kim
    /// </summary>
    [ProtoContract]
    public class SongJinRanking
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

        /// <summary>
        /// Phe
        /// </summary>
        [ProtoMember(8)]
        public string Camp { get; set; }
    }
}
