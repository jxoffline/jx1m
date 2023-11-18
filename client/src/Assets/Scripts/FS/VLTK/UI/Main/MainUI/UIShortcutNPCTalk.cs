using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using FS.GameEngine.Sprite;
using FS.GameEngine.Logic;
using System.Collections;
using FS.VLTK.Factory;

namespace FS.VLTK.UI.Main.MainUI
{
    /// <summary>
    /// Khung đối thoại nhanh với NPC
    /// </summary>
    public class UIShortcutNPCTalk : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Button tương tác
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton;

        /// <summary>
        /// Text tên NPC
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_NPCName;
        #endregion

        #region Constants
        /// <summary>
        /// Khoảng cách quét mục tiêu
        /// </summary>
        private const int ScanRange = 100;
        #endregion

        #region Private fields
        /// <summary>
        /// RectTransform đối tượng tương ứng
        /// </summary>
        private RectTransform rectTransform;
        #endregion

        #region Properties
        private GSprite _TargetNPC = null;
        /// <summary>
        /// NPC đang tương tác
        /// </summary>
        private GSprite TargetNPC
        {
            get
            {
                return this._TargetNPC;
            }
            set
            {
                this._TargetNPC = value;

                /// Kích hoạt khung nếu có NPC
                this.rectTransform.gameObject.SetActive(value != null);
                /// Nếu có NPC
                if (value != null)
                {
                    /// Cập nhật tên cho NPC
                    this.UIText_NPCName.text = value.RoleName;
                }
            }
        }
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi khi đối tượng được tạo ra
        /// </summary>
        private void Awake()
        {
            this.rectTransform = this.UIText_NPCName.transform.parent.GetComponent<RectTransform>();
        }

        /// <summary>
        /// Hàm này gọi ở Frame đầu tiên
        /// </summary>
        private void Start()
        {
            this.InitPrefabs();
            this.StartCoroutine(this.DoScanNPCAround());
        }
        #endregion

        #region Code UI
        /// <summary>
        /// Khởi tạo ban đầu
        /// </summary>
        private void InitPrefabs()
        {
            this.UIButton.onClick.AddListener(this.ButtonAction_Clicked);
        }

        /// <summary>
        /// Sự kiện khi Button tương tác được ấn
        /// </summary>
        private void ButtonAction_Clicked()
        {
            /// Nếu không có NPC
            if (this.TargetNPC == null)
            {
                return;
            }

            /// Thiết lập ID NPC mục tiêu
            Global.Data.TargetNpcID = this.TargetNPC.RoleID;
            /// Thực thi Click vào NPC
            Global.Data.GameScene.NPCClick(this.TargetNPC);
        }
        #endregion

        #region Private fields
        /// <summary>
        /// Luồng thực thi quét NPC xung quanh
        /// </summary>
        /// <returns></returns>
        private IEnumerator DoScanNPCAround()
        {
            while (true)
            {
                /// Nếu không có Leader
                if (Global.Data.Leader == null)
                {
                    /// Bỏ qua Frame
                    yield return null;
                    /// Tiếp tục vòng lặp
                    continue;
                }
                /// Vị trí của Leader
                Vector2 leaderPos = Global.Data.Leader.PositionInVector2;
                /// Tìm NPC tương ứng
                GSprite npc = KTObjectsManager.Instance.FindObjects<GSprite>(x => x.SpriteType == GSpriteTypes.NPC && Vector2.Distance(leaderPos, x.PositionInVector2) <= UIShortcutNPCTalk.ScanRange).MinBy((npc) => {
                    /// Vị trí của NPC
                    Vector2 npcPos = npc.PositionInVector2;
                    /// Khoảng cách tới NPC
                    float distance = Vector2.Distance(leaderPos, npcPos);

                    /// Trả về giá trị so sánh là khoảng cách
                    return distance;
                });
                /// Gắn đối tượng vừa tìm được vào NPC để tương tác
                this.TargetNPC = npc;

                /// Đợi 1s
                yield return new WaitForSeconds(1f);
            }
        }
        #endregion

        #region Public methods

        #endregion
    }
}
