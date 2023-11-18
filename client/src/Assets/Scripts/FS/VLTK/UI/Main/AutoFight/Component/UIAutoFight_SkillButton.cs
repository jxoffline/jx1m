using FS.VLTK.Entities.Config;
using FS.VLTK.Utilities.UnityUI;
using System;
using UnityEngine;

namespace FS.VLTK.UI.Main.AutoFight
{
    /// <summary>
    /// Ô kỹ năng trong khung thiết lập tự đánh
    /// </summary>
    public class UIAutoFight_SkillButton : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Button kỹ năng
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton;

        /// <summary>
        /// Icon kỹ năng
        /// </summary>
        [SerializeField]
        private SpriteFromAssetBundle UISprite_Icon;

        /// <summary>
        /// Ảnh mũi tên
        /// </summary>
        [SerializeField]
        private SpriteFromAssetBundle UIImage_Arrow;
        #endregion

        #region Properties
        /// <summary>
        /// Vị trí
        /// </summary>
        public int Slot { get; set; }

        private SkillDataEx _Data = null;
        /// <summary>
        /// Dữ liệu kỹ năng
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

                /// Nếu không có dữ liệu
                if (value == null)
                {
                    this.UISprite_Icon.gameObject.SetActive(false);
                }
                else
                {
                    this.UISprite_Icon.gameObject.SetActive(true);

                    this.UISprite_Icon.BundleDir = value.IconBundleDir;
                    this.UISprite_Icon.AtlasName = value.IconAtlasName;
                    this.UISprite_Icon.SpriteName = value.Icon;
                    this.UISprite_Icon.Load();
                }
            }
        }

        /// <summary>
        /// Sự kiện Click
        /// </summary>
        public Action Click { get; set; }

        /// <summary>
        /// Hiện mũi tên không
        /// </summary>
        public bool ShowArrow
        {
            get
            {
                return this.UIImage_Arrow.gameObject.activeSelf;
            }
            set
            {
                this.UIImage_Arrow.gameObject.SetActive(value);
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
            this.UIButton.onClick.AddListener(this.Button_Clicked);
        }

        /// <summary>
        /// Sự kiện khi Button được Click
        /// </summary>
        private void Button_Clicked()
        {
            this.Click?.Invoke();
        }
        #endregion

        #region Private methods

        #endregion
    }
}
