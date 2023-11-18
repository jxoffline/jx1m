using GameServer.KiemThe;
using GameServer.KiemThe.Entities;
using GameServer.KiemThe.Entities.Sprite;
using GameServer.KiemThe.Logic;
using GameServer.KiemThe.Logic.Manager.Skill.PoisonTimer;
using GameServer.KiemThe.LuaSystem;
using GameServer.KiemThe.Network.Entities;
using Server.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using static GameServer.KiemThe.Logic.KTSkillManager;
using static GameServer.Logic.KTMapManager;

namespace GameServer.Logic
{
    /// <summary>
    /// Quản lý sự kiện
    /// </summary>
    public partial class GameObject
    {
        #region Core
        /// <summary>
        /// Thời gian tồn tại của đối tượng
        /// </summary>
        public long LifeTime { get; private set; } = 0;

        /// <summary>
        /// Khóa hồi máu
        /// </summary>
        private readonly object hpRegenerateLock = new object();

        /// <summary>
        /// Sự kiện chạy liên tục mỗi khoảng 0.5s
        /// </summary>
        public virtual void Tick()
        {
            /// Nếu là người
            if (this is KPlayer)
			{
                this.LifeTime += 500;
            }
            /// Nếu là quái
			else
			{
                this.LifeTime += 500;
			}

            /// Nếu bản thân đã chết
            if (this.IsDead())
            {
                //if (this is KPlayer player)
                //{
                //    /// Thiết lập máu về 0
                //    this.m_CurrentLife = 0;
                //    /// Gửi yêu cầu đồng bộ sinh, nội, thể lực về Client
                //    this.ProcessSynsHPMPStaminaToClient();
                //    /// Bản đồ hiện tại
                //    GameMap gameMap = GameManager.MapMgr.DictMaps[this.CurrentMapCode];
                //    /// Thông báo mở bảng hồi sinh về Client
                //    string message = string.Format("Địa điểm trọng thương: <color=green>{0}</color>, vị trí <color=#42efff>({1}, {2})</color>.\nĐối tượng đả thương: <color=yellow>{3}</color>.", gameMap.MapName, (int) this.CurrentGrid.X, (int) this.CurrentGrid.Y, this.WhoKilledMeName);
                //    KT_TCPHandler.ShowClientReviveFrame(player, message, false);
                //}
                return;
            }

            try
			{
                if (this.LifeTime % 500 == 0)
                {
                    /// Thực thi trạng thái ngũ hành
                    this.ProcessSeriesState();
                    /// Thực hiện hồi máu mỗi nửa giây
                    this.ProcessFastGenenrate();
                    this.ProcessInvisibleState();
                    //this.DoPoisonState();
                    this.ProcessReduceHPCorrespondingToPositionChanged();
                    /// Gửi yêu cầu đồng bộ sinh, nội, thể lực về Client
                    this.ProcessSynsHPMPStaminaToClient();
                    /// Thực hiện Logic vòng sáng
                    this.ProcessAuraLogic();
                    /// Thực hiện tự xóa Buff hết thời hạn
                    this.Buffs.ClearTimeoutBuffs();
                }

                if (this.LifeTime % 5000 == 0)
                {
                    /// Thực hiện hồi máu mỗi 5 giây
                    this.ProcessNormalGenerate();
                }

            }
            catch (Exception ex)
			{
                LogManager.WriteLog(LogTypes.Exception, ex.ToString());
			}
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Thực hiện Logic vòng sáng
        /// </summary>
        private void ProcessAuraLogic()
        {
            /// Nếu đối tượng đã chết thì bỏ qua
            if (this.IsDead())
            {
                return;
            }

            /// Nếu chưa đến thời gian
            if (this.LifeTime % 5000 != 0)
            {
                return;
            }

            /// Duyệt danh sách vòng sáng hiện có
            foreach (BuffDataEx aura in this.Buffs.ListAruas)
            {
                /// Nếu vòng sáng không tồn tại
                if (aura == null)
                {
                    continue;
                }

                /// Nếu có kỹ năng đi kèm mà chưa được kiểm tra
                SkillDataEx childSkill = KSkill.GetSkillData(aura.Skill.Data.StartSkillID);
                if (childSkill != null)
                {
                    SkillLevelRef skillRef = new SkillLevelRef()
                    {
                        Data = childSkill,
                        AddedLevel = aura.Skill.AddedLevel,
                        BonusLevel = aura.Skill.BonusLevel,
                        CanStudy = false,
                    };

                    /// Thực hiện kỹ năng con
                    KTSkillManager.DoAruaLogic(this, null, skillRef);
                }
            }
        }

        /// <summary>
        /// Thực hiện gửi sự thay đổi máu về Client
        /// </summary>
        protected void ProcessSynsHPMPStaminaToClient()
        {
            /// Lượng máu có
            int hp = this.m_CurrentLife;

            /// Nếu đã chết
            if (this.IsDead())
			{
                /// Thiết lập máu về 0
                hp = 0;
			}

            if (this is KPlayer)
			{
                /// Gửi thông báo đến chính mình
                KT_TCPHandler.NotifySelfLifeChanged(this as KPlayer, hp);
            }

            /// Gửi thông báo đến người chơi khác
            KT_TCPHandler.NotifyObjectLifeChangedToAllPlayers(this, hp);

            /// Nếu đã chết
            if (hp <= 0)
            {
                /// Thiết lập máu
                this.m_CurrentLife = 0;
            }
        }

        /// <summary>
        /// Thực hiện gọi kỹ năng giảm máu khi khoảng cách thay đổi
        /// </summary>
        private void ProcessReduceHPCorrespondingToPositionChanged()
        {
            if (this.m_sRdcLifeWithDis != null && this.m_sRdcLifeWithDis.nMultiple > 0 && this.m_sRdcLifeWithDis.nMaxDis > 0 && this.m_sRdcLifeWithDis.nLauncher != null)
            {
                /// Nếu đối tượng ra chiêu đã chết
                if (this.m_sRdcLifeWithDis.nLauncher.IsDead())
                {
                    return;
                }
                /// Nếu đối tượng ra chiêu đang ở bản đồ khác
                else if (this.CurrentMapCode != this.m_sRdcLifeWithDis.nLauncher.CurrentMapCode)
                {
                    return;
                }
                /// Nếu đối tượng ra chiêu là người chơi nhưng đã rời mạng
                else if (this.m_sRdcLifeWithDis.nLauncher is KPlayer && (this.m_sRdcLifeWithDis.nLauncher as KPlayer).LogoutState)
                {
                    return;
                }

                /// Kỹ năng gây sát thương
                SkillDataEx skillData = KSkill.GetSkillData(this.m_sRdcLifeWithDis.nDamageSkillId);
                /// Nếu kỹ năng gây sát thương không tồn tại
                if (skillData == null)
                {
                    return;
                }
                /// Nếu kỹ năng gây sát thương có cấp độ dưới 0
                else if (this.m_sRdcLifeWithDis.nSkillLevel <= 0)
                {
                    return;
                }
                /// Kỹ năng theo cấp độ gây sát thương
                SkillLevelRef skill = new SkillLevelRef()
                {
                    Data = skillData,
                    AddedLevel = this.m_sRdcLifeWithDis.nSkillLevel,
                    CanStudy = false,
                };

                /// Vị trí trước đó
                UnityEngine.Vector2 lastPos = new UnityEngine.Vector2(this.m_sRdcLifeWithDis.nPrePosX, this.m_sRdcLifeWithDis.nPrePosY);
                /// Vị trí hiện tại
                UnityEngine.Vector2 currentPos = new UnityEngine.Vector2((int) this.CurrentPos.X, (int) this.CurrentPos.Y);
                /// Khoảng cách đã dịch chuyển
                float distance = UnityEngine.Vector2.Distance(lastPos, currentPos);

                /// Cập nhật lại vị trí hiện tại
                this.m_sRdcLifeWithDis.nPrePosX = (int) currentPos.x;
                this.m_sRdcLifeWithDis.nPrePosY = (int) currentPos.y;

                /// Nếu khoảng cách không thay đổi
                if (distance <= 0)
                {
                    return;
                }

                /// Nếu khoảng dịch lớn hơn Max của kỹ năng thì thiết lập lại
                if (distance > this.m_sRdcLifeWithDis.nMaxDis)
                {
                    distance = this.m_sRdcLifeWithDis.nMaxDis;
                }

                /// Tính toán % sát thương cộng thêm
                this.m_sRdcLifeWithDis.nDamageAddedP = (int) (distance * this.m_sRdcLifeWithDis.nMultiple / 100);

                /// Thực hiện gây sát thương
                AlgorithmProperty.AttackEnemy(this.m_sRdcLifeWithDis.nLauncher, this, skill, 0, currentPos, true);
            }
        }

        /// <summary>
        /// Thực hiện trạng thái ẩn thân
        /// </summary>
        private void ProcessInvisibleState()
        {
            /// Nếu có hiệu ứng ẩn thân
            if (this.m_InvisibleLevel != -1)
            {
                /// Giảm 9 Frame tương đương nửa giây
                this.m_InvisibleFrameCount -= 9;

                /// Nếu hết thời gian ẩn thân
                if (this.m_InvisibleFrameCount <= 0)
                {
                    /// Thực hiện sự kiện khi đối tượng kết thúc trạng thái ẩn thân
                    this.OnExitInvisibleState();

                    /// Thiết lập lại các thuộc tính ẩn thân về mặc định
                    this.RemoveInvisibleState();
                }
            }
        }

        /// <summary>
        /// Thực thi trạng thái trúng độc
        /// </summary>
        public void DoPoisonState()
        {
			/// Nếu có trạng thái trúng độc
			if (this.HaveState(KE_STATE.emSTATE_POISON))
			{
				/// Nếu đã chết thì bỏ qua
				if (this.IsDead())
                {
                    /// Hủy trạng thái trúng độc
                    this.m_nLastPoisonDamageIdx = null;
                    /// Hủy luồng trúng độc
                    KTPoisonTimerManager.Instance.RemovePoisonState(this);
                    return;
                }

                int nOldLife = this.m_CurrentLife;
                int nDamage = this.m_state[(int) KE_STATE.emSTATE_POISON].OtherParam;

                bool isDamaged = AlgorithmProperty.CalcDamage(this.m_nLastPoisonDamageIdx, this, nDamage, nDamage, DAMAGE_TYPE.damage_poison, false, null, true, 100, true);

                /// Nếu gây sát thương thành công
                if (isDamaged)
                {
                    AlgorithmProperty.SyncDamage(this.m_nLastPoisonDamageIdx, this, nOldLife - this.m_CurrentLife);
                }
			}
		}

        /// <summary>
        /// Thực hiện trừ thời gian trạng thái ngũ hành
        /// </summary>
        protected void ProcessSeriesState()
        {
            /// Duyệt danh sách trạng thái
            for (int i = (int) KE_STATE.emSTATE_BEGIN; i < (int) KE_STATE.emSTATE_ALLNUM; ++i)
            {
                /// Trạng thái tương ứng
                KNpcAttribGroup_State state = this.m_state[i];
                /// Nếu đã quá thời gian
                if (state.Duration > 0 && state.StartTick > 0 && state.IsOver)
				{
                    /// Xóa hiệu ứng tương ứng khỏi nhân vật
                    this.RemoveSpecialState((KE_STATE) i, true);
                }
            }
        }

        /// <summary>
        /// Thực hiện phục hồi nhanh mỗi nửa giây
        /// </summary>
        protected void ProcessFastGenenrate()
        {
            /// Bản đồ hiện tại
            GameMap map = KTMapManager.Find(this.CurrentMapCode);

            /// Khóa lại cho chắc
            lock (this.hpRegenerateLock)
            {
                /// Nếu đã chết thì thôi
                if (this.IsDead())
                {
                    this.m_CurrentLife = 0;
                    return;
                }

                /// Phục hồi sinh lực mỗi nửa giây
                if (this.m_CurrentLife > 0)
                {
                    int newHP = this.m_CurrentLife + (int) (this.m_CurrentLifeFastReplenish * (1 + this.m_CurrentLifeReplenishPercent / 100f));
                    /// Nếu đang ngồi thiền
                    if (this.m_eDoing == KE_NPC_DOING.do_sit)
                    {
                        newHP += map.SitHealHP;
                    }
                    if (newHP > this.m_CurrentLifeMax)
                    {
                        newHP = this.m_CurrentLifeMax;
                    }
                    if (newHP < 0)
                    {
                        newHP = 0;
                    }

                    /// Nếu đã chết thì thôi
                    if (this.IsDead())
                    {
                        return;
                    }

                    this.m_CurrentLife = newHP;
                }

                /// Nếu là người chơi thì mới có 2 dòng dưới
                if (this is KPlayer)
                {
                    /// Phục hồi nội lực mỗi nửa giây
                    if (this.m_CurrentMana >= 0)
                    {
                        this.m_CurrentMana += (int) (this.m_CurrentManaFastReplenish * (1 + this.m_CurrentManaReplenishPercent / 100f));
                        /// Nếu đang ngồi thiền
                        if (this.m_eDoing == KE_NPC_DOING.do_sit)
                        {
                            this.m_CurrentMana += map.SitHealMP;
                        }
                        this.m_CurrentMana = Math.Min(this.m_CurrentMana, this.m_CurrentManaMax);
                        this.m_CurrentMana = Math.Max(this.m_CurrentMana, 0);
                    }

                    /// Phục hồi thể lực mỗi nửa giây
                    if (this.m_CurrentStamina >= 0)
                    {
                        this.m_CurrentStamina += (int) (this.m_CurrentFastStaminaReplenish * (1 + this.m_CurrentStaminaReplenishPercent / 100f));
                        /// Nếu đang ngồi thiền
                        if (this.m_eDoing == KE_NPC_DOING.do_sit)
                        {
                            this.m_CurrentStamina += map.SitHealStamina;
                        }
                        this.m_CurrentStamina = Math.Min(this.m_CurrentStamina, this.m_CurrentStaminaMax);
                        this.m_CurrentStamina = Math.Max(this.m_CurrentStamina, 0);
                    }
                }
            }
        }

        /// <summary>
        /// Thực hiện phục hồi mỗi 5 giây
        /// </summary>
        protected void ProcessNormalGenerate()
        {
            /// Nếu đã chết thì thôi
            if (this.IsDead())
            {
                return;
            }

            /// Phục hồi sinh lực mỗi 5 giây
            if (this.m_CurrentLife > 0)
            {
                int newHP = this.m_CurrentLife + (int) ((this.m_LifeReplenish + this.m_CurrentLifeReplenish) * (1 + this.m_CurrentLifeReplenishPercent / 100f));
                if (newHP > this.m_CurrentLifeMax)
                {
                    newHP = this.m_CurrentLifeMax;
                }
                if (newHP < 0)
                {
                    newHP = 0;
                }

                /// Nếu đã chết thì thôi
                if (this.IsDead())
                {
                    return;
                }

                if (this.m_CurrentLife > 0)
                {
                    this.m_CurrentLife = newHP;
                }
            }

            /// Nếu là người chơi thì mới có 2 dòng dưới
            if (this is KPlayer)
            {
                /// Phục hồi nội lực mỗi 5 giây
                if (this.m_CurrentMana >= 0)
                {
                    this.m_CurrentMana += (int) ((this.m_ManaReplenish + this.m_CurrentManaReplenish) * (1 + this.m_CurrentManaReplenishPercent / 100f));
                    this.m_CurrentMana = Math.Min(this.m_CurrentMana, this.m_CurrentManaMax);
                    this.m_CurrentMana = Math.Max(this.m_CurrentMana, 0);
                }

                /// Phục hồi thể lực mỗi 5 giây
                if (this.m_CurrentStamina >= 0)
                {
                    this.m_CurrentStamina += (int) ((this.m_StaminaReplenish + this.m_CurrentStaminaReplenish) * (1 + this.m_CurrentStaminaReplenishPercent / 100f));
                    this.m_CurrentStamina = Math.Min(this.m_CurrentStamina, this.m_CurrentStaminaMax);
                    this.m_CurrentStamina = Math.Max(this.m_CurrentStamina, 0);
                }
            }
        }

        /// <summary>
        /// Kích hoạt kỹ năng tự động
        /// </summary>
        /// <param name="autoSkill"></param>
        protected void ActivateAutoSkill(KNpcAutoSkill autoSkill, GameObject target)
        {
            try
			{
                /// Cập nhật thời điểm kích hoạt kỹ năng tự động
                autoSkill.LastActivateTick = KTGlobal.GetCurrentTimeMilis();

                /// Tỷ lệ kích hoạt
                int castPercent = autoSkill.CastSkillActivatePercent;
                int rand = KTGlobal.GetRandomNumber(0, 100);
                /// Nếu được kích hoạt
                if (rand <= castPercent)
                {
                    /// Kỹ năng kích hoạt
                    SkillLevelRef castSkill = autoSkill.CastSkill;

                    /// Nếu tồn tại kỹ năng Buff yêu cầu có
                    if (autoSkill.Info.ParentSkillID != -1 && this.Buffs.HasBuff(autoSkill.Info.ParentSkillID))
                    {
                        /// Nếu tồn tại kỹ năng kích hoạt
                        if (castSkill != null)
                        {
                            /// Nếu Buff là bùa chú
                            BuffDataEx buff = this.Buffs.GetBuff(autoSkill.OwnerSkillID);
                            if (buff != null && buff.CurseOwner != null)
                            {
                                if (target != null)
                                {
                                    /// Nếu mục tiêu còn sống
                                    if (!target.IsDead())
                                    {
                                        KTSkillManager.UseSkill(buff.CurseOwner, this, null, castSkill, true, new UnityEngine.Vector2((int) target.CurrentPos.X, (int) target.CurrentPos.Y));
                                    }
                                }
                                else
                                {
                                    KTSkillManager.UseSkill(buff.CurseOwner, this, null, castSkill, true);
                                }
                            }
                            else
                            {
                                if (target != null)
                                {
                                    /// Nếu mục tiêu còn sống
                                    if (!target.IsDead())
                                    {
                                        KTSkillManager.UseSkill(this, target, null, castSkill, true, new UnityEngine.Vector2((int) target.CurrentPos.X, (int) target.CurrentPos.Y));
                                    }
                                }
                                else
                                {
                                    KTSkillManager.UseSkill(this, target, null, castSkill, true);
                                }
                            }
                        }
                    }
                    else
                    {
                        /// Nếu tồn tại kỹ năng kích hoạt
                        if (castSkill != null)
                        {
                            /// Nếu Buff là bùa chú
                            BuffDataEx buff = this.Buffs.GetBuff(autoSkill.OwnerSkillID);
                            if (buff != null && buff.CurseOwner != null)
                            {
                                if (target != null)
                                {
                                    /// Nếu mục tiêu còn sống
                                    if (!target.IsDead())
                                    {
                                        KTSkillManager.UseSkill(buff.CurseOwner, this, null, castSkill, true, new UnityEngine.Vector2((int) target.CurrentPos.X, (int) target.CurrentPos.Y));
                                    }
                                }
                                else
                                {
                                    KTSkillManager.UseSkill(buff.CurseOwner, this, null, castSkill, true);
                                }
                            }
                            else
                            {
                                if (target != null)
                                {
                                    /// Nếu mục tiêu còn sống
                                    if (!target.IsDead())
                                    {
                                        KTSkillManager.UseSkill(this, target, null, castSkill, true, new UnityEngine.Vector2((int) target.CurrentPos.X, (int) target.CurrentPos.Y));
                                    }
                                }
                                else
                                {
                                    KTSkillManager.UseSkill(this, target, null, castSkill, true);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
			{
                LogManager.WriteLog(LogTypes.Exception, ex.ToString());
			}
        }
        #endregion

        #region Events
        /// <summary>
        /// Trả về danh sách kỹ năng tự kích hoạt
        /// </summary>
        /// <returns></returns>
        public List<KNpcAutoSkill> GetListAutoSkills()
		{
            if (!(this is KPlayer) || this.listAutoSkills.Count <= 0)
			{
                return new List<KNpcAutoSkill>();
			}
            return this.listAutoSkills.Values.ToList();
		}

        /// <summary>
        /// Thêm kỹ năng tự kích hoạt
        /// </summary>
        /// <param name="skillID"></param>
        /// <param name="level"></param>
        public void AddAutoSkill(int skillID, int level, int ownerSkillID)
        {
            /// Nếu không phải người chơi thì thôi
            if (!(this is KPlayer))
            {
                return;
            }

            AutoSkill skill = KSkill.GetAutoSkill(skillID);
            if (skill == null)
            {
                return;
            }

            KNpcAutoSkill autoSkill = new KNpcAutoSkill()
            {
                Owner = this,
                Info = skill,
                Level = level,
                OwnerSkillID = ownerSkillID,
            };
            this.listAutoSkills[autoSkill.Info.ID] = autoSkill;
        }

        /// <summary>
        /// Xóa kỹ năng tự kích hoạt tương ứng
        /// </summary>
        /// <param name="skillID"></param>
        public void RemoveAutoSkill(int skillID)
        {
            /// Nếu không phải người chơi thì thôi
            if (!(this is KPlayer))
            {
                return;
            }

            if (this.listAutoSkills.TryGetValue(skillID, out _))
            {
                this.listAutoSkills.TryRemove(skillID, out _);
            }
        }

        /// <summary>
        /// Xóa toàn bộ kỹ năng tự kích hoạt
        /// </summary>
        public void RemoveAllAutoSkills()
        {
            /// Nếu không phải người chơi thì thôi
            if (!(this is KPlayer))
            {
                return;
            }

            this.listAutoSkills.Clear();
        }

        /// <summary>
        /// Sự kiện khi đối tượng bắt đầu vào trạng thái ẩn thân
        /// </summary>
        public void OnEnterInvisibleState()
        {

        }

        /// <summary>
        /// Sự kiện khi đối tượng kết thúc trạng thái ẩn thân
        /// </summary>
        public void OnExitInvisibleState()
        {

        }

        /// <summary>
        /// Sự kiện khi đối tượng nhận Buff tương ứng
        /// </summary>
        /// <param name="buff"></param>
        public void OnReceiveBuff(BuffDataEx buff)
        {

        }

        /// <summary>
        /// Sự kiện khi đối tượng bị mất Buff tương ứng
        /// </summary>
        /// <param name="buff"></param>
        public void OnLostBuff(BuffDataEx buff)
        {
            /// Nếu không phải người chơi thì thôi
            if (!(this is KPlayer))
            {
                return;
            }

            if (buff == null)
            {
                return;
            }

			try
			{
                /// Duyệt danh sách các kỹ năng tự kích hoạt
                foreach (KNpcAutoSkill autoSkill in this.GetListAutoSkills())
                {
                    if (autoSkill == null)
                    {
                        continue;
                    }

                    /// Nếu đây là loại kỹ năng kích hoạt khi mất Buff ID tương ứng
                    if (!autoSkill.IsCoolDown && autoSkill.Info.ActivateIfLostBuff == buff.Skill.SkillID)
                    {
                        this.ActivateAutoSkill(autoSkill, null);
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Exception, ex.ToString());
            }
        }
        /// <summary>
        /// Sự kiện khi đối tượng bị sát thương chí mạng
        /// </summary>
        /// <param name="attacker"></param>
        public void OnBeCritHit(GameObject attacker)
        {
            /// Nếu không phải người chơi thì thôi
            if (!(this is KPlayer))
            {
                return;
            }

            /// Nếu đối tượng không tồn tại
            if (attacker == null)
            {
                return;
            }
            /// Nếu đối tượng đã chết
            else if (attacker.IsDead())
            {
                return;
            }

            try
            {
                /// Duyệt danh sách các kỹ năng tự kích hoạt
                foreach (KNpcAutoSkill autoSkill in this.GetListAutoSkills())
                {
                    if (autoSkill == null)
                    {
                        continue;
                    }

                    /// Nếu đây là loại kỹ năng kích hoạt khi bị đánh chí mạng
                    if (!autoSkill.IsCoolDown && autoSkill.Info.ActivateWhenBeCritHit)
                    {
                        /// Khoảng cách đến đối phương
                        float distance = KTGlobal.GetDistanceBetweenPoints(this.CurrentPos, attacker.CurrentPos);

                        /// Nếu khoảng cách quá xa
                        if (distance >= 1000)
                        {
                            continue;
                        }

                        /// Kích hoạt kỹ năng tự động
                        this.ActivateAutoSkill(autoSkill, attacker);
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Exception, ex.ToString());
            }
        }

        /// <summary>
        /// Sự kiện khi đối tượng tấn công mục tiêu và ở trạng thái đánh chí mạng
        /// </summary>
        /// <param name="target"></param>
        public void OnDoCritOnTarget(GameObject target)
        {
            /// Nếu không phải người chơi thì thôi
            if (!(this is KPlayer))
            {
                return;
            }

            /// Nếu đối tượng không tồn tại
            if (target == null)
            {
                return;
            }
            /// Nếu đối tượng đã chết
            else if (target.IsDead())
            {
                return;
            }

            try
			{
                /// Duyệt danh sách các kỹ năng tự kích hoạt
                foreach (KNpcAutoSkill autoSkill in this.GetListAutoSkills())
                {
                    if (autoSkill == null)
                    {
                        continue;
                    }

                    /// Nếu đây là loại kỹ năng kích hoạt khi đánh chí mạng mục tiêu
                    if (!autoSkill.IsCoolDown && autoSkill.Info.ActivateWhenDoCritHit)
                    {
                        /// Khoảng cách đến đối phương
                        float distance = KTGlobal.GetDistanceBetweenPoints(this.CurrentPos, target.CurrentPos);

                        /// Nếu khoảng cách quá xa
                        if (distance >= 1000)
                        {
                            continue;
                        }

                        /// Kích hoạt kỹ năng tự động
                        this.ActivateAutoSkill(autoSkill, target);
                    }
                    /// Nếu đây là loại kỹ năng kích hoạt sau khi đánh chí mạng số lần tương ứng
                    else if (!autoSkill.IsCoolDown && autoSkill.Info.ActivateAfterDoTotalCritHit > 0)
                    {
                        /// Tăng số lần cộng dồn đếm lên
                        autoSkill.Info.StackCount++;

                        /// Nếu đủ số lượt mới kích hoạt kỹ năng
                        if (autoSkill.Info.StackCount >= autoSkill.Info.ActivateAfterDoTotalCritHit)
                        {
                            /// Khoảng cách đến đối phương
                            float distance = KTGlobal.GetDistanceBetweenPoints(this.CurrentPos, target.CurrentPos);

                            /// Nếu khoảng cách quá xa
                            if (distance >= 1000)
                            {
                                continue;
                            }

                            /// Kích hoạt kỹ năng tự động
                            this.ActivateAutoSkill(autoSkill, target);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Exception, ex.ToString());
            }
        }

        /// <summary>
        /// Sự kiện khi đối tượng tấn công mục tiêu
        /// </summary>
        /// <param name="target"></param>
        /// <param name="nDamage"></param>
        public virtual void OnHitTarget(GameObject target, int nDamage)
        {
            /// Nếu không phải người chơi thì thôi
            if (!(this is KPlayer))
            {
                return;
            }

            /// Nếu đối tượng không tồn tại
            if (target == null)
            {
                return;
            }
            ///// Nếu đối tượng đã chết
            //else if (target.IsDead())
            //{
            //    return;
            //}

            try
			{
                /// Duyệt danh sách các kỹ năng tự kích hoạt
                foreach (KNpcAutoSkill autoSkill in this.GetListAutoSkills())
                {
                    if (autoSkill == null)
                    {
                        continue;
                    }

                    /// Nếu đây là loại kỹ năng kích hoạt khi đánh trúng mục tiêu
                    if (!autoSkill.IsCoolDown && autoSkill.Info.ActivateIfHitTarget)
                    {
                        /// Khoảng cách đến đối phương
                        float distance = KTGlobal.GetDistanceBetweenPoints(this.CurrentPos, target.CurrentPos);

                        /// Nếu khoảng cách quá xa
                        if (distance >= 1000)
                        {
                            continue;
                        }

                        /// Kích hoạt kỹ năng tự động
                        this.ActivateAutoSkill(autoSkill, target);
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Exception, ex.ToString());
            }
        }

        /// <summary>
        /// Sự kiện khi đối tượng bị tấn công
        /// </summary>
        /// <param name="attacker"></param>
        /// <param name="nDamage"></param>
        public virtual void OnBeHit(GameObject attacker, int nDamage)
        {
            /// Nếu không phải người chơi thì thôi
            if (!(this is KPlayer))
            {
                return;
            }

            /// Nếu đối tượng đả thương không tồn tại
            if (attacker == null)
            {
                return;
            }
            /// Nếu đối tượng đả thương đã chết
            else if (attacker.IsDead())
            {
                return;
            }

            try
			{
                /// Duyệt danh sách các kỹ năng tự kích hoạt
                foreach (KNpcAutoSkill autoSkill in this.GetListAutoSkills())
                {
                    if (autoSkill == null)
                    {
                        continue;
                    }

                    /// Nếu đây là loại kỹ năng kích hoạt khi bị đánh trúng
                    if (!autoSkill.IsCoolDown && autoSkill.Info.ActivateWhenBeHit)
                    {
                        /// Khoảng cách đến đối phương
                        float distance = KTGlobal.GetDistanceBetweenPoints(this.CurrentPos, attacker.CurrentPos);

                        /// Nếu khoảng cách quá xa
                        if (distance >= 1000)
                        {
                            continue;
                        }

                        /// Kích hoạt kỹ năng tự động
                        this.ActivateAutoSkill(autoSkill, attacker);
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Exception, ex.ToString());
            }
        }

        /// <summary>
        /// Sự kiện khi đối tượng tấn công mục tiêu bằng kỹ năng tương ứng
        /// </summary>
        /// <param name="attacker"></param>
        /// <param name="skill"></param>
        public void OnHitTargetWithSkill(GameObject target, SkillLevelRef skill)
        {
            /// Nếu không phải người chơi thì thôi
            if (!(this is KPlayer))
            {
                return;
            }

            /// Nếu đối tượng không tồn tại
            if (target == null || skill == null)
            {
                return;
            }
            ///// Nếu đối tượng đã chết
            //else if (target.IsDead())
            //{
            //    return;
            //}

            try
			{
                /// Duyệt danh sách các kỹ năng tự kích hoạt
                foreach (KNpcAutoSkill autoSkill in this.GetListAutoSkills())
                {
                    if (autoSkill == null)
                    {
                        continue;
                    }

                    /// Nếu đây là loại kỹ năng kích hoạt khi tấn công mục tiêu bằng một trong các kỹ năng trong danh sách tương ứng
                    if (!autoSkill.IsCoolDown && autoSkill.Info.ActivateWhenHitWithSkills.Count > 0 && autoSkill.Info.ActivateWhenHitWithSkills.Contains(skill.SkillID))
                    {
                        /// Khoảng cách đến đối phương
                        float distance = KTGlobal.GetDistanceBetweenPoints(this.CurrentPos, target.CurrentPos);

                        /// Nếu khoảng cách quá xa
                        if (distance >= 1000)
                        {
                            continue;
                        }

                        /// Kích hoạt kỹ năng tự động
                        this.ActivateAutoSkill(autoSkill, this);
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Exception, ex.ToString());
            }
        }

        /// <summary>
        /// Sự kiện khi đối tượng bị tấn công bởi kỹ năng tương ứng
        /// </summary>
        /// <param name="attacker"></param>
        /// <param name="skill"></param>
        public void OnBeHitBySkill(GameObject attacker, SkillLevelRef skill)
        {
            /// Nếu không phải người chơi thì thôi
            if (!(this is KPlayer))
            {
                return;
            }

            /// Nếu đối tượng đả thương không tồn tại
            if (attacker == null || skill == null)
            {
                return;
            }
            /// Nếu đối tượng đả thương đã chết
            else if (attacker.IsDead())
            {
                return;
            }

            try
			{
                /// Duyệt danh sách các kỹ năng tự kích hoạt
                foreach (KNpcAutoSkill autoSkill in this.GetListAutoSkills())
                {
                    if (autoSkill == null)
                    {
                        continue;
                    }

                    /// Nếu đây là loại kỹ năng kích hoạt khi bị đánh trúng bởi một trong các kỹ năng trong danh sách tương ứng
                    if (!autoSkill.IsCoolDown && autoSkill.Info.ActivateWhenBeHitBySkills.Count > 0 && autoSkill.Info.ActivateWhenBeHitBySkills.Contains(skill.SkillID))
                    {
                        /// Khoảng cách đến đối phương
                        float distance = KTGlobal.GetDistanceBetweenPoints(this.CurrentPos, attacker.CurrentPos);

                        /// Nếu khoảng cách quá xa
                        if (distance >= 1000)
                        {
                            continue;
                        }

                        /// Kích hoạt kỹ năng tự động
                        this.ActivateAutoSkill(autoSkill, attacker);
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Exception, ex.ToString());
            }
        }

        /// <summary>
        /// Sự kiện khi đối tượng bị mất máu
        /// </summary>
        /// <param name="attacker"></param>
        public void OnHPDropped(GameObject attacker)
        {
            /// Nếu không phải người chơi thì thôi
            if (!(this is KPlayer))
            {
                return;
            }

            /// Nếu đối tượng đả thương không tồn tại
            if (attacker == null)
			{
                return;
			}
            /// Nếu đối tượng đả thương đã chết
            else if (attacker.IsDead())
			{
                return;
			}

            try
            {
                /// % máu hiện tại
                int hpPercent = this.m_CurrentLife * 100 / this.m_CurrentLifeMax;

                /// Duyệt danh sách các kỹ năng tự kích hoạt
                foreach (KNpcAutoSkill autoSkill in this.GetListAutoSkills())
                {
                    if (autoSkill == null)
                    {
                        continue;
                    }

                    /// Nếu đây là loại kỹ năng kích hoạt khi lượng máu giảm xuống dưới ngưỡng
                    if (!autoSkill.IsCoolDown && autoSkill.Info.ActivateWhenHPPercentDropBelow > 0 && hpPercent <= autoSkill.Info.ActivateWhenHPPercentDropBelow)
                    {
                        /// Khoảng cách đến đối phương
                        float distance = KTGlobal.GetDistanceBetweenPoints(this.CurrentPos, attacker.CurrentPos);

                        /// Nếu khoảng cách quá xa
                        if (distance >= 1000)
						{
                            continue;
						}

                        /// Kích hoạt kỹ năng tự động
                        this.ActivateAutoSkill(autoSkill, attacker);
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Exception, ex.ToString());
            }
        }

        /// <summary>
        /// Sự kiện khi đối tượng bị chết
        /// </summary>
        /// <param name="attacker"></param>
        public virtual void OnDie(GameObject attacker)
        {
            /// Nếu không phải người chơi thì thôi
            if (!(this is KPlayer))
            {
                return;
            }

            if (attacker == null)
            {
                return;
            }
            /// Nếu đối tượng đả thương đã chết
            else if (attacker.IsDead())
            {
                return;
            }

            /// Thiết lập là đã chết
            this.m_eDoing = KE_NPC_DOING.do_death;
            this.m_CurrentLife = 0;

            try
			{
                /// Duyệt danh sách các kỹ năng tự kích hoạt
                foreach (KNpcAutoSkill autoSkill in this.GetListAutoSkills())
                {
                    if (autoSkill == null)
                    {
                        continue;
                    }

                    /// Nếu đây là loại kỹ năng kích hoạt khi chết
                    if (!autoSkill.IsCoolDown && autoSkill.Info.ActivateAfterDie)
                    {
                        /// Khoảng cách đến đối phương
                        float distance = KTGlobal.GetDistanceBetweenPoints(this.CurrentPos, attacker.CurrentPos);

                        /// Nếu khoảng cách quá xa
                        if (distance >= 1000)
                        {
                            continue;
                        }

                        /// Kích hoạt kỹ năng tự động
                        this.ActivateAutoSkill(autoSkill, attacker);
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Exception, ex.ToString());
            }
        }

        /// <summary>
        /// Sự kiện khi giết đối tượng
        /// </summary>
        /// <param name="deadObj"></param>
        public virtual void OnKillObject(GameObject deadObj)
        {

        }
        #endregion
    }
}
