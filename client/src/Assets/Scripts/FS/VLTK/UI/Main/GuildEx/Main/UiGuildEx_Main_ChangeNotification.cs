using FS.GameEngine.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using static FS.VLTK.Entities.Enum;

namespace FS.VLTK.UI.Main.GuildEx.Main
{
    /// <summary>
    /// Sửa công cáo bang hội
    /// </summary>
    public class UiGuildEx_Main_ChangeNotification : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Button đóng khung
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_Close;

        /// <summary>
        /// Input công cáo bang hội
        /// </summary>
        [SerializeField]
        private TMP_InputField UIInput_GuildNotification;

        /// <summary>
        /// Button xác nhận
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_Accept;
        #endregion

        #region Properties
        /// <summary>
        /// Sự kiện sửa công cáo bang hội
        /// </summary>
        public Action<string> ChangeNotification { get; set; }
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
            this.UIButton_Close.onClick.AddListener(this.ButtonClose_Clicked);
            this.UIButton_Accept.onClick.AddListener(this.ButtonAccept_Clicked);
        }

        /// <summary>
        /// Sự kiện đóng khung
        /// </summary>
        private void ButtonClose_Clicked()
        {
            this.Hide();
        }

        /// <summary>
        /// Sự kiện khi Button xác nhận được ấn
        /// </summary>
        private void ButtonAccept_Clicked()
        {
            /// Nếu không phải bang chủ hoặc phó bang chủ
            if (Global.Data.RoleData.GuildRank != (int) GuildRank.Master && Global.Data.RoleData.GuildRank != (int) GuildRank.ViceMaster)
            {
                /// Thông báo
                KTGlobal.AddNotification("Chỉ có bang chủ hoặc phó bang chủ mới có quyền sửa công cáo bang hội!");
                return;
            }
            /// Chuỗi công cáo
            string guildNotification = this.UIInput_GuildNotification.text.Trim();
            /// Toác
            if (string.IsNullOrEmpty(guildNotification))
            {
                KTGlobal.AddNotification("Hãy nhập vào công cáo bang hội!");
                return;
            }
            else if (!KTFormValidation.IsValidString(guildNotification))
            {
                KTGlobal.AddNotification("Công cáo bang hội có chứa ký tự không hợp lệ. Hãy nhập công cáo khác!");
                return;
            }

            /// Thực thi sự kiện
            this.ChangeNotification?.Invoke(guildNotification);
            /// Đóng khung
            this.Hide();
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Hiện khung
        /// </summary>
        public void Show()
        {
            this.gameObject.SetActive(true);
        }

        /// <summary>
        /// Ẩn khung
        /// </summary>
        public void Hide()
        {
            this.gameObject.SetActive(false);
        }
        #endregion
    }
}
