using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Server.Data;

namespace GameServer.SafeData
{
    /// <summary>
    /// GoodsData 线程安全类
    /// </summary>
    public class SafeGoodsData
    {
        public SafeGoodsData(GoodsData goodsData)
        {
            _GoodsData = goodsData;
        }

        /// <summary>
        /// 私有数据
        /// </summary>
        private GoodsData _GoodsData = null;

        /// <summary>
        /// 数据库流水ID
        /// </summary>
        public int Id
        {
            get { lock (this) { return _GoodsData.Id; } }
            set { lock (this) { _GoodsData.Id = value; } }
        }

        /// <summary>
        /// 物品ID
        /// </summary>
        public int GoodsID
        {
            get { lock (this) { return _GoodsData.GoodsID; } }
            set { lock (this) { _GoodsData.GoodsID = value; } }
        }

        /// <summary>
        /// 是否正在使用
        /// </summary>
        public int Using
        {
            get { lock (this) { return _GoodsData.Using; } }
            set { lock (this) { _GoodsData.Using = value; } }
        }

        /// <summary>
        /// 锻造级别
        /// </summary>
        public int Forge_level
        {
            get { lock (this) { return _GoodsData.Forge_level; } }
            set { lock (this) { _GoodsData.Forge_level = value; } }
        }

        /// <summary>
        /// 开始使用的时间
        /// </summary>
        public string Starttime
        {
            get { lock (this) { return _GoodsData.Starttime; } }
            set { lock (this) { _GoodsData.Starttime = value; } }
        }

        /// <summary>
        /// 上次使用结束时间
        /// </summary>
        public string Endtime
        {
            get { lock (this) { return _GoodsData.Endtime; } }
            set { lock (this) { _GoodsData.Endtime = value; } }
        }

        /// <summary>
        /// 所在的位置(0: 背包, 1:仓库)
        /// </summary>
        public int Site
        {
            get { lock (this) { return _GoodsData.Site; } }
            set { lock (this) { _GoodsData.Site = value; } }
        }

        /// <summary>
        /// 物品的品质(某些装备会分品质，不同的品质属性不同，用户改变属性后要记录下来)
        /// </summary>
        public int Quality
        {
            get { lock (this) { return _GoodsData.Quality; } }
            set { lock (this) { _GoodsData.Quality = value; } }
        }

        /// <summary>
        /// 根据品质随机抽取的扩展属性的索引列表
        /// </summary>
        public string Props
        {
            get { lock (this) { return _GoodsData.Props; } }
            set { lock (this) { _GoodsData.Props = value; } }
        }

        /// <summary>
        /// 物品数量
        /// </summary>
        public int GCount
        {
            get { lock (this) { return _GoodsData.GCount; } }
            set { lock (this) { _GoodsData.GCount = value; } }
        }

        /// <summary>
        /// 是否绑定的物品(绑定的物品不可交易, 不可摆摊)
        /// </summary>
        public int Binding
        {
            get { lock (this) { return _GoodsData.Binding; } }
            set { lock (this) { _GoodsData.Binding = value; } }
        }
    }
}
