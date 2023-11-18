using FS.VLTK.Entities.Config;
using FS.GameEngine.Logic;
using Server.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FS.VLTK.UI.Main.RoleInfo
{
    /// <summary>
    /// Tab Danh vọng 
    /// </summary>
    public class UIRoleInfo_ReputesTab : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Prefab mục danh vọng
        /// </summary>
        [SerializeField]
        private UIRoleInfo_ReputesTab_ReputeCategory UICategory_Prefab;

        /// <summary>
        /// Prefab chi tiết danh vọng
        /// </summary>
        [SerializeField]
        private UIRoleInfo_ReputesTab_ReputeCategory_Item UICategoryItem_Prefab;
        #endregion

        #region Private fields
        /// <summary>
        /// RectTransform danh sách danh vọng
        /// </summary>
        private RectTransform transformList = null;
        #endregion

        #region Properties

        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi khi đối tượng được tạo ra
        /// </summary>
        private void Awake()
        {
            this.transformList = this.UICategoryItem_Prefab.transform.parent.GetComponent<RectTransform>();
        }

        /// <summary>
        /// Hàm này gọi ở Frame đàu tiên
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
            this.Refresh();
        }

        /// <summary>
        /// Hàm này gọi khi đối tượng bị hủy kích hoạt
        /// </summary>
        private void OnDisable()
        {
            this.StopAllCoroutines();
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
                UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(this.transformList);
            }));
        }

        /// <summary>
        /// Làm rỗng danh sách danh mục
        /// </summary>
        private void ClearCategoriesAndItems()
        {
            foreach (Transform child in this.transformList)
            {
                if (child.gameObject != this.UICategory_Prefab.gameObject && child.gameObject != this.UICategoryItem_Prefab.gameObject)
                {
                    GameObject.Destroy(child.gameObject);
                }
            }
        }

        /// <summary>
        /// Thêm danh mục tương ứng
        /// </summary>
        /// <param name="reputeCamp"></param>
        private void AddCategory(ReputeCamp reputeCamp)
        {
            UIRoleInfo_ReputesTab_ReputeCategory uiCategory = GameObject.Instantiate<UIRoleInfo_ReputesTab_ReputeCategory>(this.UICategory_Prefab);
            uiCategory.gameObject.SetActive(true);
            uiCategory.transform.SetParent(this.transformList, false);
            uiCategory.Name = reputeCamp.Name;
        }

        /// <summary>
        /// Thêm danh vọng tương ứng
        /// </summary>
        /// <param name="reputeInfo"></param>
        private void AddItem(ReputeInfo reputeInfo)
        {
            UIRoleInfo_ReputesTab_ReputeCategory_Item uiItem = GameObject.Instantiate<UIRoleInfo_ReputesTab_ReputeCategory_Item>(this.UICategoryItem_Prefab);
            uiItem.gameObject.SetActive(true);
            uiItem.transform.SetParent(this.transformList, false);
            uiItem.Data = reputeInfo;
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Làm mới danh vọng
        /// </summary>
        public void Refresh()
        {
            /// Xóa danh sách
            this.ClearCategoriesAndItems();

            /// Duyệt danh sách danh vọng hệ thống
            List<ReputeCamp> reputeCamps = Loader.Loader.Reputes.Camp;
            /// Tạo danh sách tương ứng
            foreach (ReputeCamp reputeCamp in reputeCamps)
            {
                /// Thêm danh mục tương ứng
                this.AddCategory(reputeCamp);
                /// Nếu không có thông tin danh vọng
                if (Global.Data.RoleData.Repute == null)
                {
                    continue;
                }

                /// Lấy danh sách danh vọng trong danh mục tương ứng
                List<ReputeInfo> reputes = Global.Data.RoleData.Repute.Where(x => x.Camp == reputeCamp.Id).ToList();
                /// Duyệt danh sách và thêm danh vọng tương ứng
                foreach (ReputeInfo repute in reputes)
                {
                    this.AddItem(repute);
                }
            }
            /// Xây lại giao diện
            this.RebuildLayout();
        }
        #endregion
    }
}
