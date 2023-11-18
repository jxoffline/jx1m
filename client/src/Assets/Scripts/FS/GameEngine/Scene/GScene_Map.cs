using FS.GameEngine.Logic;
using FS.GameEngine.Network;
using FS.GameEngine.Teleport;

namespace FS.GameEngine.Scene
{
	/// <summary>
	/// Quản lý chuyển Map, thông tin bản đồ
	/// </summary>
	public partial class GScene
    {
        #region Chuyển Map
        /// <summary>
        /// Chuyển bản đồ theo ID của điểm truyền tống
        /// </summary>
        /// <param name="teleportCode"></param>
        public void ToMapConversionByTeleportCode(int teleportCode)
        {
            GTeleport teleport = this.FindName(string.Format("Teleport{0}", teleportCode)) as GTeleport;
            if (teleport == null)
            {
                KTDebug.LogError("Not found Teleport" + teleportCode + ".");
                return;
            }

            /// ID bản đồ
            int mapCode = teleport.To;
            /// Vị trí đích X
            int leaderX = (int)(teleport.ToX);
            /// Vị trí đích Y
            int leaderY = (int)(teleport.ToY);

            ///// Nếu tài nguyên bản đồ tồn tại
            //if (KTResourceChecker.IsMapResExist(mapCode))
            //{
                /// Thực hiện động tác đứng
                this.Leader.DoStand();
                /// Gửi yêu cầu chuyển bản đồ về Server
                GameInstance.Game.SpriteMapConversion(teleportCode, mapCode, leaderX, leaderY);
            //}
        }

        /// <summary>
        /// Chuyển bản đồ
        /// </summary>
        /// <param name="mapCode"></param>
        /// <param name="mapX"></param>
        /// <param name="mapY"></param>
        /// <param name="direction"></param>
        /// <param name="relife"></param>
        public void ToMapConversionByMapCode(int mapCode, int mapX, int mapY, int direction, int relife)
        {
            if (relife > 0)
            {
                Global.Data.RoleData.CurrentHP = Global.Data.RoleData.MaxHP;
                Global.Data.RoleData.CurrentMP = Global.Data.RoleData.MaxMP;
                Global.Data.RoleData.CurrentStamina = Global.Data.RoleData.MaxStamina;

                this.Leader.DoStand();
            }
            int leaderX = mapX;
            int leaderY = mapY;

            GameInstance.Game.SpriteMapConversion(-1, mapCode, leaderX, leaderY);
        }
        #endregion
    }
}
