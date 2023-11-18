using FS.GameEngine.Logic;
using FS.VLTK.Utilities.UnityUI;
using Server.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FS.VLTK.UI.Main.MainUI.BottomBar.SkillBar
{
    /// <summary>
    /// Khung chọn kỹ năng vòng sáng
    /// </summary>
    public class UISkillBar_SelectAuraSkill : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Prefab Button kỹ năng
        /// </summary>
        [SerializeField]
        private UISkillBar_SelectAuraSkill_SkillButton UISkillButton_Prefab;
        #endregion

        #region Properties
        /// <summary>
        /// Kỹ năng được chọn
        /// </summary>
        public Action<Entities.Config.SkillDataEx> SkillSelected { get; set; }
        #endregion

        #region Private fields
        /// <summary>
        /// RectTransform danh sách kỹ năng
        /// </summary>
        private RectTransform skillListsTransform = null;
        #endregion

        #region Core MonoBehaviour
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

        /// <summary>
        /// Sự kiện khi Button kỹ năng được ấn
        /// </summary>
        /// <param name="skillData"></param>
        private void ButtonSkill_Clicked(Entities.Config.SkillDataEx skillData)
        {
            this.SkillSelected?.Invoke(skillData);
            this.Hide();
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
        /// Làm rỗng danh sách kỹ năng
        /// </summary>
        private void ClearSkills()
        {
            foreach (Transform child in this.skillListsTransform.transform)
            {
                if (child.gameObject != this.UISkillButton_Prefab.gameObject)
                {
                    GameObject.Destroy(child.gameObject);
                }
            }
        }

        /// <summary>
        /// Thêm kỹ năng
        /// </summary>
        /// <param name="skillData"></param>
        private void AddSkill(Entities.Config.SkillDataEx skillData)
        {
            UISkillBar_SelectAuraSkill_SkillButton uiSkillButton = GameObject.Instantiate<UISkillBar_SelectAuraSkill_SkillButton>(this.UISkillButton_Prefab);
            uiSkillButton.transform.SetParent(this.skillListsTransform, false);
            uiSkillButton.gameObject.SetActive(true);
            uiSkillButton.Data = skillData;
            uiSkillButton.Click = () =>
            {
                this.ButtonSkill_Clicked(skillData);
            };
        }

        /// <summary>
        /// Thêm toàn bộ kỹ năng vòng sáng vào
        /// </summary>
        private void AddAllAuraSkills()
        {
            /// Nếu không có kỹ năng
            if (Global.Data.RoleData.SkillDataList == null)
            {
                /// Bỏ qua
                return;
            }

            /// Duyệt danh sách kỹ năng
            foreach (SkillData skill in Global.Data.RoleData.SkillDataList)
            {
                /// Nếu chưa học được
                if (skill.Level <= 0)
                {
                    /// Bỏ qua
                    continue;
                }
                /// Thông tin kỹ năng tương ứng
                if (Loader.Loader.Skills.TryGetValue(skill.SkillID, out Entities.Config.SkillDataEx skillData))
                {
                    /// Nếu là vòng sáng
                    if (skillData.IsArua)
                    {
                        /// Thêm kỹ năng tương ứng
                        this.AddSkill(skillData);
                    }
                }
            }
            /// Xây lại giao diện
            this.StartCoroutine(this.ExecuteSkipFrames(1, () =>
            {
                UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(this.skillListsTransform);
            }));
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Hiện khung chọn kỹ năng vòng sáng
        /// </summary>
        public void Show()
        {
            if (this.skillListsTransform == null)
            {
                this.skillListsTransform = this.UISkillButton_Prefab.transform.parent.GetComponent<RectTransform>();
            }
            this.ClearSkills();
            this.gameObject.SetActive(true);
            this.AddAllAuraSkills();
        }

        /// <summary>
        /// Ẩn khung chọn kỹ năng vòng sáng
        /// </summary>
        public void Hide()
        {
            this.gameObject.SetActive(false);
        }
        #endregion
    }
}
