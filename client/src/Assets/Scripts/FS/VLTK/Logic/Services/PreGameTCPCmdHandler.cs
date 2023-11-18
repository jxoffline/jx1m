using FS.GameEngine.Logic;
using FS.GameEngine.Network;
using FS.GameEngine.Network.Protocol;
using FS.GameEngine.Scene;
using FS.GameFramework.Logic;
using Server.Data;
using Server.Tools;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using HSGameEngine.GameEngine.Network;
using HSGameEngine.GameEngine.Network.Protocol;
using HSGameEngine.GameEngine.Network.Tools;
using FS.VLTK.Logic.Settings;

namespace FS.GameFramework.Controls
{
    /// <summary>
    /// Quản lý gói tin trước khi vào Game (chọn nhân vật, etc...)
    /// </summary>
    public class PreGameTCPCmdHandler : TTMonoBehaviour
    {
        #region Singleton - Instance
        /// <summary>
        /// Quản lý gói tin trước khi vào Game (chọn nhân vật, etc...)
        /// </summary>
        public static PreGameTCPCmdHandler Instance { get; private set; }
        #endregion

        #region Private fields
        /// <summary>
        /// Thời gian của Socket hiện tại
        /// </summary>
        private long currentPingTicks = 0;

        /// <summary>
        /// Thời gian tồn tại của Socket
        /// </summary>
        private const long TimeOut = 120;

        /// <summary>
        /// Socket
        /// </summary>
        private TCPClient tcpClient;

        /// <summary>
        /// Luồng kiểm tra trạng thái gói tin
        /// </summary>
        private Coroutine checkSocketStateCoroutine = null;
        #endregion

        #region Properties
        /// <summary>
        /// Sự kiện khi ngắt kết nối
        /// </summary>
        public Action<bool> SocketClosed { get; set; }

        /// <summary>
        /// Sự kiện khi người chơi kết nối
        /// </summary>
        public Action UserConnected { get; set; }

        /// <summary>
        /// Sự kiện khi kết nối thành công
        /// </summary>
        public Action SocketSuccess { get; set; }

        /// <summary>
        /// Sự kiện vào Game với nhân vật vừa tạo
        /// </summary>
        public Action<SocketConnectEventArgs> EnterGameWithNewCreatedRole { get; set; }

        /// <summary>
        /// Sự kiện tạo nhân vật
        /// </summary>
        public Action CreateRole { get; set; }

        /// <summary>
        /// Sự kiện vào Game với nhân vật hiện tại
        /// </summary>
        public Action EnterGame { get; set; }

        /// <summary>
        /// Sự kiện khi gặp lỗi ở packet INIT_GAME
        /// </summary>
        public Action InitGameError { get; set; }

        /// <summary>
        /// Sự kiện vào game ở packet CMD_SYNS_TIME
        /// </summary>
        public Action SynsTimeEnterGame { get; set; }
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi khi đối tượng được tạo ra
        /// </summary>
        private void Awake()
        {
            PreGameTCPCmdHandler.Instance = this;
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Luồng kiểm tra trạng thái Socket
        /// </summary>
        /// <returns></returns>
        private IEnumerator CheckSocketStatus()
        {
            this.currentPingTicks = TimeManager.GetCorrectLocalTime();

            while (true)
            {
                yield return new WaitForSeconds(PreGameTCPCmdHandler.TimeOut);
                if ((TimeManager.GetCorrectLocalTime() - this.currentPingTicks) / 1000 > PreGameTCPCmdHandler.TimeOut)
                {
                    this.CloseSocket(true);
                    this.checkSocketStateCoroutine = null;
                    yield break;
                }

                /// Send bừa lên để kick Client và đóng Socket
				GameInstance.Game.SpriteHeart();
            }
        }

        /// <summary>
        /// Luông thực hiện kết nối lại
        /// </summary>
        /// <param name="waitTime"></param>
        /// <returns></returns>
        private IEnumerator Reconnect(float waitTime)
        {
            yield return new WaitForSeconds(waitTime);
            this.CloseSocket(false);
            this.NewTCPGame();
        }

        /// <summary>
        /// Khởi tạo Socket
        /// </summary>
        private void InitTCPClient()
        {
            string loginIP = Global.GetRootParam("serverip", "127.0.0.1");
            this.tcpClient.SocketConnect += this.SocketConnect;
            this.tcpClient.Connect(loginIP, Global.GetUserLoginPort());
            Super.ShowNetWaiting("Đang kết nối tới máy chủ...");
        }

        /// <summary>
        /// Khởi tạo Socket
        /// </summary>
        private void InitTCPClient(string ip, int port)
        {
            this.tcpClient.SocketConnect += this.SocketConnect;
            this.tcpClient.Connect(ip, port);
            Super.ShowNetWaiting("Đang kết nối tới máy chủ...");
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Thực hiện kiểm tra trạng thái kết nối của gói tin
        /// </summary>
        public void StartCheckSocketStatus()
        {
            if (this.checkSocketStateCoroutine != null)
            {
                this.StopCoroutine(this.checkSocketStateCoroutine);
            }
            this.checkSocketStateCoroutine = this.StartCoroutine(this.CheckSocketStatus());
        }

        /// <summary>
        /// Tạo mới kết nối
        /// </summary>
        public void NewConnection()
        {
            this.tcpClient = new TCPClient(2);
        }

        /// <summary>
        /// Thực hiện kết nối lại trò chơi
        /// </summary>
        public void ReconnectGame()
        {
            if (Global.g_bReconnRoleManager)
            {
                string ip;
                int port;
                if (KuaFuLoginManager.LoginKuaFuServer(out ip, out port))
                {
                    this.InitTCPClient(ip, port);
                }
                else
                {
                    this.InitTCPClient();
                }
            }

            /// Thực hiện kiểm tra trạng thái gói tin
            PreGameTCPCmdHandler.Instance.StartCheckSocketStatus();
        }

        /// <summary>
        /// Đóng Socket hiện tại
        /// </summary>
        public void CloseSocket(bool showSelectServer)
        {
            GameInstance.Game.SocketFailed -= this.GameSocketFailed;
            GameInstance.Game.SocketSuccess -= this.GameSocketSuccess;
            GameInstance.Game.SocketCommand -= this.GameSocketCommand;

            /// Ngắt kết nối
            this.tcpClient?.Disconnect();

            /// Thực thi sự kiện ngắt kết nối
            this.SocketClosed?.Invoke(showSelectServer);
            /// Xóa dữ liệu Handler
            this.SocketClosed = null;
            this.UserConnected = null;
            this.SocketSuccess = null;
            this.EnterGame = null;
            this.CreateRole = null;
            this.InitGameError = null;
            this.SynsTimeEnterGame = null;

            this.tcpClient?.Destroy();
            this.tcpClient = null;
        }

        /// <summary>
        /// Bắt đầu xử lý gói tin
        /// </summary>
        public void BeginHandle()
        {
            GameInstance.Game.SocketFailed += this.GameSocketFailed;
            GameInstance.Game.SocketSuccess += this.GameSocketSuccess;
            GameInstance.Game.SocketCommand += this.GameSocketCommand;
        }

        /// <summary>
        /// Làm mới lại gói tin lấy thông tin nhân vật
        /// </summary>
        public void NewTCPGame()
        {
            if (Global.Data != null)
            {
                Global.CopyRoleData(Global.Data.RoleData);
            }
            GameInstance.Game = new TCPGame();
            Global.SetGameRoleData();
        }
        #endregion

        #region Core network
        /// <summary>
        /// Sự kiện khi gói tin đã kết nối với Server
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SocketConnect(object sender, SocketConnectEventArgs e)
        {
            MainGame.Instance.QueueOnMainThread(() =>
            {
                int netSocketType = e.NetSocketType;
                switch (netSocketType)
                {
                    case (int) NetSocketTypes.SOCKET_CONN:
                        if (e.Error == "Success")
                        {
                            String strcmd = "";
                            byte[] bytesCmd = null;
                            int loginCmdID = -1;
                            String uid = Global.StringReplaceAll(Global.GetRootParam("uid", ""), ":", "");
                            String name = Global.StringReplaceAll(Global.GetRootParam("n", ""), ":", "");
                            String lastTime = Global.StringReplaceAll(Global.GetRootParam("t", ""), ":", "");
                            String isadult = Global.StringReplaceAll(Global.GetRootParam("cm", ""), ":", "");
                            String token = Global.StringReplaceAll(Global.GetRootParam("token", ""), ":", "");
                            loginCmdID = (int) (TCPLoginServerCmds.CMD_LOGIN_ON2);
                            strcmd = string.Format("{0}:{1}:{2}:{3}:{4}:{5}", (int) (TCPCmdProtocolVer.VerSign), uid, name, lastTime, isadult, token);
                            bytesCmd = new UTF8Encoding().GetBytes(strcmd);
                            TCPOutPacket tcpOutPacket = new TCPOutPacket();
                            tcpOutPacket.PacketCmdID = (short) (loginCmdID);
                            tcpOutPacket.FinalWriteData(bytesCmd, 0, bytesCmd.Length);
                            this.tcpClient.SendData(tcpOutPacket);
                            Super.HideNetWaiting();
                        }
                        else
                        {
                            Super.HideNetWaiting();
                            Super.ShowMessageBox("Lỗi", "Không thể kết nối tới máy chủ.", true);
                        }
                        break;
                    case (int) NetSocketTypes.SOCKET_SEND:
                        Super.HideNetWaiting();
                        Super.ShowMessageBox("Lỗi", "Không thể gửi gói tin tới máy chủ.", true);
                        break;
                    case (int) NetSocketTypes.SOCKET_RECV:
                        break;
                    case (int) NetSocketTypes.SOCKET_CLOSE:
                        GScene.ServerStopGame();

                        Super.HideNetWaiting();
                        Super.ShowMessageBox("Lỗi", "Kết nối tới máy chủ bị gián đoạn.", true);
                        break;
                    case (int) NetSocketTypes.SOCKT_CMD:
                        this.tcpClient.Disconnect();
                        if ("-1" == e.fields[0])
                        {
                            Super.HideNetWaiting();
                            Super.ShowMessageBox("Lỗi", "Tài khoản hoặc mật khẩu không chính xác, hãy thử lại.", true);
                        }
                        else if ("-2" == e.fields[0])
                        {
                            Super.HideNetWaiting();
                            Super.ShowMessageBox("Lỗi", "Phiên bản Client hiện tại đã cũ, hãy tải bản mới nhất và thử lại.", true);
                        }
                        else if ("-100" == e.fields[0])
                        {
                            Super.HideNetWaiting();
                            Super.ShowMessageBox("Lỗi", "Phiên đăng nhập đã hết hạn, hãy thoát và tiến hành đăng nhập lại.", true);
                        }
                        else
                        {
                            GameInstance.Game.CurrentSession.UserID = e.fields[0];
                            GameInstance.Game.CurrentSession.UserName = e.fields[1];
                            GameInstance.Game.CurrentSession.UserToken = e.fields[2];
                            GameInstance.Game.CurrentSession.UserIsAdult = Convert.ToInt32(e.fields[3]);
                            Super.HideNetWaiting();
                            this.tcpClient.SocketConnect -= this.SocketConnect;
                            this.tcpClient.Destroy();
                            this.tcpClient = null;

                            this.UserConnected?.Invoke();

                        }
                        break;
                    default:
                        throw new Exception("Socket Exception");
                }
            });
        }

        /// <summary>
        /// Sự kiện kết nối với Game thất bại
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GameSocketFailed(object sender, SocketConnectEventArgs e)
        {
            int errorCode = 0;
            if (null != e && null != e.fields && e.fields.Length > 0)
            {
                errorCode = Convert.ToInt32(e.fields[0]);
            }

            MainGame.Instance.QueueOnMainThread(() =>
            {
                if (KuaFuLoginManager.DirectLogin())
                {
                    if (e.CmdID == (int) TCPGameServerCmds.CMD_LOGIN_ON)
                    {
                        if (-11007 == errorCode)
                        {
                            e.ShowMsgBox = false;
                            KuaFuLoginManager.ChangeToOriginalServer();

                            GameInstance.Game.SocketFailed -= this.GameSocketFailed;
                            GameInstance.Game.SocketSuccess -= this.GameSocketSuccess;
                            GameInstance.Game.SocketCommand -= this.GameSocketCommand;
                            GameInstance.Game.Disconnect();

                            this.StartCoroutine(this.Reconnect(2.5f));
                            return;
                        }
                        else if (-2 == errorCode)
                        {
                            e.ShowMsgBox = false;

                            GameInstance.Game.SocketFailed -= this.GameSocketFailed;
                            GameInstance.Game.SocketSuccess -= this.GameSocketSuccess;
                            GameInstance.Game.SocketCommand -= this.GameSocketCommand;
                            GameInstance.Game.Disconnect();

                            this.StartCoroutine(this.Reconnect(2.5f));
                            return;
                        }
                    }
                }

                Super.HideNetWaiting();

                GameInstance.Game.SocketFailed -= this.GameSocketFailed;
                GameInstance.Game.SocketSuccess -= this.GameSocketSuccess;
                GameInstance.Game.SocketCommand -= this.GameSocketCommand;
                GameInstance.Game.Disconnect();

                /// Nếu có lỗi
                if (!string.IsNullOrEmpty(e.ErrorMsg))
                {
                    Super.ShowMessageBox("Lỗi đăng nhập", e.ErrorMsg, () =>
                    {
                        this.InitGameError?.Invoke();
                    });
                }
                this.NewTCPGame();

                /// Nếu có RoleData
                if (Global.Data != null && Global.Data.RoleData != null)
                {
                    /// Thực hiện sự kiện vào Game
                    this.EnterGame?.Invoke();
                }
            });
        }

        /// <summary>
        /// Sự kiện kết nối với Game thành công
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GameSocketSuccess(object sender, object e)
        {
            MainGame.Instance.QueueOnMainThread(() =>
            {
                this.SocketSuccess?.Invoke();
            });
        }

        /// <summary>
        /// Sự kiện giao tiếp với kết nối Socket Game
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GameSocketCommand(object sender, SocketConnectEventArgs e)
        {
            MainGame.Instance.QueueOnMainThread(() =>
            {
                if (e.CmdID >= (int) (TCPGameServerCmds.CMD_ROLE_LIST) && e.CmdID <= (int) (TCPGameServerCmds.CMD_CREATE_ROLE))
                {
                    Super.HideNetWaiting();

                    if (e.fields.Length > 1 && Convert.ToInt32(e.fields[0]) > 0)
                    {
                        this.EnterGameWithNewCreatedRole?.Invoke(e);
                    }
                    else if (e.fields.Length > 1 && Convert.ToInt32(e.fields[0]) < 0)
                    {
                        int retCode = Convert.ToInt32(e.fields[0]);
                        string op = "";
                        if (e.CmdID == (int) (TCPGameServerCmds.CMD_ROLE_LIST))
                        {
                            op = "Không thể tải danh sách nhân vật.";

                            if (-1 == retCode)
                            {
                                this.InitGameError?.Invoke();
                            }
                            else if (-2 == retCode)
                            {
                                Super.ShowMessageBox("Máy chủ hiện tại đã đầy, không thể thao tác được. Hãy chọn một máy chủ khác.", () => {
                                    this.InitGameError?.Invoke();
                                });
                            }

                            return;
                        }
                        else
                        {
                            if (retCode == -1)
                            {
                                op = "Không thể tạo nhân vật, hãy thử lại sau.";
                            }
                            else if (retCode == -2)
                            {
                                op = "Máy chủ hiện tại đã đầy, không thể thao tác được. Hãy chọn một máy chủ khác.";
                            }
                            else if (retCode == -3)
                            {
                                op = "Tên nhân vật có chứa ký tự đặc biệt, hãy chọn một tên khác.";
                            }
                            else if (retCode == -4)
                            {
                                op = "Tên nhân vật đã tồn tại, hãy chọn một tên khác và thử lại.";
                            }
                            else if (retCode == -7)
                            {
                                string str = e.fields[1];
                                int deltaTicks = int.Parse(str);
                                if (deltaTicks > 0)
                                {
                                    op = string.Format("Thao tác quá nhanh, vui lòng đợi {0}s và tiến hành thử lại.", deltaTicks / 1000);
                                }
                                else
                                {
                                    op = "Thao tác quá nhanh, hãy thử lại sau giây lát.";
                                }
                            }
                            else if (retCode == -1000)
                            {
                                op = "Số lượng nhân vật đã đạt ngưỡng tối đa, không thể tạo thêm!";
                            }
                            else
                            {
                                op = "Lỗi chưa xác định.";
                            }
                        }
                        Super.ShowMessageBox("Lỗi thao tác", op, true);
                        return;
                    }
                    else
                    {
                        if (e.CmdID == (int) (TCPGameServerCmds.CMD_ROLE_LIST))
                        {
                            this.CreateRole?.Invoke();
                        }
                    }
                }
                else if (e.CmdID == (int) (TCPGameServerCmds.CMD_INIT_GAME))
                {
                    RoleData roleData = DataHelper.BytesToObject<RoleData>(e.bytesData, 0, e.bytesData.Length);

                    if (null == roleData)
                    {
                        Super.ShowMessageBox("Lỗi", "Không thể khởi tạo dữ liệu đăng nhập hệ thống.", () => {
                            this.InitGameError?.Invoke();
                        });
                        return;
                    }
                    if (roleData.RoleID < 0)
                    {
                        string op = "";
                        if (roleData.RoleID == -1)
						{
                            op = "Số lượng tài khoản cho phép trên thiết bị đã đạt ngưỡng tối đa!";
						}
                        else if (roleData.RoleID == -2)
						{
                            op = "Máy chủ hiện đã đầy, hãy chọn một máy chủ khác và tiến hành đăng nhập lại!";
                        }
                        else if (-10 == roleData.RoleID)
                        {
                            op = "Tài khoản đã bị khóa bởi Admin!";
                        }
                        else if (-20 == roleData.RoleID)
                        {
                            op = string.Format("Hệ thống phát hiện hành vi phi pháp của tài khoản. Tạm thời bị khóa trong {0} phút.", (int) ((double) roleData.TotalValue / 60.0f));
                        }
                        else if (-30 == roleData.RoleID)
                        {
                            KuaFuLoginManager.ClearLoginInfo();
                            this.CloseSocket(true);
                            return;
                        }
                        else if (-50 == roleData.RoleID)
                        {
                            op = string.Format("Hệ thống phát hiện hành vi phi pháp của tài khoản. Tạm thời bị khóa trong {0} phút.", (int) ((double) roleData.TotalValue / 60.0f));
                        }
                        else if (-60 == roleData.RoleID)
                        {
                            Super.HideNetWaiting();
                            this.CloseSocket(true);

                            return;
                        }
                        else if (-40 == roleData.RoleID)
                        {
                            op = "-40";
                            return;
                        }
                        else if (-80 == roleData.RoleID)
                        {
                            op = string.Format("Hệ thống phát hiện hành vi phi pháp của tài khoản. Tạm thời bị khóa trong {0} phút.", (int) ((double) roleData.TotalValue / 60.0f));
                        }
                        else
                        {
                            op = "Không thể khởi tạo dữ liệu đăng nhập hệ thống.";
                        }
                        Super.ShowMessageBox("Lỗi", op, () => {
                            this.InitGameError?.Invoke();
                        });
                        return;
                    }
                    else
                    {
                        KuaFuLoginManager.OnChangeServerComplete();
                        GameInstance.Game.CurrentSession.roleData = roleData;
                        //Xử lý thiết lập của auto
                        KTAutoAttackSetting.SetAutoConfig();

                        /// Bắt đầu Game
                        this.SynsTimeEnterGame?.Invoke();
                        Super.HideNetWaiting();
                    }
                }
                else if ((int) TCPGameServerCmds.CMD_SPR_LOGIN_WAITING_INFO == e.CmdID)
                {
                    if (e.fields.Length == 2)
                    {
                        return;
                    }

                    int toWaitFor = Convert.ToInt32(e.fields[0]);
                    int leftSeconds = Convert.ToInt32(e.fields[1]);
                }
                else if ((int) TCPGameServerCmds.CMD_SPR_CLIENTHEART == e.CmdID)
                {
                    currentPingTicks = TimeManager.GetCorrectLocalTime();
                }
            });
        }
        #endregion
    }
}
