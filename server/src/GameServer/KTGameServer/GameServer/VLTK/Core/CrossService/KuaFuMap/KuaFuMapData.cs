using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using Server.Data;
using KF.Contract.Data;
using Tmsk.Contract;

namespace GameServer.Logic
{
  
    public class KuaFuMapData
    {
       
        public object Mutex = new object();

     
        public ConcurrentDictionary<IntPairKey, KuaFuLineData> LineMap2KuaFuLineDataDict = new ConcurrentDictionary<IntPairKey, KuaFuLineData>();

        public ConcurrentDictionary<IntPairKey, KuaFuLineData> ServerMap2KuaFuLineDataDict = new ConcurrentDictionary<IntPairKey, KuaFuLineData>();

  
        public ConcurrentDictionary<int, List<KuaFuLineData>> KuaFuMapServerIdDict = new ConcurrentDictionary<int, List<KuaFuLineData>>();

     
        public ConcurrentDictionary<int, List<KuaFuLineData>> MapCode2KuaFuLineDataDict = new ConcurrentDictionary<int, List<KuaFuLineData>>();
    }
}
