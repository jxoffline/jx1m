using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static FS.VLTK.Entities.Enum;

namespace FS.VLTK.Entities.Config
{
    /// <summary>
    /// Đối tượng quản lý môn phái
    /// </summary>
    public class FactionXML
    {
        /// <summary>
        /// ID phái
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// Tên phái
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Ngũ hành
        /// </summary>
        public Elemental Elemental { get; set; }

        /// <summary>
        /// Giới tính cho phép
        /// </summary>
        public Sex Gender { get; set; }

        /// <summary>
        /// Dữ liệu nhánh tu luyện
        /// </summary>
        public class Sub
        {
            /// <summary>
            /// ID nhánh
            /// </summary>
            public int ID { get; set; }
                
            /// <summary>
            /// Tên nhánh
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Danh sách ID kỹ năng nhánh
            /// </summary>
            public List<int> SkillIDs { get; set; }
        }

        /// <summary>
        /// Danh sách nhánh
        /// </summary>
        public Dictionary<int, Sub> Subs { get; set; }

        /// <summary>
        /// Chuyển đối tượng từ XML Node
        /// </summary>
        /// <param name="xmlNode"></param>
        /// <returns></returns>
        public static FactionXML Parse(XElement xmlNode)
        {
            FactionXML faction = new FactionXML()
            {
                ID = int.Parse(xmlNode.Attribute("ID").Value),
                Name = xmlNode.Attribute("Name").Value,
                Elemental = (Elemental) int.Parse(xmlNode.Attribute("Series").Value),
                Gender = (Sex) int.Parse(xmlNode.Attribute("Gender").Value),

                Subs = new Dictionary<int, Sub>(),
            };

            foreach (XElement subNode in xmlNode.Elements("Sub"))
            {
                Sub sub = new Sub()
                {
                    ID = int.Parse(subNode.Attribute("ID").Value),
                    Name = subNode.Attribute("Name").Value,
                    SkillIDs = new List<int>(),
                };
                faction.Subs[sub.ID] = sub;

                foreach (XElement skillNode in subNode.Elements("Skill"))
                {
                    sub.SkillIDs.Add(int.Parse(skillNode.Attribute("ID").Value));
                }
            }

            return faction;
        }
    }
}
