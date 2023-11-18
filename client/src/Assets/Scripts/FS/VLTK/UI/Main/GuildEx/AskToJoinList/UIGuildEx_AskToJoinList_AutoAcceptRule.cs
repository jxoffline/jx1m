using FS.GameEngine.Logic;
using FS.VLTK.Entities.Config;
using FS.VLTK.Utilities.UnityUI;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using static FS.VLTK.Entities.Enum;

namespace FS.VLTK.UI.Main.GuildEx.AskToJoinList
{
    /// <summary>
    /// Khung thiết lập điều kiện tự đồng ý cho người chơi vào bang hội
    /// </summary>
    public class UIGuildEx_AskToJoinList_AutoAcceptRule : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Button đóng khung
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_Close;

        /// <summary>
        /// Input cấp độ
        /// </summary>
        [SerializeField]
        private TMP_InputField UIInput_Level;

        /// <summary>
        /// Dropdown môn phái
        /// </summary>
        [SerializeField]
        private TMP_Dropdown UIDropdown_FactionList;

        /// <summary>
        /// Input tài phú
        /// </summary>
        [SerializeField]
        private TMP_InputField UIInput_TotalValue;

        /// <summary>
        /// Toggle tự duyệt
        /// </summary>
        [SerializeField]
        private UIToggleSprite UIToggle_AutoAccept;

        /// <summary>
        /// Button lưu lại
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_Save;
        #endregion

        #region Properties
        /// <summary>
        /// Cấp độ
        /// </summary>
        public int Level { get; set; }

        /// <summary>
        /// ID phái
        /// </summary>
        public int FactionID { get; set; }

        /// <summary>
        /// Tài phú
        /// </summary>
        public long TotalValue { get; set; }

        /// <summary>
        /// Tự động duyệt người chơi vào bang
        /// </summary>
        public bool AutoAccept { get; set; }

        /// <summary>
        /// Sự kiện lưu thiết lập
        /// </summary>
        public Action Save { get; set; }
        #endregion

        #region Private fields
        /// <summary>
        /// Đã chạy qua hàm Start chưa
        /// </summary>
        private bool isStarted = false;
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi ở Frame đầu tiên
        /// </summary>
        private void Start()
        {
            this.InitPrefabs();
            /// Đánh dấu đã chạy qua hàm Start
            this.isStarted = true;
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
            /// Làm mới dữ liệu
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
            this.UIButton_Save.onClick.AddListener(this.ButtonSave_Clicked);

            this.UIDropdown_FactionList.options.Clear();
            /// Thêm option toàn bộ
            this.UIDropdown_FactionList.options.Add(new TMP_Dropdown.OptionData()
            {
                text = "Toàn bộ",
            });
            /// Danh sách phái
            Dictionary<int, string> factionsList = new Dictionary<int, string>();
            /// Duyệt danh sách môn phái
            foreach (FactionXML factionData in Loader.Loader.Factions.Values)
            {
                /// Nếu là tân thủ thì bỏ qua
                if (factionData.ID == 0)
                {
                    continue;
                }
                string factionName = KTGlobal.GetFactionName(factionData.ID, out Color color);
                string htmlColor = ColorUtility.ToHtmlStringRGB(color);
                factionsList[factionData.ID] = string.Format("<color=#{0}>{1}</color>", htmlColor, factionName);
            }
            /// Thêm toàn bộ các phái vào
            this.UIDropdown_FactionList.options.AddRange(factionsList.OrderBy(x => x.Key).Select(option =>
            {
                return new TMP_Dropdown.OptionData()
                {
                    text = option.Value,
                };
            }).ToList());

            /// Làm mới dữ liệu
            this.RefreshData();
        }

        /// <summary>
        /// Sự kiện khi Button đóng khung được ấn
        /// </summary>
        private void ButtonClose_Clicked()
        {
            this.Hide();
        }

        /// <summary>
        /// Sự kiện khi Button tự động duyệt người chơi vào bang được ấn
        /// </summary>
        private void ButtonSave_Clicked()
        {
            /// Nếu không có bang hội
            if (Global.Data.RoleData.GuildID <= 0)
            {
                /// Bỏ qua
                KTGlobal.AddNotification("Bạn không có bang hội, không thể thực hiện thao tác này!");
                return;
            }
            /// Nếu không phải bang chủ hoặc phó bang chủ
            else if (Global.Data.RoleData.GuildRank != (int) GuildRank.Master && Global.Data.RoleData.GuildRank != (int) GuildRank.ViceMaster)
            {
                /// Bỏ qua
                KTGlobal.AddNotification("Chỉ có bang chủ hoặc phó bang chủ mới có thể thực hiện thao tác này!");
                return;
            }

            /// Đổ dữ liệu
            this.Level = int.Parse(this.UIInput_Level.text);
            this.FactionID = this.UIDropdown_FactionList.value;
            this.TotalValue = long.Parse(this.UIInput_TotalValue.text);
            this.AutoAccept = this.UIToggle_AutoAccept.Active;

            /// Thực thi sự kiện
            this.Save?.Invoke();
            /// Đóng khung
            this.Hide();
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Làm mới dữ liệu
        /// </summary>
        private void RefreshData()
        {
            this.UIInput_Level.text = this.Level.ToString();
            this.UIDropdown_FactionList.value = this.FactionID;
            this.UIInput_TotalValue.text = this.TotalValue.ToString();
            this.UIToggle_AutoAccept.Active = this.AutoAccept;
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Hiện khung
        /// </summary>
        public void Show()
        {
            this.gameObject.SetActive(true);
        }

        /// <summary>
        /// Ẩn khung
        /// </summary>
        public void Hide()
        {
            this.gameObject.SetActive(false);
        }
        #endregion
    }
}
