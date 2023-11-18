using GameServer.KiemThe.Entities;
using ProtoBuf;
using Server.Data;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace GameServer.KiemThe.Core.Item
{
    /// <summary>
    /// Thuộc tính magic sử dụng để lưu vào DATABASE
    /// </summary>
    [ProtoContract]
    public class KMagicInfo
    {
        [ProtoMember(1)]
        public int nAttribType { get; set; }

        [ProtoMember(2)]
        public int nLevel { get; set; }

        [ProtoMember(3)]
        public int Value_1 { get; set; }

        [ProtoMember(4)]
        public int Value_2 { get; set; }

        [ProtoMember(5)]
        public int Value_3 { get; set; }
    }

    /// <summary>
    /// Định nghĩa lại phần mã hóa DATA
    /// </summary>
    [ProtoContract]
    public class ItemDataByteCode
    {
        /// <summary>
        /// Tổng có bao nhiêu thông tin cơ bản
        /// </summary>
        [ProtoMember(1)]
        public int BasicPropCount { get; set; }

        /// <summary>
        /// Thông tin về chỉ số
        /// </summary>
        [ProtoMember(2)]
        public List<KMagicInfo> BasicProp { get; set; }

        /// <summary>
        /// Số dòng xanh hiện không ẩn
        /// </summary>
        [ProtoMember(3)]
        public int GreenPropCount { get; set; }

        [ProtoMember(4)]
        public List<KMagicInfo> GreenProp { get; set; }

        /// <summary>
        /// Có bao nhiêu dòng ẩn
        /// </summary>
        [ProtoMember(5)]
        public int HiddenProbsCount { get; set; }

        [ProtoMember(6)]
        public List<KMagicInfo> HiddenProbs { get; set; }
    }

    [XmlRoot(ElementName = "Refine")]
    public class Refine
    {
        [XmlAttribute(AttributeName = "RefineId")]
        public int RefineId { get; set; }

        [XmlAttribute(AttributeName = "SourceItem")]
        public int SourceItem { get; set; }

        [XmlAttribute(AttributeName = "ProduceItem")]
        public int ProduceItem { get; set; }

        [XmlAttribute(AttributeName = "Fee")]
        public int Fee { get; set; }
    }

    public class WaitBeRemove
    {
        public GoodsData _Good { get; set; }

        public int ItemLess { get; set; }
    }

    public class ActiveByItem
    {
        public int Pos1 { get; set; }
        public int Pos2 { get; set; }
    }

    /// <summary>
    /// Classs socket đảm nhận việc kích hoạt hay disable thuộc tính
    /// </summary>
    public class KSOCKET
    {
        public KMagicAttrib sMagicAttrib { get; set; }
        public bool bActive { get; set; }

        public int Index { get; set; }
    };

    [XmlRoot(ElementName = "Stuff")]
    public class Stuff
    {
        [XmlAttribute(AttributeName = "StuffDetail")]
        public int StuffDetail { get; set; }

        [XmlAttribute(AttributeName = "StuffParticular")]
        public int StuffParticular { get; set; }
    }

    [XmlRoot(ElementName = "BookAttr")]
    public class BookAttr
    {
        [XmlAttribute(AttributeName = "StrInitMin")]
        public int StrInitMin { get; set; }

        [XmlAttribute(AttributeName = "StrInitMax")]
        public int StrInitMax { get; set; }

        [XmlAttribute(AttributeName = "DexInitMin")]
        public int DexInitMin { get; set; }

        [XmlAttribute(AttributeName = "DexInitMax")]
        public int DexInitMax { get; set; }

        [XmlAttribute(AttributeName = "VitInitMin")]
        public int VitInitMin { get; set; }

        [XmlAttribute(AttributeName = "VitInitMax")]
        public int VitInitMax { get; set; }

        [XmlAttribute(AttributeName = "EngInitMin")]
        public int EngInitMin { get; set; }

        [XmlAttribute(AttributeName = "EngInitMax")]
        public int EngInitMax { get; set; }

        [XmlAttribute(AttributeName = "SkillID1")]
        public int SkillID1 { get; set; }

        [XmlAttribute(AttributeName = "SkillID2")]
        public int SkillID2 { get; set; }

        [XmlAttribute(AttributeName = "SkillID3")]
        public int SkillID3 { get; set; }

        [XmlAttribute(AttributeName = "SkillID4")]
        public int SkillID4 { get; set; }
    }

    [XmlRoot(ElementName = "ENH")]
    public class ENH
    {
        /// <summary>
        /// Lần cường hóa
        /// </summary>
        [XmlAttribute(AttributeName = "EnhTimes")]
        public int EnhTimes { get; set; }

        /// <summary>
        /// Sysmboy Hiệu ứng
        /// </summary>
        [XmlAttribute(AttributeName = "EnhMAName")]
        public string EnhMAName { get; set; }

        /// <summary>
        /// Giá trị MIN của thuộc tính cường hóa 1
        /// </summary>
        [XmlAttribute(AttributeName = "EnhMAPA1Min")]
        public int EnhMAPA1Min { get; set; }

        /// <summary>
        /// Giá trị MAX của thuộc tính cường hóa 1
        /// </summary>
        [XmlAttribute(AttributeName = "EnhMAPA1Max")]
        public int EnhMAPA1Max { get; set; }

        /// <summary>
        /// Giá trị MIN của thuộc tính cường hóa 2
        /// </summary>
        [XmlAttribute(AttributeName = "EnhMAPA2Min")]
        public int EnhMAPA2Min { get; set; }

        /// <summary>
        /// Giá trị MAX của thuộc tính cường hóa 2
        /// </summary>
        [XmlAttribute(AttributeName = "EnhMAPA2Max")]
        public int EnhMAPA2Max { get; set; }

        /// <summary>
        /// Giá trị MIN của thuộc tính cường hóa 3
        /// </summary>
        [XmlAttribute(AttributeName = "EnhMAPA3Min")]
        public int EnhMAPA3Min { get; set; }

        /// <summary>
        /// Giá trị MAX của thuộc tính cường hóa 3
        /// </summary>
        [XmlAttribute(AttributeName = "EnhMAPA3Max")]
        public int EnhMAPA3Max { get; set; }

        /// <summary>
        /// Index Số thứ tự của Thuộc tính
        /// </summary>
        [XmlAttribute(AttributeName = "Index")]
        public int Index { get; set; }
    }

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
        public int RidePropPA1Min { get; set; }

        /// <summary>
        /// Giá trị MAX của thuộc tính cường hóa 1
        /// </summary>
        [XmlAttribute(AttributeName = "RidePropPA1Max")]
        public int RidePropPA1Max { get; set; }

        /// <summary>
        /// Giá trị MIN của thuộc tính cường hóa 2
        /// </summary>
        [XmlAttribute(AttributeName = "RidePropPA2Min")]
        public int RidePropPA2Min { get; set; }

        /// <summary>
        /// Giá trị MAX của thuộc tính cường hóa 2
        /// </summary>
        [XmlAttribute(AttributeName = "RidePropPA2Max")]
        public int RidePropPA2Max { get; set; }

        /// <summary>
        /// Giá trị MIN của thuộc tính cường hóa 3
        /// </summary>
        [XmlAttribute(AttributeName = "RidePropPA3Min")]
        public int RidePropPA3Min { get; set; }

        /// <summary>
        /// Giá trị MAX của thuộc tính cường hóa 3
        /// </summary>
        [XmlAttribute(AttributeName = "RidePropPA3Max")]
        public int RidePropPA3Max { get; set; }

        /// <summary>
        /// Index Số thứ tự của Thuộc tính
        /// </summary>
        [XmlAttribute(AttributeName = "Index")]
        public int Index { get; set; }
    }

    [XmlRoot(ElementName = "Strengthen")]
    public class Strengthen
    {
        /// <summary>
        /// Lần cường hóa
        /// </summary>
        [XmlAttribute(AttributeName = "StrTimes")]
        public int StrTimes { get; set; }

        /// <summary>
        /// Sysmboy Hiệu ứng
        /// </summary>
        [XmlAttribute(AttributeName = "StrMAName")]
        public string StrMAName { get; set; }

        /// <summary>
        /// Giá trị MIN của thuộc tính cường hóa 1
        /// </summary>
        [XmlAttribute(AttributeName = "StrMAPA1Min")]
        public int StrMAPA1Min { get; set; }

        /// <summary>
        /// Giá trị MAX của thuộc tính cường hóa 1
        /// </summary>
        [XmlAttribute(AttributeName = "StrMAPA1Max")]
        public int StrMAPA1Max { get; set; }

        /// <summary>
        /// Giá trị MIN của thuộc tính cường hóa 2
        /// </summary>
        [XmlAttribute(AttributeName = "StrMAPA2Min")]
        public int StrMAPA2Min { get; set; }

        /// <summary>
        /// Giá trị MAX của thuộc tính cường hóa 2
        /// </summary>
        [XmlAttribute(AttributeName = "StrMAPA2Max")]
        public int StrMAPA2Max { get; set; }

        /// <summary>
        /// Giá trị MIN của thuộc tính cường hóa 3
        /// </summary>
        [XmlAttribute(AttributeName = "StrMAPA3Min")]
        public int StrMAPA3Min { get; set; }

        /// <summary>
        /// Giá trị MAX của thuộc tính cường hóa 3
        /// </summary>
        [XmlAttribute(AttributeName = "StrMAPA3Max")]
        public int StrMAPA3Max { get; set; }

        /// <summary>
        /// Index Số thứ tự của Thuộc tính
        /// </summary>
        [XmlAttribute(AttributeName = "Index")]
        public int Index { get; set; }
    }

    [XmlRoot(ElementName = "SingNetExp")]
    public class SingNetExp
    {
        [XmlAttribute(AttributeName = "Level")]
        public int Level { get; set; }

        [XmlAttribute(AttributeName = "UpgardeExp")]
        public int UpgardeExp { get; set; }

        [XmlAttribute(AttributeName = "Value")]
        public int Value { get; set; }
    }

    [XmlRoot(ElementName = "ExtPram")]
    public class ExtPram
    {
        [XmlAttribute(AttributeName = "Pram")]
        public int Pram { get; set; }

        [XmlAttribute(AttributeName = "Index")]
        public int Index { get; set; }
    }

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

    [XmlRoot(ElementName = "PropMagic")]
    public class PropMagic
    {     /// <summary>
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
        ///
        [XmlAttribute(AttributeName = "Index")]
        public int Index { get; set; }
    }

    [XmlRoot(ElementName = "ReqProp")]
    public class ReqProp
    {
        /// <summary>
        /// Kiểu yêu cầu
        /// </summary>
        [XmlAttribute(AttributeName = "ReqPropType")]
        public int ReqPropType { get; set; }

        /// <summary>
        /// Giá trị yêu cầu
        /// </summary>
        [XmlAttribute(AttributeName = "ReqPropValue")]
        public int ReqPropValue { get; set; }

        /// <summary>
        /// Thứ tự Của Req
        /// </summary>
        [XmlAttribute(AttributeName = "Index")]
        public int Index { get; set; }
    }

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
        public int Index { get; set; }
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
        public int Stack { get ; set; }

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

        public bool IsArtifact { get; set; } = false;


        [XmlAttribute(AttributeName = "UnLockInterval")]
        public int UnLockInterval { get; set; } = 0;
        public int ItemValue
        {
            get
            {
                return FightPower;
            }
        }

        /// <summary>
        /// Loại vũ khí
        /// </summary>
        [XmlAttribute(AttributeName = "Category")]
        public int Category { get; set; }

        /// <summary>
        /// Có phải vật phẩm có Script điều khiển không
        /// </summary>
        public bool IsScriptItem
        {
            get
            {
                if (ItemManager.KD_ISEQUIP(this.Genre))
                {
                    return false;
                }

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
                if (ItemManager.KD_ISEQUIP(this.Genre))
                {
                    return false;
                }

                if (this.Genre == 17)
                {
                    return true;
                }

                return false;
            }
        }
    }

    [XmlRoot(ElementName = "ItemRandom")]
    public class ItemRandom
    {
        [XmlAttribute(AttributeName = "Type")]
        public int Type { get; set; }

        [XmlAttribute(AttributeName = "ItemID")]
        public int ItemID { get; set; }

        [XmlAttribute(AttributeName = "Number")]
        public int Number { get; set; }

        [XmlAttribute(AttributeName = "Series")]
        public int Series { get; set; }

        [XmlAttribute(AttributeName = "TimeLimit")]
        public int TimeLimit { get; set; }

        [XmlAttribute(AttributeName = "Rate")]
        public int Rate { get; set; }

        /// <summary>
        /// 0 : Là không khóa | 1 là có
        /// </summary>
        ///
        [XmlAttribute(AttributeName = "Lock")]
        public int Lock { get; set; }
    }

    [XmlRoot(ElementName = "ConfigBox")]
    public class ConfigBox
    {
        [XmlElement(ElementName = "Boxs")]
        public List<RandomBox> Boxs { get; set; }
    }

    [XmlRoot(ElementName = "RandomBox")]
    public class RandomBox
    {
        /// <summary>
        /// Đánh dấu ID hộp
        /// </summary>
        ///
        [XmlAttribute(AttributeName = "IDBox")]
        public int IDBox { get; set; }

        /// <summary>
        /// Đánh dấu tên hộp nào đang mở
        /// </summary>
        ///
        [XmlAttribute(AttributeName = "BoxName")]
        public string BoxName { get; set; }

        /// <summary>
        /// Tổng số RATE
        /// </summary>
        ///
        [XmlAttribute(AttributeName = "TotalRate")]
        public int TotalRate { get; set; }

        /// <summary>
        /// Mở tối đa trên 1 tuần
        /// </summary>
        [XmlAttribute(AttributeName = "LimitWeek")]
        public int LimitWeek { get; set; }

        /// <summary>
        /// Mở tối đa trên ngày
        /// </summary>
        [XmlAttribute(AttributeName = "LimitDay")]
        public int LimitDay { get; set; }

        [XmlElement(ElementName = "Items")]
        public List<ItemRandom> Items { get; set; }
    }

    /// <summary>
    /// break ra 
    /// </summary>
    /// 
    [XmlRoot(ElementName = "ItemBreakItem")]
    public class ItemBreakItem
    {
        [XmlAttribute(AttributeName = "LEVEL")]
        public int LEVEL { get; set; }

        [XmlAttribute(AttributeName = "DETAILTYPE")]
        public int DETAILTYPE { get; set; }

        [XmlAttribute(AttributeName = "ITEMS")]
        public List<int> ITEMS { get; set; }
    }

    [XmlRoot(ElementName = "ItemBreakConfig")]
    public class ItemBreakConfig
    {
        /// <summary>
        ///  Cấp độ tối thiểu của vật phẩm
        /// </summary>
        /// 
        [XmlAttribute(AttributeName = "MinItemLevel")]
        public int MinItemLevel { get; set; }

        /// <summary>
        /// Số dòng tối thiểu
        /// </summary>
        /// 
        [XmlAttribute(AttributeName = "MinLine")]
        public int MinLine { get; set; }

        /// <summary>
        /// Rate tỉ lệ tính ra 
        /// </summary>
        [XmlAttribute(AttributeName = "Rate")]
        public double Rate { get; set; }

        /// <summary>
        /// Danh sách vật phẩm
        /// </summary>
        /// 
        [XmlElement(ElementName = "BreakItems")]
        public List<ItemBreakItem> BreakItems { get; set; }

    }

    public class RateBuild
    {
        public int Rate { get; set; }
        public int ItemID { get; set; }

        public string ItemName { get; set; }
    }


    #region ItemCaculationValue
    [XmlRoot(ElementName = "ItemValueCaculation")]
    public class ItemValueCaculation
    {
        [XmlElement(ElementName = "Magic_Combine_Def")]
        public Magic_Combine Magic_Combine_Def { get; set; }

        [XmlElement(ElementName = "List_Equip_Type_Rate")]
        public List<Equip_Type_Rate> List_Equip_Type_Rate { get; set; }

        [XmlElement(ElementName = "List_Enhance_Value")]
        public List<Enhance_Value> List_Enhance_Value { get; set; }

        [XmlElement(ElementName = "List_Strengthen_Value")]
        public List<Strengthen_Value> List_Strengthen_Value { get; set; }

        [XmlElement(ElementName = "List_Equip_StarLevel")]
        public List<Equip_StarLevel> List_Equip_StarLevel { get; set; }

        [XmlElement(ElementName = "List_Equip_Random_Pos")]
        public List<Equip_Random_Pos> List_Equip_Random_Pos { get; set; }

        [XmlElement(ElementName = "List_Equip_Level")]
        public List<Equip_Level> List_Equip_Level { get; set; }


        [XmlElement(ElementName = "List_StarLevelStruct")]
        public List<StarLevelStruct> List_StarLevelStruct { get; set; }

    }

    [XmlRoot(ElementName = "MagicSource")]

    public class MagicSource
    {
        [XmlAttribute(AttributeName = "MagicName")]
        public string MagicName { get; set; }

        [XmlAttribute(AttributeName = "Index")]
        public int Index { get; set; }

    }
    [XmlRoot(ElementName = "StarLevelStruct")]
    public class StarLevelStruct
    {
        [XmlAttribute(AttributeName = "Value")]
        public long Value { get; set; }
        [XmlAttribute(AttributeName = "StarLevel")]
        public int StarLevel { get; set; }
        [XmlAttribute(AttributeName = "NameColor")]

        public string NameColor { get; set; }
        [XmlAttribute(AttributeName = "EmptyStar")]
        public int EmptyStar { get; set; }
        [XmlAttribute(AttributeName = "FillStar")]
        public int FillStar { get; set; }


    }




    [XmlRoot(ElementName = "MagicDesc")]
    public class MagicDesc
    {
        [XmlAttribute(AttributeName = "MagicName")]
        public string MagicName { get; set; }

        [XmlAttribute(AttributeName = "ListValue")]
        public List<int> ListValue { get; set; }

    }


    [XmlRoot(ElementName = "Magic_Combine")]
    public class Magic_Combine
    {
        [XmlElement(ElementName = "MagicSourceDef")]
        public List<MagicSource> MagicSourceDef { get; set; }

        [XmlElement(ElementName = "MagicDescDef")]
        public List<MagicDesc> MagicDescDef { get; set; }
    }
    [XmlRoot(ElementName = "Equip_Type_Rate")]
    public class Equip_Type_Rate
    {
        [XmlAttribute(AttributeName = "EquipType")]
        public KE_ITEM_EQUIP_DETAILTYPE EquipType { get; set; }

        [XmlAttribute(AttributeName = "Value")]
        public int Value { get; set; }
    }
    [XmlRoot(ElementName = "Enhance_Value")]
    public class Enhance_Value
    {
        [XmlAttribute(AttributeName = "EnhanceTimes")]
        public int EnhanceTimes { get; set; }

        [XmlAttribute(AttributeName = "Value")]
        public int Value { get; set; }
    }
    [XmlRoot(ElementName = "Equip_StarLevel")]
    public class Equip_StarLevel
    {
        [XmlAttribute(AttributeName = "EQUIP_DETAIL_TYPE")]
        public int EQUIP_DETAIL_TYPE { get; set; }
        [XmlAttribute(AttributeName = "STAR_LEVEL")]
        public int STAR_LEVEL { get; set; }
        [XmlAttribute(AttributeName = "EQUIP_LEVEL_1")]
        public long EQUIP_LEVEL_1 { get; set; }

        [XmlAttribute(AttributeName = "EQUIP_LEVEL_2")]
        public long EQUIP_LEVEL_2 { get; set; }

        [XmlAttribute(AttributeName = "EQUIP_LEVEL_3")]
        public long EQUIP_LEVEL_3 { get; set; }

        [XmlAttribute(AttributeName = "EQUIP_LEVEL_4")]
        public long EQUIP_LEVEL_4 { get; set; }

        [XmlAttribute(AttributeName = "EQUIP_LEVEL_5")]
        public long EQUIP_LEVEL_5 { get; set; }

        [XmlAttribute(AttributeName = "EQUIP_LEVEL_6")]
        public long EQUIP_LEVEL_6 { get; set; }

        [XmlAttribute(AttributeName = "EQUIP_LEVEL_7")]
        public long EQUIP_LEVEL_7 { get; set; }

        [XmlAttribute(AttributeName = "EQUIP_LEVEL_8")]
        public long EQUIP_LEVEL_8 { get; set; }

        [XmlAttribute(AttributeName = "EQUIP_LEVEL_9")]
        public long EQUIP_LEVEL_9 { get; set; }

        [XmlAttribute(AttributeName = "EQUIP_LEVEL_10")]
        public long EQUIP_LEVEL_10 { get; set; }
    }

    [XmlRoot(ElementName = "Equip_Random_Pos")]
    public class Equip_Random_Pos
    {
        [XmlAttribute(AttributeName = "MAGIC_POS")]
        public int MAGIC_POS { get; set; }
        [XmlAttribute(AttributeName = "Value")]
        public int Value { get; set; }
    }
    [XmlRoot(ElementName = "Strengthen_Value")]
    public class Strengthen_Value
    {
        [XmlAttribute(AttributeName = "StrengthenTimes")]
        public int StrengthenTimes { get; set; }
        [XmlAttribute(AttributeName = "Value")]
        public int Value { get; set; }
    }
    [XmlRoot(ElementName = "Equip_Level")]
    public class Equip_Level
    {
        [XmlAttribute(AttributeName = "Level")]
        public int Level { get; set; }
        [XmlAttribute(AttributeName = "Value")]
        public int Value { get; set; }
    }


    public class ComposeItem
    {
        public ItemData nItemMinLevel { get; set; }
        public int nMinLevelRate { get; set; }


        public ItemData nItemMaxLevel { get; set; }

        public int nMaxLevelRate { get; set; }

        public int nFee { get; set; }
    }

    public class CalcProb
    {
        public double nProb { get; set; }

        public long nMoney { get; set; }

        public double nTrueProb { get; set; }
    }
    #endregion
}