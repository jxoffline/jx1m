using System.Xml.Linq;

namespace FS.VLTK.Entities.Config
{
	/// <summary>
	/// Dữ liệu nhiệm vụ của hệ thống
	/// </summary>
    public class TaskDataXML
    {
		/// <summary>
		/// ID nhiệm vụ
		/// </summary>
		public int ID { get; set; }

		/// <summary>
		/// Loại nhiệm vụ
		/// </summary>
		public int TaskClass { get; set; }

		/// <summary>
		/// Tên nhiệm vụ
		/// </summary>
		public string Title { get; set; }

		/// <summary>
		/// ID nhiệm vụ cần làm trước đó
		/// </summary>
		public int PrevTask { get; set; }

		/// <summary>
		/// ID nhiệm vụ tiếp theo
		/// </summary>
		public int NextTask { get; set; }

		/// <summary>
		/// Cấp độ tối thiểu cần
		/// </summary>
		public int MinLevel { get; set; }

		/// <summary>
		/// Cấp độ tối đa cần
		/// </summary>
		public int MaxLevel { get; set; }

		/// <summary>
		/// Yêu cầu giới tính
		/// </summary>
		public int SexCondition { get; set; }

		/// <summary>
		/// Yêu cầu môn phái
		/// </summary>
		public int OccupCondition { get; set; }

		/// <summary>
		/// Lời thoại của NPC khi nhận
		/// </summary>
		public string AcceptTalk { get; set; }

		/// <summary>
		/// Hướng dẫn nhiệm vụ
		/// </summary>
		public string DoingTalk { get; set; }

		/// <summary>
		/// Danh sách câu hỏi
		/// </summary>
		public string QuestionTable { get; set; }

		/// <summary>
		/// Lời thoại của NPC khi trả nhiệm vụ
		/// </summary>
		public string CompleteTalk { get; set; }

		/// <summary>
		/// ID NPC giao nhiệm vụ
		/// </summary>
		public int SourceNPC { get; set; }

		/// <summary>
		/// ID bản đồ được giao nhiệm vụ
		/// </summary>
		public int SourceMapCode { get; set; }

		/// <summary>
		/// Bản đồ NPC trả nhiệm vụ
		/// </summary>
		public int DestMapCode { get; set; }

		/// <summary>
		/// ID NPC trả nhiệm vụ
		/// </summary>
		public int DestNPC { get; set; }

		/// <summary>
		/// ID đối tượng của nhiệm vụ, ví dụ giết hươu thì đây là ID RES trong file cấu hình
		/// </summary>
		public int TargetNPC { get; set; }

		/// <summary>
		/// ID bản đồ 
		/// </summary>
		public int TargetMapCode { get; set; }

		/// <summary>
		/// Kiểu nhiệm vụ
		/// </summary>
		public int TargetType { get; set; }

		/// <summary>
		/// Danh sách vật phẩm
		/// </summary>
		public string PropsName { get; set; }

		/// <summary>
		/// Tỷ lệ rơi vật phẩm nhiệm vụ tương ứng
		/// </summary>
		public int FallPercent { get; set; }

		/// <summary>
		/// Số lượng
		/// </summary>
		public int TargetNum { get; set; }

		/// <summary>
		/// Vị trí đích
		/// </summary>
		public string TargetPos { get; set; }

		/// <summary>
		/// Danh sách phần thưởng cố định
		/// </summary>
		public string Taskaward { get; set; }

		/// <summary>
		/// Danh sách phần thưởng được chọn
		/// </summary>
		public string OtherTaskaward { get; set; }

		/// <summary>
		/// Danh sách phần thưởng ngẫu nhiêm
		/// </summary>
		public string RandomTaskaward { get; set; }

		/// <summary>
		/// Số bạc khóa nhận được
		/// </summary>
		public int BacKhoa { get; set; }

		/// <summary>
		/// Số bạc nhận được
		/// </summary>
		public int Bac { get; set; }

		/// <summary>
		/// Số KNB khóa nhận được
		/// </summary>
		public int DongKhoa { get; set; }

		/// <summary>
		/// Giá trị kinh nghiệm
		/// </summary>
		public int Experienceaward { get; set; }

		/// <summary>
		/// Tham số 1
		/// </summary>
		public int Point1 { get; set; }

		/// <summary>
		/// Tham số 2
		/// </summary>
		public int Point2 { get; set; }

		/// <summary>
		/// Tham số 3
		/// </summary>
		public int Point3 { get; set; }

		/// <summary>
		/// Tham số 4
		/// </summary>
		public int Point4 { get; set; }

		/// <summary>
		/// Tham số 5
		/// </summary>
		public int Point5 { get; set; }

		/// <summary>
		/// Hạn sử dụng vật phẩm nhiệm vụ
		/// </summary>
		public int GoodsEndTime { get; set; }

		/// <summary>
		/// Buff nhận được khi hoàn thành nhiệm vụ
		/// </summary>
		public int BuffID { get; set; }

		/// <summary>
		/// Dịch chuyển đi đâu khi hoàn thành nhiệm vụ
		/// </summary>
		public string Teleports { get; set; }

		/// <summary>
		/// Chuyển đối tượng từ XML Node
		/// </summary>
		/// <param name="xmlNode"></param>
		/// <returns></returns>
		public static TaskDataXML Parse(XElement xmlNode)
        {
			return new TaskDataXML()
			{
				ID = int.Parse(xmlNode.Attribute("ID").Value),
				TaskClass = int.Parse(xmlNode.Attribute("TaskClass").Value),
				Title = xmlNode.Attribute("Title").Value,
				PrevTask = int.Parse(xmlNode.Attribute("PrevTask").Value),
				NextTask = int.Parse(xmlNode.Attribute("NextTask").Value),
				MinLevel = int.Parse(xmlNode.Attribute("MinLevel").Value),
				MaxLevel = int.Parse(xmlNode.Attribute("MaxLevel").Value),
				SexCondition = int.Parse(xmlNode.Attribute("SexCondition").Value),
				OccupCondition = int.Parse(xmlNode.Attribute("OccupCondition").Value),
				AcceptTalk = xmlNode.Attribute("AcceptTalk") == null ? "" : xmlNode.Attribute("AcceptTalk").Value,
				DoingTalk = xmlNode.Attribute("DoingTalk") == null ? "" : xmlNode.Attribute("DoingTalk").Value,
				QuestionTable = xmlNode.Attribute("QuestionTable") == null ? "" : xmlNode.Attribute("QuestionTable").Value,
				CompleteTalk = xmlNode.Attribute("CompleteTalk") == null ? "" : xmlNode.Attribute("CompleteTalk").Value,
				TargetPos = xmlNode.Attribute("TargetPos") == null ? "" : xmlNode.Attribute("TargetPos").Value,
				SourceNPC = int.Parse(xmlNode.Attribute("SourceNPC").Value),
				SourceMapCode = int.Parse(xmlNode.Attribute("SourceMapCode").Value),
				DestMapCode = int.Parse(xmlNode.Attribute("DestMapCode").Value),
				DestNPC = int.Parse(xmlNode.Attribute("DestNPC").Value),
				TargetType = int.Parse(xmlNode.Attribute("TargetType").Value),
				TargetNPC = int.Parse(xmlNode.Attribute("TargetNPC").Value),
				PropsName = xmlNode.Attribute("PropsName") == null ? "" : xmlNode.Attribute("PropsName").Value,
				FallPercent = int.Parse(xmlNode.Attribute("FallPercent").Value),
				TargetNum = int.Parse(xmlNode.Attribute("TargetNum").Value),
				TargetMapCode = int.Parse(xmlNode.Attribute("TargetMapCode").Value),
				Taskaward = xmlNode.Attribute("Taskaward") == null ? "" : xmlNode.Attribute("Taskaward").Value,
				OtherTaskaward = xmlNode.Attribute("OtherTaskaward") == null ? "" : xmlNode.Attribute("OtherTaskaward").Value,
				RandomTaskaward = xmlNode.Attribute("RandomTaskaward") == null ? "" : xmlNode.Attribute("RandomTaskaward").Value,
				BacKhoa = int.Parse(xmlNode.Attribute("BacKhoa").Value),
				Bac = int.Parse(xmlNode.Attribute("Bac").Value),
				DongKhoa = int.Parse(xmlNode.Attribute("DongKhoa").Value),
				Experienceaward = int.Parse(xmlNode.Attribute("Experienceaward").Value),
				Point1 = int.Parse(xmlNode.Attribute("Point1").Value),
				Point2 = int.Parse(xmlNode.Attribute("Point2").Value),
				Point3 = int.Parse(xmlNode.Attribute("Point3").Value),
				Point4 = int.Parse(xmlNode.Attribute("Point4").Value),
				Point5 = int.Parse(xmlNode.Attribute("Point5").Value),
				GoodsEndTime = int.Parse(xmlNode.Attribute("GoodsEndTime").Value),
				BuffID = int.Parse(xmlNode.Attribute("BuffID").Value),
				Teleports = xmlNode.Attribute("Teleports") == null ? "" : xmlNode.Attribute("Teleports").Value,
			};
        }
	}
}
