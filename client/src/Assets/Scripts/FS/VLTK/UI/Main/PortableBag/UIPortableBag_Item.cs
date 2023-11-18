using FS.VLTK.UI.Main.ItemBox;
using Server.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FS.VLTK.UI.Main.PortableBag
{
    /// <summary>
    /// Ô vật phẩm trong túi đồ thương khố
    /// </summary>
    public class UIPortableBag_Item : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Ô vật phẩm
        /// </summary>
        [SerializeField]
        private UIItemBox UIItemBox;

        /// <summary>
        /// Icon khóa
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Image LockIcon;
        #endregion

        #region Properties
        /// <summary>
        /// Sự kiện khóa ô vật phẩm
        /// </summary>
        public bool Locked
        {
            get
            {
                return this.LockIcon.gameObject.activeSelf;
            }
            set
            {
                this.LockIcon.gameObject.SetActive(value);
            }
        }

        /// <summary>
        /// Dữ liệu vật phẩm
        /// </summary>
        public GoodsData Data
        {
            get
            {
                return this.UIItemBox.Data;
            }
            set
            {
                this.UIItemBox.Data = value;
            }
        }

        /// <summary>
        /// Sự kiện Click
        /// </summary>
        public Action Click
        {
            get
            {
                return this.UIItemBox.Click;
            }
            set
            {
                this.UIItemBox.Click = value;
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
    }
}
