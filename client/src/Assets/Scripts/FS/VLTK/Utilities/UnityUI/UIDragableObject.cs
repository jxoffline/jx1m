using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace FS.VLTK.Utilities.UnityUI
{
    /// <summary>
    /// Panel có thể kéo qua kéo lại
    /// </summary>
    public class UIDragableObject : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
    {
        #region Define
        /// <summary>
        /// Đối tượng tương tác
        /// </summary>
        [SerializeField]
        private RectTransform UITransform_Frame;

        /// <summary>
        /// Ghi nhận và thiết lập vị trí mới sau khi kéo thả
        /// </summary>
        [SerializeField]
        private bool _CommitDragPos = true;
        #endregion

        #region Private fileds
        /// <summary>
        /// Vị trí con trỏ trước
        /// </summary>
        private Vector2 lastMousePosition;

        /// <summary>
        /// Vị trí ban đầu
        /// </summary>
        private Vector2 initPosition;
        #endregion

        #region Properties
        /// <summary>
        /// Ghi nhận và thiết lập vị trí mới sau khi kéo thả
        /// </summary>
        public bool CommitDragPos
        {
            get
            {
                return this._CommitDragPos;
            }
            set
            {
                this._CommitDragPos = value;
            }
        }

        /// <summary>
        /// Sự kiện khi bắt đầu kéo thả. Vị trí đầu ra là tọa độ màn hình
        /// </summary>
        public Action<Vector2> DragStart { get; set; }

        /// <summary>
        /// Sự kiện kéo thả liên tục. Vị trí đầu ra là tọa độ màn hình
        /// </summary>
        public Action<Vector2> Drag { get; set; }

        /// <summary>
        /// Sự kiện khi kết thúc kéo thả. Vị trí đầu ra là tọa độ màn hình
        /// </summary>
        public Action<Vector2> DragFinish { get; set; }
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi khi đối tượng được tạo ra
        /// </summary>
        private void Awake()
        {
            this.initPosition = this.UITransform_Frame.anchoredPosition;
        }
        #endregion

        #region Core
        /// <summary>
        /// Hàm này kiểm tra đối tượng cửa sổ có nằm trong màn hình không
        /// </summary>
        /// <param name="rectTransform"></param>
        /// <returns></returns>
        private bool IsRectTransformInsideSreen(RectTransform rectTransform)
        {
            bool isInside = false;
            Vector3[] corners = new Vector3[4];
            rectTransform.GetWorldCorners(corners);
            int visibleCorners = 0;
            Rect rect = new Rect(0, 0, Screen.width, Screen.height);
            foreach (Vector3 corner in corners)
            {
                if (rect.Contains(corner))
                {
                    visibleCorners++;
                }
            }
            if (visibleCorners == 4)
            {
                isInside = true;
            }
            return isInside;
        }

        /// <summary>
        /// Hàm này gọi đến trong suốt quá trình di chuyển con trỏ
        /// </summary>
        /// <param name="eventData"></param>
        public void OnDrag(PointerEventData eventData)
        {
            Vector2 currentMousePosition = eventData.position;
            Vector2 diff = currentMousePosition - lastMousePosition;
            RectTransform rect = this.UITransform_Frame;

            Vector3 newPosition = rect.position + new Vector3(diff.x, diff.y, transform.position.z);
            Vector3 oldPos = rect.position;
            rect.position = newPosition;
            if (!this.IsRectTransformInsideSreen(rect))
            {
                rect.position = oldPos;
            }
            this.lastMousePosition = currentMousePosition;

            /// Thực thi sự kiện
            this.Drag?.Invoke(eventData.position);
        }

        /// <summary>
        /// Hàm này gọi đến khi bắt đầu sự kiện kéo thả
        /// </summary>
        /// <param name="eventData"></param>
        public void OnBeginDrag(PointerEventData eventData)
        {
            /// Thực thi sự kiện
            this.DragStart?.Invoke(eventData.position);
            /// Thiết lập vị trí lần trước
            this.lastMousePosition = eventData.position;
        }

        /// <summary>
        /// Hàm này gọi đến khi kết thúc quá trình kéo thả
        /// </summary>
        /// <param name="eventData"></param>
        public void OnEndDrag(PointerEventData eventData)
        {
            /// Thực thi sự kiện
            this.DragFinish?.Invoke(eventData.position);

            /// Nếu không ghi nhận vị trí kéo thả
            if (!this._CommitDragPos)
            {
                /// Thiết lập lại vị trí ban đầu
                this.UITransform_Frame.anchoredPosition = this.initPosition;
            }
        }
        #endregion
    }
}
