using GameServer.Core.Executor;
using GameServer.KiemThe.Entities;
using GameServer.KiemThe.Logic;
using GameServer.KiemThe.LuaSystem;
using GameServer.KiemThe.Utilities;
using GameServer.Logic;
using GameServer.Server;
using Server.Data;
using Server.Protocol;
using Server.TCP;
using Server.Tools;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using static GameServer.Logic.KTMapManager;

namespace GameServer.KiemThe.Core.Item
{
    public class ItemManager
    {
        #region Quản lý vật phẩm

        /// <summary>
        /// Thời gian mặc định khi endtimme
        /// // Năm tháng ngày  = giờ phút giây
        /// </summary>
        public const string ConstGoodsEndTime = "1900-01-01 12:00:00";

        /// <summary>
        /// Trả về số lượng vật phẩm xếp chồng
        /// </summary>
        /// <param name="goodsID"></param>
        /// <returns></returns>
        public static int GetGoodsGridNumByID(int goodsID)
        {
            if (!ItemManager._TotalGameItem.ContainsKey(goodsID))
            {
                return -1;
            }

            ItemData _Item = ItemManager._TotalGameItem[goodsID];

            return _Item.Stack;
        }

        /// <summary>
        /// Thêm vật phẩm tương ứng
        /// </summary>
        /// <param name="client"></param>
        public static GoodsData AddGoodsData(KPlayer client, int id, int goodsID, int forgeLevel, int quality, int goodsNum, int binding, int site, string startTime, string endTime, int strong, int bagIndex = 0, int IsUsing = -1, string Probs = "", int Series = -1, Dictionary<ItemPramenter, string> OtherParams = null)
        {
            GoodsData gd = new GoodsData()
            {
                Id = id,
                GoodsID = goodsID,
                Using = IsUsing,
                Forge_level = forgeLevel,
                Starttime = startTime,
                Endtime = endTime,
                Site = site,

                Props = Probs,
                GCount = goodsNum,
                Binding = binding,
                Series = Series,
                OtherParams = OtherParams,
                BagIndex = bagIndex,

                Strong = strong,
            };

            client.GoodsData.Add(gd);
            return gd;
        }

        /// <summary>
        /// Kiểm tra xem có thể thêm đám vật phẩm tương ứng vào túi không
        /// </summary>
        /// <param name="client"></param>
        /// <param name="goodsDataList"></param>
        /// <returns></returns>
        public static bool CanAddGoodsDataList(KPlayer client, List<GoodsData> goodsDataList)
        {
            if (null == goodsDataList)
                return true;
            return KTGlobal.IsHaveSpace(goodsDataList.Count, client);
        }

        public static string FormatUpdateDBGoodsStr(params object[] args)
        {
            if (args.Length != 23)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("FormatUpdateDBGoodsStr, param length failed. Length = {0}", args.Length));
                return null;
            }

            return string.Format("{0}:{1}:{2}:{3}:{4}:{5}:{6}:{7}:{8}:{9}:{10}:{11}:{12}:{13}:{14}:{15}:{16}:{17}:{18}:{19}:{20}:{21}:{22}", args);
        }

        /// <summary>
        /// Trả về Loại vật phẩm
        /// </summary>
        /// <param name="goodsID"></param>
        /// <returns></returns>
        public static int GetGoodsCatetoriy(int goodsID)
        {
            if (!ItemManager._TotalGameItem.ContainsKey(goodsID))
            {
                return -1;
            }

            ItemData _Item = ItemManager._TotalGameItem[goodsID];

            return _Item.Genre;
        }

        /// <summary>
        /// Tạm thời để lại xử lý sau
        /// </summary>
        /// <param name="goodsID"></param>
        /// <returns></returns>
        public static GoodsData GetNewGoodsData(int goodsID, int binding)
        {
            int maxStrong = 0;
            int lucky = 0;

            return GetNewGoodsData(goodsID, 1, 0, 0, binding, 0, lucky, maxStrong);
        }

        /// <summary>
        /// Tạm thời để lại xử lý sau
        /// </summary>
        /// <param name="goodsID"></param>
        /// <param name="gcount"></param>
        /// <param name="quality"></param>
        /// <param name="forgeLevel"></param>
        /// <param name="binding"></param>
        /// <returns></returns>
        public static GoodsData GetNewGoodsData(int goodsID, int gcount, int quality, int forgeLevel, int binding, int bornIndex, int lucky, int strong, int nExcellenceInfo = 0, int nAppendPropLev = 0, int nChangeLife = 0)
        {
            GoodsData goodsData = new GoodsData()
            {
                Id = -1,
                GoodsID = goodsID,
                Using = 0,
                Forge_level = forgeLevel,
                Starttime = "1900-01-01 12:00:00",
                Endtime = ItemManager.ConstGoodsEndTime,
                Site = 0,
                Props = "",
                GCount = gcount,
                Binding = binding,
                BagIndex = 0,
                Strong = strong,
            };

            return goodsData;
        }

        public static long DateTimeTicks(string strDateTime)
        {
            try
            {
                DateTime dt1;
                if (!DateTime.TryParse(strDateTime, out dt1))
                {
                    return 0;
                }

                return dt1.Ticks;
            }
            catch (Exception)
            {
            }

            return 0;
        }

        public static bool DateTimeEqual(string strDateTime1, string strDateTime2)
        {
            try
            {
                // 应该只需要比较两个字符串就行 ChenXiaojun
                return (strDateTime1 == strDateTime2);

                //DateTime dt1;
                //if (!DateTime.TryParse(strDateTime1, out dt1))
                //{
                //    return false;
                //}

                //DateTime dt2;
                //if (!DateTime.TryParse(strDateTime2, out dt2))
                //{
                //    return false;
                //}

                //return (dt1.Ticks == dt2.Ticks);
            }
            catch (Exception)
            {
            }

            return false;
        }

        public static bool IsTimeLimitGoods(GoodsData goodsData)
        {
            if (!string.IsNullOrEmpty(goodsData.Endtime) && !ItemManager.DateTimeEqual(goodsData.Endtime, ItemManager.ConstGoodsEndTime)) //限时物品
            {
                return true;
            }

            return false;
        }

        public static bool IsGoodsTimeOver(GoodsData goodsData)
        {
            if (!ItemManager.IsTimeLimitGoods(goodsData)) //如果非限时物品
            {
                return false;
            }

            long nowTicks = TimeUtil.NOW() * 10000;
            long goodsEndTicks = ItemManager.DateTimeTicks(goodsData.Endtime);
            if (nowTicks >= goodsEndTicks)
                return true;
            return false;
        }

        /// <summary>
        /// Số lần sử dụng mặc định của vật phẩm tạm thời set là 1
        /// </summary>
        /// <param name="client"></param>
        /// <param name="goodsID"></param>
        /// <returns></returns>
        public static int GetGoodsUsingNum(int goodsID)
        {
            return 1;
        }

        /// <summary>
        /// Update good theo pramenter
        /// </summary>
        /// <param name="fields"></param>
        public static TCPProcessCmdResults ModifyGoodsByCmdParams(KPlayer client, String cmdData)
        {
            //Nếu độ dài gửi lên mà  khác 9 thì nghỉ luôn
            string[] fields = cmdData.Split(':');
            if (fields.Length != 9)
            {
                return TCPProcessCmdResults.RESULT_FAILED;
            }

            int roleID = Convert.ToInt32(fields[0]);
            int modType = Convert.ToInt32(fields[1]);
            int id = Convert.ToInt32(fields[2]);
            int goodsID = Convert.ToInt32(fields[3]);
            int isusing = Convert.ToInt32(fields[4]);
            int site = Convert.ToInt32(fields[5]);
            int gcount = Convert.ToInt32(fields[6]);
            int bagindex = Convert.ToInt32(fields[7]); // Ô trên túi đồ
            String extraParams = fields[8];//Thông tin bổ sung khi gói tin cần gửi thêm

            //int nID = (int)TCPGameServerCmds.CMD_SPR_MOD_GOODS;
            TCPOutPacketPool pool = Global._TCPManager.TcpOutPacketPool;
            TCPClientPool tcpClientPool = Global._TCPManager.tcpClientPool;
            TCPManager tcpMgr = Global._TCPManager;
            TMSKSocket socket = client.ClientSocket;

            GoodsData goodsData = null;

            goodsData = client.GoodsData.Find(id);

            /// Yêu cầu mở khóa an toàn
            if (client.NeedToShowInputSecondPassword())
            {
                return TCPProcessCmdResults.RESULT_OK;
            }

            /// Nếu vật phẩm không tồn tại
            if (null == goodsData)
            {
                // Nếu đồ không tồn tại trả về lỗi luôn
                return TCPProcessCmdResults.RESULT_FAILED;
            }

            // nếu ID vật phẩm khác với ID truyền lên thì báo lỗi
            if (goodsData.GoodsID != goodsID)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Vật phẩm truyền lên có ID khác với ID của GS trong TEMPLATE, CMD={0}, Client={1}, RoleID={2}", TCPGameServerCmds.CMD_SPR_MOD_GOODS, Global.GetSocketRemoteEndPoint(socket), roleID));
                return TCPProcessCmdResults.RESULT_FAILED;
            }

            if (modType == (int)ModGoodsTypes.SaleToNpc && (goodsData.Site != 0 || goodsData.Using >= 0))
            {
                //Chỉ có thể bán các vật phẩm trông túi đồ và đang không mặc trên người
                LogManager.WriteLog(LogTypes.Error, string.Format("Chỉ có thể bán các vật phẩm đang ở hành trang, CMD={0}, Client={1}, RoleID={2}", TCPGameServerCmds.CMD_SPR_MOD_GOODS, Global.GetSocketRemoteEndPoint(socket), roleID));
                return TCPProcessCmdResults.RESULT_FAILED;
            }

            /// Nếu là bán đồ vào cửa hàng, mà loại vật phẩm không bán được
            if (modType == (int)ModGoodsTypes.SaleToNpc && !ItemManager.IsCanBeSold(goodsData))
            {
                KTPlayerManager.ShowNotification(client, "Vật phẩm này không thể bán!");
                return TCPProcessCmdResults.RESULT_OK;
            }

            /// Nếu là trang bị nhưng trang bị cần lại không có trong túi đồ
            if (modType == (int)ModGoodsTypes.EquipLoad && (goodsData.Site != 0 || goodsData.Using >= 0))
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Là trang bị nhưng trang bị cần lại không có trong túi đồ, CMD={0}, Client={1}, RoleID={2}", TCPGameServerCmds.CMD_SPR_MOD_GOODS, Global.GetSocketRemoteEndPoint(socket), roleID));
                return TCPProcessCmdResults.RESULT_FAILED;
            }

            /// Kiểm tra số lượng truyền lên có đúng với số lượng hiện có không
            if (gcount != goodsData.GCount && modType != (int)ModGoodsTypes.SplitItem)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Số lượng gửi lên khác với số lượng trong túi đồ không thực hiện lệnh, CMD={0}, Client={1}, RoleID={2}, GoodsID={3}", TCPGameServerCmds.CMD_SPR_MOD_GOODS, Global.GetSocketRemoteEndPoint(socket), roleID, goodsID));
                return TCPProcessCmdResults.RESULT_OK;
            }

            // THực Hiện ghi lại nếu vật phẩm bán hoặc phá hủy

            /// Nếu thao tác giống nhau thì không phải xử lý. Điều này có thể phát sinh từ việc lag packet hoặc lostpacket | Packet bị dupper
            if (modType != (int)ModGoodsTypes.Abandon && isusing == goodsData.Using && site == goodsData.Site && gcount == goodsData.GCount && bagindex == goodsData.BagIndex)
            {
                return TCPProcessCmdResults.RESULT_OK;
            }

            /// Nếu thay đổi vị trí đồ đạc
            if (site != goodsData.Site)
            {
                /// Nếu đang mặc trên người thì bỏ qua
                if (isusing >= 0)
                {
                    return TCPProcessCmdResults.RESULT_OK;
                }
            }

            /// Nếu là tháo đồ hoặc mặc đồ
            bool updateEquip = (modType >= (int)ModGoodsTypes.EquipLoad && modType <= (int)ModGoodsTypes.EquipUnload);

            bool isUsingChanged = (isusing != goodsData.Using);

            /// Nếu là thao tác mặc đồ hoặc tháo đồ
            if (updateEquip)
            {
                /// Nhưng lại không có sự thay đổi về vị trí thì không làm gì
                if (!isUsingChanged)
                {
                    return TCPProcessCmdResults.RESULT_OK;
                }
            }

            /// Nếu là mặc trang bị lên người
            if (modType == (int)ModGoodsTypes.EquipLoad)
            {
                ItemData _Tmp = ItemManager.GetItemTemplate(goodsData.GoodsID);

                if (_Tmp != null)
                {
                    if (_Tmp.Genre != 7)
                    {
                        /// Kiểm tra điều kiện xem có thể trang bị không
                        if (!client.GetPlayEquipBody().CanUsingEquip(goodsData))
                        {
                            /// Nếu không thể sử dụng return toang luôn
                            return TCPProcessCmdResults.RESULT_OK;
                        }
                    }
                    else
                    {
                        // PET LUÔN MẶC ĐƯỢC ĐỒ CHỈ CSO ĐIỀU KHI ATACK SẼ CHECK ĐIỀU KIỆN ĐỦ THÌ MỚI ATTACK CHỈ SỐ
                        //if (!client.GetPlayEquipBody().CanUsingPetEquip(goodsData))
                        //{
                        //    /// Nếu không thể sử dụng return toang luôn
                        //    return TCPProcessCmdResults.RESULT_OK;
                        //}
                    }

                    if (isusing < 100 || isusing >= 200)
                    {
                        /// Thực hiện mặc đồ vào cho nhân vật
                        if (!client.GetPlayEquipBody().EquipLoadMain(goodsData))
                        {
                            /// Nếu không thể sử dụng return toang luôn
                            return TCPProcessCmdResults.RESULT_OK;
                        }
                    }
                    else/* if (isusing >= 100)*/
                    {  /// Thực hiện mặc đồ vào cho nhân vật
                        if (!client.GetPlayEquipBody().EquipLoadSub(goodsData))
                        {
                            /// Nếu không thể sử dụng return toang luôn
                            return TCPProcessCmdResults.RESULT_OK;
                        }
                    }

                    KT_TCPHandler.SendPlayerEquipChangeToNearbyPlayers(client, goodsData, -1);
                }

                /// Thông báo tới tất cả người chơi xung quanh
            }
            /// Nếu là tháo trang bị
            else if (modType == (int)ModGoodsTypes.EquipUnload)
            {
                int oldEquipSlot = goodsData.Using;

                if (isusing < 100 || isusing >= 200)
                {
                    // Thực hiện mặc đồ vào cho nhân vật
                    if (!client.GetPlayEquipBody().EquipUnloadMain(goodsData))
                    {
                        // Nếu không thể sử dụng return toang luôn
                        return TCPProcessCmdResults.RESULT_OK;
                    }

                    /// Thông báo tới tất cả người chơi xung quanh
                    KT_TCPHandler.SendPlayerEquipChangeToNearbyPlayers(client, goodsData, oldEquipSlot);
                }
                else /*if (isusing >= 100)*/
                {
                    // Thực hiện mặc đồ vào cho nhân vật
                    if (!client.GetPlayEquipBody().EquipUnloadSub(goodsData))
                    {
                        // Nếu không thể sử dụng return toang luôn
                        return TCPProcessCmdResults.RESULT_OK;
                    }
                }
            }
            /// Nếu là vứt vật phẩm
            else if (modType == (int)ModGoodsTypes.Abandon)
            {
                /// Nếu bị khóa
                if (client.IsBannedFeature(RoleBannedFeature.ThrowItem, out int timeLeftSec))
                {
                    KTPlayerManager.ShowNotification(client, "Bạn đang bị khóa chức năng vứt vật phẩm. Thời gian còn " + KTGlobal.DisplayFullDateAndTime(timeLeftSec) + "!");
                    return TCPProcessCmdResults.RESULT_OK;
                }

                ///// Tạm khóa
                //if (true)
                //{
                //    PlayerManager.ShowNotification(client, "Chức năng tạm thời khóa. Từ sau bảo trì tới, khi vứt vật phẩm sẽ không rơi xuống đất mà mất thẳng luôn, hãy chú ý!");
                //    return TCPProcessCmdResults.RESULT_OK;
                //}

                if (client.ClientSocket.IsKuaFuLogin)
                {
                    KTPlayerManager.ShowNotification(client, "Ở liên máy chủ không thể vứt bỏ vật phẩm");
                    return TCPProcessCmdResults.RESULT_OK;
                }

                if (goodsData.Binding == 1)
                {
                    KTPlayerManager.ShowNotification(client, "Vật phẩm khóa không thể vứt bỏ");

                    return TCPProcessCmdResults.RESULT_OK;
                }

                if (goodsData.Endtime != ItemManager.ConstGoodsEndTime)
                {
                    KTPlayerManager.ShowNotification(client, "Vật phẩm có hạn sử dụng không thể vứt bỏ");

                    return TCPProcessCmdResults.RESULT_OK;
                }
                if (goodsData.Forge_level > 0)
                {
                    KTPlayerManager.ShowNotification(client, "Vật phẩm đã cường hóa không thể vứt ra");

                    return TCPProcessCmdResults.RESULT_OK;
                }

                /// Hủy và tạo vật phẩm rơi
                if (!ItemManager.AbandonItem(goodsData, client, true, "Vứt vật phẩm"))
                {
                    KTPlayerManager.ShowNotification(client, "Có lỗi khi vứt vật phẩm");
                    return TCPProcessCmdResults.RESULT_OK;
                }
            }
            /// Nếu là thay đổi vị trí
            else if (modType == (int)ModGoodsTypes.ModValue)
            {
                if (!ItemManager.ItemModValue(goodsData, client, site))
                {
                    return TCPProcessCmdResults.RESULT_OK;
                }
            }
            else if (modType == (int)ModGoodsTypes.SplitItem)
            {
                if (!KTGlobal.IsHaveSpace(1, client))
                {
                    KTPlayerManager.ShowNotification(client, "Túi đồ không đủ không đủ trỗ trống không thể tách");

                    return TCPProcessCmdResults.RESULT_OK;
                }
                else
                {
                    if (!ItemManager.SplitItem(goodsData, client, Int32.Parse(extraParams)))
                    {
                        return TCPProcessCmdResults.RESULT_OK;
                    }
                }
            }

            return TCPProcessCmdResults.RESULT_OK;
        }

        #endregion Quản lý vật phẩm

        public static Dictionary<KE_EQUIP_POSITION, ActiveByItem> g_anEquipActive = new Dictionary<KE_EQUIP_POSITION, ActiveByItem>(); // KEY CẦN 2 VALUE ĐỂ KÍCH HOẠT

        public static ItemValueCaculation _Calutaion = new ItemValueCaculation();

        // Vị trí mặc của vũ set chính
        public static int[] g_anEquipPos = { (int)KE_EQUIP_POSITION.emEQUIPPOS_WEAPON, (int)KE_EQUIP_POSITION.emEQUIPPOS_WEAPON, (int)KE_EQUIP_POSITION.emEQUIPPOS_BODY, (int)KE_EQUIP_POSITION.emEQUIPPOS_RING, (int)KE_EQUIP_POSITION.emEQUIPPOS_AMULET, (int)KE_EQUIP_POSITION.emEQUIPPOS_FOOT, (int)KE_EQUIP_POSITION.emEQUIPPOS_BELT, (int)KE_EQUIP_POSITION.emEQUIPPOS_HEAD, (int)KE_EQUIP_POSITION.emEQUIPPOS_CUFF, (int)KE_EQUIP_POSITION.emEQUIPPOS_PENDANT, (int)KE_EQUIP_POSITION.emEQUIPPOS_HORSE, (int)KE_EQUIP_POSITION.emEQUIPPOS_MASK, (int)KE_EQUIP_POSITION.emEQUIPPOS_BOOK, (int)KE_EQUIP_POSITION.emEQUIPPOS_ORNAMENT, (int)KE_EQUIP_POSITION.emEQUIPPOS_SIGNET, (int)KE_EQUIP_POSITION.emEQUIPPOS_MANTLE, (int)KE_EQUIP_POSITION.emEQUIPPOS_CHOP, (int)KE_EQUIP_POSITION.emEQUIPPOS_RING_2 };

        public static int[] g_anEquipPetPos = { (int)KE_EQUIP_PET_POSTION.emEQUIPPOS_HEAD, (int)KE_EQUIP_PET_POSTION.emEQUIPPOS_BODY, (int)KE_EQUIP_PET_POSTION.emEQUIPPOS_BELT, (int)KE_EQUIP_PET_POSTION.emEQUIPPOS_WEAPON, (int)KE_EQUIP_PET_POSTION.emEQUIPPOS_FOOT, (int)KE_EQUIP_PET_POSTION.emEQUIPPOS_CUFF, (int)KE_EQUIP_PET_POSTION.emEQUIPPOS_ORNAMENT, (int)KE_EQUIP_PET_POSTION.emEQUIPPOS_RING, (int)KE_EQUIP_PET_POSTION.emEQUIPPOS_RING_2, (int)KE_EQUIP_PET_POSTION.emEQUIPPOS_PENDANT };

        // Vị trí mặc của set dự phòng
        public static int[] g_anEquipSubPos = { (int)KE_EQUIP_POSITION.emEQUIPPOS_WEAPON, (int)KE_EQUIP_POSITION.emEQUIPPOS_WEAPON, (int)KE_EQUIP_POSITION.emEQUIPPOS_BODY, (int)KE_EQUIP_POSITION.emEQUIPPOS_RING, (int)KE_EQUIP_POSITION.emEQUIPPOS_AMULET, (int)KE_EQUIP_POSITION.emEQUIPPOS_FOOT, (int)KE_EQUIP_POSITION.emEQUIPPOS_BELT, (int)KE_EQUIP_POSITION.emEQUIPPOS_HEAD, (int)KE_EQUIP_POSITION.emEQUIPPOS_CUFF, (int)KE_EQUIP_POSITION.emEQUIPPOS_PENDANT, (int)KE_EQUIP_POSITION.emEQUIPPOS_HORSE, (int)KE_EQUIP_POSITION.emEQUIPPOS_MASK, (int)KE_EQUIP_POSITION.emEQUIPPOS_BOOK, (int)KE_EQUIP_POSITION.emEQUIPPOS_ORNAMENT, (int)KE_EQUIP_POSITION.emEQUIPPOS_SIGNET, (int)KE_EQUIP_POSITION.emEQUIPPOS_MANTLE, (int)KE_EQUIP_POSITION.emEQUIPPOS_CHOP, (int)KE_EQUIP_POSITION.emEQUIPPOS_RING_2 };

        public static string ITEMROOTPATH = "Config/KT_Item/TotalItem";

        public static string SignetExpPath = "Config/KT_Item/SignnetExp.xml";

        /// <summary>
        /// Load config action Hidden Efffect | Green Efffect | Active SET
        /// </summary>
        public static string MagicAttribLevel = "Config/KT_Item/MagicAttribLevel.xml";

        public static string SuiteActiveProp = "Config/KT_Item/SuiteActiveProp.xml";

        public static string ItemCacluation = "Config/KT_Item/ItemValueCaculation.xml";

        public static List<MagicAttribLevel> TotalMagicAttribLevel = new List<MagicAttribLevel>();

        public static List<SuiteActiveProp> TotalSuiteActiveProp = new List<SuiteActiveProp>();

        public static List<ItemData> TotalItem = new List<ItemData>();

        /// <summary>
        /// Kinh nghiệm thăng cấp ngũ hành ấn
        /// </summary>
        public static List<SingNetExp> _TotalSingNetExp = new List<SingNetExp>();

        public static ConcurrentDictionary<int, ItemData> _TotalGameItem { get; private set; } = new ConcurrentDictionary<int, ItemData>();

        /// <summary>
        /// Danh sách ngũ hành ấn
        /// </summary>
        public static List<ItemData> Signets { get; private set; } = new List<ItemData>();

        /// <summary>
        /// Danh sách phi phong
        /// </summary>
        public static List<ItemData> Mantles { get; private set; } = new List<ItemData>();

        /// <summary>
        /// Danh sách quan ấn
        /// </summary>
        public static List<ItemData> Chops { get; private set; } = new List<ItemData>();

        /// <summary>
        /// Kiểm tra có phải trang bị không
        /// </summary>
        /// <param name="itemGD"></param>
        /// <returns></returns>
        public static bool IsEquip(GoodsData itemGD)
        {
            /// Nếu vật phẩm không tồn tại
            if (!ItemManager._TotalGameItem.TryGetValue(itemGD.GoodsID, out ItemData itemData))
            {
                return false;
            }

            return ItemManager.KD_ISEQUIP(itemData.Genre) || ItemManager.KD_ISPETEQUIP(itemData.Genre);
        }


        public static bool IsCanUnlockItem(GoodsData itemGD)
        {
            /// Nếu vật phẩm không tồn tại
            if (!ItemManager._TotalGameItem.TryGetValue(itemGD.GoodsID, out ItemData itemData))
            {
                return false;
            }

            return ItemManager.KD_ISCANUNLOCK(itemData.DetailType);
        }
        /// <summary>
        /// Tìm ra 1 teamplate item
        /// </summary>
        /// <param name="nGenre"></param>
        /// <param name="nDetail"></param>
        /// <param name="nParticular"></param>
        /// <returns></returns>
        public static ItemData GetTemplateItem(int nGenre, int nDetail, int nParticular)
        {
            var FindTemplate = TotalItem.Where(x => x.Genre == nGenre && x.DetailType == nDetail && x.ParticularType == nParticular).FirstOrDefault();

            if (FindTemplate != null)
            {
                return FindTemplate;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Hàm lấy ra độ bền của vật phẩm
        /// </summary>
        /// <param name="_ItemData"></param>
        /// <returns></returns>
        public static int GetDurability(ItemData _ItemData)
        {
            var Find = _ItemData.ListBasicProp.Where(x => x.BasicPropType == "durability_v").FirstOrDefault();

            if (Find != null)
            {
                return Find.BasicPropPA1Min;
            }

            return -1;
        }

        /// <summary>
        /// Dánh ách đồ không phải trang sức
        /// </summary>
        /// <param name="detailType"></param>
        /// <returns></returns>
        public static bool KD_ISNONNAMENT(int detailType)
        {
            if (detailType == (int)KE_ITEM_EQUIP_DETAILTYPE.equip_armor ||
                detailType == (int)KE_ITEM_EQUIP_DETAILTYPE.equip_belt ||
                detailType == (int)KE_ITEM_EQUIP_DETAILTYPE.equip_boots ||
                detailType == (int)KE_ITEM_EQUIP_DETAILTYPE.equip_cuff ||
                detailType == (int)KE_ITEM_EQUIP_DETAILTYPE.equip_rangeweapon ||
                detailType == (int)KE_ITEM_EQUIP_DETAILTYPE.equip_meleeweapon ||
                detailType == (int)KE_ITEM_EQUIP_DETAILTYPE.equip_helm)
            {
                return true;
            }

            return false;
        }

        public static bool KD_ISCANUNLOCK(int detailType)
        {
            if (detailType == (int)KE_ITEM_EQUIP_DETAILTYPE.equip_armor ||
                detailType == (int)KE_ITEM_EQUIP_DETAILTYPE.equip_belt ||
                detailType == (int)KE_ITEM_EQUIP_DETAILTYPE.equip_boots ||
                detailType == (int)KE_ITEM_EQUIP_DETAILTYPE.equip_cuff ||
                detailType == (int)KE_ITEM_EQUIP_DETAILTYPE.equip_rangeweapon ||
                detailType == (int)KE_ITEM_EQUIP_DETAILTYPE.equip_meleeweapon ||
                 detailType == (int)KE_ITEM_EQUIP_DETAILTYPE.equip_signet ||
                    detailType == (int)KE_ITEM_EQUIP_DETAILTYPE.equip_pendant ||
                    detailType == (int)KE_ITEM_EQUIP_DETAILTYPE.equip_ring ||
                      detailType == (int)KE_ITEM_EQUIP_DETAILTYPE.equip_horse ||
                     detailType == (int)KE_ITEM_EQUIP_DETAILTYPE.equip_amulet ||
                detailType == (int)KE_ITEM_EQUIP_DETAILTYPE.equip_helm)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Hàm check xme có phải trang bị hay không
        /// </summary>
        /// <param name="general"></param>
        /// <returns></returns>
        public static bool KD_ISEQUIP(int general)
        {
            if (general == (int)KE_ITEM_GENRE.item_equip_general || general == (int)KE_ITEM_GENRE.item_equip_gold || general == (int)KE_ITEM_GENRE.item_equip_purple)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool KD_ISPETEQUIP(int general)
        {
            if (general == (int)KE_ITEM_GENRE.item_pet_equip)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Kiểm tra trang bị tương ứng có cường hóa được không
        /// </summary>
        /// <param name="itemData"></param>
        /// <returns></returns>
        public static bool CanEquipBeEnhance(ItemData itemData)
        {
            return itemData.DetailType <= 13;
        }

        /// <summary>
        /// Hàm check xem có phải thuộc tính bộ hay không
        /// </summary>
        /// <param name="general"></param>
        /// <returns></returns>
        public static bool KD_ISSUITE(int general)
        {
            // nếu là đồ hoàng kim hoặc đồ xanh thì có thêm thuộc tính bộ
            if (general == (int)KE_ITEM_GENRE.item_equip_gold || general == (int)KE_ITEM_GENRE.item_equip_green)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Hàm check xem có phải là vũ khí hay không
        /// </summary>
        /// <param name="DetailType"></param>
        /// <returns></returns>
        public static bool KD_ISWEAPON(int DetailType)
        {
            if (DetailType == (int)KE_ITEM_EQUIP_DETAILTYPE.equip_meleeweapon || DetailType == (int)KE_ITEM_EQUIP_DETAILTYPE.equip_rangeweapon)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Có phải là trang sức hay không
        /// </summary>
        /// <param name="DetailType"></param>
        /// <returns></returns>
        public static bool KD_ISORNAMENT(int DetailType)
        {
            if (DetailType == (int)KE_ITEM_EQUIP_DETAILTYPE.equip_ring || DetailType == (int)KE_ITEM_EQUIP_DETAILTYPE.equip_amulet || DetailType == (int)KE_ITEM_EQUIP_DETAILTYPE.equip_pendant)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Có phải quan ấn hay không
        /// </summary>
        /// <param name="DetailType"></param>
        /// <returns></returns>
        public static bool KD_ISSIGNET(int DetailType)
        {
            if (DetailType == (int)KE_ITEM_EQUIP_DETAILTYPE.equip_signet)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        ///  Nếu là kim tê
        /// </summary>
        /// <param name="DetailType"></param>
        /// <returns></returns>
        public static bool KD_ISJINXI(int ItemID)
        {
            if (ItemID == 195 || ItemID == 196 || ItemID == 197 || ItemID == 198 || ItemID == 199)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool IsQianKunFu(int ItemID)
        {
            if (ItemID == 344 || ItemID == 354)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static KE_ITEM_EQUIP_DETAILTYPE GetItemType(int DetailType)
        {
            return (KE_ITEM_EQUIP_DETAILTYPE)DetailType;
        }

        public static void LoadItemFromPath(string FilesPath)
        {
            string Files = KTGlobal.GetDataPath(FilesPath);

            string[] TotalItemFind = System.IO.Directory.GetFiles(Files, "*.xml", System.IO.SearchOption.AllDirectories);

            foreach (string Items in TotalItemFind)
            {
                using (var stream = System.IO.File.OpenRead(Items))
                {
                    var serializer = new XmlSerializer(typeof(List<ItemData>));

                    Console.WriteLine("Loading Item :" + Items);
                    List<ItemData> _Item = serializer.Deserialize(stream) as List<ItemData>;
                    // ADD TOTAL ITEM TO LIST
                    TotalItem.AddRange(_Item);
                }
            }
        }

        #region SignNetManager

        public static void LoadSigNetXml(string FilesPath)
        {
            string Files = KTGlobal.GetDataPath(FilesPath);
            using (var stream = System.IO.File.OpenRead(Files))
            {
                var serializer = new XmlSerializer(typeof(List<SingNetExp>));

                _TotalSingNetExp = serializer.Deserialize(stream) as List<SingNetExp>;
            }
        }

        #endregion SignNetManager

        #region BookManager

        /// <summary>
        /// Lấy ra exp của sách
        /// </summary>
        /// <param name="Level"></param>
        /// <param name="BookType"></param>
        /// <returns></returns>
        //public static int GetMaxbookEXP(int Level, int BookType)
        //{
        //    int Exp = 0;

        //    var find = _TotalExpBook.Where(x => x.BookLevel == BookType && x.Level == Level).FirstOrDefault();
        //    if (find != null)
        //    {
        //        Exp = find.Exp;
        //    }

        //    return Exp;
        //}

        /// <summary>
        /// Hàm trả về Exp nhận được của mật tịch khi giết 1 con quái
        /// </summary>
        /// <param name="ExpInput"></param>
        /// <returns></returns>
        public static int GetEXPEarnPerExp(int ExpInput)
        {
            return (4 * ExpInput) / 100;
        }

        //public static void LoadingBookExp(string FilesPath)
        //{
        //    string Files = KTGlobal.GetDataPath(FilesPath);
        //    using (var stream = System.IO.File.OpenRead(Files))
        //    {
        //        var serializer = new XmlSerializer(typeof(List<ExpBookLoading>));

        //        _TotalExpBook = serializer.Deserialize(stream) as List<ExpBookLoading>;
        //    }
        //}

        #endregion BookManager

        /// <summary>
        /// Trả về tên loại vũ khí
        /// </summary>
        /// <param name="weaponKind"></param>
        /// <returns></returns>
        public static string GetWeaponKind(int weaponKind)
        {
            switch (weaponKind)
            {
                case (int)KE_EQUIP_WEAPON_CATEGORY.emKEQUIP_WEAPON_CATEGORY_HAND:
                    return "Triền thủ";

                case (int)KE_EQUIP_WEAPON_CATEGORY.emKEQUIP_WEAPON_CATEGORY_SWORD:
                    return "Kiếm";

                case (int)KE_EQUIP_WEAPON_CATEGORY.emKEQUIP_WEAPON_CATEGORY_KNIFE:
                    return "Đao";

                case (int)KE_EQUIP_WEAPON_CATEGORY.emKEQUIP_WEAPON_CATEGORY_STICK:
                    return "Côn";

                case (int)KE_EQUIP_WEAPON_CATEGORY.emKEQUIP_WEAPON_CATEGORY_SPEAR:
                    return "Thương";

                case (int)KE_EQUIP_WEAPON_CATEGORY.emKEQUIP_WEAPON_CATEGORY_HAMMER:
                    return "Chùy";

                case (int)KE_EQUIP_WEAPON_CATEGORY.emKEQUIP_WEAPON_CATEGORY_FLYBAR:
                    return "Phi đao";

                case (int)KE_EQUIP_WEAPON_CATEGORY.emKEQUIP_WEAPON_CATEGORY_ARROW:
                    return "Tụ tiễn";

                case (int)KE_EQUIP_WEAPON_CATEGORY.emKEQUIP_WEAPON_CATEGORY_RANGER_KNIFE:
                    return "Trường đao";

                default:
                    return null;
            }
        }

        public static string GetEquipTypeString(KE_ITEM_EQUIP_DETAILTYPE equipType)
        {
            string ret = "";

            switch (equipType)
            {
                case KE_ITEM_EQUIP_DETAILTYPE.equip_rangeweapon:
                case KE_ITEM_EQUIP_DETAILTYPE.equip_meleeweapon:
                    {
                        ret = "Vũ Khí";
                    }
                    break;

                case KE_ITEM_EQUIP_DETAILTYPE.equip_armor:
                    {
                        ret = "Áo";
                    }
                    break;

                case KE_ITEM_EQUIP_DETAILTYPE.equip_ring:
                    {
                        ret = "Nhẫn";
                    }
                    break;

                case KE_ITEM_EQUIP_DETAILTYPE.equip_amulet:
                    {
                        ret = "Phù";
                    }
                    break;

                case KE_ITEM_EQUIP_DETAILTYPE.equip_boots:
                    {
                        ret = "Giày";
                    }
                    break;

                case KE_ITEM_EQUIP_DETAILTYPE.equip_belt:
                    {
                        ret = "Lưng";
                    }
                    break;

                case KE_ITEM_EQUIP_DETAILTYPE.equip_helm:
                    {
                        ret = "Mũ";
                    }

                    break;

                case KE_ITEM_EQUIP_DETAILTYPE.equip_cuff:
                    {
                        ret = "Tay";
                    }
                    break;

                case KE_ITEM_EQUIP_DETAILTYPE.equip_pendant:
                    {
                        ret = "Bội";
                    }
                    break;

                case KE_ITEM_EQUIP_DETAILTYPE.equip_horse:
                    {
                        ret = "Ngựa";
                    }
                    break;

                case KE_ITEM_EQUIP_DETAILTYPE.equip_mask:
                    {
                        ret = "Mặt Nạ";
                    }
                    break;

                case KE_ITEM_EQUIP_DETAILTYPE.equip_book:
                    {
                        ret = "Mật Tịch";
                    }
                    break;

                case KE_ITEM_EQUIP_DETAILTYPE.equip_ornament:
                    {
                        ret = "Trang sức";
                    }
                    break;

                case KE_ITEM_EQUIP_DETAILTYPE.equip_signet:
                    {
                        ret = "Ấn";
                    }
                    break;

                case KE_ITEM_EQUIP_DETAILTYPE.equip_mantle:
                    {
                        ret = "Phi Phong";
                    }
                    break;

                case KE_ITEM_EQUIP_DETAILTYPE.equip_chop:
                    {
                        ret = "Quan Ấn";
                    }
                    break;

                default:
                    {
                        ret = "Chưa mở";
                    }
                    break;
            }

            return ret;
        }

        public static void ActivateItemConfig()
        {
            ActiveByItem _Active = new ActiveByItem();
            _Active.Pos1 = (int)KE_EQUIP_POSITION.emEQUIPPOS_BODY;
            _Active.Pos2 = (int)KE_EQUIP_POSITION.emEQUIPPOS_AMULET;
            g_anEquipActive.Add(KE_EQUIP_POSITION.emEQUIPPOS_HEAD, _Active);

            _Active = new ActiveByItem();
            _Active.Pos1 = (int)KE_EQUIP_POSITION.emEQUIPPOS_HEAD;
            _Active.Pos2 = (int)KE_EQUIP_POSITION.emEQUIPPOS_WEAPON;
            g_anEquipActive.Add(KE_EQUIP_POSITION.emEQUIPPOS_FOOT, _Active);

            _Active = new ActiveByItem();
            _Active.Pos1 = (int)KE_EQUIP_POSITION.emEQUIPPOS_HEAD;
            _Active.Pos2 = (int)KE_EQUIP_POSITION.emEQUIPPOS_WEAPON;
            g_anEquipActive.Add(KE_EQUIP_POSITION.emEQUIPPOS_RING_2, _Active);

            _Active = new ActiveByItem();
            _Active.Pos1 = (int)KE_EQUIP_POSITION.emEQUIPPOS_FOOT;
            _Active.Pos2 = (int)KE_EQUIP_POSITION.emEQUIPPOS_RING_2;
            g_anEquipActive.Add(KE_EQUIP_POSITION.emEQUIPPOS_CUFF, _Active);

            _Active = new ActiveByItem();
            _Active.Pos1 = (int)KE_EQUIP_POSITION.emEQUIPPOS_FOOT;
            _Active.Pos2 = (int)KE_EQUIP_POSITION.emEQUIPPOS_RING_2;
            g_anEquipActive.Add(KE_EQUIP_POSITION.emEQUIPPOS_PENDANT, _Active);

            // Nhẫn dưới kích dây chuyền và áo
            _Active = new ActiveByItem();
            _Active.Pos1 = (int)KE_EQUIP_POSITION.emEQUIPPOS_PENDANT;
            _Active.Pos2 = (int)KE_EQUIP_POSITION.emEQUIPPOS_CUFF;
            g_anEquipActive.Add(KE_EQUIP_POSITION.emEQUIPPOS_RING, _Active);

            // Thát lưng kích áo và dây chuyền
            _Active = new ActiveByItem();
            _Active.Pos1 = (int)KE_EQUIP_POSITION.emEQUIPPOS_PENDANT;
            _Active.Pos2 = (int)KE_EQUIP_POSITION.emEQUIPPOS_CUFF;
            g_anEquipActive.Add(KE_EQUIP_POSITION.emEQUIPPOS_BELT, _Active);

            // Dây chuyền kích mũ và vũ khí
            _Active = new ActiveByItem();
            _Active.Pos1 = (int)KE_EQUIP_POSITION.emEQUIPPOS_BELT;
            _Active.Pos2 = (int)KE_EQUIP_POSITION.emEQUIPPOS_RING;
            g_anEquipActive.Add(KE_EQUIP_POSITION.emEQUIPPOS_AMULET, _Active);

            // Vũ khí kích nhẫn trên và giày
            _Active = new ActiveByItem();
            _Active.Pos1 = (int)KE_EQUIP_POSITION.emEQUIPPOS_BELT;
            _Active.Pos2 = (int)KE_EQUIP_POSITION.emEQUIPPOS_RING;
            g_anEquipActive.Add(KE_EQUIP_POSITION.emEQUIPPOS_BODY, _Active);

            _Active = new ActiveByItem();
            _Active.Pos1 = (int)KE_EQUIP_POSITION.emEQUIPPOS_AMULET;
            _Active.Pos2 = (int)KE_EQUIP_POSITION.emEQUIPPOS_BODY;
            g_anEquipActive.Add(KE_EQUIP_POSITION.emEQUIPPOS_WEAPON, _Active);
        }

        public static void ItemSetup()
        {
            LoadSigNetXml(SignetExpPath);

            LoadItemFromPath(ITEMROOTPATH);

            string Files = KTGlobal.GetDataPath(MagicAttribLevel);
            using (var stream = System.IO.File.OpenRead(Files))
            {
                var serializer = new XmlSerializer(typeof(List<MagicAttribLevel>));
                TotalMagicAttribLevel = serializer.Deserialize(stream) as List<MagicAttribLevel>;

                //Console.WriteLine("Loading TotalMagicAttribLevel Done : " + TotalMagicAttribLevel.Count);
            }

            Files = KTGlobal.GetDataPath(SuiteActiveProp);
            using (var stream = System.IO.File.OpenRead(Files))
            {
                var serializer = new XmlSerializer(typeof(List<SuiteActiveProp>));
                TotalSuiteActiveProp = serializer.Deserialize(stream) as List<SuiteActiveProp>;

                //Console.WriteLine("Loading TotalSuiteActiveProp Done : " + TotalSuiteActiveProp.Count);
            }

            _TotalGameItem = new ConcurrentDictionary<int, ItemData>(TotalItem.ToDictionary(x => x.ItemID, x => x));

            //Console.WriteLine("Total Item Loading: " + _TotalGameItem.Count);

            ActivateItemConfig();

            Files = KTGlobal.GetDataPath(ItemCacluation);

            using (var stream = System.IO.File.OpenRead(Files))
            {
                var serializer = new XmlSerializer(typeof(ItemValueCaculation));

                _Calutaion = serializer.Deserialize(stream) as ItemValueCaculation;

                //Console.WriteLine("Loading Item Calculation DONE ");
            }

            /// Làm rỗng danh sách đặc biệt
            ItemManager.Mantles.Clear();
            ItemManager.Signets.Clear();
            ItemManager.Chops.Clear();
            /// Duyệt danh sách vật phẩm
            foreach (ItemData itemData in ItemManager.TotalItem)
            {
                /// Nếu đây là ngũ hành ấn
                if (itemData.DetailType == (int)KE_ITEM_EQUIP_DETAILTYPE.equip_signet)
                {
                    ItemManager.Signets.Add(itemData);
                }
                /// Nếu đây là phi phong
                else if (itemData.DetailType == (int)KE_ITEM_EQUIP_DETAILTYPE.equip_mantle)
                {
                    ItemManager.Mantles.Add(itemData);
                }
                /// Nếu đây là quan ấn
                else if (itemData.DetailType == (int)KE_ITEM_EQUIP_DETAILTYPE.equip_chop)
                {
                    ItemManager.Chops.Add(itemData);
                }
            }
        }

        /// <summary>
        /// Kiểm tra vật phẩm có bán được không
        /// </summary>
        /// <param name="itemGD"></param>
        /// <returns></returns>
        public static bool IsCanBeSold(GoodsData itemGD)
        {
            /// Nếu thông tin vật phẩm không tồn tại
            if (!ItemManager._TotalGameItem.TryGetValue(itemGD.GoodsID, out ItemData itemData))
            {
                //return itemData.BindType < 3;
            }
            /// Nếu là trang bị và có cường hóa
            if (ItemManager.IsEquip(itemGD) && itemGD.Forge_level > 0)
            {
                return false;
            }
            /// Trả về không thể bán
            return false;
        }

        /// <summary>
        /// Lấy ra tempate của 1 vật paharm
        /// </summary>
        /// <param name="ItemID"></param>
        /// <returns></returns>
        public static ItemData GetItemTemplate(int ItemID)
        {
            _TotalGameItem.TryGetValue(ItemID, out ItemData itemData);
            return itemData;
        }

        /// <summary>
        /// Tính giá trị của vật phẩm
        /// </summary>
        /// <param name="itemGD"></param>
        /// <returns></returns>
        public static StarLevelStruct ItemValueCalculation(GoodsData itemGD, out long ItemValueTaiPhu, out int TotalLinesCount)
        {
            ItemValueTaiPhu = 0;
            TotalLinesCount = 0;

            try
            {
                /// Nếu không phải trang bị
                if (!ItemManager.IsEquip(itemGD))
                {
                    return null;
                }
                int LevelCuongHoa = itemGD.Forge_level;
                int LevelLuyenHoa = 0;

                ItemData _ItemData = null;
                if (!ItemManager._TotalGameItem.TryGetValue(itemGD.GoodsID, out _ItemData))
                {
                    return null;
                }

                // List ra cái fighPower
                double TotalValue = _ItemData.FightPower;

                /// Nếu vật phẩm có thể cường hóa
                if (CanEquipBeEnhance(_ItemData))
                {
                    double nLevelRate = 0;
                    double nTypeRate = 0;

                    double nEnhValue = 0;
                    double nStrValue = 0;

                    Equip_Type_Rate TypeRate = _Calutaion.List_Equip_Type_Rate.Where(x => (int)x.EquipType == _ItemData.DetailType).FirstOrDefault();
                    if (TypeRate != null)
                    {
                        nTypeRate = TypeRate.Value;
                    }

                    Equip_Level LevelRate = _Calutaion.List_Equip_Level.Where(x => x.Level == _ItemData.Level).FirstOrDefault();
                    if (LevelRate != null)
                    {
                        nLevelRate = LevelRate.Value;
                    }

                    for (int i = 0; i <= LevelCuongHoa; i++)
                    {
                        Enhance_Value _Enhance_Value = _Calutaion.List_Enhance_Value.Where(x => x.EnhanceTimes == i).FirstOrDefault();
                        if (_Enhance_Value != null)
                        {
                            nEnhValue = nEnhValue + _Enhance_Value.Value;
                        }
                    }

                    if (LevelLuyenHoa > 0)
                    {
                        Strengthen_Value _Strengthen_Value = _Calutaion.List_Strengthen_Value.Where(x => x.StrengthenTimes == LevelLuyenHoa).FirstOrDefault();
                        if (_Strengthen_Value != null)
                        {
                            nStrValue = _Strengthen_Value.Value;
                        }
                    }

                    List<KMagicInfo> TotalProbs = new List<KMagicInfo>();

                    // Nếu như vật phẩm này có probs rồi
                    // Giải mã level ra
                    if (!string.IsNullOrEmpty(itemGD.Props) && !itemGD.Props.Contains("ERORR"))
                    {
                        byte[] Base64Decode = Convert.FromBase64String(itemGD.Props);

                        // Giải mã toàn bộ chỉ số đã ghi trong gamedb
                        ItemDataByteCode _ItemBuild = DataHelper.BytesToObject<ItemDataByteCode>(Base64Decode, 0, Base64Decode.Length);

                        // Add vào đây thuộc tính dòng xanh
                        if (_ItemBuild.GreenPropCount > 0)
                        {
                            TotalProbs.AddRange(_ItemBuild.GreenProp);
                        }
                        // Add vào đây thuộc tính ẩn
                        if (_ItemBuild.HiddenProbsCount > 0)
                        {
                            TotalProbs.AddRange(_ItemBuild.HiddenProbs);
                        }
                        // Cộng tổng dòng xanh và dòng ẩn của đồ
                        TotalLinesCount += _ItemBuild.GreenPropCount;
                        TotalLinesCount += _ItemBuild.HiddenProbsCount;
                    } // Còn nếu vật phẩm này đéo có dòng nào lưu vào CSDL thì lây ra từ TMP cơ bản
                    else
                    {
                        List<PropMagic> GreenProp = _ItemData.GreenProp;

                        List<PropMagic> HiddenProb = _ItemData.HiddenProp;
                        if (GreenProp != null)
                        {
                            // Nếu như đồ có dòng xanh
                            if (GreenProp.Count > 0)
                            {
                                // Duyệt tất cả dòng xanh
                                foreach (PropMagic _probs in GreenProp)
                                {
                                    if (PropertyDefine.PropertiesBySymbolName.ContainsKey(_probs.MagicName))
                                    {
                                        int MagicID = PropertyDefine.PropertiesBySymbolName[_probs.MagicName].ID;

                                        KMagicInfo _InfoMagic = new KMagicInfo();
                                        _InfoMagic.nAttribType = MagicID;

                                        // Nếu đây là thuộc tính random từ đâu tới dâu
                                        if (_probs.MagicLevel.Contains('|'))
                                        {
                                            int MINVALUE = Int32.Parse(_probs.MagicLevel.Split('|')[0]);
                                            int MAXVALUE = Int32.Parse(_probs.MagicLevel.Split('|')[1]);

                                            int Level = KTGlobal.GetRandomNumber(MINVALUE, MAXVALUE);
                                            _InfoMagic.nLevel = Level;
                                        }
                                        else
                                        {
                                            _InfoMagic.nLevel = Int32.Parse(_probs.MagicLevel);
                                        }

                                        TotalProbs.Add(_InfoMagic);
                                    }
                                }
                            }
                        }
                        if (HiddenProb != null)
                        {
                            // Nếu như đồ có dòng xanh
                            if (HiddenProb.Count > 0)
                            {
                                // Duyệt tất cả dòng xanh
                                foreach (PropMagic _probs in HiddenProb)
                                {
                                    if (PropertyDefine.PropertiesBySymbolName.ContainsKey(_probs.MagicName))
                                    {
                                        int MagicID = PropertyDefine.PropertiesBySymbolName[_probs.MagicName].ID;

                                        KMagicInfo _InfoMagic = new KMagicInfo();
                                        _InfoMagic.nAttribType = MagicID;

                                        // Nếu đây là thuộc tính random từ đâu tới dâu
                                        if (_probs.MagicLevel.Contains('|'))
                                        {
                                            int MINVALUE = Int32.Parse(_probs.MagicLevel.Split('|')[0]);
                                            int MAXVALUE = Int32.Parse(_probs.MagicLevel.Split('|')[1]);

                                            int Level = KTGlobal.GetRandomNumber(MINVALUE, MAXVALUE);
                                            _InfoMagic.nLevel = Level;
                                        }
                                        else
                                        {
                                            _InfoMagic.nLevel = Int32.Parse(_probs.MagicLevel);
                                        }

                                        TotalProbs.Add(_InfoMagic);
                                    }
                                }
                            }
                        }
                    }

                    Dictionary<int, double> tbValue = new Dictionary<int, double>();

                    int MagicCount = 1;

                    // Vòng for thứ nhất để tính tổng VALUE của các symboy trong magicatribute level
                    foreach (KMagicInfo _probs in TotalProbs)
                    {
                        double Rate = 100;

                        Equip_Random_Pos _Rate = _Calutaion.List_Equip_Random_Pos.Where(x => x.MAGIC_POS == MagicCount).FirstOrDefault();
                        if (_Rate != null)
                        {
                            Rate = (double)_Rate.Value / 100;
                        }

                        MagicAttribLevel _Atribute = TotalMagicAttribLevel.Where(x => x.MAGIC_ID == _probs.nAttribType && x.Level == _probs.nLevel).FirstOrDefault();
                        if (_Atribute != null)
                        {
                            long Value = _Atribute.ItemValue;

                            double FinalValue = Math.Floor(Rate * Value);

                            tbValue.Add(MagicCount, FinalValue);

                            TotalValue += FinalValue;
                        }

                        MagicCount++;
                    }

                    //Vòng for thứ 2 để tính điểm giữa các dòng với nhau
                    for (int i = 1; i <= TotalProbs.Count; i++)
                    {
                        KMagicInfo SourceProb = TotalProbs[i - 1];

                        for (int j = 1; j <= TotalProbs.Count; j++)
                        {
                            KMagicInfo DescProb = TotalProbs[j - 1];

                            if (PropertyDefine.PropertiesByID.ContainsKey(SourceProb.nAttribType))
                            {
                                string SourceMagicName = PropertyDefine.PropertiesByID[SourceProb.nAttribType].SymbolName;

                                MagicSource _FindMagicSource = _Calutaion.Magic_Combine_Def.MagicSourceDef.Where(x => x.MagicName == SourceMagicName).FirstOrDefault();

                                if (_FindMagicSource != null)
                                {
                                    int SelectValue = _FindMagicSource.Index;

                                    string DestMagicName = PropertyDefine.PropertiesByID[DescProb.nAttribType].SymbolName;

                                    MagicDesc _FindMagicDest = _Calutaion.Magic_Combine_Def.MagicDescDef.Where(x => x.MagicName == DestMagicName).FirstOrDefault();
                                    if (_FindMagicDest != null)
                                    {
                                        try
                                        {
                                            if (_FindMagicDest.ListValue.Count() > SelectValue)
                                            {
                                                //Console.WriteLine("CHECK : " + SourceProb.MagicName + "===>" + DescProb.MagicName);

                                                double Value = _FindMagicDest.ListValue[SelectValue];

                                                double nRate = Math.Sqrt(Value) / 10;

                                                nRate = (nRate - 1) * SourceProb.nLevel * DescProb.nLevel / 400;

                                                double FinalValue = Math.Floor((tbValue[i] + tbValue[j]) * nRate);

                                                //  Console.WriteLine("FINAL VALUE :" + FinalValue);

                                                TotalValue += FinalValue;
                                            }
                                        }
                                        catch (Exception exx)
                                        {
                                            // Console.WriteLine(exx.ToString());
                                        }
                                    }
                                }
                            }
                        }
                    }

                    TotalValue = Math.Floor(TotalValue / 100 * nLevelRate);

                    TotalValue = Math.Floor(TotalValue / 100 * nTypeRate);

                    TotalValue = TotalValue + Math.Floor(nEnhValue / 100 * nTypeRate);

                    TotalValue = TotalValue + Math.Floor(nStrValue / 100 * nTypeRate);

                    //  Console.WriteLine("FINAL TAI PHU :" + TotalValue);
                }
                else if (ItemManager.KD_ISSIGNET(_ItemData.DetailType))
                {
                    try
                    {
                        /// Cường hóa ngũ hành tương khắc
                        {
                            string[] param = itemGD.OtherParams[ItemPramenter.Pram_1].Split('|');
                            int seriesEnhance = int.Parse(param[0]);
                            int seriesEnhanceExp = int.Parse(param[1]);
                            /// Tăng tài phú tương ứng
                            TotalValue += ItemManager._TotalSingNetExp[seriesEnhance - 1].Value;
                        }
                        /// Nhược hóa ngũ hành tương khắc
                        {
                            string[] param = itemGD.OtherParams[ItemPramenter.Pram_2].Split('|');
                            int seruesConque = int.Parse(param[0]);
                            int seriesConqueExp = int.Parse(param[1]);
                            /// Tăng tài phú tương ứng
                            TotalValue += ItemManager._TotalSingNetExp[seruesConque - 1].Value;
                        }
                    }
                    catch (Exception ex)
                    {
                        // Console.WriteLine(ex.ToString());
                    }
                }

                ItemValueTaiPhu = (long)TotalValue;

                //  LogManager.WriteLog(LogTypes.Item, "[" + _ItemData.ItemID + "][" + _ItemData.Name + "] TOTALVALUE :" + ItemValueTaiPhu);

                int ItemType = _ItemData.DetailType;
                int ItemLevel = _ItemData.Level;

                Equip_StarLevel List_Equip_StarLevel = null;
                int LevelStart = 0;

                if (_ItemData.Genre == 7)
                {
                    ItemType = ItemType + 100;
                }

                if (ItemLevel == 1)
                {
                    List_Equip_StarLevel = _Calutaion.List_Equip_StarLevel.Where(x => x.EQUIP_DETAIL_TYPE == ItemType && TotalValue >= x.EQUIP_LEVEL_1).LastOrDefault();
                }
                else if (ItemLevel == 2)
                {
                    List_Equip_StarLevel = _Calutaion.List_Equip_StarLevel.Where(x => x.EQUIP_DETAIL_TYPE == ItemType && TotalValue >= x.EQUIP_LEVEL_2).LastOrDefault();
                }
                else if (ItemLevel == 3)
                {
                    List_Equip_StarLevel = _Calutaion.List_Equip_StarLevel.Where(x => x.EQUIP_DETAIL_TYPE == ItemType && TotalValue >= x.EQUIP_LEVEL_3).LastOrDefault();
                }
                else if (ItemLevel == 4)
                {
                    List_Equip_StarLevel = _Calutaion.List_Equip_StarLevel.Where(x => x.EQUIP_DETAIL_TYPE == ItemType && TotalValue >= x.EQUIP_LEVEL_4).LastOrDefault();
                }
                else if (ItemLevel == 5)
                {
                    List_Equip_StarLevel = _Calutaion.List_Equip_StarLevel.Where(x => x.EQUIP_DETAIL_TYPE == ItemType && TotalValue >= x.EQUIP_LEVEL_5).LastOrDefault();
                }
                else if (ItemLevel == 6)
                {
                    List_Equip_StarLevel = _Calutaion.List_Equip_StarLevel.Where(x => x.EQUIP_DETAIL_TYPE == ItemType && TotalValue >= x.EQUIP_LEVEL_6).LastOrDefault();
                }
                else if (ItemLevel == 7)
                {
                    List_Equip_StarLevel = _Calutaion.List_Equip_StarLevel.Where(x => x.EQUIP_DETAIL_TYPE == ItemType && TotalValue >= x.EQUIP_LEVEL_7).LastOrDefault();
                }
                else if (ItemLevel == 8)
                {
                    List_Equip_StarLevel = _Calutaion.List_Equip_StarLevel.Where(x => x.EQUIP_DETAIL_TYPE == ItemType && TotalValue >= x.EQUIP_LEVEL_8).LastOrDefault();
                }
                else if (ItemLevel == 9)
                {
                    List_Equip_StarLevel = _Calutaion.List_Equip_StarLevel.Where(x => x.EQUIP_DETAIL_TYPE == ItemType && TotalValue >= x.EQUIP_LEVEL_9).LastOrDefault();
                }
                else if (ItemLevel == 10)
                {
                    List_Equip_StarLevel = _Calutaion.List_Equip_StarLevel.Where(x => x.EQUIP_DETAIL_TYPE == ItemType && TotalValue >= x.EQUIP_LEVEL_10).LastOrDefault();
                }
                if (List_Equip_StarLevel != null)
                {
                    LevelStart = List_Equip_StarLevel.STAR_LEVEL;
                }

                StarLevelStruct _LevelSelect = _Calutaion.List_StarLevelStruct.Where(x => x.StarLevel == LevelStart).FirstOrDefault();
                if (_LevelSelect == null)
                {
                    return null;
                }

                _LevelSelect.Value = (long)TotalValue;
                return _LevelSelect;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Fill Item không random thuộc tính theo MagicAtributeLevel mà nó phụ thuộc vào config cứng trong ItemTempate
        /// Thường được sử dụng để gọi đồ hoàng kim
        /// </summary>
        /// <param name="ItemData"></param>
        /// <returns></returns>
        public static string GenerateProbs(ItemData ItemData)
        {
            if (!KD_ISEQUIP(ItemData.Genre) && !KD_ISPETEQUIP(ItemData.Genre))
            {
                return "";
            }

            ItemDataByteCode _ItemBuild = new ItemDataByteCode();

            // WRITER PROPS
            if (ItemData.ListBasicProp.Count > 0)
            {
                List<BasicProp> List = ItemData.ListBasicProp.OrderBy(x => x.Index).ToList();

                _ItemBuild.BasicPropCount = ItemData.ListBasicProp.Count;

                // Tạo mới 1 mảng để lưu thông tin
                _ItemBuild.BasicProp = new List<KMagicInfo>();

                foreach (BasicProp _Probs in List)
                {
                    KMagicInfo _Magic = new KMagicInfo();

                    // Xem trong dict có tồn tại cái symboy này không
                    if (PropertyDefine.PropertiesBySymbolName.ContainsKey(_Probs.BasicPropType))
                    {
                        // Lấy ra ID của SYMBOY
                        int MagicID = PropertyDefine.PropertiesBySymbolName[_Probs.BasicPropType].ID;

                        // méo có level
                        _Magic.nLevel = -1;
                        // Gán ID cho nó
                        _Magic.nAttribType = MagicID;

                        // Đối với basicprob thì ko có sysboy tương ứng nên sẽ lưu thẳng vào DB không check lại
                        if (_Probs.BasicPropPA1Min > 0)
                        {
                            int RandomValue = KTGlobal.GetRandomNumber(_Probs.BasicPropPA1Min, _Probs.BasicPropPA1Max);

                            // Lưu thẳng RANDOMVAUE RA
                            _Magic.Value_1 = RandomValue;
                        }
                        else
                        {
                            _Magic.Value_1 = -1;
                        }
                        if (_Probs.BasicPropPA2Min > 0)
                        {
                            int RandomValue = KTGlobal.GetRandomNumber(_Probs.BasicPropPA2Min, _Probs.BasicPropPA2Max);

                            _Magic.Value_2 = RandomValue;
                        }
                        else
                        {
                            _Magic.Value_2 = -1;
                        }
                        if (_Probs.BasicPropPA3Min > 0)
                        {
                            int RandomValue = KTGlobal.GetRandomNumber(_Probs.BasicPropPA3Min, _Probs.BasicPropPA3Max);

                            _Magic.Value_3 = RandomValue;
                        }
                        else
                        {
                            _Magic.Value_3 = -1;
                        }
                    }
                    else
                    {
                        _Magic.nAttribType = -1;
                        _Magic.nLevel = -1;
                        _Magic.Value_1 = -1;
                        _Magic.Value_2 = -1;
                        _Magic.Value_3 = -1;
                    }

                    _ItemBuild.BasicProp.Add(_Magic);
                }
            }
            else
            {
                _ItemBuild.BasicPropCount = 0;
            }

            // Danh sách thuộc tính hiện
            if (ItemData.GreenProp == null)
            {
                _ItemBuild.GreenPropCount = 0;
            }
            else
            {
                _ItemBuild.GreenPropCount = ItemData.GreenProp.Count;

                // Green Value
                _ItemBuild.GreenProp = new List<KMagicInfo>();

                List<PropMagic> List = ItemData.GreenProp.OrderBy(x => x.Index).ToList();

                foreach (PropMagic _Probs in List)
                {
                    KMagicInfo _Magic = new KMagicInfo();

                    if (_Probs.MagicName.Length > 0)
                    {
                        // Nếu mà symboy có dấu gạch này xử lý phân cấp xử lý riêng cho việc add skill vào đồ
                        if (_Probs.MagicName.Contains("|"))
                        {
                            string MagicName = _Probs.MagicName.Split('|')[0];
                            string Pram_1 = _Probs.MagicName.Split('|')[1];
                            string Pram_2 = _Probs.MagicName.Split('|')[2];
                            string Pram_3 = _Probs.MagicName.Split('|')[3];

                            // Nếu mà có symboy này trong prodict
                            if (PropertyDefine.PropertiesBySymbolName.ContainsKey(MagicName))
                            {
                                int MagicLevel = 0;
                                if (_Probs.MagicLevel.Contains("|"))
                                {
                                    int MinValueLevel = Int32.Parse(_Probs.MagicLevel.Split('|')[0]);
                                    int MaxValueLevel = Int32.Parse(_Probs.MagicLevel.Split('|')[1]);

                                    MagicLevel = KTGlobal.GetRandomNumber(MinValueLevel, MaxValueLevel);
                                }
                                else
                                {
                                    MagicLevel = Int32.Parse(_Probs.MagicLevel);
                                }
                                // Lấy ra magic ID
                                int MagicID = PropertyDefine.PropertiesBySymbolName[MagicName].ID;
                                _Magic.nAttribType = MagicID;
                                _Magic.nLevel = MagicLevel;
                                _Magic.Value_1 = Int32.Parse(Pram_1);

                                // Xử lý cho kỹ năng
                                int MinValue = Int32.Parse(Pram_2);
                                int MaxValue = Int32.Parse(Pram_3);

                                int ValueGet = KTGlobal.GetRandomNumber(MinValue, MaxValue);
                                _Magic.Value_2 = ValueGet;
                                _Magic.Value_3 = -1;
                            }
                            else
                            {
                                // Nếu ko tồn tại gì thì đây
                                _Magic.nAttribType = -100;
                                _Magic.nLevel = -1;
                                _Magic.Value_1 = -1;
                                _Magic.Value_2 = -1;
                                _Magic.Value_3 = -1;
                            }
                        }
                        else
                        {
                            if (PropertyDefine.PropertiesBySymbolName.ContainsKey(_Probs.MagicName))
                            {
                                int MagicLevel = 0;
                                if (_Probs.MagicLevel.Contains("|"))
                                {
                                    int MinValueLevel = Int32.Parse(_Probs.MagicLevel.Split('|')[0]);
                                    int MaxValueLevel = Int32.Parse(_Probs.MagicLevel.Split('|')[1]);

                                    MagicLevel = KTGlobal.GetRandomNumber(MinValueLevel, MaxValueLevel);
                                }
                                else
                                {
                                    MagicLevel = Int32.Parse(_Probs.MagicLevel);
                                }

                                MagicAttribLevel FindMagic = TotalMagicAttribLevel.Where(x => x.MagicName == _Probs.MagicName && x.Level == MagicLevel).FirstOrDefault();

                                if (FindMagic != null)
                                {
                                    int MagicID = PropertyDefine.PropertiesBySymbolName[_Probs.MagicName].ID;

                                    _Magic.nAttribType = MagicID;
                                    _Magic.nLevel = MagicLevel;

                                    if (FindMagic.MA1Min > 0)
                                    {
                                        int RandomValue = KTGlobal.GetRandomNumber(FindMagic.MA1Min, FindMagic.MA1Max);

                                        int DIV = RandomValue - FindMagic.MA1Min;

                                        _Magic.Value_1 = DIV;
                                    }
                                    else
                                    {
                                        _Magic.Value_1 = -1;
                                    }

                                    if (FindMagic.MA2Min > 0)
                                    {
                                        int RandomValue = KTGlobal.GetRandomNumber(FindMagic.MA2Min, FindMagic.MA2Max);

                                        int DIV = RandomValue - FindMagic.MA2Min;

                                        _Magic.Value_2 = DIV;
                                    }
                                    else
                                    {
                                        _Magic.Value_2 = -1;
                                    }

                                    if (FindMagic.MA3Min > 0)
                                    {
                                        int RandomValue = KTGlobal.GetRandomNumber(FindMagic.MA3Min, FindMagic.MA3Max);

                                        int DIV = RandomValue - FindMagic.MA3Min;

                                        _Magic.Value_3 = DIV;
                                    }
                                    else
                                    {
                                        _Magic.Value_3 = -1;
                                    }
                                }
                                else // Nếu không có set MAGIC LEVEL  -1
                                {
                                    _Magic.nAttribType = -200;

                                    _Magic.nLevel = -1;
                                    _Magic.Value_1 = -1;
                                    _Magic.Value_2 = -1;
                                    _Magic.Value_3 = -1;
                                }
                            }
                            else
                            {
                                _Magic.nAttribType = -100;
                                _Magic.nLevel = -1;
                                _Magic.Value_1 = -1;
                                _Magic.Value_2 = -1;
                                _Magic.Value_3 = -1;
                            }
                        }
                    }
                    else
                    {
                        _Magic.nAttribType = -2;
                        _Magic.nLevel = -1;
                        _Magic.Value_1 = -1;
                        _Magic.Value_2 = -1;
                        _Magic.Value_3 = -1;
                    }

                    _ItemBuild.GreenProp.Add(_Magic);
                }
            }

            // Đây là danh sách thuộc tính ẩn max chắc 3 thuộc tính
            if (ItemData.HiddenProp == null)
            {
                _ItemBuild.HiddenProbsCount = 0;
            }
            else
            {
                if (ItemData.HiddenProp.Count > 0)
                {
                    _ItemBuild.HiddenProbsCount = ItemData.HiddenProp.Count;

                    _ItemBuild.HiddenProbs = new List<KMagicInfo>();

                    List<PropMagic> List = ItemData.HiddenProp.OrderBy(x => x.Index).ToList();

                    foreach (PropMagic _Probs in List)
                    {
                        KMagicInfo _Magic = new KMagicInfo();

                        if (_Probs.MagicName.Length > 0)
                        {
                            if (_Probs.MagicName.Contains("|"))
                            {
                                string MagicName = _Probs.MagicName.Split('|')[0];
                                string Pram_1 = _Probs.MagicName.Split('|')[1];
                                string Pram_2 = _Probs.MagicName.Split('|')[2];
                                string Pram_3 = _Probs.MagicName.Split('|')[3];

                                // Nếu mà có symboy này trong prodict
                                if (PropertyDefine.PropertiesBySymbolName.ContainsKey(MagicName))
                                {
                                    // Lấy ra magic ID
                                    int MagicID = PropertyDefine.PropertiesBySymbolName[MagicName].ID;

                                    int MagicLevel = 0;
                                    if (_Probs.MagicLevel.Contains("|"))
                                    {
                                        int MinValueLevel = Int32.Parse(_Probs.MagicLevel.Split('|')[0]);
                                        int MaxValueLevel = Int32.Parse(_Probs.MagicLevel.Split('|')[1]);

                                        MagicLevel = KTGlobal.GetRandomNumber(MinValueLevel, MaxValueLevel);
                                    }
                                    else
                                    {
                                        MagicLevel = Int32.Parse(_Probs.MagicLevel);
                                    }

                                    _Magic.nAttribType = MagicID;
                                    _Magic.nLevel = MagicLevel;
                                    _Magic.Value_1 = Int32.Parse(Pram_1);

                                    // Xử lý cho kỹ năng
                                    int MinValue = Int32.Parse(Pram_2);
                                    int MaxValue = Int32.Parse(Pram_3);

                                    int ValueGet = KTGlobal.GetRandomNumber(MinValue, MaxValue);
                                    _Magic.Value_2 = ValueGet;
                                    _Magic.Value_3 = -1;
                                }
                                else
                                {
                                    // Nếu ko tồn tại gì thì đây
                                    _Magic.nAttribType = -100;
                                    _Magic.nLevel = -1;
                                    _Magic.Value_1 = -1;
                                    _Magic.Value_2 = -1;
                                    _Magic.Value_3 = -1;
                                }
                            }
                            else if (PropertyDefine.PropertiesBySymbolName.ContainsKey(_Probs.MagicName))
                            {
                                int MagicLevel = 0;
                                if (_Probs.MagicLevel.Contains("|"))
                                {
                                    int MinValueLevel = Int32.Parse(_Probs.MagicLevel.Split('|')[0]);
                                    int MaxValueLevel = Int32.Parse(_Probs.MagicLevel.Split('|')[1]);

                                    MagicLevel = KTGlobal.GetRandomNumber(MinValueLevel, MaxValueLevel);
                                }
                                else
                                {
                                    MagicLevel = Int32.Parse(_Probs.MagicLevel);
                                }

                                MagicAttribLevel FindMagic = TotalMagicAttribLevel.Where(x => x.MagicName == _Probs.MagicName && x.Level == MagicLevel).FirstOrDefault();

                                if (FindMagic != null)
                                {
                                    int MagicID = PropertyDefine.PropertiesBySymbolName[_Probs.MagicName].ID;

                                    _Magic.nAttribType = MagicID;

                                    _Magic.nLevel = MagicLevel;

                                    if (FindMagic.MA1Min > 0)
                                    {
                                        int RandomValue = KTGlobal.GetRandomNumber(FindMagic.MA1Min, FindMagic.MA1Max);

                                        int DIV = RandomValue - FindMagic.MA1Min;

                                        _Magic.Value_1 = DIV;
                                    }
                                    else
                                    {
                                        _Magic.Value_1 = -1;
                                    }

                                    if (FindMagic.MA2Min > 0)
                                    {
                                        int RandomValue = KTGlobal.GetRandomNumber(FindMagic.MA2Min, FindMagic.MA2Max);

                                        int DIV = RandomValue - FindMagic.MA2Min;

                                        _Magic.Value_2 = DIV;
                                    }
                                    else
                                    {
                                        _Magic.Value_2 = -1;
                                    }

                                    if (FindMagic.MA3Min > 0)
                                    {
                                        int RandomValue = KTGlobal.GetRandomNumber(FindMagic.MA3Min, FindMagic.MA3Max);

                                        int DIV = RandomValue - FindMagic.MA3Min;

                                        _Magic.Value_3 = DIV;
                                    }
                                    else
                                    {
                                        _Magic.Value_3 = -1;
                                    }
                                }
                                else // Nếu không có set MAGIC LEVEL  -1
                                {
                                    _Magic.nAttribType = -200;
                                    _Magic.nLevel = 0;
                                    _Magic.Value_1 = -1;
                                    _Magic.Value_2 = -1;
                                    _Magic.Value_3 = -1;
                                }
                            }
                            else
                            {
                                _Magic.nAttribType = -100;
                                _Magic.nLevel = 0;
                                _Magic.Value_1 = -1;
                                _Magic.Value_2 = -1;
                                _Magic.Value_3 = -1;
                            }
                        }
                        else
                        {
                            _Magic.nAttribType = -2;
                            _Magic.nLevel = 0;
                            _Magic.Value_1 = -1;
                            _Magic.Value_2 = -1;
                            _Magic.Value_3 = -1;
                        }

                        _ItemBuild.HiddenProbs.Add(_Magic);
                    }
                }
            }

            byte[] ItemDataByteArray = DataHelper.ObjectToBytes(_ItemBuild);
            if (ItemDataByteArray.Length == 0)
            {
                return "ERORR";
            }
            else
            {
                return Convert.ToBase64String(ItemDataByteArray);
            }
        }

        /// <summary>
        /// Tạo ra 1 đồ ngẫu nhiên dòng v
        /// Theo số dòng chỉ định
        /// </summary>
        /// <param name="ItemID"></param>
        /// <param name="LinesCount"></param>
        /// <param name="Series"></param>
        public static void CreateItemRandomLine(KPlayer client, int ItemID, int LinesCount, int Series = -1)
        {
            ItemData _FindTmp = ItemManager.GetItemTemplate(ItemID);
            if (_FindTmp != null)
            {
                int nGenre = _FindTmp.Genre;
                int nDetail = _FindTmp.DetailType;

                //Tạo một mảng để lưu level của nó
                int[] pnMagicLevel = new int[LinesCount];
                // set Level magic cho từng thằng
                for (int k = 0; k < LinesCount; k++)
                {
                    int _Random = KTGlobal.GetRandomNumber(KTGlobal.KT_MIN_LEVELMAGIC, KTGlobal.KT_MAX_LEVELMAGIC);

                    pnMagicLevel[k] = _Random;
                }

                int nSeries = ItemManager.GetItemSeries(_FindTmp.Series);
                // nếu có chỉ định series
                if (Series != -1)
                {
                    nSeries = Series;
                }
                List<MagicAttribLevel> Mark = new List<MagicAttribLevel>();

                MagicAttribLevel[] pMagicAttrTable = { null, null, null, null, null, null };

                int g = 0;

                for (g = 0; g < LinesCount; g++)
                {
                    //// Nếu mà = 0 thì tức là ko có dòng nào khác nữa
                    //if (pnMagicLevel[g] == 0)
                    //    break;

                    //int TYPE = 1 - (g & 1);

                    List<MagicAttribLevel> GetAllCorrectInList = new List<MagicAttribLevel>();

                    GetAllCorrectInList = ItemManager.TotalMagicAttribLevel.Where(x => x.RATE(nDetail, nGenre) > 0/* && x.Suffix == TYPE*/ && (x.Series == nSeries || x.Series == -1) && x.Level == pnMagicLevel[g]).ToList();

                    // Nếu ko tìm thấy thuộc tính nào thì break
                    if (GetAllCorrectInList.Count == 0)
                    {
                        break;
                    }

                    LogManager.WriteLog(LogTypes.Item, "GEN DROP OF ITEM :" + _FindTmp.Name);

                    long TotalSumDrop = GetAllCorrectInList.Sum(x => x.RATE(nDetail, nGenre));

                    long RanndomValue = KTGlobal.GetRandomNumber(0, TotalSumDrop);

                    foreach (MagicAttribLevel _TmpMagicAttribLevel in GetAllCorrectInList)
                    {
                        LogManager.WriteLog(LogTypes.Item, "MAGICNAME :" + _TmpMagicAttribLevel.MagicName + "| RATE :" + _TmpMagicAttribLevel.RATE(nDetail, nGenre));
                    }

                    long SelectValue = 0;

                    foreach (MagicAttribLevel _TmpMagicAttribLevel in GetAllCorrectInList)
                    {
                        if (_TmpMagicAttribLevel == null)
                        {
                            continue;
                        }
                        // Nếu dòng này đã chọn rồi thì bỏ qua
                        var FindDupper = Mark.Where(x => x.MAGIC_ID == _TmpMagicAttribLevel.MAGIC_ID).FirstOrDefault();
                        if (FindDupper != null)
                        {
                            continue;
                        }
                        SelectValue = SelectValue + _TmpMagicAttribLevel.RATE(nDetail, nGenre);

                        if (SelectValue >= RanndomValue)
                        {
                            //  LogManager.WriteLog(LogTypes.Item, "SELECT VALUE :" + _TmpMagicAttribLevel.MagicName);
                            Mark.Add(_TmpMagicAttribLevel);
                            pMagicAttrTable[g] = _TmpMagicAttribLevel;
                            break;
                        }
                    }
                }
                //CODE GEN RA PROBS của trang bị
                string ItemProbsGeneter = ItemManager.GenerateProbsWithRandomLine(_FindTmp, pMagicAttrTable);

                // Sau khi random xong thì ta được bảng pMagicAttrTable cuối cùng đây là bảng sẽ truyền sang ItemManger dể gen ra PROBS ITEM lưu vào DB
                // Nếu có vật phẩm thực hiện random ra dòng

                if (!ItemManager.CreateItem(Global._TCPManager.TcpOutPacketPool, client, _FindTmp.ItemID, 1, 0, "GMCOMMAND", false, 0, false, ItemManager.ConstGoodsEndTime, ItemProbsGeneter, nSeries))
                {
                    KTPlayerManager.ShowNotification(client, "Có lỗi khi tạo vật phẩm!");
                    //return;
                }
            }
            else
            {
                KTPlayerManager.ShowNotification(client, "Vật phẩm không tồn tại");
            }
        }

        /// <summary>
        /// Gen PROB dựa vào (1-6 dòng phẩm chất đã random được)
        /// </summary>
        /// <param name="ItemData"></param>
        /// <param name="pMagicAttrTable"></param>
        /// <returns></returns>
        public static string GenerateProbsWithRandomLine(ItemData ItemData, MagicAttribLevel[] pMagicAttrTable)
        {
            if (!KD_ISEQUIP(ItemData.Genre) && !KD_ISPETEQUIP(ItemData.Genre))
            {
                return "";
            }

            ItemDataByteCode _ItemBuild = new ItemDataByteCode();

            // BasicPro chắc chắn phải có dựa trên ITEMDATA
            if (ItemData.ListBasicProp.Count > 0)
            {
                List<BasicProp> List = ItemData.ListBasicProp.OrderBy(x => x.Index).ToList();

                _ItemBuild.BasicPropCount = ItemData.ListBasicProp.Count;

                // Tạo mới 1 mảng để lưu thông tin
                _ItemBuild.BasicProp = new List<KMagicInfo>();

                foreach (BasicProp _Probs in List)
                {
                    KMagicInfo _Magic = new KMagicInfo();

                    // Xem trong dict có tồn tại cái symboy này không
                    if (PropertyDefine.PropertiesBySymbolName.ContainsKey(_Probs.BasicPropType))
                    {
                        // Lấy ra ID của SYMBOY
                        int MagicID = PropertyDefine.PropertiesBySymbolName[_Probs.BasicPropType].ID;

                        // Gán ID cho nó
                        _Magic.nAttribType = MagicID;
                        _Magic.nLevel = -1;

                        if (_Probs.BasicPropPA1Min > 0)
                        {
                            int RandomValue = KTGlobal.GetRandomNumber(_Probs.BasicPropPA1Min, _Probs.BasicPropPA1Max);
                            // set thẳng giá trị vì dòng này ko bị tác động khi cường hóa
                            _Magic.Value_1 = RandomValue;
                        }
                        else
                        {
                            _Magic.Value_1 = -1;
                        }
                        if (_Probs.BasicPropPA2Min > 0)
                        {
                            int RandomValue = KTGlobal.GetRandomNumber(_Probs.BasicPropPA2Min, _Probs.BasicPropPA2Max);

                            _Magic.Value_2 = RandomValue;
                        }
                        else
                        {
                            _Magic.Value_2 = -1;
                        }
                        if (_Probs.BasicPropPA3Min > 0)
                        {
                            int RandomValue = KTGlobal.GetRandomNumber(_Probs.BasicPropPA3Min, _Probs.BasicPropPA3Max);

                            _Magic.Value_3 = RandomValue;
                        }
                        else
                        {
                            _Magic.Value_3 = -1;
                        }
                    }
                    else
                    {
                        _Magic.nAttribType = -1;
                        _Magic.nLevel = -1;
                        _Magic.Value_1 = -1;
                        _Magic.Value_2 = -1;
                        _Magic.Value_3 = -1;
                    }

                    _ItemBuild.BasicProp.Add(_Magic);
                }
            }
            else
            {
                _ItemBuild.BasicPropCount = 0;
            }

            List<MagicAttribLevel> GreenProbs = new List<MagicAttribLevel>();
            List<MagicAttribLevel> HiddenProbs = new List<MagicAttribLevel>();

            for (int i = 0; i < 6; i++)
            {
                MagicAttribLevel _Magic = pMagicAttrTable[i];

                if (_Magic != null)
                {
                    if (ItemData.Genre == 7)
                    {
                        GreenProbs.Add(_Magic);
                    }
                    else
                    {
                        // Nếu là dòng chẵn
                        if (i % 2 == 0)
                        {
                            GreenProbs.Add(_Magic);
                        }
                        else
                        {
                            HiddenProbs.Add(_Magic);
                        }
                    }
                }
            }

            // Danh sách thuộc tính hiện
            if (GreenProbs.Count > 0)
            {
                _ItemBuild.GreenPropCount = GreenProbs.Count;

                // Green Value
                _ItemBuild.GreenProp = new List<KMagicInfo>();

                foreach (MagicAttribLevel _Probs in GreenProbs)
                {
                    KMagicInfo _Magic = new KMagicInfo();

                    if (PropertyDefine.PropertiesBySymbolName.ContainsKey(_Probs.MagicName))
                    {
                        MagicAttribLevel FindMagic = TotalMagicAttribLevel.Where(x => x.MagicName == _Probs.MagicName && x.Level == _Probs.Level).FirstOrDefault();

                        if (FindMagic != null)
                        {
                            int MagicID = PropertyDefine.PropertiesBySymbolName[_Probs.MagicName].ID;

                            _Magic.nAttribType = MagicID;
                            _Magic.nLevel = _Probs.Level;
                            if (FindMagic.MA1Min > 0)
                            {
                                int RandomValue = KTGlobal.GetRandomNumber(FindMagic.MA1Min, FindMagic.MA1Max);

                                int DIV = RandomValue - FindMagic.MA1Min;

                                _Magic.Value_1 = DIV;
                            }
                            else
                            {
                                _Magic.Value_1 = -1;
                            }

                            if (FindMagic.MA2Min > 0)
                            {
                                int RandomValue = KTGlobal.GetRandomNumber(FindMagic.MA2Min, FindMagic.MA2Max);

                                int DIV = RandomValue - FindMagic.MA2Min;

                                _Magic.Value_2 = DIV;
                            }
                            else
                            {
                                _Magic.Value_2 = -1;
                            }

                            if (FindMagic.MA3Min > 0)
                            {
                                int RandomValue = KTGlobal.GetRandomNumber(FindMagic.MA3Min, FindMagic.MA3Max);

                                int DIV = RandomValue - FindMagic.MA3Min;

                                _Magic.Value_3 = DIV;
                            }
                            else
                            {
                                _Magic.Value_3 = -1;
                            }
                        }
                        else // Nếu không có set MAGIC LEVEL  -1
                        {
                            _Magic.nAttribType = -200;
                            _Magic.nLevel = -1;
                            _Magic.Value_1 = -1;
                            _Magic.Value_2 = -1;
                            _Magic.Value_3 = -1;
                        }
                    }
                    else
                    {
                        _Magic.nAttribType = -100;
                        _Magic.nLevel = -1;
                        _Magic.Value_1 = -1;
                        _Magic.Value_2 = -1;
                        _Magic.Value_3 = -1;
                    }

                    _ItemBuild.GreenProp.Add(_Magic);
                }
            }
            else
            {
                _ItemBuild.GreenPropCount = 0;
                _ItemBuild.GreenProp = new List<KMagicInfo>();
            }

            if (HiddenProbs.Count > 0)
            {
                _ItemBuild.HiddenProbsCount = HiddenProbs.Count;

                _ItemBuild.HiddenProbs = new List<KMagicInfo>();

                foreach (MagicAttribLevel _Probs in HiddenProbs)
                {
                    KMagicInfo _Magic = new KMagicInfo();

                    if (_Probs.MagicName.Length > 0)
                    {
                        if (PropertyDefine.PropertiesBySymbolName.ContainsKey(_Probs.MagicName))
                        {
                            MagicAttribLevel FindMagic = TotalMagicAttribLevel.Where(x => x.MagicName == _Probs.MagicName && x.Level == _Probs.Level).FirstOrDefault();

                            if (FindMagic != null)
                            {
                                int MagicID = PropertyDefine.PropertiesBySymbolName[_Probs.MagicName].ID;

                                _Magic.nAttribType = MagicID;
                                _Magic.nLevel = _Probs.Level;

                                if (FindMagic.MA1Min > 0)
                                {
                                    int RandomValue = KTGlobal.GetRandomNumber(FindMagic.MA1Min, FindMagic.MA1Max);

                                    int DIV = RandomValue - FindMagic.MA1Min;

                                    _Magic.Value_1 = DIV;
                                }
                                else
                                {
                                    _Magic.Value_1 = -1;
                                }

                                if (FindMagic.MA2Min > 0)
                                {
                                    int RandomValue = KTGlobal.GetRandomNumber(FindMagic.MA2Min, FindMagic.MA2Max);

                                    int DIV = RandomValue - FindMagic.MA2Min;

                                    _Magic.Value_2 = DIV;
                                }
                                else
                                {
                                    _Magic.Value_2 = -1;
                                }

                                if (FindMagic.MA3Min > 0)
                                {
                                    int RandomValue = KTGlobal.GetRandomNumber(FindMagic.MA3Min, FindMagic.MA3Max);

                                    int DIV = RandomValue - FindMagic.MA3Min;

                                    _Magic.Value_3 = DIV;
                                }
                                else
                                {
                                    _Magic.Value_3 = -1;
                                }
                            }
                            else // Nếu không có set MAGIC LEVEL  -1
                            {
                                _Magic.nAttribType = -200;
                                _Magic.nLevel = -1;
                                _Magic.Value_1 = -1;
                                _Magic.Value_2 = -1;
                                _Magic.Value_3 = -1;
                            }
                        }
                        else
                        {
                            _Magic.nAttribType = -100;
                            _Magic.nLevel = -1;
                            _Magic.Value_1 = -1;
                            _Magic.Value_2 = -1;
                            _Magic.Value_3 = -1;
                        }
                    }
                    else
                    {
                        _Magic.nAttribType = -2;
                        _Magic.nLevel = -1;
                        _Magic.Value_1 = -1;
                        _Magic.Value_2 = -1;
                        _Magic.Value_3 = -1;
                    }

                    _ItemBuild.HiddenProbs.Add(_Magic);
                }
            }
            else
            {
                _ItemBuild.HiddenProbsCount = 0;
                _ItemBuild.HiddenProbs = new List<KMagicInfo>();
            }

            byte[] ItemDataByteArray = DataHelper.ObjectToBytes(_ItemBuild);
            if (ItemDataByteArray.Length == 0)
            {
                return "ERORR";
            }
            else
            {
                return Convert.ToBase64String(ItemDataByteArray);
            }
        }

        public static string ItemUpdateScriptBuild(Dictionary<UPDATEITEM, object> Input)
        {
            string OutPut = "";

            for (int i = 0; i <= 14; i++)
            {
                UPDATEITEM _Item = (UPDATEITEM)i;

                if (Input.ContainsKey(_Item))
                {
                    if (Input[_Item].GetType() == typeof(string))
                    {
                        string Value = (string)Input[_Item];

                        OutPut += Value + ":";
                    }
                    else if (Input[_Item].GetType() == typeof(int))
                    {
                        int Value = (int)Input[_Item];

                        OutPut += Value + ":";
                    }
                    else if (Input[_Item].GetType() == typeof(Dictionary<ItemPramenter, string>))
                    {
                        Dictionary<ItemPramenter, string> Value = (Dictionary<ItemPramenter, string>)Input[_Item];

                        byte[] ItemDataByteArray = DataHelper.ObjectToBytes(Value);

                        string otherPramer = Convert.ToBase64String(ItemDataByteArray);

                        OutPut += otherPramer + ":";
                    }
                }
                else
                {
                    OutPut += "*:";
                }
            }

            return OutPut;
        }

        /// <summary>
        /// Cập nhật số lượng vật phẩm tương ứng
        /// Chú ý trước hàm này không được là 1 vòng FOREACH hoặc vòng for với LOCK ITEM ở đằng trước
        /// Nếu không vào trong này sẽ bọ LOCK 1 lần nữa dẫn tưới BUG DUPPER ITEM
        /// </summary>
        /// <param name="data"></param>
        /// <param name="client"></param>
        /// <param name="Count"></param>
        /// <returns></returns>
        public static bool UpdateItemCount(GoodsData data, KPlayer client, int Count, string From = "")
        {
            //Console.WriteLine("GoodsID :" + data.GoodsID + "ITEMDBID :" + data.Id + "| Count" + Count);

            TCPOutPacketPool pool = Global._TCPManager.TcpOutPacketPool;
            TCPClientPool tcpClientPool = Global._TCPManager.tcpClientPool;

            if (!_TotalGameItem.ContainsKey(data.GoodsID))
            {
                LogManager.WriteLog(LogTypes.Item, "[" + client.strUserID + "][" + client.RoleID + "][" + client.RoleName + "]Set Số lượng thất bại vật phẩm không tồn tại");

                return false;
            }

            if (data.Id == -1)
            {
                LogManager.WriteLog(LogTypes.Item, "[" + client.strUserID + "][" + client.RoleID + "][" + client.RoleName + "]Set số lượng thất bại vật phẩm không hợp lệ");

                return false;
            }

            if (data.GCount < 0)
            {
                LogManager.WriteLog(LogTypes.Item, "[" + client.strUserID + "][" + client.RoleID + "][" + client.RoleName + "]Set số lượng thất bại số lượng còn lại không hợp lệ ");

                return false;
            }

            if (Count < 0)
            {
                LogManager.WriteLog(LogTypes.Item, "[" + client.strUserID + "][" + client.RoleID + "][" + client.RoleName + "]Set số lượng thất bại số lượng còn lại không hợp lệ ");

                return false;
            }

            // Xây dựng DICT update
            Dictionary<UPDATEITEM, object> TotalUpdate = new Dictionary<UPDATEITEM, object>();

            TotalUpdate.Add(UPDATEITEM.ROLEID, client.RoleID);
            TotalUpdate.Add(UPDATEITEM.ITEMDBID, data.Id);
            TotalUpdate.Add(UPDATEITEM.GCOUNT, Count);

            string ScriptUpdateBuild = ItemManager.ItemUpdateScriptBuild(TotalUpdate);

            string[] dbFields = null;

            TCPProcessCmdResults dbRequestResult = Global.RequestToDBServer(tcpClientPool, pool, (int)TCPGameServerCmds.CMD_DB_UPDATEGOODS_CMD, ScriptUpdateBuild, out dbFields, client.ServerId);

            if (dbRequestResult == TCPProcessCmdResults.RESULT_FAILED)
            {
                LogManager.WriteLog(LogTypes.Item, "[" + client.strUserID + "][" + client.RoleID + "][" + client.RoleName + "]Set số lượng vật phẩm thất bại :" + data.Id + " Số lượng không hợp lệ");

                return false;
            }
            else if (dbRequestResult == TCPProcessCmdResults.RESULT_DATA)
            {
                /// Thực hiện update lại vật phẩm trong túi đồ
                client.GoodsData.LocalModify(data.Id, TotalUpdate);

                /// Gửi lại dữ liệu cho client thay đổi thành công
                SCModGoods scData = new SCModGoods()
                {
                    BagIndex = data.BagIndex,
                    Count = data.GCount,
                    IsUsing = data.Using,
                    ModType = (int)ModGoodsTypes.ModValue,
                    ID = data.Id,
                    NewHint = 0,
                    Site = data.Site,
                    State = 0,
                };
                client.SendPacket((int)TCPGameServerCmds.CMD_SPR_MOD_GOODS, scData);
                if (Count == 0)
                {
                    LogManager.WriteLog(LogTypes.Item, "[" + client.strUserID + "][" + client.RoleID + "][" + client.RoleName + "][" + data.GoodsID + "][" + data.GCount + "][" + data.ItemName + "][" + From + "] Xóa vật phẩm thành công : " + data.Id);
                }
                else
                {
                    LogManager.WriteLog(LogTypes.Item, "[" + client.strUserID + "][" + client.RoleID + "][" + client.RoleName + "][" + data.GoodsID + "][" + data.ItemName + "][" + From + "] Set số lượng vật phẩm về [" + Count + "] thành công : " + data.Id);
                }

                // Thực hiện call task sử dụng vật phẩm
                ProcessTask.Process(Global._TCPManager.MySocketListener, Global._TCPManager.TcpOutPacketPool, client, -1, -1, data.GoodsID, TaskTypes.UseSomething);

                return true;
            }

            return false;
        }

        public static bool IsMaterial(ItemData _Item)
        {
            if (_Item.Genre == 1 && _Item.DetailType == 17)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Xóa bỏ vật phẩm đã hết hạn
        /// </summary>
        /// <param name="client"></param>
        /// <param name="goodsData"></param>
        /// <returns></returns>
        public static bool DestroyGoods(KPlayer client, GoodsData goodsData, string FROM = "")
        {
            String cmdData = "";

            if (goodsData.Using > 0)
            {
                // Nếu vật phẩm đang mặc
                // thì thào đồ ra khỏi người sau đó xóa
                client.GetPlayEquipBody().EquipUnloadMain(goodsData);

                ItemManager.AbandonItem(goodsData, client, false, FROM);
            }
            else
            {
                ItemManager.AbandonItem(goodsData, client, false, FROM);
            }

            return true;
        }

        /// <summary>
        /// Hàm sửa đổi thuộc tính trang bị
        /// </summary>
        /// <param name="sl"></param>
        /// <param name="pool"></param>
        /// <param name="client"></param>
        /// <param name="gd"></param>
        public static void NotifyGoodsInfo(KPlayer client, GoodsData gd)
        {
            client.SendPacket<GoodsData>((int)TCPGameServerCmds.CMD_SPR_NOTIFYGOODSINFO, gd);
        }

        /// <summary>
        /// Sửa chữa thuộc tính của 1 item DATA
        /// </summary>
        /// <param name="data"></param>
        /// <param name="client"></param>
        /// <param name="Count"></param>
        /// <returns></returns>
        public static bool ItemModValue(GoodsData data, KPlayer client, int NewSite)
        {
            TCPOutPacketPool pool = Global._TCPManager.TcpOutPacketPool;
            TCPClientPool tcpClientPool = Global._TCPManager.tcpClientPool;

            if (!_TotalGameItem.ContainsKey(data.GoodsID))
            {
                LogManager.WriteLog(LogTypes.Item, "[" + client.strUserID + "][" + client.RoleID + "][" + client.RoleName + "]ModValue thất bại vật phẩm không tồn tại");

                return false;
            }

            if (data.Id == -1)
            {
                LogManager.WriteLog(LogTypes.Item, "[" + client.strUserID + "][" + client.RoleID + "][" + client.RoleName + "]Modvalue thất bại vật phẩm không hợp lệ");

                return false;
            }

            if (data.GCount < 0)
            {
                LogManager.WriteLog(LogTypes.Item, "[" + client.strUserID + "][" + client.RoleID + "][" + client.RoleName + "]Modvalue thất bại số lượng còn lại không hợp lệ ");

                return false;
            }

            // Xây dựng DICT update
            Dictionary<UPDATEITEM, object> TotalUpdate = new Dictionary<UPDATEITEM, object>();

            TotalUpdate.Add(UPDATEITEM.ROLEID, client.RoleID);
            TotalUpdate.Add(UPDATEITEM.ITEMDBID, data.Id);

            /// Nếu mà Site == 1 tức là lấy bỏ đồ từ túi sang Thủ khố
            if (NewSite == 1)
            {
                int SlotIndex = client.GoodsData.GetBagFirstEmptyPosition(1);

                if (SlotIndex != -1)
                {
                    TotalUpdate.Add(UPDATEITEM.SITE, NewSite);
                    TotalUpdate.Add(UPDATEITEM.USING, -1);
                    TotalUpdate.Add(UPDATEITEM.BAGINDEX, SlotIndex);
                }
                else
                {
                    KTPlayerManager.ShowMessageBox(client, "Thông báo", "Kho đã đầy không thể cất đi");
                    return false;
                }
            }
            /// Nếu mà Site == 0 tức là lấy từ kho ra
            else if (NewSite == 0)
            {
                int SlotIndex = client.GoodsData.GetBagFirstEmptyPosition(0);

                if (SlotIndex != -1)
                {
                    TotalUpdate.Add(UPDATEITEM.USING, -1);
                    TotalUpdate.Add(UPDATEITEM.SITE, NewSite);
                    TotalUpdate.Add(UPDATEITEM.BAGINDEX, SlotIndex);
                }
                else
                {
                    KTPlayerManager.ShowMessageBox(client, "Thông báo", "Túi đồ đã đầy không thể lấy ra");
                    return false;
                }
            }
            else
            {
                return false;
            }

            string ScriptUpdateBuild = ItemManager.ItemUpdateScriptBuild(TotalUpdate);

            string[] dbFields = null;

            // Thực hiện update vào DB thông tin của đồ
            TCPProcessCmdResults dbRequestResult = Global.RequestToDBServer(tcpClientPool, pool, (int)TCPGameServerCmds.CMD_DB_UPDATEGOODS_CMD, ScriptUpdateBuild, out dbFields, client.ServerId);

            if (dbRequestResult == TCPProcessCmdResults.RESULT_FAILED)
            {
                LogManager.WriteLog(LogTypes.Item, "[" + client.strUserID + "][" + client.RoleID + "][" + client.RoleName + "] Có lỗi xảy ra khi thao tác với DB SV:" + data.Id);

                return false;
            }
            else if (dbRequestResult == TCPProcessCmdResults.RESULT_DATA)
            {
                // Thực hiện update Item Pram cho đồ này
                client.GoodsData.LocalModify(data.Id, TotalUpdate);
                // Tức là lấy đồ từ túi sang thủ khố
                if (NewSite == 1)
                {
                    //Xóa trên nhân vật

                    //string strcmd = string.Format("{0}:{1}", client.RoleID, data.Id);

                    //// SEND VỀ CLLIENT XÓA VẬT PHẨM TRONG TÚI ĐỒ
                    //client.SendPacket((int)TCPGameServerCmds.CMD_SPR_MOVEGOODSDATA, strcmd);

                    //// XÓA VẬT PHẨM TRONG TÚI ĐỒ Ở GS
                    //Global.RemoveGoodsData(client, data);

                    //// Gán lại vị trí mới cho vật phẩm ở thương khố
                    //data.Site = NewSite;
                    //data.BagIndex = NewBagIndex;

                    GoodsData _Data = client.GoodsData.Find(data.Id);

                    if (_Data != null)
                    {
                        ItemManager.NotifyGoodsInfo(client, _Data);
                    }

                    // ADD VẬT PHẨM VÀO TÚI ĐỒ
                    //  Global.AddPortableGoodsData(client, data);

                    // SEND PACKET ADD VẬT PHẨM VÀO THƯƠNG KHỐ THEO SITE MỚI
                    KT_TCPHandler.NotifySelfAddGoods(client, data.Id, data.GoodsID, data.Forge_level, data.GCount, data.Binding, NewSite, 0, data.Endtime, data.Strong, data.BagIndex, -1, data.Props, data.Series, data.OtherParams);
                }
                // Nếu lấy từ thủ khố sang bên này
                else if (NewSite == 0)
                {
                    GoodsData _Data = client.GoodsData.Find(data.Id);

                    if (_Data != null)
                    {
                        ItemManager.NotifyGoodsInfo(client, _Data);
                    }

                    // SEND PACKET ADD VẬT PHẨM VÀO TÚI ĐỒ THEO SITE VÀ BAGINDEX MỚI
                    KT_TCPHandler.NotifySelfAddGoods(client, data.Id, data.GoodsID, data.Forge_level, data.GCount, data.Binding, NewSite, 0, data.Endtime, data.Strong, data.BagIndex, -1, data.Props, data.Series, data.OtherParams);
                }

                LogManager.WriteLog(LogTypes.Item, "[" + client.strUserID + "][" + client.RoleID + "][" + client.RoleName + "] Thay đổi vị trí đồ thành công :" + data.Id);

                return true;
            }

            return false;
        }

        /// <summary>
        /// Hàm tách 1 vật phẩm làm 2 vật phẩm
        /// </summary>
        /// <param name="data"></param>
        /// <param name="client"></param>
        /// <param name="Count"></param>
        /// <returns></returns>
        public static bool SplitItem(GoodsData data, KPlayer client, int Count)
        {
            TCPOutPacketPool pool = Global._TCPManager.TcpOutPacketPool;
            TCPClientPool tcpClientPool = Global._TCPManager.tcpClientPool;

            if (!_TotalGameItem.ContainsKey(data.GoodsID))
            {
                LogManager.WriteLog(LogTypes.Item, "[" + client.strUserID + "][" + client.RoleID + "][" + client.RoleName + "]Tách thất bại vật phẩm không tồn tại");

                return false;
            }

            if (data.Id == -1)
            {
                LogManager.WriteLog(LogTypes.Item, "[" + client.strUserID + "][" + client.RoleID + "][" + client.RoleName + "]Tách thất bại vật phẩm không hợp lệ");

                return false;
            }

            if (data.GCount < 0)
            {
                LogManager.WriteLog(LogTypes.Item, "[" + client.strUserID + "][" + client.RoleID + "][" + client.RoleName + "]Tách thất bại số lượng còn lại không hợp lệ ");

                return false;
            }

            if (data.GCount <= Count)
            {
                LogManager.WriteLog(LogTypes.Item, "[" + client.strUserID + "][" + client.RoleID + "][" + client.RoleName + "]Số lượng muốn tách không hợp lệ ");

                return false;
            }

            int ItemLess = data.GCount - Count;

            if (ItemManager.UpdateItemCount(data, client, ItemLess, "SPLITITEM"))
            {
                if (!ItemManager.CreateItem(Global._TCPManager.TcpOutPacketPool, client, data.GoodsID, Count, 0, "SPLITITEM", false, data.Binding, false, ItemManager.ConstGoodsEndTime, data.Props, data.Series, data.Creator, data.Forge_level, 0, false))
                {
                    KTPlayerManager.ShowNotification(client, "Có lỗi khi nhận vật phẩm chế tạo");
                }
            }

            return false;
        }

        /// <summary>
        /// Xóa vật phẩm khỏi túi đồ
        /// </summary>
        /// <param name="data"></param>
        /// <param name="client"></param>
        /// <returns></returns>
        public static bool AbadonItem(int ItemDBID, KPlayer client, bool DropToMap, string From = "")
        {
            GoodsData FindGoldById = client.GoodsData.Find(ItemDBID);
            if (FindGoldById != null)
            {
                return AbandonItem(FindGoldById, client, DropToMap, From);
            }
            else
            {
                return false;
            }
        }

        public static bool AbandonItem(GoodsData data, KPlayer client, bool DropToMap, string From = "")
        {
            GoodsData _Template = new GoodsData();

            TCPOutPacketPool pool = Global._TCPManager.TcpOutPacketPool;
            TCPClientPool tcpClientPool = Global._TCPManager.tcpClientPool;

            if (!_TotalGameItem.ContainsKey(data.GoodsID))
            {
                LogManager.WriteLog(LogTypes.Item, "[" + client.strUserID + "][" + client.RoleID + "][" + client.RoleName + "][" + From + "][" + data.GoodsID + "]Hủy vật phẩm thất bại vật phẩm không tồn tại");

                return false;
            }

            if (data.Id == -1)
            {
                LogManager.WriteLog(LogTypes.Item, "[" + client.strUserID + "][" + client.RoleID + "][" + client.RoleName + "][" + From + "][" + data.GoodsID + "]Hủy thất bại vật phẩm không hợp lệ");

                return false;
            }

            if (data.GCount <= 0)
            {
                LogManager.WriteLog(LogTypes.Item, "[" + client.strUserID + "][" + client.RoleID + "][" + client.RoleName + "][" + From + "][" + data.GoodsID + "]Hủy thất bại số lượng còn lại không hợp lệ ");

                return false;
            }

            var FindItem = client.GoodsData.Find(data.Id);
            if (FindItem == null)
            {
                LogManager.WriteLog(LogTypes.Item, "[" + client.strUserID + "][" + client.RoleID + "][" + client.RoleName + "][" + From + "][" + data.GoodsID + "]Hủy vật phẩm thất bại do vật phẩm không phải của chủ nhân");

                return false;
            }

            // Xây dựng DICT update
            Dictionary<UPDATEITEM, object> TotalUpdate = new Dictionary<UPDATEITEM, object>();

            TotalUpdate.Add(UPDATEITEM.ROLEID, client.RoleID);
            TotalUpdate.Add(UPDATEITEM.ITEMDBID, data.Id);

            TotalUpdate.Add(UPDATEITEM.USING, -1);

            TotalUpdate.Add(UPDATEITEM.BAGINDEX, -1);
            // Set số lượng ==0 tức là xóa
            TotalUpdate.Add(UPDATEITEM.GCOUNT, 0);

            // Nếu mà vứt ra map thì đánh dấu site = số lượng để xử lý trong gamedb
            if (DropToMap)
            {
                TotalUpdate.Add(UPDATEITEM.SITE, (data.GCount * -1));
            }

            string ScriptUpdateBuild = ItemManager.ItemUpdateScriptBuild(TotalUpdate);

            string[] dbFields = null;

            TCPProcessCmdResults dbRequestResult = Global.RequestToDBServer(tcpClientPool, pool, (int)TCPGameServerCmds.CMD_DB_UPDATEGOODS_CMD, ScriptUpdateBuild, out dbFields, client.ServerId);

            if (dbRequestResult == TCPProcessCmdResults.RESULT_FAILED)
            {
                LogManager.WriteLog(LogTypes.Item, "[" + client.strUserID + "][" + client.RoleID + "][" + client.RoleName + "][" + From + "]Xóa vật phẩm thất bại :" + data.Id + " Số lượng không hợp lệ");

                return false;
            }
            else if (dbRequestResult == TCPProcessCmdResults.RESULT_DATA)
            {
                if (DropToMap)
                {
                    _Template.BagIndex = -1;
                    _Template.GoodsID = data.GoodsID;
                    _Template.GCount = data.GCount;
                    _Template.Forge_level = data.Forge_level;
                    _Template.OtherParams = data.OtherParams;
                    _Template.Props = data.Props;
                    _Template.Series = data.Series;
                    _Template.Site = 0;
                    _Template.Endtime = data.Endtime;
                    _Template.Strong = data.Strong;
                    _Template.Using = -1;
                    _Template.Id = data.Id;
                    _Template.Binding = data.Binding;
                    _Template.Endtime = data.Endtime;

                    // Thả ra map ngay cho thằng khác có thể nhặt
                    KTGoodsPackManager.CreateDropMapFromSingleGoods(client, _Template);
                }

                // Thực hiện update lại vật phẩm trong túi đồ
                client.GoodsData.LocalModify(data.Id, TotalUpdate);

                SCModGoods scData = new SCModGoods()
                {
                    BagIndex = data.BagIndex,
                    Count = -1,
                    IsUsing = data.Using,
                    ModType = (int)ModGoodsTypes.Abandon,
                    ID = data.Id,
                    NewHint = 0,
                    Site = data.Site,
                    State = 0,
                };
                client.SendPacket((int)TCPGameServerCmds.CMD_SPR_MOD_GOODS, scData);

                LogManager.WriteLog(LogTypes.Item, "[" + client.strUserID + "][" + client.RoleID + "][" + client.RoleName + "][" + From + "]Xóa vật phẩm thành công :" + data.GoodsID + " | COUNT : " + data.GCount + " |" + data.Id + "| " + data.ItemName + "| Forge :" + data.Forge_level);

                return true;
            }

            return false;
        }

        public static string GetNameItem(GoodsData data)
        {
            if (!_TotalGameItem.ContainsKey(data.GoodsID))
            {
                return "";
            }
            else
            {
                ItemData _Item = _TotalGameItem[data.GoodsID];
                return _Item.Name;
            }
        }

        /// <summary>
        /// Set cấp cường hóa cho trang bị
        /// </summary>
        /// <param name="data"></param>
        /// <param name="client"></param>
        /// <param name="forgeLevel"></param>
        /// <returns></returns>
        public static bool SetEquipForgeLevel(GoodsData data, KPlayer client, int forgeLevel, int Lock = 0)
        {
            TCPOutPacketPool pool = Global._TCPManager.TcpOutPacketPool;
            TCPClientPool tcpClientPool = Global._TCPManager.tcpClientPool;

            if (!_TotalGameItem.ContainsKey(data.GoodsID))
            {
                LogManager.WriteLog(LogTypes.Item, "[" + client.strUserID + "][" + client.RoleID + "][" + client.RoleName + "][" + data.GoodsID + "]Nâng cấp cường hóa thất bại :" + forgeLevel + " vật phẩm không tồn tại");

                return false;
            }

            ItemData _Item = _TotalGameItem[data.GoodsID];

            if (!KD_ISEQUIP(_Item.Genre) && !KD_ISPETEQUIP(_Item.Genre))
            {
                LogManager.WriteLog(LogTypes.Item, "[" + client.strUserID + "][" + client.RoleID + "][" + client.RoleName + "][" + data.GoodsID + "]Cường hóa thất bại :" + forgeLevel + " vật phẩm không phải là trang bị không thể nâng cấp cường hóa");

                return false;
            }

            if (data.Id == -1)
            {
                LogManager.WriteLog(LogTypes.Item, "[" + client.strUserID + "][" + client.RoleID + "][" + client.RoleName + "][" + data.GoodsID + "]Cường hóa thất bại :" + forgeLevel + " Vật phẩm không hợp lệ");

                return false;
            }

            // Xây dựng DICT update
            Dictionary<UPDATEITEM, object> TotalUpdate = new Dictionary<UPDATEITEM, object>();

            TotalUpdate.Add(UPDATEITEM.ROLEID, client.RoleID);
            TotalUpdate.Add(UPDATEITEM.ITEMDBID, data.Id);
            TotalUpdate.Add(UPDATEITEM.BINDING, Lock);
            TotalUpdate.Add(UPDATEITEM.FORGE_LEVEL, forgeLevel);

            string ScriptUpdateBuild = ItemManager.ItemUpdateScriptBuild(TotalUpdate);

            string[] dbFields = null;

            TCPProcessCmdResults dbRequestResult = Global.RequestToDBServer(tcpClientPool, pool, (int)TCPGameServerCmds.CMD_DB_UPDATEGOODS_CMD, ScriptUpdateBuild, out dbFields, client.ServerId);

            if (dbRequestResult == TCPProcessCmdResults.RESULT_FAILED)
            {
                LogManager.WriteLog(LogTypes.Item, "Nâng cấp cường hóa thất bại");

                string strcmd = string.Format("{0}:{1}:{2}:{3}:{4}", -1, client.RoleID, data.Id, data.Forge_level, data.Binding);
                client.SendPacket((int)TCPGameServerCmds.CMD_SPR_FORGE, strcmd);

                return false;
            }
            else if (dbRequestResult == TCPProcessCmdResults.RESULT_DATA)
            {
                // Thực hiện update lại vật phẩm trong túi đồ
                client.GoodsData.LocalModify(data.Id, TotalUpdate);

                string strcmd = string.Format("{0}:{1}:{2}:{3}:{4}", 1, client.RoleID, data.Id, data.Forge_level, data.Binding);
                client.SendPacket((int)TCPGameServerCmds.CMD_SPR_FORGE, strcmd);

                LogManager.WriteLog(LogTypes.Item, "Nâng cấp thành công!");

                return true;
            }

            return true;
        }

        public static bool IsCrytalDropArlet(int GoodPackID, int MinLevelPick)
        {
            int ItemLevel = 0;

            if (GoodPackID >= 183 && GoodPackID <= 194)
            {
                ItemLevel = GoodPackID - 182;

                if (ItemLevel >= MinLevelPick)
                {
                    return true;
                }
            }
            else if (GoodPackID >= 385 && GoodPackID <= 396)
            {
                ItemLevel = GoodPackID - 384;

                if (ItemLevel >= MinLevelPick)
                {
                    return true;
                }
            }
            else
            {
                return false;
            }

            return false;
        }

        /// <summary>
        /// Random ngũ hành của trang bị
        /// </summary>
        /// <param name="InputSeri"></param>
        /// <returns></returns>
        public static int GetItemSeries(int InputSeri)
        {
            int Series = InputSeri;

            if (Series == -1)
            {
                Series = KTGlobal.GetRandomNumber(1, 5);
            }

            return Series;
        }

        public static bool IsExits(GoodsData Input)
        {
            if (Input == null || !_TotalGameItem.ContainsKey(Input.GoodsID))
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public static int TotalSpaceNeed(int ItemID, int Count)
        {
            if (!_TotalGameItem.ContainsKey(ItemID))
            {
                return -1;
            }

            ItemData _Item = _TotalGameItem[ItemID];

            if (Count > _Item.Stack)
            {
                return Count / _Item.Stack;
            }
            else
            {
                return 1;
            }
        }

        /// <summary>
        /// Tạo vật phẩm từ ID tương ứng
        /// </summary>
        /// <param name="itemID"></param>
        /// <returns></returns>
        public static GoodsData CreateGoodsFromItemID(int itemID)
        {
            /// Nếu không tồn tại vật phẩm
            if (!ItemManager._TotalGameItem.TryGetValue(itemID, out ItemData itemData))
            {
                return null;
            }

            GoodsData itemGD = new GoodsData()
            {
                GoodsID = itemID,
                Props = ItemManager.KD_ISEQUIP(itemData.Genre) ? ItemManager.GenerateProbs(itemData) : "",
                Series = ItemManager.KD_ISEQUIP(itemData.Genre) ? itemData.Series >= (int)KE_SERIES_TYPE.series_metal && itemData.Series <= (int)KE_SERIES_TYPE.series_earth ? itemData.Series : KTGlobal.GetRandomNumber((int)KE_SERIES_TYPE.series_metal, (int)KE_SERIES_TYPE.series_earth) : 0,
            };
            return itemGD;
        }

        /// <summary>
        /// Tạo trang bị random thuộc tính tương ứng
        /// </summary>
        /// <param name="equipID"></param>
        /// <param name="magicRate"></param>
        /// <param name="minMagicLevel"></param>
        /// <param name="maxMagicLevel"></param>
        /// <returns></returns>
        public static GoodsData CreateRandomAttributesEquip(int equipID, int magicRate, int minMagicLevel, int maxMagicLevel)
        {
            /// Thông tin
            ItemData itemData = ItemManager.GetItemTemplate(equipID);
            /// Toác
            if (itemData == null)
            {
                /// Không có kết quả
                return null;
            }
            /// Nếu không phải trang bị
            if (!ItemManager.KD_ISEQUIP(itemData.Genre) && !ItemManager.KD_ISPETEQUIP(itemData.Genre))
            {
                /// Không có kết quả
                return null;
            }

            /// Số điểm may mắn bị giảm mỗi lần random được dòng nào đó
            int nRandPointEach = magicRate / 6;
            /// Tỷ lệ còn lại
            int nRate = magicRate;

            /// Làm 1 mảng lưu lại level của 6 dòng sẽ random được nếu mà ko random được thì set nó = 0
            int[] pnMagicLevel = new int[6] { 0, 0, 0, 0, 0, 0 };
            /// Duyệt số dòng
            for (int k = 0; k < 6; k++)
            {
                /// Tỷ lệ ngẫu nhiên có dòng này
                if (KTGlobal.GetRandomNumber(0, 100) < nRate)
                {
                    /// Cấp độ dòng
                    int nLevel = KTGlobal.GetRandomNumber(minMagicLevel, maxMagicLevel);
                    /// Lưu lại vào mảng
                    pnMagicLevel[k] = nLevel;
                    /// Giảm tỷ lệ còn lại xuống
                    nRate -= nRandPointEach;
                }
                else
                {
                    break;
                }
            }

            /// Ngũ hành tương ứng
            int nSeries = ItemManager.GetItemSeries(itemData.Series);
            /// Danh sách 6 thuộc tính
            MagicAttribLevel[] pMagicAttrTable = { null, null, null, null, null, null };

            /// Đánh dấu các thuộc tính đã dùng
            List<MagicAttribLevel> mark = new List<MagicAttribLevel>();

            /// Duyệt số dòng
            for (int g = 0; g < 6; g++)
            {
                /// Danh sách thuộc tính thỏa mãn
                List<MagicAttribLevel> attributes = ItemManager.TotalMagicAttribLevel.Where(x => x.RATE(itemData.DetailType, itemData.Genre) > 0 && (x.Series == nSeries || x.Series == -1) && x.Level == pnMagicLevel[g]).ToList();
                /// Không tìm thấy
                if (attributes.Count == 0)
                {
                    /// Thoát
                    break;
                }

                /// Ghi log
                LogManager.WriteLog(LogTypes.Item, string.Format("Generate drop item ID {0} ({1})", itemData.ItemID, itemData.Name));

                /// Tổng tỷ lệ
                long totalRate = attributes.Sum(x => x.RATE(itemData.DetailType, itemData.Genre));
                /// Giá trị ngẫu nhiên
                long randValue = KTGlobal.GetRandomNumber(0, totalRate);

                /// Duyệt danh sách thuộc tính
                foreach (MagicAttribLevel attribute in attributes)
                {
                    /// Ghi Log
                    LogManager.WriteLog(LogTypes.Item, string.Format("MagicName: {0}, Rate: {1}", attribute.MagicName, attribute.RATE(itemData.DetailType, itemData.Genre)));
                }

                /// Tổng số giá trị được chọn
                long selectValue = 0;

                /// Duyệt danh sách thuộc tính
                foreach (MagicAttribLevel attribute in attributes)
                {
                    /// Toác
                    if (attribute == null)
                    {
                        /// Bỏ qua
                        continue;
                    }
                    /// Nếu đã chọn dòng này rồi
                    else if (mark.Any(x => x.MAGIC_ID == attribute.MAGIC_ID))
                    {
                        /// Bỏ qua
                        continue;
                    }

                    /// Tăng tổng số giá trị đã chọn
                    selectValue += attribute.RATE(itemData.DetailType, itemData.Genre);

                    /// Nếu có thể lấy được dòng này
                    if (selectValue >= randValue)
                    {
                        /// Thêm vào danh sách đã chọn
                        mark.Add(attribute);
                        /// Lưu lại thông tin dòng này
                        pMagicAttrTable[g] = attribute;
                        /// Thoát vì đã chọn được dòng tương ứng rồi
                        break;
                    }
                }
            }

            /// Chuỗi thuộc tính tương ứng
            string propString = ItemManager.GenerateProbsWithRandomLine(itemData, pMagicAttrTable);
            /// Kiểm tra lần nữa
            if (itemData.Genre == 3)
            {
                propString = "";
                nSeries = -1;
            }

            /// Tạo vật phẩm tương ứng
            GoodsData itemGD = new GoodsData()
            {
                Id = -1,
                GoodsID = itemData.ItemID,
                GCount = 1,
                Props = propString,
                Series = nSeries,
                Site = 0,
                Strong = 100,
                Using = -1,
                BagIndex = -1,
                Binding = 0,
                Forge_level = 0,
            };
            /// Trả về kết quả
            return itemGD;
        }

        private static int GetItemExpiesTime(int ItemID)
        {
            const int DEF_7DAY_TIME = 60 * 24 * 7;

            const int DEF_30DAY_TIME = 60 * 24 * 30;
            switch (ItemID)
            {
                case 496:
                    return DEF_7DAY_TIME;

                case 555:
                    return DEF_30DAY_TIME;

                case 557:
                    return DEF_30DAY_TIME;
            }

            return -1;
        }

        /// <summary>
        /// Call hàm này để tạo ITEM Cho 1 GameClient Bất Kỳ
        /// From : là vật phẩm tới từ đâu | Sự kiện nào  | Note cái này theo quy tắc để sau này check logs cho tiện
        /// EndTime : Hạn sử dụng nếu vật phẩm có hạn sử dụng
        /// </summary>
        /// <param name="pool"></param>
        /// <param name="client"></param>
        /// <param name="ItemID"></param>
        /// <param name="ItemNum"></param>
        /// <returns></returns>
        public static bool CreateItem(TCPOutPacketPool pool, KPlayer client, int ItemID, int ItemNum, int Site, string From, bool useOldGrid, int IsBinding, bool bIsFromMap, string EndTime, string InputProb = "", int InputSeri = -1, string Author = "", int enhanceLevel = 0, int HitItem = 1, bool IsNeedWriterLogs = false)
        {
            /// Kiểm tra xem vật phẩm có tồn tại hay không
            if (!_TotalGameItem.ContainsKey(ItemID))
            {
                LogManager.WriteLog(LogTypes.Item, "[" + client.strUserID + "][" + client.RoleID + "][" + client.RoleName + "][" + From + "] Create Item Error : " + ItemID + " => Item Don't Exits");

                return false;
            }
            if (ItemNum <= 0)
            {
                LogManager.WriteLog(LogTypes.Item, "[" + client.strUserID + "][" + client.RoleID + "][" + client.RoleName + "][" + From + "] Create Item Error : " + ItemID + " => Number Item Not Support :" + ItemNum);

                return false;
            }

            int nBakGoodsNum = ItemNum;

            ItemData ItemData = _TotalGameItem[ItemID];

            string TmpEnd = "";
            string TmpStart = "";

            TmpEnd = EndTime;

            if (TmpEnd == "")
            {
                TmpEnd = ItemManager.ConstGoodsEndTime;
            }

            int GetTimeEndOfSpecialItem = ItemManager.GetItemExpiesTime(ItemID);

            if (GetTimeEndOfSpecialItem != -1)
            {
                DateTime dt = DateTime.Now.AddMinutes(GetTimeEndOfSpecialItem);

                // "1900-01-01 12:00:00";
                TmpEnd = dt.ToString("yyyy-MM-dd HH:mm:ss");
            }

            int Series = ItemData.Series;

            if (InputSeri != -1)
            {
                Series = InputSeri;
            }
            else
            {
                if (Series == -1)
                {
                    Series = KTGlobal.GetRandomNumber(1, 5);
                }
            }

            TmpStart = ItemManager.ConstGoodsEndTime;

            int Quality = 1;
            int ForgeLevel = enhanceLevel;

            string Probs = "";

            int NewHit = 1;// 1 : Có thông báo cho client nhận được vật phẩm mới

            string JewelList = "";

            if (ItemData.Stack == 0)
            {
                ItemData.Stack = 1;
            }
            /// NẾU LÀ TRANG BỊ
            if (KD_ISEQUIP(ItemData.Genre) || KD_ISPETEQUIP(ItemData.Genre))
            {
                // Set chỗ này đề phòng bug
                ItemData.Stack = 1;
                if (InputProb == "")
                {
                    // ItemNum = 1;
                    Probs = ItemManager.GenerateProbs(ItemData);
                }
                else
                {
                    //  ItemNum = 1;
                    Probs = InputProb;
                }
            }

            // NẾU KHÔNG PHẢI TRANG BỊ
            {
                int MaxStack = ItemData.Stack;

                // Nếu tận dụng lại các ô đồ cũ
                if (useOldGrid)
                {
                    // Nếu mà item có stack > 1 thì mới có tác dụng trong việc sử dụng OLDGIRL
                    if (MaxStack > 1)
                    {
                        GoodsData goodsData = client.GoodsData.Find(x => x.Site == 0 && x.GoodsID == ItemID && x.Binding == IsBinding && ItemManager.DateTimeEqual(x.Endtime, TmpEnd) && x.GCount < MaxStack);

                        // Lấy ra vật phẩm xem còn vật phẩm nào còn có thể STACK thêm được không

                        while (null != goodsData && ItemNum > 0)
                        {
                            if (goodsData.GCount < MaxStack)
                            {
                                int newGoodsNum = 0;
                                int newNum = ItemNum + goodsData.GCount;
                                if (newNum > MaxStack)
                                {
                                    newGoodsNum = MaxStack;
                                    ItemNum = newNum - MaxStack;
                                }
                                else
                                {
                                    newGoodsNum = goodsData.GCount + ItemNum;
                                    ItemNum = 0;
                                }

                                // Xây dựng DICT update
                                Dictionary<UPDATEITEM, object> TotalUpdate = new Dictionary<UPDATEITEM, object>();

                                TotalUpdate.Add(UPDATEITEM.ROLEID, client.RoleID);
                                TotalUpdate.Add(UPDATEITEM.ITEMDBID, goodsData.Id);

                                TotalUpdate.Add(UPDATEITEM.GCOUNT, newGoodsNum);

                                string ScriptUpdateBuild = ItemManager.ItemUpdateScriptBuild(TotalUpdate);

                                string[] UpdateFriend = null;

                                // Request Sửa Số lượng vào DB
                                Global.RequestToDBServer(Global._TCPManager.tcpClientPool, pool, (int)TCPGameServerCmds.CMD_DB_UPDATEGOODS_CMD, ScriptUpdateBuild, out UpdateFriend, client.ServerId);

                                // Modify Danh sách vật phẩm hiện tại tại GS
                                client.GoodsData.LocalModify(goodsData.Id, TotalUpdate);

                                /// Gửi về Client
                                SCModGoods scData = new SCModGoods()
                                {
                                    BagIndex = goodsData.BagIndex,
                                    Count = newGoodsNum,
                                    IsUsing = goodsData.Using,
                                    ModType = (int)ModGoodsTypes.ModValue,
                                    ID = goodsData.Id,
                                    NewHint = 1,
                                    Site = goodsData.Site,
                                    State = 0,
                                };
                                client.SendPacket((int)TCPGameServerCmds.CMD_SPR_MOD_GOODS, scData);
                            }
                            // THỰC HIỆN LOOP HẾT CÁC ITEM NẾU CHƯA FILL HẾT CÁC ITEM CÒN THIẾU
                            goodsData = client.GoodsData.Find(x => x.Site == 0 && x.GoodsID == ItemID && x.Binding == IsBinding && ItemManager.DateTimeEqual(x.Endtime, TmpEnd) && x.GCount < MaxStack);
                        }
                    }

                    if (ItemNum <= 0) // NẾU KHÔNG CẦN VADD THÊM VẬT PHẨM NỮA THÌ RETURN 0 ==> ADD VẬT PHẨM THÀNH CÔNG
                    {
                        ProcessTask.ProseccTaskBeforeDoTask(Global._TCPManager.MySocketListener, pool, client);
                        return true;
                    }
                }

                // Vòng while thứ 2 để add tới khi nào đủ số item yêu cầu thì thôi
                while (ItemNum > 0)
                {
                    int ItemCountAdd = 0;

                    if (ItemNum > MaxStack)
                    {
                        ItemCountAdd = MaxStack;
                        ItemNum = ItemNum - MaxStack;
                    }
                    else
                    {
                        ItemCountAdd = ItemNum;
                        ItemNum = 0;
                    }

                    int BagIndex = 0;
                    // Nếu là thêm vật phẩm thẳng vào túi đồ trên người
                    if (0 == Site)
                    {
                        BagIndex = client.GoodsData.GetBagFirstEmptyPosition(0);
                    }
                    else if ((int)SaleGoodsConsts.PortableGoodsID == Site)
                    {
                        BagIndex = client.GoodsData.GetBagFirstEmptyPosition(1);
                    }

                    // COde lại nếu mà vị trí túi đồ < 0 hoặc là vị trí túi đồ > 100 thì là không còn chỗ trống
                    if (BagIndex < 0)
                    {
                        KTPlayerManager.ShowNotification(client, "Túi đồ đã đầy! Vui lòng dọn túi đồ và thử lại!");
                        return false;
                    }

                    string newEndTime = TmpEnd.Replace(":", "$");
                    string newStartTime = TmpStart.Replace(":", "$");

                    Dictionary<ItemPramenter, string> OtherParams = new Dictionary<ItemPramenter, string>();

                    // Đoạn này để xử lý toàn bộ các item cần lưu thuộc tính OTHER PRAMM ngay ban đầu ví dụ như mật tịch
                    if (KD_ISEQUIP(ItemData.Genre))
                    {
                        //if (KD_ISBOOK(ItemData.DetailType))
                        //{
                        //    string LeveBegin = "1";
                        //    string ExpBegin = "0";
                        //    OtherParams.Add(ItemPramenter.Pram_1, LeveBegin);
                        //    OtherParams.Add(ItemPramenter.Pram_2, ExpBegin);
                        //    OtherParams.Add(ItemPramenter.Pram_3, GetMaxbookEXP(1, ItemData.Level) + "");
                        //}
                        //Nếu đây là ngũ hành ấn

                        if (KD_ISSIGNET(ItemData.DetailType))
                        {
                            string LeveBegin = "1";
                            string ExpBegin = "0";

                            string Final = LeveBegin + "|" + ExpBegin;

                            OtherParams.Add(ItemPramenter.Pram_1, Final);
                            OtherParams.Add(ItemPramenter.Pram_2, Final);
                        }
                    }
                    else
                    {
                        // Nếu nó là KIM tê thì ghi lại số đơn vị có thể sửa chữa đồ tối đa
                        //if (KD_ISJINXI(ItemData.ItemID))
                        //{
                        //    OtherParams.Add(ItemPramenter.Pram_1, ItemData.ListExtPram[0].Pram + "");
                        //}

                        // Nếu vật phẩm là càn khôn phù
                        if (IsQianKunFu(ItemData.ItemID))
                        {
                            if (ItemData.ItemID == 354)
                            {
                                OtherParams.Add(ItemPramenter.Pram_1, 100 + "");
                            }

                            if (ItemData.ItemID == 344)
                            {
                                OtherParams.Add(ItemPramenter.Pram_1, 10 + "");
                            }
                        }
                    }

                    if (Author != "")
                    {
                        OtherParams.Add(ItemPramenter.Creator, Author);
                    }

                    byte[] ItemDataByteArray = DataHelper.ObjectToBytes(OtherParams);

                    string otherPramer = Convert.ToBase64String(ItemDataByteArray);

                    int Duability = GetDurabilityItem(Probs, ItemData);

                    //SEND DATA TO DB SERVER REQUEST SAVE TO DB
                    string[] dbFields = null;
                    string strcmd = string.Format("{0}:{1}:{2}:{3}:{4}:{5}:{6}:{7}:{8}:{9}:{10}:{11}:{12}",
                        client.RoleID, ItemData.ItemID, ItemCountAdd, Probs, ForgeLevel, IsBinding, Site, BagIndex,
                        newStartTime, newEndTime, Duability, Series, otherPramer);

                    TCPProcessCmdResults dbRequestResult = Global.RequestToDBServer(Global._TCPManager.tcpClientPool, pool, (int)TCPGameServerCmds.CMD_DB_ADDGOODS_CMD, strcmd, out dbFields, client.ServerId);
                    // Nếu lỗi khi kết nối tới GAMEDB
                    if (dbRequestResult == TCPProcessCmdResults.RESULT_FAILED)
                    {
                        LogManager.WriteLog(LogTypes.Item, "[" + client.strUserID + "][" + client.RoleID + "][" + client.RoleName + "][" + From + "] Create Item Error : " + ItemID + " => DB Writer BUG Connect To DbServer :" + ItemCountAdd + " | ITEMCMD :" + strcmd);
                        return false;
                    }

                    // Nếu CÓ LỖI KHI ADD VÀO DB THÌ TRẢ VỀ MÃ LỖI
                    if (dbFields.Length <= 0 || Convert.ToInt32(dbFields[0]) < 0)
                    {
                        LogManager.WriteLog(LogTypes.Item, "[" + client.strUserID + "][" + client.RoleID + "][" + client.RoleName + "][" + From + "] Create Item Error : " + ItemID + " => DB Writer BUG Connect To DbServer :" + ItemCountAdd + " | EROR CODE  :" + Convert.ToInt32(dbFields[0]));

                        return false;
                    }

                    int DbItemID = Convert.ToInt32(dbFields[0]);

                    GoodsData gd = null;

                    if (Site == 0)
                    {
                        gd = ItemManager.AddGoodsData(client, DbItemID, ItemData.ItemID, ForgeLevel, Quality, ItemCountAdd, IsBinding, Site, TmpStart, TmpEnd,
                            Duability, BagIndex, -1, Probs, Series, OtherParams);

                        //Console.WriteLine("SERIES :" + Series + "|" + DbItemID);
                        // Xử lý task => đối với nhiệm vụ yêu cầu mua vật phẩm gì đó
                        // ProcessTask.Process(Global._TCPManager.MySocketListener, Global._TCPManager.TcpOutPacketPool, client, -1, -1, ItemData.ItemID, TaskTypes.BuySomething);
                    }

                    if (null != gd)
                    {
                        // Nếu mà cần ghi logs vật phẩm này
                        if (IsNeedWriterLogs)
                        {
                            int RoleID = client.RoleID;
                            string AccountName = client.strUserID;
                            string RecoreType = "ITEMCREATE";
                            string RecoreDesc = From;
                            string Source = "SYSTEM";
                            string Taget = client.RoleName;
                            string OptType = "ADD";

                            int ZONEID = client.ZoneID;

                            string OtherPram_1 = DbItemID + "";
                            string OtherPram_2 = gd.GoodsID + "";
                            string OtherPram_3 = gd.GCount + "";
                            string OtherPram_4 = "NONE";

                            //Thực hiện việc ghi LOGS vào DB
                            GameManager.logDBCmdMgr.WriterLogs(RoleID, AccountName, RecoreType, RecoreDesc, Source, Taget, OptType, ZONEID, OtherPram_1, OtherPram_2, OtherPram_3, OtherPram_4, client.ServerId);
                        }

                        // LogManager.WriteLog(LogTypes.Item, "[" + client.strUserID + "][" + client.RoleID + "][" + client.RoleName + "][" + From + "] CreateItem : " + ItemData.Name + "|" + ItemData.ItemID + "| Count :" + ItemCountAdd);
                    }

                    // SEND NOTIFY về vật phẩm mới được add vào TÚI đồ
                    KT_TCPHandler.NotifySelfAddGoods(client, DbItemID, ItemData.ItemID, ForgeLevel, ItemCountAdd, IsBinding, Site, HitItem, newEndTime, Duability, BagIndex, -1, Probs, Series, OtherParams);
                }

                // Thực hiện update các vật phẩm liên quan tới nhiệm vụ cần có vật phẩm
                ProcessTask.ProseccTaskBeforeDoTask(Global._TCPManager.MySocketListener, pool, client);

                // Kiểm tra vị trí trống của BALO

                return true;
            }
        }

        #region Vật phẩm

        /// <summary>
        /// Ghép vật phẩm
        /// </summary>
        /// <param name="inputClientItems"></param>
        /// <param name="player"></param>
        /// <returns></returns>
        public static bool MergeItems(List<GoodsData> inputClientItems, KPlayer player)
        {
            /// Toác
            if (inputClientItems == null)
            {
                KTPlayerManager.ShowNotification(player, "Dữ liệu không hợp lệ!");
                return false;
            }

            /// Tổng số vật phẩm
            int gCount = 0;
            /// Mảng đánh dấu đã tồn tại ID vật phẩm tương ứng chưa
            HashSet<int> availableItems = new HashSet<int>();
            /// ID vật phẩm
            int itemID = -1;
            /// Có tìm thấy vật phẩm khóa không
            bool foundBound = false;
            /// Có tìm thấy vật phẩm không khóa không
            bool foundUnbound = false;
            /// Duyệt danh sách
            foreach (GoodsData itemGD in inputClientItems)
            {
                /// Toác
                if (itemGD == null)
                {
                    continue;
                }

                /// Thông tin vật phẩm thật ở trong túi
                GoodsData gd = player.GoodsData.Find(itemGD.Id, 0);
                /// Toác
                if (gd == null)
                {
                    KTPlayerManager.ShowNotification(player, "Dữ liệu không hợp lệ!");
                    continue;
                }

                /// Nếu có hạn sử dụng
                if (gd.Endtime != ItemManager.ConstGoodsEndTime)
                {
                    KTPlayerManager.ShowNotification(player, "Chỉ có đồ không có hạn sử dụng mới có thể xếp chồng!");
                    return false;
                }

                /// Nếu chưa tìm thấy vật phẩm
                if (itemID == -1)
                {
                    itemID = gd.GoodsID;
                }

                /// Nếu ID không khớp nhau
                if (gd.GoodsID != itemID)
                {
                    KTPlayerManager.ShowNotification(player, "Chỉ có vật phẩm cùng loại mới có thể xếp chồng!");
                    return false;
                }

                /// Nếu số lượng không thỏa mãn
                if (gd.GCount <= 0)
                {
                    KTPlayerManager.ShowNotification(player, "Dữ liệu không hợp lệ!");
                    return false;
                }

                /// Nếu đã tồn tại => Có BUG Duplicate vật phẩm ở Client gửi lên
                if (availableItems.Contains(gd.Id))
                {
                    KTPlayerManager.ShowNotification(player, "Dữ liệu không hợp lệ!");
                    return false;
                }
                /// Thêm vào danh sách
                availableItems.Add(gd.Id);

                /// Nếu vật phẩm khóa
                if (gd.Binding == 1)
                {
                    foundBound = true;
                }
                /// Nếu vật phẩm không khóa
                else
                {
                    foundUnbound = true;
                }

                /// Tăng tổng số vật phẩm tìm thấy lên
                gCount += gd.GCount;
            }

            /// Danh sách truyền về không khớp
            if (availableItems.Count != inputClientItems.Count)
            {
                KTPlayerManager.ShowNotification(player, "Dữ liệu không hợp lệ");
                return false;
            }

            /// Nếu có ít hơn 2 vật phẩm
            if (availableItems.Count < 2)
            {
                KTPlayerManager.ShowNotification(player, "Phải nhiều hơn 1 vật phẩm mới có thể xếp chồng");
                return false;
            }

            /// Thông tin vật phẩm tương ứng
            ItemData itemData = ItemManager.GetItemTemplate(itemID);
            if (itemData != null)
            {
                if (itemData.Stack <= 1)
                {
                    KTPlayerManager.ShowNotification(player, "Vật phẩm này không thể xếp chồng!");
                    return false;
                }
            }

            /// Nếu tìm thấy cả vật phẩm khóa và không khóa
            if ((foundBound && foundUnbound) || (!foundBound && !foundUnbound))
            {
                KTPlayerManager.ShowNotification(player, "Đồ phải loại khóa hoặc không khóa mới có thể xếp chồng!");
                return false;
            }

            /// Duyệt danh sách
            foreach (int itemDBID in availableItems)
            {
                /// Xóa vật phẩm
                if (!ItemManager.AbadonItem(itemDBID, player, false, "MergeItems"))
                {
                    KTPlayerManager.ShowNotification(player, "Có sai sót dữ liệu");
                    return false;
                }
            }

            /// Tạo vật phẩm mới số lượng tương ứng
            if (!ItemManager.CreateItem(Global._TCPManager.TcpOutPacketPool, player, itemID, gCount, 0, "MergeItems", false, foundBound ? 1 : 0, false, ItemManager.ConstGoodsEndTime))
            {
                KTPlayerManager.ShowNotification(player, "Có lỗi khi nhận vật phẩm chế tạo");
            }

            /// Trả về kết quả
            return true;
        }

        /// <summary>
        /// Trả về tổng số vật phẩm tương ứng trong túi người chơi
        /// </summary>
        /// <param name="player"></param>
        /// <param name="itemID"></param>
        /// <returns></returns>
        public static int GetItemCountInBag(KPlayer player, int itemID, int binding = -1)
        {
            /// Số lượng
            int count = 0;

            /// Nếu không yêu cầu khóa hay không
            if (binding == -1)
            {
                count = player.GoodsData.FindAll(x => x.GoodsID == itemID && x.Site == 0 && x.Using == -1 && x.GCount > 0).Sum(x => x.GCount);
            }
            /// Nếu yêu cầu khóa hay không khóa
            else
            {
                count = player.GoodsData.FindAll(x => x.GoodsID == itemID && x.Site == 0 && x.Using == -1 && x.GCount > 0 && x.Binding == binding).Sum(x => x.GCount);
            }

            /// Trả về kết quả
            return count;
        }

        /// <summary>
        /// Lấy ra danh sách vật phẩm theo loại trong túi đồ
        /// Hàm này tạo ra để phục vụ cho dã tẩu, Thực hiện verify item sẽ  nhanh hơn
        /// </summary>
        /// <param name="player"></param>
        /// <param name="itemID"></param>
        /// <param name="Binding"></param>
        /// <returns></returns>
        public static List<GoodsData> GetItemByRequest(KPlayer player, int ItemType, int ItemCATEGORY, int ItemSeries)
        {
            return player.GoodsData.FindAll((itemGD) =>
            {
                /// Nếu đang mặc thì thôi
                if (itemGD.Using >= 0)
                {
                    return false;
                }

                /// Nếu không phải trong túi đồ thì thôi
                if (itemGD.Site != 0)
                {
                    return false;
                }

                /// Nếu ngũ hành không phù hợp thì bỏ qua
                if (ItemSeries != 0)
                {
                    if (ItemSeries != itemGD.Series)
                    {
                        return false;
                    }
                }

                /// Template tương ứng
                ItemData itemData = GetItemTemplate(itemGD.GoodsID);
                /// Toác
                if (itemData == null)
                {
                    return false;
                }

                /// Nếu không phải trang bị thì bỏ qua không lấy
                if (!KD_ISEQUIP(itemData.Genre))
                {
                    return false;
                }

                /// Nếu có yêu cầu loại
                if (ItemType != -1)
                {
                    /// Nếu như loại không đạt yêu cầu thì chim cút
                    if (itemData.DetailType != ItemType)
                    {
                        return false;
                    }
                }

                /// Nếu có yêu cầu loại trang bị
                if (ItemCATEGORY != -1)
                {
                    if (itemData.Category != ItemCATEGORY)
                    {
                        return false;
                    }
                }

                /// Thỏa mãn
                return true;
            });
        }

        /// <summary>
        /// Xóa vật phẩm số lượng tương ứng khỏi túi người chơi
        ///
        /// </summary>
        /// <param name="player"></param>
        /// <param name="itemID"></param>
        /// <param name="count"></param>
        public static bool RemoveItemFromBag(KPlayer player, int itemID, int count, int Binding = -1, string From = "")
        {
            /// Số lượng hiện có
            int itemCount = ItemManager.GetItemCountInBag(player, itemID);

            /// Nếu không đủ lượng xóa
            if (itemCount < count)
            {
                return false;
            }

            /// Số lượng cần xóa còn lại
            int quantityLeft = count;

            /// Danh sách cần xóa
            List<WaitBeRemove> toBeRemovedList = new List<WaitBeRemove>();
            /// Danh sách vật phẩm cần tương tác
            List<GoodsData> goods = player.GoodsData.FindAll((itemGD) =>
            {
                /// Nếu không phải túi đồ
                if (itemGD.Site != 0)
                {
                    /// Bỏ qua
                    return false;
                }
                /// Nếu đang trang bị
                else if (itemGD.Using >= 0)
                {
                    /// Bỏ qua
                    return false;
                }
                /// Nếu không phải ID vật phẩm cần xóa
                else if (itemGD.GoodsID != itemID)
                {
                    /// Bỏ qua
                    return false;
                }

                /// Nếu chỉ xóa vật phẩm khóa hoặc không khóa
                if (Binding != -1)
                {
                    /// Nếu mà có khóa hoặc không khóa thì sẽ check xem vật phẩm có thuộc tính khóa hoặc không khóa theo yêu cầu không
                    if (itemGD.Binding != Binding)
                    {
                        /// Bỏ qua
                        return false;
                    }
                }

                /// OK thì lấy
                return true;
            });
            /// Duyệt danh sách vật phẩm cần tương tác
            foreach (GoodsData itemGD in goods)
            {
                // Trường hợp 1 nếu mà vật phẩm có số lượng nhiều hơn số lượng cần
                if (itemGD.GCount >= quantityLeft)
                {
                    /// Giảm số lượng tương ứng
                    int quantity = itemGD.GCount - quantityLeft;

                    /// Tạo mới
                    WaitBeRemove Item = new WaitBeRemove();
                    Item._Good = itemGD;
                    Item.ItemLess = quantity;

                    /// Thêm vào danh sách cần xóa
                    toBeRemovedList.Add(Item);

                    /// Cập nhật số lượng còn lại cần xóa
                    quantityLeft = 0;
                }
                else
                {
                    /// Cập nhật số lượng cần xóa
                    quantityLeft = quantityLeft - itemGD.GCount;

                    /// Tạo mới
                    WaitBeRemove Item = new WaitBeRemove();
                    Item._Good = itemGD;
                    Item.ItemLess = 0;

                    /// Thêm vào danh sách cần xóa
                    toBeRemovedList.Add(Item);
                }

                /// Nếu số lượng cần xóa còn lại <= 0
                if (quantityLeft <= 0)
                {
                    break;
                }
            }

            /// Duyệt danh sách cần xóa
            foreach (WaitBeRemove request in toBeRemovedList)
            {
                /// Xóa lần lượt
                ItemManager.UpdateItemCount(request._Good, player, request.ItemLess, From);
            }

            //Console.WriteLine(quantityLeft);

            return true;
        }

        /// <summary>
        /// Xóa vật phẩm sau khi sử dụng (nếu có)
        /// </summary>
        /// <param name="player"></param>
        /// <param name="itemGD"></param>
        public static bool DeductItemOnUse(KPlayer player, GoodsData itemGD, string FROM = "")
        {
            /// Nếu vật phẩm không tồn tại
            if (!ItemManager._TotalGameItem.TryGetValue(itemGD.GoodsID, out ItemData itemData))
            {
                KTPlayerManager.ShowNotification(player, "Vật phẩm không tồn tại!");
                return false;
            }

            /// Nếu vật phẩm có thiết lập xóa sau khi sử dụng
            if (itemData.DeductOnUse)
            {
                /// Số lượng còn lại
                itemGD.GCount--;

                /// Cập nhật số lượng còn lại
                return ItemManager.UpdateItemCount(itemGD, player, itemGD.GCount, FROM);
            }

            return false;
        }

        public static bool RemoveItemByCount(KPlayer player, GoodsData itemGD, int Count, string From)
        {
            /// Nếu không có vật phẩm
            if (itemGD == null)
            {
                KTPlayerManager.ShowNotification(player, "Vật phẩm không tồn tại!");
                return false;
            }

            /// Nếu vật phẩm không tồn tại
            if (!ItemManager._TotalGameItem.TryGetValue(itemGD.GoodsID, out ItemData itemData))
            {
                KTPlayerManager.ShowNotification(player, "Vật phẩm không tồn tại!");
                return false;
            }

            if (itemGD.GCount < Count)
            {
                KTPlayerManager.ShowNotification(player, "Số lượng vật phẩm không đủ!");
                return false;
            }

            /// Số lượng còn lại
            itemGD.GCount = itemGD.GCount - Count;

            /// Cập nhật số lượng còn lại
            return ItemManager.UpdateItemCount(itemGD, player, itemGD.GCount, From);
        }

        /// <summary>
        /// Sử dụng vật phẩm tương ứng
        /// </summary>
        /// <param name="player"></param>
        /// <param name="itemGD"></param>
        public static void UseItem(KPlayer player, GoodsData itemGD)
        {
            /// Nếu trong trạng thái không thể dùng vật phẩm
            if (!player.IsCanDoLogic())
            {
                //PlayerManager.ShowNotification(player, "Trong trạng thái khống chế không thể sử dụng vật phẩm!");
                return;
            }

            /// Nếu vật phẩm không tồn tại
            if (!ItemManager._TotalGameItem.TryGetValue(itemGD.GoodsID, out ItemData itemData))
            {
                KTPlayerManager.ShowNotification(player, "Vật phẩm không tồn tại!");
                return;
            }

            /// Nếu là trang bị
            if (ItemManager.KD_ISEQUIP(itemData.Genre) || KD_ISPETEQUIP(itemData.Genre))
            {
                return;
            }
            /// Nếu cấp độ không đủ
            //else if (itemData.ReqLevel > player.m_Level)
            //{
            //    PlayerManager.ShowNotification(player, string.Format("Cần đạt cấp {0} trở lên mới có thể sử dụng vật phẩm này!", itemData.ReqLevel));
            //    return;
            //}

            /// Nếu có Script điều khiển
            if (itemData.IsScriptItem)
            {
                /// Script điều khiển tương ứng
                KTLuaScript.LuaScript script = KTLuaScript.Instance.GetScriptByID(itemData.ScriptID);
                /// Nếu Script không tồn tại
                if (script == null)
                {
                    LogManager.WriteLog(LogTypes.Item, string.Format("Player '{0}' uses item ID '{1}', calls script ID '{2}' NOT FOUND!!!", player.RoleName, itemGD.GoodsID, itemData.ScriptID));
                    return;
                }

                /// Bản đồ tương ứng
                GameMap map = KTMapManager.Find(player.MapCode);
                if (map == null)
                {
                    return;
                }

                /// Danh sách các tham biến khác
                Dictionary<int, string> otherParams = new Dictionary<int, string>();

                /// Thực thi hàm kiểm tra điều kiện ở Script tương ứng
                KTLuaEnvironment.ExecuteItemScript_OnPreCheckCondition(map, itemGD, player, itemData.ScriptID, otherParams, (res) =>
                {
                    /// Nếu kiểm tra điều kiện thất bại
                    if (!res)
                    {
                        return;
                    }

                    /// Thực thi hàm dùng vật phẩm ở Script tương ứng
                    KTLuaEnvironment.ExecuteItemScript_OnUse(map, itemGD, player, itemData.ScriptID, otherParams);

                    /// Thực thi sự kiện dùng vật phẩm
                    player.UseItemCompleted(itemGD);
                });
            }
            /// Nếu là thuốc
            else if (itemData.IsMedicine)
            {
                /// Nếu Prop không tồn tại
                if (itemData.MedicineProp == null || itemData.MedicineProp.Count <= 0 || itemData.BuffID == -1)
                {
                    return;
                }

                /// ID kỹ năng tương ứng
                int skillID = itemData.BuffID;
                /// Thời gian tồn tại
                int duration = itemData.MedicineProp[0].Time * 1000 / 18;
                /// Có lưu vào DB không
                bool saveToDB = itemData.BuffID == 100020;

                /// Kỹ năng tương ứng
                SkillDataEx skill = KSkill.GetSkillData(skillID);

                /// Nếu kỹ năng không tồn tại
                if (skill == null)
                {
                    return;
                }

                /// Dữ liệu kỹ năng tương ứng
                SkillLevelRef skillRef = new SkillLevelRef()
                {
                    Data = skill,
                    AddedLevel = 1,
                    BonusLevel = 0,
                    CanStudy = false,
                };
                BuffDataEx buff = new BuffDataEx()
                {
                    Skill = skillRef,
                    Duration = duration,
                    LoseWhenUsingSkill = false,
                    SaveToDB = saveToDB,
                    StackCount = 1,
                    StartTick = KTGlobal.GetCurrentTimeMilis(),
                    CustomProperties = new PropertyDictionary(),
                };

                /// Có phải dùng cho pet không
                bool isPet = false;
                /// Duyệt toàn bộ thuộc tính của thuốc
                foreach (Medicine prop in itemData.MedicineProp)
                {
                    string propName = prop.MagicName;
                    int value = prop.Value;

                    /// Nếu là skill_param2_v
                    if (propName == "skill_param2_v")
                    {
                        /// Dùng cho pet
                        isPet = true;
                        continue;
                    }

                    /// Lấy thuộc tính tương ứng theo tên
                    if (PropertyDefine.PropertiesBySymbolName.TryGetValue(propName, out PropertyDefine.Property property))
                    {
                        KMagicAttrib magicAttrib = new KMagicAttrib()
                        {
                            nAttribType = (MAGIC_ATTRIB)property.ID,
                            nValue = new int[] { value, 100, 0 },
                        };

                        buff.CustomProperties.Set<KMagicAttrib>(property.ID, magicAttrib);
                    }
                }

                /// Nếu là pet
                if (isPet)
                {
                    /// Nếu không có pet đang tham chiến
                    if (player.CurrentPet == null || player.CurrentPet.IsDead())
                    {
                        /// Thông báo
                        KTPlayerManager.ShowNotification(player, "Không có tinh linh đang tham chiến!");
                        return;
                    }
                    /// Thêm Buff vào cho pet của người chơi
                    player.CurrentPet.Buffs.AddBuff(buff);
                }
                /// Nếu là người
                else
                {
                    /// Thêm Buff vào cho người chơi
                    player.Buffs.AddBuff(buff);
                }

                /// Nếu vật phẩm có thiết lập xóa sau khi sử dụng
                if (itemData.DeductOnUse)
                {
                    /// Số lượng còn lại
                    itemGD.GCount--;

                    /// Cập nhật số lượng còn lại
                    ItemManager.UpdateItemCount(itemGD, player, itemGD.GCount, "USINGITEM");
                }

                /// Thực thi sự kiện dùng vật phẩm
                player.UseItemCompleted(itemGD);
            }
        }

        #endregion Vật phẩm

        public static int GetDurabilityItem(string props, ItemData itemData)
        {
            int Duabilaty = 100;

            // Nếu là trang bị mới có độ bền
            if (KD_ISNONNAMENT(itemData.DetailType))
            {
                string Value = "";

                if (String.IsNullOrEmpty(props) || props.Contains(("ERORR")))
                {
                    List<BasicProp> emptyProps = itemData.ListBasicProp?.OrderBy(x => x.Index)?.ToList();

                    var FindValue = emptyProps.Where(x => x.BasicPropType == "durability_v").FirstOrDefault();

                    if (FindValue != null)
                    {
                        Duabilaty = FindValue.BasicPropPA1Min;
                    }
                }
                else
                {
                    byte[] base64Decode = Convert.FromBase64String(props);
                    ItemDataByteCode equipProp = DataHelper.BytesToObject<ItemDataByteCode>(base64Decode, 0, base64Decode.Length);

                    if (equipProp.BasicPropCount > 0)
                    {
                        var FindValue = equipProp.BasicProp.Where(x => x.nAttribType == 2).FirstOrDefault();
                        if (FindValue != null)
                        {
                            Duabilaty = FindValue.Value_1;
                        }
                    }
                }
            }

            return Duabilaty;
        }

        #region BoundItemFaction

        /// <summary>
        /// Hàm này add vật phẩm cho người chơi khi tham gia vào môn phái nào đó
        /// </summary>
        public static void AddFactionItem(KPlayer client, int FactionJoin)
        {
            switch (FactionJoin)
            {
                case 1:
                    {
                        ItemManager.CreateItem(Global._TCPManager.TcpOutPacketPool, client, 31761, 1, 0, "JOINFACTION", false, 1, false, ItemManager.ConstGoodsEndTime);
                        ItemManager.CreateItem(Global._TCPManager.TcpOutPacketPool, client, 31762, 1, 0, "JOINFACTION", false, 1, false, ItemManager.ConstGoodsEndTime);
                        ItemManager.CreateItem(Global._TCPManager.TcpOutPacketPool, client, 31763, 1, 0, "JOINFACTION", false, 1, false, ItemManager.ConstGoodsEndTime);

                        break;
                    }
                case 2:
                    {
                        ItemManager.CreateItem(Global._TCPManager.TcpOutPacketPool, client, 31764, 1, 0, "JOINFACTION", false, 1, false, ItemManager.ConstGoodsEndTime);
                        ItemManager.CreateItem(Global._TCPManager.TcpOutPacketPool, client, 31765, 1, 0, "JOINFACTION", false, 1, false, ItemManager.ConstGoodsEndTime);
                        ItemManager.CreateItem(Global._TCPManager.TcpOutPacketPool, client, 31766, 1, 0, "JOINFACTION", false, 1, false, ItemManager.ConstGoodsEndTime);

                        break;
                    }
                case 4:
                    {
                        ItemManager.CreateItem(Global._TCPManager.TcpOutPacketPool, client, 31767, 1, 0, "JOINFACTION", false, 1, false, ItemManager.ConstGoodsEndTime);
                        ItemManager.CreateItem(Global._TCPManager.TcpOutPacketPool, client, 31768, 1, 0, "JOINFACTION", false, 1, false, ItemManager.ConstGoodsEndTime);

                        break;
                    }
                case 3:
                    {
                        ItemManager.CreateItem(Global._TCPManager.TcpOutPacketPool, client, 31769, 1, 0, "JOINFACTION", false, 1, false, ItemManager.ConstGoodsEndTime);
                        ItemManager.CreateItem(Global._TCPManager.TcpOutPacketPool, client, 31770, 1, 0, "JOINFACTION", false, 1, false, ItemManager.ConstGoodsEndTime);
                        ItemManager.CreateItem(Global._TCPManager.TcpOutPacketPool, client, 31771, 1, 0, "JOINFACTION", false, 1, false, ItemManager.ConstGoodsEndTime);

                        break;
                    }
                case 5:
                    {
                        ItemManager.CreateItem(Global._TCPManager.TcpOutPacketPool, client, 31772, 1, 0, "JOINFACTION", false, 1, false, ItemManager.ConstGoodsEndTime);
                        ItemManager.CreateItem(Global._TCPManager.TcpOutPacketPool, client, 31773, 1, 0, "JOINFACTION", false, 1, false, ItemManager.ConstGoodsEndTime);

                        break;
                    }

                case 6:
                    {
                        ItemManager.CreateItem(Global._TCPManager.TcpOutPacketPool, client, 31774, 1, 0, "JOINFACTION", false, 1, false, ItemManager.ConstGoodsEndTime);
                        ItemManager.CreateItem(Global._TCPManager.TcpOutPacketPool, client, 31775, 1, 0, "JOINFACTION", false, 1, false, ItemManager.ConstGoodsEndTime);

                        break;
                    }
                case 7:
                    {
                        ItemManager.CreateItem(Global._TCPManager.TcpOutPacketPool, client, 31776, 1, 0, "JOINFACTION", false, 1, false, ItemManager.ConstGoodsEndTime);
                        ItemManager.CreateItem(Global._TCPManager.TcpOutPacketPool, client, 31777, 1, 0, "JOINFACTION", false, 1, false, ItemManager.ConstGoodsEndTime);

                        break;
                    }
                case 8:
                    {
                        ItemManager.CreateItem(Global._TCPManager.TcpOutPacketPool, client, 31778, 1, 0, "JOINFACTION", false, 1, false, ItemManager.ConstGoodsEndTime);
                        ItemManager.CreateItem(Global._TCPManager.TcpOutPacketPool, client, 31779, 1, 0, "JOINFACTION", false, 1, false, ItemManager.ConstGoodsEndTime);

                        break;
                    }
                case 9:
                    {
                        ItemManager.CreateItem(Global._TCPManager.TcpOutPacketPool, client, 31780, 1, 0, "JOINFACTION", false, 1, false, ItemManager.ConstGoodsEndTime);
                        ItemManager.CreateItem(Global._TCPManager.TcpOutPacketPool, client, 31781, 1, 0, "JOINFACTION", false, 1, false, ItemManager.ConstGoodsEndTime);

                        break;
                    }
                case 10:
                    {
                        ItemManager.CreateItem(Global._TCPManager.TcpOutPacketPool, client, 31782, 1, 0, "JOINFACTION", false, 1, false, ItemManager.ConstGoodsEndTime);
                        ItemManager.CreateItem(Global._TCPManager.TcpOutPacketPool, client, 31783, 1, 0, "JOINFACTION", false, 1, false, ItemManager.ConstGoodsEndTime);

                        break;
                    }
                case 11:
                    {
                        ItemManager.CreateItem(Global._TCPManager.TcpOutPacketPool, client, 31784, 1, 0, "JOINFACTION", false, 1, false, ItemManager.ConstGoodsEndTime);
                        ItemManager.CreateItem(Global._TCPManager.TcpOutPacketPool, client, 31785, 1, 0, "JOINFACTION", false, 1, false, ItemManager.ConstGoodsEndTime);

                        break;
                    }
                case 12:
                    {
                        ItemManager.CreateItem(Global._TCPManager.TcpOutPacketPool, client, 41016, 1, 0, "JOINFACTION", false, 1, false, ItemManager.ConstGoodsEndTime);
                        ItemManager.CreateItem(Global._TCPManager.TcpOutPacketPool, client, 41017, 1, 0, "JOINFACTION", false, 1, false, ItemManager.ConstGoodsEndTime);

                        break;
                    }
                case 16:
                    {
                        ItemManager.CreateItem(Global._TCPManager.TcpOutPacketPool, client, 41175, 1, 0, "JOINFACTION", false, 1, false, ItemManager.ConstGoodsEndTime);
                        ItemManager.CreateItem(Global._TCPManager.TcpOutPacketPool, client, 41176, 1, 0, "JOINFACTION", false, 1, false, ItemManager.ConstGoodsEndTime);

                        break;
                    }
            }
        }

        #endregion BoundItemFaction
    }
}