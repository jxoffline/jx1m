using System.Collections.Generic;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace FS.VLTK.Entities.Config
{
    /// <summary>
    /// Kích hoạt theo bộ
    /// </summary>
    public class ActiveByItem
    {
        /// <summary>
        /// Bộ 1
        /// </summary>
        public byte Pos1 { get; set; }

        /// <summary>
        /// Bộ 2
        /// </summary>
        public byte Pos2 { get; set; }

    }


    [XmlRoot(ElementName = "ExtPram")]
    public class ExtPram
    {
        [XmlAttribute(AttributeName = "Pram")]
        public int Pram { get; set; }

        [XmlAttribute(AttributeName = "Index")]
        public int Index { get; set; }
    }


    /// <summary>
    /// Thuộc tính cường hóa
    /// </summary>
    [XmlRoot(ElementName = "ENH")]
    public class ENH
    {
        /// <summary>
        /// Lần cường hóa
        /// </summary>
        [XmlAttribute(AttributeName = "EnhTimes")]
        public byte EnhTimes { get; set; }

        /// <summary>
        /// Sysmboy Hiệu ứng
        /// </summary>
        [XmlAttribute(AttributeName = "EnhMAName")]
        public string EnhMAName { get; set; }

        /// <summary>
        /// Giá trị MIN của thuộc tính cường hóa 1
        /// </summary>
        [XmlAttribute(AttributeName = "EnhMAPA1Min")]
        public short EnhMAPA1Min { get; set; }

        /// <summary>
        /// Giá trị MAX của thuộc tính cường hóa 1
        /// </summary>
        [XmlAttribute(AttributeName = "EnhMAPA1Max")]
        public short EnhMAPA1Max { get; set; }

        /// <summary>
        /// Giá trị MIN của thuộc tính cường hóa 2
        /// </summary>
        [XmlAttribute(AttributeName = "EnhMAPA2Min")]
        public short EnhMAPA2Min { get; set; }

        /// <summary>
        /// Giá trị MAX của thuộc tính cường hóa 2
        /// </summary>
        [XmlAttribute(AttributeName = "EnhMAPA2Max")]
        public short EnhMAPA2Max { get; set; }

        /// <summary>
        /// Giá trị MIN của thuộc tính cường hóa 3
        /// </summary>
        [XmlAttribute(AttributeName = "EnhMAPA3Min")]
        public short EnhMAPA3Min { get; set; }

        /// <summary>
        /// Giá trị MAX của thuộc tính cường hóa 3
        /// </summary>
        [XmlAttribute(AttributeName = "EnhMAPA3Max")]
        public short EnhMAPA3Max { get; set; }
    }

    /// <summary>
    /// Thuộc tính thú cưỡi
    /// </summary>
    [XmlRoot(ElementName = "RiderProp")]
    public class RiderProp
    {
        /// <summary>
        /// Lần cường hóa
        /// </summary>
        [XmlAttribute(AttributeName = "RidePropType")]
        public string RidePropType { get; set; }

        /// <summary>
        /// Giá trị MIN của thuộc tính cường hóa 1
        /// </summary>
        [XmlAttribute(AttributeName = "RidePropPA1Min")]
        public short RidePropPA1Min { get; set; }

        /// <summary>
        /// Giá trị MAX của thuộc tính cường hóa 1
        /// </summary>
        [XmlAttribute(AttributeName = "RidePropPA1Max")]
        public short RidePropPA1Max { get; set; }

        /// <summary>
        /// Giá trị MIN của thuộc tính cường hóa 2
        /// </summary>
        [XmlAttribute(AttributeName = "RidePropPA2Min")]
        public short RidePropPA2Min { get; set; }

        /// <summary>
        /// Giá trị MAX của thuộc tính cường hóa 2
        /// </summary>
        [XmlAttribute(AttributeName = "RidePropPA2Max")]
        public short RidePropPA2Max { get; set; }

        /// <summary>
        /// Giá trị MIN của thuộc tính cường hóa 3
        /// </summary>
        [XmlAttribute(AttributeName = "RidePropPA3Min")]
        public short RidePropPA3Min { get; set; }

        /// <summary>
        /// Giá trị MAX của thuộc tính cường hóa 3
        /// </summary>
        [XmlAttribute(AttributeName = "RidePropPA3Max")]
        public short RidePropPA3Max { get; set; }
    }

    /// <summary>
    /// Thuộc tính thuốc
    /// </summary>
    [XmlRoot(ElementName = "Medicine")]
    public class Medicine
    {
        [XmlAttribute(AttributeName = "MagicName")]
        public string MagicName { get; set; }

        [XmlAttribute(AttributeName = "Value")]
        public int Value { get; set; }

        [XmlAttribute(AttributeName = "Time")]
        public int Time { get; set; }

        [XmlAttribute(AttributeName = "Index")]
        public int Index { get; set; }
    }

    /// <summary>
    /// Giá trị thuộc tính đồ
    /// </summary>
    [XmlRoot(ElementName = "PropMagic")]
    public class PropMagic
    {   /// <summary>
        /// ID của Thuộc tính
        /// </summary>
        [XmlAttribute(AttributeName = "MagicName")]
        public string MagicName { get; set; }

        /// <summary>
        /// Level thuộc tính
        /// </summary>
        [XmlAttribute(AttributeName = "MagicLevel")]
        public string MagicLevel { get; set; }

        /// <summary>
        /// Dòng có kích hoạt hay không
        /// </summary>
        [XmlAttribute(AttributeName = "IsActive")]
        public bool IsActive { get; set; }

        /// <summary>
        /// Số thứ tự của dòng
        /// </summary>
        [XmlAttribute(AttributeName = "Index")]
        public byte Index { get; set; }
    }

    /// <summary>
    /// Yêu cầu
    /// </summary>
    [XmlRoot(ElementName = "ReqProp")]
    public class ReqProp
    {
        /// <summary>
        /// Kiểu yêu cầu
        /// </summary>
        [XmlAttribute(AttributeName = "ReqPropType")]
        public short ReqPropType { get; set; }

        /// <summary>
        /// Giá trị yêu cầu
        /// </summary>
        [XmlAttribute(AttributeName = "ReqPropValue")]
        public int ReqPropValue { get; set; }
    }

    /// <summary>
    /// Thuộc tính cơ bản
    /// </summary>
    [XmlRoot(ElementName = "BasicProp")]
    public class BasicProp
    {
        /// <summary>
        /// Symboy thuộc tính
        /// </summary>
        [XmlAttribute(AttributeName = "BasicPropType")]
        public string BasicPropType { get; set; }

        /// <summary>
        /// Giá trị MIN thứ 1 của thuộc tính
        /// </summary>
        [XmlAttribute(AttributeName = "BasicPropPA1Min")]
        public int BasicPropPA1Min { get; set; }

        /// <summary>
        /// Giá trị MAX thứ 1 của thuộc tính
        /// </summary>
        [XmlAttribute(AttributeName = "BasicPropPA1Max")]
        public int BasicPropPA1Max { get; set; }

        /// <summary>
        /// Giá trị MIN thứ 2 của thuộc tính
        /// </summary>
        [XmlAttribute(AttributeName = "BasicPropPA2Min")]
        public int BasicPropPA2Min { get; set; }

        /// <summary>
        /// Giá trị MAX thứ 2 của thuộc tính
        /// </summary>
        [XmlAttribute(AttributeName = "BasicPropPA2Max")]
        public int BasicPropPA2Max { get; set; }

        /// <summary>
        /// Giá trị MIN thứ 3 của thuộc tính
        /// </summary>
        [XmlAttribute(AttributeName = "BasicPropPA3Min")]
        public int BasicPropPA3Min { get; set; }

        /// <summary>
        /// Giá trị MAX thứ 3 của thuộc tính
        /// </summary>
        [XmlAttribute(AttributeName = "BasicPropPA3Max")]
        public int BasicPropPA3Max { get; set; }

        /// <summary>
        /// Giá trị INDEX
        /// </summary>
        [XmlAttribute(AttributeName = "Index")]
        public sbyte Index { get; set; }
    }

    /// <summary>
    /// Thuộc tính đối tượng vật phẩm
    /// </summary>
    [XmlRoot(ElementName = "ItemData")]
    public class ItemData
    {
        /// <summary>
        /// ID tự sinh của vật phẩm
        /// </summary>
        [XmlAttribute(AttributeName = "ItemID")]
        public int ItemID { get; set; }
        /// <summary>
        /// Tên vật phẩm
        /// </summary>
        [XmlAttribute(AttributeName = "Name")]
        public string Name { get; set; }
        /// <summary>
        /// Thể loại đồ
        /// </summary>
        [XmlAttribute(AttributeName = "Genre")]
        public int Genre { get; set; }
        /// <summary>
        /// Kiểu chi tiết
        /// </summary>
        ///
        [XmlAttribute(AttributeName = "DetailType")]
        public int DetailType { get; set; }

        /// <summary>
        /// Sử dụng để tính toán ID
        /// </summary>
        [XmlAttribute(AttributeName = "ParticularType")]
        public int ParticularType { get; set; }
        /// <summary>
        /// Sử dụng để biết hình ảnh
        /// </summary>
        ///
        [XmlAttribute(AttributeName = "Image")]
        public string Image { get; set; }

        /// <summary>
        /// Chưa biết làm cái gì nhưng có vẻ cũng quan trọng khả năng là cầu hình drop?
        /// </summary>
        [XmlAttribute(AttributeName = "ObjDrop")]
        public int ObjDrop { get; set; }

        /// <summary>
        /// Một số mô tả cơ bản về nguồn gốc của vật phẩm
        /// </summary>
        [XmlAttribute(AttributeName = "Descript")]
        public string Descript { get; set; }


        /// <summary>
        /// Ngũ hành của trang bị
        /// </summary>
        [XmlAttribute(AttributeName = "Series")]
        public int Series { get; set; }


        /// <summary>
        /// Giá tiền
        /// </summary>
        [XmlAttribute(AttributeName = "Price")]
        public int Price { get; set; }


        /// <summary>
        /// Cấp của trang bị?
        /// </summary>
        [XmlAttribute(AttributeName = "Level")]
        public int Level { get; set; }

        /// <summary>
        /// Xếp chồng
        /// </summary>
        [XmlAttribute(AttributeName = "Stack")]
        public int Stack { get; set; }


        /// <summary>
        /// Cái này mình tự thêm vào để sau config hình ảnh cho nữ
        /// </summary>
        [XmlAttribute(AttributeName = "ResWomanID")]
        public int ResWomanID { get; set; } = -1;


        /// <summary>
        /// Cái này mình tự config vào để sau này config hình ảnh khi mặc đồ nam
        /// </summary>
        [XmlAttribute(AttributeName = "ResManID")]
        public int ResManID { get; set; } = -1;

        /// <summary>
        /// ID GROUP TƯƠNG ỨNG
        /// </summary>
        [XmlAttribute(AttributeName = "SuiteID")]
        public int SuiteID { get; set; }

        /// <summary>
        /// ID set phụ tương ứng
        /// </summary>
        [XmlAttribute(AttributeName = "Expansionset")]
        public int Expansionset { get; set; }

        /// <summary>
        /// Nguyên liệu khi phân giã gì đó
        /// </summary>
        [XmlAttribute(AttributeName = "SmeltableQuality")]
        public int SmeltableQuality { get; set; }

        /// <summary>
        /// Nguyên liệu khi phân giã gì đó
        /// </summary>
        [XmlAttribute(AttributeName = "MeltableQuality")]
        public int MeltableQuality { get; set; }

        /// <summary>
        /// Danh sách thuộc tính thêm vào khi trang bị đủ set
        /// Từ MAGIC1 -> MAGIC3
        /// </summary>
        [XmlElement(ElementName = "ExpansionSetHidden")]
        public List<PropMagic> ExpansionSetHidden { get; set; }
        /// <summary>
        /// Danh sách các dòng sanh
        /// Từ MAGIC1 -> MAGIC3
        /// </summary>
        [XmlElement(ElementName = "GreenProp")]
        public List<PropMagic> GreenProp { get; set; }



        [XmlElement(ElementName = "RiderProp")]
        public List<RiderProp> RiderProp { get; set; }
        /// <summary>
        /// Danh sách các dòng ẩn
        /// TỪ MAGIC 4-> MAGIC 6
        /// </summary>
        [XmlElement(ElementName = "HiddenProp")]
        public List<PropMagic> HiddenProp { get; set; }
        /// <summary>
        ///  YÊU CẦU NGŨ HÀNH ĐỂ TRANG BỊ
        [XmlElement(ElementName = "ListReqProp")]
        public List<ReqProp> ListReqProp { get; set; }
        /// <summary>
        /// Danh sách thuộc tính cơ bản của item
        /// </summary>
        [XmlElement(ElementName = "ListBasicProp")]
        public List<BasicProp> ListBasicProp { get; set; }
        /// <summary>
        /// Danh sách thuộc tính cường hóa thêm vào để sau này phát triển
        /// </summary>
        [XmlElement(ElementName = "ListEnhance")]
        public List<ENH> ListEnhance { get; set; }


        // NẾU LÀ THUỐC THÌ CÓ CÁI NÀY
        [XmlElement(ElementName = "MedicineProp")]
        public List<Medicine> MedicineProp { get; set; }


        /// <summary>
        /// Thuộc tính thêm vào
        /// </summary>
        [XmlElement(ElementName = "ListExtPram")]
        public List<ExtPram> ListExtPram { get; set; }


        [XmlAttribute(AttributeName = "BuffID")]
        public int BuffID { get; set; } = -1;


        [XmlAttribute(AttributeName = "DeductOnUse")]
        public bool DeductOnUse { get; set; } = false;


        [XmlAttribute(AttributeName = "ScriptID")]
        public int ScriptID { get; set; } = -1;


        [XmlAttribute(AttributeName = "FightPower")]
        public int FightPower { get; set; } = -1;


        /// <summary>
        /// ID Res mặt nạ (nếu là mặt nạ)
        /// </summary>
        [XmlAttribute(AttributeName = "MaskResID")]
        public string MaskResID { get; set; }


        public bool IsArtifact { get; set; } = false;


        public int ItemValue
        {
            get
            {
                return FightPower;
            }
        }

        /// <summary>
        /// Có phải vật phẩm có Script điều khiển không
        /// </summary>
        public bool IsScriptItem
        {
            get
            {
                //if (ItemManager.KD_ISEQUIP(this.Genre))
                //{
                //    return false;
                //}

                if (this.Genre == 6 && this.ScriptID != -1)
                {
                    return true;
                }

                return false;
            }
        }


        public bool IsMedicine
        {
            get
            {

                if (this.Genre == 17)
                {
                    return true;
                }

                return false;
            }
        }


        public bool IsEquip
        {
            get
            {
                if (this.Genre == 0 || this.Genre == 3 || this.Genre == 2 || this.Genre ==7)
                {
                    return true;
                }
                else
                {
                    return false;
                }

            }
        }

        // cái này chưa biết sau kịch bản khóa đồ thế nào nên tạm bỏ trống
        public int BindType { get; set; } = -1;

        // Tạm ra đây lúc nào làm xong thì tính sau
        public string MapSpriteBundleDir { get; set; } = "";
        // Tạm ra đây lúc nào làm xong thì tính sau
        public string MapSpriteAtlasName { get; set; } = "";

        /// <summary>
        /// Loại vũ khí
        /// </summary>
        public int Category { get; set; } = -1;

        public string IconBundleDir { get; set; } = "";

        public string IconAtlasName { get; set; } = "";

        public string Icon { get { return Image; } }
    }

    /// <summary>
    /// Thông tin cường hóa Ngũ Hành Ấn
    /// </summary>
    [XmlRoot(ElementName = "SingNetExp")]
    public class SingNetExp
    {
        /// <summary>
        /// Cấp độ
        /// </summary>
        [XmlAttribute(AttributeName = "Level")]
        public short Level { get; set; }

        /// <summary>
        /// Lượng Exp cần
        /// </summary>
        [XmlAttribute(AttributeName = "UpgardeExp")]
        public int UpgardeExp { get; set; }

        /// <summary>
        /// Giá trị tài phú có được
        /// </summary>
        [XmlAttribute(AttributeName = "Value")]
        public int Value { get; set; }

        /// <summary>
        /// Chuyển đối tượng từ XMLNode
        /// </summary>
        /// <param name="xmlNode"></param>
        /// <returns></returns>
        public static SingNetExp Parse(XElement xmlNode)
        {
            return new SingNetExp()
            {
                Level = short.Parse(xmlNode.Attribute("Level").Value),
                UpgardeExp = int.Parse(xmlNode.Attribute("UpgardeExp").Value),
                Value = int.Parse(xmlNode.Attribute("Value").Value),
            };
        }
    }
}
