using Server.Tools;
using System.Collections.Generic;

namespace GameDBServer.Logic
{
    public class UserOnlineManager
    {
        private static Dictionary<string, int> _RegUserIDDict = new Dictionary<string, int>();

        public static bool RegisterUserID(string userID, int serverLineID, int state)
        {
            bool ret = true;
            lock (_RegUserIDDict)
            {
                int oldServerLineID = -1;
                if (state <= 0)
                {
                    if (_RegUserIDDict.TryGetValue(userID, out oldServerLineID))
                    {
                        if (oldServerLineID == serverLineID)
                        {
                            _RegUserIDDict.Remove(userID);
                        }
                    }
                }
                else
                {
                    if (_RegUserIDDict.TryGetValue(userID, out oldServerLineID))
                    {
                        if (LineManager.GetLineHeartState(oldServerLineID) > 0)
                        {
                            LogManager.WriteLog(LogTypes.Error, string.Format("账号 {0} 请求注册登录到 {1} 线，但是该账号已经被注册到 {2} 线", userID, serverLineID, oldServerLineID));

                            ret = false;
                        }
                        else
                        {
                            _RegUserIDDict[userID] = serverLineID;
                        }
                    }
                    else
                    {
                        _RegUserIDDict[userID] = serverLineID;
                    }
                }
            }

            return ret;
        }

        public static void ClearUserIDsByServerLineID(int serverLineID)
        {
            lock (_RegUserIDDict)
            {
                List<string> userIDsList = new List<string>();
                foreach (var userID in _RegUserIDDict.Keys)
                {
                    int oldServerLineID = _RegUserIDDict[userID];
                    if (oldServerLineID == serverLineID)
                    {
                        userIDsList.Add(userID);
                    }
                }

                for (int i = 0; i < userIDsList.Count; i++)
                {
                    _RegUserIDDict.Remove(userIDsList[i]);
                }
            }
        }
    }
}