using GameServer.KiemThe.Entities;
using GameServer.KiemThe.Logic.Manager.Skill.PoisonTimer;
using GameServer.Logic;
using Server.Tools;
using System;
using System.ComponentModel;

namespace GameServer.KiemThe.Logic
{
    /// <summary>
    /// Các hàm cơ bản
    /// </summary>
    public partial class KTMonsterTimerManager
    {
        #region Core
        /// <summary>
        /// Đánh dấu buộc xóa toàn bộ Timer của quái
        /// </summary>
        private bool forceClearAllMonsterTimers = false;

        /// <summary>
        /// Sự kiện khi Background Worker hoàn tất công việc
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Worker_Completed(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                LogManager.WriteLog(LogTypes.Exception, e.Error.ToString());
            }
        }

        /// <summary>
		/// Thực thi sự kiện gì đó
		/// </summary>
		/// <param name="work"></param>
		/// <param name="onError"></param>
		private void ExecuteAction(Action work, Action<Exception> onError)
        {
            try
            {
                work?.Invoke();
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Exception, ex.ToString());
                onError?.Invoke(ex);
            }
        }

        /// <summary>
        /// Xóa rỗng toàn bộ luồng quái
        /// </summary>
        public void ClearMonsterTimers()
        {
            this.forceClearAllMonsterTimers = true;
        }
        #endregion

        #region API
        /// <summary>
        /// Thêm đối tượng vào danh sách quản lý
        /// </summary>
        /// <param name="obj"></param>
        public void Add(Monster obj)
        {
            if (obj == null)
            {
                return;
            }

            /// Nếu là quái tĩnh thì thôi
            if (obj.MonsterType == MonsterAIType.Static || obj.MonsterType == MonsterAIType.Static_ImmuneAll)
            {
                return;
            }

            MonsterTimer timer = new MonsterTimer()
            {
                TickTime = 500,
                Owner = obj,
                IsStarted = false,
                LastTick = 0,
                LifeTime = 0,
                Start = () => {
                    /// Nếu là loại đặc biệt
                    if (obj.MonsterType == MonsterAIType.Special_Normal || obj.MonsterType == MonsterAIType.Special_Boss)
                    {
                        this.SpecialMonster_ProcessStart(obj);
                    }
                    /// Nếu là loại tùy chọn
                    else if (obj.MonsterType == MonsterAIType.DynamicNPC)
                    {
                        this.CustomMonster_ProcessStart(obj);
                    }
                    /// Nếu là loại thường
                    else
                    {
                        this.NormalMonster_ProcessStart(obj);
                    }
                },
                AIRandomMoveTick = KTGlobal.GetRandomNumber(KTMonsterTimerManager.AIRandomMoveTickMin, KTMonsterTimerManager.AIRandomMoveTickMax) * 1000,
                HasCompletedLastTick = true,
            };
            timer.Tick = () => {
                /// Nếu là loại đặc biệt
                if (obj.MonsterType == MonsterAIType.Special_Normal || obj.MonsterType == MonsterAIType.Special_Boss)
                {
                    this.SpecialMonster_ProcessTick(obj);
                }
                /// Nếu là loại tùy chọn
                else if (obj.MonsterType == MonsterAIType.DynamicNPC)
                {
                    this.CustomMonster_ProcessTick(obj);
                }
                /// Nếu là loại quái thường
                else
                {
                    this.NormalMonster_ProcessTick(obj);
                }

                /// Nếu mở AI tự di chuyển
                if (ServerConfig.Instance.EnableMonsterAIRandomMove)
                {
                    timer.LifeTime += 500;
                    if (timer.LifeTime % timer.AIRandomMoveTick == 0)
                    {
                        /// Nếu là loại đặc biệt
                        if (obj.MonsterType == MonsterAIType.Special_Normal || obj.MonsterType == MonsterAIType.Special_Boss)
                        {
                            this.SpecialMonster_ProcessAIMoveTick(obj);
                        }
                        /// Nếu là loại tùy chọn
                        else if (obj.MonsterType == MonsterAIType.DynamicNPC)
                        {
                            /// Không xử lý
                        }
                        /// Nếu là loại thường
                        else
                        {
                            this.NormalMonster_ProcessAIMoveTick(obj);
                        }
                    }
                }

                /// Đánh dấu đã hoàn thành Tick lần trước
                timer.HasCompletedLastTick = true;
            };

            /// Nếu là quái thường loại đặc biệt
            if (obj.MonsterType == MonsterAIType.Special_Normal)
            {
                /// Thêm vào danh sách cần tải
                this.waitingQueueSpecial.Enqueue(new QueueItem()
                {
                    Type = 1,
                    Data = timer,
                });
            }
            /// Nếu là quái tùy chọn
            else if (obj.MonsterType == MonsterAIType.DynamicNPC)
            {
                /// Thêm vào danh sách cần tải
                this.waitingQueueDynamicNPC.Enqueue(new QueueItem()
                {
                    Type = 1,
                    Data = timer,
                });
            }
            /// Nếu là boss loại đặc biệt
            else if (obj.MonsterType == MonsterAIType.Special_Boss)
            {
                /// Thêm vào danh sách cần tải
                this.waitingQueueForSpecialBoss.Enqueue(new QueueItem()
                {
                    Type = 1,
                    Data = timer,
                });
            }
            /// Nếu là quái thường
            else if (obj.MonsterType != MonsterAIType.Boss && obj.MonsterType != MonsterAIType.Pirate)
            {
                /// Thêm vào danh sách cần tải
                this.waitingQueue.Enqueue(new QueueItem()
                {
                    Type = 1,
                    Data = timer,
                });
            }
            /// Nếu là Boss
            else
            {
                /// Thêm vào danh sách cần tải
                this.waitingQueueForBoss.Enqueue(new QueueItem()
                {
                    Type = 1,
                    Data = timer,
                });
            }
        }

        /// <summary>
        /// Dừng và xóa đối tượng khỏi luồng thực thi
        /// </summary>
        /// <param name="obj"></param>
        public void Remove(Monster obj)
        {
            if (obj == null)
            {
                return;
            }

            /// Nếu là quái tĩnh thì thôi
            if (obj.MonsterType == MonsterAIType.Static || obj.MonsterType == MonsterAIType.Static_ImmuneAll)
            {
                return;
            }

            /// Nếu là quái thường loại đặc biệt
            if (obj.MonsterType == MonsterAIType.Special_Normal)
            {
                /// Thêm vào danh sách cần tải
                this.waitingQueueSpecial.Enqueue(new QueueItem()
                {
                    Type = 0,
                    Data = obj,
                });
            }
            /// Nếu là quái tùy chọn
            else if (obj.MonsterType == MonsterAIType.DynamicNPC)
            {
                /// Thêm vào danh sách cần tải
                this.waitingQueueDynamicNPC.Enqueue(new QueueItem()
                {
                    Type = 0,
                    Data = obj,
                });
            }
            /// Nếu là boss loại đặc biệt
            else if (obj.MonsterType == MonsterAIType.Special_Boss)
            {
                /// Thêm vào danh sách cần tải
                this.waitingQueueForSpecialBoss.Enqueue(new QueueItem()
                {
                    Type = 0,
                    Data = obj,
                });
            }
            /// Nếu là quái thường
            else if (obj.MonsterType != MonsterAIType.Boss && obj.MonsterType != MonsterAIType.Pirate)
            {
                /// Thêm vào danh sách cần tải
                this.waitingQueue.Enqueue(new QueueItem()
                {
                    Type = 0,
                    Data = obj,
                });
            }
            /// Nếu là Boss
            else
            {
                /// Thêm vào danh sách cần tải
                this.waitingQueueForBoss.Enqueue(new QueueItem()
                {
                    Type = 0,
                    Data = obj,
                });
            }
        }
        #endregion
    }
}
