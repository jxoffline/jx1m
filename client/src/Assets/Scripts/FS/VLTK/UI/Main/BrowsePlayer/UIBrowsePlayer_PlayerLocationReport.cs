using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using Server.Data;
using FS.GameEngine.Logic;
using FS.VLTK.Utilities.UnityUI;

namespace FS.VLTK.UI.Main.BrowsePlayer
{
    /// <summary>
    /// Thông tin vị trí người chơi
    /// </summary>
    public class UIBrowsePlayer_PlayerLocationReport : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Button đóng khung
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_Close;

        /// <summary>
        /// Avarta người chơi
        /// </summary>
        [SerializeField]
        private UIRoleAvarta UIImage_Avarta;

        /// <summary>
        /// Text tên người chơi
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_Name;

        /// <summary>
        /// Text cấp độ người chơi
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_Level;

        /// <summary>
        /// Text tên môn phái
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_FactionName;

        /// <summary>
        /// Text tên bang hội
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_GuildName;

        /// <summary>
        /// Text thông tin vị trí
        /// </summary>
        [SerializeField]
        private UIRichText UIText_Location;
        #endregion

        #region Properties
        private RoleDataMini _Data;
        /// <summary>
        /// Dữ liệu người chơi
        /// </summary>
        public RoleDataMini Data
        {
            get
            {
                return this._Data;
            }
            set
            {
                this._Data = value;

                this.UIImage_Avarta.RoleID = value.RoleID;
                this.UIImage_Avarta.AvartaID = value.AvartaID;
                this.UIText_Name.text = value.RoleName;
                this.UIText_Level.text = value.Level.ToString();
                this.UIText_GuildName.text = value.GuildName;
                this.UIText_FactionName.text = KTGlobal.GetFactionName(value.FactionID, out Color factionColor);
                this.UIText_FactionName.color = factionColor;
                /// Nếu bản đồ không rõ
                if (!Loader.Loader.Maps.TryGetValue(value.MapCode, out Entities.Config.Map mapData))
                {
                    this.UIText_Location.Text = "Phụ bản";
                }
                else
                {
                    this.UIText_Location.Text = string.Format("<link=\"GoToTarget\"><color=green>{0}</color> <color=#05c9ff>({1}, {2})</color></link>", mapData.Name, value.PosX, value.PosY);
                    this.UIText_Location.ClickEvents = new Dictionary<string, Action<string>>()
                    {
                        { "GoToTarget", (funcName) => {
                            Vector2 worldPos = KTGlobal.GridPositionToWorldPosition(new Vector2(value.PosX, value.PosY));
                            KTGlobal.QuestAutoFindPath(value.MapCode, (int) worldPos.x, (int) worldPos.y, null);
                        } },
                    };
                }
            }
        }
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
        }

        /// <summary>
        /// Sự kiện khi Button đóng khung được ấn
        /// </summary>
        private void ButtonClose_Clicked()
        {
            this.Hide();
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
        /// Đóng khung
        /// </summary>
        public void Hide()
        {
            this.gameObject.SetActive(false);
        }
        #endregion
    }
}
