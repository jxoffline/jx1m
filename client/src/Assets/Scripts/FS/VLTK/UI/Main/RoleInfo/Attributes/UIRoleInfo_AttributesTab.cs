using GameServer.VLTK.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace FS.VLTK.UI.Main.RoleInfo
{
    /// <summary>
    /// Khung danh sách chỉ số thuộc tính khác
    /// </summary>
    public class UIRoleInfo_AttributesTab : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Prefab thông tin thuộc tính
        /// </summary>
        [SerializeField]
        private UIRoleInfo_AttributesTab_PropertyInfo UI_PropertyInfoPrefab;
        #endregion

        #region Private fields
        /// <summary>
        /// RectTransform danh sách thuộc tính
        /// </summary>
        private RectTransform rectTransform_PropertyList;
        #endregion

        #region Properties
        private Dictionary<int, int> _Properties = new Dictionary<int, int>();
        /// <summary>
        /// Danh sách thuộc tính và chỉ số tương ứng
        /// </summary>
        public Dictionary<int, int> Properties
        {
            get
            {
                return this._Properties;
            }
            set
            {
                this._Properties = value;
            }
        }
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi khi đối tượng được tạo ra
        /// </summary>
        private void Awake()
        {
            this.rectTransform_PropertyList = this.UI_PropertyInfoPrefab.transform.parent.GetComponent<RectTransform>();
            this.ClearPropertyList();
        }

        /// <summary>
        /// Hàm này gọi ở Frame đầu tiên
        /// </summary>
        private void Start()
        {
            this.InitPrefabs();
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
        /// <param name="action"></param>
        /// <returns></returns>
        private IEnumerator ExecuteSkipFrame(int skip, Action action)
        {
            for (int i = 1; i <= skip; i++)
            {
                yield return null;
            }
            action?.Invoke();
        }

        /// <summary>
        /// Xây lại giao diện
        /// </summary>
        private void RebuildLayout()
        {
            this.StartCoroutine(this.ExecuteSkipFrame(1, () => {
                UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(this.rectTransform_PropertyList);
            }));
        }

        /// <summary>
        /// Làm rỗng danh sách thuộc tính
        /// </summary>
        private void ClearPropertyList()
        {
            foreach (Transform child in this.rectTransform_PropertyList.transform)
            {
                if (child.gameObject != this.UI_PropertyInfoPrefab.gameObject)
                {
                    GameObject.Destroy(child.gameObject);
                }
            }
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Xây danh sách thuộc tính tương ứng
        /// </summary>
        public void Build()
        {
            foreach (KeyValuePair<int, int> propertyPair in this._Properties)
            {
                string propertyName;
                int propertyValue = propertyPair.Value;
                if (!PropertyDefine.PropertiesByID.TryGetValue(propertyPair.Key, out PropertyDefine.Property propertyInfo))
                {
                    propertyName = "Symbol '" + propertyPair.Key + "' not found";
                }
                else
                {
                    propertyName = propertyInfo.Description;
                    propertyName = Utils.RemoveAllHTMLTags(propertyName);
                    propertyName = Regex.Replace(propertyName, @"\:", "");
                    propertyName = Regex.Replace(propertyName, @"\{\d+\}", "");
                    propertyName = propertyName.Trim();
                }

                UIRoleInfo_AttributesTab_PropertyInfo uiPropertyInfo = GameObject.Instantiate<UIRoleInfo_AttributesTab_PropertyInfo>(this.UI_PropertyInfoPrefab);
                uiPropertyInfo.transform.SetParent(this.rectTransform_PropertyList, false);
                uiPropertyInfo.gameObject.SetActive(true);
                uiPropertyInfo.PropertyName = propertyName;
                uiPropertyInfo.PropertyValue = propertyValue;
            }
            this.RebuildLayout();
        }
        #endregion
    }
}
