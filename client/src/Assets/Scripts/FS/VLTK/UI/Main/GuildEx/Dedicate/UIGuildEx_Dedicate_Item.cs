using UnityEngine;
using TMPro;
using FS.VLTK.UI.Main.ItemBox;
using Server.Data;

namespace FS.VLTK.UI.Main.GuildEx.Dedicate
{
    /// <summary>
    /// Ô vật phẩm trong khung cống hiến bang hội
    /// </summary>
    public class UIGuildEx_Dedicate_Item : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Ô vật phẩm
        /// </summary>
        [SerializeField]
        private UIItemBox UIItemBox;

        /// <summary>
        /// Tên vật phẩm
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_ItemName;

        /// <summary>
        /// Text số lượng
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

                /// Thông tin vật phẩm
                if (!Loader.Loader.Items.TryGetValue(value, out Entities.Config.ItemData itemData))
                {
                    /// Không tồn tại thì bỏ qua
                    return;
                }

                /// Vật phẩm ảo
                GoodsData itemGD = KTGlobal.CreateItemPreview(itemData);
                /// Thiết lập vào ô
                this.UIItemBox.Data = itemGD;
                this.UIItemBox.Refresh();
                /// Tên vật phẩm
                this.UIText_ItemName.text = itemData.Name;
            }
        }

        private int _Quantity = 0;
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

                /// Số lượng
                this.UIText_Quantity.text = value.ToString();
            }
        }
        #endregion

    }
}
