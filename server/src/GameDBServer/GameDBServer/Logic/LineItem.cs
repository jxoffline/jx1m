using GameDBServer.Server;
using System;

namespace GameDBServer.Logic
{

    public class LineItem
    {
        private int _LineID = 0;


        public int LineID
        {
            get { lock (this) { return _LineID; } }
            set { lock (this) { _LineID = value; } }
        }

        private string _GameServerIP = "";


        public string GameServerIP
        {
            get { lock (this) { return _GameServerIP; } }
            set { lock (this) { _GameServerIP = value; } }
        }

        private int _GameServerPort = 0;


        public int GameServerPort
        {
            get { lock (this) { return _GameServerPort; } }
            set { lock (this) { _GameServerPort = value; } }
        }

        private int _OnlineCount = 0;

        public int OnlineCount
        {
            get { lock (this) { return _OnlineCount; } }
            set { lock (this) { _OnlineCount = value; } }
        }

        private String _MapOnlineNum = "";

        public String MapOnlineNum
        {
            get { lock (this) { return _MapOnlineNum; } }
            set { lock (this) { _MapOnlineNum = value; } }
        }

        private long _OnlineTicks = 0;


        public long OnlineTicks
        {
            get { lock (this) { return _OnlineTicks; } }
            set { lock (this) { _OnlineTicks = value; } }
        }


        public GameServerClient ServerClient;
    }
}