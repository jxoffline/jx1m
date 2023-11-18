using FS.GameEngine.Logic;
using FS.GameEngine.Network;
using FS.VLTK;
using FS.VLTK.Factory.ObjectsManager;
using FS.VLTK.UI;
using FS.VLTK.UI.Main;
using FS.VLTK.UI.Main.MainUI;
using UnityEngine;
using static FS.VLTK.Entities.Enum;

/// <summary>
/// Quản lý các khung giao diện trong màn chơi
/// </summary>
public partial class PlayZone
{
    #region UI Chat
    /// <summary>
    /// Khung Chat
    /// </summary>
    public UIChat UIChat { get; protected set; }

    /// <summary>
    /// Hiển thị khung Chat
    /// </summary>
    public void ShowUIChat()
    {
        if (this.UIChat != null)
        {
            return;
        }

        CanvasManager canvas = Global.MainCanvas.GetComponent<CanvasManager>();
        this.UIChat = canvas.LoadUIPrefab<UIChat>("MainGame/UIChat");
        canvas.AddUI(this.UIChat);

        this.UIChat.Close = this.CloseUIChat;
        this.UIChat.SendChat = (chat) => {
            GameInstance.Game.SpriteSendChat(chat.FromRoleName, chat.ToRoleName, chat.Content, (ChatChannel) chat.Channel);
        };
        this.UIChat.ClearChat = () => {
            //this.UIChatBoxMini.ClearChat();
        };
        this.UIChat.SendVoiceChat = (toRoleName, chatChannel, bytesData) => {
            /// Gửi tin nhắn thoại
            KTVoiceChatManager.Instance.SendVoiceChat(toRoleName, chatChannel, bytesData);
        };
        this.UIChat.GoTo = (mapCode, posX, posY) => {
            /// Nếu chưa đến thời gian
            if (KTGlobal.GetCurrentTimeMilis() - this.LastClickMoveTicks < this.RefreshClickMoveAfterTicks)
            {
                return;
            }
            /// Đánh dấu lần cuối Click di chuyển
            this.LastClickMoveTicks = KTGlobal.GetCurrentTimeMilis();
            /// Thực hiện tìm đường đến vị trí tương ứng
            KTGlobal.QuestAutoFindPath(mapCode, posX, posY, null);
        };
    }

    /// <summary>
    /// Đóng khung Chat
    /// </summary>
    public void CloseUIChat()
    {
        if (this.UIChat != null)
        {
            GameObject.Destroy(this.UIChat.gameObject);
            this.UIChat = null;
        }
    }
    #endregion

    #region Chat Box Mini
    /// <summary>
    /// Khung ChatBox Mini
    /// </summary>
    public UIChatBoxMini UIChatBoxMini { get; protected set; }

    /// <summary>
    /// Hiển thị khung ChatBox Mini
    /// </summary>
    protected void InitChatBoxMini()
    {
        if (this.UIChatBoxMini != null)
        {
            return;
        }

        CanvasManager canvas = Global.MainCanvas.GetComponent<CanvasManager>();
        this.UIChatBoxMini = canvas.LoadUIPrefab<UIChatBoxMini>("MainGame/MainUI/UIChatBoxMini");
        canvas.AddMainUI(this.UIChatBoxMini);

        this.UIChatBoxMini.OpenChatBox = () => {
            this.ShowUIChat();
        };
        this.UIChatBoxMini.GoTo = (mapCode, posX, posY) => {
            /// Nếu chưa đến thời gian
            if (KTGlobal.GetCurrentTimeMilis() - this.LastClickMoveTicks < this.RefreshClickMoveAfterTicks)
            {
                return;
            }
            /// Đánh dấu lần cuối Click di chuyển
            this.LastClickMoveTicks = KTGlobal.GetCurrentTimeMilis();
            /// Thực hiện tìm đường đến vị trí tương ứng
            KTGlobal.QuestAutoFindPath(mapCode, posX, posY, null);
        };
    }
    #endregion

    #region Special Chat Box
    /// <summary>
    /// Khung kênh chat đặc biệt
    /// </summary>
    public UISpecialChatBox UISpecialChatBox { get; protected set; }

    /// <summary>
    /// Hiện khung kênh chat đặc biệt
    /// </summary>
    protected void InitSpecialChatBox()
    {
        if (this.UISpecialChatBox != null)
        {
            return;
        }

        CanvasManager canvas = Global.MainCanvas.GetComponent<CanvasManager>();
        this.UISpecialChatBox = canvas.LoadUIPrefab<UISpecialChatBox>("MainGame/MainUI/UISpecialChatBox");
        canvas.AddMainUI(this.UISpecialChatBox);

        /// Ẩn khung
        this.UISpecialChatBox.Visible = false;
    }
    #endregion
}
