using FS.VLTK.Control.Component;
using FS.VLTK.Factory;
using FS.VLTK.Factory.AnimationManager;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FS.VLTK.Utilities.UnityComponent
{
    /// <summary>
    /// Đối tượng quản lý hiệu ứng nổ của đạn
    /// </summary>
    public class BulletExplodeEffectAnimation2D : TTMonoBehaviour
    {
        #region Private properties
        /// <summary>
        /// Luồng thực thi play hiệu ứng
        /// </summary>
        private Coroutine playerCoroutine;

        /// <summary>
        /// ID Res lần trước
        /// </summary>
        private int LastResID = -1;
        #endregion

        #region Define
        #region Reference objects
        /// <summary>
        /// Thân đối tượng
        /// </summary>
        public SpriteRenderer Body { get; set; }
        #endregion

        private int _ResID;
        /// <summary>
        /// Dữ liệu đạn
        /// </summary>
        public int ResID
        {
            get
            {
                return this._ResID;
            }
            set
            {
                this._ResID = value;

                this.OnDataChanged();
            }
        }

        /// <summary>
        /// Dữ liệu sắp xếp thêm vào
        /// </summary>
        public int AdditionSortingOrder { get; set; }

        /// <summary>
        /// Đang chạy không
        /// </summary>
        public bool IsPlaying { get; private set; }

        /// <summary>
        /// Đang tạm dừng không
        /// </summary>
        public bool IsPausing { get; private set; } = false;

        /// <summary>
        /// Frame đang chạy hiện tại
        /// </summary>
        public int CurrentFrameID { get; private set; }

        /// <summary>
        /// Sự kiện khi bắt đầu thực hiện động tác
        /// </summary>
        public Action OnStart { get; set; }

        /// <summary>
        /// Sự kiện khi tạm dừng động tác
        /// </summary>
        public Action OnPause { get; set; }

        /// <summary>
        /// Sự kiện khi tiếp tục động tác
        /// </summary>
        public Action OnResume { get; set; }

        /// <summary>
        /// Sự kiện khi đổi frame
        /// </summary>
        public Action OnFrameChanging { get; set; }

        /// <summary>
        /// Sự kiện khi một vòng hiệu ứng được thực hiện
        /// </summary>
        public Action OnCycleCompleted { get; set; }
        #endregion

        #region Private methods
        /// <summary>
        /// Hàm này gọi đến khi dữ liệu thay đổi
        /// </summary>
        private void OnDataChanged()
        {

        }

        #region Loader
        /// <summary>
        /// Tải xuống hiệu ứng đạn theo phương thức Async
        /// </summary>
        /// <returns></returns>
        private IEnumerator LoadAsync()
        {
            yield return BulletAnimationManager.Instance.LoadExplodeEffectSprites(this._ResID);
        }

        /// <summary>
        /// Tải xuống âm thanh đạn theo phương thức Async
        /// </summary>
        /// <returns></returns>
        private IEnumerator LoadSoundAsync()
        {
            yield return BulletAnimationManager.Instance.LoadExplodeEffectSounds(this._ResID);
        }

        /// <summary>
        /// Hủy tải xuống hiệu ứng đạn
        /// </summary>
        private void Unload()
        {
            BulletAnimationManager.Instance.UnloadExplodeEffectSprites(this.LastResID);
        }
        #endregion

        /// <summary>
        /// Thực hiện động tác
        /// </summary>
        /// <returns></returns>
        private IEnumerator DoPlay(float duration, bool isRepeat, float repeatAfter, bool isContinueFromCurrentFrame, Action Callback)
        {
            #region Init
            /// Danh sách Sprite
            List<Sprite> actionSet = null;

            /// Danh sách Frame
            Dictionary<string, UnityEngine.Object> explodeActionSetFull = BulletAnimationManager.Instance.GetExplodeSprites(this._ResID);
            if (explodeActionSetFull != null)
            {
                actionSet = new List<Sprite>();

                foreach (KeyValuePair<string, UnityEngine.Object> pair in explodeActionSetFull)
                {
                    actionSet.Add(pair.Value as Sprite);
                }
            }

            /// Nếu không tồn tại
            if (actionSet == null)
            {
                Callback?.Invoke();
                yield break;
            }
            #endregion


            if (!isContinueFromCurrentFrame)
            {
                this.CurrentFrameID = -1;
            }

            this.IsPlaying = true;
            this.IsPausing = false;
            this.OnStart?.Invoke();

            float dTime = duration / actionSet.Count;

            WaitForSeconds waitDTime = new WaitForSeconds(dTime);

            while (true)
            {
                if (this.IsPausing)
                {
                    yield return null;
                    continue;
                }

                this.CurrentFrameID++;
                if (this.CurrentFrameID >= actionSet.Count)
                {
                    if (isRepeat)
                    {
                        this.CurrentFrameID = -1;
                        this.OnCycleCompleted?.Invoke();
                        if (repeatAfter > 0)
                        {
                            yield return new WaitForSeconds(repeatAfter);
                        }
                        yield return null;
                        continue;
                    }
                    else
                    {
                        this.OnCycleCompleted?.Invoke();
                        this.IsPlaying = false;
                        break;
                    }
                }

                if (this.CurrentFrameID == 0)
                {
                    #region Sound
                    if (BulletAnimationManager.Instance.Sounds.ContainsKey(this._ResID))
                    {
                        string soundName = BulletAnimationManager.Instance.Sounds[this._ResID].SoundExplode;
                        if (!string.IsNullOrEmpty(soundName))
                        {
                            AudioClip sound = KTResourceManager.Instance.GetAsset<UnityEngine.AudioClip>(Loader.Loader.BulletActionSetXML.SoundBundleDir, soundName);
                            SkillManager.CreateStandaloneAudioPlayer(soundName, sound);
                        }
                    }
                    #endregion
                }

                #region Play actions
                if (actionSet != null && this.CurrentFrameID < actionSet.Count && actionSet[this.CurrentFrameID] != null)
                {
                    this.Body.sprite = actionSet[this.CurrentFrameID];
                    this.Body.drawMode = SpriteDrawMode.Sliced;
                    this.Body.size = actionSet[this.CurrentFrameID].rect.size;
                    this.Body.gameObject.transform.localPosition = new Vector3(0, 0, -(this.AdditionSortingOrder / 10000f + 9 / 1000000f));
                }
                else
                {
                    this.Body.sprite = null;
                }
                #endregion

                this.OnFrameChanging?.Invoke();
                yield return waitDTime;
            }
            Callback?.Invoke();
        }
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi đến khi đối tượng bị ẩn
        /// </summary>
        private void OnDisable()
        {
            this.StopAllCoroutines();
            if (this.LastResID != -1)
			{
                this.Unload();
			}
            this.LastResID = -1;

            this.playerCoroutine = null;
            this.Body.sprite = null;
            this.ResID = -1;
            this.AdditionSortingOrder = 0;
            this.IsPlaying = false;
            this.IsPausing = false;
            this.CurrentFrameID = 0;
            this.OnStart = null;
            this.OnPause = null;
            this.OnResume = null;
            this.OnFrameChanging = null;
            this.OnCycleCompleted = null;
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Tạm dừng thực hiện động tác
        /// </summary>
        public void Pause()
        {
            this.IsPausing = true;
            this.OnPause?.Invoke();
        }

        /// <summary>
        /// Tiếp tục thực hiện động tác
        /// </summary>
        public void Resume()
        {
            this.IsPausing = false;
            this.OnResume?.Invoke();
        }

        /// <summary>
        /// Dừng thực hiện động tác
        /// </summary>
        public void Stop()
        {
            if (this.playerCoroutine != null)
            {
                this.StopCoroutine(this.playerCoroutine);
                this.playerCoroutine = null;
            }
        }

        /// <summary>
        /// Phương thức Async thực hiện động tác
        /// </summary>
        /// <param name="duration"></param>
        /// <param name="isRepeat"></param>
        /// <param name="repeatAfter"></param>
        /// <param name="isContinueFromCurrentFrame"></param>
        /// <param name="Callback"></param>
        /// <returns></returns>
        public IEnumerator DoActionAsync(float duration, bool isRepeat = false, float repeatAfter = 0, bool isContinueFromCurrentFrame = false, Action Callback = null)
        {
            if (!this.gameObject.activeSelf)
            {
                yield break;
            }

            /// Hủy sprite cũ
            if (this.LastResID != -1)
			{
                this.Unload();
			}
            this.LastResID = this.ResID;

            #region Load sprites
            int totalLoaded = 0;
            IEnumerator DoLoad()
            {
                yield return this.LoadAsync();
                totalLoaded++;
            }
            IEnumerator DoLoadSound()
            {
                yield return this.LoadSoundAsync();
                totalLoaded++;
            }
            KTResourceManager.Instance.StartCoroutine(DoLoad());
            KTResourceManager.Instance.StartCoroutine(DoLoadSound());

            while (totalLoaded < 2)
            {
                yield return null;
            }
            #endregion

            if (!this.gameObject || !this.gameObject.activeSelf)
            {
                yield break;
            }

            this.Stop();
            this.playerCoroutine = this.StartCoroutine(this.DoPlay(duration, isRepeat, repeatAfter, isContinueFromCurrentFrame, Callback));
        }
        #endregion
    }
}
