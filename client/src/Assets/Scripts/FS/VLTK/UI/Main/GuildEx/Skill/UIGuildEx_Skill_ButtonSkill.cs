using FS.VLTK.Utilities.UnityUI;
using Server.Data;
using System;
using System.Linq;
using TMPro;
using UnityEngine;

namespace FS.VLTK.UI.Main.GuildEx.Skill
{
    /// <summary>
    /// Ô kỹ năng trong khung kỹ năng bang hội
    /// </summary>
    public class UIGuildEx_Skill_ButtonSkill : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Toggle
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Toggle UIToggle;

        /// <summary>
        /// Image icon kỹ năng
        /// </summary>
        [SerializeField]
        private SpriteFromAssetBundle UIImage_SkillIcon;

        /// <summary>
        /// Text tên kỹ năng
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_SkillName;

        /// <summary>
        /// Text mô tả kỹ năng
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_SkillDesc;

        /// <summary>
        /// Text cấp độ kỹ năng
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_SkillLevel;
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
                /// Thông tin kỹ năng
                GuildSkill skillData = Loader.Loader.GuildConfig.Skills.Where(x => x.ID == value).FirstOrDefault();
                /// Nếu tồn tại
                if (skillData != null)
                {
                    /// Thiết lập dữ liệu
                    this.UIText_SkillName.text = skillData.Name;
                    this.UIText_SkillDesc.text = skillData.Desc;
                    this.UIImage_SkillIcon.SpriteName = skillData.Icon;
                    this.UIImage_SkillIcon.Load();
                }
            }
        }

        private int _Level;
        /// <summary>
        /// Cấp độ kỹ năng
        /// </summary>
        public int Level
        {
            get
            {
                return this._Level;
            }
            set
            {
                this._Level = value;
                /// Thiết lập dữ liệu
                if (value <= 0)
                {
                    this.UIText_SkillLevel.text = string.Format("<color=#ff2929>{0}</color>", "Chưa kích hoạt");
                }
                else
                {
                    this.UIText_SkillLevel.text = string.Format("Cấp: <color=#64f766>{0}</color>", value);
                }
            }
        }

        /// <summary>
        /// Sự kiện chọn kỹ năng này
        /// </summary>
        public Action Select { get; set; }
        
        /// <summary>
        /// Chọn kỹ năng này
        /// </summary>
        public bool Active
        {
            get
            {
                return this.UIToggle.isOn;
            }
            set
            {
                this.UIToggle.isOn = value;
            }
        }
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
            this.UIToggle.onValueChanged.AddListener(this.Toggle_Selected);
        }

        /// <summary>
        /// Sự kiện khi Toggle được chọn
        /// </summary>
        /// <param name="isSelected"></param>
        private void Toggle_Selected(bool isSelected)
        {
            if (isSelected)
            {
                this.Select?.Invoke();
            }
        }
        #endregion
    }
}
