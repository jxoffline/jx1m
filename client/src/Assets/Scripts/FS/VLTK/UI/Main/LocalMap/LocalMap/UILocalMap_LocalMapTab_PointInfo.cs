using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace FS.VLTK.UI.Main.LocalMap
{
    /// <summary>
    /// Thông tin NPC vị trí ở bản đồ khu vực
    /// </summary>
    public class UILocalMap_LocalMapTab_PointInfo : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Tên điểm
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_Name;

        /// <summary>
        /// Tag
        /// </summary>
        [SerializeField]
        private string _Tag;
        #endregion

        #region Properties
        /// <summary>
        /// Tên điểm
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
        /// Tag
        /// </summary>
        public string Tag
        {
            get
            {
                return this._Tag;
            }
        }
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi ở Frame đầu tiên
        /// </summary>
        private void Start()
        {

        }
        #endregion
    }
}