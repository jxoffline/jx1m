using FS.GameEngine.Logic;
using FS.VLTK.UI.Main.GuildEx.MemberList;
using FS.VLTK.Utilities.UnityUI;
using Server.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using static FS.VLTK.Entities.Enum;

namespace FS.VLTK.UI.Main.GuildEx
{
    /// <summary>
    /// Khung thành viên bang hội
    /// </summary>
    public class UIGuildEx_MemberList : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Button đóng khung
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_Close;

        /// <summary>
        /// Prefab thông tin thành viên
        /// </summary>
        [SerializeField]
        private UIGuildEx_MemberList_MemberInfo UI_MemberInfoPrefab;

        /// <summary>
        /// Button phân trang - trang trước
        /// </summary>
        [SerializeField]
        private UIButtonSprite UIButton_Pagination_PreviousPage;

        /// <summary>
        /// Button phân trang - trang tiếp theo
        /// </summary>
        [SerializeField]
        private UIButtonSprite UIButton_Pagination_NextPage;

        /// <summary>
        /// Text phân trang - trang hiện tại
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_Pagination_CurrentPage;

        /// <summary>
        /// Text tên người chơi
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_RoleName;

        /// <summary>
        /// Text môn phái người chơi
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_RoleFaction;

        /// <summary>
        /// Text cấp độ người chơi
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_RoleLevel;

        /// <summary>
        /// Text cống hiến người chơi
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_RoleDedications;

        /// <summary>
        /// Text tài phú người chơi
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_RoleTotalValues;

        /// <summary>
        /// Text điểm hoạt động người chơi
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_RoleWeekPoint;

        /// <summary>
        /// Text chức vị người chơi
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_RoleGuildRank;

        /// <summary>
        /// Button bổ nhiệm
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_Approve;

        /// <summary>
        /// Button trục xuất
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_KickOut;

        /// <summary>
        /// Button mở khung danh sách xin gia nhập
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_OpenAskToJoinList;

        /// <summary>
        /// Khung bổ nhiệm chức vị
        /// </summary>
        [SerializeField]
        private UIGuildEx_MemberList_ApprovePopup UI_ApprovePopup;
        #endregion

        #region Properties
        private GuildMemberData _Data;
        /// <summary>
        /// Thông tin thành viên
        /// </summary>
        public GuildMemberData Data
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
        /// Sự kiện đóng khung
        /// </summary>
        public Action Close { get; set; }

        /// <summary>
        /// Sự kiện bổ nhiệm thành viên
        /// </summary>
        public Action<int, int> Approve { get; set; }

        /// <summary>
        /// Sự kiện trục xuất thành viên
        /// </summary>
        public Action<int> KickOut { get; set; }

        /// <summary>
        /// Sự kiện mở khung danh sách thành viên xin gia nhập bang hội
        /// </summary>
        public Action OpenAskToJoinList { get; set; }

        /// <summary>
        /// Sự kiện truy vấn danh sách thành viên theo trang
        /// </summary>
        public Action<int> QueryMemberList { get; set; }
        #endregion

        #region Private fields
        /// <summary>
        /// RectTransform danh sách thành viên
        /// </summary>
        private RectTransform transformMemberList;

        /// <summary>
        /// Thành viên được chọn
        /// </summary>
        private GuildMember selectedMember;
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi khi đối tượng được tạo ra
        /// </summary>
        private void Awake()
        {
            this.transformMemberList = this.UI_MemberInfoPrefab.transform.parent.GetComponent<RectTransform>();
        }

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
            this.UIButton_Approve.onClick.AddListener(this.ButtonApprove_Clicked);
            this.UIButton_KickOut.onClick.AddListener(this.ButtonKickOut_Clicked);
            this.UIButton_OpenAskToJoinList.onClick.AddListener(this.ButtonOpenAskToJoinList_Clicked);
            this.UIButton_Pagination_NextPage.Click = this.ButtonPaginationNextPage_Clicked;
            this.UIButton_Pagination_PreviousPage.Click = this.ButtonPaginationPreviousPage_Clicked;
        }

        /// <summary>
        /// Sự kiện khi Button đóng khung được ấn
        /// </summary>
        private void ButtonClose_Clicked()
        {
            this.Close?.Invoke();
        }

        /// <summary>
        /// Sự kiện khi Button bổ nhiệm được ấn
        /// </summary>
        private void ButtonApprove_Clicked()
        {
            /// Nếu không có bang
            if (Global.Data.RoleData.GuildID == -1)
            {
                KTGlobal.AddNotification("Bạn không có bang hội, không thể thực hiện trục xuất thành viên!");
                return;
            }
            /// Nếu chưa chọn thành viên
            else if (this.selectedMember == null)
            {
                KTGlobal.AddNotification("Hãy chọn một thành viên sau đó tiến hành thao tác!");
                return;
            }
            /// Nếu là chính mình
            else if (this.selectedMember.RoleID == Global.Data.RoleData.RoleID)
            {
                KTGlobal.AddNotification("Không thể tự bổ nhiệm chính mình!");
                return;
            }

            /// Danh sách có thể bổ nhiệm
            List<GuildRank> approvableRanks = KTGlobal.GetGuildApprovableRanks((GuildRank) Global.Data.RoleData.GuildRank);

            /// Nếu không thể bổ nhiệm người chơi này này
            if (!approvableRanks.Contains((GuildRank) this.selectedMember.Rank))
            {
                KTGlobal.AddNotification("Bạn không có quyền bổ nhiệm người chơi này!");
                return;
            }
            /// Không có
            else if (approvableRanks.Count <= 0)
            {
                KTGlobal.AddNotification("Bạn không có quyền bổ nhiệm người chơi này!");
                return;
            }

            /// Hiện khung
            this.UI_ApprovePopup.Show();
            this.UI_ApprovePopup.ApprovableRanks = approvableRanks;
            this.UI_ApprovePopup.Approve = (guildRank) =>
            {
                /// Nếu không thể bổ nhiệm người chơi này này
                if (!approvableRanks.Contains((GuildRank) this.selectedMember.Rank))
                {
                    KTGlobal.AddNotification("Bạn không có quyền bổ nhiệm người chơi này!");
                    return;
                }

                /// Xác nhận lại
                KTGlobal.ShowMessageBox("Bổ nhiệm thành viên", string.Format("Xác nhận bổ nhiệm thành viên <color=#51aef0>[{0}]</color> làm <color=green>{1}</color>?", this.selectedMember.RoleName, KTGlobal.GetGuildRankName((int) guildRank)), () =>
                {
                    /// Thực thi sự kiện
                    this.Approve?.Invoke(this.selectedMember.RoleID, (int) guildRank);
                });
            };
        }

        /// <summary>
        /// Sự kiện khi Button trục xuất thành viên được ấn
        /// </summary>
        private void ButtonKickOut_Clicked()
        {
            /// Nếu không có bang
            if (Global.Data.RoleData.GuildID == -1)
            {
                KTGlobal.AddNotification("Bạn không có bang hội, không thể thực hiện trục xuất thành viên!");
                return;
            }
            /// Nếu không phải bang chủ hoặc phó bang chủ
            else if (Global.Data.RoleData.GuildRank != (int) GuildRank.Master && Global.Data.RoleData.GuildRank != (int) GuildRank.ViceMaster)
            {
                KTGlobal.AddNotification("Chỉ có bang chủ hoặc phó bang chủ mới có thể thực hiện trục xuất thành viên!");
                return;
            }
            /// Nếu chưa chọn thành viên
            else if (this.selectedMember == null)
            {
                KTGlobal.AddNotification("Hãy chọn một thành viên sau đó tiến hành thao tác!");
                return;
            }
            /// Nếu là chính mình
            else if (this.selectedMember.RoleID == Global.Data.RoleData.RoleID)
            {
                KTGlobal.AddNotification("Không thể tự trục xuất chính mình!");
                return;
            }
            /// Danh sách có thể bổ nhiệm
            List<GuildRank> approvableRanks = KTGlobal.GetGuildApprovableRanks((GuildRank) Global.Data.RoleData.GuildRank);

            /// Nếu không thể trục xuất người chơi này này
            if (!approvableRanks.Contains((GuildRank) this.selectedMember.Rank))
            {
                KTGlobal.AddNotification("Bạn không có quyền trục xuất người chơi này!");
                return;
            }
            /// Không có
            else if (approvableRanks.Count <= 0)
            {
                KTGlobal.AddNotification("Bạn không có quyền trục xuất người chơi này!");
                return;
            }

            /// Xác nhận
            KTGlobal.ShowMessageBox("Trục xuất thành viên", string.Format("Xác định trục xuất thành viên <color=#51aef0>[{0}]</color> khỏi bang hội vĩnh viễn?", this.selectedMember.RoleName), () =>
            {
                /// Thực thi sự kiện
                this.KickOut?.Invoke(this.selectedMember.RoleID);
            }, true);
        }

        /// <summary>
        /// Sự kiện khi Button mở khung danh sách thành viên xin vào bang được ấn
        /// </summary>
        private void ButtonOpenAskToJoinList_Clicked()
        {
            /// Nếu không có bang
            if (Global.Data.RoleData.GuildID == -1)
            {
                KTGlobal.AddNotification("Bạn không có bang hội, không thể thực hiện thao tác!");
                return;
            }

            this.OpenAskToJoinList?.Invoke();
        }

        /// <summary>
        /// Sự kiện khi Button phân trang - trang kế tiếp được ấn
        /// </summary>
        private void ButtonPaginationNextPage_Clicked()
        {
            /// Toác gì đó
            if (this._Data == null)
            {
                return;
            }
            /// Nếu là trang cuối
            else if (this._Data.PageIndex >= this._Data.TotalPage)
            {
                return;
            }

            /// Ẩn Button
            this.UIButton_Pagination_PreviousPage.Enable = false;
            this.UIButton_Pagination_NextPage.Enable = false;
            /// Thực hiện truy vấn
            this.QueryMemberList?.Invoke(this._Data.PageIndex + 1);
        }

        /// <summary>
        /// Sự kiện khi Button phân trang - trang trước đó được ấn
        /// </summary>
        private void ButtonPaginationPreviousPage_Clicked()
        {

            if (this._Data == null)
            {
                return;
            }
            /// Nếu là trang đầu tiên
            else if (this._Data.PageIndex <= 1)
            {
                return;
            }

            /// Ẩn Button
            this.UIButton_Pagination_PreviousPage.Enable = false;
            this.UIButton_Pagination_NextPage.Enable = false;
            /// Thực hiện truy vấn
            this.QueryMemberList?.Invoke(this._Data.PageIndex - 1);
            /// Toác gì đó
          
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Luồng thực thi sự kiện bỏ qua một số Frame
        /// </summary>
        /// <param name="skip"></param>
        /// <param name="work"></param>
        /// <returns></returns>
        private IEnumerator DoExecuteSkipFrames(int skip, Action work)
        {
            for (int i = 1; i <= skip; i++)
            {
                yield return null;
            }
            work?.Invoke();
        }

        /// <summary>
        /// Thực thi sự kiện bỏ qua một số Frame
        /// </summary>
        /// <param name="skip"></param>
        /// <param name="work"></param>
        private void ExecuteSkipFrames(int skip, Action work)
        {
            this.StartCoroutine(this.DoExecuteSkipFrames(skip, work));
        }

        /// <summary>
        /// Làm mới dữ liệu
        /// </summary>
        private void RefreshData()
        {
            /// Nếu không có bang
            if (Global.Data.RoleData.GuildID == -1)
            {
                /// Đóng khung
                this.Close?.Invoke();
                return;
            }
            /// Toác
            else if (this._Data == null)
            {
                /// Đóng khung
                this.Close?.Invoke();
                return;
            }

            /// Làm rỗng danh sách thành viên
            foreach (Transform child in this.transformMemberList.transform)
            {
                if (child.gameObject != this.UI_MemberInfoPrefab.gameObject)
                {
                    GameObject.Destroy(child.gameObject);
                }
            }
            /// Làm rỗng Text
            this.UIText_RoleName.text = "";
            this.UIText_RoleFaction.text = "";
            this.UIText_RoleLevel.text = "";
            this.UIText_RoleDedications.text = "";
            this.UIText_RoleTotalValues.text = "";
            this.UIText_RoleWeekPoint.text = "";
            this.UIText_RoleGuildRank.text = "";

            /// Thành viên đầu tiên
            UIGuildEx_MemberList_MemberInfo uiFirstMember = null;

            /// Duyệt danh sách thành viên
            foreach (GuildMember memberInfo in this._Data.TotalGuildMember)
            {
                /// Tạo mới
                UIGuildEx_MemberList_MemberInfo uiMemberInfo = GameObject.Instantiate<UIGuildEx_MemberList_MemberInfo>(this.UI_MemberInfoPrefab);
                uiMemberInfo.transform.SetParent(this.transformMemberList, false);
                uiMemberInfo.gameObject.SetActive(true);
                uiMemberInfo.Data = memberInfo;
                uiMemberInfo.Select = () =>
                {
                    /// Đánh dấu thành viên được chọn
                    this.selectedMember = uiMemberInfo.Data;
                    /// Cập nhật Text
                    this.UIText_RoleName.text = this.selectedMember.RoleName;
                    this.UIText_RoleFaction.text = KTGlobal.GetFactionName(this.selectedMember.FactionID, out Color factionColor);
                    this.UIText_RoleFaction.color = factionColor;
                    this.UIText_RoleLevel.text = this.selectedMember.Level.ToString();
                    this.UIText_RoleDedications.text = this.selectedMember.GuildMoney.ToString();
                    this.UIText_RoleTotalValues.text = this.selectedMember.TotalValue.ToString();
                    this.UIText_RoleWeekPoint.text = this.selectedMember.WeekPoint.ToString();
                    this.UIText_RoleGuildRank.text = KTGlobal.GetGuildRankName(this.selectedMember.Rank);
                    /// Nếu là chính mình
                    if (Global.Data.RoleData.RoleID == this.selectedMember.RoleID)
                    {
                        /// Mặc định chưa cho trục xuất
                        this.UIButton_KickOut.gameObject.SetActive(false);
                        /// Mặc định chưa cho bổ nhiệm
                        this.UIButton_Approve.gameObject.SetActive(false);
                    }
                    else
                    {
                        /// Bổ nhiệm theo chức vị bên dưới
                        this.UIButton_Approve.gameObject.SetActive(KTGlobal.GetGuildApprovableRanks((GuildRank) Global.Data.RoleData.GuildRank).Contains((GuildRank) this.selectedMember.Rank));
                        /// Nếu là bang chủ hoặc phó bang chủ thì cho phép trục xuất
                        this.UIButton_KickOut.gameObject.SetActive((Global.Data.RoleData.GuildRank == (int) GuildRank.Master || Global.Data.RoleData.GuildRank == (int) GuildRank.ViceMaster) && KTGlobal.GetGuildApprovableRanks((GuildRank) Global.Data.RoleData.GuildRank).Contains((GuildRank) this.selectedMember.Rank));
                    }
                };

                /// Nếu chưa có thành viên đầu tiên
                if (uiFirstMember == null)
                {
                    /// Đánh dấu thành viên đầu tiên
                    uiFirstMember = uiMemberInfo;
                    /// Chọn thằng đầu tiên
                    uiFirstMember.Active = true;
                }
            }
            /// Xây lại giao diện
            this.ExecuteSkipFrames(1, () =>
            {
                UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(this.transformMemberList);
            });

            this.UIText_Pagination_CurrentPage.text = string.Format("{0}/{1}", this.Data.PageIndex, this.Data.TotalPage);

            /// Tương tác 2 Button phân trang
            this.UIButton_Pagination_PreviousPage.Enable = this._Data.PageIndex > 1;
            this.UIButton_Pagination_NextPage.Enable = this._Data.PageIndex < this._Data.TotalPage;

            /// Mặc định chưa cho trục xuất
            this.UIButton_KickOut.gameObject.SetActive(false);
            /// Mặc định chưa cho bổ nhiệm
            this.UIButton_Approve.gameObject.SetActive(false);
        }
        #endregion
    }
}
