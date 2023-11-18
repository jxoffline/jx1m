using FS.VLTK.UI.Main.GuildEx.Battle;
using Server.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace FS.VLTK.UI.Main.GuildEx
{
    /// <summary>
    /// Khung công thành chiến bang hội
    /// </summary>
    public class UIGuildEx_Battle : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Button đóng khung
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_Close;

        /// <summary>
        /// Text tên thành
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_CityName;

        /// <summary>
        /// Text chủ thành
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_HostName;

        /// <summary>
        /// Text ngày thi đấu loại
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_TeamFightDay;

        /// <summary>
        /// Text ngày công thành
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_BattleDay;

        /// <summary>
        /// Text tổng số bang đăng ký
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_TotalGuilds;

        /// <summary>
        /// Button hiện danh sách bang hội đăng ký
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_ShowGuildList;

        /// <summary>
        /// Danh sách thành viên bang hội tham gia
        /// </summary>
        [SerializeField]
        private UIGuildEx_Battle_MemberInfo UI_MemberInfoPrefab;

        /// <summary>
        /// Button đăng ký công thành
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_Register;

        /// <summary>
        /// Khung danh sách bang hội đăng ký công thành chiên
        /// </summary>
        [SerializeField]
        private UIGuildEx_Battle_GuildListFrame UIFrame_GuildList;

        /// <summary>
        /// Khung danh sách thành viên bang hội có thể đăng ký tham gia công thành chiến
        /// </summary>
        [SerializeField]
        private UIGuildEx_Battle_MemberListFrame UIFrame_MemberList;
        #endregion

        #region Properties
        private GuildWarInfo _Data;
        /// <summary>
        /// Dữ liệu
        /// </summary>
        public GuildWarInfo Data
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
        /// Sự kiện đăng ký công thành chiến
        /// </summary>
        public Action<List<int>> Register { get; set; }
        #endregion

        #region Private fields
        /// <summary>
        /// RectTransform danh sách thành viên
        /// </summary>
        private RectTransform transformMemberList;

        /// <summary>
        /// Danh sách thành viên được chọn
        /// </summary>
        private readonly List<int> selectedMembers = new List<int>();
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
            this.UIButton_ShowGuildList.onClick.AddListener(this.ButtonShowGuildList_Clicked);
            this.UIButton_Register.onClick.AddListener(this.ButtonRegister_Clicked);
        }

        /// <summary>
        /// Sự kiện khi Button đóng khung được ấn
        /// </summary>
        private void ButtonClose_Clicked()
        {
            this.Close?.Invoke();
        }

        /// <summary>
        /// Sự kiện khi Button hiện danh sách bang hội đăng ký công thành chiến được ấn
        /// </summary>
        private void ButtonShowGuildList_Clicked()
        {
            /// Nếu không có bang nào
            if (this._Data.GuildResgister == null)
            {
                KTGlobal.AddNotification("Hiện không có bang hội nào đăng ký công thành.");
                return;
            }

            /// Hiện khung
            this.UIFrame_GuildList.Data = this._Data.GuildResgister;
            this.UIFrame_GuildList.Show();
        }

        /// <summary>
        /// Sự kiện khi Button đăng ký công thành chiến được ấn
        /// </summary>
        private void ButtonRegister_Clicked()
        {
            /// Nếu không phải thời gian đăng ký
            if (this._Data.Status != 0)
            {
                KTGlobal.AddNotification("Hiện không phải thời gian đăng ký công thành chiến.");
                return;
            }
            /// Nếu đội chưa đủ 6 người
            else if (this.selectedMembers.Count < 6)
            {
                KTGlobal.AddNotification("Phải có đủ 6 thành viên mới có thể đăng ký công thành chiến.");
                return;
            }

            /// Thực thi sự kiện
            this.Register?.Invoke(this.selectedMembers);
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

            /// Xóa danh sách thành viên
            this.selectedMembers.Clear();
            foreach (Transform child in this.transformMemberList.transform)
            {
                if (child.gameObject != this.UI_MemberInfoPrefab.gameObject)
                {
                    GameObject.Destroy(child.gameObject);
                }
            }

            /// Danh sách các Slot
            UIGuildEx_Battle_MemberInfo[] uiMembers = new UIGuildEx_Battle_MemberInfo[6];
            /// Sinh sẵn ra 6 slot
            for (int i = 1; i <= 6; i++)
            {
                UIGuildEx_Battle_MemberInfo uiMemberInfo = GameObject.Instantiate<UIGuildEx_Battle_MemberInfo>(this.UI_MemberInfoPrefab);
                uiMemberInfo.transform.SetParent(this.transformMemberList, false);
                uiMemberInfo.gameObject.SetActive(true);
                uiMemberInfo.Data = null;
                uiMemberInfo.EnableEdit = this._Data.Status == 0;
                //Thiếu cái này
                uiMembers[i-1] = uiMemberInfo;
            }

            /// Nếu không phải lúc đăng ký
            if (this._Data.Status != 0)
            {
                if (this._Data.MemberRegister != null)
                {
                    /// Vị trí
                    int idx = -1;
                    /// Duyệt danh sách thành viên đã đăng ký
                    foreach (string memberName in this._Data.MemberRegister)
                    {
                        /// Tăng vị trí
                        idx++;
                        /// Thêm vào vị trí tương ứng
                        uiMembers[idx].Data = memberName;
                        uiMembers[idx].EnableEdit = false;
                    }
                    /// Ẩn button
                    /// 
                }
                this.UIButton_Register.interactable = false;
            }
            /// Nếu là lúc đăng ký
            else
            {
                /// Hiện button
                this.UIButton_Register.interactable = true;

                /// Duyệt danh sách
                foreach (UIGuildEx_Battle_MemberInfo uiMemberInfo in uiMembers)
                {
                    /// Sự kiện thêm thành viên
                    uiMemberInfo.Add = () =>
                    {
                        /// Mở khung chọn thành viên
                        this.UIFrame_MemberList.Data = this._Data.ListMemberCanPick.Where(x => !this.selectedMembers.Contains(x.RoleID)).ToList();
                        this.UIFrame_MemberList.Select = (memberInfo) =>
                        {
                            /// Thêm vào danh sách
                            this.selectedMembers.Add(memberInfo.RoleID);
                            /// Hiện vào danh sách đang xem
                            uiMemberInfo.Data = memberInfo.RoleName;
                            uiMemberInfo.RoleID = memberInfo.RoleID;
                        };
                        this.UIFrame_MemberList.Show();
                    };
                    /// Sự kiện xóa thành viên
                    uiMemberInfo.Remove = () =>
                    {
                        /// Không có dữ liệu
                        if (uiMemberInfo.Data == null)
                        {
                            /// Bỏ qua
                            return;
                        }

                        uiMemberInfo.Data = null;
                        /// Xóa khỏi danh sách
                        this.selectedMembers.Remove(uiMemberInfo.RoleID);

                      //  this.RefreshData();
                    };
                }
            }

            /// Thông tin khác
            this.UIText_CityName.text = this._Data.ListCity[0].CityName;
            this.UIText_HostName.text = this._Data.ListCity[0].HostName;
            this.UIText_TeamFightDay.text = this.DayToString(this._Data.ListCity[0].TeamFightDay);
            this.UIText_BattleDay.text = this.DayToString(this._Data.ListCity[0].CityFightDay);

            this.UIText_TotalGuilds.text = this._Data.GuildResgister == null ? "0" : this._Data.GuildResgister.Count.ToString();
            this.UIButton_ShowGuildList.interactable = this._Data.GuildResgister != null;

            /// Xây lại giao diện
            this.ExecuteSkipFrames(1, () =>
            {
                UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(this.transformMemberList);
            });
        }

        /// <summary>
        /// Chuyển ngày sang chuỗi
        /// </summary>
        /// <param name="day"></param>
        /// <returns></returns>
        private string DayToString(int day)
        {
            switch (day)
            {
                case 1:
                {
                    return "Thứ 2";
                }
                case 2:
                {
                    return "Thứ 3";
                }
                case 3:
                {
                    return "Thứ 4";
                }
                case 4:
                {
                    return "Thứ 5";
                }
                case 5:
                {
                    return "Thứ 6";
                }
                case 6:
                {
                    return "Thứ 7";
                }
                case 7:
                {
                    return "Chủ nhật";
                }
            }
            return "";
        }
        #endregion
    }
}
