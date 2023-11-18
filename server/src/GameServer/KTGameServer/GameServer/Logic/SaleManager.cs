using System;
using System.Collections.Generic;
using System.Windows;
using System.Linq;
using System.Text;
using Server.TCP;
using Server.Protocol;
using GameServer;
using Server.Data;
using GameServer.Tools;
using GameServer.Server;
using GameServer.Server.CmdProcesser;
using ProtoBuf;
using GameServer.KiemThe.Core.Item;

namespace GameServer.Logic
{
    #region 相关结构定义

    public enum SearchOrderTypes
    {
        OrderByMoney = 1, //按价格排序
        OrderByMoneyPerItem = 2, //按单价排序
        OrderBySuit = 4, //按阶数排序
        OrderByNameAndColor = 8, //按名字和颜色排序

        Max = 8, //有效的最大值
    }

    public class SearchArgs : IEqualityComparer<SearchArgs>
    {
        public static SearchArgs Compare = new SearchArgs();

        private int HashCode;

        private int OrderFlags;

        public int _Type;
        public int Type
        {
            get
            {
                return _Type;
            }
            set
            {
                lock (this)
                {
                    _Type = value;
                    InternalCalcHash();
                }
            }
        }

        public int _ID;
        public int ID
        {
            get
            {
                return _ID;
            }
            set
            {
                lock (this)
                {
                    _ID = value;
                    InternalCalcHash();
                }
            }
        }

        /// <summary>
        /// 有效值:1金币,2钻石,3金币和钻石
        /// </summary>
        public int _MoneyFlags;

        /// <summary>
        /// 有效值:1金币,2钻石,3金币和钻石
        /// </summary>
        public int MoneyFlags
        {
            get
            {
                return _MoneyFlags;
            }
            set
            {
                lock (this)
                {
                    _MoneyFlags = value;
                    InternalCalcHash();
                }
            }
        }

        /// <summary>
        /// 按位存储的颜色过滤选项,白色为第0位(最低位)
        /// </summary>
        public int _ColorFlags;

        /// <summary>
        /// 按位存储的颜色过滤选项,白色为第0位(最低位)
        /// </summary>
        public int ColorFlags
        {
            get
            {
                return _ColorFlags;
            }
            set
            {
                lock (this)
                {
                    _ColorFlags = value;
                    InternalCalcHash();
                }
            }
        }

        /// <summary>
        /// 0 desc,1 asc
        /// </summary>
        public int _OrderBy;

        /// <summary>
        /// 0 desc,1 asc
        /// </summary>
        public int OrderBy
        {
            get
            {
                return _OrderBy;
            }
            set
            {
                lock (this)
                {
                    _OrderBy = value;
                    InternalCalcHash();
                }
            }
        }

        /// <summary>
        /// 排序类型
        /// </summary>
        public int _OrderTypeFlags;

        /// <summary>
        /// 排序类型
        /// </summary>
        public int OrderTypeFlags
        {
            get
            {
                return _OrderTypeFlags;
            }
            set
            {
                lock (this)
                {
                    _OrderTypeFlags = value;
                    InternalCalcHash();
                }
            }
        }

        private SearchArgs()
        {
            _Type = 0;
            _ID = 0;
            _MoneyFlags = 0;
            _OrderBy = 0;
            _ColorFlags = 0;
            _OrderTypeFlags = 0;
            HashCode = 0;
            OrderFlags = 0;
        }

        public SearchArgs(SearchArgs args)
        {
            _Type = args.Type;
            _ID = args.ID;
            _MoneyFlags = args.MoneyFlags;
            _OrderBy = args.OrderBy;
            _ColorFlags = args.ColorFlags;
            _OrderTypeFlags = args.OrderTypeFlags;

            OrderFlags = 0;
            HashCode = 0;
            InternalCalcHash();
        }

        public SearchArgs(int id, int type, int moneyFlags, int colorFlags, int orderBy, int orderTypeFlags)
        {
            _ID = id;
            _Type = type;
            _MoneyFlags = moneyFlags;
            _OrderBy = orderBy;
            _ColorFlags = colorFlags;
            _OrderTypeFlags = orderTypeFlags;

            OrderFlags = 0;
            HashCode = 0;
            InternalCalcHash();
        }

        /// <summary>
        /// 任何引起OrderFlags或HashCode唯一性的修改,都可能引起交易市场逻辑出现问题,需同步的修改这个比较函数
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public bool Equals(SearchArgs x, SearchArgs y)
        {
            if (x.OrderFlags != y.OrderFlags)
            {
                return false;
            }
            return x.ID == y.ID && x.Type == y.Type;
        }

        public int GetHashCode(SearchArgs obj)
        {
            return obj.HashCode;
        }

        /// <summary>
        /// 内部更新hash值
        /// </summary>
        private void InternalCalcHash()
        {
            OrderFlags = (OrderBy << 30) + (OrderTypeFlags << 25) + (MoneyFlags << 23) + (ColorFlags << 14);
            HashCode = OrderFlags + (Type << 8) + ID;
        }

        /// <summary>
        /// 克隆本对象
        /// </summary>
        /// <returns></returns>
        public SearchArgs Clone()
        {
            return new SearchArgs(this);
        }
    }

    /// <summary>
    /// 升序排列比较器
    /// </summary>
    public class SaleGoodsMoneyCompare : IComparer<SaleGoodsData>
    {
        public static readonly SaleGoodsMoneyCompare Instance = new SaleGoodsMoneyCompare();
        /// <summary>
        ///  HÀM SẮP XẾP THEO GIÁ
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public int Compare(SaleGoodsData x, SaleGoodsData y)
        {
            //int ret = x.SalingGoodsData.SaleYuanBao - y.SalingGoodsData.SaleYuanBao;
            //if (ret != 0)
            //{
            //    return ret;
            //}
            //return x.SalingGoodsData.SaleMoney - y.SalingGoodsData.SaleMoney;


            return 1;
        }
    }

    /// <summary>
    /// 降序排列比较器
    /// </summary>
    public class SaleGoodsMoneyCompare2 : IComparer<SaleGoodsData>
    {
        public static readonly SaleGoodsMoneyCompare2 Instance = new SaleGoodsMoneyCompare2();

        public int Compare(SaleGoodsData x, SaleGoodsData y)
        {
            //int ret = y.SalingGoodsData.SaleYuanBao - x.SalingGoodsData.SaleYuanBao;
            //if (ret != 0)
            //{
            //    return ret;
            //}
            //return y.SalingGoodsData.SaleMoney - x.SalingGoodsData.SaleMoney;

            return 1;
        }
    }

    /// <summary>
    /// 出售物品单价排序比较器
    /// </summary>
    public class SaleGoodsMoneyPerItemCompare : IComparer<SaleGoodsData>
    {
        public static readonly SaleGoodsMoneyPerItemCompare DescInstance = new SaleGoodsMoneyPerItemCompare(1);
        public static readonly SaleGoodsMoneyPerItemCompare AscInstance = new SaleGoodsMoneyPerItemCompare(0);

        private bool Desc = true;
        public SaleGoodsMoneyPerItemCompare(int desc)
        {
            Desc = (desc != 0);
        }
        /// <summary>
        /// SORT THEO SỐ LƯỢNG
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public int Compare(SaleGoodsData x, SaleGoodsData y)
        {
            int ret = 0;
            if (x.SalingGoodsData.GCount <= 0)
            {
                if (y.SalingGoodsData.GCount > 0)
                {
                    ret = -1;
                }
            }
            else if (y.SalingGoodsData.GCount <= 0)
            {
                return 1;
            }
            else
            {
                return 1;
            }
            if (Desc)
            {
                ret = -ret;
            }
            return ret;
        }
    }

    /// <summary>
    /// 出售物品名字和颜色排序比较器
    /// </summary>
    public class SaleGoodsNameAndColorCompare : IComparer<SaleGoodsData>
    {
        public static readonly SaleGoodsNameAndColorCompare DescInstance = new SaleGoodsNameAndColorCompare(1);
        public static readonly SaleGoodsNameAndColorCompare AscInstance = new SaleGoodsNameAndColorCompare(0);

        private bool Desc = true;
        public SaleGoodsNameAndColorCompare(int desc)
        {
            Desc = (desc != 0);
        }

        public int Compare(SaleGoodsData x, SaleGoodsData y)
        {
            int ret = 0;
            
            return ret;
        }
    }

    /// <summary>
    /// 出售物品阶数排序比较器
    /// </summary>
    public class SaleGoodsSuitCompare : IComparer<SaleGoodsData>
    {
        public static readonly SaleGoodsSuitCompare DescInstance = new SaleGoodsSuitCompare(1);
        public static readonly SaleGoodsSuitCompare AscInstance = new SaleGoodsSuitCompare(0);

        private bool Desc = true;
        public SaleGoodsSuitCompare(int desc)
        {
            Desc = (desc != 0);
        }

        public int Compare(SaleGoodsData x, SaleGoodsData y)
        {
            int ret = 0;
           
            return ret;
        }
    }

    #endregion 相关结构定义

    /// <summary>
    /// 出售的物品的管理
    /// </summary>
    public class SaleManager : IManager
    {
        #region 继承接口实现

        private static SaleManager instance = new SaleManager();

        public static SaleManager getInstance()
        {
            return instance;
        }

        public bool initialize()
        {
            InitConfig();
            return true;
        }

        public bool startup()
        {
            TCPCmdDispatcher.getInstance().registerProcessor((int)TCPGameServerCmds.CMD_SPR_OPENMARKET2, 3, SaleCmdsProcessor.getInstance(TCPGameServerCmds.CMD_SPR_OPENMARKET2));
            TCPCmdDispatcher.getInstance().registerProcessor((int)TCPGameServerCmds.CMD_SPR_MARKETSALEMONEY2, 3, SaleCmdsProcessor.getInstance(TCPGameServerCmds.CMD_SPR_MARKETSALEMONEY2));
            TCPCmdDispatcher.getInstance().registerProcessor((int)TCPGameServerCmds.CMD_SPR_SALEGOODS2, 7, SaleCmdsProcessor.getInstance(TCPGameServerCmds.CMD_SPR_SALEGOODS2));
            TCPCmdDispatcher.getInstance().registerProcessor((int)TCPGameServerCmds.CMD_SPR_SELFSALEGOODSLIST2, 1, SaleCmdsProcessor.getInstance(TCPGameServerCmds.CMD_SPR_SELFSALEGOODSLIST2));
            TCPCmdDispatcher.getInstance().registerProcessor((int)TCPGameServerCmds.CMD_SPR_OTHERSALEGOODSLIST2, 2, SaleCmdsProcessor.getInstance(TCPGameServerCmds.CMD_SPR_OTHERSALEGOODSLIST2));
            TCPCmdDispatcher.getInstance().registerProcessor((int)TCPGameServerCmds.CMD_SPR_MARKETROLELIST2, 1, SaleCmdsProcessor.getInstance(TCPGameServerCmds.CMD_SPR_MARKETROLELIST2));
            TCPCmdDispatcher.getInstance().registerProcessor((int)TCPGameServerCmds.CMD_SPR_MARKETGOODSLIST2, 5, SaleCmdsProcessor.getInstance(TCPGameServerCmds.CMD_SPR_MARKETGOODSLIST2));
            TCPCmdDispatcher.getInstance().registerProcessor((int)TCPGameServerCmds.CMD_SPR_MARKETBUYGOODS2, 5, SaleCmdsProcessor.getInstance(TCPGameServerCmds.CMD_SPR_MARKETBUYGOODS2));
            return true;
        }

        public bool showdown()
        {
            return true;
        }

        public bool destroy()
        {
            return true;
        }

        #endregion 继承接口实现

        #region 常量定义

        public const int ConstAllColorFlags = 63;
        public const int ConstAllMoneyFlags = 3;

        public static double JiaoYiShuiJinBi { get; private set; }
        public static double JiaoYiShuiZuanShi { get; private set; }
        public static int MaxSaleNum { get; private set; }

        #endregion 常量定义

        #region 变量定义

        /// <summary>
        /// 数据锁，操作所有物品列表的Dictionary都需获取此锁
        /// </summary>
        private static object Mutex_SaleGoodsDict = new object();

        /// <summary>
        /// 保存正在出售的物品的词典
        /// </summary>
        private static Dictionary<int, SaleGoodsData> _SaleGoodsDict = new Dictionary<int, SaleGoodsData>();

        /// <summary>
        /// 出售物品分类列表
        /// </summary>
        private static Dictionary<int, Dictionary<int, List<SaleGoodsData>>> _SaleGoodsDict2 = new Dictionary<int, Dictionary<int, List<SaleGoodsData>>>();

        /// <summary>
        /// 缓存的列表排序结果
        /// </summary>
        private static Dictionary<SearchArgs, List<SaleGoodsData>> _OrderdSaleGoodsListDict = new Dictionary<SearchArgs, List<SaleGoodsData>>(SearchArgs.Compare);

        /// <summary>
        /// 开启后初始化，运行中无修改
        /// </summary>
        private static Dictionary<int, int[]> _GoodsID2TabIDDict = new Dictionary<int, int[]>();
        private static Dictionary<long, int[]> _Categoriy2TabIDDict = new Dictionary<long, int[]>();
        private static HashSet<int> _TypeHashSet = new HashSet<int>();
        private static HashSet<int> _IDHashSet = new HashSet<int>();
        private static int[] OthersGoodsTypeAndID = null;
        public static long MaxSaleGoodsTime = 43200 * 1000;

        /// <summary>
        /// 搜索文本到物品GoodsID和TabID的映射表
        /// </summary>
        private static Dictionary<string, Dictionary<int, int>> _SearchText2GoodsIDDict = new Dictionary<string, Dictionary<int, int>>();
        private static Dictionary<string, List<int>> _SearchText2GoodsIDDict2 = new Dictionary<string, List<int>>();

        #endregion 变量定义

        #region 初始化

        /// <summary>
        /// 初始化配置
        /// </summary>
        public static void InitConfig()
        {
           

        }

        #endregion 初始化

        #region 基础接口

        /// <summary>
        /// 获取物品所属的交易物品类型和子类型ID
        /// </summary>
        /// <param name="goodsID"></param>
        /// <returns></returns>
        public static int[] GetTypeAndID(int goodsID)
        {
            int[] typeAndID = null;
            lock (_GoodsID2TabIDDict)
            {
                if (!_GoodsID2TabIDDict.TryGetValue(goodsID, out typeAndID))
                {
                    int category = ItemManager.GetGoodsCatetoriy(goodsID);
                   
                }
            }
            if (null == typeAndID)
            {
                return OthersGoodsTypeAndID;
            }
            return typeAndID;
        }

        public static bool IsValidType(int type, int id)
        {
            return _TypeHashSet.Contains(type) && (id <= 0 || _IDHashSet.Contains(id));
        }

        #endregion 基础接口

        #region 功能接口

        /// <summary>
        /// 添加交易物品数据到管理器
        /// </summary>
        /// <param name="saleGoodsData"></param>
        public static void AddSaleGoodsData(SaleGoodsData saleGoodsData)
        {
            int goodsID = saleGoodsData.SalingGoodsData.GoodsID;
            int[] typeAndID = null;

            typeAndID = GetTypeAndID(goodsID);
            if (null != typeAndID)
            {
                lock (Mutex_SaleGoodsDict)
                {
                    List<SaleGoodsData> list = _SaleGoodsDict2[typeAndID[0]][typeAndID[1]];
                    UpdateOrderdList(list, saleGoodsData, true, true, SearchOrderTypes.OrderByMoney);
                    _SaleGoodsDict[saleGoodsData.GoodsDbID] = saleGoodsData;
                    //RemoveCachedListForSaleGoodsData(saleGoodsData, typeAndID);
                    UpdateCachedListForSaleGoodsData(saleGoodsData, typeAndID, true);
                }
            }
        }

        /// <summary>
        /// 添加出售的物品项（在线状态）
        /// </summary>
        /// <param name="saleGoodsItem"></param>
        public static void AddSaleGoodsItem(SaleGoodsItem saleGoodsItem)
        {
            SaleGoodsData saleGoodsData = new SaleGoodsData()
            {
                GoodsDbID = saleGoodsItem.GoodsDbID,
                SalingGoodsData = saleGoodsItem.SalingGoodsData,
                RoleID = saleGoodsItem.Client.RoleID,
                RoleName = saleGoodsItem.Client.RoleName,
                RoleLevel = saleGoodsItem.Client.m_Level,
            };

            AddSaleGoodsData(saleGoodsData);
        }

     

        /// <summary>
        /// 删除出售的物品项
        /// </summary>
        /// <param name="saleGoodsItem"></param>
        public static void RemoveSaleGoodsItem(int goodsDbID)
        {
            int[] typeAndID = null;
            lock (Mutex_SaleGoodsDict)
            {
                SaleGoodsData saleGoodsData;
                if (_SaleGoodsDict.TryGetValue(goodsDbID, out saleGoodsData))
                {
                    int goodsID = saleGoodsData.SalingGoodsData.GoodsID;
                    typeAndID = GetTypeAndID(goodsID);
                    if (null != typeAndID)
                    {
                        List<SaleGoodsData> list = _SaleGoodsDict2[typeAndID[0]][typeAndID[1]];
                        UpdateOrderdList(list, saleGoodsData, true, false, SearchOrderTypes.OrderByMoney);
                    }
                    _SaleGoodsDict.Remove(goodsDbID);
                    //RemoveCachedListForSaleGoodsData(saleGoodsData, typeAndID);
                    UpdateCachedListForSaleGoodsData(saleGoodsData, typeAndID, false);
                }
            }
        }

        /// <summary>
        /// 获取特定类别的出售物品列表
        /// </summary>
        /// <param name="type"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static List<SaleGoodsData> GetSaleGoodsDataList(int type, int id = 0)
        {
            List<SaleGoodsData> saleGoodsDataList = null;
            lock (Mutex_SaleGoodsDict)
            {
                if (type == 1)
                {
                    saleGoodsDataList = new List<SaleGoodsData>();
                    foreach (var kv in _SaleGoodsDict2)
                    {
                        if (null != kv.Value && kv.Value.Count > 0)
                        {
                            foreach (var kv2 in kv.Value)
                            {
                                if (null != kv2.Value && kv2.Value.Count > 0)
                                {
                                    saleGoodsDataList.BinaryCombineDesc(kv2.Value, SaleGoodsMoneyCompare.Instance);
                                }
                            }
                        }
                    }
                }
                else
                {
                    Dictionary<int, List<SaleGoodsData>> subDict;
                    if (_SaleGoodsDict2.TryGetValue(type, out subDict))
                    {
                        if (!subDict.TryGetValue(id, out saleGoodsDataList))
                        {
                            if (id <= 0 && saleGoodsDataList == null)
                            {
                                saleGoodsDataList = new List<SaleGoodsData>();
                                foreach (var kv in subDict)
                                {
                                    if (kv.Key > 0 && null != kv.Value)
                                    {
                                        saleGoodsDataList.BinaryCombineDesc(kv.Value, SaleGoodsMoneyCompare.Instance);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            if (null == saleGoodsDataList)
            {
                saleGoodsDataList = new List<SaleGoodsData>();
            }
            return saleGoodsDataList;
        }

        /// <summary>
        /// 获取缓存的挂售物品列表
        /// </summary>
        /// <param name="searchArgs"></param>
        /// <returns></returns>
        private static List<SaleGoodsData> GetCachedSaleGoodsList(SearchArgs searchArgs)
        {
            List<SaleGoodsData> saleGoodsDataList = null;
            SearchArgs args = new SearchArgs(searchArgs);
            int colorFlags = -1;
            int moneyFlags = -1;
            int orderBy = -1;
            int orderTypeFlags = -1;
            lock (Mutex_SaleGoodsDict)
            {
                do 
                {
                    //是否缓存已有相应结果    
                    if (_OrderdSaleGoodsListDict.TryGetValue(searchArgs, out saleGoodsDataList))
                    {
                        break;
                    }
                    else if (searchArgs.ColorFlags < SaleManager.ConstAllColorFlags)
                    {
                        colorFlags = searchArgs.ColorFlags;
                        searchArgs.ColorFlags = SaleManager.ConstAllColorFlags;
                    }
                    else if (searchArgs.MoneyFlags < SaleManager.ConstAllMoneyFlags)
                    {
                        moneyFlags = searchArgs.MoneyFlags;
                        searchArgs.MoneyFlags = SaleManager.ConstAllMoneyFlags;
                    }
                    else if (searchArgs.OrderBy > 0)
                    {
                        orderBy = searchArgs.OrderBy;
                        searchArgs.OrderBy = 0;
                    }
                    else if (searchArgs.OrderTypeFlags > (int)SearchOrderTypes.OrderByMoney)
                    {
                        orderTypeFlags = searchArgs.OrderTypeFlags;
                        searchArgs.OrderTypeFlags = 0;
                    }
                    else
                    {
                        break;
                    }
                } while (true);

                if (null == saleGoodsDataList)
                {
                    saleGoodsDataList = GetSaleGoodsDataList(searchArgs.Type, searchArgs.ID);
                    _OrderdSaleGoodsListDict.Add(searchArgs.Clone(), new List<SaleGoodsData>(saleGoodsDataList));
                }
                if (orderTypeFlags > 0)
                {
                    saleGoodsDataList = new List<SaleGoodsData>(saleGoodsDataList);
                    saleGoodsDataList.Sort(GetComparerFor(true, true, (SearchOrderTypes)orderTypeFlags));
                    searchArgs.OrderTypeFlags = orderTypeFlags;
                    _OrderdSaleGoodsListDict.Add(searchArgs.Clone(), saleGoodsDataList);
                }
                if (orderBy > 0)
                {
                    saleGoodsDataList = new List<SaleGoodsData>(saleGoodsDataList);
                    searchArgs.OrderBy = orderBy;
                    saleGoodsDataList.Reverse();
                    _OrderdSaleGoodsListDict.Add(searchArgs.Clone(), saleGoodsDataList);
                }
                // TÌM KIẾM THEO GIÁ
                if (moneyFlags > 0)
                {
                    //searchArgs.MoneyFlags = moneyFlags;
                    //saleGoodsDataList = new List<SaleGoodsData>(saleGoodsDataList);
                    //saleGoodsDataList.RemoveAll((x) => 
                    //    {
                    //        //if (x.SalingGoodsData.SaleMoney > 0)
                    //        //{
                    //        //    return (moneyFlags & 1) == 0;
                    //        //}
                    //        //else if (x.SalingGoodsData.SaleYuanBao > 0)
                    //        //{
                    //        //    return (moneyFlags & 2) == 0;
                    //        //}
                    //        //return true;
                    //    });
                    //_OrderdSaleGoodsListDict.Add(searchArgs.Clone(), saleGoodsDataList);
                }
                if (colorFlags > 0)
                {
                    saleGoodsDataList = new List<SaleGoodsData>(saleGoodsDataList);
                    searchArgs.ColorFlags = colorFlags;
                    saleGoodsDataList.RemoveAll((x) =>
                    {
                        
                        return false;
                    });
                    _OrderdSaleGoodsListDict.Add(searchArgs.Clone(), saleGoodsDataList);
                }
            }

            return saleGoodsDataList;
        }

        /// <summary>
        /// 更新缓存的列表
        /// </summary>
        /// <param name="saleGoodsData"></param>
        /// <param name="typeAndID"></param>
        /// <param name="add"></param>
        public static void UpdateCachedListForSaleGoodsData(SaleGoodsData saleGoodsData, int[] typeAndID, bool add)
        {
            lock (Mutex_SaleGoodsDict)
            {
                if (_OrderdSaleGoodsListDict.Count == 0)
                {
                    return;
                }
                if (null != typeAndID)
                {
                    List<SearchArgs> list = new List<SearchArgs>();
                    
                }
            }
        }

        private static IComparer<SaleGoodsData> GetComparerFor(bool desc, bool add, SearchOrderTypes searchOrderType)
        {
            IComparer<SaleGoodsData> compare = null;

            switch (searchOrderType)
            {
                case SearchOrderTypes.OrderByMoney:
                    {
                        if (desc && !add)
                        {
                            compare = SaleGoodsMoneyCompare2.Instance;
                        }
                        else
                        {
                            compare = SaleGoodsMoneyCompare.Instance;
                        }
                    }
                    break;
                case SearchOrderTypes.OrderByMoneyPerItem:
                    {
                        if (desc && !add)
                        {
                            compare = SaleGoodsMoneyPerItemCompare.DescInstance;
                        }
                        else
                        {
                            compare = SaleGoodsMoneyPerItemCompare.AscInstance;
                        }
                    }
                    break;
                case SearchOrderTypes.OrderByNameAndColor:
                    {
                        if (desc && !add)
                        {
                            compare = SaleGoodsNameAndColorCompare.DescInstance;
                        }
                        else
                        {
                            compare = SaleGoodsNameAndColorCompare.AscInstance;
                        }
                    }
                    break;
                case SearchOrderTypes.OrderBySuit:
                    {
                        if (desc && !add)
                        {
                            compare = SaleGoodsSuitCompare.DescInstance;
                        }
                        else
                        {
                            compare = SaleGoodsSuitCompare.AscInstance;
                        }
                    }
                    break;
                default:
                    compare = SaleGoodsMoneyCompare.Instance;
                    break;
            }

            return compare;
        }

        /// <summary>
        /// 按排序类型等条件从列表中添加或删除元素
        /// </summary>
        /// <param name="list"></param>
        /// <param name="saleGoodsData"></param>
        /// <param name="desc"></param>
        /// <param name="add"></param>
        private static void UpdateOrderdList(List<SaleGoodsData> list, SaleGoodsData saleGoodsData, bool desc, bool add, SearchOrderTypes searchOrderType)
        {
            if (add)
            {
                if (desc)
                {
                    list.BinaryInsertDesc(saleGoodsData, GetComparerFor(desc, add, searchOrderType));
                }
                else
                {
                    list.BinaryInsertAsc(saleGoodsData, GetComparerFor(desc, add, searchOrderType));
                }
            }
            else
            {
#if false
                list.Remove(saleGoodsData);
#else
                int index = list.BinarySearch(saleGoodsData, GetComparerFor(desc, add, searchOrderType));
                if (index < 0)
                {
                    //二分查找失败,可能是物品属性变化或不可比较,只能遍历了
                    for (int i = 0; i < list.Count; i++)
                    {
                        if (list[i].GoodsDbID == saleGoodsData.GoodsDbID)
                        {
                            list.RemoveAt(i);
                            return;
                        }
                    }
                }
                else
                {
                    for (int i = index; i >= 0; i--)
                    {
                        if (list[i].GoodsDbID == saleGoodsData.GoodsDbID)
                        {
                            list.RemoveAt(i);
                            return;
                        }
                    }
                    for (int i = index + 1; i < list.Count; i++)
                    {
                        if (list[i].GoodsDbID == saleGoodsData.GoodsDbID)
                        {
                            list.RemoveAt(i);
                            return;
                        }
                    }
                }
#endif
            }
        }

        /// <summary>
        /// 确保参数没有异常
        /// </summary>
        /// <param name="searchArgs"></param>
        private static void FixSearchArgs(SearchArgs searchArgs)
        {
            if (!IsValidType(searchArgs.Type, searchArgs.ID))
            {
                searchArgs.Type = 1;
                searchArgs.ID = 1;
            }

            int searchOrderType = searchArgs.OrderTypeFlags;
            for (int i = 0; i < 32; i++ )
            {
                int mask = (1 << i);
                if ((searchOrderType & mask) != 0)
                {
                    searchArgs.OrderTypeFlags &= mask;
                    if (searchOrderType > (int)SearchOrderTypes.Max)
                    {
                        searchOrderType = (int)SearchOrderTypes.OrderByMoney;
                    }
                    break;
                }
            }
        }

        /// <summary>
        /// 筛选数据返回列表
        /// </summary>
        /// <param name="seachArgs">过滤和排序选项</param>
        /// <param name="GoodsIds">过滤结果在此物品ID列表中,列表为空表示不限制</param>
        /// <returns></returns>
        public static List<SaleGoodsData> GetSaleGoodsDataList(SearchArgs searchArgs, List<int> GoodsIds)
        {
            FixSearchArgs(searchArgs);

            List<SaleGoodsData> saleGoodsDataList = GetCachedSaleGoodsList(searchArgs);

            if (null != GoodsIds && GoodsIds.Count > 0)
            {
                saleGoodsDataList = saleGoodsDataList.FindAll((x) =>
                    {
                        return GoodsIds.Contains(x.SalingGoodsData.GoodsID);
                    });
            }

            return saleGoodsDataList;
        }

        #endregion 功能接口

    }
}
