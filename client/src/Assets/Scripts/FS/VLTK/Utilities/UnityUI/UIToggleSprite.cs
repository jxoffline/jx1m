using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

namespace FS.VLTK.Utilities.UnityUI
{
    /// <summary>
    /// Toggle
    /// </summary>
    public class UIToggleSprite : MonoBehaviour
    {
        #region Defines
        /// <summary>
        /// Toggle
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Toggle UIToggle;

        /// <summary>
        /// Ảnh
        /// </summary>
        [SerializeField]
        private SpriteFromAssetBundle UIImage_Background;

        /// <summary>
        /// Tên đối tượng
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_Name;

        /// <summary>
        /// Nhóm Toggle
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.ToggleGroup UIToggle_Group;

        /// <summary>
        /// Màu chữ ở trạng thái thường
        /// </summary>
        [SerializeField]
        private Color _NormalColor;

        /// <summary>
        /// Màu chữ ở trạng thái kích hoạt
        /// </summary>
        [SerializeField]
        private Color _ActiveColor;

        /// <summary>
        /// Sử dụng màu thiết lập mặc định của chữ
        /// </summary>
        [SerializeField]
        public bool _UseOriginColor = false;

        /// <summary>
        /// Đường dẫn Bundle chứa ảnh
        /// </summary>
        [SerializeField]
        private string _BundleDir;

        /// <summary>
        /// Atlas chứa ảnh
        /// </summary>
        [SerializeField]
        private string _AtlasName;

        /// <summary>
        /// Tên ảnh ở trạng thái thường
        /// </summary>
        [SerializeField]
        private string _NormalSprite;

        /// <summary>
        /// Tên ảnh ở trạng thái kích hoạt
        /// </summary>
        [SerializeField]
        private string _ActiveSprite;
        #endregion

        #region Properties
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
        /// Atlas chứa ảnh
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
        public string NormalSprite
        {
            get
            {
                return this._NormalSprite;
            }
            set
            {
                this._NormalSprite = value;
            }
        }

        /// <summary>
        /// Tên ảnh ở trạng thái kích hoạt
        /// </summary>
        public string ActiveSprite
        {
            get
            {
                return this._ActiveSprite;
            }
            set
            {
                this._ActiveSprite = value;
            }
        }

        /// <summary>
        /// Tên
        /// </summary>
        public string Name
        {
            get
            {
                if (this.UIText_Name != null)
                {
                    return this.UIText_Name.text;
                }
                return "";
            }
            set
            {
                if (this.UIText_Name != null)
                {
                    this.UIText_Name.text = value;
                }
            }
        }

        /// <summary>
        /// Màu tên đối tượng
        /// </summary>
        public Color NameColor
        {
            get
            {
                if (this.UIText_Name != null)
                {
                    return this.UIText_Name.color;
                }
                return default;
            }
            set
            {
                if (this.UIText_Name != null)
                {
                    this.UIText_Name.color = value;
                }
            }
        }

        /// <summary>
        /// Màu chữ ở trạng thái thường
        /// </summary>
        public Color NormalColor
        {
            get
            {
                return this._NormalColor;
            }
            set
            {
                this._NormalColor = value;
            }
        }

        /// <summary>
        /// Màu chữ ở trạng thái kích hoạt
        /// </summary>
        public Color ActiveColor
        {
            get
            {
                return this._ActiveColor;
            }
            set
            {
                this._ActiveColor = value;
            }
        }

        /// <summary>
        /// Nhóm
        /// </summary>
        public UnityEngine.UI.ToggleGroup Group
        {
            get
            {
                return this.UIToggle_Group;
            }
            set
            {
                this.UIToggle_Group = value;
                this.UIToggle.group = value;
            }
        }

        /// <summary>
        /// Kích hoạt
        /// </summary>
        public bool Active
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

        /// <summary>
        /// Hiện Background
        /// </summary>
        public bool ShowBackground
        {
            get
            {
                /// Nếu không tồn tại
                if (this.UIImage_Background == null)
                {
                    /// Bỏ qua
                    return false;
                }
                return this.UIImage_Background.gameObject.activeSelf;
            }
            set
            {
                /// Nếu không tồn tại
                if (this.UIImage_Background == null)
                {
                    /// Bỏ qua
                    return;
                }
                this.UIImage_Background.gameObject.SetActive(value);
            }
        }

        /// <summary>
        /// Sự kiện khi đối tượng được chọn
        /// </summary>
        public Action<bool> OnSelected { get; set; }

        /// <summary>
        /// Kích hoạt đối tượng để có thể chủ động được tương tác bởi người dùng
        /// </summary>
        public bool Enable
        {
            get
            {
                return this.UIToggle.interactable;
            }
            set
            {
                this.UIToggle.interactable = value;
            }
        }

        /// <summary>
        /// Tag
        /// </summary>
        public object Tag { get; set; }
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
            if (this.UIToggle == null)
            {
                return;
            }

            /// Nếu kích hoạt thì thực thi sự kiện luôn
            if (this.UIToggle.isOn)
            {
                /// Thực thi sự kiện ở Frame sau
                this.StartCoroutine(this.ExecuteSkipFrames(1, () =>
                {
                    this.OnSelected?.Invoke(true);
                }));
            }

            this.UIToggle.onValueChanged.AddListener((isSelected) => {
                this.RefreshToggleVisible();
                this.OnSelected?.Invoke(isSelected);
            });
            this.UIToggle.group = this.UIToggle_Group;
            this.RefreshToggleVisible();
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Thực thi sự kiện bỏ qua một số Frame
        /// </summary>
        /// <param name="skip"></param>
        /// <param name="work"></param>
        /// <returns></returns>
        private IEnumerator ExecuteSkipFrames(int skip, Action work)
        {
            for (int i = 1; i <= skip; i++)
            {
                yield return null;
            }
            work?.Invoke();
        }

        /// <summary>
        /// Làm mới hiển thị Toggle
        /// </summary>
        private void RefreshToggleVisible()
        {
            /// Nếu không tồn tại
            if (this.UIImage_Background == null)
            {
                /// Bỏ qua
                return;
            }

            this.UIImage_Background.BundleDir = this._BundleDir;
            this.UIImage_Background.AtlasName = this._AtlasName;
            if (this.UIToggle.isOn)
            {
                this.UIImage_Background.SpriteName = this._ActiveSprite;

                if (this.UIText_Name != null && !this._UseOriginColor)
                {
                    this.UIText_Name.color = this._ActiveColor;
                }
            }
            else
            {
                this.UIImage_Background.SpriteName = this._NormalSprite;

                if (this.UIText_Name != null && !this._UseOriginColor)
                {
                    this.UIText_Name.color = this._NormalColor;
                }
            }
            this.UIImage_Background.Load();
        }
        #endregion
    }
}