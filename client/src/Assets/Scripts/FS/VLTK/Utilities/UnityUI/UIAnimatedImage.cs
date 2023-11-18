using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FS.VLTK.Utilities.UnityUI
{
    /// <summary>
    /// Ảnh động
    /// </summary>
    public class UIAnimatedImage : MonoBehaviour
    {
        /// <summary>
        /// Đối tượng ảnh động
        /// </summary>
        [Serializable]
        public class SpriteFrame
        {
            /// <summary>
            /// Bundle chứa ảnh
            /// </summary>
            public string BundleDir;

            /// <summary>
            /// Tên atlas
            /// </summary>
            public string AtlasName;

            /// <summary>
            /// Tên ảnh
            /// </summary>
            public string SpriteName;

            /// <summary>
            /// Tọa độ đặt
            /// </summary>
            public Vector2 Offset = Vector2.zero;

            /// <summary>
            /// Tự động fill vừa kích thước gốc
            /// </summary>
            public bool PixelPerfect = true;

            /// <summary>
            /// Tỷ lệ phóng to
            /// </summary>
            public float Scale = 1f;
        }

        #region Define
        /// <summary>
        /// Đối tượng ảnh
        /// </summary>
        [SerializeField]
        private SpriteFromAssetBundle UIImage;

        /// <summary>
        /// Tự động bắt đầu
        /// </summary>
        [SerializeField]
        private bool _AutoStart = true;

        /// <summary>
        /// Lặp lại
        /// </summary>
        [SerializeField]
        private bool _Repeat = true;

        /// <summary>
        /// Thời gian duy trì ban đầu
        /// </summary>
        [SerializeField]
        private float _FirstDuration;

        /// <summary>
        /// Thời gian duy trì giai đoạn lặp lại
        /// </summary>
        [SerializeField]
        private float _RepeatDuration;

        /// <summary>
        /// Lặp lại từ Frame
        /// </summary>
        [SerializeField]
        private int _RepeatFromID;

        /// <summary>
        /// Lặp lại đến Frame
        /// </summary>
        [SerializeField]
        private int _RepeatToID;

        /// <summary>
        /// Danh sách các Frame
        /// </summary>
        [SerializeField]
        private SpriteFrame[] _Sprites;

        /// <summary>
        /// Sử dụng vị trí mặc định
        /// </summary>
        [SerializeField]
        private bool _UseOriginPos;
        #endregion

        #region Properties
        /// <summary>
        /// Danh sách Frame
        /// </summary>
        public List<SpriteFrame> Sprites
        {
            get
            {
                return this._Sprites.ToList();
            }
            set
            {
                this._Sprites = value.ToArray();
            }
        }

        /// <summary>
        /// Tự động bắt đầu
        /// </summary>
        public bool AutoStart
        {
            get
            {
                return this._AutoStart;
            }
            set
            {
                this._AutoStart = value;
            }
        }

        /// <summary>
        /// Lặp lại
        /// </summary>
        public bool Repeat
        {
            get
            {
                return this._Repeat;
            }
            set
            {
                this._Repeat = value;
            }
        }

        /// <summary>
        /// Thời gian duy trì giai đoạn ban đầu
        /// </summary>
        public float FirstDuration
        {
            get
            {
                return this._FirstDuration;
            }
            set
            {
                this._FirstDuration = value;
            }
        }

        /// <summary>
        /// Thời gian duy trì giai đoạn lặp lại
        /// </summary>
        public float RepeatDuration
        {
            get
            {
                return this._RepeatDuration;
            }
            set
            {
                this._RepeatDuration = value;
            }
        }

        /// <summary>
        /// Lặp lại từ ID
        /// </summary>
        public int RepeatFromID
        {
            get
            {
                return this._RepeatFromID;
            }
            set
            {
                this._RepeatFromID = value;
            }
        }

        /// <summary>
        /// Lặp lại đến ID
        /// </summary>
        public int RepeatToID
        {
            get
            {
                return this._RepeatToID;
            }
            set
            {
                this._RepeatToID = value;
            }
        }

        /// <summary>
        /// Sự kiện khi kết thúc hiệu ứng
        /// </summary>
        public Action Finish { get; set; }

        /// <summary>
        /// Sự kiện khi hoàn thành một vòng hiệu ứng
        /// </summary>
        public Action CircleCompleted { get; set; }
        #endregion

        /// <summary>
        /// Đối tượng ảnh
        /// </summary>
        private UnityEngine.RectTransform rectTransform;

        /// <summary>
        /// Luồng thực thi hiệu ứng
        /// </summary>
        private Coroutine playCoroutine = null;

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi khi đối tượng được tạo ra
        /// </summary>
        private void Awake()
        {
            this.rectTransform = this.gameObject.GetComponent<UnityEngine.RectTransform>();
        }

        /// <summary>
        /// Hàm này gọi đến khi đối tượng được kích hoạt
        /// </summary>
        private void OnEnable()
        {
            this.StopAllCoroutines();
            if (this._AutoStart)
            {
                this.Play();
            }
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Thực hiện hiệu ứng
        /// </summary>
        public void Play()
        {
            if (this._Sprites.Length <= 0)
            {
                return;
            }
            if (this.playCoroutine != null)
            {
                this.StopCoroutine(this.playCoroutine);
            }
            this.playCoroutine = this.StartCoroutine(this.DoPlay());
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Thực hiện hiệu ứng
        /// </summary>
        /// <returns></returns>
        private IEnumerator DoPlay()
        {
            float timePerFrame_First = this._FirstDuration / this._Sprites.Length;
            float timePerFrame_Repeat = this._RepeatDuration / (this._RepeatToID - this._RepeatFromID);
            int frameID = 0;
            bool isFirstPass = true;

            //Debug.Log("timePerFrame_First = " + timePerFrame_First);
            //Debug.Log("timePerFrame_Repeat = " + timePerFrame_Repeat);
            while (true)
            {
                SpriteFrame frame = this._Sprites[frameID++];
                this.UIImage.BundleDir = frame.BundleDir;
                this.UIImage.AtlasName = frame.AtlasName;
                this.UIImage.SpriteName = frame.SpriteName;
                this.UIImage.PixelPerfect = frame.PixelPerfect;
                this.UIImage.Scale = frame.Scale;
                this.UIImage.Load();

                if (!this._UseOriginPos)
                {
                    this.rectTransform.anchoredPosition = frame.Offset;
                }

                if (isFirstPass)
                {
                    yield return new WaitForSeconds(timePerFrame_First);
                }
                else
                {
                    yield return new WaitForSeconds(timePerFrame_Repeat);
                }
                
                if (frameID >= this._Sprites.Length)
                {
                    if (this._Repeat)
                    {
                        frameID = this._RepeatFromID;
                        isFirstPass = false;
                        this.CircleCompleted?.Invoke();
                    }
                    else
                    {
                        break;
                    }
                }
                else if (!isFirstPass && frameID >= this._RepeatToID)
                {
                    if (this._Repeat)
                    {
                        frameID = this._RepeatFromID;
                    }
                    else
                    {
                        break;
                    }
                }
            }
            this.Finish?.Invoke();
            this.playCoroutine = null;
        }
        #endregion
    }
}