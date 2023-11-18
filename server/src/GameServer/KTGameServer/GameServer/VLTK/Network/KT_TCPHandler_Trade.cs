using GameServer.Core.Executor;
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
using System.Windows;
using static GameServer.Logic.KTMapManager;

namespace GameServer.KiemThe.Logic
{
    /// <summary>
    /// Class quản lý việc giao dịch
    /// </summary>
    public static partial class KT_TCPHandler
    {
        #region Giao Dịch
        /// <summary>
        /// Thực hiện yêu cầu giao dịch gì đó
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
        public static TCPProcessCmdResults ProcessSpriteGoodsExchangeCmd(TCPManager tcpMgr, TMSKSocket socket, TCPClientPool tcpClientPool, TCPRandKey tcpRandKey, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;
            string cmdData = null;

            try
            {
                cmdData = new UTF8Encoding().GetString(data, 0, count);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Trade, string.Format("Có lỗi khi gửi yêu cầu gio dịch , CMD={0}, Client={1}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket)));
                return TCPProcessCmdResults.RESULT_FAILED;
            }

            try
            {
                //Kiểm tra chuỗi dữ liệu gửi lên
                string[] fields = cmdData.Split(':');
                if (fields.Length != 4)
                {
                    // Nếu khác 4 thì chim cút
                    LogManager.WriteLog(LogTypes.Trade, string.Format("Chuỗi giao dịch gửi lên không hợp lệ, CMD={0}, Client={1}, Recv={2}",
                        (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), fields.Length));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                int roleID = Convert.ToInt32(fields[0]);

                // Tìm ra thằng muốn giao dịch
                KPlayer client = KTPlayerManager.Find(socket);

                // Nếu mà thằng muốn giao dịch gửi lên không đúng
                if (null == client || client.RoleID != roleID)
                {
                    LogManager.WriteLog(LogTypes.Trade, string.Format("Dữ liệu sai sót khi giao dịch, CMD={0}, Client={1}, RoleID={2}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), roleID));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                if (client.PKMode == (int)PKMode.Custom)
                {
                    KTPlayerManager.ShowNotification(client, "Chế độ chiến đấu không thể giao dịch");
                    return TCPProcessCmdResults.RESULT_OK;
                }

                /// Bản đồ tương ứng
                GameMap map = KTMapManager.Find(client.MapCode);
                /// Nếu bản đồ không cho phép giao dịch
                if (!map.AllowTrade)
                {
                    KTPlayerManager.ShowNotification(client, "Bản đồ hiện tại không cho phép giao dịch!");
                    return TCPProcessCmdResults.RESULT_OK;
                }
               // nếu mà đang khóa
                if (client.NeedToShowInputSecondPassword())
                {
                    return TCPProcessCmdResults.RESULT_OK;
                }

                if (client.ClientSocket.IsKuaFuLogin)
                {
                    return TCPProcessCmdResults.RESULT_OK;
                }

                //Kiểm tra xem có đang cấm giao dịch toàn máy chủ không

                int disableExchange = GameManager.GameConfigMgr.GetGameConfigItemInt("disable-exchange", 0);
                if (disableExchange > 0)
                {   // Nếu cấm giao dịch thì chim cút
                    return TCPProcessCmdResults.RESULT_OK;
                }

                // Tiến hành đọc các PRAMENTER
                int otherRoleID = Convert.ToInt32(fields[1]);
                int exchangeType = Convert.ToInt32(fields[2]);
                int exchangeID = Convert.ToInt32(fields[3]);

                string strcmd = "";

                // Nếu là kiểu yêu cầu
                if (exchangeType == (int)GoodsExchangeCmds.Request)
                {
                    long ticks = TimeUtil.NOW();

                    // Nếu thằng này bị band tra de thì chim cút
                    //if (TradeBlackManager.Instance().IsBanTrade(client.RoleID))
                    //{
                    //    GameManager.ClientMgr.NotifyImportantMsg(tcpMgr.MySocketListener, pool, client, "Bạn đã bị cấm giao dịch", GameInfoTypeIndexes.Error, ShowGameInfoTypes.ErrAndBox);
                    //    return TCPProcessCmdResults.RESULT_OK;
                    //}

                    if (client.IsBannedFeature(RoleBannedFeature.Exchange, out int timeLeftSec))
                    {
                        KTPlayerManager.ShowNotification(client, "Bạn đang bị khóa chức năng giao dịch. Thời gian còn " + KTGlobal.DisplayFullDateAndTime(timeLeftSec) + "!");
                        return TCPProcessCmdResults.RESULT_OK;
                    }

                    //Kiểm tra lại để chắc rằng nó đang không giao dịch với ai khác hoặc đang mời giao dịch mà chưa được trả lời trong 50s
                    if (client.ExchangeID <= 0 || (client.ExchangeID > 0 && (ticks - client.ExchangeTicks) >= (50 * 1000)))
                    {
                        //HỎi thằng kia xem có đồng ý không
                        KPlayer otherClient = KTPlayerManager.Find(otherRoleID);
                        if (null == otherClient) //Nếu không tìm thấy thằng kai đâu
                        {
                            // Thì trả về cho client biết là người chơi này đã off
                            strcmd = string.Format("{0}:{1}:{2}:{3}", -1, roleID, otherRoleID, exchangeType);
                            tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, strcmd, nID);
                            return TCPProcessCmdResults.RESULT_DATA;
                        }

                        // Nếu thằng kia đang giao dịch với ai đó khác thì chim cút
                        if (otherClient.ExchangeID > 0)
                        {
                            // Gửi về là thằng này đang giao dịch với thằng khác rồi -2
                            strcmd = string.Format("{0}:{1}:{2}:{3}", -2, roleID, otherRoleID, exchangeType);
                            tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, strcmd, nID);
                            return TCPProcessCmdResults.RESULT_DATA;
                        }
                        // Nếu 2 thằng đang đứng khác bản đồ thì thông báo về
                        if (otherClient.MapCode != client.MapCode)
                        {
                            // Nếu đang khác bản đồ thì thông báo về mã lỗi -3
                            strcmd = string.Format("{0}:{1}:{2}:{3}", -3, roleID, otherRoleID, exchangeType);
                            tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, strcmd, nID);
                            return TCPProcessCmdResults.RESULT_DATA;
                        }

                        // NẾu 2 thằng đang đứng cách nhau xa quá 500 PIXCEL thì báo về khoảng cách quá xa không thể giao dịch
                        if (KTGlobal.GetDistanceBetweenPoints(new Point(otherClient.PosX, otherClient.PosY), new Point(client.PosX, client.PosY)) > 500)
                        {
                            // Gửi về mã lỗi -4 quá xa đéo thể giao dịch
                            strcmd = string.Format("{0}:{1}:{2}:{3}", -4, roleID, otherRoleID, exchangeType);
                            tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, strcmd, nID);
                            return TCPProcessCmdResults.RESULT_DATA;
                        }
                        //// Nếu mà thằng muốn mời nó bị cấm giao dịch
                        //if (TradeBlackManager.Instance().IsBanTrade(otherClient.RoleID))
                        //{
                        //    GameManager.ClientMgr.NotifyImportantMsg(tcpMgr.MySocketListener, pool, client, "Đối phương bị cấm giao dịch", GameInfoTypeIndexes.Error, ShowGameInfoTypes.ErrAndBox);
                        //    return TCPProcessCmdResults.RESULT_OK;
                        //}

                        // Khởi tạo phiên giao dịch
                        int autoID = KTTradeManager.GetNewID();

                        client.ExchangeID = autoID;
                        client.ExchangeTicks = ticks;

                        //Gửi yêu cầu giao dịch về cho thằng này là toa muốn giao dịch
                        KT_TCPHandler.NotifyGoodsExchangeCmd(roleID, otherRoleID, client, otherClient, autoID, exchangeType);
                    }
                    else
                    {
                        // Bạn đang giao dịch với người khác không thể tiến hành giao dịch tiếp
                        strcmd = string.Format("{0}:{1}:{2}:{3}", -10, roleID, otherRoleID, exchangeType);
                        tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, strcmd, nID);
                        return TCPProcessCmdResults.RESULT_DATA;
                    }
                }
                // Nếu là CMD đồng ý
                else if (exchangeType == (int)GoodsExchangeCmds.Agree)
                {
                    // Nếu bản thân mình chưa có phên giao dịch nào
                    if (client.ExchangeID <= 0)
                    {
                        // Tìm thằng đã mời mình giao dịch
                        KPlayer otherClient = KTPlayerManager.Find(otherRoleID);
                        if (null == otherClient)
                        {
                            // nếu thằng mời mình giao dịch mà offline thì thông báo
                            strcmd = string.Format("{0}:{1}:{2}:{3}", -1, roleID, otherRoleID, exchangeType);
                            tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, strcmd, nID);
                            return TCPProcessCmdResults.RESULT_DATA;
                        }

                        // Nếu thằng mời mình giao dịch lại đang giao dịch với 1 thằng khác thì cũng chim cút
                        if (otherClient.ExchangeID <= 0 || exchangeID != otherClient.ExchangeID)
                        {
                            strcmd = string.Format("{0}:{1}:{2}:{3}", -2, roleID, otherRoleID, exchangeType);
                            tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, strcmd, nID);
                            return TCPProcessCmdResults.RESULT_DATA;
                        }

                        // Nếu thằng mời mình giao dịch mà đứng khác map với mình thì chim cút
                        if (otherClient.MapCode != client.MapCode)
                        {
                            strcmd = string.Format("{0}:{1}:{2}:{3}", -3, roleID, otherRoleID, exchangeType);
                            tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, strcmd, nID);
                            return TCPProcessCmdResults.RESULT_DATA;
                        }
                        // Nếu thằng mời mình giao dịch mà đứng xa quá 500 pixel thì báo lỗi
                        if (KTGlobal.GetDistanceBetweenPoints(new Point(otherClient.PosX, otherClient.PosY), new Point(client.PosX, client.PosY)) > 500)
                        {
                            // 2 thằng đang đứng quá xa
                            strcmd = string.Format("{0}:{1}:{2}:{3}", -4, roleID, otherRoleID, exchangeType);
                            tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, strcmd, nID);
                            return TCPProcessCmdResults.RESULT_DATA;
                        }

                        //Nếu ok tất thì khởi tạo sesssion giao dịch
                        ExchangeData ed = new ExchangeData()
                        {
                            RequestRoleID = otherRoleID,
                            AgreeRoleID = roleID,
                            GoodsDict = new Dictionary<int, List<GoodsData>>(),
                            MoneyDict = new Dictionary<int, int>(),
                            LockDict = new Dictionary<int, int>(),
                            DoneDict = new Dictionary<int, int>(),
                            AddDateTime = TimeUtil.NOW(),
                            Done = 0,
                            YuanBaoDict = new Dictionary<int, int>(),
                        };

                        KTTradeManager.Add(exchangeID, ed);

                        client.ExchangeID = exchangeID;
                        client.ExchangeTicks = 0;

                        //Gửi thông báo tao đồng ý giao dịch về cho cả 2 thằng
                        KT_TCPHandler.NotifyGoodsExchangeCmd(roleID, otherRoleID, client, otherClient, exchangeID, exchangeType);
                    }
                    else
                    {
                        strcmd = string.Format("{0}:{1}:{2}:{3}", -10, roleID, otherRoleID, exchangeType);
                        tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, strcmd, nID);
                        return TCPProcessCmdResults.RESULT_DATA;
                    }
                }
                else if (exchangeType == (int)GoodsExchangeCmds.Refuse) //Nếu nó từ chối giao dịch
                {
                    // Tìm thằng đã mời giao dịch
                    KPlayer otherClient = KTPlayerManager.Find(otherRoleID);
                    if (null == otherClient)
                    {
                        return TCPProcessCmdResults.RESULT_OK;
                    }

                    if (otherClient.ExchangeID <= 0 || otherClient.ExchangeID != exchangeID)
                    {
                        return TCPProcessCmdResults.RESULT_OK;
                    }

                    otherClient.ExchangeID = 0;
                    otherClient.ExchangeTicks = 0;

                    //Thông báo về là nó không đồng ý giao dịch
                    KT_TCPHandler.NotifyGoodsExchangeCmd(roleID, otherRoleID, null, otherClient, exchangeID, exchangeType);
                }
                else if (exchangeType == (int)GoodsExchangeCmds.Cancel)
                {
                    //Đang giao dịch mà hủy giao dịch
                    if (client.ExchangeID > 0 && client.ExchangeID == exchangeID)
                    {
                        // Tìm ra phiên giao dịch của 2 thằng
                        ExchangeData ed = KTTradeManager.Find(exchangeID);
                        if (null != ed)
                        {
                            int done = 0;
                            lock (ed)
                            {
                                done = ed.Done;
                            }

                            if (done <= 0) //Nếu mà chưa hoàn thành giao dịch
                            {
                                //Xóa good datas ra khỏi phiên giao dịch
                                KTTradeManager.Remove(exchangeID);

                                // Khôi phục lại tình trạng giao dịch
                                KTTradeManager.RestoreExchangeData(client, ed);

                                client.ExchangeID = 0; //Reset lại ID
                                client.ExchangeTicks = 0;

                                //Tìm thằng còn lại của phiên giao dịch
                                KPlayer otherClient = KTPlayerManager.Find(otherRoleID);
                                if (null == otherClient)
                                {
                                    return TCPProcessCmdResults.RESULT_OK;
                                }

                                if (otherClient.ExchangeID <= 0 || exchangeID != otherClient.ExchangeID)
                                {
                                    return TCPProcessCmdResults.RESULT_OK;
                                }

                                // Khôi phục lại vật phẩm của thằng còn lại
                                KTTradeManager.RestoreExchangeData(otherClient, ed);

                                otherClient.ExchangeID = 0;
                                otherClient.ExchangeTicks = 0;

                                //Thông báo hủy giao dịch tới cả 2 thằng
                                KT_TCPHandler.NotifyGoodsExchangeCmd(roleID, otherRoleID, null, otherClient, exchangeID, exchangeType);
                            }
                        }
                    }
                }
                // Thêm vật phẩm vào phiên giao dịch
                else if (exchangeType == (int)GoodsExchangeCmds.AddGoods)
                {
                    int addGoodsDbID = exchangeID;

                    //Nếu gói tin là add goolds
                    if (client.ExchangeID > 0)
                    {
                        ExchangeData ed = KTTradeManager.Find(client.ExchangeID);

                        if (null != ed)
                        {
                            //Add vật phẩm sang ô giao dịch
                            KTTradeManager.AddGoodsDataIntoExchangeData(client, addGoodsDbID, ed);

                            //Tìm thằng đang giao dịch cùng
                            KPlayer otherClient = KTPlayerManager.Find(otherRoleID);
                            if (null == otherClient)
                            {
                                return TCPProcessCmdResults.RESULT_OK;
                            }

                            if (otherClient.ExchangeID <= 0 || client.ExchangeID != otherClient.ExchangeID)
                            {
                                return TCPProcessCmdResults.RESULT_OK;
                            }

                            //Gửi về thông tin của cả phiên giao dịch cho cả 2 thằng.Bao gồm đồ update lên lưới
                            KT_TCPHandler.NotifyGoodsExchangeData(client, otherClient, ed);
                        }
                    }
                }

                //Nếu command là ADD PET
                else if (exchangeType == (int)GoodsExchangeCmds.AddPet)
                {
                    int PetDBID = exchangeID;

                    //Nếu gói tin là add goolds
                    if (client.ExchangeID > 0)
                    {
                        ExchangeData ed = KTTradeManager.Find(client.ExchangeID);

                        if (null != ed)
                        {
                            //Add pet sang ô giao dịch
                            KTTradeManager.AddPetDataToSessionTrade(client, PetDBID, ed);

                            //Tìm thằng đang giao dịch cùng
                            KPlayer otherClient = KTPlayerManager.Find(otherRoleID);
                            if (null == otherClient)
                            {
                                return TCPProcessCmdResults.RESULT_OK;
                            }

                            if (otherClient.ExchangeID <= 0 || client.ExchangeID != otherClient.ExchangeID)
                            {
                                return TCPProcessCmdResults.RESULT_OK;
                            }

                            //Gửi về thông tin của cả phiên giao dịch cho cả 2 thằng.Bao gồm đồ update lên lưới
                            KT_TCPHandler.NotifyGoodsExchangeData(client, otherClient, ed);
                        }
                    }
                }


                else if (exchangeType == (int)GoodsExchangeCmds.RemovePet)
                {
                    int PetID = exchangeID;

                    //Tìm phiên giao dịch hiện tại của thằng muốn gỡ
                    if (client.ExchangeID > 0)
                    {
                        ExchangeData ed = KTTradeManager.Find(client.ExchangeID);
                        // nếu tìm thấy phiên giao dịch
                        if (null != ed)
                        {
                            KTTradeManager.RemovePetFromExchangeData(client, PetID, ed);

                            // Tìm thằng đang giao dịch với mình
                            KPlayer otherClient = KTPlayerManager.Find(otherRoleID);
                            if (null == otherClient)
                            {
                                return TCPProcessCmdResults.RESULT_OK;
                            }

                            if (otherClient.ExchangeID <= 0 || client.ExchangeID != otherClient.ExchangeID)
                            {
                                return TCPProcessCmdResults.RESULT_OK;
                            }

                            //Gửi thông báo về cho cả 2 thằng về sự thay đổi của phiên giao dịch
                            KT_TCPHandler.NotifyGoodsExchangeData(client, otherClient, ed);
                        }
                    }
                }
                // Gỡ vật phẩm ra khỏi phiên giao dịch
                else if (exchangeType == (int)GoodsExchangeCmds.RemoveGoods)
                {
                    int addGoodsDbID = exchangeID;

                    //Tìm phiên giao dịch hiện tại của thằng muốn gỡ
                    if (client.ExchangeID > 0)
                    {
                        ExchangeData ed = KTTradeManager.Find(client.ExchangeID);
                        // nếu tìm thấy phiên giao dịch
                        if (null != ed)
                        {
                            KTTradeManager.RemoveGoodsDataFromExchangeData(client, addGoodsDbID, ed);

                            // Tìm thằng đang giao dịch với mình
                            KPlayer otherClient = KTPlayerManager.Find(otherRoleID);
                            if (null == otherClient)
                            {
                                return TCPProcessCmdResults.RESULT_OK;
                            }

                            if (otherClient.ExchangeID <= 0 || client.ExchangeID != otherClient.ExchangeID)
                            {
                                return TCPProcessCmdResults.RESULT_OK;
                            }

                            //Gửi thông báo về cho cả 2 thằng về sự thay đổi của phiên giao dịch
                            KT_TCPHandler.NotifyGoodsExchangeData(client, otherClient, ed);
                        }
                    }
                }
                else if (exchangeType == (int)GoodsExchangeCmds.UpdateMoney) //Update Bạc
                {
                    int updateMoney = exchangeID;
                    updateMoney = Math.Max(updateMoney, 0);
                    updateMoney = Math.Min(updateMoney, client.Money);

                    //Nếu mà có tìm thấy phiên giao dịch
                    if (client.ExchangeID > 0)
                    {
                        // Tìm ra phiên giao dịch
                        ExchangeData ed = KTTradeManager.Find(client.ExchangeID);
                        if (null != ed)
                        {
                            //Update tiền vào phiên giao dịch
                            KTTradeManager.UpdateExchangeDataMoney(client, updateMoney, ed);

                            //Lấy ra thằng còn lại của phiên giao dịch
                            KPlayer otherClient = KTPlayerManager.Find(otherRoleID);
                            if (null == otherClient) // nếu tìm ra thằng còn lại
                            {
                                return TCPProcessCmdResults.RESULT_OK;
                            }

                            if (otherClient.ExchangeID <= 0 || client.ExchangeID != otherClient.ExchangeID) // Nếu tìm thấy phiên giao dịch
                            {
                                return TCPProcessCmdResults.RESULT_OK;
                            }

                            //Gửi thông tin về cho cả 2 thằng về sự thay đổi
                            KT_TCPHandler.NotifyGoodsExchangeData(client, otherClient, ed);
                        }
                    }
                }
                // nếu là khóa giao dịch
                else if (exchangeType == (int)GoodsExchangeCmds.Lock)
                {
                    //Tìm ra phiên giao dịch
                    if (client.ExchangeID > 0 && exchangeID == client.ExchangeID)
                    {
                        ExchangeData ed = KTTradeManager.Find(exchangeID);
                        if (null != ed)
                        {
                            //Tiến hành lock giao dịch
                            KTTradeManager.LockExchangeData(roleID, ed, 1);

                            //Tìm ra thằng còn lại
                            KPlayer otherClient = KTPlayerManager.Find(otherRoleID);
                            if (null == otherClient)
                            {
                                return TCPProcessCmdResults.RESULT_OK;
                            }

                            if (otherClient.ExchangeID <= 0 || exchangeID != otherClient.ExchangeID)
                            {
                                return TCPProcessCmdResults.RESULT_OK;
                            }

                            //Thông báo cho cả 2 thằng về sự thay đổi của việc khóa GIAO DỊCH
                            KT_TCPHandler.NotifyGoodsExchangeData(client, otherClient, ed);
                        }
                    }
                }
                // Nếu cả 2 thằng đều khóa và bắt đầu ấn hoàn tất giao dịch
                else if (exchangeType == (int)GoodsExchangeCmds.Done)
                {
                    if (client.ExchangeID > 0 && exchangeID == client.ExchangeID)
                    {
                        ExchangeData ed = KTTradeManager.Find(exchangeID);
                        if (null != ed)
                        {
                            //Nếu mà cả 2 thằng đã khóa
                            if (KTTradeManager.IsLockExchangeData(roleID, ed) && KTTradeManager.IsLockExchangeData(otherRoleID, ed))
                            {
                                //Nếu  đã hoàn tất giao dịch
                                if (KTTradeManager.DoneExchangeData(roleID, ed))
                                {
                                    //Nếu đã hoàn tất giao dịch
                                    if (KTTradeManager.IsDoneExchangeData(otherRoleID, ed))
                                    {
                                        //Lấy ra thằng còn lại
                                        KPlayer otherClient = KTPlayerManager.Find(otherRoleID);
                                        if (null == otherClient)
                                        {
                                            return TCPProcessCmdResults.RESULT_OK;
                                        }

                                        if (otherClient.ExchangeID <= 0 || exchangeID != otherClient.ExchangeID)
                                        {
                                            return TCPProcessCmdResults.RESULT_OK;
                                        }

                                        lock (ed)
                                        {
                                            ed.Done = 1; //Set là có hoàn thành
                                        }

                                        // Thực hiện giao dịch cho 2 thằng <Swap trừ tiền>
                                        int ret = KTTradeManager.CompleteExchangeData(client, otherClient, ed);

                                        //Gỡ bỏ phiên giao dịch khỏi con quản lý giao dịch
                                        KTTradeManager.Find(exchangeID);

                                        if (ret < 0)
                                        {
                                            // Nếu giao dịch thất bại thì khôi phục lại vật phẩm
                                            KTTradeManager.RestoreExchangeData(client, ed);

                                            // Nếu giao dịch thất bại thì khôi phục lại vật phẩm
                                            KTTradeManager.RestoreExchangeData(otherClient, ed);
                                        }

                                        otherClient.ExchangeID = 0;
                                        otherClient.ExchangeTicks = 0;

                                        client.ExchangeID = 0;
                                        client.ExchangeTicks = 0;

                                        //Thông báo về cho cả 2 thằng về thông tin phiên giao dịch
                                        KT_TCPHandler.NotifyGoodsExchangeCmd(roleID, otherRoleID, client, otherClient, ret, exchangeType);
                                    }
                                }
                            }
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
        /// Thông báo thông tin giao dịch về client
        /// </summary>
        /// <param name="client"></param>
        public static void NotifyGoodsExchangeCmd(int roleID, int otherRoleID, KPlayer client, KPlayer otherClient, int status, int exchangeType)
        {
            string strcmd = string.Format("{0}:{1}:{2}:{3}", status, roleID, otherRoleID, exchangeType);

            //Gửi về cho thằng gửi lời mời giao dịch
            if (null != client)
            {
                client.SendPacket((int) TCPGameServerCmds.CMD_SPR_GOODSEXCHANGE, strcmd);
            }
            // Gửi cho thằng được mời giao dịch
            if (null != otherClient)
            {
                otherClient.SendPacket((int) TCPGameServerCmds.CMD_SPR_GOODSEXCHANGE, strcmd);
            }
        }

        /// <summary>
        /// Gửi thông tin giao dịch về Client
        /// </summary>
        /// <param name="client"></param>
        public static void NotifyGoodsExchangeData(KPlayer client, KPlayer otherClient, ExchangeData ed)
        {
            byte[] bytesData = null;

            lock (ed)
            {
                bytesData = DataHelper.ObjectToBytes<ExchangeData>(ed);
            }

            client.SendPacket((int) TCPGameServerCmds.CMD_SPR_EXCHANGEDATA, bytesData);
            otherClient.SendPacket((int) TCPGameServerCmds.CMD_SPR_EXCHANGEDATA, bytesData);
        }
        #endregion Giao Dịch
    }
}
