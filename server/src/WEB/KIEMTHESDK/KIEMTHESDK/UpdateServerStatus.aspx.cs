using KIEMTHESDK.Database;
using KIEMTHESDK.Models;
using Server.Tools;
using System;
using System.Data.Entity.Migrations;
using System.Linq;

namespace KIEMTHESDK
{
    public partial class UpdateServerStatus : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                byte[] array = base.Request.BinaryRead(base.Request.TotalBytes);
                int length = array.Length;

                string IPCLIENT = Unity.GetIPAddress();

                UpdateServerModel _UpdateServerStatus = DataHelper.BytesToObject<UpdateServerModel>(array, 0, length);

                using (var db = new KiemTheDbEntities())
                {
                    var Find = db.ServerLists.Where(x => x.nServerID == _UpdateServerStatus.SeverID && x.strURL == IPCLIENT).FirstOrDefault();
                    if (Find != null)
                    {
                        Find.nStatus = _UpdateServerStatus.Status;
                        Find.strMaintainTxt = _UpdateServerStatus.NotifyUpdate;

                        db.ServerLists.AddOrUpdate(Find);
                        db.SaveChanges();
                    }
                }
            }
            catch
            {
            }
        }
    }
}