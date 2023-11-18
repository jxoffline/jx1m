using FS.GameEngine.Logic;
using FS.VLTK.Control.Component;
using FS.VLTK.Factory;
using FS.VLTK.Network;
using FS.VLTK.UI.Main.ItemBox;
using FS.VLTK.Utilities.UnityUI;
using Server.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace FS.VLTK.UI.Main.RoleInfo
{
    /// <summary>
    /// Khung thông tin nhân vật
    /// </summary>
    public class UIRoleInfo : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Button đóng khung
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_Close;

        /// <summary>
        /// Image Avarta nhân vật
        /// </summary>
        [SerializeField]
        private UIRoleAvarta UIImage_Avarta;

        /// <summary>
        /// Text Tên nhân vật
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_RoleName;

        /// <summary>
        /// Text Môn phái nhân vật
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_RoleFaction;

        /// <summary>
        /// Text Vinh dự võ lâm
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_WorldHonorPoint;

        /// <summary>
        /// Text Vinh dự tài phú
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_PerfHonorPoint;

        /// <summary>
        /// Text Cấp độ nhân vật
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_RoleLevel;

        /// <summary>
        /// Text Ngũ hành
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_ElementalText;

        /// <summary>
        /// Text Trị PK
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_PKValue;

        /// <summary>
        /// Ô trang bị Mặt nạ
        /// </summary>
        [SerializeField]
        private UIItemBox UIItemBox_Mask;

        /// <summary>
        /// Ô trang bị Mũ
        /// </summary>
        [SerializeField]
        private UIItemBox UIItemBox_Hat;

        /// <summary>
        /// Ô trang bị Áo
        /// </summary>
        [SerializeField]
        private UIItemBox UIItemBox_Armor;

        /// <summary>
        /// Ô trang bị Lưng
        /// </summary>
        [SerializeField]
        private UIItemBox UIItemBox_Belt;

        /// <summary>
        /// Ô trang bị Tay
        /// </summary>
        [SerializeField]
        private UIItemBox UIItemBox_Wristband;

        /// <summary>
        /// Ô trang bị Giày
        /// </summary>
        [SerializeField]
        private UIItemBox UIItemBox_Shoe;

        /// <summary>
        /// Ô trang bị Ngựa
        /// </summary>
        [SerializeField]
        private UIItemBox UIItemBox_Horse;

        /// <summary>
        /// Ô trang bị Ngũ hành Ấn
        /// </summary>
        [SerializeField]
        private UIItemBox UIItemBox_FiveElementSeal;

        /// <summary>
        /// Ô trang bị Trang sức
        /// </summary>
        [SerializeField]
        private UIItemBox UIItemBox_Ornament;

        /// <summary>
        /// Ô trang bị Phù
        /// </summary>
        [SerializeField]
        private UIItemBox UIItemBox_Amulet;

        /// <summary>
        /// Ô trang bị Nang/Bội
        /// </summary>
        [SerializeField]
        private UIItemBox UIItemBox_Pendant;

        /// <summary>
        /// Ô trang bị Nhẫn
        /// </summary>
        [SerializeField]
        private UIItemBox UIItemBox_Ring;

        /// <summary>
        /// Ô trang bị Nhẫn 2
        /// </summary>
        [SerializeField]
        private UIItemBox UIItemBox_Ring2;

        /// <summary>
        /// Ô trang bị Phi phong
        /// </summary>
        [SerializeField]
        private UIItemBox UIItemBox_Coat;

        /// <summary>
        /// Ô trang bị Vũ khí
        /// </summary>
        [SerializeField]
        private UIItemBox UIItemBox_Weapon;

        /// <summary>
        /// Image ảnh chiếu của Camera lên đối tượng nhân vật
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.RawImage UIImage_RolePreview;

        /// <summary>
        /// Button đổi trang bị
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_ChangeEquip;

        /// <summary>
        /// Toggle hiện trang bị dự phòng
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Toggle UIToggle_SubSet;

        /// <summary>
        /// Tab Panel
        /// </summary>
        [SerializeField]
        private UITabPanel UITabPanel;

        /// <summary>
        /// Tab Thuộc tính
        /// </summary>
        [SerializeField]
        private UIRoleInfo_PropertiesTab UITab_Properties;

        /// <summary>
        /// Tab Thông tin người chơi
        /// </summary>
        [SerializeField]
        private UIRoleInfo_AttributesTab UITab_Attributes;

        /// <summary>
        /// Tab danh vọng
        /// </summary>
        [SerializeField]
        private UIRoleInfo_ReputesTab UITab_Reputes;

        /// <summary>
        /// Tab danh hiệu
        /// </summary>
        [SerializeField]
        private UIRoleInfo_TitlesTab UITab_Titles;
        #endregion

        #region Private fields
        /// <summary>
        /// Đối tượng đang được chiếu
        /// </summary>
        private CharacterPreview characterPreview = null;
        #endregion

        #region Properties
        /// <summary>
        /// Sự kiện khi khung bị đóng
        /// </summary>
        public Action Close { get; set; }

        /// <summary>
        /// Sự kiện mở khung chọn Avarta nhân vật
        /// </summary>
        public Action OpenSelectAvarta { get; set; }

        private RoleAttributes _RoleAttributes;
        /// <summary>
        /// Thuộc tính nhân vật (ở khung thuộc tính và khung cộng điểm tiềm năng)
        /// </summary>
        public RoleAttributes RoleAttributes
        {
            get
            {
                return this._RoleAttributes;
            }
            set
            {
                this._RoleAttributes = value;

                this.RefreshPropertiesTabData();
            }
        }

        /// <summary>
        /// Gỡ trang bị
        /// </summary>
        public Action<GoodsData> Unequip { get; set; }

        /// <summary>
        /// Quảng bá
        /// </summary>
        public Action<GoodsData> Advertise { get; set; }

        /// <summary>
        /// Sự kiện đổi set dự phòng
        /// </summary>
        public Action ChangeSubSet { get; set; }

        /// <summary>
        /// Sự kiện thiết lập danh hiệu nhân vật hiện tại
        /// </summary>
        public Action<int> SetAsCurrentRoleTitle { get; set; }
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi ở Frame đầu tiên
        /// </summary>
        private void Start()
        {
            this.InitPrefabs();
            IEnumerator DoNextFrame()
            {
                yield return null;
                this.UpdateRoleData();
                this.RefreshEquips();
            }
            this.StartCoroutine(DoNextFrame());

            /// Thiết lập Set dự phòng
            this.UIToggle_SubSet.isOn = Global.Data.ShowReserveEquip;
        }

        /// <summary>
        /// Hàm này gọi khi đối tượng bị xóa khỏi hệ thống
        /// </summary>
        private void OnDestroy()
        {
            if (this.characterPreview != null)
            {
                GameObject.Destroy(this.characterPreview.gameObject);
            }
        }
        #endregion

        #region Code UI
        /// <summary>
        /// Khởi tạo ban đầu
        /// </summary>
        private void InitPrefabs()
        {
            this.UIButton_Close.onClick.AddListener(this.ButtonClose_Clicked);

            this.UITabPanel.SelectTab = (tab) =>
            {
                this.TabItem_Selected(tab);
            };

            this.UIImage_Avarta.RoleID = Global.Data.RoleData.RoleID;
            this.UIImage_Avarta.Click = this.ButtonAvarta_Clicked;
            this.UIToggle_SubSet.onValueChanged.AddListener((isSelected) =>
            {
                /// Thiết lập Set dự phòng
                Global.Data.ShowReserveEquip = isSelected;
                this.RefreshEquips();
            });
            this.UIButton_ChangeEquip.onClick.AddListener(this.ButtonChangeSubSet_Clicked);

            this.UITab_Titles.SetAsCurrentTitle = this.SetAsCurrentRoleTitle;
        }

        /// <summary>
        /// Sự kiện khi Button Avarta nhân vật được ấn
        /// </summary>
        private void ButtonAvarta_Clicked()
        {
            this.OpenSelectAvarta?.Invoke();
        }

        /// <summary>
        /// Sự kiện khi Button đóng khung được ấn
        /// </summary>
        private void ButtonClose_Clicked()
        {
            this.Close?.Invoke();
        }

        /// <summary>
        /// Sự kiện khi Tab được chọn
        /// </summary>
        /// <param name="tab"></param>
        private void TabItem_Selected(Utilities.UnityUI.UITab tab)
        {
            if (tab.Content == this.UITab_Properties.gameObject)
            {
                this.InitPropertiesTabData();
            }
            else if (tab.Content == this.UITab_Attributes.gameObject)
            {
                this.InitAttributesTabData();
            }
            else if (tab.Content == this.UITab_Reputes.gameObject)
            {
                this.InitReputesTabData();
            }
            else if (tab.Content == this.UITab_Titles.gameObject)
            {
                this.InitTitlesTabData();
            }
        }

        /// <summary>
        /// Sự kiện khi Click vào ô trang bị
        /// </summary>
        /// <param name="uiItemBox"></param>
        private void ItemBox_Clicked(UIItemBox uiItemBox)
        {
            /// Nếu trong ô không có trang bị
            if (uiItemBox.Data == null)
            {
                return;
            }

            List<KeyValuePair<string, Action>> buttons = new List<KeyValuePair<string, Action>>();
            buttons.Add(new KeyValuePair<string, Action>("Tháo xuống", () =>
            {
                this.Unequip(uiItemBox.Data);
            }));
            buttons.Add(new KeyValuePair<string, Action>("Quảng bá", () =>
            {
                this.Advertise(uiItemBox.Data);
            }));

            /// Hiển thị ToolTip thông tin trang bị kèm Buttons tương ứng
            KTGlobal.ShowItemInfo(uiItemBox.Data, buttons);
        }

        /// <summary>
        /// Sự kiện khi Button đổi trang bị được ấn
        /// </summary>
        private void ButtonChangeSubSet_Clicked()
        {
            this.ChangeSubSet?.Invoke();
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Làm mới thông tin thuộc tính nhân vật
        /// </summary>
        private void RefreshPropertiesTabData()
        {
            if (!this.UITab_Properties.gameObject.activeSelf)
            {
                return;
            }
            if (this._RoleAttributes == null)
            {
                return;
            }

            /// Thuộc tính trái
            this.UITab_Properties.CritAtk = this._RoleAttributes.Crit;
            this.UITab_Properties.Hit = this._RoleAttributes.Hit;
            this.UITab_Properties.Dodge = this._RoleAttributes.Dodge;
            this.UITab_Properties.AtkSpeed = this._RoleAttributes.AtkSpeed;
            this.UITab_Properties.CastSpeed = this._RoleAttributes.CastSpeed;
            this.UITab_Properties.MoveSpeed = this._RoleAttributes.MoveSpeed;
            /// Thuộc tính phải
            this.UITab_Properties.Damage = this._RoleAttributes.Damage;
            this.UITab_Properties.Def = this._RoleAttributes.Def;
            this.UITab_Properties.IceRes = this._RoleAttributes.IceRes;
            this.UITab_Properties.FireRes = this._RoleAttributes.FireRes;
            this.UITab_Properties.LightningRes = this._RoleAttributes.LightningRes;
            this.UITab_Properties.PoisonRes = this._RoleAttributes.PoisonRes;
            this.UITab_Properties.RemainPoint = this._RoleAttributes.RemainPoint;

            /// Mở khung tiềm năng
            this.UITab_Properties.OpenRoleRemainPointFrame = () =>
            {
                if (PlayZone.Instance.UIRoleRemainPoint == null)
                {
                    PlayZone.Instance.ShowUIRoleRemainPoint();
                }

                PlayZone.Instance.UIRoleRemainPoint.Str = this._RoleAttributes.Str;
                PlayZone.Instance.UIRoleRemainPoint.Dex = this._RoleAttributes.Dex;
                PlayZone.Instance.UIRoleRemainPoint.Sta = this._RoleAttributes.Sta;
                PlayZone.Instance.UIRoleRemainPoint.Int = this._RoleAttributes.Int;
                PlayZone.Instance.UIRoleRemainPoint.RemainPoint = this._RoleAttributes.RemainPoint;
                PlayZone.Instance.UIRoleRemainPoint.Accept = (nStr, nDex, nSta, nInt) =>
                {

                };
            };
        }

        /// <summary>
        /// Thiết lập dữ liệu Tab Thuộc tính
        /// </summary>
        private void InitPropertiesTabData()
        {
            KT_TCPHandler.SendGetRoleAttributes();
        }

        /// <summary>
        /// Thiết lập dữ liệu Tab Danh vọng
        /// </summary>
        private void InitReputesTabData()
        {

        }

        /// <summary>
        /// Thiết lập dữ liệu Tab Danh hiệu
        /// </summary>
        private void InitTitlesTabData()
        {

        }

        /// <summary>
        /// Thiết lập dữ liệu Tab Thuộc tính khác
        /// </summary>
        private void InitAttributesTabData()
        {
            this.UITab_Attributes.Properties = this._RoleAttributes.OtherProperties;
            this.UITab_Attributes.Build();
        }

        /// <summary>
        /// Làm mới chiếu nhân vật
        /// </summary>
        /// <param name="roleData"></param>
        private void RefreshRolePreview(RoleDataMini roleData)
        {
            if (this.characterPreview == null)
            {
                this.characterPreview = Object2DFactory.MakeRolePreview();
            }
            this.characterPreview.Data = roleData;
            this.characterPreview.UpdateRoleData();

            this.characterPreview.Direction = Entities.Enum.Direction.DOWN;
            this.characterPreview.OnStart = () =>
            {
                this.UIImage_RolePreview.texture = this.characterPreview.ReferenceCamera.targetTexture;
            };
            this.characterPreview.ResumeCurrentAction();
        }

        /// <summary>
        /// Làm mới hiển thị trang bị
        /// </summary>
        private void RefreshEquips()
        {
            /// Tạo mới đối tượng RoleDataMini
            RoleDataMini roleData = new RoleDataMini()
            {
                RoleID = Global.Data.RoleData.RoleID,
                RoleSex = Global.Data.RoleData.RoleSex,
                ArmorID = -1,
                HelmID = -1,
                WeaponID = -1,
                MantleID = -1,
                WeaponEnhanceLevel = 0,
            };

            /// Làm rỗng các ô đồ trước
            this.UIItemBox_Weapon.Data = null;
            this.UIItemBox_Ring2.Data = null;
            this.UIItemBox_Ring.Data = null;
            this.UIItemBox_Pendant.Data = null;
            this.UIItemBox_Amulet.Data = null;
            this.UIItemBox_Hat.Data = null;
            this.UIItemBox_Armor.Data = null;
            this.UIItemBox_Belt.Data = null;
            this.UIItemBox_Wristband.Data = null;
            this.UIItemBox_Shoe.Data = null;
            this.UIItemBox_Coat.Data = null;
            this.UIItemBox_Horse.Data = null;
            this.UIItemBox_FiveElementSeal.Data = null;
            this.UIItemBox_Ornament.Data = null;
            this.UIItemBox_Mask.Data = null;

            /// Bắt sự kiện Click
            this.UIItemBox_Weapon.Click = () =>
            {
                this.ItemBox_Clicked(this.UIItemBox_Weapon);
            };
            this.UIItemBox_Ring2.Click = () =>
            {
                this.ItemBox_Clicked(this.UIItemBox_Ring2);
            };
            this.UIItemBox_Ring.Click = () =>
            {
                this.ItemBox_Clicked(this.UIItemBox_Ring);
            };
            this.UIItemBox_Pendant.Click = () =>
            {
                this.ItemBox_Clicked(this.UIItemBox_Pendant);
            };
            this.UIItemBox_Amulet.Click = () =>
            {
                this.ItemBox_Clicked(this.UIItemBox_Amulet);
            };
            this.UIItemBox_Hat.Click = () =>
            {
                this.ItemBox_Clicked(this.UIItemBox_Hat);
            };
            this.UIItemBox_Armor.Click = () =>
            {
                this.ItemBox_Clicked(this.UIItemBox_Armor);
            };
            this.UIItemBox_Belt.Click = () =>
            {
                this.ItemBox_Clicked(this.UIItemBox_Belt);
            };
            this.UIItemBox_Wristband.Click = () =>
            {
                this.ItemBox_Clicked(this.UIItemBox_Wristband);
            };
            this.UIItemBox_Shoe.Click = () =>
            {
                this.ItemBox_Clicked(this.UIItemBox_Shoe);
            };
            this.UIItemBox_Coat.Click = () =>
            {
                this.ItemBox_Clicked(this.UIItemBox_Coat);
            };
            this.UIItemBox_Horse.Click = () =>
            {
                this.ItemBox_Clicked(this.UIItemBox_Horse);
            };
            this.UIItemBox_FiveElementSeal.Click = () =>
            {
                this.ItemBox_Clicked(this.UIItemBox_FiveElementSeal);
            };
            this.UIItemBox_Ornament.Click = () =>
            {
                this.ItemBox_Clicked(this.UIItemBox_Ornament);
            };
            this.UIItemBox_Mask.Click = () =>
            {
                this.ItemBox_Clicked(this.UIItemBox_Mask);
            };

            /// Duyệt danh sách trang bị trên người
            if (Global.Data.RoleData.GoodsDataList != null)
            {
                foreach (GoodsData equip in Global.Data.RoleData.GoodsDataList)
                {
                    /// Nếu đang hiện Set dự phòng
                    if (Global.Data.ShowReserveEquip)
                    {
                        int usingIndex = equip.Using - 100;

                        switch (usingIndex)
                        {
                            /// Vũ khí
                            case (int) Entities.Enum.KE_EQUIP_POSITION.emEQUIPPOS_WEAPON:
                            {
                                this.UIItemBox_Weapon.Data = equip;
                                roleData.WeaponID = equip.GoodsID;
                                roleData.WeaponEnhanceLevel = equip.Forge_level;
                                roleData.WeaponSeries = equip.Series;
                                break;
                            }
                            /// Nhẫn 2
                            case (int) Entities.Enum.KE_EQUIP_POSITION.emEQUIPPOS_RING_2:
                            {
                                this.UIItemBox_Ring2.Data = equip;
                                break;
                            }
                            /// Nhẫn
                            case (int) Entities.Enum.KE_EQUIP_POSITION.emEQUIPPOS_RING:
                            {
                                this.UIItemBox_Ring.Data = equip;
                                break;
                            }
                            /// Nang/Bội
                            case (int) Entities.Enum.KE_EQUIP_POSITION.emEQUIPPOS_PENDANT:
                            {
                                this.UIItemBox_Pendant.Data = equip;
                                break;
                            }
                            /// Phù
                            case (int) Entities.Enum.KE_EQUIP_POSITION.emEQUIPPOS_AMULET:
                            {
                                this.UIItemBox_Amulet.Data = equip;
                                break;
                            }
                            /// Mũ
                            case (int) Entities.Enum.KE_EQUIP_POSITION.emEQUIPPOS_HEAD:
                            {
                                this.UIItemBox_Hat.Data = equip;
                                roleData.HelmID = equip.GoodsID;
                                break;
                            }
                            /// Áo
                            case (int) Entities.Enum.KE_EQUIP_POSITION.emEQUIPPOS_BODY:
                            {
                                this.UIItemBox_Armor.Data = equip;
                                roleData.ArmorID = equip.GoodsID;
                                break;
                            }
                            /// Lưng
                            case (int) Entities.Enum.KE_EQUIP_POSITION.emEQUIPPOS_BELT:
                            {
                                this.UIItemBox_Belt.Data = equip;
                                break;
                            }
                            /// Tay
                            case (int) Entities.Enum.KE_EQUIP_POSITION.emEQUIPPOS_CUFF:
                            {
                                this.UIItemBox_Wristband.Data = equip;
                                break;
                            }
                            /// Giày
                            case (int) Entities.Enum.KE_EQUIP_POSITION.emEQUIPPOS_FOOT:
                            {
                                this.UIItemBox_Shoe.Data = equip;
                                break;
                            }
                            /// Phi phong
                            case (int) Entities.Enum.KE_EQUIP_POSITION.emEQUIPPOS_MANTLE:
                            {
                                this.UIItemBox_Coat.Data = equip;
                                roleData.MantleID = equip.GoodsID;
                                break;
                            }
                            /// Ngựa
                            case (int) Entities.Enum.KE_EQUIP_POSITION.emEQUIPPOS_HORSE:
                            {
                                this.UIItemBox_Horse.Data = equip;
                                break;
                            }
                            /// Ngũ hành ấn
                            case (int) Entities.Enum.KE_EQUIP_POSITION.emEQUIPPOS_SIGNET:
                            {
                                this.UIItemBox_FiveElementSeal.Data = equip;
                                break;
                            }
                            /// Mật tịch
                            case (int) Entities.Enum.KE_EQUIP_POSITION.emEQUIPPOS_ORNAMENT:
                            {
                                this.UIItemBox_Ornament.Data = equip;
                                break;
                            }
                            /// Mặt nạ
                            case (int) Entities.Enum.KE_EQUIP_POSITION.emEQUIPPOS_MASK:
                            {
                                this.UIItemBox_Mask.Data = equip;
                                break;
                            }
                        }
                    }
                    else
                    {
                        switch (equip.Using)
                        {
                            /// Vũ khí
                            case (int) Entities.Enum.KE_EQUIP_POSITION.emEQUIPPOS_WEAPON:
                            {
                                this.UIItemBox_Weapon.Data = equip;
                                roleData.WeaponID = equip.GoodsID;
                                roleData.WeaponEnhanceLevel = equip.Forge_level;
                                roleData.WeaponSeries = equip.Series;
                                break;
                            }
                            /// Nhẫn Trên
                            case (int) Entities.Enum.KE_EQUIP_POSITION.emEQUIPPOS_RING_2:
                            {
                                this.UIItemBox_Ring2.Data = equip;
                                break;
                            }
                            /// Nhẫn
                            case (int) Entities.Enum.KE_EQUIP_POSITION.emEQUIPPOS_RING:
                            {
                                this.UIItemBox_Ring.Data = equip;
                                break;
                            }
                            /// Nang
                            case (int) Entities.Enum.KE_EQUIP_POSITION.emEQUIPPOS_PENDANT:
                            {
                                this.UIItemBox_Pendant.Data = equip;
                                break;
                            }
                            /// Phù
                            case (int) Entities.Enum.KE_EQUIP_POSITION.emEQUIPPOS_AMULET:
                            {
                                this.UIItemBox_Amulet.Data = equip;
                                break;
                            }
                            /// Mũ
                            case (int) Entities.Enum.KE_EQUIP_POSITION.emEQUIPPOS_HEAD:
                            {
                                this.UIItemBox_Hat.Data = equip;
                                roleData.HelmID = equip.GoodsID;
                                break;
                            }
                            /// Áo
                            case (int) Entities.Enum.KE_EQUIP_POSITION.emEQUIPPOS_BODY:
                            {
                                this.UIItemBox_Armor.Data = equip;
                                roleData.ArmorID = equip.GoodsID;
                                break;
                            }
                            /// Lưng
                            case (int) Entities.Enum.KE_EQUIP_POSITION.emEQUIPPOS_BELT:
                            {
                                this.UIItemBox_Belt.Data = equip;
                                break;
                            }
                            /// Tay
                            case (int) Entities.Enum.KE_EQUIP_POSITION.emEQUIPPOS_CUFF:
                            {
                                this.UIItemBox_Wristband.Data = equip;
                                break;
                            }
                            /// Giày
                            case (int) Entities.Enum.KE_EQUIP_POSITION.emEQUIPPOS_FOOT:
                            {
                                this.UIItemBox_Shoe.Data = equip;
                                break;
                            }
                            /// Phi phong
                            case (int) Entities.Enum.KE_EQUIP_POSITION.emEQUIPPOS_MANTLE:
                            {
                                this.UIItemBox_Coat.Data = equip;
                                roleData.MantleID = equip.GoodsID;
                                break;
                            }
                            /// Ngựa
                            case (int) Entities.Enum.KE_EQUIP_POSITION.emEQUIPPOS_HORSE:
                            {
                                this.UIItemBox_Horse.Data = equip;
                                break;
                            }
                            /// Trang sức
                            case (int) Entities.Enum.KE_EQUIP_POSITION.emEQUIPPOS_ORNAMENT:
                            {
                                this.UIItemBox_Ornament.Data = equip;
                                break;
                            }
                            /// Ngũ hành ấn
                            case (int) Entities.Enum.KE_EQUIP_POSITION.emEQUIPPOS_SIGNET:
                            {
                                this.UIItemBox_FiveElementSeal.Data = equip;
                                break;
                            }
                            /// Mặt nạ
                            case (int) Entities.Enum.KE_EQUIP_POSITION.emEQUIPPOS_MASK:
                            {
                                this.UIItemBox_Mask.Data = equip;
                                break;
                            }
                        }
                    }
                }
            }

            /// Cập nhật hiển thị chiếu nhân vật
            this.RefreshRolePreview(roleData);
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Cập nhật thông tin nhân vật
        /// </summary>
        /// <returns></returns>
        public void UpdateRoleData()
        {
            if (this.UITab_Properties.gameObject.activeSelf)
            {
                this.UITab_Properties.HP = Global.Data.RoleData.CurrentHP;
                this.UITab_Properties.HPMax = Global.Data.RoleData.MaxHP;
                this.UITab_Properties.MP = Global.Data.RoleData.CurrentMP;
                this.UITab_Properties.MPMax = Global.Data.RoleData.MaxMP;
                this.UITab_Properties.Vitality = Global.Data.RoleData.CurrentStamina;
                this.UITab_Properties.VitalityMax = Global.Data.RoleData.MaxStamina;
                this.UITab_Properties.Potential = Global.Data.RoleData.MakePoint;
                this.UITab_Properties.Energy = Global.Data.RoleData.GatherPoint;
            }

            this.UIText_RoleName.text = Global.Data.RoleData.RoleName;
            string factionName = KTGlobal.GetFactionName(Global.Data.RoleData.FactionID, out Color color);
            this.UIText_RoleFaction.text = string.Format("<color=#{0}>{1}</color>", ColorUtility.ToHtmlStringRGB(color), factionName);
            this.UIText_WorldHonorPoint.text = Global.Data.RoleData.WorldHonor.ToString();
            this.UIText_PerfHonorPoint.text = (Global.Data.RoleData.TotalValue / 10000).ToString();
            this.UIText_PKValue.text = Global.Data.RoleData.PKValue.ToString();
            this.UIText_RoleLevel.text = Global.Data.RoleData.Level.ToString();
            string elementName = KTGlobal.GetFactionElementString(Global.Data.RoleData.FactionID, out Color _color);
            this.UIText_ElementalText.text = string.Format("<color=#{0}>{1}</color>", ColorUtility.ToHtmlStringRGB(_color), elementName);

            this.UIImage_Avarta.AvartaID = Global.Data.RoleData.RolePic;
        }

        /// <summary>
        /// Cập nhật trang bị nhân vật
        /// </summary>
        public void RefreshRoleEquips()
        {
            this.RefreshEquips();
        }

        /// <summary>
        /// Cập nhật vinh dự võ lâm và uy danh
        /// </summary>
        public void UpdatePrestigeAndWorldHonor()
        {
            this.UIText_WorldHonorPoint.text = Global.Data.RoleData.WorldHonor.ToString();
        }

        /// <summary>
        /// Cập nhật danh vọng
        /// </summary>
        public void RefreshRepute()
        {
            if (this.UITab_Reputes.gameObject.activeSelf)
            {
                this.UITab_Reputes.Refresh();
            }
        }

        /// <summary>
        /// Thêm danh hiệu nhân vật cho người chơi
        /// </summary>
        /// <param name="titleID"></param>
        public void AddRoleTitle(int titleID)
        {
            this.UITab_Titles.AddRoleTitle(titleID);
        }

        /// <summary>
        /// Xóa danh hiệu nhân vật của người chơi
        /// </summary>
        /// <param name="titleID"></param>
        public void RemoveRoleTitle(int titleID)
        {
            this.UITab_Titles.RemoveRoleTitle(titleID);
        }

        /// <summary>
        /// Thiết lập làm danh hiệu hiện tại
        /// </summary>
        public void RefreshCurrentTitle()
        {
            this.UITab_Titles.RefreshCurrentTitle();
        }
        #endregion
    }
}
