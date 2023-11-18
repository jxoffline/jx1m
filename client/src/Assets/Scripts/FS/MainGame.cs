#define ENABLE_AUTO_UPDATE
#define TEST
//#define ENABLE_DEBUG_LOG
using FS.GameEngine.Data;
using FS.GameEngine.Logic;
using FS.GameEngine.TimerManager;
using FS.GameFramework.Logic;
using FS.VLTK;
using FS.VLTK.Entities;
using FS.VLTK.Factory.ObjectsManager;
using FS.VLTK.Loader;
using FS.VLTK.Utilities;
using FS.VLTK.Utilities.UnityComponent;
using HSGameEngine.GameEngine.Network.Tools;
using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using TMPro;
using UnityEngine;


/// <summary>
/// Đối tượng quản lý Game
/// </summary>
public class MainGame : TTMonoBehaviour
{
    #region Singleton Instance
    /// <summary>
    /// Đối tượng quản lý Game
    /// </summary>
    public static MainGame Instance { get; set; }
    #endregion

    #region Define
    /// <summary>
    /// Text phiên bản
    /// </summary>
    [SerializeField]
    private string _Version;

    /// <summary>
    /// Danh sách các đối tượng khởi động cùng Game
    /// </summary>
    [SerializeField]
    private Transform PreloadObjects = null;

    /// <summary>
    /// Canvas chứa UI trong Game
    /// </summary>
    [SerializeField]
    private Canvas MainCanvas = null;

    /// <summary>
    /// Camera dùng trong Radar Map
    /// </summary>
    [SerializeField]
    private Camera RadarMapCamera = null;

    /// <summary>
    /// Camera chiếu Object lên UI
    /// </summary>
    [SerializeField]
    private Camera ObjectPreviewCameraPrefab = null;

    /// <summary>
    /// Camera chính
    /// </summary>
    [SerializeField]
    private Camera MainCamera = null;

    /// <summary>
    /// Đối tượng thu âm và phát lại
    /// </summary>
    [SerializeField]
    private AudioRecorderAndPlayer Recorder = null;
    #endregion

    #region Private fields
    /// <summary>
    /// Khởi tạo trò chơi thành công
    /// </summary>
    private bool InitOK = false;

    /// <summary>
    /// Tầm nhìn mặc định của Camera ban đầu
    /// </summary>
    private int InitCameraFieldOfView;
    #endregion

    #region Properties
    /// <summary>
    /// Phiên bản Client
    /// </summary>
    public string Version
    {
        get
        {
            return this._Version;
        }
    }

    /// <summary>
    /// Thông tin trò chơi
    /// </summary>
    public static GameInfo GameInfo { get; private set; }

    /// <summary>
    /// Thông tin các File được cập nhật
    /// </summary>
    public static UpdateFiles UpdateFiles { get; set; }

    /// <summary>
    /// Thông tin các File chờ cập nhật sau
    /// </summary>
    public static UpdateFiles ListUpdateLaterFiles { get; set; }
    #endregion

    #region Core MonoBehaviour
    /// <summary>
    /// Hàm này gọi khi đối tượng được tạo ra
    /// </summary>
    private void Awake()
    {
#if UNITY_EDITOR || ENABLE_DEBUG_LOG
        KTDebug.EnableDebug = true;
#else
        KTDebug.EnableDebug = false;
#endif

        MainGame.Instance = this;
        Application.runInBackground = true;

#if !UNITY_EDITOR
        Application.targetFrameRate = 60;
#endif

#if UNITY_IOS
        UnityEngine.iOS.Device.SetNoBackupFlag(Application.persistentDataPath);
#endif

      
        // Thiết lập ban đầu
        Global.MainCanvas = this.MainCanvas;
        Global.ObjectPreviewCameraPrefab = this.ObjectPreviewCameraPrefab;
        Global.RadarMapCamera = this.RadarMapCamera;
        Global.MainCamera = this.MainCamera;
        Global.Recorder = this.Recorder;
        this.InitCameraFieldOfView = (int)this.MainCamera.orthographicSize;

        /// Thông tin thiết bị
        KTGlobal.DeviceInfo = SystemInfo.deviceUniqueIdentifier;
        /// Khởi tạo dữ liệu
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        /// Thiết lập khóa mã hóa tài nguyên
        KTResourceCrypto.SetKey("eabb22bdc77d8d8fc85f52a572ae2f52eabb22bdc77d8d8fc85f52a572ae2f52eabb22bdc77d8d8fc85f52a572ae2f52eabb22bdc77d8d8fc85f52a572ae2f52eabb22bdc77d8d8fc85f52a572ae2f52eabb22bdc77d8d8fc85f52a572ae2f52eabb22bdc77d8d8fc85f52a572ae2f52eabb22bdc77d8d8fc85f52a572ae2f52");
    }
    private void InitCallback()
    {
      
    }

    private void OnHideUnity(bool isGameShown)
    {
        if (!isGameShown)
        {
            // Pause the game - we will need to hide
            Time.timeScale = 0;
        }
        else
        {
            // Resume the game - we're getting focus again
            Time.timeScale = 1;
        }
    }

    /// <summary>
    /// Hàm này gọi ở Frame đầu tiên
    /// </summary>
    private void Start()
    {
        /// Tạo đối tượng FirstScreen
        Super.ShowFirstScreen();
        /// Thiết lập Code Page
        ZipStrings.CodePage = System.Text.Encoding.UTF8.CodePage;

        /// Nếu không có mạng
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            Super.ShowMessageBox("Lỗi kết nối", "Trò chơi yêu cầu có kết nối mạng, vui lòng kiểm tra và khởi động lại trò chơi.", () =>
            {
                Application.Quit();
            });
        }
        else if (NetworkReachability.ReachableViaCarrierDataNetwork == Application.internetReachability)
        {
            Super.ShowMessageBox("Chú ý", "Bạn đang sử dụng kết nốt trực tiếp từ các thiết bị di động. Điều này có thể làm bạn mất phí. Hệ thống khuyên bạn nên sử dụng kết nối WiFi để trải nghiệm trò chơi tốt hơn.", () =>
            {
                this.ReadVersionFile();
            });
        }
        else
        {
            this.ReadVersionFile();
        }

        this.StartCoroutine(this.UpdateRender());
        this.StartCoroutine(this.CheckTimers());
    }

    /// <summary>
    /// Hàm này gọi liên tục mỗi Frame
    /// </summary>
    private void Update()
    {
        KTGlobal.NowTicks = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
    }

    /// <summary>
    /// Hàm này gọi đến khi thoát ứng dụng
    /// </summary>
    private void OnApplicationQuit()
    {
        /// Nếu đang hiển thị UI
        if (null != PlayZone.Instance)
        {
            PlayZone.Instance.CloseSocket();
        }
        Resources.UnloadUnusedAssets();
        foreach (AssetBundle bundle in AssetBundle.GetAllLoadedAssetBundles())
        {
            bundle.Unload(true);
        }
    }
    #endregion

    #region Private methods
    #region UpdateList
    /// <summary>
    /// Ghi thông tin ra File UpdateList.xml
    /// </summary>
    public static void ExportUpdateList()
    {
        Utils.SaveXElementToFile(MainGame.GameInfo.FileUpdateURL, MainGame.UpdateFiles.ToXML());
    }

    /// <summary>
    /// Đọc dữ liệu các File Update
    /// </summary>
    private void ReadUpdateListFile()
    {
        DownloadResource downloadResource = new GameObject().AddComponent<DownloadResource>();
        downloadResource.Faild = this.ReadUpdateListFileFaild;
        downloadResource.Done = this.ReadUpdateListFileComplete;
    }

    /// <summary>
    /// Sự kiện tải File UpdateList.xml hoàn tất
    /// </summary>
    /// <param name="localFiles"></param>
    /// <param name="updateFiles"></param>
    private void ReadUpdateListFileComplete(UpdateFiles localFiles, UpdateFiles updateFiles)
    {
        /// Cập nhật thông tin các File đã tải
        MainGame.UpdateFiles = localFiles;
        /// Tạo mới danh sách các File chờ Update sau
        MainGame.ListUpdateLaterFiles = new UpdateFiles();
        MainGame.ListUpdateLaterFiles.ZipFiles = new List<UpdateZipFile>();
        MainGame.ListUpdateLaterFiles.ZipFiles.AddRange(updateFiles.ZipFiles.Where(x => !x.IsFirstDownload));

#if !UNITY_EDITOR || ENABLE_AUTO_UPDATE
        /// Nếu cần Update
        if (updateFiles != null && updateFiles.Count > 0)
        {
            /// Hiện màn hình tải tài nguyên
            Super.ShowLoadingResources(updateFiles, () =>
            {
                /// Thêm toàn bộ các File tương ứng vào hệ thống
                if (MainGame.UpdateFiles == null)
                {
                    MainGame.UpdateFiles = new UpdateFiles()
                    {
                        Files = new List<UpdateFile>(),
                        ZipFiles = new List<UpdateZipFile>(),
                    };
                }
                MainGame.UpdateFiles.Files.AddRange(updateFiles.Files);
                MainGame.UpdateFiles.ZipFiles.AddRange(updateFiles.ZipFiles);

                /// Bắt đầu Game
                this.StartGame();
            }, () =>
            {
                /// Thoát Game
                Application.Quit();
            });
        }
        /// Nếu không cần Update
        else
#endif
        {
            /// Bắt đầu Game
            this.StartGame();
        }
    }

    /// <summary>
    /// Sự kiện tải File UpdateList.xml thất bại
    /// </summary>
    /// <param name="error"></param>
    private void ReadUpdateListFileFaild(string error)
    {
        Super.ShowMessageBox("Lỗi nghiêm trọng", "Không thể tải dữ liệu bản cập nhật, xin hãy thử khởi động lại trò trơi. Nếu vẫn không được, xin hãy liên hệ với hỗ trợ để được trợ giúp!", () =>
        {
            Application.Quit();
        });
    }
    #endregion

    #region Version
    /// <summary>
    /// Đọc dữ liệu phiên bản
    /// </summary>
    private void ReadVersionFile()
    {
        LoadVersion loadVersion = new GameObject().AddComponent<LoadVersion>();
        loadVersion.Faild = this.ReadVersionFaild;
        loadVersion.Done = this.ReadVersionCompleted;
    }

    /// <summary>
    /// Sự kiện tải File Version.xml thành công
    /// </summary>
    /// <param name="gameInfo"></param>
    /// <param name="result"></param>
    private void ReadVersionCompleted(GameInfo gameInfo, LoadVersionResult result)
    {
#if TEST
        /*bool hasExtension = gameInfo.ServerListURL.Contains(".aspx");
        string serverListURL = gameInfo.ServerListURL.Replace(".aspx", "");
      

        KTDebug.LogError("SERVER LIST : " + serverListURL);*/


#endif
        /// Đánh dấu thông tin trò chơi
        MainGame.GameInfo = gameInfo;

#if !UNITY_EDITOR || ENABLE_AUTO_UPDATE
        /// Nếu cần tải mới App
        if (result == LoadVersionResult.RequireReDownloadApp)
        {
            Super.ShowMessageBox("Cập nhật ứng dụng", gameInfo.NewVersionHint, () =>
            {
                /// Nếu thiết bị là IOS
                if (Application.platform == RuntimePlatform.IPhonePlayer)
                {
                    Application.OpenURL(gameInfo.GameIoSURL);
                }
                /// Nếu là thiết bị Android
                else if (Application.platform == RuntimePlatform.Android)
                {
                    Application.OpenURL(gameInfo.GameAndroidURL);
                }
                /// Thoát ứng dụng
                Application.Quit();
            }, () =>
            {
                Application.Quit();
            });
        }
        /// Nếu cần tải tài nguyên
        else if (result == LoadVersionResult.RequireDownloadResource)
        {
            Super.ShowMessageBox("Có tài nguyên cần tải, ấn OK để bắt đầu tải xuống.", () =>
            {
                /// Đọc File UpdateList
                this.ReadUpdateListFile();
            }, () =>
            {
                Application.Quit();
            });
        }
        /// Nếu không cần tải gì
        else
#endif
        {
            /// Đọc File UpdateList
            this.ReadUpdateListFile();
        }
    }

    /// <summary>
    /// Sự kiện tải File Version.xml thất bại
    /// </summary>
    /// <param name="error"></param>
    private void ReadVersionFaild(string error)
    {
        Super.ShowMessageBox("Lỗi nghiêm trọng", "Không tìm thấy File quy định phiên bản trò chơi, hãy gỡ và thử cài đặt lại. Nếu vẫn không được, xin hãy liên hệ với hỗ trợ để được trợ giúp!", () =>
        {
            Application.Quit();
        });
    }
    #endregion

    /// <summary>
    /// Bắt đầu Game
    /// </summary>
    private void StartGame()
    {
        /// Xóa đối tượng FirstScreen
        Super.CloseFirstScreen();

        /// Xóa trùng lặp
        MainGame.UpdateFiles.Files = MainGame.UpdateFiles.Files.GroupBy(x => x.ID).Select(x => x.FirstOrDefault()).OrderBy(x => x.ID).ToList();
        MainGame.UpdateFiles.ZipFiles = MainGame.UpdateFiles.ZipFiles.GroupBy(x => x.ID).Select(x => x.FirstOrDefault()).OrderBy(x => x.ID).ToList();

        /// Ghi dữ liệu lại vào file Version.xml
        Utils.SaveXElementToFile(LoadVersion.VersionFile, MainGame.GameInfo.ToXML());

        Global.Data = new GData();
        MyDateTime.Init();
        this.InitOK = true;

        /// Hiện các đối tượng thực thi cùng hệ thống
        this.PreloadObjects.gameObject.SetActive(true);

        /// Hiển thị màn hình đọc dữ liệu
        Super.ShowLoadingGame();

        /// Khởi tạo đối tượng quản lý soi trước nhân vật
        KTRolePreviewManager.NewInstance();
        /// Khởi tạo đối tượng quản lý soi trước quái
        KTMonsterPreviewManager.NewInstance();

        /// Set LOD
        Shader.globalMaximumLOD = 1;
    }
    #endregion

    /// <summary>
    /// Thực hiện cập nhật Render
    /// </summary>
    /// <returns></returns>
    private IEnumerator UpdateRender()
    {
        /// Lặp liên tục
        while (true)
        {
            /// Nghỉ
            yield return null;
            /// Nếu chưa thiết lập xong
            if (!this.InitOK)
            {
                /// Bỏ qua
                continue;
            }

            try
            {
                /// Cập nhật chất lượng hiển thị
                this.UpdateRenderQuality();
                /// Thực thi các sự kiện chạy trên Main-Thread
                this.DoQueueMainActions();
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }
    }

    /// <summary>
    /// Luồng chạy các Timer hệ thống
    /// </summary>
    /// <returns></returns>
    private IEnumerator CheckTimers()
    {
        while (true)
        {
            yield return null;
            /// Thực thi Timer
            DispatcherTimerDriver.ExecuteTimers();
        }
    }

    #region Quản lý sự kiện thực thi ở Main-Thread
    /// <summary>
    /// Danh sách sự kiện đang chờ thực thi
    /// </summary>
    private readonly ConcurrentQueue<Action> waitToBeProcess = new ConcurrentQueue<Action>();

    /// <summary>
    /// Thực thi xử lý các sự kiện chạy ở Main-Thread
    /// </summary>
    private void DoQueueMainActions()
    {
        /// Lấy dần các sự kiện ra khỏi hàng đợi
        while (true)
        {
            /// Nếu hàng đợi đã rỗng
            if (this.waitToBeProcess.IsEmpty)
            {
                break;
            }

            /// Lấy sự kiện ra khỏi hàng đợi
            if (this.waitToBeProcess.TryDequeue(out Action action))
            {
                try
                {
                    /// Thực thi sự kiện
                    action?.Invoke();
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
            }
        }
    }

    /// <summary>
    /// Thực thi sự kiện ở Main-Thread ngay lập tức
    /// </summary>
    /// <param name="action"></param>
    public void QueueOnMainThread(Action action)
    {
        /// Thêm vào danh sách thực thi
        this.waitToBeProcess.Enqueue(action);
    }

    /// <summary>
    /// Thiết lập lại tầm nhìn của Camera về mặc định
    /// </summary>
    public void ResetCameraFieldOfView()
    {
        this.MainCamera.orthographicSize = this.InitCameraFieldOfView;
    }
    #endregion

    #region Render
    /// <summary>
    /// Chất lượng đồ họa
    /// </summary>
    public enum RenderQuality
    {
        /// <summary>
        /// Không có gì
        /// </summary>
        None = -1,
        /// <summary>
        /// Thấp
        /// </summary>
        Low = 0,
        /// <summary>
        /// Trung
        /// </summary>
        Medium = 1,
        /// <summary>
        /// Cao
        /// </summary>
        High = 2,
    }
    /// <summary>
    /// Chất lượng Render hiện tại
    /// </summary>
    private RenderQuality mRenderQuality = RenderQuality.None;

    /// <summary>
    /// Frame/Sec
    /// </summary>
    public float FPS { get; set; }

    /// <summary>
    /// Trả về chất lượng Render hiện tại
    /// </summary>
    /// <returns></returns>
    public RenderQuality GetRenderQuality()
    {
        return this.mRenderQuality;
    }

    /// <summary>
    /// Cập nhật chất lượng đồ họa hiển thị
    /// </summary>
    private void UpdateRenderQuality()
    {
        if (this.FPS <= 20f)
        {
            this.mRenderQuality = RenderQuality.Low;
        }
        else if (this.FPS <= 40f)
        {
            this.mRenderQuality = RenderQuality.Medium;
        }
        else
        {
            this.mRenderQuality = RenderQuality.High;
        }
    }
    #endregion
}
