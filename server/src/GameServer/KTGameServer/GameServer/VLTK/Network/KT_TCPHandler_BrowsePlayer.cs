using GameServer.KiemThe.Core.Item;
using GameServer.KiemThe.Utilities;
using GameServer.Logic;
using GameServer.Server;
using Server.Data;
using Server.Protocol;
using Server.TCP;
using Server.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.KiemThe.Logic
{
    /// <summary>
    /// Quản lý tìm kiếm người chơi người chơi
    /// </summary>
    public static partial class KT_TCPHandler
    {
        #region Tìm người chơi
        /// <summary>
        /// Phản hồi yêu cầu tìm kiếm người chơi
        /// </summary>
        /// <param name="tcpMgr"></param>
        /// <param name="socket"></param>
        /// <param name="tcpClientPool"></param>
        /// <param name="tcpRandKey"></param>
        /// <param name="pool"></param>
        /// <param name="nID"></param>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <param name="tcpOutPacket"></param>
        /// <returns></returns>
        public static TCPProcessCmdResults ResponseBrowsePlayers(TCPManager tcpMgr, TMSKSocket socket, TCPClientPool tcpClientPool, TCPRandKey tcpRandKey, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;

            string cmdData = "";
            try
            {
                /// Giải mã gói tin đẩy về dạng string
                cmdData = new ASCIIEncoding().GetString(data);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Error while getting DATA, CMD={0}, Client={1}", (TCPGameServerCmds) nID, Global.GetSocketRemoteEndPoint(socket)));
                return TCPProcessCmdResults.RESULT_FAILED;
            }

            try
            {
                string[] fields = cmdData.Split(':');
                if (fields.Length != 1)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Error Socket params count not fit CMD={0}, Client={1}, Recv={2}", (TCPGameServerCmds) nID, Global.GetSocketRemoteEndPoint(socket), cmdData.Length));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// Chuỗi tên tương ứng cần tìm
                string playerName = fields[0].ToLower().Trim();

                /// Tìm chủ nhân của gói tin tương ứng
                KPlayer client = KTPlayerManager.Find(socket);
                if (null == client)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Không tìm thấy thông tin người chơi, CMD={0}, Client={1}", (TCPGameServerCmds) nID, Global.GetSocketRemoteEndPoint(socket)));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// Nếu tên không hợp lệ
                if (!KTFormValidation.IsValidString(playerName))
                {
                    KTPlayerManager.ShowNotification(client, "Tên người chơi chứa ký tự đặc biệt!");
                    return TCPProcessCmdResults.RESULT_OK;
                }

                /// Nếu chưa đến thời gian tìm kiếm
                if (KTGlobal.GetCurrentTimeMilis() - client.LastBrowsePlayersTick < 5000)
                {
                    KTPlayerManager.ShowNotification(client, "Chỉ được phép tìm kiếm người chơi mỗi 5 giây!");
                    return TCPProcessCmdResults.RESULT_OK;
                }
                /// Đánh dấu lần tìm kiếm cuối
                client.LastBrowsePlayersTick = KTGlobal.GetCurrentTimeMilis();

                /// Tìm danh sách người chơi tương ứng
                List<KPlayer> players = KTPlayerManager.FindAll(x => x.GetRoleData().RoleName.ToLower().Contains(playerName));
                /// Tạo danh sách trả về
                List<RoleDataMini> rds = new List<RoleDataMini>();
                /// Duyệt danh sách người chơi thêm vào danh sách Mini
                foreach (KPlayer player in players)
                {
                    rds.Add(new RoleDataMini()
                    {
                        RoleID = player.RoleID,
                        RoleName = player.RoleName,
                        FactionID = player.m_cPlayerFaction.GetFactionId(),
                        Level = player.m_Level,
                        TeamID = player.TeamID,
                        AvartaID = player.RolePic,
                        GuildName = player.GuildName,
                        /// Dấu thông tin
                        MapCode = -1,
                        PosX = -1,
                        PosY = -1,
                    });
                }

                /// Gửi gói tin lại cho người chơi
                byte[] _cmdData = DataHelper.ObjectToBytes<List<RoleDataMini>>(rds);
                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, _cmdData, nID);

                return TCPProcessCmdResults.RESULT_DATA;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, Global.GetDebugHelperInfo(socket), false);
            }

            return TCPProcessCmdResults.RESULT_FAILED;
        }
        #endregion

        #region Kiểm tra vị trí

        /// <summary>
        /// Phản hồi yêu cầu tìm kiếm người chơi
        /// </summary>
        /// <param name="tcpMgr"></param>
        /// <param name="socket"></param>
        /// <param name="tcpClientPool"></param>
        /// <param name="tcpRandKey"></param>
        /// <param name="pool"></param>
        /// <param name="nID"></param>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <param name="tcpOutPacket"></param>
        /// <returns></returns>
        public static TCPProcessCmdResults ResponseCheckPlayerLocation(TCPManager tcpMgr, TMSKSocket socket, TCPClientPool tcpClientPool, TCPRandKey tcpRandKey, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;

            string cmdData = "";
            try
            {
                /// Giải mã gói tin đẩy về dạng string
                cmdData = new ASCIIEncoding().GetString(data);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Error while getting DATA, CMD={0}, Client={1}", (TCPGameServerCmds) nID, Global.GetSocketRemoteEndPoint(socket)));
                return TCPProcessCmdResults.RESULT_FAILED;
            }

            try
            {
                string[] fields = cmdData.Split(':');
                if (fields.Length != 1)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Error Socket params count not fit CMD={0}, Client={1}, Recv={2}", (TCPGameServerCmds) nID, Global.GetSocketRemoteEndPoint(socket), cmdData.Length));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// ID người chơi
                int roleID = int.Parse(fields[0]);

                /// Tìm chủ nhân của gói tin tương ứng
                KPlayer client = KTPlayerManager.Find(socket);
                if (null == client)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Không tìm thấy thông tin người chơi, CMD={0}, Client={1}", (TCPGameServerCmds) nID, Global.GetSocketRemoteEndPoint(socket)));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// Người chơi tương ứng
                KPlayer player = KTPlayerManager.Find(roleID);
                /// Nếu không tìm thấy
                if (player == null)
                {
                    KTPlayerManager.ShowNotification(client, "Người chơi không tồn tại hoặc đã rời mạng!");
                    return TCPProcessCmdResults.RESULT_OK;
                }

                /// Kiểm tra có vật phẩm Quan Thiên Quyển không
                GoodsData itemGD = client.GoodsData.Find(x => x.Site == 0 && KTGlobal.CheckPositionScroll.Contains(x.GoodsID));
                if (itemGD == null)
				{
                    KTPlayerManager.ShowNotification(client, "Cần vật phẩm [Quan Thiên Quyển] mới có thể sử dụng chức năng này!");
                    return TCPProcessCmdResults.RESULT_OK;
                }

                /// Xóa vật phẩm
                if (!ItemManager.RemoveItemByCount(client, itemGD, 1,"CHECKLOCALTION"))
				{
                    KTPlayerManager.ShowNotification(client, "Cần vật phẩm [Quan Thiên Quyển] mới có thể sử dụng chức năng này!");
                    return TCPProcessCmdResults.RESULT_OK;
                }

                /// Tạo gói tin
                RoleDataMini rd = new RoleDataMini()
                {
                    RoleID = player.RoleID,
                    RoleName = player.RoleName,
                    FactionID = player.m_cPlayerFaction.GetFactionId(),
                    Level = player.m_Level,
                    TeamID = player.TeamID,
                    AvartaID = player.RolePic,
                    GuildName = player.GuildName,
                    FamilyName = player.FamilyName,
                    /// Vị trí
                    MapCode = player.CurrentCopyMapID >= 0 ? -1 : player.MapCode,
                    PosX = player.CurrentCopyMapID >= 0 ? -1 : (int) player.CurrentGrid.X,
                    PosY = player.CurrentCopyMapID >= 0 ? -1 : (int) player.CurrentGrid.Y,
                };
                /// Gửi gói tin lại cho người chơi
                byte[] _cmdData = DataHelper.ObjectToBytes<RoleDataMini>(rd);
                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, _cmdData, nID);

                return TCPProcessCmdResults.RESULT_DATA;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, Global.GetDebugHelperInfo(socket), false);
            }

            return TCPProcessCmdResults.RESULT_FAILED;
        }
        #endregion

        #region Kiểm tra thông tin

        /// <summary>
        /// Phản hồi yêu cầu tìm kiếm người chơi
        /// </summary>
        /// <param name="tcpMgr"></param>
        /// <param name="socket"></param>
        /// <param name="tcpClientPool"></param>
        /// <param name="tcpRandKey"></param>
        /// <param name="pool"></param>
        /// <param name="nID"></param>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <param name="tcpOutPacket"></param>
        /// <returns></returns>
        public static TCPProcessCmdResults ResponseCheckPlayerInfo(TCPManager tcpMgr, TMSKSocket socket, TCPClientPool tcpClientPool, TCPRandKey tcpRandKey, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;

            string cmdData = "";
            try
            {
                /// Giải mã gói tin đẩy về dạng string
                cmdData = new ASCIIEncoding().GetString(data);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Error while getting DATA, CMD={0}, Client={1}", (TCPGameServerCmds) nID, Global.GetSocketRemoteEndPoint(socket)));
                return TCPProcessCmdResults.RESULT_FAILED;
            }

            try
            {
                string[] fields = cmdData.Split(':');
                if (fields.Length != 1)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Error Socket params count not fit CMD={0}, Client={1}, Recv={2}", (TCPGameServerCmds) nID, Global.GetSocketRemoteEndPoint(socket), cmdData.Length));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// ID người chơi
                int roleID = int.Parse(fields[0]);

                /// Tìm chủ nhân của gói tin tương ứng
                KPlayer client = KTPlayerManager.Find(socket);
                if (null == client)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Không tìm thấy thông tin người chơi, CMD={0}, Client={1}", (TCPGameServerCmds) nID, Global.GetSocketRemoteEndPoint(socket)));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// Người chơi tương ứng
                KPlayer player = KTPlayerManager.Find(roleID);
                /// Nếu không tìm thấy
                if (player == null)
                {
                    KTPlayerManager.ShowNotification(client, "Người chơi không tồn tại hoặc đã rời mạng!");
                    return TCPProcessCmdResults.RESULT_OK;
                }


                /// Tạo gói tin
                RoleDataMini rd = new RoleDataMini()
                {
                    RoleID = player.RoleID,
                    RoleName = player.RoleName,
                    FactionID = player.m_cPlayerFaction.GetFactionId(),
                    Level = player.m_Level,
                    TeamID = player.TeamID,
                    AvartaID = player.RolePic,
                    GuildName = player.GuildName,
                    FamilyName = player.FamilyName,
                };
                /// Gửi gói tin lại cho người chơi
                byte[] _cmdData = DataHelper.ObjectToBytes<RoleDataMini>(rd);
                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, _cmdData, nID);

                return TCPProcessCmdResults.RESULT_DATA;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, Global.GetDebugHelperInfo(socket), false);
            }

            return TCPProcessCmdResults.RESULT_FAILED;
        }
        #endregion
    }
}
