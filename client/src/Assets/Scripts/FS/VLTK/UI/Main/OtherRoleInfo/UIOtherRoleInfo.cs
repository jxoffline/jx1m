using FS.VLTK.Control.Component;
using FS.VLTK.Factory;
using FS.VLTK.UI.Main.ItemBox;
using Server.Data;
using System;
using TMPro;
using UnityEngine;

namespace FS.VLTK.UI.Main
{
    /// <summary>
    /// Khung soi thông tin trang bị người chơi khác
    /// </summary>
    public class UIOtherRoleInfo : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Button đóng khung
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_Close;

        /// <summary>
        /// Image Avarta người chơi
        /// </summary>
        [SerializeField]
        private UIRoleAvarta UIImage_Avarta;

        /// <summary>
        /// Text tên người chơi
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_Name;

        /// <summary>
        /// Text cấp độ người chơi
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_Level;

        /// <summary>
        /// Text môn phái người chơi
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_FactionName;

        /// <summary>
        /// Text ngũ hành môn phái
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_FactionSeries;

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
        /// Ô trang bị Trang sức
        /// </summary>
        [SerializeField]
        private UIItemBox UIItemBox_Ornament;

        /// <summary>
        /// Ô trang bị Ngũ hành Ấn
        /// </summary>
        [SerializeField]
        private UIItemBox UIItemBox_FiveElementSeal;

        /// <summary>
        /// Ô trang bị Phù
        /// </summary>
        [SerializeField]
        private UIItemBox UIItemBox_Amulet;

        /// <summary>
        /// Ô trang bị Nang
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
        /// Thông tin người chơi
        /// </summary>
        public RoleData Data { get; set; }
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi ở Frame đầu tiên
        /// </summary>
        private void Start()
        {
            this.InitPrefabs();
            this.Refresh();
        }

        /// <summary>
        /// Hàm này gọi khi đối tượng bị hủy
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
        }

        /// <summary>
        /// Sự kiện khi Button đóng khung được ấn
        /// </summary>
        private void ButtonClose_Clicked()
        {
            this.Close?.Invoke();
        }
        #endregion

        #region Private methods
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
                RoleID = this.Data.RoleID,
                RoleSex = this.Data.RoleSex,
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
            this.UIItemBox_Ornament.Data = null;
            this.UIItemBox_FiveElementSeal.Data = null;
            this.UIItemBox_Mask.Data = null;

            /// Duyệt danh sách trang bị trên người
            if (this.Data.GoodsDataList != null)
            {
                foreach (GoodsData equip in this.Data.GoodsDataList)
                {
                    switch (equip.Using)
                    {
                        /// Vũ khí
                        case (int)Entities.Enum.KE_EQUIP_POSITION.emEQUIPPOS_WEAPON:
                            {
                                this.UIItemBox_Weapon.Data = equip;
                                roleData.WeaponID = equip.GoodsID;
                                roleData.WeaponEnhanceLevel = equip.Forge_level;
                                roleData.WeaponSeries = equip.Series;
                                break;
                            }
                        /// Nhẫn 2
                        case (int)Entities.Enum.KE_EQUIP_POSITION.emEQUIPPOS_RING_2:
                            {
                                this.UIItemBox_Ring2.Data = equip;
                                break;
                            }
                        /// Nhẫn
                        case (int)Entities.Enum.KE_EQUIP_POSITION.emEQUIPPOS_RING:
                            {
                                this.UIItemBox_Ring.Data = equip;
                                break;
                            }
                        /// Nang
                        case (int)Entities.Enum.KE_EQUIP_POSITION.emEQUIPPOS_PENDANT:
                            {
                                this.UIItemBox_Pendant.Data = equip;
                                break;
                            }
                        /// Phù
                        case (int)Entities.Enum.KE_EQUIP_POSITION.emEQUIPPOS_AMULET:
                            {
                                this.UIItemBox_Amulet.Data = equip;
                                break;
                            }
                        /// Mũ
                        case (int)Entities.Enum.KE_EQUIP_POSITION.emEQUIPPOS_HEAD:
                            {
                                this.UIItemBox_Hat.Data = equip;
                                roleData.HelmID = equip.GoodsID;
                                break;
                            }
                        /// Áo
                        case (int)Entities.Enum.KE_EQUIP_POSITION.emEQUIPPOS_BODY:
                            {
                                this.UIItemBox_Armor.Data = equip;
                                roleData.ArmorID = equip.GoodsID;
                                break;
                            }
                        /// Lưng
                        case (int)Entities.Enum.KE_EQUIP_POSITION.emEQUIPPOS_BELT:
                            {
                                this.UIItemBox_Belt.Data = equip;
                                break;
                            }
                        /// Tay
                        case (int)Entities.Enum.KE_EQUIP_POSITION.emEQUIPPOS_CUFF:
                            {
                                this.UIItemBox_Wristband.Data = equip;
                                break;
                            }
                        /// Giày
                        case (int)Entities.Enum.KE_EQUIP_POSITION.emEQUIPPOS_FOOT:
                            {
                                this.UIItemBox_Shoe.Data = equip;
                                break;
                            }
                        /// Phi phong
                        case (int)Entities.Enum.KE_EQUIP_POSITION.emEQUIPPOS_MANTLE:
                            {
                                this.UIItemBox_Coat.Data = equip;
                                roleData.MantleID = equip.GoodsID;
                                break;
                            }
                        /// Ngựa
                        case (int)Entities.Enum.KE_EQUIP_POSITION.emEQUIPPOS_HORSE:
                            {
                                this.UIItemBox_Horse.Data = equip;
                                break;
                            }
                        /// Trận
                        case (int)Entities.Enum.KE_EQUIP_POSITION.emEQUIPPOS_ORNAMENT:
                            {
                                this.UIItemBox_Ornament.Data = equip;
                                break;
                            }
                        /// Ngũ hành ấn
                        case (int)Entities.Enum.KE_EQUIP_POSITION.emEQUIPPOS_SIGNET:
                            {
                                this.UIItemBox_FiveElementSeal.Data = equip;
                                break;
                            }
                        /// Mặt nạ
                        case (int)Entities.Enum.KE_EQUIP_POSITION.emEQUIPPOS_MASK:
                            {
                                this.UIItemBox_Mask.Data = equip;
                                break;
                            }
                    }
                }
            }

            /// Cập nhật hiển thị chiếu nhân vật
            this.RefreshRolePreview(roleData);
        }

        /// <summary>
        /// Làm mới thông tin người chơi
        /// </summary>
        private void Refresh()
        {
            /// Cập nhật thông tin
            this.UIText_Name.text = this.Data.RoleName;
            this.UIText_Level.text = this.Data.Level.ToString();
            this.UIText_FactionName.text = KTGlobal.GetFactionName(this.Data.FactionID, out _);
            this.UIText_FactionSeries.text = KTGlobal.GetFactionElementString(this.Data.FactionID, out Color seriesColor);
            this.UIText_FactionSeries.color = seriesColor;
            this.UIImage_Avarta.RoleID = this.Data.RoleID;
            this.UIImage_Avarta.AvartaID = this.Data.RolePic;

            /// Làm mới thông tin trang bị
            this.RefreshEquips();
        }
        #endregion

        #region Public methods

        #endregion
    }
}
