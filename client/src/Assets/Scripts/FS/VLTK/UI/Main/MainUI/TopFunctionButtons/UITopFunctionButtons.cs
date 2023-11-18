using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static FS.VLTK.Entities.Enum;

namespace FS.VLTK.UI.Main.MainUI
{
    /// <summary>
    /// Khung quản lý các Button hoạt động ở trên
    /// </summary>
    public class UITopFunctionButtons : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Button hiện
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_Show;

        /// <summary>
        /// Button ẩn
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_Hide;

        /// <summary>
        /// Tọa độ ở trạng thái hiện
        /// </summary>
        [SerializeField]
        private Vector2 VisiblePos;

        /// <summary>
        /// Tọa độ ở trạng thái ẩn
        /// </summary>
        [SerializeField]
        private Vector2 InvisiblePos;

        /// <summary>
        /// Thời gian thực hiện hiệu ứng
        /// </summary>
        [SerializeField]
        private float AnimationDuraiton = 0.2f;

        /// <summary>
        /// Button mở khung Kỳ Trân Các
        /// </summary>
        [SerializeField]
        private UIHintButton UIButton_OpenTokenShop;

        /// <summary>
        /// Button mở khung vòng quay may mắn
        /// </summary>
        [SerializeField]
        private UIHintButton UIButton_OpenLuckyCircle;

        /// <summary>
        /// Button mở khung phúc lợi nạp thẻ lần đầu
        /// </summary>
        [SerializeField]
        private UIHintButton UIButton_OpenWelfareFirstRecharge;

        /// <summary>
        /// Button mở khung phúc lợi
        /// </summary>
        [SerializeField]
        private UIHintButton UIButton_OpenWelfare;

        /// <summary>
        /// Button mở khung thông tin hoạt động
        /// </summary>
        [SerializeField]
        private UIHintButton UIButton_OpenActivityList;

        /// <summary>
        /// Button mở bảng xếp hạng
        /// </summary>
        [SerializeField]
        private UIHintButton UIButton_OpenRanking;

        /// <summary>
        /// Button mở khung đua top
        /// </summary>
        [SerializeField]
        private UIHintButton UIButton_OpenTopRanking;
        #endregion

        #region Properties
        /// <summary>
        /// Mở khung Kỳ Trân Các
        /// </summary>
        public Action OpenTokenShop { get; set; }

        /// <summary>
        /// Mở khung vòng quay may mắn
        /// </summary>
        public Action OpenLuckyCircle { get; set; }

        /// <summary>
        /// Mở khung phúc lợi nạp thẻ lần đầu
        /// </summary>
        public Action OpenWelfareFirstRecharge { get; set; }

        /// <summary>
        /// Mở khung phúc lợi
        /// </summary>
        public Action OpenWelfare { get; set; }

        /// <summary>
        /// Mở khung danh sách hoạt động
        /// </summary>
        public Action OpenActivityList { get; set; }

        /// <summary>
        /// Mở khung xếp hạng
        /// </summary>
        public Action OpenRanking { get; set; }

        /// <summary>
        /// Mở khung đua top
        /// </summary>
        public Action OpenTopRanking { get; set; }
        #endregion

        #region Private fields
        /// <summary>
        /// RectTransform khung
        /// </summary>
        private RectTransform rectTransform = null;

        /// <summary>
        /// Đánh dấu có đang hiển thị không
        /// </summary>
        private bool IsVisible
        {
            get
            {
                return this.UIButton_Hide.gameObject.activeSelf;
            }
            set
            {
                this.UIButton_Hide.gameObject.SetActive(value);
                this.UIButton_Show.gameObject.SetActive(!value);
            }
        }
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi ở Frame đầu tiên
        /// </summary>
        private void Awake()
        {
            this.rectTransform = this.GetComponent<RectTransform>();
        }

        /// <summary>
        /// Hàm này gọi ở Frame đầu tiên
        /// </summary>
        private void Start()
        {
            this.InitPrefabs();
            this.IsVisible = true;
        }
        #endregion

        #region Code UI
        /// <summary>
        /// Khởi tạo ban đầu
        /// </summary>
        private void InitPrefabs()
        {
            this.UIButton_Show.onClick.AddListener(this.ButtonShow_Clicked);
            this.UIButton_Hide.onClick.AddListener(this.ButtonHide_Clicked);

            this.UIButton_OpenTokenShop.Click = this.OpenTokenShop;
            this.UIButton_OpenLuckyCircle.Click = this.OpenLuckyCircle;
            this.UIButton_OpenWelfareFirstRecharge.Click = this.OpenWelfareFirstRecharge;
            this.UIButton_OpenWelfare.Click = this.OpenWelfare;
            this.UIButton_OpenActivityList.Click = this.OpenActivityList;
            this.UIButton_OpenRanking.Click = this.OpenRanking;
            this.UIButton_OpenTopRanking.Click = this.OpenTopRanking;
        }

        /// <summary>
        /// Sự kiện khi Button ẩn được ấn
        /// </summary>
        private void ButtonHide_Clicked()
        {
            /// Nếu đang ẩn
            if (!this.IsVisible)
            {
                return;
            }

            /// Ẩn cả 2 Button
            this.UIButton_Hide.gameObject.SetActive(false);
            this.UIButton_Show.gameObject.SetActive(false);
            this.StartCoroutine(this.Translate(this.VisiblePos, this.InvisiblePos, () => {
                this.IsVisible = false;
            }));
        }

        /// <summary>
        /// Sự kiện khi Button hiện được ấn
        /// </summary>
        private void ButtonShow_Clicked()
        {
            /// Nếu đang hiện
            if (this.IsVisible)
            {
                return;
            }

            /// Ẩn cả 2 Button
            this.UIButton_Hide.gameObject.SetActive(false);
            this.UIButton_Show.gameObject.SetActive(false);
            this.StartCoroutine(this.Translate(this.InvisiblePos, this.VisiblePos, () => {
                this.IsVisible = true;
            }));
        }
        #endregion

        #region Private fields
        /// <summary>
        /// Dich chuyển khung giữa các vị trí tương ứng
        /// </summary>
        /// <param name="fromPos"></param>
        /// <param name="toPos"></param>
        /// <param name="done"></param>
        /// <returns></returns>
        private IEnumerator Translate(Vector2 fromPos, Vector2 toPos, Action done)
        {
            /// Vector hướng dịch chuyển
            Vector2 dirVector = toPos - fromPos;
            /// Thiết lập vị trí ban đầu
            this.rectTransform.anchoredPosition = fromPos;
            /// Bỏ qua Frame
            yield return null;
            /// Thời gian tồn tại
            float lifeTime = 0f;
            /// Chừng nào chưa hết thời gian
            while (lifeTime < this.AnimationDuraiton)
            {
                /// Tăng thời gian tồn tại
                lifeTime += Time.deltaTime;
                /// % thời gian đã qua
                float percent = lifeTime / this.AnimationDuraiton;
                /// Cập nhật vị trí mới
                this.rectTransform.anchoredPosition = fromPos + dirVector * percent;
                /// Bỏ qua Frame
                yield return null;
            }
            /// Thiết lập vị trí đích
            this.rectTransform.anchoredPosition = toPos;
            /// Thực thi sự kiện hoàn tất
            done?.Invoke();
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Thay đổi trạng thái của Button
        /// </summary>
        /// <param name="buttonType"></param>
        /// <param name="action"></param>
        public void ChangeButtonState(FunctionButtonType buttonType, FunctionButtonAction action)
        {
            /// Đổi trạng thái của Button tương ứng
            void ChangeState(UIHintButton button)
            {
                switch (action)
                {
                    case FunctionButtonAction.Show:
                    {
                        button.Visible = true;
                        button.Enable = true;
                        button.Hint = false;
                        break;
                    }
                    case FunctionButtonAction.Hide:
                    {
                        button.Visible = false;
                        button.Enable = false;
                        button.Hint = false;
                        break;
                    }
                    case FunctionButtonAction.Enable:
                    {
                        button.Visible = true;
                        button.Enable = true;
                        button.Hint = false;
                        break;
                    }
                    case FunctionButtonAction.Disable:
                    {
                        button.Visible = true;
                        button.Enable = false;
                        button.Hint = false;
                        break;
                    }
                    case FunctionButtonAction.Hint:
                    {
                        button.Visible = true;
                        button.Enable = true;
                        button.Hint = true;
                        break;
                    }
                }
            }

            switch (buttonType)
            {
                case FunctionButtonType.OpenTokenShop:
                {
                    ChangeState(this.UIButton_OpenTokenShop);
                    break;
                }
                case FunctionButtonType.OpenLuckyCircle:
                {
                    ChangeState(this.UIButton_OpenLuckyCircle);
                    break;
                }
                case FunctionButtonType.OpenWelfareFirstRecharge:
                {
                    ChangeState(this.UIButton_OpenWelfareFirstRecharge);
                    break;
                }
                case FunctionButtonType.OpenWelfare:
                {
                    ChangeState(this.UIButton_OpenWelfare);
                    break;
                }
                case FunctionButtonType.OpenActivityList:
                {
                    ChangeState(this.UIButton_OpenActivityList);
                    break;
                }
            }
        }
        #endregion
    }
}
