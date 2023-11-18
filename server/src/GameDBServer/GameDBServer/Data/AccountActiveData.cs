using GameDBServer.MySqlHelpLib;

namespace GameDBServer.Data
{

    public class AccountActiveData
    {

        [DBMapping(ColumnName = "Account")]
        public string strAccount;


        [DBMapping(ColumnName = "createTime")]
        public string strCreateTime;


        [DBMapping(ColumnName = "seriesLoginCount")]
        public int nSeriesLoginCount;


        [DBMapping(ColumnName = "lastSeriesLoginTime")]
        public string strLastSeriesLoginTime;
    }
}