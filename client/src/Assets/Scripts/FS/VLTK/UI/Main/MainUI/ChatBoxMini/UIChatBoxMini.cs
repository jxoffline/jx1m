using FS.GameEngine.Logic;
using FS.VLTK.Factory.ObjectsManager;
using FS.VLTK.Utilities.UnityComponent;
using FS.VLTK.Utilities.UnityUI;
using Server.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using static FS.VLTK.Entities.Enum;

namespace FS.VLTK.UI.Main.MainUI
{
    /// <summary>
    /// Khung ChatBox Mini
    /// </summary>
    public class UIChatBoxMini : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// RectTransform nội dung Chat
        /// </summary>
        [SerializeField]
        private RectTransform ChatBoxContentRoot;

        /// <summary>
        /// Button mở khung Chat Box
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_OpenChatBox;

        /// <summary>
        /// Prefab nội dung Chat
        /// </summary>
        [SerializeField]
        private UIRichText UIText_ChatContentPrefab;
        #endregion

        #region Constants
        /// <summary>
        /// Số tin nhắn tối đa được lưu lại ở mỗi kênh
        /// </summary>
        private const int MaxMessage = 20;
        #endregion

        #region Private fields
        /// <summary>
        /// Danh sách nội dung Chat đang được hiển thị
        /// </summary>
        private Queue<UIRichText> chatContentList = new Queue<UIRichText>();

        /// <summary>
        /// ScrollView chứa nội dung chat
        /// </summary>
        private UnityEngine.UI.ScrollRect scrollView;
        #endregion

        #region Properties
        /// <summary>
        /// Sự kiện khi nút Mở khung Chat được ấn
        /// </summary>
        public Action OpenChatBox { get; set; }

        /// <summary>
        /// Sự kiện dịch chuyển đén vị trí tương ứng
        /// </summary>
        public Action<int, int, int> GoTo { get; set; }
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi ở Frame đầu tiên
        /// </summary>
        private void Start()
        {
            this.InitPrefabs();
            this.ClearAllChatContents();
        }
        #endregion

        #region Code UI
        /// <summary>
        /// Khởi tạo ban đầu
        /// </summary>
        private void InitPrefabs()
        {
            this.UIButton_OpenChatBox.onClick.AddListener(this.ButtonOpenChatBox_Clicked);
            this.scrollView = this.ChatBoxContentRoot.transform.parent.parent.gameObject.GetComponent<UnityEngine.UI.ScrollRect>();
        }

        /// <summary>
        /// Sự kiện khi nút Mở khung ChatBox được ấn
        /// </summary>
        private void ButtonOpenChatBox_Clicked()
        {
            this.OpenChatBox?.Invoke();
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
        private void RebuildLayout()
        {
            this.StartCoroutine(this.ExecuteNextFrame(() => {
                UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(this.ChatBoxContentRoot);
                this.scrollView.normalizedPosition = Vector2.zero;
            }));
        }

        /// <summary>
        /// Xóa tất cả nội dung Chat
        /// </summary>
        private void ClearAllChatContents()
        {
            foreach (Transform child in this.ChatBoxContentRoot)
            {
                if (child.gameObject != this.UIText_ChatContentPrefab.gameObject)
                {
                    GameObject.Destroy(child.gameObject);
                }
            }
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Thêm tin nhắn mới
        /// </summary>
        /// <param name="spriteChat"></param>
        public void AddMessage(SpriteChat spriteChat)
        {
            UIRichText richText = GameObject.Instantiate<UIRichText>(this.UIText_ChatContentPrefab);
            richText.transform.SetParent(this.ChatBoxContentRoot, false);
            richText.gameObject.SetActive(true);
            richText.Text = KTGlobal.DisplayChat(spriteChat);
            if (Regex.IsMatch(spriteChat.Content, "\\<link\\=\"(VoiceChat_\\w+)\"\\>"))
            {
                Match match = Regex.Match(spriteChat.Content, "\\<link\\=\"(VoiceChat_\\w+)\"\\>");
                if (match != null)
                {
                    string eventName = match.Groups[1].Value;
                    string chatID = match.Groups[2].Value;

                    richText.ClickEvents.Add(eventName, (_) => {
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

                        richText.ClickEvents.Add(eventName, (_) => {
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
                    richText.ClickEvents.Add("ITEM_" + itemGD.Id, (_) => {
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
                    richText.ClickEvents.Add("PET_" + petData.ID, (_) =>
                    {
                        PlayZone.Instance.ShowUIPetInfo(petData);
                    });
                }
            }

            this.chatContentList.Enqueue(richText);
            /// Nếu danh sách hiện đang quá giới hạn
            if (this.chatContentList.Count > UIChatBoxMini.MaxMessage)
            {
                UIRichText uiRichText = this.chatContentList.Dequeue();
                GameObject.Destroy(uiRichText.gameObject);
            }

            this.RebuildLayout();
        }

        /// <summary>
        /// Làm rỗng dữ liệu Chat
        /// </summary>
        public void ClearChat()
        {
            this.chatContentList.Clear();
            this.ClearAllChatContents();
        }
        #endregion
    }
}

