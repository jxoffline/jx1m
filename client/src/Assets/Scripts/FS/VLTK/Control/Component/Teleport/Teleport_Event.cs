using FS.GameEngine.Logic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace FS.VLTK.Control.Component
{
    /// <summary>
    /// Đối tượng điểm truyền tống
    /// </summary>
    public partial class Teleport : IEvent
    {
        /// <summary>
        /// Khởi tạo sự kiện
        /// </summary>
        private void InitEvents()
        {
            this.StartCoroutine(this.CheckLeaderPosition());
        }

        /// <summary>
        /// Luồng thực thi kiểm tra vị trí Leader có nằm trong vùng dịch chuyển của Teleport không
        /// </summary>
        /// <returns></returns>
        private IEnumerator CheckLeaderPosition()
        {
            bool isPreNotified = false;
            while (true)
            {
                yield return new WaitForSeconds(2f);

                if (Global.Data != null && Global.Data.GameScene != null && Global.Data.Leader != null)
                {
                    Vector2 leaderPos = new Vector2(Global.Data.Leader.PosX, Global.Data.Leader.PosY);
                    Vector2 teleportPos = this.transform.localPosition;
                    float distance = Vector2.Distance(leaderPos, teleportPos);
                    //KTDebug.LogError("Distance = " + distance);
                    if (distance <= this.Data.Radius)
                    {
                        //if (Global.Data.RoleData.Level < this.Data.ToMapLevel)
                        //{
                        //    if (!isPreNotified)
                        //    {
                        //        KTGlobal.AddNotification(string.Format("Bản đồ này yêu cầu nhân vật đạt cấp {0} trở lên.", this.Data.ToMapLevel));
                        //        isPreNotified = true;
                        //    }
                        //}
                        //else
                        {
                            Global.Data.GameScene.ToMapConversionByTeleportCode(this.Data.ID);
                        }
                    }
                    else
                    {
                        isPreNotified = false;
                    }
                }
            }
        }

        /// <summary>
        /// Sự kiện khi đối tượng được Click
        /// </summary>
        public void OnClick()
        {
            
        }

        /// <summary>
        /// Sự kiện khi vị trí của đối tượng thay đổi
        /// </summary>
        public void OnPositionChanged()
        {
            if (this.UIMinimapReference != null)
            {
                this.UIMinimapReference.UpdatePosition();
            }
        }
    }
}
