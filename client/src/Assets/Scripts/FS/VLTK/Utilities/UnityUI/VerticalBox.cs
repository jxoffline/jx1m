using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace FS.VLTK.Utilities.UnityUI
{
    /// <summary>
    /// Xếp chồng đối tượng theo chiều dọc
    /// </summary>
    [ExecuteAlways]
    public class VerticalBox : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Khoảng trống
        /// </summary>
        [SerializeField]
        private float _Space = 0;

        /// <summary>
        /// Đảo ngược thứ tự
        /// </summary>
        [SerializeField]
        private bool _Reverse = false;

#if UNITY_EDITOR
        /// <summary>
        /// Xây lại ngay lập tức
        /// </summary>
        [SerializeField]
        private bool _RebuildNow = false;
#endif
        #endregion

        #region Private fields
        /// <summary>
        /// Đã chạy qua hàm Start chưa
        /// </summary>
        private bool isStarted = false;
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi ở Frame đầu tiên
        /// </summary>
        private void Start()
        {
            /// Xây lại
            this.Rebuild();
            /// Đánh dấu đã chạy qua hàm Start
            this.isStarted = true;
        }

        /// <summary>
        /// Hàm này gọi khi đối tượng được kích hoạt
        /// </summary>
        private void OnEnable()
        {
            /// Nếu chưa chạy qua hàm Start
            if (!this.isStarted)
            {
                /// Bỏ qua
                return;
            }
            /// Xây lại
            this.Rebuild();
        }

#if UNITY_EDITOR
        /// <summary>
        /// Hàm này gọi liên tục mỗi Frame
        /// </summary>
        private void Update()
        {
            /// Nếu có yêu cầu xây lại ngay lập tức
            if (this._RebuildNow)
            {
                /// Hủy yêu cầu
                this._RebuildNow = false;

                /// Thực hiện
                this.Rebuild();
            }
        }
#endif
        #endregion

        #region Public methods
        /// <summary>
        /// Xếp lại thứ tự
        /// </summary>
        public void Rebuild()
        {
            /// Tạo mới
            List<GameObject> objects = new List<GameObject>();
            /// Duyệt danh sách con
            foreach (Transform child in this.transform)
            {
                /// Nếu đang hiển thị
                if (child.gameObject.activeSelf)
                {
                    objects.Add(child.gameObject);
                }
            }

            /// Nếu chiều từ trên xuống
            if (!this._Reverse)
            {
                /// Vị trí Y
                float dy = 0;
                /// Duyệt danh sách theo chiều ngược
                for (int i = objects.Count - 1; i >= 0; i--)
                {
                    /// Thành phần SpriteRenderer
                    SpriteRenderer spriteRenderer = objects[i].GetComponent<SpriteRenderer>();
                    /// Thành phần TextMeshPro
                    TextMeshPro tmp = objects[i].GetComponent<TextMeshPro>();

                    /// Nếu có thành phần SpriteRenderer
                    if (spriteRenderer != null && spriteRenderer.sprite != null)
                    {
                        /// Vị trí cũ
                        Vector3 oldPos = objects[i].transform.localPosition;
                        /// Cập nhật vị trí
                        objects[i].transform.localPosition = new Vector3(oldPos.x, dy + spriteRenderer.sprite.pivot.y, oldPos.z);
                        /// Tăng vị trí lên
                        dy += spriteRenderer.size.y + spriteRenderer.sprite.pivot.y;
                    }
                    /// Nếu có thành phần TextMeshPro
                    else if (tmp != null)
                    {
                        /// Vị trí cũ
                        Vector3 oldPos = objects[i].transform.localPosition;
                        /// Cập nhật vị trí
                        objects[i].transform.localPosition = new Vector3(oldPos.x, dy, oldPos.z);
                        /// Tăng vị trí lên
                        dy += tmp.GetComponent<RectTransform>().sizeDelta.y;
                    }

                    /// Tăng kèm khoảng trống
                    dy += this._Space;
                }
            }
            /// Nếu chiều từ dưới lên
            else
            {
                /// Vị trí Y
                float dy = 0;
                /// Duyệt danh sách theo chiều xuôi
                for (int i = 0; i < objects.Count; i++)
                {
                    /// Thành phần SpriteRenderer
                    SpriteRenderer spriteRenderer = objects[i].GetComponent<SpriteRenderer>();
                    /// Thành phần TextMeshPro
                    TextMeshPro tmp = objects[i].GetComponent<TextMeshPro>();

                    /// Nếu có thành phần SpriteRenderer
                    if (spriteRenderer != null && spriteRenderer.sprite != null)
                    {
                        /// Vị trí cũ
                        Vector3 oldPos = objects[i].transform.localPosition;
                        /// Cập nhật vị trí
                        objects[i].transform.localPosition = new Vector3(oldPos.x, dy + spriteRenderer.sprite.pivot.y, oldPos.z);
                        /// Tăng vị trí lên
                        dy += spriteRenderer.size.y + spriteRenderer.sprite.pivot.y;
                    }
                    /// Nếu có thành phần TextMeshPro
                    else if (tmp != null)
                    {
                        /// Vị trí cũ
                        Vector3 oldPos = objects[i].transform.localPosition;
                        /// Cập nhật vị trí
                        objects[i].transform.localPosition = new Vector3(oldPos.x, dy, oldPos.z);
                        /// Tăng vị trí lên
                        dy += tmp.GetComponent<RectTransform>().sizeDelta.y;
                    }

                    /// Tăng kèm khoảng trống
                    dy += this._Space;
                }
            }
        }
        #endregion
    }
}
