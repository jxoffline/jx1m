using GameServer.Interface;
using GameServer.KiemThe.Entities;
using GameServer.KiemThe.Logic;
using GameServer.KiemThe.Logic.Manager;
using GameServer.Logic;
using Server.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using static GameServer.Logic.KTMapManager;

namespace GameServer.KiemThe.Core
{
    /// <summary>
    /// Đối tượng thu thập
    /// </summary>
    public class GrowPoint : IObject
    {
        /// <summary>
        /// ID thu thập
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// Dữ liệu cấu hình
        /// </summary>
        public GrowPointXML Data { get; set; }

        /// <summary>
        /// Tên điểm thu thập
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Thời gian tái tạo (-1 nếu tái tạo ngay lập tức, -100 nếu không tái tạo)
        /// <para>Đơn vị Milis</para>
        /// </summary>
        public int RespawnTime { get; set; }

        /// <summary>
        /// Thời gian tồn tại (-1 nếu tồn tại vĩnh viễn)
        /// <para>Đơn vị Milis</para>
        /// </summary>
        public int LifeTime { get; set; } = -1;

        /// <summary>
        /// ID bản đồ
        /// </summary>
        public int MapCode { get; set; }

        /// <summary>
        /// ID Script điều khiển
        /// </summary>
        public int ScriptID { get; set; }

        /// <summary>
        /// Đối tượng còn tồn tại không
        /// </summary>
        public bool Alive { get; set; }

        /// <summary>
        /// Thời điểm tạo ra
        /// </summary>
        public long InitTicks { get; private set; }

        /// <summary>
        /// Thực thi tự động xóa khi hết thời gian
        /// </summary>
        public void ProcessAutoRemoveTimeout()
		{
            /// Nếu có thời gian tồn tại
            if (this.LifeTime > 0)
            {
                /// Chạy Timer
                KTGrowPointTimerManager.Instance.Add(this, this.LifeTime, () =>
                {
                    /// Xóa đối tượng
                    KTGrowPointManager.RemoveGrowPointAndRespawn(this);

                    /// Thực thi sự kiện Timeout
                    try
                    {
                        this.OnTimeout?.Invoke();
                    }
                    catch (Exception ex)
                    {
                        LogManager.WriteLog(LogTypes.Exception, ex.ToString());
                    }

                    /// Thông báo tới người chơi xung quanh xóa đối tượng
                    KTGrowPointManager.NotifyNearClientsToRemoveSelf(this);
                });
            }
        }

        /// <summary>
        /// Đối tượng thu thập
        /// </summary>
        public GrowPoint()
		{
            /// Đánh dấu thời điểm tạo ra
            this.InitTicks = KTGlobal.GetCurrentTimeMilis();
        }


        #region IObject
        /// <summary>
        /// Loại đối tượng
        /// </summary>
        public ObjectTypes ObjectType { get; set; }

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
        public int CurrentMapCode
        {
            get
            {
                lock (this)
                {
                    return this.MapCode;
                }
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

        /// <summary>
        /// Funtion thực thi thêm sau khi thu thập xong
        /// </summary>
        public Action<KPlayer> GrowPointCollectCompleted { get; set; }

        /// <summary>
        /// Sự kiện khi hết thời gian tồn tại
        /// </summary>
        public Action OnTimeout { get; set; }

        /// <summary>
        /// Kiểm tra điều kiện
        /// </summary>
        public Predicate<KPlayer> ConditionCheck { get; set; } = (player) => { return true; };
        #endregion
    }
}
