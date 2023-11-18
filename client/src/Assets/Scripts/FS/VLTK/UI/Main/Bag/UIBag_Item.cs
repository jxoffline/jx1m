using FS.VLTK.Entities.Config;
using FS.VLTK.UI.Main.ItemBox;
using Server.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FS.VLTK.UI.Main.Bag
{
    /// <summary>
    /// Ô vật phẩm trong túi đồ
    /// </summary>
    public class UIBag_Item : MonoBehaviour
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

        #region Private fields
        /// <summary>
        /// ID vật phẩm lần trước
        /// </summary>
        private int lastItemID = -1;

        /// <summary>
        /// Số lượng vật phẩm lần trước
        /// </summary>
        private int lastItemCount = -1;
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
                if (value == null)
				{
                    /// Đánh dấu ID vật phẩm và số lượng lần trước
                    this.lastItemID = -1;
                    this.lastItemCount = -1;
                    /// Bỏ qua
                    return;
				}

                /// Nếu trùng ID nhưng số lượng bị giảm tức là nó dùng vật phẩm
                if (this.lastItemID != -1 && this.lastItemID == value.GoodsID && this.lastItemCount > value.GCount)
                {
                    /// Thông tin vật phẩm tương ứng
                    if (Loader.Loader.Items.TryGetValue(this.lastItemID, out ItemData itemData))
                    {
                        /// Nếu vật phẩm này là thuốc hoặc vật phẩm có Script
                        if (itemData.IsMedicine || itemData.IsScriptItem)
                        {
                            this.UIItemBox.PlayUseItemSuccessfullyEffect();
                        }
                    }
                }

                /// Đánh dấu ID vật phẩm và số lượng lần trước
                this.lastItemID = value.GoodsID;
                this.lastItemCount = value.GCount;
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
    }
}
