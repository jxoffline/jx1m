using GameServer.KiemThe.Utilities.Algorithms;
using GameServer.Logic;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace GameServer.KiemThe.Logic
{
    /// <summary>
    /// Quản lý quái
    /// </summary>
    public static partial class KTMonsterManager
    {
        /// <summary>
        /// Danh sách quái
        /// </summary>
        private static readonly ConcurrentDictionary<int, Monster> monsters = new ConcurrentDictionary<int, Monster>();

        /// <summary>
        /// Pool quản lý ID quái
        /// </summary>
        private static readonly AutoIndexReusablePattern idPool = new AutoIndexReusablePattern(1000000);

        /// <summary>
        /// Danh sách Template quái
        /// </summary>
        private static readonly Dictionary<int, MonsterTemplateData> templateMonsters = new Dictionary<int, MonsterTemplateData>();

        /// <summary>
        /// Trả về Template quái với Res tương ứng
        /// </summary>
        /// <param name="resID"></param>
        /// <returns></returns>
        public static MonsterTemplateData GetTemplate(int resID)
        {
            /// Nếu tồn tại
            if (KTMonsterManager.templateMonsters.TryGetValue(resID, out MonsterTemplateData data))
            {
                /// Trả về kết quả
                return data;
            }
            /// Không tìm thấy
            return null;
        }
    }
}
