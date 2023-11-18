using FS.GameEngine.Logic;
using FS.GameEngine.Network;
using FS.VLTK.Network;
using FS.VLTK.UI;
using FS.VLTK.UI.Main;
using UnityEngine;

/// <summary>
/// Quản lý các khung giao diện trong màn chơi
/// </summary>
public partial class PlayZone
{
    #region Bạn bè
    /// <summary>
    /// Khung bạn bè
    /// </summary>
    public UIFriendBox UIFriendBox { get; protected set; } = null;

    /// <summary>
    /// Hiển thị khung bạn bè
    /// </summary>
    public void ShowUIFriendBox()
    {
        /// Nếu đang hiện khung
        if (this.UIFriendBox != null)
        {
            return;
        }

        /// Tạo khung
        CanvasManager canvas = Global.MainCanvas.GetComponent<CanvasManager>();
        this.UIFriendBox = canvas.LoadUIPrefab<UIFriendBox>("MainGame/UIFriendBox");
        canvas.AddUI(this.UIFriendBox);

        this.UIFriendBox.Close = this.CloseUIFriendBox;
        this.UIFriendBox.AddFriend = (rd) => {
            /// Gửi gói tin thêm bạn
            GameInstance.Game.SpriteAddFriend(-1, rd.RoleID, rd.RoleName, 0);
        };
        this.UIFriendBox.RemoveFriend = (friendData) => {
            /// Gửi gói tin xóa bạn
            GameInstance.Game.SpriteRemoveFriend(friendData.DbID);
        };
        this.UIFriendBox.RejectFriend = (rd) => {
            /// Gửi gói tin từ chối yêu cầu thêm bạn
            GameInstance.Game.SpriteRejectFriend(rd.RoleID);
        };
        this.UIFriendBox.BrowseFriendInfo = (friendData) => {
            /// Gửi gói tin yêu cầu kiểm tra thông tin người chơi
            KT_TCPHandler.SendCheckPlayerInfo(friendData.OtherRoleID);
        };
    }

    /// <summary>
    /// Đóng khung bạn bè
    /// </summary>
    public void CloseUIFriendBox()
    {
        /// Nếu đang hiện khung
        if (this.UIFriendBox != null)
        {
            GameObject.Destroy(this.UIFriendBox.gameObject);
            this.UIFriendBox = null;
        }
    }
    #endregion
}
