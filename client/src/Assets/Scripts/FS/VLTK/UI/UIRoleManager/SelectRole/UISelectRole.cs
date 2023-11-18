using FS.GameEngine.Logic;
using FS.GameEngine.Network;
using FS.GameFramework.Logic;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Lớp dữ liệu nhân vật nhận từ socket
/// </summary>
public class RoleSelectorData
{
    /// <summary>
    /// ID nhân vật
    /// </summary>
    public int ID { get; set; }

    /// <summary>
    /// Giới tính (0~Nữ, 1~Nam)
    /// </summary>
    public int Sex { get; set; }

    /// <summary>
    /// Môn phái
    /// </summary>
    public int FactionID { get; set; }

    /// <summary>
    /// Tên nhân vật
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Cấp độ
    /// </summary>
    public int Level { get; set; }

    /// <summary>
    /// ID bản đồ đang đứng
    /// </summary>
    public int MapCode { get; set; }

    /// <summary>
    /// ID áo
    /// </summary>
    public int ArmorID { get; set; }

    /// <summary>
    /// ID mũ
    /// </summary>
    public int HelmID { get; set; }

    /// <summary>
    /// ID vũ khí
    /// </summary>
    public int WeaponID { get; set; }

    /// <summary>
    /// Cấp cường hóa vũ khí
    /// </summary>
    public int WeaponEnhanceLevel { get; set; }

    /// <summary>
    /// ID phi phong
    /// </summary>
    public int MantleID { get; set; }
}


namespace FS.VLTK.UI.RoleManager
{
    /// <summary>
    /// Khung chọn nhân vật
    /// </summary>
    public class UISelectRole : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Nút quay lại màn hình đăng nhập
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_Back;

        /// <summary>
        /// Nút thoát Game
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_ExitGame;

        /// <summary>
        /// Nút tạo nhân vật
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_CreateRole;

        /// <summary>
        /// Nút vào Game
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_EnterGame;

        /// <summary>
        /// Toggle thông tin nhân vật 1
        /// </summary>
        [SerializeField]
        private UISelectRole_RoleInfoToggle UIToggle_RoleInfo1;

        /// <summary>
        /// Toggle thông tin nhân vật 2
        /// </summary>
        [SerializeField]
        private UISelectRole_RoleInfoToggle UIToggle_RoleInfo2;

        /// <summary>
        /// Toggle thông tin nhân vật 3
        /// </summary>
        [SerializeField]
        private UISelectRole_RoleInfoToggle UIToggle_RoleInfo3;

        /// <summary>
        /// Toggle thông tin nhân vật 4
        /// </summary>
        [SerializeField]
        private UISelectRole_RoleInfoToggle UIToggle_RoleInfo4;
        #endregion


        /// <summary>
        /// Dữ liệu nhân vật nhận được từ socket
        /// </summary>
        private readonly List<RoleSelectorData> ListRole = new List<RoleSelectorData>();

        #region Properties
        /// <summary>
        /// Tải xuống hoàn tất
        /// </summary>
        public bool IsReady
        {
            get
            {
                return !this.UIToggle_RoleInfo1.IsBusy && !this.UIToggle_RoleInfo2.IsBusy && !this.UIToggle_RoleInfo3.IsBusy && !UIToggle_RoleInfo4.IsBusy;
            }
        }

        /// <summary>
        /// Sự kiện vào game với nhân vật được chọn
        /// </summary>
        public Action StartGameByRole { get; set; } = null;

        /// <summary>
        /// Sự kiện khi nút thoát Game được ấn
        /// </summary>
        public Action QuitGame { get; set; }

        /// <summary>
        /// Sự kiện khi nút quay lại màn hình Chọn máy chủ được ấn
        /// </summary>
        public Action BackToSelectServer { get; set; }

        /// <summary>
        /// Sự kiện khi nút xóa nhân vật được ấn
        /// </summary>
        public Action DeleteRole { get; set; }

        /// <summary>
        /// Sự kiện khi nút tạo nhân vật được ấn
        /// </summary>
        public Action CreateRole { get; set; }

        /// <summary>
        /// Sự kiện khi nút Vào game được ấn
        /// </summary>
        public Action EnterGame { get; set; }

        /// <summary>
        /// Nhân vật được chọn gần nhất
        /// </summary>
        public RoleSelectorData LastSelectedRole { get; private set; } = null;

        /// <summary>
        /// Tổng số nhân vật
        /// </summary>
        public int RolesCount
        {
            get
            {
                return this.ListRole.Count;
            }
        }
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi đến ở Frame đầu tiên
        /// </summary>
        private void Start()
        {
            Super.ShowNetWaiting("Đang tải danh sách nhân vật, xin chờ giây lát...");
            this.InitPrefabs();
        }
        #endregion

        #region Code UI
        /// <summary>
        /// Khởi tạo ban đầu
        /// </summary>
        private void InitPrefabs()
        {
            this.UIToggle_RoleInfo1.gameObject.SetActive(false);
            this.UIToggle_RoleInfo2.gameObject.SetActive(false);
            this.UIToggle_RoleInfo3.gameObject.SetActive(false);
            this.UIToggle_RoleInfo4.gameObject.SetActive(false);

            this.UIButton_Back.onClick.AddListener(this.ButtonBack_Clicked);
            this.UIButton_ExitGame.onClick.AddListener(this.ButtonExitGame_Clicked);
            this.UIButton_CreateRole.onClick.AddListener(this.ButtonCreateRole_Clicked);
            this.UIButton_EnterGame.onClick.AddListener(this.ButtonEnterGame_Clicked);
        }

        /// <summary>
        /// Sự kiện khi nút quay lại được ấn
        /// </summary>
        private void ButtonBack_Clicked()
        {
            this.BackToSelectServer?.Invoke();
        }

        /// <summary>
        /// Sự kiện khi nút thoát Game được ấn
        /// </summary>
        private void ButtonExitGame_Clicked()
        {
            this.QuitGame?.Invoke();
        }

        /// <summary>
        /// Sự kiện khi nút tạo nhân vật được ấn
        /// </summary>
        private void ButtonCreateRole_Clicked()
        {
            this.CreateRole?.Invoke();
        }

        /// <summary>
        /// Sự kiện khi nút vào game được ấn
        /// </summary>
        private void ButtonEnterGame_Clicked()
        {
            this.EnterGame?.Invoke();
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Làm rỗng dữ liệu nhân vật
        /// </summary>
        private void ClearRoleList()
        {
            this.ListRole.Clear();

            this.UIToggle_RoleInfo1.Active = false;
            this.UIToggle_RoleInfo2.Active = false;
            this.UIToggle_RoleInfo3.Active = false;
            this.UIToggle_RoleInfo4.Active = false;

            this.UIToggle_RoleInfo1.gameObject.SetActive(false);
            this.UIToggle_RoleInfo2.gameObject.SetActive(false);
            this.UIToggle_RoleInfo3.gameObject.SetActive(false);
            this.UIToggle_RoleInfo4.gameObject.SetActive(false);
        }

        /// <summary>
        /// Chọn nhân vật từ danh sách
        /// </summary>
        /// <param name="id"></param>
        private void SelectRole(int id)
        {
            if (id < 0 || id > 3)
            {
                Super.ShowMessageBox("Lỗi", "Thông tin nhân vật được chọn bị lỗi.");
                return;
            }

            this.LastSelectedRole = id == 0 ? this.UIToggle_RoleInfo1.RoleData : id == 1 ? this.UIToggle_RoleInfo2.RoleData : id == 2 ? this.UIToggle_RoleInfo3.RoleData : this.UIToggle_RoleInfo4.RoleData;
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Thêm mới nhân vật thông tin được gửi từ socket
        /// </summary>
        /// <param name="s"></param>
        /// <param name="isAddToLogin"></param>
        public void AddRoleList(string s, bool isAddToLogin = false)
        {
            if (s == "")
            {
                return;
            }

            this.ClearRoleList();

            string[] roles = s.Split('|');
            int lastId = -1;
            for (int i = 0; i < roles.Length; i++)
            {
                string[] temp = roles[i].Split('$');
                if (temp.Length != 7)
                {
                    continue;
                }

                RoleSelectorData data = new RoleSelectorData()
                {
                    ID = Convert.ToInt32(temp[0]),
                    Sex = Convert.ToInt32(temp[1]),
                    FactionID = Convert.ToInt32(temp[2]),
                    Name = temp[3],
                    Level = Convert.ToInt32(temp[4]),
                    MapCode = Convert.ToInt32(temp[5]),
                };
                string[] equipInfos = temp[6].Split('_');
                if (equipInfos.Length == 5)
                {
                    data.ArmorID = Convert.ToInt32(equipInfos[0]);
                    data.HelmID = Convert.ToInt32(equipInfos[1]);
                    data.WeaponID = Convert.ToInt32(equipInfos[2]);
                    data.WeaponEnhanceLevel = Convert.ToInt32(equipInfos[3]);
                    data.MantleID = Convert.ToInt32(equipInfos[4]);
                }
                this.ListRole.Add(data);

                if (!isAddToLogin)
                {
                    lastId = i;
                    if (i == 0)
                    {
                        this.UIToggle_RoleInfo1.gameObject.SetActive(true);
                        this.UIToggle_RoleInfo1.RoleData = data;
                        this.UIToggle_RoleInfo1.OnSelected = (isSelected) => {
                            this.SelectRole(0);
                        };
                        this.UIToggle_RoleInfo1.Ready = () => {
                            if (this.IsReady)
                            {
                                Super.HideNetWaiting();
                            }
                        };
                    }
                    else if (i == 1)
                    {
                        this.UIToggle_RoleInfo2.gameObject.SetActive(true);
                        this.UIToggle_RoleInfo2.RoleData = data;
                        this.UIToggle_RoleInfo2.OnSelected = (isSelected) => {
                            this.SelectRole(1);
                        };
                        this.UIToggle_RoleInfo2.Ready = () => {
                            if (this.IsReady)
                            {
                                Super.HideNetWaiting();
                            }
                        };
                    }
                    else if (i == 2)
                    {
                        this.UIToggle_RoleInfo3.gameObject.SetActive(true);
                        this.UIToggle_RoleInfo3.RoleData = data;
                        this.UIToggle_RoleInfo3.OnSelected = (isSelected) => {
                            this.SelectRole(2);
                        };
                        this.UIToggle_RoleInfo3.Ready = () => {
                            if (this.IsReady)
                            {
                                Super.HideNetWaiting();
                            }
                        };
                    }
                    else if (i == 3)
                    {
                        this.UIToggle_RoleInfo4.gameObject.SetActive(true);
                        this.UIToggle_RoleInfo4.RoleData = data;
                        this.UIToggle_RoleInfo4.OnSelected = (isSelected) => {
                            this.SelectRole(3);
                        };
                        this.UIToggle_RoleInfo4.Ready = () => {
                            if (this.IsReady)
                            {
                                Super.HideNetWaiting();
                            }
                        };
                    }
                }
            }

            if (!isAddToLogin && lastId != -1)
            {
                this.SelectRole(lastId);
            }
        }

        /// <summary>
        /// Thực hiện hành động vào game trực tiếp
        /// </summary>
        /// <param name="showWaiting"></param>
        public void DirectEnterGame()
        {
            GameInstance.Game.CurrentSession.RoleID = this.ListRole[this.ListRole.Count - 1].ID;
            GameInstance.Game.CurrentSession.RoleSex = this.ListRole[this.ListRole.Count - 1].Sex;
            GameInstance.Game.CurrentSession.RoleName = this.ListRole[this.ListRole.Count - 1].Name;
            Super.ShowNetWaiting("Đang vào game...");

            GameInstance.Game.InitPlayGame();
        }

        /// <summary>
        /// Thực hiện hành động vào game
        /// </summary>
        public void StartGame()
        {
            this.StartGameByRole?.Invoke();
        }
        #endregion
    }
}