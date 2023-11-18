using GameServer.KiemThe.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GameServer.KiemThe.Logic
{
    /// <summary>
    /// Đối tượng sự kiện
    /// </summary>
    public class KTActivity
    {
        /// <summary>
        /// Thông tin sự kiên
        /// </summary>
        public ActivityXML Data { get; set; }

        /// <summary>
        /// Đã bắt đầu chưa
        /// </summary>
        public bool IsStarted { get; set; }
    }
}
