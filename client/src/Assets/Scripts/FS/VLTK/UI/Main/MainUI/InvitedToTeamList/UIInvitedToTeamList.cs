using FS.GameEngine.Logic;
using FS.VLTK.UI.Main.MainUI.InvitedToTeamList;
using System;
using System.Collections;
using UnityEngine;

namespace FS.VLTK.UI.Main.MainUI
{
    /// <summary>
    /// Danh sách mời vào nhóm
    /// </summary>
    public class UIInvitedToTeamList : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Button mở khung
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton;

        /// <summary>
        /// Frame danh sách người chơi mời vào nhóm
        /// </summary>
        [SerializeField]
        private UIInvitedToTeamList_Frame UI_Frame;
        #endregion

        #region Properties
        /// <summary>
        /// Sự kiện đồng ý vào nhóm
        /// </summary>
        public Action<int, int> AgreeJoinTeam { get; set; }

        /// <summary>
        /// Sự kiện từ chối vào nhóm
        /// </summary>
        public Action<int, int> RejectJoinTeam { get; set; }
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi ở Frame đầu tiên
        /// </summary>
        private void Start()
        {
            this.InitPrefabs();
            /// Thực hiện quét liên tục
            this.StartCoroutine(this.DoScanContinuously());
        }
        #endregion

        #region Code UI
        /// <summary>
        /// Khởi tạo ban đầu
        /// </summary>
        private void InitPrefabs()
        {
            this.UIButton.onClick.AddListener(this.Button_Clicked);
            this.UI_Frame.AgreeJoinTeam = this.AgreeJoinTeam;
            this.UI_Frame.RejectJoinTeam = this.RejectJoinTeam;
        }

        /// <summary>
        /// Sự kiện khi Button được chọn
        /// </summary>
        private void Button_Clicked()
        {
            /// Hiện khung
            this.UI_Frame.Show();
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Quét liên tục
        /// </summary>
        /// <returns></returns>
        private IEnumerator DoScanContinuously()
        {
            /// Đợi 0.5s
            WaitForSeconds wait = new WaitForSeconds(0.5f);
            /// Lặp liên tục
            while (true)
            {
                /// Nếu bản thân đã có nhóm
                if (Global.Data.RoleData.TeamID != -1)
                {
                    /// Ẩn Button
                    this.UIButton.gameObject.SetActive(false);
                    /// Ẩn khung
                    this.UI_Frame.Hide();
                }
                /// Nếu bản thân chưa có nhóm
                else
                {
                    /// Nếu không có yêu cầu mời vào nhóm
                    if (Global.Data.InvitedToTeamPlayers.Count <= 0)
                    {
                        /// Ẩn Button
                        this.UIButton.gameObject.SetActive(false);
                        /// Ẩn khung
                        this.UI_Frame.Hide();
                    }
                    /// Nếu có yêu cầu mời vào nhóm
                    else
                    {
                        /// Nếu Button đang hiện sẵn
                        if (this.UIButton.gameObject.activeSelf)
                        {
                            /// Bỏ qua
                            goto SKIP;
                        }
                        /// Hiện Button
                        this.UIButton.gameObject.SetActive(true);
                    }
                }

                /// Bỏ qua
                SKIP:
                /// Đợi
                yield return wait;
            }
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Làm mới dữ liệu khung
        /// </summary>
        public void RefreshData()
        {
            this.UI_Frame.Refresh();
        }
        #endregion
    }
}
