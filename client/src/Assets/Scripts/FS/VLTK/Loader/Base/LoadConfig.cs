using FS.GameEngine.Logic;
using FS.GameFramework.Logic;
using FS.VLTK.Entities.Object;
using FS.VLTK.Factory;
using System;
using System.Collections;
using System.IO;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace FS.VLTK.Loader
{
    /// <summary>
    /// Màn hình tải dữ liệu
    /// </summary>
    public class LoadConfig : TTMonoBehaviour
    {
        #region Private fields
        /// <summary>
        /// AssetBundle cấu hình Game
        /// </summary>
        private AssetBundle GameConfigAssetBundle = null;
        #endregion

        #region Properties
        /// <summary>
        /// Báo cáo tiến trình tải dữ liệu
        /// </summary>
        public Action<int> OnProgressBarReport { get; set; }

        /// <summary>
        /// Thực thi khi tiến trình tải xuống hoàn tất
        /// </summary>
        public Action OnLoadFinish { get; set; }
        #endregion


        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi đến khi đối tượng được kích hoạt
        /// </summary>
        private void Start()
        {
            GameObject.DontDestroyOnLoad(this);
            this.StartDownloadGameRes();
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Thực thi tải xuống Resource của Game
        /// </summary>
        private void StartDownloadGameRes()
        {
            this.StartCoroutine<bool>(this.InitGameRes(), this.CoroutineException);
        }

        /// <summary>
        /// Hàm này bắt lỗi khi có ngoại lệ xảy ra ở luồng tải dữ liệu
        /// </summary>
        private void CoroutineException()
        {

        }

        /// <summary>
        /// Luồng tải xuống Resource của Game
        /// </summary>
        /// <returns></returns>
        private IEnumerator InitGameRes()
        {
            this.OnProgressBarReport?.Invoke(0);
            yield return null;

            #region Tải AssetBundles
            string configPath = Global.WebPath(string.Format("Data/{0}", Consts.GAME_CONFIG_FILE));
            this.GameConfigAssetBundle = ResourceLoader.LoadAssetBundle(configPath, true);
            /// Toác
            if (this.GameConfigAssetBundle == null)
            {
                Super.ShowMessageBox("Lỗi tải dữ liệu", "Không thể tải dữ liệu từ file Config.unity3d. Hãy kiểm tra!");
            }
            #endregion

            //WaitForSeconds wait = new WaitForSeconds(10f);
            WaitForSeconds wait = null;

            #region Đọc dữ liệu từ các file XML bên trong
            this.OnProgressBarReport?.Invoke(1);
            yield return wait;
            Loader.LoadPlayerPray(ResourceLoader.LoadXMLFromBundle(this.GameConfigAssetBundle, Consts.XML_ACTIIVTY_PLAYERPRAY));

            this.OnProgressBarReport?.Invoke(2);
            yield return wait;
            Loader.LoadColonyMaps(ResourceLoader.LoadXMLFromBundle(this.GameConfigAssetBundle, Consts.XML_COLONYMAP_FILE));

            this.OnProgressBarReport?.Invoke(3);
            yield return wait;
            Loader.LoadCrossServerMap(ResourceLoader.LoadXMLFromBundle(this.GameConfigAssetBundle, Consts.XML_CROSSSERVERMAP_FILE));

            this.OnProgressBarReport?.Invoke(4);
            yield return wait;
            Loader.LoadWorldMap(ResourceLoader.LoadXMLFromBundle(this.GameConfigAssetBundle, Consts.XML_WORLDMAP_FILE));

            this.OnProgressBarReport?.Invoke(5);
            yield return wait;
            Loader.LoadMapConfig(ResourceLoader.LoadXMLFromBundle(this.GameConfigAssetBundle, Consts.XML_MAP_FILE));

            this.OnProgressBarReport?.Invoke(6);
            yield return wait;
            Loader.LoadAutoPaths(ResourceLoader.LoadXMLFromBundle(this.GameConfigAssetBundle, Consts.XML_AUTOPATH_FILE));

            this.OnProgressBarReport?.Invoke(10);
            yield return wait;
            Loader.LoadMonsterActionSetXML(ResourceLoader.LoadXMLFromBundle(this.GameConfigAssetBundle, Consts.XML_MONSTERACTIONSET_FILE));

            this.OnProgressBarReport?.Invoke(12);
            yield return wait;
            Loader.LoadMonsters(ResourceLoader.LoadXMLFromBundle(this.GameConfigAssetBundle, Consts.XML_MONSTER_FILE));

            this.OnProgressBarReport?.Invoke(15);
            yield return wait;
            Loader.LoadPetConfig(ResourceLoader.LoadXMLFromBundle(this.GameConfigAssetBundle, Consts.XML_PETCONFIG_FILE));
            Loader.LoadPets(ResourceLoader.LoadXMLFromBundle(this.GameConfigAssetBundle, Consts.XML_PET_FILE));

            this.OnProgressBarReport?.Invoke(16);
            yield return wait;
            Loader.LoadNPCs(ResourceLoader.LoadXMLFromBundle(this.GameConfigAssetBundle, Consts.XML_NPC_FILE));

            this.OnProgressBarReport?.Invoke(18);
            yield return wait;
            Loader.LoadRoleAvarta(ResourceLoader.LoadXMLFromBundle(this.GameConfigAssetBundle, Consts.XML_ROLEAVARTA_FILE));

            this.OnProgressBarReport?.Invoke(19);
            yield return wait;
            Loader.LoadGuildConfig(ResourceLoader.LoadXMLFromBundle(this.GameConfigAssetBundle, Consts.XML_GUILDCONFIG_FILE));

            this.OnProgressBarReport?.Invoke(20);
            yield return wait;
            Loader.LoadEffects(ResourceLoader.LoadXMLFromBundle(this.GameConfigAssetBundle, Consts.XML_EFFECT_FILE));

            this.OnProgressBarReport?.Invoke(23);
            yield return wait;
            Loader.LoadFaction(ResourceLoader.LoadXMLFromBundle(this.GameConfigAssetBundle, Consts.XML_FACTION_FILE));
            Loader.LoadFactionIntro(ResourceLoader.LoadXMLFromBundle(this.GameConfigAssetBundle, Consts.XML_FACTIONINTRO_FILE));

            this.OnProgressBarReport?.Invoke(24);
            yield return wait;
            Loader.LoadWeaponEnhanceEffectConfig(ResourceLoader.LoadXMLFromBundle(this.GameConfigAssetBundle, Consts.XML_WEAPONENHANCEEFFECT_FILE));

            this.OnProgressBarReport?.Invoke(25);
            yield return wait;
            Loader.LoadPropertyDictionary(ResourceLoader.LoadXMLFromBundle(this.GameConfigAssetBundle, Consts.XML_PROPERTYDICTIONARY_FILE));

            this.OnProgressBarReport?.Invoke(30);
            yield return wait;
            Loader.LoadSkillAttribute(ResourceLoader.LoadXMLFromBundle(this.GameConfigAssetBundle, Consts.XML_SKILLATTRIBUTE_FILE));

            this.OnProgressBarReport?.Invoke(31);
            yield return wait;
            Loader.LoadEnchantSkill(ResourceLoader.LoadXMLFromBundle(this.GameConfigAssetBundle, Consts.XML_ENCHANTSKILL_FILE));

            this.OnProgressBarReport?.Invoke(32);
            yield return wait;
            Loader.LoadAutoSkill(ResourceLoader.LoadXMLFromBundle(this.GameConfigAssetBundle, Consts.XML_AUTOSKILL_FILE));

            this.OnProgressBarReport?.Invoke(33);
            yield return wait;
            Loader.LoadSkillData(ResourceLoader.LoadXMLFromBundle(this.GameConfigAssetBundle, Consts.XML_SKILLDATA_FILE));

            this.OnProgressBarReport?.Invoke(35);
            yield return wait;
            Loader.LoadElement(ResourceLoader.LoadXMLFromBundle(this.GameConfigAssetBundle, Consts.XML_ELEMENT_FILE));

            this.OnProgressBarReport?.Invoke(36);
            yield return wait;
            Loader.LoadActivities(ResourceLoader.LoadXMLFromBundle(this.GameConfigAssetBundle, Consts.XML_ACTIIVTY_LIST));

            this.OnProgressBarReport?.Invoke(37);
            yield return wait;
            Loader.LoadBulletActionSetSound(ResourceLoader.LoadBytesFromBundle(this.GameConfigAssetBundle, Consts.XML_BULLETACTIONSETSOUND));

            this.OnProgressBarReport?.Invoke(38);
            yield return wait;
            Loader.LoadCharacterActionSetXML(ResourceLoader.LoadXMLFromBundle(this.GameConfigAssetBundle, Consts.XML_ACTIONSET_CONFIG));

            this.OnProgressBarReport?.Invoke(39);
            yield return wait;
            Loader.LoadMantleTitles(ResourceLoader.LoadXMLFromBundle(this.GameConfigAssetBundle, Consts.XML_MANTLE_TITLE));
            Loader.LoadOfficeTitles(ResourceLoader.LoadXMLFromBundle(this.GameConfigAssetBundle, Consts.XML_OFFICE_TITLE));
            Loader.LoadRoleTitles(ResourceLoader.LoadXMLFromBundle(this.GameConfigAssetBundle, Consts.XML_ROLE_TITLE));
            Loader.LoadSpecialTitles(ResourceLoader.LoadXMLFromBundle(this.GameConfigAssetBundle, Consts.XML_SPECIAL_TITLE));

            this.OnProgressBarReport?.Invoke(40);
            yield return wait;
            Loader.LoadSeashellCircle(ResourceLoader.LoadXMLFromBundle(this.GameConfigAssetBundle, Consts.XML_ACTIIVTY_SEASHELLCIRCLE));

            this.OnProgressBarReport?.Invoke(41);
            yield return wait;
            Loader.LoadBulletConfig(ResourceLoader.LoadXMLFromBundle(this.GameConfigAssetBundle, Consts.XML_BULLETCONFIG));

            this.OnProgressBarReport?.Invoke(42);
            yield return wait;
            Loader.LoadBulletActionSetXML(ResourceLoader.LoadXMLFromBundle(this.GameConfigAssetBundle, Consts.XML_BULLETACTIONSET_CONFIG));

            yield return wait;
            yield return this.LoadSkillActionSetSound();

            this.OnProgressBarReport?.Invoke(45);
            yield return wait;
            Loader.LoadCharacterActionSetLayerSort(ResourceLoader.LoadXMLFromBundle(this.GameConfigAssetBundle, Consts.XML_ACTIONSET_LAYERSORT));

            this.OnProgressBarReport?.Invoke(50);
            yield return wait;
            yield return Loader.LoadItems(this.GameConfigAssetBundle, this.OnProgressBarReport);

            this.OnProgressBarReport?.Invoke(90);
            yield return wait;
            Loader.LoadItemValue(ResourceLoader.LoadXMLFromBundle(this.GameConfigAssetBundle, Consts.ItemValueCalculation));
            Loader.LoadSignetExp(ResourceLoader.LoadXMLFromBundle(this.GameConfigAssetBundle, Consts.SignetExp));
            Loader.LoadEquipRefine(ResourceLoader.LoadXMLFromBundle(this.GameConfigAssetBundle, Consts.EquipRefineRecipe));

            this.OnProgressBarReport?.Invoke(92);
            yield return wait;
            Loader.LoadTasks(ResourceLoader.LoadXMLFromBundle(this.GameConfigAssetBundle, Consts.XML_SYSTEMTASK));

            this.OnProgressBarReport?.Invoke(93);
            yield return wait;
            byte[] actionConfigBytes = ResourceLoader.LoadBytesFromBundle(this.GameConfigAssetBundle, Consts.XML_ACTIONCONFIG);
            Loader.LoadActionConfig(actionConfigBytes);


            this.OnProgressBarReport?.Invoke(94);
            yield return wait;
            byte[] characterActionSetSoundBytes = ResourceLoader.LoadBytesFromBundle(this.GameConfigAssetBundle, Consts.XML_CHARACTERACTIONSETSOUND);
            Loader.LoadCharacterActionSetSound(characterActionSetSoundBytes);


            this.OnProgressBarReport?.Invoke(95);
            yield return wait;
            byte[] monsterActionSetSoundBytes = ResourceLoader.LoadBytesFromBundle(this.GameConfigAssetBundle, Consts.XML_MONSTERACTIONSETSOUND);
            Loader.LoadMonsterActionSetSound(monsterActionSetSoundBytes);


            this.OnProgressBarReport?.Invoke(96);
            yield return wait;
            Loader.LoadLifeSkills(ResourceLoader.LoadXMLFromBundle(this.GameConfigAssetBundle, Consts.XML_LIFESKILL));


            this.OnProgressBarReport?.Invoke(97);
            yield return wait;
            Loader.LoadReputes(ResourceLoader.LoadXMLFromBundle(this.GameConfigAssetBundle, Consts.XML_REPUTE));

            /// Mặc định tải xuống âm thanh động tác nhân vật
            KTResourceManager.Instance.LoadAssetBundle(Loader.CharacterActionSetSoundBundleDir, false, KTResourceManager.KTResourceCacheType.CachedPermenently);
            /// Mặc định tải xuống âm thanh động tác quái
            KTResourceManager.Instance.LoadAssetBundle(Loader.MonsterActionSetSoundBundleDir, false, KTResourceManager.KTResourceCacheType.CachedPermenently);
            /// Mặc định tải xuống âm thanh động tác kỹ năng
            KTResourceManager.Instance.LoadAssetBundle(Loader.SkillCastSoundBundleDir, false, KTResourceManager.KTResourceCacheType.CachedPermenently);
            /// Mặc định tải xuống âm thanh đạn
            KTResourceManager.Instance.LoadAssetBundle(Loader.BulletActionSetXML.SoundBundleDir, false, KTResourceManager.KTResourceCacheType.CachedPermenently);


#if UNITY_EDITOR
            this.DoEditorLoad();
#endif

            this.OnProgressBarReport?.Invoke(98);
            yield return wait;
            this.LoadInitResource();

            this.OnProgressBarReport?.Invoke(100);
            yield return wait;
            this.OnLoadFinish?.Invoke();

            /// Xóa đối tượng
            GameObject.Destroy(this.gameObject);

            /// Xóa AssetBundle
            this.GameConfigAssetBundle.Unload(true);
            GameObject.Destroy(this.GameConfigAssetBundle);
            #endregion

            /// Gọi GC dọn rác
            GC.Collect();
            //--------------------------------------------------------------------------------------------------
        }

#if UNITY_EDITOR
        /// <summary>
        /// Đọc File Text từ Windows
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private string ReadTextFileFromWindows(string fileName)
        {
            fileName = Application.streamingAssetsPath + "/" + "DataEditorTest" + "/" + fileName;
            if (!System.IO.File.Exists(fileName))
            {
                return null;
            }
            return System.IO.File.ReadAllText(fileName);
        }

        /// <summary>
        /// Thực hiện tải tài nguyên ở Editor
        /// </summary>
        private void DoEditorLoad()
        {

        }
#endif

        /// <summary>
        /// Tải xuống tài nguyên âm thanh kỹ năng
        /// </summary>
        /// <returns></returns>
        private IEnumerator LoadSkillActionSetSound()
        {
            yield return KTResourceManager.Instance.LoadAssetBundleAsync(Consts.SKILLCASTSOUND_FILE, false, KTResourceManager.KTResourceCacheType.CachedPermenently);
        }

        /// <summary>
        /// Tải xuống tài nguyên mặc định
        /// </summary>
        private void LoadInitResource()
        {
            #region Series
            {
                string url = Global.WebPath(string.Format("Data/{0}", Loader.ElementXML.BundleDir));
#if UNITY_IOS || UNITY_EDITOR
                if (url.Contains("file:///"))
                {
                    url = url.Replace("file:///", "");
                }
#endif
                AssetBundle bundle = ResourceLoader.LoadAssetBundle(url);
                Loader.Elements[FS.VLTK.Entities.Enum.Elemental.METAL] = new ElementData()
                {
                    ElementType = FS.VLTK.Entities.Enum.Elemental.METAL,
                    Name = Loader.ElementXML.Metal.Name,
                };
                Loader.Elements[FS.VLTK.Entities.Enum.Elemental.WOOD] = new ElementData()
                {
                    ElementType = FS.VLTK.Entities.Enum.Elemental.WOOD,
                    Name = Loader.ElementXML.Wood.Name,
                };
                Loader.Elements[FS.VLTK.Entities.Enum.Elemental.EARTH] = new ElementData()
                {
                    ElementType = FS.VLTK.Entities.Enum.Elemental.EARTH,
                    Name = Loader.ElementXML.Earth.Name,
                };
                Loader.Elements[FS.VLTK.Entities.Enum.Elemental.WATER] = new ElementData()
                {
                    ElementType = FS.VLTK.Entities.Enum.Elemental.WATER,
                    Name = Loader.ElementXML.Water.Name,
                };
                Loader.Elements[FS.VLTK.Entities.Enum.Elemental.FIRE] = new ElementData()
                {
                    ElementType = FS.VLTK.Entities.Enum.Elemental.FIRE,
                    Name = Loader.ElementXML.Fire.Name,
                };

                Sprite[] assets = bundle.LoadAssetWithSubAssets<UnityEngine.Sprite>(Loader.ElementXML.AtlasName);
                for (int i = 0; i < assets.Length; i++)
                {
                    UnityEngine.Sprite sprite = assets[i] as UnityEngine.Sprite;

                    if (sprite.name.Equals(Loader.ElementXML.Metal.SmallImage))
                    {
                        Loader.Elements[FS.VLTK.Entities.Enum.Elemental.METAL].SmallSprite = sprite;
                    }
                    if (sprite.name.Equals(Loader.ElementXML.Metal.NormalImage))
                    {
                        Loader.Elements[FS.VLTK.Entities.Enum.Elemental.METAL].NormalSprite = sprite;
                    }
                    if (sprite.name.Equals(Loader.ElementXML.Metal.BigImage))
                    {
                        Loader.Elements[FS.VLTK.Entities.Enum.Elemental.METAL].BigSprite = sprite;
                    }
                    if (sprite.name.Equals(Loader.ElementXML.Wood.SmallImage))
                    {
                        Loader.Elements[FS.VLTK.Entities.Enum.Elemental.WOOD].SmallSprite = sprite;
                    }
                    if (sprite.name.Equals(Loader.ElementXML.Wood.NormalImage))
                    {
                        Loader.Elements[FS.VLTK.Entities.Enum.Elemental.WOOD].NormalSprite = sprite;
                    }
                    if (sprite.name.Equals(Loader.ElementXML.Wood.BigImage))
                    {
                        Loader.Elements[FS.VLTK.Entities.Enum.Elemental.WOOD].BigSprite = sprite;
                    }
                    if (sprite.name.Equals(Loader.ElementXML.Earth.SmallImage))
                    {
                        Loader.Elements[FS.VLTK.Entities.Enum.Elemental.EARTH].SmallSprite = sprite;
                    }
                    if (sprite.name.Equals(Loader.ElementXML.Earth.NormalImage))
                    {
                        Loader.Elements[FS.VLTK.Entities.Enum.Elemental.EARTH].NormalSprite = sprite;
                    }
                    if (sprite.name.Equals(Loader.ElementXML.Earth.BigImage))
                    {
                        Loader.Elements[FS.VLTK.Entities.Enum.Elemental.EARTH].BigSprite = sprite;
                    }
                    if (sprite.name.Equals(Loader.ElementXML.Water.SmallImage))
                    {
                        Loader.Elements[FS.VLTK.Entities.Enum.Elemental.WATER].SmallSprite = sprite;
                    }
                    if (sprite.name.Equals(Loader.ElementXML.Water.NormalImage))
                    {
                        Loader.Elements[FS.VLTK.Entities.Enum.Elemental.WATER].NormalSprite = sprite;
                    }
                    if (sprite.name.Equals(Loader.ElementXML.Water.BigImage))
                    {
                        Loader.Elements[FS.VLTK.Entities.Enum.Elemental.WATER].BigSprite = sprite;
                    }
                    if (sprite.name.Equals(Loader.ElementXML.Fire.SmallImage))
                    {
                        Loader.Elements[FS.VLTK.Entities.Enum.Elemental.FIRE].SmallSprite = sprite;
                    }
                    if (sprite.name.Equals(Loader.ElementXML.Fire.NormalImage))
                    {
                        Loader.Elements[FS.VLTK.Entities.Enum.Elemental.FIRE].NormalSprite = sprite;
                    }
                    if (sprite.name.Equals(Loader.ElementXML.Fire.BigImage))
                    {
                        Loader.Elements[FS.VLTK.Entities.Enum.Elemental.FIRE].BigSprite = sprite;
                    }
                }

                bundle.Unload(false);
            }
            #endregion

            #region UI/Icon
            /// Đường dẫn folder UI/Icon
            string uiIconFolderDir = Global.WebPath(string.Format("Data/{0}/Icon", Consts.UI_DIR), true);
            /// Đoạn này phải để (bất kể Android, IOS hay Editor) vì đây là Folder chứ không phải File nên thằng Android đọc vào sẽ lỗi
            if (uiIconFolderDir.Contains("file:///"))
            {
                uiIconFolderDir = uiIconFolderDir.Replace("file:///", "");
            }

            /// Không tồn tại
            if (!Directory.Exists(uiIconFolderDir))
            {
                KTGlobal.ShowMessageBox(string.Format("Không tìm thấy tài nguyên: '{0}'. Hãy liên hệ với Admin để báo cáo.", uiIconFolderDir));
            }
            else
            {
                /// Duyệt danh sách các File trong folder UI/Icon
                foreach (string fileDir in Directory.GetFiles(uiIconFolderDir, "*.unity3d"))
                {
                    /// Tên File
                    string fileName = Path.GetFileName(fileDir);
                    /// Tên Atlas
                    string atlasName = Path.GetFileNameWithoutExtension(fileDir);
                    /// Path
                    string relativePath = string.Format("{0}/Icon/{1}", Consts.UI_DIR, fileName);
                    /// Tải xuống Bundle tương ứng
                    KTResourceManager.Instance.LoadAssetBundle(relativePath, false, KTResourceManager.KTResourceCacheType.CachedPermenently);
                    /// Tải luôn SubAssets
                    KTResourceManager.Instance.LoadAssetWithSubAssets<Sprite>(relativePath, atlasName, true, KTResourceManager.KTResourceCacheType.CachedPermenently);
                }
            }
            #endregion

            #region Animated titles
            /// Phi phong
            string mantleTitlesBundleDir = string.Format("{0}/{1}", Consts.UI_DIR, Loader.MantleTitlesBundleDir);
            KTResourceManager.Instance.LoadAssetBundle(mantleTitlesBundleDir, false, KTResourceManager.KTResourceCacheType.CachedPermenently);
            KTResourceManager.Instance.LoadAssetWithSubAssets<Sprite>(mantleTitlesBundleDir, Loader.MantleTitlesAtlasName, true, KTResourceManager.KTResourceCacheType.CachedPermenently);
            /// Quan hàm
            string officeTitlesBundleDir = string.Format("{0}/{1}", Consts.UI_DIR, Loader.OfficeTitlesBundleDir);
            KTResourceManager.Instance.LoadAssetBundle(officeTitlesBundleDir, false, KTResourceManager.KTResourceCacheType.CachedPermenently);
            KTResourceManager.Instance.LoadAssetWithSubAssets<Sprite>(officeTitlesBundleDir, Loader.OfficeTitlesAtlasName, true, KTResourceManager.KTResourceCacheType.CachedPermenently);
            #endregion
        }
        #endregion
    }
}