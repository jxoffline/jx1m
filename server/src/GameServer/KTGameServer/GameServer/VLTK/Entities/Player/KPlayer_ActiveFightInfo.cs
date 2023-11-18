using GameServer.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.KiemThe.Entities.Player
{
    /// <summary>
    /// Thông tin tuyên chiến người chơi khác
    /// </summary>
    public class KPlayer_ActiveFightInfo
    {
        /// <summary>
        /// Đối tượng tuyên chiến
        /// </summary>
        public KPlayer Target { get; set; }

        /// <summary>
        /// Đối tượng chủ động tuyên chiến
        /// </summary>
        public KPlayer Starter { get; set; }

        /// <summary>
        /// Thời gian lần cuối tấn công đối tượng
        /// </summary>
        public long LastAttackTick { get; set; }
    }
}
