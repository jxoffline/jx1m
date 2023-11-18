using FS.GameEngine.Interface;
using FS.GameEngine.Logic;
using FS.GameEngine.Sprite;
using FS.VLTK.Factory;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FS.VLTK.Logic.BackgroundWork
{
    /// <summary>
    /// Thực thi thiết lập hệ thống
    /// </summary>
    public partial class KTBackgroundWorkManager
    {
        /// <summary>
        /// Luồng thực thi thiết lập hệ thống lên các đối tượng động
        /// </summary>
        private Coroutine executeSettingOnGameObjectsCoroutine = null;

        /// <summary>
        /// Thực thi thiết lập hệ thống lên toàn thể các đối tượng động
        /// </summary>
        /// <returns></returns>
        private IEnumerator DoExecuteSystemSettingOnGameObjects()
        {
            /// Danh sách người chơi khác
            List<IObject> otherRoles = KTObjectsManager.Instance.GetObjects(GSpriteTypes.Other);
            /// Danh sách quái
            List<IObject> monsters = KTObjectsManager.Instance.GetObjects(GSpriteTypes.Monster);
            /// Danh sách NPC
            List<IObject> npcs = KTObjectsManager.Instance.GetObjects(GSpriteTypes.NPC);
            /// Danh sách BOT
            List<IObject> bots = KTObjectsManager.Instance.GetObjects(GSpriteTypes.Bot);

            /// Tổng số đối tượng đã xử lý ở Frame hiện tại
            int totalCheckedObjectsThisFrame = 0;

            /// Xử lý Leader
            if (Global.Data != null && Global.Data.GameScene != null && Global.Data.Leader != null)
            {
                Global.Data.Leader.ComponentCharacter.ExecuteSetting();
                /// Tăng tổng số đối tượng đã xử lý ở Frame hiện tại lên
                totalCheckedObjectsThisFrame++;
                /// Nếu đã vượt ngưỡng xử lý ở Frame này thì bỏ qua
                if (totalCheckedObjectsThisFrame >= KTBackgroundWorkManager.ExecuteSystemSettingMaxObjectsPerFrame)
                {
                    /// Reset
                    totalCheckedObjectsThisFrame = 0;
                    /// Bỏ qua Frame
                    yield return null;
                }
            }

            /// Duyệt danh sách đối tượng
            foreach (IObject obj in otherRoles)
            {
                GSprite sprite = obj as GSprite;
                sprite.ComponentCharacter.ExecuteSetting();
                /// Tăng tổng số đối tượng đã xử lý ở Frame hiện tại lên
                totalCheckedObjectsThisFrame++;
                /// Nếu đã vượt ngưỡng xử lý ở Frame này thì bỏ qua
                if (totalCheckedObjectsThisFrame >= KTBackgroundWorkManager.ExecuteSystemSettingMaxObjectsPerFrame)
                {
                    /// Reset
                    totalCheckedObjectsThisFrame = 0;
                    /// Bỏ qua Frame
                    yield return null;
                }
            }

            /// Duyệt danh sách đối tượng
            foreach (IObject obj in bots)
            {
                GSprite sprite = obj as GSprite;
                sprite.ComponentCharacter.ExecuteSetting();
                /// Tăng tổng số đối tượng đã xử lý ở Frame hiện tại lên
                totalCheckedObjectsThisFrame++;
                /// Nếu đã vượt ngưỡng xử lý ở Frame này thì bỏ qua
                if (totalCheckedObjectsThisFrame >= KTBackgroundWorkManager.ExecuteSystemSettingMaxObjectsPerFrame)
                {
                    /// Reset
                    totalCheckedObjectsThisFrame = 0;
                    /// Bỏ qua Frame
                    yield return null;
                }
            }

            /// Duyệt danh sách đối tượng
            foreach (IObject obj in monsters)
            {
                GSprite sprite = obj as GSprite;
                sprite.ComponentMonster.ExecuteSetting();
                /// Tăng tổng số đối tượng đã xử lý ở Frame hiện tại lên
                totalCheckedObjectsThisFrame++;
                /// Nếu đã vượt ngưỡng xử lý ở Frame này thì bỏ qua
                if (totalCheckedObjectsThisFrame >= KTBackgroundWorkManager.ExecuteSystemSettingMaxObjectsPerFrame)
                {
                    /// Reset
                    totalCheckedObjectsThisFrame = 0;
                    /// Bỏ qua Frame
                    yield return null;
                }
            }

            /// Duyệt danh sách đối tượng
            foreach (IObject obj in npcs)
            {
                GSprite sprite = obj as GSprite;
                sprite.ComponentMonster.ExecuteSetting();
                /// Tăng tổng số đối tượng đã xử lý ở Frame hiện tại lên
                totalCheckedObjectsThisFrame++;
                /// Nếu đã vượt ngưỡng xử lý ở Frame này thì bỏ qua
                if (totalCheckedObjectsThisFrame >= KTBackgroundWorkManager.ExecuteSystemSettingMaxObjectsPerFrame)
                {
                    /// Reset
                    totalCheckedObjectsThisFrame = 0;
                    /// Bỏ qua Frame
                    yield return null;
                }
            }
        }

        /// <summary>
        /// Thực thi thiết lập hệ thống cho các đối tượng
        /// </summary>
        public void ExecuteSystemSettingOnGameObjects()
        {
            if (this.executeSettingOnGameObjectsCoroutine != null)
            {
                this.StopCoroutine(this.executeSettingOnGameObjectsCoroutine);
            }
            this.executeSettingOnGameObjectsCoroutine = this.StartCoroutine(this.DoExecuteSystemSettingOnGameObjects());
        }
    }
}
