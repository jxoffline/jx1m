using System;
using System.Collections;
using TMPro;
using UnityEngine;

namespace FS.VLTK.UI.Main.SecondPassword
{
    /// <summary>
    /// Khung nhập mật khẩu cấp 2
    /// </summary>
    public class UISecondPassword_Input : MonoBehaviour
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
        /// Button xác nhận
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_Submit;

        /// <summary>
        /// Button yêu cầu xóa mật khẩu cấp 2
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_RequestRemove;

        /// <summary>
        /// Button hủy yêu cầu xóa mật khẩu cấp 2
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_CancelRemove;

        /// <summary>
        /// Text thời gian đếm lùi xóa mật khẩu cấp 2
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_RemoveCountDown;
        #endregion

        #region Properties
        /// <summary>
        /// Thời gian còn lại tự xóa mật khẩu cấp 2
        /// </summary>
        public int AutoRemoveSecLeft { get; set; } = -1;

        /// <summary>
        /// Sự kiện đóng khung
        /// </summary>
        public Action Close { get; set; }

        /// <summary>
        /// Sự kiện nhập mật khẩu cấp 2
        /// </summary>
        public Action<string> Submit { get; set; }

        /// <summary>
        /// Sự kiện yêu cầu xóa mật khẩu cấp 2
        /// </summary>
        public Action RequestRemove { get; set; }

        /// <summary>
        /// Sự kiện hủy yêu cầu xóa mật khẩu cấp 2
        /// </summary>
        public Action CancelRemove { get; set; }
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
                /// Mật khẩu tương ứng
                string password = this.UIInput_Password.text;
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

                /// Thực thi sự kiện
                this.Submit?.Invoke(password);
            });
            this.UIButton_RequestRemove.onClick.AddListener(() =>
            {
                this.RequestRemove?.Invoke();
            });
            this.UIButton_CancelRemove.onClick.AddListener(() =>
            {
                this.CancelRemove?.Invoke();
            });

            /// Ẩn cả 2 Button
            this.UIButton_RequestRemove.gameObject.SetActive(false);
            this.UIButton_CancelRemove.gameObject.SetActive(false);

            /// Nếu có thời gian chờ xóa
            if (this.AutoRemoveSecLeft > 0)
            {
                /// Thực hiện đếm lùi
                this.StartCoroutine(this.CountDown());
                /// Hiện Button hủy yêu cầu xóa
                this.UIButton_CancelRemove.gameObject.SetActive(true);
            }
            /// Nếu không có thời gian chờ xóa
            else
            {
                this.UIText_RemoveCountDown.text = "";
                /// Hiện Button yêu cầu xóa
                this.UIButton_RequestRemove.gameObject.SetActive(true);
            }
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Thực hiện đếm lùi
        /// </summary>
        /// <returns></returns>
        private IEnumerator CountDown()
        {
            /// Đối tượng nghỉ
            WaitForSeconds wait = new WaitForSeconds(1f);
            /// Lặp liên tục
            while (true)
            {
                /// Giảm số giây
                this.AutoRemoveSecLeft--;
                /// Nếu đã hết thời gian
                if (this.AutoRemoveSecLeft <= 0)
                {
                    /// Ẩn Text
                    this.UIText_RemoveCountDown.text = "";
                    /// Ẩn Button
                    this.UIButton_RequestRemove.gameObject.SetActive(false);
                    this.UIButton_CancelRemove.gameObject.SetActive(false);
                    /// Thoát
                    break;
                }
                /// Cập nhật thời gian
                this.UIText_RemoveCountDown.text = string.Format("Thời gian xóa còn lại: <color=green>{0}</color>", KTGlobal.DisplayFullDateAndTime(this.AutoRemoveSecLeft));
                /// Nghỉ
                yield return wait;
            }
        }
        #endregion
    }
}
