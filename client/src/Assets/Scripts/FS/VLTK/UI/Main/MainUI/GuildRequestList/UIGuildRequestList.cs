using FS.GameEngine.Logic;
using FS.VLTK.UI.Main.MainUI.GuildRequestList;
using System;
using System.Collections;
using UnityEngine;
using static FS.VLTK.Entities.Enum;

namespace FS.VLTK.UI.Main.MainUI
{
    /// <summary>
    /// Danh sách mời vào nhóm
    /// </summary>
    public class UIGuildRequestList : MonoBehaviour
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
        private UIGuildRequestList_Frame UI_Frame;
        #endregion

        #region Properties
        /// <summary>
        /// Sự kiện đồng ý
        /// </summary>
        public Action<int, int> Agree { get; set; }

        /// <summary>
        /// Sự kiện từ chối
        /// </summary>
        public Action<int, int> Reject { get; set; }
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
            this.UI_Frame.Agree = this.Agree;
            this.UI_Frame.Reject = this.Reject;
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
                /// Nếu bản thân đã có bang
                if (Global.Data.RoleData.GuildID > 0)
                {
                    /// Nếu không phải bang chủ hoặc phó bang chủ
                    if (Global.Data.RoleData.GuildRank != (int) GuildRank.Master && Global.Data.RoleData.GuildRank != (int) GuildRank.ViceMaster)
                    {
                        /// Ẩn Button
                        this.UIButton.gameObject.SetActive(false);
                        /// Ẩn khung
                        this.UI_Frame.Hide();
                    }
                    /// Nếu là bang chủ hoặc phó bang chủ
                    else
                    {
                        /// Nếu không có yêu cầu xin vào bang
                        if (Global.Data.GuildRequestJoinPlayers.Count <= 0)
                        {
                            /// Ẩn Button
                            this.UIButton.gameObject.SetActive(false);
                            /// Ẩn khung
                            this.UI_Frame.Hide();
                        }
                        /// Nếu có yêu cầu xin vào bang
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
                }
                /// Nếu bản thân chưa có bang
                else
                {
                    /// Nếu không có yêu cầu mời vào bang
                    if (Global.Data.GuildInvitationPlayers.Count <= 0)
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
