using ProtoBuf;

namespace Server.Data
{
    [ProtoContract]
    public class RoleWelfare
    {
        /// <summary>
        /// Ngày gần đây nhất đã đăng nhập là ngày nào sử dụng để tính toán đăng nhập liên tục
        /// </summary>
        ///
        [ProtoMember(1)]
        public int lastdaylogin { get; set; } = 0;

        /// <summary>
        /// Đăng nhập liên tục trong bao lâu
        /// </summary>
        ///
        [ProtoMember(2)]
        public int logincontinus { get; set; } = 0;

        /// <summary>
        /// Đã nhận mốc nào rồi trong cái đăng nhập 7 ngày liên tục | Cái này sẽ ko reset suốt đời
        /// </summary>
        ///
        [ProtoMember(3)]
        public int sevenday_continus_step { get; set; } = 0;

        /// <summary>
        /// Quà đã nhận khi đăng nhập liên tiếp trong 7 ngày | Cái này sẽ ko reset suốt đời
        /// </summary>
        ///
        [ProtoMember(4)]
        public string sevenday_continus_note { get; set; } = "NONE";

        /// <summary>
        /// Quà đã nhận khi đăng nhập 7 ngày DAYID|ITEMID|COUNT_DAYID|ITEMID|COUNT
        /// </summary>
        ///
        [ProtoMember(5)]
        public string sevendaylogin_note { get; set; } = "NONE";

        /// <summary>
        /// Đã nhận mốc nào rồi trong cái đăng nhập 7 ngày  VD : 1|2|3|4
        /// </summary>
        ///
        [ProtoMember(6)]
        public string sevendaylogin_step { get; set; } = "NONE";

        /// <summary>
        /// Ghi lại ngày đầu tiên nhân vật được tạo và kích hoạt chuỗi sự kiện 7 ngày
        /// </summary>
        ///
        [ProtoMember(7)]
        public int createdayid { get; set; } = 0;

        /// <summary>
        /// Ghi lại ngày đăng nhập vào là ngày nào | để tính toán cái online nhận thưởng=> sang ngày mới là clear chuỗi nhận
        /// </summary>
        ///
        [ProtoMember(8)]
        public int logindayid { get; set; } = 0;

        /// <summary>
        /// Ghi lại tuần đăng nhập là tuần nào
        /// </summary>
        ///
        [ProtoMember(9)]
        public int loginweekid { get; set; } = 0;

        /// <summary>
        /// Ghi lại DayLoginID và STEP nào ví dụ 254_1|2 tức là vào ngày 254 người này đã online nhận 2 mốc thưởng | Sang ngày mới thì tự động reset
        /// </summary>
        ///
        [ProtoMember(10)]
        public string online_step { get; set; } = "NONE";

        /// <summary>
        /// Ghi lại xem thằng nhân vật này đã nhận quà thăng cấp mốc nào rồi ghi theo chuỗi 1_2_3 tức là đã nhận mốc 1 2 3 nếu mà chuỗi là 2_3 thì tức là chỉ nhận mốc 2 và 3
        /// </summary>
        ///
        [ProtoMember(11)]
        public string levelup_step { get; set; } = "NONE";

        /// <summary>
        /// Ghi lại xem tháng đăng nhập này là tháng nào để xử lý quà điểm danh 30 ngày
        /// </summary>
        ///
        [ProtoMember(12)]
        public int monthid { get; set; } = 0;

        /// <summary>
        /// Ghi lại xem đã điểm danh ngày nào trong tháng | Nếu tháng này mà khác tháng cũ thì sẽ reset checkpoint đánh dấu 2_3_4 tức là đã nhận ngày 2 3 4
        /// </summary>
        ///
        [ProtoMember(13)]
        public string checkpoint { get; set; } = "NONE";

        /// <summary>
        /// Ghi lại xem đã nhận quà nạp lần đầu hay chưa | 0 là chưa nhận | 1 là đã nhận
        /// </summary>
        ///
        [ProtoMember(14)]
        public int fist_recharge_step { get; set; } = 0;

        /// <summary>
        /// Ghi lại xem đã hốc mốc quà tích nạp nào rồi 2_3_4 tức là đã nhận mốc 2 3 4
        /// </summary>
        ///
        [ProtoMember(15)]
        public string totarechage_step { get; set; } = "NONE";

        /// <summary>
        /// Ghi lại xem đã hốc mốc quà tích tiêu nào rồi nào rồi 2_3_4 tức là đã nhận mốc 2 3 4
        /// </summary>
        ///
        [ProtoMember(16)]
        public string totalconsume_step { get; set; } = "NONE";

        /// <summary>
        /// Ghi lại tích lũy nạp ngày bao gồm ngày ID và STEP nào ví dụ 254_1_2_3 tức là vào ngày 254 người này đã nhận mốc 1 2 3 | Sang ngày mới bản ghi này tự động reset
        /// </summary>
        ///
        [ProtoMember(17)]
        public string day_rechage_step { get; set; } = "NONE";

        [ProtoMember(18)]
        public int RoleID { get; set; } = -1;
    }
}