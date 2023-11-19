using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace FogTeam
{
    /// <summary>
    /// Phân tích Purchase-Response của Google
    /// </summary>
    public class GooglePurchaseResponseAnalysis
    {
        /// <summary>
        /// Đọc dữ liệu từ chuỗi Json đầu vào, theo Regex tương ứng
        /// </summary>
        /// <param name="inputJsonString"></param>
        /// <param name="regex"></param>
        /// <returns></returns>
        private static string GetJsonResult(string inputJsonString, string regex)
        {
            Match match = Regex.Match(inputJsonString, regex);
            if (match == null)
            {
                return "";
            }
            return match.Groups[1].Value;
        }


        /// <summary>
        /// Trả về dữ liệu mua hàng
        /// </summary>
        /// <param name="inputJsonString"></param>
        /// <returns></returns>
        public static PurchaseData GetPurchaseData(string inputJsonString)
        {
            /// Tạo mới dữ liệu
            PurchaseData purchaseData = new PurchaseData();

            /// OrderID
            string getOrderIDRegex = @"\\\\\\""orderId\\\\\\"":\\\\\\""(.*)\\\\\\"",\\\\\\""packageName";
            purchaseData.orderId = GooglePurchaseResponseAnalysis.GetJsonResult(inputJsonString, getOrderIDRegex);

            /// PackageName
            string getPacketNameRegex = @"\\\\\\""packageName\\\\\\"":\\\\\\""(.*)\\\\\\"",\\\\\\""productId";
            purchaseData.packageName = GooglePurchaseResponseAnalysis.GetJsonResult(inputJsonString, getPacketNameRegex);

            /// ProductID
            string getProductIDRegex = @"\\\\\\""productId\\\\\\"":\\\\\\""(.*)\\\\\\"",\\\\\\""purchaseTime";
            purchaseData.productId = GooglePurchaseResponseAnalysis.GetJsonResult(inputJsonString, getProductIDRegex);

            /// PurchaseTime
            string getPurchaseTimeRegex = @"\\\\\\""purchaseTime\\\\\\"":(.*),\\\\\\""purchaseState";
            string purchaseTimeStr = GooglePurchaseResponseAnalysis.GetJsonResult(inputJsonString, getPurchaseTimeRegex);
            try
			{
                purchaseData.purchaseTime = long.Parse(purchaseTimeStr);

            }
            catch (Exception)
			{
                //Console.WriteLine("Toac PurchaseTime => " + purchaseTimeStr);
                return null;
			}

            /// PurchaseToken
            string getPurchaseTokenRegex = @"\\\\\\""purchaseToken\\\\\\"":\\\\\\""(.*)\\\\\\"",\\\\\\""acknowledged";
            purchaseData.purchaseToken = GooglePurchaseResponseAnalysis.GetJsonResult(inputJsonString, getPurchaseTokenRegex);

            /// Acknowledged
            string getAcknowledgedRegex = @"\\\\\\""acknowledged\\\\\\"":(.*)\}\\"",\\""signature";
            string acknowledgedString = GooglePurchaseResponseAnalysis.GetJsonResult(inputJsonString, getAcknowledgedRegex);
            try
			{
                purchaseData.acknowledged = bool.Parse(acknowledgedString);
			}
            catch (Exception)
			{
                //Console.WriteLine("Toac Acknowledged => " + purchaseTimeStr);
                return null;
			}

            /// Trả về kết quả
            return purchaseData;
        }

        /// <summary>
        /// Trả về chữ ký
        /// </summary>
        /// <param name="inputJsonString"></param>
        /// <returns></returns>
        public static string GetSignature(string inputJsonString)
        {
            /// Signature
            string getSignatureRegex = @"\\""signature\\""\:\\""(.*)\\"",\\""skuDetails";
            return GooglePurchaseResponseAnalysis.GetJsonResult(inputJsonString, getSignatureRegex);
        }
    }
}
