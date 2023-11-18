using FS.GameEngine.Logic;
using FS.GameEngine.Network;
using FS.VLTK;
using FS.VLTK.UI;
using FS.VLTK.UI.Main;
using UnityEngine;

/// <summary>
/// Quản lý các khung giao diện trong màn chơi
/// </summary>
public partial class PlayZone
{
    #region Phúc lợi
    /// <summary>
    /// Khung phúc lợi
    /// </summary>
    public UIWelfare UIWelfare { get; protected set; }

    /// <summary>
    /// Mở khung phúc lợi
    /// </summary>
    /// <param name="isDefaultShowFirstRecharge"></param>
    public void OpenUIWelfare(bool isDefaultShowFirstRecharge = false)
    {
        /// Nếu đang mở sẵn khung
        if (this.UIWelfare != null)
        {
            return;
        }

        CanvasManager canvas = Global.MainCanvas.GetComponent<CanvasManager>();
        this.UIWelfare = canvas.LoadUIPrefab<UIWelfare>("MainGame/Welfare/UIWelfare");
        canvas.AddUI(this.UIWelfare);

        this.UIWelfare.Close = this.HideUIWelfare;
        this.UIWelfare.UIRecharge.DefaultShowFirstRecharge = isDefaultShowFirstRecharge;
        this.UIWelfare.UIRecharge.QueryRechargeInfo = () => {
            /// Hiển thị màn hình đợi tải
            KTGlobal.ShowLoadingFrame("Đang tải dữ liệu phúc lợi nạp thẻ...");
            /// Gửi yêu cầu truy vấn thông tin phúc lợi nạp thẻ
            GameInstance.Game.QueryWelfareRechargeInfo(Global.Data.RoleData.RoleID);
        };
        this.UIWelfare.UIRecharge.GetFirstRechargeAward = (awardInfo) => {
            /// Gửi yêu cầu nhận phần thưởng nạp lần đầu
            GameInstance.Game.SendSpriteGetRechargeAward(Global.Data.RoleData.RoleID, (int) ActivityTypes.InputFirst, -1);
        };
        this.UIWelfare.UIRecharge.GetEverydayRechargeAward = (awardInfo) => {
            /// Gửi yêu cầu nhận phần thưởng nạp mỗi ngày
            GameInstance.Game.SendSpriteGetRechargeAward(Global.Data.RoleData.RoleID, (int) ActivityTypes.MeiRiChongZhiHaoLi, awardInfo.ID);
        };
        this.UIWelfare.UIRecharge.GetTotalRechargeAward = (awardInfo) => {
            /// Gửi yêu cầu nhận phần thưởng tích nạp
            GameInstance.Game.SendSpriteGetRechargeAward(Global.Data.RoleData.RoleID, (int) ActivityTypes.TotalCharge, awardInfo.ID);
        };
        this.UIWelfare.UIRecharge.GetTotalConsumeAward = (awardInfo) => {
            /// Gửi yêu cầu nhận phần thưởng tích tiêu
            GameInstance.Game.SendSpriteGetRechargeAward(Global.Data.RoleData.RoleID, (int) ActivityTypes.TotalConsume, awardInfo.ID);
        };
        this.UIWelfare.UIEverydayOnline.QueryGetEverydayOnlineInfo = () => {
            /// Hiển thị màn hình đợi tải
            KTGlobal.ShowLoadingFrame("Đang tải dữ liệu phúc lợi Online...");
            /// Gửi yêu cầu truy vấn thông tin phúc lợi Online
            GameInstance.Game.QueryWelfareEverydayOnlineInfo(Global.Data.RoleData.RoleID);
        };
        this.UIWelfare.UIEverydayOnline.Get = (awardInfo) => {
            /// Gửi yêu cầu nhận quà
            GameInstance.Game.SendSpriteGetEverydayOnlineAward(Global.Data.RoleData.RoleID);
        };
        this.UIWelfare.UISevenDayLogin.QueryGetSevenDayLoginInfo = () => {
            /// Hiển thị màn hình đợi tải
            KTGlobal.ShowLoadingFrame("Đang tải dữ liệu phúc lợi đăng nhập 7 ngày...");
            /// Gửi yêu cầu truy vấn thông tin phúc lợi đăng nhập 7 ngày
            GameInstance.Game.QueryWelfareSevenDayLoginInfo(Global.Data.RoleData.RoleID);
        };
        this.UIWelfare.UISevenDayLogin.Get = (awardInfo) => {
            /// Gửi yêu cầu nhận quà
            GameInstance.Game.SendSpriteGetSevenDayLoginAward(Global.Data.RoleData.RoleID, 0);
        };
        this.UIWelfare.UISevenDayLogin.GetContinuous = (awardInfo) => {
            /// Gửi yêu cầu nhận quà
            GameInstance.Game.SendSpriteGetSevenDayLoginAward(Global.Data.RoleData.RoleID, 1);
        };
        this.UIWelfare.UISeriesLogin.QueryGetSeriesLoginAwards = () => {
            /// Hiển thị màn hình đợi tải
            KTGlobal.ShowLoadingFrame("Đang tải dữ liệu phúc lợi điểm danh...");
            /// Gửi yêu cầu truy vấn thông tin phúc lợi đăng nhập tích lũy
            GameInstance.Game.QueryWelfareSeriesLoginInfo(Global.Data.RoleData.RoleID);
        };
        this.UIWelfare.UISeriesLogin.GetAward = (day) => {
            /// Gửi yêu cầu nhận quà
            GameInstance.Game.SendSpriteGetSeriesLoginAward(Global.Data.RoleData.RoleID, day);
        };
        this.UIWelfare.UILevelUp.QueryGetLevelUpInfo = () => {
            /// Hiển thị màn hình đợi tải
            KTGlobal.ShowLoadingFrame("Đang tải dữ liệu phúc lợi thăng cấp...");
            /// Gửi yêu cầu truy vấn thông tin phúc lợi thăng cấp
            GameInstance.Game.QueryWelfareLevelUpInfo(Global.Data.RoleData.RoleID);
        };
        this.UIWelfare.UILevelUp.Get = (awardInfo) => {
            /// Gửi yêu cầu nhận quà
            GameInstance.Game.SendSpriteGetLevelUpAward(Global.Data.RoleData.RoleID, awardInfo.ID);
        };
        this.UIWelfare.MonthCard.BuyNow = () => {
            /// Gửi yêu cầu mua thẻ tháng
            GameInstance.Game.SpriteBuyMonthCard();
        };
        this.UIWelfare.MonthCard.QueryGetMonthCardAwards = () => {
            /// Hiển thị màn hình đợi tải
            KTGlobal.ShowLoadingFrame("Đang tải dữ liệu phúc lợi thẻ tháng...");
            /// Gửi yêu cầu truy vấn thông tin phúc lợi thẻ tháng
            GameInstance.Game.SpriteMonthCardInfo();
        };
        this.UIWelfare.MonthCard.GetAward = (day) => {
            /// Gửi yêu cầu nhận quà phúc lợi thẻ tháng
            GameInstance.Game.SpriteGetMonthCardAwardCard(day);
        };
    }

    /// <summary>
    /// Đóng khung phúc lợi
    /// </summary>
    public void HideUIWelfare()
    {
        /// Nếu đang mở khung
        if (this.UIWelfare != null)
        {
            GameObject.Destroy(this.UIWelfare.gameObject);
            this.UIWelfare = null;
        }
    }
    #endregion
}
