using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KIEMTHESDK.Models
{
	[ProtoContract]
	public class BuffServerListDataOld
	{
		// Token: 0x04000023 RID: 35
		[ProtoMember(1)]
		public List<BuffServerInfo> ListServerData = new List<BuffServerInfo>();

		// Token: 0x04000024 RID: 36
		[ProtoMember(2)]
		public List<BuffServerInfo> RecommendListServerData = new List<BuffServerInfo>();
	}
}