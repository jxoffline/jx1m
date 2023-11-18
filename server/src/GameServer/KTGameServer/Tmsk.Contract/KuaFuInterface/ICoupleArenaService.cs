using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using KF.Contract.Data;

namespace KF.Contract.Interface
{
    public interface ICoupleArenaService
    {
        int CoupleArenaJoin(int roleId1, int roleId2, int serverId);
        int CoupleArenaQuit(int roleId1, int roleId2);
        CoupleArenaSyncData CoupleArenaSync(DateTime lastSyncTime);
        int CoupleArenaPreDivorce(int roleId1, int roleId2);
        CoupleArenaFuBenData GetFuBenData(long gameId);
        CoupleArenaPkResultRsp CoupleArenaPkResult(CoupleArenaPkResultReq req);
    }
}
