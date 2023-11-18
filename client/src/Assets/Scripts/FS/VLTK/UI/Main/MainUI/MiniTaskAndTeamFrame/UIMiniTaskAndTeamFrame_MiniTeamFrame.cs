using FS.GameEngine.Logic;
using Server.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FS.VLTK.UI.Main.MainUI.MiniTaskAndTeamFrame
{
    /// <summary>
    /// Khung MiniTeamFrame trong nhóm MiniTask và MiniTeamFrame
    /// </summary>
    public class UIMiniTaskAndTeamFrame_MiniTeamFrame : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Prefab thông tin đội viên
        /// </summary>
        [SerializeField]
        private UIMiniTaskAndTeamFrame_MiniTeamFrame_TeammateInfo UI_TeammateInfosPrefab;

        /// <summary>
        /// Button mở bảng thông tin đội
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_OpenTeamBox;
        #endregion

        /// <summary>
        /// RectTransform danh sách đội viên
        /// </summary>
        private RectTransform teamListTransform = null;

        #region Properties
        /// <summary>
        /// Sự kiện khi Button mở bảng thông tin đội được ấn
        /// </summary>
        public Action OpenTeamBox { get; set; }

        /// <summary>
        /// Sự kiện hiển thị Dropdown thông tin đội viên
        /// </summary>
        public Action<RoleDataMini> ShowTeammateDetails { get; set; }
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi khi đối tượng được tạo ra
        /// </summary>
        private void Awake()
        {
            this.teamListTransform = this.UI_TeammateInfosPrefab.transform.parent.gameObject.GetComponent<RectTransform>();
        }

        /// <summary>
        /// Hàm này gọi ở Frame đầu tiên
        /// </summary>
        private void Start()
        {
            this.InitPrefabs();
        }

        /// <summary>
        /// Hàm này gọi khi đối tượng được kích hoạt
        /// </summary>
        private void OnEnable()
        {
            this.RebuildLayout();
        }

        /// <summary>
        /// Hàm này gọi khi đối tượng bị hủy kích hoạt
        /// </summary>
        private void OnDisable()
        {
            this.StopAllCoroutines();
        }
        #endregion

        #region Code UI
        /// <summary>
        /// Khởi tạo ban đầu
        /// </summary>
        private void InitPrefabs()
        {
            this.UIButton_OpenTeamBox.onClick.AddListener(this.ButtonOpenTeamBox_Clicked);
        }

        /// <summary>
        /// Sự kiện khi Button mở bảng thông tin đội được ấn
        /// </summary>
        private void ButtonOpenTeamBox_Clicked()
        {
            this.OpenTeamBox?.Invoke();
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Thực thi bỏ qua một số Frame
        /// </summary>
        /// <param name="skip"></param>
        /// <param name="callBack"></param>
        /// <returns></returns>
        private IEnumerator ExecuteSkipFrames(int skip, Action callBack)
        {
            for (int i = 1; i <= skip; i++)
            {
                yield return null;
            }
            callBack?.Invoke();
        }

        /// <summary>
        /// Xây lại Layout
        /// </summary>
        private void RebuildLayout()
        {
            if (this.gameObject.activeSelf)
            {
                this.StartCoroutine(this.ExecuteSkipFrames(1, () => {
                    UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(this.teamListTransform);
                }));
            }
        }

        /// <summary>
        /// Làm rỗng danh sách đội viên
        /// </summary>
        private void ClearTeamList()
        {
            foreach (Transform child in this.teamListTransform.transform)
            {
                if (child.gameObject != this.UI_TeammateInfosPrefab.gameObject)
                {
                    GameObject.Destroy(child.gameObject);
                }
            }
        }

        /// <summary>
        /// Tìm đội viên tương ứng trong danh sách
        /// </summary>
        /// <param name="roleID"></param>
        /// <returns></returns>
        private UIMiniTaskAndTeamFrame_MiniTeamFrame_TeammateInfo FindTeamMember(int roleID)
        {
            foreach (Transform child in this.teamListTransform.transform)
            {
                if (child.gameObject != this.UI_TeammateInfosPrefab.gameObject)
                {
                    UIMiniTaskAndTeamFrame_MiniTeamFrame_TeammateInfo uiTeammateInfo = child.gameObject.GetComponent<UIMiniTaskAndTeamFrame_MiniTeamFrame_TeammateInfo>();
                    if (uiTeammateInfo == null)
                    {
                        continue;
                    }

                    if (uiTeammateInfo.Data.RoleID == roleID)
                    {
                        return uiTeammateInfo;
                    } 
                }
            }
            return null;
        }

        /// <summary>
        /// Sắp xếp lại danh sách tổ đội
        /// </summary>
        private void SortTeamList()
        {
            /// Tìm đội trưởng, cho lên vị trí đầu tiên
            foreach (Transform child in this.teamListTransform.transform)
            {
                if (child.gameObject != this.UI_TeammateInfosPrefab.gameObject)
                {
                    UIMiniTaskAndTeamFrame_MiniTeamFrame_TeammateInfo uiTeammateInfo = child.gameObject.GetComponent<UIMiniTaskAndTeamFrame_MiniTeamFrame_TeammateInfo>();
                    if (uiTeammateInfo == null)
                    {
                        continue;
                    }

                    if (uiTeammateInfo.Data.RoleID == uiTeammateInfo.Data.TeamLeaderID)
                    {
                        uiTeammateInfo.transform.SetAsFirstSibling();
                        break;
                    }
                }
            }
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Thêm danh sách đội viên vào danh sách
        /// </summary>
        /// <param name="roleDatas"></param>
        public void AddTeamMembers(List<RoleDataMini> roleDatas)
        {
            foreach (RoleDataMini roleData in roleDatas)
            {
                UIMiniTaskAndTeamFrame_MiniTeamFrame_TeammateInfo uiTeammateInfo = GameObject.Instantiate<UIMiniTaskAndTeamFrame_MiniTeamFrame_TeammateInfo>(this.UI_TeammateInfosPrefab);
                uiTeammateInfo.transform.SetParent(this.teamListTransform, false);
                uiTeammateInfo.gameObject.SetActive(true);
                uiTeammateInfo.Data = roleData;
                uiTeammateInfo.MoreFunctionsClicked = (teammateData) => {
                    this.ShowTeammateDetails?.Invoke(roleData);
                };
            }
            this.RebuildLayout();
        }

        /// <summary>
        /// Thêm đội viên vào danh sách
        /// </summary>
        /// <param name="roleData"></param>
        public void AddTeamMember(RoleDataMini roleData)
        {
            UIMiniTaskAndTeamFrame_MiniTeamFrame_TeammateInfo uiTeammateInfo = GameObject.Instantiate<UIMiniTaskAndTeamFrame_MiniTeamFrame_TeammateInfo>(this.UI_TeammateInfosPrefab);
            uiTeammateInfo.transform.SetParent(this.teamListTransform, false);
            uiTeammateInfo.gameObject.SetActive(true);
            uiTeammateInfo.Data = roleData;
            uiTeammateInfo.MoreFunctionsClicked = (teammateData) => {
                this.ShowTeammateDetails?.Invoke(roleData);
            };
            this.RebuildLayout();
        }

        /// <summary>
        /// Xóa thành viên
        /// </summary>
        /// <param name="roleID"></param>
        public void RemoveTeamMember(int roleID)
        {
            /// Đội viên hiện tại
            UIMiniTaskAndTeamFrame_MiniTeamFrame_TeammateInfo uiTeammateInfo = this.FindTeamMember(roleID);
            /// Nếu không tìm thấy đội viên
            if (uiTeammateInfo == null)
            {
                return;
            }

            GameObject.Destroy(uiTeammateInfo.gameObject);

            this.SortTeamList();
            this.RebuildLayout();
        }

        /// <summary>
        /// Xóa toàn bộ thành viên đội
        /// </summary>
        public void RemoveAllTeamMembers()
        {
            this.ClearTeamList();
            this.RebuildLayout();
        }

        /// <summary>
        /// Cập nhật thông tin chỉ số đội viên
        /// </summary>
        /// <param name="teamMember"></param>
        public void UpdateTeammateAttributes(TeamMemberAttributes teamMember)
        {
            /// Đội viên hiện tại
            UIMiniTaskAndTeamFrame_MiniTeamFrame_TeammateInfo uiTeammateInfo = this.FindTeamMember(teamMember.RoleID);
            /// Nếu không tìm thấy đội viên
            if (uiTeammateInfo == null)
            {
                return;
            }

            uiTeammateInfo.Data.RoleID = teamMember.RoleID;
            uiTeammateInfo.Data.MapCode = teamMember.MapCode;
            uiTeammateInfo.Data.PosX = teamMember.PosX;
            uiTeammateInfo.Data.PosY = teamMember.PosY;
            uiTeammateInfo.Data.HP = teamMember.HP;
            uiTeammateInfo.Data.MaxHP = teamMember.MaxHP;
            uiTeammateInfo.Data.FactionID = teamMember.FactionID;
            uiTeammateInfo.Data.AvartaID = teamMember.AvartaID;
            uiTeammateInfo.Data.Level = teamMember.Level;
            uiTeammateInfo.RefreshData();
        }

        /// <summary>
        /// Tạo nhóm mới từ Server gửi về
        /// </summary>
        public void ServerCreateTeam()
        {
            /// Thêm bản thân vào danh sách
            this.AddTeamMember(new RoleDataMini()
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

                /// TODO
                ArmorID = 0,
                HelmID = 0,
                WeaponID = 0,
                MantleID = 0,
                WeaponEnhanceLevel = 0,

                AvartaID = Global.Data.RoleData.RolePic,
            });

            this.SortTeamList();
        }

        /// <summary>
        /// Thay đổi trưởng nhóm từ Server gửi về
        /// </summary>
        /// <param name="roleID"></param>
        public void ServerChangeTeamLeader(int roleID)
        {
            /// Duyệt danh sách thành viên
            foreach (Transform child in this.teamListTransform)
            {
                if (child.gameObject != this.teamListTransform.gameObject)
                {
                    UIMiniTaskAndTeamFrame_MiniTeamFrame_TeammateInfo memberInfo = child.gameObject.GetComponent<UIMiniTaskAndTeamFrame_MiniTeamFrame_TeammateInfo>();
                    /// Nếu đây là vị trí trống
                    if (memberInfo.Data == null)
                    {
                        continue;
                    }

                    /// Thiết lập trưởng nhóm mới
                    memberInfo.Data.TeamLeaderID = roleID;
                    memberInfo.RefreshTeamLeader();
                }
            }

            /// Sắp xếp lại danh sách
            this.SortTeamList();
        }
        #endregion
    }
}

