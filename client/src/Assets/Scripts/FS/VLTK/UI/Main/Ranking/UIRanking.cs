using FS.VLTK.UI.Main.Ranking;
using FS.VLTK.Utilities.UnityUI;
using Server.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using static FS.VLTK.Entities.Enum;

namespace FS.VLTK.UI.Main
{
    /// <summary>
    /// Khung bảng xếp hạng
    /// </summary>
    public class UIRanking : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Button đóng khung
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_Close;

        /// <summary>
        /// Prefab thông tin người chơi
        /// </summary>
        [SerializeField]
        private UIRanking_PlayerInfo UI_PlayerInfoPrefab;

        /// <summary>
        /// Prefab Tab xếp hạng
        /// </summary>
        [SerializeField]
        private UIRanking_RankTab UI_RankTabPrefab;

        /// <summary>
        /// Text Label giá trị
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_ValueLabel;

        /// <summary>
        /// Button trang tiếp theo
        /// </summary>
        [SerializeField]
        private UIButtonSprite UIButton_NextPage;

        /// <summary>
        /// Button trang trước
        /// </summary>
        [SerializeField]
        private UIButtonSprite UIButton_PreviousPage;

        /// <summary>
        /// Text số trang
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_PageNumber;

        /// <summary>
        /// Text thứ hạng bản thân
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_SelfRank;
        #endregion

        #region Constants
        /// <summary>
        /// Số người chơi tối đa mỗi trang
        /// </summary>
        private const int MaxPlayersEachPage = 10;
        #endregion

        #region Private fields
        /// <summary>
        /// RectTransform danh sách người chơi
        /// </summary>
        private RectTransform transformPlayersList = null;

        /// <summary>
        /// RectTransform danh sách Tab
        /// </summary>
        private RectTransform transformTabsList = null;

        /// <summary>
        /// Trang hiện tại
        /// </summary>
        private int currentPage = 1;
        #endregion

        #region Properties
        /// <summary>
        /// Sự kiện đóng khung
        /// </summary>
        public Action Close { get; set; }

        /// <summary>
        /// Thông tin xếp hạng tương ứng
        /// </summary>
        public Server.Data.Ranking Data { get; set; }

        /// <summary>
        /// Truy vấn thông tin xếp hạng tương ứng
        /// </summary>
        public Action<int, int> QueryPage { get; set; }
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi khi đối tượng được tạo ra
        /// </summary>
        private void Awake()
        {
            this.transformPlayersList = this.UI_PlayerInfoPrefab.transform.parent.GetComponent<RectTransform>();
            this.transformTabsList = this.UI_RankTabPrefab.transform.parent.GetComponent<RectTransform>();
        }

        /// <summary>
        /// Hàm này gọi ở Frame đầu tiên
        /// </summary>
        private void Start()
        {
            this.MakeDefaultSlots();
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
            this.UIButton_NextPage.Click = this.ButtonNextPage_Clicked;
            this.UIButton_PreviousPage.Click = this.ButtonPreviousPage_Clicked;

            /// Tạo mới các Tab mặc định
            this.BuidTab((int) RankMode.CapDo, "Cấp độ", null, "Kinh nghiệm");
            this.BuidTab((int) RankMode.TaiPhu, "Tài phú", null, "Tài phú");
            //this.BuidTab((int) RankMode.VoLam, "Võ Lâm", null, "Vinh dự");
            //this.BuidTab((int) RankMode.LienDau, "Liên đấu", null, "Điểm");
            //this.BuidTab((int) RankMode.UyDanh, "Uy danh", null, "Uy danh");
            //this.BuidTab(-1, "Môn phái", new Dictionary<int, string>() {
            //    { (int) RankMode.ThienVuong, "Thiên Vương" },
            //    { (int) RankMode.ThieuLam, "Thiếu Lâm" },
            //    { (int) RankMode.CaiBang, "Cái Bang" },
            //    { (int) RankMode.ThienNhan, "Thiên Nhẫn" },
            //    { (int) RankMode.NgaMy, "Nga My" },
            //    { (int) RankMode.ThuyYen, "Thúy Yên" },
            //    { (int) RankMode.DoanThi, "Đoàn Thị" },
            //    { (int) RankMode.VoDang, "Võ Đang" },
            //    { (int) RankMode.ConLon, "Côn Lôn" },
            //    { (int) RankMode.NguDoc, "Ngũ Độc" },
            //    { (int) RankMode.MinGiao, "Minh Giáo" },
            //    { (int) RankMode.DuongMon, "Đường Môn" },
            //}, "Điểm");
            /// Xây lại giao diện Tab
            this.RebuildTabsLayout();
            /// Cập nhật Text thứ hạng bản thân
            this.UIText_SelfRank.text = "0/0";
            /// Tự chọn Tab đầu tiên
            this.ButtonRankType_Clicked((int) RankMode.CapDo);
        }

        /// <summary>
        /// Sự kiện khi Button đóng khung được ấn
        /// </summary>
        private void ButtonClose_Clicked()
        {
            this.Close?.Invoke();
        }

        /// <summary>
        /// Sự kiện khi Button trang tiếp theo được ấn
        /// </summary>
        private void ButtonNextPage_Clicked()
        {
            /// Nếu không có dữ liệu
            if (this.Data == null)
            {
                return;
            }

            /// Tổng số trang
            int totalPages = this.Data.TotalPlayers / UIRanking.MaxPlayersEachPage;
            if (this.Data.TotalPlayers % UIRanking.MaxPlayersEachPage != 0)
            {
                totalPages++;
            }

            /// Nếu trang hiện tại đã ở cuối
            if (this.currentPage >= totalPages)
            {
                return;
            }

            /// Truy vấn trang kế tiếp
            this.QueryPage?.Invoke(this.Data.Type, ++this.currentPage);

            /// Ẩn Button trang
            this.DisablePageButtons();
        }

        /// <summary>
        /// Sự kiện khi Button trang trước được ấn
        /// </summary>
        private void ButtonPreviousPage_Clicked()
        {
            /// Nếu không có dữ liệu
            if (this.Data == null)
            {
                return;
            }

            /// Nếu trang hiện tại đã ở đầu tiên
            if (this.currentPage <= 1)
            {
                return;
            }

            /// Truy vấn trang trước đó
            this.QueryPage?.Invoke(this.Data.Type, --this.currentPage);

            /// Ẩn Button trang
            this.DisablePageButtons();
        }

        /// <summary>
        /// Sự kiện khi Button loại xếp hạng được ấn
        /// </summary>
        /// <param name="rankType"></param>
        private void ButtonRankType_Clicked(int rankType)
        {
            /// Nếu đây là Tab lớn
            if (rankType == -1)
            {
                return;
            }

            /// Đánh dấu trang hiện tại là 1
            this.currentPage = 1;
            /// Truy vấn trang trước đó
            this.QueryPage?.Invoke(rankType, this.currentPage);

            /// Ẩn Button trang
            this.DisablePageButtons();
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Thực thi sự kiện bỏ qua một số Frame
        /// </summary>
        /// <param name="skip"></param>
        /// <param name="work"></param>
        /// <returns></returns>
        private IEnumerator ExecuteSkipFrames(int skip, Action work)
        {
            for (int i = 1; i <= skip; i++)
            {
                yield return null;
            }
            work?.Invoke();
        }

        /// <summary>
        /// Xây lại giao diện
        /// </summary>
        /// <param name="rectTransform"></param>
        private void RebuildLayout(RectTransform rectTransform)
        {
            /// Nếu đối tượng không được kích hoạt
            if (!this.gameObject.activeSelf)
            {
                return;
            }

            /// Xây lại giao diện ở Frame tiếp theo
            this.StartCoroutine(this.ExecuteSkipFrames(1, () => {
                UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
            }));
        }

        /// <summary>
        /// Xây lại giao diện danh sách người chơi
        /// </summary>
        private void RebuildPlayersListLayout()
        {
            this.RebuildLayout(this.transformPlayersList);
        }

        /// <summary>
        /// Xây lại danh sách Tab
        /// </summary>
        private void RebuildTabsLayout()
        {
            this.RebuildLayout(this.transformTabsList);
        }

        /// <summary>
        /// Xây Tab tương ứng
        /// </summary>
        /// <param name="rankType"></param>
        /// <param name="rankName"></param>
        /// <param name="subRanks"></param>
        /// <param name="valueLabel"></param>
        private void BuidTab(int rankType, string rankName, Dictionary<int, string> subRanks, string valueLabel)
        {
            UIRanking_RankTab uiTab = GameObject.Instantiate<UIRanking_RankTab>(this.UI_RankTabPrefab);
            uiTab.transform.SetParent(this.transformTabsList, false);
            uiTab.gameObject.SetActive(true);
            uiTab.RankType = rankType;
            uiTab.RankName = rankName;
            uiTab.SubRankTypes = subRanks;
            uiTab.Click = (nType) => {
                this.RebuildTabsLayout();
                this.UIText_ValueLabel.text = valueLabel;
                this.ButtonRankType_Clicked(nType);
            };
        }

        /// <summary>
        /// Tạo mặc định các vị trí tương ứng
        /// </summary>
        private void MakeDefaultSlots()
        {
            for (int i = 1; i <= UIRanking.MaxPlayersEachPage; i++)
            {
                UIRanking_PlayerInfo uiPlayerInfo = GameObject.Instantiate<UIRanking_PlayerInfo>(this.UI_PlayerInfoPrefab);
                uiPlayerInfo.transform.SetParent(this.transformPlayersList, false);
                uiPlayerInfo.gameObject.SetActive(false);
                uiPlayerInfo.Data = null;
            }
        }

        /// <summary>
        /// Làm rỗng danh sách người chơi
        /// </summary>
        private void ClearPlayers()
        {
            foreach (Transform child in this.transformPlayersList.transform)
            {
                if (child.gameObject != this.UI_PlayerInfoPrefab.gameObject)
                {
                    GameObject.Destroy(child.gameObject);
                }
            }
        }

        /// <summary>
        /// Thêm người chơi vào danh sách
        /// </summary>
        /// <param name="playerRanking"></param>
        private void AddPlayer(PlayerRanking playerRanking)
        {
            UIRanking_PlayerInfo uiPlayerInfo = GameObject.Instantiate<UIRanking_PlayerInfo>(this.UI_PlayerInfoPrefab);
            uiPlayerInfo.transform.SetParent(this.transformPlayersList, false);
            uiPlayerInfo.gameObject.SetActive(true);
            uiPlayerInfo.Data = playerRanking;
        }

        /// <summary>
        /// Làm mới trạng thái Button trang
        /// </summary>
        private void RefreshPageButtonsState()
        {
            if (this.Data == null || this.Data.Players == null)
            {
                this.UIButton_PreviousPage.Enable = false;
                this.UIButton_NextPage.Enable = false;
            }

            /// Nếu trang hiện tại đã ở đầu tiên
            if (this.currentPage <= 1)
            {
                this.UIButton_PreviousPage.Enable = false;
            }
            else
            {
                this.UIButton_PreviousPage.Enable = true;
            }

            /// Tổng số trang
            int totalPages = this.Data.TotalPlayers / UIRanking.MaxPlayersEachPage;
            if (this.Data.TotalPlayers % UIRanking.MaxPlayersEachPage != 0)
            {
                totalPages++;
            }

            /// Nếu trang hiện tại đã ở cuối
            if (this.currentPage >= totalPages)
            {
                this.UIButton_NextPage.Enable = false;
            }
            else
            {
                this.UIButton_NextPage.Enable = true;
            }
        }

        /// <summary>
        /// Ẩn toàn bộ Button phân trang
        /// </summary>
        private void DisablePageButtons()
        {
            this.UIButton_PreviousPage.Enable = false;
            this.UIButton_NextPage.Enable = false;
        }

        /// <summary>
        /// Làm mới hiển thị
        /// </summary>
        private void Refresh()
        {
            /// Làm rỗng danh sách người chơi
            this.ClearPlayers();

            if (this.Data.Players != null)
            {
                /// Duyệt danh sách người chơi
                for (int i = 0; i < this.Data.Players.Count - 1; i++)
                {
                    PlayerRanking playerInfo = this.Data.Players[i];
                    this.AddPlayer(playerInfo);
                }

                /// Tổng số trang
                int totalPages = this.Data.TotalPlayers / UIRanking.MaxPlayersEachPage;
                if (this.Data.TotalPlayers % UIRanking.MaxPlayersEachPage != 0)
                {
                    totalPages++;
                }

                /// Thứ hạng bản thân
                int selfRank = this.Data.Players.LastOrDefault().ID;
                if (selfRank == -1000)
                {
                    this.UIText_SelfRank.text = "Không xếp hạng";
                }
                else
                {
                    this.UIText_SelfRank.text = string.Format("{0}/{1}", selfRank + 1, this.Data.TotalPlayers);
                }

                /// Cập nhật Text số trang
                this.UIText_PageNumber.text = string.Format("{0}/{1}", this.currentPage, totalPages);
            }
            else
            {
                this.UIText_SelfRank.text = "Không xếp hạng";
                /// Cập nhật Text số trang
                this.UIText_PageNumber.text = string.Format("{0}/{1}", 1, 1);
            }

            /// Xây lại giao diện
            this.RebuildPlayersListLayout();
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Làm mới dữ liệu
        /// </summary>
        public void RefreshData()
        {
            this.Refresh();
            this.RefreshPageButtonsState();
        }
        #endregion
    }
}
