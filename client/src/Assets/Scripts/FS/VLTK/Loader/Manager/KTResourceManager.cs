using FS.GameEngine.Logic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FS.VLTK.Factory
{
    /// <summary>
    /// Lớp quản lý tài nguyên
    /// </summary>
    public class KTResourceManager : TTMonoBehaviour
    {
        #region Singleton - Instance
        /// <summary>
        /// Lớp quản lý tài nguyên
        /// </summary>
        public static KTResourceManager Instance { get; private set; }

        /// <summary>
        /// Hàm này gọi khi đối tượng được tạo ra
        /// </summary>
        private void Awake()
        {
            KTResourceManager.Instance = this;
        }
        #endregion

        #region Define
        /// <summary>
        /// Loại đối tượng chờ tải
        /// </summary>
        private enum ItemType
        {
            /// <summary>
            /// Asset Bundle
            /// </summary>
            AssetBundle,
            /// <summary>
            /// Asset
            /// </summary>
            Asset,
            /// <summary>
            /// Sub Assets
            /// </summary>
            SubAssets,
        }

        /// <summary>
        /// Đối tượng chờ tải
        /// </summary>
        private class WaitingItem
        {
            /// <summary>
            /// Loại
            /// </summary>
            public ItemType Type { get; set; }

            /// <summary>
            /// Tên
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Loại Cache
            /// </summary>
            public KTResourceCacheType CacheType { get; set; }

            /// <summary>
            /// Kiểu dữ liệu đầu ra
            /// </summary>
            public Type ResultType { get; set; }

            /// <summary>
            /// Các tham biến khác
            /// </summary>
            public List<string> Params { get; set; }
        }

        /// <summary>
        /// Loại Cache
        /// </summary>
        public enum KTResourceCacheType
        {
            /*
            /// <summary>
            /// Lưu lại sau khoảng thời gian
            /// </summary>
            CachedForSeconds,
            */
            /// <summary>
            /// Lưu lại đến khi người chơi chuyển bản đồ
            /// </summary>
            CachedUntilChangeScene,
            /// <summary>
            /// Lưu lại vĩnh viễn
            /// </summary>
            CachedPermenently,
        }

        /// <summary>
        /// Thời gian thu thập rác
        /// </summary>
        private readonly float GCTick = 2f;

        /// <summary>
        /// Số lượng GC xử lý mỗi Frame
        /// </summary>
        private readonly int MaxGCProcessPerFrame = 10;
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi ở Frame đầu tiên
        /// </summary>
        private void Start()
        {
            /// Chạy luồng thực thi tải xuống dữ liệu
            this.StartCoroutine(this.LoadItemAsync());
            /// Chạy luồng thực thi các sự kiện chờ chạy ở luồng chính
            this.StartCoroutine(this.ExecuteOnMainThread());
            /// Chạy luồng dọn rác
            //this.StartCoroutine(this.TickTimeAssetBundle());
        }
        #endregion

        #region Waiting Queue
        /// <summary>
        /// Danh sách đang chờ tải xuống
        /// </summary>
        private readonly Queue<WaitingItem> WaitToBeLoadedItems = new Queue<WaitingItem>();

        /// <summary>
        /// Danh sách đang chờ tải xuống theo tên
        /// </summary>
        private readonly HashSet<string> WaitToBeLoadedItemsByName = new HashSet<string>();

        /// <summary>
        /// Danh sách sự kiện chờ chạy ở luồng chính
        /// </summary>
        private readonly Queue<Action> WaitToBeRunInMainThread = new Queue<Action>();
        #endregion

        #region Core functions
        /// <summary>
        /// Thực thi sự kiện ở luồng chính
        /// </summary>
        /// <returns></returns>
        private IEnumerator ExecuteOnMainThread()
        {
            /// Lặp liên tục
            while (true)
            {
                /// Nếu có sự kiện đang chờ
                if (this.WaitToBeRunInMainThread.Count > 0)
                {
                    /// Lấy sự kiện ra và thực thi
                    this.WaitToBeRunInMainThread.Dequeue()?.Invoke();
                }
                /// Bỏ qua Frame
                yield return null;
            }
        }

        /// <summary>
        /// Luồng thực hiện tải xuống dữ liệu
        /// </summary>
        /// <returns></returns>
        private IEnumerator LoadItemAsync()
        {
            /// Lặp liên tục
            while (true)
            {
                /// Nếu tồn tại đối tượng cần tải xuống
                if (this.WaitToBeLoadedItems.Count > 0)
                {
                    /// Đối tượng đang chờ tải xuống
                    WaitingItem item = this.WaitToBeLoadedItems.Dequeue();

                    /// Loại đối tượng
                    switch (item.Type)
                    {
                        case ItemType.AssetBundle:
                        {
                            /// Đường dẫn chứa Asset Bundle
                            string url = Global.WebPath(string.Format("Data/{0}", item.Name));
#if UNITY_IOS || UNITY_EDITOR
                            if (url.Contains("file:///"))
							{
                                url = url.Replace("file:///", "");
							}
#endif
                            AssetBundleCreateRequest bundleLoadRequest = AssetBundle.LoadFromFileAsync(url);
                            bundleLoadRequest.completed += (op) => {
                                /// AssetBundle tương ứng
                                AssetBundle assetBundle = bundleLoadRequest.assetBundle;

                                /// Thêm vào hệ thống
                                this.AddAssetBundle(item.Name, assetBundle, item.CacheType);

                                /// Xóa khỏi danh sách theo tên
                                this.WaitToBeLoadedItemsByName.Remove(item.Name);
                            };
                            break;
                        }
                        case ItemType.Asset:
                        {
                            /// Tên Bundle
                            string bundleName = item.Params[0];
                            /// Tên Asset
                            string assetName = item.Params[1];
                            /// Xóa Bundle sau khi tải xong
                            bool removeBundleAfterLoading = bool.Parse(item.Params[2]);

                            /// Nếu toác gì đó
                            if (this.Cache_AssetBundles[bundleName].Bundle == null)
                            {
                                /// Xóa khỏi danh sách theo tên
                                this.WaitToBeLoadedItemsByName.Remove(item.Name);
                                break;
                            }

                            /// Tạo yêu cầu tải xuống
                            AssetBundleRequest request = null;
                            try
                            {
                                if (item.ResultType == typeof(Sprite))
                                {
                                    request = this.Cache_AssetBundles[bundleName].Bundle.LoadAssetAsync<Sprite>(assetName);
                                }
                                else if (item.ResultType == typeof(AudioClip))
                                {
                                    request = this.Cache_AssetBundles[bundleName].Bundle.LoadAssetAsync<AudioClip>(assetName);
                                }
                                else
                                {
                                    request = this.Cache_AssetBundles[bundleName].Bundle.LoadAssetAsync(assetName);
                                }
                            }
                            catch (Exception ex)
                            {
                                KTDebug.LogError("Toac => " + bundleName + " - " + assetName);
                                KTDebug.LogException(ex);
                            }

                            /// Gửi yêu cầu tải
                            request.completed += (op) => {
                                /// Nếu có Asset
                                if (request.asset != null)
                                {
                                    /// Nếu cùng kiểu
                                    if (request.asset.GetType() == item.ResultType)
                                    {
                                        /// Thêm vào hệ thống
                                        this.AddAsset(item.Name, request.asset, item.CacheType);
                                    }
                                }
                                else
                                {
                                  //  KTDebug.LogError(string.Format("Can not find Asset, Bundle = {0}, Asset = {1}, Type = {2}, FullName = {3}", bundleName, assetName, item.ResultType, item.Name));
                                    //KTGlobal.WriteDebugConsoleLog(KTGlobal.DebugConsoleLogType.Error, string.Format("Can not find Asset, Bundle = {0}, Asset = {1}, Type = {2}, FullName = {3}", bundleName, assetName, item.ResultType, item.Name));
                                }

                                /// Nếu tải xong cần xóa Bundle
                                if (removeBundleAfterLoading)
                                {
                                    this.Cache_AssetBundles[bundleName].Bundle.Unload(false);
                                    GameObject.Destroy(this.Cache_AssetBundles[bundleName].Bundle);
                                    this.Cache_AssetBundles[bundleName].Bundle = null;
                                }

                                /// Xóa khỏi danh sách theo tên
                                this.WaitToBeLoadedItemsByName.Remove(item.Name);
                            };
                            break;
                        }
                        case ItemType.SubAssets:
                        {
                            /// Tên Bundle
                            string bundleName = item.Params[0];
                            /// Tên Asset
                            string assetName = item.Params[1];
                            /// Xóa Bundle sau khi tải xong
                            bool removeBundleAfterLoading = bool.Parse(item.Params[2]);

                            /// Nếu toác gì đó
                            if (this.Cache_AssetBundles[bundleName].Bundle == null)
                            {
                                /// Xóa khỏi danh sách theo tên
                                this.WaitToBeLoadedItemsByName.Remove(item.Name);
                                break;
                            }

                            //KTDebug.LogError("Begin load subassets of Bundle = " + bundleName + " - Asset = " + assetName);

                            /// Tạo yêu cầu tải xuống
                            AssetBundleRequest request = null;
                            try
                            {
                                if (item.ResultType == typeof(Sprite))
                                {
                                    request = this.Cache_AssetBundles[bundleName].Bundle.LoadAssetWithSubAssetsAsync<Sprite>(assetName);
                                }
                                else if (item.ResultType == typeof(AudioClip))
                                {
                                    request = this.Cache_AssetBundles[bundleName].Bundle.LoadAssetWithSubAssetsAsync<AudioClip>(assetName);
                                }
                                else
                                {
                                    request = this.Cache_AssetBundles[bundleName].Bundle.LoadAssetWithSubAssetsAsync(assetName);
                                }
                            }
                            catch (Exception ex)
                            {
                                KTDebug.LogError("Toac => " + bundleName + " - " + assetName);
                                KTDebug.LogException(ex);
                            }

                            /// Gửi yêu cầu tải
                            request.completed += (op) => {
                                /// Thêm vào hệ thống
                                this.AddAsset(item.Name, request.asset, item.CacheType);
                                /// Tạo mới danh sách SubAssets
                                this.Cache_Resources[item.Name].SubAssets = new Dictionary<string, UnityEngine.Object>();

                                /// Nếu có danh sách Asset con
                                if (request.allAssets != null)
                                {
                                    /// Duyệt toàn bộ danh sách Asset con tìm thấy
                                    for (int i = 0; i < request.allAssets.Length; i++)
                                    {
                                        /// Nếu kiểu thỏa mãn
                                        if (request.allAssets[i].GetType() == item.ResultType)
                                        {
                                            /// Thêm Asset con vào danh sách
                                            this.Cache_Resources[item.Name].SubAssets[request.allAssets[i].name] = request.allAssets[i];
                                        }
                                    }
                                }
                                else
                                {
                                    KTDebug.LogError(string.Format("Can not find SubAssets, Bundle = {0}, Asset = {1}, Type = {2}, FullName = {3}", bundleName, assetName, item.ResultType, item.Name));
                                    //KTGlobal.WriteDebugConsoleLog(KTGlobal.DebugConsoleLogType.Error, string.Format("Can not find SubAssets, Bundle = {0}, Asset = {1}, Type = {2}, FullName = {3}", bundleName, assetName, item.ResultType, item.Name));
                                }

                                /// Nếu tải xong cần xóa Bundle
                                if (removeBundleAfterLoading)
                                {
                                    this.Cache_AssetBundles[bundleName].Bundle.Unload(false);
                                    GameObject.Destroy(this.Cache_AssetBundles[bundleName].Bundle);
                                    this.Cache_AssetBundles[bundleName].Bundle = null;
                                }

                                /// Xóa khỏi danh sách theo tên
                                this.WaitToBeLoadedItemsByName.Remove(item.Name);
                            };

                            break;
                        }
                    }
                }

                yield return null;
            }
        }
        #endregion

        #region AssetBundle
        #region Define
        /// <summary>
        /// Lớp biểu diễn Asset Bundle
        /// </summary>
        private class KTAssetBundle
        {
            /// <summary>
            /// Tên Bundle
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Bundle
            /// </summary>
            public AssetBundle Bundle { get; set; }

            /// <summary>
            /// Loại Cache
            /// </summary>
            public KTResourceCacheType CacheType { get; set; }

            /// <summary>
            /// Tổng số tham chiếu
            /// </summary>
            public int ReferenceCount { get; set; }

            /// <summary>
            /// Thời gian xóa
            /// </summary>
            public float UnloadTick { get; set; }

            /// <summary>
            /// Đã tải xuống hoàn tất chưa
            /// </summary>
            public bool DoneLoading { get; set; }

            /// <summary>
            /// Chuyển đối tượng về dạng String
            /// </summary>
            /// <returns></returns>
			public override string ToString()
            {
                return string.Format("Name: {0}, CacheType: {1}, RefCount: {2}", this.Name, this.CacheType, this.ReferenceCount);
            }

            /// <summary>
            /// Hủy đối tượng
            /// </summary>
            public void Dispose()
            {
                this.Name = null;
                this.Bundle = null;

            }
        }

        /// <summary>
        /// Danh sách Asset Bundle được lưu lại hệ thống
        /// </summary>
        private readonly Dictionary<string, KTAssetBundle> Cache_AssetBundles = new Dictionary<string, KTAssetBundle>();

        /// <summary>
        /// Thời gian Cache AssetBundle tương ứng trong hệ thống
        /// </summary>
        private readonly float BundleKeepTick = 10f;
        #endregion

        #region Methods
        /// <summary>
        /// Kiểm tra trong Cache có chứa AssetBundle tương ứng không
        /// </summary>
        /// <param name="bundleName"></param>
        /// <returns></returns>
        public bool HasBundle(string bundleName)
        {
            return this.Cache_AssetBundles.TryGetValue(bundleName, out _);
        }

        /// <summary>
        /// Thêm AssetBundle tương ứng vào hệ thống
        /// </summary>
        /// <param name="bundleName"></param>
        /// <param name="assetBundle"></param>
        /// <param name="cacheType"></param>
        private void AddAssetBundle(string bundleName, AssetBundle assetBundle, KTResourceCacheType cacheType = KTResourceCacheType.CachedUntilChangeScene)
        {
            /// Nếu đã tồn tại trong hệ thống thì bỏ qua
            if (this.Cache_AssetBundles.TryGetValue(bundleName, out _))
            {
                this.Cache_AssetBundles[bundleName].ReferenceCount++;
                this.Cache_AssetBundles[bundleName].UnloadTick = this.BundleKeepTick;
                return;
            }
            /// Nếu AssetBundle NULL
            else if (assetBundle == null)
            {
                return;
            }

            /// Tạo mới Bundle tương ứng
            this.Cache_AssetBundles[bundleName] = new KTAssetBundle()
            {
                Name = bundleName,
                Bundle = assetBundle,
                ReferenceCount = 1,
                UnloadTick = this.BundleKeepTick,
                CacheType = cacheType,
            };

            //KTDebug.LogError("Add Bundle " + bundleName + " - RefCount = " + this.Cache_AssetBundles[bundleName].ReferenceCount);
            //KTGlobal.WriteDebugConsoleLog(KTGlobal.DebugConsoleLogType.Success, "Add Bundle " + bundleName + " - RefCount = " + this.Cache_AssetBundles[bundleName].ReferenceCount);
        }

        /// <summary>
        /// Trả ra AssetBundle tương ứng được lưu trong Cache
        /// </summary>
        /// <param name="bundleName"></param>
        /// <returns></returns>
        public AssetBundle GetAssetBundle(string bundleName)
        {
            this.Cache_AssetBundles.TryGetValue(bundleName, out KTAssetBundle bundle);
            if (bundle == null)
            {
                return null;
            }
            this.Cache_AssetBundles[bundleName].ReferenceCount++;
            this.Cache_AssetBundles[bundleName].UnloadTick = this.BundleKeepTick;

            //KTDebug.LogError("Get Bundle " + bundleName + " - RefCount = " + this.Cache_AssetBundles[bundleName].ReferenceCount);
            //KTGlobal.WriteDebugConsoleLog(KTGlobal.DebugConsoleLogType.Success, "Get Bundle " + bundleName + " - RefCount = " + this.Cache_AssetBundles[bundleName].ReferenceCount);
            return bundle.Bundle;
        }

        /// <summary>
        /// Tải xuống AssetBundle
        /// </summary>
        /// <param name="bundleDir"></param>
        /// <param name="isEncrypted"></param>
        /// <param name="cacheType"></param>
        public void LoadAssetBundle(string bundleDir, bool isEncrypted = false, KTResourceCacheType cacheType = KTResourceCacheType.CachedUntilChangeScene)
        {
            /// Nếu đã tồn tại trong hệ thống
            if (this.HasBundle(bundleDir))
            {
                /// Tăng tham chiếu đến Asset tương ứng
                this.Cache_AssetBundles[bundleDir].ReferenceCount++;
                /// Cập nhật thời gian Tick tự xóa
                this.Cache_AssetBundles[bundleDir].UnloadTick = this.BundleKeepTick;

                //KTDebug.LogError("Increase bundle " + bundleDir + " ref = " + this.Cache_AssetBundles[bundleDir].ReferenceCount);
                //KTGlobal.WriteDebugConsoleLog(KTGlobal.DebugConsoleLogType.Success, "Increase bundle " + bundleDir + " ref = " + this.Cache_AssetBundles[bundleDir].ReferenceCount);
            }
            /// Nếu chưa tồn tại trong hệ thống
            else
            {
                /// Đường dẫn chứa Asset Bundle
                string url = Global.WebPath(string.Format("Data/{0}", bundleDir));

#if UNITY_IOS || UNITY_EDITOR
                if (url.Contains("file:///"))
				{
                    url = url.Replace("file:///", "");
				}
#endif
                /// AssetBundle tương ứng
                AssetBundle assetBundle = AssetBundle.LoadFromFile(url);

                //KTDebug.LogError(assetBundle.name);
                /// Thêm vào hệ thống
                this.AddAssetBundle(bundleDir, assetBundle, cacheType);
            }
        }

        /// <summary>
        /// Tải xuống AssetBundle theo phương thức Async
        /// </summary>
        /// <param name="bundleDir"></param>
        /// <param name="isEncrypted"></param>
        /// <param name="cacheType"></param>
        public IEnumerator LoadAssetBundleAsync(string bundleDir, bool isEncrypted = false, KTResourceCacheType cacheType = KTResourceCacheType.CachedUntilChangeScene)
        {
            /// Nếu đã tồn tại trong hệ thống
            if (this.HasBundle(bundleDir))
            {
                /// Tăng tham chiếu đến Asset tương ứng
                this.Cache_AssetBundles[bundleDir].ReferenceCount++;
                /// Cập nhật thời gian Tick tự xóa
                this.Cache_AssetBundles[bundleDir].UnloadTick = this.BundleKeepTick;

                //KTDebug.LogError("Increase bundle " + bundleDir + " ref = " + this.Cache_AssetBundles[bundleDir].ReferenceCount);
                //KTGlobal.WriteDebugConsoleLog(KTGlobal.DebugConsoleLogType.Success, "Increase bundle " + bundleDir + " ref = " + this.Cache_AssetBundles[bundleDir].ReferenceCount);
            }
            /// Nếu chưa tồn tại trong hệ thống
            else
            {
                /// Nếu đã tồn tại trong danh sách chờ tải xuống
                if (this.WaitToBeLoadedItemsByName.Contains(bundleDir))
                {
                    /// Đợi tải xuống hoàn tất
                    while (!this.HasBundle(bundleDir))
                    {
                        yield return null;
                    }

                    /// Tăng tham chiếu đến Asset tương ứng
                    this.Cache_AssetBundles[bundleDir].ReferenceCount++;
                    /// Cập nhật thời gian Tick tự xóa
                    this.Cache_AssetBundles[bundleDir].UnloadTick = this.BundleKeepTick;

                    //KTDebug.LogError("Increase bundle " + bundleDir + " ref = " + this.Cache_AssetBundles[bundleDir].ReferenceCount);
                    //KTGlobal.WriteDebugConsoleLog(KTGlobal.DebugConsoleLogType.Success, "Increase bundle " + bundleDir + " ref = " + this.Cache_AssetBundles[bundleDir].ReferenceCount);
                }
                /// Nếu chưa tồn tại trong danh sách chờ thì thêm vào danh sách chờ
                else
                {
                    this.WaitToBeLoadedItems.Enqueue(new WaitingItem()
                    {
                        Name = bundleDir,
                        Type = ItemType.AssetBundle,
                        CacheType = cacheType,
                        ResultType = typeof(AssetBundle),
                    });
                    this.WaitToBeLoadedItemsByName.Add(bundleDir);

                    /// Đợi tải xuống hoàn tất
                    while (!this.HasBundle(bundleDir) && this.WaitToBeLoadedItemsByName.Contains(bundleDir))
                    {
                        yield return null;
                    }
                }
            }
        }

        /// <summary>
        /// Hủy tham chiếu tới AssetBundle được lưu trong Cache
        /// </summary>
        /// <param name="bundleName"></param>
        public void ReleaseBundle(string bundleName)
        {
            /// Nếu BundleName rỗng
            if (string.IsNullOrEmpty(bundleName))
            {
                return;
            }
            /// Nếu chưa có Bundle tên tương ứng trong hệ thống
            else if (!this.Cache_AssetBundles.ContainsKey(bundleName))
            {
                return;
            }

            this.Cache_AssetBundles[bundleName].ReferenceCount--;
            if (this.Cache_AssetBundles[bundleName].ReferenceCount <= 0)
            {
                this.Cache_AssetBundles[bundleName].UnloadTick = this.BundleKeepTick;
            }

            //KTDebug.LogError("Release Bundle " + bundleName + " - RefCount = " + this.Cache_AssetBundles[bundleName].ReferenceCount);
            //KTGlobal.WriteDebugConsoleLog(KTGlobal.DebugConsoleLogType.Info, "Release Bundle " + bundleName + " - RefCount = " + this.Cache_AssetBundles[bundleName].ReferenceCount);
        }
        #endregion
        #endregion

        #region Resources
        #region Define
        /// <summary>
        /// Lớp biểu diễn tài nguyên
        /// </summary>
        private class KTResource : IDisposable
        {
            /// <summary>
            /// Tên Asset
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Loại Cache
            /// </summary>
            public KTResourceCacheType CacheType { get; set; }

            /// <summary>
            /// Asset
            /// </summary>
            public UnityEngine.Object Asset { get; set; }

            /// <summary>
            /// Các Asset con
            /// </summary>
            public Dictionary<string, UnityEngine.Object> SubAssets { get; set; }

            /// <summary>
            /// Hủy đối tượng
            /// </summary>
            public void Dispose()
            {
                this.Name = null;
                this.Asset = null;
                this.SubAssets.Clear();
                this.SubAssets = null;
            }
        }

        /// <summary>
        /// Danh sách tài nguyên được lưu lại hệ thóng
        /// </summary>
        private readonly Dictionary<string, KTResource> Cache_Resources = new Dictionary<string, KTResource>();

        /// <summary>
        /// Thời gian Cache Asset tương ứng trong hệ thống
        /// </summary>
        private readonly float ResourceKeepTick = 20f;
        #endregion

        #region Methods
        /// <summary>
        /// Kiểm tra trong Cache có chứa Asset tên tương ứng
        /// </summary>
        /// <param name="assetName"></param>
        /// <returns></returns>
        public bool HasAsset(string assetName)
        {
            /// Trả về kết quả
            return this.Cache_Resources.TryGetValue(assetName, out _);
        }

        /// <summary>
        /// Kiểm tra trong Cache có chứa Asset tên tương ứng
        /// </summary>
        /// <param name="bundleDir"></param>
        /// <param name="assetName"></param>
        /// <returns></returns>
        public bool HasAsset(string bundleDir, string assetName)
        {
            /// Tên Asset
            assetName = string.Format("{0}_{1}", bundleDir, assetName);
            /// Trả về kết quả
            return this.Cache_Resources.TryGetValue(assetName, out _);
        }

        /// <summary>
        /// Thêm Asset vào Cache
        /// </summary>
        /// <param name="assetName"></param>
        /// <param name="asset"></param>
        private void AddAsset(string assetName, UnityEngine.Object asset, KTResourceCacheType cacheType = KTResourceCacheType.CachedUntilChangeScene)
        {
            /// Nếu đã tồn tại trong hệ thống thì bỏ qua
            if (this.Cache_Resources.TryGetValue(assetName, out _))
            {
                return;
            }
            /// Nếu Asset NULL
            else if (asset == null)
            {
                return;
            }

            this.Cache_Resources[assetName] = new KTResource()
            {
                Name = assetName,
                Asset = asset,
                CacheType = cacheType,
                SubAssets = new Dictionary<string, UnityEngine.Object>(),
            };

            //KTDebug.LogError("Add Asset " + assetName + " - RefCount = " + this.Cache_Resources[assetName].ReferenceCount);
        }

        /// <summary>
        /// Trả về Asset tương ứng được Cache trong hệ thống
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="bundleName"></param>
        /// <param name="assetName"></param>
        /// <returns></returns>
        public T GetAsset<T>(string bundleName, string assetName) where T : UnityEngine.Object
        {
            /// Nếu không tồn tại Bundle tương ứng trong hệ thống
            if (!this.HasBundle(bundleName))
            {
                string msg = string.Format("No bundle -> {0} found", bundleName);
               // KTDebug.LogError(msg);

                /// Trả về NULL
                return null;
            }

            /// Tên đầy đủ của Asset
            assetName = string.Format("{0}_{1}", bundleName, assetName);
            /// Nếu không tồn tại Asset trong hệ thống
            if (!this.HasAsset(assetName))
            {
                string msg = string.Format("Get Asset -> {0} faild -> No corresponding Asset found.", assetName);
              //  KTDebug.LogError(msg);

                /// Trả về NULL
                return null;
            }

            /// Trả ra đối tượng Asset tương ứng kiểu T
            return this.Cache_Resources[assetName].Asset as T;
        }

        /// <summary>
        /// Trả về Asset con tương ứng được Cache trong hệ thống
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="bundleName"></param>
        /// <param name="assetName"></param>
        /// <param name="subAssetName"></param>
        /// <returns></returns>
        public T GetSubAsset<T>(string bundleName, string assetName, string subAssetName) where T : UnityEngine.Object
        {
            /// Nếu trong hệ thống không tồn tại Bundle tương ứng
            if (!this.HasBundle(bundleName))
            {
                string msg = string.Format("No bundle -> {0} found", bundleName);
                KTDebug.LogError(msg);

                /// Trả về NULL
                return null;
            }

            /// Tên Asset
            assetName = string.Format("{0}_{1}", bundleName, assetName);
            /// Nếu trong hệ thống không tồn tại Asset tương ứng
            if (!this.HasAsset(assetName))
            {
                string msg = string.Format("Get SubAsset -> {0} faild -> No corresponding parent Asset {1} found.", subAssetName, assetName);
                KTDebug.LogError(msg);

                /// Trả về NULL
                return null;
            }

            /// Nếu không tồn tại asset con tương ứng
            if (!this.Cache_Resources[assetName].SubAssets.TryGetValue(subAssetName, out UnityEngine.Object asset))
            {
                string msg = string.Format("Get SubAsset -> {0} faild -> No corresponding SubAsset found inside Asset {1}.", subAssetName, assetName);
                KTDebug.LogError(msg);

                return null;
            }

            /// Nếu Asset không tồn tại
            if (asset == null)
            {
                string msg = string.Format("Get SubAsset -> {0} faild -> SubAsset is NULL.", subAssetName);
                KTDebug.LogError(msg);

                return null;
            }
            return asset as T;
        }

        /// <summary>
        /// Tải xuống Asset tương ứng trong Bundle
        /// </summary>
        /// <param name="bundleName"></param>
        /// <param name="assetName"></param>
        /// <param name="removeBundleAfterLoading"></param>
        /// <param name="cacheType"></param>
        /// <returns></returns>
        public bool LoadAsset<T>(string bundleName, string assetName, bool removeBundleAfterLoading, KTResourceCacheType cacheType = KTResourceCacheType.CachedUntilChangeScene) where T : UnityEngine.Object
        {
            /// Nếu Bundle chưa tồn tại trong hệ thống thì bỏ qua
            if (!this.HasBundle(bundleName))
            {
                string msg = string.Format("Load asset {0} from bundle {1} error -> No corresponding Bundle found.", assetName, bundleName);
                KTDebug.LogError(msg);

                /// Thoát
                return false;
            }

            /// Tên đầy đủ của Asset trong hệ thống
            string fullName = string.Format("{0}_{1}", bundleName, assetName);
            /// Nếu trong hệ thống đã tồn tại Asset tương ứng
            if (this.HasAsset(fullName))
            {
                /// Trả về kết quả không cần tải mới
                return false;
            }
            /// Nếu trong hệ thống chưa tồn tại Asset tương ứng
            else
            {
                /// Nếu toác gì đó
                if (this.Cache_AssetBundles[bundleName].Bundle == null)
                {
                    /// Xóa khỏi danh sách theo tên
                    this.WaitToBeLoadedItemsByName.Remove(fullName);
                    /// Toác
                    return false;
                }

                try
                {
                    if (typeof(T) == typeof(Sprite))
                    {
                        Sprite sprite = this.Cache_AssetBundles[bundleName].Bundle.LoadAsset<Sprite>(assetName);
                        /// Thêm vào hệ thống
                        this.AddAsset(fullName, sprite, cacheType);
                    }
                    else if (typeof(T) == typeof(AudioClip))
                    {
                        AudioClip audioClip = this.Cache_AssetBundles[bundleName].Bundle.LoadAsset<AudioClip>(assetName);
                        /// Thêm vào hệ thống
                        this.AddAsset(fullName, audioClip, cacheType);
                    }
                    else
                    {
                        UnityEngine.Object asset = this.Cache_AssetBundles[bundleName].Bundle.LoadAsset(assetName);
                        /// Thêm vào hệ thống
                        this.AddAsset(fullName, asset, cacheType);
                    }
                }
                catch (Exception ex)
                {
                    KTDebug.LogError("Toac => " + bundleName + " - " + assetName);
                    KTDebug.LogException(ex);
                    /// Toác
                    return false;
                }

                /// Nếu tải xong cần xóa Bundle
                if (removeBundleAfterLoading)
                {
                    this.Cache_AssetBundles[bundleName].Bundle.Unload(false);
                    GameObject.Destroy(this.Cache_AssetBundles[bundleName].Bundle);
                    this.Cache_AssetBundles[bundleName].Bundle = null;
                }

                /// OK
                return true;
            }
        }

        /// <summary>
        /// Tải xuống Asset tương ứng trong Bundle
        /// </summary>
        /// <param name="bundleName"></param>
        /// <param name="assetName"></param>
        /// <param name="removeBundleAfterLoading"></param>
        /// <param name="callback"></param>
        /// <param name="cacheType"></param>
        /// <returns></returns>
        public IEnumerator LoadAssetAsync<T>(string bundleName, string assetName, bool removeBundleAfterLoading, Action<bool> callback = null, KTResourceCacheType cacheType = KTResourceCacheType.CachedUntilChangeScene) where T : UnityEngine.Object
        {
            /// Nếu Bundle chưa tồn tại trong hệ thống thì bỏ qua
            if (!this.HasBundle(bundleName))
            {
                string msg = string.Format("Load asset {0} from bundle {1} error -> No corresponding Bundle found.", assetName, bundleName);
                KTDebug.LogError(msg);

                /// Thoát luồng
                yield break;
            }

            /// Tên đầy đủ của Asset trong hệ thống
            string fullName = string.Format("{0}_{1}", bundleName, assetName);
            /// Nếu trong hệ thống đã tồn tại Asset tương ứng
            if (this.HasAsset(fullName))
            {
                /// Thực thi sự kiện tải xuống hoàn tất
                callback?.Invoke(false);
            }
            /// Nếu trong hệ thống chưa tồn tại Asset tương ứng
            else
            {
                /// Nếu đã tồn tại trong danh sách chờ tải xuống
                if (this.WaitToBeLoadedItemsByName.Contains(fullName))
                {
                    /// Đợi tải xuống hoàn tất
                    while (!this.HasAsset(fullName))
                    {
                        yield return null;
                    }

                    /// Thực thi sự kiện tải xuống hoàn tất
                    callback?.Invoke(false);
                }
                /// Nếu chưa tồn tại trong danh sách chờ thì thêm vào danh sách chờ
                else
                {
                    this.WaitToBeLoadedItems.Enqueue(new WaitingItem()
                    {
                        Name = fullName,
                        Type = ItemType.Asset,
                        CacheType = cacheType,
                        ResultType = typeof(T),
                        Params = new List<string>()
                        {
                            bundleName, assetName, removeBundleAfterLoading.ToString()
                        },
                    });
                    this.WaitToBeLoadedItemsByName.Add(fullName);

                    /// Đợi tải xuống hoàn tất
                    while (!this.HasAsset(fullName) && this.WaitToBeLoadedItemsByName.Contains(fullName))
                    {
                        yield return null;
                    }

                    /// Thực thi sự kiện tải xuống hoàn tất
                    callback?.Invoke(true);
                }
            }
        }

        /// <summary>
        /// Tải xuống Asset và các Asset con bên trong
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="bundleName"></param>
        /// <param name="assetName"></param>
        /// <param name="removeBundleAfterLoading"></param>
        /// <param name="cacheType"></param>
        /// <returns></returns>
        public bool LoadAssetWithSubAssets<T>(string bundleName, string assetName, bool removeBundleAfterLoading, KTResourceCacheType cacheType = KTResourceCacheType.CachedUntilChangeScene) where T : UnityEngine.Object
        {
            /// Nếu không tồn tại Bundle tương ứng trong hệ thống
            if (!this.HasBundle(bundleName))
            {
                string msg = string.Format("Load asset {0} from bundle {1} error -> No corresponding Bundle found.", assetName, bundleName);
                KTDebug.LogError(msg);

                /// Thoát
                return false;
            }

            /// Tên đầy đủ của Asset
            string fullName = string.Format("{0}_{1}", bundleName, assetName);
            /// Nếu trong hệ thống đã tồn tại Asset tương ứng
            if (this.HasAsset(fullName))
            {
                /// Trả về kết quả không cần tải mới
                return false;
            }
            /// Nếu trong hệ thống chưa tồn tại Asset tương ứng
            else
            {
                /// Nếu toác gì đó
                if (this.Cache_AssetBundles[bundleName].Bundle == null)
                {
                    /// Xóa khỏi danh sách theo tên
                    this.WaitToBeLoadedItemsByName.Remove(fullName);
                    /// Toác
                    return false;
                }

                //KTDebug.LogError("Begin load subassets of Bundle = " + bundleName + " - Asset = " + assetName);

                try
                {
                    if (typeof(T) == typeof(Sprite))
                    {
                        Sprite[] sprites = this.Cache_AssetBundles[bundleName].Bundle.LoadAssetWithSubAssets<Sprite>(assetName);
                        /// Toác
                        if (sprites == null || sprites.Length <= 0)
                        {
                            KTDebug.LogError(string.Format("Can not find SubAssets, Bundle = {0}, Asset = {1}, Type = {2}, FullName = {3}", bundleName, assetName, typeof(T), fullName));
                            /// Toác
                            return false;
                        }
                        /// Thêm vào hệ thống
                        this.AddAsset(fullName, sprites[0], cacheType);

                        /// Duyệt toàn bộ danh sách Asset con tìm thấy
                        for (int i = 0; i < sprites.Length; i++)
                        {
                            /// Thêm Asset con vào danh sách
                            this.Cache_Resources[fullName].SubAssets[sprites[i].name] = sprites[i];
                        }
                    }
                    else if (typeof(T) == typeof(AudioClip))
                    {
                        AudioClip[] audioClips = this.Cache_AssetBundles[bundleName].Bundle.LoadAssetWithSubAssets<AudioClip>(assetName);
                        /// Toác
                        if (audioClips == null || audioClips.Length <= 0)
                        {
                            KTDebug.LogError(string.Format("Can not find SubAssets, Bundle = {0}, Asset = {1}, Type = {2}, FullName = {3}", bundleName, assetName, typeof(T), fullName));
                            /// Toác
                            return false;
                        }
                        /// Thêm vào hệ thống
                        this.AddAsset(fullName, audioClips[0], cacheType);

                        /// Duyệt toàn bộ danh sách Asset con tìm thấy
                        for (int i = 0; i < audioClips.Length; i++)
                        {
                            /// Thêm Asset con vào danh sách
                            this.Cache_Resources[fullName].SubAssets[audioClips[i].name] = audioClips[i];
                        }
                    }
                    else
                    {
                        UnityEngine.Object[] assets = this.Cache_AssetBundles[bundleName].Bundle.LoadAssetWithSubAssets(assetName);
                        /// Toác
                        if (assets == null || assets.Length <= 0)
                        {
                            KTDebug.LogError(string.Format("Can not find SubAssets, Bundle = {0}, Asset = {1}, Type = {2}, FullName = {3}", bundleName, assetName, typeof(T), fullName));
                            /// Toác
                            return false;
                        }
                        /// Thêm vào hệ thống
                        this.AddAsset(fullName, assets[0], cacheType);

                        /// Duyệt toàn bộ danh sách Asset con tìm thấy
                        for (int i = 0; i < assets.Length; i++)
                        {
                            /// Thêm Asset con vào danh sách
                            this.Cache_Resources[fullName].SubAssets[assets[i].name] = assets[i];
                        }
                    }
                }
                catch (Exception ex)
                {
                    KTDebug.LogError("Toac => " + bundleName + " - " + assetName);
                    KTDebug.LogException(ex);
                }

                /// Nếu tải xong cần xóa Bundle
                if (removeBundleAfterLoading)
                {
                    this.Cache_AssetBundles[bundleName].Bundle.Unload(false);
                    GameObject.Destroy(this.Cache_AssetBundles[bundleName].Bundle);
                    this.Cache_AssetBundles[bundleName].Bundle = null;
                }

                /// OK
                return true;
            }
        }

        /// <summary>
        /// Tải xuống Asset và các Asset con bên trong
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="bundleName"></param>
        /// <param name="assetName"></param>
        /// <param name="removeBundleAfterLoading"></param>
        /// <param name="callback"></param>
        /// <param name="cacheType"></param>
        /// <returns></returns>
        public IEnumerator LoadAssetWithSubAssetsAsync<T>(string bundleName, string assetName, bool removeBundleAfterLoading, Action<bool> callback = null, KTResourceCacheType cacheType = KTResourceCacheType.CachedUntilChangeScene) where T : UnityEngine.Object
        {
            /// Nếu không tồn tại Bundle tương ứng trong hệ thống
            if (!this.HasBundle(bundleName))
            {
                string msg = string.Format("Load asset {0} from bundle {1} error -> No corresponding Bundle found.", assetName, bundleName);
                KTDebug.LogError(msg);

                /// Thoát luồng
                yield break;
            }

            /// Tên đầy đủ của Asset
            string fullName = string.Format("{0}_{1}", bundleName, assetName);
            /// Nếu trong hệ thống đã tồn tại Asset tương ứng
            if (this.HasAsset(fullName))
            {
                /// Thực thi sự kiện tải xuống hoàn tất
                callback?.Invoke(false);
            }
            /// Nếu trong hệ thống chưa tồn tại Asset tương ứng
            else
            {
                /// Nếu đã tồn tại trong danh sách chờ tải xuống
                if (this.WaitToBeLoadedItemsByName.Contains(fullName))
                {
                    /// Đợi tải xuống hoàn tất
                    while (!this.HasAsset(fullName))
                    {
                        yield return null;
                    }

                    /// Thực thi sự kiện tải xuống hoàn tất
                    callback?.Invoke(false);
                }
                /// Nếu chưa tồn tại trong danh sách chờ thì thêm vào danh sách chờ
                else
                {
                    this.WaitToBeLoadedItems.Enqueue(new WaitingItem()
                    {
                        Name = fullName,
                        Type = ItemType.SubAssets,
                        CacheType = cacheType,
                        ResultType = typeof(T),
                        Params = new List<string>()
                        {
                            bundleName, assetName, removeBundleAfterLoading.ToString()
                        },
                    });
                    this.WaitToBeLoadedItemsByName.Add(fullName);

                    /// Đợi tải xuống hoàn tất
                    while (!this.HasAsset(fullName) && this.WaitToBeLoadedItemsByName.Contains(fullName))
                    {
                        yield return null;
                    }

                    /// Thực thi sự kiện tải xuống hoàn tất
                    callback?.Invoke(true);
                }
            }
        }

        /// <summary>
        /// Trả về tổng số Asset con có trong Asset tương ứng
        /// </summary>
        /// <param name="bundleName"></param>
        /// <param name="assetName"></param>
        /// <returns></returns>
        public int GetTotalSubAssets(string bundleName, string assetName)
        {
            /// Nếu trong hệ thống không tồn tại Bundle tương ứng
            if (!this.HasBundle(bundleName))
            {
                string msg = string.Format("No bundle -> {0} found", bundleName);
                KTDebug.LogError(msg);

                /// Trả về NULL
                return 0;
            }

            /// Tên Asset
            assetName = string.Format("{0}_{1}", bundleName, assetName);
            /// Nếu trong hệ thống không tồn tại Asset tương ứng
            if (!this.HasAsset(assetName))
            {
                string msg = string.Format("Get Total SubAssets faild -> No corresponding parent Asset {0} found.", assetName);
                KTDebug.LogError(msg);

                /// Trả về NULL
                return 0;
            }

            /// Trả về kết quả
            return this.Cache_Resources[assetName].SubAssets.Count;
        }

        /// <summary>
        /// Trả về danh sách các Asset con tương ứng được Cache trong hệ thống
        /// </summary>
        /// <param name="bundleName"></param>
        /// <param name="assetName"></param>
        /// <returns></returns>
        public Dictionary<string, UnityEngine.Object> GetSubAssets(string bundleName, string assetName)
        {
            /// Nếu trong hệ thống không tồn tại Bundle tương ứng
            if (!this.HasBundle(bundleName))
            {
                string msg = string.Format("No bundle -> {0} found", bundleName);
                KTDebug.LogError(msg);

                /// Trả về NULL
                return null;
            }

            /// Tên Asset
            assetName = string.Format("{0}_{1}", bundleName, assetName);
            /// Nếu trong hệ thống không tồn tại Asset tương ứng
            if (!this.HasAsset(assetName))
            {
                string msg = string.Format("Get SubAssets faild -> No corresponding parent Asset {0} found.", assetName);
                KTDebug.LogError(msg);

                /// Trả về NULL
                return null;
            }

            /// Trả về kết quả
            return this.Cache_Resources[assetName].SubAssets;
        }
        #endregion
        #endregion

        #region Garbage Collector
        /// <summary>
        /// Hàm này gọi đến khi thay đổi bản đồ
        /// </summary>
        public void OnSceneChanged()
        {
            /// Xóa Asset Bundle
            {
                List<string> keys = this.Cache_AssetBundles.Keys.ToList();
                foreach (string key in keys)
                {
                    KTAssetBundle bundle = this.Cache_AssetBundles[key];
                    if (bundle.CacheType == KTResourceCacheType.CachedPermenently)
                    {
                        continue;
                    }

                    /// Nếu AssetBundle NULL
                    if (bundle.Bundle != null)
                    {
                        bundle.Bundle.Unload(true);
                        GameObject.DestroyImmediate(bundle.Bundle, true);
                    }
                    this.Cache_AssetBundles.Remove(key);

                    //string debugText = string.Format("Destroy Bundle {0} -> Scene changed", bundle.Name);
                    //KTDebug.LogError(debugText);
                    //KTGlobal.WriteDebugConsoleLog(KTGlobal.DebugConsoleLogType.Info, debugText);

                    bundle.Dispose();
                }
            }

            /// Xóa Res
            {
                List<string> keys = this.Cache_Resources.Keys.ToList();
                foreach (string key in keys)
                {
                    KTResource resource = this.Cache_Resources[key];
                    if (resource.CacheType == KTResourceCacheType.CachedPermenently)
                    {
                        continue;
                    }

                    if (resource.Asset != null)
                    {
                        if (resource.Asset is Sprite)
                        {
                            Sprite sprite = resource.Asset as Sprite;
                            Resources.UnloadAsset(sprite.texture);
                            GameObject.DestroyImmediate(sprite.texture, true);
                            GameObject.DestroyImmediate(sprite, true);
                        }
                        else if (resource.Asset is AudioClip)
                        {
                            AudioClip audioClip = resource.Asset as AudioClip;
                            Resources.UnloadAsset(audioClip);
                            GameObject.DestroyImmediate(audioClip, true);
                        }

                        //string debugText = string.Format("Destroy Asset {0} -> Scene changed", resource.Name);
                        //KTDebug.LogError(debugText);
                        //KTGlobal.WriteDebugConsoleLog(KTGlobal.DebugConsoleLogType.Info, debugText);

                        resource.Dispose();
                    }

                    this.Cache_Resources.Remove(key);
                }
            }

            /// Xóa danh sách chờ
            this.WaitToBeLoadedItems.Clear();
            this.WaitToBeLoadedItemsByName.Clear();
            this.WaitToBeRunInMainThread.Clear();

            /// Xóa toàn bộ các đối tượng trong Pool vẫn đang dùng
            KTObjectPoolManager.Instance.ClearPool();

            /// Xóa các Asset không dùng
            Resources.UnloadUnusedAssets();
        }

        /// <summary>
        /// Luồng kiểm tra AssetBundle
        /// </summary>
        /// <returns></returns>
        private IEnumerator TickTimeAssetBundle()
        {
            while (true)
            {
                yield return new WaitForSeconds(this.GCTick);

                /// Tổng số đã xử lý
                int totalProcessed = 0;

                List<string> keys = this.Cache_AssetBundles.Keys.ToList();
                foreach (string key in keys)
                {
                    totalProcessed++;
                    if (totalProcessed > this.MaxGCProcessPerFrame)
                    {
                        totalProcessed = 0;
                        yield return null;
                    }

                    if (string.IsNullOrEmpty(key))
                    {
                        continue;
                    }

                    if (!this.Cache_AssetBundles.TryGetValue(key, out KTAssetBundle bundle))
                    {
                        continue;
                    }

                    if (bundle.CacheType == KTResourceCacheType.CachedPermenently || bundle.CacheType == KTResourceCacheType.CachedUntilChangeScene || bundle.ReferenceCount > 0)
                    {
                        continue;
                    }

                    /// Nếu chưa tải xong thì thôi
                    if (!bundle.DoneLoading)
                    {
                        continue;
                    }

                    bundle.UnloadTick -= this.GCTick;
                    if (bundle.UnloadTick <= 0)
                    {
                        //KTDebug.LogError("Unload Bundle => " + bundle.Name + "\n" + new System.Diagnostics.StackTrace().ToString());
                        bundle.Bundle.Unload(true);
                        GameObject.Destroy(bundle.Bundle);
                        this.Cache_AssetBundles.Remove(key);

                        /// Danh sách Asset chứa bên trong
                        List<string> assetKeys = this.Cache_Resources.Keys.Where(x => x.Contains(string.Format("{0}_", key))).ToList();
                        foreach (string assetKey in assetKeys)
                        {
                            /// Xóa Asset tương ứng
                            Resources.UnloadAsset(this.Cache_Resources[assetKey].Asset);
                            GameObject.Destroy(this.Cache_Resources[assetKey].Asset);
                            this.Cache_Resources[assetKey].SubAssets.Clear();
                            this.Cache_Resources.Remove(assetKey);
                        }

                        //string debugText = string.Format("Destroy Bundle {0} -> Unused for {1}s", bundle.Name, this.BundleKeepTick);
                        //KTDebug.LogError(debugText);
                    }
                }
            }
        }
        #endregion
    }
}
