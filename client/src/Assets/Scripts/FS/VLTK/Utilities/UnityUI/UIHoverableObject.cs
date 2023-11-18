using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace FS.VLTK.Utilities.UnityUI
{
    /// <summary>
    /// Đối tượng giao diện cung cấp sự kiện Hover
    /// </summary>
    public class UIHoverableObject : TTMonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        #region Define
        /// <summary>
        /// Tổng thời gian giữ để được tính là bắt đầu sự kiện Hover
        /// </summary>
        [SerializeField]
        private float _HoverDuration = 1f;

        /// <summary>
        /// Liên tục không
        /// </summary>
        [SerializeField]
        private bool _Continuously = false;

        /// <summary>
        /// Tick kiểm tra thực thi hàm Tick nếu sự kiện Hover liên tục
        /// </summary>
        [SerializeField]
        private float _HoverTick = 0.2f;
        #endregion

        #region Private fields
        /// <summary>
        /// Có đang được giữ chọn không
        /// </summary>
        private bool mouseDown = false;

        /// <summary>
        /// Tổng thời gian đã giữ
        /// </summary>
        private float hoverTime = 0;

        /// <summary>
        /// Tổng thời gian giữa các lần Tick
        /// </summary>
        private float hoverTickTime = 0;

        /// <summary>
        /// Thành phần UIButton
        /// </summary>
        private UnityEngine.UI.Button componentButton = null;

        /// <summary>
        /// Thành phần UIToggle
        /// </summary>
        private UnityEngine.UI.Toggle componentToggle = null;

        /// <summary>
        /// Có đang giữ không
        /// </summary>
        private bool isHovering = false;
        #endregion

        #region Properties
        /// <summary>
        /// Tổng thời gian giữ để được tính là bắt đầu sự kiện Hover
        /// </summary>
        public float HoverDuration
        {
            get
            {
                return this._HoverDuration;
            }
            set
            {
                this._HoverDuration = value;
            }
        }

        /// <summary>
        /// Liên tục không
        /// </summary>
        public bool Continuously
        {
            get
            {
                return this._Continuously;
            }
            set
            {
                this._Continuously = value;
            }
        }

        /// <summary>
        /// Tick kiểm tra thực thi hàm Tick nếu sự kiện Hover liên tục
        /// </summary>
        public float HoverTick
        {
            get
            {
                return this._HoverTick;
            }
            set
            {
                this._HoverTick = value;
            }
        }

        /// <summary>
        /// Sự kiện khi bắt đầu Hover
        /// </summary>
        public Action Hover { get; set; }

        /// <summary>
        /// Sự kiện Tick nếu sự kiện Hover là liên tục
        /// </summary>
        public Action Tick { get; set; }

        /// <summary>
        /// Sự kiện khi kết thúc Hover
        /// </summary>
        public Action HoverEnd { get; set; }

        /// <summary>
        /// Sự kiện Click (nếu không có Hover)
        /// </summary>
        public Action Click { get; set; }

        private bool _Interactable = true;
        /// <summary>
        /// Có thể tương tác được với đối tượng không
        /// </summary>
        public bool Interactable
        {
            get
            {
                return this._Interactable;
            }
            set
            {
                this._Interactable = value;

                if (this.componentButton != null)
                {
                    this.componentButton.interactable = value;
                }
                if (this.componentToggle != null)
                {
                    this.componentToggle.interactable = value;
                }
            }
        }
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi khi đối tượng được tạo ra
        /// </summary>
        private void Awake()
        {
            this.componentButton = this.GetComponent<UnityEngine.UI.Button>();
            this.componentToggle = this.GetComponent<UnityEngine.UI.Toggle>();
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
            /// Nếu không tương tác được
            if (!this.Interactable)
            {
                return;
            }

            if (this.mouseDown)
            {
                this.hoverTime += Time.deltaTime;
                if (this.hoverTime >= this._HoverDuration)
                {
                    this.isHovering = true;
                    this.Hover?.Invoke();
                    /// Nếu không liên tục
                    if (!this._Continuously)
                    {
                        this.hoverTime = 0;
                        this.mouseDown = false;
                        this.HoverEnd?.Invoke();
                    }
                    /// Nếu liên tục
                    else
                    {
                        this.hoverTickTime += Time.deltaTime;
                        if (this.hoverTickTime >= this._HoverTick)
                        {
                            this.Tick?.Invoke();
                            this.hoverTickTime = 0;
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Bắt sự kiện Click vào đối tượng
        /// </summary>
        /// <param name="pointerEventData"></param>
        public void OnPointerDown(PointerEventData pointerEventData)
        {
            /// Nếu không tương tác được
            if (!this.Interactable)
            {
                return;
            }

            this.hoverTime = 0;
            this.hoverTickTime = 0;
            this.mouseDown = true;
            this.isHovering = false;
        }

        /// <summary>
        /// Bắt sự kiện khi ngừng Click đối tượng
        /// </summary>
        /// <param name="pointerEventData"></param>
        public void OnPointerUp(PointerEventData pointerEventData)
        {
            /// Nếu không tương tác được
            if (!this.Interactable)
            {
                return;
            }

            this.hoverTime = 0;
            this.hoverTickTime = 0;
            this.mouseDown = false;
            /// Nếu có Hover
            if (this.isHovering)
            {
                this.HoverEnd?.Invoke();
            }
            else
            {
                this.Click?.Invoke();
            }
            this.isHovering = false;
        }
        #endregion

        #region Code UI
        /// <summary>
        /// Khởi tạo ban đầu
        /// </summary>
        private void InitPrefabs()
        {

        }
        #endregion

        #region Public methods
        /// <summary>
        /// Ngừng Hover
        /// </summary>
        public void Stop()
        {
            /// Nếu không tương tác được
            if (!this.Interactable)
            {
                return;
            }

            this.hoverTime = 0;
            this.hoverTickTime = 0;
            this.mouseDown = false;
            /// Nếu có Hover
            if (this.isHovering)
            {
                this.HoverEnd?.Invoke();
            }
            else
            {
                this.Click?.Invoke();
            }
            this.isHovering = false;
        }
        #endregion
    }
}
