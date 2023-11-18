using FS.GameEngine.Logic;
using FS.VLTK.Control.Component;
using FS.VLTK.Factory;
using FS.VLTK.UI.Main.ItemBox;
using FS.VLTK.UI.Main.MainUI.RadarMap;
using FS.VLTK.UI.Main.Pet.Main;
using FS.VLTK.UI.Main.Pet.StudySkill;
using Server.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace FS.VLTK.UI.Main.Pet
{
    /// <summary>
    /// Khung học kỹ năng Pet
    /// </summary>
    public class UIPet_StudySkill : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Button đóng khung
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_Close;

        /// <summary>
        /// Image preview pet
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.RawImage UIImage_PetPreview;

        /// <summary>
        /// Text loại pet
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_Type;

        /// <summary>
        /// Text tên Res
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_ResName;

        /// <summary>
        /// Text tên pet
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_PetName;

        /// <summary>
        /// Text cấp độ pet
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_PetLevel;

        /// <summary>
        /// Text lĩnh ngộ pet
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_PetEnlightenment;

        /// <summary>
        /// Prefab thông tin kỹ năng pet
        /// </summary>
        [SerializeField]
        private UIPet_StudySkill_SkillInfo UI_SkillInfoPrefab;

        /// <summary>
        /// Text yêu cầu lĩnh ngộ pet
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_RequireEnlightenment;

        /// <summary>
        /// Text tỷ lệ thành công
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_SuccessRate;

        /// <summary>
        /// Item sách kỹ năng
        /// </summary>
        [SerializeField]
        private UIItemBox UIItem_Scroll;

        /// <summary>
        /// Kỹ năng sẽ học được hoặc thăng cấp
        /// </summary>
        [SerializeField]
        private UIPet_StudySkill_SkillInfo UI_SkillInfo;

        /// <summary>
        /// Item lĩnh hội đan
        /// </summary>
        [SerializeField]
        private UIItemBox UIItem_Medicine;

        /// <summary>
        /// Button học kỹ năng
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_Study;

        /// <summary>
        /// Khung chọn vật phẩm
        /// </summary>
        [SerializeField]
        private UISelectItem UISelectItem;
        #endregion

        #region Properties
        private PetData _Data;
        /// <summary>
        /// Dữ liệu Pet
        /// </summary>
        public PetData Data
        {
            get
            {
                return this._Data;
            }
            set
            {
                this._Data = value;
                /// Làm mới giao diện
                this.DoRefreshData();
            }
        }

        /// <summary>
        /// Sự kiện đóng khung
        /// </summary>
        public Action Close { get; set; }

        /// <summary>
        /// Sự kiện học kỹ năng
        /// </summary>
        public Action<int, int> Study { get; set; }
        #endregion

        #region Private fields
        /// <summary>
        /// Đối tượng đang được chiếu
        /// </summary>
        private MonsterPreview petPreview = null;

        /// <summary>
        /// RectTransform danh sách kỹ năng
        /// </summary>
        private RectTransform skillListTransform;
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi khi đối tượng được tạo ra
        /// </summary>
        private void Awake()
        {
            this.skillListTransform = this.UI_SkillInfoPrefab.transform.parent.GetComponent<RectTransform>();
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
            this.UIButton_Study.onClick.AddListener(this.ButtonStudy_Clicked);
        }

        /// <summary>
        /// Sự kiện khi Button đóng khung được ấn
        /// </summary>
        private void ButtonClose_Clicked()
        {
            this.Close?.Invoke();
        }

        /// <summary>
        /// Sự kiện khi Button học kỹ năng được ấn
        /// </summary>
        private void ButtonStudy_Clicked()
        {
            /// Toác
            if (this._Data == null)
            {
                KTGlobal.AddNotification("Thông tin tinh linh bị lỗi. Hãy thử lại!");
                this.Close?.Invoke();
                return;
            }
            /// Không có sách kỹ năng
            else if (this.UIItem_Scroll.Data == null)
            {
                KTGlobal.AddNotification("Hãy chọn sách kỹ năng tinh linh cần học!");
                return;
            }

            /// Thực thi sự kiện
            this.Study?.Invoke(this.UIItem_Scroll.Data.Id, this.UIItem_Medicine.Data == null ? -1 : this.UIItem_Medicine.Data.Id);
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Luồng thực thi sự kiện bỏ qua một số Frame
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
        private void ExecuteSkipFrames(int skip, Action work)
        {
            this.StartCoroutine(this.DoExecuteSkipFrames(skip, work));
        }

        /// <summary>
        /// Làm mới dữ liệu
        /// </summary>
        private void DoRefreshData()
        {
            /// Toác
            if (this._Data == null)
            {
                KTGlobal.AddNotification("Thông tin tinh linh bị lỗi. Hãy thử lại!");
                this.Close?.Invoke();
                return;
            }

            /// Làm rỗng danh sách kỹ năng
            foreach (Transform child in this.skillListTransform.transform)
            {
                if (child.gameObject != this.UI_SkillInfoPrefab.gameObject)
                {
                    GameObject.Destroy(child.gameObject);
                }
            }
            /// Ẩn preview
            this.UIImage_PetPreview.gameObject.SetActive(false);
            /// Ẩn sách
            this.UIItem_Scroll.Data = null;
            /// Ẩn lĩnh hội đan
            this.UIItem_Medicine.Data = null;
            /// Ẩn button
            this.UIButton_Study.interactable = false;

            /// Thông tin Res
            if (Loader.Loader.ListPets.TryGetValue(this._Data.ResID, out Entities.Config.PetDataXML petData))
            {
                /// Tên Res
                this.UIText_ResName.text = petData.Name;
                /// Loại
                this.UIText_Type.text = petData.TypeDesc;
            }
            else
            {
                /// Tên Res
                this.UIText_ResName.text = "Chưa cập nhật";
                /// Loại
                this.UIText_Type.text = "Chưa rõ";
            }
            /// Tên pet
            this.UIText_PetName.text = this._Data.Name;
            /// Cấp độ
            this.UIText_PetLevel.text = this._Data.Level.ToString();
            /// Lĩnh ngộ
            this.UIText_PetEnlightenment.text = this._Data.Enlightenment.ToString();

            /// Ẩn kỹ năng sẽ học được
            this.UI_SkillInfo.SkillID = -1;
            this.UI_SkillInfo.SkillLevel = 0;

            /// Nếu tồn tại danh sách kỹ năng
            if (this._Data.Skills != null)
            {
                /// Duyệt danh sách kỹ năng
                foreach (KeyValuePair<int, int> pair in this._Data.Skills)
                {
                    /// Thông tin kỹ năng
                    if (Loader.Loader.Skills.TryGetValue(pair.Key, out Entities.Config.SkillDataEx skillData))
                    {
                        UIPet_StudySkill_SkillInfo uiSkillInfo = GameObject.Instantiate<UIPet_StudySkill_SkillInfo>(this.UI_SkillInfoPrefab);
                        uiSkillInfo.transform.SetParent(this.skillListTransform, false);
                        uiSkillInfo.gameObject.SetActive(true);
                        uiSkillInfo.SkillID = pair.Key;
                        uiSkillInfo.SkillLevel = pair.Value;
                    }
                }
                /// Xây lại giao diện
                this.ExecuteSkipFrames(1, () =>
                {
                    UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(this.skillListTransform);
                });
            }

            /// Yêu cầu lĩnh ngộ
            this.UIText_RequireEnlightenment.text = "";
            /// Tỷ lệ thành công
            this.UIText_SuccessRate.text = "";

            /// Button sách
            this.UIItem_Scroll.Click = () =>
            {
                /// Nếu chưa có vật phẩm
                if (this.UIItem_Scroll.Data == null)
                {
                    this.UISelectItem.Title = "Chọn sách kỹ năng tinh linh.";
                    this.UISelectItem.Items = Global.Data.RoleData.GoodsDataList?.Where(x => Loader.Loader.PetConfig.SkillScrolls.ContainsKey(x.GoodsID)).ToList();
                    this.UISelectItem.ItemSelected = (itemGD) =>
                    {
                        /// Thông tin sách
                        if (Loader.Loader.PetConfig.SkillScrolls.TryGetValue(itemGD.GoodsID, out Entities.Config.PetConfigXML.SkillScroll scrollData))
                        {
                            /// Nếu chưa có kỹ năng hoặc kỹ năng chưa tồn tại
                            if (this._Data.Skills == null || !this._Data.Skills.ContainsKey(scrollData.SkillID))
                            {
                                /// Yêu cầu lĩnh ngộ
                                this.UIText_RequireEnlightenment.text = "0";
                                /// Tỷ lệ thành công
                                this.UIText_SuccessRate.text = "100";
                                /// Ẩn button
                                this.UIButton_Study.interactable = true;
                                /// Thiết lập vật phẩm
                                this.UIItem_Scroll.Data = itemGD;

                                /// Thông tin kỹ năng
                                if (Loader.Loader.Skills.TryGetValue(scrollData.SkillID, out Entities.Config.SkillDataEx skillData))
                                {
                                    this.UI_SkillInfo.SkillID = skillData.ID;
                                    this.UI_SkillInfo.SkillLevel = 1;
                                }
                                else
                                {
                                    this.UI_SkillInfo.SkillID = -1;
                                    this.UI_SkillInfo.SkillLevel = 0;
                                }
                            }
                            /// Thông tin kỹ năng theo cấp
                            else if (this._Data.Skills.TryGetValue(scrollData.SkillID, out int skillLevel) && skillLevel > 0 && skillLevel < Loader.Loader.PetConfig.SkillLevelUps.Count)
                            {
                                /// Yêu cầu lĩnh ngộ
                                this.UIText_RequireEnlightenment.text = Loader.Loader.PetConfig.SkillLevelUps[skillLevel - 1].RequireEnlightenment.ToString();
                                /// Tỷ lệ thành công
                                this.UIText_SuccessRate.text = string.Format("{0}%", Loader.Loader.PetConfig.SkillLevelUps[skillLevel - 1].Rate);
                                /// Ẩn button
                                this.UIButton_Study.interactable = true;
                                /// Thiết lập vật phẩm
                                this.UIItem_Scroll.Data = itemGD;


                                /// Thông tin kỹ năng
                                if (Loader.Loader.Skills.TryGetValue(scrollData.SkillID, out Entities.Config.SkillDataEx skillData))
                                {
                                    this.UI_SkillInfo.SkillID = skillData.ID;
                                    this.UI_SkillInfo.SkillLevel = skillLevel + 1;
                                }
                                else
                                {
                                    this.UI_SkillInfo.SkillID = -1;
                                    this.UI_SkillInfo.SkillLevel = 0;
                                }
                            }
                            /// Toác
                            else
                            {
                                /// Yêu cầu lĩnh ngộ
                                this.UIText_RequireEnlightenment.text = "";
                                /// Tỷ lệ thành công
                                this.UIText_SuccessRate.text = "";
                                /// Ẩn button
                                this.UIButton_Study.interactable = false;
                                /// Thông báo lỗi
                                KTGlobal.AddNotification("Vật phẩm không phù hợp!");
                            }
                        }
                        /// Toác
                        else
                        {
                            /// Yêu cầu lĩnh ngộ
                            this.UIText_RequireEnlightenment.text = "";
                            /// Tỷ lệ thành công
                            this.UIText_SuccessRate.text = "";
                            /// Ẩn button
                            this.UIButton_Study.interactable = false;
                            /// Thông báo lỗi
                            KTGlobal.AddNotification("Vật phẩm không phù hợp!");
                        }
                    };
                    this.UISelectItem.Show();
                }
                /// Nếu đã có vật phẩm
                else
                {
                    List<KeyValuePair<string, Action>> buttons = new List<KeyValuePair<string, Action>>();
                    buttons.Add(new KeyValuePair<string, Action>("Gỡ xuống", () =>
                    {
                        this.UIItem_Scroll.Data = null;
                        KTGlobal.CloseItemInfo();
                    }));
                    KTGlobal.ShowItemInfo(this.UIItem_Scroll.Data, buttons);
                }
            };

            /// Button lĩnh hội đan
            this.UIItem_Medicine.Click = () =>
            {
                /// Nếu chưa có vật phẩm
                if (this.UIItem_Medicine.Data == null)
                {
                    this.UISelectItem.Title = "Chọn lĩnh hội đan.";
                    this.UISelectItem.Items = Global.Data.RoleData.GoodsDataList?.Where(x => Loader.Loader.PetConfig.SkillStudyMedicineItemID == x.GoodsID).ToList();
                    this.UISelectItem.ItemSelected = (itemGD) =>
                    {
                        /// Thiết lập vật phẩm
                        this.UIItem_Medicine.Data = itemGD;
                    };
                    this.UISelectItem.Show();
                }
                /// Nếu đã có vật phẩm
                else
                {
                    List<KeyValuePair<string, Action>> buttons = new List<KeyValuePair<string, Action>>();
                    buttons.Add(new KeyValuePair<string, Action>("Gỡ xuống", () =>
                    {
                        this.UIItem_Medicine.Data = null;
                        KTGlobal.CloseItemInfo();
                    }));
                    KTGlobal.ShowItemInfo(this.UIItem_Medicine.Data, buttons);
                }
            };

            /// Xây lại giao diện
            this.ExecuteSkipFrames(1, () =>
            {
                UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(this.skillListTransform);
            });

            /// Làm mới chiếu pet
            this.RefreshPetPreview();
        }

        /// <summary>
        /// Làm mới chiếu pet
        /// </summary>
        private void RefreshPetPreview()
        {
            /// Toác
            if (this._Data == null)
            {
                return;
            }

            /// Thông tin Res
            if (Loader.Loader.ListPets.TryGetValue(this._Data.ResID, out Entities.Config.PetDataXML petData))
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
        /// Làm mới dữ liệu
        /// </summary>
        public void RefreshData()
        {
            this.DoRefreshData();
        }
        #endregion
    }
}
