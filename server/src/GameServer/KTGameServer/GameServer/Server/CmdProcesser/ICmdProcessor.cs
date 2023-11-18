using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GameServer.Logic;
using Server.Protocol;

namespace GameServer.Server
{
    public interface ICmdProcessor
    {
        bool processCmd(KPlayer client, string[] cmdParams);
    }

    public interface ICmdProcessorEx : ICmdProcessor
    {
        bool processCmdEx(KPlayer client, int nID, byte[] bytes, string[] cmdParams);
    }
}
