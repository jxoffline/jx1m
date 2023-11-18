using GameServer.KiemThe.Entities;
using System.Windows;
using static GameServer.Logic.KTMapManager;

namespace GameServer.Logic
{
    /// <summary>
    /// Đối tượng BOT biểu diễn
    /// </summary>
    public partial class KDecoBot : GameObject
    {
        /// <summary>
        /// ID tự động
        /// </summary>
        private static int AutoID = -1;

        /// <summary>
        /// Giới tính
        /// </summary>
        public int RoleSex { get; set; }

        /// <summary>
        /// ID vũ khí
        /// </summary>
        public int WeaponID { get; set; }

        /// <summary>
        /// Ngũ hành vũ khí
        /// </summary>
        public int WeaponSeries { get; set; }

        /// <summary>
        /// Cấp cường hóa vũ khí
        /// </summary>
        public int WeaponEnhanceLevel { get; set; }

        /// <summary>
        /// ID áo
        /// </summary>
        public int ArmorID { get; set; }

        /// <summary>
        /// ID mũ
        /// </summary>
        public int HelmID { get; set; }

        /// <summary>
        /// ID ngựa
        /// </summary>
        public int HorseID { get; set; }

        /// <summary>
        /// ID phi phong
        /// </summary>
        public int MantleID { get; set; }

        /// <summary>
        /// Vị trí đứng ban đầu
        /// </summary>
        public Point InitPos { get; set; }

        /// <summary>
        /// Loại đối tượng
        /// </summary>
        public override ObjectTypes ObjectType
        {
            get
            {
                return ObjectTypes.OT_BOT;
            }
        }

        /// <summary>
        /// Số lượng người chơi xung quanh
        /// </summary>
        public int VisibleClientsNum { get; set; }

        /// <summary>
        /// ID bản đồ hiện tại
        /// </summary>
        public int MapCode { get; set; }
        /// <summary>
        /// ID bản đồ hiện tại
        /// </summary>
        public override int CurrentMapCode
        {
            get
            {
                return this.MapCode;
            }
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
        /// Tạo đối tượng BOT mới
        /// </summary>
        public KDecoBot() : base()
        {
            KDecoBot.AutoID = (KDecoBot.AutoID + 1) % 100000007;
            this.RoleID = KDecoBot.AutoID + (int) ObjectBaseID.DecoBot;

            /// Tạo mới Buff
            this.Buffs = new BuffTree(this);
            /// Thực hiện động tác đứng
            this.m_eDoing = KE_NPC_DOING.do_stand;
        }
    }
}
