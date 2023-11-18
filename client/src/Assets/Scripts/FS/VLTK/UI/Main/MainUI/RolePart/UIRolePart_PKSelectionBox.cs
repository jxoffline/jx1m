using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static FS.VLTK.Entities.Enum;

namespace FS.VLTK.UI.Main.MainUI.RolePart
{
    /// <summary>
    /// Bảng chọn trạng thái PK
    /// </summary>
    public class UIRolePart_PKSelectionBox : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Button luyện công
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_Peace;

        /// <summary>
        /// Button tổ đội
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_Team;

        /// <summary>
        /// Button bang hội
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_Guild;

        /// <summary>
        /// Button thiện ác
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_Moral;

        /// <summary>
        /// Button Server
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_Server;

        /// <summary>
        /// Button đồ sát
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_All;
        #endregion

        #region Properties
        /// <summary>
        /// Sự kiện trạng thái PK được chọn
        /// </summary>
        public Action<PKMode> PKModeSelected { get; set; }

        /// <summary>
        /// Trạng thái kích hoạt của đối tượng
        /// </summary>
        public bool Visible
        {
            get
            {
                return this.gameObject.activeSelf;
            }
            set
            {
                this.gameObject.SetActive(value);
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
            this.UIButton_Peace.onClick.AddListener(() => {
                this.PKModeSelected?.Invoke(PKMode.Peace);
                this.Visible = false;
            });
            this.UIButton_Team.onClick.AddListener(() => {
                this.PKModeSelected?.Invoke(PKMode.Team);
                this.Visible = false;
            });
            this.UIButton_Guild.onClick.AddListener(() => {
                this.PKModeSelected?.Invoke(PKMode.Guild);
                this.Visible = false;
            });
            this.UIButton_Moral.onClick.AddListener(() => {
                this.PKModeSelected?.Invoke(PKMode.Moral);
                this.Visible = false;
            });
            this.UIButton_Server.onClick.AddListener(() => {
                this.PKModeSelected?.Invoke(PKMode.Server);
                this.Visible = false;
            });
            this.UIButton_All.onClick.AddListener(() => {
                this.PKModeSelected?.Invoke(PKMode.All);
                this.Visible = false;
            });
        }
        #endregion
    }
}

