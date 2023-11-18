using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameServer.Logic
{
    public class RangeKey : IComparable<RangeKey>, IEqualityComparer<RangeKey>
    {
        public static RangeKey Comparer = new RangeKey(-1, -1);

        int Left;
        int Right;
        public object tag;

        public RangeKey(int value)
        {
            Left = value;
            Right = value;
        }

        public RangeKey(int left, int right, object obj = null)
        {
            Left = left;
            Right = right;
            tag = obj;
        }

        public int CompareTo(RangeKey obj)
        {
            if (Right < obj.Left)
            {
                return -1;
            }
            else if (Left > obj.Right)
            {
                return 1;
            }

            return 0;
        }

        public bool Equals(RangeKey x, RangeKey y)
        {
            return 0 == x.CompareTo(y);
        }

        public int GetHashCode(RangeKey obj)
        {
            return 0;
        }

        public static implicit operator RangeKey(int key)
        {
            return new RangeKey(key, key);
        }
    }
}
