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
	public partial class KTMonsterStoryBoardEx
	{
        #region Support
        /// <summary>
        /// Kiểm tra có StoryBoard của quái tương ứng đang thực thi không
        /// </summary>
        /// <param name="monster"></param>
        /// <returns></returns>
        public bool HasStoryBoard(Monster monster)
        {
            return this.objectTimers.ContainsKey(monster.RoleID);
        }

        /// <summary>
        /// Trả về danh sách đường đi hiện tại của đối tượng
        /// </summary>
        /// <param name="monster"></param>
        /// <returns></returns>
        public string GetCurrentPathString(Monster monster)
        {
            try
            {
                if (this.objectTimers.TryGetValue(monster.RoleID, out MonsterStoryBoard storyBoard))
                {
                    if (storyBoard.Paths.Count <= 0 || storyBoard.Paths.Count > 10)
                    {
                        return "";
                    }
                    Queue<UnityEngine.Vector2> paths = new Queue<UnityEngine.Vector2>(storyBoard.Paths);

                    /// Vị trí hiện tại
                    UnityEngine.Vector2 currentPos = new UnityEngine.Vector2((int) monster.CurrentPos.X, (int) monster.CurrentPos.Y);
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
        /// <param name="monster"></param>
        /// <param name="action"></param>
        public void ChangeAction(Monster monster, KE_NPC_DOING action)
        {
            try
            {
                /// Nếu không phải đi bộ hoặc chạy thì bỏ qua
                if (action != KE_NPC_DOING.do_walk && action != KE_NPC_DOING.do_run)
                {
                    return;
                }

                /// Tìm Storyboard tương ứng
                if (this.objectTimers.TryGetValue(monster.RoleID, out MonsterStoryBoard storyBoard))
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
        /// <param name="monster"></param>
        /// <param name="sendToClient"></param>
        public void Remove(Monster monster, bool synsClient = true)
        {
            /// Toác
            if (monster == null)
            {
                return;
            }

            try
            {
                /// Nếu tồn tại trong danh sách
                if (this.objectTimers.TryGetValue(monster.RoleID, out MonsterStoryBoard objectTimer))
                {
                    /// Đánh dấu đã bị hủy
                    objectTimer.IsDisposed = true;
                    /// Xóa khỏi danh sách
                    this.objectTimers.TryRemove(monster.RoleID, out _);
                    /// Thiết lập vị trí đích đến
                    monster.ToPos = new Point(-1, -1);

                    /// Đồng bộ về Client
                    if (synsClient)
                    {
                        this.SendMonsterStopMoveToClient(monster);
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
        /// <param name="monster"></param>
        /// <param name="fromPos"></param>
        /// <param name="toPos"></param>
        /// <param name="action"></param>
        /// <param name="forceMoveBySpecialState"></param>
        /// <param name="useAStarPathFinder"></param>
        public void Add(Monster monster, Point fromPos, Point toPos, KE_NPC_DOING action = KE_NPC_DOING.do_run, bool forceMoveBySpecialState = false, bool useAStarPathFinder = false)
        {
            try
            {
                if (monster == null)
                {
                    return;
                }
                else if (fromPos == default || toPos == default)
                {
                    return;
                }
                /// Nếu dính trạng thái không thể di chuyển
                else if ((!forceMoveBySpecialState && !monster.IsCanPositiveMove()) || (forceMoveBySpecialState && !monster.IsCanMove()))
                {
                    this.Remove(monster);
                    return;
                }

                /// Dừng thực thi StoryBoard trước đó
                this.Remove(monster);

                GameMap gameMap = KTMapManager.Find(monster.CurrentMapCode);

                /// Thực hiện tìm đường
                List<UnityEngine.Vector2> nodeList;
                
                /// Nếu sử dụng A*
                if (useAStarPathFinder)
				{
                    nodeList = KTGlobal.FindPath(monster, new UnityEngine.Vector2((float) fromPos.X, (float) fromPos.Y), new UnityEngine.Vector2((float) toPos.X, (float) toPos.Y), monster.CurrentCopyMapID, 2000);
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
                this.SendMonsterMoveToClient(monster, pathStr, fromPos, toPos, action);

                //Console.WriteLine("Monster {3} => Move from {0} to {1} with paths = {2}", fromPos.ToString(), toPos.ToString(), pathStr, monster.RoleName);

                /// Vị trí hiện tại của người chơi
                UnityEngine.Vector2 currentPos = new UnityEngine.Vector2((int) monster.CurrentPos.X, (int) monster.CurrentPos.Y);
                /// Vị trí đích đến
                monster.ToPos = toPos;

                MonsterStoryBoard storyBoard = new MonsterStoryBoard()
                {
                    Owner = monster,
                    Paths = paths,
                    LastGridPos = KTGlobal.WorldPositionToGridPosition(gameMap, currentPos),
                    LastTick = KTGlobal.GetCurrentTimeMilis(),
                    Action = action,
                    HasCompletedLastMove = true,
                };
                this.objectTimers[monster.RoleID] = storyBoard;

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
        /// <param name="monster"></param>
        /// <param name="pathString"></param>
        /// <param name="action"></param>
        public void Add(Monster monster, string pathString, KE_NPC_DOING action = KE_NPC_DOING.do_run, bool forceMoveBySpecialState = false)
        {
            try
            {
                if (monster == null)
                {
                    return;
                }
                /// Nếu dính trạng thái không thể di chuyển
                else if ((!forceMoveBySpecialState && !monster.IsCanPositiveMove()) || (forceMoveBySpecialState && !monster.IsCanMove()))
                {
                    this.Remove(monster);
                    return;
                }

                /// Dừng thực thi StoryBoard trước đó
                this.Remove(monster);

                ConcurrentQueue<UnityEngine.Vector2> paths = new ConcurrentQueue<UnityEngine.Vector2>();
                string[] points = pathString.Split('|');
                if (points.Length < 2)
                {
                    return;
                }

                GameMap gameMap = KTMapManager.Find(monster.CurrentMapCode);

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
                    if (!KTMapManager.Find(monster.CurrentMapCode).CanMove(new Point(posX, posY), monster.CurrentCopyMapID))
                    {
                        continue;
                    }

                    toPos = new Point(posX, posY);

                    paths.Enqueue(new UnityEngine.Vector2(posX, posY));
                }

                /// Vị trí hiện tại của người chơi
                UnityEngine.Vector2 currentPos = new UnityEngine.Vector2((int) monster.CurrentPos.X, (int) monster.CurrentPos.Y);
                /// Vị trí đích đến
                monster.ToPos = toPos;

                MonsterStoryBoard storyBoard = new MonsterStoryBoard()
                {
                    Owner = monster,
                    Paths = paths,
                    LastGridPos = KTGlobal.WorldPositionToGridPosition(gameMap, currentPos),
                    LastTick = KTGlobal.GetCurrentTimeMilis(),
                    Action = action,
                    HasCompletedLastMove = true,
                };
                this.objectTimers[monster.RoleID] = storyBoard;

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
