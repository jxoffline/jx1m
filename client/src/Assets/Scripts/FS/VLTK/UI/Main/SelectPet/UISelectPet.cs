using FS.GameEngine.Logic;
using FS.VLTK.Control.Component;
using FS.VLTK.Entities.Config;
using FS.VLTK.Factory;
using FS.VLTK.UI.Main.SelectPet;
using Server.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace FS.VLTK.UI.Main
{
    /// <summary>
    /// Khung chọn pet
    /// </summary>
    public class UISelectPet : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Button đóng khung
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_Close;

        /// <summary>
        /// Image xem trước
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.RawImage UIImage_Preview;

        /// <summary>
        /// Prefab thông tin pet
        /// </summary>
        [SerializeField]
        private UISelectPet_PetInfo UI_PetInfoPrefab;

        /// <summary>
        /// Text tên res
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_ResName;

        /// <summary>
        /// Text tên pet
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_Name;

        /// <summary>
        /// Text cấp độ pet
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_Level;

        /// <summary>
        /// Text loại hình pet
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_TypeDesc;

        /// <summary>
        /// Prefab thông tin kỹ năng
        /// </summary>
        [SerializeField]
        private UISelectPet_SkillInfo UI_SkillInfoPrefab;

        /// <summary>
        /// Button chọn pet
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_SelectPet;
        #endregion

        #region Properties
        /// <summary>
        /// Sự kiện chọn pet
        /// </summary>
        public Action<int> Select { get; set; }

        /// <summary>
        /// Sự kiện đóng khung
        /// </summary>
        public Action Close { get; set; }
        #endregion

        #region Private fields
        /// <summary>
        /// RectTransform danh sách pet
        /// </summary>
        private RectTransform transformPetList = null;

        /// <summary>
        /// RectTransform danh sách kỹ năng
        /// </summary>
        private RectTransform transformSkillList = null;

        /// <summary>
        /// Đối tượng đang được chiếu
        /// </summary>
        private MonsterPreview petPreview = null;

        /// <summary>
        /// Pet được chọn
        /// </summary>
        private PetData selectedPet;
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
            /// Làm mới dữ liệu
            this.RefreshData();
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
            this.UIButton_SelectPet.onClick.AddListener(this.ButtonSelectPet_Clicked);
        }

        /// <summary>
        /// Sự kiện khi Button đóng khung được ấn
        /// </summary>
        private void ButtonClose_Clicked()
        {
            this.Close?.Invoke();
        }

        /// <summary>
        /// Sự kiện khi Button chọn pet được ấn
        /// </summary>
        private void ButtonSelectPet_Clicked()
        {
            /// Nếu không có pet nào được chọn
            if (this.selectedPet == null)
            {
                KTGlobal.AddNotification("Hãy chọn một tinh linh!");
                return;
            }

            /// Đóng khung
            this.Close?.Invoke();
            /// Thực thi sự kiện
            this.Select?.Invoke(this.selectedPet.ID);
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
        private void RefreshData()
        {
            /// Hủy danh sách pet
            foreach (Transform child in this.transformPetList.transform)
            {
                if (child.gameObject != this.UI_PetInfoPrefab.gameObject)
                {
                    GameObject.Destroy(child.gameObject);
                }
            }
            /// Hủy danh sách kỹ năng
            foreach (Transform child in this.transformSkillList.transform)
            {
                if (child.gameObject != this.UI_SkillInfoPrefab.gameObject)
                {
                    GameObject.Destroy(child.gameObject);
                }
            }

            /// Ẩn preview
            this.UIImage_Preview.gameObject.SetActive(false);
            /// Loại
            this.UIText_TypeDesc.text = "";
            /// Tên res
            this.UIText_ResName.text = "Chưa chọn tinh linh";
            /// Tên
            this.UIText_Name.text = "";
            /// Cấp
            this.UIText_Level.text = "";
            /// Hủy button
            this.UIButton_SelectPet.interactable = false;

            /// Nếu không có dữ liệu
            if (Global.Data.RoleData.Pets == null)
            {
                /// Bỏ qua
                return;
            }

            /// Pet đầu tiên
            UISelectPet_PetInfo uiFirstPet = null;
            /// Duyệt danh sách pet
            foreach (PetData petData in Global.Data.RoleData.Pets)
            {
                /// Tạo mới
                UISelectPet_PetInfo uiPetInfo = GameObject.Instantiate<UISelectPet_PetInfo>(this.UI_PetInfoPrefab);
                uiPetInfo.transform.SetParent(this.transformPetList, false);
                uiPetInfo.gameObject.SetActive(true);
                uiPetInfo.Data = petData;
                uiPetInfo.Select = () =>
                {
                    /// Hủy danh sách kỹ năng
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
                            /// Tạo mới
                            UISelectPet_SkillInfo uiSkillInfo = GameObject.Instantiate<UISelectPet_SkillInfo>(this.UI_SkillInfoPrefab);
                            uiSkillInfo.transform.SetParent(this.transformSkillList);
                            uiSkillInfo.gameObject.SetActive(true);
                            uiSkillInfo.SkillID = pair.Key;
                            uiSkillInfo.SkillLevel = pair.Value;
                        }

                        /// Xây lại giao diện
                        this.ExecuteSkipFrames(1, () =>
                        {
                            UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(this.transformSkillList);
                        });
                    }

                    /// Thông tin pet
                    if (Loader.Loader.ListPets.TryGetValue(petData.ResID, out PetDataXML petDataXML))
                    {
                        /// Loại
                        this.UIText_TypeDesc.text = petDataXML.TypeDesc;
                        /// Tên res
                        this.UIText_ResName.text = petDataXML.Name;
                    }
                    /// Không tìm thấy
                    else
                    {
                        /// Loại
                        this.UIText_TypeDesc.text = "Chưa cập nhật";
                        /// Tên res
                        this.UIText_ResName.text = "Chưa cập nhật";
                    }
                    /// Tên
                    this.UIText_Name.text = petData.Name;
                    /// Cấp
                    this.UIText_Level.text = petData.Level.ToString();

                    /// Hiển thị Preview
                    this.RefreshPetPreview(petData);

                    /// Hiện button chọn
                    this.UIButton_SelectPet.interactable = true;

                    /// Đánh dấu Pet được chọn
                    this.selectedPet = petData;
                };

                /// Nếu chưa chọn pet đầu tiên
                if (uiFirstPet == null)
                {
                    /// Đánh dấu pet đầu tiên
                    uiFirstPet = uiPetInfo;
                    /// Kích hoạt
                    uiPetInfo.Active = true;
                }
            }

            /// Xây lại giao diện
            this.ExecuteSkipFrames(1, () =>
            {
                UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(this.transformPetList);
            });
        }

        /// <summary>
        /// Làm mới chiếu pet
        /// </summary>
        /// <param name="data"></param>
        private void RefreshPetPreview(PetData data)
        {
            /// Toác
            if (data == null)
            {
                return;
            }

            /// Thông tin Res
            if (Loader.Loader.ListPets.TryGetValue(data.ResID, out PetDataXML petData))
            {
                /// Hiện preview
                this.UIImage_Preview.gameObject.SetActive(true);

                /// Nếu chưa tạo thì tạo mới
                if (this.petPreview == null)
                {
                    this.petPreview = Object2DFactory.MakeMonsterPreview();
                }
                /// Thiết lập preview
                this.petPreview.ResID = petData.ResID;
                this.petPreview.UpdateResID();

                this.petPreview.Direction = Entities.Enum.Direction.DOWN;
                this.petPreview.OnStart = () =>
                {
                    this.UIImage_Preview.texture = this.petPreview.ReferenceCamera.targetTexture;
                };
                this.petPreview.ResumeCurrentAction();
            }
            /// Toác
            else
            {
                /// Ẩn preview
                this.UIImage_Preview.gameObject.SetActive(false);
            }
        }
        #endregion
    }
}
