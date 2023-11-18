using GameServer.Interface;
using GameServer.KiemThe.Entities;
using GameServer.KiemThe.Logic;
using GameServer.KiemThe.Logic.Manager;
using GameServer.Logic;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace GameServer.KiemThe.Core
{
    /// <summary>
    /// Đối tượng khu vực động
    /// </summary>
    public class KDynamicArea : IObject
    {
        /// <summary>
        /// ID thu thập
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// Tên điểm thu thập
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// ID Res
        /// </summary>
        public int ResID { get; set; }

        /// <summary>
        /// ID bản đồ
        /// </summary>
        public int MapCode { get; set; }

        /// <summary>
        /// ID Script điều khiển
        /// </summary>
        public int ScriptID { get; set; }

        /// <summary>
        /// Bán kính quét
        /// </summary>
        public int Radius { get; set; }

        /// <summary>
        /// Thời gian tồn tại
        /// </summary>
        public long LifeTime { get; set; }

        /// <summary>
        /// Thời gian Tick
        /// </summary>
        public int Tick { get; set; }

        /// <summary>
        /// Tag
        /// </summary>
        public string Tag { get; set; } = "";

        /// <summary>
        /// Số lượng người chơi xung quanh
        /// </summary>
        public int VisibleClientsNum { get; set; }

        /// <summary>
        /// Thời điểm được tạo ra
        /// </summary>
        public long StartTicks { get; set; }

        /// <summary>
        /// Danh sách đối tượng đang đứng trong khu vực động
        /// </summary>
        private readonly ConcurrentDictionary<GameObject, byte> holdObjects = new ConcurrentDictionary<GameObject, byte>();

        /// <summary>
        /// Kiểm tra đối tượng có nằm trong khu vực động không
        /// </summary>
        /// <param name="go"></param>
        /// <returns></returns>
        private bool IsInsideArea(GameObject go)
        {
            /// Nếu khác bản đồ hoặc phụ bản
            if (this.CurrentMapCode != go.CurrentMapCode || this.CurrentCopyMapID != go.CurrentCopyMapID)
            {
                return false;
            }

            UnityEngine.Vector2 pos = new UnityEngine.Vector2((int) go.CurrentPos.X, (int) go.CurrentPos.Y);
            UnityEngine.Vector2 selfPos = new UnityEngine.Vector2((int) this.CurrentPos.X, (int) this.CurrentPos.Y);

            return UnityEngine.Vector2.Distance(pos, selfPos) <= this.Radius;
        }

        /// <summary>
        /// Thực thi kiểm tra liên tục
        /// </summary>
        public void ProcessTick()
        {
            List<GameObject> keys = this.holdObjects.Keys.ToList();
            /// Duyệt danh sách đối tượng ghi nhận lần trước
            foreach (GameObject go in keys)
            {
                /// Nếu nằm ngoài phạm vi
                if (!this.IsInsideArea(go))
                {
                    /// Xóa đối tượng khỏi danh sách trong khu vực
                    this.holdObjects.TryRemove(go, out _);
                    /// Thực thi sự kiện OnLeave
                    KTDynamicAreaManager.ProcessOnLeave(go, this);
                }
            }

            UnityEngine.Vector2 selfPos = new UnityEngine.Vector2((int) this.CurrentPos.X, (int) this.CurrentPos.Y);
            List<GameObject> nearByObjects = KTGlobal.GetNearByObjectsAtPos<GameObject>(this.CurrentMapCode, this.CurrentCopyMapID, selfPos, this.Radius);
            /// Duyệt danh sách đối tượng xung quanh
            foreach (GameObject go in nearByObjects)
            {
                /// Nếu đối tượng đã đứng trong khu vực
                if (this.holdObjects.ContainsKey(go))
                {
                    /// Thực thi sự kiện OnStayTick
                    KTDynamicAreaManager.ProcessOnStayTick(go, this);
                }
                /// Nếu đối tượng chưa ở trng khu vực
                else
                {
                    /// Thêm đối tượng vào danh sách trong khu vực
                    this.holdObjects[go] = (byte) 0;
                    /// Thực thi sự kiện OnEnter
                    KTDynamicAreaManager.ProcessOnEnter(go, this);
                }
            }
        }

        /// <summary>
        /// Làm rỗng dữ liệu
        /// </summary>
        public void Clear()
        {
            this.holdObjects.Clear();
        }

        #region IObject
        /// <summary>
        /// Loại đối tượng
        /// </summary>
        public ObjectTypes ObjectType { get; set; }

        /// <summary>
        /// Tọa độ lưới
        /// </summary>
        public Point CurrentGrid { get; set; }

        /// <summary>
        /// Tọa độ thực
        /// </summary>
        public Point CurrentPos { get; set; }

        /// <summary>
        /// ID bản đồ hiện tại
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
        public int CurrentCopyMapID { get; set; } = -1;

        /// <summary>
        /// Hướng quay hiện tại
        /// </summary>
        public KiemThe.Entities.Direction CurrentDir { get; set; } = KiemThe.Entities.Direction.DOWN;
        #endregion

        /// <summary>
        /// Sự kiện người chơi tiến vào khu vực động
        /// </summary>
        public Action<GameObject> OnEnter { get; set; }
        /// <summary>
        /// Sự kiện người chơi đứng trong khu vực động
        /// </summary>
        public Action<GameObject> OnStayTick { get; set; }

        /// <summary>
        /// Sự kiện người chơi rời khu vực động
        /// </summary>
        public Action<GameObject> OnLeave { get; set; }

        /// <summary>
        /// Sự kiện hết thời gian
        /// </summary>
        public Action OnTimeout { get; set; }
    }
}
