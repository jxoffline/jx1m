using FS.VLTK.UI.Main.Welfare.Recharge;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using Server.Data;
using FS.VLTK.Utilities.UnityUI;

namespace FS.VLTK.UI.Main.Welfare
{
    /// <summary>
    /// Khung ưu đãi nạp trong gói phúc lợi
    /// </summary>
    public class UIWelfare_Recharge : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Khung nạp lần đầu
        /// </summary>
        [SerializeField]
        private UIWelfare_Recharge_FirstRecharge UI_FirstRechargeContent;

        /// <summary>
        /// Khung nạp mỗi ngày
        /// </summary>
        [SerializeField]
        private UIWelfare_Recharge_EverydayRecharge UI_EverydayRechargeContent;

        /// <summary>
        /// Khung tích nạp
        /// </summary>
        [SerializeField]
        private UIWelfare_Recharge_TotalRecharge UI_TotalRechargeContent;

        /// <summary>
        /// Khung tích tiêu
        /// </summary>
        [SerializeField]
        private UIWelfare_Recharge_TotalConsume UI_TotalConsumeContent;

        /// <summary>
        /// Button mở khung nạp lần đầu
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_FirstRecharge;

        /// <summary>
        /// Button mở khung nạp hàng ngày
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_EverydayRecharge;

        /// <summary>
        /// Button mở khung tích nạp
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_TotalRecharge;

        /// <summary>
        /// Button mở khung tích tiêu
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_TotalConsume;

        /// <summary>
        /// Text tổng số đã nạp
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_TotalRecharge;

        /// <summary>
        /// Text tổng số đã tiêu
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_TotalConsume;

        /// <summary>
        /// Hint nạp lần đầu
        /// </summary>
        [SerializeField]
        private UIFlicker UIFlicker_FirstRechargeHint;

        /// <summary>
        /// Hint nạp mỗi ngày
        /// </summary>
        [SerializeField]
        private UIFlicker UIFlicker_EverydayRechargeHint;

        /// <summary>
        /// Hint tích nạp
        /// </summary>
        [SerializeField]
        private UIFlicker UIFlicker_TotalRechargeHint;

        /// <summary>
        /// Hint tích tiêu
        /// </summary>
        [SerializeField]
        private UIFlicker UIFlicker_TotalConsumeHint;
        #endregion

        #region Private fields
        /// <summary>
        /// RectTransform danh sách Button chức năng
        /// </summary>
        private RectTransform transformButtonsList = null;
        #endregion

        #region Properties
        /// <summary>
        /// Sự kiện yêu cầu truy vấn thông tin phúc lợi nạp thẻ
        /// </summary>
        public Action QueryRechargeInfo { get; set; }

        /// <summary>
        /// Nhận thưởng nạp thẻ lần đầu
        /// </summary>
        public Action<FistRechage> GetFirstRechargeAward { get; set; }

        /// <summary>
        /// Nhận thưởng nạp thẻ mỗi ngày
        /// </summary>
        public Action<DayRechageAward> GetEverydayRechargeAward { get; set; }

        /// <summary>
        /// Nhận thưởng tích nạp
        /// </summary>
        public Action<TotalRechageAward> GetTotalRechargeAward { get; set; }

        /// <summary>
        /// Nhận thưởng tích tiêu
        /// </summary>
        public Action<ConsumeAward> GetTotalConsumeAward { get; set; }

        /// <summary>
        /// Dữ liệu
        /// </summary>
        public RechageAcitivty Data { get; set; }

        /// <summary>
        /// Khung nạp lần đầu
        /// </summary>
        public UIWelfare_Recharge_FirstRecharge UIFirstRecharge
        {
            get
            {
                return this.UI_FirstRechargeContent;
            }
        }

        /// <summary>
        /// Khung nạp mỗi ngày
        /// </summary>
        public UIWelfare_Recharge_EverydayRecharge UIEverydayRecharge
        {
            get
            {
                return this.UI_EverydayRechargeContent;
            }
        }

        /// <summary>
        /// Khung tích nạp
        /// </summary>
        public UIWelfare_Recharge_TotalRecharge UITotalRecharge
        {
            get
            {
                return this.UI_TotalRechargeContent;
            }
        }

        /// <summary>
        /// Khung tích tiêu
        /// </summary>
        public UIWelfare_Recharge_TotalConsume UITotalConsume
        {
            get
            {
                return this.UI_TotalConsumeContent;
            }
        }

        /// <summary>
        /// Mặc định hiện khung nạp lần đầu
        /// </summary>
        public bool DefaultShowFirstRecharge { get; set; }
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi khi đối tượng được tạo ra
        /// </summary>
        private void Awake()
        {
            this.transformButtonsList = this.UIButton_FirstRecharge.transform.parent.GetComponent<RectTransform>();
        }

        /// <summary>
        /// Hàm này gọi ở Frame đầu tiên
        /// </summary>
        private void Start()
        {
            this.InitPrefabs();
            /// Gửi yêu cầu truy vấn thông tin phúc lợi thẻ nạp
            this.QueryRechargeInfo?.Invoke();
        }
        #endregion

        #region Code UI
        /// <summary>
        /// Khởi tạo ban đầu
        /// </summary>
        private void InitPrefabs()
        {
            this.UIButton_FirstRecharge.onClick.AddListener(this.ButtonOpenFirstRecharge_Clicked);
            this.UIButton_EverydayRecharge.onClick.AddListener(this.ButtonOpenEverydayRecharge_Clicked);
            this.UIButton_TotalRecharge.onClick.AddListener(this.ButtonOpenTotalRecharge_Clicked);
            this.UIButton_TotalConsume.onClick.AddListener(this.ButtonOpenTotalConsume_Clicked);

            this.UI_FirstRechargeContent.Close = this.BackToMainUI;
            this.UI_FirstRechargeContent.GetAwards = this.GetFirstRechargeAward;
            this.UI_EverydayRechargeContent.Close = this.BackToMainUI;
            this.UI_EverydayRechargeContent.Get = this.GetEverydayRechargeAward;
            this.UI_TotalRechargeContent.Close = this.BackToMainUI;
            this.UI_TotalRechargeContent.Get = this.GetTotalRechargeAward;
            this.UI_TotalConsumeContent.Close = this.BackToMainUI;
            this.UI_TotalConsumeContent.Get = this.GetTotalConsumeAward;
        }

        /// <summary>
        /// Sự kiện khi Button mở khung nạp lần đầu được ấn
        /// </summary>
        private void ButtonOpenFirstRecharge_Clicked()
        {
            /// Ẩn khung
            this.transformButtonsList.gameObject.SetActive(false);
            this.UI_EverydayRechargeContent.Hide();
            this.UI_TotalRechargeContent.Hide();
            this.UI_TotalConsumeContent.Hide();
            /// Mở khung nạp lần đầu
            this.UI_FirstRechargeContent.Show();
        }

        /// <summary>
        /// Sự kiện khi Button mở khung nạp mỗi ngày được ấn
        /// </summary>
        private void ButtonOpenEverydayRecharge_Clicked()
        {
            /// Ẩn khung
            this.transformButtonsList.gameObject.SetActive(false);
            this.UI_FirstRechargeContent.Hide();
            this.UI_TotalRechargeContent.Hide();
            this.UI_TotalConsumeContent.Hide();
            /// Mở khung nạp mỗi ngày
            this.UI_EverydayRechargeContent.Show();
        }

        /// <summary>
        /// Sự kiện khi Button mở khung tích nạp được ấn
        /// </summary>
        private void ButtonOpenTotalRecharge_Clicked()
        {
            /// Ẩn khung
            this.transformButtonsList.gameObject.SetActive(false);
            this.UI_FirstRechargeContent.Hide();
            this.UI_EverydayRechargeContent.Hide();
            this.UI_TotalConsumeContent.Hide();
            /// Mở khung tích nạp
            this.UI_TotalRechargeContent.Show();
        }

        /// <summary>
        /// Sự kiện khi Button mở khung tích tiêu được ấn
        /// </summary>
        private void ButtonOpenTotalConsume_Clicked()
        {
            /// Ẩn khung
            this.transformButtonsList.gameObject.SetActive(false);
            this.UI_FirstRechargeContent.Hide();
            this.UI_EverydayRechargeContent.Hide();
            this.UI_TotalRechargeContent.Hide();
            /// Mở khung tích tiêu
            this.UI_TotalConsumeContent.Show();
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Trở về màn hình chính
        /// </summary>
        private void BackToMainUI()
        {
            this.UI_FirstRechargeContent.Hide();
            this.UI_EverydayRechargeContent.Hide();
            this.UI_TotalRechargeContent.Hide();
            this.UI_TotalConsumeContent.Hide();
            this.transformButtonsList.gameObject.SetActive(true);
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Làm mới dữ liệu
        /// </summary>
        public void Refresh()
        {
            /// Ẩn toàn bộ hint
            this.UIFlicker_FirstRechargeHint.Hide();
            this.UIFlicker_EverydayRechargeHint.Hide();
            this.UIFlicker_TotalRechargeHint.Hide();
            this.UIFlicker_TotalConsumeHint.Hide();

            /// Thiết lập tổng số đã nạp
            {
                string[] strInfos = this.Data._TotalRechage.BtnState.Split(':');
                if (strInfos.Length != 2)
                {
                    KTGlobal.AddNotification("Có lỗi khi tải dữ liệu phúc lợi nạp thẻ, hãy báo hỗ trợ để được trợ giúp!");
                    PlayZone.Instance.HideUIWelfare();
                    return;
                }
                try
                {
                    this.UIText_TotalRecharge.text = KTGlobal.GetDisplayMoney(int.Parse(strInfos[1]));
                }
                catch (Exception)
                {
                    KTGlobal.AddNotification("Có lỗi khi tải dữ liệu phúc lợi nạp thẻ, hãy báo hỗ trợ để được trợ giúp!");
                    PlayZone.Instance.HideUIWelfare();
                    return;
                }
            }
            /// Thiết lập tổng số đã tiêu
            {
                string[] strInfos = this.Data._TotalConsume.BtnState.Split(':');
                if (strInfos.Length != 2)
                {
                    KTGlobal.AddNotification("Có lỗi khi tải dữ liệu phúc lợi nạp thẻ, hãy báo hỗ trợ để được trợ giúp!");
                    PlayZone.Instance.HideUIWelfare();
                    return;
                }
                try
                {
                    this.UIText_TotalConsume.text = KTGlobal.GetDisplayMoney(int.Parse(strInfos[1]));
                }
                catch (Exception)
                {
                    KTGlobal.AddNotification("Có lỗi khi tải dữ liệu phúc lợi nạp thẻ, hãy báo hỗ trợ để được trợ giúp!");
                    PlayZone.Instance.HideUIWelfare();
                    return;
                }
            }

            /// Thiết lập dữ liệu cho các khung thành phần
            this.UI_FirstRechargeContent.Data = this.Data._FistRechage;
            this.UI_EverydayRechargeContent.Data = this.Data._DayRechage;
            this.UI_TotalRechargeContent.Data = this.Data._TotalRechage;
            this.UI_TotalConsumeContent.Data = this.Data._TotalConsume;

            /// Đánh dấu có phát hiện chỗ nào Hint không
            bool isFoundHint = false;

            /// Hiện các Hint tương ứng
            if (this.Data.HasFirstRechargeHint)
            {
                this.UIFlicker_FirstRechargeHint.Show();
                isFoundHint = true;
            }
            if (this.Data.HasEverydayRechargeHint)
            {
                this.UIFlicker_EverydayRechargeHint.Show();
                isFoundHint = true;
            }
            if (this.Data.HasTotalRechargeHint)
            {
                this.UIFlicker_TotalRechargeHint.Show();
                isFoundHint = true;
            }
            if (this.Data.HasTotalConsumeHint)
            {
                this.UIFlicker_TotalConsumeHint.Show();
                isFoundHint = true;
            }

            /// Nếu đang hiện khung
            if (PlayZone.Instance.UIWelfare != null)
            {
                PlayZone.Instance.UIWelfare.HintRecharge(isFoundHint);

                /// Nếu mặc định hiện khung nạp lần đầu
                if (this.DefaultShowFirstRecharge)
                {
                    this.ButtonOpenFirstRecharge_Clicked();
                }
            }
        }
        #endregion
    }
}
