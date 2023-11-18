using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using static FS.VLTK.Entities.Enum;

namespace FS.VLTK.Factory.Animation
{
    /// <summary>
    /// Quản lý âm thanh
    /// </summary>
    public partial class CharacterAnimationManager
    {
        /// <summary>
        /// Thông tin âm thanh
        /// </summary>
        public class SoundByAction
        {
            /// <summary>
            /// Tên động tác
            /// </summary>
            public string ActionName { get; set; }

            /// <summary>
            /// Âm thanh
            /// </summary>
            public string SoundName { get; set; }
        }

        /// <summary>
        /// Danh sách âm thanh nam
        /// </summary>
        public Dictionary<string, SoundByAction> MaleSounds { get; set; } = new Dictionary<string, SoundByAction>();

        /// <summary>
        /// Danh sách âm thanh nữ
        /// </summary>
        public Dictionary<string, SoundByAction> FemaleSounds { get; set; } = new Dictionary<string, SoundByAction>();

        /// <summary>
        /// Trả về tên âm thanh tương ứng
        /// </summary>
        /// <param name="actionType"></param>
        /// <param name="weaponID"></param>
        /// <param name="sex"></param>
        /// <param name="isRiding"></param>
        /// <returns></returns>
        public string GetSoundName(PlayerActionType actionType, string weaponID, Sex sex, bool isRiding)
        {
            /// Tên động tác
            string actionName = this.GetActionName(actionType, weaponID, isRiding);
            /// Nếu là nam
            if (sex == Sex.MALE && this.MaleSounds.TryGetValue(actionName, out SoundByAction maleSound))
            {
                return maleSound.SoundName;
            }
            /// Nếu là nữ
            else if (sex == Sex.FEMALE && this.FemaleSounds.TryGetValue(actionName, out SoundByAction femaleSound))
            {
                return femaleSound.SoundName;
            }
            /// Không có gì
            return "";
        }

        /// <summary>
        /// Tải xuống âm thanh, thêm vào danh sách trong Cache tương ứng
        /// </summary>
        /// <param name="resID"></param>
        /// <param name="actionType"></param>
        /// <returns></returns>
        public IEnumerator LoadSounds(PlayerActionType actionType, string weaponID, Sex sex, bool isRiding)
        {
            /// Tên âm thanh
            string soundName = this.GetSoundName(actionType, weaponID, sex, isRiding);

            /// Nếu tồn tại âm
            if (!string.IsNullOrEmpty(soundName))
            {
                /// Nếu sử dụng phương thức Async
                if (CharacterAnimationManager.UseAsyncLoad)
                {
                    yield return KTResourceManager.Instance.LoadAssetAsync<UnityEngine.AudioClip>(Loader.Loader.CharacterActionSetSoundBundleDir, soundName, false);
                }
                /// Nếu sử dụng phương thức tải tuần tự
                else
                {
                    KTResourceManager.Instance.LoadAsset<UnityEngine.AudioClip>(Loader.Loader.CharacterActionSetSoundBundleDir, soundName, false);
                }
            }
        }
    }
}
