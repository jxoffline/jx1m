using System;
using System.Net;
using System.Windows;
//using System.Windows.Controls;
//using System.Windows.Documents;
//using System.Windows.Ink;
//using System.Windows.Input;
//using System.Windows.Shapes;
using System.Runtime.InteropServices;

namespace HSGameEngine.Tools.AStarEx
{
    public class ANode
    {
		/** 节点列号 */
		public int x;
		
		/** 节点行号 */
		public int y;
		
		public ANode(int x, int y)
		{
			this.x = x;
			this.y = y;
		}		

        /// <summary>
        /// 返回唯一标识，主要用于binaryStack的辞典
        /// </summary>
        /// <returns></returns>
        public static long GetGUID(int key1, int key2)
        {
            long lKey1 = key1;
            long lKey2 = key2;
            return (lKey1 << 32) | lKey2;
        }

        public static int GetGUID_X(long val)
        {
            return (int)((val >> 32) & 0x00000000FFFFFFFFL);
        }

        public static int GetGUID_Y(long val)
        {
            return (int)(val & 0x00000000FFFFFFFFL);
        }
    }
}
