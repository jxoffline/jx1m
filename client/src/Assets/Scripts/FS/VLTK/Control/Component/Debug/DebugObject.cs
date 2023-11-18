using System;
using System.Collections;
using UnityEngine;

namespace FS.VLTK.Control.Component
{
    /// <summary>
    /// Đối tượng khối Debug Object
    /// </summary>
    public class DebugObject : TTMonoBehaviour
    {
        #region Define
        /// <summary>
        /// Đối tượng Renderer
        /// </summary>
        [SerializeField]
        private SpriteRenderer _Renderer;

        /// <summary>
        /// Vị trí
        /// </summary>
        [SerializeField]
        private Vector2 _Pos;

        /// <summary>
        /// Kích thước khối
        /// </summary>
        [SerializeField]
        private int _Size;

        /// <summary>
        /// Thời gian tồn tại
        /// </summary>
        [SerializeField]
        private float _LifeTime;
        #endregion

        #region Properties
        /// <summary>
        /// Vị trí xuất hiện
        /// </summary>
        public Vector2 Pos
        {
            get
            {
                return this._Pos;
            }
            set
            {
                this._Pos = value;
                this.gameObject.transform.localPosition = new Vector3(value.x, value.y, value.y / 10000);
            }
        }

        /// <summary>
        /// Kích thước khối
        /// </summary>
        public int Size
        {
            get
            {
                return this._Size;
            }
            set
            {
                this._Size = value;
                this._Renderer.size = new Vector2(value, value);
            }
        }

        /// <summary>
        /// Thời gian tồn tại
        /// </summary>
        public float LifeTime
        {
            get
            {
                return this._LifeTime;
            }
            set
            {
                this._LifeTime = value;
            }
        }
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi ở Frame đầu tiên
        /// </summary>
        private void Start()
        {
            IEnumerator AutoDestroy()
            {
                yield return new WaitForSeconds(this._LifeTime);
                GameObject.Destroy(this.gameObject);
            }
            this.StartCoroutine(AutoDestroy());
            this.gameObject.transform.localPosition = new Vector3(this._Pos.x, this._Pos.y, this._Pos.y / 10000);
            this._Renderer.size = new Vector2(this._Size, this._Size);
        }

        /// <summary>
        /// Hàm này gọi liên tục mỗi Frame
        /// </summary>
        private void Update()
        {
            
        }
        #endregion
    }
}
