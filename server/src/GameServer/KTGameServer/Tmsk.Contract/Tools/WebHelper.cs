using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using Server.Tools;

namespace Tmsk.Tools
{
    public static class WebHelper
    {
        #region 执行Web请求获取数据

        public static byte[] RequestByPost(string uri, byte[] bytes, int timeOut = 5000, int readWriteTimeout = 100000)
        {
            int httpStatusCode;
            return RequestByPost(uri, bytes, out httpStatusCode, timeOut, readWriteTimeout);
        }

        public static byte[] RequestByPost(string uri, byte[] bytes, out int httpStatusCode, int timeOut = 10000, int readWriteTimeout = 100000)
        {
            httpStatusCode = (int)HttpStatusCode.NotFound;

            byte[] bytes2 = null;
            try
            {
                HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(uri);
                myRequest.Method = "POST";
                myRequest.ContentType = "application/x-www-form-urlencoded";
                myRequest.ContentLength = bytes.Length;
                myRequest.KeepAlive = false;
                myRequest.Timeout = timeOut;
                myRequest.ReadWriteTimeout = readWriteTimeout;

                HttpWebResponse myResponse = null;
                using (Stream newStream = myRequest.GetRequestStream())
                {
                    // Send the data.
                    newStream.Write(bytes, 0, bytes.Length);
                    newStream.Close();

                    myResponse = (HttpWebResponse)myRequest.GetResponse();

                    if (null == myResponse)
                        return null;

                    httpStatusCode = (int)myResponse.StatusCode;
                    if (httpStatusCode == (int)HttpStatusCode.OK)
                    {
                        bytes2 = GetBytes(myResponse);
                    }

                    newStream.Close();
                    myResponse.Close();
                }
            }
            catch (Exception ex)
            {
                LogManager.WriteExceptionUseCache(ex.ToString());
                bytes2 = null;
            }
            finally
            {
                //myRequest.Abort();
            }

            return bytes2;
        }

        public static byte[] RequestByGet(string uri, int timeOut = 5000, int readWriteTimeout = 100000)
        {
            int httpStatusCode;
            return RequestByGet(uri, out httpStatusCode, timeOut, readWriteTimeout);
        }

        public static byte[] RequestByGet(string uri, out int httpStatusCode, int timeOut = 5000, int readWriteTimeout = 100000)
        {
            httpStatusCode = (int)HttpStatusCode.NotFound;

            byte[] bytes2 = null;
            try
            {
                HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(uri);
                myRequest.Method = "GET";
                myRequest.ContentType = "application/x-www-form-urlencoded";
                myRequest.ContentLength = 0;
                myRequest.KeepAlive = false;
                myRequest.Timeout = timeOut;
                myRequest.ReadWriteTimeout = readWriteTimeout;

                HttpWebResponse myResponse = (HttpWebResponse)myRequest.GetResponse();

                if (null == myResponse)
                    return null;

                httpStatusCode = (int)myResponse.StatusCode;
                if (httpStatusCode == (int)HttpStatusCode.OK)
                {
                    bytes2 = GetBytes(myResponse);
                }

                myResponse.Close();
            }
            catch (Exception ex)
            {
                LogManager.WriteExceptionUseCache(ex.ToString());
                bytes2 = null;
            }
            finally
            {
                //myRequest.Abort();
            }

            return bytes2;
        }

        #endregion 执行Web请求获取数据

        #region 辅助函数

        /// <summary>
        /// 从返回流中取出数据到字节数组
        /// </summary>
        /// <param name="res"></param>
        /// <returns></returns>
        private static byte[] GetBytes(HttpWebResponse res)
        {
            byte[] buffer = null;

            try
            {
                Stream rs = res.GetResponseStream();
                try
                {
#if Modify_2015_02_13
                    int length = (int)res.ContentLength;
                    buffer = new byte[length];
                    rs.Read(buffer, 0, length);
#else
                    MemoryStream memoryStream = new MemoryStream();
                    buffer = new byte[0x100];
                    for (int i = rs.Read(buffer, 0, buffer.Length); i > 0; i = rs.Read(buffer, 0, buffer.Length))
                    {
                        memoryStream.Write(buffer, 0, i);
                    }

                    buffer = memoryStream.ToArray();
#endif
                }
                finally
                {
                    rs.Close();
                }
            }
            catch (Exception ex)
            {
                LogManager.WriteExceptionUseCache(ex.ToString());
            }

            return buffer;
        }

        #endregion 辅助函数
    }
}
