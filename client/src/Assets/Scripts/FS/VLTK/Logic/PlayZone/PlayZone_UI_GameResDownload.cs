using FS.GameEngine.Logic;
using FS.GameEngine.Network;
using FS.VLTK.Entities;
using FS.VLTK.UI;
using FS.VLTK.UI.Main;
using Server.Data;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Quản lý các khung giao diện trong màn chơi
/// </summary>
public partial class PlayZone
{
    #region Tải dữ liệu nhận quà
    /// <summary>
    /// Khung tải dữ liệu nhận quà
    /// </summary>
    public UIGameResDownload UIGameResDownload { get; protected set; }

    /// <summary>
    /// Mở khung tải dữ liệu nhận quà
    /// </summary>
    /// <param name="awardInfo"></param>
    /// <param name="needToDownloadFiles"></param>
    public void OpenGameResDownload(BonusDownload awardInfo, List<UpdateZipFile> needToDownloadFiles)
    {
        /// Nếu đang hiện khung thì bỏ qua
        if (this.UIGameResDownload != null)
        {
            return;
        }

        /// Tạo khung
        CanvasManager canvas = Global.MainCanvas.GetComponent<CanvasManager>();
        this.UIGameResDownload = canvas.LoadUIPrefab<UIGameResDownload>("MainGame/UIGameResDownload");
        canvas.AddUI(this.UIGameResDownload);

        this.UIGameResDownload.Close = this.CloseGameResDownload;
        this.UIGameResDownload.Data = awardInfo;
        this.UIGameResDownload.NeedDownloadFiles = needToDownloadFiles;
        this.UIGameResDownload.GetAwards = () => {
            /// Gửi yêu cầu nhận quà
            GameInstance.Game.SendGetFirstDownloadResourceAward();
        };
    }

    /// <summary>
    /// Đóng khung tải dữ liệu nhận quà
    /// </summary>
    public void CloseGameResDownload()
    {
        /// Nếu đang hiện khung
        if (this.UIGameResDownload != null)
        {
            GameObject.Destroy(this.UIGameResDownload.gameObject);
            this.UIGameResDownload = null;
        }
    }
    #endregion
}
