using System;
using System.Collections.Generic;
using ProtoBuf;

namespace Server.Data
{
    /// <summary>
    /// Định nghĩa thông tin chiến đội
    /// </summary>
    [ProtoContract]
    public class TeamBattleInfo
    {
        /// <summary>
        /// ID
        /// </summary>
        [ProtoMember(1)]
        public int ID { get; set; }

        /// <summary>
        /// Tên chiến đội
        /// </summary>
        [ProtoMember(2)]
        public string Name { get; set; }

        /// <summary>
        /// Danh sách thành viên tương ứng
        /// </summary>
        [ProtoMember(3)]
        public Dictionary<int, string> Members { get; set; }

        /// <summary>
        /// Thời gian đăng ký
        /// </summary>
        [ProtoMember(4)]
        public DateTime RegisterTime { get; set; }

        /// <summary>
        /// Số điểm
        /// </summary>
        [ProtoMember(5)]
        public int Point { get; set; }

        /// <summary>
        /// Tổng số trận thi đấu vòng tròn đã tham gia
        /// </summary>
        [ProtoMember(6)]
        public int TotalBattles { get; set; }

        /// <summary>
        /// Hạng chiến đấu của chiến đội
        /// </summary>
        [ProtoMember(7)]
        public int Stage { get; set; }

        /// <summary>
        /// Xếp hạng chiến đội tính đến hôm nay
        /// </summary>
        [ProtoMember(8)]
        public int Rank { get; set; }

        /// <summary>
        /// Có phần thưởng để nhận không
        /// </summary>
        [ProtoMember(9)]
        public bool HasAwards { get; set; }

        /// <summary>
        /// Thời điểm cập nhật xếp hạng chiến đội
        /// </summary>
        [ProtoMember(10)]
        public DateTime LastUpdateRankTime { get; set; } = DateTime.MinValue;

        /// <summary>
        /// Thời điểm thắng cuộc lần trước
        /// </summary>
        [ProtoMember(11)]
        public DateTime LastWinTime { get; set; } = DateTime.MinValue;

        /// <summary>
        /// Tạo ra bản sao của đối tượng
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public TeamBattleInfo Clone()
        {
            return new TeamBattleInfo()
            {
                ID = this.ID,
                Name = this.Name,
                Members = this.Members,
                RegisterTime = this.RegisterTime,
                Point = this.Point,
                TotalBattles = this.TotalBattles,
                Stage = this.Stage,
                Rank = this.Rank,
                HasAwards = this.HasAwards,
                LastUpdateRankTime = this.LastUpdateRankTime,
                LastWinTime = this.LastWinTime,
            };
        }
    }
}
