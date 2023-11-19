using KIEMTHESDK.Database;
using KIEMTHESDK.Models;
using Server.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;

namespace KIEMTHESDK
{
    public partial class TestAndroid : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            BuffServerListDataEx buffServerListDataEx = new BuffServerListDataEx();
            List<BuffServerInfo> list = new List<BuffServerInfo>();
            BuffServerInfo lastServerLogin = new BuffServerInfo();
            string text = base.Request.Form["strUID"];
            int UserId = int.Parse(text);
            LogManager.WriteLog(LogTypes.Error, "USER :" + text + " | Request ServerList");
            using (KiemTheDbEntities kiemTheDbEntities = new KiemTheDbEntities())
            {
                List<ServerList> list2 = kiemTheDbEntities.ServerLists.Where(x => x.isTestServer == 1).ToList();

                foreach (ServerList serverList in list2)
                {
                    list.Add(new BuffServerInfo
                    {
                        nOnlineNum = serverList.nOnlineNum.Value,
                        nServerID = serverList.nServerID.Value,
                        nServerOrder = serverList.nServerOrder.Value,
                        nServerPort = serverList.nServerPort.Value,
                        strServerName = serverList.strServerName,
                        nStatus = serverList.nStatus.Value,
                        strMaintainStarTime = serverList.strMaintainStarTime.ToString(),
                        strMaintainTerminalTime = serverList.strMaintainTerminalTime.ToString(),
                        strMaintainTxt = serverList.strMaintainTxt,
                        strURL = serverList.strURL,
                        Msg = "<color=green>Exp:X1 | Money:X1</color>",
                    });
                }

                LoginTable loginTable = (from x in kiemTheDbEntities.LoginTables
                                         where x.ID == UserId
                                         select x).FirstOrDefault<LoginTable>();
                bool flag = loginTable != null;
                if (flag)
                {
                    bool flag2 = loginTable.LastServerLogin != null;
                    if (flag2)
                    {
                        int LastLogin = loginTable.LastServerLogin.Value;
                        ServerList serverList2 = (from x in kiemTheDbEntities.ServerLists
                                                  where x.nServerID == (int?)LastLogin
                                                  select x).FirstOrDefault<ServerList>();
                        bool flag3 = serverList2 != null;
                        if (flag3)
                        {
                            lastServerLogin = new BuffServerInfo
                            {
                                nOnlineNum = serverList2.nOnlineNum.Value,
                                nServerID = serverList2.nServerID.Value,
                                nServerOrder = serverList2.nServerOrder.Value,
                                nServerPort = serverList2.nServerPort.Value,
                                strServerName = serverList2.strServerName,
                                nStatus = serverList2.nStatus.Value,
                                strMaintainStarTime = serverList2.strMaintainStarTime.ToString(),
                                strMaintainTerminalTime = serverList2.strMaintainTerminalTime.ToString(),
                                strMaintainTxt = serverList2.strMaintainTxt,
                                Msg = "<color=green>Exp:X1 | Money:X1</color>",
                                strURL = serverList2.strURL
                            };
                        }
                    }
                }
            }
            buffServerListDataEx.ListServerData = list;
            buffServerListDataEx.RecommendListServerData = list;
            buffServerListDataEx.LastServerLogin = lastServerLogin;
            byte[] array = DataHelper.ObjectToBytes<BuffServerListDataEx>(buffServerListDataEx);
            base.Response.OutputStream.Write(array, 0, array.Length);
            base.Response.OutputStream.Flush();
        }
    }
}