using GameServer.KiemThe;
using GameServer.KiemThe.Entities;
using GameServer.VLTK.Entities.Pet;
using Server.Data;
using System.Windows;
using static GameServer.Logic.KTMapManager;

namespace GameServer.Logic
{
    /// <summary>
    /// Đối tượng Pet
    /// </summary>
    public partial class Pet : GameObject
    {
        /// <summary>
        /// Chủ nhân
        /// </summary>
        public KPlayer Owner { get; private set; }

        /// <summary>
        /// Quản lý kỹ năng Pet
        /// </summary>
        public PetSkillTree SkillTree { get; private set; }

        /// <summary>
        /// Thời điểm tham chiến
        /// </summary>
        public long CreateTicks { get; private set; }

        /// <summary>
        /// Thời điểm tự động giảm độ vui vẻ trước
        /// </summary>
        public long LastCostJoyTicks { get; set; }

        /// <summary>
        /// Đối tượng Pet
        /// </summary>
        /// <param name="player"></param>
        /// <param name="petData"></param>
        public Pet(KPlayer player, PetData petData)
        {
            /// Chủ nhân
            this.Owner = player;

            /// Các thông số
            this.RoleID = petData.ID + (int) ObjectBaseID.Pet;
            this.ResID = petData.ResID;
            this.RoleName = petData.Name;
            this.m_Level = petData.Level;
            this.m_Experience = petData.Exp;
            this.Enlightenment = petData.Enlightenment;
            this.Skills = petData.Skills;
            this.Equips = petData.Equips;
            this.Joyful = petData.Joyful;
            this.Life = petData.Life;
            this.BaseRemainPoints = petData.RemainPoints;

            /// Thông tin Pet
            PetDataXML data = KPet.GetPetData(petData.ResID);
            /// Nếu có dữ liệu
            if (data != null)
            {
                /// Sức, thân, ngoại, nội
                this.ChangeCurStrength(data.StrPerLevel * petData.Level + data.Str);
                this.ChangeStrength(petData.Str);
                this.ChangeCurDexterity(data.DexPerLevel * petData.Level + data.Dex);
                this.ChangeDexterity(petData.Dex);
                this.ChangeCurVitality(data.StaPerLevel * petData.Level + data.Sta);
                this.ChangeVitality(petData.Sta);
                this.ChangeCurEnergy(data.IntPerLevel * petData.Level + data.Int);
                this.ChangeEnergy(petData.Int);
                /// Sinh lực tối đa
                this.ChangeLifeMax(data.HP, 0, 0);
                this.m_CurrentLife = petData.HP <= 0 ? this.m_CurrentLifeMax : petData.HP;
                /// Tốc chạy
                this.ChangeRunSpeed(player.GetCurrentRunSpeed(), 0, 0);
            }

            /// Xây cây kỹ năng
            this.SkillTree = new PetSkillTree(this);
            /// Tạo mới Buff Tree
            this.Buffs = new BuffTree(this);
            /// Thực hiện Logic các kỹ năng bị động
            this.SkillTree.ProcessPassiveSkills();

            /// Đánh dấu thời điểm tạo
            this.CreateTicks = KTGlobal.GetCurrentTimeMilis();
            /// Reset thời điểm giảm độ vui vẻ lần trước
            this.LastCostJoyTicks = this.CreateTicks;
        }

        #region Kế thừa IObject

        /// <summary>
        /// Loại đối tượng
        /// </summary>
        public override ObjectTypes ObjectType
        {
            get { return ObjectTypes.OT_PET; }
        }

        /// <summary>
        /// Vị trí hiện tại (tọa độ lưới)
        /// </summary>
        public override Point CurrentGrid { get; set; }

        /// <summary>
        /// Vị trí hiện tại
        /// </summary>
        private Point _CurrentPos = new Point(0, 0);

        /// <summary>
        /// Vị trí hiện tại
        /// </summary>
        public override Point CurrentPos
        {
            get
            {
                return _CurrentPos;
            }

            set
            {
                GameMap gameMap = KTMapManager.Find(this.CurrentMapCode);
                this.CurrentGrid = new Point((int) (value.X / gameMap.MapGridWidth), (int) (value.Y / gameMap.MapGridHeight));
                this._CurrentPos = value;
            }
        }

        /// <summary>
        /// ID map hiện tại
        /// </summary>
        public override int CurrentMapCode
        {
            get
            {
                /// Toác
                if (this.Owner == null)
                {
                    return -1;
                }
                return this.Owner.CurrentMapCode;
            }
        }

        /// <summary>
        /// ID phụ bản hiện tại
        /// </summary>
        public override int CurrentCopyMapID
        {
            get
            {
                /// Toác
                if (this.Owner == null)
                {
                    return -1;
                }
                return this.Owner.CurrentCopyMapID;
            }
        }

        /// <summary>
        /// Hướng hiện tại
        /// </summary>
        public override KiemThe.Entities.Direction CurrentDir
        {
            get;
            set;
        }

        #endregion
    }
}
