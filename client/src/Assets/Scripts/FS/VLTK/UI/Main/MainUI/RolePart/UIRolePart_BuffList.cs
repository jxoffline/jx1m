using FS.GameEngine.Logic;
using FS.VLTK.Entities.Object;
using Server.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace FS.VLTK.UI.Main.MainUI.RolePart
{
    /// <summary>
    /// Bảng danh sách Buff
    /// </summary>
    public class UIRolePart_BuffList : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Nút gốc chứa các Buff bên trong
        /// </summary>
        [SerializeField]
        private RectTransform UIRect_BuffListRoot;

        /// <summary>
        /// Prefab thông tin Buff
        /// </summary>
        [SerializeField]
        private UIRolePart_BuffInfo UI_BuffInfo_Prefab;

        /// <summary>
        /// Khung thông tin chi tiết Buff
        /// </summary>
        [SerializeField]
        private UIRolePart_BuffDetailBox UI_BuffDetailBox;
        #endregion

        /// <summary>
        /// Danh sách Buff
        /// </summary>
        private readonly Dictionary<int, KeyValuePair<BuffData, UIRolePart_BuffInfo>> Buffs = new Dictionary<int, KeyValuePair<BuffData, UIRolePart_BuffInfo>>();

        #region Properties

        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi khi đối tượng được tạo ra
        /// </summary>
        private void Awake()
        {
            this.ClearAllBuffs();
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
            this.UI_BuffDetailBox.Hide();
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Luồng thực hiện công việc ở Frame tiếp theo
        /// </summary>
        /// <param name="skip"></param>
        /// <param name="work"></param>
        private IEnumerator ExecuteSkipFrames(int skip, Action work)
        {
            for (int i = 1; i <= skip; i++)
            {
                yield return null;
            }
            work?.Invoke();
        }

        /// <summary>
        /// Thực hiện vẽ lại giao diện ở Frame tiếp theo
        /// </summary>
        private void RebuildLayoutNextFrame()
        {
            this.StartCoroutine(this.ExecuteSkipFrames(1, () => {
                LayoutRebuilder.ForceRebuildLayoutImmediate(this.UIRect_BuffListRoot);
            }));
        }

        /// <summary>
        /// Xóa tất cả Buff trong danh sách hiển thị
        /// </summary>
        private void ClearAllBuffs()
        {
            this.Buffs.Clear();
            foreach (Transform child in this.UIRect_BuffListRoot)
            {
                if (child.gameObject != this.UI_BuffInfo_Prefab.gameObject)
                {
                    GameObject.Destroy(child.gameObject);
                }
            }
        }

        /// <summary>
        /// Cập nhật Buff mới
        /// </summary>
        /// <param name="buff"></param>
        private void UpdateBuffInfo(BuffData buff, out UIRolePart_BuffInfo displayNode)
        {
            UIRolePart_BuffInfo buffInfo = GameObject.Instantiate<UIRolePart_BuffInfo>(this.UI_BuffInfo_Prefab);
            buffInfo.gameObject.SetActive(true);
            buffInfo.transform.SetParent(this.UIRect_BuffListRoot, false);
            buffInfo.IconBundleDir = buff.IconBundleDir;
            buffInfo.IconAtlasName = buff.IconAtlasName;
            buffInfo.IconSpriteName = buff.IconSpriteName;
            buffInfo.DurationSecond = buff.DurationSecond;
            buffInfo.IconClick = () => {
                this.UI_BuffDetailBox.Position = buffInfo.transform.position;
                this.UI_BuffDetailBox.BuffName = buff.Name;
                //this.UI_BuffDetailBox.BuffDetail = buff.Description;
                this.UI_BuffDetailBox.BuffDetail = KTGlobal.GetBuffAttributeDescription(buff.RefObject);
                this.UI_BuffDetailBox.BuffLevel = buff.Level;
                this.UI_BuffDetailBox.IconBundleDir = buff.IconBundleDir;
                this.UI_BuffDetailBox.IconAtlasName = buff.IconAtlasName;
                this.UI_BuffDetailBox.IconSpriteName = buff.IconSpriteName;
                this.UI_BuffDetailBox.Show();
            };
            buffInfo.TimeOut = () => {
                this.RemoveBuff(buff.ID);
            };
            buffInfo.Refresh();
            displayNode = buffInfo;
        }

        /// <summary>
        /// Tìm Buff trong danh sách
        /// </summary>
        /// <param name="buff"></param>
        /// <returns></returns>
        private KeyValuePair<BuffData, UIRolePart_BuffInfo> FindBuff(int buffID)
        {
            if (this.Buffs.TryGetValue(buffID, out KeyValuePair<BuffData, UIRolePart_BuffInfo> pair))
            {
                return pair;
            }
            return default;
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Làm mới danh sách Buff hiện có
        /// </summary>
        public void RefreshDataList()
        {
            this.ClearAllBuffs();
            this.StartCoroutine(this.ExecuteSkipFrames(5, () => {
                if (Global.Data.RoleData != null && Global.Data.RoleData.BufferDataList != null)
                {
                    foreach (BufferData buffData in Global.Data.RoleData.BufferDataList)
                    {
                        this.AddBuff(buffData);
                    }
                }
            }));
        }

        /// <summary>
        /// Thêm Buff vào danh sách (Buff này gây ra bởi kỹ năng)
        /// </summary>
        /// <param name="buffData"></param>
        public void AddBuff(BufferData buffData)
        {
            long buffDurationSec = buffData.BufferSecs == -1 ? -1 : (buffData.BufferSecs - KTGlobal.GetServerTime() + buffData.StartTime) / 1000;
            /// Nếu tồn tại Buff cũ
            if (this.Buffs.TryGetValue(buffData.BufferID, out KeyValuePair<BuffData, UIRolePart_BuffInfo> buffNode))
            {
                buffNode.Key.RefObject = buffData;
                buffNode.Key.DurationSecond = buffDurationSec;
                if (Loader.Loader.Skills.TryGetValue(buffData.BufferID, out Entities.Config.SkillDataEx skillData))
                {
                    buffNode.Key.Name = string.Format("{0}", skillData.Name);
                }

                buffNode.Value.DurationSecond = buffDurationSec;
                buffNode.Value.RefreshTimer();

                this.RebuildLayoutNextFrame();
            }
            /// Nếu chưa tồn tại Buff
            else
            {
                if (Loader.Loader.Skills.TryGetValue(buffData.BufferID, out Entities.Config.SkillDataEx skillData))
                {
                    BuffData buff = new BuffData()
                    {
                        ID = buffData.BufferID,
                        Name = string.Format("{0}", skillData.Name),
                        Level = (int) buffData.BufferVal,
                        Description = skillData.FullDesc,
                        DurationSecond = buffDurationSec,
                        IconBundleDir = skillData.IconBundleDir,
                        IconAtlasName = skillData.IconAtlasName,
                        IconSpriteName = skillData.Icon,
                        RefObject = buffData,
                    };
                    this.UpdateBuffInfo(buff, out UIRolePart_BuffInfo buffInfo);
                    this.Buffs[buff.ID] = new KeyValuePair<BuffData, UIRolePart_BuffInfo>(buff, buffInfo);
                    buffInfo.ShowTimer = !skillData.IsArua;

                    this.RebuildLayoutNextFrame();
                }
            }
        }

        /// <summary>
        /// Xóa Buff khỏi danh sách
        /// </summary>
        /// <param name="skillID"></param>
        public void RemoveBuff(int skillID)
        {
            if (this.Buffs.TryGetValue(skillID, out KeyValuePair<BuffData, UIRolePart_BuffInfo> buffNode))
            {
                if (!buffNode.Equals(default) && buffNode.Key != null)
                {
                    GameObject.Destroy(buffNode.Value.gameObject);
                    this.Buffs.Remove(skillID);
                }

                this.RebuildLayoutNextFrame();
            }
        }
        #endregion
    }
}