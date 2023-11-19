using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KIEMTHESDK.Models
{
	[ProtoContract]
	public class BuffServerListData
	{
		// Token: 0x0400001A RID: 26
		[ProtoMember(1)]
		public List<BuffServerInfo> listServerData = new List<BuffServerInfo>();

		// Token: 0x0400001B RID: 27
		[ProtoMember(2)]
		public bool IsAllPause;

		// Token: 0x0400001C RID: 28
		[ProtoMember(3)]
		public int ServerCount;

		// Token: 0x0400001D RID: 29
		[ProtoMember(4)]
		public string strMaintainTxt;

		// Token: 0x0400001E RID: 30
		[ProtoMember(5)]
		public string strMaintainStarTime;

		// Token: 0x0400001F RID: 31
		[ProtoMember(6)]
		public string strMaintainTerminalTime;
	}
}