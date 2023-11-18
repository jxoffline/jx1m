using FS.Drawing;
using FS.GameEngine.Logic;
using FS.GameEngine.Network;
using FS.GameEngine.Sprite;
using FS.GameFramework.Logic;
using FS.VLTK;
using FS.VLTK.Control.Component;
using FS.VLTK.Factory;
using FS.VLTK.Logic;
using Server.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static FS.VLTK.Entities.Enum;

namespace FS.GameEngine.Scene
{
    /// <summary>
    /// Quản lý pet
    /// </summary>
    public partial class GScene
    {
        #region Hệ thống pet
        /// <summary>
        /// Danh sách pet
        /// </summary>
        private List<PetDataMini> waitToBeAddedPets = new List<PetDataMini>();

        /// <summary>
        /// Tải pet vật
        /// </summary>
        /// <param name="petData"></param>
        public void ToLoadPet(PetDataMini petData)
        {
            this.waitToBeAddedPets.Add(petData);
        }

        /// <summary>
        /// Thêm pet vào bản đồ
        /// </summary>
        private void AddListPet()
        {
            if (this.waitToBeAddedPets.Count <= 0)
            {
                return;
            }

            PetDataMini petData = this.waitToBeAddedPets[0];
            this.waitToBeAddedPets.RemoveAt(0);
            this.AddListPet(petData);
        }

        /// <summary>
        /// Tải danh sách pet
        /// </summary>
        /// <param name="petData"></param>
        private void AddListPet(PetDataMini petData)
        {
            /// Toác
            if (petData == null)
            {
                return;
            }

            /// Nếu đối tượng không tồn tại thì bỏ qua
            if (!Global.Data.SystemPets.TryGetValue(petData.ID, out _))
            {
                return;
            }

            /// Tên đối tượng
            string name = string.Format("Pet_{0}", petData.ID);

            /// Đối tượng cũ
            GSprite sprite = this.FindSprite(name);
            /// Nếu đối tượng có tồn tại
            if (sprite != null)
            {
                /// Xóa đối tượng
                KTGlobal.RemoveObject(sprite, true);
            }

            /// Tải xuống đối tượng
            sprite = this.LoadPet(petData);
            /// Thực hiện gửi gói tin tải xuống hoàn tất
            GameInstance.Game.SpriteLoadAlready(sprite.RoleID);
        }

        /// <summary>
        /// Tải xuống pet
        /// </summary>
        public GSprite LoadPet(PetDataMini petData)
        {
            string name = string.Format("Pet_{0}", petData.ID);

            GSprite petObj = new GSprite();
            petObj.BaseID = petData.ID;
            petObj.SpriteType = GSpriteTypes.Pet;

            this.LoadSprite(
                petObj,
                petData.ID,
                name,
                null,
                null,
                null,
                null,
                null,
                petData,
                null,
                null,
                (Direction) petData.Direction,
                petData.PosX,
                petData.PosY
            );

            /// Bắt đầu
            petObj.Start();

            /// Tìm đối tượng cũ
            GameObject oldObject = KTObjectPoolManager.Instance.FindSpawn(x => x.name == name);
            /// Nếu tồn tại
            if (oldObject != null)
            {
                /// Trả lại Pool
                KTObjectPoolManager.Instance.ReturnToPool(oldObject);
            }

            Monster pet = KTObjectPoolManager.Instance.Instantiate<Monster>("Pet");
            pet.name = name;

            /// Gắn đối tượng tham chiếu
            pet.RefObject = petObj;

            pet.ShowMinimapIcon = false;
            pet.ShowMinimapName = false;

            ColorUtility.TryParseHtmlString("#8BD4FF", out Color nameColor);
            pet.NameColor = nameColor;
            pet.ShowHPBar = true;
            pet.ShowElemental = false;

            /// Res
            pet.StaticID = petData.ResID;
            pet.ResID = FS.VLTK.Loader.Loader.ListPets[petData.ResID].ResID;
            pet.Direction = (Direction) petData.Direction;
            pet.UpdateData();


            GameObject role2D = pet.gameObject;
            petObj.Role2D = role2D;
            role2D.transform.localPosition = new Vector2(petData.PosX, petData.PosY);

            /// Cập nhật hiển thị thanh máu
            this.RefreshSpriteLife(petObj);

            /// Thực hiện động tác đứng
            petObj.DoStand();
            pet.ResumeCurrentAction();

            /// Nếu là Pet của bản thân
            if (Global.Data.RoleData.CurrentPetID + (int) ObjectBaseID.Pet == petObj.PetData.ID)
            {
                /// Đánh dấu pet bản thân
                KTAutoPetManager.Instance.Pet = petObj;
            }

            return petObj;
        }

        /// <summary>
        /// Xóa pet tương ứng khỏi hệ thống
        /// </summary>
        /// <param name="petID"></param>
        /// <returns></returns>
        public bool DelPet(int petID)
        {
            int roleID = petID;
            GSprite petObj = this.FindSprite(roleID);
            /// Xóa toàn bộ pet khác đang đợi load tương ứng
            this.waitToBeAddedPets.RemoveAll(x => x.ID == petID);

            /// Nếu tìm thấy
            if (petObj != null)
            {
                /// Nếu đang thực hiện động tác chết
                if (petObj.IsDeath || petObj.HP <= 0)
                {
                    /// Bỏ qua
                    return false;
                }

                /// Nếu là Pet của bản thân
                if (Global.Data.RoleData.CurrentPetID + (int) ObjectBaseID.Pet == petObj.RoleID)
                {
                    /// Xóa pet bản thân
                    KTAutoPetManager.Instance.Pet = null;
                }

                /// Xóa pet
                KTGlobal.RemoveObject(petObj, true);
            }

            return true;
        }
        #endregion
    }
}
