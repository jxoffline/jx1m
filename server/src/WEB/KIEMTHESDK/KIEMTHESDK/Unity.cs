using KIEMTHESDK.Database;
using KIEMTHESDK.Models;
using Server.Tools;
using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace KIEMTHESDK
{
    public class Unity
    {
        public static bool IsProseccsing = false;
        public static List<LoginTable> CacheLoginTable = new List<LoginTable>();

        /// <summary>
        ///
        /// </summary>
        public static void Setup()
        {
            IsProseccsing = true;
            lock (CacheLoginTable)
            {
                CacheLoginTable.Clear();

                using (var db = new KiemTheDbEntities())
                {
                    CacheLoginTable = db.LoginTables.ToList().ToList();
                }

                LogManager.WriteLog(LogTypes.SQL, "CACHE ACCOUNT :" + CacheLoginTable.Count);
            }


            IsProseccsing = false;


            // LoginRep _Login = ProseccLogin("thanh101", "Thanh06ht@");

            //  Console.WriteLine(_Login);
        }

        public static ServerVerifySIDData VerifyAccount(string AccessToken)
        {
            ServerVerifySIDData serverVerifySIDData = new ServerVerifySIDData();

            // lock (CacheLoginTable)
            {
                if (!IsProseccsing)
                {
                    LoginTable loginTable = CacheLoginTable.Where(x => x.AccessToken == AccessToken).FirstOrDefault();

                    bool flag2 = loginTable != null;
                    if (flag2)
                    {
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
                else
                {
                    serverVerifySIDData.strPlatformUserID = "-1";
                    serverVerifySIDData.strAccountName = "";
                    serverVerifySIDData.strCM = "Hệ thống đang bận vui lòng thử lại sau";
                    serverVerifySIDData.strToken = "";
                    serverVerifySIDData.lTime = 0L;
                }
            }

            return serverVerifySIDData;
        }

        public static LoginRep ProseccLogin(string UserName, string Password)
        {
            LoginRep _Login = new LoginRep();
            try
            {
                if (!IsProseccsing)
                {
                    string MD5 = Unity.CreateMD5(Password);

                    //lock (CacheLoginTable)
                    {
                        var find = CacheLoginTable.Where(x => x.LoginName.ToLower() == UserName.ToLower() && x.Password.ToLower() == MD5.ToLower()).FirstOrDefault();
                        if (find != null)
                        {
                            if (find.Status == 0)
                            {
                                if (find.AccessToken.Length > 35)
                                {
                                    _Login.AccessToken = find.AccessToken;
                                    _Login.ErrorCode = 0;
                                    _Login.ErorrMsg = "Đăng nhập thành công!";
                                    _Login.LastLoginIP = find.LastIPLogin;
                                    _Login.LastLoginTime = find.LastLoginTime.ToString();
                                }
                                else
                                {
                                    string accessToken = Unity.TokenGen(UserName);
                                    _Login.ErrorCode = 0;
                                    _Login.ErorrMsg = "Đăng nhập thành công!";
                                    _Login.LastLoginIP = find.LastIPLogin;
                                    _Login.LastLoginTime = find.LastLoginTime.ToString();
                                    //UPDATE TOKEN
                                    find.AccessToken = accessToken;
                                    using (var db = new KiemTheDbEntities())
                                    {
                                        var findupdate = db.LoginTables.Where(x => x.LoginName == UserName).FirstOrDefault();
                                        if (findupdate != null)
                                        {
                                            findupdate.AccessToken = accessToken;
                                            db.LoginTables.AddOrUpdate(findupdate);
                                            db.SaveChanges();
                                        }
                                    }
                                }
                            }
                            else
                            {
                                _Login.AccessToken = "";
                                _Login.ErrorCode = -1;
                                _Login.ErorrMsg = "Tài khoản đã bị khóa!";
                            }
                        }
                        else
                        {
                            _Login.AccessToken = "";
                            _Login.ErrorCode = -2;
                            _Login.ErorrMsg = "Sai tên đăng nhập hoặc mật khẩu!";
                        }
                    }
                }
                else
                {
                    _Login.AccessToken = "";
                    _Login.ErrorCode = -4;
                    _Login.ErorrMsg = "Hệ thống bận vui lòng quay lại trong ít phút!";
                }
            }
            catch (Exception ex)
            {
                _Login.AccessToken = "";
                _Login.ErrorCode = -4;
                _Login.ErorrMsg = "Hệ thống bận vui lòng quay lại trong ít phút!";
            }
            return _Login;
        }

        // Token: 0x0600001E RID: 30 RVA: 0x0000384C File Offset: 0x00001A4C
        public static string RandomString(int length)
        {
            return new string((from s in Enumerable.Repeat<string>("ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789", length)
                               select s[Unity.random.Next(s.Length)]).ToArray<char>());
        }

        public static bool Verify(string message, string base64Signature, string xmlPublicKey)
        {
            // Create the provider and load the KEY
            RSACryptoServiceProvider provider = new RSACryptoServiceProvider();
            provider.FromXmlString(xmlPublicKey);

            // The signature is supposed to be encoded in base64 and the SHA1 checksum
            // of the message is computed against the UTF-8 representation of the message
            byte[] signature = System.Convert.FromBase64String(base64Signature);
            SHA1Managed sha = new SHA1Managed();
            byte[] data = System.Text.Encoding.UTF8.GetBytes(message);

            return provider.VerifyData(data, sha, signature);
        }

        // Token: 0x0600001F RID: 31 RVA: 0x00003884 File Offset: 0x00001A84
        public bool IsNumeric(string s)
        {
            foreach (char c in s)
            {
                bool flag = !char.IsDigit(c) && c != '.';
                if (flag)
                {
                    return false;
                }
            }
            return true;
        }

        // Token: 0x06000020 RID: 32 RVA: 0x000038D4 File Offset: 0x00001AD4
        public static string GetIPAddress()
        {
            HttpContext httpContext = HttpContext.Current;
            string text = httpContext.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            bool flag = !string.IsNullOrEmpty(text);
            if (flag)
            {
                string[] array = text.Split(new char[]
                {
                    ','
                });
                bool flag2 = array.Length != 0;
                if (flag2)
                {
                    return array[0];
                }
            }
            return httpContext.Request.ServerVariables["REMOTE_ADDR"];
        }
      
        public static bool HasSpecialChar(string text)
        {
            bool check = false;
            if (System.Text.RegularExpressions.Regex.IsMatch(text, "^[a-zA-Z0-9\x20]+$"))
            {
                check = true;
            }
            if (text.Contains(" "))
            {
                check = false;
            }
            return check;
        }
        // Token: 0x06000021 RID: 33 RVA: 0x00003950 File Offset: 0x00001B50
   

        // Token: 0x06000022 RID: 34 RVA: 0x0000399C File Offset: 0x00001B9C
        public static string TokenGen(string UserName)
        {
            string AddMore = Unity.CreateMD5(DateTime.Now.ToString() + RandomString(10) + UserName);

            string SUBSTRING = AddMore.Substring(0, 10);

            return Unity.CreateMD5(DateTime.Now.ToString() + RandomString(4) + UserName) + SUBSTRING;
        }

        // Token: 0x06000023 RID: 35 RVA: 0x000039D0 File Offset: 0x00001BD0
        public static string CreateMD5(string input)
        {
            string result;
            using (MD5 md = MD5.Create())
            {
                byte[] bytes = Encoding.ASCII.GetBytes(input);
                byte[] array = md.ComputeHash(bytes);
                StringBuilder stringBuilder = new StringBuilder();
                for (int i = 0; i < array.Length; i++)
                {
                    stringBuilder.Append(array[i].ToString("X2"));
                }
                result = stringBuilder.ToString();
            }
            return result;
        }

        // Token: 0x04000009 RID: 9
        private static readonly Random random = new Random();
    }
}