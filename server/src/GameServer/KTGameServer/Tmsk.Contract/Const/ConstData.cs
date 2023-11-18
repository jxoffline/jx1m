using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tmsk.Contract
{
    public class ConstData
    {
        public static string HTTP_MD5_KEY = "tmsk_mu_06";

        /// <summary>
        /// 获取副本组队人数上限
        /// </summary>
        public static int CopyRoleMax(int copyID)
        {
            int max = 5;
            switch (copyID)
            {
                case 70000:
                case 70100:
                    max = 5;
                    break;
                case 70200:
                    max = 10;
                    break;
            }

            return max;
        }
    }
}
