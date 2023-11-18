using GameServer.Logic;
using GameServer.Server;
using Server.TCP;
using Server.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Tmsk.Contract;

namespace GameServer.Tools
{
    public class CheckHelper
    {
        /// <summary>
        /// 验证xml加载，并返回
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="xmlItems"></param>
        /// <returns></returns>
        public static XElement LoadXml(string filePath, bool isFatal = true)
        {
            if(!File.Exists(filePath))
            {
                if(isFatal)
                    LogManager.WriteLog(LogTypes.Fatal, string.Format("加载[{0}]时出错!!!文件不存在", filePath));
                else
                    LogManager.WriteLog(LogTypes.Error, string.Format("加载[{0}]时出错!!!文件不存在", filePath));

                return null;
            }

            XElement xml = XElement.Load(filePath);
            return xml;
        }


        /// <summary>
        /// TCPCmdHandler消息验证
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="nID"></param>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <param name="fields"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static bool CheckTCPCmdFields(TMSKSocket socket, int nID, byte[] data, int count, string[] fields, int length)
        {
            string cmdData = null;

            try
            {
                cmdData = new UTF8Encoding().GetString(data, 0, count);
            }
            catch (Exception) //解析错误
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("解析指令字符串错误, CMD={0}, Client={1}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket)));
                return false;
            }

            fields = cmdData.Split(':');
            if (fields.Length != length)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("指令参数个数错误, CMD={0}, Client={1}, Recv={2}",
                    (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), fields.Length));
                return false;
            }

            return true;
        }

        /// <summary>
        /// TCPCmdHandler消息验证
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="nID"></param>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <param name="cmdData"></param>
        /// <returns></returns>
        public static bool CheckTCPCmdHandle<T>(TMSKSocket socket, int nID, byte[] data, int count, out T cmdData) where T : class, IProtoBuffData, new()
        {
            cmdData = null;

            try
            {
                cmdData = DataHelper.BytesToObject2<T>(data, 0, count, socket.m_Socket);
            }
            catch (Exception) //解析错误
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("解析指令字符串错误, CMD={0}, Client={1}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket)));
                return false;
            }

            if (null == cmdData)
            {
                //LogManager.WriteLog(LogTypes.Error, string.Format("指令参数个数错误, CMD={0}, Client={1}",
                //    (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket)));
                return false;
            }

            return true;
        }

        /// <summary>
        /// 消息验证——长度，roleID
        /// </summary>
        /// <param name="client"></param>
        /// <param name="nID"></param>
        /// <param name="cmdParams"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static bool CheckCmdLengthAndRole(KPlayer client, int nID, string[] cmdParams, int length)
        {
            if (CheckCmdLength(client, nID, cmdParams, length))
                return CheckCmdRole(client, nID, cmdParams);

            return false;
        }

        /// <summary>
        /// 消息验证——长度，userID
        /// </summary>
        /// <param name="client"></param>
        /// <param name="nID"></param>
        /// <param name="cmdParams"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static bool CheckCmdLengthAndUser(KPlayer client, int nID, string[] cmdParams, int length)
        {
            if (CheckCmdLength(client, nID, cmdParams, length))
                return CheckCmdUser(client, nID, cmdParams);

            return false;
        }

        /// <summary>
        /// 消息验证——长度
        /// </summary>
        /// <param name="client"></param>
        /// <param name="nID"></param>
        /// <param name="cmdParams"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static bool CheckCmdLength(KPlayer client, int nID, string[] cmdParams, int length)
        {
            if (cmdParams.Length != length)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("指令参数个数错误, CMD={0}, Client={1}, Recv={2}",
                    (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(client.ClientSocket), cmdParams.Length));
                return false;
            }

            return true;
        }

        /// <summary>
        /// 消息验证——roleID
        /// </summary>
        /// <param name="client"></param>
        /// <param name="nID"></param>
        /// <param name="cmdParams"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static bool CheckCmdRole(KPlayer client, int nID, string[] cmdParams)
        {
            int roleID = int.Parse(cmdParams[0]);
            if (null == client || client.RoleID != roleID)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("根据RoleID定位GameClient对象失败, CMD={0}, Client={1}, RoleID={2}",
                    (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(client.ClientSocket), roleID));
                return false;
            }

            return true;
        }

        /// <summary>
        /// 消息验证——userID
        /// </summary>
        /// <param name="client"></param>
        /// <param name="nID"></param>
        /// <param name="cmdParams"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static bool CheckCmdUser(KPlayer client, int nID, string[] cmdParams)
        {
            string userID = cmdParams[0];
            if (null == client || client.strUserID != userID)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("根据userID定位GameClient对象失败, CMD={0}, Client={1}, userID={2}",
                    (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(client.ClientSocket), userID));
                return false;
            }

            return true;
        }




    }
}
