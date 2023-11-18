using FS.VLTK.UI.Main.Welfare;
using FS.VLTK.UI.Main.Welfare.MonthCard;
using FS.VLTK.Utilities.UnityUI;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FS.VLTK.UI.Main
{
    /// <summary>
    /// Khung phúc lợi
    /// </summary>
    public class UIWelfare : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Button đóng khung
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_Close;

        /// <summary>
        /// Khung phúc lợi nạp thẻ
        /// </summary>
        [SerializeField]
        private UIWelfare_Recharge UIWelfare_Recharge;

        /// <summary>
        /// Khung phúc lợi Online
        /// </summary>
        [SerializeField]
        private UIWelfare_EverydayOnline UIWelfare_EverydayOnline;

        /// <summary>
        /// Khung phúc lợi đăng nhập 7 ngày
        /// </summary>
        [SerializeField]
        private UIWelfare_SevenDayLogin UIWelfare_SevenDayLogin;

        /// <summary>
        /// Khung phúc lợi đăng nhập tích lũy
        /// </summary>
        [SerializeField]
        private UIWelfare_SeriesLogin UIWelfare_SeriesLogin;

        /// <summary>
        /// Khung phúc lợi thăng cấp
        /// </summary>
        [SerializeField]
        private UIWelfare_LevelUp UIWelfare_LevelUp;

        /// <summary>
        /// Khung phúc lợi thẻ tháng
        /// </summary>
        [SerializeField]
        private UIWelfare_MonthCard UIWelfare_MonthCard;

        /// <summary>
        /// Hint khung phúc lợi nạp thẻ
        /// </summary>
        [SerializeField]
        private UIFlicker UIRecharge_Hint;

        /// <summary>
        /// Hint khung phúc lợi Online
        /// </summary>
        [SerializeField]
        private UIFlicker UIOnline_Hint;

        /// <summary>
        /// Hint khung phúc lợi đăng nhập
        /// </summary>
        [SerializeField]
        private UIFlicker UILogin_Hint;

        /// <summary>
        /// Hint khung phúc lợi đăng nhập liên tục
        /// </summary>
        [SerializeField]
        private UIFlicker UISeriesLogin_Hint;

        /// <summary>
        /// Hint khung phúc lợi thẻ tháng
        /// </summary>
        [SerializeField]
        private UIFlicker UIMonthCard_Hint;

        /// <summary>
        /// Hint khung phúc lợi thăng cấp
        /// </summary>
        [SerializeField]
        private UIFlicker UILevelUp_Hint;
        #endregion

        #region Properties
        /// <summary>
        /// Sự kiện đóng khung
        /// </summary>
        public Action Close { get; set; }

        /// <summary>
        /// Khung phúc lợi nạp thẻ
        /// </summary>
        public UIWelfare_Recharge UIRecharge
        {
            get
            {
                return this.UIWelfare_Recharge;
            }
        }

        /// <summary>
        /// Khung phúc lợi Online mỗi ngày
        /// </summary>
        public UIWelfare_EverydayOnline UIEverydayOnline
        {
            get
            {
                return this.UIWelfare_EverydayOnline;
            }
        }

        /// <summary>
        /// Khung phúc lợi đăng nhập 7 ngày
        /// </summary>
        public UIWelfare_SevenDayLogin UISevenDayLogin
        {
            get
            {
                return this.UIWelfare_SevenDayLogin;
            }
        }

        /// <summary>
        /// Khung phúc lợi đăng nhập tích lũy
        /// </summary>
        public UIWelfare_SeriesLogin UISeriesLogin
        {
            get
            {
                return this.UIWelfare_SeriesLogin;
            }
        }

        /// <summary>
        /// Khung phúc lợi thăng cấp
        /// </summary>
        public UIWelfare_LevelUp UILevelUp
        {
            get
            {
                return this.UIWelfare_LevelUp;
            }
        }

        /// <summary>
        /// Khung phúc lợi thẻ tháng
        /// </summary>
        public UIWelfare_MonthCard MonthCard
        {
            get
            {
                return this.UIWelfare_MonthCard;
            }
        }
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi ở Frame đầu tiên
        /// </summary>
        private void Start()
        {
            this.InitPrefabs();
        }
        #endregion

        #region Code UI
        /// <summary>
        /// Khởi tạo ban đầu
        /// </summary>
        private void InitPrefabs()
        {
            this.UIButton_Close.onClick.AddListener(this.ButtonClose_Clicked);

            this.UIRecharge_Hint.Hide();
            this.UIOnline_Hint.Hide();
            this.UILogin_Hint.Hide();
            this.UISeriesLogin_Hint.Hide();
            this.UIMonthCard_Hint.Hide();
            this.UILevelUp_Hint.Hide();
        }

        /// <summary>
        /// Sự kiện khi Button đóng khung được ấn
        /// </summary>
        private void ButtonClose_Clicked()
        {
            this.Close?.Invoke();
        }
        #endregion

        #region Private methods

        #endregion

        #region Public methods
        /// <summary>
        /// Hint phúc lợi nạp thẻ
        /// </summary>
        /// <param name="isHint"></param>
        public void HintRecharge(bool isHint)
        {
            if (isHint)
            {
                this.UIRecharge_Hint.Show();
            }
            else
            {
                this.UIRecharge_Hint.Hide();
            }
        }

        /// <summary>
        /// Hint phúc lợi Online
        /// </summary>
        /// <param name="isHint"></param>
        public void HintOnline(bool isHint)
        {
            if (isHint)
            {
                this.UIOnline_Hint.Show();
            }
            else
            {
                this.UIOnline_Hint.Hide();
            }
        }

        /// <summary>
        /// Hint phúc lợi đăng nhập
        /// </summary>
        /// <param name="isHint"></param>
        public void HintLogin(bool isHint)
        {
            if (isHint)
            {
                this.UILogin_Hint.Show();
            }
            else
            {
                this.UILogin_Hint.Hide();
            }
        }

        /// <summary>
        /// Hint phúc lợi đăng nhập liên tục
        /// </summary>
        /// <param name="isHint"></param>
        public void HintSeriesLogin(bool isHint)
        {
            if (isHint)
            {
                this.UISeriesLogin_Hint.Show();
            }
            else
            {
                this.UISeriesLogin_Hint.Hide();
            }
        }

        /// <summary>
        /// Hint phúc lợi thẻ tháng
        /// </summary>
        /// <param name="isHint"></param>
        public void HintMonthCard(bool isHint)
        {
            if (isHint)
            {
                this.UIMonthCard_Hint.Show();
            }
            else
            {
                this.UIMonthCard_Hint.Hide();
            }
        }

        /// <summary>
        /// Hint phúc lợi thăng cấp
        /// </summary>
        /// <param name="isHint"></param>
        public void HintLevelUp(bool isHint)
        {
            if (isHint)
            {
                this.UILevelUp_Hint.Show();
            }
            else
            {
                this.UILevelUp_Hint.Hide();
            }
        }
        #endregion
    }
}
