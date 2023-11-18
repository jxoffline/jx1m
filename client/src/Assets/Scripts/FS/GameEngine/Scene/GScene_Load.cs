//#define USE_AS3_COMPAITABLE_ASTAR

using FS.Drawing;
using FS.GameEngine.Data;
using FS.GameEngine.Logic;
using FS.GameEngine.Teleport;
using FS.VLTK;
using FS.VLTK.Control.Component;
using FS.VLTK.Entities.Config;
using FS.VLTK.Factory;
using FS.VLTK.Factory.ObjectsManager;
using FS.VLTK.Factory.UIManager;
using FS.VLTK.Loader;
using FS.VLTK.Logic;
using System;
using System.Collections.Generic;
using System.Xml.Linq;
using static FS.VLTK.Entities.Enum;
using UnityEngine;

namespace FS.GameEngine.Scene
{
    /// <summary>
    /// Quản lý tài nguyên bản đồ
    /// </summary>
    public partial class GScene
    {
        #region Khởi tạo ban đầu
        /// <summary>
        /// Đối tượng chứa toàn bộ thông tin bản đồ
        /// </summary>
        private GameObject Root2DScene;

        /// <summary>
        /// Ký hiệu chọn mục tiêu
        /// </summary>
        private SelectTargetDeco SelectTargetDeco { get; set; }

        /// <summary>
        /// Ký hiệu vùng Farm sử dụng Auto
        /// </summary>
        private AutoFarmAreaDeco AutoFarmAreaDeco { get; set; }

        /// <summary>
        /// Ký hiệu vị trí Click di chuyển tới
        /// </summary>
        private Effect ClickMoveDeco { get; set; }

        /// <summary>
        /// Ký hiệu đánh dấu vị trí ra chiêu
        /// </summary>
        private GameObject SkillMarkTargetPosDeco { get; set; }

        /// <summary>
        /// Danh sách điểm truyền tống trong bản đồ
        /// </summary>
        private readonly List<GTeleport> listTeleport = new List<GTeleport>();

        /// <summary>
        /// Hủy tài nguyên bản đồ cũ
        /// </summary>
        public void ClearScene()
        {
            try
            {
                /// Đánh dấu chưa hoàn tất chuyển map
                this.MapLoadingCompleted = false;

                /// Ngừng di chuyển Leader
                if (null != this.Leader)
                {
                    KTLeaderMovingManager.StopMove();
                    KTLeaderMovingManager.StopChasingTarget();
                }

                /// Làm rỗng dữ liệu AutoFight
                KTAutoFightManager.Instance.Clear();
                /// Làm rỗng dữ liệu AutoPet
                KTAutoPetManager.Instance.Clear();

                /// Làm mới dữ liệu các Object kỹ năng
                SkillManager.Refresh();

                /// Xóa dữ liệu Leader
                this.Leader = null;

                /// Xóa toàn bộ Storyboard
                KTStoryBoard.Instance.RemoveAllStoryBoards();

                /// Xóa dữ liệu người chơi xung quanh
                Global.Data.OtherRoles.Clear();
                Global.Data.OtherRolesByName.Clear();

                /// Xóa dữ liệu bot xung quanh
                Global.Data.Bots.Clear();
                /// Xóa dữ liệu pet xung quanh
                Global.Data.SystemPets.Clear();

                /// Xóa dữ liệu quái xung quanh
                Global.Data.SystemMonsters.Clear();

                /// Hủy đối tượng tự tìm đường
                this.pathFinderFast = null;

                /// Làm rỗng danh sách cổng dịch chuyển
                if (this.listTeleport != null)
                {
                    this.listTeleport.Clear();
                }

                /// Hủy đối tượng ký hiệu chọn mục tiêu
                if (this.SelectTargetDeco != null)
                {
                    GameObject.Destroy(this.SelectTargetDeco);
                }
                this.SelectTargetDeco = null;

                /// Hủy đối tượng vùng Farm
                if (this.AutoFarmAreaDeco != null)
                {
                    GameObject.Destroy(this.AutoFarmAreaDeco);
                }
                this.AutoFarmAreaDeco = null;

                /// Hủy đối tượng ký hiệu Click-Move
                if (this.ClickMoveDeco != null)
                {
                    GameObject.Destroy(this.ClickMoveDeco);
                }
                this.ClickMoveDeco = null;

                /// Hủy đối tượng đánh dấu vị trí ra chiêu
                if (this.SkillMarkTargetPosDeco != null)
                {
                    GameObject.Destroy(this.SkillMarkTargetPosDeco);
                }
                this.SkillMarkTargetPosDeco = null;

                /// Xóa danh sách chờ tải
                this.waitToBeAddedMonster.Clear();
                this.waitToBeAddedRole.Clear();
                this.waitToBeAddedRole.Clear();
                this.waitToBeAddedMonster.Clear();
                this.waitToBeAddedGrowPoint.Clear();
                this.waitToBeAddedGoodsPack.Clear();
                this.waitToBeAddedDynamicArea.Clear();
                this.waitToBeAddedBot.Clear();
                this.waitToBeAddedStallBot.Clear();
                this.waitToBeAddedPets.Clear();

                /// Làm rỗng UI
                UIHintItemManager.Instance.Clear();
                UIBottomTextManager.Instance.Clear();
                /// Xóa toàn bộ các bản thể soi trước
                KTRolePreviewManager.Instance.RemoveAllInstances();

                /// nếu không phải đang bán đồ thì có thể hủy tự đánh
                if (!KTAutoFightManager.Instance.DoingBuyItem && !KTAutoFightManager.Instance.DoingAutoSell)
                {
                    /// Hủy tự động đánh
                    KTAutoFightManager.Instance.StopAutoFight();
                }

                /// Xóa dữ liệu bản đồ
                if (this.CurrentMapData != null)
                {
                    this.CurrentMapData.Dispose();
                }
                this.CurrentMapData = null;
                this.MapCode = -1;

                /// Làm rỗng danh sách đối tượng đang hiển thị
                KTGlobal.RemoveAllObjects();
            }
            catch (Exception ex)
            {
                KTGlobal.ShowMessageBox("Lỗi phát sinh", "Lỗi phát sinh khi xóa tài nguyên bản đồ: " + ex.Message + ". Hãy báo lại với hỗ trợ, sau đó ấn OK để bỏ qua", true);
            }
        }

        /// <summary>
        /// Tải xuống bản đồ
        /// </summary>
        /// <param name="mapCode"></param>
        /// <param name="leaderX"></param>
        /// <param name="leaderY"></param>
        /// <param name="direction"></param>
        public void LoadScene(int mapCode, double leaderX, double leaderY, double direction)
        {
            try
            {
                this.MapCode = mapCode;
                /// Đánh dấu tải xuống bản đồ thành công
                this.MapLoadingCompleted = true;

                this.LoadMapData();
                this.LoadLeader((int) leaderX, (int) leaderY, (int) direction);

                /// Khởi động lại Auto Pet
                KTAutoPetManager.Instance.Restart();
            }
            catch (Exception ex)
            {
                KTGlobal.ShowMessageBox("Lỗi phát sinh", "Lỗi phát sinh khi đọc dữ liệu bản đồ: " + ex.Message + ". Hãy báo lại với hỗ trợ, sau đó ấn OK để bỏ qua", true);
            }
        }
        #endregion

        #region Tải xuống dữ liệu bản đồ
        /// <summary>
        /// Tải dữ liệu bản đồ
        /// </summary>
        private void LoadMapData()
        {
            this.Root2DScene = GameObject.Find("Scene 2D Root");
            if (this.Root2DScene == null)
            {
                this.Root2DScene = new GameObject("Scene 2D Root");
                this.Root2DScene.transform.localPosition = Vector3.zero;
            }

            this.SelectTargetDeco = Object2DFactory.MakeSelectTargetDeco();
            this.SelectTargetDeco.gameObject.transform.SetParent(this.Root2DScene.transform, false);
            this.RemoveSelectTarget();

            this.AutoFarmAreaDeco = Object2DFactory.MakeFarmAreaDeco();
            this.AutoFarmAreaDeco.transform.SetParent(this.Root2DScene.transform, false);
            this.RemoveFarmAreaDeco();

            this.SkillMarkTargetPosDeco = Object2DFactory.MakeSkillTargetPositionDeco();
            this.SkillMarkTargetPosDeco.gameObject.transform.SetParent(this.Root2DScene.transform, false);
            this.RemoveSkillMarkTargetPos();

            this.ClickMoveDeco = Object2DFactory.MakeEffect();
            this.ClickMoveDeco.gameObject.transform.SetParent(this.Root2DScene.transform, false);
            this.ClickMoveDeco.gameObject.transform.localScale = new Vector3(0.5f, 0.5f, 1f);
            if (Loader.ListEffects.TryGetValue(75, out VLTK.Entities.Config.StateEffectXML effectXML))
            {
                VLTK.Entities.Config.StateEffectXML cloneEffectXML = effectXML.Clone();
                cloneEffectXML.Loop = true;
                this.ClickMoveDeco.Data = cloneEffectXML;
                this.ClickMoveDeco.RefreshAction();
                this.ClickMoveDeco.Play();
            }
            this.RemoveClickMovePos();

            this.CurrentMapData = new GMapData();
            Global.CurrentMapData = this.CurrentMapData;

            this.CurrentMapData.RealMapSize = new Vector2(Loader.Maps[this.MapCode].Width, Loader.Maps[this.MapCode].Height);

            /// Tải xuống Bundle
            AssetBundle assetBundle = this.LoadMapBundle(this.MapCode);
            //foreach (string assetName in assetBundle.GetAllAssetNames())
            //{
            //    KTDebug.LogError("Asset: " + assetName);
            //}
            this.LoadMapSetting(assetBundle);
            this.LoadObstruction(assetBundle);
            this.LoadTeleportList(assetBundle);
            this.LoadMinimapMonsterList(assetBundle);
            this.LoadMinimapNPCList(assetBundle);
            this.LoadMinimapGrowPointList(assetBundle);
            this.LoadMinimapZone(assetBundle);
            /// Giải phóng Bundle
            assetBundle.Unload(true);
            /// Xóa Bundle
            GameObject.Destroy(assetBundle);
        }

        /// <summary>
        /// Tải xuống AssetBundle chứa cấu hình bản đồ tương ứng
        /// </summary>
        private AssetBundle LoadMapBundle(int mapCode)
        {
            /// Thông tin bản đồ tương ứng
            if (Loader.Maps.TryGetValue(mapCode, out VLTK.Entities.Config.Map mapData))
            {
                /// Đường dẫn File Bundle
                string bundleDir = Global.WebPath(string.Format("Data/MapConfig/{0}.unity3d", mapData.Code));
                /// Tải xuống Bundle
                return ResourceLoader.LoadAssetBundle(bundleDir, true);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Tải thiết lập bản đồ
        /// </summary>
        /// <param name="bundle"></param>
        private void LoadMapSetting(AssetBundle bundle)
        {
            GMapData mapData = this.CurrentMapData;
            XElement xmlNode = ResourceLoader.LoadXMLFromBundle(bundle, "MapSetting.xml");
            mapData.Setting = MapSetting.Parse(xmlNode);
        }

        /// <summary>
        /// Tải danh sách Monster hiện trên bản đồ nhỏ từ bundle
        /// </summary>
        /// <param name="bundle"></param>
        private void LoadMinimapGrowPointList(AssetBundle bundle)
        {
            GMapData mapData = this.CurrentMapData;
            XElement xmlNode = FS.VLTK.Loader.ResourceLoader.LoadXMLFromBundle(bundle, "GrowPoints.xml");
            /// Nếu không có file điểm thu thập
            if (xmlNode == null)
            {
                /// Tạo mới
                mapData.GrowPointList = new List<KeyValuePair<string, Point>>();
                mapData.GrowPointListByID = new Dictionary<int, List<Point>>();
                mapData.MinimapGrowPointList = new List<KeyValuePair<string, Point>>();
                return;
            }

            List<KeyValuePair<string, Point>> growPointList = new List<KeyValuePair<string, Point>>();
            Dictionary<int, List<Point>> growPointListByID = new Dictionary<int, List<Point>>();
            List<KeyValuePair<string, Point>> minimapGrowPointList = new List<KeyValuePair<string, Point>>();
            foreach (XElement node in xmlNode.Element("GrowPoints").Elements("GrowPoint"))
            {
                /// ID tĩnh của điểm thu thập
                int staticID = int.Parse(node.Attribute("Code").Value);
                /// Hiện ở Minimap không
                bool visibleOnMinimap = int.Parse(node.Attribute("VisibleOnMinimap").Value) == 1;
                ///// Nếu không hiện ở Minimap
                //if (!visibleOnMinimap)
                //{
                //    /// Bỏ qua
                //    continue;
                //}

                string name = node.Attribute("Name").Value;
                if (string.IsNullOrEmpty(name) && Loader.ListMonsters.TryGetValue(staticID, out MonsterDataXML monsterData))
                {
                    name = monsterData.Name;
                }
                int posX = int.Parse(node.Attribute("PosX").Value);
                int posY = int.Parse(node.Attribute("PosY").Value);
                growPointList.Add(new KeyValuePair<string, Point>(name, new Point(posX, posY)));

                if (!growPointListByID.ContainsKey(staticID))
                {
                    growPointListByID[staticID] = new List<Point>();
                }
                growPointListByID[staticID].Add(new Point(posX, posY));

                /// Nếu hiện ở Minimap
                if (visibleOnMinimap)
                {
                    minimapGrowPointList.Add(new KeyValuePair<string, Point>(name, new Point(posX, posY)));
                }
            }
            mapData.GrowPointList = growPointList;
            mapData.GrowPointListByID = growPointListByID;
            mapData.MinimapGrowPointList = minimapGrowPointList;
        }

        /// <summary>
        /// Tải danh sách điểm truyền tống gợi ý trên bản đồ nhỏ từ bundle
        /// </summary>
        /// <param name="bundle"></param>
        private void LoadTeleportList(AssetBundle bundle)
        {
            this.listTeleport.Clear();

            GMapData mapData = this.CurrentMapData;

            XElement xmlNode = FS.VLTK.Loader.ResourceLoader.LoadXMLFromBundle(bundle, "Teleports.xml");
            /// Nếu không có file
            if (xmlNode == null)
            {
                /// Tạo mới
                mapData.Teleport = new List<KeyValuePair<string, Point>>();
                return;
            }

            List<KeyValuePair<string, Point>> teleportList = new List<KeyValuePair<string, Point>>();
            foreach (XElement node in xmlNode.Element("Teleports").Elements("Teleport"))
            {
                int id = int.Parse(node.Attribute("Code").Value);
                string name = node.Attribute("Tip").Value;
                int posX = int.Parse(node.Attribute("X").Value);
                int posY = int.Parse(node.Attribute("Y").Value);
                int toMapCode = int.Parse(node.Attribute("To").Value);
                teleportList.Add(new KeyValuePair<string, Point>(name, new Point(posX, posY)));

                GTeleport teleport = Global.GetTeleport(node);
                teleport.SpriteType = GSpriteTypes.Teleport;
                teleport.BaseID = (int) ObjectBaseID.Teleport + id;
                if (Loader.Maps.TryGetValue(toMapCode, out VLTK.Entities.Config.Map staticMapData))
                {
                    teleport.ToLevel = staticMapData.Level;
                    string mapTypeString = staticMapData.MapType;
                    switch (mapTypeString)
                    {
                        case "village":
                            teleport.ToType = VLTK.Entities.Enum.MapType.Village;
                            break;
                        case "city":
                            teleport.ToType = VLTK.Entities.Enum.MapType.City;
                            break;
                        case "faction":
                            teleport.ToType = VLTK.Entities.Enum.MapType.Faction;
                            break;
                        case "fight":
                            teleport.ToType = VLTK.Entities.Enum.MapType.Fight;
                            break;
                    }
                    this.Add(teleport);
                    this.listTeleport.Add(teleport);
                    teleport.Start();
                }
                else
                {
                    KTGlobal.ShowMessageBox("Lỗi VKL", "TOANG TELEPORT ~~~ Code = " + id);
                }
            }
            mapData.Teleport = teleportList;
        }

        /// <summary>
        /// Tải danh sách Monster hiện trên bản đồ nhỏ từ bundle
        /// </summary>
        /// <param name="bundle"></param>
        private void LoadMinimapMonsterList(AssetBundle bundle)
        {
            GMapData mapData = this.CurrentMapData;

            XElement xmlNode = FS.VLTK.Loader.ResourceLoader.LoadXMLFromBundle(bundle, "Monsters.xml");
            /// Nếu không có file
            if (xmlNode == null)
            {
                /// Tạo mới
                mapData.MonsterList = new List<KeyValuePair<string, Point>>();
                mapData.MonsterListByID = new Dictionary<int, List<Point>>();
                mapData.MinimapMonsterList = new List<KeyValuePair<string, Point>>();
                return;
            }

            List<KeyValuePair<string, Point>> monsterList = new List<KeyValuePair<string, Point>>();
            Dictionary<int, List<Point>> monsterListByID = new Dictionary<int, List<Point>>();
            List<KeyValuePair<string, Point>> minimapMonsterList = new List<KeyValuePair<string, Point>>();
            foreach (XElement node in xmlNode.Element("Monsters").Elements("Monster"))
            {
                /// ID tĩnh của quái
                int staticID = int.Parse(node.Attribute("Code").Value);
                /// Hiện ở Minimap không
                bool visibleOnMinimap = false;

                if (node.Attribute("VisibleOnMinimap").Value != "")
                {
                    visibleOnMinimap = int.Parse(node.Attribute("VisibleOnMinimap").Value) == 1;
                }

                ///// Nếu không hiện ở Minimap
                //if (!visibleOnMinimap)
                //{
                //    /// Bỏ qua
                //    continue;
                //}


                string name = "";
                if (Loader.ListMonsters.TryGetValue(staticID, out VLTK.Entities.Config.MonsterDataXML monsterData))
                {
                    name = monsterData.Name;
                }
                int posX = int.Parse(node.Attribute("X").Value);
                int posY = int.Parse(node.Attribute("Y").Value);
                monsterList.Add(new KeyValuePair<string, Point>(name, new Point(posX, posY)));

                if (!monsterListByID.ContainsKey(staticID))
                {
                    monsterListByID[staticID] = new List<Point>();
                }

                monsterListByID[staticID].Add(new Point(posX, posY));

                /// Nếu hiện ở Minimap
                if (visibleOnMinimap)
                {
                    minimapMonsterList.Add(new KeyValuePair<string, Point>(name, new Point(posX, posY)));
                }
            }
            mapData.MonsterList = monsterList;
            mapData.MonsterListByID = monsterListByID;
            mapData.MinimapMonsterList = minimapMonsterList;
        }

        /// <summary>
        /// Tải danh sách Npc hiện trên bản đồ nhỏ từ bundle
        /// </summary>
        /// <param name="bundle"></param>
        private void LoadMinimapNPCList(AssetBundle bundle)
        {
            GMapData mapData = this.CurrentMapData;
            int mapId = this.MapCode;

            XElement xmlNode = FS.VLTK.Loader.ResourceLoader.LoadXMLFromBundle(bundle, "NPCs.xml");
            /// Nếu không có file
            if (xmlNode == null)
            {
                /// Tạo mới
                mapData.NpcList = new List<KeyValuePair<string, Point>>();
                mapData.NpcListByID = new Dictionary<int, List<Point>>();
                mapData.MinimapNPCList = new List<KeyValuePair<string, Point>>();
                return;
            }

            List<KeyValuePair<string, Point>> npcList = new List<KeyValuePair<string, Point>>();
            Dictionary<int, List<Point>> npcListByID = new Dictionary<int, List<Point>>();
            List<KeyValuePair<string, Point>> minimapNPCList = new List<KeyValuePair<string, Point>>();
            foreach (XElement node in xmlNode.Element("NPCs").Elements("NPC"))
            {
                /// ID tĩnh của NPC
                int staticID = int.Parse(node.Attribute("Code").Value);
                /// Hiện ở Minimap không
                bool visibleOnMinimap = int.Parse(node.Attribute("VisibleOnMinimap").Value) == 1;
                ///// Nếu không cho phép hiển thị ở Minimap
                //if (!visibleOnMinimap)
                //{
                //    continue;
                //}

                string name = "";
                if (node.Attribute("Name") != null)
                {
                    name = node.Attribute("Name").Value;
                    if (node.Attribute("MinimapName") != null)
                    {
                        string mName = node.Attribute("MinimapName").Value;
                        if (!string.IsNullOrEmpty(mName))
                        {
                            name = mName;
                        }
                    }
                }
                else if (Loader.ListMonsters.TryGetValue(staticID, out VLTK.Entities.Config.MonsterDataXML monsterData))
                {
                    name = monsterData.Name;
                }
                int posX = int.Parse(node.Attribute("X").Value);
                int posY = int.Parse(node.Attribute("Y").Value);
                npcList.Add(new KeyValuePair<string, Point>(name, new Point(posX, posY)));

                if (!npcListByID.ContainsKey(staticID))
                {
                    npcListByID[staticID] = new List<Point>();
                }
                npcListByID[staticID].Add(new Point(posX, posY));

                /// Nếu hiện ở Minimap
                if (visibleOnMinimap)
                {
                    minimapNPCList.Add(new KeyValuePair<string, Point>(name, new Point(posX, posY)));
                }
            }
            mapData.NpcList = npcList;
            mapData.NpcListByID = npcListByID;
            mapData.MinimapNPCList = minimapNPCList;
        }

        /// <summary>
        /// Tải danh sách các vùng hiện trên bản đồ nhỏ từ bundle
        /// </summary>
        /// <param name="bundle"></param>
        private void LoadMinimapZone(AssetBundle bundle)
        {
            GMapData mapData = this.CurrentMapData;
            int mapId = this.MapCode;

            XElement xmlNode = FS.VLTK.Loader.ResourceLoader.LoadXMLFromBundle(bundle, "Zones.xml");
            /// Nếu không có file
            if (xmlNode == null)
            {
                /// Tạo mới
                mapData.Zone = new List<KeyValuePair<string, Point>>();
                return;
            }

            List<KeyValuePair<string, Point>> zone = new List<KeyValuePair<string, Point>>();
            if (xmlNode != null)
            {
                foreach (XElement node in xmlNode.Element("Zones").Elements("Zone"))
                {
                    string name = node.Attribute("Name").Value;
                    int posX = int.Parse(node.Attribute("X").Value);
                    int posY = int.Parse(node.Attribute("Y").Value);
                    zone.Add(new KeyValuePair<string, Point>(name, new Point(posX, posY)));
                }
            }
            mapData.Zone = zone;
        }

        /// <summary>
        /// Đọc dữ liệu các ô vật cản
        /// </summary>
        /// <param name="bundle"></param>
        private void LoadObstruction(AssetBundle bundle)
        {
            XElement xmlNode = FS.VLTK.Loader.ResourceLoader.LoadXMLFromBundle(bundle, "Obs.xml");

            this.CurrentMapData.MapWidth = int.Parse(xmlNode.Attribute("MapWidth").Value);
            this.CurrentMapData.MapHeight = int.Parse(xmlNode.Attribute("MapHeight").Value);

            this.CurrentMapData.GridSizeX = 20;
            this.CurrentMapData.GridSizeY = 20;

            this.CurrentMapData.OriginGridSizeXNum = int.Parse(xmlNode.Attribute("OriginGridSizeXNum").Value);
            this.CurrentMapData.OriginGridSizeYNum = int.Parse(xmlNode.Attribute("OriginGridSizeYNum").Value);

            int wGridsNum = (this.CurrentMapData.MapWidth - 1) / this.CurrentMapData.GridSizeX + 1;
            int hGridsNum = (this.CurrentMapData.MapHeight - 1) / this.CurrentMapData.GridSizeY + 1;

            wGridsNum = (int)Math.Ceiling(Math.Log(wGridsNum, 2));
            wGridsNum = (int)Math.Pow(2, wGridsNum);

            hGridsNum = (int)Math.Ceiling(Math.Log(hGridsNum, 2));
            hGridsNum = (int)Math.Pow(2, hGridsNum);

            this.CurrentMapData.GridSizeXNum = wGridsNum;
            this.CurrentMapData.GridSizeYNum = hGridsNum;

            this.CurrentMapData.Obstructions = new byte[wGridsNum, hGridsNum];
            this.CurrentMapData.BlurPositions = new byte[wGridsNum, hGridsNum];

            byte[] obsBytes = ResourceLoader.LoadBytesFromBundle(bundle, "Obs.txt");
            byte[] blurBytes = ResourceLoader.LoadBytesFromBundle(bundle, "Blur.txt");
            byte[] dynObsBytes = ResourceLoader.LoadBytesFromBundle(bundle, "DynamicObs.txt");
            byte[] safeAreaBytes = ResourceLoader.LoadBytesFromBundle(bundle, "SafeArea.txt");

            this.CurrentMapData.Obstructions.FromBytes<byte>(obsBytes);
            this.CurrentMapData.BlurPositions.FromBytes<byte>(blurBytes);
            /// Nếu tòn tại Obs động
            if (dynObsBytes != null)
            {
                this.CurrentMapData.DynamicObstructions = new byte[wGridsNum, hGridsNum];
                this.CurrentMapData.DynamicObstructions.FromBytes<byte>(dynObsBytes);
            }
            /// Nếu tồn tại khu an toàn
            if (safeAreaBytes != null)
            {
                this.CurrentMapData.SafeAreas = new byte[wGridsNum, hGridsNum];
                this.CurrentMapData.SafeAreas.FromBytes<byte>(safeAreaBytes);
            }

            /// Dọn rác
            obsBytes = null;
            blurBytes = null;
            dynObsBytes = null;
            safeAreaBytes = null;
        }
        #endregion
    }
}
