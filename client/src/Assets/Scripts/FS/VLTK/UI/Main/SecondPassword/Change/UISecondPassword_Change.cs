using System;
using TMPro;
using UnityEngine;

namespace FS.VLTK.UI.Main.SecondPassword
{
    /// <summary>
    /// Khung đổi mật khẩu cấp 2
    /// </summary>
    public class UISecondPassword_Change : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Button đóng khung
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_Close;

        /// <summary>
        /// Ô nhập mật khẩu cấp 2 cũ
        /// </summary>
        [SerializeField]
        private TMP_InputField UIInput_OldPassword;

        /// <summary>
        /// Ô nhập mật khẩu cấp 2 mới
        /// </summary>
        [SerializeField]
        private TMP_InputField UIInput_NewPassword;

        /// <summary>
        /// Ô nhập lại mật khẩu cấp 2 mới
        /// </summary>
        [SerializeField]
        private TMP_InputField UIInput_ReinputNewPassword;

        /// <summary>
        /// Button xác nhận
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_Submit;
        #endregion

        #region Properties
        /// <summary>
        /// Sự kiện đóng khung
        /// </summary>
        public Action Close { get; set; }

        /// <summary>
        /// Sự kiện đổi mật khẩu cấp 2
        /// </summary>
        public Action<string, string, string> Submit { get; set; }
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
            this.UIButton_Close.onClick.AddListener(() =>
            {
                this.Close?.Invoke();
            });
            this.UIButton_Submit.onClick.AddListener(() =>
            {
                /// Mật khẩu cũ
                string oldPassword = this.UIInput_OldPassword.text;
                /// Mật khẩu mới
                string newPassword = this.UIInput_NewPassword.text;
                /// Xác nhận mật khẩu mới
                string reinputNewPassword = this.UIInput_ReinputNewPassword.text;
                /// Toác
                if (string.IsNullOrEmpty(oldPassword))
                {
                    KTGlobal.AddNotification("Hãy nhập khóa an toàn!");
                    return;
                }
                else if (oldPassword.Length != 8)
                {
                    KTGlobal.AddNotification("Khóa an toàn phải có 8 chữ số!");
                    return;
                }
                else if (!int.TryParse(oldPassword, out _))
                {
                    KTGlobal.AddNotification("Khóa an toàn không hợp lệ!");
                    return;
                }
                /// Toác
                else if (string.IsNullOrEmpty(newPassword))
                {
                    KTGlobal.AddNotification("Hãy nhập mật mã mới!");
                    return;
                }
                else if (newPassword.Length != 8)
                {
                    KTGlobal.AddNotification("Khóa an toàn phải có 8 chữ số!");
                    return;
                }
                else if (!int.TryParse(newPassword, out _))
                {
                    KTGlobal.AddNotification("Khóa an toàn không hợp lệ!");
                    return;
                }
                else if (newPassword != reinputNewPassword)
                {
                    KTGlobal.AddNotification("Khóa an toàn không khớp!");
                    return;
                }

                /// Thực thi sự kiện
                this.Submit?.Invoke(oldPassword, newPassword, reinputNewPassword);
            });
        }
        #endregion
    }
}
