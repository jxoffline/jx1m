using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using System.Collections;
using Server.Data;
using FS.VLTK.UI.Main.BrowsePlayer;
using FS.GameEngine.Logic;

namespace FS.VLTK.UI.Main
{
    /// <summary>
    /// Khung tìm kiếm người chơi
    /// </summary>
    public class UIBrowsePlayer : MonoBehaviour
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
        private UIBrowsePlayer_PlayerInfo UI_PlayerInfoPrefab;

        /// <summary>
        /// Input tên người chơi tìm kiếm
        /// </summary>
        [SerializeField]
        private TMP_InputField UIInput_PlayerName;

        /// <summary>
        /// Button tìm kiếm người chơi
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_BrowsePlayer;

        /// <summary>
        /// POPUP thông tin người chơi
        /// </summary>
        [SerializeField]
        private UIBrowsePlayer_PlayerInfoPopup UI_PlayerInfoPopup;

        /// <summary>
        /// Khung thông báo thông tin vị trí người chơi
        /// </summary>
        [SerializeField]
        private UIBrowsePlayer_PlayerLocationReport UI_PlayerLocationReport;
        #endregion

        #region Private fields
        /// <summary>
        /// RectTransform danh sách người chơi
        /// </summary>
        private RectTransform transformPlayersList = null;
        #endregion

        #region Properties
        /// <summary>
        /// Sự kiện đóng khung
        /// </summary>
        public Action Close { get; set; }

        /// <summary>
        /// Sự kiện tìm người chơi
        /// </summary>
        public Action<string> BrowsePlayer { get; set; }

        /// <summary>
        /// Chat mật
        /// </summary>
        public Action<RoleDataMini> PrivateChat { get; set; }

        /// <summary>
        /// Sự kiện thêm bạn
        /// </summary>
        public Action<RoleDataMini> AddFriend { get; set; }

        /// <summary>
        /// Sự kiện chặn người chơi
        /// </summary>
        public Action<RoleDataMini> AddToBlackList { get; set; }

        /// <summary>
        /// Sự kiện thêm kẻ thù
        /// </summary>
        public Action<RoleDataMini> AddEnemy { get; set; }

        /// <summary>
        /// Sự kiện kiểm tra vị trí
        /// </summary>
        public Action<RoleDataMini> CheckLocation { get; set; }

        /// <summary>
        /// Sự kiện mời vào nhóm
        /// </summary>
        public Action<RoleDataMini> InviteToTeam { get; set; }

        /// <summary>
        /// Sự kiện xin gia nhập nhóm
        /// </summary>
        public Action<RoleDataMini> AskToJoinTeam { get; set; }

        private List<RoleDataMini> _Players = null;
        /// <summary>
        /// Danh sách người chơi
        /// </summary>
        public List<RoleDataMini> Players
        {
            get
            {
                return this._Players;
            }
            set
            {
                this._Players = value;
                this.RefreshData();
            }
        }
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi khi đối tượng được tạo ra
        /// </summary>
        private void Awake()
        {
            this.transformPlayersList = this.UI_PlayerInfoPrefab.transform.parent.GetComponent<RectTransform>();
        }

        /// <summary>
        /// Hàm này gọi ở Frame đầu tiên
        /// </summary>
        private void Start()
        {
            this.InitPrefabs();
            this.RefreshData();
        }
        #endregion

        #region Code UI
        /// <summary>
        /// Khởi tạo ban đầu
        /// </summary>
        private void InitPrefabs()
        {
            this.UIButton_Close.onClick.AddListener(this.ButtonClose_Clicked);
            this.UIButton_BrowsePlayer.onClick.AddListener(this.ButtonBrowsePlayer_Clicked);
        }

        /// <summary>
        /// Sự kiện đóng khung
        /// </summary>
        private void ButtonClose_Clicked()
        {
            this.Close?.Invoke();
        }

        /// <summary>
        /// Sự kiện khi Button tìm kiếm người chơi được ấn
        /// </summary>
        private void ButtonBrowsePlayer_Clicked()
        {
            /// Tên người chơi
            string playerName = Utils.BasicNormalizeString(this.UIInput_PlayerName.text);
            /// Nếu chưa có tên
            if (string.IsNullOrEmpty(playerName))
            {
                KTGlobal.AddNotification("Hãy nhập tên người chơi để tìm kiếm!");
                return;
            }

            /// Thực thi sự kiện tìm kiếm
            this.BrowsePlayer?.Invoke(playerName);
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
            if (!this.gameObject.activeSelf)
            {
                return;
            }

            this.StartCoroutine(this.ExecuteSkipFrames(1, () => {
                UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(this.transformPlayersList);
            }));
        }

        /// <summary>
        /// Làm rỗng danh sách
        /// </summary>
        private void ClearList()
        {
            foreach (Transform child in this.transformPlayersList.transform)
            {
                if (child.gameObject != this.UI_PlayerInfoPrefab.gameObject)
                {
                    GameObject.Destroy(child.gameObject);
                }
            }
            this.RebuildLayout();
        }

        /// <summary>
        /// Thêm người chơi vào danh sách
        /// </summary>
        /// <param name="playerInfo"></param>
        private void AddPlayer(RoleDataMini playerInfo)
        {
            UIBrowsePlayer_PlayerInfo uiPlayerInfo = GameObject.Instantiate<UIBrowsePlayer_PlayerInfo>(this.UI_PlayerInfoPrefab);
            uiPlayerInfo.gameObject.SetActive(true);
            uiPlayerInfo.transform.SetParent(this.transformPlayersList, false);

            uiPlayerInfo.Data = playerInfo;
            /// Nếu không phải bản thân
            if (playerInfo.RoleID != Global.Data.RoleData.RoleID)
            {
                uiPlayerInfo.Selected = () => {
                    this.UI_PlayerInfoPopup.Data = playerInfo;
                    this.UI_PlayerInfoPopup.PrivateChat = () => {
                        this.PrivateChat?.Invoke(playerInfo);
                    };
                    this.UI_PlayerInfoPopup.AddFriend = () => {
                        this.AddFriend?.Invoke(playerInfo);
                    };
                    this.UI_PlayerInfoPopup.AddEnemy = () => {
                        this.AddEnemy?.Invoke(playerInfo);
                    };
                    this.UI_PlayerInfoPopup.AddToBlackList = () => {
                        this.AddToBlackList?.Invoke(playerInfo);
                    };
                    this.UI_PlayerInfoPopup.CheckLocation = () => {
                        this.CheckLocation?.Invoke(playerInfo);
                    };
                    this.UI_PlayerInfoPopup.InviteToTeam = () => {
                        this.InviteToTeam?.Invoke(playerInfo);
                    };
                    this.UI_PlayerInfoPopup.AskToJoinTeam = () => {
                        this.AskToJoinTeam?.Invoke(playerInfo);
                    };
                    this.UI_PlayerInfoPopup.Show();
                };
            }
        }

        /// <summary>
        /// Làm mới dữ liệu
        /// </summary>
        private void RefreshData()
        {
            /// Làm rỗng danh sách
            this.ClearList();

            /// Nếu danh sách rỗng
            if (this._Players == null)
            {
                return;
            }

            /// Xây danh sách
            foreach (RoleDataMini playerInfo in this._Players)
            {
                this.AddPlayer(playerInfo);
            }
            /// Xây lại giao diện
            this.RebuildLayout();
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Hiển thị khung thông tin vị trí của người chơi tương ứng
        /// </summary>
        /// <param name="rd"></param>
        public void ShowPlayerLocationReport(RoleDataMini rd)
        {
            this.UI_PlayerLocationReport.Data = rd;
            this.UI_PlayerLocationReport.Show();
        }
        #endregion
    }
}
