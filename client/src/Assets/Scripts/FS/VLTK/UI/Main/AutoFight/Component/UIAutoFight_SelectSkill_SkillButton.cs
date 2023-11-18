using FS.VLTK.Entities.Config;
using FS.VLTK.Utilities.UnityUI;
using System;
using TMPro;
using UnityEngine;

namespace FS.VLTK.UI.Main.AutoFight.Component
{
    /// <summary>
    /// Button kỹ năng trong khung chọn kỹ năng thiết lập Auto
    /// </summary>
    public class UIAutoFight_SelectSkill_SkillButton : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Button
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton;

        /// <summary>
        /// Image icon kỹ năng
        /// </summary>
        [SerializeField]
        private SpriteFromAssetBundle UIImage_Icon;

        /// <summary>
        /// Text tên kỹ năng
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_Name;
        #endregion

        #region Properties
        private SkillDataEx _Data;
        /// <summary>
        /// ID kỹ năng
        /// </summary>
        public SkillDataEx Data
        {
            get
            {
                return this._Data;
            }
            set
            {
                this._Data = value;
                /// Icon
                this.UIImage_Icon.BundleDir = value.IconBundleDir;
                this.UIImage_Icon.AtlasName = value.IconAtlasName;
                this.UIImage_Icon.SpriteName = value.Icon;
                this.UIImage_Icon.Load();
                /// Tên
                this.UIText_Name.text = value.Name;
            }
        }

        /// <summary>
        /// Sự kiện chọn kỹ năng
        /// </summary>
        public Action Click { get; set; }
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
            this.UIButton.onClick.AddListener(this.Button_Clicked);
        }

        /// <summary>
        /// Sự kiện khi Button được chọn
        /// </summary>
        private void Button_Clicked()
        {
            this.Click?.Invoke();
        }
        #endregion
    }
}
