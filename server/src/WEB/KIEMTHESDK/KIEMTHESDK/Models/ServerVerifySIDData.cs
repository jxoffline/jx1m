using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KIEMTHESDK.Models
{
	[ProtoContract]
	public class ServerVerifySIDData
	{
		// Token: 0x0400002F RID: 47
		[ProtoMember(1)]
		public string strPlatformUserID = "";

		// Token: 0x04000030 RID: 48
		[ProtoMember(2)]
		public string strAccountName = "";

		// Token: 0x04000031 RID: 49
		[ProtoMember(3)]
		public long lTime;

		// Token: 0x04000032 RID: 50
		[ProtoMember(4)]
		public string strCM = "1";

		// Token: 0x04000033 RID: 51
		[ProtoMember(5)]
		public string strToken = "";
	}
}