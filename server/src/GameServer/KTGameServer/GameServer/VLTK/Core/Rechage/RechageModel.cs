namespace GameServer.KiemThe.Core.Rechage
{
    public class RechageModel
    {
        /// <summary>
        /// RoleId thực hiện rechage
        /// </summary>
        public int RoleID { get; set; }

        /// <summary>
        /// Mã giao dịch
        /// </summary>
        public string TransID { get; set; }

        /// <summary>
        /// Tên gói thực hiện mua
        /// </summary>
        public string PackageName { get; set; }


        public string Sing { get; set; }


        public string TimeBuy { get; set; }

        /// <summary>
        /// Nếu như là kích hoạt thẻ tháng
        /// </summary>
        public bool ActiveCardMonth { get; set; }
    }
}