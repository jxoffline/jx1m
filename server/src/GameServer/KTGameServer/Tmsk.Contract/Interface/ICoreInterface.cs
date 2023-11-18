using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tmsk.Contract
{
    public interface ICoreInterface
    {
        int GetNewFuBenSeqId();
        int GetLocalServerId();
        ISceneEventSource GetEventSourceInterface();
        string GetGameConfigStr(string name, string defVal);
        PlatformTypes GetPlatformType();
        void SetRuntimeVariable(string name, string val);
        string GetRuntimeVariable(string name, string defVal);
        int GetRuntimeVariable(string name, int defVal);
        string GetLocalAddressIPs();
        int GetMapClientCount(int mapCode);
    }
}
