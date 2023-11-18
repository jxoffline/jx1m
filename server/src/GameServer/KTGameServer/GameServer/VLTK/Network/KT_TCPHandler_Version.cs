using GameServer.KiemThe.Entities;
using GameServer.Logic;
using GameServer.Server;
using Server.Protocol;
using Server.TCP;
using Server.Tools;
using System;
using System.Text;

namespace GameServer.KiemThe.Logic
{
    /// <summary>
    /// Quản lý thông tin phiên bản
    /// </summary>
    public static partial class KT_TCPHandler
    {
        /// <summary>
        /// Phản hồi thông tin phiên bản Client
        /// </summary>
        /// <param name="pool"></param>
        /// <param name="nID"></param>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static TCPProcessCmdResults ProcessClientPushVersionCmd(TCPManager tcpMgr, TMSKSocket socket, TCPClientPool tcpClientPool, TCPRandKey tcpRandKey, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;
            string cmdData = null;

            try
            {
                cmdData = new UTF8Encoding().GetString(data, 0, count);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Decode data faild, CMD={0}, Client={1}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket)));
                return TCPProcessCmdResults.RESULT_FAILED;
            }

            try
            {
                string[] fields = cmdData.Split(':');
                if (fields.Length < 4)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Error Socket params count not fit CMD={0}, Client={1}, Recv={2}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), fields.Length));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// Phiên bản Client
                string clientVersion = fields[0];
                /// Loại thiết bị
                int deviceType = int.Parse(fields[1]);
                /// Mẫu thiết bị
                string deviceModel = fields[2];
                /// Thế hệ thiết bị
                string deviceGeneration = fields[3];

                /// Toác
                if (deviceType < 0 || deviceType >= (int)DeviceType.Count)
                {
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// Người chơi tương ứng
                KPlayer client = KTPlayerManager.Find(socket);
                /// Cập nhật thông tin
                client.ClientVersion = clientVersion;
                client.DeviceType = (DeviceType)deviceType;
                client.DeviceModel = deviceModel;
                client.DeviceGeneration = deviceGeneration;

                /// Ghi lại thông tin
                try
                {
                    string ip = KTGlobal.GetIPAddress(client);
                    string analysisLog = string.Format("[LOGIN] {0} (ID: {1}), IP: {2}, Device: {3} - {4} - {5}, Version: {6}", client.RoleName, client.RoleID, ip, client.DeviceType, client.DeviceModel, client.DeviceGeneration, client.ClientVersion);
                    LogManager.WriteLog(LogTypes.Analysis, analysisLog);

                    //Console.WriteLine(analysisLog);
                }
                catch (Exception ex)
                {
                    LogManager.WriteLog(LogTypes.Exception, ex.ToString());
                }

                /// Phiên bản
                const string CorrectVersion = "1.0.6";
                /// Cần update không
                bool needUpdate = false;

                /// Nếu mà phiên bản mà khác 1.0.6
                //if (client.ClientVersion != CorrectVersion && client.DeviceType == DeviceType.Android)
                //{
                //    /// Đánh dấu cần Update
                //    needUpdate = true;
                //}

                /// Nếu cần Update
                if (needUpdate)
                {
                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(TCPOutPacketPool.getInstance(), string.Format("{0}:{1}", 1, CorrectVersion), nID);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                return TCPProcessCmdResults.RESULT_OK;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, Global.GetDebugHelperInfo(socket), false);
            }

            return TCPProcessCmdResults.RESULT_FAILED;
        }
    }
}