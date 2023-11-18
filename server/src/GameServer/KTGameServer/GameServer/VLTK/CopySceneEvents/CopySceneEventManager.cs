using GameServer.KiemThe.CopySceneEvents.DynamicArena;
using GameServer.KiemThe.CopySceneEvents.MilitaryCampFuBen;
using GameServer.KiemThe.CopySceneEvents.Model;
using GameServer.KiemThe.Logic;
using GameServer.Logic;
using Server.Tools;
using System;
using System.Collections.Concurrent;
using static GameServer.Logic.KTMapManager;

namespace GameServer.KiemThe.CopySceneEvents
{
    /// <summary>
    /// Quản lý phụ bản
    /// </summary>
    public static partial class CopySceneEventManager
    {
        /// <summary>
        /// Danh sách các phụ bản đang thực thi
        /// </summary>
        private static readonly ConcurrentDictionary<int, CopySceneEvent> CopyScenes = new ConcurrentDictionary<int, CopySceneEvent>();

        #region Core

        /// <summary>
        /// Giới hạn số phụ bản tối đa
        /// </summary>
        public const int LimitCopyScenes = 200;

        /// <summary>
        /// Số lượng phụ bản hiện có
        /// </summary>
        public static int CurrentCopyScenesCount { get; private set; } = 0;

        /// <summary>
        /// Khởi tạo
        /// </summary>
        public static void Init()
        {
            ///// Bí Cảnh
            //MiJing.Init();
            ///// Du Long Các
            //YouLong.Init();
            /// Quân doanh
            MilitaryCamp.Init();
            /// Lôi đài di động
            DynamicArena.DynamicArena.Init();
            /// Thần Bí Bảo Khố
            ShenMiBaoKu.ShenMiBaoKu.Init();
        }

        #endregion Core

        #region Public methods

        /// <summary>
        /// Thêm phụ bản tương ứng vào danh sách
        /// </summary>
        /// <param name="copySceneEvent"></param>
        public static void Add(CopySceneEvent copySceneEvent)
        {
            CopySceneEventManager.CopyScenes[copySceneEvent.CopyScene.ID] = copySceneEvent;
            /// Tăng số lượng phụ bản hiện có lên
            CopySceneEventManager.CurrentCopyScenesCount++;
        }

        /// <summary>
        /// Xóa phụ bản tương ứng khỏi danh sách
        /// </summary>
        /// <param name="copySceneEvent"></param>
        public static void Remove(CopySceneEvent copySceneEvent)
        {
            CopySceneEventManager.CopyScenes.TryRemove(copySceneEvent.CopyScene.ID, out _);
            /// Giảm số lượng phụ bản hiện có xuống
            CopySceneEventManager.CurrentCopyScenesCount--;
        }

        /// <summary>
        /// Kiểm tra phụ bản ID tương ứng có tồn tại trong hệ thống không
        /// </summary>
        /// <param name="copySceneID"></param>
        /// <param name="copySceneCreateTicks"></param>
        /// <returns></returns>
        public static bool IsCopySceneExist(int copySceneID, int mapCode, int copySceneCreateTicks = -1)
        {
            if (CopySceneEventManager.CopyScenes.TryGetValue(copySceneID, out CopySceneEvent script))
            {
                if (copySceneCreateTicks == -1)
                {
                    return script.CopyScene.MapCode == mapCode;
                }
                else
                {
                    return script.CopyScene.MapCode == mapCode && copySceneCreateTicks == script.CopyScene.InitTicks;
                }
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Trả về thông tin phụ bản tương ứng
        /// </summary>
        /// <param name="copySceneID"></param>
        /// <param name="mapCode"></param>
        /// <returns></returns>
        public static KTCopyScene GetCopyScene(int copySceneID, int mapCode)
        {
            if (CopySceneEventManager.CopyScenes.TryGetValue(copySceneID, out CopySceneEvent script))
            {
                if (script.CopyScene.MapCode == mapCode)
                {
                    return script.CopyScene;
                }
            }
            return null;
        }

        /// <summary>
        /// Trả về thông tin Script điều khiển phụ bản tương ứng
        /// </summary>
        /// <param name="copySceneID"></param>
        /// <param name="mapCode"></param>
        /// <returns></returns>
        public static CopySceneEvent GetCopySceneScript(int copySceneID, int mapCode)
        {
            if (CopySceneEventManager.CopyScenes.TryGetValue(copySceneID, out CopySceneEvent script))
            {
                if (script.CopyScene.MapCode == mapCode)
                {
                    return script;
                }
            }
            return null;
        }

        #endregion Public methods

        #region Events

        /// <summary>
        /// Sự kiện khi người chơi vào bản đồ phụ bản
        /// </summary>
        /// <param name="copyScene"></param>
        /// <param name="player"></param>
        public static void OnPlayerEnter(KTCopyScene copyScene, KPlayer player)
        {
            /// Nếu toác
            if (copyScene == null)
            {
                return;
            }
            /// Nếu tồn tại
            if (CopySceneEventManager.CopyScenes.TryGetValue(copyScene.ID, out CopySceneEvent script))
            {
                try
                {
                    /// Thực thi sự kiện tương ứng
                    script.OnPlayerEnter(player);
                }
                catch (Exception ex)
                {
                    LogManager.WriteLog(LogTypes.Exception, ex.ToString());
                }
            }
        }

        /// <summary>
        /// Sự kiện khi người chơi vào bản đồ phụ bản
        /// </summary>
        /// <param name="copyScene"></param>
        /// <param name="player"></param>
        /// <param name="toMap"></param>
        public static void OnPlayerLeave(KTCopyScene copyScene, KPlayer player, GameMap toMap)
        {
            /// Nếu toác
            if (copyScene == null)
            {
                return;
            }
            /// Nếu tồn tại
            if (CopySceneEventManager.CopyScenes.TryGetValue(copyScene.ID, out CopySceneEvent script))
            {
                try
                {
                    /// Thực thi sự kiện tương ứng
                    script.OnPlayerLeave(player, toMap);
                }
                catch (Exception ex)
                {
                    LogManager.WriteLog(LogTypes.Exception, ex.ToString());
                }
            }
        }

        /// <summary>
        /// Sự kiện khi người chơi gây sát thương cho đối tượng khác trong phụ bản
        /// </summary>
        /// <param name="copyScene"></param>
        /// <param name="player"></param>
        /// <param name="obj"></param>
        /// <param name="damage"></param>
        public static void OnHitTarget(KTCopyScene copyScene, KPlayer player, GameObject obj, int damage)
        {
            /// Nếu toác
            if (copyScene == null)
            {
                return;
            }
            /// Nếu tồn tại
            if (CopySceneEventManager.CopyScenes.TryGetValue(copyScene.ID, out CopySceneEvent script))
            {
                try
                {
                    /// Thực thi sự kiện tương ứng
                    script.OnHitTarget(player, obj, damage);
                }
                catch (Exception ex)
                {
                    LogManager.WriteLog(LogTypes.Exception, ex.ToString());
                }
            }
        }

        /// <summary>
        /// Sự kiện khi người chơi giết đối tượng khác trong phụ bản
        /// </summary>
        /// <param name="copyScene"></param>
        /// <param name="player"></param>
        /// <param name="obj"></param>
        public static void OnKillObject(KTCopyScene copyScene, KPlayer player, GameObject obj)
        {
            /// Nếu toác
            if (copyScene == null)
            {
                return;
            }
            /// Nếu tồn tại
            if (CopySceneEventManager.CopyScenes.TryGetValue(copyScene.ID, out CopySceneEvent script))
            {
                try
                {
                    /// Thực thi sự kiện tương ứng
                    script.OnKillObject(player, obj);
                }
                catch (Exception ex)
                {
                    LogManager.WriteLog(LogTypes.Exception, ex.ToString());
                }
            }
        }

        /// <summary>
        /// Sự kiện khi người chơi chết bởi đối tượng khác trong phụ bản
        /// </summary>
        /// <param name="copyScene"></param>
        /// <param name="killer"></param>
        /// <param name="player"></param>
        public static void OnPlayerDie(KTCopyScene copyScene, GameObject killer, KPlayer player)
        {
            /// Nếu toác
            if (copyScene == null)
            {
                return;
            }
            /// Nếu tồn tại
            if (CopySceneEventManager.CopyScenes.TryGetValue(copyScene.ID, out CopySceneEvent script))
            {
                try
                {
                    /// Thực thi sự kiện tương ứng
                    script.OnPlayerDie(killer, player);
                }
                catch (Exception ex)
                {
                    LogManager.WriteLog(LogTypes.Exception, ex.ToString());
                }
            }
        }

        /// <summary>
        /// Sự kiện khi người chơi hồi sinh trong phụ bản
        /// </summary>
        /// <param name="copyScene"></param>
        /// <param name="player"></param>
        public static void OnPlayerRelive(KTCopyScene copyScene, KPlayer player)
        {
            /// Nếu toác
            if (copyScene == null)
            {
                return;
            }
            /// Nếu tồn tại
            if (CopySceneEventManager.CopyScenes.TryGetValue(copyScene.ID, out CopySceneEvent script))
            {
                try
                {
                    /// Thực thi sự kiện tương ứng
                    script.OnPlayerRelive(player);
                }
                catch (Exception ex)
                {
                    LogManager.WriteLog(LogTypes.Exception, ex.ToString());
                }
            }
        }

        /// <summary>
        /// Sự kiện khi người chơi mất kết nối
        /// </summary>
        /// <param name="copyScene"></param>
        /// <param name="player"></param>
        public static void OnPlayerDisconnected(KTCopyScene copyScene, KPlayer player)
        {
            /// Nếu toác
            if (copyScene == null)
            {
                return;
            }
            /// Nếu tồn tại
            if (CopySceneEventManager.CopyScenes.TryGetValue(copyScene.ID, out CopySceneEvent script))
            {
                try
                {
                    /// Thực thi sự kiện tương ứng
                    script.OnPlayerDisconnected(player);
                }
                catch (Exception ex)
                {
                    LogManager.WriteLog(LogTypes.Exception, ex.ToString());
                }
            }
        }

        /// <summary>
        /// Sự kiện khi người chơi kết nối lại
        /// </summary>
        /// <param name="copyScene"></param>
        /// <param name="player"></param>
        public static void OnPlayerReconnected(KTCopyScene copyScene, KPlayer player)
        {
            /// Nếu toác
            if (copyScene == null)
            {
                return;
            }
            /// Nếu tồn tại
            if (CopySceneEventManager.CopyScenes.TryGetValue(copyScene.ID, out CopySceneEvent script))
            {
                try
                {
                    /// Thực thi sự kiện tương ứng
                    script.OnPlayerReconnected(player);
                }
                catch (Exception ex)
                {
                    LogManager.WriteLog(LogTypes.Exception, ex.ToString());
                }
            }
        }

        #endregion Events
    }
}