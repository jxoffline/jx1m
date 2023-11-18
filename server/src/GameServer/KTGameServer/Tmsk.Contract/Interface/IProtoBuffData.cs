using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tmsk.Contract
{
    public interface IProtoBuffData
    {
        int fromBytes(byte[] data, int offset, int count);
        byte[] toBytes();
    }
}
