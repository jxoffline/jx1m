using FS.GameEngine.Logic;
using FS.VLTK.Entities;
using FS.VLTK.Loader;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace FS.VLTK.Control.Component
{
    /// <summary>
    /// Quản lý tài nguyên
    /// </summary>
    public partial class Map
    {
        #region Private methods
        /// <summary>
        /// Bắt đầu đọc dữ liệu bản đồ
        /// </summary>
        /// <param name="mapData"></param>
        private void BeginReadMapData(Entities.Config.Map mapData)
        {
            this.Name = mapData.Name;
            this.ReportProgress?.Invoke(0);
            this.StartCoroutine(this.Load2DMap());
        }

        /// <summary>
        /// Thực hiện Logic theo dõi vị trí Leader
        /// </summary>
        /// <param name="leaderPos"></param>
        /// <param name="onPartDone"></param>
        /// <param name="asyncLoad"></param>
        private void DoFollowLeaderLogic(Vector2 leaderPos, Action<int> onPartDone, bool asyncLoad = false)
        {
            int partWidth = FS.VLTK.Loader.Loader.Maps[this.MapCode].PartWidth;
            int partHeight = FS.VLTK.Loader.Loader.Maps[this.MapCode].PartHeight;
            int totalHorizontal = FS.VLTK.Loader.Loader.Maps[this.MapCode].HorizontalCount;
            int totalVertical = FS.VLTK.Loader.Loader.Maps[this.MapCode].VerticalCount;
            int totalLayers = totalHorizontal * totalVertical;

            /// <summary>
            /// Trả về ID layer ảnh tại vị trí tương ứng
            /// </summary>
            int GetLayerID(int gridX, int gridY)
            {
                /// Toác
                if (gridX < 0 || gridX >= totalHorizontal || gridY < 0 || gridY >= totalVertical)
                {
                    return -1;
                }
                return gridY * totalHorizontal + gridX + 1;
            }

            /// <summary>
            /// Trả về thông tin layer ảnh tại vị trí tương ứng
            /// </summary>
            Tuple<int, int, int> GetLayerInfo(Vector2 position)
            {
                int gridX = (int) position.x / partWidth;
                int gridY = (int) position.y / partHeight;
                int layerID = GetLayerID(gridX, gridY);
                /// Trả về kết quả
                return new Tuple<int, int, int>(gridX, gridY, layerID);
            }

            /// Thứ tự Layer hiện tại của Leader
            int currentLayerID = GetLayerInfo(leaderPos).Item3;
            /// Nếu thứ tự Layer giống nhau thì thôi
            if (this.LastLeaderLayerID == currentLayerID)
            {
                /// Toác
                return;
            }

            /// Cập nhật thứ tự Layer của Leader
            this.LastLeaderLayerID = currentLayerID;

            /// Tọa độ góc trái dưới
            Vector2 bottomLeftPos = new Vector2(leaderPos.x - partWidth * (this.DynamicViewCellSize.x - 1) / 2, leaderPos.y - partHeight * (this.DynamicViewCellSize.y - 1) / 2);
            /// Thông tin Layer góc trái dưới
            Tuple<int, int, int> bottomLeftLayerInfo = GetLayerInfo(bottomLeftPos);

            /// ID Part
            int partID = -1;
            /// Duyệt danh sách theo chiều ngang
            for (int gridX = bottomLeftLayerInfo.Item1; gridX < bottomLeftLayerInfo.Item1 + this.DynamicViewCellSize.x; gridX++)
            {
                /// Duyệt danh sách theo chiều dọc
                for (int gridY = bottomLeftLayerInfo.Item2; gridY < bottomLeftLayerInfo.Item2 + this.DynamicViewCellSize.y; gridY++)
                {
                    /// Tăng ID part
                    partID++;
                    /// ID layer hiện tại
                    int layerID = GetLayerID(gridX, gridY);
                    /// Hủy ảnh trước
                    this.ListRenderers[partID].sprite = null;
                    this.ListRenderers[partID].transform.localPosition = new Vector2(gridX * partWidth, gridY * partHeight);
                    /// Nếu nằm ngoài phạm vi
                    if (layerID < 1 || layerID > totalLayers)
                    {
                        /// Thực thi sự kiện tải xuống thành phần hoàn tất
                        onPartDone?.Invoke(layerID);
                        continue;
                    }

                    /// Ảnh tương ứng nếu tồn tại
                    if (this.ListMapSprites.TryGetValue(layerID, out Sprite sprite))
                    {
                        this.ListRenderers[partID].sprite = sprite;
                        this.ListRenderers[partID].drawMode = SpriteDrawMode.Sliced;
                        this.ListRenderers[partID].size = new Vector2(partWidth, partHeight);
                    }
                    /// Nếu chưa tồn tại
                    else
                    {
                        /// Nếu tải xuống theo phương thức Async
                        if (!asyncLoad)
                        {
                            /// Tải xuống
                            this.StartCoroutine(this.LoadMapSpriteAsync(layerID, partID, (partID, sprite) => {
                                this.ListRenderers[partID].sprite = sprite;
                                this.ListRenderers[partID].drawMode = SpriteDrawMode.Sliced;
                                this.ListRenderers[partID].size = new Vector2(partWidth, partHeight);

                                /// Thực thi sự kiện tải xuống thành phần hoàn tất
                                onPartDone?.Invoke(layerID);
                            }));
                        }
                        else
                        {
                            /// Tải xuống
                            sprite = this.LoadMapSprite(layerID);
                            this.ListRenderers[partID].sprite = sprite;
                            this.ListRenderers[partID].drawMode = SpriteDrawMode.Sliced;
                            this.ListRenderers[partID].size = new Vector2(partWidth, partHeight);
                            /// Thực thi sự kiện tải xuống thành phần hoàn tất
                            onPartDone?.Invoke(layerID);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Thực hiện theo dõi Leader cập nhật vị trí
        /// </summary>
        /// <returns></returns>
        private IEnumerator FollowLeader()
        {
            /// Tạo mới đối tượng nghỉ 1s
            WaitForSeconds wait = new WaitForSeconds(1f);

            /// Lặp liên tục
            while (true)
            {
                /// Nếu chưa vào Game
                if (Global.Data.Leader == null)
                {
                    /// Nghỉ 1s
                    yield return wait;
                    /// Tiếp tục
                    continue;
                }

                /// Thực thi Logic
                this.DoFollowLeaderLogic(Global.Data.Leader.PositionInVector2, null);

                /// Nghỉ 1s
                yield return wait;
            }
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Đọc dữ liệu tài nguyên bản đồ
        /// </summary>
        public void Load()
        {
            this.Name = "";
            if (Loader.Loader.Maps.TryGetValue(this.MapCode, out VLTK.Entities.Config.Map mapData))
            {
                /// Tên Res bản đồ cần tải
                string mapResFolderName = Regex.Match(mapData.ImageFolder, @"Resources\/Map\/(\w+)\/Image")?.Groups[1]?.Value;
                /// Thông tin trong File chờ Update sau
                UpdateZipFile updateLaterInfo = MainGame.ListUpdateLaterFiles.ZipFiles.Where(x => x.FileName == string.Format("Data/Resources/Map/{0}.zip", mapResFolderName)).FirstOrDefault();
                /// Nếu bản đồ chưa tồn tại hoặc khác MD5 cần Update
                if (updateLaterInfo != null || !KTResourceChecker.IsMapResExist(this.MapCode))
                {
                    /// Reset tổng số lượt đã thử
                    this.totalTriedDownloadTimes = 0;
                    /// Bắt đầu tải
                    this.StartCoroutine(this.StartDownload(mapData, () => {
                        /// Đọc dữ liệu bản đồ
                        this.BeginReadMapData(mapData);
                    }));
                }
                /// Nếu bản đồ đã tồn tại
                else
                {
                    /// Đọc dữ liệu bản đồ
                    this.BeginReadMapData(mapData);
                }
            }
            else
            {
                KTGlobal.ShowMessageBox("Lỗi nghiêm trọng", string.Format("Không thể tải bản đồ ID = {0}. Hãy thoát Game, thử lại hoặc liên hệ với hỗ trợ để được giúp đỡ!", this.MapCode), () => {
                    Application.Quit();
                }, false);
            }
        }
        #endregion
    }
}
