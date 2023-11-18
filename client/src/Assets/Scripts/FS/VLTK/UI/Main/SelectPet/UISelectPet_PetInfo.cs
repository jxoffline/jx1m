using FS.GameEngine.Logic;
using FS.VLTK.Entities.Config;
using FS.VLTK.Utilities.UnityUI;
using Server.Data;
using System;
using TMPro;
using UnityEngine;

namespace FS.VLTK.UI.Main.SelectPet
{
    /// <summary>
    /// Thông tin pet trong khung chọn pet
    /// </summary>
    public class UISelectPet_PetInfo : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Toggle
        /// </summary>
        [SerializeField]
        private UIToggleSprite UIToggle;

        /// <summary>
        /// Text tên pet
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_PetName;

        /// <summary>
        /// Text loại pet
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_PetResName;
        #endregion

        #region Properties
        private PetData _Data;
        /// <summary>
        /// Dữ liệu pet
        /// </summary>
        public PetData Data
        {
            get
            {
                return this._Data;
            }
            set
            {
                this._Data = value;
                /// Tên
                this.UIText_PetName.text = value.Name;
                /// Nếu là pet đang tham chiến
                if (Global.Data.RoleData.CurrentPetID == value.ID)
                {
                    this.UIText_PetName.color = Color.green;
                }
                /// Nếu không phải pet đang tham chiến
                else
                {
                    this.UIText_PetName.color = Color.white;
                }
                /// Thông tin res
                if (Loader.Loader.ListPets.TryGetValue(value.ResID, out PetDataXML petData))
                {
                    /// Tên Res
                    this.UIText_PetResName.text = string.Format("[{0}]", petData.Name);
                }
            }
        }

        /// <summary>
        /// Sự kiện chọn đối tượng
        /// </summary>
        public Action Select { get; set; }

        /// <summary>
        /// Kích hoạt đối tượng
        /// </summary>
        public bool Active
        {
            get
            {
                return this.UIToggle.Active;
            }
            set
            {
                this.UIToggle.Active = value;
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
                /// Thực thi sự kiện
                this.Select?.Invoke();
            }
        }
        #endregion
    }
}
