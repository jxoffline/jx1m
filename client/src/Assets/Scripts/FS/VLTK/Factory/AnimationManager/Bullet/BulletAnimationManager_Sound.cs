using FS.VLTK.Entities.Config;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace FS.VLTK.Factory.AnimationManager
{
	/// <summary>
	/// Quản lý hiệu ứng đạn
	/// </summary>
	public partial class BulletAnimationManager
	{
        /// <summary>
        /// Thông tin âm thanh
        /// </summary>
        public class SoundInfo
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

        /// <summary>
        /// Danh sách âm thanh hiệu ứng
        /// </summary>
        public Dictionary<int, SoundInfo> Sounds { get; set; } = new Dictionary<int, SoundInfo>();

        /// <summary>
        /// Tải xuống âm thanh, thêm vào danh sách trong Cache tương ứng
        /// </summary>
        /// <param name="resID"></param>
        /// <returns></returns>
        public IEnumerator LoadSounds(int resID)
        {
            /// Nếu không tồn tại Res của âm thanh tương ứng
            if (!this.Sounds.TryGetValue(resID, out SoundInfo soundInfo))
            {
                yield break;
            }

            BulletActionSetXML actionSetXML = Loader.Loader.BulletActionSetXML;

            /// Âm thanh hiệu ứng bay
            string flySoundName = soundInfo.SoundFly;
            /// Nếu tồn tại âm
            if (!string.IsNullOrEmpty(flySoundName))
            {
                /// Nếu sử dụng phương thức Async
                if (BulletAnimationManager.UseAsyncLoad)
                {
                    yield return KTResourceManager.Instance.LoadAssetAsync<UnityEngine.AudioClip>(actionSetXML.SoundBundleDir, flySoundName, false);
                }
                /// Nếu sử dụng phương thức tuần tự
                else
                {
                    KTResourceManager.Instance.LoadAsset<UnityEngine.AudioClip>(actionSetXML.SoundBundleDir, flySoundName, false);
                }
            }

            /// Âm thanh hiệu ứng tan biến
            string fadeOutSoundName = soundInfo.SoundFadeOut;
            /// Nếu tồn tại âm
            if (!string.IsNullOrEmpty(fadeOutSoundName))
            {
                /// Nếu sử dụng phương thức Async
                if (BulletAnimationManager.UseAsyncLoad)
                {
                    yield return KTResourceManager.Instance.LoadAssetAsync<UnityEngine.AudioClip>(actionSetXML.SoundBundleDir, fadeOutSoundName, false);
                }
                /// Nếu sử dụng phương thức tuần tự
                else
                {
                    KTResourceManager.Instance.LoadAsset<UnityEngine.AudioClip>(actionSetXML.SoundBundleDir, fadeOutSoundName, false);
                }
            }
        }

        /// <summary>
        /// Tải xuống âm thanh, thêm vào danh sách trong Cache tương ứng
        /// </summary>
        /// <param name="resID"></param>
        /// <returns></returns>
        public IEnumerator LoadExplodeEffectSounds(int resID)
        {
            /// Nếu không tồn tại Res của âm thanh tương ứng
            if (!this.Sounds.TryGetValue(resID, out SoundInfo soundInfo))
            {
                yield break;
            }

            BulletActionSetXML actionSetXML = Loader.Loader.BulletActionSetXML;

            /// Âm thanh hiệu ứng bay
            string soundName = soundInfo.SoundExplode;
            /// Nếu tồn tại âm
            if (!string.IsNullOrEmpty(soundName))
            {
                /// Nếu sử dụng phương thức Async
                if (BulletAnimationManager.UseAsyncLoad)
                {
                    yield return KTResourceManager.Instance.LoadAssetAsync<UnityEngine.AudioClip>(actionSetXML.SoundBundleDir, soundName, false);
                }
                /// Nếu sử dụng phương thức tuần tự
                else
                {
                    KTResourceManager.Instance.LoadAsset<UnityEngine.AudioClip>(actionSetXML.SoundBundleDir, soundName, false);
                }
            }
        }
    }
}
