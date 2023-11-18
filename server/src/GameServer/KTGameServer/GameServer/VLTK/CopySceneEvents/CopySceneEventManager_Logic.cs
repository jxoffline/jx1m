using GameServer.KiemThe.CopySceneEvents.Model;
using GameServer.Logic;
using System.Linq;

namespace GameServer.KiemThe.CopySceneEvents
{
    /// <summary>
    /// Quản lý phụ bản
    /// </summary>
    public static partial class CopySceneEventManager
    {
        /// <summary>
        /// Trả về tổng số lượt tham gia phụ bản trong ngày
        /// </summary>
        /// <param name="player"></param>
        /// <param name="copySceneType"></param>
        public static int GetCopySceneTotalEnterTimesToday(KPlayer player, DailyRecord copySceneType)
        {
            int value = player.GetValueOfDailyRecore((int)copySceneType);
            if (value < 0)
            {
                value = 0;
            }
            return value;
        }

        /// <summary>
        /// Lưu lại thông tin tổng số lượt tham gia phụ bản trong ngày
        /// </summary>
        /// <param name="player"></param>
        /// <param name="copySceneType"></param>
        /// <param name="totalEnterTimes"></param>
        public static void SetCopySceneTotalEnterTimesToday(KPlayer player, DailyRecord copySceneType, int totalEnterTimes)
        {
            player.SetValueOfDailyRecore((int)copySceneType, totalEnterTimes);
        }

        /// <summary>
        /// Trả về tổng số phụ bản có loại tương ứng
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static int GetTotalCopyScene<T>() where T : CopySceneEvent
        {
            return CopySceneEventManager.CopyScenes.Values.Where(x => x.GetType() == typeof(T)).Count();
        }
    }
}