using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace FS.VLTK.Entities.Config
{
    /// <summary>
    /// Kỹ năng sống
    /// </summary>
    [XmlRoot(ElementName = "LifeSkill")]
    public class LifeSkill
    {
        /// <summary>
        /// Danh sách kỹ năng
        /// </summary>
        [XmlElement(ElementName = "TotalSkill")]
        public List<LifeSkillData> TotalSkill { get; set; }

        /// <summary>
        /// Danh sách tên công thức
        /// </summary>
        [XmlElement(ElementName = "TotalRecipeDesc")]
        public List<RecipeDesc> TotalRecipeDesc { get; set; }

        /// <summary>
        /// Danh sách công thức
        /// </summary>
        [XmlElement(ElementName = "TotalRecipe")]
        public List<Recipe> TotalRecipe { get; set; }

        /// <summary>
        /// Danh sách kinh nghiệm
        /// </summary>
        [XmlElement(ElementName = "TotalExp")]
        public List<LifeSkillExp> TotalExp { get; set; }

        /// <summary>
        /// Chuyển đối tượng từ XMLNode
        /// </summary>
        /// <param name="xmlNode"></param>
        /// <returns></returns>
        public static LifeSkill Parse(XElement xmlNode)
        {
            LifeSkill lifeSkill = new LifeSkill()
            {
                TotalSkill = new List<LifeSkillData>(),
                TotalExp = new List<LifeSkillExp>(),
                TotalRecipe = new List<Recipe>(),
                TotalRecipeDesc = new List<RecipeDesc>(),
            };

            foreach (XElement node in xmlNode.Elements("TotalSkill"))
            {
                lifeSkill.TotalSkill.Add(LifeSkillData.Parse(node));
            }
            foreach (XElement node in xmlNode.Elements("TotalExp"))
            {
                lifeSkill.TotalExp.Add(LifeSkillExp.Parse(node));
            }
            foreach (XElement node in xmlNode.Elements("TotalRecipe"))
            {
                lifeSkill.TotalRecipe.Add(Recipe.Parse(node));
            }
            foreach (XElement node in xmlNode.Elements("TotalRecipeDesc"))
            {
                lifeSkill.TotalRecipeDesc.Add(RecipeDesc.Parse(node));
            }

            return lifeSkill;
        }
    }

    /// <summary>
    /// Dữ liệu kỹ năng sống
    /// </summary>
    [XmlRoot(ElementName = "LifeSkillData")]
    public class LifeSkillData
    {
        /// <summary>
        /// Tên kỹ năng
        /// </summary>
        [XmlAttribute(AttributeName = "Name")]
        public string Name { get; set; }
        
        /// <summary>
        /// Loại (1: Gia công, 0: Chế tạo)
        /// </summary>

        [XmlAttribute(AttributeName = "Gene")]
        public byte Gene { get; set; }

        /// <summary>
        /// Loại kỹ năng sống
        /// </summary>
        [XmlAttribute(AttributeName = "Belong")]
        public byte Belong { get; set; }

        /// <summary>
        /// Chuyển đối tượng từ XMLNode
        /// </summary>
        /// <param name="xmlNode"></param>
        /// <returns></returns>
        public static LifeSkillData Parse(XElement xmlNode)
        {
            return new LifeSkillData()
            {
                Name = xmlNode.Attribute("Name").Value,
                Gene = byte.Parse(xmlNode.Attribute("Gene").Value),
                Belong = byte.Parse(xmlNode.Attribute("Belong").Value),
            };
        }
    }

    /// <summary>
    /// Thông tin công thức
    /// </summary>
    [XmlRoot(ElementName = "LifeSkillData")]
    public class RecipeDesc
    {
        /// <summary>
        /// ID công thức
        /// </summary>
        [XmlAttribute(AttributeName = "KindId")]
        public short KindId { get; set; }

        /// <summary>
        /// Tên công thức
        /// </summary>
        [XmlAttribute(AttributeName = "Name")]
        public string Name { get; set; }

        /// <summary>
        /// Chuyển đối tượng từ XMLNode
        /// </summary>
        /// <param name="xmlNode"></param>
        /// <returns></returns>
        public static RecipeDesc Parse(XElement xmlNode)
        {
            return new RecipeDesc()
            {
                KindId = short.Parse(xmlNode.Attribute("KindId").Value),
                Name = xmlNode.Attribute("Name").Value,
            };
        }
    }

    /// <summary>
    /// Kinh nghiệm kỹ năng sống
    /// </summary>
    [XmlRoot(ElementName = "LifeSkillExp")]
    public class LifeSkillExp
    {
        /// <summary>
        /// Cấp độ
        /// </summary>
        [XmlAttribute(AttributeName = "Level")]
        public short Level { get; set; }

        /// <summary>
        /// Kinh nghiệm cần
        /// </summary>
        [XmlAttribute(AttributeName = "Exp")]
        public int Exp { get; set; }

        /// <summary>
        /// Chuyển đối tượng từ XMLNode
        /// </summary>
        /// <param name="xmlNode"></param>
        /// <returns></returns>
        public static LifeSkillExp Parse(XElement xmlNode)
        {
            return new LifeSkillExp()
            {
                Exp = int.Parse(xmlNode.Attribute("Exp").Value),
                Level = short.Parse(xmlNode.Attribute("Level").Value),
            };
        }
    }

    /// <summary>
    /// Chi tiết công thức
    /// </summary>
    [XmlRoot(ElementName = "Recipe")]
    public class Recipe
    {
        /// <summary>
        /// ID công thức
        /// </summary>
        [XmlAttribute(AttributeName = "ID")]
        public short ID { get; set; }

        /// <summary>
        /// ID danh mục
        /// </summary>
        [XmlAttribute(AttributeName = "Category")]
        public sbyte Category { get; set; }

        /// <summary>
        /// Loại sản phẩm
        /// </summary>
        [XmlAttribute(AttributeName = "Kind")]
        public short Kind { get; set; }

        /// <summary>
        /// Thuộc loại kỹ năng sống nào
        /// </summary>
        [XmlAttribute(AttributeName = "Belong")]
        public byte Belong { get; set; }

        /// <summary>
        /// Cấp độ yêu cầu
        /// </summary>
        [XmlAttribute(AttributeName = "SkillLevel")]
        public byte SkillLevel { get; set; }

        /// <summary>
        /// Số tinh/hoạt lực mất
        /// </summary>
        [XmlAttribute(AttributeName = "Cost")]
        public int Cost { get; set; }

        /// <summary>
        /// Số điểm kinh nghiệm có được
        /// </summary>
        [XmlAttribute(AttributeName = "ExpGain")]
        public int ExpGain { get; set; }

        /// <summary>
        /// Số Frame thời gian chế tạo
        /// </summary>
        [XmlAttribute(AttributeName = "MakeTime")]
        public int MakeTime { get; set; }

        /// <summary>
        /// Danh sách sản phẩm đầu ra
        /// </summary>
        [XmlElement(ElementName = "ListProduceOut")]
        public List<ItemCraf> ListProduceOut { get; set; }

        /// <summary>
        /// Danh sách nguyên liệu yêu cầu
        /// </summary>
        [XmlElement(ElementName = "ListStuffRequest")]
        public List<ItemStuff> ListStuffRequest { get; set; }

        /// <summary>
        /// Chuyển đối tượng từ XMLNode
        /// </summary>
        /// <param name="xmlNode"></param>
        /// <returns></returns>
        public static Recipe Parse(XElement xmlNode)
        {
            Recipe recipe = new Recipe()
            {
                ID = short.Parse(xmlNode.Attribute("ID").Value),
                Category = sbyte.Parse(xmlNode.Attribute("Category").Value),
                Kind = short.Parse(xmlNode.Attribute("Kind").Value),
                Belong = byte.Parse(xmlNode.Attribute("Belong").Value),
                SkillLevel = byte.Parse(xmlNode.Attribute("SkillLevel").Value),
                Cost = int.Parse(xmlNode.Attribute("Cost").Value),
                ExpGain = int.Parse(xmlNode.Attribute("ExpGain").Value),
                MakeTime = int.Parse(xmlNode.Attribute("MakeTime").Value),
                ListProduceOut = new List<ItemCraf>(),
                ListStuffRequest = new List<ItemStuff>(),
            };

            foreach (XElement node in xmlNode.Elements("ListProduceOut"))
            {
                recipe.ListProduceOut.Add(ItemCraf.Parse(node));
            }
            foreach (XElement node in xmlNode.Elements("ListStuffRequest"))
            {
                recipe.ListStuffRequest.Add(ItemStuff.Parse(node));
            }

            return recipe;
        }
    }

    /// <summary>
    /// Nguyên liệu yêu cầu
    /// </summary>
    [XmlRoot(ElementName = "ItemStuff")]
    public class ItemStuff
    {
        /// <summary>
        /// Số lượng cần
        /// </summary>
        [XmlAttribute(AttributeName = "Number")]
        public byte Number { get; set; }

        /// <summary>
        /// ID vật phẩm
        /// </summary>
        [XmlAttribute(AttributeName = "ItemTemplateID")]
        public int ItemTemplateID { get; set; }

        /// <summary>
        /// Chuyển đối tượng từ XMLNode
        /// </summary>
        /// <param name="xmlNode"></param>
        /// <returns></returns>
        public static ItemStuff Parse(XElement xmlNode)
        {
            return new ItemStuff()
            {
                Number = byte.Parse(xmlNode.Attribute("Number").Value),
                ItemTemplateID = int.Parse(xmlNode.Attribute("ItemTemplateID").Value),
            };
        }
    }

    /// <summary>
    /// Sản phẩm chế tạo
    /// </summary>
    [XmlRoot(ElementName = "ItemCraf")]
    public class ItemCraf
    {
        /// <summary>
        /// Tỷ lệ
        /// </summary>
        [XmlAttribute(AttributeName = "Rate")]
        public byte Rate { get; set; }

        /// <summary>
        /// ID vật phẩm
        /// </summary>
        [XmlAttribute(AttributeName = "ItemTemplateID")]
        public int ItemTemplateID { get; set; }

        /// <summary>
        /// Chuyển đối tượng từ XMLNode
        /// </summary>
        /// <param name="xmlNode"></param>
        /// <returns></returns>
        public static ItemCraf Parse(XElement xmlNode)
        {
            return new ItemCraf()
            {
                Rate = byte.Parse(xmlNode.Attribute("Rate").Value),
                ItemTemplateID = int.Parse(xmlNode.Attribute("ItemTemplateID").Value),
            };
        }
    }
}
