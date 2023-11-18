using FS.VLTK.Entities.Config;
using FS.VLTK.Utilities.UnityUI;
using UnityEngine;

namespace FS.VLTK.UI.Main.SelectPet
{
    /// <summary>
    /// Ô kỹ năng trong khung chọn pet
    /// </summary>
    public class UISelectPet_SkillInfo : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Button
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton;

        /// <summary>
        /// Image icon kỹ năng
        /// </summary>
        [SerializeField]
        private SpriteFromAssetBundle UIImage_Icon;
        #endregion

        #region Properties
        private int _SkillID;
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
                /// Nếu tồn tại
                if (Loader.Loader.Skills.TryGetValue(value, out SkillDataEx skillData))
                {
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
        /// Sự kiện khi Button được ấn
        /// </summary>
        private void Button_Clicked()
        {
            /// Nếu thông tin kỹ năng không tồn tại
            if (!Loader.Loader.Skills.TryGetValue(this._SkillID, out SkillDataEx skillData))
            {
                /// Bỏ qua
                return;
            }

            /// Xem trước
            KTGlobal.ShowSkillItemInfo(skillData, this.SkillLevel, this.SkillLevel);
        }
        #endregion
    }
}
