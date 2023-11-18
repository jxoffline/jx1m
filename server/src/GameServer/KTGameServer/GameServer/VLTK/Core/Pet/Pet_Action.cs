using GameServer.KiemThe;
using GameServer.KiemThe.Entities;
using GameServer.KiemThe.Logic;
using GameServer.KiemThe.Logic.Manager.Skill.PoisonTimer;
using GameServer.KiemThe.Utilities;
using GameServer.VLTK.Entities.Pet;
using Server.Data;
using System;
using System.Linq;
using static GameServer.Logic.KTMapManager;

namespace GameServer.Logic
{
    /// <summary>
    /// Đối tượng Pet
    /// </summary>
    public partial class Pet
    {
        #region Constants
        /// <summary>
        /// Bán kính vòng di chuyển ngẫu nhiên xung quanh chủ nhân
        /// </summary>
        private const int RandomMoveRadius = 200;

        /// <summary>
        /// Khoảng cách quá xa chủ nhân
        /// </summary>
        private const int MaxDistanceToOwner = 1000;
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
        /// Đã bị xóa chưa
        /// </summary>
        public bool IsDestroyed
        {
            get
            {
                return this.isDestroyed;
            }
        }
        #endregion

        #region Private fields
        /// <summary>
        /// Vị trí so với chủ nhân
        /// </summary>
        private UnityEngine.Vector2 positionToOwner;

        /// <summary>
        /// Đã bị xóa chưa
        /// </summary>
        private bool isDestroyed = false;
        #endregion

        #region Core
        /// <summary>
        /// Hàm này gọi đến khi bắt đầu Timer của đối tượng
        /// </summary>
        public void Start()
        {
            this.OnStart?.Invoke();

            /// Thực hiện cập nhật vị trí
            this.Tick();
        }

        /// <summary>
        /// Hàm này gọi đến trước khi chủ nhân chuyển bản đồ
        /// </summary>
        public void OnPreChangeMap()
        {
            /// Bản đồ cũ
            GameMap gameMap = KTMapManager.Find(this.CurrentMapCode);
            /// Xóa đối tượng khỏi bản đồ cũ
            gameMap.Grid.RemoveObject(this);
        }

        /// <summary>
        /// Hàm này gọi đến khi chủ nhân vào bản đồ
        /// </summary>
        public void OnEnterMap()
        {
            /// Thiết lập vị trí của đối tượng
            this.CurrentPos = this.Owner.CurrentPos;
            /// Bản đồ cũ
            GameMap gameMap = KTMapManager.Find(this.CurrentMapCode);
            /// Cập nhật vị trí đối tượng vào Map
            gameMap.Grid.MoveObject((int) this.Owner.CurrentPos.X, (int) this.Owner.CurrentPos.Y, this);
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
            else if (this.Owner.WaitingForChangeMap)
            {
                return;
            }
            /// Nếu đã bị hủy thì thôi
            else if (this.isDestroyed)
            {
                return;
            }

            /// Nếu đã đến thời gian giảm độ vui vẻ
            if (KTGlobal.GetCurrentTimeMilis() - this.LastCostJoyTicks >= KPet.Config.SubJoyInterval)
            {
                /// Đánh dấu thời điểm cuối giảm độ vui vẻ
                this.LastCostJoyTicks = KTGlobal.GetCurrentTimeMilis();
                /// Giảm 1 điểm vui vẻ
                this.Joyful--;
                /// Dữ liệu pet của người chơi
                PetData data = this.Owner.PetList.Where(x => x.ID == this.RoleID - (int) ObjectBaseID.Pet).FirstOrDefault();
                /// Nếu tồn tại
                if (data != null)
                {
                    /// Thông báo
                    KTPlayerManager.ShowNotification(this.Owner, string.Format("Tinh linh [{0}], vui vẻ giảm 1 điểm.", this.RoleName));
                    /// Lưu lại giá trị vui vẻ
                    data.Joyful = this.Joyful;
                    /// Thông báo về Client
                    KT_TCPHandler.NotifyPetBaseAttributes(this.Owner, data);
                }
            }

            /// Nếu số điểm vui vẻ không đủ
            if (this.Joyful < KPet.Config.CallFightRequịreJoyOver)
            {
                /// Thông báo
                KTPlayerManager.ShowNotification(this.Owner, string.Format("Độ vui vẻ của tinh linh không đủ {0} điểm, tự động nghỉ ngơi.", KPet.Config.CallFightRequịreJoyOver));
                /// Thu hồi
                this.Owner.CallBackPet(true);
            }
            /// Nếu số tuổi thọ không đủ
            else if (this.Life < KPet.Config.CallFightRequịreLifeOver)
            {
                /// Thông báo
                KTPlayerManager.ShowNotification(this.Owner, string.Format("Tuổi thọ của tinh linh không đủ {0} điểm, tự động nghỉ ngơi.", KPet.Config.CallFightRequịreLifeOver));
                /// Thu hồi
                this.Owner.CallBackPet(true);
            }

            /// Nếu chủ nhân đang di chuyển
            if (KTPlayerStoryBoardEx.Instance.HasStoryBoard(this.Owner) && KTGlobal.GetCurrentTimeMilis() - this.Owner.LastStoryBoardTicks < 500)
            {
                /// Cập nhật vị trí của Pet về chủ
                this.CurrentPos = this.Owner.CurrentPos;
                /// Bản đồ cũ
                GameMap gameMap = KTMapManager.Find(this.CurrentMapCode);

                /// Nếu đã chết
                if (this.IsDead())
                {
                    /// Xóa khỏi bản đồ
                    gameMap.Grid.RemoveObject(this);
                }
                /// Nếu còn sống
                else
                {
                    /// Cập nhật vị trí đối tượng vào Map
                    gameMap.Grid.MoveObject((int) this.CurrentPos.X, (int) this.CurrentPos.Y, this);
                }

                /// Bỏ qua
                return;
            }

            /// Vị trí của chủ nhân
            UnityEngine.Vector2 ownerPos = new UnityEngine.Vector2(this.Owner.PosX, this.Owner.PosY);
            /// Vị trí của pet
            UnityEngine.Vector2 petPos = new UnityEngine.Vector2((int) this.CurrentPos.X, (int) this.CurrentPos.Y);
            /// Nếu quá xa
            if (UnityEngine.Vector2.Distance(ownerPos, petPos) > Pet.MaxDistanceToOwner)
            {
                /// Đổi vị trí ngẫu nhiên xung quanh
                this.positionToOwner = KTMath.GetRandomPointAroundPos(UnityEngine.Vector2.zero, Pet.RandomMoveRadius);
                /// Cập nhật vị trí mới
                petPos = ownerPos + this.positionToOwner;
                /// Cập nhật vị trí
                this.CurrentPos = new System.Windows.Point((int) petPos.x, (int) petPos.y);

                /// Thông báo về Client
                KT_TCPHandler.SendPetChangePositionToClients(this, null, true);

                /// Bản đồ cũ
                GameMap gameMap = KTMapManager.Find(this.CurrentMapCode);
                
                /// Nếu đã chết
                if (this.IsDead())
                {
                    /// Xóa khỏi bản đồ
                    gameMap.Grid.RemoveObject(this);
                }
                /// Nếu còn sống
                else
                {
                    /// Cập nhật vị trí đối tượng vào Map
                    gameMap.Grid.MoveObject((int) petPos.x, (int) petPos.y, this);
                }
            }
        }

        /// <summary>
        /// Thực hiện Reset đối tượng
        /// </summary>
        /// <param name="alsoRemoveFromManager"></param>
        public void Destroy(bool alsoRemoveFromManager = false)
        {
            /// Đánh dấu đã bị xóa
            this.isDestroyed = true;

            /// Bản đồ cũ
            GameMap gameMap = KTMapManager.Find(this.CurrentMapCode);
            /// Cập nhật vị trí đối tượng vào Map
            gameMap.Grid.RemoveObject(this);

            /// Nếu đồng thời xóa khỏi Manager
            if (alsoRemoveFromManager)
            {
                KTPetManager.RemovePet(this);
            }

            /// Xóa toàn bộ Buff và vòng sáng tương ứng
            this.Buffs.RemoveAllBuffs();
            this.Buffs.RemoveAllAruas();
            this.Buffs.RemoveAllAvoidBuffs();

            /// Xóa toàn bộ kỹ năng tự kích hoạt tương ứng
            this.RemoveAllAutoSkills();

            /// Xóa luồng trúng độc
            KTPoisonTimerManager.Instance.RemovePoisonState(this);
        }
        #endregion
    }
}
