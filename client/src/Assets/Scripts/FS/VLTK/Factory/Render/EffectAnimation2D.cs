using FS.VLTK.Entities.Config;
using FS.VLTK.Factory;
using FS.VLTK.Factory.Animation;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FS.VLTK.Utilities.UnityComponent
{
	/// <summary>
	/// Đối tượng quản lý Animation 2D của hiệu ứng
	/// </summary>
	public class EffectAnimation2D : TTMonoBehaviour
    {
        #region Private properties
        /// <summary>
        /// Luồng thực thi play hiệu ứng
        /// </summary>
        private Coroutine playerCoroutine;

        /// <summary>
        /// Dữ liệu lần trước
        /// </summary>
        private StateEffectXML LastData = null;
        #endregion

        #region Define
        #region Reference objects
        /// <summary>
        /// Thân hiệu ứng
        /// </summary>
        public SpriteRenderer Body { get; set; }
        #endregion

        private StateEffectXML _Data;
        /// <summary>
        /// Dữ liệu hiệu ứng
        /// </summary>
        public StateEffectXML Data
        {
            get
            {
                return this._Data;
            }
            set
            {
                this._Data = value;

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
        /// Phương thức Async tải động tác hiện tại
        /// </summary>
        /// <returns></returns>
        private IEnumerator LoadBodyAsync()
        {
            yield return EffectAnimationManager.Instance.LoadSprites(this._Data.ID);
        }

        /// <summary>
        /// Hủy động tác
        /// </summary>
        private void UnloadBody()
        {
            if (this.LastData == null)
            {
                return;
            }

            EffectAnimationManager.Instance.UnloadSprites(this.LastData.ID);
        }

        /// <summary>
        /// Hàm này gọi đến khi dữ liệu thay đổi
        /// </summary>
        private void OnDataChanged()
        {
            
        }

        /// <summary>
        /// Thực hiện động tác
        /// </summary>
        /// <returns></returns>
        private IEnumerator DoPlay(float duration, bool isRepeat, float repeatAfter, bool isContinueFromCurrentFrame, Action Callback)
        {
            #region Data
            /// Danh sách Sprite
            List<Sprite> actionSet = null;

            /// Danh sách Frame
            Dictionary<string, UnityEngine.Object> effectActionSetFull = EffectAnimationManager.Instance.GetSprites(this._Data.ID);
            if (effectActionSetFull != null)
            {
                actionSet = new List<Sprite>();

                foreach (KeyValuePair<string, UnityEngine.Object> pair in effectActionSetFull)
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

            this.IsPausing = false;
            this.IsPlaying = true;
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

                #region Play actions
                if (actionSet != null && this.CurrentFrameID < actionSet.Count && actionSet[this.CurrentFrameID] != null)
                {
                    this.Body.sprite = actionSet[this.CurrentFrameID];
                    this.Body.drawMode = SpriteDrawMode.Sliced;
                    this.Body.size = actionSet[this.CurrentFrameID].rect.size;
                    this.Body.gameObject.transform.localPosition = new Vector3(0, 0, -(this.AdditionSortingOrder / 10000f + (this._Data.PosType == "ground" ? 0 : 9) / 1000000f));
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
            /// Nếu có dữ liệu lần trước
            if (this.LastData != null)
			{
                this.UnloadBody();
			}
            this.LastData = null;

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

            /// Nếu có dữ liệu lần trước
            if (this.LastData != null)
			{
                this.UnloadBody();
			}
            this.LastData = new StateEffectXML()
            {
                ID = this.Data.ID,
            };

            #region Load sprites
            int totalLoaded = 0;
            IEnumerator DoLoadBody()
            {
                yield return this.LoadBodyAsync();
                totalLoaded++;
            }
            KTResourceManager.Instance.StartCoroutine(DoLoadBody());

            while (totalLoaded < 1)
            {
                yield return null;
            }
            #endregion

            this.Stop();
            this.playerCoroutine = this.StartCoroutine(this.DoPlay(duration, isRepeat, repeatAfter, isContinueFromCurrentFrame, Callback));
        }
        #endregion
    }
}
