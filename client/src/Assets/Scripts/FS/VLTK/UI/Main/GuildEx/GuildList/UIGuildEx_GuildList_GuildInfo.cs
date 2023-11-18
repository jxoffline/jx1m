using UnityEngine;
using TMPro;
using Server.Data;
using FS.VLTK.Utilities.UnityUI;
using System;

namespace FS.VLTK.UI.Main.GuildEx.GuildList
{
    /// <summary>
    /// Thông tin bang trong khung danh sách bang hội
    /// </summary>
    public class UIGuildEx_GuildList_GuildInfo : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Toggle
        /// </summary>
        [SerializeField]
        private UIToggleSprite UIToggle;

        /// <summary>
        /// Text tên bang
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_GuildName;

        /// <summary>
        /// Text cấp độ bang
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_GuildLevel;

        /// <summary>
        /// Text tên bang chủ
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_MasterName;

        /// <summary>
        /// Text tổng số thành viên
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_MemberCount;

        /// <summary>
        /// Text bang cống
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_TotalMoney;
        #endregion

        #region Properties
        private MiniGuildInfo _Data;
        /// <summary>
        /// Dữ liệu bang hội
        /// </summary>
        public MiniGuildInfo Data
        {
            get
            {
                return this._Data;
            }
            set
            {
                this._Data = value;
                /// Thiết lập giá trị
                this.UIText_GuildName.text = value.GuildName;
                this.UIText_GuildLevel.text = value.GuilLevel.ToString();
                this.UIText_MasterName.text = value.MasterName;
                this.UIText_MemberCount.text = value.TotalMember.ToString();
                this.UIText_TotalMoney.text = KTGlobal.GetDisplayMoney(value.GuildMoney);
            }
        }

        /// <summary>
        /// Sự kiện khi bang được chọn
        /// </summary>
        public Action Select { get; set; }

        /// <summary>
        /// Chọn bang hội này
        /// </summary>
        public bool Active
        {
            get
            {
                return this.UIToggle.Active;
            }
            set
            {
                this.UIToggle.Active = value;
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
            this.UIToggle.OnSelected = this.Toggle_Selected;
        }

        /// <summary>
        /// Sự kiện khi Toggle được chọn
        /// </summary>
        /// <param name="isSelected"></param>
        private void Toggle_Selected(bool isSelected)
        {
            if (isSelected)
            {
                this.Select?.Invoke();
            }
        }
        #endregion
    }
}
