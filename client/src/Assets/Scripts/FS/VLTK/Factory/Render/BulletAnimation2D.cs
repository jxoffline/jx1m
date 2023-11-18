using FS.VLTK.Control.Component;
using FS.VLTK.Entities.ActionSet;
using FS.VLTK.Entities.ActionSet.Character;
using FS.VLTK.Entities.Config;
using FS.VLTK.Factory;
using FS.VLTK.Factory.AnimationManager;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static FS.VLTK.Entities.Enum;

namespace FS.VLTK.Utilities.UnityComponent
{
	/// <summary>
	/// Đối tượng quản lý Animation 2D của đạn bay
	/// </summary>
	public class BulletAnimation2D : TTMonoBehaviour
    {
        #region Private properties
        /// <summary>
        /// ID lần trước
        /// </summary>
        private int LastResID = -1;

        /// <summary>
        /// Luồng thực thi play hiệu ứng
        /// </summary>
        private Coroutine playerCoroutine;
        #endregion

        #region Define
        #region Reference objects
        /// <summary>
        /// Thân đối tượng
        /// </summary>
        public SpriteRenderer Body { get; set; }
        #endregion

        /// <summary>
        /// Hướng quay 16 hướng
        /// </summary>
        private Direction16 _Direction;

        /// <summary>
        /// Hướng quay 32 hướng
        /// </summary>
        private Direction32 _Direction32;

        /// <summary>
        /// Hướng quay 8 hướng
        /// </summary>
        private Direction _Direction8;

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
        private IEnumerator LoadFlyAsync()
        {
            yield return BulletAnimationManager.Instance.LoadFlySprites(this._ResID);
        }

        /// <summary>
        /// Tải xuống hiệu ứng đạn theo phương thức Async
        /// </summary>
        /// <returns></returns>
        private IEnumerator LoadFadeOutAsync()
        {
            yield return BulletAnimationManager.Instance.LoadFadeOutSprites(this._ResID);
        }

        /// <summary>
        /// Tải xuống âm thanh đạn theo phương thức Async
        /// </summary>
        /// <returns></returns>
        private IEnumerator LoadSoundAsync()
        {
            yield return BulletAnimationManager.Instance.LoadSounds(this._ResID);
        }

        /// <summary>
        /// Hủy tải xuống hiệu ứng đạn
        /// </summary>
        private void UnloadFly()
        {
            if (this.LastResID == -1)
			{
                return;
			}
            BulletAnimationManager.Instance.UnloadFlySprites(this.LastResID);
        }

        /// <summary>
        /// Hủy tải xuống hiệu ứng đạn
        /// </summary>
        private void UnloadFadeOut()
        {
            if (this.LastResID == -1)
			{
                return;
			}
            BulletAnimationManager.Instance.UnloadFadeOutSprites(this.LastResID);
        }
        #endregion

        /// <summary>
        /// Thực hiện động tác bay
        /// </summary>
        /// <returns></returns>
        private IEnumerator DoFlyAnimation(float duration, bool isRepeat, float repeatAfter, bool isContinueFromCurrentFrame, Action Callback)
        {
            /// Sử dụng 32 hướng
            bool use32Dir = (int) this._Direction32 >= (int) Direction32.Down && (int) this._Direction32 < (int) Direction32.Count;
            /// Sử dụng 16 hướng
            bool use16Dir = (int) this._Direction >= (int) Direction16.Down && (int) this._Direction < (int) Direction16.Count;
            /// Sử dụng 8 hướng
            bool use8Dir = (int) this._Direction8 >= (int) Direction.DOWN && (int) this._Direction8 < (int) Direction.Count;

            #region Init
            /// Danh sách Sprite
            List<Sprite> actionSet = null;

            /// Danh sách Frame
            Dictionary<string, UnityEngine.Object> flyActionSetFull = BulletAnimationManager.Instance.GetFlySprites(this._ResID);
            if (flyActionSetFull != null)
			{
                actionSet = new List<Sprite>();

                /// Nếu sử dụng 32 hướng
                if (use32Dir)
                {
                    /// Tổng số Frame mỗi hướng
                    int framesEachDir = flyActionSetFull.Count / 32;
                    int fromID = ((int) this._Direction32) * framesEachDir;
                    /// Thêm Frame vào
                    actionSet.AddRange(flyActionSetFull.Values.Skip(fromID).Take(framesEachDir).Select(x => x as Sprite));
                }
                /// Nếu sử dụng 16 hướng
                else if (use16Dir)
                {
                    /// Tổng số Frame mỗi hướng
                    int framesEachDir = flyActionSetFull.Count / 16;
                    int fromID = ((int) this._Direction) * framesEachDir;
                    /// Thêm Frame vào
                    actionSet.AddRange(flyActionSetFull.Values.Skip(fromID).Take(framesEachDir).Select(x => x as Sprite));
                }
                /// Nếu sử dụng 8 hướng
                else if (use8Dir)
                {
                    /// Tổng số Frame mỗi hướng
                    int framesEachDir = flyActionSetFull.Count / 8;
                    int fromID = ((int) this._Direction8) * framesEachDir;
                    /// Thêm Frame vào
                    actionSet.AddRange(flyActionSetFull.Values.Skip(fromID).Take(framesEachDir).Select(x => x as Sprite));
                }
                /// Nếu không phải
                else
                {
                    /// Thêm toàn bộ Frame vào
                    actionSet.AddRange(flyActionSetFull.Values.Select(x => x as Sprite));
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
                        string soundName = BulletAnimationManager.Instance.Sounds[this._ResID].SoundFly;
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

        /// <summary>
        /// Thực hiện động tác tàn
        /// </summary>
        /// <returns></returns>
        private IEnumerator DoFadeOutAnimation(float duration, bool isRepeat, float repeatAfter, bool isContinueFromCurrentFrame, Action Callback)
        {
            /// Sử dụng 32 hướng
            bool use32Dir = (int) this._Direction32 >= (int) Direction32.Down && (int) this._Direction32 < (int) Direction32.Count;
            /// Sử dụng 16 hướng
            bool use16Dir = (int) this._Direction >= (int) Direction16.Down && (int) this._Direction < (int) Direction16.Count;
            /// Sử dụng 8 hướng
            bool use8Dir = (int) this._Direction8 >= (int) Direction.DOWN && (int) this._Direction8 < (int) Direction.Count;

            #region Init
            /// Danh sách Sprite
            List<Sprite> actionSet = null;

            /// Danh sách Frame
            Dictionary<string, UnityEngine.Object> fadeOutActionSetFull = BulletAnimationManager.Instance.GetFadeOutSprites(this._ResID);
            if (fadeOutActionSetFull != null)
            {
                actionSet = new List<Sprite>();

                /// Nếu sử dụng 32 hướng
                if (use32Dir)
                {
                    /// Tổng số Frame mỗi hướng
                    int framesEachDir = fadeOutActionSetFull.Count / 32;
                    int fromID = ((int) this._Direction32) * framesEachDir;
                    /// Thêm Frame vào
                    actionSet.AddRange(fadeOutActionSetFull.Values.Skip(fromID).Take(framesEachDir).Select(x => x as Sprite));
                }
                /// Nếu sử dụng 16 hướng
                else if (use16Dir)
                {
                    /// Tổng số Frame mỗi hướng
                    int framesEachDir = fadeOutActionSetFull.Count / 16;
                    int fromID = ((int) this._Direction) * framesEachDir;
                    /// Thêm Frame vào
                    actionSet.AddRange(fadeOutActionSetFull.Values.Skip(fromID).Take(framesEachDir).Select(x => x as Sprite));
                }
                /// Nếu sử dụng 8 hướng
                else if (use8Dir)
                {
                    /// Tổng số Frame mỗi hướng
                    int framesEachDir = fadeOutActionSetFull.Count / 8;
                    int fromID = ((int) this._Direction8) * framesEachDir;
                    /// Thêm Frame vào
                    actionSet.AddRange(fadeOutActionSetFull.Values.Skip(fromID).Take(framesEachDir).Select(x => x as Sprite));
                }
                /// Nếu không phải
                else
                {
                    /// Thêm toàn bộ Frame vào
                    actionSet.AddRange(fadeOutActionSetFull.Values.Select(x => x as Sprite));
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
                        string soundName = BulletAnimationManager.Instance.Sounds[this._ResID].SoundFadeOut;
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
                this.UnloadFly();
                this.UnloadFadeOut();
			}
            this.LastResID = -1;

            this.playerCoroutine = null;
            this._Direction = Direction16.None;
            this._Direction32 = Direction32.None;
            this._Direction8 = Direction.NONE;
            this._ResID = -1;
            this.AdditionSortingOrder = 0;
            this.IsPlaying = false;
            this.IsPausing = false;
            this.CurrentFrameID = 0;
            this.OnStart = null;
            this.OnPause = null;
            this.OnResume = null;
            this.OnFrameChanging = null;
            this.OnCycleCompleted = null;
            this.Body.sprite = null;
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
        /// Phương thức Async thực hiện động tác bay
        /// </summary>
        /// <param name="dir8"></param>
        /// <param name="dir16"></param>
        /// <param name="dir32"></param>
        /// <param name="duration"></param>
        /// <param name="isRepeat"></param>
        /// <param name="repeatAfter"></param>
        /// <param name="isContinueFromCurrentFrame"></param>
        /// <param name="Callback"></param>
        /// <returns></returns>
        public IEnumerator DoFlyAsync(Direction dir8, Direction16 dir16, Direction32 dir32, float duration, bool isRepeat = false, float repeatAfter = 0, bool isContinueFromCurrentFrame = false, Action Callback = null)
        {
            this.Stop();

            if (!this.gameObject.activeSelf)
            {
                Callback?.Invoke();
                yield break;
            }
            if (!Loader.Loader.BulletActionSetXML.ResDatas.TryGetValue(this.ResID, out BulletActionSetXML.BulletResData bulletData))
            {
                Callback?.Invoke();
                yield break;
            }

            /// Xóa Res cũ
            if (this.LastResID != -1)
            {
                this.UnloadFly();
                this.UnloadFadeOut();
            }
            this.LastResID = this.ResID;
            this._Direction8 = dir8;
            this._Direction = dir16;
            this._Direction32 = dir32;

            /// Nếu không có động tác bay
            if (!bulletData.HasFlyAction)
			{
                Callback?.Invoke();
                yield break;
			}

            #region Load sprites
            int totalLoaded = 0;
            IEnumerator DoLoadFly()
            {
                yield return this.LoadFlyAsync();
                totalLoaded++;
            }
            IEnumerator DoLoadFadeOut()
            {
                yield return this.LoadFadeOutAsync();
                totalLoaded++;
            }
            IEnumerator DoLoadSound()
            {
                yield return this.LoadSoundAsync();
                totalLoaded++;
            }
            KTResourceManager.Instance.StartCoroutine(DoLoadFly());
            KTResourceManager.Instance.StartCoroutine(DoLoadFadeOut());
            KTResourceManager.Instance.StartCoroutine(DoLoadSound());

            while (totalLoaded < 3)
            {
                yield return null;
            }
            #endregion

            if (!this.gameObject || !this.gameObject.activeSelf)
            {
                yield break;
            }

            this.Stop();
            this.playerCoroutine = this.StartCoroutine(this.DoFlyAnimation(duration, isRepeat, repeatAfter, isContinueFromCurrentFrame, Callback));
        }

        /// <summary>
        /// Phương thức Async thực hiện động tác tan biến
        /// </summary>
        /// <param name="dir16"></param>
        /// <param name="duration"></param>
        /// <param name="isRepeat"></param>
        /// <param name="repeatAfter"></param>
        /// <param name="isContinueFromCurrentFrame"></param>
        /// <param name="Callback"></param>
        /// <returns></returns>
        public IEnumerator DoFadeOutAsync(Direction dir8, Direction16 dir16, Direction32 dir32, float duration, bool isRepeat = false, float repeatAfter = 0, bool isContinueFromCurrentFrame = false, Action Callback = null)
        {
            this.Stop();

            if (!this.gameObject.activeSelf)
            {
                Callback?.Invoke();
                yield break;
            }
            if (!Loader.Loader.BulletActionSetXML.ResDatas.TryGetValue(this.ResID, out BulletActionSetXML.BulletResData bulletData))
            {
                Callback?.Invoke();
                yield break;
            }

            /// Xóa Res cũ
            if (this.LastResID != -1)
			{
				this.UnloadFly();
				this.UnloadFadeOut();
			}
			this.LastResID = this.ResID;
            this._Direction8 = dir8;
            this._Direction = dir16;
            this._Direction32 = dir32;

            /// Nếu không có động tác tan
            if (!bulletData.HasFadeOutAction)
            {
                Callback?.Invoke();
                yield break;
            }

            #region Load sprites
            int totalLoaded = 0;
			IEnumerator DoLoadFly()
			{
				yield return this.LoadFlyAsync();
				totalLoaded++;
			}
			IEnumerator DoLoadFadeOut()
			{
				yield return this.LoadFadeOutAsync();
				totalLoaded++;
			}
			IEnumerator DoLoadSound()
			{
				yield return this.LoadSoundAsync();
				totalLoaded++;
			}
			KTResourceManager.Instance.StartCoroutine(DoLoadFly());
			KTResourceManager.Instance.StartCoroutine(DoLoadFadeOut());
			KTResourceManager.Instance.StartCoroutine(DoLoadSound());

			while (totalLoaded < 3)
			{
				yield return null;
			}
			#endregion

			this.Stop();
            this.playerCoroutine = this.StartCoroutine(this.DoFadeOutAnimation(duration, isRepeat, repeatAfter, isContinueFromCurrentFrame, Callback));
        }
        #endregion
    }
}
