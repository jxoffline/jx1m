using GameServer.KiemThe.Entities;
using System;

namespace GameServer.Logic
{
    /// <summary>
    /// Đối tượng quái vật
    /// </summary>
    public partial class Monster : GameObject, IDisposable
    {
        /// <summary>
        /// Tạo mới đối tượng quái
        /// </summary>
        public Monster()
        {
            /// Tăng ID tự động
            this.Buffs = new BuffTree(this);
        }
    }
}