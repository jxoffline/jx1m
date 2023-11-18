using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Server.Data;

namespace GameServer.SafeData
{
    /// <summary>
    /// 提供对PetData数据的安全访问
    /// </summary>
    public class SafePetData
    {
        public SafePetData(PetData petData)
        {
            _PetData = petData;
        }

        /// <summary>
        /// 私有数据
        /// </summary>
        private PetData _PetData = null;

        /// <summary>
        /// 宠物的数据库ID
        /// </summary>
        public int DbID
        {
            get { lock (this) { return _PetData.DbID; } }
            set { lock (this) { _PetData.DbID = value; } }
        }

        /// <summary>
        /// 宠物ID
        /// </summary>
        public int PetID
        {
            get { lock (this) { return _PetData.PetID; } }
            set { lock (this) { _PetData.PetID = value; } }
        }

        /// <summary>
        /// 宠物的名称
        /// </summary>
        public string PetName
        {
            get { lock (this) { return _PetData.PetName; } }
            set { lock (this) { _PetData.PetName = value; } }
        }

        /// <summary>
        /// 宠物的类型(0: 普通宠物, 高级宠物)
        /// </summary>
        public int PetType
        {
            get { lock (this) { return _PetData.PetType; } }
            set { lock (this) { _PetData.PetType = value; } }
        }

        /// <summary>
        /// 宠物的喂食次数
        /// </summary>
        public int FeedNum
        {
            get { lock (this) { return _PetData.FeedNum; } }
            set { lock (this) { _PetData.FeedNum = value; } }
        }

        /// <summary>
        /// 用户扩展的格子个数
        /// </summary>
        public int ExtGridNum
        {
            get { lock (this) { return _PetData.ExtGridNum; } }
            set { lock (this) { _PetData.ExtGridNum = value; } }
        }

        /// <summary>
        /// 宠物复活的次数
        /// </summary>
        public int ReAliveNum
        {
            get { lock (this) { return _PetData.ReAliveNum; } }
            set { lock (this) { _PetData.ReAliveNum = value; } }
        }

        /// <summary>
        /// 宠物的领养时间
        /// </summary>
        public long AddDateTime
        {
            get { lock (this) { return _PetData.AddDateTime; } }
            set { lock (this) { _PetData.AddDateTime = value; } }
        }

        /// <summary>
        /// 宠物的扩展属性
        /// </summary>
        public string PetProps
        {
            get { lock (this) { return _PetData.PetProps; } }
            set { lock (this) { _PetData.PetProps = value; } }
        }

        /// <summary>
        /// 是否自动拾取
        /// </summary>
        public int AutoGetThing
        {
            get { lock (this) { return _PetData.AutoGetThing; } }
            set { lock (this) { _PetData.AutoGetThing = value; } }
        }

        /// <summary>
        /// 宠物的级别
        /// </summary>
        public int Level
        {
            get { lock (this) { return _PetData.Level; } }
            set { lock (this) { _PetData.Level = value; } }
        }

        /// <summary>
        /// 当前物品使用的格子的个数(不存数据库，每次加载后计算)
        /// </summary>
        public int GoodsUsedGridNum
        {
            get { lock (this) { return _PetData.GoodsUsedGridNum; } }
            set { lock (this) { _PetData.GoodsUsedGridNum = value; } }
        }
    }
}
