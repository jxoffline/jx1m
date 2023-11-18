using Server.Data;
using System.Threading;

namespace GameServer.Logic
{
    /// <summary>
    /// Quản lý giao dịch
    /// </summary>
    public static partial class KTTradeManager
    {
        #region Quản lý ID
        /// <summary>
        /// ID tự động
        /// </summary>
        private static long baseAutoID = 0;

        /// <summary>
        /// Trả về ID phiên giao dịch mới
        /// </summary>
        /// <returns></returns>
        public static int GetNewID()
        {
            return (int) (Interlocked.Increment(ref KTTradeManager.baseAutoID) & 0x7fffffff);
        }
        #endregion

        #region Thêm
        /// <summary>
        /// Thêm giao dịch mới
        /// </summary>
        /// <param name="id"></param>
        /// <param name="ed"></param>
        public static void Add(int id, ExchangeData ed)
        {
            KTTradeManager.sessions[id] = ed;
        }
        #endregion

        #region Xóa
        /// <summary>
        /// Xóa giao dịch tương ứng
        /// </summary>
        /// <param name="id"></param>
        public static void Remove(int id)
        {
            KTTradeManager.sessions.TryRemove(id, out _);
        }
        #endregion

        #region Tìm kiếm
        /// <summary>
        /// Tìm phiên giao dịch tương ứng
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static ExchangeData Find(int id)
        {
            /// Nếu tồn tại
            if (KTTradeManager.sessions.TryGetValue(id, out ExchangeData ed))
            {
                /// Trả về kết quả
                return ed;
            }
            /// Không tìm thấy
            return null;
        }
        #endregion
    }
}
