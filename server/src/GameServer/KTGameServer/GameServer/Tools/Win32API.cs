using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace System
{
    public static class Win32API
    {
        #region 我的函数

        [DllImport("Tmsk.Tools.dll")]
        public static extern UInt16 GenRandKey(int val);

        [DllImport("Tmsk.Tools.dll")]
        public static extern UInt64 OpenKey(UInt16 randKey, UInt64 oldSortKey);

        [DllImport("Tmsk.Tools.dll")]
        public static extern void CloseKey(IntPtr key);

        [DllImport("Tmsk.Tools.dll")]
        public static extern unsafe void SortBytes(byte* data, int srcIndex, int length, UInt64 key);

        [DllImport("Tmsk.Tools.dll")]
        public static extern UInt16 OpenMagic(UInt16 randKey, UInt16 baseVal);

        #endregion 我的函数
    }
}
