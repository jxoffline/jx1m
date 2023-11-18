using System.Collections.Generic;
using Server.Data;

namespace FS.GameEngine.Logic
{
    /// <summary>
    /// Dữ liệu máy chủ
    /// </summary>
    public class XuanFuServerData
    {
        /// <summary>
        /// Máy chủ đề cử
        /// </summary>
        public BuffServerInfo RecommendServer { get; set; }
        /// <summary>
        /// Máy chủ lần trước
        /// </summary>
        public BuffServerInfo LastServer { get; set; }
        /// <summary>
        /// Danh sách máy chủ đã vào
        /// </summary>
        public List<BuffServerInfo> RecordServerInfos { get; set; }
        /// <summary>
        /// Danh sách máy chủ đề cử
        /// </summary>
        public List<BuffServerInfo> RecommendServerInfos { get; set; }
        /// <summary>
        /// Danh sách máy chủ
        /// </summary>
        public List<BuffServerInfo> ServerInfos { get; set; }
    }
}
