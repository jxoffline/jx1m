using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using GameServer.Logic;
using Server.Protocol;
using Server.Tools;
using GameServer.Server.CmdProcesser;
using Server.TCP;
using System.Diagnostics;
using GameServer.KiemThe.Logic;

namespace GameServer.Server
{
    public enum TCPCmdFlags
    {
        /// <summary>
        /// 是否字符串数组参数
        /// </summary>
        IsStringArrayParams = 1 << 1,

        /// <summary>
        /// 是否是二进制流参数
        /// </summary>
        IsBinaryStreamParams = 1 << 2,
    }

    public class CmdHandler
    {
        /// <summary>
        /// 指令处理特性的Flags
        /// </summary>
        public uint CmdFlags;

        /// <summary>
        /// 参数个数允许的最小值
        /// </summary>
        public short MinParamCount;

        /// <summary>
        /// 参数个数允许的最大值
        /// </summary>
        public short MaxParamCount;

        /// <summary>
        /// 已注册的指令处理器接口
        /// </summary>
        public ICmdProcessor CmdProcessor;
    }

    public class TCPCmdDispatcher
    {
        private static readonly TCPCmdDispatcher instance = new TCPCmdDispatcher();

        /// <summary>
        /// 指令参数个数映射{指令ID， 参数个数}
        /// </summary>
        private Dictionary<int, CmdHandler> cmdProcesserMapping = new Dictionary<int, CmdHandler>();

        private TCPCmdDispatcher(){}

        public static TCPCmdDispatcher getInstance()
        {
            return instance;
        }

        public void initialize()
        {
            
        }

        public void registerProcessor(int cmdId, short paramNum, ICmdProcessor processor) 
        {
            registerProcessorEx(cmdId, paramNum, paramNum, processor, TCPCmdFlags.IsStringArrayParams);
        }

        public void registerProcessorEx(int cmdId, short minParamCount, short maxParamCount, ICmdProcessor processor, TCPCmdFlags cmdFlags = TCPCmdFlags.IsStringArrayParams)
        {
            Debug.Assert(processor != null);
            CmdHandler cmdHandler = new CmdHandler
            {
                CmdFlags = (uint)cmdFlags,
                MinParamCount = minParamCount,
                MaxParamCount = maxParamCount,
                CmdProcessor = processor,
            };
            cmdProcesserMapping.Add(cmdId, cmdHandler);
        }

        public void registerStreamProcessorEx(int cmdId, ICmdProcessor processor)
        {
            registerProcessorEx(cmdId, -1, -1, processor, TCPCmdFlags.IsBinaryStreamParams);
        }

        private CmdHandler GetCmdHandler(int cmdID)
        {
            CmdHandler cmdHandler;
            if (cmdProcesserMapping.TryGetValue(cmdID, out cmdHandler))
            {
                return cmdHandler;
            }
            return null;
        }

        /// <summary>
        /// 透传到DBServer处理
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="nID"></param>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public TCPProcessCmdResults transmission(TMSKSocket socket, int nID, byte[] data, int count)
        {
            string cmdData = null;

            try
            {
                cmdData = new UTF8Encoding().GetString(data, 0, count);
            }
            catch (Exception) //解析错误
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("解析指令字符串错误, CMD={0}, Client={1}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket)));
                return TCPProcessCmdResults.RESULT_FAILED;
            }

//             KPlayer client = KTPlayerManager.Find(socket);
//             if (null == client)
//             {
//                 LogManager.WriteLog(LogTypes.Error, string.Format("根据RoleID定位GameClient对象失败, CMD={0}, Client={1}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket)));
//                 return TCPProcessCmdResults.RESULT_FAILED;
//             }

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

                TCPOutPacket tcpOutPacket = TCPOutPacketPool.getInstance().Pop();
                tcpOutPacket.PacketCmdID = cmd;
                tcpOutPacket.FinalWriteData(bytesData, 6, length - 2);

                //client.SendPacket(tcpOutPacket);
                TCPManager.getInstance().MySocketListener.SendData(socket, tcpOutPacket);
                return TCPProcessCmdResults.RESULT_OK;
            }
            catch (Exception ex)
            {
                // 格式化异常错误信息
                DataHelper.WriteFormatExceptionLog(ex, Global.GetDebugHelperInfo(socket), false);
            }


            return TCPProcessCmdResults.RESULT_FAILED;
        }

        /// <summary>
        /// 本地处理
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="nID"></param>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public TCPProcessCmdResults dispathProcessor(TMSKSocket socket, int nID, byte[] data, int count) 
        {
            string cmdData = null;
            CmdHandler tcpCmdHandler;

            try
            {
                if ((tcpCmdHandler = GetCmdHandler(nID)) == null)
                {
                    return TCPProcessCmdResults.RESULT_UNREGISTERED;
                };

                //根据socket获取GameClient
                KPlayer client = KTPlayerManager.Find(socket);
                if (null == client)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("根据RoleID定位GameClient对象失败, CMD={0}, Client={1}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket)));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                ICmdProcessorEx cmdProcessorEx = (tcpCmdHandler.CmdProcessor as ICmdProcessorEx);
                if ((tcpCmdHandler.CmdFlags & (int)TCPCmdFlags.IsStringArrayParams) > 0)
                {
                    cmdData = new UTF8Encoding().GetString(data, 0, count);
                    string[] cmdParams = cmdData.Split(':');
                    if (cmdParams.Length < tcpCmdHandler.MinParamCount || cmdParams.Length > tcpCmdHandler.MaxParamCount)
                    {
                        LogManager.WriteLog(LogTypes.Error, string.Format("指令参数个数错误, CMD={0}, Client={1}, Recv={2}",
                            (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), cmdParams.Length));
                        return TCPProcessCmdResults.RESULT_FAILED;
                    }

                    if (null == cmdProcessorEx)
                    {
                        if (!tcpCmdHandler.CmdProcessor.processCmd(client, cmdParams))
                        {
                            return TCPProcessCmdResults.RESULT_FAILED;
                        }
                    }
                    else
                    {
                        if (!cmdProcessorEx.processCmdEx(client, nID, data, cmdParams))
                        {
                            return TCPProcessCmdResults.RESULT_FAILED;
                        }
                    }

                    return TCPProcessCmdResults.RESULT_OK;
                }
                else
                {
                    if (null == cmdProcessorEx)
                    {
                        return TCPProcessCmdResults.RESULT_FAILED;
                    }

                    if (!cmdProcessorEx.processCmdEx(client, nID, data, null))
                    {
                        return TCPProcessCmdResults.RESULT_FAILED;
                    }

                    return TCPProcessCmdResults.RESULT_OK;
                }
            }
            catch (Exception ex)
            {
                // 格式化异常错误信息
                DataHelper.WriteFormatExceptionLog(ex, Global.GetDebugHelperInfo(socket), false);
            }

            return TCPProcessCmdResults.RESULT_FAILED;
        }

    }
}
