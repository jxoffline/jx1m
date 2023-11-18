using FS.GameEngine.Logic;
using FS.GameEngine.Sprite;
using FS.VLTK.Entities.Object;
using FS.VLTK.Logic.Settings;
using FS.VLTK.UI;
using FS.VLTK.Utilities.UnityComponent;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static FS.VLTK.Entities.Enum;

namespace FS.VLTK.Control.Component
{
    /// <summary>
    /// Đối tượng quái vật
    /// </summary>
    public partial class Monster : TTMonoBehaviour
    {
        #region Properties
        /// <summary>
        /// Đối tượng tham chiếu
        /// </summary>
        public GSprite RefObject { get; set; }

        /// <summary>
        /// Res quái
        /// </summary>
        public string ResID { get; set; }

        /// <summary>
        /// ID trong file cấu hình
        /// </summary>
        public int StaticID { get; set; }

        private bool _ShowHPBar;
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
                this.UIHeader.ShowHPBar = value;
            }
        }

        private bool _ShowElemental;
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
                this.UIHeader.ShowElemental = value;
            }
        }

        private bool _ShowMinimapName;
        /// <summary>
        /// Hiện tên ở bản đồ thu nhỏ
        /// </summary>
        public bool ShowMinimapName
        {
            get
            {
                return this._ShowMinimapName;
            }
            set
            {
                this._ShowMinimapName = value;

                if (this.UIMinimapReference != null)
                {
                    this.UIMinimapReference.ShowName = value;
                }
            }
        }

        private bool _ShowMinimapIcon;
        /// <summary>
        /// Hiển thị Icon ở bản đồ nỏ
        /// </summary>
        public bool ShowMinimapIcon
        {
            get
            {
                return this._ShowMinimapIcon;
            }
            set
            {
                this._ShowMinimapIcon = value;

                if (this.UIMinimapReference != null)
                {
                    this.UIMinimapReference.ShowIcon = value;
                }
            }
        }

        private Color _NameColor;
        /// <summary>
        /// Màu chữ tên đối tượng
        /// </summary>
        public Color NameColor
        {
            get
            {
                return this._NameColor;
            }
            set
            {
                this._NameColor = value;
                this.UIHeader.NameColor = value;
            }
        }

        private Color _MinimapNameColor;
        /// <summary>
        /// Màu của tên đối tượng trong bản đồ thu nhỏ
        /// </summary>
        public Color MinimapNameColor
        {
            get
            {
                return this._MinimapNameColor;
            }
            set
            {
                this._MinimapNameColor = value;

                if (this.UIMinimapReference != null)
                {
                    this.UIMinimapReference.NameColor = value;
                }
            }
        }

        private Vector2 _MinimapIconSize;
        /// <summary>
        /// Kích thước ảnh ở bản đồ thu nhỏ
        /// </summary>
        public Vector2 MinimapIconSize
        {
            get
            {
                return this._MinimapIconSize;
            }
            set
            {
                this._MinimapIconSize = value;

                if (this.UIMinimapReference != null)
                {
                    this.UIMinimapReference.IconSize = value;
                }
            }
        }

        private Direction _Direction = Direction.NONE;
        /// <summary>
        /// Hướng quay hiện tại
        /// </summary>
        public Direction Direction
        {
            get
            {
                return this._Direction;
            }

            set
            {
                if (this._Direction == value)
                {
                    return;
                }
                else
                {
                    this._Direction = value;
                    /// Tiếp tục động tác hiện tại
                    this.ResumeCurrentAction();
                }

            }
        }

        /// <summary>
        /// Độ trong suốt hiện tại của đối tượng
        /// </summary>
        public float CurrentAlpha
        {
            get
            {
                if (this.groupColor != null)
                {
                    return this.groupColor.Alpha;
                }
                else
                {
                    return this.MaxAlpha;
                }
            }
        }

        /// <summary>
        /// Sự kiện khi đối tượng bị xóa
        /// </summary>
        public Action Destroyed { get; set; }

        /// <summary>
        /// Độ trong suốt cực đại
        /// </summary>
        public float MaxAlpha { get; set; } = 1f;
        #endregion

        /// <summary>
        /// Đã chạy hàm Start chưa
        /// </summary>
        private bool isStarted = false;

        /// <summary>
        /// Đánh dấu mới thực hiện hàm Awake
        /// </summary>
        private bool justAwaken = false;

        /// <summary>
        /// Thực thi các luồng đồng bộ dữ liệu
        /// </summary>
        private void StartSynsCoroutines()
        {
            if (!this.gameObject.activeSelf)
            {
                return;
            }
            this.StartCoroutine(this.ExecuteBackgroundWork());
        }

        /// <summary>
        /// Group Transparency
        /// </summary>
        private GroupColor groupColor;

        /// <summary>
        /// Đối tượng phát âm thanh
        /// </summary>
        private AudioPlayer audioPlayer;

        /// <summary>
        /// Hàm này gọi đến khi đối tượng được tạo ra
        /// </summary>
        private void Awake()
        {
            this.groupColor = this.gameObject.GetComponent<GroupColor>();
            this.animation = this.gameObject.GetComponent<MonsterAnimation2D>();
            this.animation.Body = this.Body.GetComponent<SpriteRenderer>();
            this.Trail_Body = this.Body.GetComponent<SpriteTrailRenderer>();

            this.audioPlayer = this.gameObject.GetComponent<AudioPlayer>();

            this.justAwaken = true;
        }

        /// <summary>
        /// Hàm này gọi ở Frame đầu tiên
        /// </summary>
        private void Start()
        {
            this.InitAction();
            this.isStarted = true;

            this.DisplayUI();
            this.UpdateHP();

            this.justAwaken = false;
        }

        /// <summary>
        /// Hàm này gọi đến khi đối tượng được kích hoạt
        /// </summary>
        private void OnEnable()
        {
            if (this.justAwaken)
            {
                this.justAwaken = false;
                return;
            }

            this.StartSynsCoroutines();
            if (this.isStarted)
            {
                this.StartCoroutine(this.ExecuteSkipFrames(1, () => {
                    this.DisplayUI();
                    this.UpdateHP();
                }));
                this.InitAction();
            }
            this.InitEvents();
        }

        /// <summary>
        /// Hàm này gọi đến khi đối tượng bị hủy
        /// </summary>
        private void OnDisable()
        {
            if (this.justAwaken)
            {
                this.justAwaken = false;
                return;
            }
        }
    }
}
