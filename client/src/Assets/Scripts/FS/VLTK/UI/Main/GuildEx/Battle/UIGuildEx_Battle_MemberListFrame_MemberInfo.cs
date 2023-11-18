using Server.Data;
using System;
using TMPro;
using UnityEngine;

namespace FS.VLTK.UI.Main.GuildEx.Battle
{
    /// <summary>
    /// Thông tin thành viên trong khung chọn thành viên tham gia Công Thành Chiến
    /// </summary>
    public class UIGuildEx_Battle_MemberListFrame_MemberInfo : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Button chọn thành viên
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_Select;

        /// <summary>
        /// Text tên thành viên
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_Name;

        /// <summary>
        /// Text cấp độ
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_Level;

        /// <summary>
        /// Text phái
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_Faction;
        #endregion

        #region Properties
        private MemberRegister _Data;
        /// <summary>
        /// Thông tin thành viên
        /// </summary>
        public MemberRegister Data
        {
            get
            {
                return this._Data;
            }
            set
            {
                this._Data = value;

                /// Cập nhật dữ liệu
                this.UIText_Name.text = value.RoleName;
                this.UIText_Level.text = value.Level.ToString();
                this.UIText_Faction.text = KTGlobal.GetFactionName(value.Faction, out Color color);
                this.UIText_Faction.color = color;
            }
        }

        /// <summary>
        /// Sự kiện chọn thành viên
        /// </summary>
        public Action Select { get; set; }
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
            this.UIButton_Select.onClick.AddListener(this.ButtonSelect_Clicked);
        }

        /// <summary>
        /// Sự kiện khi Button thêm thành viên được ấn
        /// </summary>
        private void ButtonSelect_Clicked()
        {
            this.Select?.Invoke();
        }
        #endregion
    }
}
