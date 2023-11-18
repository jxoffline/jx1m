using FS.VLTK.Entities.Config;
using FS.VLTK.Factory;
using FS.VLTK.Logic.Settings;
using FS.VLTK.Utilities.UnityComponent;
using UnityEngine;
using static FS.VLTK.Entities.Enum;

namespace FS.VLTK.Control.Component
{
    public partial class Monster
    {
        #region Define
        /// <summary>
        /// Cơ thể quái vật
        /// </summary>
        [SerializeField]
        private GameObject Body;

        /// <summary>
        /// Bóng quái vật
        /// </summary>
        [SerializeField]
        private GameObject Shadow;

        /// <summary>
        /// Luồng thực thi lệnh động tác ĐỨNG
        /// </summary>
        private Coroutine routineQueryStand = null;

        /// <summary>
        /// Đối tượng thực hiện động tác
        /// </summary>
        private new MonsterAnimation2D animation = null;

        #region Trail
        /// <summary>
        /// Hiệu ứng đổ bóng thân
        /// </summary>
        private SpriteTrailRenderer Trail_Body;
        #endregion
        #endregion

        /// <summary>
        /// Model của đối tượng
        /// </summary>
        public GameObject Model
        {
            get
            {
                return this.Body.transform.parent.gameObject;
            }
        }

        /// <summary>
        /// Luồng thực thi hiệu ứng Async
        /// </summary>
        private Coroutine actionCoroutine;

        /// <summary>
        /// Hàm này gọi đến ngay khi đối tượng được tạo ra
        /// </summary>
        private void InitAction()
        {

        }

        /// <summary>
        /// Cập nhật dữ liệu
        /// </summary>
        public void UpdateData()
        {
            this.animation.ResID = this.ResID;
            /// Dịch chuyển vị trí
            if (Loader.Loader.MonsterActionSetXML.Monsters.TryGetValue(this.ResID, out MonsterActionSetXML.Component resData))
            {
                this.Body.transform.localPosition = new Vector2(resData.PosX, resData.PosY);
                this.UIHeader.OffsetY = resData.Height;
            }
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

            this.Trail_Body.Enable = KTSystemSetting.DisableTrailEffect ? false : isActivated;
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
        /// Thiết lập hiển thị Model
        /// </summary>
        /// <param name="isVisible"></param>
        private void SetModelVisible(bool isVisible)
        {
            this.Body.gameObject.SetActive(isVisible);
        }

        /// <summary>
        /// Tiếp tục thực hiện động tác hiện tại
        /// </summary>
        public void ResumeCurrentAction()
        {
            /// Thực thi thiết lập
            this.ExecuteSetting();
            /// Nếu sau khi thực thi thiết lập trả ra kết quả ẩn đối tượng thì bỏ qua
            if (this.lastHideRole)
            {
                return;
            }

            switch (this.RefObject.CurrentAction)
            {
                case KE_NPC_DOING.do_none:
                case KE_NPC_DOING.do_stand:
                    this.Stand();
                    break;
                case KE_NPC_DOING.do_run:
                    this.Run();
                    break;
                case KE_NPC_DOING.do_attack:
                    this.Attack(this.currentActionFrameSpeed);
                    break;
                case KE_NPC_DOING.do_rushattack:
                    this.SpecialAttack(this.currentActionFrameSpeed);
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
                case KE_NPC_DOING.do_hurt:
                    this.Hurt(this.currentActionFrameSpeed);
                    break;
                case KE_NPC_DOING.do_death:
                    this.Die();
                    break;
            }

            if (this.animation.IsPausing)
            {
                this.animation.Resume();
            }
        }

        /// <summary>
        /// Tạm dừng thực hiện tất cả động tác
        /// </summary>
        public void PauseAllActions()
        {
            this.animation.Pause();
        }

        /// <summary>
        /// Tiếp tục thực hiện động tác
        /// </summary>
        public void ResumeActions()
        {
            /// Nếu thiết lập không hiện NPC
            if (KTSystemSetting.HideNPC)
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

            this.StopAllCoroutines();
            this.Body.transform.localPosition = Vector2.zero;
            this.RefObject = null;
            this.ResID = null;
            this.UIMinimapReference = null;
            this._ShowHPBar = false;
            this._ShowElemental = false;
            this._ShowMinimapName = false;
            this._ShowMinimapIcon = false;
            this._NameColor = default;
            this._MinimapNameColor = default;
            this._MinimapIconSize = default;
            this.waitingAddTaskState = GameEngine.Logic.NPCTaskStates.None;
            this.currentActionFrameSpeed = 0f;
            this.otherParam = 0;
            this._Direction = default;
            this.routineQueryStand = null;
            this.actionCoroutine = null;
            this.SetModelVisible(true);
            this.lastHideRole = false;
            this.Destroyed?.Invoke();
            this.Destroyed = null;
            this.GetComponent<AudioSource>().clip = null;

            KTObjectPoolManager.Instance.ReturnToPool(this.gameObject);
        }
    }
}
