using FS.GameEngine.Logic;
using FS.VLTK.Utilities;
using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace FS.VLTK.Loader
{
	/// <summary>
	/// Tải xuống thông tin ghi nhớ tài khoản và mật khẩu người chơi
	/// </summary>
	public class LoadSavedAccountInfo : TTMonoBehaviour
	{
		#region Define
		/// <summary>
		/// Tên File AccountInfo
		/// </summary>
		public const string AccountInfoFile = "SavedData.dat";
		#endregion

		#region Properties
		/// <summary>
		/// Sự kiện đọc File hoàn tất
		/// </summary>
		public Action<string, string> Done { get; set; }

		/// <summary>
		/// Sự kiện đọc File thất bại
		/// </summary>
		public Action<string> Faild { get; set; }
		#endregion

		#region Private methods
		/// <summary>
		/// Xóa File dữ liệu
		/// </summary>
		public void DeleteData()
		{
			/// Tên File
			string fileName = Path.Combine(Application.persistentDataPath, LoadSavedAccountInfo.AccountInfoFile);
			/// Nếu tồn tại
			if (File.Exists(fileName))
			{
				/// Xóa File
				File.Delete(fileName);
			}
		}
		#endregion

		#region Public methods
		/// <summary>
		/// Đọc dữ liệu thông tin tài khoản lưu lại
		/// </summary>
		/// <returns></returns>
		public IEnumerator ReadData()
		{
			/// Đường dẫn
			string fullPath = Global.WebPath(LoadSavedAccountInfo.AccountInfoFile);

			/// Nội dung File
			UnityWebRequest request = new UnityWebRequest(fullPath);
			request.downloadHandler = new DownloadHandlerBuffer();
			/// Gửi yêu cầu
			yield return request.SendWebRequest();

			/// Nếu có lỗi gì đó
			if (!string.IsNullOrEmpty(request.error))
			{
				this.Faild?.Invoke(request.error);
				yield break;
			}

			try
			{
				/// Chuỗi Byte kết quả
				byte[] byteData = request.downloadHandler.data;
				/// Xóa yêu càu
				request.downloadHandler.Dispose();
				request.Dispose();
				/// Xóa File
				this.DeleteData();

				/// Giải mã
				byteData = KTResourceCrypto.Decrypt(byteData);

				/// Chuyển thành chuỗi XML tương ứng
				string strText = new UTF8Encoding().GetString(byteData);
				/// Loại ký tự thừa ở đầu
				string _byteOrderMarkUtf8 = Encoding.UTF8.GetString(Encoding.UTF8.GetPreamble());
				if (strText.StartsWith(_byteOrderMarkUtf8))
				{
					strText = strText.Replace(_byteOrderMarkUtf8, "");
				}
				strText = "<!--" + strText;

				XElement xmlNode = XElement.Parse(strText);
				/// Tài khoản
				string account = xmlNode.Attribute("Account").Value;
				string password = xmlNode.Attribute("Password").Value;

				/// Thực hiện hàm Callback
				this.Done?.Invoke(account, password);
			}
			catch (Exception ex)
			{
				this.Faild?.Invoke(ex.ToString());
			}
		}

		/// <summary>
		/// Lưu lại thông tin tài khoản và mật khẩu
		/// </summary>
		/// <param name="account"></param>
		public void SaveData(string account, string password)
		{
			try
			{
				XElement xmlNode = new XElement("AccountInfo");
				xmlNode.Add(new XAttribute("Account", account));
				xmlNode.Add(new XAttribute("Password", password));
				string secureKey = "VLTK.FS.vn\n\n\n-->";
				string xmlString = secureKey + xmlNode.ToString();

				/// Chuyển thành chuối Byte tương ứng
				byte[] byteData = new UTF8Encoding().GetBytes(xmlString);

				/// Mã hóa
				byteData = KTResourceCrypto.Encrypt(byteData);

				/// Lưu lại thông tin vào File
				Utils.SaveBytesToFile(LoadSavedAccountInfo.AccountInfoFile, byteData);
			}
			catch (Exception) { }
		}
		#endregion
	}
}
