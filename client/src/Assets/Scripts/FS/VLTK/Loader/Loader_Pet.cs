using FS.VLTK.Entities.Config;
using System.Collections.Generic;
using System.Xml.Linq;

namespace FS.VLTK.Loader
{
    /// <summary>
    /// Đối tượng chứa danh sách các cấu hình trong game
    /// </summary>
    public static partial class Loader
    {
        #region Pet Config
        /// <summary>
        /// Cấu hình pet
        /// </summary>
        public static PetConfigXML PetConfig { get; private set; }

        /// <summary>
        /// Tải cấu hình pet từ XMLNode
        /// </summary>
        /// <param name="xmlNode"></param>
        public static void LoadPetConfig(XElement xmlNode)
        {
            Loader.PetConfig = PetConfigXML.Parse(xmlNode);
        }
        #endregion

        #region List Pet
        /// <summary>
        /// Danh sách pet
        /// </summary>
        public static Dictionary<int, PetDataXML> ListPets { get; private set; } = new Dictionary<int, PetDataXML>();

        /// <summary>
        /// Tải danh sách pet từ XML Node
        /// </summary>
        /// <param name="xmlNode"></param>
        public static void LoadPets(XElement xmlNode)
        {
            foreach (XElement node in xmlNode.Elements("Pet"))
            {
                /// Chuyển đổi thành đối tượng
                PetDataXML petData = PetDataXML.Parse(node);
                /// Thêm vào danh sách
                Loader.ListPets[petData.ID] = petData;
            }
        }
        #endregion
    }
}
