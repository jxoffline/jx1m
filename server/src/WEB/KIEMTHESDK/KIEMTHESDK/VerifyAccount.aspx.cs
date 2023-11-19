using KIEMTHESDK.Database;
using KIEMTHESDK.Models;
using Server.Tools;
using System;
using System.Linq;

namespace KIEMTHESDK
{
    public partial class VerifyAccount : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                string ip = Request.UserHostAddress;

                ServerVerifySIDData serverVerifySIDData = new ServerVerifySIDData();
                byte[] array = base.Request.BinaryRead(base.Request.TotalBytes);
                int length = array.Length;
                ClientVerifySIDData clientVerifySIDData = DataHelper.BytesToObject<ClientVerifySIDData>(array, 0, length);
                bool flag = clientVerifySIDData == null;
                if (flag)
                {
                    serverVerifySIDData.strPlatformUserID = "-10";
                    serverVerifySIDData.strAccountName = "";
                    serverVerifySIDData.strCM = "Sai dữ liệu truyền lên!";
                    serverVerifySIDData.strToken = "";
                    serverVerifySIDData.lTime = 0L;
                }
                else
                {
                    string AccessToken = clientVerifySIDData.strSID;

                    LogManager.WriteLog(LogTypes.Info, "[" + ip + "]GET SID FROM CLIENT :" + AccessToken);

                    //serverVerifySIDData = Unity.VerifyAccount(AccessToken);
                    using (KiemTheDbEntities kiemTheDbEntities = new KiemTheDbEntities())
                    {
                        var loginTable = kiemTheDbEntities.LoginTables.Where(x => x.AccessToken == AccessToken).FirstOrDefault();

                        if (loginTable != null)
                        {
                            LogManager.WriteLog(LogTypes.Info, "[" + ip + "]GET SID FROM CLIENT :" + AccessToken + "===> FIND ACCOUNT :" + loginTable.LoginName + " | PASS : " + loginTable.Password);

                            serverVerifySIDData.strPlatformUserID = loginTable.ID + "_" + loginTable.LoginName;
                            serverVerifySIDData.strAccountName = loginTable.LoginName;
                            serverVerifySIDData.strCM = "1";
                            serverVerifySIDData.lTime = (long)DataHelper.UnixSecondsNow();
                            string strToken = MD5Helper.get_md5_string(string.Concat(new object[]
                            {
                            serverVerifySIDData.strPlatformUserID,
                            serverVerifySIDData.strAccountName,
                            serverVerifySIDData.lTime,
                            serverVerifySIDData.strCM,
                            "9377(*)#mst9"
                            }));
                            serverVerifySIDData.strToken = strToken;
                        }
                        else
                        {
                            serverVerifySIDData.strPlatformUserID = "-1";
                            serverVerifySIDData.strAccountName = "";
                            serverVerifySIDData.strCM = "Sai tài khoản hoặc mật khẩu";
                            serverVerifySIDData.strToken = "";
                            serverVerifySIDData.lTime = 0L;
                        }
                    }
                }
                byte[] array2 = DataHelper.ObjectToBytes<ServerVerifySIDData>(serverVerifySIDData);
                base.Response.OutputStream.Write(array2, 0, array2.Length);
                base.Response.OutputStream.Flush();
            }
            catch (Exception exx)
            {
                LogManager.WriteLog(LogTypes.Error, "GET SID FROM CLIENT :" + exx.ToString());
            }
        }
    }
}