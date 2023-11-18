using System;
using UnityEngine;

namespace FS.VLTK.Utilities.UnityComponent
{
    /// <summary>
    /// Component Collider cung cấp hàm CallBack khi Click vào
    /// </summary>
    [RequireComponent(typeof(BoxCollider2D))]
    public class ClickableCollider2D : TTMonoBehaviour
    {
        /// <summary>
        /// Sự kiện xảy ra khi đối tượng được chọn bởi người dùng
        /// </summary>
        public Action OnClick { get; set; } = null;

        /// <summary>
        /// Hàm này gọi đến khi đối tượng được kích hoạt
        /// </summary>
        private void Start()
        {

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
