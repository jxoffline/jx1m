using GameServer.KiemThe.Core.Item;
using GameServer.Logic;
using GameServer.VLTK.Core.Activity.X2ExpEvent;
using Server.Data;
using Server.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Xml.Serialization;
using static GameServer.Logic.KTMapManager;

namespace GameServer.KiemThe.Logic
{
    /// <summary>
    /// Quản lý quái
    /// </summary>
    public static partial class KTMonsterManager
    {
        /// <summary>
        /// Quản lý rơi vật phẩm từ quái
        /// </summary>
        public class MonsterDropManager
        {
            /// <summary>
            /// Cầu hình 1 bản ghi sẽ rơi
            /// </summary>
            [XmlRoot(ElementName = "DropItem")]
            public class DropItem
            {
                [XmlAttribute(AttributeName = "RandRate")]
                public int RandRate { get; set; }

                [XmlAttribute(AttributeName = "ItemID")]
                public int ItemID { get; set; } = -1;
            }

            /// <summary>
            /// Toàn bộ drop config
            /// </summary>
            [XmlRoot(ElementName = "DropConfig")]
            public class DropConfig
            {
                [XmlElement(ElementName = "DropProfile")]
                public List<DropProfile> DropProfile { get; set; }
            }

            /// <summary>
            /// Cầu hình 1 prifile drop
            /// </summary>
            [XmlRoot(ElementName = "DropProfile")]
            public class DropProfile
            {
                [XmlAttribute(AttributeName = "ProfileName")]
                public string ProfileName { get; set; } = "";

                /// <summary>
                /// Tổng số lượng
                /// </summary>
                ///
                [XmlAttribute(AttributeName = "CopySceneOnly")]
                public bool CopySceneOnly { get; set; } = false;

                /// <summary>
                /// Tổng số lượng
                /// </summary>
                [XmlAttribute(AttributeName = "Count")]
                public int Count { get; set; } = -1;

                /// <summary>
                /// Ranger
                /// </summary>
                [XmlAttribute(AttributeName = "RandRange")]
                public long RandRange { get; set; }

                [XmlAttribute(AttributeName = "MagicRate")]
                public int MagicRate { get; set; }

                [XmlAttribute(AttributeName = "MoneyRate")]
                public int MoneyRate { get; set; }

                [XmlAttribute(AttributeName = "MoneyScale")]
                public int MoneyScale { get; set; }

                [XmlAttribute(AttributeName = "MinItemLevel")]
                public int MinItemLevel { get; set; }

                [XmlAttribute(AttributeName = "MaxItemLevel")]
                public int MaxItemLevel { get; set; }

                /// <summary>
                /// Số tiền thấp nhất có thể rơi ra
                /// </summary>
                [XmlAttribute(AttributeName = "MinMoney")]
                public int MinMoney { get; set; } = 0;

                /// <summary>
                /// Số tiền to nhất có thể rơi ra
                /// </summary>
                [XmlAttribute(AttributeName = "MaxMoney")]
                public int MaxMoney { get; set; } = 0;

                /// <summary>
                /// Tỉ lệ rơi vật phẩm
                /// </summary>
                ///
                [XmlAttribute(AttributeName = "ItemRate")]
                public int ItemRate { get; set; }

                /// <summary>
                /// Dánh sách map nào sẽ rơi
                /// </summary>
                [XmlAttribute(AttributeName = "MapsID")]
                public string MapsID { get; set; } = "";

                /// <summary>
                /// Danh sách vật phẩm sẽ rơi
                /// </summary>
                [XmlElement(ElementName = "Drop")]
                public List<DropItem> Drop { get; set; }
            }

            /// <summary>
            /// Thông báo khi nhặt vật phẩm
            /// </summary>
            public class PickUpItemNotify
            {
                /// <summary>
                /// ID vật phẩm
                /// </summary>
                public int ItemID { get; set; }

                /// <summary>
                /// Chỉ thông báo khi nhặt từ Boss
                /// </summary>
                public bool BossOnly { get; set; }

                /// <summary>
                /// Chuyển đối tượng từ XMLNode
                /// </summary>
                /// <param name="xmlNode"></param>
                /// <returns></returns>
                public static PickUpItemNotify Parse(XElement xmlNode)
                {
                    return new PickUpItemNotify()
                    {
                        ItemID = int.Parse(xmlNode.Attribute("ID").Value),
                        BossOnly = bool.Parse(xmlNode.Attribute("BossOnly").Value),
                    };
                }
            }

            /// <summary>
            /// Drop profile config
            /// </summary>

            private static DropConfig _DropConfig = new DropConfig();

            /// <summary>
            /// Danh sách vật phẩm thông báo khi nhặt được
            /// </summary>
            private static readonly Dictionary<int, PickUpItemNotify> pickUpItemNotifications = new Dictionary<int, PickUpItemNotify>();

            #region Init

            /// <summary>
            /// Khởi tạo
            /// </summary>
            public static void Init()
            {
                Console.WriteLine("Loading MonsterDrop..");

                #region Drop

                string Files = KTGlobal.GetDataPath("Config/KT_Drop/DropConfig.xml");
                using (var stream = System.IO.File.OpenRead(Files))
                {
                    var serializer = new XmlSerializer(typeof(DropConfig));
                    _DropConfig = serializer.Deserialize(stream) as DropConfig;
                }

                #endregion Drop

                #region Pick up

                /// Dữ liệu XML
                XElement pickUpNotifyNode = KTGlobal.ReadXMLData("Config/KT_Drop/PickUpItemNotify.xml");
                /// Duyệt danh sách
                foreach (XElement node in pickUpNotifyNode.Elements("Item"))
                {
                    /// Thông tin
                    PickUpItemNotify data = PickUpItemNotify.Parse(node);
                    /// Thêm vào danh sách
                    MonsterDropManager.pickUpItemNotifications[data.ItemID] = data;
                }

                #endregion Pick up
            }

            #endregion Init

            #region Event

            /// <summary>
            /// Xử lý khi người chơi nhặt vật phẩm rơi dưới đất
            /// </summary>
            /// <param name="player"></param>
            /// <param name="itemGD"></param>
            /// <param name="source"></param>
            public static void ProcessPickUpItem(KPlayer player, GoodsData itemGD, GameObject source)
            {
                /// Toác
                if (player == null || itemGD == null)
                {
                    /// Bỏ qua
                    return;
                }

                /// Thông tin rơi
                if (!MonsterDropManager.pickUpItemNotifications.TryGetValue(itemGD.GoodsID, out PickUpItemNotify data))
                {
                    /// Toác thì bỏ qua
                    return;
                }

                /// Nếu thiết lập chỉ thông báo khi rơi ra từ Boss
                if (data.BossOnly)
                {
                    /// Nếu không có nguồn rơi
                    if (source == null)
                    {
                        /// Bỏ qua
                        return;
                    }

                    /// Nếu nguồn rơi không phải quái
                    if (!(source is Monster monster))
                    {
                        /// Bỏ qua
                        return;
                    }

                    /// Nếu không phải Boss
                    if (monster.MonsterType != Entities.MonsterAIType.Pirate && monster.MonsterType != Entities.MonsterAIType.Elite && monster.MonsterType != Entities.MonsterAIType.Leader && monster.MonsterType != Entities.MonsterAIType.Boss && monster.MonsterType != Entities.MonsterAIType.Special_Boss)
                    {
                        /// Bỏ qua
                        return;
                    }
                }

                /// Thông tin vật phẩm
                string itemInfoString = KTGlobal.GetItemDescInfoStringForChat(itemGD);
                /// Bản đồ đang đứng
                GameMap map = KTMapManager.Find(player.CurrentMapCode);
                /// Chuỗi thông báo
                string notificationString;

                /// Nếu có nguồn rơi
                if (source != null)
                {
                    notificationString = string.Format("<color=#60bae6>[{0}]</color> đã nhặt được {1} từ <color=yellow>[{2}]</color> tại <color=green>{3}</color>.", player.RoleName, itemInfoString, source.RoleName, map.MapName);
                }
                /// Nếu không có nguồn rơi
                else
                {
                    notificationString = string.Format("<color=#60bae6>[{0}]</color> đã nhặt được {1} tại <color=green>{2}</color>.", player.RoleName, itemInfoString, map.MapName);
                }

                /// Thông báo lên kênh đội
                KTGlobal.SendTeamChat(player, notificationString, new List<GoodsData>() { itemGD });
                /// Thông báo lên kênh bang
                KTGlobal.SendGuildChat(player.GuildID, notificationString, player, null, new List<GoodsData>() { itemGD });
            }

            /// <summary>
            /// Xử lý quái chết rơi vật phẩm
            /// </summary>
            /// <param name="monster"></param>
            /// <param name="player"></param>
            public static void ProcessMonsterDrop(Monster _Monster, KPlayer Player)
            {
                // Lấy ra mapcode hiện tại
                int MapCode = _Monster.CurrentMapCode;

                //Tìm xem profile riêng lẻ của quái hay của bản đồ
                string DropProfile = "";

                // Nếu monster này có rơi đồ sẵn trong config
                if (_Monster.MonsterInfo.DropProfile != "")
                {
                    DropProfile = _Monster.MonsterInfo.DropProfile;
                }
                else // Nếu đéo rơi thì check xem map có rơi không
                {
                    GameMap _Map = KTMapManager.Find(MapCode);
                    if (_Map != null)
                    {
                        if ((int)_Monster.MonsterType == 0)
                        {
                            DropProfile = _Map.NormalDropRate;
                        }
                        else
                        {
                            DropProfile = _Map.GoldenDropRate;
                        }
                    }
                }

                // Nếu bản đồ này ko rơi gì và con này không thiết lập cho rơi thì thôi
                if (DropProfile == "")
                {
                    return;
                }

                // Neeus
                if (_Monster.MonsterType == Entities.MonsterAIType.Elite)
                {
                 
                    DropProfile += "_elite";

                    LogManager.WriteLog(LogTypes.PickUpNotify, "FIND DROP FIND ITEM :" + DropProfile);
                }

                // Tìm bảng dorp trong bảng thiết lập
                var FindDropProfile = _DropConfig.DropProfile.Where(x => x.ProfileName == DropProfile).FirstOrDefault();
                // Nếu mà không tìm thấy drop nào thì thôi đéo rơi j nữa
                if (FindDropProfile == null)
                {
                    return;
                }

                // Nếu con quái này được thiết lập rơi ở phụ bản mà con quái lại đéo ở phụ bản thì thôi
                if (FindDropProfile.CopySceneOnly)
                {
                    if (_Monster.CurrentCopyMapID == -1)
                    {
                        return;
                    }
                }

                // Nếu mà không chưa cái map này thì bỏ
                if (FindDropProfile.MapsID.Length > 0)
                {
                    string[] Drops = FindDropProfile.MapsID.Split(';');

                    // Nêu như files drop này ko chứa cái map này thì bỏ
                    if (!Drops.Contains(_Monster.MapCode + ""))
                    {
                        return;
                    }
                }

                int TotalDropCount = _Monster.MonsterInfo.Treasure;

                if (_Monster.MonsterType == Entities.MonsterAIType.Elite)
                {
                    TotalDropCount = FindDropProfile.Count;
                }

                // Duyệt tất cả các bọc có thể rơi
                for (int i = 0; i < TotalDropCount; i++)
                {
                    // Tạo biến đánh dấu xem đã dùng chưa
                    List<MagicAttribLevel> Mark = new List<MagicAttribLevel>();
                    // Cho chạy random xem có rơi tiền không
                    if (KTGlobal.GetRandomNumber(0, 100) < FindDropProfile.MoneyRate)
                    {  // Nếu mà rơi tiền thì cho rơi tiền thực hiện tính toán và add tiền cho người chơi thẳng vào túi
                       //Số tiền sẽ rơi nGetExp = (int)(nGetExp * ExpMutipleEvent.GetRate() * ServerConfig.Instance.ExpRate);

                        int BindMoney = (int)(KTGlobal.GetRandomNumber(FindDropProfile.MinMoney, FindDropProfile.MaxMoney) * ExpMutipleEvent.GetMoneyRate() * ServerConfig.Instance.MoneyRate);
                        // Nếu mà rơi tiền thì cho rơi tiền thực hiện tính toán và add tiền cho người chơi thẳng vào túi
                        KTPlayerManager.AddBoundMoney(Player, BindMoney, "MONSTERDROP | " + _Monster.MonsterInfo.Name + "");
                    }
                    else
                    {
                        // Nếu như này thì đóe rơi
                        if (KTGlobal.GetRandomNumber(0, 100) > FindDropProfile.ItemRate)
                        {
                            return;
                        }

                        //nếu như min level và max level toác thì bỏ qua
                        if (FindDropProfile.MinItemLevel <= 0 || FindDropProfile.MaxItemLevel <= 0)
                        {
                            return;
                        }

                        long nRand = KTGlobal.GetRandomNumber(0, FindDropProfile.RandRange);
                        int nCheckRand = 0;

                        int j = 0;

                        for (j = 0; j < FindDropProfile.Drop.Count(); j++)
                        {
                            if (nRand >= nCheckRand && nRand < nCheckRand + FindDropProfile.Drop[j].RandRate)
                            {
                                break;
                            }
                            // + Dần cào RANDIAN
                            nCheckRand += FindDropProfile.Drop[j].RandRate;
                        }

                        if (j == FindDropProfile.Drop.Count())
                        {
                            return;
                        }

                        int nLuck = Player.m_nCurLucky;

                        if (nLuck > 150)
                        {
                            nLuck = 150;
                        }

                        int nDecide = (1 + nLuck * 10 / 100);

                        int RandLucky = KTGlobal.GetRandomNumber(0, nDecide);

                        bool bSkip = false;

                        int DIV = FindDropProfile.MagicRate / 6;

                        int FinalRate = FindDropProfile.MagicRate;

                        // LogManager.WriteLog(LogTypes.Item, "LUCKEY RANDOM ADD :" + RandLucky);
                        // Làm 1 mảng lưu lại level của 6 dòng sẽ random được nếu mà ko random được thì set nó =0
                        int[] pnMagicLevel = new int[6];
                        //DO SOMETHING HERE CODE SELECT 6 LINE
                        for (int k = 0; k < 6; k++)
                        {
                            if (!bSkip)
                            {
                                if (KTGlobal.GetRandomNumber(0, 100) < (FinalRate + nDecide))
                                {
                                    int nLevel = KTGlobal.GetRandomNumber(FindDropProfile.MinItemLevel, FindDropProfile.MaxItemLevel);

                                    pnMagicLevel[k] = (int)nLevel;

                                    FinalRate -= DIV;
                                }
                                else
                                {
                                    pnMagicLevel[k] = 0;
                                    bSkip = true;
                                }
                            }
                            else
                            {
                                pnMagicLevel[k] = 0;
                            }
                        }

                        var TempateItem = ItemManager.GetItemTemplate(FindDropProfile.Drop[j].ItemID);

                        int nGenre = TempateItem.Genre;
                        int nDetail = TempateItem.DetailType;

                        // Nếu ko tìm thấy vật phẩm này trong config DROP
                        if (TempateItem == null)
                        {
                            return;
                        }
                        else
                        {
                            int nSeries = ItemManager.GetItemSeries(TempateItem.Series);
                            MagicAttribLevel[] pMagicAttrTable = { null, null, null, null, null, null };

                            int RandomPhycical = KTGlobal.GetRandomNumber(0, 100);

                            bool IsPhysicalItem = false;

                            if (RandomPhycical < 50)
                            {
                                // LogManager.WriteLog(LogTypes.Item, TempateItem.Name + "======> PHẬT PHẨM NÀY LÀ ĐỒ NGOẠI ");
                                IsPhysicalItem = true;
                            }
                            else
                            {
                                // LogManager.WriteLog(LogTypes.Item, TempateItem.Name + "======> PHẬT PHẨM NÀY LÀ ĐỒ NỘI ");
                            }

                            // Nếu đây là đồ ngoại

                            bool IsHiddenLines = false;
                            if (ItemManager.KD_ISEQUIP(nGenre) || ItemManager.KD_ISPETEQUIP(nGenre))
                            {
                                for (int g = 0; g < 6; g++)
                                {
                                    if (pnMagicLevel[g] == 0)
                                        break;

                                    // Nếu đéo chia hết cho 2 nó là dòng lẻ
                                    if (g % 2 != 0)
                                    {
                                        IsHiddenLines = true;
                                    }
                                    else
                                    {
                                        IsHiddenLines = false;
                                    }
                                    // Nếu mà = 0 thì tức là ko có dòng nào khác nữa

                                    int Suffix = 0;

                                    // Nếu là dòng ẩn mà là dòng ngoại
                                    if (IsHiddenLines && IsPhysicalItem)
                                    {
                                        Suffix = 4;
                                    }
                                    else if (!IsHiddenLines && IsPhysicalItem)
                                    {
                                        Suffix = 3;
                                    }
                                    else if (IsHiddenLines && !IsPhysicalItem)
                                    {
                                        Suffix = 6;
                                    }
                                    else if (!IsHiddenLines && !IsPhysicalItem)
                                    {
                                        Suffix = 5;
                                    }

                                    int SuffixEx = 1;

                                    if (IsHiddenLines)
                                    {
                                        SuffixEx = 2;
                                    }

                                    List<MagicAttribLevel> GetAllCorrectInList = new List<MagicAttribLevel>();

                                    GetAllCorrectInList = ItemManager.TotalMagicAttribLevel.Where(x => x.RATE(nDetail, nGenre) > 0 && (x.Suffix == Suffix || x.Suffix == SuffixEx) && (x.Series == nSeries || x.Series == -1) && x.Level == pnMagicLevel[g]).ToList();

                                    foreach (MagicAttribLevel _NeedRemoveBeforeSelect in Mark)
                                    {
                                        GetAllCorrectInList.RemoveAll(x => x.MAGIC_ID == _NeedRemoveBeforeSelect.MAGIC_ID);
                                    }
                                    // Nếu ko tìm thấy thuộc tính nào thì break
                                    if (GetAllCorrectInList.Count == 0)
                                    {
                                        break;
                                    }

                                    //if (IsHiddenLines)
                                    //{
                                    //    LogManager.WriteLog(LogTypes.Item, "[" + g + "]CHỌN DÒNG ẨN CHO  :" + TempateItem.Name + "sử dụng Suffix :" + Suffix + " hoặc SuffixEx :" + SuffixEx);
                                    //}
                                    //else
                                    //{
                                    //    LogManager.WriteLog(LogTypes.Item, "[" + g + "]CHỌN DÒNG HIỆN CHO  :" + TempateItem.Name + "sử dụng Suffix :" + Suffix + "hoặc SuffixEx :" + SuffixEx);
                                    //}

                                    long TotalSumDrop = GetAllCorrectInList.Sum(x => x.RATE(nDetail, nGenre));

                                    long RanndomValue = KTGlobal.GetRandomNumber(0, TotalSumDrop);

                                    //foreach (MagicAttribLevel _TmpMagicAttribLevel in GetAllCorrectInList)
                                    //{
                                    //    LogManager.WriteLog(LogTypes.Item, "[" + g + "]MAGICNAME :" + _TmpMagicAttribLevel.MagicName + "| RATE :" + _TmpMagicAttribLevel.RATE(nDetail, nGenre) + " Suffix :" + _TmpMagicAttribLevel.Suffix);
                                    //}

                                    long SelectValue = 0;

                                    bool SelectOK = false;

                                    foreach (MagicAttribLevel _TmpMagicAttribLevel in GetAllCorrectInList)
                                    {
                                        if (_TmpMagicAttribLevel == null)
                                        {
                                            continue;
                                        }

                                        var FindDupper = Mark.Where(x => x.MAGIC_ID == _TmpMagicAttribLevel.MAGIC_ID).FirstOrDefault();
                                        if (FindDupper != null)
                                        {
                                            continue;
                                        }
                                        // Nếu dòng này đã chọn rồi thì bỏ qua

                                        SelectValue = SelectValue + _TmpMagicAttribLevel.RATE(nDetail, nGenre);

                                        if (SelectValue >= RanndomValue)
                                        {
                                            //if (IsHiddenLines)
                                            //{
                                            //    LogManager.WriteLog(LogTypes.Item, "[" + g + "]CHỌN DÒNG ẨN ===>  :" + _TmpMagicAttribLevel.MagicName);
                                            //}
                                            //else
                                            //{
                                            //    LogManager.WriteLog(LogTypes.Item, "[" + g + "]CHỌN DÒNG HIỆN===>  :" + _TmpMagicAttribLevel.MagicName);
                                            //}

                                            SelectOK = true;
                                            Mark.Add(_TmpMagicAttribLevel);
                                            pMagicAttrTable[g] = _TmpMagicAttribLevel;
                                            break;
                                        }
                                    }

                                    if (!SelectOK)
                                    {
                                        // LogManager.WriteLog(LogTypes.Item, "[" + g + "]KHÔNG CHỌN ĐƯỢC DÒNG NÀO TRONG VÒNG RANDOM NÀY");
                                    }
                                }
                            }
                            //CODE GEN RA PROBS của trang bị
                            string ItemProbsGeneter = ItemManager.GenerateProbsWithRandomLine(TempateItem, pMagicAttrTable);

                            if (TempateItem.Genre == 3)
                            {
                                ItemProbsGeneter = "";
                                nSeries = -1;
                            }
                            // Sau khi random xong thì ta được bảng pMagicAttrTable cuối cùng đây là bảng sẽ truyền sang ItemManger dể gen ra PROBS ITEM lưu vào DB
                            // Nếu có vật phẩm thực hiện random ra dòng

                            // Tạo 1 vật phẩm tùy chỉnh từ PROBS vừa gen được
                            GoodsData _TmpData = CreateGoodDataFromItemDataWithProbGen(TempateItem, ItemProbsGeneter, nSeries);

                            bool DropFromBoss = false;

                            if ((int)_Monster.MonsterType == 4)
                            {
                                DropFromBoss = true;
                            }

                            if (_TmpData == null)
                            {
                                return;
                            }
                            //Tạo ra drop map khi rơi
                            KTGoodsPackManager.CreateDropToMapWhenMonsterDie(_Monster, _TmpData, Player);
                        }
                    }
                }
            }

            #endregion Event

            #region Private methods

            private static GoodsData CreateGoodDataFromItemDataWithProbGen(ItemData TmpItem, string Probs, int ItemSeris)
            {
                GoodsData _GoodData = new GoodsData();
                _GoodData.Id = -1;
                _GoodData.GoodsID = TmpItem.ItemID;
                _GoodData.GCount = 1;
                _GoodData.Props = Probs;
                _GoodData.Series = ItemSeris;
                _GoodData.Site = 0;
                _GoodData.Strong = 100;
                _GoodData.Using = -1;
                _GoodData.BagIndex = -1;
                // Vì là rơi từ quái ra nên không cố định
                _GoodData.Binding = 0;
                _GoodData.Forge_level = 0;

                return _GoodData;
            }

            /// <summary>
            /// Trả về tỷ lệ ngẫu nhiên với độ may mắn của người chơi tương ứng
            /// </summary>
            /// <returns></returns>
            private static int GetRandomRate(KPlayer player)
            {
                /// Giá trị may mắn của người chơi
                int nLuck = player.m_nCurLucky;
                /// Quá giới hạn
                if (nLuck > 150)
                {
                    /// Thiết lập lại
                    nLuck = 150;
                }
                /// Tỷ lệ may mắn thêm vào
                int nLuckyAdd = nLuck * 100;

                /// Kết quả
                int result = KTGlobal.GetRandomNumber(1, 1000000) - nLuckyAdd;
                /// Nếu âm
                if (result < 0)
                {
                    /// Thiết lập lại
                    result = 0;
                }

                /// Trả về kết quả
                return result;
            }

            #endregion Private methods
        }
    }
}