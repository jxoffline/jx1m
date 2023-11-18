using FS.VLTK.Factory;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FS.VLTK.Utilities.UnityComponent
{
    /// <summary>
    /// Đối tượng Render bóng của Sprite
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class SpriteTrailRenderer : TTMonoBehaviour
    {
        #region Define
        /// <summary>
        /// Khoảng thời gian liên tục mỗi lần tạo bóng
        /// </summary>
        [SerializeField]
        private float _Period = -1;

        /// <summary>
        /// Thời gian tồn tại
        /// </summary>
        [SerializeField]
        private float _Duration = 0.5f;
        #endregion

        #region Private fields
        /// <summary>
        /// Danh sách bóng đã được tạo ra
        /// </summary>
        private readonly List<SpriteRenderer> dummies = new List<SpriteRenderer>();
        #endregion

        #region Properties
        /// <summary>
        /// Khoảng thời gian liên tục mỗi lần tạo bóng
        /// <para>Giá trị này nếu nhỏ hơn 0 thì sẽ mặc định tạo liên tục mỗi Frame</para>
        /// </summary>
        public float Period
        {
            get
            {
                return this._Period;
            }
            set
            {
                this._Period = value;
            }
        }

        /// <summary>
        /// Thời gian tồn tại
        /// </summary>
        public float Duration
        {
            get
            {
                return this._Duration;
            }
            set
            {
                this._Duration = value;
            }
        }

        /// <summary>
        /// Kích hoạt hiện bóng đối tượng không
        /// </summary>
        public bool Enable
        {
            get
            {
                return this.enabled;
            }
            set
            {
                this.enabled = value;
            }
        }
        #endregion

        /// <summary>
        /// Đối tượng Renderer
        /// </summary>
        private SpriteRenderer _Renderer;

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi khi đối tượng được tạo ra
        /// </summary>
        private void Awake()
        {
            this._Renderer = this.GetComponent<SpriteRenderer>();
        }

        /// <summary>
        /// Hàm này gọi ở Frame đầu tiên
        /// </summary>
        private void Start()
        {
            if (this._Renderer == null)
            {
                return;
            }
        }

        /// <summary>
        /// Hàm này gọi khi đối tượng được kích hoạt
        /// </summary>
        private void OnEnable()
        {
            if (this._Renderer == null)
            {
                return;
            }
            this.StartCoroutine(this.DoCreateTrailEffect());
        }

        /// <summary>
        /// Hàm này gọi khi đối tượng bị hủy kích hoạt
        /// </summary>
        private void OnDisable()
        {
            this.StopAllCoroutines();
            this.Enable = false;

            foreach (SpriteRenderer dummy in this.dummies)
            {
                dummy.sprite = null;
                KTObjectPoolManager.Instance.ReturnToPool(dummy.gameObject);
            }
            this.dummies.Clear();
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Luồng thực hiện tạo bóng liên tục sau mỗi khoảng
        /// </summary>
        /// <returns></returns>
        private IEnumerator DoCreateTrailEffect()
        {
            while (true)
            {
                this.Execute();

                if (this._Period <= 0)
                {
                    yield return null;
                }
                else
                {
                    yield return new WaitForSeconds(this._Period);
                }
            }
        }

        /// <summary>
        /// Luồng thực hiện làm mờ đối tượng
        /// </summary>
        /// <param name="renderer"></param>
        /// <param name="duration"></param>
        /// <returns></returns>
        private IEnumerator Fade(SpriteRenderer renderer, float duration)
        {
            float lifeTime = 0f;
            float originAlpha = renderer.color.a;
            yield return null;
            while (lifeTime < duration)
            {
                lifeTime += Time.deltaTime;
                float percent = lifeTime / duration;

                float newAlpha = originAlpha * (1 - percent);
                Color color = renderer.color;
                color.a = newAlpha;
                renderer.color = color;

                yield return null;
            }
            renderer.sprite = null;
            this.dummies.Remove(renderer);
            KTObjectPoolManager.Instance.ReturnToPool(renderer.gameObject);
        }

        /// <summary>
        /// Thực hiện tạo hiệu ứng bóng
        /// </summary>
        private void Execute()
        {
            SpriteRenderer dummy = KTObjectPoolManager.Instance.Instantiate<SpriteRenderer>("SpriteRenderer");
            /// Nếu Pool đã đầy hoặc có lỗi gì đó
            if (dummy == null)
            {
                return;
            }
            this.dummies.Add(dummy);
            dummy.sprite = this._Renderer.sprite;
            dummy.drawMode = SpriteDrawMode.Sliced;
            dummy.color = this._Renderer.color;
            dummy.size = this._Renderer.size;
            dummy.transform.position = this._Renderer.transform.position;
            dummy.transform.rotation = this._Renderer.transform.rotation;
            this.StartCoroutine(this.Fade(dummy, this._Duration));
        }
        #endregion
    }
}
