using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Tmsk.Contract
{
    public delegate bool DelGmCmdHandler(object client, string msgText, string[] cmdFields, bool transmit, bool isSuperGMUser);
    public class GmCmdHandler
    {
        private object mutex = new object();
        public readonly string gmCmd;
        private DelGmCmdHandler handler;
        private int nRef;
        private bool shutdown;

        public GmCmdHandler(string _gmCmd, DelGmCmdHandler _handler)
        {
            gmCmd = _gmCmd;
            handler = _handler;
        }

        public bool Process(object client, string msgText, string[] cmdFields, bool transmit, bool isSuperGMUser)
        {
            bool ret = false;
            lock (mutex)
            {
                if (shutdown) return false;
                try
                {
                    nRef++;
                    ret = handler(client, msgText, cmdFields, transmit, isSuperGMUser);
                }
                catch (System.Exception ex)
                {
                    ret = false;
                }
                finally
                {
                    nRef--;
                }
            }

            return ret;
        }

        public bool IsShutdownComplete()
        {
            bool complete = false;
            if (Monitor.TryEnter(mutex))
            {
                if (shutdown && nRef == 0)
                {
                    complete = true;
                }

                Monitor.Exit(mutex);
            }

            return complete;
        }

        public void Shutdowm()
        {
            lock (mutex)
            {
                shutdown = true;
            }
        }
    }
}
