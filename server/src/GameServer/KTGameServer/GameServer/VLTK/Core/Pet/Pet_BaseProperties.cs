using GameServer.KiemThe.Entities;
using GameServer.KiemThe.Logic;
using GameServer.VLTK.Entities.Pet;
using Server.Data;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace GameServer.Logic
{
    /// <summary>
    /// Đối tượng Pet
    /// </summary>
    public partial class Pet
    {
        /// <summary>
        /// ID của files cấu hình
        /// </summary>
        public int ResID { get; set; }

        /// <summary>
        /// Danh hiệu pet
        /// </summary>
        public override string Title
        {
            get
            {
                return string.Format("<color=yellow>Tinh linh của <color=#5cb6ff>[{0}]</color></color>", this.Owner.RoleName);
            }
        }

        /// <summary>
        /// Lĩnh ngộ
        /// </summary>
        public int Enlightenment { get; set; }

        /// <summary>
        /// Danh sách kỹ năng
        /// </summary>
        public Dictionary<int, int> Skills { get; set; }

        /// <summary>
        /// Danh sách trang bị
        /// </summary>
        public Dictionary<int, int> Equips { get; set; }

        /// <summary>
        /// Điểm vui vẻ
        /// </summary>
        public int Joyful { get; set; }

        /// <summary>
        /// Tuổi thọ
        /// </summary>
        public int Life { get; set; }

        /// <summary>
        /// Điểm tiềm năng cơ bản có thêm
        /// </summary>
        public int BaseRemainPoints { get; set; }

        /// <summary>
        /// Điểm tiềm năng hiện có
        /// </summary>
        public int RemainPoints
        {
            get
            {
                /// Tổng điểm tiềm năng đã phân phối
                int totalAssignedRemainPoints = this.m_wStrength + this.m_wDexterity + this.m_wVitality + this.m_wEnergy;
                /// Tổng điểm tiềm năng đến cấp hiện tại
                int totalRemainPointsThisLevel = KPet.GetTotalRemainPoints(this.m_Level);
                /// Trả về kết quả
                return totalRemainPointsThisLevel + this.BaseRemainPoints - totalAssignedRemainPoints;
            }
        }

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
        /// Sức mạnh cơ bản đã cộng vào
        /// </summary>
        public int BaseStr
        {
            get
            {
                return this.m_wStrength;
            }
        }

        /// <summary>
        /// Thân pháp cơ bản đã cộng vào
        /// </summary>
        public int BaseDex
        {
            get
            {
                return this.m_wDexterity;
            }
        }

        /// <summary>
        /// Ngoại công cơ bản đã cộng vào
        /// </summary>
        public int BaseSta
        {
            get
            {
                return this.m_wVitality;
            }
        }

        /// <summary>
        /// Nội công cơ bản đã cộng vào
        /// </summary>
        public int BaseInt
        {
            get
            {
                return this.m_wEnergy;
            }
        }

        /// <summary>
        /// Sức
        /// </summary>
        public int Str
        {
            get
            {
                return this.m_wCurStrength;
            }
        }

        /// <summary>
        /// Thân
        /// </summary>
        public int Dex
        {
            get
            {
                return this.m_wCurDexterity;
            }
        }

        /// <summary>
        /// Ngoại
        /// </summary>
        public int Sta
        {
            get
            {
                return this.m_wCurVitality;
            }
        }

        /// <summary>
        /// Nội
        /// </summary>
        public int Int
        {
            get
            {
                return this.m_wCurEnergy;
            }
        }

        /// <summary>
		/// Thay đổi giá trị Sức cơ bản của pet
		/// HÀM NÀY CHỈ GỌI KHI pet + ĐIỂM TIỀN NĂNG HOẶC pet TẨY ĐIỂM
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

            //// LƯU LẠI CHỈ SỐ STR HIỆN TẠI VÀO DB
            //Global.SaveRoleParamsInt32ValueToDB(this, RoleParamName.sPropStrength, this.m_wStrength, true);
        }

        /// <summary>
        /// Thay đổi giá trị Thân cơ bản của pet
        /// HÀM NÀY CHỈ GỌI KHI pet + ĐIỂM TIỀN NĂNG HOẶC pet TẨY ĐIỂM
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

            //// LƯU LẠI CHỈ SỐ DEV HIỆN TẠI VÀO DB
            //Global.SaveRoleParamsInt32ValueToDB(this, RoleParamName.sPropDexterity, this.m_wDexterity, true);
        }

        /// <summary>
        /// Thay đổi giá trị Ngoại cơ bản của pet
        ///  HÀM NÀY CHỈ GỌI KHI pet + ĐIỂM TIỀN NĂNG HOẶC pet TẨY ĐIỂM
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

            //// LƯU LẠI CHỈ SỐ THỂ HIỆN TẠI VÀO DB
            //Global.SaveRoleParamsInt32ValueToDB(this, RoleParamName.sPropConstitution, this.m_wVitality, true);
        }

        /// <summary>
        /// Thay đổi giá trị Nội cơ bản của pet
        ///  HÀM NÀY CHỈ GỌI KHI pet + ĐIỂM TIỀN NĂNG HOẶC pet TẨY ĐIỂM
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

            //// LƯU LẠI CHỈ SỐ ENERGY VÀO DB
            //Global.SaveRoleParamsInt32ValueToDB(this, RoleParamName.sPropIntelligence, this.m_wEnergy, true);
        }

        /// <summary>
        /// Thay đổi giá trị Sức hiện tại của pet
        /// HÀM NÀY GỌI KHI ATTACK CÁC BUFF TẠM THỜI HOẶC STR TEMP TỪ BASESETTING=> HOẶC KHI pet ĐƯỢC NHẬN STR TẠM THỜI
        /// TOÀN BỘ GIÁ TRỊ NÀY SẼ ĐƯỢC RESET KHI pet LOGINOUT. VÀ RECALCULATION KHI pet LOGIN
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

            int nDamagePhysicsChange = KPet.GetStrength2DamagePhysics(this.ResID, nCurStrengthNew) -
                                       KPet.GetStrength2DamagePhysics(this.ResID, this.m_wCurStrength);
            this.ChangePhysicsDamage(nDamagePhysicsChange);

            this.m_wCurStrength = nCurStrengthNew;
        }

        /// <summary>
        /// Thay đổi giá trị Thân hiện tại của pet
        /// HÀM NÀY GỌI KHI ATTACK CÁC BUFF TẠM THỜI HOẶC Dexterity TEMP TỪ BASESETTING=> HOẶC KHI pet ĐƯỢC NHẬN Dexterity TẠM THỜI
        /// TOÀN BỘ GIÁ TRỊ NÀY SẼ ĐƯỢC RESET KHI pet LOGINOUT. VÀ RECALCULATION KHI pet LOGIN
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

            int nAttackRateChange = KPet.GetDexterity2AttackRate(this.ResID, nCurDexterityNew) -
                                    KPet.GetDexterity2AttackRate(this.ResID, this.m_wCurDexterity);
            this.ChangeAttackRating(nAttackRateChange, 0, 0);

            int nDefenceChange = KPet.GetDexterity2Defence(this.ResID, nCurDexterityNew) -
                                 KPet.GetDexterity2Defence(this.ResID, this.m_wCurDexterity);
            this.ChangeDefend(nDefenceChange, 0, 0);

            this.m_wCurDexterity = nCurDexterityNew;
        }

        /// <summary>
        /// Thay đổi giá trị Ngoại hiện tại của pet
        /// HÀM NÀY GỌI KHI ATTACK CÁC BUFF TẠM THỜI HOẶC Vitality TEMP TỪ BASESETTING=> HOẶC KHI pet ĐƯỢC NHẬN Vitality TẠM THỜI
        /// TOÀN BỘ GIÁ TRỊ NÀY SẼ ĐƯỢC RESET KHI pet LOGINOUT. VÀ RECALCULATION KHI pet LOGIN
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

            int nLifeChange = KPet.GetVitality2Life(this.ResID, nCurVitalityNew) -
                              KPet.GetVitality2Life(this.ResID, this.m_wCurVitality);
            this.ChangeLifeMax(nLifeChange, 0, 0);

            this.m_wCurVitality = nCurVitalityNew;
        }

        /// <summary>
        /// Thay đổi giá trị Nội hiện tại của pet
        /// HÀM NÀY GỌI KHI ATTACK CÁC BUFF TẠM THỜI HOẶC CurEnergy TEMP TỪ BASESETTING=> HOẶC KHI pet ĐƯỢC NHẬN CurEnergy TẠM THỜI
        /// TOÀN BỘ GIÁ TRỊ NÀY SẼ ĐƯỢC RESET KHI pet LOGINOUT. VÀ RECALCULATION KHI pet LOGIN
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

            int nDamageMagicChange = KPet.GetEnergy2DamageMagic(this.ResID, nCurEnergyNew) -
                                     KPet.GetEnergy2DamageMagic(this.ResID, this.m_wCurEnergy);
            this.ChangeMagicDamage(nDamageMagicChange);

            this.m_wCurEnergy = nCurEnergyNew;
        }

        /// <summary>
        /// Gửi gói tin thông báo tiềm năng của pet thay đổi về cho chủ nhân
        /// </summary>
        public void NotifyPetAttributes()
        {
            /// Lấy thông tin
            PetData petData = this.GetDBData();

            /// Thông tin Pet
            PetDataXML _data = KPet.GetPetData(petData.ResID);
            /// Toác
            if (_data == null)
            {
                /// Bỏ qua
                return;
            }

            /// Tiềm năng
            petData.RemainPoints = this.RemainPoints;
            /// Sức, thân, ngoại, nội
            petData.Str = this.m_wCurStrength;
            petData.Dex = this.m_wCurDexterity;
            petData.Sta = this.m_wCurVitality;
            petData.Int = this.m_wCurEnergy;

            /// Gửi gói tin
            KT_TCPHandler.NotifyPetAttributes(this.Owner, petData);
        }

        /// <summary>
        /// Thực hiện phân phối điểm tiềm năng pet
        /// </summary>
        /// <param name="str"></param>
        /// <param name="dex"></param>
        /// <param name="sta"></param>
        /// <param name="ene"></param>
        /// <param name="syncClient"></param>
        public bool DisputeAttributes(int str, int dex, int sta, int ene, bool syncClient = true)
        {
            /// Tổng số điểm tiềm năng sẽ sử dụng
            int totalWillUseRemainPoints = str + dex + sta + ene;
            /// Nếu quá tổng điểm tiềm năng hiện có
            if (this.RemainPoints < totalWillUseRemainPoints)
            {
                /// Toác
                return false;
            }

            /// OK thì cho phân phối
            this.ChangeStrength(str);
            this.ChangeDexterity(dex);
            this.ChangeVitality(sta);
            this.ChangeEnergy(ene);

            /// Thông tin tương ứng
            PetData petData = this.Owner.PetList.Where(x => x.ID == this.RoleID - (int) ObjectBaseID.Pet).FirstOrDefault();
            /// Nếu tìm thấy
            if (petData != null)
            {
                /// Lưu lại
                petData.Str = this.m_wStrength;
                petData.Dex = this.m_wDexterity;
                petData.Sta = this.m_wVitality;
                petData.Int = this.m_wEnergy;
            }

            /// Nếu có yêu cầu đồng bộ về client
            if (syncClient)
            {
                /// Thông báo cho chủ nhân
                this.NotifyPetAttributes();
            }

            /// OK
            return true;
        }

        /// <summary>
        /// Thực hiện tẩy toàn bộ tiềm năng
        /// </summary>
        /// <param name="syncClient"></param>
        /// <returns></returns>
        public void ResetAllAttributes(bool syncClient = true)
        {
            /// Reset toàn bộ
            this.ChangeStrength(-this.m_wStrength);
            this.ChangeDexterity(-this.m_wDexterity);
            this.ChangeVitality(-this.m_wVitality);
            this.ChangeEnergy(-this.m_wEnergy);

            /// Nếu có yêu cầu đồng bộ về client
            if (syncClient)
            {
                /// Thông báo cho chủ nhân
                this.NotifyPetAttributes();
            }
        }
    }
}
