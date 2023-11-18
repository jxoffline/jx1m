using GameServer.Core.Executor;
using GameServer.KiemThe;
using GameServer.KiemThe.Core.Item;
using GameServer.KiemThe.Entities;
using GameServer.KiemThe.Logic;
using GameServer.KiemThe.Logic.Manager.Shop;
using GameServer.Logic;
using GameServer.Server;
using Server.Protocol;
using Server.TCP;
using Server.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameServer.VLTK.Core.GuildManager
{
    /// <summary>
    /// Quản lý toàn bộ tầng mạng liên kết với gameclient
    /// </summary>
    public partial class GuildManager
    {
        /// <summary>
        /// Sửa công cáo bang hội
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
        public static TCPProcessCmdResults CMD_KT_GUILD_CHANGE_NOTIFY(TCPManager tcpMgr, TMSKSocket socket, TCPClientPool tcpClientPool, TCPRandKey tcpRandKey, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;

            GuildChangeSlogan changeSlogan = null;
            try
            {
                /// Giải mã gói tin đẩy về
                changeSlogan = DataHelper.BytesToObject<GuildChangeSlogan>(data, 0, data.Length);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Error while getting DATA, CMD={0}, Client={1}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket)));
                return TCPProcessCmdResults.RESULT_FAILED;
            }

            try
            {
                KPlayer client = KTPlayerManager.Find(socket);
                if (null == client)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Can not find player corresponding ID, CMD={0}, Client={1}, RoleID={2}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), client.RoleID));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// Tôn chỉ
                string msg = changeSlogan.Slogan;

                /// Nếu không có bang
                if (client.GuildID <= 0 || client.GuildID != changeSlogan.GuildID)
                {
                    KTPlayerManager.ShowMessageBox(client, "Thông báo", "Bạn không có bang!");
                    return TCPProcessCmdResults.RESULT_OK;
                }
                /// Nếu không phải bang chủ hoặc phó bang chủ
                else if (client.GuildRank != (int)GuildRank.Master && client.GuildRank != (int)GuildRank.ViceMaster)
                {
                    KTPlayerManager.ShowMessageBox(client, "Thông báo", "Chỉ có bang chủ hoặc phó bang chủ mới có thể sửa tôn chỉ bang hội!");
                    return TCPProcessCmdResults.RESULT_OK;
                }

                if (msg.Length > 400)
                {
                    KTPlayerManager.ShowMessageBox(client, "Thông báo", "Thông báo không thể vượt quá 400 ký tự");
                    return TCPProcessCmdResults.RESULT_OK;
                }

                // byte[] dbData = DataHelper.ObjectToBytes<GuildChangeSlogan>(changeSlogan);
                TCPProcessCmdResults result = Global.TransferRequestToDBServer(tcpMgr, socket, tcpClientPool, tcpRandKey, pool, nID, data, count, out TCPOutPacket tcpOutPacket2, client.ServerId);

                string strCmdResult2 = null;

                tcpOutPacket2.GetPacketCmdData(out strCmdResult2);

                if (strCmdResult2 != null)
                {
                    string[] Pram2 = strCmdResult2.Split(':');

                    int Status = Int32.Parse(Pram2[0]);

                    if (Status > 0)
                    {
                        MiniGuildInfo _Info = GuildManager.getInstance()._GetInfoGuildByGuildID(client.GuildID);
                        if (_Info != null)
                        {
                            var date = DateTime.Now;
                            var nextSunday = date.AddDays(7 - (int)date.DayOfWeek);

                            _Info.GuildNotify = "<color=red>Lợi Tức Tuần:</color> " + KTGlobal.GetDisplayMoney(_Info.MoneyBound) + "\n<color=red>Trả Lợi Tức :</color> " + nextSunday.ToShortDateString() + "\n<color=yellow>===================================</color>\n<color=green>Thông Báo Bang :</color>\n" + msg;
                        }

                        KTPlayerManager.ShowMessageBox(client, "Thông báo", "Thiết lập thành công");

                        /// Gói tin gửi lại

                        return TCPProcessCmdResults.RESULT_OK;
                    }
                    else
                    {
                        KTPlayerManager.ShowMessageBox(client, "Thông báo", "Thiết lập thất bại");
                        return TCPProcessCmdResults.RESULT_OK;
                    }
                }

                return TCPProcessCmdResults.RESULT_OK;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, Global.GetDebugHelperInfo(socket), false);
            }

            return TCPProcessCmdResults.RESULT_FAILED;
        }

        /// <summary>
        /// Lấy ra thông tin bang hội
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
        public static TCPProcessCmdResults CMD_KT_GUILD_GETINFO(TCPManager tcpMgr, TMSKSocket socket, TCPClientPool tcpClientPool, TCPRandKey tcpRandKey, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
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
                LogManager.WriteLog(LogTypes.Error, string.Format("Error while getting DATA, CMD={0}, Client={1}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket)));
                return TCPProcessCmdResults.RESULT_FAILED;
            }

            try
            {
                KPlayer client = KTPlayerManager.Find(socket);
                if (null == client)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Can not find player corresponding ID, CMD={0}, Client={1}, RoleID={2}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), client.RoleID));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                if (client.GuildID <= 0)
                {
                    KTPlayerManager.ShowNotification(client, "Bạn không có bang hội");

                    return TCPProcessCmdResults.RESULT_OK;
                }

                string[] fields = cmdData.Split(':');
                if (fields.Length != 2)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Có lỗi ở tham số gửi lên, CMD={0}, Client={1}, Recv={2}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), fields.Length));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                int RoleID = Int32.Parse(fields[0]);

                int GuildID = Int32.Parse(fields[1]);

                GuildInfo _GuildInfo = GuildManager.getInstance().GetGuildInfoByGuildID(GuildID, client);

                if (_GuildInfo == null)
                {
                    // Nếu đây là liên máy chủ
                    if (GameManager.IsKuaFuServer)
                    {
                        if (!GuildManager.getInstance().CheckGuildExist(client.GuildID))
                        {
                            // Thực hiện lấy thông tin từ máy chủ thật
                            GuildManager.getInstance().GetAllGuildInfo(client.ZoneID);

                            KTPlayerManager.ShowNotification(client, "Hãy thử lại sau 10 giây!");
                        }
                    }
                    else
                    {
                        KTPlayerManager.ShowNotification(client, "Dữ liệu bang hội chưa sẵn sàng");
                    }
                    return TCPProcessCmdResults.RESULT_OK;
                }
                // Trả về thông tin bang hội
                tcpOutPacket = DataHelper.ObjectToTCPOutPacket<GuildInfo>(_GuildInfo, pool, nID);

                return TCPProcessCmdResults.RESULT_DATA;

                // trả vào DB xử lý
                // return Global.TransferRequestToDBServer(tcpMgr, socket, tcpClientPool, tcpRandKey, pool, nID, data, count, out tcpOutPacket, client.ServerId);
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, Global.GetDebugHelperInfo(socket), false);
            }

            return TCPProcessCmdResults.RESULT_FAILED;
        }

        /// <summary>
        /// Packet tạo bang hội
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
        public static TCPProcessCmdResults CMD_KT_GUILD_CREATE(TCPManager tcpMgr, TMSKSocket socket, TCPClientPool tcpClientPool, TCPRandKey tcpRandKey, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
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
                LogManager.WriteLog(LogTypes.Error, string.Format("Error while getting DATA, CMD={0}, Client={1}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket)));
                return TCPProcessCmdResults.RESULT_FAILED;
            }

            try
            {
                KPlayer client = KTPlayerManager.Find(socket);
                if (null == client)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Can not find player corresponding ID, CMD={0}, Client={1}, RoleID={2}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), client.RoleID));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                string[] fields = cmdData.Split(':');
                if (fields.Length != 1)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Có lỗi ở tham số gửi lên, CMD={0}, Client={1}, Recv={2}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), fields.Length));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                if (client.m_cPlayerFaction.GetFactionId() == 0)
                {
                    KTPlayerManager.ShowNotification(client, "Phải vào phái mới có thể tạo lập bang hội");
                    return TCPProcessCmdResults.RESULT_OK;
                }

                string name = fields[0];

                /// Kiểm tra xem tên có hợp lệ không
                if (!Utils.CheckValidString(name))
                {
                    KTPlayerManager.ShowNotification(client, "Tên bang không được chứa ký tự đặc biệt");
                    return TCPProcessCmdResults.RESULT_OK;
                }

                /// Kiểm tra độ dài của tên có hợp lệ hay không
                if (string.IsNullOrEmpty(name) || name.Length < 6 || name.Length > 18)
                {
                    KTPlayerManager.ShowNotification(client, "Tên bang phải từ 6 tới 18 ký tự");
                    return TCPProcessCmdResults.RESULT_OK;
                }

                // Kiểm tra xem có tiền không
                if (!KTGlobal.IsHaveMoney(client, 5000000, MoneyType.Bac))
                {
                    KTPlayerManager.ShowNotification(client, "Bạc trên người không đủ");
                    return TCPProcessCmdResults.RESULT_OK;
                }

                int ITEMCOUNT = ItemManager.GetItemCountInBag(client, GuildManager.ITEM_REQUEST_CREATE_GUILD, -1);
                if (ITEMCOUNT < 1)
                {
                    KTPlayerManager.ShowNotification(client, "Vật phẩm yêu cầu tạo bang không đủ");
                    return TCPProcessCmdResults.RESULT_OK;
                }
                if (client.m_Level < 60)
                {
                    KTPlayerManager.ShowNotification(client, "Cấp độ tối thiểu là 60 mới có thể tạo bang hội");
                    return TCPProcessCmdResults.RESULT_OK;
                }

                string CMDBUILD = client.RoleID + ":" + name + ":" + client.ZoneID;

                byte[] ByteSendToDB = Encoding.ASCII.GetBytes(CMDBUILD);

                TCPProcessCmdResults result = Global.TransferRequestToDBServer(tcpMgr, socket, tcpClientPool, tcpRandKey, pool, nID, ByteSendToDB, count, out tcpOutPacket, client.ServerId);
                if (null != tcpOutPacket)
                {
                    string strCmdResult = null;
                    tcpOutPacket.GetPacketCmdData(out strCmdResult);
                    if (null != strCmdResult)
                    {
                        string[] Pram = strCmdResult.Split(':');

                        int Status = Int32.Parse(Pram[0]);

                        if (Status == -3)
                        {
                            KTPlayerManager.ShowMessageBox(client, "Thông báo", "Tên bang hội đã tồn tại vui lòng chọn tên khác");
                        }
                        else if (Status == -4)
                        {
                            KTPlayerManager.ShowMessageBox(client, "Thông báo", "Bạn đã có bang hội không thể tạo thêm");
                        }
                        else if (Status == 0) // Tạo bang thành công
                        {
                            // Xóa vật phẩm tạo bang trên người
                            ItemManager.RemoveItemFromBag(client, GuildManager.ITEM_REQUEST_CREATE_GUILD, 1);

                            KTGlobal.SubMoney(client, 5000000, MoneyType.Bac, "CREATE GUILD");
                            KTPlayerManager.ShowMessageBox(client, "Thông báo", "Tạo bang hội thành công");

                            int GUILDID = Int32.Parse(Pram[1]);
                            string GUILDSTR = Pram[2];

                            /// Sau khi tạo bang thành công thằng này nó sẽ là bang chủ luôn
                            client.GuildID = GUILDID;
                            client.GuildName = GUILDSTR;
                            client.GuildRank = (int)GuildRank.Master;

                            //TODO MINI GUILD INFO

                            MiniGuildInfo _MiniGuild = new MiniGuildInfo();
                            _MiniGuild.GuilLevel = 1;
                            _MiniGuild.GuildExp = 0;
                            _MiniGuild.GuildId = GUILDID;
                            _MiniGuild.GuildName = GUILDSTR;
                            _MiniGuild.MoneyBound = 0;
                            _MiniGuild.GuildNotify = "Chào mừng các bạn đã gia nhập bang [" + GUILDSTR + "]";
                            _MiniGuild.GuildMoney = 0;
                            _MiniGuild.Total_Copy_Scenes_This_Week = 0;
                            _MiniGuild.Task = new GuildTask();
                            _MiniGuild.ItemStore = "";
                            _MiniGuild.SkillNote = "";
                            _MiniGuild.SkillProbsKMagicAttribs = new List<KMagicAttrib>();
                            _MiniGuild.HostName = client.RoleName;
                            _MiniGuild.TotalMember = 1;
                            _MiniGuild.IsFinishTaskInDay = false;
                            _TotalGuild.TryAdd(GUILDID, _MiniGuild);
                            ///Xử lý nhiệm vụ cho nó
                            GuildManager.getInstance().ReloadTaskOfGuild(GUILDID);

                            KT_TCPHandler.NotifyOthersMyTitleChanged(client);

                            /// Thông báo cập nhật thông tin gia tộc và bang hội
                            KT_TCPHandler.NotifyOtherMyGuildRankChanged(client);
                        }

                        tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, strCmdResult, nID);
                        return TCPProcessCmdResults.RESULT_DATA;
                    }
                }
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, Global.GetDebugHelperInfo(socket), false);
            }

            return TCPProcessCmdResults.RESULT_FAILED;
        }

        /// <summary>
        /// Lấy danh sách thành viên trong bang hội
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
        public static TCPProcessCmdResults CMD_KT_GUILD_GETMEMBERLIST(TCPManager tcpMgr, TMSKSocket socket, TCPClientPool tcpClientPool, TCPRandKey tcpRandKey, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
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
                LogManager.WriteLog(LogTypes.Error, string.Format("Error while getting DATA, CMD={0}, Client={1}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket)));
                return TCPProcessCmdResults.RESULT_FAILED;
            }

            try
            {
                KPlayer client = KTPlayerManager.Find(socket);
                if (null == client)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Can not find player corresponding ID, CMD={0}, Client={1}, RoleID={2}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), client.RoleID));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                string[] fields = cmdData.Split(':');
                if (fields.Length != 3)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Có lỗi ở tham số gửi lên, CMD={0}, Client={1}, Recv={2}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), fields.Length));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                if (client.GuildID <= 0)
                {
                    KTPlayerManager.ShowNotification(client, "Bạn không có bang hội");

                    return TCPProcessCmdResults.RESULT_OK;
                }

                int RoleID = Int32.Parse(fields[0]);

                int GuildID = Int32.Parse(fields[1]);

                int PageIndex = Int32.Parse(fields[2]);

                if (client.GuildID != GuildID)
                {
                    KTPlayerManager.ShowNotification(client, "Bạn không phải là thành viên của bang hội");

                    return TCPProcessCmdResults.RESULT_OK;
                }

                // trả vào DB xử lý
                return Global.TransferRequestToDBServer(tcpMgr, socket, tcpClientPool, tcpRandKey, pool, nID, data, count, out tcpOutPacket, client.ServerId);
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, Global.GetDebugHelperInfo(socket), false);
            }

            return TCPProcessCmdResults.RESULT_FAILED;
        }

        /// <summary>
        /// Trả về danh sách xin vào bang
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
        public static TCPProcessCmdResults CMD_KT_GUILD_REQUEST_JOIN_LIST(TCPManager tcpMgr, TMSKSocket socket, TCPClientPool tcpClientPool, TCPRandKey tcpRandKey, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
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
                LogManager.WriteLog(LogTypes.Error, string.Format("Error while getting DATA, CMD={0}, Client={1}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket)));
                return TCPProcessCmdResults.RESULT_FAILED;
            }

            try
            {
                KPlayer client = KTPlayerManager.Find(socket);
                if (null == client)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Can not find player corresponding ID, CMD={0}, Client={1}, RoleID={2}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), client.RoleID));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                string[] fields = cmdData.Split(':');
                if (fields.Length != 3)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Có lỗi ở tham số gửi lên, CMD={0}, Client={1}, Recv={2}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), fields.Length));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                if (client.GuildID <= 0)
                {
                    KTPlayerManager.ShowNotification(client, "Bạn không có bang hội");

                    return TCPProcessCmdResults.RESULT_OK;
                }

                //  int RoleID = Int32.Parse(fields[0]);

                int GuildID = Int32.Parse(fields[1]);

                // int PageIndex = Int32.Parse(fields[2]);

                if (client.GuildID != GuildID)
                {
                    KTPlayerManager.ShowNotification(client, "Bạn không phải là thành viên của bang hội");

                    return TCPProcessCmdResults.RESULT_OK;
                }

                // trả vào DB xử lý
                return Global.TransferRequestToDBServer(tcpMgr, socket, tcpClientPool, tcpRandKey, pool, nID, data, count, out tcpOutPacket, client.ServerId);
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, Global.GetDebugHelperInfo(socket), false);
            }

            return TCPProcessCmdResults.RESULT_FAILED;
        }

        /// <summary>
        /// Trả về danh sách kỹ năng bang
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
        public static TCPProcessCmdResults CMD_KT_GUILD_SKILL_LIST(TCPManager tcpMgr, TMSKSocket socket, TCPClientPool tcpClientPool, TCPRandKey tcpRandKey, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
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
                LogManager.WriteLog(LogTypes.Error, string.Format("Error while getting DATA, CMD={0}, Client={1}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket)));
                return TCPProcessCmdResults.RESULT_FAILED;
            }

            try
            {
                KPlayer client = KTPlayerManager.Find(socket);
                if (null == client)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Can not find player corresponding ID, CMD={0}, Client={1}, RoleID={2}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), client.RoleID));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                if (client.GuildID <= 0)
                {
                    KTPlayerManager.ShowNotification(client, "Bạn không có bang hội");

                    return TCPProcessCmdResults.RESULT_OK;
                }

                string[] fields = cmdData.Split(':');
                if (fields.Length != 2)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Có lỗi ở tham số gửi lên, CMD={0}, Client={1}, Recv={2}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), fields.Length));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                int RoleID = Int32.Parse(fields[0]);

                int GuildID = Int32.Parse(fields[1]);

                List<SkillDef> _SkillDef = GuildManager.getInstance().GetTotalSkillDef(GuildID);

                // Trả về danh sách kỹ năng của bang hội đang active
                tcpOutPacket = DataHelper.ObjectToTCPOutPacket<List<SkillDef>>(_SkillDef, pool, nID);

                return TCPProcessCmdResults.RESULT_DATA;

                // trả vào DB xử lý
                // return Global.TransferRequestToDBServer(tcpMgr, socket, tcpClientPool, tcpRandKey, pool, nID, data, count, out tcpOutPacket, client.ServerId);
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, Global.GetDebugHelperInfo(socket), false);
            }

            return TCPProcessCmdResults.RESULT_FAILED;
        }

        /// <summary>
        /// CMD lấy ra danh sách nhiệm vụ
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
        public static TCPProcessCmdResults CMD_KT_GUILD_QUEST_LIST(TCPManager tcpMgr, TMSKSocket socket, TCPClientPool tcpClientPool, TCPRandKey tcpRandKey, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
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
                LogManager.WriteLog(LogTypes.Error, string.Format("Error while getting DATA, CMD={0}, Client={1}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket)));
                return TCPProcessCmdResults.RESULT_FAILED;
            }

            try
            {
                KPlayer client = KTPlayerManager.Find(socket);
                if (null == client)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Can not find player corresponding ID, CMD={0}, Client={1}, RoleID={2}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), client.RoleID));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                if (client.GuildID <= 0)
                {
                    KTPlayerManager.ShowNotification(client, "Bạn không có bang hội");

                    return TCPProcessCmdResults.RESULT_OK;
                }

                string[] fields = cmdData.Split(':');
                if (fields.Length != 2)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Có lỗi ở tham số gửi lên, CMD={0}, Client={1}, Recv={2}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), fields.Length));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                int RoleID = Int32.Parse(fields[0]);

                int GuildID = Int32.Parse(fields[1]);

                GuildTask _TaskDef = GuildManager.getInstance().GetGuildTask(GuildID);

                if (_TaskDef == null)
                {
                    // nếu như đéo có thì tạo mới 1 cái -1 gửi về client sẽ tự hiển thị là ko có quest
                    _TaskDef = new GuildTask();
                }

                // Trả về danh sách kỹ năng của bang hội đang active
                tcpOutPacket = DataHelper.ObjectToTCPOutPacket<GuildTask>(_TaskDef, pool, nID);

                return TCPProcessCmdResults.RESULT_DATA;

                // trả vào DB xử lý
                // return Global.TransferRequestToDBServer(tcpMgr, socket, tcpClientPool, tcpRandKey, pool, nID, data, count, out tcpOutPacket, client.ServerId);
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, Global.GetDebugHelperInfo(socket), false);
            }

            return TCPProcessCmdResults.RESULT_FAILED;
        }

        /// <summary>
        /// Lấy ra danh sách các bang hội khác
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
        public static TCPProcessCmdResults CMD_KT_GUILD_OTHER_LIST(TCPManager tcpMgr, TMSKSocket socket, TCPClientPool tcpClientPool, TCPRandKey tcpRandKey, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
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
                LogManager.WriteLog(LogTypes.Error, string.Format("Error while getting DATA, CMD={0}, Client={1}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket)));
                return TCPProcessCmdResults.RESULT_FAILED;
            }

            try
            {
                KPlayer client = KTPlayerManager.Find(socket);
                if (null == client)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Can not find player corresponding ID, CMD={0}, Client={1}, RoleID={2}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), client.RoleID));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                //if (client.GuildID <= 0)
                //{
                //    PlayerManager.ShowNotification(client, "Bạn không có bang hội");

                //    return TCPProcessCmdResults.RESULT_OK;
                //}

                // List<MiniGuildInfo> _SendToClient = new List<MiniGuildInfo>();

                List<MiniGuildInfo> _TotalGuild = GuildManager._TotalGuild.Values.ToList();

                // Trả về danh sách bang hội hiện tại
                tcpOutPacket = DataHelper.ObjectToTCPOutPacket<List<MiniGuildInfo>>(_TotalGuild, pool, nID);

                return TCPProcessCmdResults.RESULT_DATA;

                // trả vào DB xử lý
                // return Global.TransferRequestToDBServer(tcpMgr, socket, tcpClientPool, tcpRandKey, pool, nID, data, count, out tcpOutPacket, client.ServerId);
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, Global.GetDebugHelperInfo(socket), false);
            }

            return TCPProcessCmdResults.RESULT_FAILED;
        }

        /// <summary>
        /// Xin vào 1 bang nào đó
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
        public static TCPProcessCmdResults CMD_KT_GUILD_REQUEST_JOIN(TCPManager tcpMgr, TMSKSocket socket, TCPClientPool tcpClientPool, TCPRandKey tcpRandKey, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
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
                LogManager.WriteLog(LogTypes.Error, string.Format("Error while getting DATA, CMD={0}, Client={1}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket)));
                return TCPProcessCmdResults.RESULT_FAILED;
            }

            try
            {
                KPlayer client = KTPlayerManager.Find(socket);
                if (null == client)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Can not find player corresponding ID, CMD={0}, Client={1}, RoleID={2}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), client.RoleID));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                string[] fields = cmdData.Split(':');
                if (fields.Length != 2)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Có lỗi ở tham số gửi lên, CMD={0}, Client={1}, Recv={2}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), fields.Length));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                //ID thằng xin vào
                int RoleID = Int32.Parse(fields[0]);

                //ID Bang muốn vào
                int GuildID = Int32.Parse(fields[1]);

                string CMDBUILD = client.RoleID + ":" + GuildID + ":" + client.ZoneID;

                byte[] ByteSendToDB = Encoding.ASCII.GetBytes(CMDBUILD);

                TCPProcessCmdResults result = Global.TransferRequestToDBServer(tcpMgr, socket, tcpClientPool, tcpRandKey, pool, nID, ByteSendToDB, count, out tcpOutPacket, client.ServerId);
                if (null != tcpOutPacket)
                {
                    string strCmdResult = null;

                    tcpOutPacket.GetPacketCmdData(out strCmdResult);
                    if (null != strCmdResult)
                    {
                        string[] Pram = strCmdResult.Split(':');

                        int Status = Int32.Parse(Pram[0]);

                        if (Status == -1)
                        {
                            KTPlayerManager.ShowMessageBox(client, "Thông báo", "Bạn đã vào bang rồi không thể xin vào bang thêm nữa");
                        }
                        else if (Status == -2)
                        {
                            KTPlayerManager.ShowMessageBox(client, "Thông báo", "Số thành viên của bang hội đã đạt giới hạn!");
                        }
                        else if (Status == -300)
                        {
                            KTPlayerManager.ShowMessageBox(client, "Thông báo", "Bạn không đủ yêu cầu gia nhập của bạng hội");
                        }
                        else if (Status == -100)
                        {
                            KTPlayerManager.ShowMessageBox(client, "Thông báo", "Bạn đã xin vào bang này rồi! Vui lòng chờ phản hồi");
                        }
                        else if (Status == 10) // Xin gia nhập nhưng lại có duyệt tự động
                        {
                            _TotalGuild.TryGetValue(GuildID, out MiniGuildInfo findGuild);
                            //  var findGuild = GuildManager._TotalGuild.Where(x => x.GuildId == GuildID).FirstOrDefault();
                            if (findGuild != null)
                            {
                                client.GuildID = findGuild.GuildId;
                                client.GuildName = findGuild.GuildName;
                                // Set cho nó làm thành viên
                                client.GuildRank = (int)GuildRank.Member;

                                KT_TCPHandler.NotifyOthersMyTitleChanged(client);

                                /// Thông báo cập nhật thông tin gia tộc và bang hội
                                KT_TCPHandler.NotifyOtherMyGuildRankChanged(client);
                            }
                        }
                        else if (Status == 0)
                        {
                            KTPlayerManager.ShowMessageBox(client, "Thông báo", "Yêu cầu gia nhập thành công");
                        }
                        tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, strCmdResult, nID);
                        return TCPProcessCmdResults.RESULT_DATA;
                    }
                }
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, Global.GetDebugHelperInfo(socket), false);
            }

            return TCPProcessCmdResults.RESULT_FAILED;
        }

        /// <summary>
        /// Trả lời yêu càu vào bang
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
        public static TCPProcessCmdResults CMD_KT_GUILD_RESPONSE_JOINREQUEST(TCPManager tcpMgr, TMSKSocket socket, TCPClientPool tcpClientPool, TCPRandKey tcpRandKey, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
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
                LogManager.WriteLog(LogTypes.Error, string.Format("Error while getting DATA, CMD={0}, Client={1}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket)));
                return TCPProcessCmdResults.RESULT_FAILED;
            }

            try
            {
                KPlayer client = KTPlayerManager.Find(socket);
                if (null == client)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Can not find player corresponding ID, CMD={0}, Client={1}, RoleID={2}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), client.RoleID));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                string[] fields = cmdData.Split(':');
                if (fields.Length != 2)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Có lỗi ở tham số gửi lên, CMD={0}, Client={1}, Recv={2}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), fields.Length));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                // Nếu không phải bang chủ hoặc phó bang chủ
                if (client.GuildRank != (int)GuildRank.Master && client.GuildRank != (int)GuildRank.ViceMaster)
                {
                    KTPlayerManager.ShowMessageBox(client, "Thông báo", "Chỉ có bang chủ hoặc phó bang chủ mới có quyền xét duyệt thành viên");
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                //ID thằng thao tác
                int RoleID = Int32.Parse(fields[0]);

                // Có hay là không đồng ý
                int YesORNO = Int32.Parse(fields[1]);

                string CMDBUILD = RoleID + ":" + YesORNO + ":" + client.GuildID;

                byte[] ByteSendToDB = Encoding.ASCII.GetBytes(CMDBUILD);

                TCPProcessCmdResults result = Global.TransferRequestToDBServer(tcpMgr, socket, tcpClientPool, tcpRandKey, pool, nID, ByteSendToDB, count, out tcpOutPacket, client.ServerId);
                if (null != tcpOutPacket)
                {
                    string strCmdResult = null;

                    tcpOutPacket.GetPacketCmdData(out strCmdResult);
                    if (null != strCmdResult)
                    {
                        string[] Pram = strCmdResult.Split(':');

                        int Status = Int32.Parse(Pram[0]);

                        if (Status == -1)
                        {
                            KTPlayerManager.ShowMessageBox(client, "Thông báo", "Thành viên đã vào 1 bang hội khác");
                        }
                        else if (Status == -2)
                        {
                            KTPlayerManager.ShowMessageBox(client, "Thông báo", "Số thành viên của bang hội đã đạt giới hạn!");
                        }
                        else if (Status == -3)
                        {
                            KTPlayerManager.ShowMessageBox(client, "Thông báo", "Bang hội không tồn tại");
                        }
                        else if (Status == 0)
                        {
                            _TotalGuild.TryGetValue(client.GuildID, out MiniGuildInfo findGuild);

                            //  var findGuild = GuildManager._TotalGuild.Where(x => x.GuildId == client.GuildID).FirstOrDefault();
                            if (findGuild != null)
                            {
                                KPlayer MemberFind = KTPlayerManager.Find(RoleID);

                                if (MemberFind != null)
                                {
                                    MemberFind.GuildID = findGuild.GuildId;
                                    MemberFind.GuildName = findGuild.GuildName;
                                    // Set cho nó làm thành viên
                                    MemberFind.GuildRank = (int)GuildRank.Member;

                                    KT_TCPHandler.NotifyOthersMyTitleChanged(MemberFind);

                                    /// Thông báo cập nhật thông tin gia tộc và bang hội
                                    KT_TCPHandler.NotifyOtherMyGuildRankChanged(MemberFind);
                                    KTPlayerManager.ShowNotification(MemberFind, "Bạn đã gia nhập bang [" + findGuild.GuildName + "]");
                                }
                            }
                            KTPlayerManager.ShowMessageBox(client, "Thông báo", "Duyệt bang thành công");
                        }
                        else if (Status == 10)
                        {
                            KPlayer MemberFind = KTPlayerManager.Find(RoleID);

                            if (MemberFind != null)
                            {
                                _TotalGuild.TryGetValue(client.GuildID, out MiniGuildInfo findGuild);

                                //var findGuild = GuildManager._TotalGuild.Where(x => x.GuildId == client.GuildID).FirstOrDefault();
                                KTPlayerManager.ShowNotification(MemberFind, "Đơn xin gia nhập bang  [" + findGuild.GuildName + "] bị từ chối!");
                            }

                            KTPlayerManager.ShowNotification(client, "Từ chối gia nhập thành công!");
                        }

                        tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, strCmdResult, nID);
                        return TCPProcessCmdResults.RESULT_DATA;
                    }
                }
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, Global.GetDebugHelperInfo(socket), false);
            }

            return TCPProcessCmdResults.RESULT_FAILED;
        }

        /// <summary>
        /// Thay đổi chức vụ cho thành viên trong bang hội
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
        public static TCPProcessCmdResults CMD_KT_GUILD_CHANGERANK(TCPManager tcpMgr, TMSKSocket socket, TCPClientPool tcpClientPool, TCPRandKey tcpRandKey, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
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
                LogManager.WriteLog(LogTypes.Error, string.Format("Error while getting DATA, CMD={0}, Client={1}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket)));
                return TCPProcessCmdResults.RESULT_FAILED;
            }

            try
            {
                KPlayer client = KTPlayerManager.Find(socket);
                if (null == client)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Can not find player corresponding ID, CMD={0}, Client={1}, RoleID={2}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), client.RoleID));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                // Gửi lên 2 cái gồm ID ROLE CUA THANG MUỐN SET, RANKMUONSET
                string[] fields = cmdData.Split(':');
                if (fields.Length != 2)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Có lỗi ở tham số gửi lên, CMD={0}, Client={1}, Recv={2}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), fields.Length));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                if (client.GuildID <= 0)
                {
                    KTPlayerManager.ShowMessageBox(client, "Thông báo", "Bạn không ở 1 bang hội nào");
                    return TCPProcessCmdResults.RESULT_OK;
                }

                if (client.GuildRank != (int)GuildRank.Master && client.GuildRank != (int)GuildRank.ViceMaster)
                {
                    KTPlayerManager.ShowMessageBox(client, "Thông báo", "Chỉ có bang chủ và phó bang chủ mới có quyền thanh đổi chức danh cho thành viên");
                    return TCPProcessCmdResults.RESULT_OK;
                }

                int RoleID = Int32.Parse(fields[0]);

                int RankSet = Int32.Parse(fields[1]);

                if (RoleID == client.RoleID)
                {
                    KTPlayerManager.ShowMessageBox(client, "Thông báo", "Không thể bổ nhiệm chính mình!");
                    return TCPProcessCmdResults.RESULT_OK;
                }

                // Thế này cho nó vãi loài
                string CMDBUILD = client.GuildID + ":" + client.RoleID + ":" + RoleID + ":" + RankSet;

                byte[] ByteSendToDB = Encoding.ASCII.GetBytes(CMDBUILD);

                TCPProcessCmdResults result = Global.TransferRequestToDBServer(tcpMgr, socket, tcpClientPool, tcpRandKey, pool, nID, ByteSendToDB, count, out tcpOutPacket, client.ServerId);
                if (null != tcpOutPacket)
                {
                    string strCmdResult = null;
                    tcpOutPacket.GetPacketCmdData(out strCmdResult);
                    if (null != strCmdResult)
                    {
                        string[] Pram = strCmdResult.Split(':');

                        int Status = Int32.Parse(Pram[0]);

                        if (Status == -2)
                        {
                            KTPlayerManager.ShowMessageBox(client, "Thông báo", "Bạn không đủ quyền hạn để thay đổi chức vụ");
                        }
                        else if (Status == -5)
                        {
                            KTPlayerManager.ShowMessageBox(client, "Thông báo", "Số thành viên chức vụ bạn tiến cử đã đạt tối đa");
                        }
                        else if (Status == -100)
                        {
                            KTPlayerManager.ShowMessageBox(client, "Thông báo", "Có lỗi khi truy vấn thông tin người chơi");
                        }
                        else if (Status == -1)
                        {
                            KTPlayerManager.ShowMessageBox(client, "Thông báo", "Bang không tồn tại");
                        }
                        else if (Status == 0)
                        {
                            KPlayer _TargetPlayer = KTPlayerManager.Find(RoleID);
                            if (_TargetPlayer != null)
                            {
                                _TargetPlayer.GuildRank = RankSet;
                                KT_TCPHandler.NotifyOthersMyTitleChanged(_TargetPlayer);

                                /// Thông báo cập nhật thông tin gia tộc và bang hội
                                KT_TCPHandler.NotifyOtherMyGuildRankChanged(_TargetPlayer);

                                KTPlayerManager.ShowNotification(_TargetPlayer, "Bạn đã được [" + client.RoleName + "] thay đổi chức vụ!");
                            }

                            KTPlayerManager.ShowMessageBox(client, "Thông báo", "Tiến cử thành viên thành công");
                        }
                        else if (Status == 10) // trong trường hợp thay đổi chức vụ
                        {
                            KPlayer _TargetPlayer = KTPlayerManager.Find(RoleID);

                            if (_TargetPlayer != null)
                            {
                                _TargetPlayer.GuildRank = (int)GuildRank.Master;
                                KT_TCPHandler.NotifyOthersMyTitleChanged(_TargetPlayer);

                                /// Thông báo cập nhật thông tin gia tộc và bang hội
                                KT_TCPHandler.NotifyOtherMyGuildRankChanged(_TargetPlayer);

                                KTPlayerManager.ShowNotification(_TargetPlayer, "Bạn đã được [" + client.RoleName + "] thay đổi chức vụ!");

                                _TotalGuild.TryGetValue(client.GuildID, out MiniGuildInfo FindGuild);

                                // var FindGuild = GuildManager._TotalGuild.Where(x => x.GuildId == client.GuildID)
                                // .FirstOrDefault();
                                if (FindGuild != null)
                                {
                                    FindGuild.HostName = _TargetPlayer.RoleName;
                                }
                            }

                            // Gửi cho thằng bang chủ chim cút
                            client.GuildRank = (int)GuildRank.Member;

                            KT_TCPHandler.NotifyOthersMyTitleChanged(client);

                            /// Thông báo cập nhật thông tin gia tộc và bang hội
                            KT_TCPHandler.NotifyOtherMyGuildRankChanged(client);

                            KTPlayerManager.ShowMessageBox(client, "Thông báo", "Tiến cử thành viên thành công");
                        }

                        tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, strCmdResult, nID);
                        return TCPProcessCmdResults.RESULT_DATA;
                    }
                }

                //Chuyển vào gameDB để xử lý

                return TCPProcessCmdResults.RESULT_OK;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, Global.GetDebugHelperInfo(socket), false);
            }

            return TCPProcessCmdResults.RESULT_FAILED;
        }

        /// <summary>
        /// Cống hiến vào bang
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
        public static TCPProcessCmdResults CMD_KT_GUILD_DONATE(TCPManager tcpMgr, TMSKSocket socket, TCPClientPool tcpClientPool, TCPRandKey tcpRandKey, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
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
                LogManager.WriteLog(LogTypes.Error, string.Format("Error while getting DATA, CMD={0}, Client={1}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket)));
                return TCPProcessCmdResults.RESULT_FAILED;
            }

            try
            {
                KPlayer client = KTPlayerManager.Find(socket);
                if (null == client)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Can not find player corresponding ID, CMD={0}, Client={1}, RoleID={2}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), client.RoleID));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                string[] fields = cmdData.Split(':');
                if (fields.Length != 2)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Có lỗi ở tham số gửi lên, CMD={0}, Client={1}, Recv={2}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), fields.Length));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                // Chưa có bang thì không phải cống hiến

                if (client.GuildID <= 0)
                {
                    KTPlayerManager.ShowMessageBox(client, "Thông báo", "Bạn phải gia nhập bang hội trước!");
                    return TCPProcessCmdResults.RESULT_OK;
                }

                if (GameManager.IsKuaFuServer)
                {
                    KTPlayerManager.ShowNotification(client, "Chức năng này không khả dụng ở liên máy chủ");

                    return TCPProcessCmdResults.RESULT_OK;
                }

                //Kiểu sẽ cống hiến
                int Type = Int32.Parse(fields[0]);

                //0 Là cống hiến bạc
                //1 Là vật phẩm

                string Value = fields[1];

                // Nếu là cống hiến bạc
                if (Type == 0)
                {
                    int MoneyWantDonate = Int32.Parse(Value);

                    if (MoneyWantDonate <= 0)
                    {
                        KTPlayerManager.ShowMessageBox(client, "Thông báo", "Số bạc cống hiến phải lớn hơn 0");
                        return TCPProcessCmdResults.RESULT_OK;
                    }

                    if (!KTGlobal.IsHaveMoney(client, MoneyWantDonate, MoneyType.Bac))
                    {
                        KTPlayerManager.ShowMessageBox(client, "Thông báo", "Số bạc trong người không đủ");
                        return TCPProcessCmdResults.RESULT_OK;
                    }

                    _TotalGuild.TryGetValue(client.GuildID, out MiniGuildInfo FindGuildInfo);
                    // var FindGuildInfo = _TotalGuild.Where(x => x.GuildId == client.GuildID).FirstOrDefault();
                    if (FindGuildInfo != null)
                    {
                        if (MoneyWantDonate < 1000)
                        {
                            KTPlayerManager.ShowMessageBox(client, "Thông báo", "Số bạc cống hiến quá thấp!");
                            return TCPProcessCmdResults.RESULT_OK;
                        }

                        int CurMoneyGuild = FindGuildInfo.GuildMoney;
                        if (CurMoneyGuild + MoneyWantDonate > 1000000000)
                        {
                            KTPlayerManager.ShowMessageBox(client, "Thông báo", "Số lượng bang cống đã đạt gới hạn không thể cống hiến thêm");
                            return TCPProcessCmdResults.RESULT_OK;
                        }

                        FindGuildInfo.GuildMoney += MoneyWantDonate;

                        if (UpdateGuildResource(client.GuildID, GUILD_RESOURCE.GUILD_MONEY,
                                FindGuildInfo.GuildMoney + ""))
                        {
                            KTGlobal.SubMoney(client, MoneyWantDonate, MoneyType.Bac, "CONGHIENBANG");

                            KTGlobal.SendGuildChat(client.GuildID, "Thành viên [" + client.RoleName + "] đã cống hiến vào bang " + MoneyWantDonate + " bạc!", null, "");
                        }

                        int GuildMoney = (int)(MoneyWantDonate * _GuildConfig.Donates.PointPerGold);
                        int WeekPoint = (int)(MoneyWantDonate * _GuildConfig.Donates.PointPerGold);

                        if (GuildMoney > 0)
                        {
                            if (KTGlobal.AddMoney(client, GuildMoney, MoneyType.GuildMoney, "DONATEGUILD").IsOK)
                            {
                                KTPlayerManager.ShowNotification(client, "Cống hiến bang hội của bạn tăng thêm :" + GuildMoney);
                            }
                        }
                        if (WeekPoint > 0)
                        {
                            int WeekPointHave = GuildManager.getInstance().GetWeekPoint(client);

                            if (WeekPointHave == -1)
                            {
                                WeekPointHave = 0;
                            }

                            WeekPointHave += WeekPoint;

                            GuildManager.getInstance().SetWeekPoint(client, WeekPointHave);

                            KTPlayerManager.ShowNotification(client, "Điểm hoạt động tuần của bạn tăng thêm :" + WeekPoint);
                        }

                        // sau khi cống hiến xong thì trả về cho packet MINIGUILD INFO
                        GuildInfo _GuildInfo = GuildManager.getInstance().GetGuildInfoByGuildID(client.GuildID, client);

                        if (_GuildInfo == null)
                        {
                            KTPlayerManager.ShowNotification(client, "Dữ liệu bang hội chưa sẵn sàng");

                            return TCPProcessCmdResults.RESULT_OK;
                        }
                        // Trả về thông tin bang hội
                        tcpOutPacket = DataHelper.ObjectToTCPOutPacket<GuildInfo>(_GuildInfo, pool, (int)TCPGameServerCmds.CMD_KT_GUILD_GETINFO);
                        return TCPProcessCmdResults.RESULT_DATA;
                    }
                }
                else if (Type == 1) // Nếu là cống hiến = vật phẩm thì xử lý chuỗi gửi lên
                {
                    _TotalGuild.TryGetValue(client.GuildID, out MiniGuildInfo FindGuild);

                    //  var FindGuild = _TotalGuild.Where(x => x.GuildId == client.GuildID).FirstOrDefault();
                    if (FindGuild != null)
                    {
                        // Giãi mã đống vật phẩm này
                        List<ItemRequest> ItemDecode = GuildManager.ItemDecode(Value);

                        List<ItemRequest> ItemGuildHave = GuildManager.ItemDecode(FindGuild.ItemStore);

                        int TotalPointWillGet = 0;
                        //List lại danh sách vật phẩm
                        foreach (var _ItemRequest in ItemDecode)
                        {
                            int ItemID = _ItemRequest.ItemID;
                            int ItemNum = _ItemRequest.ItemNum;

                            var FindRequest = _GuildConfig.Donates.ItemDonate.Where(x => x.ItemID == ItemID).FirstOrDefault();
                            // Nếu như có vật phẩm này trong danh sách bang thì sẽ đồng ý nhận
                            if (FindRequest != null)
                            {
                                // Tích lũy cá nhân + tích lũy tuần sẽ tăng bao nhiêu
                                int PointWillGet = FindRequest.Point;

                                TotalPointWillGet += PointWillGet * ItemNum;
                                // tìm xem trong kho có item này chưa
                                var FindItemSTORE = ItemGuildHave.Where(x => x.ItemID == ItemID).FirstOrDefault();
                                if (FindItemSTORE != null)
                                {
                                    FindItemSTORE.ItemNum += ItemNum;
                                }
                                else // Nếu đéo có item này thì tạo mới để mà bú
                                {
                                    ItemRequest _NewItem = new ItemRequest();
                                    _NewItem.ItemID = ItemID;
                                    _NewItem.ItemNum = ItemNum;
                                    ItemGuildHave.Add(_NewItem);
                                }
                            }
                        }

                        // Xóa trước cho nó ăn cứt rồi mới set guild res sau
                        foreach (var _ItemRequest in ItemDecode)
                        {
                            if (!ItemManager.RemoveItemFromBag(client, _ItemRequest.ItemID, _ItemRequest.ItemNum, -1, "DONATEBANG"))
                            {
                                KTPlayerManager.ShowNotification(client,
                                    "Có lỗi khi trừ vật phẩm!, Vui lòng thử lại sau");

                                return TCPProcessCmdResults.RESULT_OK;
                            }
                        }

                        string FinalItemStore = ItemEncode(ItemGuildHave);
                        // Lock lại để tránh luồng khác cũng
                        lock (FindGuild.ItemStore)
                        {
                            FindGuild.ItemStore = FinalItemStore;
                        }

                        if (UpdateGuildResource(client.GuildID, GUILD_RESOURCE.ITEM, FinalItemStore))
                        {
                            int GuildMoney = TotalPointWillGet;
                            int WeekPoint = TotalPointWillGet;

                            if (GuildMoney > 0)
                            {
                                if (KTGlobal.AddMoney(client, GuildMoney, MoneyType.GuildMoney, "DONATEGUILD").IsOK)
                                {
                                    KTPlayerManager.ShowNotification(client, "Cống hiến bang hội của bạn tăng thêm :" + GuildMoney);
                                }
                            }
                            if (WeekPoint > 0)
                            {
                                int WeekPointHave = GuildManager.getInstance().GetWeekPoint(client);

                                if (WeekPointHave == -1)
                                {
                                    WeekPointHave = 0;
                                }

                                WeekPointHave += WeekPoint;

                                GuildManager.getInstance().SetWeekPoint(client, WeekPointHave);

                                KTPlayerManager.ShowNotification(client, "Điểm hoạt động tuần của bạn tăng thêm :" + WeekPoint);
                            }
                        }

                        // sau khi cống hiến xong thì trả về cho packet MINIGUILD INFO
                        GuildInfo _GuildInfo = GuildManager.getInstance().GetGuildInfoByGuildID(client.GuildID, client);

                        if (_GuildInfo == null)
                        {
                            KTPlayerManager.ShowNotification(client, "Dữ liệu bang hội chưa sẵn sàng");

                            return TCPProcessCmdResults.RESULT_OK;
                        }
                        // Trả về thông tin bang hội
                        tcpOutPacket = DataHelper.ObjectToTCPOutPacket<GuildInfo>(_GuildInfo, pool, (int)TCPGameServerCmds.CMD_KT_GUILD_GETINFO);
                        return TCPProcessCmdResults.RESULT_DATA;
                    }
                }

                return TCPProcessCmdResults.RESULT_OK;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, Global.GetDebugHelperInfo(socket), false);
            }

            return TCPProcessCmdResults.RESULT_FAILED;
        }

        /// <summary>
        /// Kick 1 thằng ra khỏi bang
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
        public static TCPProcessCmdResults CMD_KT_GUILD_KICK_ROLE(TCPManager tcpMgr, TMSKSocket socket, TCPClientPool tcpClientPool, TCPRandKey tcpRandKey, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
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
                LogManager.WriteLog(LogTypes.Error, string.Format("Error while getting DATA, CMD={0}, Client={1}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket)));
                return TCPProcessCmdResults.RESULT_FAILED;
            }

            try
            {
                KPlayer client = KTPlayerManager.Find(socket);

                if (null == client)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Can not find player corresponding ID, CMD={0}, Client={1}, RoleID={2}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), client.RoleID));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                string[] fields = cmdData.Split(':');
                if (fields.Length != 2)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Có lỗi ở tham số gửi lên, CMD={0}, Client={1}, Recv={2}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), fields.Length));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                int GUILDID = Int32.Parse(fields[0]);

                int ROLEIDGETKICK = Int32.Parse(fields[1]);

                if (client.RoleID == ROLEIDGETKICK)
                {
                    KTPlayerManager.ShowMessageBox(client, "Thông báo", "Bạn không thể tự trục xuất chính mình!");
                    return TCPProcessCmdResults.RESULT_OK;
                }
                if (client.GuildID <= 0)
                {
                    KTPlayerManager.ShowMessageBox(client, "Thông báo", "Bạn không thuộc 1 bang hội nào");
                    return TCPProcessCmdResults.RESULT_OK;
                }

                if (client.GuildRank != (int)GuildRank.Master && client.GuildRank != (int)GuildRank.ViceMaster)
                {
                    KTPlayerManager.ShowMessageBox(client, "Thông báo", "Chỉ bang chủ hoặc phó bang chủ mới có quyền kick người chơi");
                    return TCPProcessCmdResults.RESULT_OK;
                }

                TCPProcessCmdResults result = Global.TransferRequestToDBServer(tcpMgr, socket, tcpClientPool, tcpRandKey, pool, nID, data, count, out TCPOutPacket tcpOutPacket2, client.ServerId);

                string strCmdResult2 = null;

                tcpOutPacket2.GetPacketCmdData(out strCmdResult2);

                if (strCmdResult2 != null)
                {
                    string[] Pram2 = strCmdResult2.Split(':');

                    int Status = Int32.Parse(Pram2[0]);

                    if (Status == 0)
                    {
                        KPlayer _TargetPlayer = KTPlayerManager.Find(ROLEIDGETKICK);

                        if (_TargetPlayer != null)
                        {
                            _TargetPlayer.GuildRank = 0;
                            _TargetPlayer.GuildID = 0;
                            _TargetPlayer.GuildName = "";

                            //Update lại title cho thằng bị trục xuất
                            KT_TCPHandler.NotifyOthersMyTitleChanged(_TargetPlayer);

                            //Update lại title cho thằng bị trục xuất
                            KT_TCPHandler.NotifyOtherMyGuildRankChanged(_TargetPlayer);

                            KTPlayerManager.ShowNotification(_TargetPlayer, "Bạn đã bị trục xuất khỏi bang hội!");
                        }

                        // gửi thông báo tới gia tộc là đã kick thằng này thành công
                        KTPlayerManager.ShowMessageBox(client, "Thông báo", "Trục xuất thành công người chơi");

                        string responseData = string.Format("{0}", ROLEIDGETKICK);
                        tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, responseData, nID);
                        return TCPProcessCmdResults.RESULT_DATA;
                    }
                    else
                    {
                        KTPlayerManager.ShowMessageBox(client, "Thông báo", "Có lỗi khi trục xuất người chơi");
                        return TCPProcessCmdResults.RESULT_OK;
                    }
                }

                return TCPProcessCmdResults.RESULT_OK;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, Global.GetDebugHelperInfo(socket), false);
            }

            return TCPProcessCmdResults.RESULT_FAILED;
        }

        /// <summary>
        /// Thăng cấp bang hội
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
        public static TCPProcessCmdResults CMD_KT_GUILD_LEVELUP(TCPManager tcpMgr, TMSKSocket socket, TCPClientPool tcpClientPool, TCPRandKey tcpRandKey, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
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
                LogManager.WriteLog(LogTypes.Error, string.Format("Error while getting DATA, CMD={0}, Client={1}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket)));
                return TCPProcessCmdResults.RESULT_FAILED;
            }

            try
            {
                KPlayer client = KTPlayerManager.Find(socket);
                if (null == client)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Can not find player corresponding ID, CMD={0}, Client={1}, RoleID={2}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), client.RoleID));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                if (client.GuildID <= 0)
                {
                    KTPlayerManager.ShowNotification(client, "Bạn không có bang hội");

                    return TCPProcessCmdResults.RESULT_OK;
                }

                //string[] fields = cmdData.Split(':');
                //if (fields.Length != 2)
                //{
                //    LogManager.WriteLog(LogTypes.Error, string.Format("Có lỗi ở tham số gửi lên, CMD={0}, Client={1}, Recv={2}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), fields.Length));
                //    return TCPProcessCmdResults.RESULT_FAILED;
                //}

                //int RoleID = Int32.Parse(fields[0]);

                //int GuildID = Int32.Parse(fields[1]);

                if (client.GuildRank != (int)GuildRank.Master && client.GuildRank != (int)GuildRank.ViceMaster)
                {
                    KTPlayerManager.ShowNotification(client, "Chỉ có bang chủ hoặc phó bang chủ mới có quyền thăng cấp bang hội");

                    return TCPProcessCmdResults.RESULT_OK;
                }

                if (GameManager.IsKuaFuServer)
                {
                    KTPlayerManager.ShowNotification(client, "Chức năng này không khả dụng ở liên máy chủ");

                    return TCPProcessCmdResults.RESULT_OK;
                }

                int KQ = GuildManager.GuildLevelUp(client.GuildID, client);

                if (KQ == -1)
                {
                    KTPlayerManager.ShowNotification(client, "Bang của bạn đã đạt cấp độ tối đa!");

                    return TCPProcessCmdResults.RESULT_OK;
                }
                else if (KQ == -2)
                {
                    KTPlayerManager.ShowNotification(client, "Kinh nghiệm thăng cấp chưa đủ!");

                    return TCPProcessCmdResults.RESULT_OK;
                }
                else if (KQ == -3)
                {
                    KTPlayerManager.ShowNotification(client, "Vật phẩm yêu cầu không đủ");

                    return TCPProcessCmdResults.RESULT_OK;
                }
                else if (KQ == -4)
                {
                    KTPlayerManager.ShowNotification(client, "Bang cống yêu cầu không đủ");

                    return TCPProcessCmdResults.RESULT_OK;
                }
                else if (KQ == 0)
                {
                    GuildInfo _GuildInfo = GuildManager.getInstance().GetGuildInfoByGuildID(client.GuildID, client);

                    //trả về là thăng cấp thành công
                    tcpOutPacket = DataHelper.ObjectToTCPOutPacket<GuildInfo>(_GuildInfo, pool, (int)TCPGameServerCmds.CMD_KT_GUILD_GETINFO);
                    KTPlayerManager.ShowNotification(client, "Thăng cấp bang thành công");

                    return TCPProcessCmdResults.RESULT_DATA;
                }
                else if (KQ == -10)
                {
                    KTPlayerManager.ShowNotification(client, "Máy chủ đang bận vui lòng thử lại sau!");

                    return TCPProcessCmdResults.RESULT_OK;
                }

                return TCPProcessCmdResults.RESULT_OK;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, Global.GetDebugHelperInfo(socket), false);
            }

            return TCPProcessCmdResults.RESULT_FAILED;
        }

        /// <summary>
        /// Thăng cấp kỹ năng cho bang hội
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
        public static TCPProcessCmdResults CMD_KT_GUILD_SKILL_LEVELUP(TCPManager tcpMgr, TMSKSocket socket, TCPClientPool tcpClientPool, TCPRandKey tcpRandKey, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
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
                LogManager.WriteLog(LogTypes.Error, string.Format("Error while getting DATA, CMD={0}, Client={1}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket)));
                return TCPProcessCmdResults.RESULT_FAILED;
            }

            try
            {
                KPlayer client = KTPlayerManager.Find(socket);
                if (null == client)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Can not find player corresponding ID, CMD={0}, Client={1}, RoleID={2}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), client.RoleID));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                if (client.GuildID <= 0)
                {
                    KTPlayerManager.ShowNotification(client, "Bạn không có bang hội");

                    return TCPProcessCmdResults.RESULT_OK;
                }

                if (GameManager.IsKuaFuServer)
                {
                    KTPlayerManager.ShowNotification(client, "Chức năng này không khả dụng ở liên máy chủ");

                    return TCPProcessCmdResults.RESULT_OK;
                }
                string[] fields = cmdData.Split(':');
                if (fields.Length != 1)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Có lỗi ở tham số gửi lên, CMD={0}, Client={1}, Recv={2}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), fields.Length));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                int SkillID = Int32.Parse(fields[0]);

                if (client.GuildRank != (int)GuildRank.Master && client.GuildRank != (int)GuildRank.ViceMaster)
                {
                    KTPlayerManager.ShowNotification(client, "Chỉ có bang chủ hoặc phó bang chủ mới có quyền thăng cấp kỹ năng");

                    return TCPProcessCmdResults.RESULT_OK;
                }

                int KQ = GuildManager.LevelUpSkill(client.GuildID, SkillID);

                if (KQ == -100)
                {
                    KTPlayerManager.ShowNotification(client, "Kỹ năng muốn thăng cấp không tồn tại!");

                    return TCPProcessCmdResults.RESULT_OK;
                }
                else if (KQ == -1)
                {
                    KTPlayerManager.ShowNotification(client, "Đã đạt tới cấp độ tối đa!");

                    return TCPProcessCmdResults.RESULT_OK;
                }
                else if (KQ == -200)
                {
                    KTPlayerManager.ShowNotification(client, "Cấp bang chưa đủ để thăng cấp kỹ năng này!");

                    return TCPProcessCmdResults.RESULT_OK;
                }
                else if (KQ == -2)
                {
                    KTPlayerManager.ShowNotification(client, "Bang cống không đủ để thăng cấp kỹ năng");

                    return TCPProcessCmdResults.RESULT_OK;
                }
                else if (KQ == -3)
                {
                    KTPlayerManager.ShowNotification(client, "Vật phẩm không đủ để thăng cấp kỹ năng");

                    return TCPProcessCmdResults.RESULT_OK;
                }
                else if (KQ == -4)
                {
                    KTPlayerManager.ShowNotification(client, "Có lỗi khi trừ bang cống");

                    return TCPProcessCmdResults.RESULT_OK;
                }
                else if (KQ == 1)
                {
                    List<SkillDef> _SkillDef = GuildManager.getInstance().GetTotalSkillDef(client.GuildID);

                    // Trả về danh sách kỹ năng của bang hội đang active
                    tcpOutPacket = DataHelper.ObjectToTCPOutPacket<List<SkillDef>>(_SkillDef, pool, (int)(TCPGameServerCmds.CMD_KT_GUILD_SKILL_LIST));

                    KTPlayerManager.ShowNotification(client, "Thăng cấp kỹ năng thành công");

                    return TCPProcessCmdResults.RESULT_DATA;
                }
                else if (KQ == -100)
                {
                    KTPlayerManager.ShowNotification(client, "Máy chủ đang bận vui lòng thử lại sau!");

                    return TCPProcessCmdResults.RESULT_OK;
                }

                return TCPProcessCmdResults.RESULT_OK;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, Global.GetDebugHelperInfo(socket), false);
            }

            return TCPProcessCmdResults.RESULT_FAILED;
        }

        /// <summary>
        /// Xử lý từ bỏ nhiệm vụ hoặc là đổi nhiệm vụ khác
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
        public static TCPProcessCmdResults CMD_KT_GUILD_QUEST_CMD(TCPManager tcpMgr, TMSKSocket socket, TCPClientPool tcpClientPool, TCPRandKey tcpRandKey, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
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
                LogManager.WriteLog(LogTypes.Error, string.Format("Error while getting DATA, CMD={0}, Client={1}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket)));
                return TCPProcessCmdResults.RESULT_FAILED;
            }

            try
            {
                KPlayer client = KTPlayerManager.Find(socket);
                if (null == client)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Can not find player corresponding ID, CMD={0}, Client={1}, RoleID={2}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), client.RoleID));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                if (client.GuildID <= 0)
                {
                    KTPlayerManager.ShowNotification(client, "Bạn không có bang hội");

                    return TCPProcessCmdResults.RESULT_OK;
                }

                string[] fields = cmdData.Split(':');
                if (fields.Length != 1)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Có lỗi ở tham số gửi lên, CMD={0}, Client={1}, Recv={2}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), fields.Length));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                int COMMNAD = Int32.Parse(fields[0]);

                if (client.GuildRank != (int)GuildRank.Master && client.GuildRank != (int)GuildRank.ViceMaster)
                {
                    KTPlayerManager.ShowNotification(client, "Chỉ có bang chủ hoặc phó bang chủ mới có quyền thăng cấp kỹ năng");

                    return TCPProcessCmdResults.RESULT_OK;
                }

                // Thực hiện hành động với nhiệm vụ
                int KQ = GuildManager.getInstance().GuildQuestCmd(client.GuildID, COMMNAD);

                if (KQ == -2)
                {
                    KTPlayerManager.ShowNotification(client, "Bang cống không đủ để thực hiện đổi nhiệm vụ khác");

                    return TCPProcessCmdResults.RESULT_DATA;
                }
                else if (KQ == 1)
                {
                    GuildTask _TaskDef = GuildManager.getInstance().GetGuildTask(client.GuildID);

                    if (_TaskDef == null)
                    {
                        // nếu như đéo có thì tạo mới 1 cái -1 gửi về client sẽ tự hiển thị là ko có quest
                        _TaskDef = new GuildTask();
                    }

                    // Trả về danh sách kỹ năng của bang hội đang active
                    client.SendPacket<GuildTask>((int)TCPGameServerCmds.CMD_KT_GUILD_QUEST_LIST, _TaskDef);
                    KTPlayerManager.ShowNotification(client, "Đổi nhiệm vụ thành mới thành công");

                    return TCPProcessCmdResults.RESULT_OK;
                }
                else if (KQ == 2)
                {
                    GuildTask _TaskDef = GuildManager.getInstance().GetGuildTask(client.GuildID);

                    if (_TaskDef == null)
                    {
                        // nếu như đéo có thì tạo mới 1 cái -1 gửi về client sẽ tự hiển thị là ko có quest
                        _TaskDef = new GuildTask();
                    }

                    // Trả về danh sách kỹ năng của bang hội đang active
                    client.SendPacket<GuildTask>((int)TCPGameServerCmds.CMD_KT_GUILD_QUEST_LIST, _TaskDef);

                    KTPlayerManager.ShowNotification(client, "Từ bỏ nhiệm vụ thành công");

                    return TCPProcessCmdResults.RESULT_OK;
                }
                else if (KQ == -1)
                {
                    KTPlayerManager.ShowNotification(client, "Máy chủ đang bận vui lòng thử lại sau!");

                    return TCPProcessCmdResults.RESULT_OK;
                }
                else if (KQ == -10)
                {
                    KTPlayerManager.ShowNotification(client, "Số lần làm nhiệm vụ hôm nay đã hết vui lòng quay lại vào ngày mai!");

                    return TCPProcessCmdResults.RESULT_OK;
                }

                return TCPProcessCmdResults.RESULT_OK;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, Global.GetDebugHelperInfo(socket), false);
            }

            return TCPProcessCmdResults.RESULT_FAILED;
        }

        /// <summary>
        /// Thiết lập tự động duyệt bang
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
        public static TCPProcessCmdResults CMD_KT_GUILD_AUTO_ACCPECT_SETTING(TCPManager tcpMgr, TMSKSocket socket, TCPClientPool tcpClientPool, TCPRandKey tcpRandKey, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
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
                LogManager.WriteLog(LogTypes.Error, string.Format("Error while getting DATA, CMD={0}, Client={1}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket)));
                return TCPProcessCmdResults.RESULT_FAILED;
            }

            try
            {
                KPlayer client = KTPlayerManager.Find(socket);

                if (null == client)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Can not find player corresponding ID, CMD={0}, Client={1}, RoleID={2}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), client.RoleID));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                if (client.GuildID <= 0)
                {
                    KTPlayerManager.ShowNotification(client, "Bạn không có bang hội");

                    return TCPProcessCmdResults.RESULT_OK;
                }

                if (client.IsSpamClick())
                {
                    KTPlayerManager.ShowNotification(client, "Bạn đang thao tác quá nhanh");

                    return TCPProcessCmdResults.RESULT_OK;
                }
                else
                {
                    client.SendClick();
                }

                if (client.GuildRank != (int)GuildRank.ViceMaster && client.GuildRank != (int)GuildRank.Master)
                {
                    KTPlayerManager.ShowNotification(client, "Chỉ có bang chủ hoặc phó bang chủ mới có thể thiết lập tự duyệt thành viên");

                    return TCPProcessCmdResults.RESULT_OK;
                }

                string[] fields = cmdData.Split(':');
                if (fields.Length != 5)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Có lỗi ở tham số gửi lên, CMD={0}, Client={1}, Recv={2}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), fields.Length));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                return Global.TransferRequestToDBServer(tcpMgr, socket, tcpClientPool, tcpRandKey, pool, nID, data, count, out tcpOutPacket, client.ServerId);
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, Global.GetDebugHelperInfo(socket), false);
            }

            return TCPProcessCmdResults.RESULT_FAILED;
        }

        /// <summary>
        /// Mời vào bang từ bang chủ
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
        public static TCPProcessCmdResults CMD_KT_GUILD_INVITE(TCPManager tcpMgr, TMSKSocket socket, TCPClientPool tcpClientPool, TCPRandKey tcpRandKey, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
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
                LogManager.WriteLog(LogTypes.Error, string.Format("Error while getting DATA, CMD={0}, Client={1}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket)));
                return TCPProcessCmdResults.RESULT_FAILED;
            }

            try
            {
                KPlayer client = KTPlayerManager.Find(socket);
                if (null == client)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Can not find player corresponding ID, CMD={0}, Client={1}, RoleID={2}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), client.RoleID));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                string[] fields = cmdData.Split(':');
                if (fields.Length != 1)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Có lỗi ở tham số gửi lên, CMD={0}, Client={1}, Recv={2}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), fields.Length));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                int RoleID = Int32.Parse(fields[0]);

                KPlayer RoleIDInvite = KTPlayerManager.Find(RoleID);

                // Xem thằng này có online hay không
                if (null == RoleIDInvite)
                {
                    KTPlayerManager.ShowMessageBox(client, "Thông báo", "Đối phương đã rời mạng");
                    return TCPProcessCmdResults.RESULT_OK;
                }

                if (RoleIDInvite.GuildID > 0)
                {
                    KTPlayerManager.ShowMessageBox(client, "Thông báo", "Đối phương đã có bang hội");
                    return TCPProcessCmdResults.RESULT_OK;
                }

                if (client.GuildID <= 0)
                {
                    KTPlayerManager.ShowMessageBox(client, "Thông báo", "Bạn không có 1 bang hội nào");
                    return TCPProcessCmdResults.RESULT_OK;
                }

                // Xem thằng này có phải bang chủ hay không
                if (client.GuildRank != (int)GuildRank.Master && client.GuildRank != (int)GuildRank.ViceMaster)
                {
                    KTPlayerManager.ShowMessageBox(client, "Thông báo", "Chỉ có bang chủ hoặc phó bang chủ mới có quyền trực tiếp mời người chơi vào bang.");
                    return TCPProcessCmdResults.RESULT_OK;
                }

                /// Gửi lời mời bang hội cho thằng kia
                string responseData = string.Format("{0}:{1}:{2}:{3}:{4}:{5}:{6}", client.RoleID, client.RoleName, client.GuildID, client.GuildName, client.RolePic, client.m_Level, client.m_cPlayerFaction.GetFactionId());

                // Gửi cho thằng kia lời mời chiêu mộ bang hội
                RoleIDInvite.SendPacket(nID, responseData);

                KTPlayerManager.ShowMessageBox(client, "Thông báo", "Đã gửi lời mời vào bang tới [" + RoleIDInvite.RoleName + "]\nVui lòng chờ phản hồi");
                return TCPProcessCmdResults.RESULT_OK;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, Global.GetDebugHelperInfo(socket), false);
            }

            return TCPProcessCmdResults.RESULT_FAILED;
        }

        /// <summary>
        /// 1 thằng trả lời xem có vào bang hay không
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
        public static TCPProcessCmdResults CMD_KT_GUILD_RESPONSEINVITE(TCPManager tcpMgr, TMSKSocket socket, TCPClientPool tcpClientPool, TCPRandKey tcpRandKey, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
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
                LogManager.WriteLog(LogTypes.Error, string.Format("Error while getting DATA, CMD={0}, Client={1}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket)));
                return TCPProcessCmdResults.RESULT_FAILED;
            }

            try
            {
                KPlayer client = KTPlayerManager.Find(socket);
                if (null == client)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Can not find player corresponding ID, CMD={0}, Client={1}, RoleID={2}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), client.RoleID));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                string[] fields = cmdData.Split(':');
                if (fields.Length != 3)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Có lỗi ở tham số gửi lên, CMD={0}, Client={1}, Recv={2}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), fields.Length));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                // Gửi lên 3 tham số 1 có đồng ý hay ko \ 2 ID thằng bagn chủ | 3 là ID bang của thằng bagn chủ
                //
                // Đống ý hay ko
                int Appect = Int32.Parse(fields[0]);

                //ID CỦA BAGN CHỦ
                int RoleID = Int32.Parse(fields[1]);

                // ID của bang xin vào
                int IDGuild = Int32.Parse(fields[2]);

                KPlayer clientBangchu = KTPlayerManager.Find(RoleID);
                if (null == clientBangchu)
                {
                    KTPlayerManager.ShowMessageBox(client, "Thông báo", "Đối phương đã rời mạng");
                    return TCPProcessCmdResults.RESULT_OK;
                }

                // Nếu là từ chối vào bang
                if (Appect == 0)
                {
                    KTPlayerManager.ShowMessageBox(clientBangchu, "Thông báo", "Người chơi từ chối gia nhập bang hội");
                    return TCPProcessCmdResults.RESULT_OK;
                }
                else
                {
                    // String Build để gửi vào GAMEDB
                    string CMDBUILD = client.RoleID + ":" + IDGuild;

                    byte[] ByteSendToDB = Encoding.ASCII.GetBytes(CMDBUILD);
                    // Nếu là đồng ý thì gửi vào trong gameDB
                    TCPProcessCmdResults result = Global.TransferRequestToDBServer(tcpMgr, socket, tcpClientPool, tcpRandKey, pool, nID, ByteSendToDB, count, out TCPOutPacket tcpOutPacket2, client.ServerId);

                    string strCmdResult2 = null;

                    if (tcpOutPacket2 != null)
                    {
                        tcpOutPacket2.GetPacketCmdData(out strCmdResult2);

                        string[] Pram2 = strCmdResult2.Split(':');

                        int Status = Int32.Parse(Pram2[0]);

                        if (Status == 0)
                        {
                            _TotalGuild.TryGetValue(IDGuild, out MiniGuildInfo find);

                            // MiniGuildInfo find = _TotalGuild.Where(x => x.GuildId == IDGuild).FirstOrDefault();

                            if (find != null)
                            {
                                client.GuildID = find.GuildId;
                                client.GuildRank = (int)GuildRank.Member;
                                client.GuildName = find.GuildName;

                                //Update lại title cho thằng bị trục xuất
                                KT_TCPHandler.NotifyOthersMyTitleChanged(client);

                                //Update lại title cho thằng bị trục xuất
                                KT_TCPHandler.NotifyOtherMyGuildRankChanged(client);
                            }

                            KTPlayerManager.ShowMessageBox(clientBangchu, "Thông báo", "Thành viên [" + client.RoleName + "] đã đồng ý gia nhập bang hội");

                            KTPlayerManager.ShowMessageBox(client, "Thông báo", "Gia nhập bang hội [" + client.GuildName + "] thành công!");

                            return TCPProcessCmdResults.RESULT_OK;
                        }
                        else if (Status == -1)
                        {
                            KTPlayerManager.ShowMessageBox(client, "Thông báo", "Thành viên đã có bang hội rồi");
                            return TCPProcessCmdResults.RESULT_OK;
                        }
                        else if (Status == -2)
                        {
                            KTPlayerManager.ShowMessageBox(client, "Thông báo", "Bang hội đã đạt số thành viên tối đa");
                            return TCPProcessCmdResults.RESULT_OK;
                        }
                        else if (Status == -3)
                        {
                            KTPlayerManager.ShowMessageBox(client, "Thông báo", "Bang không tồn tại");
                            return TCPProcessCmdResults.RESULT_OK;
                        }
                        else
                        {
                            KTPlayerManager.ShowMessageBox(client, "Thông báo", "Có lỗi xảy ra khi tham gia bang hội");
                            return TCPProcessCmdResults.RESULT_OK;
                        }
                    }
                }

                return TCPProcessCmdResults.RESULT_DATA;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, Global.GetDebugHelperInfo(socket), false);
            }

            return TCPProcessCmdResults.RESULT_FAILED;
        }

        /// <summary>
        /// Yêu cầu xin vào bang trực tiếp từ phó bang chủ hoặc bang chủ
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
        public static TCPProcessCmdResults CMD_KT_GUILD_ASKJOIN(TCPManager tcpMgr, TMSKSocket socket, TCPClientPool tcpClientPool, TCPRandKey tcpRandKey, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
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
                LogManager.WriteLog(LogTypes.Error, string.Format("Error while getting DATA, CMD={0}, Client={1}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket)));
                return TCPProcessCmdResults.RESULT_FAILED;
            }

            try
            {
                KPlayer client = KTPlayerManager.Find(socket);
                if (null == client)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Can not find player corresponding ID, CMD={0}, Client={1}, RoleID={2}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), client.RoleID));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                string[] fields = cmdData.Split(':');
                if (fields.Length != 2)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Có lỗi ở tham số gửi lên, CMD={0}, Client={1}, Recv={2}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), fields.Length));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                int GUILDWANTJOIN = Int32.Parse(fields[0]);

                int ROLEIDHOST = Int32.Parse(fields[1]);

                KPlayer RoleIDDest = KTPlayerManager.Find(ROLEIDHOST);

                if (RoleIDDest == null)
                {
                    KTPlayerManager.ShowMessageBox(client, "Thông báo", "Đối phượng đã offline");
                    return TCPProcessCmdResults.RESULT_OK;
                }
                else
                {
                    if (RoleIDDest.GuildID <= 0)
                    {
                        KTPlayerManager.ShowMessageBox(client, "Thông báo", "Đối phương không có bang hội");
                        return TCPProcessCmdResults.RESULT_OK;
                    }

                    if (RoleIDDest.GuildRank != (int)GuildRank.Master && RoleIDDest.GuildRank != (int)GuildRank.ViceMaster)
                    {
                        KTPlayerManager.ShowMessageBox(client, "Thông báo", "Đối phương không phải là bang chủ hoặc phó bang chủ.");
                        return TCPProcessCmdResults.RESULT_OK;
                    }

                    string responseData = string.Format("{0}:{1}:{2}:{3}:{4}", client.RoleID, client.RoleName, client.RolePic, client.m_Level, client.m_cPlayerFaction.GetFactionId());

                    // Gửi cho thằng kia biết là đang có người xin vào bang
                    RoleIDDest.SendPacket(nID, responseData);

                    KTPlayerManager.ShowMessageBox(client, "Thông báo", "Đẫ gửi đơn xin gia nhập bang tới " + RoleIDDest.RoleName + "\nVui lòng chờ phản hồi");

                    return TCPProcessCmdResults.RESULT_OK;
                }
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, Global.GetDebugHelperInfo(socket), false);
            }

            return TCPProcessCmdResults.RESULT_FAILED;
        }

        /// <summary>
        /// Bang chủ hoặc phó bang chủ trả lời đơn xin vào của người chơi
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
        public static TCPProcessCmdResults CMD_KT_GUILD_RESPONSEASK(TCPManager tcpMgr, TMSKSocket socket, TCPClientPool tcpClientPool, TCPRandKey tcpRandKey, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
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
                LogManager.WriteLog(LogTypes.Error, string.Format("Error while getting DATA, CMD={0}, Client={1}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket)));
                return TCPProcessCmdResults.RESULT_FAILED;
            }

            try
            {
                KPlayer client = KTPlayerManager.Find(socket);
                if (null == client)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Can not find player corresponding ID, CMD={0}, Client={1}, RoleID={2}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), client.RoleID));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                string[] fields = cmdData.Split(':');
                if (fields.Length != 3)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Có lỗi ở tham số gửi lên, CMD={0}, Client={1}, Recv={2}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), fields.Length));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                if (client.GuildID <= 0)
                {
                    KTPlayerManager.ShowMessageBox(client, "Thông báo", "Bạn không có bang hội");
                    return TCPProcessCmdResults.RESULT_OK;
                }

                if (client.GuildRank != (int)GuildRank.Master && client.GuildRank != (int)GuildRank.ViceMaster)
                {
                    KTPlayerManager.ShowMessageBox(client, "Thông báo", "Chỉ có bang chủ hoặc phó bang chủ mới có quyền trực tiếp phê duyệt thành viên xin gia nhập bang hội.");
                    return TCPProcessCmdResults.RESULT_OK;
                }

                // ĐỒNG Ý HAY KO
                int Appect = Int32.Parse(fields[0]);

                // ID CỦA THẰNG ĐÃ XIN VÀO
                int RoleID = Int32.Parse(fields[1]);

                // ID BANG CỦA BẢN THÂN
                int GuildID = Int32.Parse(fields[2]);

                // nếu là 0 thì là từ chối

                KPlayer WantJoin = KTPlayerManager.Find(RoleID);

                //if (Appect == 0)
                //{
                //    if (WantJoin != null)
                //    {
                //        PlayerManager.ShowMessageBox(WantJoin, "Thông báo", "Đối phương từ chối đơn xin gia nhập bang hội");
                //        return TCPProcessCmdResults.RESULT_OK;
                //    }
                //}
                //else

                TCPProcessCmdResults result = Global.TransferRequestToDBServer(tcpMgr, socket, tcpClientPool, tcpRandKey, pool, nID, data, count, out TCPOutPacket tcpOutPacket2, client.ServerId);

                if (tcpOutPacket2 != null)
                {
                    string strCmdResult2 = null;

                    tcpOutPacket2.GetPacketCmdData(out strCmdResult2);

                    if (strCmdResult2 != null)
                    {
                        string[] Pram2 = strCmdResult2.Split(':');

                        int Status = Int32.Parse(Pram2[0]);

                        if (Status == 0)
                        {
                            // MiniGuildInfo find = _TotalGuild.Where(x => x.GuildId == GuildID).FirstOrDefault();

                            _TotalGuild.TryGetValue(GuildID, out MiniGuildInfo find);
                            if (find != null)
                            {
                                if (WantJoin != null)
                                {
                                    WantJoin.GuildID = find.GuildId;
                                    WantJoin.GuildRank = (int)GuildRank.Member;
                                    WantJoin.GuildName = find.GuildName;

                                    KT_TCPHandler.NotifyOthersMyTitleChanged(WantJoin);

                                    KT_TCPHandler.NotifyOtherMyGuildRankChanged(WantJoin);
                                }

                                KTPlayerManager.ShowMessageBox(client, "Thông báo",
                                    "Người chơi [" + WantJoin.RoleName + "] đã gia nhập bang hội");

                                KTPlayerManager.ShowMessageBox(WantJoin, "Thông báo",
                                    "Gia nhập bang hội [" + client.GuildName + "] thành công!");

                                return TCPProcessCmdResults.RESULT_OK;
                            }
                        }
                        else if (Status == -1)
                        {
                            KTPlayerManager.ShowMessageBox(client, "Thông báo", "Thành viên đã có bang hội rồi");
                            return TCPProcessCmdResults.RESULT_OK;
                        }
                        else if (Status == 10)
                        {
                            if (WantJoin != null)
                            {
                                KTPlayerManager.ShowMessageBox(WantJoin, "Thông báo", "Đối phương từ chối đơn xin gia nhập bang hội");
                                return TCPProcessCmdResults.RESULT_OK;
                            }
                            //PlayerManager.ShowMessageBox(client, "Thông báo", "Thành viên đã có bang hội rồi");
                            //return TCPProcessCmdResults.RESULT_OK;
                        }
                        else if (Status == -2)
                        {
                            KTPlayerManager.ShowMessageBox(client, "Thông báo",
                                "Bang hội đã đạt số thành viên tối đa");
                            return TCPProcessCmdResults.RESULT_OK;
                        }
                        else if (Status == -3)
                        {
                            KTPlayerManager.ShowMessageBox(client, "Thông báo", "Bang không tồn tại");
                            return TCPProcessCmdResults.RESULT_OK;
                        }
                        else
                        {
                            KTPlayerManager.ShowMessageBox(client, "Thông báo",
                                "Có lỗi xảy ra khi tham gia bang hội");
                            return TCPProcessCmdResults.RESULT_OK;
                        }
                    }
                }

                return TCPProcessCmdResults.RESULT_OK;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, Global.GetDebugHelperInfo(socket), false);
            }

            return TCPProcessCmdResults.RESULT_FAILED;
        }

        /// <summary>
        /// Packet thoát khỏi bang hội
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
        public static TCPProcessCmdResults CMD_KT_GUILD_QUIT(TCPManager tcpMgr, TMSKSocket socket, TCPClientPool tcpClientPool, TCPRandKey tcpRandKey, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
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
                LogManager.WriteLog(LogTypes.Error, string.Format("Error while getting DATA, CMD={0}, Client={1}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket)));
                return TCPProcessCmdResults.RESULT_FAILED;
            }

            try
            {
                KPlayer client = KTPlayerManager.Find(socket);
                if (null == client)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Can not find player corresponding ID, CMD={0}, Client={1}, RoleID={2}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), client.RoleID));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                string[] fields = cmdData.Split(':');
                if (fields.Length != 1)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Có lỗi ở tham số gửi lên, CMD={0}, Client={1}, Recv={2}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), fields.Length));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                if (client.GuildRank == (int)GuildRank.Master)
                {
                    KTPlayerManager.ShowMessageBox(client, "Thông báo", "Bạn phải nhường lại bang chủ cho người khác trước khi rời bang");
                    return TCPProcessCmdResults.RESULT_OK;
                }

                int GuildID = Int32.Parse(fields[0]);

                string CMDBUILD = client.RoleID + ":" + client.GuildID + ":" + client.ZoneID;

                byte[] ByteSendToDB = Encoding.ASCII.GetBytes(CMDBUILD);

                TCPProcessCmdResults result = Global.TransferRequestToDBServer(tcpMgr, socket, tcpClientPool, tcpRandKey, pool, nID, ByteSendToDB, count, out tcpOutPacket, client.ServerId);
                if (null != tcpOutPacket)
                {
                    string strCmdResult = null;
                    tcpOutPacket.GetPacketCmdData(out strCmdResult);
                    if (null != strCmdResult)
                    {
                        string[] Pram = strCmdResult.Split(':');

                        int Status = Int32.Parse(Pram[0]);

                        if (Status == 0)
                        {
                            client.GuildRank = 0;
                            client.GuildID = 0;
                            client.GuildName = "";

                            //Update lại title cho thằng bị trục xuất
                            KT_TCPHandler.NotifyOthersMyTitleChanged(client);

                            //Update lại title cho thằng bị trục xuất
                            KT_TCPHandler.NotifyOtherMyGuildRankChanged(client);

                            KTPlayerManager.ShowNotification(client, "Rời bang hội thành công!");

                            // gửi số 1 này về để reload lại khung bang hội
                            string responseData = "1";
                            tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, responseData, nID);
                            return TCPProcessCmdResults.RESULT_DATA;
                        }
                        else
                        {
                            KTPlayerManager.ShowMessageBox(client, "Thông báo", "Có lỗi khi trục xuất người chơi");
                            return TCPProcessCmdResults.RESULT_OK;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, Global.GetDebugHelperInfo(socket), false);
            }

            return TCPProcessCmdResults.RESULT_FAILED;
        }

        /// <summary>
        /// Lấy ra thông tin thi công thành chiến
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
        public static TCPProcessCmdResults CMD_KT_GUILD_WARINFO(TCPManager tcpMgr, TMSKSocket socket, TCPClientPool tcpClientPool, TCPRandKey tcpRandKey, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
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
                LogManager.WriteLog(LogTypes.Error, string.Format("Error while getting DATA, CMD={0}, Client={1}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket)));
                return TCPProcessCmdResults.RESULT_FAILED;
            }

            try
            {
                KPlayer client = KTPlayerManager.Find(socket);
                if (null == client)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Can not find player corresponding ID, CMD={0}, Client={1}, RoleID={2}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), client.RoleID));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                if (client.GuildID <= 0)
                {
                    KTPlayerManager.ShowNotification(client, "Bạn không có bang hội");

                    return TCPProcessCmdResults.RESULT_OK;
                }

                GuildWarInfo _guildWarInfo = new GuildWarInfo();
                // Tỉm ra cái bang này
                MiniGuildInfo _GUILDINFO = GuildManager.getInstance()._GetInfoGuildByGuildID(client.GuildID);

                if (_GUILDINFO != null)
                {
                    if (GuildWarCity.getInstance().IsTimeRegisterFight())
                    {
                        _guildWarInfo.Status = 0;
                    }
                    else
                    {
                        _guildWarInfo.Status = 1;
                    }

                    // Nếu có bang này
                    if (_GUILDINFO.GuildWar != null)
                    {
                        // Tạo mới 1 danh sách đăng ký
                        _guildWarInfo.MemberRegister = new List<string>();

                        if (_GUILDINFO.GuildWar.TeamList != null)
                        {
                            //Xem có thành viên đăng ký ko
                            if (_GUILDINFO.GuildWar.TeamList.Length > 0)
                            {
                                // Lấy ra danh sách danh đăng ký
                                string[] MemberList = _GUILDINFO.GuildWar.TeamList.Split('_');
                                foreach (string Member in MemberList)
                                {
                                    string[] MemberPram = Member.Split('|');
                                    //Đút vào list
                                    _guildWarInfo.MemberRegister.Add(MemberPram[1]);
                                }
                            }
                        }
                    }

                    //Xem có những bang nào đăng ký
                    _guildWarInfo.GuildResgister = new List<string>();

                    // Check toàn bộ danh sách các bang hiện tại
                    foreach (MiniGuildInfo _Info in GuildManager.getInstance().GetTotalGuildInfo())
                    {
                        // Xem thằng nào đăng ký thành công
                        if (_Info.IsMainCity == (int)GUILD_CITY_STATUS.REGISTERFIGHT)
                        {
                            // Thì đúng vào dánh sách
                            _guildWarInfo.GuildResgister.Add(_Info.GuildName);
                        }
                    }
                }

                //Set cho map nào active war trong đợt công tuần này
                _guildWarInfo.CityFightID = GuildWarCity._WarConfig.Citys[0].CityID;

                //List danh sách thành sẽ diễn ra công thành
                _guildWarInfo.ListCity = new List<CityInfo>();

                CityInfo _City = new CityInfo();

                // CTC vào ngày àno
                _City.CityFightDay = GuildWarCity._WarConfig.Citys[0].CityFightDay;
                //Lên lôi đài vào ngày nào
                _City.TeamFightDay = GuildWarCity._WarConfig.Citys[0].TeamFightDay;

                // Tìm xem thằng chủ thành là thằng nào
                MiniGuildInfo _Host = GuildManager.getInstance().GetGuildWinHostCity();

                if (_Host != null)
                {
                    // Nếu có chủ thành
                    _City.HostName = _Host.GuildName;
                }
                else
                {
                    // Nếu chưa có chủ thành
                    _City.HostName = "Chưa có bang nào";
                }

                _City.CityName = GuildWarCity._WarConfig.Citys[0].CityName;
                _guildWarInfo.ListCity.Add(_City);

                List<MemberRegister> ListMemberCanPick = new List<MemberRegister>();

                List<KPlayer> _ListPlayerMemberFind = KTPlayerManager.FindAll(x => x.GuildID == client.GuildID);

                foreach (KPlayer Member in _ListPlayerMemberFind)
                {
                    MemberRegister _Member = new MemberRegister();
                    _Member.RoleName = Member.RoleName;
                    _Member.Level = Member.m_Level;
                    _Member.Faction = Member.m_cPlayerFaction.GetFactionId();
                    _Member.RoleID = Member.RoleID;
                    ListMemberCanPick.Add(_Member);
                }

                // Danh sách những người online có thể chọn
                _guildWarInfo.ListMemberCanPick = ListMemberCanPick;

                // Trả về danh sách bang hội hiện tại
                tcpOutPacket = DataHelper.ObjectToTCPOutPacket<GuildWarInfo>(_guildWarInfo, pool, nID);

                return TCPProcessCmdResults.RESULT_DATA;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, Global.GetDebugHelperInfo(socket), false);
            }

            return TCPProcessCmdResults.RESULT_FAILED;
        }

        /// <summary>
        /// Packet đăng ký tham gia thi đấu
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
        public static TCPProcessCmdResults CMD_KT_GUILD_WAR_REGISTER(TCPManager tcpMgr, TMSKSocket socket, TCPClientPool tcpClientPool, TCPRandKey tcpRandKey, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
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
                LogManager.WriteLog(LogTypes.Error, string.Format("Error while getting DATA, CMD={0}, Client={1}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket)));
                return TCPProcessCmdResults.RESULT_FAILED;
            }

            try
            {
                KPlayer client = KTPlayerManager.Find(socket);
                if (null == client)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Can not find player corresponding ID, CMD={0}, Client={1}, RoleID={2}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), client.RoleID));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                if (client.GuildID <= 0)
                {
                    KTPlayerManager.ShowNotification(client, "Bạn không có bang hội");

                    return TCPProcessCmdResults.RESULT_OK;
                }
                // nếu mà đéo phải bang chủ
                if (client.GuildRank != (int)GuildRank.Master)
                {
                    KTPlayerManager.ShowNotification(client, "Bạn không phải là bang chủ");

                    return TCPProcessCmdResults.RESULT_OK;
                }

                if (GameManager.IsKuaFuServer)
                {
                    KTPlayerManager.ShowNotification(client, "Chức năng này không khả dụng ở liên máy chủ");

                    return TCPProcessCmdResults.RESULT_OK;
                }
                string[] fields = cmdData.Split(':');

                // 6 thằng thành viên gửi lên để đăng ký thi đấu
                if (fields.Length != 7)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Có lỗi ở tham số gửi lên, CMD={0}, Client={1}, Recv={2}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), fields.Length));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }
                var Find = GuildManager.getInstance()._GetInfoGuildByGuildID(client.GuildID);
                if (Find != null)
                {
                    if (Find.IsMainCity == (int)GUILD_CITY_STATUS.REGISTERFIGHT)
                    {
                        KTPlayerManager.ShowNotification(client, "Bạn đã đăng ký thi đấu rồi không thể đăng ký tiếp");

                        return TCPProcessCmdResults.RESULT_OK;
                    }

                    if (Find.IsMainCity == (int)GUILD_CITY_STATUS.HOSTCITY)
                    {
                        KTPlayerManager.ShowNotification(client, "Chủ thành thì không thể đăng ký công thành");

                        return TCPProcessCmdResults.RESULT_OK;
                    }
                }
                else
                {
                    KTPlayerManager.ShowNotification(client, "Có lỗi khi thi lấy dữ liệu bang hội");

                    return TCPProcessCmdResults.RESULT_OK;
                }

                if (!GuildWarCity.getInstance().IsTimeRegisterFight())
                {
                    KTPlayerManager.ShowNotification(client, "Đây không phải thời gian đăng ký lôi đài");

                    return TCPProcessCmdResults.RESULT_OK;
                }

                // Gửi gói tin này vào gameDB
                byte[] ByteSendToDB = Encoding.ASCII.GetBytes(cmdData);

                TCPProcessCmdResults result = Global.TransferRequestToDBServer(tcpMgr, socket, tcpClientPool, tcpRandKey, pool, nID, ByteSendToDB, count, out tcpOutPacket, client.ServerId);
                if (null != tcpOutPacket)
                {
                    string strCmdResult = null;
                    tcpOutPacket.GetPacketCmdData(out strCmdResult);
                    if (null != strCmdResult)
                    {
                        string[] Pram = strCmdResult.Split(':');

                        int Status = Int32.Parse(Pram[0]);
                        // Nếu như status = 0 tức là insert thành công
                        if (Status == 0)
                        {
                            int WEEKID = TimeUtil.GetIso8601WeekOfYear(DateTime.Now);
                            // Set thằng này đã đăng ký thi đấu
                            Find.IsMainCity = 2;
                            if (Find.GuildWar == null)
                            {
                                Find.GuildWar = new GuildWar();
                            }
                            Find.GuildWar.TeamList = Pram[2];
                            Find.GuildWar.GuildName = Find.GuildName;

                            Find.GuildWar.WeekID = WEEKID;
                            Find.GuildWar.GuildID = Find.GuildId;

                            KTPlayerManager.ShowMessageBox(client, "Thông báo", "Đăng ký thi đấu lôi đài cho bang hội thành công!");

                            return TCPProcessCmdResults.RESULT_OK;
                        }
                        else
                        {
                            KTPlayerManager.ShowMessageBox(client, "Thông báo", "Có lỗi khi đăng ký lôi đài công thành chiến");
                            return TCPProcessCmdResults.RESULT_OK;
                        }
                    }
                }

                // trả vào DB xử lý
                // return Global.TransferRequestToDBServer(tcpMgr, socket, tcpClientPool, tcpRandKey, pool, nID, data, count, out tcpOutPacket, client.ServerId);
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, Global.GetDebugHelperInfo(socket), false);
            }

            return TCPProcessCmdResults.RESULT_FAILED;
        }

        /// <summary>
        /// Mở shop bang hội
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
        public static TCPProcessCmdResults CMD_KT_GUILD_OPENSHOP(TCPManager tcpMgr, TMSKSocket socket, TCPClientPool tcpClientPool, TCPRandKey tcpRandKey, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
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
                LogManager.WriteLog(LogTypes.Error, string.Format("Error while getting DATA, CMD={0}, Client={1}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket)));
                return TCPProcessCmdResults.RESULT_FAILED;
            }

            try
            {
                KPlayer client = KTPlayerManager.Find(socket);
                if (null == client)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Can not find player corresponding ID, CMD={0}, Client={1}, RoleID={2}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), client.RoleID));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                if (client.GuildID <= 0)
                {
                    KTPlayerManager.ShowNotification(client, "Bạn không có bang hội");

                    return TCPProcessCmdResults.RESULT_OK;
                }

                if (GameManager.IsKuaFuServer)
                {
                    KTPlayerManager.ShowNotification(client, "Chức năng này không khả dụng ở liên máy chủ");

                    return TCPProcessCmdResults.RESULT_OK;
                }
                // nếu mà đéo phải bang chủ

                ShopTab _Shop = ShopManager.GetShopTable(226, client);

                if (_Shop != null)
                {
                    KT_TCPHandler.SendShopData(client, _Shop);
                }
                else
                {
                    KTPlayerManager.ShowNotification(client, "Cửa hàng bạn tìm không tồn tại");
                }

                return TCPProcessCmdResults.RESULT_OK;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, Global.GetDebugHelperInfo(socket), false);
            }

            return TCPProcessCmdResults.RESULT_FAILED;
        }
    }
}