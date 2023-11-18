using FS.VLTK.Entities.Object;
using FS.VLTK.UI.UICore;
using TMPro;
using UnityEngine;
using static FS.VLTK.Entities.Enum;

namespace FS.VLTK.UI.CoreUI
{
    /// <summary>
    /// Lớp quản lý text trên thông tin nhân vật
    /// </summary>
    public class UIMonsterHeader : TTMonoBehaviour
    {
        #region Define
        /// <summary>
        /// Thanh máu
        /// </summary>
        [SerializeField]
        private UISpriteHPBar UISprite_HPBar;

        /// <summary>
        /// Ảnh ngũ hành
        /// </summary>
        [SerializeField]
        private SpriteRenderer UIImage_Series;

        /// <summary>
        /// Text tên quái
        /// </summary>
        [SerializeField]
        private TextMeshPro UIText_Name;

        /// <summary>
        /// Text danh hiệu
        /// </summary>
        [SerializeField]
        private TextMeshPro UIText_Title;

        /// <summary>
        /// Kích thước Icon ngũ hành
        /// </summary>
        [SerializeField]
        private Vector2 SeriesSize;
        #endregion

        #region Properties
        /// <summary>
        /// Vị trí đặt Y
        /// </summary>
        public float OffsetY
        {
            get
            {
                return this.transform.localPosition.y;
            }
            set
            {
                Vector2 oldPos = this.transform.localPosition;
                this.transform.localPosition = new Vector2(oldPos.x, value);
            }
        }

        private bool _IsBoss = false;
        /// <summary>
        /// Có phải Boss không
        /// </summary>
        public bool IsBoss
        {
            get
            {
                return this._IsBoss;
            }
            set
            {
                this._IsBoss = value;

                /// Nếu không hiện thanh máu
                if (!this.ShowHPBar)
                {
                    this.UISprite_HPBar.gameObject.SetActive(false);
                    return;
                }

                this.UISprite_HPBar.gameObject.SetActive(true);

                /// Nếu là Boss
                if (value)
                {
                    /// Thay đổi kích thuóc
                    this.UISprite_HPBar.BackgroundSize = new Vector2(50, 6);
                    this.UISprite_HPBar.ThumbSize = new Vector2(50, 6);
                }
                /// Nếu không phải Boss
                else
                {
                    /// Thay đổi kích thuóc
                    this.UISprite_HPBar.BackgroundSize = new Vector2(50, 4);
                    this.UISprite_HPBar.ThumbSize = new Vector2(50, 4);
                }
            }
        }

        /// <summary>
        /// Màu của thanh máu
        /// </summary>
        public Color HPBarColor
        {
            get
            {
                return this.UISprite_HPBar.Color;
            }
            set
            {
                this.UISprite_HPBar.Color = value;
            }
        }

        /// <summary>
        /// Màu của tên
        /// </summary>
        public Color NameColor
        {
            get
            {
                return this.UIText_Name.color;
            }
            set
            {
                this.UIText_Name.color = value;
            }
        }

        private int _HPPercent = 0;
        /// <summary>
        /// % máu
        /// </summary>
        public int HPPercent
        {
            get
            {
                return this._HPPercent;
            }
            set
            {
                this.UISprite_HPBar.Value = value / 100f;
            }
        }

        /// <summary>
        /// Tên đối tượng
        /// </summary>
        public string Name
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

        private string _Title = "";
        /// <summary>
        /// Danh hiệu
        /// </summary>
        public string Title
        {
            get
            {
                return this._Title;
            }
            set
            {
                this._Title = value;

                if (string.IsNullOrEmpty(value))
                {
                    this.UIText_Title.gameObject.SetActive(false);
                }
                else
                {
                    this.UIText_Title.gameObject.SetActive(true);
                    this.UIText_Title.text = value;
                }
            }
        }

        /// <summary>
        /// Ngũ hành
        /// </summary>
        public Elemental Series
        {
            set
            {
                if (Loader.Loader.Elements.TryGetValue(value, out ElementData elementDetail))
                {
                    this.UIImage_Series.sprite = elementDetail.SmallSprite;
                    this.UIImage_Series.drawMode = SpriteDrawMode.Sliced;
                    this.UIImage_Series.size = this.SeriesSize;
                }
                
            }
        }

        private bool _ShowHPBar = false;
        /// <summary>
        /// Hiển thị thanh máu
        /// </summary>
        public bool ShowHPBar
        {
            get
            {
                return this._ShowHPBar;
            }
            set
            {
                this._ShowHPBar = value;
                this.UISprite_HPBar.gameObject.SetActive(value);
            }
        }

        private bool _ShowElemental = true;
        /// <summary>
        /// Hiển thị ngũ hành
        /// </summary>
        public bool ShowElemental
        {
            get
            {
                return this._ShowElemental;
            }
            set
            {
                this._ShowElemental = value;
                this.UIImage_Series.gameObject.SetActive(value);
            }
        }

        /// <summary>
        /// Hiển thị thanh máu không (dùng trong thiết lập hiển thị hệ thống)
        /// </summary>
        public bool SystemSettingShowHPBar
        {
            get
            {
                return this.UISprite_HPBar.gameObject.activeSelf;
            }
            set
            {
                this.UISprite_HPBar.gameObject.SetActive(value && this._ShowHPBar);
            }
        }

        /// <summary>
        /// Có hiển thị tên, danh hiệu không
        /// </summary>
        public bool SystemSettingShowName
        {
            get
            {
                return this.UIText_Name.gameObject.activeSelf;
            }
            set
            {
                this.UIText_Name.gameObject.SetActive(value);
                if (!string.IsNullOrEmpty(this.Title))
                {
                    this.UIText_Title.gameObject.SetActive(value);
                }
                if (this._ShowElemental)
                {
                    this.UIImage_Series.gameObject.SetActive(value);
                }
            }
        }
        #endregion

        #region Private fields
        /// <summary>
        /// Màu ban đầu của thanh HP Bar
        /// </summary>
        private Color initHPBarColor;

        /// <summary>
        /// Màu ban đầu của thanh HP Bar
        /// </summary>
        private Color initNameColor;

        /// <summary>
        /// Vị trí đặt ban đầu
        /// </summary>
        private Vector2 initOffset;
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi đến khi đối tượng được tạo ra
        /// </summary>
        private void Awake()
        {
            this.initHPBarColor = this.UISprite_HPBar.Color;
            this.initNameColor = this.UIText_Name.color;
            this.initOffset = this.transform.localPosition;
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Hủy đối tượng
        /// </summary>
        public void Destroy()
        {
            this.StopAllCoroutines();
            this.NameColor = default;
            this.HPPercent = 0;
            this.IsBoss = false;
            this.Name = "";
            this.Title = "";
            this.Series = default;
            this.OffsetY = this.initOffset.y;
            this.ShowElemental = false;
            this.SystemSettingShowName = true;
            this.SystemSettingShowHPBar = true;
            this._ShowElemental = true;
            this.RestoreColor();
        }

        /// <summary>
        /// Chuyển màu tên, thanh máu về như ban đầu
        /// </summary>
        public void RestoreColor()
        {
            this.HPBarColor = this.initHPBarColor;
            this.NameColor = this.initNameColor;
        }
        #endregion
    }
}
