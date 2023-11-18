using FS.VLTK.Entities.Config;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FS.VLTK.Factory.AnimationManager
{
	/// <summary>
	/// Quản lý hiệu ứng đạn
	/// </summary>
	public partial class BulletAnimationManager
	{
        /// <summary>
        /// Trả về danh sách ảnh động tác nổ tương ứng
        /// </summary>
        /// <param name="resID"></param>
        /// <returns></returns>
        public Dictionary<string, UnityEngine.Object> GetExplodeSprites(int resID)
        {
            /// File quy định đường dẫn tương ứng
            BulletActionSetXML actionSetXML = Loader.Loader.BulletActionSetXML;

            /// Nếu Res không tồn tại
            if (!actionSetXML.ResDatas.TryGetValue(resID, out BulletActionSetXML.BulletResData bulletData))
            {
                return null;
            }

            /// Nếu không có hiệu ứng tan
            if (!bulletData.HasExplodeAction)
            {
                return null;
            }

            /// Tên động tác
            string actionName = "AnimFile4";
            /// File AssetBundle tương ứng
            string bundleDir = string.Format("{0}/{1}/{2}.unity3d", bulletData.BundleDir, actionName, resID);

            /// Trả về kết quả
            return KTResourceManager.Instance.GetSubAssets(bundleDir, resID.ToString());
        }

        /// <summary>
        /// Tải xuống Sprite hiệu ứng nổ và thêm vào danh sách trong Cache (nếu chưa được tải xuống)
        /// </summary>
        /// <param name="resID"></param>
        /// <returns></returns>
        public IEnumerator LoadExplodeEffectSprites(int resID, Action callbackIfNeedToLoad = null)
        {
            /// Thông tin Res
            BulletActionSetXML actionSetXML = Loader.Loader.BulletActionSetXML;

            /// Nếu Res không tồn tại
            if (!actionSetXML.ResDatas.TryGetValue(resID, out BulletActionSetXML.BulletResData bulletData))
            {
                yield break;
            }

            /// Nếu không có hiệu ứng nổ
            if (!bulletData.HasExplodeAction)
            {
                yield break;
            }

            /// Tên động tác
            string actionName = "AnimFile4";
            /// File AssetBundle tương ứng
            string bundleDir = string.Format("{0}/{1}/{2}.unity3d", bulletData.BundleDir, actionName, resID);

            /// Nếu Bundle chưa được tải xuống
            if (!KTResourceManager.Instance.HasBundle(bundleDir))
            {
                /// Thực hiện Callback đánh dấu cần Load
                callbackIfNeedToLoad?.Invoke();

                /// Nếu sử dụng phương thức Async
                if (BulletAnimationManager.UseAsyncLoad)
                {
                    /// Tải xuống AssetBundle
                    yield return KTResourceManager.Instance.LoadAssetBundleAsync(bundleDir, false, KTResourceManager.KTResourceCacheType.CachedUntilChangeScene);
                    /// Tải xuống Asset tương ứng
                    yield return KTResourceManager.Instance.LoadAssetWithSubAssetsAsync<Sprite>(bundleDir, resID.ToString(), true, null, KTResourceManager.KTResourceCacheType.CachedUntilChangeScene);
                }
                /// Nếu sử dụng phương thức tuần tự
                else
                {
                    /// Tải xuống AssetBundle
                    KTResourceManager.Instance.LoadAssetBundle(bundleDir, false, KTResourceManager.KTResourceCacheType.CachedUntilChangeScene);
                    /// Tải xuống Asset tương ứng
                    KTResourceManager.Instance.LoadAssetWithSubAssets<Sprite>(bundleDir, resID.ToString(), true, KTResourceManager.KTResourceCacheType.CachedUntilChangeScene);
                }
            }
            /// Nếu Bundle đã được tải xuống
            else
            {
                /// Tăng tham chiếu Bundle
                KTResourceManager.Instance.GetAssetBundle(bundleDir);
                /// Nếu Asset chưa được tải xuống
                if (!KTResourceManager.Instance.HasAsset(bundleDir, resID.ToString()))
                {
                    /// Nếu sử dụng phương thức Async
                    if (BulletAnimationManager.UseAsyncLoad)
                    {
                        /// Tải xuống Asset tương ứng
                        yield return KTResourceManager.Instance.LoadAssetWithSubAssetsAsync<Sprite>(bundleDir, resID.ToString(), true, null, KTResourceManager.KTResourceCacheType.CachedUntilChangeScene);
                    }
                    /// Nếu sử dụng phương thức tuần tự
                    else
                    {
                        /// Tải xuống Asset tương ứng
                        KTResourceManager.Instance.LoadAssetWithSubAssets<Sprite>(bundleDir, resID.ToString(), true, KTResourceManager.KTResourceCacheType.CachedUntilChangeScene);
                    }
                }
            }
        }

        /// <summary>
        /// Giải phóng Sprite đã tải xuống
        /// </summary>
        /// <param name="resID"></param>
        public void UnloadExplodeEffectSprites(int resID)
        {
            /// Thông tin Res
            BulletActionSetXML actionSetXML = Loader.Loader.BulletActionSetXML;

            /// Nếu Res không tồn tại
            if (!actionSetXML.ResDatas.TryGetValue(resID, out BulletActionSetXML.BulletResData bulletData))
            {
                return;
            }

            /// Nếu không có hiệu ứng nổ
            if (!bulletData.HasExplodeAction)
            {
                return;
            }

            /// Tên động tác
            string actionName = "AnimFile4";
            /// File AssetBundle tương ứng
            string bundleDir = string.Format("{0}/{1}/{2}.unity3d", bulletData.BundleDir, actionName, resID);

            /// Giải phóng AssetBundle
            KTResourceManager.Instance.ReleaseBundle(bundleDir);
        }
    }
}
