using FS.Drawing;
using UnityEngine;
using FS.GameEngine.Logic;
using FS.GameEngine.Interface;
using FS.VLTK.Factory;
using static FS.VLTK.Entities.Enum;

namespace FS.GameEngine.Teleport
{
    /// <summary>
    /// Đối tượng điểm truyền tống
    /// </summary>
    public class GTeleport : IObject
	{
        #region Khởi tạo
        /// <summary>
        /// Khởi tạo
        /// </summary>
        /// <param name="resID"></param>
        public GTeleport(string resID)
        {
            this.ResID = resID;
        }

        #endregion

        #region Kế thừa interface IObject
        /// <summary>
        /// BaseID đối tượng
        /// </summary>
        public int BaseID { get; set; }

        /// <summary>
        /// Tên đối tượng
        /// </summary>
        public string Name { get; set; }

        private bool _InitStatus = false;
        /// <summary>
        /// Trạng thái bắt đầu
        /// </summary>
        public bool InitStatus
        {
            get { return _InitStatus; }
        }

        /// <summary>
        /// Script điều khiển đối tượng 2D
        /// </summary>
        public GameObject Role2D { get; set; } = null;

        /// <summary>
        /// Tọa độ hiện tại
        /// </summary>
        public Point Coordinate
        {
            get { return new Point(PosX, PosY); }
            set
            {
                PosX = value.X;
                PosY = value.Y;

                this.ApplyXYPos();
            }
        }

        private int _PosX = 0;

        /// <summary>
        /// Tọa độ thực X
        /// </summary>
        public int PosX
        {
            get { return _PosX; }
            set { _PosX = value; }
        }

        private int _PosY = 0;

        /// <summary>
        /// Tọa độ thực Y
        /// </summary>
        public int PosY
        {
            get { return _PosY; }
            set
            {
                _PosY = value;

                ApplyXYPos();
            }
        }

        /// <summary>
        /// Cập nhật sự kiện tọa độ đối tượng thay đổi
        /// </summary>
        private void ApplyXYPos()
        {
            if (this.Role2D != null)
            {
                this.Role2D.transform.localPosition = new Vector2(this._PosX, this._PosY);
            }
        }

        #endregion

        #region Kế thừa interface ISprite

        /// <summary>
        /// Loại Sprite
        /// </summary>
        public GSpriteTypes SpriteType { get; set; }

        /// <summary>
        /// Động tác
        /// </summary>
        public KE_NPC_DOING CurrentAction { get; set; }

        /// <summary>
        /// Hướng (8 hướng)
        /// </summary>
        public Direction Direction { get; set; }

        /// <summary>
        /// Tốc chạy
        /// </summary>
        public int MoveSpeed { get; set; }

        #endregion

        #region Thuộc tính
        /// <summary>
        /// ID Res
        /// </summary>
        public string ResID { get; set; }

        /// <summary>
        /// Chú thích
        /// </summary>
        public string Tip { get; set; }

        /// <summary>
        /// Mã đánh dấu
        /// </summary>
        public byte Key { get; set; }

        /// <summary>
        /// ID map đến
        /// </summary>
        public int To { get; set; }

        /// <summary>
        /// Vị trí X đến
        /// </summary>
        public int ToX { get; set; }

        /// <summary>
        /// Vị trí Y đến
        /// </summary>
        public int ToY { get; set; }

        /// <summary>
        /// Loại bản đồ
        /// </summary>
        public MapType ToType { get; set; }

        /// <summary>
        /// Cấp độ yêu cầu
        /// </summary>
        public int ToLevel { get; set; }

        /// <summary>
        /// Bán kính vùng dịch chuyển
        /// </summary>
        public int Radius { get; set; }
        #endregion

        #region Kế thừa interface IObject (Hiển thị)
        /// <summary>
        /// Đã bắt đầu chưa
        /// </summary>
        private bool _Started = false;

        /// <summary>
        /// Bắt đầu
        /// </summary>
        public void Start()
        {
            if (this._Started)
            {
                return;
            }

            this._Started = true;
            this._InitStatus = true;

            KTObjectsManager.Instance.AddObject(this);

            //VLTK.Control.Component.Teleport teleport = Object2DFactory.MakeTeleport();
            VLTK.Control.Component.Teleport teleport = KTObjectPoolManager.Instance.Instantiate<VLTK.Control.Component.Teleport>("Teleport");
            teleport.name = this.Name;

            this.Role2D = teleport.gameObject;
            this.Role2D.name = this.Name;
            this.ApplyXYPos();

            teleport.MinimapIconSize = new Vector2(50, 50);
            teleport.Data.ID = this.Key;
            teleport.Data.ResID = this.ResID;
            teleport.Data.Name = this.Tip;
            teleport.Data.Radius = this.Radius;
            teleport.Data.ToMapID = this.To;
            teleport.Data.ToMapType = this.ToType;
            teleport.Data.ToMapLevel = this.ToLevel;
            teleport.Data.ToMapPosition = new Vector2((int) this.ToX, (int) this.ToY);
            teleport.ResumeCurrentAction();
        }

        /// <summary>
        /// Hủy đối tượng
        /// </summary>
        public void Destroy()
        {
            KTObjectsManager.Instance.RemoveObject(this);

            if (null != this.Role2D)
            {
                this.Role2D.GetComponent<VLTK.Control.Component.Teleport>().Destroy();
                this.Role2D = null;
            }
        }

        #endregion

        #region Kế thừa lớp IObject - Gọi liên tục mỗi Frame
        /// <summary>
        /// Gọi liên tục mỗi Frame
        /// </summary>
        /// <param name="time"></param>
        public void OnFrameRender()
        {

        }

        #endregion 
    }
}
