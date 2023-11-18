using GameServer.Core.Executor;
using GameServer.KiemThe.Core;
using GameServer.KiemThe.Logic;
using GameServer.KiemThe.Logic.Manager;
using GameServer.Logic;
using GameServer.Server;
using Server.Data;
using Server.Tools;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using static GameServer.Logic.KTMapManager;

namespace GameServer.KiemThe.GameEvents.Interface
{
    /// <summary>
    /// Lớp mẫu sự kiện trong Game
    /// </summary>
    public abstract class GameMapEvent : IDisposable
    {
        /// <summary>
        /// Đối tượng Delay Task dùng cục bộ
        /// </summary>
        private class DelayTask
        {
            /// <summary>
            /// ID tự động
            /// </summary>
            private static int _AutoID = -1;

            /// <summary>
            /// ID
            /// </summary>
            public int ID { get; private set; }

            /// <summary>
            /// Thời gian Delay
            /// </summary>
            public int Delay { get; set; }

            /// <summary>
            /// Công việc cần thực thi
            /// </summary>
            public Action Work { get; set; }

            /// <summary>
            /// Thời điểm tạo ra
            /// </summary>
            public long InitTicks { get; set; }

            /// <summary>
            /// Đã đến thời gian chưa
            /// </summary>
            public bool InTime
            {
                get
                {
                    return KTGlobal.GetCurrentTimeMilis() - this.InitTicks >= this.Delay;
                }
            }

            /// <summary>
            /// Đối tượng Delay Task dùng cục bộ
            /// </summary>
            public DelayTask()
            {
                /// Tăng ID tự động
                DelayTask._AutoID = (DelayTask._AutoID + 1) % 10000007;
                /// Thiết lập ID
                this.ID = DelayTask._AutoID;
            }
        }

        /// <summary>
        /// Hoạt động tương ứng
        /// </summary>
        public KTActivity Activity { get; private set; }

        /// <summary>
        /// Bản đồ tương ứng
        /// </summary>
        public GameMap Map { get; private set; }

        /// <summary>
        /// Thời gian tồn tại (milis)
        /// </summary>
        public long DurationTicks { get; private set; }

        /// <summary>
        /// Thời gian mỗi lần gọi hàm OnTick (milis)
        /// </summary>
        public int Ticks { get; set; } = 500;

        /// <summary>
        /// Thời gian tồn tại hoạt động
        /// </summary>
        protected long LifeTimeTicks { get; private set; } = 0;

        /// <summary>
        /// Biến đánh dấu có tự xóa NPC và quái trong các bản đồ tương ứng khi hoạt động bị hủy không
        /// </summary>
        public bool RemoveAllObjectsOnDispose { get; set; } = true;

		/// <summary>
		/// Thời điểm Tick lần trước
		/// </summary>
		private long LastTicks = 0;

        /// <summary>
        /// Đối tượng đã bị hủy chưa
        /// </summary>
        protected bool isDisposed = false;

        /// <summary>
        /// Danh sách DelayTask chạy song song
        /// </summary>
        private readonly ConcurrentDictionary<int, DelayTask> delayTasks = new ConcurrentDictionary<int, DelayTask>();

        /// <summary>
        /// Sự kiện bắt đầu
        /// </summary>
        protected abstract void OnStart();

        /// <summary>
        /// Sự kiện Tick
        /// </summary>
        protected abstract void OnTick();

        /// <summary>
        /// Sự kiện kết thúc
        /// </summary>
        protected abstract void OnClose();

        /// <summary>
        /// Sự kiện hết thời gian
        /// </summary>
        protected virtual void OnTimeout()
        {

        }

        /// <summary>
        /// Sự kiện khi người chơi vào bản đồ
        /// </summary>
        /// <param name="player"></param>
        public virtual void OnPlayerEnter(KPlayer player)
		{

		}

        /// <summary>
        /// Sự kiện khi người chơi rời bản đồ
        /// </summary>
        /// <param name="player"></param>
        /// <param name="toMap"></param>
        public virtual void OnPlayerLeave(KPlayer player, GameMap toMap)
		{

		}

        /// <summary>
        /// Sự kiện khi người chơi giết quái
        /// </summary>
        /// <param name="player"></param>
        /// <param name="obj"></param>
        public virtual void OnKillObject(KPlayer player, GameObject obj)
		{

		}

        /// <summary>
        /// Sự kiện khi người chơi ấn nút về thành
        /// </summary>
        /// <param name="player"></param>
        /// <returns>Nếu False thì sẽ lấy điểm về thành mặc định</returns>
        public virtual bool OnPlayerClickReliveButton(KPlayer player)
		{
            return false;
		}

        #region Actions
        /// <summary>
        /// Tạo mới đối tượng
        /// </summary>
        /// <param name="map"></param>
        public GameMapEvent(GameMap map, KTActivity activity)
        {
            /// Gắn bản đồ tương ứng
            this.Map = map;
            /// Gắn sự kiện tương ứng
            this.Activity = activity;
            /// Đánh dấu đối tượng chưa bị hủy
            this.isDisposed = false;
            /// Đánh dấu thời gian tồn tại
            this.DurationTicks = activity.Data.DurationTicks;

            /// Thêm vào lớp điều khiển
            GameMapEventsManager.Add(this);
        }

        /// <summary>
        /// Bắt đầu
        /// </summary>
        public void Begin()
		{
            /// Thiết lập thời gian tồn tại
            this.LifeTimeTicks = 0;
            /// Cập nhật thời gian Tick lần trước
            this.LastTicks = KTGlobal.GetCurrentTimeMilis();
            try
            {
                /// Thực thi sự kiện OnStart
                this.OnStart();
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Exception, ex.ToString());
            }
            /// Bắt đầu Timer
            this.StartEventTimer();
        }

        /// <summary>
        /// Background Worker
        /// </summary>
        protected BackgroundWorker worker = new BackgroundWorker();

        /// <summary>
        /// Task tương ứng
        /// </summary>
        protected NormalScheduleTask task;

        /// <summary>
        /// Bắt đầu luồng thực thi sự kiện
        /// </summary>
        private void StartEventTimer()
        {
            /// Khởi tạo Background Worker
            this.worker.DoWork += this.ExecuteTick;
            this.task = new NormalScheduleTask(string.Format("GameMapEventTimer.{0}", this.Activity.Data.Name), (o, e) => {
                try
                {
                    if (!this.worker.IsBusy)
                    {
                        this.worker.RunWorkerAsync();
                    }
                }
                catch (Exception ex)
                {
                    LogManager.WriteLog(LogTypes.Exception, ex.ToString());
                }
            });
            ScheduleExecutor2.Instance.scheduleExecute(this.task, 0, this.Ticks);
        }

        /// <summary>
        /// Thực thi sự kiện liên tiếp
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        private void ExecuteTick(object o, DoWorkEventArgs e)
        {
            /// Tăng thời gian tồn tại lên
            this.LifeTimeTicks += KTGlobal.GetCurrentTimeMilis() - this.LastTicks;
            /// Cập nhật thời gian Tick lần trước
            this.LastTicks = KTGlobal.GetCurrentTimeMilis();

            /// Thực thi sự kiện Tick
            try
			{
                this.OnTick();
			}
            catch (Exception ex)
			{
                LogManager.WriteLog(LogTypes.Exception, ex.ToString());
            }



            /// Danh sách DelayTask cần xóa
            List<int> toRemoveTasks = null;

            /// Khóa
            List<int> keys = this.delayTasks.Keys.ToList();
            /// Duyệt danh sách DelayTask
            foreach (int key in keys)
            {
                /// Thông tin
                if (!this.delayTasks.TryGetValue(key, out DelayTask delayTask))
                {
                    continue;
                }

                /// Nếu đã đến thời gian
                if (delayTask.InTime)
                {
                    /// Nếu danh sách cần xóa không tồn tại
                    if (toRemoveTasks == null)
                    {
                        /// Tạo mới
                        toRemoveTasks = new List<int>();
                    }
                    /// Thêm vào danh sách cần xóa
                    toRemoveTasks.Add(key);

                    /// Thực thi sự kiện
                    try
                    {
                        delayTask.Work?.Invoke();
                    }
                    catch (Exception ex)
                    {
                        LogManager.WriteLog(LogTypes.Exception, ex.ToString());
                    }
                }
            }
            /// Nếu tồn tại danh sách cần xóa
            if (toRemoveTasks != null)
            {
                /// Duyệt danh sách cần xóa
                foreach (int delayTaskID in toRemoveTasks)
                {
                    /// Xóa khỏi danh sách gốc
                    this.delayTasks.TryRemove(delayTaskID, out _);
                }
                /// Xóa danh sách để giải phóng tài nguyên
                toRemoveTasks?.Clear();
            }

            /// Nếu đã quá thời gian hoạt động
            if (this.LifeTimeTicks >= this.DurationTicks)
            {
                /// Thực thi sự kiện khi hết thời gian
                this.OnTimeout();
                /// Hủy đối tượng
                this.Dispose();
            }
        }

        /// <summary>
        /// Hủy đối tượng
        /// <para>Cần thận trọng khi gọi hàm này ở ngoài, vì sẽ làm ảnh hưởng đến hệ thống do kiến trúc đa luồng, tốt nhất nên để Script tự kết thúc khi hết thời gian thực thi.</para>
        /// </summary>
        public void Dispose()
		{
            /// Nếu đối tượng đã bị hủy thì thôi
            if (this.isDisposed)
			{
                return;
			}
            /// Đánh dấu đối tượng đã bị hủy
            this.isDisposed = true;

            /// Xóa toàn bộ Timer chạy cùng
            this.delayTasks.Clear();

            /// Đánh dấu hoạt động chưa bắt đấu
            this.Activity.IsStarted = false;

            /// Thực thi sự kiện OnClose
            try
            {
                this.OnClose();
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Exception, ex.ToString());
            }

            /// Nếu có đánh dấu tự hủy NPC và quái
            if (this.RemoveAllObjectsOnDispose)
			{
                /// Xóa toàn bộ các đối tượng trong bản đồ
                this.RemoveAllNPCs();
                this.RemoveAllMonsters();
                this.RemoveAllGrowPoints();
                this.RemoveAllDynamicAreas();
            }

            /// Ngừng luồng thực thi
            ScheduleExecutor2.Instance.scheduleCancle(this.task);

            /// Xóa khỏi lớp điều khiển
            GameMapEventsManager.Remove(this);
        }
        #endregion

        #region Objects
        /// <summary>
        /// Trả về tổng số người chơi trong bản đồ
        /// </summary>
        /// <returns></returns>
        protected int GetTotalPlayers()
        {
            return KTPlayerManager.GetPlayersCount(this.Map.MapCode);
        }

        /// <summary>
        /// Trả về danh sách người chơi trong bản đồ hoạt động
        /// </summary>
        /// <returns></returns>
        protected List<KPlayer> GetPlayers()
        {
            /// Trả về kết quả
            return KTPlayerManager.FindAll(this.Map.MapCode);
        }

        /// <summary>
        /// Trả về tổng số quái trong bản đồ
        /// </summary>
        /// <returns></returns>
        protected int GetTotalMonsters()
        {
            /// Trả về kết quả
            return KTMonsterManager.GetTotalMonstersAtMap(this.Map.MapCode);
        }

        /// <summary>
        /// Trả về danh sách quái trong bản đồ
        /// </summary>
        /// <returns></returns>
        protected List<Monster> GetMonsters()
        {
            /// Trả về kết quả
            return KTMonsterManager.GetMonstersAtMap(this.Map.MapCode);
        }

        /// <summary>
        /// Xóa quái khỏi bản đồ
        /// </summary>
        /// <param name="monster"></param>
        protected void RemoveMonster(Monster monster)
        {
            KTMonsterManager.Remove(monster);
        }

        /// <summary>
        /// Xóa toàn bộ quái trong bản đồ
        /// </summary>
        protected void RemoveAllMonsters()
        {
            foreach (Monster monster in this.GetMonsters())
            {
                this.RemoveMonster(monster);
            }
        }

        /// <summary>
        /// Trả về tổng số NPC trong bản đồ
        /// </summary>
        /// <returns></returns>
        protected int GetTotalNPCs()
        {
            return KTNPCManager.GetMapNPCs(this.Map.MapCode).Count;
        }

        /// <summary>
        /// Trả về danh sách NPC trong bản đồ
        /// </summary>
        /// <returns></returns>
        protected List<NPC> GetNPCs()
        {
            /// Danh sách NPC trong bản đồ
            return KTNPCManager.GetMapNPCs(this.Map.MapCode);
        }

        /// <summary>
        /// Xóa NPC khỏi bản đồ
        /// </summary>
        /// <param name="npc"></param>
        protected void RemoveNPC(NPC npc)
        {
            KTNPCManager.Remove(npc);
        }

        /// <summary>
        /// Xóa toàn bộ NPC trong bản đồ
        /// </summary>
        protected void RemoveAllNPCs()
        {
            foreach (NPC npc in this.GetNPCs())
            {
                this.RemoveNPC(npc);
            }
        }

        /// <summary>
        /// Trả về tổng số điểm thu thập trong bản đồ
        /// </summary>
        /// <returns></returns>
        protected int GetTotalGrowPoints()
        {
            return KTGrowPointManager.GetMapGrowPoints(this.Map.MapCode).Count;
        }

        /// <summary>
        /// Trả về danh sách điểm thu thập trong bản đồ
        /// </summary>
        /// <returns></returns>
        protected List<GrowPoint> GetGrowPoints()
        {
            /// Danh sách điểm thu thập trong bản đồ
            return KTGrowPointManager.GetMapGrowPoints(this.Map.MapCode);
        }

        /// <summary>
        /// Xóa điểm thu thập khỏi bản đồ
        /// </summary>
        /// <param name="growPoint"></param>
        protected void RemoveGrowPoint(GrowPoint growPoint)
        {
            KTGrowPointManager.Remove(growPoint);
        }

        /// <summary>
        /// Xóa toàn bộ điểm thu thập trong bản đồ
        /// </summary>
        protected void RemoveAllGrowPoints()
        {
            foreach (GrowPoint growPoint in this.GetGrowPoints())
            {
                this.RemoveGrowPoint(growPoint);
            }
        }

        /// <summary>
        /// Trả về tổng số khu vực động trong bản đồ
        /// </summary>
        /// <returns></returns>
        protected int GetTotalDynamicAreas()
        {
            return KTDynamicAreaManager.GetMapDynamicAreas(this.Map.MapCode).Count;
        }

        /// <summary>
        /// Trả về danh sách khu vực động trong bản đồ
        /// </summary>
        /// <returns></returns>
        protected List<KDynamicArea> GetDynamicAreas()
        {
            /// Danh sách khu vực động trong bản đồ
            return KTDynamicAreaManager.GetMapDynamicAreas(this.Map.MapCode);
        }

        /// <summary>
        /// Xóa khu vực động khỏi bản đồ
        /// </summary>
        /// <param name="dynArea"></param>
        protected void RemoveDynamicAreas(KDynamicArea dynArea)
        {
            KTDynamicAreaManager.Remove(dynArea);
        }

        /// <summary>
        /// Xóa toàn bộ cổng dịch chuyển trong bản đồ
        /// </summary>
        protected void RemoveAllDynamicAreas()
        {
            foreach (KDynamicArea teleport in this.GetDynamicAreas())
            {
                this.RemoveDynamicAreas(teleport);
            }
        }
        #endregion

        #region Support methods
        /// <summary>
        /// Thiết lập thời gian đếm lùi thực thi công việc
        /// </summary>
        /// <param name="ticks"></param>
        /// <param name="work"></param>
        protected void SetTimeout(int delay, Action work)
        {
            /// Tạo mới
            DelayTask delayTask = new DelayTask()
            {
                Delay = delay,
                InitTicks = KTGlobal.GetCurrentTimeMilis(),
                Work = work,
            };
            /// Thêm vào danh sách
            this.delayTasks[delayTask.ID] = delayTask;
        }

        /// <summary>
        /// Gửi thông báo dạng Tooltip tới tất cả người chơi
        /// </summary>
        /// <param name="message"></param>
        protected void NotifyAllPlayers(string message)
        {
            foreach (KPlayer player in this.GetPlayers())
            {
                KTPlayerManager.ShowNotification(player, message);
            }
        }

        /// <summary>
        /// Gửi yêu cầu mở khung thông tin sự kiện ở góc trái
        /// </summary>
        /// <param name="player"></param>
        /// <param name="eventID"></param>
        protected void OpenEventBroadboard(KPlayer player, int eventID)
        {
            G2C_EventState state = new G2C_EventState();
            state.EventID = eventID;
            state.State = 1;
            player.SendPacket<G2C_EventState>((int) TCPGameServerCmds.CMD_KT_EVENT_STATE, state);
        }

        /// <summary>
        /// Gửi yêu cầu đóng khung thông tin sự kiện ở góc trái
        /// </summary>
        /// <param name="player"></param>
        /// <param name="eventID"></param>
        protected void CloseEventBroadboard(KPlayer player, int eventID)
        {
            G2C_EventState state = new G2C_EventState();
            state.EventID = eventID;
            state.State = 0;
            player.SendPacket<G2C_EventState>((int) TCPGameServerCmds.CMD_KT_EVENT_STATE, state);
        }

        /// <summary>
        /// Cập nhật thông tin sự kiện vòa khung ở góc trái
        /// </summary>
        /// <param name="player"></param>
        /// <param name="name"></param>
        /// <param name="eventTimeMilis"></param>
        /// <param name="contents"></param>
        protected void UpdateEventDetails(KPlayer player, string name, long eventTimeMilis, params string[] contents)
        {
            G2C_EventNotification eventNotification = new G2C_EventNotification();
            eventNotification.EventName = name;
            int eventTimeSec = (int) (eventTimeMilis / 1000);
            if (eventTimeSec > 0)
            {
                eventNotification.ShortDetail = string.Format("TIME|{0}", eventTimeSec);
            }
            else
            {
                eventNotification.ShortDetail = "Đã kết thúc!";
            }
            eventNotification.TotalInfo = new List<string>();
            eventNotification.TotalInfo.AddRange(contents);
            player.SendPacket<G2C_EventNotification>((int) TCPGameServerCmds.CMD_KT_EVENT_NOTIFICATION, eventNotification);
        }

        /// <summary>
        /// Cập nhật thông tin sự kiện vòa khung ở góc trái
        /// </summary>
        /// <param name="player"></param>
        /// <param name="name"></param>
        /// <param name="eventShortDesc"></param>
        /// <param name="contents"></param>
        protected void UpdateEventDetails(KPlayer player, string name, string eventShortDesc, params string[] contents)
        {
            G2C_EventNotification eventNotification = new G2C_EventNotification();
            eventNotification.EventName = name;
            eventNotification.ShortDetail = eventShortDesc;
            eventNotification.TotalInfo = new List<string>();
            eventNotification.TotalInfo.AddRange(contents);
            player.SendPacket<G2C_EventNotification>((int) TCPGameServerCmds.CMD_KT_EVENT_NOTIFICATION, eventNotification);
        }


        /// <summary>
        /// Cập nhật thông tin sự kiện tới tất cả người chơi
        /// </summary>
        /// <param name="name"></param>
        /// <param name="eventTimeMilis"></param>
        /// <param name="contents"></param>
        protected void UpdateEventDetailsToPlayers(string name, long eventTimeMilis, params string[] contents)
        {
            foreach (KPlayer player in this.GetPlayers())
            {
                this.UpdateEventDetails(player, name, eventTimeMilis, contents);
            }
        }

        /// <summary>
        /// Cập nhật thông tin sự kiện tới tất cả người chơi
        /// </summary>
        /// <param name="name"></param>
        /// <param name="eventShortDesc"></param>
        /// <param name="contents"></param>
        protected void UpdateEventDetailsToPlayers(string name, string eventShortDesc, params string[] contents)
        {
            foreach (KPlayer player in this.GetPlayers())
            {
                this.UpdateEventDetails(player, name, eventShortDesc, contents);
            }
        }
		#endregion

		#region Debug
        /// <summary>
        /// Chuyển đối tượng về dạng String
        /// </summary>
        /// <returns></returns>
		public override string ToString()
		{
            if (this.Activity == null || this.Map == null)
			{
                return "Undefined";
			}
            return string.Format("Activity ID: {0}, Name: {1}, Map = {2}, Duration: {3}", this.Activity.Data.ID, this.Activity.Data.Name, this.Map.MapName, this.Activity.Data.DurationTicks);
		}
		#endregion
	}
}
