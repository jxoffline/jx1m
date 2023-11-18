using FS.GameEngine.Logic;
using FS.GameEngine.Scene;
using FS.GameEngine.Network;
using FS.VLTK;
using FS.VLTK.UI;

/// <summary>
/// Quản lý màn chơi
/// </summary>
public partial class PlayZone : BasePlayZone
{
    /// <summary>
    /// Đối tượng quản lý màn chơi
    /// </summary>
    public static PlayZone Instance { get; private set; } = null;

    #region Inheritance MonoBehaviour
    /// <summary>
    /// Hàm này gọi khi đối tượng được tạo ra
    /// </summary>
    protected override void OnAwake()
	{
        base.OnAwake();
        PlayZone.Instance = this;
		
	}

    /// <summary>
    /// Hàm này gọi ở Frame đầu tiên
    /// </summary>
	protected override void OnStart()
	{
        this.EnterGame();
	}

    /// <summary>
    /// Thời điểm lần trước cập nhật thông tin mặt nhân vật
    /// </summary>
    private long lastTickRefreshRoleFace = 0;

    /// <summary>
    /// Hàm này gọi liên tục mỗi Frame
    /// </summary>
    protected override void OnUpdate()
    {
        base.OnUpdate();

        if (KTGlobal.GetCurrentTimeMilis() - this.lastTickRefreshRoleFace >= 200)
        {
            this.lastTickRefreshRoleFace = KTGlobal.GetCurrentTimeMilis();
            this.RefreshRoleFace();
        }
        
        if (this.scene != null)
        {
            this.scene.OnRenderScene();
        }
    }

    /// <summary>
    /// Hàm này gọi khi đối tượng bị hủy
    /// </summary>
    protected override void OnDestroyed()
    {
        /// Xóa toàn bộ UI
        CanvasManager canvas = Global.MainCanvas.GetComponent<CanvasManager>();
        if (canvas != null)
        {
            canvas.DestroyDynamicUI();
            canvas.DestroyOnTopUI();
            canvas.DestroyUnderLayerUI();
        }

        base.OnDestroyed();
    }
    #endregion

    /// <summary>
    /// Bắt đầu vào Game
    /// </summary>		
    public void StartPlayGame()
    {
        /// Bắt đầu Game
        GameInstance.Game.StartPlayGame();

        /// Gửi thông tin Version
        GameInstance.Game.SendVersion();
    }

    /// <summary>
    /// Bắt đầu vào Game ở Liên máy chủ
    /// </summary>
    public void EnterGameForCrossServer()
    {
        this.scene.Scene_Loaded();
        this.InitNetwork();
    }

    /// <summary>
    /// Khởi tạo màn chơi
    /// </summary>		
    protected void InitializeGameScene()
    {
        this.scene = new GScene();
        Global.Data.GameScene = this.scene;
    }

    /// <summary>
    /// Bắt đầu vào Game
    /// </summary>		
    protected void EnterGame()
    {
        this.InitializeGameInterface();
        this.InitializeGameScene();
        this.InitRadarMap();
        this.InitNetwork();
        this.LoadScene(Global.Data.RoleData.MapCode, Global.Data.RoleData.PosX, Global.Data.RoleData.PosY, Global.Data.RoleData.RoleDirection);
        this.InitFirstEnterGame();
    }

    /// <summary>
    /// Khởi tạo lúc bắt đầu vào Game
    /// </summary>
    private void InitFirstEnterGame()
    {
        this.StartPlayGame();
    }

    /// <summary>
    /// Làm rỗng bản đồ
    /// </summary>		
    protected override void ClearScene()
    {
        base.ClearScene();
    }

    /// <summary>
    /// Tải xuống bản đồ
    /// </summary>
    /// <param name="mapCode"></param>
    /// <param name="leaderX"></param>
    /// <param name="leaderY"></param>
    /// <param name="direction"></param>
    /// <param name="newLifeDeco"></param>		
    public override void LoadScene(int mapCode, int leaderX, int leaderY, double direction)
    {
        base.LoadScene(mapCode, leaderX, leaderY, direction);
        this.LoadRadarMap();
    }
}
