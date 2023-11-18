using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Logic
{
    /// <summary>
    /// Sự kiện
    /// </summary>
    public partial class Monster
    {
        /// <summary>
        /// Danh sách các biến cục bộ của đối tượng
        /// <para>Sử dụng ở Script Lua</para>
        /// </summary>
        private readonly ConcurrentDictionary<int, long> LocalVariables = new ConcurrentDictionary<int, long>();

        /// <summary>
        /// Trả về biến cục bộ tại vị trí tương ứng của đối tượng
        /// </summary>
        /// <param name="idx"></param>
        /// <returns></returns>
        public long GetLocalParam(int idx)
        {
            if (this.LocalVariables.TryGetValue(idx, out long value))
            {
                return value;
            }
            else
            {
                this.LocalVariables[idx] = 0;
                return 0;
            }
        }

        /// <summary>
        /// Thiết lập biến cục bộ tại vị trí tương ứng của đối tượng
        /// </summary>
        /// <param name="idx"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public void SetLocalParam(int idx, long value)
        {
            this.LocalVariables[idx] = value;
        }

        /// <summary>
        /// Xóa rỗng toàn bộ danh sách biến cục bộ
        /// </summary>
        public void RemoveAllLocalParams()
        {
            this.LocalVariables.Clear();
        }
    }
}
