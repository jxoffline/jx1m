using KIEMTHESDK.Database;
using KIEMTHESDK.Models;
using Server.Tools;
using System;
using System.Linq;

namespace KIEMTHESDK
{
    public partial class RegisterSDK : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            RegisterRep registerRep = new RegisterRep();
            Unity unity = new Unity();
            string UserName = base.Request.Form["UserName"];
            string input = base.Request.Form["Password"];
            string text = base.Request.Form["Email"];
            string text2 = base.Request.Form["PhoneNumber"];
            registerRep.ErorrMsg = "Chức năng đang tạm đóng";
            registerRep.ErrorCode = -1;
            bool flag = !Unity.HasSpecialChar(UserName);
            if (flag)
            {
                registerRep.ErrorCode = -1;
                registerRep.ErorrMsg = "Tên đăng nhập không được chứa ký tự đặc biệt";
            }
            else
            {
                bool flag2 = UserName.Length >= 6 && UserName.Length <= 12;
                if (flag2)
                {
                    bool flag3 = (text2.Length == 10 || text2.Length == 11) && unity.IsNumeric(text2);
                    if (flag3)
                    {
                        bool flag4 = text.Contains("@");
                        if (flag4)
                        {
                            if (input.Length < 8)
                            {
                                registerRep.ErrorCode = -1;
                                registerRep.ErorrMsg = "Mật khẩu phải tối thiểu 8 ký tự có chữ hoa và chữ thường";
                            }
                            else
                            {
                                string password = Unity.CreateMD5(input);
                                using (KiemTheDbEntities kiemTheDbEntities = new KiemTheDbEntities())
                                {
                                    LoginTable loginTable = (from x in kiemTheDbEntities.LoginTables
                                                             where x.LoginName == UserName
                                                             select x).FirstOrDefault<LoginTable>();
                                    bool flag5 = loginTable != null;
                                    if (flag5)
                                    {
                                        registerRep.ErrorCode = -1;
                                        registerRep.ErorrMsg = "Tên đăng nhập đã có người sử dụng.Vui lòng chọn tên khác!";
                                    }
                                    else
                                    {
                                        LoginTable loginTable2 = new LoginTable();
                                        loginTable2.AccessToken = "";
                                        loginTable2.ActiveRoleID = new int?(0);
                                        loginTable2.ActiveRoleName = "";
                                        loginTable2.Date = new DateTime?(DateTime.Now);
                                        loginTable2.Email = text;
                                        loginTable2.FullName = "";
                                        loginTable2.LastIPLogin = "";
                                        loginTable2.LastLoginTime = new DateTime?(DateTime.Now);
                                        loginTable2.LastServerLogin = new int?(0);
                                        loginTable2.LoginName = UserName;
                                        loginTable2.Password = password;
                                        loginTable2.Phone = text2;
                                        loginTable2.Status = new int?(0);
                                        loginTable2.TokenTimeExp = new DateTime?(DateTime.Now);
                                        kiemTheDbEntities.LoginTables.Add(loginTable2);
                                        kiemTheDbEntities.SaveChanges();
                                        registerRep.ErrorCode = 0;
                                        registerRep.ErorrMsg = "Đăng ký tài khoản thành công!";
                                    }
                                }
                            }
                        }
                        else
                        {
                            registerRep.ErrorCode = -1;
                            registerRep.ErorrMsg = "Email không hợp lệ!";
                        }
                    }
                    else
                    {
                        registerRep.ErrorCode = -1;
                        registerRep.ErorrMsg = "Số điện thoại không hợp lệ!";
                    }
                }
                else
                {
                    registerRep.ErrorCode = -1;
                    registerRep.ErorrMsg = "Tên đăng nhập phải từ 6 tới 12 ký tự!";
                }
            }
            byte[] array = DataHelper.ObjectToBytes<RegisterRep>(registerRep);
            base.Response.OutputStream.Write(array, 0, array.Length);
            base.Response.OutputStream.Flush();
        }
    }
}