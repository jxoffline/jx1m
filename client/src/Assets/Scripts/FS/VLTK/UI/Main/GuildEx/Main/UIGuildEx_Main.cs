using FS.GameEngine.Logic;
using FS.VLTK.UI.Main.GuildEx.Dedicate;
using FS.VLTK.UI.Main.GuildEx.Main;
using Server.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static FS.VLTK.Entities.Enum;

namespace FS.VLTK.UI.Main.GuildEx
{
    /// <summary>
    /// Khung bang hội tổng quan
    /// </summary>
    public class UIGuildEx_Main : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Button đóng khung
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_Close;

        /// <summary>
        /// Text tên bang
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_GuildName;

        /// <summary>
        /// Text tên bang chủ
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_MasterName;

        /// <summary>
        /// Text cấp độ bang
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_GuildLevel;

        /// <summary>
        /// Slider kinh nghiệm bang
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Slider UISlider_GuildExp;

        /// <summary>
        /// Text kinh nghiệm bang
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_GuildExp;

        /// <summary>
        /// Text tổng số thành viên bang
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_MemberCount;

        /// <summary>
        /// Text tổng số phụ bản đã mở
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_CopySceneCount;

        /// <summary>
        /// Button thăng cấp bang
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_LevelUp;

        /// <summary>
        /// Text công cáo bang
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_Notification;

        /// <summary>
        /// Button sửa công cáo
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_ChangeNotification;

        /// <summary>
        /// Text tổng cống hiến bang
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_TotalMoney;

        /// <summary>
        /// Prefab tổng số vật phẩm
        /// </summary>
        [SerializeField]
        private UIGuildEx_Main_ItemInfo UI_ItemInfoPrefab;

        /// <summary>
        /// Text tổng cống hiến cá nhân
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_SelfTotalDedication;

        /// <summary>
        /// Text cống hiến cá nhân trong tuần
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_SelfWeekDedication;

        /// <summary>
        /// Button mở khung kỹ năng bang
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_OpenGuildSkills;

        /// <summary>
        /// Button mở khung công thành chiến bang
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_OpenGuildBattle;

        /// <summary>
        /// Button mở khung liên minh bang
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_OpenGuildAllies;

        /// <summary>
        /// Button mở khung thành viên bang
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_OpenGuildMemberList;

        /// <summary>
        /// Button mở khung cống hiến bang
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_OpenGuildDedication;

        /// <summary>
        /// Button mở khung nhiệm vụ bang
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_OpenGuildQuest;

        /// <summary>
        /// Button mở khung cửa hàng bang hội
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_OpenGuildShop;

        /// <summary>
        /// Button rời bang
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_QuitGuild;

        /// <summary>
        /// Khung sửa công cáo bang hội
        /// </summary>
        [SerializeField]
        private UiGuildEx_Main_ChangeNotification UI_ChangeNotificationFrame;
        #endregion

        #region Properties
        private GuildInfo _Data;
        /// <summary>
        /// Dữ liệu bang hội
        /// </summary>
        public GuildInfo Data
        {
            get
            {
                return this._Data;
            }
            set
            {
                this._Data = value;
                this.RefreshData();
            }
        }

        /// <summary>
        /// Sự kiện đóng khung
        /// </summary>
        public Action Close { get; set; }

        /// <summary>
        /// Sự kiện thăng cấp bang
        /// </summary>
        public Action LevelUp { get; set; }

        /// <summary>
        /// Sự kiện sửa công cáo
        /// </summary>
        public Action<string> ChangeNotification { get; set; }

        /// <summary>
        /// Sự kiện mở khung kỹ năng bang
        /// </summary>
        public Action OpenGuildSkills { get; set; }

        /// <summary>
        /// Sự kiện mở khung công thành chiến bang
        /// </summary>
        public Action OpenGuildBattle { get; set; }

        /// <summary>
        /// Sự kiện mở khung liên minh bang
        /// </summary>
        public Action OpenGuildAllies { get; set; }

        /// <summary>
        /// Sự kiện mở khung thành viên bang
        /// </summary>
        public Action OpenGuildMemberList { get; set; }

        /// <summary>
        /// Sự kiện mở khung cống hiến bang
        /// </summary>
        public Action OpenGuildDedication { get; set; }

        /// <summary>
        /// Sự kiện mở khung nhiệm vụ bang
        /// </summary>
        public Action OpenGuildQuest { get; set; }

        /// <summary>
        /// Sự kiện mở khung cửa hàng bang hội
        /// </summary>
        public Action OpenGuildShop { get; set; }

        /// <summary>
        /// Sự kiện rời bang
        /// </summary>
        public Action QuitGuild { get; set; }
        #endregion

        #region Private fields
        /// <summary>
        /// RectTransform danh sách vật phẩm
        /// </summary>
        private RectTransform itemListTransform;
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi khi đối tượng được tạo ra
        /// </summary>
        private void Awake()
        {
            this.itemListTransform = this.UI_ItemInfoPrefab.transform.parent.GetComponent<RectTransform>();
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
            this.UIButton_LevelUp.onClick.AddListener(this.ButtonLevelUp_Clicked);
            this.UIButton_ChangeNotification.onClick.AddListener(this.ButtonChangeNotification_Clicked);
            this.UIButton_OpenGuildSkills.onClick.AddListener(this.ButtonOpenGuildSkills_Clicked);
            this.UIButton_OpenGuildBattle.onClick.AddListener(this.ButtonOpenGuildBattle_Clicked);
            this.UIButton_OpenGuildAllies.onClick.AddListener(this.ButtonOpenGuildAllies_Clicked);
            this.UIButton_OpenGuildMemberList.onClick.AddListener(this.ButtonOpenGuildMemberList_Clicked);
            this.UIButton_OpenGuildDedication.onClick.AddListener(this.ButtonOpenGuildDedication_Clicked);
            this.UIButton_OpenGuildQuest.onClick.AddListener(this.ButtonOpenGuildQuest_Clicked);
            this.UIButton_OpenGuildShop.onClick.AddListener(this.ButtonOpenGuildShop_Clicked);
            this.UIButton_QuitGuild.onClick.AddListener(this.ButtonQuitGuild_Clicked);
        }

        /// <summary>
        /// Sự kiện khi Button đóng khung được ấn
        /// </summary>
        private void ButtonClose_Clicked()
        {
            this.Close?.Invoke();
        }

        /// <summary>
        /// Sự kiện khi Button thăng cấp bang được ấn
        /// </summary>
        private void ButtonLevelUp_Clicked()
        {
            /// Nếu bản thân không có bang hội
            if (Global.Data.RoleData.GuildID <= 0)
            {
                KTGlobal.AddNotification("Bạn chưa gia nhập bang hội nào, không thể thực hiện thăng cấp!");
                return;
            }
            /// Nếu không phải bang chủ hoặc phó bang chủ
            else if (Global.Data.RoleData.GuildRank != (int) GuildRank.Master && Global.Data.RoleData.GuildRank != (int) GuildRank.ViceMaster)
            {
                KTGlobal.AddNotification("Chỉ có bang chủ hoặc phó bang chủ mới có quyền thăng cấp bang hội!");
                return;
            }
            this.LevelUp?.Invoke();
        }

        /// <summary>
        /// Sự kiện khi Button sửa công cáo được ấn
        /// </summary>
        private void ButtonChangeNotification_Clicked()
        {
            /// Nếu bản thân không có bang hội
            if (Global.Data.RoleData.GuildID <= 0)
            {
                KTGlobal.AddNotification("Bạn chưa gia nhập bang hội nào, không thể thực hiện sửa công cáo!");
                return;
            }
            /// Nếu không phải bang chủ hoặc phó bang chủ
            else if (Global.Data.RoleData.GuildRank != (int) GuildRank.Master && Global.Data.RoleData.GuildRank != (int) GuildRank.ViceMaster)
            {
                KTGlobal.AddNotification("Chỉ có bang chủ hoặc phó bang chủ mới có quyền sửa công cáo bang hội!");
                return;
            }

            /// Hiện khung
            this.UI_ChangeNotificationFrame.Show();
            /// Sự kiện
            this.UI_ChangeNotificationFrame.ChangeNotification = this.ChangeNotification;
        }

        /// <summary>
        /// Sự kiện khi Button mở khung kỹ năng bang được ấn
        /// </summary>
        public void ButtonOpenGuildSkills_Clicked()
        {
            this.OpenGuildSkills?.Invoke();
        }

        /// <summary>
        /// Sự kiện khi Button mở khung công thành chiến bang được ấn
        /// </summary>
        public void ButtonOpenGuildBattle_Clicked()
        {
            this.OpenGuildBattle?.Invoke();
        }

        /// <summary>
        /// Sự kiện khi Button mở khung liên minh bang được ấn
        /// </summary>
        public void ButtonOpenGuildAllies_Clicked()
        {
            this.OpenGuildAllies?.Invoke();
        }

        /// <summary>
        /// Sự kiện khi Button mở khung thành viên bang được ấn
        /// </summary>
        public void ButtonOpenGuildMemberList_Clicked()
        {
            this.OpenGuildMemberList?.Invoke();
        }

        /// <summary>
        /// Sự kiện khi Button mở khung cống hiến bang được ấn
        /// </summary>
        public void ButtonOpenGuildDedication_Clicked()
        {
            this.OpenGuildDedication?.Invoke();
        }

        /// <summary>
        /// Sự kiện khi Button mở khung nhiệm vụ bang được ấn
        /// </summary>
        public void ButtonOpenGuildQuest_Clicked()
        {
            this.OpenGuildQuest?.Invoke();
        }

        /// <summary>
        /// Sự kiện khi Button mở khung cửa hàng bang hội được ấn
        /// </summary>
        public void ButtonOpenGuildShop_Clicked()
        {
            this.OpenGuildShop?.Invoke();
        }

        /// <summary>
        /// Sự kiện khi Button rời bang được ấn
        /// </summary>
        public void ButtonQuitGuild_Clicked()
        {
            /// Nếu bản thân không có bang hội
            if (Global.Data.RoleData.GuildID <= 0)
            {
                KTGlobal.AddNotification("Bạn chưa gia nhập bang hội nào, không thể thực hiện thao tác!");
                return;
            }
            /// Nếu là bang chủ
            else if (Global.Data.RoleData.GuildRank == (int) GuildRank.Master)
            {
                KTGlobal.AddNotification("Thân là bang chủ, không được phép rời bỏ bang hội của mình!");
                return;
            }

            KTGlobal.ShowMessageBox("Rời bang", "Xác nhận rời khỏi bang hội này?", () =>
            {
                this.QuitGuild?.Invoke();
            }, true);
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
                /// Hiện thông báo lỗi
                KTGlobal.AddNotification("Thông tin bang hội bị lỗi. Hãy thử lại sau!");
                /// Đóng khung
                this.Close?.Invoke();
                return;
            }
            /// Nếu bản thân không có bang hội
            else if (Global.Data.RoleData.GuildID <= 0)
            {
                KTGlobal.AddNotification("Bạn chưa gia nhập bang hội nào, không thể thực hiện thao tác!");
                /// Đóng khung
                this.Close?.Invoke();
                return;
            }

            /// Kinh nghiệm thăng cấp cấp độ hiện tại
            int maxExp = Loader.Loader.GuildConfig.GetLevelRequireExp(this._Data.Data.GuilLevel);

            /// Thông tin bang
            this.UIText_GuildName.text = this._Data.Data.GuildName;
            this.UIText_MasterName.text = this._Data.Data.MasterName;
            this.UIText_GuildLevel.text = this._Data.Data.GuilLevel.ToString();
            this.UIText_GuildExp.text = string.Format("{0}/{1}", this._Data.Data.GuildExp, maxExp);
            this.UISlider_GuildExp.value = this._Data.Data.GuildExp / (float) maxExp;
            this.UIText_MemberCount.text = this._Data.Data.TotalMember.ToString();
            this.UIText_CopySceneCount.text = this._Data.Data.Total_Copy_Scenes_This_Week.ToString();

            /// Công cáo
            this.UIText_Notification.text = this._Data.Data.Notification;

            /// Quỹ bang
            this.UIText_TotalMoney.text = KTGlobal.GetDisplayMoney(this._Data.Data.GuildMoney);
            /// Xóa danh sách vật phẩm
            foreach (Transform child in this.itemListTransform.transform)
            {
                if (child.gameObject != this.UI_ItemInfoPrefab.gameObject)
                {
                    GameObject.Destroy(child.gameObject);
                }
            }


            /// Danh sách vật phẩm đã tích lũy
            Dictionary<int, int> storeItems = new Dictionary<int, int>();
            /// Nếu có dữ liệu
            if (!string.IsNullOrEmpty(this._Data.Data.ItemStore))
            {
                /// Chuỗi mã hóa danh sách vật phẩm đã tích lũy
                string[] storeItemsString = this._Data.Data.ItemStore.Split('|');
                /// Duyệt danh sách vật phẩm đã tích lũy
                foreach (string storeItemString in storeItemsString)
                {
                    /// Tách thành các trường
                    string[] fields = storeItemString.Split('_');
                    /// ID vật phẩm
                    int itemID = int.Parse(fields[0]);
                    /// Số lượng
                    int quantity = int.Parse(fields[1]);
                    /// Thêm vào danh sách
                    storeItems[itemID] = quantity;
                }
            }
            /// Duyệt danh sách vật phẩm cống hiến
            foreach (DedicationItem dedicationItem in Loader.Loader.GuildConfig.Dedication.PointForItems)
            {
                /// ID vật phẩm
                int itemID = dedicationItem.ItemID;
                /// Số lượng
                int quantity = storeItems.ContainsKey(itemID) ? storeItems[itemID] : 0;
                /// Tạo mới ô vật phẩm
                UIGuildEx_Main_ItemInfo uiItemInfo = GameObject.Instantiate<UIGuildEx_Main_ItemInfo>(this.UI_ItemInfoPrefab);
                uiItemInfo.transform.SetParent(this.itemListTransform, false);
                uiItemInfo.gameObject.SetActive(true);
                uiItemInfo.ItemID = itemID;
                uiItemInfo.Quantity = quantity;
            }
            /// Xây lại giao diện
            this.ExecuteSkipFrames(1, () =>
            {
                UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(this.itemListTransform);
            });

            /// Cá nhân
            this.UIText_SelfTotalDedication.text = this._Data.TotalDedications.ToString();
            this.UIText_SelfWeekDedication.text = this._Data.WeekDedications.ToString();

            /// Button thăng cấp
            this.UIButton_LevelUp.gameObject.SetActive(Global.Data.RoleData.GuildRank == (int) GuildRank.Master || Global.Data.RoleData.GuildRank == (int) GuildRank.ViceMaster);
            /// Button sửa công cáo
            this.UIButton_ChangeNotification.gameObject.SetActive(Global.Data.RoleData.GuildRank == (int) GuildRank.Master || Global.Data.RoleData.GuildRank == (int) GuildRank.ViceMaster);
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Cập nhật công cáo
        /// </summary>
        public void UpdateNotification()
        {
            /// Toác
            if (this._Data == null)
            {
                return;
            }
            /// Nếu bản thân không có bang hội
            else if (Global.Data.RoleData.GuildID <= 0)
            {
                return;
            }

            /// Công cáo
            this.UIText_Notification.text = this._Data.Data.Notification;
        }
        #endregion
    }
}
