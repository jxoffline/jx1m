using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace FS.VLTK.Entities.ActionSet.Bullet
{
    /// <summary>
    /// Âm thanh tương ứng hiệu ứng của đạn
    /// </summary>
    public class BulletActionSetSound
    {
        /// <summary>
        /// ID đạn
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// Tên âm thanh đạn bay
        /// </summary>
        public string SoundFly { get; set; }

        /// <summary>
        /// Tên âm thanh đạn rơi trên trời xuống
        /// </summary>
        public string SoundFadeOut { get; set; }

        /// <summary>
        /// Tên âm thanh đạn nổ
        /// </summary>
        public string SoundExplode { get; set; }
    }
}
