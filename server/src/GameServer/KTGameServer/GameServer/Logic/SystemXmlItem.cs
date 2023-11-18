using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
//using System.Windows.Controls;
//using System.Windows.Threading;
using System.Xml.Linq;
using GameServer.Interface;
using Server.Data;
using Server.Tools;

namespace GameServer.Logic
{
    /// Xml列表项
    public class SystemXmlItem
    {
        /// <summary>
        /// 物品对应的Xml节点
        /// </summary>
        public XElement XMLNode
        {
            get;
            set;
        }

        /// <summary>
        /// 获取字符串值
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public string GetStringValue(string name)
        {
            string ret = "";
            try
            {
                ret = (string)XMLNode.Attribute(name);
            }
            catch (Exception)
            {
                string path = Global.GetXElementNodePath(XMLNode);
                LogManager.WriteLog(LogTypes.Warning, string.Format("解析XMLNode 中的属性值: {0}, 失败, XML节点路径: {1}", name, path));
            }

            return ret;
        }

        /// <summary>
        /// 获取整数值
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public int GetIntValue(string name, int defaultValue = -1)
        {
            int ret = defaultValue;
            try
            {
                string str = (string)XMLNode.Attribute(name);
                if (null != str && str != "")
                {
                    ret = (int)Convert.ToDouble(str);
                }
            }
            catch (Exception)
            {
                string path = Global.GetXElementNodePath(XMLNode);
                LogManager.WriteLog(LogTypes.Warning, string.Format("解析XMLNode 中的属性值: {0}, 失败, XML节点路径: {1}", name, path));
            }

            return ret;
        }

        /// <summary>
        /// 获取长整数值
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public long GetLongValue(string name)
        {
            long ret = -1;
            try
            {
                string str = (string)XMLNode.Attribute(name);
                if (null != str && str != "")
                {
                    ret = (long)Convert.ToInt64(str);
                }
            }
            catch (Exception)
            {
                string path = Global.GetXElementNodePath(XMLNode);
                LogManager.WriteLog(LogTypes.Warning, string.Format("解析XMLNode 中的属性值: {0}, 失败, XML节点路径: {1}", name, path));
            }

            return ret;
        }

        /// <summary>
        /// 获取浮点数值
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public double GetDoubleValue(string name)
        {
            double ret = 0.0;
            try
            {
                string str = (string)XMLNode.Attribute(name);
                if (null != str && str != "")
                {
                    ret = Convert.ToDouble(str);
                }
            }
            catch (Exception)
            {
                string path = Global.GetXElementNodePath(XMLNode);
                LogManager.WriteLog(LogTypes.Warning, string.Format("解析XMLNode 中的属性值: {0}, 失败, XML节点路径: {1}", name, path));
            }

            return ret;
        }

        /// <summary>
        /// 获取整数列表值
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public int[] GetIntArrayValue(string name, char split=',')
        {
            int[] ret = null;
            try
            {
                string str = (string)XMLNode.Attribute(name);
                if (null != str && str != "")
                {
                    String[] strArr = str.Split(split);

                    if (strArr.Length > 0)
                    {
                        ret = new int[strArr.Length];

                        for (int n = 0; n < strArr.Length; n++)
                        {
                            ret[n] = Convert.ToInt32(strArr[n]);
                        }
                    }
                }
            }
            catch (Exception)
            {
                string path = Global.GetXElementNodePath(XMLNode);
                LogManager.WriteLog(LogTypes.Warning, string.Format("解析XMLNode 中的属性值: {0}, 失败, XML节点路径: {1},采用整形数组返回", name, path));
            }

            return ret;
        }

        /// <summary>
        /// 获取double列表值
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public double[] GetDoubleArrayValue(string name, char split = ',')
        {
            double[] ret = null;
            try
            {
                string str = (string)XMLNode.Attribute(name);
                if (null != str && str != "")
                {
                    String[] strArr = str.Split(split);

                    if (strArr.Length > 0)
                    {
                        ret = new double[strArr.Length];

                        for (int n = 0; n < strArr.Length; n++)
                        {
                            ret[n] = Convert.ToDouble(strArr[n]);
                        }
                    }
                }
            }
            catch (Exception)
            {
                string path = Global.GetXElementNodePath(XMLNode);
                LogManager.WriteLog(LogTypes.Warning, string.Format("解析XMLNode 中的属性值: {0}, 失败, XML节点路径: {1},采用整形数组返回", name, path));
            }

            return ret;
        }
    }
}
