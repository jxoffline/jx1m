using System.Collections.Generic;
using FS.Drawing;
using UnityEngine;
using FS.GameEngine.Logic;
using FS.GameEngine.Interface;
using Server.Data;
using static FS.VLTK.Entities.Enum;
using FS.VLTK.Logic;
using FS.VLTK.Factory;
using FS.VLTK;
using FS.VLTK.Utilities.Threading;
using FS.VLTK.Loader;
using FS.VLTK.Entities.Config;

namespace FS.GameEngine.Sprite
{
    /// <summary>
    /// Trạng thái ngũ hành của đối tượng
    /// </summary>
    public class SpriteSeriesState
    {
        /// <summary>
        /// ID trạng thái
        /// </summary>
        public KE_STATE State { get; set; }

        /// <summary>
        /// Thời gian duy trì
        /// </summary>
        public float Duration { get; set; }

        /// <summary>
        /// Thời gian tồn tại
        /// </summary>
        public float LifeTime { get; set; } = 0f;

        /// <summary>
        /// Còn tồn tại không
        /// </summary>
        public bool IsAlive
        {
            get
            {
                return this.LifeTime < this.Duration;
            }
        }
    }

    /// <summary>
    /// Thực thi toàn bộ hành động của các Sprite trong game
    /// </summary>
    public partial class GSprite : IObject
    {
        #region Khởi tạo

        /// <summary>
        /// Khởi tạo hàm
        /// </summary>
        public GSprite()
        {
        }

        #endregion

        #region Component 2D
        /// <summary>
        /// Tọa độ đối tượng dạng Vector2
        /// </summary>
        public Vector2 PositionInVector2
        {
            get
            {
                return new Vector2(this.PosX, this.PosY);
            }
        }

        private GameObject _Role2D;
        /// <summary>
        /// Script điều khiển nhân vật 2D
        /// </summary>
        public GameObject Role2D
        {
            get
            {
                return this._Role2D;
            }
            set
            {
                this._Role2D = value;

                if (value != null)
                {
                    this.ComponentCharacter = this._Role2D.GetComponent<FS.VLTK.Control.Component.Character>();
                    this.ComponentMonster = this._Role2D.GetComponent<FS.VLTK.Control.Component.Monster>();
                    this.ComponentTeleport = this._Role2D.GetComponent<FS.VLTK.Control.Component.Teleport>();
                }
                else
                {
                    this.ComponentCharacter = null;
                    this.ComponentMonster = null;
                    this.ComponentTeleport = null;
                }
            }
        }

        /// <summary>
        /// Thành phần này khác NULL nếu là nhân vật hoặc người chơi khác
        /// </summary>
        public FS.VLTK.Control.Component.Character ComponentCharacter { get; private set; }

        /// <summary>
        /// Thành phần này khác NULL nếu là quái hoặc NPC
        /// </summary>
        public FS.VLTK.Control.Component.Monster ComponentMonster { get; private set; }

        /// <summary>
        /// Thành phần này khác NULL nếu là cổng dịch chuyển
        /// </summary>
        public FS.VLTK.Control.Component.Teleport ComponentTeleport { get; private set; }
        #endregion

        #region Kế thừa lớp IObject
        /// <summary>
        /// BaseID đối tượng
        /// </summary>
        public int BaseID { get; set; }

        /// <summary>
        /// Tên đối tượng
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Đã khởi tạo chưa
        /// </summary>
        public bool InitStatus { get; private set; }

        /// <summary>
        /// Tọa độ thực
        /// </summary>
        public Point Coordinate
        {
            get { return new Point(PosX, PosY); }
            set
            {
                PosX = value.X;
                PosY = value.Y;

                this.ChangeCoordinateProperty();
            }
        }

        /// <summary>
        /// Cập nhật tọa độ đối tượng 2D tương ứng
        /// </summary>
        /// <param name="p"></param>
        public void ProcessRole2DCoordinate(Point p)
        {
            Vector2 position = new Vector2(p.X, p.Y);
            if (!this.Role2D)
            {
                return;
            }
            this.Role2D.transform.localPosition = position;

            if (this.ComponentCharacter != null)
            {
                this.ComponentCharacter.OnPositionChanged();
            }
            else if (this.ComponentMonster != null)
            {
                this.ComponentMonster.OnPositionChanged();
            }
            else if (this.ComponentTeleport != null)
            {
                this.ComponentTeleport.OnPositionChanged();
            }
        }

        /// <summary>
        /// Hàm gọi đến khi vị trí thay đổi
        /// </summary>
        private void ChangeCoordinateProperty()
        {
            if (this.RoleData != null)
			{
                this.RoleData.PosX = this._PosX;
                this.RoleData.PosY = this._PosY;
			}
            else if (this.MonsterData != null)
			{
                this.MonsterData.PosX = this._PosX;
                this.MonsterData.PosY = this._PosY;
			}
            else if (this.NPCData != null)
			{
                this.NPCData.PosX = this._PosX;
                this.NPCData.PosY = this._PosY;
			}
            else if (this.GPData != null)
			{
                this.GPData.PosX = this._PosX;
                this.GPData.PosY = this._PosY;
            }
            else if (this.DynAreaData != null)
            {
                this.DynAreaData.PosX = this._PosX;
                this.DynAreaData.PosY = this._PosY;
            }
            else if (this.PetData != null)
            {
                this.PetData.PosX = this._PosX;
                this.PetData.PosY = this._PosY;
            }

            this.ProcessRole2DCoordinate(new Drawing.Point(_PosX, _PosY));
        }

        private int _PosX = 0;

        /// <summary>
        /// Tọa độ thực X
        /// </summary>
        public int PosX
        {
            get { return _PosX; }
            set
            {
                _PosX = value;
                this.ChangeCoordinateProperty();
            }
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
                this.ChangeCoordinateProperty();
            }
        }

        #endregion

        /// <summary>
        /// Luồng thực thi khinh công
        /// </summary>
        public Coroutine FlyingCoroutine { get; set; }

        #region Kế thừa Interface ISprite

        /// <summary>
        /// Loại đối tượng
        /// </summary>
        public GSpriteTypes SpriteType
        {
            get;
            set;
        }


        private Direction _CurrentDir = Direction.DOWN;
        /// <summary>
        /// Hướng của đối tượng
        /// </summary>
        public Direction Direction
        {
            get
            {
                return this._CurrentDir;
            }
            set
            {
                this._CurrentDir = value;
                this.ChangeDirection(value);
            }
        }

        /// <summary>
        /// Cập nhật thay đổi hướng
        /// </summary>
        /// <param name="dir"></param>
        private void ChangeDirection(Direction dir)
        {
            /// Nếu là người chơi
            if (this.ComponentCharacter != null)
            {
                this.ComponentCharacter.Direction = dir;
            }
            /// Nếu là quái
            else if (this.ComponentMonster != null)
            {
                this.ComponentMonster.Direction = dir;
            }
        }

        #endregion

        #region Thuộc tính đối tượng

        /// <summary>
        /// Thuộc tính đối tượng nếu là người chơi
        /// </summary>
        public RoleData RoleData { get; set; }

        /// <summary>
        /// Thuộc tính đối tượng nếu là quái
        /// </summary>
        public MonsterData MonsterData { get; set; }

        /// <summary>
        /// Thuộc tính đối tượng nếu là NPC
        /// </summary>
        public NPCRole NPCData { get; set; }

        /// <summary>
        /// Thuộc tính đối tượng nếu là điểm thu thập
        /// </summary>
        public GrowPointObject GPData { get; set; }

        /// <summary>
        /// Thuộc tính đối tượng nếu là khu vực động
        /// </summary>
        public DynamicArea DynAreaData { get; set; }

        /// <summary>
        /// Thuộc tính đối tượng nếu là pet
        /// </summary>
        public PetDataMini PetData { get; set; }

        /// <summary>
        /// Thuộc tính đối tượng nếu là xe tiêu
        /// </summary>
        public TraderCarriageData TraderCarriageData { get; set; }

        /// <summary>
        /// Thuộc tính đối tượng nếu là Bot bán hàng
        /// </summary>
        public StallBotData StallBotData { get; set; }

        /// <summary>
        /// ID đối tượng
        /// </summary>
        public int RoleID { get; set; }

        /// <summary>
        /// Tên
        /// </summary>
        public string RoleName
        {
            get
            {
                if (this.RoleData != null)
                {
                    return this.RoleData.RoleName;
                }
                else if (this.MonsterData != null)
                {
                    return this.MonsterData.RoleName;
                }
                else if (this.NPCData != null)
                {
                    return this.NPCData.Name;
                }
                else if (this.GPData != null)
                {
                    return this.GPData.Name;
                }
                else if (this.DynAreaData != null)
                {
                    return this.DynAreaData.Name;
                }
                else if (this.PetData != null)
                {
                    return this.PetData.Name;
                }
                else if (this.TraderCarriageData != null)
                {
                    return this.TraderCarriageData.Name;
                }
                else
                {
                    return "";
                }
            }
        }

        /// <summary>
        /// Danh hiệu
        /// </summary>
        public string Title
        {
            get
            {
                if (this.RoleData != null)
                {
                    return this.RoleData.Title;
                }
                else if (this.MonsterData != null)
                {
                    return this.MonsterData.Title;
                }
                else if (this.NPCData != null)
                {
                    return this.NPCData.Title;
                }
                else if (this.PetData != null)
                {
                    return this.PetData.Title;
                }
                else if (this.TraderCarriageData != null)
                {
                    return this.TraderCarriageData.Title;
                }
                else
                {
                    return "";
                }
            }
        }

        /// <summary>
        /// Danh hiệu
        /// </summary>
        public string GuildTitle
        {
            get
            {
                if (this.RoleData != null)
                {
                    return this.RoleData.GuildTitle;
                }
                else
                {
                    return "";
                }
            }
        }

        /// <summary>
        /// ID danh hiệu đặc biệt
        /// </summary>
        public int SpecialTitleID
        {
            get
            {
                if (this.RoleData != null)
                {
                    return this.RoleData.SpecialTitleID;
                }
                else
                {
                    return -1;
                }
            }
        }

        /// <summary>
        /// Sinh lực
        /// </summary>
        public int HP
        {
            get
            {
                if (this.RoleData != null)
                {
                    return this.RoleData.CurrentHP;
                }
                else if (this.MonsterData != null)
                {
                    return this.MonsterData.HP;
                }
                else if (this.PetData != null)
                {
                    return this.PetData.HP;
                }
                else if (this.TraderCarriageData != null)
                {
                    return this.TraderCarriageData.HP;
                }
                else
                {
                    return 0;
                }
            }
        }

        /// <summary>
        /// Sinh lực thượng hạn
        /// </summary>
        public int HPMax
        {
            get
            {
                if (this.RoleData != null)
                {
                    return this.RoleData.MaxHP;
                }
                else if (this.MonsterData != null)
                {
                    return this.MonsterData.MaxHP;
                }
                else if (this.PetData != null)
                {
                    return this.PetData.MaxHP;
                }
                else if (this.TraderCarriageData != null)
                {
                    return this.TraderCarriageData.MaxHP;
                }
                else
                {
                    return 0;
                }
            }
        }

        /// <summary>
        /// Nội lực
        /// </summary>
        public int MP
        {
            get
            {
                if (this.RoleData != null)
                {
                    return this.RoleData.CurrentMP;
                }
                else
                {
                    return 0;
                }
            }
        }

        /// <summary>
        /// Nội lực thượng hạn
        /// </summary>
        public int MPMax
        {
            get
            {
                if (this.RoleData != null)
                {
                    return this.RoleData.MaxMP;
                }
                else
                {
                    return 0;
                }
            }
        }

        /// <summary>
        /// Thể lực
        /// </summary>
        public int Stamina
        {
            get
            {
                if (this.RoleData != null)
                {
                    return this.RoleData.CurrentStamina;
                }
                else
                {
                    return 0;
                }
            }
        }

        /// <summary>
        /// Thể lực thượng hạn
        /// </summary>
        public int StaminaMax
        {
            get
            {
                if (this.RoleData != null)
                {
                    return this.RoleData.MaxStamina;
                }
                else
                {
                    return 0;
                }
            }
        }

        /// <summary>
        /// Cờ PK
        /// </summary>
        public int Camp
        {
            get
            {
                if (this.RoleData != null)
                {
                    return this.RoleData.Camp;
                }
                else if (this.MonsterData != null)
                {
                    return this.MonsterData.Camp;
                }
                else
                {
                    return -1;
                }
            }
        }

        /// <summary>
        /// Ngũ hành
        /// </summary>
        public Elemental Elemental
        {
            get
            {
                if (this.RoleData != null)
                {
                    return KTGlobal.GetFactionElement(this.RoleData.FactionID);
                }
                else if (this.MonsterData != null)
                {
                    return (Elemental) this.MonsterData.Elemental;
                }
                else
                {
                    return Elemental.NONE;
                }
            }
        }

        /// <summary>
        /// Tốc độ di chuyển của đối tượng
        /// </summary>
        public int MoveSpeed
        {
            get
            {
                if (this.RoleData != null)
                {
                    return this.RoleData.MoveSpeed;
                }
                else if (this.MonsterData != null)
                {
                    return this.MonsterData.MoveSpeed;
                }
                else if (this.PetData != null)
                {
                    return this.PetData.MoveSpeed;
                }
                else if (this.TraderCarriageData != null)
                {
                    return this.TraderCarriageData.MoveSpeed;
                }
                else
                {
                    return 0;
                }
            }
        }

        /// <summary>
        /// Tốc độ xuất chiêu hệ ngoại công
        /// </summary>
        public int AttackSpeed
        {
            get
            {
                if (this.RoleData != null)
                {
                    return this.RoleData.AttackSpeed;
                }
                else if (this.MonsterData != null)
                {
                    return this.MonsterData.AttackSpeed;
                }
                else if (this.PetData != null)
                {
                    return this.PetData.AtkSpeed;
                }
                else
                {
                    return 0;
                }
            }
        }

        /// <summary>
        /// Tốc độ xuất chiêu hệ nội công
        /// </summary>
        public int CastSpeed
        {
            get
            {
                if (this.RoleData != null)
                {
                    return this.RoleData.CastSpeed;
                }
                else if (this.MonsterData != null)
                {
                    return this.MonsterData.AttackSpeed;
                }
                else if (this.PetData != null)
                {
                    return this.PetData.CastSpeed;
                }
                else
                {
                    return 0;
                }
            }
        }

        #endregion

        #region Chiến đấu

        /// <summary>
        /// Đối tượng có đang di chuyển không
        /// </summary>
        public bool IsMoving { get; private set; } = false;

        /// <summary>
        /// Đối tượng đã chết
        /// </summary>
        public bool IsDeath { get; private set; }

        /// <summary>
        /// Thời gian Delay chờ xóa khỏi hệ thống
        /// </summary>
        private int deathDelay = 0;

        /// <summary>
        /// Danh sách trạng thái ngũ hành
        /// </summary>
        private readonly Dictionary<KE_STATE, SpriteSeriesState> seriesStates = new Dictionary<KE_STATE, SpriteSeriesState>();

        /// <summary>
        /// Thêm trạng thái ngũ hành
        /// </summary>
        /// <param name="state"></param>
        /// <param name="time"></param>
        public void AddSeriesState(KE_STATE state, float time)
        {
            this.seriesStates[state] = new SpriteSeriesState()
            {
                State = state,
                Duration = time,
                LifeTime = 0f,
            };
            if (!this.CanMove)
            {
                /// Nếu là Leader
                if (this == Global.Data.Leader)
                {
                    KTLeaderMovingManager.StopMoveImmediately(true, false);
                }
                else
                {
                    this.StopMove();
                }
            }
        }

        /// <summary>
        /// Xóa trạng thái ngũ hành
        /// </summary>
        /// <param name="state"></param>
        public void RemoveSeriesState(KE_STATE state)
        {
            if (this.seriesStates.TryGetValue(state, out _))
            {
                this.seriesStates.Remove(state);
            }
        }

        /// <summary>
        /// Làm mới hiển thị trạng thái ngũ hành trên đối tượng
        /// </summary>
        private void RefreshSeriesStateDisplay()
        {
            /// Nếu đã chết thì bỏ qua
            if (this.IsDeath)
            {
                return;
            }

            /// Thiết lập màu đối tượng về mặc định ban đầu
            if (this.ComponentCharacter != null)
            {
                this.ComponentCharacter.MixColor(Color.white);
            }
            else if (this.ComponentMonster != null)
            {
                this.ComponentMonster.MixColor(Color.white);
            }

            /// Nếu đối tượng đã chết thì không làm gì cả
            if (this.IsDeath)
            {
                return;
            }

            /// Danh sách cần xóa
            List<KE_STATE> removeList = null;

            /// Biến đánh dấu còn tồn tại trạng thái bị đơ người không
            bool isIncapacitated = false;

            /// Duyệt toàn bộ danh sách hiệu ứng có
            foreach (SpriteSeriesState state in seriesStates.Values)
            {
                /// Cộng thời gian tồn tại
                state.LifeTime += Time.deltaTime;

                /// Nếu còn tồn tại
                if (state.IsAlive)
                {
                    Color color = Color.white;
                    switch (state.State)
                    {
                        case KE_STATE.emSTATE_POISON:
                            ColorUtility.TryParseHtmlString("#149900", out color);
                            break;
                        case KE_STATE.emSTATE_SLOWALL:
                            ColorUtility.TryParseHtmlString("#52bfff", out color);
                            break;
                        case KE_STATE.emSTATE_SLOWRUN:
                            ColorUtility.TryParseHtmlString("#52bfff", out color);
                            break;
                        case KE_STATE.emSTATE_BURN:
                            ColorUtility.TryParseHtmlString("#ffa229", out color);
                            break;
                    }

                    /// Nếu thuộc một trong những trạng thái bị đổi màu
                    if (color != Color.white)
                    {
                        if (this.ComponentCharacter != null)
                        {
                            this.ComponentCharacter.MixColor(color);
                        }
                        else if (this.ComponentMonster != null)
                        {
                            this.ComponentMonster.MixColor(color);
                        }
                    }

                    /// Thực hiện trạng thái đặc biệt
                    switch (state.State)
                    {
                        case KE_STATE.emSTATE_HURT:
                            if (this.CurrentAction != KE_NPC_DOING.do_hurt)
                            {
                                this.DoHurt(state.Duration);
                            }
                            break;
                        case KE_STATE.emSTATE_STUN:
                            this.PauseCurrentAction();
                            isIncapacitated = true;
                            break;
                    }
                }
                /// Nếu đã hết thời gian trạng thái
                else
                {
                    /// Nếu danh sách cần xóa đang rỗng
                    if (removeList == null)
                    {
                        removeList = new List<KE_STATE>();
                    }
                    removeList.Add(state.State);
                }
            }

            /// Duyệt toàn bộ danh sách cần xóa
            if (removeList != null)
            {
                foreach (KE_STATE stateID in removeList)
                {
                    /// Xóa trạng thái khỏi danh sách
                    this.seriesStates.Remove(stateID);
                }
            }

            /// Nếu không còn tồn tại trạng thái đơ người thì tiếp tục thực hiện động tác hiện tại
            if (!isIncapacitated && this.lastIsIncapacitated)
            {
                this.ResumeCurrentAction();
            }
            this.lastIsIncapacitated = isIncapacitated;
        }

        /// <summary>
        /// Đánh dấu lần trước có bị khóa không
        /// </summary>
        private bool lastIsIncapacitated = false;

        /// <summary>
        /// Có thể thực hiện các thao tác Logic như sử dụng kỹ năng không
        /// </summary>
        public bool CanDoLogic
        {
            get
            {
                /// Nếu đã chết thì không thể thực hiện thao tác logic
                if (this.IsDeath || this.HP <= 0)
                {
                    return false;
                }

                foreach (SpriteSeriesState state in this.seriesStates.Values)
                {
                    if (state.IsAlive)
                    {
                        if (state.State == KE_STATE.emSTATE_HURT || state.State == KE_STATE.emSTATE_STUN || state.State == KE_STATE.emSTATE_FREEZE || state.State == KE_STATE.emSTATE_PALSY || state.State == KE_STATE.emSTATE_CONFUSE || state.State == KE_STATE.emSTATE_DRAG || state.State == KE_STATE.emSTATE_KNOCK || state.State == KE_STATE.emSTATE_FLOAT)
                        {
                            return false;
                        }
                    }
                }

                return true;
            }
        }

        /// <summary>
        /// Có thể di chuyển không
        /// </summary>
        public bool CanMove
        {
            get
            {
                /// Nếu đã chết thì không thể thực hiện thao tác logic
                if (this.IsDeath || this.HP <= 0)
                {
                    return false;
                }

                /// Nếu là người chơi
                if (this.ComponentCharacter != null)
                {
                    /// Nếu là bản thân
                    if (this.RoleID == Global.Data.RoleData.RoleID)
                    {
                        /// Nếu có trạng thái bán hàng thì không thể di chuyển được
                        if (Global.Data.StallDataItem != null && Global.Data.StallDataItem.Start == 1 && !Global.Data.StallDataItem.IsBot)
                        {
                            return false;
                        }
                    }
                    /// Nếu là người chơi khác
                    else if (Global.Data.OtherRoles.TryGetValue(this.RoleID, out RoleData rd))
                    {
                        /// Nếu có trạng thái bán hàng thì không thể di chuyển được
                        if (!string.IsNullOrEmpty(rd.StallName))
                        {
                            return false;
                        }
                    }
                }

                foreach (SpriteSeriesState state in this.seriesStates.Values)
                {
                    if (state.IsAlive)
                    {
                        if (state.State == KE_STATE.emSTATE_HURT || state.State == KE_STATE.emSTATE_STUN || state.State == KE_STATE.emSTATE_FREEZE || state.State == KE_STATE.emSTATE_PALSY || state.State == KE_STATE.emSTATE_DRAG || state.State == KE_STATE.emSTATE_KNOCK || state.State == KE_STATE.emSTATE_FIXED || state.State == KE_STATE.emSTATE_FLOAT)
                        {
                            return false;
                        }
                    }
                }

                return true;
            }
        }

        /// <summary>
        /// Có thể chủ động điều khiển đối tượng di chuyển không
        /// </summary>
        public bool CanPositiveMove
        {
            get
            {
                /// Nếu đã chết thì không thể thực hiện thao tác logic
                if (this.IsDeath || this.HP <= 0)
                {
                    return false;
                }

                /// Nếu là người chơi
                if (this.ComponentCharacter != null)
                {
                    /// Nếu là bản thân
                    if (this.RoleID == Global.Data.RoleData.RoleID)
                    {
                        /// Nếu có trạng thái bán hàng thì không thể di chuyển được
                        if (Global.Data.StallDataItem != null && Global.Data.StallDataItem.Start == 1 && !Global.Data.StallDataItem.IsBot)
                        {
                            return false;
                        }
                    }
                    /// Nếu là người chơi khác
                    else if (Global.Data.OtherRoles.TryGetValue(this.RoleID, out RoleData rd))
                    {
                        /// Nếu có trạng thái bán hàng thì không thể di chuyển được
                        if (!string.IsNullOrEmpty(rd.StallName))
                        {
                            return false;
                        }
                    }
                }

                foreach (SpriteSeriesState state in this.seriesStates.Values)
                {
                    if (state.IsAlive)
                    {
                        if (state.State == KE_STATE.emSTATE_HURT || state.State == KE_STATE.emSTATE_STUN || state.State == KE_STATE.emSTATE_FREEZE || state.State == KE_STATE.emSTATE_PALSY || state.State == KE_STATE.emSTATE_CONFUSE || state.State == KE_STATE.emSTATE_DRAG || state.State == KE_STATE.emSTATE_KNOCK || state.State == KE_STATE.emSTATE_FIXED || state.State == KE_STATE.emSTATE_FLOAT)
                        {
                            return false;
                        }
                    }
                }

                return true;
            }
        }

        /// <summary>
        /// Sẵn sàng để di chuyển chưa
        /// </summary>
        public bool IsReadyToMove
        {
            get
            {
                if (!this.CanPositiveMove)
                {
                    return false;
                }

                ///// Nếu có luồng khinh công hoặc tốc biến
                //if (this.FlyingCoroutine != null)
                //{
                //    return false;
                //}

                /// Nếu động tác hiện tại là chạy hoặc đi bộ
                if (this.doingAction != null && (this.doingAction.Action == KE_NPC_DOING.do_run || this.doingAction.Action == KE_NPC_DOING.do_walk))
                {
                    return true;
                }

                /// Xem động tác hiện tại đã hoàn tất chưa
                return this.IsCurrentActionCompleted();
            }
        }

        #endregion

        #region IObject - Hiển thị

        /// <summary>
        /// Đã khởi tạo chưa
        /// </summary>
        private bool _Started = false;

        /// <summary>
        /// Thực hiện khởi tạo đối tượng
        /// </summary>
        public void Start()
        {
            if (this._Started)
            {
                return;
            }

            this._Started = true;
            this.InitStatus = true;

            /// Thêm đối tượng vào danh sách quản lý
            KTObjectsManager.Instance.AddObject(this);
        }


        /// <summary>
        /// Xóa đối tượng
        /// </summary>
        public void Destroy()
        {
            //KTDebug.LogError(new System.Diagnostics.StackTrace().ToString());
            //KTDebug.LogError("Destroy => " + this.RoleName);

            /// Ngừng di chuyển
            this.StopMove();

            /// Ngừng toàn bộ luồng đang thực thi
            foreach (SpriteTimer spriteTimer in this.SpriteTimers.Values)
            {
                KTTimerManager.Instance.StopCoroutine(spriteTimer.Coroutine);
            }
            this.SpriteTimers.Clear();

            /// Dọn rác sinh ra
            this.QueueActions.Clear();
            this.Buffs.Clear();
            this.othersAction.Clear();
            this.queueFlyingText.Clear();

            /// Xóa đối tượng khỏi Render
            KTObjectsManager.Instance.RemoveObject(this);

            /// Nếu đối tượng đang được trỏ bởi người chơi
            if (Global.Data.GameScene.IsSpriteSelected(this))
            {
                Global.Data.GameScene.RemoveSelectTarget();
            }

            if (null != this.Role2D)
            {
                if (GSpriteTypes.Leader == this.SpriteType || GSpriteTypes.Other == this.SpriteType || GSpriteTypes.Bot == this.SpriteType || GSpriteTypes.StallBot == this.SpriteType)
                {
                    this.ComponentCharacter.Destroy();
                }
                else if (GSpriteTypes.NPC == this.SpriteType || GSpriteTypes.Monster == this.SpriteType || GSpriteTypes.GrowPoint == this.SpriteType || GSpriteTypes.DynamicArea == this.SpriteType || GSpriteTypes.Pet == this.SpriteType || GSpriteTypes.TraderCarriage == this.SpriteType)
                {
                    this.ComponentMonster.Destroy();
                }
                else
                {
                    GameObject.Destroy(this.Role2D);
                }

                this.Role2D = null;
            }
        }

        #endregion

        #region IObject FrameRender
        /// <summary>
        /// Hàm này gọi liên tục chừng nào đối tượng còn sống
        /// </summary>
        public void OnFrameRender()
        {
            /// Thực hiện biểu diễn chữ bay trên đầu
            this.ProcessHeadText();

            /// Nếu bản thân dưới 0 máu
            if (this.SpriteType == GSpriteTypes.Other || this.SpriteType == GSpriteTypes.Leader || this.SpriteType == GSpriteTypes.Monster || this.SpriteType == GSpriteTypes.Pet || this.SpriteType == GSpriteTypes.TraderCarriage || this.SpriteType == GSpriteTypes.Monster)
            {
                /// Nếu máu dưới 0
                if (this.HP <= 0)
                {
                    /// Nếu chưa thực hiện động tác chết thì thưc hiện
                    if (!this.IsDeath)
					{
                        /// Thực hiện động tác chết
                        this.DoDeath();
                    }
                    /// Nếu đã thực hiện động tác chết và là pet
                    else if (this.SpriteType == GSpriteTypes.Pet)
                    {
                        this.PetData.HP = 0;
                        /// Cập nhật lên thanh máu chống bug
                        this.ComponentMonster.UpdateHP();
                        /// Nếu đang thực thi động tác chết
                        this.ProcessDead();
                    }
                    /// Nếu đã thực hiện động tác chết và là xe tiêu
                    else if (this.SpriteType == GSpriteTypes.TraderCarriage)
                    {
                        this.TraderCarriageData.HP = 0;
                        /// Cập nhật lên thanh máu chống bug
                        this.ComponentMonster.UpdateHP();
                        /// Nếu đang thực thi động tác chết
                        this.ProcessDead();
                    }
                    /// Nếu đã thực hiện động tác chết và là quái
                    else if (this.SpriteType == GSpriteTypes.Monster)
                    {
                        this.MonsterData.HP = 0;
                        /// Cập nhật lên thanh máu chống bug
                        this.ComponentMonster.UpdateHP();
                        /// Nếu đang thực thi động tác chết
                        this.ProcessDead();
                    }


                    /// Nếu là Leader
                    if (Global.Data.RoleData.RoleID == this.RoleID)
                    {
                        /// Nếu chưa mở bảng thông báo chết
                        if (PlayZone.Instance.UIReviveFrame == null)
                        {
                            if (Loader.Maps.TryGetValue(Global.Data.RoleData.MapCode, out Map gameMap))
                            {
                                Vector2 gridPos = KTGlobal.WorldPositionToGridPosition(this.PositionInVector2);
                                string message = string.Format("Địa điểm trọng thương: <color=green>{0}</color>, vị trí <color=#42efff>({1}, {2})</color>.\nĐối tượng đả thương: <color=yellow>Chưa rõ</color>.", gameMap.Name, (int) gridPos.x, (int) gridPos.y);
                                /// Mở bảng thông báo chết
                                PlayZone.Instance.ShowReviveFrame(message, false);
                            }
                        }
                    }
                    return;
                }
                /// Nếu máu > 0
                else
                {
                    /// Nếu là Leader
                    if (Global.Data.RoleData.RoleID == this.RoleID)
                    {
                        /// Nếu đang hiện bảng thông báo chết
                        if (PlayZone.Instance.UIReviveFrame != null)
                        {
                            /// Thực hiện động tác đứng
                            this.DoRevive();
                            /// Ẩn bảng thông báo chết
                            PlayZone.Instance.CloseReviveFrame();
                        }
                    }
                }
            }

            /// Nếu đang thực thi động tác chết
            if (this.ProcessDead())
            {
                return;
            }

            /// Thực hiện hiển thị các trạng thái ngũ hành nếu có
            this.RefreshSeriesStateDisplay();

            /// Thực hiện động tác trong danh sách chờ
            this.ProcessAction();

            /// Thực hiện hành động từ Server lệnh về
            this.TickOthersAction();

            /// Thực hiện Tick BUff
            this.TickBuffs();
        }

        #endregion

        /// <summary>
        /// Động tác hiện tại
        /// </summary>
        public KE_NPC_DOING CurrentAction { get; private set; }

        #region Tử vong
        /// <summary>
        /// Thực hiện đếm lùi để xóa đối tượng khi chết
        /// </summary>
        private bool ProcessDead()
        {
            /// Nếu đã được gắn cờ chết
            if (this.IsDeath)
            {
                /// Thời gian tồn tại trước khi bị xóa
                int maxTicks;
                int elapsedTicks = Global.GetMyTimer() - deathDelay;

                /// Nếu không phải người chơi thì thực hiện tự xóa sau khi đếm lùi
                if (GSpriteTypes.Leader != this.SpriteType && GSpriteTypes.Other != this.SpriteType && GSpriteTypes.Bot != this.SpriteType)
                {
                    maxTicks = 3000;
                    if (elapsedTicks >= maxTicks)
                    {
                        //KTDebug.LogError("HERE: ProcessDead -> elapsedTicks >= maxTicks");
                        KTGlobal.RemoveObject(this, true);
                    }
                }
                /// Nếu là người chơi
                else
                {
                    maxTicks = 1000;
                    if (this.deathDelay != 0)
                    {
                        if (elapsedTicks >= maxTicks)
                        {
                            this.deathDelay = 0;
                        }
                    }
                    else
                    {
                        if ((null != Global.Data.RoleData && Global.Data.RoleData.CurrentHP <= 0))
                        {
                            this.deathDelay = Global.GetMyTimer() + 1000;
                        }
                    }
                }

                return true;
            }
            /// Nếu đối tượng là Leader và trạng thái đối tượng là tử vong nhưng chưa được gắn cờ chết
            else if (SpriteType == GSpriteTypes.Leader && this.CurrentAction != KE_NPC_DOING.do_death)
            {
                if (Global.Data.RoleData != null && Global.Data.RoleData.CurrentHP <= 0)
                {
                    this.IsDeath = true;
                    this.deathDelay = Global.GetMyTimer();
                }
            }

            return false;
        }

        #endregion

        #region Động tác tạm thời
        /// <summary>
        /// Tạm thời đổi hướng quay của đối tượng
        /// </summary>
        /// <param name="dir"></param>
        /// <param name="duration"></param>
        public void TempChangeDir(Direction dir, float duration)
        {
            /// Đối hướng thành hướng tạm thời
            this.ChangeDirection(dir);

            /// Thực hiện đếm lùi khôi phục về hướng quay cũ
            this.SetTimer("SpriteTempChangeDir", duration, () => {
                /// Đổi về hướng cũ
                this.ChangeDirection(this._CurrentDir);
            });
        }

        /// <summary>
        /// Tạm thời chuyển hướng theo vị trí của mục tiêu
        /// </summary>
        /// <param name="target"></param>
        /// <param name="duration"></param>
        public void TempChangeDirFollowTarget(GSprite target, float duration)
        {
            /// Thực hiện đếm lùi khôi phục về hướng quay cũ
            this.SetTimer("SpriteTempChangeDir", duration, 0.2f, () => {
                /// Hướng tạm thời
                Vector2 dirVector = target.PositionInVector2 - this.PositionInVector2;
                float angle = KTMath.GetAngle360WithXAxis(dirVector);
                Direction dir = KTMath.GetDirectionByAngle360(angle);

                /// Đối hướng thành hướng tạm thời
                this.ChangeDirection(dir);
            }, () => {
                /// Đổi về hướng cũ
                this.ChangeDirection(this._CurrentDir);
            });
        }
        #endregion

        /// <summary>
        /// Chuyển đối tượng về dạng String
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("ID = {0}, Name = {1}", this.RoleID, this.RoleName);
        }
    }
}
