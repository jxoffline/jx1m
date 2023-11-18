using FS.VLTK.Entities.Config;
using FS.VLTK.Factory.UIManager;
using FS.VLTK.Utilities.UnityUI;
using System;
using UnityEngine;

namespace FS.VLTK.UI.Main
{
    /// <summary>
    /// Avarta nhân vật
    /// </summary>
    public class UIRoleAvarta : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Button chọn Avarta
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton;

        /// <summary>
        /// Image avarta
        /// </summary>
        [SerializeField]
        private SpriteFromAssetBundle UIImage_Avarta;
        #endregion

        #region Properties
        /// <summary>
        /// Sự kiện khi đối tượng được chọn
        /// </summary>
        public Action Click { get; set; }

        /// <summary>
        /// ID đối tượng
        /// </summary>
        public int RoleID { get; set; }

        private int _AvartaID = -1;
        /// <summary>
        /// ID avarta
        /// </summary>
        public int AvartaID
        {
            get
            {
                return this._AvartaID;
            }
            set
            {
                this._AvartaID = value;

                /// Lấy thông tin cấu hình Avarta tương ứng
                if (Loader.Loader.RoleAvartas.TryGetValue(value, out RoleAvartaXML roleAvarta))
                {
                    this.UIImage_Avarta.BundleDir = roleAvarta.BundleDir;
                    this.UIImage_Avarta.AtlasName = roleAvarta.AtlasName;
                    this.UIImage_Avarta.SpriteName = roleAvarta.SpriteName;
                    this.UIImage_Avarta.Load();
                }
            }
        }
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi khi đối tượng được tạo ra
        /// </summary>
        private void Awake()
        {
            /// Thêm vào danh sách quản lý
            UIRoleAvartaManager.Instance.AddElement(this);
        }

        /// <summary>
        /// Hàm này gọi ở Frame đầu tiên
        /// </summary>
        private void Start()
        {
            this.InitPrefabs();
        }

        /// <summary>
        /// Hàm này gọi khi đối tượng bị hủy
        /// </summary>
        private void OnDestroy()
        {
            /// Xóa khỏi danh sách quản lý
            UIRoleAvartaManager.Instance.RemoveElement(this);
        }
        #endregion

        #region Code UI
        /// <summary>
        /// Khởi tạo ban đầu
        /// </summary>
        private void InitPrefabs()
        {
            if (this.UIButton != null)
            {
                this.UIButton.onClick.AddListener(this.ButtonAvarta_Clicked);
            }
        }

        /// <summary>
        /// Sự kiện khi Button Avarta được chọn
        /// </summary>
        private void ButtonAvarta_Clicked()
        {
            this.Click?.Invoke();
        }
        #endregion
    }
}
