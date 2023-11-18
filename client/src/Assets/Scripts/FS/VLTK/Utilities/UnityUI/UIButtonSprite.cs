using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace FS.VLTK.Utilities.UnityUI
{
    /// <summary>
    /// Đối tượng Button với 2 trạng thái thường và Disable, tương đương 2 loại Sprite
    /// </summary>
    public class UIButtonSprite : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Thành phần Button
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton;

        /// <summary>
        /// Text
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText;

        /// <summary>
        /// Màu chữ
        /// </summary>
        [SerializeField]
        private Color _TextColor;

        /// <summary>
        /// Sử dụng màu chữ mặc định ban đầu
        /// </summary>
        [SerializeField]
        private bool UseOriginColor = true;

        /// <summary>
        /// Thành phần Sprite
        /// </summary>
        [SerializeField]
        private SpriteFromAssetBundle UISprite;

        /// <summary>
        /// Đường dẫn Bundle chứa ảnh
        /// </summary>
        [SerializeField]
        private string _BundleDir;

        /// <summary>
        /// Đường dẫn Atlas chứa ảnh
        /// </summary>
        [SerializeField]
        private string _AtlasName;

        /// <summary>
        /// Tên ảnh ở trạng thái thường
        /// </summary>
        [SerializeField]
        private string _NormalSpriteName;

        /// <summary>
        /// Tên ảnh ở trạng thái Disable
        /// </summary>
        [SerializeField]
        private string _DisabledSpriteName;
        #endregion

        #region Properties
        /// <summary>
        /// Tên Button
        /// </summary>
        public string Name
        {
            get
            {
                if (this.UIText != null)
                {
                    return this.UIText.text;
                }
                return "";
            }
            set
            {
                if (this.UIText != null)
                {
                    this.UIText.text = value;
                }
            }
        }

        /// <summary>
        /// Màu chữ
        /// </summary>
        public Color TextColor
        {
            get
            {
                if (this.UIText != null)
                {
                    return this.UIText.color;
                }
                return default;
            }
            set
            {
                if (this.UIText != null)
                {
                    this.UIText.color = value;
                }
            }
        }

        /// <summary>
        /// Đường dẫn Bundle chứa ảnh
        /// </summary>
        public string BundleDir
        {
            get
            {
                return this._BundleDir;
            }
            set
            {
                this._BundleDir = value;
            }
        }

        /// <summary>
        /// Tên file Atlas
        /// </summary>
        public string AtlasName
        {
            get
            {
                return this._AtlasName;
            }
            set
            {
                this._AtlasName = value;
            }
        }

        /// <summary>
        /// Tên ảnh ở trạng thái thường
        /// </summary>
        public string NormalSpriteName
        {
            get
            {
                return this._NormalSpriteName;
            }
            set
            {
                this._NormalSpriteName = value;
            }
        }

        /// <summary>
        /// Tên ảnh ở trạng thái Disable
        /// </summary>
        public string DisabledSpriteName
        {
            get
            {
                return this._DisabledSpriteName;
            }
            set
            {
                this._DisabledSpriteName = value;
            }
        }

        /// <summary>
        /// Màu ảnh
        /// </summary>
        public Color Color
        {
            get
            {
                /// Toác
                if (this.componentImage == null)
                {
                    return Color.white;
                }
                return this.componentImage.color;
            }
            set
            {
                /// Toác
                if (this.componentImage == null)
                {
                    return;
                }
                this.componentImage.color = value;
            }
        }

        /// <summary>
        /// Sự kiện khi trạng thái của đối tượng thay đổi
        /// </summary>
        public Action<bool> OnInteractionChanged { get; set; }

        /// <summary>
        /// Sự kiện khi đối tượng được Click
        /// </summary>
        public Action Click { get; set; }

        /// <summary>
        /// Kích hoạt đối tượng
        /// </summary>
        public bool Enable
        {
            get
            {
                return this.UIButton.interactable;
            }
            set
            {
                this.UIButton.interactable = value;
            }
        }
        #endregion

        #region Private fields
        /// <summary>
        /// Trạng thái hiện tại
        /// </summary>
        private bool isEnable = true;

        /// <summary>
        /// Thành phần Image
        /// </summary>
        private UnityEngine.UI.Image componentImage = null;
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi khi đối tượng được tạo ra
        /// </summary>
        private void Awake()
        {
            if (this.UISprite != null)
            {
                this.componentImage = this.UISprite.GetComponent<UnityEngine.UI.Image>();
            }
        }

        /// <summary>
        /// Hàm này gọi ở Frame đầu tiên
        /// </summary>
        private void Start()
        {
            this.InitPrefabs();
        }

        /// <summary>
        /// Hàm này gọi liên tục mỗi Frame
        /// </summary>
        private void Update()
        {
            this.Refresh();
        }
        #endregion

        #region Code UI
        /// <summary>
        /// Khởi tạo ban đàu
        /// </summary>
        private void InitPrefabs()
        {
            this.Refresh(true);
            this.UIButton.onClick.AddListener(this.Button_Clicked);

            if (!this.UseOriginColor && this.UIText != null)
            {
                this.UIText.color = this._TextColor;
            }
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
        /// <summary>
        /// Làm mới đối tượng
        /// </summary>
        public void Refresh(bool isIgnoreCurrentState = false)
        {
            if (isIgnoreCurrentState || this.UIButton.interactable != this.isEnable)
            {
                this.isEnable = this.UIButton.interactable;

                if (this.UISprite != null)
                {
                    this.UISprite.BundleDir = this._BundleDir;
                    this.UISprite.AtlasName = this._AtlasName;
                    if (this.UIButton.interactable)
                    {
                        this.UISprite.SpriteName = this._NormalSpriteName;
                    }
                    else
                    {
                        this.UISprite.SpriteName = this._DisabledSpriteName;
                    }
                    this.UISprite.Load();
                }

                this.OnInteractionChanged?.Invoke(this.UIButton.interactable);
            }
        }
        #endregion
    }
}
