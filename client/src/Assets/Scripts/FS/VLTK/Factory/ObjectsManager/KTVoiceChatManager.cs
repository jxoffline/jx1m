using FS.GameEngine.Logic;
using FS.GameEngine.Network;
using FS.VLTK.Utilities;
using Server.Data;
using Server.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using static FS.VLTK.Entities.Enum;

namespace FS.VLTK.Factory.ObjectsManager
{
	/// <summary>
	/// Quản lý tin nhắn thoại
	/// </summary>
	public class KTVoiceChatManager : MonoBehaviour
	{
		#region Singleton - Instance
		/// <summary>
		/// Quản lý tin nhắn thoại
		/// </summary>
		public static KTVoiceChatManager Instance { get; private set; }
		#endregion

		#region Core MonoBehaviour
		/// <summary>
		/// Hàm này gọi khi đối tượng được tạo ra
		/// </summary>
		private void Awake()
		{
			KTVoiceChatManager.Instance = this;
		}
		#endregion

		#region Tạo tin
		/// <summary>
		/// Tạo gói tin tin nhắn thoại mới
		/// </summary>
		/// <param name="toRoleName"></param>
		/// <param name="channel"></param>
		/// <param name="contentData"></param>
		/// <returns></returns>
		private ChatVoiceData MakeVoiceChat(string toRoleName, ChatChannel channel, byte[] contentData)
		{
			string chatID = KTResourceCrypto.MakeMD5Hash(string.Format("{0}|{1}|{2}", Global.Data.RoleData.RoleID, Global.Data.RoleData.ZoneID, DateTime.Now.ToString()));
			return new ChatVoiceData()
			{
				ChatID = chatID,
				Channel = (int) channel,
				FromRoleName = Global.Data.RoleData.RoleName,
				ToRoleName = toRoleName,
				VoiceData = new ServerGetAudioChatData() { arrAudioChat = contentData },
			};
		}
		#endregion

		#region Gửi tin
		/// <summary>
		/// Gửi tin nhắn thoại
		/// </summary>
		/// <param name="toRoleName"></param>
		/// <param name="channel"></param>
		/// <param name="contentData"></param>
		public void SendVoiceChat(string toRoleName, ChatChannel channel, byte[] contentData)
		{
			/// Tạo đối tượng tương ứng
			ChatVoiceData data = this.MakeVoiceChat(toRoleName, channel, contentData);

			/// Chuỗi Byte gói tin gửi đi
			byte[] cmdData = DataHelper.ObjectToBytes<ChatVoiceData>(data);

			/// Gửi lên SDK VoiceChatServer-Push
			SimpleHttpTask.HttpPost(MainGame.GameInfo.PushVoiceUrl, null, cmdData, (request) => {
				this.SendVoiceChatCallback(request, data);
			}, 10f);
		}

		/// <summary>
		/// Hàm Callback phản hồi của SDKServer sau khi gửi gói tin tin nhắn thoại
		/// </summary>
		/// <param name="request"></param>
		/// <param name="data"></param>
		private void SendVoiceChatCallback(UnityWebRequest request, ChatVoiceData data)
		{
			/// Toác gì đó
			if (request == null)
			{
				KTGlobal.AddNotification("Gửi tin nhắn thoại thất bại. Mã lỗi: 'Request is null'.");
				return;
			}
			else if (!string.IsNullOrEmpty(request.error))
			{
				KTGlobal.AddNotification("Gửi tin nhắn thoại thất bại. Mã lỗi: '" + request.error + "'.");
				return;
			}

			/// Chuỗi Byte kết quả trả về
			byte[] returnBytes = request.downloadHandler.data;

			/// Nếu không có kết quả
			if (returnBytes == null || returnBytes.Length <= 0)
			{
				KTGlobal.AddNotification("Gửi tin nhắn thoại thất bại. Mã lỗi: 'ResponseData is null'.");
				return;
			}

			/// Chuyển sang đối tượng tương ứng
			VoiceChatResponse response = DataHelper.BytesToObject<VoiceChatResponse>(returnBytes, 0, returnBytes.Length);
			/// Nếu toác
			if (response == null)
			{
				KTGlobal.AddNotification("Gửi tin nhắn thoại thất bại. Mã lỗi: 'ResponseData is not correct'.");
				return;
			}

			/// Nếu không gửi được
			if (response.Status == 0)
			{
				KTGlobal.AddNotification("Gửi tin nhắn thoại thất bại. Mã lỗi: 'Server can not save data'.");
				return;
			}

			/// Gửi nội dung Chat lên Server
			GameInstance.Game.SpriteSendChat(Global.Data.RoleData.RoleName, data.ToRoleName, string.Format("@VOICE_{0}", data.ChatID), (ChatChannel) data.Channel);

			/// Thông báo gửi thành công
			KTGlobal.AddNotification("Gửi tin nhắn thành công!");
		}
		#endregion

		#region Gửi yêu cầu tải nội dung tin nhắn thoại
		/// <summary>
		/// Gửi yêu cầu tải nội dung tin nhắn thoại
		/// </summary>
		/// <param name="chatID"></param>
		/// <param name="callback"></param>
		public void SendGetVoiceChatContent(string chatID, Action<byte[]> callback)
		{
			/// Tạo đối tượng tương ứng
			GetVoiceFile data = new GetVoiceFile()
			{
				ChatID = chatID,
			};

			/// Chuỗi Byte gói tin gửi đi
			byte[] cmdData = DataHelper.ObjectToBytes<GetVoiceFile>(data);

			/// Gửi lên SDK VoiceChatServer-Get
			SimpleHttpTask.HttpPost(MainGame.GameInfo.GetVoiceUrl, null, cmdData, (request) => {
				this.GetVoiceChatContentCallback(request, callback);
			}, 10f);
		}

		/// <summary>
		/// Hàm Callback phản hồi của SDKServer sau khi yêu cầu lấy nội dung tin nhắn thoại
		/// </summary>
		/// <param name="request"></param>
		/// <param name="callback"></param>
		private void GetVoiceChatContentCallback(UnityWebRequest request, Action<byte[]> callback)
		{
			/// Toác gì đó
			if (request == null)
			{
				KTGlobal.AddNotification("Tải nội dung tin nhắn thoại thất bại. Mã lỗi: 'Request is null'.");
				return;
			}
			else if (!string.IsNullOrEmpty(request.error))
			{
				KTGlobal.AddNotification("Tải nội dung tin nhắn thoại thất bại. Mã lỗi: '" + request.error + "'.");
				return;
			}

			/// Chuỗi Byte kết quả trả về
			byte[] returnBytes = request.downloadHandler.data;

			/// Nếu không có kết quả
			if (returnBytes == null || returnBytes.Length <= 0)
			{
				KTGlobal.AddNotification("Tải nội dung tin nhắn thoại thất bại. Mã lỗi: 'ResponseData is null'.");
				return;
			}

			/// Chuyển sang đối tượng tương ứng
			ServerGetAudioChatData chatData = DataHelper.BytesToObject<ServerGetAudioChatData>(returnBytes, 0, returnBytes.Length);
			/// Nếu không có kết quả
			if (chatData == null)
			{
				KTGlobal.AddNotification("Tải nội dung tin nhắn thoại thất bại. Mã lỗi: 'ResponseData is not correct'.");
				return;
			}

			/// Thực hiện hàm Callback
			callback?.Invoke(chatData.arrAudioChat);
		}
		#endregion
	}
}
