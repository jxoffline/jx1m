using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static FS.VLTK.Entities.Enum;

namespace FS.VLTK.Factory.Animation
{
    /// <summary>
    /// Quản lý động tác
    /// </summary>
    public partial class CharacterAnimationManager
    {
        /// <summary>
        /// Động tác
        /// </summary>
        public class PlayerAction
        {
            /// <summary>
            /// Loại động tác
            /// </summary>
            public PlayerActionType Type { get; set; }

            /// <summary>
            /// Tên động tác khi cưỡi
            /// </summary>
            public string Ride { get; set; }

            /// <summary>
            /// Tên động tác khi ở trạng thái thường
            /// </summary>
            public string Normal { get; set; }
        }

        public class ActionByWeapon
        {
            /// <summary>
            /// ID vũ khí
            /// </summary>
            public string WeaponID { get; set; }

            /// <summary>
            /// Danh sách động tác với 2 trạng thái tương ứng
            /// </summary>
            public Dictionary<PlayerActionType, PlayerAction> Actions { get; set; }
        }

        /// <summary>
        /// Danh sách tên động tác theo loại và vũ khí
        /// </summary>
        public Dictionary<string, ActionByWeapon> ActionNames { get; set; } = new Dictionary<string, ActionByWeapon>();

        /// <summary>
        /// Lấy tên động tác dựa theo loại
        /// </summary>
        /// <param name="actionType"></param>
        /// <param name="weaponID"></param>
        /// <param name="isRide"></param>
        /// <returns></returns>
        public string GetActionName(PlayerActionType actionType, string weaponID, bool isRide)
        {
            /// Nếu tên động tác theo vũ khí không tồn tại
            if (!this.ActionNames.TryGetValue(weaponID, out ActionByWeapon actionInfo))
            {
                return "";
            }

            /// Nếu động tác không tồn tại
            if (!actionInfo.Actions.TryGetValue(actionType, out PlayerAction actionName))
            {
                return "";
            }

            /// Trả về tên động tác tương ứng
            return isRide ? actionName.Ride : actionName.Normal;
        }
    }
}
