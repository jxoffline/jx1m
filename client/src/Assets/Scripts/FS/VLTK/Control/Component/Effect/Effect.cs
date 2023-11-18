using FS.VLTK.Entities.Config;
using FS.VLTK.Entities.Object;
using FS.VLTK.Logic.Settings;
using FS.VLTK.Utilities.UnityComponent;
using System;
using UnityEngine;
using static FS.VLTK.Entities.Enum;

namespace FS.VLTK.Control.Component
{
    /// <summary>
    /// Đối tượng hiệu ứng
    /// </summary>
    public partial class Effect : TTMonoBehaviour
    {
        #region Define
        /// <summary>
        /// Dữ liệu hiệu ứng
        /// </summary>
        public StateEffectXML Data { get; set; } = new StateEffectXML();

        /// <summary>
        /// Sự kiện khi đối tượng bị xóa
        /// </summary>
        public Action Destroyed { get; set; }

        /// <summary>
        /// Độ trong suốt của đối tượng
        /// </summary>
        public float Alpha
        {
            get
            {
                return this.animation.Body.color.a;
            }
            set
            {
                Color color = this.animation.Body.color;
                color.a = value;
                this.animation.Body.color = color;
            }
        }

        private GameObject _Owner;
        /// <summary>
        /// Chủ nhân của hiệu ứng, sẽ đi theo đối tượng chủ nhân
        /// </summary>
        public GameObject Owner
        {
            get
            {
                return this._Owner;
            }
            set
            {
                this._Owner = value;

                if (value.GetComponent<Character>() != null)
                {
                    this.ownerPlayer = value.GetComponent<Character>();
                }
                else if (value.GetComponent<Monster>() != null)
                {
                    this.ownerMonster = value.GetComponent<Monster>();
                }
            }
        }

        /// <summary>
        /// Loại hiệu ứng
        /// </summary>
        public EffectType Type { get; set; } = EffectType.None;
        #endregion

        /// <summary>
        /// Thành phần GroupColor của đối tượng
        /// </summary>
        private GroupColor groupColor;

        /// <summary>
        /// Chủ nhân nếu là người chơi
        /// </summary>
        private Character ownerPlayer = null;
        /// <summary>
        /// Chủ nhân nếu là quái
        /// </summary>
        private Monster ownerMonster = null;

        /// <summary>
        /// Đánh dấu vừa thực hiện hàm Awake
        /// </summary>
        private bool justAwaken = false;

        /// <summary>
        /// Đánh dấu hiển thị nhân vật lần trước
        /// </summary>
        private bool lastHideRole = false;

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
            this.animation = this.gameObject.GetComponent<EffectAnimation2D>();
            this.animation.Body = this.Body.GetComponent<SpriteRenderer>();
            this.groupColor = this.gameObject.GetComponent<GroupColor>();
            this.justAwaken = true;
        }

        /// <summary>
        /// Hàm này gọi ở Frame đầu tiên
        /// </summary>
        private void Start()
        {
            this.InitAction();
            this.justAwaken = false;
        }

        /// <summary>
        /// Hàm này gọi liên tục mỗi Frame
        /// </summary>
        private void Update()
        {
            if (this.Owner != null)
            {
                this.gameObject.transform.position = this.Owner.transform.position;
            }
        }

        /// <summary>
        /// Hàm này gọi khi đối tượng được kích hoạt
        /// </summary>
        private void OnEnable()
        {
            this.StartSynsCoroutines();
        }

        /// <summary>
        /// Hàm này gọi trước khi đối tượng bị xóa
        /// </summary>
        private void OnDisable()
        {
            if (this.justAwaken)
            {
                this.justAwaken = false;
                return;
            }

            this.Data = null;
        }
        #endregion
    }
}
