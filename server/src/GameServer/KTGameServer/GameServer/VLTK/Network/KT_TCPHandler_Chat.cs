using GameServer.KiemThe.Core.Item;
using GameServer.KiemThe.Entities;
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
using System.Text.RegularExpressions;
using static GameServer.Logic.KTMapManager;

namespace GameServer.KiemThe.Logic
{
    /// <summary>
    /// Quản lý Chat
    /// </summary>
    public static partial class KT_TCPHandler
    {
        /// <summary>
        /// Nhận gói tin thông báo người chơi thực hiện Chat
        /// </summary>
        /// <param name="pool"></param>
        /// <param name="nID"></param>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static TCPProcessCmdResults ProcessSpriteChatCmd(TCPManager tcpMgr, TMSKSocket socket, TCPClientPool tcpClientPool, TCPRandKey tcpRandKey, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;
            SpriteChat cmdData = null;

            try
            {
                cmdData = DataHelper.BytesToObject<SpriteChat>(data, 0, count);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Decode data faild, CMD={0}, Client={1}", (TCPGameServerCmds) nID, Global.GetSocketRemoteEndPoint(socket)));
                return TCPProcessCmdResults.RESULT_FAILED;
            }

            try
            {
                KPlayer client = KTPlayerManager.Find(socket);
                if (null == client)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Can not find player corresponding ID, CMD={0}, Client={1}, RoleID={2}", (TCPGameServerCmds) nID, Global.GetSocketRemoteEndPoint(socket), client.RoleID));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// Kênh chat
                ChatChannel channel = (ChatChannel) cmdData.Channel;
                /// Nội dung chat
                string content = cmdData.Content;
                /// Xóa thẻ HTML
                content = Utils.RemoveAllHTMLTags(content);

                /// Số lần đã lặp
                int nLoop = 0;
                /// Khôi phục các thẻ HTML chứa biểu cảm
                do
                {
                    /// Tăng số lần đã lặp
                    nLoop++;
                    /// Nếu quá 100 lần thì toác
                    if (nLoop >= 100)
                    {
                        break;
                    }

                    Match match = Regex.Match(content, @"#(\d+)");
                    if (match.Groups.Count <= 1 || string.IsNullOrEmpty(match.Groups[0].Value))
                    {
                        break;
                    }
                    try
                    {
                        string emojiID = match.Groups[1].Value;
                        content = content.Replace(match.Groups[0].Value, string.Format("<sprite={0}>", emojiID));
                    }
                    catch (Exception)
                    {
                        break;
                    }
                }
                while (true);

                /// Số lần đã lặp
                nLoop = 0;
                /// Khôi phục các thẻ ghi vị trí
                do
                {
                    /// Tăng số lần đã lặp
                    nLoop++;
                    /// Nếu quá 10 lần thì toác
                    if (nLoop >= 10)
                    {
                        break;
                    }

                    Match match = Regex.Match(content, @"\@GOTO_(\d+)_(\d+)_(\d+)");
                    if (match.Groups.Count <= 1 || string.IsNullOrEmpty(match.Groups[0].Value))
                    {
                        break;
                    }
                    try
                    {
                        int mapCode = int.Parse(match.Groups[1].Value);
                        int posX = int.Parse(match.Groups[2].Value);
                        int posY = int.Parse(match.Groups[3].Value);

                        /// Bản đồ tương ứng
                        GameMap gameMap = KTMapManager.Find(mapCode);
                        /// Nếu toác
                        if (gameMap == null)
                        {
                            content = content.Replace(match.Groups[0].Value, "");
                        }
                        else
                        {
                            string mapName = gameMap.MapName;
                            UnityEngine.Vector2 gridPos = KTGlobal.WorldPositionToGridPosition(gameMap, new UnityEngine.Vector2(posX, posY));
                            int gridPosX = (int) gridPos.x;
                            int gridPosY = (int) gridPos.y;
                            content = content.Replace(match.Groups[0].Value, string.Format("<color=#3dfff9><link=\"GoTo_{0}_{1}_{2}\">[{3} ({4}, {5})]</link></color>", mapCode, posX, posY, mapName, gridPosX, gridPosY));
                        }
                    }
                    catch (Exception)
                    {
                        break;
                    }
                }
                while (true);

                /// Khôi phục thẻ Chat Voice
                {
                    Match match = Regex.Match(content, @"\@VOICE_(\w+)");
                    if (match.Groups.Count > 1 && !string.IsNullOrEmpty(match.Groups[0].Value))
                    {
                        try
                        {
                            string chatID = match.Groups[1].Value;
                            content = content.Replace(match.Groups[0].Value, string.Format("<link=\"VoiceChat_{0}\"><color=#6b6bff>[Tin nhắn thoại]</color> <sprite name=\"3929-1\">.</link>", chatID));
                        }
                        catch (Exception) { }
                    }
                }

                /// Nếu bị cấm Chat
                if (client.IsBannedChat)
                {
                    /// Thời gian còn lại
                    long timeLeft = client.BanChatDuration - (KTGlobal.GetCurrentTimeMilis() - client.BanChatStartTime);
                    string secLeft = client.BanChatDuration == -1 ? "vĩnh viễn" : KTGlobal.DisplayFullDateAndTime(timeLeft / 1000);
                    string strinfo = string.Format("<color=red>Xin lỗi, bạn đang bị cấm Chat, thời gian còn lại <color=yellow>{0}</color>.</color>", secLeft);
                    KTGlobal.SendDefaultChat(client, strinfo);
                    return TCPProcessCmdResults.RESULT_OK;
                }
                /// Nếu không thể thực hiện Chat lúc này
                else if (!client.CanChat(channel, out long tickLeft))
                {
                    string strinfo = "<color=red>Bạn hiện không thể gửi tin nhắn, hãy liên lạc với hỗ trợ viên để được xử lý.</color>";
                    if (tickLeft != 99999999)
                    {
                        strinfo = string.Format("<color=red>Hiện không thể gửi tin nhắn ở kênh này, cần chờ sau <color=yellow>{0}</color> nữa mới có thể tiếp tục gửi.</color>", KTGlobal.DisplayTime(tickLeft / 1000f));
                    }
                    KTGlobal.SendDefaultChat(client, strinfo);
                    return TCPProcessCmdResults.RESULT_OK;
                }
                /// Nếu không có nội dung Chat
                else if (string.IsNullOrEmpty(content))
                {
                    string strinfo = "<color=red>Không thể gửi tin nhắn khi không có nội dung.</color>";
                    KTGlobal.SendDefaultChat(client, strinfo);
                    return TCPProcessCmdResults.RESULT_OK;
                }
                /// Nếu là Chat nhóm nhưng lại không có nhóm
                else if (cmdData.Channel == (int) ChatChannel.Team && (client.TeamID == -1 || !KTTeamManager.IsTeamExist(client.TeamID)))
                {
                    string strinfo = "<color=red>Không có đội ngũ, không thể gửi tin nhắn.</color>";
                    KTGlobal.SendDefaultChat(client, strinfo);
                    return TCPProcessCmdResults.RESULT_OK;
                }

                /// Nếu là chat thế giới
                else if (cmdData.Channel == (int) ChatChannel.Global && client.m_Level < 20)
                {
                    string strinfo = "<color=red>Cấp 20 trở lên mới có thể chat thế giới.</color>";
                    KTGlobal.SendDefaultChat(client, strinfo);
                    return TCPProcessCmdResults.RESULT_OK;
                }
                /// Nếu là chat kênh đặc biệt
                else if (cmdData.Channel == (int) ChatChannel.Special)
                {
                    ///// Nếu không phải GM
                    //if (!KTGMCommandManager.IsGM(client))
                    //{
                    //    string strinfo = "Chức năng tạm khóa!";
                    //    PlayerManager.ShowNotification(client, strinfo);
                    //    return TCPProcessCmdResults.RESULT_OK;
                    //}

                    /// Kiểm tra xem có vật phẩm Ốc biển truyền thanh (tiểu) không
                    GoodsData speakerItem = client.GoodsData.Find(x => x.Site == 0 && KTGlobal.SpecialChatMaterial.Contains(x.GoodsID));
                    /// Nếu không có vật phẩm
                    if (speakerItem == null)
                    {
                        string strinfo = "Không có vật phẩm [Ốc biển truyền thanh (tiểu)], không thể Chat ở kênh đặc biệt!";
                        KTPlayerManager.ShowNotification(client, strinfo);
                        return TCPProcessCmdResults.RESULT_OK;
                    }
                    /// TODO thực hiện xóa vật phẩm Ốc biển truyền thanh (tiểu)
                    ItemManager.RemoveItemByCount(client, speakerItem, 1, "CHAT");
                }
                /// Nếu là kênh chat liên máy chủ
                else if (cmdData.Channel == (int) ChatChannel.KuaFuLine)
                {
                    /// Kiểm tra xem có vật phẩm Ốc biển truyền thanh (trung) không
                    GoodsData speakerItem = client.GoodsData.Find(x => x.Site == 0 && KTGlobal.CrossServerChatMaterial.Contains(x.GoodsID));
                    /// Nếu không có vật phẩm
                    if (speakerItem == null)
                    {
                        string strinfo = "Không có vật phẩm [Ốc biển truyền thanh (trung)], không thể Chat ở kênh đặc biệt!";
                        KTPlayerManager.ShowNotification(client, strinfo);
                        return TCPProcessCmdResults.RESULT_OK;
                    }
                    /// TODO thực hiện xóa vật phẩm Ốc biển truyền thanh (trung)
                    ItemManager.RemoveItemByCount(client, speakerItem, 1, "CHAT");
                }

                /// Lọc nội dung Chat
                content = KTChatFilter.Filter(content);

                /// Nội dung Chat
                StringBuilder contentString = new StringBuilder(content);
                /// Danh sách vật phẩm đính kèm
                List<GoodsData> items = null;
                if (cmdData.Items != null && cmdData.Items.Count > 0)
                {
                    /// Nếu vật phẩm tồn tại trong túi người chơi thì mới cho lấy thông tin
                    items = client.GoodsData.FindAll(x => x.Site == 0 && cmdData.Items.Any(y => y.Id == x.Id));
                    /// Giới hạn Client truyền lên chỉ được 1 vật phẩm duy nhất
                    items = items.Take(1).ToList();

                    /// Nếu tồn tại danh sách đính kèm
                    if (items != null && items.Count > 0)
                    {
                        List<string> itemStrings = new List<string>();
                        foreach (GoodsData itemGD in items)
                        {
                            itemStrings.Add(KTGlobal.GetItemDescInfoStringForChat(itemGD));
                        }
                        contentString.AppendLine();
                        contentString.AppendLine(string.Format("<color=#a8ecff>Vật phẩm đính kèm:</color> {0}", string.Join(", ", itemStrings)));
                    }
                }

                /// Danh sách pet đính kèm
                List<PetData> pets = null;
                if (cmdData.Pets != null && cmdData.Pets.Count > 0)
                {
                    /// Thông tin pet tương ứng của người chơi
                    pets = client.PetList?.Where(x => cmdData.Pets.Any(y => y.ID == x.ID)).Select(x => KTPetManager.ClonePetData(x)).ToList();
                    /// Duyệt danh sách
                    for (int i = 0; i < pets.Count; i++)
                    {
                        /// Thông tin pet và các chỉ số
                        pets[i] = KTPetManager.GetPetDataWithAttributes(pets[i], client);
                    }
                    /// Giới hạn Client truyền lên chỉ được 1 pet duy nhất
                    pets = pets.Take(1).ToList();

                    /// Nếu tồn tại danh sách đính kèm
                    if (pets != null && pets.Count > 0)
                    {
                        List<string> petDataStrings = new List<string>();
                        foreach (PetData petData in pets)
                        {
                            petDataStrings.Add(KTGlobal.GetPetDescInfoStringForChat(petData));
                        }
                        contentString.AppendLine();
                        contentString.AppendLine(string.Format("<color=#a8ecff>Tinh linh đính kèm:</color> {0}", string.Join(", ", petDataStrings)));
                    }
                }

                switch (cmdData.Channel)
                {
                    case (int) ChatChannel.Near:
                    {
                        List<KPlayer> playersAround = KTGlobal.GetNearByObjectsAtPos<KPlayer>(client.CurrentMapCode, client.CurrentCopyMapID, new UnityEngine.Vector2((int) client.CurrentPos.X, (int) client.CurrentPos.Y), 1000);
                        foreach (KPlayer player in playersAround)
                        {
                            KT_TCPHandler.SendChatMessage(player, client, player, contentString.ToString(), channel, items, pets);
                        }
                        break;
                    }
                    case (int) ChatChannel.Team:
                    {
                        List<KPlayer> teammates = client.Teammates;
                        foreach (KPlayer player in teammates)
                        {
                            KT_TCPHandler.SendChatMessage(player, client, player, contentString.ToString(), channel, items, pets);
                        }
                        break;
                    }
                    case (int) ChatChannel.Faction:
                    {
                        KTGlobal.SendFactionChat(client.m_cPlayerFaction.GetFactionId(), contentString.ToString(), client);
                        break;
                    }
                    case (int) ChatChannel.Guild:
                    {
                        KTGlobal.SendGuildChat(client.GuildID, contentString.ToString(), client);

                        // Thực hiện gửi sang liên máy chủ
                        KT_TCPHandler.ProseccChatKuaFu(client.RoleID, client.RoleName, 1, cmdData.ToRoleName, cmdData.Channel, contentString.ToString(), client.GuildID, client.ServerId);

                        break;
                    }
                    case (int) ChatChannel.Allies:
                    {
                        //KTGlobal.SendFamilyChat(Global._TCPManager.MySocketListener, Global._TCPManager.TcpOutPacketPool, client.FamilyID, contentString.ToString(), client);

                        //// Thực hiện gửi sang liên máy chủ
                        //KT_TCPHandler.ProseccChatKuaFu(client.RoleID, client.RoleName, 1, cmdData.ToRoleName, cmdData.Channel, contentString.ToString(), client.FamilyID, client.ServerId);

                        break;
                    }
                    case (int) ChatChannel.Global:
                    case (int) ChatChannel.Special:
                    {
                        /// Danh sách người chơi
                        List<KPlayer> players = KTPlayerManager.GetAll();
                        /// Duyệt danh sách
                        foreach (KPlayer player in players)
                        {
                            KT_TCPHandler.SendChatMessage(player, client, player, contentString.ToString(), channel, items, pets);
                        }

                        // KT_TCPHandler.ProseccChatKuaFu(client.RoleID, client.RoleName, 1, cmdData.ToRoleName, (int)ChatChannel.KuaFuLine, contentString.ToString(), 0, client.ServerId);

                        break;
                    }

                    // Nếu là chát liên máy chủ thì gửi tới cho toàn bộ các sv khác cùng NHÓM
                    case (int) ChatChannel.KuaFuLine:
                    {
                        //int idx = 0;
                        //KPlayer player = null;
                        //// Thực hiện gửi cho toàn bộ máy chủ bên mình trước

                        //while ((player = GameManager.ClientMgr.GetNextClient(ref idx)) != null)
                        //{
                        //    KT_TCPHandler.SendChatMessage(player, client, player, contentString.ToString(), channel, items, pets);
                        //}
                        string RoleName = KTGlobal.FormatRoleNameWithZoneId(client);
                        // Thực hiện gửi packet tới gamedb để gửi sang cho liên máy chủ
                        KT_TCPHandler.ProseccChatKuaFu(client.RoleID, RoleName, 1, cmdData.ToRoleName, cmdData.Channel, contentString.ToString(), 0, client.ServerId);

                        break;
                    }
                    case (int) ChatChannel.Private:
                    {
                        string playerName = cmdData.ToRoleName;
                        KPlayer player = KTPlayerManager.Find(playerName);
                        if (player == null)
                        {
                            string strinfo = "<color=red>Người chơi không tồn tại hoặc đã rời mạng, không thể gửi tin nhắn.</color>";
                            KTGlobal.SendDefaultChat(client, strinfo);
                            break;
                        }

                        KT_TCPHandler.SendChatMessage(client, client, player, contentString.ToString(), channel, items, pets);
                        KT_TCPHandler.SendChatMessage(player, client, player, contentString.ToString(), channel, items, pets);
                        break;
                    }
                }
                /// Lưu thời gian Chat ở kênh tương ứng
                client.RecordChatTick(channel);

                return TCPProcessCmdResults.RESULT_OK;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, Global.GetDebugHelperInfo(socket), false);
            }

            return TCPProcessCmdResults.RESULT_FAILED;
        }

        /// <summary>
        /// Xử lý chat liên máy chủ
        /// Add 26/8/2021
        /// </summary>
        public static void ProseccChatKuaFu(int roleID, string roleName, int status, string toRoleName, int index, string textMsg, int chatType, int ServerID)
        {
            string ChatData = roleID + ":" + roleName + ":" + status + ":" + toRoleName + ":" + index + ":" + DataHelper.EncodeBase64(textMsg) + ":" + chatType;
            Global.ExecuteDBCmd((int) TCPGameServerCmds.CMD_SPR_CHAT, string.Format("{0}:{1}:{2}", ChatData, chatType, GameManager.ServerLineIdAllLineExcludeSelf), ServerID);
        }

        /// <summary>
        /// Gửi tin nhắn tới người chơi tương ứng
        /// </summary>
        /// <param name="client">Đối tượng được gửi gói tin về</param>
        /// <param name="fromClient"></param>
        /// <param name="toClient"></param>
        /// <param name="content"></param>
        /// <param name="channel"></param>
        public static void SendChatMessage(KPlayer client, KPlayer fromClient, KPlayer toClient, string content, ChatChannel channel, List<GoodsData> items, List<PetData> pets)
        {
            /// Tên thằng gửi tin nhắn
            string fromRoleName = "";
            /// Nếu có thằng gửi
            if (fromClient != null)
            {
                /// Lấy tên mặc định
                fromRoleName = fromClient.RoleName;
                /// Nếu là GM
                if (KTGMCommandManager.IsGM(client))
                {
                    /// Ghi kèm RoleID vào
                    fromRoleName = string.Format("{0} (ID: {1})", fromClient.RoleName, fromClient.RoleID);
                }
            }

            SpriteChat chat = new SpriteChat()
            {
                FromRoleID = fromClient == null ? -1 : fromClient.RoleID,
                FromRoleName = fromRoleName,
                ToRoleName = toClient.RoleName,
                Channel = (int) channel,
                Content = content,
                Items = items,
                Pets = pets,
            };
            client.SendPacket<SpriteChat>((int) TCPGameServerCmds.CMD_SPR_CHAT, chat);
        }

        /// <summary>
        /// Gửi tin nhắn tới người chơi tương ứng
        /// </summary>
        /// <param name="sl"></param>
        /// <param name="pool"></param>
        /// <param name="client"></param>
        /// <param name="fromClient"></param>
        /// <param name="toClient"></param>
        /// <param name="content"></param>
        /// <param name="channel"></param>
        /// <param name="items"></param>
        /// <param name="pets"></param>
        public static void SendChatMessageWithName(KPlayer client, string fromClient, KPlayer toClient, string content, ChatChannel channel, List<GoodsData> items, List<PetData> pets)
        {
            SpriteChat chat = new SpriteChat()
            {
                FromRoleName = fromClient,
                ToRoleName = toClient.RoleName,
                Channel = (int) channel,
                Content = content,
                Items = items,
            };
            client.SendPacket<SpriteChat>((int) TCPGameServerCmds.CMD_SPR_CHAT, chat);
        }

        #region GetDatabaseServerChat

        /// <summary>
        /// Thời gian xử lý chat từ GameDatabase truyền về
        /// </summary>
        private static long LastTransferTicks = 0;

        /// <summary>
        /// Nhận toàn bộ chat từ Dabase server về
        /// </summary>
        public static void HandleTransferChatMsg()
        {
            long ticks = KTGlobal.GetCurrentTimeMilis();
            if (ticks - KT_TCPHandler.LastTransferTicks < 5000)
            {
                return;
            }
            KT_TCPHandler.LastTransferTicks = ticks;

            string strcmd = "";

            TCPOutPacket tcpOutPacket = null;

            strcmd = string.Format("{0}:{1}:{2}:{3}", GameManager.ServerLineID, KTPlayerManager.GetPlayersCount(), Global.SendServerHeartCount, "");

            Global.SendServerHeartCount++;

            TCPProcessCmdResults dbRequestResult = Global.RequestToDBServer2(Global._TCPManager.tcpClientPool, Global._TCPManager.TcpOutPacketPool, (int) TCPGameServerCmds.CMD_DB_GET_CHATMSGLIST, strcmd, out tcpOutPacket, GameManager.LocalServerId);

            if (dbRequestResult == TCPProcessCmdResults.RESULT_FAILED)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Không kết nối được với DBServer để lấy danh sách thư khi xử lý thư được chuyển tiếp"));
                return;
            }

            if (null == tcpOutPacket)
            {
                return;
            }

            List<string> chatMsgList = DataHelper.BytesToObject<List<string>>(tcpOutPacket.GetPacketBytes(), 6, tcpOutPacket.PacketDataSize - 6);

            Global._TCPManager.TcpOutPacketPool.Push(tcpOutPacket);

            if (null == chatMsgList || chatMsgList.Count <= 0)
            {
                return;
            }

            // Thực hiện chat
            for (int i = 0; i < chatMsgList.Count; i++)
            {
                KT_TCPHandler.TransferChatMsg(chatMsgList[i]);
            }

            chatMsgList = null;
        }

        /// <summary>
        /// Function xử lý chat gửi từ DATABASE QUA
        /// </summary>
        /// <param name="chatMsg"></param>
        public static void TransferChatMsg(string chatMsg)
        {
            try
            {
                string[] fields = chatMsg.Split(':');
                if (fields.Length != 9)
                {
                    return;
                }

                int roleID = Convert.ToInt32(fields[0]);
                string roleName = fields[1];
                int status = Convert.ToInt32(fields[2]);
                string toRoleName = fields[3];
                int index = Convert.ToInt32(fields[4]);
                string textMsg = fields[5];
                int chatType = Convert.ToInt32(fields[6]);
                int extTag1 = Convert.ToInt32(fields[7]);
                int serverLineID = Convert.ToInt32(fields[8]);

                // Dữ liệu chat có mã hóa hay không
                if (status == 1)
                {
                    textMsg = DataHelper.DecodeBase64(textMsg);
                }

                // nếu như máy chủ thực hiện chát lại chính là máy chủ này thì thôi đéo làm gì cả
                if (serverLineID == GameManager.ServerLineID)
                {
                    return;
                }

                // Nếu là lệnh từ DB gửi ra để thực hiện 1 điều gì đó thì đó
                // Như thông báo, add tiền cho ai đó,Reload data với client
                // Đây là nơi trò chuyện với GS hoặc client từ GAMEDB
                if (KTGlobal.ProseccDatabaseServerChat(textMsg, extTag1))
                {
                    return;
                }
                else
                {
                    // Nếu là chát faction
                    if (index == (int) ChatChannel.Faction)
                    {
                        // Thực hiện gửi nó cho các thành viên trong faction của mình
                        KTGlobal.SendFactionChat(extTag1, textMsg);
                    }
                    if (index == (int) ChatChannel.Guild)
                    {
                        // Thực hiện gửi nó cho
                        KTGlobal.SendGuildChat(extTag1, textMsg, null, roleName);
                    }
                    else if (index == (int) ChatChannel.Allies)
                    {
                        ////Gửi chat cho 1 gia tộc nào đó
                        //KTGlobal.SendFamilyChat(Global._TCPManager.MySocketListener, Global._TCPManager.TcpOutPacketPool, extTag1, textMsg, null, roleName);
                    }
                    else if (index == (int) ChatChannel.Private)
                    {
                        // Gửi tin nhắn cá nhân cho nó
                        KTGlobal.SendPrivateChat(extTag1, textMsg);
                    }
                    else if (index == (int) ChatChannel.KuaFuLine)
                    {
                        // Nếu đây là máy chủ liên server thì sẽ thực hiện add msg này vào db cho các db khác với line khác line đã gửi
                        // Nếu đây là máy chủ thông thường thì sẽ thực hiện gửi về client dưới dạng kênh liên máy chủ

                        if (GameManager.IsKuaFuServer)
                        {
                            KuaFuChatData _ChatData = new KuaFuChatData();
                            _ChatData.chatType = chatType;
                            _ChatData.extTag1 = extTag1;
                            _ChatData.index = index;
                            _ChatData.roleID = roleID;
                            _ChatData.roleName = roleName;
                            _ChatData.serverLineID = serverLineID;
                            _ChatData.status = status;
                            _ChatData.textMsg = textMsg;
                            _ChatData.toRoleName = toRoleName;

                            // Nếu đây là liên server thì có nhiệm vụ nói chuyện với các gamedb khác và đẩy tin nhắn này vào cho bên đó xử lý
                            KuaFuManager.getInstance().PushChatData(_ChatData);

                            KTGlobal.SendKuaFuServerChat(textMsg, roleName);
                        }
                        else // nếu đây là 1 server thông thường được link với nó thì thực hiện bắn chat lên
                        {
                            KTGlobal.SendKuaFuServerChat(textMsg, roleName);
                        }
                    }
                    else
                    {
                        LogManager.WriteLog(LogTypes.Data, "ProseccDatabaseServerChat :" + textMsg);
                    }
                }
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, "TransferChatMsg", false);
            }
        }

        #endregion GetDatabaseServerChat
    }
}