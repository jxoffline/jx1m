using FS.GameEngine.Logic;
using FS.GameEngine.Network;
using FS.VLTK.Logic.Settings;
using FS.VLTK.Network;
using Server.Data;
using System.Collections;
using System.Linq;
using UnityEngine;
using static FS.VLTK.Entities.Enum;

namespace FS.VLTK.Logic
{
    /// <summary>
    /// Tự bán đồ
    /// </summary>
    public partial class KTAutoFightManager
    {
        /// <summary>
        /// Đánh dấu có đang triệu hồi pet hay không
        /// </summary>
        public bool IsCallingPet { get; set; } = false;

        /// <summary>
        /// Thao tác với pet
        /// </summary>
        private IEnumerator AutoPet()
        {
            /// Nghỉ
            WaitForSeconds wait = new WaitForSeconds(0.5f);
            /// Thời gian gửi yêu cầu gọi Pet trước đó
            long lastTryCallingPetTicks = 0;
            /// Lặp liên tục
            while (true)
            {
                /// Nếu đang đợi gọi pet
                if (this.IsCallingPet)
                {
                    /// Bỏ qua
                    yield return null;
                }

                /// Nếu có pet đang tham chiến
                if (Global.Data.RoleData.CurrentPetID != -1 && Global.Data.SystemPets.TryGetValue(Global.Data.RoleData.CurrentPetID + (int) ObjectBaseID.Pet, out PetDataMini petData))
                {
                    /// Nếu có thiết lập tự dùng thuốc cho pet
                    if (KTAutoAttackSetting.Config.Support.AutoPetHP)
                    {
                        /// % sinh lực hiện tại
                        int hpPercent = (int) (petData.HP * 100 / (float) petData.MaxHP);
                        /// Nếu sinh lực dưới ngưỡng
                        if (hpPercent <= KTAutoAttackSetting.Config.Support.AutoPetHPPercent)
                        {
                            /// Nếu có vật phẩm thuốc
                            if (KTAutoAttackSetting.Config.Support.PetHPMedicine != -1)
                            {
                                /// Thuốc
                                GoodsData itemGD = Global.Data.RoleData.GoodsDataList?.Where(x => x.GoodsID == KTAutoAttackSetting.Config.Support.PetHPMedicine && x.GCount > 0).FirstOrDefault();
                                /// Nếu có thuốc trong túi người chơi
                                if (itemGD != null)
                                {
                                    /// Sử dụng vật phẩm
                                    GameInstance.Game.SpriteUseGoods(itemGD.Id, itemGD.GoodsID);
                                }
                            }
                        }
                    }
                }
                /// Nếu không có pet đang tham chiến
                else
                {
                    /// Nếu đang mở Auto và có thiết lập tự gọi Pet
                    if (this.IsAutoFighting && KTAutoAttackSetting.Config.Support.AutoCallPet)
                    {
                        /// ID pet
                        int petID = Global.Data.LastPetID;
                        /// Nếu không có
                        if (petID == -1)
                        {
                            /// Nếu có danh sách pet
                            if (Global.Data.RoleData.Pets != null)
                            {
                                /// Chọn pet đầu tiên
                                petID = Global.Data.RoleData.Pets.FirstOrDefault().ID;
                            }
                        }

                        /// Nếu có pet
                        if (petID != -1)
                        {
                            /// Thông tin pet
                            PetData _petData = Global.Data.RoleData.Pets?.Where(x => x.ID == petID).FirstOrDefault();
                            /// Nếu tồn tại
                            if (_petData != null)
                            {
                                /// Nếu tuổi thọ không đủ
                                if (_petData.Life < Loader.Loader.PetConfig.CallFightRequịreLifeOver)
                                {
                                    /// Tìm vật phẩm tăng tuổi thọ
                                    GoodsData itemGD = Global.Data.RoleData.GoodsDataList?.Where(x => x.GCount > 0 && Loader.Loader.PetConfig.FeedLifeItems.Keys.Contains(x.GoodsID)).FirstOrDefault();
                                    /// Nếu tìm thấy
                                    if (itemGD != null)
                                    {
                                        /// Gửi yêu cầu
                                        KT_TCPHandler.SendFeedPet(_petData.ID, 1);
                                    }
                                }
                                /// Nếu độ vui vẻ không đủ
                                else if (_petData.Joyful < Loader.Loader.PetConfig.CallFightRequịreJoyOver)
                                {
                                    /// Tìm vật phẩm tăng độ vui vẻ
                                    GoodsData itemGD = Global.Data.RoleData.GoodsDataList?.Where(x => x.GCount > 0 && Loader.Loader.PetConfig.FeedJoyItems.Keys.Contains(x.GoodsID)).FirstOrDefault();
                                    /// Nếu tìm thấy
                                    if (itemGD != null)
                                    {
                                        /// Gửi yêu cầu
                                        KT_TCPHandler.SendFeedPet(_petData.ID, 0);
                                    }
                                }
                                /// Thỏa mãn
                                else
                                {
                                    /// Nếu sau khoảng delay 10s mà vẫn chưa có pet thì mới gọi
                                    if (KTGlobal.GetCurrentTimeMilis() - lastTryCallingPetTicks >= 10000)
                                    {
                                        /// Thông báo
                                        KTGlobal.AddNotification(string.Format("Đang triệu hồi tinh linh [{0}].", _petData.Name));
                                        /// Ngừng di chuyển
                                        KTLeaderMovingManager.StopChasingTarget();
                                        KTLeaderMovingManager.StopMove();
                                        /// Gửi yêu cầu gọi pet
                                        KT_TCPHandler.SendDoPetCommand(_petData.ID, 1);
                                        /// Đánh dấu thời gian gửi yêu cầu gọi pet
                                        lastTryCallingPetTicks = KTGlobal.GetCurrentTimeMilis();
                                        /// Đánh dấu đợi gọi pet
                                        this.IsCallingPet = true;
                                    }
                                }
                            }
                        }
                    }
                }

                /// Nghỉ
                yield return wait;
            }
        }
    }
}
