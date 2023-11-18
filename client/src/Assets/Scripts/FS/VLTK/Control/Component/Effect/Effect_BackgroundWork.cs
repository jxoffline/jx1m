using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Collections;
using static FS.VLTK.Entities.Enum;
using FS.VLTK.Logic.Settings;

namespace FS.VLTK.Control.Component
{
    /// <summary>
    /// Quản lý luồng thực thi ngầm
    /// </summary>
    public partial class Effect
    {
        /// <summary>
        /// Kiểm tra vị trí và thực hiện đổi màu nếu cần
        /// </summary>
        private void SynsLocation()
        {
            if (this.Owner != null)
            {
                this.groupColor.Alpha = this.ownerPlayer != null ? this.ownerPlayer.CurrentAlpha : this.ownerMonster != null ? this.ownerMonster.CurrentAlpha : 1f;
            }
        }

        /// <summary>
        /// Thực thi thiết lập hệ thống
        /// </summary>
        private void ExecuteSetting()
        {
            ///// Nếu FPS quá thấp thì ẩn hiệu ứng
            //if (MainGame.Instance.GetRenderQuality() == MainGame.RenderQuality.Low)
            //{
            //    if (!this.lastHideRole)
            //    {
            //        this.PauseAllActions();
            //        this.SetModelVisible(false);
            //        this.lastHideRole = true;
            //    }
            //}
            ///// Nếu FPS thỏa mãn thì thực thi theo thiết lập hệ thống
            //else
            {
                if ((this.Type == EffectType.Buff && this.lastHideRole != KTSystemSetting.HideSkillBuffEffect) || (this.Type == EffectType.CastEffect && this.lastHideRole != KTSystemSetting.HideSkillCastEffect))
                {
                    this.lastHideRole = this.Type == EffectType.Buff ? KTSystemSetting.HideSkillBuffEffect : KTSystemSetting.HideSkillCastEffect;
                    if (!this.lastHideRole)
                    {
                        this.ResumeActions();
                        this.SetModelVisible(true);

                        this.RefreshAction();
                        this.ResumeActions();
                        this.Play();
                    }
                    else
                    {
                        this.PauseAllActions();
                        this.SetModelVisible(false);
                    }
                }
            }
        }

        /// <summary>
        /// Thực thi sự kiện ngầm
        /// </summary>
        /// <returns></returns>
        private IEnumerator ExecuteBackgroundWork()
        {
            while (true)
            {
                yield return new WaitForSeconds(0.2f);
                this.SynsLocation();
                this.SortingOrderHandler();
                this.ExecuteSetting();
            }
        }

        /// <summary>
        /// Thực thi sự kiện bỏ qua một số Frame
        /// </summary>
        /// <param name="skip"></param>
        /// <param name="work"></param>
        /// <returns></returns>
        private IEnumerator ExecuteSkipFrames(int skip, Action work)
        {
            for (int i = 1; i <= skip; i++)
            {
                yield return null;
            }
            work?.Invoke();
        }

        /// <summary>
        /// Thực thi sự kiện sau khoảng thời gian tương ứng
        /// </summary>
        /// <param name="sec"></param>
        /// <param name="work"></param>
        /// <returns></returns>
        private IEnumerator DelayTask(float sec, Action work)
        {
            yield return new WaitForSeconds(sec);
            work?.Invoke();
        }
    }
}
