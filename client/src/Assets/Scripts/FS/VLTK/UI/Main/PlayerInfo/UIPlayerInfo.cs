using UnityEngine;
using TMPro;
using Server.Data;
using System;

namespace FS.VLTK.UI.Main
{
	/// <summary>
	/// Thông tin người chơi
	/// </summary>
	public class UIPlayerInfo : MonoBehaviour
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
            }
        }

        /// <summary>
        /// Sự kiện đóng khung
        /// </summary>
        public Action Close { get; set; }
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
            this.Close?.Invoke();
        }
        #endregion
    }
}
