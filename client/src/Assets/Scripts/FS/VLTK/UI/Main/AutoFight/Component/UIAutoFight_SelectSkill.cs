using FS.GameEngine.Logic;
using FS.VLTK.Entities.Config;
using FS.VLTK.UI.Main.AutoFight.Component;
using Server.Data;
using System;
using System.Collections;
using UnityEngine;

namespace FS.VLTK.UI.Main.AutoFight
{
    /// <summary>
    /// Khung chọn kỹ năng
    /// </summary>
    public class UIAutoFight_SelectSkill : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Button đóng khung
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_Close;

        /// <summary>
        /// Prefab ô kỹ năng
        /// </summary>
        [SerializeField]
        private UIAutoFight_SelectSkill_SkillButton UIButton_SkillPrefab;
        #endregion

        #region Private fields
        /// <summary>
        /// RectTransform chứa danh sách kỹ năng
        /// </summary>
        private RectTransform transformSkillList = null;
        #endregion

        #region Properties
        /// <summary>
        /// Sự kiện đóng khung
        /// </summary>
        public Action Close { get; set; }

        /// <summary>
        /// Sự kiện khi kỹ năng được chọn
        /// </summary>
        public Action<SkillDataEx> SkillSelected { get; set; }
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi khi đối tượng được tạo ra
        /// </summary>
        private void Awake()
        {
            this.transformSkillList = this.UIButton_SkillPrefab.transform.parent.gameObject.GetComponent<RectTransform>();
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

        /// <summary>
        /// Sự kiện khi Button kỹ năng được chọn
        /// </summary>
        /// <param name="skillData"></param>
        private void ButtonSkill_Clicked(SkillDataEx skillData)
        {
            this.SkillSelected?.Invoke(skillData);
            this.ButtonClose_Clicked();
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Thực thi bỏ qua một số Frame nhất định
        /// </summary>
        /// <param name="skip"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        private IEnumerator ExecuteSkipFrame(int skip, Action callback)
        {
            for (int i = 1; i <= skip; i++)
            {
                yield return null;
            }
            callback?.Invoke();
        }

        /// <summary>
        /// Xây lại giao diện
        /// </summary>
        private void RebuildLayout()
        {
            this.StartCoroutine(this.ExecuteSkipFrame(1, () => {
                UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(this.transformSkillList);
            }));
        }

        /// <summary>
        /// Làm rỗng danh sách kỹ năng
        /// </summary>
        private void ClearSkillList()
        {
            foreach (Transform child in this.transformSkillList.transform)
            {
                if (child.gameObject != this.UIButton_SkillPrefab.gameObject)
                {
                    GameObject.Destroy(child.gameObject);
                }
            }
        }

        /// <summary>
        /// Thêm kỹ năng
        /// </summary>
        /// <param name="skillData"></param>
        private void AddSkill(SkillDataEx skillData)
        {
            UIAutoFight_SelectSkill_SkillButton uiButtonSkill = GameObject.Instantiate<UIAutoFight_SelectSkill_SkillButton>(this.UIButton_SkillPrefab);
            uiButtonSkill.gameObject.SetActive(true);
            uiButtonSkill.transform.SetParent(this.transformSkillList, false);
            uiButtonSkill.Data = skillData;
            uiButtonSkill.Click = () => {
                this.ButtonSkill_Clicked(skillData);
            };
        }

        /// <summary>
        /// Làm mới khung
        /// </summary>
        private void Refresh()
        {
            this.ClearSkillList();

            foreach (SkillData dbSkillData in Global.Data.RoleData.SkillDataList)
            {
                if (Loader.Loader.Skills.TryGetValue(dbSkillData.SkillID, out SkillDataEx skillData))
                {
                    /// Nếu không phải kỹ năng bị động và vòng sáng
                    if (dbSkillData.Level > 0 && !skillData.IsArua && KTGlobal.IsCanUseSkill(skillData))
                    {
                        this.AddSkill(skillData);
                    }
                }
            }

            this.RebuildLayout();
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Hiện đối tượng
        /// </summary>
        public void Show()
        {
            this.gameObject.SetActive(true);
            this.Refresh();
        }

        /// <summary>
        /// Ẩn đối tượng
        /// </summary>
        public void Hide()
        {
            this.gameObject.SetActive(false);
        }
        #endregion
    }
}
