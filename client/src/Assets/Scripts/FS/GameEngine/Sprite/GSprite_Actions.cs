using FS.VLTK.Utilities.Algorithms;
using static FS.VLTK.Entities.Enum;
using FS.VLTK;
using System;
using FS.GameEngine.Logic;

namespace FS.GameEngine.Sprite
{
    /// <summary>
    /// Quản lý động tác
    /// </summary>
    public partial class GSprite
    {
        /// <summary>
        /// Thông tin động tác đang chờ thực thi
        /// </summary>
        private class QueueActionInfo
        {
            /// <summary>
            /// Động tác
            /// </summary>
            public KE_NPC_DOING Action { get; set; }

            /// <summary>
            /// Thời gian thực thi
            /// </summary>
            public long DurationTick { get; set; }

            /// <summary>
            /// Thời gian bắt đầu
            /// </summary>
            public long StartTick { get; set; }

            /// <summary>
            /// Đã hoàn thành chưa
            /// </summary>
            public bool IsCompleted
            {
                get
                {
                    if (this.StartTick == -1)
                    {
                        return false;
                    }
                    else if (this.DurationTick == -1)
                    {
                        return false;
                    }
                    return KTGlobal.GetCurrentTimeMilis() - this.StartTick >= this.DurationTick;
                }
            }

            /// <summary>
            /// Sự kiện khi thực hiện động tác tương ứng
            /// </summary>
            public Action Start { get; set; }

            /// <summary>
            /// Sự kiện khi kết thúc động tác tương ứng
            /// </summary>
            public Action Done { get; set; }

            /// <summary>
            /// Dữ liệu đi kèm
            /// </summary>
            public int Param1 { get; set; }

            /// <summary>
            /// Đánh dấu có phải tự gọi đến không
            /// </summary>
            public bool IsAuto { get; set; }
        }

        /// <summary>
        /// Hàng đợi động tác chờ thực thi
        /// </summary>
        private readonly DoubleEndedQueue<QueueActionInfo> QueueActions = new DoubleEndedQueue<QueueActionInfo>();

        /// <summary>
        /// Đánh dấu tạm dừng toàn bộ động tác không
        /// </summary>
        private bool pauseCurrentAction = false;

        /// <summary>
        /// Thời điểm lần trước thực thi động tác tấn công
        /// </summary>
        private long lastProcessFightTick = 0;

        /// <summary>
        /// Thời điểm lần cuối thực thi động tác tấn công đến khi chuyển về trạng thái đánh đứng
        /// </summary>
        private const long StandIdleChangeToNormalStandTime = 10000;

        /// <summary>
        /// Động tác đang thực hiện
        /// </summary>
        private QueueActionInfo doingAction;

        /// <summary>
        /// Thêm động tác vào danh sách chờ
        /// </summary>
        /// <param name="action"></param>
        /// <param name="duration"></param>
        /// <param name="start"></param>
        /// <param name="done"></param>
        /// <param name="param1"></param>
        private void AddAction(KE_NPC_DOING action, float duration, Action start = null, Action done = null, int param1 = -1)
        {
            /// Nếu có động tác liền sau trùng với động tác chờ
            if (this.QueueActions.Count > 0)
            {
                /// Động tác ở cuối hàng đợi
                QueueActionInfo actionInfo = this.QueueActions.PeakRear();
                /// Nếu trùng với động tác cần thên vào
                if (actionInfo.Action == action)
                {
                    /// So sánh thời gian nếu nhỏ hơn thời gian cũ thì thay vào
                    if (duration == -1 || actionInfo.DurationTick < duration * 1000)
                    {
                        actionInfo.DurationTick = duration == -1 ? -1 : (long) (duration * 1000 * (param1 == -1 ? 1 : param1));
                        actionInfo.Start = start;
                        actionInfo.Done = done;
                        actionInfo.Param1 = param1;
                        actionInfo.IsAuto = false;
                    }
                    return;
                }
            }

            /// Thêm động tác vào cuối danh sách chờ
            this.QueueActions.EnqueueRear(new QueueActionInfo()
            {
                Action = action,
                DurationTick = duration == -1 ? -1 : (long) (duration * 1000 * (param1 == -1 ? 1 : param1)),
                StartTick = -1,
                Start = start,
                Done = done,
                Param1 = param1,
                IsAuto = false,
            });;
        }

        /// <summary>
        /// Thay thế động tác hiện tại thành động tác tương ứng
        /// </summary>
        /// <param name="action"></param>
        /// <param name="duration"></param>
        /// <param name="start"></param>
        /// <param name="done"></param>
        /// <param name="param1"></param>
        private void ReplaceCurrentActionWith(KE_NPC_DOING action, float duration, Action start = null, Action done = null, int param1 = -1)
        {
            this.doingAction = new QueueActionInfo()
            {
                Action = action,
                StartTick = KTGlobal.GetCurrentTimeMilis(),
                DurationTick = (long) (duration * 1000),
                Start = start,
                Done = done,
                Param1 = param1,
                IsAuto = false,
            };
            /// Cập nhật động tạc hiện tại
            this.CurrentAction = action;
        }

        /// <summary>
        /// Tạm ngưng động tác hiện tại
        /// </summary>
        private void PauseCurrentAction()
        {
            /// Nếu trong trạng thái tạm dừng động tác hiện tại thì bỏ qua
            if (this.pauseCurrentAction)
            {
                return;
            }
            /// Đánh dấu đang tạm dừng động tác hiện tại
            this.pauseCurrentAction = true;

            /// Nếu có động tác đang thực hiện dở
            if (this.doingAction  != null)
            {
                /// Cập nhật thời gian còn lại của động tác hiện tại
                this.doingAction.DurationTick = KTGlobal.GetCurrentTimeMilis() - this.doingAction.StartTick;
            }

            /// Khóa Render không cho thực thi động tác
            this.LockRender();
        }

        /// <summary>
        /// Tiếp tục động tác hiện tại
        /// </summary>
        private void ResumeCurrentAction()
        {
            /// Nếu không trong trạng thái tạm dừng động tác hiện tại thì bỏ qua
            if (!this.pauseCurrentAction)
            {
                return;
            }

            /// Đánh dấu không tạm dừng động tác hiện tại
            this.pauseCurrentAction = false;

            /// Nếu có động tác đang thực hiện dở
            if (this.doingAction != null)
            {
                /// Cập nhật thời gian bắt đầu lại động tác hiện tại
                this.doingAction.StartTick = KTGlobal.GetCurrentTimeMilis();
            }

            /// Mở khóa Render để thực thi động tác
            this.UnlockRender();
        }

        /// <summary>
        /// Khóa Render
        /// </summary>
        private void LockRender()
        {
            if (this.ComponentCharacter != null)
            {
                this.ComponentCharacter.PauseAllActions();
            }
            else if (this.ComponentMonster != null)
            {
                this.ComponentMonster.PauseAllActions();
            }
        }

        /// <summary>
        /// Mở khóa Render
        /// </summary>
        private void UnlockRender()
        {
            if (this.ComponentCharacter != null)
            {
                this.ComponentCharacter.ResumeActions();
            }
            else if (this.ComponentMonster != null)
            {
                this.ComponentMonster.ResumeActions();
            }
        }

        /// <summary>
        /// Làm rỗng danh sách động tác chờ
        /// </summary>
        private void ClearActions()
        {
            this.QueueActions.Clear();
            this.doingAction = null;
        }

        /// <summary>
        /// Có đang thực hiện động tác mặc định không
        /// </summary>
        /// <returns></returns>
        private bool IsDoingDefaultAction()
        {
            /// Nếu không có động tác
            if (this.doingAction == null)
            {
                return false;
            }
            /// Nếu không phải động tác đứng
            else if (this.doingAction.Action != KE_NPC_DOING.do_stand)
            {
                return false;
            }
            /// Nếu là tự động
            else if (this.doingAction.IsAuto)
            {
                return true;
            }
            /// Đúng nếu thời gian là -1
            return this.doingAction.DurationTick == -1;
        }

        /// <summary>
        /// Động tác hiện tại đang thực hiện đã hoàn thành chưa
        /// </summary>
        /// <returns></returns>
        public bool IsCurrentActionCompleted()
        {
            /// Nếu không có động tác
            if (this.doingAction == null)
            {
                return true;
            }
            /// Nếu đang thực thi động tác mặc định
            else if (this.IsDoingDefaultAction())
            {
                return true;
            }
            /// Trả về giá trị nếu động tác hiện tại đã hoàn thành
            return this.doingAction.IsCompleted;
        }

        /// <summary>
        /// Thực thi động tác chờ trong danh sách
        /// </summary>
        private void ProcessAction()
        {
            /// Nếu đang tạm dừng toàn bộ động tác
            if (this.pauseCurrentAction)
            {
                return;
            }

            /// Nếu động tác trước chưa hoàn thành thì bỏ qua
            if (!this.IsCurrentActionCompleted())
            {
                return;
            }

            /// Nếu là động tác đứng mặc định và hàng đợi rỗng
            if (this.QueueActions.Count <= 0 && this.IsDoingDefaultAction())
            {
                /// Nếu đang thực hiện động tác đứng tấn công và đã hết thời gian thực hiện
                if (this.doingAction.Param1 == 1 && KTGlobal.GetCurrentTimeMilis() - this.lastProcessFightTick >= GSprite.StandIdleChangeToNormalStandTime)
                {
                    /// Thực hiện động tác đứng thường
                    this.doingAction = new QueueActionInfo()
                    {
                        Action = KE_NPC_DOING.do_stand,
                        DurationTick = -1,
                        Done = null,
                        Start = null,
                        IsAuto = true,
                        Param1 = 0,
                    };
                }
                else
                {
                    return;
                }
            }
            else
            {
                /// Thực thi sự kiện hoàn thành động tác trước đó
                if (this.doingAction != null)
                {
                    this.doingAction.Done?.Invoke();
                }

                /// Lấy động tác tiếp theo ở đầu hàng đợi
                this.doingAction = this.QueueActions.DequeueFront();

                /// Nếu không tồn tại tức hàng đợi rỗng thì tự động thực thi động tác đứng mặc định
                if (this.doingAction == null)
                {
                    /// Nếu là động tác ngồi thì bỏ qua
                    if (this.CurrentAction == KE_NPC_DOING.do_sit)
                    {
                        return;
                    }
                    /// Nếu đối tượng đã chết thì bỏ qua
                    if (this.IsDeath)
                    {
                        return;
                    }

                    /// Nếu chưa đến thời gian thực thi động tác đứng thường
                    if (KTGlobal.GetCurrentTimeMilis() - this.lastProcessFightTick <= GSprite.StandIdleChangeToNormalStandTime)
                    {
                        /// Thực thi động tác đứng tấn công
                        this.doingAction = new QueueActionInfo()
                        {
                            Action = KE_NPC_DOING.do_stand,
                            DurationTick = GSprite.StandIdleChangeToNormalStandTime - KTGlobal.GetCurrentTimeMilis() + this.lastProcessFightTick,
                            Done = null,
                            Start = null,
                            IsAuto = true,
                            Param1 = 1,
                        };
                    }
                    else
                    {
                        /// Thực hiện động tác đứng thường
                        this.doingAction = new QueueActionInfo()
                        {
                            Action = KE_NPC_DOING.do_stand,
                            DurationTick = -1,
                            Done = null,
                            Start = null,
                            IsAuto = true,
                            Param1 = 0,
                        };
                    }
                }
            }
            

            /// Cập nhật động tạc hiện tại
            this.CurrentAction = this.doingAction.Action;
            /// Cập nhật thời gian bắt đầu thực hiện động tác tương ứng
            this.doingAction.StartTick = KTGlobal.GetCurrentTimeMilis();
            /// Thực thi động tác tương ứng
            this.ProcessAction(this.doingAction.Action, this.doingAction.DurationTick / 1000f, this.doingAction.Param1);
            /// Thực thi sự kiện bắt đầu động tác
            this.doingAction.Start?.Invoke();
        }

        #region Thực thi động tác
        /// <summary>
        /// Thực thi động tác
        /// </summary>
        /// <param name="action"></param>
        /// <param name="duration"></param>
        /// <param name="param1"></param>
        private void ProcessAction(KE_NPC_DOING action, float duration, int param1)
        {
            /// Fix Bug chạy trên trời
            if (this.ComponentCharacter != null)
			{
                this.ComponentCharacter.Model.transform.localPosition = this.ComponentCharacter.OriginModelPos;
            }

            switch (action)
            {
                case KE_NPC_DOING.do_attack:
                    this.lastProcessFightTick = KTGlobal.GetCurrentTimeMilis();
                    this.ProcessAttack(duration);
                    break;
                case KE_NPC_DOING.do_rushattack:
                    this.lastProcessFightTick = KTGlobal.GetCurrentTimeMilis();
                    this.ProcessSpecialAttack(duration);
                    break;
                case KE_NPC_DOING.do_hurt:
                    this.ProcessHurt(duration);
                    break;
                case KE_NPC_DOING.do_magic:
                    this.lastProcessFightTick = KTGlobal.GetCurrentTimeMilis();
                    this.ProcessMagicAction(duration);
                    break;
                case KE_NPC_DOING.do_death:
                    this.ProcessDie();
                    break;
                case KE_NPC_DOING.do_run:
                    this.ProcessRun();
                    break;
                case KE_NPC_DOING.do_walk:
                    this.ProcessWalk();
                    break;
                case KE_NPC_DOING.do_sit:
                    this.ProcessSit(duration);
                    break;
                case KE_NPC_DOING.do_stand:
                    if (duration > 0)
                    {
                        this.ProcessFightStand();
                    }
                    else
                    {
                        this.ProcessStand();
                    }
                    break;
                case KE_NPC_DOING.do_jump:
                    this.ProcessJump(duration);
                    break;
                case KE_NPC_DOING.do_runattack:
                    this.lastProcessFightTick = KTGlobal.GetCurrentTimeMilis();
                    this.ProcessRunAttack(duration);
                    break;
                case KE_NPC_DOING.do_manyattack:
                    this.lastProcessFightTick = KTGlobal.GetCurrentTimeMilis();
                    this.ProcessManyAttack(duration / param1, param1);
                    break;
                case KE_NPC_DOING.do_runattackmany:
                    this.lastProcessFightTick = KTGlobal.GetCurrentTimeMilis();
                    this.ProcessSpecialManyAttack(duration / param1, param1);
                    break;

            }
        }

        /// <summary>
        /// Thực hiện động tác tấn công
        /// </summary>
        /// <param name="duration"></param>
        private void ProcessAttack(float duration)
        {
            if (this.Role2D == null)
            {
                return;
            }

            /// Nếu là người chơi
            if (this.ComponentCharacter != null)
            {
                this.ComponentCharacter.Attack(duration);
            }
            /// Nếu là quái
            else if (this.ComponentMonster != null)
            {
                this.ComponentMonster.Attack(duration);
            }
        }

        /// <summary>
        /// Thực hiện động tác tấn công
        /// </summary>
        /// <param name="duration"></param>
        private void ProcessSpecialAttack(float duration)
        {
            if (this.Role2D == null)
            {
                return;
            }

            /// Nếu là người chơi
            if (this.ComponentCharacter != null)
            {
                this.ComponentCharacter.SpecialAttack(duration);
            }
            /// Nếu là quái
            else if (this.ComponentMonster != null)
            {
                this.ComponentMonster.SpecialAttack(duration);
            }
        }

        /// <summary>
        /// Thực hiện động tác xuất chiêu nội công
        /// </summary>
        /// <param name="duration"></param>
        private void ProcessMagicAction(float duration)
        {
            if (this.Role2D == null)
            {
                return;
            }

            /// Nếu là người chơi
            if (this.ComponentCharacter != null)
            {
                this.ComponentCharacter.PlayMagicAction(duration);
            }
            /// Nếu là quái
            else if (this.ComponentMonster != null)
            {
                this.ComponentMonster.Attack(duration);
            }
        }

        /// <summary>
        /// Thực hiện động tác nhận sát thương
        /// </summary>
        /// <param name="duration"></param>
        private void ProcessHurt(float duration)
        {
            if (this.Role2D == null)
            {
                return;
            }

            /// Nếu là người chơi
            if (this.ComponentCharacter != null)
            {
                this.ComponentCharacter.Hurt(duration);
            }
            /// Nếu là quái
            else if (this.ComponentMonster != null)
            {
                this.ComponentMonster.Hurt(duration);
            }
        }

        /// <summary>
        /// Thực hiện động tác chết
        /// </summary>
        private void ProcessDie()
        {
            if (this.Role2D == null)
            {
                return;
            }

            /// Nếu là người chơi
            if (this.ComponentCharacter != null)
            {
                this.ComponentCharacter.Die();
            }
            /// Nếu là quái
            else if (this.ComponentMonster != null)
            {
                this.ComponentMonster.Die();
            }
        }

        /// <summary>
        /// Thực hiện động tác chạy lao nhanh về phía mục tiêu
        /// </summary>
        /// <param name="duration"></param>
        private void ProcessRunAttack(float duration)
        {
            if (this.Role2D == null)
            {
                return;
            }

            /// Nếu là người chơi
            if (this.ComponentCharacter != null)
            {
                this.ComponentCharacter.RunAttack(duration);
            }
            /// Nếu là quái
            else if (this.ComponentMonster != null)
            {
                this.ComponentMonster.RunAttack(duration);
            }
        }

        /// <summary>
        /// Thực hiện tấn công nhiều lần
        /// </summary>
        /// <param name="duration"></param>
        private void ProcessManyAttack(float duration, int count)
        {
            if (this.Role2D == null)
            {
                return;
            }

            /// Nếu là người chơi
            if (this.ComponentCharacter != null)
            {
                this.ComponentCharacter.AttackMultipleTimes(duration, count);
            }
            /// Nếu là quái
            else if (this.ComponentMonster != null)
            {
                this.ComponentMonster.AttackMultipleTimes(duration, count);
            }
        }

        /// <summary>
        /// Thực hiện tấn công nhiều lần
        /// </summary>
        /// <param name="duration"></param>
        private void ProcessSpecialManyAttack(float duration, int count)
        {
            if (this.Role2D == null)
            {
                return;
            }

            /// Nếu là người chơi
            if (this.ComponentCharacter != null)
            {
                this.ComponentCharacter.SpecialAttackMultipleTimes(duration, count);
            }
            /// Nếu là quái
            else if (this.ComponentMonster != null)
            {
                this.ComponentMonster.SpecialAttackMultipleTimes(duration, count);
            }
        }

        /// <summary>
        /// Thực hiện động tác chạy
        /// </summary>
        private void ProcessRun()
        {
            if (this.Role2D == null)
            {
                return;
            }

            /// Nếu là người chơi
            if (this.ComponentCharacter != null)
            {
                this.ComponentCharacter.Run();
            }
            /// Nếu là quái
            else if (this.ComponentMonster != null)
            {
                this.ComponentMonster.Run();
            }
        }

        /// <summary>
        /// Thực hiện động tác đi bộ
        /// </summary>
        private void ProcessWalk()
        {
            if (this.Role2D == null)
            {
                return;
            }

            /// Nếu là người chơi
            if (this.ComponentCharacter != null)
            {
                this.ComponentCharacter.Walk();
            }
            /// Nếu là quái
            else if (this.ComponentMonster != null)
            {
                this.ComponentMonster.Run();
            }
        }

        /// <summary>
        /// Thực hiện động tác đứng
        /// </summary>
        private void ProcessStand()
        {
            if (this.Role2D == null)
            {
                return;
            }

            /// Nếu là người chơi
            if (this.ComponentCharacter != null)
            {
                this.ComponentCharacter.Stand();
            }
            /// Nếu là quái
            else if (this.ComponentMonster != null)
            {
                this.ComponentMonster.Stand();
            }
        }

        /// <summary>
        /// Thực hiện động tác đứng
        /// </summary>
        private void ProcessFightStand()
        {
            if (this.Role2D == null)
            {
                return;
            }

            /// Nếu là người chơi
            if (this.ComponentCharacter != null)
            {
                /// Nếu đang cưỡi
                if (this.RoleData.IsRiding)
                {
                    /// Thực hiện động tác đứng thường
                    this.ComponentCharacter.Stand();
                }
                else
                {
                    this.ComponentCharacter.FightStand();
                }
            }
            /// Nếu là quái
            else if (this.ComponentMonster != null)
            {
                this.ComponentMonster.FightStand();
            }
        }

        /// <summary>
        /// Thực hiện động tác khinh công
        /// </summary>
        /// <param name="duration"></param>
        private void ProcessJump(float duration)
        {
            if (this.Role2D == null)
            {
                return;
            }

            /// Nếu là người chơi
            if (this.ComponentCharacter != null)
            {
                this.ComponentCharacter.Jump(duration);
            }
        }

        /// <summary>
        /// Thực hiện động tác ngồi
        /// </summary>
        /// <param name="duration"></param>
        private void ProcessSit(float duration)
        {
            if (this.Role2D == null)
            {
                return;
            }

            /// Nếu là người chơi
            if (this.ComponentCharacter != null)
            {
                this.ComponentCharacter.Sit(duration);
            }
        }
        #endregion
    }
}

