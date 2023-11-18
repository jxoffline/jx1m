using FS.GameEngine.Logic;
using FS.VLTK.UI.Main.TeamFrame;
using Server.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FS.VLTK.UI.Main
{
    /// <summary>
    /// Khung thông tin đội
    /// </summary>
    public class UITeamFrame : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Button đóng khung
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_Close;

        /// <summary>
        /// Button tạo nhóm
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_CreateTeam;

        /// <summary>
        /// Button rời nhóm
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_LeaveTeam;

        /// <summary>
        /// Button nhường chức đội trưởng
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_ApproveTeamLeader;

        /// <summary>
        /// Button trục xuất đội viên
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_KickOut;

        /// <summary>
        /// Prefab thông tin đội viên
        /// </summary>
        [SerializeField]
        private UITeamFrame_TeamMemberInfo UI_TeamMemberInfoPrefab;

        /// <summary>
        /// Prefab thông tin người chơi xin nhập đội
        /// </summary>
        [SerializeField]
        private UITeamFrame_AskToJoinPlayerInfo UI_AskToJoinPlayerInfoPrefab;
        #endregion

        #region Constants
        /// <summary>
        /// Số thành viên nhóm tối đa
        /// </summary>
        private const int MaxTeamSize = 6;
        #endregion

        #region Properties
        /// <summary>
        /// Danh sách người chơi đang yêu cầu thêm vào nhóm
        /// </summary>
        public static Dictionary<int, RoleDataMini> ListAskingToJoinTeam { get; private set; } = new Dictionary<int, RoleDataMini>();

        /// <summary>
        /// Sự kiện đóng khung
        /// </summary>
        public Action Close { get; set; }

        /// <summary>
        /// Sự kiện khi đối tượng được tải xuống hoàn tất (đã thực thi qua hàm Start)
        /// </summary>
        public Action Ready { get; set; }

        #region Thông tin nhóm
        /// <summary>
        /// Sự kiện tạo nhóm
        /// </summary>
        public Action CreateTeam { get; set; }

        /// <summary>
        /// Sự kiện rời nhóm
        /// </summary>
        public Action LeaveTeam { get; set; }

        /// <summary>
        /// Sự kiện bổ nhiệm đội trưởng
        /// </summary>
        public Action<RoleDataMini> ApproveTeamLeader { get; set; }

        /// <summary>
        /// Sự kiện trục xuất người chơi
        /// </summary>
        public Action<RoleDataMini> KickOut { get; set; }
        #endregion

        #region Xin vào nhóm
        /// <summary>
        /// Sự kiện đồng ý cho người chơi trong danh sách chờ tương ứng vào nhóm
        /// </summary>
        public Action<RoleDataMini> AgreeJoinTeam { get; set; }

        /// <summary>
        /// Sự kiện từ chối người chơi trong danh sách chờ tương ứng vào nhóm
        /// </summary>
        public Action<RoleDataMini> RefuseJoinTeam { get; set; }
        #endregion
        #endregion

        #region Private fields
        /// <summary>
        /// Đội viên đang được chọn
        /// </summary>
        private RoleDataMini selectedTeammate = null;

        /// <summary>
        /// Transform danh sách chứa thông tin đội viên
        /// </summary>
        private RectTransform teamMemberListTransform = null;

        /// <summary>
        /// Transform danh sách chứa thông tin người chơi xin gia nhập nhóm
        /// </summary>
        private RectTransform waitingPlayerListTransform = null;
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi khi đối tượng được tạo ra
        /// </summary>
        private void Awake()
        {
            this.teamMemberListTransform = this.UI_TeamMemberInfoPrefab.transform.parent.gameObject.GetComponent<RectTransform>();
            this.waitingPlayerListTransform = this.UI_AskToJoinPlayerInfoPrefab.transform.parent.gameObject.GetComponent<RectTransform>();
        }

        /// <summary>
        /// Hàm này gọi ở Frame đầu tiên
        /// </summary>
        private void Start()
        {
            this.InitPrefabs();

            this.InitDefaultTeammateList();
            this.RefreshButtonsState();
            this.ClearAskToJoinList();
            this.BuildWaitingToJoinList();

            /// Thực thi sự kiện khi đã qua hàm Start
            this.Ready?.Invoke();
        }
        #endregion

        #region Code UI
        /// <summary>
        /// Khởi tạo ban đầu
        /// </summary>
        private void InitPrefabs()
        {
            this.UIButton_Close.onClick.AddListener(this.ButtonClose_Clicked);
            this.UIButton_KickOut.onClick.AddListener(this.ButtonKickOut_Clicked);
            this.UIButton_LeaveTeam.onClick.AddListener(this.ButtonLeaveTeam_Clicked);
            this.UIButton_CreateTeam.onClick.AddListener(this.ButtonCreateTeam_Clicked);
            this.UIButton_ApproveTeamLeader.onClick.AddListener(this.ButtonApproveTeamLeader_Clicked);

        }

        /// <summary>
        /// Sự kiện khi Button đóng khung được ấn
        /// </summary>
        private void ButtonClose_Clicked()
        {
            this.Close?.Invoke();
        }

        /// <summary>
        /// Sự kiện khi Button tạo nhóm được ấn
        /// </summary>
        private void ButtonCreateTeam_Clicked()
        {
            this.CreateTeam?.Invoke();
        }

        /// <summary>
        /// Sự kiện khi Button rời nhóm được ấn
        /// </summary>
        private void ButtonLeaveTeam_Clicked()
        {
            this.LeaveTeam?.Invoke();
        }

        /// <summary>
        /// Sự kiện khi Button bổ nhiệm đội trưởng được ấn
        /// </summary>
        private void ButtonApproveTeamLeader_Clicked()
        {
            this.ApproveTeamLeader?.Invoke(this.selectedTeammate);
        }

        /// <summary>
        /// Sự kiện khi Button trục xuất đội viên được ấn
        /// </summary>
        private void ButtonKickOut_Clicked()
        {
            this.KickOut?.Invoke(this.selectedTeammate);
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Bỏ qua một số lượng nhất định Frame và sau đó thực thi công việc
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
        /// Thực hiện Rebuild lại layout tương ứng ở Frame tiếp theo
        /// </summary>
        /// <param name="layout"></param>
        private void RebuildLayout(RectTransform layout)
        {
            this.StartCoroutine(this.ExecuteSkipFrames(1, () => {
                UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(layout);
            }));
        }

        #region Thành viên nhóm
        /// <summary>
        /// Xóa danh sách đội viên đang hiển thị
        /// </summary>
        private void ClearTeammateList()
        {
            foreach (Transform child in this.teamMemberListTransform.transform)
            {
                if (child.gameObject != this.UI_TeamMemberInfoPrefab.gameObject)
                {
                    GameObject.Destroy(child.gameObject);
                }
            }
        }

        /// <summary>
        /// Làm rỗng danh sách đội viên đang hiển thị
        /// </summary>
        private void EmptyTeammateList()
        {
            foreach (Transform child in this.teamMemberListTransform.transform)
            {
                if (child.gameObject != this.UI_TeamMemberInfoPrefab.gameObject)
                {
                    UITeamFrame_TeamMemberInfo memberInfo = child.gameObject.GetComponent<UITeamFrame_TeamMemberInfo>();
                    memberInfo.Active = false;
                    memberInfo.RoleData = null;
                    memberInfo.Selected = null;
                    memberInfo.Enable = false;
                }
            }
            this.RebuildLayout(this.teamMemberListTransform);
        }

        /// <summary>
        /// Khởi tạo danh sách đội viên trên UI
        /// </summary>
        /// <param name="count"></param>
        private void InitDefaultTeammateList()
        {
            this.ClearTeammateList();

            for (int i = 1; i <= UITeamFrame.MaxTeamSize; i++)
            {
                UITeamFrame_TeamMemberInfo memberInfo = GameObject.Instantiate<UITeamFrame_TeamMemberInfo>(this.UI_TeamMemberInfoPrefab);
                memberInfo.gameObject.SetActive(true);
                memberInfo.transform.SetParent(this.teamMemberListTransform, false);
                memberInfo.RoleData = null;
                memberInfo.Selected = null;
                memberInfo.Enable = false;
                memberInfo.Active = false;
            }

            this.RebuildLayout(this.teamMemberListTransform);
        }

        /// <summary>
        /// Trả ra vị trí trống hiện chưa có thành viên
        /// </summary>
        /// <returns></returns>
        private UITeamFrame_TeamMemberInfo GetEmptyTeammateSlot()
        {
            foreach (Transform child in this.teamMemberListTransform.transform)
            {
                if (child.gameObject != this.UI_TeamMemberInfoPrefab.gameObject)
                {
                    UITeamFrame_TeamMemberInfo memberInfo = child.gameObject.GetComponent<UITeamFrame_TeamMemberInfo>();
                    if (memberInfo.RoleData == null)
                    {
                        return memberInfo;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Trả về vị trí chứa thông tin đội viên tương ứng
        /// </summary>
        /// <param name="roleData"></param>
        /// <returns></returns>
        private UITeamFrame_TeamMemberInfo GetTeamMemberSlot(int roleID)
        {
            foreach (Transform child in this.teamMemberListTransform.transform)
            {
                if (child.gameObject != this.UI_TeamMemberInfoPrefab.gameObject)
                {
                    UITeamFrame_TeamMemberInfo memberInfo = child.gameObject.GetComponent<UITeamFrame_TeamMemberInfo>();
                    if (memberInfo.RoleData != null && memberInfo.RoleData.RoleID == roleID)
                    {
                        return memberInfo;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Sắp xếp lại danh sách nhóm
        /// </summary>
        private void SortTeamMemberList()
        {
            /// Danh sách thành viên
            List<UITeamFrame_TeamMemberInfo> members = new List<UITeamFrame_TeamMemberInfo>();
            UITeamFrame_TeamMemberInfo teamLeader = null;
            /// Tìm đội trưởng, và các thành viên, lưu vào mảng tương ứng
            foreach (Transform child in this.teamMemberListTransform.transform)
            {
                if (child.gameObject != this.UI_TeamMemberInfoPrefab.gameObject)
                {
                    UITeamFrame_TeamMemberInfo memberInfo = child.gameObject.GetComponent<UITeamFrame_TeamMemberInfo>();
                    /// Nếu đây là vị trí trống
                    if (memberInfo.RoleData == null)
                    {
                        continue;
                    }

                    /// Nếu là trưởng nhóm
                    if (memberInfo.RoleData.TeamLeaderID == memberInfo.RoleData.RoleID)
                    {
                        teamLeader = memberInfo;
                    }
                    /// Nếu là đội viên bình thường
                    else
                    {
                        members.Add(memberInfo);
                    }
                }
            }

            /// Nếu đội trưởng NULL
            if (teamLeader == null)
            {
                return;
            }

            /// Các thành viên sẽ lần lượt được thêm vào đầu danh sách
            for (int i = members.Count - 1; i >= 0; i--)
            {
                members[i].gameObject.transform.SetAsFirstSibling();
                //members[i].gameObject.transform.SetSiblingIndex(2);
            }

            /// Đội trưởng sẽ ở vị trí đầu tiên trong nhóm
            teamLeader.transform.SetAsFirstSibling();
            
        }

        /// <summary>
        /// Cập nhật lại trạng thái của các Button chức năng bên dưới
        /// </summary>
        private void RefreshButtonsState()
        {
            this.UIButton_CreateTeam.gameObject.SetActive(false);
            this.UIButton_LeaveTeam.gameObject.SetActive(false);
            this.UIButton_KickOut.gameObject.SetActive(false);
            this.UIButton_ApproveTeamLeader.gameObject.SetActive(false);

            /// Nếu bản thân không có nhóm
            if (Global.Data.RoleData.TeamID == -1)
            {
                this.UIButton_CreateTeam.gameObject.SetActive(true);
            }
            /// Nếu bản thân có nhóm
            else
            {
                this.UIButton_LeaveTeam.gameObject.SetActive(true);

                /// Nếu bản thân là đội trưởng
                if (Global.Data.RoleData.TeamLeaderRoleID == Global.Data.RoleData.RoleID)
                {
                    /// Nếu đội viên đang được chọn là thằng khác
                    if (this.selectedTeammate != null && Global.Data.RoleData.RoleID != this.selectedTeammate.RoleID)
                    {
                        this.UIButton_KickOut.gameObject.SetActive(true);
                        this.UIButton_ApproveTeamLeader.gameObject.SetActive(true);
                    }
                }
            }
        }

        /// <summary>
        /// Chọn đội viên đầu tiên trong danh sách
        /// </summary>
        private void SelectFirstTeamMember()
        {
            this.selectedTeammate = null;

            /// Bỏ chọn tất cả
            foreach (Transform child in this.teamMemberListTransform.transform)
            {
                if (child.gameObject != this.UI_TeamMemberInfoPrefab.gameObject)
                {
                    UITeamFrame_TeamMemberInfo memberInfo = child.gameObject.GetComponent<UITeamFrame_TeamMemberInfo>();
                    /// Nếu đây là vị trí trống
                    if (memberInfo.RoleData == null)
                    {
                        continue;
                    }

                    memberInfo.Active = false;
                }
            }

            /// Chọn thằng đầu tiên
            foreach (Transform child in this.teamMemberListTransform.transform)
            {
                if (child.gameObject != this.UI_TeamMemberInfoPrefab.gameObject)
                {
                    UITeamFrame_TeamMemberInfo memberInfo = child.gameObject.GetComponent<UITeamFrame_TeamMemberInfo>();
                    /// Nếu đây là vị trí trống
                    if (memberInfo.RoleData == null)
                    {
                        continue;
                    }

                    memberInfo.Active = true;
                    break;
                }
            }
        }
        #endregion

        #region Xin vào nhóm
        /// <summary>
        /// Xóa danh sách người chơi đang yêu cầu vào nhóm
        /// </summary>
        private void ClearAskToJoinList()
        {
            foreach (Transform child in this.waitingPlayerListTransform.transform)
            {
                if (child.gameObject != this.UI_AskToJoinPlayerInfoPrefab.gameObject)
                {
                    GameObject.Destroy(child.gameObject);
                }
            }
        }

        /// <summary>
        /// Tìm vị trí người chơi đang đợi vào nhóm tương ứng
        /// </summary>
        /// <param name="roleID"></param>
        /// <returns></returns>
        private UITeamFrame_AskToJoinPlayerInfo GetAskToJoinSlot(int roleID)
        {
            foreach (Transform child in this.waitingPlayerListTransform.transform)
            {
                if (child.gameObject != this.UI_AskToJoinPlayerInfoPrefab.gameObject)
                {
                    UITeamFrame_AskToJoinPlayerInfo playerInfo = child.gameObject.GetComponent<UITeamFrame_AskToJoinPlayerInfo>();
                    if (playerInfo.RoleData != null && playerInfo.RoleData.RoleID == roleID)
                    {
                        return playerInfo;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Xây danh sách chờ thêm vào nhóm
        /// </summary>
        private void BuildWaitingToJoinList()
        {
            foreach (RoleDataMini roleData in UITeamFrame.ListAskingToJoinTeam.Values)
            {
                this.AddWaitingPlayer(roleData);
            }
        }
        #endregion
        #endregion

        #region Public methods
        #region Thành viên nhóm
        /// <summary>
        /// Tạo nhóm mới từ Server gửi về
        /// </summary>
        public void ServerCreateTeam()
        {
            /// Tạo mới đối tượng RoleDataMini
            RoleDataMini roleData = new RoleDataMini()
            {
                RoleID = Global.Data.RoleData.RoleID,
                TeamLeaderID = Global.Data.RoleData.TeamLeaderRoleID,
                MapCode = Global.Data.RoleData.MapCode,
                PosX = Global.Data.RoleData.PosX,
                PosY = Global.Data.RoleData.PosY,
                HP = Global.Data.RoleData.CurrentHP,
                MaxHP = Global.Data.RoleData.MaxHP,
                FactionID = Global.Data.RoleData.FactionID,
                Level = Global.Data.RoleData.Level,
                RoleName = Global.Data.RoleData.RoleName,
                RoleSex = Global.Data.RoleData.RoleSex,
                AvartaID = Global.Data.RoleData.RolePic,
            };

            /// Duyệt danh sách trang bị trên người
            if (Global.Data.RoleData.GoodsDataList != null)
            {
                foreach (GoodsData equip in Global.Data.RoleData.GoodsDataList)
                {
                    /// Nếu đang hiện Set dự phòng
                    if (Global.Data.ShowReserveEquip)
                    {
                        int usingIndex = equip.Using - 100;

                        switch (usingIndex)
                        {
                            /// Vũ khí
                            case (int) Entities.Enum.KE_EQUIP_POSITION.emEQUIPPOS_WEAPON:
                            {
                                roleData.WeaponID = equip.GoodsID;
                                roleData.WeaponEnhanceLevel = equip.Forge_level;
                                roleData.WeaponSeries = equip.Series;
                                break;
                            }
                            /// Mũ
                            case (int) Entities.Enum.KE_EQUIP_POSITION.emEQUIPPOS_HEAD:
                            {
                                roleData.HelmID = equip.GoodsID;
                                break;
                            }
                            /// Áo
                            case (int) Entities.Enum.KE_EQUIP_POSITION.emEQUIPPOS_BODY:
                            {
                                roleData.ArmorID = equip.GoodsID;
                                break;
                            }
                            /// Phi phong
                            case (int) Entities.Enum.KE_EQUIP_POSITION.emEQUIPPOS_MANTLE:
                            {
                                roleData.MantleID = equip.GoodsID;
                                break;
                            }
                        }
                    }
                    else
                    {
                        switch (equip.Using)
                        {
                            /// Vũ khí
                            case (int) Entities.Enum.KE_EQUIP_POSITION.emEQUIPPOS_WEAPON:
                            {
                                roleData.WeaponID = equip.GoodsID;
                                roleData.WeaponEnhanceLevel = equip.Forge_level;
                                roleData.WeaponSeries = equip.Series;
                                break;
                            }
                            /// Mũ
                            case (int) Entities.Enum.KE_EQUIP_POSITION.emEQUIPPOS_HEAD:
                            {
                                roleData.HelmID = equip.GoodsID;
                                break;
                            }
                            /// Áo
                            case (int) Entities.Enum.KE_EQUIP_POSITION.emEQUIPPOS_BODY:
                            {
                                roleData.ArmorID = equip.GoodsID;
                                break;
                            }
                            /// Phi phong
                            case (int) Entities.Enum.KE_EQUIP_POSITION.emEQUIPPOS_MANTLE:
                            {
                                roleData.MantleID = equip.GoodsID;
                                break;
                            }
                        }
                    }
                }
            }

            /// Thêm bản thân vào danh sách
            this.AddTeammate(roleData);

            this.SelectFirstTeamMember();
            this.RefreshButtonsState();
        }

        /// <summary>
        /// Thay đổi trưởng nhóm từ Server gửi về
        /// </summary>
        /// <param name="roleID"></param>
        public void ServerChangeTeamLeader(int roleID)
        {
            /// Duyệt danh sách thành viên
            foreach (Transform child in this.teamMemberListTransform)
            {
                if (child.gameObject != this.UI_TeamMemberInfoPrefab.gameObject)
                {
                    UITeamFrame_TeamMemberInfo memberInfo = child.gameObject.GetComponent<UITeamFrame_TeamMemberInfo>();
                    /// Nếu đây là vị trí trống
                    if (memberInfo.RoleData == null)
                    {
                        continue;
                    }

                    /// Thiết lập trưởng nhóm mới
                    memberInfo.RoleData.TeamLeaderID = roleID;
                    memberInfo.RefreshTeamLeader();
                }
            }

            /// Sắp xếp lại danh sách
            this.SortTeamMemberList();
            this.SelectFirstTeamMember();
            this.RefreshButtonsState();
        }

        /// <summary>
        /// Thêm đội viên
        /// </summary>
        /// <param name="roleData"></param>
        public void AddTeammate(RoleDataMini roleData)
        {
            if (roleData == null)
            {
                return;
            }

            /// Tìm 1 vị trí trống
            UITeamFrame_TeamMemberInfo memberInfo = this.GetEmptyTeammateSlot();
            /// Nếu không tìm thấy
            if (memberInfo == null)
            {
                KTGlobal.AddNotification("Không thể thêm thành viên khi nhóm đã đầy!");
                return;
            }

            memberInfo.RoleData = roleData;
            memberInfo.Enable = true;
            memberInfo.Selected = () => {
                this.selectedTeammate = roleData;
                this.RefreshButtonsState();
                memberInfo.Active = true;
            };
            memberInfo.RefreshTeamLeader();
            this.SortTeamMemberList();
        }

        /// <summary>
        /// Thêm danh sách đội viên tương ứng
        /// </summary>
        /// <param name="roleDatas"></param>
        public void AddTeammates(List<RoleDataMini> roleDatas)
        {
            if (roleDatas == null)
            {
                return;
            }

            foreach (RoleDataMini roleData in roleDatas)
            {
                /// Tìm 1 vị trí trống
                UITeamFrame_TeamMemberInfo memberInfo = this.GetEmptyTeammateSlot();
                /// Nếu không tìm thấy
                if (memberInfo == null)
                {
                    KTGlobal.AddNotification("Không thể thêm thành viên khi nhóm đã đầy!");
                    return;
                }

                memberInfo.RoleData = roleData;
                memberInfo.Enable = true;
                memberInfo.Selected = () => {
                    this.selectedTeammate = roleData;
                    this.RefreshButtonsState();
                    memberInfo.Active = true;
                };
            }
            this.SortTeamMemberList();
            this.RefreshButtonsState();
        }

        /// <summary>
        /// Xóa đội viên
        /// </summary>
        /// <param name="roleID"></param>
        public void RemoveTeammate(int roleID)
        {
            /// Tìm vị trí tương ứng
            UITeamFrame_TeamMemberInfo memberInfo = this.GetTeamMemberSlot(roleID);
            /// Nếu không tìm thấy
            if (memberInfo == null)
            {
                KTGlobal.AddNotification("Đội viên này không tồn tại trong nhóm!");
                return;
            }

            memberInfo.RoleData = null;
            memberInfo.Enable = false;
            memberInfo.Active = false;

            this.SortTeamMemberList();
            this.SelectFirstTeamMember();
            this.RefreshButtonsState();
        }

        /// <summary>
        /// Xóa toàn bộ thành viên trong nhóm
        /// </summary>
        public void RemoveAllTeammates()
        {
            this.EmptyTeammateList();
            this.selectedTeammate = null;
            this.RefreshButtonsState();
        }

        /// <summary>
        /// Cập nhật thông tin đội viên
        /// </summary>
        /// <param name="roleData"></param>
        public void UpdateTeammateAttributes(TeamMemberAttributes teamMember)
        {
            /// Tìm vị trí tương ứng
            UITeamFrame_TeamMemberInfo memberInfo = this.GetTeamMemberSlot(teamMember.RoleID);
            /// Nếu không tìm thấy
            if (memberInfo == null)
            {
                //KTGlobal.AddNotification("Đội viên này không tồn tại trong nhóm!");
                return;
            }

            memberInfo.RoleData.RoleID = teamMember.RoleID;
            memberInfo.RoleData.MapCode = teamMember.MapCode;
            memberInfo.RoleData.PosX = teamMember.PosX;
            memberInfo.RoleData.PosY = teamMember.PosY;
            memberInfo.RoleData.HP = teamMember.HP;
            memberInfo.RoleData.MaxHP = teamMember.MaxHP;
            memberInfo.RoleData.AvartaID = teamMember.AvartaID;
            memberInfo.RoleData.FactionID = teamMember.FactionID;
            memberInfo.RoleData.Level = teamMember.Level;
            memberInfo.RefreshData();
        }

        /// <summary>
        /// Kiểm tra người chơi tương ứng đã có trong nhóm chưa
        /// </summary>
        /// <param name="roleID"></param>
        /// <returns></returns>
        public bool IsTeammateExist(int roleID)
        {
            /// Tìm vị trí tương ứng
            UITeamFrame_TeamMemberInfo memberInfo = this.GetTeamMemberSlot(roleID);

            return memberInfo != null;
        }
        #endregion

        #region Xin vào nhóm
        /// <summary>
        /// Xóa người chơi xin vào nhóm tương ứng
        /// </summary>
        /// <param name="roleID"></param>
        public void RemoveWaitingPlayer(int roleID)
        {
            /// Tìm vị trí tương ứng
            UITeamFrame_AskToJoinPlayerInfo playerInfo = this.GetAskToJoinSlot(roleID);
            /// Nếu không tìm thấy
            if (playerInfo == null)
            {
                //KTGlobal.AddNotification("Người chơi này không tồn tại hoặc đã rời mạng!");
                return;
            }

            GameObject.Destroy(playerInfo.gameObject);
            this.RebuildLayout(this.waitingPlayerListTransform);
        }

        /// <summary>
        /// Thêm người chơi vào danh sách xin vào nhóm
        /// </summary>
        /// <param name="roleData"></param>
        public void AddWaitingPlayer(RoleDataMini roleData)
        {
            /// Tìm vị trí tương ứng
            UITeamFrame_AskToJoinPlayerInfo playerInfo = this.GetAskToJoinSlot(roleData.RoleID);
            /// Nếu đã có trong danh sách chờ
            if (playerInfo != null)
            {
                return;
            }

            playerInfo = GameObject.Instantiate<UITeamFrame_AskToJoinPlayerInfo>(this.UI_AskToJoinPlayerInfoPrefab);
            playerInfo.gameObject.SetActive(true);
            playerInfo.transform.SetParent(this.waitingPlayerListTransform, false);
            playerInfo.RoleData = roleData;
            playerInfo.Agree = () => {
                this.AgreeJoinTeam?.Invoke(roleData);
            };
            playerInfo.Refuse = () => {
                this.RefuseJoinTeam?.Invoke(roleData);
            };
        }
        #endregion
        #endregion
    }
}
