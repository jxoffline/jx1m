using GameServer.Interface;
using GameServer.Logic;
using System.Collections.Generic;

namespace GameServer.KiemThe.Logic
{
    /// <summary>
    /// Quản lý người chơi
    /// </summary>
    public static partial class KTPlayerManager
    {
        #region Gửi gói tin
        /// <summary>
        /// Gửi gói tin đến tất cả người chơi xung quanh
        /// </summary>
        /// <param name="cmdID">ID gói tin</param>
        /// <param name="objsList">Danh sách người chơi sẽ nhận gói tin này</param>
        /// <param name="data">Dữ liệu</param>
        /// <param name="fromObject">Bắt nguồn từ đối tượng (sẽ dùng để so sánh người chơi trong danh sách được gửi gói tin có thể nhìn thấy đối tượng này không, nếu không nhìn thấy thì sẽ không gửi)</param>
        /// <param name="ignoreObject">Bỏ qua đối tượng</param>
        public static void SendPacketToPlayers<T>(int cmdID, List<KPlayer> objsList, T data, IObject fromObject, IObject ignoreObject)
        {
            /// Toác
            if (null == objsList)
            {
                return;
            }

            /// Duyệt danh sách người chơi
            for (int i = 0; i < objsList.Count; i++)
            {
                /// Nếu là đối tượng cần bỏ qua
                if (ignoreObject == objsList[i])
                {
                    continue;
                }

                /// Người chơi đang xét
                KPlayer client = objsList[i];
                /// Nếu đã Logout
                if (client.LogoutState)
                {
                    continue;
                }

                /// Nếu đối tượng bắt nguồn tồn tại
                if (fromObject != null)
                {
                    /// Nếu là đối tượng GameObject
                    if (fromObject is GameObject go)
                    {
                        /// Nếu không nhìn thấy
                        if (go.IsInvisible() && !go.VisibleTo(client))
                        {
                            /// Bỏ qua
                            continue;
                        }
                    }

                    /// Nếu là Pet
                    if (fromObject is Pet pet)
                    {
                        /// Nếu không nhìn thấy
                        if (!pet.VisibleTo(client))
                        {
                            /// Bỏ qua
                            continue;
                        }
                    }
                }

                /// Gửi gói tin đi
                client.SendPacket<T>(cmdID, data);
            }
        }

        /// <summary>
        /// Gửi gói tin đến tất cả người chơi xung quanh
        /// </summary>
        /// <param name="cmdID">ID gói tin</param>
        /// <param name="objsList">Danh sách người chơi sẽ nhận gói tin này</param>
        /// <param name="bytesData">Dữ liệu dạng byte</param>
        /// <param name="fromObject">Bắt nguồn từ đối tượng (sẽ dùng để so sánh người chơi trong danh sách được gửi gói tin có thể nhìn thấy đối tượng này không, nếu không nhìn thấy thì sẽ không gửi)</param>
        /// <param name="ignoreObject">Bỏ qua đối tượng</param>
        public static void SendPacketToPlayers(int cmdID, List<KPlayer> objsList, byte[] bytesData, IObject fromObject, IObject ignoreObject)
        {
            /// Toác
            if (null == objsList)
            {
                return;
            }

            /// Duyệt danh sách người chơi
            for (int i = 0; i < objsList.Count; i++)
            {
                /// Nếu là đối tượng cần bỏ qua
                if (ignoreObject == objsList[i])
                {
                    continue;
                }

                /// Người chơi đang xét
                KPlayer client = objsList[i];
                /// Nếu đã Logout
                if (client.LogoutState)
                {
                    continue;
                }

                /// Nếu đối tượng bắt nguồn tồn tại
                if (fromObject != null)
                {
                    /// Nếu là đối tượng GameObject
                    if (fromObject is GameObject go)
                    {
                        /// Nếu không nhìn thấy
                        if (go.IsInvisible() && !go.VisibleTo(client))
                        {
                            /// Bỏ qua
                            continue;
                        }
                    }

                    /// Nếu là Pet
                    if (fromObject is Pet pet)
                    {
                        /// Nếu không nhìn thấy
                        if (!pet.VisibleTo(client))
                        {
                            /// Bỏ qua
                            continue;
                        }
                    }
                }
                
                /// Gửi gói tin đi
                client.SendPacket(cmdID, bytesData);
            }
        }

        /// <summary>
        /// Gửi gói tin đến tất cả người chơi xung quanh, nếu nhìn thấy bản thân
        /// </summary>
        /// <param name="cmdID">ID gói tin</param>
        /// <param name="objsList">Danh sách người chơi sẽ nhận gói tin này</param>
        /// <param name="strCmd">Chuỗi thông tin</param>
        /// <param name="fromObject">Bắt nguồn từ đối tượng (sẽ dùng để so sánh người chơi trong danh sách được gửi gói tin có thể nhìn thấy đối tượng này không, nếu không nhìn thấy thì sẽ không gửi)</param>
        /// <param name="ignoreObject">Bỏ qua đối tượng</param>
        public static void SendPacketToPlayers(int cmdID, List<KPlayer> objsList, string strCmd, IObject fromObject, IObject ignoreObject)
        {
            /// Toác
            if (null == objsList)
            {
                return;
            }

            /// Duyệt danh sách người chơi
            for (int i = 0; i < objsList.Count; i++)
            {
                /// Nếu là đối tượng cần bỏ qua
                if (ignoreObject == objsList[i])
                {
                    continue;
                }

                /// Người chơi đang xét
                KPlayer client = objsList[i];
                /// Nếu đã Logout
                if (client.LogoutState)
                {
                    continue;
                }

                /// Nếu đối tượng bắt nguồn tồn tại
                if (fromObject != null)
                {
                    /// Nếu là đối tượng GameObject
                    if (fromObject is GameObject go)
                    {
                        /// Nếu không nhìn thấy
                        if (go.IsInvisible() && !go.VisibleTo(client))
                        {
                            /// Bỏ qua
                            continue;
                        }
                    }

                    /// Nếu là Pet
                    if (fromObject is Pet pet)
                    {
                        /// Nếu không nhìn thấy
                        if (!pet.VisibleTo(client))
                        {
                            /// Bỏ qua
                            continue;
                        }
                    }
                }

                /// Gửi gói tin đi
                client.SendPacket(cmdID, strCmd);
            }
        }
        #endregion
    }
}
