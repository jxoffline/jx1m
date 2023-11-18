using GameServer.Core.GameEvent;
using GameServer.KiemThe;
using GameServer.KiemThe.Logic;
using GameServer.Server;
using System.Collections.Generic;
using Tmsk.Contract;

namespace GameServer.Logic
{
    public class GameCoreInterface : ICoreInterface
    {
        private static GameCoreInterface CoreInterface = new GameCoreInterface();

        public static GameCoreInterface getinstance()
        {
            return CoreInterface;
        }

        /// <summary>
        /// 提供公共的运行时键值对存储
        /// </summary>
        private Dictionary<string, string> RuntimeVariableDict = new Dictionary<string, string>();

        #region 接口实现

        public int GetNewFuBenSeqId()
        {
            int nSeqID = -1;
            try
            {
                //从DBServer获取副本顺序ID
                string[] dbFields = Global.ExecuteDBCmd((int)TCPGameServerCmds.CMD_DB_GETFUBENSEQID, string.Format("{0}", 0), GameManager.LocalServerId);
                if (null != dbFields && dbFields.Length >= 2)
                {
                    nSeqID = Global.SafeConvertToInt32(dbFields[1]);
                }
            }
            catch (System.Exception ex)
            {
                nSeqID = -1;
            }

            return nSeqID;
        }

        public int GetLocalServerId()
        {
            return GameManager.ServerId;
        }

        public ISceneEventSource GetEventSourceInterface()
        {
            return GlobalEventSource4Scene.getInstance();
        }

        public string GetGameConfigStr(string name, string defVal)
        {
            return GameManager.GameConfigMgr.GetGameConfigItemStr(name, defVal);
        }

        public PlatformTypes GetPlatformType()
        {
            return GameManager.PlatformType;
        }

        /// <summary>
        /// 设置运行时变量的值
        /// </summary>
        /// <param name="name"></param>
        /// <param name="val"></param>
        public void SetRuntimeVariable(string name, string val)
        {
            if (null == name)
            {
                return;
            }

            lock (RuntimeVariableDict)
            {
                RuntimeVariableDict[name] = val;
            }
        }

        /// <summary>
        /// 获取运行时变量的值
        /// </summary>
        /// <param name="name"></param>
        /// <param name="defVal"></param>
        /// <returns></returns>
        public string GetRuntimeVariable(string name, string defVal)
        {
            if (null == name)
            {
                return defVal;
            }

            lock (RuntimeVariableDict)
            {
                string val;
                if (RuntimeVariableDict.TryGetValue(name, out val))
                {
                    return val;
                }
            }

            return defVal;
        }

        /// <summary>
        /// 获取运行时变量的值(int)
        /// </summary>
        /// <param name="name"></param>
        /// <param name="defVal"></param>
        /// <returns></returns>
        public int GetRuntimeVariable(string name, int defVal)
        {
            if (null == name)
            {
                return defVal;
            }

            lock (RuntimeVariableDict)
            {
                string val;
                if (RuntimeVariableDict.TryGetValue(name, out val))
                {
                    int ret;
                    if (int.TryParse(val, out ret))
                    {
                        return ret;
                    }
                }
            }

            return defVal;
        }

        public string GetLocalAddressIPs()
        {
            return KTGlobal.GetLocalAddressIPs();
        }

        public int GetMapClientCount(int mapCode)
        {
            return KTPlayerManager.GetPlayersCount(mapCode);
        }

        #endregion 接口实现
    }   //class
}   //namespace