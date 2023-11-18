using FS.GameEngine.GoodsPack;
using FS.GameEngine.Logic;
using FS.VLTK.Entities.Config;
using FS.VLTK.Logic.Settings;
using FS.VLTK.Utilities.UnityComponent;
using FS.VLTK.Utilities.UnityUI;
using Server.Data;
using System;
using System.Collections;
using UnityEngine;

namespace FS.VLTK.Control.Component
{
    /// <summary>
    /// Đối tượng vật phẩm rơi ở bản đồ
    /// </summary>
    public partial class Item : TTMonoBehaviour
    {
        #region Define
        /// <summary>
        /// Cấu hình vật phẩm
        /// </summary>
        private ItemData ItemData { get; set; }

        private GoodsData _Data;
        /// <summary>
        /// Dữ liệu vật phẩm
        /// </summary>
        public GoodsData Data
        {
            get
            {
                return this._Data;
            }
            set
            {
                this._Data = value;

                if (Loader.Loader.Items.TryGetValue(value.GoodsID, out ItemData itemData))
                {
                    this.ItemData = itemData;
                }
                else
                {
                    this.ItemData = null;
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

        private Color _NameColor;
        /// <summary>
        /// Màu chữ tên vật phẩm
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

        /// <summary>
        /// Đối tượng ở Map tương ứng
        /// </summary>
        public GGoodsPack RefObject { get; set; }
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

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi khi đối tượng được tạo ra
        /// </summary>
        private void Awake()
        {
            this.groupColor = this.gameObject.GetComponent<GroupColor>();
            this.animation = this.gameObject.GetComponent<DropItemAnimation2D>();
            this.animation.Body = this.Body.GetComponent<SpriteFromAssetBundle>();

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
        #endregion
    }
}
