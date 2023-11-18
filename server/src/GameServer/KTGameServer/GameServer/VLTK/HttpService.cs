using GameServer.KiemThe.Core.Rechage;
using GameServer.KiemThe.Logic;
using GameServer.Logic;
using Server.Tools;
using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;

namespace GameServer.VLTK
{
    public class HttpService
    {
        private BackgroundWorker WebAPIService;

        public void Start()
        {
            SysConOut.WriteLine("BackgroundWorker WEBAPI Start");
            WebAPIService = new BackgroundWorker();
            WebAPIService.DoWork += LoadWebApi_DoWork;
            SysConOut.WriteLine("Server is Started..");
            WebAPIService.RunWorkerAsync();
        }

        private void LoadWebApi_DoWork(object sender, EventArgs e)
        {
            HttpListener web = new HttpListener();

            web.Prefixes.Add("http://" + Program.GetLocalIPAddress() + ":" + GameManager.HttpServiceCode + "/");
            string msg = "OK";

            web.Start();

            SysConOut.WriteLine("HTTP Service Bring : " + Program.GetLocalIPAddress() + ":" + GameManager.HttpServiceCode);
            while (true)
            {
                try
                {
                    HttpListenerContext context = web.GetContext();

                    if (context.Request.QueryString["Rechage"] != null && context.Request.QueryString["KeyStore"] != null)
                    {
                        string MD5Key = RechageServiceManager.MakeMD5Hash(KT_TCPHandler.WebKey);

                        string KeyStore = context.Request.QueryString["KeyStore"].ToString().Trim();

                        if (KeyStore == MD5Key)
                        {
                            string RoleID = context.Request.QueryString["RoleID"].ToString().Trim();

                            string TransID = context.Request.QueryString["TransID"].ToString().Trim();

                            string PackageName = context.Request.QueryString["PackageName"].ToString().Trim();

                            string Sing = context.Request.QueryString["Sing"].ToString().Trim();

                            string TimeBuy = context.Request.QueryString["TimeBuy"].ToString().Trim();

                            string ActiveCardMonth = context.Request.QueryString["ActiveCardMonth"].ToString().Trim();

                            bool IsOK = false;

                            if (ActiveCardMonth == "1")
                            {
                                IsOK = true;
                            }

                            RechageModel _Rechage = new RechageModel();

                            _Rechage.RoleID = Int32.Parse(RoleID);
                            _Rechage.TransID = TransID;
                            _Rechage.TimeBuy = TimeBuy;
                            _Rechage.PackageName = PackageName;
                            _Rechage.Sing = Sing;
                            _Rechage.ActiveCardMonth = IsOK;

                            LogManager.WriteLog(LogTypes.Rechage, "[PackageName] Add yêu cầu mua gói :" + PackageName + "|RoleId :" + RoleID);

                            RechageServiceManager.ReachageData.Add(_Rechage);
                        }
                    }

                    if (context.Request.QueryString["CcuOnline"] != null && context.Request.QueryString["KeyStore"] != null)
                    {
                        string MD5Key = RechageServiceManager.MakeMD5Hash(KT_TCPHandler.WebKey);

                        string KeyStore = context.Request.QueryString["KeyStore"].ToString().Trim();

                        if (KeyStore == MD5Key)
                        {
                            msg = KTPlayerManager.GetPlayersCount() + "";
                        }
                    }

                    if (context.Request.QueryString["SendNotify"] != null && context.Request.QueryString["KeyStore"] != null)
                    {
                        string MD5Key = RechageServiceManager.MakeMD5Hash(KT_TCPHandler.WebKey);

                        string KeyStore = context.Request.QueryString["KeyStore"].ToString().Trim();

                        if (KeyStore == MD5Key)
                        {
                            string MSGENCODE = context.Request.QueryString["Msg"].ToString().Trim();

                            string Notify = DataHelper.DecodeBase64(MSGENCODE);

                            KTGMCommandManager.SendSystemEventNotification(Notify);
                        }
                    }

                    if (context.Request.QueryString["ShutDownServer"] != null && context.Request.QueryString["KeyStore"] != null)
                    {
                        string MD5Key = RechageServiceManager.MakeMD5Hash(KT_TCPHandler.WebKey);

                        string KeyStore = context.Request.QueryString["KeyStore"].ToString().Trim();

                        if (KeyStore == MD5Key)
                        {
                            Thread _Thread = new Thread(() => Program.OnExitServer());
                            _Thread.Start();
                        }
                    }
                    HttpListenerResponse response = context.Response;

                    byte[] buffer = Encoding.UTF8.GetBytes(msg);
                    response.ContentLength64 = buffer.Length;
                    Stream st = response.OutputStream;
                    st.Write(buffer, 0, buffer.Length);
                    context.Response.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
        }
    }
}