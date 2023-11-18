using FS.VLTK.Utilities.UnityUI;
using UnityEngine;

namespace FS.VLTK.UI.Main.AutoFight
{
    /// <summary>
    /// Thiết lập chung
    /// </summary>
    public class UIAutoFight_General: MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Toggle kích hoạt tự PK
        /// </summary>
        [SerializeField]
        private UIToggleSprite UIToggle_EnableAutoPK;

        /// <summary>
        /// Toggle từ chối thách đấu
        /// </summary>
        [SerializeField]
        private UIToggleSprite UIToggle_RefuseChallenge;

        /// <summary>
        /// Toggle từ chối giao dịch
        /// </summary>
        [SerializeField]
        private UIToggleSprite UIToggle_RefuseExchange;

        /// <summary>
        /// Từ chối tổ đội
        /// </summary>
        [SerializeField]
        private UIToggleSprite UIToggle_RefuseTeam;
        #endregion

        #region Properties
        /// <summary>
        /// Kích hoạt tự PK
        /// </summary>
        public bool EnableAutoPK
        {
            get
            {
                return this.UIToggle_EnableAutoPK.Active;
            }
            set
            {
                this.UIToggle_EnableAutoPK.Active = value;
            }
        }

        /// <summary>
        /// Từ chối thách đấu
        /// </summary>
        public bool RefuseChallenge
        {
            get
            {
                return this.UIToggle_RefuseChallenge.Active;
            }
            set
            {
                this.UIToggle_RefuseChallenge.Active = value;
            }
        }

        /// <summary>
        /// Từ chối giao dịch
        /// </summary>
        public bool RefuseExchange
        {
            get
            {
                return this.UIToggle_RefuseExchange.Active;
            }
            set
            {
                this.UIToggle_RefuseExchange.Active = value;
            }
        }

        /// <summary>
        /// Từ chối tổ đội
        /// </summary>
        public bool RefuseTeam
        {
            get
            {
                return this.UIToggle_RefuseTeam.Active;
            }
            set
            {
                this.UIToggle_RefuseTeam.Active = value;
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

        }
        #endregion
    }
}
