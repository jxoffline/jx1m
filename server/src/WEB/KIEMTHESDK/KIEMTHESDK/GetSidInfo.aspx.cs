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
    public partial class GetSidInfo : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
           
            ServerVerifySIDData serverVerifySIDData = new ServerVerifySIDData();
            serverVerifySIDData.strPlatformUserID = 1102 + "_" + "thanh06";
            serverVerifySIDData.strAccountName = "thanh06";
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

            byte[] array2 = DataHelper.ObjectToBytes<ServerVerifySIDData>(serverVerifySIDData);
            base.Response.OutputStream.Write(array2, 0, array2.Length);
            base.Response.OutputStream.Flush();
        }
    }
}