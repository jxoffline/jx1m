using GameServer.Logic;
using GameServer.Server;
using Server.Protocol;
using Server.TCP;
using Server.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameServer.KiemThe.Logic
{
    /// <summary>
    /// Quản lý thao tác tiền tệ trong thương khố
    /// </summary>
    public static partial class KT_TCPHandler
    {
        /// <summary>
        /// Thao tác với số tiền trong thương khố
        /// </summary>
        /// <param name="pool"></param>
        /// <param name="nID"></param>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static TCPProcessCmdResults ProcessModifyStoreMoney(TCPManager tcpMgr, TMSKSocket socket, TCPClientPool tcpClientPool, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;
            string cmdData = null;

            try
            {
                cmdData = new UTF8Encoding().GetString(data, 0, count);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Decode data faild, CMD={0}, Client={1}", (TCPGameServerCmds) nID, Global.GetSocketRemoteEndPoint(socket)));
                return TCPProcessCmdResults.RESULT_FAILED;
            }

            try
            {
                string[] fields = cmdData.Split(':');
                if (fields.Length != 2)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Error Socket params count not fit CMD={0}, Client={1}, Recv={2}", (TCPGameServerCmds) nID, Global.GetSocketRemoteEndPoint(socket), fields.Length));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// ID người chơi
                int roleID = Convert.ToInt32(fields[0]);
                /// Giá trị
                int value = Convert.ToInt32(fields[1]);

                /// Người chơi tương ứng
                KPlayer client = KTPlayerManager.Find(socket);
                if (null == client || client.RoleID != roleID)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Can not find player corresponding ID, CMD={0}, Client={1}", (TCPGameServerCmds) nID, Global.GetSocketRemoteEndPoint(socket)));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// Gói tin gửi lại người chơi
                string strCmd = "";

                int oldMoney = client.Money;
                long oldStoreMoney = client.StoreMoney;
                int ret;

                /// Nếu là thao tác thêm vào
                if (value > 0)
                {
                    /// Nếu số bạc nhập vào lớn hơn số bạc hiện có trong túi thì thông báo không đủ tiền
                    if (value > oldMoney)
                    {
                        strCmd = string.Format("{0}:{1}", roleID, -1);
                        tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, strCmd, nID);
                        return TCPProcessCmdResults.RESULT_DATA;
                    }

                    /// Thực hiện trừ bạc trong túi
                    if (!KTPlayerManager.SubMoney(client, value, "Add_Store_Money"))
                    {
                        strCmd = string.Format("{0}:{1}", roleID, -1);
                        tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, strCmd, nID);
                        return TCPProcessCmdResults.RESULT_DATA;
                    }

                    /// Thêm bạc vào kho cho người chơi
                    KTPlayerManager.AddUserStoreMoney(client, value, "Add_Store_Money");

                    ret = 1;
                }
                /// Nếu là thao tác rút ra
                else if (value < 0)
                {
                    /// Nếu số tiền vượt quá ngưỡng
                    if (oldMoney + value > KTGlobal.Max_Role_Money)
                    {
                        strCmd = string.Format("{0}:{1}", roleID, -3);
                        tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, strCmd, nID);
                        return TCPProcessCmdResults.RESULT_DATA;
                    }

                    /// Nếu số lượng rút vượt quá số bạc hiện có trong kho
                    if (Math.Abs(value) > oldStoreMoney)
                    {
                        strCmd = string.Format("{0}:{1}", roleID, -2);
                        tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, strCmd, nID);
                        return TCPProcessCmdResults.RESULT_DATA;
                    }

                    /// Trừ bạc trong kho của người chơi
                    if (!KTPlayerManager.AddUserStoreMoney(client, value, "Withdraw_Store_Money"))
                    {
                        strCmd = string.Format("{0}:{1}", roleID, -2);
                        tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, strCmd, nID);
                        return TCPProcessCmdResults.RESULT_DATA;
                    }

                    /// Thêm bạc cho người chơi
                    KTPlayerManager.AddMoney(client, -value, "Withdraw_Store_Money");

                    ret = 0;
                }
                /// Nếu không có gì thì toác
                else
                {
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                strCmd = string.Format("{0}:{1}", roleID, ret);
                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, strCmd, nID);
                return TCPProcessCmdResults.RESULT_DATA;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, Global.GetDebugHelperInfo(socket), false);
            }

            return TCPProcessCmdResults.RESULT_FAILED;
        }
    }
}
