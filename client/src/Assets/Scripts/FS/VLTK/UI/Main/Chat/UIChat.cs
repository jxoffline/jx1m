#define ENABLE_TEST

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using TMPro;
using FS.VLTK.Utilities.UnityUI;
using FS.VLTK.UI.Main.Chat;
using Server.Data;
using FS.GameEngine.Logic;
using static FS.VLTK.Entities.Enum;
using System.Collections;
using System.Text.RegularExpressions;
using FS.VLTK.Factory.ObjectsManager;
using FS.VLTK.Utilities.UnityComponent;
using FS.VLTK.Entities.Config;

namespace FS.VLTK.UI.Main
{
    /// <summary>
    /// Khung Chat
    /// </summary>
    public class UIChat : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Prefab Text nội dung Chat
        /// </summary>
        [SerializeField]
        private UIRichText UIText_ContentPrefab;

        /// <summary>
        /// Prefab Tab kênh
        /// </summary>
        [SerializeField]
        private UIToggleSprite UITab_Prefab;

        /// <summary>
        /// Khung chọn kênh chat
        /// </summary>
        [SerializeField]
        private UIChat_SelectChannelFrame UI_SelectChannelFrame;

        /// <summary>
        /// Button chọn kênh chat
        /// </summary>
        [SerializeField]
        private UIButtonSprite UIButton_SelectChatChannel;

        /// <summary>
        /// Button chọn biểu tượng cảm xúc
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_SelectSticker;

        /// <summary>
        /// Khung chọn biểu tượng cảm xúc
        /// </summary>
        [SerializeField]
        private UIChat_SelectStickerFrame UI_SelectStickerFrame;

        /// <summary>
        /// Button Voice chat
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_VoiceChat;

        /// <summary>
        /// Button xóa Chat
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_Clear;

        /// <summary>
        /// Button gửi Chat
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_SendChat;

        /// <summary>
        /// Gửi vị trí hiện tại lên kênh Chat
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_SendCurrentLocation;

        /// <summary>
        /// Button đóng khung
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_Close;

        /// <summary>
        /// Input nhập nội dung Chat
        /// </summary>
        [SerializeField]
        private TMP_InputField UIInput_ChatMessage;

        /// <summary>
        /// Khung tin nhắn thoại
        /// </summary>
        [SerializeField]
        private UIChat_VoiceChatFrame UI_VoiceChatFrame;
        #endregion

        #region Constants
        /// <summary>
        /// Số tin nhắn tối đa được lưu lại ở mỗi kênh
        /// </summary>
        private const int MaxMessage = 50;
        #endregion

        #region Properties
        /// <summary>
        /// Sự kiện đóng khung
        /// </summary>
        public Action Close { get; set; }

        /// <summary>
        /// Sự kiện gửi tin nhắn Chat
        /// </summary>
        public Action<SpriteChat> SendChat { get; set; }

        /// <summary>
        /// Sự kiện xóa dữ liệu Chat
        /// </summary>
        public Action ClearChat { get; set; }

        /// <summary>
        /// Sự kiện gửi tin nhắn thoại
        /// </summary>
        public Action<string, ChatChannel, byte[]> SendVoiceChat { get; set; }

        /// <summary>
        /// SỰ kiện dịch chuyển đến vị trí tương ứng
        /// </summary>
        public Action<int, int, int> GoTo { get; set; }
        #endregion

        #region Private fields
        /// <summary>
        /// Đối tượng chứa danh sách nội dung Chat
        /// </summary>
        private RectTransform chatContainer;

        /// <summary>
        /// Đối tượng chứa danh sách Toggle kênh Chat
        /// </summary>
        private RectTransform channelTabContainer;

        /// <summary>
        /// ScrollView nội dung Chat
        /// </summary>
        private UnityEngine.UI.ScrollRect chatScrollView;

        /// <summary>
        /// Danh sách nội dung Chat
        /// </summary>
        private static readonly Dictionary<ChatChannel, Queue<SpriteChat>> chatListData = new Dictionary<ChatChannel, Queue<SpriteChat>>()
        {
            {ChatChannel.Faction, new Queue<SpriteChat>()},
            {ChatChannel.Guild, new Queue<SpriteChat>()},
            //{ChatChannel.Allies, new Queue<SpriteChat>()},
            {ChatChannel.Near, new Queue<SpriteChat>()},
            {ChatChannel.Private, new Queue<SpriteChat>()},
            {ChatChannel.System, new Queue<SpriteChat>()},
            {ChatChannel.System_Broad_Chat, new Queue<SpriteChat>()},
            {ChatChannel.Team, new Queue<SpriteChat>()},
            {ChatChannel.Global, new Queue<SpriteChat>()},
            {ChatChannel.KuaFuLine, new Queue<SpriteChat>()},
            {ChatChannel.Special, new Queue<SpriteChat>()},
        };

        /// <summary>
        /// Hàng đợi các phần tử đang được xếp trong danh sách kênh hiện tại
        /// </summary>
        private readonly Queue<UIRichText> tmpChatTextQueue = new Queue<UIRichText>();

        /// <summary>
        /// Kênh chat hiện tại
        /// </summary>
        private ChatChannel currentChannel;

        /// <summary>
        /// Kênh được chọn để gửi tin nhắn
        /// </summary>
        private ChatChannel selectedChatChannel;

        /// <summary>
        /// Tên người chơi được chọn để Chat mật
        /// </summary>
        private string selectedPlayerName = "Chưa có";
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi khi đối tượng được tạo ra
        /// </summary>
        private void Awake()
        {
            this.chatContainer = this.UIText_ContentPrefab.transform.parent.gameObject.GetComponent<RectTransform>();
            this.channelTabContainer = this.UITab_Prefab.transform.parent.gameObject.GetComponent<RectTransform>();
            this.chatScrollView = this.UIText_ContentPrefab.transform.parent.parent.parent.gameObject.GetComponent<UnityEngine.UI.ScrollRect>();
        }

        /// <summary>
        /// Hàm này gọi ở Frame đầu tiên
        /// </summary>
        private void Start()
        {
            this.InitPrefabs();
            this.UI_SelectChannelFrame.Hide();
            this.UI_SelectStickerFrame.Hide();
            this.UI_VoiceChatFrame.Hide();
        }

        /// <summary>
        /// Hàm này gọi liên tục mỗi Frame
        /// </summary>
        private void Update()
        {
            this.UIButton_SendChat.interactable = !string.IsNullOrEmpty(this.UIInput_ChatMessage.text);
        }

        /// <summary>
        /// Hàm này gọi khi đối tượng bị hủy
        /// </summary>
		private void OnDestroy()
        {
            /// Xóa dữ liệu cũ
            Global.Recorder.Release();
        }
        #endregion

        #region Code UI
        /// <summary>
        /// Khởi tạo bân đầu
        /// </summary>
        private void InitPrefabs()
        {
            this.UIButton_SendCurrentLocation.onClick.AddListener(this.ButtonSendCurrentLocation_Clicked);
            this.UIButton_Close.onClick.AddListener(this.ButtonClose_Clicked);
            this.UIButton_SelectChatChannel.Click = this.ButtonSelectChatChannel_Clicked;
            this.UIButton_SelectSticker.onClick.AddListener(this.ButtonSelectStick_Clicked);
            this.UIButton_SendChat.onClick.AddListener(this.ButtonSendChat_Clicked);
            this.UIButton_Clear.onClick.AddListener(this.ButtonClearChat_Clicked);
            this.UIButton_VoiceChat.onClick.AddListener(this.ButtonVoiceChat_Clicked);

            this.InitChannels();
            this.InitStickers();
            this.InitVoiceChat();

            if (!string.IsNullOrEmpty(KTGlobal.LastPrivateToRoleName))
            {
                this.selectedPlayerName = KTGlobal.LastPrivateToRoleName;
            }
        }

        /// <summary>
        /// Sự kiện khi Button lấy vị trí hiện tại được ấn
        /// </summary>
        private void ButtonSendCurrentLocation_Clicked()
        {
            /// Thông tin bản đồ hiện tại
            if (!Loader.Loader.Maps.TryGetValue(Global.Data.RoleData.MapCode, out Map mapData))
            {
                KTGlobal.AddNotification("Không tìm thấy bản đồ hiện tại!");
                return;
            }
            /// Nếu đang ở trong phụ bản
            else if (Global.Data.GameScene.CurrentMapData.Setting.IsCopyScene)
            {
                KTGlobal.AddNotification("Bản đồ hiện tại không cho phép lấy vị trí!");
                return;
            }

            /// ID bản đồ tương ứng
            int mapCode = mapData.ID;
            /// Vị trí hiện tại
            int posX = Global.Data.Leader.PosX;
            int posY = Global.Data.Leader.PosY;

            /// Chuỗi được thêm vào
            string appendText = string.Format("@GOTO_{0}_{1}_{2}", mapCode, posX, posY);

            /// Nếu độ dài đã quá giới hạn
            if (this.UIInput_ChatMessage.text.Length + appendText.Length >= this.UIInput_ChatMessage.characterLimit)
            {
                KTGlobal.AddNotification("Độ dài tin nhắn đã vượt quá giới hạn!");
                return;
            }

            /// Thêm vào nội dung tin nhắn
            this.UIInput_ChatMessage.text += appendText;
        }

        /// <summary>
        /// Sự kiện khi Button đóng khung được ấn
        /// </summary>
        private void ButtonClose_Clicked()
        {
            if (!string.IsNullOrEmpty(this.UIInput_ChatMessage.text))
            {
                KTGlobal.ShowMessageBox("Nội dung Chat vẫn chưa được gửi. Xác nhận <color=yellow>thoát</color>?", () => {
                    this.Close?.Invoke();
                }, true);
            }
            else
            {
                this.Close?.Invoke();
            }
        }

        /// <summary>
        /// Sự kiện khi Button chọn kênh chat được chọn
        /// </summary>
        private void ButtonSelectChatChannel_Clicked()
        {
            if (this.UI_SelectChannelFrame.Visible)
            {
                this.UI_SelectChannelFrame.Hide();
            }
            else
            {
                this.UI_SelectChannelFrame.Show();
                this.StartCoroutine(this.ExecuteNextFrame(() => {
                    this.UI_SelectChannelFrame.SelectChannel(this.selectedChatChannel);
                }, 4));
            }
        }

        /// <summary>
        /// Sự kiện khi Button chọn biểu cảm được ấn
        /// </summary>
        private void ButtonSelectStick_Clicked()
        {
            if (this.UI_SelectStickerFrame.Visible)
            {
                this.UI_SelectStickerFrame.Hide();
            }
            else
            {
                this.UI_SelectStickerFrame.Show();
            }
        }

        /// <summary>
        /// Sự kiện khi Toggle kênh chat được ấn
        /// </summary>
        /// <param name="channel"></param>
        private void ToggleChannel_Selected(ChatChannel channel)
        {
            this.currentChannel = channel;
            this.BuildChatContentByChannel(channel);
        }

        /// <summary>
        /// Sự kiện khi Button gửi tin nhắn được ấn
        /// </summary>
        private void ButtonSendChat_Clicked()
        {
            string content = this.UIInput_ChatMessage.text;
            if (string.IsNullOrEmpty(content))
            {
                KTGlobal.ShowMessageBox("Hãy nhập vào nội dung chat.", true);
                return;
            }
            else if (this.selectedChatChannel == ChatChannel.Private && string.IsNullOrEmpty(this.selectedPlayerName))
            {
                KTGlobal.ShowMessageBox("Không có tên người chơi cần gửi tin mật!", true);
                return;
            }

#if ENABLE_TEST
            /// Nếu đây là lệnh Test
            if (content.StartsWith("/"))
            {
                /// Chia thành các trường
                string[] fields = content.Split(' ');
                /// Tên lệnh
                string commandName = fields[0].Remove(0, 1);
                /// Các tham biến đằng sau
                string[] args = fields;
                /// Gọi sang bên Test
                UIChat_Test.ResolveClientTestCommand(commandName, args);
            }
            /// Nếu không phải lệnh Test
            else
            {
                /// Xóa toàn bộ thẻ HTML
                content = Utils.RemoveAllHTMLTags(content);

                this.SendChat?.Invoke(new SpriteChat()
                {
                    FromRoleName = Global.Data.RoleData.RoleName,
                    ToRoleName = this.selectedChatChannel == ChatChannel.Private ? this.selectedPlayerName : "",
                    Content = Utils.BasicNormalizeString(content),
                    Channel = (int) this.selectedChatChannel,
                });
            }
#else
            /// Xóa toàn bộ thẻ HTML
            content = Utils.RemoveAllHTMLTags(content);

            this.SendChat?.Invoke(new SpriteChat()
            {
                FromRoleName = Global.Data.RoleData.RoleName,
                ToRoleName = this.selectedChatChannel == ChatChannel.Private ? this.selectedPlayerName : "",
                Content = Utils.BasicNormalizeString(content),
                Channel = (int) this.selectedChatChannel,
            });
#endif

            this.UIInput_ChatMessage.text = "";
        }

        /// <summary>
        /// Sự kiện khi Button xóa toàn bộ tin nhắn được ấn
        /// </summary>
        private void ButtonClearChat_Clicked()
        {
            KTGlobal.ShowMessageBox("Xác nhận xóa toàn bộ tin nhắn trên kênh hiện tại?", () => {
                /// Kênh hiện tại
                if (UIChat.chatListData.TryGetValue(this.currentChannel, out Queue<SpriteChat> channelChats))
                {
                    /// Làm rỗng
                    channelChats.Clear();
                }

                this.tmpChatTextQueue.Clear();
                this.ClearChatContents();
                this.RebuildChatLayout();

                this.ClearChat?.Invoke();
            }, true);
        }

        /// <summary>
        /// Sự kiện khi Button tin nhắn thoại được ấn
        /// </summary>
        private void ButtonVoiceChat_Clicked()
        {
            if (this.UI_VoiceChatFrame.Visible)
            {
                this.UI_VoiceChatFrame.Hide();
            }
            else
            {
                this.UI_VoiceChatFrame.Show();
            }
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Thực thi sự kiện ở Frame tiếp theo
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        private IEnumerator ExecuteNextFrame(Action action, int skipFrameCount = 1)
        {
            for (int i = 1; i <= skipFrameCount; i++)
            {
                yield return null;
            }
            action?.Invoke();
        }

        /// <summary>
        /// Làm mới hiển thị danh sách Toggle kênh
        /// </summary>
        private void RebuildChannelToggleLayout()
        {
            this.StartCoroutine(this.ExecuteNextFrame(() => {
                UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(this.channelTabContainer);
            }));
        }

        /// <summary>
        /// Làm mới hiển thị danh sách tin nhắn
        /// </summary>
        private void RebuildChatLayout()
        {
            this.StartCoroutine(this.ExecuteNextFrame(() => {
                UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(this.chatContainer);
                this.chatScrollView.normalizedPosition = Vector2.zero;
            }));
        }

        /// <summary>
        /// Làm rỗng danh sách Toggle kênh chat
        /// </summary>
        private void ClearChannelToggles()
        {
            foreach (Transform child in this.channelTabContainer.transform)
            {
                if (child.gameObject != this.UITab_Prefab.gameObject)
                {
                    GameObject.Destroy(child.gameObject);
                }
            }
        }

        /// <summary>
        /// Làm rỗng danh sách Chat
        /// </summary>
        private void ClearChatContents()
        {
            foreach (Transform child in this.chatContainer.transform)
            {
                if (child.gameObject != this.UIText_ContentPrefab.gameObject)
                {
                    GameObject.Destroy(child.gameObject);
                }
            }
        }

        /// <summary>
        /// Tạo Toggle kênh chat tương ứng
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="name"></param>
        /// <param name="active"></param>
        private UIToggleSprite CreateChannelToggle(ChatChannel channel, string name)
        {
            UIToggleSprite toggle = GameObject.Instantiate<UIToggleSprite>(this.UITab_Prefab);
            toggle.transform.SetParent(this.channelTabContainer, false);
            toggle.gameObject.SetActive(true);
            toggle.Name = name;
            toggle.Active = false;
            toggle.OnSelected = (isSelected) => {
                if (isSelected)
                {
                    this.ToggleChannel_Selected(channel);
                }
            };
            return toggle;
        }

        /// <summary>
        /// Tạo tin nhắn tương ứng
        /// </summary>
        /// <param name="spriteChat"></param>
        private UIRichText CreateMessage(SpriteChat spriteChat)
        {
            UIRichText uiText = GameObject.Instantiate<UIRichText>(this.UIText_ContentPrefab);
            uiText.transform.SetParent(this.chatContainer, false);
            uiText.gameObject.SetActive(true);
            uiText.Text = KTGlobal.DisplayChat(spriteChat);
            uiText.ClickEvents.Add("SelectRole", (roleName) => {
                /// Nếu trùng tên với bản thân
                if (roleName == Global.Data.RoleData.RoleName)
                {
                    return;
                }

                this.selectedPlayerName = roleName;

                this.UI_SelectChannelFrame.PrivateChatToPlayerName = this.selectedPlayerName;
                this.UI_SelectChannelFrame.SelectChannel(ChatChannel.Private);
                this.selectedChatChannel = ChatChannel.Private;
                this.UIButton_SelectChatChannel.Name = string.Format("{0} - <color=#6bc4ff>[{1}]</color>", this.UI_SelectChannelFrame.Channels[this.selectedChatChannel], this.selectedPlayerName);
            });
            if (Regex.IsMatch(spriteChat.Content, "\\<link\\=\"(VoiceChat_\\w+)\"\\>"))
            {
                Match match = Regex.Match(spriteChat.Content, "\\<link\\=\"(VoiceChat_(\\w+))\"\\>");
                if (match != null)
                {
                    string eventName = match.Groups[1].Value;
                    string chatID = match.Groups[2].Value;

                    uiText.ClickEvents.Add(eventName, (_) => {
                        /// Gửi yêu cầu lấy nội dung tin nhắn thoại
                        KTVoiceChatManager.Instance.SendGetVoiceChatContent(chatID, (bytesData) => {
                            /// Chạy
                            Global.Recorder.Play(bytesData);
                        });
                    });
                }
            }
            if (Regex.IsMatch(spriteChat.Content, "\\<link\\=\"GoTo_(\\d+)_(\\d+)_(\\d+)\"\\>"))
            {
                Match match = Regex.Match(spriteChat.Content, "\\<link\\=\"(GoTo_(\\d+)_(\\d+)_(\\d+))\"\\>");
                if (match != null)
                {
                    try
                    {
                        string eventName = match.Groups[1].Value;
                        int mapCode = int.Parse(match.Groups[2].Value);
                        int posX = int.Parse(match.Groups[3].Value);
                        int posY = int.Parse(match.Groups[4].Value);

                        uiText.ClickEvents.Add(eventName, (_) => {
                            /// Thực hiện dịch chuyển đến vị trí tương ứng
                            this.GoTo?.Invoke(mapCode, posX, posY);
                        });
                    }
                    catch (Exception) { }
                }
            }
            /// Nếu có danh sách vật phẩm
            if (spriteChat.Items != null)
            {
                /// Duyệt danh sách vật phẩm được đính kèm
                foreach (GoodsData itemGD in spriteChat.Items)
                {
                    /// Bắt sự kiện Click tương ứng
                    uiText.ClickEvents.Add("ITEM_" + itemGD.Id, (_) => {
                        KTGlobal.ShowItemInfo(itemGD);
                    });
                }
            }
            /// Nếu có danh sách pet
            if (spriteChat.Pets != null)
            {
                /// Duyệt danh sách pet được đính kèm
                foreach (PetData petData in spriteChat.Pets)
                {
                    /// Bắt sự kiện Click tương ứng
                    uiText.ClickEvents.Add("PET_" + petData.ID, (_) =>
                    {
                        PlayZone.Instance.ShowUIPetInfo(petData);
                    });
                }
            }

            return uiText;
        }

        /// <summary>
        /// Xây danh sách Chat theo kênh
        /// </summary>
        /// <param name="channel"></param>
        private void BuildChatContentByChannel(ChatChannel channel)
        {
            this.tmpChatTextQueue.Clear();
            this.ClearChatContents();

            /// Toàn bộ, đặc biệt
            if (channel == ChatChannel.All)
            {
                /// Danh sách tin nhắn
                List<SpriteChat> list = new List<SpriteChat>();
                foreach (KeyValuePair<ChatChannel, Queue<SpriteChat>> pair in UIChat.chatListData)
                {
                    list.AddRange(pair.Value.ToList());
                }
                list = list.OrderBy(x => x.TickTime).ToList();

                foreach (SpriteChat spriteChat in list)
                {
                    UIRichText uiText = this.CreateMessage(spriteChat);
                    this.tmpChatTextQueue.Enqueue(uiText);
                }
            }
            /// Hệ thống
            else if (channel == ChatChannel.System)
            {
                List<SpriteChat> list = UIChat.chatListData[ChatChannel.System].ToList();
                list.AddRange(UIChat.chatListData[ChatChannel.System_Broad_Chat].ToList());
                list = list.OrderBy(x => x.TickTime).ToList();

                foreach (SpriteChat spriteChat in list)
                {
                    UIRichText uiText = this.CreateMessage(spriteChat);
                    this.tmpChatTextQueue.Enqueue(uiText);
                }
            }
            /// Loại khác thì tìm theo kênh
            else
            {
                if (UIChat.chatListData.TryGetValue(channel, out Queue<SpriteChat> chatList))
                {
                    List<SpriteChat> list = chatList.ToList();
                    //list = list.OrderBy(x => x.TickTime).ToList();

                    foreach (SpriteChat spriteChat in list)
                    {
                        UIRichText uiText = this.CreateMessage(spriteChat);
                        this.tmpChatTextQueue.Enqueue(uiText);
                    }
                }
            }

            this.RebuildChatLayout();
        }

        /// <summary>
        /// Khởi tạo kênh
        /// </summary>
        private void InitChannels()
        {
            this.UI_SelectChannelFrame.Channels = new Dictionary<ChatChannel, string>();
            this.UI_SelectChannelFrame.Channels[ChatChannel.Near] = "Lân cận";
            this.UI_SelectChannelFrame.Channels[ChatChannel.Team] = "Đội ngũ";
            this.UI_SelectChannelFrame.Channels[ChatChannel.Faction] = "Môn phái";
            this.UI_SelectChannelFrame.Channels[ChatChannel.Guild] = "Bang hội";
            //this.UI_SelectChannelFrame.Channels[ChatChannel.Allies] = "Liên minh";
            this.UI_SelectChannelFrame.Channels[ChatChannel.Global] = "Thế giới";
            this.UI_SelectChannelFrame.Channels[ChatChannel.KuaFuLine] = "Liên máy chủ";
            this.UI_SelectChannelFrame.Channels[ChatChannel.Special] = "Đặc biệt";
            this.UI_SelectChannelFrame.Channels[ChatChannel.Private] = "Mật";
            this.UI_SelectChannelFrame.SelectChannel(ChatChannel.Near);
            this.UI_SelectChannelFrame.ChannelSelected = (channel) => {
                this.selectedChatChannel = channel;
                if (channel == ChatChannel.Private)
                {
                    this.UIButton_SelectChatChannel.Name = string.Format("{0} - <color=#6bc4ff>[{1}]</color>", this.UI_SelectChannelFrame.Channels[this.selectedChatChannel], this.selectedPlayerName);
                }
                else
                {
                    this.UIButton_SelectChatChannel.Name = this.UI_SelectChannelFrame.Channels[channel];
                }

                /// Nếu là kênh Chat đặc biệt thì hiện thông báo vật phẩm yêu cầu
                if (channel == ChatChannel.Special)
                {
                    KTGlobal.AddNotification("Kênh chat đặc biệt yêu cầu vật phẩm [Ốc biển truyền thanh (tiểu)]!");
                }
                /// Nếu là kênh Chat liên máy củ
                else if (channel == ChatChannel.KuaFuLine)
                {
                    KTGlobal.AddNotification("Kênh chat đặc biệt yêu cầu vật phẩm [Ốc biển truyền thanh (trung)]!");
                }

                this.UI_SelectChannelFrame.Hide();
            };
            this.UI_SelectChannelFrame.ChatTo = (playerName) => {
                this.selectedPlayerName = playerName;
                KTGlobal.LastPrivateToRoleName = this.selectedPlayerName;
            };
            /// Nếu có Chat mật thì ưu tiên chọn kênh Chat mật
            if (!string.IsNullOrEmpty(KTGlobal.LastPrivateToRoleName))
            {
                this.selectedPlayerName = KTGlobal.LastPrivateToRoleName;
                this.UI_SelectChannelFrame.PrivateChatToPlayerName = this.selectedPlayerName;
                this.selectedChatChannel = ChatChannel.Private;
                this.UIButton_SelectChatChannel.Name = string.Format("{0} - <color=#6bc4ff>[{1}]</color>", this.UI_SelectChannelFrame.Channels[this.selectedChatChannel], this.selectedPlayerName);
            }
            else
            {
                this.selectedChatChannel = ChatChannel.Near;
                this.UIButton_SelectChatChannel.Name = this.UI_SelectChannelFrame.Channels[this.selectedChatChannel];
            }

            this.ClearChannelToggles();
            UIToggleSprite allChannels = this.CreateChannelToggle(ChatChannel.All, "Tất cả");
            this.CreateChannelToggle(ChatChannel.Near, "Lân cận");
            this.CreateChannelToggle(ChatChannel.Team, "Đội ngũ");
            this.CreateChannelToggle(ChatChannel.Faction, "Môn phái");
            //this.CreateChannelToggle(ChatChannel.Allies, "Liên minh");
            this.CreateChannelToggle(ChatChannel.Guild, "Bang hội");
            this.CreateChannelToggle(ChatChannel.Global, "Thế giới");
            this.CreateChannelToggle(ChatChannel.KuaFuLine, "Liên máy chủ");
            this.CreateChannelToggle(ChatChannel.Special, "Đặc biệt");
            this.CreateChannelToggle(ChatChannel.Private, "Mật");
            this.CreateChannelToggle(ChatChannel.System, "Hệ thống");
            this.StartCoroutine(this.ExecuteNextFrame(() => {
                allChannels.Active = false;
                allChannels.Active = true;
            }, 4));
            this.RebuildChannelToggleLayout();

            this.currentChannel = ChatChannel.All;
        }

        /// <summary>
        /// Khởi tạo biểu cảm
        /// </summary>
        private void InitStickers()
        {
            this.UI_SelectStickerFrame.SelectSticker = (stickerIndex) => {

                /// Chuỗi thêm vào
                string appendText = string.Format("#{0}", stickerIndex);
                /// Nếu độ dài đã quá giới hạn
                if (this.UIInput_ChatMessage.text.Length + appendText.Length >= this.UIInput_ChatMessage.characterLimit)
                {
                    KTGlobal.AddNotification("Độ dài tin nhắn đã vượt quá giới hạn!");
                    return;
                }
                this.UIInput_ChatMessage.text += appendText;
            };
        }

        /// <summary>
        /// Khởi tạo tin nhắn thoại
        /// </summary>
        private void InitVoiceChat()
        {
            this.UI_VoiceChatFrame.Send = (voiceData) => {
                this.SendVoiceChat?.Invoke(this.selectedPlayerName, this.selectedChatChannel, voiceData);
            };
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Thêm tin nhắn mới
        /// </summary>
        /// <param name="chat"></param>
        public void AddMessage(SpriteChat chat)
        {
            /// Nếu là kênh mặc định thì không thêm
            if (chat.Channel == (int) ChatChannel.Default)
            {
                /// Bỏ qua
                return;
            }

            if (UIChat.chatListData.TryGetValue((ChatChannel) chat.Channel, out Queue<SpriteChat> listChat))
            {
                /// Thêm đoạn Chat vào kênh
                listChat.Enqueue(chat);
                /// Nếu kênh hiện tại trùng với kênh có tin nhắn mới
                if (chat.Channel == (int) this.currentChannel || this.currentChannel == ChatChannel.All)
                {
                    this.tmpChatTextQueue.Enqueue(this.CreateMessage(chat));
                }

                /// Nếu vượt quá số lượng tin nhắn lưu lại
                if (listChat.Count > UIChat.MaxMessage)
                {
                    listChat.Dequeue();
                }
            }
            else
            {
                chat.Content = "<color=#ff2e2e>" + chat.Content + "</color>";
                this.tmpChatTextQueue.Enqueue(this.CreateMessage(chat));
            }

            /// Nếu số phần tử đang hiển thị ở kênh hiện tại vượt quá giới hạn
            if (this.tmpChatTextQueue.Count > UIChat.MaxMessage)
            {
                UIRichText pair = this.tmpChatTextQueue.Dequeue();
                if (pair != null)
                {
                    GameObject.Destroy(pair.gameObject);
                }
            }

            this.RebuildChatLayout();
        }

        /// <summary>
        /// Thêm tin nhắn mới mà không tương tác với UI
        /// </summary>
        /// <param name="chat"></param>
        public static void AddMessageWithoutNotify(SpriteChat chat)
        {
            if (UIChat.chatListData.TryGetValue((ChatChannel) chat.Channel, out Queue<SpriteChat> listChat))
            {
                /// Thêm đoạn Chat vào kênh
                listChat.Enqueue(chat);

                /// Nếu vượt quá số lượng tin nhắn lưu lại
                if (listChat.Count > UIChat.MaxMessage)
                {
                    listChat.Dequeue();
                }
            }
        }
        #endregion
    }
}