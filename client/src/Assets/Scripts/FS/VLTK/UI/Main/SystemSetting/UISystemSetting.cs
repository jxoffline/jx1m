using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;
using FS.VLTK.Utilities.UnityUI;
using FS.GameFramework.Logic;

namespace FS.VLTK.UI.Main
{
    /// <summary>
    /// Khung thiết lập hệ thống
    /// </summary>
    public class UISystemSetting : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Button đóng khung
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_Close;

        /// <summary>
        /// Button lưu thiết lập
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_Save;

        /// <summary>
        /// Slider độ lớn nhạc nền
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Slider UISlider_MusicVolume;

        /// <summary>
        /// Slider độ lớn âm thanh hiệu ứng
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Slider UISlider_EffectSoundVolume;

        /// <summary>
        /// Slider tầm nhìn
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Slider UISlider_FieldOfView;

        /// <summary>
        /// Text hiển thị độ lớn nhạc nền
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_MusicVolume;

        /// <summary>
        /// Text hiển thị độ lớn âm thanh hiệu ứng
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_EffectSoundVolume;

        /// <summary>
        /// Text hiển thị tầm nhìn
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_FieldOfView;

        /// <summary>
        /// Toggle ẩn tên đối tượng khác
        /// </summary>
        [SerializeField]
        private UIToggleSprite UIToggle_HideOtherName;

        /// <summary>
        /// Toggle ẩn thanh máu đối tượng khác
        /// </summary>
        [SerializeField]
        private UIToggleSprite UIToggle_HideOtherHPBar;

        /// <summary>
        /// Toggle ẩn nhân vật
        /// </summary>
        [SerializeField]
        private UIToggleSprite UIToggle_HideLeader;

        /// <summary>
        /// Toggle ẩn người chơi khác
        /// </summary>
        [SerializeField]
        private UIToggleSprite UIToggle_HideOtherRole;

        /// <summary>
        /// Toggle ẩn quái và NPC
        /// </summary>
        [SerializeField]
        private UIToggleSprite UIToggle_HideMonsterAndNPC;

        /// <summary>
        /// Toggle ẩn khung chat nhanh trên đầu người chơi
        /// </summary>
        [SerializeField]
        private UIToggleSprite UIToggle_HidePlayerChat;

        /// <summary>
        /// Toggle ẩn hiệu ứng xuất chiêu
        /// </summary>
        [SerializeField]
        private UIToggleSprite UIToggle_HideSkillCastEffect;

        /// <summary>
        /// Toggle ẩn hiệu ứng nổ của kỹ năng
        /// </summary>
        [SerializeField]
        private UIToggleSprite UIToggle_HideSkillExplodeEffect;

        /// <summary>
        /// Toggle ẩn hiệu ứng Buff
        /// </summary>
        [SerializeField]
        private UIToggleSprite UIToggle_HideSkillBuffEffect;

        /// <summary>
        /// Toggle ẩn đạn
        /// </summary>
        [SerializeField]
        private UIToggleSprite UIToggle_HideBullet;

        /// <summary>
        /// Toggle vô hiệu hóa hiệu ứng đổ bóng
        /// </summary>
        [SerializeField]
        private UIToggleSprite UIToggle_DisableTrailEffect;

        /// <summary>
        /// Toggle ẩn hiệu ứng cường hóa vũ khí
        /// </summary>
        [SerializeField]
        private UIToggleSprite UIToggle_HideWeaponEnhanceEffect;

        /// <summary>
        /// Button thoát Game
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_QuitGame;

        /// <summary>
        /// Button quay trở lại màn hình đăng nhập Game
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_BackToLogin;
        #endregion

        #region Private fields

        #endregion

        #region Properties
        /// <summary>
        /// Sự kiện đóng khung
        /// </summary>
        public Action Close { get; set; }
        
        /// <summary>
        /// Sự kiện lưu thiết lập
        /// </summary>
        public Action SaveSetting { get; set; }

        /// <summary>
        /// Sự kiện thoát khỏi trò chơi
        /// </summary>
        public Action QuitGame { get; set; }

        /// <summary>
        /// Sự kiện quay lại màn hình đăng nhập
        /// </summary>
        public Action BackToLogin { get; set; }

        /// <summary>
        /// Độ lớn âm thanh nhạc nền
        /// </summary>
        public float MusicVolume
        {
            get
            {
                return this.UISlider_MusicVolume.value;
            }
            set
            {
                this.UISlider_MusicVolume.value = value;
                /// Gọi sự kiện để cập nhật giá trị vào Text
                this.SliderMusicVolume_ValueChanged(value);
            }
        }

        /// <summary>
        /// Độ lớn âm thanh hiệu ứng
        /// </summary>
        public float EffectSoundVolume
        {
            get
            {
                return this.UISlider_EffectSoundVolume.value;
            }
            set
            {
                this.UISlider_EffectSoundVolume.value = value;
                /// Gọi sự kiện để cập nhật giá trị vào Text
                this.SliderEffectSoundVolume_ValueChanged(value);
            }
        }

        /// <summary>
        /// Tầm nhìn
        /// </summary>
        public float FieldOfView
        {
            get
            {
                return this.UISlider_FieldOfView.value;
            }
            set
            {
                this.UISlider_FieldOfView.value = value;
                /// Gọi sự kiện để cập nhật giá trị vào Text
                this.SliderFieldOfView_ValueChanged(value);
            }
        }

        /// <summary>
        /// Ẩn tên đối tượng khác
        /// </summary>
        public bool HideOtherName
        {
            get
            {
                return this.UIToggle_HideOtherName.Active;
            }
            set
            {
                this.UIToggle_HideOtherName.Active = value;
            }
        }

        /// <summary>
        /// Ẩn thanh máu đối tượng khác
        /// </summary>
        public bool HideOtherHPBar
        {
            get
            {
                return this.UIToggle_HideOtherHPBar.Active;
            }
            set
            {
                this.UIToggle_HideOtherHPBar.Active = value;
            }
        }

        /// <summary>
        /// Ẩn nhân vật
        /// </summary>
        public bool HideLeader
        {
            get
            {
                return this.UIToggle_HideLeader.Active;
            }
            set
            {
                this.UIToggle_HideLeader.Active = value;
            }
        }

        /// <summary>
        /// Ẩn người chơi khác
        /// </summary>
        public bool HideOtherRole
        {
            get
            {
                return this.UIToggle_HideOtherRole.Active;
            }
            set
            {
                this.UIToggle_HideOtherRole.Active = value;
            }
        }

        /// <summary>
        /// Ẩn quái và NPC
        /// </summary>
        public bool HideMonsterAndNPC
        {
            get
            {
                return this.UIToggle_HideMonsterAndNPC.Active;
            }
            set
            {
                this.UIToggle_HideMonsterAndNPC.Active = value;
            }
        }

        /// <summary>
        /// Ẩn khung chat nhanh trên đầu người chơi
        /// </summary>
        public bool HidePlayerChat
        {
            get
            {
                return this.UIToggle_HidePlayerChat.Active;
            }
            set
            {
                this.UIToggle_HidePlayerChat.Active = value;
            }
        }

        /// <summary>
        /// Ẩn hiệu ứng xuất chiêu
        /// </summary>
        public bool HideSkillCastEffect
        {
            get
            {
                return this.UIToggle_HideSkillCastEffect.Active;
            }
            set
            {
                this.UIToggle_HideSkillCastEffect.Active = value;
            }
        }

        /// <summary>
        /// Ẩn hiệu ứng nổ
        /// </summary>
        public bool HideSkillExplodeEffect
        {
            get
            {
                return this.UIToggle_HideSkillExplodeEffect.Active;
            }
            set
            {
                this.UIToggle_HideSkillExplodeEffect.Active = value;
            }
        }

        /// <summary>
        /// Ẩn hiệu ứng Buff
        /// </summary>
        public bool HideSkillBuffEffect
        {
            get
            {
                return this.UIToggle_HideSkillBuffEffect.Active;
            }
            set
            {
                this.UIToggle_HideSkillBuffEffect.Active = value;
            }
        }

        /// <summary>
        /// Ẩn đạn
        /// </summary>
        public bool HideBullet
        {
            get
            {
                return this.UIToggle_HideBullet.Active;
            }
            set
            {
                this.UIToggle_HideBullet.Active = value;
            }
        }

        /// <summary>
        /// Vô hiệu hóa hiệu ứng đổ bóng
        /// </summary>
        public bool DisableTrailEffect
        {
            get
            {
                return this.UIToggle_DisableTrailEffect.Active;
            }
            set
            {
                this.UIToggle_DisableTrailEffect.Active = value;
            }
        }

        /// <summary>
        /// Ẩn hiệu ứng cường hóa vũ khí
        /// </summary>
        public bool HideWeaponEnhanceEffect
        {
            get
            {
                return this.UIToggle_HideWeaponEnhanceEffect.Active;
            }
            set
            {
                this.UIToggle_HideWeaponEnhanceEffect.Active = value;
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
            this.UIButton_Close.onClick.AddListener(this.ButtonClose_Clicked);
            this.UIButton_Save.onClick.AddListener(this.ButtonSave_Clicked);
            this.UIButton_QuitGame.onClick.AddListener(this.ButtonQuitGame_Clicked);
            this.UIButton_BackToLogin.onClick.AddListener(this.ButtonBackToLogin_Clicked);

            this.UISlider_MusicVolume.onValueChanged.AddListener(this.SliderMusicVolume_ValueChanged);
            this.UISlider_EffectSoundVolume.onValueChanged.AddListener(this.SliderEffectSoundVolume_ValueChanged);
            this.UISlider_FieldOfView.onValueChanged.AddListener(this.SliderFieldOfView_ValueChanged);
        }

        /// <summary>
        /// Sự kiện khi Button thoát Game được ấn
        /// </summary>
        private void ButtonQuitGame_Clicked()
		{
            /// Hỏi lại
            KTGlobal.ShowMessageBox("Thoát Game", "Xác nhận thoát khỏi Game?", () => {
                this.QuitGame?.Invoke();
            }, true);
		}

        /// <summary>
        /// Sự kiện khi Button trở lại màn hình đăng nhập được ấn
        /// </summary>
        private void ButtonBackToLogin_Clicked()
        {
            /// Hỏi lại
            KTGlobal.ShowMessageBox("Thoát Game", "Xác nhận thoát Game và trở lại màn hình đăng nhập tài khoản?", () =>
            {
                this.BackToLogin?.Invoke();
            }, true);
        }

        /// <summary>
        /// Sự kiện khi Button đóng khung được ấn
        /// </summary>
        private void ButtonClose_Clicked()
        {
            this.Close?.Invoke();
        }

        /// <summary>
        /// Sự kiện khi Button lưu thiết lập được ấn
        /// </summary>
        private void ButtonSave_Clicked()
        {
            this.SaveSetting?.Invoke();
        }

        /// <summary>
        /// Sự kiện khi giá trị của Slider độ lớn nhạc nền thay đổi
        /// </summary>
        /// <param name="value"></param>
        private void SliderMusicVolume_ValueChanged(float value)
        {
            int percent = (int) (value * 100);
            this.UIText_MusicVolume.text = string.Format("{0}%", percent);
        }

        /// <summary>
        /// Sự kiện khi giá trị của Slider độ lớn âm thanh hiệu ứng thay đổi
        /// </summary>
        /// <param name="value"></param>
        private void SliderEffectSoundVolume_ValueChanged(float value)
        {
            int percent = (int) (value * 100);
            this.UIText_EffectSoundVolume.text = string.Format("{0}%", percent);
        }

        /// <summary>
        /// Sự kiện khi giá trị của Slider tầm nhìn thay đổi
        /// </summary>
        /// <param name="value"></param>
        private void SliderFieldOfView_ValueChanged(float value)
        {
            this.UIText_FieldOfView.text = string.Format("{0}", (int) value);
        }
        #endregion

        #region Private methods

        #endregion

        #region Public methods
        
        #endregion
    }
}
