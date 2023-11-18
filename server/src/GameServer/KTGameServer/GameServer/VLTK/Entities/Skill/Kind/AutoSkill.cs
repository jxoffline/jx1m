using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace GameServer.KiemThe.Entities
{
    /// <summary>
    /// Đối tượng kỹ năng tự động xuất dựa vào các điều kiện
    /// </summary>
    public class AutoSkill
    {
        /// <summary>
        /// Giá trị theo cấp độ
        /// </summary>
        public class ValueByLevel
        {
            /// <summary>
            /// Cấp độ
            /// </summary>
            public int Level { get; set; }

            /// <summary>
            /// Giá trị
            /// </summary>
            public int Value { get; set; }
        }

        /// <summary>
        /// Số lần cộng dồn dùng để kiểm tra điều kiện yêu cầu số lần thỏa mãn nhất định
        /// </summary>
        public int StackCount { get; set; } = 0;

        /// <summary>
        /// ID kỹ năng
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// Tên kỹ năng
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Loại kỹ năng
        /// <para>8: Kích hoạt khi khiên nội lực bị phá</para>
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// Danh sách tỷ lệ xuất theo cấp độ
        /// </summary>
        public Dictionary<int, ValueByLevel> CastPercentByLevel { get; set; }

        /// <summary>
        /// ID kỹ năng xuất kèm
        /// </summary>
        public int CastSkillID { get; set; }

        /// <summary>
        /// Cấp độ kỹ năng xuất kèm
        /// </summary>
        public Dictionary<int, ValueByLevel> CastSkillLevelByLevel { get; set; }

        /// <summary>
        /// ID kỹ năng cha
        /// </summary>
        public int ParentSkillID { get; set; }

        /// <summary>
        /// Thời gian giãn cách giữa các lần thi triển theo cấp độ
        /// </summary>
        public Dictionary<int, ValueByLevel> DelayPerCastByLevel { get; set; }

        /// <summary>
        /// Số chiêu xuất tối đa mỗi lần theo cấp
        /// </summary>
        public Dictionary<int, ValueByLevel> MaxCastCountByLevel { get; set; }

        /// <summary>
        /// Kích hoạt tự động khi mất trạng thái Buff ID tương ứng
        /// <para>-1: Vô hiệu</para>
        /// </summary>
        public int ActivateIfLostBuff { get; set; }

        /// <summary>
        /// Kích hoạt khi đánh trúng mục tiêu
        /// </summary>
        public bool ActivateIfHitTarget { get; set; }

        /// <summary>
        /// Kích hoạt khi % sinh lực giảm xuống dưới ngưỡng
        /// </summary>
        public int ActivateWhenHPPercentDropBelow { get; set; }

        /// <summary>
        /// Kích hoạt khi bị đánh trúng
        /// </summary>
        public bool ActivateWhenBeHit { get; set; }


        /// <summary>
        /// Kích hoạt khi bị đánh trúng chí mạng
        /// </summary>
        public bool ActivateWhenBeCritHit { get; set; }
        /// <summary>
        /// Kích hoạt khi đánh chí mạng
        /// </summary>
        public bool ActivateWhenDoCritHit { get; set; }

        /// <summary>
        /// Kích hoạt khi bị trọng thương
        /// </summary>
        public bool ActivateAfterDie { get; set; }

        /// <summary>
        /// Kích hoạt sau khi đánh đủ số đòn chí mạng lên mục tiêu
        /// </summary>
        public int ActivateAfterDoTotalCritHit { get; set; }

        /// <summary>
        /// Kích hoạt khi bị đánh trúng bởi các kỹ năng
        /// </summary>
        public List<int> ActivateWhenBeHitBySkills { get; set; }

        /// <summary>
        /// Kích hoạt khi đánh trúng mục tiêu bằng các kỹ năng
        /// </summary>
        public List<int> ActivateWhenHitWithSkills { get; set; }

        /// <summary>
        /// Kích hoạt nếu không dùng kỹ năng trong số Frame tương ứng
        /// </summary>
        public int ActivateIfNoUseSkillForFrame { get; set; }

        /// <summary>
        /// Kích hoạt nếu không dùng kỹ năng làm mất trạng thái tàng hình trong số Frame tương ứng
        /// </summary>
        public int ActivateIfNoUseSkillCauseLostInvisibilityForFrame { get; set; }

        /// <summary>
        /// Kích hoạt mỗi khoảng thời gian
        /// </summary>
        public int ActivateEachFrame { get; set; }

        /// <summary>
        /// Chuyển đối tượng từ XML Node
        /// </summary>
        /// <param name="xmlNode"></param>
        /// <returns></returns>
        public static AutoSkill Parse(XElement xmlNode)
        {
            AutoSkill autoSkill = new AutoSkill()
            {
                ID = int.Parse(xmlNode.Attribute("ID").Value),
                Type = int.Parse(xmlNode.Attribute("Type").Value),
                Name = xmlNode.Attribute("Name").Value,
                CastPercentByLevel = AutoSkill.AnalysisValueByLevel(xmlNode.Attribute("CastPercent").Value),
                CastSkillID = int.Parse(xmlNode.Attribute("CastSkillID").Value),
                CastSkillLevelByLevel = AutoSkill.AnalysisValueByLevel(xmlNode.Attribute("CastSkillLevel").Value),
                ParentSkillID = int.Parse(xmlNode.Attribute("ParentSkillID").Value),
                DelayPerCastByLevel = AutoSkill.AnalysisValueByLevel(xmlNode.Attribute("DelayPerCast").Value),
                MaxCastCountByLevel = AutoSkill.AnalysisValueByLevel(xmlNode.Attribute("MaxCastCount").Value),
                ActivateIfLostBuff = int.Parse(xmlNode.Attribute("ActivateIfLostBuff").Value),
                ActivateIfHitTarget = bool.Parse(xmlNode.Attribute("ActivateIfHitTarget").Value),
                ActivateWhenHPPercentDropBelow = int.Parse(xmlNode.Attribute("ActivateWhenHPPercentDropBelow").Value),
                ActivateWhenBeCritHit = bool.Parse(xmlNode.Attribute("ActivateWhenBeCritHit").Value),
                ActivateWhenBeHit = bool.Parse(xmlNode.Attribute("ActivateWhenBeHit").Value),
                ActivateWhenDoCritHit = bool.Parse(xmlNode.Attribute("ActivateWhenDoCritHit").Value),
                ActivateAfterDie = bool.Parse(xmlNode.Attribute("ActivateAfterDie").Value),
                ActivateAfterDoTotalCritHit = int.Parse(xmlNode.Attribute("ActivateAfterDoTotalCritHit").Value),
                ActivateWhenBeHitBySkills = new List<int>(),
                ActivateWhenHitWithSkills = new List<int>(),
                ActivateIfNoUseSkillForFrame = int.Parse(xmlNode.Attribute("ActivateIfNoUseSkillForFrame").Value),
                ActivateIfNoUseSkillCauseLostInvisibilityForFrame = int.Parse(xmlNode.Attribute("ActivateIfNoUseSkillCauseLostInvisibilityForFrame").Value),
                ActivateEachFrame = int.Parse(xmlNode.Attribute("ActivateEachFrame").Value),
            };
            string[] activateWhenByHitBySkills = xmlNode.Attribute("ActivateWhenBeHitBySkills").Value.Split(',');
            foreach (string str in activateWhenByHitBySkills)
            {
                int skillID = int.Parse(str);
                if (skillID != -1)
                {
                    autoSkill.ActivateWhenBeHitBySkills.Add(skillID);
                }
            }
            string[] activateWhenHitWithSkills = xmlNode.Attribute("ActivateWhenHitWithSkills").Value.Split(',');
            foreach (string str in activateWhenHitWithSkills)
            {
                int skillID = int.Parse(str);
                if (skillID != -1)
                {
                    autoSkill.ActivateWhenHitWithSkills.Add(skillID);
                }
            }

            return autoSkill;
        }

        /// <summary>
        /// Phân tích dữ liệu theo cấp độ 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private static Dictionary<int, ValueByLevel> AnalysisValueByLevel(string data)
        {
            Dictionary<int, ValueByLevel> array = new Dictionary<int, ValueByLevel>();
            string regex = @"\{(\d+),(\d+)\}";
            if (Regex.IsMatch(data, regex))
            {
                foreach (Match match in Regex.Matches(data, regex))
                {
                    int level = int.Parse(match.Groups[1].Value);
                    int value = int.Parse(match.Groups[2].Value);
                    array[level] = new ValueByLevel()
                    {
                        Level = level,
                        Value = value,
                    };
                }
                List<ValueByLevel> list = AutoSkill.MakeChildLevel(array.Values.ToList());
                foreach (ValueByLevel val in list)
                {
                    array[val.Level] = val;
                }
            }
            else
            {
                for (int i = 1; i <= SkillDataEx.SystemMaxLevel; i++)
                {
                    array[i] = new ValueByLevel()
                    {
                        Level = i,
                        Value = int.Parse(data),
                    };
                }
            }
            return array;
        }

        /// <summary>
        /// Tạo danh sách giá trị các cấp độ con
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        private static List<ValueByLevel> MakeChildLevel(List<ValueByLevel> array)
        {
            List<ValueByLevel> list = new List<ValueByLevel>();

            array.Sort((o1, o2) =>
            {
                return o1.Level - o2.Level;
            });

            list.Add(array[0]);

            float lastDelta = 0;
            for (int i = 1; i < array.Count; i++)
            {
                int dLevel = array[i].Level - array[i - 1].Level;
                int dValue = array[i].Value - array[i - 1].Value;
                float delta = dValue / dLevel;
                for (int j = array[i - 1].Level + 1; j < array[i].Level; j++)
                {
                    list.Add(new ValueByLevel()
                    {
                        Level = j,
                        Value = (int)(array[i - 1].Value + (j - array[i - 1].Level) * delta),
                    });
                }
                list.Add(array[i]);

                lastDelta = Math.Max(lastDelta, delta);
            }

            for (int j = array[array.Count - 1].Level + 1; j <= SkillDataEx.SystemMaxLevel; j++)
            {
                list.Add(new ValueByLevel()
                {
                    Level = j,
                    Value = (int)(array[array.Count - 1].Value + (j - array[array.Count - 1].Level) * lastDelta),
                });
            }

            return list;
        }
    }
}
