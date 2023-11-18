using GameServer.Core.Executor;
using GameServer.KiemThe.Core;
using GameServer.KiemThe.Entities;
using GameServer.KiemThe.Logic;
using GameServer.Logic;
using GameServer.Server;
using Server.Data;
using Server.Protocol;
using Server.TCP;
using Server.Tools;
using System;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

namespace GameServer.KiemThe
{
    /// <summary>
    /// Các phương thức và đối tượng toàn cục của Kiếm Thế
    /// </summary>
    public static partial class KTGlobal
    {
        #region Chat

        /// <summary>
        /// Gửi tin nhắn dưới kênh hệ thống đồng thời hiển thị ở dòng chữ chạy ngang trêu đầu tới toàn bộ người chơi
        /// </summary>
        /// <param name="message"></param>
        /// <param name="items">Danh sách vật phẩm đính kèm</param>
        /// <param name="pets">Danh sách pet đính kèm</param>
        public static void SendSystemEventNotification(string message, List<GoodsData> items = null, List<PetData> pets = null)
        {
            /// Danh sách người chơi
            List<KPlayer> players = KTPlayerManager.GetAll();
            /// Duyệt danh sách
            foreach (KPlayer player in players)
            {
                KT_TCPHandler.SendChatMessage(player, null, player, message, ChatChannel.System_Broad_Chat, items, pets);
            }
        }

        /// <summary>
        /// Gửi tin nhắn tới kênh đội của người chơi tương ứng
        /// </summary>
        /// <param name="player"></param>
        /// <param name="message"></param>
        public static void SendTeamChat(KPlayer player, string message, List<GoodsData> items = null, List<PetData> pets = null)
        {
            /// Nếu không có đội
            if (player.TeamID == -1 || !KTTeamManager.IsTeamExist(player.TeamID))
            {
                /// Bỏ qua
                return;
            }

            /// Duyệt danh sách thành viên
            foreach (KPlayer teammate in player.Teammates)
            {
                /// Gửi tin nhắn đến thằng này
                KT_TCPHandler.SendChatMessage(teammate, player, teammate, message, ChatChannel.Team, items, pets);
            }
        }

        /// <summary>
        /// Gửi tin nhắn chữ đỏ tới người chơi tương ứng
        /// </summary>
        /// <param name="player"></param>
        /// <param name="message"></param>
        public static void SendDefaultChat(KPlayer player, string message)
        {
            string strinfo = string.Format("<color=red>{0}</color>", message);
            KT_TCPHandler.SendChatMessage(player, null, player, strinfo, ChatChannel.Default, null, null);
        }


        public static void SendSystemChatForPlayer(KPlayer client,string Msg)
        {
            KT_TCPHandler.SendChatMessage(client, null, client, Msg, ChatChannel.System, null, null);
        }
        /// <summary>
        /// Gửi tin nhắn dưới kênh hệ thống tới toàn bộ người chơi
        /// </summary>
        /// <param name="message">Tin nhắn</param>
        /// <param name="items">Danh sách vật phẩm đính kèm</param>
        /// <param name="pets">Danh sách pet đính kèm</param>
        public static void SendSystemChat(string message, List<GoodsData> items = null, List<PetData> pets = null)
        {
            /// Danh sách người chơi
            List<KPlayer> players = KTPlayerManager.GetAll();
            /// Duyệt danh sách
            foreach (KPlayer player in players)
            {
                KT_TCPHandler.SendChatMessage(player, null, player, message, ChatChannel.System, items, pets);
            }
        }

        /// <summary>
        /// Xử lý lệnh gửi từ GameDB về
        /// </summary>
        /// <param name="MSG"></param>
        /// <param name="ExTag"></param>
        /// <returns></returns>
        public static bool ProseccDatabaseServerChat(string MSG, int ExTag)
        {
            if (MSG.Contains("|"))
            {
                string[] Prams = MSG.Split('|');

                string CMD = Prams[0];

                switch (CMD)
                {
                    case "ADDBACKHOA":

                        int WeekID = TimeUtil.GetIso8601WeekOfYear(DateTime.Now);

                        int RoleID = Int32.Parse(Prams[1]);
                        string RoleName = Prams[2];
                        int MoneyAdd = Int32.Parse(Prams[3]);
                        string From = Prams[4];

                        KTGlobal.SendMail(null, RoleID, RoleName, "Trao thưởng ưu tú tuần ["+ WeekID + "]", "Trong tuần này bạn đã hoạt động bagn hội rất tốt!Vui lòng nhận phúc lợi đính kèm trong thư", 0, MoneyAdd, 0);

                        LogManager.WriteLog(LogTypes.Guild, "Xử lý thêm bạc khóa thưởng ưu tú cho người chơi :" + RoleID + "|" + RoleName + "| Số bạc :" + MoneyAdd);

                        //// Gửi thông báo tới toàn bang hội
                        //KTGlobal.SendGuildChat(ExTag, "Người chơi [" + RoleName + "] đã nằm trong TOP 5 người chơi ưu tú của bang.Nhận được " + MoneyAdd + " bạc khóa từ Quỹ Thưởng của bang hội", null);

                        break;

                    case "CHANGERANK":

                        int RoleIDDes = Int32.Parse(Prams[1]);
                        int GuildID = Int32.Parse(Prams[2]);
                        int Rank = Int32.Parse(Prams[3]);

                        KPlayer playerFind = KTPlayerManager.Find(RoleIDDes);
                        if (playerFind != null)
                        {
                            playerFind.GuildID = GuildID;

                            playerFind.GuildRank = Rank;
                            /// Thông báo danh hiệu thay đổi
                            KT_TCPHandler.NotifyOthersMyTitleChanged(playerFind);

                            /// Thông báo cập nhật thông tin gia tộc và bang hội
                            KT_TCPHandler.NotifyOtherMyGuildRankChanged(playerFind);
                        }

                        break;
                }

                //Console.WriteLine(MSG);

                return true;
            }
            else
            {
                return false;
            }
        }

        

        /// <summary>
        /// Gửi chat mật cho ai đó
        /// </summary>
        /// <param name="sl"></param>
        /// <param name="pool"></param>
        /// <param name="RoleID"></param>
        /// <param name="msg"></param>
        /// <param name="sender"></param>
        public static void SendPrivateChat(int RoleID, string msg, KPlayer sender = null)
        {
            KPlayer FindPlayer = KTPlayerManager.Find(RoleID);

            if (FindPlayer != null)
            {
                KT_TCPHandler.SendChatMessage(FindPlayer, sender, FindPlayer, msg, ChatChannel.Private, null, null);
            }
        }

        /// <summary>
        /// Chat bang
        /// </summary>
        /// <param name="sl"></param>
        /// <param name="pool"></param>
        /// <param name="GuildID"></param>
        /// <param name="msg"></param>
        /// <param name="sender"></param>
        public static void SendGuildChat(int GuildID, string msg, KPlayer sender = null, string SendName = "", List<GoodsData> Good = null, List<PetData> pets = null)
        {
            if (GuildID <= 0)
            {
                return;
            }

            if (sender != null)
            {
                SendName = sender.RoleName;
            }

            /// Danh sách người chơi
            List<KPlayer> players = KTPlayerManager.FindAll(x => x.GuildID == GuildID);
            /// Duyệt danh sách
            foreach (KPlayer player in players)
            {
                /// Nếu được gửi từ liên máy chủ
                if (sender != null && player.ClientSocket.IsKuaFuLogin)
                {
                    if (sender.ZoneID == player.ZoneID)
                    {
                        KT_TCPHandler.SendChatMessageWithName(player, SendName, player, msg, ChatChannel.Guild, Good, pets);
                    }
                }
                else
                {
                    KT_TCPHandler.SendChatMessageWithName(player, SendName, player, msg, ChatChannel.Guild, Good, pets);
                }
            }
        }

        /// <summary>
        /// Gửi tin nhắn cho phái
        /// </summary>
        /// <param name="sl"></param>
        /// <param name="pool"></param>
        /// <param name="FamilyID"></param>
        /// <param name="cmdText"></param>
        /// <param name="sender"></param>
        public static void SendFactionChat(int FactionID, string msg, KPlayer sender = null)
        {
            if (FactionID <= 0)
            {
                return;
            }

            /// Danh sách người chơi
            List<KPlayer> players = KTPlayerManager.FindAll(x => x.m_cPlayerFaction.GetFactionId() == FactionID);
            /// Duyệt danh sách
            foreach (KPlayer player in players)
            {
                /// Nếu được gửi từ liên máy chủ
                if (sender != null && player.ClientSocket.IsKuaFuLogin)
                {
                    // Check nếu mà cùng máy chủ thì cho phép nhận tin nhắn
                    if (sender.ZoneID == player.ZoneID)
                    {
                        KT_TCPHandler.SendChatMessage(player, sender, player, msg, ChatChannel.Faction, null, null);
                    }
                }
                else
                {
                    KT_TCPHandler.SendChatMessage(player, sender, player, msg, ChatChannel.Faction, null, null);
                }
            }
        }

        /// <summary>
        /// Kênh chát liên máy chủ dữ liệu sễ bắn ra từ GAMEDB
        /// </summary>
        /// <param name="sl"></param>
        /// <param name="pool"></param>
        /// <param name="FamilyID"></param>
        /// <param name="msg"></param>
        /// <param name="sender"></param>
        public static void SendKuaFuServerChat(string msg, string sender = "")
        {
            /// Danh sách người chơi
            List<KPlayer> players = KTPlayerManager.GetAll();
            /// Duyệt danh sách
            foreach (KPlayer player in players)
            {
                KT_TCPHandler.SendChatMessageWithName(player, sender, player, msg, ChatChannel.KuaFuLine, null, null);
            }
        }

        #endregion

        #region Gửi thư
        /// <summary>
        /// Thực hiện gửi thư cho 1 người chơi bất kể offline hay online
        /// </summary>
        /// <param name="goodsData"></param>
        /// <param name="RoleID"></param>
        /// <param name="RoleName"></param>
        /// <param name="Title"></param>
        /// <param name="Content"></param>
        /// <param name="BoundToken"></param>
        /// <param name="BoundMoney"></param>
        /// <returns></returns>
        public static bool SendMail(List<GoodsData> goodsData, int RoleID, string RoleName, string Title, string Content, int BoundToken, int BoundMoney,int MailType)
        {
            string mailGoodsString = "";

            /// Nếu danh sách vật phẩm tồn tại
            if (null != goodsData && goodsData.Count > 0)
            {
                /// Duyệt danh sách vật phẩm
                foreach (GoodsData itemGD in goodsData)
                {
                    mailGoodsString += KTMailManager.GoodsToMailGoodsString(itemGD) + "|";
                }
                /// Xóa ký tự thừa ở cuối
                mailGoodsString = mailGoodsString.Remove(mailGoodsString.Length - 1);
            }

            string strDbCmd = string.Format("{0}:{1}:{2}:{3}:{4}:{5}:{6}:{7}:{8}:{9}", -1, "Hệ thống", RoleID, RoleName, Title, Content, BoundMoney, BoundToken, mailGoodsString,MailType);

            string[] fieldsData = null;
            /// Thực thi gửi thư
            fieldsData = Global.ExecuteDBCmd((int) TCPGameServerCmds.CMD_DB_SENDUSERMAIL, strDbCmd, GameManager.LocalServerId);

            /// Nếu có lỗi gì đó
            if (null == fieldsData || fieldsData.Length != 3)
            {
                return false;
            }

            /// Trả về ID thư
            return true;
        }
        #endregion
    }
}
