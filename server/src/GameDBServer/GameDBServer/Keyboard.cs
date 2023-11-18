using Microsoft.Win32;
using System;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace REGHWIDPC
{
    public static class HardwareAnalyzer
    {
        #region Methods: Imports

        [DllImport("Iphlpapi.dll", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true)]
        internal static extern Int32 GetAdaptersInfo(IntPtr handle, ref UInt32 size);

        [DllImport("Kernel32.dll", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern Boolean CloseHandle([In] IntPtr handle);

        [DllImport("Kernel32.dll", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern Boolean DeviceIoControl([In] IntPtr handle, [In] UInt32 controlCode, [In, Optional] IntPtr bufferIn, [In] UInt32 bufferInSize, [Out, Optional] IntPtr bufferOut, [In] UInt32 bufferOutSize, [Out] out UInt32 bytesReturned, [In, Out, Optional] IntPtr overlapped);

        [DllImport("Kernel32.dll", CharSet = CharSet.Unicode, ExactSpelling = false, SetLastError = true)]
        internal static extern IntPtr CreateFile([In] String fileName, [In] eFileAccess fileAccess, [In] EFileShare fileShare, [In, Optional] IntPtr fileSecurity, [In] eCreationDisposition creationDisposition, [In] UInt32 flags, [In, Optional] IntPtr handleTemplateFile);

        #endregion Methods: Imports

        #region Methods: Functions

        private static String RetrieveDiskSerial()
        {
            String serial = String.Empty;

            try
            {
                IntPtr handle = IntPtr.Zero;

                for (Int32 i = 0; i < 16; ++i)
                {
                    handle = CreateFile(String.Format("\\\\.\\PhysicalDrive{0}", i), (eFileAccess.GenericRead | eFileAccess.GenericWrite), (EFileShare.Read | EFileShare.Write), IntPtr.Zero, eCreationDisposition.OpenExisting, 0, IntPtr.Zero);

                    if (handle != IntPtr.Zero)
                    {
                        serial = RetrieveDiskSerialSmart(handle);

                        if (serial.Length == 0)
                            serial = RetrieveDiskSerialStorageQuery(handle);

                        if (serial.Length == 0)
                            continue;
                        /*
                        if (!CloseHandle(handle))
                           // Console.WriteLine("WARNING: a file handle has not been correctly closed.");
                        */
                        break;
                    }
                }
            }
            catch { }

            return serial;
        }

        private static String RetrieveDiskSerialSmart(IntPtr handle)
        {
            IntPtr bufferIn = Marshal.AllocHGlobal(32);
            IntPtr bufferOut = Marshal.AllocHGlobal(24);
            String serial = String.Empty;
            UInt32 bytesReturned = 0;

            try
            {
                if (DeviceIoControl(handle, 0x074080, IntPtr.Zero, 0, bufferOut, 24, out bytesReturned, IntPtr.Zero))
                {
                    if ((Marshal.ReadInt32(bufferOut, 4) & 4) > 0)
                    {
                        SCInputParameters parameters = new SCInputParameters();
                        bufferOut = Marshal.ReAllocHGlobal(bufferOut, (IntPtr)528);

                        Marshal.StructureToPtr(parameters, bufferIn, true);

                        if (DeviceIoControl(handle, 0x07C088, bufferIn, 32, bufferOut, 528, out bytesReturned, IntPtr.Zero))
                        {
                            String serialANSI = Marshal.PtrToStringAnsi((IntPtr)(bufferOut.ToInt32() + 36), 20);

                            if (serialANSI.Length != 0)
                            {
                                Char[] serialANSICharacters = serialANSI.ToCharArray();

                                for (Int32 i = 0; i <= (serialANSICharacters.Length - 2); i += 2)
                                {
                                    Char current = serialANSICharacters[i];

                                    serialANSICharacters[i] = serialANSICharacters[(i + 1)];
                                    serialANSICharacters[(i + 1)] = current;
                                }

                                serial = new String(serialANSICharacters).Trim();
                            }
                        }
                    }
                }
            }
            finally
            {
                Marshal.FreeHGlobal(bufferIn);
                Marshal.FreeHGlobal(bufferOut);
            }

            return serial;
        }

        private static String RetrieveDiskSerialStorageQuery(IntPtr handle)
        {
            IntPtr bufferIn = Marshal.AllocHGlobal(12);
            IntPtr bufferOut = Marshal.AllocHGlobal(1024);
            StoragePropertyQuery query = new StoragePropertyQuery();
            String serial = String.Empty;
            UInt32 bytesReturned = 0;

            try
            {
                Marshal.StructureToPtr(query, bufferIn, true);

                if (DeviceIoControl(handle, 0x2D1400, bufferIn, 12, bufferOut, 1024, out bytesReturned, IntPtr.Zero))
                {
                    Int32 address = bufferOut.ToInt32();
                    Int32 offset = Marshal.ReadInt32(bufferOut, 24);

                    if (offset != 0)
                    {
                        String serialANSI = Marshal.PtrToStringAnsi((IntPtr)(address + offset));

                        if (serialANSI.Length != 0)
                        {
                            StringBuilder builder = new StringBuilder();

                            for (Int32 i = 0; i < serialANSI.Length; i += 4)
                            {
                                for (Int32 j = 1; j >= 0; --j)
                                {
                                    Int32 sum = 0;

                                    for (Int32 y = 0; y < 2; ++y)
                                    {
                                        sum *= 16;

                                        switch (serialANSI[(i + (j * 2) + y)])
                                        {
                                            case '0':
                                                sum += 0;
                                                break;

                                            case '1':
                                                sum += 1;
                                                break;

                                            case '2':
                                                sum += 2;
                                                break;

                                            case '3':
                                                sum += 3;
                                                break;

                                            case '4':
                                                sum += 4;
                                                break;

                                            case '5':
                                                sum += 5;
                                                break;

                                            case '6':
                                                sum += 6;
                                                break;

                                            case '7':
                                                sum += 7;
                                                break;

                                            case '8':
                                                sum += 8;
                                                break;

                                            case '9':
                                                sum += 9;
                                                break;

                                            case 'a':
                                                sum += 10;
                                                break;

                                            case 'b':
                                                sum += 11;
                                                break;

                                            case 'c':
                                                sum += 12;
                                                break;

                                            case 'd':
                                                sum += 13;
                                                break;

                                            case 'e':
                                                sum += 14;
                                                break;

                                            case 'f':
                                                sum += 15;
                                                break;
                                        }
                                    }

                                    if (sum > 0)
                                        builder.Append((Char)sum);
                                }
                            }

                            serial = builder.ToString().Trim();
                        }
                    }
                }
            }
            finally
            {
                Marshal.FreeHGlobal(bufferIn);
                Marshal.FreeHGlobal(bufferOut);
            }

            return serial;
        }

        private static String RetrieveMACAddress()
        {
            String address = String.Empty;

            try
            {
                UInt32 size = 0;
                Int32 result = GetAdaptersInfo(IntPtr.Zero, ref size);

                if ((result == 0) || (result == 111))
                {
                    IntPtr buffer = Marshal.AllocHGlobal((IntPtr)size);
                    result = GetAdaptersInfo(buffer, ref size);

                    if (result == 0)
                    {
                        while (true)
                        {
                            String adapterName = Marshal.PtrToStringAnsi((IntPtr)(buffer.ToInt32() + 8));
                            IntPtr handle = CreateFile(String.Format("\\\\.\\{0}", adapterName), (eFileAccess.GenericRead | eFileAccess.GenericWrite), (EFileShare.Read | EFileShare.Write), IntPtr.Zero, eCreationDisposition.OpenExisting, 0, IntPtr.Zero);

                            if (handle != IntPtr.Zero)
                            {
                                IntPtr bufferIn = GCHandle.Alloc(0x1010101, GCHandleType.Pinned).AddrOfPinnedObject();
                                IntPtr bufferOut = Marshal.AllocHGlobal(6);
                                UInt32 bytesReturned = 0;

                                try
                                {
                                    if (DeviceIoControl(handle, 0x170002, bufferIn, 4, bufferOut, 6, out bytesReturned, IntPtr.Zero))
                                    {
                                        String temporaryAddress = String.Empty;

                                        for (Int32 i = 0; i < 6; ++i)
                                            temporaryAddress += Marshal.ReadByte(bufferOut, i).ToString("X2") + ((i == 5) ? "" : ":");

                                        if (temporaryAddress != "00:00:00:00:00:00")
                                        {
                                            address = temporaryAddress;
                                            break;
                                        }
                                    }
                                }
                                finally
                                {
                                    /*
                                    if (!CloseHandle(handle))
                                       // Console.WriteLine("WARNING: a file handle has not been correctly closed.");
                                    */

                                    Marshal.FreeHGlobal(bufferOut);
                                }
                            }

                            Int32 nextAdapterOffset = Marshal.ReadInt32(buffer);

                            if (nextAdapterOffset != 0)
                                buffer = (IntPtr)nextAdapterOffset;
                            else
                                break;
                        }
                    }
                }
            }
            catch { }

            return address;
        }

        private static String RetrieveSMBiosData()
        {
            String data = String.Empty;

            try
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"System\CurrentControlSet\Services\MSSMBios\Data", false))
                {
                    if (key != null)
                    {
                        Byte[] keyData = (Byte[])key.GetValue("SMBiosData");

                        if (keyData != null)
                        {
                            using (MD5CryptoServiceProvider provider = new MD5CryptoServiceProvider())
                                keyData = provider.ComputeHash(keyData);

                            for (Int32 i = 0; i < keyData.Length; ++i)
                                data += keyData[i].ToString("X2");
                        }
                    }
                }
            }
            catch { }

            return data;
        }

        public static String CreateFingerprint()
        {
            String serial = RetrieveDiskSerial();
            String address = RetrieveMACAddress();
            String data = RetrieveSMBiosData();

            if ((serial.Length == 0) && (address.Length == 0) && (data.Length == 0))
                return "0000-0000-0000-0000-0000-0000-0000-0000";

            String fingerprint = String.Empty;

            using (MD5CryptoServiceProvider provider = new MD5CryptoServiceProvider())
            {
                Byte[] hash = provider.ComputeHash(Encoding.ASCII.GetBytes(serial + " - " + address + " - " + data));

                for (Int32 i = 0; i < 16; ++i)
                {
                    fingerprint += hash[i].ToString("X2");

                    if (((i & 1) != 0) && (i != 15))
                        fingerprint += "-";
                }
            }

            return fingerprint;
        }

        #endregion Methods: Functions

        #region Nesting: Enumerators

        public enum eCreationDisposition : uint
        {
            New = 1,
            CreateAlways = 2,
            OpenExisting = 3,
            OpenAlways = 4,
            TruncateExisting = 5
        }

        [Flags]
        public enum eFileAccess : uint
        {
            Delete = 0x00010000,
            ReadControl = 0x00020000,
            WriteDAC = 0x00040000,
            WriteOwner = 0x00080000,
            Synchronize = 0x00100000,
            AccessSystemSecurity = 0x01000000,
            MaximumAllowed = 0x02000000,
            GenericAll = 0x10000000,
            GenericExecute = 0x20000000,
            GenericWrite = 0x40000000,
            GenericRead = 0x80000000
        }

        [Flags]
        public enum EFileShare : uint
        {
            None = 0x00000000,
            Read = 0x00000001,
            Write = 0x00000002,
            Delete = 0x00000004
        }

        #endregion Nesting: Enumerators

        #region Nesting: Structures

        [StructLayout(LayoutKind.Sequential)]
        private class SCInputParameters
        {
            private int BufferSize = 528;
            private Byte Features = 0;
            private Byte SectorCount = 1;
            private Byte SectorNumber = 1;
            private Byte LowOrderCylinder = 0;
            private Byte HighOrderCylinder = 0;
            private Byte DriveHead = 160;
            private Byte Command = 236;
            private Byte Reserved = 0;
            private Byte DriveNumber = 0;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            private Byte[] UselessData = new Byte[16];
        }

        [StructLayout(LayoutKind.Sequential)]
        private class StoragePropertyQuery
        {
            private Int32 PropertyID;
            private Int32 QueryType;
            private Int32 UselessData;
        }

        #endregion Nesting: Structures
    }
}