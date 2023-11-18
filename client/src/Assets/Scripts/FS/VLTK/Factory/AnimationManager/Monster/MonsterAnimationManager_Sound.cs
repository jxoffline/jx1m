using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static FS.VLTK.Entities.Enum;

namespace FS.VLTK.Factory.Animation
{
    /// <summary>
    /// Quản lý âm thanh quái
    /// </summary>
    public partial class MonsterAnimationManager
    {
        /// <summary>
        /// Âm thanh theo động tác
        /// </summary>
        public class SoundByRes
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

        /// <summary>
        /// Danh sách âm thanh quái
        /// </summary>
        public Dictionary<string, SoundByRes> Sounds { get; set; } = new Dictionary<string, SoundByRes>();

        /// <summary>
        /// Trả về âm thanh động tác quái theo loại
        /// </summary>
        /// <param name="resID"></param>
        /// <param name="actionType"></param>
        /// <returns></returns>
        public string GetSoundNameByActionType(string resID, MonsterActionType actionType)
        {
            if (string.IsNullOrEmpty(resID))
            {
                return "";
            }

            if (!this.Sounds.TryGetValue(resID, out SoundByRes _))
            {
                return "";
            }

            string soundName = "";
            switch (actionType)
            {
                case MonsterActionType.FightStand:
                    soundName = this.Sounds[resID].FightStand;
                    break;
                case MonsterActionType.NormalStand:
                    soundName = this.Sounds[resID].NormalStand;
                    break;
                case MonsterActionType.Run:
                    soundName = this.Sounds[resID].Run;
                    break;
                case MonsterActionType.RunAttack:
                    soundName = this.Sounds[resID].Run;
                    break;
                case MonsterActionType.Wound:
                    soundName = this.Sounds[resID].Wound;
                    break;
                case MonsterActionType.NormalAttack:
                    soundName = this.Sounds[resID].NormalAttack;
                    break;
                case MonsterActionType.CritAttack:
                    soundName = this.Sounds[resID].CritAttack;
                    break;
                case MonsterActionType.Die:
                    soundName = this.Sounds[resID].Die;
                    break;
            }
            return soundName;
        }

        /// <summary>
        /// Tải xuống âm thanh, thêm vào danh sách trong Cache tương ứng
        /// </summary>
        /// <param name="resID"></param>
        /// <param name="actionType"></param>
        /// <returns></returns>
        public IEnumerator LoadSounds(string resID, MonsterActionType actionType)
        {
            if (string.IsNullOrEmpty(resID))
            {
                yield break;
            }

            /// Tên âm thanh
            string soundName = this.GetSoundNameByActionType(resID, actionType);
            /// Nếu tồn tại âm
            if (!string.IsNullOrEmpty(soundName))
            {
                /// Nếu sử dụng phương thức Async
                if (MonsterAnimationManager.UseAsyncLoad)
                {
                    yield return KTResourceManager.Instance.LoadAssetAsync<UnityEngine.AudioClip>(Loader.Loader.MonsterActionSetSoundBundleDir, soundName, false);
                }
                /// Nếu sử dụng phương thức tuần tự
                else
                {
                    KTResourceManager.Instance.LoadAsset<UnityEngine.AudioClip>(Loader.Loader.MonsterActionSetSoundBundleDir, soundName, false);
                }
            }
        }
    }
}
