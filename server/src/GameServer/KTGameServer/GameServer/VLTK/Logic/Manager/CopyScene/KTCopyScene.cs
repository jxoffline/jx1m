using GameServer.Interface;
using GameServer.KiemThe.Core;
using GameServer.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using static GameServer.Logic.KTMapManager;

namespace GameServer.KiemThe.Logic
{
    /// <summary>
    /// Định nghĩa phụ bản
    /// </summary>
    public class KTCopyScene
    {
        /// <summary>
        /// ID tự tăng
        /// </summary>
        private static int AutoID = 0;

        /// <summary>
        /// ID phụ bản
        /// </summary>
        public int ID { get; private set; }

        /// <summary>
        /// Cấp độ phụ bản
        /// </summary>
        public int Level { get; set; }

        /// <summary>
        /// ID bản đồ tham chiếu
        /// </summary>
        public int MapCode { get; private set; }

        /// <summary>
        /// Tên phụ bản
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Vị trí X ở trong phụ bản
        /// </summary>
        public int EnterPosX { get; set; }

        /// <summary>
        /// Vị trí Y ở trong phụ bản
        /// </summary>
        public int EnterPosY { get; set; }

        /// <summary>
        /// ID bản đồ đích sau khi rời khỏi phụ bản
        /// </summary>
        public int OutMapCode { get; set; } = -1;

        /// <summary>
        /// Vị trí X trên bản đồ sau khi rời phụ bản
        /// </summary>
        public int OutPosX { get; set; }

        /// <summary>
        /// Vị trí Y trên bản đồ sau khi rời phụ bản
        /// </summary>
        public int OutPosY { get; set; }

        /// <summary>
        /// ID bản đồ sống lại
        /// <para>Nếu -1 thì nghĩa là sống ngay tại phụ bản</para>
        /// </summary>
        public int ReliveMapCode { get; set; }
        /// <summary>
        /// Tọa độ X vị trí hồi sinh sau khi chết
        /// </summary>
        public int RelivePosX { get; set; }
        /// <summary>
        /// Tọa độ Y vị trí hồi sinh sau khi chết
        /// </summary>
        public int RelivePosY { get; set; }
        /// <summary>
        /// Sau khi chết hồi sinh sẽ phục hồi % sinh lực
        /// </summary>
        public int ReliveHPPercent { get; set; }
        /// <summary>
        /// Sau khi chết hồi sinh sẽ phục hồi % nội lực
        /// </summary>
        public int ReliveMPPercent { get; set; }
        /// <summary>
        /// Sau khi chết hồi sinh sẽ phục hồi % thể lực
        /// </summary>
        public int ReliveStaminaPercent { get; set; }

        /// <summary>
        /// Thời điểm tạo ra (giây)
        /// </summary>
        public int InitTicks { get; private set; }

        /// <summary>
        /// Thời gian phụ bản
        /// </summary>
        public long DurationTicks { get; private set; }

        /// <summary>
        /// Chấp nhận người chơi kết nối lại không
        /// </summary>
        public bool AllowReconnect { get; set; }

        /// <summary>
        /// Phụ bản
        /// </summary>
        /// <param name="map"></param>
        /// <param name="durationTicks"></param>
        public KTCopyScene(GameMap map, long durationTicks)
		{
            /// Tăng ID tự động
            KTCopyScene.AutoID = (KTCopyScene.AutoID + 1) % 1000000007;
            this.ID = KTCopyScene.AutoID;
            this.MapCode = map.MapCode;
            this.Name = map.MapName;
            this.DurationTicks = durationTicks;
            this.InitTicks = KTGlobal.GetOffsetSecond(DateTime.Now);
        }
    }
}
