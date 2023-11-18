using Server.Data;
using System.Collections.Generic;

namespace GameServer.Logic
{

    public class SaleGoodsItem
    {

        public int GoodsDbID = 0;

        public GoodsData SalingGoodsData = null;


        public KPlayer Client = null;
    }


    public class SaleGoodsManager
    {
        private static List<SaleGoodsData> _SaleGoodsDataList = null;


        private static Dictionary<int, SaleGoodsItem> _SaleGoodsDict = new Dictionary<int, SaleGoodsItem>();


        public static void AddSaleGoodsItem(SaleGoodsItem saleGoodsItem)
        {
            SaleManager.AddSaleGoodsItem(saleGoodsItem);
            lock (_SaleGoodsDict)
            {
                _SaleGoodsDict[saleGoodsItem.GoodsDbID] = saleGoodsItem;
                _SaleGoodsDataList = null; //强迫刷新
            }
        }




        public static SaleGoodsItem RemoveSaleGoodsItem(int goodsDbID)
        {
            SaleManager.RemoveSaleGoodsItem(goodsDbID);
            lock (_SaleGoodsDict)
            {
                SaleGoodsItem saleGoodsItem = null;
                if (_SaleGoodsDict.TryGetValue(goodsDbID, out saleGoodsItem))
                {
                    _SaleGoodsDict.Remove(goodsDbID);
                }

                _SaleGoodsDataList = null; //强迫刷新
                return saleGoodsItem;
            }
        }


        public static void RemoveSaleGoodsItems(KPlayer client)
        {
            List<GoodsData> goodsDataList = client.SaleGoodsDataList;
            if (null != goodsDataList)
            {
                lock (goodsDataList)
                {
                    for (int i = 0; i < goodsDataList.Count; i++)
                    {
                        RemoveSaleGoodsItem(goodsDataList[i].Id);
                    }
                }
            }
        }


        public static List<SaleGoodsData> GetSaleGoodsDataList()
        {
            lock (_SaleGoodsDict)
            {
                if (null != _SaleGoodsDataList)
                {
                    return _SaleGoodsDataList; //防止频繁计算
                }

                List<SaleGoodsData> saleGoodsDataList = new List<SaleGoodsData>();

                foreach (var saleGoodsItem in _SaleGoodsDict.Values)
                {
                    saleGoodsDataList.Add(new SaleGoodsData()
                    {
                        GoodsDbID = saleGoodsItem.GoodsDbID,
                        SalingGoodsData = saleGoodsItem.SalingGoodsData,
                        RoleID = saleGoodsItem.Client.RoleID,
                        RoleName = saleGoodsItem.Client.RoleName,
                        RoleLevel = saleGoodsItem.Client.m_Level,
                    });

                    if (saleGoodsDataList.Count >= (int)SaleGoodsConsts.MaxReturnNum)
                    {
                        break;
                    }
                }

                _SaleGoodsDataList = saleGoodsDataList;
                return saleGoodsDataList;
            }
        }


        public static List<SaleGoodsData> FindSaleGoodsDataList(Dictionary<int, bool> goodsIDDict)
        {
            lock (_SaleGoodsDict)
            {
                List<SaleGoodsData> saleGoodsDataList = new List<SaleGoodsData>();

                foreach (var saleGoodsItem in _SaleGoodsDict.Values)
                {
                    if (!goodsIDDict.ContainsKey(saleGoodsItem.SalingGoodsData.GoodsID))
                    {
                        continue; //跳过
                    }

                    saleGoodsDataList.Add(new SaleGoodsData()
                    {
                        GoodsDbID = saleGoodsItem.GoodsDbID,
                        SalingGoodsData = saleGoodsItem.SalingGoodsData,
                        RoleID = saleGoodsItem.Client.RoleID,
                        RoleName = saleGoodsItem.Client.RoleName,
                        RoleLevel = saleGoodsItem.Client.m_Level,
                    });

                    if (saleGoodsDataList.Count >= (int)SaleGoodsConsts.MaxReturnNum)
                    {
                        break;
                    }
                }

                return saleGoodsDataList;
            }
        }


        public static List<SaleGoodsData> FindSaleGoodsDataListByRoleName(string searchText)
        {
            lock (_SaleGoodsDict)
            {
                List<SaleGoodsData> saleGoodsDataList = new List<SaleGoodsData>();

                foreach (var saleGoodsItem in _SaleGoodsDict.Values)
                {
                    if (-1 == saleGoodsItem.Client.RoleName.IndexOf(searchText))
                    {
                        continue;
                    }

                    saleGoodsDataList.Add(new SaleGoodsData()
                    {
                        GoodsDbID = saleGoodsItem.GoodsDbID,
                        SalingGoodsData = saleGoodsItem.SalingGoodsData,
                        RoleID = saleGoodsItem.Client.RoleID,
                        RoleName = saleGoodsItem.Client.RoleName,
                        RoleLevel = saleGoodsItem.Client.m_Level,
                    });

                    if (saleGoodsDataList.Count >= (int)SaleGoodsConsts.MaxReturnNum)
                    {
                        break;
                    }
                }

                return saleGoodsDataList;
            }
        }


    }
}
