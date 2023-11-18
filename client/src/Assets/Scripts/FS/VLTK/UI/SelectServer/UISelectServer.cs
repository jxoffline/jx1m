using FS.VLTK.Utilities.UnityUI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Server.Data;
using FS.GameEngine.Logic;
using UnityEngine.Networking;
using FS.GameFramework.Logic;
using Server.Tools;
using FS.GameEngine.Network;
using FS.GameEngine.Network.Protocol;
using System.Text;
using FS.GameEngine.Scene;
using HSGameEngine.GameEngine.Network;
using HSGameEngine.GameEngine.Network.Protocol;

namespace FS.VLTK.UI.SelectServer
{
    /// <summary>
    /// Khung chọn Server
    /// </summary>
    public class UISelectServer : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Button quay lại màn hình Login
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_BackToLogin;

        /// <summary>
        /// Khung chọn máy chủ
        /// </summary>
        [SerializeField]
        private GameObject UIFrame_SelectServer;

        /// <summary>
        /// Prefab danh sách cụm máy chủ
        /// </summary>
        [SerializeField]
        private UISelectServer_ToggleGroupServer UIToggle_ServerGroupPrefab;

        /// <summary>
        /// Prefab thông tin máy chủ
        /// </summary>
        [SerializeField]
        private UISelectServer_ButtonServerDetails UIButton_ServerDetailsPrefab;
        #endregion

        #region Properties
        /// <summary>
        /// Gọi đến khi có sự kiện chọn Server
        /// </summary>
        public EventHandler NextStep { get; set; }
        #endregion

        #region Private fields
        /// <summary>
        /// Socket
        /// </summary>
        private TCPClient tcpClient = new TCPClient(2);

        /// <summary>
        /// Tập hợp Buttons thông tin máy chủ trong cụm
        /// </summary>
        private readonly UISelectServer_ButtonServerDetails[] UIButtons_ListServer = new UISelectServer_ButtonServerDetails[10];
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi dến ở Frame đầu tiên
        /// </summary>
        private void Start()
        {
            this.InitPrefabs();
            this.StartCoroutine(this.GetFullData());
        }
        #endregion

        #region Code UI
        /// <summary>
        /// Khởi tạo ban đầu
        /// </summary>
        private void InitPrefabs()
        {
            this.UIButton_BackToLogin.onClick.AddListener(this.ButtonBackToLogin_Click);

            /// Sinh ra các Button máy chủ mặc định
            for (int i = 0; i < this.UIButtons_ListServer.Length; i++)
            {
                /// Tạo mới
                this.UIButtons_ListServer[i] = GameObject.Instantiate<UISelectServer_ButtonServerDetails>(this.UIButton_ServerDetailsPrefab);
                this.UIButtons_ListServer[i].transform.SetParent(this.UIButton_ServerDetailsPrefab.transform.parent, false);
                this.UIButtons_ListServer[i].gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Sự kiện khi nút quay lại màn hình Login được ấn
        /// </summary>
        private void ButtonBackToLogin_Click()
        {
            GameObject.Destroy(this.gameObject);
            Super.ShowGameLogin();
        }

        /// <summary>
        /// Sự kiện khi Server được chọn
        /// </summary>
        /// <param name="serverInfo"></param>
        private void ButtonServer_Click(BuffServerInfo serverInfo)
        {
            if (serverInfo == null)
            {
                Super.ShowMessageBox("Lỗi", "Máy chủ được lựa chọn không chính xác.", true);
                return;
            }
            if (serverInfo.nStatus == 1)
            {
                Super.ShowMessageBox("Lỗi", "Máy chủ được chọn hiện đang bảo trì.", true);
                return;
            }

            if (serverInfo != null)
            {
                Global.RootParams["serverip"] = serverInfo.strURL;
                Global.RootParams["gameport"] = "" + serverInfo.nServerPort;
                Global.RootParams["loginport"] = "" + serverInfo.nServerPort;

                if (Global.Data.ServerData != null)
                {
                    Global.Data.ServerData.LastServer = serverInfo;
                }

                PlayerPrefs.SetInt("NewLastServerInfoID", serverInfo.nServerID);
                this.ConnectToLoginServer();
            }
        }

        /// <summary>
        /// Cập nhật danh sách máy chủ
        /// </summary>
        /// <param name="listServer"></param>
        private void UpdateServerInfo(List<BuffServerInfo> listServer)
        {
            for (int i = 0; i < this.UIButtons_ListServer.Length; i++)
            {
                this.UIButtons_ListServer[i].gameObject.SetActive(false);
            }

            if (listServer == null)
            {
                return;
            }

            for (int i = 0; i < Math.Min(listServer.Count, this.UIButtons_ListServer.Length); i++)
            {
                BuffServerInfo serverInfo = listServer[i];
                this.UIButtons_ListServer[i].gameObject.SetActive(true);
                this.UIButtons_ListServer[i].ServerName = serverInfo.strServerName;
                this.UIButtons_ListServer[i].Status = serverInfo.nStatus;
                this.UIButtons_ListServer[i].Description = listServer[i].Msg;
                this.UIButtons_ListServer[i].Select = () => {
                    this.ButtonServer_Click(serverInfo);
                };
            }
        }

        /// <summary>
        /// Cập nhật danh sách cụm máy chủ
        /// </summary>
        private void UpdateGroupServerInfo()
        {
            XuanFuServerData serverData = Global.Data.ServerData;

            if (serverData == null)
            {
                return;
            }
            int count = serverData.ServerInfos.Count;

            int totalServerItemCount = 0;
            int modelCount = 0;
            if (count % 10 == 0)
            {
                totalServerItemCount = count / 10;
            }
            else
            {
                totalServerItemCount = count / 10 + 1;
            }
            modelCount = count % 10;
            int startIndex = 0;
            int endIndex = 0;

            UISelectServer_ToggleGroupServer toggleRecommendedListServer = GameObject.Instantiate<UISelectServer_ToggleGroupServer>(this.UIToggle_ServerGroupPrefab);
            toggleRecommendedListServer.Group = this.UIToggle_ServerGroupPrefab.transform.parent.gameObject.GetComponent<UnityEngine.UI.ToggleGroup>();
            toggleRecommendedListServer.transform.SetParent(this.UIToggle_ServerGroupPrefab.transform.parent, false);
            toggleRecommendedListServer.gameObject.SetActive(true);
            toggleRecommendedListServer.Name = "Máy chủ đề cử";
            toggleRecommendedListServer.OnActivated = (isSelected) =>
            {
                toggleRecommendedListServer.Active = isSelected;
                if (isSelected)
                {
                    this.UpdateServerInfo(serverData.RecommendServerInfos);
                }
            };
            toggleRecommendedListServer.Active = false;
            IEnumerator SelectFirst()
            {
                yield return null;
                toggleRecommendedListServer.Active = true;
            }
            this.StartCoroutine(SelectFirst());

            for (int i = totalServerItemCount - 1; i >= 0; i--)
            {
                UISelectServer_ToggleGroupServer toggleListServerByGroup = GameObject.Instantiate<UISelectServer_ToggleGroupServer>(this.UIToggle_ServerGroupPrefab);
                toggleListServerByGroup.Group = this.UIToggle_ServerGroupPrefab.transform.parent.gameObject.GetComponent<UnityEngine.UI.ToggleGroup>();
                toggleListServerByGroup.transform.SetParent(this.UIToggle_ServerGroupPrefab.transform.parent, false);
                toggleListServerByGroup.gameObject.SetActive(true);
                if (i < totalServerItemCount - 1)
                {
                    startIndex = i * 10;
                    endIndex = i * 10 + 9;
                    toggleListServerByGroup.Name = string.Format("Cụm {0} - {1}", (startIndex + 1), (endIndex + 1));
                }
                else
                {
                    startIndex = i * 10;
                    if (modelCount != 0)
                    {
                        endIndex = i * 10 + modelCount - 1;
                    }
                    else
                    {
                        endIndex = i * 10 + 10 - 1;
                    }
                    toggleListServerByGroup.Name = string.Format("Cụm {0} - {1}", (startIndex + 1), (endIndex + 1));
                }

                List<BuffServerInfo> listServers = new List<BuffServerInfo>();
                for (int j = startIndex; j <= endIndex; j++)
                {
                    listServers.Add(serverData.ServerInfos[j]);
                }
                toggleListServerByGroup.OnActivated = (isSelected) =>
                {
                    toggleListServerByGroup.Active = isSelected;
                    if (isSelected)
                    {
                        this.UpdateServerInfo(listServers);
                    }
                };
                toggleListServerByGroup.Active = false;
            }
        }
        #endregion

        #region Network
        /// <summary>
        /// Lấy dữ liệu từ Socket vào màn hình chọn Server
        /// </summary>
        /// <returns></returns>
        public IEnumerator GetFullData()
        {
           string url = MainGame.GameInfo.ServerListURL;

            WWWForm wwwForm = new WWWForm();
            wwwForm.AddField("strUID", GameInstance.Game.CurrentSession.UserID);

            UnityWebRequest www = UnityWebRequest.Post(url, wwwForm);
            yield return www.SendWebRequest();

            if (!string.IsNullOrEmpty(www.error))
            {
                Super.ShowMessageBox("Lỗi đăng nhập", "Không thể tải danh sách máy chủ. Hãy thử thoát game và đăng nhập lại.", true);
                yield break;
            }

            BuffServerListDataEx listDataEx = DataHelper.BytesToObject<BuffServerListDataEx>(www.downloadHandler.data, 0, www.downloadHandler.data.Length);

            if (listDataEx == null || listDataEx.RecommendListServerData == null || listDataEx.RecommendListServerData.Count == 0)
            {
                Super.ShowMessageBox("Lỗi đăng nhập", "Dịch vụ đang bảo trì, hãy thử đăng nhập lại sau.", true);
                yield break;
            }


            Global.Data.ServerData = new XuanFuServerData();

            Global.Data.ServerData.ServerInfos = listDataEx.ListServerData;


            if (listDataEx.RecommendListServerData.Count > 0)
            {
                Global.Data.ServerData.RecommendServer = listDataEx.RecommendListServerData[0];
            }
            if (listDataEx.ListServerData.Count > 0)
            {
                Global.Data.ServerData.LastServer = listDataEx.ListServerData[0];
            }
            else
            {
                if (listDataEx.RecommendListServerData.Count > 0)
                {
                    Global.Data.ServerData.LastServer = listDataEx.RecommendListServerData[0];
                }
            }

            www.Dispose();
            www = null;

            Super.HideNetWaiting();
            this.UpdateGroupServerInfo();
        }


#region TCP Client
        /// <summary>
        /// Kết nối tới Server
        /// </summary>		
        public void ConnectToLoginServer()
        {
            this.ResetTCPClient();

            this.tcpClient.SocketConnect += this.SocketConnect;
            String loginIP = Global.GetRootParam("serverip", "127.0.0.1");
            Super.ShowNetWaiting("Đang kết nối tới Server...");
            this.tcpClient.Connect(loginIP, Global.GetUserLoginPort());
        }

        /// <summary>
        /// Làm mới Socket
        /// </summary>		
        public void ResetTCPClient()
        {
            if (null != tcpClient)
            {
                this.tcpClient.Disconnect();
                this.tcpClient.Destroy();
                this.tcpClient = new TCPClient(2);
            }
        }

        /// <summary>
        /// Hàm gọi đến mỗi khi gói tin được nhận
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
                    case (int)NetSocketTypes.SOCKET_CONN:
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
                            loginCmdID = (int)(TCPLoginServerCmds.CMD_LOGIN_ON2);

                            strcmd = string.Format("{0}:{1}:{2}:{3}:{4}:{5}", (int)(TCPCmdProtocolVer.VerSign), uid, name, lastTime, isadult, token);
                            bytesCmd = new UTF8Encoding().GetBytes(strcmd);

                            TCPOutPacket tcpOutPacket = new TCPOutPacket();
                            tcpOutPacket.PacketCmdID = (short)(loginCmdID);
                            tcpOutPacket.FinalWriteData(bytesCmd, 0, bytesCmd.Length);
                            this.tcpClient.SendData(tcpOutPacket);
                        }
                        else
                        {
                            Super.HideNetWaiting();
                            Super.ShowMessageBox("Lỗi đăng nhập", "Kết nối tới máy chủ thất bại, xin hãy thử lại sau.", true);
                        }
                        break;
                    case (int)NetSocketTypes.SOCKET_SEND:
                        Super.HideNetWaiting();
                        Super.ShowMessageBox("Lỗi đăng nhập", "Không thể gửi gói tin tới máy chủ.", true);
                        break;
                    case (int)NetSocketTypes.SOCKET_RECV:
                        break;
                    case (int)NetSocketTypes.SOCKET_CLOSE:
                        GScene.ServerStopGame();
                        Super.HideNetWaiting();
                        Super.ShowMessageBox("Lỗi đăng nhập", "Kết nối tới máy chủ bị gián đoạn.", true);
                        break;
                    case (int)NetSocketTypes.SOCKT_CMD:
                        this.tcpClient.Disconnect();
                        if ("-1" == e.fields[0])
                        {
                            Super.HideNetWaiting();
                            Super.ShowMessageBox("Lỗi đăng nhập", "Tài khoản hoặc mật khẩu nhập vào không chính xác.", true);
                        }
                        else if ("-2" == e.fields[0])
                        {
                            Super.HideNetWaiting();
                            Super.ShowMessageBox("Lỗi đăng nhập", "Phiên bản Client đã cũ, hãy tiến hành cập nhật trước tiên.", true);
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
                            
                            this.NextStep?.Invoke(this, EventArgs.Empty);
                        }
                        break;
                    default:
                        throw new Exception("Error on Socket PlatformUserLogin");
                }
            });
        }
#endregion
#endregion
    }
}