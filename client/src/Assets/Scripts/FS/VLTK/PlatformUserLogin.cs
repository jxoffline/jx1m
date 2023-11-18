using FS.GameEngine.Logic;
using HSGameEngine.GameEngine.Network.Tools;
using FS.GameFramework.Logic;
using Server.Data;
using Server.Tools;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace FS.VLTK
{
    public static class PlatformUserLogin
    {
        /// <summary>
        /// Máy chủ được chọn đăng nhập lần trước
        /// </summary>
        public static int RecordSelectServerID { get; set; }

        /// <summary>
        /// Ghi nhận gói tin Login tới Server
        /// </summary>
        /// <param name="infoVo"></param>
        public static void RecordLoginServerIDs(BuffServerInfo infoVo)
        {
            string strUID = Global.StringReplaceAll(Global.GetRootParam("uid", ""), ":", "");
            if ("-1" == strUID)
            {
                KTDebug.LogError("strUID = -1");
            }

            if (string.IsNullOrEmpty(strUID) || strUID == "-1")
            {
                strUID = "LastServerInfoID";
            }

            string serverIDs = PlayerPrefs.GetString(strUID);
            if (string.IsNullOrEmpty(serverIDs))
            {
                int recordServerID = PlayerPrefs.GetInt("LastServerInfoID");
                if (recordServerID > 0)
                {
                    serverIDs = recordServerID.ToString();
                }
            }

            bool isSend = false;
            if (string.IsNullOrEmpty(serverIDs) || PlayerPrefs.GetInt("IsSendedToServer") != 1)
            {
                isSend = true;
            }
            else
            {
                string[] ids = serverIDs.Split(',');
                if (ids.Length > 0 && ids[0] != infoVo.nServerID.ToString())
                {
                    isSend = true;
                }
            }
            if (isSend)
            {
                string url = MainGame.GameInfo.ServerListURL + "WriteUserLogInServerId.aspx";
                ClientServerListDataEx clientListData = new ClientServerListDataEx();
                clientListData.Time = TimeManager.GetCorrectLocalTime();
                clientListData.Md5 = MD5Helper.get_md5_string("" + clientListData.Time.ToString());
                clientListData.ServerId = infoVo.nServerID;
                clientListData.UserId = strUID;
                //KTDebug.Log("WriteUserLogInServerId UserID = " + strUID + "__ServerID=" + infoVo.nServerID);
                byte[] clientBytes = DataHelper.ObjectToBytes<ClientServerListDataEx>(clientListData);
                //WWW www = new WWW(url, clientBytes);
                UnityWebRequest request = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST);
                UploadHandlerRaw MyUploadHandler = new UploadHandlerRaw(clientBytes);
                MyUploadHandler.contentType = "application/x-www-form-urlencoded"; // might work with 'multipart/form-data'
                request.uploadHandler = MyUploadHandler;
                request.SendWebRequest();

                PlayerPrefs.SetInt("IsSendedToServer", 1);
            }

            string serverIDsOrder = GetOrderServerIDs(serverIDs, infoVo.nServerID);
            PlayerPrefs.SetString(strUID, serverIDsOrder);
            PlayerPrefs.SetString("LastServerInfoID", serverIDsOrder);
            PlayerPrefs.SetInt("NewLastServerInfoID", infoVo.nServerID); //当前最新的登陆服务器ID

        }

        //根据传入的serverIDs数组，然后重新排序整合。
        private static string GetOrderServerIDs(string serverIDs, int curServerID)
        {
            string result = "";
            string[] strArray = serverIDs.Split(',');
            ArrayList arrList = new ArrayList();
            arrList.AddRange(strArray);
            int index = arrList.IndexOf(curServerID.ToString());
            if (index >= 0)
            {
                arrList.RemoveAt(index);
            }
            arrList.Insert(0, curServerID);

            int count = arrList.Count;
            if (count > 5)  //记录的数量，最多为5个
            {
                count = 5;
            }
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < count; i++)
            {
                builder.Append(arrList[i]);
                if (i != (count - 1))  //最后一位不插入‘，’
                {
                    builder.Append(',');
                }
            }
            result = builder.ToString();
            return result;
        }
    }
}
