using GameServer.KiemThe.Entities;
using GameServer.KiemThe.Logic;
using GameServer.KiemThe.Utilities;
using GameServer.KiemThe.Utilities.Algorithms;
using GameServer.Logic;
using GameServer.Server;
using Server.Data;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Xml.Linq;
using UnityEngine;
using static GameServer.KiemThe.Logic.KTTaskManager;

namespace GameServer.KiemThe
{
    /// <summary>
    /// Các phương thức và đối tượng toàn cục của Kiếm Thế
    /// </summary>
    public static class KTGlobal
    {
        #region Config

        /// <summary>
        /// Tổng số EXP max sẽ share cho bọn đồng đội
        /// </summary>
        public const int KD_MAX_TEAMATE_EXP_SHARE = 60;
        /// <summary>
        /// Nửa chiều rộng phạm vi lưới quét đối tượng xung quanh
        /// </summary>
        public const int RadarHalfWidth = 30;

        /// <summary>
        /// Nửa chiều cao phạm vi lưới quét đối tượng xung quanh
        /// </summary>
        public const int RadarHalfHeight = 25;

        /// <summary>
        /// Thời gian quay đến hướng gần nhất
        /// </summary>
        public const float DefaultTurnTick = 0.05f;

        /// <summary>
        /// Khoảng cách lệch tối đa cho phép Client và Server
        /// <para>Tính cả Delay packet, etc...</para>
        /// </summary>
        public const float MaxClientServerMoveDistance = 100f;

        /// <summary>
        /// Thời gian Delay packet gửi từ Client về Server chấp nhận được
        /// </summary>
        public const long MaxClientPacketDelayAllowed = 500;

        /// <summary>
        /// Vật phẩm Cửu Chuyển Tục Mệnh Hoàn
        /// </summary>
        public static readonly List<int> ReviveMedicine = new List<int>()
        {
            223, 538,
        };
        #endregion Config

        #region Tốc chạy và tốc đánh

        #region Tốc chạy
        /// <summary>
        /// Chuyển tốc độ di chuyển sang dạng lưới Pixel
        /// </summary>
        /// <param name="moveSpeed"></param>
        /// <returns></returns>
        public static int MoveSpeedToPixel(int moveSpeed)
        {
            return moveSpeed * 15;
        }
        #endregion

        #region Tốc đánh
        /// <summary>
        /// Kiểm tra đối tượng đã kết thúc thực thi động tác xuất chiêu chưa
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="attackSpeed"></param>
        /// <returns></returns>
        public static bool FinishedUseSkillAction(GameServer.Logic.GameObject obj, int attackSpeed)
        {
            /// Tổng thời gian 
            float frameDuration = KTGlobal.AttackSpeedToFrameDuration(attackSpeed);

            /// Trả ra kết quả
            return KTGlobal.GetCurrentTimeMilis() - obj.LastAttackTicks >= frameDuration * 1000 + KTGlobal.AttackSpeedAdditionDuration * 1000;
        }

        /// <summary>
        /// Thời gian thực hiện động tác xuất chiêu tối thiểu
        /// </summary>
        public const float MinAttackActionDuration = 0.2f;

        /// <summary>
        /// Thời gian thực hiện động tác xuất chiêu tối đa
        /// </summary>
        public const float MaxAttackActionDuration = 0.8f;

        /// <summary>
        /// Thời gian cộng thêm giãn cách giữa các lần ra chiêu
        /// </summary>
        public const float AttackSpeedAdditionDuration = 0.1f;

        /// <summary>
        /// Tốc đánh tối thiểu
        /// </summary>
        public const int MinAttackSpeed = 0;

        /// <summary>
        /// Tốc đánh tối đa
        /// </summary>
        public const int MaxAttackSpeed = 100;

        /// <summary>
        /// Chuyển tốc độ đánh sang thời gian thực hiện động tác xuất chiêu
        /// </summary>
        /// <param name="attackSpeed"></param>
        /// <returns></returns>
        public static float AttackSpeedToFrameDuration(int attackSpeed)
        {
            /// Nếu tốc đánh nhỏ hơn tốc tối thiểu
            if (attackSpeed < KTGlobal.MinAttackSpeed)
            {
                attackSpeed = KTGlobal.MinAttackSpeed;
            }
            /// Nếu tốc đánh vượt quá tốc tối đa
            if (attackSpeed > KTGlobal.MaxAttackSpeed)
            {
                attackSpeed = KTGlobal.MaxAttackSpeed;
            }

            /// Tỷ lệ % so với tốc đánh tối đa
            float percent = attackSpeed / (float) KTGlobal.MaxAttackSpeed;

            /// Thời gian thực hiện động tác xuất chiêu
            float animationDuration = KTGlobal.MinAttackActionDuration + (KTGlobal.MaxAttackActionDuration - KTGlobal.MinAttackActionDuration) * (1f - percent);

            /// Trả về kết quả
            return animationDuration;
        }
        #endregion
        #endregion

        #region Position
        /// <summary>
        /// Số lần thử tối đa tìm điểm ngẫu nhiên xung quanh không chứa vật cản mà có thể trực tiếp đi tới được theo đường thẳng
        /// </summary>
        private const int TryGetRandomLinearNoObsPointMaxTimes = 10;

        /// <summary>
        /// Chuyển từ tọa độ thực sang tọa độ lưới
        /// </summary>
        /// <param name="map"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        public static Vector2 WorldPositionToGridPosition(GameMap map, Vector2 position)
        {
            return new Vector2(position.x / map.MapGridWidth, position.y / map.MapGridHeight);
        }

        /// <summary>
        /// Chuyển từ tọa độ lưới sang tọa độ thực
        /// </summary>
        /// <param name="map"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        public static Vector2 GridPositionToWorldPosition(GameMap map, Vector2 position)
        {
            return new Vector2(position.x * map.MapGridWidth, position.y * map.MapGridHeight);
        }

        /// <summary>
        /// Tìm 1 điểm nằm trên đường đi không chứa vật cản
        /// </summary>
        /// <param name="map"></param>
        /// <param name="fromPos"></param>
        /// <param name="toPos"></param>
        /// <returns></returns>
        public static Vector2 FindLinearNoObsPoint(GameMap map, Vector2 fromPos, Vector2 toPos)
        {
            /// Vị trí hiện tại theo tọa độ lưới
            Vector2 fromGridPos = KTGlobal.WorldPositionToGridPosition(map, fromPos);
            /// Vị trí đích đến theo tọa độ lưới
            Vector2 toGridPos = KTGlobal.WorldPositionToGridPosition(map, toPos);

            Point fromGridPOINT = new Point((int) fromGridPos.x, (int) fromGridPos.y);
            Point toGridPOINT = new Point((int) toGridPos.x, (int) toGridPos.y);

            /// Nếu 2 vị trí trùng nhau
            if (fromGridPOINT == toGridPOINT)
            {
                return fromPos;
            }

            /// Nếu tìm thấy điểm không chứa vật cản
            if (Global.FindLinearNoObsPoint(map, fromGridPOINT, toGridPOINT, out Point newNoObsPoint))
            {
                Vector2 newNoObsPos = new Vector2((int) newNoObsPoint.X, (int) newNoObsPoint.Y);
                /// Trả ra kết quả
                return KTGlobal.GridPositionToWorldPosition(map, newNoObsPos);
            }
            /// Trả ra kết quả nếu không tìm thấy vị trí thỏa mãn
            return fromPos;
        }

        /// <summary>
        /// Trả về vị trí ngãu nhiên xung quanh không chứa vật cản có thể di chuyển trực tiếp đến được theo đường thẳng
        /// </summary>
        /// <param name="map"></param>
        /// <param name="pos"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        public static Vector2 GetRandomLinearNoObsPoint(GameMap map, Vector2 pos, float distance)
        {
            int triedTime = 0;
            do
            {
                triedTime++;

                //Vector2 randPos = KTMath.GetRandomPointInCircle(pos, distance);
                Vector2 randPos = KTMath.GetRandomPointAroundPos(pos, distance);

                /*
                Vector2 randGridPos = KTGlobal.WorldPositionToGridPosition(map, randPos);
                Point randGridPOINT = new Point((int) randGridPos.x, (int) randGridPos.y);

                /// Nếu vị trí có thể đến được
                if (map.MyNodeGrid.isWalkable((int) randGridPOINT.X, (int) randGridPOINT.Y))
                {
                    return randPos;
                }
                */

                Vector2 noObsPos = KTGlobal.FindLinearNoObsPoint(map, pos, randPos);
                if (noObsPos != pos)
                {
                    return noObsPos;
                }
            }
            while (triedTime <= KTGlobal.TryGetRandomLinearNoObsPointMaxTimes);

            return pos;
        }
        #endregion Position

        #region Path Finder

        /// <summary>
        /// Tìm đường sử dụng giải thuật A*
        /// </summary>
        /// <param name="fromPos">Tọa độ thực điểm bắt đầu</param>
        /// <param name="toPos">Tọa độ thực điểm kết thúc</param>
        /// <returns></returns>
        private static List<Vector2> FindPathUsingAStar(int mapCode, Point fromPos, Point toPos)
        {
            GameMap gameMap = GameManager.MapMgr.DictMaps[mapCode];
            if (null == gameMap)
            {
                return new List<Vector2>();
            }

            /// Tìm đường sử dụng giải thuật A*
            List<int[]> nodeList = GlobalNew.FindPath(fromPos, toPos, mapCode);
            if (nodeList == null)
            {
                return new List<Vector2>();
            }
            nodeList.Reverse();

            List<Vector2> path = new List<Vector2>();

            for (int i = 0; i < nodeList.Count; i++)
            {
                path.Add(new Vector2(nodeList[i][0], nodeList[i][1]));
            }

            /// Làm mịn
            path = LineSmoother.SmoothPath(path, gameMap.MyNodeGrid.GetFixedObstruction());

            return path;
        }

        /// <summary>
        /// Tìm đường giữa 2 vị trí
        /// <para>Sử dụng giải thuật A* để tìm đường ngắn nhất</para>
        /// <para>Sử dụng thuật toán Ramer–Douglas–Peucker để làm mịn đường đi</para>
        /// </summary>
        /// <param name="go">Đối tượng</param>
        /// <param name="fromPos">Tọa độ thực vị trí bắt đầu</param>
        /// <param name="toPos">Tọa độ thực vị trí kết thúc</param>
        public static List<Vector2> FindPath(GameServer.Logic.GameObject go, Vector2 fromPos, Vector2 toPos)
        {
            GameMap gameMap = GameManager.MapMgr.DictMaps[go.CurrentMapCode];
            if (null == gameMap)
            {
                return new List<Vector2>();
            }

            Vector2 fromGridPos = KTGlobal.WorldPositionToGridPosition(gameMap, fromPos);
            Vector2 toGridPos = KTGlobal.WorldPositionToGridPosition(gameMap, toPos);

            Point fromGridPOINT = new Point((int)fromGridPos.x, (int)fromGridPos.y);
            Point toGridPOINT = new Point((int)toGridPos.x, (int)toGridPos.y);

            /// Nếu vị trí đầu và cuối cùng một ô lưới thì cho chạy giữa 2 vị trí này luôn
            if (fromGridPOINT == toGridPOINT)
            {
                return new List<Vector2>()
                {
                    fromPos, toPos
                };
            }

            Point fromPosPOINT = new Point((int)fromPos.x, (int)fromPos.y);
            Point toPosPOINT = new Point((int)toPos.x, (int)toPos.y);

            /// Sử dụng A* tìm đường đi
            List<Vector2> nodes = KTGlobal.FindPathUsingAStar(go.CurrentMapCode, fromPosPOINT, toPosPOINT);

            /// Nếu danh sách nút tìm được nhỏ hơn 2
            if (nodes.Count < 2)
            {
                return new List<Vector2>();
            }

            /// Danh sách điểm trên đường đi
            List<Vector2> result = new List<Vector2>();

            /*
            /// Nếu khoảng cách từ vị trí bắt đầu đến điểm thứ 2 trong danh sách đường đi tìm được lớn hơn khoảng cách từ điểm đầu tiên đến điểm thứ 2 thì thêm vị trí bắt đầu vào đầu danh sách
            if (Vector2.Distance(fromPos, KTGlobal.GridPositionToWorldPosition(gameMap, nodes[1])) > Vector2.Distance(KTGlobal.GridPositionToWorldPosition(gameMap, nodes[0]), KTGlobal.GridPositionToWorldPosition(gameMap, nodes[1])))
            {
                result.Add(fromPos);
                result.Add(KTGlobal.GridPositionToWorldPosition(gameMap, nodes[0]));
            }
            /// Nếu vị trí bắt đầu khác điểm đầu tiên tìm được trong danh sách đường đi thì thay thế vị trí bắt đầu vào điểm đầu tiên
            else if (KTGlobal.GridPositionToWorldPosition(gameMap, nodes[0]) != fromPos)
            */
            {
                result.Add(fromPos);
            }

            /// Thêm tất cả các nút tìm được trên đường đi vào danh sách
            for (int i = 1; i < nodes.Count; i++)
            {
                result.Add(KTGlobal.GridPositionToWorldPosition(gameMap, nodes[i]));
            }

            /*
            /// Nếu khoảng cách từ điểm gần cuối danh sách đến vị trí đích nhỏ hơn khoảng cách từ điểm gần cuối danh sách đến điểm cuối danh sách thì thay thế vị trí cuối danh sách bằng vị trí đích
            if (Vector2.Distance(KTGlobal.GridPositionToWorldPosition(gameMap, nodes[nodes.Count - 2]), toPos) < Vector2.Distance(KTGlobal.GridPositionToWorldPosition(gameMap, nodes[nodes.Count - 2]), KTGlobal.GridPositionToWorldPosition(gameMap, nodes[nodes.Count - 1])))
            */
            {
                result[result.Count - 1] = toPos;
            }
            /*
            /// Nếu vị trí đích khác vị trí hiện tại
            else if (KTGlobal.GridPositionToWorldPosition(gameMap, nodes[nodes.Count - 1]) != toPos)
            {
                result.Add(toPos);
            }
            */

            return result;
        }

        #endregion Path Finder

        #region PropertyDictionary

        /// <summary>
        /// Đọc dữ liệu PropertyDictionary
        /// </summary>
        public static void ReadPropertyDictionary()
        {
            XElement xmlNode = Global.GetGameResXml("Config/PropertyDictionary.xml");
            PropertyDefine.Parse(xmlNode);
        }

        #endregion PropertyDictionary

        #region PKRegular 

        // TODO SAU CHUYỂN HẾT THÀNH CONFIG

        public static int PKDamageRate = 25;

        public static int NpcPKDamageRate = 750;

        public static int EnmityExpLimitPercent = -50;

        public static int FightExpLimitPercent = -50;

        public static int PKStateChangeLimitTime = 180;

        #endregion PKRegular

        #region SkillSettings

        public static int[] m_arPhysicsEnhance = new int[(int)KE_EQUIP_WEAPON_CATEGORY.emKEQUIP_WEAPON_CATEGORY_NUM];

        public static int HitPercentMin = 5;// Tỉ lệ chính xác nhỏ nhất

        public static int HitPercentMax = 95; // TỈ lệ chính xác tối đa

        public static int StateBaseRateParam = 250; // tỉ lệ dính phải trạng thái base
        public static int StateBaseTimeParam = 250; // Thời gian tồn tại trạng thái base

        public static int DeadlyStrikeBaseRate = 1900; // tỉ lệ dính phải trạng thái base
        public static int DeadlyStrikeDamagePercent = 180; // Thời gian tồn tại trạng thái base

        public static int DefenceMaxPercent = 85; // Bor qua kháng max

        public static int IngoreResistMaxP = 95; // Bor qua kháng max

        public static int SeriesTrimMax = 95;

        public static int FatallyStrikePercent = 50; // Chí mạng Apply Dame tối đa

        public static int SeriesTrimParam1 = 1;

        public static int SeriesTrimParam2 = 8;
        public static int SeriesTrimParam3 = 200;

        public static int SeriesTrimParam4 = 0; // Cường hóa sát thương của thằng đánh
        public static int SeriesTrimParam5 = 10;

        public static int WeakDamagePercent = 50; // Tỉ lệ thiệt hại khi bị suy yếu
        public static int SlowAllPercent = 50; // Tốc dộ sẽ làm chậm
        public static int BurnDamagePercent = 150; // Dame bị bỏng tính thêm

        public static int AngerTime = 10; // Thời gian nộ tính bằng giây


        public static int DropRate = 1; // DROPRATE TỪ 1-10 tương ứng với từ 10->100%


        public static void NotifyGoood(KPlayer _Play,int BangIndex,int Count,int IsUsing, ModGoodsTypes TypeMod, int GoodID,int Hit,int Site,int State)
        {
            SCModGoods scData = new SCModGoods()
            {
                BagIndex = BangIndex,
                Count = Count,
                IsUsing = IsUsing,
                ModType = (int)TypeMod,
                ID = GoodID,
                NewHint = Hit,
                Site = Site,
                State = State,
            };
            // Gửi thông báo về là tháo đồ ra thành công
            _Play.sendCmd((int)TCPGameServerCmds.CMD_SPR_MOD_GOODS, scData);
        }

        public static void ModifyGoods(KPlayer client, int DBID, Dictionary<UPDATEITEM, object> ModifyDict)
        {
            if (null == client.GoodsDataList)
                return;

            lock (client.GoodsDataList)
            {
                for (int i = 0; i < client.GoodsDataList.Count; i++)
                {
                    if (client.GoodsDataList[i].Id == DBID)
                    {
                        if (ModifyDict.ContainsKey(UPDATEITEM.BAGINDEX))
                        {
                            client.GoodsDataList[i].BagIndex = (int)ModifyDict[UPDATEITEM.BAGINDEX];
                        }

                        if (ModifyDict.ContainsKey(UPDATEITEM.END_TIME))
                        {
                            client.GoodsDataList[i].Endtime = (string)ModifyDict[UPDATEITEM.END_TIME];
                        }

                        if (ModifyDict.ContainsKey(UPDATEITEM.OTHER_PRAM))
                        {
                            client.GoodsDataList[i].OtherParams = (Dictionary<ItemPramenter, int>)ModifyDict[UPDATEITEM.OTHER_PRAM];
                        }

                        if (ModifyDict.ContainsKey(UPDATEITEM.PROPS))
                        {
                            client.GoodsDataList[i].Props = (string)ModifyDict[UPDATEITEM.PROPS];
                        }

                        if (ModifyDict.ContainsKey(UPDATEITEM.STRONG))
                        {
                            client.GoodsDataList[i].Strong = (int)ModifyDict[UPDATEITEM.STRONG];
                        }

                        if (ModifyDict.ContainsKey(UPDATEITEM.BINDING))
                        {
                            client.GoodsDataList[i].Binding = (int)ModifyDict[UPDATEITEM.BINDING];
                        }

                        if (ModifyDict.ContainsKey(UPDATEITEM.FORGE_LEVEL))
                        {
                            client.GoodsDataList[i].Forge_level = (int)ModifyDict[UPDATEITEM.FORGE_LEVEL];
                        }

                        if (ModifyDict.ContainsKey(UPDATEITEM.GCOUNT))
                        {
                            client.GoodsDataList[i].GCount = (int)ModifyDict[UPDATEITEM.GCOUNT];
                        }

                        if (ModifyDict.ContainsKey(UPDATEITEM.USING))
                        {
                            client.GoodsDataList[i].Using = (int)ModifyDict[UPDATEITEM.USING];
                        }

                        break;
                    }
                }
            }

            return;
        }

        public static string ItemUpdateScriptBuild(Dictionary<UPDATEITEM, object> Input)
        {
            string OutPut = "";

            for (int i = 0; i < 13; i++)
            {
                UPDATEITEM _Item = (UPDATEITEM)i;

                if (Input.ContainsKey(_Item))
                {

                    if(Input[_Item].GetType() == typeof(string))
                    {
                        string Value = (string)Input[_Item];

                        OutPut += Value + ":";
                    }
                    else if (Input[_Item].GetType() == typeof(int))
                    {
                        int Value = (int)Input[_Item];

                        OutPut += Value + ":";
                    }    

                }
                else
                {
                    OutPut += "*:";
                }
            }

            return OutPut;
        }

        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        public static String SexToString(int Sex)
        {
            if (Sex == 0)
                return "Nam";
            else
                return "Nữ";
        }

        public static string GetSeriesText(int seriesID)
        {
            switch (seriesID)
            {
                case 1:
                    return string.Format("<color={0}>{1}</color>", "#ffe552", "Kim");

                case 2:
                    return string.Format("<color={0}>{1}</color>", "#77ff33", "Mộc");

                case 3:
                    return string.Format("<color={0}>{1}</color>", "#61d7ff", "Thủy");

                case 4:
                    return string.Format("<color={0}>{1}</color>", "#ff4242", "Hỏa");

                case 5:
                    return string.Format("<color={0}>{1}</color>", "#debba0", "Thổ");

                default:
                    return string.Format("<color={0}>{1}</color>", "#cccccc", "Vô");
            }
        }

        public static int GetSkillStyleDef(string StyleInput)
        {
            switch (StyleInput)
            {
                case "meleephysicalattack":
                    return 1;

                case "rangephysicalattack":
                    return 2;

                case "rangemagicattack":
                    return 3;

                case "aurarangemagicattack":
                    return 4;

                case "defendcurse":
                    return 5;

                case "attackcurse":
                    return 6;

                case "fightcurse":
                    return 7;

                case "curse":
                    return 8;

                case "trap":
                    return 9;

                case "initiativeattackassistant":
                    return 10;

                case "initiativedefendassistant":
                    return 11;

                case "initiativefightassistant":
                    return 12;

                case "stolenassistant":
                    return 13;

                case "initiativeattackassistantally":
                    return 14;

                case "initiativedefendassistantally":
                    return 15;

                case "initiativefightassistantally":
                    return 16;

                case "specialattack":
                    return 17;

                case "disable":
                    return 18;

                case "assistant":
                    return 19;

                case "nonpcskill":
                    return 20;
            }
            return -1;
        }

        #endregion SkillSettings

        #region NGUHANHTUONGKHAC

        public static int[] g_nAccrueSeries = new int[(int)KE_SERIES_TYPE.series_num]; // TƯƠNG SINH
        public static int[] g_nConquerSeries = new int[(int)KE_SERIES_TYPE.series_num]; // TƯƠNG KHẮC
        public static int[] g_nAccruedSeries = new int[(int)KE_SERIES_TYPE.series_num]; // ĐƯỢC TƯƠNG SINH
        public static int[] g_nConqueredSeries = new int[(int)KE_SERIES_TYPE.series_num]; // BỊ KHẮC

        public static void LoadAccrueSeries()
        {
            g_nAccrueSeries[(int)KE_SERIES_TYPE.series_none] = (int)KE_SERIES_TYPE.series_none;
            g_nConquerSeries[(int)KE_SERIES_TYPE.series_none] = (int)KE_SERIES_TYPE.series_none;
            g_nAccruedSeries[(int)KE_SERIES_TYPE.series_none] = (int)KE_SERIES_TYPE.series_none;
            g_nConqueredSeries[(int)KE_SERIES_TYPE.series_none] = (int)KE_SERIES_TYPE.series_none;
            // 五行生克关系
            g_nAccrueSeries[(int)KE_SERIES_TYPE.series_metal] = (int)KE_SERIES_TYPE.series_water;
            g_nConquerSeries[(int)KE_SERIES_TYPE.series_metal] = (int)KE_SERIES_TYPE.series_wood;
            g_nAccruedSeries[(int)KE_SERIES_TYPE.series_metal] = (int)KE_SERIES_TYPE.series_earth;
            g_nConqueredSeries[(int)KE_SERIES_TYPE.series_metal] = (int)KE_SERIES_TYPE.series_fire;
            g_nAccrueSeries[(int)KE_SERIES_TYPE.series_wood] = (int)KE_SERIES_TYPE.series_fire;
            g_nConquerSeries[(int)KE_SERIES_TYPE.series_wood] = (int)KE_SERIES_TYPE.series_earth;
            g_nAccruedSeries[(int)KE_SERIES_TYPE.series_wood] = (int)KE_SERIES_TYPE.series_water;
            g_nConqueredSeries[(int)KE_SERIES_TYPE.series_wood] = (int)KE_SERIES_TYPE.series_metal;
            g_nAccrueSeries[(int)KE_SERIES_TYPE.series_water] = (int)KE_SERIES_TYPE.series_wood;
            g_nConquerSeries[(int)KE_SERIES_TYPE.series_water] = (int)KE_SERIES_TYPE.series_fire;
            g_nAccruedSeries[(int)KE_SERIES_TYPE.series_water] = (int)KE_SERIES_TYPE.series_metal;
            g_nConqueredSeries[(int)KE_SERIES_TYPE.series_water] = (int)KE_SERIES_TYPE.series_earth;
            g_nAccrueSeries[(int)KE_SERIES_TYPE.series_fire] = (int)KE_SERIES_TYPE.series_earth;
            g_nConquerSeries[(int)KE_SERIES_TYPE.series_fire] = (int)KE_SERIES_TYPE.series_metal;
            g_nAccruedSeries[(int)KE_SERIES_TYPE.series_fire] = (int)KE_SERIES_TYPE.series_wood;
            g_nConqueredSeries[(int)KE_SERIES_TYPE.series_fire] = (int)KE_SERIES_TYPE.series_water;
            g_nAccrueSeries[(int)KE_SERIES_TYPE.series_earth] = (int)KE_SERIES_TYPE.series_metal;
            g_nConquerSeries[(int)KE_SERIES_TYPE.series_earth] = (int)KE_SERIES_TYPE.series_water;
            g_nAccruedSeries[(int)KE_SERIES_TYPE.series_earth] = (int)KE_SERIES_TYPE.series_fire;
            g_nConqueredSeries[(int)KE_SERIES_TYPE.series_earth] = (int)KE_SERIES_TYPE.series_wood;
        }

        public static bool g_IsAccrue(int nSrcSeries, int nDesSeries)
        {
            return g_InternalIsAccrueConquer(g_nAccrueSeries, nSrcSeries, nDesSeries);
        }

        public static bool g_IsConquer(int nSrcSeries, int nDesSeries)
        {
            return g_InternalIsAccrueConquer(g_nConquerSeries, nSrcSeries, nDesSeries);
        }

        public static bool g_InternalIsAccrueConquer(int[] pAccrueConquerTable, int nSrcSeries, int nDesSeries)
        {
            if (nSrcSeries < (int)KE_SERIES_TYPE.series_none || nSrcSeries >= (int)KE_SERIES_TYPE.series_num)
                return false;

            return nDesSeries == pAccrueConquerTable[nSrcSeries];
        }

        #endregion NGUHANHTUONGKHAC


        #region CheckTuiDo

        public static bool IsHaveSpace(int NeedSpace,KPlayer client)
        {

            bool IsHave = false;

            if (client.GoodsDataList == null)
            {
                IsHave =  true;
            }


            int totalGridNum = 0;
            lock (client.GoodsDataList)
            {
                for (int i = 0; i < client.GoodsDataList.Count; i++)
                {
                    if (client.GoodsDataList[i].Using >= 0)
                    {
                        continue;
                    }

                    totalGridNum++;

                }
            }


            // TODO LẤY RA SỐ Ô HIỆN TẠI CỦA CLIENT
            int totalMaxGridCount = 100;

            int LessSpace = totalMaxGridCount - totalGridNum;

            if(LessSpace >= NeedSpace)
            {
                IsHave = true;
            }
            else
            {
                IsHave = false;
            }


            return IsHave;
        }

        #endregion

        /// <summary>
        /// Đối tượng Random
        /// </summary>
        private static readonly System.Random random = new System.Random();

        /// <summary>
        /// Trả về số nguyên trong khoảng tương ứng
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static int GetRandomNumber(int min, int max)
        {
            lock (KTGlobal.random)
            {
                int nRand = KTGlobal.random.Next(min, max + 1);
                if (nRand > max)
                {
                    nRand = max;
                }
                return nRand;
            }
        }

        /// <summary>
        /// Trả về số thực trong khoảng tương ứng
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static float GetRandomNumber(float min, float max)
        {
            lock (KTGlobal.random)
            {
                return (float)KTGlobal.random.NextDouble() * (max - min) + min;
            }
        }

        /// <summary>
        /// Trả về số thực trong khoảng tương ứng
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static double GetRandomNumber(double min, double max)
        {
            lock (KTGlobal.random)
            {
                return KTGlobal.random.NextDouble() * (max - min) + min;
            }
        }

        #region Newbie Villages

        /// <summary>
        /// Danh sách các tân thủ thôn khi tạo nhân vật sẽ được vào
        /// </summary>
        public static List<NewbieVillage> NewbieVillages { get; } = new List<NewbieVillage>();

        /// <summary>
        /// Đọc dữ liệu danh sách các tân thủ thôn khi tạo nhân vật sẽ được vào
        /// </summary>
        public static void LoadNewbieVillages()
        {
            KTGlobal.NewbieVillages.Clear();

            XElement xmlNode = Global.GetGameResXml("Config/NewbieVillages.xml");
            foreach (XElement node in xmlNode.Elements())
            {
                NewbieVillage newbieVillage = NewbieVillage.Parse(node);
                KTGlobal.NewbieVillages.Add(newbieVillage);
            }
        }

        #endregion Newbie Villages

        #region Timer

        /// <summary>
        /// Tạo luồng thực hiện công việc sau khoảng thời gian
        /// </summary>
        /// <param name="work">Công việc</param>
        /// <param name="interval">Thời gian chờ trước khi thực thi</param>
        /// <returns></returns>
        public static KTSchedule SetTimer(Action work, float interval)
        {
            KTSchedule schedule = new KTSchedule()
            {
                Name = "Timer from GLOBAL",
                Interval = interval,
                Loop = false,
                Work = work,
            };
            KTTaskManager.Instance.AddSchedule(schedule);
            return schedule;
        }

        #endregion Timer

        /// <summary>
        /// Trả về giờ hệ thống hiện tại dưới đơn vị Mili giây
        /// </summary>
        /// <returns></returns>
        public static long GetCurrentTimeMilis()
        {
            return DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
        }

        /// <summary>
        /// Trả về giờ quốc tế dưới đơn vị Mili giây
        /// </summary>
        /// <returns></returns>
        public static long GetGlobalTimeMilis()
        {
            return DateTime.UtcNow.Ticks / TimeSpan.TicksPerMillisecond;
        }

        #region Date Time

        /// <summary>
        /// Hiển thị thời gian
        /// <para>Thời gian dạng giây</para>
        /// </summary>
        public static string DisplayTime(float timeInSec)
        {
            int sec = (int)timeInSec;
            if (sec >= 86400)
            {
                int nDay = sec / 86400;
                return string.Format("{0} ngày", nDay);
            }
            else if (sec >= 3600)
            {
                int nHour = sec / 3600;
                return string.Format("{0} giờ", nHour);
            }
            else
            {
                int nMinute = sec / 60;
                int nSecond = sec - nMinute * 60;
                string secondString = nSecond.ToString();
                while (secondString.Length < 2)
                {
                    secondString = "0" + secondString;
                }
                return string.Format("{0} phút {1} giây", nMinute, secondString);
            }
        }

        #endregion Date Time





    }
}