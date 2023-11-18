using GameServer.Core.Executor;
using Server.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Tmsk.Contract;
using Tmsk.Contract.Data;
using Tmsk.Tools;

namespace KF.Remoting
{
    /// <summary>
    /// 平台充值王
    /// 用于GameServer <---> 跨服中心 <---> 后台中心
    /// 这个功能先寄宿在跨服多人副本中，所有功能写在这里，以后要拆分单独的service也比较方便
    /// </summary>
    class PlatChargeKingManager
    {
        private object Mutex = new object();
        private InputKingPaiHangDataEx rankEx = null;
        private bool bHasVisitor = false;

        public void Update()
        {
            if (!IsNeedDownload())
            {
                return;
            }


            try
            {
                ClientServerListData clientListData = new ClientServerListData();
                clientListData.lTime = TimeUtil.NOW();
                clientListData.strMD5 = MD5Helper.get_md5_string(ConstData.HTTP_MD5_KEY + clientListData.lTime.ToString());
                byte[] clientBytes = DataHelper2.ObjectToBytes<ClientServerListData>(clientListData);
                byte[] responseData = WebHelper.RequestByPost(KuaFuServerManager.GetPlatChargeKingUrl, clientBytes, 2000, 30000);
                if (responseData == null)
                {
                    return;
                }

                InputKingPaiHangDataEx tmpRankEx = DataHelper2.BytesToObject<InputKingPaiHangDataEx>(responseData, 0, responseData.Length);

                if (tmpRankEx != null)
                {
                    rankEx = tmpRankEx;
                }
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Error, "PlatChargeKingManager.Update exception", ex);
            }
        }


        public InputKingPaiHangDataEx GetRankEx()
        {
            InputKingPaiHangDataEx result = null;

            lock (Mutex)
            {
                bHasVisitor = true;
                result = rankEx;
            }

            return result;
        }


        private bool IsNeedDownload()
        {
            if (!bHasVisitor)
            {
                return false;
            }

            return true;
        }
    }
}
