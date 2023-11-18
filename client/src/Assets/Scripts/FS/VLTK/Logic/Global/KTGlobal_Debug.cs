using FS.VLTK.Control.Component;
using FS.VLTK.Factory;
using FS.VLTK.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace FS.VLTK
{
    /// <summary>
    /// Các hàm toàn cục dùng trong Kiếm Thế
    /// </summary>
    public static partial class KTGlobal
    {
        #region Debug Objects

        /// <summary>
        /// Hiện khối Debug ở vị trí tương ứng
        /// </summary>
        /// <param name="pos">Vị trí</param>
        /// <param name="size">Kích thước khối</param>
        /// <param name="lifeTime">Thời gian tồn tại</param>
        public static void ShowDebugObjectAtPos(Vector2 pos, int size, float lifeTime)
        {
            DebugObject debugObject = Object2DFactory.MakeDebugObject();
            debugObject.Pos = pos;
            debugObject.Size = size;
            debugObject.LifeTime = lifeTime;
        }

        #endregion Debug Objects

        #region Debug Console

        /// <summary>
        /// Loại Log trong Debug Console
        /// </summary>
        public enum DebugConsoleLogType
        {
            /// <summary>
            /// Log có tiền tố [LOG]
            /// </summary>
            Log,

            /// <summary>
            /// Log có tiền tố [SUCCESS]
            /// </summary>
            Success,

            /// <summary>
            /// Log có tiền tố [INFO]
            /// </summary>
            Info,

            /// <summary>
            /// Log có tiền tố [WARNING]
            /// </summary>
            Warning,

            /// <summary>
            /// Log có tiền tố [ERROR]
            /// </summary>
            Error,

            /// <summary>
            /// Log có tiền tố [EXCEPTION]
            /// </summary>
            Exception,
        }

        /// <summary>
        /// Hiển thị Text ra Debug Console
        /// </summary>
        /// <param name="text"></param>
        public static void WriteDebugConsoleLog(DebugConsoleLogType logType, string text)
        {
            if (UIDebugConsole.Instance == null)
            {
                return;
            }

            switch (logType)
            {
                case DebugConsoleLogType.Log:
                    UIDebugConsole.Instance.WriteLog(text);
                    break;

                case DebugConsoleLogType.Success:
                    UIDebugConsole.Instance.WriteLogSuccess(text);
                    break;

                case DebugConsoleLogType.Info:
                    UIDebugConsole.Instance.WriteLogInfo(text);
                    break;

                case DebugConsoleLogType.Warning:
                    UIDebugConsole.Instance.WriteLogWarning(text);
                    break;

                case DebugConsoleLogType.Error:
                    UIDebugConsole.Instance.WriteLogError(text);
                    break;

                case DebugConsoleLogType.Exception:
                    UIDebugConsole.Instance.WriteLogException(text);
                    break;
            }
        }

        #endregion Debug Console
    }
}
