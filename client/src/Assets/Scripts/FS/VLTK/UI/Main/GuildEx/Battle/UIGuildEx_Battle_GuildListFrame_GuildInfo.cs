using TMPro;
using UnityEngine;

namespace FS.VLTK.UI.Main.GuildEx.Battle
{
    /// <summary>
    /// Thông tin bang hội trong khung danh sách bang hội đăng ký tham gia công thành chiến
    /// </summary>
    public class UIGuildEx_Battle_GuildListFrame_GuildInfo : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Text tên bang hội
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_Name;
        #endregion

        #region Properties
        private string _Data;
        /// <summary>
        /// Thông tin bang
        /// </summary>
        public string Data
        {
            get
            {
                return this._Data;
            }
            set
            {
                this._Data = value;
                /// Dữ liệu
                this.UIText_Name.text = value;
            }
        }
        #endregion
    }
}
