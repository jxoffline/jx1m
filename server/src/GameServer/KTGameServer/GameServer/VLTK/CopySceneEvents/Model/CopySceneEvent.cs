using GameServer.Core.Executor;
using GameServer.KiemThe.Core;
using GameServer.KiemThe.GameEvents.TeamBattle;
using GameServer.KiemThe.Logic;
using GameServer.KiemThe.Logic.Manager;
using GameServer.Logic;
using GameServer.Server;
using Server.Data;
using Server.Protocol;
using Server.Tools;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using static GameServer.Logic.KTMapManager;

namespace GameServer.KiemThe.CopySceneEvents.Model
{
	/// <summary>
	/// Lớp mẫu phụ bản
	/// </summary>
	public abstract class CopySceneEvent
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
        /// Phụ bản tương ứng
        /// </summary>
        public KTCopyScene CopyScene { get; private set; }

        /// <summary>
        /// Thời gian tồn tại (milis)
        /// </summary>
        public long DurationTicks { get; private set; }

        /// <summary>
        /// Thời gian tự hủy nếu không có người chơi trong phụ bản
        /// </summary>
        private const int AutoRemoveIfNoPlayersFor = 10000;

        /// <summary>
        /// Thời gian mỗi lần gọi hàm OnTick (milis)
        /// </summary>
        public int Ticks { get; set; } = 500;

        /// <summary>
        /// Thời gian tồn tại phụ bản
        /// </summary>
        protected long LifeTimeTicks { get; private set; } = 0;

        /// <summary>
        /// Thời điểm Tick lần trước
        /// </summary>
        private long LastTicks = 0;

        /// <summary>
        /// Đối tượng đã bị hủy chưa
        /// </summary>
        private bool isDisposed = false;

        /// <summary>
        /// Thời điểm Tick không có người chơi lần trước
        /// </summary>
        private long LastTicksHasPlayer = 0;

        /// <summary>
        /// Thời điểm lần trước kiểm tra Bug phụ bản
        /// </summary>
        private long LastCheckBugCopySceneID = 0;

        /// <summary>
        /// Danh sách DelayTask chạy song song
        /// </summary>
        private readonly ConcurrentDictionary<int, DelayTask> delayTasks = new ConcurrentDictionary<int, DelayTask>();

        /// <summary>
        /// Danh sách người chơi trong nhóm
        /// </summary>
        protected List<KPlayer> teamPlayers = new List<KPlayer>();

        /// <summary>
        /// Đã bị hủy chưa
        /// </summary>
        protected bool Disposed
        {
            get
            {
                return this.isDisposed;
            }
        }

        
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
        /// Sự kiện khi người chơi vào bản đồ phụ bản
        /// </summary>
        /// <param name="player"></param>
        public virtual void OnPlayerEnter(KPlayer player)
		{
            
		}

        /// <summary>
        /// Sự kiện khi người chơi rời bản đồ phụ bản
        /// </summary>
        /// <param name="player"></param>
        /// <param name="toMap"></param>
        public virtual void OnPlayerLeave(KPlayer player, GameMap toMap)
		{
            /// Hủy phụ bản
            player.CurrentCopyMapID = -1;
            /// Lưu thông tin vị trí trước đó của người chơi
            KT_TCPHandler.UpdateCopySceneInfo(player, -1, -1);
        }

        /// <summary>
        /// Sự kiện khi người chơi gây sát thương cho đối tượng khác trong phụ bản
        /// <para>Cẩn thận khi sử dụng hàm này vì ảnh hưởng bởi cơ chế đa luồng</para>
        /// </summary>
        /// <param name="player"></param>
        /// <param name="obj"></param>
        /// <param name="damage"></param>
        public virtual void OnHitTarget(KPlayer player, GameObject obj, int damage)
        {

        }

        /// <summary>
        /// Sự kiện khi người chơi giết quái trong phụ bản
        /// <para>Cẩn thận khi sử dụng hàm này vì ảnh hưởng bởi cơ chế đa luồng</para>
        /// </summary>
        /// <param name="player"></param>
        /// <param name="obj"></param>
        public virtual void OnKillObject(KPlayer player, GameObject obj)
		{

		}

        /// <summary>
        /// Sự kiện khi người chơi bị giết trong phụ bản
        /// </summary>
        /// <param name="killer"></param>
        /// <param name="player"></param>
        public virtual void OnPlayerDie(GameObject killer, KPlayer player)
		{

		}

        /// <summary>
        /// Sự kiện khi người chơi hồi sinh trong phụ bản
        /// </summary>
        /// <param name="player"></param>
        public virtual void OnPlayerRelive(KPlayer player)
		{

		}

        /// <summary>
        /// Sự kiện khi người chơi mất kết nối
        /// </summary>
        /// <param name="player"></param>
        public virtual void OnPlayerDisconnected(KPlayer player)
		{

		}

        /// <summary>
        /// Sự kiện khi người chơi kết nối lại
        /// </summary>
        /// <param name="player"></param>
        public virtual void OnPlayerReconnected(KPlayer player)
		{

		}

        #region Actions
        /// <summary>
        /// Tạo mới đối tượng
        /// </summary>
        /// <param name="copyScene"></param>
        public CopySceneEvent(KTCopyScene copyScene)
        {
            /// Đánh dấu đối tượng chưa bị hủy
            this.isDisposed = false;
            /// Đánh dấu phụ bản
            this.CopyScene = copyScene;
            /// Đánh dấu thời gian tồn tại
            this.DurationTicks = copyScene.DurationTicks;

            /// Thêm vào lớp điều khiển
            CopySceneEventManager.Add(this);
        }

        /// <summary>
        /// Bắt đầu
        /// </summary>
        /// <param name="players"></param>
        public void Begin(List<KPlayer> players)
        {

            /// Thiết lập danh sách người chơi trong nhóm
            this.teamPlayers = players;
            /// Thiết lập thời gian tồn tại
            this.LifeTimeTicks = 0;
            /// Cập nhật thời gian Tick lần trước
            this.LastTicks = KTGlobal.GetCurrentTimeMilis();
            /// Đánh dấu thời điểm Tick có người chơi lần trước
            this.LastTicksHasPlayer = KTGlobal.GetCurrentTimeMilis();
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
            /// Đưa người chơi vào phụ bản
            this.MovePlayersToCopyScene(players);

            //Console.WriteLine("Begin copyscene => ID: {0}, Name: {1}", this.CopyScene.ID, this.CopyScene.Name);
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
            this.task = new NormalScheduleTask(string.Format("CopySceneEventTimer.{0}", this.CopyScene.Name), (o, e) => {
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
            
            /// Nếu không có người chơi
            if (this.GetTotalPlayers() > 0)
			{
                /// Thực thi sự kiện Tick
                try
                {
                    this.SetCopySceneIDToAllPlayers();
                    this.OnTick();
                }
                catch (Exception ex)
                {
                    LogManager.WriteLog(LogTypes.Exception, ex.ToString());
                }

                /// Cập nhật thời điểm có người chơi
                this.LastTicksHasPlayer = KTGlobal.GetCurrentTimeMilis();
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

            /// Nếu đã quá thời gian hoạt động hoặc không có người chơi trong khoảng thời gian nhất định
            if (this.LifeTimeTicks >= this.DurationTicks || KTGlobal.GetCurrentTimeMilis() - this.LastTicksHasPlayer >= CopySceneEvent.AutoRemoveIfNoPlayersFor)
            {
                /// Nếu hết thời gian
                if (this.LifeTimeTicks >= this.DurationTicks)
                {
                    /// Thực thi sự kiện khi hết thời gian
                    this.OnTimeout();
                }
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

            /// Thực thi sự kiện OnClose
            try
            {
                this.OnClose();
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Exception, ex.ToString());
            }

            /// Xóa toàn bộ danh sách người chơi thuộc nhóm vào phụ bản
            this.teamPlayers.Clear();

            /// Xóa toàn bộ các đối tượng trong phụ bản
            this.RemoveAllNPCs();
            this.RemoveAllMonsters();
            this.RemoveAllGrowPoints();
            this.RemoveAllDynamicAreas();

            /// Đưa người chơi ra khỏi
            this.MovePlayersOutOfCopyScene();

            /// Xóa toàn bộ các đối tượng trong phụ bản
            this.ClearCopySceneObjects();

            /// Ngừng luồng thực thi
            ScheduleExecutor2.Instance.scheduleCancle(this.task);

            /// Xóa khỏi lớp điều khiển
            CopySceneEventManager.Remove(this);

            //Console.WriteLine("Remove copyscene => ID: {0}, Name: {1}", this.CopyScene.ID, this.CopyScene.Name);
        }
        #endregion

        #region Core
        /// <summary>
        /// Thiết lập ID phụ bản cho toàn bộ người chơi xử lý trường hợp toác
        /// </summary>
        private void SetCopySceneIDToAllPlayers()
        {
            /// Nếu chưa đến thời điểm kiểm tra Bug
            if (KTGlobal.GetCurrentTimeMilis() - this.LastCheckBugCopySceneID <= 5000)
            {
                return;
            }
            /// Đánh dấu thời điểm kiểm tra Bug
            this.LastCheckBugCopySceneID = KTGlobal.GetCurrentTimeMilis();

            /// Duyệt danh sách người chơi
            foreach (KPlayer player in this.teamPlayers)
            {
                /// Nếu đang ở bản đồ phụ bản mà toác CopySceneID
                if (player.CurrentMapCode == this.CopyScene.MapCode && player.CurrentCopyMapID != this.CopyScene.ID)
                {
                    /// Thiết lập lại
                    player.CurrentCopyMapID = this.CopyScene.ID;
                }
            }
        }

        /// <summary>
        /// Đưa tất cả người chơi vào phụ bản tương ứng
        /// </summary>
        /// <param name="players"></param>
        private void MovePlayersToCopyScene(List<KPlayer> players)
        {

            /// Duyệt danh sách người chơi, đẩy vào phụ bản
            foreach (KPlayer player in players)
            {
               
                /// Lưu thông tin bản đồ đứng trước đó
                KT_TCPHandler.UpdateLastMapInfo(player, player.CurrentMapCode, player.PosX, player.PosY);

                /// Thực hiện chuyển bản đồ
                KTPlayerManager.ChangeMap(player, this.CopyScene.MapCode, this.CopyScene.EnterPosX, this.CopyScene.EnterPosY, true);

                /// Lưu thông tin vị trí trước đó của người chơi
                KT_TCPHandler.UpdateCopySceneInfo(player, this.CopyScene.ID, this.CopyScene.InitTicks);
                /// Thiết lập phụ bản tương ứng
                player.CopyMapID = this.CopyScene.ID;
            }
        }

        /// <summary>
        /// Đưa tất cả người chơi rời khỏi phụ bản
        /// </summary>
        private void MovePlayersOutOfCopyScene()
        {
            /// Duyệt danh sách người chơi, đẩy vào phụ bản
            foreach (KPlayer player in this.GetPlayers())
            {
                /// Nếu đang đợi chuyển map thì thôi
                if (player.WaitingForChangeMap)
				{
                    continue;
				}

                /// Nếu không có bản đồ ra
                if (this.CopyScene.OutMapCode == -1 || this.CopyScene.OutPosX == -1 || this.CopyScene.OutPosY == -1)
                {
                    /// Lấy thông tin bản đồ trước đó
                    KT_TCPHandler.GetLastMapInfo(player, out int mapCode, out int posX, out int posY);
                    /// Đưa người chơi về bản đồ đó
                    KTPlayerManager.ChangeMap(player, mapCode, posX, posY, true);
                }
                /// Nếu có bản đồ ra
                else
                {
                    /// Đưa người chơi về bản đồ ra
                    KTPlayerManager.ChangeMap(player, this.CopyScene.OutMapCode, this.CopyScene.OutPosX, this.CopyScene.OutPosY, true);
                }
            }
        }

        /// <summary>
        /// Xóa toàn bộ đối tượng trong phụ bản
        /// </summary>
        private void ClearCopySceneObjects()
        {
            /// Xóa toàn bộ NPC trong phụ bản
            KTNPCManager.RemoveMapNpcs(this.CopyScene.MapCode, this.CopyScene.ID);

            /// Xóa toàn bộ điểm thu thập trong phụ bản
            KTGrowPointManager.RemoveMapGrowPoints(this.CopyScene.MapCode, this.CopyScene.ID);

            /// Xóa toàn bộ khu vực động trong phụ bản
            KTDynamicAreaManager.RemoveMapDynamicAreas(this.CopyScene.MapCode, this.CopyScene.ID);
        }
        #endregion

        #region Objects
        /// <summary>
        /// Trả về tổng số người chơi trong bản đồ
        /// </summary>
        /// <returns></returns>
        protected int GetTotalPlayers()
        {
            return KTPlayerManager.GetPlayersCount(this.CopyScene.MapCode, this.CopyScene.ID);
        }

        /// <summary>
        /// Trả về danh sách người chơi trong bản đồ hoạt động
        /// </summary>
        /// <returns></returns>
        protected List<KPlayer> GetPlayers()
        {
            /// Trả về kết quả
            return KTPlayerManager.FindAll(this.CopyScene.MapCode, this.CopyScene.ID);
        }

        /// <summary>
        /// Trả về tổng số quái trong bản đồ
        /// </summary>
        /// <returns></returns>
        protected int GetTotalMonsters()
        {
            /// Trả về kết quả
            return KTMonsterManager.GetTotalMonstersAtMap(this.CopyScene.MapCode, this.CopyScene.ID);
        }

        /// <summary>
        /// Trả về danh sách quái trong bản đồ
        /// </summary>
        /// <returns></returns>
        protected List<Monster> GetMonsters()
        {
            /// Trả về kết quả
            return KTMonsterManager.GetMonstersAtMap(this.CopyScene.MapCode, this.CopyScene.ID);
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
            return KTNPCManager.GetMapNPCs(this.CopyScene.MapCode, this.CopyScene.ID).Count;
        }

        /// <summary>
        /// Trả về danh sách NPC trong bản đồ
        /// </summary>
        /// <returns></returns>
        protected List<NPC> GetNPCs()
        {
            /// Danh sách NPC trong bản đồ
            return KTNPCManager.GetMapNPCs(this.CopyScene.MapCode, this.CopyScene.ID);
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
            return KTGrowPointManager.GetMapGrowPoints(this.CopyScene.MapCode, this.CopyScene.ID).Count;
        }

        /// <summary>
        /// Trả về danh sách điểm thu thập trong bản đồ
        /// </summary>
        /// <returns></returns>
        protected List<GrowPoint> GetGrowPoints()
        {
            /// Danh sách điểm thu thập trong bản đồ
            return KTGrowPointManager.GetMapGrowPoints(this.CopyScene.MapCode, this.CopyScene.ID);
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
            return KTDynamicAreaManager.GetMapDynamicAreas(this.CopyScene.MapCode, this.CopyScene.ID).Count;
        }

        /// <summary>
        /// Trả về danh sách khu vực động trong bản đồ
        /// </summary>
        /// <returns></returns>
        protected List<KDynamicArea> GetDynamicAreas()
        {
            /// Danh sách khu vực động trong bản đồ
            return KTDynamicAreaManager.GetMapDynamicAreas(this.CopyScene.MapCode, this.CopyScene.ID);
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
        /// <param name="delay"></param>
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
    }
}
