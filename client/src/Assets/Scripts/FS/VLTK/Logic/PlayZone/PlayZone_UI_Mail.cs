using FS.GameEngine.Logic;
using FS.GameEngine.Network;
using FS.VLTK.UI;
using FS.VLTK.UI.Main;
using UnityEngine;

/// <summary>
/// Quản lý các khung giao diện trong màn chơi
/// </summary>
public partial class PlayZone
{
    #region MailBox
    /// <summary>
    /// Khung hộp thư của người chơi
    /// </summary>
    public UIMailBox UIMailBox { get; protected set; } = null;

    /// <summary>
    /// Hiển thị khung hộp thư của người chơi
    /// </summary>
    public void ShowUIMailBox()
    {
        /// Nếu đang mở
        if (this.UIMailBox != null)
        {
            return;
        }

        CanvasManager canvas = Global.MainCanvas.GetComponent<CanvasManager>();
        this.UIMailBox = canvas.LoadUIPrefab<UIMailBox>("MainGame/UIMailBox");
        canvas.AddUI(this.UIMailBox);

        this.UIMailBox.Close = this.CloseUIMailBox;
        this.UIMailBox.DeleteMail = (mailData) => {
            GameInstance.Game.SpriteDeleteUserMail(mailData.MailID.ToString());
        };
        this.UIMailBox.ReadMail = (mailData) => {
            GameInstance.Game.SpriteGetUserMailData(mailData.MailID);
        };
        this.UIMailBox.GetAllStickItemsAndMoney = (mailData) => {
            GameInstance.Game.SpriteFetchMailGoods(mailData.MailID);
        };
    }

    /// <summary>
    /// Đóng khung hộp thư của người chơi
    /// </summary>
    public void CloseUIMailBox()
    {
        /// Nếu đang mở
        if (this.UIMailBox != null)
        {
            GameObject.Destroy(this.UIMailBox.gameObject);
            this.UIMailBox = null;
        }
    }
    #endregion
}
