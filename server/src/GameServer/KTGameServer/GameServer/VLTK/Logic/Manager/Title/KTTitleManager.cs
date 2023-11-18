using GameServer.KiemThe.Entities.Player;
using GameServer.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace GameServer.KiemThe.Core.Title
{
	/// <summary>
	/// Quản lý danh hiệu của Game
	/// </summary>
	public static class KTTitleManager
	{
		/// <summary>
		/// Danh sách danh hiệu
		/// </summary>
		private static readonly Dictionary<int, KTitleXML> titles = new Dictionary<int, KTitleXML>();

		/// <summary>
		/// Danh sách danh hiệu đặc biệt
		/// </summary>
		private static readonly Dictionary<int, KSpecialTitleXML> specialTitles = new Dictionary<int, KSpecialTitleXML>();

		/// <summary>
		/// Khởi tạo dữ liệu
		/// </summary>
		public static void Init()
		{
			/// Đối tượng XML tương ứng đọc từ File
			XElement xmlNode = KTGlobal.ReadXMLData("Config/KT_Title/Title.xml");
			/// Duyệt danh sách
			foreach (XElement node in xmlNode.Elements("Title"))
			{
				KTitleXML titleData = KTitleXML.Parse(node);
				KTTitleManager.titles[titleData.ID] = titleData;
			}

			/// Đối tượng XML tương ứng đọc từ File
			XElement _xmlNode = KTGlobal.ReadXMLData("Config/KT_Title/SpecialTitle.xml");
			/// Duyệt danh sách
			foreach (XElement node in _xmlNode.Elements("Title"))
			{
				KSpecialTitleXML titleData = KSpecialTitleXML.Parse(node);
				KTTitleManager.specialTitles[titleData.ID] = titleData;
			}
		}

        #region Normal title
        /// <summary>
        /// Trả về thông tin danh hiệu tương ứng
        /// </summary>
        /// <param name="titleID"></param>
        /// <returns></returns>
        public static KTitleXML GetTitleData(int titleID)
		{
			if (KTTitleManager.titles.TryGetValue(titleID, out KTitleXML titleData))
			{
				return titleData;
			}
			return null;
		}

		/// <summary>
		/// Kiểm tra danh hiệu tương ứng có tồn tại không
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public static bool IsTitleExist(int id)
		{
			return KTTitleManager.titles.ContainsKey(id);
		}
		#endregion

		#region Special title
		/// <summary>
		/// Trả về thông tin danh hiệu đặc biệt tương ứng
		/// </summary>
		/// <param name="titleID"></param>
		/// <returns></returns>
		public static KSpecialTitleXML GetSpecialTitleData(int titleID)
		{
			if (KTTitleManager.specialTitles.TryGetValue(titleID, out KSpecialTitleXML titleData))
			{
				return titleData;
			}
			return null;
		}

		/// <summary>
		/// Kiểm tra danh hiệu đặc biệt tương ứng có tồn tại không
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public static bool IsSpecialTitleExist(int id)
		{
			return KTTitleManager.specialTitles.ContainsKey(id);
		}
		#endregion
	}
}
