using System;
using System.Collections.Generic;
using System.Windows;
using System.Linq;
using System.Text;
using Server.TCP;
using Server.Protocol;
using GameServer;
using Server.Data;
using GameServer.Server;
using GameServer.Core.Executor;

namespace GameServer.Logic
{
    /// <summary>
    /// 出售的物品的管理
    /// </summary>
    public class SaleRoleManager
    {
        /// <summary>
        /// 缓存的数据
        /// </summary>
        private static List<SaleRoleData> _SaleRoleDataList = null;

        /// <summary>
        /// 缓存数据的时间
        /// </summary>
        private static long _SaleRoleDataListTicks = 0;

        /// <summary>
        /// 保存正在出售的角色的词典
        /// </summary>
        private static Dictionary<int, KPlayer> _SaleRoleDict = new Dictionary<int, KPlayer>();

        /// <summary>
        /// 添加出售的角色
        /// </summary>
        /// <param name="dbRoleInfo"></param>
        public static void AddSaleRoleItem(KPlayer client)
        {
            lock (_SaleRoleDict)
            {
                _SaleRoleDict[client.RoleID] = client;
                _SaleRoleDataList = null; //强迫刷新
            }
        }

        /// <summary>
        /// 删除出售的角色项
        /// </summary>
        /// <param name="dbRoleInfo"></param>
        public static KPlayer RemoveSaleRoleItem(int roleID)
        {
            lock (_SaleRoleDict)
            {
                KPlayer client = null;
                if (_SaleRoleDict.TryGetValue(roleID, out client))
                {
                    _SaleRoleDict.Remove(roleID);
                }

                _SaleRoleDataList = null; //强迫刷新
                return client;
            }
        }

        /// <summary>
        /// 获取所有的出售的物品的列表(有返回的最大数限制)
        /// </summary>
        /// <returns></returns>
        public static List<SaleRoleData> GetSaleRoleDataList()
        {
            long ticks = TimeUtil.NOW();
            lock (_SaleRoleDict)
            {
                //if (null != _SaleRoleDataList)
                //{
                //    if (ticks - _SaleRoleDataListTicks < (30 * 1000))
                //    {
                //        return _SaleRoleDataList; //防止频繁计算
                //    }
                //}

                List<SaleRoleData> saleRoleDataList = new List<SaleRoleData>();

                foreach (var client in _SaleRoleDict.Values)
                {
                    int saleGoodsNum = (null == client.SaleGoodsDataList ? 0 : client.SaleGoodsDataList.Count);
                    if (saleGoodsNum <= 0)
                    {
                        continue;
                    }

                    saleRoleDataList.Add(new SaleRoleData()
                    {
                        RoleID = client.RoleID,
                        RoleName = client.RoleName,
                        RoleLevel = client.m_Level,
                        SaleGoodsNum = saleGoodsNum,
                    });

                    if (saleRoleDataList.Count >= (int)SaleGoodsConsts.MaxReturnNum)
                    {
                        break;
                    }
                }

                _SaleRoleDataList = saleRoleDataList;
                _SaleRoleDataListTicks = ticks;
                return saleRoleDataList;
            }            
        }
    }
}
