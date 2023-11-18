using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using FS.VLTK.UI.Main.Pet.Main;
using FS.VLTK.UI.Main.ItemBox;
using Server.Data;
using System.Collections;
using FS.GameEngine.Logic;
using FS.VLTK.Control.Component;
using FS.VLTK.Factory;
using System.Linq;
using static FS.VLTK.KTMath;
using FS.VLTK.Entities.Config;
using static FS.VLTK.Entities.Enum;
using FS.GameEngine.Network;

namespace FS.VLTK.UI.Main
{
    /// <summary>
    /// Khung pet
    /// </summary>
    public class UIPet : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Button đóng khung
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_Close;

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
        private TMP_InputField UIInput_PetName;

        /// <summary>
        /// Button đổi tên
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_ChangeName;

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
        /// Button tặng quà
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_GiveItems;

        /// <summary>
        /// Text độ vui vẻ
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_Joyful;

        /// <summary>
        /// Button tăng độ vui vẻ
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_FeedJoy;

        /// <summary>
        /// Text tuổi thọ
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_Life;

        /// <summary>
        /// Button tăng tuổi thọ
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_FeedLife;

        /// <summary>
        /// Button quảng bá
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_Advertise;

        /// <summary>
        /// Prefab thông tin pet đang mang theo
        /// </summary>
        [SerializeField]
        private UIPet_PetInfo UI_PetInfoPrefab;

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
        /// Button phân phối tiềm năng
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_Dispute;

        /// <summary>
        /// Button tẩy điểm tiềm năng
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_ResetAttributes;

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
        private UIPet_SkillInfo UI_SkillInfoPrefab;

        /// <summary>
        /// Button triệu hồi pet
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_Call;

        /// <summary>
        /// Button thu hồi pet
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_CallBack;

        /// <summary>
        /// Button phóng thích
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_Release;

        /// <summary>
        /// Button mở khung học kỹ năng pet
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_OpenStudySkill;

        /// <summary>
        /// Button hiển thị thêm chú thích
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_ShowMoreHint;

        /// <summary>
        /// Khung chú thích thêm
        /// </summary>
        [SerializeField]
        private UIPet_MoreHintPopup UI_ExpandedHint;

        /// <summary>
        /// Khung phân phối tiềm năng pet
        /// </summary>
        [SerializeField]
        private UIPet_AssignAttributes UI_AssignAttributes;
        #endregion

        #region Properties
        private List<PetData> _Data;
        /// <summary>
        /// Dữ liệu
        /// </summary>
        public List<PetData> Data
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

        /// <summary>
        /// Sự kiện đổi tên
        /// </summary>
        public Action<int, string> ChangeName { get; set; }

        /// <summary>
        /// Sự kiện tặng quà
        /// </summary>
        public Action<int, List<int>> GiftItems { get; set; }

        /// <summary>
        /// Sự kiện tăng độ vui vẻ
        /// </summary>
        public Action<int> FeedJoy { get; set; }

        /// <summary>
        /// Sự kiện tăng tuổi thọ
        /// </summary>
        public Action<int> FeedLife { get; set; }

        /// <summary>
        /// Sự kiện quảng bá
        /// </summary>
        public Action<PetData> Advertise { get; set; }

        /// <summary>
        /// Sự kiện xuất chiến pet
        /// </summary>
        public Action<int> Call { get; set; }

        /// <summary>
        /// Sự kiện thu hồi pet
        /// </summary>
        public Action<int> CallBack { get; set; }

        /// <summary>
        /// Sự kiện phóng thích pet
        /// </summary>
        public Action<int> Release { get; set; }

        /// <summary>
        /// Sự kiện mở khung học kỹ năng pet
        /// </summary>
        public Action<PetData> OpenStudySkill { get; set; }

        /// <summary>
        /// Sự kiện phân phối điểm tiềm năng
        /// </summary>
        public Action<int, int, int, int, int> AssignAttributes { get; set; }

        /// <summary>
        /// Sự kiện tẩy điểm tiềm năng
        /// </summary>
        public Action<int> ResetAttributes { get; set; }

        /// <summary>
        /// Gỡ trang bị
        /// </summary>
        public Action<GoodsData> Unequip { get; set; }
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

        /// <summary>
        /// Pet đang được chọn
        /// </summary>
        private PetData selectedPet = null;
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
            this.UIButton_ChangeName.onClick.AddListener(this.ButtonChangeName_Clicked);
            this.UIButton_GiveItems.onClick.AddListener(this.ButtonGiftItems_Clicked);
            this.UIButton_FeedJoy.onClick.AddListener(this.ButtonFeedJoy_Clicked);
            this.UIButton_FeedLife.onClick.AddListener(this.ButtonFeedLife_Clicked);
            this.UIButton_Advertise.onClick.AddListener(this.ButtonAdvertise_Clicked);
            this.UIButton_Call.onClick.AddListener(this.ButtonCall_Clicked);
            this.UIButton_CallBack.onClick.AddListener(this.ButtonCallBack_Clicked);
            this.UIButton_Dispute.onClick.AddListener(this.ButtonDispute_Clicked);
            this.UIButton_ResetAttributes.onClick.AddListener(this.ButtonResetAttributes_Clicked);
            this.UIButton_OpenStudySkill.onClick.AddListener(this.ButtonOpenStudySkill_Clicked);
            this.UIButton_Release.onClick.AddListener(this.ButtonRelease_Clicked);
            this.UIButton_ShowMoreHint.onClick.AddListener(this.ButtonShowMoreHint_Clicked);

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
        /// Sự kiện khi Button quảng bá được ấn
        /// </summary>
        private void ButtonAdvertise_Clicked()
        {
            /// Nếu chưa chọn pet
            if (this.selectedPet == null)
            {
                KTGlobal.AddNotification("Hãy chọn một tinh linh!");
                return;
            }
            /// Thực thi sự kiện
            this.Advertise?.Invoke(this.selectedPet);
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

            /// Danh sách Button chức năng
            List<KeyValuePair<string, Action>> buttons = new List<KeyValuePair<string, Action>>()
            {
                new KeyValuePair<string, Action>("Gỡ xuống", () =>
                {
                    /// Ẩn tooltip
                    KTGlobal.CloseItemInfo();
                    /// Thực thi sự kiện
                    this.Unequip?.Invoke(uiItemBox.Data);
                }),
            };
            /// Hiện Tooltip
            KTGlobal.ShowItemInfo(uiItemBox.Data, buttons);
        }

        /// <summary>
        /// Sự kiện khi Button đóng khung được ấn
        /// </summary>
        private void ButtonClose_Clicked()
        {
            this.Close?.Invoke();
        }

        /// <summary>
        /// Sự kiện khi Button đổi tên pet được ấn
        /// </summary>
        private void ButtonChangeName_Clicked()
        {
            /// Nếu chưa chọn pet
            if (this.selectedPet == null)
            {
                KTGlobal.AddNotification("Hãy chọn một tinh linh!");
                return;
            }
            /// Tên pet
            string petName = this.UIInput_PetName.text.Trim();
            /// Chưa nhập
            if (string.IsNullOrEmpty(petName))
            {
                KTGlobal.AddNotification("Hãy nhập vào tên tinh linh!");
                return;
            }
            /// Độ dài không phù hợp
            else if (petName.Length < 6 || petName.Length > 16)
            {
                KTGlobal.AddNotification("Tên tinh linh phải có từ 6 đến 16 ký tự!");
                return;
            }
            /// Chứa ký tự cấm
            else if (!KTFormValidation.IsValidString(petName, false, true, false, false))
            {
                KTGlobal.AddNotification("Tên tinh linh không được chứa ký tự đặc biệt!");
                return;
            }

            /// Thực thi sự kiện
            this.ChangeName?.Invoke(this.selectedPet.ID, petName);
        }

        /// <summary>
        /// Sự kiện khi Button tặng quà được ấn
        /// </summary>
        private void ButtonGiftItems_Clicked()
        {
            /// Nếu chưa chọn pet
            if (this.selectedPet == null)
            {
                KTGlobal.AddNotification("Hãy chọn một tinh linh!");
                return;
            }

            /// Danh sách tên vật phẩm có thể tặng
            HashSet<string> giftItems = new HashSet<string>();
            /// Duyệt danh sách vật phẩm có thể tặng
            foreach (PetConfigXML.FeedItem itemInfo in Loader.Loader.PetConfig.FeedEnlightenmentItems.Values)
            {
                /// Thông tin vật phẩm
                if (Loader.Loader.Items.TryGetValue(itemInfo.ItemID, out ItemData itemData))
                {
                    /// Thêm vào danh sách có thể tặng
                    giftItems.Add(string.Format("<color=yellow>[{0}]</color>", itemData.Name));
                }
            }

            /// Hiện khung chọn vật phẩm
            KTGlobal.ShowInputItems("Tặng quà tinh linh", string.Format("Các vật phẩm có thể <color=orange>tặng</color> cho <color=green>tinh linh</color> để <color=yellow>tăng</color> điểm <color=green>lĩnh ngộ</color>: {0}", string.Join(", ", giftItems)), "<color=green>Lĩnh ngộ</color> dùng để <color=green>học kỹ năng</color> cho <color=yellow>tinh linh</color>.", (itemGD) =>
            {
                /// Nếu là vật phẩm có thể tặng
                return Loader.Loader.PetConfig.FeedEnlightenmentItems.ContainsKey(itemGD.GoodsID);
            }, (itemGDs) =>
            {
                /// Danh sách ID vật phẩm
                List<int> itemDbIDs = itemGDs.Select(x => x.Id).ToList();
                /// Thực thi sự kiện
                this.GiftItems?.Invoke(this.selectedPet.ID, itemDbIDs);
            });
        }

        /// <summary>
        /// Sự kiện khi Button tăng độ vui vẻ
        /// </summary>
        private void ButtonFeedJoy_Clicked()
        {
            /// Nếu chưa chọn pet
            if (this.selectedPet == null)
            {
                KTGlobal.AddNotification("Hãy chọn một tinh linh!");
                return;
            }
            /// Thực thi sự kiện
            this.FeedJoy?.Invoke(this.selectedPet.ID);
        }

        /// <summary>
        /// Sự kiện khi Button tăng tuổi thọ
        /// </summary>
        private void ButtonFeedLife_Clicked()
        {
            /// Nếu chưa chọn pet
            if (this.selectedPet == null)
            {
                KTGlobal.AddNotification("Hãy chọn một tinh linh!");
                return;
            }
            /// Thực thi sự kiện
            this.FeedLife?.Invoke(this.selectedPet.ID);
        }

        /// <summary>
        /// Sự kiện khi Button xuất chiến được ấn
        /// </summary>
        private void ButtonCall_Clicked()
        {
            /// Nếu chưa chọn pet
            if (this.selectedPet == null)
            {
                KTGlobal.AddNotification("Hãy chọn một tinh linh!");
                return;
            }
            /// Thực thi sự kiện
            this.Call?.Invoke(this.selectedPet.ID);
        }

        /// <summary>
        /// Sự kiện khi Button thu hồi được ấn
        /// </summary>
        private void ButtonCallBack_Clicked()
        {
            /// Nếu chưa chọn pet
            if (this.selectedPet == null)
            {
                KTGlobal.AddNotification("Hãy chọn một tinh linh!");
                return;
            }
            /// Thực thi sự kiện
            this.CallBack?.Invoke(this.selectedPet.ID);
        }

        /// <summary>
        /// Sự kiện khi Button phân phối tiềm năng được ấn
        /// </summary>
        private void ButtonDispute_Clicked()
        {
            /// Nếu chưa chọn pet
            if (this.selectedPet == null)
            {
                KTGlobal.AddNotification("Hãy chọn một tinh linh!");
                return;
            }
            /// Mở khung
            this.UI_AssignAttributes.Show();
            /// Đổ dữ liệu
            this.UI_AssignAttributes.Str = this.selectedPet.Str;
            this.UI_AssignAttributes.Dex = this.selectedPet.Dex;
            this.UI_AssignAttributes.Sta = this.selectedPet.Sta;
            this.UI_AssignAttributes.Int = this.selectedPet.Int;
            this.UI_AssignAttributes.RemainPoint = this.selectedPet.RemainPoints;
            /// Sự kiện
            this.UI_AssignAttributes.Accept = (str, dex, sta, ene) =>
            {
                /// Thực thi sự kiện
                this.AssignAttributes?.Invoke(this.selectedPet.ID, str, dex, sta, ene);
            };
        }

        /// <summary>
        /// Sự kiện khi Button tẩy điểm tiềm năng được ấn
        /// </summary>
        private void ButtonResetAttributes_Clicked()
        {
            /// Nếu chưa chọn pet
            if (this.selectedPet == null)
            {
                KTGlobal.AddNotification("Hãy chọn một tinh linh!");
                return;
            }
            /// Xác nhận
            KTGlobal.ShowMessageBox("Tẩy điểm tiềm năng", string.Format("Xác nhận tẩy toàn bộ điểm tiềm năng của tinh linh <color=yellow>[{0}]</color>?", this.selectedPet.Name), () =>
            {
                /// Thực thi sự kiện
                this.ResetAttributes?.Invoke(this.selectedPet.ID);
            }, true);
        }

        /// <summary>
        /// Sự kiện khi Button mở khung học kỹ năng pet được ấn
        /// </summary>
        private void ButtonOpenStudySkill_Clicked()
        {
            /// Nếu chưa chọn pet
            if (this.selectedPet == null)
            {
                KTGlobal.AddNotification("Hãy chọn một tinh linh!");
                return;
            }
            /// Thực thi sự kiện
            this.OpenStudySkill?.Invoke(this.selectedPet);
        }

        /// <summary>
        /// Sự kiện khi Button mở khung chú thích thêm được ấn
        /// </summary>
        private void ButtonShowMoreHint_Clicked()
        {
            this.UI_ExpandedHint.gameObject.SetActive(true);
        }

        /// <summary>
        /// Sự kiện khi Button phóng thích pet được ấn
        /// </summary>
        private void ButtonRelease_Clicked()
        {
            /// Nếu chưa chọn pet
            if (this.selectedPet == null)
            {
                KTGlobal.AddNotification("Hãy chọn một tinh linh!");
                return;
            }

            /// Hỏi lại lần nữa
            KTGlobal.ShowMessageBox("Phóng thích tinh linh", string.Format("Xác nhận <color=orange>phóng thích</color> tinh linh <color=yellow>[{0}]</color>? Sau khi <color=orange>phóng thích</color> sẽ <color=green>không thể lấy lại</color> được.", this.selectedPet.Name), () =>
            {
                /// Thực thi sự kiện
                this.Release?.Invoke(this.selectedPet.ID);
            }, true);
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
            if (Global.Data.RoleData.GoodsDataList != null)
            {
                /// Duyệt danh sách vật phẩm trên người
                foreach (GoodsData itemGD in Global.Data.RoleData.GoodsDataList)
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
            this.UIInput_PetName.text = "";
            this.UIInput_PetName.interactable = false;
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
            /// Hủy button
            this.UIButton_Release.gameObject.SetActive(false);
            this.UIButton_Call.gameObject.SetActive(false);
            this.UIButton_CallBack.gameObject.SetActive(false);
            this.UIButton_ChangeName.interactable = false;
            this.UIButton_GiveItems.interactable = false;
            this.UIButton_FeedJoy.interactable = false;
            this.UIButton_FeedLife.interactable = false;
            this.UIButton_Dispute.gameObject.SetActive(false);
            this.UIButton_ResetAttributes.gameObject.SetActive(false);
            this.UIButton_OpenStudySkill.gameObject.SetActive(false);

            /// Hủy pet đang được chọn
            this.selectedPet = null;

            /// Toác
            if (this._Data == null)
            {
                /// Bỏ qua
                return;
            }

            /// Chú thích thêm
            this.UI_ExpandedHint.Text = string.Format("   - <color=orange>Xuất chiến</color> <color=yellow>tinh linh</color> yêu cầu tối thiểu <color=green>{0} tuổi thọ</color> và <color=green>{1} điểm vui vẻ</color>.\n   - Mỗi lần <color=orange>xuất chiến chủ động</color> hoặc <color=orange>tự xuất chiến khi đăng nhập hay mất kết nối</color> sẽ làm giảm <color=green>{2} điểm vui vẻ</color> của <color=yellow>tinh linh</color>.\n   - <color=yellow>Tinh linh</color> khi <color=yellow>tham chiến</color> sau mỗi <color=green>{3}</color> sẽ giảm <color=green>1 điểm vui vẻ</color>.\n   - Khi <color=green>điểm vui vẻ</color> giảm xuống <color=green>dưới {1}</color>, <color=yellow>tinh linh</color> sẽ <color=orange>tự động</color> chuyển về trạng thái <color=yellow>nghỉ ngơi</color>.\n   - <color=yellow>Tinh linh</color> sẽ nhận được <color=green>{4}% kinh nghiệm</color> khi <color=orange>giết quái</color> của chủ nhân, tối thiểu <color=green>1 điểm</color>.\n   - <color=yellow>Tinh linh</color> sau khi được <color=orange>phóng thích</color> sẽ <color=green>không thể lấy lại</color> được.", Loader.Loader.PetConfig.CallFightRequịreLifeOver, Loader.Loader.PetConfig.CallFightRequịreJoyOver, Loader.Loader.PetConfig.CallFightCostJoy, KTGlobal.DisplayFullDateAndTime(Loader.Loader.PetConfig.SubJoyInterval / 1000f), Loader.Loader.PetConfig.OwnerExpP);

            /// UI Pet ở vị trí đầu tiên
            UIPet_PetInfo uiFirstPet = null;

            /// Duyệt danh sách tinh linh
            foreach (PetData petData in this._Data)
            {
                /// Tạo mới
                UIPet_PetInfo uiPetInfo = GameObject.Instantiate<UIPet_PetInfo>(this.UI_PetInfoPrefab);
                uiPetInfo.transform.SetParent(this.transformPetList, false);
                uiPetInfo.gameObject.SetActive(true);
                uiPetInfo.Data = petData;
                uiPetInfo.Select = () =>
                {
                    /// Đánh dấu pet đang được chọn
                    this.selectedPet = uiPetInfo.Data;
                    /// Thông tin Res
                    if (Loader.Loader.ListPets.TryGetValue(this.selectedPet.ResID, out Entities.Config.PetDataXML petData))
                    {
                        /// Tên Res
                        this.UIText_PetResName.text = petData.Name;
                        /// Loại
                        this.UIText_PetType.text = petData.TypeDesc;
                    }
                    else
                    {
                        /// Tên Res
                        this.UIText_PetResName.text = "Chưa cập nhật";
                        /// Loại
                        this.UIText_PetType.text = "Không rõ";
                    }
                    /// Tên
                    this.UIInput_PetName.interactable = true;
                    this.UIInput_PetName.text = this.selectedPet.Name;
                    /// Cấp
                    this.UIText_PetLevel.text = this.selectedPet.Level.ToString();
                    /// Kinh nghiệm yêu cầu thăng cấp
                    int maxExp = 0;
                    if (this.selectedPet.Level < Loader.Loader.PetConfig.LevelUpExps.Count)
                    {
                        maxExp = Loader.Loader.PetConfig.LevelUpExps[this.selectedPet.Level - 1];
                    }
                    /// Kinh nghiệm
                    this.UIText_PetExp.text = string.Format("{0}/{1}", this.selectedPet.Exp, maxExp);
                    /// Toác
                    if (maxExp == 0)
                    {
                        this.UISlider_PetExp.value = 0f;
                    }
                    else
                    {
                        this.UISlider_PetExp.value = this.selectedPet.Exp / (float) maxExp;
                    }
                    /// Sinh lực
                    int hp = Math.Min(this.selectedPet.HP, this.selectedPet.MaxHP);
                    this.UIText_PetHP.text = string.Format("{0}/{1}", hp, this.selectedPet.MaxHP);
                    this.UISlider_PetHP.value = hp / (float) this.selectedPet.MaxHP;
                    /// Lĩnh ngộ
                    this.UIText_PetEnlightenment.text = this.selectedPet.Enlightenment.ToString();
                    /// Vui vẻ
                    this.UIText_Joyful.text = string.Format("{0}/{1}", this.selectedPet.Joyful, Loader.Loader.PetConfig.MaxJoy);
                    /// Tuổi thọ
                    this.UIText_Life.text = string.Format("{0}/{1}", this.selectedPet.Life, Loader.Loader.PetConfig.MaxLife);
                    /// Sức mạnh
                    this.UIText_Str.text = this.selectedPet.Str.ToString();
                    /// Thân pháp
                    this.UIText_Dex.text = this.selectedPet.Dex.ToString();
                    /// Tiềm năng
                    this.UIText_RemainPoints.text = this.selectedPet.RemainPoints.ToString();
                    /// Ngoại công
                    this.UIText_Sta.text = this.selectedPet.Sta.ToString();
                    /// Nội công
                    this.UIText_Int.text = this.selectedPet.Int.ToString();
                    /// Vật công ngoại
                    this.UIText_PAtk.text = this.selectedPet.PAtk.ToString();
                    /// Vật công nội
                    this.UIText_MAtk.text = this.selectedPet.MAtk.ToString();
                    /// Chính xác
                    this.UIText_Hit.text = this.selectedPet.Hit.ToString();
                    /// Né tránh
                    this.UIText_Dodge.text = this.selectedPet.Dodge.ToString();
                    /// Tốc đánh - ngoại công
                    this.UIText_AtkSpeed.text = this.selectedPet.AtkSpeed.ToString();
                    /// Tốc đánh - nội công
                    this.UIText_CastSpeed.text = this.selectedPet.CastSpeed.ToString();
                    /// Phòng ngự - vật công
                    this.UIText_Def.text = this.selectedPet.PDef.ToString();
                    /// Kháng băng
                    this.UIText_IceRes.text = this.selectedPet.IceRes.ToString();
                    /// Kháng hỏa
                    this.UIText_FireRes.text = this.selectedPet.FireRes.ToString();
                    /// Kháng lôi
                    this.UIText_LightningRes.text = this.selectedPet.LightningRes.ToString();
                    /// Kháng độc
                    this.UIText_PoisonRes.text = this.selectedPet.PoisonRes.ToString();

                    /// Làm rỗng danh sách kỹ năng
                    foreach (Transform child in this.transformSkillList.transform)
                    {
                        if (child.gameObject != this.UI_SkillInfoPrefab.gameObject)
                        {
                            GameObject.Destroy(child.gameObject);
                        }
                    }
                    /// Nếu tồn tại danh sách kỹ năng
                    if (this.selectedPet.Skills != null)
                    {
                        /// Duyệt danh sách kỹ năng
                        foreach (KeyValuePair<int, int> pair in this.selectedPet.Skills)
                        {
                            /// Thông tin kỹ năng
                            if (Loader.Loader.Skills.TryGetValue(pair.Key, out Entities.Config.SkillDataEx skillData))
                            {
                                UIPet_SkillInfo uiSkillInfo = GameObject.Instantiate<UIPet_SkillInfo>(this.UI_SkillInfoPrefab);
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

                    /// Nếu pet này đang xuất chiến
                    if (Global.Data.RoleData.CurrentPetID == this.selectedPet.ID)
                    {
                        /// Hủy tương tác Button mở khung học kỹ năng
                        this.UIButton_OpenStudySkill.interactable = false;
                        /// Hủy tương tác Button phóng thích
                        this.UIButton_Release.interactable = false;
                        /// Ẩn Button xuất chiến
                        this.UIButton_Call.gameObject.SetActive(false);
                        /// Hiện Button thu hồi
                        this.UIButton_CallBack.gameObject.SetActive(true);
                    }
                    /// Nếu pet này đang nghỉ ngơi
                    else
                    {
                        /// Cho tương tác Button mở khung học kỹ năng
                        this.UIButton_OpenStudySkill.interactable = true;
                        /// Cho tương tác Button phóng thích
                        this.UIButton_Release.interactable = true;
                        /// Hiện Button xuất chiến
                        this.UIButton_Call.gameObject.SetActive(true);
                        /// Ẩn Button thu hồi
                        this.UIButton_CallBack.gameObject.SetActive(false);
                    }

                    /// Hiện Button phóng thích
                    this.UIButton_Release.gameObject.SetActive(true);
                    /// Cho tương tác với Button đổi tên và tặng quà
                    this.UIButton_ChangeName.interactable = true;
                    this.UIButton_GiveItems.interactable = true;
                    this.UIButton_FeedJoy.interactable = true;
                    this.UIButton_FeedLife.interactable = true;
                    /// Hiện Button phân phối tiềm năng
                    this.UIButton_Dispute.gameObject.SetActive(true);
                    this.UIButton_ResetAttributes.gameObject.SetActive(true);
                    /// Hiện Button mở khung học kỹ năng
                    this.UIButton_OpenStudySkill.gameObject.SetActive(true);

                    /// Cập nhật hiển thị chiếu pet
                    this.RefreshPetPreview();
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
        private void RefreshPetPreview()
        {
            /// Toác
            if (this.selectedPet == null)
            {
                return;
            }
            
            /// Thông tin Res
            if (Loader.Loader.ListPets.TryGetValue(this.selectedPet.ResID, out Entities.Config.PetDataXML petData))
            {
                /// Nếu chưa tạo thì tạo mới
                if (this.petPreview == null)
                {
                    this.petPreview = Object2DFactory.MakeMonsterPreview();
                }
                /// Hiện preview
                this.UIImage_PetPreview.gameObject.SetActive(true);
                /// Thiết lập preview
                this.petPreview.ResID = petData.ResID;
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

        #region Public methods
        /// <summary>
        /// Cập nhật pet đang tham chiến hiện tại
        /// </summary>
        public void UpdateCurrentPetID()
        {
            /// Nếu không có pet nào đang được chọn
            if (this.selectedPet == null)
            {
                /// Ẩn Button xuất chiến
                this.UIButton_Call.gameObject.SetActive(false);
                /// Ẩn Button thu hồi
                this.UIButton_CallBack.gameObject.SetActive(false);
                /// Ẩn Button phóng thích
                this.UIButton_Release.gameObject.SetActive(false);
                /// Bỏ qua
                return;
            }

            /// Nếu pet này đang xuất chiến
            if (Global.Data.RoleData.CurrentPetID == this.selectedPet.ID)
            {
                /// Ẩn Button xuất chiến
                this.UIButton_Call.gameObject.SetActive(false);
                /// Hiện Button thu hồi
                this.UIButton_CallBack.gameObject.SetActive(true);
                /// Ẩn chức năng phóng thích
                this.UIButton_Release.gameObject.SetActive(true);
                this.UIButton_Release.interactable = false;
            }
            /// Nếu pet này đang nghỉ ngơi
            else
            {
                /// Hiện Button xuất chiến
                this.UIButton_Call.gameObject.SetActive(true);
                /// Ẩn Button thu hồi
                this.UIButton_CallBack.gameObject.SetActive(false);
                /// Hiện chức năng phóng thích
                this.UIButton_Release.gameObject.SetActive(true);
                this.UIButton_Release.interactable = true;
            }
        }

        /// <summary>
        /// Cập nhật tên của pet
        /// </summary>
        /// <param name="petID"></param>
        /// <param name="newName"></param>
        public void UpdateNewName(int petID, string newName)
        {
            /// Nếu không có dữ liệu
            if (this._Data == null)
            {
                /// Bỏ qua
                return;
            }
            /// Thông tin pet tương ứng
            PetData petData = this._Data.Where(x => x.ID == petID).FirstOrDefault();
            /// Nếu tồn tại
            if (petData != null)
            {
                /// Cập nhật tên mới
                petData.Name = newName;
            }

            /// Nếu không có pet nào đang được chọn
            if (this.selectedPet == null)
            {
                /// Bỏ qua
                return;
            }
            /// Nếu pet được chọn không phải pet cần cập nhật
            else if (this.selectedPet.ID != petID)
            {
                /// Bỏ qua
                return;
            }
            /// Cập nhật tên
            this.UIInput_PetName.text = newName;
        }

        /// <summary>
        /// Cập nhật toàn bộ thuộc tính (sức, thân, nội, ngoại và các chỉ số liên quan) của pet
        /// </summary>
        /// <param name="data"></param>
        public void UpdateAttributes(PetData data)
        {
            /// Nếu không có dữ liệu
            if (this._Data == null)
            {
                /// Bỏ qua
                return;
            }
            /// Thông tin pet tương ứng
            PetData petData = this._Data.Where(x => x.ID == data.ID).FirstOrDefault();
            /// Nếu tồn tại
            if (petData != null)
            {
                /// Cập nhật
                petData.Str = data.Str;
                petData.Dex = data.Dex;
                petData.Sta = data.Sta;
                petData.Int = data.Int;
                petData.RemainPoints = data.RemainPoints;
                petData.PAtk = data.PAtk;
                petData.MAtk = data.MAtk;
                petData.Hit = data.Hit;
                petData.Dodge = data.Dodge;
                petData.AtkSpeed = data.AtkSpeed;
                petData.CastSpeed = data.CastSpeed;
                petData.MaxHP = data.MaxHP;
                petData.PDef = data.PDef;
                petData.IceRes = data.IceRes;
                petData.FireRes = data.FireRes;
                petData.LightningRes = data.LightningRes;
                petData.PoisonRes = data.PoisonRes;
            }

            /// Nếu không có pet nào đang được chọn
            if (this.selectedPet == null)
            {
                /// Bỏ qua
                return;
            }
            /// Nếu pet được chọn không phải pet cần cập nhật
            else if (this.selectedPet.ID != data.ID)
            {
                /// Bỏ qua
                return;
            }

            /// Sức mạnh
            this.UIText_Str.text = this.selectedPet.Str.ToString();
            /// Thân pháp
            this.UIText_Dex.text = this.selectedPet.Dex.ToString();
            /// Ngoại công
            this.UIText_Sta.text = this.selectedPet.Sta.ToString();
            /// Nội công
            this.UIText_Int.text = this.selectedPet.Int.ToString();
            /// Tiềm năng
            this.UIText_RemainPoints.text = this.selectedPet.RemainPoints.ToString();
            /// Vật công ngoại
            this.UIText_PAtk.text = this.selectedPet.PAtk.ToString();
            /// Vật công nội
            this.UIText_MAtk.text = this.selectedPet.MAtk.ToString();
            /// Chính xác
            this.UIText_Hit.text = this.selectedPet.Hit.ToString();
            /// Né tránh
            this.UIText_Dodge.text = this.selectedPet.Dodge.ToString();
            /// Tốc đánh - ngoại công
            this.UIText_AtkSpeed.text = this.selectedPet.AtkSpeed.ToString();
            /// Tốc đánh - nội công
            this.UIText_CastSpeed.text = this.selectedPet.CastSpeed.ToString();
            /// Sinh lực
            int hp = Math.Min(this.selectedPet.HP, this.selectedPet.MaxHP);
            this.UIText_PetHP.text = string.Format("{0}/{1}", hp, this.selectedPet.MaxHP);
            this.UISlider_PetHP.value = hp / (float) this.selectedPet.MaxHP;
            /// Phòng ngự - vật công
            this.UIText_Def.text = this.selectedPet.PDef.ToString();
            /// Kháng băng
            this.UIText_IceRes.text = this.selectedPet.IceRes.ToString();
            /// Kháng hỏa
            this.UIText_FireRes.text = this.selectedPet.FireRes.ToString();
            /// Kháng lôi
            this.UIText_LightningRes.text = this.selectedPet.LightningRes.ToString();
            /// Kháng độc
            this.UIText_PoisonRes.text = this.selectedPet.PoisonRes.ToString();

        }

        /// <summary>
        /// Cập nhật thông tin thuộc tính cơ bản của pet
        /// </summary>
        /// <param name="petID"></param>
        /// <param name="joyful"></param>
        /// <param name="life"></param>
        /// <param name="level"></param>
        /// <param name="exp"></param>
        /// <param name="enlightenment"></param>
        public void UpdateBaseAttributes(int petID, int joyful, int life, int level, int exp, int enlightenment)
        {
            /// Nếu không có dữ liệu
            if (this._Data == null)
            {
                /// Bỏ qua
                return;
            }
            /// Thông tin pet tương ứng
            PetData petData = this._Data.Where(x => x.ID == petID).FirstOrDefault();
            /// Nếu tồn tại
            if (petData != null)
            {
                /// Cập nhật
                petData.Joyful = joyful;
                petData.Life = life;
                petData.Level = level;
                petData.Exp = exp;
                petData.Enlightenment = enlightenment;
            }

            /// Nếu không có pet nào đang được chọn
            if (this.selectedPet == null)
            {
                /// Bỏ qua
                return;
            }
            /// Nếu pet được chọn không phải pet cần cập nhật
            else if (this.selectedPet.ID != petID)
            {
                /// Bỏ qua
                return;
            }

            /// Cấp
            this.UIText_PetLevel.text = this.selectedPet.Level.ToString();
            /// Kinh nghiệm yêu cầu thăng cấp
            int maxExp = 0;
            if (this.selectedPet.Level < Loader.Loader.PetConfig.LevelUpExps.Count)
            {
                maxExp = Loader.Loader.PetConfig.LevelUpExps[this.selectedPet.Level - 1];
            }
            /// Kinh nghiệm
            this.UIText_PetExp.text = string.Format("{0}/{1}", this.selectedPet.Exp, maxExp);
            this.UISlider_PetExp.value = this.selectedPet.Exp / (float) maxExp;
            /// Lĩnh ngộ
            this.UIText_PetEnlightenment.text = this.selectedPet.Enlightenment.ToString();
            /// Vui vẻ
            this.UIText_Joyful.text = string.Format("{0}/{1}", this.selectedPet.Joyful, Loader.Loader.PetConfig.MaxJoy);
            /// Tuổi thọ
            this.UIText_Life.text = string.Format("{0}/{1}", this.selectedPet.Life, Loader.Loader.PetConfig.MaxLife);
        }

        /// <summary>
        /// Cập nhật máu của pet
        /// </summary>
        /// <param name="petID"></param>
        /// <param name="hp"></param>
        /// <param name="maxHP"></param>
        public void UpdateHP(int petID, int hp, int maxHP)
        {
            /// Nếu không có dữ liệu
            if (this._Data == null)
            {
                /// Bỏ qua
                return;
            }
            /// Thông tin pet tương ứng
            PetData petData = this._Data.Where(x => x.ID == petID).FirstOrDefault();
            /// Nếu tồn tại
            if (petData != null)
            {
                /// Cập nhật
                petData.HP = hp;
                petData.MaxHP = maxHP;
            }

            /// Nếu không có pet nào đang được chọn
            if (this.selectedPet == null)
            {
                /// Bỏ qua
                return;
            }
            /// Nếu pet được chọn không phải pet cần cập nhật
            else if (this.selectedPet.ID != petID)
            {
                /// Bỏ qua
                return;
            }

            /// Sinh lực
            hp = Math.Min(this.selectedPet.HP, this.selectedPet.MaxHP);
            this.UIText_PetHP.text = string.Format("{0}/{1}", hp, this.selectedPet.MaxHP);
            this.UISlider_PetHP.value = hp / (float) this.selectedPet.MaxHP;
        }

        /// <summary>
        /// Làm mới trang bị pet
        /// </summary>
        public void RefreshEquip()
        {
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
            if (Global.Data.RoleData.GoodsDataList != null)
            {
                /// Duyệt danh sách vật phẩm trên người
                foreach (GoodsData itemGD in Global.Data.RoleData.GoodsDataList)
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
        }
        #endregion
    }
}
