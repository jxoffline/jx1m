using GameServer.KiemThe.Entities;
using GameServer.KiemThe.Logic;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace GameServer.Logic
{
	/// <summary>
	/// Thuộc tính cơ bản của đối tượng
	/// </summary>
	public partial class GameObject
	{
		#region Define
		private int _m_LifeMax;
		/// <summary>
		/// Sinh lực tối đa cơ bản
		/// </summary>
		public int m_LifeMax
		{
			get
			{
				return this._m_LifeMax;
			}
			private set
			{
				lock (this)
				{
					this._m_LifeMax = value;
				}
			}
		}

		private int _m_LifeMaxAddP;
		/// <summary>
		/// % sinh lực tối đa cơ bản
		/// </summary>
		public int m_LifeMaxAddP
		{
			get
			{
				return this._m_LifeMaxAddP;
			}
			private set
			{
				lock (this)
				{
					this._m_LifeMaxAddP = value;
				}
			}
		}

		private int _m_CurrentLifeMax;
		/// <summary>
		/// Sinh lực tối đa hiện tại
		/// </summary>
		public int m_CurrentLifeMax
		{
			get
			{
				lock (this)
				{
					return this._m_CurrentLifeMax;
				}
			}
			private set
			{
				lock (this)
				{
					this._m_CurrentLifeMax = value;
				}
			}
		}

		private int _m_CurrentLife;
		/// <summary>
		/// Sinh lực hiện tại
		/// </summary>
		public int m_CurrentLife
		{
			get
			{
				lock (this)
				{
					return this._m_CurrentLife;
				}
			}
			set
			{
				lock (this)
                {
					this._m_CurrentLife = value;
                }
			}
		}

		private int _m_LifeReplenish;
		/// <summary>
		/// Phục hồi sinh lực mỗi 5 giây cơ bản
		/// </summary>
		public int m_LifeReplenish
		{
			get
			{
				lock (this)
				{
					if (this._m_LifeReplenish < 0)
					{
						this._m_LifeReplenish = 0;
					}
					return this._m_LifeReplenish;
				}
			}
			private set
			{
				lock (this)
				{
					if (value < 0)
					{
						value = 0;
					}
					this._m_LifeReplenish = value;
				}
			}
		}

		private int _m_CurrentLifeReplenish;
		/// <summary>
		/// Phục hồi sinh lực mỗi 5 giây hiện tại (nếu dương thì do thuốc và vật phẩm hỗ trợ, âm thì do bùa chú bất lợi)
		/// </summary>
		public int m_CurrentLifeReplenish
		{
			get
			{
				lock (this)
				{
					if (this._m_CurrentLifeReplenish < 0)
					{
						this._m_CurrentLifeReplenish = 0;
					}
					return this._m_CurrentLifeReplenish;
				}
			}
			set
			{
				lock (this)
				{
					if (value < 0)
					{
						value = 0;
					}
					this._m_CurrentLifeReplenish = value;
				}
			}
		}

		private int _m_CurrentLifeReplenishPercent;
		/// <summary>
		/// Hiệu suất phục hồi sinh lực hiện tại (100% là giá trị khởi đầu)
		/// </summary>
		public int m_CurrentLifeReplenishPercent
		{
			get
			{
				lock (this)
				{
					if (this._m_CurrentLifeReplenishPercent < 0)
					{
						this._m_CurrentLifeReplenishPercent = 0;
					}
					return this._m_CurrentLifeReplenishPercent;
				}
			}
			set
			{
				lock (this)
				{
					if (value < 0)
					{
						value = 0;
					}
					this._m_CurrentLifeReplenishPercent = value;
				}
			}
		}

		private int _m_CurrentLifeFastReplenish;
		/// <summary>
		/// Phục hồi sinh lực mỗi nửa giây hiện tại
		/// </summary>
		public int m_CurrentLifeFastReplenish
		{
			get
			{
				lock (this)
				{
					if (this._m_CurrentLifeFastReplenish < 0)
					{
						this._m_CurrentLifeFastReplenish = 0;
					}
					return this._m_CurrentLifeFastReplenish;
				}
			}
			set
			{
				lock (this)
				{
					if (value < 0)
					{
						value = 0;
					}
					this._m_CurrentLifeFastReplenish = value;
				}
			}
		}

		private int _m_ManaMax;
		/// <summary>
		/// Nội lực tối đa cơ bản
		/// </summary>
		public int m_ManaMax
		{
			get
			{
				return this._m_ManaMax;
			}
			private set
			{
				lock (this)
				{
					this._m_ManaMax = value;
				}
			}
		}

		private int _m_ManaMaxAddP;
		/// <summary>
		/// % nội lực tối đa
		/// </summary>
		public int m_ManaMaxAddP
		{
			get
			{
				return this._m_ManaMaxAddP;
			}
			private set
			{
				lock (this)
				{
					this._m_ManaMaxAddP = value;
				}
			}
		}

		private int _m_CurrentManaMax;
		/// <summary>
		/// Nội lực tối đa hiện tại (sau khi đã bao gồm trang bị, Buff và kỹ năng hỗ trợ)
		/// </summary>
		public int m_CurrentManaMax
		{
			get
			{
				lock (this)
				{
					return this._m_CurrentManaMax;
				}
			}
			private set
			{
				lock (this)
				{
					this._m_CurrentManaMax = value;
				}
			}
		}

		private int _m_CurrentMana;
		/// <summary>
		/// Nội lực hiện tại
		/// </summary>
		public int m_CurrentMana
		{
			get
			{
				lock (this)
				{
					return this._m_CurrentMana;
				}
			}
			set
			{
				lock (this)
				{
					this._m_CurrentMana = value;
				}
			}
		}

		private int _m_ManaReplenish;
		/// <summary>
		/// Phục hồi nội lực mỗi 5 giây cơ bản
		/// </summary>
		public int m_ManaReplenish
		{
			get
			{
				return this._m_ManaReplenish;
			}
			private set
			{
				lock (this)
				{
					this._m_ManaReplenish = value;
				}
			}
		}

		private int _m_CurrentManaReplenish;
		/// <summary>
		/// Phục hồi nội lực mỗi 5 giây hiện tại
		/// </summary>
		public int m_CurrentManaReplenish
		{
			get
			{
				return this._m_CurrentManaReplenish;
			}
			set
			{
				lock (this)
				{
					this._m_CurrentManaReplenish = value;
				}
			}
		}

		private int _m_CurrentManaReplenishPercent;
		/// <summary>
		/// Hiệu suất phục hồi nội lực hiện tại
		/// </summary>
		public int m_CurrentManaReplenishPercent
		{
			get
			{
				return this._m_CurrentManaReplenishPercent;
			}
			set
			{
				lock (this)
				{
					this._m_CurrentManaReplenishPercent = value;
				}
			}
		}

		private int _m_CurrentManaFastReplenish;
		/// <summary>
		/// Phục hồi nội lực mỗi nửa giây
		/// </summary>
		public int m_CurrentManaFastReplenish
		{
			get
			{
				return this._m_CurrentManaFastReplenish;
			}
			set
			{
				lock (this)
				{
					this._m_CurrentManaFastReplenish = value;
				}
			}
		}

		private int _m_StaminaMax;
		/// <summary>
		/// Thể lực tối đa cơ bản
		/// </summary>
		public int m_StaminaMax
		{
			get
			{
				return this._m_StaminaMax;
			}
			private set
			{
				lock (this)
				{
					this._m_StaminaMax = value;
				}
			}
		}

		private int _m_StaminaMaxAddP;
		/// <summary>
		/// % thể lực tối đa
		/// </summary>
		public int m_StaminaMaxAddP
		{
			get
			{
				return this._m_StaminaMaxAddP;
			}
			private set
			{
				lock (this)
				{
					this._m_StaminaMaxAddP = value;
				}
			}
		}

		private int _m_CurrentStaminaMax;
		/// <summary>
		/// Thể lực tối đa hiện tại (sau khi đã bao gồm trang bị, Buff và kỹ năng hỗ trợ)
		/// </summary>
		public int m_CurrentStaminaMax
		{
			get
			{
				lock (this)
				{
					return this._m_CurrentStaminaMax;
				}
			}
			private set
			{
				lock (this)
				{
					this._m_CurrentStaminaMax = value;
				}
			}
		}

		private int _m_CurrentStamina;
		/// <summary>
		/// Thể lực hiện tại
		/// </summary>
		public int m_CurrentStamina
		{
			get
			{
				lock (this)
				{
					return this._m_CurrentStamina;
				}
			}
			set
			{
				lock (this)
				{
					this._m_CurrentStamina = value;
				}
			}
		}

		private int _m_StaminaReplenish;
		/// <summary>
		/// Phục hồi thể lực mỗi 5 giây cơ bản
		/// </summary>
		public int m_StaminaReplenish
		{
			get
			{
				return this._m_StaminaReplenish;
			}
			private set
			{
				lock (this)
				{
					this._m_StaminaReplenish = value;
				}
			}
		}

		private int _m_CurrentStaminaReplenish;
		/// <summary>
		/// Phục hồi thể lực mỗi 5 giây hiện tại
		/// </summary>
		public int m_CurrentStaminaReplenish
		{
			get
			{
				return this._m_CurrentStaminaReplenish;
			}
			set
			{
				lock (this)
				{
					this._m_CurrentStaminaReplenish = value;
				}
			}
		}

		private int _m_CurrentStaminaReplenishPercent;
		/// <summary>
		/// Hiệu suất phục hồi thể lực hiện tại (100% là giá trị khởi đầu)
		/// </summary>
		public int m_CurrentStaminaReplenishPercent
		{
			get
			{
				return this._m_CurrentStaminaReplenishPercent;
			}
			private set
			{
				lock (this)
				{
					this._m_CurrentStaminaReplenishPercent = value;
				}
			}
		}

		private int _m_CurrentFastStaminaReplenish;
		/// <summary>
		/// Phục hồi thể lực mỗi nửa giây hiện tại
		/// </summary>
		public int m_CurrentFastStaminaReplenish
		{
			get
			{
				return this._m_CurrentFastStaminaReplenish;
			}
			set
			{
				lock (this)
				{
					this._m_CurrentFastStaminaReplenish = value;
				}
			}
		}

		private KMagicAttrib _m_PhysicsDamage = new KMagicAttrib();
		/// <summary>
		/// Vật công ngoại hiện tại
		/// <para>Value[0]: Giá trị Min</para>
		/// <para>Value[1]: Vô nghĩa</para>
		/// <para>Value[2]: Giá trị Max</para>
		/// </summary>
		public KMagicAttrib m_PhysicsDamage
		{
			get
			{
				return this._m_PhysicsDamage;
			}
			private set
			{
				lock (this)
				{
					this._m_PhysicsDamage = value;
				}
			}
		}

		/// <summary>
		/// Vật công ngoại hiện tại từ Vũ khí
		/// <para>Value[0]: Giá trị Min</para>
		/// <para>Value[1]: Vô nghĩa</para>
		/// <para>Value[2]: Giá trị Max</para>
		/// </summary>
		public KMagicAttrib m_PhysicPhysic { get; set; } = new KMagicAttrib();

		/// <summary>
		/// Vật công nội hiện tại từ Vũ khí
		/// <para>Value[0]: Giá trị Min</para>
		/// <para>Value[1]: Vô nghĩa</para>
		/// <para>Value[2]: Giá trị Max</para>
		/// </summary>
		public KMagicAttrib m_PhysicsMagic { get; set; } = new KMagicAttrib();

		/// <summary>
		/// Độc công ngoại hiện tại
		/// </summary>
		public KMagicAttrib m_CurrentPoisonDamage { get; set; } = new KMagicAttrib();

		private KMagicAttrib _m_MagicDamage = new KMagicAttrib();
		/// <summary>
		/// Vật công nội hiện tại
		/// <para>Value[0]: Giá trị Min</para>
		/// <para>Value[1]: Vô nghĩa</para>
		/// <para>Value[2]: Giá trị Max</para>
		/// </summary>
		public KMagicAttrib m_MagicDamage
        {
			get
            {
				return this._m_MagicDamage;
            }
			private set
            {
				lock (this)
                {
					this._m_MagicDamage = value;
                }
            }
        }

		/// <summary>
		/// Sát thương vật công nội
		/// <para>Value[0]: Giá trị Min</para>
		/// <para>Value[1]: Vô nghĩa</para>
		/// <para>Value[2]: Giá trị Max</para>
		/// </summary>
		public KMagicAttrib m_MagicPhysicsDamage { get; set; } = new KMagicAttrib();
		/// <summary>
		/// Độc công nội hiện tại
		/// </summary>
		public KMagicAttrib m_MagicPoisonDamage { get; set; } = new KMagicAttrib();

		private int _m_AttackRating;
		/// <summary>
		/// Chính xác cơ bản
		/// </summary>
		public int m_AttackRating
		{
			get
			{
				return this._m_AttackRating;
			}
			private set
			{
				lock (this)
				{
					this._m_AttackRating = value;
				}
			}
		}

		private int _m_AttackRatingAddP;
		/// <summary>
		/// % chính xác
		/// </summary>
		public int m_AttackRatingAddP
		{
			get
			{
				return this._m_AttackRatingAddP;
			}
			private set
			{
				lock (this)
				{
					this._m_AttackRatingAddP = value;
				}
			}
		}

		private int _m_CurrentAttackRating;
		/// <summary>
		/// Chính xác hiện tại
		/// </summary>
		public int m_CurrentAttackRating
		{
			get
			{
				return this._m_CurrentAttackRating;
			}
			private set
			{
				lock (this)
				{
					this._m_CurrentAttackRating = value;
				}
			}
		}

		private int _m_Defend;
		/// <summary>
		/// Né tránh
		/// </summary>
		public int m_Defend
		{
			get
			{
				return this._m_Defend;
			}
			private set
			{
				lock (this)
				{
					this._m_Defend = value;
				}
			}
		}

		private int _m_DefendAddP;
		/// <summary>
		/// % né tránh
		/// </summary>
		public int m_DefendAddP
		{
			get
			{
				return this._m_DefendAddP;
			}
			private set
			{
				lock (this)
				{
					this._m_DefendAddP = value;
				}
			}
		}

		private int _m_CurrentDefend;
		/// <summary>
		/// Né tránh hiện tại
		/// </summary>
		public int m_CurrentDefend
		{
			get
			{
				return this._m_CurrentDefend;
			}
			private set
			{
				lock (this)
				{
					this._m_CurrentDefend = value;
				}
			}
		}

        #region Tốc chạy
        private int _m_RunSpeed;
		/// <summary>
		/// Tốc chạy
		/// </summary>
		public int m_RunSpeed
		{
			get
			{
				return this._m_RunSpeed;
			}
			private set
			{
				lock (this)
				{
					this._m_RunSpeed = value;
				}
			}
		}

		private int _m_CurrentRunSpeed;
		/// <summary>
		/// Tốc chạy hiện tại
		/// </summary>
		private int m_CurrentRunSpeed
		{
			get
			{
				return this._m_CurrentRunSpeed;
			}
			set
			{
				lock (this)
				{
					this._m_CurrentRunSpeed = value;
				}
			}
		}

		private int _m_RunSpeedAddP;
		/// <summary>
		/// Tăng % tốc chạy
		/// </summary>
		public int m_RunSpeedAddP
		{
			get
			{
				return this._m_RunSpeedAddP;
			}
			private set
			{
				lock (this)
				{
					this._m_RunSpeedAddP = value;
				}
			}
		}

		private int _m_RunSpeedAddV;
		/// <summary>
		/// Tăng tốc chạy
		/// </summary>
		public int m_RunSpeedAddV
		{
			get
			{
				return this._m_RunSpeedAddV;
			}
			private set
			{
				lock (this)
				{
					this._m_RunSpeedAddV = value;
				}
			}
		}

		/// <summary>
		/// % tốc độ bị giảm
		/// </summary>
		private int _m_RunSpeedReduceP = 0;
		#endregion


		#region Tốc đánh
		private int _m_AttackSpeed;
		/// <summary>
		/// Tốc độ xuất chiêu hệ ngoại công cơ bản
		/// </summary>
		private int m_AttackSpeed
		{
			get
			{
				return this._m_AttackSpeed;
			}
			set
			{
				lock (this)
				{
					this._m_AttackSpeed = value;
				}
			}
		}

		private int _m_CurrentAttackSpeed;
		/// <summary>
		/// Tốc độ xuất chiêu hệ ngoại công hiện tại
		/// </summary>
		private int m_CurrentAttackSpeed
		{
			get
			{
				return this._m_CurrentAttackSpeed;
			}
			set
			{
				lock (this)
				{
					this._m_CurrentAttackSpeed = value;
				}
			}
		}

		private int _m_CastSpeed;
		/// <summary>
		/// Tốc độ xuất chiêu hệ nội công cơ bản
		/// </summary>
		private int m_CastSpeed
		{
			get
			{
				return this._m_CastSpeed;
			}
			set
			{
				lock (this)
				{
					this._m_CastSpeed = value;
				}
			}
		}

		private int _m_CurrentCastSpeed;
		/// <summary>
		/// Tốc độ xuất chiêu hệ nội công hiện tại
		/// </summary>
		private int m_CurrentCastSpeed
		{
			get
			{
				return this._m_CurrentCastSpeed;
			}
			set
			{
				lock (this)
				{
					this._m_CurrentCastSpeed = value;
				}
			}
		}

		/// <summary>
		/// % tốc đánh bị giảm
		/// </summary>
		private int _m_AttackSpeedReduceP = 0;
        #endregion


		private int _m_nCurLucky;
		/// <summary>
		/// May mắn hiện tại
		/// </summary>
		public int m_nCurLucky
        {
			get
            {
				return this._m_nCurLucky;
            }
			private set
            {
				lock (this)
                {
					this._m_nCurLucky = value;
                }
            }
        }

		/// <summary>
		/// % phản đòn cận chiến hiện tại
		/// </summary>
		public int m_CurrentMeleeDmgRetPercent { get; set; }
		/// <summary>
		/// Phản đòn cận chiếu
		/// </summary>
		public int m_CurrentMeleeDmgRet { get; set; }

		/// <summary>
		/// % phản đòn tầm xa hiện tại
		/// </summary>
		public int m_CurrentRangeDmgRetPercent { get; set; }
		/// <summary>
		/// Phản đòn tầm xa
		/// </summary>
		public int m_CurrentRangeDmgRet { get; set; }

		/// <summary>
		/// % phản đòn sát thương độc công
		/// </summary>
		public int m_CurrentPoisonDmgRetPercent { get; set; }
		/// <summary>
		/// Phản đòn sát thương độc công
		/// </summary>
		public int m_CurrentPoisonDmgRet { get; set; }

		/// <summary>
		/// Trạng thái miễn dịch
		/// </summary>
		public bool m_CurrentStatusImmunity { get; set; }

		/// <summary>
		/// Nội lực bị mất khi chịu sát thương độc
		/// </summary>
		public int m_CurrentPoison2Mana { get; set; }

        #region Dùng nội lực hút sát thương
        /// <summary>
        /// % hút sát thương của khiên nội lực hiện tại khi nội lực trên 15%
        /// </summary>
        public int m_CurrentManaShield { get; set; }

		/// <summary>
		/// ID kỹ năng dùng nội lực hút sát thương khi nội lực trên 15%
		/// </summary>
		public int m_CurrentManaShield_SkillID { get; set; }
        #endregion

		/// <summary>
		/// % hút sinh lực
		/// </summary>
		public int m_CurrentLifeStolen { get; set; }
		/// <summary>
		/// % hút nội lực
		/// </summary>
		public int m_CurrentManaStolen { get; set; }
		/// <summary>
		/// % hút thể lực
		/// </summary>
		public int m_CurrentStaminaStolen { get; set; }

		/// <summary>
		/// Kháng ngũ hành tương khắc
		/// </summary>
		public int m_CurrentSeriesResist { get; set; }
		/// <summary>
		/// Tăng cường ngũ hành tương khắc
		/// </summary>
		public int m_CurrentSeriesConquar { get; set; }
		/// <summary>
		/// Ngũ hành tương khắc
		/// </summary>
		public int m_CurrentSeriesEnhance { get; set; }

		/// <summary>
		/// Điểm chí mạng
		/// </summary>
		public int m_CurrentDeadlyStrike { get; set; }
		/// <summary>
		/// Xác suất làm tấn công chí mạng
		/// </summary>
		public int m_CurrentFatallyStrike { get; set; }

		/// <summary>
		/// Giảm % thời gian trúng độc
		/// </summary>
		public int m_CurrentPoisonTimeReducePercent { get; set; }

		/// <summary>
		/// Tăng vật công ngoại
		/// </summary>
		public int m_AddPhysicsDamage { get; set; }

		/// <summary>
		/// % vật công ngoại hiện tại
		/// </summary>
		public int m_CurrentMagicPhysicsEnhanceP { get; set; }
		/// <summary>
		/// % vật công ngoại hiện tại
		/// </summary>
		public int m_CurrentPhysicsDamageEnhanceP { get; set; }



        #region Khiên nội lực hấp thụ sát thương
        /// <summary>
        /// Dùng nội lực tạo thành khiên hóa giải sát thương
        /// </summary>
        public int m_CurrentDynamicMagicShield { get; set; }

		/// <summary>
		/// ID kỹ năng dùng nội lực tạo thành khiên
		/// </summary>
		public int m_CurrentDynamicMagicShield_SkillID { get; set; }
        #endregion


        #region Hóa giải sát thương
        /// <summary>
        /// Hóa giải sát thương (điểm)
        /// </summary>
        public int m_CurrentDynamicShield { get; set; }

		/// <summary>
		/// % sát thương tối đa có thể hóa giải
		/// </summary>
		public int m_CurrentDynamicShieldMaxP { get; set; }
        #endregion

        #region Chí tử
		/// <summary>
		/// Tỷ lệ tấn công chí tử giảm trực tiếp 10% sinh lực
		/// </summary>
		public int m_CurrentFatalStrikePercent { get; set; }
        #endregion


        /// <summary>
        /// Tỷ lệ bỏ qua bùa chú
        /// </summary>
        public int m_CurrentIgnoreCursePercent { get; set; }

        /// <summary>
        /// Xác suất phản đòn bùa chú
        /// </summary>
        public int m_CurrentReturnSkillPercent { get; set; }

		/// <summary>
		/// Bỏ qua né tránh đối thủ
		/// </summary>
		public int m_CurrentIgnoreDefense { get; set; }
		/// <summary>
		/// Bỏ qua % né trnahs đối thủ
		/// </summary>
		public int m_CurrentIgnoreDefensePercent { get; set; }

		/// <summary>
		/// Vô địch, không bị sát thương
		/// </summary>
		public int m_CurrentInvincibility { get; set; }

		/// <summary>
		/// Miễn nhiễm toàn bộ trạng thái ngũ hành
		/// </summary>
		public bool m_IgnoreAllSeriesStates { get; set; } = false;

		/// <summary>
		/// Có thể nhìn người khác trong trạng thái ẩn thân
		/// </summary>
		public bool m_CurrentShowHide { get; set; }

		/// <summary>
		/// Kéo dài thời gian trúng độc %
		/// </summary>
		public int m_nPoisonTimeEnhanceP { get; set; }

		/// <summary>
		/// Kỹ năng phát huy lực tấn công kỹ năng
		/// </summary>
		public int m_nSkillSelfDamagePTrim { get; set; }
		/// <summary>
		/// Kỹ năng phát huy lực tấn công cơ bản
		/// </summary>
		public int m_nSkillDamagePTrim { get; set; }

		/// <summary>
		/// Chịu sát thương chí mạng
		/// </summary>
		public int m_nDefenceDeadlyStrikeDamageTrim { get; set; }

		/// <summary>
		/// Cường hóa ngũ hành tương khắc
		/// </summary>
		public int m_nSeriesEnhance { get; set; }
		/// <summary>
		/// Nhược hóa ngũ hành tương khắc
		/// </summary>
		public int m_nSeriesAbate { get; set; }

		/// <summary>
		/// Sát thương cơ bản của vũ khí
		/// </summary>
		public int m_nWeaponBaseDamageTrim { get; set; }

		/// <summary>
		/// Hóa giải sát thương độc
		/// </summary>
		public int m_nPosionWeakenPoint { get; set; }

		/// <summary>
		/// Lượng sát thương ban đầu tối đa có thể hóa giải
		/// </summary>
		public int m_nPoisonWeakenMaxDamageP { get; set; }

		/// <summary>
		/// Tăng % kinh nghiệm
		/// </summary>
		public int m_nExpAddtionP { get; set; }

		/// <summary>
		/// Tấn công khi đánh chí mạng
		/// </summary>
		public int m_DeadlystrikeDamagePercent { get; set; }

		/// <summary>
		/// % kinh nghiệm nhận được từ đồng đội
		/// </summary>
		public int m_nShareExpP { get; set; }

		/// <summary>
		/// Giảm % kinh nghiệm hao tổn khi chết
		/// </summary>
		public int m_nSubExpPLost { get; set; }

        #region Mỗi khoảng bỏ qua nửa giây tấn công
        /// <summary>
        /// Mỗi khoảng thời gian sẽ bỏ qua số Frame công kích
        /// </summary>
        public int m_nIgnoreAttackOnTime { get; set; }

		/// <summary>
		/// Thời điểm bỏ qua công kích cuối cùng
		/// </summary>
		public long m_nLastIgnoreAttackTime { get; set; }

		/// <summary>
		/// Số Frame bỏ qua công kích
		/// </summary>
		public int m_nIgnoreAttackDuration { get; set; }
        #endregion

        #region Cộng thêm với mỗi kẻ địch mới
		/// <summary>
		/// ID kỹ năng khi có thêm kẻ địch
		/// </summary>
		public int m_sAddedWithEnemy_SkillID { get; set; }

		/// <summary>
		/// Cộng dồn tối đa khi có kẻ địch xung quanh
		/// </summary>
		public int m_sAddedWithEnemy_MaxCount { get; set; }

		/// <summary>
		/// Phạm vi hiệu quả tính kẻ địch xung quanh
		/// </summary>
		public int m_sAddedWithEnemy_Range { get; set; }

		/// <summary>
		/// ID kỹ năng chủ gọi kỹ năng phụ khi có thêm kẻ địch
		/// </summary>
		public int m_sAddedWithEnemy_OwnerSkillID { get; set; }

		/// <summary>
		/// Cấp độ kỹ năng khi có thêm kẻ địch
		/// </summary>
		public int m_sAddedWithEnemy_SkillLevel
        {
			get
            {
				if (this is KPlayer)
                {
					KPlayer player = this as KPlayer;
					SkillLevelRef skill = player.Skills.GetSkillLevelRef(m_sAddedWithEnemy_OwnerSkillID);
					if (skill == null)
                    {
						return 0;
                    }
					return skill.Level;
                }
				else
                {
					return 1;
                }
            }
        }
		#endregion

		/// <summary>
		/// Sát thương tăng thêm khi tấn công quái
		/// </summary>
		public int m_nDamageAddedPercentWhenHitNPC { get; set; }

        /// <summary>
        /// Sát thương tăng thêm
        /// </summary>
        public int m_nDamageAddedPercent { get; set; }


        /// <summary>
        /// Giảm sát thương phải chịu
        /// </summary>
        public int m_nDamageReceiveDecresedPercent { get; set; }
        /// <summary>
        /// Sát thương tăng thêm bởi nội lực hiện có
        /// </summary>
        public int m_nAttackAddedByMana { get; set; }

        #region Xác suất tránh công nội ngoại
        /// <summary>
        /// Xác suất tránh công ngoại
        /// </summary>
        public int m_sIgnorePhysicDamage { get; set; }

        /// <summary>
        /// Xác suất tránh công nội
        /// </summary>
        public int m_sIgnoreMagicDamage { get; set; }
        #endregion

        #region Xác suất tránh công tầm xa gần
		/// <summary>
		/// Xác suất tránh công tầm xa
		/// </summary>
		public int m_sIgnoreRangerDamage { get; set; }

		/// <summary>
		/// Xác suất tránh công tầm gần
		/// </summary>
		public int m_sIgnoreMeleeDamage { get; set; }
        #endregion

        #region Phản đòn khi tấn công
		/// <summary>
		/// Tỷ lệ bị phản đòn khi tấn công
		/// </summary>
		public int m_sReflectDamageWhenHit { get; set; }

		/// <summary>
		/// Mục tiêu khi tấn công sẽ bị phản đòn
		/// </summary>
		public int m_sReflectDamageWhenHitTargetID { get; set; } = -1;
        #endregion

        #region Bỏ qua phục hồi kỹ năng
        /// <summary>
        /// Bỏ qua thời gian phục hồi kỹ năng
        /// </summary>
        public bool m_sIgnoreSkillCooldowns { get; set; } = false;
		
		/// <summary>
		/// ID kỹ năng gây trạng thái bỏ qua thời gian phục hồi kỹ năng
		/// </summary>
		public int m_sIgnoreSkillCooldownsBuffID { get; set; }
		#endregion

		#region Miễn dịch sát thương nội ngoại
		/// <summary>
		/// Miễn dịch sát thương ngoại
		/// </summary>
		public bool m_ImmuneToPhysicDamage { get; set; } = false;

		/// <summary>
		/// Miễn dịch sát thương nội
		/// </summary>
		public bool m_ImmuneToMagicDamage { get; set; } = false;
        #endregion

		/// <summary>
		/// % sát thương được chuyển hóa thành nội lực
		/// </summary>
		public int m_Damage2AddManaP { get; set; }

        #region Danh sách kỹ năng bị cấm sử dụng
        /// <summary>
        /// Danh sách kỹ năng bị cấm sử dụng
        /// </summary>
        private HashSet<int> m_ForbidenSkills = new HashSet<int>();
        #endregion
        #region Danh sách kỹ năng miễn nhiễm
        /// <summary>
        /// Danh sách kỹ năng miễn nhiễm
        /// </summary>
        private ConcurrentDictionary<int, int> m_ImmuneSkills = new ConcurrentDictionary<int, int>();
        #endregion

        /// <summary>
        /// Tỷ lệ giảm sát thương nội công tầm gần
        /// </summary>
        public int m_ReduceNearMagicDamageP { get; set; }
        /// <summary>
        /// Tỷ lệ giảm sát thương nội công tầm xa
        /// </summary>
        public int m_ReduceFarMagicDamageP { get; set; }
        /// <summary>
        /// Tỷ lệ giảm sát thương ngoại công tầm gần
        /// </summary>
        public int m_ReduceNearPhysicDamageP { get; set; }
        /// <summary>
        /// Tỷ lệ giảm sát thương ngoại công tầm xa
        /// </summary>
        public int m_ReduceFarPhysicDamageP { get; set; }

		/// <summary>
		/// Tăng % sát thương khi nội lực đầy
		/// </summary>
		public int m_DamageMultipleWhenFullMana { get; set; }

        /// <summary>
        /// Lượng sinh lực tối đa được tăng thêm từ nội lực tối đa trước đó
        /// <para>Trường hợp này chỉ duy nhất 1 symbol có tác dụng 1 lúc nếu không sẽ BUG</para>
        /// </summary>
        public int m_MaxHPIncreasedByMaxMP { get; set; }

        /// <summary>
        /// Lượng sinh lực trước khi giảm lượng tăng thêm từ nội lực tối đa
        /// </summary>
        public int m_LastHPBeforeDetachAddMaxHPIncreasedByMaxMP { get; set; } = -1;
        #endregion

        #region Private methods
        /// <summary>
        /// Đồng bộ dữ liệu tốc chạy về Client
        /// </summary>
        private void SyncMoveSpeed()
		{
			KT_TCPHandler.NotifyTargetMoveSpeedChanged(this);
		}

		/// <summary>
		/// Đồng bộ dữ liệu tốc đánh về Client
		/// </summary>
		private void SynsAttackSpeed()
        {
			KT_TCPHandler.NotifyTargetAttackSpeedChanged(this);
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Thiết lập giá trị sát thương vật công ngoại
        /// </summary>
        /// <param name="nMinDamage"></param>
        /// <param name="nMaxDamage"></param>
        public void SetPhysicsDamage(int nMinDamage, int nMaxDamage)
		{
			m_PhysicsDamage.nValue[0] = nMinDamage;
			m_PhysicsDamage.nValue[2] = nMaxDamage;
			this.m_PhysicsDamage.nValue[0] = Math.Max(0, this.m_PhysicsDamage.nValue[0]);
			this.m_PhysicsDamage.nValue[2] = Math.Max(0, this.m_PhysicsDamage.nValue[2]);
			this.m_PhysicsDamage.nValue[0] = Math.Min(this.m_PhysicsDamage.nValue[0], this.m_PhysicsDamage.nValue[2]);
		}

        /// <summary>
        /// Thiết lập giá trị sát thương vật công nội
        /// </summary>
        /// <param name="nMinDamage"></param>
        /// <param name="nMaxDamage"></param>
        public void SetMagicDamage(int nMinDamage, int nMaxDamage)
		{
			m_MagicDamage.nValue[0] = nMinDamage;
			m_MagicDamage.nValue[2] = nMaxDamage;
			this.m_MagicDamage.nValue[0] = Math.Max(0, this.m_MagicDamage.nValue[0]);
			this.m_MagicDamage.nValue[2] = Math.Max(0, this.m_MagicDamage.nValue[2]);
			this.m_MagicDamage.nValue[0] = Math.Min(this.m_MagicDamage.nValue[0], this.m_MagicDamage.nValue[2]);
		}

		/// <summary>
		/// Thay đổi sát thương vật công ngoại
		/// </summary>
		/// <param name="nData"></param>
		public void ChangePhysicsDamage(int nData)
		{
			this.m_PhysicsDamage.nValue[0] += nData;
			this.m_PhysicsDamage.nValue[2] += nData;
			this.m_PhysicsDamage.nValue[0] = Math.Max(0, this.m_PhysicsDamage.nValue[0]);
			this.m_PhysicsDamage.nValue[2] = Math.Max(0, this.m_PhysicsDamage.nValue[2]);
			this.m_PhysicsDamage.nValue[0] = Math.Min(this.m_PhysicsDamage.nValue[0], this.m_PhysicsDamage.nValue[2]);
		}

		/// <summary>
		/// Thay đổi sát thương vật công nội
		/// </summary>
		/// <param name="nData"></param>
		public void ChangeMagicDamage(int nData)
		{
			this.m_MagicDamage.nValue[0] += nData;
			this.m_MagicDamage.nValue[2] += nData;
			this.m_MagicDamage.nValue[0] = Math.Max(0, this.m_MagicDamage.nValue[0]);
			this.m_MagicDamage.nValue[2] = Math.Max(0, this.m_MagicDamage.nValue[2]);
			this.m_MagicDamage.nValue[0] = Math.Min(this.m_MagicDamage.nValue[0], this.m_MagicDamage.nValue[2]);
		}

		/// <summary>
		/// Thay đổi giá trị sinh lực tối đa tối đa
		/// </summary>
		/// <param name="nBaseChange"></param>
		/// <param name="nAddPChange"></param>
		/// <param name="nAddVChange"></param>
		public void ChangeLifeMax(int nBaseChange, int nAddPChange, int nAddVChange)
		{
			this.m_CurrentLifeMax -= this.m_LifeMax * this.m_LifeMaxAddP / 100;
			this.m_LifeMax += nBaseChange;
			this.m_LifeMaxAddP += nAddPChange;
			this.m_CurrentLifeMax += this.m_LifeMax * this.m_LifeMaxAddP / 100 + nBaseChange + nAddVChange;

			/// Nếu sinh lực hiện tại lớn hơn sinh lực cực đại
			if (this.m_CurrentLife > this.m_CurrentLifeMax)
            {
				this.m_CurrentLife = this.m_CurrentLifeMax;
            }
		}

		/// <summary>
		/// Thiết lập lại giá trị sinh lực
		/// </summary>
		public void ResetLife()
        {
			this.m_CurrentLifeMax = 0;
			this.m_LifeMax = 0;
			this.m_LifeMaxAddP = 0;
			this.m_CurrentLifeMax = 0;
			this.m_CurrentLife = 0;
		}

		/// <summary>
		/// Get ra tốc độ đánh hệ ngoại 
		/// </summary>
		/// <returns></returns>
		public int GetCurrentAttackSpeed()
        {
			return this.m_CurrentAttackSpeed - this.m_CurrentAttackSpeed * this._m_AttackSpeedReduceP / 100;
		}

		/// <summary>
		/// Trả về tốc độ di chuyển
		/// </summary>
		/// <returns></returns>
		public int GetCurrentRunSpeed()
        {
			return this.m_CurrentRunSpeed - this._m_CurrentRunSpeed * this._m_RunSpeedReduceP / 100;

		}
		/// <summary>
		/// Lấy ra tốc độ đnáh hệ nội
		/// </summary>
		/// <returns></returns>
		public int GetCurrentCastSpeed()
        {
			return this.m_CurrentCastSpeed - this.m_CurrentCastSpeed * this._m_AttackSpeedReduceP / 100;

		}

		/// <summary>
		/// Lấy ra né trnash của nhân vật
		/// </summary>
		/// <returns></returns>
		public int GetCurrentDefend()
        {
			return m_CurrentDefend;

		}
		/// <summary>
		/// Lấy ra chính xác của nhân vật
		/// </summary>
		/// <returns></returns>
		public int CurrentAttackRating()
        {
			return m_CurrentAttackRating;

		}
		/// <summary>
		/// Thay đổi giá trị nội lực tối đa hiện tại
		/// </summary>
		/// <param name="nBaseChange"></param>
		/// <param name="nAddPChange"></param>
		/// <param name="nAddVChange"></param>
		public void ChangeManaMax(int nBaseChange, int nAddPChange, int nAddVChange)
		{
			this.m_CurrentManaMax -= this.m_ManaMax * this.m_ManaMaxAddP / 100;
			this.m_ManaMax += nBaseChange;
			this.m_ManaMaxAddP += nAddPChange;
			this.m_CurrentManaMax += this.m_ManaMax * this.m_ManaMaxAddP / 100 + nBaseChange + nAddVChange;

			/// Nếu nội lực hiện tại lớn hơn nội lực cực đại
			if (this.m_CurrentMana > this.m_CurrentManaMax)
			{
				this.m_CurrentMana = this.m_CurrentManaMax;
			}
		}

		/// <summary>
		/// Thay đổi giá trị thể lực tối đa hiện tại
		/// </summary>
		/// <param name="nBaseChange"></param>
		/// <param name="nAddPChange"></param>
		/// <param name="nAddVChange"></param>
		public void ChangeStaminaMax(int nBaseChange, int nAddPChange, int nAddVChange)
		{
			this.m_CurrentStaminaMax -= this.m_StaminaMax * this.m_StaminaMaxAddP / 100;
			this.m_StaminaMax += nBaseChange;
			this.m_StaminaMaxAddP += nAddPChange;
			this.m_CurrentStaminaMax += this.m_StaminaMax * this.m_StaminaMaxAddP / 100 + nBaseChange + nAddVChange;

			/// Nếu thể lực hiện tại lớn hơn thể lực cực đại
			if (this.m_CurrentStamina > this.m_CurrentStaminaMax)
			{
				this.m_CurrentStamina = this.m_CurrentStaminaMax;
			}
		}



		/// <summary>
		/// Thay đổi giá trị tốc chạy
		/// </summary>
		/// <param name="nBaseChange"></param>
		/// <param name="nAddPChange"></param>
		/// <param name="nAddVChange"></param>
		public void ChangeRunSpeed(int nBaseChange, int nAddPChange, int nAddVChange)
		{
			this.m_CurrentRunSpeed -= (this.m_RunSpeed * this.m_RunSpeedAddP / 100 + this.m_RunSpeedAddV / 10);
			this.m_RunSpeed += nBaseChange;
			this.m_RunSpeedAddP += nAddPChange;
			this.m_RunSpeedAddV += nAddVChange;
			this.m_CurrentRunSpeed += (this.m_RunSpeed * this.m_RunSpeedAddP / 100 + nBaseChange + this.m_RunSpeedAddV / 10);

			this.SyncMoveSpeed();
		}

		/// <summary>
		/// Thay đổi giá trị Tốc độ xuất chiêu hệ ngoại công
		/// </summary>
		/// <param name="nBaseChange"></param>
		/// <param name="nAddVChange"></param>
		public void ChangeAttackSpeed(int nBaseChange, int nAddVChange)
        {
			this.m_CurrentAttackSpeed += nAddVChange;
			this.m_AttackSpeed += nBaseChange;

			this.SynsAttackSpeed();
        }

		/// <summary>
		/// Thay đổi giá trị Tốc độ xuất chiêu hệ nội công
		/// </summary>
		/// <param name="nBaseChange"></param>
		/// <param name="nAddVChange"></param>
		public void ChangeCastSpeed(int nBaseChange, int nAddVChange)
        {
			this.m_CurrentCastSpeed += nAddVChange;
			this.m_CastSpeed += nBaseChange;

			this.SynsAttackSpeed();
		}

		/// <summary>
		/// Thay đổi giá trị chính xác
		/// </summary>
		/// <param name="nBaseChange"></param>
		/// <param name="nAddPChange"></param>
		/// <param name="nAddVChange"></param>
		public void ChangeAttackRating(int nBaseChange, int nAddPChange, int nAddVChange)
		{
			this.m_CurrentAttackRating -= this.m_AttackRating * this.m_AttackRatingAddP / 100;
			this.m_AttackRating += nBaseChange;
			this.m_AttackRatingAddP += nAddPChange;
			this.m_CurrentAttackRating += this.m_AttackRating * this.m_AttackRatingAddP / 100 + nBaseChange + nAddVChange;

			//this.SyncAttributes();
		}

		/// <summary>
		/// Thay đổi giá trị né tránh
		/// </summary>
		/// <param name="nBaseChange"></param>
		/// <param name="nAddPChange"></param>
		/// <param name="nAddVChange"></param>
		public void ChangeDefend(int nBaseChange, int nAddPChange, int nAddVChange)
		{
			this.m_CurrentDefend -= this.m_Defend * this.m_DefendAddP / 100;
			this.m_Defend += nBaseChange;
			this.m_DefendAddP += nAddPChange;
			this.m_CurrentDefend += this.m_Defend * this.m_DefendAddP / 100 + nBaseChange + nAddVChange;

			//this.SyncAttributes();
		}

		/// <summary>
		/// Thay đổi giá trị may mắn
		/// </summary>
		/// <param name="nData"></param>
		public void ChangeCurLucky(int nData)
		{
			this.m_nCurLucky += nData;
		}

		/// <summary>
		/// Trả về giá trị công kích thuộc tính (ngoại) tương ứng hiện tại
		/// </summary>
		/// <param name="eType"></param>
		/// <returns></returns>
		public int GetSeriesDamagePhysics(DAMAGE_TYPE eType)
		{
			if ((int) eType >= (int) DAMAGE_TYPE.damage_num)
            {
				return 0;
            }

			return this.m_damage[(int)eType].GetEnhanceDamage();
		}

		/// <summary>
		/// Trả về giá trị công kích thuộc tính (nội) tương ứng hiện tại
		/// </summary>
		/// <param name="eType"></param>
		/// <returns></returns>
		public int GetSeriesDamageMagics(DAMAGE_TYPE eType)
		{
			if ((int) eType >= (int) DAMAGE_TYPE.damage_num)
			{
				return 0;
			}

			return this.m_damage[(int)eType].GetEnhanceMagic();
		}

		/// <summary>
		/// Thêm giá trị công kích thuộc tính (ngoại) tương ứng hiện tại
		/// </summary>
		/// <param name="eType"></param>
		/// <returns></returns>
		public void AddSeriesDamagePhysics(DAMAGE_TYPE eType, int nAdd)
		{
			if ((int) eType >= (int) DAMAGE_TYPE.damage_num)
			{
				return;
			}

			this.m_damage[(int)eType].AddEnhanceDamage(nAdd);
		}
        /// <summary>
        /// Thêm % công kích thuộc tính (ngoại) tương ứng hiện tại
        /// </summary>
        /// <param name="eType"></param>
        /// <returns></returns>
        public void AddSeriesDamagePhysicsP(KE_SERIES_TYPE eType, int nAdd)
        {
            if (!this.SeriesEnhanceDamageP.ContainsKey(eType))
            {
                return;
            }

            this.SeriesEnhanceDamageP[eType] += nAdd;
        }


        /// <summary>
        /// Thêm % công kích thuộc tính (nội) tương ứng hiện tại
        /// </summary>
        /// <param name="eType"></param>
        /// <returns></returns>
        public void AddSeriesDamageMagicsP(KE_SERIES_TYPE eType, int nAdd)
        {
            if (!this.SeriesEnhanceMagicP.ContainsKey(eType))
            {
                return;
            }

            this.SeriesEnhanceMagicP[eType] += nAdd;
        }
        /// <summary>
        /// Thiết lập giá trị công kích thuộc tính (ngoại) tương ứng hiện tại
        /// </summary>
        /// <param name="nValue"></param>
        /// <returns></returns>
        public void SetSeriesDamagePhysics(DAMAGE_TYPE eType, int nValue)
		{
			if ((int) eType >= (int) DAMAGE_TYPE.damage_num)
			{
				return;
			}

			this.m_damage[(int)eType].SetEnhanceDamage(nValue);
		}

		/// <summary>
		/// Trả về giá trị công kích thuộc tính (nội) tương ứng hiện tại
		/// </summary>
		/// <param name="eType"></param>
		/// <returns></returns>
		public void AddSeriesDamageMagics(DAMAGE_TYPE eType, int nAdd)
		{
			if ((int) eType >= (int) DAMAGE_TYPE.damage_num)
			{
				return;
			}

			this.m_damage[(int)eType].AddEnhanceMagic(nAdd);
		}

		/// <summary>
		/// Trả về giá trị kháng thuộc tính tương ứng hiện tại
		/// </summary>
		/// <param name="eType"></param>
		/// <returns></returns>
		public int GetCurResist(DAMAGE_TYPE eType)
		{
			if ((int) eType >= (int) DAMAGE_TYPE.damage_num)
			{
				return 0;
			}

			return this.m_damage[(int)eType].GetCurResist();
		}

		/// <summary>
		/// Trả về giá trị kháng thuộc tính tương ứng cơ bản
		/// </summary>
		/// <param name="eType"></param>
		/// <returns></returns>
		public int GetResist(DAMAGE_TYPE eType)
		{
			if ((int) eType >= (int) DAMAGE_TYPE.damage_num)
			{
				return 0;
			}

			return this.m_damage[(int)eType].GetResist();
		}

		/// <summary>
		/// Thêm giá trị kháng thuộc tính hiện tại
		/// </summary>
		/// <param name="eType"></param>
		/// <param name="nAdd"></param>
		public void AddCurResist(DAMAGE_TYPE eType, int nAdd)
		{
			if ((int) eType >= (int) DAMAGE_TYPE.damage_num)
			{
				return;
			}

			this.m_damage[(int)eType].AddCurResist(nAdd);
		}

		/// <summary>
		/// Thêm giá trị kháng thuộc tính cơ bản
		/// </summary>
		/// <param name="eType"></param>
		/// <param name="nAdd"></param>
		public void AddResist(DAMAGE_TYPE eType, int nAdd)
		{
			if ((int) eType >= (int) DAMAGE_TYPE.damage_num)
			{
				return;
			}

			this.m_damage[(int)eType].AddResist(nAdd);
		}

		/// <summary>
		/// Thiết lập giá trị kháng thuộc tính cơ bản
		/// </summary>
		/// <param name="eType"></param>
		/// <param name="nResist"></param>
		public void SetResist(DAMAGE_TYPE eType, int nResist)
		{
			if ((int) eType >= (int) DAMAGE_TYPE.damage_num)
			{
				return;
			}

			this.m_damage[(int)eType].SetResist(nResist);
		}

		/// <summary>
		/// Thiết lập giá trị cường hóa công kích ngũ hành tương ứng
		/// </summary>
		/// <param name="eType"></param>
		/// <param name="nDamage"></param>
		public void SetAttackEnhance(DAMAGE_TYPE eType, int nDamage)
		{
			if ((int) eType >= (int) DAMAGE_TYPE.damage_num)
			{
				return;
			}

			this.m_damage[(int)eType].SetEnhance(nDamage);
		}

        #region Miễn nhiễm kỹ năng
		/// <summary>
		/// Thiết lập tỷ lệ miễn nhiễm kỹ năng tương ứng
		/// </summary>
		/// <param name="skillID"></param>
		/// <param name="percent"></param>
		public void SetImmuneToSkill(int skillID, int percent)
		{
			/// Thiết lập giá trị
			this.m_ImmuneSkills[skillID] = percent;
		}

		/// <summary>
		/// Xóa tỷ lệ miễn nhiễm kỹ năng tương ứng
		/// </summary>
		/// <param name="skillID"></param>
		public void RemoveImmuneToSkill(int skillID)
        {
			/// Xóa
			this.m_ImmuneSkills.TryRemove(skillID, out _);
        }

		/// <summary>
		/// Trả về tỷ lệ miễn nhiễm với kỹ năng
		/// </summary>
		/// <param name="skillID"></param>
		/// <returns></returns>
		public int GetImmuneToSkill(int skillID)
		{
            /// Nếu không tồn tại
            if (!this.m_ImmuneSkills.TryGetValue(skillID, out int percent))
            {
				/// Trả về 0
				return 0;
            }
			/// Trả về kết quả
			return percent;
        }
        #endregion

        #region Cấm dùng kỹ năng
        /// <summary>
        /// Kiểm tra kỹ năng có bị cấm sử dụng không
        /// </summary>
        /// <param name="skillID"></param>
        public bool IsSkillForbidden(int skillID)
        {
            return this.m_ForbidenSkills.Contains(skillID);
        }

        /// <summary>
        /// Thêm kỹ năng vào danh sách cấm sử dụng
        /// </summary>
        /// <param name="skillID"></param>
        public void AddForbidSkill(int skillID)
        {
            this.m_ForbidenSkills.Add(skillID);
        }

        /// <summary>
        /// Xóa kỹ năng khỏi danh sách cấm sử dụng
        /// </summary>
        /// <param name="skillID"></param>
        public void RemoveForbidSkill(int skillID)
        {
            this.m_ForbidenSkills.Remove(skillID);
        }
        #endregion
        #endregion
    }
}
