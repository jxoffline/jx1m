using FS.GameEngine.Logic;
using FS.VLTK.Utilities.UnityUI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static FS.VLTK.Entities.Enum;

namespace FS.VLTK.UI.Main.GuildEx.MemberList
{
    /// <summary>
    /// Khung danh sách chức vị bổ nhiệm thành viên
    /// </summary>
    public class UIGuildEx_MemberList_ApprovePopup : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Prefab Toggle chức vị
        /// </summary>
        [SerializeField]
        private UIToggleSprite UIToggle_RankPrefab;

        /// <summary>
        /// Button xác nhận
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_Accept;

        /// <summary>
        /// Button đóng khung
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_Close;
        #endregion

        #region Properties
        private List<GuildRank> _ApprovableRanks;
        /// <summary>
        /// Danh sách chức vị có thể bổ nhiệm
        /// </summary>
        public List<GuildRank> ApprovableRanks
        {
            get
            {
                return this._ApprovableRanks;
            }
            set
            {
                this._ApprovableRanks = value;
                /// Làm mới dữ liệu
                this.RefreshData();
            }
        }

        /// <summary>
        /// Sự kiện bổ nhiệm chức vị
        /// </summary>
        public Action<GuildRank> Approve { get; set; }
        #endregion

        #region Private fields
        /// <summary>
        /// RectTransform danh sách chức vị
        /// </summary>
        private RectTransform transformRankList;

        /// <summary>
        /// Chức vị được chọn
        /// </summary>
        private GuildRank selectedRank = GuildRank.Count;
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi khi đối tượng được tạo ra
        /// </summary>
        private void Awake()
        {
            this.transformRankList = this.UIToggle_RankPrefab.transform.parent.GetComponent<RectTransform>();
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
            this.UIButton_Accept.onClick.AddListener(this.ButtonAccept_Clicked);
        }

        /// <summary>
        /// Sự kiện khi Button đóng khung được ấn
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
            /// Nếu không có bang
            if (Global.Data.RoleData.GuildID <= 0)
            {
                KTGlobal.AddNotification("Bạn không có bang hội, không thể thực hiện thao tác này!");
                return;
            }
            /// Nếu không phải bang chủ hoặc phó bang chủ
            else if (Global.Data.RoleData.GuildRank != (int) GuildRank.Master && Global.Data.RoleData.GuildRank != (int) GuildRank.ViceMaster)
            {
                KTGlobal.AddNotification("Chỉ có bang chủ hoặc phó bang chủ mới có thể thực hiện thao tác này!");
                return;
            }
            /// Nếu chưa có chức vị nào được chọn
            else if (this.selectedRank == GuildRank.Count)
            {
                KTGlobal.AddNotification("Hãy chọn một chức vị muốn bổ nhiệm!");
                return;
            }

            /// Thực thi sự kiện
            this.Approve?.Invoke(this.selectedRank);
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
            /// Toác
            if (this._ApprovableRanks == null || this._ApprovableRanks.Count <= 0)
            {
                /// Ẩn khung
                this.Hide();
                return;
            }

            /// Toggle đầu tiên
            UIToggleSprite uiFirstButton = null;

            /// Làm rỗng danh sách
            foreach (Transform child in this.transformRankList.transform)
            {
                if (child.gameObject != this.UIToggle_RankPrefab.gameObject)
                {
                    GameObject.Destroy(child.gameObject);
                }
            }
            /// Duyệt danh sách chức vị có thể bổ nhiệm
            foreach (GuildRank guildRank in this._ApprovableRanks)
            {
                /// Tạo mới
                UIToggleSprite uiRankButton = GameObject.Instantiate<UIToggleSprite>(this.UIToggle_RankPrefab);
                uiRankButton.transform.SetParent(this.transformRankList, false);
                uiRankButton.gameObject.SetActive(true);
                uiRankButton.Name = KTGlobal.GetGuildRankName((int) guildRank);
                uiRankButton.OnSelected = (isSelected) =>
                {
                    /// Đánh dấu chức vị được chọn
                    this.selectedRank = guildRank;
                };

                /// Nếu chưa đánh dấu Toggle đầu tiên
                if (uiFirstButton == null)
                {
                    /// Đánh dấu Toggle đầu tiên
                    uiFirstButton = uiRankButton;
                }
            }
            /// Xây lại giao diện
            this.ExecuteSkipFrames(1, () =>
            {
                UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(this.transformRankList);
            });

            /// Chọn Toggle đầu tiên
            this.ExecuteSkipFrames(2, () =>
            {
                /// Toác
                if (uiFirstButton == null)
                {
                    return;
                }

                uiFirstButton.Active = true;
            });
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
