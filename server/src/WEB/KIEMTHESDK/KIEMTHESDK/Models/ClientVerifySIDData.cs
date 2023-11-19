using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KIEMTHESDK.Models
{
	[ProtoContract]
	public class ClientVerifySIDData
	{
		// Token: 0x04000025 RID: 37
		[ProtoMember(1)]
		public string strSID = "";

		// Token: 0x04000026 RID: 38
		[ProtoMember(2)]
		public long lTime;

		// Token: 0x04000027 RID: 39
		[ProtoMember(3)]
		public string strMD5 = "";
	}
}