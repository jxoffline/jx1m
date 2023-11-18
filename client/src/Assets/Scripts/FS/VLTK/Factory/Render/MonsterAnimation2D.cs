using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static FS.VLTK.Entities.Enum;
using FS.VLTK.Factory;
using FS.VLTK.Factory.Animation;

namespace FS.VLTK.Utilities.UnityComponent
{
	/// <summary>
	/// Đối tượng quản lý Animation 2D của quái
	/// </summary>
	public class MonsterAnimation2D : TTMonoBehaviour
    {
        #region Private properties
        /// <summary>
        /// Luồng thực thi play hiệu ứng
        /// </summary>
        private Coroutine playerCoroutine;
        #endregion

        #region Define
        #region Reference objects
        /// <summary>
        /// Thân quái
        /// </summary>
        public SpriteRenderer Body { get; set; }
        #endregion

        private string _ResID;
        /// <summary>
        /// ID Res
        /// </summary>
        public string ResID
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
        /// Động tác trước đó
        /// </summary>
        public MonsterActionType LastAction { get; private set; }

        /// <summary>
        /// ID Res lần trước
        /// </summary>
        public string LastResID { get; private set; } = null;

        /// <summary>
        /// Hướng
        /// </summary>
        public Direction Dir { get; private set; }

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

        /// <summary>
        /// Tải lại Res
        /// </summary>
        public void Reload()
        {
            this.OnDataChanged();
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Phương thức Async tải động tác hiện tại
        /// </summary>
        /// <param name="actionType"></param>
        /// <param name="callbackIfNeedToLoad"></param>
        /// <returns></returns>
        private IEnumerator LoadBodyAsync(MonsterActionType actionType, Action callbackIfNeedToLoad)
        {
            string resID = this.ResID;
            Direction dir = this.Dir;

            yield return MonsterAnimationManager.Instance.LoadSprites(resID, actionType, dir, callbackIfNeedToLoad);
        }

        /// <summary>
        /// Phương thức Async tải âm thanh theo động tác hiện tại
        /// </summary>
        /// <param name="actionType"></param>
        /// <returns></returns>
        private IEnumerator LoadSoundAsync(MonsterActionType actionType)
        {
            string resID = this.ResID;

            yield return MonsterAnimationManager.Instance.LoadSounds(resID, actionType);
        }

        /// <summary>
        /// Hủy động tác
        /// </summary>
        private void UnloadBody()
        {
            if (this.LastResID == null)
            {
                return;
            }

            string resID = this.LastResID;
            MonsterActionType actionType = this.LastAction;
            Direction dir = this.Dir;

            MonsterAnimationManager.Instance.UnloadSprites(resID, actionType, dir);
        }

        /// <summary>
        /// Hàm này gọi đến khi dữ liệu thay đổi
        /// </summary>
        private void OnDataChanged()
        {
            /// Xóa toàn bộ Sprite hiệu ứng
            this.RemoveAllAnimationSprites();
        }

        /// <summary>
        /// Thực hiện động tác
        /// </summary>
        /// <returns></returns>
        private IEnumerator DoPlay(MonsterActionType actionType, float duration, int repeatCount, float repeatAfter, bool isContinueFromCurrentFrame, Action Callback)
        {
            #region Init
            /// Danh sách Sprite
            List<Sprite> actionSet = null;

            /// Tự động xoay
            bool autoFlip = false;
            /// Res tương ứng
            if (Loader.Loader.MonsterActionSetXML.Monsters.TryGetValue(this.ResID, out Entities.Config.MonsterActionSetXML.Component resData))
            {
                /// Nếu tự xoay
                if (resData.AutoFlip)
                {
                    /// Hướng xoay mới
                    Direction flipDir = KTGlobal.GetAutoFlipDirection(this.Dir);
                    /// Nếu khác thì sẽ tự xoay
                    autoFlip = flipDir != this.Dir;
                }
            }
            /// Toác
            else
            {
                yield break;
            }

            /// Tên động tác
            string actionName = MonsterAnimationManager.Instance.GetActionName(actionType, resData.AutoFlip);

            /// Danh sách Frame
            Dictionary<string, UnityEngine.Object> fullActionSprites = MonsterAnimationManager.Instance.GetSprites(this.ResID, actionType, this.Dir);
            /// Nếu tồn tại
            if (fullActionSprites != null)
			{
                actionSet = new List<Sprite>();

                /// Duyệt danh sách
                foreach (KeyValuePair<string, UnityEngine.Object> pair in fullActionSprites)
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

            //this.IsPausing = false;
            this.IsPlaying = true;
            this.OnStart?.Invoke();

            float dTime = duration / actionSet.Count;

            WaitForSeconds waitDTime = new WaitForSeconds(dTime);

            /// Tổng số lần đã lặp
            int totalRepeated = 1;

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
                    /// Nếu vẫn còn lặp lại
                    if (totalRepeated < repeatCount)
                    {
                        /// Tăng số lần lặp lên
                        totalRepeated = (totalRepeated + 1) % 100000007;

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
                    AudioPlayer player = this.gameObject.GetComponent<AudioPlayer>();
                    if (player != null)
                    {
                        player.Stop();
                        string soundName = MonsterAnimationManager.Instance.GetSoundNameByActionType(this.ResID, actionType);
                        if (!string.IsNullOrEmpty(soundName))
                        {
                            AudioClip sound = KTResourceManager.Instance.GetAsset<UnityEngine.AudioClip>(Loader.Loader.MonsterActionSetSoundBundleDir, soundName);
                            if (sound != null)
                            {
                                player.Sound = sound;
                                player.IsRepeat = false;
                                player.RepeatTimer = duration - sound.length;
                                player.Play();
                            }
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
                    ///// Nếu không Flip
                    //if (!autoFlip)
                    //{
                    //    /// Cập nhật vị trí
                    //    this.Body.transform.localPosition = new Vector2(resData.PosX, resData.PosY);

                    //    this.Body.flipX = autoFlip;
                    //}
                    ///// Nếu Flip
                    //else
                    //{
                    //    /// Pivot
                    //    Vector2 pivot = actionSet[this.CurrentFrameID].pivot;
                    //    /// Chiều ngang ảnh
                    //    float width = actionSet[this.CurrentFrameID].rect.size.x;
                    //    /// Vị trí X mới
                    //    float newX = 500 - width / 2 + pivot.x;
                    //    ///// Nếu lớn hơn kích thước bóng
                    //    //if (width > 150)
                    //    //{
                    //    //    newX += 150 + width / 2;
                    //    //}
                    //    //else
                    //    //{
                    //    //    newX += width;
                    //    //}

                    //    /// Cập nhật vị trí
                    //    this.Body.transform.localPosition = new Vector2(newX, resData.PosY);

                    //    this.Body.flipX = autoFlip;
                    //}
                    Vector2 originPosX = this.Body.transform.localPosition;
                    this.Body.transform.localPosition = new Vector3(originPosX.x, originPosX.y, -(this.AdditionSortingOrder / 10000f + 9 / 1000000f));
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
            if (!string.IsNullOrEmpty(this.LastResID))
            {
                this.UnloadBody();
            }
            this.LastResID = null;

            this._ResID = null;
            this.AdditionSortingOrder = 0;
            this.LastAction = MonsterActionType.NormalStand;
            this.IsPlaying = false;
            this.IsPausing = false;
            this.CurrentFrameID = 0;
            this.OnStart = null;
            this.OnPause = null;
            this.OnResume = null;
            this.OnFrameChanging = null;
            this.OnCycleCompleted = null;
            this.Body.sprite = null;

            this.StopAllCoroutines();
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
        /// <param name="actionType"></param>
        /// <param name="dir"></param>
        /// <param name="duration"></param>
        /// <param name="isRepeat"></param>
        /// <param name="repeatAfter"></param>
        /// <param name="isContinueFromCurrentFrame"></param>
        /// <param name="Callback"></param>
        /// <returns></returns>
        public IEnumerator DoActionAsync(MonsterActionType actionType, Direction dir, float duration, int repeatCount = 0, float repeatAfter = 0, bool isContinueFromCurrentFrame = false, Action Callback = null)
        {
            //if (true)
            //{
            //    yield break;
            //}

            if (!this.gameObject.activeSelf)
            {
                yield break;
            }

            if (this.IsPausing)
            {
                yield return null;
            }

            /// Fix toác hướng
            if (dir == Direction.NONE)
			{
                dir = Direction.DOWN;
			}

            /// Nếu tồn tại Res cũ
            if (!string.IsNullOrEmpty(this.LastResID))
			{
                this.UnloadBody();
			}
            this.LastAction = actionType;
            this.LastResID = this._ResID;
            this.Dir = dir;

            #region Load sprites
            int totalLoaded = 0;
            IEnumerator DoLoadBody()
            {
                yield return this.LoadBodyAsync(actionType, () => {
                    this.RemoveAllAnimationSprites();
                });
                totalLoaded++;
            }
            IEnumerator DoLoadSound()
            {
                yield return this.LoadSoundAsync(actionType);
                totalLoaded++;
            }
            KTResourceManager.Instance.StartCoroutine(DoLoadBody());
            KTResourceManager.Instance.StartCoroutine(DoLoadSound());
            while (totalLoaded < 2)
            {
                yield return null;
            }
            #endregion

            /// Ngừng luồng cũ
            if (this.playerCoroutine != null)
            {
                this.StopCoroutine(this.playerCoroutine);
            }

            this.Stop();
            this.playerCoroutine = this.StartCoroutine(this.DoPlay(actionType, duration, repeatCount, repeatAfter, isContinueFromCurrentFrame, Callback));
        }

        /// <summary>
        /// Xóa toàn bộ Sprite hiệu ứng
        /// </summary>
        public void RemoveAllAnimationSprites()
        {
            if (this.playerCoroutine != null)
            {
                this.StopCoroutine(this.playerCoroutine);
            }
            this.Body.sprite = null;
        }
        #endregion
    }
}