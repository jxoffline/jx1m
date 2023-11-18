using FS.GameEngine.Logic;
using FS.GameEngine.Sprite;
using FS.VLTK.Factory;
using FS.VLTK.Logic.Settings;
using FS.VLTK.Network;
using System.Collections.Generic;
using System.Linq;

namespace FS.VLTK.Logic
{
    /// <summary>
    /// Chức năng liên quan tổ đội
    /// </summary>
    public partial class KTAutoFightManager
    {
        /// <summary>
        /// Thực hiện tự mời người chơi khác vào nhóm
        /// </summary>
        private void ProcessInviteToTeam()
        {
            /// Nếu không có thiết lập tự mời người chơi khác vào nhóm
            if (!KTAutoAttackSetting.Config.PK.AutoInviteToTeam)
            {
                return;
            }

            /// Nếu chưa đến thời gian kiểm tra thì bỏ qua
            if (KTGlobal.GetCurrentTimeMilis() - this.AutoFightLastCheckNearbyPlayersToInviteToTeamEveryTick < this.AutoFight_CheckNearbyPlayersToInviteToTeamEveryTick)
            {
                return;
            }

            /// Đánh dấu thời gian
            this.AutoFightLastCheckNearbyPlayersToInviteToTeamEveryTick = KTGlobal.GetCurrentTimeMilis();

            /// Nếu bản thân chưa có nhóm thì bỏ qua
            if (Global.Data.RoleData.TeamID == -1)
            {
                return;
            }
            /// Nếu nhóm đã đầy thì bỏ qua
            else if (Global.Data.Teammates != null && Global.Data.Teammates.Count >= 6)
            {
                return;
            }

            /// Tìm danh sách người chơi xung quanh chưa có nhóm
            List<GSprite> players = KTObjectsManager.Instance.FindObjects<GSprite>(x => x.SpriteType == GSpriteTypes.Other && x.RoleData.TeamID == -1).ToList();
            foreach (GSprite player in players)
            {
                KT_TCPHandler.SendInviteToTeam(player.RoleID);
            }
        }
    }
}
