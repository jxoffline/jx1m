using FS.GameEngine.Logic;
using FS.VLTK.Factory;
using FS.VLTK.UI;
using FS.VLTK.UI.UICore;
using Server.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace FS.VLTK.Control.Component
{
    /// <summary>
    /// Hiển thị UI
    /// </summary>
    public partial class Item : IDisplayUI
    {
        #region Define
        /// <summary>
        /// Khung trên đầu vật phẩm
        /// </summary>
        [SerializeField]
        private UIItemHeader UIHeader;
        #endregion

        /// <summary>
        /// Xóa UI
        /// </summary>
        public void DestroyUI()
        {
            this.UIHeader.Destroy();
        }

        /// <summary>
        /// Hiện UI
        /// </summary>
        public void DisplayUI()
        {
            GoodsData itemGD = KTGlobal.CreateItemPreview(this.ItemData);
            itemGD.Forge_level = this.RefObject.EnhanceLevel;
            this.UIHeader.Name = KTGlobal.GetItemName(itemGD);
            /// Nếu số lượng > 1
            if (this._Data.GCount > 1)
            {
                this.UIHeader.Name += string.Format(" - SL: {0}", this._Data.GCount);
            }
            this.UIHeader.NameColor = this._NameColor;
        }
    }
}
