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
    public partial class GetUserServerList : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
			BuffServerListDataOld buffServerListDataOld = new BuffServerListDataOld();
			List<BuffServerInfo> list = new List<BuffServerInfo>();
			BuffServerInfo buffServerInfo = new BuffServerInfo();
			using (KiemTheDbEntities kiemTheDbEntities = new KiemTheDbEntities())
			{
				List<ServerList> list2 = (from x in kiemTheDbEntities.ServerLists
										  orderby x.nServerOrder descending
										  select x).ToList<ServerList>();
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
						strURL = serverList.strURL
					});
				}
			}
			buffServerListDataOld.ListServerData = list;
			buffServerListDataOld.RecommendListServerData = list;
			byte[] array = DataHelper.ObjectToBytes<BuffServerListDataOld>(buffServerListDataOld);
			base.Response.OutputStream.Write(array, 0, array.Length);
			base.Response.OutputStream.Flush();
		}
    }
}