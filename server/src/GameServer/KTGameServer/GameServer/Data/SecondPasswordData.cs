using ProtoBuf;
using Server.Tools;
using System;
using System.Text;

namespace Server.Data
{
    /// <summary>
    /// 二级密码使用对称加密协议RC4, 加密后的字符串可能含有:，因此不能使用:分割协议
    /// 这里使用protobuf序列化
    /// </summary>
    public static class SecondPasswordRC4
    {
        private static string _Key = "SecPwd";

        // 加密
        public static string Encrypt(string input)
        {
            if (string.IsNullOrEmpty(input)) return null;

            byte[] b = new UTF8Encoding().GetBytes(input);
            RC4Helper.RC4(b, _Key);
            return Convert.ToBase64String(b);
        }

        //解密
        public static string Decrypt(string input)
        {
            if (string.IsNullOrEmpty(input)) return null;

            byte[] b = Convert.FromBase64String(input);
            RC4Helper.RC4(b, _Key);
            return new UTF8Encoding().GetString(b);
        }
    }

    /// <summary>
    /// Client--->Server, 客户端请求验证用户密码
    /// Example:
    /// VerifySecondPassword VerifyPwd = new VerifySecondPassword() {
    ///     RoleID = 110,
    ///     SecPwd = SecondPasswordRC4.Encrypt("加密前的二级密码");
    /// };
    /// </summary>
    [ProtoContract]
    public class VerifySecondPassword
    {
        // 用户ID
        [ProtoMember(1)]
        public string UserID;

        // RC4加密后的二级密码
        [ProtoMember(2)]
        public string SecPwd;
    }

    /// <summary>
    /// Client--->Server, 客户端设置密码
    /// </summary>
    [ProtoContract]
    public class SetSecondPassword
    {
        // 角色ID， 注意：虽然是以角色的身份设置的
        // 但是其实对角色对应账号下的所有角色都生效
        [ProtoMember(1)]
        public int RoleID;

        // RC4加密后的二级密码, 如果没有旧密码，此项不写
        [ProtoMember(2)]
        public string OldSecPwd;

        // RC4加密后的二级密码
        [ProtoMember(3)]
        public string NewSecPwd;
    }
}