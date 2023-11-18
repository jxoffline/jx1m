using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;

namespace FS.VLTK.UI.Main.SeashellCircle
{
    /// <summary>
    /// Thông tin tầng quà thưởng khung Bách bảo rương
    /// </summary>
    public class UISeashellCircle_AwardStageInfo : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Toggle
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Toggle UIToggle;

        /// <summary>
        /// Text thông tin quà thưởng
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_AwardInfo;
        #endregion

        #region Properties
        private string _Text;
        /// <summary>
        /// Text thông tin quà thưởng
        /// </summary>
        public string Text
        {
            get
            {
                return this._Text;
            }
            set
            {
                this._Text = value;
                this.RefreshText();
            }
        }

        private int _Number;
        /// <summary>
        /// Số lượng nhân
        /// </summary>
        public int Number
        {
            get
            {
                return this._Number;
            }
            set
            {
                this._Number = value;
                this.RefreshText();
            }
        }

        /// <summary>
        /// Đang dừng lại ở tầng này không
        /// </summary>
        public bool Selected
        {
            get
            {
                return this.UIToggle.isOn;
            }
            set
            {
                this.UIToggle.isOn = value;
            }
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Làm mới hiển thị Text
        /// </summary>
        private void RefreshText()
        {
            this.UIText_AwardInfo.text = string.Format("{0} x{1}", this._Text, this._Number);
        }
        #endregion
    }
}
