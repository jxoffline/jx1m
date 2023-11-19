using KIEMTHESDK.Database;
using KIEMTHESDK.Models;
using Server.Tools;
using System;
using System.Linq;

namespace KIEMTHESDK
{
    public partial class PaymentCreate : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            PaymentRequestRep _DEMOREP = new PaymentRequestRep();

            byte[] array = base.Request.BinaryRead(base.Request.TotalBytes);
            int length = array.Length;

            PaymentRequest _GoogleRequest = DataHelper.BytesToObject<PaymentRequest>(array, 0, length);

            LogManager.WriteLog(LogTypes.Error, "================================================START============================================" + length);
            LogManager.WriteLog(LogTypes.Error, " _GoogleRequest.RoleID: " + _GoogleRequest.RoleID + "| _GoogleRequest.UserId :" + _GoogleRequest.UserToken + "| TransID :" + _GoogleRequest.TransID + "| PackageName :" + _GoogleRequest.PackageName + "| ServerID :" + _GoogleRequest.ServerID + "| DeviceID :" + _GoogleRequest.DeviceID + "|PLATFORM : " + _GoogleRequest.PlatForm);
            LogManager.WriteLog(LogTypes.Error, "================================================END============================================");

          

            string MD5TransID = Unity.CreateMD5(_GoogleRequest.RoleID + _GoogleRequest.PackageName + DateTime.Now.ToString());

            using (var db = new KiemTheDbEntities())
            {
                var FindLogin = db.LoginTables.Where(x => x.AccessToken == _GoogleRequest.UserToken).FirstOrDefault();
                if (FindLogin == null)
                {
                    _DEMOREP.Msg = "Không tìm thấy User";
                    _DEMOREP.ProductBuy = _GoogleRequest.PackageName;
                    _DEMOREP.Status = -1;
                    _DEMOREP.TransID = MD5TransID;
                }
                else
                {
                    //RechageLog _Rechage = new RechageLog();

                    //_Rechage.ActionBy = "USER";
                    //_Rechage.AfterCoin = 0;
                    //_Rechage.BeforeCoin = 0;
                    //_Rechage.CoinValue = 0;
                    //_Rechage.Messenger = "Khởi tạo giao dịch GOOGLEPLAY :" + _GoogleRequest.PackageName;
                    //_Rechage.Pram_0 = "GOOGLE";
                    //_Rechage.Pram_1 = "";
                    //_Rechage.Pram_2 = "";
                    //_Rechage.Pram_3 = 0;
                    //_Rechage.RechageDate = DateTime.Now;
                    //_Rechage.RechageType = "GOOGLE";
                    //_Rechage.Status = 0;
                    //_Rechage.TransID = MD5TransID;
                    //_Rechage.UserID = FindLogin.ID;
                    //_Rechage.UserName = FindLogin.LoginName;
                    //_Rechage.ValueRechage = 0;

                    //db.RechageLogs.Add(_Rechage);
                    //db.SaveChanges();

                    //_DEMOREP.Msg = "Khởi tạo giao dịch thành công";
                    //_DEMOREP.ProductBuy = _GoogleRequest.PackageName;
                    //_DEMOREP.Status = 0;
                    //_DEMOREP.TransID = MD5TransID;

                    _DEMOREP.Msg = "Kênh thanh toán thông qua STORE hiện đang bảo trì";
                    _DEMOREP.ProductBuy = _GoogleRequest.PackageName;
                    _DEMOREP.Status = -1;
                    _DEMOREP.TransID = MD5TransID;
                }
            }

            byte[] array2 = DataHelper.ObjectToBytes<PaymentRequestRep>(_DEMOREP);
            base.Response.OutputStream.Write(array2, 0, array2.Length);
            base.Response.OutputStream.Flush();
        }
    }
}