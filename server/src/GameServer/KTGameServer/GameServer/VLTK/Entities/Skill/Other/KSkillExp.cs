using GameServer.KiemThe;
using GameServer.KiemThe.Entities;
using GameServer.KiemThe.Logic;
using GameServer.Logic;
using Server.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace GameServer.Entities.Skill.Other
{
    /// <summary>
    /// Quản lý kinh nghiệm thăng cấp kỹ năng
    /// </summary>
    public static class KSkillExp
    {
        /// <summary>
        /// Thông tin kinh nghiệm thăng cấp kỹ năng
        /// </summary>
        private class SkillExp
        {
            /// <summary>
            /// Thông tin nhóm kỹ năng
            /// </summary>
            public class SkillExpGroup
            {
                /// <summary>
                /// Thông tin kinh nghiệm thăng cấp
                /// </summary>
                public class SkillExpValue
                {
                    /// <summary>
                    /// Lượng kinh nghiệm yêu cầu
                    /// </summary>
                    public int Require { get; set; }

                    /// <summary>
                    /// % kinh nghiệm nhận được khi giết quái
                    /// </summary>
                    public int MonsterExpP { get; set; }

                    /// <summary>
                    /// % kinh nghiệm nhận được khi giết Boss
                    /// </summary>
                    public int BossExpP { get; set; }

                    /// <summary>
                    /// Tự thăng cấp khi đầy thanh kinh nghiệm
                    /// </summary>
                    public bool AutoLevelUp { get; set; }

                    /// <summary>
                    /// Chuyển đối tượng từ XMLNode
                    /// </summary>
                    /// <param name="xmlNode"></param>
                    /// <returns></returns>
                    public static SkillExpValue Parse(XElement xmlNode)
                    {
                        return new SkillExpValue()
                        {
                            Require = int.Parse(xmlNode.Attribute("Require").Value),
                            MonsterExpP = int.Parse(xmlNode.Attribute("MonsterExpP").Value),
                            BossExpP = int.Parse(xmlNode.Attribute("BossExpP").Value),
                            AutoLevelUp = bool.Parse(xmlNode.Attribute("AutoLevelUp").Value),
                        };
                    }
                }

                /// <summary>
                /// Danh sách kỹ năng
                /// </summary>
                public HashSet<int> SkillIDs { get; set; }

                /// <summary>
                /// Danh sách kinh nghiệm
                /// </summary>
                public List<SkillExpValue> Exps { get; set; }

                /// <summary>
                /// Chuyển đối tượng từ XMLNode
                /// </summary>
                /// <param name="xmlNode"></param>
                /// <returns></returns>
                public static SkillExpGroup Parse(XElement xmlNode)
                {
                    /// Tạo mới
                    SkillExpGroup group = new SkillExpGroup()
                    {
                        SkillIDs = new HashSet<int>(),
                        Exps = new List<SkillExpValue>(),
                    };

                    /// Chuỗi lưu thông tin danh sách kỹ năng
                    string skillIDsString = xmlNode.Attribute("IDs").Value;
                    /// Duyệt danh sách kỹ năng
                    foreach (string skillIDString in skillIDsString.Split(';'))
                    {
                        /// ID kỹ năng
                        int skillID = int.Parse(skillIDString);
                        /// Thêm vào danh sách
                        group.SkillIDs.Add(skillID);
                    }

                    /// Duyệt danh sách kinh nghiệm
                    foreach (XElement node in xmlNode.Elements("Exp"))
                    {
                        /// Thông tin kinh nghiệm
                        SkillExpValue expValue = SkillExpValue.Parse(node);
                        /// Thêm vào danh sách
                        group.Exps.Add(expValue);
                    }

                    /// Trả về kết quả
                    return group;
                }
            }

            /// <summary>
            /// Lượng kinh nghiệm kỹ năng tối đa mỗi ngày có thể đạt được
            /// </summary>
            public int MaxExpEachDay { get; set; }

            /// <summary>
            /// Danh sách nhóm kinh nghiệm kỹ năng
            /// </summary>
            public List<SkillExpGroup> Groups { get; set; }

            /// <summary>
            /// Chuyển đối tượng từ XMLNode
            /// </summary>
            /// <param name="xmlNode"></param>
            /// <returns></returns>
            public static SkillExp Parse(XElement xmlNode)
            {
                /// Tạo mới
                SkillExp skillExp = new SkillExp()
                {
                    MaxExpEachDay = int.Parse(xmlNode.Attribute("MaxExpEachDay").Value),
                    Groups = new List<SkillExpGroup>(),
                };

                /// Duyệt danh sách nhóm kỹ năng
                foreach (XElement node in xmlNode.Elements("Skills"))
                {
                    skillExp.Groups.Add(SkillExpGroup.Parse(node));
                }

                /// Trả về kết quả
                return skillExp;
            }
        }

        /// <summary>
        /// Dữ liệu kinh nghiệm kỹ năng theo cấp
        /// </summary>
        private static SkillExp Data;

        /// <summary>
        /// Khởi tạo
        /// </summary>
        public static void Init()
        {
            XElement attribNode = KTGlobal.ReadXMLData("Config/KT_Skill/SkillExp.xml");
            KSkillExp.Data = SkillExp.Parse(attribNode);
        }

        /// <summary>
        /// Trả về thông tin kinh nghiệm của kỹ năng tương ứng cấp hiện tại
        /// </summary>
        /// <param name="skill"></param>
        /// <returns></returns>
        private static SkillExp.SkillExpGroup.SkillExpValue GetSkillExpInfo(SkillLevelRef skill)
        {
            /// Thông tin nhóm
            SkillExp.SkillExpGroup group = KSkillExp.Data.Groups.Where(x => x.SkillIDs.Contains(skill.SkillID)).FirstOrDefault();
            /// Nếu không tồn tại
            if (group == null)
            {
                /// Không có kết quả
                return null;
            }

            /// Nếu vượt phạm vi
            if (skill.AddedLevel <= 0 || skill.AddedLevel >= group.Exps.Count)
            {
                /// Không có kết quả
                return null;
            }
            /// Trả về kết quả
            return group.Exps[skill.AddedLevel - 1];
        }

        /// <summary>
        /// Trả về lượng kinh nghiệm cần để thăng cấp kỹ năng
        /// </summary>
        /// <param name="skill"></param>
        /// <returns></returns>
        public static int GetSkillLevelUpExp(SkillLevelRef skill)
        {
            /// Thông tin
            SkillExp.SkillExpGroup.SkillExpValue data = KSkillExp.GetSkillExpInfo(skill);
            /// Toác
            if (data == null)
            {
                /// Bỏ qua
                return 0;
            }

            /// Trả về kết quả
            return data.Require;
        }

        /// <summary>
        /// Thực hiện thêm kinh nghiệm kỹ năng khi đánh quái
        /// </summary>
        /// <param name="player"></param>
        /// <param name="monster"></param>
        /// <param name="expGet"></param>
        public static void ProcessSkillExpGain(KPlayer player, Monster monster, int expGet)
        {
            /// Lượng kinh nghiệm đã nhận trong ngày
            int nExpGetToday = player.GetValueOfDailyRecore((int) DailyRecord.DailySkillExpGet);
            /// Nếu đã vượt quá ngưỡng của ngày
            if (nExpGetToday >= KSkillExp.Data.MaxExpEachDay)
            {
                /// Bỏ qua
                return;
            }

            /// ID kỹ năng đang tu luyện
            int currentExpSkillID = player.GetValueOfForeverRecore(ForeverRecord.CurrentExpSkill);

            /// Duyệt danh sách kỹ năng
            foreach (SkillLevelRef skill in player.Skills.GetExpSkills())
            {
                /// Nếu chưa học
                if (skill.AddedLevel <= 0)
                {
                    /// Bỏ qua
                    continue;
                }
                /// Nếu không phải kỹ năng có kinh nghiệm
                else if (!skill.Data.IsExpSkill)
                {
                    /// Bỏ qua
                    continue;
                }
                /// Nếu không phải kỹ năng đang tu luyện
                else if (skill.SkillID != currentExpSkillID)
                {
                    /// Bỏ qua
                    continue;
                }

                /// Thông tin kinh nghiệm kỹ năng
                SkillExp.SkillExpGroup.SkillExpValue expValue = KSkillExp.GetSkillExpInfo(skill);
                /// Nếu không tồn tại
                if (expValue == null)
                {
                    /// Bỏ qua
                    continue;
                }

                /// Lượng kinh nghiệm tăng thêm theo loại quái
                int expP;
                switch (monster.MonsterType)
                {
                    case MonsterAIType.Normal:
                    case MonsterAIType.Static:
                    case MonsterAIType.Special_Normal:
                    {
                        expP = expValue.MonsterExpP;
                        break;
                    }
                    default:
                    {
                        expP = expValue.BossExpP;
                        break;
                    }
                }

                /// Kinh nghiệm sẽ thêm
                int nAddExp = expGet * expP / 100;
                /// Nếu quá ngưỡng
                if (nExpGetToday + nAddExp > KSkillExp.Data.MaxExpEachDay)
                {
                    /// Thiết lập về ngưỡng
                    nAddExp = KSkillExp.Data.MaxExpEachDay - nExpGetToday;
                }
                /// Toác
                if (nAddExp <= 0)
                {
                    /// Bỏ qua
                    continue;
                }
                /// Thêm kinh nghiệm kỹ năng tương ứng
                int nExpLeft = KSkillExp.DoAddSkillExp(player, skill, nAddExp, expValue);
                /// Tổng lượng kinh nghiệm đã cộng vào
                int nTotal = nAddExp - nExpLeft;

                /// Nếu có lượng kinh nghiệm thêm vào
                if (nTotal > 0)
                {
                    /// Tăng thêm lượng đã thêm trong ngày
                    nExpGetToday += nTotal;
                    /// Thoát luôn
                    break;
                }
            }
            /// Lưu lại lượng kinh nghiệm sách đã nhận trong ngày
            player.SetValueOfDailyRecore((int) DailyRecord.DailySkillExpGet, nExpGetToday);
        }

        /// <summary>
        /// Thực hiện thêm kinh nghiệm kỹ năng tương ứng
        /// </summary>
        /// <param name="player"></param>
        /// <param name="skill"></param>
        /// <param name="nExp"></param>
        /// <param name="expValue"></param>
        private static int DoAddSkillExp(KPlayer player, SkillLevelRef skill, int nExp, SkillExp.SkillExpGroup.SkillExpValue expValue)
        {
            if(skill==null || expValue==null)
            {
                return 0;
            }
            /// Lượng kinh nghiệm đã nhận trong ngày
            int nExpGetToday = player.GetValueOfDailyRecore((int) DailyRecord.DailySkillExpGet);

            /// Cấp hiện tại
            int currentLevel = skill.AddedLevel;
            /// Kinh nghiệm thêm vào
            int nAddExp = skill.Exp + nExp;
            /// Lượng kỹ năng còn lại
            int nExpLeft = nAddExp;
            /// Lượng kinh nghiệm ban đầu
            int nOriginExp = nAddExp;

            /// Lặp liên tục
            do
            {
                /// Nếu cấp độ vượt quá ngưỡng
                if (skill.AddedLevel >= skill.Data.MaxSkillLevel)
                {
                    /// Thoát
                    break;
                }

                /// Kinh nghiệm cấp hiện tại cần
                int thisLevelMaxExp = expValue.Require;

                /// Nếu kinh nghiệm vượt quá ngưỡng thăng cấp
                if (nAddExp >= thisLevelMaxExp)
                {
                    /// Giảm lượng kinh nghiệm còn dư
                    nAddExp -= thisLevelMaxExp;
                    /// Nếu tự thăng cấp
                    if (expValue.AutoLevelUp)
                    {
                        /// Thăng cấp kỹ năng
                        skill.AddedLevel++;
                        /// Cập nhật giá trị kinh nghiệm theo cấp mới
                        expValue = KSkillExp.GetSkillExpInfo(skill);
                    }
                    /// Nếu không tự thăng cấp
                    else
                    {
                        /// Cập nhật kinh nghiệm còn dư
                        nExpLeft = nAddExp;
                        /// Cập nhật lượng kinh nghiệm thêm vào
                        skill.Exp = thisLevelMaxExp;
                        /// Thoát
                        break;
                    }
                }
                else
                {
                    /// Cập nhật lượng kinh nghiệm thêm vào
                    skill.Exp = nAddExp;
                    /// Cập nhật lượng kinh nghiệm còn dư
                    nExpLeft = 0;
                    break;
                }
            }
            while (true);

            /// Lượng kinh nghiệm tu luyện còn lại
            int nExpLeftToday = KSkillExp.Data.MaxExpEachDay - nExpGetToday - (nExp - nExpLeft);

            /// Nếu cấp độ thay đổi
            if (currentLevel != skill.Level)
            {
                /// Thực thi sự kiện kỹ năng thăng cấp tự động
                player.Skills.OnSkillLevelUp(skill);
                /// Thông báo
                KTGlobal.SendDefaultChat(player, string.Format("Kỹ năng <color=yellow>[{0}]</color>, thăng lên cấp <color=yellow>{1}</color>.\nKinh nghiệm tu luyện còn lại <color=yellow>{2} điểm</color>.", skill.Data.Name, skill.AddedLevel, nExpLeftToday));
            }

            /// Nếu kinh nghiệm thêm vào
            if (nExpLeft < nOriginExp)
            {
                /// Thông báo
                KTGlobal.SendDefaultChat(player, string.Format("Kỹ năng <color=yellow>[{0}]</color>, kinh nghiệm tăng: <color=yellow>{1}/{2}</color>.\nKinh nghiệm tu luyện còn lại <color=yellow>{3} điểm</color>.", skill.Data.Name, skill.Exp, expValue.Require, nExpLeftToday));
            }

            /// Trả về lượng kinh nghiệm còn dư
            return nExpLeft;
        }

        /// <summary>
        /// Thêm kinh nghiệm kỹ năng tương ứng
        /// </summary>
        /// <param name="player"></param>
        /// <param name="skill"></param>
        /// <param name="nExp"></param>
        /// <param name="expValue"></param>
        public static int AddSkillExp(KPlayer player, SkillLevelRef skill, int nExp)
        {
            /// Thông tin kinh nghiệm kỹ năng
            SkillExp.SkillExpGroup.SkillExpValue expValue = KSkillExp.GetSkillExpInfo(skill);
            /// Nếu không tồn tại
            if (expValue == null)
            {
                /// Bỏ qua
                return 0;
            }
            /// Thực hiện
            return KSkillExp.DoAddSkillExp(player, skill, nExp, expValue);
        }
    }
}
