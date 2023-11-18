using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using FS.VLTK.UI.Main.ItemBox;
using Server.Data;

namespace FS.VLTK.UI.Main.Crafting
{
    /// <summary>
    /// Sản phẩm chế tạo
    /// </summary>
    public class UICrafting_ProductItem : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Ô vật phẩm
        /// </summary>
        [SerializeField]
        private UIItemBox UIItemBox;

        /// <summary>
        /// Text tên vật phẩm
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_ItemName;

        /// <summary>
        /// Tỷ lệ % sản phẩm
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_ProductRate;
        #endregion

        #region Properties
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

                /// Nếu có dữ liệu
                if (value != null)
                {
                    this.UIText_ItemName.text = KTGlobal.GetItemName(value, false);
                    this.UIText_ItemName.color = KTGlobal.GetItemColor(value);
                }
                else
                {
                    this.UIText_ItemName.text = "";
                }
            }
        }

        private int _Rate = 0;
        /// <summary>
        /// Tỷ lệ đạt được
        /// </summary>
        public int Rate
        {
            get
            {
                return this._Rate;
            }
            set
            {
                this._Rate = value;
                this.UIText_ProductRate.text = string.Format("{0}%", value);
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
