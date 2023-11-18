using FS.VLTK.Utilities.UnityUI;
using System;
using UnityEngine;
using TMPro;
using System.Collections.Generic;
using FS.GameEngine.Logic;
using FS.GameFramework.Logic;
using FS.GameEngine.Network;
using FS.VLTK.Utilities.UnityComponent;
using FS.VLTK.Entities.Config;
using System.Linq;
using System.Collections;
using FS.VLTK.Factory;
using UnityEngine.Video;

namespace FS.VLTK.UI.RoleManager
{
    /// <summary>
    /// Màn hình tạo nhân vật
    /// </summary>
    public class UICreateRole : MonoBehaviour
    {
        /// <summary>
        /// Toggle ngũ hành
        /// </summary>
        [Serializable]
        private class SeriesToggle
        {
            /// <summary>
            /// Toggle
            /// </summary>
            public UIToggleSprite UIToggle;

            /// <summary>
            /// Ngũ hành
            /// </summary>
            public Entities.Enum.Elemental Series;
        }

        #region Define
        /// <summary>
        /// Button quay trở lại
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_GoBack;

        /// <summary>
        /// Toggle giới tính nhân vật Nam
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Toggle UIToggle_Man;

        /// <summary>
        /// Toggle giới tính nhân vật nữ
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Toggle UIToggle_Woman;

        /// <summary>
        /// Input tên nhân vật
        /// </summary>
        [SerializeField]
        private TMP_InputField UIInput_RoleName;

        /// <summary>
        /// Button tạo nhân vật
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_CreateRole;

        /// <summary>
        /// Danh sách các Toggle ngũ hành
        /// </summary>
        [SerializeField]
        private SeriesToggle[] UIToggle_Series;

        /// <summary>
        /// Prefab Toggle môn phái
        /// </summary>
        [SerializeField]
        private UIToggleSprite UIToggle_FactionPrefab;

        /// <summary>
        /// Video player
        /// </summary>
        [SerializeField]
        private VideoPlayerEx UI_VideoPlayer;

        /// <summary>
        /// Logo môn phái
        /// </summary>
        [SerializeField]
        private SpriteFromAssetBundle UIImage_FactionLogo;

        /// <summary>
        /// Text mô tả môn phái
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_FactionDescription;

        /// <summary>
        /// Text đặc điểm môn phái
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_FactionTypeDesc;

        /// <summary>
        /// Text các nhánh phái
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_FactionRoutes;

        /// <summary>
        /// Text giới tính cho phép
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_FactionGenders;

        /// <summary>
        /// Prefab sao độ khó
        /// </summary>
        [SerializeField]
        private RectTransform UI_StarPrefab;
        #endregion

        #region Properties
        /// <summary>
        /// Sự kiện khi nút quay lại được ấn
        /// </summary>
        public Action GoBack { get; set; }
        #endregion

        #region Private fields
        /// <summary>
        /// RectTransform độ khó sao
        /// </summary>
        private RectTransform starBoxTransform;

        /// <summary>
        /// RectTransform danh sách môn phái
        /// </summary>
        private RectTransform factionListTransform;

        /// <summary>
        /// Video Bundle lần trước
        /// </summary>
        private string lastVideoBundle = null;
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi khi đối tượng được tạo ra
        /// </summary>
        private void Awake()
        {
            this.starBoxTransform = this.UI_StarPrefab.transform.parent.GetComponent<RectTransform>();
            this.factionListTransform = this.UIToggle_FactionPrefab.transform.parent.GetComponent<RectTransform>();
        }

        /// <summary>
        /// Hàm này gọi đến ở Frame đầu tiên
        /// </summary>
        private void Start()
        {
            this.InitPrefabs();
        }
        #endregion

        #region Code UI
        /// <summary>
        /// Thiết lập mặc định
        /// </summary>
        private void InitPrefabs()
        {
            this.UIButton_GoBack.onClick.AddListener(this.ButtonGoBack_Clicked);
            this.UIButton_CreateRole.onClick.AddListener(this.ButtonCreateRole_Clicked);

            /// Duyệt danh sách Toggle ngũ hành
            foreach (SeriesToggle seriesToggle in this.UIToggle_Series)
            {
                /// Ngũ hành
                Entities.Enum.Elemental nSeries = seriesToggle.Series;
                /// Sự kiện
                seriesToggle.UIToggle.OnSelected = (isSelected) =>
                {
                    /// Nếu không được chọn
                    if (!isSelected)
                    {
                        /// Bỏ qua
                        return;
                    }
                    /// Hiện danh sách các phái theo ngũ hành tương ứng
                    this.DisplayFactionsBySeries(nSeries);
                };
            }
            /// Thực thi ở 2 Frame tiếp
            this.StartCoroutine(this.ExecuteSkipFrames(2, () =>
            {
                /// Chọn Toggle đầu tiên
                SeriesToggle seriesToggle = this.UIToggle_Series.FirstOrDefault();
                /// Kích hoạt
                seriesToggle.UIToggle.Active = true;
            }));
        }

        /// <summary>
        /// Sự kiện khi nút quay lại được ấn
        /// </summary>
        private void ButtonGoBack_Clicked()
        {
            this.GoBack?.Invoke();
        }

        /// <summary>
        /// Sự kiện khi nút tạo nhân vật được ấn
        /// </summary>
        private void ButtonCreateRole_Clicked()
        {
            string roleName = this.UIInput_RoleName.text;
            if (string.IsNullOrEmpty(roleName))
            {
                Super.ShowMessageBox("Lỗi nhập liệu", "Hãy nhập vào tên nhân vật!", true);
                return;
            }
            else if (!KTFormValidation.IsValidString(roleName, false, true, false, false))
            {
                Super.ShowMessageBox("Lỗi nhập liệu", "Tên nhân vật có chứa ký tự đặc biệt, hãy nhập lại!", true);
                return;
            }

            /// Tên nhân vật
            roleName = KTFormValidation.StandardizeString(roleName, true, false);

            if (roleName.Length < 6)
            {
                Super.ShowMessageBox("Lỗi nhập liệu", "Tên nhân vật phải có tối thiểu 6 ký tự!", true);
                return;
            }
            if (this.UIInput_RoleName.text.Length > 12)
            {
                Super.ShowMessageBox("Lỗi nhập liệu", "Tên nhân vật chỉ được có tối đa 12 ký tự!", true);
                return;
            }

            int serverID = 1;
            if (Global.Data != null)
            {
                serverID = Global.Data.GameServerID;
            }

            Super.ShowNetWaiting("Đang khởi tạo nhân vật...");

            /// Giới tính
            int sex = this.UIToggle_Man.isOn ? (int) Entities.Enum.Sex.MALE : (int) Entities.Enum.Sex.FEMALE;
            /// Môn phái
            int factionID = 0;
            
            GameInstance.Game.CreateRole(sex, factionID, this.UIInput_RoleName.text.Trim(), serverID);

            /// Gửi gói tin lên Server
            if (Global.Data.ServerData != null)
            {
                if (Global.Data.ServerData.LastServer != null)
                {
                    PlatformUserLogin.RecordLoginServerIDs(Global.Data.ServerData.LastServer);
                }
            }
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
        /// Hiển thị thông tin phái tương ứng
        /// </summary>
        /// <param name="factionID"></param>
        private void DisplayFactionInfo(int factionID)
        {
            /// Thông tin phái
            if (!Loader.Loader.FactionIntros.TryGetValue(factionID, out FactionIntroXML factionIntro))
            {
                return;
            }
            if (!Loader.Loader.Factions.TryGetValue(factionID, out FactionXML factionInfo))
            {
                return;
            }

            /// Logo
            this.UIImage_FactionLogo.SpriteName = factionIntro.Logo;
            this.UIImage_FactionLogo.PixelPerfect = true;
            this.UIImage_FactionLogo.Load();
            /// Mô tả
            this.UIText_FactionDescription.text = "   " + factionIntro.Description;
            /// Đặc điểm
            this.UIText_FactionTypeDesc.text = factionIntro.TypeDesc;
            /// Võ công nhánh phái
            this.UIText_FactionRoutes.text = string.Join(", ", factionInfo.Subs.Values.Select(x => x.Name).ToArray());
            /// Giới tính
            this.UIText_FactionGenders.text = (int) factionInfo.Gender == -1 ? "Nam, Nữ" : factionInfo.Gender == (int) Entities.Enum.Sex.MALE ? "Nam" : "Nữ";

            /// Xóa toàn bộ sao
            foreach (Transform child in this.starBoxTransform.transform)
            {
                if (child.gameObject != this.UI_StarPrefab.gameObject)
                {
                    GameObject.Destroy(child.gameObject);
                }
            }
            /// Khởi tạo số sao độ khó tương ứng
            for (int i = 1; i <= factionIntro.DiffRate; i++)
            {
                RectTransform uiStar = GameObject.Instantiate<RectTransform>(this.UI_StarPrefab);
                uiStar.transform.SetParent(this.starBoxTransform, false);
                uiStar.gameObject.SetActive(true);
            }
            /// Xây lại giao diện
            this.StartCoroutine(this.ExecuteSkipFrames(1, () =>
            {
                UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(this.starBoxTransform);
            }));

            /// Tải xuống và phát Video
            this.LoadAndPlayVideo(factionIntro.VideoBundle, factionIntro.Video);
        }

        /// <summary>
        /// Tải xuống và phát video
        /// </summary>
        /// <param name="bundleDir"></param>
        /// <param name="videoName"></param>
        private void LoadAndPlayVideo(string bundleDir, string videoName)
        {
            /// Nếu tồn tại Video cũ
            if (!string.IsNullOrEmpty(this.lastVideoBundle))
            {
                /// Hủy
                KTResourceManager.Instance.ReleaseBundle(this.lastVideoBundle);
            }

            /// Đánh dấu Bundle
            this.lastVideoBundle = bundleDir;
            /// Tải xuống Bundle
            KTResourceManager.Instance.LoadAssetBundle(bundleDir);
            /// Tải xuống Asset
            KTResourceManager.Instance.LoadAsset<VideoClip>(bundleDir, videoName, false);

            /// Video tương ứng
            VideoClip clip = KTResourceManager.Instance.GetAsset<VideoClip>(bundleDir, videoName);
            /// Video
            this.UI_VideoPlayer.Video = clip;
            this.UI_VideoPlayer.Repeat = true;
            /// Phát
            this.UI_VideoPlayer.Play();
        }

        /// <summary>
        /// Hiển thị danh sách các phái theo ngũ hành
        /// </summary>
        /// <param name="nSeries"></param>
        private void DisplayFactionsBySeries(Entities.Enum.Elemental nSeries)
        {
            /// Xóa toàn bộ Toggle phái
            foreach (Transform child in this.factionListTransform.transform)
            {
                if (child.gameObject != this.UIToggle_FactionPrefab.gameObject)
                {
                    GameObject.Destroy(child.gameObject);
                }
            }

            /// Toggle phái đầu tiên
            UIToggleSprite uiFirstFactionToggle = null;
            /// ID phái tương ứng Toggle đầu tiên
            int firstFactionID = -1;

            /// Danh sách phái
            List<FactionXML> factions = Loader.Loader.Factions.Values.Where(x => x.Elemental == nSeries).ToList();
            /// Duyệt danh sách phái
            foreach (FactionXML factionInfo in factions)
            {
                /// Thông tin phái
                if (!Loader.Loader.FactionIntros.TryGetValue(factionInfo.ID, out FactionIntroXML factionIntro))
                {
                    continue;
                }

                /// Tạo Toggle phái
                UIToggleSprite uiFactionToggle = GameObject.Instantiate<UIToggleSprite>(this.UIToggle_FactionPrefab);
                uiFactionToggle.transform.SetParent(this.factionListTransform, false);
                uiFactionToggle.gameObject.SetActive(true);
                /// Icon
                uiFactionToggle.NormalSprite = factionIntro.Icon;
                uiFactionToggle.ActiveSprite = factionIntro.ActiveIcon;
                SpriteFromAssetBundle img = uiFactionToggle.GetComponent<SpriteFromAssetBundle>();
                img.SpriteName = factionIntro.Icon;
                img.PixelPerfect = true;
                img.Load();
                /// Sự kiện
                uiFactionToggle.OnSelected = (isSelected) =>
                {
                    /// Nếu không được chọn
                    if (!isSelected)
                    {
                        /// Bỏ qua
                        return;
                    }
                    /// Hiển thị thông tin phái tương ứng
                    this.DisplayFactionInfo(factionInfo.ID);
                };

                /// Đánh dấu Toggle đầu tiên
                if (uiFirstFactionToggle == null)
                {
                    uiFirstFactionToggle = uiFactionToggle;
                    firstFactionID = factionInfo.ID;
                }
            }

            /// Xây lại giao diện
            this.StartCoroutine(this.ExecuteSkipFrames(1, () =>
            {
                UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(this.factionListTransform);
            }));

            /// Chọn phái đầu tiên ở 2 frame tiếp theo
            this.StartCoroutine(this.ExecuteSkipFrames(2, () =>
            {
                /// Nếu không có Toggle đầu tiên
                if (uiFirstFactionToggle == null)
                {
                    /// Bỏ qua
                    return;
                }
                /// Chọn
                uiFirstFactionToggle.Active = true;
            }));
        }
        #endregion
    }
}
