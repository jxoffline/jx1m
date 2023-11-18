using GameServer.KiemThe;
using GameServer.Logic;
using System.Collections.Generic;
using System.Xml.Linq;

namespace GameServer.VLTK.Entities.Pet
{
    /// <summary>
    /// Quản lý cấu hình Pet
    /// </summary>
    public static class KPet
    {
        /// <summary>
        /// Thiết lập
        /// </summary>
        public static PetConfigXML Config { get; private set; }
        
        /// <summary>
        /// Danh sách Pet
        /// </summary>
        private static readonly Dictionary<int, PetDataXML> pets = new Dictionary<int, PetDataXML>();

        /// <summary>
        /// Tải danh sách Pet
        /// </summary>
        public static void LoadPets()
        {
            KPet.pets.Clear();

            XElement petConfigNode = KTGlobal.ReadXMLData("Config/KT_Pet/PetConfig.xml");
            KPet.Config = PetConfigXML.Parse(petConfigNode);

            XElement petNode = KTGlobal.ReadXMLData("Config/KT_Pet/Pet.xml");
            foreach (XElement node in petNode.Elements("Pet"))
            {
                PetDataXML petData = PetDataXML.Parse(node);
                KPet.pets[petData.ID] = petData;
            }
        }

        /// <summary>
        /// Trả về thông tin Pet theo ID tương ứng
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static PetDataXML GetPetData(int id)
        {
            /// Nếu tìm thấy
            if (KPet.pets.TryGetValue(id, out PetDataXML petData))
            {
                /// Trả về kết quả
                return petData;
            }
            /// Toác
            return null;
        }

        /// <summary>
        /// Trả về tổng số điểm tiềm năng đến cấp hiện tại
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        public static int GetTotalRemainPoints(int level)
        {
            return KPet.Config.LevelUpRemainPoints * level;
        }

        /// <summary>
        /// Trả về giá trị vật công ngoại có được theo tổng số sức mạnh của pet có ID tương ứng
        /// </summary>
        /// <param name="id"></param>
        /// <param name="strength"></param>
        /// <returns></returns>
        public static int GetStrength2DamagePhysics(int id, int strength)
        {
            /// Thông tin pet
            PetDataXML petData = KPet.GetPetData(id);
            /// Toác
            if (petData == null)
            {
                return 0;
            }

            /// Trả về kết quả
            return strength * petData.StrToPAtk;
        }

        /// <summary>
        /// Trả về giá trị chính xác có được theo tổng số thân pháp của pet có ID tương ứng
        /// </summary>
        /// <param name="id"></param>
        /// <param name="dexterity"></param>
        /// <returns></returns>
        public static int GetDexterity2AttackRate(int id, int dexterity)
        {
            /// Thông tin pet
            PetDataXML petData = KPet.GetPetData(id);
            /// Toác
            if (petData == null)
            {
                return 0;
            }

            /// Trả về kết quả
            return dexterity * petData.DexToHit;
        }

        /// <summary>
        /// Trả về giá trị né tránh có được theo tổng số thân pháp của pet có ID tương ứng
        /// </summary>
        /// <param name="id"></param>
        /// <param name="dexterity"></param>
        /// <returns></returns>
        public static int GetDexterity2Defence(int id, int dexterity)
        {
            /// Thông tin pet
            PetDataXML petData = KPet.GetPetData(id);
            /// Toác
            if (petData == null)
            {
                return 0;
            }

            /// Trả về kết quả
            return dexterity * petData.DexToDodge;
        }

        /// <summary>
        /// Trả về giá trị sinh lực có được theo tổng số ngoại công của pet có ID tương ứng
        /// </summary>
        /// <param name="id"></param>
        /// <param name="dexterity"></param>
        /// <returns></returns>
        public static int GetVitality2Life(int id, int vitality)
        {
            /// Thông tin pet
            PetDataXML petData = KPet.GetPetData(id);
            /// Toác
            if (petData == null)
            {
                return 0;
            }

            /// Trả về kết quả
            return vitality * petData.StaToHP;
        }

        /// <summary>
        /// Trả về giá trị vật công nội có được theo tổng số nội công của pet có ID tương ứng
        /// </summary>
        /// <param name="id"></param>
        /// <param name="energy"></param>
        /// <returns></returns>
        public static int GetEnergy2DamageMagic(int id, int energy)
        {
            /// Thông tin pet
            PetDataXML petData = KPet.GetPetData(id);
            /// Toác
            if (petData == null)
            {
                return 0;
            }

            /// Trả về kết quả
            return energy * petData.IntToMAtk;
        }
    }
}
