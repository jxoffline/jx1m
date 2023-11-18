using System;
using System.Net;
using System.Runtime.InteropServices;

namespace Server.Tools.AStarEx
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
        public static int GetGUID(int key1, int key2)
        {
            int lKey1 = key1;
            int lKey2 = key2;
            return (lKey1 << 16) | lKey2;
        }

        public static int GetGUID_X(int val)
        {
            return (int)((val >> 16) & 0x0000FFFFL);
        }

        public static int GetGUID_Y(int val)
        {
            return (int)(val & 0x0000FFFFL);
        }
    }
}
