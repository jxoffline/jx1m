using FS.VLTK.Entities.Config;
using FS.VLTK.UI.Main.ItemBox;
using FS.VLTK.Utilities.UnityUI;
using System;
using System.Linq;
using TMPro;
using UnityEngine;
using static FS.VLTK.Entities.Enum;

namespace FS.VLTK.UI.Main.Crafting
{
    /// <summary>
    /// Vật phẩm trong danh sách chế tạo
    /// </summary>
    public class UICrafting_CraftingItem : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Toggle
        /// </summary>
        [SerializeField]
        private UIToggleSprite UIToggle;

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
        /// Text ngũ hành vật phẩm
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_ItemSeries;
        #endregion

        #region Properties
        /// <summary>
        /// Sự kiện khi đối tượng được chọn
        /// </summary>
        public Action Selected { get; set; }

        private Recipe _Data = null;
        /// <summary>
        /// Dữ liệu vật phẩm
        /// </summary>
        public Recipe Data
        {
            get
            {
                return this._Data;
            }
            set
            {
                this._Data = value;

                /// Nếu dữ liệu không tồn tại
                if (value == null)
                {
                    this.UIText_ItemName.text = "";
                    this.UIText_ItemSeries.text = "";
                }
                else
                {
                    ItemCraf itemCraft = value.ListProduceOut.FirstOrDefault();
                    if (itemCraft != null && Loader.Loader.Items.TryGetValue(itemCraft.ItemTemplateID, out ItemData itemData))
                    {
                        this.UIItemBox.Data = KTGlobal.CreateItemPreview(itemData);
                        this.UIText_ItemName.text = KTGlobal.GetItemName(this.UIItemBox.Data, false);

                        if (itemData.Series <= (int)Elemental.NONE || itemData.Series >= (int)Elemental.COUNT)
                        {
                            this.UIText_ItemSeries.text = "";
                        }
                        else
                        {
                            this.UIText_ItemSeries.text = "";
                        }
                    }
                }
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
            this.UIItemBox.Click = null;
            this.UIToggle.OnSelected = this.Toggle_Selected;
        }

        /// <summary>
        /// Sự kiện khi Toggle được chọn
        /// </summary>
        /// <param name="isSelected"></param>
        private void Toggle_Selected(bool isSelected)
        {
            if (isSelected)
            {
                this.Selected?.Invoke();
            }
        }
        #endregion
    }
}
