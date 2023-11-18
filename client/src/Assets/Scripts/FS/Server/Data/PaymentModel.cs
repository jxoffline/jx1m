using ProtoBuf;

namespace Server.Data
{
    [ProtoContract]
    public class RecipeVerify
    {
        /// <summary>
        /// Tên thằng mua gói
        /// </summary>
        ///
        [ProtoMember(1)]
        public int RoleID { get; set; }

        /// <summary>
        /// Tài khoản thằng mua gói
        /// </summary>
        ///
        [ProtoMember(2)]
        public string UserToken { get; set; }

        /// <summary>
        /// Mã giao dịch của gói mua
        /// <summary>
        /// Mã của gói mua lúc đầu
        /// </summary>
        ///
        [ProtoMember(3)]
        public string TransID { get; set; }

        /// <summary>
        ///Toàn bộ chuỗi json của google đẩy lên đây
        /// </summary>
        ///
        [ProtoMember(4)]
        public string PurchaseData { get; set; }

        [ProtoMember(5)]
        public int ServerID { get; set; }
    }

    [ProtoContract]
    public class PaymentVerifyRep
    {
        [ProtoMember(1)]
        public int Status { get; set; }

        [ProtoMember(2)]
        public string Msg { get; set; }

        [ProtoMember(3)]
        public string ProductBuy { get; set; }
    }

    [ProtoContract]
    public class PaymentRequestRep
    {
        [ProtoMember(1)]
        public int Status { get; set; }

        [ProtoMember(2)]
        public string Msg { get; set; }

        [ProtoMember(3)]
        public string ProductBuy { get; set; }

        [ProtoMember(4)]
        public string TransID { get; set; }
    }

    [ProtoContract]
    public class PaymentRequest
    {
        /// <summary>
        /// Tên thằng mua gói
        /// </summary>
        ///
        [ProtoMember(1)]
        public int RoleID { get; set; }

        /// <summary>
        /// Tài khoản thằng mua gói
        /// </summary>
        ///

        [ProtoMember(2)]
        public string UserToken { get; set; }

        /// <summary>
        /// Mã giao dịch của gói mua
        /// </summary>
        ///
        [ProtoMember(3)]
        public string TransID { get; set; }

        /// <summary>
        /// Tên gói mua
        /// </summary>
        ///
        [ProtoMember(4)]
        public string PackageName { get; set; }

        /// <summary>
        /// ServerID để biết server nào mà còn gọi SERVICE ADD KNB
        /// </summary>
        ///
        [ProtoMember(5)]
        public int ServerID { get; set; }

        /// <summary>
        /// DEVICE ID Của thiết bị
        /// </summary>
        ///
        [ProtoMember(6)]
        public string DeviceID { get; set; }

        [ProtoMember(7)]
        public string PlatForm { get; set; }
    }
}