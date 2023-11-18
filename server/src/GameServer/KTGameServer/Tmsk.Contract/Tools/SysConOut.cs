using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Server.Tools
{
    class SysConOut
    {        
        
        #region 控制台输入的通用接口

        /// <summary>
        /// 控制台输入的锁对象
        /// </summary>
        private static object SystemConsoleOutMutex = new object();

        public static void WriteLine(string value)
        {
            lock (SystemConsoleOutMutex)
            {
                System.Console.WriteLine(value);
            }
        }

        #endregion

    }
}
