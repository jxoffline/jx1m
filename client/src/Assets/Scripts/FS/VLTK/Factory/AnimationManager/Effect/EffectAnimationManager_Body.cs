using FS.VLTK.Entities.ActionSet;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace FS.VLTK.Factory.Animation
{
	/// <summary>
	/// Quản lý Animation hiệu ứng
	/// </summary>
	public partial class EffectAnimationManager
	{
        /// <summary>
        /// Trả về danh sách ảnh hiệu ứng tương ứng
        /// </summary>
        /// <param name="resID"></param>
        /// <param name="dir16"></param>
        /// <returns></returns>
        public Dictionary<string, UnityEngine.Object> GetSprites(int resID)
        {
            /// Nếu Res không tồn tại
            if (!Loader.Loader.ListEffects.TryGetValue(resID, out Entities.Config.StateEffectXML actionSetXML))
            {
                return null;
            }

            /// Tên động tác
            string actionName = Path.GetFileNameWithoutExtension(actionSetXML.EffectBundle);
            /// File AssetBundle tương ứng
            string bundleDir = actionSetXML.EffectBundle;

            /// Nếu không tồn tại Bundle
            if (string.IsNullOrEmpty(bundleDir))
			{
                return null;
			}

            /// Trả về kết quả
            return KTResourceManager.Instance.GetSubAssets(bundleDir, actionName);
        }

        /// <summary>
        /// Tải xuống Sprite và thêm vào danh sách trong Cache (nếu chưa được tải xuống)
        /// </summary>
        /// <param name="resID"></param>
        /// <param name="callbackIfNeedToLoad"></param>
        /// <returns></returns>
        public IEnumerator LoadSprites(int resID, Action callbackIfNeedToLoad = null)
        {
            /// Nếu Res không tồn tại
            if (!Loader.Loader.ListEffects.TryGetValue(resID, out Entities.Config.StateEffectXML actionSetXML))
            {
                yield break;
            }

            /// Tên động tác
            string actionName = Path.GetFileNameWithoutExtension(actionSetXML.EffectBundle);
            /// File AssetBundle tương ứng
            string bundleDir = actionSetXML.EffectBundle;

            /// Nếu không tồn tại Bundle
            if (string.IsNullOrEmpty(bundleDir))
            {
                yield break;
            }

            /// Nếu Bundle chưa được tải xuống
            if (!KTResourceManager.Instance.HasBundle(bundleDir))
            {
                /// Thực hiện Callback đánh dấu cần Load
                callbackIfNeedToLoad?.Invoke();

                /// Nếu sử dụng phương thức Async
                if (EffectAnimationManager.UseAsyncLoad)
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
                    if (EffectAnimationManager.UseAsyncLoad)
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
        public void UnloadSprites(int resID)
        {
            /// Nếu Res không tồn tại
            if (!Loader.Loader.ListEffects.TryGetValue(resID, out Entities.Config.StateEffectXML actionSetXML))
            {
                return;
            }

            /// File AssetBundle tương ứng
            string bundleDir = actionSetXML.EffectBundle;

            /// Nếu không tồn tại Bundle
            if (string.IsNullOrEmpty(bundleDir))
            {
                return;
            }

            /// Giải phóng AssetBundle
            KTResourceManager.Instance.ReleaseBundle(bundleDir);
        }
    }
}
