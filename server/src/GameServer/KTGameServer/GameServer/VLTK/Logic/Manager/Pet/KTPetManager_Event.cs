using GameServer.VLTK.Entities.Pet;
using GameServer.KiemThe.Logic;
using Server.Data;
using System.Linq;
using System.Drawing;
using GameServer.KiemThe;
using GameServer.KiemThe.Entities;

namespace GameServer.Logic
{
    /// <summary>
    /// Quản lý Pet
    /// </summary>
    public static partial class KTPetManager
    {
        /// <summary>
        /// Thêm điểm lĩnh ngộ cho Pet
        /// </summary>
        /// <param name="player"></param>
        /// <param name="petID"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public static void AddPetEnlightenment(KPlayer player, int petID, int point)
        {
            /// Nếu là pet đang tham chiến
            if (player.CurrentPet != null && player.CurrentPet.RoleID - (int) ObjectBaseID.Pet == petID)
            {
                /// Pet tương ứng
                Pet pet = player.CurrentPet;

                /// Tăng điểm lĩnh ngộ
                pet.Enlightenment += point;
                
                /// Dữ liệu pet của người chơi
                PetData data = player.PetList.Where(x => x.ID == petID).FirstOrDefault();
                /// Nếu tồn tại
                if (data != null)
                {
                    /// Lưu lại
                    data.Enlightenment = pet.Enlightenment;
                    /// Lưu vào GameDB
                    bool ret = KT_TCPHandler.SendDBUpdatePetEnlightenment(player, data);
                    /// Toác
                    if (!ret)
                    {
                        /// Thông báo
                        KTPlayerManager.ShowNotification(player, "Tăng điểm lĩnh ngộ cho tinh linh thất bại!");
                    }
                    else
                    {
                        /// Thông báo về Client
                        KT_TCPHandler.NotifyPetBaseAttributes(pet.Owner, data);
                        /// Thông báo
                        KTPlayerManager.ShowNotification(player, "Tăng điểm lĩnh ngộ cho tinh linh thành công!");
                        KTPlayerManager.ShowNotification(player, string.Format("Tinh linh <color=yellow>[{0}]</color> lĩnh ngộ tăng <color=yellow>{1} điểm</color>, lên thành <color=yellow>{2}</color>.", pet.RoleName, point, pet.Enlightenment));
                    }
                }
            }
            /// Nếu không phải pet đang tham chiến
            else
            {
                /// Dữ liệu pet của người chơi
                PetData data = player.PetList.Where(x => x.ID == petID).FirstOrDefault();
                /// Nếu tồn tại
                if (data != null)
                {
                    /// Lưu lại
                    data.Enlightenment += point;
                    /// Lưu vào GameDB
                    bool ret = KT_TCPHandler.SendDBUpdatePetEnlightenment(player, data);
                    /// Toác
                    if (!ret)
                    {
                        /// Thông báo
                        KTPlayerManager.ShowNotification(player, "Tăng điểm lĩnh ngộ cho tinh linh thất bại!");
                    }
                    else
                    {
                        /// Thông báo về Client
                        KT_TCPHandler.NotifyPetBaseAttributes(player, data);
                        /// Thông báo
                        KTPlayerManager.ShowNotification(player, "Tăng điểm lĩnh ngộ cho tinh linh thành công!");
                        KTPlayerManager.ShowNotification(player, string.Format("Tinh linh <color=yellow>[{0}]</color> lĩnh ngộ tăng <color=yellow>{1} điểm</color>, lên thành <color=yellow>{2}</color>.", data.Name, point, data.Enlightenment));
                    }
                }
            }
        }

        /// <summary>
        /// Thêm điểm vui vẻ cho pet tương ứng
        /// </summary>
        /// <param name="player"></param>
        /// <param name="petID"></param>
        /// <param name="joyful"></param>
        public static void AddPetJoyful(KPlayer player, int petID, int joyful)
        {
            /// Nếu là pet đang tham chiến
            if (player.CurrentPet != null && player.CurrentPet.RoleID - (int) ObjectBaseID.Pet == petID)
            {
                /// Pet tương ứng
                Pet pet = player.CurrentPet;

                /// Tăng điểm vui vẻ
                pet.Joyful += joyful;
                /// Nếu quá ngưỡng
                if (pet.Joyful > KPet.Config.MaxJoy)
                {
                    /// Thiết lập lại
                    pet.Joyful = KPet.Config.MaxJoy;
                }

                /// Thông báo
                KTPlayerManager.ShowNotification(player, "Tăng độ vui vẻ cho tinh linh thành công!");
                KTPlayerManager.ShowNotification(player, string.Format("Tinh linh <color=yellow>[{0}]</color> độ vui vẻ tăng lên <color=yellow>{1}</color>.", pet.RoleName, pet.Joyful));

                /// Dữ liệu pet của người chơi
                PetData data = player.PetList.Where(x => x.ID == petID).FirstOrDefault();
                /// Nếu tồn tại
                if (data != null)
                {
                    /// Lưu lại
                    data.Joyful = pet.Joyful;
                    /// Thông báo về Client
                    KT_TCPHandler.NotifyPetBaseAttributes(pet.Owner, data);
                }
            }
            /// Nếu không phải pet đang tham chiến
            else
            {
                /// Dữ liệu pet của người chơi
                PetData data = player.PetList.Where(x => x.ID == petID).FirstOrDefault();
                /// Nếu tồn tại
                if (data != null)
                {
                    /// Lưu lại
                    data.Joyful += joyful;
                    /// Nếu quá ngưỡng
                    if (data.Joyful > KPet.Config.MaxJoy)
                    {
                        /// Thiết lập lại
                        data.Joyful = KPet.Config.MaxJoy;
                    }
                    /// Thông báo về Client
                    KT_TCPHandler.NotifyPetBaseAttributes(player, data);
                    /// Thông báo
                    KTPlayerManager.ShowNotification(player, "Tăng độ vui vẻ cho tinh linh thành công!");
                    KTPlayerManager.ShowNotification(player, string.Format("Tinh linh <color=yellow>[{0}]</color> độ vui vẻ tăng lên <color=yellow>{1}</color>.", data.Name, data.Joyful));
                }
            }
        }

        /// <summary>
        /// Thêm tuổi thọ cho pet tương ứng
        /// </summary>
        /// <param name="player"></param>
        /// <param name="petID"></param>
        /// <param name="life"></param>
        public static void AddPetLife(KPlayer player, int petID, int life)
        {
            /// Nếu là pet đang tham chiến
            if (player.CurrentPet != null && player.CurrentPet.RoleID - (int) ObjectBaseID.Pet == petID)
            {
                /// Pet tương ứng
                Pet pet = player.CurrentPet;

                /// Tăng tuổi thọ
                pet.Life += life;
                /// Nếu quá ngưỡng
                if (pet.Life > KPet.Config.MaxLife)
                {
                    /// Thiết lập lại
                    pet.Life = KPet.Config.MaxLife;
                }

                /// Thông báo
                KTPlayerManager.ShowNotification(player, "Tăng tuổi thọ cho tinh linh thành công!");
                KTGlobal.SendDefaultChat(player, string.Format("Tinh linh <color=yellow>[{0}]</color> tuổi thọ tăng lên <color=yellow>{1}</color>.", pet.RoleName, pet.Life));

                /// Dữ liệu pet của người chơi
                PetData data = player.PetList.Where(x => x.ID == petID).FirstOrDefault();
                /// Nếu tồn tại
                if (data != null)
                {
                    /// Lưu lại
                    data.Life = pet.Life;
                    /// Thông báo về Client
                    KT_TCPHandler.NotifyPetBaseAttributes(pet.Owner, data);
                }
            }
            /// Nếu không phải pet đang tham chiến
            else
            {
                /// Dữ liệu pet của người chơi
                PetData data = player.PetList.Where(x => x.ID == petID).FirstOrDefault();
                /// Nếu tồn tại
                if (data != null)
                {
                    /// Lưu lại
                    data.Life += life;
                    /// Nếu quá ngưỡng
                    if (data.Life > KPet.Config.MaxLife)
                    {
                        /// Thiết lập lại
                        data.Life = KPet.Config.MaxLife;
                    }
                    /// Thông báo về Client
                    KT_TCPHandler.NotifyPetBaseAttributes(player, data);
                    /// Thông báo
                    KTPlayerManager.ShowNotification(player, "Tăng tuổi thọ cho tinh linh thành công!");
                    KTGlobal.SendDefaultChat(player, string.Format("Tinh linh <color=yellow>[{0}]</color> tuổi thọ tăng lên <color=yellow>{1}</color>.", data.Name, data.Life));
                }
            }
        }

        /// <summary>
        /// Thêm kinh nghiệm cho Pet tương ứng
        /// </summary>
        /// <param name="pet"></param>
        /// <param name="exp"></param>
        public static void AddPetExp(Pet pet, int exp)
        {
            /// Nếu không có chủ nhân
            if (pet.Owner == null)
            {
                /// Bỏ qua
                return;
            }

            /// Nếu Pet đã đạt giới hạn cấp
            if (pet.m_Level >= KPet.Config.MaxLevel || pet.m_Level >= KPet.Config.LevelUpExps.Count)
            {
                /// Thông báo
                KTGlobal.SendDefaultChat(pet.Owner, string.Format("Tinh linh <color=yellow>[{0}]</color> đã đạt cấp độ tối đa.", pet.RoleName));
                /// Bỏ qua
                return;
            }
            /// Nếu lớn hơn cấp của chủ nhân
            else if (pet.m_Level >= pet.Owner.m_Level)
            {
                /// Thông báo
                KTGlobal.SendDefaultChat(pet.Owner, string.Format("Tinh linh <color=yellow>[{0}]</color> đã đạt cấp độ tối đa.", pet.RoleName));
                /// Bỏ qua
                return;
            }

            /// Cấp độ cũ
            int oldLevel = pet.m_Level;
            /// Lượng kinh nghiệm còn tồn
            int expLeft = exp;

            /// Lặp liên tục
            do
            {
                /// Nếu Pet đã đạt giới hạn cấp
                if (pet.m_Level >= KPet.Config.MaxLevel || pet.m_Level >= KPet.Config.LevelUpExps.Count)
                {
                    /// Thoát
                    break;
                }
                /// Nếu lớn hơn cấp của chủ nhân
                else if (pet.m_Level >= pet.Owner.m_Level)
                {
                    /// Thoát
                    break;
                }

                /// Kinh nghiệm thăng cấp
                int maxLevelExp = KPet.Config.LevelUpExps[pet.m_Level - 1];
                /// Nếu vượt quá
                if (pet.m_Experience + expLeft >= maxLevelExp)
                {
                    /// Tăng cấp
                    pet.m_Level++;
                    /// Giảm lượng kinh nghiệm xuống ngưỡng tương ứng
                    expLeft -= (maxLevelExp - (int)pet.m_Experience);
                    /// Lưu lại lượng kinh nghiệm có
                    pet.m_Experience = 0;
                }
                /// Nếu chưa vượt quá
                else
                {
                    /// Thiết lập tổng kinh nghiệm cộng thêm
                    pet.m_Experience += expLeft;
                    /// Thoát
                    break;
                }
            }
            while (true);

            /// Nếu có thăng cấp
            if (oldLevel != pet.m_Level)
            {
                /// Ghi vào DB
                bool ret = KT_TCPHandler.SendDBUpdatePetLevelAndExp(pet.Owner, pet.RoleID - (int) ObjectBaseID.Pet, pet.m_Level, (int)pet.m_Experience);
                /// Toác
                if (!ret)
                {
                    KTPlayerManager.ShowNotification(pet.Owner, "Không thể cập nhật cấp độ tinh linh. Hãy liên hệ với hỗ trợ để báo lỗi!");
                    return;
                }

                /// Thông báo
                KTGlobal.SendDefaultChat(pet.Owner, string.Format("Tinh linh <color=yellow>[{0}]</color> nhận được <color=yellow>{1} kinh nghiệm</color>, thăng lên <color=yellow>cấp {2}</color>.", pet.RoleName, exp, pet.m_Level));
                /// Thực thi sự kiện thăng cấp
                pet.OnLevelUp(oldLevel);
            }
            /// Nếu chỉ nhận kinh nghiệm thường
            else
            {
                /// Thông báo
                KTGlobal.SendDefaultChat(pet.Owner, string.Format("Tinh linh <color=yellow>[{0}]</color> nhận được <color=yellow>{1}</color> kinh nghiệm.", pet.RoleName, exp));
            }

            /// Thực thi sự kiện nhận kinh nghiệm
            pet.OnEarnExp(exp);

            /// Dữ liệu pet của người chơi
            PetData data = pet.Owner.PetList.Where(x => x.ID == pet.RoleID - (int) ObjectBaseID.Pet).FirstOrDefault();
            /// Nếu tồn tại
            if (data != null)
            {
                /// Lưu lại
                data.Level = pet.m_Level;
                data.Exp = (int)pet.m_Experience;
                /// Thông báo về Client
                KT_TCPHandler.NotifyPetBaseAttributes(pet.Owner, data);
            }
        }

        /// <summary>
        /// Thực hiện thêm kinh nghiệm cho Pet của người chơi tương ứng khi giết quái
        /// </summary>
        /// <param name="player"></param>
        /// <param name="exp"></param>
        public static void ProcessPetExpGainByPlayerKillMonster(KPlayer player, int exp)
        {
            /// Nếu không có pet đang tham chiến
            if (player.CurrentPet == null)
            {
                /// Bỏ qua
                return;
            }
            /// Nếu pet đã chết
            else if (player.CurrentPet.IsDead())
            {
                /// Bỏ qua
                return;
            }

            /// Lượng kinh nghiệm chia sẻ cho pet
            int nExp = exp * KPet.Config.OwnerExpP / 100;
            /// Nếu không tồn tại
            if (nExp <= 0)
            {
                /// Thiết lập = 1
                nExp = 1;
            }

            /// Thêm kinh nghiệm cho Pet tương ứng
            KTPetManager.AddPetExp(player.CurrentPet, nExp);
        }
    }
}
