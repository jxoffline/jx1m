using GameServer.KiemThe.Core.Item;
using GameServer.KiemThe.Entities;
using GameServer.Logic;
using GameServer.Server;
using Server.Data;
using Server.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GameServer.KiemThe.Logic
{
    /// <summary>
    /// Quản lý người chơi
    /// </summary>
    public static partial class KTPlayerManager
    {
        #region Message Box
        /// <summary>
        /// Hiển thị bảng thông báo
        /// </summary>
        /// <param name="type"></param>
        /// <param name="msgBox"></param>
        private static void ShowMessageBox(KPlayer player, KMessageBox msgBox)
        {
            KT_TCPHandler.SendOpenMessageBox(player, msgBox.ID, msgBox.MessageType, msgBox.Title, msgBox.Text, msgBox.Parameters);
        }

        /// <summary>
        /// Hiện bảng thông báo có Title, Text tương ứng
        /// </summary>
        /// <param name="player"></param>
        /// <param name="title"></param>
        /// <param name="text"></param>
        public static void ShowMessageBox(KPlayer player, string title, string text)
        {
            KTMessageBox msgBox = new KTMessageBox()
            {
                Owner = player,
                Text = text,
                Title = title,
                MessageType = 0,
                OK = null,
                Cancel = null,
                Parameters = new List<string>() { "0" },
            };
            KTMessageBoxManager.AddMessageBox(msgBox);
            KTPlayerManager.ShowMessageBox(player, msgBox);
        }

        /// <summary>
        /// Hiện bảng thông báo có Title, Text tương ứng và kèm sự kiện OK
        /// </summary>
        /// <param name="player"></param>
        /// <param name="title"></param>
        /// <param name="text"></param>
        /// <param name="ok"></param>
        public static void ShowMessageBox(KPlayer player, string title, string text, Action ok)
        {
            KTMessageBox msgBox = new KTMessageBox()
            {
                Owner = player,
                Text = text,
                Title = title,
                MessageType = 0,
                OK = ok,
                Cancel = null,
                Parameters = new List<string>() { "0" },
            };
            KTMessageBoxManager.AddMessageBox(msgBox);
            KTPlayerManager.ShowMessageBox(player, msgBox);
        }

        /// <summary>
        /// Hiện bảng thông báo có Title, Text tương ứng và kèm sự kiện OK
        /// </summary>
        /// <param name="player"></param>
        /// <param name="title"></param>
        /// <param name="text"></param>
        /// <param name="ok"></param>
        /// <param name="showButtonCancel"></param>
        public static void ShowMessageBox(KPlayer player, string title, string text, Action ok, bool showButtonCancel)
        {
            KTMessageBox msgBox = new KTMessageBox()
            {
                Owner = player,
                Text = text,
                Title = title,
                MessageType = 0,
                OK = ok,
                Cancel = null,
                Parameters = new List<string>() { showButtonCancel ? "1" : "0" },
            };
            KTMessageBoxManager.AddMessageBox(msgBox);
            KTPlayerManager.ShowMessageBox(player, msgBox);
        }

        /// <summary>
        /// Hiện bảng thông báo có Title, Text tương ứng và kèm sự kiện OK, Cancel
        /// </summary>
        /// <param name="player"></param>
        /// <param name="title"></param>
        /// <param name="text"></param>
        /// <param name="ok"></param>
        /// <param name="cancel"></param>
        public static void ShowMessageBox(KPlayer player, string title, string text, Action ok, Action cancel)
        {
            KTMessageBox msgBox = new KTMessageBox()
            {
                Owner = player,
                Text = text,
                Title = title,
                MessageType = 0,
                OK = ok,
                Cancel = cancel,
                Parameters = new List<string>() { "1" },
            };
            KTMessageBoxManager.AddMessageBox(msgBox);
            KTPlayerManager.ShowMessageBox(player, msgBox);
        }

        /// <summary>
        /// Hiện bảng nhập số có Text tương ứng, và sự kiện OK
        /// </summary>
        /// <param name="player"></param>
        /// <param name="title"></param>
        /// <param name="text"></param>
        public static void ShowInputNumberBox(KPlayer player, string text, Action<int> ok)
        {
            KTInputNumberBox msgBox = new KTInputNumberBox()
            {
                Owner = player,
                Text = text,
                Title = "",
                MessageType = 1,
                OK = ok,
                Cancel = null,
                Parameters = new List<string>() { "0" },
            };
            KTMessageBoxManager.AddMessageBox(msgBox);
            KTPlayerManager.ShowMessageBox(player, msgBox);
        }

        /// <summary>
        /// Hiện bảng nhập số có Text tương ứng, và sự kiện OK, Cancel
        /// </summary>
        /// <param name="player"></param>
        /// <param name="title"></param>
        /// <param name="text"></param>
        public static void ShowInputNumberBox(KPlayer player, string text, Action<int> ok, Action cancel)
        {
            KTInputNumberBox msgBox = new KTInputNumberBox()
            {
                Owner = player,
                Text = text,
                Title = "",
                MessageType = 1,
                OK = ok,
                Cancel = cancel,
                Parameters = new List<string>() { "1" },
            };
            KTMessageBoxManager.AddMessageBox(msgBox);
            KTPlayerManager.ShowMessageBox(player, msgBox);
        }

        /// <summary>
        /// Hiển bảng nhập chuỗi
        /// </summary>
        /// <param name="player"></param>
        /// <param name="title"></param>
        /// <param name="description"></param>
        /// <param name="initValue"></param>
        /// <param name="ok"></param>
        public static void ShowInputStringBox(KPlayer player, string description, Action<string> ok)
        {
            KTInputStringBox msgBox = new KTInputStringBox()
            {
                Owner = player,
                Text = description,
                Title = "",
                MessageType = 2,
                OK = ok,
                Cancel = null,
                Parameters = null,
            };
            KTMessageBoxManager.AddMessageBox(msgBox);
            KTPlayerManager.ShowMessageBox(player, msgBox);
        }

        /// <summary>
        /// Hiển bảng nhập chuỗi
        /// </summary>
        /// <param name="player"></param>
        /// <param name="title"></param>
        /// <param name="description"></param>
        /// <param name="initValue"></param>
        /// <param name="ok"></param>
        public static void ShowInputStringBox(KPlayer player, string description, Action<string> ok, Action cancel)
        {
            KTInputStringBox msgBox = new KTInputStringBox()
            {
                Owner = player,
                Text = description,
                Title = "",
                MessageType = 2,
                OK = ok,
                Cancel = cancel,
                Parameters = null,
            };
            KTMessageBoxManager.AddMessageBox(msgBox);
            KTPlayerManager.ShowMessageBox(player, msgBox);
        }

        /// <summary>
        /// Hiển bảng nhập chuỗi
        /// </summary>
        /// <param name="player"></param>
        /// <param name="title"></param>
        /// <param name="description"></param>
        /// <param name="initValue"></param>
        /// <param name="ok"></param>
        public static void ShowInputStringBox(KPlayer player, string description, string initValue, Action<string> ok)
        {
            KTInputStringBox msgBox = new KTInputStringBox()
            {
                Owner = player,
                Text = description,
                Title = "",
                MessageType = 2,
                OK = ok,
                Cancel = null,
                Parameters = new List<string>() { initValue },
            };
            KTMessageBoxManager.AddMessageBox(msgBox);
            KTPlayerManager.ShowMessageBox(player, msgBox);
        }

        /// <summary>
        /// Hiển bảng nhập chuỗi
        /// </summary>
        /// <param name="player"></param>
        /// <param name="title"></param>
        /// <param name="description"></param>
        /// <param name="initValue"></param>
        /// <param name="ok"></param>
        public static void ShowInputStringBox(KPlayer player, string description, string initValue, Action<string> ok, Action cancel)
        {
            KTInputStringBox msgBox = new KTInputStringBox()
            {
                Owner = player,
                Text = description,
                Title = "",
                MessageType = 2,
                OK = ok,
                Cancel = cancel,
                Parameters = new List<string>() { initValue },
            };
            KTMessageBoxManager.AddMessageBox(msgBox);
            KTPlayerManager.ShowMessageBox(player, msgBox);
        }
        #endregion

        #region Notification Tip
        /// <summary>
        /// Hiển thị thông báo Tooltip trên đầu người chơi
        /// </summary>
        /// <param name="player"></param>
        /// <param name="message"></param>
        public static void ShowNotification(KPlayer player, string message)
        {
            KT_TCPHandler.ShowClientNotificationTip(player, message);
        }

        /// <summary>
        /// Hiển thị thông báo Tooltip trên đầu tất cả thành viên trong nhóm
        /// </summary>
        /// <param name=""></param>
        /// <param name="message"></param>
        public static void ShowNotificationToAllTeammates(KPlayer player, string message)
        {
            foreach (KPlayer teammate in player.Teammates)
            {
                KTPlayerManager.ShowNotification(teammate, message);
            }
        }
        #endregion

        #region Xóa vật phẩm
        /// <summary>
        /// Mở khung xóa vật phẩm
        /// </summary>
        /// <param name="player"></param>
        public static void OpenRemoveItems(KPlayer player)
        {
            /// Mở khung tiêu hủy vật phẩm
            KT_TCPHandler.SendOpenInputItems(player, "Tiêu hủy vật phẩm", "Đặt vào danh sách vật phẩm muốn tiêu hủy...", "Vật phẩm sau khi bị tiêu hủy sẽ không thể lấy lại được.", "RemoveItems");
        }

        /// <summary>
        /// Gửi về client yêu cầu nhận vật phẩm
        /// </summary>
        /// <param name="player"></param>
        public static void SendOpenQuestReviceItem(KPlayer player, int TaskID, int NpcID)
        {
            /// Mở khung tiêu hủy vật phẩm
            KT_TCPHandler.SendOpenInputItems(player, "Trả nhiệm vụ giã tẩu", "Đặt vào vật phẩm nhiệm vụ cần trả vào đây", "Vật phẩm sau khi trả nhiệm vụ sẽ không thể lấy lại được", "QuestItem_" + TaskID + "_" + NpcID);
        }

        /// <summary>
        /// Thực hiện xóa danh sách vật phẩm tương ứng
        /// </summary>
        /// <param name="player"></param>
        /// <param name="itemGDs"></param>
        public static void ResolveRemoveItems(KPlayer player, List<GoodsData> itemGDs)
        {
            /// Duyệt danh sách
            foreach (GoodsData itemGD in itemGDs)
            {
                /// Thực hiện xóa vật phẩm
                ItemManager.AbandonItem(itemGD, player, false, "GM_ClearBag");
            }

            /// Thông báo
            KTPlayerManager.ShowNotification(player, "Tiêu hủy vật phẩm thành công!");
        }

        /// <summary>
        /// Thực hiện xóa danh sách vật phẩm tương ứng trong thương khố
        /// </summary>
        /// <param name="player"></param>
        /// <param name="itemGDs"></param>
        public static void ResolveRemovePortableBagItems(KPlayer player, List<GoodsData> itemGDs)
        {
            /// Duyệt danh sách
            foreach (GoodsData itemGD in itemGDs)
            {
                /// Thực hiện xóa vật phẩm
                ItemManager.AbandonItem(itemGD, player, false, "GM_ClearPortableBag");
            }

            /// Thông báo
            KTPlayerManager.ShowNotification(player, "Tiêu hủy vật phẩm trong thương khố thành công!");
        }
        #endregion

        #region Gộp vật phẩm
        /// <summary>
        /// Mở khung ghép vật phẩm
        /// </summary>
        /// <param name="player"></param>
        public static void OpenMergeItems(KPlayer player)
        {
            /// Mở khung tiêu hủy vật phẩm
            KT_TCPHandler.SendOpenInputItems(player, "Ghép vật phẩm", "Đặt vào danh sách vật phẩm muốn ghép...", "Chỉ ghép được các vật phẩm cùng loại.", "MergeItems");
        }
        #endregion

        #region Open change signet, mantle, chopstick
        /// <summary>
        /// Mở khung đổi ngũ hành ấn, quan ấn và phi phong theo hệ của nhân vật tương ứng
        /// </summary>
        /// <param name="player"></param>
        public static void OpenChangeSignetMantleAndChopstick(KPlayer player)
        {
            /// Mở khung tiêu hủy vật phẩm
            KT_TCPHandler.SendOpenInputItems(player, "Đổi hệ trang bị", "Đặt vào <color=green>Ngũ Hành Ấn, Phi Phong</color> cần đổi hệ...", "Trang bị sau khi đổi sẽ theo môn phái và ngũ hành của nhân vật.", "ChangeSignetMantleAndChopstick");
        }

        /// <summary>
        /// Xử lý đổi ngũ hành ấn, quan ấn và phi phong theo hệ của nhân vật tương ứng
        /// </summary>
        /// <param name="player"></param>
        /// <param name="itemGDs"></param>
        public static void ResolveChangeSignetMantleAndChopstick(KPlayer player, List<GoodsData> itemGDs)
        {
            /// Kết quả
            bool result = false;

            /// Môn phái nhân vật
            int factionID = player.m_cPlayerFaction.GetFactionId();
            /// Giới tính nhân vật
            int sex = player.RoleSex;
            /// Ngũ hành nhân vật
            KE_SERIES_TYPE series = player.m_Series;
            /// Thông tin TCP
            TCPOutPacketPool pool = Global._TCPManager.TcpOutPacketPool;
            TCPClientPool tcpClientPool = Global._TCPManager.tcpClientPool;

            /// Duyệt danh sách
            foreach (GoodsData itemGD in itemGDs)
            {
                /// Toác
                if (itemGD == null)
                {
                    return;
                }

                /// Vật phẩm không tồn tại
                if (!ItemManager._TotalGameItem.ContainsKey(itemGD.GoodsID))
                {
                    continue;
                }
                /// Không có DbID
                else if (itemGD.Id == -1)
                {
                    continue;
                }
                /// Số lượng không thỏa mãn
                else if (itemGD.GCount <= 0)
                {
                    continue;
                }

                /// Thông tin vật phẩm
                ItemData _itemData = ItemManager.GetItemTemplate(itemGD.GoodsID);
                /// Nếu không phải quan ấn, phi phong và ngũ hành ấn thì thôi
                if (_itemData.DetailType != (int) KE_ITEM_EQUIP_DETAILTYPE.equip_signet && _itemData.DetailType != (int) KE_ITEM_EQUIP_DETAILTYPE.equip_mantle && _itemData.DetailType != (int) KE_ITEM_EQUIP_DETAILTYPE.equip_chop)
                {
                    continue;
                }

                /// Vật phẩm thật trên người
                GoodsData goodsGD = player.GoodsData.Find(itemGD.Id, 0);
                /// Nếu không tìm thấy
                if (goodsGD == null)
                {
                    continue;
                }

                /// Vật phẩm không tồn tại
                if (!ItemManager._TotalGameItem.ContainsKey(goodsGD.GoodsID))
                {
                    continue;
                }
                /// Không có DbID
                else if (goodsGD.Id == -1)
                {
                    continue;
                }
                /// Số lượng không thỏa mãn
                else if (goodsGD.GCount <= 0)
                {
                    continue;
                }
                /// ID vật phẩm khác
                else if (goodsGD.GoodsID != itemGD.GoodsID)
                {
                    continue;
                }

                /// Thông tin vật phẩm
                ItemData goodsItemData = ItemManager.GetItemTemplate(goodsGD.GoodsID);
                /// Toác
                if (goodsItemData == null)
                {
                    continue;
                }

                /// Danh sách cập nhật chỉ số trang bị
                Dictionary<UPDATEITEM, object> updateProperties = new Dictionary<UPDATEITEM, object>();
                updateProperties.Add(UPDATEITEM.ROLEID, player.RoleID);
                updateProperties.Add(UPDATEITEM.ITEMDBID, itemGD.Id);

                /// ID vật phẩm
                int goodsID = itemGD.GoodsID;
                /// Ngũ hành
                int goodsSeries = itemGD.Series;
                /// Nếu là ngũ hành ấn
                if (goodsItemData.DetailType == (int) KE_ITEM_EQUIP_DETAILTYPE.equip_signet)
                {
                    /// Tìm ngũ hành ấn tương ứng phái
                    ItemData itemData = ItemManager.Signets.Where((goods) =>
                    {
                        /// Nếu không cùng cấp độ
                        if (goods.Level != goodsItemData.Level)
                        {
                            return false;
                        }
                        /// Nếu phái không khớp
                        else if (!goods.ListReqProp.Any(x => x.ReqPropType == (int) KE_ITEM_REQUIREMENT.emEQUIP_REQ_FACTION && x.ReqPropValue == factionID))
                        {
                            return false;
                        }

                        /// Thỏa mãn
                        return true;
                    }).FirstOrDefault();
                    /// Nếu tìm thấy
                    if (itemData != null)
                    {
                        /// Thay đổi ID vật phẩm
                        goodsID = itemData.ItemID;
                        updateProperties.Add(UPDATEITEM.GOODSID, itemData.ItemID);
                        /// Thay đổi ngũ hành
                        goodsSeries = itemData.Series;
                        updateProperties.Add(UPDATEITEM.SERIES, itemData.Series);
                    }
                }
                /// Nếu là phi phong
                else if (goodsItemData.DetailType == (int) KE_ITEM_EQUIP_DETAILTYPE.equip_mantle)
                {
                    /// Tìm phi phong tương ứng hệ
                    ItemData itemData = ItemManager.Mantles.Where((goods) =>
                    {
                        /// Nếu không cùng cấp độ
                        if (goods.Level != goodsItemData.Level)
                        {
                            return false;
                        }
                        /// Nếu yêu cầu giới tính không thỏa mãn
                        else if (!goods.ListReqProp.Any(x => x.ReqPropType == (int) KE_ITEM_REQUIREMENT.emEQUIP_REQ_SEX && x.ReqPropValue == sex))
                        {
                            return false;
                        }
                        /// Nếu yêu cầu ngũ hành không thỏa mãn
                        else if (!goods.ListReqProp.Any(x => x.ReqPropType == (int) KE_ITEM_REQUIREMENT.emEQUIP_REQ_SERIES && x.ReqPropValue == (int) series))
                        {
                            return false;
                        }

                        /// Thỏa mãn
                        return true;
                    }).FirstOrDefault();
                    /// Nếu tìm thấy
                    if (itemData != null)
                    {
                        /// Thay đổi ID vật phẩm
                        goodsID = itemData.ItemID;
                        updateProperties.Add(UPDATEITEM.GOODSID, itemData.ItemID);
                        /// Thay đổi ngũ hành
                        goodsSeries = itemData.Series;
                        updateProperties.Add(UPDATEITEM.SERIES, itemData.Series);
                    }
                }
                /// Nếu là quan ấn
                else if (goodsItemData.DetailType == (int) KE_ITEM_EQUIP_DETAILTYPE.equip_chop)
                {
                    /// Tìm quan ấn tương ứng hệ
                    ItemData itemData = ItemManager.Chops.Where((goods) =>
                    {
                        /// Nếu không cùng cấp độ
                        if (goods.Level != goodsItemData.Level)
                        {
                            return false;
                        }
                        /// Nếu yêu cầu ngũ hành không thỏa mãn
                        else if (!goods.ListReqProp.Any(x => x.ReqPropType == (int) KE_ITEM_REQUIREMENT.emEQUIP_REQ_SERIES && x.ReqPropValue == (int) series))
                        {
                            return false;
                        }

                        /// Thỏa mãn
                        return true;
                    }).FirstOrDefault();
                    /// Nếu tìm thấy
                    if (itemData != null)
                    {
                        /// Thay đổi ID vật phẩm
                        goodsID = itemData.ItemID;
                        updateProperties.Add(UPDATEITEM.GOODSID, itemData.ItemID);
                        /// Thay đổi ngũ hành
                        goodsSeries = itemData.Series;
                        updateProperties.Add(UPDATEITEM.SERIES, itemData.Series);
                    }
                }

                /// Nếu không có gì thay đổi thì thôi
                if (goodsID == goodsGD.GoodsID)
                {
                    continue;
                }
                /// Đánh dấu có kết quả
                result = true;

                string cmdData = ItemManager.ItemUpdateScriptBuild(updateProperties);
                TCPProcessCmdResults dbRequestResult = Global.RequestToDBServer(tcpClientPool, pool, (int) TCPGameServerCmds.CMD_DB_UPDATEGOODS_CMD, cmdData, out string[] dbFields, player.ServerId);
                /// Nếu thay đổi thành công
                if (dbRequestResult == TCPProcessCmdResults.RESULT_DATA)
                {
                    /// Cập nhật thay đổi ở GS
                    goodsGD.GoodsID = goodsID;
                    goodsGD.Series = goodsSeries;
                    /// Cập nhật thay đổi về Client
                    player.SendPacket<GoodsData>((int) TCPGameServerCmds.CMD_SPR_NOTIFYGOODSINFO, goodsGD);
                }
            }

            /// Nếu có kết quả
            if (result)
            {
                KTPlayerManager.ShowNotification(player, "Thao tác thành công!");
            }
            else
            {
                KTPlayerManager.ShowNotification(player, "Không có gì cần thay đổi!");
            }
        }
        #endregion

        #region Captcha
        /// <summary>
        /// Mở Captcha
        /// </summary>
        /// <param name="player"></param>
        /// <param name="onAnswer"></param>
        /// <param name="delayTicks"></param>
        public static bool OpenCaptcha(KPlayer player, Action<bool> onAnswer, int delayTicks = 30000)
        {
            /// Nếu không kích hoạt Captcha
            if (!ServerConfig.Instance.EnableCaptcha)
            {
                /// Thực hiện hàm Callback
                onAnswer?.Invoke(true);
                /// Hủy hàm Callback
                player.AnswerCaptcha = null;
                /// Bỏ qua
                return true;
            }

            /// Nếu chưa đến giờ
            if (KTGlobal.GetCurrentTimeMilis() - player.LastJailCaptchaTicks < delayTicks)
            {
                return false;
            }
            /// Đánh dấu thời gian xuất hiện Captcha
            player.LastJailCaptchaTicks = KTGlobal.GetCurrentTimeMilis();

            /// Ghi lại hàm Callback
            player.AnswerCaptcha = (isCorrect) =>
            {
                /// Nếu trả lời đúng
                if (isCorrect)
                {
                    /// Đánh dấu thời điểm xuất hiện Captcha tiếp theo
                    player.NextCaptchaTicks = KTGlobal.GetCurrentTimeMilis() + KTGlobal.GetRandomNumber(ServerConfig.Instance.CaptchaAppearMinPeriod, ServerConfig.Instance.CaptchaAppearMaxPeriod);
                }

                /// Thực hiện hàm Callback
                onAnswer?.Invoke(isCorrect);
            };
            /// Thực hiện mở bảng Captcha
            player.GenerateCaptcha();
            /// Trả về kết quả
            return true;
        }
        #endregion
    }
}
