using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KIEMTHESDK.Models
{
	[ProtoContract]
	public class BuffServerListDataEx
	{
		// Token: 0x04000020 RID: 32
		[ProtoMember(1)]
		public List<BuffServerInfo> ListServerData = new List<BuffServerInfo>();

		// Token: 0x04000021 RID: 33
		[ProtoMember(2)]
		public List<BuffServerInfo> RecommendListServerData = new List<BuffServerInfo>();

		// Token: 0x04000022 RID: 34
		[ProtoMember(3)]
		public BuffServerInfo LastServerLogin = new BuffServerInfo();
	}
}