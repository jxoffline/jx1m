using System;
using System.Collections.Generic;
using System.Linq;
using ProtoBuf;

namespace Server.Data
{
    /// <summary>
    /// 请求语音信息时，客户端传过的参数数据
    /// </summary>
    [ProtoContract]
    public class ClientExchangeOrderResultData
    {
		/// <summary>
		/// 平台名
		/// </summary>
		[ProtoMember(1)]
		public String strPlatform;
		
		/// <summary>
		/// 用户ID
		/// </summary>
		[ProtoMember(2)]
		public String strUserID;
		
		/// <summary>
		/// 充值成功的金额
		/// </summary>
		[ProtoMember(3)]
		public int nMoney;
		
		/// <summary>
		/// 交易流水号
		/// </summary>
		[ProtoMember(4)]
		public String strExchangeOrder;
		
		/// <summary>
		/// 服务器编号
		/// </summary>
		[ProtoMember(5)]
		public int nServerID;        
		
		/// <summary>
		/// 发送消息时的时间戳
		/// </summary>
		[ProtoMember(6)]
		public long lTime;
		
		/// <summary>
		/// 客户端访问appstrore充值接口的验证数据
		/// </summary>
		[ProtoMember(7)]
		public byte[] recipent_data;
		
		/// <summary>
		/// 平台名、用户ID、充值成功的金额、交易流水号、服务器ID、时间戳格式化成字符串后，与私钥生成的MD5码，以便作简单的验证。
		/// </summary>
		[ProtoMember(8)]
		public String strMD5;

		/// <summary>
		/// IOS版本号
		/// </summary>
		[ProtoMember(9)]
		public String strIOSVer;
		
		/// <summary>
		/// Transaction ID
		/// </summary>
		[ProtoMember(10)]
		public String strTransactionId;
		
		/// <summary>
		/// NSLocaleCountryCode
		/// </summary>
		[ProtoMember(11)]
		public String strNSLocaleCountryCode = "";
		
		/// <summary>
		/// NSLocaleCurrencyCode
		/// </summary>
		[ProtoMember(12)]
		public String strNSLocaleCurrencyCode = "";

		[ProtoMember(13)]
		public String VersionCode = "";
	}

    /// <summary>
    /// 验证SID后，返回平台用户ID
    /// </summary>
    [ProtoContract]
    public class ServerExchangeOrderResultData
    {
        /// <summary>
        /// 验证状态(0表示验证成功，-1表示验证失败，其他待添加)
        /// </summary>
        [ProtoMember(1)]
        public string strState = "";

        /// <summary>
        /// 发送消息时的时间戳
        /// </summary>
        [ProtoMember(2)]
        public long lTime;

        /// <summary>
        /// 平台名、用户ID、充值成功的金额、交易流水号、服务器ID、时间戳格式化成字符串后，与私钥生成的MD5码，以便作简单的验证。
        /// </summary>
        [ProtoMember(3)]
        public String strMD5;
    }
}
