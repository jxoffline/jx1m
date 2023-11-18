using GameServer.KiemThe.Entities;
using GameServer.KiemThe.Utilities;
using GameServer.Logic;
using Server.Tools;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using static GameServer.Logic.KTMapManager;

namespace GameServer.KiemThe.Logic
{
    /// <summary>
    /// Thực hiện Logic StoryBoard
    /// </summary>
    public static class KTStoryBoardLogic
    {
        /// <summary>
        /// Số lần thử tối đa di chuyển Storyboard
        /// </summary>
        private const int MaxStepMoveTriedSteps = 5;

        /// <summary>
        /// Kiểm tra vị trí tương ứng có thể đi được không
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        private static bool IsValidPos(int mapCode, UnityEngine.Vector2 pos, int copySceneID)
        {
            GameMap gameMap = KTMapManager.Find(mapCode);
            return KTGlobal.IsGridReachable(mapCode, (int) pos.x / gameMap.MapGridWidth, (int) pos.y / gameMap.MapGridHeight, copySceneID);
        }

        /// <summary>
        /// Kiểm tra StoryBoard của đối tượng còn tồn tại không
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private static bool IsStoryBoardAlive(GameObject obj)
		{
            /// Nếu là người chơi
            if (obj is KPlayer player)
			{
                return KTPlayerStoryBoardEx.Instance.HasStoryBoard(player);
			}
            /// Nếu là quái
            else if (obj is Monster monster)
			{
                return KTMonsterStoryBoardEx.Instance.HasStoryBoard(monster);
			}
            /// Toác
            return false;
		}


        /// <summary>
        /// Dịch chuyển đối tượng qua lại giữa 2 vị trí theo khoảng thời gian tương ứng
        /// </summary>
        /// <param name="obj">Đối tượng</param>
        /// <param name="pathsList">Đoạn đường</param>
        /// <param name="elapseTimeSec">Thời gian tổng cần mô phỏng</param>
        /// <param name="action">Động tác đang thực hiện (chạy hay đi bộ)</param>
        /// <param name="needToStopImmediately">Có phải dừng ngay lập tức không (thường xảy ra khi StoryBoard của đối tượng bị hủy)</param>
        /// <returns>True: Dừng StoryBoard đang thực thi, False: Không dừng StoryBoard đang thực thi.</returns>
        public static bool StepMove(GameObject obj, ConcurrentQueue<UnityEngine.Vector2> pathsList, float elapseTimeSec, KE_NPC_DOING action, Func<bool> needToStopImmediately)
		{
            /// Số lần đã thử
            int triedStep = 0;

            /// Hàm đệ quy thực hiện di chuyển
            bool DoStepMove(GameObject go, ConcurrentQueue<UnityEngine.Vector2> paths, float elapseTime)
            {
                /// Nếu số lần đã thử vượt quá số lần cho phép
                if (triedStep >= KTStoryBoardLogic.MaxStepMoveTriedSteps)
                {
                    /// Trả ra kết quả dừng StoryBoard đang thực thi do lỗi gì đó
                    return false;
                }
                /// Tăng số lần đã thử lên
                triedStep++;

                try
                {
                    /// Nếu phải dừng ngay lập tức
                    if (needToStopImmediately())
                    {
                        /// Trả về kết quả dừng StoryBoard đang thực thi
                        return true;
                    }

					/// Nếu StoryBoard không còn tồn tại
					if (go is KPlayer && !KTStoryBoardLogic.IsStoryBoardAlive(go))
					{
                        /// Trả về kết quả dừng StoryBoard đang thực thi
                        return true;
					}

					/// Nếu thời gian cần mô phỏng dưới 0
					if (elapseTime <= 0)
                    {
                        /// Trả ra kết quả dừng StoryBoard đang thực thi
                        return true;
                    }
                    /// Tốc độ di chuyển của đối tượng
                    float moveSpeed = KTGlobal.MoveSpeedToPixel(go.GetCurrentRunSpeed()) / (action == KE_NPC_DOING.do_walk ? 2 : 1);
                    /// Nếu tốc độ di chuyển dưới 0
                    if (moveSpeed <= 0)
                    {
                        /// Trả ra kết quả dừng StoryBoard đang thực thi
                        return true;
                    }
                    /// Nếu danh sách đoạn đường đã rỗng
                    if (paths.Count <= 0)
                    {
                        /// Trả ra kết quả dừng StoryBoard đang thực thi
                        return true;
                    }
                    /// Vị trí hiện tại của đối tượng
                    UnityEngine.Vector2 currentPos = new UnityEngine.Vector2((int) go.CurrentPos.X, (int) go.CurrentPos.Y);
                    /// Vị trí đích tiếp theo
                    paths.TryPeek(out UnityEngine.Vector2 nextPos);
                    /// Nếu không phải quái, xe tiêu
                    if (!(go is Monster) && !(go is TraderCarriage))
                    {
                        /// Nếu vị trí đích không thể đến được
                        if (!KTStoryBoardLogic.IsValidPos(go.CurrentMapCode, nextPos, go.CurrentCopyMapID) || KTGlobal.InDynamicObs(go.CurrentMapCode, (int) currentPos.x, (int) currentPos.y, go.CurrentCopyMapID))
                        {
                            /// Trả ra kết quả dừng StoryBoard đang thực thi
                            return true;
                        }
                    }
                    
                    /// Vector chỉ hướng di chuyển
                    UnityEngine.Vector2 dirVector = nextPos - currentPos;
                    /// Khoảng cách giữa vị trí hiện tại và đích tiếp theo
                    float distance = UnityEngine.Vector2.Distance(currentPos, nextPos);
                    /// Thời gian cần để di chuyển từ vị trí hiện tại đến đích
                    float timeToNextPos = distance / moveSpeed;

                    /// Nếu thời gian từ vị trí hiện tại đến vị trí đích lớn hơn thời gian tổng cần mô phỏng
                    if (timeToNextPos >= elapseTime)
                    {
                        float t = elapseTime / timeToNextPos;
                        /// Vị trí mới của đối tượng
                        UnityEngine.Vector2 newPos = UnityEngine.Vector2.Lerp(currentPos, nextPos, t);

                        /// Toác gì đó
                        if (newPos == UnityEngine.Vector2.zero)
                        {
                            LogManager.WriteLog(LogTypes.RolePosition, string.Format("Storyboard of {0} has stopped because newPos is Empty. Stop pos: 1", go.RoleName));
                            //Console.WriteLine(string.Format("Storyboard of {0} has stopped because newPos is Empty. Stop pos: 1", go.RoleName));
                            /// Trả ra kết quả không dừng StoryBoard đang thực thi
                            return false;
                        }

                        /// Nếu phải dừng ngay lập tức
                        if (needToStopImmediately())
                        {
                            /// Trả về kết quả dừng StoryBoard đang thực thi
                            return true;
                        }

                        /// Cập nhật vị trí mới cho đối tượng
                        go.CurrentPos = new System.Windows.Point(newPos.x, newPos.y);

                        /// Hướng quay của đối tượng
                        float rotationAngle = KTMath.GetAngle360WithXAxis(dirVector);
                        go.CurrentDir = KTMath.GetDirectionByAngle360(rotationAngle);

                        /// Trả ra kết quả không dừng StoryBoard đang thực thi
                        return false;
                    }
                    else if (timeToNextPos <= 0)
                    {
                        /// Vị trí mới của đối tượng
                        paths.TryDequeue(out UnityEngine.Vector2 newPos);

                        if (newPos == UnityEngine.Vector2.zero)
                        {
                            //LogManager.WriteLog(LogTypes.RolePosition, string.Format("Storyboard of {0} has stopped because newPos is Empty. Stop pos: 2", go.RoleName));
                            //Console.WriteLine(string.Format("Storyboard of {0} has stopped because newPos is Empty. Stop pos: 2", go.RoleName));
                            /// Thực hiện di chuyển tiếp
                            return DoStepMove(go, paths, elapseTime);
                            ///// Trả ra kết quả dừng StoryBoard đang thực thi
                            //return true;
                        }

                        /// Nếu phải dừng ngay lập tức
                        if (needToStopImmediately())
                        {
                            /// Trả về kết quả dừng StoryBoard đang thực thi
                            return true;
                        }

                        /// Cập nhật vị trí mới cho đối tượng
                        go.CurrentPos = new System.Windows.Point(newPos.x, newPos.y);

                        /// Thực hiện di chuyển tiếp
                        return DoStepMove(go, paths, elapseTime);
                    }
                    else
                    {
                        /// Vị trí mới của đối tượng
                        paths.TryDequeue(out UnityEngine.Vector2 newPos);

                        /// Giảm thời gian cần mô phỏng
                        elapseTime -= timeToNextPos;

                        if (newPos == UnityEngine.Vector2.zero)
                        {
                            //LogManager.WriteLog(LogTypes.RolePosition, string.Format("Storyboard of {0} has stopped because newPos is Empty. Stop pos: 3", go.RoleName));
                            //Console.WriteLine(string.Format("Storyboard of {0} has stopped because newPos is Empty. Stop pos: 3", go.RoleName));

                            /// Thực hiện di chuyển tiếp
                            return DoStepMove(go, paths, elapseTime);
                            ///// Trả ra kết quả dừng StoryBoard đang thực thi
                            //return true;
                        }

                        /// Nếu phải dừng ngay lập tức
                        if (needToStopImmediately())
                        {
                            /// Trả về kết quả dừng StoryBoard đang thực thi
                            return true;
                        }

                        /// Cập nhật vị trí mới cho đối tượng
                        go.CurrentPos = new System.Windows.Point(newPos.x, newPos.y);

                        /// Hướng quay của đối tượng
                        float rotationAngle = KTMath.GetAngle360WithXAxis(dirVector);
                        go.CurrentDir = KTMath.GetDirectionByAngle360(rotationAngle);

                        /// Thực hiện di chuyển tiếp
                        return DoStepMove(go, paths, elapseTime);
                    }
                }
                catch (Exception ex)
                {
                    //Console.WriteLine("Exception -> " + ex.ToString());
                    LogManager.WriteLog(LogTypes.Exception, ex.ToString());
                    /// Tăng Step lên để nó buộc phải Return
                    triedStep = KTStoryBoardLogic.MaxStepMoveTriedSteps;
                    /// Trả ra kết quả dừng StoryBoard đang thực thi
                    return true;
                }
            }
            /// Gọi hàm thực hiện logic
            return DoStepMove(obj, pathsList, elapseTimeSec);
        }
    }
}
