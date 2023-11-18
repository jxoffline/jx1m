using FS.GameEngine.Logic;
using FS.VLTK.Factory;
using System.Collections;
using TMPro;
using UnityEngine;

namespace FS.VLTK.UI.CoreUI
{
    /// <summary>
    /// Đối tượng chữ bay hiển thị sát thương
    /// </summary>
    public class UIFlyingText : TTMonoBehaviour
    {
        #region Defines
        /// <summary>
        /// Tọa độ điểm đặt ban đầu (theo màn hình)
        /// </summary>
        [SerializeField]
        private Vector2 _Offset;

        /// <summary>
        /// Vector biểu diễn vị trí bay (theo màn hình)
        /// </summary>
        [SerializeField]
        private Vector2 _FlyVector;

        /// <summary>
        /// Thời gian bay
        /// </summary>
        [SerializeField]
        private float _Duration;
        #endregion

        #region Properties
        /// <summary>
        /// Tọa độ điểm đặt ban đầu (theo màn hình)
        /// </summary>
        public Vector2 Offset
        {
            get
            {
                return this._Offset;
            }
            set
            {
                this._Offset = value;
            }
        }

        /// <summary>
        /// Vector biểu diễn vị trí bay (theo màn hình)
        /// </summary>
        public Vector2 FlyVector
        {
            get
            {
                return this._FlyVector;
            }
            set
            {
                this._FlyVector = value;
            }
        }

        /// <summary>
        /// Thời gian bay
        /// </summary>
        public float Duration
        {
            get
            {
                return this._Duration;
            }
            set
            {
                this._Duration = value;
            }
        }

        /// <summary>
        /// Giá trị
        /// </summary>
        public string Text
        {
            get
            {
                return this.UIText.text;
            }
            set
            {
                this.UIText.text = value;
            }
        }

        /// <summary>
        /// Màu chữ
        /// </summary>
        public Color Color
        {
            get
            {
                return this.UIText.color;
            }
            set
            {
                this.UIText.color = value;
            }
        }

        /// <summary>
        /// Thời gian Delay trước khi thực hiện hiệu ứng
        /// </summary>
        public float Delay { get; set; } = 0f;

        /// <summary>
        /// Đối tượng tham chiếu
        /// </summary>
        public GameObject ReferenceObject { get; set; }
        #endregion

        #region Private fields
        /// <summary>
        /// Text
        /// </summary>
        private TextMeshPro UIText;

        /// <summary>
        /// Màu sắc ban đầu
        /// </summary>
        private Color firstColor;

        /// <summary>
        /// Vector chỉ hướng bay ban đầu
        /// </summary>
        private Vector2 firstFlyVector;
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi khi đối tượng được tạo ra
        /// </summary>
        private void Awake()
        {
            this.UIText = this.GetComponent<TextMeshPro>();
            this.firstColor = this.UIText.color;
            this.firstFlyVector = this._FlyVector;
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Luồng thực hiện hiệu ứng
        /// </summary>
        /// <param name="fromPos"></param>
        /// <param name="toPos"></param>
        /// <param name="duration"></param>
        /// <returns></returns>
        private IEnumerator DoPlay(Vector2 fromPos, Vector2 toPos, float duration)
        {
            this.gameObject.transform.localPosition = fromPos;
            yield return null;

            Vector2 dirVector = toPos - fromPos;
            float tickTime = 0;

            while (tickTime < duration)
            {
                float percent = tickTime / duration;
                Vector2 newPos = fromPos + dirVector * percent;
                this.gameObject.transform.localPosition = newPos;

                yield return null;
                tickTime += Time.deltaTime;
            }

            this.gameObject.transform.localPosition = toPos;
            yield return null;

            this.Destroy();
        }

        /// <summary>
        /// Luồng thực hiện hiệu ứng theo vị trí Local
        /// </summary>
        /// <param name="fromPos"></param>
        /// <param name="toPos"></param>
        /// <param name="duration"></param>
        /// <returns></returns>
        private IEnumerator DoPlayLocal(Vector2 fromPos, Vector2 toPos, float duration)
        {
            this.transform.localPosition = fromPos;
            yield return null;

            Vector2 dirVector = toPos - fromPos;
            float tickTime = 0;

            while (tickTime < duration)
            {
                float percent = tickTime / duration;
                Vector2 newPos = fromPos + dirVector * percent;

                this.transform.localPosition = newPos;

                yield return null;
                tickTime += Time.deltaTime;
            }

            this.transform.localPosition = toPos;
            yield return null;

            this.Destroy();
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Thực hiện hiệu ứng
        /// </summary>
        public void Play()
        {
            /// Ngừng luồng
            this.StopAllCoroutines();

            /// Cho ra góc
            this.gameObject.transform.localPosition = new Vector2(-99999, -99999);
            /// Luồng thực thi
            IEnumerator DelayThenExecute()
            {
                if (this.Delay > 0)
                {
                    yield return new WaitForSeconds(this.Delay);
                }

                /// Toác
                if (this.ReferenceObject == null)
                {
                    yield break;
                }

                Vector2 fromPos = (Vector2) this.ReferenceObject.transform.localPosition + this._Offset;
                Vector2 toPos = (Vector2) this.ReferenceObject.transform.localPosition + this._Offset + this._FlyVector;
                yield return this.DoPlay(fromPos, toPos, this._Duration);
            }
            /// Bắt đầu luồng
            this.StartCoroutine(DelayThenExecute());
        }

        /// <summary>
        /// Hủy đối tượng
        /// </summary>
        public void Destroy()
        {
            this.StopAllCoroutines();
            this.Offset = default;
            this.FlyVector = this.firstFlyVector;
            this.Duration = 0f;
            this.Text = "";
            this.Color = this.firstColor;
            this.Delay = 0f;
            this.ReferenceObject = null;
            KTObjectPoolManager.Instance.ReturnToPool(this.gameObject);
        }
        #endregion
    }
}