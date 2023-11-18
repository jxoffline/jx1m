using FS.Drawing;
using UnityEngine;
using FS.GameEngine.Logic;
using FS.GameEngine.Sprite;
using FS.GameFramework.Logic;
using FS.VLTK.Network;
using FS.VLTK.Logic;
using FS.VLTK;
using static FS.VLTK.Entities.Enum;
using FS.GameEngine.Network;
using FS.GameEngine.GoodsPack;
using FS.VLTK.Control.Component;
using System;

namespace FS.GameEngine.Scene
{
    /// <summary>
    /// Quản lý động tác
    /// </summary>
    public partial class GScene
    {
        #region Sự kiện
        /// <summary>
        /// Sự kiện khi Click vào điểm thu thập
        /// </summary>
        /// <param name="sprite"></param>
        /// <param name="callback"></param>
        public void GrowPointClick(GSprite sprite, Action callback = null)
        {
            /// Nếu không có đối tượng
            if (sprite == null)
            {
                return;
            }

            ///// Nếu vị trí đích không đến được
            //if (!this.CanMoveByWorldPos(sprite.Coordinate))
            //{
            //    KTGlobal.AddNotification("Vị trí này không thể đến được!");
            //    return;
            //}

            /// Nếu đang trong trạng thái khinh công thì không thao tác
            if (Global.Data.Leader.CurrentAction == KE_NPC_DOING.do_jump)
            {
                return;
            }
            /// Nếu chết thì không thao tác
            else if (this.Leader.IsDeath || this.Leader.HP <= 0)
            {
                return;
            }
            /// Nếu đang bày bán
            else if (Global.Data.StallDataItem != null && Global.Data.StallDataItem.Start == 1 && !Global.Data.StallDataItem.IsBot)
            {
                KTGlobal.AddNotification("Trong trạng thái bán hàng không thể thao tác!");
                return;
            }
            /// Nếu đang bị khóa bởi kỹ năng
            else if (!this.Leader.CanPositiveMove)
            {
                KTGlobal.AddNotification("Đang trong trạng thái bị khống chế, không thể thao tác!");
                return;
            }
            /// Nếu chưa thực hiện xong động tác trước
            else if (!this.Leader.IsReadyToMove)
            {
                return;
            }
            /// Nếu đang trong thời gian thực hiện động tác dùng kỹ năng
            else if (!KTGlobal.FinishedUseSkillAction)
            {
                return;
            }
            /// Nếu đang đợi dùng kỹ năng
            else if (SkillManager.IsWaitingToUseSkill)
            {
                return;
            }

            /// Ngừng di chuyển
            KTLeaderMovingManager.StopMove();
            KTLeaderMovingManager.StopChasingTarget();

            /// Đánh dấu có đối tượng vừa được chọn để tránh Click-Move
            KTGlobal.IsClickOnTarget = true;

            /// Vị trí của đối tượng
            Vector2 sprPos = sprite.PositionInVector2;

            /// Vị trí của Leader
            Vector2 leaderPos = Global.Data.Leader.PositionInVector2;

            /// Vector chỉ hướng di chuyển
            Vector2 dirVector = sprPos - leaderPos;

            /// Khoảng cách đến đối tượng
            float distance = Vector2.Distance(sprPos, leaderPos);

            /// Nếu khoảng cách nằm trong vùng cho phép tương tác với đối tượng
            if (distance <= 50)
            {
                /// Nếu không thể thực hiện Logic
                if (!Global.Data.Leader.CanDoLogic)
                {
                    return;
                }

                /// Ngừng di chuyển
                KTLeaderMovingManager.StopMove();
                KTLeaderMovingManager.StopChasingTarget();

                /// Gửi gói tin thông báo click vào điểm thu thập
                KT_TCPHandler.GrowPointClick(sprite.RoleID);

                /// Thực thi sự kiện khi hoàn tất
                callback?.Invoke();
            }
            /// Nếu khoảng cách chưa nằm trong vùng cho phép tương tác với đối tượng thì tiến hành dịch chuyển đến vị trí tương ứng
            else
            {
                /// Nếu không thể di chuyển
                if (!Global.Data.Leader.CanMove)
                {
                    return;
                }

                /// Vị trí đích đến điểm tương tác với đối tượng
                Vector2 destPos = KTMath.FindPointInVectorWithDistance(leaderPos, dirVector, distance - 45);
                /// Nếu điểm đích không đến được
                if (!this.CanMoveByWorldPos(new Point((int) destPos.x, (int) destPos.y)))
                {
                    destPos = sprPos;
                }
                //Vector2 destPos = sprPos;
                /// Nếu điểm đích không đến được
                if (!this.CanMoveByWorldPos(new Point((int) destPos.x, (int) destPos.y)))
                {
                    KTGlobal.AddNotification("Vị trí này không đến được!");
                    return;
                }

                /// Thực hiện di chuyển đến vị trí đích để tương tác với đối tượng
                KTLeaderMovingManager.AutoFindRoad(new Point((int) destPos.x, (int) destPos.y), () => {
                    this.GrowPointClick(sprite, callback);
                });
            }
        }

        /// <summary>
        /// Sự kiện khi Click vào vật phẩm rơi dưới đất
        /// </summary>
        /// <param name="goodsPack"></param>
        public void DropItemClick(GGoodsPack goodsPack)
        {
            /// Nếu không có vật phẩm
            if (goodsPack == null)
            {
                return;
            }

            ///// Nếu vị trí đích không đến được
            //if (!this.CanMoveByWorldPos(goodsPack.Coordinate))
            //{
            //    KTGlobal.AddNotification("Vị trí này không thể đến được!");
            //    return;
            //}

            /// Nếu đang trong trạng thái khinh công thì không thao tác
            if (Global.Data.Leader.CurrentAction == KE_NPC_DOING.do_jump)
            {
                return;
            }
            /// Nếu chết thì không thao tác
            else if (this.Leader.IsDeath || this.Leader.HP <= 0)
            {
                return;
            }
            /// Nếu đang bày bán
            else if (Global.Data.StallDataItem != null && Global.Data.StallDataItem.Start == 1 && !Global.Data.StallDataItem.IsBot)
            {
                KTGlobal.AddNotification("Trong trạng thái bán hàng không thể thao tác!");
                return;
            }
            /// Nếu đang bị khóa bởi kỹ năng
            else if (!this.Leader.CanPositiveMove)
            {
                KTGlobal.AddNotification("Đang trong trạng thái bị khống chế, không thể thao tác!");
                return;
            }
            /// Nếu chưa thực hiện xong động tác trước
            else if (!this.Leader.IsReadyToMove)
            {
                return;
            }
            /// Nếu đang trong thời gian thực hiện động tác dùng kỹ năng
            else if (!KTGlobal.FinishedUseSkillAction)
            {
                return;
            }
            /// Nếu đang đợi dùng kỹ năng
            else if (SkillManager.IsWaitingToUseSkill)
            {
                return;
            }

            /// Ngừng di chuyển
            KTLeaderMovingManager.StopMove();
            KTLeaderMovingManager.StopChasingTarget();

            /// Đánh dấu có đối tượng vừa được chọn để tránh Click-Move
            KTGlobal.IsClickOnTarget = true;

            /// Vị trí của item
            Vector2 itemPos = goodsPack.PositionInVector2;

            /// Vị trí của Leader
            Vector2 leaderPos = Global.Data.Leader.PositionInVector2;

            /// Vector chỉ hướng di chuyển
            Vector2 dirVector = itemPos - leaderPos;

            /// Khoảng cách đến NPC
            float distance = Vector2.Distance(itemPos, leaderPos);

            /// Nếu khoảng cách nằm trong vùng cho phép tương tác với item
            if (distance <= 50)
            {
                /// Nếu không thể thực hiện Logic
                if (!Global.Data.Leader.CanDoLogic)
                {
                    return;
                }

                /// Gửi gói tin nhặt đồ lên GS
                GameInstance.Game.SpriteGetThing(goodsPack.BaseID);
            }
            /// Nếu khoảng cách chưa nằm trong vùng cho phép tương tác với item thì tiến hành dịch chuyển đến vị trí tương ứng
            else
            {
                /// Nếu không thể di chuyển
                if (!Global.Data.Leader.CanMove)
                {
                    return;
                }

                /// Vị trí đích đến điểm tương tác với item
                Vector2 destPos = KTMath.FindPointInVectorWithDistance(leaderPos, dirVector, distance - 45);
                /// Nếu điểm đích không đến được
                if (!this.CanMoveByWorldPos(new Point((int) destPos.x, (int) destPos.y)))
                {
                    destPos = itemPos;
                }
                //Vector2 destPos = itemPos;
                /// Nếu điểm đích không đến được
                if (!this.CanMoveByWorldPos(new Point((int) destPos.x, (int) destPos.y)))
                {
                    KTGlobal.AddNotification("Vị trí này không đến được!");
                    return;
                }

                /// Thực hiện di chuyển đến vị trí đích để tương tác với item
                KTLeaderMovingManager.AutoFindRoad(new Point((int) destPos.x, (int) destPos.y), () => {
                    this.DropItemClick(goodsPack);
                });
            }
        }

        /// <summary>
        /// Sự kiện khi Click vào người chơi khác
        /// </summary>
        /// <param name="sprite"></param>
        public void OtherRoleClick(GSprite sprite)
        {
            /// Chọn đối tượng
            this.SetSelectTarget(sprite);
            /// Đánh dấu có đối tượng vừa được chọn để tránh Click-Move
            KTGlobal.IsClickOnTarget = true;

            /// Thông báo hiển thị khung mặt đối tượng
            PlayZone.Instance.HideAllFace();
            PlayZone.Instance.NotifyRoleFace(sprite.RoleID, -1, true);

            /// Nếu đối phương đang bày bán
            if (!string.IsNullOrEmpty(sprite.RoleData.StallName))
            {
                this.PlayerShopClick(sprite);
            }
        }

        /// <summary>
        /// Sự kiện khi click vào quái
        /// </summary>
        /// <param name="sprite"></param>
        public void MonsterClick(GSprite sprite)
        {
            /// Chọn đối tượng
            this.SetSelectTarget(sprite);
            /// Đánh dấu có đối tượng vừa được chọn để tránh Click-Move
            KTGlobal.IsClickOnTarget = true;

            /// Thông báo hiển thị khung mặt đối tượng
            PlayZone.Instance.HideAllFace();
            PlayZone.Instance.NotifyRoleFace(sprite.RoleID, -1, true);
        }

        /// <summary>
        /// Sự kiện khi click vào pet
        /// </summary>
        /// <param name="sprite"></param>
        public void PetClick(GSprite sprite)
        {
            /// Chọn đối tượng
            this.SetSelectTarget(sprite);
            /// Đánh dấu có đối tượng vừa được chọn để tránh Click-Move
            KTGlobal.IsClickOnTarget = true;

            /// Thông báo hiển thị khung mặt đối tượng
            PlayZone.Instance.HideAllFace();
            PlayZone.Instance.NotifyRoleFace(sprite.RoleID, -1, true);
        }

        /// <summary>
        /// Sự kiện khi click NPC
        /// </summary>
        public void NPCClick(GSprite sprite)
        {
            /// Nếu không có NPC
            if (sprite == null)
            {
                return;
            }

            ///// Nếu vị trí đích không đến được
            //if (!this.CanMoveByWorldPos(sprite.Coordinate))
            //{
            //    KTGlobal.AddNotification("Vị trí này không thể đến được!");
            //    return;
            //}

            /// Nếu đang trong trạng thái khinh công thì không thao tác
            if (Global.Data.Leader.CurrentAction == KE_NPC_DOING.do_jump)
            {
                return;
            }
            /// Nếu chết thì không thao tác
            else if (this.Leader.IsDeath || this.Leader.HP <= 0)
            {
                return;
            }
            /// Nếu đang bày bán
            else if (Global.Data.StallDataItem != null && Global.Data.StallDataItem.Start == 1 && !Global.Data.StallDataItem.IsBot)
            {
                KTGlobal.AddNotification("Trong trạng thái bán hàng không thể thao tác!");
                return;
            }
            /// Nếu đang bị khóa bởi kỹ năng
            else if (!this.Leader.CanPositiveMove)
            {
                KTGlobal.AddNotification("Đang trong trạng thái bị khống chế, không thể thao tác!");
                return;
            }
            /// Nếu chưa thực hiện xong động tác trước
            else if (!this.Leader.IsReadyToMove)
            {
                return;
            }
            /// Nếu đang trong thời gian thực hiện động tác dùng kỹ năng
            else if (!KTGlobal.FinishedUseSkillAction)
            {
                return;
            }
            /// Nếu đang đợi dùng kỹ năng
            else if (SkillManager.IsWaitingToUseSkill)
            {
                return;
            }

            /// Ngừng di chuyển
            KTLeaderMovingManager.StopMove();
            KTLeaderMovingManager.StopChasingTarget();

            /// Chọn đối tượng
            this.SetSelectTarget(sprite);
            /// Đánh dấu có đối tượng vừa được chọn để tránh Click-Move
            KTGlobal.IsClickOnTarget = true;

            /// Thông báo hiển thị khung mặt đối tượng
            PlayZone.Instance.HideAllFace();

            /// Vị trí của NPC
            Vector2 npcPos = sprite.PositionInVector2;

            /// Vị trí của Leader
            Vector2 leaderPos = Global.Data.Leader.PositionInVector2;

            /// Vector chỉ hướng di chuyển
            Vector2 dirVector = npcPos - leaderPos;

            /// Khoảng cách đến NPC
            float distance = Vector2.Distance(npcPos, leaderPos);

            /// Nếu khoảng cách nằm trong vùng cho phép đối thoại cùng NPC
            if (distance <= 50)
            {
                /// Nếu không thể thực hiện Logic
                if (!Global.Data.Leader.CanDoLogic)
                {
                    return;
                }
                /// Tạm thời đổi hướng của NPC
                sprite.TempChangeDirFollowTarget(Global.Data.Leader, 10f);

                /// Ngừng di chuyển
                KTLeaderMovingManager.StopMove();
                KTLeaderMovingManager.StopChasingTarget();

                /// Thực thi hàm Click vào NPC
                KT_TCPHandler.NPCClick(Global.Data.TargetNpcID);
            }
            /// Nếu khoảng cách chưa nằm trong vùng cho phép đối thoại cùng NPC thì tiến hành dịch chuyển đến vị trí tương ứng
            else
            {
                /// Nếu không thể di chuyển
                if (!Global.Data.Leader.CanMove)
                {
                    return;
                }

                /// Vị trí đích đến điểm nói chuyện với NPC
                Vector2 destPos = KTMath.FindPointInVectorWithDistance(leaderPos, dirVector, distance - 45);
                /// Nếu điểm đích không đến được
                if (!this.CanMoveByWorldPos(new Point((int) destPos.x, (int) destPos.y)))
                {
                    destPos = npcPos;
                }
                //Vector2 destPos = npcPos;
                /// Nếu điểm đích không đến được
                if (!this.CanMoveByWorldPos(new Point((int) destPos.x, (int) destPos.y)))
                {
                    return;
                }

                /// Thực hiện di chuyển đến vị trí đích để đối thoại cùng NPC
                KTLeaderMovingManager.AutoFindRoad(new Point((int) destPos.x, (int) destPos.y), () => {
                    this.NPCClick(sprite);
                });
            }
        }

        /// <summary>
        /// Sự kiện khi Click vào cửa hàng của người chơi
        /// </summary>
        /// <param name="goodsPack"></param>
        public void PlayerShopClick(GSprite sprite)
        {
            /// Nếu không có đối tượng
            if (sprite == null)
            {
                return;
            }
            /// Nếu không phải người chơi
            else if (sprite.ComponentCharacter == null)
            {
                return;
            }
            /// Nếu là Leader
            else if (sprite.RoleID == Global.Data.RoleData.RoleID)
            {
                return;
            }

            ///// Nếu vị trí đích không đến được
            //if (!this.CanMoveByWorldPos(sprite.Coordinate))
            //{
            //    KTGlobal.AddNotification("Vị trí này không thể đến được!");
            //    return;
            //}

            /// Nếu đang trong trạng thái khinh công thì không thao tác
            if (Global.Data.Leader.CurrentAction == KE_NPC_DOING.do_jump)
            {
                return;
            }
            /// Nếu chết thì không thao tác
            else if (this.Leader.IsDeath || this.Leader.HP <= 0)
            {
                return;
            }
            /// Nếu đang bày bán
            else if (Global.Data.StallDataItem != null && Global.Data.StallDataItem.Start == 1 && !Global.Data.StallDataItem.IsBot)
            {
                KTGlobal.AddNotification("Trong trạng thái bán hàng không thể thao tác!");
                return;
            }
            /// Nếu đang bị khóa bởi kỹ năng
            else if (!this.Leader.CanPositiveMove)
            {
                KTGlobal.AddNotification("Đang trong trạng thái bị khống chế, không thể thao tác!");
                return;
            }
            /// Nếu chưa thực hiện xong động tác trước
            else if (!this.Leader.IsReadyToMove)
            {
                return;
            }
            /// Nếu đang trong thời gian thực hiện động tác dùng kỹ năng
            else if (!KTGlobal.FinishedUseSkillAction)
            {
                return;
            }
            /// Nếu đang đợi dùng kỹ năng
            else if (SkillManager.IsWaitingToUseSkill)
            {
                return;
            }

            /// Ngừng di chuyển
            KTLeaderMovingManager.StopMove();
            KTLeaderMovingManager.StopChasingTarget();

            /// Đánh dấu có đối tượng vừa được chọn để tránh Click-Move
            KTGlobal.IsClickOnTarget = true;

            /// Vị trí của người chơi
            Vector2 playerPos = sprite.PositionInVector2;

            /// Vị trí của Leader
            Vector2 leaderPos = Global.Data.Leader.PositionInVector2;

            /// Vector chỉ hướng di chuyển
            Vector2 dirVector = playerPos - leaderPos;

            /// Khoảng cách đến người chơi
            float distance = Vector2.Distance(playerPos, leaderPos);

            /// Nếu khoảng cách nằm trong vùng cho phép tương tác với người chơi
            if (distance <= 50)
            {
                /// Nếu không thể thực hiện Logic
                if (!Global.Data.Leader.CanDoLogic)
                {
                    return;
                }
                /// Ngừng di chuyển
                KTLeaderMovingManager.StopMove();
                KTLeaderMovingManager.StopChasingTarget();

                /// Thực hiện gửi yêu cầu mở sạp hàng
                KT_TCPHandler.SendOpenStall(sprite.RoleID);
            }
            /// Nếu khoảng cách chưa nằm trong vùng cho phép tương tác với người chơi tương ứng thì tiến hành dịch chuyển đến vị trí tương ứng
            else
            {
                /// Nếu không thể di chuyển
                if (!Global.Data.Leader.CanMove)
                {
                    return;
                }

                /// Vị trí đích đến điểm tương tác với người chơi tương ứng
                Vector2 destPos = KTMath.FindPointInVectorWithDistance(leaderPos, dirVector, distance - 45);
                /// Nếu điểm đích không đến được
                if (!this.CanMoveByWorldPos(new Point((int) destPos.x, (int) destPos.y)))
                {
                    destPos = playerPos;
                }
                //Vector2 destPos = playerPos;
                /// Nếu điểm đích không đến được
                if (!this.CanMoveByWorldPos(new Point((int) destPos.x, (int) destPos.y)))
                {
                    KTGlobal.AddNotification("Vị trí này không đến được!");
                    return;
                }

                /// Thực hiện di chuyển đến vị trí đích để tương tác với người chơi tương ứng
                KTLeaderMovingManager.AutoFindRoad(new Point((int) destPos.x, (int) destPos.y), () => {
                    /// Thực hiện gửi yêu cầu mở sạp hàng
                    KT_TCPHandler.SendOpenStall(sprite.RoleID);
                });
            }
        }
        #endregion
    }
}
