using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KIEMTHESDK.Models
{
	[ProtoContract]
	public class BuffServerInfo
	{
		// Token: 0x0400000D RID: 13
		[ProtoMember(1)]
		public int nServerOrder;

		// Token: 0x0400000E RID: 14
		[ProtoMember(2)]
		public int nServerID;

		// Token: 0x0400000F RID: 15
		[ProtoMember(3)]
		public int nOnlineNum;

		// Token: 0x04000010 RID: 16
		[ProtoMember(4)]
		public List<int> listMapOnline = new List<int>();

		// Token: 0x04000011 RID: 17
		[ProtoMember(5)]
		public string strServerName = string.Empty;

		// Token: 0x04000012 RID: 18
		[ProtoMember(6)]
		public string strStartTime = string.Empty;

		// Token: 0x04000013 RID: 19
		[ProtoMember(7)]
		public int nStatus;

		// Token: 0x04000014 RID: 20
		[ProtoMember(8)]
		public string strURL;

		// Token: 0x04000015 RID: 21
		[ProtoMember(9)]
		public int nServerPort;

		// Token: 0x04000016 RID: 22
		[ProtoMember(10)]
		public string strMaintainTxt;

		// Token: 0x04000017 RID: 23
		[ProtoMember(11)]
		public string strMaintainStarTime;

		// Token: 0x04000018 RID: 24
		[ProtoMember(12)]
		public string strMaintainTerminalTime;

		// Token: 0x04000019 RID: 25
		[ProtoMember(13)]
		public string Msg;
	}
}