using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tmsk.Contract
{
    public class RangeKey64 : IComparable<RangeKey64>, IEqualityComparer<RangeKey64>, IComparer<RangeKey64>
    {
        public static RangeKey64 Comparer = new RangeKey64(-1, -1);

        public long Left { get; private set; }
        public long Right { get; private set; }
        public object tag;

        public RangeKey64(long value)
        {
            Left = value;
            Right = value;
        }

        public RangeKey64(long left, long right, object obj = null)
        {
            Left = left;
            Right = right;
            tag = obj;
        }

        public int CompareTo(RangeKey64 obj)
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

        public int Compare(RangeKey64 x, RangeKey64 y)
        {
            return x.CompareTo(y);
        }

        public bool Equals(RangeKey64 x, RangeKey64 y)
        {
            return 0 == x.CompareTo(y);
        }

        public int GetHashCode(RangeKey64 obj)
        {
            return 0;
        }

        public static implicit operator RangeKey64(int key)
        {
            return new RangeKey64(key, key);
        }
    }
}
