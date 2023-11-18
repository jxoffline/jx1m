using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

namespace FS.VLTK.Utilities.UnityUI
{
    /// <summary>
    /// Đối tượng chữ tự xóa sau khoảng thời gian
    /// </summary>
    public class UIFadeOutText : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Đối tượng Text
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText;

        /// <summary>
        /// Thời gian delay trước khi thực hiện ẩn đối tượng
        /// </summary>
        [SerializeField]
        private float _DelayTime = 5f;

        /// <summary>
        /// Thời gian thực hiện Animation ẩn đối tượng
        /// </summary>
        [SerializeField]
        private float _FadeDuration = 2f;
        #endregion

        #region Properties
        /// <summary>
        /// Nội dung
        /// </summary>
        public string Text
        {
            get
            {
                return this.UIText.text;
            }
            set
            {
                this.UIText.text = value;
            }
        }

        /// <summary>
        /// Thời gian delay trước khi thực hiện ẩn đối tượng
        /// </summary>
        public float DelayTime
        {
            get
            {
                return this._DelayTime;
            }
            set
            {
                this._DelayTime = value;
            }
        }

        /// <summary>
        /// Thời gian thực hiện Animation ẩn đối tượng
        /// </summary>
        public float FadeDuration
        {
            get
            {
                return this._FadeDuration;
            }
            set
            {
                this._FadeDuration = value;
            }
        }

        /// <summary>
        /// Sự kiện khi Animation hoàn tất
        /// </summary>
        public Action Finish { get; set; }
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi ở Frame đầu tiên
        /// </summary>
        private void Start()
        {
            this.InitPrefabs();
            this.StartCoroutine(this.DoAnimation());
        }
        #endregion

        #region Code UI
        /// <summary>
        /// Khởi tạo ban đầu
        /// </summary>
        private void InitPrefabs()
        {

        }
        #endregion

        #region Private methods
        /// <summary>
        /// Luồng thực thi hiệu ứng
        /// </summary>
        /// <returns></returns>
        private IEnumerator DoAnimation()
        {
            if (this._DelayTime > 0)
            {
                yield return new WaitForSeconds(this._DelayTime);
            }

            float lifeTime = 0f;
            while (lifeTime < this._FadeDuration)
            {
                float percent = lifeTime / this._FadeDuration;
                Color color = this.UIText.color;
                color.a = 1 - percent;
                this.UIText.color = color;

                lifeTime += Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }

            this.Finish?.Invoke();
            GameObject.Destroy(this.gameObject);
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Xóa đối tượng ngay lập tức, nếu dùng hàm này thì sự kiện Finish sẽ không được gọi tới
        /// </summary>
        public void DestroyImmediate()
        {
            GameObject.DestroyImmediate(this.gameObject);
        }
        #endregion
    }
}

