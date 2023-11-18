using FS.GameEngine.Logic;
using FS.VLTK.Logic.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FS.VLTK.Control.Component
{
    /// <summary>
    /// Thiết lập hệ thống
    /// </summary>
    public partial class Monster
    {
        /// <summary>
        /// Thực thi nếu có thiết lập
        /// </summary>
        public void ExecuteSetting()
        {
            if (KTSystemSetting.SkillVolume != this.lastVolume)
            {
                this.audioPlayer.Volume = KTSystemSetting.SkillVolume / 100f;
                this.lastVolume = KTSystemSetting.SkillVolume;
            }

            if (this.lastHideRole != KTSystemSetting.HideNPC)
            {
                this.lastHideRole = KTSystemSetting.HideNPC;
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

                    this.StartCoroutine(ExecuteSkipFrames(1, () => {
                        this.UIHeader.gameObject.SetActive(true);
                    }));
                }
            }

            if (this.RefObject != Global.Data.Leader)
            {
                this.UIHeader.SystemSettingShowHPBar = !KTSystemSetting.HideOtherHPBar;
                this.UIHeader.SystemSettingShowName = !KTSystemSetting.HideOtherName;
            }
        }
    }
}
