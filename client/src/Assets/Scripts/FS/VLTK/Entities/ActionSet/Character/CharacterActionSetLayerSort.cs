using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using static FS.VLTK.Entities.Enum;

namespace FS.VLTK.Entities.ActionSet.Character
{
	/// <summary>
	/// Thông tin thứ tự sắp xếp Frame người
	/// </summary>
	public class CharacterActionSetLayerSort
	{
		/// <summary>
		/// Thông tin động tác
		/// </summary>
		public class ActionInfo
		{
			/// <summary>
			/// Tên động tác
			/// </summary>
			public string Name { get; set; }

			/// <summary>
			/// Danh sách theo hướng
			/// </summary>
			public Dictionary<byte, byte[]> Dirs { get; set; }

			/// <summary>
			/// Danh sách theo Layer
			/// </summary>
			public Dictionary<byte, byte[]> Layers { get; set; }

			/// <summary>
			/// Chuyển đối tượng từ XMLNode
			/// </summary>
			/// <param name="xmlNode"></param>
			/// <returns></returns>
			public static ActionInfo Parse(XElement xmlNode)
			{
				/// Tạo mới đối tượng
				ActionInfo actionInfo = new ActionInfo()
				{
					Name = xmlNode.Attribute("Name").Value,
				};

				/// Nếu tồn tại danh sách theo hướng
				if (xmlNode.Element("Dirs").HasElements)
				{
					actionInfo.Dirs = new Dictionary<byte, byte[]>();
					foreach (XElement node in xmlNode.Element("Dirs").Elements("Dir"))
					{
						byte dirIndex = byte.Parse(node.Attribute("ID").Value);

						List<byte> values = new List<byte>();
						string[] fields = node.Attribute("Values").Value.Split(',');
						foreach (string field in fields)
						{
							values.Add(byte.Parse(field));
						}

						/// Thêm vào danh sách
						actionInfo.Dirs[dirIndex] = values.ToArray();
					}
				}

				/// Nếu tồn tại danh sách theo Layer
				if (xmlNode.Element("Layers").HasElements)
				{
					actionInfo.Layers = new Dictionary<byte, byte[]>();
					foreach (XElement node in xmlNode.Element("Layers").Elements("Layer"))
					{
						byte layerID = byte.Parse(node.Attribute("ID").Value);

						List<byte> values = new List<byte>();
						string[] fields = node.Attribute("Values").Value.Split(',');
						foreach (string field in fields)
						{
							values.Add(byte.Parse(field));
						}

						/// Thêm vào danh sách
						actionInfo.Layers[layerID] = values.ToArray();
					}
				}

				/// Trả về đối tượng
				return actionInfo;
			}
		}

		/// <summary>
		/// Danh sách động tác của nam
		/// </summary>
		public Dictionary<string, ActionInfo> MaleActions { get; set; }

		/// <summary>
		/// Danh sách động tác của nữ
		/// </summary>
		public Dictionary<string, ActionInfo> FemaleActions { get; set; }

		/// <summary>
		/// Chuyển đối tượng từ XMLNode
		/// </summary>
		/// <param name="xmlNode"></param>
		/// <returns></returns>
		public static CharacterActionSetLayerSort Parse(XElement xmlNode)
		{
			/// Tạo mới đối tượng
			CharacterActionSetLayerSort actionSetLayerSort = new CharacterActionSetLayerSort()
			{
				MaleActions = new Dictionary<string, ActionInfo>(),
				FemaleActions = new Dictionary<string, ActionInfo>(),
			};

			/// Duyệt danh sách động tác của nam
			foreach (XElement node in xmlNode.Element("Male").Elements("Action"))
			{
				ActionInfo actionInfo = ActionInfo.Parse(node);
				actionSetLayerSort.MaleActions[actionInfo.Name] = actionInfo;
			}

			/// Duyệt danh sách động tác của nữ
			foreach (XElement node in xmlNode.Element("Female").Elements("Action"))
			{
				ActionInfo actionInfo = ActionInfo.Parse(node);
				actionSetLayerSort.FemaleActions[actionInfo.Name] = actionInfo;
			}

			/// Trả về kết quả
			return actionSetLayerSort;
		}

		/// <summary>
		/// Trả về Layer tương ứng
		/// </summary>
		/// <param name="sex">Giới tính</param>
		/// <param name="frameID">Thứ tự Frame</param>
		/// <param name="dir">Hướng</param>
		/// <param name="actionName">Tên động tác</param>
		/// <param name="pathIndex">ID bộ: ARMOR_BODY = 5, ARMOR_LEFTHARD = 6, ARMOR_RIGHTHARD = 7, HELM_HEAD = 0, HELM_HAIR = 1, MANTLE = 16, WEP_LEFT = 8, WEP_RIGHT = 9, m_horse_horseback = 14, m_horse_horsefront = 12, m_horse_horsemiddle = 13</param>
		/// <returns></returns>
		public int GetLayer(int sex, int frameID, int dir, string actionName, byte pathIndex)
		{
			/// Layer tương ứng
			int layer = -1;

			/// Thông tin động tác
			Dictionary<string, ActionInfo> actions = sex == (int) Sex.MALE ? this.MaleActions : this.FemaleActions;

			/// Thông tin động tác tương ứng
			if (actions.TryGetValue(actionName, out ActionInfo actionInfo))
			{
				/// Nếu nằm trong danh sách Layer
				if (actionInfo.Layers != null && actionInfo.Layers.TryGetValue((byte) frameID, out byte[] layerValues))
				{
					/// Duyệt danh sách
					for (int i = 0; i < layerValues.Length; i++)
					{
						/// Nếu đây là bộ phận cần tìm
						if (layerValues[i] == pathIndex)
						{
							/// Trả ra kết quả
							layer = i;
							break;
						}
					}
				}
				/// Nếu nằm trong danh sách hướng
				else if (actionInfo.Dirs != null && actionInfo.Dirs.TryGetValue((byte) dir, out byte[] dirValues))
				{
					/// Duyệt danh sách
					for (int i = 0; i < dirValues.Length; i++)
					{
						/// Nếu đây là bộ phận cần tìm
						if (dirValues[i] == pathIndex)
						{
							/// Trả ra kết quả
							layer = i;
							break;
						}
					}
				}
			}
			
			/// Nếu không tìm thấy
			if (layer == -1)
			{
				/// Lấy giá trị mặc định
				if (actions.TryGetValue("DEFAULT", out ActionInfo defaultActionInfo))
				{
					/// Nếu nằm trong danh sách hướng
					if (defaultActionInfo.Dirs != null && defaultActionInfo.Dirs.TryGetValue((byte) dir, out byte[] dirValues))
					{
						/// Duyệt danh sách
						for (int i = 0; i < dirValues.Length; i++)
						{
							/// Nếu đây là bộ phận cần tìm
							if (dirValues[i] == pathIndex)
							{
								/// Trả ra kết quả
								layer = i;
								break;
							}
						}
					}
				}
			}

			/// Trả về kết quả
			return layer;
		}
	}
}
