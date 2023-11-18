using GameServer.KiemThe.Entities;
using GameServer.Server;
using Server.Tools;
using System.Collections;
using System.Threading;

namespace GameServer.Logic
{
    public class EventLogManager
    {
        #region 日志对象

        private static long _LogId;
        private static long LogId { get { return Interlocked.Increment(ref _LogId); } }

        private const string NA = "-1"; //"N/A"

        public static ServerEvents[] SystemRoleEvents = new ServerEvents[(int)RoleEvent.EventMax];

        static EventLogManager()
        {
            Init();
        }

        private static void Init()
        {
            for (int i = 0; i < (int)RoleEvent.EventMax; i++)
            {
                SystemRoleEvents[i] = new ServerEvents() { EventRootPath = "Events", EventPreFileName = ((RoleEvent)i).ToString() };
                SystemRoleEvents[i].EventLevel = GameManager.SystemServerEvents.EventLevel;
            }
        }

        public static void WriteAllEvents()
        {
            for (int i = 0; i < (int)RoleEvent.EventMax; i++)
            {
                while (SystemRoleEvents[i].WriteEvent()) ;
            }
        }

        #endregion 日志对象

        #region 角色行为日志

        /// <summary>
        /// 添加货币、代币日志
        /// </summary>
        /// <param name="client">角色对象</param>
        /// <param name="optType">操作类型</param>
        /// <param name="optTag"></param>
        /// <param name="addValue">正负值</param>
        /// <param name="curValue">如果不知道，可以传-1</param>
        /// <param name="msg">附加消息</param>
        public static void AddMoneyEvent(KPlayer client, OpTypes optType, OpTags optTag, MoneyType moneyType, long addValue, long curValue = -1, string msg = "NONE")
        {
            try
            {
                if (!GameManager.FlagEnableMoneyEventLog)
                {
                    return;
                }

                
                if (optType == OpTypes.AddOrSub && addValue == 0)
                {
                    return;
                }

              
                if (curValue == -1)
                {
                    switch (moneyType)
                    {
                        case MoneyType.Bac:
                            curValue = client.Money;
                            break;

                        case MoneyType.BacKhoa:
                            curValue = client.BoundMoney;
                            break;

                        case MoneyType.Dong:
                            curValue = client.Token;
                            break;

                        case MoneyType.DongKhoa:
                            curValue = client.BoundMoney;

                            break;
                        case MoneyType.StoreMoney:
                            curValue = client.StoreMoney;

                            break;

                    }
                }

                AddMoneyEvent(client.ServerId, client.ZoneID, client.strUserID, client.RoleID, optType, optTag, moneyType, addValue, curValue, msg);
            }
            catch (System.Exception ex)
            {
                LogManager.WriteLog(LogTypes.Exception, ex.ToString());
            }
        }


        public static void AddMoneyEvent(int serverId, int zoneId, string userId, long roleId, OpTypes optType, OpTags optTag, MoneyType moneyType, long addValue, long curValue, string msg)
        {
            try
            {
                if (!GameManager.FlagEnableMoneyEventLog)
                {
                    return;
                }

                if (zoneId == 0)
                {
                    zoneId = CacheManager.GetZoneIdByRoleId(roleId, serverId);
                }

                string eventMsg = string.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}\t{8}\t{9}\t{10}",
                    (int)LogRecordType.MoneyEvent,
                    serverId,
                    zoneId,
                    userId,
                    roleId,
                    (int)moneyType,
                    (int)optType,
                    (int)optTag,
                    addValue,
                    curValue,
                    msg
                    );

                SystemRoleEvents[(int)RoleEvent.MoneyEvent].AddEvent(eventMsg, EventLevels.Important);
            }
            catch (System.Exception ex)
            {
                LogManager.WriteLog(LogTypes.Exception, ex.ToString());
            }
        }

        public static void AddGoodsEvent(KPlayer client, OpTypes optType, OpTags optTag, int goodsId, long dbId, int addValue, int curValue, string msg)
        {
            try
            {
             
                if (addValue == 0)
                {
                    return;
                }

                if (!GameManager.FlagEnableGoodsEventLog)
                {
                    return;
                }

                int serverId = client.ServerId;
                string userID = client.strUserID;
                long roleId = client.RoleID;
                int zoneId = client.ZoneID;

                string eventMsg = string.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}\t{8}\t{9}\t{10}\t{11}",
                    (int)LogRecordType.GoodsEvent,
                    serverId,
                    zoneId,
                    userID,
                    roleId,
                    (int)optType,
                    (int)optTag,
                    goodsId,
                    dbId,
                    addValue,
                    curValue,
                    msg
                    );

                SystemRoleEvents[(int)RoleEvent.GoodsEvent].AddEvent(eventMsg, EventLevels.Important);
            }
            catch (System.Exception ex)
            {
                LogManager.WriteLog(LogTypes.Exception, ex.ToString());
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="client"></param>
        /// <param name="optType"></param>
        /// <param name="optTag"></param>
        /// <param name="logRecordType">每个写日志的地方，都应该在LogRecordType中增加一个定义，格式不要修改，只改数字；
        /// 原则上，上线运行后，修改记录参数的个数和内容，也应该重新定义一个新的ID,以保证日志分析程序能正确的分析原来的格式和新的格式;
        /// </param>
        /// <param name="args">所传参数必须能默认转化为字符串形式表示其值</param>
        public static void AddGameEvent(LogRecordType logRecordType, params object[] args)
        {
            try
            {
                if (!GameManager.FlagEnableGameEventLog)
                {
                    return;
                }

                int serverId = GameManager.ServerId;
                string eventMsg = string.Format("{0}\t{1}",
                    (int)logRecordType,
                    serverId
                    );

                if (logRecordType == LogRecordType.Json)
                {
                    if (args[0].GetType() == typeof(Hashtable))
                    {
                        eventMsg += "\t" + MUJson.jsonEncode(args[0]);
                    }
                    else
                    {
                        Hashtable table = new Hashtable();
                        for (int i = 0; i < args.Length - 1; i += 2)
                        {
                            table.Add(args[i], args[1]);
                        }

                        eventMsg += "\t" + MUJson.jsonEncode(table);
                    }
                }
                else
                {
                    foreach (var arg in args)
                    {
                        eventMsg += "\t" + arg.ToString();
                    }
                }

                SystemRoleEvents[(int)RoleEvent.GameEvent].AddEvent(eventMsg, EventLevels.Important);
            }
            catch (System.Exception ex)
            {
                LogManager.WriteLog(LogTypes.Exception, ex.ToString());
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="client"></param>
        /// <param name="optType"></param>
        /// <param name="optTag"></param>
        /// <param name="logRecordType"></param>
        /// <param name="args"></param>
        public static void AddRoleEvent(KPlayer client, OpTypes optType, OpTags optTag, LogRecordType logRecordType, params object[] args)
        {
            try
            {
                if (!GameManager.FlagEnableOperatorEventLog)
                {
                    return;
                }

                string eventMsg = string.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}",
                    (int)logRecordType,
                    client.ServerId,
                    client.ZoneID,
                    client.strUserID,
                    client.RoleID,
                    (int)optType,
                    (int)optTag
                    );

                foreach (var arg in args)
                {
                    eventMsg += "\t" + arg;
                }

                SystemRoleEvents[(int)RoleEvent.OperatorEvent].AddEvent(eventMsg, EventLevels.Important);
            }
            catch (System.Exception ex)
            {
                LogManager.WriteLog(LogTypes.Exception, ex.ToString());
            }
        }

        public static void AddRoleSkillEvent(KPlayer client, SkillLogTypes optType, LogRecordType logRecordType, params object[] args)
        {
            try
            {
                if (!GameManager.FlagEnableRoleSkillLog)
                {
                    return;
                }

                string eventMsg = string.Format("{0}\t{1}\t{2}\t{3}\t{4}",
                    (int)logRecordType,
                    client.ServerId,
                    client.strUserID,
                    client.RoleID,
                    (int)optType
                    );

                foreach (var arg in args)
                {
                    eventMsg += "\t" + arg;
                }

                SystemRoleEvents[(int)RoleEvent.RoleSkill].AddEvent(eventMsg, EventLevels.Important);
            }
            catch (System.Exception ex)
            {
                LogManager.WriteLog(LogTypes.Exception, ex.ToString());
            }
        }

        public static void AddUnionPalaceEvent(KPlayer client, LogRecordType logRecordType, params object[] args)
        {
            try
            {
                if (!GameManager.FlagEnableUnionPalaceLog)
                {
                    return;
                }

                string eventMsg = string.Format("{0}\t{1}\t{2}\t{3}",
                    (int)logRecordType,
                    client.ServerId,
                    client.strUserID,
                    client.RoleID
                    );

                foreach (var arg in args)
                {
                    eventMsg += "\t" + arg;
                }

                SystemRoleEvents[(int)RoleEvent.UnionPalace].AddEvent(eventMsg, EventLevels.Important);
            }
            catch (System.Exception ex)
            {
                LogManager.WriteLog(LogTypes.Exception, ex.ToString());
            }
        }

        #endregion 角色行为日志
    }
}