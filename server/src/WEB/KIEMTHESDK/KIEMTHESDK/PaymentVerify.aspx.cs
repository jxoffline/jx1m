using FogTeam;
using KIEMTHESDK.Database;
using KIEMTHESDK.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Server.Tools;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace KIEMTHESDK
{
    public partial class PaymentVerify : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            PaymentVerifyRep _DEMOREP = new PaymentVerifyRep();

            string GoogleBase64Key = "MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAtd7DPa4z07DY0otC25UILgLaXPppup+7jmsR7jajQ7KpHpymJQIzJUiTeM0bYqtQV29HoDi7UPyRx41W+T8IcJtNFyMitVAsX67SQa5SF2KQJIW0fCZCAQOHRrV1TLKfcdx7jDT9iortaXI9JIv30KwnMJJAx0//s49coDH+NKLdYbBjEk/VstCtWb39/hkhxavZXDWQJmexlMsQSmTTs1jRpuU/Smansbol6ytMiTJO+xKdxUk9Ly45H4mkIqRvPTH+8LMH+4kx1IZAMlXIE1WplwI4CySQXl3hPdq/YKUYN+ZwqVbH/fsQ/Jxnmx3HECgxp4N7/v14GEZVH3OQkQIDAQAB";

            byte[] array = base.Request.BinaryRead(base.Request.TotalBytes);
            int length = array.Length;

            RecipeVerify _RecipeVerify = DataHelper.BytesToObject<RecipeVerify>(array, 0, length);

            PaymentInfo _Payment = JsonConvert.DeserializeObject<PaymentInfo>(_RecipeVerify.PurchaseData);

            Unity _unity = new Unity();

            if (_Payment.Store == "GooglePlay")
            {
                string signature = GooglePurchaseResponseAnalysis.GetSignature(_RecipeVerify.PurchaseData);
                PurchaseData purchaseData = GooglePurchaseResponseAnalysis.GetPurchaseData(_RecipeVerify.PurchaseData);

                RSACryptoServiceProvider provider = PEMKeyLoader.CryptoServiceProviderFromPublicKeyInfo(GoogleBase64Key);

                string xmlPublicKey = provider.ToXmlString(false);

                string BuyData = JsonConvert.SerializeObject(purchaseData);

                bool IsOK = Unity.Verify(BuyData, signature, xmlPublicKey);
                if (IsOK)
                {
                    string Key = Unity.CreateMD5("9377(*)#mst9");
                    string RoleID = _RecipeVerify.RoleID + "";
                    string TransID = Unity.CreateMD5(_RecipeVerify.TransID + "");
                    string TimeBuy = Unity.CreateMD5(DateTime.Now.ToString());
                    string PackageName = purchaseData.productId + "";
                    string Sing = Unity.CreateMD5(PackageName + RoleID + TimeBuy + TransID);
                    string ActiveCardMonth = "0";

                    using (var db = new KiemTheDbEntities())
                    {
                        var FindServer = db.ServerLists.Where(x => x.nServerID == _RecipeVerify.ServerID).FirstOrDefault();
                        if (FindServer != null)
                        {
                            string IP = FindServer.strURL;

                            int PORT = (int)FindServer.HttpServicePort;

                            string HttpRequest = "http://" + IP + ":" + PORT + "/?Rechage=true&KeyStore=" + Key + "&RoleID=" + RoleID + "&TransID=" + TransID + "&PackageName=" + PackageName + "&Sing=" + Sing + "&TimeBuy=" + TimeBuy + "&ActiveCardMonth=" + ActiveCardMonth;

                            LogManager.WriteLog(LogTypes.SQL, "HTTP URL :" + HttpRequest);

                            try
                            {
                                WebClient wc = new WebClient();
                                wc.DownloadString(HttpRequest);
                                _DEMOREP.Msg = "Xác thực giao dịch thành công!";
                                _DEMOREP.ProductBuy = purchaseData.productId;
                                _DEMOREP.Status = 0;
                            }
                            catch (Exception ex)
                            {
                                LogManager.WriteLog(LogTypes.SQL, "HTTP URL :" + ex.ToString());
                                _DEMOREP.Msg = "Xác thực lỗi";
                                _DEMOREP.ProductBuy = purchaseData.productId;
                                _DEMOREP.Status = -1;
                            }
                        }
                    }
                }
                else
                {
                    _DEMOREP.Msg = "Xác thực giao dịch thất bại\nVui lòng liên hệ ADM để được hỗ trợ!";
                    _DEMOREP.ProductBuy = purchaseData.productId;
                    _DEMOREP.Status = -1;
                }
            }
            else if (_Payment.Store == "AppleAppStore")
            {
                try
                {
                    // CHANGE THIS URL IF SANDBOX
                    string URL = "https://sandbox.itunes.apple.com/verifyReceipt";
                    //string URL = "https://buy.itunes.apple.com/verifyReceipt";

                    var json = new JObject(new JProperty("receipt-data", _Payment.Payload)).ToString();

                    ASCIIEncoding ascii = new ASCIIEncoding();
                    byte[] postBytes = Encoding.UTF8.GetBytes(json);

                    //  HttpWebRequest request;
                    var request = System.Net.HttpWebRequest.Create(URL);
                    request.Method = "POST";
                    request.ContentType = "application/json";
                    request.ContentLength = postBytes.Length;

                    using (var stream = request.GetRequestStream())
                    {
                        stream.Write(postBytes, 0, postBytes.Length);
                        stream.Flush();
                    }

                    var sendresponse = request.GetResponse();

                    using (var streamReader = new StreamReader(sendresponse.GetResponseStream()))
                    {
                        string sendresponsetext = streamReader.ReadToEnd().Trim();

                        LogManager.WriteLog(LogTypes.SQL, "APPLE RESPONSE :" + sendresponsetext);

                        var JsonAppleRep = JsonConvert.DeserializeObject<IOSPAYREP>(sendresponsetext);

                        int AppleStatus = JsonAppleRep.status;

                        string Product_ID = JsonAppleRep.receipt.in_app[0].product_id;

                        //  string TransID = JsonAppleRep.receipt.in_app[0].transaction_id;

                        //  Console.WriteLine(AppleStatus + " | " + Product_ID + "| TransID : " + TransID);

                        if (AppleStatus == 0)
                        {
                            string Key = Unity.CreateMD5("9377(*)#mst9");
                            string RoleID = _RecipeVerify.RoleID + "";
                            string TransID = Unity.CreateMD5(_RecipeVerify.TransID + "");
                            string TimeBuy = Unity.CreateMD5(DateTime.Now.ToString());
                            string PackageName = Product_ID;
                            string Sing = Unity.CreateMD5(PackageName + RoleID + TimeBuy + TransID);
                            string ActiveCardMonth = "0";

                            using (var db = new KiemTheDbEntities())
                            {
                                var FindServer = db.ServerLists.Where(x => x.nServerID == _RecipeVerify.ServerID).FirstOrDefault();
                                if (FindServer != null)
                                {
                                    string IP = FindServer.strURL;

                                    int PORT = (int)FindServer.HttpServicePort;

                                    string HttpRequest = "http://" + IP + ":" + PORT + "/?Rechage=true&KeyStore=" + Key + "&RoleID=" + RoleID + "&TransID=" + TransID + "&PackageName=" + PackageName + "&Sing=" + Sing + "&TimeBuy=" + TimeBuy + "&ActiveCardMonth=" + ActiveCardMonth;

                                    LogManager.WriteLog(LogTypes.SQL, "HTTP URL :" + HttpRequest);

                                    try
                                    {
                                        WebClient wc = new WebClient();
                                        wc.DownloadString(HttpRequest);
                                        _DEMOREP.Msg = "Xác thực giao dịch thành công!";
                                        _DEMOREP.ProductBuy = Product_ID;
                                        _DEMOREP.Status = 0;
                                    }
                                    catch (Exception ex)
                                    {
                                        LogManager.WriteLog(LogTypes.SQL, "HTTP URL :" + ex.ToString());
                                        _DEMOREP.Msg = "Xác thực lỗi";
                                        _DEMOREP.ProductBuy = Product_ID;
                                        _DEMOREP.Status = -1;
                                    }
                                }
                            }
                        }
                        else
                        {
                            _DEMOREP.Msg = "Xác thực lỗi";
                            _DEMOREP.ProductBuy = Product_ID;
                            _DEMOREP.Status = -1;
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogManager.WriteLog(LogTypes.SQL, "VERIFY BUG :" + ex.ToString());
                }
            }

            LogManager.WriteLog(LogTypes.SQL, "================================================START============================================" + length);
            LogManager.WriteLog(LogTypes.SQL, "_RecipeVerify:" + _RecipeVerify.PurchaseData + "| RoleID :" + _RecipeVerify.RoleID + " | UserId :" + _RecipeVerify.UserToken + "| TransID :" + _RecipeVerify.TransID);
            LogManager.WriteLog(LogTypes.SQL, "================================================END============================================");

            byte[] array2 = DataHelper.ObjectToBytes<PaymentVerifyRep>(_DEMOREP);
            base.Response.OutputStream.Write(array2, 0, array2.Length);
            base.Response.OutputStream.Flush();
        }
    }
}