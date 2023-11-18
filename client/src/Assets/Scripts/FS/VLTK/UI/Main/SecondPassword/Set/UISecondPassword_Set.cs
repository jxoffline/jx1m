using System;
using TMPro;
using UnityEngine;

namespace FS.VLTK.UI.Main.SecondPassword
{
    /// <summary>
    /// Khung thiết lập mật khẩu cấp 2
    /// </summary>
    public class UISecondPassword_Set : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Button đóng khung
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_Close;

        /// <summary>
        /// Ô nhập mật khẩu cấp 2
        /// </summary>
        [SerializeField]
        private TMP_InputField UIInput_Password;

        /// <summary>
        /// Ô nhập lại mật khẩu cấp 2
        /// </summary>
        [SerializeField]
        private TMP_InputField UIInput_ReinputPassword;

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
        /// Sự kiện thiết lập mật khẩu cấp 2
        /// </summary>
        public Action<string, string> Submit { get; set; }
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
                /// Mật khẩu mới
                string password = this.UIInput_Password.text;
                /// Xác nhận mật khẩu mới
                string reinputPassword = this.UIInput_ReinputPassword.text;
                /// Toác
                if (string.IsNullOrEmpty(password))
                {
                    KTGlobal.AddNotification("Hãy nhập khóa an toàn!");
                    return;
                }
                else if (password.Length != 8)
                {
                    KTGlobal.AddNotification("Khóa an toàn phải có 8 chữ số!");
                    return;
                }
                else if (!int.TryParse(password, out _))
                {
                    KTGlobal.AddNotification("Khóa an toàn không hợp lệ!");
                    return;
                }
                else if (password != reinputPassword)
                {
                    KTGlobal.AddNotification("Khóa an toàn không khớp!");
                    return;
                }

                /// Thực thi sự kiện
                this.Submit?.Invoke(password, reinputPassword);
            });
        }
        #endregion
    }
}
