using FS.VLTK.Utilities.UnityUI;
using Server.Data;
using System;
using TMPro;
using UnityEngine;

namespace FS.VLTK.UI.Main.SetExpSkill
{
    /// <summary>
    /// Button thông tin kỹ năng trong khung thiết lập kỹ năng tu luyện
    /// </summary>
    public class UISetExpSkill_SkillButton : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Image icon kỹ năng
        /// </summary>
        [SerializeField]
        private SpriteFromAssetBundle UIImage_Icon;

        /// <summary>
        /// Button thông tin kỹ năng
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_SkillInfo;

        /// <summary>
        /// Text tên kỹ năng
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_Name;

        /// <summary>
        /// Text mô tả ngắn kỹ năng
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_ShortDesc;

        /// <summary>
        /// Text cấp độ kỹ năng
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_Level;

        /// <summary>
        /// Slider % kinh nghiệm
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Slider UISlider_ExpPercent;

        /// <summary>
        /// Text % kinh nghiệm
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_ExpPercent;

        /// <summary>
        /// Button thiết lập tu luyện
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_Set;

        /// <summary>
        /// Mark kỹ năng hiện tại đang tu luyện
        /// </summary>
        [SerializeField]
        private RectTransform UIMark_Current;
        #endregion

        #region Properties
        private ExpSkillData _Data;
        /// <summary>
        /// Dữ liệu
        /// </summary>
        public ExpSkillData Data
        {
            get
            {
                return this._Data;
            }
            set
            {
                this._Data = value;

                /// Thông tin kỹ năng
                if (!Loader.Loader.Skills.TryGetValue(value.SkillID, out Entities.Config.SkillDataEx skillData))
                {
                    /// Toác
                    return;
                }

                /// Icon kỹ năng
                this.UIImage_Icon.BundleDir = skillData.IconBundleDir;
                this.UIImage_Icon.AtlasName = skillData.IconAtlasName;
                this.UIImage_Icon.SpriteName = skillData.Icon;
                this.UIImage_Icon.Load();
                /// Tên kỹ năng
                this.UIText_Name.text = skillData.Name;
                /// Mô tả ngắn
                this.UIText_ShortDesc.text = skillData.ShortDesc;
                /// Cấp độ
                this.UIText_Level.text = value.Level.ToString();
                /// % kinh nghiệm
                int expPercent = value.LevelUpExp == 0 ? 0 : value.CurrentExp * 100 / value.LevelUpExp;
                this.UIText_ExpPercent.text = string.Format("{0}%", expPercent);
                this.UISlider_ExpPercent.value = expPercent;
                /// Đánh dấu kỹ năng tu luyện
                this.RefreshMark();
            }
        }

        /// <summary>
        /// Sự kiện thiết lập làm kỹ năng tu luyện
        /// </summary>
        public Action SetAsCurrentExpSkill { get; set; }
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

        #region Private methods
        /// <summary>
        /// Khởi tạo ban đầu
        /// </summary>
        private void InitPrefabs()
        {
            this.UIButton_Set.onClick.AddListener(this.Button_Clicked);
            this.UIButton_SkillInfo.onClick.AddListener(this.ButtonSkillInfo_Clicked);
        }

        /// <summary>
        /// Sự kiện khi Button được ấn
        /// </summary>
        private void Button_Clicked()
        {
            /// Thực thi sự kiện
            this.SetAsCurrentExpSkill?.Invoke();
        }

        /// <summary>
        /// Sự kiện khi Button thông tin kỹ năng được ấn
        /// </summary>
        private void ButtonSkillInfo_Clicked()
        {
            /// Toác
            if (this._Data == null)
            {
                return;
            }
            /// Thông tin kỹ năng
            if (!Loader.Loader.Skills.TryGetValue(this._Data.SkillID, out Entities.Config.SkillDataEx skillData))
            {
                /// Toác
                return;
            }

            /// Hiện Tooltip kỹ năng
            KTGlobal.ShowSkillItemInfo(skillData, this._Data.Level, this._Data.Level);
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Làm mới hiển thị có phải kỹ năng đang tu luyện không
        /// </summary>
        public void RefreshMark()
        {
            /// Toác
            if (this._Data == null)
            {
                /// Bỏ qua
                return;
            }

            /// Nếu là kỹ năng đang tu luyện
            if (this._Data.IsCurrentExpSkill)
            {
                /// Ẩn button luyện
                this.UIButton_Set.interactable = false;
                /// Hiện Mark đang tu luyện
                this.UIMark_Current.gameObject.SetActive(true);
            }
            /// Nếu không phải kỹ năng đang tu luyện
            else
            {
                /// Hiện Button luyện
                this.UIButton_Set.interactable = true;
                /// Ẩn Mark đang tu luyện
                this.UIMark_Current.gameObject.SetActive(false);
            }
        }
        #endregion
    }
}
