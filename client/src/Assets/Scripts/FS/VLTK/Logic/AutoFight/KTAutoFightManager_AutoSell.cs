using FS.GameEngine.Logic;
using FS.GameEngine.Network;
using FS.GameEngine.Sprite;
using FS.VLTK.Entities.Config;
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
    /// Tự bán đồ
    /// </summary>
    public partial class KTAutoFightManager
    {
        /// <summary>
        /// Đang thực hiện tự bán đồ
        /// </summary>
        public bool DoingAutoSell { get; set; } = false;

        /// <summary>
        /// Có đang thực hiện mua vật phẩm hay không
        /// </summary>
        public bool DoingBuyItem { get; set; } = false;

        /// <summary>
        /// Tự động bán đồ
        /// </summary>
        /// <returns></returns>
        private IEnumerator DoSellItem()
        {
            /// Nếu thiết lập không tự bán vật phẩm thì thôi
            if (!KTAutoAttackSetting.Config.PickAndSell.AutoSellItems)
            {
                /// Bỏ đánh dấu bán
                this.DoingAutoSell = false;
                yield break;
            }

            /// Nếu chưa đến thời gian kiểm tra
            if (KTGlobal.GetCurrentTimeMilis() - this.AutoFightLastCheckAutoSell < this.AutoFight_CheckSellFullItemEveryTick)
            {
                /// Bỏ đánh dấu bán
                this.DoingAutoSell = false;
                yield break;
            }

            /// Thời gian gầy đây nhất check túi đồ xem đầy hay chưa
            this.AutoFightLastCheckAutoSell = KTGlobal.GetCurrentTimeMilis();

            /// Nếu tồn tại danh sách vật phẩm
            if (Global.Data.RoleData.GoodsDataList != null)
            {
                /// Số lượng đang có
                int count = Global.Data.RoleData.GoodsDataList.Count(x => x.Using < 0);
                /// Nếu túi đồ chưa đầy thì thôi
                if (count < 100)
                {
                    /// Bỏ đánh dấu bán
                    this.DoingAutoSell = false;
                    yield break;
                }
            }
            else
            {
                this.DoingAutoSell = false;
                yield break;
            }

            /// Đánh dấu đang tự bán vật phẩm
            this.DoingAutoSell = true;

            int MapCodeBack = Global.Data.RoleData.MapCode;
            int XPostion = Global.Data.RoleData.PosX;
            int YPostion = Global.Data.RoleData.PosY;

            SELLSTATE = AUTOSELLSTATE.NONE;

            /// Tạo ra 1 cái temp đồ sẽ bán để tránh bán quá nhanh trong LABADA
            List<GoodsData> TmpGoodWillBeSoul = new List<GoodsData>();

            /// Lặp liên tục
            while (this.DoingAutoSell)
            {
                KTGlobal.AddNotification("Đang thực hiện bán đồ");
                /// Thực hiện CLICK này
                if (SELLSTATE == AUTOSELLSTATE.NONE)
                {
                    SELLSTATE = AUTOSELLSTATE.GOTONPC;

                    KTGlobal.QuestAutoFindPathToNPC(-1, 1966, () =>
                    {
                        AutoPathManager.Instance.StopAutoPath();

                        /// Duyệt 1 vòng để lấy ra toàn bộ các vật phẩm có thể bán
                        foreach (GoodsData Data in Global.Data.RoleData.GoodsDataList)
                        {
                            /// Nếu vật phẩm đang sử dụng bỏ qua
                            if (Data.Using >= 0)
                            {
                                continue;
                            }

                            /// Nếu vật phẩm đã cường hóa bỏ qua
                            if (Data.Forge_level > 0)
                            {
                                continue;
                            }

                            /// nếu vật phẩm là trang bị
                            if (!KTGlobal.IsEquip(Data.GoodsID))
                            {
                                continue;
                            }

                            /// Nếu vật phẩm không thể bán thì bỏ qua
                            if (!KTGlobal.IsCanBeSold(Data))
                            {
                                continue;
                            }

                            StarLevelStruct starInfo = KTGlobal.StarCalculation(Data);
                            float starLevel = starInfo.StarLevel / 2f;

                            /// Nếu vật phẩm thỏa mãn điều kiện bán thì sẽ add vào danh sách bán
                            if (starLevel < KTAutoAttackSetting.Config.PickAndSell.SellEquipStar)
                            {
                                TmpGoodWillBeSoul.Add(Data);
                            }
                        }

                        SELLSTATE = AUTOSELLSTATE.CLICKSELL;
                    });
                }

                if (SELLSTATE == AUTOSELLSTATE.CLICKSELL)
                {
                    /// Chuyển trạng thái sang start SELL
                    SELLSTATE = AUTOSELLSTATE.STARTSELL;

                    /// Nếu có danh sách vật phẩm cần bán thì ta sẽ loop để bán lần lượt
                    if (TmpGoodWillBeSoul.Count > 0)
                    {
                        /// Duyệt 1 vòng và thực hiện bán
                        for (int i = 0; i < TmpGoodWillBeSoul.Count; i++)
                        {
                            GoodsData _SelectItem = TmpGoodWillBeSoul[i];

                            /// Check null phát nữa cho nó chắc
                            if (_SelectItem != null)
                            {
                                KTGlobal.AddNotification("[" + DateTime.Now.ToString() + "] Bán vật phẩm: " + KTGlobal.GetItemName(_SelectItem) + " thành công!");
                                //Thực hiện bán vật phẩm này
                                GameInstance.Game.SpriteBuyOutGoodsEx(_SelectItem.Id);
                            }

                            /// Nếu đang bán mà nhân vật di chuyển là cũng toác
                            if (!this.DoingAutoSell)
                            {
                                break;
                            }
                            // Mỗi vòng for nghỉ 1s mới bán cho nó đỡ cost gs
                            yield return new WaitForSeconds(1f);
                        }

                        /// Nếu có config tự động sắp xếp lại túi đồ thì thực hiện sắp xếp
                        if (KTAutoAttackSetting.Config.PickAndSell.AutoSortBag)
                        {
                            GameInstance.Game.SpriteSortBag();
                        }
                    }

                    /// Nếu check 1 lần nữa mà đéo có vật phẩm nào có thể bán tức là trong túi đồ toàn đồ không thể bán thực hiện FORCE tắt nhặt đồ ở SETTINGS
                    if (Global.Data.RoleData.GoodsDataList.Count >= 100)
                    {
                        /// Force tắt chức năng bán vật phẩm
                        KTAutoAttackSetting.Config.PickAndSell.AutoSellItems = false;
                    }

                    SELLSTATE = AUTOSELLSTATE.MOVEBACK;
                }

                if (SELLSTATE == AUTOSELLSTATE.MOVEBACK)
                {
                    SELLSTATE = AUTOSELLSTATE.END;

                    /// Tự tìm đường về chỗ cũ
                    KTGlobal.QuestAutoFindPath(MapCodeBack, XPostion, YPostion, () =>
                    {
                        AutoPathManager.Instance.StopAutoPath();

                        /// Set lại trạng thái đã bán xong
                        this.DoingAutoSell = false;

                        this.StopAutoFight();
                        this.StartAuto();

                        this.StartPos = new Vector2(XPostion, YPostion);
                    });
                }

                if (!this.DoingAutoSell)
                {
                    SELLSTATE = AUTOSELLSTATE.END;
                    /// Thoát lặp
                    break;
                }
                /// Chờ 1 giây mới thực hiện vòng lặp
                yield return new WaitForSeconds(1f);
            }
        }

        /// <summary>
        /// Thực hiện mua vật phẩm
        /// </summary>
        /// <returns></returns>
        private IEnumerator DoBuyItem()
        {
            /// Nếu thiết lập không tự bán vật phẩm thì thôi
            if (!KTAutoAttackSetting.Config.Support.AutoBuyMedicines)
            {
                /// Bỏ đánh dấu bán
                this.DoingBuyItem = false;
                yield break;
            }

            /// Nếu chưa đến thời gian kiểm tra
            if (KTGlobal.GetCurrentTimeMilis() - this.AutoFightLastCheckBuyItem < this.AutoFight_CheckBuyItemEveryTick)
            {
                /// Bỏ đánh dấu bán
                this.DoingBuyItem = false;
                yield break;
            }

            /// Thời gian gầy đây nhất check túi đồ xem đầy hay chưa
            this.AutoFightLastCheckBuyItem = KTGlobal.GetCurrentTimeMilis();

            /// Nếu tồn tại danh sách vật phẩm

            GSprite leader = Global.Data.Leader;
            /// Nếu Loader NULL
            if (leader == null)
            {
                this.DoingBuyItem = false;
                yield break;
            }



            /// Thuốc sinh lực
            Loader.Loader.Items.TryGetValue(KTAutoAttackSetting.Config.Support.HPMedicine, out ItemData hpMedicine);
            /// Thuốc nội lực
            Loader.Loader.Items.TryGetValue(KTAutoAttackSetting.Config.Support.MPMedicine, out ItemData mpMedicine);

            /// Nếu đéo thiết lập thuốc nào thì thôi
            if (hpMedicine == null && mpMedicine == null)
            {
                this.DoingBuyItem = false;
                yield break;
            }

            // Đếm xem có bao nhiêu máu trong người
            int CountHp = 0;
            int CountMP = 0;

            if (hpMedicine != null)
            {
                CountHp = Global.Data.RoleData.GoodsDataList.Where(x => x.GoodsID == hpMedicine.ItemID).Sum(x => (int?)x.GCount) ?? 0;
            }

            if (mpMedicine != null)
            {
                CountMP = Global.Data.RoleData.GoodsDataList.Where(x => x.GoodsID == mpMedicine.ItemID).Sum(x => (int?)x.GCount) ?? 0;
            }
            // Đém xem có bao nhiêu mana trong người

            if (Global.Data.RoleData.GoodsDataList != null)
            {
                int count = Global.Data.RoleData.GoodsDataList.Where(x => x.Using < 0).Count();

                /// Nếu đầy túi đồ thì đéo thèm nhặt
                if (count >= 100)
                {
                    KTGlobal.AddNotification("Trên túi đồ không còn chỗ trống để quay về mua thuốc");
                    this.DoingBuyItem = false;
                    yield break;
                }
            }

            int MoneyType = 0;
            // Nếu như 1 trong 2 cái mà hết
            if ((hpMedicine != null && CountHp == 0) || (CountMP == 0) && mpMedicine != null)
            {
                // nếu như số lượng cần mua lớn hơn 0
                if (KTAutoAttackSetting.Config.Support.AutoBuyMedicinesQuantity > 0)
                {
                    int TotalMoney = hpMedicine.Price * KTAutoAttackSetting.Config.Support.AutoBuyMedicinesQuantity;

                    // Nếu mà sử dụng bạc khóa
                    if (KTAutoAttackSetting.Config.Support.AutoBuyMedicinesUsingBoundMoneyPriority == true)
                    {
                        if (Global.Data.RoleData.BoundMoney < TotalMoney)
                        {
                            KTGlobal.AddNotification("Bạc khóa trên người không đủ để tự quay về mua thuốc");
                            this.DoingBuyItem = false;
                            yield break;
                        }

                        // Kiểm tra xem bạc khóa còn không
                    }
                    else
                    {
                        MoneyType = 1;
                        if (Global.Data.RoleData.Money < TotalMoney)
                        {
                            KTGlobal.AddNotification("Bạc trên người không đủ để tự quay về mua thuốc");
                            this.DoingBuyItem = false;
                            yield break;
                        }
                    }
                }

                ///Đánh dấu là cần mua
                this.DoingBuyItem = true;
            }
            else
            {
                // Nếu đéo cần mua thì thôi
                this.DoingBuyItem = false;
                yield break;
            }

            int MapCodeBack = Global.Data.RoleData.MapCode;
            int XPostion = Global.Data.RoleData.PosX;
            int YPostion = Global.Data.RoleData.PosY;

            BUYSTATE = AUTOSELLSTATE.NONE;

            /// Lặp liên tục
            while (this.DoingBuyItem && BUYSTATE != AUTOSELLSTATE.END)
            {
                KTGlobal.AddNotification("Đang thực hiện quay về mua thuốc");
                /// Thực hiện CLICK này
                if (BUYSTATE == AUTOSELLSTATE.NONE)
                {
                    BUYSTATE = AUTOSELLSTATE.GOTONPC;

                    KTGlobal.QuestAutoFindPathToNPC(-2, 205, () =>
                    {
                        AutoPathManager.Instance.StopAutoPath();
                        // Sau khi tìm tới nơi thì chuyển state sang bắt đầu

                        BUYSTATE = AUTOSELLSTATE.CLICKSELL;
                    });
                }

                if (BUYSTATE == AUTOSELLSTATE.CLICKSELL)
                {
                    KTGlobal.AddNotification("Thực hiện mua thuốc");
                    /// Chuyển trạng thái sang start SELL
                    BUYSTATE = AUTOSELLSTATE.STARTSELL;

                    if (hpMedicine != null && CountHp==0)
                    {
                        GameInstance.Game.AutoBuyMachine(hpMedicine.ItemID, KTAutoAttackSetting.Config.Support.AutoBuyMedicinesQuantity, MoneyType);
                    }

                    if (mpMedicine != null && CountMP == 0)
                    {
                        GameInstance.Game.AutoBuyMachine(mpMedicine.ItemID, KTAutoAttackSetting.Config.Support.AutoBuyMedicinesQuantity, MoneyType);
                    }

                    yield return new WaitForSeconds(1f);

                    /// Nếu có config tự động sắp xếp lại túi đồ thì thực hiện sắp xếp
                    if (KTAutoAttackSetting.Config.PickAndSell.AutoSortBag)
                    {
                        GameInstance.Game.SpriteSortBag();
                    }

                    // Nếu mà sau khi mua xong vẫn là 0 thì tức là toác
                    if (hpMedicine != null)
                    {
                        CountHp = Global.Data.RoleData.GoodsDataList.Where(x => x.GoodsID == hpMedicine.ItemID).Sum(x => (int?)x.GCount) ?? 0;

                        if (CountHp == 0)
                        {
                            KTAutoAttackSetting.Config.Support.AutoBuyMedicines = false;
                        }
                    }

                    // nếu như có thiết lập tự bú mp
                    if (mpMedicine != null)
                    {
                        CountMP = Global.Data.RoleData.GoodsDataList.Where(x => x.GoodsID == mpMedicine.ItemID).Sum(x => (int?)x.GCount) ?? 0;
                        if (CountMP == 0)
                        {
                            KTAutoAttackSetting.Config.Support.AutoBuyMedicines = false;
                        }
                    }
                    // Check j đó ở đây để ko quay về mua 1 cái vô hạn

                    BUYSTATE = AUTOSELLSTATE.MOVEBACK;
                }

                if (BUYSTATE == AUTOSELLSTATE.MOVEBACK)
                {
                    KTGlobal.AddNotification("Thực hiện quay về bãi");
                    BUYSTATE = AUTOSELLSTATE.END;

                    /// Tự tìm đường về chỗ cũ
                    KTGlobal.QuestAutoFindPath(MapCodeBack, XPostion, YPostion, () =>
                    {
                        KTGlobal.AddNotification("Thực hiện quay về bãi...");
                        AutoPathManager.Instance.StopAutoPath();

                        /// Set lại trạng thái đã bán xong
                        this.DoingBuyItem = false;

                        this.StopAutoFight();
                        this.StartAuto();

                        this.StartPos = new Vector2(XPostion, YPostion);
                    });
                }

                if (!this.DoingBuyItem)
                {
                    BUYSTATE = AUTOSELLSTATE.END;
                    /// Thoát lặp
                    break;
                }
                /// Chờ 1 giây mới thực hiện vòng lặp
                yield return new WaitForSeconds(1f);
            }
        }

        /// <summary>
        /// Thoát việc tự bán nếu có sự di chuyển của người chơi
        /// </summary>
        public void StopAutoSell()
        {
            this.DoingAutoSell = false;
        }

        public void StopAutoBuyItem()
        {
            this.DoingBuyItem = false;
        }
    }
}