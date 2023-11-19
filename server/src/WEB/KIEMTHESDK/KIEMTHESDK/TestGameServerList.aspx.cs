using KIEMTHESDK.Database;
using KIEMTHESDK.Models;
using Server.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace KIEMTHESDK
{
    public partial class TestGameServerList : System.Web.UI.Page
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
                List<ServerListsIo> list2 = kiemTheDbEntities.ServerListsIos.Where(x => x.isTestServer == 13).ToList();
                foreach (ServerListsIo serverLists_ in list2)
                {
                    list.Add(new BuffServerInfo
                    {
                        nOnlineNum = serverLists_.nOnlineNum.Value,
                        nServerID = serverLists_.nServerID.Value,
                        nServerOrder = serverLists_.nServerOrder.Value,
                        nServerPort = serverLists_.nServerPort.Value,
                        strServerName = serverLists_.strServerName,
                        nStatus = serverLists_.nStatus.Value,
                        strMaintainStarTime = serverLists_.strMaintainStarTime.ToString(),
                        strMaintainTerminalTime = serverLists_.strMaintainTerminalTime.ToString(),
                        strMaintainTxt = serverLists_.strMaintainTxt,
                        strURL = serverLists_.strURL,
                        Msg = serverLists_.strMaintainTxt,
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
                        ServerListsIo serverLists_2 = (from x in kiemTheDbEntities.ServerListsIos
                                                       where x.nServerID == (int?)LastLogin
                                                       select x).FirstOrDefault<ServerListsIo>();
                        bool flag3 = serverLists_2 != null;
                        if (flag3)
                        {
                            lastServerLogin = new BuffServerInfo
                            {
                                nOnlineNum = serverLists_2.nOnlineNum.Value,
                                nServerID = serverLists_2.nServerID.Value,
                                nServerOrder = serverLists_2.nServerOrder.Value,
                                nServerPort = serverLists_2.nServerPort.Value,
                                strServerName = serverLists_2.strServerName,
                                nStatus = serverLists_2.nStatus.Value,
                                strMaintainStarTime = serverLists_2.strMaintainStarTime.ToString(),
                                strMaintainTerminalTime = serverLists_2.strMaintainTerminalTime.ToString(),
                                strMaintainTxt = serverLists_2.strMaintainTxt,
                                Msg = serverLists_2.strMaintainTxt,
                                strURL = serverLists_2.strURL
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