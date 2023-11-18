using GameServer.KiemThe.Entities;
using GameServer.KiemThe.Logic;
using GameServer.VLTK.Entities.Pet;
using Server.Data;
using System.Linq;

namespace GameServer.Logic
{
    /// <summary>
    /// Quản lý các sự kiện
    /// </summary>
    public partial class Pet
    {
        /// <summary>
        /// Sự kiện khi pet lên cấp
        /// </summary>
        /// <param name="oldLevel"></param>
        public void OnLevelUp(int oldLevel)
        {
            /// Tăng thêm bao nhiêu cấp
            int totalLevelEarned = this.m_Level - oldLevel;
            /// Thông tin pet
            PetDataXML petData = KPet.GetPetData(this.ResID);
            /// Toác
            if (petData == null)
            {
                return;
            }

            /// Tăng thêm các điểm thuộc tính tương ứng
            this.ChangeCurStrength(petData.StrPerLevel * totalLevelEarned);
            this.ChangeCurDexterity(petData.DexPerLevel * totalLevelEarned);
            this.ChangeCurVitality(petData.StaPerLevel * totalLevelEarned);
            this.ChangeCurEnergy(petData.IntPerLevel * totalLevelEarned);

            /// Thông tin tương ứng
            PetData data = this.Owner.PetList.Where(x => x.ID == this.RoleID - (int) ObjectBaseID.Pet).FirstOrDefault();
            /// Nếu tìm thấy
            if (petData != null)
            {
                /// Lưu lại
                data.Str = this.m_wStrength;
                data.Dex = this.m_wDexterity;
                data.Sta = this.m_wVitality;
                data.Int = this.m_wEnergy;
            }

            /// Thông báo chỉ số về Client
            this.NotifyPetAttributes();

            /// Thông báo cho bọn xung quanh cập nhật cấp độ
            KT_TCPHandler.SendPetLevelChanged(this);
        }

        /// <summary>
        /// Sự kiện khi pet tăng kinh nghiệm
        /// </summary>
        /// <param name="exp"></param>
        public void OnEarnExp(int exp)
        {

        }

        /// <summary>
        /// Hàm này gọi đến khi pet bị giết
        /// </summary>
        /// <param name="attacker"></param>
        public override void OnDie(GameObject attacker)
        {
            /// Gọi đến Base
            base.OnDie(attacker);

            /// Thông báo
            KTPlayerManager.ShowNotification(this.Owner, string.Format("Tinh linh [{0}] đã bị [{1}] đánh chết. Tuổi thọ giảm {2} điểm.", this.RoleName, attacker.RoleName, KPet.Config.DieLoseLife));

            /// Giảm tuổi thọ
            this.Life -= KPet.Config.DieLoseLife;
            /// Dữ liệu pet của người chơi
            PetData data = this.Owner.PetList.Where(x => x.ID == this.RoleID - (int) ObjectBaseID.Pet).FirstOrDefault();
            /// Nếu tồn tại
            if (data != null)
            {
                /// Lưu lại giá trị tuổi thọ
                data.Life = this.Life;
                /// Thông báo về Client
                KT_TCPHandler.NotifyPetBaseAttributes(this.Owner, data);
            }

            /// Xóa pet
            this.Owner.CallBackPet(true);
        }
    }
}
