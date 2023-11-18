using FS.GameEngine.Network;
using Tmsk.Contract;


public class KuaFuLoginManager
{
    public static KuaFuServerLoginData KuaFuServerLoginData = null;

    /// <summary>
    /// Thông tin Login máy chủ liên Server
    /// </summary>
    public static KuaFuServerLoginData KuaFuServerLoginDataKuaFu = null;

    public static KuaFuServerLoginData KuaFuServerLoginDataOriginal = new KuaFuServerLoginData();

    #region 跨服逻辑

    /// <summary>
    /// 平台登录成功后,更新跨服登录信息里的token
    /// </summary>
    /// <param name="verSign"></param>
    /// <param name="userID"></param>
    /// <param name="userName"></param>
    /// <param name="lastTime"></param>
    /// <param name="isadult"></param>
    /// <param name="signCode"></param>
    public static void UpdateWebToken(int verSign, string userID, string userName, string lastTime, string isadult, string signCode)
    {
        WebLoginToken webLoginToken = new WebLoginToken()
        {
            VerSign = verSign,
            UserID = userID,
            UserName = userName,
            LastTime = lastTime,
            Isadult = isadult,
            SignCode = signCode,
        };

        KuaFuServerLoginDataOriginal.WebLoginToken = webLoginToken;

        if (null != KuaFuServerLoginData)
        {
            KuaFuServerLoginData.WebLoginToken = webLoginToken;
        }
    }

    public static bool LoginKuaFuServer(out string ip, out int port)
    {
        ip = "";
        port = 0;
        if (KuaFuServerLoginData == KuaFuServerLoginDataKuaFu && KuaFuServerLoginDataKuaFu != null && KuaFuServerLoginDataKuaFu.RoleId > 0)
        {
            ip = KuaFuServerLoginData.ServerIp;
            port = KuaFuServerLoginData.ServerPort;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Chuyển người chơi sang máy chủ liên Server
    /// </summary>
    /// <param name="kuaFuServerLoginData"></param>
    /// <returns></returns>
    public static bool ChangeToKuaFuServer(KuaFuServerLoginData kuaFuServerLoginData)
    {
        if (null != kuaFuServerLoginData)
        {
            KuaFuServerLoginDataKuaFu = kuaFuServerLoginData;
            KuaFuServerLoginData = KuaFuServerLoginDataKuaFu;

            //记录当前角色
            KuaFuServerLoginDataOriginal.RoleId = GameInstance.Game.CurrentSession.roleData.RoleID;

            //判断是否需要切换服务器
            if (KuaFuServerLoginDataKuaFu.ServerId != KuaFuServerLoginDataOriginal.ServerId)
            {
                return true;
            }
        }

        return false;
    }

    public static void ChangeToOriginalServer()
    {
        KuaFuServerLoginData = KuaFuServerLoginDataOriginal;
    }

    public static void OnChangeServerComplete()
    {
        if (KuaFuServerLoginData == KuaFuServerLoginDataOriginal)
        {
            //切换回区服务器
            //清除临时信息
            KuaFuServerLoginDataOriginal.RoleId = 0;
            KuaFuServerLoginDataKuaFu = null;
        }
        else
        {
            //切换到跨服服务器

        }
    }
    /// <summary>
    /// 清楚跨服信息
    /// </summary>
    public static void ClearLoginInfo()
    {
        KuaFuServerLoginDataOriginal.RoleId = 0;
        KuaFuServerLoginDataKuaFu = null;
        KuaFuServerLoginData = null;
    }

    public static bool DirectLogin()
    {
        if (null != KuaFuServerLoginData && KuaFuServerLoginData.RoleId > 0)
        {
            GameInstance.Game.CurrentSession.RoleID = KuaFuServerLoginData.RoleId;
            return true;
        }

        return false;
    }
    public static bool IsKuaFuLoginMode1(ref string uid, ref string name, ref string lastTime, ref string isadult, ref string token)
    {
        if (KuaFuServerLoginData == KuaFuServerLoginDataKuaFu && null != KuaFuServerLoginDataKuaFu && KuaFuServerLoginDataKuaFu.RoleId > 0)
        {
            uid = KuaFuServerLoginData.WebLoginToken.UserID;
            name = KuaFuServerLoginData.WebLoginToken.UserName;
            lastTime = KuaFuServerLoginData.WebLoginToken.LastTime;
            isadult = KuaFuServerLoginData.WebLoginToken.Isadult;
            token = KuaFuServerLoginData.WebLoginToken.SignCode;
            return true;
        }

        return false;
    }


    public static string GetKuaFuLoginString(string normal)
    {
        string loginStr = normal;

        if (KuaFuServerLoginData == KuaFuServerLoginDataKuaFu && null != KuaFuServerLoginDataKuaFu && KuaFuServerLoginDataKuaFu.RoleId > 0)
        {
            //跨服登录
            loginStr = string.Format("{0}:{1}:{2}:{3}:{4}:{5}:{6}", normal, KuaFuServerLoginData.RoleId, KuaFuServerLoginData.GameId, KuaFuServerLoginData.GameType, 
                KuaFuServerLoginData.ServerId, KuaFuServerLoginData.ServerIp, KuaFuServerLoginData.ServerPort);
        }
        else
        {
            loginStr = string.Format("{0}:{1}:{2}:{3}:{4}:{5}:{6}", normal, 0, 0, 0,
                0, "", 0);
        }

        return loginStr;
    }

    #endregion 跨服逻辑
}

