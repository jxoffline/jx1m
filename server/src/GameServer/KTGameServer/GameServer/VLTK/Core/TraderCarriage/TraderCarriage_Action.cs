#define FORCE_SYNS_POSITION_ON_STOP_MOVE

using GameServer.KiemThe;
using GameServer.KiemThe.Logic;
using GameServer.KiemThe.Logic.Manager.Skill.PoisonTimer;
using System;
using System.Collections.Generic;
using static GameServer.Logic.KTMapManager;

namespace GameServer.Logic
{
    /// <summary>
    /// Đối tượng xe tiêu
    /// </summary>
    public partial class TraderCarriage
    {
        #region Constants
        /// <summary>
        /// Khoảng cách đến đích tối đa
        /// </summary>
        private const int MaxDestinationDistance = 200;

        /// <summary>
        /// Thời gian tiếp tục hành trình sau khi bị đập
        /// </summary>
        private const int ContinueMovingAfterBeAttackedTicks = 2000;

        /// <summary>
        /// Khoảng thời gian quét quái xung quanh liên tục để cho vã
        /// </summary>
        private const int ScanMonstersAroundPeriod = 3000;

        /// <summary>
        /// Số quái quét tối đa
        /// </summary>
        private const int MaxMonstersScan = 10;

        /// <summary>
        /// Khoảng cách quá xa chủ nhân
        /// </summary>
        private const int MaxDistanceToOwner = 1000;

        /// <summary>
        /// Khoảng thời gian thông báo liên tục khi bị tấn công
        /// </summary>
        private const int NotifyOnUnderAttackTicks = 5000;
        #endregion

        #region Private fields
        /// <summary>
        /// Vị trí đích đến tiếp theo
        /// </summary>
        private UnityEngine.Vector2Int nextPos = UnityEngine.Vector2Int.zero;

        /// <summary>
        /// Thời điểm bị đập lần cuối
        /// </summary>
        private long lastBeAttackedTicks = 0;

        /// <summary>
        /// Đánh dấu đang đợi chủ nhân chuyển bản đồ không
        /// </summary>
        private bool waitingOwnerChangeMap = false;

        /// <summary>
        /// Thời điểm thông báo bị đập lần cuối
        /// </summary>
        private long lastNotifyOnUnderAttackTicks = 0;
        #endregion

        #region Properties
        /// <summary>
        /// Sự kiện Start
        /// </summary>
        public Action OnStart { get; set; }

        /// <summary>
        /// Sự kiện Tick
        /// </summary>
        public Action OnTick { get; set; }

        /// <summary>
        /// Sự kiện bị hủy
        /// </summary>
        public Action<KPlayer> OnBeKilled { get; set; }

        /// <summary>
        /// Sự kiện khi hết thời gian
        /// </summary>
        public Action OnBeTimeout { get; set; }

        /// <summary>
        /// Sự kiện vận tiêu thành công
        /// </summary>
        public Action Complete { get; set; }
        #endregion

        #region Core
        /// <summary>
        /// Hàm này gọi đến khi bắt đầu Timer của đối tượng
        /// </summary>
        public void Start()
        {
            this.OnStart?.Invoke();
        }

        /// <summary>
        /// Hàm này gọi đến trước khi chủ nhân chuyển bản đồ
        /// </summary>
        public void OnPreChangeMap()
        {
            /// Nếu có đích đến
            if (this.nextPos != UnityEngine.Vector2Int.zero)
            {
                /// Vị trí hiện tại
                UnityEngine.Vector2Int currentPos = new UnityEngine.Vector2Int((int) this.CurrentPos.X, (int) this.CurrentPos.Y);
                /// Nếu xe tiêu chưa chạy đến đích
                if (UnityEngine.Vector2Int.Distance(this.nextPos, currentPos) > TraderCarriage.MaxDestinationDistance)
                {
                    /// Thông báo
                    KTPlayerManager.ShowNotification(this.Owner, "Xe tiêu chưa đến đích, không thể đi tiếp, tự động bị hủy.");
                    /// Thực thi sự kiện bị phá hủy
                    this.OnBeKilled?.Invoke(null);
                    /// Xóa đối tượng
                    KTTraderCarriageManager.RemoveTraderCarriage(this);
                    /// Bỏ qua luôn
                    return;
                }
            }

            /// Bản đồ cũ
            GameMap gameMap = KTMapManager.Find(this.CurrentMapCode);
            /// Xóa đối tượng khỏi bản đồ cũ
            gameMap.Grid.RemoveObject(this);

            /// Đánh dấu đang đợi chủ nhân chuyển bản đồ
            this.waitingOwnerChangeMap = true;

            /// Ngừng StoryBoard
            KTTraderCarriageStoryBoardEx.Instance.Remove(this, false);
        }

        /// <summary>
        /// Hàm này gọi đến khi chủ nhân vào bản đồ
        /// </summary>
        public void OnEnterMap()
        {
            /// Nếu chưa đến đích
            if (this.Paths.Count > 0)
            {
                /// Đích đến tiếp theo
                KeyValuePair<int, UnityEngine.Vector2Int> nextLocation = this.Paths.Peek();
                /// Nếu khác bản đồ
                if (nextLocation.Key != this.CurrentMapCode)
                {
                    /// Thông báo
                    KTPlayerManager.ShowNotification(this.Owner, "Khoảng cách đến xe tiêu quá xa, tự động bị hủy.");
                    /// Thực thi sự kiện bị phá hủy
                    this.OnBeKilled?.Invoke(null);
                    /// Xóa đối tượng
                    KTTraderCarriageManager.RemoveTraderCarriage(this);
                    /// Bỏ qua luôn
                    return;
                }

                /// Hủy đích đến
                this.nextPos = UnityEngine.Vector2Int.zero;
                /// Thiết lập vị trí của đối tượng
                this.CurrentPos = new System.Windows.Point(nextLocation.Value.x, nextLocation.Value.y);
                /// Bản đồ
                GameMap gameMap = KTMapManager.Find(this.CurrentMapCode);
                /// Cập nhật vị trí đối tượng vào Map
                gameMap.Grid.MoveObject((int) this.CurrentPos.X, (int) this.CurrentPos.Y, this);
            }

            ///// Tạm bỏ đoạn này, do lỗi đéo gì đó lúc chuyển map, thay vào đó khi nhân vật di chuyển thì mới kích hoạt chức năng này
            ///// Bỏ đánh dấu đang đợi chủ nhân chuyển bản đồ
            //this.waitingOwnerChangeMap = false;
        }

        /// <summary>
        /// Hàm này gọi tới khi chủ nhân di chuyển
        /// </summary>
        public void OnOwnerMove()
        {
            /// Bỏ đánh dấu đang đợi chủ nhân chuyển bản đồ
            this.waitingOwnerChangeMap = false;
        }

        /// <summary>
        /// Hàm này gọi liên tục mỗi 0.5s trong Timer của đối tượng
        /// </summary>
        public override void Tick()
        {
            /// Gọi đến Base
            base.Tick();

            /// Thực thi sự kiện Tick
            this.OnTick?.Invoke();

            /// Toác
            if (this.Owner == null)
            {
                return;
            }
            else if (this.Owner.IsDead())
            {
                return;
            }
            else if (!this.Owner.IsOnline())
            {
                return;
            }
            /// Nếu chủ nhân đang chuyển map
            else if (this.Owner.WaitingForChangeMap || this.waitingOwnerChangeMap)
            {
                return;
            }

            /// Vị trí của chủ nhân
            UnityEngine.Vector2 ownerPos = new UnityEngine.Vector2(this.Owner.PosX, this.Owner.PosY);
            /// Vị trí của bản thân
            UnityEngine.Vector2 selfPos = new UnityEngine.Vector2((int) this.CurrentPos.X, (int) this.CurrentPos.Y);
            /// Nếu quá xa
            if (UnityEngine.Vector2.Distance(ownerPos, selfPos) > TraderCarriage.MaxDistanceToOwner)
            {
                ///// Thông báo
                //KTPlayerManager.ShowNotification(this.Owner, "Khoảng cách đến xe tiêu quá xa, tự động bị hủy.");
                ///// Thực thi sự kiện bị phá hủy
                //this.OnBeKilled?.Invoke(null);
                ///// Xóa đối tượng
                //KTTraderCarriageManager.RemoveTraderCarriage(this);

                /// Ngừng StoryBoard
                KTTraderCarriageStoryBoardEx.Instance.Remove(this);
#if FORCE_SYNS_POSITION_ON_STOP_MOVE
                /// Cập nhật vị trí của nó (tạm thời như này vì một số bọn dùng bản APK và IOS cũ nên đéo update được ở packet StopMove)
                KT_TCPHandler.SendBlinkToPosition(this, (int) this.CurrentPos.X, (int) this.CurrentPos.Y, 0.1f);
#endif
                /// Bỏ qua luôn
                return;
            }

            /// Cứ 3s 1 lần sẽ quét toàn bộ quái xung quanh
            if (this.LifeTime % TraderCarriage.ScanMonstersAroundPeriod == 0)
            {
                /// Danh sách kẻ địch xung quanh
                List<Monster> monsters = KTGlobal.GetNearByEnemies<Monster>(this, this.Vision, TraderCarriage.MaxMonstersScan);
                /// Duyệt danh sách quái
                foreach (Monster monster in monsters)
                {
                    /// Trigger sự kiện này để quái nó đuổi đánh xe tiêu
                    monster.TakeDamage(this, 0);
                }
            }

            /// Nếu đang di chuyển thì thôi
            if (KTTraderCarriageStoryBoardEx.Instance.HasStoryBoard(this))
            {
                return;
            }

            /// Nếu bị đập chưa phục hồi
            if (KTGlobal.GetCurrentTimeMilis() - this.lastBeAttackedTicks < TraderCarriage.ContinueMovingAfterBeAttackedTicks)
            {
                return;
            }

            /// Nếu có đích đến cũ
            if (this.nextPos != UnityEngine.Vector2Int.zero)
            {
                /// Vị trí hiện tại
                UnityEngine.Vector2Int currentPos = new UnityEngine.Vector2Int((int) this.CurrentPos.X, (int) this.CurrentPos.Y);
                /// Nếu chưa tiếp cận
                if (UnityEngine.Vector2Int.Distance(this.nextPos, currentPos) > TraderCarriage.MaxDestinationDistance)
                {
                    /// Nếu chưa đến đích mà chưa có StoryBoard nghĩa là bị đập xong đứng lại
                    if (!KTTraderCarriageStoryBoardEx.Instance.HasStoryBoard(this))
                    {
                        /// Thực hiện di chuyển đến vị trí đích
                        KTTraderCarriageStoryBoardEx.Instance.Add(this, this.CurrentPos, new System.Windows.Point(this.nextPos.x, this.nextPos.y), KiemThe.Entities.KE_NPC_DOING.do_run, false, true);
                    }
                    /// Bỏ qua
                    return;
                }
                /// Nếu đã tiếp cận
                else
                {
                    /// Thông báo vận tiêu thành công
                    KTPlayerManager.ShowNotification(this.Owner, "Xe tiêu đã đến điểm truyền tống, hãy mau mau chuyển cảnh!");
                }
            }

            /// Nếu đã đến đích thành công tức danh sách đường đi rỗng
            if (this.Paths.Count <= 0)
            {
                /// Lưu lại thông tin nhiệm vụ vận tiêu thành công chưa nhận thưởng
                this.Owner.CurrentCompletedCargoCarriageTask = this.Owner.CurrentCargoCarriageTask;
                /// Thực thi sự kiện vận tiêu thành công
                this.Complete?.Invoke();
                /// Thông báo vận tiêu thành công
                KTPlayerManager.ShowNotification(this.Owner, "Vận tiêu thành công!");
                /// Xóa đối tượng
                KTTraderCarriageManager.RemoveTraderCarriage(this);
            }
            /// Nếu chưa đến đích
            else
            {
                /// Đích đến tiếp theo
                KeyValuePair<int, UnityEngine.Vector2Int> nextLocation = this.Paths.Peek();
                /// Nếu trùng bản đồ
                if (nextLocation.Key == this.CurrentMapCode)
                {
                    /// Lấy ra khỏi Queue
                    this.Paths.Dequeue();

                    /// Thực hiện di chuyển đến vị trí đích
                    KTTraderCarriageStoryBoardEx.Instance.Add(this, this.CurrentPos, new System.Windows.Point(nextLocation.Value.x, nextLocation.Value.y), KiemThe.Entities.KE_NPC_DOING.do_run, false, true);

                    /// Lưu lại vị trí tiếp theo
                    this.nextPos = nextLocation.Value;
                }
            }
        }

        /// <summary>
        /// Sự kiện khi chủ nhân của xe tiêu bị chết
        /// </summary>
        /// <param name="attacker"></param>
        public void OnOwnerDie(GameObject attacker)
        {
            /// Toác
            if (attacker == null)
            {
                return;
            }

            /// Thằng người giết
            KPlayer killer = null;
            /// Nếu thằng giết là pet
            if (attacker is Pet pet)
            {
                killer = pet.Owner;
            }
            /// Nếu thằng giết là người
            else if (attacker is KPlayer)
            {
                killer = attacker as KPlayer;
            }

            /// Nếu là người
            if (killer != null)
            {
                /// Thông báo
                KTPlayerManager.ShowNotification(this.Owner, string.Format("<color=red>Xe tiêu của bạn đã bị <color=yellow>[{0}]</color> phá hủy!</color>", killer.RoleName));
            }
            /// Nếu là quái
            else
            {
                /// Thông báo
                KTPlayerManager.ShowNotification(this.Owner, string.Format("<color=red>Xe tiêu của bạn đã bị <color=yellow>[{0}]</color> phá hủy!</color>", attacker.RoleName));
            }

            /// Thực thi sự kiện
            this.OnBeKilled?.Invoke(killer);

            /// Xóa xe tiêu
            KTTraderCarriageManager.RemoveTraderCarriage(this);
        }

        /// <summary>
        /// Hàm này gọi đến khi đối tượng bị tấn công
        /// </summary>
        /// <param name="attacker"></param>
        /// <param name="nDamage"></param>
        public override void OnBeHit(GameObject attacker, int nDamage)
        {
            /// Gọi phương thức cha
            base.OnBeHit(attacker, nDamage);

            /// Ghi lại thời điểm bị đánh
            this.lastBeAttackedTicks = KTGlobal.GetCurrentTimeMilis();

            /// Ngừng StoryBoard
            KTTraderCarriageStoryBoardEx.Instance.Remove(this);
#if FORCE_SYNS_POSITION_ON_STOP_MOVE
            /// Cập nhật vị trí của nó (tạm thời như này vì một số bọn dùng bản APK và IOS cũ nên đéo update được ở packet StopMove)
            KT_TCPHandler.SendBlinkToPosition(this, (int) this.CurrentPos.X, (int) this.CurrentPos.Y, 0.1f);
#endif

            /// Nếu không có chủ nhân thì thôi
            if (this.Owner == null)
            {
                return;
            }

            /// Nếu đã đến thời điểm thông báo bị đập
            if (KTGlobal.GetCurrentTimeMilis() - this.lastNotifyOnUnderAttackTicks >= TraderCarriage.NotifyOnUnderAttackTicks)
            {
                /// Thông báo tới chủ nhân là đang bị đập
                KTPlayerManager.ShowNotification(this.Owner, "Xe tiêu của bạn đang bị tấn công, hãy mau bảo vệ!");
                /// Đánh dấu thời điểm thông báo lần trước
                this.lastNotifyOnUnderAttackTicks = KTGlobal.GetCurrentTimeMilis();
            }
        }

        /// <summary>
        /// Hàm này gọi đến khi đối tượng bị giết
        /// </summary>
        /// <param name="attacker"></param>
        public override void OnDie(GameObject attacker)
        {
            /// Toác
            if (attacker == null)
            {
                return;
            }

            /// Gọi phương thức cha
            base.OnDie(attacker);

            /// Thằng người giết
            KPlayer killer = null;
            /// Nếu thằng giết là pet
            if (attacker is Pet pet)
            {
                killer = pet.Owner;
            }
            /// Nếu thằng giết là người
            else if (attacker is KPlayer)
            {
                killer = attacker as KPlayer;
            }

            /// Nếu là người
            if (killer != null)
            {
                /// Thông báo
                KTPlayerManager.ShowNotification(this.Owner, string.Format("<color=red>Xe tiêu của bạn đã bị <color=yellow>[{0}]</color> phá hủy!</color>", killer.RoleName));
            }
            /// Nếu là quái
            else
            {
                /// Thông báo
                KTPlayerManager.ShowNotification(this.Owner, string.Format("<color=red>Xe tiêu của bạn đã bị <color=yellow>[{0}]</color> phá hủy!</color>", attacker.RoleName));
            }

            /// Thực thi sự kiện
            this.OnBeKilled?.Invoke(killer);

            /// Xóa đối tượng
            KTTraderCarriageManager.RemoveTraderCarriage(this);
        }

        /// <summary>
        /// Hàm này gọi khi đối tượng hết thời gian
        /// </summary>
        public void OnTimeout()
        {
            /// Thực thi sự kiện
            this.OnBeTimeout?.Invoke();
            /// Thực thi sự kiện bị phá hủy
            this.OnBeKilled?.Invoke(null);

            /// Thông báo
            KTPlayerManager.ShowNotification(this.Owner, "Thời gian vận tiêu đã hết, xe tiêu tự động bị hủy.");

            /// Xóa đối tượng
            KTTraderCarriageManager.RemoveTraderCarriage(this);
        }

        /// <summary>
        /// Thực hiện Reset đối tượng
        /// </summary>
        public void Reset()
        {
            /// Nếu có chủ nhân
            if (this.Owner != null)
            {
                /// Hủy xe tiêu
                this.Owner.CurrentTraderCarriage = null;
                /// Hủy nhiệm vụ vận tiêu
                this.Owner.CurrentCargoCarriageTask = null;
            }

            /// Bản đồ
            GameMap gameMap = KTMapManager.Find(this.CurrentMapCode);
            /// Cập nhật vị trí đối tượng vào Map
            gameMap.Grid.RemoveObject(this);

            /// Làm rỗng danh sách đường đi
            this.Paths.Clear();

            /// Xóa toàn bộ Buff và vòng sáng tương ứng
            this.Buffs.RemoveAllBuffs();
            this.Buffs.RemoveAllAruas();
            this.Buffs.RemoveAllAvoidBuffs();

            /// Xóa toàn bộ kỹ năng tự kích hoạt tương ứng
            this.RemoveAllAutoSkills();

            /// Xóa StoryBoard
            KTTraderCarriageStoryBoardEx.Instance.Remove(this, false);

            /// Xóa luồng trúng độc
            KTPoisonTimerManager.Instance.RemovePoisonState(this);
        }
        #endregion
    }
}
