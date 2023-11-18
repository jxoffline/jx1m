using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static FS.VLTK.Entities.Enum;

namespace FS.VLTK.Entities.Object
{
    /// <summary>
    /// Dữ liệu điểm truyền tống
    /// </summary>
    public class TeleportData : StaticObjectData
    {
        /// <summary>
        /// ID Res
        /// </summary>
        public string ResID { get; set; }

        /// <summary>
        /// ID map dịch đến
        /// </summary>
        public int ToMapID { get; set; }

        /// <summary>
        /// Tọa độ điểm dịch đến
        /// </summary>
        public Vector2 ToMapPosition { get; set; }

        /// <summary>
        /// Bán kính quét
        /// </summary>
        public double Radius { get; set; }

        /// <summary>
        /// Loại bản đồ
        /// </summary>
        public MapType ToMapType { get; set; }

        /// <summary>
        /// Cấp độ bản đồ
        /// </summary>
        public int ToMapLevel { get; set; }
    }
}
