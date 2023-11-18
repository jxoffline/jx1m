using System;
using UnityEngine;
using TMPro;
using FS.VLTK.Utilities.UnityUI;
using Server.Data;
using FS.GameEngine.Logic;
using System.Collections;

namespace FS.VLTK.UI.Main.Pet.Main
{
    /// <summary>
    /// Thông tin pet trong ô danh sách pet
    /// </summary>
    public class UIPet_PetInfo : MonoBehaviour
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
        /// Text tên Res pet
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_PetResName;

        /// <summary>
        /// Text trạng thái
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_Status;
        #endregion

        #region Properties
        private PetData _Data;
        /// <summary>
        /// Dữ liệu Pet
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
                /// Làm mới dữ liệu
                this.RefreshData();
            }
        }

        /// <summary>
        /// Sự kiện chọn pet này
        /// </summary>
        public Action Select { get; set; }

        /// <summary>
        /// Chọn pet này
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
            this.StartCoroutine(this.ExecuteContinuously());
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
            /// Nếu không được chọn
            if (!isSelected)
            {
                return;
            }
            /// Thực thi sự kiện
            this.Select?.Invoke();
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Thực hiện quét liên tục
        /// </summary>
        /// <returns></returns>
        private IEnumerator ExecuteContinuously()
        {
            WaitForSeconds wait = new WaitForSeconds(1f);
            /// Lặp liên tục
            while (true)
            {
                /// Làm mới dữ liệu
                this.RefreshData();
                /// Nghỉ
                yield return wait;
            }
        }

        /// <summary>
        /// Làm mới dữ liệu
        /// </summary>
        private void RefreshData()
        {
            /// Nếu không có dữ liệu
            if (this._Data == null)
            {
                /// Bỏ qua
                return;
            }

            this.UIText_PetName.text = this._Data.Name;
            /// Nếu Res tồn tại
            if (Loader.Loader.ListPets.TryGetValue(this._Data.ResID, out Entities.Config.PetDataXML petData))
            {
                this.UIText_PetResName.text = petData.Name;
            }
            /// Toác
            else
            {
                this.UIText_PetResName.text = "Chưa cập nhật";
            }
            this.UIText_Status.text = Global.Data.RoleData.CurrentPetID == this._Data.ID ? "<color=green>[Tham chiến]</color>" : "<color=red>[Nghỉ ngơi]</color>";
        }
        #endregion
    }
}
