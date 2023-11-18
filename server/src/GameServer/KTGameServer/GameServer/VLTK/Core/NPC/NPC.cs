using GameServer.Interface;
using GameServer.KiemThe.Entities;
using System;
using System.Windows;
using static GameServer.Logic.KTMapManager;

namespace GameServer.Logic
{
    /// <summary>
    /// Đối tượng NPC
    /// </summary>
    public class NPC : IObject
    {
        /// <summary>
        /// ID tự tăng
        /// </summary>
        private static int _AutoID = 0;

        /// <summary>
        /// Đối tượng NPC
        /// </summary>
        public NPC()
        {
            /// Tăng ID tự động
            NPC._AutoID = (NPC._AutoID + 1) % 10000007;
            /// Thiết lập ID
            this.NPCID = NPC._AutoID + (int) ObjectBaseID.NPC;
        }

        /// <summary>
        /// ID NPC
        /// </summary>
        public int NPCID { get; private set; }

        /// <summary>
        /// ID của files cấu hình
        /// </summary>
        public int ResID { get; set; }

        /// <summary>
        /// ID Map
        /// </summary>
        public int MapCode { get; set; } = -1;

        /// <summary>
        /// ID phụ bản
        /// </summary>
        public int CopyMapID { get; set; } = -1;

        /// <summary>
        /// ID Script điều khiển
        /// </summary>
        public int ScriptID { get; set; }

        /// <summary>
        /// Tên NPC
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Tên ở bản đồ nhỏ
        /// </summary>
        public string MinimapName { get; set; }

        /// <summary>
        /// Có hiển thị ở bản đồ khu vực không
        /// </summary>
        public bool VisibleOnMinimap { get; set; }

        /// <summary>
        /// Danh hiệu NPC
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Tag
        /// </summary>
        public string Tag { get; set; } = "";

        /// <summary>
        /// Sự kiện khi NPC được Click
        /// </summary>
        public Action<KPlayer> Click { get; set; }

        #region Kế thừa IObject

        /// <summary>
        /// Loại đối tượng
        /// </summary>
        public ObjectTypes ObjectType
        {
            get { return ObjectTypes.OT_NPC; }
        }

        /// <summary>
        /// Vị trí hiện tại (tọa độ lưới)
        /// </summary>
        public Point CurrentGrid { get; set; }

        /// <summary>
        /// Vị trí hiện tại
        /// </summary>
        private Point _CurrentPos = new Point(0, 0);

        /// <summary>
        /// Vị trí hiện tại
        /// </summary>
        public Point CurrentPos
        {
            get
            {
                return _CurrentPos;
            }

            set
            {
                GameMap gameMap = KTMapManager.Find(this.MapCode);
                this.CurrentGrid = new Point((int) (value.X / gameMap.MapGridWidth), (int) (value.Y / gameMap.MapGridHeight));
                this._CurrentPos = value;
            }
        }

        /// <summary>
        /// ID map hiện tại
        /// </summary>
        public int CurrentMapCode
        {
            get
            {
                return this.MapCode;
            }
        }

        /// <summary>
        /// ID phụ bản hiện tại
        /// </summary>
        public int CurrentCopyMapID
        {
            get
            {
                return this.CopyMapID;
            }
        }

        /// <summary>
        /// Hướng hiện tại
        /// </summary>
        public KiemThe.Entities.Direction CurrentDir
        {
            get;
            set;
        }

        #endregion

        /// <summary>
        /// Hiển thị NPC
        /// </summary>
        public bool ShowNpc { get; set; } = true;
    }
}
