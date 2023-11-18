using FS.GameEngine.Logic;
using FS.VLTK.UI.Main.ItemBox;
using Server.Data;
using System;
using System.Collections;
using TMPro;
using UnityEngine;

namespace FS.VLTK.UI.Main.GuildEx
{
    /// <summary>
    /// Khung tạo bang hội
    /// </summary>
    public class UIGuildEx_CreateNew : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Button đóng khung
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_Close;

        /// <summary>
        /// Input tên bang
        /// </summary>
        [SerializeField]
        private TMP_InputField UIInput_GuildName;

        /// <summary>
        /// Text số bạc yêu cầu
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_RequireMoney;

        /// <summary>
        /// Ô vật phẩm yêu cầu
        /// </summary>
        [SerializeField]
        private UIItemBox UIItem_RequireItem;

        /// <summary>
        /// Text điều kiện thêm
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_Addition;

        /// <summary>
        /// Button xác nhận
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_Accept;
        #endregion

        #region Properties
        /// <summary>
        /// Sự kiện đóng khung
        /// </summary>
        public Action Close { get; set; }

        /// <summary>
        /// Sự kiện tạo bang
        /// </summary>
        public Action<string> CreateGuild { get; set; }
        #endregion

        #region Private fields

        #endregion

        #region Core MonoBehaviour
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
            this.UIButton_Accept.onClick.AddListener(this.ButtonCreateGuild_Clicked);

            /// Nếu vật phẩm yêu cầu tồn tại
            if (Loader.Loader.Items.TryGetValue(Loader.Loader.GuildConfig.RequireItem, out Entities.Config.ItemData itemData))
            {
                GoodsData itemGD = KTGlobal.CreateItemPreview(itemData);
                this.UIItem_RequireItem.Data = itemGD;
                this.UIItem_RequireItem.Refresh();
            }

            /// Yêu cầu bạc
            this.UIText_RequireMoney.text = KTGlobal.GetDisplayMoney(Loader.Loader.GuildConfig.RequireMoney);

            /// Điều kiện thêm
            this.UIText_Addition.text = Loader.Loader.GuildConfig.RequireAdditionString;
        }

        /// <summary>
        /// Sự kiện khi Button đóng khung được ấn
        /// </summary>
        private void ButtonClose_Clicked()
        {
            this.Close?.Invoke();
        }

        /// <summary>
        /// Sự kiện khi Button tạo bang được ấn
        /// </summary>
        private void ButtonCreateGuild_Clicked()
        {
            /// Nếu bản thân đã có bang
            if (Global.Data.RoleData.GuildID > 0)
            {
                /// Thông báo
                KTGlobal.AddNotification("Bạn đã có bang hội, không thể tạo bang!");
                return;
            }
            ///// Số tiền mang theo không đủ
            //else if (Global.Data.RoleData.Money < Loader.Loader.GuildConfig.RequireMoney)
            //{
            //    /// Thông báo
            //    KTGlobal.AddNotification(string.Format("Số bạc mang theo không đủ {0}, không thể tạo bang!", KTGlobal.GetDisplayMoney(Loader.Loader.GuildConfig.RequireMoney)));
            //    return;
            //}
            ///// Vật phẩm không đủ
            //else if (KTGlobal.GetItemCountInBag(Loader.Loader.GuildConfig.RequireItem) <= 0)
            //{
            //    /// Thông báo
            //    KTGlobal.AddNotification(string.Format("Không có [{0}], không thể tạo bang!", KTGlobal.GetItemName(Loader.Loader.GuildConfig.RequireItem)));
            //    return;
            //}

            /// Tên bang
            string guildName = this.UIInput_GuildName.text.Trim();
            /// Toác
            if (string.IsNullOrEmpty(guildName))
            {
                /// Thông báo
                KTGlobal.AddNotification("Hãy nhập tên bang hội.");
                return;
            }
            else if (!KTFormValidation.IsValidString(guildName, false, true, false, false))
            {
                /// Thông báo
                KTGlobal.AddNotification("Tên bang hội có chứa ký tự phi pháp.");
                return;
            }
            else if (guildName.Length < 6 || guildName.Length > 18)
            {
                /// Thông báo
                KTGlobal.AddNotification("Tên bang hội chỉ được phép có từ 6 đến 18 ký tự.");
                return;
            }

            /// Thực thi sự kiện
            this.CreateGuild?.Invoke(guildName);
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
        #endregion
    }
}
