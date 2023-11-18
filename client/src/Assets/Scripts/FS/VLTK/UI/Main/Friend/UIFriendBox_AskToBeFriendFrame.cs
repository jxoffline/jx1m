using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using Server.Data;
using FS.GameEngine.Logic;
using System.Collections;

namespace FS.VLTK.UI.Main.Friend
{
    /// <summary>
    /// Khung danh sách chờ thêm bạn
    /// </summary>
    public class UIFriendBox_AskToBeFriendFrame : MonoBehaviour
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
        private UIFriendBox_AskToBeFriendFrame_PlayerInfo UI_PlayerInfoPrefab;

        /// <summary>
        /// Button thêm bạn
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_AddFriend;

        /// <summary>
        /// Button từ chối
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_Reject;
        #endregion

        #region Private fields
        /// <summary>
        /// RectTransform danh sách người chơi đang chờ
        /// </summary>
        private RectTransform transformPendingList = null;

        /// <summary>
        /// Người chơi được chọn
        /// </summary>
        private RoleDataMini selectedPlayer = null;
        #endregion

        #region Properties
        /// <summary>
        /// Sự kiện thêm bạn
        /// </summary>
        public Action<RoleDataMini> AddFriend { get; set; }

        /// <summary>
        /// Sự kiện từ chối thêm bạn
        /// </summary>
        public Action<RoleDataMini> RejectFriend { get; set; }

        /// <summary>
        /// Có đang hiển thị không
        /// </summary>
        public bool Visible
        {
            get
            {
                return this.gameObject.activeSelf;
            }
        }
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi khi đối tượng được tạo ra
        /// </summary>
        private void Awake()
        {
            this.transformPendingList = this.UI_PlayerInfoPrefab.transform.parent.GetComponent<RectTransform>();
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
            this.UIButton_AddFriend.onClick.AddListener(this.ButtonAddFriend_Clicked);
            this.UIButton_Reject.onClick.AddListener(this.ButtonRejectFriend_Clicked);
        }

        /// <summary>
        /// Sự kiện khi Button đóng khung được ấn
        /// </summary>
        private void ButtonClose_Clicked()
        {
            this.Hide();
        }

        /// <summary>
        /// Sự kiện khi Button thêm bạn được ấn
        /// </summary>
        private void ButtonAddFriend_Clicked()
        {
            /// Nếu không có người chơi nào được chọn
            if (this.selectedPlayer == null)
            {
                KTGlobal.AddNotification("Hãy chọn một người chơi trong danh sách chờ!");
                return;
            }

            this.AddFriend?.Invoke(this.selectedPlayer);
        }

        /// <summary>
        /// Sự kiện khi Button từ chối bạn được ấn
        /// </summary>
        private void ButtonRejectFriend_Clicked()
        {
            /// Nếu không có người chơi nào được chọn
            if (this.selectedPlayer == null)
            {
                KTGlobal.AddNotification("Hãy chọn một người chơi trong danh sách chờ!");
                return;
            }

            this.RejectFriend?.Invoke(this.selectedPlayer);
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
                UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(this.transformPendingList);
            }));
        }

        /// <summary>
        /// Làm rỗng danh sách
        /// </summary>
        private void ClearList()
        {
            foreach (Transform child in this.transformPendingList.transform)
            {
                if (child.gameObject != this.UI_PlayerInfoPrefab.gameObject)
                {
                    GameObject.Destroy(child.gameObject);
                }
            }
        }

        /// <summary>
        /// Tìm ô chứa thông tin người chơi có ID tương ứng
        /// </summary>
        /// <param name="roleID"></param>
        /// <returns></returns>
        private UIFriendBox_AskToBeFriendFrame_PlayerInfo FindPlayer(int roleID)
        {
            foreach (Transform child in this.transformPendingList.transform)
            {
                if (child.gameObject != this.UI_PlayerInfoPrefab.gameObject)
                {
                    UIFriendBox_AskToBeFriendFrame_PlayerInfo uiPlayerInfo = child.GetComponent<UIFriendBox_AskToBeFriendFrame_PlayerInfo>();
                    if (uiPlayerInfo.Data.RoleID == roleID)
                    {
                        return uiPlayerInfo;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Thêm vào danh sách chờ
        /// </summary>
        /// <param name="rd"></param>
        private void AddToList(RoleDataMini rd)
        {
            UIFriendBox_AskToBeFriendFrame_PlayerInfo uiPlayerInfo = GameObject.Instantiate<UIFriendBox_AskToBeFriendFrame_PlayerInfo>(this.UI_PlayerInfoPrefab);
            uiPlayerInfo.transform.SetParent(this.transformPendingList, false);
            uiPlayerInfo.gameObject.SetActive(true);

            uiPlayerInfo.Data = rd;
            uiPlayerInfo.PlayerSelected = () => {
                /// Đánh dấu người chơi được chọn
                this.selectedPlayer = rd;
            };
        }

        /// <summary>
        /// Xóa người chơi có ID tương ứng khỏi danh sách chờ
        /// </summary>
        /// <param name="roleID"></param>
        private void RemoveFromList(int roleID)
        {
            UIFriendBox_AskToBeFriendFrame_PlayerInfo uiPlayerInfo = this.FindPlayer(roleID);
            if (uiPlayerInfo != null)
            {
                GameObject.Destroy(uiPlayerInfo.gameObject);
            }
        }

        /// <summary>
        /// Xây danh sách chờ
        /// </summary>
        private void BuildPendingList()
        {
            /// Duyệt danh sách chờ
            foreach (RoleDataMini rd in Global.Data.AskToBeFriendList)
            {
                this.AddToList(rd);
            }
            /// Nếu có người chơi thì chọn ở vị trí đầu tiên
            if (Global.Data.AskToBeFriendList.Count > 0)
            {
                /// Đánh dấu người chơi được chọn
                this.selectedPlayer = Global.Data.AskToBeFriendList[0];
            }

            /// Xây lại giao diện
            this.RebuildLayout();
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Hiển thị khung
        /// </summary>
        public void Show()
        {
            this.gameObject.SetActive(true);
            this.ClearList();
            this.BuildPendingList();
        }

        /// <summary>
        /// Ẩn khung
        /// </summary>
        public void Hide()
        {
            this.gameObject.SetActive(false);
        }

        /// <summary>
        /// Làm mới danh sách
        /// </summary>
        public void Refresh()
        {
            this.ClearList();
            this.BuildPendingList();
        }
        #endregion
    }
}
