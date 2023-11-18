using TMPro;
using UnityEngine;

namespace FS.VLTK.UI.UICore
{
    /// <summary>
    /// Thanh tên vật phẩm
    /// </summary>
    public class UIItemHeader : TTMonoBehaviour
    {
        #region Define
        /// <summary>
        /// Text tên vật phẩm
        /// </summary>
        [SerializeField]
        private TextMeshPro UIText_Name;
        #endregion

        #region Properties
        /// <summary>
        /// Tên vật phẩm
        /// </summary>
        public string Name
        {
            get
            {
                return this.UIText_Name.text;
            }
            set
            {
                this.UIText_Name.text = value;
            }
        }

        /// <summary>
        /// Màu tên vật phẩm
        /// </summary>
        public Color NameColor
        {
            get
            {
                return this.UIText_Name.color;
            }
            set
            {
                this.UIText_Name.color = value;
            }
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Hủy đối tượng
        /// </summary>
        public void Destroy()
        {
            this.StopAllCoroutines();
            this.Name = "";
            this.NameColor = default;
        }
        #endregion
    }
}
