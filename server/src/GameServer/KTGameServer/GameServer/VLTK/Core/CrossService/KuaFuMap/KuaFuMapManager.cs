using GameServer.Core.Executor;
using GameServer.KiemThe;
using GameServer.KiemThe.Logic;
using GameServer.Server;
using KF.Client;
using Server.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Xml.Linq;
using Tmsk.Contract;
using static GameServer.Logic.KTMapManager;

namespace GameServer.Logic
{
    public partial class KuaFuMapManager : IManager, ICmdProcessorEx, IManager2
    {
        private enum EKuaFuMapEnterFlag
        {
            FromMapCode = 0,
            FromTeleport = 1,
            TargetBossId = 2,
            ToMapX = 3,
            ToMapY = 4,
            Max = 5
        }

        #region Quản lý phần liên máy chủ

        public const SceneUIClasses ManagerType = SceneUIClasses.KuaFuMap;

        private static KuaFuMapManager instance = new KuaFuMapManager();

        public static KuaFuMapManager getInstance()
        {
            return instance;
        }

        /// <summary>
        /// Thiết lập xử ý máy chủ liên server
        /// </summary>
        public KuaFuMapData RuntimeData = new KuaFuMapData();

        public bool initialize()
        {
            if (!InitConfig())
            {
                return false;
            }

            return true;
        }

        public bool initialize(ICoreInterface coreInterface)
        {
            ScheduleExecutor2.Instance.scheduleExecute(new NormalScheduleTask("KuaFuBossManager.TimerProc", TimerProc), 15000, 5000);
            return true;
        }

        public bool startup()
        {
            //Statup
            TCPCmdDispatcher.getInstance().registerProcessorEx((int)TCPGameServerCmds.CMD_SPR_KUAFU_MAP_ENTER, 2, 4, getInstance());
            TCPCmdDispatcher.getInstance().registerProcessorEx((int)TCPGameServerCmds.CMD_SPR_KUAFU_MAP_INFO, 1, 1, getInstance());
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

        public bool processCmd(KPlayer client, string[] cmdParams)
        {
            return false;
        }

        public bool processCmdEx(KPlayer client, int nID, byte[] bytes, string[] cmdParams)
        {
            switch (nID)
            {
                case (int)TCPGameServerCmds.CMD_SPR_KUAFU_MAP_ENTER:
                    return ProcessKuaFuMapEnterCmd(client, nID, bytes, cmdParams);

                case (int)TCPGameServerCmds.CMD_SPR_KUAFU_MAP_INFO:
                    return ProcessGetKuaFuLineDataListCmd(client, nID, bytes, cmdParams);
            }

            return true;
        }

        #endregion 标准接口

        #region Initial configuration

        /// <summary>
        ///  Initial configuration
        /// </summary>
        public bool InitConfig()
        {
            bool success = true;
            XElement xml = null;

            string MapLinesPath = "Config/KT_Map/MapLine.xml";
            string fullPathFileName = "";
            IEnumerable<XElement> nodes;

            lock (RuntimeData.Mutex)
            {
                try
                {
                    //Đọc ra MAP LINES
                    RuntimeData.LineMap2KuaFuLineDataDict.Clear();
                    RuntimeData.ServerMap2KuaFuLineDataDict.Clear();
                    RuntimeData.KuaFuMapServerIdDict.Clear();
                    RuntimeData.MapCode2KuaFuLineDataDict.Clear();

                    fullPathFileName = KTGlobal.GetDataPath(MapLinesPath);

                    xml = XElement.Load(fullPathFileName);
                    nodes = xml.Elements();
                    foreach (var node in nodes)
                    {
                        int mapMaxOnlineCount = (int)Global.GetSafeAttributeLong(node, "MaxNum");
                        string str = Global.GetSafeAttributeStr(node, "Line");
                        if (!string.IsNullOrEmpty(str))
                        {
                            string[] mapLineStrs = str.Split('|');
                            foreach (var mapLineStr in mapLineStrs)
                            {
                                KuaFuLineData kuaFuLineData = new KuaFuLineData();
                                string[] mapLineParams = mapLineStr.Split(',');
                                kuaFuLineData.Line = int.Parse(mapLineParams[0]);
                                kuaFuLineData.MapCode = int.Parse(mapLineParams[1]);
                                if (mapLineParams.Length >= 3)
                                {
                                    kuaFuLineData.ServerId = int.Parse(mapLineParams[2]);
                                }

                                kuaFuLineData.MaxOnlineCount = mapMaxOnlineCount;
                                RuntimeData.LineMap2KuaFuLineDataDict.TryAdd(new IntPairKey(kuaFuLineData.Line, kuaFuLineData.MapCode), kuaFuLineData);
                                List<KuaFuLineData> list = null;
                                if (kuaFuLineData.ServerId > 0)
                                {
                                    if (RuntimeData.ServerMap2KuaFuLineDataDict.TryAdd(new IntPairKey(kuaFuLineData.ServerId, kuaFuLineData.MapCode), kuaFuLineData))
                                    {
                                        if (!RuntimeData.KuaFuMapServerIdDict.TryGetValue(kuaFuLineData.ServerId, out list))
                                        {
                                            list = new List<KuaFuLineData>();
                                            RuntimeData.KuaFuMapServerIdDict.TryAdd(kuaFuLineData.ServerId, list);
                                        }

                                        list.Add(kuaFuLineData);
                                    }
                                }

                                if (!RuntimeData.MapCode2KuaFuLineDataDict.TryGetValue(kuaFuLineData.MapCode, out list))
                                {
                                    list = new List<KuaFuLineData>();
                                    RuntimeData.MapCode2KuaFuLineDataDict.TryAdd(kuaFuLineData.MapCode, list);
                                }

                                list.Add(kuaFuLineData);
                            }
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    success = false;
                    LogManager.WriteLog(LogTypes.Fatal, string.Format("Có lỗi khi load Config ", MapLinesPath), ex);
                }
            }

            return success;
        }

        #endregion Initial configuration

        #region CheckMap

        public bool IsKuaFuMap(int mapCode)
        {
            if (RuntimeData.MapCode2KuaFuLineDataDict.ContainsKey(mapCode))
            {
                return true;
            }

            return false;
        }

        public bool ProcessGetKuaFuLineDataListCmd(KPlayer client, int nID, byte[] bytes, string[] cmdParams)
        {
            try
            {
                int mapCode = Global.SafeConvertToInt32(cmdParams[0]);
                List<KuaFuLineData> list = YongZheZhanChangClient.getInstance().GetKuaFuLineDataList(mapCode) as List<KuaFuLineData>;

                //Console.WriteLine(list);

                client.SendPacket(nID, list);
                return true;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, Global.GetDebugHelperInfo(client.ClientSocket), false);
            }

            return false;
        }

        public bool ProcessKuaFuMapEnterCmd(KPlayer client, int nID, byte[] bytes, string[] cmdParams)
        {
            try
            {
                int result = StdErrorCode.Error_Success_No_Info;
                int toMapCode = Global.SafeConvertToInt32(cmdParams[0]);
                int line = Global.SafeConvertToInt32(cmdParams[1]);

                int teleportId = Global.SafeConvertToInt32(cmdParams[3]);
                int ToPosX = Global.SafeConvertToInt32(cmdParams[4]);
                int ToPosY = Global.SafeConvertToInt32(cmdParams[5]);

                do
                {
                    if (!KuaFuMapManager.getInstance().IsKuaFuMap(toMapCode))
                    {
                        result = StdErrorCode.Error_Operation_Denied;
                        break;
                    }

                    if (KTMapManager.Find(toMapCode) == null || toMapCode == client.MapCode)
                    {
                        result = StdErrorCode.Error_Operation_Denied;
                        break;
                    }

                    //if (!KuaFuMapManager.getInstance().IsKuaFuMap(client.MapCode))
                    //{
                    //    result = StdErrorCode.Error_Denied_In_Current_Map;
                    //    break;
                    //}

                    KuaFuLineData kuaFuLineData;
                    if (!RuntimeData.LineMap2KuaFuLineDataDict.TryGetValue(new IntPairKey(line, toMapCode), out kuaFuLineData))
                    {
                        result = StdErrorCode.Error_Operation_Denied;
                        break;
                    }

                    if (kuaFuLineData.OnlineCount >= kuaFuLineData.MaxOnlineCount)
                    {
                        result = StdErrorCode.Error_Server_Connections_Limit;
                        break;
                    }

                    int fromMapCode = client.MapCode;
                    if (teleportId > 0)
                    {

                        GameMap fromGameMap = KTMapManager.Find(fromMapCode);
                        if (fromGameMap == null)
                        {
                            result = StdErrorCode.Error_Config_Fault;
                            break;
                        }

                        MapTeleport mapTeleport = null;
                        if (!fromGameMap.MapTeleportDict.TryGetValue(teleportId, out mapTeleport) || mapTeleport.ToMapID != toMapCode)
                        {
                            result = StdErrorCode.Error_Operation_Denied;
                            break;
                        }


                        if (KTGlobal.GetDistanceBetweenPoints(client.CurrentPos, new Point(mapTeleport.X, mapTeleport.Y)) > 800)
                        {
                            result = StdErrorCode.Error_Too_Far;
                            break;
                        }
                    }

                    int kuaFuServerId = YongZheZhanChangClient.getInstance().EnterKuaFuMap(client.RoleID, kuaFuLineData.MapCode, kuaFuLineData.Line, client.ServerId, Global.GetClientKuaFuServerLoginData(client));
                    if (kuaFuServerId > 0)
                    {
                        // Abandon this judgment, the two cross-server mainline maps are configured on the same server, and the short-term reconnection is still unified

                        if (false && kuaFuServerId == GameManager.ServerId)
                        {
                            KTGlobal.GotoCrossServer(client, toMapCode);
                        }
                        else
                        {
                            // Cứ cho vào luôn đéo cần phải tiền bạc gì hết
                            int[] enterFlags = new int[(int)EKuaFuMapEnterFlag.Max];
                            enterFlags[(int)EKuaFuMapEnterFlag.FromMapCode] = fromMapCode;
                            enterFlags[(int)EKuaFuMapEnterFlag.FromTeleport] = teleportId;
                            enterFlags[(int)EKuaFuMapEnterFlag.TargetBossId] = -1;
                            enterFlags[(int)EKuaFuMapEnterFlag.ToMapX] = ToPosX;
                            enterFlags[(int)EKuaFuMapEnterFlag.ToMapY] = ToPosY;



                            Global.SaveRoleParamsIntListToDB(client, new List<int>(enterFlags), RoleParamName.EnterKuaFuMapFlag, true);

                            Global.RecordSwitchKuaFuServerLog(client);
                            client.SendPacket((int)TCPGameServerCmds.CMD_SPR_KF_SWITCH_SERVER, Global.GetClientKuaFuServerLoginData(client));
                        }
                    }
                    else
                    {
                        Global.GetClientKuaFuServerLoginData(client).RoleId = 0;
                        result = kuaFuServerId;
                    }
                } while (false);

                client.SendPacket(nID, result);
                return true;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, Global.GetDebugHelperInfo(client.ClientSocket), false);
            }

            return false;
        }

        #endregion 指令处理

        #region 其他

        public bool OnInitGame(KPlayer client)
        {
            KuaFuServerLoginData kuaFuServerLoginData = Global.GetClientKuaFuServerLoginData(client);

            client.MapCode = (int)kuaFuServerLoginData.GameId;
            client.PosX = 0;
            client.PosY = 0;

            List<int> enterFlags = Global.GetRoleParamsIntListFromDB(client, RoleParamName.EnterKuaFuMapFlag);
            if (enterFlags != null && enterFlags.Count >= (int)EKuaFuMapEnterFlag.Max)
            {
                int fromMapCode = enterFlags[(int)EKuaFuMapEnterFlag.FromMapCode];
                int fromTeleport = enterFlags[(int)EKuaFuMapEnterFlag.FromTeleport];
                int targetBossId = enterFlags[(int)EKuaFuMapEnterFlag.TargetBossId];

                int ToMapX = enterFlags[(int)EKuaFuMapEnterFlag.ToMapX];
                int ToMapY = enterFlags[(int)EKuaFuMapEnterFlag.ToMapY];

                if (fromMapCode > 0 && fromTeleport > 0)
                {
                    GameMap fromGameMap = KTMapManager.Find(fromMapCode);
                    if (fromGameMap != null && fromGameMap.MapTeleportDict.TryGetValue(fromTeleport, out MapTeleport mapTeleport))
                    {
                        GameMap toGameMap = KTMapManager.Find(mapTeleport.ToMapID);
                        if (toGameMap != null && toGameMap.CanMove(mapTeleport.ToX / toGameMap.MapGridWidth, mapTeleport.ToY / toGameMap.MapGridHeight, client.CurrentCopyMapID))
                        {
                            client.MapCode = mapTeleport.ToMapID;
                            client.PosX = mapTeleport.ToX;
                            client.PosY = mapTeleport.ToY;
                        }
                    }
                }
                else if (fromTeleport < 0)
                {
                    client.PosX = ToMapX;
                    client.PosY = ToMapY;
                }

            }

            return true;
        }

        public void OnStartPlayGame(KPlayer client)
        {
            bool bUserTeleport = false;
            List<int> enterFlags = Global.GetRoleParamsIntListFromDB(client, RoleParamName.EnterKuaFuMapFlag);
            if (enterFlags != null && enterFlags.Count >= (int)EKuaFuMapEnterFlag.Max && enterFlags[(int)EKuaFuMapEnterFlag.FromTeleport] > 0)
            {
                bUserTeleport = true;
            }

            if (!bUserTeleport)
            {
                KuaFuServerLoginData kuaFuServerLoginData = Global.GetClientKuaFuServerLoginData(client);

            }

            int[] clearFlags = new int[(int)EKuaFuMapEnterFlag.Max];
            clearFlags[(int)EKuaFuMapEnterFlag.FromMapCode] = enterFlags[(int)EKuaFuMapEnterFlag.FromMapCode];
            clearFlags[(int)EKuaFuMapEnterFlag.FromTeleport] = 0;
            clearFlags[(int)EKuaFuMapEnterFlag.TargetBossId] = 0;
            Global.SaveRoleParamsIntListToDB(client, new List<int>(clearFlags), RoleParamName.EnterKuaFuMapFlag, true);
        }

        public void TimerProc(object sender, EventArgs e)
        {
            Dictionary<int, int> dict = new Dictionary<int, int>();
            lock (RuntimeData.Mutex)
            {
                if (YongZheZhanChangClient.getInstance().CanKuaFuLogin())
                {
                    foreach (var mapCode in RuntimeData.MapCode2KuaFuLineDataDict.Keys)
                    {
                        dict[mapCode] = 0;
                    }
                }
            }

            List<int> list = dict.Keys.ToList();
            foreach (var mapCode in list)
            {
                dict[mapCode] = KTPlayerManager.GetPlayersCount(mapCode);
            }

            lock (RuntimeData.Mutex)
            {
                YongZheZhanChangClient.getInstance().UpdateKuaFuMapClientCount(dict);
            }
        }

        #endregion 其他
    }
}