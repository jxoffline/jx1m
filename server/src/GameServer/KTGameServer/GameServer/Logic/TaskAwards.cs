using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Server.Tools;
using Server.Data;

namespace GameServer.Logic
{
    /// <summary>
    /// 任务奖励
    /// </summary>
    public class TaskAwards
    {
        public TaskAwards()
        {
        }

        #region 缓存接口

        /// <summary>
        /// 任务装备奖励
        /// </summary>
        private Dictionary<int, List<AwardsItemData>> _TaskAwardsDict = new Dictionary<int, List<AwardsItemData>>();

        /// <summary>
        /// 任务物品奖励
        /// </summary>
        private Dictionary<int, List<AwardsItemData>> _OtherAwardsDict = new Dictionary<int, List<AwardsItemData>>();

        /// <summary>
        /// 任务金钱奖励
        /// </summary>
        private Dictionary<int, int> _MoneyDict = new Dictionary<int, int>();

        /// <summary>
        /// 任务金钱奖励
        /// </summary>
        private Dictionary<int, long> _ExperienceDict = new Dictionary<int, long>();

        /// <summary>
        /// 任务银两奖励
        /// </summary>
        private Dictionary<int, int> _YinLiangDict = new Dictionary<int, int>();

        /// <summary>
        /// 绑定元宝奖励
        /// </summary>
        private Dictionary<int, int> _BindYuanBaoDict = new Dictionary<int, int>();

        /// <summary>
        /// 任务灵力奖励
        /// </summary>
        private Dictionary<int, int> _LingLiDict = new Dictionary<int, int>();

        /// <summary>
        /// 任务坐骑祝福点奖励
        /// </summary>
        private Dictionary<int, int> _BlessPointDict = new Dictionary<int, int>();

        /// <summary>
        /// 任务真气值奖励
        /// </summary>
        private Dictionary<int, int> _ZhenQiDict = new Dictionary<int, int>();

        /// <summary>
        /// 任务猎杀值奖励
        /// </summary>
        private Dictionary<int, int> _LieShaDict = new Dictionary<int, int>();

        /// <summary>
        /// 任务悟性值奖励
        /// </summary>
        private Dictionary<int, int> _WuXingDict = new Dictionary<int, int>();

        /// <summary>
        /// 元宝领取消耗
        /// </summary>
        private Dictionary<int, int> _NeedYuanBaoDict = new Dictionary<int, int>();

        /// <summary>
        /// 任务军功值奖励
        /// </summary>
        private Dictionary<int, int> _JunGongDict = new Dictionary<int, int>();

        /// <summary>
        /// 荣誉奖励
        /// </summary>
        private Dictionary<int, int> _RongYuDict = new Dictionary<int, int>();

        /// <summary>
        /// 魔晶奖励 -  天地精元 [4/10/2014 LiaoWei]
        /// </summary>
        private Dictionary<int, int> _JingYuanDict = new Dictionary<int, int>();

        /// <summary>
        /// 星魂奖励 [8/11/2014 LiaoWei]
        /// </summary>
        private Dictionary<int, int> _XinHunAwardDict = new Dictionary<int, int>();

        /// <summary>
        /// 解析任务装备奖励
        /// </summary>
        /// <param name="fields"></param>
        /// <param name="taskAwards"></param>
        private void ParseTaskAwardsItem(string awardsStr, out AwardsItemData taskAwards)
        {
            // 物品通用奖励改造 [1/7/2014 LiaoWei]
            taskAwards = null;
            string[] fields = awardsStr.Split(',');
            if (fields.Length != 7)
            {
                return;
            }


            //Tìm vật phẩm nhận được từ nhiệm vụ

            //SystemXmlItem systemGoods = null;
            //if (!GameManager.SystemGoods.SystemXmlItemDict.TryGetValue(Convert.ToInt32(fields[0]), out systemGoods))
            //{
            //    LogManager.WriteLog(LogTypes.Error, string.Format("解析任务装备奖励时，物品不存在: GoodsID={0}", Convert.ToInt32(fields[0])));
            //    return;
            //}

            //taskAwards = new AwardsItemData()
            //{
            //    FactionID = (null == systemGoods) ? -1 : Global.GetMainOccupationByGoodsID(Convert.ToInt32(fields[0])), //取的物品本身的职业
            //    RoleSex = (null == systemGoods) ? -1 : systemGoods.GetIntValue("ToSex"), //取的物品本身的职业
            //    GoodsID             = Convert.ToInt32(fields[0]),
            //    GoodsNum            = Convert.ToInt32(fields[1]),
            //    Binding             = Convert.ToInt32(fields[2]),
            //    Level               = Convert.ToInt32(fields[3]),
            //    AppendLev           = Convert.ToInt32(fields[4]),   // 追加等级
            //    IsHaveLuckyProp     = Convert.ToInt32(fields[5]),   // 是否有幸运属性
            //    ExcellencePorpValue = Convert.ToInt32(fields[6]),   // 卓越属性值
            //    EndTime = Global.ConstGoodsEndTime,
            //};
        }

        /// <summary>
        /// 解析任务其他奖励
        /// </summary>
        /// <param name="fields"></param>
        /// <param name="taskAwards"></param>
        private void ParseOtherAwardsItem(string awardsStr, out AwardsItemData otherAwards, string goodsEndTime)
        {
            // 物品通用奖励改造 [1/7/2014 LiaoWei]
            otherAwards = null;
            string[] fields = awardsStr.Split(',');
            if (fields.Length != 7)
            {
                return;
            }

            if (string.IsNullOrEmpty(goodsEndTime) || Global.DateTimeTicks(goodsEndTime) <= 0)
            {
                goodsEndTime = Global.ConstGoodsEndTime;
            }

            otherAwards = new AwardsItemData()
            {
                FactionID = -1,
                RoleSex                 = -1,
                GoodsID                 = Convert.ToInt32(fields[0]),
                GoodsNum                = Convert.ToInt32(fields[1]),
                Binding                 = Convert.ToInt32(fields[2]),
                Level                   = Convert.ToInt32(fields[3]),
                AppendLev               = Convert.ToInt32(fields[4]),   // 追加等级
                IsHaveLuckyProp         = Convert.ToInt32(fields[5]),   // 是否有幸运属性
                ExcellencePorpValue     = Convert.ToInt32(fields[6]),   // 卓越属性值
                EndTime = goodsEndTime,
                
            };
        }

        /// <summary>
        /// 解析任务奖励
        /// </summary>
        /// <param name="systemTask"></param>
        private void ParseAwards(SystemXmlItem systemTask, out List<AwardsItemData> taskAwardsList, out List<AwardsItemData> otherAwardsList)
        {
            taskAwardsList = otherAwardsList = null;
            AwardsItemData awardsItem = null;
            string taskAwardsString = systemTask.GetStringValue("Taskaward").Trim();
            if (!string.IsNullOrEmpty(taskAwardsString))
            {
                string[] taskAwardsFields = taskAwardsString.Split('|');
                if (null != taskAwardsFields)
                {
                    taskAwardsList = new List<AwardsItemData>();
                    for (int i = 0; i < taskAwardsFields.Length; i++)
                    {
                        // 解析任务装备奖励 
                        awardsItem = null;
                        ParseTaskAwardsItem(taskAwardsFields[i], out awardsItem);
                        if (null != awardsItem)
                        {
                            taskAwardsList.Add(awardsItem);
                        }
                        else
                        {
                            LogManager.WriteLog(LogTypes.Error, string.Format("解析任务装备奖励失败: TaskID={0}", systemTask.GetIntValue("ID")));
                        }
                    }
                }
            }

            string goodsEndTime = systemTask.GetStringValue("GoodsEndTime").Trim();
            string otherAwardsString = systemTask.GetStringValue("OtherTaskaward").Trim();
            if (!string.IsNullOrEmpty(otherAwardsString))
            {
                string[] otherAwardsFields = otherAwardsString.Split('|');
                if (null != otherAwardsFields)
                {
                    otherAwardsList = new List<AwardsItemData>();
                    for (int i = 0; i < otherAwardsFields.Length; i++)
                    {
                        // 解析任务装备奖励 
                        awardsItem = null;
                        ParseOtherAwardsItem(otherAwardsFields[i], out awardsItem, goodsEndTime);
                        if (null != awardsItem)
                        {
                            otherAwardsList.Add(awardsItem);
                        }
                        else
                        {
                            LogManager.WriteLog(LogTypes.Error, string.Format("解析任务其他奖励失败: TaskID={0}", systemTask.GetIntValue("ID")));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 查找任务奖励
        /// </summary>
        /// <param name="taskID"></param>
        /// <returns></returns>
        public List<AwardsItemData> FindTaskAwards(int taskID)
        {
            List<AwardsItemData> awardsList = null;
            lock (_TaskAwardsDict)
            {
                if (_TaskAwardsDict.TryGetValue(taskID, out awardsList))
                {
                    return awardsList;
                }
            }

            //先查找缓存
            //SystemXmlItem systemTask = null;
            //if (!GameManager.SystemTasksMgr.SystemXmlItemDict.TryGetValue(taskID, out systemTask))
            //{
            //    return null;
            //}

            List<AwardsItemData> otherAwardsList = null;
            //ParseAwards(systemTask, out awardsList, out otherAwardsList);

            if (null != awardsList)
            {
                lock (_TaskAwardsDict)
                {
                    _TaskAwardsDict[taskID] = awardsList;
                }
            }

            return awardsList;
        }

        /// <summary>
        /// 查找其他奖励
        /// </summary>
        /// <param name="taskID"></param>
        /// <returns></returns>
        public List<AwardsItemData> FindOtherAwards(int taskID)
        {
            List<AwardsItemData> awardsList = null;
            lock (_OtherAwardsDict)
            {
                if (_OtherAwardsDict.TryGetValue(taskID, out awardsList))
                {
                    return awardsList;
                }
            }

            //先查找缓存
            //SystemXmlItem systemTask = null;
            //if (!GameManager.SystemTasksMgr.SystemXmlItemDict.TryGetValue(taskID, out systemTask))
            //{
            //    return null;
            //}

            List<AwardsItemData> taskAwardsList = null;
            //ParseAwards(systemTask, out taskAwardsList, out awardsList);

            if (null != awardsList)
            {
                lock (_OtherAwardsDict)
                {
                    _OtherAwardsDict[taskID] = awardsList;
                }
            }

            return awardsList;
        }

        /// <summary>
        /// 查找金钱
        /// </summary>
        /// <param name="taskID"></param>
        /// <returns></returns>
        public int FindMoney(int taskID)
        {
            int money = -1;
            lock (_MoneyDict)
            {
                if (_MoneyDict.TryGetValue(taskID, out money))
                {
                    return money;
                }
            }

            //SystemXmlItem systemTask = null;
            //if (!GameManager.SystemTasksMgr.SystemXmlItemDict.TryGetValue(taskID, out systemTask))
            //{
            //    return money;
            //}

            //money = systemTask.GetIntValue("BindMoneyaward");
            lock (_MoneyDict)
            {
                _MoneyDict[taskID] = money;
            }

            return money;
        }

        /// <summary>
        /// 查找经验
        /// </summary>
        /// <param name="taskID"></param>
        /// <returns></returns>
        public long FindExperience(KPlayer client, int taskID)
        {
            long experience = -1;
            lock (_ExperienceDict)
            {
                if (_ExperienceDict.TryGetValue(taskID, out experience))
                {
                    if (experience < 0)
                    {
                        //从lua脚本计算
                        experience = CalcLuaScript(client,taskID, null, "ExpLua");
                    }

                    return experience;
                }
            }

            //SystemXmlItem systemTask = null;
            //if (!GameManager.SystemTasksMgr.SystemXmlItemDict.TryGetValue(taskID, out systemTask))
            //{
            //    return experience;
            //}

            //experience = systemTask.GetLongValue("Experienceaward");
            lock (_ExperienceDict)
            {
                _ExperienceDict[taskID] = experience;
            }

            //if (experience < 0)
            //{
            //    //从lua脚本计算
            //    experience = CalcLuaScript(client, taskID, systemTask, "ExpLua");
            //}

            return experience;
        }

        /// <summary>
        /// 查找银两
        /// </summary>
        /// <param name="taskID"></param>
        /// <returns></returns>
        public int FindYinLiang(int taskID)
        {
            int yinLiang = -1;
            lock (_YinLiangDict)
            {
                if (_YinLiangDict.TryGetValue(taskID, out yinLiang))
                {
                    return yinLiang;
                }
            }

            //SystemXmlItem systemTask = null;
            //if (!GameManager.SystemTasksMgr.SystemXmlItemDict.TryGetValue(taskID, out systemTask))
            //{
            //    return yinLiang;
            //}

            //yinLiang = systemTask.GetIntValue("Moneyaward");
            lock (_YinLiangDict)
            {
                _YinLiangDict[taskID] = yinLiang;
            }

            return yinLiang;
        }

        /// <summary>
        /// 查找绑定元宝
        /// </summary>
        /// <param name="taskID"></param>
        /// <returns></returns>
        public int FindBindYuanBao(int taskID)
        {
            int bindYuanBao = -1;
            lock (_BindYuanBaoDict)
            {
                if (_BindYuanBaoDict.TryGetValue(taskID, out bindYuanBao))
                {
                    return bindYuanBao;
                }
            }

            //SystemXmlItem systemTask = null;
            //if (!GameManager.SystemTasksMgr.SystemXmlItemDict.TryGetValue(taskID, out systemTask))
            //{
            //    return bindYuanBao;
            //}

            //bindYuanBao = systemTask.GetIntValue("BindYuanBao");
            lock (_BindYuanBaoDict)
            {
                _BindYuanBaoDict[taskID] = bindYuanBao;
            }

            return bindYuanBao;
        }

        /// <summary>
        /// 查找灵力
        /// </summary>
        /// <param name="taskID"></param>
        /// <returns></returns>
        public int FindLingLi(int taskID)
        {
            int lingLi = -1;
            lock (_LingLiDict)
            {
                if (_LingLiDict.TryGetValue(taskID, out lingLi))
                {
                    return lingLi;
                }
            }

            //SystemXmlItem systemTask = null;
            //if (!GameManager.SystemTasksMgr.SystemXmlItemDict.TryGetValue(taskID, out systemTask))
            //{
            //    return lingLi;
            //}

            //lingLi = systemTask.GetIntValue("LingLi");
            lock (_LingLiDict)
            {
                _LingLiDict[taskID] = lingLi;
            }

            return lingLi;
        }

        /// <summary>
        /// 查找坐骑祝福点
        /// </summary>
        /// <param name="taskID"></param>
        /// <returns></returns>
        public int FindBlessPoint(int taskID)
        {
            int blessPoint = -1;
            lock (_BlessPointDict)
            {
                if (_BlessPointDict.TryGetValue(taskID, out blessPoint))
                {
                    return blessPoint;
                }
            }

            //SystemXmlItem systemTask = null;
            //if (!GameManager.SystemTasksMgr.SystemXmlItemDict.TryGetValue(taskID, out systemTask))
            //{
            //    return blessPoint;
            //}

            //blessPoint = systemTask.GetIntValue("BlessPoint");
            lock (_BlessPointDict)
            {
                _BlessPointDict[taskID] = blessPoint;
            }

            return blessPoint;
        }

        /// <summary>
        /// 查找真气值
        /// </summary>
        /// <param name="taskID"></param>
        /// <returns></returns>
        public int FindZhenQi(KPlayer client, int taskID)
        {
            int zhenQi = -1;
            lock (_ZhenQiDict)
            {
                if (_ZhenQiDict.TryGetValue(taskID, out zhenQi))
                {
                    if (zhenQi < 0)
                    {
                        //从lua脚本计算
                        zhenQi = (int)CalcLuaScript(client, taskID, null, "ZhenQiLua");
                    }

                    return zhenQi;
                }
            }

            //SystemXmlItem systemTask = null;
            //if (!GameManager.SystemTasksMgr.SystemXmlItemDict.TryGetValue(taskID, out systemTask))
            //{
            //    return zhenQi;
            //}

            //zhenQi = systemTask.GetIntValue("ZhenQi");
            lock (_ZhenQiDict)
            {
                _ZhenQiDict[taskID] = zhenQi;
            }

            //if (zhenQi < 0)
            //{
            //    //从lua脚本计算
            //    zhenQi = (int)CalcLuaScript(client, taskID, systemTask, "ZhenQiLua");
            //}

            return zhenQi;
        }

        /// <summary>
        /// 查找猎杀值
        /// </summary>
        /// <param name="taskID"></param>
        /// <returns></returns>
        public int FindLieSha(KPlayer client, int taskID)
        {
            int lieSha = -1;
            lock (_LieShaDict)
            {
                if (_LieShaDict.TryGetValue(taskID, out lieSha))
                {
                    if (lieSha < 0)
                    {
                        //从lua脚本计算
                        lieSha = (int)CalcLuaScript(client, taskID, null, "LieShaLua");
                    }

                    return lieSha;
                }
            }

            //SystemXmlItem systemTask = null;
            //if (!GameManager.SystemTasksMgr.SystemXmlItemDict.TryGetValue(taskID, out systemTask))
            //{
            //    return lieSha;
            //}

            //lieSha = systemTask.GetIntValue("LieSha");
            lock (_LieShaDict)
            {
                _LieShaDict[taskID] = lieSha;
            }

            //if (lieSha < 0)
            //{
            //    //从lua脚本计算
            //    lieSha = (int)CalcLuaScript(client, taskID, systemTask, "LieShaLua");
            //}

            return lieSha;
        }

        /// <summary>
        /// 查找悟性值
        /// </summary>
        /// <param name="taskID"></param>
        /// <returns></returns>
        public int FindWuXing(KPlayer client, int taskID)
        {
            int wuXing = -1;
            lock (_WuXingDict)
            {
                if (_WuXingDict.TryGetValue(taskID, out wuXing))
                {
                    if (wuXing < 0)
                    {
                        //从lua脚本计算
                        wuXing = (int)CalcLuaScript(client, taskID, null, "WuXingLua");
                    }

                    return wuXing;
                }
            }

            //SystemXmlItem systemTask = null;
            //if (!GameManager.SystemTasksMgr.SystemXmlItemDict.TryGetValue(taskID, out systemTask))
            //{
            //    return wuXing;
            //}

            //wuXing = systemTask.GetIntValue("WuXing");
            lock (_WuXingDict)
            {
                _WuXingDict[taskID] = wuXing;
            }

            //if (wuXing < 0)
            //{
            //    //从lua脚本计算
            //    wuXing = (int)CalcLuaScript(client, taskID, systemTask, "WuXingLua");
            //}

            return wuXing;
        }

        /// <summary>
        /// 查找元宝完成需要消耗的元宝
        /// </summary>
        /// <param name="taskID"></param>
        /// <returns></returns>
        public int FindNeedYuanBao(KPlayer client, int taskID)
        {
            int needYuanBao = -1;
            //lock (_NeedYuanBaoDict)
            //{
            //    if (_NeedYuanBaoDict.TryGetValue(taskID, out needYuanBao))
            //    {
            //        return needYuanBao;
            //    }
            //}

            //SystemXmlItem systemTask = null;
            //if (!GameManager.SystemTasksMgr.SystemXmlItemDict.TryGetValue(taskID, out systemTask))
            //{
            //    return needYuanBao;
            //}

            //从lua脚本计算
            //needYuanBao = (int)CalcLuaScript(client, taskID, systemTask, "DoubleAwardLua");

            //lock (_NeedYuanBaoDict)
            //{
            //    _NeedYuanBaoDict[taskID] = needYuanBao;
            //}

            return needYuanBao;
        }

        /// <summary>
        /// 查找军功
        /// </summary>
        /// <param name="taskID"></param>
        /// <returns></returns>
        public int FindJunGong(KPlayer client, int taskID)
        {
            int junGong = -1;
            lock (_JunGongDict)
            {
                if (_JunGongDict.TryGetValue(taskID, out junGong))
                {
                    return junGong;
                }
            }

            //SystemXmlItem systemTask = null;
            //if (!GameManager.SystemTasksMgr.SystemXmlItemDict.TryGetValue(taskID, out systemTask))
            //{
            //    return junGong;
            //}

            //junGong = systemTask.GetIntValue("JunGong");
            lock (_JunGongDict)
            {
                _JunGongDict[taskID] = junGong;
            }

            return junGong;
        }

        /// <summary>
        /// 查找荣誉
        /// </summary>
        /// <param name="taskID"></param>
        /// <returns></returns>
        public int FindRongYu(KPlayer client, int taskID)
        {
            int rongYu = -1;
            lock (_RongYuDict)
            {
                if (_RongYuDict.TryGetValue(taskID, out rongYu))
                {
                    return rongYu;
                }
            }

            //SystemXmlItem systemTask = null;
            //if (!GameManager.SystemTasksMgr.SystemXmlItemDict.TryGetValue(taskID, out systemTask))
            //{
            //    return rongYu;
            //}

            //rongYu = systemTask.GetIntValue("RongYu");
            lock (_RongYuDict)
            {
                _RongYuDict[taskID] = rongYu;
            }

            return rongYu;
        }

        /// <summary>
        /// 魔晶-天地精元 [4/10/2014 LiaoWei]
        /// </summary>
        /// <param name="taskID"></param>
        /// <returns></returns>
        public int FindMoJing(KPlayer client, int taskID)
        {
            int nJingYuan = -1;
            lock (_JingYuanDict)
            {
                if (_JingYuanDict.TryGetValue(taskID, out nJingYuan))
                {
                    return nJingYuan;
                }
            }

            //SystemXmlItem systemTask = null;
            //if (!GameManager.SystemTasksMgr.SystemXmlItemDict.TryGetValue(taskID, out systemTask))
            //{
            //    return nJingYuan;
            //}

            //nJingYuan = systemTask.GetIntValue("MoJing");
            lock (_JingYuanDict)
            {
                _JingYuanDict[taskID] = nJingYuan;
            }

            return nJingYuan;
        }

        /// <summary>
        /// 星魂 [8/11/2014 LiaoWei]
        /// </summary>
        /// <param name="taskID"></param>
        /// <returns></returns>
        public int FindXingHun(KPlayer client, int taskID)
        {
            int nXinghun = -1;
            lock (_XinHunAwardDict)
            {
                if (_XinHunAwardDict.TryGetValue(taskID, out nXinghun))
                {
                    return nXinghun;
                }
            }

            //SystemXmlItem systemTask = null;
            //if (!GameManager.SystemTasksMgr.SystemXmlItemDict.TryGetValue(taskID, out systemTask))
            //{
            //    return nXinghun;
            //}

            //nXinghun = systemTask.GetIntValue("Xinghun");
            lock (_XinHunAwardDict)
            {
                _XinHunAwardDict[taskID] = nXinghun;
            }

            return nXinghun;
        }

        /// <summary>
        /// 清空所有的缓存
        /// </summary>
        public void ClearAllDictionary()
        {
            lock (_TaskAwardsDict)
            {
                _TaskAwardsDict.Clear();
            }

            lock (_OtherAwardsDict)
            {
                _OtherAwardsDict.Clear();
            }

            lock (_MoneyDict)
            {
                _MoneyDict.Clear();
            }

            lock (_ExperienceDict)
            {
                _ExperienceDict.Clear();
            }

            lock (_YinLiangDict)
            {
                _YinLiangDict.Clear();
            }

            lock (_BindYuanBaoDict)
            {
                _BindYuanBaoDict.Clear();
            }

            lock (_LingLiDict)
            {
                _LingLiDict.Clear();
            }

            lock (_BlessPointDict)
            {
                _BlessPointDict.Clear();
            }

            lock (_ZhenQiDict)
            {
                _ZhenQiDict.Clear();
            }

            lock (_LieShaDict)
            {
                _LieShaDict.Clear();
            }

            lock (_WuXingDict)
            {
                _WuXingDict.Clear();
            }

            lock (_NeedYuanBaoDict)
            {
                _NeedYuanBaoDict.Clear();
            }

            lock (_JunGongDict)
            {
                _JunGongDict.Clear();
            }

            lock (_RongYuDict)
            {
                _RongYuDict.Clear();
            }
        }

        #endregion 缓存接口

        #region lua脚本接口

        /// <summary>
        /// 从lua脚本计算
        /// </summary>
        /// <param name="taskID"></param>
        /// <param name="itemName"></param>
        /// <returns></returns>
        private long CalcLuaScript(KPlayer client, int taskID, SystemXmlItem systemTask, string itemName)
        {
            //if (null == systemTask)
            //{
            //    if (!GameManager.SystemTasksMgr.SystemXmlItemDict.TryGetValue(taskID, out systemTask))
            //    {
            //        return -1;
            //    }
            //}

            long ret = -1;
            string luaScriptFileName = systemTask.GetStringValue(itemName);
            if (string.IsNullOrEmpty(luaScriptFileName))
            {
                return ret;
            }
            
            //生成完整的脚本文件路径
            luaScriptFileName = DataHelper.CurrentDirectory + @"scripts/tasks/" + luaScriptFileName;

            //执行对话脚本
            object[] result = Global.ExcuteLuaFunction(client, luaScriptFileName, "calcTaskAwards", null, null);
            if (null != result && result.Length > 0)
            {
                ret = (long)result[0];
            }

            return ret;
        }

        #endregion lua脚本接口
    }
}
