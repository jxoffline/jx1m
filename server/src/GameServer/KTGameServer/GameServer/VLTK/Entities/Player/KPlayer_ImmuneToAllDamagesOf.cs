using GameServer.Logic;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GameServer.KiemThe.Entities.Player
{
    /// <summary>
    /// Miễn nhiễm toàn bộ sát thương của người chơi
    /// </summary>
    public class KPlayer_ImmuneToAllDamagesOf
    {
        /// <summary>
        /// Đối phương
        /// </summary>
        public KPlayer Player { get; set; }

        /// <summary>
        /// Thời gian duy trì (Milis)
        /// </summary>
        public long Tick { get; set; }

        /// <summary>
        /// Thời gian bắt đầu
        /// </summary>
        public long StartTick { get; set; }

        /// <summary>
        /// Đã kết thúc chưa
        /// </summary>
        public bool IsOver
        {
            get
            {
                return KTGlobal.GetCurrentTimeMilis() - this.StartTick >= this.Tick;
            }
        }
    }
}
