using FS.GameEngine.Logic;
using FS.VLTK.UI.Main.MainUI.MiniTaskAndTeamFrame;
using FS.VLTK.Utilities.UnityUI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FS.VLTK.UI.Main.MainUI
{
    /// <summary>
    /// Khung MiniTaskBox và TeamFrame
    /// </summary>
    public class UIMiniTaskAndTeamFrame : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Khung MiniTaskBox
        /// </summary>
        [SerializeField]
        private UIMiniTaskAndTeamFrame_MiniTaskBox UI_MiniTaskBox;

        /// <summary>
        /// Khung MiniTeamFrame
        /// </summary>
        [SerializeField]
        private UIMiniTaskAndTeamFrame_MiniTeamFrame UI_MiniTeamFrame;

        /// <summary>
        /// Button hiện khung
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_Show;

        /// <summary>
        /// Button ẩn khung
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_Hide;

        /// <summary>
        /// Vị trí khung xuất hiện trên màn hình
        /// </summary>
        [SerializeField]
        private Vector2 VisiblePosition;

        /// <summary>
        /// Vị trí khung ẩn khỏi màn hình
        /// </summary>
        [SerializeField]
        private Vector2 InvisiblePosition;

        /// <summary>
        /// Thời gian thực hiện hiệu ứng
        /// </summary>
        [SerializeField]
        private float AnimationDuration;
        #endregion

        #region Properties
        /// <summary>
        /// Khung Mini Task Box
        /// </summary>
        public UIMiniTaskAndTeamFrame_MiniTaskBox UIMiniTaskBox
        {
            get
            {
                return this.UI_MiniTaskBox;
            }
        }

        /// <summary>
        /// Khung Mini Team Frame
        /// </summary>
        public UIMiniTaskAndTeamFrame_MiniTeamFrame UITeamFrame
        {
            get
            {
                return this.UI_MiniTeamFrame;
            }
        }
        #endregion

        /// <summary>
        /// RectTransform của đối tượng
        /// </summary>
        private RectTransform rectTransform;

        /// <summary>
        /// Luồng thực hiện hiệu ứng
        /// </summary>
        private Coroutine animationCoroutine;

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi ở Frame đầu tiên
        /// </summary>
        private void Start()
        {
            this.InitPrefabs();
        }

        /// <summary>
        /// Hàm này gọi khi đối tượng bị hủy kích hoạt
        /// </summary>
        private void OnDisable()
        {
            this.StopAllCoroutines();
            this.animationCoroutine = null;
        }
        #endregion

        #region Code UI
        /// <summary>
        /// Khởi tạo ban đầu
        /// </summary>
        private void InitPrefabs()
        {
            this.rectTransform = this.GetComponent<RectTransform>();
            this.UIButton_Hide.gameObject.SetActive(true);
            this.UIButton_Show.gameObject.SetActive(false);

            this.UIButton_Hide.onClick.AddListener(this.Hide);
            this.UIButton_Show.onClick.AddListener(this.Show);
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Luồng thực hiện chạy hiệu ứng
        /// </summary>
        /// <param name="fromPosition"></param>
        /// <param name="toPosition"></param>
        /// <returns></returns>
        private IEnumerator PlayAnimation(Vector2 fromPosition, Vector2 toPosition)
        {
            if (this.rectTransform == null)
            {
                yield break;
            }

            this.UIButton_Hide.gameObject.SetActive(false);
            this.UIButton_Show.gameObject.SetActive(false);

            this.rectTransform.anchoredPosition = fromPosition;
            yield return null;
            float lifeTime = 0f;
            while (lifeTime < this.AnimationDuration)
            {
                lifeTime += Time.deltaTime;
                float percent = lifeTime / this.AnimationDuration;
                Vector2 newPos = fromPosition + (toPosition - fromPosition) * percent;
                this.rectTransform.anchoredPosition = newPos;
                yield return null;
            }
            this.rectTransform.anchoredPosition = toPosition;

            if (toPosition == this.VisiblePosition)
            {
                this.UIButton_Hide.gameObject.SetActive(true);
            }
            else
            {
                this.UIButton_Show.gameObject.SetActive(true);
            }
        }

        /// <summary>
        /// Hiện khung
        /// </summary>
        private void Show()
        {
            if (this.animationCoroutine != null)
            {
                this.StopCoroutine(this.animationCoroutine);
            }

            this.animationCoroutine = this.StartCoroutine(this.PlayAnimation(this.InvisiblePosition, this.VisiblePosition));
        }

        /// <summary>
        /// Ẩn khung
        /// </summary>
        private void Hide()
        {
            if (this.animationCoroutine != null)
            {
                this.StopCoroutine(this.animationCoroutine);
            }

            this.animationCoroutine = this.StartCoroutine(this.PlayAnimation(this.VisiblePosition, this.InvisiblePosition));
        }
        #endregion
    }
}

