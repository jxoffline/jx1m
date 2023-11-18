using FS.GameEngine.Logic;
using FS.VLTK.Logic.Settings;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FS.VLTK.Control.Component
{
    /// <summary>
    /// Thiết lập hệ thống
    /// </summary>
    public partial class Character
    {
        /// <summary>
        /// Thực thi thiết lập hệ thống
        /// </summary>
        public void ExecuteSetting()
        {
            if (KTSystemSetting.SkillVolume != this.lastVolume)
            {
                this.audioPlayer.Volume = KTSystemSetting.SkillVolume / 100f;
                this.lastVolume = KTSystemSetting.SkillVolume;
            }

            if ((this.RefObject == Global.Data.Leader && this.lastHideRole != KTSystemSetting.HideRole) || (this.RefObject != Global.Data.Leader && this.lastHideRole != KTSystemSetting.HideOtherRole))
            {
                this.lastHideRole = this.RefObject == Global.Data.Leader ? KTSystemSetting.HideRole : KTSystemSetting.HideOtherRole;
                if (!this.lastHideRole)
                {
                    this.ResumeActions();
                    this.SetModelVisible(true);

                    this.ResumeCurrentAction();
                }
                else
                {
                    this.PauseAllActions();
                    this.SetModelVisible(false);
                }
            }

            if (this.RefObject != Global.Data.Leader)
            {
                this.UIHeader.SystemSettingShowHPBar = !KTSystemSetting.HideOtherHPBar;
                this.UIHeader.SystemSettingShowName = !KTSystemSetting.HideOtherName;
            }

            /// Thực thi thiết lập hệ thống cho hiệu ứng cường hóa
            this.animation.LeftWeaponEnhanceEffects.ExecuteSetting();
            this.animation.RightWeaponEnhanceEffects.ExecuteSetting();
        }
    }
}
