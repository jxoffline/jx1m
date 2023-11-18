using FS.Drawing;
using UnityEngine;
using FS.GameEngine.Logic;
using FS.GameEngine.Interface;
using FS.VLTK.Control.Component;
using FS.VLTK;
using static FS.VLTK.Entities.Enum;
using FS.VLTK.Factory;

namespace FS.GameEngine.GoodsPack
{
    /// <summary>
    /// Đối tượng vật phẩm rơi ở Map
    /// </summary>
    public class GGoodsPack : IObject
    {
        /// <summary>
        /// Thời gian tồn tại tối đa
        /// </summary>
        public const int GoodsPackKeepTimes = 60000;

        #region Khởi tạo đối tượng

        /// <summary>
        /// Khởi tạo đối tượng
        /// </summary>
        public GGoodsPack()
        {
        }

        #endregion

        #region 2D Objects
        /// <summary>
        /// Component Item
        /// </summary>
        public Item ComponentItem { get; private set; } = null;
        #endregion

        #region Kế thừa IObject
        /// <summary>
        /// BaseID
        /// </summary>
        public int BaseID { get; set; }

        /// <summary>
        /// Tên
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Trạng thái ban đầu
        /// </summary>
        private bool _InitStatus = false;
        /// <summary>
        /// Trạng thái ban đầu
        /// </summary>
        public bool InitStatus
        {
            get { return _InitStatus; }
        }

        /// <summary>
        /// Đối tượng GameObject 2D
        /// </summary>
        public GameObject Role2D { get; set; }

        /// <summary>
        /// Vị trí hiện tại (tọa độ thực)
        /// </summary>
        public Point Coordinate
        {
            get { return new Point(this.PosX, this.PosY); }
            set
            {
                this.PosX = value.X;
                this.PosY = value.Y;

                this.ApplyXYPos();
            }
        }

        private int _PosX = 0;

        /// <summary>
        /// Tọa độ thực X
        /// </summary>
        public int PosX
        {
            get { return this._PosX; }
            set
            {
                this._PosX = value;
                this.ApplyXYPos();
            }
        }

        private int _PosY = 0;

        /// <summary>
        /// Tọa độ thực Y
        /// </summary>
        public int PosY
        {
            get { return this._PosY; }
            set
            {
                this._PosY = value;
                this.ApplyXYPos();
            }
        }

        /// <summary>
        /// Cập nhật tọa độ XY
        /// </summary>
        private void ApplyXYPos()
        {
            if (null != this.Role2D)
            {
                this.Role2D.transform.localPosition = new Vector3(this._PosX, this._PosY);
            }
        }

        /// <summary>
        /// Trả về tọa độ của đối tượng dưới dạng UnityEngine.Vector2
        /// </summary>
        public Vector2 PositionInVector2
        {
            get
            {
                return new Vector2(this.PosX, this.PosY);
            }
        }

        #endregion

        #region Kế thừa ISprite

        /// <summary>
        /// Loại đối tượng
        /// </summary>
        public GSpriteTypes SpriteType
        {
            get;
            set;
        }

        /// <summary>
        /// Động tác
        /// </summary>
        public KE_NPC_DOING CurrentAction { get; set; } 

        /// <summary>
        /// Hướng quay (8 hướng)
        /// </summary>
        public Direction Direction { get; set; }

        /// <summary>
        /// Tốc chạy
        /// </summary>
        public int MoveSpeed { get; set; }

        #endregion


        #region Kế thừa IObject - Hiển thị

        /// <summary>
        /// Đã bắt đầu chưa
        /// </summary>
        private bool _Started = false;

        /// <summary>
        /// Bắt đầu
        /// </summary>
        public void Start()
        {
            /// Nếu đã bắt đầu
            if (this._Started)
            {
                return;
            }

            /// Đánh dấu đã bắt đầu
            this._Started = true;
            /// Đánh dấu đã khởi tạo
            this._InitStatus = true;

            /// Thêm thành phần vào
            this.ComponentItem = this.Role2D.GetComponent<Item>();
            /// Đánh dấu thời điểm bắt đầu
            this.StartTick = KTGlobal.GetCurrentTimeMilis();

            /// Thêm đối tượng vào danh sách quản lý
            KTObjectsManager.Instance.AddObject(this);
        }

        /// <summary>
        /// Hủy đối tượng
        /// </summary>
        public void Destroy()
        {
            /// Xóa đối tượng
            KTObjectsManager.Instance.RemoveObject(this);

            /// Nếu tồn tại đối tượng
            if (null != Role2D)
            {
                /// Thực hiện hủy
                this.ComponentItem.Destroy();
                this.Role2D = null;
            }
        }

        #endregion 

        #region Kế thừa IObject - Render

        /// <summary>
        /// Hàm này gọi liên tục mỗi Frame, tương tự hàm Update
        /// </summary>
        public void OnFrameRender()
        {
            /// Nếu chưa bắt đầu
            if (!this._Started)
            {
                return;
            }

            /// Nếu hết thời gian thì tự hủy vật phẩm
            if (this.StartTick > 0 && KTGlobal.GetCurrentTimeMilis() - this.StartTick >= GGoodsPack.GoodsPackKeepTimes - this.LifeTimeTicks)
            {
                /// Hủy đối tượng
                KTGlobal.RemoveObject(this, true);
                return;
            }
        }

        #endregion

        #region Thuộc tính
        /// <summary>
        /// Thời điểm được tạo ra
        /// </summary>
        private long StartTick = 0;

        /// <summary>
        /// Thời gian tồn tại
        /// </summary>
        public long LifeTimeTicks { get; set; }

        /// <summary>
        /// ID vật phẩm
        /// </summary>
        public int GoodsID { get; set; }

        /// <summary>
        /// Số sao
        /// </summary>
        public int Stars { get; set; }

        /// <summary>
        /// Số dòng
        /// </summary>
        public int LinesCount { get; set; }

        /// <summary>
        /// Cấp cường hóa
        /// </summary>
        public int EnhanceLevel { get; set; }
        #endregion
    }
}
