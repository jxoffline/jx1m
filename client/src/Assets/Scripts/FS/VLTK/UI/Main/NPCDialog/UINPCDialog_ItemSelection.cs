using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using FS.VLTK.UI.Main.ItemBox;
using Server.Data;

namespace FS.VLTK.UI.Main.NPCDialog
{
    /// <summary>
    /// Selection chọn vật phẩm trong khung thoại NPC
    /// </summary>
    public class UINPCDialog_ItemSelection : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Prefab ItemBox
        /// </summary>
        [SerializeField]
        private UIItemBox UI_ItemPrefab;

        /// <summary>
        /// Animation chọn vật phẩm
        /// </summary>
        [SerializeField]
        private RectTransform UI_SelectAnimation;
        #endregion

        #region Properties
        /// <summary>
        /// Dữ liệu vật phẩm
        /// </summary>
        public GoodsData Data
        {
            get
            {
                return this.UI_ItemPrefab.Data;
            }
            set
            {
                this.UI_ItemPrefab.Data = value;
            }
        }

        /// <summary>
        /// Có tương tác được không
        /// </summary>
        public bool Interactable { get; set; }

        /// <summary>
        /// Vật phẩm có đang được chọn
        /// </summary>
        public bool IsSelected
        {
            get
            {
                return this.UI_SelectAnimation.gameObject.activeSelf;
            }
            set
            {
                this.UI_SelectAnimation.gameObject.SetActive(value);
            }
        }

        /// <summary>
        /// Sự kiện khi vật phẩm được chọn
        /// </summary>
        public Action Select { get; set; }
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
            this.UI_ItemPrefab.Click = this.ButtonItem_Clicked;
        }

        /// <summary>
        /// Sự kiện khi Button vật phẩm được chọn
        /// </summary>
        private void ButtonItem_Clicked()
        {
            List<KeyValuePair<string, Action>> buttons = new List<KeyValuePair<string, Action>>();
            /// Nếu tương tác được
            if (this.Interactable)
            {
                buttons.Add(new KeyValuePair<string, Action>("Chọn", () => {
                    /// Thực thi sự kiện chọn
                    this.Select?.Invoke();
                    /// Đóng Tooltip thông tin vật phẩm
                    KTGlobal.CloseItemInfo();
                }));
            }
            /// Hiển thị Tooltip thông tin vật phẩm
            KTGlobal.ShowItemInfo(this.Data, buttons);
        }
        #endregion
    }
}