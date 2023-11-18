using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;

namespace FS.VLTK.Utilities.UnityComponent
{
    /// <summary>
    /// Nhóm các đối tượng thay đổi độ trong suốt
    /// </summary>
    public class GroupColor : MonoBehaviour
    {
        #region Define
        [SerializeField]
        private float _Alpha = 1f;

        /// <summary>
        /// Màu
        /// </summary>
        [SerializeField]
        private Color _Color;

        /// <summary>
        /// Các đối tượng tương quan
        /// </summary>
        [SerializeField]
        private GameObject[] TargetObjects;
        #endregion

        /// <summary>
        /// Độ trong suốt (từ 0 đến 1)
        /// </summary>
        public float Alpha
        {
            get
            {
                return this._Alpha;
            }
            set
            {
                if (this._Alpha == value)
                {
                    return;
                }

                this._Alpha = value;
                this.ChangeAlpha();
            }
        }

        /// <summary>
        /// Màu của nhóm đối tượng
        /// </summary>
        public Color Color
        {
            get
            {
                return this._Color;
            }
            set
            {
                if (this._Color == value)
                {
                    return;
                }

                this._Color = value;
                this.ChangeColor();
            }
        }

        /// <summary>
        /// Thay đổi độ trong suốt của nhóm đối tượng
        /// </summary>
        private void ChangeAlpha()
        {
            foreach (GameObject go in this.TargetObjects)
            {
                SpriteRenderer renderer = go.GetComponent<SpriteRenderer>();
                UnityEngine.UI.Image image = go.GetComponent<UnityEngine.UI.Image>();
                TextMeshProUGUI text = go.GetComponent<TextMeshProUGUI>();

                if (renderer != null)
                {
                    Color color = renderer.color;
                    color.a = this.Alpha;
                    renderer.color = color;
                }
                else if (image != null)
                {
                    Color color = image.color;
                    color.a = this.Alpha;
                    image.color = color;
                }
                else if (text != null)
                {
                    Color color = text.color;
                    color.a = this.Alpha;
                    text.color = color;
                }
            }
        }

        /// <summary>
        /// Thay đổi màu của nhóm đối tượng
        /// </summary>
        private void ChangeColor()
        {
            foreach (GameObject go in this.TargetObjects)
            {
                SpriteRenderer renderer = go.GetComponent<SpriteRenderer>();
                UnityEngine.UI.Image image = go.GetComponent<UnityEngine.UI.Image>();
                TextMeshProUGUI text = go.GetComponent<TextMeshProUGUI>();

                if (renderer != null)
                {
                    Color color = this._Color;
                    color.a = this.Alpha;
                    renderer.color = color;
                }
                else if (image != null)
                {
                    Color color = this._Color;
                    color.a = this.Alpha;
                    image.color = color;
                }
                else if (text != null)
                {
                    Color color = this._Color;
                    color.a = this.Alpha;
                    text.color = color;
                }
            }
        }
    }
}