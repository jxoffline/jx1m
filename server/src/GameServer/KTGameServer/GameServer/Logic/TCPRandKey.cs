using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameServer.Logic
{
    /// <summary>
    /// 通过一个指定的随机数种子来构建指定数据的随机key
    /// </summary>
    public class TCPRandKey
    {
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="capacity"></param>
        public TCPRandKey(int capacity)
        {
            ListRandKey = new List<Int32>(capacity);
            DictRandKey = new Dictionary<Int32, bool>(capacity);
        }

        /// <summary>
        /// 随机数发生器
        /// </summary>
        private Random Rand = null;

        /// <summary>
        /// 保存随机的密码
        /// </summary>
        private List<Int32> ListRandKey = null;

        /// <summary>
        /// 快速访问密码是否存在
        /// </summary>
        private Dictionary<Int32, bool> DictRandKey = null;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="count"></param>
        public void Init(int count, int randSeed )
        {
            Int32 key = 0;
            Rand = new Random(randSeed);
            for (int i = 0; i < count; i++)
            {
                key = Rand.Next(0, Int32.MaxValue);
                ListRandKey.Add(key);
                DictRandKey.Add(key, true);
            }
        }

        /// <summary>
        /// 查找指定的Key是否存在
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool FindKey(Int32 key)
        {
            return DictRandKey.ContainsKey(key);
        }

        /// <summary>
        /// 随机获取一个Key
        /// </summary>
        /// <returns></returns>
        public Int32 GetKey()
        {
            int randIndex = Rand.Next(0, ListRandKey.Count);
            return ListRandKey[randIndex];
        }
    }
}
