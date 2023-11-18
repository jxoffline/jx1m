using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using Server.Data;
using FS.GameEngine.Logic;
using FS.VLTK.Utilities.UnityUI;
using System.Linq;
using FS.VLTK.Entities.Config;

namespace FS.VLTK.UI.Main.SkillTree
{
    /// <summary>
    /// Khung kỹ năng nhân vật
    /// </summary>
    public class UISkillTree : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Button đóng khung
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_Close;

        /// <summary>
        /// Prefab Tab Header
        /// </summary>
        [SerializeField]
        private UIToggleSprite UIToggle_TabHeaderPrefab;

        /// <summary>
        /// Nút gốc chứa các ô kỹ năng bên trong
        /// </summary>
        [SerializeField]
        private RectTransform UITransform_Root;

        /// <summary>
        /// Prefab ô kỹ năng
        /// </summary>
        [SerializeField]
        private SkillTree_SkillItemBox UISkill_BoxPrefab;

        /// <summary>
        /// Button xác nhận
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_Accept;

        /// <summary>
        /// Text điểm kỹ năng
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_SkillPoints;

        /// <summary>
        /// ScrollBar chứa danh sách kỹ năng
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.ScrollRect UIScrollBar_SkillList;

        /// <summary>
        /// Button thiết lập kỹ năng đánh tay
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_SetHandSkill;

        /// <summary>
        /// Button thiết lập kỹ năng tu luyện
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_SetExpSkill;
        #endregion

        #region Properties
        /// <summary>
        /// Xác nhận cộng điểm
        /// </summary>
        public Action<Dictionary<int, int>> Accept { get; set; }

        /// <summary>
        /// Đóng khung
        /// </summary>
        public Action Close { get; set; }

        /// <summary>
        /// Sự kiện mở khung thiết lập kỹ năng tay
        /// </summary>
        public Action SetHandSkill { get; set; }

        /// <summary>
        /// Sự kiện mở khung thiết lập kỹ năng tu luyện
        /// </summary>
        public Action SetExpSkill { get; set; }

        /// <summary>
        /// ID nhánh đang được chọn
        /// </summary>
        public int SelectedSubID
        {
            get
            {
                if (this.selectedSub == null)
                {
                    return 0;
                }

                return this.selectedSub.ID;
            }
        }
        #endregion

        #region Private fields
        /// <summary>
        /// Biến tạm lưu số điểm kỹ năng còn lại
        /// </summary>
        private int tempSkillPointsLeft = 0;

        /// <summary>
        /// Nhánh đang được chọn
        /// </summary>
        private FactionXML.Sub selectedSub = null;

        /// <summary>
        /// Tab đang được chọn
        /// </summary>
        private UIToggleSprite selectedTab = null;

        /// <summary>
        /// Vị trí ScrollView trước đó
        /// </summary>
        private float lastScrollValue = 0f;
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
            this.UIButton_Close.onClick.AddListener(this.ButtonClose_Clicked);
            this.UIButton_Accept.onClick.AddListener(this.ButtonAccept_Clicked);
            this.UIButton_SetHandSkill.onClick.AddListener(this.ButtonSetHandSkill_Clicked);
            this.UIButton_SetExpSkill.onClick.AddListener(this.ButtonSetExpSkill_Clicked);
        }

        /// <summary>
        /// Sự kiện khi Button đóng khung được ấn
        /// </summary>
        private void ButtonClose_Clicked()
        {
            if (this.IsContentChanged())
            {
                KTGlobal.ShowMessageBox("Điểm cộng vào kỹ năng vẫn <color=yellow>chưa được lưu</color>, nếu xác nhận <color=yellow>thoát</color> bạn sẽ <color=green>mất toàn bộ</color> số điểm <color=orange>đã phân phối</color>. Xác nhận <color=yellow>thoát</color>?", () => {
                    GameObject.Destroy(this.gameObject);
                    this.Close?.Invoke();
                }, true);
            }
            else
            {
                GameObject.Destroy(this.gameObject);
                this.Close?.Invoke();
            }
        }

        /// <summary>
        /// Sự kiện khi Button thiết lập kỹ năng tay được ấn
        /// </summary>
        private void ButtonSetHandSkill_Clicked()
		{
            this.SetHandSkill?.Invoke();
		}

        /// <summary>
        /// Sự kiện khi Button thiết lập kỹ năng tu luyện được ấn
        /// </summary>
        private void ButtonSetExpSkill_Clicked()
        {
            this.SetExpSkill?.Invoke();
        }

        /// <summary>
        /// Sự kiện khi Button đồng ý được ấn
        /// </summary>
        private void ButtonAccept_Clicked()
        {
            /// Lưu lại vị trí lần trước của ScrollBar
            this.lastScrollValue = this.UIScrollBar_SkillList.verticalNormalizedPosition;
            /// Xác nhận cộng điểm
            this.DoAcceptAddPoint();
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Thực hiện đồng ý cộng điểm
        /// </summary>
        private void DoAcceptAddPoint()
        {
            Dictionary<int, int> listChangedSkills = new Dictionary<int, int>();
            foreach (Transform child in this.UITransform_Root.transform)
            {
                if (child.gameObject != this.UISkill_BoxPrefab.gameObject)
                {
                    SkillTree_SkillItemBox skillItemBox = child.GetComponent<SkillTree_SkillItemBox>();
                    if (skillItemBox.ContentChanged)
                    {
                        listChangedSkills[skillItemBox.SkillID] = skillItemBox.TempSkillLevel - skillItemBox.SkillLevel;
                    }
                }
            }
            
            if (listChangedSkills.Count <= 0)
            {
                return;
            }
            this.Accept?.Invoke(listChangedSkills);
        }

        /// <summary>
        /// Làm mới hiển thị của Button chấp nhận
        /// </summary>
        private void RefreshButtonAcceptVisible()
        {
            this.UIButton_Accept.interactable = this.IsContentChanged();
        }

        /// <summary>
        /// Điểm cộng vào các ô đã có sự thay đổi
        /// </summary>
        /// <returns></returns>
        private bool IsContentChanged()
        {
            foreach (Transform child in this.UITransform_Root.transform)
            {
                if (child.gameObject != this.UISkill_BoxPrefab.gameObject)
                {
                    if (child.GetComponent<SkillTree_SkillItemBox>().ContentChanged)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Làm rỗng dữ liệu Tab
        /// </summary>
        private void ClearTabList()
        {
            foreach (Transform child in this.UIToggle_TabHeaderPrefab.transform.parent)
            {
                if (child.gameObject != this.UIToggle_TabHeaderPrefab.gameObject)
                {
                    GameObject.Destroy(child.gameObject);
                }
            }
        }

        /// <summary>
        /// Xây danh sách các Tab
        /// </summary>
        private void BuildTabList()
        {
            this.ClearTabList();

            bool isFirstInit = true;

            UIToggleSprite generalTabHeader = GameObject.Instantiate<UIToggleSprite>(this.UIToggle_TabHeaderPrefab);
            generalTabHeader.gameObject.transform.SetParent(this.UIToggle_TabHeaderPrefab.transform.parent, false);
            generalTabHeader.gameObject.SetActive(true);
            generalTabHeader.Name = "Chung";
            generalTabHeader.OnSelected = (isSelected) => {
                if (isSelected)
                {
                    if (this.IsContentChanged() && this.selectedTab != generalTabHeader && !isFirstInit)
                    {
                        KTGlobal.ShowMessageBox("Điểm cộng vào kỹ năng vẫn <color=yellow>chưa được lưu</color>, nếu xác nhận <color=yellow>chuyển Tab</color> bạn sẽ <color=green>mất toàn bộ</color> số điểm <color=orange>đã phân phối</color>. Xác nhận chuyển?", () => {
                            this.BuildSkillListBySub(0);
                            this.tempSkillPointsLeft = Global.Data.RoleData.SkillPoint;
                            this.UIText_SkillPoints.text = this.tempSkillPointsLeft.ToString();

                            this.selectedSub = null;
                            this.selectedTab = generalTabHeader;
                        }, () => {
                            this.selectedTab.Active = true;
                        });
                    }
                    else if (this.selectedTab != generalTabHeader)
                    {
                        this.BuildSkillListBySub(0);
                        this.tempSkillPointsLeft = Global.Data.RoleData.SkillPoint;
                        this.UIText_SkillPoints.text = this.tempSkillPointsLeft.ToString();

                        this.selectedSub = null;
                        this.selectedTab = generalTabHeader;
                    }

                    if (isFirstInit)
                    {
                        isFirstInit = false;
                    }
                }
            };
            if (this.selectedSub == null)
            {
                this.StartCoroutine(this.RunSkipFrame(1, () => {
                    isFirstInit = true;
                    generalTabHeader.Active = true;
                }));
            }

            if (Loader.Loader.Factions.TryGetValue(Global.Data.RoleData.FactionID, out FactionXML faction))
            {
                foreach (FactionXML.Sub sub in faction.Subs.Values)
                {
                    UIToggleSprite subTabHeader = GameObject.Instantiate<UIToggleSprite>(this.UIToggle_TabHeaderPrefab);
                    subTabHeader.gameObject.transform.SetParent(this.UIToggle_TabHeaderPrefab.transform.parent, false);
                    subTabHeader.gameObject.SetActive(true);
                    subTabHeader.Name = sub.Name;
                    subTabHeader.OnSelected = (isSelected) => {
                        if (isSelected)
                        {
                            if (this.IsContentChanged() && this.selectedTab != subTabHeader && !isFirstInit)
                            {
                                KTGlobal.ShowMessageBox("Điểm cộng vào kỹ năng vẫn <color=yellow>chưa được lưu</color>, nếu xác nhận <color=yellow>chuyển Tab</color> bạn sẽ <color=green>mất toàn bộ</color> số điểm <color=orange>đã phân phối</color>. Xác nhận chuyển?", () => {
                                    this.BuildSkillListBySub(sub.ID);
                                    this.tempSkillPointsLeft = Global.Data.RoleData.SkillPoint;
                                    this.UIText_SkillPoints.text = this.tempSkillPointsLeft.ToString();

                                    this.selectedSub = sub;
                                    this.selectedTab = subTabHeader;
                                }, () => {
                                    this.selectedTab.Active = true;
                                });
                            }
                            else if (this.selectedTab != subTabHeader)
                            {
                                this.BuildSkillListBySub(sub.ID);
                                this.tempSkillPointsLeft = Global.Data.RoleData.SkillPoint;
                                this.UIText_SkillPoints.text = this.tempSkillPointsLeft.ToString();

                                this.selectedSub = sub;
                                this.selectedTab = subTabHeader;
                            }

                            if (isFirstInit)
                            {
                                isFirstInit = false;
                            }
                        }
                    };
                    if (this.selectedSub == sub)
                    {
                        this.StartCoroutine(this.RunSkipFrame(1, () => {
                            isFirstInit = true;
                            subTabHeader.Active = true;
                        }));
                    }
                }
            }
        }

        /// <summary>
        /// Xây danh sách kỹ năng theo nhánh
        /// <param name="subID"></param>
        /// <param name="enableAddAndSubPoint"></param>
        /// </summary>
        private void BuildSkillListBySub(int subID)
        {
            this.ClearSkillList();
            if (Loader.Loader.Factions.TryGetValue(Global.Data.RoleData.FactionID, out FactionXML faction))
            {
                /// <summary>
                /// Kiểm tra có nhánh nào chứa kỹ năng ID tương ứng không
                /// <param name="skillID"></param>
                /// </summary>
                bool IsSubContainsSkill(int skillID)
                {
                    foreach (FactionXML.Sub sub in faction.Subs.Values)
                    {
                        if (sub.SkillIDs.Contains(skillID))
                        {
                            return true;
                        }
                    }
                    return false;
                }

                if (subID == 0)
                {
                    List<SkillData> skills = Global.Data.RoleData.SkillDataList.Where(x => this.GetSkillData(x.SkillID) != null && !IsSubContainsSkill(x.SkillID)).ToList();
                    skills.Sort((s1, s2) => {
                        return this.GetSkillData(s1.SkillID).RequireLevel - this.GetSkillData(s2.SkillID).RequireLevel;
                    });
                    this.BuildSkillList(skills, subID);
                }
                else
                {
                    if (faction.Subs.TryGetValue(subID, out FactionXML.Sub sub))
                    {
                        List<SkillData> skills = Global.Data.RoleData.SkillDataList.Where(x => this.GetSkillData(x.SkillID) != null && sub.SkillIDs.Contains(x.SkillID)).ToList();
                        skills.Sort((s1, s2) => {
                            /// Cấp độ yêu cầu kỹ năng 1
                            int skill1RequireLevel = this.GetSkillData(s1.SkillID).RequireLevel;
                            /// Cấp độ yêu cầu kỹ năng 2
                            int skill2RequireLevel = this.GetSkillData(s2.SkillID).RequireLevel;

                            /// Nếu cấp độ yêu cầu khác nhau
                            if (skill1RequireLevel != skill2RequireLevel)
                            {
                                /// Trả về độ lệch
                                return skill1RequireLevel - skill2RequireLevel;
                            }

                            /// Thứ tự xuất hiện trong nhánh kỹ năng 1
                            int skill1Order = sub.SkillIDs.IndexOf(s1.SkillID);
                            /// Thứ tự xuất hiện trong nhánh kỹ năng 2
                            int skill2Order = sub.SkillIDs.IndexOf(s2.SkillID);

                            /// Trả về độ lệch thứ tự
                            return skill1Order - skill2Order;
                        });
                        this.BuildSkillList(skills, subID);
                    }
                }
            }
        }

        /// <summary>
        /// Làm sạch danh sách kỹ năng trong khung
        /// </summary>
        private void ClearSkillList()
        {
            foreach (Transform child in this.UITransform_Root.transform)
            {
                if (child.gameObject != this.UISkill_BoxPrefab.gameObject)
                {
                    GameObject.Destroy(child.gameObject);
                }
            }
        }

        /// <summary>
        /// Xây danh sách kỹ năng tương ứng
        /// </summary>
        /// <param name="skills"></param>
        /// <param name="subID"></param>
        private void BuildSkillList(List<SkillData> skills, int subID)
        {
            /// Kỹ năng tân thủ tương ứng vũ khí
            SkillDataEx newbieAttackSkill = KTGlobal.GetNewbieSkillCorrespondingToCurrentWeapon();

            foreach (SkillData skill in skills)
            {
                if (!Loader.Loader.Skills.TryGetValue(skill.SkillID, out SkillDataEx skillInfo))
                {
                    continue;
                }
                /// Nếu là kỹ năng tân thủ nhưng loại vũ khí không phải vũ khí đang cầm
                else if (newbieAttackSkill != null && KTGlobal.ListNewbieAttackSkill.Contains(skill.SkillID) && newbieAttackSkill.ID != skill.SkillID)
                {
                    continue;
                }
                /// Nếu không tồn tại kỹ năng tân thủ tương ứng vũ khí tức đang ở trạng thái tay không, và kỹ năng tân thủ này khác kỹ năng đánh thường quyền
                else if (newbieAttackSkill == null && KTGlobal.ListNewbieAttackSkill.Contains(skill.SkillID) && skill.SkillID != KTGlobal.NewbieHandAttackSkill)
                {
                    continue;
                }

                SkillTree_SkillItemBox skillItem = GameObject.Instantiate<SkillTree_SkillItemBox>(this.UISkill_BoxPrefab);
                skillItem.transform.SetParent(this.UITransform_Root, false);
                skillItem.gameObject.SetActive(true);

                skillItem.SkillID = skillInfo.ID;
                skillItem.IconBundleDir = skillInfo.IconBundleDir;
                skillItem.IconAtlasName = skillInfo.IconAtlasName;
                skillItem.IconSpriteName = skillInfo.Icon;
                skillItem.SkillName = skillInfo.Name;
                skillItem.SkillShortDesc = skillInfo.ShortDesc;
                skillItem.SkillLevel = skill.SkillLevel;
                skillItem.AdditionLevel = skill.BonusLevel;
                int skillMaxRequireLevel = skillInfo.RequireLevel + skillInfo.MaxSkillLevel - 1;
                if (skillMaxRequireLevel <= Global.Data.RoleData.Level)
                {
                    skillItem.SkillMaxLevel = skillInfo.MaxSkillLevel;
                }
                else
                {
                    skillItem.SkillMaxLevel = Global.Data.RoleData.Level - skillInfo.RequireLevel + 1;
                }
                //KTDebug.LogError(skillInfo.Name + " - Require Lv: " + skillInfo.RequireLevel + " - Current can study up to level of skill = " + skillItem.SkillMaxLevel);
                if (subID == 0)
                {
                    skillItem.CanStudy = false;
                }
                //else if ((Global.Data.RoleData.SubID == 0 || subID == Global.Data.RoleData.SubID) && skill.CanStudy)
                //{
                //    skillItem.CanStudy = true;
                //}
                //else
                //{
                //    skillItem.CanStudy = false;
                //}
                else
                {
                    skillItem.CanStudy = skillInfo.CanAddPoint;
                }
                skillItem.PointAdd = () => {
                    if (!skillItem.CanStudy)
                    {
                        return;
                    }

                    this.tempSkillPointsLeft--;
                    this.UIText_SkillPoints.text = this.tempSkillPointsLeft.ToString();
                    this.UpdateSkillPointsToAllSkillItems();
                    this.RefreshButtonAcceptVisible();
                };
                skillItem.PointSub = () => {
                    if (!skillItem.CanStudy)
                    {
                        return;
                    }

                    this.tempSkillPointsLeft++;
                    this.UIText_SkillPoints.text = this.tempSkillPointsLeft.ToString();
                    this.UpdateSkillPointsToAllSkillItems();
                    this.RefreshButtonAcceptVisible();
                };
                skillItem.ShowSkillInfo = () => {
                    KTGlobal.ShowSkillItemInfo(skillInfo, skill.SkillLevel, skill.SkillLevel + skill.BonusLevel);
                };
                skillItem.Refresh();
            }

            this.StartCoroutine(this.RunSkipFrame(1, () => {
                this.UpdateSkillPointsToAllSkillItems();
                this.RefreshButtonAcceptVisible();
            }));
        }

        /// <summary>
        /// Thực hiện chức năng ở Frame tiếp theo
        /// </summary>
        /// <returns></returns>
        private IEnumerator RunSkipFrame(int skipCount, Action function)
        {
            for (int i = 1; i <= skipCount; i++)
            {
                yield return null;
            }
            function?.Invoke();
        }

        /// <summary>
        /// Cập nhật thông báo điểm kỹ năng đã thay đổi cho tất cả các ô kỹ năng con
        /// </summary>
        private void UpdateSkillPointsToAllSkillItems()
        {
            foreach (Transform child in this.UITransform_Root.transform)
            {
                if (child.gameObject != this.UISkill_BoxPrefab.gameObject)
                {
                    child.GetComponent<SkillTree_SkillItemBox>().SkillPointBackToZero = this.tempSkillPointsLeft <= 0;
                }
            }
        }

        /// <summary>
        /// Trả về dữ liệu kỹ năng tương ứng
        /// </summary>
        /// <param name="skillID"></param>
        /// <returns></returns>
        private SkillDataEx GetSkillData(int skillID)
        {
            if (Loader.Loader.Skills.TryGetValue(skillID, out SkillDataEx skill))
            {
                return skill;
            }
            return null;
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Làm mới danh sách kỹ năng
        /// </summary>
        public void RefreshSkillData()
        {
            this.tempSkillPointsLeft = Global.Data.RoleData.SkillPoint;
            this.UIText_SkillPoints.text = this.tempSkillPointsLeft.ToString();
            this.BuildTabList();

            /// Cập nhật lại vị trí của ScrollBar
            this.StartCoroutine(this.RunSkipFrame(2, () => {
                this.UIScrollBar_SkillList.verticalNormalizedPosition = this.lastScrollValue;
            }));
        }
        #endregion
    }
}
