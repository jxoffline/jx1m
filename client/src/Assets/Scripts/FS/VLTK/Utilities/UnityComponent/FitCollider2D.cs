using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace FS.VLTK.Utilities.UnityComponent
{
    /// <summary>
    /// Tự điều chỉnh Box Collider 2D khớp với kích thước Sprite trong SpriteRenderer
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer), typeof(BoxCollider2D))]
    public class FitCollider2D : TTMonoBehaviour
    {
        /// <summary>
        /// Tọa độ vị trí trung tâm
        /// </summary>
        public Vector2 Pivot = new Vector2(0.5f, 0.5f);

        /// <summary>
        /// Thuộc tính BoxCollider của đối tượng
        /// </summary>
        private new BoxCollider2D collider;

        /// <summary>
        /// Thuộc tính SpriteRenderer của đối tượng
        /// </summary>
        private new SpriteRenderer renderer;

        /// <summary>
        /// Sự kiện xảy ra khi đối tượng được chọn bởi người dùng
        /// </summary>
        public Action OnClick { get; set; } = null;

        /// <summary>
        /// Hàm này gọi đến khi đối tượng được kích hoạt
        /// </summary>
        private void Start()
        {
            this.collider = this.gameObject.GetComponent<BoxCollider2D>();
            this.renderer = this.gameObject.GetComponent<SpriteRenderer>();
        }

        /// <summary>
        /// Hàm này gọi liên tục mỗi Frame
        /// </summary>
        private void Update()
        {
            this.collider.size = this.renderer.size;
            this.collider.offset = new Vector2(this.renderer.size.x * this.Pivot.x, this.renderer.size.y * this.Pivot.y);
        }

        /// <summary>
        /// Hàm này gọi đến khi đối tượng được click chọn
        /// </summary>
        private void OnMouseUpAsButton()
        {
            if (!FS.VLTK.Utils.IsClickOnGUI())
            {
                this.OnClick?.Invoke();
            }
        }
    }
}
