using FS.GameEngine.Logic;
using FS.VLTK.UI;
using FS.VLTK.UI.Main;
using UnityEngine;

/// <summary>
/// Quản lý các khung giao diện trong màn chơi
/// </summary>
public partial class PlayZone
{
    #region SuperToolTip
    /// <summary>
    /// Khung thông tin vật phẩm hoặc kỹ năng
    /// </summary>
    public UISuperToolTip UISuperToolTip { get; protected set; }

    /// <summary>
    /// Mở khung SuperToolTip
    /// </summary>
    public void OpenUISuperToolTip()
    {
        if (this.UISuperToolTip != null)
        {
            return;
        }

        CanvasManager canvas = Global.MainCanvas.GetComponent<CanvasManager>();
        this.UISuperToolTip = canvas.LoadUIPrefab<UISuperToolTip>("MainGame/UISuperToolTip");
        canvas.AddUI(this.UISuperToolTip);

        this.UISuperToolTip.Close = this.CloseUISuperToolTip;
    }

    /// <summary>
    /// Đóng khung SuperToolTip
    /// </summary>
    public void CloseUISuperToolTip()
    {
        if (this.UISuperToolTip != null)
        {
            GameObject.Destroy(this.UISuperToolTip.gameObject);
            this.UISuperToolTip = null;
        }
    }
    #endregion
}
