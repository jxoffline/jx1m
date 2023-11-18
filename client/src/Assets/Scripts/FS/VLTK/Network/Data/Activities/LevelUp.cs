using FS.GameEngine.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using ProtoBuf;

namespace Server.Data
{
	[XmlRoot(ElementName = "LevelUpItem")]
	[ProtoContract]
	public class LevelUpItem
	{
		[XmlAttribute(AttributeName = "ID")]
		[ProtoMember(1)]
		public int ID { get; set; }

		[XmlAttribute(AttributeName = "ToLevel")]
		[ProtoMember(2)]
		public int ToLevel { get; set; }

		[XmlAttribute(AttributeName = "LevelUpGift")]
		[ProtoMember(3)]
		public string LevelUpGift { get; set; }

		private List<KeyValuePair<int, int>> _Items = null;
		/// <summary>
		/// Danh sách vật phẩm
		/// </summary>
		public List<KeyValuePair<int, int>> Items
		{
			get
			{
				if (this._Items != null)
				{
					return this._Items;
				}

				List<KeyValuePair<int, int>> items = new List<KeyValuePair<int, int>>();
				string[] keyPairs = this.LevelUpGift.Split('|');
				foreach (string keyPair in keyPairs)
				{
					try
					{
						string[] para = keyPair.Split(',');
						int itemID = int.Parse(para[0]);
						int number = int.Parse(para[1]);
						items.Add(new KeyValuePair<int, int>(itemID, number));
					}
					catch (Exception) { }
				}
				return items;
			}
		}
	}

	[XmlRoot(ElementName = "LevelUpGiftConfig")]
	[ProtoContract]
	public class LevelUpGiftConfig
	{

		[XmlElement(ElementName = "LevelUpItem")]
		[ProtoMember(1)]
		public List<LevelUpItem> LevelUpItem { get; set; }

		/// <summary>
		/// Danh sách trạng thái
		/// </summary>
		[ProtoMember(2)]
		public string BitFlags { get; set; }

		/// <summary>
		/// Có gì để nhận không
		/// </summary>
		/// <param name="myLevel"></param>
		public bool HasSomethingToGet
		{
            get
			{
				/// Nếu không có trạng thái
				if (this.BitFlags == null)
				{
					return false;
				}

				/// Giá trị từng mốc
				Dictionary<int, int> states = new Dictionary<int, int>();
				foreach (string stateString in this.BitFlags.Split('|'))
				{
					string[] fields = stateString.Split('_');
					int stepID = int.Parse(fields[0]);
					int value = int.Parse(fields[1]);
					states[stepID] = value;
				}

				/// Tổng số mốc
				int count = this.LevelUpItem.Count;
				/// Duyệt từng mốc
				for (int i = 1; i <= count; i++)
				{
					/// Phần quà tương ứng
					LevelUpItem awardInfo = this.LevelUpItem[i - 1];
					/// Trạng thái tương ứng
					int value = states[i];
					/// Nếu chưa nhận
					if (value == 0 && Global.Data.RoleData.Level >= awardInfo.ToLevel)
					{
						return true;
					}
				}
				/// Không có gì để nhận
				return false;
			}
		}

		/// <summary>
		/// Trả về thông tin phần thưởng tại mốc tương ứng
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public LevelUpItem GetAward(int id)
		{
			return this.LevelUpItem.Where(x => x.ID == id).FirstOrDefault();
		}

		/// <summary>
		/// Trả về trạng thái tại mốc có ID tương ứng
		/// </summary>
		/// <param name="id"></param>
		/// <param name="myLevel"></param>
		/// <returns></returns>
		public int GetState(int id)
		{
			/// Quà tương ứng
			LevelUpItem awardInfo = this.GetAward(id);
			/// Nếu không tìm thấy
			if (awardInfo == null)
			{
				return 0;
            }

            /// Giá trị từng mốc
            Dictionary<int, int> states = new Dictionary<int, int>();
            foreach (string stateString in this.BitFlags.Split('|'))
            {
                string[] fields = stateString.Split('_');
                int stepID = int.Parse(fields[0]);
                int value = int.Parse(fields[1]);
                states[stepID] = value;
            }

            /// Nếu không đủ cấp
            if (states[id] == 1)
			{
				return 0;
			}

            /// Nếu đã nhận
            if (states[id] == 2)
			{
				return 2;
			}

			/// Trả ra có thể nhận
			return 1;
		}
	}
}
