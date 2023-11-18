using FS.VLTK.Control.Component;
using FS.VLTK.Factory;
using Server.Data;
using TMPro;
using UnityEngine;

namespace FS.VLTK.UI.Main.Challenge
{
    /// <summary>
    /// Thông tin người chơi trong khung thách đấu
    /// </summary>
    public class UIChallenge_PlayerInfo : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Mark trưởng nhóm
        /// </summary>
        [SerializeField]
        private RectTransform UIMark_TeamLeader;

        /// <summary>
        /// Text tên người chơi
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_PlayerName;

        /// <summary>
        /// Text cấp độ người chơi
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_Level;

        /// <summary>
        /// Text môn phái người chơi
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_FactionName;

        /// <summary>
        /// Image xem trước nhân vật
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.RawImage UIImage_RolePreview;
        #endregion

        #region Properties
        private RoleChallenge_PlayerData _Data;
        /// <summary>
        /// Dữ liệu người chơi
        /// </summary>
        public RoleChallenge_PlayerData Data
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
        #endregion

        #region Private fields
        /// <summary>
        /// Đội viên đang được chiếu lên RawImage
        /// </summary>
        private CharacterPreview previewRole = null;
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Sự kiện khi đối tượng được kích hoạt
        /// </summary>
        private void OnEnable()
        {
            if (this.previewRole != null)
            {
                this.previewRole.ResumeCurrentAction();
            }
        }

        /// <summary>
        /// Hàm này gọi khi đối tượng bị hủy
        /// </summary>
        private void OnDestroy()
        {
            if (this.previewRole != null)
            {
                GameObject.Destroy(this.previewRole.gameObject);
            }
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Làm mới dữ liệu
        /// </summary>
        private void RefreshData()
        {
            /// Nếu không có dữ liệu
            if (this._Data == null)
            {
                return;
            }

            this.UIMark_TeamLeader.gameObject.SetActive(this._Data.IsTeamLeader);
            this.UIImage_RolePreview.gameObject.SetActive(true);

            this.UIText_FactionName.text = KTGlobal.GetFactionName(this._Data.FactionID, out Color color);
            this.UIText_FactionName.color = color;
            this.UIText_PlayerName.text = this._Data.RoleName;
            this.UIText_Level.text = this._Data.Level.ToString();

            /// Nếu đang hiển thị
            if (this.previewRole == null)
            {
                this.previewRole = Object2DFactory.MakeRolePreview();
                this.previewRole.Data = new RoleDataMini()
                {
                    HelmID = this._Data.HelmID,
                    ArmorID = this._Data.ArmorID,
                    WeaponID = this._Data.WeaponID,
                    WeaponSeries = this._Data.WeaponSeries,
                    WeaponEnhanceLevel = this._Data.WeaponEnhanceLevel,
                    RoleSex = this._Data.RoleSex,
                };
                this.previewRole.UpdateRoleData();
                this.previewRole.Direction = Entities.Enum.Direction.DOWN;
                this.previewRole.OnStart = () => {
                    this.UIImage_RolePreview.texture = this.previewRole.ReferenceCamera.targetTexture;
                };
                this.previewRole.ResumeCurrentAction();
            }
        }
        #endregion
    }
}
