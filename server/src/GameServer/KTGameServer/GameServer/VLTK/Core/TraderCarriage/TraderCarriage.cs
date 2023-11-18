using GameServer.KiemThe;
using GameServer.KiemThe.Entities;
using System.Collections.Generic;
using System.Windows;
using static GameServer.KiemThe.GameEvents.CargoCarriage.CargoCarriage;
using static GameServer.Logic.KTMapManager;

namespace GameServer.Logic
{
    /// <summary>
    /// Đối tượng xe tiêu
    /// </summary>
    public partial class TraderCarriage : GameObject
    {
        /// <summary>
        /// ID tự tăng
        /// </summary>
        private static int _AutoID = 0x7F470000;

        /// <summary>
        /// Base ID của xe tiêu để phân biệt các Object khác
        /// </summary>
        public const int TraderCarriageBaseID = 0x7F470000;

        /// <summary>
        /// Chủ nhân
        /// </summary>
        public KPlayer Owner { get; private set; }

        /// <summary>
        /// Thời điểm tạo ra
        /// </summary>
        public long CreateTicks { get; private set; }

        /// <summary>
        /// Thời gian tồn tại
        /// </summary>
        public long DurationTicks { get; set; }

        /// <summary>
        /// ID Res
        /// </summary>
        public int ResID { get; set; }

        /// <summary>
        /// Tầm nhìn
        /// Khi đến đâu, sẽ quét toàn bộ quái xung quanh sẽ lao vào đập xe tiêu
        /// </summary>
        public int Vision { get; set; }

        /// <summary>
        /// Loại xe tiêu
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// Danh sách quãng đường di chuyển
        /// Key: ID bản đồ
        /// Value: Vị trí đích đến
        /// </summary>
        public Queue<KeyValuePair<int, UnityEngine.Vector2Int>> Paths { get; set; }

        /// <summary>
        /// Đối tượng Pet
        /// </summary>
        /// <param name="player"></param>
        /// <param name="petData"></param>
        public TraderCarriage(KPlayer player)
        {
            /// Tăng ID tự động
            TraderCarriage._AutoID = (TraderCarriage._AutoID + 1) % 1000007;
            /// Thiết lập ID
            this.RoleID = TraderCarriage._AutoID + TraderCarriage.TraderCarriageBaseID;

            /// Chủ nhân
            this.Owner = player;
            /// Thiết lập xe tiêu hiện tại
            this.Owner.CurrentTraderCarriage = this;

            /// Đánh dấu thời điểm tạo
            this.CreateTicks = KTGlobal.GetCurrentTimeMilis();

            /// Tạo mới Buff Tree
            this.Buffs = new BuffTree(this);

            /// Thiết lập vị trí là vị trí của chủ nhân
            this.CurrentPos = player.CurrentPos;

            /// Bản đồ
            GameMap gameMap = KTMapManager.Find(this.CurrentMapCode);
            /// Cập nhật vị trí đối tượng vào Map
            gameMap.Grid.MoveObject((int) this.CurrentPos.X, (int) this.CurrentPos.Y, this);
        }

        #region Kế thừa IObject

        /// <summary>
        /// Loại đối tượng
        /// </summary>
        public override ObjectTypes ObjectType
        {
            get { return ObjectTypes.OT_TRADER_CARRIAGE; }
        }

        /// <summary>
        /// Vị trí hiện tại (tọa độ lưới)
        /// </summary>
        public override Point CurrentGrid { get; set; }

        /// <summary>
        /// Vị trí hiện tại
        /// </summary>
        private Point _CurrentPos = new Point(0, 0);

        /// <summary>
        /// Vị trí hiện tại
        /// </summary>
        public override Point CurrentPos
        {
            get
            {
                return _CurrentPos;
            }

            set
            {
                GameMap gameMap = KTMapManager.Find(this.CurrentMapCode);
                this.CurrentGrid = new Point((int) (value.X / gameMap.MapGridWidth), (int) (value.Y / gameMap.MapGridHeight));
                this._CurrentPos = value;
            }
        }

        /// <summary>
        /// ID map hiện tại
        /// </summary>
        public override int CurrentMapCode
        {
            get
            {
                /// Toác
                if (this.Owner == null)
                {
                    return -1;
                }
                return this.Owner.CurrentMapCode;
            }
        }

        /// <summary>
        /// ID phụ bản hiện tại
        /// </summary>
        public override int CurrentCopyMapID
        {
            get
            {
                /// Toác
                if (this.Owner == null)
                {
                    return -1;
                }
                return this.Owner.CurrentCopyMapID;
            }
        }

        /// <summary>
        /// Hướng hiện tại
        /// </summary>
        public override KiemThe.Entities.Direction CurrentDir
        {
            get;
            set;
        }

        #endregion
    }
}
