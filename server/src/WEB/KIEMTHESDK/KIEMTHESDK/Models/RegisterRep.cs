using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KIEMTHESDK.Models
{
	[ProtoContract]
	public class RegisterRep
	{
		// Token: 0x0400002D RID: 45
		[ProtoMember(1)]
		public int ErrorCode = 0;

		// Token: 0x0400002E RID: 46
		[ProtoMember(2)]
		public string ErorrMsg = "";
	}
}