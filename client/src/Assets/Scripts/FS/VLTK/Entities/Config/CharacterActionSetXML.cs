using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace FS.VLTK.Entities.Config
{
    /// <summary>
    /// Đối tượng quản lý ActionSet
    /// </summary>
    public class CharacterActionSetXML
    {
        /// <summary>
        /// Thành phần
        /// </summary>
        public class Component
        {
            /// <summary>
            /// Mã của thành phần
            /// </summary>
            public int Code { get; set; }

            /// <summary>
            /// ID thành phần
            /// </summary>
            public string ID { get; set; }

            /// <summary>
            /// Đường dẫn file Bundle chứa thành phần
            /// </summary>
            public string BundleDir { get; set; }
        }

        #region Male
        /// <summary>
        /// Quần áo nhân vật nam
        /// </summary>
        public Dictionary<string, Component> MaleArmor { get; set; } = new Dictionary<string, Component>();
        /// <summary>
        /// Quần áo nhân vật nam theo mã thành phần
        /// </summary>
        public Dictionary<int, Component> MaleArmorByCode { get; set; } = new Dictionary<int, Component>();

        /// <summary>
        /// Đầu nhân vật nam
        /// </summary>
        public Dictionary<string, Component> MaleHead { get; set; } = new Dictionary<string, Component>();
        /// <summary>
        /// Đầu nhân nhân vật nam theo mã thành phần
        /// </summary>
        public Dictionary<int, Component> MaleHeadByCode { get; set; } = new Dictionary<int, Component>();

        /// <summary>
        /// Phi phong nhân vật nam
        /// </summary>
        public Dictionary<string, Component> MaleMantle { get; set; } = new Dictionary<string, Component>();
        /// <summary>
        /// Phi phong nhân vật nam theo mã thành phần
        /// </summary>
        public Dictionary<int, Component> MaleMantleByCode { get; set; } = new Dictionary<int, Component>();

        /// <summary>
        /// Vũ khí nhân vật nam
        /// </summary>
        public Dictionary<string, Component> MaleWeapon { get; set; } = new Dictionary<string, Component>();
        /// <summary>
        /// Vũ khí nhân vật nam theo mã thành phần
        /// </summary>
        public Dictionary<int, Component> MaleWeaponByCode { get; set; } = new Dictionary<int, Component>();
        #endregion

        #region Female
        /// <summary>
        /// Quần áo nhân vật nữ
        /// </summary>
        public Dictionary<string, Component> FemaleArmor { get; set; } = new Dictionary<string, Component>();
        /// <summary>
        /// Quần áo nhân vật nữ theo mã thành phần
        /// </summary>
        public Dictionary<int, Component> FemaleArmorByCode { get; set; } = new Dictionary<int, Component>();

        /// <summary>
        /// Đầu nhân vật nữ
        /// </summary>
        public Dictionary<string, Component> FemaleHead { get; set; } = new Dictionary<string, Component>();
        /// <summary>
        /// Đầu nhân nhân vật nữ theo mã thành phần
        /// </summary>
        public Dictionary<int, Component> FemaleHeadByCode { get; set; } = new Dictionary<int, Component>();

        /// <summary>
        /// Phi phong nhân vật nữ
        /// </summary>
        public Dictionary<string, Component> FemaleMantle { get; set; } = new Dictionary<string, Component>();
        /// <summary>
        /// Phi phong nhân vật nữ theo mã thành phần
        /// </summary>
        public Dictionary<int, Component> FemaleMantleByCode { get; set; } = new Dictionary<int, Component>();

        /// <summary>
        /// Vũ khí nhân vật nữ
        /// </summary>
        public Dictionary<string, Component> FemaleWeapon { get; set; } = new Dictionary<string, Component>();
        /// <summary>
        /// Vũ khí nhân vật nữ theo mã thành phần
        /// </summary>
        public Dictionary<int, Component> FemaleWeaponByCode { get; set; } = new Dictionary<int, Component>();
        #endregion

        #region Rider
        /// <summary>
        /// Ngựa
        /// </summary>
        public Dictionary<string, Component> Rider { get; set; } = new Dictionary<string, Component>();
        /// <summary>
        /// Ngựa theo mã thành phần
        /// </summary>
        public Dictionary<int, Component> RiderByCode { get; set; } = new Dictionary<int, Component>();
        #endregion

        /// <summary>
        /// Chuyển đối tượng từ XML Node
        /// </summary>
        /// <param name="xmlNode"></param>
        /// <returns></returns>
        public static CharacterActionSetXML Parse(XElement xmlNode)
        {
            CharacterActionSetXML actionSet = new CharacterActionSetXML();

            #region Male
            foreach (XElement data in xmlNode.Element("Male").Element("Armor").Elements("Set"))
            {
                int code = int.Parse(data.Attribute("Code").Value);
                string id = data.Attribute("ID").Value;
                string bundleDir = data.Attribute("BundleDir").Value;
                actionSet.MaleArmor[id] = new Component()
                {
                    Code = code,
                    ID = id,
                    BundleDir = bundleDir,
                };
                actionSet.MaleArmorByCode[code] = actionSet.MaleArmor[id];
            }
            foreach (XElement data in xmlNode.Element("Male").Element("Head").Elements("Set"))
            {
                int code = int.Parse(data.Attribute("Code").Value);
                string id = data.Attribute("ID").Value;
                string bundleDir = data.Attribute("BundleDir").Value;
                actionSet.MaleHead[id] = new Component()
                {
                    Code = code,
                    ID = id,
                    BundleDir = bundleDir,
                };
                actionSet.MaleHeadByCode[code] = actionSet.MaleHead[id];
            }
            foreach (XElement data in xmlNode.Element("Male").Element("Mantle").Elements("Set"))
            {
                int code = int.Parse(data.Attribute("Code").Value);
                string id = data.Attribute("ID").Value;
                string bundleDir = data.Attribute("BundleDir").Value;
                actionSet.MaleMantle[id] = new Component()
                {
                    Code = code,
                    ID = id,
                    BundleDir = bundleDir,
                };
                actionSet.MaleMantleByCode[code] = actionSet.MaleMantle[id];
            }
            foreach (XElement data in xmlNode.Element("Male").Element("Weapon").Elements("Set"))
            {
                int code = int.Parse(data.Attribute("Code").Value);
                string id = data.Attribute("ID").Value;
                string bundleDir = data.Attribute("BundleDir").Value;
                actionSet.MaleWeapon[id] = new Component()
                {
                    Code = code,
                    ID = id,
                    BundleDir = bundleDir,
                };
                actionSet.MaleWeaponByCode[code] = actionSet.MaleWeapon[id];
            }
            #endregion

            #region Male
            foreach (XElement data in xmlNode.Element("Female").Element("Armor").Elements("Set"))
            {
                int code = int.Parse(data.Attribute("Code").Value);
                string id = data.Attribute("ID").Value;
                string bundleDir = data.Attribute("BundleDir").Value;
                actionSet.FemaleArmor[id] = new Component()
                {
                    Code = code,
                    ID = id,
                    BundleDir = bundleDir,
                };
                actionSet.FemaleArmorByCode[code] = actionSet.FemaleArmor[id];
            }
            foreach (XElement data in xmlNode.Element("Female").Element("Head").Elements("Set"))
            {
                int code = int.Parse(data.Attribute("Code").Value);
                string id = data.Attribute("ID").Value;
                string bundleDir = data.Attribute("BundleDir").Value;
                actionSet.FemaleHead[id] = new Component()
                {
                    Code = code,
                    ID = id,
                    BundleDir = bundleDir,
                };
                actionSet.FemaleHeadByCode[code] = actionSet.FemaleHead[id];
            }
            foreach (XElement data in xmlNode.Element("Female").Element("Mantle").Elements("Set"))
            {
                int code = int.Parse(data.Attribute("Code").Value);
                string id = data.Attribute("ID").Value;
                string bundleDir = data.Attribute("BundleDir").Value;
                actionSet.FemaleMantle[id] = new Component()
                {
                    Code = code,
                    ID = id,
                    BundleDir = bundleDir,
                };
                actionSet.FemaleMantleByCode[code] = actionSet.FemaleMantle[id];
            }
            foreach (XElement data in xmlNode.Element("Female").Element("Weapon").Elements("Set"))
            {
                int code = int.Parse(data.Attribute("Code").Value);
                string id = data.Attribute("ID").Value;
                string bundleDir = data.Attribute("BundleDir").Value;
                actionSet.FemaleWeapon[id] = new Component()
                {
                    Code = code,
                    ID = id,
                    BundleDir = bundleDir,
                };
                actionSet.FemaleWeaponByCode[code] = actionSet.FemaleWeapon[id];
            }
            #endregion

            #region Rider
            foreach (XElement data in xmlNode.Element("Rider").Elements("Set"))
            {
                int code = int.Parse(data.Attribute("Code").Value);
                string id = data.Attribute("ID").Value;
                string bundleDir = data.Attribute("BundleDir").Value;
                actionSet.Rider[id] = new Component()
                {
                    Code = code,
                    ID = id,
                    BundleDir = bundleDir,
                };
                actionSet.RiderByCode[code] = actionSet.Rider[id];
            }
            #endregion

            return actionSet;
        }
    }
}
