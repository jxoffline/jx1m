using FS.GameEngine.Logic;
using FS.VLTK.Factory;
using FS.VLTK.Utilities.UnityComponent;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FS.VLTK.Control
{
    /// <summary>
    /// Quản lý LoginScene
    /// </summary>
    public class LoginSceneManager : TTMonoBehaviour
    {
        #region Define
        /// <summary>
        /// Đối tượng AudioPlayer
        /// </summary>
        [SerializeField]
        private AudioPlayer AudioPlayer;

        /// <summary>
        /// Đường dẫn Bundle chứa nhạc
        /// </summary>
        [SerializeField]
        private string MusicBundleDir;

        /// <summary>
        /// Tên nhạc
        /// </summary>
        [SerializeField]
        public string MusicName;
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi ở Frame đầu tiên
        /// </summary>
        private void Start()
        {
            this.StartCoroutine(this.LoadMusicAndPlay());
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Tải nhạc và phát
        /// </summary>
        /// <returns></returns>
        private IEnumerator LoadMusicAndPlay()
        {
            yield return KTResourceManager.Instance.LoadAssetBundleAsync(string.Format("{0}/{1}", Consts.RESOURCES_DIR, this.MusicBundleDir), false, KTResourceManager.KTResourceCacheType.CachedUntilChangeScene);
            yield return KTResourceManager.Instance.LoadAssetAsync<AudioClip>(string.Format("{0}/{1}", Consts.RESOURCES_DIR, this.MusicBundleDir), this.MusicName, true, null, KTResourceManager.KTResourceCacheType.CachedUntilChangeScene);

            /// AudioClip tương ứng
            AudioClip music = KTResourceManager.Instance.GetAsset<AudioClip>(string.Format("{0}/{1}", Consts.RESOURCES_DIR, this.MusicBundleDir), this.MusicName);
            /// Thực hiện phát
            this.AudioPlayer.ActivateAfter = 0f;
            this.AudioPlayer.IsRepeat = true;
            this.AudioPlayer.RepeatTimer = 0f;
            this.AudioPlayer.Sound = music;
            this.AudioPlayer.Volume = 1f;
            this.AudioPlayer.Play();
        }
        #endregion
    }
}
