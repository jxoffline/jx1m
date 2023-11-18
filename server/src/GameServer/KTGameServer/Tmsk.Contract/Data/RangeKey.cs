using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tmsk.Contract
{
    public class RangeKey : IComparable<RangeKey>, IEqualityComparer<RangeKey>, IComparer<RangeKey>
    {
        public static RangeKey Comparer = new RangeKey(-1, -1);

        public int Left { get; private set; }
        public int Right { get; private set; }
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

        public int Compare(RangeKey x, RangeKey y)
        {
            return x.CompareTo(y);
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
