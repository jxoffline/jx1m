using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace GameServer.KiemThe
{
    /// <summary>
    /// Các phương thức và đối tượng toàn cục của Kiếm Thế
    /// </summary>
    public static partial class KTGlobal
    {
        #region Random number

        /// <summary>
        /// Đối tượng Random
        /// </summary>
        private static readonly Random random = new Random();

        public static Random GetRandom()
        {
            return random;
        }
        /// <summary>
        /// Trả về số nguyên trong khoảng tương ứng
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static int GetRandomNumber(int min, int max)
        {
            if (max < min)
            {
                max = min;
            }
            lock (KTGlobal.random)
            {
                int nRand = KTGlobal.random.Next(min, max + 1);
                if (nRand > max)
                {
                    nRand = max;
                }
                return nRand;
            }
        }

        /// <summary>
        /// Trả về số thực trong khoảng tương ứng
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static float GetRandomNumber(float min, float max)
        {
            lock (KTGlobal.random)
            {
                return (float)KTGlobal.random.NextDouble() * (max - min) + min;
            }
        }

        /// <summary>
        /// Get random long value
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <param name="rand"></param>
        /// <returns></returns>
        public static long GetRandomNumber(long min, long max)
        {
            lock (KTGlobal.random)
            {
                byte[] buf = new byte[8];
                KTGlobal.random.NextBytes(buf);
                long longRand = BitConverter.ToInt64(buf, 0);

                return (Math.Abs(longRand % (max - min)) + min);
            }
        }

        /// <summary>
        /// Trả về số thực trong khoảng tương ứng
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static double GetRandomNumber(double min, double max)
        {
            lock (KTGlobal.random)
            {
                return KTGlobal.random.NextDouble() * (max - min) + min;
            }
        }

        /// <summary>
        /// Trả về số nguyên ngẫu nhiên trong khoảng từ 0 đến giá trị tương ứng
        /// </summary>
        /// <param name="nMax"></param>
        /// <returns></returns>
        public static int GetRandomNumber(int nMax)
        {
            return KTGlobal.GetRandomNumber(0, nMax);
        }

        /// <summary>
        /// Trả về số thực ngẫu nhiên trong khoảng từ 0 đến giá trị tương ứng
        /// </summary>
        /// <param name="nMax"></param>
        /// <returns></returns>
        public static float GetRandomNumber(float nMax)
        {
            return KTGlobal.GetRandomNumber(0, nMax);
        }

        /// <summary>
        /// Trả về số thực ngẫu nhiên trong khoảng từ 0 đến giá trị tương ứng
        /// </summary>
        /// <param name="nMax"></param>
        /// <returns></returns>
        public static double GetRandomNumber(double nMax)
        {
            return KTGlobal.GetRandomNumber(0, nMax);
        }

        #endregion Random number

        #region Random string

        /// <summary>
        /// Trả về chuỗi ngẫu nhiên có độ dài tương ứng
        /// </summary>
        /// <param name="leng"></param>
        /// <returns></returns>
        public static string MakeRandomString(int leng)
        {
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new System.Random();
            var result = new string(
                Enumerable.Repeat(chars, leng)
                          .Select(s => s[random.Next(s.Length)])
                          .ToArray());
            string gifcode = result;

            return gifcode;
        }

        #endregion Random string

        #region RandomList
        /// <summary>
        /// Trộn 1 list lại với nhau
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="rnd"></param>
        public static void Shuffle<T>(IList<T> list, Random rnd)
        {
            for (var i = list.Count; i > 0; i--)
                KTGlobal.Swap(list,0, rnd.Next(0, i));
        }
        /// <summary>
        /// Swap 2 vị trí trong list cho nhau
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="i"></param>
        /// <param name="j"></param>
        public static void Swap<T>(IList<T> list, int i, int j)
        {
            var temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }
        #endregion
    }
}