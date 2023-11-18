using GameServer.Core.Executor;
using GameServer.Interface;
using GameServer.KiemThe;
using GameServer.KiemThe.Core.Item;
using GameServer.KiemThe.Core.Task;
using GameServer.KiemThe.Entities;
using GameServer.KiemThe.GameDbController;
using GameServer.KiemThe.Logic;
using GameServer.Server;
using HSGameEngine.Tools.AStar;
using Server.Data;
using Server.Protocol;
using Server.TCP;
using Server.Tools;
using System;
using System.Collections.Generic;

using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows;
using System.Xml.Linq;

using Tmsk.Contract;
using Tmsk.Tools.Tools;
using static GameServer.Logic.KTMapManager;

namespace GameServer.Logic
{
    public class Global
    {

        #region Tầng mạng

        /// <summary>
        /// _TCPManager
        /// </summary>
        public static TCPManager _TCPManager { get; set; } = null;

        /// <summary>
        /// Quản lý việc gửi gói tin
        /// </summary>
        public static SendBufferManager _SendBufferManager { get; set; } = null;

        /// <summary>
        /// QUản lý bộ nhớ
        /// </summary>
        public static MemoryManager _MemoryManager { get; set; } = null;

        /// <summary>
        /// Quản lý ghi gửi nhận phụ bản
        /// </summary>
        public static FullBufferManager _FullBufferManager { get; set; } = null;

        #endregion Tầng mạng

        #region Xử lý XMl
        public static string GetXElementNodePath(XElement element)
        {
            try
            {
                string path = element.Name.ToString();
                element = element.Parent;
                while (null != element)
                {
                    path = element.Name.ToString() + "/" + path;
                    element = element.Parent;
                }

                return path;
            }
            catch (Exception)
            {
                return "";
            }
        }

        public static XAttribute GetSafeAttribute(XElement XML, string attribute)
        {
            try
            {
                XAttribute attrib = XML.Attribute(attribute);
                if (null == attrib)
                {
                    throw new Exception(string.Format("Get attribute failed: {0}, Error: {1}", attribute, GetXElementNodePath(XML)));
                }

                return attrib;
            }
            catch (Exception)
            {
                throw new Exception(string.Format("Get attribute: {0} failed: {0}, Error: {1}", attribute, GetXElementNodePath(XML)));
            }
        }

        public static XAttribute GetSafeAttributeAppectNull(XElement XML, string attribute)
        {
            try
            {
                XAttribute attrib = XML.Attribute(attribute);
                if (null == attrib)
                {
                    return null;
                }

                return attrib;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static string GetSafeAttributeStr(XElement XML, string attribute)
        {
            XAttribute attrib = GetSafeAttribute(XML, attribute);
            return (string)attrib;
        }

        public static long GetSafeAttributeLong(XElement XML, string attribute)
        {
            XAttribute attrib = GetSafeAttribute(XML, attribute);
            string str = (string)attrib;
            if (null == str || str == "") return -1;

            try
            {
                return (long)Convert.ToDouble(str);
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Exception, "EX :" + ex.ToString());

                return 0;
            }
        }

        public static long GetSafeAttributeLongWithNull(XElement XML, string attribute)
        {
            XAttribute attrib = GetSafeAttributeAppectNull(XML, attribute);
            if (attrib == null)
            {
                return -1;
            }

            string str = (string)attrib;
            if (null == str || str == "") return -1;

            try
            {
                return (long)Convert.ToDouble(str);
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Exception, "EX :" + ex.ToString());

                return 0;
            }
        }

        public static double GetSafeAttributeDouble(XElement XML, string attribute)
        {
            XAttribute attrib = GetSafeAttribute(XML, attribute);
            string str = (string)attrib;
            if (null == str || str == "") return 0.0;

            try
            {
                return Convert.ToDouble(str);
            }
            catch (Exception)
            {
                throw new Exception(string.Format("Read attribute: {0} failed: {0}, Error: {1}", attribute, GetXElementNodePath(XML)));
            }
        }

        public static XAttribute GetSafeAttribute(XElement XML, string root, string attribute)
        {
            try
            {
                XAttribute attrib = XML.Element(root).Attribute(attribute);
                if (null == attrib)
                {
                    throw new Exception(string.Format("Read attribute: {0}/{1} failed: {0}, Error: {2}", root, attribute, GetXElementNodePath(XML)));
                }

                return attrib;
            }
            catch (Exception)
            {
                throw new Exception(string.Format("Read attribute: {0}/{1} failed: {0}, Error: {2}", root, attribute, GetXElementNodePath(XML)));
            }
        }

        public static string GetSafeAttributeStr(XElement XML, string root, string attribute)
        {
            XAttribute attrib = GetSafeAttribute(XML, root, attribute);
            return (string)attrib;
        }

        public static long GetSafeAttributeLong(XElement XML, string root, string attribute)
        {
            XAttribute attrib = GetSafeAttribute(XML, root, attribute);
            string str = (string)attrib;
            if (null == str || str == "") return -1;

            try
            {
                return (long)Convert.ToDouble(str);
            }
            catch (Exception)
            {
                throw new Exception(string.Format("Read attribute: {0}/{1} failed: {0}, Error: {2}", root, attribute, GetXElementNodePath(XML)));
            }
        }

        #endregion Xử lý XMl




        #region Convert chuyển đổi đơn vị

        public static int SafeConvertToInt32(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return 0;
            }
            str = str.Trim();
            if (string.IsNullOrEmpty(str)) return 0;

            try
            {
                return Convert.ToInt32(str);
            }
            catch (Exception)
            {
            }

            return 0;
        }

        public static long SafeConvertToInt64(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return 0;
            }

            str = str.Trim();
            if (string.IsNullOrEmpty(str)) return 0;

            try
            {
                return Convert.ToInt64(str);
            }
            catch (Exception)
            {
            }

            return 0;
        }


        

        #endregion Convert chuyển đổi đơn vị


        


        

        #region Quản lý kết nối

        public static string GetSocketRemoteIP(KPlayer client, bool bForce = false)
        {
            long canRecordIp = 1;
            if (0 == canRecordIp && false == bForce)
            {
                return "";
            }

            string ipAndPort = GetSocketRemoteEndPoint(client.ClientSocket);
            int idx = ipAndPort.IndexOf(':');
            if (idx > 0)
            {
                return ipAndPort.Substring(0, idx);
            }
            else
            {
                return ipAndPort;
            }
        }

        /// <summary>
        /// Gửi packet giữ kết nối
        /// </summary>
        public static void SendGameServerHeart(TCPClient tcpClient)
        {
            if (null == tcpClient)
                return;

            string cmd = string.Format("{0}:{1}:{2}", GameManager.ServerLineID, KTPlayerManager.GetPlayersCount(), Global.SendServerHeartCount);
            Global.SendServerHeartCount++;

            TCPOutPacket tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(TCPOutPacketPool.getInstance(), cmd, (int)TCPGameServerCmds.CMD_DB_ONLINE_SERVERHEART);

            if (null != tcpOutPacket)
            {
                byte[] bytesData = Global.SendAndRecvData(tcpClient, tcpOutPacket);
            }
        }

        public static string GetSocketRemoteEndPoint(TMSKSocket s, bool bForce = false)
        {
            try
            {
                long canRecordIp = 1;
                if (0 == canRecordIp && false == bForce)
                {
                    return "";
                }

                if (null == s)
                {
                    return "";
                }

                return string.Format("{0} ", s.RemoteEndPoint);
            }
            catch (Exception)
            {
            }

            return "";
        }

        

        public static string GetDebugHelperInfo(TMSKSocket socket)
        {
            if (null == socket)
            {
                return "socket为null, 无法打印错误信息";
            }

            string ret = "";
            try
            {
                ret += string.Format("IP={0} ", GetSocketRemoteEndPoint(socket));
            }
            catch (Exception)
            {
            }

            try
            {
                KPlayer client = KTPlayerManager.Find(socket);
                if (null != client)
                {
                    ret += string.Format("RoleID={0}({1})", client.RoleID, client.RoleName);
                }
            }
            catch (Exception)
            {
            }

            return ret;
        }

        #endregion Quản lý kết nối



        #region Giao tiếp với gameDBserver

        public static byte[] SendAndRecvData(TCPClient tcpClient, TCPOutPacket tcpOutPacket)
        {
            byte[] bytesData = null;

            //查询
            try
            {
                //获取
                if (null != tcpClient)
                {
                    bytesData = tcpClient.SendData(tcpOutPacket);
                }

                if (null != bytesData && bytesData.Length >= 6)
                {
                    UInt16 returnCmdID = BitConverter.ToUInt16(bytesData, 4);
                    if ((UInt16)TCPGameServerCmds.CMD_DB_ERR_RETURN == returnCmdID) //返回失败的错误信息
                    {
                        //告诉外边失败
                        bytesData = null;
                        LogManager.WriteLog(LogTypes.Error, "Return from NameServer => CMD_DB_ERR_RETURN");
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Error, "Return from NameServer => " + ex);
            }

            return bytesData;
        }

        public static byte[] SendAndRecvData(TCPOutPacket tcpOutPacket, int serverId, int PoolId)
        {
            TCPClient tcpClient = null;
            byte[] bytesData = null;

            try
            {
                tcpClient = Global.PopGameDbClient(serverId, PoolId);
                if (null != tcpClient)
                {
                    bytesData = tcpClient.SendData(tcpOutPacket);
                }

                if (null != bytesData && bytesData.Length >= 6)
                {
                    UInt16 returnCmdID = BitConverter.ToUInt16(bytesData, 4);
                    if ((UInt16)TCPGameServerCmds.CMD_DB_ERR_RETURN == returnCmdID)
                    {
                        bytesData = null;
                        LogManager.WriteLog(LogTypes.Error, "Return from DBServer => CMD_DB_ERR_RETURN");
                    }
                }
            }
            finally
            {
                if (null != tcpClient)
                {
                    Global.PushGameDbClient(serverId, tcpClient, PoolId);
                }
            }

            return bytesData;
        }

        public static TCPProcessCmdResults RequestToDBServer(TCPClientPool tcpClientPool, TCPOutPacketPool pool, int nID, string strcmd, out string[] fields, int serverId)
        {
            fields = null;

            try
            {
                byte[] bytesData = Global.SendAndRecvData(nID, strcmd, serverId);
                if (null == bytesData)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Send packet to GameDB faild, CMD={0}", (TCPGameServerCmds)nID));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                Int32 length = BitConverter.ToInt32(bytesData, 0);
                string strData = new UTF8Encoding().GetString(bytesData, 6, length - 2);

                //解析客户端的指令
                fields = strData.Split(':');
                return TCPProcessCmdResults.RESULT_DATA;
            }
            catch (Exception ex)
            {
                //System.Windows.Application.Current.Dispatcher.Invoke((MethodInvoker)delegate
                //{
                // 格式化异常错误信息
                DataHelper.WriteExceptionLogEx(ex, "RequestToDBServer");
                //throw ex;
                //});
            }

            return TCPProcessCmdResults.RESULT_FAILED;
        }

        /// <summary>
        /// Gửi và nhận gói tin
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cmdId"></param>
        /// <param name="data"></param>
        /// <param name="serverId"></param>
        /// <param name="PoolId">0 GameDbServer,1 LogDbServer</param>
        /// <returns></returns>
        public static byte[] SendAndRecvData<T>(int cmdId, T data, int serverId, int PoolId = 0)
        {
            byte[] bytesData = null;
            TCPOutPacket tcpOutPacket = null;
            TCPOutPacketPool pool = TCPOutPacketPool.getInstance();

            if (null != pool)
            {
                //查询
                try
                {
                    //获取
                    if (data is string)
                    {
                        tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, data as string, cmdId);
                    }
                    else if (data is byte[])
                    {
                        tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, data as byte[], cmdId);
                    }
                    else
                    {
                        byte[] cmdData = DataHelper.ObjectToBytes<T>(data);
                        if (null != cmdData)
                        {
                            tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, cmdData, cmdId);
                        }
                    }

                    if (null != tcpOutPacket)
                    {
                        bytesData = Global.SendAndRecvData(tcpOutPacket, serverId, PoolId);
                    }
                }
                finally
                {
                    //还回
                    if (null != tcpOutPacket)
                    {
                        pool.Push(tcpOutPacket);
                    }
                }
            }

            return bytesData;
        }

        /// <summary>
        /// Chuyển yêu cầu sang GameDB
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cmdId"></param>
        /// <param name="cmd"></param>
        /// <returns></returns>
        public static T1 SendToDB<T1, T2>(int cmdId, T2 cmd, int serverId)
        {
            try
            {
                byte[] bytesData = Global.SendAndRecvData(cmdId, cmd, serverId);

                if (null == bytesData)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Send packet to GameDB faild, CMD={0}", (TCPGameServerCmds)cmdId));
                    return default(T1);
                }

                Int32 length = BitConverter.ToInt32(bytesData, 0);

                T1 obj = DataHelper.BytesToObject<T1>(bytesData, 6, length - 2);

                return obj;
            }
            catch (Exception ex)
            {
                // 格式化异常错误信息
                DataHelper.WriteFormatExceptionLog(ex, "SendToDB", false);
            }

            return default(T1);
        }

        /// <summary>
        /// Gửi yêu cầu sang GameDB
        /// </summary>
        /// <param name="cmdID"></param>
        /// <param name="cmdData"></param>
        /// <param name="serverId"></param>
        /// <returns></returns>
        public static string[] SendToDB(int cmdID, string cmdData, int serverId)
        {
            try
            {
                byte[] bytesData = Global.SendAndRecvData(cmdID, cmdData, serverId);

                if (null == bytesData)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Send packet to GameDB faild, CMD={0}", (TCPGameServerCmds)cmdID));
                    return null;
                }

                int length = BitConverter.ToInt32(bytesData, 0);
                string strData = new UTF8Encoding().GetString(bytesData, 6, length - 2);

                string[] fields = strData.Split(':');
                return fields;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, "SendToDB", false);
            }

            return null;
        }

        /// <summary>
        /// Chuyển yêu cầu sang GameDB
        /// </summary>
        /// <param name="nID"></param>
        /// <param name="strcmd"></param>
        /// <returns></returns>
        public static string[] SendToDB<T>(int nCmdID, T CmdInfo, int serverId)
        {
            byte[] bytesCmd = DataHelper.ObjectToBytes<T>(CmdInfo);

            byte[] bytesData = Global.SendAndRecvData(nCmdID, bytesCmd, serverId);

            if (null == bytesData)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Send packet to GameDB faild, CMD={0}", (TCPGameServerCmds)nCmdID));
                return null;
            }

            string[] fieldsData = null;
            Int32 length = BitConverter.ToInt32(bytesData, 0);
            string strData = new UTF8Encoding().GetString(bytesData, 6, length - 2);

            fieldsData = strData.Split(':');
            if (null == fieldsData || fieldsData.Length <= 0)
            {
                return null;
            }

            return fieldsData;
        }

        public static TCPProcessCmdResults RequestToDBServer2(TCPClientPool tcpClientPool, TCPOutPacketPool pool, int nID, string strcmd, out TCPOutPacket tcpOutPacket, int serverId)
        {
            tcpOutPacket = null;

            try
            {
                byte[] bytesData = Global.SendAndRecvData(nID, strcmd, serverId);
                if (null == bytesData)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Send packet to GameDB faild, CMD={0}", (TCPGameServerCmds)nID));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                Int32 length = BitConverter.ToInt32(bytesData, 0);
                UInt16 cmd = BitConverter.ToUInt16(bytesData, 4);

                tcpOutPacket = pool.Pop();
                tcpOutPacket.PacketCmdID = (UInt16)cmd;
                tcpOutPacket.FinalWriteData(bytesData, 6, length - 2);
                return TCPProcessCmdResults.RESULT_DATA;
            }
            catch (Exception ex)
            {
                //System.Windows.Application.Current.Dispatcher.Invoke((MethodInvoker)delegate
                //{
                // 格式化异常错误信息
                DataHelper.WriteFormatExceptionLog(ex, "RequestToDBServer2", false);
                //throw ex;
                //});
            }

            return TCPProcessCmdResults.RESULT_FAILED;
        }

        public static TCPProcessCmdResults RequestToDBServer3(TCPClientPool tcpClientPool, TCPOutPacketPool pool, int nID, string strcmd, out byte[] bytesData, int serverId)
        {
            bytesData = null;

            try
            {
                bytesData = Global.SendAndRecvData(nID, strcmd, serverId);
                if (null == bytesData)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Send packet to GameDB faild, CMD={0}", (TCPGameServerCmds)nID));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                return TCPProcessCmdResults.RESULT_DATA;
            }
            catch (Exception ex)
            {
                //System.Windows.Application.Current.Dispatcher.Invoke((MethodInvoker)delegate
                //{
                // 格式化异常错误信息
                DataHelper.WriteFormatExceptionLog(ex, "RequestToDBServer3", false);
                //throw ex;
                //});
            }

            return TCPProcessCmdResults.RESULT_FAILED;
        }

        public static TCPProcessCmdResults RequestToDBServer4(TCPClientPool tcpClientPool, TCPOutPacketPool pool, int nID, string strcmd, out byte[] bytesData, out int dataStartPos, out int dataLen, int serverId)
        {
            bytesData = null;
            dataStartPos = 0;
            dataLen = 0;

            try
            {
                bytesData = Global.SendAndRecvData(nID, strcmd, serverId);
                if (null == bytesData)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Send packet to GameDB faild, CMD={0}", (TCPGameServerCmds)nID));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                Int32 length = BitConverter.ToInt32(bytesData, 0);

                dataStartPos = 6;
                dataLen = length - 2;

                return TCPProcessCmdResults.RESULT_DATA;
            }
            catch (Exception ex)
            {
                //System.Windows.Application.Current.Dispatcher.Invoke((MethodInvoker)delegate
                //{
                // 格式化异常错误信息
                DataHelper.WriteFormatExceptionLog(ex, "RequestToDBServer4", false);
                //throw ex;
                //});
            }

            return TCPProcessCmdResults.RESULT_FAILED;
        }

        public static TCPProcessCmdResults TransferRequestToDBServer(TCPManager tcpMgr, TMSKSocket socket, TCPClientPool tcpClientPool, TCPRandKey tcpRandKey, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket, int serverId)
        {
            tcpOutPacket = null;

            try
            {
                byte[] bytesData = Global.SendAndRecvData(nID, data, socket.ServerId);
                if (null == bytesData)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Send packet to GameDB faild, CMD={0}", (TCPGameServerCmds)nID));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                Int32 length = BitConverter.ToInt32(bytesData, 0);
                UInt16 cmd = BitConverter.ToUInt16(bytesData, 4);

                tcpOutPacket = pool.Pop();
                tcpOutPacket.PacketCmdID = (UInt16)cmd;
                tcpOutPacket.FinalWriteData(bytesData, 6, length - 2);

                return TCPProcessCmdResults.RESULT_DATA;
            }
            catch (Exception ex)
            {
                //System.Windows.Application.Current.Dispatcher.Invoke((MethodInvoker)delegate
                //{
                // 格式化异常错误信息
                DataHelper.WriteFormatExceptionLog(ex, Global.GetDebugHelperInfo(socket), false);
                //throw ex;
                //});
            }

            return TCPProcessCmdResults.RESULT_FAILED;
        }

        public static TCPProcessCmdResults TransferRequestToDBServer2(TCPManager tcpMgr, TMSKSocket socket, TCPClientPool tcpClientPool, TCPRandKey tcpRandKey, TCPOutPacketPool pool, int nID, byte[] data, int count, out byte[] bytesData, int serverId)
        {
            bytesData = null;

            try
            {
                bytesData = Global.SendAndRecvData(nID, data, socket.ServerId);
                if (null == bytesData)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Send packet to GameDB faild, CMD={0}", (TCPGameServerCmds)nID));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                return TCPProcessCmdResults.RESULT_DATA;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, Global.GetDebugHelperInfo(socket), false);
            }

            return TCPProcessCmdResults.RESULT_FAILED;
        }

        public static TCPProcessCmdResults ReadDataFromDb(int nID, byte[] data, int count, out byte[] bytesData, int serverId)
        {
            bytesData = null;

            try
            {
                bytesData = Global.SendAndRecvData(nID, data, serverId);
                if (null == bytesData)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Send packet to GameDB faild, CMD={0}", (TCPGameServerCmds)nID));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                return TCPProcessCmdResults.RESULT_DATA;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                //  DataHelper.WriteFormatExceptionLog(ex, Global.GetDebugHelperInfo(socket), false);
            }

            return TCPProcessCmdResults.RESULT_FAILED;
        }

        public static string[] ExecuteDBCmd(int nID, string strcmd, int serverId)
        {
            string[] fieldsData = null;
            if (TCPProcessCmdResults.RESULT_FAILED == Global.RequestToDBServer(Global._TCPManager.tcpClientPool, Global._TCPManager.TcpOutPacketPool,
                nID, strcmd, out fieldsData, serverId))
            {
                return null;
            }

            if (null == fieldsData || fieldsData.Length <= 0)
            {
                return null;
            }

            return fieldsData;
        }

        #endregion

        #region Tickcount

        public static int SendServerHeartCount { get; set; } = 0;

        #endregion

        public static int GetSwitchServerWaitSecs(TMSKSocket socket)
        {
            TimeSpan timeOfDay = TimeUtil.NowDateTime().TimeOfDay;
            if (timeOfDay.TotalMinutes >= GameManager.ConstCheckServerTimeDiffMinutes && timeOfDay.TotalMinutes < TimeSpan.FromDays(1).TotalMinutes - GameManager.ConstCheckServerTimeDiffMinutes)
            {
                return 0;
            }

            long waitSecs = (socket.session.LastLogoutServerTicks - TimeUtil.NOW()) / 1000;
            if (waitSecs < 0 || waitSecs > 60)
            {
                if (waitSecs > 60 && waitSecs < 60 * 60)
                {
                    //超过60秒误差的，属于系统故障、测试环境或配置错误
                    LogManager.WriteLog(LogTypes.Error, string.Format("账号登陆时检测，服务器时间误差可能超过60秒，本次登录比上次下线时间早{0}秒", waitSecs));
                }
                waitSecs = 0;
            }

            return (int)waitSecs;
        }

        public static void UpdateDBGameConfigg(string paramName, string paramValue)
        {
            Global.ExecuteDBCmd((int)TCPGameServerCmds.CMD_DB_GAMECONIFGITEM,
                string.Format("{0}:{1}", paramName, paramValue),
                 GameManager.LocalServerId);
        }

        public static Dictionary<string, string> LoadDBGameConfigDict()
        {
            Dictionary<string, string> dict = null;

            byte[] bytesData = null;
            if (TCPProcessCmdResults.RESULT_FAILED == Global.RequestToDBServer3(Global._TCPManager.tcpClientPool, Global._TCPManager.TcpOutPacketPool,
                (int)TCPGameServerCmds.CMD_DB_GAMECONFIGDICT, string.Format("{0}", GameManager.ServerLineID), out bytesData, GameManager.LocalServerId))
            {
                return dict; //如果查询失败，就当做时不在线了
            }

            if (null == bytesData || bytesData.Length <= 6)
            {
                return dict;
            }

            Int32 length = BitConverter.ToInt32(bytesData, 0);

            dict = DataHelper.BytesToObject<Dictionary<string, string>>(bytesData, 6, length - 2);
            return dict;
        }

        public static void SetLastDBCmdTicks(KPlayer client, int dbCmdID, long nowTicks)
        {
            lock (client.LastDBCmdTicksDict)
            {
                client.LastDBCmdTicksDict[dbCmdID] = nowTicks;
            }
        }

        #region ForceCloseClient

        public static void ForceCloseClient(KPlayer client, string reason = "", bool sync = true)
        {
            if (!string.IsNullOrEmpty(reason))
            {
                /**/
                reason = string.Format("RoleID={0}, RoleName={1}, 强制关闭:{2}", client.RoleID, client.RoleName, reason);
            }

            client.ClosingClientStep = 1;
            Global._TCPManager.MySocketListener.CloseSocket(client.ClientSocket, reason);
        }

        public static void ForceCloseSocket(TMSKSocket socket, string reason = "", bool sync = true)
        {
            //Global._TCPManager.MySocketListener.CloseSocket(socket);
            if ("" != reason || string.IsNullOrEmpty(socket.CloseReason))
            {
                socket.CloseReason = reason;
            }

            Global._TCPManager.ExternalClearSocket(socket);
        }

        #endregion ForceCloseClient

        #region Xử lý toàn bộ phần phúc lợi ở đây

        //TODO Nếu mà nó chưa có recore nào liên quan tới phúc lợi
        //Thì sẽ thực hiện cập nhật thông tin vào DB
        //Nếu đã có recore rồi thì sẽ check xem nó có đăng nhập liên tục ko để update vào continus login
        //Nếu mà ngày hôm nay khác ngày hôm trước thì sẽ tiện hành RESET các recore cần thiết
        // TÝ ĂN CƠM XONG LÊN VÃ TIẾP
        public static bool UpdateWelfareRole(KPlayer client)
        {
            ///Lấy ra tuần hiện tại
            int weekID = TimeUtil.GetIso8601WeekOfYear(DateTime.Now);
            ////Lấy ra ngày hiện tại
            int todayID = TimeUtil.NowDateTime().DayOfYear;

            int monthID = TimeUtil.NowDateTime().Month;

            // Nếu mà cái này = 0 tức là nhân vật vừa tạo xong
            if (client.RoleWelfareData.createdayid == 0)
            {
                client.RoleWelfareData.createdayid = todayID;
                client.RoleWelfareData.lastdaylogin = todayID;
                // Đã đăng nhập liên tiếp 0 ngày
                client.RoleWelfareData.logincontinus = 0;
                // Chưa nhận mốc nào
                client.RoleWelfareData.sevenday_continus_step = 0;
                client.RoleWelfareData.sevenday_continus_note = "NONE";
                client.RoleWelfareData.sevendaylogin_note = "NONE";
                client.RoleWelfareData.sevendaylogin_step = "NONE";
                client.RoleWelfareData.createdayid = todayID;
                client.RoleWelfareData.logindayid = todayID;
                client.RoleWelfareData.loginweekid = weekID;
                client.RoleWelfareData.online_step = "NONE";
                client.RoleWelfareData.levelup_step = "NONE";
                client.RoleWelfareData.monthid = monthID;
                client.RoleWelfareData.checkpoint = "NONE";
                client.RoleWelfareData.fist_recharge_step = 0;
                client.RoleWelfareData.totarechage_step = "NONE";
                client.RoleWelfareData.totalconsume_step = "NONE";
                client.RoleWelfareData.day_rechage_step = "NONE";
                client.RoleWelfareData.RoleID = client.RoleID;
                // Thực hiện ghi vào DB
                string[] Pram = Global.SendToDB((int)TCPGameServerCmds.CMD_DB_UPDATE_WELFARE, client.RoleWelfareData, client.ServerId);

                if (Pram[1] == "-1")
                {
                    LogManager.WriteLog(LogTypes.Welfare, "[BUG SQL WRITER] :" + client.RoleWelfareData.ToString());
                }
            }
            else // DO SOEMTHING
            {
                // Nếu mà ngày đăng nhập khác ngày hôm nay thì suy ra nó đăng nhập lần đầu tiên trong ngày thực hiện reset các thứ cần thiến
                if (client.RoleWelfareData.logindayid != todayID)
                {
                    // Set lại ngày đăng nhập
                    client.RoleWelfareData.logindayid = todayID;
                    // Xử lý sự kiện đăng nhập liên tục
                    // Nếu mà ngày hôm nay trừ đi ngày hôm qua mà là 1 => suy ra nó đã đăng nhập liên tiếp 1 lần
                    if (todayID - client.RoleWelfareData.lastdaylogin == 1)
                    {
                        // Tăng số ngày đăng nhập lên 1
                        client.RoleWelfareData.logincontinus++;
                    }
                    else
                    {
                        // Thực hiện Reset
                        client.RoleWelfareData.logincontinus = 0;
                    }
                    // Reset quà online nhận thưởng
                    client.RoleWelfareData.online_step = "NONE";

                    // Reset các mốc nạp ngày đã nhận

                    client.RoleWelfareData.day_rechage_step = "NONE";

                    // Thực hiện cập nhật tháng nếu cần
                    if (client.RoleWelfareData.monthid != monthID)
                    {
                        client.RoleWelfareData.monthid = monthID;
                        // Reset lại checkpoint
                        client.RoleWelfareData.checkpoint = "NONE";
                    }

                    client.RoleWelfareData.RoleID = client.RoleID;
                    // Thực hiện update vào gamedb
                    string[] Pram = Global.SendToDB((int)TCPGameServerCmds.CMD_DB_UPDATE_WELFARE, client.RoleWelfareData, client.ServerId);

                    if (Pram[1] == "-1")
                    {
                        LogManager.WriteLog(LogTypes.Welfare, "[BUG SQL WRITER] :" + client.RoleWelfareData.ToString());
                    }

                    //TODO : UPdate status icon effect cho client | Cho người chơi biết để có thể nhận
                }
            }

            return false;
        }

        /// <summary>
        /// Force ghi lại phúc lợi
        /// </summary>
        /// <param name="client"></param>
        public static bool WriterWelfare(KPlayer client)
        {
            int todayID = TimeUtil.NowDateTime().DayOfYear;

            // Đánh dấu vào ngày đăng nhập cuối cùng
            client.RoleWelfareData.RoleID = client.RoleID;
            client.RoleWelfareData.lastdaylogin = todayID;

            string[] Pram = Global.SendToDB((int)TCPGameServerCmds.CMD_DB_UPDATE_WELFARE, client.RoleWelfareData, client.ServerId);
            if (Pram == null || Pram[1] == "-1")
            {
                LogManager.WriteLog(LogTypes.Welfare, "[BUG SQL WRITER] :" + client.RoleWelfareData.ToString());
                return false;
            }
            else
            {
                return true;
            }
        }


        #endregion Xử lý toàn bộ phần phúc lợi ở đây

        /// <summary>
        /// Lấy ra rolepram theo tên
        /// </summary>
        /// <param name="client"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string GetRoleParamByName(KPlayer client, string name)
        {
            if (null == client.RoleParamsDict) return null;

            lock (client.RoleParamsDict)
            {
                RoleParamsData roleParamsData = null;
                if (client.RoleParamsDict.TryGetValue(name, out roleParamsData))
                {
                    return roleParamsData.ParamValue;
                }
            }

            return null;
        }

        public static List<int> StringToIntList(string str, char split1 = '$')
        {
            List<int> ls = new List<int>();
            if (null != str && str.Length > 0)
            {
                string[] arr = str.Split(split1);
                foreach (var s in arr)
                {
                    ls.Add(Global.SafeConvertToInt32(s));
                }
            }
            return ls;
        }

        #region Xử lý các vấn đề liên quan tới RolePRAM

        public static KPlayer MakeGameClientForGetRoleParams(RoleDataEx roleDataEx)
        {
            KPlayer clientData = new KPlayer()
            {
                RoleData = roleDataEx,
            };

            return clientData;
        }

        public static int GetRoleParamsInt32FromDB(KPlayer client, String roleParamsKey)
        {
            String valueString = Global.GetRoleParamByName(client, roleParamsKey);

            if (String.IsNullOrEmpty(valueString))
            {
                return 0;
            }

            return Global.SafeConvertToInt32(valueString);
        }

        public static long GetRoleParamsInt64FromDB(KPlayer client, String roleParamsKey)
        {
            String valueString = Global.GetRoleParamByName(client, roleParamsKey);

            if (String.IsNullOrEmpty(valueString))
            {
                return 0;
            }

            return Global.SafeConvertToInt64(valueString);
        }

        public static String GetRoleParamsStringWithNullFromDB(KPlayer client, String roleParamsKey)
        {
            String valueString = Global.GetRoleParamByName(client, roleParamsKey);

            if (String.IsNullOrEmpty(valueString))
            {
                return valueString;
            }

            byte[] bytes = Convert.FromBase64String(valueString);

            valueString = Encoding.GetEncoding("latin1").GetString(bytes);

            return valueString;
        }

        public static String GetRoleParamsStringWithDB(KPlayer client, String roleParamsKey)
        {
            String valueString = Global.GetRoleParamByName(client, roleParamsKey);

            if (String.IsNullOrEmpty(valueString))
            {
                return valueString;
            }

            return valueString;
        }

        public static List<int> GetRoleParamsIntListFromDB(KPlayer client, String roleParamsKey)
        {
            List<int> lsValues = new List<int>();

            String valueString = GetRoleParamsStringWithNullFromDB(client, roleParamsKey);

            if (String.IsNullOrEmpty(valueString))
            {
                return lsValues;
            }

            int pos = 0;
            int usedLenght = 0;

            //依次生成各个32位整数
            while (usedLenght < valueString.Length)
            {
                byte[] bytes_4 = Encoding.GetEncoding("latin1").GetBytes(valueString.Substring(pos, 4));
                lsValues.Add(BitConverter.ToInt32(bytes_4, 0));

                pos += 4;
                usedLenght += 4;
            }

            return lsValues;
        }

        public static List<ushort> GetRoleParamsUshortListFromDB(KPlayer client, String roleParamsKey)
        {
            List<ushort> lsValues = new List<ushort>();

            String valueString = GetRoleParamsStringWithNullFromDB(client, roleParamsKey);

            if (String.IsNullOrEmpty(valueString))
            {
                return lsValues;
            }

            int pos = 0;
            int usedLenght = 0;

            while (usedLenght < valueString.Length)
            {
                byte[] bytes_2 = Encoding.GetEncoding("latin1").GetBytes(valueString.Substring(pos, 2));
                lsValues.Add(BitConverter.ToUInt16(bytes_2, 0));

                pos += 2;
                usedLenght += 2;
            }

            return lsValues;
        }

        public static void SaveRoleParamsInt32ValueToDB(KPlayer client, String roleParamsKey, Int32 nValue, bool writeToDB)
        {
            GameDb.UpdateRoleParamByName(client, roleParamsKey, nValue.ToString(), writeToDB);
        }

        public static void SaveRoleParamsInt64ValueToDB(KPlayer client, String roleParamsKey, Int64 nValue, bool writeToDB)
        {
            GameDb.UpdateRoleParamByName(client, roleParamsKey, nValue.ToString(), writeToDB);
        }

        public static void SaveRoleParamsStringToDB(KPlayer client, String roleParamsKey, String valueString, bool writeToDB)
        {
            GameDb.UpdateRoleParamByName(client, roleParamsKey, valueString, writeToDB);
        }

        public static void SaveRoleParamsStringWithNullToDB(KPlayer client, String roleParamsKey, String valueString, bool writeToDB)
        {
            byte[] bytes = Encoding.GetEncoding("latin1").GetBytes(valueString);

            GameDb.UpdateRoleParamByName(client, roleParamsKey, Convert.ToBase64String(bytes), writeToDB);
        }

        public static void SaveRoleParamsUshortListToDB(KPlayer client, List<ushort> lsUshort, String roleParamsKey, bool writeToDB = false)
        {
            String newStringValue = "";

            for (int n = 0; n < lsUshort.Count; n++)
            {
                byte[] bytes = BitConverter.GetBytes(lsUshort[n]);
                newStringValue += Encoding.GetEncoding("latin1").GetString(bytes);
            }

            Global.SaveRoleParamsStringWithNullToDB(client, roleParamsKey, newStringValue, writeToDB);
        }

        public static void SaveRoleParamsIntListToDB(KPlayer client, List<int> lsInt, String roleParamsKey, bool writeToDB = false)
        {
            String newStringValue = "";

            for (int n = 0; n < lsInt.Count; n++)
            {
                byte[] bytes = BitConverter.GetBytes(lsInt[n]);
                newStringValue += Encoding.GetEncoding("latin1").GetString(bytes);
            }

            Global.SaveRoleParamsStringWithNullToDB(client, roleParamsKey, newStringValue, writeToDB);
        }

        #endregion

        #region RELOG CONVERT RATE

        /// <summary>
        /// Lấy RATE QUY ĐÔI TỪ KNB RA KNB
        /// </summary>
        /// <param name="money"></param>
        /// <returns></returns>
        public static int TransMoneyToYuanBao(int money)
        {
            int moneyToYuanBao = GameManager.GameConfigMgr.GetGameConfigItemInt("money-to-yuanbao", 10);

            int yuanBao = money * moneyToYuanBao;

            return yuanBao;
        }

        #endregion RELOG CONVERT RATE

      
        #region Lưu lại tích lũy tiêu

        public static void SaveConsumeLog(KPlayer client, int money, int type)
        {
            try
            {
                string dbCmds = client.RoleID + ":" + money + ":" + type;
                string[] dbFields = null;
                Global.RequestToDBServer(Global._TCPManager.tcpClientPool, Global._TCPManager.TcpOutPacketPool, (int)TCPGameServerCmds.CMD_DB_SAVECONSUMELOG, dbCmds, out dbFields, client.ServerId);
            }
            catch (Exception e)
            {
                LogManager.WriteException(e.ToString());
            }
        }

        #endregion Lưu lại tích lũy tiêu

        


        public static KuaFuServerLoginData GetClientKuaFuServerLoginData(KPlayer client)
        {
            TMSKSocket clientSocket = client.ClientSocket;
            if (null != clientSocket)
            {
                if (null != clientSocket.ClientKuaFuServerLoginData)
                {
                    return clientSocket.ClientKuaFuServerLoginData;
                }
            }

            return new KuaFuServerLoginData();
        }

        private static int RoleLoginRecordDayCount = 50;

        /// <summary>
        /// Ghi lại nhật ký loign
        /// </summary>
        public static void UpdateRoleLoginRecord(KPlayer client)
        {
            int currDayID = KTGlobal.GetOffsetDay();

            string strParam = Global.GetRoleParamByName(client, RoleParamName.RoleLoginRecorde);

            int recordDayID = 0;
            string strRecord = "";

            string[] strFields = null == strParam ? null : strParam.Split(',');
            if (null != strFields && strFields.Length == 2)
            {
                recordDayID = Convert.ToInt32(strFields[0]);
                strRecord = strFields[1];
            }

            if (recordDayID == currDayID)
            {
                return;
            }

            if (recordDayID > 0)
            {
                for (int i = recordDayID + 1; i < currDayID; i++)
                {
                    strRecord = "0" + strRecord;
                }
            }

            strRecord = "1" + strRecord;

            if (strRecord.Length > RoleLoginRecordDayCount)
            {
                strRecord = strRecord.Substring(0, RoleLoginRecordDayCount);
            }

            string result = string.Format("{0},{1}", currDayID, strRecord);

            Global.SaveRoleParamsStringToDB(client, RoleParamName.RoleLoginRecorde, result, true);
        }


        #region Lấy ra kết nối có sẵn với gamedb

        public static TCPClient PopGameDbClient(int serverId, int poolId)
        {
#if BetaConfig
            if (serverId <= 0)
#else
            if (serverId <= 0 || serverId == GameManager.ServerId)
#endif
            {
                if (poolId == 0)
                {
                    return Global._TCPManager.tcpClientPool.Pop();
                }
                else// if(poolId == 1)
                {
                    return Global._TCPManager.tcpLogClientPool.Pop();
                }
            }
            else
            {
                return KuaFuManager.getInstance().PopGameDbClient(serverId, poolId);
            }
        }

        public static void PushGameDbClient(int serverId, TCPClient tcpClient, int poolId)
        {
#if BetaConfig
            if (serverId <= 0)
#else
            if (serverId <= 0 || serverId == GameManager.ServerId)
#endif
            {
                if (poolId == 0)
                {
                    Global._TCPManager.tcpClientPool.Push(tcpClient);
                }
                else// if(poolId == 1)
                {
                    Global._TCPManager.tcpLogClientPool.Push(tcpClient);
                }
            }
            else
            {
                KuaFuManager.getInstance().PushGameDbClient(serverId, tcpClient, poolId);
            }
        }

        #endregion Lấy ra kết nối có sẵn với gamedb

        public static void RecordSwitchKuaFuServerLog(KPlayer client)
        {
            KuaFuServerLoginData kuaFuServerLoginData = Global.GetClientKuaFuServerLoginData(client);
            LogManager.WriteLog(LogTypes.Error, string.Format("RoleId={0},GameId={1},SrcServerId={2},KfIp={3},KfPort={4}", kuaFuServerLoginData.RoleId, kuaFuServerLoginData.GameId, kuaFuServerLoginData.ServerId, kuaFuServerLoginData.ServerIp, kuaFuServerLoginData.ServerPort));
        }
    }
}