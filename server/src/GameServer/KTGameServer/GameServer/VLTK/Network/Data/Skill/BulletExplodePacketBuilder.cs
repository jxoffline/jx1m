using GameServer.Logic;
using Server.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.KiemThe.Network.Entities
{
    /// <summary>
    /// Đối tượng xây gói tin đạn nổ về Client
    /// </summary>
    public class BulletExplodePacketBuilder
    {
        /// <summary>
        /// Danh sách gói tin biểu diễn đạn nổ
        /// </summary>
        private readonly List<G2C_BulletExplode> explodes = new List<G2C_BulletExplode>();

        /// <summary>
        /// Thêm gói tin biểu diễn đạn nổ vào danh sách
        /// </summary>
        /// <param name="bulletID">ID đạn</param>
        /// <param name="resID">ID Res của đạn</param>
        /// <param name="pos">Vị trí nổ</param>
        /// <param name="isDestroy">Có xóa đạn luôn không</param>
        /// <param name="delay">Thời gian Delay trước khi thao tác</param>
        /// <param name="target">Mục tiêu nổ</param>
        public void Append(int bulletID, int resID, UnityEngine.Vector2 pos, float delay, GameObject target)
        {
            G2C_BulletExplode bulletExplode = new G2C_BulletExplode()
            {
                BulletID = bulletID,
                ResID = resID,
                PosX = (int) pos.x,
                PosY = (int) pos.y,
                Delay = delay,
                TargetID = target != null ? target.RoleID : -1,
            };
            this.explodes.Add(bulletExplode);
        }

        /// <summary>
        /// Xây gói tin
        /// </summary>
        /// <returns></returns>
        public List<G2C_BulletExplode> Build()
        {
            return this.explodes;
        }
    }
}
