using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using FS.VLTK.Utilities.UnityUI;
using System;
using System.Linq;
using UnityEngine.UI;

namespace FS.VLTK.UI.Main.LocalMap
{
    /// <summary>
    /// Đối tượng Tab nằm trong bản đồ khu vực
    /// </summary>
    public class UILocalMap_TabItem : MonoBehaviour
    {
        /// <summary>
        /// Item phụ bên trong Tab
        /// </summary>
        private class UILocalMap_TabSubItem
        {
            /// <summary>
            /// Toggle
            /// </summary>
            public UIToggleSprite UIToggle;

            /// <summary>
            /// Tên Tab
            /// </summary>
            public string Name;

            /// <summary>
            /// Vị trí dịch đến
            /// </summary>
            public Vector2 DestinationPosition;
        }

        #region Define
        /// <summary>
        /// Toggle
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Toggle UIToggle;

        /// <summary>
        /// Icon
        /// </summary>
        [SerializeField]
        private SpriteFromAssetBundle UIImage_Icon;

        /// <summary>
        /// Label
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_Label;

        /// <summary>
        /// Prefab item bên trong
        /// </summary>
        [SerializeField]
        private UIToggleSprite ItemPrefab;

        /// <summary>
        /// Tên Sprite mũi tên xuống
        /// </summary>
        [SerializeField]
        private string ArrowDownSprite;

        /// <summary>
        /// Tên Sprite mũi tên lên
        /// </summary>
        [SerializeField]
        private string ArrowUpSprite;
        #endregion

        /// <summary>
        /// Danh sách item bên trong
        /// </summary>
        private readonly List<UILocalMap_TabSubItem> Items = new List<UILocalMap_TabSubItem>();

        #region Properties
        /// <summary>
        /// Kích hoạt
        /// </summary>
        public bool Active
        {
            get
            {
                return this.UIToggle.isOn;
            }
            set
            {
                this.UIToggle.isOn = value;
            }
        }

        private List<KeyValuePair<string, Vector2>> _ListItems = new List<KeyValuePair<string, Vector2>>();
        /// <summary>
        /// Danh sách Item bên trong
        /// </summary>
        public List<KeyValuePair<string, Vector2>> ListItems
        {
            get
            {
                return this._ListItems;
            }
            set
            {
                this._ListItems = value;
                this.UpdateListItems();
            }
        }

        /// <summary>
        /// Text tên Tab
        /// </summary>
        public string Text
        {
            get
            {
                return this.UIText_Label.text;
            }
            set
            {
                this.UIText_Label.text = value;
            }
        }

        /// <summary>
        /// Sự kiện khi Toggle được kích hoạt
        /// </summary>
        public Action<bool> OnSelected { get; set; }

        /// <summary>
        /// Sự kiện khi vị trí được chọn
        /// </summary>
        public Action<KeyValuePair<string, Vector2>> LocationSelected { get; set; }
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Rect Transform
        /// </summary>
        private RectTransform parentRectTransform;

        /// <summary>
        /// Hàm này gọi ở Frame đầu tiên
        /// </summary>
        private void Start()
        {
            this.InitPrefabs();
            this.parentRectTransform = this.transform.parent.gameObject.GetComponent<RectTransform>();
            this.UpdateListItems();
        }
        #endregion

        #region Code UI
        /// <summary>
        /// Khởi tạo ban đầu
        /// </summary>
        private void InitPrefabs()
        {
            this.UIToggle.onValueChanged.AddListener(this.Toggle_Selected);
        }

        /// <summary>
        /// Sự kiện khi trạng thái Toggle thay đổi
        /// </summary>
        private void Toggle_Selected(bool isSelected)
        {
            this.UpdateToggleVisible(isSelected);
            this.OnSelected?.Invoke(isSelected);
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Cập nhật sự thay đổi trạng thái của Toggle
        /// </summary>
        private void UpdateToggleVisible(bool isVisible)
        {
            foreach (UILocalMap_TabSubItem child in this.Items)
            {
                child.UIToggle.gameObject.SetActive(isVisible);
                child.UIToggle.Active = false;
            }

            if (isVisible)
            {
                this.UIImage_Icon.SpriteName = this.ArrowUpSprite;
                this.UIImage_Icon.Load();
            }
            else
            {
                this.UIImage_Icon.SpriteName = this.ArrowDownSprite;
                this.UIImage_Icon.Load();
            }
        }

        /// <summary>
        /// Xóa tất cả Item con bên trong
        /// </summary>
        private void ClearAllItems()
        {
            foreach (UILocalMap_TabSubItem item in this.Items)
            {
                GameObject.Destroy(item.UIToggle.gameObject);
            }
            this.Items.Clear();
        }

        /// <summary>
        /// Cập nhật danh sách Item
        /// </summary>
        private void UpdateListItems()
        {
            this.ClearAllItems();
            foreach (KeyValuePair<string, Vector2> pair in this._ListItems)
            {
                UIToggleSprite toggle = GameObject.Instantiate<UIToggleSprite>(this.ItemPrefab);
                //toggle.gameObject.SetActive(true);
                toggle.Active = false;
                toggle.ShowBackground = true;
                toggle.Name = pair.Key;
                toggle.transform.SetParent(this.gameObject.transform, false);
                toggle.OnSelected = (isSelected) => {
                    if (isSelected)
                    {
                        this.LocationSelected?.Invoke(pair);
                    }
                    LayoutRebuilder.ForceRebuildLayoutImmediate(this.parentRectTransform);
                };
                this.Items.Add(new UILocalMap_TabSubItem()
                {
                    Name = pair.Key,
                    DestinationPosition = pair.Value,
                    UIToggle = toggle,
                });
            }
        }
        #endregion
    }
}
