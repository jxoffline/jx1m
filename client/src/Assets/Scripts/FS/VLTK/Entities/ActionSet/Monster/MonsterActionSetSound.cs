using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FS.VLTK.Entities.ActionSet.Monster
{
    /// <summary>
    /// Âm thanh với động tác tương ứng của nhân vật
    /// </summary>
    public class MonsterActionSetSound
    {
        /// <summary>
        /// ID Res
        /// </summary>
        public string ResID { get; set; }

        /// <summary>
        /// Âm thanh động tác đứng tấn công
        /// </summary>
        public string FightStand { get; set; }

        /// <summary>
        /// Âm thanh động tác đứng thường
        /// </summary>
        public string NormalStand { get; set; }

        /// <summary>
        /// Âm thanh động tác chạy
        /// </summary>
        public string Run { get; set; }

        /// <summary>
        /// Âm thanh động tác bị thương
        /// </summary>
        public string Wound { get; set; }

        /// <summary>
        /// Âm thanh động tác chết
        /// </summary>
        public string Die { get; set; }

        /// <summary>
        /// Âm thanh động tác tấn công thường
        /// </summary>
        public string NormalAttack { get; set; }

        /// <summary>
        /// Âm thanh động tác tấn công Crit
        /// </summary>
        public string CritAttack { get; set; }
    }

    public class MonsterActionSetSoundDictionary
    {
        /// <summary>
        /// Đường dẫn file Bundle chứa âm thanh
        /// </summary>
        public string BundleDir { get; set; }

        /// <summary>
        /// Danh sách âm thanh tương ứng
        /// </summary>
        public Dictionary<string, MonsterActionSetSound> ListSounds { get; set; }
    }
}
