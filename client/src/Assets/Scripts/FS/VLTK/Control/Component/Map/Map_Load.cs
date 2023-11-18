using FS.GameEngine.Logic;
using FS.GameFramework.Logic;
using FS.VLTK.Utilities.UnityComponent;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace FS.VLTK.Control.Component
{
    /// <summary>
    /// Đối tượng bản đồ
    /// </summary>
    public partial class Map
    {
        /// <summary>
        /// Tải ảnh map có Layer tương ứng
        /// </summary>
        /// <param name="layerID"></param>
        /// <returns></returns>
        private Sprite LoadMapSprite(int layerID)
        {
            string url = Global.WebPath(string.Format("Data/{0}/{1}.unity3d", FS.VLTK.Loader.Loader.Maps[this.MapCode].ImageFolder, layerID));
#if UNITY_IOS || UNITY_EDITOR
            if (url.Contains("file:///"))
            {
                url = url.Replace("file:///", "");
            }
#endif
            /// Bundle tương ứng
            AssetBundle mapImageBundle = AssetBundle.LoadFromFile(url);

            /// Nếu tồn tại
            if (mapImageBundle != null)
            {
                Sprite sprite = mapImageBundle.LoadAssetWithSubAssets<Sprite>(layerID.ToString())[0];

                /// Thêm vào danh sách đã tải
                this.ListMapSprites[layerID] = sprite;

                /// Xóa Bundle tương ứng
                mapImageBundle.Unload(false);
                GameObject.Destroy(mapImageBundle);

                /// Trả về kết quả
                return sprite;
            }
            /// Toác
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Tải ảnh map có Layer tương ứng
        /// </summary>
        /// <param name="layerID"></param>
        /// <param name="partID"></param>
        /// <param name="done"></param>
        /// <returns></returns>
        private IEnumerator LoadMapSpriteAsync(int layerID, int partID, Action<int, Sprite> done)
        {
            string url = Global.WebPath(string.Format("Data/{0}/{1}.unity3d", FS.VLTK.Loader.Loader.Maps[this.MapCode].ImageFolder, layerID));
#if UNITY_IOS || UNITY_EDITOR
            if (url.Contains("file:///"))
            {
                url = url.Replace("file:///", "");
            }
#endif
            UnityWebRequest request = UnityWebRequestAssetBundle.GetAssetBundle(url);
            yield return request.SendWebRequest();

            /// Bundle tương ứng
            AssetBundle mapImageBundle = DownloadHandlerAssetBundle.GetContent(request);
            /// Giải phóng bộ nhớ
            request.downloadHandler.Dispose();
            request.Dispose();

            /// Nếu tồn tại
            if (mapImageBundle != null)
            {
                AssetBundleRequest assetRequest = mapImageBundle.LoadAssetWithSubAssetsAsync<Sprite>(layerID.ToString());
                yield return assetRequest;
                Sprite sprite = assetRequest.allAssets[0] as Sprite;

                /// Thêm vào danh sách đã tải
                this.ListMapSprites[layerID] = sprite;

                /// Xóa Bundle tương ứng
                mapImageBundle.Unload(false);
                GameObject.Destroy(mapImageBundle);

                /// Thực hiện Callback khi hoàn tất
                done?.Invoke(partID, sprite);
            }
            /// Toác
            else
            {
                done?.Invoke(partID, null);
            }
        }

        /// <summary>
        /// Tải tài nguyên bản đồ 2D từ Prefab
        /// </summary>
        private IEnumerator Load2DMap()
        {
            /// Thông báo đang tải xuống cái gì
            this.UpdateProgressText?.Invoke(string.Format("Chuyển đến: {0}", FS.VLTK.Loader.Loader.Maps[this.MapCode].Name));

            #region Map values
            int mapWidth = FS.VLTK.Loader.Loader.Maps[this.MapCode].Width;
            int mapHeight = FS.VLTK.Loader.Loader.Maps[this.MapCode].Height;
            int partWidth = FS.VLTK.Loader.Loader.Maps[this.MapCode].PartWidth;
            int partHeight = FS.VLTK.Loader.Loader.Maps[this.MapCode].PartHeight;
            int totalHorizontal = FS.VLTK.Loader.Loader.Maps[this.MapCode].HorizontalCount;
            int totalVertical = FS.VLTK.Loader.Loader.Maps[this.MapCode].VerticalCount;
            string folderDir = FS.VLTK.Loader.Loader.Maps[this.MapCode].ImageFolder;
            int totalParts = totalHorizontal * totalVertical;

            this.ListRenderers = new List<SpriteRenderer>();
            this.ListMapSprites = new Dictionary<int, Sprite>();
            this.name = FS.VLTK.Loader.Loader.Maps[this.MapCode].Name;

            this.gameObject.AddComponent<AudioSource>();
            this.MapMusic = this.gameObject.AddComponent<AudioPlayer>();
            #endregion

            #region Create scene gameobject
            this.ReportProgress?.Invoke(1);

            /// Thiết lập Layer
            this.gameObject.layer = 8;

            /// Nếu tải xuống Full
            if (Map.EnableLoadFullMap)
            {
                /// Vị trí gốc
                this.gameObject.transform.localPosition = Vector2.zero;

                int layerID = 0;
                /// Duyệt danh sách chiều dọc
                for (int j = 0; j < totalVertical; j++)
                {
                    /// Duyệt danh sách chiều ngang
                    for (int i = 0; i < totalHorizontal; i++)
                    {
                        layerID++;
                        GameObject go = new GameObject(string.Format("Map image - {0}", layerID));
                        go.transform.SetParent(this.gameObject.transform, false);
                        go.layer = this.gameObject.layer;
                        go.transform.localPosition = new Vector2(i * partWidth, j * partHeight);
                        SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
                        renderer.sprite = null;
                        this.ListRenderers.Add(renderer);
                    }
                }
            }
            /// Nếu chỉ tải xuống phần cần thiết
            else
            {
                /// Vị trí gốc
                this.gameObject.transform.localPosition = Vector2.zero;

                int layerID = 0;
                /// Duyệt danh sách chiều ngang
                for (int partX = 0; partX < this.DynamicViewCellSize.x; partX++)
                {
                    /// Duyệt danh sách chiều dọc
                    for (int partY = 0; partY < this.DynamicViewCellSize.y; partY++)
                    {
                        layerID++;
                        GameObject go = new GameObject(string.Format("Map dynamic image - {0}", layerID));
                        go.transform.SetParent(this.gameObject.transform, false);
                        go.layer = this.gameObject.layer;
                        SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
                        renderer.sprite = null;
                        this.ListRenderers.Add(renderer);
                    }
                }
            }
            yield return null;
            #endregion

            #region Load music
            this.ReportProgress?.Invoke(10);

            {
                string url = Global.WebPath(string.Format("Data/{0}", FS.VLTK.Loader.Loader.Maps[this.MapCode].MusicBundle));
#if UNITY_IOS || UNITY_EDITOR
                if (url.Contains("file:///"))
                {
                    url = url.Replace("file:///", "");
                }
#endif
                AssetBundle musicBundle = AssetBundle.LoadFromFile(url);

                if (musicBundle != null)
                {

                    this.MapMusic.ActivateAfter = 0f;
                    this.MapMusic.IsRepeat = true;
                    this.MapMusic.RepeatTimer = 10f;

                    AudioClip sound = musicBundle.LoadAsset<AudioClip>("Music");
                    this.MapMusic.Sound = sound;

                    /// Hủy bundle tương ứng
                    musicBundle.Unload(false);
                    GameObject.Destroy(musicBundle);

                    // KTDebug.LogError("Load asset bundle -> " + FS.VLTK.Loader.Loader.Maps[this.MapCode].MusicBundle + " - SUCCESS");
                }
                else
                {
                    KTDebug.LogError("Load asset bundle -> " + FS.VLTK.Loader.Loader.Maps[this.MapCode].MusicBundle + " - FAILD");
                }
            }
            #endregion

            if (true)
            {
                #region Tải các thành phần
                this.ReportProgress?.Invoke(20);

                /// Nếu thiết lập tải Full
                if (Map.EnableLoadFullMap)
                {
                    /// Tổng số phần đã tải xong
                    int totalLoadedPart = 0;
                    /// Tổng số ảnh cần tải
                    int totalRequireParts = totalHorizontal * totalVertical;

                    /// Duyệt tổng số ảnh chiều ngang
                    for (int i = 0; i < totalHorizontal; i++)
                    {
                        /// Duyệt tổng số ảnh chiều dọc
                        for (int j = 0; j < totalVertical; j++)
                        {
                            /// ID ảnh
                            int layerID = totalLoadedPart + 1;
                            //KTDebug.LogError(layerID.ToString());
                            /// Tải xuống
                            this.ListRenderers[layerID - 1].sprite = this.LoadMapSprite(layerID);
                            this.ListRenderers[layerID - 1].drawMode = SpriteDrawMode.Sliced;
                            this.ListRenderers[layerID - 1].size = new Vector2(partWidth, partHeight);

                            /// Tăng tổng số đã tải xuống
                            totalLoadedPart++;
                        }

                        if (i % 5 == 0)
                        {
                            /// % đã tải
                            int loadedPercent = (int) (totalLoadedPart / (float) totalRequireParts * (95 - 20)) + 20;
                            this.ReportProgress?.Invoke(loadedPercent);

                            /// Bỏ qua 1 Frame
                            yield return null;
                        }
                    }
                }
                /// Nếu tải phần xung quanh nhân vật
                else
                {
                    /// Tổng số phần đã tải xong
                    int totalLoadedPart = 0;
                    int totalRequireParts = this.DynamicViewCellSize.x * this.DynamicViewCellSize.y;

                    /// Thực hiện tải
                    this.DoFollowLeaderLogic(this.LeaderPosition, (layerID) =>
                    {
                        /// Tăng số phần đã tải
                        totalLoadedPart++;
                        /// % đã tải
                        int loadedPercent = (int) (totalLoadedPart / (float) totalRequireParts * (95 - 20)) + 20;
                        this.ReportProgress?.Invoke(loadedPercent);
                    });

                    /// Chừng nào chưa tải xong
                    while (totalLoadedPart < totalRequireParts)
                    {
                        /// Đợi
                        yield return null;
                    }
                }
                #endregion
            }

            #region Load minimap
            this.ReportProgress?.Invoke(95);

            /// Nếu bản đồ hiện tại có hiện bản đồ nhỏ ở góc
            if (Loader.Loader.Maps[this.MapCode].ShowMiniMap)
            {
                string url = Global.WebPath(string.Format("Data/{0}/{1}.unity3d", FS.VLTK.Loader.Loader.Maps[this.MapCode].ImageFolder, FS.VLTK.Loader.Loader.Maps[this.MapCode].MinimapName));
#if UNITY_IOS || UNITY_EDITOR
                if (url.Contains("file:///"))
                {
                    url = url.Replace("file:///", "");
                }
#endif
                AssetBundle mapImageBundle = AssetBundle.LoadFromFile(url);

                /// Nếu tồn tại
                if (mapImageBundle != null)
                {
                    Sprite sprite = mapImageBundle.LoadAssetWithSubAssets<Sprite>(FS.VLTK.Loader.Loader.Maps[this.MapCode].MinimapName)[0];
                    this.LocalMapSprite = sprite;

                    /// Hủy Bundle tương ứng
                    mapImageBundle.Unload(false);
                    GameObject.Destroy(mapImageBundle);

                    /// Tạo mới bản đồ thu nhỏ
                    GameObject radarMapObj = new GameObject("RadarMap");
                    radarMapObj.transform.SetParent(this.gameObject.transform, false);
                    radarMapObj.transform.localPosition = Vector2.zero;
                    radarMapObj.layer = 10;
                    /// Tạo Renderer
                    SpriteRenderer radarMapRenderer = radarMapObj.AddComponent<SpriteRenderer>();
                    /// Thiết lập ảnh bản đồ thu nhỏ
                    radarMapRenderer.sprite = sprite;
                    radarMapRenderer.drawMode = SpriteDrawMode.Sliced;
                    radarMapRenderer.size = sprite.rect.size;
                }
            }
            else
            {
                this.LocalMapSprite = null;
            }
            #endregion

            #region Hoàn tất
            /// Phát nhạc nền map
            try
            {
                this.MapMusic.Play();
            }
            catch (Exception ex)
            {
                KTGlobal.ShowMessageBox("Lỗi phát sinh", "Lỗi phát sinh khi phát nhạc bản đồ: " + ex.Message + ". Hãy báo lại với hỗ trợ, sau đó ấn OK để bỏ qua", true);
            }

            this.ReportProgress?.Invoke(99);
            yield return new WaitForSeconds(0.1f);

            /// Hủy màn tải bản đồ
            try
            {
                Super.DestroyLoadingMap();
            }
            catch (Exception ex)
            {
                KTGlobal.ShowMessageBox("Lỗi phát sinh", "Lỗi phát sinh khi khởi tạo bản đồ: " + ex.Message + ". Hãy báo lại với hỗ trợ, sau đó ấn OK để bỏ qua", true);
            }

            try
            {
                /// Thực thi sự kiện hoàn tất
                this.Finish?.Invoke();
            }
            catch (Exception ex)
            {
                KTGlobal.ShowMessageBox("Lỗi phát sinh", "Lỗi phát sinh khi tải tài nguyên bản đồ: " + ex.Message + ". Hãy báo lại với hỗ trợ, sau đó ấn OK để bỏ qua", true);
            }

            this.ReportProgress?.Invoke(100);
            yield return new WaitForSeconds(0.1f);
            #endregion
        }
    }
}
