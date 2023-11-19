using KIEMTHESDK.Database;
using KIEMTHESDK.Models;
using Server.Tools;
using System;
using System.Data.Entity.Migrations;
using System.Linq;

namespace KIEMTHESDK
{
    public partial class LoginSDK : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            LoginRep loginRep = new LoginRep();
            Unity unity = new Unity();
            string UserName = base.Request.Form["UserName"];
            string text = base.Request.Form["Password"];
            string text2 = base.Request.Form["LoginType"];

            string ip = Request.UserHostAddress;

            LogManager.WriteLog(LogTypes.Info, "[" + ip + "]LOGIN NAME :" + UserName + "");

            if (string.IsNullOrEmpty(UserName) || string.IsNullOrEmpty(text))
            {
                loginRep.AccessToken = "";
                loginRep.ErrorCode = -10;
                loginRep.ErorrMsg = "Tên đăng nhập không hợp lệ";
            }
            else
            {
                LogManager.WriteLog(LogTypes.Info, string.Concat(new string[]
                {
                    "Request Login : UserName ",
                    UserName,
                    " | Password : ",
                    text,
                    " | LoginType :",
                    text2
                }));
                bool flag2 = !Unity.HasSpecialChar(UserName);
                if (flag2)
                {
                    loginRep.AccessToken = "";
                    loginRep.ErrorCode = -10;
                    loginRep.ErorrMsg = "Tên đăng nhập chứa ký tự đặc biệt";
                }
                else
                {

                    int Random = new Random().Next(0, 100);

                    //if (Random < 50)
                    //{
                    //    LogManager.WriteLog(LogTypes.Info, "[" + ip + "]LOGIN NAME :" + UserName + "====> SAI PASSS");
                    //    loginRep.AccessToken = "";
                    //    loginRep.ErrorCode = -2;
                    //    loginRep.ErorrMsg = "Hệ thống đang bận vui lòng thử lại sau!";

                    //}
                    //else
                    {
                        //loginRep = Unity.ProseccLogin(UserName, text);

                        using (KiemTheDbEntities kiemTheDbEntities = new KiemTheDbEntities())
                        {
                            string MD5 = Unity.CreateMD5(text);

                            LoginTable loginTable = kiemTheDbEntities.LoginTables.Where(x => x.LoginName == UserName && x.Password == MD5).FirstOrDefault();

                            bool flag3 = loginTable != null;
                            if (flag3)
                            {
                                bool flag4 = loginTable.Status == 0;
                                if (flag4)
                                {
                                    string accessToken = Unity.TokenGen(UserName);
                                    loginRep.AccessToken = accessToken;
                                    loginRep.ErrorCode = 0;
                                    loginRep.ErorrMsg = "Đăng nhập thành công!";
                                    loginRep.LastLoginIP = loginTable.LastIPLogin;
                                    loginRep.LastLoginTime = loginTable.LastLoginTime.ToString();
                                    loginTable.AccessToken = accessToken;
                                    loginTable.TokenTimeExp = new DateTime?(DateTime.Now.AddDays(2.0));
                                    loginTable.LastLoginTime = new DateTime?(DateTime.Now);
                                    loginTable.LastIPLogin = Unity.GetIPAddress();

                                    kiemTheDbEntities.LoginTables.AddOrUpdate(loginTable);
                                    kiemTheDbEntities.SaveChanges();

                                    LogManager.WriteLog(LogTypes.Info, "[" + ip + "]LOGIN OKKK :" + UserName + " ASSCESSTOKEN CHANGE :" + accessToken);
                                }
                                else
                                {
                                    loginRep.AccessToken = "";
                                    loginRep.ErrorCode = -3;
                                    loginRep.ErorrMsg = "Tài khoản đang bị khóa Lý do :" + loginTable.Note;
                                }
                            }
                            else
                            {
                                LogManager.WriteLog(LogTypes.Info, "[" + ip + "]LOGIN NAME :" + UserName + "====> SAI PASSS");
                                loginRep.AccessToken = "";
                                loginRep.ErrorCode = -2;
                                loginRep.ErorrMsg = "Sai tên đăng nhập hoặc mật khẩu!";
                            }
                        }
                    }
                }
            }

            byte[] array = DataHelper.ObjectToBytes<LoginRep>(loginRep);
            base.Response.OutputStream.Write(array, 0, array.Length);
            base.Response.OutputStream.Flush();
        }
    }
}