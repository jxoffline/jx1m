using GameServer.KiemThe.Core.Item;
using GameServer.KiemThe.Logic;
using GameServer.Logic;
using GameServer.Server;
using Server.Protocol;
using Server.TCP;
using Server.Tools;
using System;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace GameServer.KiemThe.Core.Activity.LevelUpEvent
{
    /// <summary>
    ///  Sự kiện thăng cấp nhận quà
    ///  Cái này thấy bảo toác khả năng cao là do role pram của nó hoạt động đéo chuẩn
    ///  Chuyển sang kiểu lưu trữ mới
    /// </summary>
    public class LevelUpEventManager
    {
        public static LevelUpGiftConfig _Event = new LevelUpGiftConfig();

        public static string KTEveryDayEvent_XML = "Config/KT_Activity/KTLevelUpEvent.xml";

        public static void Setup()
        {
            string Files = KTGlobal.GetDataPath(KTEveryDayEvent_XML);

            using (var stream = System.IO.File.OpenRead(Files))
            {
                var serializer = new XmlSerializer(typeof(LevelUpGiftConfig));
                _Event = serializer.Deserialize(stream) as LevelUpGiftConfig;
            }
        }

        #region TPC_NETWORK

        public static TCPProcessCmdResults ProcessQueryUpLevelGiftFlagList(TCPManager tcpMgr, TMSKSocket socket, TCPClientPool tcpClientPool, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;
            string cmdData = null;

            try
            {
                cmdData = new UTF8Encoding().GetString(data, 0, count);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Có lỗi ProcessQueryUpLevelGiftFlagList, CMD={0}, Client={1}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket)));
                return TCPProcessCmdResults.RESULT_FAILED;
            }

            try
            {
                string[] fields = cmdData.Split(':');
                if (fields.Length != 1)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Pram send to GS missing, CMD={0}, Client={1}, Recv={2}",
                        (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), fields.Length));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                int roleID = Convert.ToInt32(fields[0]);
                KPlayer client = KTPlayerManager.Find(socket);
                if (null == client || client.RoleID != roleID)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Role not found, CMD={0}, Client={1}, RoleID={2}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), roleID));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                LevelUpGiftConfig levelup = new LevelUpGiftConfig()
                {
                    LevelUpItem = LevelUpEventManager._Event.LevelUpItem,
                    // BUILD BitFlagBuild sau đó gửi về cho client
                    BitFlags = BitFlagBuild(client),
                };
                byte[] _cmdData = DataHelper.ObjectToBytes<LevelUpGiftConfig>(levelup);

                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, _cmdData, nID);

                return TCPProcessCmdResults.RESULT_DATA;
            }
            catch (Exception e)
            {
                LogManager.WriteException(e.ToString());
            }
            return TCPProcessCmdResults.RESULT_FAILED;
        }

        public static bool IsRevice(KPlayer client, string IDSTEP)
        {
            if (client.RoleWelfareData.levelup_step != "NONE")
            {
                string[] TotalStep = client.RoleWelfareData.levelup_step.Split('_');
                // Nếu mà có chứa STEP NÀY
                if (TotalStep.Contains(IDSTEP))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public static string BitFlagBuild(KPlayer client)
        {
            int LevelHienTai = client.m_Level;

            string BUILD = "";

            foreach (var levelUpItem in LevelUpEventManager._Event.LevelUpItem)
            {
                int LevelRequest = levelUpItem.ToLevel;

                // Nếu đã nhận ID này rồi
                if (IsRevice(client, levelUpItem.ID + ""))
                {
                    BUILD += levelUpItem.ID + "_" + "2|";
                }
                else
                {
                    // Nếu chưa thỏa mãn yêu cầu
                    if (LevelRequest > client.m_Level)
                    {
                        BUILD += levelUpItem.ID + "_" + "1|";
                    }
                    else
                    {
                        BUILD += levelUpItem.ID + "_" + "0|";
                    }
                }
            }

            return BUILD.Substring(0,BUILD.Length-1);
        }

        public static TCPProcessCmdResults ProcessGetUpLevelGiftAward(TCPManager tcpMgr, TMSKSocket socket, TCPClientPool tcpClientPool, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;
            string cmdData = null;

            try
            {
                cmdData = new UTF8Encoding().GetString(data, 0, count);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Lỗi khi phân giải dữ liệu, CMD={0}, Client={1}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket)));
                return TCPProcessCmdResults.RESULT_FAILED;
            }

            try
            {
                string[] fields = cmdData.Split(':');
                if (fields.Length != 2)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Pram gửi lên mising, CMD={0}, Client={1}, Recv={2}",
                        (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), fields.Length));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                int roleID = Convert.ToInt32(fields[0]);
                int id = Convert.ToInt32(fields[1]);
                KPlayer client = KTPlayerManager.Find(socket);
                if (null == client || client.RoleID != roleID)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Role ko tìm thấy, CMD={0}, Client={1}, RoleID={2}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), roleID));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }


                if (client.IsSpamClick())
                {
                    KTPlayerManager.ShowNotification(client, "Bạn đang thao tác quá nhanh");

                    return TCPProcessCmdResults.RESULT_DATA;
                }
                else
                {
                    client.SendClick();
                }
                // Lấy ra quà mà nó muốn nhận
                LevelUpItem upLevelItem = GetAllGiftCanGet(id);

                // Nếu có quà có thể nhận và level hiện tại của nó thõa mãn điều kiện muốn nhận cái quà này
                if (null != upLevelItem && client.m_Level >= upLevelItem.ToLevel)
                {
                    // Kiểm tra xem nó đã nhận chưa nếu đã nhận rồi thì chim cút
                    if (IsRevice(client, id + ""))
                    {
                        // Thông báo cho client là đã nhận rồi
                        cmdData = string.Format("{0}:{1}", -103, id);
                        tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, cmdData, nID);
                        return TCPProcessCmdResults.RESULT_DATA;

                    }

                    // Check xem nó có đủ túi đồ để nhận không
                    bool KQ = CanGetAward(upLevelItem, client);
                    if (KQ)
                    {
                        cmdData = string.Format("{0}:{1}", 0, id);
                        tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, cmdData, nID);
                        return TCPProcessCmdResults.RESULT_DATA;
                    }
                    else
                    {
                        cmdData = string.Format("{0}:{1}", -1, id);
                        tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, cmdData, nID);
                        return TCPProcessCmdResults.RESULT_DATA;
                    }


                }
                else
                {
                    cmdData = string.Format("{0}:{1}", -101, id);
                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, cmdData, nID);
                    return TCPProcessCmdResults.RESULT_DATA;
                }
            }
            catch (Exception e)
            {
                LogManager.WriteException(e.ToString());
            }
            return TCPProcessCmdResults.RESULT_FAILED;
        }

        #endregion TPC_NETWORK

        public static LevelUpItem GetAllGiftCanGet(int Index)
        {
            LevelUpItem _Total = new LevelUpItem();

            _Total = _Event.LevelUpItem.Where(x => x.ID == Index).FirstOrDefault();

            return _Total;
        }

        public static bool CanGetAward(LevelUpItem _Gift, KPlayer player)
        {
            string TotalItemCanGet = _Gift.LevelUpGift;

            string[] Pram = TotalItemCanGet.Split('|');

            int COUNT = 10;//Pram.Count();

            if (!KTGlobal.IsHaveSpace(COUNT, player))
            {
                KTPlayerManager.ShowNotification(player, "Cần sắp xếp 10 ô trống trong túi đồ!");
                return false;
            }

            // TODO Thực hiện đánh dấu là thằng này đã nhận mốc này


            if (player.RoleWelfareData.levelup_step != "NONE")
            {
                player.RoleWelfareData.levelup_step = player.RoleWelfareData.levelup_step + "_" + _Gift.ID;
            }
            else
            {
                player.RoleWelfareData.levelup_step = _Gift.ID + "";
            }

            // Thực hiện ghi vào DB
            Global.WriterWelfare(player);

            foreach (string Item in Pram)
            {
                string[] ItemPram = Item.Split(',');

                int ItemID = Int32.Parse(ItemPram[0]);
                int Number = Int32.Parse(ItemPram[1]);

                LogManager.WriteLog(LogTypes.Welfare, "[LevelUPEvent][" + player.RoleID + "] Revice Gift : STEP :" + _Gift.ID + "| ITEMID :" + ItemID + "| ITEMNUM :" + Number);
                // Mặc định toàn bộ vật phẩm nhận từ event này sẽ khóa hết
                if (!ItemManager.CreateItem(Global._TCPManager.TcpOutPacketPool, player, ItemID, Number, 0, "LEVELUPEVENT", false, 1, false, ItemManager.ConstGoodsEndTime, "", -1, "", 0, 1, true))
                {
                    KTPlayerManager.ShowNotification(player, "Có lỗi khi nhận vật phẩm chế tạo");
                }
            }

            return true;
        }
    }
}