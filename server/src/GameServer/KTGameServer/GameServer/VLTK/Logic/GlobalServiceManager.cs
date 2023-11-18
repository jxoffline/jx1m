
using GameServer.KiemThe;
using GameServer.KiemThe.Core.Task;
using GameServer.KiemThe.GameEvents.FactionBattle;
using GameServer.KiemThe.Logic.Manager.Battle;
using Server.Tools;
using System;
using System.Collections.Generic;
using Tmsk.Contract;

namespace GameServer.Logic
{
 
    public interface IManager
    {
        bool initialize();

        bool startup();

        bool showdown();

        bool destroy();
    }
  
    public class GlobalServiceManager
    {
        private static Dictionary<int, List<IManager>> Scene2ManagerDict = new Dictionary<int, List<IManager>>();


        public static bool RegisterManager4Scene(int ManagerType, IManager manager)
        {
            lock (Scene2ManagerDict)
            {
                List<IManager> list;
                if (!Scene2ManagerDict.TryGetValue(ManagerType, out list))
                {
                    list = new List<IManager>();
                    Scene2ManagerDict[ManagerType] = list;
                }

                if (!list.Contains(manager))
                {
                    list.Add(manager);
                }
            }

            return true;
        }
        
        public static void Initialize()
        {
            Battel_SonJin_Manager.BattleStatup();

            FactionBattleManager.BattleStatup();


            if (ServerConfig.Instance.EnableCrossServer)
            {
                ////REGISTER LIÊN SV
                RegisterManager4Scene((int)SceneUIClasses.Normal, KuaFuManager.getInstance());
                RegisterManager4Scene((int)SceneUIClasses.KuaFuMap, KuaFuMapManager.getInstance());
                RegisterManager4Scene((int)SceneUIClasses.YongZheZhanChang, YongZheZhanChangManager.getInstance());
            }

            lock (Scene2ManagerDict)
            {
                foreach (var list in Scene2ManagerDict.Values)
                {
                    foreach (var m in list)
                    {
                        bool success = m.initialize();
                        IManager2 m2 = m as IManager2;
                        if (null != m2)
                        {
                            success = success && m2.initialize(GameCoreInterface.getinstance());
                        }

                        if (GameManager.ServerStarting && !success)
                        {
                            LogManager.WriteLog(LogTypes.Fatal, string.Format("Gloal Servicie initialize Bug0}!", m.GetType()));
                            Console.ReadKey();
                        }
                    }
                }
            }

        }

        public static void Startup()
        {
          
            lock (Scene2ManagerDict)
            {
                foreach (var list in Scene2ManagerDict.Values)
                {
                    foreach (var m in list)
                    {
                        bool success = m.startup();
                        if (GameManager.ServerStarting && !success)
                        {
                            LogManager.WriteLog(LogTypes.Fatal, string.Format("Gloal Servicie startup Bug  {0}!", m.GetType()));
                            Console.ReadKey();
                        }
                    }
                }
            }
        }

        public static void Showdown()
        {
            Battel_SonJin_Manager.ShutDown();
            FactionBattleManager.ShutDown();

            lock (Scene2ManagerDict)
            {
                foreach (var list in Scene2ManagerDict.Values)
                {
                    foreach (var m in list)
                    {
                        m.showdown();
                    }
                }
            }
        }

        public static void Destroy()
        {

            lock (Scene2ManagerDict)
            {
                foreach (var list in Scene2ManagerDict.Values)
                {
                    foreach (var m in list)
                    {
                        m.destroy();
                    }
                }
            }
        }
    }
}