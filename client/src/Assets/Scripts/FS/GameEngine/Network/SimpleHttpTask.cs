using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using System;

/// <summary>
/// Lớp thực hiện phương thức trên HTTP
/// </summary>
public class SimpleHttpTask : TTMonoBehaviour
{
	#region Define
	/// <summary>
	/// Thông tin yêu cầu HTTP
	/// </summary>
	private class HttpInfo
	{
		/// <summary>
		/// Đường dẫn
		/// </summary>
		public string URL { get; set; } = null;

		/// <summary>
		/// Loại phương thức
		/// </summary>
		public string Type { get; set; } = "GET";

		/// <summary>
		/// Thời gian chờ tối đa
		/// </summary>
		public float TimeOut { get; set; } = 10f;

		/// <summary>
		/// Dữ liệu Form
		/// </summary>
		public Dictionary<string, string> FormData { get; set; }

		/// <summary>
		/// Chuỗi Byte dữ liệu
		/// </summary>
		public byte[] ByteData { get; set; } = null;

		/// <summary>
		/// Phương thức CallBack khi hoàn tất
		/// </summary>
		public Action<UnityWebRequest> Callback = null;
	}
	#endregion

	#region Private fields
	/// <summary>
	/// Yêu cầu hiện tại
	/// </summary>
	private UnityWebRequest currentRequest = null;

	/// <summary>
	/// Thời gian đã chờ
	/// </summary>
	private float currentTime = -1;

	/// <summary>
	/// Thông tin HTTP hiện tại
	/// </summary>
	private HttpInfo currentHttpInfo = null;
	#endregion

	#region Core MonoBehaviour
	/// <summary>
	/// Hàm này gọi liên tục mỗi Frame
	/// </summary>
	private void Update()
	{
		if (this.currentHttpInfo != null && this.currentRequest != null && (int) this.currentTime != -1)
		{
			this.currentTime += Time.deltaTime;
			if (this.currentTime >= this.currentHttpInfo.TimeOut)
			{
				this.currentRequest.Dispose();
				this.currentHttpInfo.Callback?.Invoke(null);
				this.currentTime = -1f;
				GameObject.DestroyImmediate(this.gameObject);

			}
		}
	}
	#endregion

	#region Private methods
	/// <summary>
	/// Tạo mới
	/// </summary>
	private static SimpleHttpTask NewInstance
	{
		get
		{
			GameObject obj = new GameObject();
			obj.SetActive(true);
			SimpleHttpTask newInstance = obj.AddComponent<SimpleHttpTask>();
			GameObject.DontDestroyOnLoad(obj);
			return newInstance;
		}
	}

	/// <summary>
	/// Bắt đầu gửi yêu cầu
	/// </summary>
	/// <param name="info"></param>
	private void StartHttp(HttpInfo info)
	{
		if (info != null)
		{
			if (info.Type == "GET")
			{
				this.StartCoroutine<bool>(this.DoHttpGet(info));
			}
			if (info.Type == "POST")
			{
				this.StartCoroutine<bool>(this.DoHttpPost(info));
			}
		}
	}

	/// <summary>
	/// Thực hiện gửi yêu cầu phương thức Get
	/// </summary>
	/// <param name="info"></param>
	/// <returns></returns>
	private IEnumerator DoHttpGet(HttpInfo info)
	{
		string dataurl = info.URL;
		if (info.FormData != null)
		{
			dataurl += "?";
			int i = 0;
			foreach (KeyValuePair<string, string> item in info.FormData)
			{
				dataurl += item.Key + "=" + item.Value;
				i++;
				if (i < info.FormData.Count)
					dataurl += "&";
			}
		}

		UnityWebRequest request = UnityWebRequest.Get(dataurl);
		this.currentRequest = request;
		this.currentTime = 0f;
		this.currentHttpInfo = info;
		yield return request.SendWebRequest();
		info.Callback?.Invoke(request);
		if (request.uploadHandler != null)
		{
			request.uploadHandler.Dispose();
		}
		if (request.downloadHandler != null)
		{
			request.downloadHandler.Dispose();
		}
		request.Dispose();
		this.currentHttpInfo = null;
		this.currentTime = -1;
		this.currentRequest = null;
		GameObject.DestroyImmediate(this.gameObject);
	}

	/// <summary>
	/// Thực hiện gửi yêu cầu phương thức Post
	/// </summary>
	/// <param name="info"></param>
	/// <returns></returns>
	private IEnumerator DoHttpPost(HttpInfo info)
	{
		WWWForm form = null;
		UnityWebRequest request = null;
		if (info != null && info.FormData != null)
		{
			form = new WWWForm();
			foreach (KeyValuePair<string, string> item in info.FormData)
			{
				form.AddField(item.Key, item.Value);
			}
			request = UnityWebRequest.Post(info.URL, form);
		}
		if (info.ByteData != null)
		{
			request = new UnityWebRequest(info.URL, UnityWebRequest.kHttpVerbPOST);
			UploadHandlerRaw handler = new UploadHandlerRaw(info.ByteData);
			handler.contentType = "application/octet-stream";
			request.uploadHandler = handler;
			request.downloadHandler = new DownloadHandlerBuffer();
		}

		this.currentRequest = request;
		this.currentTime = 0f;
		this.currentHttpInfo = info;
		yield return request.SendWebRequest();
		info.Callback?.Invoke(request);
		if (request.uploadHandler != null)
		{
			request.uploadHandler.Dispose();
		}
		if (request.downloadHandler != null)
		{
			request.downloadHandler.Dispose();
		}
		request.Dispose();
		this.currentRequest = null;
		this.currentTime = -1;
		this.currentRequest = null;

		GameObject.Destroy(this.gameObject);
	}
	#endregion

	#region Public methods
	/// <summary>
	/// Tạo yêu cầu gửi đi theo phương thức Get
	/// </summary>
	/// <param name="url"></param>
	/// <param name="data"></param>
	/// <param name="byteData"></param>
	/// <param name="callback"></param>
	/// <param name="timeOut"></param>
	public static void HttpGet(string url, Dictionary<string, string> data, byte[] byteData, Action<UnityWebRequest> callback, float timeOut = 10f)
	{
		HttpInfo info = new HttpInfo();
		info.Callback = callback;
		info.URL = url;
		info.FormData = data;
		info.ByteData = byteData;
		info.Type = "GET";
		info.TimeOut = timeOut;
		SimpleHttpTask.NewInstance.StartHttp(info);
	}

	/// <summary>
	/// Tạo yêu cầu gửi đi theo phương thức Post
	/// </summary>
	/// <param name="url"></param>
	/// <param name="data"></param>
	/// <param name="byteData"></param>
	/// <param name="callback"></param>
	/// <param name="timeOut"></param>
	public static void HttpPost(string url, Dictionary<string, string> data, byte[] byteData, Action<UnityWebRequest> callback, float timeOut = 10f)
	{
		HttpInfo info = new HttpInfo();
		info.Callback = callback;
		info.URL = url;
		info.FormData = data;
		info.ByteData = byteData;
		info.Type = "POST";
		info.TimeOut = timeOut;
		SimpleHttpTask.NewInstance.StartHttp(info);
	}
	#endregion
}