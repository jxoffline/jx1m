using FS.GameEngine.Logic;
using FS.GameEngine.Sprite;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static FS.VLTK.Entities.Enum;

namespace FS.VLTK.Logic
{
    /// <summary>
    /// Quản lý tương tác
    /// </summary>
    public partial class KTStoryBoard
    {
        /// <summary>
        /// Thêm đối tượng vào StoryBoard
        /// </summary>
        /// <param name="obj">Đối tượng</param>
        /// <param name="fromPos">Tọa độ thực điểm đầu</param>
        /// <param name="toPos">Tọa độ thực điểm cuối</param>
        /// <param name="moveDone">Hàm callback khi hoàn tất di chuyển</param>
        public void Add(GSprite obj, Vector2 fromPos, Vector2 toPos, Action moveDone = null, KE_NPC_DOING action = KE_NPC_DOING.do_run)
        {
            if (obj == null)
            {
                return;
            }
            else if (fromPos == default || toPos == default)
            {
                return;
            }

            /// Thực hiện tìm đường
            List<Vector2> pathFromStoryBoard = Global.Data.GameScene.FindPath(obj, ref fromPos, ref toPos);

            /// Nếu không có đường đi thì bỏ qua
            if (pathFromStoryBoard == null || pathFromStoryBoard.Count < 2)
            {
                return;
            }

            /// Hàng đợi danh sách các điểm trên đường đi
            Queue<Vector2> paths = new Queue<Vector2>();
            /// Nạp tất cả các điểm vừa tìm được vào hàng đợi
            foreach (Vector2 pos in pathFromStoryBoard)
            {
                paths.Enqueue(pos);
            }
            
            /// Thực hiện di chuyển
            obj.StartMove(paths, action, moveDone);
        }

        /// <summary>
        /// Thêm đối tượng vào StoryBoard (bỏ qua vị trí đầu tiên)
        /// </summary>
        /// <param name="obj">Đối tượng</param>
        /// <param name="pathString">Đường đi dạng tọa độ lưới</param>
        /// <param name="moveDone">Hàm callback khi hoàn tất di chuyển</param>
        /// <param name="skipFirstPath">Bỏ qua vị trí đầu tiên</param>
        public void Add(GSprite obj, string pathString, Action moveDone = null, KE_NPC_DOING action = KE_NPC_DOING.do_run, bool skipFirstPath = true)
        {
            if (obj == null)
            {
                return;
            }
            else if (string.IsNullOrEmpty(pathString))
            {
                return;
            }

            string[] param = pathString.Split('|');
            if (param.Length < 2)
            {
                return;
            }

            Queue<Vector2> paths = new Queue<Vector2>();
            try
            {
                /// Thêm vị trí hiện tại vào vị trí đầu tiên
                paths.Enqueue(obj.PositionInVector2);

                /// Bỏ qua vị trí đầu tiên
                for (int i = skipFirstPath ? 1 : 0; i < param.Length; i++)
                {
                    string[] pos = param[i].Split('_');
                    if (pos.Length < 2)
                    {
                        throw new Exception();
                    }

                    int posX = int.Parse(pos[0]);
                    int posY = int.Parse(pos[1]);

                    paths.Enqueue(new Vector2(posX, posY));
                }
            }
            catch (Exception)
            {
                return;
            }

            /// Thực hiện di chuyển
            obj.StartMove(paths, action, moveDone);
        }

        /// <summary>
        /// Thêm hoặc thay đổi giá trị của StoryBoard hiện tại nếu đã tồn tại
        /// </summary>
        /// <param name="obj">Đối tượng</param>
        /// <param name="pathString">Đường đi dạng tọa độ lưới</param>
        /// <param name="moveDone">Hàm callback khi hoàn tất di chuyển</param>
        /// <param name="action">Động tác di chuyển</param>
        /// <param name="skipFirstPath">Bỏ qua vị trí đầu tiên</param>
        public void AddOrUpdate(GSprite obj, string pathString, Action moveDone = null, KE_NPC_DOING action = KE_NPC_DOING.do_run, bool skipFirstPath = true)
        {
            if (obj == null)
            {
                return;
            }
            else if (string.IsNullOrEmpty(pathString))
            {
                return;
            }

            string[] param = pathString.Split('|');
            if (param.Length < 2)
            {
                return;
            }

            Queue<Vector2> paths = new Queue<Vector2>();
            try
            {
                /// Bỏ qua vị trí đầu tiên
                for (int i = skipFirstPath ? 1 : 0; i < param.Length; i++)
                {
                    string[] pos = param[i].Split('_');
                    if (pos.Length < 2)
                    {
                        throw new Exception();
                    }

                    int posX = int.Parse(pos[0]);
                    int posY = int.Parse(pos[1]);

                    paths.Enqueue(new Vector2(posX, posY));
                }
            }
            catch (Exception)
            {
                return;
            }

            /// Thực hiện di chuyển
            obj.StartMove(paths, action, moveDone);
        }

        /// <summary>
        /// Xóa toàn bộ Storyboard
        /// </summary>
        public void RemoveAllStoryBoards()
		{
            this.StopAllCoroutines();
		}
    }
}
