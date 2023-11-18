using GameServer.KiemThe.Entities;
using GameServer.Logic;
using Server.Tools;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Windows;
using static GameServer.Logic.KTMapManager;

namespace GameServer.KiemThe.Logic
{
    /// <summary>
    /// API
    /// </summary>
    public partial class KTBotStoryBoardEx
    {
        #region Support
        /// <summary>
        /// Kiểm tra có StoryBoard của đối tượng tương ứng đang thực thi không
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public bool HasStoryBoard(GameObject obj)
        {
            return this.objectTimers.ContainsKey(obj.RoleID);
        }

        /// <summary>
        /// Trả về danh sách đường đi hiện tại của đối tượng
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public string GetCurrentPathString(GameObject obj)
        {
            try
            {
                if (this.objectTimers.TryGetValue(obj.RoleID, out OtherStoryBoard storyBoard))
                {
                    if (storyBoard.Paths.Count <= 0 || storyBoard.Paths.Count > 10)
                    {
                        return "";
                    }
                    Queue<UnityEngine.Vector2> paths = new Queue<UnityEngine.Vector2>(storyBoard.Paths);

                    /// Vị trí hiện tại
                    UnityEngine.Vector2 currentPos = new UnityEngine.Vector2((int) obj.CurrentPos.X, (int) obj.CurrentPos.Y);
                    string str = string.Format("{0}_{1}", (int) currentPos.x, (int) currentPos.y) + "|";
                    while (paths.Count > 0)
                    {
                        UnityEngine.Vector2 pos = paths.Dequeue();
                        str += string.Format("{0}_{1}", (int) pos.x, (int) pos.y) + "|";
                    }
                    if (str.Length > 0)
                    {
                        return str.Substring(0, str.Length - 1);
                    }
                    else
                    {
                        return str;
                    }
                }
                else
                {
                    return "";
                }
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Exception, ex.ToString());
                return "";
            }
        }

        /// <summary>
        /// Đổi trạng thái di chuyển
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="action"></param>
        public void ChangeAction(GameObject obj, KE_NPC_DOING action)
        {
            try
            {
                /// Nếu không phải đi bộ hoặc chạy thì bỏ qua
                if (action != KE_NPC_DOING.do_walk && action != KE_NPC_DOING.do_run)
                {
                    return;
                }

                /// Tìm Storyboard tương ứng
                if (this.objectTimers.TryGetValue(obj.RoleID, out OtherStoryBoard storyBoard))
                {
                    /// Cập nhật trạng thái di chuyển cho Storyboard tương ứng
                    storyBoard.Action = action;
                }
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Exception, ex.ToString());
            }
        }
        #endregion

        #region Remove
        /// <summary>
        /// Xóa đối tượng khỏi danh sách
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="synsClient"></param>
        public void Remove(GameObject obj, bool synsClient = true)
        {
            /// Toác
            if (obj == null)
            {
                return;
            }

            try
            {
                /// Nếu tồn tại trong danh sách
                if (this.objectTimers.TryGetValue(obj.RoleID, out OtherStoryBoard objectTimer))
                {
                    /// Đánh dấu huỷ
                    objectTimer.IsDisposed = true;
                    /// Xóa khỏi danh sách
                    this.objectTimers.TryRemove(obj.RoleID, out _);
                    /// Thiết lập vị trí đích đến
                    obj.ToPos = new Point(-1, -1);

                    /// Đồng bộ về Client
                    if (synsClient)
                    {
                        this.SendObjectStopMoveToClient(obj);
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Exception, ex.ToString());
            }
        }
        #endregion

        #region Add
        /// <summary>
        /// Thêm đối tượng cần dịch chuyển vào danh sách
        /// </summary>
        /// <param name="carriage"></param>
        /// <param name="fromPos"></param>
        /// <param name="toPos"></param>
        /// <param name="action"></param>
        /// <param name="forceMoveBySpecialState"></param>
        /// <param name="useAStarPathFinder"></param>
        public void Add(GameObject carriage, Point fromPos, Point toPos, KE_NPC_DOING action = KE_NPC_DOING.do_run, bool forceMoveBySpecialState = false, bool useAStarPathFinder = false)
        {
            try
            {
                if (carriage == null)
                {
                    return;
                }
                else if (fromPos == default || toPos == default)
                {
                    return;
                }
                /// Nếu dính trạng thái không thể di chuyển
                else if ((!forceMoveBySpecialState && !carriage.IsCanPositiveMove()) || (forceMoveBySpecialState && !carriage.IsCanMove()))
                {
                    this.Remove(carriage);
                    return;
                }

                /// Dừng thực thi StoryBoard trước đó
                this.Remove(carriage);

                GameMap gameMap = KTMapManager.Find(carriage.CurrentMapCode);

                /// Thực hiện tìm đường
                List<UnityEngine.Vector2> nodeList;

                /// Nếu sử dụng A*
                if (useAStarPathFinder)
                {
                    nodeList = KTGlobal.FindPath(carriage, new UnityEngine.Vector2((float) fromPos.X, (float) fromPos.Y), new UnityEngine.Vector2((float) toPos.X, (float) toPos.Y), carriage.CurrentCopyMapID);
                }
                else
                {
                    nodeList = new List<UnityEngine.Vector2>()
                    {
                        new UnityEngine.Vector2((float) fromPos.X, (float) fromPos.Y), new UnityEngine.Vector2((float) toPos.X, (float) toPos.Y),
                    };
                }

                /// Nếu không có đường đi thì bỏ qua
                if (nodeList.Count < 2)
                {
                    return;
                }

                /// Hàng đợi chứa kết quả
                ConcurrentQueue<UnityEngine.Vector2> paths = new ConcurrentQueue<UnityEngine.Vector2>();
                /// Nạp tất cả các điểm vừa tìm được vào hàng đợi
                foreach (UnityEngine.Vector2 pos in nodeList)
                {
                    paths.Enqueue(pos);
                }


                string pathStr = "";
                foreach (UnityEngine.Vector2 pos in paths)
                {
                    pathStr += "|" + string.Format("{0}_{1}", (int) pos.x, (int) pos.y);
                }
                pathStr = pathStr.Substring(1);
                /// Gửi gói tin về Client thông báo
                this.SendObjectMoveToClient(carriage, pathStr, fromPos, toPos, action);

                //Console.WriteLine("GameObject {3} => Move from {0} to {1} with paths = {2}", fromPos.ToString(), toPos.ToString(), pathStr, obj.RoleName);

                /// Vị trí hiện tại của người chơi
                UnityEngine.Vector2 currentPos = new UnityEngine.Vector2((int) carriage.CurrentPos.X, (int) carriage.CurrentPos.Y);
                /// Vị trí đích đến
                carriage.ToPos = toPos;

                OtherStoryBoard storyBoard = new OtherStoryBoard()
                {
                    Owner = carriage,
                    Paths = paths,
                    LastGridPos = KTGlobal.WorldPositionToGridPosition(gameMap, currentPos),
                    LastTick = KTGlobal.GetCurrentTimeMilis(),
                    Action = action,
                    HasCompletedLastMove = true,
                };
                this.objectTimers[carriage.RoleID] = storyBoard;

                /// Bắt đầu StoryBoard
                this.StartStoryBoard(storyBoard);
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Exception, ex.ToString());
            }
        }

        /// <summary>
        /// Thêm đối tượng cần dịch chuyển vào danh sách
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="pathString"></param>
        /// <param name="action"></param>
        public void Add(GameObject obj, string pathString, KE_NPC_DOING action = KE_NPC_DOING.do_run, bool forceMoveBySpecialState = false)
        {
            try
            {
                if (obj == null)
                {
                    return;
                }
                /// Nếu dính trạng thái không thể di chuyển
                else if ((!forceMoveBySpecialState && !obj.IsCanPositiveMove()) || (forceMoveBySpecialState && !obj.IsCanMove()))
                {
                    this.Remove(obj);
                    return;
                }

                /// Dừng thực thi StoryBoard trước đó
                this.Remove(obj);

                ConcurrentQueue<UnityEngine.Vector2> paths = new ConcurrentQueue<UnityEngine.Vector2>();
                string[] points = pathString.Split('|');
                if (points.Length < 2)
                {
                    return;
                }

                GameMap gameMap = KTMapManager.Find(obj.CurrentMapCode);

                Point toPos = new Point(-1, -1);
                /// Nạp tất cả các điểm vừa tìm được vào hàng đợi
                for (int i = 0; i < points.Length; i++)
                {
                    string[] pos = points[i].Split('_');
                    if (pos.Length < 2)
                    {
                        throw new Exception();
                    }

                    int posX = int.Parse(pos[0]);
                    int posY = int.Parse(pos[1]);

                    /// Nếu vị trí hiện tại không thể đến được
                    if (!KTMapManager.Find(obj.CurrentMapCode).CanMove(new Point(posX, posY), obj.CurrentCopyMapID))
                    {
                        continue;
                    }

                    toPos = new Point(posX, posY);

                    paths.Enqueue(new UnityEngine.Vector2(posX, posY));
                }

                /// Vị trí hiện tại của người chơi
                UnityEngine.Vector2 currentPos = new UnityEngine.Vector2((int) obj.CurrentPos.X, (int) obj.CurrentPos.Y);
                /// Vị trí đích đến
                obj.ToPos = toPos;

                OtherStoryBoard storyBoard = new OtherStoryBoard()
                {
                    Owner = obj,
                    Paths = paths,
                    LastGridPos = KTGlobal.WorldPositionToGridPosition(gameMap, currentPos),
                    LastTick = KTGlobal.GetCurrentTimeMilis(),
                    Action = action,
                    HasCompletedLastMove = true,
                };
                this.objectTimers[obj.RoleID] = storyBoard;

                /// Bắt đầu StoryBoard
                this.StartStoryBoard(storyBoard);
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Exception, ex.ToString());
            }
        }
        #endregion
    }
}
