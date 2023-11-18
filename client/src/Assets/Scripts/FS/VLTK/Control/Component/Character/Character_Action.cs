using FS.GameEngine.Logic;
using FS.VLTK.Entities.ActionSet;
using FS.VLTK.Entities.Config;
using FS.VLTK.Entities.Object;
using FS.VLTK.Factory;
using FS.VLTK.Loader;
using FS.VLTK.Logic.Settings;
using FS.VLTK.Utilities.UnityComponent;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using static FS.VLTK.Entities.Enum;

namespace FS.VLTK.Control.Component
{
    /// <summary>
    /// Quản lý động tác nhân vật
    /// </summary>
    public partial class Character
    {
        #region Define
        /// <summary>
        /// Bóng
        /// </summary>
        [SerializeField]
        private GameObject Shadow;

        /// <summary>
        /// Bóng khi cưỡi
        /// </summary>
        [SerializeField]
        private GameObject Shadow_Horse;

        /// <summary>
        /// Đầu
        /// </summary>
        [SerializeField]
        private GameObject Head;

        /// <summary>
        /// Tóc
        /// </summary>
        [SerializeField]
        private GameObject Hair;

        /// <summary>
        /// Thân
        /// </summary>
        [SerializeField]
        private GameObject Body;

        /// <summary>
        /// Tay trái
        /// </summary>
        [SerializeField]
        private GameObject LeftArm;

        /// <summary>
        /// Tay phải
        /// </summary>
        [SerializeField]
        private GameObject RightArm;

        /// <summary>
        /// Vũ khí tay trái
        /// </summary>
        [SerializeField]
        private GameObject LeftWeapon;

        /// <summary>
        /// Vũ khí tay phải
        /// </summary>
        [SerializeField]
        private GameObject RightWeapon;

        /// <summary>
        /// Phi phong
        /// </summary>
        [SerializeField]
        private GameObject Coat;

        #region Mask
        /// <summary>
        /// Mặt nạ
        /// </summary>
        [SerializeField]
        private GameObject Mask;
        #endregion

        #region Horse
        /// <summary>
        /// Đầu ngựa
        /// </summary>
        [SerializeField]
        private GameObject HorseHead;

        /// <summary>
        /// Thân ngựa
        /// </summary>
        [SerializeField]
        private GameObject HorseBody;

        /// <summary>
        /// Đuôi ngựa
        /// </summary>
        [SerializeField]
        private GameObject HorseTail;
        #endregion

        /// <summary>
        /// Đối tượng thực hiện động tác nhân vật
        /// </summary>
        private new CharacterAnimation2D animation = null;

        /// <summary>
        /// Đối tượng thực hiện động tác quái (khi người chơi đeo mặt nạ)
        /// </summary>
        private MonsterAnimation2D maskAnimation = null;

        /// <summary>
        /// Hiệu ứng cường hóa vũ khí trái
        /// </summary>
        [SerializeField]
        private GameObject LeftWeaponEnhanceEffects;

        /// <summary>
        /// Hiệu ứng cường hóa vũ khí phải
        /// </summary>
        [SerializeField]
        private GameObject RightWeaponEnhanceEffects;

        #region Trail
        /// <summary>
        /// Hiệu ứng đổ bóng đầu
        /// </summary>
        private SpriteTrailRenderer Trail_Head;

        /// <summary>
        /// Hiệu ứng đổ bóng tóc
        /// </summary>
        private SpriteTrailRenderer Trail_Hair;

        /// <summary>
        /// Hiệu ứng đổ bóng thân
        /// </summary>
        private SpriteTrailRenderer Trail_Body;

        /// <summary>
        /// Hiệu ứng đổ bóng tay trái
        /// </summary>
        private SpriteTrailRenderer Trail_LeftArm;

        /// <summary>
        /// Hiệu ứng đổ bóng tay phải
        /// </summary>
        private SpriteTrailRenderer Trail_RightArm;

        /// <summary>
        /// Hiệu ứng đổ bóng vũ khí trái
        /// </summary>
        private SpriteTrailRenderer Trail_LeftWeapon;

        /// <summary>
        /// Hiệu ứng đổ bóng vũ khí phải
        /// </summary>
        private SpriteTrailRenderer Trail_RightWeapon;

        /// <summary>
        /// Hiệu ứng đổ bóng phi phong
        /// </summary>
        private SpriteTrailRenderer Trail_Coat;

        /// <summary>
        /// Hiệu ứng đổ bóng đầu ngựa
        /// </summary>
        private SpriteTrailRenderer Trail_HorseHead;

        /// <summary>
        /// Hiệu ứng đổ bóng thân ngựa
        /// </summary>
        private SpriteTrailRenderer Trail_HorseBody;

        /// <summary>
        /// Hiệu ứng đổ bóng đuôi ngựa
        /// </summary>
        private SpriteTrailRenderer Trail_HorseTail;

        /// <summary>
        /// Hiệu ứng đổ bóng mặt nạ
        /// </summary>
        private SpriteTrailRenderer Trail_Mask;

        /// <summary>
        /// Kích hoạt mặt nạ không
        /// </summary>
        private bool isActiveMask = false;
        #endregion
        #endregion

        #region Properties
        /// <summary>
        /// Model của đối tượng
        /// </summary>
        public GameObject Model
        {
            get
            {
                return this.isActiveMask ? this.Mask.transform.parent.gameObject : this.Body.transform.parent.gameObject;
            }
        }

        private Vector3? _OriginModelPos;
        /// <summary>
        /// Vị trí gốc của Model
        /// </summary>
        public Vector3 OriginModelPos
        {
            get
            {
                if (!this._OriginModelPos.HasValue)
                {
                    this._OriginModelPos = this.Model.transform.localPosition;
                }
                return this._OriginModelPos.Value;
            }
        }
        #endregion

        #region Loader
        /// <summary>
        /// Thay đổi Res trang phục
        /// </summary>
        /// <param name="resID"></param>
        public void ChangeResArmor(string resID)
        {
            this.animation.RemoveAllAnimationSprites();
            this.animation.FixedArmorResID = resID;
            this.ReloadAnimation();
        }

        /// <summary>
        /// Thay đổi Res mũ
        /// </summary>
        /// <param name="resID"></param>
        public void ChangeResHelm(string resID)
        {
            this.animation.RemoveAllAnimationSprites();
            this.animation.FixedHeadResID = resID;
            this.ReloadAnimation();
        }

        /// <summary>
        /// Thay đổi Res vũ khí
        /// </summary>
        /// <param name="resID"></param>
        public void ChangeResWeapon(string resID)
        {
            this.animation.RemoveAllAnimationSprites();
            this.animation.FixedWeaponResID = resID;
            this.ReloadAnimation();
        }

        /// <summary>
        /// Thay đổi Res vũ khí
        /// </summary>
        /// <param name="resID"></param>
        public void ChangeResMantle(string resID)
        {
            this.animation.RemoveAllAnimationSprites();
            this.animation.FixedMantleResID = resID;
            this.ReloadAnimation();
        }

        /// <summary>
        /// Thay đổi Res ngựa
        /// </summary>
        /// <param name="resID"></param>
        public void ChangeResHorse(string resID)
        {
            this.animation.RemoveAllAnimationSprites();
            this.animation.FixedHorseResID = resID;
            this.ReloadAnimation();
        }

        /// <summary>
        /// Thay đổi trang phục
        /// </summary>
        /// <param name="armorID"></param>
        public void ChangeArmor(int armorID)
        {
            this.Data.ArmorID = armorID;
            this.ReloadAnimation();
        }

        /// <summary>
        /// Thay đổi mũ
        /// </summary>
        /// <param name="helmID"></param>
        public void ChangeHelm(int helmID)
        {
            this.Data.HelmID = helmID;
            this.ReloadAnimation();
        }

        /// <summary>
        /// Thay đổi vũ khí
        /// </summary>
        /// <param name="weaponID"></param>
        public void ChangeWeapon(int weaponID)
        {
            this.Data.WeaponID = weaponID;
            this.ReloadAnimation();
        }

        /// <summary>
        /// Thay đổi phi phong
        /// </summary>
        /// <param name="mantleID"></param>
        public void ChangeMantle(int mantleID)
        {
            this.Data.MantleID = mantleID;
            this.ReloadAnimation();
        }

        /// <summary>
        /// Thay đổi ngựa
        /// </summary>
        /// <param name="horseID"></param>
        public void ChangeHorse(int horseID)
        {
            this.Data.HorseID = horseID;
            this.ReloadAnimation();
        }

        /// <summary>
        /// Cập nhật RoleData
        /// </summary>
        public void UpdateRoleData()
        {
            this.animation.Data = this.Data;
        }

        /// <summary>
        /// Thiết lập ID res mặt nạ
        /// </summary>
        /// <param name="resID"></param>
        public void SetMaskID(string resID)
        {
            /// Nếu không có mặt nạ
            if (string.IsNullOrEmpty(resID))
            {
                /// Đánh dấu ẩn mặt nạ
                this.isActiveMask = false;
                /// Hiện Model tương ứng
                this.SetModelVisible(true);
            }
            /// Nếu có mặt nạ
            else
            {
                /// Đánh dấu hiện mặt nạ
                this.isActiveMask = true;
                /// Hiện Model tương ứng
                this.SetModelVisible(true);
                /// Thiết lập Res cho mặt nạ
                this.maskAnimation.ResID = resID;

                /// Dịch chuyển vị trí
                if (Loader.Loader.MonsterActionSetXML.Monsters.TryGetValue(resID, out MonsterActionSetXML.Component resData))
                {
                    this.Mask.transform.localPosition = new Vector2(resData.PosX, resData.PosY);
                }
            }

            this.ReloadAnimation();
        }
        #endregion

        /// <summary>
        /// Luồng thực thi hiệu ứng Async
        /// </summary>
        private Coroutine actionCoroutine;

        /// <summary>
        /// Luồng thực hiện tải lại động tác
        /// </summary>
        private Coroutine reloadAnimationCoroutine = null;

        /// <summary>
        /// Tải lại động tác
        /// </summary>
        private void ReloadAnimation()
        {
            if (this.reloadAnimationCoroutine != null)
            {
                this.StopCoroutine(this.reloadAnimationCoroutine);
            }
            this.reloadAnimationCoroutine = this.StartCoroutine(this.ExecuteSkipFrames(1, () => {
                if (!this.isActiveMask)
                {
                    this.animation.Reload();
                }
                else
                {
                    this.maskAnimation.Reload();
                }
                this.ResumeCurrentAction();
                this.reloadAnimationCoroutine = null;
            }));
        }

        /// <summary>
        /// Kích hoạt hiệu ứng bóng
        /// </summary>
        /// <param name="isActivated"></param>
        public void ActivateTrailEffect(bool isActivated)
        {
            /// Nếu FPS không phải mức cao thì mặc định Disable
            if (MainGame.Instance.GetRenderQuality() != MainGame.RenderQuality.High)
            {
                isActivated = false;
            }

            /// Bóng người
            this.Trail_Head.Enable = this.isActiveMask || KTSystemSetting.DisableTrailEffect ? false : isActivated;
            this.Trail_Hair.Enable = this.isActiveMask || KTSystemSetting.DisableTrailEffect ? false : isActivated;
            this.Trail_Body.Enable = this.isActiveMask || KTSystemSetting.DisableTrailEffect ? false : isActivated;
            this.Trail_LeftArm.Enable = this.isActiveMask || KTSystemSetting.DisableTrailEffect ? false : isActivated;
            this.Trail_LeftWeapon.Enable = this.isActiveMask || KTSystemSetting.DisableTrailEffect ? false : isActivated;
            this.Trail_RightArm.Enable = this.isActiveMask || KTSystemSetting.DisableTrailEffect ? false : isActivated;
            this.Trail_RightWeapon.Enable = this.isActiveMask || KTSystemSetting.DisableTrailEffect ? false : isActivated;
            this.Trail_Coat.Enable = this.isActiveMask || KTSystemSetting.DisableTrailEffect ? false : isActivated;
            this.Trail_HorseHead.Enable = this.isActiveMask || KTSystemSetting.DisableTrailEffect ? false : isActivated;
            this.Trail_HorseBody.Enable = this.isActiveMask || KTSystemSetting.DisableTrailEffect ? false : isActivated;
            this.Trail_HorseTail.Enable = this.isActiveMask || KTSystemSetting.DisableTrailEffect ? false : isActivated;

            /// Bóng mặt nạ
            this.Trail_Mask.Enable = !this.isActiveMask || KTSystemSetting.DisableTrailEffect ? false : isActivated;
        }


        /// <summary>
        /// Thiết lập Sorting Order
        /// </summary>
        public void SortingOrderHandler()
        {
            Vector2 currentPos = this.gameObject.transform.localPosition;
            this.gameObject.transform.localPosition = new Vector3(currentPos.x, currentPos.y, currentPos.y / 10000);
        }

        /// <summary>
        /// Tiếp tục thực hiện động tác hiện tại
        /// </summary>
        public void ResumeCurrentAction()
        {
            /// Làm mới trạng thái cưỡi
            this.UpdateRidingState();
            /// Thực thi thiết lập
            this.ExecuteSetting();
            /// Nếu sau khi thực thi thiết lập trả ra kết quả ẩn đối tượng thì bỏ qua
            if (this.lastHideRole)
            {
                return;
            }

            /// Nếu là động tác cưỡi ngựa thì chuyển sang bóng ngựa
            if (this.Data.IsRiding)
            {
                this.Shadow_Horse.gameObject.SetActive(true);
                this.Shadow.gameObject.SetActive(false);
            }
            else
            {
                this.Shadow_Horse.gameObject.SetActive(false);
                this.Shadow.gameObject.SetActive(true);
            }

            switch (this.RefObject.CurrentAction)
            {
                case KE_NPC_DOING.do_sit:
                    this.Sit(this.currentActionFrameSpeed);
                    break;
                case KE_NPC_DOING.do_jump:
                    this.Jump(this.currentActionFrameSpeed);
                    break;
                case KE_NPC_DOING.do_stand:
                    this.Stand();
                    break;
                case KE_NPC_DOING.do_run:
                    this.Run();
                    break;
                case KE_NPC_DOING.do_walk:
                    this.Walk();
                    break;
                case KE_NPC_DOING.do_attack:
                    this.Attack(this.currentActionFrameSpeed);
                    break;
                case KE_NPC_DOING.do_rushattack:
                    this.SpecialAttack(this.currentActionFrameSpeed);
                    break;
                case KE_NPC_DOING.do_magic:
                    this.PlayMagicAction(this.currentActionFrameSpeed);
                    break;
                case KE_NPC_DOING.do_hurt:
                    this.Hurt(this.currentActionFrameSpeed);
                    break;
                case KE_NPC_DOING.do_death:
                    this.Die();
                    break;
                case KE_NPC_DOING.do_manyattack:
                    this.AttackMultipleTimes(this.currentActionFrameSpeed, this.otherParam);
                    break;
                case KE_NPC_DOING.do_runattackmany:
                    this.SpecialAttackMultipleTimes(this.currentActionFrameSpeed, this.otherParam);
                    break;
                case KE_NPC_DOING.do_runattack:
                    this.RunAttack(this.currentActionFrameSpeed);
                    break;
            }

            if (this.isActiveMask && this.animation.IsPausing)
            {
                this.animation.Resume();
                /// Ngừng animation mặt nạ
                this.maskAnimation.Stop();
            }
            else if (!this.isActiveMask && this.maskAnimation.IsPausing)
            {
                this.maskAnimation.Resume();
                /// Ngừng animation người
                this.animation.Stop();
            }
        }

        /// <summary>
        /// Thiết lập hiển thị Model
        /// </summary>
        /// <param name="isVisible"></param>
        private void SetModelVisible(bool isVisible)
        {
            /// Người
            this.Body.gameObject.SetActive(this.isActiveMask ? false : isVisible);
            this.LeftArm.gameObject.SetActive(this.isActiveMask ? false : isVisible);
            this.LeftWeapon.gameObject.SetActive(this.isActiveMask ? false : isVisible);
            this.RightArm.gameObject.SetActive(this.isActiveMask ? false : isVisible);
            this.RightWeapon.gameObject.SetActive(this.isActiveMask ? false : isVisible);
            this.Head.gameObject.SetActive(this.isActiveMask ? false : isVisible);
            this.Hair.gameObject.SetActive(this.isActiveMask ? false : isVisible);
            this.Coat.gameObject.SetActive(this.isActiveMask ? false : isVisible);
            this.HorseHead.gameObject.SetActive(this.isActiveMask ? false : isVisible);
            this.HorseBody.gameObject.SetActive(this.isActiveMask ? false : isVisible);
            this.HorseTail.gameObject.SetActive(this.isActiveMask ? false : isVisible);

            /// Mặt nạ
            this.Mask.gameObject.SetActive(!this.isActiveMask ? false : isVisible);
        }

        /// <summary>
        /// Tạm dừng thực hiện tất cả động tác
        /// </summary>
        public void PauseAllActions()
        {
            this.animation.Pause();
            this.maskAnimation.Pause();
        }

        /// <summary>
        /// Tiếp tục thực hiện động tác
        /// </summary>
        public void ResumeActions()
        {
            /// Nếu thiết lập không hiện nhân vật
            if ((this.RefObject == Global.Data.Leader && KTSystemSetting.HideRole) || (this.RefObject != Global.Data.Leader && KTSystemSetting.HideOtherRole))
            {
                return;
            }

            this.animation.Resume();
        }

        /// <summary>
        /// Thay đổi màu của đối tượng
        /// </summary>
        /// <param name="color"></param>
        public void MixColor(Color color)
        {
            this.groupColor.Color = color;
        }

        /// <summary>
        /// Xóa đối tượng
        /// </summary>
        public void Destroy()
        {
            this.RemoveAllEffects();
            this.DestroyUI();

            this.MaxAlpha = 1f;
            this.groupColor.Alpha = 1f;
            this.StopAllCoroutines();
            this.RefObject = null;
            this.Data = null;
            this._ShowMPBar = false;
            this._ShowMinimapName = false;
            this._ShowMinimapIcon = false;
            this._MinimapNameColor = default;
            this._MinimapIconSize = default;
            this._Direction = default;
            this.currentActionFrameSpeed = 0f;
            this.otherParam = 0;
            this.UIMinimapReference = null;
            this.actionCoroutine = null;
            this.reloadAnimationCoroutine = null;
            /// Hủy đeo mặt nạ
            this.isActiveMask = false;
            this.SetModelVisible(true);
            this.lastHideRole = false;
            this.Destroyed?.Invoke();
            this.Destroyed = null;
            this.GetComponent<AudioSource>().clip = null;

            KTObjectPoolManager.Instance.ReturnToPool(this.gameObject);
        }
    }
}
