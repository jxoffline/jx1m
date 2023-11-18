using GameServer.Logic;
using System;
using System.IO;
using System.Xml.Linq;

namespace GameServer.KiemThe
{
	/// <summary>
	/// Thiết lập hệ thống
	/// </summary>
	public class ServerConfig
	{
		#region Singleton - Instance
		/// <summary>
		/// Thiết lập hệ thống
		/// </summary>
		public static ServerConfig Instance { get; private set; }

		/// <summary>
		/// Thiết lập hệ thống
		/// </summary>
		private ServerConfig() { }
		#endregion

		/// <summary>
		/// CCU tối đa
		/// </summary>
		public int MaxCCU { get; set; }



        public double MoneyRate { get; set; }


        public double ExpRate { get; set; }

        /// <summary>
        /// Giới hạn tài khoản trên địa chỉ IP
        /// </summary>
        public int LimitAccountPerIPAddress { get; set; }

		/// <summary>
		/// Mở luồng thực thi di chuyển ngẫu nhiên của quái
		/// </summary>
		public bool EnableMonsterAIRandomMove { get; set; }

		/// <summary>
		/// Mở luồng thực thi AI quái
		/// </summary>
		public bool EnableMonsterAI { get; set; }

		/// <summary>
		/// Mở liên máy chủ
		/// </summary>
		public bool EnableCrossServer { get; set; }

		/// <summary>
		/// Số luồng UpdateGrid chạy đồng thời
		/// </summary>
		public int MaxUpdateGridThread { get; set; }

		/// <summary>
		/// Số luồng StoryBoard chạy đồng thời
		/// </summary>
		public int MaxPlayerStoryBoardThread { get; set; }

		/// <summary>
		/// SỐ luồng Monster chạy đồng thời
		/// </summary>
		public int MaxMonsterTimer { get; set; }

		/// <summary>
		/// Số luồng Buff chạy đồng thời
		/// </summary>
		public int MaxBuffTimer { get; set; }

        /// <summary>
        /// SỐ luồng Pet chạy đồng thời
        /// </summary>
        public int MaxPetTimer { get; set; }

        /// <summary>
        /// SỐ luồng xe tiêu chạy đồng thời
        /// </summary>
        public int MaxTraderCarriageTimer { get; set; }

		/// <summary>
		/// Số luồng Bot chạy đồng thời
		/// </summary>
		public int MaxBotTimer { get; set; }

        /// <summary>
        /// Giới hạn cấp độ
        /// </summary>
        public int LimitLevel { get; set; }

		/// <summary>
		/// Tổng số quái sau khi tiêu diệt đủ sẽ xuất hiện tinh anh ở mỗi bãi
		/// </summary>
		public int MonsterKilledToSpawnElite { get; set; }

		/// <summary>
		/// Tổng số quái sau khi tiêu diệt đủ sẽ xuất hiện thủ lĩnh ở mỗi bãi
		/// </summary>
		public int MonsterKilledToSpawnLeader { get; set; }

		/// <summary>
		/// Số lượng lửa trại đồng thời cùng tinh anh tối đa có thể tồn tại đồng thời trên cùng 1 bãi
		/// </summary>
		public int MaxFireCampAndElitePerZone { get; set; }

		/// <summary>
		/// Kích hoạt Captcha
		/// </summary>
		public bool EnableCaptcha { get; set; }

		/// <summary>
		/// Kích hoạt Captcha cho các thiết bị iOS
		/// </summary>
		public bool EnableCaptchaForIOS { get; set; }

		/// <summary>
		/// Thời gian xuất hiện tối thiểu
		/// </summary>
		public int CaptchaAppearMinPeriod { get; set; }

		/// <summary>
		/// Thời gian xuất hiện tối đa
		/// </summary>
		public int CaptchaAppearMaxPeriod { get; set; }

		/// <summary>
		/// Captcha chỉ xuất hiện ở người chơi có nhóm
		/// </summary>
		public bool CaptchaTeamPlayersOnly { get; set; }

		/// <summary>
		/// Thời gian xuất hiện Captcha trong ngày
		/// </summary>
		public DateTime CaptchaAppearFromTime { get; set; }

		/// <summary>
		/// Thời gian kết thúc xuất hiện Captcha trong ngày
		/// </summary>
		public DateTime CaptchaAppearToTime { get; set; }

		/// <summary>
		/// Kinh nghiệm nhận được khi trả lời đúng Captcha
		/// </summary>
		public int CaptchaExpAddPerLevel { get; set; }

		/// <summary>
		/// Bạc khóa nhận được khi trả lời đúng Captcha
		/// </summary>
		public int CaptchaBoundMoneyAddPerLevel { get; set; }

		/// <summary>
		/// Khởi tạo
		/// </summary>
		public static void Init()
		{
			XElement xmlNode = XElement.Parse(File.ReadAllText("ServerConfig.xml"));
			ServerConfig.Instance = new ServerConfig()
			{
                MoneyRate = double.Parse(xmlNode.Element("ServerRate").Attribute("MoneyRate").Value),
                ExpRate = double.Parse(xmlNode.Element("ServerRate").Attribute("ExpRate").Value),

                MaxCCU = int.Parse(xmlNode.Element("LimitAccount").Attribute("MaxCCU").Value),
				LimitAccountPerIPAddress = int.Parse(xmlNode.Element("LimitAccount").Attribute("LimitAccountPerIPAddress").Value),
				EnableMonsterAIRandomMove = bool.Parse(xmlNode.Element("MonsterAI").Attribute("EnableMonsterAIRandomMove").Value),
				EnableMonsterAI = bool.Parse(xmlNode.Element("MonsterAI").Attribute("EnableMonsterAI").Value),
				MaxUpdateGridThread = int.Parse(xmlNode.Element("Threading").Attribute("MaxUpdateGridThread").Value),
				MaxPlayerStoryBoardThread = int.Parse(xmlNode.Element("Threading").Attribute("MaxPlayerStoryBoardThread").Value),
				MaxMonsterTimer = int.Parse(xmlNode.Element("Threading").Attribute("MaxMonsterTimer").Value),
				MaxBuffTimer = int.Parse(xmlNode.Element("Threading").Attribute("MaxBuffTimer").Value),
				MaxPetTimer = int.Parse(xmlNode.Element("Threading").Attribute("MaxPetTimer").Value),
				MaxTraderCarriageTimer = int.Parse(xmlNode.Element("Threading").Attribute("MaxTraderCarriageTimer").Value),
				MaxBotTimer = int.Parse(xmlNode.Element("Threading").Attribute("MaxBotTimer").Value),
				LimitLevel = int.Parse(xmlNode.Element("GameConfig").Attribute("LimitLevel").Value),
				MonsterKilledToSpawnElite = int.Parse(xmlNode.Element("GameConfig").Attribute("MonsterKilledToSpawnElite").Value),
				MonsterKilledToSpawnLeader = int.Parse(xmlNode.Element("GameConfig").Attribute("MonsterKilledToSpawnLeader").Value),
				MaxFireCampAndElitePerZone = int.Parse(xmlNode.Element("GameConfig").Attribute("MaxFireCampAndElitePerZone").Value),
				EnableCrossServer = bool.Parse(xmlNode.Element("CrossServer").Attribute("EnableCrossServer").Value),

				EnableCaptcha = bool.Parse(xmlNode.Element("Captcha").Attribute("EnableCaptcha").Value),
				EnableCaptchaForIOS = bool.Parse(xmlNode.Element("Captcha").Attribute("EnableCaptchaForIOS").Value),
				CaptchaAppearMinPeriod = int.Parse(xmlNode.Element("Captcha").Attribute("CaptchaAppearMinPeriod").Value),
				CaptchaAppearMaxPeriod = int.Parse(xmlNode.Element("Captcha").Attribute("CaptchaAppearMaxPeriod").Value),
				CaptchaTeamPlayersOnly = bool.Parse(xmlNode.Element("Captcha").Attribute("CaptchaTeamPlayersOnly").Value),
				CaptchaExpAddPerLevel = int.Parse(xmlNode.Element("Captcha").Attribute("CaptchaExpAddPerLevel").Value),
				CaptchaBoundMoneyAddPerLevel = int.Parse(xmlNode.Element("Captcha").Attribute("CaptchaBoundMoneyAddPerLevel").Value),
			};

            /// Thời gian bắt đầu xuất hiện
            {
				string timeString = xmlNode.Element("Captcha").Attribute("CaptchaAppearFromTime").Value;
				string[] fields = timeString.Split(':');
				int hour = int.Parse(fields[0]);
                int minute = int.Parse(fields[1]);
				/// Ngày hôm nay
				DateTime now = DateTime.Now;
				ServerConfig.Instance.CaptchaAppearFromTime = new DateTime(now.Year, now.Month, now.Day, hour, minute, 0);
			}

            /// Thời gian kết thúc xuất hiện
            {
				string timeString = xmlNode.Element("Captcha").Attribute("CaptchaAppearToTime").Value;
				string[] fields = timeString.Split(':');
				int hour = int.Parse(fields[0]);
                int minute = int.Parse(fields[1]);
				/// Ngày hôm nay
				DateTime now = DateTime.Now;
				ServerConfig.Instance.CaptchaAppearToTime = new DateTime(now.Year, now.Month, now.Day, hour, minute, 0);
			}
		}
	}
}
