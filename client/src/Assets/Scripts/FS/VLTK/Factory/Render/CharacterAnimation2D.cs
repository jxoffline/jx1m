using FS.VLTK.Control.Component;
using FS.VLTK.Entities.ActionSet.Character;
using FS.VLTK.Entities.Config;
using FS.VLTK.Factory;
using FS.VLTK.Factory.Animation;
using Server.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static FS.VLTK.Entities.Enum;
using static FS.VLTK.KTMath;

namespace FS.VLTK.Utilities.UnityComponent
{
    /// <summary>
    /// Đối tượng quản lý Animation 2D của nhân vật
    /// </summary>
    public class CharacterAnimation2D : TTMonoBehaviour
    {
        #region Private properties
        /// <summary>
        /// Luồng thực thi play hiệu ứng
        /// </summary>
        private Coroutine playerCoroutine;

        /// <summary>
        /// Danh sách điểm hiệu ứng cường hóa nếu vũ khí là triền thủ, phi đao, tụ tiễn
        /// </summary>
        private readonly List<Vector2Short> handEffectPoints = new List<Vector2Short>()
        {
            new Vector2Short(5, -5),
            new Vector2Short(10, -5),
        };

        /// <summary>
        /// Có phải vũ khí triền thủ, phi đao, tụ tiễn không
        /// </summary>
        private bool isHandWeapon = false;

        /// <summary>
        /// Dữ liệu lần trước
        /// </summary>
        private RoleDataMini _LastData = null;
        #endregion

        #region Defines
        #region Reference Objects
        /// <summary>
        /// Áo
        /// </summary>
        public SpriteRenderer Armor { get; set; }

        /// <summary>
        /// Đầu
        /// </summary>
        public SpriteRenderer Head { get; set; }

        /// <summary>
        /// Tóc
        /// </summary>
        public SpriteRenderer Hair { get; set; }

        /// <summary>
        /// Cánh tay trái
        /// </summary>
        public SpriteRenderer LeftHand { get; set; }

        /// <summary>
        /// Cánh tay phải
        /// </summary>
        public SpriteRenderer RightHand { get; set; }

        /// <summary>
        /// Vũ khí trái
        /// </summary>
        public SpriteRenderer LeftWeapon { get; set; }

        /// <summary>
        /// Vũ khí tay phải
        /// </summary>
        public SpriteRenderer RightWeapon { get; set; }

        /// <summary>
        /// Phi phong
        /// </summary>
        public SpriteRenderer Mantle { get; set; }

        /// <summary>
        /// Đầu ngựa
        /// </summary>
        public SpriteRenderer HorseHead { get; set; }

        /// <summary>
        /// Thân ngựa
        /// </summary>
        public SpriteRenderer HorseBody { get; set; }

        /// <summary>
        /// Đuôi ngựa
        /// </summary>
        public SpriteRenderer HorseTail { get; set; }

        /// <summary>
        /// Hiệu ứng cường hóa vũ khí trái
        /// </summary>
        public WeaponEnhanceEffect_Particle LeftWeaponEnhanceEffects { get; set; }

        /// <summary>
        /// Hiệu ứng cường hóa vũ khí phải
        /// </summary>
        public WeaponEnhanceEffect_Particle RightWeaponEnhanceEffects { get; set; }
        #endregion

        private RoleDataMini _Data = null;
        /// <summary>
        /// Dữ liệu nhân vật
        /// </summary>
        public RoleDataMini Data
        {
            get
            {
                return this._Data;
            }
            set
            {
                this._Data = value;

                this.OnDataChanged();
            }
        }

        /// <summary>
        /// Dữ liệu sắp xếp thêm vào
        /// </summary>
        public int AdditionSortingOrder { get; set; }

        /// <summary>
        /// Động tác trước đó
        /// </summary>
        public PlayerActionType LastAction { get; private set; }

        /// <summary>
        /// Đang chạy không
        /// </summary>
        public bool IsPlaying { get; private set; }

        /// <summary>
        /// Đang tạm dừng không
        /// </summary>
        public bool IsPausing { get; private set; } = false;

        /// <summary>
        /// Frame đang chạy hiện tại
        /// </summary>
        public int CurrentFrameID { get; private set; }

        /// <summary>
        /// Test ID Res quần áo
        /// </summary>
        public string FixedArmorResID { get; set; } = null;

        /// <summary>
        /// Test ID Res mũ
        /// </summary>
        public string FixedHeadResID { get; set; } = null;

        /// <summary>
        /// Test ID Res vũ khí
        /// </summary>
        public string FixedWeaponResID { get; set; } = null;

        /// <summary>
        /// Test ID Res phi phong
        /// </summary>
        public string FixedMantleResID { get; set; } = null;

        /// <summary>
        /// Test ID Res ngựa
        /// </summary>
        public string FixedHorseResID { get; set; } = null;
        #endregion

        #region Private methods
        /// <summary>
        /// Phương thức Async tải áo theo động tác hiện tại
        /// </summary>
        /// <param name="actionType"></param>
        /// <param name="isRiding"></param>
        /// <param name="callbackIfNeedToLoad"></param>
        /// <returns></returns>
        private IEnumerator LoadArmorAsync(PlayerActionType actionType, bool isRiding, Direction dir, Action callbackIfNeedToLoad)
        {
            string weaponID = this.FixedWeaponResID ?? KTGlobal.GetWeaponResByID(this.Data.WeaponID, this.Data.RoleSex);
            string bodyID = this.FixedArmorResID ?? KTGlobal.GetArmorResByID(this.Data.ArmorID, this.Data.RoleSex);
            bool isRide = isRiding;

            yield return CharacterAnimationManager.Instance.LoadArmorSprites(actionType, bodyID, weaponID, (Sex) this.Data.RoleSex, isRide, dir, callbackIfNeedToLoad);
        }

        /// <summary>
        /// Phương thức Async tải mũ theo động tác hiện tại
        /// </summary>
        /// <param name="actionType"></param>
        /// <param name="isRiding"></param>
        /// <param name="callbackIfNeedToLoad"></param>
        /// <returns></returns>
        private IEnumerator LoadHeadAsync(PlayerActionType actionType, bool isRiding, Direction dir, Action callbackIfNeedToLoad)
        {
            string weaponID = this.FixedWeaponResID ?? KTGlobal.GetWeaponResByID(this.Data.WeaponID, this.Data.RoleSex);
            string helmID = this.FixedHeadResID ?? KTGlobal.GetHelmResByID(this.Data.HelmID, this.Data.RoleSex);
            bool isRide = isRiding;

            yield return CharacterAnimationManager.Instance.LoadHelmSprites(actionType, helmID, weaponID, (Sex) this.Data.RoleSex, isRide, dir, callbackIfNeedToLoad);
        }

        /// <summary>
        /// Phương thức Async tải vũ khí theo động tác hiện tại
        /// </summary>
        /// <param name="actionType"></param>
        /// <param name="isRiding"></param>
        /// <param name="callbackIfNeedToLoad"></param>
        /// <returns></returns>
        private IEnumerator LoadWeaponAsync(PlayerActionType actionType, bool isRiding, Direction dir, Action callbackIfNeedToLoad)
        {
            string weaponID = this.FixedWeaponResID ?? KTGlobal.GetWeaponResByID(this.Data.WeaponID, this.Data.RoleSex);
            bool isRide = isRiding;

            yield return CharacterAnimationManager.Instance.LoadWeaponSprites(actionType, weaponID, (Sex) this.Data.RoleSex, isRide, dir, callbackIfNeedToLoad);
        }

        /// <summary>
        /// Phương thức Async tải phi phong theo động tác hiện tại
        /// </summary>
        /// <param name="actionType"></param>
        /// <param name="isRiding"></param>
        /// <param name="callbackIfNeedToLoad"></param>
        /// <returns></returns>
        private IEnumerator LoadMantleAsync(PlayerActionType actionType, bool isRiding, Direction dir, Action callbackIfNeedToLoad)
        {
            string weaponID = this.FixedWeaponResID ?? KTGlobal.GetWeaponResByID(this.Data.WeaponID, this.Data.RoleSex);
            string mantleID = this.FixedMantleResID ?? KTGlobal.GetMantleResByID(this.Data.MantleID, this.Data.RoleSex);
            bool isRide = isRiding;

            yield return CharacterAnimationManager.Instance.LoadMantleSprites(actionType, mantleID, weaponID, (Sex) this.Data.RoleSex, isRide, dir, callbackIfNeedToLoad);
        }

        /// <summary>
        /// Phương thức Async tải ngựa theo động tác hiện tại
        /// </summary>
        /// <param name="actionType"></param>
        /// <param name="isRiding"></param>
        /// <param name="callbackIfNeedToLoad"></param>
        /// <returns></returns>
        private IEnumerator LoadHorseAsync(PlayerActionType actionType, bool isRiding, Direction dir, Action callbackIfNeedToLoad)
        {
            string weaponID = this.FixedWeaponResID ?? KTGlobal.GetWeaponResByID(this.Data.WeaponID, this.Data.RoleSex);
            string horseID = this.FixedHorseResID ?? KTGlobal.GetHorseResByID(this.Data.HorseID);
            bool isRide = isRiding;
            if (!isRide)
            {
                yield break;
            }

            yield return CharacterAnimationManager.Instance.LoadHorseSprites(actionType, horseID, weaponID, isRide, dir, callbackIfNeedToLoad);
        }

        /// <summary>
        /// Phương thức Async tải âm thanh theo động tác hiện tại
        /// </summary>
        /// <param name="actionType"></param>
        /// <param name="isRiding"></param>
        /// <returns></returns>
        private IEnumerator LoadSoundAsync(PlayerActionType actionType, bool isRiding)
        {
            string weaponID = KTGlobal.GetWeaponResByID(this.Data.WeaponID, this.Data.RoleSex);

            yield return CharacterAnimationManager.Instance.LoadSounds(actionType, weaponID, (Sex) this.Data.RoleSex, isRiding);
        }

        /// <summary>
        /// Hủy áo
        /// </summary>
        private void UnloadArmor()
        {
            if (this._LastData == null)
            {
                return;
            }

            if (this.LastAction.Equals(default))
            {
                return;
            }

            string weaponID = this.FixedWeaponResID ?? KTGlobal.GetWeaponResByID(this._LastData.WeaponID, this._LastData.RoleSex);
            string bodyID = this.FixedArmorResID ?? KTGlobal.GetArmorResByID(this._LastData.ArmorID, this._LastData.RoleSex);
            bool isRide = this._LastData.IsRiding;
            Sex sex = (Sex) this._LastData.RoleSex;

            CharacterAnimationManager.Instance.UnloadArmorSprites(this.LastAction, bodyID, weaponID, sex, isRide, (Direction) this._LastData.CurrentDir);
        }

        /// <summary>
        /// Hủy mũ
        /// </summary>
        private void UnloadHelm()
        {
            if (this._LastData == null)
            {
                return;
            }

            if (this.LastAction.Equals(default))
            {
                return;
            }

            string weaponID = this.FixedWeaponResID ?? KTGlobal.GetWeaponResByID(this._LastData.WeaponID, this._LastData.RoleSex);
            string helmID = this.FixedHeadResID ?? KTGlobal.GetHelmResByID(this._LastData.HelmID, this._LastData.RoleSex);
            bool isRide = this._LastData.IsRiding;
            Sex sex = (Sex) this._LastData.RoleSex;

            CharacterAnimationManager.Instance.UnloadHelmSprites(this.LastAction, helmID, weaponID, sex, isRide, (Direction) this._LastData.CurrentDir);
        }

        /// <summary>
        /// Hủy vũ khí
        /// </summary>
        private void UnloadWeapon()
        {
            if (this._LastData == null)
            {
                return;
            }

            if (this.LastAction.Equals(default))
            {
                return;
            }

            string weaponID = this.FixedWeaponResID ?? KTGlobal.GetWeaponResByID(this._LastData.WeaponID, this._LastData.RoleSex);
            bool isRide = this._LastData.IsRiding;
            Sex sex = (Sex) this._LastData.RoleSex;

            CharacterAnimationManager.Instance.UnloadWeaponSprites(this.LastAction, weaponID, sex, isRide, (Direction) this._LastData.CurrentDir);
        }

        /// <summary>
        /// Hủy phi phong
        /// </summary>
        private void UnloadMantle()
        {
            if (this._LastData == null)
            {
                return;
            }

            if (this.LastAction.Equals(default))
            {
                return;
            }

            string weaponID = this.FixedWeaponResID ?? KTGlobal.GetWeaponResByID(this._LastData.WeaponID, this._LastData.RoleSex);
            string mantleID = this.FixedMantleResID ?? KTGlobal.GetMantleResByID(this._LastData.MantleID, this._LastData.RoleSex);
            bool isRide = this._LastData.IsRiding;
            Sex sex = (Sex) this._LastData.RoleSex;

            CharacterAnimationManager.Instance.UnloadMantleSprites(this.LastAction, mantleID, weaponID, sex, isRide, (Direction) this._LastData.CurrentDir);
        }

        /// <summary>
        /// Hủy ngựa
        /// </summary>
        private void UnloadHorse()
        {
            if (this._LastData == null)
            {
                return;
            }

            if (this.LastAction.Equals(default))
            {
                return;
            }

            string weaponID = this.FixedWeaponResID ?? KTGlobal.GetWeaponResByID(this._LastData.WeaponID, this._LastData.RoleSex);
            string horseID = this.FixedHorseResID ?? KTGlobal.GetHorseResByID(this._LastData.HorseID);
            bool isRide = this._LastData.IsRiding;
            if (!isRide)
            {
                return;
            }

            CharacterAnimationManager.Instance.UnloadHorseSprites(this.LastAction, horseID, weaponID, (Direction) this._LastData.CurrentDir);
        }

        /// <summary>
        /// Hàm gọi đến khi dữ liệu nhân vật thay đổi
        /// </summary>
        private void OnDataChanged()
        {
            /// Xóa toàn bộ Sprite hiệu ứng
            this.RemoveAllAnimationSprites();

            if (this.LeftWeaponEnhanceEffects != null && this.RightWeaponEnhanceEffects != null)
            {
                /// Thông tin vũ khí
                GoodsData weaponGD = KTGlobal.GetWeaponData(this.Data);

                /// Lấy thông tin vũ khí
                if (weaponGD != null && Loader.Loader.Items.TryGetValue(weaponGD.GoodsID, out ItemData weapon))
                {
                    /// Loại vũ khí
                    int weaponCategory = weapon.Category;

                    /// Đánh dấu có phải vũ khí tay không, triền thủ hoặc tụ tiễn không
                    this.isHandWeapon = weaponCategory == (int) KE_EQUIP_WEAPON_CATEGORY.emKEQUIP_WEAPON_CATEGORY_HAND || weaponCategory == (int) KE_EQUIP_WEAPON_CATEGORY.emKEQUIP_WEAPON_CATEGORY_FLYBAR || weaponCategory == (int) KE_EQUIP_WEAPON_CATEGORY.emKEQUIP_WEAPON_CATEGORY_ARROW;

                    WeaponEnhanceConfigXML enhanceConfigXML;
                    /// Lấy thông tin hiệu ứng cường hóa vũ khí tương ứng theo loại, nếu không tồn tại thì lấy thiết lập mặc định cho tất cả các loại vũ khí
                    if (!Loader.Loader.WeaponEnhanceConfigXMLs.TryGetValue(weaponCategory, out enhanceConfigXML))
                    {
                        enhanceConfigXML = Loader.Loader.WeaponEnhanceConfigXMLs[-1];
                    }

                    /// Ngũ hành vũ khí
                    Elemental series = (Elemental) weaponGD.Series;

                    /// Thiết lập theo ngũ hành
                    WeaponEnhanceConfigXML.EffectBySeries effectBySeries;
                    /// Lấy thông tin hiệu ứng cường hóa theo ngũ hành, nếu không tồn tại thì lấy giá trị mặc định
                    if (!enhanceConfigXML.EffectsBySeries.TryGetValue(series, out effectBySeries))
                    {
                        effectBySeries = enhanceConfigXML.EffectsBySeries[Elemental.NONE];
                    }

                    /// Cấp cường hóa
                    int enhanceLevel = weaponGD.Forge_level;
                    enhanceLevel = Math.Max(0, enhanceLevel);
                    enhanceLevel = Math.Min(16, enhanceLevel);

                    Color effectColor = effectBySeries.EffectsByLevel[enhanceLevel].StarColor;
                    float effectAlpha = effectBySeries.EffectsByLevel[enhanceLevel].StarAlpha;
                    effectColor.a = effectAlpha;
                    Color weaponEffectColorFrom = effectBySeries.EffectsByLevel[enhanceLevel].BodyGlowColorFrom;
                    Color weaponEffectColorTo = effectBySeries.EffectsByLevel[enhanceLevel].BodyGlowColorTo;
                    float alpha = effectBySeries.EffectsByLevel[enhanceLevel].BodyAlpha;

                    int glowThreshold = effectBySeries.EffectsByLevel[enhanceLevel].BodyGlowThreshold;

                    this.LeftWeaponEnhanceEffects.Clear();
                    this.RightWeaponEnhanceEffects.Clear();

                    /// Nếu Alpha > 0
                    if (effectAlpha > 0)
                    {
                        this.RightWeaponEnhanceEffects.GlowAlpha = alpha;
                        this.RightWeaponEnhanceEffects.BodyGlowThreshold = glowThreshold;
                        this.RightWeaponEnhanceEffects.GlowColorRange = new KeyValuePair<Color, Color>(weaponEffectColorFrom, weaponEffectColorTo);
                        this.RightWeaponEnhanceEffects.PlayLater();

                        /// Nếu là các loại vũ khí 2 tay
                        if (weapon.Category == (int) KE_EQUIP_WEAPON_CATEGORY.emKEQUIP_WEAPON_CATEGORY_HAND || weapon.Category == (int) KE_EQUIP_WEAPON_CATEGORY.emKEQUIP_WEAPON_CATEGORY_HAMMER || weapon.Category == (int) KE_EQUIP_WEAPON_CATEGORY.emKEQUIP_WEAPON_CATEGORY_FLYBAR || weapon.Category == (int) KE_EQUIP_WEAPON_CATEGORY.emKEQUIP_WEAPON_CATEGORY_ARROW || weapon.Category == (int) KE_EQUIP_WEAPON_CATEGORY.emKEQUIP_WEAPON_CATEGORY_DOUBLESWORDS || weapon.Category == (int) KE_EQUIP_WEAPON_CATEGORY.emKEQUIP_WEAPON_CATEGORY_DART)
                        {
                            this.LeftWeaponEnhanceEffects.GlowAlpha = alpha;
                            this.LeftWeaponEnhanceEffects.BodyGlowThreshold = glowThreshold;
                            this.LeftWeaponEnhanceEffects.GlowColorRange = new KeyValuePair<Color, Color>(weaponEffectColorFrom, weaponEffectColorTo);
                            this.LeftWeaponEnhanceEffects.PlayLater();
                        }
                    }
                }
                else
                {
                    this.isHandWeapon = false;

                    this.RightWeaponEnhanceEffects.Clear();
                    this.LeftWeaponEnhanceEffects.Clear();
                }
            }
        }

        /// <summary>
        /// Thực hiện động tác
        /// </summary>
        /// <returns></returns>
        private IEnumerator DoPlay(PlayerActionType actionType, bool isRiding, float duration, int repeatCount, float repeatAfter, bool isContinueFromCurrentFrame, Direction dir, Action Callback)
        {
            #region Prepare
            /// Dữ liệu đầu vào
            string bodyID = this.FixedArmorResID ?? KTGlobal.GetArmorResByID(this._Data.ArmorID, this.Data.RoleSex);
            string helmID = this.FixedHeadResID ?? KTGlobal.GetHelmResByID(this._Data.HelmID, this.Data.RoleSex);
            string weaponID = this.FixedWeaponResID ?? KTGlobal.GetWeaponResByID(this._Data.WeaponID, this.Data.RoleSex);
            string mantleID = this.FixedMantleResID ?? KTGlobal.GetMantleResByID(this._Data.MantleID, this.Data.RoleSex);
            string horseID = this.FixedHorseResID ?? KTGlobal.GetHorseResByID(this._Data.HorseID);

            Dictionary<string, UnityEngine.Object> fullArmorActionSet = CharacterAnimationManager.Instance.GetArmorSprites(actionType, bodyID, weaponID, (Sex) this._Data.RoleSex, isRiding, dir);
            Dictionary<string, UnityEngine.Object> fullHelmActionSet = CharacterAnimationManager.Instance.GetHelmSprites(actionType, helmID, weaponID, (Sex) this._Data.RoleSex, isRiding, dir);
            Dictionary<string, UnityEngine.Object> fullWeaponActionSet = CharacterAnimationManager.Instance.GetWeaponSprites(actionType, weaponID, (Sex) this._Data.RoleSex, isRiding, dir);
            Dictionary<string, UnityEngine.Object> fullMantleActionSet = CharacterAnimationManager.Instance.GetMantleSprites(actionType, mantleID, weaponID, (Sex) this._Data.RoleSex, isRiding, dir);
            Dictionary<string, UnityEngine.Object> fullHorseActionSet = CharacterAnimationManager.Instance.GetHorseSprites(actionType, horseID, weaponID, isRiding, dir);

            List<ActionSetFrameInfo> armorActionSet = null;
            List<ActionSetFrameInfo> leftHandActionSet = null;
            List<ActionSetFrameInfo> rightHandActionSet = null;
            List<ActionSetFrameInfo> headActionSet = null;
            List<ActionSetFrameInfo> hairActionSet = null;
            List<ActionSetFrameInfo> leftWeaponActionSet = null;
            List<ActionSetFrameInfo> rightWeaponActionSet = null;
            List<ActionSetFrameInfo> mantleActionSet = null;
            List<ActionSetFrameInfo> horseHeadActionSet = null;
            List<ActionSetFrameInfo> horseBodyActionSet = null;
            List<ActionSetFrameInfo> horseTailActionSet = null;

            /// Tên động tác
            string actionName = CharacterAnimationManager.Instance.GetActionName(actionType, weaponID, isRiding);

            /// Nếu tồn tại áo
            if (fullArmorActionSet != null)
            {
                armorActionSet = new List<ActionSetFrameInfo>();
                leftHandActionSet = new List<ActionSetFrameInfo>();
                rightHandActionSet = new List<ActionSetFrameInfo>();

                /// Duyệt danh sách
                foreach (KeyValuePair<string, UnityEngine.Object> pair in fullArmorActionSet)
                {
                    /// Nếu là áo
                    if (pair.Key.Contains("BODY_"))
                    {
                        int frameID = int.Parse(pair.Key.Replace("BODY_", ""));
                        armorActionSet.Add(new ActionSetFrameInfo()
                        {
                            Sprite = pair.Value as Sprite,
                            Layer = Loader.Loader.CharacterActionSetLayerSort.GetLayer(this._Data.RoleSex, frameID, (int) dir, actionName, 5),
                        });
                    }
                    /// Nếu là tay trái
                    else if (pair.Key.Contains("LEFTHAND_"))
                    {
                        int frameID = int.Parse(pair.Key.Replace("LEFTHAND_", ""));
                        leftHandActionSet.Add(new ActionSetFrameInfo()
                        {
                            Sprite = pair.Value as Sprite,
                            Layer = Loader.Loader.CharacterActionSetLayerSort.GetLayer(this._Data.RoleSex, frameID, (int) dir, actionName, 6),
                        });
                    }
                    /// Nếu là tay phải
                    else if (pair.Key.Contains("RIGHTHAND_"))
                    {
                        int frameID = int.Parse(pair.Key.Replace("RIGHTHAND_", ""));
                        rightHandActionSet.Add(new ActionSetFrameInfo()
                        {
                            Sprite = pair.Value as Sprite,
                            Layer = Loader.Loader.CharacterActionSetLayerSort.GetLayer(this._Data.RoleSex, frameID, (int) dir, actionName, 7),
                        });
                    }
                }

                /// Xóa dữ liệu tạm
                fullArmorActionSet = null;
            }

            /// Nếu tồn tại mũ
            if (fullHelmActionSet != null)
            {
                headActionSet = new List<ActionSetFrameInfo>();
                hairActionSet = new List<ActionSetFrameInfo>();

                /// Duyệt danh sách
                foreach (KeyValuePair<string, UnityEngine.Object> pair in fullHelmActionSet)
                {
                    /// Nếu là mũ
                    if (pair.Key.Contains("HEAD_"))
                    {
                        int frameID = int.Parse(pair.Key.Replace("HEAD_", ""));
                        headActionSet.Add(new ActionSetFrameInfo()
                        {
                            Sprite = pair.Value as Sprite,
                            Layer = Loader.Loader.CharacterActionSetLayerSort.GetLayer(this._Data.RoleSex, frameID, (int) dir, actionName, 0),
                        });
                    }
                    /// Nếu là tóc
                    else if (pair.Key.Contains("HAIR_"))
                    {
                        int frameID = int.Parse(pair.Key.Replace("HAIR_", ""));
                        hairActionSet.Add(new ActionSetFrameInfo()
                        {
                            Sprite = pair.Value as Sprite,
                            Layer = Loader.Loader.CharacterActionSetLayerSort.GetLayer(this._Data.RoleSex, frameID, (int) dir, actionName, 1),
                        });
                    }
                }

                /// Xóa dữ liệu tạm
                fullHelmActionSet = null;
            }

            /// Nếu tồn tại vũ khí
            if (fullWeaponActionSet != null)
            {
                leftWeaponActionSet = new List<ActionSetFrameInfo>();
                rightWeaponActionSet = new List<ActionSetFrameInfo>();

                /// Duyệt danh sách
                foreach (KeyValuePair<string, UnityEngine.Object> pair in fullWeaponActionSet)
                {
                    /// Nếu là mũ
                    if (pair.Key.Contains("WEPLEFT_"))
                    {
                        int frameID = int.Parse(pair.Key.Replace("WEPLEFT_", ""));
                        leftWeaponActionSet.Add(new ActionSetFrameInfo()
                        {
                            Sprite = pair.Value as Sprite,
                            Layer = Loader.Loader.CharacterActionSetLayerSort.GetLayer(this._Data.RoleSex, frameID, (int) dir, actionName, 8),
                        });
                    }
                    /// Nếu là tóc
                    else if (pair.Key.Contains("WEPRIGHT_"))
                    {
                        int frameID = int.Parse(pair.Key.Replace("WEPRIGHT_", ""));
                        rightWeaponActionSet.Add(new ActionSetFrameInfo()
                        {
                            Sprite = pair.Value as Sprite,
                            Layer = Loader.Loader.CharacterActionSetLayerSort.GetLayer(this._Data.RoleSex, frameID, (int) dir, actionName, 9),
                        });
                    }
                }

                /// Xóa dữ liệu tạm
                fullWeaponActionSet = null;
            }

            /// Nếu tồn tại phi phong
            if (fullMantleActionSet != null)
            {
                mantleActionSet = new List<ActionSetFrameInfo>();

                /// Duyệt danh sách
                foreach (KeyValuePair<string, UnityEngine.Object> pair in fullMantleActionSet)
                {
                    int frameID = int.Parse(pair.Key.Replace("MANTLE_", ""));
                    mantleActionSet.Add(new ActionSetFrameInfo()
                    {
                        Sprite = pair.Value as Sprite,
                        Layer = Loader.Loader.CharacterActionSetLayerSort.GetLayer(this._Data.RoleSex, frameID, (int) dir, actionName, 16),
                    });
                }

                /// Xóa dữ liệu tạm
                fullMantleActionSet = null;
            }

            /// Nếu tồn tại ngựa
            if (isRiding && fullHorseActionSet != null)
            {
                horseHeadActionSet = new List<ActionSetFrameInfo>();
                horseBodyActionSet = new List<ActionSetFrameInfo>();
                horseTailActionSet = new List<ActionSetFrameInfo>();

                /// Duyệt danh sách
                foreach (KeyValuePair<string, UnityEngine.Object> pair in fullHorseActionSet)
                {
                    /// Nếu là mũ
                    if (pair.Key.Contains("HORSEFONT_"))
                    {
                        int frameID = int.Parse(pair.Key.Replace("HORSEFONT_", ""));
                        horseHeadActionSet.Add(new ActionSetFrameInfo()
                        {
                            Sprite = pair.Value as Sprite,
                            Layer = Loader.Loader.CharacterActionSetLayerSort.GetLayer(this._Data.RoleSex, frameID, (int) dir, actionName, 12),
                        });
                    }
                    /// Nếu là tóc
                    else if (pair.Key.Contains("HORSEMID_"))
                    {
                        int frameID = int.Parse(pair.Key.Replace("HORSEMID_", ""));
                        horseBodyActionSet.Add(new ActionSetFrameInfo()
                        {
                            Sprite = pair.Value as Sprite,
                            Layer = Loader.Loader.CharacterActionSetLayerSort.GetLayer(this._Data.RoleSex, frameID, (int) dir, actionName, 13),
                        });
                    }
                    /// Nếu là tóc
                    else if (pair.Key.Contains("HORSEBACK_"))
                    {
                        int frameID = int.Parse(pair.Key.Replace("HORSEBACK_", ""));
                        horseTailActionSet.Add(new ActionSetFrameInfo()
                        {
                            Sprite = pair.Value as Sprite,
                            Layer = Loader.Loader.CharacterActionSetLayerSort.GetLayer(this._Data.RoleSex, frameID, (int) dir, actionName, 14),
                        });
                    }
                }

                /// Xóa dữ liệu tạm
                fullHorseActionSet = null;
            }
            #endregion

            /// Thực hiện hiệu ứng vũ khí
            if (this.RightWeaponEnhanceEffects != null && this.RightWeaponEnhanceEffects.IsWaitingToPlay())
            {
                this.RightWeaponEnhanceEffects.Play();
            }
            if (this.LeftWeaponEnhanceEffects != null && this.LeftWeaponEnhanceEffects.IsWaitingToPlay())
            {
                this.LeftWeaponEnhanceEffects.Play();
            }

            /// Nếu không phải tiếp tục ở Frame hiện tại
            if (!isContinueFromCurrentFrame)
            {
                this.CurrentFrameID = -1;
            }

            this.IsPlaying = true;

            float dTime = duration / armorActionSet.Count;

            WaitForSeconds waitDTime = new WaitForSeconds(dTime);

            /// Tổng số lần đã lặp
            int totalRepeated = 1;

            while (true)
            {
                if (this.IsPausing)
                {
                    yield return null;
                    continue;
                }

                this.CurrentFrameID++;
                if (this.CurrentFrameID >= armorActionSet.Count)
                {
                    /// Nếu vẫn còn lặp lại
                    if (totalRepeated < repeatCount)
                    {
                        /// Tăng số lần lặp lên
                        totalRepeated = (totalRepeated + 1) % 100000007;

                        this.CurrentFrameID = -1;
                        if (repeatAfter > 0)
                        {
                            yield return new WaitForSeconds(repeatAfter);
                        }
                        yield return null;
                        continue;
                    }
                    else
                    {
                        this.IsPlaying = false;
                        break;
                    }
                }

                if (this.CurrentFrameID == 0)
                {
                    #region Sound
                    AudioPlayer player = this.gameObject.GetComponent<AudioPlayer>();
                    player.Stop();
                    string soundName = CharacterAnimationManager.Instance.GetSoundName(actionType, KTGlobal.GetWeaponResByID(this.Data.WeaponID, this.Data.RoleSex), (Sex) this.Data.RoleSex, isRiding);
                    if (!string.IsNullOrEmpty(soundName))
                    {
                        AudioClip sound = KTResourceManager.Instance.GetAsset<UnityEngine.AudioClip>(Loader.Loader.CharacterActionSetSoundBundleDir, soundName);
                        if (sound != null)
                        {
                            player.Sound = sound;
                            player.IsRepeat = false;
                            player.RepeatTimer = duration - sound.length;
                            player.Play();
                        }
                    }
                    #endregion
                }

                #region Play actions
                if (armorActionSet != null && this.CurrentFrameID < armorActionSet.Count && armorActionSet[this.CurrentFrameID] != null && armorActionSet[this.CurrentFrameID].Sprite != null)
                {
                    this.Armor.sprite = armorActionSet[this.CurrentFrameID].Sprite;
                    this.Armor.drawMode = SpriteDrawMode.Sliced;
                    this.Armor.size = armorActionSet[this.CurrentFrameID].Sprite.rect.size;
                    this.Armor.gameObject.transform.localPosition = new Vector3(0, 0, -(this.AdditionSortingOrder / 10000f + (armorActionSet[this.CurrentFrameID].Layer) / 1000000f));
                }
                else
                {
                    this.Armor.sprite = null;
                }

                if (leftHandActionSet != null && this.CurrentFrameID < leftHandActionSet.Count && leftHandActionSet[this.CurrentFrameID] != null && leftHandActionSet[this.CurrentFrameID].Sprite != null)
                {
                    this.LeftHand.sprite = leftHandActionSet[this.CurrentFrameID].Sprite;
                    this.LeftHand.drawMode = SpriteDrawMode.Sliced;
                    this.LeftHand.size = leftHandActionSet[this.CurrentFrameID].Sprite.rect.size;
                    this.LeftHand.gameObject.transform.localPosition = new Vector3(0, 0, -(this.AdditionSortingOrder / 10000f + (leftHandActionSet[this.CurrentFrameID].Layer) / 1000000f));
                }
                else
                {
                    this.LeftHand.sprite = null;
                }

                if (rightHandActionSet != null && this.CurrentFrameID < rightHandActionSet.Count && rightHandActionSet[this.CurrentFrameID] != null && rightHandActionSet[this.CurrentFrameID].Sprite != null)
                {
                    this.RightHand.sprite = rightHandActionSet[this.CurrentFrameID].Sprite;
                    this.RightHand.drawMode = SpriteDrawMode.Sliced;
                    this.RightHand.size = rightHandActionSet[this.CurrentFrameID].Sprite.rect.size;
                    this.RightHand.gameObject.transform.localPosition = new Vector3(0, 0, -(this.AdditionSortingOrder / 10000f + (rightHandActionSet[this.CurrentFrameID].Layer) / 1000000f));
                }
                else
                {
                    this.RightHand.sprite = null;
                }

                if (headActionSet != null && this.CurrentFrameID < headActionSet.Count && headActionSet[this.CurrentFrameID] != null && headActionSet[this.CurrentFrameID].Sprite != null)
                {
                    this.Head.sprite = headActionSet[this.CurrentFrameID].Sprite;
                    this.Head.drawMode = SpriteDrawMode.Sliced;
                    this.Head.size = headActionSet[this.CurrentFrameID].Sprite.rect.size;
                    this.Head.gameObject.transform.localPosition = new Vector3(0, 0, -(this.AdditionSortingOrder / 10000f + (headActionSet[this.CurrentFrameID].Layer) / 1000000f));
                }
                else
                {
                    this.Head.sprite = null;
                }

                if (hairActionSet != null && this.CurrentFrameID < hairActionSet.Count && hairActionSet[this.CurrentFrameID] != null && hairActionSet[this.CurrentFrameID].Sprite != null)
                {
                    this.Hair.sprite = hairActionSet[this.CurrentFrameID].Sprite;
                    this.Hair.drawMode = SpriteDrawMode.Sliced;
                    this.Hair.size = hairActionSet[this.CurrentFrameID].Sprite.rect.size;
                    this.Hair.gameObject.transform.localPosition = new Vector3(0, 0, -(this.AdditionSortingOrder / 10000f + (hairActionSet[this.CurrentFrameID].Layer) / 1000000f));
                }
                else
                {
                    this.Hair.sprite = null;
                }

                if (leftWeaponActionSet != null && this.CurrentFrameID < leftWeaponActionSet.Count && leftWeaponActionSet[this.CurrentFrameID] != null && leftWeaponActionSet[this.CurrentFrameID].Sprite != null)
                {
                    this.LeftWeapon.sprite = leftWeaponActionSet[this.CurrentFrameID].Sprite;
                    this.LeftWeapon.drawMode = SpriteDrawMode.Sliced;
                    this.LeftWeapon.size = leftWeaponActionSet[this.CurrentFrameID].Sprite.rect.size;
                    this.LeftWeapon.gameObject.transform.localPosition = new Vector3(0, 0, -(this.AdditionSortingOrder / 10000f + (leftWeaponActionSet[this.CurrentFrameID].Layer) / 1000000f));
                }
                else
                {
                    this.LeftWeapon.sprite = null;
                }

                if (rightWeaponActionSet != null && this.CurrentFrameID < rightWeaponActionSet.Count && rightWeaponActionSet[this.CurrentFrameID] != null && rightWeaponActionSet[this.CurrentFrameID].Sprite != null)
                {
                    this.RightWeapon.sprite = rightWeaponActionSet[this.CurrentFrameID].Sprite;
                    this.RightWeapon.drawMode = SpriteDrawMode.Sliced;
                    this.RightWeapon.size = rightWeaponActionSet[this.CurrentFrameID].Sprite.rect.size;
                    this.RightWeapon.gameObject.transform.localPosition = new Vector3(0, 0, -(this.AdditionSortingOrder / 10000f + (rightWeaponActionSet[this.CurrentFrameID].Layer) / 1000000f));
                }
                else
                {
                    this.RightWeapon.sprite = null;
                }

                if (mantleActionSet != null && this.CurrentFrameID < mantleActionSet.Count && mantleActionSet[this.CurrentFrameID] != null && mantleActionSet[this.CurrentFrameID].Sprite != null)
                {
                    this.Mantle.sprite = mantleActionSet[this.CurrentFrameID].Sprite;
                    this.Mantle.drawMode = SpriteDrawMode.Sliced;
                    this.Mantle.size = mantleActionSet[this.CurrentFrameID].Sprite.rect.size;
                    this.Mantle.gameObject.transform.localPosition = new Vector3(0, 0, -(this.AdditionSortingOrder / 10000f + (mantleActionSet[this.CurrentFrameID].Layer) / 1000000f));
                }
                else
                {
                    this.Mantle.sprite = null;
                }

                if (this.HorseHead != null)
                {
                    if (horseHeadActionSet != null && this.CurrentFrameID < horseHeadActionSet.Count && horseHeadActionSet[this.CurrentFrameID] != null && horseHeadActionSet[this.CurrentFrameID].Sprite != null)
                    {
                        this.HorseHead.sprite = horseHeadActionSet[this.CurrentFrameID].Sprite;
                        this.HorseHead.drawMode = SpriteDrawMode.Sliced;
                        this.HorseHead.size = horseHeadActionSet[this.CurrentFrameID].Sprite.rect.size;
                        this.HorseHead.gameObject.transform.localPosition = new Vector3(0, 0, -(this.AdditionSortingOrder / 10000f + (horseHeadActionSet[this.CurrentFrameID].Layer) / 1000000f));
                    }
                    else
                    {
                        this.HorseHead.sprite = null;
                    }
                }


                if (this.HorseBody != null)
                {
                    if (horseBodyActionSet != null && this.CurrentFrameID < horseBodyActionSet.Count && horseBodyActionSet[this.CurrentFrameID] != null && horseBodyActionSet[this.CurrentFrameID].Sprite != null)
                    {
                        this.HorseBody.sprite = horseBodyActionSet[this.CurrentFrameID].Sprite;
                        this.HorseBody.drawMode = SpriteDrawMode.Sliced;
                        this.HorseBody.size = horseBodyActionSet[this.CurrentFrameID].Sprite.rect.size;
                        this.HorseBody.gameObject.transform.localPosition = new Vector3(0, 0, -(this.AdditionSortingOrder / 10000f + (horseBodyActionSet[this.CurrentFrameID].Layer) / 1000000f));
                    }
                    else
                    {
                        this.HorseBody.sprite = null;
                    }
                }


                if (this.HorseTail != null)
                {
                    if (horseTailActionSet != null && this.CurrentFrameID < horseTailActionSet.Count && horseTailActionSet[this.CurrentFrameID] != null && horseTailActionSet[this.CurrentFrameID].Sprite != null)
                    {
                        this.HorseTail.sprite = horseTailActionSet[this.CurrentFrameID].Sprite;
                        this.HorseTail.drawMode = SpriteDrawMode.Sliced;
                        this.HorseTail.size = horseTailActionSet[this.CurrentFrameID].Sprite.rect.size;
                        this.HorseTail.gameObject.transform.localPosition = new Vector3(0, 0, -(this.AdditionSortingOrder / 10000f + (horseTailActionSet[this.CurrentFrameID].Layer) / 1000000f));
                    }
                    else
                    {
                        this.HorseTail.sprite = null;
                    }
                }
                #endregion

                yield return waitDTime;
            }
            Callback?.Invoke();
        }
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi đến khi đối tượng bị ẩn
        /// </summary>
        private void OnDisable()
        {
            /// Unload toàn bộ Bundle cũ
            if (this._LastData != null)
            {
                this.UnloadArmor();
                this.UnloadHelm();
                this.UnloadWeapon();
                this.UnloadMantle();
                this.UnloadHorse();
            }
            this._LastData = null;
            this.Armor.sprite = null;
            this.Head.sprite = null;
            this.Hair.sprite = null;
            this.LeftHand.sprite = null;
            this.RightHand.sprite = null;
            this.LeftWeapon.sprite = null;
            this.RightWeapon.sprite = null;
            this.Mantle.sprite = null;
            if (this.HorseBody != null)
            {
                this.HorseBody.sprite = null;
                this.HorseHead.sprite = null;
                this.HorseTail.sprite = null;
            }

            this.StopAllCoroutines();
            this.playerCoroutine = null;
            this._Data = null;
            this.AdditionSortingOrder = 0;
            this.IsPlaying = false;
            this.IsPausing = false;
            this.CurrentFrameID = 0;

            this.FixedArmorResID = null;
            this.FixedHeadResID = null;
            this.FixedHorseResID = null;
            this.FixedMantleResID = null;
            this.FixedWeaponResID = null;
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Tải lại Res
        /// </summary>
        public void Reload()
        {
            this.OnDataChanged();
        }

        /// <summary>
        /// Tạm dừng thực hiện động tác
        /// </summary>
        public void Pause()
        {
            this.IsPausing = true;
        }

        /// <summary>
        /// Tiếp tục thực hiện động tác
        /// </summary>
        public void Resume()
        {
            this.IsPausing = false;
        }

        /// <summary>
        /// Dừng thực hiện động tác
        /// </summary>
        public void Stop()
        {
            if (this.playerCoroutine != null)
            {
                this.StopCoroutine(this.playerCoroutine);
                this.playerCoroutine = null;
            }
        }

        /// <summary>
        /// Phương thức Async thực hiện động tác
        /// </summary>
        /// <param name="isRiding"></param>
        /// <param name="actionType"></param>
        /// <param name="dir"></param>
        /// <param name="duration"></param>
        /// <param name="isRepeat"></param>
        /// <param name="repeatAfter"></param>
        /// <param name="isContinueFromCurrentFrame"></param>
        /// <param name="Callback"></param>
        /// <returns></returns>
        public IEnumerator DoActionAsync(bool isRiding, PlayerActionType actionType, Direction dir, float duration, int repeatCount = 0, float repeatAfter = 0, bool isContinueFromCurrentFrame = false, Action Callback = null)
        {
            //if (true)
            //{
            //    yield break;
            //}

            if (!this.gameObject.activeSelf)
            {
                yield break;
            }
            else if (actionType == PlayerActionType.None || dir == Direction.NONE)
            {
                yield break;
            }

            /// Nếu đang tạm dừng
            if (this.IsPausing)
            {
                yield break;
            }

            /// Unload toàn bộ Bundle cũ
            if (this._LastData != null)
            {
                this.UnloadArmor();
                this.UnloadHelm();
                this.UnloadWeapon();
                this.UnloadMantle();
                this.UnloadHorse();
            }
            this._LastData = new RoleDataMini()
            {
                ArmorID = this.Data.ArmorID,
                HelmID = this.Data.HelmID,
                WeaponID = this.Data.WeaponID,
                MantleID = this.Data.MantleID,
                HorseID = this.Data.HorseID,
                RoleSex = this.Data.RoleSex,
                CurrentDir = (int) dir,
                IsRiding = isRiding,
            };
            this.LastAction = actionType;

            #region Load sprites
            int totalLoaded = 0;
            IEnumerator DoLoadArmor()
            {
                yield return this.LoadArmorAsync(actionType, isRiding, dir, () => {
                    this.RemoveAllAnimationSprites();
                });
                totalLoaded++;
            }
            IEnumerator DoLoadHelm()
            {
                yield return this.LoadHeadAsync(actionType, isRiding, dir, () => {
                    this.RemoveAllAnimationSprites();
                });
                totalLoaded++;
            }
            IEnumerator DoLoadWeapon()
            {
                yield return this.LoadWeaponAsync(actionType, isRiding, dir, () => {
                    this.RemoveAllAnimationSprites();
                });
                totalLoaded++;
            }
            IEnumerator DoLoadMantle()
            {
                yield return this.LoadMantleAsync(actionType, isRiding, dir, () => {
                    this.RemoveAllAnimationSprites();
                });
                totalLoaded++;
            }
            IEnumerator DoLoadHorse()
            {
                yield return this.LoadHorseAsync(actionType, isRiding, dir, () => {
                    this.RemoveAllAnimationSprites();
                });
                totalLoaded++;
            }
            IEnumerator DoLoadSound()
            {
                yield return this.LoadSoundAsync(actionType, isRiding);
                totalLoaded++;
            }
            KTResourceManager.Instance.StartCoroutine(DoLoadArmor());
            KTResourceManager.Instance.StartCoroutine(DoLoadHelm());
            KTResourceManager.Instance.StartCoroutine(DoLoadWeapon());
            KTResourceManager.Instance.StartCoroutine(DoLoadMantle());
            KTResourceManager.Instance.StartCoroutine(DoLoadHorse());
            KTResourceManager.Instance.StartCoroutine(DoLoadSound());

            while (totalLoaded < 6)
            {
                yield return null;
            }
            #endregion

            #region Ngừng luồng cũ
            if (this.playerCoroutine != null)
            {
                this.StopCoroutine(this.playerCoroutine);
            }
            #endregion

            this.Stop();
            this.playerCoroutine = this.StartCoroutine(this.DoPlay(actionType, isRiding, duration, repeatCount, repeatAfter, isContinueFromCurrentFrame, dir, Callback));
        }

        /// <summary>
        /// Xóa toàn bộ Sprite hiệu ứng
        /// </summary>
        public void RemoveAllAnimationSprites()
        {
            if (this.playerCoroutine != null)
            {
                this.StopCoroutine(this.playerCoroutine);
                this.playerCoroutine = null;
            }
            this.Armor.sprite = null;
            this.Head.sprite = null;
            this.Hair.sprite = null;
            this.LeftHand.sprite = null;
            this.RightHand.sprite = null;
            this.LeftWeapon.sprite = null;
            this.RightWeapon.sprite = null;
            this.Mantle.sprite = null;
            if (this.HorseBody != null)
            {
                this.HorseBody.sprite = null;
                this.HorseHead.sprite = null;
                this.HorseTail.sprite = null;
            }
        }
        #endregion
    }
}
