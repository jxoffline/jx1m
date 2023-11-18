using System;
using FS.Drawing;
using UnityEngine;
using FS.GameEngine.Logic;
using FS.GameEngine.Sprite;
using FS.VLTK.Control.Component;
using FS.VLTK.Logic;
using FS.VLTK;
using System.Collections;
using static FS.VLTK.Entities.Enum;

namespace FS.GameEngine.Scene
{
    /// <summary>
    /// Quản lý sự kiện I/O
    /// </summary>
    public partial class GScene
    {
        #region Di chuyển
        /// <summary>
        /// Thời điểm Tick lần trước của JoyStick
        /// </summary>
        private long LastJoyStickTick = 0;

        /// <summary>
        /// Thời gian giãn cách giữa 2 lần liên tiếp kiểm tra hướng của JoyStick
        /// </summary>
        private readonly int JoyStickCheckDirectionChangePeriodMilis = 200;

        /// <summary>
        /// Thời gian cập nhật tọa độ mới của JoyStick
        /// </summary>
        private readonly int JoyStickChangePosEveryMilis = 1000;

        /// <summary>
        /// Thời điểm Click di chuyển lần trước
        /// </summary>
        private long LastClickTick = 0;

        /// <summary>
        /// Thời gian giãn cách giữa 2 lần Click di chuyển liên tiếp
        /// </summary>
        private readonly int ClickCheckPeriodMilis = 100;

        /// <summary>
        /// Hướng di chuyển lần trước của JoyStick
        /// </summary>
        private VLTK.Entities.Enum.Direction16 LastJoyStickDir = VLTK.Entities.Enum.Direction16.None;

        /// <summary>
        /// Tọa độ trước đó của JoyStick
        /// </summary>
        private Vector2 LastJoyPosition = Vector2.zero;

        /// <summary>
        /// Tọa độ dùng kỹ năng tay phải trước đó
        /// </summary>
        public Vector2 LastSkillMarkTargetPosition { get; private set; } = new Vector2(-1, -1);


        /// <summary>
        /// Cập nhật di chuyển của Leader bởi JoyStick
        /// </summary>
        private void UpdateJoyStickDirection()
        {
            Joystick moveJoystick = PlayZone.Instance.UIJoyStick;
            if (null == moveJoystick || null == Leader)
            {
                return;
            }

            /// Nếu đang trong trạng thái khinh công thì không thao tác
            if (this.Leader.CurrentAction == KE_NPC_DOING.do_jump)
            {
                return;
            }
            /// Nếu Leader đã chết
            else if (this.Leader.IsDeath || this.Leader.HP <= 0)
            {
                return;
            }
            /// Nếu đang bày bán
            else if (Global.Data.StallDataItem != null && Global.Data.StallDataItem.Start == 1 && !Global.Data.StallDataItem.IsBot)
            {
                if (moveJoystick.Direction != Vector2.zero)
                {
                    KTGlobal.AddNotification("Trong trạng thái bán hàng không thể di chuyển!");
                }
                return;
            }
            /// Nếu Leader đang bị khóa bởi kỹ năng
            else if (!this.Leader.CanPositiveMove)
            {
                if (moveJoystick.Direction != Vector2.zero)
                {
                    KTGlobal.AddNotification("Đang trong trạng thái bị khống chế, không thể di chuyển!");
                }
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
            /// Nếu đang Blink thì thôi
            else if (this.Leader.FlyingCoroutine != null)
            {
                return;
            }
            /// Nếu vừa bị GS bắt đổi vị trí
            else if (KTGlobal.GetCurrentTimeMilis() - KTLeaderMovingManager.LastForceChangePosTick < 500)
            {
                return;
            }

            /// Nhả chuột ra hoặc giá trị JoyStick dịch về 0
            if (this.LastJoyPosition != Vector2.zero && moveJoystick.Direction == Vector2.zero)
            {
                //KTDebug.LogError("Release mouse");
                /// Hủy trạng thái tự tìm đường
                KTLeaderMovingManager.StopMove();
                KTLeaderMovingManager.StopChasingTarget();
            }

            /// Vị trí lần trước của JoyStick
            this.LastJoyPosition = moveJoystick.Direction;

            /// Nếu JoyStick không có giá trị
            if (moveJoystick.Direction == Vector2.zero)
            {
                //KTDebug.LogError("Joystick back to ZERO");
                return;
            }

            /// Xóa bỏ dòng chữ tự tìm đường trên đầu
            PlayZone.Instance.HideTextAutoFindPath();

            /// Ngừng tự tìm đường
            AutoPathManager.Instance.StopAutoPath();
            /// Ngừng tự làm nhiệm vụ
            AutoQuest.Instance.StopAutoQuest();
            /// Ngừng tự đốt lửa trại
            KTAutoFightManager.Instance.StopAutoFireCamp();

            //Ngừng chạy về thành bán đồ
            KTAutoFightManager.Instance.StopAutoSell();
            KTAutoFightManager.Instance.StopAutoBuyItem();

            /// Xóa ký hiệu Click-Move
            this.RemoveClickMovePos();
            /// Xóa ký hiệu vị trí ra chiêu tay phải
            this.RemoveSkillMarkTargetPos();

            /// Bỏ đánh dấu mục tiêu tạm thời
            SkillManager.LastWaitingTarget = null;

            /// Nếu thời gian Tick JoyStick trước dưới thời gian Tick JoyStick
            if (KTGlobal.GetCurrentTimeMilis() - this.LastJoyStickTick < this.JoyStickCheckDirectionChangePeriodMilis)
            {
                return;
            }

            /// Hủy hàm Callback
            KTLeaderMovingManager.Done = null;

            //KTDebug.LogError("Joystick has param");

            /// Hướng chạy hiện tại
            float angle = KTMath.GetAngle360WithXAxis(moveJoystick.Direction);

            /// Nếu khóa di chuyển bằng JoyStick
            if (Global.Data.LockJoyStickMove)
            {
                VLTK.Entities.Enum.Direction dir8 = KTMath.GetDirectionByAngle360(angle);
                /// Cập nhật hướng quay hiện tại
                if (Global.Data.Leader.Direction != dir8)
                {
                    Global.Data.Leader.Direction = dir8;
                }

                /// Cập nhật thời gian Tick JoyStick lần cuối
                this.LastJoyStickTick = KTGlobal.GetCurrentTimeMilis();

                /// Xóa hướng di chuyển của JoyStick
                this.LastJoyStickDir = VLTK.Entities.Enum.Direction16.None;

                /// Thoát hàm
                return;
            }

            VLTK.Entities.Enum.Direction16 dir16 = KTMath.GetDirection16ByAngle360(angle);

            /// Nếu trùng với hướng di chuyển cũ
            if (dir16 == this.LastJoyStickDir)
            {
                /// Nếu thời gian Tick JoyStick trước dưới thời gian Tick JoyStick
                if (KTGlobal.GetCurrentTimeMilis() - this.LastJoyStickTick < this.JoyStickChangePosEveryMilis)
                {
                    return;
                }
            }

            /// Cập nhật hướng hiện tại của JoyStick
            this.LastJoyStickDir = dir16;

            /// Cập nhật thời gian Tick JoyStick lần cuối
            this.LastJoyStickTick = KTGlobal.GetCurrentTimeMilis();

            /// Vector chỉ hướng di chuyển
            Vector2 dirVector = KTMath.Dir16ToDirVector(dir16);

            /// Thực hiện di chuyển bằng JoyStick
            KTLeaderMovingManager.MoveByDirVector(dirVector, KTGlobal.MoveSpeedToPixel(Leader.MoveSpeed) * 1.5f, () =>
            {
                /// Thiết lập lại vị trí của Auto
               // KTAutoFightManager.Instance.StartPos = Global.Data.Leader.PositionInVector2;
            });
        }
        #endregion

        #region Sự kiện hệ thống

        /// <summary>
        /// Cập nhật các sự kiện ở Frame
        /// </summary>
        private void UpdateSelectedObject()
        {
            Vector3 position = Vector3.zero;

            /// Nếu đang chạy trên Editor hoặc bản Build Windows
            if (Application.isEditor || Application.platform == RuntimePlatform.WindowsPlayer)
            {
                if (Input.GetMouseButtonDown(0) && !FS.VLTK.Utils.IsClickOnGUI())
                {
                    /// Ngừng tự tìm đường
                    AutoPathManager.Instance.StopAutoPath();
                    /// Ngừng tự làm nhiệm vụ
                    AutoQuest.Instance.StopAutoQuest();

                    position = Input.mousePosition;
                    this.RunLater(() =>
                    {
                        this.HandleClickMove(position);
                    }, 0, 0.1f);
                }
            }
            /// Nếu đang chạy trên bản Build thiết bị di động
            else
            {
                if (Input.touchCount > 0)
                {
                    if (Input.GetTouch(0).phase == TouchPhase.Began && !FS.VLTK.Utils.IsClickOnGUI())
                    {
                        /// Ngừng tự tìm đường
                        AutoPathManager.Instance.StopAutoPath();
                        /// Ngừng tự làm nhiệm vụ
                        AutoQuest.Instance.StopAutoQuest();

                        position = Input.GetTouch(0).position;
                        this.RunLater(() =>
                        {
                            this.HandleClickMove(position);
                        }, 0, 0.1f);
                    }
                }
            }
        }

        /// <summary>
        /// Sự kiện khi chuột được ấn
        /// </summary>
        /// <param name="position"></param>
        private void HandleClickMove(Vector3 position)
        {
            /// Nếu chưa tải map xong
            if (!this.MapLoadingCompleted)
            {
                return;
            }
            /// Nếu không có nhân vật
            else if (this.Leader == null)
            {
                return;
            }

            /// Nếu đang trong trạng thái khinh công thì không thao tác
            if (this.Leader.CurrentAction == KE_NPC_DOING.do_jump)
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
                KTGlobal.AddNotification("Trong trạng thái bán hàng không thể di chuyển!");
                return;
            }
            /// Nếu đang bị khóa bởi kỹ năng
            else if (!this.Leader.CanPositiveMove)
            {
                KTGlobal.AddNotification("Đang trong trạng thái bị khống chế, không thể di chuyển!");
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
            /// Nếu đang Blink thì thôi
            else if (this.Leader.FlyingCoroutine != null)
            {
                return;
            }
            /// Nếu vừa bị GS bắt đổi vị trí
            else if (KTGlobal.GetCurrentTimeMilis() - KTLeaderMovingManager.LastForceChangePosTick < 500)
            {
                return;
            }

            /// Ngừng đuổi mục tiêu
            KTLeaderMovingManager.StopChasingTarget();

            /// Xóa bỏ dòng chữ tự tìm đường trên đầu
            PlayZone.Instance.HideTextAutoFindPath();

            /// Ngừng tự tìm đường
            AutoPathManager.Instance.StopAutoPath();
            /// Ngừng tự làm nhiệm vụ
            AutoQuest.Instance.StopAutoQuest();
            /// Ngừng tự đốt lửa trại
            KTAutoFightManager.Instance.StopAutoFireCamp();

            /// Ngừng chạy về thành bán đồ
            KTAutoFightManager.Instance.StopAutoSell();

            KTAutoFightManager.Instance.StopAutoBuyItem();

            Vector3 v3 = Global.MainCamera.ScreenToWorldPoint(position);

            //KTDebug.LogError("Click on target -> " + KTGlobal.IsClickOnTarget);
            if (!KTGlobal.IsClickOnTarget)
            {
                //KTDebug.LogError("Jump inside Click on target");
                PlayZone.Instance.HideAllFace();

                /// Bỏ đánh dấu mục tiêu tạm thời
                SkillManager.LastWaitingTarget = null;

                /// Hủy hàm Callback
                KTLeaderMovingManager.Done = null;

                /// Nếu đã đến thời điểm kiểm tra
                if (KTGlobal.GetCurrentTimeMilis() - this.LastClickTick >= this.ClickCheckPeriodMilis)
                {
                    /// Cập nhật vị trí dịch bằng Click Move
                    KTLeaderMovingManager.LeaderPositiveMoveToPos = v3;

                    /// Thực hiện tìm đường đến vị trí tương ứng
                    KTLeaderMovingManager.AutoFindRoad(new Point((int) v3.x, (int) v3.y), () =>
                    {
                        /// Thiết lập vị trí đích đến của Auto
                      //  KTAutoFightManager.Instance.StartPos = Global.Data.Leader.PositionInVector2;
                    });

                    /// Thiết lập vị trí ký hiệu Click-Move
                    this.SetClickMovePos(v3);

                    /// Cập nhật thời gian Tick lần trước
                    this.LastClickTick = KTGlobal.GetCurrentTimeMilis();
                }

                SkillManager.SelectedTarget = null;
                this.RemoveSelectTarget();
                /// Xóa ký hiệu vị trí ra chiêu tay phải
                this.RemoveSkillMarkTargetPos();
            }
        }
        #endregion

        #region Các sự kiện điều khiển đối tượng từ ngoài
        /// <summary>
        /// Thiết lập mục tiêu đang được chọn
        /// </summary>
        /// <param name="target"></param>
        public void SetSelectTarget(GSprite target)
        {
            if (this.SelectTargetDeco != null)
            {
                this.SelectTargetDeco.TargetSprite = target;
            }
        }

        /// <summary>
        /// Đối tượng hiện tại có đang được chọn không
        /// </summary>
        /// <param name="sprite"></param>
        /// <returns></returns>
        public bool IsSpriteSelected(GSprite sprite)
        {
            return this.SelectTargetDeco != null && this.SelectTargetDeco.TargetSprite == sprite;
        }

        /// <summary>
        /// Xóa mục tiêu đang được chọn
        /// </summary>
        public void RemoveSelectTarget()
        {
            if (this.SelectTargetDeco != null)
            {
                this.SelectTargetDeco.TargetSprite = null;
                this.SelectTargetDeco.transform.localPosition = new Vector2(-99999, -99999);
            }
        }

        /// <summary>
        /// Trả về vị trí ClickMove hiện tại
        /// </summary>
        /// <returns></returns>
        public Vector2 GetClickMovePos()
        {
            if (this.ClickMoveDeco == null)
            {
                return Vector2.zero;
            }
            return this.ClickMoveDeco.transform.localPosition;
        }

        /// <summary>
        /// Thiết lập vị trí Click-Move
        /// </summary>
        /// <param name="position"></param>
        public void SetClickMovePos(Vector2 position)
        {
            if (this.ClickMoveDeco != null)
            {
                this.ClickMoveDeco.transform.localPosition = new Vector3(position.x, position.y, position.y / 10000);

                Point point = new Point((int) position.x / Global.CurrentMapData.GridSizeX, (int) position.y / Global.CurrentMapData.GridSizeY);

                /// Nếu nằm trong ô bị làm mờ
                if (Global.CurrentMapData.BlurPositions[point.X, point.Y] == 1)
                {
                    this.ClickMoveDeco.Alpha = 0.5f;
                }
                else
                {
                    this.ClickMoveDeco.Alpha = 1f;
                }
            }
        }

        /// <summary>
        /// Xóa ký hiệu Click-Move
        /// </summary>
        public void RemoveClickMovePos()
        {
            if (this.ClickMoveDeco != null)
            {
                this.ClickMoveDeco.transform.localPosition = new Vector2(-99999, -99999);
                this.ClickMoveDeco.Alpha = 1f;
            }
        }

        /// <summary>
        /// Thiết lập đánh dấu vị trí ra chiêu
        /// </summary>
        /// <param name="position"></param>
        public void SetSkillMarkTargetPos(Vector2 position)
        {
            this.SkillMarkTargetPosDeco.transform.position = new Vector3(position.x, position.y, position.y / 10000);
            /// Đánh dấu tọa độ dùng kỹ năng tay phải
            this.LastSkillMarkTargetPosition = this.SkillMarkTargetPosDeco.transform.localPosition;
        }

        /// <summary>
        /// Xóa ký hiệu
        /// </summary>
        public void RemoveSkillMarkTargetPos()
        {
            if (this.SkillMarkTargetPosDeco != null)
            {
                this.SkillMarkTargetPosDeco.transform.localPosition = new Vector2(-99999, -99999);
                /// Hủy tọa độ dùng kỹ năng tay phải
                this.LastSkillMarkTargetPosition = new Vector2(-1, -1);
            }
        }

        /// <summary>
        /// Thiết lập đánh dấu vị trí Farm
        /// </summary>
        /// <param name="position"></param>
        /// <param name="radius"></param>
        public void SetFarmAreaDeco(Vector2 position, float radius)
        {
            if (this.AutoFarmAreaDeco != null)
            {
                this.AutoFarmAreaDeco.Position = position;
                this.AutoFarmAreaDeco.Radius = radius;
                this.AutoFarmAreaDeco.Show();
            }
        }

        /// <summary>
        /// Xóa đánh dấu vị trí Farm
        /// </summary>
        public void RemoveFarmAreaDeco()
        {
            if (this.AutoFarmAreaDeco != null)
            {
                this.AutoFarmAreaDeco.Hide();
            }
        }
        #endregion

        #region Update
        /// <summary>
        /// Luồng thực hiện chạy sau bỏ qua một số khoảng Frame
        /// </summary>
        /// <param name="action"></param>
        /// <param name="totalSkipFrames"></param>
        private void RunLater(Action action, int totalSkipFrames = 1, float skipForSeconds = 0f)
        {
            IEnumerator Do()
            {
                for (int i = 1; i <= totalSkipFrames; i++)
                {
                    yield return null;
                }
                if (skipForSeconds > 0)
                {
                    yield return new WaitForSeconds(skipForSeconds);
                }
                action?.Invoke();
            }
            PlayZone.Instance.StartCoroutine(Do());
        }

        private bool isMarkClick = false;

        /// <summary>
        /// Xử lý mỗi Frame
        /// </summary>
        private void OnFrameEvents()
        {
            this.UpdateJoyStickDirection();
            this.UpdateSelectedObject();

            if (KTGlobal.IsClickOnTarget && !this.isMarkClick)
            {
                this.isMarkClick = true;
                this.RunLater(() => {
                    //KTDebug.Log("REMOVE");
                    this.isMarkClick = false;
                    KTGlobal.IsClickOnTarget = false;
                }, 0, 0.1f);
            }
        }
        #endregion
    }
}