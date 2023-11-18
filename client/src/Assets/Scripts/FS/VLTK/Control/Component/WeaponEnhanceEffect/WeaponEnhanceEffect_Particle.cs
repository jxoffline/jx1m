//#define USE_WEAPON_ANIMATION

using FS.VLTK.Logic.Settings;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static FS.VLTK.KTMath;

namespace FS.VLTK.Control.Component
{
    /// <summary>
    /// Hiệu ứng cường hóa vũ khí
    /// </summary>
    public partial class WeaponEnhanceEffect_Particle : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Đối tượng vũ khí tương ứng
        /// </summary>
        [SerializeField]
        private SpriteGlow.SpriteGlowEffect Weapon;
        #endregion

        #region Private fields
        /// <summary>
        /// Đang thực thi
        /// </summary>
        private bool isPlaying = false;

        /// <summary>
        /// Có đang đợi thực thi không
        /// </summary>
        private bool isWaitingToPlay = false;

        /// <summary>
        /// Đang thiết lập hủy hiệu ứng cường hóa
        /// </summary>
        private bool isEffectDisabled = true;
        #endregion

        #region Properties
        private KeyValuePair<Color, Color> _GlowColorRange;
        /// <summary>
        /// Phạm vi màu hiệu ứng Glow
        /// </summary>
        public KeyValuePair<Color, Color> GlowColorRange
        {
            get
            {
                return this._GlowColorRange;
            }
            set
            {
                this._GlowColorRange = value;
            }
        }

        /// <summary>
        /// Ngưỡng Alpha hiệu ứng Glow
        /// </summary>
        public float GlowAlpha { get; set; }

        /// <summary>
        /// Ngưỡng sáng hiệu ứng Glow
        /// </summary>
        public float BodyGlowThreshold
        {
            get
            {
                return this.Weapon.GlowBrightness;
            }
            set
            {
                this.Weapon.GlowBrightness = value;
            }
        }
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi khi đối tượng bị hủy kích hoạt
        /// </summary>
        private void OnDisable()
        {
            this.Clear();
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Thực thi sự kiện bỏ qua một số Frame
        /// </summary>
        /// <param name="skip"></param>
        /// <param name="work"></param>
        /// <returns></returns>
        private IEnumerator ExecuteSkipFrames(int skip, Action work)
		{
            for (int i = 1; i <= skip; i++)
			{
                yield return null;
			}
            work?.Invoke();
		}

        /// <summary>
        /// Thực hiện đổi màu liên tục
        /// </summary>
        /// <param name="fromColor"></param>
        /// <param name="toColor"></param>
        /// <returns></returns>
        private IEnumerator ChangeColor(Color fromColor, Color toColor, float duration)
        {
            float waitTime = 0.2f;
            WaitForSeconds wait = new WaitForSeconds(waitTime);

            Color color;

            color = fromColor;
            color.a = this.GlowAlpha;
            this.Weapon.GlowColor = color;
            yield return wait;

            float lifeTime = 0;

            while (true)
            {
                lifeTime += waitTime;
                if (lifeTime >= duration)
                {
                    break;
                }

                float percent = lifeTime / duration;
                Color newColor = fromColor + (toColor - fromColor) * percent;
                newColor.a = this.GlowAlpha;
                this.Weapon.GlowColor = newColor;

                yield return wait;
            }

            color = toColor;
            color.a = this.GlowAlpha;
            this.Weapon.GlowColor = color;
            yield return wait;
        }

        /// <summary>
        /// Thực thi hiệu ứng Glow lên vũ khí
        /// </summary>
        /// <returns></returns>
        private IEnumerator AnimateGlow()
        {
#if USE_WEAPON_ANIMATION
            /// Thời gian thực hiện hiệu ứng
            float glowDuration = 1f;
            /// Lặp liên tục
            while (true)
            {
				yield return this.ChangeColor(this._GlowColorRange.Key, this._GlowColorRange.Value, glowDuration);
				yield return this.ChangeColor(this._GlowColorRange.Value, this._GlowColorRange.Key, glowDuration);
            }
#else
            /// Toác không có Render
            while (this.Weapon.Renderer.sprite == null)
            {
                yield return null;
                continue;
            }

            WaitForSeconds wait = new WaitForSeconds(0.2f);
            yield return wait;
            Color color = this._GlowColorRange.Value;
            color.a = this.GlowAlpha;
            this.Weapon.GlowColor = color;
#endif
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Có đang thực thi không
        /// </summary>
        /// <returns></returns>
        public bool IsWaitingToPlay()
		{
            return this.isWaitingToPlay;
		}

        /// <summary>
        /// Thực hiện sau
        /// </summary>
        public void PlayLater()
		{
            this.isWaitingToPlay = true;
		}

        /// <summary>
        /// Bắt đầu thực thi hiệu ứng
        /// </summary>
        public void Play()
        {
            if (this.isPlaying)
            {
                return;
            }

            this.isPlaying = true;

            /// Bỏ đánh dấu đang chờ
            this.isWaitingToPlay = false;

            /// Kích hoạt đối tượng Glow
            this.Weapon.Apply();

            /// Hủy toàn bộ luồng cũ
            this.StopAllCoroutines();

            /// Bắt đầu thực thi hiệu ứng Glow
            this.StartCoroutine(this.AnimateGlow());
        }

        /// <summary>
        /// Ngừng thực thi hiệu ứng
        /// </summary>
        public void Pause()
        {
            if (!this.isPlaying)
            {
                return;
            }

            this.isPlaying = false;

            /// Bỏ đánh dấu đang chờ
            this.isWaitingToPlay = false;

            /// Ngừng toàn bộ các luồng đang thực thi
            this.StopAllCoroutines();

            /// Đánh dấu không phải đang thực thi hiệu ứng
            this.isPlaying = false;
            this.isWaitingToPlay = false;

            /// Hủy bỏ đối tượng hiệu ứng Glow
            this.Weapon.Apply();
        }

        /// <summary>
        /// Làm rỗng dữ liệu
        /// </summary>
        public void Clear()
        {
            /// Ngừng toàn bộ các luồng đang thực thi
            this.StopAllCoroutines();

            /// Đánh dấu không phải đang thực thi hiệu ứng
            this.isPlaying = false;
            this.isWaitingToPlay = false;

            this.GlowColorRange = new KeyValuePair<Color, Color>(default, default);
            this.GlowAlpha = 0f;
            this.BodyGlowThreshold = 0;
            this.Weapon.GlowColor = default;

            /// Hủy bỏ đối tượng hiệu ứng Glow
            this.Weapon.Apply();
        }
        #endregion
    }
}
