using FS.GameEngine.Logic;
using Server.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static FS.VLTK.Entities.Enum;

namespace FS.VLTK.UI.Main.MainUI.GuildRequestList
{
    /// <summary>
    /// Khung danh sách người chơi mời vào đội
    /// </summary>
    public class UIGuildRequestList_Frame : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Button đóng khung
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_Close;

        /// <summary>
        /// Text Tiêu đề
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_Title;

        /// <summary>
        /// Prefab thông tin người chơi
        /// </summary>
        [SerializeField]
        private UIGuildRequestList_Frame_PlayerInfo UI_PlayerInfoPrefab;
        #endregion

        #region Properties
        /// <summary>
        /// Sự kiện đồng ý
        /// </summary>
        public Action<int, int> Agree { get; set; }

        /// <summary>
        /// Sự kiện từ chói
        /// </summary>
        public Action<int, int> Reject { get; set; }
        #endregion

        #region Private fields
        /// <summary>
        /// RectTransform danh sách người chơi
        /// </summary>
        private RectTransform transformPlayerList;

        /// <summary>
        /// Đã chạy qua hàm Start chưa
        /// </summary>
        private bool isStarted = false;
        #endregion

        #region Constants
        /// <summary>
        /// Số yêu cầu mời tối đa cần hiển thị
        /// </summary>
        private const int MaxRecords = 5;
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi khi đối tượng được tạo ra
        /// </summary>
        private void Awake()
        {
            this.transformPlayerList = this.UI_PlayerInfoPrefab.transform.parent.GetComponent<RectTransform>();
        }

        /// <summary>
        /// Hàm này gọi ở Frame đầu tiên
        /// </summary>
        private void Start()
        {
            this.InitPrefabs();
            /// Đánh dấu chạy qua hàm Start
            this.isStarted = true;
            /// Xây danh sách người chơi
            this.BuildPlayerList();
        }

        /// <summary>
        /// Hàm này gọi khi đối tượng được kích hoạt
        /// </summary>
        private void OnEnable()
        {
            /// Nếu chưa chạy qua hàm Start
            if (!this.isStarted)
            {
                /// Bỏ qua
                return;
            }
            /// Xây danh sách người chơi
            this.BuildPlayerList();
        }
        #endregion

        #region Code UI
        /// <summary>
        /// Khởi tạo ban đầu
        /// </summary>
        private void InitPrefabs()
        {
            this.UIButton_Close.onClick.AddListener(this.ButtonClose_Clicked);
            /// Sinh ra các ô mặc định
            this.AddBlankPlayerInfo();
        }

        /// <summary>
        /// Sự kiện khi Button đóng khung được ấn
        /// </summary>
        private void ButtonClose_Clicked()
        {
            this.Hide();
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
        /// Sinh ra các ô thông tin người chơi mặc định
        /// </summary>
        private void AddBlankPlayerInfo()
        {
            for (int i = 1; i <= UIGuildRequestList_Frame.MaxRecords; i++)
            {
                /// Tạo mới
                UIGuildRequestList_Frame_PlayerInfo uiPlayerInfo = GameObject.Instantiate<UIGuildRequestList_Frame_PlayerInfo>(this.UI_PlayerInfoPrefab);
                uiPlayerInfo.transform.SetParent(this.transformPlayerList, false);
            }
            /// Xây lại giao diện
            this.StartCoroutine(this.ExecuteSkipFrames(1, () =>
            {
                UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(this.transformPlayerList);
            }));
        }

        /// <summary>
        /// Trả về vị trí thông tin người chơi còn trống
        /// </summary>
        /// <returns></returns>
        private UIGuildRequestList_Frame_PlayerInfo GetEmptySlot()
        {
            /// Duyệt toàn bộ các con
            foreach (Transform child in this.transformPlayerList.transform)
            {
                if (child.gameObject != this.UI_PlayerInfoPrefab.gameObject && child.GetComponent<UIGuildRequestList_Frame_PlayerInfo>().Data == null)
                {
                    /// Trả về kết quả
                    return child.GetComponent<UIGuildRequestList_Frame_PlayerInfo>();
                }
            }
            /// Không tìm thấy
            return null;
        }

        /// <summary>
        /// Xây danh sách người chơi
        /// </summary>
        private void BuildPlayerList()
        {
            /// Làm rỗng danh sách người chơi
            foreach (Transform child in this.transformPlayerList.transform)
            {
                if (child.gameObject != this.UI_PlayerInfoPrefab.gameObject)
                {
                    UIGuildRequestList_Frame_PlayerInfo uiPlayerInfo = child.GetComponent<UIGuildRequestList_Frame_PlayerInfo>();
                    uiPlayerInfo.gameObject.SetActive(false);
                    uiPlayerInfo.Data = null;
                    uiPlayerInfo.Agree = null;
                    uiPlayerInfo.Reject = null;
                }
            }

            /// Danh sách người chơi
            List<RoleDataMini> requestList = null;

            /// Nếu bản thân chưa có bang
            if (Global.Data.RoleData.GuildID <= 0)
            {
                requestList = Global.Data.GuildInvitationPlayers;
            }
            /// Nếu bản thân đã có bang và là bang chủ hoặc phó bang chủ
            else if (Global.Data.RoleData.GuildID > 0 && (Global.Data.RoleData.GuildRank == (int) GuildRank.Master || Global.Data.RoleData.GuildRank == (int) GuildRank.ViceMaster))
            {
                requestList = Global.Data.GuildRequestJoinPlayers;
            }

            /// Toác
            if (requestList == null)
            {
                return;
            }

            /// Duyệt danh sách người chơi
            for (int i = requestList.Count - 1; i >= Math.Max(0, requestList.Count - UIGuildRequestList_Frame.MaxRecords); i--)
            {
                /// Thông tin
                RoleDataMini rd = requestList[i];
                /// Vị trí còn trống
                UIGuildRequestList_Frame_PlayerInfo uiPlayerInfo = this.GetEmptySlot();
                /// Không tìm thấy
                if (uiPlayerInfo == null)
                {
                    /// Bỏ qua
                    break;
                }
                /// Gắn dữ liệu
                uiPlayerInfo.transform.SetParent(this.transformPlayerList, false);
                uiPlayerInfo.gameObject.SetActive(true);
                uiPlayerInfo.Data = rd;
                uiPlayerInfo.Agree = () =>
                {
                    /// Xóa khỏi dữ liệu
                    requestList.Remove(uiPlayerInfo.Data);
                    /// Xây lại giao diện
                    this.BuildPlayerList();
                    /// Thực thi sự kiện
                    this.Agree?.Invoke(rd.RoleID, rd.GuildID);
                };
                uiPlayerInfo.Reject = () =>
                {
                    /// Xóa khỏi dữ liệu
                    requestList.Remove(uiPlayerInfo.Data);
                    /// Xây lại giao diện
                    this.BuildPlayerList();
                    /// Thực thi sự kiện
                    this.Reject?.Invoke(rd.RoleID, rd.GuildID);
                };
            }
            /// Xây lại giao diện
            this.StartCoroutine(this.ExecuteSkipFrames(1, () =>
            {
                UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(this.transformPlayerList);
            }));
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Hiện khung
        /// </summary>
        public void Show()
        {
            /// Nếu chưa có bang
            if (Global.Data.RoleData.GuildID <= 0)
            {
                this.UIText_Title.text = "Mời vào bang";
            }
            /// Nếu đã có bang
            else
            {
                this.UIText_Title.text = "Xin vào bang";
            }

            this.gameObject.SetActive(true);
        }
        
        /// <summary>
        /// Ẩn khung
        /// </summary>
        public void Hide()
        {
            this.gameObject.SetActive(false);
        }

        /// <summary>
        /// Làm mới lại khung
        /// </summary>
        public void Refresh()
        {
            /// Nếu không hiện khung
            if (!this.gameObject.activeSelf)
            {
                /// Bỏ qua
                return;
            }
            this.BuildPlayerList();
        }
        #endregion
    }
}
