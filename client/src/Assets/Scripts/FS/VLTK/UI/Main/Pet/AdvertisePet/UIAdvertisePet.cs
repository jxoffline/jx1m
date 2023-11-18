using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Server.Data;
using static FS.VLTK.Entities.Enum;

namespace FS.VLTK.UI.Main
{
    /// <summary>
    /// Khung quảng bá tinh linh
    /// </summary>
    public class UIAdvertisePet : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Button đóng khung
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_Close;

        /// <summary>
        /// Input nội dung chat
        /// </summary>
        [SerializeField]
        private TMP_InputField UIInput_Content;

        /// <summary>
        /// Dropdown chọn kênh Chat
        /// </summary>
        [SerializeField]
        private TMP_Dropdown UIDropdown_ChatChannel;

        /// <summary>
        /// Button gửi đi
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_Send;
        #endregion

        #region Private fields
        /// <summary>
        /// Danh sách kênh
        /// </summary>
        private readonly Dictionary<ChatChannel, string> channels = new Dictionary<ChatChannel, string>();
        #endregion

        #region Properties
        /// <summary>
        /// Sự kiện đóng khung
        /// </summary>
        public Action Close { get; set; }

        /// <summary>
        /// Sự kiện gửi tin
        /// </summary>
        public Action<ChatChannel, string, string> Send { get; set; }

        /// <summary>
        /// Thông tin pet được quảng bá
        /// </summary>
        public PetData ItemGD { get; set; }
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
            this.UIButton_Send.onClick.AddListener(this.ButtonSend_Clicked);
            this.channels.Clear();
            this.channels[ChatChannel.Near] = "Lân cận";
            this.channels[ChatChannel.Faction] = "Môn phái";
            this.channels[ChatChannel.Guild] = "Bang hội";
            this.channels[ChatChannel.Global] = "Thế giới";
            if (!string.IsNullOrEmpty(KTGlobal.LastPrivateToRoleName))
            {
                this.channels[ChatChannel.Private] = string.Format("Mật - <color=#6bc4ff>[{0}]</color>", KTGlobal.LastPrivateToRoleName);
            }
            /// Duyệt danh sách kênh Chat, cho thêm vào
            foreach (string channelName in this.channels.Values)
            {
                this.UIDropdown_ChatChannel.options.Add(new TMP_Dropdown.OptionData()
                {
                    text = channelName,
                });
            }
            /// Chọn mặc định kênh Chat là kênh lân cận
            this.UIDropdown_ChatChannel.captionText.text = this.channels[ChatChannel.Near];
        }

        /// <summary>
        /// Sự kiện khi Button đóng khung được ấn
        /// </summary>
        private void ButtonClose_Clicked()
        {
            this.Close?.Invoke();
        }

        /// <summary>
        /// Sự kiện khi Button gửi đi được ấn
        /// </summary>
        private void ButtonSend_Clicked()
        {
            string content = this.UIInput_Content.text;
            /// Nếu không có nội dung quảng bá
            if (string.IsNullOrEmpty(content))
            {
                KTGlobal.AddNotification("Hãy nhập vào nội dung quảng bá!");
                return;
            }

            /// Xóa toàn bộ thẻ HTML
            content = Utils.RemoveAllHTMLTags(content);

            /// Nếu chọn kênh Chat mật và có thông tin đối tượng chat mật
            if (this.UIDropdown_ChatChannel.value == 4 && !string.IsNullOrEmpty(KTGlobal.LastPrivateToRoleName))
            {
                KTGlobal.AddNotification("Gửi tin nhắn quảng bá tinh linh thành công!");
                this.Send?.Invoke(ChatChannel.Private, KTGlobal.LastPrivateToRoleName, content);
            }
            else
            {
                switch (this.UIDropdown_ChatChannel.value)
                {
                    case 0:
                    {
                        this.Send?.Invoke(ChatChannel.Near, "", content);
                        KTGlobal.AddNotification("Gửi tin nhắn quảng bá tinh linh thành công!");
                        break;
                    }
                    case 1:
                    {
                        this.Send?.Invoke(ChatChannel.Faction, "", content);
                        KTGlobal.AddNotification("Gửi tin nhắn quảng bá tinh linh thành công!");
                        break;
                    }
                    case 2:
                    {
                        this.Send?.Invoke(ChatChannel.Guild, "", content);
                        KTGlobal.AddNotification("Gửi tin nhắn quảng bá tinh linh thành công!");
                        break;
                    }
                    case 3:
                    {
                        this.Send?.Invoke(ChatChannel.Global, "", content);
                        KTGlobal.AddNotification("Gửi tin nhắn quảng bá tinh linh thành công!");
                        break;
                    }
                    default:
                    {
                        KTGlobal.AddNotification("Thông tin kênh Chat bị lỗi, hãy đóng khung và thử lại!");
                        break;
                    }
                }
            }
            this.Close?.Invoke();
        }
        #endregion

        #region Private methods

        #endregion

        #region Public methods

        #endregion
    }
}
