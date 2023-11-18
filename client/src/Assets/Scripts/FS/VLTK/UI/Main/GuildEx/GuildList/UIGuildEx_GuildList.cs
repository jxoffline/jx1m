using FS.GameEngine.Logic;
using FS.VLTK.UI.Main.GuildEx.GuildList;
using Server.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FS.VLTK.UI.Main.GuildEx
{
    /// <summary>
    /// Khung danh sách bang hội
    /// </summary>
    public class UIGuildEx_GuildList : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Button đóng khung
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_Close;

        /// <summary>
        /// Prefab thông tin bang
        /// </summary>
        [SerializeField]
        private UIGuildEx_GuildList_GuildInfo UI_GuildInfoPrefab;

        /// <summary>
        /// Button xin vào bang
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_AskToJoin;

        /// <summary>
        /// Button tạo bang
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_CreateGuild;
        #endregion

        #region Properties
        private List<MiniGuildInfo> _Data;
        /// <summary>
        /// Danh sách bang hội
        /// </summary>
        public List<MiniGuildInfo> Data
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
        /// Sự kiện xin vào bang
        /// </summary>
        public Action<int> AskToJoin { get; set; }

        /// <summary>
        /// Sự kiện mở khung tạo bang
        /// </summary>
        public Action OpenCreateGuild { get; set; }
        #endregion

        #region Private fields
        /// <summary>
        /// RectTransform danh sách bang hội
        /// </summary>
        private RectTransform transformGuildList;

        /// <summary>
        /// Bang hội được chọn
        /// </summary>
        private MiniGuildInfo selectedGuild;
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi khi đối tượng được tạo ra
        /// </summary>
        private void Awake()
        {
            this.transformGuildList = this.UI_GuildInfoPrefab.transform.parent.GetComponent<RectTransform>();
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
            this.UIButton_AskToJoin.onClick.AddListener(this.ButtonAskToJoin_Clicked);
            this.UIButton_CreateGuild.onClick.AddListener(this.ButtonOpenCreateGuild_Clicked);
        }

        /// <summary>
        /// Sự kiện khi Button đóng khung được ấn
        /// </summary>
        private void ButtonClose_Clicked()
        {
            this.Close?.Invoke();
        }

        /// <summary>
        /// Sự kiện khi Button xin vào bang được ấn
        /// </summary>
        private void ButtonAskToJoin_Clicked()
        {
            /// Toác
            if (this.selectedGuild == null)
            {
                KTGlobal.AddNotification("Hãy chọn một bang hội sau đó tiến hành xin gia nhập!");
                return;
            }
            /// Nếu bản thân đã có bang hội
            else if (Global.Data.RoleData.GuildID > 0)
            {
                KTGlobal.AddNotification("Bạn đã có bang hội, không thể xin gia nhập thêm!");
                return;
            }
            /// Thực thi sự kiện
            this.AskToJoin?.Invoke(this.selectedGuild.GuildId);
        }

        /// <summary>
        /// Sự kiện khi Button mở khung tạo bang được ấn
        /// </summary>
        private void ButtonOpenCreateGuild_Clicked()
        {
            /// Nếu bản thân đã có bang hội
            if (Global.Data.RoleData.GuildID > 0)
            {
                KTGlobal.AddNotification("Bạn đã có bang hội, không thể xin gia nhập thêm!");
                return;
            }
            /// Thực thi sự kiện
            this.OpenCreateGuild?.Invoke();
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
                return;
            }

            /// Hủy bang hội được chọn
            this.selectedGuild = null;

            /// Đánh dấu bang hội đầu tiên trong danh sách
            UIGuildEx_GuildList_GuildInfo uiFirstGuild = null;

            /// Làm rỗng danh sách bang hội
            foreach (Transform child in this.transformGuildList.transform)
            {
                if (child.gameObject != this.UI_GuildInfoPrefab.gameObject)
                {
                    GameObject.Destroy(child.gameObject);
                }
            }
            /// Duyệt danh sách bang hội
            foreach (MiniGuildInfo guildInfo in this._Data)
            {
                /// Tạo mới khung
                UIGuildEx_GuildList_GuildInfo uiGuildInfo = GameObject.Instantiate<UIGuildEx_GuildList_GuildInfo>(this.UI_GuildInfoPrefab);
                uiGuildInfo.transform.SetParent(this.transformGuildList, false);
                uiGuildInfo.gameObject.SetActive(true);
                uiGuildInfo.Data = guildInfo;
                uiGuildInfo.Select = () =>
                {
                    this.selectedGuild = uiGuildInfo.Data;
                };

                /// Nếu chưa chọn bang hội đầu tiên
                if (uiFirstGuild == null)
                {
                    /// Đánh dấu lại
                    uiFirstGuild = uiGuildInfo;
                    /// Thực hiện chọn thằng đầu tiên
                    uiFirstGuild.Active = true;
                }
            }
            /// Xây lại giao diện
            this.ExecuteSkipFrames(1, () =>
            {
                UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(this.transformGuildList);
            });

            /// Nếu đã có bang hội thì ẩn Button xin gia nhập và tạo bang
            this.UIButton_AskToJoin.gameObject.SetActive(Global.Data.RoleData.GuildID <= 0);
            this.UIButton_CreateGuild.gameObject.SetActive(Global.Data.RoleData.GuildID <= 0);
        }
        #endregion
    }
}
