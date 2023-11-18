using System;
using System.Xml.Linq;
using System.Collections.Generic;
using FS.Drawing;
using FS.GameEngine.Logic;
using FS.GameEngine.Scene;
using Server.Tools;
using FS.VLTK.Loader;
using FS.VLTK.Factory;
using UnityEngine;

/// <summary>
/// Quản lý màn chơi
/// </summary>
public class BasePlayZone : TTMonoBehaviour
{
    #region Define
    #endregion

    #region Inheritance
    /// <summary>
    /// Đối tượng bản đồ hiện tại
    /// </summary>
    protected GScene scene = null;
    #endregion

    #region Properties
    /// <summary>
    /// Kiểm tra tốc độ
    /// </summary>
    public static bool IsSpeedCheck { get; set; } = true;

    /// <summary>
    /// Số PING đã gửi
    /// </summary>
    public static int InWaitPingCount { get; set; }

    /// <summary>
    /// Thời gian hết hạn PING
    /// </summary>
    public static int PING_TIMEOUT { get; set; } = 20000;
    #endregion


    #region Core MonoBehaviour
    /// <summary>
    /// Hàm này gọi khi đối tượng được tạo ra
    /// </summary>
    private void Awake()
    {
        this.OnAwake();
    }

    /// <summary>
    /// Hàm này gọi ở Frame đầu tiên
    /// </summary>
    private void Start()
    {
        this.OnStart();
    }

    /// <summary>
    /// Hàm này gọi liên tục mỗi Frame
    /// </summary>
    private void Update()
    {
        this.OnUpdate();
    }

    /// <summary>
    /// Hàm này gọi khi đối tượng bị hủy
    /// </summary>
    private void OnDestroy()
    {
        this.OnDestroyed();
    }
    #endregion

    #region Make inheritance methods
    /// <summary>
    /// Hàm này có thể kế thừa ở lớp con, thực thi ở hàm Awake trong MonoBehaviour
    /// </summary>
    protected virtual void OnAwake()
    {

    }

    /// <summary>
    /// Hàm này có thể kế thừa ở lớp con, thực thi ở hàm Start trong MonoBehaviour
    /// </summary>
    protected virtual void OnStart()
    {

    }

    /// <summary>
    /// Hàm này có thể kế thừa ở lớp con, thực thi ở hàm Update trong MonoBehaviour
    /// </summary>
    protected virtual void OnUpdate()
    {

    }

    /// <summary>
    /// Hàm này có thể kế thừa ở lớp con, thực thi ở hàm OnDestroy trong MonoBehaviour
    /// </summary>
    protected virtual void OnDestroyed()
    {

    }
    #endregion

    #region Truyền tống
    /// <summary>
    /// Định nghĩa sự kiện truyền tống theo vị trí tới
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>		
    public void MapConversion(int toMapCode, int mapX, int mapY, int direction, int relife)
    {
        if (Global.Data.WaitingForMapChange)
        {
            return;
        }

        //Global.Data.WaitingForMapChange = true;
        this.scene.ToMapConversionByMapCode(toMapCode, mapX, mapY, direction, relife);
    }
    #endregion

    #region Quản lý bản đồ
    /// <summary>
    /// Làm trống bản đồ
    /// </summary>		
    protected virtual void ClearScene()
    {
		if (this.scene != null)
		{
            this.scene.ClearScene();
		}
    }

    /// <summary>
    /// Tải xuống bản đồ tương ứng
    /// </summary>
    /// <param name="mapCode"></param>
    /// <param name="leaderX"></param>
    /// <param name="leaderY"></param>
    /// <param name="direction"></param>
    public virtual void LoadScene(int mapCode, int leaderX, int leaderY, double direction)
    {
        this.ClearScene();
        this.scene.LoadScene(mapCode, leaderX, leaderY, direction);
    }
    #endregion

    #region Hủy
    /// <summary>
    /// Hủy đối tượng
    /// </summary>
    public void Destroy()
    {
        GameObject.Destroy(this.gameObject);
    }
    #endregion
}
