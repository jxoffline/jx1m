using System.Text;
using System.Xml.Linq;

namespace FogTeam
{

    public class PaymentInfo
    {
        public string Payload { get; set; }
        public string Store { get; set; }
        public string TransactionID { get; set; }
    }
    /// <summary>
    /// Thông tin gói hàng
    /// </summary>
    public class PurchaseData
    {
        /// <summary>
        /// ID gói hàng
        /// </summary>
        public string orderId { get; set; }

        /// <summary>
        /// Tên Packet
        /// </summary>
        public string packageName { get; set; }

        /// <summary>
        /// ID sản phẩm
        /// </summary>
        public string productId { get; set; }

        /// <summary>
        /// Thời gian mua
        /// </summary>
        public long purchaseTime { get; set; }

        /// <summary>
        /// Trạng thái mua
        /// </summary>
        public int purchaseState { get; set; }

        /// <summary>
        /// Chuỗi mã hóa đơn hàng
        /// </summary>
        public string purchaseToken { get; set; }

        /// <summary>
        /// Chứng nhận không
        /// </summary>
        public bool acknowledged { get; set; }

        /// <summary>
        /// Chuyển đối tượng thành dạng String
        /// </summary>
        /// <returns></returns>
		public override string ToString()
		{
            StringBuilder builder = new StringBuilder();
            builder.AppendLine(string.Format("OrderID = {0}", this.orderId));
            builder.AppendLine(string.Format("PackageName = {0}", this.packageName));
            builder.AppendLine(string.Format("ProductID = {0}", this.productId));
            builder.AppendLine(string.Format("PurchaseTime = {0}", this.purchaseTime));
            builder.AppendLine(string.Format("PurchaseState = {0}", this.purchaseState));
            builder.AppendLine(string.Format("PurchaseToken = {0}", this.purchaseToken));
            builder.AppendLine(string.Format("Acknowledged = {0}", this.acknowledged));
            return builder.ToString();
		}

     
	}
}
