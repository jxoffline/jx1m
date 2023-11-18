using FS.VLTK.UI.Main.ItemBox;
using Server.Data;
using System;
using TMPro;
using UnityEngine;

namespace FS.VLTK.UI.Main.SelectItem
{
    /// <summary>
    /// Ô thông tin vật phẩm trong khung chọn vật phẩm
    /// </summary>
    public class UISelectItem_ButtonItem : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Button
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton;

        /// <summary>
        /// Ô vật phẩm
        /// </summary>
        [SerializeField]
        private UIItemBox UIItem;

        /// <summary>
        /// Text tên vật phẩm
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_Name;
        #endregion

        #region Properties
        private GoodsData _Data;
        /// <summary>
        /// Thông tin vật phẩm
        /// </summary>
        public GoodsData Data
        {
            get
            {
                return this._Data;
            }
            set
            {
                this._Data = value;
                /// Gắn vào ô
                this.UIItem.Data = value;
                this.UIItem.Refresh();
                /// Tên
                this.UIText_Name.text = KTGlobal.GetItemName(value.GoodsID);
                /// Màu
                Color color = KTGlobal.GetItemColor(value);
                this.UIText_Name.color = color;
            }
        }

        /// <summary>
        /// Sự kiện chọn vật phẩm
        /// </summary>
        public Action Click { get; set; }
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
            this.UIButton.onClick.AddListener(this.Button_Clicked);
        }

        /// <summary>
        /// Sự kiện khi Button được chọn
        /// </summary>
        private void Button_Clicked()
        {
            this.Click?.Invoke();
        }
        #endregion
    }
}
