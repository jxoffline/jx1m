using FS.VLTK.UI.Main.ItemBox;
using Server.Data;
using TMPro;
using UnityEngine;

namespace FS.VLTK.UI.Main.GuildEx.Dedicate
{
    /// <summary>
    /// Ô vật phẩm quy đổi sang cống hiến cá nhân trong khung cống hiến bang hội
    /// </summary>
    public class UIGuildEx_Dedicate_DedicationPerItem : MonoBehaviour
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
        /// Tỷ lệ quy đổi sang điểm cống hiến bang hội cá nhân
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_DedicationRate;

        /// <summary>
        /// Số lượng vật phẩm đang có trong túi
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_QuantityInBags;
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
                    /// Toác thì bỏ qua
                    return;
                }

                /// Tạo vật phẩm ảo
                GoodsData itemGD = KTGlobal.CreateItemPreview(itemData);
                /// Gắn vào ô vật phẩm
                this.UIItemBox.Data = itemGD;
                this.UIItemBox.Refresh();
                /// Tên vật phẩm
                this.UIText_ItemName.text = itemData.Name;

                /// Số lượng có trong túi
                this.UIText_QuantityInBags.text = KTGlobal.GetItemCountInBag(value).ToString();
            }
        }

        private int _DedicationRate = 0;
        /// <summary>
        /// Tỷ lệ quy đổi sang điểm cống hiến cá nhân
        /// </summary>
        public int DedicationRate
        {
            get
            {
                return this._DedicationRate;
            }
            set
            {
                this._DedicationRate = value;

                /// Tỷ lệ quy đổi
                this.UIText_DedicationRate.text = value.ToString();
            }
        }
        #endregion
    }
}
