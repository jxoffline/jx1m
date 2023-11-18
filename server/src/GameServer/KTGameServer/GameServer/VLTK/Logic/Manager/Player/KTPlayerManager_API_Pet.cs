using GameServer.Logic;
using GameServer.VLTK.Entities.Pet;
using Server.Data;
using System.Collections.Generic;

namespace GameServer.KiemThe.Logic
{
    /// <summary>
    /// Quản lý người chơi
    /// </summary>
    public static partial class KTPlayerManager
    {
        #region Pet
        /// <summary>
        /// Tạo pet cho người chơi tương ứng
        /// </summary>
        /// <param name="player"></param>
        /// <param name="id"></param>
        public static PetData CreatePet(KPlayer player, int id)
        {
            /// Nếu đã quá số lượng
            if (player.PetList != null && player.PetList.Count >= KPet.Config.MaxCanTake)
            {
                /// Thông báo
                KTPlayerManager.ShowNotification(player, "Số lượng tinh linh mang theo đã đạt tối đa.");
                /// Bỏ qua
                return null;
            }

            /// Thông tin Pet theo ID
            PetDataXML petData = KPet.GetPetData(id);
            /// Toác
            if (petData == null)
            {
                /// Thông báo
                KTPlayerManager.ShowNotification(player, "Thông tin tinh linh không tồn tại.");
                /// Không có kết quả
                return null;
            }
            /// Tạo mới
            PetData pet = new PetData()
            {
                ResID = id,
                RoleID = player.RoleID,
                Name = petData.Name,
                Level = 1,
                Exp = 0,
                Enlightenment = 100,
                Skills = new Dictionary<int, int>(),
                Equips = new Dictionary<int, int>(),
                Str = 0,
                Dex = 0,
                Sta = 0,
                Int = 0,
                RemainPoints = 0,
                Joyful = 100,
                Life = 100,
                HP = petData.HP + KPet.GetVitality2Life(id, petData.Sta),
            };
            /// Duyệt danh sách kỹ năng sẵn có
            foreach (int skillID in petData.Skills)
            {
                /// Thêm vào danh sách kỹ năng
                pet.Skills[skillID] = 1;
            }

            /// Gửi lên GameDB
            int ret = KT_TCPHandler.SendDBAddPet(player, pet);
            /// Toác
            if (ret == -1)
            {
                /// Thông báo
                KTPlayerManager.ShowNotification(player, "Không thể tạo tinh linh. Hãy liên hệ với hỗ trợ để báo lỗi!");
                /// Toác
                return null;
            }
            /// Đánh dấu lại ID
            pet.ID = ret;

            /// Chưa tồn tại thì tạo mới
            if (player.PetList == null)
            {
                player.PetList = new List<PetData>();
            }

            /// Thêm vào cho người chơi
            player.PetList.Add(pet);

            /// Thông báo
            KTPlayerManager.ShowNotification(player, "Triệu hồi tinh linh thành công!");

            /// Trả về kết quả
            return pet;
        }
        #endregion
    }
}
