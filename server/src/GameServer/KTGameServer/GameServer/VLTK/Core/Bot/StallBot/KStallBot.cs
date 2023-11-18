using GameServer.Interface;
using GameServer.KiemThe.Entities;
using Server.Data;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using static GameServer.Logic.KTMapManager;

namespace GameServer.Logic
{
    /// <summary>
    /// Đối tượng Bot bán hàng
    /// </summary>
    public partial class KStallBot : IObject
    {
        #region Properties
        /// <summary>
        /// ID đối tượng
        /// </summary>
        public int RoleID { get; private set; }

        /// <summary>
        /// ID mũ
        /// </summary>
        public int HelmID { get; set; }

        /// <summary>
        /// ID áo
        /// </summary>
        public int ArmorID { get; set; }

        /// <summary>
        /// ID phi phong
        /// </summary>
        public int MantleID { get; set; }

        /// <summary>
        /// Tên cửa hàng
        /// </summary>
        public string StallName { get; set; }

        /// <summary>
        /// ID chủ nhân
        /// </summary>
        public int OwnerRoleID { get; set; }

        /// <summary>
        /// Tên chủ nhân
        /// </summary>
        public string OwnerRoleName { get; set; }

        /// <summary>
        /// Giới tính chủ nhân
        /// </summary>
        public int OwnerRoleSex { get; set; }

        /// <summary>
        /// Thời gian tồn tại
        /// </summary>
        public long LifeTime { get; set; }
        #endregion

        #region Private fields
        /// <summary>
        /// ID tự tăng
        /// </summary>
        private static int _AutoID = -1;
        #endregion

        #region Constructors
        /// <summary>
        /// Tạo mới đối tượng StallBot
        /// </summary>
        public KStallBot()
        {
            /// Tăng ID tự động
            KStallBot._AutoID = (KStallBot._AutoID + 1) % 10000007;
            /// Thiết lập ID
            this.RoleID = KStallBot._AutoID + (int) ObjectBaseID.StallBot;
        }

        /// <summary>
        /// Tạo mới đối tượng StallBot
        /// </summary>
        /// <param name="player"></param>
        public KStallBot(KPlayer player)
        {
            /// Tăng ID tự động
            KStallBot._AutoID = (KStallBot._AutoID + 1) % 10000007;
            /// Thiết lập ID
            this.RoleID = KStallBot._AutoID + (int) ObjectBaseID.StallBot;

            /// Thông tin chủ nhân
            this.OwnerRoleID = player.RoleID;
            this.OwnerRoleName = player.RoleName;
            this.OwnerRoleSex = player.RoleSex;

            /// Danh sách vị trí cần truy vấn
            KE_EQUIP_POSITION[] listEquipPos = new KE_EQUIP_POSITION[]
            {
                KE_EQUIP_POSITION.emEQUIPPOS_BODY,
                KE_EQUIP_POSITION.emEQUIPPOS_HEAD,
                KE_EQUIP_POSITION.emEQUIPPOS_MANTLE,
            };

            /// Danh sách trang bị đang mặc
            List<GoodsData> equips = player.GoodsData.FindAll(x => listEquipPos.Any(y => x.Using == (int) y));
            /// Duyệt danh sách
            for (int i = 0; i < equips.Count; i++)
            {
                GoodsData itemGD = equips[i];
                /// Loại trang bị là gì
                switch (itemGD.Using)
                {
                    case (int) KE_EQUIP_POSITION.emEQUIPPOS_BODY:
                    {
                        this.ArmorID = itemGD.GoodsID;
                        break;
                    }
                    case (int) KE_EQUIP_POSITION.emEQUIPPOS_HEAD:
                    {
                        this.HelmID = itemGD.GoodsID;
                        break;
                    }
                    case (int) KE_EQUIP_POSITION.emEQUIPPOS_MANTLE:
                    {
                        this.MantleID = itemGD.GoodsID;
                        break;
                    }
                }
            }
        }
        #endregion

        #region Implements - IObject
        /// <summary>
        /// Loại đối tượng
        /// </summary>
        public ObjectTypes ObjectType
        {
            get
            {
                return ObjectTypes.OT_STALLBOT;
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
        /// Tọa độ thực
        /// </summary>
        public Point CurrentPos { get; set; }

        /// <summary>
        /// ID bản đồ hiện tại
        /// </summary>
        public int CurrentMapCode { get; set; } = -1;

        /// <summary>
        /// ID phụ bản hiện tại
        /// </summary>
        public int CurrentCopyMapID { get; set; } = -1;

        /// <summary>
        /// Hướng quay hiện tại
        /// </summary>
        public KiemThe.Entities.Direction CurrentDir { get; set; } = KiemThe.Entities.Direction.DOWN;
        #endregion
    }
}
