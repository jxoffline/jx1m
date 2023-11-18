using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Server.Tools
{
    /// <summary>
    /// 获取crc32校验和的类
    /// </summary>
    public class CRC32
    {
		/** The crc data checksum so far. */
		private uint crc = 0;

		/** The fast CRC table. Computed once when the CRC32 class is loaded. */
		private static uint[] crcTable = makeCrcTable();

		/** Make the table for a fast CRC. */
		private static uint[] makeCrcTable() {
			uint[] crcTable = new uint[256];
			for (int n = 0; n < 256; n++) {
				uint c = (uint)n;
				for (int k = 8; --k >= 0; ) {
					if((c & 1) != 0) c = 0xedb88320 ^ (c >> 1);
					else c = c >> 1;
				}
				crcTable[n] = c;
			}
			return crcTable;
		}

		/**
		 * Returns the CRC32 data checksum computed so far.
		 */
		public uint getValue() {
			return crc & 0xffffffff;
		}
		
		/**
		 * Resets the CRC32 data checksum as if no update was ever called.
		 */
		public void reset() {
			crc = 0;
		}

		/**
		 * Adds the complete byte array to the data checksum.
		 * 
		 * @param buf the buffer which contains the data
		 */
		public void update(byte[] buf) {
			uint off = 0;
			int len = buf.Length;
			uint c = ~crc;
			while(--len >= 0) c = crcTable[(c ^ buf[off++]) & 0xff] ^ (c >> 8);
			crc = ~c;
		}

        /**
         * Adds the complete byte array to the data checksum.
         * 
         * @param buf the buffer which contains the data
         */
        public void update(byte[] buf, int off, int len)
        {
            uint c = ~crc;
            while (--len >= 0) c = crcTable[(c ^ buf[off++]) & 0xff] ^ (c >> 8);
            crc = ~c;
        }
    }
}
