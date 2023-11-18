using Server.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FS.VLTK.Entities.Object
{
    /// <summary>
    /// Thông tin Buff
    /// </summary>
    public class BuffData : StaticObjectData
    {
        /// <summary>
        /// Bundle chứa Icon
        /// </summary>
        public string IconBundleDir { get; set; }

        /// <summary>
        /// Atlas chứa Icon
        /// </summary>
        public string IconAtlasName { get; set; }

        /// <summary>
        /// Tên Sprite Icon
        /// </summary>
        public string IconSpriteName { get; set; }

        /// <summary>
        /// Mô tả chi tiết
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Thời gian duy trì (giây)
        /// </summary>
        public long DurationSecond { get; set; }

        /// <summary>
        /// Cấp độ Buff
        /// </summary>
        public int Level { get; set; }

        /// <summary>
        /// Đối tượng Buff tham chiếu
        /// </summary>
        public BufferData RefObject { get; set; }
    }
}
