using FS.GameEngine.Sprite;
using UnityEngine;

namespace FS.VLTK.Control.Component
{
    /// <summary>
    /// Đối tượng đánh dấu mục tiêu đang được chọn
    /// </summary>
    public class SelectTargetDeco : TTMonoBehaviour
    {
        #region Define
#if UNITY_EDITOR
        /// <summary>
        /// Đối tượng đang theo dõi (dùng trong Debug ở Editor)
        /// </summary>
        [SerializeField]
        private GameObject _Target;
#endif
        #endregion

        #region Properties
        private GSprite _TargetSprite;
        /// <summary>
        /// Đối tượng tham chiếu
        /// </summary>
        public GSprite TargetSprite
        {
            get
            {
                return this._TargetSprite;
            }
            set
            {
                this._TargetSprite = value;

                if (value == null)
                {
                    this.transform.localPosition = Vector2.zero;
                }
            }
        }
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này chạy liên tục mỗi Frame
        /// </summary>
        private void Update()
        {
            if (this.TargetSprite != null && this.TargetSprite.Role2D)
            {
                this.gameObject.transform.position = (Vector2) this.TargetSprite.Role2D.transform.position;

#if UNITY_EDITOR
                this._Target = this.TargetSprite.Role2D;
#endif
            }
        }
        #endregion
    }
}