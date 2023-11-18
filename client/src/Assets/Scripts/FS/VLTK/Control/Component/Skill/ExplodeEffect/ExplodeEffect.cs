using FS.GameEngine.Logic;
using FS.GameEngine.Sprite;
using FS.VLTK.Utilities.UnityComponent;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace FS.VLTK.Control.Component.Skill
{
    /// <summary>
    /// Hiệu ứng đạn nổ
    /// </summary>
    public partial class ExplodeEffect : TTMonoBehaviour
    {
        #region Properties
        /// <summary>
        /// Thời điểm khởi tạo
        /// </summary>
        public long CreateTick { get; set; }

        /// <summary>
        /// ID đạn bay
        /// </summary>
        public int ResID { get; set; }

        /// <summary>
        /// Vị trí xuất hiện
        /// </summary>
        public Vector2 Position { get; set; }

        /// <summary>
        /// Mục tiêu
        /// <para>Nếu giá trị này tồn tại thì sẽ không lấy giá trị Position thiết lập đầu vào</para>
        /// </summary>
        public GameObject Target { get; set; }

        /// <summary>
        /// Thời gian Delay trước khi thực hiện
        /// </summary>
        public float Delay { get; set; }

        /// <summary>
        /// Sự kiện khi đối tượng bị hủy
        /// </summary>
        public Action Destroyed { get; set; }
        #endregion

        /// <summary>
        /// Group Transparency
        /// </summary>
        private GroupColor groupColor;

        /// <summary>
        /// Đã thực hiện xong pha Delay chưa
        /// </summary>
        private bool isReady = false;

        /// <summary>
        /// Mới thực hiện hàm Awake
        /// </summary>
        private bool justAwaked = false;

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

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi đến khi đối tượng được tạo ra
        /// </summary>
        private void Awake()
        {
            this.groupColor = this.gameObject.GetComponent<GroupColor>();
            this.animation = this.gameObject.GetComponent<BulletExplodeEffectAnimation2D>();
            this.animation.Body = this.Body.GetComponent<SpriteRenderer>();

            this.justAwaked = true;
        }

        /// <summary>
        /// Hàm này gọi ở Frame đầu tiên
        /// </summary>
        private void Start()
        {
            this.justAwaked = false;
        }

        /// <summary>
        /// Hàm này gọi liên tục mỗi frame
        /// </summary>
        private void Update()
        {
            /// Nếu có mục tiêu thì Follow theo mục tiêu luôn
            if (this.Target != null)
            {
                this.transform.localPosition = this.Target.transform.localPosition;
            }
        }

        /// <summary>
        /// Hàm này gọi khi đối tượng được kích hoạt
        /// </summary>
        private void OnEnable()
        {
            /// Nếu ở pha đầu tiên tạo ở Pool
            if (this.justAwaked)
            {
                return;
            }

            this.InitActions();
            this.InitEvents();
            this.StartSynsCoroutines();
            this.isReady = false;
        }

        /// <summary>
        /// Hàm này gọi khi đối tượng bị hủy trạng thái kích hoạt
        /// </summary>
        private void OnDisable()
        {
            this.justAwaked = false;
        }
        #endregion
    }
}
