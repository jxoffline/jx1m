using FS.GameEngine.Logic;
using FS.VLTK.Entities.Config;
using FS.VLTK.Utilities.UnityUI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FS.VLTK.UI.Main
{
    /// <summary>
    /// Khung chọn Avarta nhân vật
    /// </summary>
    public class UISelectAvarta : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Button đóng khung
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_Close;

        /// <summary>
        /// Prefab ảnh Avarta nhân vật
        /// </summary>
        [SerializeField]
        private UIRoleAvarta UIImage_Prefab;
        #endregion

        #region Private fields
        /// <summary>
        /// RectTransform danh sách Avarta
        /// </summary>
        private RectTransform transformAvartaList = null;
        #endregion

        #region Properties
        /// <summary>
        /// Sự kiện khi Avarta nhân vật được chọn
        /// </summary>
        public Action<int> AvartaSelected { get; set; }

        /// <summary>
        /// Sự kiện đóng khung
        /// </summary>
        public Action Close { get; set; }
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi khi đối tượng được tạo ra
        /// </summary>
        private void Awake()
        {
            this.transformAvartaList = this.UIImage_Prefab.transform.parent.GetComponent<RectTransform>();
        }

        /// <summary>
        /// Hàm này gọi ở Frame đầu tiên
        /// </summary>
        private void Start()
        {
            this.InitPrefabs();
            this.BuildAvartaList();
        }
        #endregion

        #region Code UI
        /// <summary>
        /// Khởi tạo ban đầu
        /// </summary>
        private void InitPrefabs()
        {
            this.UIButton_Close.onClick.AddListener(this.ButtonClose_Clicked);
        }

        /// <summary>
        /// Sự kiện khi Button đóng khung được ấn
        /// </summary>
        private void ButtonClose_Clicked()
        {
            this.Close?.Invoke();
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
            this.StartCoroutine(this.ExecuteSkipFrames(1, () => {
                UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(this.transformAvartaList);
            }));
        }

        /// <summary>
        /// Xây danh sách Avarta
        /// </summary>
        private void BuildAvartaList()
        {
            /// Duyệt danh sách Avarta
            foreach (RoleAvartaXML roleAvarta in Loader.Loader.RoleAvartas.Values)
            {
                /// Nếu giới tính phù hợp
                if (Global.Data.RoleData.RoleSex == roleAvarta.Sex)
                {
                    UIRoleAvarta uiImage = GameObject.Instantiate<UIRoleAvarta>(this.UIImage_Prefab);
                    uiImage.gameObject.SetActive(true);
                    uiImage.transform.SetParent(this.transformAvartaList, false);

                    uiImage.AvartaID = roleAvarta.ID;
                    uiImage.Click = () => {
                        this.AvartaSelected?.Invoke(roleAvarta.ID);
                    };
                }
            }
            this.RebuildLayout();
        }
        #endregion
    }
}
