using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KF.Remoting
{
    class CoupleArenaDivorceRecord
    {
        private HashSet<long> keySet = new HashSet<long>();

        public void Add(int roleId1, int roleId2)
        {
            var key = GetUnionCouple(roleId1, roleId2);
            if (!keySet.Contains(key))
                keySet.Add(key);
        }

        public bool IsDivorce(int roleId1, int roleId2)
        {
            var key = GetUnionCouple(roleId1, roleId2);
            return keySet.Contains(key);
        }

        public void Reset()
        {
            keySet.Clear();
        }

        private long GetUnionCouple(int a1, int a2)
        {
            int min = Math.Min(a1, a2);
            int max = Math.Max(a1, a2);

            long v = min;
            v = v << 32;
            v = v | (uint)max;

            return v;
        }
    }
}
