using FS.GameEngine.Logic;
using FS.VLTK.Factory.UIManager;
using FS.VLTK.Utilities.UnityUI;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using static FS.VLTK.Entities.Enum;

namespace FS.VLTK.UI.Main.Money
{
    /// <summary>
    /// Ô hiển thị giá trị tiền tệ với đơn vị tương ứng
    /// </summary>
    public class UIMoneyBox : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Loại tiền
        /// </summary>
        [SerializeField]
        private MoneyType _Type;

        /// <summary>
        /// Giá trị số tiền
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_Value;

        /// <summary>
        /// Icon của tiền
        /// </summary>
        [SerializeField]
        private SpriteFromAssetBundle UIImage_Icon;
        #endregion

        #region Properties
        /// <summary>
        /// Loại tiền
        /// </summary>
        public MoneyType Type
        {
            get
            {
                return this._Type;
            }
            set
            {
                this._Type = value;
                this.RefreshDisplay();
                this.Refresh();
            }
        }
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi khi đối tượng được tạo ra
        /// </summary>
        private void Awake()
        {
            UIMoneyManager.Instance.AddElement(this);
        }

        /// <summary>
        /// Hàm này gọi ở Frame đầu tiên
        /// </summary>
        private void Start()
        {
            this.InitPrefabs();
            this.Refresh();
            this.RefreshDisplay();
        }

        /// <summary>
        /// Hàm này gọi khi đối tượng bị xóa
        /// </summary>
        private void OnDestroy()
        {
            UIMoneyManager.Instance.RemoveElement(this);
        }
        #endregion

        #region Code UI
        /// <summary>
        /// Khởi tạo ban đầu
        /// </summary>
        private void InitPrefabs()
        {

        }
        #endregion

        #region Private methods
        /// <summary>
        /// Làm mới hiển thị
        /// </summary>
        private void RefreshDisplay()
        {
            KTGlobal.GetMoneyDisplayImage(this._Type, out string bundleDir, out string atlasName, out string spriteName);
            this.UIImage_Icon.BundleDir = bundleDir;
            this.UIImage_Icon.AtlasName = atlasName;
            if (!string.IsNullOrEmpty(spriteName))
            {
                this.UIImage_Icon.SpriteName = spriteName;
                this.UIImage_Icon.Load();
            }
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Cập nhật giá trị tiền
        /// </summary>
        public void Refresh()
        {
            switch (this._Type)
            {
                case MoneyType.Bac:
                {
                    this.UIText_Value.text = KTGlobal.GetDisplayMoney(Global.Data.RoleData.Money);
                    break;
                }
                case MoneyType.BacKhoa:
                {
                    this.UIText_Value.text = KTGlobal.GetDisplayMoney(Global.Data.RoleData.BoundMoney);
                    break;
                }
                case MoneyType.Dong:
                {
                    this.UIText_Value.text = KTGlobal.GetDisplayMoney(Global.Data.RoleData.Token);
                    break;
                }
                case MoneyType.DongKhoa:
                {
                    this.UIText_Value.text = KTGlobal.GetDisplayMoney(Global.Data.RoleData.BoundToken);
                    break;
                }
                case MoneyType.GuildMoney:
                    {
                        this.UIText_Value.text = KTGlobal.GetDisplayMoney(Global.Data.RoleData.GuildMoney);
                        break;
                    }
                    /// ETC...
            }
        }
        #endregion
    }
}
