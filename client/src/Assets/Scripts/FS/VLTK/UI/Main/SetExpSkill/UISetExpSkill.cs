using FS.VLTK.UI.Main.SetExpSkill;
using Server.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FS.VLTK.UI.Main
{
    /// <summary>
    /// Khung thiết lập kỹ năng tu luyện
    /// </summary>
    public class UISetExpSkill : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Button đóng khung
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_Close;

        /// <summary>
        /// Prefab Button kỹ năng
        /// </summary>
        [SerializeField]
        private UISetExpSkill_SkillButton UIButton_SkillPrefab;
        #endregion

        #region Properties
        private List<ExpSkillData> _Data;
        /// <summary>
        /// Dữ liệu
        /// </summary>
        public List<ExpSkillData> Data
        {
            get
            {
                return this._Data;
            }
            set
            {
                this._Data = value;
                /// Làm mới dữ liệu
                this.RefreshData();
            }
        }

        /// <summary>
        /// Sự kiện đóng khung
        /// </summary>
        public Action Close { get; set; }

        /// <summary>
        /// Sự kiện thiết lập làm kỹ năng tu luyện
        /// </summary>
        public Action<int> SetAsCurrentExpSkill { get; set; }
        #endregion

        #region Private fields
        /// <summary>
        /// RectTransform danh sách kỹ năng
        /// </summary>
        private RectTransform transformSkillList;
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi khi đối tượng được tạo ra
        /// </summary>
        private void Awake()
        {
            this.transformSkillList = this.UIButton_SkillPrefab.transform.parent.GetComponent<RectTransform>();
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
        /// Luồng thực thi sự kiện bỏ qua một số Frame
        /// </summary>
        /// <param name="skip"></param>
        /// <param name="work"></param>
        /// <returns></returns>
        private IEnumerator DoExecuteSkipFrames(int skip, Action work)
        {
            for (int i = 1; i <= skip; i++)
            {
                yield return null;
            }
            work?.Invoke();
        }

        /// <summary>
        /// Thực thi sự kiện bỏ qua một số Frame
        /// </summary>
        /// <param name="skip"></param>
        /// <param name="work"></param>
        private void ExecuteSkipFrames(int skip, Action work)
        {
            this.StartCoroutine(this.DoExecuteSkipFrames(skip, work));
        }

        /// <summary>
        /// Làm mới dữ liệu
        /// </summary>
        private void RefreshData()
        {
            /// Toác
            if (this._Data == null)
            {
                /// Đóng khung
                this.Close?.Invoke();
                /// Bỏ qua
                return;
            }

            /// Xóa toàn bộ kỹ năng cũ
            foreach (Transform child in this.transformSkillList.transform)
            {
                if (child.gameObject != this.UIButton_SkillPrefab.gameObject)
                {
                    GameObject.Destroy(child.gameObject);
                }
            }

            /// Duyệt danh sách kỹ năng
            foreach (ExpSkillData skillData in this._Data)
            {
                /// Tạo mới
                UISetExpSkill_SkillButton uiButton = GameObject.Instantiate<UISetExpSkill_SkillButton>(this.UIButton_SkillPrefab);
                uiButton.transform.SetParent(this.transformSkillList, false);
                uiButton.gameObject.SetActive(true);
                uiButton.Data = skillData;
                uiButton.SetAsCurrentExpSkill = () =>
                {
                    /// Thực thi sự kiện
                    this.SetAsCurrentExpSkill?.Invoke(skillData.SkillID);
                };
            }

            /// Xây lại giao diện
            this.ExecuteSkipFrames(1, () =>
            {
                UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(this.transformSkillList);
            });
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Cập nhật kỹ năng đang tu luyện
        /// </summary>
        /// <param name="skillID"></param>
        public void UpdateCurrentExpSkill(int skillID)
        {
            /// Duyệt danh sách
            foreach (Transform child in this.transformSkillList.transform)
            {
                /// Nếu không phải Prefab
                if (child.gameObject != this.UIButton_SkillPrefab.gameObject)
                {
                    /// Đối tượng tương ứng
                    UISetExpSkill_SkillButton uiButton = child.GetComponent<UISetExpSkill_SkillButton>();
                    /// Toác
                    if (uiButton == null)
                    {
                        /// Bỏ qua
                        continue;
                    }

                    /// Nếu là kỹ năng được cập nhật
                    if (uiButton.Data.SkillID == skillID)
                    {
                        /// Đánh dấu là kỹ năng được chọn
                        uiButton.Data.IsCurrentExpSkill = true;
                    }
                    /// Nếu không phải kỹ năng được cập nhật
                    else
                    {
                        /// Bỏ đánh dấu là kỹ năng được chọn
                        uiButton.Data.IsCurrentExpSkill = false;
                    }

                    /// Cập nhật hiển thị
                    uiButton.RefreshMark();
                }
            }
        }
        #endregion
    }
}
