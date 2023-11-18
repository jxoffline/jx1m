using FS.GameEngine.Logic;
using FS.VLTK.Logic.BackgroundWork;
using System;
using System.Collections.Generic;
using System.Linq;
//using UnityEngine.Rendering.PostProcessing;

namespace FS.VLTK.Logic.Settings
{
    /// <summary>
    /// Thực thi thiết lập hệ thống
    /// </summary>
    public static partial class KTSystemSetting
    {
        /// <summary>
        /// Thực thi thiết lập hệ thống
        /// </summary>
        public static void Apply()
        {
            /// Hiệu ứng
            Global.MainCamera.GetComponent<MobilePostProcessing>().enabled = !KTSystemSetting.HideWeaponEnhanceEffect;

            /// Cập nhật tầm nhìn Camera
            Global.MainCamera.orthographicSize = KTSystemSetting.FieldOfView;

            /// Cập nhật lên toàn bộ đối tượng động
            KTBackgroundWorkManager.Instance.ExecuteSystemSettingOnGameObjects();
        }
    }
}
