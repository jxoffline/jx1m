using FS.GameEngine.Logic;
using FS.GameEngine.Sprite;

namespace FS.VLTK
{
    /// <summary>
    /// Các hàm toàn cục dùng trong Kiếm Thế
    /// </summary>
    public static partial class KTGlobal
    {
        #region Boss
        /// <summary>
        /// Hiệu ứng dưới chân quái tinh anh
        /// </summary>
        public const int MonsterEliteStateEffectID = 72;

        /// <summary>
        /// Hiệu ứng dưới chân quái thủ lĩnh
        /// </summary>
        public const int MonsterLeaderStateEffectID = 73;

        /// <summary>
        /// Hiệu ứng dưới chân Boss
        /// </summary>
        public const int MonsterBossStateEffectID = 74;

        /// <summary>
        /// Thực thi hiệu ứng đặc biệt của quái
        /// </summary>
        /// <param name="sprite"></param>
        /// <param name="monsterType"></param>
        public static void PlayMonsterSpecialEffect(GSprite sprite, int monsterType)
        {
            switch (monsterType)
            {
                case (int) MonsterTypes.Elite:
                {
                    sprite.AddBuff(KTGlobal.MonsterEliteStateEffectID, -1);
                    break;
                }
                case (int) MonsterTypes.Leader:
                case (int) MonsterTypes.Pirate:
                {
                    sprite.AddBuff(KTGlobal.MonsterLeaderStateEffectID, -1);
                    break;
                }
                case (int) MonsterTypes.Boss:
                case (int) MonsterTypes.Special_Boss:
                {
                    sprite.AddBuff(KTGlobal.MonsterBossStateEffectID, -1);
                    break;
                }
            }
        }
        #endregion
    }
}
