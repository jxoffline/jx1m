using FS.GameEngine.Logic;
using FS.VLTK.UI.Main.Challenge;
using Server.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace FS.VLTK.UI.Main
{
    /// <summary>
    /// Khung thách đấu
    /// </summary>
    public class UIChallenge : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Button đóng khung
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_Close;

        /// <summary>
        /// Prefab người chơi đối thủ
        /// </summary>
        [SerializeField]
        private UIChallenge_PlayerInfo UI_OpponentInfoPrefab;

        /// <summary>
        /// Prefab người chơi phe mình
        /// </summary>
        [SerializeField]
        private UIChallenge_PlayerInfo UI_TeammateInfoPrefab;

        /// <summary>
        /// Text số tiền cược bản thân
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_SelfMoney;

        /// <summary>
        /// Button thiết lập số tiền cược
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_InputSelfMoney;

        /// <summary>
        /// Text số tiền cược đối thủ
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_OpponentMoney;

        /// <summary>
        /// Button xác nhận
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_Confirm;

        /// <summary>
        /// Mark đối phương đã xác nhận
        /// </summary>
        [SerializeField]
        private RectTransform UIMark_OpponentConfirmed;

        /// <summary>
        /// Mark đối phương chưa xác nhận
        /// </summary>
        [SerializeField]
        private RectTransform UIMark_OpponentNotConfirmed;

        /// <summary>
        /// Button bắt đầu trận chiến
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_Fight;
        #endregion

        #region Properties
        private RoleChallengeData _Data;
        /// <summary>
        /// Dữ liệu thách đấu
        /// </summary>
        public RoleChallengeData Data
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
        /// Sự kiện thiết lập tiền cược
        /// </summary>
        public Action<int> SetMoney { get; set; }

        /// <summary>
        /// Sự kiện xác nhận
        /// </summary>
        public Action Confirm { get; set; }

        /// <summary>
        /// Sự kiện chiến đấu
        /// </summary>
        public Action Fight { get; set; }
        #endregion

        #region Private fields
        /// <summary>
        /// RectTransform danh sách thành viên đội bản thân
        /// </summary>
        private RectTransform transformTeammateList = null;

        /// <summary>
        /// RectTransform danh sách thành viên đội đối thủ
        /// </summary>
        private RectTransform transformOpponentTeammateList = null;
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi khi đối tượng được tạo ra
        /// </summary>
        private void Awake()
        {
            this.transformTeammateList = this.UI_TeammateInfoPrefab.transform.parent.GetComponent<RectTransform>();
            this.transformOpponentTeammateList = this.UI_OpponentInfoPrefab.transform.parent.GetComponent<RectTransform>();
        }

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
            this.UIButton_InputSelfMoney.onClick.AddListener(this.ButtonInputSelfMoney_Clicked);
            this.UIButton_Confirm.onClick.AddListener(this.ButtonConfirm_Clicked);
            this.UIButton_Fight.onClick.AddListener(this.ButtonFight_Clicked);
        }

        /// <summary>
        /// Sự kiện khi Button đóng khung được ấn
        /// </summary>
        private void ButtonClose_Clicked()
        {
            this.Close?.Invoke();
        }

        /// <summary>
        /// Sự kiện khi Button nhập số tiền cược bản thân được ấn
        /// </summary>
        private void ButtonInputSelfMoney_Clicked()
        {
            /// Thứ tự nhóm bản thân
            int teamID = this.GetSelfTeamIndex();
            /// Nếu bản thân đã khóa
            if (this._Data.TeamReadyStates[teamID])
            {
                /// Thông báo
                KTGlobal.AddNotification("Đã xác nhận, không thể thay đổi tiền cược.");
                /// Bỏ qua
                return;
            }

            /// Mở khung nhập số tiền cược
            KTGlobal.ShowInputNumber("Nhập số bạc sẽ cược <color=yellow>(tối đa 1000 vạn)</color>.", 0, 10000000, 0, (moneyAmount) =>
            {
                /// Thực thi sự kiện thiết lập số tiền cược
                this.SetMoney?.Invoke(moneyAmount);
            });
        }

        /// <summary>
        /// Sự kiện khi Button xác nhận được ấn
        /// </summary>
        private void ButtonConfirm_Clicked()
        {
            /// Thứ tự nhóm bản thân
            int teamID = this.GetSelfTeamIndex();
            /// Nếu bản thân đã khóa
            if (this._Data.TeamReadyStates[teamID])
            {
                /// Thông báo
                KTGlobal.AddNotification("Đã xác nhận, không thể xác nhận thêm nữa.");
                /// Bỏ qua
                return;
            }

            /// Thực thi sự kiện xác nhận
            this.Confirm?.Invoke();
        }

        /// <summary>
        /// Sự kiện khi Button chiến đấu được ấn
        /// </summary>
        private void ButtonFight_Clicked()
        {
            /// Thứ tự nhóm bản thân
            int teamID = this.GetSelfTeamIndex();
            /// Nếu bản thân chưa khóa
            if (!this._Data.TeamReadyStates[teamID])
            {
                /// Thông báo
                KTGlobal.AddNotification("Bản thân chưa xác nhận, không thể bắt đầu chiến đấu.");
                /// Bỏ qua
                return;
            }
            /// Nếu một trong 2 bên chưa khóa
            else if (!this._Data.TeamReadyStates[1] || !this._Data.TeamReadyStates[2])
            {
                /// Thông báo
                KTGlobal.AddNotification("Đối phương chưa xác nhận, không thể bắt đầu chiến đấu.");
                /// Bỏ qua
                return;
            }

            /// Thực thi sự kiện
            this.Fight?.Invoke();
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
            if (this._Data == null)
            {
                /// Đóng khung
                this.Close?.Invoke();
                /// Bỏ qua
                return;
            }

            /// Làm rỗng danh sách người chơi
            foreach (Transform child in this.transformTeammateList.transform)
            {
                if (child.gameObject != this.UI_TeammateInfoPrefab.gameObject)
                {
                    GameObject.Destroy(child.gameObject);
                }
            }
            foreach (Transform child in this.transformOpponentTeammateList.transform)
            {
                if (child.gameObject != this.UI_OpponentInfoPrefab.gameObject)
                {
                    GameObject.Destroy(child.gameObject);
                }
            }

            /// ID nhóm bản thân
            int teamID = this.GetSelfTeamIndex();
            /// Duyệt danh sách thành viên nhóm bản thân
            foreach (RoleChallenge_PlayerData playerData in this._Data.TeamPlayers[teamID])
            {
                /// Tạo mới
                UIChallenge_PlayerInfo uiPlayerInfo = GameObject.Instantiate<UIChallenge_PlayerInfo>(this.UI_TeammateInfoPrefab);
                uiPlayerInfo.transform.SetParent(this.transformTeammateList, false);
                uiPlayerInfo.gameObject.SetActive(true);
                uiPlayerInfo.Data = playerData;
            }

            /// ID nhóm đối phương
            int opponentTeamID = teamID == 1 ? 2 : 1;
            /// Duyệt danh sách thành viên nhóm đối phương
            foreach (RoleChallenge_PlayerData playerData in this._Data.TeamPlayers[opponentTeamID])
            {
                /// Tạo mới
                UIChallenge_PlayerInfo uiPlayerInfo = GameObject.Instantiate<UIChallenge_PlayerInfo>(this.UI_OpponentInfoPrefab);
                uiPlayerInfo.transform.SetParent(this.transformOpponentTeammateList, false);
                uiPlayerInfo.gameObject.SetActive(true);
                uiPlayerInfo.Data = playerData;
            }

            /// Xây lại giao diện
            this.ExecuteSkipFrames(1, () =>
            {
                UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(this.transformTeammateList);
                UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(this.transformOpponentTeammateList);
            });

            /// Thiết lập Text
            this.UIText_SelfMoney.text = KTGlobal.GetDisplayMoney(this._Data.TeamMoneys[teamID]);
            this.UIText_OpponentMoney.text = KTGlobal.GetDisplayMoney(this._Data.TeamMoneys[opponentTeamID]);
            this.UIMark_OpponentNotConfirmed.gameObject.SetActive(!this._Data.TeamReadyStates[opponentTeamID]);
            this.UIMark_OpponentConfirmed.gameObject.SetActive(this._Data.TeamReadyStates[opponentTeamID]);

            /// Cập nhật trạng thái của Button
            this.UpdateButtonStates();
        }

        /// <summary>
        /// Cập nhật trạng thái của Button chức năng
        /// </summary>
        private void UpdateButtonStates()
        {
            /// ID nhóm bản thân
            int teamID = this.GetSelfTeamIndex();

            /// Cập nhật trạng thái
            this.UIButton_InputSelfMoney.interactable = !this._Data.TeamReadyStates[teamID];
            this.UIButton_Confirm.interactable = !this._Data.TeamReadyStates[teamID];
            this.UIButton_Fight.interactable = this._Data.TeamReadyStates[teamID];
        }

        /// <summary>
        /// Trả về thứ tự nhóm bản thân
        /// </summary>
        /// <returns></returns>
        private int GetSelfTeamIndex()
        {
            /// Nếu nằm trong team 1
            if (this._Data.TeamPlayers[1].Any(x => x.RoleID == Global.Data.RoleData.RoleID))
            {
                return 1;
            }
            /// Nếu không nằm trong team 1 thì ở team 2
            else
            {
                return 2;
            }
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Cập nhật tiền cược của 2 nhóm
        /// </summary>
        /// <param name="teamMoneys"></param>
        public void UpdateMoney(Dictionary<int, int> teamMoneys)
        {
            /// Đồng bộ vào dữ liệu
            this._Data.TeamMoneys = teamMoneys;

            /// ID nhóm bản thân
            int teamID = this.GetSelfTeamIndex();
            /// ID nhóm đối phương
            int opponentTeamID = teamID == 1 ? 2 : 1;

            /// Thiết lập Text
            this.UIText_SelfMoney.text = KTGlobal.GetDisplayMoney(teamMoneys[teamID]);
            this.UIText_OpponentMoney.text = KTGlobal.GetDisplayMoney(teamMoneys[opponentTeamID]);

            /// Cập nhật trạng thái Button
            this.UpdateButtonStates();
        }

        /// <summary>
        /// Cập nhật trạng thái sẵn sàng của 2 nhóm
        /// </summary>
        /// <param name="teamReadyStates"></param>
        public void UpdateReadyState(Dictionary<int, bool> teamReadyStates)
        {
            /// Đồng bộ vào dữ liệu
            this._Data.TeamReadyStates = teamReadyStates;

            /// ID nhóm bản thân
            int teamID = this.GetSelfTeamIndex();
            /// ID nhóm đối phương
            int opponentTeamID = teamID == 1 ? 2 : 1;

            /// Thiết lập Text
            this.UIMark_OpponentNotConfirmed.gameObject.SetActive(!this._Data.TeamReadyStates[opponentTeamID]);
            this.UIMark_OpponentConfirmed.gameObject.SetActive(this._Data.TeamReadyStates[opponentTeamID]);

            /// Cập nhật trạng thái Button
            this.UpdateButtonStates();
        }
        #endregion
    }
}
