using FS.VLTK.Entities.Config;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static FS.VLTK.Entities.Enum;

namespace FS.VLTK.Factory.Animation
{
    /// <summary>
    /// Quản lý vũ khí
    /// </summary>
    public partial class CharacterAnimationManager
    {
        /// <summary>
        /// Trả về danh sách ảnh mũ tương ứng
        /// </summary>
        /// <param name="actionType"></param>
        /// <param name="resID"></param>
        /// <param name="sex"></param>
        /// <param name="isRiding"></param>
        /// <param name="dir"></param>
        /// <returns></returns>
        public Dictionary<string, UnityEngine.Object> GetWeaponSprites(PlayerActionType actionType, string resID, Sex sex, bool isRiding, Direction dir)
        {
            /// File quy định đường dẫn tương ứng
            CharacterActionSetXML actionSetXML = Loader.Loader.CharacterActionSetXML;

            /// Tên động tác
            string actionName = string.Format("{0}_{1}", this.GetActionName(actionType, resID, isRiding), (int) dir);
            /// Đường dẫn AssetBundle chứa động tác
            string bundleDir = string.Format("{0}/{1}.unity3d", (sex == Sex.FEMALE ? actionSetXML.FemaleWeapon[resID].BundleDir : actionSetXML.MaleWeapon[resID].BundleDir), actionName);

            /// Trả về kết quả
            return KTResourceManager.Instance.GetSubAssets(bundleDir, actionName);
        }

        /// <summary>
        /// Tải xuống ảnh vũ khí tương ứng
        /// </summary>
        /// <param name="actionType"></param>
        /// <param name="resID"></param>
        /// <param name="weaponID"></param>
        /// <param name="sex"></param>
        /// <param name="isRiding"></param>
        /// <param name="dir"></param>
        /// <param name="callbackIfNeedToLoad"></param>
        /// <returns></returns>
        public IEnumerator LoadWeaponSprites(PlayerActionType actionType, string resID, Sex sex, bool isRiding, Direction dir, Action callbackIfNeedToLoad)
        {
            /// File quy định đường dẫn tương ứng
            CharacterActionSetXML actionSetXML = Loader.Loader.CharacterActionSetXML;

            /// Tên động tác
            string actionName = string.Format("{0}_{1}", this.GetActionName(actionType, resID, isRiding), (int) dir);
            /// Đường dẫn AssetBundle chứa động tác
            string bundleDir = string.Format("{0}/{1}.unity3d", (sex == Sex.FEMALE ? actionSetXML.FemaleWeapon[resID].BundleDir : actionSetXML.MaleWeapon[resID].BundleDir), actionName);

            /// Nếu Bundle chưa được tải xuống
            if (!KTResourceManager.Instance.HasBundle(bundleDir))
            {
                /// Thực hiện Callback đánh dấu cần Load
                callbackIfNeedToLoad?.Invoke();

                /// Nếu sử dụng phương thức Async
                if (CharacterAnimationManager.UseAsyncLoad)
                {
                    /// Tải xuống AssetBundle
                    yield return KTResourceManager.Instance.LoadAssetBundleAsync(bundleDir, false, KTResourceManager.KTResourceCacheType.CachedUntilChangeScene);
                    /// Tải xuống Asset tương ứng
                    yield return KTResourceManager.Instance.LoadAssetWithSubAssetsAsync<Sprite>(bundleDir, actionName, true, null, KTResourceManager.KTResourceCacheType.CachedUntilChangeScene);
                }
                /// Nếu sử dụng phương thức tải tuần tự
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
                /// Tăng tham chiếu
                KTResourceManager.Instance.GetAssetBundle(bundleDir);
                /// Nếu Asset chưa được tải xuống
                if (!KTResourceManager.Instance.HasAsset(bundleDir, actionName))
                {
                    /// Nếu sử dụng phương thức Async
                    if (CharacterAnimationManager.UseAsyncLoad)
                    {
                        /// Tải xuống Asset tương ứng
                        yield return KTResourceManager.Instance.LoadAssetWithSubAssetsAsync<Sprite>(bundleDir, actionName, true, null, KTResourceManager.KTResourceCacheType.CachedUntilChangeScene);
                    }
                    /// Nếu sử dụng phương thức tải tuần tự
                    else
                    {
                        /// Tải xuống Asset tương ứng
                        KTResourceManager.Instance.LoadAssetWithSubAssets<Sprite>(bundleDir, actionName, true, KTResourceManager.KTResourceCacheType.CachedUntilChangeScene);
                    }
                }
            }
        }

        /// <summary>
        /// Giải phóng Sprite vũ khí đã tải xuống
        /// </summary>
        /// <param name="actionType"></param>
        /// <param name="resID"></param>
        /// <param name="weaponID"></param>
        /// <param name="sex"></param>
        /// <param name="isRiding"></param>
        public void UnloadWeaponSprites(PlayerActionType actionType, string resID, Sex sex, bool isRiding, Direction dir)
        {
            /// File quy định đường dẫn tương ứng
            CharacterActionSetXML actionSetXML = Loader.Loader.CharacterActionSetXML;

            /// Tên động tác
            string actionName = string.Format("{0}_{1}", this.GetActionName(actionType, resID, isRiding), (int) dir);
            /// Đường dẫn AssetBundle chứa động tác
            string bundleDir = string.Format("{0}/{1}.unity3d", (sex == Sex.FEMALE ? actionSetXML.FemaleWeapon[resID].BundleDir : actionSetXML.MaleWeapon[resID].BundleDir), actionName);

            /// Giải phóng AssetBundle
            KTResourceManager.Instance.ReleaseBundle(bundleDir);
        }
    }
}
