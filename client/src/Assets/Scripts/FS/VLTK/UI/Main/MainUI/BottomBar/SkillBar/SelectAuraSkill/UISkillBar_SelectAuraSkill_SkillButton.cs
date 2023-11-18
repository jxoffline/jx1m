using FS.VLTK.Utilities.UnityUI;
using System;
using UnityEngine;

namespace FS.VLTK.UI.Main.MainUI.BottomBar.SkillBar
{
    /// <summary>
    /// Button kỹ năng trong khung chọn kỹ năng vòng sáng
    /// </summary>
    public class UISkillBar_SelectAuraSkill_SkillButton : MonoBehaviour
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
        private SpriteFromAssetBundle UIImage_SkillIcon;
        #endregion

        #region Properties
        private Entities.Config.SkillDataEx _Data;
        /// <summary>
        /// Dữ liệu kỹ năng
        /// </summary>
        public Entities.Config.SkillDataEx Data
        {
            get
            {
                return this._Data;
            }
            set
            {
                this._Data = value;
                this.UIImage_SkillIcon.BundleDir = value.IconBundleDir;
                this.UIImage_SkillIcon.AtlasName = value.IconAtlasName;
                this.UIImage_SkillIcon.SpriteName = value.Icon;
                this.UIImage_SkillIcon.Load();
            }
        }

        /// <summary>
        /// Sự kiện Click vào ô kỹ năng
        /// </summary>
        public Action Click { get; set; }
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi ở Frame đầu tiên
        /// </summary>
        private void Start()
        {
            this.InitPrefab();
        }
        #endregion

        #region Code UI
        /// <summary>
        /// Khởi tạo ban đầu
        /// </summary>
        private void InitPrefab()
        {
            this.UIButton.onClick.AddListener(this.ButtonSkill_Clicked);
        }

        /// <summary>
        /// Sự kiện khi Button kỹ năng được ấn
        /// </summary>
        private void ButtonSkill_Clicked()
        {
            this.Click?.Invoke();
        }
        #endregion
    }
}
