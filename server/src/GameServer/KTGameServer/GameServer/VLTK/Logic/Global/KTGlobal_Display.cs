using GameServer.KiemThe.Core.Item;
using GameServer.Logic;
using GameServer.VLTK.Entities.Pet;
using Server.Data;
using System.Collections.Generic;

namespace GameServer.KiemThe
{
    /// <summary>
    /// Các phương thức và đối tượng toàn cục của Kiếm Thế
    /// </summary>
    public static partial class KTGlobal
    {
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
            int? intPart = (int?) number;
            int? decPart = (int?) (((decimal?) number % 1) * 100);
            string intStr = KTGlobal.GetDisplayNumber(intPart);
            string decStr = decPart.ToString();

            return intStr + "," + decStr;
        }
        #endregion

        #region Màu trang bị
        /// <summary>
        /// Trả về mã màu trang bị tương ứng
        /// </summary>
        /// <param name="itemGD"></param>
        /// <returns></returns>
        public static string GetItemNameColor(GoodsData itemGD)
        {
            /// Nếu không có dữ liệu hoặc không phải trang bị thì màu trắng
            if (itemGD == null || !ItemManager.IsEquip(itemGD))
            {
                return "#ffffff";
            }

            StarLevelStruct starLevelStruct = ItemManager.ItemValueCalculation(itemGD, out _,out int LinesCount);
            if (starLevelStruct == null)
            {
                return "#ffffff";
            }
            else
            {
                switch (starLevelStruct.NameColor)
                {
                    case "green":
                    {
                        return "#78ff1f";
                    }
                    case "blue":
                    {
                        return "#14a9ff";
                    }
                    case "purple":
                    {
                        return "#d82efa";
                    }
                    case "orange":
                    {
                        return "#ff8e3d";
                    }
                    case "yellow":
                    {
                        return "#fffb29";
                    }
                    default:
                    {
                        return "#ffffff";
                    }
                }
            }
        }

        /// <summary>
        /// Trả về chuỗi mô tả thông tin vật phẩm ở kênh Chat
        /// </summary>
        /// <param name="itemGD"></param>
        /// <returns></returns>
        public static string GetItemDescInfoStringForChat(GoodsData itemGD)
        {
            if (itemGD == null)
            {
                return "";
            }

            /// Dữ liệu vật phẩm
            string itemName = KTGlobal.GetItemName(itemGD);
            string itemDescString = string.Format("<color={0}><link=\"ITEM_{1}\">[{2}]</link></color>", KTGlobal.GetItemNameColor(itemGD), itemGD.Id, itemName);

            /// Trả về kết quả
            return itemDescString;
        }

        /// <summary>
        /// Trả về chuỗi mô tả thông tin pet ở kênh chat
        /// </summary>
        /// <param name="petData"></param>
        /// <returns></returns>
        public static string GetPetDescInfoStringForChat(PetData petData)
        {
            /// Toác
            if (petData == null)
            {
                return "";
            }

            /// Thông tin res
            PetDataXML petDataXML = KPet.GetPetData(petData.ResID);
            /// Toác
            if (petDataXML == null)
            {
                return "";
            }

            /// Tên pet
            string petName = petDataXML.Name;
            string petDescString = string.Format("<color=#a733ff><link=\"PET_{0}\">[{1}]</link></color>", petData.ID, petName);

            /// Trả về kết quả
            return petDescString;
        }

        /// <summary>
        /// Trả về tên vật phẩm tương ứng
        /// </summary>
        /// <param name="itemGD"></param>
        /// <returns></returns>
        public static string GetItemName(GoodsData itemGD)
        {
            /// Nếu vật phẩm không tồn tại
            if (!ItemManager._TotalGameItem.TryGetValue(itemGD.GoodsID, out ItemData itemData))
            {
                return "";
            }

            /// Tên vật phẩm
            string name = itemData.Name;

            ///// Tên đi kèm đằng sau loại trang bị
            //string suffixName = "";

            ///// Nếu tồn tại danh sách thuộc tính ẩn
            //if (itemData.HiddenProp != null)
            //{
            //    if (itemData.HiddenProp.Count > 0)
            //    {
            //        /// Tìm danh sách thuộc tính tương ứng đảo ngược
            //        List<PropMagic> props = itemData.HiddenProp.OrderByDescending(x => x.Index).ToList();

            //        /// Duyệt danh sách thuộc tính
            //        foreach (PropMagic prop in props)
            //        {
            //            if (prop.MagicName.Length > 0)
            //            {
            //                /// Thuộc tính tương ứng
            //                MagicAttribLevel magicAttrib = ItemManager.TotalMagicAttribLevel.Where(x => x.MagicName == prop.MagicName && x.Level == prop.MagicLevel).FirstOrDefault();

            //                /// Nếu tìm thấy
            //                if (magicAttrib != null)
            //                {
            //                    /// Trả về tên đi kèm đằng sau loại trang bị
            //                    suffixName = magicAttrib.Suffix + "";
            //                    break;
            //                }
            //            }
            //        }
            //    }
            //}

            ///// Nếu tên đi kèm đằng sau chưa tồn tại
            //if (string.IsNullOrEmpty(suffixName))
            //{
            //    /// Nếu tồn tại danh sách thuộc tính ngẫu nhiên
            //    if (itemData.GreenProp != null)
            //    {
            //        if (itemData.GreenProp.Count > 0)
            //        {
            //            /// Tìm danh sách thuộc tính tương ứng đảo ngược
            //            List<PropMagic> props = itemData.GreenProp.OrderByDescending(x => x.Index).ToList();

            //            /// Duyệt danh sách thuộc tính
            //            foreach (PropMagic prop in props)
            //            {
            //                if (prop.MagicName.Length > 0)
            //                {
            //                    /// Thuộc tính tương ứng
            //                    MagicAttribLevel magicAttrib = ItemManager.TotalMagicAttribLevel.Where(x => x.MagicName == prop.MagicName && x.Level == prop.MagicLevel).FirstOrDefault();

            //                    /// Nếu tìm thấy
            //                    if (magicAttrib != null)
            //                    {
            //                        /// Trả về tên đi kèm đằng sau loại trang bị
            //                        suffixName = magicAttrib.Suffix + "";
            //                        break;
            //                    }
            //                }
            //            }
            //        }
            //    }
            //}

            ///// Nếu tồn tại tên đi kèm loại trang bị
            //if (!string.IsNullOrEmpty(suffixName))
            //{
            //    name += string.Format(" - {0}", suffixName);
            //}

            /// Nếu là trang bị có thể cường hóa
            if (ItemManager.KD_ISEQUIP(itemGD.GoodsID) && ItemManager.CanEquipBeEnhance(itemData))
            {
                /// Cấp cường hóa nếu có
                if (itemGD.Forge_level > 0)
                {
                    name += string.Format(" +{0}", itemGD.Forge_level);
                }
                else
                {
                    name += " - Chưa cường hóa";
                }
            }

            /// Trả về kết quả
            return name;
        }

        /// <summary>
        /// Trả về tên vật phẩm kèm mã màu HTML dạng RichText
        /// </summary>
        /// <param name="itemGD"></param>
        /// <returns></returns>
        public static string GetItemNameWithHTMLColor(GoodsData itemGD)
        {
            string htmlColor = KTGlobal.GetItemNameColor(itemGD);
            string itemName = KTGlobal.GetItemName(itemGD);

            return string.Format("<color={0}>[{1}]</color>", htmlColor, itemName);
        }

        /// <summary>
        /// Trả về tên vật phẩm có ID tương ứng
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static string GetItemName(int id)
        {
            ItemData itemData = ItemManager.GetItemTemplate(id);
            if (itemData == null)
            {
                return "";
            }
            return itemData.Name;
        }
        #endregion

        #region Sex
        /// <summary>
        /// Chuyển giới tính sang chuỗi tương ứng
        /// </summary>
        /// <param name="sex"></param>
        /// <returns></returns>
        public static string SexToString(int sex)
        {
            if (sex == 0)
            {
                return "Nam";
            }
            else
            {
                return "Nữ";
            }
        }
        #endregion

        #region Ngũ hành
        /// <summary>
        /// Chuyển ngũ hành sang tên tương ứng
        /// </summary>
        /// <param name="seriesID"></param>
        /// <returns></returns>
        public static string GetSeriesText(int seriesID)
        {
            switch (seriesID)
            {
                case 1:
                    return string.Format("<color={0}>{1}</color>", "#ffe552", "Kim");

                case 2:
                    return string.Format("<color={0}>{1}</color>", "#77ff33", "Mộc");

                case 3:
                    return string.Format("<color={0}>{1}</color>", "#61d7ff", "Thủy");

                case 4:
                    return string.Format("<color={0}>{1}</color>", "#ff4242", "Hỏa");

                case 5:
                    return string.Format("<color={0}>{1}</color>", "#debba0", "Thổ");

                default:
                    return string.Format("<color={0}>{1}</color>", "#cccccc", "Vô");
            }
        }
        #endregion

        #region String Color
        /// <summary>
        /// Trả về mã màu Hex tương ứng
        /// </summary>
        /// <param name="Input"></param>
        /// <param name="Color"></param>
        /// <returns></returns>
        public static string CreateStringByColor(string Input, ColorType Color)
        {
            string Ref = "";

            if (Color == ColorType.Done)
            {
                Ref = "<color=#00EC1C>" + Input + "</color>";
            }
            else if (Color == ColorType.Accpect)
            {
                Ref = "<color=#ECD600>" + Input + "</color>";
            }
            else if (Color == ColorType.Importal)
            {
                Ref = "<color=#ec0000>" + Input + "</color>";
            }
            else if (Color == ColorType.Green)
            {
                Ref = "<color=#00ff2a>" + Input + "</color>";
            }
            else if (Color == ColorType.Pure)
            {
                Ref = "<color=#e100ff>" + Input + "</color>";
            }
            else if (Color == ColorType.Blue)
            {
                Ref = "<color=#001eff>" + Input + "</color>";
            }
            else if (Color == ColorType.Yellow)
            {
                Ref = "<color=yellow>" + Input + "</color>";
            }
            else if (Color == ColorType.Normal)
            {
                Ref = Input;
            }

            return Ref;
        }

        #endregion String Color

        #region Date Time

        /// <summary>
        /// Hiển thị thời gian
        /// <para>Thời gian dạng giây</para>
        /// </summary>
        public static string DisplayTime(float timeInSec)
        {
            int sec = (int) timeInSec;
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
            int sec = (int) timeInSec;
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
            int sec = (int) timeInSec;
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
            int sec = (int) timeInSec;
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

        #endregion Date Time

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
        #endregion

        #region Hiển thị tên
        /// <summary>
        /// Format tên hiển thị theo Server (dùng ở Liên Server)
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        public static string FormatRoleNameWithZoneId(KPlayer client)
        {
            return string.Format("<color=#e425e4>[S{0}]</color> {1}", client.ZoneID, client.RoleName);
        }
        #endregion
    }
}
