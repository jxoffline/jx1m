namespace Server.Data
{
    using ProtoBuf;
    using System;

    [ProtoContract]
    public class UserMiniData
    {
        /// <summary>
        /// UserID
        /// </summary>
        [ProtoMember(1)]
        public string UserId;

        /// <summary>
        /// LastRoleID Login
        /// </summary>
        [ProtoMember(2)]
        public int LastRoleId;

        /// <summary>
        /// Tiền Web
        /// </summary>
        [ProtoMember(3)]
        public int RealMoney;

        /// <summary>
        /// Thời gian tọa ROLE
        /// </summary>
        [ProtoMember(4)]
        public DateTime MinCreateRoleTime;

        /// <summary>
        /// Thời gian đăng nhập gần đây
        /// </summary>
        [ProtoMember(5)]
        public DateTime LastLoginTime;

        /// <summary>
        /// THời gian đăng xuất gần đây
        /// </summary>
        [ProtoMember(6)]
        public DateTime LastLogoutTime;

        /// <summary>
        /// Thời gian tọa nhân vật
        /// </summary>
        [ProtoMember(7)]
        public DateTime RoleCreateTime;

        /// <summary>
        /// Thời gian nhân vật đăng nhập gần đây
        /// </summary>
        [ProtoMember(8)]
        public DateTime RoleLastLoginTime;

        /// Thời gian nhân vật đăng nhập gần đây
        [ProtoMember(9)]
        public DateTime RoleLastLogoutTime;

        /// Cấp tối đa
        [ProtoMember(10)]
        public int MaxLevel;

    }
}