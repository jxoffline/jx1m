using System.Collections.Generic;
using System.Linq;
using System.Net;
using Server.TCP;

namespace GameServer.Logic
{
	/// <summary>
	/// Quản lý phiên đăng nhập
	/// </summary>
	public class UserSession
    {
        /// <summary>
        /// Danh sách Account theo kết nối
        /// </summary>
        private Dictionary<TMSKSocket, string> _S2UDict = new Dictionary<TMSKSocket, string>(1000);

        /// <summary>
        /// Danh sách kết nối theo Account
        /// </summary>
        private Dictionary<string, TMSKSocket> _U2SDict = new Dictionary<string, TMSKSocket>(1000);

        /// <summary>
        /// Danh sách tên nhân vật theo kết nối
        /// </summary>
        private Dictionary<TMSKSocket, string> _S2UNameDict = new Dictionary<TMSKSocket, string>(1000);

        /// <summary>
        /// Danh sách kết nối theo tên nhân vật
        /// </summary>
        private Dictionary<string, TMSKSocket> _UName2SDict = new Dictionary<string, TMSKSocket>(1000);

        /// <summary>
        /// Danh sách người chơi theo địa chỉ IP
        /// </summary>
        private Dictionary<string, List<KPlayer>> _ClientByIPAddress = new Dictionary<string, List<KPlayer>>(1000);

        /// <summary>
        /// Trả về danh sách Socket
        /// </summary>
        /// <returns></returns>
        public List<TMSKSocket> GetSocketList()
        {
            lock (this)
            {
                return _S2UDict.Keys.ToList<TMSKSocket>();
            }
        }
        /// <summary>
        /// Thêm phiên đăng nhập tương ứng
        /// </summary>
        /// <param name="clientSocket"></param>
        /// <param name="userID"></param>
        public bool AddSession(TMSKSocket clientSocket, string userID)
        {
            lock (this)
            {
                string oldUserID = "";
                /// Đã tồn tại
                if (_S2UDict.TryGetValue(clientSocket, out oldUserID))
                {
                    return false;
                }

                TMSKSocket oldClientSocket = null;
                /// Đã tồn tại
                if (_U2SDict.TryGetValue(userID, out oldClientSocket))
                {
                    return false;
                }

                _S2UDict[clientSocket] = userID;
                _U2SDict[userID] = clientSocket;
            }

            return true;
        }

        /// <summary>
        /// Xóa phiên đăng nhập tương ứng
        /// </summary>
        /// <param name="clientSocket"></param>
        public void RemoveSession(TMSKSocket clientSocket)
        {
            if (null == clientSocket) return;
            string userID = "";
            lock (this)
            {
                if (_S2UDict.TryGetValue(clientSocket, out userID))
                {
                    _S2UDict.Remove(clientSocket);
                    _U2SDict.Remove(userID);
                }
            }
        }

        /// <summary>
        /// Tìm Account theo socket
        /// </summary>
        /// <param name="clientSocket"></param>
        public string FindUserID(TMSKSocket clientSocket)
        {
            string userID = "";
            lock (this)
            {
                _S2UDict.TryGetValue(clientSocket, out userID);
            }

            return userID;
        }

        /// <summary>
        /// Tìm socket theo Account
        /// </summary>
        /// <param name="clientSocket"></param>
        public TMSKSocket FindSocketByUserID(string userID)
        {
            TMSKSocket clientSocket = null;
            lock (this)
            {
                _U2SDict.TryGetValue(userID, out clientSocket);
            }

            return clientSocket;
        }

        /// <summary>
        /// Thêm phiên đăng nhập với tên nhân vật tương ứng
        /// </summary>
        /// <param name="clientSocket"></param>
        /// <param name="userID"></param>
        public void AddUserName(TMSKSocket clientSocket, string userName)
        {
            lock (this)
            {
                _S2UNameDict[clientSocket] = userName;
                _UName2SDict[userName] = clientSocket;
            }
        }

        /// <summary>
        /// Xóa phiên đăng nhập tương ứng
        /// </summary>
        /// <param name="clientSocket"></param>
        public void RemoveUserName(TMSKSocket clientSocket)
        {
            if (null == clientSocket) return;
            lock (this)
            {
                string userName = null;
                if (_S2UNameDict.TryGetValue(clientSocket, out userName))
                {
                    _S2UNameDict.Remove(clientSocket);
                    _UName2SDict.Remove(userName);
                }
            }
        }

        /// <summary>
        /// Tìm tên nhân vật theo socket
        /// </summary>
        /// <param name="clientSocket"></param>
        public string FindUserName(TMSKSocket clientSocket)
        {
            string userName = null;
            lock (this)
            {
                _S2UNameDict.TryGetValue(clientSocket, out userName);
            }

            return userName;
        }

        /// <summary>
        /// Tìm socket theo tên nhân vật
        /// </summary>
        /// <param name="clientSocket"></param>
        public TMSKSocket FindSocketByUserName(string userName)
        {
            TMSKSocket clientSocket = null;
            lock (this)
            {
                _UName2SDict.TryGetValue(userName, out clientSocket);
            }

            return clientSocket;
        }

        /// <summary>
        /// Thêm người chơi vào danh sách theo IP
        /// </summary>
        /// <param name="client"></param>
        public void AddClientToIPAddressList(KPlayer client)
		{
            /// Thông tin kết nối
            IPEndPoint socketInfo = client.ClientSocket.RemoteEndPoint as IPEndPoint;
            /// Địa chỉ IP
            string ipAddress = socketInfo.Address.ToString();

            lock (this)
			{
                /// Nếu chưa tồn tại thì tạo mới
                if (!this._ClientByIPAddress.ContainsKey(ipAddress))
				{
                    this._ClientByIPAddress[ipAddress] = new List<KPlayer>();
				}
                this._ClientByIPAddress[ipAddress].Add(client);
			}
		}

        /// <summary>
        /// Xóa người chơi khỏi danh sách theo IP
        /// </summary>
        /// <param name="client"></param>
        public void RemoveClientFromIPAddressList(KPlayer client)
		{
            /// Thông tin kết nối
            IPEndPoint socketInfo = client.ClientSocket.RemoteEndPoint as IPEndPoint;
            /// Địa chỉ IP
            string ipAddress = socketInfo.Address.ToString();

            lock (this)
			{
                /// Nếu tồn tại
                if (this._ClientByIPAddress.TryGetValue(ipAddress, out List<KPlayer> players))
				{
                    players.Remove(client);
				}
			}
        }

        /// <summary>
        /// Trả về số lượng kết nối theo IP tương ứng
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <returns></returns>
        public int GetClientCountsByIPAddress(string ipAddress)
		{
            lock (this)
			{
                if (this._ClientByIPAddress.TryGetValue(ipAddress, out List<KPlayer> players))
                {
                    return players.Count;
                }
            }
            
            return 0;
		}
    }
}
