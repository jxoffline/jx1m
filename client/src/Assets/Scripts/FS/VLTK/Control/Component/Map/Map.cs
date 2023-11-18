using FS.GameEngine.Logic;
using FS.GameFramework.Logic;
using FS.VLTK.Loader;
using FS.VLTK.Logic.Settings;
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
    public partial class Map : TTMonoBehaviour
    {
        #region Properties
        /// <summary>
        /// ID bản đồ cần tải
        /// </summary>
        public int MapCode { get; set; }

        /// <summary>
        /// Tọa độ hiện tại của Leader
        /// </summary>
        public Vector2 LeaderPosition { get; set; }

        /// <summary>
        /// Gọi tới khi quá trình hoàn tất
        /// </summary>
        public Action Finish { get; set; }

        /// <summary>
        /// Báo cáo tiến trình tải
        /// </summary>
        public Action<int> ReportProgress { get; set; }

        /// <summary>
        /// Báo cáo nội dung tiến trình tải xuống
        /// </summary>
        public Action<string> UpdateProgressText { get; set; }

        /// <summary>
        /// Ảnh bản đồ khu vực
        /// </summary>
        public UnityEngine.Sprite LocalMapSprite { get; private set; }

        /// <summary>
        /// Tên bản đồ
        /// </summary>
        public string Name { get; private set; }
        #endregion

        #region Constants
        /// <summary>
        /// Tải xuống toàn bộ bản đồ luôn không
        /// </summary>
        private const bool EnableLoadFullMap = true;
        #endregion

        #region Private fields
        /// <summary>
        /// Nhạc Map
        /// </summary>
        private AudioPlayer MapMusic;

        /// <summary>
        /// Danh sách ảnh tạo nên bản đồ
        /// </summary>
        private Dictionary<int, Sprite> ListMapSprites;

        /// <summary>
        /// Danh sách SpriteRenderer
        /// </summary>
        private List<SpriteRenderer> ListRenderers;

        /// <summary>
        /// Kích thước tầm nhìn (số ảnh chiều ngang, dọc)
        /// </summary>
        private readonly Vector2Int DynamicViewCellSize = new Vector2Int(10, 5);

        /// <summary>
        /// Thứ tự Layer trước đó của Leader
        /// </summary>
        private int LastLeaderLayerID;
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi ở Frame đầu tiên
        /// </summary>
        private void Start()
        {
            this.StartCoroutine(this.ExecuteBackgroundWork());
            /// Nếu không tải Full bản đồ
            if (!Map.EnableLoadFullMap)
            {
                this.StartCoroutine(this.FollowLeader());
            }
        }

        /// <summary>
        /// Hàm này gọi đến khi đối tượng bị xóa
        /// </summary>
        private void OnDestroy()
        {
            if (this.ListMapSprites != null)
            {
                foreach (Sprite sprite in this.ListMapSprites.Values)
                {
                    if (sprite == null)
                    {
                        continue;
                    }
                    Resources.UnloadAsset(sprite.texture);
                    GameObject.DestroyImmediate(sprite.texture, true);
                    GameObject.DestroyImmediate(sprite, true);
                }
                this.ListMapSprites.Clear();
                this.ListMapSprites = null;
            }
            
            if (this.ListRenderers != null)
            {
                this.ListRenderers.Clear();
                this.ListRenderers = null;
            }
            
            if (this.MapMusic.Sound)
			{
                Resources.UnloadAsset(this.MapMusic.Sound);
                GameObject.DestroyImmediate(this.MapMusic.Sound, true);
			}

            if (this.LocalMapSprite != null)
			{
                Resources.UnloadAsset(this.LocalMapSprite.texture);
                GameObject.DestroyImmediate(this.LocalMapSprite.texture, true);
                GameObject.DestroyImmediate(this.LocalMapSprite, true);
			}

            this.Finish = null;
            this.ReportProgress = null;

            /// Xóa rác
            Resources.UnloadUnusedAssets();
        }
        #endregion
    }
}

