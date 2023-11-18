using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;

class ProtoUtil
{
    #region int about
    public static int CalcIntSize(int val)
    {
        if (val < 0) return 10;

        int count = 0;
        do { count++; } while ((val >>= 7) != 0);

        return count;
    }

    public static int GetIntSize(int val, bool calcMember = false, int protoMember = 0, bool useDef = true, int defval = 0)
    {
        if (useDef && val == defval)
            return 0;

        int ntag = 0;
        if (calcMember)
        {
            if (protoMember <= 15)
                ntag = 1;
            else
                ntag = 2;
        }

        if (val < 0) return ntag + 10;

        int count = 0;
        do { count++; } while ((val >>= 7) != 0);

        return ntag + count;
    }

    public static int IntToBytes(byte[] data, int offset, int val)
    {
        int count = 0;
        if (val >= 0)
        {
            do
            {
                count++;
                data[offset++] = (byte)((val & 0x7F) | 0x80);
            } while ((val >>= 7) != 0);

            data[offset - 1] &= 0x7F;
        }
        else
        {
            data[offset] = (byte)(val | 0x80);
            data[offset + 1] = (byte)((val >> 7) | 0x80);
            data[offset + 2] = (byte)((val >> 14) | 0x80);
            data[offset + 3] = (byte)((val >> 21) | 0x80);
            data[offset + 4] = (byte)((val >> 28) | 0x80);
            data[offset + 5] = data[offset + 6] = data[offset + 7] = data[offset + 8] = (byte)0xFF;
            data[offset + 9] = (byte)0x01;
            count = 10;
        }
        return count;
    }

    public static int IntFromBytes(byte[] data, ref int offset, ref int ncount)
    {
        int readPos = offset;
        int count = 0;
        uint value = 0;
        do
        {
            value = data[readPos++];
            if ((value & 0x80) == 0)
            {
                count = 1;
                break;
            }
            value &= 0x7F;

            uint chunk = data[readPos++];
            value |= (chunk & 0x7F) << 7;
            if ((chunk & 0x80) == 0)
            {
                count = 2;
                break;
            }

            chunk = data[readPos++];
            value |= (chunk & 0x7F) << 14;
            if ((chunk & 0x80) == 0)
            {
                count = 3;
                break;
            }

            chunk = data[readPos++];
            value |= (chunk & 0x7F) << 21;
            if ((chunk & 0x80) == 0)
            {
                count = 4;
                break;
            }

            chunk = data[readPos];
            value |= chunk << 28; // can only use 4 bits from this chunk
            if ((chunk & 0xF0) == 0)
            {
                count = 5;
                break;
            }

            if ((chunk & 0xF0) == 0xF0 &&
                    data[++readPos] == 0xFF &&
                    data[++readPos] == 0xFF &&
                    data[++readPos] == 0xFF &&
                    data[++readPos] == 0xFF &&
                    data[++readPos] == 0x01)
            {
                count = 10;
            }

        } while (false);

        offset += count;
        ncount += count;
        return (int)value;
    }

    public static void GetTag(byte[] data, ref int offset, ref int fieldnumber, ref int wt, ref int ncount)
    {
        int tag = IntFromBytes(data, ref offset, ref ncount);
        fieldnumber = (tag >> 3);
        wt = (tag & 7);
    }

    public static void IntMemberToBytes(byte[] data, int fieldnumber, ref int offset, int val, bool useDef = true, int defval = 0)
    {
        if (useDef && val == defval)
            return;

        int tag = ((fieldnumber << 3) | (int)WireType.Variant);
        offset += IntToBytes(data, offset, tag);
        offset += IntToBytes(data, offset, val);
    }

    public static int IntMemberFromBytes(byte[] data, int wt, ref int offset, ref int ncount)
    {
        if (wt != (int)WireType.Variant)
            throw new ArgumentException("int member from bytes error, type error!!!");

        return IntFromBytes(data, ref offset, ref ncount);
    }

    #endregion

    #region long about
    public static int GetLongSize(long val, bool calcMember = false, int protoMember = 0, bool useDef = true, long defval = 0)
    {
        if (useDef && val == defval) return 0;
        int ntag = 0;
        if (calcMember)
        {
            if (protoMember <= 15)
                ntag = 1;
            else
                ntag = 2;
        }

        if (val < 0) return 10 + ntag;

        int count = 0;
        do { count++; } while ((val >>= 7) != 0);

        return ntag + count;
    }

    private static int LongToBytes(byte[] data, int offset, long val)
    {
        int count = 0;
        if (val >= 0)
        {
            do
            {
                data[offset++] = (byte)((val & 0x7F) | 0x80);
                count++;
            } while ((val >>= 7) != 0);

            data[offset - 1] &= 0x7F;
        }
        else
        {
            count = 10;
            data[offset] = (byte)(val | 0x80);
            data[offset + 1] = (byte)((int)(val >> 7) | 0x80);
            data[offset + 2] = (byte)((int)(val >> 14) | 0x80);
            data[offset + 3] = (byte)((int)(val >> 21) | 0x80);
            data[offset + 4] = (byte)((int)(val >> 28) | 0x80);
            data[offset + 5] = (byte)((int)(val >> 35) | 0x80);
            data[offset + 6] = (byte)((int)(val >> 42) | 0x80);
            data[offset + 7] = (byte)((int)(val >> 49) | 0x80);
            data[offset + 8] = (byte)((int)(val >> 56) | 0x80);
            data[offset + 9] = 0x01; // sign bit
        }
        return count;
    }

    private static long LongFromBytes(byte[] data, ref int offset, ref int ncount)
    {
        int count = 0;
        ulong value = 0;
        do
        {
            int readPos = offset;
            value = data[readPos++];
            if ((value & 0x80) == 0) { count = 1; break; }
            value &= 0x7F;

            ulong chunk = data[readPos++];
            value |= (chunk & 0x7F) << 7;
            if ((chunk & 0x80) == 0) { count = 2; break; }

            chunk = data[readPos++];
            value |= (chunk & 0x7F) << 14;
            if ((chunk & 0x80) == 0) { count = 3; break; }

            chunk = data[readPos++];
            value |= (chunk & 0x7F) << 21;
            if ((chunk & 0x80) == 0) { count = 4; break; }

            chunk = data[readPos++];
            value |= (chunk & 0x7F) << 28;
            if ((chunk & 0x80) == 0) { count = 5; break; }

            chunk = data[readPos++];
            value |= (chunk & 0x7F) << 35;
            if ((chunk & 0x80) == 0) { count = 6; break; }

            chunk = data[readPos++];
            value |= (chunk & 0x7F) << 42;
            if ((chunk & 0x80) == 0) { count = 7; break; }


            chunk = data[readPos++];
            value |= (chunk & 0x7F) << 49;
            if ((chunk & 0x80) == 0) { count = 8; break; }

            chunk = data[readPos++];
            value |= (chunk & 0x7F) << 56;
            if ((chunk & 0x80) == 0) { count = 9; break; }

            chunk = data[readPos];
            value |= chunk << 63; // can only use 1 bit from this chunk
            count = 10;

            if ((chunk & ~(ulong)0x01) != 0) throw new OverflowException("long parse over flow, sign bit error!");

        } while (false);

        offset += count;
        ncount += count;
        return (long)value;
    }

    public static void LongMemberToBytes(byte[] data, int fieldnumber, ref int offset, long val, bool useDef = true, long defval = 0)
    {
        if (useDef && val == defval) return;

        int tag = ((fieldnumber << 3) | (int)WireType.Variant);
        offset += IntToBytes(data, offset, tag);
        offset += LongToBytes(data, offset, val);
    }

    public static long LongMemberFromBytes(byte[] data, int wt, ref int offset, ref int ncount)
    {
        if (wt != (int)WireType.Variant)
            throw new ArgumentException("long member from bytes error, type error!!!");

        return LongFromBytes(data, ref offset, ref ncount);
    }
    #endregion

    #region double about
    public static int GetDoubleSize(double val, bool calcMember = false, int protoMember = 0, bool useDef = true, double defval = 0)
    {
        if (useDef && val == defval) return 0;

        int ntag = 0;
        if (calcMember)
        {
            if (protoMember <= 15)
                ntag = 1;
            else
                ntag = 2;
        }

        return ntag + 8;
    }

    public static void DoubleMemberToBytes(byte[] data, int fieldnumber, ref int offset, double val, bool useDef = true, double valdef = 0)
    {
        if (useDef && val == valdef) return;

        int tag = ((fieldnumber << 3) | (int)WireType.Fixed64);
        offset += IntToBytes(data, offset, tag);

        //long value = *( (long*) &val );
        long value = BitConverter.ToInt64(BitConverter.GetBytes(val), 0);

        data[offset++] = (byte)value;
        data[offset++] = (byte)(value >> 8);
        data[offset++] = (byte)(value >> 16);
        data[offset++] = (byte)(value >> 24);
        data[offset++] = (byte)(value >> 32);
        data[offset++] = (byte)(value >> 40);
        data[offset++] = (byte)(value >> 48);
        data[offset++] = (byte)(value >> 56);
    }

    public static double DoubleMemberFromBytes(byte[] data, int wt, ref int offset, ref int ncount)
    {
        if (wt != (int)WireType.Fixed64)
            throw new ArgumentException("double from bytes error, type error!!!");

        long value = 0;
        value = ((long)data[offset++]);
        value |= (((long)data[offset++]) << 8);
        value |= (((long)data[offset++]) << 16);
        value |= (((long)data[offset++]) << 24);
        value |= (((long)data[offset++]) << 32);
        value |= (((long)data[offset++]) << 40);
        value |= (((long)data[offset++]) << 48);
        value |= (((long)data[offset++]) << 56);

        ncount += 8;
        return BitConverter.ToDouble(BitConverter.GetBytes(value), 0);
    }

    #endregion

    #region string about
    public static int GetStringSize(string val, bool calcMember = false, int protoMember = 0)
    {
        if (val == null) return 0;

        int ntag = 0;
        if (calcMember)
        {
            if (protoMember <= 15)
                ntag = 1;
            else
                ntag = 2;
        }

        int len = val.Length;
        if (len == 0) return 1 + ntag;

        int predicted = new UTF8Encoding().GetByteCount(val);
        return ntag + CalcIntSize(predicted) + predicted;
    }

    public static void StringMemberToBytes(byte[] data, int fieldnumber, ref int offset, string val)
    {
        if (val == null) return;

        int tag = ((fieldnumber << 3) | (int)WireType.String);
        offset += IntToBytes(data, offset, tag);

        int len = val.Length;
        if (len == 0)
        {
            data[offset++] = 0;
            return;
        }

        int predicted = new UTF8Encoding().GetByteCount(val);
        offset += IntToBytes(data, offset, predicted);

        int actual = new UTF8Encoding().GetBytes(val, 0, val.Length, data, offset);
        offset += predicted;

        //Helpers.DebugAssert(predicted == actual);
    }

    public static string StringMemberFromBytes(byte[] data, int wt, ref int offset, ref int ncount)
    {
        if (wt != (int)WireType.String)
            throw new ArgumentException("string from bytes error, type error!!!");

        int bytes = IntFromBytes(data, ref offset, ref ncount);
        if (bytes == 0) return "";

        string s = new UTF8Encoding().GetString(data, offset, bytes);
        offset += bytes;
        ncount += bytes;
        return s;
    }

    #endregion

    public static int GetDictionaryMemberHeader(int fieldnumber)
    {
        return ((fieldnumber << 3) | (int)WireType.String);
    }
}
