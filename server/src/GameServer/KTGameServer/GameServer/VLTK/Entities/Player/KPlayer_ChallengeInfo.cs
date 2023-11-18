using GameServer.Logic;
using System.Collections.Generic;

namespace GameServer.KiemThe.Entities.Player
{
    /// <summary>
    /// Thông tin tỷ thí của người chơi tương ứng
    /// </summary>
    public class KPlayer_ChallengeInfo
    {
        /// <summary>
        /// Danh sách nhóm đối phương
        /// </summary>
        public List<KPlayer> OpponentTeamPlayerIDs { get; set; }

        /// <summary>
        /// Đội trưởng nhóm đối phương
        /// </summary>
        public KPlayer OpponentTeamLeader { get; set; }

        /// <summary>
        /// Số tiền cược bản thân
        /// </summary>
        public int MyselfMoney { get; set; }

        /// <summary>
        /// Bản thân đã sẵn sàng chưa
        /// </summary>
        public bool MyselfReady { get; set; } = false;

        /// <summary>
        /// Thứ tự nhóm bản thân
        /// </summary>
        public int MyselfIndex { get; set; }

        /// <summary>
        /// Bản thân đã yêu cầu chiến đấu chưa
        /// </summary>
        public bool MyselfFight { get; set; } = false;
    }
}
