using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Tmsk.Contract
{
    /// <summary>
    /// 功能模块管理器接口
    /// </summary>
    public interface IManager2
    {
        bool initialize(ICoreInterface coreInterface);

        bool startup();

        bool showdown();

        bool destroy();
    }
}
