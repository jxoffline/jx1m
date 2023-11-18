using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tmsk.Contract
{
    public class IntPairKey : IEquatable<IntPairKey>, IComparable<IntPairKey>, IEqualityComparer<IntPairKey>, IComparer<IntPairKey>
    {
        public static IntPairKey Comparer = new IntPairKey();

        int Int1;
        int Int2;
        public IntPairKey(){}
        public IntPairKey(int int1, int int2)
        {
            Int1 = int1;
            Int2 = int2;
        }

        public bool Equals(IntPairKey other)
        {
            return Int2 == other.Int2 && Int1 == other.Int1;
        }

        public override int GetHashCode()
        {
            return Int2;
        }

        public override bool Equals(object other)
        {
            IntPairKey obj = other as IntPairKey;
            if (null == obj)
            {
                return false;
            }

            return Int2 == obj.Int2 && Int1 == obj.Int1;
        }

        public bool Equals(IntPairKey x, IntPairKey y)
        {
            return x.Int2 == y.Int2 && x.Int1 == y.Int1;
        }

        public int GetHashCode(IntPairKey obj)
        {
            return (obj.Int1 << 24) + (obj.Int2);
        }

        public int CompareTo(IntPairKey other)
        {
            int ret = Int1 - other.Int1;
            if (ret != 0)
            {
                return ret;
            }

            return Int2 - other.Int2;
        }

        public int Compare(IntPairKey x, IntPairKey y)
        {
            int ret = x.Int1 - y.Int1;
            if (ret != 0)
            {
                return ret;
            }

            return x.Int2 - y.Int2;
        }
    }
}
