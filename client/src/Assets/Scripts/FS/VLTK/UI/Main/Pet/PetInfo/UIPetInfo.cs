using FS.VLTK.Control.Component;
using FS.VLTK.Factory;
using FS.VLTK.UI.Main.Pet.PetInfo;
using Server.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace FS.VLTK.UI.Main.Pet
{
    /// <summary>
    /// Khung thông tin pet
    /// </summary>
    public class UIPetInfo : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Button đóng khung
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_Close;

        /// <summary>
        /// Text tên Res
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_ResName;

        /// <summary>
        /// Text loại pet
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_Type;

        /// <summary>
        /// Image preview res pet
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.RawImage UI_Preview;

        /// <summary>
        /// Text tên
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_Name;

        /// <summary>
        /// Text cấp độ
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_Level;

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
        /// Text lĩnh ngộ
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_Enlightenment;

        /// <summary>
        /// Prefab kỹ năng
        /// </summary>
        [SerializeField]
        private UIPetInfo_Skill UI_SkillPrefab;

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
        /// Text vật công - ngoại
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_PAtk;

        /// <summary>
        /// Text vật công - nội
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
        /// Text tốc đánh - ngoại
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_AtkSpeed;

        /// <summary>
        /// Text tốc đánh - nội
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_CastSpeed;

        /// <summary>
        /// Text phòng ngự - vật lý
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_PDef;

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
        #endregion

        #region Properties
        private PetData _Data;
        /// <summary>
        /// Thông tin pet
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
                /// Làm mới dữ liệu
                this.Refresh();
            }
        }

        /// <summary>
        /// Sự kiện đóng khung
        /// </summary>
        public Action Close { get; set; }
        #endregion

        #region Private fields
        /// <summary>
        /// RectTransform danh sách kỹ năng
        /// </summary>
        private RectTransform transformSkillList;

        /// <summary>
        /// Đối tượng đang được chiếu
        /// </summary>
        private MonsterPreview petPreview = null;
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi khi đối tượng được tạo ra
        /// </summary>
        private void Awake()
        {
            this.transformSkillList = this.UI_SkillPrefab.transform.parent.GetComponent<RectTransform>();
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
        private void Refresh()
        {
            /// Toác
            if (this._Data == null)
            {
                /// Đóng khung
                this.Close?.Invoke();
                /// Bỏ qua
                return;
            }

            /// Thông tin Res
            if (!Loader.Loader.ListPets.TryGetValue(this._Data.ResID, out Entities.Config.PetDataXML petData))
            {
                /// Đóng khung
                this.Close?.Invoke();
                /// Bỏ qua
                return;
            }

            /// Làm rỗng danh sách kỹ năng
            foreach (Transform child in this.transformSkillList.transform)
            {
                if (child.gameObject != this.UI_SkillPrefab.gameObject)
                {
                    GameObject.Destroy(child.gameObject);
                }
            }
            /// Nếu tồn tại danh sách kỹ năng
            if (this._Data.Skills != null)
            {
                /// Duyệt danh sách kỹ năng
                foreach (KeyValuePair<int, int> pair in this._Data.Skills)
                {
                    /// Tạo mới
                    UIPetInfo_Skill uiSkill = GameObject.Instantiate<UIPetInfo_Skill>(this.UI_SkillPrefab);
                    uiSkill.transform.SetParent(this.transformSkillList, false);
                    uiSkill.gameObject.SetActive(true);
                    uiSkill.SkillID = pair.Key;
                    uiSkill.SkillLevel = pair.Value;
                }
            }
            /// Xây lại giao diện kỹ năng
            this.ExecuteSkipFrames(1, () =>
            {
                UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(this.transformSkillList);
            });

            /// Tên Res
            this.UIText_ResName.text = petData.Name;
            /// Loại pet
            this.UIText_Type.text = petData.TypeDesc;
            /// Tên
            this.UIText_Name.text = this._Data.Name;
            /// Cấp
            this.UIText_Level.text = this._Data.Level.ToString();
            /// Độ vui vẻ
            this.UIText_Joyful.text = this._Data.Joyful.ToString();
            /// Tuổi thọ
            this.UIText_Life.text = this._Data.Life.ToString();
            /// Lĩnh ngộ
            this.UIText_Enlightenment.text = this._Data.Enlightenment.ToString();
            /// Sức
            this.UIText_Str.text = this._Data.Str.ToString();
            /// Thân
            this.UIText_Dex.text = this._Data.Dex.ToString();
            /// Ngoại
            this.UIText_Sta.text = this._Data.Sta.ToString();
            /// Nội
            this.UIText_Int.text = this._Data.Int.ToString();
            /// Tiềm năng
            this.UIText_RemainPoints.text = this._Data.RemainPoints.ToString();
            /// Vật công ngoại
            this.UIText_PAtk.text = this._Data.PAtk.ToString();
            /// Vật công nội
            this.UIText_MAtk.text = this._Data.MAtk.ToString();
            /// Chính xác
            this.UIText_Hit.text = this._Data.Hit.ToString();
            /// Né tránh
            this.UIText_Dodge.text = this._Data.Dodge.ToString();
            /// Tốc đánh ngoại
            this.UIText_AtkSpeed.text = this._Data.AtkSpeed.ToString();
            /// Tốc đánh nội
            this.UIText_CastSpeed.text = this._Data.CastSpeed.ToString();
            /// Kháng vật
            this.UIText_PDef.text = this._Data.PDef.ToString();
            /// Kháng độc
            this.UIText_PoisonRes.text = this._Data.PoisonRes.ToString();
            /// Kháng băng
            this.UIText_IceRes.text = this._Data.IceRes.ToString();
            /// Kháng hỏa
            this.UIText_FireRes.text = this._Data.FireRes.ToString();
            /// Kháng lôi
            this.UIText_LightningRes.text = this._Data.LightningRes.ToString();

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
                /// Thiết lập preview
                this.petPreview.ResID = petData.ResID;
                this.petPreview.UpdateResID();

                this.petPreview.Direction = Entities.Enum.Direction.DOWN;
                this.petPreview.OnStart = () =>
                {
                    this.UI_Preview.texture = this.petPreview.ReferenceCamera.targetTexture;
                };
                this.petPreview.ResumeCurrentAction();
            }
        }
        #endregion
    }
}
