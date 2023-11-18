using GameServer.KiemThe.Entities;
using GameServer.KiemThe.Logic;
using GameServer.Server;
using GameServer.VLTK.Entities.Pet;
using Server.Data;
using Server.Protocol;
using Server.TCP;
using Server.Tools;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static GameServer.Logic.KTMapManager;

namespace GameServer.Logic
{
    /// <summary>
    /// Quản lý Pet
    /// </summary>
    public static partial class KTPetManager
    {
        #region Quản lý Pet
        /// <summary>
        /// Tính toán tất cả các chỉ số thuộc tính của pet tương ứng
        /// </summary>
        /// <param name="petData"></param>
        /// <param name="player"></param>
        /// <returns></returns>
        public static PetData GetPetDataWithAttributes(PetData petData, KPlayer player = null)
        {
            /// Thông tin Pet
            PetDataXML _data = KPet.GetPetData(petData.ResID);
            /// Toác
            if (_data == null)
            {
                return null;
            }
            /// Tạo bản sao
            petData = KTPetManager.ClonePetData(petData);

            /// Nếu pet này đang tham chiến
            if (player != null && player.CurrentPet != null && player.CurrentPet.RoleID - (int) ObjectBaseID.Pet == petData.ID)
            {

                /// Lấy thông tin mới nhất
                petData = player.CurrentPet.GetDBData();

                /// Tiềm năng
                petData.RemainPoints = KPet.GetTotalRemainPoints(petData.Level) + petData.RemainPoints - petData.Str - petData.Dex - petData.Sta - petData.Int;
                /// Sức, thân, ngoại, nội
                petData.Str = _data.Str + _data.StrPerLevel * petData.Level + petData.Str;
                petData.Dex = _data.Dex + _data.DexPerLevel * petData.Level + petData.Dex;
                petData.Sta = _data.Sta + _data.StaPerLevel * petData.Level + petData.Sta;
                petData.Int = _data.Int + _data.IntPerLevel * petData.Level + petData.Int;
            }
            /// Nếu pet này đang nghỉ ngơi
            else
            {
                /// Tính toán các chỉ số
                petData = KTPetManager.CalculatePetAttributes(petData);
            }
            /// Trả về kết quả
            return petData;
        }

        /// <summary>
        /// Tính toán các chỉ số thuộc tính pet dựa vào các điểm thuộc tính trong dữ liệu
        /// </summary>
        /// <param name="petData"></param>
        /// <returns></returns>
        public static PetData CalculatePetAttributes(PetData petData)
        {
            /// Thông tin pet
            PetDataXML _data = KPet.GetPetData(petData.ResID);
            /// Toác
            if (_data == null)
            {
                return null;
            }

            /// Tạo bản sao
            petData = KTPetManager.ClonePetData(petData);
            /// Tiềm năng
            petData.RemainPoints = KPet.GetTotalRemainPoints(petData.Level) + petData.RemainPoints - petData.Str - petData.Dex - petData.Sta - petData.Int;
            /// Sức, thân, ngoại, nội
            petData.Str = _data.Str + _data.StrPerLevel * petData.Level + petData.Str;
            petData.Dex = _data.Dex + _data.DexPerLevel * petData.Level + petData.Dex;
            petData.Sta = _data.Sta + _data.StaPerLevel * petData.Level + petData.Sta;
            petData.Int = _data.Int + _data.IntPerLevel * petData.Level + petData.Int;

            /// Vật công - ngoại
            petData.PAtk = KPet.GetStrength2DamagePhysics(petData.ResID, petData.Str);
            /// Chính xác, né tránh
            petData.Hit = KPet.GetDexterity2AttackRate(petData.ResID, petData.Dex);
            petData.Dodge = KPet.GetDexterity2Defence(petData.ResID, petData.Dex);
            /// Sinh lực
            petData.MaxHP = KPet.GetVitality2Life(petData.ResID, petData.Sta) + _data.HP;
            /// Vật công - nội
            petData.MAtk = KPet.GetEnergy2DamageMagic(petData.ResID, petData.Int);

            /// Trả về kết quả
            return petData;
        }

        /// <summary>
        /// Tạo bản sao dữ liệu Pet
        /// </summary>
        /// <param name="petData"></param>
        /// <returns></returns>
        public static PetData ClonePetData(PetData petData)
        {
            return new PetData()
            {
                ID = petData.ID,
                ResID = petData.ResID,
                RoleID = petData.RoleID,
                AtkSpeed = petData.AtkSpeed,
                CastSpeed = petData.CastSpeed,
                Crit = petData.Crit,
                Dex = petData.Dex,
                Dodge = petData.Dodge,
                Enlightenment = petData.Enlightenment,
                Equips = petData.Equips?.ToDictionary(tKey => tKey.Key, tValue => tValue.Value),
                Exp = petData.Exp,
                FireRes = petData.FireRes,
                Hit = petData.Hit,
                HP = petData.HP,
                IceRes = petData.IceRes,
                Int = petData.Int,
                Joyful = petData.Joyful,
                Level = petData.Level,
                MaxHP = petData.MaxHP,
                MAtk = petData.MAtk,
                LightningRes = petData.LightningRes,
                Life = petData.Life,
                MoveSpeed = petData.MoveSpeed,
                Name = petData.Name,
                PAtk = petData.PAtk,
                PDef = petData.PDef,
                PoisonRes = petData.PoisonRes,
                RemainPoints = petData.RemainPoints,
                Skills = petData.Skills?.ToDictionary(tKey => tKey.Key, tValue => tValue.Value),
                Sta = petData.Sta,
                Str = petData.Str,
            };
        }

        /// <summary>
        /// Tạo mới Pet
        /// </summary>
        /// <param name="petData"></param>
        /// <returns></returns>
        public static Pet CreatePet(KPlayer owner, PetData petData)
        {
            /// Tạo mới
            Pet pet = new Pet(owner, petData);

            /// Bắt đầu luồng Pet
            KTPetTimerManager.Instance.Add(pet);
            /// Thêm vào danh sách
            KTPetManager.pets[pet.RoleID] = pet;
            /// Trả về kết quả
            return pet;
        }

        /// <summary>
        /// Xóa pet tương ứng
        /// </summary>
        /// <param name="pet"></param>
        public static void RemovePet(Pet pet)
        {
            /// Toác
            if (pet == null)
            {
                return;
            }

            /// Ngừng luồng Pet
            KTPetTimerManager.Instance.Remove(pet);

            /// Chuyển động tác sang chết
            pet.m_eDoing = KiemThe.Entities.KE_NPC_DOING.do_death;
            /// Xóa
            pet.Destroy();

            /// Xóa khỏi danh sách
            KTPetManager.pets.TryRemove(pet.RoleID, out _);

            /// Data
            PetData petData = pet.GetDBData();
            /// Lưu lại thông tin của Pet
            KT_TCPHandler.SendDBUpdatePetDataBeforeCallBack(pet.Owner, petData);
        }
        #endregion

        #region Hiển thị
        /// <summary>
        /// Xử lý khi đối tượng pet được tải xuống thành công
        /// </summary>
        /// <param name="client"></param>
        /// <param name="pet"></param>
        public static void HandlePetLoaded(KPlayer client, Pet pet)
        {
            LoadAlreadyData data = new LoadAlreadyData()
            {
                RoleID = pet.RoleID,
                PosX = (int) pet.CurrentPos.X,
                PosY = (int) pet.CurrentPos.Y,
                Direction = (int) pet.CurrentDir,
                Action = (int) pet.m_eDoing,
                PathString = "",
                ToX = (int) pet.CurrentPos.X,
                ToY = (int) pet.CurrentPos.Y,
                Camp = pet.Camp,
            };
            client.SendPacket<LoadAlreadyData>((int) TCPGameServerCmds.CMD_SPR_LOADALREADY, data);
        }
        #endregion
    }
}
