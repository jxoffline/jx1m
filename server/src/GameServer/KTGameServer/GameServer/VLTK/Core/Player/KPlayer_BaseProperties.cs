//#define USING_LIMIT_ASSIGNED_POINT

using GameServer.KiemThe.Entities;
using GameServer.KiemThe.Logic;
using Server.Data;
using Server.Tools;
using System;
using System.Collections.Generic;

namespace GameServer.Logic
{
    /// <summary>
    /// Định nghĩa các thuộc tính
    /// </summary>
    public partial class KPlayer
    {
        protected const int NUM_20E = 2000000000;

        #region Define

        /// <summary>
        /// ID kỹ năng vòng sáng lần trước khi Detach
        /// </summary>
        private int lastAuraSkillID = -1;


        /// <summary>
        /// ID kỹ năng chính thay đổi lần trước (áp dụng để tính lực tay khi hiện ở khung nhân vật)
        /// </summary>
        public int LastMainSkillID { get; set; } = -1;

        /// <summary>
        /// Tiềm năng hiện tại (chưa phân phối)
        /// </summary>
        protected int m_wRemainPotential
        {
            get
            {
                return this.m_nBaseRemainPotential + this.m_nBonusRemainPotential - this.m_wStrength - this.m_wDexterity - this.m_wVitality - this.m_wEnergy;
            }
        }

        /// <summary>
        /// Tiềm năng cơ bản
        /// </summary>
        protected int m_nBaseRemainPotential;

        /// <summary>
        /// Tiềm năng có được thêm được lưu ở DB (Do ăn bánh các kiểu)
        /// </summary>
        protected int m_nBonusRemainPotential;

        /// <summary>
        /// Danh sách kỹ năng mà người chơi đã học
        /// thuộc tính set sẽ được gọi khi nhân vật có sự thay đổi về skill
        /// </summary>
        public SkillTree _SkillTree { get; set; }

        /// <summary>
        /// Sức cơ bản
        /// </summary>
        protected int m_wStrength;

        /// <summary>
        /// Thân cơ bản
        /// </summary>
        protected int m_wDexterity;

        /// <summary>
        /// Ngoại cơ bản
        /// </summary>
        protected int m_wVitality;

        /// <summary>
        /// Nội cơ bản
        /// </summary>
        protected int m_wEnergy;

        /// <summary>
        /// Sức hiện tại
        /// </summary>
        protected int m_wCurStrength;

        /// <summary>
        /// Thân hiện tại
        /// </summary>
        protected int m_wCurDexterity;

        /// <summary>
        /// Ngoại hiện tại
        /// </summary>
        protected int m_wCurVitality;

        /// <summary>
        /// Nội hiện tại
        /// </summary>
        protected int m_wCurEnergy;

        /// <summary>
        /// Tăng % kinh nghiệm nhận được
        /// </summary>
        public int m_nExpEnhancePercent { get; set; } = 0;

        /// <summary>
        /// Tăng % kinh nghiệm của kỹ năng cấp 120
        /// </summary>
        protected int m_n120SkillExpEnhancePercent;

        /// <summary>
        /// Hoạt lực hiện tại
        /// </summary>
        protected int m_dwGatherPoint;

        /// <summary>
        /// Tinh lực hiện tại
        /// </summary>
        protected int m_dwMakePoint;

        /// <summary>
        /// Số điểm kỹ năng cơ bản
        /// </summary>
        protected int m_nBaseSkillPoints;

        /// <summary>
        /// Số điểm kỹ năng hiện có
        /// </summary>
        protected int m_nCurSkillPoints
        {
            get
            {
                return this.m_nBaseSkillPoints + this.m_nBonusSkillPoints - this.Skills.AddedSkillPoints;
            }
        }

        /// <summary>
        /// Điểm kỹ năng cộng thêm từ ăn bánh được lưu trong DB
        /// </summary>
        protected int m_nBonusSkillPoints;

        /// <summary>
        /// Phục hồi sinh lực theo % Ngoại hiện tại
        /// </summary>
        public int m_nFastLifeReplenishByVitality { get; set; }

        /// <summary>
        /// Tăng vật công cơ bản của vũ khí dựa theo số lần Ngoại hiện tại
        /// </summary>
        public int m_nAddWeaponBaseDamageTrimByVitality { get; set; }

        #endregion Define

        #region Protected Methods

        /// <summary>
        /// Kiểm tra điểm tiềm năng cộng vào có hợp lệ không, nếu không hợp lệ thì phân phối lại cho giá trị hợp lệ đẩy ra ngoài
        /// </summary>
        /// <param name="nStrengthAssign"></param>
        /// <param name="nDexterityAssign"></param>
        /// <param name="nVitalityAssign"></param>
        /// <param name="nEnergyAssign"></param>
        /// <returns></returns>
        public bool CheckAssignPotential(ref int nStrengthAssign, ref int nDexterityAssign, ref int nVitalityAssign, ref int nEnergyAssign)
        {
            /// Tổng điểm tiềm năng được cộng thêm từ các loại bánh đã ăn
            int additionRemainPoint = this.m_nBonusRemainPotential;
            /// Tổng điểm tiềm năng có từ Base
            int baseRemainPoint = this.m_nBaseRemainPotential;

            /// Tính điểm mới sau khi cộng
            int str = this.m_wStrength + nStrengthAssign;
            int dex = this.m_wDexterity + nDexterityAssign;
            int vit = this.m_wVitality + nVitalityAssign;
            int ene = this.m_wEnergy + nEnergyAssign;

#if USING_LIMIT_ASSIGNED_POINT
			/// Kiểm tra công thức cộng vào như nào cho hợp lý giữa các cấp độ
			int maxStr = KPlayerSetting.GetLevelMaxStrength(this);
			int maxEnergy = KPlayerSetting.GetLevelMaxEnergy(this);
			int maxVitality = KPlayerSetting.GetLevelMaxVitality(this);
			int maxDex = KPlayerSetting.GetLevelMaxDexterity(this);

			/// Nếu Str vượt quá ngưỡng
			if (str > maxStr)
            {
				str = maxStr;
            }
			/// Nếu Energy vượt quá ngưỡng
			if (ene > maxEnergy)
            {
				ene = maxEnergy;
            }
			/// Nếu Vitality vượt quá ngưỡng
			if (vit > maxVitality)
            {
				vit = maxVitality;
            }
			/// Nếu Dex vượt quá ngưỡng
			if (dex > maxDex)
            {
				dex = maxDex;
            }
#endif

            /// Tổng điểm tiềm năng còn lại
            int totalRemainPointLeft = additionRemainPoint + baseRemainPoint - str - dex - vit - ene;

            /// BUG
            if (totalRemainPointLeft < 0)
            {
                KTPlayerManager.ShowNotification(this, "Có lỗi trong quá trình phân phối điểm tiềm năng. Tất cả điểm tiềm năng bị làm mới từ đầu.");

                /// Xóa toàn bộ điểm đã phân phối trước đó
                this.ChangeStrength(-this.m_wStrength);
                this.ChangeDexterity(-this.m_wDexterity);
                this.ChangeVitality(-this.m_wVitality);
                this.ChangeEnergy(-this.m_wEnergy);

                nStrengthAssign = 0;
                nDexterityAssign = 0;
                nVitalityAssign = 0;
                nEnergyAssign = 0;

                return false;
            }

            /// Trả lại kết quả sau khi đã kiểm tra ngưỡng Max
            nStrengthAssign = str - this.m_wStrength;
            nEnergyAssign = ene - this.m_wEnergy;
            nVitalityAssign = vit - this.m_wVitality;
            nDexterityAssign = dex - this.m_wDexterity;

            /// Trả ra TRUE
            return true;
        }

        #endregion Protected Methods

        #region Public Methods

        /// <summary>
        /// Trả về tổng điểm tiềm năng có được thêm nhờ ăn bánh được lưu trong DB
        /// </summary>
        /// <returns></returns>
        public int GetBonusRemainPotentialPoints()
        {
            return this.m_nBonusRemainPotential;
        }

        /// <summary>
        /// Thiết lập tổng điểm tiềm năng có được thêm nhờ ăn bánh được lưu ở trong DB
        /// </summary>
        /// <param name="value"></param>
        public void SetBonusRemainPotentialPoints(int value)
        {
            this.m_nBonusRemainPotential = value;

            /// THỰC HIỆN GHI LUÔN VÀO DB
            Global.SaveRoleParamsInt32ValueToDB(this, RoleParamName.TotalPropPoint, this.m_nBonusRemainPotential, true);
        }

        /// <summary>
        /// Trả về tổng điểm tiềm năng cơ bản có được
        /// </summary>
        public int GetBaseRemainPotentialPoints()
        {
            return this.m_nBaseRemainPotential;
        }

        /// <summary>
        /// Thiết lập tổng điểm tiềm năng cơ bản có được
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public void SetBaseRemainPotentialPoints(int value)
        {
            this.m_nBaseRemainPotential = value;
        }

        /// <summary>
        /// Trả về giá trị điểm kỹ năng được cộng thêm từ ăn bánh, được lưu trong DB
        /// </summary>
        public int GetBonusSkillPoint()
        {
            return this.m_nBonusSkillPoints;
        }

        /// <summary>
        /// Thiết lập giá trị điểm kỹ năng được cộng thêm từ ăn bánh, được lưu trong DB
        /// </summary>
        /// <param name="value"></param>
        /// <param name="bSyncClient"></param>
        public void SetBonusSkillPoint(int value, bool bSyncClient = true)
        {
            this.m_nBonusSkillPoints = value;

            /// THỰC HIỆN GHI LUÔN VÀO DB
            Global.SaveRoleParamsInt32ValueToDB(this, RoleParamName.TotalSkillPoint, this.m_nBonusSkillPoints, true);

            /// Nếu có gửi thông báo về Client
            if (bSyncClient)
            {
                /// Gửi tín hiệu thay đổi danh sách kỹ năng về Client
                KT_TCPHandler.SendRenewSkillList(this);
            }
        }

        /// <summary>
        /// Thiết lập tổng điểm kỹ năng cơ bản
        /// </summary>
        /// <param name="value"></param>
        public void SetBaseSkillPoints(int value)
        {
            this.m_nBaseSkillPoints = value;
        }

        /// <summary>
        /// Trả về tổng số điểm kỹ năng hiện có
        /// </summary>
        /// <returns></returns>
        public int GetBaseSkillPoint()
        {
            return this.m_nBaseSkillPoints;
        }

        /// <summary>
        /// Số điểm kỹ năng còn lại
        /// </summary>
        /// <returns></returns>
        public int GetCurrentSkillPoints()
        {
            return this.m_nCurSkillPoints;
        }

        /// <summary>
        /// Trả ra giá trị điểm tiềm năng hiện tại chưa được phân phối
        /// </summary>
        /// <returns></returns>
        public int GetRemainPotential()
        {
            return this.m_wRemainPotential;
        }

        /// <summary>
        /// Trả về giá trị tinh lực
        /// </summary>
        /// <returns></returns>
        public int GetMakePoint()
        {
            return this.m_dwMakePoint;
        }

        /// <summary>
        /// Trả về giá trị hoạt lực
        /// </summary>
        /// <returns></returns>
        public int GetGatherPoint()
        {
            return this.m_dwGatherPoint;
        }

        /// <summary>
        /// Trả ra giá trị Sức cơ bản
        /// </summary>
        /// <returns></returns>
        public int GetStrength()
        {
            return this.m_wStrength;
        }

        /// <summary>
        /// Trả ra giá trị Thân cơ bản
        /// </summary>
        /// <returns></returns>
        public int GetDexterity()
        {
            return this.m_wDexterity;
        }

        /// <summary>
        /// Trả ra giá trị Ngoại cơ bản
        /// </summary>
        /// <returns></returns>
        public int GetVitality()
        {
            return this.m_wVitality;
        }

        /// <summary>
        /// Trả ra giá trị Nội cơ bản
        /// </summary>
        /// <returns></returns>
        public int GetEnergy()
        {
            return this.m_wEnergy;
        }

        /// <summary>
        /// Trả ra giá trị Sức hiện tại
        /// </summary>
        /// <returns></returns>
        public int GetCurStrength()
        {
            return this.m_wCurStrength;
        }

        /// <summary>
        /// Trả ra giá trị Thân hiện tại
        /// </summary>
        /// <returns></returns>
        public int GetCurDexterity()
        {
            return this.m_wCurDexterity;
        }

        /// <summary>
        /// Trả ra giá trị Ngoại hiện tại
        /// </summary>
        /// <returns></returns>
        public int GetCurVitality()
        {
            return this.m_wCurVitality;
        }

        /// <summary>
        /// Trả ra giá trị Nội hiện tại
        /// </summary>
        /// <returns></returns>
        public int GetCurEnergy()
        {
            return this.m_wCurEnergy;
        }

        /// <summary>
        /// Phân phối điểm tiềm năng
        /// </summary>
        /// <param name="nStrengthAssign"></param>
        /// <param name="nDexterityAssign"></param>
        /// <param name="nVitalityAssign"></param>
        /// <param name="nEnergyAssign"></param>
        /// <param name="bNegativePermit"></param>
        /// <returns></returns>
        public bool AssignPotential(int nStrengthAssign, int nDexterityAssign, int nVitalityAssign, int nEnergyAssign)
        {
#if USING_LIMIT_ASSIGNED_POINT
			/// Nếu không có nhánh thì toác
			if (this.m_cPlayerFaction.GetFactionId() == 0 || this.m_cPlayerFaction.GetRouteId() == 0)
            {
				PlayerManager.ShowNotification(this, "Chức năng này yêu cầu gia nhập môn phái và chọn nhánh tu luyện tương ứng!");
				return false;
            }
#endif

            if (nStrengthAssign < 0 || nDexterityAssign < 0 || nVitalityAssign < 0 || nEnergyAssign < 0)
            {
                return false;
            }

            /// Kiểm tra điều kiện
            if (!this.CheckAssignPotential(ref nStrengthAssign, ref nDexterityAssign, ref nVitalityAssign, ref nEnergyAssign))
            {
                return false;
            }

            int nNewRemainPotential = this.m_wRemainPotential - nStrengthAssign - nDexterityAssign - nVitalityAssign - nEnergyAssign;
            if (nNewRemainPotential < 0)
            {
                return false;
            }

            /// SET CHỈ SỐ STR VĨNH VIỄN CỦA NHÂN VẬT
            this.ChangeStrength(nStrengthAssign);
            /// SET CHỈ SỐ DEX VĨNH VIỄN CỦA NHÂN VẬT
            this.ChangeDexterity(nDexterityAssign);
            /// SET CHỈ SỐ VIT VĨNH VIỄN CỦA NHÂN VẬT
            this.ChangeVitality(nVitalityAssign);
            /// SET CHỈ SỐ ENEGN VĨNH VIỄN CỦA NHÂN VẬT
            this.ChangeEnergy(nEnergyAssign);

            return true;
        }

        /// <summary>
        /// Trả về thuộc tính nhân vật cho khung giao diện
        /// </summary>
        /// <returns></returns>
        public RoleAttributes GetRoleAttributes()
        {
            /// ID kỹ năng
            SkillLevelRef nSkill = this.Skills.GetSkillLevelRef(this.LastMainSkillID);

            RoleAttributes roleData = new RoleAttributes();

            roleData.Crit = this.m_CurrentFatallyStrike;
            roleData.Dex = this.GetCurDexterity();
            roleData.Dodge = this.GetCurrentDefend();
            roleData.Hit = this.CurrentAttackRating();
            roleData.Int = this.GetCurEnergy();
            roleData.MoveSpeed = this.GetCurrentRunSpeed();
            roleData.RemainPoint = this.GetRemainPotential();
            roleData.Sta = this.GetCurVitality();
            roleData.Str = this.GetCurStrength();
            roleData.AtkSpeed = this.GetCurrentAttackSpeed();
            roleData.CastSpeed = this.GetCurrentCastSpeed();
            roleData.Def = this.GetCurResist(DAMAGE_TYPE.damage_physics);
            roleData.IceRes = this.GetCurResist(DAMAGE_TYPE.damage_cold);
            roleData.FireRes = this.GetCurResist(DAMAGE_TYPE.damage_fire);
            roleData.LightningRes = this.GetCurResist(DAMAGE_TYPE.damage_light);
            roleData.PoisonRes = this.GetCurResist(DAMAGE_TYPE.damage_poison);

            /// Nếu toác
            if (nSkill == null || nSkill.Level <= 0 || nSkill.Properties == null)
            {
                roleData.Damage = 0;
            }
            else
            {
                try
                {
                    roleData.Damage = AlgorithmProperty.GetDamageInfo(this, nSkill, 0);
                }
                catch (Exception ex)
                {
                    /// Ghi Log
                    LogManager.WriteLog(LogTypes.Exception, ex.ToString());

                    roleData.Damage = 0;
                }
            }

            /// Danh sách thuộc tính khác
            roleData.OtherProperties = new Dictionary<int, int>();
            roleData.OtherProperties[(int)MAGIC_ATTRIB.magic_skilldamageptrim] = this.m_nSkillDamagePTrim;
            roleData.OtherProperties[(int)MAGIC_ATTRIB.magic_skillselfdamagetrim] = this.m_nSkillSelfDamagePTrim;

            roleData.OtherProperties[(int)MAGIC_ATTRIB.magic_rangedamagereturn_v] = this.m_CurrentRangeDmgRet;
            roleData.OtherProperties[(int)MAGIC_ATTRIB.magic_rangedamagereturn_p] = this.m_CurrentRangeDmgRetPercent;
            roleData.OtherProperties[(int)MAGIC_ATTRIB.magic_meleedamagereturn_v] = this.m_CurrentMeleeDmgRet;
            roleData.OtherProperties[(int)MAGIC_ATTRIB.magic_meleedamagereturn_p] = this.m_CurrentMeleeDmgRetPercent;
            roleData.OtherProperties[(int)MAGIC_ATTRIB.magic_poisondamagereturn_v] = this.m_CurrentPoisonDmgRet;
            roleData.OtherProperties[(int)MAGIC_ATTRIB.magic_poisondamagereturn_p] = this.m_CurrentPoisonDmgRetPercent;

            roleData.OtherProperties[(int) MAGIC_ATTRIB.magic_deadlystrikedamageenhance_p] = this.m_DeadlystrikeDamagePercent;
            roleData.OtherProperties[(int) MAGIC_ATTRIB.magic_trice_eff_fatallystrike_p] = this.m_CurrentFatalStrikePercent;

            roleData.OtherProperties[(int)MAGIC_ATTRIB.magic_lifereplenish_p] = this.m_CurrentLifeReplenishPercent;
            roleData.OtherProperties[(int)MAGIC_ATTRIB.magic_manareplenish_p] = this.m_CurrentManaReplenishPercent;
            roleData.OtherProperties[(int)MAGIC_ATTRIB.magic_lucky_v] = this.m_nCurLucky;
            roleData.OtherProperties[(int)MAGIC_ATTRIB.magic_poisontimereduce_p] = this.m_CurrentPoisonTimeReducePercent;

            roleData.OtherProperties[(int)MAGIC_ATTRIB.magic_ignoredefenseenhance_v] = this.m_CurrentIgnoreDefense;
            roleData.OtherProperties[(int)MAGIC_ATTRIB.magic_ignoredefenseenhance_p] = this.m_CurrentIgnoreDefensePercent;

            roleData.OtherProperties[(int)MAGIC_ATTRIB.magic_damage_return_receive_p] = this.m_damage[(int)DAMAGE_TYPE.damage_return].GetCurResist();

            roleData.OtherProperties[(int)MAGIC_ATTRIB.magic_infectpoison] = this.m_nPoisonInfect;

            //roleData.OtherProperties[(int)MAGIC_ATTRIB.state_hurt_attacktime] = this.m_state[(int)KE_STATE.emSTATE_HURT].StateAddTime;
            //roleData.OtherProperties[(int)MAGIC_ATTRIB.state_hurt_attackrate] = this.m_state[(int)KE_STATE.emSTATE_HURT].StateAddRate;
            //roleData.OtherProperties[(int)MAGIC_ATTRIB.state_hurt_resisttime] = -this.m_state[(int)KE_STATE.emSTATE_HURT].StateRestTime;
            //roleData.OtherProperties[(int)MAGIC_ATTRIB.state_hurt_resistrate] = -this.m_state[(int)KE_STATE.emSTATE_HURT].StateRestRate;

            //roleData.OtherProperties[(int)MAGIC_ATTRIB.state_weak_attacktime] = this.m_state[(int)KE_STATE.emSTATE_WEAK].StateAddTime;
            //roleData.OtherProperties[(int)MAGIC_ATTRIB.state_weak_attackrate] = this.m_state[(int)KE_STATE.emSTATE_WEAK].StateAddRate;
            //roleData.OtherProperties[(int)MAGIC_ATTRIB.state_weak_resisttime] = -this.m_state[(int)KE_STATE.emSTATE_WEAK].StateRestTime;
            //roleData.OtherProperties[(int)MAGIC_ATTRIB.state_weak_resistrate] = -this.m_state[(int)KE_STATE.emSTATE_WEAK].StateRestRate;

            //roleData.OtherProperties[(int)MAGIC_ATTRIB.state_slowall_attacktime] = this.m_state[(int)KE_STATE.emSTATE_SLOWALL].StateAddTime;
            //roleData.OtherProperties[(int)MAGIC_ATTRIB.state_slowall_attackrate] = this.m_state[(int)KE_STATE.emSTATE_SLOWALL].StateAddRate;
            //roleData.OtherProperties[(int)MAGIC_ATTRIB.state_slowall_resisttime] = -this.m_state[(int)KE_STATE.emSTATE_SLOWALL].StateRestTime;
            //roleData.OtherProperties[(int)MAGIC_ATTRIB.state_slowall_resistrate] = -this.m_state[(int)KE_STATE.emSTATE_SLOWALL].StateRestRate;

            //roleData.OtherProperties[(int)MAGIC_ATTRIB.state_burn_attacktime] = this.m_state[(int)KE_STATE.emSTATE_BURN].StateAddTime;
            //roleData.OtherProperties[(int)MAGIC_ATTRIB.state_burn_attackrate] = this.m_state[(int)KE_STATE.emSTATE_BURN].StateAddRate;
            //roleData.OtherProperties[(int)MAGIC_ATTRIB.state_burn_resisttime] = -this.m_state[(int)KE_STATE.emSTATE_BURN].StateRestTime;
            //roleData.OtherProperties[(int)MAGIC_ATTRIB.state_burn_resistrate] = -this.m_state[(int)KE_STATE.emSTATE_BURN].StateRestRate;

            //roleData.OtherProperties[(int)MAGIC_ATTRIB.state_stun_attacktime] = this.m_state[(int)KE_STATE.emSTATE_STUN].StateAddTime;
            //roleData.OtherProperties[(int)MAGIC_ATTRIB.state_stun_attackrate] = this.m_state[(int)KE_STATE.emSTATE_STUN].StateAddRate;
            //roleData.OtherProperties[(int)MAGIC_ATTRIB.state_stun_resisttime] = -this.m_state[(int)KE_STATE.emSTATE_STUN].StateRestTime;
            //roleData.OtherProperties[(int)MAGIC_ATTRIB.state_stun_resistrate] = -this.m_state[(int)KE_STATE.emSTATE_STUN].StateRestRate;

            //roleData.OtherProperties[(int)MAGIC_ATTRIB.state_fixed_attacktime] = this.m_state[(int)KE_STATE.emSTATE_FIXED].StateAddTime;
            //roleData.OtherProperties[(int)MAGIC_ATTRIB.state_fixed_attackrate] = this.m_state[(int)KE_STATE.emSTATE_FIXED].StateAddRate;
            //roleData.OtherProperties[(int)MAGIC_ATTRIB.state_fixed_resisttime] = -this.m_state[(int)KE_STATE.emSTATE_FIXED].StateRestTime;
            //roleData.OtherProperties[(int)MAGIC_ATTRIB.state_fixed_resistrate] = -this.m_state[(int)KE_STATE.emSTATE_FIXED].StateRestRate;

            //roleData.OtherProperties[(int)MAGIC_ATTRIB.state_palsy_attacktime] = this.m_state[(int)KE_STATE.emSTATE_PALSY].StateAddTime;
            //roleData.OtherProperties[(int)MAGIC_ATTRIB.state_palsy_attackrate] = this.m_state[(int)KE_STATE.emSTATE_PALSY].StateAddRate;
            //roleData.OtherProperties[(int)MAGIC_ATTRIB.state_palsy_resisttime] = -this.m_state[(int)KE_STATE.emSTATE_PALSY].StateRestTime;
            //roleData.OtherProperties[(int)MAGIC_ATTRIB.state_palsy_resistrate] = -this.m_state[(int)KE_STATE.emSTATE_PALSY].StateRestRate;

            //roleData.OtherProperties[(int)MAGIC_ATTRIB.state_slowrun_attacktime] = this.m_state[(int)KE_STATE.emSTATE_SLOWRUN].StateAddTime;
            //roleData.OtherProperties[(int)MAGIC_ATTRIB.state_slowrun_attackrate] = this.m_state[(int)KE_STATE.emSTATE_SLOWRUN].StateAddRate;
            //roleData.OtherProperties[(int)MAGIC_ATTRIB.state_slowrun_resisttime] = -this.m_state[(int)KE_STATE.emSTATE_SLOWRUN].StateRestTime;
            //roleData.OtherProperties[(int)MAGIC_ATTRIB.state_slowrun_resistrate] = -this.m_state[(int)KE_STATE.emSTATE_SLOWRUN].StateRestRate;

            //roleData.OtherProperties[(int)MAGIC_ATTRIB.state_freeze_attacktime] = this.m_state[(int)KE_STATE.emSTATE_FREEZE].StateAddTime;
            //roleData.OtherProperties[(int)MAGIC_ATTRIB.state_freeze_attackrate] = this.m_state[(int)KE_STATE.emSTATE_FREEZE].StateAddRate;
            //roleData.OtherProperties[(int)MAGIC_ATTRIB.state_freeze_resisttime] = -this.m_state[(int)KE_STATE.emSTATE_FREEZE].StateRestTime;
            //roleData.OtherProperties[(int)MAGIC_ATTRIB.state_freeze_resistrate] = -this.m_state[(int)KE_STATE.emSTATE_FREEZE].StateRestRate;

            //roleData.OtherProperties[(int)MAGIC_ATTRIB.state_confuse_attacktime] = this.m_state[(int)KE_STATE.emSTATE_CONFUSE].StateAddTime;
            //roleData.OtherProperties[(int)MAGIC_ATTRIB.state_confuse_attackrate] = this.m_state[(int)KE_STATE.emSTATE_CONFUSE].StateAddRate;
            //roleData.OtherProperties[(int)MAGIC_ATTRIB.state_confuse_resisttime] = -this.m_state[(int)KE_STATE.emSTATE_CONFUSE].StateRestTime;
            //roleData.OtherProperties[(int)MAGIC_ATTRIB.state_confuse_resistrate] = -this.m_state[(int)KE_STATE.emSTATE_CONFUSE].StateRestRate;

            //roleData.OtherProperties[(int)MAGIC_ATTRIB.state_knock_attacktime] = this.m_state[(int)KE_STATE.emSTATE_KNOCK].StateAddTime;
            //roleData.OtherProperties[(int)MAGIC_ATTRIB.state_knock_attackrate] = this.m_state[(int)KE_STATE.emSTATE_KNOCK].StateAddRate;
            //roleData.OtherProperties[(int)MAGIC_ATTRIB.state_knock_resisttime] = -this.m_state[(int)KE_STATE.emSTATE_KNOCK].StateRestTime;
            //roleData.OtherProperties[(int)MAGIC_ATTRIB.state_knock_resistrate] = -this.m_state[(int)KE_STATE.emSTATE_KNOCK].StateRestRate;

            //roleData.OtherProperties[(int)MAGIC_ATTRIB.state_silence_attacktime] = this.m_state[(int)KE_STATE.emSTATE_SILENCE].StateAddTime;
            //roleData.OtherProperties[(int)MAGIC_ATTRIB.state_silence_attackrate] = this.m_state[(int)KE_STATE.emSTATE_SILENCE].StateAddRate;
            //roleData.OtherProperties[(int)MAGIC_ATTRIB.state_silence_resisttime] = -this.m_state[(int)KE_STATE.emSTATE_SILENCE].StateRestTime;
            //roleData.OtherProperties[(int)MAGIC_ATTRIB.state_silence_resistrate] = -this.m_state[(int)KE_STATE.emSTATE_SILENCE].StateRestRate;

            //roleData.OtherProperties[(int)MAGIC_ATTRIB.state_drag_attacktime] = this.m_state[(int)KE_STATE.emSTATE_DRAG].StateAddTime;
            //roleData.OtherProperties[(int)MAGIC_ATTRIB.state_drag_attackrate] = this.m_state[(int)KE_STATE.emSTATE_DRAG].StateAddRate;
            //roleData.OtherProperties[(int)MAGIC_ATTRIB.state_drag_resisttime] = -this.m_state[(int)KE_STATE.emSTATE_DRAG].StateRestTime;
            //roleData.OtherProperties[(int)MAGIC_ATTRIB.state_drag_resistrate] = -this.m_state[(int)KE_STATE.emSTATE_DRAG].StateRestRate;

            return roleData;
        }

        /// <summary>
        /// Tẩy điểm tiềm năng
        /// </summary>
        public void UnAssignPotential()
        {
            /// Tổng điểm tiềm năng được cộng thêm từ các loại bánh đã ăn
            int additionRemainPoint = this.m_nBonusRemainPotential;
            /// Tổng điểm tiềm năng có từ Base (đoạn này cần tính lại vì nhỡ may nó có dùng lệnh thay đổi cấp độ)
            int baseRemainPoint = KPlayerSetting.GetLevelPotential(this.m_Level);

            /// Xóa toàn bộ điểm đã phân phối trước đó
            this.ChangeStrength(-this.m_wStrength);
            this.ChangeDexterity(-this.m_wDexterity);
            this.ChangeVitality(-this.m_wVitality);
            this.ChangeEnergy(-this.m_wEnergy);
        }

        /// <summary>
        /// Thay đổi giá trị Sức cơ bản của nhân vật
        /// HÀM NÀY CHỈ GỌI KHI NHÂN VẬT + ĐIỂM TIỀN NĂNG HOẶC NHÂN VẬT TẨY ĐIỂM
        /// </summary>
        /// <param name="nValueChange"></param>
        public void ChangeStrength(int nValueChange)
        {
            if (nValueChange == 0)
            {
                return;
            }

            if (nValueChange < 0 && -nValueChange > this.m_wStrength)
            {
                nValueChange = -this.m_wStrength;
            }

            this.m_wStrength += nValueChange;

            this.ChangeCurStrength(nValueChange);

            // LƯU LẠI CHỈ SỐ STR HIỆN TẠI VÀO DB
            Global.SaveRoleParamsInt32ValueToDB(this, RoleParamName.sPropStrength, this.m_wStrength, true);
        }

        /// <summary>
        /// Thay đổi giá trị Thân cơ bản của nhân vật
        /// HÀM NÀY CHỈ GỌI KHI NHÂN VẬT + ĐIỂM TIỀN NĂNG HOẶC NHÂN VẬT TẨY ĐIỂM
        /// </summary>
        /// <param name="nValueChange"></param>
        public void ChangeDexterity(int nValueChange)
        {
            if (nValueChange == 0)
            {
                return;
            }

            if (nValueChange < 0 && -nValueChange > this.m_wDexterity)
            {
                nValueChange = -this.m_wDexterity;
            }

            this.m_wDexterity += nValueChange;

            this.ChangeCurDexterity(nValueChange);

            // LƯU LẠI CHỈ SỐ DEV HIỆN TẠI VÀO DB
            Global.SaveRoleParamsInt32ValueToDB(this, RoleParamName.sPropDexterity, this.m_wDexterity, true);
        }

        /// <summary>
        /// Thay đổi giá trị Ngoại cơ bản của nhân vật
        ///  HÀM NÀY CHỈ GỌI KHI NHÂN VẬT + ĐIỂM TIỀN NĂNG HOẶC NHÂN VẬT TẨY ĐIỂM
        /// </summary>
        /// <param name="nValueChange"></param>
        public void ChangeVitality(int nValueChange)
        {
            if (nValueChange == 0)
            {
                return;
            }

            if (nValueChange < 0 && -nValueChange > this.m_wVitality)
            {
                nValueChange = -this.m_wVitality;
            }

            this.m_wVitality += nValueChange;

            this.ChangeCurVitality(nValueChange);
            // LƯU LẠI CHỈ SỐ THỂ HIỆN TẠI VÀO DB
            Global.SaveRoleParamsInt32ValueToDB(this, RoleParamName.sPropConstitution, this.m_wVitality, true);
        }

        /// <summary>
        /// Thay đổi giá trị Nội cơ bản của nhân vật
        ///  HÀM NÀY CHỈ GỌI KHI NHÂN VẬT + ĐIỂM TIỀN NĂNG HOẶC NHÂN VẬT TẨY ĐIỂM
        /// </summary>
        /// <param name="nValueChange"></param>
        public void ChangeEnergy(int nValueChange)
        {
            if (nValueChange == 0)
            {
                return;
            }

            if (nValueChange < 0 && -nValueChange > this.m_wEnergy)
            {
                nValueChange = -this.m_wEnergy;
            }

            this.m_wEnergy += nValueChange;

            this.ChangeCurEnergy(nValueChange);

            // LƯU LẠI CHỈ SỐ ENERGY VÀO DB
            Global.SaveRoleParamsInt32ValueToDB(this, RoleParamName.sPropIntelligence, this.m_wEnergy, true);
        }

        /// <summary>
        /// Thay đổi giá trị Sức hiện tại của nhân vật
        /// HÀM NÀY GỌI KHI ATTACK CÁC BUFF TẠM THỜI HOẶC STR TEMP TỪ BASESETTING=> HOẶC KHI NHÂN VẬT ĐƯỢC NHẬN STR TẠM THỜI
        /// TOÀN BỘ GIÁ TRỊ NÀY SẼ ĐƯỢC RESET KHI NHÂN VẬT LOGINOUT. VÀ RECALCULATION KHI NHÂN VẬT LOGIN
        /// </summary>
        /// <param name="nValueChange"></param>
        public void ChangeCurStrength(int nValueChange)
        {
            if (nValueChange == 0)
            {
                return;
            }

            int nCurStrengthNew = this.m_wCurStrength + nValueChange;
            if (nCurStrengthNew < 0)
            {
                nCurStrengthNew = 0;
            }

            int nDamagePhysicsChange = KPlayerSetting.GetStrength2DamagePhysics(this.m_cPlayerFaction.GetFactionId(), nCurStrengthNew) -
                                       KPlayerSetting.GetStrength2DamagePhysics(this.m_cPlayerFaction.GetFactionId(), this.m_wCurStrength);
            this.ChangePhysicsDamage(nDamagePhysicsChange);

            this.m_wCurStrength = nCurStrengthNew;
        }

        /// <summary>
        /// Thay đổi giá trị Thân hiện tại của nhân vật
        /// HÀM NÀY GỌI KHI ATTACK CÁC BUFF TẠM THỜI HOẶC Dexterity TEMP TỪ BASESETTING=> HOẶC KHI NHÂN VẬT ĐƯỢC NHẬN Dexterity TẠM THỜI
        /// TOÀN BỘ GIÁ TRỊ NÀY SẼ ĐƯỢC RESET KHI NHÂN VẬT LOGINOUT. VÀ RECALCULATION KHI NHÂN VẬT LOGIN
        /// </summary>
        /// <param name="nValueChange"></param>
        public void ChangeCurDexterity(int nValueChange)
        {
            if (nValueChange == 0)
            {
                return;
            }

            int nCurDexterityNew = this.m_wCurDexterity + nValueChange;
            if (nCurDexterityNew < 0)
            {
                nCurDexterityNew = 0;
            }

            int nAttackRateChange = KPlayerSetting.GetDexterity2AttackRate(this.m_cPlayerFaction.GetFactionId(), nCurDexterityNew) -
                                    KPlayerSetting.GetDexterity2AttackRate(this.m_cPlayerFaction.GetFactionId(), this.m_wCurDexterity);
            this.ChangeAttackRating(nAttackRateChange, 0, 0);

            int nDefenceChange = KPlayerSetting.GetDexterity2Defence(this.m_cPlayerFaction.GetFactionId(), nCurDexterityNew) -
                                 KPlayerSetting.GetDexterity2Defence(this.m_cPlayerFaction.GetFactionId(), this.m_wCurDexterity);
            this.ChangeDefend(nDefenceChange, 0, 0);

            int nDamagePhysicsChange = KPlayerSetting.GetDexterity2DamagePhysics(this.m_cPlayerFaction.GetFactionId(), nCurDexterityNew) -
                                       KPlayerSetting.GetDexterity2DamagePhysics(this.m_cPlayerFaction.GetFactionId(), this.m_wCurDexterity);
            this.ChangePhysicsDamage(nDamagePhysicsChange);

            this.m_wCurDexterity = nCurDexterityNew;
        }

        /// <summary>
        /// Thay đổi giá trị Ngoại hiện tại của nhân vật
        /// HÀM NÀY GỌI KHI ATTACK CÁC BUFF TẠM THỜI HOẶC Vitality TEMP TỪ BASESETTING=> HOẶC KHI NHÂN VẬT ĐƯỢC NHẬN Vitality TẠM THỜI
        /// TOÀN BỘ GIÁ TRỊ NÀY SẼ ĐƯỢC RESET KHI NHÂN VẬT LOGINOUT. VÀ RECALCULATION KHI NHÂN VẬT LOGIN
        /// </summary>
        /// <param name="nValueChange"></param>
        public void ChangeCurVitality(int nValueChange)
        {
            if (nValueChange == 0)
            {
                return;
            }

            int nCurVitalityNew = this.m_wCurVitality + nValueChange;
            if (nCurVitalityNew < 0)
            {
                nCurVitalityNew = 0;
            }

            int nLifeChange = KPlayerSetting.GetVitality2Life(this.m_cPlayerFaction.GetFactionId(), nCurVitalityNew) -
                              KPlayerSetting.GetVitality2Life(this.m_cPlayerFaction.GetFactionId(), this.m_wCurVitality);
            this.ChangeLifeMax(nLifeChange, 0, 0);

            this.m_wCurVitality = nCurVitalityNew;
        }

        /// <summary>
        /// Thay đổi giá trị Nội hiện tại của nhân vật
        /// HÀM NÀY GỌI KHI ATTACK CÁC BUFF TẠM THỜI HOẶC CurEnergy TEMP TỪ BASESETTING=> HOẶC KHI NHÂN VẬT ĐƯỢC NHẬN CurEnergy TẠM THỜI
        /// TOÀN BỘ GIÁ TRỊ NÀY SẼ ĐƯỢC RESET KHI NHÂN VẬT LOGINOUT. VÀ RECALCULATION KHI NHÂN VẬT LOGIN
        /// </summary>
        /// <param name="nValueChange"></param>
        public void ChangeCurEnergy(int nValueChange)
        {
            if (nValueChange == 0)
            {
                return;
            }

            int nCurEnergyNew = this.m_wCurEnergy + nValueChange;
            if (nCurEnergyNew < 0)
            {
                nCurEnergyNew = 0;
            }

            int nManaChange = KPlayerSetting.GetEnergy2Mana(this.m_cPlayerFaction.GetFactionId(), nCurEnergyNew) -
                              KPlayerSetting.GetEnergy2Mana(this.m_cPlayerFaction.GetFactionId(), this.m_wCurEnergy);
            this.ChangeManaMax(nManaChange, 0, 0);

            int nDamageMagicChange = KPlayerSetting.GetEnergy2DamageMagic(this.m_cPlayerFaction.GetFactionId(), nCurEnergyNew) -
                                     KPlayerSetting.GetEnergy2DamageMagic(this.m_cPlayerFaction.GetFactionId(), this.m_wCurEnergy);
            this.ChangeMagicDamage(nDamageMagicChange);

            this.m_wCurEnergy = nCurEnergyNew;
        }

        /// <summary>
        /// Thay đổi giá trị kinh nghiệm tăng thêm
        /// </summary>
        /// <param name="nMin"></param>
        /// <param name="nMax"></param>
        public void ChangeExpEnhanceV(int nMin, int nMax)
        {
        }

        /// <summary>
        /// Thay đổi giá trị % kinh nghiệm nhận được
        /// </summary>
        /// <param name="nPercent"></param>
        public void ChangeExpEnhanceP(int nPercent)
        {
            this.m_nExpEnhancePercent += nPercent;
        }

        /// <summary>
        /// Thay đổi giá trị % kinh nghiệm nhận được của kỹ năng cấp 120
        /// </summary>
        /// <param name="nPercent"></param>
        public void Change120SKillExpEnhanceP(int nPercent)
        {
            this.m_n120SkillExpEnhancePercent += nPercent;
        }

        /// <summary>
        /// Đồng bộ giá trị hoạt lực về Client
        /// </summary>
        public void SyncGatherAndMakePoint()
        {
            KT_TCPHandler.NotifySelfGatherMakePointChanged(this);
        }

        /// <summary>
        /// Thay đổi giá trị hoạt lực hiện tại
        /// </summary>
        /// <param name="nDelta"></param>
        public void ChangeCurGatherPoint(int nDelta)
        {
            int OLDVALUE = this.m_dwGatherPoint;

            this.m_dwGatherPoint += nDelta;
            if (this.m_dwGatherPoint < 0)
            {
                this.m_dwGatherPoint = 0;
            }
            if (this.m_dwGatherPoint > KPlayer.NUM_20E)
            {
                this.m_dwGatherPoint = KPlayer.NUM_20E;
            }

            LogManager.WriteLog(LogTypes.LifeSkill, "[" + this.RoleID + "][" + this.RoleName + "] Giá trị hoạt lực thay đổi :" + OLDVALUE + "==>" + this.m_dwMakePoint + "|Biến động :" + nDelta);

            this.SyncGatherAndMakePoint();
        }

        /// <summary>
        /// Thay đổi giá trị hoạt lực hiện tại
        /// </summary>
        /// <param name="nDelta"></param>
        /// <returns></returns>
        public void ChangeCurMakePoint(int nDelta)
        {
            int OLDVALUE = this.m_dwMakePoint;

            this.m_dwMakePoint += nDelta;
            if (this.m_dwMakePoint < 0)
            {
                this.m_dwMakePoint = 0;
            }
            if (this.m_dwMakePoint > KPlayer.NUM_20E)
            {
                this.m_dwMakePoint = KPlayer.NUM_20E;
            }

            LogManager.WriteLog(LogTypes.LifeSkill, "[" + this.RoleID + "][" + this.RoleName + "] Giá trị tinh lực thay đổi :" + OLDVALUE + "==>" + this.m_dwMakePoint + "|Biến động :" + nDelta);

            this.SyncGatherAndMakePoint();
        }

        #endregion Public Methods
    }
}