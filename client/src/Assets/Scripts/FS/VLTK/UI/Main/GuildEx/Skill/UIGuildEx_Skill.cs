using FS.GameEngine.Logic;
using FS.VLTK.Entities;
using FS.VLTK.UI.Main.GuildEx.Skill;
using FS.VLTK.UI.Main.ItemBox;
using GameServer.VLTK.Utilities;
using Server.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using static FS.VLTK.Entities.Enum;

namespace FS.VLTK.UI.Main.GuildEx
{
    /// <summary>
    /// Khung kỹ năng bang hội
    /// </summary>
    public class UIGuildEx_Skill : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Button đóng khung
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_Close;

        /// <summary>
        /// Prefab button kỹ năng
        /// </summary>
        [SerializeField]
        private UIGuildEx_Skill_ButtonSkill UIButton_SkillPrefab;

        /// <summary>
        /// Text Label cấp hiện tại
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_CurrentLevelLabel;

        /// <summary>
        /// Text thuộc tính cấp hiện tại
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_CurrentLevelProperties;

        /// <summary>
        /// Text Label cấp kế tiếp
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_NextLevelLabel;

        /// <summary>
        /// Text thuộc tính cấp kế tiếp
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_NextLevelProperties;

        /// <summary>
        /// Text bang cống yêu cầu
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_LevelUpRequireMoney;

        /// <summary>
        /// Text cấp độ bang hội yêu cầu
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_LevelUpRequireGuildLevel;

        /// <summary>
        /// Prefab vật phẩm yêu cầu
        /// </summary>
        [SerializeField]
        private UIItemBox UI_RequireItemPrefab;

        /// <summary>
        /// Button thăng cấp kỹ năng bang hội
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_LevelUp;
        #endregion

        #region Properties
        private List<SkillDef> _Data;
        /// <summary>
        /// Danh sách kỹ năng hiện có
        /// </summary>
        public List<SkillDef> Data
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
        /// Sự kiện thăng cấp kỹ năng
        /// </summary>
        public Action<int> LevelUp { get; set; }
        #endregion

        #region Private fields
        /// <summary>
        /// RectTransform danh sách kỹ năng
        /// </summary>
        private RectTransform transformSkillList;

        /// <summary>
        /// RectTransform danh sách vật phẩm yêu cầu
        /// </summary>
        private RectTransform transformRequireItemList;

        /// <summary>
        /// ID kỹ năng được chọn
        /// </summary>
        private int selectedSkillID = -1;
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi khi đối tượng được tạo ra
        /// </summary>
        private void Awake()
        {
            this.transformSkillList = this.UIButton_SkillPrefab.transform.parent.GetComponent<RectTransform>();
            this.transformRequireItemList = this.UI_RequireItemPrefab.transform.parent.GetComponent<RectTransform>();
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
            this.UIButton_LevelUp.onClick.AddListener(this.ButtonLevelUp_Clicked);
        }

        /// <summary>
        /// Sự kiện khi Button đóng khung được ấn
        /// </summary>
        private void ButtonClose_Clicked()
        {
            this.Close?.Invoke();
        }

        /// <summary>
        /// Sự kiện khi Button thăng cấp kỹ năng bang hội được ấn
        /// </summary>
        private void ButtonLevelUp_Clicked()
        {
            /// Nếu không có bang hội
            if (Global.Data.RoleData.GuildID <= 0)
            {
                /// Bỏ qua
                KTGlobal.AddNotification("Bạn không có bang hội, không thể thực hiện thao tác này!");
                return;
            }
            /// Nếu không phải bang chủ hoặc phó bang chủ
            else if (Global.Data.RoleData.GuildRank != (int) GuildRank.Master && Global.Data.RoleData.GuildRank != (int) GuildRank.ViceMaster)
            {
                /// Bỏ qua
                KTGlobal.AddNotification("Chỉ có bang chủ hoặc phó bang chủ mới có thể thực hiện thao tác này!");
                return;
            }
            /// Nếu không có kỹ năng được chọn
            else if (this.selectedSkillID == -1)
            {
                /// Bỏ qua
                KTGlobal.AddNotification("Hãy chọn một kỹ năng sau đó mới tiến hành thăng cấp!");
                return;
            }

            /// Xác nhận lần nữa
            KTGlobal.ShowMessageBox("Thăng cấp kỹ năng", "Xác nhận thăng cấp kỹ năng này?", () =>
            {
                /// Thực thi sự kiện
                this.LevelUp?.Invoke(this.selectedSkillID);
            }, true);
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
            if (Global.Data.RoleData.GuildID <= 0)
            {
                /// Đóng khung
                this.Close?.Invoke();
                /// Bỏ qua
                return;
            }
            else if (this._Data == null)
            {
                /// Đóng khung
                this.Close?.Invoke();
                /// Bỏ qua
                return;
            }

            /// Làm rỗng danh sách vật phẩm yêu cầu
            foreach (Transform child in this.transformRequireItemList.transform)
            {
                if (child.gameObject != this.UI_RequireItemPrefab.gameObject)
                {
                    GameObject.Destroy(child.gameObject);
                }
            }

            /// UI Button kỹ năng đầu tiên
            UIGuildEx_Skill_ButtonSkill uiFirstButtonSkill = null;

            /// Làm rỗng danh sách kỹ năng
            foreach (Transform child in this.transformSkillList.transform)
            {
                if (child.gameObject != this.UIButton_SkillPrefab.gameObject)
                {
                    GameObject.Destroy(child.gameObject);
                }
            }
            /// Duyệt danh sách kỹ năng bang hội
            foreach (GuildSkill guildSkillData in Loader.Loader.GuildConfig.Skills)
            {
                /// Tạo mới
                UIGuildEx_Skill_ButtonSkill uiButtonSkill = GameObject.Instantiate<UIGuildEx_Skill_ButtonSkill>(this.UIButton_SkillPrefab);
                uiButtonSkill.transform.SetParent(this.transformSkillList, false);
                uiButtonSkill.gameObject.SetActive(true);
                /// ID kỹ năng
                uiButtonSkill.SkillID = guildSkillData.ID;
                /// Cấp độ kỹ năng
                int skillLevel = 0;
                /// Thông tin tương ứng
                SkillDef gsSkillData = this._Data.Where(x => x.SkillID == guildSkillData.ID).FirstOrDefault();
                /// Tìm thấy
                if (gsSkillData != null)
                {
                    /// Thiết lập cấp độ tương ứng
                    skillLevel = gsSkillData.Level;
                }
                uiButtonSkill.Level = skillLevel;
                /// Sự kiện
                uiButtonSkill.Select = () =>
                {
                    /// Làm rỗng danh sách vật phẩm yêu cầu
                    foreach (Transform child in this.transformRequireItemList.transform)
                    {
                        if (child.gameObject != this.UI_RequireItemPrefab.gameObject)
                        {
                            GameObject.Destroy(child.gameObject);
                        }
                    }

                    /// Đánh dấu kỹ năng được chọn
                    this.selectedSkillID = guildSkillData.ID;

                    /// Thông tin thăng cấp kỹ năng hiện tại
                    GuildSkillLevelData currentLevelkillLevelData = guildSkillData.LevelData.Where(x => x.Level == skillLevel).FirstOrDefault();
                    /// Thông tin thăng cấp kỹ năng kế tiếp
                    GuildSkillLevelData nextLevelSkillLevelData = guildSkillData.LevelData.Where(x => x.Level == skillLevel + 1).FirstOrDefault();

                    /// Nếu không tồn tại
                    if (currentLevelkillLevelData == null)
                    {
                        /// Thiết lập mô tả cấp kế tiếp
                        this.UIText_CurrentLevelLabel.text = string.Format("Cấp hiện tại: {0}", "Không có");
                        /// Thiết lập mô tả cấp kế tiếp
                        this.UIText_CurrentLevelProperties.text = "";
                        /// Thăng cấp yêu cầu
                        this.UIText_LevelUpRequireGuildLevel.text = "0";
                        this.UIText_LevelUpRequireMoney.text = "0";
                    }
                    else
                    {
                        /// Cấp hiện tại
                        this.UIText_CurrentLevelLabel.text = string.Format("Cấp hiện tại: {0}", skillLevel);
                        /// Mô tả cấp hiện tại
                        StringBuilder currentLevelPropertiesString = new StringBuilder();
                        /// Duyệt danh sách thuộc tính cấp hiện tại
                        foreach (SkillProperty skillProperty in currentLevelkillLevelData.SkillProperties)
                        {
                            MAGIC_ATTRIB symbol = MAGIC_ATTRIB.magic_item_begin;
                            /// Thông tin Symbol
                            if (PropertyDefine.PropertiesBySymbolName.TryGetValue(skillProperty.Type, out PropertyDefine.Property property))
                            {
                                symbol = (MAGIC_ATTRIB) property.ID;
                            }
                            /// Toác
                            if (symbol == MAGIC_ATTRIB.magic_item_begin)
                            {
                                currentLevelPropertiesString.AppendLine("Symbol not found: " + skillProperty.Type);
                                /// Bỏ qua
                                continue;
                            }
                            /// Chuyển về KMagicAttrib
                            KMagicAttrib attrib = new KMagicAttrib()
                            {
                                nAttribType = symbol,
                                nType = new short[] { 0, 0, 0 },
                                nValue = new int[]
                                {
                                    skillProperty.Value1,
                                    skillProperty.Value2,
                                    skillProperty.Value3,
                                },
                            };
                            currentLevelPropertiesString.AppendLine(KTGlobal.GetAttributeDescription(attrib));
                        }
                        /// Thiết lập mô tả cấp hiện tại
                        this.UIText_CurrentLevelProperties.text = currentLevelPropertiesString.ToString();

                        /// Thăng cấp yêu cầu
                        this.UIText_LevelUpRequireGuildLevel.text = currentLevelkillLevelData.Level.ToString();
                        this.UIText_LevelUpRequireMoney.text = KTGlobal.GetDisplayMoney(currentLevelkillLevelData.RequireMoney);
                        /// Duyệt danh sách vật phẩm yêu cầu
                        foreach (ItemRequest item in currentLevelkillLevelData.RequireItems)
                        {
                            /// Thông tin vật phẩm
                            if (Loader.Loader.Items.TryGetValue(item.ItemID, out Entities.Config.ItemData itemData))
                            {
                                /// Tạo vật phẩm ảo tương ứng
                                GoodsData itemGD = KTGlobal.CreateItemPreview(itemData);
                                /// Số lượng
                                itemGD.GCount = item.Quantity;
                                /// Tạo mới ô vật phẩm
                                UIItemBox uiItemBox = GameObject.Instantiate<UIItemBox>(this.UI_RequireItemPrefab);
                                uiItemBox.transform.SetParent(this.transformRequireItemList, false);
                                uiItemBox.gameObject.SetActive(true);
                                uiItemBox.Data = itemGD;
                                uiItemBox.Refresh();
                            }
                        }
                        /// Xây lại giao diện
                        this.ExecuteSkipFrames(1, () =>
                        {
                            UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(this.transformSkillList);
                        });

                        /// Kích hoạt Button thăng cấp
                        this.UIButton_LevelUp.gameObject.SetActive(true);
                    }

                    /// Nếu không tồn tại cấp kế tiếp
                    if (nextLevelSkillLevelData == null)
                    {
                        /// Thiết lập mô tả cấp kế tiếp
                        this.UIText_NextLevelLabel.text = string.Format("Cấp kế tiếp: {0}", "Tối đa");
                        /// Thiết lập mô tả cấp kế tiếp
                        this.UIText_NextLevelProperties.text = "";
                    }
                    else
                    {
                        /// Thiết lập mô tả cấp kế tiếp
                        this.UIText_NextLevelLabel.text = string.Format("Cấp kế tiếp: {0}", skillLevel + 1);
                        /// Mô tả cấp kế tiếp
                        StringBuilder nextLevelPropertiesString = new StringBuilder();
                        /// Duyệt danh sách thuộc tính cấp hiện tại
                        foreach (SkillProperty skillProperty in nextLevelSkillLevelData.SkillProperties)
                        {
                            MAGIC_ATTRIB symbol = MAGIC_ATTRIB.magic_item_begin;
                            /// Thông tin Symbol
                            if (PropertyDefine.PropertiesBySymbolName.TryGetValue(skillProperty.Type, out PropertyDefine.Property property))
                            {
                                symbol = (MAGIC_ATTRIB) property.ID;
                            }
                            /// Toác
                            if (symbol == MAGIC_ATTRIB.magic_item_begin)
                            {
                                nextLevelPropertiesString.AppendLine("Symbol not found: " + skillProperty.Type);
                                /// Bỏ qua
                                continue;
                            }
                            /// Chuyển về KMagicAttrib
                            KMagicAttrib attrib = new KMagicAttrib()
                            {
                                nAttribType = symbol,
                                nType = new short[] { 0, 0, 0 },
                                nValue = new int[]
                                {
                                skillProperty.Value1,
                                skillProperty.Value2,
                                skillProperty.Value3,
                                },
                            };
                            nextLevelPropertiesString.AppendLine(KTGlobal.GetAttributeDescription(attrib));
                        }
                        /// Thiết lập mô tả cấp kế tiếp
                        this.UIText_NextLevelProperties.text = nextLevelPropertiesString.ToString();
                    }

                    /// Xây lại giao diện
                    this.ExecuteSkipFrames(1, () =>
                    {
                        UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(this.transformRequireItemList);
                    });
                };

                /// Nếu chưa có Button đầu tiên
                if (uiFirstButtonSkill == null)
                {
                    /// Đánh dấu là Button đầu tiên
                    uiFirstButtonSkill = uiButtonSkill;
                    /// Chọn Button kỹ năng đầu tiên
                    uiFirstButtonSkill.Active = true;
                }
            }
            /// Xây lại giao diện
            this.ExecuteSkipFrames(1, () =>
            {
                UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(this.transformSkillList);
            });

            /// Hủy Button thăng cấp
            this.UIButton_LevelUp.gameObject.SetActive((Global.Data.RoleData.GuildRank == (int) GuildRank.Master || Global.Data.RoleData.GuildRank == (int) GuildRank.ViceMaster) && uiFirstButtonSkill != null);
        }
        #endregion
    }
}
