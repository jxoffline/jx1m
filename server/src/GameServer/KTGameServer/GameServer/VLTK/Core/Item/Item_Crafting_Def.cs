using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace GameServer.KiemThe.Core.Item
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
        /// Danh sách công thức vật phẩm sự kiện hoạt động
        /// </summary>
        [XmlElement(ElementName = "TotalEventRecipe")]
        public List<EventRecipe> TotalEventRecipe { get; set; }

        /// <summary>
        /// Danh sách hạng mục chế
        /// </summary>
        [XmlElement(ElementName = "TotalCategoryDesc")]
        public List<CategoryDesc> TotalCategoryDesc { get; set; }

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
                TotalCategoryDesc = new List<CategoryDesc>(),
                TotalEventRecipe = new List<EventRecipe>(),
                TotalExp = new List<LifeSkillExp>(),
                TotalRecipe = new List<Recipe>(),
                TotalRecipeDesc = new List<RecipeDesc>(),
            };

            foreach (XElement node in xmlNode.Elements("TotalSkill"))
            {
                lifeSkill.TotalSkill.Add(LifeSkillData.Parse(node));
            }
            foreach (XElement node in xmlNode.Elements("TotalCategoryDesc"))
            {
                lifeSkill.TotalCategoryDesc.Add(CategoryDesc.Parse(node));
            }
            foreach (XElement node in xmlNode.Elements("TotalEventRecipe"))
            {
                lifeSkill.TotalEventRecipe.Add(EventRecipe.Parse(node));
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
        /// ID kỹ năng
        /// </summary>
        [XmlAttribute(AttributeName = "ID")]
        public int ID { get; set; }

        /// <summary>
        /// Tên kỹ năng
        /// </summary>
        [XmlAttribute(AttributeName = "Name")]
        public string Name { get; set; }

        /// <summary>
        /// Loại (1: Gia công, 0: Chế tạo)
        /// </summary>

        [XmlAttribute(AttributeName = "Gene")]
        public int Gene { get; set; }

        /// <summary>
        /// Loại kỹ năng sống
        /// </summary>
        [XmlAttribute(AttributeName = "Belong")]
        public int Belong { get; set; }

        /// <summary>
        /// Icon kỹ năng
        /// </summary>
        [XmlAttribute(AttributeName = "Icon")]
        public string Icon { get; set; }

        /// <summary>
        /// Mô tả kỹ năng
        /// </summary>
        [XmlAttribute(AttributeName = "Desc")]
        public string Desc { get; set; }

        /// <summary>
        /// Chưa dùng
        /// </summary>
        [XmlAttribute(AttributeName = "BGM")]
        public string BGM { get; set; }

        /// <summary>
        /// Cấp độ tối đa
        /// </summary>
        [XmlAttribute(AttributeName = "MaxLevel")]
        public int MaxLevel { get; set; }

        /// <summary>
        /// Chuyển đối tượng từ XMLNode
        /// </summary>
        /// <param name="xmlNode"></param>
        /// <returns></returns>
        public static LifeSkillData Parse(XElement xmlNode)
        {
            return new LifeSkillData()
            {
                ID = int.Parse(xmlNode.Attribute("ID").Value),
                Name = xmlNode.Attribute("Name").Value,
                Gene = int.Parse(xmlNode.Attribute("Gene").Value),
                Belong = int.Parse(xmlNode.Attribute("Belong").Value),
                Icon = xmlNode.Attribute("Icon").Value,
                Desc = xmlNode.Attribute("Desc").Value,
                BGM = xmlNode.Attribute("BGM").Value,
                MaxLevel = int.Parse(xmlNode.Attribute("MaxLevel").Value),
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
        public int KindId { get; set; }

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
                KindId = int.Parse(xmlNode.Attribute("KindId").Value),
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
        public int Level { get; set; }

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
                Level = int.Parse(xmlNode.Attribute("Level").Value),
            };
        }
    }

    /// <summary>
    /// Công thức hoạt động, sự kiện
    /// </summary>
    [XmlRoot(ElementName = "EventRecipe")]
    public class EventRecipe
    {
        /// <summary>
        /// ID công thức
        /// </summary>
        [XmlAttribute(AttributeName = "Id")]
        public int Id { get; set; }

        /// <summary>
        /// Ngày bắt đầu
        /// </summary>
        [XmlAttribute(AttributeName = "StartDate")]
        public string StartDate { get; set; }

        /// <summary>
        /// Ngày kết thúc
        /// </summary>
        [XmlAttribute(AttributeName = "EndDate")]
        public string EndDate { get; set; }

        /// <summary>
        /// Mô tả
        /// </summary>
        [XmlAttribute(AttributeName = "Desc")]
        public string Desc { get; set; }

        /// <summary>
        /// Chuyển đối tượng từ XMLNode
        /// </summary>
        /// <param name="xmlNode"></param>
        /// <returns></returns>
        public static EventRecipe Parse(XElement xmlNode)
        {
            return new EventRecipe()
            {
                Id = int.Parse(xmlNode.Attribute("Id").Value),
                StartDate = xmlNode.Attribute("StartDate").Value,
                EndDate = xmlNode.Attribute("EndDate").Value,
                Desc = xmlNode.Attribute("Desc").Value,
            };
        }
    }

    /// <summary>
    /// Danh mục
    /// </summary>
    [XmlRoot(ElementName = "CategoryDesc")]
    public class CategoryDesc
    {
        /// <summary>
        /// ID danh mục
        /// </summary>
        [XmlAttribute(AttributeName = "CategoryId")]
        public int CategoryId { get; set; }

        /// <summary>
        /// Tên danh mục
        /// </summary>
        [XmlAttribute(AttributeName = "Name")]
        public string Name { get; set; }

        /// <summary>
        /// Chuyển đối tượng từ XMLNode
        /// </summary>
        /// <param name="xmlNode"></param>
        /// <returns></returns>
        public static CategoryDesc Parse(XElement xmlNode)
        {
            return new CategoryDesc()
            {
                CategoryId = int.Parse(xmlNode.Attribute("CategoryId").Value),
                Name = xmlNode.Attribute("Name").Value,
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
        public int ID { get; set; }

        /// <summary>
        /// ID danh mục
        /// </summary>
        [XmlAttribute(AttributeName = "Category")]
        public int Category { get; set; }

        /// <summary>
        /// Loại sản phẩm
        /// </summary>
        [XmlAttribute(AttributeName = "Kind")]
        public int Kind { get; set; }


        [XmlAttribute(AttributeName = "Storage")]
        public int Storage { get; set; }

        /// <summary>
        /// Tên công thức
        /// </summary>
        [XmlAttribute(AttributeName = "Name")]
        public string Name { get; set; }

        /// <summary>
        /// Thuộc loại kỹ năng sống nào
        /// </summary>
        [XmlAttribute(AttributeName = "Belong")]
        public int Belong { get; set; }

        /// <summary>
        /// Cấp độ yêu cầu
        /// </summary>
        [XmlAttribute(AttributeName = "SkillLevel")]
        public int SkillLevel { get; set; }

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
                ID = int.Parse(xmlNode.Attribute("ID").Value),
                Category = int.Parse(xmlNode.Attribute("Category").Value),
                Kind = int.Parse(xmlNode.Attribute("Kind").Value),
                Storage = int.Parse(xmlNode.Attribute("Storage").Value),
                Name = xmlNode.Attribute("Name").Value,
                Belong = int.Parse(xmlNode.Attribute("Belong").Value),
                SkillLevel = int.Parse(xmlNode.Attribute("SkillLevel").Value),
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
        public int Number { get; set; }

        /// <summary>
        /// Ngũ hành
        /// </summary>
        [XmlAttribute(AttributeName = "Series")]
        public int Series { get; set; }

        /// <summary>
        /// Khóa hay không
        /// </summary>
        [XmlAttribute(AttributeName = "Bind")]
        public int Bind { get; set; }

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
                Number = int.Parse(xmlNode.Attribute("Number").Value),
                Series = int.Parse(xmlNode.Attribute("Series").Value),
                Bind = int.Parse(xmlNode.Attribute("Bind").Value),
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
        public int Rate { get; set; }

        /// <summary>
        /// Ngũ hành
        /// </summary>
        [XmlAttribute(AttributeName = "Series")]
        public int Series { get; set; }

        /// <summary>
        /// Khóa hay không
        /// </summary>
        [XmlAttribute(AttributeName = "Bind")]
        public int Bind { get; set; }

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
                Rate = int.Parse(xmlNode.Attribute("Rate").Value),
                Series = int.Parse(xmlNode.Attribute("Series").Value),
                Bind = int.Parse(xmlNode.Attribute("Bind").Value),
                ItemTemplateID = int.Parse(xmlNode.Attribute("ItemTemplateID").Value),
            };
        }
    }
}
