using GameServer.Interface;
using GameServer.KiemThe;
using GameServer.KiemThe.Entities;
using Server.Data;
using System.Collections.Generic;
using System.Windows;
using static GameServer.Logic.KTMapManager;

namespace GameServer.Logic
{
    /// <summary>
    /// Định nghĩa vật phẩm rơi dưới đất
    /// </summary>
    public partial class KGoodsPack : IObject
    {
        /// <summary>
        /// ID tự tăng
        /// </summary>
        private static int AutoID = -1;

        /// <summary>
        /// ID tự tăng
        /// </summary>
        public int ID { get; private set; }

        /// <summary>
        /// Danh sách chủ nhân (nếu có)
        /// </summary>
        public HashSet<int> OwnerRoleIDs { get; set; }

        /// <summary>
        /// Đối tượng rơi vật phẩm (chỉ phục vụ ghi LOG)
        /// </summary>
        public GameObject Source { get; set; }

        /// <summary>
        /// Thông tin vật phẩm sẽ rơi
        /// </summary>
        public GoodsData GoodsData { get; private set; }

        /// <summary>
        /// Thời điểm tạo ra
        /// </summary>
        public long InitTics { get; private set; }

        /// <summary>
        /// Có còn tồn tại không
        /// </summary>
        public bool IsAlive { get; private set; }

        /// <summary>
        /// Kích hoạt ghi Log khi nhặt không (chỉ phục vụ ghi LOG)
        /// </summary>
        public bool EnableWriteLogOnPickUp { get; set; } = false;

        /// <summary>
        /// Thời gian tồn tại còn lại
        /// </summary>
        public long LifeTime
        {
            get
            {
                return KTGlobal.GetCurrentTimeMilis() - this.InitTics;
            }
        }

        /// <summary>
        /// Tạo mới vật phẩm rơi dưới đất
        /// </summary>
        /// <param name="goods">Thông tin vật phẩm rơi (nếu DbID là -1 thì khi nhặt sẽ tạo vật phẩm mới)</param>
        public KGoodsPack(GoodsData goods)
        {
            /// Tăng ID tự động lên
            KGoodsPack.AutoID = (KGoodsPack.AutoID + 1) % 10000007;
            /// Thiết lập ID tự động
            this.ID = KGoodsPack.AutoID + (int) ObjectBaseID.GoodsPack;

            /// Lưu lại thông tin vật phẩm
            this.GoodsData = goods;
            /// Thời điểm tạo ra
            this.InitTics = KTGlobal.GetCurrentTimeMilis();
            /// Đánh dấu có còn tồn tại
            this.IsAlive = true;
        }

        #region Implements IObject
        /// <summary>
        /// Loại đối tượng
        /// </summary>
        public ObjectTypes ObjectType
        {
            get
            {
                return ObjectTypes.OT_GOODSPACK;
            }
        }

        /// <summary>
        /// Tọa độ lưới
        /// </summary>
        public Point CurrentGrid
        {
            get
            {
                GameMap gameMap = KTMapManager.Find(this.CurrentMapCode);
                return new Point((int) (this.CurrentPos.X / gameMap.MapGridWidth), (int) (this.CurrentPos.Y / gameMap.MapGridHeight));
            }

            set
            {
                GameMap gameMap = KTMapManager.Find(this.CurrentMapCode);
                this.CurrentPos = new Point((int) (value.X * gameMap.MapGridWidth + gameMap.MapGridWidth / 2), (int) (value.Y * gameMap.MapGridHeight + gameMap.MapGridHeight / 2));
            }
        }

        /// <summary>
        /// Vị trí hiện tại
        /// </summary>
        public Point CurrentPos { get; set; }

        /// <summary>
        /// ID bản đồ hiện tại
        /// </summary>
        public int CurrentMapCode { get; set; }

        /// <summary>
        /// ID phụ bản hiện tại
        /// </summary>
        public int CurrentCopyMapID { get; set; }

        /// <summary>
        /// Hướng quay (đéo dùng ở đây)
        /// </summary>
        public KiemThe.Entities.Direction CurrentDir { get; set; } = KiemThe.Entities.Direction.DOWN;
        #endregion
    }
}
