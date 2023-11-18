using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Server.Tools
{
    public class StringUtil
    {
        #region 模拟AS3函数

        //模拟AS3中的StringUtil.substitute 函数
        public static string substitute(string format, params object[] args)
        {
            string ret = "";
            try
            {
                ret = string.Format(format, args);
            }
            catch (Exception)
            {
                ret = format;
            }

            return ret;
        }

        ///忽略大小写比较两个字符串是否相等
        public static Boolean IsEqualIgnoreCase(String a, String b)
		{
            return a.ToLower() == b.ToLower();
		}

        #endregion 模拟AS3函数

        #region ---------角色id转换

        public static string IDToCode(int roleID)
        {
            char[] arr = roleID.ToString().ToArray();
            for (int i = 0; i < arr.Length; i += 2)
                arr[i] += 'A';

            StringBuilder sb = new StringBuilder();
            for (int ii = 0; ii < arr.Length; ii++)
                sb.Append(arr[ii].ToString());

            return sb.ToString().ToUpper();
        }

        public static int CodeToID(string code)
        {
            char[] arr = code.ToLower().ToArray();
            for (int i = 0; i < arr.Length; i += 2)
                arr[i] -= 'A';

            StringBuilder sb = new StringBuilder();
            for (int ii = 0; ii < arr.Length; ii++)
                sb.Append(arr[ii].ToString());

            int id = 0;
            int.TryParse(sb.ToString(), out id);
            return id;
        }


        public static string SpreadIDToCode(int roleID)
        {
            char[] arr = roleID.ToString().ToArray();
            for (int i = 0; i < arr.Length; i++)
            {
                arr[i] += 'A';

                if (i >= 4) break;
            }

            StringBuilder sb = new StringBuilder();
            for (int ii = 0; ii < arr.Length; ii++)
                sb.Append(arr[ii].ToString());

            return sb.ToString().ToUpper();
        }

        public static int SpreadCodeToID(string code)
        {
            char[] arr = code.ToLower().ToArray();
            for (int i = 0; i < arr.Length; i++)
            {
                arr[i] -= 'A';

                if (i >= 4) break;
            }

            StringBuilder sb = new StringBuilder();
            for (int ii = 0; ii < arr.Length; ii++)
                sb.Append(arr[ii].ToString());

            int id = 0;
            int.TryParse(sb.ToString(), out id);
            return id;
        }

        #endregion
    }
}
