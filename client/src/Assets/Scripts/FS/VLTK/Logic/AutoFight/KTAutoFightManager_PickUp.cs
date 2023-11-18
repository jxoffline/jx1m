using FS.GameEngine.GoodsPack;
using FS.GameEngine.Logic;
using FS.VLTK.Logic.Settings;
using System.Linq;
using UnityEngine;

namespace FS.VLTK.Logic
{
    /// <summary>
    /// Tự nhặt
    /// </summary>
    public partial class KTAutoFightManager
    {
        /// <summary>
        /// Làm rỗng danh sách vật phẩm không nhặt
        /// </summary>
        private void ResetListIgnoreGoodsPack()
        {
            /// Nếu chưa đến thời gian kiểm tra thì bỏ qua
            if (KTGlobal.GetCurrentTimeMilis() - this.AutoFightLastCheckClearListIgnoreGoodsPack < this.AutoFight_AutoClearListIgnoreGoodsPack)
            {
                return;
            }

            /// Đánh dấu thời gian tự xóa danh sách vật phẩm không nhặt
            this.AutoFightLastCheckClearListIgnoreGoodsPack = KTGlobal.GetCurrentTimeMilis();

            /// Làm rỗng danh sách
            this.ListIgnoreGoodsPack.Clear();
        }

        /// <summary>
        /// Thực thi tự nhặt vật phẩm rơi ở Map
        /// </summary>
        private GGoodsPack AutoPickUpItems()
        {
            /// Nếu thiết lập không nhặt vật phẩm
            if (!KTAutoAttackSetting.Config.PickAndSell.EnableAutoPickUp)
            {
                return null;
            }

            /// Nếu chưa đến thời gian kiểm tra thì bỏ qua
            if (KTGlobal.GetCurrentTimeMilis() - this.AutoFightLastCheckAutoPickUpItems < this.AutoFight_AutoPickUpItemsEveryTick)
            {
                return null;
            }

            /// Đánh dấu thời gian kiểm tra tự động nhặt vật phẩm
            this.AutoFightLastCheckAutoPickUpItems = KTGlobal.GetCurrentTimeMilis();

            if (Global.Data.RoleData.GoodsDataList != null)
            {
                int count = Global.Data.RoleData.GoodsDataList.Where(x => x.Using < 0).Count();

                /// Nếu đầy túi đồ thì đéo thèm nhặt
                if (count >= 100)
                {
                    return null;
                }
            }

            /// Tìm vật phẩm gần nhất xung quanh
            GGoodsPack goodsPack = Global.Data.GameScene.FindNearestGoodsPack((gp) =>
            {
                /// Nếu nằm trong danh sách không nhặt thì bỏ qua
                //if (this.ListIgnoreGoodsPack.Contains(gp.AutoID))
                //{
                //    return false;
                //}

                float Dis = Vector2.Distance(gp.PositionInVector2, Global.Data.Leader.PositionInVector2);

                // Nếu như là thiết lập train quanh điểm
                if (KTAutoAttackSetting.Config.Farm.FarmAround)
                {
                    float Dis2 = Vector2.Distance(gp.PositionInVector2, this.StartPos);

                    if (Dis2 > KTAutoAttackSetting.Config.Farm.ScanRange)
                    {
                        return false;
                    }
                }

                /// Nếu vật phẩm nằm ngoài range nhặt thì không nhặt
                if (Dis > KTAutoAttackSetting.Config.PickAndSell.ScanRadius)
                {
                    return false;
                }

                /// Nếu đây là huyền tinh thì
                if (KTAutoAttackSetting.IsCrytalItem(gp.GoodsID))
                {
                    /// Xem có tích vào rule lọc huyền tinh không
                    if (KTAutoAttackSetting.Config.PickAndSell.PickUpCrystalStoneOnly)
                    {
                        /// Thì check xem huyền tinh có thỏa mãn cấp không thì mới nhặt
                        if (KTAutoAttackSetting.IsCanPickCrytalItem(gp.GoodsID, KTAutoAttackSetting.Config.PickAndSell.PickUpCrystalStoneLevel))
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }

                if (KTAutoAttackSetting.IsEquip(gp.GoodsID))
                {
                    /// Nếu auto thiết lập nhặt trang bị với số sao
                    if (KTAutoAttackSetting.Config.PickAndSell.PickUpEquip)
                    {  /// Nếu số sao thỏa mãn thì ko nhặt
                        if (gp.Stars >= KTAutoAttackSetting.Config.PickAndSell.PickUpEquipStar && gp.LinesCount >= KTAutoAttackSetting.Config.PickAndSell.PickUpItemByLinesCount)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }

                if (KTAutoAttackSetting.Config.PickAndSell.PickUpOtherItems)
                {  /// Nếu số sao thỏa mãn thì ko nhặt
                    if (!KTAutoAttackSetting.IsEquip(gp.GoodsID) && !KTAutoAttackSetting.IsCrytalItem(gp.GoodsID))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }

                /// Nếu không thì bú tất
            });

            /// Trả về vật phẩm tìm được
            return goodsPack;
        }
    }
}