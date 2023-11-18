using FS.GameEngine.Logic;
using Server.Data;
using Server.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FS.VLTK.Network
{
    /// <summary>
    /// Đối tượng quản lý các gói tin liên quan đến môn phái và nhánh tu luyện của người chơi
    /// </summary>
    public static class KTFactionSocketManager
    {
        /// <summary>
        /// Nhận gói tin thông báo môn phái hoặc nhánh của người chơi đã thay đổi
        /// </summary>
        /// <param name="cmdID"></param>
        /// <param name="bytes"></param>
        /// <param name="length"></param>
        public static void ReceiveFactionRouteChanged(int cmdID, byte[] bytes, int length)
        {
            RoleFactionChanged roleFaction = DataHelper.BytesToObject<RoleFactionChanged>(bytes, 0, length);

            /// Nếu là Leader
            if (roleFaction.RoleID == Global.Data.RoleData.RoleID)
            {
                Global.Data.RoleData.FactionID = roleFaction.FactionID;
                Global.Data.RoleData.SubID = roleFaction.RouteID;
                PlayZone.Instance.UpdateLeaderRoleFaction();

                /// Hủy thiết lập QuickKey
                Global.Data.RoleData.MainQuickBarKeys = "-1|-1|-1|-1|-1|-1|-1|-1|-1|-1|-1|-1|-1|-1|-1|-1|-1|-1|-1|-1";
                PlayZone.Instance.UIBottomBar.UISkillBar.RefreshSkillIcon();
                /// Hủy thiết lập AruaKey
                Global.Data.RoleData.OtherQuickBarKeys = "-1_0";
                PlayZone.Instance.UIBottomBar.UISkillBar.RefreshAruaIcon();
            }
            /// Nếu là người chơi khác
            else
            {
                PlayZone.Instance.NotifyRoleFace(roleFaction.RoleID, -1);
            }
        }
    }
}
