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
	/// Quản lý API StoryBoard
	/// </summary>
	public partial class KTPlayerStoryBoardEx
	{
		#region Support
		/// <summary>
		/// Kiểm tra có StoryBoard của người chơi tương ứng đang thực thi không
		/// </summary>
		/// <param name="player"></param>
		/// <returns></returns>
		public bool HasStoryBoard(KPlayer player)
        {
            return this.objectTimers.ContainsKey(player.RoleID);
        }

        /// <summary>
        /// Trả về danh sách đường đi hiện tại của đối tượng
        /// </summary>
        /// <returns></returns>
        public string GetCurrentPathString(KPlayer player)
        {
            try
            {
                if (this.objectTimers.TryGetValue(player.RoleID, out PlayerStoryBoard storyBoard))
                {
                    if (storyBoard.Paths.Count <= 0 || storyBoard.Paths.Count > 10)
                    {
                        return "";
                    }
                    Queue<UnityEngine.Vector2> paths = new Queue<UnityEngine.Vector2>(storyBoard.Paths);

                    /// Vị trí hiện tại
                    UnityEngine.Vector2 currentPos = new UnityEngine.Vector2((int) player.CurrentPos.X, (int) player.CurrentPos.Y);
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
        /// <param name="player"></param>
        /// <param name="action"></param>
        public void ChangeAction(KPlayer player, KE_NPC_DOING action)
        {
            try
            {
                /// Nếu không phải đi bộ hoặc chạy thì bỏ qua
                if (action != KE_NPC_DOING.do_walk && action != KE_NPC_DOING.do_run)
                {
                    return;
                }

                /// Tìm Storyboard tương ứng
                if (this.objectTimers.TryGetValue(player.RoleID, out PlayerStoryBoard storyBoard))
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
		/// <param name="player"></param>
		/// <param name="sendToClient"></param>
		public void Remove(KPlayer player, bool sendToClient = true)
        {
            /// Toác
            if (player == null)
            {
                return;
            }

            try
            {
                /// Nếu tồn tại trong danh sách
                if (this.objectTimers.TryGetValue(player.RoleID, out PlayerStoryBoard objectTimer))
                {
                    /// Đánh dấu đã bị hủy
                    objectTimer.IsDisposed = true;
                    /// Cập nhật thời điểm Tick StoryBoard lần trước
                    player.LastStoryBoardTicks = KTGlobal.GetCurrentTimeMilis();
                    /// Xóa khỏi danh sách
                    this.objectTimers.TryRemove(player.RoleID, out _);
                    /// Thiết lập vị trí đích đến
                    player.ToPos = new Point(-1, -1);

                    /// Đồng bộ về Client
                    if (sendToClient)
                    {
                        this.SendPlayerStopMove(player);
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
        /// <param name="fromPos"></param>
        /// <param name="toPos"></param>
        /// <param name="action"></param>
        /// <param name="forceMoveBySpecialState"></param>
        public void Add(KPlayer player, Point fromPos, Point toPos, KE_NPC_DOING action = KE_NPC_DOING.do_run, bool forceMoveBySpecialState = false)
        {
            try
            {
                if (player == null)
                {
                    return;
                }
                else if (fromPos == default || toPos == default)
                {
                    return;
                }
                /// Nếu dính trạng thái không thể di chuyển
                else if ((!forceMoveBySpecialState && !player.IsCanPositiveMove()) || (forceMoveBySpecialState && !player.IsCanMove()))
                {
                    this.Remove(player);
                    return;
                }

                /// Nếu thể lực dưới 5%
                if (player.m_CurrentStamina * 100 / player.m_CurrentStaminaMax < 5)
                {
                    /// Ép về trạng thái đi bộ
                    action = KE_NPC_DOING.do_walk;
                }

                /// Cập nhật thời điểm Tick StoryBoard lần trước
                player.LastStoryBoardTicks = KTGlobal.GetCurrentTimeMilis();

                /// Nếu đang thao tác gì đó thì dừng
                player.CurrentProgress = null;

                /// Dừng thực thi StoryBoard trước đó
                this.Remove(player);

                GameMap gameMap = KTMapManager.Find(player.CurrentMapCode);

                /// Thực hiện tìm đường
                List<UnityEngine.Vector2> nodeList = KTGlobal.FindPath(player, new UnityEngine.Vector2((float) fromPos.X, (float) fromPos.Y), new UnityEngine.Vector2((float) toPos.X, (float) toPos.Y), player.CurrentCopyMapID);

                /// Nếu không có đường đi thì bỏ qua
                if (nodeList.Count < 2)
                {
                    return;
                }

                ///// Xóa Buff bảo vệ
                //player.RemoveChangeMapProtectionBuff();

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
                this.SendPlayerMoveToClient(player, pathStr, fromPos, toPos, action);

                //Console.WriteLine("Move from {0} to {1} with paths = {2}", fromPos.ToString(), toPos.ToString(), pathStr);

                /// Vị trí hiện tại của người chơi
                UnityEngine.Vector2 currentPos = new UnityEngine.Vector2((int) player.CurrentPos.X, (int) player.CurrentPos.Y);
                /// Vị trí đích đến
                player.ToPos = toPos;

                PlayerStoryBoard storyBoard = new PlayerStoryBoard()
                {
                    Owner = player,
                    RoleID = player.RoleID,
                    Paths = paths,
                    LastGridPos = KTGlobal.WorldPositionToGridPosition(gameMap, currentPos),
                    LastTick = KTGlobal.GetCurrentTimeMilis(),
                    Action = action,
                    HasCompletedLastMove = true,
                };
                this.objectTimers[player.RoleID] = storyBoard;

                /// Cập nhật thời điểm Tick StoryBoard lần trước
                player.LastStoryBoardTicks = KTGlobal.GetCurrentTimeMilis();

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
        /// <param name="player"></param>
        /// <param name="pathString"></param>
        /// <param name="action"></param>
        public void Add(KPlayer player, string pathString, KE_NPC_DOING action = KE_NPC_DOING.do_run, bool forceMoveBySpecialState = false)
        {
            try
            {
                if (player == null)
                {
                    return;
                }
                /// Nếu dính trạng thái không thể di chuyển
                else if ((!forceMoveBySpecialState && !player.IsCanPositiveMove()) || (forceMoveBySpecialState && !player.IsCanMove()))
                {
                    this.Remove(player);
                    return;
                }

                /// Nếu thể lực dưới 5%
                if (player.m_CurrentStamina * 100 / player.m_CurrentStaminaMax < 5)
                {
                    /// Ép về trạng thái đi bộ
                    action = KE_NPC_DOING.do_walk;
                }

                /// Cập nhật thời điểm Tick StoryBoard lần trước
                player.LastStoryBoardTicks = KTGlobal.GetCurrentTimeMilis();

                /// Nếu đang thao tác gì đó thì dừng
                player.CurrentProgress = null;

                /// Dừng thực thi StoryBoard trước đó
                this.Remove(player);

                ConcurrentQueue<UnityEngine.Vector2> paths = new ConcurrentQueue<UnityEngine.Vector2>();
                string[] points = pathString.Split('|');
                if (points.Length < 2)
                {
                    return;
                }

                ///// Xóa Buff bảo vệ
                //player.RemoveChangeMapProtectionBuff();

                GameMap gameMap = KTMapManager.Find(player.CurrentMapCode);

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
                    if (!KTMapManager.Find(player.CurrentMapCode).CanMove(new Point(posX, posY), player.CurrentCopyMapID))
                    {
                        continue;
                    }

                    toPos = new Point(posX, posY);

                    paths.Enqueue(new UnityEngine.Vector2(posX, posY));
                }

                //Console.WriteLine("Move with paths = {0}", pathString);

                /// Vị trí hiện tại của người chơi
                UnityEngine.Vector2 currentPos = new UnityEngine.Vector2((int) player.CurrentPos.X, (int) player.CurrentPos.Y);
                /// Vị trí đích đến
                player.ToPos = toPos;

                PlayerStoryBoard storyBoard = new PlayerStoryBoard()
                {
                    Owner = player,
                    RoleID = player.RoleID,
                    Paths = paths,
                    LastGridPos = KTGlobal.WorldPositionToGridPosition(gameMap, currentPos),
                    LastTick = KTGlobal.GetCurrentTimeMilis(),
                    Action = action,
                    HasCompletedLastMove = true,
                };
                this.objectTimers[player.RoleID] = storyBoard;

                /// Cập nhật thời điểm Tick StoryBoard lần trước
                player.LastStoryBoardTicks = KTGlobal.GetCurrentTimeMilis();

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
