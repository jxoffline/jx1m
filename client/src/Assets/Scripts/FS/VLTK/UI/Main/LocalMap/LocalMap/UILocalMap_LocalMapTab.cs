using FS.Drawing;
using FS.GameEngine.Logic;
using FS.VLTK.Logic;
using FS.VLTK.Network;
using FS.VLTK.Utilities.UnityUI;
using Server.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.SymbolStore;
using System.Linq;
using TMPro;
using UnityEngine;

namespace FS.VLTK.UI.Main.LocalMap
{
    /// <summary>
    /// Tab bản đồ khu vực
    /// </summary>
    public class UILocalMap_LocalMapTab : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Hiệu ứng trạng thái nhiệm vụ tương ứng
        /// </summary>
        [Serializable]
        private class QuestStateIconPrefab
        {
            /// <summary>
            /// Trạng thái nhiệm vụ
            /// </summary>
            public NPCTaskStates State;

            /// <summary>
            /// Đối tượng thực thi hiệu ứng
            /// </summary>
            public UIAnimatedSprite UIAnimatedSprite;
        }

        /// <summary>
        /// Hiệu ứng trạng thái nhiệm vụ tương ứng
        /// </summary>
        [SerializeField]
        private QuestStateIconPrefab[] QuestStateIconPrefabs;

        /// <summary>
        /// Scroll View bản đồ khu vực
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.ScrollRect UIScroll_LocalMap;

        /// <summary>
        /// Kích thước vùng nhìn ở bản đồ khu vực
        /// </summary>
        [SerializeField]
        private Vector2 UIScroll_ViewSize;

        /// <summary>
        /// Ảnh Icon của Leader
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Image UIImage_LeaderIcon;

        /// <summary>
        /// Ảnh Icon vị trí đích đến
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Image UIImage_DestinationIcon;

        /// <summary>
        /// Ảnh bản đồ
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Image UIImage_LocalMapImage;

        /// <summary>
        /// Button chứa ảnh bản đồ (để bắt sự kiện Click)
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_LocalMapImage;

        /// <summary>
        /// Nút gốc chứa các thông tin bản đồ (vùng, bãi train, NPC, ...)
        /// </summary>
        [SerializeField]
        private RectTransform UI_LocalMapInfoRoot;

        /// <summary>
        /// Prefab vùng
        /// </summary>
        [SerializeField]
        private UILocalMap_LocalMapTab_PointInfo UI_Zone_Prefab;

        /// <summary>
        /// Prefab bãi train
        /// </summary>
        [SerializeField]
        private UILocalMap_LocalMapTab_PointInfo UI_TrainArea_Prefab;

        /// <summary>
        /// Prefab NPC
        /// </summary>
        [SerializeField]
        private UILocalMap_LocalMapTab_PointInfo UI_NPC_Prefab;

        /// <summary>
        /// Prefab điểm truyền tống
        /// </summary>
        [SerializeField]
        private UILocalMap_LocalMapTab_PointInfo UI_Teleport_Prefab;

        /// <summary>
        /// Prefab đồng đội
        /// </summary>
        [SerializeField]
        private UILocalMap_LocalMapTab_PointInfo UI_Teammate_Prefab;

        /// <summary>
        /// Prefab quái đặc biệt
        /// </summary>
        [SerializeField]
        private UILocalMap_LocalMapTab_PointInfo UI_SpecialMonster_Prefab;

        /// <summary>
        /// Prefab boss đặc biệt
        /// </summary>
        [SerializeField]
        private UILocalMap_LocalMapTab_PointInfo UI_SpecialBoss_Prefab;

        /// <summary>
        /// Prefab điểm thu thập
        /// </summary>
        [SerializeField]
        private UILocalMap_LocalMapTab_PointInfo UI_GrowPoint_Prefab;

        /// <summary>
        /// Đường kẻ nối 2 điểm đầu và cuối
        /// </summary>
        [SerializeField]
        private RectTransform UI_PathLine;
        #endregion

        #region Private fields
        /// <summary>
        /// Rect Transform của Icon Leader
        /// </summary>
        private RectTransform leaderIconRectTransform;

        /// <summary>
        /// Rect Transform của Icon đích đến
        /// </summary>
        private RectTransform destinationIconRectTransform;

        /// <summary>
        /// RectTransform danh sách hiệu ứng trạng thái nhiệm vụ của NPC
        /// </summary>
        private RectTransform npcTaskStateRectTransform;

        /// <summary>
        /// Luồng thực hiện cập nhật thông tin quái
        /// </summary>
        private Coroutine updateMonsterDataCoroutine = null;
        #endregion

        #region Properties
        /// <summary>
        /// Ảnh Map
        /// </summary>
        public UnityEngine.Sprite LocalMapSprite
        {
            get
            {
                return this.UIImage_LocalMapImage.sprite;
            }
            set
            {
                this.UIImage_LocalMapImage.sprite = value;
                this.UIImage_LocalMapImage.gameObject.GetComponent<RectTransform>().sizeDelta = this.UIImage_LocalMapImage.sprite.rect.size;
            }
        }

        /// <summary>
        /// Kích thước bản đồ thực tế
        /// </summary>
        public Vector2 RealMapSize { get; set; }

        /// <summary>
        /// Kích thước bản đồ nhỏ
        /// </summary>
        public Vector2 LocalMapSize
        {
            get
            {
                if (this.UIImage_LocalMapImage.sprite != null)
                {
                    return this.UIImage_LocalMapImage.sprite.rect.size;
                }
                return Vector2.zero;
            }
        }

        /// <summary>
        /// Sự kiện khi bản đồ được Click
        /// </summary>
        public Action<Vector2> LocalMapClicked { get; set; }

        private List<KeyValuePair<string, Point>> _ListNPCs;
        /// <summary>
        /// Danh sách NPC trong Map
        /// </summary>
        public List<KeyValuePair<string, Point>> ListNPCs
        {
            get
            {
                return this._ListNPCs;
            }
            set
            {
                this._ListNPCs = value;
                this.UpdateNPCList();
            }
        }

        private List<KeyValuePair<string, Point>> _ListTrainArea;
        /// <summary>
        /// Danh sách bãi train trong Map
        /// </summary>
        public List<KeyValuePair<string, Point>> ListTrainArea
        {
            get
            {
                return this._ListTrainArea;
            }
            set
            {
                this._ListTrainArea = value;
                this.UpdateTrainAreaList();
            }
        }

        private List<KeyValuePair<string, Point>> _ListZone;
        /// <summary>
        /// Danh sách các vùng trong bản đồ
        /// </summary>
        public List<KeyValuePair<string, Point>> ListZone
        {
            get
            {
                return this._ListZone;
            }
            set
            {
                this._ListZone = value;
                this.UpdateZoneList();
            }
        }

        private List<KeyValuePair<string, Point>> _ListTeleport;
        /// <summary>
        /// Danh sách điểm truyền tống
        /// </summary>
        public List<KeyValuePair<string, Point>> ListTeleport
        {
            get
            {
                return this._ListTeleport;
            }
            set
            {
                this._ListTeleport = value;
                this.UpdateTeleportList();
            }
        }


        private List<KeyValuePair<string, Point>> _ListGrowPoint;
        /// <summary>
        /// Danh sách các điểm thu thập trong bản đồ
        /// </summary>
        public List<KeyValuePair<string, Point>> ListGrowPoint
        {
            get
            {
                return this._ListGrowPoint;
            }
            set
            {
                this._ListGrowPoint = value;
                this.UpdateGrowPointList();
            }
        }
        #endregion

        #region Private fields
        /// <summary>
        /// Đã chạy qua hàm Start chưa
        /// </summary>
        private bool isStarted = false;
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi khi đối tượng được tạo ra
        /// </summary>
        private void Awake()
        {
            this.leaderIconRectTransform = this.UIImage_LeaderIcon.gameObject.GetComponent<RectTransform>();
            this.destinationIconRectTransform = this.UIImage_DestinationIcon.gameObject.GetComponent<RectTransform>();
            this.npcTaskStateRectTransform = this.QuestStateIconPrefabs.FirstOrDefault().UIAnimatedSprite.transform.parent.GetComponent<RectTransform>();
        }

        /// <summary>
        /// Hàm này gọi ở Frame đầu tiên
        /// </summary>
        private void Start()
        {
            this.InitPrefabs();

            this.ClearTeammateList();
            this.RefreshTeamMembers();

            /// Làm rỗng danh sách quái đặc biệt
            this.ClearSpecialMonsterList();
            this.ClearSpecialBossList();

            this.UIImage_DestinationIcon.gameObject.SetActive(false);
            this.UpdateLeaderLocation();
            this.gameObject.SetActive(true);
            this.PointToLeaderPosition();

            if (Global.Data.Leader.IsMoving && KTLeaderMovingManager.LeaderMoveToPos != default)
            {
                this.UIImage_DestinationIcon.gameObject.SetActive(true);
                Vector2 localMapPos = this.WorldPosToLocalMapPos(KTLeaderMovingManager.LeaderMoveToPos);
                this.destinationIconRectTransform.anchoredPosition = localMapPos;
            }
            this.DisplayNPCTaskStates();

            /// Nếu đã tồn tại luồng cập nhật vị trí quái thì ngừng lại
            if (this.updateMonsterDataCoroutine != null)
            {
                this.StopCoroutine(this.updateMonsterDataCoroutine);
            }
            /// Chạy luồng cập nhật vị trí quái
            this.updateMonsterDataCoroutine = this.StartCoroutine(this.QueryUpdateMonsterData());

            /// Đánh dấu đã chạy qua hàm Start
            this.isStarted = true;
        }

        /// <summary>
        /// Hàm này gọi liên tục mỗi Frame
        /// </summary>
        private void Update()
        {
            this.UpdateDirectionLine();
            /// Cập nhật vị trí của Leader
            this.leaderIconRectTransform.anchoredPosition = this.WorldPosToLocalMapPos(Global.Data.Leader.PositionInVector2);
            /// Nếu không di chuyển
            if (!Global.Data.Leader.IsMoving || KTLeaderMovingManager.LeaderMoveToPos == default)
            {
                this.DestroyPathLine();
            }
            else
            {
                this.ShowPathLine();
            }
        }

        /// <summary>
        /// Hàm này gọi khi đối tượng được kích hoạt
        /// </summary>
        private void OnEnable()
        {
            /// Nếu chưa chạy qua hàm Start thì thôi
            if (!this.isStarted)
            {
                return;
            }

            this.ClearTeammateList();
            this.RefreshTeamMembers();

            /// Làm rỗng danh sách quái đặc biệt
            this.ClearSpecialMonsterList();
            this.ClearSpecialBossList();

            this.UIImage_DestinationIcon.gameObject.SetActive(false);
            this.UpdateLeaderLocation();
            this.gameObject.SetActive(true);
            this.PointToLeaderPosition();

            if (Global.Data.Leader.IsMoving && KTLeaderMovingManager.LeaderMoveToPos != default)
            {
                this.UIImage_DestinationIcon.gameObject.SetActive(true);
                Vector2 localMapPos = this.WorldPosToLocalMapPos(KTLeaderMovingManager.LeaderMoveToPos);
                this.destinationIconRectTransform.anchoredPosition = localMapPos;
            }
            this.DisplayNPCTaskStates();

            /// Nếu đã tồn tại luồng cập nhật vị trí quái thì ngừng lại
            if (this.updateMonsterDataCoroutine != null)
            {
                this.StopCoroutine(this.updateMonsterDataCoroutine);
            }
            /// Chạy luồng cập nhật vị trí quái
            this.updateMonsterDataCoroutine = this.StartCoroutine(this.QueryUpdateMonsterData());
        }

        /// <summary>
        /// Hàm này gọi khi đối tượng bị hủy kích hoạt
        /// </summary>
        private void OnDisable()
        {
            /// Nếu đã tồn tại luồng cập nhật vị trí quái thì ngừng lại
            if (this.updateMonsterDataCoroutine != null)
            {
                this.StopCoroutine(this.updateMonsterDataCoroutine);
            }
        }
        #endregion

        #region Code UI
        /// <summary>
        /// Khởi tạo ban đầu
        /// </summary>
        private void InitPrefabs()
        {
            this.UIButton_LocalMapImage.onClick.AddListener(this.ButtonMapImage_Clicked);
        }

        /// <summary>
        /// Sự kiện khi bản đồ được ấn
        /// </summary>
        private void ButtonMapImage_Clicked()
        {
#if UNITY_EDITOR
            Vector2 position = Input.mousePosition;
#else
            Vector2 position = Input.GetTouch(0).position;
#endif
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(this.UIImage_LocalMapImage.gameObject.GetComponent<RectTransform>(), position, null, out Vector2 inputPosition))
            {
                /// Tọa độ thực
                Vector2 worldPos = this.LocalMapPosToWorldPos(inputPosition);

                /// Thực hiện di chuyển đến vị trí chỉ định
                this.GoToPos(worldPos);
            }
        }
        #endregion

        #region Private fields
        /// <summary>
        /// Luồng thực hiện gửi yêu cầu cập nhật vị trí quái đặc biệt liên tục
        /// </summary>
        /// <returns></returns>
        private IEnumerator QueryUpdateMonsterData()
        {
            WaitForSeconds delay = new WaitForSeconds(2f);
            while (true)
            {
                KT_TCPHandler.SendUpdateLocalMapSpecialMonster();
                yield return delay;
            }
        }

        /// <summary>
        /// Làm rỗng danh sách TaskState đang hiển thị
        /// </summary>
        private void ClearNPCTaskStates()
        {
            foreach (Transform child in this.npcTaskStateRectTransform.transform)
            {
                if (!this.QuestStateIconPrefabs.Any(x => x.UIAnimatedSprite.gameObject == child.gameObject))
                {
                    GameObject.Destroy(child.gameObject);
                }
            }
        }

        /// <summary>
        /// Cập nhật hiển thị TaskState của NPC trong bản đồ
        /// </summary>
        private void DisplayNPCTaskStates()
        {
            this.ClearNPCTaskStates();

            if (Global.Data.RoleData.NPCTaskStateList != null)
            {
                /// Duyệt danh sách trạng thái nhiệm vụ của NPC
                foreach (NPCTaskState state in Global.Data.RoleData.NPCTaskStateList)
                {
                    /// Nếu NPC tồn tại
                    if (Global.Data.GameScene.CurrentMapData.NpcListByID.TryGetValue(state.NPCID, out List<Point> listPos))
                    {
                        /// Duyệt danh sách
                        foreach (Point pos in listPos)
                        {
                            /// Toác
                            if (pos.X == 0 || pos.Y == 0)
                            {
                                continue;
                            }

                            Vector2 worldPos = new Vector2(pos.X, pos.Y);
                            /// Tọa độ bản đồ
                            Vector2 localMapPos = this.WorldPosToLocalMapPos(worldPos);
                            /// Toác
                            if (float.IsNaN(localMapPos.x) || float.IsNaN(localMapPos.x))
                            {
                                continue;
                            }

                            QuestStateIconPrefab uiStatePrefab = this.QuestStateIconPrefabs.Where(x => (int) x.State == state.TaskState).FirstOrDefault();
                            /// Nếu Prefab tồn tại hiệu ứng tương ứng
                            if (uiStatePrefab != null)
                            {
                                /// Tạo đối tượng tương ứng
                                UIAnimatedSprite uiAnimatedSprite = GameObject.Instantiate<UIAnimatedSprite>(uiStatePrefab.UIAnimatedSprite);
                                uiAnimatedSprite.gameObject.SetActive(true);
                                uiAnimatedSprite.transform.SetParent(this.npcTaskStateRectTransform, false);
                                uiAnimatedSprite.transform.localPosition = localMapPos;
                                uiAnimatedSprite.Play();
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Dịch đến vị trí hiện tại của Leader
        /// </summary>
        private void PointToLeaderPosition()
        {
            Vector2 leaderPos = this.WorldPosToLocalMapPos(Global.Data.Leader.PositionInVector2);
            float scrollPosX = Math.Max(0, leaderPos.x - this.UIScroll_ViewSize.x / 2);
            float scrollPosY = Math.Max(0, leaderPos.y - this.UIScroll_ViewSize.y / 2);

            this.UIScroll_LocalMap.verticalNormalizedPosition = scrollPosY / this.LocalMapSize.y;
            this.UIScroll_LocalMap.horizontalNormalizedPosition = scrollPosX / this.LocalMapSize.x;
        }

        /// <summary>
        /// Chuyển đổi tọa độ thực sang tọa độ ở bản đồ khu vực
        /// </summary>
        /// <param name="worldPos"></param>
        /// <returns></returns>
        private Vector2 WorldPosToLocalMapPos(Vector2 worldPos)
        {
            return new Vector2(worldPos.x / this.RealMapSize.x * this.LocalMapSize.x, worldPos.y / this.RealMapSize.y * this.LocalMapSize.y);
        }

        /// <summary>
        /// Chuyển đổi tọa độ ở bản đồ khu vực sang tọa độ thực
        /// </summary>
        /// <param name="localMapPos"></param>
        /// <returns></returns>
        private Vector2 LocalMapPosToWorldPos(Vector2 localMapPos)
        {
            return new Vector2((localMapPos.x / this.LocalMapSize.x * this.RealMapSize.x), localMapPos.y / this.LocalMapSize.y * this.RealMapSize.y);
        }

        /// <summary>
        /// Cập nhật vị trí của Leader trong bản đồ
        /// </summary>
        /// <param name="mapName"></param>
        /// <param name="position"></param>
        private void UpdateLeaderLocation()
        {
            if (this.leaderIconRectTransform != null)
            {
                this.leaderIconRectTransform.anchoredPosition = this.WorldPosToLocalMapPos(Global.Data.Leader.PositionInVector2);
            }
        }

        /// <summary>
        /// Xóa các vị trí đã đi (tự tìm đường)
        /// </summary>
        private void UpdateDirectionLine()
        {
            if (!this.destinationIconRectTransform.gameObject.activeSelf)
            {
                return;
            }

            Vector2 leaderPos = this.WorldPosToLocalMapPos(Global.Data.Leader.PositionInVector2);
            Vector2 destinationPos = this.destinationIconRectTransform.anchoredPosition;
            float distance = Vector2.Distance(leaderPos, destinationPos);
            float degree = KTMath.GetAngleBetweenVector(new Vector2(1, 0), destinationPos - leaderPos);

            this.UI_PathLine.anchoredPosition = leaderPos;
            this.UI_PathLine.sizeDelta = new Vector2(distance, 2f);
            this.UI_PathLine.localRotation = Quaternion.Euler(new Vector3(0, 0, degree));
        }

        /// <summary>
        /// Làm rỗng danh sách NPC
        /// </summary>
        private void ClearNPCList()
        {
            foreach (Transform child in this.UI_LocalMapInfoRoot.transform)
            {
                if (child.gameObject != this.UI_NPC_Prefab.gameObject && child.gameObject.GetComponent<UILocalMap_LocalMapTab_PointInfo>() != null && child.gameObject.GetComponent<UILocalMap_LocalMapTab_PointInfo>().Tag.Equals(this.UI_NPC_Prefab.Tag))
                {
                    GameObject.Destroy(child.gameObject);
                }
            }
        }

        /// <summary>
        /// Cập nhật danh sách NPC
        /// </summary>
        private void UpdateNPCList()
        {
            this.ClearNPCList();
            foreach (KeyValuePair<string, Point> pair in this._ListNPCs)
            {
                UILocalMap_LocalMapTab_PointInfo node = GameObject.Instantiate<UILocalMap_LocalMapTab_PointInfo>(this.UI_NPC_Prefab);
                node.gameObject.SetActive(true);
                node.gameObject.transform.SetParent(this.UI_LocalMapInfoRoot, false);
                node.gameObject.transform.SetAsFirstSibling();
                node.Name = pair.Key;
                node.GetComponent<RectTransform>().anchoredPosition = this.WorldPosToLocalMapPos(new Vector2(pair.Value.X, pair.Value.Y));
            }
        }

        /// <summary>
        /// Làm rỗng danh sách bãi train
        /// </summary>
        private void ClearTrainAreaList()
        {
            foreach (Transform child in this.UI_LocalMapInfoRoot.transform)
            {
                if (child.gameObject != this.UI_TrainArea_Prefab.gameObject && child.gameObject.GetComponent<UILocalMap_LocalMapTab_PointInfo>() != null && child.gameObject.GetComponent<UILocalMap_LocalMapTab_PointInfo>().Tag.Equals(this.UI_TrainArea_Prefab.Tag))
                {
                    GameObject.Destroy(child.gameObject);
                }
            }
        }

        /// <summary>
        /// Cập nhật danh sách bãi train
        /// </summary>
        private void UpdateTrainAreaList()
        {
            this.ClearTrainAreaList();
            foreach (KeyValuePair<string, Point> pair in this._ListTrainArea)
            {
                UILocalMap_LocalMapTab_PointInfo node = GameObject.Instantiate<UILocalMap_LocalMapTab_PointInfo>(this.UI_TrainArea_Prefab);
                node.gameObject.SetActive(true);
                node.gameObject.transform.SetParent(this.UI_LocalMapInfoRoot, false);
                node.gameObject.transform.SetAsFirstSibling();
                node.Name = pair.Key;
                node.GetComponent<RectTransform>().anchoredPosition = this.WorldPosToLocalMapPos(new Vector2(pair.Value.X, pair.Value.Y));
            }
        }

        /// <summary>
        /// Làm rỗng danh sách khu vực
        /// </summary>
        private void ClearZoneList()
        {
            foreach (Transform child in this.UI_LocalMapInfoRoot.transform)
            {
                if (child.gameObject != this.UI_Zone_Prefab.gameObject && child.gameObject.GetComponent<UILocalMap_LocalMapTab_PointInfo>() != null && child.gameObject.GetComponent<UILocalMap_LocalMapTab_PointInfo>().Tag.Equals(this.UI_Zone_Prefab.Tag))
                {
                    GameObject.Destroy(child.gameObject);
                }
            }
        }

        /// <summary>
        /// Cập nhật danh sách khu vực
        /// </summary>
        private void UpdateZoneList()
        {
            this.ClearZoneList();
            foreach (KeyValuePair<string, Point> pair in this._ListZone)
            {
                UILocalMap_LocalMapTab_PointInfo node = GameObject.Instantiate<UILocalMap_LocalMapTab_PointInfo>(this.UI_Zone_Prefab);
                node.gameObject.SetActive(true);
                node.gameObject.transform.SetParent(this.UI_LocalMapInfoRoot, false);
                node.gameObject.transform.SetAsFirstSibling();
                node.Name = pair.Key;
                node.GetComponent<RectTransform>().anchoredPosition = this.WorldPosToLocalMapPos(new Vector2(pair.Value.X, pair.Value.Y));
            }
        }

        /// <summary>
        /// Làm rỗng danh sách điểm truyền tống
        /// </summary>
        private void ClearTeleportList()
        {
            foreach (Transform child in this.UI_LocalMapInfoRoot.transform)
            {
                if (child.gameObject != this.UI_Teleport_Prefab.gameObject && child.gameObject.GetComponent<UILocalMap_LocalMapTab_PointInfo>() != null && child.gameObject.GetComponent<UILocalMap_LocalMapTab_PointInfo>().Tag.Equals(this.UI_Teleport_Prefab.Tag))
                {
                    GameObject.Destroy(child.gameObject);
                }
            }
        }

        /// <summary>
        /// Làm rỗng danh sách đội viên
        /// </summary>
        private void ClearTeammateList()
        {
            foreach (Transform child in this.UI_LocalMapInfoRoot.transform)
            {
                if (child.gameObject != this.UI_Teammate_Prefab.gameObject && child.gameObject.GetComponent<UILocalMap_LocalMapTab_PointInfo>() != null && child.gameObject.GetComponent<UILocalMap_LocalMapTab_PointInfo>().Tag.Equals(this.UI_Teammate_Prefab.Tag))
                {
                    GameObject.Destroy(child.gameObject);
                }
            }
        }

        /// <summary>
        /// Làm rỗng danh sách quái đặc biệt
        /// </summary>
        private void ClearSpecialMonsterList()
        {
            foreach (Transform child in this.UI_LocalMapInfoRoot.transform)
            {
                if (child.gameObject != this.UI_SpecialMonster_Prefab.gameObject && child.gameObject.GetComponent<UILocalMap_LocalMapTab_PointInfo>() != null && child.gameObject.GetComponent<UILocalMap_LocalMapTab_PointInfo>().Tag.Equals(this.UI_SpecialMonster_Prefab.Tag))
                {
                    GameObject.Destroy(child.gameObject);
                }
            }
        }

        /// <summary>
        /// Làm rỗng danh sách boss đặc biệt
        /// </summary>
        private void ClearSpecialBossList()
        {
            foreach (Transform child in this.UI_LocalMapInfoRoot.transform)
            {
                if (child.gameObject != this.UI_SpecialBoss_Prefab.gameObject && child.gameObject.GetComponent<UILocalMap_LocalMapTab_PointInfo>() != null && child.gameObject.GetComponent<UILocalMap_LocalMapTab_PointInfo>().Tag.Equals(this.UI_SpecialBoss_Prefab.Tag))
                {
                    GameObject.Destroy(child.gameObject);
                }
            }
        }

        /// <summary>
        /// Cập nhật danh sách điểm truyền tống
        /// </summary>
        private void UpdateTeleportList()
        {
            this.ClearTeleportList();
            foreach (KeyValuePair<string, Point> pair in this._ListTeleport)
            {
                UILocalMap_LocalMapTab_PointInfo node = GameObject.Instantiate<UILocalMap_LocalMapTab_PointInfo>(this.UI_Teleport_Prefab);
                node.gameObject.SetActive(true);
                node.gameObject.transform.SetParent(this.UI_LocalMapInfoRoot, false);
                node.gameObject.transform.SetAsFirstSibling();
                node.Name = pair.Key;
                node.GetComponent<RectTransform>().anchoredPosition = this.WorldPosToLocalMapPos(new Vector2(pair.Value.X, pair.Value.Y));
            }
        }

        /// <summary>
        /// Làm mới danh sách thành viên nhóm
        /// </summary>
        public void RefreshTeamMembers()
        {
            this.ClearTeammateList();
            if (Global.Data.Teammates != null)
            {
                foreach (RoleDataMini rd in Global.Data.Teammates)
                {
                    /// Nếu là bản thân
                    if (Global.Data.RoleData.RoleID == rd.RoleID)
                    {
                        continue;
                    }
                    /// Nếu không chung bản đồ
                    else if (rd.MapCode != Global.Data.RoleData.MapCode)
                    {
                        continue;
                    }

                    UILocalMap_LocalMapTab_PointInfo node = GameObject.Instantiate<UILocalMap_LocalMapTab_PointInfo>(this.UI_Teammate_Prefab);
                    node.gameObject.SetActive(true);
                    node.gameObject.transform.SetParent(this.UI_LocalMapInfoRoot, false);
                    node.Name = rd.RoleName;
                    node.GetComponent<RectTransform>().anchoredPosition = this.WorldPosToLocalMapPos(new Vector2(rd.PosX, rd.PosY));
                }
            }
        }

        /// <summary>
        /// Cập nhật danh sách quái đặc biệt
        /// </summary>
        /// <param name="monsters"></param>
        public void UpdateSpecialMonsterList(List<LocalMapMonsterData> monsters)
        {
            this.ClearSpecialMonsterList();
            this.ClearSpecialBossList();

            /// Duyệt danh sách
            foreach (LocalMapMonsterData monster in monsters)
            {
                UILocalMap_LocalMapTab_PointInfo node;
                /// Nếu là boss
                if (monster.IsBoss)
                {
                    node = GameObject.Instantiate<UILocalMap_LocalMapTab_PointInfo>(this.UI_SpecialBoss_Prefab);
                }
                /// Nếu là quái
                else
                {
                    node = GameObject.Instantiate<UILocalMap_LocalMapTab_PointInfo>(this.UI_SpecialMonster_Prefab);
                }

                node.gameObject.SetActive(true);
                node.gameObject.transform.SetParent(this.UI_LocalMapInfoRoot, false);
                node.gameObject.transform.SetAsFirstSibling();
                node.Name = monster.Name;
                node.GetComponent<RectTransform>().anchoredPosition = this.WorldPosToLocalMapPos(new Vector2(monster.PosX, monster.PosY));
            }
        }



        /// <summary>
        /// Làm rỗng danh sách điểm thu thập
        /// </summary>
        private void ClearGrowPointList()
        {
            foreach (Transform child in this.UI_LocalMapInfoRoot.transform)
            {
                if (child.gameObject != this.UI_GrowPoint_Prefab.gameObject && child.gameObject.GetComponent<UILocalMap_LocalMapTab_PointInfo>() != null && child.gameObject.GetComponent<UILocalMap_LocalMapTab_PointInfo>().Tag.Equals(this.UI_GrowPoint_Prefab.Tag))
                {
                    GameObject.Destroy(child.gameObject);
                }
            }
        }

        /// <summary>
        /// Cập nhật danh sách khu vực
        /// </summary>
        private void UpdateGrowPointList()
        {
            this.ClearGrowPointList();
            foreach (KeyValuePair<string, Point> pair in this._ListGrowPoint)
            {
                UILocalMap_LocalMapTab_PointInfo node = GameObject.Instantiate<UILocalMap_LocalMapTab_PointInfo>(this.UI_GrowPoint_Prefab);
                node.gameObject.SetActive(true);
                node.gameObject.transform.SetParent(this.UI_LocalMapInfoRoot, false);
                node.gameObject.transform.SetAsFirstSibling();
                node.Name = pair.Key;
                node.GetComponent<RectTransform>().anchoredPosition = this.WorldPosToLocalMapPos(new Vector2(pair.Value.X, pair.Value.Y));
            }
        }
        #endregion

        #region Public fields
        /// <summary>
        /// Xóa đường kẻ đường đi
        /// </summary>
        public void DestroyPathLine()
        {
            this.UI_PathLine.gameObject.SetActive(false);
            this.UIImage_DestinationIcon.gameObject.SetActive(false);
        }

        /// <summary>
        /// Hiển thị đường kẻ đường đi
        /// </summary>
        public void ShowPathLine()
        {
            this.UI_PathLine.gameObject.SetActive(true);
            this.UIImage_DestinationIcon.gameObject.SetActive(true);
        }

        /// <summary>
        /// Làm mới hiển thị trạng thái nhiệm vụ của NPC
        /// </summary>
        public void RefreshNPCTaskStates()
        {
            this.DisplayNPCTaskStates();
        }

        /// <summary>
        /// Di chuyển nhân vật đến vị trí chỉ định
        /// </summary>
        /// <param name="worldPos"></param>
        public void GoToPos(Vector2 worldPos)
        {
            /// Cập nhật vị trí trên UI
            this.UpdateLocalMapFlagPos(worldPos);
            /// Tọa độ lưới
            Vector2 gridPos = KTGlobal.WorldPositionToGridPosition(worldPos);

            /// Nếu vị trí này không đến được
            if (!Global.Data.GameScene.CanMove(new Point((int) gridPos.x, (int) gridPos.y)))
            {
                KTGlobal.AddNotification("Vị trí này không đến được!");
                return;
            }

            /// Thực hiện Callback dịch chuyển
            this.LocalMapClicked?.Invoke(worldPos);

            /// Hiển thị dòng chữ tự tìm đường
            PlayZone.Instance.ShowTextAutoFindPath();
        }

        /// <summary>
        /// Cập nhật vị trí cờ chỉ đường trên bản đồ nhỏ
        /// </summary>
        /// <param name="worldPos"></param>
        public void UpdateLocalMapFlagPos(Vector2 worldPos)
		{
            /// Tọa độ ở bản đồ nhỏ
            Vector2 localMapPos = this.WorldPosToLocalMapPos(worldPos);

            this.UIImage_DestinationIcon.gameObject.SetActive(true);
            this.destinationIconRectTransform.anchoredPosition = localMapPos;
        }
        #endregion
    }
}
