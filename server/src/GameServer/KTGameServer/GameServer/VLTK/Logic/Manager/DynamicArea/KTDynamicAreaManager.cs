using GameServer.KiemThe.Core;
using System.Collections.Concurrent;

namespace GameServer.KiemThe.Logic.Manager
{
	/// <summary>
	/// Đối tượng quản lý khu vực động
	/// </summary>
	public static partial class KTDynamicAreaManager
    {
        /// <summary>
        /// Danh sách khu vực động
        /// </summary>
        private static readonly ConcurrentDictionary<int, KDynamicArea> dynamicAreas = new ConcurrentDictionary<int, KDynamicArea>();
    }
}
