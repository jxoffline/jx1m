using FS.VLTK.Utilities.UnityUI;
using UnityEngine;

namespace FS.VLTK.UI.Main.Pet.Main
{
    /// <summary>
    /// Ô kỹ năng của pet
    /// </summary>
    public class UIPet_SkillInfo : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Button
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton;

        /// <summary>
        /// Icon kỹ năng
        /// </summary>
        [SerializeField]
        private SpriteFromAssetBundle UIImage_Icon;
        #endregion

        #region Properties
        private int _SkillID = -1;
        /// <summary>
        /// ID kỹ năng
        /// </summary>
        public int SkillID
        {
            get
            {
                return this._SkillID;
            }
            set
            {
                this._SkillID = value;
                /// Thông tin kỹ năng
                if (Loader.Loader.Skills.TryGetValue(value, out Entities.Config.SkillDataEx skillData))
                {
                    /// Ảnh
                    this.UIImage_Icon.BundleDir = skillData.IconBundleDir;
                    this.UIImage_Icon.AtlasName = skillData.IconAtlasName;
                    this.UIImage_Icon.SpriteName = skillData.Icon;
                    this.UIImage_Icon.Load();
                }
            }
        }

        /// <summary>
        /// Cấp độ kỹ năng
        /// </summary>
        public int SkillLevel { get; set; }
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
            this.UIButton.onClick.AddListener(this.Button_Clicked);
        }

        /// <summary>
        /// Sự kiện khi Button được chọn
        /// </summary>
        private void Button_Clicked()
        {
            /// Thông tin kỹ năng
            if (Loader.Loader.Skills.TryGetValue(this._SkillID, out Entities.Config.SkillDataEx skillData))
            {
                KTGlobal.ShowSkillItemInfo(skillData, this.SkillLevel, this.SkillLevel);
            }
        }
        #endregion
    }
}
