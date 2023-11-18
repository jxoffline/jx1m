using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using FS.VLTK.UI.Main.Friend;
using FS.VLTK.Utilities.UnityUI;
using Server.Data;
using System.Collections;
using FS.GameEngine.Logic;

namespace FS.VLTK.UI.Main
{
    /// <summary>
    /// Khung bạn bè
    /// </summary>
    public class UIFriendBox : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Button đóng khung
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_Close;

        /// <summary>
        /// Toggle danh sách bạn bè
        /// </summary>
        [SerializeField]
        private UIToggleSprite UIToggle_FriendList;

        /// <summary>
        /// Toggle danh sách đen
        /// </summary>
        [SerializeField]
        private UIToggleSprite UIToggle_BlackList;

        /// <summary>
        /// Toggle danh sách kẻ thù
        /// </summary>
        [SerializeField]
        private UIToggleSprite UIToggle_EnemyList;

        /// <summary>
        /// Prefab thông tin người chơi
        /// </summary>
        [SerializeField]
        private UIFriendBox_PlayerInfo UI_PlayerInfoPrefab;

        /// <summary>
        /// Khung tương tác với người chơi được chọn
        /// </summary>
        [SerializeField]
        private UIFriendBox_InteractionBox UI_InteractionBox;

        /// <summary>
        /// Khung danh sách chờ thêm bạn
        /// </summary>
        [SerializeField]
        private UIFriendBox_AskToBeFriendFrame UI_AskToBeFriendFrame;

        /// <summary>
        /// Text trang hiện tại
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_CurrentPage;

        /// <summary>
        /// Button trang trước
        /// </summary>
        [SerializeField]
        private UIButtonSprite UIButton_PreviousPage;

        /// <summary>
        /// Button trang kế
        /// </summary>
        [SerializeField]
        private UIButtonSprite UIButton_NextPage;

        /// <summary>
        /// Button mở khung chờ thêm bạn
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_AskToBeFriendList;
        #endregion

        #region Constants
        /// <summary>
        /// Số phần tử tối đa trong 1 trang
        /// </summary>
        private const int MaxElement = 6;
        #endregion

        #region Private fields
        /// <summary>
        /// RectTransform danh sách bạn bè
        /// </summary>
        private RectTransform transformFriendList = null;

        /// <summary>
        /// Trang hiện tại
        /// </summary>
        private int currentPage = -1;

        /// <summary>
        /// Tổng số trang
        /// </summary>
        private int maxPage = -1;

        /// <summary>
        /// Loại hiện tại
        /// </summary>
        private int currentType = -1;

        /// <summary>
        /// Danh sách bạn bè ở loại hiện tại
        /// </summary>
        private List<FriendData> currentFriends = null;
        #endregion

        #region Properties
        /// <summary>
        /// Sự kiện đóng khung
        /// </summary>
        public Action Close { get; set; }

        /// <summary>
        /// Sự kiện thêm bạn
        /// </summary>
        public Action<RoleDataMini> AddFriend { get; set; }

        /// <summary>
        /// Sự kiện từ chối thêm bạn
        /// </summary>
        public Action<RoleDataMini> RejectFriend { get; set; }

        /// <summary>
        /// Sự kiện xóa bạn
        /// </summary>
        public Action<FriendData> RemoveFriend { get; set; }

        /// <summary>
        /// Sự kiện xem thông tin bạn
        /// </summary>
        public Action<FriendData> BrowseFriendInfo { get; set; }
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi khi đối tượng được tạo ra
        /// </summary>
        private void Awake()
        {
            this.transformFriendList = this.UI_PlayerInfoPrefab.transform.parent.GetComponent<RectTransform>();
        }

        /// <summary>
        /// Hàm này gọi ở Frame đầu tiên
        /// </summary>
        private void Start()
        {
            this.InitPrefabs();
            this.BuildLayout();
        }
        #endregion

        #region Code UI
        /// <summary>
        /// Khởi tạo ban đầu
        /// </summary>
        private void InitPrefabs()
        {
            this.UIButton_Close.onClick.AddListener(this.ButtonClose_Clicked);
            this.UIToggle_FriendList.OnSelected = this.ToggleFriendList_Clicked;
            this.UIToggle_BlackList.OnSelected = this.ToggleBlackList_Clicked;
            this.UIToggle_EnemyList.OnSelected = this.ToggleEnemyList_Clicked;
            this.UIButton_AskToBeFriendList.onClick.AddListener(this.ButtonOpenAskToBeFriendFrame_Clicked);
            this.UIButton_PreviousPage.Click = this.ButtonPreviousPage_Clicked;
            this.UIButton_NextPage.Click = this.ButtonNextPage_Clicked;
        }

        /// <summary>
        /// Sự kiện khi Button đóng khung được ấn
        /// </summary>
        private void ButtonClose_Clicked()
        {
            this.Close?.Invoke();
        }

        /// <summary>
        /// Sự kiện khi Button mở khung danh sách chờ thêm bạn được ấn
        /// </summary>
        private void ButtonOpenAskToBeFriendFrame_Clicked()
        {
            this.UI_AskToBeFriendFrame.AddFriend = this.AddFriend;
            this.UI_AskToBeFriendFrame.RejectFriend = this.RejectFriend;
            this.UI_AskToBeFriendFrame.Show();
        }

        /// <summary>
        /// Sự kiện khi Toggle danh sách bạn bè được chọn
        /// </summary>
        /// <param name="isSelected"></param>
        private void ToggleFriendList_Clicked(bool isSelected)
        {
            if (isSelected)
            {
                this.currentType = 0;
                /// Số trang hiện tại
                this.currentPage = 1;
                /// Nếu chưa có danh sách bạn bè
                if (Global.Data.FriendDataList == null)
                {
                    Global.Data.FriendDataList = new List<FriendData>();
                }
                /// Cập nhật danh sách bạn bè ở loại tương ứng
                this.currentFriends = Global.Data.FriendDataList.Where(x => x.FriendType == this.currentType).ToList();
                /// Tổng số bạn bè trong nhóm
                int friendsCount = this.currentFriends.Count;
                /// Tổng số trang
                this.maxPage = friendsCount / UIFriendBox.MaxElement + 1;
                if (friendsCount % UIFriendBox.MaxElement == 0)
                {
                    this.maxPage--;
                }
                /// Xây danh sách theo trang
                this.RefreshCurrentPage();
            }
        }

        /// <summary>
        /// Sự kiện khi Toggle danh sách đen được chọn
        /// </summary>
        /// <param name="isSelected"></param>
        private void ToggleBlackList_Clicked(bool isSelected)
        {
            if (isSelected)
            {
                this.currentType = 1;
                /// Số trang hiện tại
                this.currentPage = 1;
                /// Nếu chưa có danh sách bạn bè
                if (Global.Data.FriendDataList == null)
                {
                    Global.Data.FriendDataList = new List<FriendData>();
                }
                /// Cập nhật danh sách bạn bè ở loại tương ứng
                this.currentFriends = Global.Data.FriendDataList.Where(x => x.FriendType == this.currentType).ToList();
                /// Tổng số bạn bè trong nhóm
                int friendsCount = this.currentFriends.Count;
                /// Tổng số trang
                this.maxPage = friendsCount / UIFriendBox.MaxElement + 1;
                if (friendsCount % UIFriendBox.MaxElement == 0)
                {
                    this.maxPage--;
                }
                /// Xây danh sách theo trang
                this.RefreshCurrentPage();
            }
        }

        /// <summary>
        /// Sự kiện khi Toggle danh sách kẻ địch được chọn
        /// </summary>
        /// <param name="isSelected"></param>
        private void ToggleEnemyList_Clicked(bool isSelected)
        {
            if (isSelected)
            {
                this.currentType = 2;
                /// Số trang hiện tại
                this.currentPage = 1;
                /// Nếu chưa có danh sách bạn bè
                if (Global.Data.FriendDataList == null)
                {
                    Global.Data.FriendDataList = new List<FriendData>();
                }
                /// Cập nhật danh sách bạn bè ở loại tương ứng
                this.currentFriends = Global.Data.FriendDataList.Where(x => x.FriendType == this.currentType).ToList();
                /// Tổng số bạn bè trong nhóm
                int friendsCount = this.currentFriends.Count;
                /// Tổng số trang
                this.maxPage = friendsCount / UIFriendBox.MaxElement + 1;
                if (friendsCount % UIFriendBox.MaxElement == 0)
                {
                    this.maxPage--;
                }
                /// Xây danh sách theo trang
                this.RefreshCurrentPage();
            }
        }

        /// <summary>
        /// Sự kiện khi Button bạn được ấn
        /// </summary>
        /// <param name="mousePos"></param>
        /// <param name="friend"></param>
        private void ButtonFriend_Clicked(Vector2 mousePos, FriendData friend)
        {
            /// Nếu không có dữ liệu
            if (friend == null)
            {
                return;
            }

            this.UI_InteractionBox.transform.position = mousePos;
            this.UI_InteractionBox.BrowseInfo = () => {
                this.BrowseFriendInfo?.Invoke(friend);
                this.UI_InteractionBox.Hide();
            };
            this.UI_InteractionBox.Remove = () => {
                this.RemoveFriend?.Invoke(friend);
                this.UI_InteractionBox.Hide();
            };
            this.UI_InteractionBox.Show();
        }

        /// <summary>
        /// Sự kiện khi Button trang trước được ấn
        /// </summary>
        private void ButtonPreviousPage_Clicked()
        {
            this.currentPage--;
            this.RefreshCurrentPage();
        }

        /// <summary>
        /// Sự kiện khi Button trang kế tiếp được ấn
        /// </summary>
        private void ButtonNextPage_Clicked()
        {
            this.currentPage++;
            this.RefreshCurrentPage();
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
        private void RebuildLayout()
        {
            this.StartCoroutine(this.ExecuteSkipFrames(1, () => {
                UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(this.transformFriendList);
            }));
        }

        /// <summary>
        /// Xây giao diện mặc định
        /// </summary>
        private void BuildLayout()
        {
            for (int i = 1; i <= UIFriendBox.MaxElement; i++)
            {
                UIFriendBox_PlayerInfo uiPlayerInfo = GameObject.Instantiate<UIFriendBox_PlayerInfo>(this.UI_PlayerInfoPrefab);
                uiPlayerInfo.transform.SetParent(this.transformFriendList, false);
                uiPlayerInfo.gameObject.SetActive(true);

                uiPlayerInfo.Data = null;
                uiPlayerInfo.Click = null;
            }
            this.RebuildLayout();
        }

        /// <summary>
        /// Làm rỗng giao diện
        /// </summary>
        private void ClearLayout()
        {
            foreach (Transform child in this.transformFriendList.transform)
            {
                if (child.gameObject != this.UI_PlayerInfoPrefab.gameObject)
                {
                    UIFriendBox_PlayerInfo uiPlayerInfo = child.GetComponent<UIFriendBox_PlayerInfo>();
                    uiPlayerInfo.gameObject.SetActive(false);
                    uiPlayerInfo.Data = null;
                    uiPlayerInfo.Click = null;
                }
            }
        }

        /// <summary>
        /// Tìm một vị trí trống trong trang hiện tại
        /// </summary>
        /// <returns></returns>
        private UIFriendBox_PlayerInfo FindEmptySlot()
        {
            foreach (Transform child in this.transformFriendList.transform)
            {
                if (child.gameObject != this.UI_PlayerInfoPrefab.gameObject)
                {
                    UIFriendBox_PlayerInfo uiPlayerInfo = child.GetComponent<UIFriendBox_PlayerInfo>();
                    if (uiPlayerInfo.Data == null)
                    {
                        return uiPlayerInfo;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Thêm bạn vào danh sách hiển thị
        /// </summary>
        /// <param name="friend"></param>
        private void AppendData(FriendData friend)
        {
            UIFriendBox_PlayerInfo emptySlot = this.FindEmptySlot();
            if (emptySlot == null)
            {
                return;
            }

            emptySlot.gameObject.SetActive(true);
            emptySlot.Data = friend;
            emptySlot.Click = (mousePos) => {
                this.ButtonFriend_Clicked(mousePos, friend);
            };
        }

        /// <summary>
        /// Làm mới hiển thị trang hiện tại
        /// </summary>
        private void RefreshCurrentPage()
        {
            /// Nếu danh sách bạn bè rỗng
            if (this.currentFriends == null)
            {
                return;
            }
            /// Nếu trang hiện tại dưới 1 thì set = 1
            if (this.currentPage < 1)
            {
                this.currentPage = 1;
            }
            /// Nếu trang hiện tại lớn hơn tổng số trang thì set = tổng số trang
            if (this.currentPage > this.maxPage)
            {
                this.currentPage = this.maxPage;
            }

            /// Cập nhật thông tin trang
            this.UIText_CurrentPage.text = string.Format("{0}/{1}", this.currentPage, this.maxPage);

            /// Nếu ở trang đầu tiên
            if (this.currentPage <= 1)
            {
                this.UIButton_PreviousPage.Enable = false;
            }
            else
            {
                this.UIButton_PreviousPage.Enable = true;
            }

            /// Nếu ở trang cuối cùng
            if (this.currentPage == this.maxPage)
            {
                this.UIButton_NextPage.Enable = false;
            }
            else
            {
                this.UIButton_NextPage.Enable = true;
            }

            /// Xóa rỗng danh sách bạn trong trang
            this.ClearLayout();

            /// Danh sách bạn được thêm vào
            List<FriendData> currentPageFriends;
            if (this.currentFriends.Count > 0)
            {
                currentPageFriends = this.currentFriends.GetRange((this.currentPage - 1) * UIFriendBox.MaxElement, Math.Min(this.currentFriends.Count, this.currentPage * UIFriendBox.MaxElement - 1));
            }
            else
            {
                currentPageFriends = this.currentFriends;
            }
            /// Duyệt danh sách và thêm vào
            foreach (FriendData friend in currentPageFriends)
            {
                this.AppendData(friend);
            }
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Làm mới danh sách
        /// </summary>
        public void Refresh()
        {
            /// Nếu chưa có danh sách bạn bè
            if (Global.Data.FriendDataList == null)
            {
                Global.Data.FriendDataList = new List<FriendData>();
            }
            /// Cập nhật danh sách bạn bè ở loại tương ứng
            this.currentFriends = Global.Data.FriendDataList.Where(x => x.FriendType == this.currentType).ToList();
            /// Tổng số bạn bè trong nhóm
            int friendsCount = this.currentFriends.Count;
            /// Tổng số trang
            this.maxPage = friendsCount / UIFriendBox.MaxElement + 1;

            this.RefreshCurrentPage();
        }

        /// <summary>
        /// Làm mới danh sách chờ
        /// </summary>
        public void RefreshAskToBeFriendList()
        {
            /// Nếu đang hiển thị khung
            if (this.UI_AskToBeFriendFrame.Visible)
            {
                this.UI_AskToBeFriendFrame.Refresh();
            }
        }
        #endregion
    }
}
