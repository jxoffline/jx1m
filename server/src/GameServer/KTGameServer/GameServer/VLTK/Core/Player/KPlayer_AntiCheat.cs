using GameServer.KiemThe.Logic.CheatDetector;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace GameServer.Logic
{
    /// <summary>
    /// Chống Cheat
    /// </summary>
    public partial class KPlayer
    {

        //DEBUG 
        public Stopwatch _DebugTime = Stopwatch.StartNew();

        /// <summary>
        /// Phát hiện Cheat tốc chạy
        /// </summary>
        public SpeedCheatDetector SpeedCheatDetector { get; private set; }

        #region Init
        /// <summary>
        /// Khởi tạo chống Cheat
        /// </summary>
        private void InitCheatDetectors()
        {
            this.SpeedCheatDetector = new SpeedCheatDetector(this);
        }
        #endregion
    }
}
