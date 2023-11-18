using System.Collections.Generic;

namespace GameDBServer.Logic
{

    /// <summary>
    /// Logs level
    /// </summary>
    public enum EventLevels
    {
        Ignore = -1,
        Debug = 0,
        Hint = 1,
        Record = 2,
        Important = 3,
    }

    /// <summary>
    /// Các sự kiện event
    /// </summary>
    public class ServerEvents
    {
        public ServerEvents()
        {
        }

        /// <summary>
        /// Queue để phát triển
        /// </summary>
        private Queue<string> EventsQueue = new Queue<string>();

        /// <summary>
        /// Event level
        /// </summary>
        public EventLevels EventLevel
        {
            get;
            set;
        }


        private string _EventRootPath = "events";


        /// <summary>
        /// Event root
        /// </summary>
        public string EventRootPath
        {
            get { return _EventRootPath; }
            set { _EventRootPath = value; }
        }


        private string _EventPreFileName = "Event";


        /// <summary>
        /// Tên ghi event files
        /// </summary>
        public string EventPreFileName
        {
            get { return _EventPreFileName; }
            set { _EventPreFileName = value; }
        }

        /// <summary>
        /// Add event vào
        /// </summary>
        /// <param name="msg"></param>
        public void AddEvent(string msg, EventLevels eventLevel)
        {
            if ((int)eventLevel < (int)EventLevel) //不必记录
            {
                return;
            }

            lock (EventsQueue)
            {
                EventsQueue.Enqueue(msg);
            }
        }

    }
}