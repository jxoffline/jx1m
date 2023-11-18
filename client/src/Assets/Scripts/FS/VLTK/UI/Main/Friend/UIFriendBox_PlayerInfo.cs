using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using Server.Data;
using FS.GameEngine.Logic;
using UnityEngine.EventSystems;

namespace FS.VLTK.UI.Main.Friend
{
    /// <summary>
    /// Thông tin người chơi trong khung bạn bè
    /// </summary>
    public class UIFriendBox_PlayerInfo : MonoBehaviour, IPointerClickHandler
    {
        #region Define
        /// <summary>
        /// Text tên người chơi
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_PlayerName;

        /// <summary>
        /// Avarta người chơi
        /// </summary>
        [SerializeField]
        private UIRoleAvarta UIImage_PlayerAvarta;

        /// <summary>
        /// Text môn phái người chơi
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_PlayerFactionName;

        /// <summary>
        /// Text cấp độ người chơi
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_PlayerLevel;

        /// <summary>
        /// Text trạng thái Online của người chơi
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_PlayerOnlineStatus;
        #endregion

        #region Private fields

        #endregion

        #region Properties
        /// <summary>
        /// Sự kiện Click chọn người chơi
        /// </summary>
        public Action<Vector2> Click { get; set; }

        private FriendData _Data;
        /// <summary>
        /// Thông tin bạn bè
        /// </summary>
        public FriendData Data
        {
            get
            {
                return this._Data;
            }
            set
            {
                this._Data = value;

                /// Nếu tồn tại thông tin bạn bè
                if (value != null)
                {
                    this.UIImage_PlayerAvarta.RoleID = value.OtherRoleID;
                    this.UIImage_PlayerAvarta.AvartaID = value.PicCode;
                    this.UIText_PlayerName.text = value.OtherRoleName;
                    this.UIText_PlayerFactionName.text = KTGlobal.GetFactionName(value.FactionID, out Color color);
                    this.UIText_PlayerFactionName.color = color;
                    this.UIText_PlayerLevel.text = value.OtherLevel.ToString();
                    this.UIText_PlayerOnlineStatus.text = value.OnlineState == 1 ? "<color=green>[ONL]</color>" : "<color=red>[OFF]</color>";
                }
                else
                {
                    this.UIImage_PlayerAvarta.RoleID = -1;
                    this.UIImage_PlayerAvarta.AvartaID = -1;
                    this.UIText_PlayerName.text = "";
                    this.UIText_PlayerFactionName.text = "";
                    this.UIText_PlayerFactionName.color = Color.white;
                    this.UIText_PlayerLevel.text = "";
                    this.UIText_PlayerOnlineStatus.text = "";
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
            
        }

        /// <summary>
        /// Sự kiện khi đối tượng được click chọn
        /// </summary>
        /// <param name="eventData"></param>
        public void OnPointerClick(PointerEventData eventData)
        {
            this.Click?.Invoke(eventData.position);
        }
        #endregion

        #region Private methods

        #endregion

        #region Public methods

        #endregion
    }
}
