using FS.VLTK.Utilities.UnityUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace FS.VLTK.UI.SelectServer
{
    /// <summary>
    /// Đối tượng Toggle cụm máy chủ trong màn hình chọn máy chủ
    /// </summary>
    public class UISelectServer_ToggleGroupServer : TTMonoBehaviour
    {
        #region Define
        /// <summary>
        /// Toggle
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Toggle UIToggle_GroupServer;

        /// <summary>
        /// Background
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Image UIImage_Background;

        /// <summary>
        /// Tên cụm
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_GroupName;

        /// <summary>
        /// Bundle chứa ảnh
        /// </summary>
        [SerializeField]
        private string _BundleDir;

        /// <summary>
        /// Atlas
        /// </summary>
        [SerializeField]
        private string _AtlasName;

        /// <summary>
        /// Tên Sprite ở trạng thái thường
        /// </summary>
        [SerializeField]
        private string _NormalSprite;

        /// <summary>
        /// Tên Sprite ở trạng thái kích hoạt
        /// </summary>
        [SerializeField]
        private string _ActiveSprite;

        /// <summary>
        /// Màu chữ ở trạng thái thường
        /// </summary>
        [SerializeField]
        private Color _NormalColor;

        /// <summary>
        /// Màu chữ ở trạng thái kích hoạt
        /// </summary>
        [SerializeField]
        private Color _ActiveColor;
        #endregion

        #region Properties
        /// <summary>
        /// Tên cụm
        /// </summary>
        public string Name
        {
            get
            {
                return this.UIText_GroupName.text;
            }
            set
            {
                this.UIText_GroupName.text = value;
            }
        }

        /// <summary>
        /// Kích hoạt không
        /// </summary>
        public bool Active
        {
            set
            {
                SpriteFromAssetBundle sprite = this.UIImage_Background.gameObject.GetComponent<SpriteFromAssetBundle>();
                sprite.BundleDir = this._BundleDir;
                sprite.AtlasName = this._AtlasName;

                if (value)
                {
                    sprite.SpriteName = this._ActiveSprite;
                    sprite.Load();

                    this.UIText_GroupName.color = this._ActiveColor;
                }
                else
                {
                    sprite.SpriteName = this._NormalSprite;
                    sprite.Load();

                    this.UIText_GroupName.color = this._NormalColor;
                }

                this.UIToggle_GroupServer.isOn = value;
            }
        }

        /// <summary>
        /// Nhóm đối tượng liên quan
        /// </summary>
        public UnityEngine.UI.ToggleGroup Group
        {
            set
            {
                this.UIToggle_GroupServer.group = value;
            }
        }

        /// <summary>
        /// Sự kiện khi đối tượng được kích hoạt
        /// </summary>
        public Action<bool> OnActivated { get; set; }
        #endregion


        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi đến ở Frame đầu tiên
        /// </summary>
        private void Start()
        {
            this.InitPrefabs();
        }
        #endregion

        #region Code UI
        private void InitPrefabs()
        {
            this.UIToggle_GroupServer.onValueChanged.AddListener((isSelected) => {
                this.OnActivated?.Invoke(isSelected);
            });
        }
        #endregion
    }
}
