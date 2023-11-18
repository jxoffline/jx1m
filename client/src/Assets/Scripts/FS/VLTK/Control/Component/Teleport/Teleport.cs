using FS.GameEngine.Logic;
using FS.VLTK.UI;
using UnityEngine;
using System.Collections;
using FS.VLTK.Utilities.UnityComponent;
using System;

namespace FS.VLTK.Control.Component
{
    /// <summary>
    /// Đối tượng điểm truyền tống
    /// </summary>
    public partial class Teleport : TTMonoBehaviour
    {
        #region Properties
        /// <summary>
        /// Dữ liệu điểm truyền tống
        /// </summary>
        public Entities.Object.TeleportData Data { get; private set; } = new Entities.Object.TeleportData();

        /// <summary>
        /// Kích thước icon minimap
        /// </summary>
        public Vector2 MinimapIconSize { get; set; }

        /// <summary>
        /// Sự kiện khi đối tượng bị xóa
        /// </summary>
        public Action Destroyed { get; set; }
        #endregion

        /// <summary>
        /// Group Transparency
        /// </summary>
        private GroupColor groupColor;

        /// <summary>
        /// Đã chạy qua hàm Start chưa
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
        /// Hàm này gọi đến khi đối tượng được tạo ra
        /// </summary>
        private void Awake()
        {
            this.groupColor = this.gameObject.GetComponent<GroupColor>();
            this.animation = this.gameObject.GetComponent<MonsterAnimation2D>();
            this.animation.Body = this.Body.GetComponent<SpriteRenderer>();

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
        /// Hàm này gọi đến khi đối tượng vào trạng thái Active
        /// </summary>
        private void OnEnable()
        {
            this.StopAllCoroutines();
            this.StartSynsCoroutines();
            if (this.isStarted)
            {
                this.StartCoroutine(this.ExecuteSkipFrames(1, () => {
                    this.DisplayUI();

                    if (this.UIMinimapReference != null)
                    {
                        this.UIMinimapReference.gameObject.SetActive(true);
                    }
                }));
                this.InitAction();
            }
            this.InitEvents();
        }


        /// <summary>
        /// Hàm này gọi đến khi đối tượng vào trạng thái Inactive
        /// </summary>
        private void OnDisable()
        {
            if (this.justAwaken)
            {
                this.justAwaken = false;
                return;
            }

            if (this.UIMinimapReference != null)
            {
                this.UIMinimapReference.gameObject.SetActive(false);
            }
        }
    }
}
