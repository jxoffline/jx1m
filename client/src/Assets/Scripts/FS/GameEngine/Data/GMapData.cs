//#define USE_AS3_COMPAITABLE_ASTAR

using System;
using System.Net;
using FS.Drawing;
using System.Collections.Generic;
using Server.Tools.AStarEx;
using FS.GameEngine.Logic;
using UnityEngine;
using FS.VLTK.Entities.Config;

namespace FS.GameEngine.Data
{
    /// <summary>
    /// Dữ liệu bản đồ
    /// </summary>
    public class GMapData : IDisposable
    {
        /// <summary>
        /// Dữ liệu bản đồ
        /// </summary>
        public GMapData()
        {
        }

        #region Các thành phần của bản đồ
        /// <summary>
        /// Kích thước bản đồ hiện tại (theo hình vuông, không phải kích thước thật)
        /// </summary>
        public Vector2 MapSize
        {
            get
            {
                return new Vector2(this.MapWidth, this.MapHeight);
            }
        }

        /// <summary>
        /// Kích thước thật của map
        /// </summary>
        public Vector2 RealMapSize { get; set; }

        /// <summary>
        /// Danh sách NPC sẽ hiện trên Minimap
        /// </summary>
        public List<KeyValuePair<string, Point>> MinimapNPCList { get; set; }

        /// <summary>
        /// Danh sách quái sẽ hiện trên Minimap
        /// </summary>
        public List<KeyValuePair<string, Point>> MinimapMonsterList { get; set; }

        /// <summary>
        /// Danh sách điểm thu thập sẽ hiện trên Minimap
        /// </summary>
        public List<KeyValuePair<string, Point>> MinimapGrowPointList { get; set; }

        /// <summary>
        /// Danh sách các điểm truyền tống
        /// </summary>
        public List<KeyValuePair<string, Point>> Teleport { get; set; }

        /// <summary>
        /// Danh sách NPC hiện trên bản đồ nhỏ theo tên
        /// </summary>
        public List<KeyValuePair<string, Point>> NpcList { get; set; }

        /// <summary>
        /// Danh sách NPC hiện trên bản đồ nhỏ theo ID
        /// </summary>
        public Dictionary<int, List<Point>> NpcListByID { get; set; }

        /// <summary>
        /// Danh sách Monster hiện trên bản đồ nhỏ
        /// </summary>
        public List<KeyValuePair<string, Point>> MonsterList { get; set; }

        /// <summary>
        /// Danh sách quái hiện trên bản đồ nhỏ theo ID
        /// </summary>
        public Dictionary<int, List<Point>> MonsterListByID { get; set; }

        /// <summary>
        /// Danh sách điểm thu thập hiện trên bản đồ nhỏ
        /// </summary>
        public List<KeyValuePair<string, Point>> GrowPointList { get; set; }

        /// <summary>
        /// Danh sách điểm thu thập hiện trên bản đồ nhỏ theo ID
        /// </summary>
        public Dictionary<int, List<Point>> GrowPointListByID { get; set; }

        /// <summary>
        /// Danh sách các vùng hiện trên bản đồ nhỏ
        /// </summary>
        public List<KeyValuePair<string, Point>> Zone { get; set; }

        /// <summary>
        /// Thiết lập bản đồ
        /// </summary>
        public MapSetting Setting { get; set; }

        /// <summary>
        /// Chiều rộng bản đồ
        /// </summary>
        public int MapWidth { get; set; }

        /// <summary>
        /// Chiều cao bản đồ
        /// </summary>
        public int MapHeight { get; set; }

        /// <summary>
        /// Kích thước lưới X (POT)
        /// </summary>
        public int GridSizeX { get; set; } = 10;

        /// <summary>
        /// Kích thước lưới Y (POT)
        /// </summary>
        public int GridSizeY { get; set; } = 10;

        /// <summary>
        /// Tổng số ô lưới theo chiều ngang
        /// </summary>
        public int GridSizeXNum { get; set; } = 0;

        /// <summary>
        /// Tổng số ô lưới theo chiều dọc
        /// </summary>
        public int GridSizeYNum { get; set; } = 0;

        /// <summary>
        /// Kích thước lưới X gốc
        /// </summary>
        public int OriginGridSizeXNum { get; set; }

        /// <summary>
        /// Kích thước lưới Y gốc
        /// </summary>
        public int OriginGridSizeYNum { get; set; }

        /// <summary>
        /// Vùng Block không đi được
        /// </summary>
        public byte[,] Obstructions { get; set; }

        /// <summary>
        /// Vùng làm mờ
        /// </summary>
        public byte[,] BlurPositions { get; set; }

        /// <summary>
        /// Vùng Block động được đóng mở tù ý
        /// </summary>
        public byte[,] DynamicObstructions { get; set; }

        /// <summary>
        /// Khu vực an toàn
        /// </summary>
        public byte[,] SafeAreas { get; set; }

        /// <summary>
        /// Danh sách các nhãn Obs động được mở
        /// </summary>
        public HashSet<byte> OpenedDynamicObsLabels { get; } = new HashSet<byte>();

        #endregion

        /// <summary>
        /// Hủy đối tượng
        /// </summary>
        public void Dispose()
        {
            this.MinimapNPCList?.Clear();
            this.MinimapNPCList = null;
            this.MinimapMonsterList?.Clear();
            this.MinimapMonsterList = null;
            this.MinimapGrowPointList?.Clear();
            this.MinimapGrowPointList = null;
            this.Teleport?.Clear();
            this.Teleport = null;
            this.NpcList?.Clear();
            this.NpcList = null;
            this.NpcListByID?.Clear();
            this.NpcListByID = null;
            this.MonsterList?.Clear();
            this.MonsterList = null;
            this.MonsterListByID?.Clear();
            this.MonsterListByID = null;
            this.GrowPointList?.Clear();
            this.GrowPointList = null;
            this.GrowPointListByID?.Clear();
            this.GrowPointListByID = null;
            this.Zone?.Clear();
            this.Zone = null;
            this.Setting?.Dispose();
            this.Setting = null;
            this.Obstructions = null;
            this.BlurPositions = null;
            this.DynamicObstructions = null;
            this.OpenedDynamicObsLabels.Clear();
        }
    }
}
