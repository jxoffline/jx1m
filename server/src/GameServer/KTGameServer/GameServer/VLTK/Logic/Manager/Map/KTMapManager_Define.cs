using GameServer.KiemThe;
using GameServer.Logic;
using HSGameEngine.Tools.AStarEx;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Xml.Linq;

namespace GameServer.Logic
{
    /// <summary>
    /// Quản lý bản đồ
    /// </summary>
    public static partial class KTMapManager
    {
        /// <summary>
        /// Thông tin điểm truyền tống
        /// </summary>
        public class MapTeleport
        {
            /// <summary>
            /// ID cổng trong bản đồ
            /// </summary>
            public int Code
            {
                get;
                set;
            }

            /// <summary>
            /// ID bản đồ
            /// </summary>
            public int MapID
            {
                get;
                set;
            }

            /// <summary>
            /// Tọa độ X
            /// </summary>
            public int X
            {
                get;
                set;
            }

            /// <summary>
            /// Tọa độ Y
            /// </summary>
            public int Y
            {
                get;
                set;
            }

            /// <summary>
            /// Radius
            /// </summary>
            public int Radius
            {
                get;
                set;
            }

            /// <summary>
            /// ID bản đồ tới
            /// </summary>
            public int ToMapID
            {
                get;
                set;
            }

            /// <summary>
            /// Tọa độ tới X
            /// </summary>
            public int ToX
            {
                get;
                set;
            }

            /// <summary>
            /// Tọa độ tới Y
            /// </summary>
            public int ToY
            {
                get;
                set;
            }

            /// <summary>
            /// Phe nào được sử dụng dịch chuyển này
            /// </summary>
            public int Camp
            {
                get; set;
            }
        }

        /// <summary>
        /// Đối tượng bản đồ
        /// </summary>
        public class GameMap
        {
            #region Properties
            /// <summary>
            /// Lưới bản đồ
            /// </summary>
            public MapGridManager.MapGrid Grid { get; set; }

            /// <summary>
            /// Đường dẫn chứa cấu hình bản đồ
            /// </summary>
            public string MapConfigDir { get; set; }

            /// <summary>
            /// Thiết lập rơi cho quái thường
            /// </summary>
            public string NormalDropRate { get; set; }

            /// <summary>
            /// DROP GLOABL cho quái tinh anh
            /// </summary>
            public string GoldenDropRate { get; set; }

            /// <summary>
            /// Tên bản đồ
            /// </summary>
            public string MapName { get; set; }

            /// <summary>
            /// Danh sách điểm truyền tống
            /// </summary>
            public Dictionary<int, MapTeleport> MapTeleportDict = new Dictionary<int, MapTeleport>();

            /// <summary>
            /// ID bản đồ
            /// </summary>
            public int MapCode { get; set; }

            /// <summary>
            /// Cấp độ bản đồ
            /// </summary>
            public int MapLevel { get; set; }

            /// <summary>
            /// Tên loại bản đồ
            /// </summary>
            public string MapType { get; set; }


            /// <summary>
            /// Đây có phải map liên máy chủ không
            /// </summary>
            public bool IsKuaFuMap { get; set; }

            /// <summary>
            /// Chiều rộng bản đồ
            /// </summary>
            public int MapWidth { get; set; }

            /// <summary>
            /// Chiều cao bản đồ
            /// </summary>
            public int MapHeight { get; set; }

            /// <summary>
            /// Kích thước lưới chiều rộng của bản đồ
            /// </summary>
            public int MapGridWidth { get; set; }

            /// <summary>
            /// Kích thước lưới chiều cao của bản đồ
            /// </summary>
            public int MapGridHeight { get; set; }

            /// <summary>
            /// Tổng số cột của lưới bản đồ
            /// </summary>
            public int MapGridColsNum { get; set; }

            /// <summary>
            /// Tổng số hàng của lưới bản đồ
            /// </summary>
            public int MapGridRowsNum { get; set; }

            /// <summary>
            /// Đối tượng lưới quản lý
            /// </summary>
            private MapGridManager.NodeGrid _NodeGrid;
            /// <summary>
            /// Đối tượng lưới quản lý
            /// </summary>
            public MapGridManager.NodeGrid MyNodeGrid
            {
                get
                {
                    return this._NodeGrid;
                }
            }

            /// <summary>
            /// Đối tượng tìm đường A*
            /// </summary>
            private AStar _AStarFinder;
            /// <summary>
            /// Đối tượng tìm đường A*
            /// </summary>
            public AStar MyAStarFinder
            {
                get
                {
                    return this._AStarFinder;
                }
            }

            #region Config
            /// <summary>
            /// Tỷ lệ Drop (đơn vị %)
            /// </summary>
            public int DropRate { get; set; }

            /// <summary>
            /// Tỷ lệ nhận kinh nghiệm (đơn vị %)
            /// </summary>
            public int ExpRate { get; set; }

            /// <summary>
            /// Tỷ lệ rơi tiền (đơn vị %)
            /// </summary>
            public int MoneyRate { get; set; }

            /// <summary>
            /// Có phải phụ bản không
            /// </summary>
            public bool IsCopyScene { get; set; }

            /// <summary>
            /// Cho phép PK không tăng sát khí không
            /// </summary>
            public bool FreePK { get; set; }

            /// <summary>
            /// Cho phép PK không
            /// </summary>
            public bool AllowPK { get; set; }

            /// <summary>
            /// Cho phép mở sạp không
            /// </summary>
            public bool AllowStall { get; set; }

            /// <summary>
            /// Cho phép giao dịch không
            /// </summary>
            public bool AllowTrade { get; set; }

            /// <summary>
            /// Khi ở trạng thái PK mà chạy thì giảm thể lực bao nhiêu điểm
            /// </summary>
            public int PKAllSubStamina { get; set; }

            /// <summary>
            /// Khi ở trạng thái ngồi thì hồi sinh lực bao nhiêu điểm
            /// </summary>
            public int SitHealHP { get; set; }

            /// <summary>
            /// Khi ở trạng thái ngồi thì hồi nội lực bao nhiêu điểm
            /// </summary>
            public int SitHealMP { get; set; }

            /// <summary>
            /// Khi ở trạng thái ngồi thì hồi thể lực bao nhiêu điểm
            /// </summary>
            public int SitHealStamina { get; set; }

            /// <summary>
            /// Thời gian mỗi lần giảm sát khí đi 1 điểm
            /// <para>-1 nếu vô hiệu</para>
            /// </summary>
            public long SubPKValueTick { get; set; }

            /// <summary>
            /// Buộc chuyển trạng thái PK thành
            /// <para>-1 nếu không yêu cầu</para>
            /// </summary>
            public int ForceChangePKStatusTo { get; set; }

            /// <summary>
            /// Cho phép chủ động thay đổi trạng thái PK
            /// </summary>
            public bool AllowChangePKStatus { get; set; }

            /// <summary>
            /// Cho phép chủ động tuyên chiến với người chơi tương ứng
            /// </summary>
            public bool AllowFightTarget { get; set; }

            /// <summary>
            /// Cho phép tỷ thí không
            /// </summary>
            public bool AllowChallenge { get; set; }

            /// <summary>
            /// Cho phép sử dụng kỹ năng không
            /// </summary>
            public bool AllowUseSkill { get; set; }

            /// <summary>
            /// Cho phép chủ động sử dụng kỹ năng tấn công
            /// </summary>
            public bool AllowUseOffensiveSkill { get; set; }

            /// <summary>
            /// Danh sách kỹ năng bị cấm
            /// </summary>
            public HashSet<int> BanSkills { get; set; }

            /// <summary>
            /// Cho phép tổ đội không, nếu không có thì khi vào toàn bộ người sẽ bị xóa trạng thái đội
            /// </summary>
            public bool AllowTeam { get; set; }

            /// <summary>
            /// Cho phép tạo nhóm không
            /// </summary>
            public bool AllowCreateTeam { get; set; }

            /// <summary>
            /// Cho phép mời người chơi khác vào nhóm không
            /// </summary>
            public bool AllowInviteToTeam { get; set; }

            /// <summary>
            /// Cho phép trục xuất thành viên khỏi nhóm không
            /// </summary>
            public bool AllowKickFromTeam { get; set; }

            /// <summary>
            /// Cho phép chủ động rời nhóm không
            /// </summary>
            public bool AllowLeaveTeam { get; set; }

            /// <summary>
            /// Cho phép thành viên gia nhập nhóm không
            /// </summary>
            public bool AllowJoinTeam { get; set; }

            /// <summary>
            /// Cho phép thay đổi nhóm trưởng không
            /// </summary>
            public bool AllowChangeTeamLeader { get; set; }

            /// <summary>
            /// Cho phép dùng vật phẩm không
            /// </summary>
            public bool AllowUseItem { get; set; }

            /// <summary>
            /// Danh sách vật phẩm bị cấm sử dụng
            /// </summary>
            public HashSet<int> BanItems { get; set; }

            /// <summary>
            /// Cho phép hồi sinh tại chỗ (có thể bởi kỹ năng) không
            /// </summary>
            public bool AllowReviveAtPos { get; set; }

            /// <summary>
            /// Cho phép dùng Cửu Chuyển Hoàn Hồn Đơn không
            /// </summary>
            public bool AllowUseReviveMedicine { get; set; }
            #endregion

            #endregion


            #region Chuyển đổi

            /// <summary>
            /// Chuyển tọa độ thực X sang tọa độ lưới
            /// </summary>
            /// <param name="value"></param>
            /// <returns></returns>
            public int CorrectWidthPointToGridPoint(int value)
            {
                return ((value / this.MapGridWidth) * this.MapGridWidth + this.MapGridWidth / 2);
            }

            /// <summary>
            /// Chuyển tọa độ thực Y sang tọa độ lưới
            /// </summary>
            /// <param name="value"></param>
            /// <returns></returns>
            public int CorrectHeightPointToGridPoint(int value)
            {
                return ((value / this.MapGridHeight) * this.MapGridHeight + this.MapGridHeight / 2);
            }

            #endregion

            #region Khởi tạo

            /// <summary>
            /// Đọc dữ liệu các thành phần tạo nên bản đồ
            /// </summary>
            public void LoadComponents()
            {
                /// Tải thiết lập bản đồ
                this.LoadMapSetting();

                /// Tải danh sách vật cản, khu liên thông, khu an toàn, obs động
                this.LoadObstruction();

                /// Tải danh sách cổng dịch chuyển
                this.LoadMapTeleportDict();

                /// Tải dữ liệu bản đồ dùng cho tìm đường
                this.LoadPathFinderFast();
            }

            /// <summary>
            /// Tải thiết lập bản đồ
            /// </summary>
            private void LoadMapSetting()
            {
                string name = string.Format("MapConfig/{0}/MapSetting.xml", this.MapConfigDir);
                XElement xml = null;

                try
                {
                    xml = KTGlobal.ReadXMLData(name);
                }
                catch (Exception)
                {
                    throw new Exception(string.Format("Load XML file {0} faild", name));
                }



                this.DropRate = int.Parse(xml.Element("RateConfig").Attribute("DropRate").Value);
                this.ExpRate = int.Parse(xml.Element("RateConfig").Attribute("ExpRate").Value);
                this.MoneyRate = int.Parse(xml.Element("RateConfig").Attribute("MoneyRate").Value);

                this.IsCopyScene = bool.Parse(xml.Element("Base").Attribute("IsCopyScene").Value);
                this.FreePK = bool.Parse(xml.Element("Base").Attribute("FreePK").Value);
                this.AllowPK = bool.Parse(xml.Element("Base").Attribute("AllowPK").Value);
                this.AllowStall = bool.Parse(xml.Element("Base").Attribute("AllowStall").Value);
                this.AllowTrade = bool.Parse(xml.Element("Base").Attribute("AllowTrade").Value);
                this.PKAllSubStamina = int.Parse(xml.Element("Base").Attribute("PKAllSubStamina").Value);
                this.SitHealHP = int.Parse(xml.Element("Base").Attribute("SitHealHP").Value);
                this.SitHealMP = int.Parse(xml.Element("Base").Attribute("SitHealMP").Value);
                this.SitHealStamina = int.Parse(xml.Element("Base").Attribute("SitHealStamina").Value);

                this.SubPKValueTick = long.Parse(xml.Element("PK").Attribute("SubPKValueTick").Value);
                this.ForceChangePKStatusTo = int.Parse(xml.Element("PK").Attribute("ForceChangePKStatusTo").Value);
                this.AllowChangePKStatus = bool.Parse(xml.Element("PK").Attribute("AllowChangePKStatus").Value);
                this.AllowFightTarget = bool.Parse(xml.Element("PK").Attribute("AllowFightTarget").Value);

                this.AllowChallenge = bool.Parse(xml.Element("Challenge").Attribute("AllowChallenge").Value);

                this.AllowUseSkill = bool.Parse(xml.Element("Skill").Attribute("AllowUseSkill").Value);
                this.AllowUseOffensiveSkill = bool.Parse(xml.Element("Skill").Attribute("AllowUseOffensiveSkill").Value);
                this.BanSkills = new HashSet<int>();
                string banSkillsStr = xml.Element("Skill").Attribute("BanSkills").Value;
                if (!string.IsNullOrEmpty(banSkillsStr))
                {
                    string[] skillIDs = banSkillsStr.Split(';');
                    foreach (string idStr in skillIDs)
                    {
                        this.BanSkills.Add(int.Parse(idStr));
                    }
                }

                this.AllowTeam = bool.Parse(xml.Element("Team").Attribute("AllowTeam").Value);
                this.AllowCreateTeam = bool.Parse(xml.Element("Team").Attribute("AllowCreateTeam").Value);
                this.AllowInviteToTeam = bool.Parse(xml.Element("Team").Attribute("AllowInviteToTeam").Value);
                this.AllowKickFromTeam = bool.Parse(xml.Element("Team").Attribute("AllowKickFromTeam").Value);
                this.AllowLeaveTeam = bool.Parse(xml.Element("Team").Attribute("AllowLeaveTeam").Value);
                this.AllowJoinTeam = bool.Parse(xml.Element("Team").Attribute("AllowJoinTeam").Value);
                this.AllowChangeTeamLeader = bool.Parse(xml.Element("Team").Attribute("AllowChangeTeamLeader").Value);

                this.AllowUseItem = bool.Parse(xml.Element("Item").Attribute("AllowUseItem").Value);
                this.BanItems = new HashSet<int>();
                string banItemsStr = xml.Element("Item").Attribute("BanItems").Value;
                if (!string.IsNullOrEmpty(banItemsStr))
                {
                    string[] itemIDs = banItemsStr.Split(';');
                    foreach (string idStr in itemIDs)
                    {
                        this.BanItems.Add(int.Parse(idStr));
                    }
                }

                this.AllowReviveAtPos = bool.Parse(xml.Element("Revive").Attribute("AllowReviveAtPos").Value);
                this.AllowUseReviveMedicine = bool.Parse(xml.Element("Revive").Attribute("AllowUseReviveMedicine").Value);
            }

            /// <summary>
            /// Tải danh sách vật cản, khu liên thông, khu an toàn và Obs động
            /// </summary>
            private void LoadObstruction()
            {
                /// Kích thước bản đồ
                this.MapGridWidth = 20;
                this.MapGridHeight = 20;


                /// Số hàng và cột
                int numCols = (this.MapWidth - 1) / MapGridWidth + 1;
                int numRows = (this.MapHeight - 1) / MapGridHeight + 1;

                this.MapGridColsNum = numCols;
                this.MapGridRowsNum = numRows;

                numCols = (int) Math.Ceiling(Math.Log(numCols, 2));
                numCols = (int) Math.Pow(2, numCols);

                numRows = (int) Math.Ceiling(Math.Log(numRows, 2));
                numRows = (int) Math.Pow(2, numRows);


                /// Tạo mới bản đồ theo dạng hàng cột
                this._NodeGrid = new MapGridManager.NodeGrid(numCols, numRows);

                /// Danh sách điểm Block
                string obstructionFileName = KTGlobal.GetDataPath(string.Format("MapConfig/{0}/Obs.txt", this.MapConfigDir));
                byte[] obsBytes = File.ReadAllBytes(obstructionFileName);
                byte[,] _obsBytes = new byte[numCols, numRows];
                _obsBytes.FromBytes<byte>(obsBytes);
                this._NodeGrid.SetFixedObstruction(_obsBytes);

                /// Danh sách điểm đánh dấu khu vực liên thông
                string blurFileName = KTGlobal.GetDataPath(string.Format("MapConfig/{0}/Blur.txt", this.MapConfigDir));
                byte[] blurBytes = File.ReadAllBytes(blurFileName);
                byte[,] _blurBytes = new byte[numCols, numRows];
                _blurBytes.FromBytes<byte>(blurBytes);
                this._NodeGrid.SetBlurObstruction(_blurBytes);

                /// Danh sách điểm đánh dấu khu vực an toàn
                string saFileName = KTGlobal.GetDataPath(string.Format("MapConfig/{0}/SafeArea.txt", this.MapConfigDir));
                /// Nếu tồn tại
                if (File.Exists(saFileName))
                {
                    byte[] saBytes = File.ReadAllBytes(saFileName);
                    byte[,] _saBytes = new byte[numCols, numRows];
                    _saBytes.FromBytes<byte>(saBytes);
                    this._NodeGrid.SetSafeAreas(_saBytes);
                }

                /// Danh sách điểm đánh dấu khu vực obs động
                string dynObsFileName = KTGlobal.GetDataPath(string.Format("MapConfig/{0}/DynamicObs.txt", this.MapConfigDir));
                /// Nếu tồn tại
                if (File.Exists(dynObsFileName))
                {
                    byte[] dynObsBytes = File.ReadAllBytes(dynObsFileName);
                    byte[,] _dynObsBytes = new byte[numCols, numRows];
                    _dynObsBytes.FromBytes<byte>(dynObsBytes);
                    this._NodeGrid.SetDynamicObs(_dynObsBytes);
                }
            }

            /// <summary>
            /// Tải danh sách điểm truyền tống
            /// </summary>
            private void LoadMapTeleportDict()
            {
                string name = string.Format("MapConfig/{0}/teleports.xml", this.MapConfigDir);
                XElement xml = null;

                try
                {
                    xml = KTGlobal.ReadXMLData(name);
                }
                catch (Exception)
                {
                    throw new Exception(string.Format("Load XML file {0} faild", name));
                }

                IEnumerable<XElement> images = xml.Element("Teleports").Elements();
                if (null == images)
                    return;

                // Read the entire XML
                foreach (var image_item in images)
                {
                    int code = (int) Global.GetSafeAttributeLong(image_item, "Key");
                    int to = (int) Global.GetSafeAttributeLong(image_item, "To");
                    int toX = (int) Global.GetSafeAttributeLong(image_item, "ToX");
                    int toY = (int) Global.GetSafeAttributeLong(image_item, "ToY");
                    int x = (int) Global.GetSafeAttributeLong(image_item, "X");
                    int y = (int) Global.GetSafeAttributeLong(image_item, "Y");
                    int radius = (int) Global.GetSafeAttributeLong(image_item, "Radius");
                    int _Camp = (int) Global.GetSafeAttributeLongWithNull(image_item, "Camp");

                    MapTeleport mapTeleport = new MapTeleport()
                    {
                        Code = code,
                        MapID = -1,
                        X = x,
                        Y = y,
                        ToX = toX,
                        ToY = toY,
                        ToMapID = to,
                        Radius = radius,
                        Camp = _Camp,
                    };

                    MapTeleportDict[code] = mapTeleport;
                }

                xml = null;
            }

            /// <summary>
            /// Tải dữ liệu dùng cho tìm đường
            /// </summary>
            private void LoadPathFinderFast()
            {
                this._AStarFinder = new AStar();
            }

            #endregion

            #region Logic

            /// <summary>
            /// Kiểm tra vị trí tương ứng có thể đi vào được không (tọa độ thực)
            /// </summary>
            /// <param name="realPos"></param>
            /// <returns></returns>
            public bool CanMove(Point realPos, int copySceneID)
            {
                if (realPos.X >= MapWidth || realPos.X < 0 || realPos.Y >= MapHeight || realPos.Y < 0)
                {
                    return false;
                }

                if (!this.MyNodeGrid.CanEnter((int) (realPos.X / this.MapGridWidth), (int) (realPos.Y / this.MapGridHeight), copySceneID))
                {
                    return false;
                }

                return true;
            }

            /// <summary>
            /// Kiểm tra vị trí tương ứng có thể đi vào được không (tọa độ lưới)
            /// </summary>
            /// <param name="gridX"></param>
            /// <param name="gridY"></param>
            /// <returns></returns>
            public bool CanMove(int gridX, int gridY, int copySceneID)
            {
                if (gridX * this.MapGridWidth >= this.MapWidth || gridX < 0 || gridY * this.MapGridHeight >= this.MapHeight || gridY < 0)
                {
                    return false;
                }

                if (!this.MyNodeGrid.CanEnter(gridX, gridY, copySceneID))
                {
                    return false;
                }

                return true;
            }

            #endregion
        }
    }
}
