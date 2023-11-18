using System.Collections.Generic;
using System.Xml.Linq;

namespace GameServer.VLTK.Entities.Pet
{
    /// <summary>
    /// Cấu hình Pet
    /// </summary>
    public class PetDataXML
    {
        /// <summary>
        /// ID
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// Cấu hình
        /// </summary>
        public string ResID { get; set; }

        /// <summary>
        /// Tên
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Loại pet
        /// </summary>
        public string TypeDesc { get; set; }

        /// <summary>
        /// Danh sách kỹ năng ban đầu
        /// </summary>
        public List<int> Skills { get; set; }

        /// <summary>
        /// Sinh lực cơ bản
        /// </summary>
        public int HP { get; set; }

        /// <summary>
        /// Sức mạnh cơ bản
        /// </summary>
        public int Str { get; set; }

        /// <summary>
        /// Thân pháp cơ bản
        /// </summary>
        public int Dex { get; set; }

        /// <summary>
        /// Ngoại công cơ bản
        /// </summary>
        public int Sta { get; set; }

        /// <summary>
        /// Nội công cơ bản
        /// </summary>
        public int Int { get; set; }

        /// <summary>
        /// 1 điểm sức tương đương bao nhiêu điểm vật công ngoại
        /// </summary>
        public int StrToPAtk { get; set; }

        /// <summary>
        /// 1 điểm thân tương đương bao nhiêu điểm chính xác
        /// </summary>
        public int DexToHit { get; set; }

        /// <summary>
        /// 1 điểm thân tương đương bao nhiêu điểm né tránh
        /// </summary>
        public int DexToDodge { get; set; }

        /// <summary>
        /// 1 điểm ngoại tương đương bao nhiêu sinh lực
        /// </summary>
        public int StaToHP { get; set; }

        /// <summary>
        /// 1 điểm nội tương đương bao nhiêu điểm vật công nội
        /// </summary>
        public int IntToMAtk { get; set; }

        /// <summary>
        /// Số điểm sức cộng thêm mỗi khi lên cấp
        /// </summary>
        public int StrPerLevel { get; set; }

        /// <summary>
        /// Số điểm thân cộng thêm mỗi khi lên cấp
        /// </summary>
        public int DexPerLevel { get; set; }

        /// <summary>
        /// Số điểm ngoại cộng thêm mỗi khi lên cấp
        /// </summary>
        public int StaPerLevel { get; set; }

        /// <summary>
        /// Số điểm nội cộng thêm mỗi khi lên cấp
        /// </summary>
        public int IntPerLevel { get; set; }

        /// <summary>
        /// Chuyển đối tượng từ XMLNode
        /// </summary>
        /// <param name="xmlNode"></param>
        /// <returns></returns>
        public static PetDataXML Parse(XElement xmlNode)
        {
            /// Tạo mới
            PetDataXML petData = new PetDataXML()
            {
                ID = int.Parse(xmlNode.Attribute("ID").Value),
                ResID = xmlNode.Attribute("ResID").Value,
                Name = xmlNode.Attribute("Name").Value,
                TypeDesc = xmlNode.Attribute("TypeDesc").Value,
                Skills = new List<int>(),
                HP = int.Parse(xmlNode.Attribute("HP").Value),
                Str = int.Parse(xmlNode.Attribute("Str").Value),
                Dex = int.Parse(xmlNode.Attribute("Dex").Value),
                Sta = int.Parse(xmlNode.Attribute("Sta").Value),
                Int = int.Parse(xmlNode.Attribute("Int").Value),
                StrToPAtk = int.Parse(xmlNode.Attribute("StrToPAtk").Value),
                DexToHit = int.Parse(xmlNode.Attribute("DexToHit").Value),
                DexToDodge = int.Parse(xmlNode.Attribute("DexToDodge").Value),
                StaToHP = int.Parse(xmlNode.Attribute("StaToHP").Value),
                IntToMAtk = int.Parse(xmlNode.Attribute("IntToMAtk").Value),
                StrPerLevel = int.Parse(xmlNode.Attribute("StrPerLevel").Value),
                DexPerLevel = int.Parse(xmlNode.Attribute("DexPerLevel").Value),
                StaPerLevel = int.Parse(xmlNode.Attribute("StaPerLevel").Value),
                IntPerLevel = int.Parse(xmlNode.Attribute("IntPerLevel").Value),
            };

            /// Chuỗi danh sách kỹ năng ban đầu
            string initSkillsString = xmlNode.Attribute("Skills").Value;
            /// Nếu tồn tại
            if (!string.IsNullOrEmpty(initSkillsString))
            {
                /// Duyệt danh sách kỹ năng Pet
                foreach (string skillIDString in initSkillsString.Split(';'))
                {
                    /// ID kỹ năg
                    int skillID = int.Parse(skillIDString);
                    /// Thêm vào danh sách
                    petData.Skills.Add(skillID);
                }
            }

            /// Trả về kết quả
            return petData;
        }
    }
}
