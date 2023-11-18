using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tmsk.Contract.Const
{
    public class ServerFlags
    {
        public const int NormalServerOnly = 0;
        public const int All = 2^32 - 1;
        public const int KuaFuServer = 1;
        public const int NormalServer = 2;
    }
}
