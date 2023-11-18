using FS.VLTK.Utilities.UnityUI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;
using FS.VLTK.Entities.Config;
using FS.GameEngine.Logic;
using System;
using Server.Data;

namespace FS.VLTK.UI.Main.MainUI.MiniTaskAndTeamFrame
{
    /// <summary>
    /// Thông tin đội viên trong khung TeamFrameMini
    /// </summary>
    public class UIMiniTaskAndTeamFrame_MiniTeamFrame_TeammateInfo : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Avarta
        /// </summary>
        [SerializeField]
        private UIRoleAvarta UI_Avarta;

        /// <summary>
        /// Cấp độ
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_Level;

        /// <summary>
        /// Tên
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_Name;

        /// <summary>
        /// Icon trưởng nhóm
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Image UIImage_TeamLeader_Icon;

        /// <summary>
        /// Thanh máu
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Slider UISlider_HPBar;

        /// <summary>
        /// Giá trị máu
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_HPValue;

        /// <summary>
        /// Nút chức năng mở rộng
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_MoreFunction;

        /// <summary>
        /// Text tên phái
        /// </summary>
        [SerializeField]
        public TextMeshProUGUI UIText_FactionName;
        #endregion

        #region Properties
        private RoleDataMini _Data;
        /// <summary>
        /// Dữ liệu đội việ
        /// </summary>
        public RoleDataMini Data
        {
            get
            {
                return this._Data;
            }
            set
            {
                this._Data = value;
                this.DoRefreshData();
            }
        }

        /// <summary>
        /// Sự kiện khi nút chức năng mở rộng được ấn
        /// </summary>
        public Action<RoleDataMini> MoreFunctionsClicked { get; set; }
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi đến ở Frame đầu tiên
        /// </summary>
        private void Start()
        {
            this.InitPrefabs();
            this.DoRefreshData();
        }
        #endregion

        #region Code UI
        /// <summary>
        /// Khởi tạo ban đầu
        /// </summary>
        private void InitPrefabs()
        {
            this.UIButton_MoreFunction.onClick.AddListener(this.ButtonMoreFunctions_Clicked);
        }

        /// <summary>
        /// Sự kiện khi Button chức năng mở rộng được ấn
        /// </summary>
        private void ButtonMoreFunctions_Clicked()
        {
            this.MoreFunctionsClicked?.Invoke(this._Data);
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Làm mới dữ liệu
        /// </summary>
        private void DoRefreshData()
        {
            if (this._Data == null)
            {
                return;
            }

            this.UIText_FactionName.text = KTGlobal.GetFactionName(this._Data.FactionID, out Color factionColor);
            this.UIText_FactionName.color = factionColor;
            this.UI_Avarta.RoleID = this._Data.RoleID;
            this.UI_Avarta.AvartaID = this._Data.AvartaID;
            this.UIText_Name.text = this._Data.RoleName;
            this.UIText_Level.text = this._Data.Level.ToString();
            /// Nếu không rõ mức máu
            if (this._Data.HP == -1 && this._Data.MaxHP == -1)
            {
                this.UIText_HPValue.text = "Không rõ";
                this.UISlider_HPBar.value = 1f;
            }
            else
            {
                this.UIText_HPValue.text = string.Format("{0}/{1}", this._Data.HP, this._Data.MaxHP);
                this.UISlider_HPBar.value = this._Data.HP / (float) this._Data.MaxHP;
            }
            
            if (this._Data.RoleID == this._Data.TeamLeaderID)
            {
                this.UIImage_TeamLeader_Icon.gameObject.SetActive(true);
            }
            else
            {
                this.UIImage_TeamLeader_Icon.gameObject.SetActive(false);
            }

            if (this._Data.RoleID == Global.Data.RoleData.RoleID)
            {
                this.UIButton_MoreFunction.interactable = false;
            }
            else
            {
                this.UIButton_MoreFunction.interactable = true;
            }
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Làm mới biểu tượng đội trưởng
        /// </summary>
        public void RefreshTeamLeader()
        {
            if (this._Data.RoleID == this._Data.TeamLeaderID)
            {
                this.UIImage_TeamLeader_Icon.gameObject.SetActive(true);
            }
            else
            {
                this.UIImage_TeamLeader_Icon.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Làm mới dữ liệu
        /// </summary>
        public void RefreshData()
        {
            this.DoRefreshData();
        }
        #endregion
    }
}
