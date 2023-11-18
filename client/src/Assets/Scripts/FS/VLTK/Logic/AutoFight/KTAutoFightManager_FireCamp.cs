using FS.GameEngine.Logic;
using FS.GameEngine.Network;
using FS.GameEngine.Sprite;
using FS.VLTK.Entities.Config;
using FS.VLTK.Factory;
using FS.VLTK.Logic.Settings;
using Server.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FS.VLTK.Logic
{
    /// <summary>
    /// Tự đốt lửa trại
    /// </summary>
    public partial class KTAutoFightManager
    {
        /// <summary>
        /// Res ID đống củi
        /// </summary>
        private const int FireWoodResID = 20021;

        /// <summary>
        /// ID Res lửa trại
        /// </summary>
        private const int FireCampResID = 20022;

        /// <summary>
        /// Đánh dấu đang tự đốt lửa trại
        /// </summary>
        private bool DoingAutoFireCamp = false;

        /// <summary>
        /// Thời điểm lần trước xóa đống củi khỏi danh sách bỏ qua
        /// </summary>
        private long lastTickRemoveFireCampWoodsFromIgnoreList = 0;

        /// <summary>
        /// Tự xóa đống củi bị đánh dấu bỏ qua khỏi danh sách
        /// </summary>
        private void RemoveFireCampWoodsFromIgnoreList()
        {
            if (KTGlobal.GetCurrentTimeMilis() - this.lastTickRemoveFireCampWoodsFromIgnoreList >= 10000)
            {
                return;
            }
            this.lastTickRemoveFireCampWoodsFromIgnoreList = KTGlobal.GetCurrentTimeMilis();

            /// Xóa
            this.ignoredFireCamps.Clear();
        }

        /// <summary>
        /// Tìm đống củi gần nhất
        /// </summary>
        /// <returns></returns>
        private GSprite FindNearestFireCampWood(Predicate<GSprite> predicate)
        {
            /// Nếu dữ liệu rỗng
            if (Global.Data == null || Global.Data.Leader == null)
            {
                return null;
            }

            Vector2 leaderPos = Global.Data.Leader.PositionInVector2;
            return KTObjectsManager.Instance.FindObjects<GSprite>((sprite) =>
            {
                if (sprite.GPData == null)
                {
                    return false;
                }
                /// Nếu không thỏa mãn điều kiện
                if (!predicate(sprite))
				{
                    return false;
				}

                /// Nếu quá phạm vi
                if (Vector2.Distance(leaderPos, sprite.PositionInVector2) > KTAutoAttackSetting.Config.Farm.ScanRange)
                {
                    /// Bỏ qua
                    return false;
                }

                return sprite.GPData.ResID == KTAutoFightManager.FireWoodResID;
            }).MinBy((sprite) =>
            {
                Vector2 gpPos = sprite.PositionInVector2;
                return Vector2.Distance(leaderPos, gpPos);
            });
        }

        /// <summary>
        /// Tìm lửa trại gần nhất
        /// </summary>
        /// <returns></returns>
        private GSprite FindNearestFireCamp(Predicate<GSprite> predicate)
		{
            /// Nếu dữ liệu rỗng
            if (Global.Data == null || Global.Data.Leader == null)
            {
                return null;
            }

            Vector2 leaderPos = Global.Data.Leader.PositionInVector2;
            return KTObjectsManager.Instance.FindObjects<GSprite>((sprite) =>
            {
                if (sprite.DynAreaData == null)
                {
                    return false;
                }
                /// Nếu không thỏa mãn điều kiện
                if (!predicate(sprite))
                {
                    return false;
                }

                return sprite.DynAreaData.ResID == KTAutoFightManager.FireCampResID;
            }).MinBy((sprite) =>
            {
                Vector2 gpPos = sprite.PositionInVector2;
                return Vector2.Distance(leaderPos, gpPos);
            });
        }

        /// <summary>
        /// Tự uống rượu
        /// </summary>
        private void AutoDrinkWine()
		{
            /// Nếu không có thiết lập tự uống rượu thì bỏ qua
            if (!KTAutoAttackSetting.Config.Farm.AutoDrinkWine)
            {
                return;
            }
            /// Nếu chưa đến thời gian kiểm tra thì bỏ qua
            else if (KTGlobal.GetCurrentTimeMilis() - this.AutoFightLastCheckDrinkWine < this.AutoFight_AutoDrinkWineEveryTick)
            {
                return;
            }

            /// Đánh dấu thời gian kiểm tra tự dùng thức ăn
            this.AutoFightLastCheckDrinkWine = KTGlobal.GetCurrentTimeMilis();

            /// Nếu đang có Buff rượu thì bỏ qua
            if (Global.Data.RoleData.BufferDataList != null && Global.Data.RoleData.BufferDataList.Where(x => x.BufferID == KTGlobal.WineBuffID).FirstOrDefault() != null)
            {
                return;
            }
            /// Nếu không có danh sách vật phẩm
            else if (Global.Data.RoleData.GoodsDataList == null)
            {
                return;
            }

            /// Nếu không tìm thấy lửa trại thì thôi
            if (this.FindNearestFireCamp(x => x != null) == null)
			{
                return;
			}

            /// Rượu được chọn
            GoodsData wineSelected = null;
            string wineName = "";
            /// Xem trong túi người chơi có rượu gì
            foreach (GoodsData itemGD in Global.Data.RoleData.GoodsDataList)
            {
                /// Nếu tìm thấy rượu trong túi người chơi
                if (itemGD.GCount > 0 && KTGlobal.ListWines.TryGetValue(itemGD.GoodsID, out ItemData itemData))
                {
                    wineSelected = itemGD;
                    wineName = itemData.Name;
                }
            }

            /// Nếu tìm thấy thức ăn
            if (wineSelected != null)
            {
                KTGlobal.AddNotification(string.Format("Tự dùng rượu - {0}", wineName));

                /// Nếu vật phẩm không thể sử dụng
                GameInstance.Game.SpriteUseGoods(wineSelected.Id);
            }
            else if (KTGlobal.GetCurrentTimeMilis() - this.AutoFightLastNotifyNoWineTick >= this.AutoFight_NotifyNoWineTick)
            {
                this.AutoFightLastNotifyNoWineTick = KTGlobal.GetCurrentTimeMilis();
                KTGlobal.AddNotification("Không tìm thấy rượu!");
            }
        }

        /// <summary>
        /// Tự đốt lửa trại
        /// </summary>
        /// <param name="ignoredList"></param>
        /// <param name="processed"></param>
        /// <returns></returns>
        private IEnumerator AutoFireCamp(List<GSprite> ignoredList, Action<GSprite> processed)
        {
            /// Nếu thiết lập không đốt lửa trại thì thôi
            if (!KTAutoAttackSetting.Config.Farm.AutoFireCamp)
            {
                /// Bỏ đánh dấu đang tự đốt lửa trại
                this.DoingAutoFireCamp = false;

                yield break;
            }

            /// Nếu đang đốt rồi thì thôi
            if (this.DoingAutoFireCamp && PlayZone.Instance.IsProgressBarVisible())
			{
                yield break;
			}

            /// Nếu chưa đến thời gian kiểm tra
            if (KTGlobal.GetCurrentTimeMilis() - this.AutoFight_CheckNearbyFireCampEveryTick < this.AutoFightLastCheckNearbyFireCamp)
            {
                /// Bỏ đánh dấu đang tự đốt lửa trại
                this.DoingAutoFireCamp = false;

                yield break;
            }
            /// Thiết lập thời gian kiểm tra
            this.AutoFightLastCheckNearbyFireCamp = KTGlobal.GetCurrentTimeMilis();

            /// Đánh dấu đang tự đốt lửa trại
            this.DoingAutoFireCamp = true;

            /// Lửa trại tương ứng
            GSprite fireCamp = this.FindNearestFireCampWood(x => !ignoredList.Contains(x));
            /// Nếu không tồn tại thì bỏ qua
            if (fireCamp == null)
            {
                /// Bỏ đánh dấu đang tự đốt lửa trại
                this.DoingAutoFireCamp = false;
                yield break;
            }

            /// Thực hiện hàm Callback
            processed?.Invoke(fireCamp);
            /// Thực hiện đốt lửa trại
            Global.Data.GameScene.GrowPointClick(fireCamp);

            /// Thời gian bắt đầu đợi
            long waitTicks = KTGlobal.GetCurrentTimeMilis();
            /// Chừng nào thanh Progress Bar chưa hiện thì còn đợi (vì chưa có đốt được)
            while (!PlayZone.Instance.IsProgressBarVisible())
			{
                /// Nếu đợi quá 2s thì thôi
                if (KTGlobal.GetCurrentTimeMilis() - waitTicks >= 2000)
                {
                    yield break;
                }
                /// Bỏ qua Frame
                yield return null;
			}

            /// Chừng nào vẫn còn củi chưa đốt
            while (KTObjectsManager.Instance.FindObject<GSprite>(fireCamp.BaseID) != null)
            {
                /// Nếu không đánh dấu đang tự đốt lửa trại
                if (!this.DoingAutoFireCamp)
                {
                    /// Thoát lặp
                    break;
                }
                /// Nếu vì lý do nào đó mà GS nó hủy thì sẽ mất thanh ProgressBar
                if (!PlayZone.Instance.IsProgressBarVisible())
				{
                    /// Thoát lặp
                    break;
				}

                /// Bỏ qua Frame
                yield return null;
            }

            /// Đợi tầm 1s
            yield return new WaitForSeconds(1f);

            /// Chạy lại Auto cho chắc
            this.StopAutoFight();
            this.StartAuto(true);
        }

        /// <summary>
        /// Tạm dừng tự đốt lửa trại
        /// </summary>
        public void StopAutoFireCamp()
        {
            this.DoingAutoFireCamp = false;
        }
    }
}
