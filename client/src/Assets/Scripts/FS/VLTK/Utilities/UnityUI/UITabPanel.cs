using FS.VLTK.Utilities.UnityUI;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FS.VLTK.Utilities.UnityUI
{
    /// <summary>
    /// Tab
    /// </summary>
    [Serializable]
    public class UITab
    {
        /// <summary>
        /// Tab header
        /// </summary>
        public Toggle TabHeader;

        /// <summary>
        /// Text chứa tên tab
        /// </summary>
        public TextMeshProUGUI UIText_Name;

        /// <summary>
        /// Nội dung
        /// </summary>
        public GameObject Content;

        /// <summary>
        /// Tên
        /// </summary>
        public string Name;

        /// <summary>
        /// Đường dẫn Bundle chứa Tab
        /// </summary>
        public string BundleDir;

        /// <summary>
        /// Tên Atlas
        /// </summary>
        public string AtlasName;

        /// <summary>
        /// Sprite trạng thái thường (bỏ trống nếu mặc định)
        /// </summary>
        public string TabHeaderNormalSprite;

        /// <summary>
        /// Sprite trạng thái kích hoạt (bỏ trống nếu mặc định)
        /// </summary>
        public string TabHeaderActiveSprite;

        /// <summary>
        /// Kích hoạt Tab
        /// </summary>
        public bool Active
        {
            get
            {
                return this.TabHeader.isOn;
            }
            set
            {
                this.TabHeader.isOn = value;
            }
        }
    }

    /// <summary>
    /// Đối tượng Tab panel
    /// </summary>
    public class UITabPanel : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Tab headers
        /// </summary>
        [SerializeField]
        private UITab[] UITabHeaders;
        #endregion

        #region Properties
        /// <summary>
        /// Sự kiện khi Tab được chọn
        /// </summary>
        public Action<UITab> SelectTab { get; set; }
        #endregion

        /// <summary>
        /// Tab đã được chọn trước
        /// </summary>
        private UITab lastSelectedTab;

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này chạy ở frame đầu tiên
        /// </summary>
        private void Start()
        {
            foreach (UITab tab in this.UITabHeaders)
            {
                tab.TabHeader.onValueChanged.AddListener((isSelected) => {
                    if (this.lastSelectedTab != tab)
                    {
                        this.OnTabSelected(tab);
                        this.lastSelectedTab = tab;
                    }
                });
            }

            if (this.UITabHeaders.Length > 0)
            {
                this.UITabHeaders[0].TabHeader.isOn = true;
            }
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Sự kiện khi Tab được kích hoạt
        /// </summary>
        /// <param name="tab"></param>
        private void OnTabSelected(UITab tab)
        {
            SpriteFromAssetBundle assetBundle = tab.TabHeader.transform.GetChild(0).gameObject.GetComponent<SpriteFromAssetBundle>();
            assetBundle.BundleDir = tab.BundleDir;
            assetBundle.AtlasName = tab.AtlasName;
            assetBundle.SpriteName = tab.TabHeaderActiveSprite;
            assetBundle.Load();
            tab.Content.SetActive(true);
            tab.Active = true;

            foreach (UITab _tab in this.UITabHeaders)
            {
                if (_tab != tab)
                {
                    SpriteFromAssetBundle _assetBundle = _tab.TabHeader.transform.GetChild(0).gameObject.GetComponent<SpriteFromAssetBundle>();
                    _assetBundle.BundleDir = _tab.BundleDir;
                    _assetBundle.AtlasName = _tab.AtlasName;
                    _assetBundle.SpriteName = _tab.TabHeaderNormalSprite;
                    _assetBundle.Load();
                    _tab.Content.SetActive(false);
                    _tab.Active = false;
                }
            }

            this.SelectTab?.Invoke(tab);
        }
        #endregion
    }

}