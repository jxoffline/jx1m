using FS.GameEngine.Logic;
using FS.VLTK.Network;
using FS.VLTK.UI;
using FS.VLTK.UI.Main;
using FS.VLTK.UI.Main.GiftCode;
using FS.VLTK.UI.Main.MainUI;
using FS.VLTK.UI.Main.SpecialEvents;
using FS.VLTK.UI.Main.SpecialEvents.FactionBattle;
using Server.Data;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Quản lý các khung giao diện trong màn chơi
/// </summary>
public partial class PlayZone
{
    #region UIEventBroadboardMini
    /// <summary>
    /// Khung công cáo sự kiện thông tin Mini
    /// </summary>
    public UIEventBroadboardMini UIEventBroadboardMini { get; protected set; }

    /// <summary>
    /// Tạo khung MiniEventBroadboard
    /// </summary>
    protected void InitMiniEventBroadboard()
    {
        if (this.UIEventBroadboardMini != null)
        {
            return;
        }

        CanvasManager canvas = Global.MainCanvas.GetComponent<CanvasManager>();
        this.UIEventBroadboardMini = canvas.LoadUIPrefab<UIEventBroadboardMini>("MainGame/MainUI/UIEventBroadboardMini");
        canvas.AddMainUI(this.UIEventBroadboardMini);

        this.UIEventBroadboardMini.OpenTeamFrame = () => {
            KT_TCPHandler.SendGetTeamInfo();
        };
    }
    #endregion

    #region UIStreakKillNotification
    /// <summary>
    /// Khung thông báo số liên trảm có được
    /// </summary>
    public UIStreakKillNotification UIStreakKillNotification { get; protected set; }

    /// <summary>
    /// Tạo khung thông báo số liên trảm có được
    /// </summary>
    protected void InitUIStreakKillNotification()
    {
        if (this.UIStreakKillNotification != null)
        {
            return;
        }

        CanvasManager canvas = Global.MainCanvas.GetComponent<CanvasManager>();
        this.UIStreakKillNotification = canvas.LoadUIPrefab<UIStreakKillNotification>("MainGame/MainUI/UIStreakKillNotification");
        canvas.AddMainUI(this.UIStreakKillNotification);
    }
    #endregion

    #region Khung GiftCode
    /// <summary>
    /// Khung GiftCode
    /// </summary>
    public UIGiftCode UIGiftCode { get; protected set; }

    /// <summary>
    /// Mở khung nhập GiftCode
    /// </summary>
    public void OpenUIGiftCode()
    {
        if (this.UIGiftCode != null)
        {
            return;
        }
        this.UIGiftCode = CanvasManager.Instance.LoadUIPrefab<UIGiftCode>("MainGame/UIGiftCode");
        CanvasManager.Instance.AddUI(this.UIGiftCode);

        this.UIGiftCode.Close = this.CloseUIGiftCode;
        this.UIGiftCode.Enter = (inputString) => {
            KT_TCPHandler.SendInputGiftCode(inputString);
            /// Đóng khung
            this.CloseUIGiftCode();
        };
    }

    /// <summary>
    /// Đóng khung nhập GiftCode
    /// </summary>
    public void CloseUIGiftCode()
    {
        if (this.UIGiftCode != null)
        {
            GameObject.Destroy(this.UIGiftCode.gameObject);
            this.UIGiftCode = null;
        }
    }
    #endregion

    #region Quản lý Button sự kiện đặc biệt
    /// <summary>
    /// Khung quản lý Button sự kiện đặc biệt
    /// </summary>
    public UISpecialEventButtons UISpecialEventButtons { get; protected set; }

    /// <summary>
    /// Khởi tạo khung quản lý Button sự kiện đặc biệt
    /// </summary>
    protected void InitUISpecialEventButtons()
    {
        if (this.UISpecialEventButtons != null)
        {
            return;
        }

        CanvasManager canvas = Global.MainCanvas.gameObject.GetComponent<CanvasManager>();
        this.UISpecialEventButtons = canvas.LoadUIPrefab<UISpecialEventButtons>("MainGame/SpecialEvents/UISpecialEventButtons");
        canvas.AddMainUI(this.UISpecialEventButtons);

        this.UISpecialEventButtons.OpenSongJinRankingBoard = () => {
            KT_TCPHandler.SendOpenSongJinRankingBoard();
        };
        this.UISpecialEventButtons.OpenFactionBattleBoard = () => {
            KT_TCPHandler.SendGetFactionBattleData();
        };
        this.UISpecialEventButtons.OpenFengHuoLianChengScoreBoard = () =>
        {
            KT_TCPHandler.SendGetFHLCScoreboard();
        };
    }
    #endregion

    #region UI sự kiện đặc biệt
    #region Tống Kim
    /// <summary>
    /// Khung bảng xếp hạng Tống Kim
    /// </summary>
    public UISongJinRankingBoard UISongJinRankingBoard { get; protected set; }

    /// <summary>
    /// Hiện bảng xếp hạng Tống Kim
    /// </summary>
    public void ShowUISongJinRankingBoard()
    {
        if (this.UISongJinRankingBoard != null)
        {
            return;
        }

        CanvasManager canvas = Global.MainCanvas.gameObject.GetComponent<CanvasManager>();
        this.UISongJinRankingBoard = canvas.LoadUIPrefab<UISongJinRankingBoard>("MainGame/SpecialEvents/UISongJinRankingBoard");
        canvas.AddUI(this.UISongJinRankingBoard);

        this.UISongJinRankingBoard.Close = this.CloseUISongJinRankingBoard;
    }

    /// <summary>
    /// Ẩn bảng xếp hạng Tống Kim
    /// </summary>
    public void CloseUISongJinRankingBoard()
    {
        if (this.UISongJinRankingBoard != null)
        {
            GameObject.Destroy(this.UISongJinRankingBoard.gameObject);
            this.UISongJinRankingBoard = null;
        }
    }
    #endregion

    #region Thi đấu môn phái
    #region Bảng xếp hạng
    /// <summary>
    /// Bảng xếp hạng thi đấu môn phái
    /// </summary>
    public UIFactionBattle_Ranking UIFactionBattle_Ranking { get; protected set; }

    /// <summary>
    /// Hiển thị khung bảng xếp hạng Thi đấu môn phái
    /// </summary>
    /// <param name="data"></param>
    public void OpenUIFactionBattleRanking(List<FACTION_PVP_RANKING> data)
    {
        CanvasManager canvas = Global.MainCanvas.GetComponent<CanvasManager>();
        this.UIFactionBattle_Ranking = canvas.LoadUIPrefab<UIFactionBattle_Ranking>("MainGame/SpecialEvents/UIFactionBattle_Ranking");
        canvas.AddUI(this.UIFactionBattle_Ranking);

        this.UIFactionBattle_Ranking.Close = this.CloseUIFactionBattleRanking;
        this.UIFactionBattle_Ranking.Data = data;
    }

    /// <summary>
    /// Đóng khung thông tin nhân vật
    /// </summary>
    public void CloseUIFactionBattleRanking()
    {
        if (this.UIFactionBattle_Ranking != null)
        {
            GameObject.Destroy(this.UIFactionBattle_Ranking.gameObject);
            this.UIFactionBattle_Ranking = null;
        }
    }
    #endregion

    #region Thông tin vòng thi đấu
    /// <summary>
    /// Khung thông tin các vòng đấu trong thi đấu môn phái
    /// </summary>
    public UIFactionBattle_Round UIFactionBattle_Round { get; set; }

    /// <summary>
    /// Hiện khung thông tin các vòng đấu trong thi đấu môn phái
    /// </summary>
    /// <param name="data"></param>
    public void OpenUIFactionBattleRoundInfo(FACTION_PVP_RANKING_INFO data)
    {
        if (this.UIFactionBattle_Round != null)
        {
            return;
        }

        CanvasManager canvas = Global.MainCanvas.GetComponent<CanvasManager>();
        this.UIFactionBattle_Round = canvas.LoadUIPrefab<UIFactionBattle_Round>("MainGame/SpecialEvents/UIFactionBattle_Round");
        canvas.AddUI(this.UIFactionBattle_Round);

        this.UIFactionBattle_Round.Close = this.CloseUIFactionBattleRoundInfo;
        this.UIFactionBattle_Round.Data = data;
    }

    /// <summary>
    /// Đóng khung thông tin các vòng đấu trong thi đấu môn phái
    /// </summary>
    public void CloseUIFactionBattleRoundInfo()
    {
        if (this.UIFactionBattle_Round != null)
        {
            GameObject.Destroy(this.UIFactionBattle_Round.gameObject);
            this.UIFactionBattle_Round = null;
        }
    }
    #endregion
    #endregion

    #region Bách bảo rương
    /// <summary>
    /// Khung bách bảo rương
    /// </summary>
    public UISeashellCircle UISeashellCircle { get; protected set; }

    /// <summary>
    /// Mở khung Bách bảo rương
    /// </summary>
    /// <param name="serverTotalSeashells"></param>
    /// <param name="lastStage"></param>
    /// <param name="lastStopPos"></param>
    /// <param name="lastBet"></param>
    public void OpenSeashellCircle(long serverTotalSeashells, int lastStage, int lastStopPos, int lastBet)
    {
        /// Nếu đang mở khung
        if (this.UISeashellCircle != null)
        {
            return;
        }

        /// Mở khung
        CanvasManager canvas = Global.MainCanvas.gameObject.GetComponent<CanvasManager>();
        this.UISeashellCircle = canvas.LoadUIPrefab<UISeashellCircle>("MainGame/UISeashellCircle");
        canvas.AddUI(this.UISeashellCircle);
        this.UISeashellCircle.Close = this.CloseSeashellCircle;

        this.UISeashellCircle.ServerTotalSeashells = serverTotalSeashells;
        this.UISeashellCircle.CurrentBet = lastBet;
        this.UISeashellCircle.CurrentPos = lastStopPos;
        this.UISeashellCircle.CurrentStage = lastStage;

        this.UISeashellCircle.GetAward = () => {
            KT_TCPHandler.SendGetSeashellCircleAward();
        };
        this.UISeashellCircle.GetSeashell = () => {
            KT_TCPHandler.SendExchangeSeashellCircleSeashell();
        };
        this.UISeashellCircle.StartTurn = (nBet) => {
            KT_TCPHandler.SendStartSeashellCircleTurn(nBet);
        };
    }

    /// <summary>
    /// Đóng khung Bách bảo rương
    /// </summary>
    public void CloseSeashellCircle()
    {
        /// Nếu đang mở khung
        if (this.UISeashellCircle != null)
        {
            GameObject.Destroy(this.UISeashellCircle.gameObject);
            this.UISeashellCircle = null;
        }
    }
    #endregion

    #region Vòng quay may mắn
    /// <summary>
    /// Khung vòng quay may mắn
    /// </summary>
    public UILuckyCircle UILuckyCircle { get; protected set; }

    /// <summary>
    /// Mở khung Vòng quay may mắn
    /// </summary>
    /// <param name="data"></param>
    public void OpenUILuckyCircle(G2C_LuckyCircle data)
    {
        /// Nếu đang mở khung
        if (this.UILuckyCircle != null)
        {
            /// Đóng khung
            this.CloseUILuckyCircle();
        }

        /// Mở khung
        CanvasManager canvas = Global.MainCanvas.gameObject.GetComponent<CanvasManager>();
        this.UILuckyCircle = canvas.LoadUIPrefab<UILuckyCircle>("MainGame/UILuckyCircle");
        canvas.AddUI(this.UILuckyCircle);
        this.UILuckyCircle.Close = this.CloseUILuckyCircle;

        this.UILuckyCircle.Data = data;
        this.UILuckyCircle.GetAward = () => {
            KT_TCPHandler.SendGetLuckyCircleAward();
        };
        this.UILuckyCircle.StartTurn = (method) => {
            KT_TCPHandler.SendStartLuckyCircleTurn(method);
        };
    }

    /// <summary>
    /// Đóng khung Vòng quay may mắn
    /// </summary>
    public void CloseUILuckyCircle()
    {
        /// Nếu đang mở khung
        if (this.UILuckyCircle != null)
        {
            GameObject.Destroy(this.UILuckyCircle.gameObject);
            this.UILuckyCircle = null;
        }
    }
    #endregion

    #region Vòng quay may mắn - đặc biệt
    /// <summary>
    /// Khung vòng quay may mắn - đặc biệt
    /// </summary>
    public UITurnPlate UITurnPlate { get; protected set; }

    /// <summary>
    /// Mở khung Vòng quay may mắn - đặc biệt
    /// </summary>
    /// <param name="items"></param>
    /// <param name="lastStopPos"></param>
    public void OpenUITurnPlate(List<KeyValuePair<int, int>> items, int lastStopPos)
    {
        /// Nếu đang mở khung
        if (this.UITurnPlate != null)
        {
            /// Đóng khung
            this.CloseUITurnPlate();
        }

        /// Mở khung
        CanvasManager canvas = Global.MainCanvas.gameObject.GetComponent<CanvasManager>();
        this.UITurnPlate = canvas.LoadUIPrefab<UITurnPlate>("MainGame/UITurnPlate");
        canvas.AddUI(this.UITurnPlate);
        this.UITurnPlate.Close = this.CloseUITurnPlate;

        this.UITurnPlate.Items = items;
        this.UITurnPlate.UpdateLastStopPos(lastStopPos);
        this.UITurnPlate.GetAwards = () => {
            KT_TCPHandler.SendGetTurnPlateAward();
        };
        this.UITurnPlate.StartTurn = () => {
            KT_TCPHandler.SendStartTurnPlateTurn();
        };
    }

    /// <summary>
    /// Đóng khung Vòng quay may mắn - đặc biệt
    /// </summary>
    public void CloseUITurnPlate()
    {
        /// Nếu đang mở khung
        if (this.UITurnPlate != null)
        {
            GameObject.Destroy(this.UITurnPlate.gameObject);
            this.UITurnPlate = null;
        }
    }
    #endregion

    #region Du Long Các
    /// <summary>
    /// Du Long Các
    /// </summary>
    public UIYouLong UIYouLong { get; protected set; }

    /// <summary>
    /// Mở khung Du Long Các
    /// </summary>
    public void OpenUIYouLong()
    {
        /// Nếu đang mở khung
        if (this.UIYouLong != null)
        {
            return;
        }

        /// Mở khung
        CanvasManager canvas = Global.MainCanvas.gameObject.GetComponent<CanvasManager>();
        this.UIYouLong = canvas.LoadUIPrefab<UIYouLong>("MainGame/UIYouLong");
        canvas.AddUI(this.UIYouLong);
        this.UIYouLong.Close = this.CloseUIYouLong;

        this.UIYouLong.GetAward = (step) => {
            /// Nếu là bước chọn ngẫu nhiên nhận thưởng
            if (step == 0)
            {
                KT_TCPHandler.SendYouLongGetRandomAwardFromList();
            }
            /// Nếu là bước nhận thưởng
            else if (step == 1)
            {
                KT_TCPHandler.SendYouLongGetAward();
            }
        };
        this.UIYouLong.ExchangeCoin = () => {
            KT_TCPHandler.SendYouLongExchangeCoin();
        };
        this.UIYouLong.TryAgain = () => {
            KT_TCPHandler.SendYouLongTryAgain();
        };
        this.UIYouLong.NextRound = () => {
            KT_TCPHandler.SendYouLongNextRound();
        };
        this.UIYouLong.Exit = () => {
            KT_TCPHandler.SendYouLongExit();
        };
    }

    /// <summary>
    /// Đóng khung Du Long Các
    /// </summary>
    public void CloseUIYouLong()
    {
        /// Nếu đang mở khung
        if (this.UIYouLong != null)
        {
            GameObject.Destroy(this.UIYouLong.gameObject);
            this.UIYouLong = null;
        }
    }
	#endregion

	#region Tranh đoạt lãnh thổ
    /// <summary>
    /// Khung chiến báo tranh đoạt lãnh thổ
    /// </summary>
    public UIColonyDispute_Achievement UIColonyDispute_Achievement { get; protected set; }

    /// <summary>
    /// Mở khung chiến báo tranh đoạt lãnh thổ
    /// </summary>
    /// <param name="data"></param>
    public void OpenUIColonyDisputeAchievement(GuildWarReport data)
	{
        /// Nếu đang mở khung
        if (this.UIColonyDispute_Achievement != null)
        {
            return;
        }

        /// Mở khung
        CanvasManager canvas = Global.MainCanvas.gameObject.GetComponent<CanvasManager>();
        this.UIColonyDispute_Achievement = canvas.LoadUIPrefab<UIColonyDispute_Achievement>("MainGame/SpecialEvents/UIColonyDispute_Achievement");
        canvas.AddUI(this.UIColonyDispute_Achievement);
        this.UIColonyDispute_Achievement.Close = this.CloseUIColonyDisputeAchievement;
        this.UIColonyDispute_Achievement.Data = data;
    }

    /// <summary>
    /// Đóng khung chiến báo tranh đoạt lãnh thổ
    /// </summary>
    public void CloseUIColonyDisputeAchievement()
	{
        /// Nếu đang mở khung
        if (this.UIColonyDispute_Achievement != null)
        {
            GameObject.Destroy(this.UIColonyDispute_Achievement.gameObject);
            this.UIColonyDispute_Achievement = null;
        }
    }
	#endregion

	#region Chúc phúc
    /// <summary>
    /// Khung vòng quay chúc phúc
    /// </summary>
    public UIPlayerPray UIPlayerPray { get; protected set; }

    /// <summary>
    /// Mở khung vòng quay chúc phúc
    /// </summary>
    public void OpenUIPlayerPray()
	{
        /// Nếu đang mở khung
        if (this.UIPlayerPray != null)
        {
            return;
        }

        /// Mở khung
        CanvasManager canvas = Global.MainCanvas.gameObject.GetComponent<CanvasManager>();
        this.UIPlayerPray = canvas.LoadUIPrefab<UIPlayerPray>("MainGame/UIPlayerPray");
        canvas.AddUI(this.UIPlayerPray);
        this.UIPlayerPray.Close = this.CloseUIPlayerPray;

        this.UIPlayerPray.StartPray = () => {
            KT_TCPHandler.SendStartTurnPlayerPray();
        };
        this.UIPlayerPray.GetAward = () => {
            KT_TCPHandler.SendGetPlayerPrayAward();
        };
    }

    /// <summary>
    /// Đóng khung vòng quay chúc phúc
    /// </summary>
    public void CloseUIPlayerPray()
	{
        /// Nếu đang mở khung
        if (this.UIPlayerPray != null)
        {
            GameObject.Destroy(this.UIPlayerPray.gameObject);
            this.UIPlayerPray = null;
        }
    }
    #endregion

    #region Võ lâm liên đấu
    /// <summary>
    /// Khung chiến báo tranh đoạt lãnh thổ
    /// </summary>
    public UITeamBattle_RankingBoard UITeamBattle_RankingBoard { get; protected set; }

    /// <summary>
    /// Mở khung chiến báo tranh đoạt lãnh thổ
    /// </summary>
    /// <param name="data"></param>
    public void OpenUITeamBattleRankingBoard(List<TeamBattleInfo> data)
    {
        /// Nếu đang mở khung
        if (this.UITeamBattle_RankingBoard != null)
        {
            return;
        }

        /// Mở khung
        CanvasManager canvas = Global.MainCanvas.gameObject.GetComponent<CanvasManager>();
        this.UITeamBattle_RankingBoard = canvas.LoadUIPrefab<UITeamBattle_RankingBoard>("MainGame/SpecialEvents/UITeamBattle_RankingBoard");
        canvas.AddUI(this.UITeamBattle_RankingBoard);
        this.UITeamBattle_RankingBoard.Close = this.CloseUITeamBattleRankingBoard;
        this.UITeamBattle_RankingBoard.Data = data;
    }

    /// <summary>
    /// Đóng khung chiến báo tranh đoạt lãnh thổ
    /// </summary>
    public void CloseUITeamBattleRankingBoard()
    {
        /// Nếu đang mở khung
        if (this.UITeamBattle_RankingBoard != null)
        {
            GameObject.Destroy(this.UITeamBattle_RankingBoard.gameObject);
            this.UITeamBattle_RankingBoard = null;
        }
    }
    #endregion

    #region Vận tiêu
    /// <summary>
    /// Khung thông tin nhiệm vụ vận tiêu
    /// </summary>
    public UICargoCarriageTaskInfo UICargoCarriageTaskInfo { get; protected set; }

    /// <summary>
    /// Mở khung thông tin nhiệm vụ vận tiêu
    /// </summary>
    /// <param name="data"></param>
    public void OpenUICargoCarriageTaskInfo(G2C_CargoCarriageTaskData data)
    {
        /// Nếu đang mở khung
        if (this.UICargoCarriageTaskInfo != null)
        {
            return;
        }

        /// Mở khung
        CanvasManager canvas = Global.MainCanvas.gameObject.GetComponent<CanvasManager>();
        this.UICargoCarriageTaskInfo = canvas.LoadUIPrefab<UICargoCarriageTaskInfo>("MainGame/SpecialEvents/UICargoCarriageTaskInfo");
        canvas.AddUI(this.UICargoCarriageTaskInfo);
        this.UICargoCarriageTaskInfo.Data = data;
        this.UICargoCarriageTaskInfo.Completed = false;
    }

    /// <summary>
    /// Đóng khung chiến báo tranh đoạt lãnh thổ
    /// </summary>
    public void CloseUICargoCarriageTaskInfo()
    {
        /// Nếu đang mở khung
        if (this.UICargoCarriageTaskInfo != null)
        {
            GameObject.Destroy(this.UICargoCarriageTaskInfo.gameObject);
            this.UICargoCarriageTaskInfo = null;
        }
    }
    #endregion

    #region Phong Hỏa Liên Thành
    /// <summary>
    /// Khung bảng xếp hạng Tống Kim
    /// </summary>
    public UIFHLCScoreboard UIFHLCScoreboard { get; protected set; }

    /// <summary>
    /// Hiện bảng xếp hạng Tống Kim
    /// </summary>
    /// <param name="data"></param>
    public void ShowUIFHLCScoreboard(FHLCScoreboardData data)
    {
        if (this.UIFHLCScoreboard != null)
        {
            return;
        }

        CanvasManager canvas = Global.MainCanvas.gameObject.GetComponent<CanvasManager>();
        this.UIFHLCScoreboard = canvas.LoadUIPrefab<UIFHLCScoreboard>("MainGame/SpecialEvents/UIFHLCScoreboard");
        canvas.AddUI(this.UIFHLCScoreboard);

        this.UIFHLCScoreboard.Close = this.CloseUIFHLCScoreboard;
        this.UIFHLCScoreboard.Data = data;
    }

    /// <summary>
    /// Ẩn bảng xếp hạng Tống Kim
    /// </summary>
    public void CloseUIFHLCScoreboard()
    {
        if (this.UIFHLCScoreboard != null)
        {
            GameObject.Destroy(this.UIFHLCScoreboard.gameObject);
            this.UIFHLCScoreboard = null;
        }
    }
    #endregion
    #endregion

    #region Danh sách hoạt động
    /// <summary>
    /// Khung danh sách hoạt động
    /// </summary>
    public UIActivityList UIActivityList { get; protected set; }

    /// <summary>
    /// Mở khung danh sách hoạt động
    /// </summary>
    public void OpenActivityList()
    {
        /// Nếu đang mở khung
        if (this.UIActivityList != null)
        {
            return;
        }

        /// Mở khung
        CanvasManager canvas = Global.MainCanvas.gameObject.GetComponent<CanvasManager>();
        this.UIActivityList = canvas.LoadUIPrefab<UIActivityList>("MainGame/UIActivityList");
        canvas.AddUI(this.UIActivityList);
        this.UIActivityList.Close = this.CloseActivityList;
    }

    /// <summary>
    /// Đóng khung danh sách hoạt động
    /// </summary>
    public void CloseActivityList()
    {
        /// Nếu đang mở khung
        if (this.UIActivityList != null)
        {
            GameObject.Destroy(this.UIActivityList.gameObject);
            this.UIActivityList = null;
        }
    }
    #endregion
}
