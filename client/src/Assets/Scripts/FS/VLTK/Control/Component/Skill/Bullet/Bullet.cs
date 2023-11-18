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
using static FS.VLTK.Entities.Enum;

namespace FS.VLTK.Control.Component.Skill
{
    /// <summary>
    /// Đạn bay
    /// </summary>
    public partial class Bullet : TTMonoBehaviour
    {
        #region Properties
        /// <summary>
        /// Trạng thái đạn
        /// </summary>
        public enum BulletState
        {
            /// <summary>
            /// Không có
            /// </summary>
            None = -1,
            /// <summary>
            /// Đang bay
            /// </summary>
            Flying = 0,
            /// <summary>
            /// Đang tan biến
            /// </summary>
            FadingOut = 1,
        }

        /// <summary>
        /// Trạng thái của đạn
        /// </summary>
        public BulletState State { get; private set; } = BulletState.None;

        /// <summary>
        /// ID đạn bay
        /// </summary>
        public int ResID { get; set; }

        /// <summary>
        /// Thời điểm tạo ra
        /// </summary>
        public long CreateTick { get; set; }

        /// <summary>
        /// Tọa độ vị trí bắt đầu
        /// </summary>
        public Vector2 FromPos { get; set; }

        /// <summary>
        /// Tọa độ vị trí tới
        /// </summary>
        public Vector2 ToPos { get; set; }

        /// <summary>
        /// Mục tiêu nhắm tới (nếu là đạn đuổi mục tiêu)
        /// </summary>
        public GameObject ChaseTarget { get; set; }

        /// <summary>
        /// Theo đối tượng ra chiêu không
        /// </summary>
        public bool CircleFollowCaster { get; set; }

        /// <summary>
        /// Bán kính di chuyển theo đường tròn
        /// </summary>
        public float CircleMoveRadius { get; set; }

        /// <summary>
        /// Vector chỉ hướng di chuyển theo đường tròn
        /// </summary>
        public Vector2 CircleDirVector { get; set; }

        /// <summary>
        /// Vận tốc bay
        /// </summary>
        public float Velocity { get; set; }

        /// <summary>
        /// Thời gian tồn tại tối đa (sau khoảng này đạn sẽ tự bị xóa)
        /// </summary>
        public float MaxLifeTime { get; set; } = 20f;

        /// <summary>
        /// Thời gian một vòng hiệu ứng
        /// </summary>
        public float AnimationLifeTime { get; set; } = 0.5f;

        /// <summary>
        /// Hướng quay hiện tại (8 hướng)
        /// </summary>
        public Direction Direction8 { get; private set; } = Entities.Enum.Direction.NONE;

        /// <summary>
        /// Hướng quay hiện tại (16 hướng)
        /// </summary>
        public Direction16 Direction { get; private set; } = Direction16.None;

        /// <summary>
        /// Hướng quay hiện tại (32 hướng)
        /// </summary>
        public Direction32 Direction32 { get; private set; } = Direction32.None;

        /// <summary>
        /// Hiệu ứng lặp đi lặp lại
        /// </summary>
        public bool RepeatAnimation { get; set; }

        /// <summary>
        /// Thời gian delay trước khi thực hiện hiệu ứng
        /// </summary>
        public float Delay { get; set; }

        /// <summary>
        /// Có phải kỹ năng ném không
        /// </summary>
        public bool IsThrowing { get; set; }

        /// <summary>
        /// Sự kiện khi đạn bị xóa
        /// </summary>
        public Action Destroyed { get; set; }

        /// <summary>
        /// Đối tượng xuất chiêu
        /// </summary>
        public GSprite Caster { get; set; }

        /// <summary>
        /// Có phải bẫy không
        /// </summary>
        public bool IsTrap { get; set; } = false;

        /// <summary>
        /// Quay trở lại vị trí ban đầu không
        /// </summary>
        public bool Comeback { get; set; } = false;

        /// <summary>
        /// Bay từ vị trí đối tượng ra chiêu đến vị trí bắt đầu bay (chỉ áp dụng với loại đạn bay từ A đến B)
        /// </summary>
        public bool FlyToStartPosFirst { get; set; } = false;
        #endregion

        /// <summary>
        /// Đã chạy qua Delay chưa
        /// </summary>
        private bool isReady = false;

        /// <summary>
        /// Group Transparency
        /// </summary>
        private GroupColor groupColor;

        /// <summary>
        /// Mới thực hiện hàm Awake
        /// </summary>
        private bool justAwaked = false;

        /// <summary>
        /// Thực hiện Delay nếu có
        /// </summary>
        private void StartDelay()
        {
            if (this.Delay <= 0)
            {
                this.isReady = true;
                return;
            }

            this.StartCoroutine(this.DelayTask(this.Delay, () => {
                this.isReady = true;
            }));
        }

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
            this.animation = this.gameObject.GetComponent<BulletAnimation2D>();
            this.animation.Body = this.Body.GetComponent<SpriteRenderer>();
            this.Trail_Body = this.Body.GetComponent<SpriteTrailRenderer>();

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
