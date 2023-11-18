using FS.GameEngine.Logic;
using Server.Data;
using System.Collections;
using TMPro;
using UnityEngine;
using static FS.VLTK.Entities.Enum;

namespace FS.VLTK.UI.Main.MainUI
{
    /// <summary>
    /// Khung thông tin pet của bản thân
    /// </summary>
    public class UIPetPart : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Text cấp độ pet
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_PetLevel;

        /// <summary>
        /// Text tên pet
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_PetName;

        /// <summary>
        /// Slider thanh máu pet
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Slider UISlider_PetHP;

        /// <summary>
        /// Text lượng máu pet
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_PetHP;
        #endregion

        #region Private fields
        /// <summary>
        /// RectTransform
        /// </summary>
        private RectTransform rectTransform;
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi khi đối tượng được tạo ra
        /// </summary>
        private void Awake()
        {
            this.rectTransform = this.transform.GetChild(0).GetComponent<RectTransform>();
        }

        /// <summary>
        /// Hàm này gọi ở Frame đầu tiên
        /// </summary>
        private void Start()
        {
            /// Thực thi luồng kiểm tra liên tục
            this.StartCoroutine(this.ExecuteAlways());
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Thực thi kiểm tra liên tục
        /// </summary>
        /// <returns></returns>
        private IEnumerator ExecuteAlways()
        {
            /// Đợi
            WaitForSeconds wait = new WaitForSeconds(0.2f);
            /// Lặp liên tục
            while (true)
            {
                /// Nếu không có pet
                if (Global.Data.RoleData.CurrentPetID == -1 || !Global.Data.SystemPets.ContainsKey(Global.Data.RoleData.CurrentPetID + (int) ObjectBaseID.Pet))
                {
                    /// Ẩn khung
                    this.rectTransform.gameObject.SetActive(false);
                }
                /// Nếu có pet
                else
                {
                    /// Thông tin pet
                    PetDataMini petData = Global.Data.SystemPets[Global.Data.RoleData.CurrentPetID + (int) ObjectBaseID.Pet];
                    /// Hiện khung
                    this.rectTransform.gameObject.SetActive(true);
                    /// Cấp
                    this.UIText_PetLevel.text = petData.Level.ToString();
                    /// Tên
                    this.UIText_PetName.text = petData.Name;
                    /// Máu
                    this.UISlider_PetHP.value = petData.HP / (float) petData.MaxHP;
                    this.UIText_PetHP.text = string.Format("{0}/{1}", petData.HP, petData.MaxHP);
                }
                /// Đợi
                yield return wait;
            }
            
        }
        #endregion
    }
}
