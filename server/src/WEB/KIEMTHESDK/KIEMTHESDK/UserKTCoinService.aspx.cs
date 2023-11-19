using KIEMTHESDK.Database;
using KIEMTHESDK.Models;
using Server.Tools;
using System;
using System.Data.Entity.Migrations;
using System.Linq;

namespace KIEMTHESDK
{
    public partial class UserKTCoinService : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                byte[] array = base.Request.BinaryRead(base.Request.TotalBytes);
                int length = array.Length;

                KTCoinResponse _CodeRep = new KTCoinResponse();

                KTCoinRequest _KTCoinRequest = DataHelper.BytesToObject<KTCoinRequest>(array, 0, length);

                if (_KTCoinRequest != null)
                {
                    using (var db = new KiemTheDbEntities())
                    {
                        if (_KTCoinRequest.Type == 1)
                        {
                            var findCoin = db.KTCoins.Where(x => x.UserID == _KTCoinRequest.UserID).FirstOrDefault();
                            if (findCoin != null)
                            {
                                _CodeRep.Status = 0;
                                _CodeRep.Msg = "Truy vấn thành công";
                                _CodeRep.Value = (int)findCoin.KCoin;
                            }
                            else
                            {
                                _CodeRep.Status = 0;
                                _CodeRep.Msg = "Không tìm thấy tài khoản";
                                _CodeRep.Value = 0;
                            }
                        }
                        else if (_KTCoinRequest.Type == 2)
                        {
                            var findCoin = db.KTCoins.Where(x => x.UserID == _KTCoinRequest.UserID).FirstOrDefault();
                            if (findCoin != null)
                            {
                                if (_KTCoinRequest.Value > 0)
                                {
                                    if (findCoin.KCoin < _KTCoinRequest.Value)
                                    {
                                        _CodeRep.Status = -1;
                                        _CodeRep.Msg = "Token của bạn không đủ vui lòng nạp thêm";
                                        _CodeRep.Value = 0;
                                    }
                                    else
                                    {
                                        int BeforeValue = (int)findCoin.KCoin;

                                        findCoin.KCoin = findCoin.KCoin - _KTCoinRequest.Value;

                                        int Affter = (int)findCoin.KCoin;

                                        db.KTCoins.AddOrUpdate(findCoin);
                                        db.SaveChanges();

                                        _CodeRep.Status = 0;
                                        _CodeRep.Msg = "Trừ coin thành công";
                                        _CodeRep.Value = 0;

                                        LogManager.WriteLog(LogTypes.SQL, "[" + _KTCoinRequest.UserID + "][" + _KTCoinRequest.RoleID + "]Reuqest PayPacket :" + _KTCoinRequest.Value);

                                        LogsTran logs = new LogsTran();
                                        logs.RoleID = _KTCoinRequest.RoleID;
                                        logs.RoleName = _KTCoinRequest.RoleName;
                                        logs.ServerID = _KTCoinRequest.SeverID;
                                        logs.TimeTrans = DateTime.Now;
                                        logs.UserID = _KTCoinRequest.UserID;
                                        logs.Value = _KTCoinRequest.Value;
                                        logs.BeforeValue = BeforeValue;
                                        logs.AfterValue = Affter;

                                        db.LogsTrans.Add(logs);
                                        db.SaveChanges();
                                    }
                                }
                                else
                                {
                                    _CodeRep.Status = -1;
                                    _CodeRep.Msg = "Tham số gửi lên không hợp lệ";
                                    _CodeRep.Value = 0;
                                }
                            }
                            else
                            {
                                _CodeRep.Status = -1;
                                _CodeRep.Msg = "Token của bạn không đủ vui lòng nạp thêm";
                                _CodeRep.Value = 0;
                            }
                        }
                    }
                }
                else
                {
                    _CodeRep.Msg = "Dữ liệu truyền lên sai định dạng";
                    _CodeRep.Status = -1;
                }

                //Trả về client
                byte[] ResponCode = DataHelper.ObjectToBytes<KTCoinResponse>(_CodeRep);
                base.Response.OutputStream.Write(ResponCode, 0, ResponCode.Length);
                base.Response.OutputStream.Flush();
            }
            catch (Exception exx)
            {
                LogManager.WriteLog(LogTypes.SQL, "BUG :" + exx.ToString());
            }
        }
    }
}