using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Management;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace Server.Tools
{
    public class SHA2Helper
    {
        /// <summary>
        /// 取机器名
        /// </summary>
        /// <returns></returns>
        public static string GethostName()
        {
            return System.Net.Dns.GetHostName();
        }
        /// <summary>
        /// 获取cpu序列号
        /// </summary>
        /// <returns></returns>
        public static string GetCPUSerialNumber()
        {
            string cpuSerialNumber = string.Empty;
            ManagementClass mc = new ManagementClass("Win32_Processor");
            ManagementObjectCollection moc = mc.GetInstances();
            foreach (ManagementObject mo in moc)
            {
                cpuSerialNumber = mo["ProcessorId"].ToString();
                break;
            }
            mc.Dispose();
            moc.Dispose();
            return cpuSerialNumber;
        }
        /// <summary>
        /// 获取硬盘序列号
        /// </summary>
        /// <returns></returns>
        public static string GetDiskSerialNumber()
        {
            return "VKL";
        }
        /// <summary>
        /// 获取网卡硬件地址
        /// </summary>
        /// <returns></returns>
        public static string GetMoAddress()
        {
            string MoAddress = " ";
            using (ManagementClass mc = new ManagementClass("Win32_NetworkAdapterConfiguration"))
            {
                ManagementObjectCollection moc2 = mc.GetInstances();
                foreach (ManagementObject mo in moc2)
                {
                    //if ((bool)mo["IPEnabled"] == true)
                    MoAddress = mo["MacAddress"].ToString();
                    mo.Dispose();
                }
            }
            return MoAddress.ToString();
        }
    }

    public class NetworkAdapterInformation
    {
        public string PNPDeviceID;      // 设备ID
        public UInt32 Index;            // 在系统注册表中的索引号
        public string ProductName;      // 产品名称
        public string ServiceName;      // 服务名称

        public string MACAddress;       // 网卡当前物理地址
        public string PermanentAddress; // 网卡原生物理地址

        public string IPv4Address;      // IP 地址
        public string IPv4Subnet;       // 子网掩码
        public string IPv4Gateway;      // 默认网关
        public Boolean IPEnabled;       // 有效状态       
    }

    /// <summary>
    /// 基于WMI获取本机真实网卡信息
    /// </summary>
    public static class NetworkAdapter
    {
        /// <summary>
        /// 获取本机真实网卡信息，包括物理地址和IP地址
        /// </summary>
        /// <param name="isIncludeUsb">是否包含USB网卡，默认为不包含</param>
        /// <returns>本机真实网卡信息</returns>
        public static NetworkAdapterInformation[] GetNetworkAdapterInformation(Boolean isIncludeUsb = false)
        {   // IPv4正则表达式
            const string IPv4RegularExpression = "^(?:(?:25[0-5]|2[0-4]\\d|((1\\d{2})|([1-9]?\\d)))\\.){3}(?:25[0-5]|2[0-4]\\d|((1\\d{2})|([1-9]?\\d)))$";

            // 注意：只获取已连接的网卡
            string NetworkAdapterQuerystring;
            if (isIncludeUsb)
                NetworkAdapterQuerystring = "SELECT * FROM Win32_NetworkAdapter WHERE (NetConnectionStatus = 2) AND (MACAddress IS NOT NULL) AND (NOT (PNPDeviceID LIKE 'ROOT%'))";
            else
                NetworkAdapterQuerystring = "SELECT * FROM Win32_NetworkAdapter WHERE (NetConnectionStatus = 2) AND (MACAddress IS NOT NULL) AND (NOT (PNPDeviceID LIKE 'ROOT%')) AND (NOT (PNPDeviceID LIKE 'USB%'))";

            ManagementObjectCollection NetworkAdapterQueryCollection = new ManagementObjectSearcher(NetworkAdapterQuerystring).Get();
            if (NetworkAdapterQueryCollection == null) return null;

            List<NetworkAdapterInformation> NetworkAdapterInformationCollection = new List<NetworkAdapterInformation>(NetworkAdapterQueryCollection.Count);
            foreach (ManagementObject mo in NetworkAdapterQueryCollection)
            {
                NetworkAdapterInformation NetworkAdapterItem = new NetworkAdapterInformation();
                NetworkAdapterItem.PNPDeviceID = mo["PNPDeviceID"] as string;
                NetworkAdapterItem.Index = (UInt32)mo["Index"];
                NetworkAdapterItem.ProductName = mo["ProductName"] as string;
                NetworkAdapterItem.ServiceName = mo["ServiceName"] as string;
                NetworkAdapterItem.MACAddress = mo["MACAddress"] as string; // 网卡当前物理地址

                // 网卡原生物理地址
                NetworkAdapterItem.PermanentAddress = GetNetworkAdapterPermanentAddress(NetworkAdapterItem.PNPDeviceID);

                // 获取网卡配置信息
                string ConfigurationQuerystring = "SELECT * FROM Win32_NetworkAdapterConfiguration WHERE Index = " + NetworkAdapterItem.Index.ToString();
                ManagementObjectCollection ConfigurationQueryCollection = new ManagementObjectSearcher(ConfigurationQuerystring).Get();
                if (ConfigurationQueryCollection == null) continue;

                foreach (ManagementObject nacmo in ConfigurationQueryCollection)
                {
                    string[] IPCollection = nacmo["IPAddress"] as string[]; // IP地址
                    if (IPCollection != null)
                    {
                        foreach (string adress in IPCollection)
                        {
                            Match match = Regex.Match(adress, IPv4RegularExpression);
                            if (match.Success) { NetworkAdapterItem.IPv4Address = adress; break; }
                        }
                    }

                    IPCollection = nacmo["IPSubnet"] as string[];   // 子网掩码
                    if (IPCollection != null)
                    {
                        foreach (string address in IPCollection)
                        {
                            Match match = Regex.Match(address, IPv4RegularExpression);
                            if (match.Success) { NetworkAdapterItem.IPv4Subnet = address; break; }
                        }
                    }

                    IPCollection = nacmo["DefaultIPGateway"] as string[];   // 默认网关
                    if (IPCollection != null)
                    {
                        foreach (string address in IPCollection)
                        {
                            Match match = Regex.Match(address, IPv4RegularExpression);
                            if (match.Success) { NetworkAdapterItem.IPv4Gateway = address; break; }
                        }
                    }

                    NetworkAdapterItem.IPEnabled = (Boolean)nacmo["IPEnabled"];
                }

                NetworkAdapterInformationCollection.Add(NetworkAdapterItem);
            }

            if (NetworkAdapterInformationCollection.Count > 0) return NetworkAdapterInformationCollection.ToArray(); else return null;
        }

        /// <summary>
        /// 获取网卡原生物理地址
        /// </summary>
        /// <param name="PNPDeviceID">设备ID</param>
        /// <returns>网卡原生物理地址</returns>
        public static string GetNetworkAdapterPermanentAddress(string PNPDeviceID)
        {
            const UInt32 FILE_SHARE_READ = 0x00000001;
            const UInt32 FILE_SHARE_WRITE = 0x00000002;
            const UInt32 OPEN_EXISTING = 3;
            const UInt32 OID_802_3_PERMANENT_ADDRESS = 0x01010101;
            const UInt32 IOCTL_NDIS_QUERY_GLOBAL_STATS = 0x00170002;
            IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);

            // 生成设备路径名
            string DevicePath = "\\\\.\\" + PNPDeviceID.Replace('\\', '#') + "#{ad498944-762f-11d0-8dcb-00c04fc3358c}";

            // 获取设备句柄
            IntPtr hDeviceFile = CreateFile(DevicePath, 0, FILE_SHARE_READ | FILE_SHARE_WRITE, IntPtr.Zero, OPEN_EXISTING, 0, IntPtr.Zero);
            if (hDeviceFile != INVALID_HANDLE_VALUE)
            {
                Byte[] ucData = new Byte[8];
                Int32 nBytesReturned;

                // 获取原生MAC地址
                UInt32 dwOID = OID_802_3_PERMANENT_ADDRESS;
                Boolean isOK = DeviceIoControl(hDeviceFile, IOCTL_NDIS_QUERY_GLOBAL_STATS, ref dwOID, Marshal.SizeOf(dwOID), ucData, ucData.Length, out nBytesReturned, IntPtr.Zero);
                CloseHandle(hDeviceFile);
                if (isOK)
                {
                    System.Text.StringBuilder sb = new System.Text.StringBuilder(nBytesReturned * 3);
                    foreach (Byte b in ucData)
                    {
                        sb.Append(b.ToString("X2"));
                        sb.Append(':');
                    }
                    return sb.ToString(0, nBytesReturned * 3 - 1);
                }
            }

            return null;
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr CreateFile(
            string lpFileName,
            UInt32 dwDesiredAccess,
            UInt32 dwShareMode,
            IntPtr lpSecurityAttributes,
            UInt32 dwCreationDisposition,
            UInt32 dwFlagsAndAttributes,
            IntPtr hTemplateFile
            );

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern Boolean CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern Boolean DeviceIoControl(
            IntPtr hDevice,
            UInt32 dwIoControlCode,
            ref UInt32 lpInBuffer,
            Int32 nInBufferSize,
            Byte[] lpOutBuffer,
            Int32 nOutBufferSize,
            out Int32 nBytesReturned,
            IntPtr lpOverlapped
            );
    }
}
