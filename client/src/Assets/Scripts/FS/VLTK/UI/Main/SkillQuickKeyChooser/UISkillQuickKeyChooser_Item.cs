using FS.VLTK.Utilities.UnityUI;
using System;
using TMPro;
using UnityEngine;

namespace FS.VLTK.UI.Main.SkillQuickKeyChooser
{
    /// <summary>
    /// Khung chọn kỹ năng và thiết lập vào danh sách kỹ năng nhanh
    /// </summary>
    public class UISkillQuickKeyChooser_Item : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Button
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton;

        /// <summary>
        /// Ảnh Icon của kỹ năng
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
        /// <summary>
        /// Đường dẫn Bundle chứa Icon
        /// </summary>
        public string IconBundleDir
        {
            get
            {
                return this.UIImage_Icon.BundleDir;
            }
            set
            {
                this.UIImage_Icon.BundleDir = value;
            }
        }

        /// <summary>
        /// Tên Atlas chứa Icon
        /// </summary>
        public string IconAtlasName
        {
            get
            {
                return this.UIImage_Icon.AtlasName;
            }
            set
            {
                this.UIImage_Icon.AtlasName = value;
            }
        }

        /// <summary>
        /// Tên Sprite Icon
        /// </summary>
        public string IconSpriteName
        {
            get
            {
                return this.UIImage_Icon.SpriteName;
            }
            set
            {
                this.UIImage_Icon.SpriteName = value;
            }
        }

        /// <summary>
        /// Tên kỹ năng
        /// </summary>
        public string SkillName
        {
            get
            {
                return this.UIText_Name.text;
            }
            set
            {
                this.UIText_Name.text = value;
            }
        }

        /// <summary>
        /// Sự kiện khi đối tượng được Click
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
        /// Sự kiện khi Button được Click
        /// </summary>
        private void Button_Clicked()
        {
            this.Click?.Invoke();
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Đọc và tải ảnh Icon lên
        /// </summary>
        public void Load()
        {
            this.UIImage_Icon.Load();
        }
        #endregion
    }
}

