using GameServer.Interface;
using GameServer.KiemThe.Utilities;
using GameServer.Logic;
using System.Collections.Generic;
using static GameServer.Logic.KTMapManager;

namespace GameServer.KiemThe
{
    /// <summary>
    /// Các phương thức và đối tượng toàn cục của Kiếm Thế
    /// </summary>
    public static partial class KTGlobal
    {
        #region Tìm đối tượng xung quanh
        /// <summary>
        /// Trả về danh sách đối tượng xung quanh
        /// </summary>
        /// <param name="mapCode"></param>
        /// <param name="copyMap"></param>
        /// <param name="pos"></param>
        /// <param name="distance"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static List<T> GetNearByObjectsAtPos<T>(int mapCode, int copyMap, UnityEngine.Vector2 pos, float distance, int count = -1) where T : GameObject
        {
            List<T> list = new List<T>();

            HashSet<string> mark = new HashSet<string>();
            HashSet<object> markObject = new HashSet<object>();
            /// Bản đồ hiện tại
            GameMap map = KTMapManager.Find(mapCode);

            if (map != null)
            {
                UnityEngine.Vector2 gridPos = KTGlobal.WorldPositionToGridPosition(map, pos);

                /// Nếu tọa độ này chưa được tìm
                if (!mark.Contains(gridPos.ToString()))
                {
                    /// Đánh dấu đã tìm quanh tọa độ này
                    mark.Add(gridPos.ToString());

                    /// Danh sách đối tượng xung quanh
                    List<IObject> objectsAround = map.Grid.FindObjects((int) pos.x, (int) pos.y, (int) distance, count != -1);

                    if (objectsAround != null)
                    {
                        /// Duyệt toàn bộ danh sách đối tượng xung quanh
                        foreach (IObject obj in objectsAround)
                        {
                            /// Nếu đối tượng chưa được xét
                            if (!markObject.Contains(obj))
                            {
                                /// Nếu là đối tượng GameObject
                                if (obj is T)
                                {
                                    T target = obj as T;

                                    /// Nếu không cùng phụ bản
                                    if (target.CurrentCopyMapID != copyMap)
                                    {
                                        continue;
                                    }

                                    /// Vị trí đối tượng
                                    UnityEngine.Vector2 targetPos = new UnityEngine.Vector2((int) target.CurrentPos.X, (int) target.CurrentPos.Y);

                                    /// Khoảng cách từ vị trí của đối tượng tới điểm
                                    float distanceToPoint = UnityEngine.Vector2.Distance(pos, targetPos);

                                    /// Nếu đối tượng nằm trong vùng ảnh hưởng
                                    if (distanceToPoint <= distance)
                                    {
                                        /// Thêm đối tượng vào danh sách
                                        list.Add(target);
                                        /// Đánh dấu đã xét đối tượng
                                        markObject.Add(obj);

                                        /// Nếu số mục tiêu đã vượt quá giới hạn tìm kiếm
                                        if (count != -1 && list.Count >= count)
                                        {
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return list;
        }

        /// <summary>
        /// Trả về danh sách đối tượng xung quanh
        /// </summary>
        /// <param name="go"></param>
        /// <param name="distance"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static List<T> GetNearByObjects<T>(GameObject go, float distance, int count = -1) where T : GameObject
        {
            List<T> list = new List<T>();

            UnityEngine.Vector2 pos = new UnityEngine.Vector2((int) go.CurrentPos.X, (int) go.CurrentPos.Y);

            HashSet<string> mark = new HashSet<string>();
            HashSet<object> markObject = new HashSet<object>();
            /// Bản đồ hiện tại
            GameMap map = KTMapManager.Find(go.CurrentMapCode);

            if (map != null)
            {
                UnityEngine.Vector2 gridPos = KTGlobal.WorldPositionToGridPosition(map, pos);

                /// Nếu tọa độ này chưa được tìm
                if (!mark.Contains(gridPos.ToString()))
                {
                    /// Đánh dấu đã tìm quanh tọa độ này
                    mark.Add(gridPos.ToString());

                    /// Danh sách đối tượng xung quanh
                    List<IObject> objectsAround = map.Grid.FindObjects((int) pos.x, (int) pos.y, (int) distance, count != -1);

                    if (objectsAround != null)
                    {
                        /// Duyệt toàn bộ danh sách đối tượng xung quanh
                        foreach (IObject obj in objectsAround)
                        {
                            /// Nếu đối tượng chưa được xét
                            if (!markObject.Contains(obj))
                            {
                                /// Nếu là đối tượng GameObject
                                if (obj is T)
                                {
                                    T target = obj as T;

                                    /// Nếu không trong cùng phụ bản
                                    if (target.CurrentCopyMapID != go.CurrentCopyMapID)
                                    {
                                        continue;
                                    }

                                    /// Vị trí đối tượng
                                    UnityEngine.Vector2 targetPos = new UnityEngine.Vector2((int) target.CurrentPos.X, (int) target.CurrentPos.Y);

                                    /// Khoảng cách từ vị trí của đối tượng tới điểm
                                    float distanceToPoint = UnityEngine.Vector2.Distance(pos, targetPos);

                                    /// Nếu đối tượng nằm trong vùng ảnh hưởng
                                    if (distanceToPoint <= distance)
                                    {
                                        /// Thêm đối tượng vào danh sách
                                        list.Add(target);
                                        /// Đánh dấu đã xét đối tượng
                                        markObject.Add(obj);

                                        /// Nếu số mục tiêu đã vượt quá giới hạn tìm kiếm
                                        if (count != -1 && list.Count >= count)
                                        {
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return list;
        }

        /// <summary>
        /// Trả về danh sách người chơi xung quanh
        /// </summary>
        /// <param name="go"></param>
        /// <param name="distance"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static List<KPlayer> GetNearByPlayers(GameObject go, float distance, int count = -1)
        {
            List<KPlayer> list = new List<KPlayer>();

            UnityEngine.Vector2 pos = new UnityEngine.Vector2((int) go.CurrentPos.X, (int) go.CurrentPos.Y);

            HashSet<string> mark = new HashSet<string>();
            HashSet<object> markObject = new HashSet<object>();
            /// Bản đồ hiện tại
            GameMap map = KTMapManager.Find(go.CurrentMapCode);

            if (map != null)
            {
                UnityEngine.Vector2 gridPos = KTGlobal.WorldPositionToGridPosition(map, pos);

                /// Nếu tọa độ này chưa được tìm
                if (!mark.Contains(gridPos.ToString()))
                {
                    /// Đánh dấu đã tìm quanh tọa độ này
                    mark.Add(gridPos.ToString());

                    /// Danh sách đối tượng xung quanh
                    List<IObject> objectsAround = map.Grid.FindObjects((int) pos.x, (int) pos.y, (int) distance, count != -1);

                    if (objectsAround != null)
                    {
                        /// Duyệt toàn bộ danh sách đối tượng xung quanh
                        foreach (IObject obj in objectsAround)
                        {
                            /// Nếu đối tượng chưa được xét
                            if (!markObject.Contains(obj))
                            {
                                /// Nếu là đối tượng GameObject
                                if (obj is KPlayer)
                                {
                                    KPlayer target = obj as KPlayer;

                                    /// Nếu không trong cùng phụ bản
                                    if (target.CurrentCopyMapID != go.CurrentCopyMapID)
                                    {
                                        continue;
                                    }

                                    /// Vị trí đối tượng
                                    UnityEngine.Vector2 targetPos = new UnityEngine.Vector2((int) target.CurrentPos.X, (int) target.CurrentPos.Y);

                                    /// Khoảng cách từ vị trí của đối tượng tới điểm
                                    float distanceToPoint = UnityEngine.Vector2.Distance(pos, targetPos);

                                    /// Nếu đối tượng nằm trong vùng ảnh hưởng
                                    if (distanceToPoint <= distance)
                                    {
                                        /// Thêm đối tượng vào danh sách
                                        list.Add(target);
                                        /// Đánh dấu đã xét đối tượng
                                        markObject.Add(obj);

                                        /// Nếu số mục tiêu đã vượt quá giới hạn tìm kiếm
                                        if (count != -1 && list.Count >= count)
                                        {
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return list;
        }

        /// <summary>
        /// Trả về danh sách đội viên xung quanh
        /// </summary>
        /// <param name="player"></param>
        /// <param name="distance"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static List<KPlayer> GetNearByTeammates(KPlayer player, float distance, int count = -1)
        {
            List<KPlayer> list = new List<KPlayer>();

            /// Vị trí bản thân
            UnityEngine.Vector2 objPos = new UnityEngine.Vector2((float) player.CurrentPos.X, (float) player.CurrentPos.Y);

            /// Duyệt toàn bộ danh sách đội viên trong nhóm
            foreach (KPlayer teammate in player.Teammates)
            {
                /// Nếu không trong cùng phụ bản
                if (teammate.CurrentCopyMapID != player.CurrentCopyMapID)
                {
                    continue;
                }

                /// Nếu nằm chung bản đồ
                if (teammate.CurrentMapCode == player.CurrentMapCode)
                {
                    UnityEngine.Vector2 targetPos = new UnityEngine.Vector2((int) teammate.CurrentPos.X, (int) teammate.CurrentPos.Y);
                    /// Nếu nằm trong vùng tìm kiếm
                    if (UnityEngine.Vector2.Distance(objPos, targetPos) <= distance)
                    {
                        list.Add(teammate);
                    }

                    /// Nếu số mục tiêu đã vượt quá giới hạn tìm kiếm
                    if (count != -1 && list.Count >= count)
                    {
                        break;
                    }
                }
            }

            return list;
        }

        /// <summary>
        /// Trả về danh sách đối tượng xung quanh có chung CampID
        /// </summary>
        /// <param name="go"></param>
        /// <param name="distance"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static List<GameObject> GetNearBySameCampObject(GameObject go, float distance, int count = -1)
        {
            List<GameObject> list = new List<GameObject>();

            HashSet<string> mark = new HashSet<string>();
            HashSet<object> markObject = new HashSet<object>();
            /// Bản đồ hiện tại
            GameMap map = KTMapManager.Find(go.CurrentMapCode);

            /// Vị trí bản thân
            UnityEngine.Vector2 objPos = new UnityEngine.Vector2((float) go.CurrentPos.X, (float) go.CurrentPos.Y);

            if (map != null)
            {
                UnityEngine.Vector2 pos = new UnityEngine.Vector2((float) go.CurrentPos.X, (float) go.CurrentPos.Y);
                UnityEngine.Vector2 gridPos = KTGlobal.WorldPositionToGridPosition(map, pos);

                /// Nếu tọa độ này chưa được tìm
                if (!mark.Contains(gridPos.ToString()))
                {
                    /// Đánh dấu đã tìm quanh tọa độ này
                    mark.Add(gridPos.ToString());

                    /// Danh sách đối tượng xung quanh
                    List<IObject> objectsAround = map.Grid.FindObjects((int) pos.x, (int) pos.y, (int) distance, count != -1);

                    if (objectsAround != null)
                    {
                        /// Duyệt toàn bộ danh sách đối tượng xung quanh
                        foreach (IObject obj in objectsAround)
                        {
                            /// Nếu đối tượng chưa được xét
                            if (!markObject.Contains(obj))
                            {
                                /// Nếu là đối tượng GameObject
                                if (obj is GameObject)
                                {
                                    GameObject target = obj as GameObject;

                                    /// Nếu không trong cùng phụ bản
                                    if (target.CurrentCopyMapID != go.CurrentCopyMapID)
                                    {
                                        continue;
                                    }

                                    /// Vị trí đối tượng
                                    UnityEngine.Vector2 targetPos = new UnityEngine.Vector2((int) target.CurrentPos.X, (int) target.CurrentPos.Y);

                                    /// Có phải kẻ địch không
                                    bool isOpposite = KTGlobal.CanAttack(go, target);
                                    /// Khoảng cách từ vị trí của đối tượng tới điểm
                                    float distanceToPoint = UnityEngine.Vector2.Distance(objPos, targetPos);

                                    /// Nếu đối tượng không phải là kẻ địch
                                    if (!target.IsDead() && !isOpposite)
                                    {
                                        /// Nếu đối tượng nằm trong vùng ảnh hưởng
                                        if (distanceToPoint <= distance)
                                        {
                                            /// Thêm đối tượng vào danh sách
                                            list.Add(target);
                                            /// Đánh dấu đã xét đối tượng
                                            markObject.Add(obj);

                                            /// Nếu số mục tiêu đã vượt quá giới hạn tìm kiếm
                                            if (count != -1 && list.Count >= count)
                                            {
                                                break;
                                            }
                                        }
                                    }
                                    /// Nếu là kẻ địch thì đánh dấu để sau không bị lặp lại nữa
                                    else
                                    {
                                        /// Đánh dấu đã xét đối tượng
                                        markObject.Add(obj);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return list;
        }

        /// <summary>
        /// Trả về danh sách đối tượng xung quanh có chung CampID
        /// </summary>
        /// <param name="go"></param>
        /// <param name="distance"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static List<T> GetNearBySameCampObject<T>(T go, float distance, int count = -1) where T : GameObject
        {
            List<T> list = new List<T>();

            HashSet<string> mark = new HashSet<string>();
            HashSet<object> markObject = new HashSet<object>();
            /// Bản đồ hiện tại
            GameMap map = KTMapManager.Find(go.CurrentMapCode);

            /// Vị trí bản thân
            UnityEngine.Vector2 objPos = new UnityEngine.Vector2((float) go.CurrentPos.X, (float) go.CurrentPos.Y);

            if (map != null)
            {
                UnityEngine.Vector2 pos = new UnityEngine.Vector2((float) go.CurrentPos.X, (float) go.CurrentPos.Y);
                UnityEngine.Vector2 gridPos = KTGlobal.WorldPositionToGridPosition(map, pos);

                /// Nếu tọa độ này chưa được tìm
                if (!mark.Contains(gridPos.ToString()))
                {
                    /// Đánh dấu đã tìm quanh tọa độ này
                    mark.Add(gridPos.ToString());

                    /// Danh sách đối tượng xung quanh
                    List<IObject> objectsAround = map.Grid.FindObjects((int) pos.x, (int) pos.y, (int) distance, count != -1);

                    if (objectsAround != null)
                    {
                        /// Duyệt toàn bộ danh sách đối tượng xung quanh
                        foreach (IObject obj in objectsAround)
                        {
                            /// Nếu đối tượng chưa được xét
                            if (!markObject.Contains(obj))
                            {
                                /// Nếu là đối tượng GameObject
                                if (obj is T)
                                {
                                    T target = obj as T;

                                    /// Nếu không trong cùng phụ bản
                                    if (target.CurrentCopyMapID != go.CurrentCopyMapID)
                                    {
                                        continue;
                                    }

                                    /// Vị trí đối tượng
                                    UnityEngine.Vector2 targetPos = new UnityEngine.Vector2((int) target.CurrentPos.X, (int) target.CurrentPos.Y);

                                    /// Có phải kẻ địch không
                                    bool isOpposite = KTGlobal.IsOpposite(go, target);
                                    /// Khoảng cách từ vị trí của đối tượng tới điểm
                                    float distanceToPoint = UnityEngine.Vector2.Distance(objPos, targetPos);

                                    /// Nếu đối tượng không phải là kẻ địch
                                    if (!target.IsDead() && !isOpposite)
                                    {
                                        /// Nếu đối tượng nằm trong vùng ảnh hưởng
                                        if (distanceToPoint <= distance)
                                        {
                                            /// Thêm đối tượng vào danh sách
                                            list.Add(target);
                                            /// Đánh dấu đã xét đối tượng
                                            markObject.Add(obj);

                                            /// Nếu số mục tiêu đã vượt quá giới hạn tìm kiếm
                                            if (count != -1 && list.Count >= count)
                                            {
                                                break;
                                            }
                                        }
                                    }
                                    /// Nếu là kẻ địch thì đánh dấu để sau không bị lặp lại nữa
                                    else
                                    {
                                        /// Đánh dấu đã xét đối tượng
                                        markObject.Add(obj);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return list;
        }

        /// <summary>
        /// Trả về danh sách người chơi hòa bình xung quanh
        /// </summary>
        /// <param name="go"></param>
        /// <param name="distance"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static List<KPlayer> GetNearByPeacePlayers(GameObject go, float distance, int count = -1)
        {
            List<KPlayer> list = new List<KPlayer>();

            HashSet<string> mark = new HashSet<string>();
            HashSet<object> markObject = new HashSet<object>();
            /// Bản đồ hiện tại
            GameMap map = KTMapManager.Find(go.CurrentMapCode);

            /// Vị trí bản thân
            UnityEngine.Vector2 objPos = new UnityEngine.Vector2((float) go.CurrentPos.X, (float) go.CurrentPos.Y);

            if (map != null)
            {
                UnityEngine.Vector2 pos = new UnityEngine.Vector2((float) go.CurrentPos.X, (float) go.CurrentPos.Y);
                UnityEngine.Vector2 gridPos = KTGlobal.WorldPositionToGridPosition(map, pos);

                /// Nếu tọa độ này chưa được tìm
                if (!mark.Contains(gridPos.ToString()))
                {
                    /// Đánh dấu đã tìm quanh tọa độ này
                    mark.Add(gridPos.ToString());

                    /// Danh sách đối tượng xung quanh
                    List<IObject> objectsAround = map.Grid.FindObjects((int) pos.x, (int) pos.y, (int) distance, count != -1);

                    if (objectsAround != null)
                    {
                        /// Duyệt toàn bộ danh sách đối tượng xung quanh
                        foreach (IObject obj in objectsAround)
                        {
                            /// Nếu đối tượng chưa được xét
                            if (!markObject.Contains(obj))
                            {
                                /// Nếu là đối tượng người chơi
                                if (obj is KPlayer)
                                {
                                    KPlayer target = obj as KPlayer;

                                    /// Nếu không trong cùng phụ bản
                                    if (target.CurrentCopyMapID != go.CurrentCopyMapID)
                                    {
                                        continue;
                                    }

                                    /// Vị trí đối tượng
                                    UnityEngine.Vector2 targetPos = new UnityEngine.Vector2((int) target.CurrentPos.X, (int) target.CurrentPos.Y);

                                    /// Có phải kẻ địch không
                                    bool isOpposite = KTGlobal.IsOpposite(go, target);
                                    /// Khoảng cách từ vị trí của đối tượng tới điểm
                                    float distanceToPoint = UnityEngine.Vector2.Distance(objPos, targetPos);

                                    /// Nếu đối tượng không phải là kẻ địch
                                    if (!target.IsDead() && !isOpposite)
                                    {
                                        /// Nếu đối tượng nằm trong vùng ảnh hưởng
                                        if (distanceToPoint <= distance)
                                        {
                                            /// Thêm đối tượng vào danh sách
                                            list.Add(target);
                                            /// Đánh dấu đã xét đối tượng
                                            markObject.Add(obj);

                                            /// Nếu số mục tiêu đã vượt quá giới hạn tìm kiếm
                                            if (count != -1 && list.Count >= count)
                                            {
                                                break;
                                            }
                                        }
                                    }
                                    /// Nếu là kẻ địch thì đánh dấu để sau không bị lặp lại nữa
                                    else
                                    {
                                        /// Đánh dấu đã xét đối tượng
                                        markObject.Add(obj);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return list;
        }

        /// <summary>
        /// Trả về danh sách kẻ địch xung quanh
        /// </summary>
        /// <param name="go"></param>
        /// <param name="distance"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static List<GameObject> GetNearByEnemies(GameObject go, float distance, int count = -1)
        {
            List<GameObject> list = new List<GameObject>();

            HashSet<string> mark = new HashSet<string>();
            HashSet<object> markObject = new HashSet<object>();
            /// Bản đồ hiện tại
            GameMap map = KTMapManager.Find(go.CurrentMapCode);

            /// Vị trí bản thân
            UnityEngine.Vector2 objPos = new UnityEngine.Vector2((float) go.CurrentPos.X, (float) go.CurrentPos.Y);

            if (map != null)
            {
                UnityEngine.Vector2 pos = new UnityEngine.Vector2((float) go.CurrentPos.X, (float) go.CurrentPos.Y);
                UnityEngine.Vector2 gridPos = KTGlobal.WorldPositionToGridPosition(map, pos);

                /// Nếu tọa độ này chưa được tìm
                if (!mark.Contains(gridPos.ToString()))
                {
                    /// Đánh dấu đã tìm quanh tọa độ này
                    mark.Add(gridPos.ToString());

                    /// Danh sách đối tượng xung quanh
                    List<IObject> objectsAround = map.Grid.FindObjects((int) pos.x, (int) pos.y, (int) distance, count != -1);

                    if (objectsAround != null)
                    {
                        /// Duyệt toàn bộ danh sách đối tượng xung quanh
                        foreach (IObject obj in objectsAround)
                        {
                            /// Nếu đối tượng chưa được xét
                            if (!markObject.Contains(obj))
                            {
                                /// Nếu là đối tượng có thể tấn công
                                if (obj is GameObject && obj != go)
                                {
                                    GameObject target = obj as GameObject;

                                    /// Nếu không trong cùng phụ bản
                                    if (target.CurrentCopyMapID != go.CurrentCopyMapID)
                                    {
                                        continue;
                                    }
                                    /// Vị trí đối tượng
                                    UnityEngine.Vector2 targetPos = new UnityEngine.Vector2((int) target.CurrentPos.X, (int) target.CurrentPos.Y);

                                    /// Có phải kẻ địch không
                                    bool isOpposite = KTGlobal.CanAttack(go, target);
                                    /// Khoảng cách từ vị trí của đối tượng tới điểm
                                    float distanceToPoint = UnityEngine.Vector2.Distance(objPos, targetPos);

                                    /// Nếu đối tượng là kẻ địch
                                    if (!target.IsDead() && isOpposite)
                                    {
                                        /// Nếu đối tượng nằm trong vùng ảnh hưởng
                                        if (distanceToPoint <= distance)
                                        {
                                            /// Thêm đối tượng vào danh sách
                                            list.Add(target);
                                            /// Đánh dấu đã xét đối tượng
                                            markObject.Add(obj);

                                            /// Nếu số mục tiêu đã vượt quá giới hạn tìm kiếm
                                            if (count != -1 && list.Count >= count)
                                            {
                                                break;
                                            }
                                        }
                                    }
                                    /// Nếu không phải kẻ địch thì đánh dấu để sau không bị lặp lại nữa
                                    else
                                    {
                                        /// Đánh dấu đã xét đối tượng
                                        markObject.Add(obj);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return list;
        }

        /// <summary>
        /// Trả về danh sách kẻ địch xung quanh
        /// </summary>
        /// <param name="go"></param>
        /// <param name="distance"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static List<T> GetNearByEnemies<T>(GameObject go, float distance, int count = -1) where T : GameObject
        {
            List<T> list = new List<T>();

            HashSet<string> mark = new HashSet<string>();
            HashSet<object> markObject = new HashSet<object>();
            /// Bản đồ hiện tại
            GameMap map = KTMapManager.Find(go.CurrentMapCode);

            /// Vị trí bản thân
            UnityEngine.Vector2 objPos = new UnityEngine.Vector2((float) go.CurrentPos.X, (float) go.CurrentPos.Y);

            if (map != null)
            {
                UnityEngine.Vector2 pos = new UnityEngine.Vector2((float) go.CurrentPos.X, (float) go.CurrentPos.Y);
                UnityEngine.Vector2 gridPos = KTGlobal.WorldPositionToGridPosition(map, pos);

                /// Nếu tọa độ này chưa được tìm
                if (!mark.Contains(gridPos.ToString()))
                {
                    /// Đánh dấu đã tìm quanh tọa độ này
                    mark.Add(gridPos.ToString());

                    /// Danh sách đối tượng xung quanh
                    List<IObject> objectsAround = map.Grid.FindObjects((int) pos.x, (int) pos.y, (int) distance, count != -1);

                    if (objectsAround != null)
                    {
                        /// Duyệt toàn bộ danh sách đối tượng xung quanh
                        foreach (IObject obj in objectsAround)
                        {
                            /// Nếu đối tượng chưa được xét
                            if (!markObject.Contains(obj))
                            {
                                /// Nếu là đối tượng có thể tấn công
                                if (obj is T && obj != go)
                                {
                                    T target = obj as T;

                                    /// Nếu không trong cùng phụ bản
                                    if (target.CurrentCopyMapID != go.CurrentCopyMapID)
                                    {
                                        continue;
                                    }

                                    /// Vị trí đối tượng
                                    UnityEngine.Vector2 targetPos = new UnityEngine.Vector2((int) target.CurrentPos.X, (int) target.CurrentPos.Y);

                                    /// Có phải kẻ địch không
                                    bool isOpposite = KTGlobal.CanAttack(go, target);
                                    /// Khoảng cách từ vị trí của đối tượng tới điểm
                                    float distanceToPoint = UnityEngine.Vector2.Distance(objPos, targetPos);

                                    /// Nếu đối tượng là kẻ địch
                                    if (!target.IsDead() && isOpposite)
                                    {
                                        /// Nếu đối tượng nằm trong vùng ảnh hưởng
                                        if (distanceToPoint <= distance)
                                        {
                                            /// Thêm đối tượng vào danh sách
                                            list.Add(target);
                                            /// Đánh dấu đã xét đối tượng
                                            markObject.Add(obj);

                                            /// Nếu số mục tiêu đã vượt quá giới hạn tìm kiếm
                                            if (count != -1 && list.Count >= count)
                                            {
                                                break;
                                            }
                                        }
                                    }
                                    /// Nếu không phải kẻ địch thì đánh dấu để sau không bị lặp lại nữa
                                    else
                                    {
                                        /// Đánh dấu đã xét đối tượng
                                        markObject.Add(obj);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return list;
        }

        /// <summary>
        /// Trả về danh sách kẻ địch xung quanh vị trí
        /// </summary>
        /// <param name="go"></param>
        /// <param name="pos"></param>
        /// <param name="distance"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static List<GameObject> GetEnemiesAroundPos(GameObject go, UnityEngine.Vector2 pos, float distance, int count = -1)
        {
            List<GameObject> list = new List<GameObject>();

            HashSet<string> mark = new HashSet<string>();
            HashSet<object> markObject = new HashSet<object>();
            /// Bản đồ hiện tại
            GameMap map = KTMapManager.Find(go.CurrentMapCode);

            if (map != null)
            {
                /// Tọa độ lưới
                UnityEngine.Vector2 gridPos = KTGlobal.WorldPositionToGridPosition(map, pos);

                /// Nếu tọa độ này chưa được tìm
                if (!mark.Contains(gridPos.ToString()))
                {
                    /// Đánh dấu đã tìm quanh tọa độ này
                    mark.Add(gridPos.ToString());

                    /// Danh sách đối tượng xung quanh
                    List<IObject> objectsAround = map.Grid.FindObjects((int) pos.x, (int) pos.y, (int) distance, count != -1);

                    if (objectsAround != null)
                    {
                        /// Duyệt toàn bộ danh sách đối tượng xung quanh
                        foreach (IObject obj in objectsAround)
                        {
                            /// Nếu đối tượng chưa được xét
                            if (!markObject.Contains(obj))
                            {
                                /// Nếu là đối tượng có thể tấn công
                                if (obj is GameObject && obj != go)
                                {
                                    GameObject target = obj as GameObject;

                                    /// Nếu không trong cùng phụ bản
                                    if (target.CurrentCopyMapID != go.CurrentCopyMapID)
                                    {
                                        continue;
                                    }

                                    /// Vị trí đối tượng
                                    UnityEngine.Vector2 targetPos = new UnityEngine.Vector2((int) target.CurrentPos.X, (int) target.CurrentPos.Y);

                                    /// Có phải kẻ địch không
                                    bool isOpposite = KTGlobal.CanAttack(go, target);
                                    /// Khoảng cách từ vị trí của đối tượng tới điểm
                                    float distanceToPoint = UnityEngine.Vector2.Distance(pos, targetPos);

                                    /// Nếu đối tượng là kẻ địch
                                    if (!target.IsDead() && isOpposite)
                                    {
                                        /// Nếu đối tượng nằm trong vùng ảnh hưởng
                                        if (distanceToPoint <= distance)
                                        {
                                            /// Thêm đối tượng vào danh sách
                                            list.Add(target);
                                            /// Đánh dấu đã xét đối tượng
                                            markObject.Add(obj);

                                            /// Nếu số mục tiêu đã vượt quá giới hạn tìm kiếm
                                            if (count != -1 && list.Count >= count)
                                            {
                                                break;
                                            }
                                        }
                                    }
                                    /// Nếu không phải kẻ địch thì đánh dấu để sau không bị lặp lại nữa
                                    else
                                    {
                                        /// Đánh dấu đã xét đối tượng
                                        markObject.Add(obj);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return list;
        }

        /// <summary>
        /// Trả về danh sách kẻ địch nằm trong phạm vi 2 điểm với bán kính quét tương ứng
        /// </summary>
        /// <param name="go"></param>
        /// <param name="fromPos"></param>
        /// <param name="toPos"></param>
        /// <param name="radius"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static List<GameObject> GetEnemiesBetweenTwoPoints(GameObject go, UnityEngine.Vector2 fromPos, UnityEngine.Vector2 toPos, float radius, int count = -1)
        {
            List<GameObject> list = new List<GameObject>();

            HashSet<string> mark = new HashSet<string>();
            HashSet<object> markObject = new HashSet<object>();
            /// Bản đồ hiện tại
            GameMap map = KTMapManager.Find(go.CurrentMapCode);

            if (map != null)
            {
                /// Vector hướng
                UnityEngine.Vector2 dirVector = toPos - fromPos;

                /// Tổng số điểm
                int totalPoints = (int) (dirVector.magnitude / radius) + 1;

                /// Danh sách các điểm nằm giữa 2 điểm cho trước
                List<UnityEngine.Vector2> points = KTMath.GetPointsBetweenTwoPoints(fromPos, toPos, totalPoints);

                ///// Debug hiện khối Debug Object
                //KT_TCPHandler.ShowDebugObjects(go.CurrentMapCode, points, (int)radius, 10f);

                /// Duyệt toàn bộ danh sách các điểm
                foreach (UnityEngine.Vector2 point in points)
                {
                    /// Tọa độ lưới
                    UnityEngine.Vector2 gridPos = KTGlobal.WorldPositionToGridPosition(map, point);

                    /// Nếu tọa độ này chưa được tìm
                    if (!mark.Contains(gridPos.ToString()))
                    {
                        /// Đánh dấu đã tìm quanh tọa độ này
                        mark.Add(gridPos.ToString());

                        /// Danh sách đối tượng xung quanh
                        List<IObject> objectsAround = map.Grid.FindObjects((int) point.x, (int) point.y, (int) radius, count != -1);

                        if (objectsAround != null)
                        {
                            /// Duyệt toàn bộ danh sách đối tượng xung quanh
                            foreach (IObject obj in objectsAround)
                            {
                                /// Nếu đối tượng chưa được xét
                                if (!markObject.Contains(obj))
                                {
                                    /// Nếu là đối tượng có thể tấn công
                                    if (obj is GameObject && obj != go)
                                    {
                                        GameObject target = obj as GameObject;

                                        /// Nếu không trong cùng phụ bản
                                        if (target.CurrentCopyMapID != go.CurrentCopyMapID)
                                        {
                                            continue;
                                        }

                                        /// Vị trí đối tượng
                                        UnityEngine.Vector2 targetPos = new UnityEngine.Vector2((int) target.CurrentPos.X, (int) target.CurrentPos.Y);

                                        /// Có phải kẻ địch không
                                        bool isOpposite = KTGlobal.CanAttack(go, target);
                                        /// Khoảng cách từ vị trí của đối tượng tới điểm
                                        float distanceToPoint = UnityEngine.Vector2.Distance(point, targetPos);

                                        /// Nếu đối tượng là kẻ địch
                                        if (!target.IsDead() && isOpposite)
                                        {
                                            /// Nếu đối tượng nằm trong vùng ảnh hưởng
                                            if (distanceToPoint <= radius)
                                            {
                                                /// Thêm đối tượng vào danh sách
                                                list.Add(target);
                                                /// Đánh dấu đã xét đối tượng
                                                markObject.Add(obj);

                                                /// Nếu số mục tiêu đã vượt quá giới hạn tìm kiếm
                                                if (count != -1 && list.Count >= count)
                                                {
                                                    break;
                                                }
                                            }
                                        }
                                        /// Nếu không phải kẻ địch thì đánh dấu để sau không bị lặp lại nữa
                                        else
                                        {
                                            /// Đánh dấu đã xét đối tượng
                                            markObject.Add(obj);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return list;
        }


        /// <summary>
        /// Trả về danh sách kẻ địch xung quanh mà bản thân có thể nhìn thấy
        /// </summary>
        /// <param name="go"></param>
        /// <param name="distance"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static List<GameObject> GetNearByVisibleEnemies(GameObject go, float distance, int count = -1)
        {
            List<GameObject> list = new List<GameObject>();

            HashSet<string> mark = new HashSet<string>();
            HashSet<object> markObject = new HashSet<object>();
            /// Bản đồ hiện tại
            GameMap map = KTMapManager.Find(go.CurrentMapCode);

            /// Vị trí bản thân
            UnityEngine.Vector2 objPos = new UnityEngine.Vector2((float) go.CurrentPos.X, (float) go.CurrentPos.Y);

            if (map != null)
            {
                UnityEngine.Vector2 pos = new UnityEngine.Vector2((float) go.CurrentPos.X, (float) go.CurrentPos.Y);
                UnityEngine.Vector2 gridPos = KTGlobal.WorldPositionToGridPosition(map, pos);

                /// Nếu tọa độ này chưa được tìm
                if (!mark.Contains(gridPos.ToString()))
                {
                    /// Đánh dấu đã tìm quanh tọa độ này
                    mark.Add(gridPos.ToString());

                    /// Danh sách đối tượng xung quanh
                    List<IObject> objectsAround = map.Grid.FindObjects((int) pos.x, (int) pos.y, (int) distance, count != -1);

                    if (objectsAround != null)
                    {
                        /// Duyệt toàn bộ danh sách đối tượng xung quanh
                        foreach (IObject obj in objectsAround)
                        {
                            /// Nếu đối tượng chưa được xét
                            if (!markObject.Contains(obj))
                            {
                                /// Nếu là đối tượng có thể tấn công
                                if (obj is GameObject && obj != go)
                                {
                                    GameObject target = obj as GameObject;

                                    /// Nếu không trong cùng phụ bản
                                    if (target.CurrentCopyMapID != go.CurrentCopyMapID)
                                    {
                                        continue;
                                    }
                                    /// Nếu không thể nhìn thấy
                                    else if (target.IsInvisible() && !target.VisibleTo(go))
                                    {
                                        continue;
                                    }


                                    /// Vị trí đối tượng
                                    UnityEngine.Vector2 targetPos = new UnityEngine.Vector2((int) target.CurrentPos.X, (int) target.CurrentPos.Y);

                                    /// Có phải kẻ địch không
                                    bool isOpposite = KTGlobal.CanAttack(go, target);
                                    /// Khoảng cách từ vị trí của đối tượng tới điểm
                                    float distanceToPoint = UnityEngine.Vector2.Distance(objPos, targetPos);

                                    /// Nếu đối tượng là kẻ địch
                                    if (!target.IsDead() && isOpposite)
                                    {
                                        /// Nếu đối tượng nằm trong vùng ảnh hưởng
                                        if (distanceToPoint <= distance)
                                        {
                                            /// Thêm đối tượng vào danh sách
                                            list.Add(target);
                                            /// Đánh dấu đã xét đối tượng
                                            markObject.Add(obj);

                                            /// Nếu số mục tiêu đã vượt quá giới hạn tìm kiếm
                                            if (count != -1 && list.Count >= count)
                                            {
                                                break;
                                            }
                                        }
                                    }
                                    /// Nếu không phải kẻ địch thì đánh dấu để sau không bị lặp lại nữa
                                    else
                                    {
                                        /// Đánh dấu đã xét đối tượng
                                        markObject.Add(obj);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return list;
        }

        /// <summary>
        /// Trả về danh sách kẻ địch xung quanh mà bản thân có thể nhìn thấy
        /// </summary>
        /// <param name="go"></param>
        /// <param name="distance"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static List<T> GetNearByVisibleEnemies<T>(GameObject go, float distance, int count = -1) where T : GameObject
        {
            List<T> list = new List<T>();

            HashSet<string> mark = new HashSet<string>();
            HashSet<object> markObject = new HashSet<object>();
            /// Bản đồ hiện tại
            GameMap map = KTMapManager.Find(go.CurrentMapCode);

            /// Vị trí bản thân
            UnityEngine.Vector2 objPos = new UnityEngine.Vector2((float) go.CurrentPos.X, (float) go.CurrentPos.Y);

            if (map != null)
            {
                UnityEngine.Vector2 pos = new UnityEngine.Vector2((float) go.CurrentPos.X, (float) go.CurrentPos.Y);
                UnityEngine.Vector2 gridPos = KTGlobal.WorldPositionToGridPosition(map, pos);

                /// Nếu tọa độ này chưa được tìm
                if (!mark.Contains(gridPos.ToString()))
                {
                    /// Đánh dấu đã tìm quanh tọa độ này
                    mark.Add(gridPos.ToString());

                    /// Danh sách đối tượng xung quanh
                    List<IObject> objectsAround = map.Grid.FindObjects((int) pos.x, (int) pos.y, (int) distance, count != -1);

                    if (objectsAround != null)
                    {
                        /// Duyệt toàn bộ danh sách đối tượng xung quanh
                        foreach (IObject obj in objectsAround)
                        {
                            /// Nếu đối tượng chưa được xét
                            if (!markObject.Contains(obj))
                            {
                                /// Nếu là đối tượng có thể tấn công
                                if (obj is T && obj != go)
                                {
                                    T target = obj as T;

                                    /// Nếu không trong cùng phụ bản
                                    if (target.CurrentCopyMapID != go.CurrentCopyMapID)
                                    {
                                        continue;
                                    }
                                    /// Nếu không thể nhìn thấy
                                    else if (target.IsInvisible() && !target.VisibleTo(go))
                                    {
                                        continue;
                                    }

                                    /// Vị trí đối tượng
                                    UnityEngine.Vector2 targetPos = new UnityEngine.Vector2((int) target.CurrentPos.X, (int) target.CurrentPos.Y);

                                    /// Có phải kẻ địch không
                                    bool isOpposite = KTGlobal.CanAttack(go, target);
                                    /// Khoảng cách từ vị trí của đối tượng tới điểm
                                    float distanceToPoint = UnityEngine.Vector2.Distance(objPos, targetPos);

                                    /// Nếu đối tượng là kẻ địch
                                    if (!target.IsDead() && isOpposite)
                                    {
                                        /// Nếu đối tượng nằm trong vùng ảnh hưởng
                                        if (distanceToPoint <= distance)
                                        {
                                            /// Thêm đối tượng vào danh sách
                                            list.Add(target);
                                            /// Đánh dấu đã xét đối tượng
                                            markObject.Add(obj);

                                            /// Nếu số mục tiêu đã vượt quá giới hạn tìm kiếm
                                            if (count != -1 && list.Count >= count)
                                            {
                                                break;
                                            }
                                        }
                                    }
                                    /// Nếu không phải kẻ địch thì đánh dấu để sau không bị lặp lại nữa
                                    else
                                    {
                                        /// Đánh dấu đã xét đối tượng
                                        markObject.Add(obj);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return list;
        }

        /// <summary>
        /// Trả về danh sách đối tượng xung quanh mà bản thân có thể nhìn thấy
        /// </summary>
        /// <param name="go"></param>
        /// <param name="distance"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static List<T> GetNearByVisibleObjects<T>(GameObject go, float distance, int count = -1) where T : GameObject
        {
            List<T> list = new List<T>();

            UnityEngine.Vector2 pos = new UnityEngine.Vector2((int) go.CurrentPos.X, (int) go.CurrentPos.Y);

            HashSet<string> mark = new HashSet<string>();
            HashSet<object> markObject = new HashSet<object>();
            /// Bản đồ hiện tại
            GameMap map = KTMapManager.Find(go.CurrentMapCode);

            if (map != null)
            {
                UnityEngine.Vector2 gridPos = KTGlobal.WorldPositionToGridPosition(map, pos);

                /// Nếu tọa độ này chưa được tìm
                if (!mark.Contains(gridPos.ToString()))
                {
                    /// Đánh dấu đã tìm quanh tọa độ này
                    mark.Add(gridPos.ToString());

                    /// Danh sách đối tượng xung quanh
                    List<IObject> objectsAround = map.Grid.FindObjects((int) pos.x, (int) pos.y, (int) distance, count != -1);

                    if (objectsAround != null)
                    {
                        /// Duyệt toàn bộ danh sách đối tượng xung quanh
                        foreach (IObject obj in objectsAround)
                        {
                            /// Nếu đối tượng chưa được xét
                            if (!markObject.Contains(obj))
                            {
                                /// Nếu là đối tượng GameObject
                                if (obj is T)
                                {
                                    T target = obj as T;

                                    /// Nếu không trong cùng phụ bản
                                    if (target.CurrentCopyMapID != go.CurrentCopyMapID)
                                    {
                                        continue;
                                    }
                                    /// Nếu không thể nhìn thấy
                                    else if (target.IsInvisible() && !target.VisibleTo(go))
                                    {
                                        continue;
                                    }

                                    /// Vị trí đối tượng
                                    UnityEngine.Vector2 targetPos = new UnityEngine.Vector2((int) target.CurrentPos.X, (int) target.CurrentPos.Y);

                                    /// Khoảng cách từ vị trí của đối tượng tới điểm
                                    float distanceToPoint = UnityEngine.Vector2.Distance(pos, targetPos);

                                    /// Nếu đối tượng nằm trong vùng ảnh hưởng
                                    if (distanceToPoint <= distance)
                                    {
                                        /// Thêm đối tượng vào danh sách
                                        list.Add(target);
                                        /// Đánh dấu đã xét đối tượng
                                        markObject.Add(obj);

                                        /// Nếu số mục tiêu đã vượt quá giới hạn tìm kiếm
                                        if (count != -1 && list.Count >= count)
                                        {
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return list;
        }

        /// <summary>
        /// Trả về danh sách người chơi xung quanh mà bản thân có thể nhìn thấy
        /// </summary>
        /// <param name="go"></param>
        /// <param name="distance"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static List<KPlayer> GetNearByVisiblePlayers(GameObject go, float distance, int count = -1)
        {
            List<KPlayer> list = new List<KPlayer>();

            UnityEngine.Vector2 pos = new UnityEngine.Vector2((int) go.CurrentPos.X, (int) go.CurrentPos.Y);

            HashSet<string> mark = new HashSet<string>();
            HashSet<object> markObject = new HashSet<object>();
            /// Bản đồ hiện tại
            GameMap map = KTMapManager.Find(go.CurrentMapCode);

            if (map != null)
            {
                UnityEngine.Vector2 gridPos = KTGlobal.WorldPositionToGridPosition(map, pos);

                /// Nếu tọa độ này chưa được tìm
                if (!mark.Contains(gridPos.ToString()))
                {
                    /// Đánh dấu đã tìm quanh tọa độ này
                    mark.Add(gridPos.ToString());

                    /// Danh sách đối tượng xung quanh
                    List<IObject> objectsAround = map.Grid.FindObjects((int) pos.x, (int) pos.y, (int) distance, count != -1);

                    if (objectsAround != null)
                    {
                        /// Duyệt toàn bộ danh sách đối tượng xung quanh
                        foreach (IObject obj in objectsAround)
                        {
                            /// Nếu đối tượng chưa được xét
                            if (!markObject.Contains(obj))
                            {
                                /// Nếu là đối tượng GameObject
                                if (obj is KPlayer)
                                {
                                    KPlayer target = obj as KPlayer;

                                    /// Nếu không trong cùng phụ bản
                                    if (target.CurrentCopyMapID != go.CurrentCopyMapID)
                                    {
                                        continue;
                                    }
                                    /// Nếu không thể nhìn thấy
                                    else if (target.IsInvisible() && !target.VisibleTo(go))
                                    {
                                        continue;
                                    }

                                    /// Vị trí đối tượng
                                    UnityEngine.Vector2 targetPos = new UnityEngine.Vector2((int) target.CurrentPos.X, (int) target.CurrentPos.Y);

                                    /// Khoảng cách từ vị trí của đối tượng tới điểm
                                    float distanceToPoint = UnityEngine.Vector2.Distance(pos, targetPos);

                                    /// Nếu đối tượng nằm trong vùng ảnh hưởng
                                    if (distanceToPoint <= distance)
                                    {
                                        /// Thêm đối tượng vào danh sách
                                        list.Add(target);
                                        /// Đánh dấu đã xét đối tượng
                                        markObject.Add(obj);

                                        /// Nếu số mục tiêu đã vượt quá giới hạn tìm kiếm
                                        if (count != -1 && list.Count >= count)
                                        {
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return list;
        }
        #endregion
    }
}
