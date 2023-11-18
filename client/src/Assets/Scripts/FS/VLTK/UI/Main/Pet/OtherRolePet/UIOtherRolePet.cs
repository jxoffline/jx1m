using FS.VLTK.Control.Component;
using FS.VLTK.Entities.Config;
using FS.VLTK.Factory;
using FS.VLTK.UI.Main.ItemBox;
using FS.VLTK.UI.Main.Pet.OtherRolePet;
using Server.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static FS.VLTK.Entities.Enum;

namespace FS.VLTK.UI.Main.Pet
{
    /// <summary>
    /// Khung thông tin pet của người chơi
    /// </summary>
    public class UIOtherRolePet : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Button đóng khung
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_Close;

        /// <summary>
        /// Text tiêu đề khung
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_Title;

        /// <summary>
        /// Text tên Res của pet
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_PetResName;

        /// <summary>
        /// Text loại pet
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_PetType;

        /// <summary>
        /// Image preview pet
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.RawImage UIImage_PetPreview;

        /// <summary>
        /// Input tên pet
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_PetName;

        /// <summary>
        /// Text cấp độ pet
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_PetLevel;

        /// <summary>
        /// Slider kinh nghiệm pet
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Slider UISlider_PetExp;

        /// <summary>
        /// Text kinh nghiệm pet
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_PetExp;

        /// <summary>
        /// Slider sinh lực pet
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Slider UISlider_PetHP;

        /// <summary>
        /// Text sinh lực pet
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_PetHP;

        /// <summary>
        /// Text lĩnh ngộ pet
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_PetEnlightenment;

        /// <summary>
        /// Text độ vui vẻ
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_Joyful;

        /// <summary>
        /// Text tuổi thọ
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_Life;

        /// <summary>
        /// Prefab thông tin pet đang mang theo
        /// </summary>
        [SerializeField]
        private UIOtherRolePet_PetInfo UI_PetInfoPrefab;

        /// <summary>
        /// Text sức mạnh
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_Str;

        /// <summary>
        /// Text thân pháp
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_Dex;

        /// <summary>
        /// Text ngoại công
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_Sta;

        /// <summary>
        /// Text nội công
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_Int;

        /// <summary>
        /// Text tiềm năng
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_RemainPoints;

        /// <summary>
        /// Text vật công ngoại
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_PAtk;

        /// <summary>
        /// Text vật công nội
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_MAtk;

        /// <summary>
        /// Text chính xác
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_Hit;

        /// <summary>
        /// Text né tránh
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_Dodge;

        /// <summary>
        /// Text tốc đánh ngoại
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_AtkSpeed;

        /// <summary>
        /// Text tốc đánh nội
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_CastSpeed;

        /// <summary>
        /// Text phòng ngự vật công
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_Def;

        /// <summary>
        /// Text kháng độc
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_PoisonRes;

        /// <summary>
        /// Text kháng băng
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_IceRes;

        /// <summary>
        /// Text kháng hỏa
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_FireRes;

        /// <summary>
        /// Text kháng lôi
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_LightningRes;

        /// <summary>
        /// Item nón
        /// </summary>
        [SerializeField]
        private UIItemBox UIItem_Head;

        /// <summary>
        /// Item giáp
        /// </summary>
        [SerializeField]
        private UIItemBox UIItem_Armor;

        /// <summary>
        /// Item lưng
        /// </summary>
        [SerializeField]
        private UIItemBox UIItem_Belt;

        /// <summary>
        /// Item giày
        /// </summary>
        [SerializeField]
        private UIItemBox UIItem_Boot;

        /// <summary>
        /// Item vũ khí
        /// </summary>
        [SerializeField]
        private UIItemBox UIItem_Weapon;

        /// <summary>
        /// Item dây chuyền
        /// </summary>
        [SerializeField]
        private UIItemBox UIItem_Necklace;

        /// <summary>
        /// Item vòng
        /// </summary>
        [SerializeField]
        private UIItemBox UIItem_Cuff;

        /// <summary>
        /// Item nhẫn
        /// </summary>
        [SerializeField]
        private UIItemBox UIItem_Ring;

        /// <summary>
        /// Prefab thông tin kỹ năng
        /// </summary>
        [SerializeField]
        private UIOtherRolePet_SkillInfo UI_SkillInfoPrefab;
        #endregion

        #region Properties
        private OtherRolePetData _Data;
        /// <summary>
        /// Dữ liệu
        /// </summary>
        public OtherRolePetData Data
        {
            get
            {
                return this._Data;
            }
            set
            {
                this._Data = value;
                /// Làm mới dữ liệu
                this.RefreshData();
            }
        }

        /// <summary>
        /// Sự kiện đóng khung
        /// </summary>
        public Action Close { get; set; }
        #endregion

        #region Private fields
        /// <summary>
        /// Đối tượng đang được chiếu
        /// </summary>
        private MonsterPreview petPreview = null;

        /// <summary>
        /// RectTransform danh sách pet
        /// </summary>
        private RectTransform transformPetList;

        /// <summary>
        /// RectTransform danh sách kỹ năng pet
        /// </summary>
        private RectTransform transformSkillList;
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi khi đối tượng được tạo ra
        /// </summary>
        private void Awake()
        {
            this.transformPetList = this.UI_PetInfoPrefab.transform.parent.GetComponent<RectTransform>();
            this.transformSkillList = this.UI_SkillInfoPrefab.transform.parent.GetComponent<RectTransform>();
        }

        /// <summary>
        /// Hàm này gọi ở Frame đầu tiên
        /// </summary>
        private void Start()
        {
            this.InitPrefabs();
        }

        /// <summary>
        /// Hàm này gọi khi đối tượng bị hủy
        /// </summary>
        private void OnDestroy()
        {
            if (this.petPreview != null)
            {
                GameObject.Destroy(this.petPreview.gameObject);
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

            this.UIItem_Head.Click = () =>
            {
                this.ButtonEquip_Clicked(this.UIItem_Head);
            };
            this.UIItem_Armor.Click = () =>
            {
                this.ButtonEquip_Clicked(this.UIItem_Armor);
            };
            this.UIItem_Belt.Click = () =>
            {
                this.ButtonEquip_Clicked(this.UIItem_Belt);
            };
            this.UIItem_Boot.Click = () =>
            {
                this.ButtonEquip_Clicked(this.UIItem_Boot);
            };
            this.UIItem_Weapon.Click = () =>
            {
                this.ButtonEquip_Clicked(this.UIItem_Weapon);
            };
            this.UIItem_Necklace.Click = () =>
            {
                this.ButtonEquip_Clicked(this.UIItem_Necklace);
            };
            this.UIItem_Cuff.Click = () =>
            {
                this.ButtonEquip_Clicked(this.UIItem_Cuff);
            };
            this.UIItem_Ring.Click = () =>
            {
                this.ButtonEquip_Clicked(this.UIItem_Ring);
            };
        }

        /// <summary>
        /// Sự kiện khi Button trang bị pet được ấn
        /// </summary>
        private void ButtonEquip_Clicked(UIItemBox uiItemBox)
        {
            /// Nếu không có gì
            if (uiItemBox.Data == null)
            {
                /// Bỏ qua
                return;
            }

            /// Hiện Tooltip
            KTGlobal.ShowItemInfo(uiItemBox.Data);
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
        /// Luồng thực thi sự kiện bỏ qua một số frame
        /// </summary>
        /// <param name="skip"></param>
        /// <param name="work"></param>
        /// <returns></returns>
        private IEnumerator DoExecuteSkipFrames(int skip, Action work)
        {
            for (int i = 1; i <= skip; i++)
            {
                yield return null;
            }
            work?.Invoke();
        }

        /// <summary>
        /// Thực thi sự kiện bỏ qua một số Frame
        /// </summary>
        /// <param name="skip"></param>
        /// <param name="work"></param>
        private void ExecuteSkipFrame(int skip, Action work)
        {
            this.StartCoroutine(this.DoExecuteSkipFrames(skip, work));
        }

        /// <summary>
        /// Làm mới dữ liệu
        /// </summary>
        private void RefreshData()
        {
            /// Toác
            if (this._Data == null)
            {
                /// Đóng khung
                this.Close?.Invoke();
                /// Bỏ qua
                return;
            }

            /// Tiêu đề
            this.UIText_Title.text = string.Format("Tinh linh của {0}", this._Data.RoleName);

            /// Làm rỗng danh sách trang bị
            this.UIItem_Head.Data = null;
            this.UIItem_Armor.Data = null;
            this.UIItem_Belt.Data = null;
            this.UIItem_Boot.Data = null;
            this.UIItem_Weapon.Data = null;
            this.UIItem_Necklace.Data = null;
            this.UIItem_Cuff.Data = null;
            this.UIItem_Ring.Data = null;

            /// Nếu tồn tại vật phẩm
            if (this._Data.PetEquips != null)
            {
                /// Duyệt danh sách vật phẩm trên người
                foreach (GoodsData itemGD in this._Data.PetEquips)
                {
                    /// Nếu không phải đang mặc trên người
                    if (itemGD.Using <= 0)
                    {
                        continue;
                    }

                    /// Thông tin vật phẩm
                    if (!Loader.Loader.Items.TryGetValue(itemGD.GoodsID, out ItemData itemData))
                    {
                        /// Bỏ qua
                        continue;
                    }
                    /// Nếu là trang bị pet
                    if (KTGlobal.IsPetEquip(itemData.Genre))
                    {
                        /// Loại
                        switch (itemGD.Using)
                        {
                            /// Mũ
                            case (int) KE_EQUIP_PET_POSTION.emEQUIPPOS_HEAD:
                            {
                                this.UIItem_Head.Data = itemGD;
                                break;
                            }
                            /// Giáp
                            case (int) KE_EQUIP_PET_POSTION.emEQUIPPOS_BODY:
                            {
                                this.UIItem_Armor.Data = itemGD;
                                break;
                            }
                            /// Lưng
                            case (int) KE_EQUIP_PET_POSTION.emEQUIPPOS_BELT:
                            {
                                this.UIItem_Belt.Data = itemGD;
                                break;
                            }
                            /// Vũ khí
                            case (int) KE_EQUIP_PET_POSTION.emEQUIPPOS_WEAPON:
                            {
                                this.UIItem_Weapon.Data = itemGD;
                                break;
                            }
                            /// Giày
                            case (int) KE_EQUIP_PET_POSTION.emEQUIPPOS_FOOT:
                            {
                                this.UIItem_Boot.Data = itemGD;
                                break;
                            }
                            /// Tay
                            case (int) KE_EQUIP_PET_POSTION.emEQUIPPOS_CUFF:
                            {
                                this.UIItem_Cuff.Data = itemGD;
                                break;
                            }
                            /// Liên
                            case (int) KE_EQUIP_PET_POSTION.emEQUIPPOS_ORNAMENT:
                            {
                                this.UIItem_Necklace.Data = itemGD;
                                break;
                            }
                            /// Nhẫn
                            case (int) KE_EQUIP_PET_POSTION.emEQUIPPOS_RING:
                            {
                                this.UIItem_Ring.Data = itemGD;
                                break;
                            }
                        }
                    }
                }
            }

            /// Làm rỗng danh sách pet
            foreach (Transform child in this.transformPetList.transform)
            {
                if (child.gameObject != this.UI_PetInfoPrefab.gameObject)
                {
                    GameObject.Destroy(child.gameObject);
                }
            }
            /// Làm rỗng danh sách kỹ năng
            foreach (Transform child in this.transformSkillList.transform)
            {
                if (child.gameObject != this.UI_SkillInfoPrefab.gameObject)
                {
                    GameObject.Destroy(child.gameObject);
                }
            }
            /// Ẩn preview
            this.UIImage_PetPreview.gameObject.SetActive(false);
            /// Làm rỗng Text
            this.UIText_PetType.text = "";
            this.UIText_PetResName.text = "Chưa chọn tinh linh";
            this.UIText_PetName.text = "";
            this.UIText_PetLevel.text = "";
            this.UIText_PetEnlightenment.text = "";
            this.UIText_PetExp.text = "";
            this.UISlider_PetExp.value = 0f;
            this.UIText_PetHP.text = "";
            this.UISlider_PetHP.value = 0f;
            this.UIText_Joyful.text = "";
            this.UIText_Life.text = "";
            this.UIText_Str.text = "";
            this.UIText_Dex.text = "";
            this.UIText_Sta.text = "";
            this.UIText_Int.text = "";
            this.UIText_RemainPoints.text = "";
            this.UIText_PAtk.text = "";
            this.UIText_MAtk.text = "";
            this.UIText_Hit.text = "";
            this.UIText_Dodge.text = "";
            this.UIText_AtkSpeed.text = "";
            this.UIText_CastSpeed.text = "";
            this.UIText_Def.text = "";
            this.UIText_IceRes.text = "";
            this.UIText_FireRes.text = "";
            this.UIText_LightningRes.text = "";
            this.UIText_PoisonRes.text = "";

            /// Toác
            if (this._Data.Pets == null)
            {
                /// Bỏ qua
                return;
            }

            /// UI Pet ở vị trí đầu tiên
            UIOtherRolePet_PetInfo uiFirstPet = null;

            /// Duyệt danh sách tinh linh
            foreach (PetData petData in this._Data.Pets)
            {
                /// Tạo mới
                UIOtherRolePet_PetInfo uiPetInfo = GameObject.Instantiate<UIOtherRolePet_PetInfo>(this.UI_PetInfoPrefab);
                uiPetInfo.transform.SetParent(this.transformPetList, false);
                uiPetInfo.gameObject.SetActive(true);
                uiPetInfo.Data = petData;
                uiPetInfo.Select = () =>
                {
                    /// Thông tin Res
                    if (Loader.Loader.ListPets.TryGetValue(petData.ResID, out Entities.Config.PetDataXML _data))
                    {
                        /// Tên Res
                        this.UIText_PetResName.text = _data.Name;
                        /// Loại
                        this.UIText_PetType.text = _data.TypeDesc;
                    }
                    else
                    {
                        /// Tên Res
                        this.UIText_PetResName.text = "Chưa cập nhật";
                        /// Loại
                        this.UIText_PetType.text = "Không rõ";
                    }
                    /// Tên
                    this.UIText_PetName.text = petData.Name;
                    /// Cấp
                    this.UIText_PetLevel.text = petData.Level.ToString();
                    /// Kinh nghiệm yêu cầu thăng cấp
                    int maxExp = 0;
                    if (petData.Level < Loader.Loader.PetConfig.LevelUpExps.Count)
                    {
                        maxExp = Loader.Loader.PetConfig.LevelUpExps[petData.Level - 1];
                    }
                    /// Kinh nghiệm
                    this.UIText_PetExp.text = string.Format("{0}/{1}", petData.Exp, maxExp);
                    /// Toác
                    if (maxExp == 0)
                    {
                        this.UISlider_PetExp.value = 0f;
                    }
                    else
                    {
                        this.UISlider_PetExp.value = petData.Exp / (float) maxExp;
                    }
                    /// Sinh lực
                    int hp = Math.Min(petData.HP, petData.MaxHP);
                    this.UIText_PetHP.text = string.Format("{0}/{1}", hp, petData.MaxHP);
                    this.UISlider_PetHP.value = hp / (float) petData.MaxHP;
                    /// Lĩnh ngộ
                    this.UIText_PetEnlightenment.text = petData.Enlightenment.ToString();
                    /// Vui vẻ
                    this.UIText_Joyful.text = string.Format("{0}/{1}", petData.Joyful, Loader.Loader.PetConfig.MaxJoy);
                    /// Tuổi thọ
                    this.UIText_Life.text = string.Format("{0}/{1}", petData.Life, Loader.Loader.PetConfig.MaxLife);
                    /// Sức mạnh
                    this.UIText_Str.text = petData.Str.ToString();
                    /// Thân pháp
                    this.UIText_Dex.text = petData.Dex.ToString();
                    /// Tiềm năng
                    this.UIText_RemainPoints.text = petData.RemainPoints.ToString();
                    /// Ngoại công
                    this.UIText_Sta.text = petData.Sta.ToString();
                    /// Nội công
                    this.UIText_Int.text = petData.Int.ToString();
                    /// Vật công ngoại
                    this.UIText_PAtk.text = petData.PAtk.ToString();
                    /// Vật công nội
                    this.UIText_MAtk.text = petData.MAtk.ToString();
                    /// Chính xác
                    this.UIText_Hit.text = petData.Hit.ToString();
                    /// Né tránh
                    this.UIText_Dodge.text = petData.Dodge.ToString();
                    /// Tốc đánh - ngoại công
                    this.UIText_AtkSpeed.text = petData.AtkSpeed.ToString();
                    /// Tốc đánh - nội công
                    this.UIText_CastSpeed.text = petData.CastSpeed.ToString();
                    /// Phòng ngự - vật công
                    this.UIText_Def.text = petData.PDef.ToString();
                    /// Kháng băng
                    this.UIText_IceRes.text = petData.IceRes.ToString();
                    /// Kháng hỏa
                    this.UIText_FireRes.text = petData.FireRes.ToString();
                    /// Kháng lôi
                    this.UIText_LightningRes.text = petData.LightningRes.ToString();
                    /// Kháng độc
                    this.UIText_PoisonRes.text = petData.PoisonRes.ToString();

                    /// Làm rỗng danh sách kỹ năng
                    foreach (Transform child in this.transformSkillList.transform)
                    {
                        if (child.gameObject != this.UI_SkillInfoPrefab.gameObject)
                        {
                            GameObject.Destroy(child.gameObject);
                        }
                    }
                    /// Nếu tồn tại danh sách kỹ năng
                    if (petData.Skills != null)
                    {
                        /// Duyệt danh sách kỹ năng
                        foreach (KeyValuePair<int, int> pair in petData.Skills)
                        {
                            /// Thông tin kỹ năng
                            if (Loader.Loader.Skills.TryGetValue(pair.Key, out Entities.Config.SkillDataEx skillData))
                            {
                                UIOtherRolePet_SkillInfo uiSkillInfo = GameObject.Instantiate<UIOtherRolePet_SkillInfo>(this.UI_SkillInfoPrefab);
                                uiSkillInfo.transform.SetParent(this.transformSkillList, false);
                                uiSkillInfo.gameObject.SetActive(true);
                                uiSkillInfo.SkillID = pair.Key;
                                uiSkillInfo.SkillLevel = pair.Value;
                            }
                        }
                        /// Xây lại giao diện
                        this.ExecuteSkipFrame(1, () =>
                        {
                            UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(this.transformSkillList);
                        });
                    }

                    /// Cập nhật hiển thị chiếu pet
                    this.RefreshPetPreview(petData);
                };

                /// Nếu chưa chọn pet ở vị trí đầu tiên
                if (uiFirstPet == null)
                {
                    /// Đánh dấu vị trí đầu tiên
                    uiFirstPet = uiPetInfo;
                    /// Thực hiện chọn vị trí đầu tiên
                    uiFirstPet.Active = true;
                }
            }

            /// Xây lại giao diện
            this.ExecuteSkipFrame(1, () =>
            {
                UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(this.transformPetList);
                UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(this.transformSkillList);
            });
        }

        /// <summary>
        /// Làm mới chiếu pet
        /// </summary>
        /// <param name="petData"></param>
        private void RefreshPetPreview(PetData petData)
        {
            /// Toác
            if (petData == null)
            {
                return;
            }

            /// Thông tin Res
            if (Loader.Loader.ListPets.TryGetValue(petData.ResID, out Entities.Config.PetDataXML _data))
            {
                /// Nếu chưa tạo thì tạo mới
                if (this.petPreview == null)
                {
                    this.petPreview = Object2DFactory.MakeMonsterPreview();
                }
                /// Hiện preview
                this.UIImage_PetPreview.gameObject.SetActive(true);
                /// Thiết lập preview
                this.petPreview.ResID = _data.ResID;
                this.petPreview.UpdateResID();

                this.petPreview.Direction = Entities.Enum.Direction.DOWN;
                this.petPreview.OnStart = () =>
                {
                    this.UIImage_PetPreview.texture = this.petPreview.ReferenceCamera.targetTexture;
                };
                this.petPreview.ResumeCurrentAction();
            }
        }
        #endregion
    }
}
