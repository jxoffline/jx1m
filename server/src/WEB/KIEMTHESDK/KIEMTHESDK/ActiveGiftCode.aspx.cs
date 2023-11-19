using KIEMTHESDK.Database;
using KIEMTHESDK.Models;
using Server.Tools;
using System;
using System.Linq;

namespace KIEMTHESDK
{
    public partial class ActiveGiftCode : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            byte[] array = base.Request.BinaryRead(base.Request.TotalBytes);
            int length = array.Length;

            LogManager.WriteLog(LogTypes.SQL, "ACTIVE GIFTCODE KICK :" + length);
            GiftCodeRep _CodeRep = new GiftCodeRep();

            GiftCodeRequest GiftCodeRequest = DataHelper.BytesToObject<GiftCodeRequest>(array, 0, length);

           // LogManager.WriteLog(LogTypes.SQL, "1111");

            if (GiftCodeRequest != null)
            {
                try
                {
                   // LogManager.WriteLog(LogTypes.SQL, "2222");
                    using (var db = new KiemTheDbEntities())
                    {
                        var find = db.GiftCodes.Where(x => x.Code == GiftCodeRequest.CodeActive).FirstOrDefault();

                      //  LogManager.WriteLog(LogTypes.SQL, "23333");
                        if (find != null)
                        {
                            //LogManager.WriteLog(LogTypes.SQL, "444444");

                            bool IsBand = false;
                            // Nếu code này có check máy chủ
                            if (find.ServerID != 0)
                            {
                               // LogManager.WriteLog(LogTypes.SQL, "55555");
                                int ServerKick = GiftCodeRequest.ServerID;

                                if (find.ServerID != ServerKick)
                                {
                                    //LogManager.WriteLog(LogTypes.SQL, "666666");
                                    _CodeRep.Msg = "Mã quà tặng không thể sử dụng tại máy chủ này";
                                    _CodeRep.Status = -1;
                                    IsBand = true;
                                }
                            }

                            if (!IsBand)
                            {
                               // LogManager.WriteLog(LogTypes.SQL, "77777");
                                // Lấy ra số lượt kích hoạt tối đa
                                int MaxActive = (int)find.MaxActive;

                                int CountActive = db.GiftCodeLogs.Where(x => x.Code == GiftCodeRequest.CodeActive).Count();

                               // LogManager.WriteLog(LogTypes.SQL, "888888");
                                if (CountActive >= MaxActive)
                                {
                                  //  LogManager.WriteLog(LogTypes.SQL, "999999");
                                    // LogManager.WriteLog(LogTypes.Info, "UserName :" + identityName + " | Gifcode đã hết lượt kích hoạt" + serverId);

                                    _CodeRep.Msg = "Mã quà tặng đã hết lượt kích hoạt";
                                    _CodeRep.Status = -1;
                                }
                                else
                                {
                                    //LogManager.WriteLog(LogTypes.SQL, "10000");
                                    string CodeType = find.CodeType;

                                    var FindActiveType = db.GiftCodeLogs.Where(x => x.CodeType == CodeType && x.ActiveRole == GiftCodeRequest.RoleActive && x.ServerID == GiftCodeRequest.ServerID).FirstOrDefault();
                                    if (FindActiveType != null)
                                    {
                                      //  LogManager.WriteLog(LogTypes.SQL, "1112121");
                                        _CodeRep.Msg = "Mỗi loại code chỉ được kích hoạt 1 lần";
                                        _CodeRep.Status = -1;
                                    }
                                    else
                                    {
                                       // LogManager.WriteLog(LogTypes.SQL, "122222");
                                        _CodeRep.Msg = "Kích hoạt thành công";
                                        _CodeRep.Status = 0;
                                        _CodeRep.GiftItem = find.ItemList;

                                        GiftCodeLog _Logs = new GiftCodeLog();
                                        _Logs.ActiveRole = GiftCodeRequest.RoleActive;
                                        _Logs.ActiveTime = DateTime.Now;
                                        _Logs.Code = GiftCodeRequest.CodeActive;
                                        _Logs.CodeType = find.CodeType;
                                        _Logs.ServerID = GiftCodeRequest.ServerID;

                                        db.GiftCodeLogs.Add(_Logs);
                                        db.SaveChanges();
                                       // LogManager.WriteLog(LogTypes.SQL, "34343");
                                    }
                                }
                            }
                        }
                        else
                        {
                            _CodeRep.Msg = "Mã quà tặng không tồn tại";
                            _CodeRep.Status = -1;
                        }
                    }
                }
                catch(Exception ex)
                {
                    LogManager.WriteLog(LogTypes.SQL, "ACTIVE BUG :" + ex.ToString());

                }
            }
            else
            {
                _CodeRep.Msg = "Dữ liệu truyền lên sai định dạng";
                _CodeRep.Status = -1;
            }

            //Trả về client
            byte[] ResponCode = DataHelper.ObjectToBytes<GiftCodeRep>(_CodeRep);
            base.Response.OutputStream.Write(ResponCode, 0, ResponCode.Length);
            base.Response.OutputStream.Flush();
        }
    }
}