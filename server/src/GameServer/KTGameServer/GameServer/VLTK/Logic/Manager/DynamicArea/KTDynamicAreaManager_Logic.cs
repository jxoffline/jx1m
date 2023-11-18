using GameServer.KiemThe.Core;
using GameServer.KiemThe.LuaSystem;
using GameServer.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using static GameServer.Logic.KTMapManager;

namespace GameServer.KiemThe.Logic.Manager
{
    /// <summary>
    /// Quản lý Logic khu vực động
    /// </summary>
    public static partial class KTDynamicAreaManager
    {
        #region Sự kiện
        /// <summary>
        /// Sự kiện khi đối tượng bắt đầu tiến vào khu vực động
        /// </summary>
        /// <param name="go"></param>
        /// <param name="dynArea"></param>
        public static void ProcessOnEnter(GameObject go, KDynamicArea dynArea)
        {
            /// Bản đồ tương ứng
            GameMap map = KTMapManager.Find(go.CurrentMapCode);
            if (map == null)
            {
                return;
            }

            /// Thực thi Script Lua tương ứng
            if (dynArea.ScriptID != -1)
            {
                /// Thực thi hàm kiểm tra điều kiện ở Script tương ứng
                KTLuaEnvironment.ExecuteDynamicAreaScript_OnEnter(map, go, dynArea, dynArea.ScriptID);
            }
            /// Thực thi sự kiện OnEnter
            dynArea.OnEnter?.Invoke(go);
        }

        /// <summary>
        /// Sự kiện khi đối tượng đang ở trong khu vực động
        /// </summary>
        /// <param name="go"></param>
        /// <param name="dynArea"></param>
        public static void ProcessOnStayTick(GameObject go, KDynamicArea dynArea)
        {
            /// Bản đồ tương ứng
            GameMap map = KTMapManager.Find(go.CurrentMapCode);
            if (map == null)
            {
                return;
            }

            /// Thực thi Script Lua tương ứng
            if (dynArea.ScriptID != -1)
            {
                /// Thực thi hàm kiểm tra điều kiện ở Script tương ứng
                KTLuaEnvironment.ExecuteDynamicAreaScript_OnStayTick(map, go, dynArea, dynArea.ScriptID);
            }

            /// Thực thi sự kiện OnStayTick
            dynArea.OnStayTick?.Invoke(go);
        }

        /// <summary>
        /// Sự kiện khi đối tượng rời khu vực động
        /// </summary>
        /// <param name="go"></param>
        /// <param name="dynArea"></param>
        public static void ProcessOnLeave(GameObject go, KDynamicArea dynArea)
        {
            /// Bản đồ tương ứng
            GameMap map = KTMapManager.Find(go.CurrentMapCode);
            if (map == null)
            {
                return;
            }

            /// Thực thi Script Lua tương ứng
            if (dynArea.ScriptID != -1)
            {
                /// Thực thi hàm kiểm tra điều kiện ở Script tương ứng
                KTLuaEnvironment.ExecuteDynamicAreaScript_OnLeave(map, go, dynArea, dynArea.ScriptID);
            }
            /// Thực thi sự kiện OnStayTick
            dynArea.OnLeave?.Invoke(go);
        }
        #endregion
    }
}
