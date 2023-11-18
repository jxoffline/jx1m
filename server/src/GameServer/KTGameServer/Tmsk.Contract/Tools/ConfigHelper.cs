using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Server.Tools;

namespace Tmsk.Tools.Tools
{
    public static class ConfigHelper
    {
        public static XElement Load(string fileName)
        {
            XElement xml = null;
            if (File.Exists(fileName))
            {
                xml = XElement.Load(fileName);
            }

            return xml;
        }

        public static IEnumerable<XElement> GetXElements(XElement xml, string name)
        {
            try
            {
                if (xml == null)
                {
                    return null;
                }
                return xml.DescendantsAndSelf(name);
            }
            catch (System.Exception ex)
            {
                LogManager.WriteExceptionUseCache(ex.ToString());
            }

            return null;
        }

        public static XElement GetXElement(XElement xml, string name)
        {
            try
            {
                if (xml == null)
                {
                    return null;
                }
                return xml.DescendantsAndSelf(name).SingleOrDefault();
            }
            catch (System.Exception ex)
            {
                LogManager.WriteExceptionUseCache(ex.ToString());
            }

            return null;
        }

        public static XElement GetXElement(XElement xml, string name, string attrName, string attrValue)
        {
            try
            {
                if (xml == null)
                {
                    return null;
                }
                return xml.DescendantsAndSelf(name).SingleOrDefault(X => X.Attribute(attrName).Value == attrValue);
            }
            catch (System.Exception ex)
            {
                LogManager.WriteExceptionUseCache(ex.ToString());
            }

            return null;
        }

        public static string GetElementAttributeValue(XElement xml, string name, string attrName, string attrValue, string attribute, string defVal = "")
        {
            string val = defVal;
            try
            {
                if (xml == null)
                {
                    return val;
                }
                XElement node = xml.DescendantsAndSelf(name).SingleOrDefault(X => X.Attribute(attrName).Value == attrValue);
                if (null != node)
                {
                    XAttribute attrib = node.Attribute(attribute);
                    if (null != attrib)
                    {
                        val = attrib.Value;
                    }
                }
            }
            catch (System.Exception ex)
            {
                LogManager.WriteExceptionUseCache(ex.ToString());
            }

            return val;
        }

        public static long GetElementAttributeValueLong(XElement xml, string name, string attrName, string attrValue, string attribute, long defVal = 0)
        {
            long val = defVal;
            try
            {
                if (xml == null)
                {
                    return defVal;
                }

                XElement node = xml.DescendantsAndSelf(name).SingleOrDefault(X => X.Attribute(attrName).Value == attrValue);
                if (null != node)
                {
                    XAttribute attrib = node.Attribute(attribute);
                    if (null != attrib)
                    {
                        if (!long.TryParse(attrib.Value, out val))
                        {
                            val = defVal;
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                LogManager.WriteExceptionUseCache(ex.ToString());
            }

            return val;
        }

        public static int[] GetElementAttributeValueIntArray(XElement xml, string name, string attrName, string attrValue, string attribute, int[] defArr = null)
        {
            int[] arr = defArr;
            try
            {
                if (xml == null)
                {
                    return defArr;
                }

                XElement node = xml.DescendantsAndSelf(name).Single(X => X.Attribute(attrName).Value == attrValue);
                if (null != node)
                {
                    XAttribute attrib = node.Attribute(attribute);
                    if (null == attrib) return defArr;

                    arr = String2IntArray(attrib.Value);
                    if (arr == null) return defArr;
                }
            }
            catch (System.Exception ex)
            {
                LogManager.WriteExceptionUseCache(ex.ToString());
            }

            return arr;
        }

        public static string[] GetElementAttributeValueStrArray(XElement xml, string name, string attrName, string attrValue, string attribute, string[] defArr = null)
        {
            string[] arr = defArr;
            try
            {
                if (xml == null)
                {
                    return defArr;
                }

                XElement node = xml.DescendantsAndSelf(name).Single(X => X.Attribute(attrName).Value == attrValue);
                if (null != node)
                {
                    XAttribute attrib = node.Attribute(attribute);
                    if (null == attrib) return defArr;

                    arr = attrib.Value.Split(',');
                }
            }
            catch (System.Exception ex)
            {
                LogManager.WriteExceptionUseCache(ex.ToString());
            }

            return arr;
        }

        public static string GetElementAttributeValue(XElement xml, string attribute, string defVal = "")
        {
            string val = defVal;
            try
            {
                if (xml == null)
                {
                    return defVal;
                }
                XAttribute attrib = xml.Attribute(attribute);
                if (null != attrib)
                {
                    val = attrib.Value;
                }
            }
            catch (Exception ex)
            {
                LogManager.WriteExceptionUseCache(ex.ToString());
            }

            return val;
        }

        public static long GetElementAttributeValueLong(XElement xml, string attribute, long defVal = 0)
        {
            long val = defVal;
            try
            {
                if (xml == null)
                {
                    return defVal;
                }
                XAttribute attrib = xml.Attribute(attribute);
                if (null != attrib)
                {
                    if (!long.TryParse(attrib.Value, out val))
                    {
                        val = defVal;
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.WriteExceptionUseCache(ex.ToString());
            }

            return val;
        }

        public static double GetElementAttributeValueDouble(XElement xml, string attribute, double defVal = 0)
        {
            double val = defVal;
            try
            {
                if (xml == null)
                {
                    return defVal;
                }
                XAttribute attrib = xml.Attribute(attribute);
                if (null != attrib)
                {
                    if (!double.TryParse(attrib.Value, out val))
                    {
                        val = defVal;
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.WriteExceptionUseCache(ex.ToString());
            }

            return val;
        }

        #region

        //将字符串转换为Int类型数组
        public static int[] String2IntArray(string str, char spliter = ',')
        {
            if (string.IsNullOrEmpty(str)) return null;

            string[] sa = str.Split(spliter);
            return StringArray2IntArray(sa, 0, sa.Length);
        }

        private static int[] StringArray2IntArray(string[] sa, int start, int count)
        {
            if (sa == null) return null;
            if (start < 0 || start >= sa.Length) return null;
            if (count <= 0) return null;
            if (sa.Length - start < count) return null;

            int[] result = new int[count];
            for (int i = 0; i < count; ++i)
            {
                string str = sa[start + i].Trim();
                str = string.IsNullOrEmpty(str) ? "0" : str;
                result[i] = Convert.ToInt32(str);
            }

            return result;
        }

        #endregion

    }
}
