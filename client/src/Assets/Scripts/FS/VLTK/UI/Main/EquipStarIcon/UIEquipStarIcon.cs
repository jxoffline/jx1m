using FS.VLTK.Utilities.UnityUI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FS.VLTK.UI.Main
{
    /// <summary>
    /// Hiệu ứng sao của trang bị
    /// </summary>
    public class UIEquipStarIcon : MonoBehaviour
    {
        /// <summary>
        /// Các loại sao
        /// </summary>
        public enum EquipStarType
        {
            /// <summary>
            /// Nửa sao cơ bản
            /// </summary>
            Half_Basic,
            /// <summary>
            /// Cơ bản
            /// </summary>
            Basic,
            /// <summary>
            /// Nửa sao xanh lam
            /// </summary>
            Half_Blue,
            /// <summary>
            /// Xanh lam
            /// </summary>
            Blue,
            /// <summary>
            /// Nửa sao tím
            /// </summary>
            Half_Purple,
            /// <summary>
            /// Tím
            /// </summary>
            Purple,
            /// <summary>
            /// Nửa sao cam
            /// </summary>
            Half_Orange,
            /// <summary>
            /// Cam
            /// </summary>
            Orange,
            /// <summary>
            /// Nửa sao vàng
            /// </summary>
            Half_Yellow,
            /// <summary>
            /// Vàng
            /// </summary>
            Yellow,
        }

        /// <summary>
        /// Thông tin Sprite tạo thành hiệu ứng sao tương ứng
        /// </summary>
        [Serializable]
        private class EquipStarSprite
        {
            /// <summary>
            /// Loại sao
            /// </summary>
            public EquipStarType Type;

            /// <summary>
            /// Danh sách Sprite tạo thành hiệu ứng
            /// </summary>
            public string[] SpriteNames;
        }

        #region Define
        /// <summary>
        /// Danh sách Sprite tạo thành hiệu ứng
        /// </summary>
        [SerializeField]
        private EquipStarSprite[] _Sprites;

        /// <summary>
        /// Thời gian thực thi hiệu ứng
        /// </summary>
        [SerializeField]
        private float _AnimationDuration;

        /// <summary>
        /// Bundle chứa sprite hiệu ứng
        /// </summary>
        [SerializeField]
        private string _SpriteBundleDir;

        /// <summary>
        /// Atlas chứa Sprite hiệu ứng
        /// </summary>
        [SerializeField]
        private string _SpriteAtlasName;

        /// <summary>
        /// Pixel Perfect
        /// </summary>
        [SerializeField]
        private bool _PixelPerfect = false;
        #endregion

        #region Private fields
        /// <summary>
        /// Đối tượng SpriteFromAssetBundle
        /// </summary>
        private SpriteFromAssetBundle spriteRenderer;

        /// <summary>
        /// Luồng thực thi hiệu ứng
        /// </summary>
        private Coroutine animationCoroutine;
        #endregion

        #region Properties
        private EquipStarType _Type;
        /// <summary>
        /// Loại sao
        /// </summary>
        public EquipStarType Type
        {
            get
            {
                return this._Type;
            }
            set
            {
                this._Type = value;
                this.RefreshSprites();
            }
        }
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi khi đối tượng được tạo ra
        /// </summary>
        private void Awake()
        {
            this.spriteRenderer = this.GetComponent<SpriteFromAssetBundle>();
            this.spriteRenderer.BundleDir = this._SpriteBundleDir;
            this.spriteRenderer.AtlasName = this._SpriteAtlasName;
            this.spriteRenderer.PixelPerfect = this._PixelPerfect;
        }

        /// <summary>
        /// Hàm này gọi ở Frame đầu tiên
        /// </summary>
        private void Start()
        {
            this.InitPrefabs();
            this.RefreshSprites();
        }
        #endregion

        #region Code UI
        /// <summary>
        /// Khởi tạo ban đầu
        /// </summary>
        private void InitPrefabs()
        {

        }
        #endregion

        #region Private methods
        /// <summary>
        /// Làm mới danh sách Sprite
        /// </summary>
        private void RefreshSprites()
        {
            this.PlayAnimation();
        }

        /// <summary>
        /// Thực thi hiệu ứng
        /// </summary>
        private void PlayAnimation()
        {
            if (this.animationCoroutine != null)
            {
                this.animationCoroutine = null;
            }
            string[] spriteNames = this._Sprites.Where(x => x.Type == this._Type).FirstOrDefault()?.SpriteNames;
            this.animationCoroutine = this.StartCoroutine(this.DoPlayAnimation(spriteNames));
        }

        /// <summary>
        /// Thực thi hiệu ứng
        /// </summary>
        /// <param name="spriteNames"></param>
        /// <returns></returns>
        private IEnumerator DoPlayAnimation(string[] spriteNames)
        {
            /// ID vị trí hiện tại
            int idx = 0;
            /// Thời gian thực thi hiệu ứng
            float tickTimeFrame = spriteNames.Length <= 0 ? 0 : this._AnimationDuration / spriteNames.Length;
            while (true)
            {
                /// Nếu danh sách rỗng
                if (spriteNames == null || spriteNames.Length <= 0)
                {
                    yield return null;
                    continue;
                }
                /// Nếu vượt quá kích thước danh sách thì reset về 0
                if (idx >= spriteNames.Length)
                {
                    idx = 0;
                }
                /// Thiết lập Sprite
                this.spriteRenderer.SpriteName = spriteNames[idx];
                this.spriteRenderer.Load();
                /// Nghỉ
                yield return new WaitForSeconds(tickTimeFrame);
                /// Tăng ID vị trí hiện tại lên
                idx++;
            }
        }
        #endregion
    }
}
