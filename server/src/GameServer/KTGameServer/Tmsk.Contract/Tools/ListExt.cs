using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.Collections.Generic
{
    public static class ListExt
    {
        /// <summary>
        /// 是否列表为null或空
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        public static bool IsNullOrEmpty<T>(this IList<T> list)
        {
            if (null == list || list.Count == 0)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 向有序的IList列表中，插入一个元素并结果保持有序(列表是从大到小排序的)
        /// 基于二分的思想,算法复杂度Log(n)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="data"></param>
        /// <param name="comparer">需要一个升序比较器</param>
        /// <returns>返回插入处的索引,从0开始</returns>
        public static int BinaryInsertDesc<T>(this IList<T> list, T data, IComparer<T> comparer)
        {
            if (null == list)
            {
                return -1;
            }
            else if (list.Count == 0)
            {
                list.Add(data);
                return 0;
            }

            int left = 0;
            int right = list.Count - 1;
            int mid;
            int sub;
            do 
            {
                if (left == right)
                {
                    sub = comparer.Compare(data, list[left]);
                    if (sub <= 0)
                    {
                        left++;
                    }
                    list.Insert(left, data);
                    return left;
                }
                mid = (left + right) / 2;
                sub = comparer.Compare(data, list[mid]);
                if (sub >= 0)
                {
                    right = mid;
                }
                else
                {
                    left = mid + 1;
                }
            } while (true);
        }

        /// <summary>
        /// 向有序的IList列表中，插入一个元素并结果保持有序(列表是从大到小排序的)
        /// 基于二分的思想,算法复杂度Log(n)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="data"></param>
        /// <param name="comparer">需要一个升序比较器</param>
        /// <returns>返回插入处的索引,从0开始</returns>
        public static int BinaryInsertAsc<T>(this IList<T> list, T data, IComparer<T> comparer)
        {
            if (null == list)
            {
                return -1;
            }
            else if (list.Count == 0)
            {
                list.Add(data);
                return 0;
            }

            int left = 0;
            int right = list.Count - 1;
            int mid;
            int sub;
            do 
            {
                if (left == right)
                {
                    sub = comparer.Compare(data, list[left]);
                    if (sub >= 0)
                    {
                        left++;
                    }
                    list.Insert(left, data);
                    return left;
                }
                mid = (left + right) / 2;
                sub = comparer.Compare(data, list[mid]);
                if (sub >= 0)
                {
                    left = mid + 1;
                }
                else
                {
                    right = mid;
                }
            } while (true);
        }

        /// <summary>
        /// 合并两个有序列表,他们都是从大到小排序的
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="list2"></param>
        /// <param name="comparer"></param>
        /// <returns></returns>
        public static int BinaryCombineDesc<T>(this IList<T> list, IList<T> list2, IComparer<T> comparer)
        {
            if (null == list)
            {
                return -1;
            }
            else if (list.Count == 0)
            {
                if (null != list2 && list2.Count > 0)
                {
                    foreach (var data in list2)
                    {
                        list.Add(data);
                    }
                }
                return list.Count;
            }

            int sub;
            int i = 0, j = 0;
            for (; i < list.Count && j < list2.Count; i++)
            {
                sub = comparer.Compare(list[i], list2[j]);
                if (sub <= 0)
                {
                    list.Insert(i, list2[j]);
                    j++;
                }
            }
            for (; j < list2.Count; j++)
            {
                list.Add(list2[j]);
            }
            return list.Count;
        }

        /// <summary>
        /// 合并两个有序列表,他们都是从小到大排序的
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="list2"></param>
        /// <param name="comparer"></param>
        /// <returns></returns>
        public static int BinaryCombineAsc<T>(this IList<T> list, IList<T> list2, IComparer<T> comparer)
        {
            if (null == list)
            {
                return -1;
            }
            else if (list.Count == 0)
            {
                if (null != list2 && list2.Count > 0)
                {
                    foreach (var data in list2)
                    {
                        list.Add(data);
                    }
                }
                return list.Count;
            }

            int sub;
            int i = 0, j = 0;
            for (; i < list.Count && j < list2.Count; i++)
            {
                sub = comparer.Compare(list[i], list2[j]);
                if (sub > 0)
                {
                    list.Insert(i, list2[j]);
                    j++;
                }
            }
            for (; j < list2.Count; j++ )
            {
                list.Add(list2[j]);
            }
            return list.Count;
        }
    }
}
