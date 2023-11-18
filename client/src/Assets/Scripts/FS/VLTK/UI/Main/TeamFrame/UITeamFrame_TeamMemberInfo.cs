using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using FS.VLTK.Utilities.UnityUI;
using Server.Data;
using FS.VLTK.Control.Component;
using FS.VLTK.Factory;
using FS.GameEngine.Logic;

namespace FS.VLTK.UI.Main.TeamFrame
{
    /// <summary>
    /// Thông tin đội viên
    /// </summary>
    public class UITeamFrame_TeamMemberInfo : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Toggle
        /// </summary>
        [SerializeField]
        private UIToggleSprite UIToggle;

        /// <summary>
        /// Icon đội trưởng
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Image UIImage_TeamLeaderIcon;

        /// <summary>
        /// Text tên phái
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_Faction;

        /// <summary>
        /// Text vị trí hiện tại
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_CurrentLocation;

        /// <summary>
        /// Icon đánh dấu vị trí trống
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Image UIImage_EmptySlotIcon;

        /// <summary>
        /// Image chiếu ảnh đội viên
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.RawImage UIImage_RolePreview;

        /// <summary>
        /// Text tên đội viên
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_RoleName;

        /// <summary>
        /// Text cấp độ đội viên
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_RoleLevel;

        /// <summary>
        /// RectTransform chứa tên đội viên
        /// </summary>
        [SerializeField]
        private RectTransform UITransform_RoleNameBox;

        /// <summary>
        /// Rect Transform chứa cấp độ đội viên
        /// </summary>
        [SerializeField]
        private RectTransform UITransform_RoleLevelBox;
        #endregion

        #region Properties
        private RoleDataMini _RoleData = null;
        /// <summary>
        /// Thông tin đội viên
        /// </summary>
        public RoleDataMini RoleData
        {
            get
            {
                return this._RoleData;
            }
            set
            {
                this._RoleData = value;
                this.DoRefreshData();
            }
        }

        /// <summary>
        /// Kích hoạt đối tượng để có thể tương tác với người dùng
        /// </summary>
        public bool Enable
        {
            get
            {
                return this.UIToggle.Enable;
            }
            set
            {
                this.UIToggle.Enable = value;
            }
        }

        /// <summary>
        /// Chọn đội viên
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

        /// <summary>
        /// Sự kiện khi đội viên được nhấn chọn
        /// </summary>
        public Action Selected { get; set; }
        #endregion

        #region Private fields
        /// <summary>
        /// Đội viên đang được chiếu lên RawImage
        /// </summary>
        private CharacterPreview previewRole = null;
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi ở Frame đầu tiên
        /// </summary>
        private void Start()
        {
            this.InitPrefabs();
        }

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
                this.DestroyPreview();
            }
        }
        #endregion

        #region Code UI
        /// <summary>
        /// Khởi tạo ban đầu
        /// </summary>
        private void InitPrefabs()
        {
            this.UIToggle.OnSelected = this.UIToggle_Selected;
        }

        /// <summary>
        /// Sự kiện khi Toggle được chọn
        /// </summary>
        /// <param name="isSelected"></param>
        private void UIToggle_Selected(bool isSelected)
        {
            if (isSelected)
            {
                this.Selected?.Invoke();
            }
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Làm mới dữ liệu
        /// </summary>
        private void DoRefreshData()
        {
            /// Nếu không có đội viên tại vị trí này
            if (this._RoleData == null)
            {
                this.UIImage_TeamLeaderIcon.gameObject.SetActive(false);
                this.UIImage_EmptySlotIcon.gameObject.SetActive(true);
                this.UIImage_RolePreview.gameObject.SetActive(false);

                this.UITransform_RoleLevelBox.gameObject.SetActive(false);
                this.UITransform_RoleNameBox.gameObject.SetActive(false);
                this.UIText_Faction.gameObject.SetActive(false);
                this.UIText_CurrentLocation.gameObject.SetActive(false);

                this.DestroyPreview();
            }
            else
            {
                this.UIImage_TeamLeaderIcon.gameObject.SetActive(this._RoleData.RoleID == this._RoleData.TeamLeaderID);
                this.UIImage_EmptySlotIcon.gameObject.SetActive(false);
                this.UIImage_RolePreview.gameObject.SetActive(true);

                this.UITransform_RoleLevelBox.gameObject.SetActive(true);
                this.UITransform_RoleNameBox.gameObject.SetActive(true);
                this.UIText_Faction.gameObject.SetActive(true);
                this.UIText_CurrentLocation.gameObject.SetActive(true);

                this.UIText_RoleName.text = this._RoleData.RoleName;
                this.UIText_RoleLevel.text = string.Format("Cấp: {0}", this._RoleData.Level);
                this.UIText_Faction.text = KTGlobal.GetFactionName(this._RoleData.FactionID, out Color factionColor);
                this.UIText_Faction.color = factionColor;
                if (Loader.Loader.Maps.TryGetValue(this._RoleData.MapCode, out Entities.Config.Map map))
                {
                    this.UIText_CurrentLocation.text = map.Name;
                }

                /// Nếu đang hiển thị
                if (this.previewRole == null)
                {
                    this.previewRole = Object2DFactory.MakeRolePreview();
                    this.previewRole.Data = this._RoleData;
                    this.previewRole.UpdateRoleData();
                    this.previewRole.Direction = Entities.Enum.Direction.DOWN;
                    this.previewRole.OnStart = () => {
                        this.UIImage_RolePreview.texture = this.previewRole.ReferenceCamera.targetTexture;
                    };
                    this.previewRole.ResumeCurrentAction();
                }
            }
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Làm mới dữ liệu
        /// </summary>
        public void RefreshData()
        {
            this.DoRefreshData();
        }

        /// <summary>
        /// Xóa đối tượng soi trước
        /// </summary>
        public void DestroyPreview()
        {
            if (this.previewRole != null)
            {
                GameObject.Destroy(this.previewRole.gameObject);
            }
        }

        /// <summary>
        /// Làm mới hiển thị Icon đội trưởng
        /// </summary>
        public void RefreshTeamLeader()
        {
            this.UIImage_TeamLeaderIcon.gameObject.SetActive(this.RoleData.RoleID == this.RoleData.TeamLeaderID);
        }
        #endregion
    }
}
