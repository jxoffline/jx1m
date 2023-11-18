using FS.GameEngine.Logic;
using HSGameEngine.GameEngine.Network.Tools;
using System;
using System.Collections.Generic;
using UnityEngine;
using static FS.VLTK.Entities.Enum;

namespace FS.VLTK
{
    /// <summary>
    /// Các hàm toàn cục dùng trong Kiếm Thế
    /// </summary>
    public static partial class KTGlobal
    {
        #region Bit converter

        /// <summary>
        /// 1 KB = ? Byte
        /// </summary>
        private const long OneKb = 1024;

        /// <summary>
        /// 1 MB = ? Byte
        /// </summary>
        private const long OneMb = KTGlobal.OneKb * 1024;

        /// <summary>
        /// 1 GB = ? Byte
        /// </summary>
        private const long OneGb = KTGlobal.OneMb * 1024;

        /// <summary>
        /// 1 TB = ? Byte
        /// </summary>
        private const long OneTb = KTGlobal.OneGb * 1024;

        /// <summary>
        /// Chuyển Byte thành đơn vị phù hợp
        /// </summary>
        /// <param name="value"></param>
        /// <param name="decimalPlaces"></param>
        /// <returns></returns>
        private static string ToPrettySize(this long value, int decimalPlaces = 0)
        {
            var asTb = Math.Round((double)value / KTGlobal.OneTb, decimalPlaces);
            var asGb = Math.Round((double)value / KTGlobal.OneGb, decimalPlaces);
            var asMb = Math.Round((double)value / KTGlobal.OneMb, decimalPlaces);
            var asKb = Math.Round((double)value / KTGlobal.OneKb, decimalPlaces);
            string chosenValue = asTb > 1 ? string.Format("{0}TB", asTb)
                : asGb > 1 ? string.Format("{0}GB", asGb)
                : asMb > 1 ? string.Format("{0}MB", asMb)
                : asKb > 1 ? string.Format("{0}KB", asKb)
                : string.Format("{0}B", Math.Round((double)value, decimalPlaces));
            return chosenValue;
        }

        /// <summary>
        /// Chuyển đơn vị Bytes thành dạng String
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static string BytesToString(long bytes)
        {
            return KTGlobal.ToPrettySize(bytes, 2);
        }

        #endregion Bit converter

        #region Other

        /// <summary>
        /// Chuyển giới tính thành dạng String
        /// </summary>
        /// <param name="sex"></param>
        /// <returns></returns>
        public static string SexToString(int sex)
        {
            switch (sex)
            {
                case (int)Sex.MALE:
                    {
                        return "Nam";
                    }
                case (int)Sex.FEMALE:
                    {
                        return "Nữ";
                    }
                default:
                    {
                        return "Không rõ";
                    }
            }
        }

        #endregion Other

        #region Tiền tệ

        /// <summary>
        /// Bundle chứa Icon tiền tệ
        /// </summary>
        private const string MoneyIconBundleDir = "Game/MainUI/Main.unity3d";

        /// <summary>
        /// Atlas chứa Icon tiền tệ
        /// </summary>
        private const string MoneyIconAtlasName = "Main";

        /// <summary>
        /// Danh sách Icon tiền tệ
        /// </summary>
        private static readonly Dictionary<MoneyType, string> MoneySpriteName = new Dictionary<MoneyType, string>()
        {
            { MoneyType.Bac, "Icon_Bac" },
            { MoneyType.BacKhoa, "Icon_Bac_Khoa" },
            { MoneyType.Dong, "Icon_Vang" },
            { MoneyType.DongKhoa, "Icon_Vang_Khoa" },
            { MoneyType.GuildMoney, "Icon_Bac" },
            { MoneyType.StoreMoney, "Icon_Bac" },
        };

        /// <summary>
        /// Trả về thông tin hiển thị của Icon tiền
        /// </summary>
        /// <param name="type"></param>
        /// <param name="bundleDir"></param>
        /// <param name="atlasName"></param>
        /// <param name="spriteName"></param>
        public static void GetMoneyDisplayImage(MoneyType type, out string bundleDir, out string atlasName, out string spriteName)
        {
            /// Nếu tồn tại
            if (KTGlobal.MoneySpriteName.TryGetValue(type, out string iconSpriteName))
            {
                bundleDir = KTGlobal.MoneyIconBundleDir;
                atlasName = KTGlobal.MoneyIconAtlasName;
                spriteName = iconSpriteName;
            }
            else
            {
                bundleDir = "";
                atlasName = "";
                spriteName = "";
            }
        }

        /// <summary>
        /// Trả về tên tiền tệ tương ứng
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string GetMoneyName(MoneyType type)
        {
            switch (type)
            {
                case MoneyType.Bac:
                    {
                        return "Bạc";
                    }
                case MoneyType.BacKhoa:
                    {
                        return "Bạc khóa";
                    }
                case MoneyType.Dong:
                    {
                        return "KNB";
                    }
                case MoneyType.DongKhoa:
                    {
                        return "Đòng khóa";
                    }
                case MoneyType.GuildMoney:
                    {
                        return "Bang cống";
                    }

                default:
                    {
                        return "Unknow";
                    }
            }
        }

        #endregion Tiền tệ

        #region Date Time

        /// <summary>
        /// Hiển thị thời gian
        /// <para>Thời gian dạng giây</para>
        /// </summary>
        public static string DisplayTime(float timeInSec)
        {
            int sec = (int)timeInSec;
            if (sec >= 86400)
            {
                int nDay = sec / 86400;
                return string.Format("{0} ngày", nDay);
            }
            else if (sec >= 3600)
            {
                int nHour = sec / 3600;
                return string.Format("{0} giờ", nHour);
            }
            else
            {
                int nMinute = sec / 60;
                int nSecond = sec - nMinute * 60;
                string secondString = nSecond.ToString();
                while (secondString.Length < 2)
                {
                    secondString = "0" + secondString;
                }
                return string.Format("{0}:{1}", nMinute, secondString);
            }
        }

        /// <summary>
        /// Hiển thị đầy đủ ngày và giờ
        /// </summary>
        /// <param name="timeInSec">Thời gian dạng giây</param>
        /// <param name="fullNameDisplay">Hiển thị đầy đủ text NGÀY, GIỜ, PHÚT, GIÂY</param>
        /// <returns></returns>
        public static string DisplayFullDateAndTime(float timeInSec, bool fullNameDisplay = true)
        {
            int sec = (int)timeInSec;
            int nDay = sec / 86400;
            sec -= nDay * 86400;
            int nHour = sec / 3600;
            sec -= nHour * 3600;
            int nMinute = sec / 60;
            sec -= nMinute * 60;
            int nSecond = sec;

            List<string> result = new List<string>();
            if (nDay > 0)
            {
                if (fullNameDisplay)
                {
                    result.Add(string.Format("{0} ngày", nDay));
                }
                else
                {
                    result.Add(string.Format("{0} ngày", nDay));
                }
            }
            if (nHour > 0)
            {
                if (fullNameDisplay)
                {
                    result.Add(string.Format("{0} giờ", nHour));
                }
                else
                {
                    result.Add(string.Format("{0}", nHour));
                }
            }
            if (nMinute > 0)
            {
                if (fullNameDisplay)
                {
                    result.Add(string.Format("{0} phút", nMinute));
                }
                else
                {
                    result.Add(string.Format("{0}", nMinute));
                }
            }
            if (nSecond > 0)
            {
                if (fullNameDisplay)
                {
                    result.Add(string.Format("{0} giây", nSecond));
                }
                else
                {
                    result.Add(string.Format("{0}", nSecond));
                }
            }

            if (fullNameDisplay)
            {
                if (result.Count <= 0)
                {
                    return "0 giây";
                }
                return string.Join(" ", result);
            }
            else
            {
                if (result.Count <= 0)
                {
                    return "00:00";
                }
                return string.Join(":", result);
            }
        }

        /// <summary>
        /// Hiển thị ngày
        /// </summary>
        /// <param name="timeInSec">Thời gian dạng giây</param>
        /// <param name="fullNameDisplay">Hiển thị đầy đủ text NGÀY, GIỜ, PHÚT, GIÂY</param>
        /// <returns></returns>
        public static string DisplayDateOnly(float timeInSec)
        {
            int sec = (int)timeInSec;
            int nDay = sec / 86400;

            return string.Format("{0} ngày", nDay);
        }

        /// <summary>
        /// Hiển thị thời gian dưới dạng giờ phút giây
        /// </summary>
        /// <param name="timeInSec">Thời gian dạng giây</param>
        /// <param name="fullNameDisplay">Hiển thị đầy đủ text GIỜ, PHÚT, GIÂY</param>
        /// <returns></returns>
        public static string DisplayTimeHourMinuteSecondOnly(float timeInSec, bool fullNameDisplay = true)
        {
            int sec = (int)timeInSec;
            int nHour = sec / 3600;
            sec -= nHour * 3600;
            int nMinute = sec / 60;
            sec -= nMinute * 60;
            int nSecond = sec;

            List<string> result = new List<string>();
            if (nHour > 0)
            {
                if (fullNameDisplay)
                {
                    result.Add(string.Format("{0} giờ", nHour));
                }
                else
                {
                    string nHourString = nHour.ToString();
                    while (nHourString.Length < 2)
                    {
                        nHourString = "0" + nHourString;
                    }
                    result.Add(string.Format("{0}", nHourString));
                }
            }
            if (nMinute > 0)
            {
                if (fullNameDisplay)
                {
                    result.Add(string.Format("{0} phút", nMinute));
                }
                else
                {
                    string nMinuteString = nMinute.ToString();
                    while (nMinuteString.Length < 2)
                    {
                        nMinuteString = "0" + nMinuteString;
                    }
                    result.Add(string.Format("{0}", nMinuteString));
                }
            }
            if (nSecond > 0)
            {
                if (fullNameDisplay)
                {
                    result.Add(string.Format("{0} giây", nSecond));
                }
                else
                {
                    string nSecondString = nSecond.ToString();
                    while (nSecondString.Length < 2)
                    {
                        nSecondString = "0" + nSecondString;
                    }
                    result.Add(string.Format("{0}", nSecondString));
                }
            }

            if (fullNameDisplay)
            {
                if (result.Count <= 0)
                {
                    return "0 giây";
                }
                return string.Join(" ", result);
            }
            else
            {
                if (result.Count <= 0)
                {
                    return "00:00";
                }
                return string.Join(":", result);
            }
        }

        /// <summary>
        /// Chuyển DateTime về dạng Tick
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public static long ConvertDateTimeInt(System.DateTime time)
        {
            System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1, 0, 0, 0, 0));
            long t = (time.Ticks - startTime.Ticks) / 10000000;
            return t;
        }

        /// <summary>
        /// Trả về TimeStamp của hệ thống
        /// </summary>
        /// <returns></returns>
        public static long GetTimeStamp()
        {
            return TimeManager.GetTimeStamp();
        }

        #endregion Date Time

        #region Các hàm chuyển đổi tọa độ

        /// <summary>
        /// Trả về tọa độ tương ứng của đối tượng trên bản đồ Minimap dựa vào tọa độ trong map thực tế
        /// </summary>
        /// <param name="worldPos"></param>
        /// <returns></returns>
        public static Vector2 WorldPositionToWorldNavigationMapPosition(Vector2 worldPos)
        {
            /// Toác
            if (Global.CurrentMap.LocalMapSprite == null)
            {
                /// Bỏ qua
                return Vector2.zero;
            }

            Vector2 minimapSize = Global.CurrentMap.LocalMapSprite.rect.size;
            Vector2 worldMapSize = Global.CurrentMapData.RealMapSize;

            return worldPos * minimapSize / worldMapSize;
        }

        /// <summary>
        /// Trả về tọa độ tương ứng của đối tượng ở bản đồ thực tế khi biết tọa độ trên bản đồ thu nhỏ
        /// </summary>
        /// <param name="worldNavPos"></param>
        /// <returns></returns>
        public static Vector2 WorldNavigationMapPositionToWorldPosition(Vector2 worldNavPos)
        {
            /// Toác
            if (Global.CurrentMap.LocalMapSprite == null)
            {
                /// Bỏ qua
                return Vector2.zero;
            }

            Vector2 minimapSize = Global.CurrentMap.LocalMapSprite.rect.size;
            Vector2 worldMapSize = Global.CurrentMapData.RealMapSize;

            return worldNavPos * worldMapSize / minimapSize;
        }

        /// <summary>
        /// Chuyển từ tọa độ trên bản đồ về tọa độ lưới
        /// </summary>
        /// <param name="worldPos"></param>
        /// <returns></returns>
        public static Vector2 WorldPositionToGridPosition(Vector2 worldPos)
        {
            float x = worldPos.x / Global.CurrentMapData.GridSizeX;
            float y = worldPos.y / Global.CurrentMapData.GridSizeY;

            return new Vector2(x, y);
        }

        /// <summary>
        /// Chuyển từ tọa độ lưới ra tọa độ trên bản đồ
        /// </summary>
        /// <param name="gamePos"></param>
        /// <returns></returns>
        public static Vector2 GridPositionToWorldPosition(Vector2 gamePos)
        {
            float x = gamePos.x * Global.CurrentMapData.GridSizeX;
            float y = gamePos.y * Global.CurrentMapData.GridSizeY;

            return new Vector2(x, y);
        }

        #endregion Các hàm chuyển đổi tọa độ

        #region Chuyển hướng quay

        /// <summary>
        /// Chuyển hướng quay 16 hướng sang 8 hướng
        /// </summary>
        /// <param name="dir16"></param>
        /// <returns></returns>
        public static Direction Dir16ToDir8(Direction16 dir16)
        {
            if (dir16 == Direction16.Down)
            {
                return Direction.DOWN;
            }
            else if (dir16 == Direction16.Down_Down_Left || dir16 == Direction16.Down_Left || dir16 == Direction16.Down_Up_Left)
            {
                return Direction.DOWN_LEFT;
            }
            else if (dir16 == Direction16.Left)
            {
                return Direction.LEFT;
            }
            else if (dir16 == Direction16.Up_Down_Left || dir16 == Direction16.Up_Left || dir16 == Direction16.Up_Up_Left)
            {
                return Direction.UP_LEFT;
            }
            else if (dir16 == Direction16.Up)
            {
                return Direction.UP;
            }
            else if (dir16 == Direction16.Up_Up_Right || dir16 == Direction16.Up_Right || dir16 == Direction16.Up_Down_Right)
            {
                return Direction.UP_RIGHT;
            }
            else if (dir16 == Direction16.Right)
            {
                return Direction.RIGHT;
            }
            else
            {
                return Direction.DOWN_RIGHT;
            }
        }

        /// <summary>
        /// Chuyển hướng quay 8 hướng sang 16 hướng
        /// </summary>
        /// <param name="dir"></param>
        /// <returns></returns>
        public static Direction16 Dir8ToDir16(Direction dir)
        {
            if (dir == Direction.DOWN)
            {
                return Direction16.Down;
            }
            else if (dir == Direction.DOWN_LEFT)
            {
                return Direction16.Down_Left;
            }
            else if (dir == Direction.LEFT)
            {
                return Direction16.Left;
            }
            else if (dir == Direction.UP_LEFT)
            {
                return Direction16.Up_Left;
            }
            else if (dir == Direction.UP)
            {
                return Direction16.Up;
            }
            else if (dir == Direction.UP_RIGHT)
            {
                return Direction16.Up_Right;
            }
            else if (dir == Direction.RIGHT)
            {
                return Direction16.Right;
            }
            else
            {
                return Direction16.Down_Right;
            }
        }

        #endregion Chuyển hướng quay

        #region Tiền tệ

        /// <summary>
        /// Chuyển đổi đơn vị tiền thành chữ
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public static string GetDisplayMoney(long number)
        {
            string result = "";
            string str = number.ToString();

            int value = 0;
            int thousand_10 = 0;
            int billion = 0;

            if (str.Length <= 4)
            {
                value = int.Parse(str);
            }
            else
            {
                value = int.Parse(str.Substring(str.Length - 4));
                if (str.Length <= 8)
                {
                    thousand_10 = int.Parse(str.Substring(0, str.Length - 4));
                }
                else
                {
                    billion = int.Parse(str.Substring(0, str.Length - 8));
                    thousand_10 = int.Parse(str.Substring(str.Length - 8, 4));
                }
            }

            if (billion > 0)
            {
                result += string.Format("{0} Ức ", billion);
            }
            if (thousand_10 > 0)
            {
                result += string.Format("{0} Vạn ", thousand_10);
            }
            if (value > 0)
            {
                result += string.Format("{0}", value);
            }

            if (string.IsNullOrEmpty(result))
            {
                result = "0";
            }

            return result.Trim();
        }

        #endregion Tiền tệ

        #region Hiển thị số

        /// <summary>
        /// Chuyển số thành chuỗi với các dấu chấm ngăn cách
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public static string GetDisplayNumber(long? number)
        {
            string input = number.ToString();
            string nPoint = "";
            int j = 0;
            for (int i = input.Length - 1; i >= 0; i--)
            {
                j++;
                nPoint = input[i] + nPoint;
                if (j == 3 && i > 0)
                {
                    nPoint = '.' + nPoint;
                    j = 0;
                }
            }
            return nPoint;
        }

        /// <summary>
        /// Chuyển số thành chuỗi với các dấu chấm ngăn cách
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public static string GetDisplayNumber(float? number)
        {
            int? intPart = (int?)number;
            int? decPart = (int?)(((decimal?)number % 1) * 100);
            string intStr = KTGlobal.GetDisplayNumber(intPart);
            string decStr = decPart.ToString();

            return intStr + "," + decStr;
        }

        #endregion Hiển thị số

        #region Chuyển đổi thuộc tính

        /// <summary>
        /// Chuyển thuộc tính số thành dạng String
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public static string AttributeToString(this int number)
        {
            if (number >= 0)
            {
                return string.Format("+{0}", Math.Abs(number));
            }
            else
            {
                return string.Format("-{0}", Math.Abs(number));
            }
        }

        /// <summary>
        /// Chuyển thuộc tính số thành dạng String
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public static string AttributeToString(this short number)
        {
            if (number >= 0)
            {
                return string.Format("+{0}", Math.Abs(number));
            }
            else
            {
                return string.Format("-{0}", Math.Abs(number));
            }
        }

        /// <summary>
        /// Chuyển thuộc tính số thành dạng String
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public static string AttributeToString(this byte number)
        {
            if (number >= 0)
            {
                return string.Format("+{0}", Math.Abs(number));
            }
            else
            {
                return string.Format("-{0}", Math.Abs(number));
            }
        }

        /// <summary>
        /// Chuyển thuộc tính số thành dạng String
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public static string AttributeToString(this long number)
        {
            if (number >= 0)
            {
                return string.Format("+{0}", Math.Abs(number));
            }
            else
            {
                return string.Format("-{0}", Math.Abs(number));
            }
        }

        /// <summary>
        /// Chuyển thuộc tính số thành dạng String
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public static string AttributeToString(this float number)
        {
            if (number >= 0)
            {
                return string.Format("+{0}", Math.Abs(number));
            }
            else
            {
                return string.Format("-{0}", Math.Abs(number));
            }
        }

        /// <summary>
        /// Chuyển thuộc tính số thành dạng String
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public static string AttributeToString(this double number)
        {
            if (number >= 0)
            {
                return string.Format("+{0}", Math.Abs(number));
            }
            else
            {
                return string.Format("-{0}", Math.Abs(number));
            }
        }

        #endregion Chuyển đổi thuộc tính

        #region Chức vụ trong bang

        /// <summary>
        /// Trả về tên chức vụ trong bang
        /// </summary>
        /// <param name="guildRank"></param>
        /// <returns></returns>
        public static string GetGuildRankName(int guildRank)
        {
            if (guildRank <= (int)GuildRank.Member)
            {
                return "Thành viên";
            }
            else if (guildRank == (int)GuildRank.Master)
            {
                return "Bang chủ";
            }
            else if (guildRank == (int)GuildRank.ViceMaster)
            {
                return "Phó bang chủ";
            }
            else if (guildRank == (int)GuildRank.Ambassador)
            {
                return "Trưởng lão";
            }
            else if (guildRank == (int)GuildRank.ViceAmbassador)
            {
                return "Đường chủ";
            }
            else if (guildRank == (int)GuildRank.Elite)
            {
                return "Tinh anh";
            }
            else
            {
                return "Không rõ";
            }
        }

        /// <summary>
        /// Trả về danh sách có thể bổ nhiệm theo chức vị hiện tại
        /// </summary>
        /// <param name="guildRank"></param>
        /// <returns></returns>
        public static List<GuildRank> GetGuildApprovableRanks(GuildRank guildRank)
        {
            /// Danh sách có thể bổ nhiệm
            List<GuildRank> ranks = new List<GuildRank>();

            /// Nếu là bang chủ
            if (guildRank == GuildRank.Master)
            {
                ranks.Add(GuildRank.Master);
                ranks.Add(GuildRank.ViceMaster);
                ranks.Add(GuildRank.Ambassador);
                ranks.Add(GuildRank.ViceAmbassador);
                ranks.Add(GuildRank.Elite);
                ranks.Add(GuildRank.Member);
            }
            /// Nếu là phó bang chủ
            else if (guildRank == GuildRank.ViceMaster)
            {
                ranks.Add(GuildRank.Ambassador);
                ranks.Add(GuildRank.ViceAmbassador);
                ranks.Add(GuildRank.Elite);
                ranks.Add(GuildRank.Member);
            }
            /// Nếu là trưởng lão
            else if (guildRank == GuildRank.Ambassador)
            {
                ranks.Add(GuildRank.ViceAmbassador);
                ranks.Add(GuildRank.Elite);
                ranks.Add(GuildRank.Member);
            }
            /// Nếu là đường chủ
            else if (guildRank == GuildRank.Ambassador)
            {
                ranks.Add(GuildRank.Elite);
                ranks.Add(GuildRank.Member);
            }

            /// Trả về kết quả
            return ranks;
        }

        /// <summary>
        /// Kiểm tra có thể bổ nhiệm chức vị này không
        /// </summary>
        /// <param name="guildRank"></param>
        /// <param name="targetRank"></param>
        /// <returns></returns>
        public static bool CanApproveGuildRank(GuildRank guildRank, GuildRank targetRank)
        {
            return KTGlobal.GetGuildApprovableRanks(guildRank).Contains(targetRank);
        }

        #endregion Chức vụ trong bang

        #region Xoay hướng

        /// <summary>
        /// Trả về hướng xoay tự lật tương ứng áp dụng với res quái
        /// </summary>
        /// <param name="inputDir"></param>
        /// <returns></returns>
        public static Direction GetAutoFlipDirection(Direction inputDir)
        {
            switch (inputDir)
            {
                case Direction.DOWN:
                case Direction.DOWN_LEFT:
                case Direction.LEFT:
                case Direction.UP_LEFT:
                case Direction.UP:
                    {
                        return inputDir;
                    }
                case Direction.UP_RIGHT:
                    {
                        return Direction.UP_LEFT;
                    }
                case Direction.RIGHT:
                    {
                        return Direction.LEFT;
                    }
                case Direction.DOWN_RIGHT:
                    {
                        return Direction.DOWN_LEFT;
                    }
                default:
                    {
                        return Direction.NONE;
                    }
            }
        }

        #endregion Xoay hướng
    }
}