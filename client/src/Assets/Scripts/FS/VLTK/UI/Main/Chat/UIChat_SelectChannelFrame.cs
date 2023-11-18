using FS.VLTK.Utilities.UnityUI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static FS.VLTK.Entities.Enum;
using TMPro;
using FS.GameEngine.Logic;

namespace FS.VLTK.UI.Main.Chat
{
    /// <summary>
    /// Bảng chọn kênh chat
    /// </summary>
    public class UIChat_SelectChannelFrame : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Prefab kênh
        /// </summary>
        [SerializeField]
        private UIToggleSprite UITab_ChannelPrefab;

        /// <summary>
        /// Button đóng khung
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_Close;

        /// <summary>
        /// Input nhập tên chat với người chơi
        /// </summary>
        [SerializeField]
        private TMP_InputField UIInput_ChatTo;

        /// <summary>
        /// Button thiết lập chat với người chơi có tên tương ứng
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_OKChatTo;
        #endregion

        #region Properties
        /// <summary>
        /// Danh sách kênh
        /// </summary>
        public Dictionary<ChatChannel, string> Channels { get; set; }

        /// <summary>
        /// Kênh được chọn
        /// </summary>
        public Action<ChatChannel> ChannelSelected { get; set; }

        /// <summary>
        /// Sự kiện đóng khung
        /// </summary>
        public Action Close { get; set; }

        /// <summary>
        /// Sự kiện thiết lập chat cùng người chơi có tên tương ứng được nhập vào
        /// </summary>
        public Action<string> ChatTo { get; set; }

        /// <summary>
        /// Đang trong trạng thái hiển thị không
        /// </summary>
        public bool Visible
        {
            get
            {
                return this.gameObject.activeSelf;
            }
        }
        #endregion

        #region Private fields
        /// <summary>
        /// Đối tượng chứa danh sách kênh
        /// </summary>
        private RectTransform container;

        /// <summary>
        /// Danh sách Toggle theo kênh chat
        /// </summary>
        private readonly Dictionary<ChatChannel, UIToggleSprite> channelToggles = new Dictionary<ChatChannel, UIToggleSprite>();

        /// <summary>
        /// Kênh được chọn lần trước
        /// </summary>
        private ChatChannel lastSelectedChanned;

        /// <summary>
        /// Chat mật với người chơi tên tương ứng
        /// </summary>
        public string PrivateChatToPlayerName { get; set; } = "";
        #endregion

        #region Private fields
        /// <summary>
        /// Đã chạy qua hàm Start chưa
        /// </summary>
        private bool isStarted = false;
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi khi đối tượng được tạo ra
        /// </summary>
        private void Awake()
        {
            this.container = this.UITab_ChannelPrefab.transform.parent.gameObject.GetComponent<RectTransform>();
        }

        /// <summary>
        /// Hàm này gọi ở Frame đầu tiên
        /// </summary>
        private void Start()
        {
            this.InitPrefabs();
            this.Build();
            this.StartCoroutine(this.RunNextFrame(() => {
                /// Đánh dấu đã chạy qua hàm Start
                this.isStarted = true;
            }));
        }

        /// <summary>
        /// Hàm này gọi liên tục mỗi Frame
        /// </summary>
        private void Update()
        {
            this.RefreshPrivateChannel();
        }
        #endregion

        #region Code UI
        /// <summary>
        /// Khởi tạo ban đầu
        /// </summary>
        private void InitPrefabs()
        {
            this.UIButton_Close.onClick.AddListener(this.ButtonClose_Clicked);
            this.UIButton_OKChatTo.onClick.AddListener(this.ButtonChatTo_Clicked);
        }

        /// <summary>
        /// Sự kiện khi Button đóng khung được ấn
        /// </summary>
        private void ButtonClose_Clicked()
        {
            this.Close?.Invoke();
            this.Hide();
        }

        /// <summary>
        /// Sự kiện khi Button chat với người chơi có tên tương ứng nhập vào được ấn
        /// </summary>
        private void ButtonChatTo_Clicked()
        {
            string playerName = Utils.BasicNormalizeString(this.UIInput_ChatTo.text);
            if (string.IsNullOrEmpty(playerName))
            {
                KTGlobal.ShowMessageBox("Hãy nhập vào tên người chơi muốn chat cùng, sau đó ấn OK!", true);
                return;
            }
            else if (playerName == Global.Data.RoleData.RoleName)
            {
                KTGlobal.ShowMessageBox("Không thể tự chat mật với bản thân mình!", true);
                return;
            }

            this.PrivateChatToPlayerName = playerName;
            this.ChatTo?.Invoke(this.PrivateChatToPlayerName);
            this.SelectChannel(ChatChannel.Private);
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Thực thi ở Frame tiếp theo
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        private IEnumerator RunNextFrame(Action action)
        {
            yield return null;
            action?.Invoke();
        }

        /// <summary>
        /// Làm rỗng danh sách
        /// </summary>
        private void Clear()
        {
            foreach (Transform child in this.container.transform)
            {
                if (child.gameObject != this.UITab_ChannelPrefab.gameObject)
                {
                    GameObject.Destroy(child.gameObject);
                }
            }
            this.channelToggles.Clear();
        }

        /// <summary>
        /// Làm mới hiển thị
        /// </summary>
        private void RebuildLayout()
        {
            //this.StartCoroutine(this.RunNextFrame(() => {
                UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(this.container);
            //}));
        }

        /// <summary>
        /// Làm mới hiển thị kênh mật
        /// </summary>
        private void RefreshPrivateChannel()
        {
            if (this.channelToggles.ContainsKey(ChatChannel.Private))
            {
                if (string.IsNullOrEmpty(this.PrivateChatToPlayerName))
                {
                    this.channelToggles[ChatChannel.Private].Active = false;
                    this.channelToggles[ChatChannel.Private].gameObject.SetActive(false);
                }
                else
                {
                    this.channelToggles[ChatChannel.Private].gameObject.SetActive(true);
                    this.channelToggles[ChatChannel.Private].Name = string.Format("{0} - <color=#6bc4ff>[{1}]</color>", "Mật", this.PrivateChatToPlayerName);
                }
            }
        }

        /// <summary>
        /// Xây danh sách
        /// </summary>
        private void Build()
        {
            this.Clear();
            if (this.Channels == null)
            {
                return;
            }

            foreach (KeyValuePair<ChatChannel, string> pair in this.Channels)
            {
                UIToggleSprite toggle = GameObject.Instantiate<UIToggleSprite>(this.UITab_ChannelPrefab);
                toggle.transform.SetParent(this.container, false);
                toggle.gameObject.SetActive(true);
                toggle.Name = pair.Value;
                toggle.Active = false;
                toggle.OnSelected = (isSelected) => {
                    /// Nếu chưa chạy qua hàm Start
                    if (!this.isStarted)
                    {
                        return;
                    }

                    if (isSelected)
                    {
                        this.lastSelectedChanned = pair.Key;
                        this.ChannelSelected?.Invoke(pair.Key);
                    }
                };
                this.channelToggles[pair.Key] = toggle;
            }
        }

        /// <summary>
        /// Làm mới dữ liệu Toggle
        /// </summary>
        private void ResetToggles()
        {
            foreach (UIToggleSprite uiToggle in this.channelToggles.Values)
            {
                uiToggle.Active = false;
            }
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Chọn kệnh
        /// </summary>
        /// <param name="channel"></param>
        public void SelectChannel(ChatChannel channel)
        {
            /// Nếu trùng với kênh được chọn lần trước
            if (this.lastSelectedChanned == channel)
            {
                return;
            }
            this.lastSelectedChanned = channel;

            /// Nếu là kênh mật
            if (channel == ChatChannel.Private)
            {
                this.RefreshPrivateChannel();
            }

            this.ResetToggles();

            if (this.channelToggles.TryGetValue(channel, out UIToggleSprite toggle))
            {
                toggle.Active = true;
            }
        }

        /// <summary>
        /// Hiển thị
        /// </summary>
        public void Show()
        {
            /// Lỗi đéo gì nên phải hiện thằng Prefab lên trước
            this.UITab_ChannelPrefab.gameObject.SetActive(true);
            this.gameObject.SetActive(true);
            /// Xong thì ẩn Prefab
            this.UITab_ChannelPrefab.gameObject.SetActive(false);
            this.UIInput_ChatTo.text = this.PrivateChatToPlayerName;
            this.RebuildLayout();
        }

        /// <summary>
        /// Ẩn
        /// </summary>
        public void Hide()
        {
            this.gameObject.SetActive(false);
        }
        #endregion
    }
}
