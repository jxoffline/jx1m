using FS.GameEngine.Logic;
using FS.GameEngine.Network;
using FS.VLTK.Network;
using FS.VLTK.UI;
using FS.VLTK.UI.Main;
using FS.VLTK.UI.Main.MainUI;
using UnityEngine;

/// <summary>
/// Quản lý các khung giao diện trong màn chơi
/// </summary>
public partial class PlayZone
{
    #region MiniTaskAndTeamFrame
    /// <summary>
    /// Khung MiniTaskBox và MiniTeamFrame
    /// </summary>
    public UIMiniTaskAndTeamFrame UIMiniTaskAndTeamFrame { get; protected set; }

    /// <summary>
    /// Tạo khung MiniTaskBox và MiniTeamFrame
    /// </summary>
    protected void InitMiniTaskAndTeamFrame()
    {
        if (this.UIMiniTaskAndTeamFrame != null)
        {
            return;
        }

        CanvasManager canvas = Global.MainCanvas.GetComponent<CanvasManager>();
        this.UIMiniTaskAndTeamFrame = canvas.LoadUIPrefab<UIMiniTaskAndTeamFrame>("MainGame/MainUI/UIMiniTaskAndTeamFrame");
        canvas.AddMainUI(this.UIMiniTaskAndTeamFrame);

        /// Mini Task Box

        this.UIMiniTaskAndTeamFrame.UITeamFrame.OpenTeamBox = () => {
            KT_TCPHandler.SendGetTeamInfo();
        };
        //this.UIMiniTaskAndTeamFrame.UITeamFrame.ShowTeammateDetails = ()
    }
    #endregion

    #region Khung nhiệm vụ
    /// <summary>
    /// Khung nhiệm vụ
    /// </summary>
    public UITaskBox UITaskBox { get; protected set; }

    /// <summary>
    /// Hiển thị khung nhiệm vụ
    /// </summary>
    public void ShowUITaskBox()
    {
        if (this.UITaskBox != null)
        {
            return;
        }
        this.UITaskBox = CanvasManager.Instance.LoadUIPrefab<UITaskBox>("MainGame/UITaskBox");
        CanvasManager.Instance.AddUI(this.UITaskBox);
        this.UITaskBox.Close = this.CloseUITaskBox;
        this.UITaskBox.AbandonTask = (taskData) => {
            GameInstance.Game.SpriteAbandonTask(taskData.DbID, taskData.DoingTaskID);
        };
    }

    /// <summary>
    /// Đóng khung nhiệm vụ
    /// </summary>
    public void CloseUITaskBox()
    {
        if (this.UITaskBox != null)
        {
            GameObject.Destroy(this.UITaskBox.gameObject);
            this.UITaskBox = null;
        }
    }
    #endregion
}
