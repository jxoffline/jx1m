using FS.GameEngine.Logic;
using FS.VLTK.Entities.Config;
using FS.VLTK.Factory;
using FS.VLTK.Loader;
using FS.VLTK.Utilities.UnityComponent;
using System;
using System.Collections;
using UnityEngine;
using static FS.VLTK.Entities.Enum;

namespace FS.VLTK.Control.Component
{
    /// <summary>
    /// Quản lý động tác
    /// </summary>
    public partial class CharacterPreview
    {
        #region Define
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

        /// <summary>
        /// Luồng thực thi hiệu ứng Async
        /// </summary>
        private Coroutine actionCoroutine;

        /// <summary>
        /// Đối tượng thực hiện động tác nhân vật
        /// </summary>
        private new CharacterAnimation2D animation = null;
        #endregion

        #region Loader
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
        #endregion


        /// <summary>
        /// Khởi tạo động tác
        /// </summary>
        private void InitAction()
        {
            this.animation = this.gameObject.GetComponent<CharacterAnimation2D>();
            this.animation.Head = this.Head.GetComponent<SpriteRenderer>();
            this.animation.Hair = this.Hair.GetComponent<SpriteRenderer>();
            this.animation.Armor = this.Body.GetComponent<SpriteRenderer>();
            this.animation.LeftHand = this.LeftArm.GetComponent<SpriteRenderer>();
            this.animation.RightHand = this.RightArm.GetComponent<SpriteRenderer>();
            this.animation.LeftWeapon = this.LeftWeapon.GetComponent<SpriteRenderer>();
            this.animation.RightWeapon = this.RightWeapon.GetComponent<SpriteRenderer>();
            this.animation.Mantle = this.Coat.GetComponent<SpriteRenderer>();
            this.animation.LeftWeaponEnhanceEffects = this.LeftWeaponEnhanceEffects.GetComponent<WeaponEnhanceEffect_Particle>();
            this.animation.RightWeaponEnhanceEffects = this.RightWeaponEnhanceEffects.GetComponent<WeaponEnhanceEffect_Particle>();
        }

        /// <summary>
        /// Thực thi bỏ qua một số Frame
        /// </summary>
        /// <param name="skip"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        private IEnumerator ExecuteSkipFrame(int skip, Action callback)
        {
            for (int i = 1; i <= skip; i++)
            {
                yield return null;
            }
            callback?.Invoke();
        }

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
            this.reloadAnimationCoroutine = this.StartCoroutine(this.ExecuteSkipFrame(1, () => {
                this.animation.Reload();
                this.reloadAnimationCoroutine = null;
            }));
        }

        /// <summary>
        /// Khởi tạo Camera
        /// </summary>
        private void InitCamera()
        {
            this.ReferenceCamera = GameObject.Instantiate<Camera>(Global.ObjectPreviewCameraPrefab);
            this.ReferenceCamera.transform.SetParent(Global.ObjectPreviewCameraPrefab.transform.parent, false);
            this.ReferenceCamera.targetTexture = new RenderTexture(512, 512, 16);
            this.ReferenceCamera.gameObject.SetActive(true);
            this.ReferenceCamera.transform.localPosition = new Vector3(this.gameObject.transform.localPosition.x, this.gameObject.transform.localPosition.y, this.ReferenceCamera.transform.localPosition.z);
        }

        /// <summary>
        /// Xóa đối tượng Camera
        /// </summary>
        private void DestroyCamera()
        {
            if (this.ReferenceCamera != null)
            {
                GameObject.Destroy(this.ReferenceCamera.gameObject);
            }
        }

        /// <summary>
        /// Tiếp tục thực hiện động tác hiện tại
        /// </summary>
        public void ResumeCurrentAction()
        {
            this.animation.Data = this.Data;
            this.InstantStand();
        }

        /// <summary>
        /// Cập nhật RoleData
        /// </summary>
        public void UpdateRoleData()
        {
            this.animation.Data = this.Data;
        }

        /// <summary>
        /// Đứng ngay lập tức
        /// </summary>
        private void InstantStand()
        {
            if (this.gameObject == null || !this.gameObject.activeSelf)
            {
                return;
            }

            if (this.actionCoroutine != null)
            {
                this.StopCoroutine(this.actionCoroutine);
            }
            this.actionCoroutine = this.StartCoroutine(this.animation.DoActionAsync(false, PlayerActionType.NormalStand, this.Direction, 0.5f, int.MaxValue, 0f));
        }
    }
}
