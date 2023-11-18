using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using FS.GameEngine.Logic;
using FS.VLTK.Entities.Config;
using System.Collections;

namespace FS.VLTK.UI.Main.RoleInfo
{
    /// <summary>
    /// Tab danh hiệu
    /// </summary>
    public class UIRoleInfo_TitlesTab : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Prefab danh hiệu
        /// </summary>
        [SerializeField]
        private UIRoleInfo_TitlesTab_Item UIItem_Prefab;

        /// <summary>
        /// Text mô tả danh hiệu
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_Description;

        /// <summary>
        /// Button thiết lập làm danh hiệu hiện tại
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_SetAsCurrentTitle;
        #endregion

        #region Private fields
        /// <summary>
        /// RectTransform danh sách danh hiệu
        /// </summary>
        private RectTransform transformTitleList = null;

        /// <summary>
        /// Danh hiệu được chọn
        /// </summary>
        private KTitleXML selectedTitle = null;
		#endregion

		#region Properties
        /// <summary>
        /// Sự kiện chọn làm danh hiệu hiện tại
        /// </summary>
        public Action<int> SetAsCurrentTitle { get; set; }
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi khi đối tượng được tạo ra
        /// </summary>
        private void Awake()
		{
            this.transformTitleList = this.UIItem_Prefab.transform.parent.GetComponent<RectTransform>();
		}

        /// <summary>
        /// Hàm này gọi ở Frame đầu tiên
        /// </summary>
        private void Start()
        {
            this.InitPrefabs();
            this.Refresh();
        }
        #endregion

        #region Code UI
        /// <summary>
        /// Khởi tạo ban đầu
        /// </summary>
        private void InitPrefabs()
        {
            this.UIButton_SetAsCurrentTitle.onClick.AddListener(this.ButtonSetAsCurrentTitle_Clicked);
            this.UIButton_SetAsCurrentTitle.interactable = false;
        }

        /// <summary>
        /// Sự kiện khi Button chọn làm danh hiệu hiện tại được ấn
        /// </summary>
        private void ButtonSetAsCurrentTitle_Clicked()
        {
            /// Nếu không có danh hiệu nào được chọn
            if (this.selectedTitle == null)
            {
                KTGlobal.AddNotification("Hãy chọn một danh hiệu!");
                return;
            }

            /// Hủy Button chức năng
            this.UIButton_SetAsCurrentTitle.interactable = false;
            /// Thực thi sự kiện
            this.SetAsCurrentTitle?.Invoke(this.selectedTitle.ID);
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
            /// Nếu đối tượng không được kích hoạt
            if (!this.gameObject.activeSelf)
			{
                return;
			}
            /// Thực hiện xây lại giao diện ở Frame tiếp theo
            this.StartCoroutine(this.ExecuteSkipFrames(1, () => {
                UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(this.transformTitleList);
            }));
		}

        /// <summary>
        /// Làm mới danh sách danh hiệu
        /// </summary>
        private void ClearTitles()
        {
            foreach (Transform child in this.transformTitleList.transform)
            {
                if (child.gameObject != this.UIItem_Prefab.gameObject)
                {
                    GameObject.Destroy(child.gameObject);
                }
            }
        }

        /// <summary>
        /// Thêm danh hiệu nhân vật tương ứng
        /// </summary>
        /// <param name="titleInfo"></param>
        private void AddTitle(KTitleXML titleInfo)
		{
            UIRoleInfo_TitlesTab_Item titleItem = GameObject.Instantiate<UIRoleInfo_TitlesTab_Item>(this.UIItem_Prefab);
            titleItem.transform.SetParent(this.transformTitleList, false);
            titleItem.gameObject.SetActive(true);

            titleItem.Data = titleInfo;
            titleItem.IsCurrentTitle = Global.Data.RoleData.SelfCurrentTitleID == titleInfo.ID;
            titleItem.Select = () => {
                /// Đánh dấu danh hiệu được chọn
                this.selectedTitle = titleInfo;
                /// Thiết lập mô tả
                this.UIText_Description.text = titleInfo.Description;
                /// Nếu chưa phải danh hiệu hiện tại thì cho tương tác với Button
                this.UIButton_SetAsCurrentTitle.interactable = titleInfo.ID != Global.Data.RoleData.SelfCurrentTitleID;
            };
        }

        /// <summary>
        /// Tìm nút chứa danh hiệu tương ứng
        /// </summary>
        /// <param name="titleID"></param>
        /// <returns></returns>
        private UIRoleInfo_TitlesTab_Item FindTitle(int titleID)
		{
            foreach (Transform child in this.transformTitleList.transform)
			{
                if (child.gameObject != this.UIItem_Prefab.gameObject)
				{
                    UIRoleInfo_TitlesTab_Item uiTitleInfo = child.GetComponent<UIRoleInfo_TitlesTab_Item>();
                    if (uiTitleInfo.Data.ID == titleID)
					{
                        return uiTitleInfo;
					}
				}
			}
            return null;
		}

        /// <summary>
        /// Xóa danh hiệu tương ứng
        /// </summary>
        /// <param name="titleID"></param>
        private void RemoveTitle(int titleID)
		{
            /// Tìm nút chứa thông tin danh hiệu tương ứng
            UIRoleInfo_TitlesTab_Item uiTitleInfo = this.FindTitle(titleID);
            /// Nếu không tìm thấy thì thôi
            if (uiTitleInfo == null)
			{
                return;
			}
            /// Xóa nút tương ứng
            GameObject.Destroy(uiTitleInfo.gameObject);
		}

        /// <summary>
        /// Làm mới hiển thị toàn bộ danh hiệu
        /// </summary>
        private void ResetAllTitles()
		{
            foreach (Transform child in this.transformTitleList.transform)
            {
                if (child.gameObject != this.UIItem_Prefab.gameObject)
                {
                    UIRoleInfo_TitlesTab_Item uiTitleInfo = child.GetComponent<UIRoleInfo_TitlesTab_Item>();
                    uiTitleInfo.IsCurrentTitle = false;
                }
            }
        }

        /// <summary>
        /// Làm mới danh sách
        /// </summary>
        private void Refresh()
        {
            /// Xóa toàn bộ danh hiệu
            this.ClearTitles();

            /// Ẩn Button chức năng
            this.UIButton_SetAsCurrentTitle.gameObject.SetActive(false);
            /// Hủy Text
            this.UIText_Description.text = "";
            /// Hủy danh hiệu đầu tiên
            this.selectedTitle = null;

            /// Nếu không có danh hiệu
            if (Global.Data.RoleData.SelfTitles == null || Global.Data.RoleData.SelfTitles.Count <= 0)
			{
                return;
			}

            /// Duyệt danh sách
            foreach (int titleID in Global.Data.RoleData.SelfTitles)
            {
                /// Thông tin danh hiệu tương ứng
                if (!Loader.Loader.RoleTitles.TryGetValue(titleID, out KTitleXML titleInfo))
				{
                    continue;
				}

                /// Nếu chưa được chọn thì chọn làm danh hiệu đầu tiên
                if (this.selectedTitle == null)
				{
                    this.selectedTitle = titleInfo;
				}

                /// Thêm danh hiệu tương ứng
                this.AddTitle(titleInfo);
            }

            /// Hiện Button chức năng
            this.UIButton_SetAsCurrentTitle.gameObject.SetActive(true);

            /// Đánh dấu tương tác với Button
            this.UIButton_SetAsCurrentTitle.interactable = this.selectedTitle == null ? false : this.selectedTitle.ID != Global.Data.RoleData.SelfCurrentTitleID;
            if (this.selectedTitle != null)
			{
                this.UIText_Description.text = this.selectedTitle.Description;
			}

            /// Xây lại giao diện
            this.RebuildLayout();
        }
		#endregion

		#region Public methods
        /// <summary>
        /// Thêm danh hiệu nhân vật tương ứng
        /// </summary>
        /// <param name="titleID"></param>
        public void AddRoleTitle(int titleID)
		{
            /// Tìm nút chứa thông tin danh hiệu tương ứng
            UIRoleInfo_TitlesTab_Item uiTitleInfo = this.FindTitle(titleID);
            /// Nếu đã tìm thấy thì thôi
            if (uiTitleInfo != null)
            {
                return;
            }

            /// Nếu danh hiệu không tồn tại
            if (!Loader.Loader.RoleTitles.TryGetValue(titleID, out KTitleXML titleInfo))
			{
                return;
			}

            /// Thêm danh hiệu
            this.AddTitle(titleInfo);
            /// Xây lại giao diện
            this.RebuildLayout();
        }

        /// <summary>
        /// Xóa danh hiệu nhân vật tương ứng
        /// </summary>
        /// <param name="titleID"></param>
        public void RemoveRoleTitle(int titleID)
		{
            /// Xóa danh hiệu tương ứng
            this.RemoveTitle(titleID);
            /// Xây lại giao diện
            this.RebuildLayout();
		}

        /// <summary>
        /// Thiết lập làm danh hiệu hiện tại
        /// </summary>
        public void RefreshCurrentTitle()
		{
            this.ResetAllTitles();
            /// Tìm nút chứa thông tin danh hiệu tương ứng
            UIRoleInfo_TitlesTab_Item uiTitleInfo = this.FindTitle(Global.Data.RoleData.SelfCurrentTitleID);
            /// Nếu không tìm thấy thì thôi
            if (uiTitleInfo == null)
            {
                return;
            }

            /// Đánh dấu là danh hiệu hiện tại
            uiTitleInfo.IsCurrentTitle = true;
        }
		#endregion
	}
}
