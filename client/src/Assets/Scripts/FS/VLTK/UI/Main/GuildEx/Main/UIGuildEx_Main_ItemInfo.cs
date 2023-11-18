using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using FS.VLTK.UI.Main.ItemBox;
using Server.Data;

namespace FS.VLTK.UI.Main.GuildEx.Main
{
    /// <summary>
    /// Thông tin vật phẩm tích lũy bang trong khung bang hội tổng quan
    /// </summary>
    public class UIGuildEx_Main_ItemInfo : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Ô vật phẩm
        /// </summary>
        [SerializeField]
        private UIItemBox UIItemBox;

        /// <summary>
        /// Text số lượng vật phẩm
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_Quantity;
        #endregion

        #region Properties
        private int _ItemID = -1;
        /// <summary>
        /// ID vật phẩm
        /// </summary>
        public int ItemID
        {
            get
            {
                return this._ItemID;
            }
            set
            {
                this._ItemID = value;
                /// Nếu vật phẩm tồn tại
                if (Loader.Loader.Items.TryGetValue(value, out Entities.Config.ItemData itemData))
                {
                    /// Tạo vật phẩm ảo
                    GoodsData itemGD = KTGlobal.CreateItemPreview(itemData);
                    /// Gắn vào ô
                    this.UIItemBox.Data = itemGD;
                    this.UIItemBox.Refresh();
                }
            }
        }

        private int _Quantity;
        /// <summary>
        /// Số lượng
        /// </summary>
        public int Quantity
        {
            get
            {
                return this._Quantity;
            }
            set
            {
                this._Quantity = value;
                /// Thiết lập số lượng
                this.UIText_Quantity.text = value.ToString();
            }
        }
        #endregion
    }
}
