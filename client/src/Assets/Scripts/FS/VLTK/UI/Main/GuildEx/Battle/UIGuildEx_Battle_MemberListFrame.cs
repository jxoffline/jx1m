using Server.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FS.VLTK.UI.Main.GuildEx.Battle
{
    /// <summary>
    /// Khung danh sách thành viên bang hội để chọn tham gia công thành chiến
    /// </summary>
    public class UIGuildEx_Battle_MemberListFrame : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Button đóng khung
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_Close;

        /// <summary>
        /// Prefab thông tin thành viên
        /// </summary>
        [SerializeField]
        private UIGuildEx_Battle_MemberListFrame_MemberInfo UI_MemberInfoPrefab;
        #endregion

        #region Properties
        private List<MemberRegister> _Data;
        /// <summary>
        /// Danh sách thành viên
        /// </summary>
        public List<MemberRegister> Data
        {
            get
            {
                return this._Data;
            }
            set
            {
                this._Data = value;
            }
        }

        /// <summary>
        /// Sự kiện chọn thành viên
        /// </summary>
        public Action<MemberRegister> Select { get; set; }
        #endregion

        #region Private fields
        /// <summary>
        /// RectTransform danh sách thành viên
        /// </summary>
        private RectTransform transformMemberList;
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi khi đối tượng được tạo ra
        /// </summary>
        private void Awake()
        {
            this.transformMemberList = this.UI_MemberInfoPrefab.transform.parent.GetComponent<RectTransform>();
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
            foreach (Transform child in this.transformMemberList.transform)
            {
                if (child.gameObject != this.UI_MemberInfoPrefab.gameObject)
                {
                    GameObject.Destroy(child.gameObject);
                }
            }

            /// Duyệt danh sách thành viên
            foreach (MemberRegister memberData in this._Data)
            {
                /// Thêm vào danh sách
                UIGuildEx_Battle_MemberListFrame_MemberInfo uiMemberInfo = GameObject.Instantiate<UIGuildEx_Battle_MemberListFrame_MemberInfo>(this.UI_MemberInfoPrefab);
                uiMemberInfo.transform.SetParent(this.transformMemberList, false);
                uiMemberInfo.gameObject.SetActive(true);
                uiMemberInfo.Data = memberData;
                uiMemberInfo.Select = () =>
                {
                    /// Thực thi sự kiện
                    this.Select?.Invoke(memberData);
                    /// Đóng khung
                    this.Hide();
                };
            }

            /// Xây lại giao diện
            this.ExecuteSkipFrames(1, () =>
            {
                UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(this.transformMemberList);
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
