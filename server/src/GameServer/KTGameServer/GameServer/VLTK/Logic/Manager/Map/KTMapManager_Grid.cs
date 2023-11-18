using GameServer.Interface;
using GameServer.Logic;
using HSGameEngine.Tools.AStarEx;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace GameServer.Logic
{
    /// <summary>
    /// Quản lý bản đồ
    /// </summary>
    public static partial class KTMapManager
    {
        /// <summary>
        /// Quản lý lưới bản đồ
        /// </summary>
        public class MapGridManager
        {
            #region Define
            #region Structs
            [StructLayout(LayoutKind.Sequential, Pack = 1)]
            public struct NodeFast
            {
                #region Variables Declaration
                public double f;
                public double g;
                public double h;
                public int parentX;
                public int parentY;
                #endregion
            }
            #endregion

            /// <summary>
            /// Quản lý lưới
            /// </summary>
            public class NodeGrid
            {
                private static NodeFast[,] nodes;

                /// <summary>
                /// Mảng thông tin vật cản
                /// </summary>
                private byte[,] obs;

                /// <summary>
                /// Mảng thông tin điểm mờ ở client và khu vực liên thông ở GS
                /// </summary>
                private byte[,] blur;

                /// <summary>
                /// Mảng thông tin khu an toàn
                /// </summary>
                private byte[,] safeAreas;

                /// <summary>
                /// Mảng thông tin Obs động
                /// </summary>
                private byte[,] dynamicObs;

                /// <summary>
                /// Tổng số ô chiều dọc
                /// </summary>
                private static int numCols;

                /// <summary>
                /// Tổng số ô chiều ngang
                /// </summary>
                private static int numRows;

                /// <summary>
                /// Danh sách nhãn obs động đã được mở có thể đi vào
                /// </summary>
                private readonly Dictionary<int, HashSet<byte>> openDynamicObsLabels = new Dictionary<int, HashSet<byte>>();

                /// <summary>
                /// Khởi tạo
                /// </summary>
                /// <param name="numCols"></param>
                /// <param name="numRows"></param>
                public NodeGrid(int numCols, int numRows)
                {
                    this.SetSize(numCols, numRows);
                }

                /// <summary>
                /// Trả về danh sách vật cản
                /// </summary>
                /// <returns></returns>
                public byte[,] GetFixedObstruction()
                {
                    return this.obs;
                }

                /// <summary>
                /// Trả về danh sách khu vực liên thông
                /// </summary>
                /// <returns></returns>
                public byte[,] GetBlurObstruction()
                {
                    return this.blur;
                }

                /// <summary>
                /// Trả về danh sách các điểm khu an toàn
                /// </summary>
                /// <returns></returns>
                public byte[,] GetSafeAreas()
                {
                    return this.safeAreas;
                }

                /// <summary>
                /// Trả về danh sách các điểm Obs động
                /// </summary>
                /// <returns></returns>
                public byte[,] GetDynamicObstruction()
                {
                    return this.dynamicObs;
                }

                /// <summary>
                /// Thiết lập danh sách vật cản
                /// </summary>
                /// <param name="obs"></param>
                public void SetFixedObstruction(byte[,] obs)
                {
                    this.obs = obs;
                }

                /// <summary>
                /// Thiết lập danh sách các khu vực liên thông
                /// </summary>
                /// <param name="blurs"></param>
                public void SetBlurObstruction(byte[,] blurs)
                {
                    this.blur = blurs;
                }

                /// <summary>
                /// Thiết lập danh sách các điểm khu an toàn
                /// </summary>
                /// <param name="safeAreas"></param>
                public void SetSafeAreas(byte[,] safeAreas)
                {
                    this.safeAreas = safeAreas;
                }

                /// <summary>
                /// Thiết lập danh sách các điểm Obs động
                /// </summary>
                /// <param name="dynObs"></param>
                public void SetDynamicObs(byte[,] dynObs)
                {
                    this.dynamicObs = dynObs;
                }

                /// <summary>
                /// Mở nhãn obs động tương ứng
                /// </summary>
                /// <param name="copySceneID"></param>
                /// <param name="label"></param>
                public void OpenDynamicObsLabel(int copySceneID, byte label)
                {
                    /// Nếu chưa tồn tại
                    if (!this.openDynamicObsLabels.ContainsKey(copySceneID))
                    {
                        /// Tạo mới
                        this.openDynamicObsLabels[copySceneID] = new HashSet<byte>();
                    }
                    /// Thêm vào
                    this.openDynamicObsLabels[copySceneID].Add(label);
                }

                /// <summary>
                /// Xóa toàn bộ nhãn Obs động tương ứng
                /// </summary>
                /// <param name="copySceneID"></param>
                public void ClearDynamicObsLabels(int copySceneID)
                {
                    
                    /// Xóa
                    this.openDynamicObsLabels.Remove(copySceneID);
                }

                /// <summary>
                /// Đóng nhãn obs động tương ứng
                /// </summary>
                /// <param name="copySceneID"></param>
                /// <param name="label"></param>
                public void CloseDynamicObsLabel(int copySceneID, byte label)
                {
                    /// Nếu chưa tồn tại
                    if (!this.openDynamicObsLabels.ContainsKey(copySceneID))
                    {
                        /// Bỏ qua
                        return;
                    }
                    /// Xóa
                    this.openDynamicObsLabels.Remove(label);
                }

                /// <summary>
                /// Close tryFix
                /// </summary>
                /// <param name="copySceneID"></param>
                /// <param name="label"></param>
                public void CloseDynamicObsLabelTry(int copySceneID, byte label)
                {
                    /// Nếu chưa tồn tại
                    if (!this.openDynamicObsLabels.ContainsKey(copySceneID))
                    {
                        /// Bỏ qua
                        return;
                    }
                    /// Xóa
                    this.openDynamicObsLabels.TryGetValue(copySceneID,out HashSet<byte> value);
                    if(value!=null)
                    {
                        value.Remove(label);
                    }    
                }


                /// <summary>
                /// Thiết lập kích thước
                /// </summary>
                /// <param name="numCols"></param>
                /// <param name="numRows"></param>
                private void SetSize(int numCols, int numRows)
                {
                    if (NodeGrid.nodes == null || NodeGrid.numCols < numCols || NodeGrid.numRows < numRows)
                    {
                        NodeGrid.numCols = Math.Max(numCols, NodeGrid.numCols);
                        NodeGrid.numRows = Math.Max(numRows, NodeGrid.numRows);

                        NodeGrid.nodes = new NodeFast[NodeGrid.numCols, NodeGrid.numRows];
                    }

                    /// Tạo mới Obs
                    this.obs = new byte[numCols, numRows];

                    /// Duyệt 2 chiều
                    for (int i = 0; i < numCols; i++)
                    {
                        for (int j = 0; j < numRows; j++)
                        {
                            /// Khởi tạo dữ liệu mặc định của Obs
                            this.obs[i, j] = 1;
                        }
                    }
                }

                /// <summary>
                /// Làm rỗng danh sách Node
                /// </summary>
                public void Clear()
                {
                    Array.Clear(NodeGrid.nodes, 0, NodeGrid.nodes.Length);
                }

                /// <summary>
                /// Danh sách Node
                /// </summary>
                public NodeFast[,] Nodes
                {
                    get
                    {
                        return NodeGrid.nodes;
                    }
                }

                /// <summary>
                /// Trả về danh sách các nhãn khu Obs động đã được mở
                /// </summary>
                /// <param name="copySceneID"></param>
                /// <returns></returns>
                public byte[] GetOpenDynamicObsLabels(int copySceneID)
                {
                    /// Nếu tồn tại
                    if (this.openDynamicObsLabels.TryGetValue(copySceneID, out HashSet<byte> data))
                    {
                        return data.ToArray();
                    }
                    /// Nếu không tồn tại
                    else
                    {
                        return new byte[0];
                    }
                }

                /// <summary>
                /// Kiểm tra 2 Node có đi được không
                /// </summary>
                /// <param name="node1"></param>
                /// <param name="node2"></param>
                /// <returns></returns>
                public bool IsDiagonalWalkable(long node1, long node2)
                {
                    int node1x = ANode.GetGUID_X(node1);
                    int node1y = ANode.GetGUID_Y(node1);

                    int node2x = ANode.GetGUID_X(node2);
                    int node2y = ANode.GetGUID_Y(node2);

                    if (this.obs[node1x, node2y] == 1 && this.obs[node2x, node1y] == 1)
                    {
                        return true;
                    }
                    return false;
                }

                /// <summary>
                /// Kiểm tra vị trí tương ứng có nằm trong khu Obs động không
                /// </summary>
                /// <param name="x"></param>
                /// <param name="y"></param>
                /// <param name="copySceneID"></param>
                /// <returns></returns>
                public bool InDynamicObs(int x, int y, int copySceneID)
                {
                    if (x < 0 || y < 0)
                    {
                        return false;
                    }
                    else if (x >= this.obs.GetUpperBound(0) || y >= this.obs.GetUpperBound(1))
                    {
                        return false;
                    }

                    /// Nếu có Obs động
                    if (this.dynamicObs != null && this.openDynamicObsLabels.TryGetValue(copySceneID, out HashSet<byte> dynObs))
                    {
                        /// Nếu Obs động này chưa được mở
                        if (dynamicObs[x, y] > 0 && !dynObs.Contains(this.dynamicObs[x, y]))
                        {
                            return true;
                        }
                    }

                    /// Không nằm trong khu Obs động
                    return false;
                }

                /// <summary>
                /// Kiểm tra vị trí tương ứng có thể đến được không
                /// </summary>
                /// <param name="x"></param>
                /// <param name="y"></param>
                /// <param name="copySceneID"></param>
                /// <returns></returns>
                public bool CanEnter(int x, int y, int copySceneID)
                {
                    if (x < 0 || y < 0)
                    {
                        return false;
                    }
                    else if (x >= this.obs.GetUpperBound(0) || y >= this.obs.GetUpperBound(1))
                    {
                        return false;
                    }

                    /// Nếu là điểm Block
                    if (this.obs[x, y] == 0)
                    {
                        return false;
                    }

                    /// Nếu có Obs động
                    if (this.dynamicObs != null && this.openDynamicObsLabels.TryGetValue(copySceneID, out HashSet<byte> dynObs))
                    {
                        /// Nếu Obs động này chưa được mở
                        if (dynamicObs[x, y] > 0 && !dynObs.Contains(this.dynamicObs[x, y]))
                        {
                            return false;
                        }
                    }

                    /// Có thể đến được
                    return true;
                }

                /// <summary>
                /// Kiểm tra vị trí tương úng có nằm trong vùng an toàn không
                /// </summary>
                /// <param name="gridX"></param>
                /// <param name="gridY"></param>
                /// <returns></returns>
                public bool InSafeArea(int gridX, int gridY)
                {
                    /// Nếu không có vùng an toàn
                    if (this.safeAreas == null)
                    {
                        return false;
                    }
                    /// Quá phạm vi
                    else if (gridX < 0 || gridY < 0)
                    {
                        return false;
                    }
                    else if (gridX >= this.safeAreas.GetUpperBound(0) || gridY >= this.safeAreas.GetUpperBound(1))
                    {
                        return false;
                    }

                    /// Trả về kết quả
                    return this.safeAreas[gridX, gridY] == 1;
                }

                /// <summary>
                /// Kiểm tra có đường đi giữa 2 nút cho trước không
                /// </summary>
                /// <param name="fromPos"></param>
                /// <param name="toPos"></param>
                /// <returns></returns>
                public bool HasPath(Point fromPos, Point toPos)
                {
                    return this.blur[(int) fromPos.X, (int) fromPos.Y] / 2 == this.blur[(int) toPos.X, (int) toPos.Y] / 2;
                }

                /**
                 * Returns the number of columns in the grid.
                 */
                public int NumCols
                {
                    get { return NodeGrid.numCols; }
                }

                /**
                 * Returns the number of rows in the grid.
                 */
                public int NumRows
                {
                    get { return NodeGrid.numRows; }
                }
            }

            /// <summary>
            /// Danh sách các đối tượng trong bản đồ
            /// </summary>
            public struct MapGridSpriteItem
            {
                /// <summary>
                /// Đối tượng dùng để sử dụng khóa LOCK
                /// </summary>
                public object GridLock { get; set; }

                /// <summary>
                /// Danh sách đối tượng
                /// </summary>
                public List<IObject> ObjsList { get; set; }

                /// <summary>
                /// Tổng số đối tượng người chơi
                /// </summary>
                public short RoleNum { get; set; }

                /// <summary>
                /// Tổng số đối tượng quái
                /// </summary>
                public short MonsterNum { get; set; }

                /// <summary>
                /// Tổng số NPC
                /// </summary>
                public short NPCNum { get; set; }

                /// <summary>
                /// Tổng số vật phẩm
                /// </summary>
                public short GoodsPackNum { get; set; }

                /// <summary>
                /// Tổng số bẫy
                /// </summary>
                public short TrapNum { get; set; }

                /// <summary>
                /// Tổng số điểm thu thập
                /// </summary>
                public short GrowPointNum { get; set; }

                /// <summary>
                /// Tổng số khu vực động
                /// </summary>
                public short DynamicAreaNum { get; set; }

                /// <summary>
                /// Tổng số BOT
                /// </summary>
                public short BotNum { get; set; }

                /// <summary>
                /// Tổng số Pet
                /// </summary>
                public short PetNum { get; set; }

                /// <summary>
                /// Tổng số xe tiêu
                /// </summary>
                public short TraderCarriageNum { get; set; }

                /// <summary>
                /// Tổng số BOT bán hàng
                /// </summary>
                public short StallBotNum { get; set; }
            }

            /// <summary>
            /// Quản lý bản đồ dạng lưới
            /// </summary>
            public class MapGrid
            {
                /// <summary>
                /// Đối tượng quản lý bản đồ
                /// </summary>
                /// <param name="mapCode"></param>
                /// <param name="mapWidth"></param>
                /// <param name="mapHeight"></param>
                /// <param name="mapGridWidth"></param>
                /// <param name="mapGridHeight"></param>
                /// <param name="gameMap"></param>
                public MapGrid(int mapCode, int mapWidth, int mapHeight, int mapGridWidth, int mapGridHeight)
                {
                    this._MapCode = mapCode;

                    this.MapWidth = mapWidth;
                    this.MapHeight = mapHeight;
                    this._MapGridWidth = mapGridWidth;
                    this._MapGridHeight = mapGridHeight;

                    this._MapGridXNum = (this.MapWidth - 1) / this._MapGridWidth + 1;
                    this._MapGridYNum = (this.MapHeight - 1) / this._MapGridHeight + 1;
                    this._MapGridTotalNum = this._MapGridXNum * this._MapGridYNum;

                    this.MyMapGridSpriteItem = new MapGridSpriteItem[this._MapGridTotalNum];
                    for (int i = 0; i < this.MyMapGridSpriteItem.Length; i++)
                    {
                        this.MyMapGridSpriteItem[i].GridLock = new object();
                        this.MyMapGridSpriteItem[i].ObjsList = new List<IObject>();
                    }
                }

                /// <summary>
                /// ID bản đồ
                /// </summary>
                private readonly int _MapCode;

                /// <summary>
                /// Chiều rộng
                /// </summary>
                private readonly int MapWidth;

                /// <summary>
                /// Chiều cao
                /// </summary>
                private readonly int MapHeight;

                /// <summary>
                /// Chiều rộng kích thước lưới
                /// </summary>
                private readonly int _MapGridWidth;

                /// <summary>
                /// ID bản đồ
                /// </summary>
                public int MapCode
                {
                    get
                    {
                        return this._MapCode;
                    }
                }

                /// <summary>
                /// Chiều rộng kích thước lưới
                /// </summary>
                public int MapGridWidth
                {
                    get { return _MapGridWidth; }
                }

                /// <summary>
                /// Chiều dài kích thước lưới
                /// </summary>
                private readonly int _MapGridHeight;

                /// <summary>
                /// Chiều dài kích thước lưới
                /// </summary>
                public int MapGridHeight
                {
                    get { return _MapGridHeight; }
                }

                /// <summary>
                /// Số điểm theo trục X trong lưới
                /// </summary>
                private readonly int _MapGridXNum = 0;

                /// <summary>
                /// Số điểm theo trục X trong lưới
                /// </summary>
                public int MapGridXNum
                {
                    get { return _MapGridXNum; }
                }

                /// <summary>
                /// Số điểm theo trục Y trong lưới
                /// </summary>
                private readonly int _MapGridYNum = 0;

                /// <summary>
                /// Số điểm theo trục Y trong lưới
                /// </summary>
                public int MapGridYNum
                {
                    get { return _MapGridYNum; }
                }

                private int _MapGridTotalNum = 0;

                /// <summary>
                /// ID đối tượng tương ứng
                /// </summary>
                private readonly ConcurrentDictionary<IObject, int> _Obj2GridDict = new ConcurrentDictionary<IObject, int>();

                /// <summary>
                /// Danh sách đối tượng trong lưới
                /// </summary>
                private readonly MapGridSpriteItem[] MyMapGridSpriteItem = null;

                /// <summary>
                /// Trả về vị trí trong danh sách lưới
                /// </summary>
                /// <param name="gridX"></param>
                /// <param name="gridY"></param>
                /// <returns></returns>
                private int GetGridIndex(int gridX, int gridY)
                {
                    return (this._MapGridXNum * gridY) + gridX;
                }

                /// <summary>
                /// Thay đổi số lượng đối tượng trong lưới
                /// </summary>
                /// <param name="key"></param>
                /// <param name="obj"></param>
                /// <param name="addOrSubNum"></param>
                private void ChangeMapGridsSpriteNum(int index, IObject obj, short addOrSubNum)
                {
                    switch (obj.ObjectType)
                    {
                        case ObjectTypes.OT_CLIENT:
                            this.MyMapGridSpriteItem[index].RoleNum += addOrSubNum;
                            this.MyMapGridSpriteItem[index].RoleNum = Math.Max((short) 0, this.MyMapGridSpriteItem[index].RoleNum);
                            break;
                        case ObjectTypes.OT_MONSTER:
                            this.MyMapGridSpriteItem[index].MonsterNum += addOrSubNum;
                            this.MyMapGridSpriteItem[index].MonsterNum = Math.Max((short) 0, this.MyMapGridSpriteItem[index].MonsterNum);
                            break;
                        case ObjectTypes.OT_NPC:
                            this.MyMapGridSpriteItem[index].NPCNum += addOrSubNum;
                            this.MyMapGridSpriteItem[index].NPCNum = Math.Max((short) 0, this.MyMapGridSpriteItem[index].NPCNum);
                            break;
                        case ObjectTypes.OT_GOODSPACK:
                            this.MyMapGridSpriteItem[index].GoodsPackNum += addOrSubNum;
                            this.MyMapGridSpriteItem[index].GoodsPackNum = Math.Max((short) 0, this.MyMapGridSpriteItem[index].GoodsPackNum);
                            break;
                        case ObjectTypes.OT_TRAP:
                            this.MyMapGridSpriteItem[index].TrapNum += addOrSubNum;
                            this.MyMapGridSpriteItem[index].TrapNum = Math.Max((short) 0, this.MyMapGridSpriteItem[index].TrapNum);
                            break;
                        case ObjectTypes.OT_GROWPOINT:
                            this.MyMapGridSpriteItem[index].GrowPointNum += addOrSubNum;
                            this.MyMapGridSpriteItem[index].GrowPointNum = Math.Max((short) 0, this.MyMapGridSpriteItem[index].GrowPointNum);
                            break;
                        case ObjectTypes.OT_DYNAMIC_AREA:
                            this.MyMapGridSpriteItem[index].DynamicAreaNum += addOrSubNum;
                            this.MyMapGridSpriteItem[index].DynamicAreaNum = Math.Max((short) 0, this.MyMapGridSpriteItem[index].DynamicAreaNum);
                            break;
                        case ObjectTypes.OT_BOT:
                            this.MyMapGridSpriteItem[index].BotNum += addOrSubNum;
                            this.MyMapGridSpriteItem[index].BotNum = Math.Max((short) 0, this.MyMapGridSpriteItem[index].BotNum);
                            break;
                        case ObjectTypes.OT_PET:
                            this.MyMapGridSpriteItem[index].PetNum += addOrSubNum;
                            this.MyMapGridSpriteItem[index].PetNum = Math.Max((short) 0, this.MyMapGridSpriteItem[index].PetNum);
                            break;
                        case ObjectTypes.OT_TRADER_CARRIAGE:
                            this.MyMapGridSpriteItem[index].TraderCarriageNum += addOrSubNum;
                            this.MyMapGridSpriteItem[index].TraderCarriageNum = Math.Max((short) 0, this.MyMapGridSpriteItem[index].TraderCarriageNum);
                            break;
                        case ObjectTypes.OT_STALLBOT:
                            this.MyMapGridSpriteItem[index].StallBotNum += addOrSubNum;
                            this.MyMapGridSpriteItem[index].StallBotNum = Math.Max((short) 0, this.MyMapGridSpriteItem[index].StallBotNum);
                            break;
                    }
                }


                /// <summary>
                /// Di chuyển đối tượng trên bản đồ
                /// </summary>
                /// <param name="newX">Tọa độ thực mới X</param>
                /// <param name="newY">Tọa độ thực mới Y</param>
                public bool MoveObject(int newX, int newY, IObject obj)
                {
                    if (newX < 0 || newY < 0 || newX >= MapWidth || newY >= MapHeight)
                    {
                        return false;
                    }

                    int gridX = newX / this._MapGridWidth;
                    int gridY = newY / this._MapGridHeight;
                    int oldGridIndex;
                    if (!this._Obj2GridDict.TryGetValue(obj, out oldGridIndex))
                    {
                        oldGridIndex = -1;
                    }

                    int gridIndex = this.GetGridIndex(gridX, gridY);
                    if (-1 != oldGridIndex && oldGridIndex == gridIndex)
                    {
                        return true;
                    }

                    if (-1 != oldGridIndex)
                    {
                        lock (this.MyMapGridSpriteItem[oldGridIndex].GridLock)
                        {
                            if (!this.MyMapGridSpriteItem[oldGridIndex].ObjsList.Remove(obj))
                            {
                                return false;
                            }

                            this.ChangeMapGridsSpriteNum(oldGridIndex, obj, -1);
                        }
                    }

                    lock (this.MyMapGridSpriteItem[gridIndex].GridLock)
                    {
                        this.MyMapGridSpriteItem[gridIndex].ObjsList.Add(obj);
                        this.ChangeMapGridsSpriteNum(gridIndex, obj, 1);
                    }

                    this._Obj2GridDict[obj] = gridIndex;

                    return true;
                }

                /// <summary>
                /// Xóa đối tượng khỏi lưới
                /// </summary>
                /// <param name="newX"></param>
                /// <param name="newY"></param>
                /// <param name="obj"></param>
                public bool RemoveObject(IObject obj)
                {
                    int oldGridIndex = -1;
                    if (!this._Obj2GridDict.TryGetValue(obj, out oldGridIndex))
                    {
                        oldGridIndex = -1;
                    }
                    else
                    {
                        this._Obj2GridDict.TryRemove(obj, out _);
                    }

                    if (-1 == oldGridIndex)
                        return false;

                    lock (this.MyMapGridSpriteItem[oldGridIndex].GridLock)
                    {
                        if (this.MyMapGridSpriteItem[oldGridIndex].ObjsList.Remove(obj))
                        {
                            this.ChangeMapGridsSpriteNum(oldGridIndex, obj, -1);
                        }
                    }

                    return true;
                }

                /// <summary>
                /// Tìm đối tượng tương ứng trong bản đồ tại vị trí tương ứng
                /// </summary>
                /// <param name="gridX">Tọa độ lưới X</param>
                /// <param name="gridY">Tọa độ lưới Y</param>
                public List<IObject> FindObjects(int gridX, int gridY)
                {
                    int gridIndex = (_MapGridXNum * gridY) + gridX;
                    if (gridIndex < 0 || gridIndex >= _MapGridTotalNum)
                    {
                        return null;
                    }

                    List<IObject> listObjs2 = null;

                    lock (this.MyMapGridSpriteItem[gridIndex].GridLock)
                    {
                        listObjs2 = this.MyMapGridSpriteItem[gridIndex].ObjsList;
                        if (listObjs2.Count == 0)
                        {
                            return null;
                        }

                        listObjs2 = listObjs2.GetRange(0, listObjs2.Count);
                    }

                    return listObjs2;
                }

                /// <summary>
                /// Tìm đối tượng xung quanh vị trí
                /// </summary>
                /// <param name="toX">Tọa độ thực X</param>
                /// <param name="toY">Tọa độ thực Y</param>
                /// <param name="radius">Phạm vi xung quanh</param>
                /// <param name="sortList">Sắp xếp danh sách theo khoảng cách tăng dần</param>
                /// <returns></returns>
                public List<IObject> FindObjects(int toX, int toY, int radius, bool sortList = false)
                {
                    if (toX < 0 || toY < 0 || toX >= this.MapWidth || toY >= this.MapHeight)
                    {
                        return null;
                    }

                    int gridX = toX / this._MapGridWidth;
                    int gridY = toY / this._MapGridHeight;

                    List<IObject> listObjs = new List<IObject>();
                    List<IObject> listObjs2 = null;

                    int gridRadiusWidthNum = ((radius - 1) / this._MapGridWidth) + 1;
                    int gridRadiusHeightNum = ((radius - 1) / this._MapGridHeight) + 1;

                    int lowGridY = gridY - gridRadiusHeightNum;
                    int hiGridY = gridY + gridRadiusHeightNum;

                    int lowGridX = gridX - gridRadiusWidthNum;
                    int hiGridX = gridX + gridRadiusWidthNum;

                    for (int y = lowGridY; y <= hiGridY; y++)
                    {
                        for (int x = lowGridX; x <= hiGridX; x++)
                        {
                            listObjs2 = this.FindObjects(x, y);
                            if (null != listObjs2)
                            {
                                listObjs.AddRange(listObjs2);
                            }
                        }
                    }

                    /// Nếu có yêu cầu sắp xếp danh sách tăng dần theo khoảng cách
                    if (sortList)
                    {
                        /// Vị trí đang tìm kiếm
                        UnityEngine.Vector2 pos = new UnityEngine.Vector2(toX, toY);

                        /// Sắp xếp danh sách theo khoảng cách tăng dần
                        listObjs.Sort((obj1, obj2) => {
                            try
                            {
                                IObject o1 = obj1;
                                IObject o2 = obj2;

                                /// Toác đéo gì đó
                                if (o1 == null || o2 == null)
                                {
                                    return 99999999;
                                }

                                UnityEngine.Vector2 o1Pos = new UnityEngine.Vector2((int) o1.CurrentPos.X, (int) o1.CurrentPos.Y);
                                UnityEngine.Vector2 o2Pos = new UnityEngine.Vector2((int) o2.CurrentPos.X, (int) o2.CurrentPos.Y);

                                return (int) (UnityEngine.Vector2.Distance(pos, o1Pos) - UnityEngine.Vector2.Distance(pos, o2Pos));
                            }
                            catch (Exception)
                            {
                                return 99999999;
                            }
                        });
                    }

                    return listObjs;
                }
            }
            #endregion
        }
    }
}
