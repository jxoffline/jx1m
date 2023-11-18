using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static FS.VLTK.Entities.Enum;

namespace FS.VLTK.Entities.Object
{
    /// <summary>
    /// Thông tin ngũ hành
    /// </summary>
    public class ElementData
    {
        /// <summary>
        /// Loại ngũ hành
        /// </summary>
        public Elemental ElementType { get; set; }

        /// <summary>
        /// Tên ngũ hành
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Sprite ảnh nhỏ
        /// </summary>
        public UnityEngine.Sprite SmallSprite { get; set; }

        /// <summary>
        /// Sprite ảnh thường
        /// </summary>
        public UnityEngine.Sprite NormalSprite { get; set; }

        /// <summary>
        /// Sprite ảnh lớn
        /// </summary>
        public UnityEngine.Sprite BigSprite { get; set; }
    }
}
