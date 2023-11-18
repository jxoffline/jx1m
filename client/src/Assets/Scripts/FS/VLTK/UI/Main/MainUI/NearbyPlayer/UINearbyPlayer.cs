using FS.VLTK.UI.Main.MainUI.NearbyPlayer;
using Server.Data;
using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine;
using FS.GameEngine.Logic;
using FS.GameEngine.Sprite;

namespace FS.VLTK.UI.Main.MainUI
{
    /// <summary>
    /// Khung người chơi lân cận
    /// </summary>
    public class UINearbyPlayer : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Button đóng khung
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_Close;

        /// <summary>
        /// Prefab thông tin người chơi
        /// </summary>
        [SerializeField]
        private UINearbyPlayer_PlayerInfo UIPlayerInfo_Prefab;
        #endregion

        #region Properties
        /// <summary>
        /// Sự kiện người chơi được chọn
        /// </summary>
        public Action<RoleData> PlayerSelected { get; set; }

        /// <summary>
        /// Sự kiện mời người chơi vào nhóm
        /// </summary>
        public Action<RoleData> InviteToTeam { get; set; }

        /// <summary>
        /// Sự kiện xin gia nhập nhóm của người chơi
        /// </summary>
        public Action<RoleData> AskToJoinTeam { get; set; }

        /// <summary>
        /// Sự kiện đóng khung
        /// </summary>
        public Action Close { get; set; }

        /// <summary>
        /// Khung có đang hiển thị không
        /// </summary>
        public bool Visible
        {
            get
            {
                return this.gameObject.activeSelf;
            }
        }
        #endregion

        #region Private fields
        /// <summary>
        /// RectTransform danh sách người chơi
        /// </summary>
        private RectTransform listTransform;
        #endregion

        #region Constants
        /// <summary>
        /// Số người chơi được lưu tối đa
        /// </summary>
        private const int MaxCapacity = 10;
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi khi đối tượng được tạo ra
        /// </summary>
        private void Awake()
        {
            this.listTransform = this.UIPlayerInfo_Prefab.transform.parent.GetComponent<RectTransform>();
        }

        /// <summary>
        /// Hàm này gọi ở Frame đầu tiên
        /// </summary>
        private void Start()
        {
            this.InitPrefabs();
            this.ClearList();
            this.InitEmptySlot(UINearbyPlayer.MaxCapacity);
        }

        /// <summary>
        /// Hàm này gọi khi đối tượng được kích hoạt
        /// </summary>
        private void OnEnable()
        {
            this.StartCoroutine(this.FindPlayersAround());
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
        /// Sự kiện khi người chơi tương ứng được chọn
        /// </summary>
        /// <param name="roleData"></param>
        private void ButtonPlayer_Clicked(RoleData roleData)
        {
            this.PlayerSelected?.Invoke(roleData);
        }

        /// <summary>
        /// Sự kiện khi Button mời vào nhóm được ấn
        /// </summary>
        /// <param name="roleData"></param>
        private void ButtonInviteToTeam_Clicked(RoleData roleData)
        {
            this.InviteToTeam?.Invoke(roleData);
        }

        /// <summary>
        /// Sự kiện khi Button xin gia nhập nhóm được ấn
        /// </summary>
        /// <param name="roleData"></param>
        private void ButtonAskToJoinTeam_Clicked(RoleData roleData)
        {
            this.AskToJoinTeam?.Invoke(roleData);
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
        /// Thực thi sự kiện bỏ qua số lượng Frame tương ứng
        /// </summary>
        /// <param name="skip"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        private IEnumerator ExecuteSkipFrames(int skip, Action callback)
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
            this.StartCoroutine(this.ExecuteSkipFrames(1, () => {
                UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(this.listTransform);
            }));
        }

        /// <summary>
        /// Làm rỗng danh sách
        /// </summary>
        private void ClearList()
        {
            foreach (Transform child in this.listTransform.transform)
            {
                if (child.gameObject != this.UIPlayerInfo_Prefab.gameObject)
                {
                    GameObject.Destroy(child.gameObject);
                }
            }
        }

        /// <summary>
        /// Khởi tạo các ô trống ban đầu
        /// </summary>
        /// <param name="slot"></param>
        private void InitEmptySlot(int slot)
        {
            for (int i = 1; i <= slot; i++)
            {
                UINearbyPlayer_PlayerInfo uiPlayerInfo = GameObject.Instantiate<UINearbyPlayer_PlayerInfo>(this.UIPlayerInfo_Prefab);
                uiPlayerInfo.gameObject.SetActive(false);
                uiPlayerInfo.transform.SetParent(this.listTransform, false);
                uiPlayerInfo.RoleData = null;
            }
        }

        /// <summary>
        /// Lấy thông tin người chơi tương ứng trong danh sách
        /// </summary>
        /// <param name="roleID"></param>
        /// <returns></returns>
        private UINearbyPlayer_PlayerInfo GetPlayerSlot(int roleID)
        {
            foreach (Transform child in this.listTransform.transform)
            {
                if (child.gameObject != this.UIPlayerInfo_Prefab.gameObject)
                {
                    UINearbyPlayer_PlayerInfo uiPlayerInfo = child.gameObject.GetComponent<UINearbyPlayer_PlayerInfo>();
                    if (uiPlayerInfo.RoleData != null && uiPlayerInfo.RoleData.RoleID == roleID)
                    {
                        return uiPlayerInfo;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Tìm một vị trí trống
        /// </summary>
        /// <returns></returns>
        private UINearbyPlayer_PlayerInfo FindEmptySlot()
        {
            foreach (Transform child in this.listTransform.transform)
            {
                if (child.gameObject != this.UIPlayerInfo_Prefab.gameObject)
                {
                    UINearbyPlayer_PlayerInfo uiPlayerInfo = child.gameObject.GetComponent<UINearbyPlayer_PlayerInfo>();
                    if (uiPlayerInfo.RoleData == null)
                    {
                        return uiPlayerInfo;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Xóa người chơi tương ứng khỏi danh sách
        /// </summary>
        /// <param name="roleID"></param>
        private void RemovePlayer(int roleID)
        {
            UINearbyPlayer_PlayerInfo uiPlayerInfo = this.GetPlayerSlot(roleID);
            if (uiPlayerInfo == null)
            {
                return;
            }

            uiPlayerInfo.RoleData = null;
            this.RebuildLayout();
        }

        /// <summary>
        /// Xóa người chơi tương ứng khỏi danh sách
        /// </summary>
        /// <param name="uiPlayerInfo"></param>
        private void RemovePlayer(UINearbyPlayer_PlayerInfo uiPlayerInfo)
        {
            if (uiPlayerInfo == null)
            {
                return;
            }

            uiPlayerInfo.gameObject.SetActive(false);
            uiPlayerInfo.RoleData = null;
            this.RebuildLayout();
        }

        /// <summary>
        /// Thêm người chơi vào danh sách hiển thị
        /// </summary>
        /// <param name="roleData"></param>
        private void AddPlayer(RoleData roleData)
        {
            UINearbyPlayer_PlayerInfo uiPlayerInfo = this.FindEmptySlot();
            if (uiPlayerInfo == null)
            {
                return;
            }

            uiPlayerInfo.gameObject.SetActive(true);
            /// Đối tượng tương ứng
            GSprite sprite = KTGlobal.FindSpriteByID(roleData.RoleID);
            /// Nếu mục tiêu tiềm ẩn nguy hiểm
            if (sprite != null && KTGlobal.IsDangerous(sprite))
            {
                uiPlayerInfo.NameColor = KTGlobal.DangerousPlayerNameColor;
            }
            /// Nếu là kẻ địch
            else if (sprite != null && KTGlobal.IsEnemy(sprite))
            {
                uiPlayerInfo.NameColor = KTGlobal.EnemyPlayerNameColor;
            }
            uiPlayerInfo.RoleData = roleData;
            uiPlayerInfo.Click = () => {
                this.ButtonPlayer_Clicked(roleData);
            };
            uiPlayerInfo.InviteToTeam = () => {
                this.ButtonInviteToTeam_Clicked(roleData);
            };
            uiPlayerInfo.AskToJoinTeam = () => {
                this.ButtonAskToJoinTeam_Clicked(roleData);
            };
            this.RebuildLayout();
        }

        /// <summary>
        /// Cập nhật dữ liệu mới
        /// </summary>
        /// <returns></returns>
        private IEnumerator FindPlayersAround()
        {
            while (true)
            {
                if (Global.Data == null)
                {
                    yield return null;
                    continue;
                }

                /// Duyệt toàn bộ danh sách cũ kiểm tra xem thằng nào không tồn tại nữa thì xóa
                foreach (Transform child in this.listTransform.transform)
                {
                    if (child.gameObject != this.UIPlayerInfo_Prefab.gameObject)
                    {
                        UINearbyPlayer_PlayerInfo uiPlayerInfo = child.gameObject.GetComponent<UINearbyPlayer_PlayerInfo>();
                        if (uiPlayerInfo.RoleData == null || !Global.Data.OtherRoles.TryGetValue(uiPlayerInfo.RoleData.RoleID, out _))
                        {
                            this.RemovePlayer(uiPlayerInfo);
                        }
                        else
                        {
                            /// Nếu khoảng cách quá xa thì xóa
                            if (Vector2.Distance(new Vector2(uiPlayerInfo.RoleData.PosX, uiPlayerInfo.RoleData.PosY), Global.Data.Leader.PositionInVector2) > 1500)
                            {
                                this.RemovePlayer(uiPlayerInfo);
                            }
                        }
                    }
                }

                /// Danh sách người chơi xung quanh, ưu tiên kẻ địch lên trước
                List<RoleData> nearbyPlayers = Global.Data.OtherRoles.Values.OrderBy((rd) => {
                    /// Đối tượng tương ứng
                    GSprite sprite = KTGlobal.FindSpriteByID(rd.RoleID);
                    return KTGlobal.IsEnemy(sprite);
                }).ThenBy((rd) => {
                    return Vector2.Distance(new Vector2(rd.PosX, rd.PosY), Global.Data.Leader.PositionInVector2);
                }).ToList();

                /// Tổng số người chơi xung quanh đã duyệt
                int refCount = 0;
                /// Duyệt danh sách người chơi xung quanh
                foreach (RoleData roleData in nearbyPlayers)
                {
                    /// Tăng tổng số người chơi đã duyệt
                    refCount++;

                    /// Nếu tổng số vượt quá giới hạn
                    if (refCount > UINearbyPlayer.MaxCapacity)
                    {
                        break;
                    }

                    /// Tìm vị trí tương ứng nếu đã tồn tại trong danh sách
                    UINearbyPlayer_PlayerInfo uiPlayerInfo = this.GetPlayerSlot(roleData.RoleID);
                    /// Nếu đã tồn tại trong danh sách
                    if (uiPlayerInfo != null)
                    {
                        /// Cập nhật sinh lực
                        uiPlayerInfo.UpdateHP(roleData.CurrentHP, roleData.MaxHP);
                        continue;
                    }

                    /// Nếu chưa tồn tại thì thêm người chơi vào danh sách
                    this.AddPlayer(roleData);
                }

                /// Xóa danh sách người chơi xung quanh
                nearbyPlayers.Clear();
                nearbyPlayers = null;

                /// Lấy danh sách người chơi xung quanh
                yield return new WaitForSeconds(1f);
            }
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Hiện khung
        /// </summary>
        public void Show()
        {
            this.gameObject.SetActive(true);
        }

        /// <summary>
        /// Ẩn khung
        /// </summary>
        public void Hide()
        {
            this.gameObject.SetActive(false);
        }
        #endregion
    }
}
