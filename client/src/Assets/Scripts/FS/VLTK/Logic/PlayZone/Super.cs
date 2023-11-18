using System;
using System.Collections.Generic;
using UnityEngine;
using FS.GameEngine.Logic;
using FS.GameEngine.Network;
using Server.Data;
using FS.VLTK.UI;
using FS.VLTK.UI.Main.MessageBox;
using FS.VLTK.UI.LoadingResources;
using FS.VLTK.Entities;
using FS.GameFramework.Controls;
using FS.VLTK.Control;
using FS.VLTK.Factory;
using FS.VLTK.Utilities.Threading;
using System.Collections;
using FS.GameEngine.Data;

namespace FS.GameFramework.Logic
{
	/// <summary>
	/// Quản lý các đối tượng toàn cục dùng cho Game
	/// </summary>
	public static class Super
    {
        #region UI bảng thông báo đợi
        /// <summary>
        /// Hiện UI bảng thông báo đợi
        /// </summary>  
        public static void ShowNetWaiting(string message = null)
        {
            Super.ShowMessageBox(message, false);
        }

        /// <summary>
        /// Ẩn UI bảng thông báo đợi
        /// </summary>  
        public static void HideNetWaiting()
        {
            Super.HideMessageBox();
        }

        #endregion

        #region Trở lại màn hình đăng nhập
        /// <summary>
        /// Trở lại màn hình đăng nhập từ Game
        /// </summary>
        public static void BackToLoginFromGame()
        {
            IEnumerator DoWork()
            {
                /// Đăng xuất
                GameInstance.Game.Disconnect();
                GameInstance.Game.ActiveDisconnect = true;
                /// Xóa dữ liệu Game
                Super.ClearGameData();
                /// Đợi 2 Frame
                yield return null;
                yield return null;
                /// Hiển thị màn hình đăng nhập
                Super.ShowGameLogin();
                /// Tạo mới Global Data
                Global.Data = new GData();
            }
            MainGame.Instance.StartCoroutine(DoWork());
        }

        /// <summary>
        /// Xóa dữ liệu Game
        /// </summary>
        private static void ClearGameData()
        {
            /// Thực hiện xóa rác
            KTResourceManager.Instance.OnSceneChanged();
            /// Dữ liệu Game
            Global.Data?.Clear();
            Global.Data = null;
            /// Xóa bản đồ hiện tại
            Super.DestroyCurrentMap();
            /// PlayZone
            PlayZone.Instance?.Destroy();
            /// Đóng Socket
            PreGameTCPCmdHandler.Instance.CloseSocket(false);
            /// Ngắt TCPGame
            GameInstance.Game.Disconnect();
            GameInstance.Game = null;
            /// Tạo mới TCPGame
            GameInstance.Game = new TCPGame();

            /// Chỉnh lại tầm nhìn Camera
            MainGame.Instance.ResetCameraFieldOfView();
        }
        #endregion

        #region Xóa bản đồ
        /// <summary>
        /// Xóa bản đồ hiện tại
        /// </summary>
        public static void DestroyCurrentMap()
        {
            GameObject root2DScene = GameObject.Find("Scene 2D Root");
            if (root2DScene != null)
            {
                GameObject.Destroy(root2DScene);
            }
        }
        #endregion


        #region Bảng thông báo
        /// <summary>
        /// Tạo khung bảng thông báo nếu chưa tồn tại
        /// </summary>
        private static void CreateMessageBoxIfNotExist()
        {
            /// Nếu đã tồn tại
            if (Global.MainCanvas.GetComponent<CanvasManager>().UIMessageBox != null)
            {
                /// Bỏ qua
                return;
            }
            /// Tạo mới
            CanvasManager canvas = Global.MainCanvas.GetComponent<CanvasManager>();
            canvas.UIMessageBox = canvas.LoadUIPrefab<UIMessageBox>("MainGame/UIMessageBox");
            canvas.AddStaticUI(canvas.UIMessageBox);
        }

        /// <summary>
        /// Hiển thị bảng thông báo
        /// </summary>
        /// <param name="content"></param>
        public static void ShowMessageBox(string content)
        {
            /// Tạo khung nếu chưa tồn tại
            Super.CreateMessageBoxIfNotExist();
            UIMessageBox uiMessageBox = Global.MainCanvas.GetComponent<CanvasManager>().UIMessageBox;
            uiMessageBox.Title = "Thông báo";
            uiMessageBox.Content = content;
            uiMessageBox.ShowButtonOK = false;
            uiMessageBox.ShowButtonCancel = false;
            uiMessageBox.OK = null;
            uiMessageBox.Cancel = null;
            uiMessageBox.Show();
        }

        /// <summary>
        /// Hiển thị bảng thông báo
        /// </summary>
        /// <param name="content"></param>
        public static void ShowMessageBox(string content, bool isShowButtonOK)
        {
            /// Tạo khung nếu chưa tồn tại
            Super.CreateMessageBoxIfNotExist();
            UIMessageBox uiMessageBox = Global.MainCanvas.GetComponent<CanvasManager>().UIMessageBox;
            uiMessageBox.Title = "Thông báo";
            uiMessageBox.Content = content;
            uiMessageBox.ShowButtonOK = isShowButtonOK;
            uiMessageBox.ShowButtonCancel = false;
            uiMessageBox.OK = null;
            uiMessageBox.Cancel = null;
            uiMessageBox.Show();
        }

        /// <summary>
        /// Hiển thị bảng thông báo
        /// </summary>
        /// <param name="title"></param>
        /// <param name="content"></param>
        public static void ShowMessageBox(string title, string content)
        {
            /// Tạo khung nếu chưa tồn tại
            Super.CreateMessageBoxIfNotExist();
            UIMessageBox uiMessageBox = Global.MainCanvas.GetComponent<CanvasManager>().UIMessageBox;
            uiMessageBox.Title = title;
            uiMessageBox.Content = content;
            uiMessageBox.ShowButtonOK = false;
            uiMessageBox.ShowButtonCancel = false;
            uiMessageBox.OK = null;
            uiMessageBox.Cancel = null;
            uiMessageBox.Show();
        }

        /// <summary>
        /// Hiển thị bảng thông báo
        /// </summary>
        /// <param name="title"></param>
        /// <param name="content"></param>
        public static void ShowMessageBox(string title, string content, bool isShowButtonOK)
        {
            /// Tạo khung nếu chưa tồn tại
            Super.CreateMessageBoxIfNotExist();
            UIMessageBox uiMessageBox = Global.MainCanvas.GetComponent<CanvasManager>().UIMessageBox;
            uiMessageBox.Title = title;
            uiMessageBox.Content = content;
            uiMessageBox.ShowButtonOK = isShowButtonOK;
            uiMessageBox.ShowButtonCancel = false;
            uiMessageBox.OK = null;
            uiMessageBox.Cancel = null;
            uiMessageBox.Show();
        }

        /// <summary>
        /// Hiển thị bảng thông báo
        /// </summary>
        /// <param name="content"></param>
        public static void ShowMessageBox(string content, Action OnOK)
        {
            /// Tạo khung nếu chưa tồn tại
            Super.CreateMessageBoxIfNotExist();
            UIMessageBox uiMessageBox = Global.MainCanvas.GetComponent<CanvasManager>().UIMessageBox;
            uiMessageBox.Title = "Thông báo";
            uiMessageBox.Content = content;
            uiMessageBox.ShowButtonOK = true;
            uiMessageBox.ShowButtonCancel = false;
            uiMessageBox.OK = OnOK;
            uiMessageBox.Cancel = null;
            uiMessageBox.Show();
        }

        /// <summary>
        /// Hiển thị bảng thông báo
        /// </summary>
        /// <param name="title"></param>
        /// <param name="content"></param>
        public static void ShowMessageBox(string title, string content, Action OnOK)
        {
            /// Tạo khung nếu chưa tồn tại
            Super.CreateMessageBoxIfNotExist();
            UIMessageBox uiMessageBox = Global.MainCanvas.GetComponent<CanvasManager>().UIMessageBox;
            uiMessageBox.Title = title;
            uiMessageBox.Content = content;
            uiMessageBox.ShowButtonOK = true;
            uiMessageBox.ShowButtonCancel = false;
            uiMessageBox.OK = OnOK;
            uiMessageBox.Cancel = null;
            uiMessageBox.Show();
        }

        /// <summary>
        /// Hiển thị bảng thông báo
        /// </summary>
        /// <param name="title"></param>
        /// <param name="content"></param>
        public static void ShowMessageBox(string content, Action OnOK, Action OnCancel)
        {
            /// Tạo khung nếu chưa tồn tại
            Super.CreateMessageBoxIfNotExist();
            UIMessageBox uiMessageBox = Global.MainCanvas.GetComponent<CanvasManager>().UIMessageBox;
            uiMessageBox.Title = "Thông báo";
            uiMessageBox.Content = content;
            uiMessageBox.ShowButtonOK = true;
            uiMessageBox.ShowButtonCancel = true;
            uiMessageBox.OK = OnOK;
            uiMessageBox.Cancel = OnCancel;
            uiMessageBox.Show();
        }

        /// <summary>
        /// Hiển thị bảng thông báo
        /// </summary>
        /// <param name="title"></param>
        /// <param name="content"></param>
        /// <param name="OnOK"></param>
        /// <param name="OnCancel"></param>
        public static void ShowMessageBox(string title, string content, Action OnOK, Action OnCancel)
        {
            /// Tạo khung nếu chưa tồn tại
            Super.CreateMessageBoxIfNotExist();
            UIMessageBox uiMessageBox = Global.MainCanvas.GetComponent<CanvasManager>().UIMessageBox;
            uiMessageBox.Title = title;
            uiMessageBox.Content = content;
            uiMessageBox.ShowButtonOK = true;
            uiMessageBox.ShowButtonCancel = true;
            uiMessageBox.OK = OnOK;
            uiMessageBox.Cancel = OnCancel;
            uiMessageBox.Show();
        }

        /// <summary>
        /// Ẩn bảng thông báo
        /// </summary>
        public static void HideMessageBox()
        {
            /// Tạo khung nếu chưa tồn tại
            Super.CreateMessageBoxIfNotExist();
            UIMessageBox uiMessageBox = Global.MainCanvas.GetComponent<CanvasManager>().UIMessageBox;
            uiMessageBox.Hide();
        }
        #endregion

        #region Login Scene
        /// <summary>
        /// Login Scene
        /// </summary>
        private static Transform LoginScene = null;

        /// <summary>
        /// Tải xuống màn Login Scene
        /// </summary>
        public static void LoadLoginScene()
        {
            if (Super.LoginScene != null)
            {
                return;
            }
            Transform loginScenePrefab = Resources.Load<Transform>("VLTK/Prefabs/Login Scene");
            Super.LoginScene = GameObject.Instantiate<Transform>(loginScenePrefab);
        }

        /// <summary>
        /// Xóa màn Login Scene
        /// </summary>
        public static void DestroyLoginScene()
        {
            if (Super.LoginScene != null)
            {
                GameObject.Destroy(Super.LoginScene.gameObject);
                Super.LoginScene = null;
            }
        }
        #endregion

        #region Tải xuống/Làm mới/Đăng ký/Đăng nhập trò chơi
        /// <summary>
        /// Màn hình mặc định ban đầu mở App
        /// </summary>
        private static UIFirstScreen FirstScreen = null;

        /// <summary>
        /// Hiển thị màn hình mặc định ban đầu mở App
        /// </summary>
        public static void ShowFirstScreen()
        {
            /// Nếu đang hiện thì bỏ qua
            if (Super.FirstScreen != null)
            {
                return;
            }
            Super.FirstScreen = CanvasManager.Instance.LoadUIPrefab<UIFirstScreen>("FirstScreen/UIFirstScreen");
            CanvasManager.Instance.AddUI(Super.FirstScreen);
        }

        /// <summary>
        /// Đóng màn hình mặc định ban đầu mở App
        /// </summary>
        public static void CloseFirstScreen()
        {
            /// Nếu đang hiện
            if (Super.FirstScreen != null)
            {
                /// Xóa đối tượng
                GameObject.Destroy(Super.FirstScreen.gameObject);
                Super.FirstScreen = null;
            }
        }

        /// <summary>
        /// Hiển thị màn hình Download dữ liệu
        /// </summary>
        public static void ShowLoadingResources(UpdateFiles toDownloadFiles, Action done, Action faild)
        {
            UILoadingResources loadingResource = CanvasManager.Instance.LoadUIPrefab<UILoadingResources>("LoadingResources/UILoadingResources");
            CanvasManager.Instance.AddUI(loadingResource);
            loadingResource.ToDownloadFiles = toDownloadFiles;
            loadingResource.NextStep = done;
            loadingResource.Faild = faild;

        }

        /// <summary>
        /// Hiển thị màn hình tải dữ liệu đầu game
        /// </summary>  
        public static void ShowLoadingGame()
        {
            FS.VLTK.UI.LoadingGame.UILoadingGame loadingGame = FS.VLTK.UI.CanvasManager.Instance.LoadUIPrefab<FS.VLTK.UI.LoadingGame.UILoadingGame>("LoadingGame/UILoadingGame");
            FS.VLTK.UI.CanvasManager.Instance.AddUI(loadingGame);
            loadingGame.NextStep = () => {
                FS.VLTK.UI.CanvasManager.Instance.RemoveUI(loadingGame);
                Super.ShowGameLogin();
            };
        }

        /// <summary>
        /// Hiển thị màn hình đăng nhập và đăng ký
        /// </summary>
        /// <param name="root"></param>
        public static void ShowGameLogin()
        {
            /// Hiển thị màn hình LoginScene
            Super.LoadLoginScene();

            FS.VLTK.UI.LoginGame.UILoginGame loginGame = FS.VLTK.UI.CanvasManager.Instance.LoadUIPrefab<FS.VLTK.UI.LoginGame.UILoginGame>("LoginGame/UILoginGame");
            FS.VLTK.UI.CanvasManager.Instance.AddUI(loginGame);
            loginGame.LoginSuccess = () => {
                Super.ShowNetWaiting("Đang tải thông tin Server...");
                FS.VLTK.UI.CanvasManager.Instance.RemoveUI(loginGame);
                Super.ShowSelectServer();
            };
        }

        /// <summary>
        /// Hiển thị màn hình chọn Server
        /// </summary>
        public static void ShowSelectServer()
        {
            FS.VLTK.UI.SelectServer.UISelectServer selectServer = FS.VLTK.UI.CanvasManager.Instance.LoadUIPrefab<FS.VLTK.UI.SelectServer.UISelectServer>("SelectServer/UISelectServer");
            FS.VLTK.UI.CanvasManager.Instance.AddUI(selectServer);
            selectServer.NextStep = (o, e) => {
                Super.FakeConnectToLineServer();
                Super.ShowRoleManager();
                FS.VLTK.UI.CanvasManager.Instance.RemoveUI(selectServer);
            };
        }

        /// <summary>
        /// Tạo kết nối giả tới Server
        /// </summary>
        public static void FakeConnectToLineServer()
        {
            string loginIP = Global.GetRootParam("serverip", "127.0.0.1");
            Global.LineDataList = new List<LineData>();
            LineData lineData = new LineData()
            {
                LineID = 1,
                GameServerIP = loginIP,
                GameServerPort = Global.GetGameServerPort(),
                OnlineCount = 0,
            };

            Global.LineDataList.Add(lineData);
            Global.CurrentListData = Global.LineDataList[0];
        }

        /// <summary>
        /// Hiển thị màn hình quản lý nhân vật (gồm tạo nhân vật, xóa nhân vật, chọn nhân vật)
        /// </summary>  
        public static void ShowRoleManager()
        {
            FS.VLTK.UI.RoleManager.UIRoleManager roleManager = FS.VLTK.UI.CanvasManager.Instance.LoadUIPrefab<FS.VLTK.UI.RoleManager.UIRoleManager>("RoleManager/UIRoleManager");
            FS.VLTK.UI.CanvasManager.Instance.AddUI(roleManager);
            roleManager.DirectLogin = GameInstance.Game.CurrentSession.RoleID != -1;

			if (KuaFuLoginManager.DirectLogin())
            {
                roleManager.DirectLogin = true;
            }									
            roleManager.StartGameByRole = () =>
            {
                CanvasManager.Instance.RemoveUI(roleManager);
                Super.ShowLoadingMap(() => {
                        
                });
            };

            roleManager.GoBack = () =>
            {
                /// Xóa UI
                CanvasManager.Instance.RemoveUI(roleManager);
                /// Đăng xuất
                GameInstance.Game.Disconnect();
                GameInstance.Game.ActiveDisconnect = true;
                /// Xóa dữ liệu Game
                Super.ClearGameData();
                /// Tạo mới Global Data
                Global.Data = new GData();
            };
        }

        /// <summary>
        /// Màn hình tải bản đồ hiện tại
        /// </summary>
        public static FS.VLTK.UI.LoadingMap.UILoadingMap CurrentLoadingMap { get; private set; } = null;

        /// <summary>
        /// Xóa màn hình tải bản đồ
        /// </summary>
        public static void DestroyLoadingMap()
        {
            if (Super.CurrentLoadingMap == null)
            {
                return;
            }

            FS.VLTK.UI.CanvasManager.Instance.RemoveUI(Super.CurrentLoadingMap);
        }

        /// <summary>
        /// Hiển thị màn hình tải map
        /// </summary>  
        public static void ShowLoadingMap(Action WorkFinished = null)
        {
            /// Đánh dấu đang đợi chuyển Map
            Global.Data.WaitingForMapChange = true;

            /// Thực hiện xóa rác
            KTResourceManager.Instance.OnSceneChanged();

            /// Xóa màn cũ
            Super.DestroyLoadingMap();

            /// Tạo màn mới
            FS.VLTK.UI.LoadingMap.UILoadingMap loadingMap = FS.VLTK.UI.CanvasManager.Instance.LoadUIPrefab<FS.VLTK.UI.LoadingMap.UILoadingMap>("LoadingMap/UILoadingMap");
            loadingMap.DoDestroy = () => {
                Super.CurrentLoadingMap = null;
            };
            FS.VLTK.UI.CanvasManager.Instance.AddUI(loadingMap);
            loadingMap.MapCode = Global.Data.RoleData.MapCode;
            /// Lỗi đéo gì đó nên phải làm như này
            loadingMap.StartLoading = () =>
            {
                /// Lưu lại thông tin
                Super.CurrentLoadingMap = loadingMap;
            };
            loadingMap.WorkFinished = () => {
                if (PlayZone.Instance == null)
                {
                    Super.ShowGamePlayZone();
                }
                else
                {
                    PlayZone.Instance.LoadScene(Global.Data.RoleData.MapCode, Global.Data.RoleData.PosX, Global.Data.RoleData.PosY, 0);
                }
                /// Xóa màn hình LoginScene
                Super.DestroyLoginScene();

                WorkFinished?.Invoke();

                /// Bỏ đánh dấu đang đợi chuyển Map
                Global.Data.WaitingForMapChange = false;
            };
            loadingMap.BeginLoad2DMapRes();
        }

        /// <summary>
        /// Hiển thị quản lý Game
        /// </summary>  
        public static void ShowGamePlayZone()
        {
            GameObject go = new GameObject("PlayZone");
            go.AddComponent<PlayZone>();
        }
        #endregion
    }
}
