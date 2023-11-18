using System;

namespace GameServer.KiemThe.Utilities
{
    public class KTMemoryUtilities
    {
        public byte[] GlobalArray { get; set; }

        public int Read1Byte(int address)
        {
            byte[] array = new byte[4];
            Array.Copy(GlobalArray, address, array, 0, 1);
            return BitConverter.ToInt32(array, 0);
        }

        public int Read2Byte(int address)
        {
            byte[] array = new byte[4];
            Array.Copy(GlobalArray, address, array, 0, 2);
            return BitConverter.ToInt32(array, 0);
        }

        public int Read4Byte(int address)
        {
            byte[] array = new byte[4];
            Array.Copy(GlobalArray, address, array, 0, 4);
            return BitConverter.ToInt32(array, 0);
        }
    }
}