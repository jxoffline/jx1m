using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using FS.VLTK.Utilities.UnityUI;
using FS.VLTK.Entities.Config;
using Server.Data;
using FS.GameEngine.Logic;

namespace FS.VLTK.UI.Main.TeamFrame
{
    /// <summary>
    /// Thông tin người chơi xin vào nhóm
    /// </summary>
    public class UITeamFrame_AskToJoinPlayerInfo : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Avarta người chơi
        /// </summary>
        [SerializeField]
        private SpriteFromAssetBundle UIImage_PlayerAvarta;

        /// <summary>
        /// Text tên người chơi
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_PlayerName;

        /// <summary>
        /// Text cấp độ người chơi
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_PlayerLevel;

        /// <summary>
        /// Text tên môn phái người chơi
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_PlayerFaction;

        /// <summary>
        /// Button đồng ý cho gia nhập
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_Agree;

        /// <summary>
        /// Button hủy 
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_Refuse;
        #endregion

        #region Properties
        private RoleDataMini _RoleData;
        /// <summary>
        /// Thông tin người chơi
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
                if (value == null)
                {
                    return;
                }

                this.UpdateAvarta(value.AvartaID);
                this.UIText_PlayerName.text = value.RoleName;
                this.UIText_PlayerFaction.text = KTGlobal.GetFactionName(value.FactionID, out Color factionColor);
                this.UIText_PlayerFaction.color = factionColor;
                this.UIText_PlayerLevel.text = value.Level.ToString();
            }
        }

        /// <summary>
        /// Sự kiện đồng ý cho người chơi vào nhóm
        /// </summary>
        public Action Agree { get; set; }

        /// <summary>
        /// Sự kiện từ chối không cho người chơi vào nhóm
        /// </summary>
        public Action Refuse { get; set; }
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
            this.UIButton_Agree.onClick.AddListener(this.ButtonAgree_Clicked);
            this.UIButton_Refuse.onClick.AddListener(this.ButtonRefuse_Clicked);
        }

        /// <summary>
        /// Sự kiện khi Button đồng ý được ấn
        /// </summary>
        private void ButtonAgree_Clicked()
        {
            this.Agree?.Invoke();
        }

        /// <summary>
        /// Sự kiện khi Button cự tuyệt được ấn
        /// </summary>
        private void ButtonRefuse_Clicked()
        {
            this.Refuse?.Invoke();
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Cập nhật Avarta
        /// </summary>
        /// <param name="avartaID"></param>
        private void UpdateAvarta(int avartaID)
        {
            if (Loader.Loader.RoleAvartas.TryGetValue(avartaID, out RoleAvartaXML roleAvarta))
            {
                this.UIImage_PlayerAvarta.BundleDir = roleAvarta.BundleDir;
                this.UIImage_PlayerAvarta.AtlasName = roleAvarta.AtlasName;
                this.UIImage_PlayerAvarta.SpriteName = roleAvarta.SpriteName;
                this.UIImage_PlayerAvarta.Load();
            }
            else
            {
                KTDebug.LogError("Can not find RoleAvarta ID = " + avartaID);
            }
        }
        #endregion

        #region Public methods

        #endregion
    }
}
