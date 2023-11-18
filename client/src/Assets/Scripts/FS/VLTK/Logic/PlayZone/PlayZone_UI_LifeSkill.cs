using FS.GameEngine.Logic;
using FS.VLTK.Network;
using FS.VLTK.UI;
using FS.VLTK.UI.Main;
using UnityEngine;

/// <summary>
/// Quản lý các khung giao diện trong màn chơi
/// </summary>
public partial class PlayZone
{
    #region Kỹ năng sống
    /// <summary>
    /// Khung kỹ năng sống
    /// </summary>
    public UICrafting UICrafting { get; protected set; } = null;

    /// <summary>
    /// Mở khung kỹ năng sống
    /// </summary>
    public void ShowUICrafting()
    {
        /// Nếu đang hiện khung
        if (this.UICrafting != null)
        {
            return;
        }

        /// Tạo khung
        CanvasManager canvas = Global.MainCanvas.GetComponent<CanvasManager>();
        this.UICrafting = canvas.LoadUIPrefab<UICrafting>("MainGame/UICrafting");
        canvas.AddUI(this.UICrafting);

        this.UICrafting.Close = this.CloseUICrafting;
        this.UICrafting.Craft = (recipe) => {
            /// Gửi yêu cầu chế đồ lên Server
            KT_TCPHandler.SendCraftItem(recipe.ID);
        };
    }

    /// <summary>
    /// Đóng khung kỹ năng sống
    /// </summary>
    public void CloseUICrafting()
    {
        /// Nếu đang hiện khung
        if (this.UICrafting != null)
        {
            GameObject.Destroy(this.UICrafting.gameObject);
            this.UICrafting = null;
        }
    }
    #endregion
}
