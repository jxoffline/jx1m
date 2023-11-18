using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using KF.Contract.Data;

namespace KF.Contract.Interface
{
    public interface ICoupleWishService
    {
        int CoupleWishWishRole(CoupleWishWishRoleReq req);
        List<CoupleWishWishRecordData> CoupleWishGetWishRecord(int roleId);
        CoupleWishSyncData CoupleWishSyncCenterData(DateTime oldThisWeek, DateTime oldLastWeek, DateTime oldStatue);
        int CoupleWishPreDivorce(int man, int wife);
        void CoupleWishReportCoupleStatue(CoupleWishReportStatueData req);
        int CoupleWishAdmire(int fromRole, int fromZone, int admireType, int toCoupleId);
        int CoupleWishJoinParty(int fromRole, int fromZone, int toCoupleId);
    }
}
