using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using KF.Contract.Interface;

using ProtoBuf;

#region 声明友元程序集

[assembly: InternalsVisibleTo("KF.Remoting.HuanYingSiYuan")]
[assembly: InternalsVisibleTo("KF.Client.HuanYingSiYuan")]

#endregion 声明友元程序集

namespace KF.Contract.Data
{
    public struct KuaFuRoleId : IEqualityComparer<KuaFuRoleId>
    {
        public int ServerId;
        public int RoleId;

        public bool Equals(KuaFuRoleId x, KuaFuRoleId y)
        {
            if (x.RoleId != y.RoleId)
            {
                return false;
            }

            return x.ServerId == y.ServerId;
        }

        public int GetHashCode(KuaFuRoleId obj)
        {
            return obj.RoleId;
        }
    }

    [Serializable]
    [ProtoContract]
    public class KuaFuRoleMiniData
    {
        [ProtoMember(1)]
        public int RoleId;
        [ProtoMember(2)]
        public int ZoneId;
        [ProtoMember(3)]
        public string RoleName;
    }

    [Serializable]
    public class KuaFuRoleData : ICloneable
    {
        public int Age { get; set; }  
        public string UserId { get; set; } 
        public int ServerId { get; set; } 
        public int ZoneId { get; set; } 
        public int RoleId { get; set; } 
        public KuaFuRoleStates State { get; set; } 
        public int GameId { get; set; } 
        public int GameType { get; set; } 
        public int GroupIndex { get; set; } 
        public int KuaFuServerId { get; set; } 
        public int ZhanDouLi { get; set; }
        public long StateEndTicks { get; set; }
        public IGameData GameData { get; set; }
        public int TeamCombatAvg { get; set; } // 队伍平均战斗力

        [NonSerialized()]
        public KuaFuRoleData Next;

        public object Clone()
        {
            return new KuaFuRoleData()
            {
                Age = Age,
                UserId = UserId,
                ServerId = ServerId,
                ZoneId = ZoneId,
                RoleId = RoleId,
                State = State,
                GameId = GameId,
                GameType = GameType,
                GroupIndex = GroupIndex,
                ZhanDouLi = ZhanDouLi,
                StateEndTicks = StateEndTicks,
                GameData = null == GameData ? null : (IGameData)GameData.Clone(),
                TeamCombatAvg = TeamCombatAvg,
            };
        }

        public void UpdateStateTime(int gameId, KuaFuRoleStates state, long stateEndTicks)
        {
            lock (this)
            {
                Age++;
                GameId = gameId;
                State = state;
                StateEndTicks = stateEndTicks;
            }
        }
    }

    [Serializable]
    public class KuaFuMapRoleData
    {
        public int ServerId;
        public int RoleId;
        public int KuaFuServerId;
        public int KuaFuMapCode;
    }

    public class KuaFuServerGameConfig
    {
        public int ServerId;
        public int GameType;
        public int Capacity;
    }

    [Serializable]
    public class KuaFuServerInfo
    {
        public int ServerId;
        public string Ip;
        public int Port;
        public string DbIp;
        public int DbPort;
        public string LogDbIp;
        public int LogDbPort;
        public int State;
        public int Flags;
        public int Age;
        public int Load;
    }

    [Serializable]
    public class KuaFuFuBenRoleData
    {
        public int ServerId { get; set; }
        public int RoleId { get; set; } 
        public int Side { get; set; } 
        public int GameId { get; set; } 
        public int ZhanDouLi { get; set; } 
    }

    [Serializable]
    public class HuanYingSiYuanFuBenData : IKuaFuFuBenData
    {
        public int GameId { get; set; } 
        public int ServerId { get; set; } 
        public int GroupIndex { get; set; } 
        public GameFuBenState State { get; set; } 
        public int Age { get; set; } 
        public DateTime EndTime { get; set; } 

        public int RoleCountSide1 { get; set; } 
        public int RoleCountSide2 { get; set; }

        public Dictionary<int, KuaFuFuBenRoleData> RoleDict { get; set; }

        public HuanYingSiYuanFuBenData()
        {
            RoleDict = new Dictionary<int, KuaFuFuBenRoleData>();
        }

        public HuanYingSiYuanFuBenData Clone()
        {
            lock (this)
            {
                return new HuanYingSiYuanFuBenData()
                {
                    GameId = GameId,
                    ServerId = ServerId,
                    GroupIndex = GroupIndex,
                    State = State,
                    Age = Age,
                    EndTime = EndTime,
                    RoleDict = new  Dictionary<int, KuaFuFuBenRoleData>(RoleDict),
                };
            }
        }

        public int AddKuaFuFuBenRoleData(KuaFuFuBenRoleData kuaFuFuBenRoleData, GameFuBenRoleCountChanged handler)
        {
            int roleCount = -1;

            lock (this)
            {
                if (RoleDict.Count < Consts.HuanYingSiYuanRoleCountTotal && !RoleDict.ContainsKey(kuaFuFuBenRoleData.RoleId))
                {
                    if (RoleCountSide1 < Consts.HuanYingSiYuanRoleCountPerSide)
                    {
                        RoleCountSide1++;
                        kuaFuFuBenRoleData.Side = 1;
                    }
                    else if (RoleCountSide2 < Consts.HuanYingSiYuanRoleCountPerSide)
                    {
                        RoleCountSide2++;
                        kuaFuFuBenRoleData.Side = 2;
                    }
                    else
                    {
                        return roleCount;
                    }

                    RoleDict[kuaFuFuBenRoleData.RoleId] = kuaFuFuBenRoleData;
                    roleCount = RoleDict.Count;
                    if (null != handler)
                    {
                        handler(this, roleCount);
                    }

                    return roleCount;
                }
            }

            return roleCount;
        }

        public int RemoveKuaFuFuBenRoleData(int roleId, GameFuBenRoleCountChanged handler)
        {
            int roleCount;
            bool changed = false;
            lock (this)
            {
                KuaFuFuBenRoleData kuaFuFuBenRoleData;
                if (RoleDict.TryGetValue(roleId, out kuaFuFuBenRoleData))
                {
                    RoleDict.Remove(roleId);
                    changed = true;
                    if (kuaFuFuBenRoleData.Side == 1)
                    {
                        RoleCountSide1--;
                    }
                    else
                    {
                        RoleCountSide2--;
                    }
                }

                roleCount = RoleDict.Count;
                if (changed && null != handler)
                {
                    handler(this, roleCount);
                }

                return roleCount;
            }
        }

        public int GetFuBenRoleCount()
        {
            lock (this)
            {
                return RoleDict.Count;
            }
        }

        public bool CanRemove()
        {
            lock (this)
            {
                if (State == GameFuBenState.End)
                {
                    return true;
                }
                else if (State == GameFuBenState.Start && RoleDict.Count == 0)
                {
                    return true;
                }
            }

            return false;
        }

        public bool CanEnter(int groupIndex, long waitTicks1, long waitTicks2)
        {
            if (State == GameFuBenState.Wait)
            {
                if (EndTime.Ticks < waitTicks2)
                {
                    return true;
                }
                else if (EndTime.Ticks < waitTicks1)
                {
                    if (groupIndex >= GroupIndex - 1 && groupIndex <= GroupIndex + 1)
                    {
                        return true;
                    }
                }
            }

            return GroupIndex == groupIndex;
        }

        public List<KuaFuFuBenRoleData> SortFuBenRoleList()
        {
            lock (this)
            {
                List<KuaFuFuBenRoleData> roleList = RoleDict.Values.ToList();
                roleList.Sort((x, y) => { return x.ZhanDouLi - y.ZhanDouLi; });
                for (int i = 0; i < roleList.Count; i++)
                {
                    int r = i % 4;
                    if (r == 0 || r == 3)
                    {
                        roleList[i].Side = 1;
                    }
                    else
                    {
                        roleList[i].Side = 2;
                    }
                }

                return roleList;
            }
        }
    }

    [Serializable]
    public class TianTiFuBenData : IKuaFuFuBenData
    {
        public int GameId { get; set; } 
        public int ServerId { get; set; } 
        public int GroupIndex { get; set; } 
        public GameFuBenState State { get; set; } 
        public int Age { get; set; } 
        public DateTime EndTime { get; set; } 

        public int RoleCountSide1 { get; set; } 
        public int RoleCountSide2 { get; set; }

        public Dictionary<int, KuaFuFuBenRoleData> RoleDict { get; set; }

        public TianTiFuBenData()
        {
            RoleDict = new Dictionary<int, KuaFuFuBenRoleData>();
        }

        public TianTiFuBenData Clone()
        {
            lock (this)
            {
                return new TianTiFuBenData()
                {
                    GameId = GameId,
                    ServerId = ServerId,
                    GroupIndex = GroupIndex,
                    State = State,
                    Age = Age,
                    EndTime = EndTime,
                    RoleDict = new  Dictionary<int, KuaFuFuBenRoleData>(RoleDict),
                };
            }
        }

        public int AddKuaFuFuBenRoleData(KuaFuFuBenRoleData kuaFuFuBenRoleData)
        {
            int roleCount = -1;

            lock (this)
            {
                if (RoleDict.Count < Consts.TianTiRoleCountTotal && !RoleDict.ContainsKey(kuaFuFuBenRoleData.RoleId))
                {
                    if (RoleCountSide1 < Consts.TianTiRoleCountPerSide)
                    {
                        RoleCountSide1++;
                        kuaFuFuBenRoleData.Side = 1;
                    }
                    else if (RoleCountSide2 < Consts.TianTiRoleCountPerSide)
                    {
                        RoleCountSide2++;
                        kuaFuFuBenRoleData.Side = 2;
                    }
                    else
                    {
                        return roleCount;
                    }

                    RoleDict[kuaFuFuBenRoleData.RoleId] = kuaFuFuBenRoleData;
                    roleCount = RoleDict.Count;
                    return roleCount;
                }
            }

            return roleCount;
        }

        public int RemoveKuaFuFuBenRoleData(int roleId)
        {
            int roleCount;
            lock (this)
            {
                KuaFuFuBenRoleData kuaFuFuBenRoleData;
                if (RoleDict.TryGetValue(roleId, out kuaFuFuBenRoleData))
                {
                    RoleDict.Remove(roleId);
                    if (kuaFuFuBenRoleData.Side == 1)
                    {
                        RoleCountSide1--;
                    }
                    else
                    {
                        RoleCountSide2--;
                    }
                }

                roleCount = RoleDict.Count;
                return roleCount;
            }
        }

        public int GetFuBenRoleCount()
        {
            lock (this)
            {
                return RoleDict.Count;
            }
        }

        public bool CanRemove()
        {
            lock (this)
            {
                if (State == GameFuBenState.End)
                {
                    return true;
                }
                else if (State == GameFuBenState.Start && RoleDict.Count == 0)
                {
                    return true;
                }
            }

            return false;
        }
    }

    public class YongZheZhanChangGameFuBenPreAssignData
    {
        public List<YongZheZhanChangFuBenData> FullFuBenDataList = new List<YongZheZhanChangFuBenData>();
        public YongZheZhanChangFuBenData RemainFuBenData;
    }

    [Serializable]
    public class YongZheZhanChangFuBenData : IKuaFuFuBenData
    {
        public int GameId { get; set; } 
        public int ServerId { get; set; } 
        public int SequenceId { get; set; } 
        public int GroupIndex { get; set; } 
        public GameFuBenState State { get; set; } 
        public int Age { get; set; } 
        public DateTime EndTime { get; set; } 

        public int RoleCountSide1 { get; set; } 
        public int RoleCountSide2 { get; set; }

        public Dictionary<int, KuaFuFuBenRoleData> RoleDict { get; set; }

        public YongZheZhanChangFuBenData()
        {
            RoleDict = new Dictionary<int, KuaFuFuBenRoleData>();
        }

        public YongZheZhanChangFuBenData Clone()
        {
            lock (this)
            {
                return new YongZheZhanChangFuBenData()
                {
                    GameId = GameId,
                    ServerId = ServerId,
                    SequenceId = SequenceId,
                    GroupIndex = GroupIndex,
                    State = State,
                    Age = Age,
                    EndTime = EndTime,
                    RoleCountSide1 = RoleCountSide1,
                    RoleCountSide2 = RoleCountSide2,
                    RoleDict = new  Dictionary<int, KuaFuFuBenRoleData>(RoleDict),
                };
            }
        }

        public int AddKuaFuFuBenRoleData(KuaFuFuBenRoleData kuaFuFuBenRoleData)
        {
            int roleCount = -1;

            lock (this)
            {
                if (RoleDict.Count < Consts.YongZheZhanChangRoleCountTotal && !RoleDict.ContainsKey(kuaFuFuBenRoleData.RoleId))
                {
                    if (RoleCountSide1 < RoleCountSide2)
                    {
                        RoleCountSide1++;
                        kuaFuFuBenRoleData.Side = 1;
                    }
                    else
                    {
                        RoleCountSide2++;
                        kuaFuFuBenRoleData.Side = 2;
                    }

                    RoleDict[kuaFuFuBenRoleData.RoleId] = kuaFuFuBenRoleData;
                    roleCount = RoleDict.Count;
                    return roleCount;
                }
            }

            return roleCount;
        }

        public int RemoveKuaFuFuBenRoleData(int roleId)
        {
            int roleCount;
            lock (this)
            {
                KuaFuFuBenRoleData kuaFuFuBenRoleData;
                if (RoleDict.TryGetValue(roleId, out kuaFuFuBenRoleData))
                {
                    RoleDict.Remove(roleId);
                    if (kuaFuFuBenRoleData.Side == 1)
                    {
                        RoleCountSide1--;
                    }
                    else
                    {
                        RoleCountSide2--;
                    }
                }

                roleCount = RoleDict.Count;
                return roleCount;
            }
        }

        public int GetFuBenRoleCount()
        {
            lock (this)
            {
                return RoleDict.Count;
            }
        }

        public bool CanRemove()
        {
            lock (this)
            {
                if (State == GameFuBenState.End)
                {
                    return true;
                }
            }

            return false;
        }

        public List<KuaFuFuBenRoleData> SortFuBenRoleList()
        {
            lock (this)
            {
                List<KuaFuFuBenRoleData> roleList = RoleDict.Values.ToList();
                roleList.Sort((x, y) => { return x.ZhanDouLi - y.ZhanDouLi; });
                for (int i = 0; i < roleList.Count; i++)
                {
                    int r = i % 4;
                    if (r == 0 || r == 3)
                    {
                        roleList[i].Side = 1;
                    }
                    else
                    {
                        roleList[i].Side = 2;
                    }
                }

                return roleList;
            }
        }
    }

    [Serializable]
    public class LangHunLingYuFuBenData : IKuaFuFuBenData
    {
        public int GameId;
        public int ServerId;
        public int SequenceId;
        public int GroupIndex;
        public GameFuBenState State;
        public int Age;
        public DateTime EndTime;
        public int CityId;

        public LangHunLingYuCityDataEx CityDataEx;
    }

    [Serializable]
    public class MoRiJudgeFuBenData : IKuaFuFuBenData
    {
        public int GameId { get; set; }
        public int ServerId { get; set; }
        public int SequenceId { get; set; }
        public int GroupIndex { get; set; }
        public GameFuBenState State { get; set; }
        public int Age { get; set; }
        public DateTime EndTime { get; set; }
        public long TeamCombatSum { get; set; }
        public Dictionary<int, KuaFuFuBenRoleData> RoleDict { get; set; }

        public MoRiJudgeFuBenData()
        {
            RoleDict = new Dictionary<int, KuaFuFuBenRoleData>();
        }

        public MoRiJudgeFuBenData Clone()
        {
            lock (this)
            {
                return new MoRiJudgeFuBenData()
                {
                    GameId = GameId,
                    ServerId = ServerId,
                    SequenceId = SequenceId,
                    GroupIndex = GroupIndex,
                    State = State,
                    Age = Age,
                    EndTime = EndTime,
                    TeamCombatSum = TeamCombatSum,
                    RoleDict = new Dictionary<int, KuaFuFuBenRoleData>(RoleDict),              
                };
            }
        }

        public int AddKuaFuFuBenRoleData(KuaFuFuBenRoleData kuaFuFuBenRoleData, int maxRoleCount)
        {
            int roleCount = -1;

            lock (this)
            {
                if (RoleDict.Count < maxRoleCount && !RoleDict.ContainsKey(kuaFuFuBenRoleData.RoleId))
                {
                    TeamCombatSum += kuaFuFuBenRoleData.ZhanDouLi;
                    RoleDict[kuaFuFuBenRoleData.RoleId] = kuaFuFuBenRoleData;
                    roleCount = RoleDict.Count;
                    return roleCount;
                }
            }

            return roleCount;
        }

        public int RemoveKuaFuFuBenRoleData(int roleId)
        {
            int roleCount;
            lock (this)
            {
                KuaFuFuBenRoleData kuaFuFuBenRoleData;
                if (RoleDict.TryGetValue(roleId, out kuaFuFuBenRoleData))
                {
                    RoleDict.Remove(roleId);
                    TeamCombatSum -= kuaFuFuBenRoleData.ZhanDouLi;
                    TeamCombatSum = Math.Max(TeamCombatSum, 0);
                }

                roleCount = RoleDict.Count;
                return roleCount;
            }
        }

        public int GetFuBenRoleCount()
        {
            lock (this)
            {
                return RoleDict.Count;
            }
        }

        public bool CanRemove()
        {
            lock (this)
            {
                if (State == GameFuBenState.End)
                {
                    return true;
                }
                else if (State == GameFuBenState.Start && RoleDict.Count == 0)
                {
                    return true;
                }
            }

            return false;
        }

        public List<KuaFuFuBenRoleData> GetRoleList()
        {
            lock (this)
            {
                return RoleDict.Values.ToList();
            }
        }
    }



    [Serializable]    
	public class ElementWarFuBenData : IKuaFuFuBenData
    {
        public int GameId { get; set; }
        public int ServerId { get; set; }
        public int SequenceId { get; set; }
        public GameFuBenState State { get; set; }
        public int Age { get; set; }
        public DateTime EndTime { get; set; }
        public long TeamCombatSum { get; set; }
        public int RoleCount { get; set; }
        public Dictionary<int, KuaFuFuBenRoleData> RoleDict { get; set; }

        public ElementWarFuBenData()
        {
            RoleDict = new Dictionary<int, KuaFuFuBenRoleData>();
        }

        public ElementWarFuBenData Clone()
        {
            lock (this)
            {
                return new ElementWarFuBenData()
                {
                    GameId = GameId,
                    ServerId = ServerId,
                    SequenceId = SequenceId,
                    State = State,
                    Age = Age,
                    EndTime = EndTime,
                    TeamCombatSum = TeamCombatSum,
                    RoleCount = RoleCount,
                    RoleDict = new Dictionary<int, KuaFuFuBenRoleData>(RoleDict),
                };
            }
        }

        public int AddKuaFuFuBenRoleData(KuaFuFuBenRoleData kuaFuFuBenRoleData, int maxRoleCount, GameElementWarRoleCountChanged handler)
        {
            int roleCount = -1;

            lock (this)
            {
                if (RoleDict.Count < maxRoleCount && !RoleDict.ContainsKey(kuaFuFuBenRoleData.RoleId))
                {
                    RoleCount++;

                    TeamCombatSum += kuaFuFuBenRoleData.ZhanDouLi;
                    RoleDict[kuaFuFuBenRoleData.RoleId] = kuaFuFuBenRoleData;
                    roleCount = RoleDict.Count;
                    
                    if (null != handler)
                    {
                        handler(this, roleCount);
                    }

                    return roleCount;
                }
            }

            return roleCount;
        }

        public int RemoveKuaFuFuBenRoleData(int roleId, GameElementWarRoleCountChanged handler)
        {
            int roleCount;
            bool changed = false;
            lock (this)
            {
                KuaFuFuBenRoleData kuaFuFuBenRoleData;
                if (RoleDict.TryGetValue(roleId, out kuaFuFuBenRoleData))
                {
                    RoleDict.Remove(roleId);
                    changed = true;
                    RoleCount--;
                    TeamCombatSum -= kuaFuFuBenRoleData.ZhanDouLi;
                    TeamCombatSum = Math.Max(TeamCombatSum, 0);
                }

                roleCount = RoleDict.Count;

                if (changed && null != handler)
                {
                    handler(this, roleCount);
                }

                return roleCount;
            }
        }

        public int GetFuBenRoleCount()
        {
            lock (this)
            {
                return RoleDict.Count;
            }
        }

        public bool CanRemove()
        {
            lock (this)
            {
                if (State == GameFuBenState.End)
                {
                    return true;
                }
                else if (State == GameFuBenState.Start && RoleDict.Count == 0)
                {
                    return true;
                }
            }

            return false;
        }

        public bool CanEnter(int maxRoleCount)
        {
            if (State == GameFuBenState.Wait)
            {
                if (RoleCount < maxRoleCount)
                {
                    return true;
                }
            }

            return false;
        }
    }


    [Serializable]    
	public class AsyncDataItem
    {
        public KuaFuEventTypes EventType;
        public object[] Args;

        public AsyncDataItem() { }
        public AsyncDataItem(KuaFuEventTypes eventType, params object[] args)
        {
            EventType = eventType;
            Args = args;
        }
    }

    /// <summary>
    public class KuaFuRoleKey : IEquatable<KuaFuRoleKey>
    {
        int RoleId;
        int ServerId;

        public static KuaFuRoleKey Get(int serverId, int roleId)
        {
            return new KuaFuRoleKey(serverId, roleId);
        }

        private KuaFuRoleKey(int serverId, int roleId)
        {
            ServerId = serverId;
            RoleId = roleId;
        }

        public bool Equals(KuaFuRoleKey other)
        {
            return RoleId == other.RoleId && ServerId == other.ServerId;
        }

        public override int GetHashCode()
        {
            return RoleId;
        }

        public override bool Equals(object other)
        {
            KuaFuRoleKey obj = other as KuaFuRoleKey;
            if (null == obj)
            {
                return false;
            }

            return RoleId == obj.RoleId && ServerId == obj.ServerId;
        }
    }
}
