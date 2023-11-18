using FS.VLTK.Utilities.UnityComponent;
using FS.VLTK.Utilities.UnityUI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FS.VLTK.UI.Main.MainUI
{
    /// <summary>
    /// Khung thông báo số liên trảm có được
    /// </summary>
    public class UIStreakKillNotification : MonoBehaviour
    {
        /// <summary>
        /// Danh sách số
        /// </summary>
        [Serializable]
        private enum Number
        {
            /// <summary>
            /// Số 0
            /// </summary>
            Zero = 0,
            /// <summary>
            /// Số 1
            /// </summary>
            One = 1,
            /// <summary>
            /// Số 2
            /// </summary>
            Two = 2,
            /// <summary>
            /// Số 3
            /// </summary>
            Three = 3,
            /// <summary>
            /// Số 4
            /// </summary>
            Four = 4,
            /// <summary>
            /// Số 5
            /// </summary>
            Five = 5,
            /// <summary>
            /// Số 6
            /// </summary>
            Six = 6,
            /// <summary>
            /// Số 7
            /// </summary>
            Seven = 7,
            /// <summary>
            /// Số 8
            /// </summary>
            Eight = 8,
            /// <summary>
            /// Số 8
            /// </summary>
            Nine = 9,
        }

        /// <summary>
        /// Tên Sprite tương ứng số
        /// </summary>
        [Serializable]
        private class NumberSprite
        {
            /// <summary>
            /// Số
            /// </summary>
            public Number Number;

            /// <summary>
            /// Tên ảnh
            /// </summary>
            public string SpriteName;
        }

        #region Define
        /// <summary>
        /// Image background
        /// </summary>
        [SerializeField]
        private SpriteFromAssetBundle UIImage_Background;

        /// <summary>
        /// Prefab ảnh giá trị số
        /// </summary>
        [SerializeField]
        private SpriteFromAssetBundle UIImage_NumberIconPrefab;

        /// <summary>
        /// Sprite tương ứng số
        /// </summary>
        [SerializeField]
        private NumberSprite[] Sprites;

        /// <summary>
        /// Thời gian hiển thị
        /// </summary>
        [SerializeField]
        private float _VisibleDuration = 5f;

        /// <summary>
        /// Thời gian thực hiện hiệu ứng ẩn
        /// </summary>
        [SerializeField]
        private float _FadeDuration = 1f;
        #endregion

        #region Private fields
        /// <summary>
        /// Transform danh sách số
        /// </summary>
        private RectTransform numberTransform = null;

        /// <summary>
        /// Luồng thực thi tự ẩn
        /// </summary>
        private Coroutine autoFadeCoroutine = null;
        #endregion

        #region Properties
        private int _KillNumber = 0;
        /// <summary>
        /// Số lượng liên trảm
        /// </summary>
        public int KillNumber
        {
            get
            {
                return this._KillNumber;
            }
            set
            {
                this._KillNumber = value;
                this.BuildNumber();
            }
        }
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi khi đối tượng được tạo ra
        /// </summary>
        private void Awake()
        {
            this.numberTransform = this.UIImage_NumberIconPrefab.transform.parent.GetComponent<RectTransform>();
        }

        /// <summary>
        /// Hàm này gọi ở Frame đầu tiên
        /// </summary>
        private void Start()
        {
            this.InitPrefabs();
            this.RebuildLayout();
        }

        /// <summary>
        /// Hàm này gọi khi đối tượng được kích hoạt
        /// </summary>
        private void OnEnable()
        {
            this.RebuildLayout();
        }

        /// <summary>
        /// Hàm này gọi khi đối tượng bị hủy kích hoạt
        /// </summary>
        private void OnDisable()
        {
            this.StopAllCoroutines();
            this.autoFadeCoroutine = null;
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
        /// Thực thi sự kiện bỏ qua một số Frame
        /// </summary>
        /// <param name="skip"></param>
        /// <param name="work"></param>
        /// <returns></returns>
        private IEnumerator ExecuteSkipFrames(int skip, Action work)
        {
            for (int i = 1; i <= skip; i++)
            {
                yield return null;
            }
            work?.Invoke();
        }

        /// <summary>
        /// Xây lại giao diện
        /// </summary>
        private void RebuildLayout()
        {
            if (!this.gameObject.activeSelf)
            {
                return;
            }

            this.StartCoroutine(this.ExecuteSkipFrames(1, () => {
                UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(this.numberTransform);
            }));
        }

        /// <summary>
        /// Ẩn toàn bộ số
        /// </summary>
        private void DisableAllNumbers()
        {
            foreach (Transform child in this.numberTransform.transform)
            {
                child.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Sinh mới số
        /// </summary>
        /// <returns></returns>
        private SpriteFromAssetBundle SpawnNewNumber()
        {
            SpriteFromAssetBundle uiImage = GameObject.Instantiate<SpriteFromAssetBundle>(this.UIImage_NumberIconPrefab);
            uiImage.gameObject.SetActive(false);
            uiImage.transform.SetParent(this.numberTransform, false);

            return uiImage;
        }

        /// <summary>
        /// Trả về số ở vị trí tương ứng (đánh số từ 1)
        /// </summary>
        /// <param name="idx"></param>
        /// <returns></returns>
        private SpriteFromAssetBundle GetNumber(int idx)
        {
            /// Nếu tràn kích thước
            if (idx < 0 || idx >= this.numberTransform.childCount)
            {
                return null;
            }
            /// Trả ra kết quả
            return this.numberTransform.GetChild(idx).GetComponent<SpriteFromAssetBundle>();
        }

        /// <summary>
        /// Xây giá trị số
        /// </summary>
        private void BuildNumber()
        {
            /// Nếu không hiện
            if (!this.gameObject.activeSelf)
            {
                return;
            }

            /// Số lượng
            string numberStr = this._KillNumber.ToString();

            /// Nếu không đủ số lượng thì sinh thêm
            while (this.numberTransform.childCount <= numberStr.Length)
            {
                this.SpawnNewNumber();
            }

            /// Nếu đầu vào dưới 0 thì bỏ qua
            if (this._KillNumber < 0)
            {
                return;
            }

            /// Ẩn toàn bộ số
            this.DisableAllNumbers();

            /// Vị trí
            int idx = 0;
            /// Duyệt từ đầu để xây số
            foreach (char number in numberStr)
            {
                idx++;
                SpriteFromAssetBundle uiImage = this.GetNumber(idx);
                if (uiImage == null)
                {
                    break;
                }

                uiImage.transform.gameObject.SetActive(true);
                uiImage.SpriteName = this.Sprites.Where(x => (int) x.Number == (number - '0')).FirstOrDefault().SpriteName;
                uiImage.Load();
            }
            /// Xây lại giao diện
            this.RebuildLayout();
        }

        /// <summary>
        /// Thay đổi độ trong suốt của ảnh tương ứng
        /// </summary>
        /// <param name="image"></param>
        private void ChangeAlphaOfImage(SpriteFromAssetBundle image, float alpha)
        {
            UnityEngine.UI.Image uiImage = this.UIImage_Background.GetComponent<UnityEngine.UI.Image>();
            Color color = uiImage.color;
            color.a = alpha;
            uiImage.color = color;
        }

        /// <summary>
        /// Thay đổi độ trong suốt của các thành phần
        /// </summary>
        /// <param name="alpha"></param>
        private void ChangeAlpha(float alpha)
        {
            /// Background
            this.ChangeAlphaOfImage(this.UIImage_Background, alpha);
            /// Các số
            foreach (Transform child in this.numberTransform.transform)
            {
                if (child.gameObject != this.UIImage_NumberIconPrefab.gameObject)
                {
                    SpriteFromAssetBundle uiImage = child.GetComponent<SpriteFromAssetBundle>();
                    this.ChangeAlphaOfImage(uiImage, alpha);
                }
            }
        }

        /// <summary>
        /// Luồng thực thi hiệu ứng ẩn
        /// </summary>
        /// <returns></returns>
        private IEnumerator DoAnimation()
        {
            /// Đợi hiện đủ số giây quy định
            if (this._VisibleDuration > 0)
            {
                yield return new WaitForSeconds(this._VisibleDuration);
            }

            float lifeTime = 0f;
            while (lifeTime < this._FadeDuration)
            {
                float percent = lifeTime / this._FadeDuration;
                this.ChangeAlpha(1 - percent);

                lifeTime += Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }

            /// Ẩn
            this.Hide();
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Hiện khung
        /// </summary>
        public void Show()
        {
            this.gameObject.SetActive(true);
            this.BuildNumber();
            /// Thiết lập màu
            this.ChangeAlpha(1f);
            /// Thực hiện tự ẩn
            if (this.autoFadeCoroutine != null)
            {
                this.StopCoroutine(this.autoFadeCoroutine);
            }
            this.autoFadeCoroutine = this.StartCoroutine(this.DoAnimation());
        }

        /// <summary>
        /// Ẩn khung
        /// </summary>
        public void Hide()
        {
            this.gameObject.SetActive(false);
        }
        #endregion
    }
}
