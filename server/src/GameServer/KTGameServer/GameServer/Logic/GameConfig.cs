using System;
using System.Collections.Generic;

namespace GameServer.Logic
{
   
    /// <summary>
    /// Cái này chứa toàn bộ config của trò chơi tý dọn
    /// </summary>
    public class GameConfig
    {
        #region 基础数据

        /// <summary>
        /// 公告字典
        /// </summary>
        private Dictionary<string, string> _GameConfigDict = new Dictionary<string, string>();

        #endregion 基础数据

        #region 基础方法

        /// <summary>
        /// 从数据库中获取配置参数
        /// </summary>
        public void LoadGameConfigFromDBServer()
        {
            //查询游戏配置参数
            //从DBserver加载配置参数
            _GameConfigDict = Global.LoadDBGameConfigDict();
            if (null == _GameConfigDict)
            {
                _GameConfigDict = new Dictionary<string, string>();
            }
        }

        /// <summary>
        /// 设置游戏配置项
        /// </summary>
        public void SetGameConfigItem(string paramName, string paramValue)
        {
            lock (_GameConfigDict)
            {
                _GameConfigDict[paramName] = paramValue;
            }

            //当参数发生变化时通知
            ChangeParams(paramName, paramValue);
        }

        /// <summary>
        /// 更新服务器参数，如果当前值和目标值相同，则跳过写数据库，除非指定force参数
        /// </summary>
        /// <param name="paramName"></param>
        /// <param name="paramValue"></param>
        /// <param name="force"></param>
        public void UpdateGameConfigItem(string paramName, string paramValue, bool force = false)
        {
            lock (_GameConfigDict)
            {
                string oldValue;
                if (_GameConfigDict.TryGetValue(paramName, out oldValue))
                {
                    if (oldValue == paramValue && !force)
                    {
                        return;
                    }
                }
            }

            SetGameConfigItem(paramName, paramValue);

            //当参数发生变化时通知
            Global.UpdateDBGameConfigg(paramName, paramValue);
        }

        /// <summary>
        /// 在原有数值上修改游戏配置项
        /// </summary>
        public void ModifyGameConfigItem(string paramName, int paramValue)
        {
            int value = 0;
            lock (_GameConfigDict)
            {
                value = GetGameConfigItemInt(paramName, 0) + paramValue;
                _GameConfigDict[paramName] = value.ToString();
            }

            //当参数发生变化时通知
            ChangeParams(paramName, value.ToString());
        }

     
        public string GetGameConifgItem(string paramName)
        {
            string paramValue = null;
            lock (_GameConfigDict)
            {
                if (!_GameConfigDict.TryGetValue(paramName, out paramValue))
                {
                    paramValue = null;
                }
            }

            return paramValue;
        }

 
        public string GetGameConfigItemStr(string paramName, string defVal)
        {
            string ret = GetGameConifgItem(paramName);
            if (string.IsNullOrEmpty(ret))
            {
                return defVal;
            }

            return ret;
        }

      
        public int GetGameConfigItemInt(string paramName, int defVal)
        {
            string str = GetGameConifgItem(paramName);
            if (string.IsNullOrEmpty(str))
            {
                return defVal;
            }

            int ret = 0;

            try
            {
                ret = Convert.ToInt32(str);
            }
            catch (Exception)
            {
                ret = defVal;
            }

            return ret;
        }

       

      
      
        private void ChangeParams(string paramName, string paramValue)
        {
           
        }

        #endregion 
    }
}