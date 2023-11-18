using FS.GameEngine.Logic;
using FS.VLTK;
using FS.VLTK.Network;
using FS.VLTK.UI;
using FS.VLTK.UI.Main;
using FS.VLTK.UI.Main.SpecialEvents.FactionBattle;
using Server.Data;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Quản lý các khung giao diện trong màn chơi
/// </summary>
public partial class PlayZone
{
    #region Đua top
    /// <summary>
    /// Khung đua top
    /// </summary>
    public UITopRanking UITopRanking { get; protected set; }

    /// <summary>
    /// Hiển thị khung đua top
    /// </summary>
    /// <param name="data"></param>
    public void ShowUITopRanking(List<TopRankingConfig> data)
    {
        CanvasManager canvas = Global.MainCanvas.GetComponent<CanvasManager>();
        this.UITopRanking = canvas.LoadUIPrefab<UITopRanking>("MainGame/UITopRanking");
        canvas.AddUI(this.UITopRanking);

        this.UITopRanking.Close = this.CloseUITopRanking;
        this.UITopRanking.Data = data;
        this.UITopRanking.GetAward = (rankType, awardIndex) =>
        {
            KT_TCPHandler.SendGetTopRankingAward(rankType, awardIndex);
        };
    }

    /// <summary>
    /// Đóng khung đua top
    /// </summary>
    public void CloseUITopRanking()
    {
        if (this.UITopRanking != null)
        {
            GameObject.Destroy(this.UITopRanking.gameObject);
            this.UITopRanking = null;
            return;
        }
    }
    #endregion

    #region Bảng xếp hạng
    /// <summary>
    /// Khung bảng xếp hạng
    /// </summary>
    public UIRanking UIRanking { get; protected set; }

    /// <summary>
    /// Hiển thị khung bảng xếp hạng
    /// </summary>
    public void ShowUIRanking()
    {
        CanvasManager canvas = Global.MainCanvas.GetComponent<CanvasManager>();
        this.UIRanking = canvas.LoadUIPrefab<UIRanking>("MainGame/UIRanking");
        canvas.AddUI(this.UIRanking);

        this.UIRanking.Close = this.CloseUIRanking;
        this.UIRanking.QueryPage = (type, pageID) => {
            KTGlobal.ShowLoadingFrame("Đang tải dữ liệu bảng xếp hạng...");

            KT_TCPHandler.SendQueryPlayerRanking(type, pageID);
        };
    }

    /// <summary>
    /// Đóng khung bảng xếp hạng
    /// </summary>
    public void CloseUIRanking()
    {
        if (this.UIRanking != null)
        {
            GameObject.Destroy(this.UIRanking.gameObject);
            this.UIRanking = null;
            return;
        }
    }
    #endregion
}
