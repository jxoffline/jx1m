using GameDBServer.Server;
using Server.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace GameDBServer.Logic
{
    public class LineManager
    {
        private static object Mutex = new object();


        private static Dictionary<int, LineItem> _LinesDict = new Dictionary<int, LineItem>();


        private static List<LineItem> _LinesList = new List<LineItem>();


        public static void LoadConfig()
        {
            bool success = false;
            Dictionary<int, LineItem> linesDict = new Dictionary<int, LineItem>();

            try
            {
                XElement xml = XElement.Load(@"GameServer.xml");
                IEnumerable<XElement> mapItems = xml.Element("GameServer").Elements();
                foreach (var mapItem in mapItems)
                {
                    LineItem lineItem = new LineItem()
                    {
                        LineID = (int)Global.GetSafeAttributeLong(mapItem, "ID"),
                        GameServerIP = Global.GetSafeAttributeStr(mapItem, "ip"),
                        GameServerPort = (int)Global.GetSafeAttributeLong(mapItem, "port"),
                        OnlineCount = 0,
                    };

                    linesDict[lineItem.LineID] = lineItem;
                    success = true;
                }
            }
            catch (System.Exception ex)
            {
                LogManager.WriteException(ex.ToString());
                success = false;
            }

            if (success)
            {
                lock (Mutex)
                {
                    foreach (var item in linesDict.Values)
                    {
                        if (!_LinesDict.ContainsKey(item.LineID))
                        {
                            _LinesDict.Add(item.LineID, item);
                        }
                    }

                    _LinesList = _LinesDict.Values.ToList();
                }
            }
        }


        public static void UpdateLineHeart(GameServerClient client, int lineID, int onlineNum, String strMapOnlineNum = "")
        {
            lock (Mutex)
            {
                LineItem lineItem = null;
                if (_LinesDict.TryGetValue(lineID, out lineItem))
                {
                    lineItem.OnlineCount = onlineNum;
                    lineItem.OnlineTicks = DateTime.Now.Ticks / 10000;
                    lineItem.MapOnlineNum = strMapOnlineNum;
                    lineItem.ServerClient = client;
                    client.LineId = lineID;
                }
                else if (lineID >= GameDBManager.KuaFuServerIdStartValue)
                {
                    lineItem = new LineItem();
                    _LinesDict[lineID] = lineItem;
                    lineItem.LineID = lineID;

                    lineItem.OnlineCount = onlineNum;
                    lineItem.OnlineTicks = DateTime.Now.Ticks / 10000;
                    lineItem.MapOnlineNum = strMapOnlineNum;
                    lineItem.ServerClient = client;
                    client.LineId = lineID;

                    if (!_LinesList.Contains(lineItem))
                    {
                        _LinesList.Add(lineItem);
                    }
                }
            }
        }

        public static GameServerClient GetGameServerClient(int lineId)
        {
            lock (Mutex)
            {
                LineItem lineItem = null;
                if (_LinesDict.TryGetValue(lineId, out lineItem))
                {
                    return lineItem.ServerClient;
                }
            }

            return null;
        }

        public static int GetLineHeartState(int lineID)
        {
            long ticks = DateTime.Now.Ticks / 10000;
            int state = 0;
            lock (Mutex)
            {
                LineItem lineItem = null;
                if (_LinesDict.TryGetValue(lineID, out lineItem))
                {
                    if (ticks - lineItem.OnlineTicks < (60 * 1000)) //服务器是否在线
                    {
                        state = 1;
                    }
                }
            }

            return state;
        }


        public static List<LineItem> GetLineItemList()
        {
            List<LineItem> lineItemList = new List<LineItem>();

            long ticks = DateTime.Now.Ticks / 10000;
            lock (Mutex)
            {
                for (int i = 0; i < _LinesList.Count; i++)
                {
                    if (ticks - _LinesList[i].OnlineTicks < (3 * 60 * 1000)) //服务器是否在线,这里用3分钟
                    {
                        lineItemList.Add(_LinesList[i]);
                    }
                }
            }

            return lineItemList;
        }

        public static int GetTotalOnlineNum()
        {
            int totalNum = 0;
            long ticks = DateTime.Now.Ticks / 10000;
            lock (Mutex)
            {
                for (int i = 0; i < _LinesList.Count; i++)
                {
                    if (ticks - _LinesList[i].OnlineTicks < (60 * 1000)) //服务器是否在线
                    {
                        totalNum += _LinesList[i].OnlineCount;
                    }
                }
            }

            return totalNum;
        }


        public static String GetMapOnlineNum()
        {
            String strMapOnlineInfo = "";
            long ticks = DateTime.Now.Ticks / 10000;
            lock (Mutex)
            {
                for (int i = 0; i < _LinesList.Count; i++)
                {
                    if (ticks - _LinesList[i].OnlineTicks < (60 * 1000)) //服务器是否在线
                    {
                        return _LinesList[i].MapOnlineNum;
                    }
                }
            }

            return strMapOnlineInfo;
        }
    }
}