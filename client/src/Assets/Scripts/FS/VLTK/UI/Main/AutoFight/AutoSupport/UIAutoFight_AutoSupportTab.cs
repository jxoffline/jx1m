using FS.GameEngine.Logic;
using FS.VLTK.Entities.Config;
using FS.VLTK.Logic;
using FS.VLTK.UI.Main.ItemBox;
using FS.VLTK.Utilities.UnityUI;
using Server.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace FS.VLTK.UI.Main.AutoFight
{
    /// <summary>
    /// Khung thiết lập Auto hỗ trợ
    /// </summary>
    public class UIAutoFight_AutoSupportTab : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Toggle tự dùng thuốc phục hòi sinh lực
        /// </summary>
        [SerializeField]
        private UIToggleSprite UIToggle_AutoHP;

        /// <summary>
        /// Toggle tự dùng thuốc phục hòi sinh lực
        /// </summary>
        [SerializeField]
        private UIToggleSprite UIToggle_AuToX2;

        /// <summary>
        /// Item thuốc phục hồi sinh lực
        /// </summary>
        [SerializeField]
        private UIItemBox UIItem_HPMedicine;

        /// <summary>
        /// Button chọn % sinh lực tự dùng thuốc
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_SelectHPPercent;

        /// <summary>
        /// Text % sinh lực tự dùng thuốc
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_AutoHPPercent;

        /// <summary>
        /// Toggle tự dùng thuốc phục hồi nội lực
        /// </summary>
        [SerializeField]
        private UIToggleSprite UIToggle_AutoMP;

        /// <summary>
        /// Item thuốc phục hồi nội lực
        /// </summary>
        [SerializeField]
        private UIItemBox UIItem_MPMedicine;

        /// <summary>
        /// Button chọn % nội lực tự dùng thuốc
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_SelectMPPercent;

        /// <summary>
        /// Text % nội lực tự dùng thuốc
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_AutoMPPercent;

        /// <summary>
        /// Toggle tự mua thuốc
        /// </summary>
        [SerializeField]
        private UIToggleSprite UIToggle_AutoBuyMedicines;

        /// <summary>
        /// Button chọn số lượng thuốc sẽ mua mỗi lần
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_AutoBuyMedicinesQuantity;

        /// <summary>
        /// Text số lượng thuốc sẽ mua mỗi lần
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_AutoBuyMedicinesQuantity;

        /// <summary>
        /// Toggle tự mua thuốc ưu tiên sử dụng bạc khóa
        /// </summary>
        [SerializeField]
        private UIToggleSprite UIToggle_AutoBuyMedicineUsingBoundMoneyPriority;

        /// <summary>
        /// Toggle Nga My tự dùng Từ Hàng Phổ Độ cho đồng đội
        /// </summary>
        [SerializeField]
        private UIToggleSprite UIToggle_EMAutoHeal;

        /// <summary>
        /// Button chọn % sinh lực đồng đội để tự dùng Từ Hàng Phổ Độ
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_SelectEMAutoHealHPPercent;

        /// <summary>
        /// Text % sinh lực đồng đội để tự dùng Từ Hàng Phổ Độ
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_EMAutoHealHPPercent;

        /// <summary>
        /// Toggle tự dùng thuốc phục hòi sinh lực cho pet
        /// </summary>
        [SerializeField]
        private UIToggleSprite UIToggle_AutoPetHP;

        /// <summary>
        /// Item thuốc phục hồi sinh lực cho pet
        /// </summary>
        [SerializeField]
        private UIItemBox UIItem_PetHPMedicine;

        /// <summary>
        /// Button chọn % sinh lực cho pet tự dùng thuốc
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_SelectPetHPPercent;

        /// <summary>
        /// Text % sinh lực cho pet tự dùng thuốc
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_AutoPetHPPercent;

        /// <summary>
        /// Toggle tự dùng vật phẩm tăng độ vui vẻ cho pet khi dưới ngưỡng tham chiến
        /// </summary>
        [SerializeField]
        private UIToggleSprite UIToggle_AutoPetJoy;

        /// <summary>
        /// Toggle tự dùng vật phẩm tăng độ tuổi thọ cho pet khi dưới ngưỡng tham chiến
        /// </summary>
        [SerializeField]
        private UIToggleSprite UIToggle_AutoPetLife;

        /// <summary>
        /// Toggle tự triệu hồi pet khi chết
        /// </summary>
        [SerializeField]
        private UIToggleSprite UIToggle_AutoCallPet;
        #endregion

        #region Properties
        /// <summary>
        /// Khung gốc
        /// </summary>
        public UIAutoFight Parent { get; set; }

        /// <summary>
        /// Tự dùng thuốc phục hồi sinh lực
        /// </summary>
        public bool AutoHP
        {
            get
            {
                return this.UIToggle_AutoHP.Active;
            }
            set
            {
                this.UIToggle_AutoHP.Active = value;
            }
        }


        /// <summary>
        /// Tự cắn X2 khi có X2 trong túi đồ
        /// </summary>
        public bool AutoX2
        {
            get
            {
                return this.UIToggle_AuToX2.Active;
            }
            set
            {
                this.UIToggle_AuToX2.Active = value;
            }
        }


        private int _AutoHPPercent;
        /// <summary>
        /// % sinh lực tự dùng thuốc
        /// </summary>
        public int AutoHPPercent
        {
            get
            {
                return this._AutoHPPercent;
            }
            set
            {
                this._AutoHPPercent = value;
                this.UIText_AutoHPPercent.text = string.Format("{0}%", value);
            }
        }

        /// <summary>
        /// Tự dùng thuốc phục hồi nội lực
        /// </summary>
        public bool AutoMP
        {
            get
            {
                return this.UIToggle_AutoMP.Active;
            }
            set
            {
                this.UIToggle_AutoMP.Active = value;
            }
        }

        private int _AutoMPPercent;
        /// <summary>
        /// % nội lực tự dùng thuốc
        /// </summary>
        public int AutoMPPercent
        {
            get
            {
                return this._AutoMPPercent;
            }
            set
            {
                this._AutoMPPercent = value;
                this.UIText_AutoMPPercent.text = string.Format("{0}%", value);
            }
        }

        /// <summary>
        /// Tự mua thuốc
        /// </summary>
        public bool AutoBuyMedicines
        {
            get
            {
                return this.UIToggle_AutoBuyMedicines.Active;
            }
            set
            {
                this.UIToggle_AutoBuyMedicines.Active = value;
            }
        }

        private int _AutoBuyMedicinesQuantity;
        /// <summary>
        /// Số lượng thuốc tự mua mỗi lần
        /// </summary>
        public int AutoBuyMedicinesQuantity
        {
            get
            {
                return this._AutoBuyMedicinesQuantity;
            }
            set
            {
                this._AutoBuyMedicinesQuantity = value;
                this.UIText_AutoBuyMedicinesQuantity.text = value.ToString();
            }
        }

        /// <summary>
        /// Tự mua thuốc ưu tiên dùng bạc khóa
        /// </summary>
        public bool AutoBuyMedicinesUsingBoundMoneyPriority
        {
            get
            {
                return this.UIToggle_AutoBuyMedicineUsingBoundMoneyPriority.Active;
            }
            set
            {
                this.UIToggle_AutoBuyMedicineUsingBoundMoneyPriority.Active = value;
            }
        }

        /// <summary>
        /// Tự dùng Từ Hàng Phổ Độ
        /// </summary>
        public bool EM_AutoHeal
        {
            get
            {
                return this.UIToggle_EMAutoHeal.Active;
            }
            set
            {
                this.UIToggle_EMAutoHeal.Active = value;
            }
        }

        private int _EM_AutoHealHPPercent;
        /// <summary>
        /// % sinh lực đồng đội tự dùng Từ Hàng Phổ Độ
        /// </summary>
        public int EM_AutoHealHPPercent
        {
            get
            {
                return this._EM_AutoHealHPPercent;
            }
            set
            {
                this._EM_AutoHealHPPercent = value;
                this.UIText_EMAutoHealHPPercent.text = string.Format("{0}%", value);
            }
        }

        private int _HPMedicineID = -1;
        /// <summary>
        /// ID thuốc hồi sinh lực
        /// </summary>
        public int HPMedicineID
        {
            get
            {
                return this._HPMedicineID;
            }
            set
            {
                this._HPMedicineID = value;

                /// Xóa thông tin vật phẩm tương ứng
                this.UIItem_HPMedicine.Data = null;

                /// Thông tin thuốc tương ứng
                if (!Loader.Loader.Items.TryGetValue(value, out ItemData itemData))
                {
                    return;
                }

                /// Tạo vật phẩm
                GoodsData itemGD = KTGlobal.CreateItemPreview(itemData);
                itemGD.Binding = 1;

                /// Đổ dữ liệu vào ô
                this.UIItem_HPMedicine.Data = itemGD;
            }
        }

        private int _MPMedicineID = -1;
        /// <summary>
        /// ID thuốc hồi nội lực
        /// </summary>
        public int MPMedicineID
        {
            get
            {
                return this._MPMedicineID;
            }
            set
            {
                this._MPMedicineID = value;

                /// Xóa thông tin vật phẩm tương ứng
                this.UIItem_MPMedicine.Data = null;

                /// Thông tin thuốc tương ứng
                if (!Loader.Loader.Items.TryGetValue(value, out ItemData itemData))
                {
                    return;
                }

                /// Tạo vật phẩm
                GoodsData itemGD = KTGlobal.CreateItemPreview(itemData);
                itemGD.Binding = 1;

                /// Đổ dữ liệu vào ô
                this.UIItem_MPMedicine.Data = itemGD;
            }
        }

        /// <summary>
        /// Tự dùng thuốc phục hồi sinh lực cho pet
        /// </summary>
        public bool AutoPetHP
        {
            get
            {
                return this.UIToggle_AutoPetHP.Active;
            }
            set
            {
                this.UIToggle_AutoPetHP.Active = value;
            }
        }

        private int _PetHPMedicineID = -1;
        /// <summary>
        /// ID thuốc hồi sinh lực cho pet
        /// </summary>
        public int PetHPMedicineID
        {
            get
            {
                return this._PetHPMedicineID;
            }
            set
            {
                this._PetHPMedicineID = value;

                /// Xóa thông tin vật phẩm tương ứng
                this.UIItem_PetHPMedicine.Data = null;

                /// Thông tin thuốc tương ứng
                if (!Loader.Loader.Items.TryGetValue(value, out ItemData itemData))
                {
                    return;
                }

                /// Tạo vật phẩm
                GoodsData itemGD = KTGlobal.CreateItemPreview(itemData);
                itemGD.Binding = 1;

                /// Đổ dữ liệu vào ô
                this.UIItem_PetHPMedicine.Data = itemGD;
            }
        }

        private int _AutoPetHPPercent;
        /// <summary>
        /// % sinh lực tự dùng thuốc
        /// </summary>
        public int AutoPetHPPercent
        {
            get
            {
                return this._AutoPetHPPercent;
            }
            set
            {
                this._AutoPetHPPercent = value;
                this.UIText_AutoPetHPPercent.text = string.Format("{0}%", value);
            }
        }

        /// <summary>
        /// Tự dùng vật phẩm tăng độ vui vẻ cho pet
        /// </summary>
        public bool AutoPetJoy
        {
            get
            {
                return this.UIToggle_AutoPetJoy.Active;
            }
            set
            {
                this.UIToggle_AutoPetJoy.Active = value;
            }
        }

        /// <summary>
        /// Tự dùng vật phẩm tăng tuổi thọ cho pet
        /// </summary>
        public bool AutoPetLife
        {
            get
            {
                return this.UIToggle_AutoPetLife.Active;
            }
            set
            {
                this.UIToggle_AutoPetLife.Active = value;
            }
        }

        /// <summary>
        /// Tự dùng vật phẩm tăng tuổi thọ cho pet
        /// </summary>
        public bool AutoCallPet
        {
            get
            {
                return this.UIToggle_AutoCallPet.Active;
            }
            set
            {
                this.UIToggle_AutoCallPet.Active = value;
            }
        }
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi ở Frame đầu tiên
        /// </summary>
        private void Start()
        {
            this.InitPrefabs();
        }
        #endregion

        #region Code UI
        /// <summary>
        /// Khởi tạo ban đầu
        /// </summary>
        private void InitPrefabs()
        {
            this.UIButton_SelectHPPercent.onClick.AddListener(() =>
            {
                this.Button_Clicked("Nhập % sinh lực tự dùng thuốc.", (value) =>
                {
                    this.AutoHPPercent = value;
                });
            });
            this.UIButton_SelectMPPercent.onClick.AddListener(() =>
            {
                this.Button_Clicked("Nhập % nội lực tự dùng thuốc.", (value) =>
                {
                    this.AutoMPPercent = value;
                });
            });
            this.UIButton_AutoBuyMedicinesQuantity.onClick.AddListener(() =>
            {
                /// Mở khung nhập số lượng
                KTGlobal.ShowInputNumber("Nhập số lượng thuốc tự mua mỗi lần", 1, 10000, (value) =>
                {
                    this.AutoBuyMedicinesQuantity = value;
                });
            });
            this.UIButton_SelectEMAutoHealHPPercent.onClick.AddListener(() =>
            {
                /// Tên kỹ năng
                string skillName = "Chưa rõ";
                /// Thông tin kỹ năng
                if (Loader.Loader.Skills.TryGetValue(KTAutoFightManager.EM_HealHPSkillID, out SkillDataEx skillData))
                {
                    skillName = skillData.Name;
                }
                this.Button_Clicked(string.Format("Nhập % sinh lực đồng đội để tự dùng <color=yellow>[{0}]</color>.", skillName), (value) =>
                {
                    this.EM_AutoHealHPPercent = value;
                });
            });
            this.UIButton_SelectPetHPPercent.onClick.AddListener(() =>
            {
                this.Button_Clicked("Nhập % sinh lực tinh linh tự dùng thuốc.", (value) =>
                {
                    this.AutoPetHPPercent = value;
                });
            });

            this.UIItem_HPMedicine.Click = this.ButtonHPMedicine_Clicked;
            this.UIItem_MPMedicine.Click = this.ButtonMPMedicine_Clicked;
            this.UIItem_PetHPMedicine.Click = this.ButtonPetHPMedicine_Clicked;
        }

        /// <summary>
        /// Sự kiện khi Button được ấn
        /// </summary>
        /// <param name="text"></param>
        /// <param name="callback"></param>
        private void Button_Clicked(string text, Action<int> callback)
        {
            /// Mở khung nhập số
            KTGlobal.ShowInputNumber(text, 0, 100, callback);
        }

        /// <summary>
		/// Sự kiện khi Button chọn thuốc hồi sinh lực được ấn
		/// </summary>
		private void ButtonPetHPMedicine_Clicked()
        {
            /// Danh sách thuốc
            List<GoodsData> items = new List<GoodsData>();
            /// Duyệt danh sách vật phẩm tương ứng
            foreach (ItemData itemData in KTGlobal.ListPetHPMedicines.Values)
            {
                /// Thông tin vật phẩm tương ứng
                GoodsData itemGD = Global.Data.RoleData.GoodsDataList?.Where(x => x.GoodsID == itemData.ItemID).FirstOrDefault();
                /// Nếu có vật phẩm tương ứng trong người
                if (itemGD != null)
                {
                    items.Add(itemGD);
                }
            }

            /// Hiện khung chọn thuốc
            this.Parent.ShowSelectItem(items, (itemGD) => {
                this.PetHPMedicineID = itemGD.GoodsID;
            });
        }

        /// <summary>
		/// Sự kiện khi Button chọn thuốc hồi sinh lực được ấn
		/// </summary>
		private void ButtonHPMedicine_Clicked()
        {
            /// Danh sách thuốc
            List<GoodsData> items = new List<GoodsData>();
            /// Duyệt danh sách vật phẩm tương ứng
            foreach (ItemData itemData in KTGlobal.ListHPMedicines.Values)
            {
                /// Thông tin vật phẩm tương ứng
                GoodsData itemGD = Global.Data.RoleData.GoodsDataList?.Where(x => x.GoodsID == itemData.ItemID).FirstOrDefault();
                /// Nếu có vật phẩm tương ứng trong người
                if (itemGD != null)
                {
                    items.Add(itemGD);
                }
            }

            /// Hiện khung chọn thuốc
            this.Parent.ShowSelectItem(items, (itemGD) => {
                this.HPMedicineID = itemGD.GoodsID;
            });
        }

        /// <summary>
        /// Sự kiện khi Button chọn thuốc hồi nội lực được ấn
        /// </summary>
        private void ButtonMPMedicine_Clicked()
        {
            /// Danh sách thuốc
            List<GoodsData> items = new List<GoodsData>();
            /// Duyệt danh sách vật phẩm tương ứng
            foreach (ItemData itemData in KTGlobal.ListMPMedicines.Values)
            {
                /// Thông tin vật phẩm tương ứng
                GoodsData itemGD = Global.Data.RoleData.GoodsDataList?.Where(x => x.GoodsID == itemData.ItemID).FirstOrDefault();
                /// Nếu có vật phẩm tương ứng trong người
                if (itemGD != null)
                {
                    items.Add(itemGD);
                }
            }

            /// Hiện khung chọn thuốc
            this.Parent.ShowSelectItem(items, (itemGD) => {
                this.MPMedicineID = itemGD.GoodsID;
            });
        }
        #endregion
    }
}
