using FS.GameEngine.Logic;
using FS.VLTK.Network;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FS.VLTK.Logic.Settings
{
    /// <summary>
    /// Thiết lập hệ thống
    /// </summary>
    public static partial class KTSystemSetting
    {
        #region Indexes
        /// <summary>
        /// ID loại thiết lập hệ thống
        /// </summary>
        public enum SystemSettingIndex
        {
            /// <summary>
            /// Độ lớn nhạc nền
            /// </summary>
            MusicVolume = 0,
            /// <summary>
            /// Độ lớn âm thanh kỹ năng
            /// </summary>
            SkillVolume = 1,
            /// <summary>
            /// Tầm nhìn
            /// </summary>
            FieldOfView = 2,
            /// <summary>
            /// Hiển thị tên đối tượng khác
            /// </summary>
            HideOtherName = 3,
            /// <summary>
            /// Hiển thị thanh máu đối tượng khác
            /// </summary>
            HideOtherHPBar = 4,
            /// <summary>
            /// Ẩn nhân vật
            /// </summary>
            HideRole = 5,
            /// <summary>
            /// Ẩn người chơi khác
            /// </summary>
            HideOtherRole = 6,
            /// <summary>
            /// Ẩn NPC và quái
            /// </summary>
            HideNPC = 7,
            /// <summary>
            /// Ẩn hiệu ứng xuất chiêu
            /// </summary>
            HideSkillCastEffect = 8,
            /// <summary>
            /// Ẩn hiệu ứng phát nổ của kỹ năng
            /// </summary>
            HideSkillExplodeEffect = 9,
            /// <summary>
            /// Ẩn hiệu ứng đạn
            /// </summary>
            HideSkillBullet = 10,
            /// <summary>
            /// Ẩn hiệu ứng Buff
            /// </summary>
            HideSkillBuffEffect = 11,
            /// <summary>
            /// Tắt hiệu ứng đổ bóng
            /// </summary>
            DisableTrailEffect = 12,
            /// <summary>
            /// Ẩn hiệu ứng cường hóa vũ khí
            /// </summary>
            HideWeaponEnhanceEffect = 13,
            ///// <summary>
            ///// Thiết lập chất lượng hiệu ứng
            ///// </summary>
            //EffectQualitySetting = 14,
            /// <summary>
            /// Ẩn hiện khung chat nhanh trên đầu người chơi
            /// </summary>
            HidePlayerChat = 15,

            /// <summary>
            /// Tổng số
            /// </summary>
            Count,
        }
        #endregion

        #region Properties
        /// <summary>
        /// Thiết lập hệ thống dạng chuỗi
        /// </summary>
        private static string settingData = "";

        /// <summary>
        /// Danh sách thiết lập hệ thống
        /// </summary>
        private static Dictionary<SystemSettingIndex, object> _Settings = new Dictionary<SystemSettingIndex, object>()
        {
            { SystemSettingIndex.MusicVolume, 100 },
            { SystemSettingIndex.SkillVolume, 100 },
            { SystemSettingIndex.FieldOfView, 300 },
            { SystemSettingIndex.HideOtherName, false },
            { SystemSettingIndex.HideOtherHPBar, false },
            { SystemSettingIndex.HideRole, false },
            { SystemSettingIndex.HideOtherRole, false },
            { SystemSettingIndex.HideNPC, false },
            { SystemSettingIndex.HideSkillCastEffect, false },
            { SystemSettingIndex.HideSkillExplodeEffect, false },
            { SystemSettingIndex.HideSkillBullet, false },
            { SystemSettingIndex.HideSkillBuffEffect, false },
            { SystemSettingIndex.DisableTrailEffect, false },
            { SystemSettingIndex.HideWeaponEnhanceEffect, false },
            { SystemSettingIndex.HidePlayerChat, false },
        };
        /// <summary>
        /// Danh sách thiết lập hệ thống
        /// </summary>
        private static Dictionary<SystemSettingIndex, object> Settings
        {
            get
            {
                /// Toác
                if (Global.Data == null || Global.Data.RoleData == null)
                {
                    return default;
                }

                if (Global.Data.RoleData.SystemSettings != KTSystemSetting.settingData)
                {
                    KTSystemSetting.settingData = Global.Data.RoleData.SystemSettings;
                    KTSystemSetting.BuildSettings();
                }

                return KTSystemSetting._Settings;
            }
        }

        /// <summary>
        /// Độ lớn nhạc nền
        /// </summary>
        public static int MusicVolume
        {
            get
            {
                return (int) KTSystemSetting.Settings[SystemSettingIndex.MusicVolume];
            }
            set
            {
                KTSystemSetting.Settings[SystemSettingIndex.MusicVolume] = value;
            }
        }

        /// <summary>
        /// Độ lớn âm thanh kỹ năng
        /// </summary>
        public static int SkillVolume
        {
            get
            {
                return (int) KTSystemSetting.Settings[SystemSettingIndex.SkillVolume];
            }
            set
            {
                KTSystemSetting.Settings[SystemSettingIndex.SkillVolume] = value;
            }
        }

        /// <summary>
        /// Tầm nhìn
        /// </summary>
        public static int FieldOfView
        {
            get
            {
                return (int) KTSystemSetting.Settings[SystemSettingIndex.FieldOfView];
            }
            set
            {
                KTSystemSetting.Settings[SystemSettingIndex.FieldOfView] = value;
            }
        }

        /// <summary>
        /// Hiển thị tên đối tượng khác
        /// </summary>
        public static bool HideOtherName
        {
            get
            {
                return (bool) KTSystemSetting.Settings[SystemSettingIndex.HideOtherName];
            }
            set
            {
                KTSystemSetting.Settings[SystemSettingIndex.HideOtherName] = value;
            }
        }

        /// <summary>
        /// Hiển thị thanh máu của đối tượng khác
        /// </summary>
        public static bool HideOtherHPBar
        {
            get
            {
                return (bool) KTSystemSetting.Settings[SystemSettingIndex.HideOtherHPBar];
            }
            set
            {
                KTSystemSetting.Settings[SystemSettingIndex.HideOtherHPBar] = value;
            }
        }

        /// <summary>
        /// Ẩn nhân vật
        /// </summary>
        public static bool HideRole
        {
            get
            {
                return (bool) KTSystemSetting.Settings[SystemSettingIndex.HideRole];
            }
            set
            {
                KTSystemSetting.Settings[SystemSettingIndex.HideRole] = value;
            }
        }

        /// <summary>
        /// Ẩn người chơi khác
        /// </summary>
        public static bool HideOtherRole
        {
            get
            {
                return (bool) KTSystemSetting.Settings[SystemSettingIndex.HideOtherRole];
            }
            set
            {
                KTSystemSetting.Settings[SystemSettingIndex.HideOtherRole] = value;
            }
        }

        /// <summary>
        /// Ẩn NPC và quái
        /// </summary>
        public static bool HideNPC
        {
            get
            {
                return (bool) KTSystemSetting.Settings[SystemSettingIndex.HideNPC];
            }
            set
            {
                KTSystemSetting.Settings[SystemSettingIndex.HideNPC] = value;
            }
        }

        /// <summary>
        /// Ẩn hiệu ứng xuất chiêu
        /// </summary>
        public static bool HideSkillCastEffect
        {
            get
            {
                return (bool) KTSystemSetting.Settings[SystemSettingIndex.HideSkillCastEffect];
            }
            set
            {
                KTSystemSetting.Settings[SystemSettingIndex.HideSkillCastEffect] = value;
            }
        }

        /// <summary>
        /// Ẩn hiệu ứng phát nổ của kỹ năng
        /// </summary>
        public static bool HideSkillExplodeEffect
        {
            get
            {
                return (bool) KTSystemSetting.Settings[SystemSettingIndex.HideSkillExplodeEffect];
            }
            set
            {
                KTSystemSetting.Settings[SystemSettingIndex.HideSkillExplodeEffect] = value;
            }
        }

        /// <summary>
        /// Ẩn hiệu ứng đạn bay
        /// </summary>
        public static bool HideSkillBullet
        {
            get
            {
                return (bool) KTSystemSetting.Settings[SystemSettingIndex.HideSkillBullet];
            }
            set
            {
                KTSystemSetting.Settings[SystemSettingIndex.HideSkillBullet] = value;
            }
        }

        /// <summary>
        /// Ẩn hiệu ứng Buff
        /// </summary>
        public static bool HideSkillBuffEffect
        {
            get
            {
                return (bool) KTSystemSetting.Settings[SystemSettingIndex.HideSkillBuffEffect];
            }
            set
            {
                KTSystemSetting.Settings[SystemSettingIndex.HideSkillBuffEffect] = value;
            }
        }

        /// <summary>
        /// Ẩn hiệu ứng đổ bóng
        /// </summary>
        public static bool DisableTrailEffect
        {
            get
            {
                return (bool) KTSystemSetting.Settings[SystemSettingIndex.DisableTrailEffect];
            }
            set
            {
                KTSystemSetting.Settings[SystemSettingIndex.DisableTrailEffect] = value;
            }
        }

        /// <summary>
        /// Ẩn hiệu ứng cường hóa vũ khí
        /// </summary>
        public static bool HideWeaponEnhanceEffect
        {
            get
            {
                return (bool) KTSystemSetting.Settings[SystemSettingIndex.HideWeaponEnhanceEffect];
            }
            set
            {
                KTSystemSetting.Settings[SystemSettingIndex.HideWeaponEnhanceEffect] = value;
            }
        }

        /// <summary>
        /// Ẩn khung chat nhanh trên đầu người chơi
        /// </summary>
        public static bool HidePlayerChat
        {
            get
            {
                return (bool) KTSystemSetting.Settings[SystemSettingIndex.HidePlayerChat];
            }
            set
            {
                KTSystemSetting.Settings[SystemSettingIndex.HidePlayerChat] = value;
            }
        }
        #endregion

        #region Private methods

        #endregion

        #region Public methods
        /// <summary>
        /// Đổ dữ liệu thiết lập hệ thống vào từ điển
        /// </summary>
        public static void BuildSettings()
        {
            /// Làm rỗng danh sách
            KTSystemSetting._Settings.Clear();

            /// Nếu chuỗi rỗng
            if (KTSystemSetting.settingData == null)
            {
                KTSystemSetting.settingData = "";
                Global.Data.RoleData.SystemSettings = "";
            }

            /// Tách dữ liệu từ chuỗi
            string[] data = KTSystemSetting.settingData.Split('|');
            /// Nếu lượng thông tin không phù hợp
            if (data.Length != (int) SystemSettingIndex.Count)
            {
                KTSystemSetting.MusicVolume = 100;
                KTSystemSetting.SkillVolume = 100;
                KTSystemSetting.FieldOfView = 300;
                KTSystemSetting.HideOtherName = false;
                KTSystemSetting.HideOtherHPBar = false;
                KTSystemSetting.HideRole = false;
                KTSystemSetting.HideOtherRole = false;
                KTSystemSetting.HideNPC = false;
                KTSystemSetting.HideSkillCastEffect = false;
                KTSystemSetting.HideSkillExplodeEffect = false;
                KTSystemSetting.HideSkillBullet = false;
                KTSystemSetting.HideSkillBuffEffect = false;
                KTSystemSetting.DisableTrailEffect = false;
                KTSystemSetting.HideWeaponEnhanceEffect = false;
                KTSystemSetting.HidePlayerChat = false;
            }
            else
            {
                KTSystemSetting.MusicVolume = Utils.ParseNumber(data[(int) SystemSettingIndex.MusicVolume], 100);
                KTSystemSetting.SkillVolume = Utils.ParseNumber(data[(int) SystemSettingIndex.SkillVolume], 100);
                KTSystemSetting.FieldOfView = Utils.ParseNumber(data[(int) SystemSettingIndex.FieldOfView], 300);
                KTSystemSetting.HideOtherName = Utils.ParseNumber(data[(int) SystemSettingIndex.HideOtherName], 0) == 1;
                KTSystemSetting.HideOtherHPBar = Utils.ParseNumber(data[(int) SystemSettingIndex.HideOtherHPBar], 0) == 1;
                KTSystemSetting.HideRole = Utils.ParseNumber(data[(int) SystemSettingIndex.HideRole], 0) == 1;
                KTSystemSetting.HideOtherRole = Utils.ParseNumber(data[(int) SystemSettingIndex.HideOtherRole], 0) == 1;
                KTSystemSetting.HideNPC = Utils.ParseNumber(data[(int) SystemSettingIndex.HideNPC], 0) == 1;
                KTSystemSetting.HideSkillCastEffect = Utils.ParseNumber(data[(int) SystemSettingIndex.HideSkillCastEffect], 0) == 1;
                KTSystemSetting.HideSkillExplodeEffect = Utils.ParseNumber(data[(int) SystemSettingIndex.HideSkillExplodeEffect], 0) == 1;
                KTSystemSetting.HideSkillBullet = Utils.ParseNumber(data[(int) SystemSettingIndex.HideSkillBullet], 0) == 1;
                KTSystemSetting.HideSkillBuffEffect = Utils.ParseNumber(data[(int) SystemSettingIndex.HideSkillBuffEffect], 0) == 1;
                KTSystemSetting.DisableTrailEffect = Utils.ParseNumber(data[(int) SystemSettingIndex.DisableTrailEffect], 0) == 1;
                KTSystemSetting.HideWeaponEnhanceEffect = Utils.ParseNumber(data[(int) SystemSettingIndex.HideWeaponEnhanceEffect], 0) == 1;
                KTSystemSetting.HidePlayerChat = Utils.ParseNumber(data[(int) SystemSettingIndex.HidePlayerChat], 0) == 1;
            }
        }

        /// <summary>
        /// Lưu thiết lập
        /// </summary>
        /// <param name="settingBits"></param>
        public static void SaveSettings()
        {
            List<string> data = new List<string>();
            data.Add(KTSystemSetting.MusicVolume.ToString());
            data.Add(KTSystemSetting.SkillVolume.ToString());
            data.Add(KTSystemSetting.FieldOfView.ToString());
            data.Add(KTSystemSetting.HideOtherName ? "1" : "0");
            data.Add(KTSystemSetting.HideOtherHPBar ? "1" : "0");
            data.Add(KTSystemSetting.HideRole ? "1" : "0");
            data.Add(KTSystemSetting.HideOtherRole ? "1" : "0");
            data.Add(KTSystemSetting.HideNPC ? "1" : "0");
            data.Add(KTSystemSetting.HideSkillCastEffect ? "1" : "0");
            data.Add(KTSystemSetting.HideSkillExplodeEffect ? "1" : "0");
            data.Add(KTSystemSetting.HideSkillBullet ? "1" : "0");
            data.Add(KTSystemSetting.HideSkillBuffEffect ? "1" : "0");
            data.Add(KTSystemSetting.DisableTrailEffect ? "1" : "0");
            data.Add(KTSystemSetting.HideWeaponEnhanceEffect ? "1" : "0");
            data.Add("1");
            data.Add(KTSystemSetting.HidePlayerChat ? "1" : "0");

            /// Lưu thiết lập
            Global.Data.RoleData.SystemSettings = string.Join("|", data);
            KTSystemSetting.settingData = Global.Data.RoleData.SystemSettings;

            /// Gửi yêu cầu lưu lại vào hệ thống
            KT_TCPHandler.SendSaveSystemSettings();
        }
        #endregion
    }
}
