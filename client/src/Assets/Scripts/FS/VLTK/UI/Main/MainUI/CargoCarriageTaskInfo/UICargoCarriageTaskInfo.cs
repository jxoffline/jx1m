using FS.VLTK.UI.Main.MainUI.CargoCarriageTaskInfo;
using Server.Data;
using System.Collections;
using TMPro;
using UnityEngine;

namespace FS.VLTK.UI.Main.MainUI
{
    /// <summary>
    /// Thông tin nhiệm vụ vận tiêu
    /// </summary>
    public class UICargoCarriageTaskInfo : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Button Icon nhiệm vụ
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_Icon;

        /// <summary>
        /// Khung tương ứng
        /// </summary>
        [SerializeField]
        private UICargoCarriageTaskInfo_Frame UI_Frame;

        /// <summary>
        /// Text thời gian đếm ngược
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_CountdownTimer;
        #endregion

        #region Properties
        /// <summary>
        /// Dữ liệu
        /// </summary>
        public G2C_CargoCarriageTaskData Data
        {
            get
            {
                return this.UI_Frame.Data;
            }
            set
            {
                this.UI_Frame.Data = value;
            }
        }

        /// <summary>
        /// Đã hoàn thành chưa
        /// </summary>
        public bool Completed
        {
            get
            {
                return this.UI_Frame.Completed;
            }
            set
            {
                this.UI_Frame.Completed = value;
            }
        }

        /// <summary>
        /// Thời gian còn lại
        /// </summary>
        public int TimeLeft { get; set; } = -1;
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi ở Frame đầu tiên
        /// </summary>
        private void Start()
        {
            this.InitPrefabs();
            this.StartCoroutine(this.DoCountDown());
        }
        #endregion

        #region Code UI
        /// <summary>
        /// Khởi tạo ban đầu
        /// </summary>
        private void InitPrefabs()
        {
            this.UIButton_Icon.onClick.AddListener(this.Button_Clicked);
        }

        /// <summary>
        /// Sự kiện khi Icon được ấn
        /// </summary>
        private void Button_Clicked()
        {
            /// Hiện khung thông tin chi tiết
            this.UI_Frame.Show();
        }
        #endregion

        #region Code UI
        /// <summary>
        /// Luồng thực thi đếm lùi
        /// </summary>
        /// <returns></returns>
        private IEnumerator DoCountDown()
        {
            /// Đợi 1s
            WaitForSeconds wait = new WaitForSeconds(1f);
            /// Lặp liên tục
            while (true)
            {
                /// Nếu không có thời gian
                if (this.TimeLeft < 0)
                {
                    /// Ẩn đi
                    this.UIText_CountdownTimer.text = "";
                }
                /// Nếu có thời gian
                else
                {
                    /// Điền Text vào
                    this.UIText_CountdownTimer.text = KTGlobal.DisplayTimeHourMinuteSecondOnly(this.TimeLeft, false);
                    /// Giảm đi
                    this.TimeLeft--;
                }
                /// Đợi 1s
                yield return wait;
            }
        }
        #endregion
    }
}
