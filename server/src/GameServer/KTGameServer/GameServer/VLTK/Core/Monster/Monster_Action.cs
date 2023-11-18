using GameServer.KiemThe;
using GameServer.KiemThe.Entities;
using GameServer.KiemThe.Logic;
using GameServer.KiemThe.Logic.Manager.Skill.PoisonTimer;
using Server.Data;
using System.Windows;
using static GameServer.Logic.KTMapManager;

namespace GameServer.Logic
{
    /// <summary>
    /// Sự kiện
    /// </summary>
    public partial class Monster
    {
        /// <summary>
        /// Hàm này gọi đến khi đối tượng được tạo ra
        /// </summary>
        public void Awake()
        {
            /// Nếu là loại quái đặc biệt hoặc NPC di động
            if (this.MonsterType == MonsterAIType.DynamicNPC || this.MonsterType == MonsterAIType.Special_Boss || this.MonsterType == MonsterAIType.Special_Normal)
            {
                /// Thực thi Timer của quái
                KTMonsterTimerManager.Instance.Add(this);
            }
        }

        /// <summary>
        /// Hàm này gọi mỗi khi Timer quái được thêm vào
        /// </summary>
        public void Start()
        {
            /// Nếu là Boss hoặc hải tặc thì cho miễn dịch trạng thái
            if (this.MonsterType == MonsterAIType.Boss || this.MonsterType == MonsterAIType.Pirate)
            {
                this.m_IgnoreAllSeriesStates = true;
            }
            /// Nếu là quái tĩnh miễn dịch toàn bộ
            else if (this.MonsterType == MonsterAIType.Static_ImmuneAll)
            {
                this.m_IgnoreAllSeriesStates = true;
            }

            /// Nếu là quái tĩnh
            if (this.MonsterType == MonsterAIType.Static || this.MonsterType == MonsterAIType.Static_ImmuneAll)
            {
                this.IsStatic = true;
            }

            /// Danh sách vòng sáng
            if (!string.IsNullOrEmpty(this.MonsterInfo.Auras))
            {
                foreach (string auraSkillString in this.MonsterInfo.Auras.Split(';'))
                {
                    string[] fields = auraSkillString.Split('_');
                    int skillID = int.Parse(fields[0]);
                    int skilllevel = int.Parse(fields[1]);

                    /// Kích hoạt vòng sáng
                    this.UseSkill(skillID, skilllevel, this);
                }
            }

            /// Danh sách kỹ năng
            if (!string.IsNullOrEmpty(this.MonsterInfo.Skills))
            {
                foreach (string skillString in this.MonsterInfo.Skills.Split(';'))
                {
                    string[] fields = skillString.Split('_');
                    int skillID = int.Parse(fields[0]);
                    int skilllevel = int.Parse(fields[1]);
                    int cooldown = int.Parse(fields[2]);

                    SkillDataEx skillData = KSkill.GetSkillData(skillID);
                    SkillLevelRef skill = new SkillLevelRef()
                    {
                        Data = skillData,
                        AddedLevel = skilllevel,
                        Exp = cooldown,
                    };
                    this.CustomAISkills.Add(skill);
                }
            }
        }

        /// <summary>
        /// Hủy đối tượng
        /// </summary>
        public void Dispose()
        {
            /// Đánh dấu đã chết
            this.m_CurrentLife = 0;
            this.m_eDoing = KE_NPC_DOING.do_death;

            this.DamageTakeRecord.Clear();
            this.LocalVariables.Clear();
            this.CustomAISkills.Clear();

            /// Xóa toàn bộ Buff và vòng sáng tương ứng
            this.Buffs.RemoveAllBuffs(false, false);
            this.Buffs.RemoveAllAruas(false, false);

            /// Ngừng Storyboard tương ứng
            KTMonsterStoryBoardEx.Instance.Remove(this);

            /// Ngừng Timer tương ứng
            KTMonsterTimerManager.Instance.Remove(this);

            /// Xóa luồng trúng độc
            KTPoisonTimerManager.Instance.RemovePoisonState(this);
        }

        /// <summary>
        /// Làm mới đối tượng
        /// </summary>
        public void ResetAI()
        {
            /// Hủy mục tiêu đang đuổi
            this.chaseTarget = null;
        }

        /// <summary>
        /// Thiết lập lại đối tượng
        /// </summary>
        public void Reset()
        {
            /// Nếu đã chết thì thôi
            if (this.IsDead())
            {
                return;
            }

            /// Ngừng StoryBoard
            KTMonsterStoryBoardEx.Instance.Remove(this);

            /// Hủy mục tiêu đang đuổi
            this.chaseTarget = null;
            /// Hủy vị trí tiếp theo cần đến
            this.NextMoveTo = new Point(-1, -1);
            /// Hủy đánh dấu AI tự di chuyển
            this.IsAIRandomMove = false;
            /// Bỏ vị trí đích đến
            this.ToPos = new Point(-1, -1);
            /// Hủy đánh dấu đang tự chạy về vị trí ban đầu
            this.IsBackingToOriginPos = false;
            /// Bản đồ
            GameMap gameMap = KTMapManager.Find(this.CurrentMapCode);
            /// Nếu vị trí ban đầu lỗi thì thiết lập lại vị trí hiện tại
            if (!gameMap.CanMove(this.StartPos, this.CurrentCopyMapID))
            {
                /// Gắn lại vị trí hiện tại
                this.StartPos = this.CurrentPos;

                /// Cập nhật vị trí đối tượng vào Map
                gameMap.Grid.MoveObject((int) this.CurrentPos.X, (int) this.CurrentPos.Y, this);
            }
            else
            {
                /// Thiết lập vị trí là vị trí ban đầu
                this.CurrentPos = this.StartPos;

                /// Cập nhật vị trí đối tượng vào Map
                gameMap.Grid.MoveObject((int) this.CurrentPos.X, (int) this.CurrentPos.Y, this);
            }
            /// Thiết lập đang đứng
            this.m_eDoing = KE_NPC_DOING.do_stand;

            /// Xóa luồng trúng độc
            KTPoisonTimerManager.Instance.RemovePoisonState(this);
        }

        /// <summary>
        /// Di chuyển quái đến vị trí tương ứng
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="isAIRandomMove"></param>
        public void MoveTo(Point pos, bool isAIRandomMove = false)
        {
            /// Nếu quái tĩnh thì thôi
            if (this.IsStatic || this.GetCurrentRunSpeed() <= 0)
            {
                return;
            }
            /// Nếu đang quay trở lại vị trí ban đầu
            else if (this.IsBackingToOriginPos)
            {
                return;
            }

            this.NextMoveTo = pos;
            this.IsAIRandomMove = isAIRandomMove;
        }

        /// <summary>
        /// Di chuyển ngẫu nhiên đến vị trí chỉ định
        /// </summary>
        public void RandomMoveAround()
        {
            /// Nếu quái tĩnh thì thôi
            if (this.IsStatic || this.GetCurrentRunSpeed() <= 0)
            {
                return;
            }
            /// Nếu phạm vi tìm kiếm quá nhỏ
            else if (this.SeekRange <= 0)
            {
                return;
            }
            /// Nếu quái đã chết
            else if (this.IsDead())
            {
                return;
            }
            /// Nếu đang đuổi theo mục tiêu hoặc đang trở về vị trí ban đầu
            else if (this.chaseTarget != null || this.IsBackingToOriginPos)
            {
                return;
            }

            /// Bản đồ hiện tại
            GameMap gameMap = KTMapManager.Find(this.MapCode);
            /// Toác
            if (gameMap == null)
            {
                return;
            }

            /// Lấy giá trị ngẫu nhiên để thực hiện di chuyển
            int nRand = KTGlobal.GetRandomNumber(1, 100);
            /// Nếu có thể di chuyển
            if (nRand <= Monster.AIRandomMoveBelowRate)
            {
                /// Vị trí ngẫu nhiên xung quanh không có vật cản
                Point randomPos = KTGlobal.GetRandomAroundNoObsPoint(gameMap, this.CurrentPos, this.SeekRange, this.CurrentCopyMapID, false);
                /// Nếu vị trí này không có vật cản
                if (!KTGlobal.InObs(this.MapCode, (int) randomPos.X, (int) randomPos.Y, this.CurrentCopyMapID))
                {
                    this.MoveTo(randomPos, true);
                }
            }
        }

        /// <summary>
        /// Sử dụng kỹ năng
        /// </summary>
        /// <param name="skillID"></param>
        /// <param name="level"></param>
        /// <param name="target"></param>
        public bool UseSkill(int skillID, int level, GameObject target)
        {
            /// Nếu đã chết
            if (this.IsDead() || target.IsDead())
            {
                return false;
            }

            /// Nếu tiến vào khu an toàn
            if (KTMapManager.Find(this.CurrentMapCode).MyNodeGrid.InSafeArea((int) this.CurrentGrid.X, (int) this.CurrentGrid.Y))
            {
                return false;
            }

            /// Nếu chưa đến thời gian dùng kỹ năng
            if (!KTGlobal.FinishedUseSkillAction(this, this.GetCurrentAttackSpeed()))
            {
                return false;
            }

            /// Lấy dữ liệu kỹ năng tương ứng
            SkillDataEx skillData = KSkill.GetSkillData(skillID);
            /// Nếu kỹ năng không tồn tại
            if (skillData == null)
            {
                return false;
            }

            /// Làm mới đối tượng kỹ năng theo cấp
            SkillLevelRef skill = new SkillLevelRef()
            {
                Data = skillData,
                AddedLevel = level,
                BonusLevel = 0,
            };

            /// Thực hiện sử dụng kỹ năng
            KTSkillManager.UseSkillResult result = KTSkillManager.UseSkill(this, target, null, skill);
            return true;
        }

        #region Data
        /// <summary>
        /// Trả về dữ liệu quái
        /// </summary>
        /// <returns></returns>
        public MonsterData GetMonsterData()
        {
            MonsterData md = new MonsterData();

            md.RoleID = this.RoleID;
            md.RoleName = this.RoleName;
            md.ExtensionID = this.MonsterInfo.Code;
            md.Level = this.m_Level;
            md.MaxHP = this.m_CurrentLifeMax;
            md.Camp = this.Camp;
            md.PosX = (int) this.CurrentPos.X;
            md.PosY = (int) this.CurrentPos.Y;
            md.Direction = (int) this.CurrentDir;
            md.HP = this.m_CurrentLife;
            md.Elemental = (int) this.m_Series;
            md.MoveSpeed = this.GetCurrentRunSpeed();
            md.Title = this.Title;
            md.AttackSpeed = this.GetCurrentAttackSpeed();
            md.MonsterType = (int) this.MonsterType;


            /// Danh sách Buff
            md.ListBuffs = this.Buffs.ToBufferData();

            return md;
        }
        #endregion
    }
}
