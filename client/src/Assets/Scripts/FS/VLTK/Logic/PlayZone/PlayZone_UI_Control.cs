using FS.GameEngine.Logic;
using FS.GameEngine.Network;
using FS.GameFramework.Controls;
using FS.GameFramework.Logic;
using FS.VLTK;
using FS.VLTK.Control.Component;
using FS.VLTK.Logic;
using FS.VLTK.Network;
using FS.VLTK.Network.Skill;
using FS.VLTK.UI;
using FS.VLTK.UI.Main;
using FS.VLTK.UI.Main.MainUI;
using FS.VLTK.UI.Main.SytemNotification;
using Server.Data;
using System.Linq;
using UnityEngine;
using static FS.VLTK.Entities.Enum;

/// <summary>
/// Quản lý các khung giao diện trong màn chơi
/// </summary>
public partial class PlayZone
{
    #region Top Function Buttons
    /// <summary>
    /// Khung danh sách Button sự kiện phía trên
    /// </summary>
    public UITopFunctionButtons UITopFunctionButtons { get; protected set; }

    /// <summary>
    /// Khởi tạo khung danh sách Button sự kiện phía trên
    /// </summary>
    protected void InitTopFunctionButtons()
    {
        if (this.UITopFunctionButtons != null)
        {
            return;
        }
        CanvasManager canvas = Global.MainCanvas.GetComponent<CanvasManager>();
        this.UITopFunctionButtons = canvas.LoadUIPrefab<UITopFunctionButtons>("MainGame/MainUI/UITopFunctionButtons");
        canvas.AddMainUI(this.UITopFunctionButtons);

        this.UITopFunctionButtons.OpenTokenShop = () => {
            KT_TCPHandler.SendOpenTokenShop();
        };
        this.UITopFunctionButtons.OpenLuckyCircle = () => {
            /// Gửi gói tin yêu cầu mở Vòng quay may mắn
            KT_TCPHandler.SendOpenLuckyCircle();
        };
        this.UITopFunctionButtons.OpenWelfareFirstRecharge = () => {
            this.OpenUIWelfare(true);
        };
        this.UITopFunctionButtons.OpenWelfare = () => {
            this.OpenUIWelfare();
        };
        this.UITopFunctionButtons.OpenActivityList = () => {
            this.OpenActivityList();
        };
        this.UITopFunctionButtons.OpenRanking = () => {
            this.ShowUIRanking();
        };
        this.UITopFunctionButtons.OpenTopRanking = () => {
            KT_TCPHandler.SendGetTopRankingInfo();
        };
    }
    #endregion

    #region Button Bar
    /// <summary>
    /// Khung phía dưới gồm SkillBar và ControlButtons
    /// </summary>
    public UIBottomBar UIBottomBar { get; protected set; }

    /// <summary>
    /// Khởi tạo BottomBar
    /// </summary>
    protected void InitBottomBar()
    {
        if (this.UIBottomBar != null)
        {
            return;
        }
        CanvasManager canvas = Global.MainCanvas.GetComponent<CanvasManager>();
        this.UIBottomBar = canvas.LoadUIPrefab<UIBottomBar>("MainGame/MainUI/UIBottomBar");
        canvas.AddMainUI(this.UIBottomBar);

        this.InitSkillBar();
        this.InitControlButtons();

        this.UIBottomBar.UISkillBarVisible = () => {
            /// Nếu là GM thì cho hiện Button nhập lệnh GM
            this.UIBottomBar.UIControlButtons.ShowButtonGMCommand = Global.Data.RoleData.GMAuth == 1;

            this.UIBottomBar.UISkillBar.RefreshSkillIcon();
        };
        this.UIBottomBar.UIControlButtonVisible = () => {
            /// Nếu là GM thì cho hiện Button nhập lệnh GM
            this.UIBottomBar.UIControlButtons.ShowButtonGMCommand = Global.Data.RoleData.GMAuth == 1;
        };
        this.UIBottomBar.ShowNearbyPlayer = () => {
            /// Nếu đang hiện khung
            if (this.UINearbyPlayer.Visible)
            {
                /// Đóng khung
                this.HideUINearbyPlayers();
                /// Hiện khung MiniTask và EventBroadboard
                this.UIMiniTaskAndTeamFrame.gameObject.SetActive(!Global.Data.ShowUIMiniEventBroadboard);
                this.UIEventBroadboardMini.gameObject.SetActive(Global.Data.ShowUIMiniEventBroadboard);
            }
            /// Nếu đang ẩn khung
            else
            {
                /// Hiện khung
                this.ShowUINearbyPlayers();
                /// Đóng khung MiniTask và EventBroadboard
                this.UIMiniTaskAndTeamFrame.gameObject.SetActive(false);
                this.UIEventBroadboardMini.gameObject.SetActive(false);
            }
        };
        this.UIBottomBar.ShowUISkillBar(true);
    }

    /// <summary>
    /// Khởi tạo SkillBar
    /// </summary>
    protected void InitSkillBar()
    {
        if (this.UIBottomBar == null)
        {
            return;
        }

        this.UIBottomBar.UISkillBar.RefreshSkillIcon();
        this.UIBottomBar.UISkillBar.RefreshAruaIcon();
        this.UIBottomBar.UISkillBar.SkillButtonClicked = (skillID, ignoreTarget) => {
            /// Nếu kỹ năng ID -1 thì bỏ qua
            if (skillID == -1)
            {
                return;
            }

            /// Nếu là kỹ năng tay trái thì sẽ cho tìm mục tiêu, kỹ năng tay phải thì sẽ đánh theo hướng
            SkillManager.LeaderUseSkill(skillID, this.UIBottomBar.UISkillBar.ShowMainSkill, ignoreTarget);
        };
        this.UIBottomBar.UISkillBar.AruaButtonClicked = () => {
            KTTCPSkillManager.SendSaveAndActivateAruaKey(this.UIBottomBar.UISkillBar.AruaSkillID, this.UIBottomBar.UISkillBar.ActivateArua ? 1 : 0);
        };
        this.UIBottomBar.UISkillBar.JumpClicked = () => {
            SkillData skillFly = Global.Data.RoleData.SkillDataList.Where(x => x.SkillID == KTGlobal.FlySkillID).FirstOrDefault();
            if (skillFly == null || skillFly.Level <= 0)
            {
                KTGlobal.AddNotification("Bạn chưa học kỹ năng khinh công!");
            }
            else if (Global.Data.Leader.CanDoLogic)
            {
                KTLeaderMovingManager.StopMove(true, false);
                KTLeaderMovingManager.StopChasingTarget();
                /// Nếu đang chạy thì ngừng lại
                if (Global.Data.Leader.IsMoving)
                {
                    Global.Data.Leader.DoStand(KTGlobal.RunningWaitToUseSkill, () => {
                        KTTCPSkillManager.SendUseSkill(skillFly.SkillID);
                    });
                }
                /// Nếu không chạy thì cho dùng kỹ năng luôn
                else
                {
                    KTTCPSkillManager.SendUseSkill(skillFly.SkillID);
                }
            }
        };
        this.UIBottomBar.UISkillBar.SitClicked = () => {
            if (Global.Data.Leader.CanDoLogic)
            {
                KTLeaderMovingManager.StopMoveImmediately(true, false);

                /// Nếu đang ngồi thì bỏ qua
                if (Global.Data.Leader.CurrentAction == KE_NPC_DOING.do_sit)
                {
                    return;
                }
                /// Nếu đang bày bán thì bỏ qua
                else if (Global.Data.StallDataItem != null && Global.Data.StallDataItem.Start == 1 && !Global.Data.StallDataItem.IsBot)
                {
                    return;
                }
                /// Nếu đang cưỡi thì bỏ qua
                else if (Global.Data.RoleData.IsRiding)
                {
                    KTGlobal.AddNotification("Đang trong trạng thái cưỡi, không thể thực hiện ngồi!");
                    return;
                }

                /// Nếu chưa hết thời gian dùng kỹ năng
                if (!KTGlobal.FinishedUseSkillAction)
                {
                    return;
                }

                /// Gửi gói tìn về GS
                KTTCPSkillManager.SendLeaderChangeAction(KE_NPC_DOING.do_sit);
            }
        };
        this.UIBottomBar.UISkillBar.ToggleMountClicked = () => {
            /// Nếu đang bị khống chế
            if (!Global.Data.Leader.CanDoLogic)
            {
                KTGlobal.AddNotification("Bạn đang bị khống chế, không thể thay đổi trạng thái cưỡi!");
                return;
            }

            /// Thông tin ngựa tương ứng
            GoodsData horseGD = Global.Data.RoleData.GoodsDataList?.Where(x => x.Using == (int) KE_EQUIP_POSITION.emEQUIPPOS_HORSE).FirstOrDefault();
            /// Nếu không có ngựa
            if (horseGD == null)
            {
                KTGlobal.AddNotification("Bạn chưa trang bị ngựa cưỡi!");
                return;
            }

            /// Gửi sự kiện thay đổi trạng thái cưỡi ngựa lên Server
            KT_TCPHandler.SendChangeToggleHorseState();
        };
        this.UIBottomBar.UISkillBar.OpenAutoSetting = () => {
            this.OpenUIAutoFight();
        };
        this.UIBottomBar.UISkillBar.LockJoyStickMove = (isSelected) => {
            Global.Data.LockJoyStickMove = isSelected;

            if (isSelected)
            {
                KTGlobal.AddNotification("Mở chức năng dùng JoyStick chỉ đổi hướng nhân vật.");
            }
            else
            {
                KTGlobal.AddNotification("Tắt chức năng dùng JoyStick chỉ đổi hướng nhân vật.");
            }
        };
        this.UIBottomBar.UISkillBar.SetSkillTargetPos = (position) =>
        {
            Global.Data.GameScene.SetSkillMarkTargetPos(position);
        };
    }

    /// <summary>
    /// Khởi tạo ControlButtons
    /// </summary>
    protected void InitControlButtons()
    {
        if (this.UIBottomBar == null)
        {
            return;
        }

        this.UIBottomBar.UIControlButtons.OpenRoleAttributes = () => {
            KT_TCPHandler.SendGetRoleAttributes();
        };
        this.UIBottomBar.UIControlButtons.OpenBag = () => {
            this.ShowUIBag();
        };
        this.UIBottomBar.UIControlButtons.OpenSkillTree = () => {
            this.ShowUISkillTree();
        };
        this.UIBottomBar.UIControlButtons.OpenQuestManager = () => {
            this.ShowUITaskBox();
        };
        this.UIBottomBar.UIControlButtons.OpenFriendsFrame = () => {
            GameInstance.Game.SpriteGetFriends();
        };
        this.UIBottomBar.UIControlButtons.OpenGuildManager = () => {
            /// Nếu chưa có bang hội
            if (Global.Data.RoleData.GuildID <= 0)
            {
                /// Truy vấn danh sách bang hội
                KT_TCPHandler.SendGetGuildList();
            }
            /// Nếu đã có bang hội
            else
            {
                /// Gửi yêu cầu truy vấn thông tin bang hội
                KT_TCPHandler.SendGetGuildInfo();
            }
        };
        this.UIBottomBar.UIControlButtons.OpenSystemSetting = () => {
            this.ShowSystemSetting();
        };
        this.UIBottomBar.UIControlButtons.OpenGMCommand = () => {
            this.ShowGMCommand();
        };
        this.UIBottomBar.UIControlButtons.OpenLifeSkill = () => {
            this.ShowUICrafting();
        };
        this.UIBottomBar.UIControlButtons.OpenBrowsePlayer = () => {
            this.ShowUIBrowsePlayer();
        };
        this.UIBottomBar.UIControlButtons.OpenMailBox = () => {
            /// Gửi gói tin lên Server lấy danh sách thư
            GameInstance.Game.SpriteGetUserMailList();
        };
        this.UIBottomBar.UIControlButtons.OpenPet = () =>
        {
            /// Gửi gói tin lên Server lấy danh sách pet
            KT_TCPHandler.SendGetPetList();
        };
    }
    #endregion

    #region Main Buttons
    /// <summary>
    /// Khung danh sách Button ở MainUI
    /// </summary>
    public UIMainButtons UIMainButtons { get; protected set; }

    /// <summary>
    /// Khởi tạo khung danh sách Button sự kiện phía trên
    /// </summary>
    protected void InitUIMainButtons()
    {
        if (this.UIMainButtons != null)
        {
            return;
        }
        CanvasManager canvas = Global.MainCanvas.GetComponent<CanvasManager>();
        this.UIMainButtons = canvas.LoadUIPrefab<UIMainButtons>("MainGame/MainUI/UIMainButtons");
        canvas.AddMainUI(this.UIMainButtons);

        /// Kích hoạt Auto
        this.UIMainButtons.OnAutoActivated = (isActivated) => {
            if (isActivated)
            {
                KTAutoFightManager.Instance.StopAutoFight();
                KTAutoFightManager.Instance.StartAuto();
                AutoPathManager.Instance.StopAutoPath();
                AutoQuest.Instance.StopAutoQuest();
            }
            else
            {
                KTAutoFightManager.Instance.StopAutoFight();
                AutoPathManager.Instance.StopAutoPath();
                AutoQuest.Instance.StopAutoQuest();
            }
        };
    }
    #endregion

    #region JoyStick
    /// <summary>
    /// JoyStick
    /// </summary>
    public Joystick UIJoyStick { get; protected set; }

    /// <summary>
    /// Hiển thị JoyStick
    /// </summary>
    protected void InitJoyStick()
    {
        CanvasManager canvas = Global.MainCanvas.GetComponent<CanvasManager>();
        this.UIJoyStick = canvas.LoadUIPrefab<Joystick>("MainGame/MainUI/UIJoyStick");
        canvas.AddMainUI(this.UIJoyStick);
    }
    #endregion

    #region Progress Bar
    /// <summary>
    /// Thanh Progress Bar
    /// </summary>
    protected UIProgressBar UIProgressBar = null;

    /// <summary>
    /// Khởi tạo thanh Progress Bar
    /// </summary>
    protected void InitProgressBar()
    {
        CanvasManager canvas = Global.MainCanvas.GetComponent<CanvasManager>();
        this.UIProgressBar = canvas.LoadUIPrefab<UIProgressBar>("MainGame/MainUI/UIProgressBar");
        canvas.AddUI(this.UIProgressBar, true);
    }

    /// <summary>
    /// Có đang hiển thị thanh Progress Bar không
    /// </summary>
    public bool IsProgressBarVisible()
    {
        return this.UIProgressBar != null && this.UIProgressBar.Visible;
    }

    /// <summary>
    /// Hiển thị thanh Progress Bar với điểm bắt đầu và thời gian tồn tại tương ứng
    /// </summary>
    /// <param name="currentLifeTime"></param>
    /// <param name="duration"></param>
    public void ShowProgressBar(float currentLifeTime, float duration, string hint)
    {
        if (this.UIProgressBar == null)
        {
            return;
        }

        this.UIProgressBar.Text = hint;
        this.UIProgressBar.CurrentLifeTime = currentLifeTime;
        this.UIProgressBar.Duration = duration;
        this.UIProgressBar.Complete = this.HideProgressBar;
        this.UIProgressBar.StartProgress();
    }

    /// <summary>
    /// Hiển thị thanh Progress Bar thời gian tồn tại tương ứng
    /// </summary>
    /// <param name="duration"></param>
    public void ShowProgressBar(float duration, string hint)
    {
        this.ShowProgressBar(0, duration, hint);
    }

    /// <summary>
    /// Đóng Progress Bar
    /// </summary>
    public void HideProgressBar()
    {
        if (this.UIProgressBar != null && this.UIProgressBar.Visible)
        {
            /// Đóng Progress Bar
            this.UIProgressBar.CloseProgress();
        }
    }
    #endregion

    #region Decoration Text
    /// <summary>
    /// Text tự tìm đường
    /// </summary>
    public UIDecorationText UITextAutoFindPath { get; protected set; }

    /// <summary>
    /// Text tự động đánh
    /// </summary>
    public UIDecorationText UITextAutoAttack { get; protected set; }

    /// <summary>
    /// Khởi tạo Text trang trí
    /// </summary>
    protected void InitDecorationText()
    {
        CanvasManager canvas = Global.MainCanvas.GetComponent<CanvasManager>();
        this.UITextAutoFindPath = canvas.LoadUIPrefab<UIDecorationText>("MainGame/MainUI/UITextAutoFindPath");
        canvas.AddUnderLayerUI(this.UITextAutoFindPath);
        this.UITextAutoFindPath.Hide();
        this.UITextAutoAttack = canvas.LoadUIPrefab<UIDecorationText>("MainGame/MainUI/UITextAutoAttack");
        canvas.AddUnderLayerUI(this.UITextAutoAttack);
        this.UITextAutoAttack.Hide();
    }

    /// <summary>
    /// Hiển thị Text tự tìm đường
    /// </summary>
    public void ShowTextAutoFindPath()
    {
        this.UITextAutoFindPath.Show();
    }

    /// <summary>
    /// Ẩn Text tự động đánh
    /// </summary>
    public void HideTextAutoFindPath()
    {
        this.UITextAutoFindPath.Hide();
    }

    /// <summary>
    /// Hiển thị Text tự động đánh
    /// </summary>
    public void ShowTextAutoAttack()
    {
        this.UITextAutoAttack.Show();
    }

    /// <summary>
    /// Ẩn Text tự động đánh
    /// </summary>
    public void HideTextAutoAttack()
    {
        this.UITextAutoAttack.Hide();
    }
    #endregion

    #region System Notification
    /// <summary>
    /// Khung dòng chữ chạy phía trên
    /// </summary>
    private UISystemNotification UISystemNotification;

    /// <summary>
    /// Thêm tin vào khung dòng chữ chạy phía trên
    /// </summary>
    /// <param name="message"></param>
    public void AddSystemNotification(string message)
    {
        if (this.UISystemNotification == null)
        {
            CanvasManager canvas = Global.MainCanvas.GetComponent<CanvasManager>();
            this.UISystemNotification = canvas.UISystemNotification;
        }

        this.UISystemNotification.AddMessage(message);
    }
    #endregion

    #region GM Command
    /// <summary>
    /// Khung nhập lệnh GM
    /// </summary>
    public UIGMCommand UIGMCommand { get; protected set; }

    /// <summary>
    /// Hiện khung nhập lệnh GM
    /// </summary>
    public void ShowGMCommand()
    {
        /// Nếu không phải GM
        if (Global.Data.RoleData.GMAuth == 0)
        {
            return;
        }

        /// Nếu khung chưa tồn tại thì tạo mới
        if (this.UIGMCommand == null)
        {
            CanvasManager canvas = Global.MainCanvas.GetComponent<CanvasManager>();
            this.UIGMCommand = canvas.LoadUIPrefab<UIGMCommand>("MainGame/UIGMCommand");
            canvas.AddUI(this.UIGMCommand);
            this.UIGMCommand.ProcessGMCommand = (command) => {
                if (Global.Data.RoleData.GMAuth == 0)
                {
                    return;
                }

                KT_TCPHandler.SendGMCommand(command);
            };
            this.UIGMCommand.Close = () => {
                this.CloseGMCommand();
            };
        }

        /// Hiển thị khung
        if (!this.UIGMCommand.Visible)
        {
            this.UIGMCommand.Visible = true;
        }
    }

    /// <summary>
    /// Đóng khung nhập lệnh GM
    /// </summary>
    public void CloseGMCommand()
    {
        if (this.UIGMCommand != null && this.UIGMCommand.Visible)
        {
            this.UIGMCommand.Visible = false;
        }
    }
    #endregion

    #region Kết nối lại
    /// <summary>
    /// Khung mất kết nối
    /// </summary>
    protected UIDisconnected UIDisconnected = null;

    /// <summary>
    /// Hiển thị bảng kết nối lại
    /// </summary>
    private void ShowUIDisconnected()
    {
        if (this.UIDisconnected != null)
        {
            this.UIDisconnected.DurationTick = KTGlobal.AutoQuitGameWhenDisconnectedAfter;
            this.UIDisconnected.RefreshCountDown();
            return;
        }

        CanvasManager canvas = Global.MainCanvas.gameObject.GetComponent<CanvasManager>();
        this.UIDisconnected = canvas.LoadUIPrefab<UIDisconnected>("MainGame/UIDisconnected");
        canvas.AddUI(this.UIDisconnected);
        this.UIDisconnected.DurationTick = KTGlobal.AutoQuitGameWhenDisconnectedAfter;
        this.UIDisconnected.Reconnect = () => {
            this.ReLoadGame(3);
        };
        this.UIDisconnected.QuitGame = () => {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        };
    }

    /// <summary>
    /// Đóng khung mất kết nối
    /// </summary>
    private void CloseUIDisconnected()
    {
        if (this.UIDisconnected != null)
        {
            GameObject.Destroy(this.UIDisconnected.gameObject);
            this.UIDisconnected = null;
        }
    }
    #endregion

    #region Core Network
    /// <summary>
    /// Làm mới lại Game
    /// </summary>
    /// <param name="nType"></param>
    private void ReLoadGame(int nType)
    {
        if (null != this.scene)
        {
            if (5 != nType)
            {
                KTLeaderMovingManager.StopMove();
                KTLeaderMovingManager.StopChasingTarget();
            }

            this.ReLoadGameEx(nType);
        }
        else
        {
            this.ReLoadGameEx(nType);
        }
    }

    /// <summary>
    /// Thực hiện tải mới lại Game
    /// </summary>
    /// <param name="nType"></param>
    private void ReLoadGameEx(int nType)
    {
        /// Đóng khung vận tiêu
        PlayZone.Instance.CloseUICargoCarriageTaskInfo();

        if (0 == nType)
        {
            KuaFuLoginManager.ClearLoginInfo();
            this.CloseSocket();
            this.ClearGameData();
        }
        /// Trở về Map thường từ Liên Server
        else if (1 == nType)
        {
            Global.g_bReconnRoleManager = true;
            this.CloseSocket();
            this.ClearGameData();

            KuaFuLoginManager.ClearLoginInfo();
            PreGameTCPCmdHandler.Instance.ReconnectGame();
        }
        /// Kết nối lại khi bị ngắt kết nối
        else if (3 == nType)
        {
            this.CloseUIDisconnected();

            Super.ShowNetWaiting("Đang kết nối lại với máy chủ...");

            GameInstance.Game.Disconnect();
            GameInstance.Game.ResetGameClient();
            this.ClearGameData();
            this.LoadScene(Global.Data.RoleData.MapCode, Global.Data.RoleData.PosX, Global.Data.RoleData.PosY, 0);
            GameInstance.Game.Connect(Global.CurrentListData.GameServerIP, Global.CurrentListData.GameServerPort, true);
        }
        else if (4 == nType)
        {
            this.CloseSocket();
            this.ClearGameData();

            KuaFuLoginManager.ClearLoginInfo();
        }
        /// Login liên Server
        else if ((int) (ReConnectType.ChangeServer) == nType)
        {
            Global.g_bReconnRoleManager = true;

            RoleData lastData = Global.Data.RoleData;

            this.CloseSocket();
            this.ClearGameData();

            /// Tạo mới kết nối
            PreGameTCPCmdHandler.Instance.NewConnection();
            PreGameTCPCmdHandler.Instance.ReconnectGame();
            PreGameTCPCmdHandler.Instance.BeginHandle();
            if (null != Global.CurrentListData)
            {
                if (KuaFuLoginManager.LoginKuaFuServer(out string ip, out int port))
                {
                    GameInstance.Game.Connect(ip, port);
                }
                else
                {
                    GameInstance.Game.Connect(Global.CurrentListData.GameServerIP, Global.CurrentListData.GameServerPort);
                }
            }
            PreGameTCPCmdHandler.Instance.SynsTimeEnterGame = () => {
                Super.HideNetWaiting();

                this.ClearScene();
                this.ShowLoadingMap(() => {
                    this.EnterGameForCrossServer();
                });
            };
            PreGameTCPCmdHandler.Instance.UserConnected = () => {
                if (GameInstance.Game.ConnectedState)
                {
                    PreGameTCPCmdHandler.Instance.NewTCPGame();
                }

                /// Thiết lập Session
                GameInstance.Game.CurrentSession.RoleID = lastData.RoleID;
                GameInstance.Game.CurrentSession.RoleSex = lastData.RoleSex;
                GameInstance.Game.CurrentSession.RoleName = lastData.RoleName;

                PreGameTCPCmdHandler.Instance.BeginHandle();

                if (null != Global.CurrentListData)
                {
                    if (KuaFuLoginManager.LoginKuaFuServer(out string ip, out int port))
                    {
                        GameInstance.Game.Connect(ip, port);
                    }
                    else
                    {
                        GameInstance.Game.Connect(Global.CurrentListData.GameServerIP, Global.CurrentListData.GameServerPort);
                    }
                }
            };
            PreGameTCPCmdHandler.Instance.SocketSuccess = () => {
                Super.HideNetWaiting();
                /// Gửi CMD_INIT_GAME lên Server
                GameInstance.Game.InitPlayGame();
            };
            PreGameTCPCmdHandler.Instance.SocketClosed = (showSelectServer) => {

            };
        }
    }

    /// <summary>
    /// Đóng toàn bộ Socket
    /// </summary>
    public void CloseSocket()
    {
        GameInstance.Game.Disconnect();
        GameInstance.Game.SocketFailed -= GameSocketFailed;
        GameInstance.Game.SocketSuccess -= GameSocketSuccess;
        GameInstance.Game.SocketCommand -= GameSocketCommand;
        if (Global.Data != null)
        {
            Global.CopyRoleData(Global.Data.RoleData);
        }
        GameInstance.Game = new TCPGame();
        Global.SetGameRoleData();
    }

    /// <summary>
    /// Xóa dữ liệu Game
    /// </summary>
    /// <param name="nClearType"></param>
    private void ClearGameData()
    {
        if (null != this.uiTimer)
        {
            this.StopCoroutine(this.uiTimer);
            this.uiTimer = null;
        }

        if (null != Global.Data.RoleData)
        {
            if (null != Global.Data.RoleData.GoodsDataList)
            {
                Global.Data.RoleData.GoodsDataList.Clear();
            }
        }

        if (Global.Data.OtherRoles != null)
        {
            Global.Data.OtherRoles.Clear();
        }

        this.CloseReviveFrame();
        this.ClearScene();
    }
    #endregion

    #region Thông báo tài nguyên
    /// <summary>
    /// Mở bảng thông báo pin yếu
    /// </summary>
    public void OpenLowPowerPart()
    {
        KTDebug.LogError("Hiển thị thông báo pin yếu");
    }

    /// <summary>
    /// Đóng bảng thông báo pin yếu
    /// </summary>
    public void CloseLowPower()
    {

    }
    #endregion
}
