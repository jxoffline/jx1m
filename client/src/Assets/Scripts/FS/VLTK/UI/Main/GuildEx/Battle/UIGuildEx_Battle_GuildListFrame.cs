using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FS.VLTK.UI.Main.GuildEx.Battle
{
    /// <summary>
    /// Khung danh sách bang hội đăng ký tham gia công thành chiến
    /// </summary>
    public class UIGuildEx_Battle_GuildListFrame : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Button đóng khung
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_Close;

        /// <summary>
        /// Prefab thông tin bang hội
        /// </summary>
        [SerializeField]
        private UIGuildEx_Battle_GuildListFrame_GuildInfo UI_GuildInfoPrefab;
        #endregion

        #region Properties
        /// <summary>
        /// Danh sách thành viên
        /// </summary>
        public List<string> Data { get; set; }
        #endregion

        #region Private fields
        /// <summary>
        /// RectTransform danh sách bang hội
        /// </summary>
        private RectTransform transformGuildList;
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi khi đối tượng được tạo ra
        /// </summary>
        private void Awake()
        {
            this.transformGuildList = this.UI_GuildInfoPrefab.transform.parent.GetComponent<RectTransform>();
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
            this.Hide();
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
            /// Xóa toàn bộ thành viên cũ
            foreach (Transform child in this.transformGuildList.transform)
            {
                if (child.gameObject != this.UI_GuildInfoPrefab.gameObject)
                {
                    GameObject.Destroy(child.gameObject);
                }
            }

            /// Nếu có danh sách bang
            if (this.Data != null)
            {
                /// Duyệt danh sách bang
                foreach (string guildName in this.Data)
                {
                    /// Thêm vào danh sách
                    UIGuildEx_Battle_GuildListFrame_GuildInfo uiGuildInfo = GameObject.Instantiate<UIGuildEx_Battle_GuildListFrame_GuildInfo>(this.UI_GuildInfoPrefab);
                    uiGuildInfo.transform.SetParent(this.transformGuildList, false);
                    uiGuildInfo.gameObject.SetActive(true);
                    uiGuildInfo.Data = guildName;
                }
            }

            /// Xây lại giao diện
            this.ExecuteSkipFrames(1, () =>
            {
                UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(this.transformGuildList);
            });
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Hiện khung
        /// </summary>
        public void Show()
        {
            /// Kích hoạt
            this.gameObject.SetActive(true);
            /// Làm mới dữ liệu
            this.RefreshData();
        }

        /// <summary>
        /// Ẩn khung
        /// </summary>
        public void Hide()
        {
            /// Ẩn
            this.gameObject.SetActive(false);
        }
        #endregion
    }
}
