using FS.GameEngine.Logic;
using FS.VLTK.Entities.Config;
using Server.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FS.VLTK.UI.Main.SkillQuickKeyChooser
{
    /// <summary>
    /// Khung chọn kỹ năng và thiết lập vào danh sách kỹ năng nhanh
    /// </summary>
    public class UISkillQuickKeyChooser : MonoBehaviour
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
        private UISkillQuickKeyChooser_Item UIButton_SkillPrefab;
        #endregion

        #region Properties
        /// <summary>
        /// Loại
        /// <para>0: Kỹ năng thường, 1: Vòng sáng</para>
        /// </summary>
        public int Type { get; set; } = 0;

        /// <summary>
        /// Sự kiện khi người chơi ấn chọn kỹ năng trong danh sách
        /// </summary>
        public Action<int> SkillClick { get; set; }

        /// <summary>
        /// Sự kiện đóng khung
        /// </summary>
        public Action Close { get; set; }
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi ở Frame đầu tiên
        /// </summary>
        private void Start()
        {
            this.InitPrefabs();
            this.RefreshSkillList();
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
            GameObject.Destroy(this.gameObject);
            this.Close?.Invoke();
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Làm rỗng danh sách kỹ năng
        /// </summary>
        private void ClearSkillList()
        {
            foreach (Transform child in this.UIButton_SkillPrefab.transform.parent)
            {
                if (child.gameObject != this.UIButton_SkillPrefab.gameObject)
                {
                    GameObject.Destroy(child);
                }
            }
        }

        /// <summary>
        /// Làm mới danh sách kỹ năng
        /// </summary>
        private void RefreshSkillList()
        {
            this.ClearSkillList();

            /// Nếu danh sách kỹ năng rỗng
            if (Global.Data.RoleData.SkillDataList == null)
            {
                return;
            }

            /// Nếu là kỹ năng thường
            if (this.Type == 0)
            {
                foreach (SkillData skill in Global.Data.RoleData.SkillDataList)
                {
                    if (Loader.Loader.Skills.TryGetValue(skill.SkillID, out SkillDataEx skillData))
                    {
                        /// Nếu không phải kỹ năng bị động và vòng sáng
                        if (skill.Level > 0 && !skillData.IsArua && KTGlobal.IsCanUseSkill(skillData))
                        {
                            //KTDebug.LogError(skillData.Name);
                            UISkillQuickKeyChooser_Item item = GameObject.Instantiate<UISkillQuickKeyChooser_Item>(this.UIButton_SkillPrefab);
                            item.gameObject.SetActive(true);
                            item.transform.SetParent(this.UIButton_SkillPrefab.transform.parent, false);

                            item.IconBundleDir = skillData.IconBundleDir;
                            item.IconAtlasName = skillData.IconAtlasName;
                            item.IconSpriteName = skillData.Icon;
                            item.SkillName = skillData.Name;
                            item.Click = () =>
                            {
                                this.SkillClick?.Invoke(skill.SkillID);
                            };
                            item.Load();
                        }
                    }
                }
            }
            /// Nếu là kỹ năng vòng sáng
            else
            {
                foreach (SkillData skill in Global.Data.RoleData.SkillDataList)
                {
                    if (Loader.Loader.Skills.TryGetValue(skill.SkillID, out SkillDataEx skillData))
                    {
                        /// Nếu không phải kỹ năng bị động và phải là vòng sáng
                        if (skill.Level > 0 && skillData.IsArua && KTGlobal.IsCanUseSkill(skillData))
                        {
                            UISkillQuickKeyChooser_Item item = GameObject.Instantiate<UISkillQuickKeyChooser_Item>(this.UIButton_SkillPrefab);
                            item.gameObject.SetActive(true);
                            item.transform.SetParent(this.UIButton_SkillPrefab.transform.parent, false);

                            item.IconBundleDir = skillData.IconBundleDir;
                            item.IconAtlasName = skillData.IconAtlasName;
                            item.IconSpriteName = skillData.Icon;
                            item.SkillName = skillData.Name;
                            item.Click = () =>
                            {
                                this.SkillClick?.Invoke(skill.SkillID);
                            };
                            item.Load();
                        }
                    }
                }
            }
        }
        #endregion
    }
}

