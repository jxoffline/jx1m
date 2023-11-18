using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Server.Tools.Pattern
{
    public class SingletonTemplate<T> where T : class
    {
        private static T __singletionInstance = null;
        private static readonly Object __singletionInstanceLock = new Object();

        public static T Instance()
        {
            if (__singletionInstance == null)
            {
                lock (__singletionInstanceLock)
                {
                    if (__singletionInstance == null)
                    {
                        __singletionInstance = (T)Activator.CreateInstance(typeof(T), true);
                    }
                }
            }

            return __singletionInstance;
        }
    }
}
