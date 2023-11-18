using FS.GameEngine.Logic;
using FS.VLTK.Factory;
using FS.VLTK.Loader;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace FS.VLTK.Utilities.UnityUI
{
    /// <summary>
    /// Đối tượng quản lý Resource UI của game
    /// </summary>
    public class KTUIResourceManager : MonoBehaviour
    {
        #region Singleton Instance
        /// <summary>
        /// Đối tượng quản lý Resource UI của game
        /// </summary>
        public static KTUIResourceManager Instance { get; private set; }

        /// <summary>
        /// Hàm này gọi đến khi đối tượng được khởi tạo
        /// </summary>
        private void Awake()
        {
            KTUIResourceManager.Instance = this;
        }
        #endregion

        #region Defines
        /// <summary>
        /// Danh sách Bundle chứa UI được tải lên
        /// </summary>
        private readonly Dictionary<string, SpriteBundle> listBundles = new Dictionary<string, SpriteBundle>();

        /// <summary>
        /// Lớp mô tả Bundle Sprite
        /// </summary>
        private class SpriteBundle : IDisposable
        {
            /// <summary>
            /// Tên Bundle
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Số tham chiếu đến đối tượng
            /// </summary>
            public int ReferenceCount { get; set; }

            /// <summary>
            /// Danh sách Sprite bên trong
            /// </summary>
            public Dictionary<string, Sprite> Sprites { get; set; }

            /// <summary>
            /// Thời điểm dùng lần cuối
            /// </summary>
            public long LastUsedTicks { get; set; }

            /// <summary>
            /// Hủy đối tượng
            /// </summary>
            public void Dispose()
			{
                this.Sprites.Clear();
                this.Sprites = null;
			}
        }
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi đến ở frame đầu tiên
        /// </summary>
        private void Start()
        {
            this.StartCoroutine(this.AutoCollectUnusedBundle());
        }

        /// <summary>
        /// Tự động thu thập các AssetBundle không được dùng đến
        /// </summary>
        /// <returns></returns>
        private IEnumerator AutoCollectUnusedBundle()
        {
            while (true)
            {
                yield return new WaitForSeconds(10f);

                foreach (string key in this.listBundles.Keys.ToList())
                {
                    SpriteBundle bundle = this.listBundles[key];
                    /// Nếu tham chiếu về 0 và đã đến lúc xóa
                    if (bundle.ReferenceCount <= 0 && KTGlobal.GetCurrentTimeMilis() - bundle.LastUsedTicks >= 10000)
                    {
                        /// Xóa Sprite bên trong
                        foreach (Sprite sprite in bundle.Sprites.Values)
						{
                            Resources.UnloadAsset(sprite.texture);
                            GameObject.DestroyImmediate(sprite.texture, true);
                            GameObject.DestroyImmediate(sprite, true);
                        }
                        /// Xóa khỏi danh sách
                        this.listBundles.Remove(key);

                        //KTDebug.LogError("Remove UIAssetBundle -> " + key + ", not used for 10s");
                        bundle.Dispose();
                    }
                }
            }
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Tải Bundle
        /// </summary>
        /// <param name="bundleDir"></param>
        public void LoadBundle(string bundleDir)
        {
            /// Nếu tồn tại ở ResourceManager
            if (KTResourceManager.Instance.HasBundle(bundleDir))
            {
                /// Bỏ qua
                return;
            }

            if (this.listBundles.ContainsKey(bundleDir))
            {
                this.listBundles[bundleDir].ReferenceCount++;
                return;
            }

            string url = Global.WebPath(string.Format("Data/{0}", bundleDir));
#if UNITY_IOS || UNITY_EDITOR
            if (url.Contains("file:///"))
			{
                url = url.Replace("file:///", "");
			}
#endif


            AssetBundle bundle = AssetBundle.LoadFromFile(url);
            if (bundle == null)
            {
                KTDebug.LogError("UIAssetBundle not found -> " + url);
                return;
            }

            Dictionary<string, Sprite> loadedAssets = new Dictionary<string, Sprite>();
            /// Tải xuống các Asset con
            foreach (string assetName in bundle.GetAllAssetNames())
			{
                string atlasName = Path.GetFileNameWithoutExtension(assetName);
                Sprite[] subAssets = bundle.LoadAssetWithSubAssets<UnityEngine.Sprite>(atlasName);
                foreach (Sprite sprite in subAssets)
				{
                    string spriteName = string.Format("{0}_{1}", atlasName, sprite.name);
                    loadedAssets[spriteName] = sprite;
				}
            }

            /// Giải phóng Bundle
            bundle.Unload(false);
            GameObject.Destroy(bundle);

            this.listBundles[bundleDir] = new SpriteBundle()
            {
                Name = bundleDir,
                Sprites = loadedAssets,
                ReferenceCount = 1,
            };
            //KTDebug.LogError("Load UIAssetBundle successfully at -> " + url);
        }

        /// <summary>
        /// Lấy giá trị Sprite được tải ở Atlas tương ứng trong Bundle
        /// </summary>
        /// <param name="bundleDir"></param>
        /// <param name="atlasName"></param>
        /// <param name="spriteName"></param>
        /// <returns></returns>
        public Sprite GetSprite(string bundleDir, string atlasName, string spriteName)
        {
            /// Nếu tồn tại ở ResourceManager
            if (KTResourceManager.Instance.HasBundle(bundleDir))
            {
                /// Trả về kết quả
                return KTResourceManager.Instance.GetSubAsset<Sprite>(bundleDir, atlasName, spriteName);
            }

            /// Nếu không tìm thấy Bundle
            if (!this.listBundles.TryGetValue(bundleDir, out SpriteBundle bundle))
            {
                KTDebug.LogError("UIAssetBundle not found -> " + bundleDir);
                return null;
            }

            /// Tên Sprite
            string fullSpriteName = string.Format("{0}_{1}", atlasName.ToLower(), spriteName);
            if (!bundle.Sprites.TryGetValue(fullSpriteName, out Sprite sprite))
            {
                KTDebug.LogError("UISprite not found -> " + fullSpriteName + ".");
                return null;
            }

            /// Trả về kết quả
            return sprite;
        }

        /// <summary>
        /// Giảm tham chiếu đến AssetBundle tương ứng, nếu tham chiếu <= 0 thì tự động bị thu hồi
        /// </summary>
        /// <param name="bundleDir"></param>
        public void UnloadAssetBundle(string bundleDir)
        {
            /// Nếu tồn tại ở ResourceManager
            if (KTResourceManager.Instance.HasBundle(bundleDir))
            {
                /// Bỏ qua
                return;
            }

            if (!this.listBundles.ContainsKey(bundleDir))
            {
                KTDebug.LogError("UIAssetBundle not found -> " + bundleDir);
                return;
            }
            this.listBundles[bundleDir].ReferenceCount--;
            this.listBundles[bundleDir].LastUsedTicks = KTGlobal.GetCurrentTimeMilis();
        }
        #endregion
    }
}