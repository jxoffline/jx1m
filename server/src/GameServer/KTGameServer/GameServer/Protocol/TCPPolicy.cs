using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Server.Tools;
using GameServer.Tools;

namespace Server.Protocol
{
    /// <summary>
    /// SilverLight策略类
    /// </summary>
    class TCPPolicy
    {
        public static NetTMSK_KY TM = new NetTMSK_KY();
        public const string POLICY_STRING = "<policy-file-request/>";//这个是固定的
        public static byte[] PolicyServerFileContent; //策略文件转化成的字节数组  

        /// <summary>
        /// 加载策略文件
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static void LoadPolicyServerFile(string file)
        {
            //OutPut("(943端口)策略请求监听开始...");
            PolicyServerFileContent = File.ReadAllBytes(file);
            byte[] bytesData = new byte[PolicyServerFileContent.Length + 1];
            DataHelper.CopyBytes(bytesData, 0, PolicyServerFileContent, 0, PolicyServerFileContent.Length);
            bytesData[bytesData.Length - 1] = 0;
            PolicyServerFileContent = bytesData;
        }
    }
}
