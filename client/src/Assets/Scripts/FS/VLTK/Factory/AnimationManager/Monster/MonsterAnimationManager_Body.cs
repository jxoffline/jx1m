using FS.VLTK.Entities.Config;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using static FS.VLTK.Entities.Enum;

namespace FS.VLTK.Factory.Animation
{
    /// <summary>
    /// Quản lý thân quái
    /// </summary>
    public partial class MonsterAnimationManager
    {
        /// <summary>
        /// Trả về danh sách ảnh động tác tương ứng
        /// </summary>
        /// <param name="resID"></param>
        /// <param name="actionType"></param>
        /// <param name="dir"></param>
        /// <returns></returns>
        public Dictionary<string, UnityEngine.Object> GetSprites(string resID, MonsterActionType actionType, Direction dir)
        {
            /// Nếu không có resID
            if (string.IsNullOrEmpty(resID))
            {
                return null;
            }

            /// File quy định đường dẫn tương ứng
            MonsterActionSetXML actionSetXML = Loader.Loader.MonsterActionSetXML;

            /// Nếu không tồn tại Res
            if (!actionSetXML.Monsters.TryGetValue(resID, out MonsterActionSetXML.Component resData))
            {
                return null;
            }

            /// Tên động tác
            string actionName;
            /// Nếu là đơn hướng
            if (!resData.Use8Dir)
            {
                actionName = this.GetActionName(actionType, resData.AutoFlip);
            }
            else
            {
                /// Nếu tự xoay hướng
                if (resData.AutoFlip)
                {
                    /// Trả về hướng tự xoay
                    dir = KTGlobal.GetAutoFlipDirection(dir);
                }
                actionName = string.Format("{0}_{1}", this.GetActionName(actionType, resData.AutoFlip), (int) dir);
            }

            /// File AssetBundle tương ứng
            string bundleDir = string.Format("{0}/{1}.unity3d", actionSetXML.Monsters[resID].BundleDir, actionName);

            /// Trả về kết quả
            return KTResourceManager.Instance.GetSubAssets(bundleDir, actionName);
        }

        /// <summary>
        /// Tải xuống Sprite và thêm vào danh sách trong Cache (nếu chưa được tải xuống)
        /// </summary>
        /// <param name="resID"></param>
        /// <param name="actionType"></param>
        /// <param name="dir"></param>
        /// <param name="callbackIfNeedToLoad"></param>
        /// <returns></returns>
        public IEnumerator LoadSprites(string resID, MonsterActionType actionType, Direction dir, Action callbackIfNeedToLoad)
        {
            /// Nếu không có resID
            if (string.IsNullOrEmpty(resID))
			{
                yield break;
			}

            /// File quy định đường dẫn tương ứng
            MonsterActionSetXML actionSetXML = Loader.Loader.MonsterActionSetXML;

            /// Nếu không tồn tại Res
            if (!actionSetXML.Monsters.TryGetValue(resID, out MonsterActionSetXML.Component resData))
			{
                yield break;
			}

            /// Tên động tác
            string actionName;
            /// Nếu là đơn hướng
            if (!resData.Use8Dir)
			{
                actionName = this.GetActionName(actionType, resData.AutoFlip);
            }
			else
			{
                /// Nếu tự xoay hướng
                if (resData.AutoFlip)
                {
                    /// Trả về hướng tự xoay
                    dir = KTGlobal.GetAutoFlipDirection(dir);
                }
                actionName = string.Format("{0}_{1}", this.GetActionName(actionType, resData.AutoFlip), (int) dir);
            }

            /// File AssetBundle tương ứng
            string bundleDir = string.Format("{0}/{1}.unity3d", actionSetXML.Monsters[resID].BundleDir, actionName);

            /// Nếu Bundle chưa được tải xuống
            if (!KTResourceManager.Instance.HasBundle(bundleDir))
            {
                /// Thực hiện Callback đánh dấu cần Load
                callbackIfNeedToLoad?.Invoke();

                /// Nếu sử dụng phương thức Async
                if (MonsterAnimationManager.UseAsyncLoad)
                {
                    /// Tải xuống AssetBundle
                    yield return KTResourceManager.Instance.LoadAssetBundleAsync(bundleDir, false, KTResourceManager.KTResourceCacheType.CachedUntilChangeScene);
                    /// Tải xuống Asset tương ứng
                    yield return KTResourceManager.Instance.LoadAssetWithSubAssetsAsync<Sprite>(bundleDir, actionName, true, null, KTResourceManager.KTResourceCacheType.CachedUntilChangeScene);
                }
                /// Nếu sử dụng phương thức tuần tự
                else
                {
                    /// Tải xuống AssetBundle
                    KTResourceManager.Instance.LoadAssetBundle(bundleDir, false, KTResourceManager.KTResourceCacheType.CachedUntilChangeScene);
                    /// Tải xuống Asset tương ứng
                    KTResourceManager.Instance.LoadAssetWithSubAssets<Sprite>(bundleDir, actionName, true, KTResourceManager.KTResourceCacheType.CachedUntilChangeScene);
                }
            }
			/// Nếu Bundle đã được tải xuống
			else
			{
                /// Tăng tham chiếu Bundle
                KTResourceManager.Instance.GetAssetBundle(bundleDir);
                /// Nếu Asset chưa được tải xuống
                if (!KTResourceManager.Instance.HasAsset(bundleDir, actionName))
                {
                    /// Nếu sử dụng phương thức Async
                    if (MonsterAnimationManager.UseAsyncLoad)
                    {
                        /// Tải xuống Asset tương ứng
                        yield return KTResourceManager.Instance.LoadAssetWithSubAssetsAsync<Sprite>(bundleDir, actionName, true, null, KTResourceManager.KTResourceCacheType.CachedUntilChangeScene);
                    }
                    /// Nếu sử dụng phương thức tuần tự
                    else
                    {
                        /// Tải xuống Asset tương ứng
                        KTResourceManager.Instance.LoadAssetWithSubAssets<Sprite>(bundleDir, actionName, true, KTResourceManager.KTResourceCacheType.CachedUntilChangeScene);
                    }
                }
            }
        }

        /// <summary>
        /// Giải phóng Sprite đã tải xuống
        /// </summary>
        /// <param name="resID"></param>
        public void UnloadSprites(string resID, MonsterActionType actionType, Direction dir)
        {
            /// Nếu không có resID
            if (string.IsNullOrEmpty(resID))
            {
                return;
            }

            /// File quy định đường dẫn tương ứng
            MonsterActionSetXML actionSetXML = Loader.Loader.MonsterActionSetXML;

            /// Nếu không tồn tại Res
            if (!actionSetXML.Monsters.TryGetValue(resID, out MonsterActionSetXML.Component resData))
            {
                return;
            }

            /// Tên động tác
            string actionName;
            /// Nếu là đơn hướng
            if (!resData.Use8Dir)
            {
                actionName = this.GetActionName(actionType, resData.AutoFlip);
            }
            else
            {
                /// Nếu tự xoay hướng
                if (resData.AutoFlip)
                {
                    /// Trả về hướng tự xoay
                    dir = KTGlobal.GetAutoFlipDirection(dir);
                }
                actionName = string.Format("{0}_{1}", this.GetActionName(actionType, resData.AutoFlip), (int) dir);
            }

            /// File AssetBundle tương ứng
            string bundleDir = string.Format("{0}/{1}.unity3d", actionSetXML.Monsters[resID].BundleDir, actionName);

            /// Giải phóng AssetBundle
            KTResourceManager.Instance.ReleaseBundle(bundleDir);
        }
    }
}
