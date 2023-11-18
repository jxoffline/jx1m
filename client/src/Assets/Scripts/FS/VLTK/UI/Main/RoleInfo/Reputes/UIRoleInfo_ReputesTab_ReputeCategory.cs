using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace FS.VLTK.UI.Main.RoleInfo
{
    /// <summary>
    /// Đối tượng Mục danh vọng, bên trong gồm các con tương ứng
    /// </summary>
    public class UIRoleInfo_ReputesTab_ReputeCategory : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Text tên mục danh vọng
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_Name;
        #endregion

        #region Properties
        /// <summary>
        /// Tên mục danh vọng
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
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi ở Frame đầu tiên
        /// </summary>
        private void Start()
        {
            this.InitPrefabs();
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

        #region Private methods

        #endregion

        #region Public methods

        #endregion
    }
}
