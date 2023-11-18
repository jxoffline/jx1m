using FS.GameEngine.Logic;
using FS.GameEngine.Network;
using FS.GameEngine.Sprite;
using FS.VLTK.Entities.Config;
using FS.VLTK.Logic.Settings;
using Server.Data;
using System.Collections.Generic;
using System.Linq;

namespace FS.VLTK.Logic
{
    /// <summary>
    /// Tự dùng thuốc
    /// </summary>
    public partial class KTAutoFightManager
    {
        /// <summary>
        /// Tự dùng thuốc
        /// </summary>
        private void AutoUseMedicine()
        {
            /// Nếu không có thiết lập tự dùng thuốc thì bỏ qua
            if (!KTAutoAttackSetting.Config.Support.AutoHP && !KTAutoAttackSetting.Config.Support.AutoMP)
            {
                return;
            }
            /// Nếu chưa đến thời gian kiểm tra thì bỏ qua
            else if (KTGlobal.GetCurrentTimeMilis() - this.AutoFightLastCheckUseMedicineTick < this.AutoFight_AutoUseMedicineEveryTick)
            {
                return;
            }
            /// Nếu không có danh sách vật phẩm
            else if (Global.Data.RoleData.GoodsDataList == null)
			{
                return;
			}

            /// Đánh dấu thời gian kiểm tra tự dùng thuốc
            this.AutoFightLastCheckUseMedicineTick = KTGlobal.GetCurrentTimeMilis();

            /// Đối tượng người chơi
            GSprite leader = Global.Data.Leader;
            /// Nếu Loader NULL
            if (leader == null)
			{
                return;
			}

            /// % sinh nội lực
            int hpPercent = leader.HP * 100 / leader.HPMax;
            int mpPercent = leader.MP * 100 / leader.MPMax;

            /// Thuốc sinh lực
            Loader.Loader.Items.TryGetValue(KTAutoAttackSetting.Config.Support.HPMedicine, out ItemData hpMedicine);
            /// Thuốc nội lực
            Loader.Loader.Items.TryGetValue(KTAutoAttackSetting.Config.Support.MPMedicine, out ItemData mpMedicine);

            /// Nếu không có thiết lập thuốc phục hồi sinh lực, nội lực
            if (hpMedicine == null && mpMedicine == null)
            {
                if (KTGlobal.GetCurrentTimeMilis() - this.AutoFightLastNotifyNoMedicineTick >= this.AutoFight_NotifyNoMedicineTick)
                {
                    this.AutoFightLastNotifyNoMedicineTick = KTGlobal.GetCurrentTimeMilis();
                   // KTGlobal.AddNotification("Chưa thiết lập thuốc phục hồi sinh nội lực!");
                }
                return;
            }

            /// Đánh dấu có thuốc trong túi không
            bool hpMedicineFound = true;
            bool mpMedicineFound = true;

            GoodsData hpMedicineGD = null;
            GoodsData mpMedicineGD = null;

            /// Nếu lượng sinh lực dưới mức thiết lập tự dùng thuốc
            if (KTAutoAttackSetting.Config.Support.AutoHP && hpPercent <= KTAutoAttackSetting.Config.Support.AutoHPPercent)
            {
                /// Nếu tồn tại thuốc hồi sinh lực
                if (hpMedicine != null)
                {
                    /// Thuốc
                    GoodsData itemGD = Global.Data.RoleData.GoodsDataList.Where(x => x.GoodsID == hpMedicine.ItemID && x.GCount > 0).FirstOrDefault();
                    /// Nếu có thuốc trong túi người chơi
                    if (itemGD != null)
                    {
                        /// Sử dụng vật phẩm
                        //GameInstance.Game.SpriteUseGoods(itemGD.Id, itemGD.GoodsID);
                        hpMedicineGD = itemGD;
                    }
                    else
                    {
                        hpMedicineFound = false;
                    }
                }
            }

            /// Nếu lượng nội lực dưới mức thiết lập tự dùng thuốc
            if (KTAutoAttackSetting.Config.Support.AutoMP && mpPercent <= KTAutoAttackSetting.Config.Support.AutoMPPercent)
            {
                /// Nếu tồn tại thuốc hồi sinh lực
                if (mpMedicine != null)
                {
                    /// Thuốc
                    GoodsData itemGD = Global.Data.RoleData.GoodsDataList.Where(x => x.GoodsID == mpMedicine.ItemID && x.GCount > 0).FirstOrDefault();
                    /// Nếu có thuốc trong túi người chơi
                    if (itemGD != null)
                    {
                        /// Sử dụng vật phẩm
                        //GameInstance.Game.SpriteUseGoods(itemGD.Id, itemGD.GoodsID);
                        mpMedicineGD = itemGD;
                    }
                    else
                    {
                        mpMedicineFound = false;
                    }
                }
            }

            /// Nếu có thuốc hồi sinh lực
            if (hpMedicineGD != null)
			{
                /// Nếu có thuốc hồi nội lực
                if (mpMedicineGD != null)
				{
                    /// Sử dụng thuốc
                    GameInstance.Game.SpriteUseGoods(hpMedicineGD.Id, mpMedicineGD.Id);
				}
				/// Nếu không có thuốc hồi nội lực
				else
				{
                    /// Sử dụng thuốc
                    GameInstance.Game.SpriteUseGoods(hpMedicineGD.Id);
                }
			}
            /// Nếu có thuốc hồi nội lực
            else if (mpMedicineGD != null)
			{
                /// Sử dụng thuốc
                GameInstance.Game.SpriteUseGoods(mpMedicineGD.Id);
            }

            /// Nếu không tìm thấy thuốc
            if ((KTAutoAttackSetting.Config.Support.AutoHP && !hpMedicineFound) || (KTAutoAttackSetting.Config.Support.AutoMP && !mpMedicineFound))
            {
                /// Nếu đã đến thời gian thông báo
                if (KTGlobal.GetCurrentTimeMilis() - this.AutoFightLastNotifyNoMedicineTick >= this.AutoFight_NotifyNoMedicineTick)
                {
                    this.AutoFightLastNotifyNoMedicineTick = KTGlobal.GetCurrentTimeMilis();

                    List<string> medicineNames = new List<string>();
                    if (!hpMedicineFound)
                    {
                        medicineNames.Add(hpMedicine.Name);
                    }
                    if (!mpMedicineFound)
                    {
                        medicineNames.Add(mpMedicine.Name);
                    }
                    KTGlobal.AddNotification(string.Format("Đã hết [{0}], hãy mua thêm!", string.Join(", ", medicineNames)));
                }
            }
        }

        /// <summary>
        /// Tự động bú X2
        /// </summary>
        private void AutoEatX2()
        {
            /// Nếu không có thiết lập tự dùng thuốc thì bỏ qua
            if (!KTAutoAttackSetting.Config.Support.AutoX2)
            {
                return;
            }
            /// Nếu chưa đến thời gian kiểm tra thì bỏ qua
            else if (KTGlobal.GetCurrentTimeMilis() - this.AutoFightLastCheckEatX2 < this.AutoFight_AutoEatX2Tick)
            {
                return;
            }
            /// Nếu như đang có bufff trên người
            else if (Global.Data.RoleData.BufferDataList != null)
            {
                var CheckBuffExist = Global.Data.RoleData.BufferDataList.Where(x => x.BufferID == 1997 || x.BufferID == 1998).FirstOrDefault();
                // Nếu như đang có buff này trên người rồi thì thôi ko cần ăn thêm nữa
                if(CheckBuffExist!=null)
                {
                    /// Đánh dấu lại thời gian gần đây nhất đã kiểm tra
                    this.AutoFightLastCheckEatX2 = KTGlobal.GetCurrentTimeMilis();
                    return;
                }    
               
            }

            /// Đánh dấu thời gian kiểm tra tự dùng thuốc
            this.AutoFightLastCheckEatX2 = KTGlobal.GetCurrentTimeMilis();

            ///// Đối tượng người chơi
            GSprite leader = Global.Data.Leader;
            /// Nếu Loader NULL
            if (leader == null)
            {
                return;
            }

         
            // Tìm ra tiên thảo nộ thường
            GoodsData FindX2 = Global.Data.RoleData.GoodsDataList?.Where(x => x.GoodsID == 8483 && x.GCount > 0).FirstOrDefault();

            if(FindX2 == null)
            {
                FindX2 = Global.Data.RoleData.GoodsDataList?.Where(x => x.GoodsID == 9594 && x.GCount > 0).FirstOrDefault();
            }    


            // Nếu như có tiên thảo lộ trong người
            if(FindX2!=null)
            {
                Loader.Loader.Items.TryGetValue(FindX2.GoodsID, out ItemData X2Item);

                KTGlobal.AddNotification("Tự sử dụng :" + X2Item.Name);
                // Thực hiện bú x2
                GameInstance.Game.SpriteUseGoods(FindX2.Id);
            }    

         
        }
    }
}
