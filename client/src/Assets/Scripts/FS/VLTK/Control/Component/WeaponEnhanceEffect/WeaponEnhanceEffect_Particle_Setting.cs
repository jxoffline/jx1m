using FS.VLTK.Logic.Settings;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace FS.VLTK.Control.Component
{
    /// <summary>
    /// Quản lý luồng thực thi ngầm
    /// </summary>
    public partial class WeaponEnhanceEffect_Particle
    {
        /// <summary>
        /// Thực thi thiết lập hệ thống
        /// </summary>
        public void ExecuteSetting()
        {
            try
            {
                /// Nếu có thiết lập ẩn hiệu ứng cường hóa
                if (KTSystemSetting.HideWeaponEnhanceEffect)
                {
                    /// Nếu đang hiện hiệu ứng
                    if (!this.isEffectDisabled)
                    {
                        this.Pause();
                    }
                    this.isEffectDisabled = true;
                }
                /// Nếu không thiết lập ẩn hiệu ứng cường hóa
                else
                {
                    /// Nếu đang ẩn hiệu ứng
                    if (this.isEffectDisabled)
                    {
                        this.Play();
                    }
                    this.isEffectDisabled = false;
                }
            }
            catch (Exception) { }
        }
    }
}
