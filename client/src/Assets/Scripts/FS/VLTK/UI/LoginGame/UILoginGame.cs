using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using FS.GameFramework.Logic;
using FS.GameEngine.Logic;
using Server.Tools;
using FS.GameEngine.Network;
using Server.Data;
using UnityEngine.Networking;
using FS.GameEngine.SDK.ProtoModel;
using System;
using FS.VLTK.Loader;

namespace FS.VLTK.UI.LoginGame
{
    /// <summary>
    /// Màn hình đăng ký đăng nhập
    /// </summary>
    public class UILoginGame : MonoBehaviour
    {
        /// <summary>
        /// Sự kiện khi quá trình Login hoàn tất
        /// </summary>
        public Action LoginSuccess { get; set; } = null;

        #region Define
        #region Tab Login
        /// <summary>
        /// Ô nhập UserName Tab Login
        /// </summary>
        [SerializeField]
        private TMP_InputField Input_LoginUserName;

        /// <summary>
        /// Ô nhập Password Tab Login
        /// </summary>
        [SerializeField]
        private TMP_InputField Input_LoginPassword;

        /// <summary>
        /// Toggle lưu lại thông tin tài khoản mật khẩu
        /// </summary>
        [SerializeField]
        private Toggle UIToggle_SaveAccountInfo;

        /// <summary>
        /// Nút đăng nhập
        /// </summary>
        [SerializeField]
        private Button Button_Login;
        #endregion

        #region Tab Register
        /// <summary>
        /// Ô nhập UserName Tab Register
        /// </summary>
        [SerializeField]
        private TMP_InputField Input_RegisterUserName;

        /// <summary>
        /// Ô nhập Password Tab Register
        /// </summary>
        [SerializeField]
        private TMP_InputField Input_RegisterPassword;

        /// <summary>
        /// Ô nhập lại mật khẩu Tab Register
        /// </summary>
        [SerializeField]
        private TMP_InputField Input_RegisterRepassword;

        /// <summary>
        /// Nút đăng ký
        /// </summary>
        [SerializeField]
        private Button Button_Register;
        #endregion
        #endregion

        #region Private fields
        /// <summary>
        /// Lưu thiết lập tài khoản mật khẩu
        /// </summary>
        private LoadSavedAccountInfo accountInfoManager = null;
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi khi đối tượng được tạo ra
        /// </summary>
        private void Awake()
		{
            this.accountInfoManager = this.gameObject.AddComponent<LoadSavedAccountInfo>();
            this.accountInfoManager.Done = (account, password) => {
                this.Input_LoginUserName.text = account;
                this.Input_LoginPassword.text = password;
            };
		}

		/// <summary>
		/// Hàm này gọi đến ở Frame đầu tiên
		/// </summary>
		private void Start()
        {
            this.InitPrefabs();
            /// Tải dữ liệu tài khoản và mật khẩu cũ
            this.StartCoroutine(this.accountInfoManager.ReadData());
        }
        #endregion

        #region Code UI
        /// <summary>
        /// Thiết lập ban đầu
        /// </summary>
        private void InitPrefabs()
        {
            this.Button_Login.onClick.AddListener(this.ButtonLogin_Click);
            this.Button_Register.onClick.AddListener(this.ButtonRegister_Click);
        }

        /// <summary>
        /// Sự kiện khi nút Login được ấn
        /// </summary>
        private void ButtonLogin_Click()
        {
            Super.ShowNetWaiting("Đang kiểm tra thông tin tài khoản, xin đợi giây lát...");

            string account = this.Input_LoginUserName.text;
            string password = this.Input_LoginPassword.text;
            this.LoginCallBack = () => {
                this.LoginSuccess?.Invoke();
            };
            this.RequestLogin(account, password);
        }

        /// <summary>
        /// Sự kiện khi nút Register được ấn
        /// </summary>
        private void ButtonRegister_Click()
        {
            Super.ShowNetWaiting("Đang đăng ký tài khoản, xin đợi giây lát...");

            string account = this.Input_RegisterUserName.text;
            string password = this.Input_RegisterPassword.text;
            string repassword = this.Input_RegisterRepassword.text;

            /// Nếu mật khẩu nhập vào khác nhau
            if (password != repassword)
			{
                Super.ShowMessageBox("Lỗi đăng ký", "Mật khẩu nhập vào không khớp!", true);
                return;
			}

            this.RegisterCallBack = () => {
                Super.ShowMessageBox("Đăng ký thành công", "Đăng ký tài khoản thành công.", true);
            };
            this.RequestRegister(account, password, "unknow@email.com", "01234567891");
        }
        #endregion

        #region SDK
        #region SDK đăng ký
        /// <summary>
        /// Sự kiện trả về khi quá trình hoàn tất
        /// </summary>
        private Action RegisterCallBack = null;

        /// <summary>
        /// Gửi yêu cầu đăng ký tài khoản vào hệ thống
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="email"></param>
        /// <param name="phoneNumber"></param>
        private void RequestRegister(string username, string password, string email, string phoneNumber)
        {
            this.StartCoroutine(this.AccountRegister(username, password, email, phoneNumber));
        }

        /// <summary>
        /// Thực hiện quá trình đăng ký tài khoản
        /// </summary>
        /// <param name="UserName"></param>
        /// <param name="Password"></param>
        /// <param name="Email"></param>
        /// <param name="PhoneNumber"></param>
        /// <returns></returns>
        private IEnumerator AccountRegister(string UserName, string Password, string Email, string PhoneNumber)
        {
            string url = "http://localhost:81/RegisterSDK.aspx"; // MainGame.GameInfo.RegisterAccountSDK;

            WWWForm wwwForm = new WWWForm();

            wwwForm.AddField("UserName", UserName);
            wwwForm.AddField("Password", Password);
            wwwForm.AddField("Email", Email);
            wwwForm.AddField("PhoneNumber", PhoneNumber);

            UnityWebRequest www = UnityWebRequest.Post(url, wwwForm);
            yield return www.SendWebRequest();

            if (!string.IsNullOrEmpty(www.error))
            {
                Super.HideNetWaiting();
                Super.ShowMessageBox("Lỗi đăng ký", "Đăng ký thất bại, vui lòng kiểm tra lại mạng.", true);
                yield break;
            }

            RegisterRep RegisterRep = DataHelper.BytesToObject<RegisterRep>(www.downloadHandler.data, 0, www.downloadHandler.data.Length);
            if (RegisterRep == null)
            {
                Super.HideNetWaiting();
                Super.ShowMessageBox("Lỗi đăng ký", "Đăng ký thất bại, vui lòng thử lại sau.", true);
                yield break;
            }
            else
            {
                if (RegisterRep.ErrorCode != 0)
                {
                    Super.HideNetWaiting();
                    Super.ShowMessageBox("Lỗi đăng ký", RegisterRep.ErorrMsg, true);
                }
                else
                {
                    Super.HideNetWaiting();
                    this.RegisterCallBack?.Invoke();
                    Super.ShowMessageBox("Đăng ký", "Đăng ký thành công!", true);
                }

            }


            www.Dispose();
            www = null;
        }
        #endregion

        #region SDK đăng nhập
        /// <summary>
        /// Thiết lập sự kiện khi có phản hồi
        /// </summary>
        private Action LoginCallBack = null;

        /// <summary>
        /// Gửi yêu cầu đăng nhập
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        private void RequestLogin(string username, string password)
        {
            this.StartCoroutine(this.DoLogin(username, password, 0));
        }

        /// <summary>
        /// Thực thi đăng nhập hệ thống
        /// </summary>
        /// <param name="UserName"></param>
        /// <param name="Password"></param>
        /// <param name="LoginType"></param>
        /// <returns></returns>
        private IEnumerator DoLogin(string UserName, string Password, int LoginType)
        {
            string url = MainGame.GameInfo.LoginAccountSDK;

            WWWForm wwwForm = new WWWForm();

            wwwForm.AddField("UserName", UserName);
            wwwForm.AddField("Password", Password);
            wwwForm.AddField("LoginType", LoginType);

            UnityWebRequest www = UnityWebRequest.Post(url, wwwForm);
            yield return www.SendWebRequest();

            if (!string.IsNullOrEmpty(www.error))
            {
                Super.HideNetWaiting();
                Super.ShowMessageBox("Lỗi kết nối", "Không thể đăng nhập. Hãy kiểm tra kết nối mạng.", true);
                yield break;
            }

            LoginRep LoginRep = DataHelper.BytesToObject<LoginRep>(www.downloadHandler.data, 0, www.downloadHandler.data.Length);

            if (LoginRep == null)
            {
                Super.HideNetWaiting();
                Super.ShowMessageBox("Lỗi đăng nhập", "Đăng nhập thất bại, vui lòng thử lại sau.", true);
                yield break;
            }
            else
            {
                if (LoginRep.ErrorCode != 0)
                {
                    Super.HideNetWaiting();
                    Super.ShowMessageBox("Lỗi đăng nhập", LoginRep.ErorrMsg, true);
                }
                else
                {
                    SDKSession.AccessToken = LoginRep.AccessToken;
                    GameInstance.Game.CurrentSession.UserToken = LoginRep.AccessToken;
                    GameInstance.Game.CurrentSession.LastLoginIP = LoginRep.LastLoginIP;
                    GameInstance.Game.CurrentSession.LastLoginTime = LoginRep.LastLoginTime;

                    this.AccountVerify();
                }

            }

            www.Dispose();
        }

        /// <summary>
        /// Thực hiện kiểm tra thông tin đăng nhập
        /// </summary>
        private void AccountVerify()
        {
            ClientVerifySIDData requestData = new ClientVerifySIDData();
            requestData.lTime = KTGlobal.GetTimeStamp();
            requestData.strMD5 = MD5Helper.get_md5_string(Consts.HTTP_MD5_KEY + requestData.strSID + requestData.lTime);
            requestData.strSID = GameInstance.Game.CurrentSession.UserToken;

            SimpleHttpTask.HttpPost(MainGame.GameInfo.VerifyAccountSDK, null, DataHelper.ObjectToBytes(requestData), (request) =>
            {
                this.GetPlatformUIDCallback(request);
            }, 10f);
        }

        /// <summary>
        /// Sau khi kiểm tra có kết quả, thực thi hàm này
        /// </summary>
        /// <param name="request"></param>
        private void GetPlatformUIDCallback(UnityWebRequest request)
        {
            if (request != null && string.IsNullOrEmpty(request.error))
            {
                byte[] returnBytes = request.downloadHandler.data;

                if (returnBytes != null && returnBytes.Length > 0)
                {
                    ServerVerifySIDData responseData = DataHelper.BytesToObject<ServerVerifySIDData>(returnBytes, 0, returnBytes.Length);
                    if (responseData != null)
                    {
                        string strUID = responseData.strPlatformUserID;
                        if ("-1" == strUID)
                        {
                            Super.HideNetWaiting();
                            Super.ShowMessageBox("Lỗi đăng nhập", "Sai tài khoản hoặc mật khẩu.", true);
                        }
                        else if ("-2" == strUID)
                        {
                            Super.HideNetWaiting();
                            Super.ShowMessageBox("Lỗi đăng nhập", "Phiên đăng nhập đã hết hạn, hãy thử đăng nhập lại.", true);
                        }
                        else if ("-10" == strUID)
                        {
                            Super.HideNetWaiting();
                            Super.ShowMessageBox("Lỗi đăng nhập", "Sai cấu trúc dữ liệu.", true);
                        }
                        else if (strUID.StartsWith("-3"))
                        {
                            string[] strArr = strUID.Split(':');
                            int second = 0;
                            if (strArr != null && strArr.Length >= 2)
                            {
                                int.TryParse(strArr[1], out second);
                            }
                            Super.HideNetWaiting();
                            Super.ShowMessageBox("Lỗi đăng nhập", "Tài khoản hiện bị khóa đăng nhập, hãy thử lại sau " + second + " giây nữa.", true);

                        }
                        else if ("-4" == strUID)
                        {
                            Super.HideNetWaiting();
                            Super.ShowMessageBox("Lỗi đăng nhập", "Tài khoản hiện đang bị khóa, vui lòng liên hệ bộ phận hỗ trợ để được giúp đỡ.", true);
                        }
                        else
                        {
                            Global.RootParams["uid"] = responseData.strPlatformUserID;
                            Global.RootParams["n"] = responseData.strAccountName;
                            Global.RootParams["t"] = "" + responseData.lTime;
                            Global.RootParams["cm"] = responseData.strCM;
                            Global.RootParams["token"] = responseData.strToken;

                            GameInstance.Game.CurrentSession.UserID = responseData.strPlatformUserID;
                            GameInstance.Game.CurrentSession.UserName = responseData.strAccountName;
                            GameInstance.Game.CurrentSession.TimeActive = responseData.lTime;
                            GameInstance.Game.CurrentSession.Cm = responseData.strCM;
                            GameInstance.Game.CurrentSession.TokenGS = responseData.strToken;

                            /// Nếu có thiết lập lưu lại tài khoản và mật khẩu
                            if (this.UIToggle_SaveAccountInfo.isOn)
							{
                                /// Lưu lại tài khoản và mật khẩu
                                this.accountInfoManager.SaveData(this.Input_LoginUserName.text, this.Input_LoginPassword.text);
							}

                            this.LoginCallBack?.Invoke();
                        }
                    }
                }
                else
                {
                    Super.ShowMessageBox("Lỗi đăng nhập", "Server đang bảo trì, vui lòng thử lại sau.", true);
                }
            }
			else if (request != null)
			{
                /// Hủy đối tượng
                request.downloadHandler?.Dispose();
                request.Dispose();
            }
        }
        #endregion
        #endregion
    }
}

