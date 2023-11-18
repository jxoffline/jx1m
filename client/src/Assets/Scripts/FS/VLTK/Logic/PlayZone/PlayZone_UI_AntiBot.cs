using FS.GameEngine.Logic;
using FS.VLTK.Network;
using FS.VLTK.UI;
using FS.VLTK.UI.Main;
using Server.Data;
using UnityEngine;

/// <summary>
/// Quản lý các khung giao diện trong màn chơi
/// </summary>
public partial class PlayZone
{
    #region Captcha
    /// <summary>
    /// Khung Captcha
    /// </summary>
    public UICaptcha UICaptcha { get; protected set; }

    /// <summary>
    /// Hiển thị khung Captcha
    /// </summary>
    /// <param name="data"></param>
    public void ShowUICaptcha(G2C_Captcha data)
    {
        /// Nếu đang hiển thị
        if (this.UICaptcha != null)
        {
            /// Đóng khung
            this.CloseUICaptcha();
        }

        CanvasManager canvas = Global.MainCanvas.GetComponent<CanvasManager>();
        this.UICaptcha = canvas.LoadUIPrefab<UICaptcha>("MainGame/UICaptcha");
        canvas.AddUI(this.UICaptcha);

        this.UICaptcha.Data = data;
        this.UICaptcha.Close = this.CloseUICaptcha;
        this.UICaptcha.Submit = (answer) => {
            /// Gửi đáp án Captcha
            KT_TCPHandler.SendAnswerCaptcha(answer);
        };
    }

    /// <summary>
    /// Đóng khung Chat
    /// </summary>
    public void CloseUICaptcha()
    {
        if (this.UICaptcha != null)
        {
            GameObject.Destroy(this.UICaptcha.gameObject);
            this.UICaptcha = null;
        }
    }
    #endregion
}
